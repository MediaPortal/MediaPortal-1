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
  /// enum describing the possible result codes for the tv engine when TV suddenly stops
  /// </summary>
  public enum TvStoppedReason
  {
    /// <summary>
    /// Timeshifting stopped because of an unknown reason.
    /// </summary>
    UnknownReason,
    /// <summary>
    /// Timeshifting stopped because a recording started which needed the card.
    /// </summary>
    RecordingStarted,
    /// <summary>
    /// Timeshifting stopped because client was kicked by server admin.
    /// </summary>
    KickedByAdmin,
    /// <summary>
    /// Timeshifting stopped because client heartbeat timed out.
    /// </summary>
    HeartBeatTimeOut,
    /// <summary>
    /// Timeshifting stopped because the owner of the same transponder has decided to change transponder.
    /// </summary>
    OwnerChangedTS
  }
}