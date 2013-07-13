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
  /// base class for DVB cards
  /// </summary>
  public abstract class TvCardDvbBase : DeviceDirectShow, IDisposable, ITVCard
  {
    #region variables

    /// <summary>
    /// A pre-configured tuning space, used to speed up the tuning process. 
    /// </summary>
    private ITuningSpace _tuningSpace = null;

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
    /// Internal Network provider instance
    /// </summary>
    protected IDvbNetworkProvider _interfaceNetworkProvider;

    /// <summary>
    /// Indicates if the internal network provider is used
    /// </summary>
    protected bool _useInternalNetworkProvider;

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
      Guid networkProviderClsId = new Guid("{D7D42E5C-EB36-4aad-933B-B4C419429C98}");
      _useInternalNetworkProvider = FilterGraphTools.IsThisComObjectInstalled(networkProviderClsId);
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

      if (_useCustomTuning)
      {
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          ICustomTuner customTuner = deviceInterface as ICustomTuner;
          if (customTuner != null && customTuner.CanTuneChannel(channel))
          {
            this.LogDebug("TvCardDvbBase: using custom tuning");
            if (customTuner.Tune(channel))
            {
              return;
            }
            this.LogWarn("TvCardDvbBase: custom tuning failed, falling back to BDA tuning");
          }
        }
      }

      this.LogDebug("TvCardDvbBase: using BDA tuning");
      ITuneRequest tuneRequest = AssembleTuneRequest(_tuningSpace, channel);
      this.LogDebug("TvCardDvbBase: calling put_TuneRequest");
      int hr = (_filterNetworkProvider as ITuner).put_TuneRequest(tuneRequest);
      this.LogDebug("TvCardDvbBase: put_TuneRequest returned, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      Release.ComObject("base DVB tuner tune request", ref tuneRequest);

      // TerraTec tuners return a positive HRESULT value when already tuned with the required
      // parameters. See mantis 3469 for more details.
      if (hr < 0)
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
      return new TvDvbChannel(id, this, _filterTsWriter, _filterTIF);
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
      AddNetworkProviderFilter();
      if (!_useInternalNetworkProvider && _filterNetworkProvider != null)
      {
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
        int hr = tuner.put_TuningSpace(_tuningSpace);
        HResult.ThrowException(hr, "Failed to put_TuningSpace() on ITuner.");

        AddMpeg2DemuxerToGraph();
      }
      AddAndConnectBdaBoardFilters(_device);
      FilterGraphTools.SaveGraphFile(_graph, _device.Name + " - " + _tunerType + " Graph.grf");
      GetTunerSignalStatistics();
    }

    /// <summary>
    /// The registered name of BDA tuning space for the device.
    /// </summary>
    protected abstract string TuningSpaceName
    {
      get;
    }

    /// <summary>
    /// Get the registered BDA tuning space for the device if it exists.
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
    /// Create and register the BDA tuning space for the device.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected abstract ITuningSpace CreateTuningSpace();

    /// <summary>
    /// Add the appropriate BDA network provider filter to the graph.
    /// </summary>
    private void AddNetworkProviderFilter()
    {
      this.LogDebug("TvCardDvbBase: add network provider");

      string networkProviderName = string.Empty;
      if (_useInternalNetworkProvider)
      {
        networkProviderName = "MediaPortal Network Provider";
        Guid internalNetworkProviderClsId = new Guid("{D7D42E5C-EB36-4aad-933B-B4C419429C98}");
        this.LogDebug("TvCardDvbBase:   add {0}", networkProviderName);
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graph, internalNetworkProviderClsId,
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
      _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graph, networkProviderClsId,
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
      _filterTuner = null;
      // Enumerate BDA Source filters category and found one that can connect to the network provider
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        if (device.DevicePath != devices[i].DevicePath)
          continue;
        if (!DevicesInUse.Instance.Add(devices[i]))
        {
          this.LogInfo("dvb:  [Tuner]: {0} is being used by TVServer already or another application!", devices[i].Name);
          continue;
        }
        try
        {
          int hr;
          try
          {
            hr = _graph.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
          }
          catch (Exception)
          {
            continue;
          }
          if (hr != 0)
          {
            if (tmp != null)
            {
              _graph.RemoveFilter(tmp);
              Release.ComObject("base DVB tuner tuner filter candidate", ref tmp);
            }
            continue;
          }
          //render [Network provider]->[Tuner]
          hr = _captureGraphBuilder.RenderStream(null, null, _filterNetworkProvider, null, tmp);
          if (hr == 0)
          {
            // Got it !
            _filterTuner = tmp;
            this.LogDebug("dvb:  using [Tuner]: {0}", devices[i].Name);
            this.LogDebug("dvb:  Render [Network provider]->[Tuner] OK");
            break;
          }
        }
        finally
        {
          if (_filterTuner == null)
          {
            DevicesInUse.Instance.Remove(devices[i]);
          }
        }
        // Try another...
        _graph.RemoveFilter(tmp);
        Release.ComObject("base DVB tuner tuner filter candidate", ref tmp);
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
      AddTsWriterToGraph(lastFilter);
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
              Release.AmMediaType(ref mediaTypes[i]);
            }
          }
          Release.ComObject("base DVB tuner tuner filter output pin media type enumerator", ref enumMedia);
        }
      }
      Release.ComObject("base DVB tuner tuner filter output pin", ref pinOut);
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
      if (_device.Name.Contains("Silicondust HDHomeRun Tuner"))
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
        if (!DevicesInUse.Instance.Add(devices[i]))
        {
          continue;
        }
        int hr;
        try
        {
          try
          {
            hr = _graph.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
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
              _graph.RemoveFilter(tmp);
              Release.ComObject("base DVB tuner capture filter candidate", ref tmp);
            }
            continue;
          }
          //render [Tuner]->[Capture]
          hr = _captureGraphBuilder.RenderStream(null, null, _filterTuner, null, tmp);
          if (hr == 0)
          {
            this.LogDebug("dvb:  Render [Tuner]->[Capture] AOK");
            // render [Capture]->[Inf Tee]
            _filterCapture = tmp;
            _captureDevice = devices[i];
            this.LogDebug("dvb:  Setting lastFilter to Capture device");
            currentLastFilter = _filterCapture;
            break;
          }
        }
        finally
        {
          if (_filterCapture == null)
          {
            DevicesInUse.Instance.Remove(devices[i]);
          }
        }

        // Try another...
        this.LogDebug("dvb:  Looking for another bda receiver...");
        _graph.RemoveFilter(tmp);
        Release.ComObject("base DVB tuner capture filter candidate", ref tmp);
      }
    }

    /// <summary>
    /// Add an MPEG 2 demultiplexer filter to the BDA filter graph.
    /// </summary>
    protected void AddMpeg2DemuxerToGraph()
    {
      this.LogDebug("TvCardDvbBase: add MPEG 2 demultiplexer filter");
      _filterMpeg2DemuxTif = (IBaseFilter)new MPEG2Demultiplexer();
      int hr = _graph.AddFilter(_filterMpeg2DemuxTif, "MPEG 2 Demultiplexer");
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to add MPEG 2 demultiplexer, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionDeviceLoadFailed("TvCardDvbBase: failed to add MPEG 2 demultiplexer");
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
      int hr = _graph.Connect(infTeeOut, demuxPinIn);
      Release.ComObject("base DVB tuner infinite tee output pin", ref infTeeOut);
      Release.ComObject("base DVB tuner MPEG 2 demultiplexer input pin", ref demuxPinIn);
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to connect MPEG 2 demultiplexer, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionDeviceLoadFailed("TvCardDvbBase: failed to connect MPEG 2 demultiplexer");
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
      int hr = _graph.AddFilter(_infTee, "Infinite Tee");
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to add infinite tee, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionDeviceLoadFailed("TvCardDvbBase: failed to add infinite tee");
      }
      this.LogDebug("TvCardDvbBase:   render...->[inf tee]");
      hr = _captureGraphBuilder.RenderStream(null, null, lastFilter, null, _infTee);
      if (hr != 0)
      {
        this.LogError("TvCardDvbBase: failed to render stream through the infinite tee, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvExceptionDeviceLoadFailed("TvCardDvbBase: failed to render stream through the infinite tee");
      }
      lastFilter = _infTee;
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
            hr = _graph.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out _filterTIF);
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
        try
        {
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
            try
            {
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input)
              {
                continue;
              }
              IEnumMediaTypes enumMedia;
              pins[0].EnumMediaTypes(out enumMedia);
              if (enumMedia != null)
              {
                enumMedia.Next(1, mediaTypes, out fetched);
                Release.ComObject("base DVB tuner MPEG 2 demultiplexer output pin media type enumerator", ref enumMedia);
                if (fetched == 1 && mediaTypes[0] != null)
                {
                  try
                  {
                    if (mediaTypes[0].majorType == MediaType.Audio || mediaTypes[0].majorType == MediaType.Video)
                    {
                      // We're not interested in audio or video pins.
                      continue;
                    }
                  }
                  finally
                  {
                    Release.AmMediaType(ref mediaTypes[0]);
                  }
                }
              }
              try
              {
                hr = _graph.Connect(pins[0], pinInTif);
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
            }
            finally
            {
              Release.ComObject("base DVB tuner MPEG 2 demultiplexer output pin " + pinNr, ref pins[0]);
            }
          }
        }
        finally
        {
          Release.ComObject("base DVB tuner MPEG 2 demultiplexer pin enumerator", ref enumPins);
        }
      }
      finally
      {
        Release.ComObject("base DVB tuner TIF input pin", ref pinInTif);
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
      this.LogDebug("dvb:Decompose");
      _timeshiftingEPGGrabber.Dispose();

      if (_epgGrabbing)
      {
        if (_epgGrabberCallback != null && _epgGrabbing)
        {
          this.LogInfo("dvb:cancel epg->decompose");
          _epgGrabberCallback.OnEpgCancelled();
        }
        _epgGrabbing = false;
      }

      base.Dispose();

      this.LogDebug("  free...");
      Release.ComObject("base DVB tuner MPEG 2 demultiplexer", ref _filterMpeg2DemuxTif);
      Release.ComObject("base DVB tuner network provider", ref _filterNetworkProvider);
      Release.ComObject("base DVB tuner infinite tee", ref _infTee);
      Release.ComObjectAllRefs("base DVB tuner tuner filter", ref _filterTuner);
      Release.ComObjectAllRefs("base DVB tuner capture filter", ref _filterCapture);
      Release.ComObject("base DVB tuner transport information filter", ref _filterTIF);

      this.LogDebug("  free devices...");
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
          Release.ComObjectAllRefs("base DVB tuner signal statistic", ref stat);
        }
        _tunerStatistics.Clear();
      }
      this.LogDebug("  decompose done...");
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
        if (_currentTuningDetail == null || _state != DeviceState.Started || _tunerStatistics == null || _tunerStatistics.Count == 0)
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
      if (_timeshiftingEPGGrabber != null)
      {
        _timeshiftingEPGGrabber.OnEpgCancelled();
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