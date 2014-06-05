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
  internal enum Error
  {
    NotLockedOnSignal = -1878982379,    // [0x90010115]

    // For AddPIDsToPin() or AddPIDs()...
    AlreadyExists = 0x10011000,         // PID already registered.
    PidError = -1878978559,             // [0x90011001]
    AlreadyFull = -1878978558,          // Max PID count exceeded. [0x90011002]

    // General...
    CreateInterface = -1878917119,      // Not all interfaces could be created correctly. [0x90020001]
    UnsupportedDevice = -1878917118,    // The given device is not B2C2-compatible (Linux). [0x90020002]
    NotInitialised,                     // Initialize() needs to be called.

    // (B2C2MPEG2AdapterWin.cpp code...)
    InvalidPin,
    NoTsFilter,
    PinAlreadyConnected,
    NoInputPin,

    InvalidTid,                         // Invalid table ID passed to SetTableId().

    // Decrypt keys...
    SetGlobalFixedKey,
    SetPidFixedKey,
    GetPidFixedKeys,
    DeletePidFixedKey,
    PurgeFixedKey,

    DiseqcInProgress = -1878917105,     // [0x9002000f]
    Diseqc12NotSupported,
    NoDeviceAvailable
  }
}