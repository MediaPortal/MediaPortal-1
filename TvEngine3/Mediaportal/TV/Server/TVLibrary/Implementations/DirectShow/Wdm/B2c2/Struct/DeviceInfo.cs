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
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  internal struct DeviceInfo
  {
    public uint DeviceId;
    public MacAddress MacAddress;
    private ushort Padding1;
    public TunerType TunerType;
    public BusType BusInterface;
    [MarshalAs(UnmanagedType.I1)]
    public bool IsInUse;
    private byte Padding2;
    private ushort Padding3;
    public uint ProductId;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string ProductName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
    public string ProductDescription;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
    public string ProductRevision;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 61)]
    public string ProductFrontEnd;
  }
}