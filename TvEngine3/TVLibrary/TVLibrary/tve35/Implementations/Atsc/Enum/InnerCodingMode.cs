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
  // Refer to ATSC A/56 or SCTE 57 table 5.7, SCTE 65 5.8.
  internal enum InnerCodingMode : byte
  {
    /// <summary>
    /// convolution code rate 5/11
    /// </summary>
    Rate5_11 = 0,
    /// <summary>
    /// convolution code rate 1/2
    /// </summary>
    Rate1_2 = 1,

    // (2 reserved)

    /// <summary>
    /// convolution code rate 3/5
    /// </summary>
    Rate3_5 = 3,

    // (4 reserved)

    /// <summary>
    /// convolution code rate 2/3
    /// </summary>
    Rate2_3 = 5,

    // (6 reserved)

    /// <summary>
    /// convolution code rate 3/4
    /// </summary>
    Rate3_4 = 7,
    /// <summary>
    /// convolution code rate 4/5
    /// </summary>
    Rate4_5 = 8,
    /// <summary>
    /// convolution code rate 5/6
    /// </summary>
    Rate5_6 = 9,

    // (10 reserved)

    /// <summary>
    /// convolution code rate 7/8
    /// </summary>
    Rate7_8 = 11,

    // (12 to 14 reserved)

    /// <summary>
    /// Concatenated coding is not used.
    /// </summary>
    None = 15
  }
}