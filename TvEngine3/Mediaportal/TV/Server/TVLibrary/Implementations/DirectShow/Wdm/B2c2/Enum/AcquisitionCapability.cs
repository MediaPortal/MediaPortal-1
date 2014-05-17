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

using System;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum
{
  [Flags]
  internal enum AcquisitionCapability
  {
    None = 0,
    AutoSymbolRate = 1,       // DVB-S automatic symbol rate detection support.
    AutoGuardInterval = 2,    // DVB-T automatic guard interval detection support.
    AutoFrequencyOffset = 4,  // DVB-T automatic frequency offset handling support.
    VhfSupport = 8,           // DVB-T VHF (30 - 300 MHz) support.
    Bandwidth6 = 16,          // DVB-T 6 MHz transponder support.
    Bandwidth7 = 32,          // DVB-T 7 MHz transponder support.
    Bandwidth8 = 64,          // DVB-T 8 MHz transponder support.
    RawDiseqc = 128,          // DVB-S DiSEqC 1.2 16 port switch and motor control support.
    IrInput = 256             // IR remote input support.
  }
}