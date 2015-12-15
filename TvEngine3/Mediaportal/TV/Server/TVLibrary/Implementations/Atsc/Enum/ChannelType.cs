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
  // Refer to ATSC A/56 or SCTE 57 table 5.22, SCTE 65 table 5.20.
  internal enum ChannelType : byte
  {
    /// <summary>
    /// Normal.
    /// </summary>
    Normal = 0,
    /// <summary>
    /// Hidden.
    /// </summary>
    Hidden = 1,
    /// <summary>
    /// ATSC A/56, SCTE 57: local access.
    /// SCTE 65: reserved.
    /// </summary>
    LocalAccess = 2,
    /// <summary>
    /// ATSC A/56, SCTE 57: near video on demand access.
    /// SCTE 65: reserved.
    /// </summary>
    NvodAccess = 3

    // (4 to 15 reserved)
  }
}