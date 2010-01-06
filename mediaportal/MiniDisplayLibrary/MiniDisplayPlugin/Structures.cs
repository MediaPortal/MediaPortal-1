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
using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public enum DefaultControl
  {
    BackLight = 0x41,
    DisplayControls = 40,
    DisplayOptions = 30,
    Equalizer = 20,
    KeyPad = 60,
    Main = 3,
    RemoteControl = 50
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct DisplayControl
  {
    public bool BlankDisplayWithVideo;
    public bool EnableDisplayAction;
    public int DisplayActionTime;
    public bool BlankDisplayWhenIdle;
    public int BlankIdleDelay;
    public long _BlankIdleTime;
    public long _BlankIdleTimeout;
    public bool _DisplayControlAction;
    public long _DisplayControlLastAction;
    public long _DisplayControlTimeout;
    public string _Shutdown1;
    public string _Shutdown2;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct DisplayOptions
  {
    public bool DiskIcon;
    public bool VolumeDisplay;
    public bool ProgressDisplay;
    public bool DiskMediaStatus;
    public bool DiskMonitor;
    public bool UseCustomFont;
    public bool UseLargeIcons;
    public bool UseCustomIcons;
    public bool UseInvertedIcons;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct DisplayOptionsLayout
  {
    public bool Volume;
    public bool Progress;
    public bool DiskIcon;
    public bool MediaStatus;
    public bool DiskStatus;
    public bool CustomFont;
    public bool LargeIcons;
    public bool CustomIcons;
    public bool InvertIcons;
    public bool FontEditor;
    public bool IconEditor;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct KeyPadLayout
  {
    public bool EnableKeyPad;
    public bool EnableCustom;
    public bool KeyPadMapping;
    public bool Label1;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct RemoteLayout
  {
    public bool DisableRemote;
    public bool DisableRepeat;
    public bool RepeatDelay;
    public bool RemoteMapping;
    public bool Label1;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct MainMenuLayout
  {
    public bool LabelInfo1;
    public bool LabelInfo2;
    public bool Backlight;
    public bool DisplayControl;
    public bool DisplayOptions;
    public bool Equalizer;
    public bool KeyPad;
    public bool Remote;
    public bool Contrast;
    public bool MonitorPower;
    public bool Brightness;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct EQControl
  {
    public int Render_BANDS;
    public int Render_MaxValue;
    public bool UseEqDisplay;
    public bool UseNormalEq;
    public bool UseStereoEq;
    public bool UseVUmeter;
    public bool UseVUmeter2;
    public bool RestrictEQ;
    public bool SmoothEQ;
    public bool DelayEQ;
    public bool _useVUindicators;
    public int _useEqMode;
    public bool EQTitleDisplay;
    public int _DelayEQTime;
    public int _EQTitleDisplayTime;
    public int _EQTitleShowTime;
    public bool _EqDataAvailable;
    public bool _EQDisplayTitle;
    public DateTime _LastEQupdate;
    public float[] EqFftData;
    public byte[] EqArray;
    public int[] LastEQ;
    public double _LastEQTitle;
    public int _Max_EQ_FPS;
    public int _EQ_Framecount;
    public int _EQ_Restrict_FPS;
    public int _EqUpdateDelay;
    public DateTime _EQ_FPS_time;
    public bool _AudioIsMixing;
    public bool _AudioUseASIO;
  }

  public enum PluginIcons : ulong
  {
    ICON_AC3 = 0x10000000L,
    ICON_Alarm = 0x800000000L,
    ICON_CD_DVD = 0x10L,
    ICON_DivX = 0x10000L,
    ICON_DTS = 0x8000000L,
    ICON_FFWD = 0x20000000000L,
    ICON_FIT = 0x400000L,
    ICON_FRWD = 0x10000000000L,
    ICON_HDTV = 0x100000L,
    ICON_Movie = 0x40L,
    ICON_MP3 = 0x2000000L,
    ICON_MPG = 0x20000L,
    ICON_MPG2 = 0x20000000L,
    ICON_Music = 0x80L,
    ICON_News = 2L,
    ICON_OGG = 0x1000000L,
    ICON_Pause = 0x80000000000L,
    ICON_Photo = 0x20L,
    ICON_Play = 0x100000000000L,
    ICON_Rec = 0x400000000L,
    ICON_REP = 0x2000000000L,
    ICON_SCR1 = 0x80000L,
    ICON_SCR2 = 0x40000L,
    ICON_SFL = 0x1000000000L,
    ICON_SPDIF = 0x200L,
    ICON_SRC = 0x800000L,
    ICON_Stop = 0x40000000000L,
    ICON_Time = 0x100000000L,
    ICON_TV = 8L,
    ICON_TV_2 = 0x200000L,
    ICON_Vol = 0x200000000L,
    ICON_WAV = 0x4000000000L,
    ICON_WebCast = 4L,
    ICON_WMA = 0x4000000L,
    ICON_WMA2 = 0x8000000000L,
    ICON_WMV = 0x40000000L,
    ICON_xVid = 0x80000000L,
    SPKR_FC = 0x8000L,
    SPKR_FL = 1L,
    SPKR_FR = 0x4000L,
    SPKR_LFE = 0x1000L,
    SPKR_RL = 0x400L,
    SPKR_RR = 0x100L,
    SPKR_SL = 0x2000L,
    SPKR_SR = 0x800L
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct SystemStatus
  {
    public Status CurrentPluginStatus;
    public ulong CurrentIconMask;
    public bool MP_Is_Idle;
    public int SystemVolumeLevel;
    public bool IsMuted;
    public bool MediaPlayer_Active;
    public bool MediaPlayer_Playing;
    public bool MediaPlayer_Paused;
    public bool Media_IsRecording;
    public bool Media_IsTV;
    public bool Media_IsTVRecording;
    public bool Media_IsDVD;
    public bool Media_IsCD;
    public bool Media_IsRadio;
    public bool Media_IsVideo;
    public bool Media_IsMusic;
    public bool Media_IsTimeshifting;
    public double Media_CurrentPosition;
    public double Media_Duration;
    public int Media_Speed;
    public bool _AudioIsMixing;
    public bool _AudioUseASIO;
    public bool _AudioUseMasterVolume;
    public bool _AudioUseWaveVolume;
  }

  public enum WindowIDs
  {
    WindowID_BackLight = 0x4dac,
    WindowID_DisplayControl = 0x4da9,
    WindowID_DisplayOptions = 0x4da8,
    WindowID_Equalizer = 0x4da7,
    WindowID_Internal = 0x4da5,
    WindowID_KeyPad = 0x4dab,
    WindowID_Main = 0x4da6,
    WindowID_RemoteControl = 0x4daa
  }
}