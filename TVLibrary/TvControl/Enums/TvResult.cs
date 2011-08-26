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

namespace TvControl
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
    /// Operation failed since all cards are busy and no free card could be found
    /// </summary>
    AllCardsBusy,
    /// <summary>
    /// Operation failed since channel is encrypted
    /// </summary>
    ChannelIsScrambled,
    /// <summary>
    /// Opetation failed since no audio/video was detected after tuning
    /// </summary>
    NoVideoAudioDetected,
    /// <summary>
    /// Operation failed since no signal was detected
    /// </summary>
    NoSignalDetected,
    /// <summary>
    /// Operation failed due to an unknown error
    /// </summary>
    UnknownError,
    /// <summary>
    /// Operation failed since the graph could not be build or started
    /// </summary>
    UnableToStartGraph,
    /// <summary>
    /// Operation failed since the channel is unknown
    /// </summary>
    UnknownChannel,
    /// <summary>
    /// Operation failed since the there is no tuning information for the channel
    /// </summary>
    NoTuningDetails,
    /// <summary>
    /// Operation failed since the channel is not mapped to any card
    /// </summary>
    ChannelNotMappedToAnyCard,
    /// <summary>
    /// Operation failed since the card is disabled
    /// </summary>
    CardIsDisabled,
    /// <summary>
    /// Operation failed since we are unable to connect to the slave server
    /// </summary>
    ConnectionToSlaveFailed,
    /// <summary>
    /// Operation failed since we are not the owner of the card
    /// </summary>
    NotTheOwner,
    /// <summary>
    /// Operation failed since we are unable to build the graph
    /// </summary>
    GraphBuildingFailed,
    /// <summary>
    /// Operation failed since we can't find a suitable software encoder
    /// </summary>
    SWEncoderMissing,
    /// <summary>
    /// Operation failed since there is no free disk space
    /// </summary>
    NoFreeDiskSpace,
    /// <summary>
    /// No PMT found
    /// </summary>
    NoPmtFound
  }
}