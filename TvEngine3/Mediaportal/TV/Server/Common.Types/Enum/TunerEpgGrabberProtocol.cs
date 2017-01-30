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

using System;

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  [Flags]
  public enum TunerEpgGrabberProtocol
  {
    None = 0,

    // protocols
    DvbEit = 0x01,
    OpenTv = 0x02,                      // Sky [IT/NZ/UK, satellite], Foxtel [AU, satellite/cable]
    MediaHighway1 = 0x04,               // Canalsat [FR, satellite/cable/terrestrial/IPTV], Canal Digitaal [NL, satellite], Cyfra+ [PL, satellite]
    MediaHighway2 = 0x08,               // Canal+/Digital+ [ES, satellite]
    AtscEit = 0x10,                     // terrestrial CA/US
    ScteAeit = 0x20,                    // cable CA/US
    Premiere = 0x40,                    // Sky [DE, satellite]

    // variant formats
    Freesat = 0x02000000,               // UK, satellite
    MultiChoice = 0x04000000,           // ZA, satellite
    OrbitShowtimeNetwork = 0x08000000,  // Middle East & North Africa, satellite
    ViasatSweden = 0x10000000,          // SE, satellite
    BellTv = 0x20000000,                // CA, satellite
    DishNetwork = 0x40000000            // US, satellite
  }
}