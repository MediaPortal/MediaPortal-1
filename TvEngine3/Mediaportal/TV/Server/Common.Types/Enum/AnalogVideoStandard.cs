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
using System.ComponentModel;

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  /// <summary>
  /// From AnalogVideoStandard
  /// </summary>
  [Flags]
  public enum AnalogVideoStandard
  {
    None = 0x00000000,
    [Description("NTSC M")]
    NtscM = 0x00000001,
    [Description("NTSC J")]
    NtscMj = 0x00000002,
    [Description("NTSC 4.43")]
    Ntsc433 = 0x00000004,
    [Description("PAL B")]
    PalB = 0x00000010,
    [Description("PAL D")]
    PalD = 0x00000020,
    [Description("PAL G")]
    PalG = 0x00000040,
    [Description("PAL H")]
    PalH = 0x00000080,
    [Description("PAL I")]
    PalI = 0x00000100,
    [Description("PAL M")]
    PalM = 0x00000200,
    [Description("PAL N")]
    PalN = 0x00000400,
    [Description("PAL 60")]
    Pal60 = 0x00000800,
    [Description("SECAM B")]
    SecamB = 0x00001000,
    [Description("SECAM D")]
    SecamD = 0x00002000,
    [Description("SECAM G")]
    SecamG = 0x00004000,
    [Description("SECAM H")]
    SecamH = 0x00008000,
    [Description("SECAM K")]
    SecamK = 0x00010000,
    [Description("SECAM K1")]
    SecamK1 = 0x00020000,
    [Description("SECAM L")]
    SecamL = 0x00040000,
    [Description("SECAM L1")]
    SecamL1 = 0x00080000,
    [Description("PAL N Combo")]
    PalNCombo = 0x00100000
  }
}