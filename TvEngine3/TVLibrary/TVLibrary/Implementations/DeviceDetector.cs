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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Microsoft.Win32;
using TvDatabase;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.Dri;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Pbda;
using TvLibrary.Implementations.RadioWebStream;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using UPnP.Infrastructure;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;
using UPnP.Infrastructure.CP.SSDP;


namespace TvLibrary.Implementations
{
  /// <summary>
  /// This class continually monitors device events.
  /// </summary>
  public class DeviceDetector : IDisposable
  {
    // Used for detecting and communicating with UPnP devices.
    private CPData _upnpControlPointData = null;
    private UPnPNetworkTracker _upnpAgent = null;
    private UPnPControlPoint _upnpControlPoint = null;
    private HashSet<string> _knownUpnpDevices = new HashSet<string>();

    // Used for detecting and communicating with devices that are directly
    // connected to the system.
    private ManagementEventWatcher _systemDeviceChangeEventWatcher = null;
    private DateTime _previousSystemDeviceChange = DateTime.MinValue;
    private object _lockObject = new object();
    private HashSet<string> _knownBdaWdmDevices = new HashSet<string>();
    private RadioWebStreamCard _rwsTuner = new RadioWebStreamCard();    // Always one RWS "tuner".

    // The listener that we notify when device events occur.
    private IDeviceEventListener _deviceEventListener = null;

    // network providers
    private IBaseFilter _atscNp = null;
    private IBaseFilter _dvbcNp = null;
    private IBaseFilter _dvbsNp = null;
    private IBaseFilter _dvbtNp = null;
    private IBaseFilter _mpNp = null;

    private IFilterGraph2 _graphBuilder = null;
    private DsROTEntry _rotEntry = null;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="listener">A listener that wishes to be notified about device events.</param>
    public DeviceDetector(IDeviceEventListener listener)
    {
      _deviceEventListener = listener;

      // Setup UPnP device detection.
      UPnPConfiguration.LOGGER = new Tve3Logger();
      _upnpControlPointData = new CPData();
      _upnpAgent = new UPnPNetworkTracker(_upnpControlPointData);
      _upnpAgent.RootDeviceAdded += OnUpnpRootDeviceAdded;
      _upnpAgent.RootDeviceRemoved += OnUpnpRootDeviceRemoved;
      _upnpControlPoint = new UPnPControlPoint(_upnpAgent);

      // Setup other (BDA, PBDA, WDM) device detection.
      try
      {
        InitBdaDetectionGraph();
      }
      catch (Exception ex)
      {
        Log.Log.Error("Failed to initialise the BDA device detection graph!\r\n{0}", ex);
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
      Log.Log.Info("Starting async device detection...");
      // Start detecting BDA and WDM devices.
      _deviceEventListener.OnDeviceAdded(_rwsTuner);
      DetectBdaWdmDevices();
      _systemDeviceChangeEventWatcher.Start();

      // Start detecting UPnP devices.
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
      _knownUpnpDevices.Clear();
      _knownBdaWdmDevices.Clear();
      Start();
    }

    public void Stop()
    {
      Log.Log.Info("Stopping async device detection...");
      _upnpAgent.Close();
      _upnpControlPoint.Close();
      _systemDeviceChangeEventWatcher.Stop();
    }

    #region IDisposable member

    /// <summary>
    /// Clean up, dispose, release.
    /// </summary>
    public void Dispose()
    {
      Stop();

      if (_rwsTuner != null)
      {
        _rwsTuner.Dispose();
        _rwsTuner = null;
      }
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      if (_graphBuilder != null)
      {
        FilterGraphTools.RemoveAllFilters(_graphBuilder);
        Release.ComObject("device detection graph builder", _graphBuilder);
      }
      Release.ComObject("device detection ATSC network provider", _atscNp);
      Release.ComObject("device detection DVB-C network provider", _dvbcNp);
      Release.ComObject("device detection DVB-S network provider", _dvbsNp);
      Release.ComObject("device detection DVB-T network provider", _dvbtNp);
      Release.ComObject("device detection MediaPortal network provider", _mpNp);
    }

    #endregion

    #region BDA/WDM device detection

    private static bool ConnectFilter(IFilterGraph2 graphBuilder, IBaseFilter networkFilter, IBaseFilter tunerFilter)
    {
      IPin pinOut = DsFindPin.ByDirection(networkFilter, PinDirection.Output, 0);
      IPin pinIn = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      int hr = graphBuilder.Connect(pinOut, pinIn);
      Release.ComObject(pinOut);
      Release.ComObject(pinIn);
      return (hr == 0);
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
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("delayCardDetect", "0");
      int delayDetect = Convert.ToInt32(setting.Value);
      if (delayDetect >= 1)
      {
        Thread.Sleep(delayDetect * 1000);
      }
      DetectBdaWdmDevices();
    }

    private void DetectBdaWdmDevices()
    {
      lock (_lockObject)
      {
        Log.Log.Debug("Detecting BDA/WDM devices...");

        HashSet<string> previouslyKnownDevices = _knownBdaWdmDevices;
        _knownBdaWdmDevices = new HashSet<string>();
        _knownBdaWdmDevices.Add(_rwsTuner.DevicePath);

        // Detect TechniSat SkyStar/AirStar/CableStar 2 & IP streaming devices.
        DetectSupportedLegacyAmFilterDevices(ref previouslyKnownDevices, ref _knownBdaWdmDevices);

        // Detect capture devices. Currently only the Hauppauge HD PVR & Colossus.
        DetectSupportedAmKsCrossbarDevices(ref previouslyKnownDevices, ref _knownBdaWdmDevices);

        // Detect analog tuners.
        DetectSupportedAmKsTvTunerDevices(ref previouslyKnownDevices, ref _knownBdaWdmDevices);

        // Detect digital BDA tuners.
        DetectSupportedBdaSourceDevices(ref previouslyKnownDevices, ref _knownBdaWdmDevices);

        // Remove the devices that are no longer connected.
        foreach (string previouslyKnownDevice in previouslyKnownDevices)
        {
          if (!_knownBdaWdmDevices.Contains(previouslyKnownDevice))
          {
            Log.Log.Info("BDA/WDM device {0} removed", previouslyKnownDevice);
            _deviceEventListener.OnDeviceRemoved(previouslyKnownDevice);
          }
        }
      }
    }

    private void InitBdaDetectionGraph()
    {
      Log.Log.Debug("Initialise BDA device detection graph");
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graphBuilder);

      Guid mpNetworkProviderClsId = new Guid("{D7D42E5C-EB36-4aad-933B-B4C419429C98}");
      if (FilterGraphTools.IsThisComObjectInstalled(mpNetworkProviderClsId))
      {
        _mpNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, mpNetworkProviderClsId, "MediaPortal Network Provider");
        return;
      }

