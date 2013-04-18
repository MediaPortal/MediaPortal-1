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

namespace MediaPortal.MusicPlayer.BASS
{
  /// <summary>
  /// Selected Audio Player
  /// </summary>
  public enum AudioPlayer
  {
    Bass = 0,
    Asio = 1,
    WasApi = 2,
    DShow = 3
  }

  /// <summary>
  /// States, how the Playback is handled
  /// </summary>
  public enum PlayBackType
  {
    NORMAL = 0,
    GAPLESS = 1,
    CROSSFADE = 2
  }

  public enum BassDSPDynAmpPreset
  {
    None = 0,
    Soft = 1,
    Medium = 2,
    Hard = 3
  }

  public enum PlayState
  {
    Buffering,
    Playing,
    Paused,
    Stopped
  }

  public enum FileMainType
  {
    Unknown = 0,
    WebStream = 1,
    MODFile = 2,
    AudioFile = 3,
    CDTrack = 4,
    MidiFile = 5
  }

  public enum FileSubType
  {
    None = 0,
    ASXWebStream = 1,
    LastFmWebStream = 3
  }

  // Values must match those used in MediaPortal (IPlayer.PlaybackType)
  public enum PlayBackMode
  {
    Normal = 0,
    Gapless = 1
  }

  public enum MonoUpMix
  {
    None = 0,
    Stereo = 1,
    QuadraphonicPhonic = 2,
    FiveDotOne = 3,
    SevenDotOne = 4
  }

  public enum StereoUpMix
  {
    None = 0,
    QuadraphonicPhonic = 1,
    FiveDotOne = 2,
    SevenDotOne = 3
  }

  public enum FiveDotOneUpMix
  {
    None = 0,
    SevenDotOne = 1,
  }

  public enum QuadraphonicUpMix
  {
    None = 0,
    FiveDotOne = 1,
    SevenDotOne = 2
  }
}
