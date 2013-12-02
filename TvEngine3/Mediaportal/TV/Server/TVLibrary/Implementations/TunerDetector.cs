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

using UPnP.Infrastructure.CP;
using System.Collections.Generic;
using System.Management;
using System;
using UPnP.Infrastructure.CP.SSDP;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using UPnP.Infrastructure;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using UPnP.Infrastructure.CP.Description;
using Microsoft.Win32;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Pbda;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda;
using System.IO;

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
    private object _lockObject = new object();

    // network providers
    private bool _isMicrosoftGenericNpAvailable = false;
    private bool _isMediaPortalNpAvailable = false;
    private IBaseFilter _atscNp = null;
    private IBaseFilter _dvbcNp = null;
    private IBaseFilter _dvbsNp = null;
    private IBaseFilter _dvbtNp = null;

    private IFilterGraph2 _graph = null;
    private DsROTEntry _rotEntry = null;

    private struct TunerInfo
    {
      public string ExternalId;
      public string Name;
      public string ProductId;
      public string ProductInstanceId;
      public string TunerInstanceId;
    }

    private HashSet<string> _knownUpnpRootDevices = new HashSet<string>();
    private Dictionary<string, ITVCard> _knownTuners = new Dictionary<string, ITVCard>();
    private Dictionary<string, TunerInfo> _knownTunerInfo = new Dictionary<string, TunerInfo>();
    private HashSet<string> _knownDirectShowTuners = new HashSet<string>();

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
        this.LogCritical(ex, "Detector: failed to initialise the DirectShow tuner detection graph");
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
      this.LogInfo("Detector: starting tuner detection...");
      // Start detecting BDA and WDM tuners.
      DetectBdaWdmTuners();
      _systemDeviceChangeEventWatcher.Start();

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
    }

    public void Reset()
    {
      Stop();
      _knownUpnpRootDevices.Clear();
      _knownDirectShowTuners.Clear();
      Start();
    }

    public void Stop()
    {
      this.LogInfo("Detector: stopping tuner detection...");
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
        if (_atscNp != null)
        {
          _graph.RemoveFilter(_atscNp);
        }
        if (_dvbcNp != null)
        {
          _graph.RemoveFilter(_dvbcNp);
        }
        if (_dvbsNp != null)
        {
          _graph.RemoveFilter(_dvbsNp);
        }
        if (_dvbtNp != null)
        {
          _graph.RemoveFilter(_dvbtNp);
        }
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
      // devices once. Also, the first event may occur before the device is
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
      DetectBdaWdmTuners();
    }

    private void DetectBdaWdmTuners()
    {
      lock (_lockObject)
      {
        Log.Log.Debug("Detecting BDA/WDM tuners...");

        HashSet<string> previouslyKnownTuners = _knownDirectShowTuners;
        _knownDirectShowTuners = new HashSet<string>();

        // Detect TechniSat SkyStar/AirStar/CableStar 2 and stream source tuners.
        DetectSupportedLegacyAmFilterDevices(ref previouslyKnownTuners, ref _knownDirectShowTuners);

        // Detect analog tuners and capture devices.
        DetectSupportedAmKsCrossbars(ref previouslyKnownTuners, ref _knownDirectShowTuners);

        // Detect BDA and PBDA tuners.
        DetectSupportedBdaSources(ref previouslyKnownTuners, ref _knownDirectShowTuners);

        // Remove the devices that are no longer connected.
        foreach (string previouslyKnownDevice in previouslyKnownTuners)
        {
          if (!_knownDirectShowTuners.Contains(previouslyKnownDevice))
          {
            Log.Log.Info("BDA/WDM device {0} removed", previouslyKnownDevice);
            _eventListener.OnDeviceRemoved(previouslyKnownDevice);
          }
        }
      }
    }

    private void InitDirectShowDetectionGraph()
    {
      this.LogDebug("Detector: initialise DirectShow detection graph");
      _graph = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graph);

      ITuningSpace tuningSpace = null;
      ILocator locator = null;

      // The MediaPortal and Microsoft generic network providers must be added/removed to/from the
      // graph for each tuner that is checked. If you don't do this, the network types list gets
      // longer and longer and longer.
      // MS generic, MCE 2005 roll-up 2 or better
      _isMicrosoftGenericNpAvailable = FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID);
      // MediaPortal private network provider, in testing - contact MisterD
      _isMediaPortalNpAvailable = File.Exists(PathManager.BuildAssemblyRelativePath("NetworkProvider.ax"));

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
        this.LogError(ex, "Detector: failed to add and configure specific Microsoft network provider(s), is BDA installed?");
      }
    }

    private void DetectSupportedLegacyAmFilterDevices(ref HashSet<string> previouslyKnownTuners, ref HashSet<string> knownTuners)
    {
      this.LogDebug("Detector: detect legacy AM filter devices");

      int iptvTunerCount = SettingsManagement.GetValue("iptvCardCount", 1);

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }

        if (name.Equals("B2C2 MPEG-2 Source"))
        {
          this.LogInfo("Detector: detected B2C2 root device, detecting tuners");
          IEnumerable<ITVCard> b2c2Tuners = TunerB2c2Base.DetectTuners();
          foreach (ITVCard tuner in b2c2Tuners)
          {
            knownTuners.Add(tuner.DevicePath);
            if (!previouslyKnownTuners.Contains(tuner.DevicePath))
            {
              _eventListener.OnDeviceAdded(tuner);
            }
          }
          device.Dispose();
        }
        else if (name.Equals("Elecard NWSource-Plus"))
        {
          for (int i = 0; i < iptvTunerCount; i++)
          {
            TunerStreamElecard iptvTuner = new TunerStreamElecard(device, i);
            knownTuners.Add(iptvTuner.DevicePath);
            if (!previouslyKnownTuners.Contains(iptvTuner.DevicePath))
            {
              Log.Log.Info("Detected new Elecard IPTV tuner {0} {1}", iptvTuner.Name, iptvTuner.DevicePath);
              _eventListener.OnDeviceAdded(iptvTuner);
            }
            else
            {
              iptvTuner.Dispose();
            }
          }
        }
        else if (name.Equals("MediaPortal IPTV Source Filter"))
        {
          for (int i = 0; i < iptvTunerCount; i++)
          {
            TunerStream iptvTuner = new TunerStream(connectedDevice, i);
            knownTuners.Add(iptvTuner.DevicePath);
            if (!previouslyKnownTuners.Contains(iptvTuner.DevicePath))
            {
              Log.Log.Info("Detected new MediaPortal IPTV tuner {0} {1}", iptvTuner.Name, iptvTuner.DevicePath);
              _eventListener.OnDeviceAdded(iptvTuner);
            }
            else
            {
              iptvTuner.Dispose();
            }
          }
        }
      }
    }

    private void DetectSupportedAmKsCrossbars(ref HashSet<string> previouslyKnownTuners, ref HashSet<string> knownTuners)
    {
      Log.Log.Debug("Detect AM KS crossbar devices");
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
      foreach (DsDevice device in devices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(devicePath))
        {
          knownTuners.Add(devicePath);
          if (!previouslyKnownTuners.Contains(devicePath))
          {
            Log.Log.Info("Detected new Hauppauge capture device {0} {1)", name, devicePath);
            TvCardHDPVR captureDevice = new TvCardHDPVR(connectedDevice);
            _eventListener.OnDeviceAdded(captureDevice);
          }
        }
      }
    }

    private void DetectSupportedBdaSources(ref HashSet<string> previouslyKnownTuners, ref HashSet<string> knownTuners)
    {
      this.LogDebug("Detector: detect BDA sources");

      DsDevice[] connectedDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice device in connectedDevices)
      {
        string name = device.Name;
        string devicePath = device.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          device.Dispose();
          continue;
        }
        if (previouslyKnownTuners.Contains(devicePath))
        {
          knownTuners.Add(devicePath);
          continue;
        }

        IBaseFilter tunerFilter;
        try
        {
          tunerFilter = FilterGraphTools.AddFilterFromDevice(_graph, device);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Detector: failed to add BDA source filter to detect tuner type for {0}", name);
          continue;
        }

        try
        {
          this.LogInfo("Detector: detected new BDA source {0} {1}", name, devicePath);

          // Silicondust regular (non-CableCARD) HDHomeRun. Workaround for tuner type detection
          // issue: the MS generic NP would always detect DVB-C/T models as DVB-T.
          bool isCablePreferred = false;
          if (name.StartsWith("Silicondust HDHomeRun Tuner"))
          {
            isCablePreferred = GetHdHomeRunSourceType(name).Equals("Digital Cable");
          }

          // Try the MediaPortal network provider first.
          ITVCard tuner = null;
          if (_isMediaPortalNpAvailable)
          {
            this.LogDebug("  check type with MediaPortal network provider");
            tuner = DetectBdaSourceTypeMediaPortalNetworkProvider(tunerFilter, device, isCablePreferred);
          }
          // Try the Microsoft network provider next if the MediaPortal network provider failed and
          // the MS generic NP is available.
          if (tuner == null && _isMicrosoftGenericNpAvailable)
          {
            this.LogDebug("  check type with Microsoft generic network provider");
            tuner = DetectBdaSourceTypeMicrosoftGenericNetworkProvider(tunerFilter, device, isCablePreferred);
          }
          // Last shot is the old style Microsoft network providers.
          if (tuner == null)
          {
            this.LogDebug("  check type with specific Microsoft network providers");
            tuner = DetectBdaSourceTypeMicrosoftSpecificNetworkProvider(tunerFilter, device, isCablePreferred);
          }

          if (tuner != null)
          {
            Log.Log.Info("  tuner type = {0}", tuner.CardType);
            knownTuners.Add(devicePath);
            _eventListener.OnDeviceAdded(tuner);
          }
        }
        finally
        {
          _graph.RemoveFilter(tmpDeviceFilter);
          Release.ComObject("device detection device filter", tmpDeviceFilter);
        }
      }
    }

    private ITVCard DetectBdaSourceTypeMediaPortalNetworkProvider(IBaseFilter filter, DsDevice device, bool isCablePreferred)
    {
      IBaseFilter mpNp = null;
      try
      {
        mpNp = FilterGraphTools.AddFilterFromFile(_graph, "NetworkProvider.ax", typeof(MediaPortalNetworkProvider).GUID, "MediaPortal Network Provider");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "  failed to add MediaPortal network provider to detection graph");
        return null;
      }

      try
      {
        IDvbNetworkProvider interfaceNetworkProvider = mpNp as IDvbNetworkProvider;
        string hash = MediaPortalNetworkProvider.GetHash(device.DevicePath);
        interfaceNetworkProvider.ConfigureLogging(MediaPortalNetworkProvider.GetFileName(device.DevicePath), hash, LogLevelOption.Debug);
        if (!ConnectFilter(_graph, mpNp, filter))
        {
          this.LogDebug("  failed to connect to MediaPortal network provider");
          return null;
        }

        TuningType tuningTypes;
        int hr = interfaceNetworkProvider.GetAvailableTuningTypes(out tuningTypes);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("  connected to MediaPortal network provider but failed to get available tuning types, hr = 0x{0:x}", hr);
          return null;
        }

        this.LogDebug("  tuning types = {0}, hash = {1}", tuningTypes, hash);
        if ((tuningTypes & TuningType.DvbT) != 0 && !isCablePreferred)
        {
          return new TunerBdaTerrestrial(device);
        }
        if ((tuningTypes & TuningType.DvbS) != 0 && !isCablePreferred)
        {
          return new TunerBdaSatellite(device);
        }
        if ((tuningTypes & TuningType.DvbC) != 0)
        {
          return new TunerBdaCable(device);
        }
        if ((tuningTypes & TuningType.Atsc) != 0)
        {
          return new TunerBdaAtsc(device);
        }

        this.LogDebug("  connected to MediaPortal network provider but type not recognised");
        return null;
      }
      finally
      {
        _graph.RemoveFilter(mpNp);
        Release.ComObject("tuner detector MediaPortal network provider", ref mpNp);
      }
    }

    private ITVCard DetectBdaSourceTypeMicrosoftGenericNetworkProvider(IBaseFilter filter, DsDevice device, bool isCablePreferred)
    {
      IBaseFilter genericNp = null;
      try
      {
        genericNp = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, typeof(NetworkProvider).GUID, "Microsoft Network Provider");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "  failed to add Microsoft generic network provider to detection graph");
        return null;
      }

      try
      {
        if (!ConnectFilter(_graph, genericNp, filter))
        {
          this.LogDebug("  failed to connect to Microsoft generic network provider");
          return null;
        }

        int networkTypesMax = 10;
        int networkTypeCount;
        Guid[] networkTypes = new Guid[networkTypesMax];
        int hr = (genericNp as ITunerCap).get_SupportedNetworkTypes(networkTypesMax, out networkTypeCount, networkTypes);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("  connected to Microsoft generic network provider but failed to get supported network types, hr = 0x{0:x}", hr);
          return null;
        }

        this.LogDebug("  network type count = {0}", networkTypeCount);
        for (int n = 0; n < networkTypeCount; n++)
        {
          this.LogDebug("  network type {0} = {1}", n, networkTypes[n]);
        }
        bool isLocalDigitalCableSupported = false;
        for (int n = 0; n < networkTypeCount; n++)
        {
          Guid networkType = networkTypes[n];
          if (networkType == typeof(DVBTNetworkProvider).GUID && !isCablePreferred)
          {
            return new TunerBdaTerrestrial(device);
          }
          if (networkType == typeof(DVBSNetworkProvider).GUID && !isCablePreferred)
          {
            return new TunerBdaSatellite(device);
          }
          if (networkType == typeof(DVBCNetworkProvider).GUID)
          {
            return new TunerBdaCable(device);
          }
          if (networkType == typeof(ATSCNetworkProvider).GUID)
          {
            if (filter is IBDA_ConditionalAccess)
            {
              return new TunerPbdaCableCard(device);
            }
            return new TunerBdaAtsc(device);
          }

          // BDA hybrid ATSC/QAM, BDA DVB-C and PBDA CableCARD tuners all advertise this network
          // type. ATSC and DVB-C tuners usually advertise additional types which allow us to
          // identifiy them. If we don't see those types, we assume this is a QAM or CableCARD
          // tuner.
          if (networkType == MediaPortalGuid.DIGITAL_CABLE_NETWORK_TYPE)
          {
            isLocalDigitalCableSupported = true;
          }
        }

        if (isLocalDigitalCableSupported)
        {
          if (filter is IBDA_ConditionalAccess)
          {
            return new TunerPbdaCableCard(device);
          }
          return new TunerBdaAtsc(device);
        }
        this.LogDebug("  connected to Microsoft generic network provider but type not recognised");
        return null;
      }
      finally
      {
        _graph.RemoveFilter(genericNp);
        Release.ComObject("tuner detector Microsoft generic network provider", ref genericNp);
      }
    }

    private ITVCard DetectBdaSourceTypeMicrosoftSpecificNetworkProvider(IBaseFilter filter, DsDevice device, bool isCablePreferred)
    {
      if (ConnectFilter(_graph, _dvbtNp, filter))
      {
        return new TunerBdaTerrestrial(device);
      }
      if (ConnectFilter(_graph, _dvbcNp, filter))
      {
        // PBDA CableCARD tuners connect with the DVB-C network provider because they advertise the
        // local cable tuning type.
        if (filter is IBDA_ConditionalAccess)
        {
          return new TunerPbdaCableCard(device);
        }
        return new TunerBdaCable(device);
      }
      if (ConnectFilter(_graph, _dvbsNp, filter))
      {
        return new TunerBdaSatellite(device);
      }
      if (ConnectFilter(_graph, _atscNp, filter))
      {
        if (filter is IBDA_ConditionalAccess)
        {
          return new TunerPbdaCableCard(device);
        }
        return new TunerBdaAtsc(device);
      }

      this.LogDebug("  failed to connect to specific Microsoft network provider");
      return null;
    }

    #endregion

    #region UPnP device detection

    private void OnUpnpRootDeviceAdded(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor == null || rootDescriptor.State != RootDescriptorState.Ready)
      {
        return;
      }
      if (_knownUpnpRootDevices.Contains(rootDescriptor.SSDPRootEntry.RootDeviceUUID))
      {
        this.LogWarn("Detector: ignoring known root device {0}", rootDescriptor.SSDPRootEntry.RootDeviceUUID);
        return;
      }

      _knownUpnpRootDevices.Add(rootDescriptor.SSDPRootEntry.RootDeviceUUID);
      DeviceDescriptor deviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
      IEnumerator<DeviceEntry> childDeviceEn = rootDescriptor.SSDPRootEntry.Devices.Values.GetEnumerator();
      bool isFirst = true;
      while (childDeviceEn.MoveNext())
      {
        foreach (string serviceUrn in childDeviceEn.Current.Services)
        {
          // Supported device?
          if (serviceUrn.Equals("urn:schemas-opencable-com:service:Tuner:1"))
          {
            if (isFirst)
            {
              isFirst = false;
              this.LogInfo("Detector: OCUR/DRI tuner added {0} {1}", deviceDescriptor.FriendlyName, rootDescriptor.SSDPRootEntry.RootDeviceUUID);
            }

            // Find the corresponding DeviceDescriptor.
            IEnumerator<DeviceDescriptor> childDeviceDescriptorEn = deviceDescriptor.ChildDevices.GetEnumerator();
            while (childDeviceDescriptorEn.MoveNext())
            {
              if (childDeviceDescriptorEn.Current.DeviceUUID == childDeviceEn.Current.UUID)
              {
                break;
              }
            }
            this.LogInfo("  add {0} {1}", childDeviceDescriptorEn.Current.FriendlyName, childDeviceDescriptorEn.Current.DeviceUDN);
            ITVCard tuner = new TunerDri(childDeviceDescriptorEn.Current, _upnpControlPoint);
            TunerInfo info = new TunerInfo();
            info.ExternalId = tuner.DevicePath;
            info.Name = tuner.Name;
            info.ProductId = ???;
            info.ProductInstanceId = rootDescriptor.SSDPRootEntry.RootDeviceUUID;
            info.TunerInstanceId = 
            _eventListener.OnDeviceAdded();
            break;
          }
        }
      }
    }

    private void OnUpnpRootDeviceRemoved(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor == null)
      {
        return;
      }

      _knownUpnpRootDevices.Remove(rootDescriptor.SSDPRootEntry.RootDeviceUUID);
      DeviceDescriptor deviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
      IEnumerator<DeviceEntry> childDeviceEn = rootDescriptor.SSDPRootEntry.Devices.Values.GetEnumerator();
      bool isFirst = true;
      while (childDeviceEn.MoveNext())
      {
        foreach (string serviceUrn in childDeviceEn.Current.Services)
        {
          if (serviceUrn.Equals("urn:schemas-opencable-com:service:Tuner:1"))
          {
            if (isFirst)
            {
              isFirst = false;
              this.LogInfo("Detector: OCUR/DRI tuner removed {0} {1}", deviceDescriptor.FriendlyName, rootDescriptor.SSDPRootEntry.RootDeviceUUID);
            }

            IEnumerator<DeviceDescriptor> childDeviceDescriptorEn = deviceDescriptor.ChildDevices.GetEnumerator();
            while (childDeviceDescriptorEn.MoveNext())
            {
              if (childDeviceDescriptorEn.Current.DeviceUUID == childDeviceEn.Current.UUID)
              {
                break;
              }
            }
            this.LogInfo("  remove {0} {1}", childDeviceDescriptorEn.Current.FriendlyName, childDeviceDescriptorEn.Current.DeviceUDN);
            _eventListener.OnDeviceRemoved(childDeviceDescriptorEn.Current.DeviceUDN);
            break;
          }
        }
      }
    }

    #endregion

    #region hardware specific functions

    private string GetHdHomeRunSourceType(string tunerName)
    {
      try
      {
        // The tuner settings (configured by "HDHomeRun Setup" - part of the
        // Silicondust HDHomeRun driver and software package) are stored in the
        // registry. Example:
        // tuner device name = "Silicondust HDHomeRun Tuner 1210551E-0"
        // registry key = "HKEY_LOCAL_MACHINE\SOFTWARE\Silicondust\HDHomeRun\Tuners\1210551E-0"
        // possible source type values =
        //    "Digital Cable" [DVB-C, clear QAM]
        //    "Digital Antenna" [DVB-T, ATSC]
        //    "CableCARD" [North American encrypted cable television]
        string serialNumber = tunerName.Replace("Silicondust HDHomeRun Tuner ", "");
        using (
          RegistryKey registryKey =
            Registry.LocalMachine.OpenSubKey(string.Format(@"SOFTWARE\Silicondust\HDHomeRun\Tuners\{0}", serialNumber)))
        {
          if (registryKey != null)
          {
            return registryKey.GetValue("Source").ToString();
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Detector: failed to check HDHomeRun preferred mode");
      }
      return "Digital Antenna";
    }

    #endregion
  }
}