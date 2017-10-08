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
  // Refer to ATSC A/65 table 6.5 or SCTE 65 table 5.28.
  internal enum ModulationMode : byte
  {
    // (0 reserved)

    /// <summary>
    /// The virtual channel is modulated using standard analog methods for
    /// analog television.
    /// </summary>
    Analog = 0x01,
    /// <summary>
    /// The virtual channel has a symbol rate of 5.057 Ms/s, transmitted in
    /// accordance with Digital Transmission Standard for Cable Television.
    /// Typically, mode 1 will be used for 64-QAM.
    /// </summary>
    ScteMode1 = 0x02,
    /// <summary>
    /// The virtual channel has a symbol rate of 5.361 Ms/s, transmitted in
    /// accordance with Digital Transmission Standard for Cable Television.
    /// Typically, mode 2 will be used for 256-QAM.
    /// </summary>
    ScteMode2 = 0x03,
    /// <summary>
    /// The virtual channel uses the 8-VSB modulation method conforming to the
    /// ATSC Digital Television Standard.
    /// </summary>
    Atsc8Vsb = 0x04,
    /// <summary>
    /// The virtual channel uses the 16-VSB modulation method conforming to the
    /// ATSC Digital Television Standard.
    /// </summary>
    Atsc16Vsb = 0x05,

    // (0x06 to 0x7f reserved for future use)

    /// <summary>
    /// Modulation parameters are defined by a private descriptor.
    /// </summary>
    PrivateDescriptor = 0x80

    // (0x81 to 0xff user private)
  }
}