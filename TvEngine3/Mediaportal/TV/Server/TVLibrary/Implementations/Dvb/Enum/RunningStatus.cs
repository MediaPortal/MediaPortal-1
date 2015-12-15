#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum
{
  /// <summary>
  /// DVB service and event running statuses. See EN 300 468 table 6.
  /// </summary>
  internal enum RunningStatus : byte
  {
    // (0x00 undefined)

    /// <summary>
    /// not running
    /// </summary>
    NotRunning = 1,
    /// <summary>
    /// starts in a few seconds (eg. for video recording)
    /// </summary>
    StartsInAFewSeconds = 2,
    /// <summary>
    /// pausing
    /// </summary>
    Pausing = 3,
    /// <summary>
    /// running
    /// </summary>
    Running = 4,
    /// <summary>
    /// service off-air
    /// </summary>
    ServiceOffAir = 5

    // (6 to 7 reserved for future use)
  }
}