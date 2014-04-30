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

using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
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
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Dri;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Pbda;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.SatIp;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;
using UPnP.Infrastructure;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.SSDP;
using RtspClient = Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp.RtspClient;

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

    private struct TunerGroupInfo
    {
      public CardGroup Group;
      public string ProductInstanceIdentifier;
      public string TunerInstanceIdentifier;
      public HashSet<string> Tuners;
    }

    private HashSet<string> _firstDetectionTuners = new HashSet<string>();
    private HashSet<string> _knownUpnpRootDevices = new HashSet<string>();

    // device path/UUID => [tuner external IDs] (needed because device path and UUID are not directly used as the external ID)
    private Dictionary<string, HashSet<string>> _tunerExternalIds = new Dictionary<string, HashSet<string>>();
    // tuner external ID => tuner
    private Dictionary<string, ITVCard> _knownTuners = new Dictionary<string, ITVCard>();
    // product instance ID => tuner instance ID => tuner external IDs
    private Dictionary<string, Dictionary<string, HashSet<string>>> _detectedNaturalTunerGroups = new Dictionary<string, Dictionary<string, HashSet<string>>>();
    // database group ID => info
    private Dictionary<int, TunerGroupInfo> _configuredTunerGroups = new Dictionary<int, TunerGroupInfo>();

    // The listener that we notify when tuner changes are detected.
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
      _firstDetectionTuners.Clear();
      _knownUpnpRootDevices.Clear();
      _tunerExternalIds.Clear();
      _knownTuners.Clear();
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

      if (_upnpControlPoint != null)
      {
        _upnpControlPoint.DisconnectAll();
        _upnpControlPoint.Dispose();
        _upnpControlPoint = null;
      }
      if (_upnpAgent != null)
      {
        _upnpAgent.Dispose();
        _upnpAgent = null;
      }
      if (_systemDeviceChangeEventWatcher != null)
      {
        _systemDeviceChangeEventWatcher.Dispose();
        _systemDeviceChangeEventWatcher = null;
      }
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
      lock (_knownTuners)
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
        List<ITVCard> unavailableTuners = new List<ITVCard>();
        HashSet<string> unavailableTunerDevicePaths = new HashSet<string>();
        foreach (ITVCard tuner in _knownTuners.Values)
        {
          if (!knownTuners.Contains(tuner.ExternalId))
          {
            unavailableTuners.Add(tuner);
            foreach (KeyValuePair<string, HashSet<string>> pair in _tunerExternalIds)
            {
              if (pair.Value.Contains(tuner.ExternalId))
              {
                unavailableTunerDevicePaths.Add(pair.Key);
              }
            }
          }
        }
        foreach (ITVCard tuner in unavailableTuners)
        {
          OnTunerRemoved(tuner);
        }
        foreach (string devicePath in unavailableTunerDevicePaths)
        {
          _tunerExternalIds.Remove(devicePath);
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

    private void OnTunerDetected(ITVCard tuner)
    {
      this.LogInfo("  add...");
      this.LogInfo("    name        = {0}", tuner.Name);
      this.LogInfo("    external ID = {0}", tuner.ExternalId);
      this.LogInfo("    type        = {0}", tuner.CardType);
      this.LogInfo("    product ID  = {0}", tuner.ProductInstanceId ?? "[null]");
      this.LogInfo("    tuner ID    = {0}", tuner.TunerInstanceId ?? "[null]");
      _knownTuners.Add(tuner.ExternalId, tuner);

      // Detect the naturally related tuners. These are tuners with the same product and tuner
      // instance ID.
      HashSet<string> relatedTuners = FindRelatedTuners(tuner);

      // Find or create a group for the tuner if appropriate.
      TunerGroupInfo? groupInfo = null;
      Card tunerDbSettings = CardManagement.GetCardByDevicePath(tuner.ExternalId, CardIncludeRelationEnum.None);
      if (tunerDbSettings == null)
      {
        // First ever detection. Create the tuner settings.
        this.LogInfo("    new tuner...");
        _firstDetectionTuners.Add(tuner.ExternalId);
        tunerDbSettings = new Card
        {
          TimeshiftingFolder = string.Empty,
          RecordingFolder = string.Empty,
          DevicePath = tuner.ExternalId,
          Name = tuner.Name,
          Priority = 1,
          GrabEPG = (tuner.CardType != CardType.Analog && tuner.CardType != CardType.Atsc),   // analog signals don't carry EPG, ATSC EPG not supported
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
        CardManagement.SaveCard(tunerDbSettings);

        // If we have product/tuner instance information and detected the tuner is a member of a
        // natural group...
        if (tuner.ProductInstanceId != null && tuner.TunerInstanceId != null && relatedTuners != null && relatedTuners.Count > 1)
        {
          // Automatically add the tuner to an existing tuner group if the group product and tuner
          // instance information matches.
          groupInfo = AddNewTunerToExistingNaturalGroup(tuner, tunerDbSettings);

          // ...or if that doesn't apply, automatically add the tuner to an existing tuner group if
          // the group contains *all* the naturally related tuners. The assumption in this case is
          // that our group detection failed to automatically detect all the tuners in the group.
          bool foundAnyRelatedTunerInAnyGroup = false;
          if (groupInfo == null)
          {
            groupInfo = AddNewTunerToExistingExtendedNaturalGroup(tuner, tunerDbSettings, relatedTuners, out foundAnyRelatedTunerInAnyGroup);
          }

          // Finally, if all of the tuners in the natural group are new and not in any other group
          // then create a new group.
          if (groupInfo == null && !foundAnyRelatedTunerInAnyGroup)
          {
            groupInfo = CreateNewGroupOnFirstDetection(tuner, tunerDbSettings, relatedTuners);
          }
        }
      }
      else
      {
        // This tuner has been detected previously. Check if it is a member of an existing tuner
        // group.
        this.LogInfo("    existing tuner...");
        groupInfo = AddExistingTunerToConfiguredGroup(tuner, tunerDbSettings);
      }

      _eventListener.OnTunerAdded(tuner);
    }

    private HashSet<string> FindRelatedTuners(ITVCard tuner)
    {
      HashSet<string> relatedTuners = null;
      if (tuner.ProductInstanceId != null && tuner.TunerInstanceId != null)
      {
        Dictionary<string, HashSet<string>> productTuners;
        if (!_detectedNaturalTunerGroups.TryGetValue(tuner.ProductInstanceId, out productTuners))
        {
          productTuners = new Dictionary<string, HashSet<string>>();
          relatedTuners = new HashSet<string>();
          relatedTuners.Add(tuner.ExternalId);
          productTuners.Add(tuner.TunerInstanceId, relatedTuners);
          _detectedNaturalTunerGroups.Add(tuner.ProductInstanceId, productTuners);
        }
        else
        {
          if (!productTuners.TryGetValue(tuner.TunerInstanceId, out relatedTuners))
          {
            relatedTuners = new HashSet<string>();
            productTuners.Add(tuner.TunerInstanceId, relatedTuners);
          }
          else if (relatedTuners.Count > 0)
          {
            this.LogInfo("    detected naturally related tuners...");
            foreach (string id in relatedTuners)
            {
              ITVCard t = _knownTuners[id];
              this.LogInfo("      name = {0}, type = {1}, external ID = {2}", t.Name, t.CardType, id);
            }
          }
          relatedTuners.Add(tuner.ExternalId);
        }
      }
      return relatedTuners;
    }

    private TunerGroupInfo? AddNewTunerToExistingNaturalGroup(ITVCard tuner, Card tunerDbSettings)
    {
      foreach (TunerGroupInfo groupInfo in _configuredTunerGroups.Values)
      {
        if (groupInfo.ProductInstanceIdentifier == tuner.ProductInstanceId && groupInfo.TunerInstanceIdentifier == tuner.TunerInstanceId)
        {
          this.LogInfo("    detected existing tuner group with matching information, name = {0}...", groupInfo.Group.Name);
          groupInfo.Tuners.Add(tuner.ExternalId);
          CardGroupMap map = new CardGroupMap();
          map.IdCard = tunerDbSettings.IdCard;
          map.IdCardGroup = groupInfo.Group.IdCardGroup;
          CardManagement.SaveCardGroupMap(map);
          return groupInfo;
        }
      }
      return null;
    }

    private TunerGroupInfo? AddNewTunerToExistingExtendedNaturalGroup(ITVCard tuner, Card tunerDbSettings, HashSet<string> relatedTuners, out bool foundAnyRelatedTunerInAnyGroup)
    {
      foundAnyRelatedTunerInAnyGroup = false;
      foreach (TunerGroupInfo groupInfo in _configuredTunerGroups.Values)
      {
        // Only look at mixed product/tuner instance groups. AddNewTunerToExistingNaturalGroup() already
        // checked and failed to find a non-extended/pure group.
        if (groupInfo.ProductInstanceIdentifier == null && groupInfo.TunerInstanceIdentifier == null)
        {
          // For each naturally related tuner that is not this tuner...
          foreach (string t1 in relatedTuners)
          {
            if (!t1.Equals(tuner.ExternalId))
            {
              if (groupInfo.Tuners.Contains(t1))
              {
                // Okay, we found a group containing one of the related tuners. If all of the
                // related tuners are in this group then we'll add the tuner to this group.
                // Otherwise this tuner won't get a group.
                foundAnyRelatedTunerInAnyGroup = true;
                foreach (string t2 in relatedTuners)
                {
                  if (!t2.Equals(tuner.ExternalId) && !t2.Equals(t1))
                  {
                    if (!groupInfo.Tuners.Contains(t1))
                    {
                      // One of the related tuners is not in this group => its all over.
                      return null;
                    }
                  }
                }
                // All tuners are in the group!
                this.LogInfo("    detected existing tuner group containing all related tuners, name = {0}...", groupInfo.Group.Name);
                groupInfo.Tuners.Add(tuner.ExternalId);
                CardGroupMap map = new CardGroupMap();
                map.IdCard = tunerDbSettings.IdCard;
                map.IdCardGroup = groupInfo.Group.IdCardGroup;
                CardManagement.SaveCardGroupMap(map);
                return groupInfo;
              }
            }
          }
        }
      }
      return null;
    }

    private TunerGroupInfo? CreateNewGroupOnFirstDetection(ITVCard tuner, Card tunerDbSettings, HashSet<string> relatedTuners)
    {
      bool createNewGroup = true;
      foreach (string t in relatedTuners)
      {
        if (!_firstDetectionTuners.Contains(t))
        {
          createNewGroup = false;
          break;
        }
      }
      if (createNewGroup)
      {
        CardGroup group = new CardGroup();
        group.Name = string.Format("Product Instance {0} Tuner {1}", tuner.ProductInstanceId, tuner.TunerInstanceId);
        CardManagement.SaveCardGroup(group);
        TunerGroupInfo groupInfo = new TunerGroupInfo();
        groupInfo.Group = group;
        groupInfo.ProductInstanceIdentifier = tuner.ProductInstanceId;
        groupInfo.TunerInstanceIdentifier = tuner.TunerInstanceId;
        groupInfo.Tuners = new HashSet<string>();
        _configuredTunerGroups.Add(group.IdCardGroup, groupInfo);
        foreach (string t in relatedTuners)
        {
          CardGroupMap map = new CardGroupMap();
          map.IdCard = CardManagement.GetCardByDevicePath(t, CardIncludeRelationEnum.None).IdCard;
          map.IdCardGroup = group.IdCardGroup;
          CardManagement.SaveCardGroupMap(map);
          groupInfo.Tuners.Add(t);
        }
        this.LogInfo("    creating new tuner group, name = {0}...", group.Name);
        return groupInfo;
      }
      return null;
    }

    private TunerGroupInfo? AddExistingTunerToConfiguredGroup(ITVCard tuner, Card tunerDbSettings)
    {
      IList<CardGroup> groups = CardManagement.ListAllCardGroups();
      foreach (CardGroup group in groups)
      {
        TrackableCollection<CardGroupMap> groupMaps = group.CardGroupMaps;
        foreach (CardGroupMap map in groupMaps)
        {
          // Does the tuner belong to this group?
          if (map.IdCard == tunerDbSettings.IdCard)
          {
            // Find and update our local group info.
            TunerGroupInfo groupInfo;
            if (!_configuredTunerGroups.TryGetValue(group.IdCardGroup, out groupInfo))
            {
              groupInfo = new TunerGroupInfo();
              groupInfo.Group = group;
              groupInfo.ProductInstanceIdentifier = tuner.ProductInstanceId;
              groupInfo.TunerInstanceIdentifier = tuner.TunerInstanceId;
              groupInfo.Tuners = new HashSet<string>();
              _configuredTunerGroups.Add(group.IdCardGroup, groupInfo);
              this.LogInfo("    detected existing tuner group, name = {0}, product instance ID = {1}, tuner instance ID = {2}...", group.Name, groupInfo.ProductInstanceIdentifier ?? "[null]", groupInfo.TunerInstanceIdentifier ?? "[null]");
            }
            else
            {
              this.LogInfo("    detected existing tuner group, name = {0}, product instance ID = {1}, tuner instance ID = {2}...", group.Name, groupInfo.ProductInstanceIdentifier ?? "[null]", groupInfo.TunerInstanceIdentifier ?? "[null]");
              // Does the product/tuner instance info indicate that this is a natural group?
              if (groupInfo.ProductInstanceIdentifier != null && groupInfo.TunerInstanceIdentifier != null &&
                tuner.ProductInstanceId != null && tuner.TunerInstanceId != null &&
                (groupInfo.ProductInstanceIdentifier != tuner.ProductInstanceId || groupInfo.TunerInstanceIdentifier != tuner.TunerInstanceId))
              {
                groupInfo.ProductInstanceIdentifier = null;
                groupInfo.TunerInstanceIdentifier = null;
              }
            }
            if (groupInfo.Tuners.Count > 0)
            {
              foreach (string id in groupInfo.Tuners)
              {
                ITVCard t = _knownTuners[id];
                this.LogInfo("      name = {0}, type = {1}, external ID = {2}", t.Name, t.CardType, id);
              }
            }
            groupInfo.Tuners.Add(tuner.ExternalId);
            // Tuners can't be a member of more than one group.
            return groupInfo;
          }
        }
      }
      return null;
    }

    private void OnTunerRemoved(ITVCard tuner)
    {
      this.LogInfo("  remove...");
      this.LogInfo("    name        = {0}", tuner.Name);
      this.LogInfo("    external ID = {0}", tuner.ExternalId);
      this.LogInfo("    type        = {0}", tuner.CardType);
      this.LogInfo("    product ID  = {0}", tuner.ProductInstanceId ?? "[null]");
      this.LogInfo("    tuner ID    = {0}", tuner.TunerInstanceId ?? "[null]");
      _eventListener.OnTunerRemoved(tuner);

      foreach (Dictionary<string, HashSet<string>> productTunerInstance in _detectedNaturalTunerGroups.Values)
      {
        foreach (HashSet<string> tunerInstance in productTunerInstance.Values)
        {
          tunerInstance.Remove(tuner.ExternalId);
        }
      }

      foreach (TunerGroupInfo groupInfo in _configuredTunerGroups.Values)
      {
        groupInfo.Tuners.Remove(tuner.ExternalId);
      }

      _firstDetectionTuners.Remove(tuner.ExternalId);
      _knownTuners.Remove(tuner.ExternalId);
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
              knownTuners.Add(tuner.ExternalId);
              if (!_knownTuners.ContainsKey(tuner.ExternalId))
              {
                OnTunerDetected(tuner);
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
            for (int i = 1; i <= streamTunerCount; i++)
            {
              ITVCard tuner = new TunerStreamElecard(i);
              knownTuners.Add(tuner.ExternalId);
              if (!_knownTuners.ContainsKey(tuner.ExternalId))
              {
                OnTunerDetected(tuner);
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
        for (int i = 1; i <= streamTunerCount; i++)
        {
          ITVCard tuner = new TunerStream(i);
          knownTuners.Add(tuner.ExternalId);
          if (!_knownTuners.ContainsKey(tuner.ExternalId))
          {
            OnTunerDetected(tuner);
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
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(device.Name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        HashSet<string> tunerExternalIds;
        if (_tunerExternalIds.TryGetValue(devicePath, out tunerExternalIds))
        {
          knownTuners.UnionWith(tunerExternalIds);
          device.Dispose();
          continue;
        }
        ITVCard tuner = new TunerAnalog(device, FilterCategory.AMKSCrossbar);
        knownTuners.Add(tuner.ExternalId);
        _tunerExternalIds.Add(devicePath, new HashSet<string>() { tuner.ExternalId });
        OnTunerDetected(tuner);
      }
    }

    private void DetectSupportedAmKsCaptureDevices(ref HashSet<string> knownTuners)
    {
      this.LogDebug("detector: detect AM KS capture devices");
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      foreach (DsDevice device in devices)
      {
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(device.Name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        HashSet<string> tunerExternalIds;
        if (_tunerExternalIds.TryGetValue(devicePath, out tunerExternalIds))
        {
          knownTuners.UnionWith(tunerExternalIds);
          device.Dispose();
          continue;
        }

        // We don't want to add duplicate entries for multi-input capture
        // devices (already detected via crossbar).
        ITVCard tuner = new TunerAnalog(device, FilterCategory.AMKSCapture);
        if (tuner.ProductInstanceId != null)
        {
          bool found = false;
          foreach (ITVCard t in _knownTuners.Values)
          {
            if (tuner.ProductInstanceId.Equals(t.ProductInstanceId))
            {
              // This source has a crossbar. Don't use it.
              found = true;
              _tunerExternalIds.Add(devicePath, new HashSet<string>() { t.ExternalId });
              tuner.Dispose();
              break;
            }
          }
          if (found)
          {
            continue;
          }
        }

        knownTuners.Add(tuner.ExternalId);
        _tunerExternalIds.Add(devicePath, new HashSet<string>() { tuner.ExternalId });
        OnTunerDetected(tuner);
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

        HashSet<string> tunerExternalIds;
        if (_tunerExternalIds.TryGetValue(devicePath, out tunerExternalIds))
        {
          knownTuners.UnionWith(tunerExternalIds);
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

          tunerExternalIds = new HashSet<string>();
          foreach (ITVCard t in tuners)
          {
            tunerExternalIds.Add(t.ExternalId);
            knownTuners.Add(t.ExternalId);
            if (_knownTuners.ContainsKey(t.ExternalId))
            {
              t.Dispose();
            }
            else
            {
              OnTunerDetected(t);
            }
          }
          _tunerExternalIds.Add(devicePath, tunerExternalIds);
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
        this.LogError("  failed to get node descriptors from topology, hr = 0x{0:x}", hr);
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
      lock (_knownTuners)
      {
        if (!_knownUpnpRootDevices.Add(rootDescriptor.SSDPRootEntry.RootDeviceUUID))
        {
          this.LogWarn("detector: re-detecting known root device {0}", rootDescriptor.SSDPRootEntry.RootDeviceUUID);
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

      lock (_knownTuners)
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
        HashSet<string> tunerExternalIds;
        if (_tunerExternalIds.TryGetValue(rootDeviceDescriptor.DeviceUUID, out tunerExternalIds))
        {
          foreach (string t in tunerExternalIds)
          {
            ITVCard tuner;
            if (_knownTuners.TryGetValue(t, out tuner))
            {
              OnTunerRemoved(tuner);
            }
          }
          _tunerExternalIds.Remove(rootDeviceDescriptor.DeviceUUID);
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
      string remoteHost = new Uri(rootDeviceDescriptor.RootDescriptor.SSDPRootEntry.PreferredLink.DescriptionLocation).Host;

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
            this.LogError("detector: failed to interpret SAT>IP X_SATIPCAP {0}, section {1}", it.Current.Value, section);
          }
        }
      }
      else
      {
        // X_SATIPCAP is not available. Try an RTSP DESCRIBE.
        this.LogDebug("detector: attempt to get capabilities using RTSP DESCRIBE");
        RtspResponse response = null;
        try
        {
          RtspRequest request = new RtspRequest(RtspMethod.Describe, string.Format("rtsp://{0}/", remoteHost));
          request.Headers.Add("Accept", "application/sdp");
          request.Headers.Add("Connection", "close");
          RtspClient client = new RtspClient(remoteHost, 554);
          client.SendRequest(request, out response);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "detector: SAT>IP RTSP DESCRIBE request and/or response failed");
        }
        if (response != null)
        {
          if (response.StatusCode == RtspStatusCode.Ok)
          {
            Match m = Regex.Match(response.Body, @"s=SatIPServer:1\s+([^\s]+)\s+", RegexOptions.Singleline | RegexOptions.IgnoreCase);
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
                this.LogError(ex, "detector: failed to interpret SAT>IP RTSP DESCRIBE response SatIPServer section {0}", frontEndInfo);
              }
            }
            else
            {
              this.LogDebug("detector: RTSP DESCRIBE response does not contain SatIPServer section");
            }
          }
          else if (response.StatusCode == RtspStatusCode.NotFound)
          {
            this.LogDebug("detector: server does not have any active streams");
          }
          else
          {
            this.LogError("detector: SAT>IP server RTSP DESCRIBE response status code {0} {1}", response.StatusCode, response.ReasonPhrase);
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

      HashSet<string> tunerExternalIds = new HashSet<string>();
      for (int i = 1; i <= satelliteFrontEndCount; i++)
      {
        ITVCard tuner = new TunerSatIpSatellite(rootDeviceDescriptor, i);
        tunerExternalIds.Add(tuner.ExternalId);
        OnTunerDetected(tuner);
      }
      for (int i = satelliteFrontEndCount + 1; i <= satelliteFrontEndCount + terrestrialFrontEndCount; i++)
      {
        // Currently the Digital Devices Octopus Net is the only SAT>IP product
        // to support DVB-T/T2. The DVB-T tuners also support DVB-C.
        ITVCard tuner = new TunerSatIpTerrestrial(rootDeviceDescriptor, i);
        tunerExternalIds.Add(tuner.ExternalId);
        OnTunerDetected(tuner);
        tuner = new TunerSatIpCable(rootDeviceDescriptor, i);
        tunerExternalIds.Add(tuner.ExternalId);
        OnTunerDetected(tuner);
      }
      _tunerExternalIds.Add(rootDeviceDescriptor.DeviceUUID, tunerExternalIds);
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
      HashSet<string> tunerExternalIds = new HashSet<string>();
      IEnumerator<DeviceEntry> childDeviceEn = rootDeviceDescriptor.RootDescriptor.SSDPRootEntry.Devices.Values.GetEnumerator();
      while (childDeviceEn.MoveNext())
      {
        // Have to check for the tuner service to avoid Ceton tuning adaptor devices.
        foreach (string serviceUrn in childDeviceEn.Current.Services)
        {
          if (serviceUrn.Equals("urn:schemas-opencable-com:service:Tuner:1"))
          {
            ITVCard tuner = new TunerDri(rootDeviceDescriptor.FindDevice(childDeviceEn.Current.UUID), _upnpControlPoint);
            tunerExternalIds.Add(tuner.ExternalId);
            OnTunerDetected(tuner);
            break;
          }
        }
      }
      if (tunerExternalIds.Count > 0)
      {
        _tunerExternalIds.Add(rootDeviceDescriptor.DeviceUUID, tunerExternalIds);
      }
    }

    #endregion

    #endregion
  }
}