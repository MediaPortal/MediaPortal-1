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
  [Flags]
  public enum FrameSize
  {
    Automatic = 0,
    [Description("320x240 NTSC CIF square pixels")]
    Fs320_240 = 0x00000001,
    [Description("352x240 NTSC CIF")]
    Fs352_240 = 0x00000002,
    [Description("352x288 PAL CIF")]
    Fs352_288 = 0x00000004,
    [Description("384x288")]
    Fs384_288 = 0x00000008,
    [Description("480x360")]
    Fs480_360 = 0x00000010,
    [Description("640x360")]
    Fs640_360 = 0x00000020,
    [Description("640x480 NTSC square pixels")]
    Fs640_480 = 0x00000040,
    [Description("704x480 NTSC TV broadcast")]
    Fs704_480 = 0x00000080,
    [Description("704x576 PAL/SECAM TV broadcast")]
    Fs704_576 = 0x00000100,
    [Description("720x480 NTSC ITU-601 D1 (recommended)")]
    Fs720_480 = 0x00000200,
    [Description("720x576 PAL/SECAM ITU-601 D1 (recommended)")]
    Fs720_576 = 0x00000400,
    [Description("768x480")]
    Fs768_480 = 0x00000800,
    [Description("768x576 PAL square pixels")]
    Fs768_576 = 0x00001000,
    [Description("1280x720 HD")]
    Fs1280_720 = 0x00002000,
    [Description("1440x1080")]
    Fs1440_1080 = 0x00004000,
    [Description("1920x1080 full HD")]
    Fs1920_1080 = 0x00008000
  }
}