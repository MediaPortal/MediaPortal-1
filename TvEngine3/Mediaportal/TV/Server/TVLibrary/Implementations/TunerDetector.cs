#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Pbda;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Components;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;
using UPnP.Infrastructure;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.SSDP;

using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.SatIp;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// This class is responsible for detecting tuners.
  /// </summary>
  public class TunerDetector : IDisposable
  {
    // Used for detecting and communicating with UPnP devices.
    private CPData _upnpControlPointData = null;
    private UPnPNetworkTracker _upnpAgent = null;
    private UPnPControlPoint _upnpControlPoint = null;

    // Used for detecting and communicating with devices that are directly
    // connected to the system.
    private ManagementEventWatcher _systemDeviceChangeEventWatcher = null;
    private DateTime _previousSystemDeviceChange = DateTime.MinValue;

    // network providers
    private bool _isMicrosoftGenericNpAvailable = false;
    private IBaseFilter _atscNp = null;
    private IBaseFilter _dvbcNp = null;
    private IBaseFilter _dvbsNp = null;
    private IBaseFilter _dvbtNp = null;

    private IFilterGraph2 _graph = null;
    private DsROTEntry _rotEntry = null;

    private struct TunerInfo
    {
      public ITVCard Tuner;
      public string ProductInstanceId;
      public string TunerInstanceId;
      public bool IsFirstDetection;
    }

    private struct TunerGroupInfo
    {
      public CardGroup Group;
      public string ProductInstanceIdentifier;
      public string TunerInstanceIdentifier;
      public Dictionary<string, TunerInfo> Tuners;
    }

    private HashSet<string> _knownUpnpRootDevices = new HashSet<string>();
    private Dictionary<string, TunerInfo> _knownTunerInfo = new Dictionary<string, TunerInfo>();

    private Dictionary<string, Dictionary<string, Dictionary<string, TunerInfo>>> _detectedNaturalTunerGroups = new Dictionary<string, Dictionary<string, Dictionary<string, TunerInfo>>>();
    private Dictionary<int, TunerGroupInfo> _configuredTunerGroups = new Dictionary<int, TunerGroupInfo>();

    // The listener that we notify when tuners are detected or lost.
    private ITunerDetectionEventListener _eventListener = null;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="listener">A listener that wishes to be notified about tuner detection events.</param>
    public TunerDetector(ITunerDetectionEventListener listener)
    {
      _eventListener = listener;

      // Setup UPnP tuner detection.
      UPnPConfiguration.LOGGER = new Logger();
      _upnpControlPointData = new CPData();
      _upnpAgent = new UPnPNetworkTracker(_upnpControlPointData);
      _upnpAgent.RootDeviceAdded += OnUpnpRootDeviceAdded;
      _upnpAgent.RootDeviceRemoved += OnUpnpRootDeviceRemoved;
      _upnpControlPoint = new UPnPControlPoint(_upnpAgent);

      // Setup DirectShow tuner detection.
      try
      {
        InitDirectShowDetectionGraph();
      }
      catch (Exception ex)
      {
        this.LogCritical(ex, "detector: failed to initialise the DirectShow tuner detection graph");
        throw;
      }
      _systemDeviceChangeEventWatcher = new ManagementEventWatcher();
      // EventType 2 and 3 are device arrival and removal. See:
      // http://msdn.microsoft.com/en-us/library/windows/desktop/aa394124%28v=vs.85%29.aspx
      _systemDeviceChangeEventWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
      _systemDeviceChangeEventWatcher.EventArrived += OnSystemDeviceConnectedOrDisconnected;
    }

    public void Start()
    {
      this.LogInfo("detector: starting tuner detection...");
      // Start detecting tuners connected directly to the system.
      DetectSystemTuners();

      try
      {
        _systemDeviceChangeEventWatcher.Start();
      }
      catch
      {
        // Fails on Windows Media Center 2005 (ManagementException "unsupported", despite MS documentation).
        this.LogWarn("detector: failed to start device change event watcher, you'll have to restart TV Server to detect new tuners");
      }

      // Start detecting UPnP tuners.
      // IMPORTANT: this parameter must be set to allow devices with many sub-devices
      // and/or services to be detected. The timer interval specifies how long the
      // SSDP controller has from first detection of the root device SSDP packet
      // until descriptions for all devices and services have been requested, received
      // and processed. DRI tuners normally take about 5 seconds.
      SSDPClientController.EXPIRATION_TIMER_INTERVAL = 60000;
      // IMPORTANT: you should start the control point before the network tracker.
      _upnpControlPoint.Start();
      _upnpAgent.Start();
      _upnpAgent.SharedControlPointData.SSDPController.SearchDeviceByDeviceTypeVersion("schemas-opencable-com:service:Tuner", "1", null);
      _upnpAgent.SharedControlPointData.SSDPController.SearchDeviceByDeviceTypeVersion("urn:ses-com:device:SatIPServer", "1", null);
    }

    public void Reset()
    {
      Stop();
      _knownUpnpRootDevices.Clear();
      _knownTunerInfo.Clear();
      _detectedNaturalTunerGroups.Clear();
      _configuredTunerGroups.Clear();
      Start();
    }

    public void Stop()
    {
      this.LogInfo("detector: stopping tuner detection...");
      _upnpAgent.Close();
      _upnpControlPoint.Close();
      _systemDeviceChangeEventWatcher.Stop();
    }

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Stop();

      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      if (_graph != null)
      {
        _graph.RemoveFilter(_atscNp);
        _graph.RemoveFilter(_dvbcNp);
        _graph.RemoveFilter(_dvbsNp);
        _graph.RemoveFilter(_dvbtNp);
        Release.ComObject("tuner detector graph", ref _graph);
      }
      Release.ComObject("tuner detector ATSC network provider", ref _atscNp);
      Release.ComObject("tuner detector DVB-C network provider", ref _dvbcNp);
      Release.ComObject("tuner detector DVB-S network provider", ref _dvbsNp);
      Release.ComObject("tuner detector DVB-T network provider", ref _dvbtNp);
    }

    #endregion

    #region BDA/WDM device detection

    private static bool ConnectFilter(IFilterGraph2 graph, IBaseFilter networkFilter, IBaseFilter tunerFilter)
    {
      IPin pinOut = DsFindPin.ByDirection(networkFilter, PinDirection.Output, 0);
      IPin pinIn = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      try
      {
        int hr = graph.ConnectDirect(pinOut, pinIn, null);
        return (hr == (int)HResult.Severity.Success);
      }
      catch
      {
        return false;
      }
      finally
      {
        Release.ComObject("tuner detector filter output pin", ref pinOut);
        Release.ComObject("tuner detector filter input pin", ref pinIn);
      }
    }

    private void OnSystemDeviceConnectedOrDisconnected(object sender, EventArrivedEventArgs e)
    {
      // Often several events will be triggered within a very short period of
      // time when a device is added/removed. We only want to check for new
      // tuners once. Also, the first event may occur before the device is
      // ready, so we apply the device detection delay here.
      if ((DateTime.Now - _previousSystemDeviceChange).TotalMilliseconds < 10000)
      {
        return;
      }
      _previousSystemDeviceChange = DateTime.Now;
      int delayDetect = SettingsManagement.GetValue("delayCardDetect", 0);
      if (delayDetect >= 1)
      {
        Thread.Sleep(delayDetect * 1000);
      }
      DetectSystemTuners();
    }

    private void DetectSystemTuners()
    {
      lock (_knownTunerInfo)
      {
        this.LogDebug("Detecting system tuners...");

        HashSet<string> knownTuners = new HashSet<string>();

        // Detect TechniSat SkyStar/AirStar/CableStar 2 and stream source tuners.
        DetectSupportedLegacyAmFilters(ref knownTuners);

        // Detect analog tuners and multi-input capture devices.
        DetectSupportedAmKsCrossbarDevices(ref knownTuners);

        // Detect single-input capture devices.
        DetectSupportedAmKsCaptureDevices(ref knownTuners);

        // Detect BDA and PBDA tuners.
        DetectSupportedBdaSources(ref knownTuners);

        // Remove the tuners that are no longer available.
        List<TunerInfo> unavailableTuners = new List<TunerInfo>();
        foreach (TunerInfo info in _knownTunerInfo.Values)
        {
          if (!knownTuners.Contains(info.Tuner.DevicePath))
          {
            unavailableTuners.Add(info);
          }
        }
        foreach (TunerInfo info in unavailableTuners)
        {
          OnTunerRemoved(info);
        }
      }
    }

    private void InitDirectShowDetectionGraph()
    {
      this.LogDebug("detector: initialise DirectShow detection graph");
      _graph = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graph);

      ITuningSpace tuningSpace = null;
      ILocator locator = null;

      // The MediaPortal and Microsoft generic network providers must be added/removed to/from the
      // graph for each tuner that is checked. If you don't do this, the network types list gets
      // longer and longer and longer.
      // MS generic, MCE 2005 roll-up 2 or better
      _isMicrosoftGenericNpAvailable = FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID);

      try
      {
        // ATSC
        _atscNp = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, typeof(ATSCNetworkProvider).GUID, "ATSC Network Provider");
        tuningSpace = (ITuningSpace)new ATSCTuningSpace();
        tuningSpace.put_UniqueName("ATSC TuningSpace");
        tuningSpace.put_FriendlyName("ATSC TuningSpace");
        ((IATSCTuningSpace)tuningSpace).put_MaxChannel(10000);
        ((IATSCTuningSpace)tuningSpace).put_MaxMinorChannel(10000);
        ((IATSCTuningSpace)tuningSpace).put_MinChannel(0);
        ((IATSCTuningSpace)tuningSpace).put_MinMinorChannel(0);
        ((IATSCTuningSpace)tuningSpace).put_MinPhysicalChannel(0);
        ((IATSCTuningSpace)tuningSpace).put_InputType(TunerInputType.Antenna);
        locator = (IATSCLocator)new ATSCLocator();
        locator.put_CarrierFrequency(-1);
        locator.put_InnerFEC(FECMethod.MethodNotSet);
        locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        locator.put_Modulation(ModulationType.ModNotSet);
        locator.put_OuterFEC(FECMethod.MethodNotSet);
        locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        locator.put_SymbolRate(-1);
        locator.put_CarrierFrequency(-1);
        ((IATSCLocator)locator).put_PhysicalChannel(-1);
        ((IATSCLocator)locator).put_TSID(-1);
        tuningSpace.put_DefaultLocator(locator);
        ((ITuner)_atscNp).put_TuningSpace(tuningSpace);

        // DVB-C
        _dvbcNp = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, typeof(DVBCNetworkProvider).GUID, "DVB-C Network Provider");
        tuningSpace = (ITuningSpace)new DVBTuningSpace();
        tuningSpace.put_UniqueName("DVB-C TuningSpace");
        tuningSpace.put_FriendlyName("DVB-C TuningSpace");
        tuningSpace.put__NetworkType(typeof(DVBCNetworkProvider).GUID);
        ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Cable);
        locator = (ILocator)new DVBCLocator();
        locator.put_CarrierFrequency(-1);
        locator.put_InnerFEC(FECMethod.MethodNotSet);
        locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        locator.put_Modulation(ModulationType.ModNotSet);
        locator.put_OuterFEC(FECMethod.MethodNotSet);
        locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        locator.put_SymbolRate(-1);
        tuningSpace.put_DefaultLocator(locator);
        ((ITuner)_dvbcNp).put_TuningSpace(tuningSpace);

        // DVB-S
        _dvbsNp = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, typeof(DVBSNetworkProvider).GUID, "DVB-S Network Provider");
        tuningSpace = (ITuningSpace)new DVBSTuningSpace();
        tuningSpace.put_UniqueName("DVB-S TuningSpace");
        tuningSpace.put_FriendlyName("DVB-S TuningSpace");
        tuningSpace.put__NetworkType(typeof(DVBSNetworkProvider).GUID);
        ((IDVBSTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Satellite);
        locator = (ILocator)new DVBSLocator();
        locator.put_CarrierFrequency(-1);
        locator.put_InnerFEC(FECMethod.MethodNotSet);
        locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        locator.put_Modulation(ModulationType.ModNotSet);
        locator.put_OuterFEC(FECMethod.MethodNotSet);
        locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        locator.put_SymbolRate(-1);
        tuningSpace.put_DefaultLocator(locator);
        ((ITuner)_dvbsNp).put_TuningSpace(tuningSpace);

        // DVB-T
        _dvbtNp = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, typeof(DVBTNetworkProvider).GUID, "DVB-T Network Provider");
        tuningSpace = (ITuningSpace)new DVBTuningSpace();
        tuningSpace.put_UniqueName("DVB-T TuningSpace");
        tuningSpace.put_FriendlyName("DVB-T TuningSpace");
        tuningSpace.put__NetworkType(typeof(DVBTNetworkProvider).GUID);
        ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Terrestrial);
        locator = (ILocator)new DVBTLocator();
        locator.put_CarrierFrequency(-1);
        locator.put_InnerFEC(FECMethod.MethodNotSet);
        locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        locator.put_Modulation(ModulationType.ModNotSet);
        locator.put_OuterFEC(FECMethod.MethodNotSet);
        locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        locator.put_SymbolRate(-1);
        tuningSpace.put_DefaultLocator(locator);
        ((ITuner)_dvbtNp).put_TuningSpace(tuningSpace);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "detector: failed to add and configure specific Microsoft network provider(s), is BDA installed?");
      }
    }

    private void OnTunerDetected(TunerInfo info)
    {
      this.LogInfo("  add...");
      this.LogInfo("    name        = {0}", info.Tuner.Name);
      this.LogInfo("    external ID = {0}", info.Tuner.DevicePath);
      this.LogInfo("    type        = {0}", info.Tuner.CardType);
      this.LogInfo("    product ID  = {0}", info.ProductInstanceId ?? "[null]");
      this.LogInfo("    tuner ID    = {0}", info.TunerInstanceId ?? "[null]");
      _knownTunerInfo.Add(info.Tuner.DevicePath, info);

      // Detect the naturally related tuners.
      Dictionary<string, TunerInfo> relatedTuners = null;
      if (info.ProductInstanceId != null && info.TunerInstanceId != null)
      {
        Dictionary<string, Dictionary<string, TunerInfo>> productTuners;
        if (!_detectedNaturalTunerGroups.TryGetValue(info.ProductInstanceId, out productTuners))
        {
          productTuners = new Dictionary<string, Dictionary<string, TunerInfo>>();
          relatedTuners = new Dictionary<string, TunerInfo>();
          relatedTuners.Add(info.Tuner.DevicePath, info);
          productTuners.Add(info.TunerInstanceId, relatedTuners);
          _detectedNaturalTunerGroups.Add(info.ProductInstanceId, productTuners);
        }
        else
        {
          if (!productTuners.TryGetValue(info.TunerInstanceId, out relatedTuners))
          {
            relatedTuners = new Dictionary<string, TunerInfo>();
            productTuners.Add(info.TunerInstanceId, relatedTuners);
          }
          else if (relatedTuners.Count > 0)
          {
            this.LogInfo("    detected naturally related tuners...");
            foreach (TunerInfo i in relatedTuners.Values)
            {
              ITVCard t = i.Tuner;
              this.LogInfo("      name = {0}, type = {1}, external ID = {2}", t.Name, t.CardType, t.DevicePath);
            }
          }
          relatedTuners.Add(info.Tuner.DevicePath, info);
        }
      }

      Card tuner = CardManagement.GetCardByDevicePath(info.Tuner.DevicePath, CardIncludeRelationEnum.None);
      if (tuner == null)
      {
        // First ever detection. Create the tuner settings.
        this.LogInfo("    new tuner...");
        info.IsFirstDetection = true;
        tuner = new Card
        {
          TimeshiftingFolder = string.Empty,
          RecordingFolder = string.Empty,
          DevicePath = info.Tuner.DevicePath,
          Name = info.Tuner.Name,
          Priority = 1,
          GrabEPG = true,
          Enabled = true,
          PreloadCard = false,
          AlwaysSendDiseqcCommands = false,
          DiseqcCommandRepeatCount = 0,
          UseConditionalAccess = false,
          CamType = (int)CamType.Default,
          DecryptLimit = 0,
          MultiChannelDecryptMode = (int)MultiChannelDecryptMode.Disabled,
          NetProvider = (int)DbNetworkProvider.Generic,
          PidFilterMode = (int)PidFilterMode.Auto,
          IdleMode = (int)IdleMode.Stop
        };
        CardManagement.SaveCard(tuner);

        // If we have product/tuner instance information and detected the tuner is a member of a
        // natural group...
        if (info.ProductInstanceId != null && info.TunerInstanceId != null && relatedTuners != null && relatedTuners.Count > 1)
        {
          // Automatically add the tuner to an existing tuner group if the group product and tuner
          // instance information matches.
          bool foundGroup = false;
          foreach (TunerGroupInfo groupInfo in _configuredTunerGroups.Values)
          {
            if (groupInfo.ProductInstanceIdentifier == info.ProductInstanceId && groupInfo.TunerInstanceIdentifier == info.TunerInstanceId)
            {
              this.LogInfo("    detected existing tuner group with matching information, name = {0}...", groupInfo.Group.Name);
              groupInfo.Tuners.Add(info.Tuner.DevicePath, info);
              CardGroupMap map = new CardGroupMap();
              map.IdCard = tuner.IdCard;
              map.IdCardGroup = groupInfo.Group.IdCardGroup;
              CardManagement.SaveCardGroupMap(map);
              foundGroup = true;
              break;
            }
          }

          // ...or if that doesn't apply, automatically add the tuner to an existing tuner group if
          // the group contains all the naturally related tuners.
          bool foundTunerInGroup = false;
          if (!foundGroup)
          {
            foreach (TunerGroupInfo groupInfo in _configuredTunerGroups.Values)
            {
              if (groupInfo.ProductInstanceIdentifier == null && groupInfo.TunerInstanceIdentifier == null)
              {
                bool foundTuner = false;
                foreach (string t1 in relatedTuners.Keys)
                {
                  foundTuner = false;
                  if (!t1.Equals(info.Tuner.DevicePath))
                  {
                    foreach (string t2 in groupInfo.Tuners.Keys)
                    {
                      if (t1.Equals(t2))
                      {
                        foundTunerInGroup = true;
                        foundTuner = true;
                        break;
                      }
                    }
                    if (!foundTuner)
                    {
                      break;
                    }
                  }
                }
                if (foundTuner)
                {
                  this.LogInfo("    detected existing tuner group containing all related tuners, name = {0}...", groupInfo.Group.Name);
                  groupInfo.Tuners.Add(info.Tuner.DevicePath, info);
                  CardGroupMap map = new CardGroupMap();
                  map.IdCard = tuner.IdCard;
                  map.IdCardGroup = groupInfo.Group.IdCardGroup;
                  CardManagement.SaveCardGroupMap(map);
                  foundGroup = true;
                  break;
                }
              }
            }
          }

          // Otherwise, if all of the tuners in the natural group are new and not in any other
          // group then create a new group.
          if (!foundGroup && !foundTunerInGroup)
          {
            bool createNewGroup = true;
            foreach (TunerInfo i in relatedTuners.Values)
            {
              if (!i.IsFirstDetection)
              {
                createNewGroup = false;
                break;
              }
            }
            if (createNewGroup)
            {
              CardGroup group = new CardGroup();
              group.Name = string.Format("Product Instance {0} Tuner {1}", info.ProductInstanceId, info.TunerInstanceId);
              CardManagement.SaveCardGroup(group);
              TunerGroupInfo groupInfo = new TunerGroupInfo();
              groupInfo.Group = group;
              groupInfo.ProductInstanceIdentifier = info.ProductInstanceId;
              groupInfo.TunerInstanceIdentifier = info.TunerInstanceId;
              groupInfo.Tuners = new Dictionary<string, TunerInfo>();
              _configuredTunerGroups.Add(group.IdCardGroup, groupInfo);
              foreach (TunerInfo i in relatedTuners.Values)
              {
                CardGroupMap map = new CardGroupMap();
                map.IdCard = tuner.IdCard;
                map.IdCardGroup = group.IdCardGroup;
                CardManagement.SaveCardGroupMap(map);
                groupInfo.Tuners.Add(i.Tuner.DevicePath, info);
              }
              this.LogInfo("    creating new tuner group, name = {0}...", group.Name);
            }
          }
        }
      }
      else
      {
        // This tuner has been detected previously. Check if it is a member of an existing tuner
        // group.
        this.LogInfo("    existing tuner...");
        IList<CardGroup> groups = CardManagement.ListAllCardGroups();
        foreach (CardGroup group in groups)
        {
          TrackableCollection<CardGroupMap> groupMaps = group.CardGroupMaps;
          bool found = false;
          foreach (CardGroupMap map in groupMaps)
          {
            if (map.IdCard == tuner.IdCard)
            {
              TunerGroupInfo groupInfo;
              if (!_configuredTunerGroups.TryGetValue(group.IdCardGroup, out groupInfo))
              {
                groupInfo = new TunerGroupInfo();
                groupInfo.Group = group;
                groupInfo.ProductInstanceIdentifier = info.ProductInstanceId;
                groupInfo.TunerInstanceIdentifier = info.TunerInstanceId;
                groupInfo.Tuners = new Dictionary<string, TunerInfo>();
                _configuredTunerGroups.Add(group.IdCardGroup, groupInfo);
                this.LogInfo("    detected existing tuner group, name = {0}, product instance ID = {1}, tuner instance ID = {2}...", group.Name, groupInfo.ProductInstanceIdentifier ?? "[null]", groupInfo.TunerInstanceIdentifier ?? "[null]");
              }
              else
              {
                this.LogInfo("    detected existing tuner group, name = {0}, product instance ID = {1}, tuner instance ID = {2}...", group.Name, groupInfo.ProductInstanceIdentifier ?? "[null]", groupInfo.TunerInstanceIdentifier ?? "[null]");
                if (groupInfo.ProductInstanceIdentifier != null && groupInfo.TunerInstanceIdentifier != null &&
                  info.ProductInstanceId != null && info.TunerInstanceId != null &&
                  groupInfo.ProductInstanceIdentifier != info.ProductInstanceId && groupInfo.TunerInstanceIdentifier != info.TunerInstanceId)
                {
                  groupInfo.ProductInstanceIdentifier = null;
                  groupInfo.TunerInstanceIdentifier = null;
                }
              }
              if (groupInfo.Tuners.Count > 0)
              {
                foreach (TunerInfo i in groupInfo.Tuners.Values)
                {
                  this.LogInfo("      name = {0}, type = {1}, external ID = {2}", i.Tuner.Name, i.Tuner.CardType, i.Tuner.DevicePath);
                }
              }
              groupInfo.Tuners.Add(info.Tuner.DevicePath, info);
              found = true;
              break;
            }
          }

          // Tuners can't be a member of more than one group.
          if (found)
          {
            break;
          }
        }
      }

      _eventListener.OnTunerAdded(info.Tuner);
    }

    private void OnTunerRemoved(TunerInfo info)
    {
      this.LogInfo("  remove...");
      this.LogInfo("    name        = {0}", info.Tuner.Name);
      this.LogInfo("    external ID = {0}", info.Tuner.DevicePath);
      this.LogInfo("    type        = {0}", info.Tuner.CardType);
      this.LogInfo("    product ID  = {0}", info.ProductInstanceId ?? "[null]");
      this.LogInfo("    tuner ID    = {0}", info.TunerInstanceId ?? "[null]");
      _eventListener.OnTunerRemoved(info.Tuner);

      foreach (Dictionary<string, Dictionary<string, TunerInfo>> productTuners in _detectedNaturalTunerGroups.Values)
      {
        foreach (Dictionary<string, TunerInfo> tuners in productTuners.Values)
        {
          tuners.Remove(info.Tuner.DevicePath);
        }
      }

      foreach (TunerGroupInfo groupInfo in _configuredTunerGroups.Values)
      {
        groupInfo.Tuners.Remove(info.Tuner.DevicePath);
      }

      _knownTunerInfo.Remove(info.Tuner.DevicePath);
    }

    private void DetectSupportedLegacyAmFilters(ref HashSet<string> knownTuners)
    {
      this.LogDebug("detector: detect legacy AM filters");

      int streamTunerCount = SettingsManagement.GetValue("iptvCardCount", 1);

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      foreach (DsDevice device in devices)
      {
        try
        {
          string name = device.Name;
          if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(device.DevicePath))
          {
            continue;
          }

          if (name.Equals("B2C2 MPEG-2 Source"))
          {
            this.LogInfo("detector: detected B2C2 root device");
            IEnumerable<ITVCard> b2c2Tuners = TunerB2c2Base.DetectTuners();
            foreach (ITVCard tuner in b2c2Tuners)
            {
              knownTuners.Add(tuner.DevicePath);
              if (!_knownTunerInfo.ContainsKey(tuner.DevicePath))
              {
                TunerInfo info = new TunerInfo();
                info.Tuner = tuner;
                OnTunerDetected(info);
              }
              else
              {
                tuner.Dispose();
              }
            }
          }
          else if (name.Equals("Elecard NWSource-Plus"))
          {
            this.LogInfo("detector: detected Elecard stream source");
            for (int i = 0; i < streamTunerCount; i++)
            {
              ITVCard tuner = new TunerStreamElecard(i);
              knownTuners.Add(tuner.DevicePath);
              if (!_knownTunerInfo.ContainsKey(tuner.DevicePath))
              {
                TunerInfo info = new TunerInfo();
                info.Tuner = tuner;
                OnTunerDetected(info);
              }
              else
              {
                tuner.Dispose();
              }
            }
          }
        }
        finally
        {
          device.Dispose();
        }
      }

      if (File.Exists(PathManager.BuildAssemblyRelativePath("MPIPTVSource.ax")))
      {
        this.LogInfo("detector: detected MediaPortal stream source");
        for (int i = 0; i < streamTunerCount; i++)
        {
          ITVCard tuner = new TunerStream(i);
          knownTuners.Add(tuner.DevicePath);
          if (!_knownTunerInfo.ContainsKey(tuner.DevicePath))
          {
            TunerInfo info = new TunerInfo();
            info.Tuner = tuner;
            OnTunerDetected(info);
          }
          else
          {
            tuner.Dispose();
          }
        }
      }
    }

    private void DetectSupportedAmKsCrossbarDevices(ref HashSet<string> knownTuners)
    {
      this.LogDebug("detector: detect AM KS crossbar devices");
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        knownTuners.Add(devicePath);
        if (_knownTunerInfo.ContainsKey(devicePath))
        {
          device.Dispose();
          continue;
        }

        TunerInfo info = new TunerInfo();
        info.ProductInstanceId = device.ProductInstanceIdentifier;

        // As far as I know the tuner instance ID is only stored with the
        // tuner component moniker. So, we have to do some graph work.
        Crossbar crossbar = new Crossbar(device);
        try
        {
          crossbar.PerformLoading(_graph);
          int tunerInstanceId = -1;
          if (crossbar.PinIndexInputTunerVideo >= 0 || crossbar.PinIndexInputTunerAudio >= 0)
          {
            Tuner tuner = new Tuner();
            try
            {
              tuner.PerformLoading(_graph, info.ProductInstanceId, crossbar);
              tunerInstanceId = tuner.Device.TunerInstanceIdentifier;
            }
            finally
            {
              tuner.PerformUnloading(_graph);
            }
          }
          else
          {
            tunerInstanceId = device.TunerInstanceIdentifier;
          }
          if (tunerInstanceId >= 0)
          {
            info.TunerInstanceId = tunerInstanceId.ToString();
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "detector: caught exception while attempting to determine tuner instance ID for {0}", name);
        }
        finally
        {
          crossbar.PerformUnloading(_graph);
        }

        info.Tuner = new TunerAnalog(device, FilterCategory.AMKSCrossbar);
        OnTunerDetected(info);
      }
    }

    private void DetectSupportedAmKsCaptureDevices(ref HashSet<string> knownTuners)
    {
      this.LogDebug("detector: detect AM KS capture devices");
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        knownTuners.Add(devicePath);
        if (_knownTunerInfo.ContainsKey(devicePath))
        {
          device.Dispose();
          continue;
        }

        // We don't want to add duplicate entries for multi-input capture
        // devices (already detected via crossbar).
        string productInstanceId = device.ProductInstanceIdentifier;
        if (productInstanceId != null)
        {
          bool found = false;
          foreach (TunerInfo knownInfo in _knownTunerInfo.Values)
          {
            if (productInstanceId.Equals(knownInfo.ProductInstanceId))
            {
              found = true;
              break;
            }
          }
          if (found)
          {
            device.Dispose();
            continue;
          }
        }

        TunerInfo info = new TunerInfo();
        info.ProductInstanceId = productInstanceId;
        int tunerInstanceId = device.TunerInstanceIdentifier;
        if (tunerInstanceId >= 0)
        {
          info.TunerInstanceId = tunerInstanceId.ToString();
        }
        info.Tuner = new TunerAnalog(device, FilterCategory.AMKSCapture);
        OnTunerDetected(info);
      }
    }

    private void DetectSupportedBdaSources(ref HashSet<string> knownTuners)
    {
      this.LogDebug("detector: detect BDA sources");

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        knownTuners.Add(devicePath);
        if (_knownTunerInfo.ContainsKey(devicePath))
        {
          device.Dispose();
          continue;
        }

        IBaseFilter tunerFilter;
        try
        {
          tunerFilter = FilterGraphTools.AddFilterFromDevice(_graph, device);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "detector: failed to add BDA source filter to determine tuner type for {0} {1}", name, devicePath);
          device.Dispose();
          continue;
        }

        bool isAtsc = false;
        bool isCable = false;
        bool isSatellite = false;
        bool isTerrestrial = false;
        try
        {
          // We shouldn't need network providers to detect the tuner type.
          this.LogDebug("  check type with topology node descriptors");
          DetectBdaSourceTypeInternal(tunerFilter, out isAtsc, out isCable, out isSatellite, out isTerrestrial);

          // If we couldn't detect the type with the topology info, try network
          // providers starting with the MediaPortal network provider.
          if (!isAtsc && !isCable && !isSatellite && !isTerrestrial)
          {
            this.LogDebug("  check type with MediaPortal network provider");
            DetectBdaSourceTypeMediaPortalNetworkProvider(tunerFilter, devicePath, out isAtsc, out isCable, out isSatellite, out isTerrestrial);
          }
          // Try the Microsoft network provider next if the MediaPortal network provider failed and
          // the MS generic NP is available.
          if (!isAtsc && !isCable && !isSatellite && !isTerrestrial && _isMicrosoftGenericNpAvailable)
          {
            this.LogDebug("  check type with Microsoft generic network provider");
            DetectBdaSourceTypeMicrosoftGenericNetworkProvider(tunerFilter, out isAtsc, out isCable, out isSatellite, out isTerrestrial);
          }
          // Last shot is the old style Microsoft network providers.
          if (!isAtsc && !isCable && !isSatellite && !isTerrestrial)
          {
            this.LogDebug("  check type with specific Microsoft network providers");
            DetectBdaSourceTypeMicrosoftSpecificNetworkProvider(tunerFilter, out isAtsc, out isCable, out isSatellite, out isTerrestrial);
          }
          if (!isAtsc && !isCable && !isSatellite && !isTerrestrial)
          {
            this.LogError("detector: failed to determine tuner type for {0} {1}", name, devicePath);
            device.Dispose();
            continue;
          }

          List<ITVCard> tuners = new List<ITVCard>();
          if (isAtsc)
          {
            if (tunerFilter is IBDA_ConditionalAccess)
            {
              tuners.Add(new TunerPbdaCableCard(device));
            }
            else
            {
              tuners.Add(new TunerBdaAtsc(device));
            }
          }
          if (isCable)
          {
            tuners.Add(new TunerBdaCable(device));
          }
          if (isSatellite)
          {
            tuners.Add(new TunerBdaSatellite(device));
          }
          if (isTerrestrial)
          {
            tuners.Add(new TunerBdaTerrestrial(device));
          }
          string productInstanceId = device.ProductInstanceIdentifier;
          int tunerInstanceId = device.TunerInstanceIdentifier;
          foreach (ITVCard t in tuners)
          {
            if (_knownTunerInfo.ContainsKey(t.DevicePath))
            {
              knownTuners.Add(t.DevicePath);
              t.Dispose();
            }
            else
            {
              TunerInfo info = new TunerInfo();
              info.Tuner = t;
              info.ProductInstanceId = productInstanceId;
              if (tunerInstanceId >= 0)
              {
                info.TunerInstanceId = tunerInstanceId.ToString();
              }
              OnTunerDetected(info);
            }
          }
        }
        finally
        {
          _graph.RemoveFilter(tunerFilter);
          Release.ComObject("tuner detector BDA source filter", ref tunerFilter);
        }
      }
    }

    private void DetectBdaSourceTypeInternal(IBaseFilter filter, out bool isAtsc, out bool isCable, out bool isSatellite, out bool isTerrestrial)
    {
      isAtsc = false;
      isCable = false;
      isSatellite = false;
      isTerrestrial = false;
      IBDA_Topology topology = filter as IBDA_Topology;
      if (topology == null)
      {
        this.LogDebug("  filter is not a BDA topology");
        return;
      }
      BDANodeDescriptor[] descriptors = new BDANodeDescriptor[21];
      int descriptorCount = 0;
      int hr = topology.GetNodeDescriptors(out descriptorCount, 20, descriptors);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogDebug("  failed to get node descriptors, hr = 0x{0:x}", hr);
        return;
      }
      this.LogDebug("  descriptor count = {0}", descriptorCount);
      for (int d = 0; d < descriptorCount; d++)
      {
        Guid function = descriptors[d].guidFunction;
        this.LogDebug("  function {0} = {1}", d, function);
        if (function == BDANodeCategory.EightVSBDemodulator)
        {
          isAtsc = true;
        }
        else if (function == BDANodeCategory.QAMDemodulator)
        {
          if (filter is IBDA_ConditionalAccess)
          {
            isAtsc = true;
          }
          else
          {
            isCable = true;
          }
        }
        else if (function == BDANodeCategory.COFDMDemodulator ||
          function == MediaPortalGuid.KS_NODE_BDA_ISDB_T_DEMODULATOR)
        {
          isTerrestrial = true;
        }
        else if (function == BDANodeCategory.QPSKDemodulator ||
          function == MediaPortalGuid.KS_NODE_BDA_8PSK_DEMODULATOR ||
          function == MediaPortalGuid.KS_NODE_BDA_ISDB_S_DEMODULATOR)
        {
          isSatellite = true;
        }
      }

      if (isCable && isAtsc)
      {
        isCable = false;
      }
      else if (!isAtsc && !isCable && !isSatellite && !isTerrestrial)
      {
        this.LogDebug("  traversed topology but type not recognised");
      }
    }

    private void DetectBdaSourceTypeMediaPortalNetworkProvider(IBaseFilter filter, string devicePath, out bool isAtsc, out bool isCable, out bool isSatellite, out bool isTerrestrial)
    {
      isAtsc = false;
      isCable = false;
      isSatellite = false;
      isTerrestrial = false;
      IBaseFilter mpNp = null;
      try
      {
        mpNp = FilterGraphTools.AddFilterFromFile(_graph, "NetworkProvider.ax", typeof(MediaPortalNetworkProvider).GUID, "MediaPortal Network Provider");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "  failed to add MediaPortal network provider to detection graph");
        return;
      }

      try
      {
        IDvbNetworkProvider interfaceNetworkProvider = mpNp as IDvbNetworkProvider;
        string hash = MediaPortalNetworkProvider.GetHash(devicePath);
        interfaceNetworkProvider.ConfigureLogging(MediaPortalNetworkProvider.GetFileName(devicePath), hash, LogLevelOption.Debug);
        if (!ConnectFilter(_graph, mpNp, filter))
        {
          this.LogDebug("  failed to connect to MediaPortal network provider");
          return;
        }

        TuningType tuningTypes;
        int hr = interfaceNetworkProvider.GetAvailableTuningTypes(out tuningTypes);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("  connected to MediaPortal network provider but failed to get available tuning types, hr = 0x{0:x}", hr);
          return;
        }

        this.LogDebug("  tuning types = {0}, hash = {1}", tuningTypes, hash);
        if (tuningTypes.HasFlag(TuningType.Atsc))
        {
          isAtsc = true;
        }
        if (tuningTypes.HasFlag(TuningType.DvbC))
        {
          if (filter is IBDA_ConditionalAccess)
          {
            isAtsc = true;
          }
          else
          {
            isCable = true;
          }
        }
        if (tuningTypes.HasFlag(TuningType.DvbT) || tuningTypes.HasFlag(TuningType.IsdbT))
        {
          isTerrestrial = true;
        }
        if (tuningTypes.HasFlag(TuningType.DvbS) || tuningTypes.HasFlag(TuningType.DvbS2) || tuningTypes.HasFlag(TuningType.IsdbS))
        {
          isSatellite = true;
        }

        if (!isAtsc && !isCable && !isSatellite && !isTerrestrial)
        {
          this.LogDebug("  connected to MediaPortal network provider but type not recognised");
        }
      }
      finally
      {
        _graph.RemoveFilter(mpNp);
        Release.ComObject("tuner detector MediaPortal network provider", ref mpNp);
      }
    }

    private void DetectBdaSourceTypeMicrosoftGenericNetworkProvider(IBaseFilter filter, out bool isAtsc, out bool isCable, out bool isSatellite, out bool isTerrestrial)
    {
      isAtsc = false;
      isCable = false;
      isSatellite = false;
      isTerrestrial = false;
      IBaseFilter genericNp = null;
      try
      {
        genericNp = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, typeof(NetworkProvider).GUID, "Microsoft Network Provider");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "  failed to add Microsoft generic network provider to detection graph");
        return;
      }

      try
      {
        if (!ConnectFilter(_graph, genericNp, filter))
        {
          this.LogDebug("  failed to connect to Microsoft generic network provider");
          return;
        }

        int networkTypesMax = 10;
        int networkTypeCount;
        Guid[] networkTypes = new Guid[networkTypesMax];
        int hr = (genericNp as ITunerCap).get_SupportedNetworkTypes(networkTypesMax, out networkTypeCount, networkTypes);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("  connected to Microsoft generic network provider but failed to get supported network types, hr = 0x{0:x}", hr);
          return;
        }

        this.LogDebug("  network type count = {0}", networkTypeCount);
        bool isLocalDigitalCableSupported = false;
        for (int n = 0; n < networkTypeCount; n++)
        {
          Guid networkType = networkTypes[n];
          this.LogDebug("  network type {0} = {1}", n, networkType);
          if (networkType == NetworkType.DIGITAL_CABLE)
          {
            // BDA hybrid ATSC/QAM, BDA DVB-C and PBDA CableCARD tuners all advertise this network
            // type. ATSC and DVB-C tuners usually advertise additional types which allow us to
            // identifiy them. If we don't see those types, we assume this is a QAM or CableCARD
            // tuner.
            isLocalDigitalCableSupported = true;
          }
          else if (networkType == NetworkType.ATSC_TERRESTRIAL)
          {
            isAtsc = true;
          }
          else if (networkType == NetworkType.DVB_CABLE ||
            networkType == NetworkType.ISDB_CABLE)
          {
            isCable = true;
          }
          else if (networkType == NetworkType.DVB_TERRESTRIAL ||
            networkType == NetworkType.ISDB_TERRESTRIAL ||
            networkType == NetworkType.ISDB_T)
          {
            isTerrestrial = true;
          }
          else if (networkType == NetworkType.DVB_SATELLITE ||
            networkType == NetworkType.DIRECTV_SATELLITE ||
            networkType == NetworkType.ECHOSTAR_SATELLITE ||
            networkType == NetworkType.ISDB_SATELLITE ||
            networkType == NetworkType.ISDB_S)
          {
            isSatellite = true;
          }
        }

        if (isLocalDigitalCableSupported && !isAtsc && !isCable)
        {
          isAtsc = true;
        }
        else if (!isAtsc && !isCable && !isSatellite && !isTerrestrial)
        {
          this.LogDebug("  connected to Microsoft generic network provider but type not recognised");
        }
      }
      finally
      {
        _graph.RemoveFilter(genericNp);
        Release.ComObject("tuner detector Microsoft generic network provider", ref genericNp);
      }
    }

    private void DetectBdaSourceTypeMicrosoftSpecificNetworkProvider(IBaseFilter filter, out bool isAtsc, out bool isCable, out bool isSatellite, out bool isTerrestrial)
    {
      isAtsc = false;
      isCable = false;
      isSatellite = false;
      isTerrestrial = false;
      if (ConnectFilter(_graph, _dvbtNp, filter))
      {
        isTerrestrial = true;
      }
      else if (ConnectFilter(_graph, _dvbcNp, filter))
      {
        // PBDA CableCARD tuners connect with the DVB-C network provider because they advertise the
        // local cable tuning type.
        if (filter is IBDA_ConditionalAccess)
        {
          isAtsc = true;
        }
        else
        {
          isCable = true;
        }
      }
      else if (ConnectFilter(_graph, _dvbsNp, filter))
      {
        isSatellite = true;
      }
      else if (ConnectFilter(_graph, _atscNp, filter))
      {
        isAtsc = true;
      }

      if (!isAtsc && !isCable && !isSatellite && !isTerrestrial)
      {
        this.LogDebug("  failed to connect to specific Microsoft network provider");
      }
    }

    #endregion

    #region UPnP device detection

    private void OnUpnpRootDeviceAdded(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor == null || rootDescriptor.State != RootDescriptorState.Ready)
      {
        return;
      }
      lock (_knownTunerInfo)
      {
        if (_knownUpnpRootDevices.Contains(rootDescriptor.SSDPRootEntry.RootDeviceUUID))
        {
          this.LogWarn("detector: re-detecting known root device {0}", rootDescriptor.SSDPRootEntry.RootDeviceUUID);
        }
        else
        {
          _knownUpnpRootDevices.Add(rootDescriptor.SSDPRootEntry.RootDeviceUUID);
        }

        DeviceDescriptor rootDeviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
        bool isSatIpTuner = IsSatIpTuner(rootDeviceDescriptor);
        bool isOcurTuner = false;
        if (isSatIpTuner)
        {
          this.LogInfo("detector: SAT>IP tuner added");
        }
        else
        {
          isOcurTuner = IsOcurTuner(rootDeviceDescriptor);
          if (!isOcurTuner)
          {
            return;
          }
          this.LogInfo("detector: OCUR/DRI tuner added");
        }

        LogUpnpDeviceInfo(rootDeviceDescriptor);
        if (isSatIpTuner)
        {
          DetectSatIpTunerArrival(rootDeviceDescriptor);
        }
        else
        {
          DetectOcurTunerArrival(rootDeviceDescriptor);
        }
      }
    }

    private void OnUpnpRootDeviceRemoved(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor == null)
      {
        return;
      }

      lock (_knownTunerInfo)
      {
        if (!_knownUpnpRootDevices.Remove(rootDescriptor.SSDPRootEntry.RootDeviceUUID))
        {
          this.LogWarn("detector: detecting removal of unknown root device {0}", rootDescriptor.SSDPRootEntry.RootDeviceUUID);
        }

        DeviceDescriptor rootDeviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
        bool isSatIpTuner = IsSatIpTuner(rootDeviceDescriptor);
        bool isOcurTuner = false;
        if (isSatIpTuner)
        {
          this.LogInfo("detector: SAT>IP tuner removed");
        }
        else
        {
          isOcurTuner = IsOcurTuner(rootDeviceDescriptor);
          if (!isOcurTuner)
          {
            return;
          }
          this.LogInfo("detector: OCUR/DRI tuner removed");
        }

        LogUpnpDeviceInfo(rootDeviceDescriptor);
        if (isSatIpTuner)
        {
          DetectSatIpTunerRemoval(rootDeviceDescriptor);
        }
        else
        {
          DetectOcurTunerRemoval(rootDeviceDescriptor);
        }
      }
    }

    private void LogUpnpDeviceInfo(DeviceDescriptor rootDeviceDescriptor)
    {
      this.LogInfo("  friendly name = {0}", rootDeviceDescriptor.FriendlyName);
      this.LogInfo("  UUID          = {0}", rootDeviceDescriptor.DeviceUUID);

      XmlNamespaceManager nm = new XmlNamespaceManager(rootDeviceDescriptor.DeviceNavigator.NameTable);
      nm.AddNamespace("d", UPnPConsts.NS_DEVICE_DESCRIPTION);
      XPathNavigator nav = rootDeviceDescriptor.DeviceNavigator;
      this.LogDebug("  manufacturer");
      this.LogDebug("    name        = {0}", nav.SelectSingleNode("d:manufacturer/text()", nm).Value);
      XPathNodeIterator it = nav.Select("d:manufacturerURL/text()", nm);
      if (it.MoveNext())
      {
        this.LogDebug("    URL         = {0}", it.Current.Value);
      }

      this.LogDebug("  model");
      string[] propertyDescriptions = new string[] { "name", "number", "description", "URL" };
      string[] propertyNames = new string[] { "modelName", "modelNumber", "modelDescription", "modelURL" };
      for (int i = 0; i < propertyDescriptions.Length; i++)
      {
        it = nav.Select("d:" + propertyNames[i] + "/text()", nm);
        if (it.MoveNext())
        {
          this.LogDebug("    {0, -11} = {1}", propertyDescriptions[i], it.Current.Value);
        }
      }

      it = nav.Select("d:serialNumber/text()", nm);
      if (it.MoveNext())
      {
        this.LogDebug("  serial number = {0}", it.Current.Value);
      }
      it = nav.Select("d:UPC/text()", nm);
      if (it.MoveNext())
      {
        this.LogDebug("  UPC           = {0}", it.Current.Value);
      }
    }

    #region SAT>IP tuner detection

    private bool IsSatIpTuner(DeviceDescriptor rootDeviceDescriptor)
    {
      return rootDeviceDescriptor.TypeVersion_URN.Equals("urn:ses-com:device:SatIPServer:1");
    }

    private void DetectSatIpTunerArrival(DeviceDescriptor rootDeviceDescriptor)
    {
      int satelliteFrontEndCount = 0;
      int terrestrialFrontEndCount = 0;
      string host = new Uri(rootDeviceDescriptor.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation).Host;

      // SAT>IP servers may have more than one tuner, but their descriptors
      // only ever have one device node. We have to find out how many tuners
      // are available and what type they are.
      XmlNamespaceManager nm = new XmlNamespaceManager(rootDeviceDescriptor.DeviceNavigator.NameTable);
      nm.AddNamespace("s", "urn:ses-com:satip");
      XPathNodeIterator it = rootDeviceDescriptor.DeviceNavigator.Select("s:X_SATIPCAP/text()");
      if (it.MoveNext())
      {
        // The easiest way to get the information we need is from X_SATIPCAP,
        // but unfortunately that element is optional.
        this.LogDebug("detector: get capabilites from X_SATIPCAP");
        string[] sections = it.Current.Value.Split(',');
        foreach (string section in sections)
        {
          Match m = Regex.Match(section, @"^([^-]+)-(\d+)$", RegexOptions.IgnoreCase);
          if (m.Success)
          {
            string msys = m.Groups[1].Captures[0].Value;
            if (msys.Equals("DVBS2"))
            {
              satelliteFrontEndCount += int.Parse(m.Groups[2].Captures[0].Value);
            }
            else if (msys.Equals("DVBT") || msys.Equals("DVBT2"))
            {
              terrestrialFrontEndCount += int.Parse(m.Groups[2].Captures[0].Value);
            }
            else
            {
              this.LogWarn("detector: unsupported SAT>IP msys {0} found in SAT>IP X_SATIPCAP {1}, section {2}", msys, it.Current.Value, section);
            }
          }
          else
          {
            this.LogWarn("detector: failed to interpret SAT>IP X_SATIPCAP {0}, section {1}", it.Current.Value, section);
          }
        }
      }
      else
      {
        // X_SATIPCAP is not available. Try an RTSP DESCRIBE.
        this.LogDebug("detector: attempt to get capabilities using RTSP DESCRIBE");
        bool requestSucceeded = true;
        StringBuilder rtspDescribeResponse = new StringBuilder();
        try
        {
          StringBuilder rtspDescribeRequest = new StringBuilder();
          rtspDescribeRequest.Append("DESCRIBE rtsp://").Append(host).Append(":554/ RTSP/1.0\r\n");
          rtspDescribeRequest.Append("CSeq: 1\r\n");
          rtspDescribeRequest.Append("Accept: application/sdp\r\n");
          rtspDescribeRequest.Append("Connection:close\r\n\r\n");
          TcpClient client = new TcpClient(host, 554);
          NetworkStream stream = client.GetStream();
          byte[] requestBytes = System.Text.Encoding.ASCII.GetBytes(rtspDescribeRequest.ToString());
          stream.Write(requestBytes, 0, requestBytes.Length);
          byte[] responseBytes = new byte[client.ReceiveBufferSize];
          while (stream.DataAvailable)
          {
            int byteCount = stream.Read(responseBytes, 0, responseBytes.Length);
            rtspDescribeResponse.Append(System.Text.Encoding.ASCII.GetString(responseBytes, 0, byteCount));
          }
          stream.Close();
          client.Close();
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "detector: SAT>IP RTSP DESCRIBE request failed");
          requestSucceeded = false;
        }
        if (requestSucceeded)
        {
          Match m = Regex.Match(rtspDescribeResponse.ToString(), @"s=SatIPServer:1\s+([^\s]+)\s+", RegexOptions.Singleline | RegexOptions.IgnoreCase);
          if (m.Success)
          {
            string frontEndInfo = m.Groups[1].Captures[0].Value;
            try
            {
              string[] frontEndCounts = frontEndInfo.Split(',');
              if (frontEndCounts.Length >= 2)
              {
                satelliteFrontEndCount = int.Parse(frontEndCounts[0]);
                terrestrialFrontEndCount = int.Parse(frontEndCounts[1]);
                if (frontEndCounts.Length > 2)
                {
                  this.LogWarn("detector: SAT>IP RTSP DESCRIBE response contains more than 2 front end counts, not supported");
                }
              }
              else
              {
                satelliteFrontEndCount = int.Parse(frontEndCounts[0]);
              }
            }
            catch (Exception ex)
            {
              this.LogWarn(ex, "detector: failed to interpret SAT>IP RTSP DESCRIBE response SatIPServer section {0}", frontEndInfo);
            }
          }
          else
          {
            this.LogWarn("detector: failed to interpret SAT>IP RTSP DESCRIBE response");
          }
        }
      }

      if (satelliteFrontEndCount == 0 && terrestrialFrontEndCount == 0)
      {
        this.LogWarn("detector: failed to gather SAT>IP front end information, assuming 2 satellite front ends");
        satelliteFrontEndCount = 2;
      }

      this.LogInfo("  sat FE count  = {0}", satelliteFrontEndCount);
      this.LogInfo("  terr FE count = {0}", terrestrialFrontEndCount);

      for (int i = 1; i <= satelliteFrontEndCount; i++)
      {
        TunerInfo info = new TunerInfo();
        info.Tuner = new TunerSatIpSatellite(rootDeviceDescriptor.FriendlyName, rootDeviceDescriptor.DeviceUUID, host, i);
        info.ProductInstanceId = null;
        info.TunerInstanceId = null;
        OnTunerDetected(info);
      }
      for (int i = 1; i <= terrestrialFrontEndCount; i++)
      {
        // TODO add DVB-C tuner support
        TunerInfo info = new TunerInfo();
        info.Tuner = new TunerSatIpTerrestrial(rootDeviceDescriptor.FriendlyName, rootDeviceDescriptor.DeviceUUID, host, i);
        info.ProductInstanceId = null;
        info.TunerInstanceId = null;
        OnTunerDetected(info);
      }
    }

    private void DetectSatIpTunerRemoval(DeviceDescriptor rootDeviceDescriptor)
    {
      // Find the devices that we're going to remove.
      List<string> keys = new List<string>();
      foreach (string key in _knownTunerInfo.Keys)
      {
        if (key.StartsWith(rootDeviceDescriptor.DeviceUUID))
        {
          keys.Add(key);
        }
      }
      foreach (string key in keys)
      {
        TunerInfo info;
        if (_knownTunerInfo.TryGetValue(key, out info))
        {
          OnTunerRemoved(info);
        }
      }
    }

    #endregion

    #region OCUR/DRI (CableCARD) tuner detection

    private bool IsOcurTuner(DeviceDescriptor rootDeviceDescriptor)
    {
      IEnumerator<DeviceEntry> childDeviceEn = rootDeviceDescriptor.RootDescriptor.SSDPRootEntry.Devices.Values.GetEnumerator();
      while (childDeviceEn.MoveNext())
      {
        foreach (string serviceUrn in childDeviceEn.Current.Services)
        {
          if (serviceUrn.Equals("urn:schemas-opencable-com:service:Tuner:1"))
          {
            return true;
          }
        }
      }
      return false;
    }

    private void DetectOcurTunerArrival(DeviceDescriptor rootDeviceDescriptor)
    {
      IEnumerator<DeviceEntry> childDeviceEn = rootDeviceDescriptor.RootDescriptor.SSDPRootEntry.Devices.Values.GetEnumerator();
      while (childDeviceEn.MoveNext())
      {
        // Have to check for the tuner service to avoid Ceton tuning adaptor devices.
        foreach (string serviceUrn in childDeviceEn.Current.Services)
        {
          if (serviceUrn.Equals("urn:schemas-opencable-com:service:Tuner:1"))
          {
            TunerInfo info;
            if (_knownTunerInfo.TryGetValue(childDeviceEn.Current.UUID, out info))
            {
              OnTunerRemoved(info);
            }

            info = new TunerInfo();
            info.Tuner = new TunerDri(rootDeviceDescriptor.FindDevice(childDeviceEn.Current.UUID), _upnpControlPoint);
            if (info.Tuner.Name.Contains("Ceton"))
            {
              // Example: Ceton InfiniTV PCIe (00-80-75-05) Tuner 1 (00-00-22-00-00-80-75-05)
              Match m = Regex.Match(info.Tuner.Name, @"\s+\(([^\s]+)\)\s+Tuner\s+(\d+)", RegexOptions.IgnoreCase);
              if (m.Success)
              {
                info.ProductInstanceId = m.Groups[1].Captures[0].Value;
                info.TunerInstanceId = m.Groups[2].Captures[0].Value;
              }
            }
            else
            {
              // Examples: HDHomeRun Prime Tuner 1316890F-1, Hauppauge OpenCable Receiver 201200AA-1
              Match m = Regex.Match(info.Tuner.Name, @"\s+([^\s]+)-(\d)$", RegexOptions.IgnoreCase);
              if (m.Success)
              {
                info.ProductInstanceId = m.Groups[1].Captures[0].Value;
                info.TunerInstanceId = m.Groups[2].Captures[0].Value;
              }
            }
            // To handle any other future tuners...
            if (string.IsNullOrEmpty(info.ProductInstanceId))
            {
              info.ProductInstanceId = info.Tuner.DevicePath.Substring(5).ToLowerInvariant();
              info.TunerInstanceId = info.ProductInstanceId;
            }

            OnTunerDetected(info);
            break;
          }
        }
      }
    }

    private void DetectOcurTunerRemoval(DeviceDescriptor rootDeviceDescriptor)
    {
      IEnumerator<DeviceEntry> childDeviceEn = rootDeviceDescriptor.RootDescriptor.SSDPRootEntry.Devices.Values.GetEnumerator();
      while (childDeviceEn.MoveNext())
      {
        // Have to check for the tuner service to avoid Ceton tuning adaptor devices.
        foreach (string serviceUrn in childDeviceEn.Current.Services)
        {
          if (serviceUrn.Equals("urn:schemas-opencable-com:service:Tuner:1"))
          {
            TunerInfo info;
            if (_knownTunerInfo.TryGetValue(childDeviceEn.Current.UUID, out info))
            {
              OnTunerRemoved(info);
            }
            break;
          }
        }
      }
    }

    #endregion

    #endregion
  }
}