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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Atsc.Enum
{
  // Refer to ATSC A/56 or SCTE 57 table 5.16.
  internal enum MatrixMode : byte
  {
    /// <summary>
    /// Indicates that sub-carrier 1 carries the L+R (mono) audio channel.
    /// Sub-carrier 2, if present (specified as a different frequency than
    /// sub-carrier 1) carries cue tones or other audio.
    /// </summary>
    Mono = 0,
    /// <summary>
    /// Indicates that sub-carrier 1 carries the left audio channel, and
    /// sub-carrier 2 carries the right audio channel.
    /// </summary>
    DiscreteStereo = 1,
    /// <summary>
    /// Indicates that sub-carrier 1 carries the L+R (sum) vector and
    /// sub-carrier 2 carries the L-R (difference) vector.
    /// </summary>
    MatrixStereo = 2

    // (3 reserved)
  }
}