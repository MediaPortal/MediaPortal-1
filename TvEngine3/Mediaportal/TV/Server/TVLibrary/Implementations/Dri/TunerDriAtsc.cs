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
using System.Text.RegularExpressions;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for digital tuners which
  /// implement the CableLabs/OpenCable Digital Receiver Interface.
  /// </summary>
  internal class TunerDriAtsc : TunerDriBase
  {
    private static readonly Regex REGEX_SIGNAL_INFO = new Regex(@"^(\d+(\.\d+)?)[^\d]");

    private ServiceFdc _serviceFdc = null;                    // forward data channel, provides meta-data (service information etc.)
    private ICollection<TunerModulation> _supportedModulationSchemes = null;
    private CasCardStatus _cardStatus = CasCardStatus.Removed;
    private bool _isScanningOutOfBandChannel = false;

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDriAtsc"/> class.
    /// </summary>
    /// <param name="descriptor">The UPnP device description.</param>
    /// <param name="tunerInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single tuner.</param>
    /// <param name="productInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single product.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    /// <param name="supportedModulationSchemes">The modulation schemes supported by the hardware.</param>
    /// <param name="controlPoint">The control point to use to connect to the device.</param>
    /// <param name="streamTuner">An internal tuner implementation, used for RTP stream reception.</param>
    public TunerDriAtsc(DeviceDescriptor descriptor, string tunerInstanceId, string productInstanceId, BroadcastStandard supportedBroadcastStandards, ICollection<TunerModulation> supportedModulationSchemes, UPnPControlPoint controlPoint, ITunerInternal streamTuner)
      : base(descriptor, descriptor.DeviceUUID + "ATSC", tunerInstanceId, productInstanceId, supportedBroadcastStandards, controlPoint, streamTuner)
    {
      _supportedModulationSchemes = supportedModulationSchemes;
    }

    /// <summary>
    /// Handle UPnP evented state variable changes.
    /// </summary>
    /// <param name="stateVariable">The state variable that has changed.</param>
    /// <param name="newValue">The new value of the state variable.</param>
    protected override void OnStateVariableChanged(CpStateVariable stateVariable, object newValue)
    {
      try
      {
        if (stateVariable.Name.Equals("CardStatus"))
        {
          _cardStatus = (CasCardStatus)(string)newValue;
          return;
        }
        else if (!stateVariable.Name.Equals("TableSection"))
        {
          base.OnStateVariableChanged(stateVariable, newValue);
          return;
        }

        byte[] section = (byte[])newValue;
        if (section == null || section.Length < 4)
        {
          this.LogWarn("DRI ATSC: received invalid table section from the FDC");
          if (section != null)
          {
            Dump.DumpBinary(section);
          }
          return;
        }

        ushort pid = (ushort)((section[0] << 8) | section[1]);
        byte tableId = section[2];
        if (pid == 0x1ffc && tableId >= 0xc2 && tableId <= 0xc9 && tableId != 0xc5 && tableId != 0xc6)
        {
          ChannelScannerDri scanner = InternalChannelScanningInterface as ChannelScannerDri;
          if (scanner != null)
          {
            scanner.OnOutOfBandSectionReceived(section);
          }
        }
        else if (pid != 0x1ffc || ((tableId != 0xc5 || section.Length != 16) && tableId != 0xfd)) // not a standard system time or stuffing table
        {
          this.LogDebug("DRI ATSC: received unusual table section from the FDC, PID = {0}, table ID = {1}", pid, tableId);
          Dump.DumpBinary(section);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI ATSC: failed to handle state variable change, tuner ID = {0}", TunerId);
      }
    }

    #region ITunerInternal overrides

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      // Is tuning physically possible?
      if (!base.CanTune(channel))
      {
        return false;
      }

      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null)
      {
        return atscChannel.ModulationScheme == ModulationSchemeVsb.Vsb8 && _supportedModulationSchemes.Contains(TunerModulation.Vsb8);
      }

      ChannelScte scteChannel = channel as ChannelScte;
      if (
        scteChannel == null ||
        (
          scteChannel.ModulationScheme != ModulationSchemeQam.Automatic &&  // switched digital video - modulation unknown, assume supported
          (scteChannel.ModulationScheme != ModulationSchemeQam.Qam64 || _supportedModulationSchemes.Contains(TunerModulation.Qam64)) &&
          (scteChannel.ModulationScheme != ModulationSchemeQam.Qam256 || _supportedModulationSchemes.Contains(TunerModulation.Qam256))
        )
      )
      {
        return false;
      }

      // Can we assemble a tune request that might succeed?

      // Yes, if we can tune without a CableCARD.
      if (!scteChannel.IsCableCardNeededToTune())
      {
        return true;
      }

      // Otherwise, yes, if the CableCARD is present and we have a valid
      // virtual channel number or source ID, or we're scanning using the
      // out-of-band tuner.
      uint virtualChannelNumber;
      if (
        (_serviceFdc == null || _cardStatus == CasCardStatus.Inserted) &&
        (
          (uint.TryParse(channel.LogicalChannelNumber, out virtualChannelNumber) && virtualChannelNumber > 0) ||
          scteChannel.SourceId > 0 ||
          scteChannel.IsOutOfBandScanChannel()
        )
      )
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("DRI ATSC: perform tuning");

      bool isSuccessful = true;
      bool isOobChannelScan = false;
      try
      {
        ChannelAtsc atscChannel = channel as ChannelAtsc;
        if (atscChannel != null)
        {
          TuneByFrequency(atscChannel.Frequency, TunerModulation.Vsb8);
          return;
        }

        ChannelScte scteChannel = channel as ChannelScte;
        if (scteChannel == null)
        {
          throw new TvException("Received request to tune incompatible channel.");
        }

        // Is a CableCARD required?
        if (!scteChannel.IsCableCardNeededToTune())
        {
          if (scteChannel.ModulationScheme == ModulationSchemeQam.Qam256)
          {
            TuneByFrequency(scteChannel.Frequency, TunerModulation.Qam256);
          }
          else
          {
            TuneByFrequency(scteChannel.Frequency, TunerModulation.Qam64);
          }
          return;
        }

        if (_cardStatus != CasCardStatus.Inserted)
        {
          throw new TvException("CableCARD required but not available, current status is {0}.", _cardStatus);
        }
        isOobChannelScan = scteChannel.IsOutOfBandScanChannel();
        if (!isOobChannelScan)
        {
          uint virtualChannelNumber;
          if (uint.TryParse(scteChannel.LogicalChannelNumber, out virtualChannelNumber) && virtualChannelNumber > 0)
          {
            TuneByVirtualChannelNumber(virtualChannelNumber);
          }
          else if (scteChannel.SourceId > 0)
          {
            TuneBySourceId(scteChannel.SourceId);
          }
          else
          {
            throw new TvException("Received request to tune channel with invalid channel number and source ID.");
          }
          return;
        }

        // We don't need to start streaming when scanning using the out-of-band
        // tuner. Simply check that the OOB tuner is locked.
        this.LogDebug("DRI ATSC: check out-of-band tuner lock");
        uint bitRate = 0;
        bool isSignalLocked;
        uint frequency = 0;
        bool spectrumInversion = false;
        IList<ushort> pids;
        _serviceFdc.GetFdcStatus(out bitRate, out isSignalLocked, out frequency, out spectrumInversion, out pids);
        if (!isSignalLocked)
        {
          throw new TvExceptionNoSignal(TunerId, channel, "Out-of-band tuner not locked.");
        }
        this.LogDebug("  frequency = {0} kHz", frequency);
        this.LogDebug("  bit-rate  = {0} kb/s", _vendor == TunerVendor.Ceton ? bitRate / 1000 : bitRate);   // Ceton supplies b/s instead of kb/s
        this.LogDebug("  PIDs      = [{0}]", string.Join(", ", pids));
      }
      catch
      {
        isSuccessful = false;
        throw;
      }
      finally
      {
        if (isSuccessful)
        {
          _isScanningOutOfBandChannel = isOobChannelScan;

          _streamChannel.IsEncrypted = channel.IsEncrypted;
          _streamChannel.LogicalChannelNumber = channel.LogicalChannelNumber;
          _streamChannel.MediaType = channel.MediaType;
          _streamChannel.Name = channel.Name;
          _streamChannel.OriginalNetworkId = -1;
          _streamChannel.Provider = channel.Provider;

          IChannelMpeg2Ts mpeg2TsChannel = channel as IChannelMpeg2Ts;
          if (mpeg2TsChannel != null)
          {
            _streamChannel.PmtPid = mpeg2TsChannel.PmtPid;
            _streamChannel.ProgramNumber = mpeg2TsChannel.ProgramNumber;
            _streamChannel.TransportStreamId = mpeg2TsChannel.TransportStreamId;
          }
        }
      }
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      this.LogDebug("DRI ATSC: perform loading");

      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts;
        if (SupportedBroadcastStandards.HasFlag(BroadcastStandard.Atsc))
        {
          streamFormat |= StreamFormat.Atsc;
        }
        if (SupportedBroadcastStandards.HasFlag(BroadcastStandard.Scte))
        {
          streamFormat |= StreamFormat.Scte;
        }
      }

      IList<ITunerExtension> extensions = base.PerformLoading(streamFormat);

      _serviceFdc = new ServiceFdc(_deviceConnection.Device);
      _serviceFdc.SubscribeStateVariables(_stateVariableChangeDelegate, _eventSubscriptionFailDelegate);

      if (_vendor == TunerVendor.Ceton)
      {
        _channelScanner = new ChannelScannerDriCeton(_channelScanner, _serviceFdc.RequestTables, _serverIpAddress);
      }
      else if (_vendor == TunerVendor.SiliconDust || _vendor == TunerVendor.Hauppauge)
      {
        _channelScanner = new ChannelScannerDriSiliconDust(_channelScanner, _serviceFdc.RequestTables, _serverIpAddress);
      }
      else
      {
        _channelScanner = new ChannelScannerDri(_channelScanner, _serviceFdc.RequestTables);
      }
      return extensions;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformSetTunerState(TunerState state, bool isFinalising = false)
    {
      this.LogDebug("DRI ATSC: perform set tuner state");

      if (!isFinalising && _isScanningOutOfBandChannel)
      {
        // Scanning the out-of-band (OOB) channel using a CableCARD doesn't
        // require streaming. The channel information is evented via the
        // forward data channel service.
        return;
      }

      base.PerformSetTunerState(state, isFinalising);
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      this.LogDebug("DRI ATSC: perform unloading");

      if (!isFinalising)
      {
        if (_serviceFdc != null)
        {
          _serviceFdc.Dispose();
          _serviceFdc = null;
        }
      }

      base.PerformUnloading(isFinalising);
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    public override void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock)
    {
      if (_serviceFdc == null || !_isScanningOutOfBandChannel)
      {
        base.GetSignalStatus(out isLocked, out isPresent, out strength, out quality, onlyGetLock);
        return;
      }

      isLocked = false;
      isPresent = false;
      strength = 0;
      quality = 0;
      if (onlyGetLock)
      {
        // When we're asked to only update locked status it means the tuner is
        // trying to lock in on signal. When scanning with the out-of-band
        // tuner, we already checked lock during tuning.
        isLocked = true;
        return;
      }

      try
      {
        uint bitrate = 0;
        uint frequency = 0;
        bool spectrumInversion = false;
        IList<ushort> pids;
        _serviceFdc.GetFdcStatus(out bitrate, out isLocked, out frequency, out spectrumInversion, out pids);
        isPresent = isLocked;

        bool readStrength = false;
        bool readQuality = false;
        if (_vendor != TunerVendor.Unknown && _serviceDiag != null)
        {
          string value = string.Empty;
          bool isVolatile = false;
          Match m;
          if (DiagParameter.OobSignalLevel.SupportedVendors.HasFlag(_vendor))
          {
            // Example: "0.8 dBmV".
            _serviceDiag.GetParameter(DiagParameter.OobSignalLevel, out value, out isVolatile);
            m = REGEX_SIGNAL_INFO.Match(value);
            if (!m.Success)
            {
              this.LogWarn("DRI ATSC: failed to interpret out-of-band signal level, value = {0}", value);
            }
            else
            {
              readStrength = true;
              strength = SignalStrengthDecibelsToPercentage(double.Parse(value));
            }
          }
          if (DiagParameter.OobSnr.SupportedVendors.HasFlag(_vendor))
          {
            // Example: "31.9 dB".
            _serviceDiag.GetParameter(DiagParameter.OobSnr, out value, out isVolatile);
            m = REGEX_SIGNAL_INFO.Match(value);
            if (!m.Success)
            {
              this.LogWarn("DRI ATSC: failed to interpret out-of-band signal-to-noise ratio, value = {0}", value);
            }
            else
            {
              readQuality = true;
              quality = SignalQualitySnrToPercentage(double.Parse(value));
            }
          }
        }

        if (!readStrength)
        {
          strength = isLocked ? 100 : 0;
        }
        if (!readQuality)
        {
          quality = isLocked ? 100 : 0;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DRI ATSC: exception updating signal status");
      }
    }

    #endregion

    #endregion
  }
}