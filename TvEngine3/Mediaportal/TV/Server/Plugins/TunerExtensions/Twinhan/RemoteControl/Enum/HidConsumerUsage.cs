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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl.Enum
{
  // usage page 0x0c, usage 1
  internal enum HidConsumerUsage
  {
    NumericKeyPad = 0x2,
    MediaSelectProgramGuide = 0x8d,
    ChannelIncrement = 0x9c,
    ChannelDecrement = 0x9d,

    Play = 0xb0,
    Pause,
    Record,
    FastForward,
    Rewind,
    ScanNextTrack,
    ScanPreviousTrack,
    Stop, // 0xb7

    Mute = 0xe2,
    VolumeIncrement = 0xe9,
    VolumeDecrement = 0xea,
    ApplicationControlProperties = 0x209
  }
}