      ITuningSpace tuningSpace = null;
      ILocator locator = null;

      // ATSC
      _atscNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(ATSCNetworkProvider).GUID, "ATSC Network Provider");
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
      _dvbcNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(DVBCNetworkProvider).GUID, "DVB-C Network Provider");
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
      _dvbsNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(DVBSNetworkProvider).GUID, "DVB-S Network Provider");
      tuningSpace = (ITuningSpace)new DVBSTuningSpace();
      tuningSpace.put_UniqueName("DVB-S TuningSpace");
      tuningSpace.put_FriendlyName("DVB-S TuningSpace");
      tuningSpace.put__NetworkType(typeof(DVBSNetworkProvider).GUID);
      ((IDVBSTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Satellite);
      locator = (ILocator)new DVBTLocator();
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
      _dvbtNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(DVBTNetworkProvider).GUID, "DVB-T Network Provider");
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

    private void DetectSupportedLegacyAmFilterDevices(ref HashSet<string> previouslyKnownDevices, ref HashSet<string> knownDevices)
    {
      Log.Log.Debug("Detect legacy AM filter devices");
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("iptvCardCount", "1");
      int iptvTunerCount = Convert.ToInt32(setting.Value);

      DsDevice[] connectedDevices = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      foreach (DsDevice connectedDevice in connectedDevices)
      {
        string name = connectedDevice.Name;
        string devicePath = connectedDevice.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          continue;
        }

        if (name.Equals("B2C2 MPEG-2 Source"))
        {
          knownDevices.Add(devicePath);
          if (!previouslyKnownDevices.Contains(devicePath))
          {
            Log.Log.Info("Detected new TechniSat *Star 2 tuner root device");
            TvCardDvbSS2 tuner = new TvCardDvbSS2(connectedDevice);
            _deviceEventListener.OnDeviceAdded(tuner);
          }
        }
        else if (name.Equals("Elecard NWSource-Plus"))
        {
          for (int i = 0; i < iptvTunerCount; i++)
          {
            TvCardDVBIP iptvTuner = new TvCardDVBIPElecard(connectedDevice, i);
            knownDevices.Add(iptvTuner.DevicePath);
            if (!previouslyKnownDevices.Contains(iptvTuner.DevicePath))
            {
              Log.Log.Info("Detected new Elecard IPTV tuner {0} {1}", iptvTuner.Name, iptvTuner.DevicePath);
              _deviceEventListener.OnDeviceAdded(iptvTuner);
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
            TvCardDVBIP iptvTuner = new TvCardDVBIPBuiltIn(connectedDevice, i);
            knownDevices.Add(iptvTuner.DevicePath);
            if (!previouslyKnownDevices.Contains(iptvTuner.DevicePath))
            {
              Log.Log.Info("Detected new MediaPortal IPTV tuner {0} {1}", iptvTuner.Name, iptvTuner.DevicePath);
              _deviceEventListener.OnDeviceAdded(iptvTuner);
            }
            else
            {
              iptvTuner.Dispose();
            }
          }
        }
      }
    }

    private void DetectSupportedAmKsCrossbarDevices(ref HashSet<string> previouslyKnownDevices, ref HashSet<string> knownDevices)
    {
      Log.Log.Debug("Detect AM KS crossbar devices");
      DsDevice[] connectedDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
      foreach (DsDevice connectedDevice in connectedDevices)
      {
        string name = connectedDevice.Name;
        string devicePath = connectedDevice.DevicePath;
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(devicePath) &&
          (name.Equals("Hauppauge HD PVR Crossbar") || name.Contains("Hauppauge Colossus Crossbar"))
        )
        {
          knownDevices.Add(devicePath);
          if (!previouslyKnownDevices.Contains(devicePath))
          {
            Log.Log.Info("Detected new Hauppauge capture device {0} {1)", name, devicePath);
            TvCardHDPVR captureDevice = new TvCardHDPVR(connectedDevice);
            _deviceEventListener.OnDeviceAdded(captureDevice);
          }
        }
      }
    }

    private void DetectSupportedAmKsTvTunerDevices(ref HashSet<string> previouslyKnownDevices, ref HashSet<string> knownDevices)
    {
      Log.Log.Debug("Detect AM KS TV tuner devices");
      DsDevice[] connectedDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSTVTuner);
      foreach (DsDevice connectedDevice in connectedDevices)
      {
        string name = connectedDevice.Name;
        string devicePath = connectedDevice.DevicePath;
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(devicePath))
        {
          knownDevices.Add(devicePath);
          if (!previouslyKnownDevices.Contains(devicePath))
          {
            Log.Log.Info("Detected new analog tuner device {0} {1}", name, devicePath);
            TvCardAnalog analogTuner = new TvCardAnalog(connectedDevice);
            _deviceEventListener.OnDeviceAdded(analogTuner);
          }
        }
      }
    }

    private void DetectSupportedBdaSourceDevices(ref HashSet<string> previouslyKnownDevices, ref HashSet<string> knownDevices)
    {
      Log.Log.Debug("Detect BDA source devices");

      // MS generic, MCE 2005 roll-up 2 or better
      bool isMsGenericNpAvailable = FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID);

      DsDevice[] connectedDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice connectedDevice in connectedDevices)
      {
        string name = connectedDevice.Name;
        string devicePath = connectedDevice.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          continue;
        }
        if (previouslyKnownDevices.Contains(devicePath))
        {
          knownDevices.Add(devicePath);
          continue;
        }

        // North American CableCARD tuners [PBDA].
        if (name.StartsWith("HDHomeRun Prime") || name.StartsWith("Ceton InfiniTV"))
        {
          Log.Log.Info("Detected new PBDA CableCARD tuner device {0} {1}", name, devicePath);
          TunerPbdaCableCard cableCardTuner = new TunerPbdaCableCard(connectedDevice);
          knownDevices.Add(devicePath);
          _deviceEventListener.OnDeviceAdded(cableCardTuner);
          continue;
        }

        IBaseFilter tmpDeviceFilter;
        try
        {
          _graphBuilder.AddSourceFilterForMoniker(connectedDevice.Mon, null, name, out tmpDeviceFilter);
        }
        catch (Exception ex)
        {
          Log.Log.Error("Failed to add filter to detect device type for {0}!\r\n{1}", name, ex);
          continue;
        }

        try
        {
          // Silicondust regular (non-CableCARD) HDHomeRun. Workaround for tuner type
          // detection issue. The MS generic provider would always detect DVB-T.
          bool isCablePreferred = false;
          if (name.StartsWith("Silicondust HDHomeRun Tuner"))
          {
            isCablePreferred = GetHdHomeRunSourceType(name).Equals("Digital Cable");
          }

          Log.Log.Info("Detected new digital BDA tuner device {0} {1}", name, devicePath);

          // Try the MediaPortal network provider first.
          ITVCard deviceToAdd = null;
          if (_mpNp != null)
          {
            Log.Log.Debug("  check type with MP NP");
            IDvbNetworkProvider interfaceNetworkProvider = (IDvbNetworkProvider)_mpNp;
            string hash = GetHash(devicePath);
            interfaceNetworkProvider.ConfigureLogging(GetFileName(devicePath), hash, LogLevelOption.Debug);
            if (ConnectFilter(_graphBuilder, _mpNp, tmpDeviceFilter))
            {
              TuningType tuningTypes;
              interfaceNetworkProvider.GetAvailableTuningTypes(out tuningTypes);
              Log.Log.Debug("  tuning types = {0}, hash = {1}", tuningTypes, hash);
              if ((tuningTypes & TuningType.DvbT) != 0 && !isCablePreferred)
              {
                deviceToAdd = new TvCardDVBT(connectedDevice);
              }
              else if ((tuningTypes & TuningType.DvbS) != 0 && !isCablePreferred)
              {
                deviceToAdd = new TvCardDVBS(connectedDevice);
              }
              else if ((tuningTypes & TuningType.DvbC) != 0)
              {
                deviceToAdd = new TvCardDVBC(connectedDevice);
              }
              else if ((tuningTypes & TuningType.Atsc) != 0)
              {
                deviceToAdd = new TvCardATSC(connectedDevice);
              }
              else
              {
                Log.Log.Debug("  connected to MP NP but type not recognised");
              }
            }
            else
            {
              Log.Log.Debug("  failed to connect to MP NP");
            }
          }
          // Try the Microsoft network provider next if the MP NP
          // failed and the MS generic NP is available.
          if (deviceToAdd == null && isMsGenericNpAvailable)
          {
            // Note: the MS NP must be added/removed to/from the graph for each
            // device that is checked. If you don't do this, the networkTypes
            // list gets longer and longer and longer.
            Log.Log.Debug("  check type with MS NP");
            IBaseFilter genericNp = null;
            try
            {
              genericNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(NetworkProvider).GUID, "Microsoft Network Provider");
            }
            catch
            {
              genericNp = null;
            }
            if (genericNp == null)
            {
              Log.Log.Error(" failed to add MS NP to graph");
            }
            else
            {
              if (ConnectFilter(_graphBuilder, genericNp, tmpDeviceFilter))
              {
                int networkTypesMax = 5;
                int networkTypeCount;
                Guid[] networkTypes = new Guid[networkTypesMax];
                int hr = (genericNp as ITunerCap).get_SupportedNetworkTypes(networkTypesMax, out networkTypeCount, networkTypes);
                Log.Log.Debug("  network type count = {0}", networkTypeCount);
                for (int n = 0; n < networkTypeCount; n++)
                {
                  Log.Log.Debug("  network type {0} = {1}", n, networkTypes[n]);
                }
                for (int n = 0; n < networkTypeCount; n++)
                {
                  if (networkTypes[n] == typeof(DVBTNetworkProvider).GUID && !isCablePreferred)
                  {
                    deviceToAdd = new TvCardDVBT(connectedDevice);
                  }
                  else if (networkTypes[n] == typeof(DVBSNetworkProvider).GUID && !isCablePreferred)
                  {
                    deviceToAdd = new TvCardDVBS(connectedDevice);
                  }
                  else if (networkTypes[n] == typeof(DVBCNetworkProvider).GUID)
                  {
                    deviceToAdd = new TvCardDVBC(connectedDevice);
                  }
                  else if (networkTypes[n] == typeof(ATSCNetworkProvider).GUID)
                  {
                    deviceToAdd = new TvCardATSC(connectedDevice);
                  }
                  if (deviceToAdd != null)
                  {
                    break;
                  }
                  else if (n == (networkTypeCount - 1))
                  {
                    Log.Log.Debug(" connected to MS NP but type not recognised");
                  }
                }
              }
              else
              {
                Log.Log.Debug("  failed to connect to MS NP");
              }

              _graphBuilder.RemoveFilter(genericNp);
              Release.ComObject("device detection generic network provider", genericNp);
              genericNp = null;
            }
          }
          // Last shot is the old style Microsoft network providers.
          if (deviceToAdd == null)
          {
            Log.Log.Debug("  check type with specific NPs");
            if (ConnectFilter(_graphBuilder, _dvbtNp, tmpDeviceFilter))
            {
              deviceToAdd = new TvCardDVBT(connectedDevice);
            }
            else if (ConnectFilter(_graphBuilder, _dvbcNp, tmpDeviceFilter))
            {
              deviceToAdd = new TvCardDVBC(connectedDevice);
            }
            else if (ConnectFilter(_graphBuilder, _dvbsNp, tmpDeviceFilter))
            {
              deviceToAdd = new TvCardDVBS(connectedDevice);
            }
            else if (ConnectFilter(_graphBuilder, _atscNp, tmpDeviceFilter))
            {
              deviceToAdd = new TvCardATSC(connectedDevice);
            }
            else
            {
              Log.Log.Debug("  failed to connect to specific NP");
            }
          }

          if (deviceToAdd != null)
          {
            Log.Log.Info("  tuner type = {0}", deviceToAdd.CardType);
            knownDevices.Add(devicePath);
            _deviceEventListener.OnDeviceAdded(deviceToAdd);
          }
        }
        finally
        {
          _graphBuilder.RemoveFilter(tmpDeviceFilter);
          Release.ComObject("device detection device filter", tmpDeviceFilter);
        }
      }
    }

    #endregion

    #region MP network provider only (contact MisterD)

    /// <summary>
    /// Generates the file and pathname of the log file
    /// </summary>
    /// <param name="devicePath">Device Path of the card</param>
    /// <returns>Complete filename of the configuration file</returns>
    public static string GetFileName(string devicePath)
    {
      string hash = GetHash(devicePath);
      string pathName = PathManager.GetDataPath;
      string fileName = string.Format(@"{0}\Log\NetworkProvider-{1}.log", pathName, hash);
      Log.Log.Debug("NetworkProvider logfilename: " + fileName);
      Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      return fileName;
    }

    public static string GetHash(string value)
    {
      byte[] data = Encoding.ASCII.GetBytes(value);
      byte[] hashData = new SHA1Managed().ComputeHash(data);

      string hash = string.Empty;

      foreach (byte b in hashData)
      {
        hash += b.ToString("X2");
      }
      return hash;
    }

    #endregion

    #region UPnP device detection

    private void OnUpnpRootDeviceAdded(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor == null || rootDescriptor.State != RootDescriptorState.Ready || _knownUpnpDevices.Contains(rootDescriptor.SSDPRootEntry.RootDeviceUUID))
      {
        return;
      }

      _knownUpnpDevices.Add(rootDescriptor.SSDPRootEntry.RootDeviceUUID);
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
              Log.Log.Info("Detected new OCUR/DRI device {0}", deviceDescriptor.FriendlyName);
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
            Log.Log.Info("  add {0} {1}", childDeviceDescriptorEn.Current.FriendlyName, childDeviceDescriptorEn.Current.DeviceUDN);
            _deviceEventListener.OnDeviceAdded(new TunerDri(childDeviceDescriptorEn.Current, _upnpControlPoint));
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

      _knownUpnpDevices.Remove(rootDescriptor.SSDPRootEntry.RootDeviceUUID);
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
              Log.Log.Info("UPnP device {0} removed", deviceDescriptor.FriendlyName);
            }

            IEnumerator<DeviceDescriptor> childDeviceDescriptorEn = deviceDescriptor.ChildDevices.GetEnumerator();
            while (childDeviceDescriptorEn.MoveNext())
            {
              if (childDeviceDescriptorEn.Current.DeviceUUID == childDeviceEn.Current.UUID)
              {
                break;
              }
            }
            Log.Log.Info("  remove {0} {1}", childDeviceDescriptorEn.Current.FriendlyName, childDeviceDescriptorEn.Current.DeviceUDN);
            _deviceEventListener.OnDeviceRemoved(childDeviceDescriptorEn.Current.DeviceUDN);
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
        Log.Log.Error("Failed to check HDHomeRun preferred mode.\r\n{0}", ex);
      }
      return "Digital Antenna";
    }

    #endregion
  }
}