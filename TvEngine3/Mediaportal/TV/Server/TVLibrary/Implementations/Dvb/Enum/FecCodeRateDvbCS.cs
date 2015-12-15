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
  /// DVB FEC code rates for DVB-C and DVB-S/S2. See EN 300 468 table 35.
  /// </summary>
  internal enum FecCodeRateDvbCS : byte
  {
    // (0 not defined)

    /// <summary>
    /// convolution code rate 1/2
    /// </summary>
    Rate1_2 = 1,
    /// <summary>
    /// convolution code rate 2/3
    /// </summary>
    Rate2_3 = 2,
    /// <summary>
    /// convolution code rate 3/4
    /// </summary>
    Rate3_4 = 3,
    /// <summary>
    /// convolution code rate 5/6
    /// </summary>
    Rate5_6 = 4,
    /// <summary>
    /// convolution code rate 7/8
    /// </summary>
    Rate7_8 = 5,
    /// <summary>
    /// convolution code rate 8/9
    /// </summary>
    Rate8_9 = 6,
    /// <summary>
    /// convolution code rate 3/5
    /// </summary>
    Rate3_5 = 7,
    /// <summary>
    /// convolution code rate 4/5
    /// </summary>
    Rate4_5 = 8,
    /// <summary>
    /// convolution code rate 9/10
    /// </summary>
    Rate9_10 = 9,

    // (10 to 14 reserved for future use)

    /// <summary>
    /// no convolutional coding
    /// </summary>
    NoConvolutionalCoding = 15
  }
}