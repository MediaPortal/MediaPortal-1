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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Scte.Enum
{
  // See ATSC A/56 or SCTE 57 table 5.6, SCTE 65 table 5.7.
  internal enum TransmissionSystem
  {
    Unknown = 0,
    ItutAnnex1,     // ITU ETSI cable (DVB-C); SCTE 65 reserved ETSI
    ItutAnnex2,     // ITU North American cable (SCTE)
    ItuR,           // ITU ETSI satellite; SCTE 65 defined for use in other systems
    Atsc,
    DigiCipher      // DC II satellite; SCTE 65 reserved satellite
  }
}