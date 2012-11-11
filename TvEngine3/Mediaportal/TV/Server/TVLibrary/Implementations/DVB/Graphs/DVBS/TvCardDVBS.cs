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

using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs.DVBS
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

    /// <summary>
    /// Enable or disable always sending DiSEqC commands.
    /// </summary>
    /// <remarks>
    /// DiSEqC commands are usually only sent when changing to a channel on a different switch port or at a
    /// different positioner location. Enabling this option will cause DiSEqC commands to be sent on each
    /// channel change.
    /// </remarks>
    private bool _alwaysSendDiseqcCommands = false;

    /// <summary>
    /// The number of times to repeat DiSEqC commands.
    /// </summary>
    /// <remarks>
    /// When set to zero, commands are sent once; when set to one, commands are sent twice... etc.
    /// </remarks>
    private ushort _diseqcCommandRepeatCount = 0;

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
      if (_devicePath != null)
      {
        Card c = CardManagement.GetCardByDevicePath(_devicePath, CardIncludeRelationEnum.None);        
        if (c != null)
        {
          _alwaysSendDiseqcCommands = c.AlwaysSendDiseqcCommands;
          _diseqcCommandRepeatCount = (ushort)c.DiseqcCommandRepeatCount;
          if (_diseqcCommandRepeatCount > 5)
          {
            // It would be rare that commands would need to be repeated more than twice. Five times
            // is a more than reasonable practical limit.
            _diseqcCommandRepeatCount = 5;
          }
        }
      }
    }

    #endregion

    #region graphbuilding

    /// <summary>
    /// Build the BDA filter graph.
    /// </summary>
    public override void BuildGraph()
    {
      this.LogDebug("TvCardDvbS: build graph");
      base.BuildGraph();

      // Check if one of the supported interfaces is capable of sending DiSEqC commands.
      foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
      {
        IDiseqcDevice diseqcDevice = deviceInterface as IDiseqcDevice;
        if (diseqcDevice != null)
        {
          this.LogDebug("TvCardDvbS: found DiSEqC command interface");
          _diseqcController = new DiseqcController(diseqcDevice, _alwaysSendDiseqcCommands, _diseqcCommandRepeatCount);
          break;
        }
      }
    }

    /// <summary>
    /// Create the BDA tuning space for the tuner. This will be used for BDA tuning.
    /// </summary>
    protected override void CreateTuningSpace()
    {
      this.LogDebug("TvCardDvbS: create tuning space");

      // Check if the system already has an appropriate tuning space.
      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
      if (container == null)
      {
        this.LogError("TvCardDvbS: failed to get the tuning space container");
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
            this.LogDebug("TvCardDvbS: found correct tuningspace");
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
      this.LogDebug("TvCardDvbS: create new tuningspace");
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

    protected override DVBBaseChannel CreateChannel()
    {
      return new DVBSChannel();
    }

    #endregion

    #region tuning & scanning

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
        this.LogDebug("TvCardDvbS: channel is not a DVB-S channel!!! {0}", channel.GetType().ToString());
        return null;
      }

      _tuningSpace.put_LowOscillator(dvbsChannel.LnbType.LowBandFrequency);
      _tuningSpace.put_HighOscillator(dvbsChannel.LnbType.HighBandFrequency);
      _tuningSpace.put_LNBSwitch(dvbsChannel.LnbType.SwitchFrequency);

      ILocator locator;
      _tuningSpace.get_DefaultLocator(out locator);
      IDVBSLocator dvbsLocator = (IDVBSLocator)locator;
      dvbsLocator.put_CarrierFrequency((int)dvbsChannel.Frequency);
      dvbsLocator.put_SymbolRate(dvbsChannel.SymbolRate);
      dvbsLocator.put_SignalPolarisation(dvbsChannel.Polarisation);
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

    #endregion

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