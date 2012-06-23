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
using DirectShowLib;
using DirectShowLib.BDA;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Implementations.Helper;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Device;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-S/S2 tuners with BDA drivers.
  /// </summary>
  public class TvCardDVBS : TvCardDvbBase
  {
    #region variables

    /// <summary>
    /// A pre-configured tuning space, used to speed up the tuning process. 
    /// </summary>
    private IDVBSTuningSpace _tuningSpace = null;

    /// <summary>
    /// The DiSEqC control interface for the tuner.
    /// </summary>
    private IDiseqcController _diseqcController = null;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDVBS"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface.</param>
    /// <param name="device">The device.</param>
    public TvCardDVBS(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _tunerType = CardType.DvbS;
    }

    #endregion

    #region graphbuilding

    /// <summary>
    /// Build the BDA filter graph.
    /// </summary>
    public override void BuildGraph()
    {
      Log.Log.Debug("TvCardDvbS: BuildGraph()");
      base.BuildGraph();

      // Check if one of the supported interfaces is capable of sending DiSEqC commands.
      foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
      {
        IDiseqcDevice diseqcDevice = deviceInterface as IDiseqcDevice;
        if (diseqcDevice != null)
        {
          Log.Log.Debug("TvCardDvbS: found DiSEqC command interface");
          _diseqcController = new DiseqcController(diseqcDevice);
          break;
        }
      }
    }

    /// <summary>
    /// Create the BDA tuning space for the tuner. This will be used for BDA tuning.
    /// </summary>
    protected override void CreateTuningSpace()
    {
      Log.Log.Debug("TvCardDvbS: CreateTuningSpace()");

      // Check if the system already has an appropriate tuning space.
      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
      if (container == null)
      {
        Log.Log.Error("TvCardDvbS: failed to get the tuning space container");
        return;
      }

      ITuner tuner = (ITuner)_filterNetworkProvider;

      // Defaults: Ku linear "universal" LNB settings.
      int lowOsc = 9750000;
      int hiOsc = 10600000;
      int switchFrequency = 11700000;

      IEnumTuningSpaces enumTuning;
      container.get_EnumTuningSpaces(out enumTuning);
      try
      {
        ITuningSpace[] spaces = new ITuningSpace[2];
        while (true)
        {
          int fetched;
          enumTuning.Next(1, spaces, out fetched);
          if (fetched != 1)
          {
            break;
          }
          string name;
          spaces[0].get_UniqueName(out name);
          if (name.Equals("MediaPortal DVBS TuningSpace"))
          {
            Log.Log.Debug("TvCardDvbS: found correct tuningspace");
            _tuningSpace = (IDVBSTuningSpace)spaces[0];
            _tuningSpace.put_SpectralInversion(SpectralInversion.Automatic);
            _tuningSpace.put_LowOscillator(lowOsc);
            _tuningSpace.put_HighOscillator(hiOsc);
            _tuningSpace.put_LNBSwitch(switchFrequency);
            tuner.put_TuningSpace(_tuningSpace);
            Release.ComObject("TuningSpaceContainer", container);
            return;
          }
          Release.ComObject("ITuningSpace", spaces[0]);
        }
      }
      finally
      {
        Release.ComObject("IEnumTuningSpaces", enumTuning);
      }

      // We didn't find our tuning space registered in the system, so create a new one.
      Log.Log.Debug("TvCardDvbS: create new tuningspace");
      _tuningSpace = (IDVBSTuningSpace)new DVBSTuningSpace();
      _tuningSpace.put_UniqueName("MediaPortal DVBS TuningSpace");
      _tuningSpace.put_FriendlyName("MediaPortal DVBS TuningSpace");
      _tuningSpace.put__NetworkType(typeof(DVBSNetworkProvider).GUID);
      _tuningSpace.put_SystemType(DVBSystemType.Satellite);
      _tuningSpace.put_LowOscillator(lowOsc);
      _tuningSpace.put_HighOscillator(hiOsc);
      _tuningSpace.put_LNBSwitch(switchFrequency);

      IDVBSLocator locator = (IDVBSLocator)new DVBSLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_SymbolRate(-1);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);

      _tuningSpace.put_DefaultLocator(locator);

      object newIndex;
      container.Add(_tuningSpace, out newIndex);
      Release.ComObject("TuningSpaceContainer", container);

      tuner.put_TuningSpace(_tuningSpace);
    }

    #endregion

    #region tuning & recording

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      // Send DiSEqC commands (if necessary) before actually tuning in case the driver applies the commands
      // during the tuning process.
      if (_diseqcController != null)
      {
        _diseqcController.SwitchToChannel(channel as DVBSChannel);
      }
      base.PerformTuning(channel);
    }

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="channel">The channel that will be tuned.</param>
    /// <returns>the assembled tune request</returns>
    protected override ITuneRequest AssembleTuneRequest(IChannel channel)
    {
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel == null)
      {
        Log.Log.Debug("TvCardDvbS: channel is not a DVB-S channel!!! {0}", channel.GetType().ToString());
        return null;
      }

      uint lnbLof;
      uint lnbSwitchFrequency;
      Polarisation polarisation;
      LnbTypeConverter.GetLnbTuningParameters(dvbsChannel, out lnbLof, out lnbSwitchFrequency, out polarisation);
      _tuningSpace.put_LowOscillator((int)lnbLof);
      _tuningSpace.put_HighOscillator((int)lnbLof);
      _tuningSpace.put_LNBSwitch((int)lnbSwitchFrequency);

      ILocator locator;
      _tuningSpace.get_DefaultLocator(out locator);
      IDVBSLocator dvbsLocator = (IDVBSLocator)locator;
      dvbsLocator.put_CarrierFrequency((int)dvbsChannel.Frequency);
      dvbsLocator.put_SymbolRate(dvbsChannel.SymbolRate);
      dvbsLocator.put_SignalPolarisation(polarisation);
      dvbsLocator.put_Modulation(dvbsChannel.ModulationType);
      dvbsLocator.put_InnerFECRate(dvbsChannel.InnerFecRate);

      ITuneRequest request;
      _tuningSpace.CreateTuneRequest(out request);
      IDVBTuneRequest tuneRequest = (IDVBTuneRequest)request;
      tuneRequest.put_ONID(dvbsChannel.NetworkId);
      tuneRequest.put_TSID(dvbsChannel.TransportId);
      tuneRequest.put_SID(dvbsChannel.ServiceId);
      tuneRequest.put_Locator(locator);

      return tuneRequest;
    }

    #endregion

    #region epg & scanning
    
    /// <summary>
    /// checks if a received EPGChannel should be filtered from the resultlist
    /// </summary>
    /// <value></value>
    protected override bool FilterOutEPGChannel(EpgChannel epgChannel)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      if (layer.GetSetting("generalGrapOnlyForSameTransponder", "no").Value == "yes")
      {
        DVBBaseChannel chan = epgChannel.Channel as DVBBaseChannel;
        Channel dbchannel = layer.GetChannelByTuningDetail(chan.NetworkId, chan.TransportId, chan.ServiceId);
        DVBSChannel dvbschannel = new DVBSChannel();
        if (dbchannel == null)
        {
          return false;
        }
        foreach (TuningDetail detail in dbchannel.ReferringTuningDetail())
        {
          if (detail.ChannelType == 3)
          {
            dvbschannel.Frequency = detail.Frequency;
            dvbschannel.Polarisation = (Polarisation)detail.Polarisation;
            dvbschannel.ModulationType = (ModulationType)detail.Modulation;
            dvbschannel.SatelliteIndex = detail.SatIndex;
            dvbschannel.InnerFecRate = (BinaryConvolutionCodeRate)detail.InnerFecRate;
            dvbschannel.Pilot = (Pilot)detail.Pilot;
            dvbschannel.RollOff = (RollOff)detail.RollOff;
            dvbschannel.Diseqc = (DiseqcPort)detail.Diseqc;
          }
        }
        return this.CurrentChannel.IsDifferentTransponder(dvbschannel);
      }
      else
        return false;  
    }

    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    /// <value></value>
    public override ITVScanning ScanningInterface
    {
      get
      {
        if (!CheckThreadId())
          return null;
        return new DVBSScanning(this);
      }
    }

    #endregion

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (channel is DVBSChannel)
      {
        return true;
      }
      return false;
    }

    protected override DVBBaseChannel CreateChannel(int networkid, int transportid, int serviceid, string name)
    {
      DVBSChannel channel = new DVBSChannel();
      channel.NetworkId = networkid;
      channel.TransportId = transportid;
      channel.ServiceId = serviceid;
      channel.Name = name;
      return channel;
    }

    /// <summary>
    /// Get the device's DiSEqC control interface. This interface is only applicable for satellite tuners.
    /// It is used for controlling switch, positioner and LNB settings.
    /// </summary>
    /// <value><c>null</c> if the tuner is not a satellite tuner or the tuner does not support sending/receiving
    /// DiSEqC commands</value>
    public override IDiseqcController DiseqcController
    {
      get
      {
        return _diseqcController;
      }
    }

    /// <summary>
    /// Stop the device. The actual result of this function depends on device configuration:
    /// - graph stop
    /// - graph pause
    /// TODO graph destroy
    /// </summary>
    public override void Stop()
    {
      base.Stop();
      // Force the DiSEqC controller to forget the previously tuned channel. This guarantees that the
      // next call to SwitchToChannel() will actually cause commands to be sent.
      if (_diseqcController != null)
      {
        _diseqcController.SwitchToChannel(null);
      }
    }
  }
}