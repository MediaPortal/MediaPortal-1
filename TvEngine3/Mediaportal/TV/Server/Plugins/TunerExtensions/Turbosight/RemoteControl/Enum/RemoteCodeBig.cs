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
  /// Image:
  ///   v1 = http://www.tbsdtv.com/products/images/tbs6981/tbs6981_4.jpg
  ///   v2 = http://kubik-digital.com/wp-content/uploads/2013/10/41zlbmefDGL4.jpg
  /// Testing: v1 (TBS5980 CI), v2 (TBS5980 CI, TBS6991)
  /// </remarks>
  internal enum RemoteCodeBig : byte
  {
    Recall = 128,       // text [v1]: recall, text [v2]: back
    MINIMUM_VALUE = 128,
    Up,
    Right,
    Record,
    Power,
    Three,
    Two,
    One,
    Down,
    Six,
    Five,
    Four,
    VolumeDown, // 140  // overlay [v2]: blue
    Nine,
    Eight,
    Seven,
    Left,
    ChannelDown,        // overlay [v2]: yellow
    Zero,
    VolumeUp,           // overlay [v2]: green
    Mute,
    Favourites,         // overlay [v1]: green
    ChannelUp,  // 150  // overlay [v2]: red
    Subtitles,
    Pause,
    Okay,
    Screenshot,
    Mode,
    Epg,
    Zoom,               // overlay [v1]: yellow
    Menu,               // overlay [v1]: red
    Exit, // 159        // overlay [v1]: blue

    Asterix = 209,
    Hash = 210,
    Clear = 212,

    SkipForward = 216,
    SkipBack,
    FastForward,
    Rewind,
    Stop,
    Tv,
    Play  // 222
  }
}