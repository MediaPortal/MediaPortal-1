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

using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.MCEDisplay_Interop
{
  [ComVisible(false)]
  public enum MediaStatusPropertyTag
  {
    Application = 0xf001,
    ArtistName = 0x2018,
    CallingPartyName = 0x2029,
    CallingPartyNumber = 0x2028,
    CD = 0x2002,
    CurrentPicture = 0x201b,
    DiscWriter_ProgressPercentageChanged = 0x202f,
    DiscWriter_ProgressTimeChanged = 0x202e,
    DiscWriter_SelectedFormat = 0x202d,
    DiscWriter_Start = 0x202c,
    DiscWriter_Stop = 0x2030,
    DVD = 0x2001,
    Ejecting = 0x1010,
    Error = 0x100f,
    FF1 = 0x100a,
    FF2 = 0x100b,
    FF3 = 0x100c,
    FS_DVD = 0x2010,
    FS_Extensibility = 0x2016,
    FS_Guide = 0x2011,
    FS_Home = 0x200e,
    FS_Music = 0x2012,
    FS_Photos = 0x2013,
    FS_Radio = 0x2025,
    FS_RecordedShows = 0x2015,
    FS_TV = 0x200f,
    FS_Unknown = 0x2016,
    FS_Videos = 0x2014,
    GuideLoaded = 0x201d,
    MediaControl = -8,
    MediaName = 0x2017,
    MediaTime = 0x2007,
    MediaTypes = 0x2000,
    MSASPrivateTags = 0xf000,
    Mute = 0x1000,
    Navigation = -7,
    Next = 0x100d,
    NextFrame = 0x2021,
    ParentalAdvisoryRating = 0x202a,
    Pause = 0x1002,
    PhoneCall = 0x2027,
    Photos = 0x201a,
    Play = 0x1001,
    Prev = 0x100e,
    PrevFrame = 0x2022,
    PVR = 0x2003,
    Radio = 0x2023,
    RadioFrequency = 0x2024,
    Recording = 0x1006,
    RepeatSet = 0x1005,
    RequestForTuner = 0x202b,
    Rewind1 = 0x1007,
    Rewind2 = 0x1008,
    Rewind3 = 0x1009,
    SessionEnd = -9,
    SessionStart = -10,
    Shuffle = 0x1004,
    SlowMotion1 = 0x201e,
    SlowMotion2 = 0x201f,
    SlowMotion3 = 0x2020,
    Stop = 0x1003,
    StreamingContentAudio = 0x2004,
    StreamingContentVideo = 0x2005,
    TitleNumber = 0x200c,
    TotalTracks = 0x2009,
    TrackDuration = 0x200a,
    TrackName = 0x2019,
    TrackNumber = 0x2008,
    TrackTime = 0x200b,
    TransitionTime = 0x201c,
    TVTuner = 0x2006,
    Unknown = 0,
    Visualization = 0x2026,
    Volume = 0x200d
  }
}