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
  // Refer to ATSC A/56 or SCTE 57 table 5.14.
  internal enum WaveformStandard : byte
  {
    /// <summary>
    /// The waveform type is unknown or undefined at this time.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The waveform is standard NTSC.
    /// </summary>
    Ntsc = 1,
    /// <summary>
    /// The waveform is standard 625-line PAL.
    /// </summary>
    Pal625 = 2,
    /// <summary>
    /// The waveform is standard 525-line PAL.
    /// </summary>
    Pal525 = 3,
    /// <summary>
    /// The waveform is standard SECAM.
    /// </summary>
    Secam = 4,
    /// <summary>
    /// The waveform is standard D2-MAC.
    /// </summary>
    D2Mac = 5,
    /// <summary>
    /// The waveform is standard B-MAC.
    /// </summary>
    Bmac = 6,
    /// <summary>
    /// The waveform is standard c-MAC.
    /// </summary>
    Cmac = 7,
    /// <summary>
    /// The waveform conforms to the General Instrument DigiCipher I scrambling standard.
    /// </summary>
    Dci = 8,
    /// <summary>
    /// The waveform conforms to the General Instrument VideoCipher scrambling standard.
    /// </summary>
    VideoCipher = 9,
    /// <summary>
    /// The waveform conforms to the RCA DSS system.
    /// </summary>
    RcaDss = 10,
    /// <summary>
    /// The waveform is scrambled using the TvCom (formerly Oak) Orion system.
    /// </summary>
    Orion = 11,
    /// <summary>
    /// The waveform is scrambled using the Leitch system.
    /// </summary>
    Leitch = 12

    // (13 - 31 reserved)
  }
}