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
  // Refer to ATSC A/56 or SCTE 57 table 5.24, SCTE 65 table 5.21.
  internal enum VideoStandard : byte
  {
    /// <summary>
    /// The video standard is NTSC.
    /// </summary>
    Ntsc = 0,
    /// <summary>
    /// The video standard is 625-line PAL.
    /// </summary>
    Pal625 = 1,
    /// <summary>
    /// The video standard is 525-line PAL.
    /// </summary>
    Pal525 = 2,
    /// <summary>
    /// The video standard is SECAM.
    /// </summary>
    Secam = 3,
    /// <summary>
    /// The video standard is MAC.
    /// </summary>
    Mac = 4

    // (5 - 15 reserved)
  }
}