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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using IBdaTuner = DirectShowLib.BDA.ITuner;
using IMpTuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// An implementation of <see cref="ITunerDetectorSystem"/> which detects BDA and PBDA tuners.
  /// </summary>
  internal class TunerDetectorBda : ITunerDetectorSystem, IDisposable
  {
    #region variables

    private bool _isMicrosoftGenericNpAvailable = false;
    private IBaseFilter _atscNp = null;
    private IBaseFilter _dvbcNp = null;
    private IBaseFilter _dvbsNp = null;
    private IBaseFilter _dvbtNp = null;

    private IFilterGraph2 _graph = null;
    private DsROTEntry _rotEntry = null;

    // key = device path
    private IDictionary<string, ICollection<IMpTuner>> _knownTuners = new Dictionary<string, ICollection<IMpTuner>>();

    #endregion

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
    /// Detect and instanciate the compatible tuners connected to the system.
    /// </summary>
    /// <returns>the tuners that are currently available</returns>
    public ICollection<IMpTuner> DetectTuners()
    {
      this.LogDebug("BDA detector: detect tuners");
      List<IMpTuner> tuners = new List<IMpTuner>();
      IDictionary<string, ICollection<IMpTuner>> knownTuners = new Dictionary<string, ICollection<IMpTuner>>();

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

        // Is this a new device?
        ICollection<IMpTuner> deviceTuners;
        if (_knownTuners.TryGetValue(devicePath, out deviceTuners))
        {
          device.Dispose();
          tuners.AddRange(deviceTuners);
          knownTuners.Add(devicePath, deviceTuners);
          continue;
        }
        deviceTuners = new List<IMpTuner>(4);

        this.LogDebug("BDA detector: tuner, name = {0}, device path = {1}", name, devicePath);
        IBaseFilter tunerFilter;
        try
        {
          tunerFilter = FilterGraphTools.AddFilterFromDevice(_graph, device);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "BDA detector: failed to add BDA source filter to determine tuner type, name = {0}, device path = {1}", name, devicePath);
          device.Dispose();
          continue;
        }

        bool isVsbSupported = false;
        bool isQamSupported = false;
        bool isQamDvbSupported = false;
        bool isPskSupported = false;
        bool isPsk8Supported = false;
        bool isOfdmSupported = false;
        try
        {
          // We shouldn't need network providers to detect the tuner type.
          this.LogDebug("  check type with topology node descriptors");
          DetectSourceTypeInternal(tunerFilter, out isVsbSupported, out isQamSupported, out isPskSupported, out isPsk8Supported, out isOfdmSupported);

          // If we couldn't detect the type with the topology info, try network
          // providers. Try the Microsoft network provider first if available.
          if (!isVsbSupported && !isQamSupported && !isPskSupported && !isOfdmSupported && _isMicrosoftGenericNpAvailable)
          {
            this.LogDebug("  check type with Microsoft generic network provider");
            DetectSourceTypeMicrosoftGenericNetworkProvider(tunerFilter, out isVsbSupported, out isQamSupported, out isQamDvbSupported, out isPskSupported, out isOfdmSupported);
          }
          // Last shot is the old style Microsoft network providers.
          if (!isVsbSupported && !isQamSupported && !isPskSupported && !isOfdmSupported)
          {
            this.LogDebug("  check type with specific Microsoft network providers");
            DetectSourceTypeMicrosoftSpecificNetworkProvider(tunerFilter, out isVsbSupported, out isQamSupported, out isPskSupported, out isOfdmSupported);
          }
          if (!isVsbSupported && !isQamSupported && !isPskSupported && !isOfdmSupported)
          {
            this.LogError("BDA detector: failed to determine tuner type, name = {0}, device path = {1}", name, devicePath);
            device.Dispose();
            continue;
          }

          if (isVsbSupported)
          {
            BroadcastStandard supportedBroadcastStandards = BroadcastStandard.Atsc;
            if (isQamSupported)
            {
              supportedBroadcastStandards |= BroadcastStandard.Scte;
            }

            deviceTuners.Add(new TunerBdaAtsc(device, supportedBroadcastStandards));
          }
          else if (isQamSupported)
          {
            if (isQamDvbSupported)
            {
              deviceTuners.Add(new TunerBdaQam(device));
            }
            else if (tunerFilter is IBDA_ConditionalAccess)
            {
              deviceTuners.Add(new TunerPbdaCableCard(device));
            }
            else
            {
              string countryName = System.Globalization.RegionInfo.CurrentRegion.EnglishName;
              if (countryName != null && (countryName.Equals("United States") || countryName.Equals("Canada")))
              {
                deviceTuners.Add(new TunerBdaAtsc(device, BroadcastStandard.Scte));
              }
              else
              {
                deviceTuners.Add(new TunerBdaQam(device));
              }
            }
          }

          if (isPskSupported)
          {
            if (isPsk8Supported || device.Name.ToLowerInvariant().Contains("s2"))
            {
              deviceTuners.Add(new TunerBdaPsk(device, BroadcastStandard.DvbS | BroadcastStandard.DvbS2));
            }
            else
            {
              deviceTuners.Add(new TunerBdaPsk(device, BroadcastStandard.DvbS));
            }
          }
          if (isOfdmSupported)
          {
            if (device.Name.ToLowerInvariant().Contains("t2"))
            {
              deviceTuners.Add(new TunerBdaOfdm(device, BroadcastStandard.DvbT | BroadcastStandard.DvbT2));
            }
            else
            {
              deviceTuners.Add(new TunerBdaOfdm(device, BroadcastStandard.DvbT));
            }
          }

          tuners.AddRange(deviceTuners);
          knownTuners.Add(devicePath, deviceTuners);
        }
        finally
        {
          _graph.RemoveFilter(tunerFilter);
          Release.ComObject("BDA detector source filter", ref tunerFilter);
        }
      }

      _knownTuners = knownTuners;
      return tuners;
    }

    #endregion

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

    private void DetectSourceTypeInternal(IBaseFilter filter, out bool isVsbSupported, out bool isQamSupported, out bool isPskSupported, out bool isPsk8Supported, out bool isOfdmSupported)
    {
      isVsbSupported = false;
      isQamSupported = false;
      isPskSupported = false;
      isPsk8Supported = false;
      isOfdmSupported = false;

      IBDA_Topology topology = filter as IBDA_Topology;
      if (topology == null)
      {
        this.LogDebug("  filter is not a BDA topology");
        return;
      }
      BDANodeDescriptor[] descriptors = new BDANodeDescriptor[21];
      int descriptorCount = 0;
      int hr = topology.GetNodeDescriptors(out descriptorCount, 20, descriptors);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("BDA detector: failed to get node descriptors from topology, hr = 0x{0:x}", hr);
        return;
      }
      this.LogDebug("  descriptor count = {0}", descriptorCount);
      for (int d = 0; d < descriptorCount; d++)
      {
        Guid function = descriptors[d].guidFunction;
        this.LogDebug("    function {0} = {1}", d, function);
        if (function == BDANodeCategory.EightVSBDemodulator)
        {
          isVsbSupported = true;
        }
        else if (function == BDANodeCategory.QAMDemodulator)
        {
          isQamSupported = true;
        }
        else if (function == BDANodeCategory.COFDMDemodulator ||
          function == TveGuid.KS_NODE_BDA_ISDB_T_DEMODULATOR)
        {
          isOfdmSupported = true;
        }
        else if (function == BDANodeCategory.QPSKDemodulator)
        {
          isPskSupported = true;
        }
        else if (function == TveGuid.KS_NODE_BDA_8PSK_DEMODULATOR ||
          function == TveGuid.KS_NODE_BDA_ISDB_S_DEMODULATOR)
        {
          isPskSupported = true;
          isPsk8Supported = true;
        }
      }

      if (!isVsbSupported && !isQamSupported && !isPskSupported && !isOfdmSupported)
      {
        this.LogDebug("  traversed topology but type not recognised");
      }
    }

    private void DetectSourceTypeMicrosoftGenericNetworkProvider(IBaseFilter filter, out bool isVsbSupported, out bool isQamSupported, out bool isQamDvbSupported, out bool isPskSupported, out bool isOfdmSupported)
    {
      isVsbSupported = false;
      isQamSupported = false;
      isQamDvbSupported = false;
      isPskSupported = false;
      isOfdmSupported = false;
      IBaseFilter genericNp = null;
      try
      {
        genericNp = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, typeof(NetworkProvider).GUID, "Microsoft Network Provider");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "BDA detector: failed to add Microsoft generic network provider to detection graph");
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
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("BDA detector: connected to Microsoft generic network provider but failed to get supported network types, hr = 0x{0:x}", hr);
          return;
        }

        this.LogDebug("  network type count = {0}", networkTypeCount);
        for (int n = 0; n < networkTypeCount; n++)
        {
          Guid networkType = networkTypes[n];
          this.LogDebug("    network type {0} = {1}", n, networkType);
          if (networkType == NetworkType.DIGITAL_CABLE)
          {
            // BDA hybrid ATSC/QAM, BDA DVB-C and PBDA CableCARD tuners all
            // advertise this network type. ATSC and DVB-C tuners usually
            // advertise additional types which allow us to identifiy them.
            // If we don't see those types, we assume this is a QAM or
            // CableCARD tuner.
            isQamSupported = true;
          }
          else if (networkType == NetworkType.ATSC_TERRESTRIAL)
          {
            isVsbSupported = true;
          }
          else if (networkType == NetworkType.DVB_CABLE ||
            networkType == NetworkType.ISDB_CABLE)
          {
            isQamSupported = true;
            isQamDvbSupported = true;
          }
          else if (networkType == NetworkType.DVB_TERRESTRIAL ||
            networkType == NetworkType.ISDB_TERRESTRIAL ||
            networkType == NetworkType.ISDB_T)
          {
            isOfdmSupported = true;
          }
          else if (networkType == NetworkType.DVB_SATELLITE ||
            networkType == NetworkType.DIRECTV_SATELLITE ||
            networkType == NetworkType.ECHOSTAR_SATELLITE ||
            networkType == NetworkType.ISDB_SATELLITE ||
            networkType == NetworkType.ISDB_S)
          {
            isPskSupported = true;
          }
        }

        if (!isVsbSupported && !isQamSupported && !isPskSupported && !isOfdmSupported)
        {
          this.LogDebug("  connected to Microsoft generic network provider but type not recognised");
        }
      }
      finally
      {
        _graph.RemoveFilter(genericNp);
        Release.ComObject("BDA detector Microsoft generic network provider", ref genericNp);
      }
    }

    private void DetectSourceTypeMicrosoftSpecificNetworkProvider(IBaseFilter filter, out bool isVsbSupported, out bool isQamSupported, out bool isPskSupported, out bool isOfdmSupported)
    {
      isVsbSupported = false;
      isQamSupported = false;
      isPskSupported = false;
      isOfdmSupported = false;
      if (ConnectFilter(_graph, _dvbtNp, filter))
      {
        isOfdmSupported = true;
      }
      else if (ConnectFilter(_graph, _dvbcNp, filter))
      {
        isQamSupported = true;
      }
      else if (ConnectFilter(_graph, _dvbsNp, filter))
      {
        isPskSupported = true;
      }
      else if (ConnectFilter(_graph, _atscNp, filter))
      {
        isVsbSupported = true;
      }
      else
      {
        this.LogDebug("  failed to connect to specific Microsoft network provider");
      }
    }
  }
}