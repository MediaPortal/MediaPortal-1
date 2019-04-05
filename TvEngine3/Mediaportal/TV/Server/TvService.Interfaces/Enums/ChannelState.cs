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
  /// current availability of a specific channel
  /// </summary>
  public enum ChannelState
  {
    /// <summary>
    /// the channel cannot be tuned
    /// </summary>
    NotTunable = 0,
    /// <summary>
    /// the channel can be tuned
    /// </summary>
    Tunable,
    /// <summary>
    /// the channel is currently being time-shifted
    /// </summary>
    TimeShifting,
    /// <summary>
    /// the channel is currently being recorded
    /// </summary>
    Recording,

    #region these values correspond to TvResult values

    NotTunable_AllTunersUnavailable,
    NotTunable_AllTunersDisabled,
    NotTunable_AllTunersBusy,
    NotTunable_ChannelNotTunable,         // (...by any tuner)
    NotTunable_TuningDetailsNotMapped,    // (...to any tuner)
    NotTunable_ChannelNotDecryptable,     // (...by any tuner)
    NotTunable_NoTuningDetails

    #endregion
  }
}