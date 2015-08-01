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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.RemoteControl.Enum
{
  /// <remarks>
  /// Image: [none]
  /// Testing: untested, based on old SDK.
  /// </remarks>
  internal enum RemoteCodeSmall : byte
  {
    Mute = 1,
    Left,
    Down,
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,  // 10

    FullScreen = 12,
    Okay = 15,
    Exit = 18,
    Right = 26,
    Eight = 27,
    Up = 30,
    Nine = 31
  }
}