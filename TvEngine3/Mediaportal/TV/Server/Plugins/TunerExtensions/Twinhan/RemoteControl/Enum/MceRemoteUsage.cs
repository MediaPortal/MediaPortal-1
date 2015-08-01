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
  // usage page 0xffbc, usage 0x88
  // http://msdn.microsoft.com/en-us/library/windows/desktop/bb417079.aspx
  // http://download.microsoft.com/download/0/7/E/07EF37BB-33EA-4454-856A-7D663BC109DF/Windows-Media-Center-RC-IR-Collection-Green-Button-Specification-03-08-2011-V2.pdf
  internal enum MceRemoteUsage
  {
    Undefined = 0,
    GreenButton = 0x0d,

    DvdMenu = 0x24,
    LiveTv,         // TV/jump

    Zoom = 0x27,
    Eject = 0x28,
    ClosedCaptioning = 0x2b,
    NetworkSelection = 0x2c,
    SubAudio = 0x2d,

    Ext0 = 0x32,
    Ext1,
    Ext2,
    Ext3,
    Ext4,
    Ext5,
    Ext6,
    Ext7,
    Ext8,

    Extras = 0x3c,
    ExtrasApp,
    Ten,
    Eleven,
    Twelve,
    ChannelInformation,
    ChannelInput,    // 3 digit input
    DvdTopMenu, // 0x43

    MyTv = 0x46,
    MyMusic,
    RecordedTv,
    MyPictures,
    MyVideos,
    DvdAngle,
    DvdAudio,
    DvdSubtitle,  // 0x4d

    Display = 0x4f,
    FmRadio = 0x50,

    TeletextOnOff = 0x5a,
    Red,
    Green,
    Yellow,
    Blue,

    Kiosk = 0x6a,
    Ext11 = 0x6f,

    BdTool = 0x78,

    Oem1 = 0x80,  // Ext9
    Oem2          // Ext10
  }
}