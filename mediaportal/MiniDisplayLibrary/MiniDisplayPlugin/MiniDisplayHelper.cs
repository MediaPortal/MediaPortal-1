using System;
using System.IO;
using DShowNET.AudioMixer;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Un4seen.Bass;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class MiniDisplayHelper
  {
    private static int _IdleTimeout = 5;
    public static bool _PropertyBrowserAvailable = false;
    public static SystemStatus MPStatus;
    public static object PropertyBrowserMutex = new object();
    public static object StatusMutex = new object();
    public static bool UseTVServer = false;

    public static void DisablePropertyBrowser()
    {
      lock (PropertyBrowserMutex)
      {
        _PropertyBrowserAvailable = false;
      }
    }

    public static bool GetEQ(ref EQControl EQSETTINGS)
    {
      bool extensiveLogging = Settings.Instance.ExtensiveLogging;
      bool flag2 = (EQSETTINGS.UseStereoEq | EQSETTINGS.UseVUmeter) | EQSETTINGS.UseVUmeter2;
      if (g_Player.Player != null)
      {
        if (!EQSETTINGS.UseEqDisplay)
        {
          return false;
        }
        if (EQSETTINGS._AudioUseASIO)
        {
          return false;
        }
        try
        {
          if (EQSETTINGS.DelayEQ & (g_Player.CurrentPosition < EQSETTINGS._DelayEQTime))
          {
            EQSETTINGS._EQDisplayTitle = false;
            EQSETTINGS._LastEQTitle = (DateTime.Now.Ticks/1000);
            return false;
          }
          if (EQSETTINGS.EQTitleDisplay)
          {
            if (g_Player.CurrentPosition < EQSETTINGS._EQTitleDisplayTime)
            {
              EQSETTINGS._EQDisplayTitle = false;
            }
            if (((DateTime.Now.Ticks/1000) - EQSETTINGS._LastEQTitle) > (EQSETTINGS._EQTitleDisplayTime*10000))
            {
              EQSETTINGS._LastEQTitle = (DateTime.Now.Ticks/1000);
              EQSETTINGS._EQDisplayTitle = !EQSETTINGS._EQDisplayTitle;
            }
            if (EQSETTINGS._EQDisplayTitle &
                (((DateTime.Now.Ticks/1000) - EQSETTINGS._LastEQTitle) < (EQSETTINGS._EQTitleShowTime*10000)))
            {
              return false;
            }
          }
        }
        catch
        {
          EQSETTINGS._EQDisplayTitle = false;
          EQSETTINGS._LastEQTitle = (DateTime.Now.Ticks/1000);
          return false;
        }
        int handle = -1;
        try
        {
          handle = g_Player.Player.CurrentAudioStream;
        }
        catch (Exception exception)
        {
          Log.Debug("MiniDisplay.GetEQ(): Caugth exception obtaining audio stream: {0}", new object[] {exception});
          return false;
        }
        if ((handle != 0) & (handle != -1))
        {
          int num2;
          if (extensiveLogging)
          {
            Log.Info("MiniDisplay.GetEQ(): attempting to retrieve equalizer data from audio stream {0}",
                     new object[] {handle});
          }
          try
          {
            int num3;
            if (flag2)
            {
              num3 = -2147483630;
            }
            else
            {
              num3 = -2147483646;
            }
            num2 = Bass.BASS_ChannelGetData(handle, EQSETTINGS.EqFftData, num3);
          }
          catch
          {
            if (extensiveLogging)
            {
              Log.Info("MiniDisplay.GetEQ(): CAUGHT EXCeption - audio stream {0} disappeared", new object[] {handle});
            }
            return false;
          }
          if (num2 > 0)
          {
            return true;
          }
          if (extensiveLogging)
          {
            Log.Info("MiniDisplay.GetEQ(): unable to retreive equalizer data");
          }
          return false;
        }
        if (extensiveLogging)
        {
          Log.Info("MiniDisplay.GetEQ(): Audio Stream not available");
        }
      }
      return false;
    }

    public static void GetSystemStatus(ref SystemStatus CurrentStatus)
    {
      lock (StatusMutex)
      {
        CurrentStatus.CurrentPluginStatus = MPStatus.CurrentPluginStatus;
        CurrentStatus.CurrentIconMask = MPStatus.CurrentIconMask;
        CurrentStatus.MP_Is_Idle = MPStatus.MP_Is_Idle;
        GetSystemVolume(ref CurrentStatus);
        CurrentStatus.MediaPlayer_Active = MPStatus.MediaPlayer_Active;
        CurrentStatus.MediaPlayer_Playing = MPStatus.MediaPlayer_Playing;
        CurrentStatus.MediaPlayer_Paused = MPStatus.MediaPlayer_Paused;
        CurrentStatus.Media_IsRecording = MPStatus.Media_IsRecording;
        CurrentStatus.Media_IsTV = MPStatus.Media_IsTV;
        CurrentStatus.Media_IsTVRecording = MPStatus.Media_IsTVRecording;
        CurrentStatus.Media_IsDVD = MPStatus.Media_IsDVD;
        CurrentStatus.Media_IsCD = MPStatus.Media_IsCD;
        CurrentStatus.Media_IsRadio = MPStatus.Media_IsRadio;
        CurrentStatus.Media_IsVideo = MPStatus.Media_IsVideo;
        CurrentStatus.Media_IsMusic = MPStatus.Media_IsMusic;
        //
        // Race condition during stop of LiveTV or DVD playback      
        //
        try
        {
          CurrentStatus.Media_CurrentPosition = g_Player.CurrentPosition;
          CurrentStatus.Media_Duration = g_Player.Duration;
          CurrentStatus.Media_Speed = g_Player.Speed;
        }
        catch (Exception ex)
        {
          Log.Debug("GetSystemStatus(): unable to update g_player properties (playback stop in progress?): " +
                    ex.Message);
        }
      }
    }

    public static void GetSystemVolume(ref SystemStatus CurrentStatus)
    {
      CurrentStatus.SystemVolumeLevel = -1;
      try
      {
        if (!CurrentStatus.IsMuted)
        {
          try
          {
            CurrentStatus.SystemVolumeLevel = AudioMixerHelper.GetVolume();
          }
          catch
          {
          }
          if (CurrentStatus.SystemVolumeLevel < 0)
          {
            try
            {
              CurrentStatus.SystemVolumeLevel = VolumeHandler.Instance.Volume;
            }
            catch
            {
            }
          }
          if (CurrentStatus.SystemVolumeLevel >= 0)
          {
            return;
          }
          try
          {
            CurrentStatus.SystemVolumeLevel = g_Player.Volume;
            return;
          }
          catch
          {
            CurrentStatus.SystemVolumeLevel = 0;
            return;
          }
        }
        CurrentStatus.SystemVolumeLevel = 0;
      }
      catch
      {
        CurrentStatus.SystemVolumeLevel = 0;
        CurrentStatus.IsMuted = false;
      }
    }

    public static void InitDisplayControl(ref DisplayControl DisplaySettings)
    {
      DisplaySettings._Shutdown1 = string.Empty;
      DisplaySettings._Shutdown2 = string.Empty;
      DisplaySettings.BlankDisplayWithVideo = false;
      DisplaySettings.EnableDisplayAction = false;
      DisplaySettings.BlankDisplayWhenIdle = false;
      DisplaySettings._BlankIdleTime = 0L;
      DisplaySettings._BlankIdleTimeout = 0L;
      DisplaySettings._DisplayControlAction = false;
      DisplaySettings._DisplayControlLastAction = 0L;
      DisplaySettings.DisplayActionTime = 0;
      DisplaySettings._DisplayControlTimeout = 0L;
    }

    public static void InitDisplayOptions(ref DisplayOptions DisplayOptions)
    {
      DisplayOptions.DiskIcon = false;
      DisplayOptions.VolumeDisplay = false;
      DisplayOptions.ProgressDisplay = false;
      DisplayOptions.DiskMediaStatus = true;
      DisplayOptions.DiskMonitor = false;
      DisplayOptions.UseCustomFont = false;
      DisplayOptions.UseLargeIcons = false;
      DisplayOptions.UseCustomIcons = false;
      DisplayOptions.UseInvertedIcons = false;
    }

    public static void InitEQ(ref EQControl EQSettings)
    {
      EQSettings.UseEqDisplay = false;
      EQSettings.UseNormalEq = true;
      EQSettings.UseStereoEq = false;
      EQSettings.UseVUmeter = false;
      EQSettings.UseVUmeter2 = false;
      EQSettings._useVUindicators = false;
      EQSettings._useEqMode = 0;
      EQSettings.RestrictEQ = false;
      EQSettings.SmoothEQ = false;
      EQSettings.DelayEQ = true;
      EQSettings.EQTitleDisplay = false;
      EQSettings._EqDataAvailable = false;
      EQSettings.EqFftData = new float[0x800];
      EQSettings.EqArray = new byte[0x11];
      EQSettings.LastEQ = new int[0x11];
      EQSettings._EQTitleDisplayTime = 10;
      EQSettings._EQTitleShowTime = 2;
      EQSettings._LastEQTitle = 0.0;
      EQSettings._EQDisplayTitle = false;
      EQSettings._Max_EQ_FPS = 0;
      EQSettings._EQ_Framecount = 0;
      EQSettings._EQ_Restrict_FPS = 10;
      EQSettings._EqUpdateDelay = 0;
      EQSettings._DelayEQTime = 0;
      using (Profile.Settings settings = new Profile.MPSettings())
      {
        EQSettings._AudioIsMixing = settings.GetValueAsBool("audioplayer", "mixing", false);
        EQSettings._AudioUseASIO = settings.GetValueAsBool("audioplayer", "asio", false);
      }
    }

    public static void InitSystemStatus(ref SystemStatus CurrentStatus)
    {
      lock (StatusMutex)
      {
        using (Profile.Settings settings = new Profile.MPSettings())
        {
          CurrentStatus._AudioIsMixing = settings.GetValueAsBool("audioplayer", "mixing", false);
          CurrentStatus._AudioUseASIO = settings.GetValueAsBool("audioplayer", "asio", false);
          bool flag = settings.GetValueAsBool("volume", "digital", true);
          CurrentStatus._AudioUseMasterVolume = !flag;
          CurrentStatus._AudioUseWaveVolume = flag;
        }
        CurrentStatus.CurrentIconMask = 0L;
        CurrentStatus.MP_Is_Idle = false;
        CurrentStatus.SystemVolumeLevel = 0;
        CurrentStatus.IsMuted = false;
        CurrentStatus.MediaPlayer_Active = false;
        CurrentStatus.MediaPlayer_Playing = false;
        CurrentStatus.MediaPlayer_Paused = false;
        CurrentStatus.Media_IsRecording = false;
        CurrentStatus.Media_IsTV = false;
        CurrentStatus.Media_IsTVRecording = false;
        CurrentStatus.Media_IsDVD = false;
        CurrentStatus.Media_IsCD = false;
        CurrentStatus.Media_IsRadio = false;
        CurrentStatus.Media_IsVideo = false;
        CurrentStatus.Media_IsMusic = false;
        CurrentStatus.Media_IsTimeshifting = false;
        CurrentStatus.Media_CurrentPosition = 0.0;
        CurrentStatus.Media_Duration = 0.0;
        CurrentStatus.Media_Speed = 0;
      }
    }

    public static bool IsCaptureCardRecording()
    {
      if (UseTVServer)
      {
        return
          (bool)
          DynaInvoke.InvokeMethod(Config.GetFolder(Config.Dir.Base) + @"\TvControl.dll", "TvServer",
                                  "IsAnyCardRecording", null);
      }
      return false;
    }

    public static bool IsCaptureCardViewing()
    {
      if (UseTVServer)
      {
        return 
          (bool)
          DynaInvoke.InvokeMethod(Config.GetFolder(Config.Dir.Base) + @"\TvControl.dll", "TvServer",
                                  "IsAnyCardRecording", null);
      }
      return false;
    }

    public static bool Player_Playing()
    {
      return g_Player.Playing;
    }

    public static string PluginIconsToAudioFormat(ulong IconMask)
    {
      string str = string.Empty;
      if ((IconMask & ((ulong) 0x8000000000L)) > 0L)
      {
        str = str + " ICON_WMA2";
      }
      if ((IconMask & ((ulong) 0x4000000000L)) > 0L)
      {
        str = str + " ICON_WAV";
      }
      if ((IconMask & ((ulong) 0x4000000L)) > 0L)
      {
        str = str + " ICON_WMA";
      }
      if ((IconMask & ((ulong) 0x2000000L)) > 0L)
      {
        str = str + " ICON_MP3";
      }
      if ((IconMask & ((ulong) 0x1000000L)) > 0L)
      {
        str = str + " ICON_OGG";
      }
      return str.Trim();
    }

    public static string PluginIconsToString(ulong IconMask)
    {
      string str = string.Empty;
      if ((IconMask & ((ulong) 0x100000000000L)) > 0L)
      {
        str = str + " ICON_Play";
      }
      if ((IconMask & ((ulong) 0x80000000000L)) > 0L)
      {
        str = str + " ICON_Pause";
      }
      if ((IconMask & ((ulong) 0x40000000000L)) > 0L)
      {
        str = str + " ICON_Stop";
      }
      if ((IconMask & ((ulong) 0x20000000000L)) > 0L)
      {
        str = str + " ICON_FFWD";
      }
      if ((IconMask & ((ulong) 0x10000000000L)) > 0L)
      {
        str = str + " ICON_FRWD";
      }
      if ((IconMask & ((ulong) 0x400000000L)) > 0L)
      {
        str = str + " ICON_Rec";
      }
      if ((IconMask & ((ulong) 0x200000000L)) > 0L)
      {
        str = str + " ICON_Vol";
      }
      if ((IconMask & ((ulong) 0x100000000L)) > 0L)
      {
        str = str + " ICON_Time";
      }
      if ((IconMask & ((ulong) 0x80L)) > 0L)
      {
        str = str + " ICON_Music";
      }
      if ((IconMask & ((ulong) 0x40L)) > 0L)
      {
        str = str + " ICON_Movie";
      }
      if ((IconMask & ((ulong) 0x20L)) > 0L)
      {
        str = str + " ICON_Photo";
      }
      if ((IconMask & ((ulong) 8L)) > 0L)
      {
        str = str + " ICON_TV";
      }
      if ((IconMask & ((ulong) 0x10L)) > 0L)
      {
        str = str + " ICON_CD_DVD";
      }
      if ((IconMask & ((ulong) 0x200000L)) > 0L)
      {
        str = str + " ICON_TV_2";
      }
      if ((IconMask & ((ulong) 0x100000L)) > 0L)
      {
        str = str + " ICON_HDTV";
      }
      if ((IconMask & ((ulong) 0x8000000000L)) > 0L)
      {
        str = str + " ICON_WMA2";
      }
      if ((IconMask & ((ulong) 0x4000000000L)) > 0L)
      {
        str = str + " ICON_WAV";
      }
      if ((IconMask & ((ulong) 0x4000000L)) > 0L)
      {
        str = str + " ICON_WMA";
      }
      if ((IconMask & ((ulong) 0x2000000L)) > 0L)
      {
        str = str + " ICON_MP3";
      }
      if ((IconMask & ((ulong) 0x1000000L)) > 0L)
      {
        str = str + " ICON_OGG";
      }
      if ((IconMask & 0x80000000L) > 0L)
      {
        str = str + " ICON_xVid";
      }
      if ((IconMask & ((ulong) 0x40000000L)) > 0L)
      {
        str = str + " ICON_WMV";
      }
      if ((IconMask & ((ulong) 0x20000000L)) > 0L)
      {
        str = str + " ICON_MPG2";
      }
      if ((IconMask & ((ulong) 0x20000L)) > 0L)
      {
        str = str + " ICON_MPG";
      }
      if ((IconMask & ((ulong) 0x10000L)) > 0L)
      {
        str = str + " ICON_DivX";
      }
      if ((IconMask & ((ulong) 1L)) > 0L)
      {
        str = str + " SPKR_FL";
      }
      if ((IconMask & ((ulong) 0x8000L)) > 0L)
      {
        str = str + " SPKR_FC";
      }
      if ((IconMask & ((ulong) 0x4000L)) > 0L)
      {
        str = str + " SPKR_FR";
      }
      if ((IconMask & ((ulong) 0x400L)) > 0L)
      {
        str = str + " SPKR_RL";
      }
      if ((IconMask & ((ulong) 0x100L)) > 0L)
      {
        str = str + " SPKR_RR";
      }
      if ((IconMask & ((ulong) 0x2000L)) > 0L)
      {
        str = str + " SPKR_SL";
      }
      if ((IconMask & ((ulong) 0x800L)) > 0L)
      {
        str = str + " SPKR_SR";
      }
      if ((IconMask & ((ulong) 0x1000L)) > 0L)
      {
        str = str + " SPKR_LFE";
      }
      if ((IconMask & ((ulong) 0x200L)) > 0L)
      {
        str = str + " ICON_SPDIF";
      }
      if ((IconMask & ((ulong) 0x10000000L)) > 0L)
      {
        str = str + " ICON_AC3";
      }
      if ((IconMask & ((ulong) 0x8000000L)) > 0L)
      {
        str = str + " ICON_DTS";
      }
      if ((IconMask & ((ulong) 0x2000000000L)) > 0L)
      {
        str = str + " ICON_REF";
      }
      if ((IconMask & ((ulong) 0x1000000000L)) > 0L)
      {
        str = str + " ICON_SFL";
      }
      if ((IconMask & ((ulong) 0x800000000L)) > 0L)
      {
        str = str + " ICON_Alarm";
      }
      if ((IconMask & ((ulong) 0x800000L)) > 0L)
      {
        str = str + " ICON_SRC";
      }
      if ((IconMask & ((ulong) 0x400000L)) > 0L)
      {
        str = str + " ICON_FIT";
      }
      if ((IconMask & ((ulong) 0x80000L)) > 0L)
      {
        str = str + " ICON_SCR1";
      }
      if ((IconMask & ((ulong) 0x40000L)) > 0L)
      {
        str = str + " ICON_SCR2";
      }
      if ((IconMask & ((ulong) 4L)) > 0L)
      {
        str = str + " ICON_WebCast";
      }
      if ((IconMask & ((ulong) 2L)) > 0L)
      {
        str = str + " ICON_News";
      }
      return str.Trim();
    }

    public static string PluginIconsToVideoFormat(ulong IconMask)
    {
      string str = string.Empty;
      if ((IconMask & 0x80000000L) > 0L)
      {
        str = str + " ICON_xVid";
      }
      if ((IconMask & ((ulong) 0x40000000L)) > 0L)
      {
        str = str + " ICON_WMV";
      }
      if ((IconMask & ((ulong) 0x20000000L)) > 0L)
      {
        str = str + " ICON_MPG2";
      }
      if ((IconMask & ((ulong) 0x20000L)) > 0L)
      {
        str = str + " ICON_MPG";
      }
      if ((IconMask & ((ulong) 0x10000L)) > 0L)
      {
        str = str + " ICON_DivX";
      }
      return str.Trim();
    }

    public static void ProcessEqData(ref EQControl EQSettings)
    {
      bool extensiveLogging = Settings.Instance.ExtensiveLogging;
      if (extensiveLogging)
      {
        Log.Info("MiniDisplay.ProcessEqData(): called... MaxValue = {0}, BANDS = {1}",
                 new object[] {EQSettings.Render_MaxValue, EQSettings.Render_BANDS});
      }
      if ((EQSettings.UseStereoEq || EQSettings.UseVUmeter) || EQSettings.UseVUmeter2)
      {
        if (EQSettings.UseStereoEq)
        {
          int num = EQSettings.Render_MaxValue;
          int num2 = EQSettings.Render_BANDS;
          int num3 = 0;
          for (int i = 0; i < num2; i++)
          {
            float num7 = 0f;
            float num8 = 0f;
            int num10 = (int) Math.Pow(2.0, (i*10.0)/((double) (num2 - 1)));
            if (num10 > 0x3ff)
            {
              num10 = 0x3ff;
            }
            if (num10 <= num3)
            {
              num10 = num3 + 1;
            }
            int num9 = (10 + num10) - num3;
            while (num3 < num10)
            {
              num7 += EQSettings.EqFftData[2 + (num3*2)];
              num8 += EQSettings.EqFftData[(2 + (num3*2)) + 1];
              num3++;
            }
            int num4 = (int) ((Math.Sqrt(((double) num7)/Math.Log10((double) num9))*1.7)*num);
            int num5 = (int) ((Math.Sqrt(((double) num8)/Math.Log10((double) num9))*1.7)*num);
            if (extensiveLogging)
            {
              Log.Info("MiniDisplay.ProcessEqData(): Processing StereoEQ band {0}: L = {1}, R = {2}",
                       new object[] {i, num4, num5});
            }
            num4 = Math.Min(num, num4);
            EQSettings.EqArray[1 + i] = (byte) num4;
            num5 = Math.Min(num, num5);
            EQSettings.EqArray[9 + i] = (byte) num5;
            if (EQSettings.SmoothEQ)
            {
              if (EQSettings.EqArray[1 + i] < EQSettings.LastEQ[1 + i])
              {
                int num11 = EQSettings.LastEQ[1 + i];
                num11 = EQSettings.LastEQ[1 + i] - ((int) 0.5);
                if (num11 < 0)
                {
                  num11 = 0;
                }
                EQSettings.EqArray[1 + i] = (byte) num11;
                EQSettings.LastEQ[1 + i] = num11;
              }
              else
              {
                EQSettings.LastEQ[1 + i] = EQSettings.EqArray[1 + i];
              }
              if (EQSettings.EqArray[9 + i] < EQSettings.LastEQ[9 + i])
              {
                int num12 = EQSettings.LastEQ[9 + i];
                num12 = EQSettings.LastEQ[9 + i] - ((int) 0.5);
                if (num12 < 0)
                {
                  num12 = 0;
                }
                EQSettings.EqArray[9 + i] = (byte) num12;
                EQSettings.LastEQ[9 + i] = num12;
              }
              else
              {
                EQSettings.LastEQ[9 + i] = EQSettings.EqArray[9 + i];
              }
            }
            if (extensiveLogging)
            {
              Log.Info("MiniDisplay.ProcessEqData.(): Processed StereoEQ mode {0} byte {1}: L = {2}, R = {3}.",
                       new object[]
                         {
                           EQSettings.EqArray[0], i, EQSettings.EqArray[1 + (i*2)].ToString(),
                           EQSettings.EqArray[2 + (i*2)].ToString()
                         });
            }
          }
        }
        else
        {
          int num13 = EQSettings.Render_MaxValue;
          int num14 = EQSettings.Render_BANDS;
          int num15 = 0;
          for (int j = 0; j < num14; j++)
          {
            float num19 = 0f;
            float num20 = 0f;
            int num22 = 0x3ff;
            int num21 = (10 + num22) - num15;
            while (num15 < num22)
            {
              if (EQSettings.EqFftData[2 + (num15*2)] > num19)
              {
                num19 = EQSettings.EqFftData[2 + (num15*2)];
              }
              if (EQSettings.EqFftData[(2 + (num15*2)) + 1] > num20)
              {
                num20 = EQSettings.EqFftData[(2 + (num15*2)) + 1];
              }
              num15++;
            }
            int num16 = (int) ((Math.Sqrt(((double) num19)/Math.Log10((double) num21))*1.7)*num13);
            int num17 = (int) ((Math.Sqrt(((double) num20)/Math.Log10((double) num21))*1.7)*num13);
            if (extensiveLogging)
            {
              Log.Info("MiniDisplay.ProcessEqData(): Processing VUmeter band {0}: L = {1}, R = {2}",
                       new object[] {j, num16, num17});
            }
            num16 = Math.Min(num13, num16);
            EQSettings.EqArray[1 + (j*2)] = (byte) num16;
            num17 = Math.Min(num13, num17);
            EQSettings.EqArray[2 + (j*2)] = (byte) num17;
            if (EQSettings.SmoothEQ)
            {
              if (EQSettings.EqArray[1] < EQSettings.LastEQ[1])
              {
                int num23 = EQSettings.LastEQ[1];
                num23 = EQSettings.LastEQ[1] - ((int) 0.5);
                if (num23 < 0)
                {
                  num23 = 0;
                }
                EQSettings.EqArray[1] = (byte) num23;
                EQSettings.LastEQ[1] = num23;
              }
              else
              {
                EQSettings.LastEQ[1] = EQSettings.EqArray[1];
              }
              if (EQSettings.EqArray[2] < EQSettings.LastEQ[2])
              {
                int num24 = EQSettings.LastEQ[2];
                num24 = EQSettings.LastEQ[2] - ((int) 0.5);
                if (num24 < 0)
                {
                  num24 = 0;
                }
                EQSettings.EqArray[2] = (byte) num24;
                EQSettings.LastEQ[2] = num24;
              }
              else
              {
                EQSettings.LastEQ[2] = EQSettings.EqArray[2];
              }
            }
            if (extensiveLogging)
            {
              Log.Info("MiniDisplay.ProcessEqData(): Processed VUmeter byte {0}: L = {1}, R = {2}.",
                       new object[]
                         {j, EQSettings.EqArray[1 + (j*2)].ToString(), EQSettings.EqArray[2 + (j*2)].ToString()});
            }
          }
        }
      }
      else
      {
        int num25 = EQSettings.Render_MaxValue;
        int num26 = EQSettings.Render_BANDS;
        int num27 = 0;
        for (int k = 0; k < num26; k++)
        {
          float num30 = 0f;
          int num32 = (int) Math.Pow(2.0, (k*10.0)/((double) (num26 - 1)));
          if (num32 > 0x3ff)
          {
            num32 = 0x3ff;
          }
          if (num32 <= num27)
          {
            num32 = num27 + 1;
          }
          int num31 = (10 + num32) - num27;
          while (num27 < num32)
          {
            num30 += EQSettings.EqFftData[1 + num27];
            num27++;
          }
          int num28 = (int) ((Math.Sqrt(((double) num30)/Math.Log10((double) num31))*1.7)*num25);
          if (extensiveLogging)
          {
            Log.Info("MiniDisplay.ProcessEqData(): Processing EQ band {0} = {1}", new object[] {k, num28});
          }
          num28 = Math.Min(num25, num28);
          EQSettings.EqArray[1 + k] = (byte) num28;
          if (EQSettings.SmoothEQ)
          {
            if (EQSettings.EqArray[1 + k] < EQSettings.LastEQ[1 + k])
            {
              int num33 = EQSettings.LastEQ[1 + k];
              num33 = EQSettings.LastEQ[1 + k] - ((int) 0.5);
              if (num33 < 0)
              {
                num33 = 0;
              }
              EQSettings.EqArray[1 + k] = (byte) num33;
              EQSettings.LastEQ[1 + k] = num33;
            }
            else
            {
              EQSettings.LastEQ[1 + k] = EQSettings.EqArray[1 + k];
            }
          }
          if (extensiveLogging)
          {
            Log.Info("MiniDisplay.ProcessEqData(): Processed EQ mode {0} byte {1} = {2}.",
                     new object[] {EQSettings.EqArray[0], k, EQSettings.EqArray[1 + k].ToString()});
          }
        }
      }
      if (extensiveLogging)
      {
        Log.Info("MiniDisplay.ProcessEqData(): called");
      }
    }

    public static void SetIdleTimeout(int TimeOutSeconds)
    {
      if (TimeOutSeconds == -1)
      {
        _IdleTimeout = 5;
      }
      else
      {
        _IdleTimeout = TimeOutSeconds;
      }
    }

    public static int GetIdleTimeout()
    {
      return _IdleTimeout;
    }

    public static ulong SetPluginIcons()
    {
      string[] strArray;
      string str3;
      ulong num = 0L;
      string property = string.Empty;
      MPStatus.MediaPlayer_Active = false;
      MPStatus.MediaPlayer_Playing = false;
      MPStatus.MediaPlayer_Paused = false;
      MPStatus.Media_IsCD = false;
      MPStatus.Media_IsRadio = false;
      MPStatus.Media_IsDVD = false;
      MPStatus.Media_IsMusic = false;
      MPStatus.Media_IsRecording = false;
      MPStatus.Media_IsTV = false;
      MPStatus.Media_IsTVRecording = false;
      MPStatus.Media_IsVideo = false;
      if (IsCaptureCardRecording())
      {
        num |= (ulong) 0x400000000L;
        MPStatus.Media_IsRecording = true;
      }
      else if (IsCaptureCardViewing())
      {
        num |= (ulong) 8L;
        MPStatus.Media_IsTV = true;
      }
      if (g_Player.Player == null)
      {
        return (num | ((ulong) 0x40000000000L));
      }
      MPStatus.MediaPlayer_Active = true;
      if (!g_Player.Playing)
      {
        num |= (ulong) 0x40000000000L;
        MPStatus.MediaPlayer_Active = false;
        MPStatus.MediaPlayer_Paused = false;
        MPStatus.MediaPlayer_Playing = false;
        return num;
      }
      if (g_Player.Playing & !g_Player.Paused)
      {
        MPStatus.MediaPlayer_Playing = true;
        if (g_Player.Speed > 1)
        {
          num |= (ulong) 0x20000000000L;
        }
        else if (g_Player.Speed < 0)
        {
          num |= (ulong) 0x10000000000L;
        }
        else
        {
          num |= (ulong) 0x100000000000L;
        }
      }
      else
      {
        MPStatus.MediaPlayer_Paused = true;
        num |= (ulong) 0x80000000000L;
      }
      if (g_Player.IsMusic)
      {
        MPStatus.Media_IsMusic = true;
        num |= (ulong) 0x80L;
        property = GUIPropertyManager.GetProperty("#Play.Current.File");
        if (property.Length > 0)
        {
          string str2;
          strArray = property.Split(new char[] {'.'});
          if ((strArray.Length > 1) && ((str2 = strArray[1]) != null))
          {
            if (!(str2 == "mp3"))
            {
              if (str2 == "ogg")
              {
                num |= (ulong) 0x1000000L;
              }
              else if (str2 == "wma")
              {
                num |= (ulong) 0x4000000L;
              }
              else if (str2 == "wav")
              {
                num |= (ulong) 0x4000000000L;
              }
            }
            else
            {
              num |= (ulong) 0x2000000L;
            }
          }
        }
      }
      if ((g_Player.IsTV || g_Player.IsTVRecording) & (!g_Player.IsDVD && !g_Player.IsCDA))
      {
        if (g_Player.IsTV)
        {
          MPStatus.Media_IsTV = true;
        }
        else
        {
          MPStatus.Media_IsTVRecording = true;
        }
        num |= (ulong) 8L;
      }
      if (g_Player.IsDVD || g_Player.IsCDA)
      {
        if (g_Player.IsDVD & g_Player.IsVideo)
        {
          MPStatus.Media_IsDVD = true;
          MPStatus.Media_IsVideo = true;
          num |= (ulong) 0x40L;
        }
        else if (g_Player.IsCDA & !g_Player.IsVideo)
        {
          MPStatus.Media_IsCD = true;
          MPStatus.Media_IsMusic = true;
          num |= (ulong) 0x80L;
        }
        num |= (ulong) 0x10L;
      }
      if (!(g_Player.IsVideo & !g_Player.IsDVD))
      {
        return num;
      }
      MPStatus.Media_IsVideo = true;
      num |= (ulong) 0x40L;
      property = GUIPropertyManager.GetProperty("#Play.Current.File");
      if (property.Length <= 0)
      {
        return num;
      }
      num |= (ulong) 0x80L;
      strArray = property.Split(new char[] {'.'});
      if ((strArray.Length <= 1) || ((str3 = strArray[1].ToLower()) == null))
      {
        return num;
      }
      if ((!(str3 == "ifo") && !(str3 == "vob")) && !(str3 == "mpg"))
      {
        if (str3 != "wmv")
        {
          if (str3 == "divx")
          {
            return (num | ((ulong) 0x10000L));
          }
          if (str3 != "xvid")
          {
            return num;
          }
          return (num | 0x80000000L);
        }
      }
      else
      {
        return (num | ((ulong) 0x20000L));
      }
      return (num | ((ulong) 0x40000000L));
    }

    public static void ShowSystemStatus(ref SystemStatus CurrentStatus)
    {
    }

    public static bool xPlayer_IsActive()
    {
      return (g_Player.Player != null);
    }

    public static bool xPlayer_IsCDA()
    {
      return g_Player.IsCDA;
    }

    public static bool xPlayer_IsDVD()
    {
      return g_Player.IsDVD;
    }

    public static bool xPlayer_IsMusic()
    {
      return g_Player.IsMusic;
    }

    public static bool xPlayer_IsRadio()
    {
      return g_Player.IsRadio;
    }

    public static bool xPlayer_IsTimeshifting()
    {
      return g_Player.IsTimeShifting;
    }

    public static bool xPlayer_IsTV()
    {
      return g_Player.IsTV;
    }

    public static bool xPlayer_IsTVRecording()
    {
      return g_Player.IsTV;
    }

    public static bool xPlayer_IsVideo()
    {
      return g_Player.IsVideo;
    }

    public static bool xPlayer_Paused()
    {
      return g_Player.Paused;
    }

    public static int xPlayer_Speed()
    {
      return g_Player.Speed;
    }

    public static string Plugin_Version
    {
      get { return "MiniDisplay Plugin v06_03_2009"; }
    }

    public static bool IsSetupAvailable()
    {
      string str = Settings.Instance.Type;
      if (!File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay.xml")))
      {
        return false;
      }
      if (str == null ||
          (str != "DebugForm" && str != "iMONLCDg" && str != "MatrixMX" && str != "MatrixGX" && str != "VLSYS_Mplay"))
      {
        return false;
      }
      if (Settings.Instance.DisableGUISetup)
      {
        return false;
      }
      bool enabled = false;
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        enabled = xmlreader.GetValueAsBool("plugins", "MiniDisplay", false);
      }
      return enabled;
    }
  }
}
