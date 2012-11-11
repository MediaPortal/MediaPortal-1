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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using LNB_Source = DirectShowLib.BDA.LNB_Source;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs
{
  /// <summary>
  /// base class for DVB cards
  /// </summary>
  public abstract class TvCardDvbBase : TvCardBase, IDisposable, ITVCard
  {


    #region constants

    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    private class MpTsAnalyzer { }

    #endregion

    #region variables

    /// <summary>
    /// Capture graph builder
    /// </summary>
    protected ICaptureGraphBuilder2 _capBuilder;

    /// <summary>
    /// ROT entry
    /// </summary>
    protected DsROTEntry _rotEntry;

    /// <summary>
    /// Network provider filter
    /// </summary>
    protected IBaseFilter _filterNetworkProvider;

    /// <summary>
    /// MPEG2 Demux filter
    /// </summary>
    protected IBaseFilter _filterMpeg2DemuxTif;

    /// <summary>
    /// Main inf tee
    /// </summary>
    protected IBaseFilter _infTee;

    /// <summary>
    /// Tuner filter
    /// </summary>
    protected IBaseFilter _filterTuner;

    /// <summary>
    /// Capture filter
    /// </summary>
    protected IBaseFilter _filterCapture;

    /// <summary>
    /// TIF filter
    /// </summary>
    protected IBaseFilter _filterTIF;

    /// <summary>
    /// Capture device
    /// </summary>
    protected DsDevice _captureDevice;

    /// <summary>
    /// EPG Grabber callback
    /// </summary>
    protected BaseEpgGrabber _epgGrabberCallback;

    /// <summary>
    /// Tuner statistics
    /// </summary>
    protected List<IBDA_SignalStatistics> _tunerStatistics = new List<IBDA_SignalStatistics>();

    /// <summary>
    /// TsWriter filter
    /// </summary>
    protected IBaseFilter _filterTsWriter;

    /// <summary>
    /// EPG Grabber interface
    /// </summary>
    protected ITsEpgScanner _interfaceEpgGrabber;

    /// <summary>
    /// Channel scan interface
    /// </summary>
    protected ITsChannelScan _interfaceChannelScan;

    /// <summary>
    /// Internal Network provider instance
    /// </summary>
    protected IDvbNetworkProvider _interfaceNetworkProvider;

    /// <summary>
    /// Indicates if the internal network provider is used
    /// </summary>
    protected bool _useInternalNetworkProvider;

    /// <summary>
    /// Channel linkage scanner interface
    /// </summary>
    protected ITsChannelLinkageScanner _interfaceChannelLinkageScanner;

    private readonly TimeShiftingEPGGrabber _timeshiftingEPGGrabber;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDvbBase"/> class.
    /// </summary>
    public TvCardDvbBase(IEpgEvents epgEvents, DsDevice device)
      : base(device)
    {
      _timeshiftingEPGGrabber = new TimeShiftingEPGGrabber(epgEvents, (ITVCard)this);
      _supportsSubChannels = true;
      Guid networkProviderClsId = new Guid("{D7D42E5C-EB36-4aad-933B-B4C419429C98}");
      _useInternalNetworkProvider = FilterGraphTools.IsThisComObjectInstalled(networkProviderClsId);
    }

    #endregion

    #region tuning & scanning

    protected virtual void OnAfterTune(IChannel channel) { }

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

      if (_useCustomTuning)
      {
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          ICustomTuner customTuner = deviceInterface as ICustomTuner;
          if (customTuner != null && customTuner.CanTuneChannel(channel))
          {
            this.LogDebug("TvCardDvbBase: using custom tuning");
            if (!customTuner.Tune(channel))
            {
              throw new TvException("TvCardDvbBase: failed to tune to channel");
            }
            return;
          }
        }
      }

      this.LogDebug("TvCardDvbBase: using BDA tuning");
      ITuneRequest tuneRequest = AssembleTuneRequest(channel);
      if (tuneRequest == null)
      {
        throw new TvException("TvCardDvbBase: failed to assemble tune request");
      }
      this.LogDebug("TvCardDvbBase: calling put_TuneRequest");
      int hr = ((ITuner)_filterNetworkProvider).put_TuneRequest(tuneRequest);
      this.LogDebug("TvCardDvbBase: put_TuneRequest returned, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));

      // TerraTec tuners return a positive HRESULT value when already tuned with the required
      // parameters. See mantis 3469 for more details.
      if (hr < 0)
      {
        throw new TvException("TvCardDvbBase: failed to tune to channel");
      }
    }

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="channel">The channel that will be tuned.</param>
    /// <returns>the assembled tune request</returns>
    protected virtual ITuneRequest AssembleTuneRequest(IChannel channel)
    {
      return null;
    }

    /// <summary>
    /// Performs a tuning using the internal network provider
    /// </summary>
    /// <param name="channel">Channel to tune</param>
    private void PerformInternalNetworkProviderTuning(IChannel channel)
    {
      this.LogDebug("dvb:Submit tunerequest calling put_TuneRequest");
      int hr = 0;
      int undefinedValue = -1;
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
        hr = _interfaceNetworkProvider.TuneDVBT(fSettings);
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
        hr = _interfaceNetworkProvider.TuneDVBS(fSettings, dSettings, lSettings, sSettings);
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

        hr = _interfaceNetworkProvider.TuneDVBC(fSettings, dSettings);
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

          hr = _interfaceNetworkProvider.TuneATSC((uint)undefinedValue, fSettings, dSettings);
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

          hr = _interfaceNetworkProvider.TuneATSC((uint)undefinedValue, fSettings, dSettings);
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
        return new DvbBaseScanning(this);
      }
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">Handle to the subchannel.</param>
    public override void FreeSubChannel(int id)
    {
      base.FreeSubChannel(id);
    }

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="channel">The service or channel to associate with the subchannel.</param>
    /// <returns>a handle for the subchannel</returns>
    protected override int CreateNewSubChannel(IChannel channel)
    {
      int id = _subChannelId++;
      this.LogInfo("TvCardDvbBase: new subchannel, ID = {0}, subchannel count = {1}", id, _mapSubChannels.Count);
      TvDvbChannel subChannel = new TvDvbChannel(id, this, _filterTsWriter, _filterTIF);
      subChannel.Parameters = Parameters;
      subChannel.CurrentChannel = channel;
      _mapSubChannels[id] = subChannel;
      FireNewSubChannelEvent(id);
      return id;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Build the BDA filter graph.
    /// </summary>
    public override void BuildGraph()
    {
      this.LogDebug("TvCardDvbBase: build graph");
      try
      {
        if (_isDeviceInitialised)
        {
          this.LogError("TvCardDvbBase: the graph is already built");
          throw new TvException("The graph is already built.");
        }

        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);

        AddNetworkProviderFilter();
        AddTsWriterFilterToGraph();
        if (!_useInternalNetworkProvider)
        {
          CreateTuningSpace();
          AddMpeg2DemuxerToGraph();
        }
        AddAndConnectBdaBoardFilters(_device);
        FilterGraphTools.SaveGraphFile(_graphBuilder, _device.Name + " - " + _tunerType + " Graph.grf");
        GetTunerSignalStatistics();
        _isDeviceInitialised = true;

        // Plugins can request to pause or start the device - other actions don't make sense here. The running
        // state is considered more compatible than the paused state, so start takes precedence.
        DeviceAction actualAction = DeviceAction.Default;
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          DeviceAction action;
          deviceInterface.OnInitialised(this, out action);
          if (action == DeviceAction.Pause)
          {
            if (actualAction == DeviceAction.Default)
            {
              this.LogDebug("TvCardDvbBase: plugin \"{0}\" will cause device pause", deviceInterface.Name);
              actualAction = DeviceAction.Pause;
            }
            else
            {
              this.LogDebug("TvCardDvbBase: plugin \"{0}\" wants to pause the device, overriden", deviceInterface.Name);
            }
          }
          else if (action == DeviceAction.Start)
          {
            this.LogDebug("TvCardDvbBase: plugin \"{0}\" will cause device start", deviceInterface.Name);
            actualAction = action;
          }
          else if (action != DeviceAction.Default)
          {
            this.LogDebug("TvCardDvbBase: plugin \"{0}\" wants unsupported action {1}", deviceInterface.Name, action);
          }
        }
        if (actualAction == DeviceAction.Start || _idleMode == DeviceIdleMode.AlwaysOn)
        {
          SetGraphState(FilterState.Running);
        }
        else if (actualAction == DeviceAction.Pause)
        {
          SetGraphState(FilterState.Paused);
        }
      }
      catch (Exception ex)
      {
        const string graphBuildingFailed = "Graph building failed.";
        this.LogError(ex, graphBuildingFailed);
        Dispose();
        _isDeviceInitialised = false;        
        throw new TvExceptionGraphBuildingFailed(graphBuildingFailed, ex);
      }
    }

    /// <summary>
    /// Create the BDA tuning space for the tuner. This will be used for BDA tuning.
    /// </summary>
    protected virtual void CreateTuningSpace()
    {
      // (Not abstract to avoid forcing the DVB-IP and SS2 classes to implement this.)
    }

    /// <summary>
    /// Add the appropriate BDA network provider filter to the graph.
    /// </summary>
    private void AddNetworkProviderFilter()
    {
      this.LogDebug("TvCardDvbBase: add network provider");

      string networkProviderName = String.Empty;
      if (_useInternalNetworkProvider)
      {
        networkProviderName = "MediaPortal Network Provider";
        Guid internalNetworkProviderClsId = new Guid("{D7D42E5C-EB36-4aad-933B-B4C419429C98}");
        this.LogDebug("TvCardDvbBase:   add {0}", networkProviderName);
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, internalNetworkProviderClsId,
                                                                     networkProviderName);
        _interfaceNetworkProvider = (IDvbNetworkProvider)_filterNetworkProvider;
        string hash = TvCardCollection.GetHash(DevicePath);
        _interfaceNetworkProvider.ConfigureLogging(TvCardCollection.GetFileName(DevicePath), hash,
                                                   LogLevelOption.Debug);
        return;
      }

      // If the generic network provider is preferred for this tuner then check if it is installed. The
      // generic NP is set as default, however it is only available on MCE 2005 Update Rollup 2 and newer.
      // We gracefully fall back to using one of the specific network providers if necessary.      
      Card c = CardManagement.GetCardByDevicePath(DevicePath, CardIncludeRelationEnum.None);
      if (((DbNetworkProvider)c.NetProvider) == DbNetworkProvider.Generic)
      {
        if (!FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID))
        {
          // The generic network provider is not installed. Fall back...
          if (_tunerType == CardType.DvbT)
          {
            c.NetProvider = (int)DbNetworkProvider.DvbT;
          }
          else if (_tunerType == CardType.DvbS)
          {
            c.NetProvider = (int)DbNetworkProvider.DvbS;
          }
          else if (_tunerType == CardType.Atsc)
          {
            c.NetProvider = (int)DbNetworkProvider.ATSC;
          }
          else if (_tunerType == CardType.DvbC)
          {
            c.NetProvider = (int)DbNetworkProvider.DvbC;
          }
          CardManagement.SaveCard(c);
        }
      }

      // Final selecion for Network provider based on user selection.
      Guid networkProviderClsId;
      switch ((DbNetworkProvider)c.NetProvider)
      {
        case DbNetworkProvider.DvbT:
          networkProviderName = "DVBT Network Provider";
          networkProviderClsId = typeof(DVBTNetworkProvider).GUID;
          break;
        case DbNetworkProvider.DvbS:
          networkProviderName = "DVBS Network Provider";
          networkProviderClsId = typeof(DVBSNetworkProvider).GUID;
          break;
        case DbNetworkProvider.DvbC:
          networkProviderName = "DVBC Network Provider";
          networkProviderClsId = typeof(DVBCNetworkProvider).GUID;
          break;
        case DbNetworkProvider.ATSC:
          networkProviderName = "ATSC Network Provider";
          networkProviderClsId = typeof(ATSCNetworkProvider).GUID;
          break;
        case DbNetworkProvider.Generic:
          networkProviderName = "Generic Network Provider";
          networkProviderClsId = typeof(NetworkProvider).GUID;
          break;
        default:
          // Tuning Space can also describe Analog TV but this application don't support them
          this.LogError("TvCardDvbBase: unrecognised tuner network provider setting {0}", c.NetProvider);
          throw new TvException("TvCardDvbBase: unrecognised tuner network provider setting");
      }
      this.LogDebug("TvCardDvbBase:   add {0}", networkProviderName);
      _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId,
                                                                   networkProviderName);
    }

    /// <summary>
    /// Finds the correct bda tuner/capture filters and adds them to the graph
    /// Creates a graph like
    /// [NetworkProvider]->[Tuner]->[Capture]->[...device filters...]->[TsWriter]
    /// or if no capture filter is present:
    /// [NetworkProvider]->[Tuner]->[...device filters...]->[TsWriter]
    /// </summary>
    /// <param name="device">Tuner device</param>
    protected void AddAndConnectBdaBoardFilters(DsDevice device)
    {
      this.LogDebug("dvb:AddAndConnectBDABoardFilters");
      _rotEntry = new DsROTEntry(_graphBuilder);
      this.LogDebug("dvb: find bda tuner");
      // Enumerate BDA Source filters category and found one that can connect to the network provider
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        if (device.DevicePath != devices[i].DevicePath)
          continue;
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          this.LogInfo("dvb:  [Tuner]: {0} is being used by TVServer already or another application!", devices[i].Name);
          continue;
        }
        int hr;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          continue;
        }
        if (hr != 0)
        {
          if (tmp != null)
          {
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("bda tuner", tmp);
          }
          continue;
        }
        //render [Network provider]->[Tuner]
        hr = _capBuilder.RenderStream(null, null, _filterNetworkProvider, null, tmp);
        if (hr == 0)
        {
          // Got it !
          _filterTuner = tmp;
          this.LogDebug("dvb:  using [Tuner]: {0}", devices[i].Name);
          _tunerDevice = devices[i];
          DevicesInUse.Instance.Add(devices[i]);
          this.LogDebug("dvb:  Render [Network provider]->[Tuner] OK");
          break;
        }
        // Try another...
        _graphBuilder.RemoveFilter(tmp);
        Release.ComObject("bda tuner", tmp);
      }
      // Assume we found a tuner filter...
      if (_filterTuner == null)
      {
        this.LogInfo(
          "dvb:  A useable TV Tuner cannot be found! Either the device no longer exists or it's in use by another application!");
        this.LogError("dvb:  No TVTuner installed");
        throw new TvException("No TVTuner installed");
      }

      this.LogDebug("dvb:  Setting lastFilter to Tuner filter");
      IBaseFilter lastFilter = _filterTuner;

      // Attempt to connect [Tuner]->[Capture]
      if (UseCaptureFilter())
      {
        this.LogDebug("dvb:  Find BDA receiver");
        this.LogDebug("dvb:  match Capture by Tuner device path");
        AddBDARendererToGraph(device, ref lastFilter, true);
        if (_filterCapture == null)
        {
          this.LogDebug("dvb:  Match by device path failed - trying alternative method");
          this.LogDebug("dvb:  match Capture filter by Tuner device connection");
          AddBDARendererToGraph(device, ref lastFilter, false);
        }
      }

      // Check for and load plugins, adding any additional device filters to the graph.
      LoadPlugins(_filterTuner, ref lastFilter);

      // Now connect the required filters if not using the internal network provider
      if (!_useInternalNetworkProvider)
      {
        // Connect the inf tee and demux to the last filter (saves one inf tee)
        AddInfiniteTeeToGraph(ref lastFilter);
        ConnectMpeg2DemuxerIntoGraph(ref lastFilter);
        // Connect and add the filters to the demux
        AddTransportInformationFilterToGraph();
      }
      // Render the last filter with the tswriter
      ConnectTsWriterIntoGraph(lastFilter);

      // Open any plugins we found. This is separated from loading because some plugins can't be opened
      // until the graph has finished being built.
      OpenPlugins();
    }

    /// <summary>
    /// Determine whether the tuner filter needs to connect to a capture filter,
    /// or whether it can be directly connected to an inf tee.
    /// </summary>
    /// <returns><c>true</c> if the tuner filter must be connected to a capture filter, otherwise <c>false</c></returns>
    private bool UseCaptureFilter()
    {
      // First: check the media types and formats on the tuner output
      // pin. The WDK specifies (http://msdn.microsoft.com/en-us/library/ff557729%28v=vs.85%29.aspx)
      // a set of formats that the tuner and capture filter output pins should set
      // to allow the tuner filter to connect to the capture filter, and the capture
      // filter to connect to the MPEG 2 Demultiplexor filter (see http://msdn.microsoft.com/en-us/library/dd390716%28v=vs.85%29.aspx).
      // Most tuners support the MEDIASUBTYPE_MPEG2_TRANSPORT media sub-type on their
      // output pin, while most capture filters use either MEDIASUBTYPE_MPEG2_TRANSPORT
      // or STATIC_KSDATAFORMAT_SUBTYPE_BDA_MPEG2_TRANSPORT (also known as MEDIASUBTYPE_BDA_MPEG2_TRANSPORT).
      bool useCaptureFilter = true;
      IPin pinOut = DsFindPin.ByDirection(_filterTuner, PinDirection.Output, 0);
      if (pinOut != null)
      {
        IEnumMediaTypes enumMedia;
        pinOut.EnumMediaTypes(out enumMedia);
        if (enumMedia != null)
        {
          int fetched;
          AMMediaType[] mediaTypes = new AMMediaType[21];
          enumMedia.Next(20, mediaTypes, out fetched);
          if (fetched > 0)
          {
            for (int i = 0; i < fetched; ++i)
            {
              if (mediaTypes[i].majorType == MediaType.Stream && mediaTypes[i].subType == MpMediaSubType.BdaMpeg2Transport &&
                  mediaTypes[i].formatType == FormatType.None)
              {
                this.LogDebug("dvb:  tuner filter has capture filter output");
                useCaptureFilter = false;
              }
              DsUtils.FreeAMMediaType(mediaTypes[i]);
            }
          }
          Release.ComObject("tuner filter output pin media types enum", enumMedia);
        }
      }
      Release.ComObject("tuner filter output pin", pinOut);
      if (!useCaptureFilter)
      {
        return false;
      }

      // Second: check whether the tuner filter implements the
      // capture/receiver filter interface (KSCATEGORY_BDA_RECEIVER_COMPONENT).
      // NOTE: not all filters expose this information.
      IKsTopologyInfo topologyInfo = _filterTuner as IKsTopologyInfo;
      if (topologyInfo != null)
      {
        int categoryCount;
        topologyInfo.get_NumCategories(out categoryCount);
        for (int i = 0; i < categoryCount; i++)
        {
          Guid guid;
          topologyInfo.get_Category(i, out guid);
          if (guid == FilterCategory.BDAReceiverComponentsCategory)
          {
            this.LogDebug("dvb:  tuner filter is also capture filter");
            return false;
          }
        }
      }

      // Finally: if the tuner is a Silicondust HDHomeRun then a capture
      // filter is not required. Neither of the other two detection methods
      // work as of 2011-02-11 (mm1352000).
      if (_tunerDevice.Name.Contains("Silicondust HDHomeRun Tuner"))
      {
        return false;
      }

      // Default: capture filter is required.
      return true;
    }

    /// <summary>
    /// adds the BDA renderer filter to the graph by elimination
    /// then tries to match tuner &amp; render filters if successful then connects them.
    /// </summary>
    /// <param name="device">Tuner device</param>
    /// <param name="currentLastFilter">The current last filter if we add multiple captures</param>
    /// <param name="matchDevicePath">If <c>true</c> only attempt to use renderer filters on the same physical device as the tuner device.</param>
    protected void AddBDARendererToGraph(DsDevice device, ref IBaseFilter currentLastFilter, bool matchDevicePath)
    {
      if (_filterCapture != null)
        return;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
      const string guidBdaMPEFilter = @"\{8e60217d-a2ee-47f8-b0c5-0f44c55f66dc}";
      const string guidBdaSlipDeframerFilter = @"\{03884cb6-e89a-4deb-b69e-8dc621686e6a}";
      for (int i = 0; i < devices.Length; i++)
      {
        if (devices[i].DevicePath.ToUpperInvariant().IndexOf(guidBdaMPEFilter.ToUpperInvariant()) >= 0)
          continue;
        if (devices[i].DevicePath.ToUpperInvariant().IndexOf(guidBdaSlipDeframerFilter.ToUpperInvariant()) >= 0)
          continue;
        IBaseFilter tmp;
        const string deviceIdDelimter = @"#{";
        this.LogDebug("dvb:  -{0}", devices[i].Name);
        //Make sure the BDA Receiver Component is on the same physical device as the BDA Source Filter.
        //This is done by checking the DeviceId and DeviceInstance part of the DevicePath.
        if (matchDevicePath)
        {
          int indx1 = device.DevicePath.IndexOf(deviceIdDelimter);
          int indx2 = devices[i].DevicePath.IndexOf(deviceIdDelimter);
          if (indx1 < 0 || indx2 < 0)
          {
            continue;
          }

          if (device.DevicePath.Remove(indx1) != devices[i].DevicePath.Remove(indx2))
          {
            continue;
          }
        }
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        int hr;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          continue;
        }
        if (hr != 0)
        {
          if (tmp != null)
          {
            this.LogError("dvb:  Failed to add bda receiver: {0}. Is it in use?", devices[i].Name);
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("bda receiver", tmp);
          }
          continue;
        }
        //render [Tuner]->[Capture]
        hr = _capBuilder.RenderStream(null, null, _filterTuner, null, tmp);
        if (hr == 0)
        {
          this.LogDebug("dvb:  Render [Tuner]->[Capture] AOK");
          // render [Capture]->[Inf Tee]
          _filterCapture = tmp;
          _captureDevice = devices[i];
          DevicesInUse.Instance.Add(devices[i]);
          this.LogDebug("dvb:  Setting lastFilter to Capture device");
          currentLastFilter = _filterCapture;
          break;
        }
        // Try another...
        this.LogDebug("dvb:  Looking for another bda receiver...");
        _graphBuilder.RemoveFilter(tmp);
        Release.ComObject("bda receiver", tmp);
      }
    }

    /// <summary>
    /// Add an MPEG 2 demultiplexer filter to the BDA filter graph.
    /// </summary>
    protected void AddMpeg2DemuxerToGraph()
    {
      this.LogDebug("TvCardDvbBase: add MPEG 2 demultiplexer filter");
      _filterMpeg2DemuxTif = (IBaseFilter)new MPEG2Demultiplexer();
      int hr = _graphBuilder.AddFilter(_filterMpeg2DemuxTif, "MPEG 2 Demultiplexer");
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to add MPEG 2 demultiplexer, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionGraphBuildingFailed("TvCardDvbBase: failed to add MPEG 2 demultiplexer");
      }
    }

    /// <summary>
    /// Connect the MPEG 2 demultiplexer into the BDA filter graph.
    /// </summary>
    /// <param name="lastFilter">The filter in the filter chain that the demultiplexer should be connected to.</param>
    protected void ConnectMpeg2DemuxerIntoGraph(ref IBaseFilter lastFilter)
    {
      this.LogDebug("TvCardDvbBase: connect MPEG 2 demultiplexer filter");
      IPin infTeeOut = DsFindPin.ByDirection(_infTee, PinDirection.Output, 0);
      IPin demuxPinIn = DsFindPin.ByDirection(_filterMpeg2DemuxTif, PinDirection.Input, 0);
      int hr = _graphBuilder.Connect(infTeeOut, demuxPinIn);
      Release.ComObject("Infinite tee output pin", infTeeOut);
      Release.ComObject("MPEG 2 demux input pin", demuxPinIn);
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to connect MPEG 2 demultiplexer, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionGraphBuildingFailed("TvCardDvbBase: failed to connect MPEG 2 demultiplexer");
      }
    }

    /// <summary>
    /// Add and connect an infinite tee into the BDA filter graph.
    /// </summary>
    /// <param name="lastFilter">The filter in the filter chain that the infinite tee should be connected to.</param>
    protected virtual void AddInfiniteTeeToGraph(ref IBaseFilter lastFilter)
    {
      this.LogDebug("TvCardDvbBase: add infinite tee filter");
      _infTee = (IBaseFilter)new InfTee();
      int hr = _graphBuilder.AddFilter(_infTee, "Infinite Tee");
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to add infinite tee, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionGraphBuildingFailed("TvCardDvbBase: failed to add infinite tee");
      }
      this.LogDebug("TvCardDvbBase:   render...->[inf tee]");
      hr = _capBuilder.RenderStream(null, null, lastFilter, null, _infTee);
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to render stream through the infinite tee, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionGraphBuildingFailed("TvCardDvbBase: failed to render stream through the infinite tee");
      }
      lastFilter = _infTee;
    }

    /// <summary>
    /// Add the MediaPortal TS writer/analyser filter to the BDA filter graph.
    /// </summary>
    protected void AddTsWriterFilterToGraph()
    {
      this.LogDebug("TvCardDvbBase: add Mediaportal TsWriter filter");

      _filterTsWriter = FilterLoader.LoadFilterFromDll("TsWriter.ax", typeof(MpTsAnalyzer).GUID, true);
      if (_filterTsWriter == null)
      {
        this.LogError("TvCardDvbBase: failed to marshal TsWriter filter");
        throw new TvExceptionGraphBuildingFailed("TvCardDvbBase: failed to marshal TsWriter filter");
      }
      int hr = _graphBuilder.AddFilter(_filterTsWriter, "MediaPortal TS Analyzer");
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to add TsWriter filter, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionGraphBuildingFailed("TvCardDvbBase: failed to add TsWriter filter");
      }
      _interfaceChannelScan = (ITsChannelScan)_filterTsWriter;
      _interfaceEpgGrabber = (ITsEpgScanner)_filterTsWriter;
      _interfaceChannelLinkageScanner = (ITsChannelLinkageScanner)_filterTsWriter;
    }

    /// <summary>
    /// Connect the MediaPortal TS writer/analyser filter into the BDA filter graph, completing the graph.
    /// </summary>
    /// <param name="lastFilter">The filter in the filter chain that the TsWriter filter should be connected to.</param>
    protected void ConnectTsWriterIntoGraph(IBaseFilter lastFilter)
    {
      this.LogDebug("TvCardDvbBase: connect Mediaportal TsWriter filter");
      this.LogDebug("TvCardDvbBase:   render...->[TsWriter]");
      int hr = _capBuilder.RenderStream(null, null, lastFilter, null, _filterTsWriter);
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to render stream into the TsWriter filter, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionGraphBuildingFailed("TvCardDvbBase: failed to render stream into the TsWriter filter");
      }
    }

    /// <summary>
    /// Add and connect a transport information filter into the BDA filter graph.
    /// </summary>
    protected void AddTransportInformationFilterToGraph()
    {
      this.LogDebug("TvCardDvbBase: add transport information filter");
      // No point bothering with anything if the demuxer is not present to connect to.
      if (_filterMpeg2DemuxTif == null)
      {
        this.LogError("TvCardDvbBase: MPEG 2 demultiplexer is null");
        return;
      }

      // Add the filter to the graph.
      int hr = 1;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        if (String.Compare(devices[i].Name, "BDA MPEG2 Transport Information Filter", true) == 0)
        {
          try
          {
            hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out _filterTIF);
            if (hr == 0)
            {
              break;  // Success!
            }
          }
          catch (Exception)
          {
          }
          this.LogError("TvCardDvbBase: failed to add transport information filter, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return; // Not a critical error...
        }
      }
      if (_filterTIF == null)
      {
        this.LogError("TvCardDvbBase: transport information filter not found");
        return;
      }

      // Connect the filter into the graph.
      IPin pinInTif = DsFindPin.ByDirection(_filterTIF, PinDirection.Input, 0);
      if (pinInTif == null)
      {
        this.LogError("TvCardDvbBase: failed to find transport information filter input pin");
        return;
      }
      bool tifConnected = false;
      try
      {
        IEnumPins enumPins;
        _filterMpeg2DemuxTif.EnumPins(out enumPins);
        if (enumPins == null)
        {
          this.LogError("TvCardDvbBase: MPEG 2 demultiplexer has not sprouted pins");
          return;
        }
        int pinNr = 0;
        while (true)
        {
          pinNr++;
          PinDirection pinDir;
          AMMediaType[] mediaTypes = new AMMediaType[2];
          IPin[] pins = new IPin[2];
          int fetched;
          enumPins.Next(1, pins, out fetched);
          if (fetched != 1 || pins[0] == null)
          {
            break;
          }
          pins[0].QueryDirection(out pinDir);
          if (pinDir == PinDirection.Input)
          {
            Release.ComObject("MPEG 2 demux input pin " + pinNr, pins[0]);
            continue;
          }
          IEnumMediaTypes enumMedia;
          pins[0].EnumMediaTypes(out enumMedia);
          if (enumMedia != null)
          {
            enumMedia.Next(1, mediaTypes, out fetched);
            Release.ComObject("MPEG 2 demux output pin media type enum", enumMedia);
            if (fetched == 1 && mediaTypes[0] != null)
            {
              if (mediaTypes[0].majorType == MediaType.Audio || mediaTypes[0].majorType == MediaType.Video)
              {
                // We're not interested in audio or video pins.
                DsUtils.FreeAMMediaType(mediaTypes[0]);
                Release.ComObject("MPEG 2 demux output pin " + pinNr, pins[0]);
                continue;
              }
            }
            DsUtils.FreeAMMediaType(mediaTypes[0]);
          }
          try
          {
            hr = _graphBuilder.Connect(pins[0], pinInTif);
            if (hr == 0)
            {
              tifConnected = true;
              break;
            }
          }
          catch (Exception ex)
          {
            this.LogError(ex, "TvCardDvbBase: exception on connect attempt");
          }
          finally
          {
            Release.ComObject("MPEG 2 demux output pin " + pinNr, pins[0]);
          }
        }
        Release.ComObject("MPEG 2 demux pin enum", enumPins);
      }
      finally
      {
        Release.ComObject("TIF input pin", pinInTif);
      }
      this.LogDebug("TvCardDvbBase: result = {0}", tifConnected);
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
      Decompose();
    }

    /// <summary>
    /// destroys the graph and cleans up any resources
    /// </summary>
    protected void Decompose()
    {
      if (_graphBuilder == null)
        return;

      this.LogDebug("dvb:Decompose");
      if (_epgGrabbing)
      {
        if (_epgGrabberCallback != null && _epgGrabbing)
        {
          this.LogInfo("dvb:cancel epg->decompose");
          _epgGrabberCallback.OnEpgCancelled();
        }
        _epgGrabbing = false;
      }

      FreeAllSubChannels();
      this.LogDebug("  stop");
      // Decompose the graph

      int counter = 0, hr = 0;
      FilterState state = FilterState.Running;
      hr = ((IMediaControl)_graphBuilder).Stop();
      while (state != FilterState.Stopped)
      {
        System.Threading.Thread.Sleep(100);
        hr = ((IMediaControl)_graphBuilder).GetState(10, out state);
        counter++;
        if (counter >= 30)
        {
          if (state != FilterState.Stopped)
            this.LogError("dvb:graph still running");
          break;
        }
      }

      base.Dispose();

      this.LogDebug("  free...");
      _interfaceChannelScan = null;
      _interfaceEpgGrabber = null;
      _previousChannel = null;
      if (_filterMpeg2DemuxTif != null)
      {
        Release.ComObject("_filterMpeg2DemuxTif filter", _filterMpeg2DemuxTif);
        _filterMpeg2DemuxTif = null;
      }
      if (_filterNetworkProvider != null)
      {
        Release.ComObject("_filterNetworkProvider filter", _filterNetworkProvider);
        _filterNetworkProvider = null;
      }
      if (_infTee != null)
      {
        Release.ComObject("main inftee filter", _infTee);
        _infTee = null;
      }
      if (_filterTuner != null)
      {
        while (Release.ComObject(_filterTuner) > 0)
          ;
        _filterTuner = null;
      }
      if (_filterCapture != null)
      {
        while (Release.ComObject(_filterCapture) > 0)
          ;
        _filterCapture = null;
      }
      if (_filterTIF != null)
      {
        Release.ComObject("TIF filter", _filterTIF);
        _filterTIF = null;
      }
      this.LogDebug("  free pins...");
      if (_filterTsWriter as IBaseFilter != null)
      {
        Release.ComObject("TSWriter filter", _filterTsWriter);
        _filterTsWriter = null;
      }
      else
      {
        this.LogDebug("!!! Error releasing TSWriter filter (_filterTsWriter as IBaseFilter was null!)");
        _filterTsWriter = null;
      }
      this.LogDebug("  free graph...");
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
        _rotEntry = null;
      }
      if (_capBuilder != null)
      {
        Release.ComObject("capture builder", _capBuilder);
        _capBuilder = null;
      }
      if (_graphBuilder != null)
      {
        FilterGraphTools.RemoveAllFilters(_graphBuilder);
        Release.ComObject("graph builder", _graphBuilder);
        _graphBuilder = null;
      }
      this.LogDebug("  free devices...");
      if (_tunerDevice != null)
      {
        DevicesInUse.Instance.Remove(_tunerDevice);
        _tunerDevice = null;
      }
      if (_captureDevice != null)
      {
        DevicesInUse.Instance.Remove(_captureDevice);
        _captureDevice = null;
      }
      if (_tunerStatistics != null)
      {
        for (int i = 0; i < _tunerStatistics.Count; i++)
        {
          IBDA_SignalStatistics stat = _tunerStatistics[i];
          while (Release.ComObject(stat) > 0)
            ;
        }
        _tunerStatistics.Clear();
      }
      this.LogDebug("  decompose done...");
      _isDeviceInitialised = false;
    }

    #endregion

    #region signal quality, level etc

    /// <summary>
    /// this method gets the signal statistics interfaces from the bda tuner device
    /// and stores them in _tunerStatistics
    /// </summary>
    protected void GetTunerSignalStatistics()
    {
      this.LogDebug("dvb: GetTunerSignalStatistics()");
      //no tuner filter? then return;
      _tunerStatistics = new List<IBDA_SignalStatistics>();
      if (_filterTuner == null)
      {
        this.LogError("dvb: could not get IBDA_Topology since no tuner device");
        return;
      }
      //get the IBDA_Topology from the tuner device
      //Log.this.LogDebug("dvb: get IBDA_Topology");
      IBDA_Topology topology = _filterTuner as IBDA_Topology;
      if (topology == null)
      {
        this.LogError("dvb: could not get IBDA_Topology from tuner");
        return;
      }
      //get the NodeTypes from the topology
      //Log.this.LogDebug("dvb: GetNodeTypes");
      int nodeTypeCount;
      int[] nodeTypes = new int[33];
      Guid[] guidInterfaces = new Guid[33];
      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      if (hr != 0)
      {
        this.LogError("dvb: FAILED could not get node types from tuner:0x{0:X}", hr);
        return;
      }
      if (nodeTypeCount == 0)
      {
        this.LogError("dvb: FAILED could not get any node types");
      }
      Guid GuidIBDA_SignalStatistic = new Guid("1347D106-CF3A-428a-A5CB-AC0D9A2A4338");
      //for each node type
      //Log.this.LogDebug("dvb: got {0} node types", nodeTypeCount);
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        object objectNode;
        int numberOfInterfaces;
        hr = topology.GetNodeInterfaces(nodeTypes[i], out numberOfInterfaces, 32, guidInterfaces);
        if (hr != 0)
        {
          this.LogError("dvb: FAILED could not GetNodeInterfaces for node:{0} 0x:{1:X}", i, hr);
        }
        hr = topology.GetControlNode(0, 1, nodeTypes[i], out objectNode);
        if (hr != 0)
        {
          this.LogError("dvb: FAILED could not GetControlNode for node:{0} 0x:{1:X}", i, hr);
          return;
        }
        //and get the final IBDA_SignalStatistics
        for (int iface = 0; iface < numberOfInterfaces; iface++)
        {
          if (guidInterfaces[iface] == GuidIBDA_SignalStatistic)
          {
            //this.LogDebug(" got IBDA_SignalStatistics on node:{0} interface:{1}", i, iface);
            _tunerStatistics.Add((IBDA_SignalStatistics)objectNode);
          }
        }
      }
    }

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
        if (!GraphRunning() ||
          CurrentChannel == null ||
          _tunerStatistics == null ||
          _tunerStatistics.Count == 0)
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
        for (int i = 0; i < _tunerStatistics.Count; i++)
        {
          IBDA_SignalStatistics stat = _tunerStatistics[i];
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
        if (_tunerStatistics.Count > 0)
        {
          _signalQuality = (int)signalQuality / _tunerStatistics.Count;
          _signalLevel = (int)signalStrength / _tunerStatistics.Count;
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

    #region properties

    /// <summary>
    /// returns the ITsChannelScan interface for the graph
    /// </summary>
    public ITsChannelScan StreamAnalyzer
    {
      get { return _interfaceChannelScan; }
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
      _interfaceChannelLinkageScanner.SetCallBack(callback);
      _interfaceChannelLinkageScanner.Start();
    }

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public override void ResetLinkageScanner()
    {
      _interfaceChannelLinkageScanner.Reset();
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public override List<PortalChannel> ChannelLinkages
    {
      get
      {
        try
        {
          uint channelCount;
          List<PortalChannel> portalChannels = new List<PortalChannel>();
          _interfaceChannelLinkageScanner.GetChannelCount(out channelCount);
          if (channelCount == 0)
            return portalChannels;
          for (uint i = 0; i < channelCount; i++)
          {
            ushort network_id = 0;
            ushort transport_id = 0;
            ushort service_id = 0;
            _interfaceChannelLinkageScanner.GetChannel(i, ref network_id, ref transport_id, ref service_id);
            PortalChannel pChannel = new PortalChannel();
            pChannel.NetworkId = network_id;
            pChannel.TransportId = transport_id;
            pChannel.ServiceId = service_id;
            uint linkCount;
            _interfaceChannelLinkageScanner.GetLinkedChannelsCount(i, out linkCount);
            if (linkCount > 0)
            {
              for (uint j = 0; j < linkCount; j++)
              {
                ushort nid = 0;
                ushort tid = 0;
                ushort sid = 0;
                IntPtr ptrName;
                _interfaceChannelLinkageScanner.GetLinkedChannel(i, j, ref nid, ref tid, ref sid, out ptrName);
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
          _interfaceChannelLinkageScanner.Reset();
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
      if (_interfaceEpgGrabber == null)
        return;
      _interfaceEpgGrabber.SetCallBack(callback);
      _interfaceEpgGrabber.GrabEPG();
      _interfaceEpgGrabber.GrabMHW();
      _epgGrabbing = true;
    }

    /// <summary>
    /// Start grabbing the epg while timeshifting
    /// </summary>
    public override void GrabEpg()
    {
      if (_timeshiftingEPGGrabber.StartGrab())
        GrabEpg(_timeshiftingEPGGrabber);
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
        _interfaceEpgGrabber.Reset();
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
      if (_interfaceEpgGrabber != null)
        _interfaceEpgGrabber.AbortGrabbing();
      if (_timeshiftingEPGGrabber != null)
        _timeshiftingEPGGrabber.OnEpgCancelled();
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
          bool dvbReady, mhwReady;
          _interfaceEpgGrabber.IsEPGReady(out dvbReady);
          _interfaceEpgGrabber.IsMHWReady(out mhwReady);
          if (dvbReady == false || mhwReady == false)
            return null;
          uint titleCount;
          uint channelCount;
          _interfaceEpgGrabber.GetMHWTitleCount(out titleCount);
          mhwReady = titleCount > 10;
          _interfaceEpgGrabber.GetEPGChannelCount(out channelCount);
          dvbReady = channelCount > 0;
          List<EpgChannel> epgChannels = new List<EpgChannel>();
          this.LogInfo("dvb:mhw ready MHW {0} titles found", titleCount);
          this.LogInfo("dvb:dvb ready.EPG {0} channels", channelCount);
          if (mhwReady)
          {
            _interfaceEpgGrabber.GetMHWTitleCount(out titleCount);
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
              _interfaceEpgGrabber.GetMHWTitle((ushort)i, ref id, ref tmp1, ref tmp2, ref channelnr, ref programid,
                                               ref themeid, ref PPV, ref summaries, ref duration, ref datestart,
                                               ref timestart, out ptrTitle, out ptrProgramName);
              _interfaceEpgGrabber.GetMHWChannel(channelnr, ref channelid, ref networkid, ref transportid,
                                                 out ptrChannelName);
              _interfaceEpgGrabber.GetMHWSummary(programid, out ptrSummary);
              _interfaceEpgGrabber.GetMHWTheme(themeid, out ptrTheme);
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
            _interfaceEpgGrabber.Reset();
            return epgChannels;
          }

          if (dvbReady)
          {
            ushort networkid = 0;
            ushort transportid = 0;
            ushort serviceid = 0;
            for (uint x = 0; x < channelCount; ++x)
            {
              _interfaceEpgGrabber.GetEPGChannel(x, ref networkid, ref transportid, ref serviceid);
              // We need to use a matching channel type per card, because tuning details will be looked up with cardtype as filter.
              DVBBaseChannel channel = CreateChannel();
              channel.NetworkId = networkid;
              channel.TransportId = transportid;
              channel.ServiceId = serviceid;
              EpgChannel epgChannel = new EpgChannel { Channel = channel };
              uint eventCount;
              _interfaceEpgGrabber.GetEPGEventCount(x, out eventCount);
              for (uint i = 0; i < eventCount; ++i)
              {
                uint start_time_MJD, start_time_UTC, duration, languageCount;
                string title, description;
                IntPtr ptrGenre;
                int starRating;
                IntPtr ptrClassification;

                _interfaceEpgGrabber.GetEPGEvent(x, i, out languageCount, out start_time_MJD, out start_time_UTC,
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
                    _interfaceEpgGrabber.GetEPGLanguage(x, i, (uint)z, out languageId, out ptrTitle, out ptrDesc,
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
          _interfaceEpgGrabber.Reset();
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
      Setting setting = SettingsManagement.GetSetting("generalGrapOnlyForSameTransponder", "no");

      if (setting != null && !setting.Value.Equals("yes"))
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
          return CurrentChannel.IsDifferentTransponder(channel);
        }
      }
      return false;
    }

    #endregion
  }
}