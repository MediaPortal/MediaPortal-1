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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum
{
  internal enum KeyType
  {
    // PID TSC Keys. Up to 7 Keys possible; highest priority used.
    PidTscReserved = 1,   // Key is used if the PID matches and the scrambling control bits are '01' (reserved).
    PidTscEven = 2,       // Key is used if the PID matches and the scrambling control bits are '10' (even).
    PidTscOdd = 3,        // Key is used if the PID matches and the scrambling control bits are '11' (odd).
    // PID-only Key. 1 key only; used if no global key is set.
    PidTscAny,            // Key is used if none of the scrambling patterns matches and the PID value matches.
    // Global Key. 1 key only; used if no PID-only key is set.
    Global,               // Key is used if no other key matches.
  }
}