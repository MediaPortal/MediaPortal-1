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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Enum
{
  [Flags]
  internal enum TransmitZone : uint
  {
    None = 0,
    All = 1,
    [Description("1, External Right")]
    Zone1 = 2,
    [Description("2, External Left")]
    Zone2 = 4,
    [Description("3, Built In")]
    Zone3 = 8
  }
}