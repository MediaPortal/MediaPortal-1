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
  // See ATSC A/65 table 6.5 or SCTE 65 table 5.28.
  internal enum ModulationMode : byte
  {
    Analog = 0x01,
    ScteMode1 = 0x02, // 64 QAM
    ScteMode2 = 0x03, // 256 QAM
    Atsc8Vsb = 0x04,
    Atsc16Vsb = 0x05,
    PrivateDescriptor = 0x80
  }
}