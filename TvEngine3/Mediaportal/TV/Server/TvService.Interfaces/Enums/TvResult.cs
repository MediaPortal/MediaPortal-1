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

namespace Mediaportal.TV.Server.TVService.Interfaces.Enums
{
  /// <summary>
  /// enum describing the possible result codes for the tv engine
  /// </summary>
  public enum TvResult
  {
    /// <summary>
    /// Operation succeeded
    /// </summary>
    Succeeded,

    /// <summary>
    /// The channel does not have any tuning details, and is therefore
    /// untunable.
    /// </summary>
    ChannelNoTuningDetails,
    /// <summary>
    /// All the channel's tuning details are not mapped to any tuners.
    /// </summary>
    ChannelTuningDetailsNotMapped,
    /// <summary>
    /// The tuner or tuners are physically incapable of tuning the channel, or
    /// are prevented by configuration (eg. tuner broadcast standard and/or
    /// satellite config).
    /// </summary>
    ChannelNotTunable,
    /// <summary>
    /// The channel is encrypted and the tuner or tuners are incapable of
    /// decrypting it. This result can be returned in response to a
    /// configuration limitation (for example: CA disabled or provider not
    /// supported) or after actually attempting and failing to decrypt.
    /// </summary>
    ChannelNotDecryptable,
    /// <summary>
    /// The channel is currently not active. Currently this result is returned
    /// after the channel's transmitter has been successfully tuned but the
    /// MPEG 2 program is unavailable (program PMT not found or DVB service
    /// record marked as not running in the SDT).
    /// </summary>
    ChannelNotActive,
    /// <summary>
    /// The channel was not found. Currently this result is returned after the
    /// channel's transmitter has been successfully tuned but the MPEG 2
    /// program is not found in the PAT. This result may indicate that the
    /// channel's tuning details are incorrect or that the channel has been
    /// moved to a different transmitter.
    /// </summary>
    ChannelNotFound,
    /// <summary>
    /// The channel's transmitter was successfully tuned, but subsequently the
    /// channel's video and/or audio were not received.
    /// </summary>
    ChannelVideoAndOrAudioNotReceived,

    /// <summary>
    /// The target tuner or tuners are not available (disabled in Windows
    /// device manager, disconnected, turned off or otherwise undetectable).
    /// </summary>
    TunerNotAvailable,
    /// <summary>
    /// The target tuner or tuners are configured to be disabled.
    /// </summary>
    TunerDisabled,
    /// <summary>
    /// The target tuner or tuners are capable of tuning the channel but
    /// currently tuned to other channels for other users, and therefore cannot
    /// be used.
    /// </summary>
    TunerBusy,
    /// <summary>
    /// The target tuner or tuners could not be loaded.
    /// </summary>
    TunerLoadFailed,
    /// <summary>
    /// The target tuner or tuners could not be loaded because the required
    /// software encoders are not installed.
    /// </summary>
    TunerLoadFailedSoftwareEncoderRequired,
    /// <summary>
    /// The channel is associated with a satellite for which there is no
    /// corresponding tuning configuration for the tuner or tuners.
    /// </summary>
    TunerSatelliteNotReceivable,
    /// <summary>
    /// The channel's tuning details require DiSEqC commands to be sent, but
    /// TV Server is unable to send DiSEqC commands using the tuner or tuners.
    /// </summary>
    TunerDiseqcNotSupported,
    /// <summary>
    /// After tuning according to the channel's tuning details, the tuner
    /// failed to lock onto (locate) the transmitter's signal or stream.
    /// </summary>
    TunerNoSignalDetected,

    /// <summary>
    /// An unexpected, unspecified, unknown or unhandleable error occurred.
    /// </summary>
    UnexpectedError,
    /// <summary>
    /// (Currently not used.)
    /// </summary>
    UnknownChannel,

    /// <summary>
    /// There is insufficient free disk space to peform the operation.
    /// </summary>
    InsufficientFreeDiskSpace,
    /// <summary>
    /// The tune operation was cancelled.
    /// </summary>
    TuneCancelled,


    AlreadyParked,
    SubChannelDoesNotExist,
    SubChannelIsParked,
    SubChannelIsNotParked,

    /// <summary>
    /// Tuning failed for an unspecified reason.
    /// </summary>
    TuneFailed
  }
}