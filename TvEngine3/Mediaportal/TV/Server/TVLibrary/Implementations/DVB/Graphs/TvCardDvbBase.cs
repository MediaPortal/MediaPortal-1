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
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for tuners with BDA drivers.
  /// </summary>
  public abstract class TvCardDvbBase : DeviceDirectShow, IDisposable, ITVCard
  {
    #region variables

    /// <summary>
    /// The network provider filter.
    /// </summary>
    private IBaseFilter _filterNetworkProvider = null;

    /// <summary>
    /// The [optional] BDA capture/receiver filter.
    /// </summary>
    private IBaseFilter _filterCapture = null;

    /// <summary>
    /// The DsDevice interface for the BDA capture/receiver component.
    /// </summary>
    private DsDevice _deviceCapture = null;

    #region compatibility filters

    /// <summary>
    /// Enable or disable use of the MPEG 2 demultiplexer and BDA TIF. Compatibility with some tuners
    /// requires these filters to be connected into the graph.
    /// </summary>
    private bool _addCompatibilityFilters = false;

    /// <summary>
    /// An infinite tee filter, used to fork the stream to the demultiplexer and TS writer/analyser.
    /// </summary>
    private IBaseFilter _filterInfiniteTee = null;

    /// <summary>
    /// The Microsoft MPEG 2 demultiplexer filter.
    /// </summary>
    private IBaseFilter _filterMpeg2Demultiplexer = null;

    /// <summary>
    /// The Microsoft transport information filter.
    /// </summary>
    private IBaseFilter _filterTransportInformation = null;

    #endregion

    /// <summary>
    /// A pre-configured tuning space, used to speed up the tuning process. 
    /// </summary>
    private ITuningSpace _tuningSpace = null;

    /// <summary>
    /// Tuner and demodulator signal statistic interfaces.
    /// </summary>
    private IList<IBDA_SignalStatistics> _signalStatistics = new List<IBDA_SignalStatistics>();

    /// <summary>
    /// Enable or disable the use of the internal network provider.
    /// </summary>
    private bool _useInternalNetworkProvider = false;

    // EPG grabbing - TODO refactor
    private IEpgEvents _epgEventsCallback = null;
    private BaseEpgGrabber _epgGrabberCallback = null;
    private TimeShiftingEPGGrabber _timeshiftingEpgGrabber = null;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TvCardDvbBase"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface for the instance to use.</param>
    /// <param name="device">The <see cref="DsDevice"/> instance that the instance will encapsulate.</param>
    public TvCardDvbBase(IEpgEvents epgEvents, DsDevice device)
      : base(device)
    {
      _epgEventsCallback = epgEvents;
      _useInternalNetworkProvider = FilterGraphTools.IsThisComObjectInstalled(InternalNetworkProvider.CLSID);
    }

    #endregion

    #region tuning & scanning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      if (_useInternalNetworkProvider)
      {
        this.LogDebug("TvCardDvbBase: using internal network provider tuning");
        PerformInternalNetworkProviderTuning(channel);
        return;
      }

      this.LogDebug("TvCardDvbBase: using BDA tuning");
      ITuneRequest tuneRequest = AssembleTuneRequest(_tuningSpace, channel);
      this.LogDebug("TvCardDvbBase: calling put_TuneRequest");
      int hr = (_filterNetworkProvider as ITuner).put_TuneRequest(tuneRequest);
      this.LogDebug("TvCardDvbBase: put_TuneRequest returned, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      Release.ComObject("base DVB tuner tune request", ref tuneRequest);

      // TerraTec tuners return a positive HRESULT value when already tuned with the required
      // parameters. See mantis 3469 for more details.
      if (hr < (int)HResult.Severity.Success)
      {
        HResult.ThrowException(hr, "Failed to tune channel.");
      }
    }

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="tuningSpace">The device's tuning space.</param>
    /// <param name="channel">The channel to translate into a tune request.</param>
    /// <returns>a tune request instance</returns>
    protected abstract ITuneRequest AssembleTuneRequest(ITuningSpace tuningSpace, IChannel channel);

    /// <summary>
    /// Tune using the internal network provider.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    private void PerformInternalNetworkProviderTuning(IChannel channel)
    {
      this.LogDebug("dvb:Submit tunerequest calling put_TuneRequest");
      int hr = 0;
      int undefinedValue = -1;
      IDvbNetworkProvider internalNpInterface = (IDvbNetworkProvider)_filterNetworkProvider;
      if (channel is DVBTChannel)
      {
        DVBTChannel dvbtChannel = channel as DVBTChannel;
        FrequencySettings fSettings = new FrequencySettings
        {
          Multiplier = 1000,
          Frequency = (uint)(dvbtChannel.Frequency),
          Bandwidth = (uint)dvbtChannel.Bandwidth,
          Polarity = Polarisation.NotSet,
          Range = (uint)undefinedValue
        };
        hr = internalNpInterface.TuneDVBT(fSettings);
      }
      if (channel is DVBSChannel)
      {
        DVBSChannel dvbsChannel = channel as DVBSChannel;
        if (dvbsChannel.ModulationType == ModulationType.ModNotSet)
        {
          dvbsChannel.ModulationType = ModulationType.ModQpsk;
        }
        FrequencySettings fSettings = new FrequencySettings
        {
          Multiplier = 1000,
          Frequency = (uint)dvbsChannel.Frequency,
          Bandwidth = (uint)undefinedValue,
          Polarity = dvbsChannel.Polarisation,
          Range = (uint)undefinedValue
        };
        DigitalDemodulator2Settings dSettings = new DigitalDemodulator2Settings
        {
          InnerFECRate = dvbsChannel.InnerFecRate,
          InnerFECMethod = FECMethod.MethodNotSet,
          Modulation = dvbsChannel.ModulationType,
          OuterFECMethod = FECMethod.MethodNotSet,
          OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
          Pilot = Pilot.NotSet,
          RollOff = RollOff.NotSet,
          SpectralInversion = SpectralInversion.NotSet,
          SymbolRate = (uint)dvbsChannel.SymbolRate,
          TransmissionMode = TransmissionMode.ModeNotSet
        };
        LnbInfoSettings lSettings = new LnbInfoSettings
        {
          LnbSwitchFrequency = (uint)dvbsChannel.LnbType.SwitchFrequency,
          LowOscillator = (uint)dvbsChannel.LnbType.LowBandFrequency,
          HighOscillator = (uint)dvbsChannel.LnbType.HighBandFrequency
        };
        DiseqcSatelliteSettings sSettings = new DiseqcSatelliteSettings
        {
          ToneBurstEnabled = 0,
          Diseq10Selection = LNB_Source.NOT_SET,
          Diseq11Selection = DiseqC11Switches.Switch_NOT_SET,
          Enabled = 0
        };
        hr = internalNpInterface.TuneDVBS(fSettings, dSettings, lSettings, sSettings);
      }
      if (channel is DVBCChannel)
      {
        DVBCChannel dvbcChannel = channel as DVBCChannel;
        FrequencySettings fSettings = new FrequencySettings
        {
          Multiplier = 1000,
          Frequency = (uint)dvbcChannel.Frequency,
          Bandwidth = (uint)undefinedValue,
          Polarity = Polarisation.NotSet,
          Range = (uint)undefinedValue
        };
        DigitalDemodulatorSettings dSettings = new DigitalDemodulatorSettings
        {
          InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
          InnerFECMethod = FECMethod.MethodNotSet,
          Modulation = dvbcChannel.ModulationType,
          OuterFECMethod = FECMethod.MethodNotSet,
          OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
          SpectralInversion = SpectralInversion.NotSet,
          SymbolRate = (uint)dvbcChannel.SymbolRate
        };

        hr = internalNpInterface.TuneDVBC(fSettings, dSettings);
      }
      if (channel is ATSCChannel)
      {
        ATSCChannel atscChannel = channel as ATSCChannel;
        if (atscChannel.ModulationType == ModulationType.Mod256Qam)
        {
          FrequencySettings fSettings = new FrequencySettings
          {
            Multiplier = 1000,
            Frequency = (uint)atscChannel.Frequency,
            Bandwidth = (uint)undefinedValue,
            Polarity = Polarisation.NotSet,
            Range = (uint)undefinedValue
          };
          DigitalDemodulatorSettings dSettings = new DigitalDemodulatorSettings
          {
            InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
            InnerFECMethod = FECMethod.MethodNotSet,
            Modulation = atscChannel.ModulationType,
            OuterFECMethod = FECMethod.MethodNotSet,
            OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
            SpectralInversion = SpectralInversion.NotSet,
            SymbolRate = (uint)undefinedValue
          };

          hr = internalNpInterface.TuneATSC((uint)undefinedValue, fSettings, dSettings);
        }
        else
        {
          FrequencySettings fSettings = new FrequencySettings
          {
            Multiplier = (uint)undefinedValue,
            Frequency = (uint)undefinedValue,
            Bandwidth = (uint)undefinedValue,
            Polarity = Polarisation.NotSet,
            Range = (uint)undefinedValue
          };
          DigitalDemodulatorSettings dSettings = new DigitalDemodulatorSettings
          {
            InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
            InnerFECMethod = FECMethod.MethodNotSet,
            Modulation = atscChannel.ModulationType,
            OuterFECMethod = FECMethod.MethodNotSet,
            OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
            SpectralInversion = SpectralInversion.NotSet,
            SymbolRate = (uint)undefinedValue
          };

          hr = internalNpInterface.TuneATSC((uint)undefinedValue, fSettings, dSettings);
        }
      }
      this.LogDebug("dvb:Submit tunerequest done calling put_TuneRequest");
      if (hr != 0)
      {
        this.LogDebug("dvb:SubmitTuneRequest  returns:0x{0:X} - {1}{2}", hr, HResult.GetDXErrorString(hr),
                          HResult.GetDXErrorString(hr));
        //remove subchannel.
        /*if (newSubChannel)
            {
            _mapSubChannels.Remove(subChannelId);
            }*/
        throw new TvException("Unable to tune to channel");
      }
    }

    /// <summary>
    /// Get the device's channel scanning interface.
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        return new DvbBaseScanning(this, _filterTsWriter as ITsChannelScan);
      }
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="id">The identifier for the subchannel.</param>
    /// <returns>the new subchannel instance</returns>
    protected override ITvSubChannel CreateNewSubChannel(int id)
    {
      return new TvDvbChannel(id, this, _filterTsWriter, _filterTransportInformation);
    }

    #endregion

    #region graph building

    /// <summary>
    /// Actually load the device.
    /// </summary>
    protected override void PerformLoading()
    {
      this.LogDebug("TvCardDvbBase: load device");
      InitialiseGraph();
      AddNetworkProviderFilterToGraph();

      int hr = 0;
      if (!_useInternalNetworkProvider)
      {
        // Some Microsoft network providers won't connect to the tuner filter
        // unless you set a tuning space.
        _tuningSpace = GetTuningSpace();
        if (_tuningSpace == null)
        {
          _tuningSpace = CreateTuningSpace();
        }
        ITuner tuner = _filterNetworkProvider as ITuner;
        if (tuner == null)
        {
          throw new TvException("Failed to get ITuner handle from network provider.");
        }
        hr = tuner.put_TuningSpace(_tuningSpace);
        HResult.ThrowException(hr, "Failed to put_TuningSpace() on ITuner.");
      }

      AddMainDeviceFilterToGraph();
      hr = _captureGraphBuilder.RenderStream(null, null, _filterNetworkProvider, null, _filterDevice);
      HResult.ThrowException(hr, "Failed to RenderStream() into the tuner filter.");

      // If we can't connect an infinite tee to the tuner filter then we need a capture filter.
      IBaseFilter lastFilter = _filterDevice;
      IBaseFilter tempInfiniteTee = (IBaseFilter)new InfTee();
      bool needCaptureFilter = false;
      try
      {
        FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, tempInfiniteTee, "Temp Infinite Tee", _filterDevice, _captureGraphBuilder);
        _graph.RemoveFilter(tempInfiniteTee);
      }
      catch
      {
        needCaptureFilter = true;
      }
      finally
      {
        Release.ComObject("base DVB tuner capture filter test infinite tee", ref tempInfiniteTee);
      }

      if (needCaptureFilter)
      {
        AddAndConnectCaptureFilterIntoGraph();
        lastFilter = _filterCapture;
      }

      // Check for and load plugins, adding any additional device filters to the graph.
      LoadPlugins(_filterDevice, ref lastFilter);

      // If using a Microsoft network provider and configured to do so, add an infinite
      // tee, MPEG 2 demultiplexer and transport information filter.
      if (!_useInternalNetworkProvider && _addCompatibilityFilters)
      {
        _filterInfiniteTee = (IBaseFilter)new InfTee();
        FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterInfiniteTee, "Infinite Tee", lastFilter, _captureGraphBuilder);
        lastFilter = _filterInfiniteTee;
        _filterMpeg2Demultiplexer = (IBaseFilter)new MPEG2Demultiplexer();
        FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterMpeg2Demultiplexer, "MPEG 2 Demultiplexer", _filterInfiniteTee, _captureGraphBuilder);
        AddAndConnectTransportInformationFilterIntoGraph();
      }

      // Complete the graph.
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();

      _signalStatistics = GetTunerSignalStatistics();
      _timeshiftingEpgGrabber = new TimeShiftingEPGGrabber(_epgEventsCallback, this);
    }

    /// <summary>
    /// Add the appropriate BDA network provider filter to the graph.
    /// </summary>
    private void AddNetworkProviderFilterToGraph()
    {
      this.LogDebug("TvCardDvbBase: add network provider filter");

      string filterName = string.Empty;
      Guid npClsid = Guid.Empty;
      if (_useInternalNetworkProvider)
      {
        filterName = "MediaPortal Network Provider";
        npClsid = InternalNetworkProvider.CLSID;
      }
      else
      {
        DbNetworkProvider npSetting = DbNetworkProvider.Generic;
        switch (_tunerType)
        {
          case CardType.DvbT:
            filterName = "DVB-T Network Provider";
            npClsid = typeof(DVBTNetworkProvider).GUID;
            npSetting = DbNetworkProvider.DvbT;
            break;
          case CardType.DvbS:
            filterName = "DVB-S Network Provider";
            npClsid = typeof(DVBSNetworkProvider).GUID;
            npSetting = DbNetworkProvider.DvbS;
            break;
          case CardType.DvbC:
            filterName = "DVB-C Network Provider";
            npClsid = typeof(DVBCNetworkProvider).GUID;
            npSetting = DbNetworkProvider.DvbC;
            break;
          case CardType.Atsc:
            filterName = "ATSC Network Provider";
            npClsid = typeof(ATSCNetworkProvider).GUID;
            npSetting = DbNetworkProvider.ATSC;
            break;
        }
        // If the generic network provider is preferred for this tuner then check if it is installed. The
        // generic NP is set as default, however it is only available on MCE 2005 Update Rollup 2 and newer.
        // We gracefully fall back to using one of the specific network providers if necessary.      
        Card d = CardManagement.GetCardByDevicePath(DevicePath, CardIncludeRelationEnum.None);
        if (d.NetProvider == (int)DbNetworkProvider.Generic)
        {
          if (!FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID))
          {
            // The generic network provider is not installed. Amend configuration and fall back...
            d.NetProvider = (int)npSetting;
            CardManagement.SaveCard(d);
          }
          else
          {
            filterName = "Generic Network Provider";
            npClsid = typeof(NetworkProvider).GUID;
          }
        }
      }

      this.LogDebug("TvCardDvbBase: using {0}", filterName);
      _filterNetworkProvider = FilterGraphTools.AddFilterByClsid(_graph, npClsid, filterName);

      if (_useInternalNetworkProvider)
      {
        IDvbNetworkProvider internalNpInterface = _filterNetworkProvider as IDvbNetworkProvider;
        internalNpInterface.ConfigureLogging(InternalNetworkProvider.GetFileName(DevicePath), InternalNetworkProvider.GetHash(DevicePath), LogLevelOption.Debug);
      }
    }

    /// <summary>
    /// The registered name of the BDA tuning space for the device type.
    /// </summary>
    protected abstract string TuningSpaceName
    {
      get;
    }

    /// <summary>
    /// Get the registered BDA tuning space for the device type if it exists.
    /// </summary>
    /// <returns>the registed tuning space</returns>
    private ITuningSpace GetTuningSpace()
    {
      this.LogDebug("TvCardDvbBase: get tuning space");

      // We're going to enumerate the tuning spaces registered in the system to
      // see if we can find the correct MediaPortal tuning space.
      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      try
      {
        ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
        if (container == null)
        {
          throw new TvException("Failed to get ITuningSpaceContainer handle from SystemTuningSpaces instance.");
        }

        IEnumTuningSpaces enumTuningSpaces;
        int hr = container.get_EnumTuningSpaces(out enumTuningSpaces);
        HResult.ThrowException(hr, "Failed to get_EnumTuningSpaces() on ITuningSpaceContainer.");
        try
        {
          // Enumerate...
          ITuningSpace[] spaces = new ITuningSpace[2];
          int fetched;
          while (enumTuningSpaces.Next(1, spaces, out fetched) == 0 && fetched == 1)
          {
            try
            {
              // Is this the one we're looking for?
              string name;
              hr = spaces[0].get_UniqueName(out name);
              HResult.ThrowException(hr, "Failed to get_UniqueName() on ITuningSpace.");
              if (name.Equals(TuningSpaceName))
              {
                this.LogDebug("TvCardDvbBase: found correct tuning space");
                return spaces[0];
              }
              else
              {
                Release.ComObject("base DVB tuner tuning space", ref spaces[0]);
              }
            }
            catch (Exception)
            {
              Release.ComObject("base DVB tuner tuning space", ref spaces[0]);
              throw;
            }
          }
          return null;
        }
        finally
        {
          Release.ComObject("base DVB tuner tuning space enumerator", ref enumTuningSpaces);
        }
      }
      finally
      {
        Release.ComObject("base DVB tuner tuning space container", ref systemTuningSpaces);
      }
    }

    /// <summary>
    /// Create and register a BDA tuning space for the device type.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected abstract ITuningSpace CreateTuningSpace();

    /// <summary>
    /// Add and connect the appropriate BDA capture/receiver filter into the graph.
    /// </summary>
    private void AddAndConnectCaptureFilterIntoGraph()
    {
      this.LogDebug("TvCardDvbBase: add capture filter");
      bool matchDeviceIdentifier = true;
      string tunerDeviceIdentifier = DevicePathUtils.ExtractDeviceIdentifier(DevicePath);
      while (true)
      {
        DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
        for (int i = 0; i < devices.Length; i++)
        {
          DsDevice device = devices[i];
          if (device == null)
          {
            continue;
          }
          string devicePath = device.DevicePath;
          if (devicePath == null ||
            devicePath.Contains("root#system#") ||
            (matchDeviceIdentifier && !tunerDeviceIdentifier.Equals(DevicePathUtils.ExtractDeviceIdentifier(devicePath))) ||
            !DevicesInUse.Instance.Add(device)
          )
          {
            continue;
          }
          this.LogDebug("TvCardDvbBase: try {0} {1}", device.Name, devicePath);
          try
          {
            _filterCapture = FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, device, _filterDevice, _captureGraphBuilder);
            _deviceCapture = device;
            return;
          }
          catch
          {
            DevicesInUse.Instance.Remove(device);
          }
        }
        if (!matchDeviceIdentifier)
        {
          this.LogWarn("TvCardDvbBase: failed to add and connect capture filter, assuming plugin required to complete graph");
          break;
        }
        matchDeviceIdentifier = false;
        this.LogDebug("TvCardDvbBase: allow non-matching capture devices");
      }
    }

    /// <summary>
    /// Add and connect a transport information filter into the graph.
    /// </summary>
    private void AddAndConnectTransportInformationFilterIntoGraph()
    {
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        DsDevice device = devices[i];
        if (device == null || device.Name == null || !device.Name.Equals("BDA MPEG2 Transport Information Filter"))
        {
          continue;
        }
        FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, device, _filterMpeg2Demultiplexer, _captureGraphBuilder);
        return;
      }
      this.LogWarn("TvCardDvbBase: transport information filter not found");
    }

    /// <summary>
    /// Get the signal statistic interfaces from the device.
    /// </summary>
    /// <returns>the signal statistic interfaces</returns>
    private IList<IBDA_SignalStatistics> GetTunerSignalStatistics()
    {
      this.LogDebug("TvCardDvbBase: get tuner signal statistics");

      IBDA_Topology topology = _filterDevice as IBDA_Topology;
      if (topology == null)
      {
        throw new TvException("Failed to get IBDA_Topology handle from tuner.");
      }

      int nodeTypeCount;
      int[] nodeTypes = new int[33];
      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      HResult.ThrowException(hr, "Failed to GetNodeTypes() on IBDA_Topology.");

      IList<IBDA_SignalStatistics> statistics = new List<IBDA_SignalStatistics>();
      Guid[] guidInterfaces = new Guid[33];
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        object controlNode;
        hr = topology.GetControlNode(0, 1, nodeTypes[i], out controlNode);
        HResult.ThrowException(hr, "Failed to GetControlNode() on IBDA_Topology.");

        IBDA_SignalStatistics s = controlNode as IBDA_SignalStatistics;
        if (s != null)
        {
          statistics.Add(s);
        }
      }
      if (statistics.Count == 0)
      {
        throw new TvException("Failed to locate signal statistic interfaces.");
      }
      return statistics;
    }

    /// <summary>
    /// Actually unload the device.
    /// </summary>
    protected override void PerformUnloading()
    {
      if (_epgGrabbing)
      {
        if (_epgGrabberCallback != null)
        {
          _epgGrabberCallback.OnEpgCancelled();
        }
        _epgGrabbing = false;
      }
      _timeshiftingEpgGrabber.Dispose();

      if (_graph != null)
      {
        if (_filterNetworkProvider != null)
        {
          _graph.RemoveFilter(_filterNetworkProvider);
        }
        if (_filterCapture != null)
        {
          _graph.RemoveFilter(_filterCapture);
        }
        if (_filterInfiniteTee != null)
        {
          _graph.RemoveFilter(_filterInfiniteTee);
        }
        if (_filterMpeg2Demultiplexer != null)
        {
          _graph.RemoveFilter(_filterMpeg2Demultiplexer);
        }
        if (_filterTransportInformation != null)
        {
          _graph.RemoveFilter(_filterTransportInformation);
        }
      }
      Release.ComObject("base DVB tuner network provider", ref _filterNetworkProvider);
      Release.ComObject("base DVB tuner capture filter", ref _filterCapture);
      Release.ComObject("base DVB tuner infinite tee", ref _filterInfiniteTee);
      Release.ComObject("base DVB tuner MPEG 2 demultiplexer", ref _filterMpeg2Demultiplexer);
      Release.ComObject("base DVB tuner transport information filter", ref _filterTransportInformation);
      Release.ComObject("base DVB tuner tuning space", ref _tuningSpace);

      if (_deviceCapture != null)
      {
        DevicesInUse.Instance.Remove(_deviceCapture);
        _deviceCapture = null;
      }
      if (_signalStatistics != null)
      {
        for (int i = 0; i < _signalStatistics.Count; i++)
        {
          IBDA_SignalStatistics s = _signalStatistics[i];
          Release.ComObject("base DVB tuner signal statistic", ref s);
        }
        _signalStatistics.Clear();
      }

      CleanUpGraph();
    }

    #endregion

    #region signal quality, level etc

    /// <summary>
    /// Update the tuner signal status statistics.
    /// </summary>
    /// <param name="force"><c>True</c> to force the status to be updated (status information may be cached).</param>
    protected override void UpdateSignalStatus(bool force)
    {
      if (!force)
      {
        TimeSpan ts = DateTime.Now - _lastSignalUpdate;
        if (ts.TotalMilliseconds < 5000)
        {
          return;
        }
      }
      try
      {
        if (_currentTuningDetail == null || _state != DeviceState.Started || _signalStatistics == null || _signalStatistics.Count == 0)
        {
          //System.Diagnostics.Debugger.Launch();
          _tunerLocked = false;
          _signalPresent = false;
          _signalLevel = 0;
          _signalQuality = 0;
          return;
        }

        bool isTunerLocked = false;
        long signalQuality = 0;
        long signalStrength = 0;

        //       Log.this.LogDebug("dvb:UpdateSignalQuality() count:{0}", _tunerStatistics.Count);
        for (int i = 0; i < _signalStatistics.Count; i++)
        {
          IBDA_SignalStatistics stat = _signalStatistics[i];
          //          Log.this.LogDebug("   dvb:  #{0} get locked",i );
          try
          {
            bool isLocked;
            //is the tuner locked?
            stat.get_SignalLocked(out isLocked);
            isTunerLocked |= isLocked;
            //  Log.this.LogDebug("   dvb:  #{0} isTunerLocked:{1}", i,isLocked);
          }
          catch (COMException)
          {
            //            Log.this.LogDebug("get_SignalLocked() locked :{0}", ex);
          }
          catch (Exception ex)
          {
            this.LogDebug("get_SignalLocked() locked :{0}", ex);
          }

          //          Log.this.LogDebug("   dvb:  #{0} get signalquality", i);
          try
          {
            int quality;
            //is a signal quality ok?
            stat.get_SignalQuality(out quality); //1-100
            if (quality > 0)
              signalQuality += quality;
            //   Log.this.LogDebug("   dvb:  #{0} signalQuality:{1}", i, quality);
          }
          catch (COMException)
          {
            //            Log.this.LogDebug("get_SignalQuality() locked :{0}", ex);
          }
          catch (Exception ex)
          {
            this.LogDebug("get_SignalQuality() locked :{0}", ex);
          }
          //          Log.this.LogDebug("   dvb:  #{0} get signalstrength", i);
          try
          {
            int strength;
            //is a signal strength ok?
            stat.get_SignalStrength(out strength); //1-100
            if (strength > 0)
              signalStrength += strength;
            //    Log.this.LogDebug("   dvb:  #{0} signalStrength:{1}", i, strength);
          }
          catch (COMException)
          {
            //            Log.this.LogDebug("get_SignalQuality() locked :{0}", ex);
          }
          catch (Exception ex)
          {
            this.LogDebug("get_SignalQuality() locked :{0}", ex);
          }
          //Log.this.LogDebug("  dvb:#{0}  locked:{1} present:{2} quality:{3} strength:{4}", i, isLocked, isPresent, quality, strength);          
        }
        if (_signalStatistics.Count > 0)
        {
          _signalQuality = (int)signalQuality / _signalStatistics.Count;
          _signalLevel = (int)signalStrength / _signalStatistics.Count;
        }
        _tunerLocked = isTunerLocked;

        _signalPresent = isTunerLocked;
      }
      finally
      {
        _lastSignalUpdate = DateTime.Now;
      }
    }

    #endregion

    #region Channel linkage handling

    private static bool SameAsPortalChannel(PortalChannel pChannel, LinkedChannel lChannel)
    {
      return ((pChannel.NetworkId == lChannel.NetworkId) && (pChannel.TransportId == lChannel.TransportId) &&
              (pChannel.ServiceId == lChannel.ServiceId));
    }

    private static bool IsNewLinkedChannel(PortalChannel pChannel, LinkedChannel lChannel)
    {
      bool bRet = true;
      foreach (LinkedChannel lchan in pChannel.LinkedChannels)
      {
        if ((lchan.NetworkId == lChannel.NetworkId) && (lchan.TransportId == lChannel.TransportId) &&
            (lchan.ServiceId == lChannel.ServiceId))
        {
          bRet = false;
          break;
        }
      }
      return bRet;
    }

    /// <summary>
    /// Starts scanning for linkage info
    /// </summary>
    public override void StartLinkageScanner(BaseChannelLinkageScanner callback)
    {
      ITsChannelLinkageScanner linkageScanner = _filterTsWriter as ITsChannelLinkageScanner;
      linkageScanner.SetCallBack(callback);
      linkageScanner.Start();
    }

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public override void ResetLinkageScanner()
    {
      (_filterTsWriter as ITsChannelLinkageScanner).Reset();
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public override List<PortalChannel> ChannelLinkages
    {
      get
      {
        ITsChannelLinkageScanner linkageScanner = _filterTsWriter as ITsChannelLinkageScanner;
        try
        {
          uint channelCount;
          List<PortalChannel> portalChannels = new List<PortalChannel>();
          linkageScanner.GetChannelCount(out channelCount);
          if (channelCount == 0)
            return portalChannels;
          for (uint i = 0; i < channelCount; i++)
          {
            ushort network_id = 0;
            ushort transport_id = 0;
            ushort service_id = 0;
            linkageScanner.GetChannel(i, ref network_id, ref transport_id, ref service_id);
            PortalChannel pChannel = new PortalChannel();
            pChannel.NetworkId = network_id;
            pChannel.TransportId = transport_id;
            pChannel.ServiceId = service_id;
            uint linkCount;
            linkageScanner.GetLinkedChannelsCount(i, out linkCount);
            if (linkCount > 0)
            {
              for (uint j = 0; j < linkCount; j++)
              {
                ushort nid = 0;
                ushort tid = 0;
                ushort sid = 0;
                IntPtr ptrName;
                linkageScanner.GetLinkedChannel(i, j, ref nid, ref tid, ref sid, out ptrName);
                LinkedChannel lChannel = new LinkedChannel();
                lChannel.NetworkId = nid;
                lChannel.TransportId = tid;
                lChannel.ServiceId = sid;
                lChannel.Name = Marshal.PtrToStringAnsi(ptrName);
                if ((!SameAsPortalChannel(pChannel, lChannel)) && (IsNewLinkedChannel(pChannel, lChannel)))
                  pChannel.LinkedChannels.Add(lChannel);
              }
            }
            if (pChannel.LinkedChannels.Count > 0)
              portalChannels.Add(pChannel);
          }
          linkageScanner.Reset();
          return portalChannels;
        }
        catch (Exception ex)
        {
          this.LogError(ex);
          return new List<PortalChannel>();
        }
      }
    }

    #endregion

    #region EPG

    /// <summary>
    /// Start grabbing the epg
    /// </summary>
    public override void GrabEpg(BaseEpgGrabber callback)
    {
      _epgGrabberCallback = callback;
      this.LogDebug("dvb:grab epg...");
      ITsEpgScanner epgGrabber = _filterTsWriter as ITsEpgScanner;
      if (epgGrabber == null)
      {
        return;
      }
      epgGrabber.SetCallBack(callback);
      epgGrabber.GrabEPG();
      epgGrabber.GrabMHW();
      _epgGrabbing = true;
    }

    /// <summary>
    /// Start grabbing the epg while timeshifting
    /// </summary>
    public override void GrabEpg()
    {
      if (_timeshiftingEpgGrabber.StartGrab())
        GrabEpg(_timeshiftingEpgGrabber);
    }

    /// <summary>
    /// Activates / deactivates the epg grabber
    /// </summary>
    /// <param name="value">Mode</param>
    protected override void UpdateEpgGrabber(bool value)
    {
      if (_epgGrabbing && value == false)
      {
        if (_epgGrabberCallback != null)
        {
          _epgGrabberCallback.OnEpgCancelled();
        }
        (_filterTsWriter as ITsEpgScanner).Reset();
      }
      _epgGrabbing = value;
    }

    /// <summary>
    /// Gets the UTC.
    /// </summary>
    /// <param name="val">The val.</param>
    /// <returns></returns>
    private static int getUTC(int val)
    {
      if ((val & 0xF0) >= 0xA0)
        return 0;
      if ((val & 0xF) >= 0xA)
        return 0;
      return ((val & 0xF0) >> 4) * 10 + (val & 0xF);
    }

    /// <summary>
    /// Aborts grabbing the epg
    /// </summary>
    public override void AbortGrabbing()
    {
      this.LogDebug("dvb:abort grabbing epg");
      ITsEpgScanner epgGrabber = _filterTsWriter as ITsEpgScanner;
      if (epgGrabber != null)
      {
        epgGrabber.AbortGrabbing();
      }
      if (_timeshiftingEpgGrabber != null)
      {
        _timeshiftingEpgGrabber.OnEpgCancelled();
      }
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public override List<EpgChannel> Epg
    {
      get
      {
        try
        {
          ITsEpgScanner epgGrabber = _filterTsWriter as ITsEpgScanner;
          bool dvbReady, mhwReady;
          epgGrabber.IsEPGReady(out dvbReady);
          epgGrabber.IsMHWReady(out mhwReady);
          if (dvbReady == false || mhwReady == false)
            return null;
          uint titleCount;
          uint channelCount;
          epgGrabber.GetMHWTitleCount(out titleCount);
          mhwReady = titleCount > 10;
          epgGrabber.GetEPGChannelCount(out channelCount);
          dvbReady = channelCount > 0;
          List<EpgChannel> epgChannels = new List<EpgChannel>();
          this.LogInfo("dvb:mhw ready MHW {0} titles found", titleCount);
          this.LogInfo("dvb:dvb ready.EPG {0} channels", channelCount);
          if (mhwReady)
          {
            epgGrabber.GetMHWTitleCount(out titleCount);
            for (int i = 0; i < titleCount; ++i)
            {
              uint id = 0;
              UInt32 programid = 0;
              uint transportid = 0, networkid = 0, channelnr = 0, channelid = 0, themeid = 0, PPV = 0, duration = 0;
              byte summaries = 0;
              uint datestart = 0, timestart = 0;
              uint tmp1 = 0, tmp2 = 0;
              IntPtr ptrTitle, ptrProgramName;
              IntPtr ptrChannelName, ptrSummary, ptrTheme;
              epgGrabber.GetMHWTitle((ushort)i, ref id, ref tmp1, ref tmp2, ref channelnr, ref programid,
                                               ref themeid, ref PPV, ref summaries, ref duration, ref datestart,
                                               ref timestart, out ptrTitle, out ptrProgramName);
              epgGrabber.GetMHWChannel(channelnr, ref channelid, ref networkid, ref transportid,
                                                 out ptrChannelName);
              epgGrabber.GetMHWSummary(programid, out ptrSummary);
              epgGrabber.GetMHWTheme(themeid, out ptrTheme);
              string channelName = DvbTextConverter.Convert(ptrChannelName, "");
              string title = DvbTextConverter.Convert(ptrTitle, "");
              string summary = DvbTextConverter.Convert(ptrSummary, "");
              string theme = DvbTextConverter.Convert(ptrTheme, "");
              if (channelName == null)
                channelName = "";
              if (title == null)
                title = "";
              if (summary == null)
                summary = "";
              if (theme == null)
                theme = "";
              channelName = channelName.Trim();
              title = title.Trim();
              summary = summary.Trim();
              theme = theme.Trim();
              EpgChannel epgChannel = null;
              foreach (EpgChannel chan in epgChannels)
              {
                DVBBaseChannel dvbChan = (DVBBaseChannel)chan.Channel;
                if (dvbChan.NetworkId == networkid && dvbChan.TransportId == transportid &&
                    dvbChan.ServiceId == channelid)
                {
                  epgChannel = chan;
                  break;
                }
              }
              if (epgChannel == null)
              {
                // We need to use a matching channel type per card, because tuning details will be looked up with cardtype as filter.
                DVBBaseChannel channel = CreateChannel();
                channel.NetworkId = (int)networkid;
                channel.TransportId = (int)transportid;
                channel.ServiceId = (int)channelid;
                epgChannel = new EpgChannel { Channel = channel };
                //Log.this.LogInfo("dvb: start filtering channel NID {0} TID {1} SID{2}", dvbChan.NetworkId, dvbChan.TransportId, dvbChan.ServiceId);
                if (FilterOutEPGChannel((ushort)networkid, (ushort)transportid, (ushort)channelid) == false)
                {
                  //Log.this.LogInfo("dvb: Not Filtered channel NID {0} TID {1} SID{2}", dvbChan.NetworkId, dvbChan.TransportId, dvbChan.ServiceId);
                  epgChannels.Add(epgChannel);
                }
              }
              uint d1 = datestart;
              uint m = timestart & 0xff;
              uint h1 = (timestart >> 16) & 0xff;
              DateTime dayStart = DateTime.Now;
              dayStart =
                dayStart.Subtract(new TimeSpan(1, dayStart.Hour, dayStart.Minute, dayStart.Second, dayStart.Millisecond));
              int day = (int)dayStart.DayOfWeek;
              DateTime programStartTime = dayStart;
              int minVal = (int)((d1 - day) * 86400 + h1 * 3600 + m * 60);
              if (minVal < 21600)
                minVal += 604800;
              programStartTime = programStartTime.AddSeconds(minVal);
              EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));
              EpgLanguageText epgLang = new EpgLanguageText("ALL", title, summary, theme, 0, "", -1);
              program.Text.Add(epgLang);
              epgChannel.Programs.Add(program);
            }
            for (int i = 0; i < epgChannels.Count; ++i)
            {
              epgChannels[i].Sort();
            }
            // free the epg infos in TsWriter so that the mem used gets released 
            epgGrabber.Reset();
            return epgChannels;
          }

          if (dvbReady)
          {
            ushort networkid = 0;
            ushort transportid = 0;
            ushort serviceid = 0;
            for (uint x = 0; x < channelCount; ++x)
            {
              epgGrabber.GetEPGChannel(x, ref networkid, ref transportid, ref serviceid);
              // We need to use a matching channel type per card, because tuning details will be looked up with cardtype as filter.
              DVBBaseChannel channel = CreateChannel();
              channel.NetworkId = networkid;
              channel.TransportId = transportid;
              channel.ServiceId = serviceid;
              EpgChannel epgChannel = new EpgChannel { Channel = channel };
              uint eventCount;
              epgGrabber.GetEPGEventCount(x, out eventCount);
              for (uint i = 0; i < eventCount; ++i)
              {
                uint start_time_MJD, start_time_UTC, duration, languageCount;
                string title, description;
                IntPtr ptrGenre;
                int starRating;
                IntPtr ptrClassification;

                epgGrabber.GetEPGEvent(x, i, out languageCount, out start_time_MJD, out start_time_UTC,
                                                 out duration, out ptrGenre, out starRating, out ptrClassification);
                string genre = DvbTextConverter.Convert(ptrGenre, "");
                string classification = DvbTextConverter.Convert(ptrClassification, "");

                if (starRating < 1 || starRating > 7)
                  starRating = 0;

                int duration_hh = getUTC((int)((duration >> 16)) & 255);
                int duration_mm = getUTC((int)((duration >> 8)) & 255);
                int duration_ss = 0; //getUTC((int) (duration )& 255);
                int starttime_hh = getUTC((int)((start_time_UTC >> 16)) & 255);
                int starttime_mm = getUTC((int)((start_time_UTC >> 8)) & 255);
                int starttime_ss = 0; //getUTC((int) (start_time_UTC )& 255);

                if (starttime_hh > 23)
                  starttime_hh = 23;
                if (starttime_mm > 59)
                  starttime_mm = 59;
                if (starttime_ss > 59)
                  starttime_ss = 59;

                // DON'T ENABLE THIS. Some entries can be indeed >23 Hours !!!
                //if (duration_hh > 23) duration_hh = 23;
                if (duration_mm > 59)
                  duration_mm = 59;
                if (duration_ss > 59)
                  duration_ss = 59;

                // convert the julian date
                int year = (int)((start_time_MJD - 15078.2) / 365.25);
                int month = (int)((start_time_MJD - 14956.1 - (int)(year * 365.25)) / 30.6001);
                int day = (int)(start_time_MJD - 14956 - (int)(year * 365.25) - (int)(month * 30.6001));
                int k = (month == 14 || month == 15) ? 1 : 0;
                year += 1900 + k; // start from year 1900, so add that here
                month = month - 1 - k * 12;
                int starttime_y = year;
                int starttime_m = month;
                int starttime_d = day;
                if (year < 2000)
                  continue;

                try
                {
                  DateTime dtUTC = new DateTime(starttime_y, starttime_m, starttime_d, starttime_hh, starttime_mm,
                                                starttime_ss, 0);
                  DateTime dtStart = dtUTC.ToLocalTime();
                  if (dtStart < DateTime.Now.AddDays(-1) || dtStart > DateTime.Now.AddMonths(2))
                    continue;
                  DateTime dtEnd = dtStart.AddHours(duration_hh);
                  dtEnd = dtEnd.AddMinutes(duration_mm);
                  dtEnd = dtEnd.AddSeconds(duration_ss);
                  EpgProgram epgProgram = new EpgProgram(dtStart, dtEnd);
                  //EPGEvent newEvent = new EPGEvent(genre, dtStart, dtEnd);
                  for (int z = 0; z < languageCount; ++z)
                  {
                    uint languageId;
                    IntPtr ptrTitle;
                    IntPtr ptrDesc;
                    int parentalRating;
                    epgGrabber.GetEPGLanguage(x, i, (uint)z, out languageId, out ptrTitle, out ptrDesc,
                                                        out parentalRating);
                    //title = DvbTextConverter.Convert(ptrTitle,"");
                    //description = DvbTextConverter.Convert(ptrDesc,"");
                    string language = String.Empty;
                    language += (char)((languageId >> 16) & 0xff);
                    language += (char)((languageId >> 8) & 0xff);
                    language += (char)((languageId) & 0xff);
                    //allows czech epg
                    if (language.ToUpperInvariant() == "CZE" || language.ToUpperInvariant() == "CES")
                    {
                      title = Iso6937ToUnicode.Convert(ptrTitle);
                      description = Iso6937ToUnicode.Convert(ptrDesc);
                    }
                    else
                    {
                      title = DvbTextConverter.Convert(ptrTitle, "");
                      description = DvbTextConverter.Convert(ptrDesc, "");
                    }
                    if (title == null)
                      title = "";
                    if (description == null)
                      description = "";
                    if (string.IsNullOrEmpty(language))
                      language = "";
                    if (genre == null)
                      genre = "";
                    if (classification == null)
                      classification = "";
                    title = title.Trim();
                    description = description.Trim();
                    language = language.Trim();
                    genre = genre.Trim();
                    EpgLanguageText epgLangague = new EpgLanguageText(language, title, description, genre, starRating,
                                                                      classification, parentalRating);
                    epgProgram.Text.Add(epgLangague);
                  }
                  epgChannel.Programs.Add(epgProgram);
                }
                catch (Exception ex)
                {
                  this.LogError(ex);
                }
              } //for (uint i = 0; i < eventCount; ++i)
              if (epgChannel.Programs.Count > 0)
              {
                epgChannel.Sort();
                //Log.this.LogInfo("dvb: start filtering channel NID {0} TID {1} SID{2}", chan.NetworkId, chan.TransportId, chan.ServiceId);
                if (FilterOutEPGChannel(networkid, transportid, serviceid) == false)
                {
                  //Log.this.LogInfo("dvb: Not Filtered channel NID {0} TID {1} SID{2}", chan.NetworkId, chan.TransportId, chan.ServiceId);
                  epgChannels.Add(epgChannel);
                }
              }
            } //for (uint x = 0; x < channelCount; ++x)
          }
          // free the epg infos in TsWriter so that the mem used gets released 
          epgGrabber.Reset();
          return epgChannels;
        }
        catch (Exception ex)
        {
          this.LogError(ex);
          return new List<EpgChannel>();
        }
      }
    }

    /// <summary>
    /// Creates a channel that matches to the card type (DVB-C/-S-/-T...).
    /// </summary>
    /// <returns></returns>
    protected abstract DVBBaseChannel CreateChannel();

    /// <summary>
    /// Check if the EPG data found in a scan should not be kept.
    /// </summary>
    /// <remarks>
    /// This function implements the logic to filter out data for services that are not on the same transponder.
    /// </remarks>
    /// <value><c>false</c> if the data should be kept, otherwise <c>true</c></value>
    protected virtual bool FilterOutEPGChannel(ushort networkId, ushort transportStreamId, ushort serviceId)
    {
      bool setting = SettingsManagement.GetValue("generalGrapOnlyForSameTransponder", true);

      if (!setting)
      {
        return false;
      }

      // The following code attempts to find a tuning detail for the tuner type (eg. a DVB-T tuning detail for
      // a DVB-T tuner), and check if that tuning detail corresponds with the same transponder that the EPG was
      // collected from (ie. the transponder that the tuner is currently tuned to). This logic will potentially
      // fail for people that merge HD and SD tuning details that happen to be for the same tuner type.
      Channel dbchannel = ChannelManagement.GetChannelByTuningDetail(networkId, transportStreamId, serviceId);
      if (dbchannel == null)
      {
        return false;
      }
      foreach (TuningDetail detail in dbchannel.TuningDetails)
      {
        IChannel channel = ChannelManagement.GetTuningChannel(detail);
        if (CanTune(channel))
        {
          return _currentTuningDetail.IsDifferentTransponder(channel);
        }
      }
      return false;
    }

    #endregion
  }
}