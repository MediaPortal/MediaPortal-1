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
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Pbda;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Implementations.Scte;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using IBdaTuner = DirectShowLib.BDA.ITuner;
using ITveTuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;
using TveAnalogVideoStandard = Mediaportal.TV.Server.Common.Types.Enum.AnalogVideoStandard;
using WdmAnalogVideoStandard = DirectShowLib.AnalogVideoStandard;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects BDA and PBDA tuners.
  /// </summary>
  internal class TunerDetectorBda : ITunerDetectorSystem, IDisposable
  {
    [Flags]
    private enum TunerType
    {
      Unknown = 0,
      AnalogTv = 1,
      AuxiliaryInput = 2,
      FmRadio = 4,
      Ofdm = 8,
      Psk = 16,
      Psk8 = 32,
      Qam = 64,
      QamDvb = 128,
      Vsb = 256
    }

    #region variables

    private bool _isMicrosoftGenericNpAvailable = false;
    private IBaseFilter _atscNp = null;
    private IBaseFilter _dvbcNp = null;
    private IBaseFilter _dvbsNp = null;
    private IBaseFilter _dvbtNp = null;

    private IFilterGraph2 _graph = null;
    private DsROTEntry _rotEntry = null;

    private IDictionary<string, ICollection<ITveTuner>> _knownTuners = new Dictionary<string, ICollection<ITveTuner>>();    // key = device path

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDetectorBda"/> class.
    /// </summary>
    public TunerDetectorBda()
    {
      try
      {
        InitDirectShowGraph();
      }
      catch (Exception ex)
      {
        this.LogCritical(ex, "BDA detector: failed to initialise the DirectShow graph");
        throw;
      }
    }

    ~TunerDetectorBda()
    {
      Dispose(false);
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        _knownTuners.Clear();
        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
          _rotEntry = null;
        }
        if (_graph != null)
        {
          _graph.RemoveFilter(_atscNp);
          _graph.RemoveFilter(_dvbcNp);
          _graph.RemoveFilter(_dvbsNp);
          _graph.RemoveFilter(_dvbtNp);
          Release.ComObject("BDA detector graph", ref _graph);
        }
        Release.ComObject("BDA detector ATSC network provider", ref _atscNp);
        Release.ComObject("BDA detector DVB-C network provider", ref _dvbcNp);
        Release.ComObject("BDA detector DVB-S network provider", ref _dvbsNp);
        Release.ComObject("BDA detector DVB-T network provider", ref _dvbtNp);
      }
    }

    #endregion

    #region ITunerDetectorSystem members

    /// <summary>
    /// Get the detector's name.
    /// </summary>
    public string Name
    {
      get
      {
        return "BDA";
      }
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners exposed by a system device
    /// interface.
    /// </summary>
    /// <param name="classGuid">The identifier for the interface's class.</param>
    /// <param name="devicePath">The interface's device path.</param>
    /// <returns>the compatible tuners exposed by the interface</returns>
    public ICollection<ITveTuner> DetectTuners(Guid classGuid, string devicePath)
    {
      ICollection<ITveTuner> tuners = new List<ITveTuner>(0);

      // Is the interface device a BDA source?
      if (classGuid != FilterCategory.BDASourceFiltersCategory || string.IsNullOrEmpty(devicePath))
      {
        return tuners;
      }
      this.LogInfo("BDA detector: tuner added");

      // Detect the tuners associated with the device interface.
      DsDevice device = DsDevice.FromDevicePath(devicePath);
      if (device == null)
      {
        return tuners;
      }
      tuners = DetectTunersForDevice(device);
      if (tuners == null || tuners.Count == 0)
      {
        device.Dispose();
        return tuners;
      }

      _knownTuners[devicePath] = tuners;
      return tuners;
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<ITveTuner> DetectTuners()
    {
      this.LogDebug("BDA detector: detect tuners");
      List<ITveTuner> tuners = new List<ITveTuner>();
      IDictionary<string, ICollection<ITveTuner>> knownTuners = new Dictionary<string, ICollection<ITveTuner>>();

      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice device in devices)
      {
        // Is this a new device?
        string devicePath = device.DevicePath;
        ICollection<ITveTuner> deviceTuners = null;
        if (!string.IsNullOrEmpty(devicePath) && _knownTuners.TryGetValue(devicePath, out deviceTuners))
        {
          // No. Reuse the tuner instances we've previously created.
          device.Dispose();
          tuners.AddRange(deviceTuners);
          knownTuners.Add(devicePath, deviceTuners);
          continue;
        }

        deviceTuners = DetectTunersForDevice(device);
        if (deviceTuners == null || deviceTuners.Count == 0)
        {
          device.Dispose();
          continue;
        }

        tuners.AddRange(deviceTuners);
        knownTuners.Add(devicePath, deviceTuners);
      }

      _knownTuners = knownTuners;
      return tuners;
    }

    #endregion

    #region private members

    private void InitDirectShowGraph()
    {
      this.LogDebug("BDA detector: initialise DirectShow graph");
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
        ((IBdaTuner)_atscNp).put_TuningSpace(tuningSpace);

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
        ((IBdaTuner)_dvbcNp).put_TuningSpace(tuningSpace);

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
        ((IBdaTuner)_dvbsNp).put_TuningSpace(tuningSpace);

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
        ((IBdaTuner)_dvbtNp).put_TuningSpace(tuningSpace);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "BDA detector: failed to add and configure specific Microsoft network provider(s), is BDA installed?");
      }
    }

    private static bool ConnectFilter(IFilterGraph2 graph, IBaseFilter networkProviderFilter, IBaseFilter tunerFilter)
    {
      try
      {
        FilterGraphTools.ConnectFilters(graph, networkProviderFilter, 0, tunerFilter, 0);
        return true;
      }
      catch
      {
        return false;
      }
    }

    private ICollection<ITveTuner> DetectTunersForDevice(DsDevice device)
    {
      ICollection<ITveTuner> tuners = new List<ITveTuner>(4);
      string name = device.Name;
      string devicePath = device.DevicePath;
      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
      {
        return tuners;
      }

      this.LogDebug("BDA detector: tuner, name = {0}, device path = {1}", name, devicePath);
      IBaseFilter tunerFilter;
      try
      {
        tunerFilter = FilterGraphTools.AddFilterFromDevice(_graph, device);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "BDA detector: failed to add BDA source filter to determine tuner type, name = {0}, device path = {1}", name, devicePath);
        return tuners;
      }

      TunerType tunerTypes = TunerType.Unknown;
      try
      {
        // We shouldn't need network providers to detect the tuner type.
        tunerTypes = DetectSourceTypeInternal(tunerFilter);

        // If we couldn't detect the type with the topology info, try network
        // providers. Try the Microsoft network provider first if available.
        if (_isMicrosoftGenericNpAvailable && tunerTypes == TunerType.Unknown)
        {
          tunerTypes = DetectSourceTypeMicrosoftGenericNetworkProvider(tunerFilter, _graph);
        }

        // Last shot is the old style Microsoft network providers.
        if (tunerTypes == TunerType.Unknown)
        {
          tunerTypes = DetectSourceTypeMicrosoftSpecificNetworkProvider(tunerFilter);
        }
        if (tunerTypes == TunerType.Unknown)
        {
          this.LogError("BDA detector: failed to determine tuner type, name = {0}, device path = {1}", name, devicePath);
          return tuners;
        }

        if (tunerTypes.HasFlag(TunerType.Vsb))
        {
          BroadcastStandard supportedBroadcastStandards = BroadcastStandard.Atsc;
          if (tunerTypes.HasFlag(TunerType.Qam))
          {
            supportedBroadcastStandards |= BroadcastStandard.Scte;
          }

          tuners.Add(new TunerBdaAtsc(device, supportedBroadcastStandards));
        }
        else if (tunerTypes.HasFlag(TunerType.Qam))
        {
          string countryName = System.Globalization.RegionInfo.CurrentRegion.EnglishName;
          if (countryName != null && (countryName.Equals("United States") || countryName.Equals("Canada")))
          {
            if (tunerTypes.HasFlag(TunerType.QamDvb))
            {
              tuners.Add(new TunerScteWrapper(new TunerBdaQam(device)));
            }
            else if (tunerFilter is IBDA_ConditionalAccess)
            {
              tuners.Add(new TunerPbdaCableCard(device));

              // Normally analog TV and auxiliary input support in BDA is
              // implemented using a WDM analog filter graph behind the scenes.
              // Using the BDA layer gives less flexibility and control.
              // Therefore the native WDM implementation is normally preferred.
              // CableCARD tuners are a special case. The PBDA proxy filter
              // implements support internally by utilising the underlying DRI
              // interface. There is no WDM interface available.
              if (tunerTypes.HasFlag(TunerType.AnalogTv) || tunerTypes.HasFlag(TunerType.AuxiliaryInput))
              {
                BroadcastStandard broadcastStandards;
                CaptureSourceVideo videoSources;
                DetectAuxiliaryInputDetails(tunerFilter, out broadcastStandards, out videoSources);
                if (videoSources != CaptureSourceVideo.None)
                {
                  tuners.Add(new TunerBdaAuxiliaryInput(device, videoSources));
                }
              }
            }
            else
            {
              tuners.Add(new TunerBdaAtsc(device, BroadcastStandard.Scte));
            }
          }
          else
          {
            tuners.Add(new TunerBdaQam(device));
          }
        }

        if (tunerTypes.HasFlag(TunerType.Psk))
        {
          if (tunerTypes.HasFlag(TunerType.Psk8) || device.Name.ToLowerInvariant().Contains("s2"))
          {
            tuners.Add(new TunerBdaPsk(device, BroadcastStandard.DvbS | BroadcastStandard.DvbS2));
          }
          else
          {
            tuners.Add(new TunerBdaPsk(device, BroadcastStandard.DvbS));
          }
        }
        if (tunerTypes.HasFlag(TunerType.Ofdm))
        {
          if (device.Name.ToLowerInvariant().Contains("t2"))
          {
            tuners.Add(new TunerBdaOfdm(device, BroadcastStandard.DvbT | BroadcastStandard.DvbT2));
          }
          else
          {
            tuners.Add(new TunerBdaOfdm(device, BroadcastStandard.DvbT));
          }
        }

        return tuners;
      }
      finally
      {
        _graph.RemoveFilter(tunerFilter);
        Release.ComObject("BDA detector source filter", ref tunerFilter);
      }
    }

    private static TunerType DetectSourceTypeInternal(IBaseFilter filter)
    {
      Log.Debug("  check type with topology node descriptors");
      IBDA_Topology topology = filter as IBDA_Topology;
      if (topology == null)
      {
        Log.Debug("  filter is not a BDA topology");
        return TunerType.Unknown;
      }

      BDANodeDescriptor[] descriptors = new BDANodeDescriptor[20];
      int descriptorCount = 0;
      int hr = topology.GetNodeDescriptors(out descriptorCount, descriptors.Length, descriptors);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        Log.Error("BDA detector: failed to get node descriptors from topology, hr = 0x{0:x}", hr);
        return TunerType.Unknown;
      }

      Log.Debug("  descriptor count = {0}", descriptorCount);
      TunerType tunerTypes = TunerType.Unknown;
      for (int d = 0; d < descriptorCount; d++)
      {
        Guid function = descriptors[d].guidFunction;
        Log.Debug("    function {0} = {1}", d, function);
        if (function == BDANodeCategory.ANALOG_DEMODULATOR)
        {
          tunerTypes |= TunerType.AnalogTv;
        }
        else if (function == BDANodeCategory.VSB_8_DEMODULATOR)
        {
          tunerTypes |= TunerType.Vsb;
        }
        else if (function == BDANodeCategory.QAM_DEMODULATOR)
        {
          tunerTypes |= TunerType.Qam;
        }
        else if (function == BDANodeCategory.COFDM_DEMODULATOR ||
          function == BDANodeCategory.ISDB_T_DEMODULATOR)
        {
          tunerTypes |= TunerType.Ofdm;
        }
        else if (function == BDANodeCategory.QPSK_DEMODULATOR)
        {
          tunerTypes |= TunerType.Psk;
        }
        else if (function == BDANodeCategory.PSK_8_DEMODULATOR ||
          function == BDANodeCategory.ISDB_S_DEMODULATOR)
        {
          tunerTypes |= TunerType.Psk | TunerType.Psk8;
        }
      }

      if (tunerTypes == TunerType.Unknown)
      {
        Log.Debug("  traversed topology but type not recognised");
      }
      return tunerTypes;
    }

    private static TunerType DetectSourceTypeMicrosoftGenericNetworkProvider(IBaseFilter filter, IFilterGraph2 graph)
    {
      Log.Debug("  check type with Microsoft generic network provider");
      IBaseFilter genericNp = null;
      try
      {
        genericNp = FilterGraphTools.AddFilterFromRegisteredClsid(graph, typeof(NetworkProvider).GUID, "Microsoft Network Provider");
      }
      catch (Exception ex)
      {
        Log.Error(ex, "BDA detector: failed to add Microsoft generic network provider to detection graph");
        return TunerType.Unknown;
      }

      try
      {
        if (!ConnectFilter(graph, genericNp, filter))
        {
          Log.Debug("  failed to connect to Microsoft generic network provider");
          return TunerType.Unknown;
        }

        int networkTypeCount;
        Guid[] networkTypes = new Guid[10];
        int hr = (genericNp as ITunerCap).get_SupportedNetworkTypes(networkTypes.Length, out networkTypeCount, networkTypes);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          Log.Error("BDA detector: connected to Microsoft generic network provider but failed to get supported network types, hr = 0x{0:x}", hr);
          return TunerType.Unknown;
        }

        Log.Debug("  network type count = {0}", networkTypeCount);
        TunerType tunerTypes = TunerType.Unknown;
        for (int n = 0; n < networkTypeCount; n++)
        {
          Guid networkType = networkTypes[n];
          Log.Debug("    network type {0} = {1}", n, networkType);
          if (networkType == NetworkType.ANALOG_AUX_IN)
          {
            tunerTypes |= TunerType.AuxiliaryInput;
          }
          else if (networkType == NetworkType.ANALOG_FM)
          {
            tunerTypes |= TunerType.FmRadio;
          }
          else if (networkType == NetworkType.ANALOG_TV)
          {
            tunerTypes |= TunerType.AnalogTv;
          }
          else if (networkType == NetworkType.ATSC_TERRESTRIAL)
          {
            tunerTypes |= TunerType.Vsb;
          }
          else if (networkType == NetworkType.DIGITAL_CABLE)
          {
            // BDA hybrid ATSC/QAM, BDA DVB-C and PBDA CableCARD tuners all
            // advertise this network type. ATSC and DVB-C tuners usually
            // advertise additional types which allow us to identifiy them.
            // If we don't see those types, we assume this is a QAM or
            // CableCARD tuner.
            tunerTypes |= TunerType.Qam;
          }
          else if (networkType == NetworkType.DVB_CABLE ||
            networkType == NetworkType.ISDB_CABLE)
          {
            tunerTypes |= TunerType.Qam | TunerType.QamDvb;
          }
          else if (networkType == NetworkType.DVB_TERRESTRIAL ||
            networkType == NetworkType.ISDB_TERRESTRIAL ||
            networkType == NetworkType.ISDB_T)
          {
            tunerTypes |= TunerType.Ofdm;
          }
          else if (networkType == NetworkType.DVB_SATELLITE ||
            networkType == NetworkType.DIRECTV_SATELLITE ||
            networkType == NetworkType.ECHOSTAR_SATELLITE ||
            networkType == NetworkType.ISDB_SATELLITE ||
            networkType == NetworkType.ISDB_S)
          {
            tunerTypes |= TunerType.Psk;
          }
        }

        if (tunerTypes == TunerType.Unknown)
        {
          Log.Debug("  connected to Microsoft generic network provider but type not recognised");
        }
        return tunerTypes;
      }
      finally
      {
        graph.RemoveFilter(genericNp);
        Release.ComObject("BDA detector Microsoft generic network provider", ref genericNp);
      }
    }

    private TunerType DetectSourceTypeMicrosoftSpecificNetworkProvider(IBaseFilter filter)
    {
      this.LogDebug("  check type with specific Microsoft network providers");
      if (ConnectFilter(_graph, _dvbtNp, filter))
      {
        return TunerType.Ofdm;
      }
      else if (ConnectFilter(_graph, _dvbcNp, filter))
      {
        return TunerType.QamDvb;
      }
      else if (ConnectFilter(_graph, _dvbsNp, filter))
      {
        return TunerType.Psk;
      }
      else if (ConnectFilter(_graph, _atscNp, filter))
      {
        return TunerType.Vsb;
      }
      this.LogDebug("  failed to connect to specific Microsoft network provider");
      return TunerType.Unknown;
    }

    private static void DetectAuxiliaryInputDetails(IBaseFilter filter, out BroadcastStandard broadcastStandards, out CaptureSourceVideo videoSources)
    {
      broadcastStandards = BroadcastStandard.Unknown;
      videoSources = CaptureSourceVideo.None;

      Log.Debug("  check for extended auto-demodulate support");
      IBDA_Topology topology = filter as IBDA_Topology;
      if (topology == null)
      {
        Log.Debug("  filter is not a BDA topology");
        return;
      }
      int nodeTypeCount;
      int[] nodeTypes = new int[20];
      int hr = topology.GetNodeTypes(out nodeTypeCount, nodeTypes.Length, nodeTypes);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        Log.Error("BDA detector: failed to get node types from topology, hr = 0x{0:x}", hr);
        return;
      }

      Guid[] interfaces = new Guid[20];
      Log.Debug("  topology node type count = {0}", nodeTypeCount);
      for (int i = 0; i < nodeTypeCount; i++)
      {
        int nodeType = nodeTypes[i];

        int interfaceCount;
        hr = topology.GetNodeInterfaces(nodeType, out interfaceCount, interfaces.Length, interfaces);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          Log.Error("BDA detector: failed to get topology node type interfaces, node index = {0}, hr = 0x{1:x}", i, hr);
          continue;
        }

        for (int j = 0; j < interfaceCount; j++)
        {
          if (interfaces[j] != typeof(IBDA_DigitalDemodulator).GUID)
          {
            continue;
          }

          object controlNode;
          hr = topology.GetControlNode(0, 1, nodeType, out controlNode);
          if (hr != (int)NativeMethods.HResult.S_OK || controlNode == null)
          {
            Log.Error("BDA detector: failed to get topology node type control node, node index = {0}, hr = 0x{1:x}", i, hr);
            continue;
          }
          try
          {
            IBDA_AutoDemodulateEx autoDemodInterface = controlNode as IBDA_AutoDemodulateEx;
            if (autoDemodInterface == null)
            {
              continue;
            }

            Log.Debug("  found extended auto-demodulate interface on node {0}", i);
            int inputCountComposite;
            int inputCountSvideo;
            hr = autoDemodInterface.get_AuxInputCount(out inputCountComposite, out inputCountSvideo);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              Log.Error("BDA detector: failed to get auto-demodulator auxiliary input count, node index = {0}, hr = 0x{1:x}", i, hr);
            }

            AMTunerModeType tunerModes;
            WdmAnalogVideoStandard videoStandards;
            hr = autoDemodInterface.get_SupportedVideoFormats(out tunerModes, out videoStandards);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              Log.Error("BDA detector: failed to get auto-demodulator supported video formats, node index = {0}, hr = 0x{1:x}", i, hr);
            }

            Log.Debug("    s-video count   = {0}", inputCountSvideo);
            Log.Debug("    composite count = {0}", inputCountComposite);
            Log.Debug("    tuner modes     = [{0}]", tunerModes);
            Log.Debug("    video standards = [{0}]", videoStandards);

            if (tunerModes.HasFlag(AMTunerModeType.AMRadio))
            {
              broadcastStandards |= BroadcastStandard.AmRadio;
            }
            if (tunerModes.HasFlag(AMTunerModeType.FMRadio))
            {
              broadcastStandards |= BroadcastStandard.FmRadio;
            }
            if (tunerModes.HasFlag(AMTunerModeType.TV))
            {
              broadcastStandards |= BroadcastStandard.AnalogTelevision;
            }

            if (inputCountSvideo > 0)
            {
              broadcastStandards |= BroadcastStandard.ExternalInput;
              videoSources |= CaptureSourceVideo.Svideo1;
              if (inputCountSvideo > 1)
              {
                videoSources |= CaptureSourceVideo.Svideo2;
                if (inputCountSvideo > 2)
                {
                  videoSources |= CaptureSourceVideo.Svideo3;
                  if (inputCountSvideo > 3)
                  {
                    Log.Warn("BDA detector: {0} s-video video inputs detected, only 3 supported", inputCountSvideo);
                  }
                }
              }
            }
            if (inputCountComposite > 0)
            {
              broadcastStandards |= BroadcastStandard.ExternalInput;
              videoSources |= CaptureSourceVideo.Composite1;
              if (inputCountComposite > 1)
              {
                videoSources |= CaptureSourceVideo.Composite2;
                if (inputCountComposite > 2)
                {
                  videoSources |= CaptureSourceVideo.Composite3;
                  if (inputCountComposite > 3)
                  {
                    Log.Warn("BDA detector: {0} composite video inputs detected, only 3 supported", inputCountComposite);
                  }
                }
              }
            }

            return;
          }
          finally
          {
            Release.ComObject(string.Format("BDA detector control node {0}", i), ref controlNode);
          }
        }
      }
    }

    #endregion
  }
}