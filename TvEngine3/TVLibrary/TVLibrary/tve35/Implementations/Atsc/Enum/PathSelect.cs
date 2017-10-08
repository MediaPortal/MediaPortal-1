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
  // Refer to ATSC A/56 or SCTE 57 table 5.25, ATSC A/65 table 6.9, SCTE 65 table 5.29.
  internal enum PathSelect : byte
  {
    /// <summary>
    /// path 1
    /// </summary>
    Path1 = 0,
    /// <summary>
    /// path 2
    /// </summary>
    Path2 = 1
  }
}