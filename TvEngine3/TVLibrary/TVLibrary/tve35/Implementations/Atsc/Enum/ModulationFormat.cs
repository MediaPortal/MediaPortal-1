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
  // Refer to SCTE 65 table 5.9.
  // ATSC A/56 and SCTE 57 also define modulation format in table 5.8. However
  // values higher than QAM 64 are not consistent with SCTE 65.
  internal enum ModulationFormat : byte
  {
    /// <summary>
    /// The modulation format is unknown.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The modulation format is QPSK (Quadrature Phase Shift Keying).
    /// </summary>
    Qpsk = 1,
    /// <summary>
    /// The modulation format is BPSK (Binary Phase Shift Keying).
    /// </summary>
    Bpsk = 2,
    /// <summary>
    /// The modulation format is offset QPSK.
    /// </summary>
    Oqpsk = 3,
    /// <summary>
    /// The modulation format is 8-level VSB (Vestigial Sideband).
    /// </summary>
    Vsb8 = 4,
    /// <summary>
    /// The modulation format is 16-level VSB.
    /// </summary>
    Vsb16 = 5,
    /// <summary>
    /// Modulation format 16-level Quadrature Amplitude Modulation (QAM).
    /// </summary>
    Qam16 = 6,
    /// <summary>
    /// 32-level QAM.
    /// </summary>
    Qam32 = 7,
    /// <summary>
    /// 64-level QAM.
    /// </summary>
    Qam64 = 8,
    /// <summary>
    /// 80-level QAM.
    /// </summary>
    Qam80 = 9,
    /// <summary>
    /// 96-level QAM.
    /// </summary>
    Qam96 = 10,
    /// <summary>
    /// 112-level QAM.
    /// </summary>
    Qam112 = 11,
    /// <summary>
    /// 128-level QAM.
    /// </summary>
    Qam128 = 12,
    /// <summary>
    /// 160-level QAM.
    /// </summary>
    Qam160 = 13,
    /// <summary>
    /// 192-level QAM.
    /// </summary>
    Qam192 = 14,
    /// <summary>
    /// 224-level QAM.
    /// </summary>
    Qam224 = 15,
    /// <summary>
    /// 256-level QAM.
    /// </summary>
    Qam256 = 16,
    /// <summary>
    /// 320-level QAM.
    /// </summary>
    Qam320 = 17,
    /// <summary>
    /// 384-level QAM.
    /// </summary>
    Qam384 = 18,
    /// <summary>
    /// 448-level QAM.
    /// </summary>
    Qam448 = 19,
    /// <summary>
    /// 512-level QAM.
    /// </summary>
    Qam512 = 20,
    /// <summary>
    /// 640-level QAM.
    /// </summary>
    Qam640 = 21,
    /// <summary>
    /// 768-level QAM.
    /// </summary>
    Qam768 = 22,
    /// <summary>
    /// 896-level QAM.
    /// </summary>
    Qam896 = 23,
    /// <summary>
    /// 1024-level QAM.
    /// </summary>
    Qam1024 = 24

    // (25 to 31 reserved)
  }
}