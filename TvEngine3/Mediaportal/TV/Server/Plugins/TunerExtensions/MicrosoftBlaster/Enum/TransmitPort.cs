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
using System.ComponentModel;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum
{
  [Flags]
  internal enum TransmitPort : uint
  {
    None = 0,
    [Description("1")]
    Port1 = 0x00000001,
    [Description("2")]
    Port2 = 0x00000002,
    [Description("3")]
    Port3 = 0x00000004,
    [Description("4")]
    Port4 = 0x00000008,
    [Description("5")]
    Port5 = 0x00000010,
    [Description("6")]
    Port6 = 0x00000020,
    [Description("7")]
    Port7 = 0x00000040,
    [Description("8")]
    Port8 = 0x00000080,
    [Description("9")]
    Port9 = 0x00000100,
    [Description("10")]
    Port10 = 0x00000200,
    [Description("11")]
    Port11 = 0x00000400,
    [Description("12")]
    Port12 = 0x00000800,
    [Description("13")]
    Port13 = 0x00001000,
    [Description("14")]
    Port14 = 0x00002000,
    [Description("15")]
    Port15 = 0x00004000,
    [Description("16")]
    Port16 = 0x00008000,
    [Description("17")]
    Port17 = 0x00010000,
    [Description("18")]
    Port18 = 0x00020000,
    [Description("19")]
    Port19 = 0x00040000,
    [Description("20")]
    Port20 = 0x00080000,
    [Description("21")]
    Port21 = 0x00100000,
    [Description("22")]
    Port22 = 0x00200000,
    [Description("23")]
    Port23 = 0x00400000,
    [Description("24")]
    Port24 = 0x00800000,
    [Description("25")]
    Port25 = 0x01000000,
    [Description("26")]
    Port26 = 0x02000000,
    [Description("27")]
    Port27 = 0x04000000,
    [Description("28")]
    Port28 = 0x08000000,
    [Description("29")]
    Port29 = 0x10000000,
    [Description("30")]
    Port30 = 0x20000000,
    [Description("31")]
    Port31 = 0x40000000,
    [Description("32")]
    Port32 = 0x80000000
  }
}