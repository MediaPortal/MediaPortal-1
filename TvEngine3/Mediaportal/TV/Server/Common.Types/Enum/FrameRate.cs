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
  public enum FrameRate
  {
    Automatic = 0,
    [Description("15")]
    Fr15 = 1,
    [Description("23.976")]
    Fr23_976 = 2,
    [Description("24")]
    Fr24 = 4,
    [Description("25 (PAL/SECAM)")]
    Fr25 = 8,
    [Description("29.97 (NTSC)")]
    Fr29_97 = 16,
    [Description("30")]
    Fr30 = 32,
    [Description("50 (PAL/SECAM)")]
    Fr50 = 64,
    [Description("59.94 (NTSC)")]
    Fr59_94 = 128,
    [Description("60")]
    Fr60 = 256
  }
}