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

using System.Runtime.InteropServices;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2.Struct
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal struct TunerCapabilities
  {
    public TunerType TunerType;
    [MarshalAs(UnmanagedType.Bool)]
    public bool IsConstellationSupported;     // Is SetModulation() supported?
    [MarshalAs(UnmanagedType.Bool)]
    public bool IsFecSupported;               // Is SetFec() suppoted?
    public uint MinTransponderFrequency;      // unit = kHz
    public uint MaxTransponderFrequency;      // unit = kHz
    public uint MinTunerFrequency;            // unit = kHz
    public uint MaxTunerFrequency;            // unit = kHz
    public uint MinSymbolRate;                // unit = Baud
    public uint MaxSymbolRate;                // unit = Baud
    private uint AutoSymbolRate;              // Obsolete		
    public PerformanceMonitoringCapability PerformanceMonitoringCapabilities;
    public uint LockTime;                     // unit = ms
    public uint KernelLockTime;               // unit = ms
    public AcquisitionCapability AcquisitionCapabilities;
  }
}