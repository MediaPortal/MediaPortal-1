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
  // Refer to ATSC A/56 or SCTE 57 table 5.6, SCTE 65 table 5.7.
  internal enum TransmissionSystem : byte
  {
    /// <summary>
    /// The transmission system is not known.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// ATSC A/56, SCTE 57: the transmission system conforms to ITU-T Rec. J.83
    /// Annex A (DVB-C).
    /// SCTE 65: reserved ETSI.
    /// </summary>
    ItutJ83AnnexA = 1,
    /// <summary>
    /// The transmission system conforms to ITU-T Rec. J.83 Annex B
    /// (SCTE/OpenCable).
    /// </summary>
    ItutJ83AnnexB = 2,
    /// <summary>
    /// ATSC A/56, SCTE 57: the transmission system conforms to ITU-R Rec.
    /// BO.1211 (ETSI satellite).
    /// SCTE 65: defined for use in other systems.
    /// </summary>
    IturBo1211 = 3,
    /// <summary>
    /// The transmission system conforms to the ATSC Digital Television
    /// Standard.
    /// </summary>
    Atsc = 4,
    /// <summary>
    /// ATSC A/56, SCTE 57: the transmission system conforms to the General
    /// Instrument DigiCipher II System for satellite distribution of
    /// compressed audio and video.
    /// SCTE 65: reserved satellite.
    /// </summary>
    DigiCipher = 5

    // (6 to 15 reserved)
  }
}