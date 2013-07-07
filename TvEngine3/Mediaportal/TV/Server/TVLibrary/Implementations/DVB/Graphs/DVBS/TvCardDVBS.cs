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
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles satellite tuners with BDA drivers.
  /// </summary>
  public class TvCardDVBS : TvCardDvbBase
  {
    #region variables

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

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TvCardDVBS"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface for the instance to use.</param>
    /// <param name="device">The <see cref="DsDevice"/> instance that the instance will encapsulate.</param>
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

    #region graph building

    /// <summary>
    /// Actually load the device.
    /// </summary>
    protected override void PerformLoading()
    {
      this.LogDebug("TvCardDvbS: load device");
      base.PerformLoading();

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
    /// Create and register the BDA tuning space for the device.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace()
    {
      this.LogDebug("TvCardDvbS: CreateTuningSpace()");

      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      IDVBSTuningSpace tuningSpace = null;
      IDVBSLocator locator = null;
      try
      {
        ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
        if (container == null)
        {
          throw new TvException("Failed to get ITuningSpaceContainer handle from SystemTuningSpaces instance.");
        }

        tuningSpace = (IDVBSTuningSpace)new DVBSTuningSpace();
        int hr = tuningSpace.put_UniqueName(TuningSpaceName);
        hr |= tuningSpace.put_FriendlyName(TuningSpaceName);
        hr |= tuningSpace.put__NetworkType(typeof(DVBSNetworkProvider).GUID);
        hr |= tuningSpace.put_SystemType(DVBSystemType.Satellite);
        hr |= tuningSpace.put_SpectralInversion(SpectralInversion.Automatic);
        hr |= tuningSpace.put_LowOscillator(9750000);
        hr |= tuningSpace.put_HighOscillator(10600000);
        hr |= tuningSpace.put_LNBSwitch(11700000);

        locator = (IDVBSLocator)new DVBSLocator();
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_SymbolRate(-1);
        hr |= locator.put_Modulation(ModulationType.ModNotSet);
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != 0)
        {
          this.LogWarn("TvCardDvbS: potential error in CreateTuningSpace(), hr = 0x{0:X}", hr);
        }

        object index;
        hr = container.Add(tuningSpace, out index);
        HResult.ThrowException(hr, "Failed to Add() on ITuningSpaceContainer.");
        return tuningSpace;
      }
      catch (Exception)
      {
        Release.ComObject("Satellite tuner tuning space", ref tuningSpace);
        Release.ComObject("Satellite tuner locator", ref locator);
        throw;
      }
      finally
      {
        Release.ComObject("Satellite tuner tuning space container", ref systemTuningSpaces);
      }
    }

    /// <summary>
    /// The registered name of BDA tuning space for the device.
    /// </summary>
    protected override string TuningSpaceName
    {
      get
      {
        return "MediaPortal Satellite Tuning Space";
      }
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
    /// <param name="tuningSpace">The device's tuning space.</param>
    /// <param name="channel">The channel to translate into a tune request.</param>
    /// <returns>a tune request instance</returns>
    protected override ITuneRequest AssembleTuneRequest(ITuningSpace tuningSpace, IChannel channel)
    {
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      IDVBSTuningSpace dvbsTuningSpace = tuningSpace as IDVBSTuningSpace;
      if (dvbsTuningSpace == null)
      {
        throw new TvException("Failed to get IDVBSTuningSpace handle from ITuningSpace.");
      }
      int hr = dvbsTuningSpace.put_LowOscillator(dvbsChannel.LnbType.LowBandFrequency);
      hr |= dvbsTuningSpace.put_HighOscillator(dvbsChannel.LnbType.HighBandFrequency);
      hr |= dvbsTuningSpace.put_LNBSwitch(dvbsChannel.LnbType.SwitchFrequency);

      ILocator locator;
      hr |= tuningSpace.get_DefaultLocator(out locator);
      HResult.ThrowException(hr, "Failed to get_DefaultLocator() on ITuningSpace.");
      try
      {
        IDVBSLocator dvbsLocator = locator as IDVBSLocator;
        if (dvbsLocator == null)
        {
          throw new TvException("Failed to get IDVBSLocator handle from ILocator.");
        }
        hr = dvbsLocator.put_CarrierFrequency((int)dvbsChannel.Frequency);
        hr |= dvbsLocator.put_SymbolRate(dvbsChannel.SymbolRate);
        hr |= dvbsLocator.put_SignalPolarisation(dvbsChannel.Polarisation);
        hr |= dvbsLocator.put_Modulation(dvbsChannel.ModulationType);
        hr |= dvbsLocator.put_InnerFECRate(dvbsChannel.InnerFecRate);

        ITuneRequest tuneRequest;
        hr = tuningSpace.CreateTuneRequest(out tuneRequest);
        HResult.ThrowException(hr, "Failed to CreateTuneRequest() on ITuningSpace.");
        try
        {
          IDVBTuneRequest dvbTuneRequest = tuneRequest as IDVBTuneRequest;
          if (dvbTuneRequest == null)
          {
            throw new TvException("Failed to get IDVBTuneRequest handle from ITuneRequest.");
          }
          hr |= dvbTuneRequest.put_ONID(dvbsChannel.NetworkId);
          hr |= dvbTuneRequest.put_TSID(dvbsChannel.TransportId);
          hr |= dvbTuneRequest.put_SID(dvbsChannel.ServiceId);
          hr |= dvbTuneRequest.put_Locator(locator);

          if (hr != 0)
          {
            this.LogWarn("TvCardDvbS: potential error in AssembleTuneRequest(), hr = 0x{0:X}", hr);
          }

          return dvbTuneRequest;
        }
        catch (Exception)
        {
          Release.ComObject("Satellite tuner tune request", ref tuneRequest);
          throw;
        }
      }
      finally
      {
        Release.ComObject("Satellite tuner locator", ref locator);
      }
    }

    /// <summary>
    /// Check if the device can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the device can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBSChannel;
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
    /// Stop the device. The actual result of this function depends on device configuration.
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

    // TODO: remove this method, it should not be required and it is bad style!
    protected override DVBBaseChannel CreateChannel()
    {
      return new DVBSChannel();
    }
  }
}