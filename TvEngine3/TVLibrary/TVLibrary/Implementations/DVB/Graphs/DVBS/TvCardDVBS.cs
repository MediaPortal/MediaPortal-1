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
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-S BDA cards
  /// </summary>
  public class TvCardDVBS : TvCardDvbBase
  {
    #region variables

    private IDiseqcController _diseqcController = null;
    private IDiSEqCMotor _diseqcMotor = null;

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
      _cardType = CardType.DvbS;
    }

    #endregion

    #region graphbuilding

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
    {
      Log.Log.Debug("TvCardDvbS: BuildGraph()");
      base.BuildGraph();

      // Check if one of the supported interfaces is capable of sending DiSEqC commands.
      foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
      {
        _diseqcController = deviceInterface as IDiseqcController;
        if (_diseqcController != null)
        {
          Log.Log.Debug("TvCardDvbS: found DiSEqC command interface");
          _diseqcMotor = new DiSEqCMotor(_diseqcController);
          break;
        }
      }
    }

    /// <summary>
    /// Create the BDA tuning space for the tuner. This will be used for BDA tuning.
    /// </summary>
    protected override void CreateTuningSpace()
    {
      Log.Log.WriteFile("dvbs:CreateTuningSpace()");
      ITuner tuner = (ITuner)_filterNetworkProvider;
      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
      if (container == null)
      {
        Log.Log.Error("CreateTuningSpace() Failed to get ITuningSpaceContainer");
        return;
      }
      IEnumTuningSpaces enumTuning;
      ITuningSpace[] spaces = new ITuningSpace[2];
      int lowOsc;
      int hiOsc;
      int lnbSwitch;
      if (_parameters.UseDefaultLnbFrequencies)
      {
        lowOsc = 9750;
        hiOsc = 10600;
        lnbSwitch = 11700;
      }
      else
      {
        lowOsc = _parameters.LnbLowFrequency;
        hiOsc = _parameters.LnbHighFrequency;
        lnbSwitch = _parameters.LnbSwitchFrequency;
      }
      ITuneRequest request;
      container.get_EnumTuningSpaces(out enumTuning);
      IDVBSTuningSpace tuningSpace;
      while (true)
      {
        int fetched;
        enumTuning.Next(1, spaces, out fetched);
        if (fetched != 1)
          break;
        string name;
        spaces[0].get_UniqueName(out name);
        if (name == "MediaPortal DVBS TuningSpace")
        {
          Log.Log.WriteFile("dvbs:found correct tuningspace {0}", name);

          _tuningSpace = (IDVBSTuningSpace)spaces[0];
          tuningSpace = (IDVBSTuningSpace)_tuningSpace;
          tuningSpace.put_LNBSwitch(lnbSwitch * 1000);
          tuningSpace.put_SpectralInversion(SpectralInversion.Automatic);
          tuningSpace.put_LowOscillator(lowOsc * 1000);
          tuningSpace.put_HighOscillator(hiOsc * 1000);
          tuner.put_TuningSpace(tuningSpace);
          tuningSpace.CreateTuneRequest(out request);
          _tuneRequest = (IDVBTuneRequest)request;
          return;
        }
        Release.ComObject("ITuningSpace", spaces[0]);
      }
      Release.ComObject("IEnumTuningSpaces", enumTuning);
      Log.Log.WriteFile("dvbs:Create new tuningspace");
      _tuningSpace = (IDVBSTuningSpace)new DVBSTuningSpace();
      tuningSpace = (IDVBSTuningSpace)_tuningSpace;
      tuningSpace.put_UniqueName("MediaPortal DVBS TuningSpace");
      tuningSpace.put_FriendlyName("MediaPortal DVBS TuningSpace");
      tuningSpace.put__NetworkType(typeof (DVBSNetworkProvider).GUID);
      tuningSpace.put_SystemType(DVBSystemType.Satellite);
      tuningSpace.put_LNBSwitch(lnbSwitch * 1000);
      tuningSpace.put_LowOscillator(lowOsc * 1000);
      tuningSpace.put_HighOscillator(hiOsc * 1000);
      IDVBSLocator locator = (IDVBSLocator)new DVBSLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);
      object newIndex;
      _tuningSpace.put_DefaultLocator(locator);
      container.Add(_tuningSpace, out newIndex);
      tuner.put_TuningSpace(_tuningSpace);
      Release.ComObject("TuningSpaceContainer", container);
      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = (IDVBTuneRequest)request;
    }

    #endregion

    #region tuning & recording

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="channel">The channel that will be tuned.</param>
    /// <returns><c>true</c> if the tune request is created successfully, otherwise <c>false</c></returns>
    protected override bool AssembleTuneRequest(IChannel channel)
    {
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel == null)
      {
        Log.Log.WriteFile("TvCardDvbS: channel is not a DVB-S channel!!! {0}", channel.GetType().ToString());
        return false;
      }
      /*
        SendDiseqcCommands(dvbsChannel);
        return true;
      }*/

      int lowOsc;
      int highOsc;
      int switchFrequency;
      BandTypeConverter.GetDefaultLnbSetup(Parameters, dvbsChannel.BandType, out lowOsc, out highOsc, out switchFrequency);
      // Convert MHz -> kHz.
      lowOsc *= 1000;
      highOsc *= 1000;
      switchFrequency *= 1000;
      Log.Log.Info("LNB settings: low = {0}, high = {1}, switch = {2}", lowOsc, highOsc, switchFrequency);

      // Setting the switch frequency to zero is the equivalent of saying that the switch frequency is
      // irrelevant - that the 22 kHz tone state shouldn't depend on the transponder frequency.
      if (switchFrequency == 0)
      {
        switchFrequency = 18000000;
      }
      // Some tuners (eg. Prof USB series) don't handle multiple LOFs correctly. We need to pass either
      // the low or high frequency with the switch frequency.
      int lof;
      if (dvbsChannel.Frequency > switchFrequency)
      {
        lof = highOsc;
      }
      else
      {
        lof = lowOsc;
      }
      Log.Log.Info("LNB translated settings: oscillator = {0}, switch = {1}", lof, switchFrequency);
      IDVBSTuningSpace tuningSpace = (IDVBSTuningSpace)_tuningSpace;
      tuningSpace.put_LowOscillator(lof);
      tuningSpace.put_HighOscillator(lof);
      tuningSpace.put_LNBSwitch(switchFrequency);

      ITuneRequest request;
      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = (IDVBTuneRequest)request;
      ILocator locator;
      _tuningSpace.get_DefaultLocator(out locator);
      IDVBSLocator dvbsLocator = (IDVBSLocator)locator;
      IDVBTuneRequest tuneRequest = (IDVBTuneRequest)_tuneRequest;
      tuneRequest.put_ONID(dvbsChannel.NetworkId);
      tuneRequest.put_SID(dvbsChannel.ServiceId);
      tuneRequest.put_TSID(dvbsChannel.TransportId);
      locator.put_CarrierFrequency((int)dvbsChannel.Frequency);
      dvbsLocator.put_SymbolRate(dvbsChannel.SymbolRate);
      dvbsLocator.put_SignalPolarisation(dvbsChannel.Polarisation);
      dvbsLocator.put_Modulation(dvbsChannel.ModulationType);
      dvbsLocator.put_InnerFECRate(dvbsChannel.InnerFecRate);
      _tuneRequest.put_Locator(locator);

      SendDiseqcCommands(dvbsChannel);

      return true;
    }

    /// <summary>
    /// Send the required switch and motor DiSEqC command(s) to tune a given channel.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the command(s) are sent successfully, otherwise <c>false</c></returns>
    private bool SendDiseqcCommands(DVBSChannel channel)
    {
      Log.Log.Debug("TvCardDvbS: send switch and motor DiSEqC commands");
      if (channel == null)
      {
        Log.Log.Debug("TvCardDvbS: channel is null");
        return true;
      }
      if (_diseqcController == null)
      {
        Log.Log.Debug("TvCardDvbS: DiSEqC not supported");
        return false;
      }

      bool success = true;
      bool alwaysSendCommands = false;
      DVBSChannel previousChannel = _previousChannel as DVBSChannel;
      bool isHighBand = BandTypeConverter.IsHighBand(channel, _parameters);
      bool wasHighBand = !isHighBand;
      if (previousChannel != null)
      {
        wasHighBand = BandTypeConverter.IsHighBand(previousChannel, _parameters);
      }

      // There is a well defined order in which commands may be sent:
      // "raw" DiSEqC commands -> DiSEqC 1.0 (committed) -> tone burst (simple DiSEqC) -> 22 kHz tone on/off

      // Switch command.
      bool sendCommand = channel.Diseqc != DiseqcSwitchCommand.None &&
        channel.Diseqc != DiseqcSwitchCommand.SimpleA &&
        channel.Diseqc != DiseqcSwitchCommand.SimpleB;
      if (sendCommand)
      {
        // If we get to here then there is a valid command to send, but we might not need/want to send it.
        if (!alwaysSendCommands &&
          previousChannel != null &&
          previousChannel.Diseqc == channel.Diseqc &&
          (
            (channel.Diseqc != DiseqcSwitchCommand.PortA &&
            channel.Diseqc != DiseqcSwitchCommand.PortB &&
            channel.Diseqc != DiseqcSwitchCommand.PortC &&
            channel.Diseqc != DiseqcSwitchCommand.PortD)
            ||
            (previousChannel.Polarisation == channel.Polarisation &&
            wasHighBand == isHighBand)
          )
        )
        {
          sendCommand = false;
        }
      }
      if (!sendCommand)
      {
        Log.Log.Debug("TvCardDvbS: no need to send switch command");
      }
      else
      {
        byte command = 0xf0;
        if (channel.Diseqc == DiseqcSwitchCommand.PortA ||
          channel.Diseqc == DiseqcSwitchCommand.PortB ||
          channel.Diseqc == DiseqcSwitchCommand.PortC ||
          channel.Diseqc == DiseqcSwitchCommand.PortD)
        {
          Log.Log.Debug("TvCardDvbS: DiSEqC 1.0 switch command");
          int portNumber = BandTypeConverter.GetPortNumber(channel.Diseqc);
          bool isHorizontal = channel.Polarisation == Polarisation.LinearH || channel.Polarisation == Polarisation.CircularL;
          command |= (byte)(isHighBand ? 1 : 0);
          command |= (byte)((isHorizontal) ? 2 : 0);
          command |= (byte)((portNumber - 1) << 2);
          if (!_diseqcController.SendCommand(new byte[4] { 0xe0, 0x10, 0x38, command }))
          {
            success = false;
          }
        }
        else
        {
          Log.Log.Debug("TvCardDvbS: DiSEqC 1.1 switch command");
          if (!_diseqcController.SendCommand(new byte[4] { 0xe0, 0x10, 0x39, command }))
          {
            success = false;
          }
        }
      }

      // Motor movement.
      sendCommand = channel.SatelliteIndex > 0;
      if (sendCommand)
      {
        if (!alwaysSendCommands &&
          previousChannel != null &&
          previousChannel.SatelliteIndex == channel.SatelliteIndex)
        {
          sendCommand = false;
        }
      }
      if (!sendCommand)
      {
        Log.Log.Debug("TvCardDvbS: no need to send motor command");
      }
      else
      {
        Log.Log.Debug("TvCardDvbS: motor command(s)");
        _diseqcMotor.GotoPosition((byte)channel.SatelliteIndex);
      }

      // Tone burst and final state.
      ToneBurst toneBurst = ToneBurst.Off;
      if (channel.Diseqc == DiseqcSwitchCommand.SimpleA)
      {
        toneBurst = ToneBurst.ToneBurst;
      }
      else if (channel.Diseqc == DiseqcSwitchCommand.SimpleB)
      {
        toneBurst = ToneBurst.DataBurst;
      }
      Tone22k tone22k = Tone22k.Off;
      if (isHighBand)
      {
        tone22k = Tone22k.On;
      }
      if (!_diseqcController.SetToneState(toneBurst, tone22k))
      {
        success = false;
      }

      return success;
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
            dvbschannel.Diseqc = (DiseqcSwitchCommand)detail.Diseqc;
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
    /// Check if the tuner can tune to a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if ((channel as DVBSChannel) == null)
      {
        return false;
      }
      return true;
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
    /// Gets the interface for controlling the DiSEqC satellite dish motor.
    /// </summary>
    /// <value>
    /// <c>null</c> if the tuner is not a satellite tuner or the tuner doesn't support controlling a motor,
    /// otherwise the interface for controlling the motor
    /// </value>
    public override IDiSEqCMotor DiSEqCMotor
    {
      get
      {
        return _diseqcMotor;
      }
    }
  }
}