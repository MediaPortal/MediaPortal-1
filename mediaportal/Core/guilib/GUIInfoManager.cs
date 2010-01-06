#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using MediaPortal.Configuration;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace MediaPortal.GUI.Library
{
  public class GUIInfoManager
  {
    #region classes

    private class GUIInfo
    {
      public int m_info;
      public int m_data1;
      public int m_data2;
      public string m_stringData;

      public GUIInfo(int info)
      {
        m_info = info;
        m_data1 = 0;
        m_data2 = 0;
        m_stringData = string.Empty;
      }

      public GUIInfo(int info, int data1)
      {
        m_info = info;
        m_data1 = data1;
        m_data2 = 0;
        m_stringData = string.Empty;
      }

      public GUIInfo(int info, string stringData)
      {
        m_info = info;
        m_data1 = 0;
        m_data2 = 0;
        m_stringData = stringData;
      }

      public GUIInfo(int info, int data1, int data2)
      {
        m_info = info;
        m_data1 = data1;
        m_data2 = data2;
        m_stringData = string.Empty;
      }

      public override bool Equals(object r)
      {
        GUIInfo right = (GUIInfo)r;
        return (m_info == right.m_info && m_data1 == right.m_data1 && m_data2 == right.m_data2 &&
                m_stringData == right.m_stringData);
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }
    }

    private class CCombinedValue
    {
      public string m_info; // the text expression
      public int m_id; // the id used to identify this expression
      public List<int> m_postfix = new List<int>(); // the postfix binary expression

      public CCombinedValue() {}

      public CCombinedValue(CCombinedValue mSrc)
      {
        this.m_info = mSrc.m_info;
        this.m_id = mSrc.m_id;
        this.m_postfix = mSrc.m_postfix;
      }
    }

    #endregion

    #region consts

    public const int OPERATOR_NOT = 3;
    public const int OPERATOR_AND = 2;
    public const int OPERATOR_OR = 1;

    public const int PLAYER_HAS_MEDIA = 1;
    public const int PLAYER_HAS_AUDIO = 2;
    public const int PLAYER_HAS_VIDEO = 3;
    public const int PLAYER_PLAYING = 4;
    public const int PLAYER_PAUSED = 5;
    public const int PLAYER_REWINDING = 6;
    public const int PLAYER_REWINDING_2x = 7;
    public const int PLAYER_REWINDING_4x = 8;
    public const int PLAYER_REWINDING_8x = 9;
    public const int PLAYER_REWINDING_16x = 10;
    public const int PLAYER_REWINDING_32x = 11;
    public const int PLAYER_FORWARDING = 12;
    public const int PLAYER_FORWARDING_2x = 13;
    public const int PLAYER_FORWARDING_4x = 14;
    public const int PLAYER_FORWARDING_8x = 15;
    public const int PLAYER_FORWARDING_16x = 16;
    public const int PLAYER_FORWARDING_32x = 17;
    public const int PLAYER_CAN_RECORD = 18;
    public const int PLAYER_RECORDING = 19;
    public const int PLAYER_CACHING = 20;
    public const int PLAYER_DISPLAY_AFTER_SEEK = 21;
    public const int PLAYER_PROGRESS = 22;
    public const int PLAYER_SEEKBAR = 23;
    public const int PLAYER_SEEKTIME = 24;
    public const int PLAYER_SEEKING = 25;
    public const int PLAYER_SHOWTIME = 26;
    public const int PLAYER_TIME = 27;
    public const int PLAYER_TIME_REMAINING = 28;
    public const int PLAYER_DURATION = 29;
    public const int PLAYER_SHOWCODEC = 30;
    public const int PLAYER_SHOWINFO = 31;
    public const int PLAYER_VOLUME = 32;
    public const int PLAYER_MUTED = 33;
    public const int PLAYER_HASDURATION = 34;

    public const int WEATHER_CONDITIONS = 100;
    public const int WEATHER_TEMPERATURE = 101;
    public const int WEATHER_LOCATION = 102;
    public const int WEATHER_IS_FETCHED = 103;

    public const int SYSTEM_TIME = 110;
    public const int SYSTEM_DATE = 111;
    public const int SYSTEM_CPU_TEMPERATURE = 112;
    public const int SYSTEM_GPU_TEMPERATURE = 113;
    public const int SYSTEM_FAN_SPEED = 114;
    public const int SYSTEM_FREE_SPACE_C = 115;
    /* 
    public const int  SYSTEM_FREE_SPACE_D         =116 //116 is reserved for space on D
    */
    public const int SYSTEM_FREE_SPACE_E = 117;
    public const int SYSTEM_FREE_SPACE_F = 118;
    public const int SYSTEM_FREE_SPACE_G = 119;
    public const int SYSTEM_BUILD_VERSION = 120;
    public const int SYSTEM_BUILD_DATE = 121;
    public const int SYSTEM_ETHERNET_LINK_ACTIVE = 122;
    public const int SYSTEM_FPS = 123;
    public const int SYSTEM_KAI_CONNECTED = 124;

    public const int SYSTEM_ALWAYS_TRUE = 125;
    // useful for <visible fade="10" start="hidden">true</visible>, to fade in a control

    public const int SYSTEM_ALWAYS_FALSE = 126;
    // used for <visible fade="10">false</visible>, to fade out a control (ie not particularly useful!)

    public const int SYSTEM_MEDIA_DVD = 127;
    public const int SYSTEM_DVDREADY = 128;
    public const int SYSTEM_HAS_ALARM = 129;
    public const int SYSTEM_AUTODETECTION = 130;
    public const int SYSTEM_FREE_MEMORY = 131;
    public const int SYSTEM_SCREEN_MODE = 132;
    public const int SYSTEM_SCREEN_WIDTH = 133;
    public const int SYSTEM_SCREEN_HEIGHT = 134;
    public const int SYSTEM_CURRENT_WINDOW = 135;
    public const int SYSTEM_CURRENT_CONTROL = 136;
    public const int SYSTEM_XBOX_NICKNAME = 137;
    public const int SYSTEM_DVD_LABEL = 138;
    public const int SYSTEM_HASLOCKS = 140;
    public const int SYSTEM_ISMASTER = 141;
    public const int SYSTEM_TRAYOPEN = 142;
    public const int SYSTEM_KAI_ENABLED = 143;
    public const int SYSTEM_ALARM_POS = 144;
    public const int SYSTEM_LOGGEDON = 145;
    public const int SYSTEM_PROFILENAME = 146;
    public const int SYSTEM_PROFILETHUMB = 147;
    public const int SYSTEM_HAS_LOGINSCREEN = 148;

    // reserved for systeminfo stuff
    public const int SYSTEM_HDD_SMART = 150;
    public const int SYSTEM_INTERNET_STATE = 159;
    //

    public const int LCD_PLAY_ICON = 160;
    public const int LCD_PROGRESS_BAR = 161;
    public const int LCD_CPU_TEMPERATURE = 162;
    public const int LCD_GPU_TEMPERATURE = 163;
    public const int LCD_FAN_SPEED = 164;
    public const int LCD_DATE = 166;
    public const int LCD_FREE_SPACE_C = 167;
    /*
    public const int  LCD_FREE_SPACE_D            =168; // 168 is reserved for space on D
    */
    public const int LCD_FREE_SPACE_E = 169;
    public const int LCD_FREE_SPACE_F = 170;
    public const int LCD_FREE_SPACE_G = 171;

    public const int NETWORK_IP_ADDRESS = 190;

    public const int MUSICPLAYER_TITLE = 200;
    public const int MUSICPLAYER_ALBUM = 201;
    public const int MUSICPLAYER_ARTIST = 202;
    public const int MUSICPLAYER_GENRE = 203;
    public const int MUSICPLAYER_YEAR = 204;
    public const int MUSICPLAYER_TIME = 205;
    public const int MUSICPLAYER_TIME_REMAINING = 206;
    public const int MUSICPLAYER_TIME_SPEED = 207;
    public const int MUSICPLAYER_TRACK_NUMBER = 208;
    public const int MUSICPLAYER_DURATION = 209;
    public const int MUSICPLAYER_COVER = 210;
    public const int MUSICPLAYER_BITRATE = 211;
    public const int MUSICPLAYER_PLAYLISTLEN = 212;
    public const int MUSICPLAYER_PLAYLISTPOS = 213;
    public const int MUSICPLAYER_CHANNELS = 214;
    public const int MUSICPLAYER_BITSPERSAMPLE = 215;
    public const int MUSICPLAYER_SAMPLERATE = 216;
    public const int MUSICPLAYER_CODEC = 217;
    public const int MUSICPLAYER_DISC_NUMBER = 218;

    public const int VIDEOPLAYER_TITLE = 250;
    public const int VIDEOPLAYER_GENRE = 251;
    public const int VIDEOPLAYER_DIRECTOR = 252;
    public const int VIDEOPLAYER_YEAR = 253;
    public const int VIDEOPLAYER_TIME = 254;
    public const int VIDEOPLAYER_TIME_REMAINING = 255;
    public const int VIDEOPLAYER_TIME_SPEED = 256;
    public const int VIDEOPLAYER_DURATION = 257;
    public const int VIDEOPLAYER_COVER = 258;
    public const int VIDEOPLAYER_USING_OVERLAYS = 259;
    public const int VIDEOPLAYER_ISFULLSCREEN = 260;
    public const int VIDEOPLAYER_HASMENU = 261;
    public const int VIDEOPLAYER_PLAYLISTLEN = 262;
    public const int VIDEOPLAYER_PLAYLISTPOS = 263;

    public const int AUDIOSCROBBLER_ENABLED = 300;
    public const int AUDIOSCROBBLER_CONN_STATE = 301;
    public const int AUDIOSCROBBLER_SUBMIT_INT = 302;
    public const int AUDIOSCROBBLER_FILES_CACHED = 303;
    public const int AUDIOSCROBBLER_SUBMIT_STATE = 304;

    public const int LISTITEM_START = 310;
    public const int LISTITEM_THUMB = 310;
    public const int LISTITEM_LABEL = 311;
    public const int LISTITEM_TITLE = 312;
    public const int LISTITEM_TRACKNUMBER = 313;
    public const int LISTITEM_ARTIST = 314;
    public const int LISTITEM_ALBUM = 315;
    public const int LISTITEM_YEAR = 316;
    public const int LISTITEM_GENRE = 317;
    public const int LISTITEM_ICON = 318;
    public const int LISTITEM_DIRECTOR = 319;
    public const int LISTITEM_OVERLAY = 320;
    public const int LISTITEM_LABEL2 = 321;
    public const int LISTITEM_FILENAME = 322;
    public const int LISTITEM_DATE = 323;
    public const int LISTITEM_SIZE = 324;
    public const int LISTITEM_RATING = 325;
    public const int LISTITEM_PROGRAM_COUNT = 326;
    public const int LISTITEM_DURATION = 327;
    public const int LISTITEM_ISPLAYING = 328;
    public const int LISTITEM_ISSELECTED = 329;
    public const int LISTITEM_PLOT = 330;
    public const int LISTITEM_PLOT_OUTLINE = 331;
    public const int LISTITEM_EPISODE = 332;
    public const int LISTITEM_SEASON = 333;
    public const int LISTITEM_TVSHOW = 334;
    public const int LISTITEM_PREMIERED = 335;
    public const int LISTITEM_COMMENT = 336;
    public const int LISTITEM_ACTUAL_ICON = 337;
    public const int LISTITEM_PATH = 338;
    public const int LISTITEM_PICTURE_PATH = 339;
    public const int LISTITEM_PICTURE_DATETIME = 340;
    public const int LISTITEM_PICTURE_RESOLUTION = 341;
    public const int LISTITEM_END = 350;


    public const int MUSICPM_ENABLED = 350;
    public const int MUSICPM_SONGSPLAYED = 351;
    public const int MUSICPM_MATCHINGSONGS = 352;
    public const int MUSICPM_MATCHINGSONGSPICKED = 353;
    public const int MUSICPM_MATCHINGSONGSLEFT = 354;
    public const int MUSICPM_RELAXEDSONGSPICKED = 355;
    public const int MUSICPM_RANDOMSONGSPICKED = 356;

    public const int CONTAINER_FOLDERTHUMB = 360;
    public const int CONTAINER_FOLDERPATH = 361;
    public const int CONTAINER_CONTENT = 362;
    public const int CONTAINER_HAS_THUMB = 363;
    public const int CONTAINER_SORT_METHOD = 364;
    public const int CONTAINER_ON_NEXT = 365;
    public const int CONTAINER_ON_PREVIOUS = 366;
    public const int CONTAINER_HAS_FOCUS = 367;


    public const int PLAYLIST_LENGTH = 390;
    public const int PLAYLIST_POSITION = 391;
    public const int PLAYLIST_RANDOM = 392;
    public const int PLAYLIST_REPEAT = 393;
    public const int PLAYLIST_ISRANDOM = 394;
    public const int PLAYLIST_ISREPEAT = 395;
    public const int PLAYLIST_ISREPEATONE = 396;

    public const int VISUALISATION_LOCKED = 400;
    public const int VISUALISATION_PRESET = 401;
    public const int VISUALISATION_NAME = 402;
    public const int VISUALISATION_ENABLED = 403;

    public const int SKIN_HAS_THEME_START = 500;
    public const int SKIN_HAS_THEME_END = 599; // allow for max 100 themes

    public const int SKIN_BOOL = 600;
    public const int STRING_EQUALS = 601;
    public const int STRING_STARTS = 602;
    public const int STRING_CONTAINS = 603;

    public const int XLINK_KAI_USERNAME = 701;
    public const int SKIN_THEME = 702;

    public const int FACADEVIEW_ALBUM = 800;
    public const int FACADEVIEW_FILMSTRIP = 801;
    public const int FACADEVIEW_LARGEICONS = 802;
    public const int FACADEVIEW_LIST = 803;
    public const int FACADEVIEW_PLAYLIST = 804;
    public const int FACADEVIEW_SMALLICONS = 805;

    public const int WINDOW_IS_TOPMOST = 9994;
    public const int WINDOW_IS_VISIBLE = 9995;
    public const int WINDOW_NEXT = 9996;
    public const int WINDOW_PREVIOUS = 9997;
    public const int WINDOW_IS_MEDIA = 9998;
    public const int WINDOW_IS_OSD_VISIBLE = 9999;
    //public const int WINDOW_ACTIVE_START = WINDOW_HOME;
    //public const int WINDOW_ACTIVE_END = WINDOW_PYTHON_END;

    public const int SYSTEM_IDLE_TIME_START = 20000;
    public const int SYSTEM_IDLE_TIME_FINISH = 21000; // 1000 seconds

    public const int PLUGIN_IS_ENABLED = 25000;

    public const int CONTROL_HAS_TEXT = 29996;
    public const int CONTROL_HAS_THUMB = 29997;
    public const int CONTROL_IS_VISIBLE = 29998;
    public const int CONTROL_GROUP_HAS_FOCUS = 29999;
    public const int CONTROL_HAS_FOCUS = 30000;
    public const int BUTTON_SCROLLER_HAS_ICON = 30001;

    // static string VERSION_STRING = "2.0.0";

    // the multiple information vector
    private static int MULTI_INFO_START = 40000;
    private static int MULTI_INFO_END = 50000; // 10000 references is all we have for now
    private static int COMBINED_VALUES_START = 100000;

    #endregion

    #region variables

    private static List<CCombinedValue> m_CombinedValues = new List<CCombinedValue>();
    private static Dictionary<int, bool> m_boolCache = new Dictionary<int, bool>();
    private static List<string> m_stringParameters = new List<string>();

    // Array of multiple information mapped to a single integer lookup
    private static List<GUIInfo> m_multiInfo = new List<GUIInfo>();

    // Current playing stuff
    private static int i_SmartRequest;

    private static int m_nextWindowID;
    private static int m_prevWindowID;

    #endregion

    #region ctor

    static GUIInfoManager()
    {
      m_nextWindowID = (int)GUIWindow.Window.WINDOW_INVALID;
      m_prevWindowID = (int)GUIWindow.Window.WINDOW_INVALID;
      m_stringParameters.Add("__ZZZZ__");
      // to offset the string parameters by 1 to assure that all entries are non-zero
    }

    #endregion

    /// \brief Translates a string as given by the skin into an int that we use for more
    /// efficient retrieval of data. Can handle combined strings on the form
    /// Player.Caching + VideoPlayer.IsFullscreen (Logical and)
    /// Player.HasVideo | Player.HasAudio (Logical or)
    public static int TranslateString(string strCondition)
    {
      if (strCondition.IndexOf("|") > 0 ||
          strCondition.IndexOf("+") > 0 ||
          strCondition.IndexOf("[") > 0 ||
          strCondition.IndexOf("]") > 0)
      {
        // Have a boolean expression
        // Check if this was added before

        for (int it = 0; it < m_CombinedValues.Count; it++)
        {
          if (String.Compare(strCondition, m_CombinedValues[it].m_info, true) == 0)
          {
            return m_CombinedValues[it].m_id;
          }
        }
        return TranslateBooleanExpression(strCondition);
      }
      //Just single command.
      return TranslateSingleString(strCondition);
    }


    public static int TranslateSingleString(string strCondition)
    {
      if (strCondition.Length == 0)
      {
        return 0;
      }
      string strTest = strCondition;
      strTest = strTest.ToLower();
      strTest = strTest.TrimStart(new char[] {' '});
      strTest = strTest.TrimEnd(new char[] {' '});
      if (strTest.Length == 0)
      {
        return 0;
      }

      bool bNegate = strTest[0] == '!';
      int ret = 0;
      string strCategory = "";

      if (bNegate)
      {
        strTest = strTest.Remove(0, 1);
      }

      // translate conditions...
      if (strTest == "false" || strTest == "no" || strTest == "off" || strTest == "disabled")
      {
        ret = SYSTEM_ALWAYS_FALSE;
      }
      else if (strTest == "true" || strTest == "yes" || strTest == "on" || strTest == "enabled")
      {
        ret = SYSTEM_ALWAYS_TRUE;
      }
      else
      {
        strCategory = strTest.Substring(0, strTest.IndexOf("."));
      }

      if (strCategory == "player")
      {
        if (strTest == "player.hasmedia")
        {
          ret = PLAYER_HAS_MEDIA;
        }
        else if (strTest == "player.hasaudio")
        {
          ret = PLAYER_HAS_AUDIO;
        }
        else if (strTest == "player.hasvideo")
        {
          ret = PLAYER_HAS_VIDEO;
        }
        else if (strTest == "player.playing")
        {
          ret = PLAYER_PLAYING;
        }
        else if (strTest == "player.paused")
        {
          ret = PLAYER_PAUSED;
        }
        else if (strTest == "player.rewinding")
        {
          ret = PLAYER_REWINDING;
        }
        else if (strTest == "player.forwarding")
        {
          ret = PLAYER_FORWARDING;
        }
        else if (strTest == "player.rewinding2x")
        {
          ret = PLAYER_REWINDING_2x;
        }
        else if (strTest == "player.rewinding4x")
        {
          ret = PLAYER_REWINDING_4x;
        }
        else if (strTest == "player.rewinding8x")
        {
          ret = PLAYER_REWINDING_8x;
        }
        else if (strTest == "player.rewinding16x")
        {
          ret = PLAYER_REWINDING_16x;
        }
        else if (strTest == "player.rewinding32x")
        {
          ret = PLAYER_REWINDING_32x;
        }
        else if (strTest == "player.forwarding2x")
        {
          ret = PLAYER_FORWARDING_2x;
        }
        else if (strTest == "player.forwarding4x")
        {
          ret = PLAYER_FORWARDING_4x;
        }
        else if (strTest == "player.forwarding8x")
        {
          ret = PLAYER_FORWARDING_8x;
        }
        else if (strTest == "player.forwarding16x")
        {
          ret = PLAYER_FORWARDING_16x;
        }
        else if (strTest == "player.forwarding32x")
        {
          ret = PLAYER_FORWARDING_32x;
        }
        else if (strTest == "player.canrecord")
        {
          ret = PLAYER_CAN_RECORD;
        }
        else if (strTest == "player.recording")
        {
          ret = PLAYER_RECORDING;
        }
        else if (strTest == "player.displayafterseek")
        {
          ret = PLAYER_DISPLAY_AFTER_SEEK;
        }
        else if (strTest == "player.caching")
        {
          ret = PLAYER_CACHING;
        }
        else if (strTest == "player.seekbar")
        {
          ret = PLAYER_SEEKBAR;
        }
        else if (strTest == "player.seektime")
        {
          ret = PLAYER_SEEKTIME;
        }
        else if (strTest == "player.progress")
        {
          ret = PLAYER_PROGRESS;
        }
        else if (strTest == "player.seeking")
        {
          ret = PLAYER_SEEKING;
        }
        else if (strTest == "player.showtime")
        {
          ret = PLAYER_SHOWTIME;
        }
        else if (strTest == "player.showcodec")
        {
          ret = PLAYER_SHOWCODEC;
        }
        else if (strTest == "player.showinfo")
        {
          ret = PLAYER_SHOWINFO;
        }
        else if (strTest == "player.time")
        {
          ret = PLAYER_TIME;
        }
        else if (strTest == "player.timeremaining")
        {
          ret = PLAYER_TIME_REMAINING;
        }
        else if (strTest == "player.duration")
        {
          ret = PLAYER_DURATION;
        }
        else if (strTest == "player.volume")
        {
          ret = PLAYER_VOLUME;
        }
        else if (strTest == "player.muted")
        {
          ret = PLAYER_MUTED;
        }
        else if (strTest == "player.hasduration")
        {
          ret = PLAYER_HASDURATION;
        }
      }
      else if (strCategory == "weather")
      {
        if (strTest == "weather.conditions")
        {
          ret = WEATHER_CONDITIONS;
        }
        else if (strTest == "weather.temperature")
        {
          ret = WEATHER_TEMPERATURE;
        }
        else if (strTest == "weather.location")
        {
          ret = WEATHER_LOCATION;
        }
        else if (strTest == "weather.isfetched")
        {
          ret = WEATHER_IS_FETCHED;
        }
      }
      else if (strCategory == "system")
      {
        if (strTest == "system.date")
        {
          ret = SYSTEM_DATE;
        }
        else if (strTest == "system.time")
        {
          ret = SYSTEM_TIME;
        }
        else if (strTest == "system.cputemperature")
        {
          ret = SYSTEM_CPU_TEMPERATURE;
        }
        else if (strTest == "system.gputemperature")
        {
          ret = SYSTEM_GPU_TEMPERATURE;
        }
        else if (strTest == "system.fanspeed")
        {
          ret = SYSTEM_FAN_SPEED;
        }
        else if (strTest == "system.freespace(c)")
        {
          ret = SYSTEM_FREE_SPACE_C;
        }
        else if (strTest == "system.freespace(e)")
        {
          ret = SYSTEM_FREE_SPACE_E;
        }
        else if (strTest == "system.freespace(f)")
        {
          ret = SYSTEM_FREE_SPACE_F;
        }
        else if (strTest == "system.freespace(g)")
        {
          ret = SYSTEM_FREE_SPACE_G;
        }
        else if (strTest == "system.buildversion")
        {
          ret = SYSTEM_BUILD_VERSION;
        }
        else if (strTest == "system.builddate")
        {
          ret = SYSTEM_BUILD_DATE;
        }
        else if (strTest == "system.hasnetwork")
        {
          ret = SYSTEM_ETHERNET_LINK_ACTIVE;
        }
        else if (strTest == "system.fps")
        {
          ret = SYSTEM_FPS;
        }
        else if (strTest == "system.kaiconnected")
        {
          ret = SYSTEM_KAI_CONNECTED;
        }
        else if (strTest == "system.kaienabled")
        {
          ret = SYSTEM_KAI_ENABLED;
        }
        else if (strTest == "system.hasmediadvd")
        {
          ret = SYSTEM_MEDIA_DVD;
        }
        else if (strTest == "system.dvdready")
        {
          ret = SYSTEM_DVDREADY;
        }
        else if (strTest == "system.trayopen")
        {
          ret = SYSTEM_TRAYOPEN;
        }
        else if (strTest == "system.autodetection")
        {
          ret = SYSTEM_AUTODETECTION;
        }
        else if (strTest == "system.freememory")
        {
          ret = SYSTEM_FREE_MEMORY;
        }
        else if (strTest == "system.screenmode")
        {
          ret = SYSTEM_SCREEN_MODE;
        }
        else if (strTest == "system.screenwidth")
        {
          ret = SYSTEM_SCREEN_WIDTH;
        }
        else if (strTest == "system.screenheight")
        {
          ret = SYSTEM_SCREEN_HEIGHT;
        }
        else if (strTest == "system.currentwindow")
        {
          ret = SYSTEM_CURRENT_WINDOW;
        }
        else if (strTest == "system.currentcontrol")
        {
          ret = SYSTEM_CURRENT_CONTROL;
        }
        else if (strTest == "system.xboxnickname")
        {
          ret = SYSTEM_XBOX_NICKNAME;
        }
        else if (strTest == "system.dvdlabel")
        {
          ret = SYSTEM_DVD_LABEL;
        }
        else if (strTest == "system.haslocks")
        {
          ret = SYSTEM_HASLOCKS;
        }
        else if (strTest == "system.hasloginscreen")
        {
          ret = SYSTEM_HAS_LOGINSCREEN;
        }
        else if (strTest == "system.ismaster")
        {
          ret = SYSTEM_ISMASTER;
        }
        else if (strTest == "system.internetstate")
        {
          ret = SYSTEM_INTERNET_STATE;
        }
        else if (strTest == "system.loggedon")
        {
          ret = SYSTEM_LOGGEDON;
        }
        else if (strTest.Substring(0, 16) == "system.idletime(")
        {
          int time = Int32.Parse((strTest.Substring(16, strTest.Length - 17)));
          if (time > SYSTEM_IDLE_TIME_FINISH - SYSTEM_IDLE_TIME_START)
          {
            time = SYSTEM_IDLE_TIME_FINISH - SYSTEM_IDLE_TIME_START;
          }
          if (time > 0)
          {
            ret = SYSTEM_IDLE_TIME_START + time;
          }
        }
        else if (strTest.Substring(0, 16) == "system.hddsmart(")
        {
          i_SmartRequest = Int32.Parse((strTest.Substring(16, strTest.Length - 17)));
          if (i_SmartRequest <= 0)
          {
            i_SmartRequest = 17; //falling back to HDD temp
          }
          ret = SYSTEM_HDD_SMART;
        }
        else if (strTest.Substring(0, 16) == "system.hasalarm(")
        {
          return
            AddMultiInfo(new GUIInfo(bNegate ? -SYSTEM_HAS_ALARM : SYSTEM_HAS_ALARM,
                                     ConditionalStringParameter(strTest.Substring(16, strTest.Length - 17)), 0));
        }
          //else if (strTest.Substring(0,16)=="system.alarmpos(")
        else if (strTest == "system.alarmpos")
        {
          ret = SYSTEM_ALARM_POS;
        }
        else if (strTest == "system.profilename")
        {
          ret = SYSTEM_PROFILENAME;
        }
        else if (strTest == "system.profilethumb")
        {
          ret = SYSTEM_PROFILETHUMB;
        }
      }
      else if (strCategory == "xlinkkai")
      {
        if (strTest == "xlinkkai.username")
        {
          ret = XLINK_KAI_USERNAME;
        }
      }
      else if (strCategory == "lcd")
      {
        if (strTest == "lcd.playicon")
        {
          ret = LCD_PLAY_ICON;
        }
        else if (strTest == "lcd.progressbar")
        {
          ret = LCD_PROGRESS_BAR;
        }
        else if (strTest == "lcd.cputemperature")
        {
          ret = LCD_CPU_TEMPERATURE;
        }
        else if (strTest == "lcd.gputemperature")
        {
          ret = LCD_GPU_TEMPERATURE;
        }
        else if (strTest == "lcd.fanspeed")
        {
          ret = LCD_FAN_SPEED;
        }
        else if (strTest == "lcd.date")
        {
          ret = LCD_DATE;
        }
        else if (strTest == "lcd.freespace(c)")
        {
          ret = LCD_FREE_SPACE_C;
        }
        else if (strTest == "lcd.freespace(e)")
        {
          ret = LCD_FREE_SPACE_E;
        }
        else if (strTest == "lcd.freespace(f)")
        {
          ret = LCD_FREE_SPACE_F;
        }
        else if (strTest == "lcd.freespace(g)")
        {
          ret = LCD_FREE_SPACE_G;
        }
      }
      else if (strCategory == "network")
      {
        if (strTest == "network.ipaddress")
        {
          ret = NETWORK_IP_ADDRESS;
        }
      }
      else if (strCategory == "musicplayer")
      {
        if (strTest == "musicplayer.title")
        {
          ret = MUSICPLAYER_TITLE;
        }
        else if (strTest == "musicplayer.album")
        {
          ret = MUSICPLAYER_ALBUM;
        }
        else if (strTest == "musicplayer.artist")
        {
          ret = MUSICPLAYER_ARTIST;
        }
        else if (strTest == "musicplayer.year")
        {
          ret = MUSICPLAYER_YEAR;
        }
        else if (strTest == "musicplayer.genre")
        {
          ret = MUSICPLAYER_GENRE;
        }
        else if (strTest == "musicplayer.time")
        {
          ret = MUSICPLAYER_TIME;
        }
        else if (strTest == "musicplayer.timeremaining")
        {
          ret = MUSICPLAYER_TIME_REMAINING;
        }
        else if (strTest == "musicplayer.timespeed")
        {
          ret = MUSICPLAYER_TIME_SPEED;
        }
        else if (strTest == "musicplayer.tracknumber")
        {
          ret = MUSICPLAYER_TRACK_NUMBER;
        }
        else if (strTest == "musicplayer.duration")
        {
          ret = MUSICPLAYER_DURATION;
        }
        else if (strTest == "musicplayer.cover")
        {
          ret = MUSICPLAYER_COVER;
        }
        else if (strTest == "musicplayer.bitrate")
        {
          ret = MUSICPLAYER_BITRATE;
        }
        else if (strTest == "musicplayer.playlistlength")
        {
          ret = MUSICPLAYER_PLAYLISTLEN;
        }
        else if (strTest == "musicplayer.playlistposition")
        {
          ret = MUSICPLAYER_PLAYLISTPOS;
        }
        else if (strTest == "musicplayer.channels")
        {
          ret = MUSICPLAYER_CHANNELS;
        }
        else if (strTest == "musicplayer.bitspersample")
        {
          ret = MUSICPLAYER_BITSPERSAMPLE;
        }
        else if (strTest == "musicplayer.samplerate")
        {
          ret = MUSICPLAYER_SAMPLERATE;
        }
        else if (strTest == "musicplayer.codec")
        {
          ret = MUSICPLAYER_CODEC;
        }
        else if (strTest == "musicplayer.discnumber")
        {
          ret = MUSICPLAYER_DISC_NUMBER;
        }
      }
      else if (strCategory == "videoplayer")
      {
        if (strTest == "videoplayer.title")
        {
          ret = VIDEOPLAYER_TITLE;
        }
        else if (strTest == "videoplayer.genre")
        {
          ret = VIDEOPLAYER_GENRE;
        }
        else if (strTest == "videoplayer.director")
        {
          ret = VIDEOPLAYER_DIRECTOR;
        }
        else if (strTest == "videoplayer.year")
        {
          ret = VIDEOPLAYER_YEAR;
        }
        else if (strTest == "videoplayer.time")
        {
          ret = VIDEOPLAYER_TIME;
        }
        else if (strTest == "videoplayer.timeremaining")
        {
          ret = VIDEOPLAYER_TIME_REMAINING;
        }
        else if (strTest == "videoplayer.timespeed")
        {
          ret = VIDEOPLAYER_TIME_SPEED;
        }
        else if (strTest == "videoplayer.duration")
        {
          ret = VIDEOPLAYER_DURATION;
        }
        else if (strTest == "videoplayer.cover")
        {
          ret = VIDEOPLAYER_COVER;
        }
        else if (strTest == "videoplayer.usingoverlays")
        {
          ret = VIDEOPLAYER_USING_OVERLAYS;
        }
        else if (strTest == "videoplayer.isfullscreen")
        {
          ret = VIDEOPLAYER_ISFULLSCREEN;
        }
        else if (strTest == "videoplayer.hasmenu")
        {
          ret = VIDEOPLAYER_HASMENU;
        }
        else if (strTest == "videoplayer.playlistlength")
        {
          ret = VIDEOPLAYER_PLAYLISTLEN;
        }
        else if (strTest == "videoplayer.playlistposition")
        {
          ret = VIDEOPLAYER_PLAYLISTPOS;
        }
      }
      else if (strCategory == "playlist")
      {
        if (strTest == "playlist.length")
        {
          ret = PLAYLIST_LENGTH;
        }
        else if (strTest == "playlist.position")
        {
          ret = PLAYLIST_POSITION;
        }
        else if (strTest == "playlist.random")
        {
          ret = PLAYLIST_RANDOM;
        }
        else if (strTest == "playlist.repeat")
        {
          ret = PLAYLIST_REPEAT;
        }
        else if (strTest == "playlist.israndom")
        {
          ret = PLAYLIST_ISRANDOM;
        }
        else if (strTest == "playlist.isrepeat")
        {
          ret = PLAYLIST_ISREPEAT;
        }
        else if (strTest == "playlist.isrepeatone")
        {
          ret = PLAYLIST_ISREPEATONE;
        }
      }
      else if (strCategory == "musicpartymode")
      {
        if (strTest == "musicpartymode.enabled")
        {
          ret = MUSICPM_ENABLED;
        }
        else if (strTest == "musicpartymode.songsplayed")
        {
          ret = MUSICPM_SONGSPLAYED;
        }
        else if (strTest == "musicpartymode.matchingsongs")
        {
          ret = MUSICPM_MATCHINGSONGS;
        }
        else if (strTest == "musicpartymode.matchingsongspicked")
        {
          ret = MUSICPM_MATCHINGSONGSPICKED;
        }
        else if (strTest == "musicpartymode.matchingsongsleft")
        {
          ret = MUSICPM_MATCHINGSONGSLEFT;
        }
        else if (strTest == "musicpartymode.relaxedsongspicked")
        {
          ret = MUSICPM_RELAXEDSONGSPICKED;
        }
        else if (strTest == "musicpartymode.randomsongspicked")
        {
          ret = MUSICPM_RANDOMSONGSPICKED;
        }
      }
      else if (strCategory == "audioscrobbler")
      {
        if (strTest == "audioscrobbler.enabled")
        {
          ret = AUDIOSCROBBLER_ENABLED;
        }
        else if (strTest == "audioscrobbler.connectstate")
        {
          ret = AUDIOSCROBBLER_CONN_STATE;
        }
        else if (strTest == "audioscrobbler.submitinterval")
        {
          ret = AUDIOSCROBBLER_SUBMIT_INT;
        }
        else if (strTest == "audioscrobbler.filescached")
        {
          ret = AUDIOSCROBBLER_FILES_CACHED;
        }
        else if (strTest == "audioscrobbler.submitstate")
        {
          ret = AUDIOSCROBBLER_SUBMIT_STATE;
        }
      }
      else if (strCategory.StartsWith("container"))
      {
        //000000000011111111111222222222233333333334444444444
        //012345678900123456789012345678901234567890123456789
        //Container.ListItem(1).Icon
        //Listitem(
        int id = 0;
        if (strCategory.Length > 10)
        {
          Int32.TryParse(strCategory.Substring(10), out id);
        }
        string info = strTest.Substring(10);
        if (info.StartsWith("listitem"))
        {
          int offset = 0;
          string sid = info.Substring(9);
          int p = sid.IndexOf(")");
          sid = sid.Substring(0, p);
          Int32.TryParse(sid, out offset);
          ret = TranslateListItem(info.Substring(info.IndexOf(".") + 1));
          if (offset != 0 || id != 0)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -ret : ret, id, offset));
          }
        }
        else if (info == "folderthumb")
        {
          ret = CONTAINER_FOLDERTHUMB;
        }
        else if (info == "folderpath")
        {
          ret = CONTAINER_FOLDERPATH;
        }
        else if (info == "onnext")
        {
          ret = CONTAINER_ON_NEXT;
        }
        else if (info == "onprevious")
        {
          ret = CONTAINER_ON_PREVIOUS;
        }
        else if (info.StartsWith("content("))
        {
          return
            AddMultiInfo(new GUIInfo(bNegate ? -CONTAINER_CONTENT : CONTAINER_CONTENT,
                                     ConditionalStringParameter(info.Substring(8, info.Length - 9)), 0));
        }
        else if (info == "hasthumb")
        {
          ret = CONTAINER_HAS_THUMB;
        }
        else if (info.StartsWith("sort("))
        {
          //SORT_METHOD sort = SORT_METHOD_NONE;
          //string method = info.Substring(5, info.Length - 6);
          //if (method.Equals("songrating")) sort = SORT_METHOD_SONG_RATING;
          //if (sort != SORT_METHOD_NONE)
          //  return AddMultiInfo(new GUIInfo(bNegate ? -CONTAINER_SORT_METHOD : CONTAINER_SORT_METHOD, sort));
        }
        else if (id != 0 && info.StartsWith("hasfocus("))
        {
          int itemID;
          string sid = info.Substring(9);
          int p = sid.IndexOf(")");
          sid = sid.Substring(0, p);
          Int32.TryParse(sid, out itemID);
          return AddMultiInfo(new GUIInfo(bNegate ? -CONTAINER_HAS_FOCUS : CONTAINER_HAS_FOCUS, id, itemID));
        }
        if (id != 0 && (ret == CONTAINER_ON_NEXT || ret == CONTAINER_ON_PREVIOUS))
        {
          return AddMultiInfo(new GUIInfo(bNegate ? -ret : ret, id));
        }
      }
      else if (strCategory == "listitem")
      {
        if (strTest == "listitem.thumb")
        {
          ret = LISTITEM_THUMB;
        }
        else if (strTest == "listitem.icon")
        {
          ret = LISTITEM_ICON;
        }
        else if (strTest == "listitem.label")
        {
          ret = LISTITEM_LABEL;
        }
        else if (strTest == "listitem.title")
        {
          ret = LISTITEM_TITLE;
        }
        else if (strTest == "listitem.tracknumber")
        {
          ret = LISTITEM_TRACKNUMBER;
        }
        else if (strTest == "listitem.artist")
        {
          ret = LISTITEM_ARTIST;
        }
        else if (strTest == "listitem.album")
        {
          ret = LISTITEM_ALBUM;
        }
        else if (strTest == "listitem.year")
        {
          ret = LISTITEM_YEAR;
        }
        else if (strTest == "listitem.genre")
        {
          ret = LISTITEM_GENRE;
        }
        else if (strTest == "listitem.director")
        {
          ret = LISTITEM_DIRECTOR;
        }
      }
      else if (strCategory == "visualisation")
      {
        if (strTest == "visualisation.locked")
        {
          ret = VISUALISATION_LOCKED;
        }
        else if (strTest == "visualisation.preset")
        {
          ret = VISUALISATION_PRESET;
        }
        else if (strTest == "visualisation.name")
        {
          ret = VISUALISATION_NAME;
        }
        else if (strTest == "visualisation.enabled")
        {
          ret = VISUALISATION_ENABLED;
        }
      }
      else if (strCategory == "skin" || strCategory == "string")
      {
        if (strTest == "skin.currenttheme")
        {
          ret = SKIN_THEME;
        }
          // string.equals(val1, val2) will check the equality of val1 to val2.
          // string.equals(val1)       will return true if val1 has a length > 0
        else if (strTest.Substring(0, 14) == "string.equals(")
        {
          // this condition uses GUIPropertyManager.Parse, which is case sensitive.
          string strTestKeepCase = strCondition;
          strTestKeepCase = strTestKeepCase.TrimStart(new char[] {' '});
          strTestKeepCase = strTestKeepCase.TrimEnd(new char[] {' '});
          if (bNegate)
          {
            strTestKeepCase = strTestKeepCase.Remove(0, 1);
          }

          int skinOffset;
          int pos = strTestKeepCase.IndexOf(",");
          if (pos >= 0)
          {
            skinOffset = SkinSettings.TranslateSkinString(strTestKeepCase.Substring(14, pos - 14));
            int compareString =
              ConditionalStringParameter(strTestKeepCase.Substring(pos + 1, strTestKeepCase.Length - (pos + 2)));
            return AddMultiInfo(new GUIInfo(bNegate ? -STRING_EQUALS : STRING_EQUALS, skinOffset, compareString));
          }
          skinOffset = SkinSettings.TranslateSkinString(strTestKeepCase.Substring(14, strTestKeepCase.Length - 15));
          return AddMultiInfo(new GUIInfo(bNegate ? -STRING_EQUALS : STRING_EQUALS, skinOffset));
        }
        else if (strTest.Substring(0, 16) == "string.contains(")
        {
          // this condition uses GUIPropertyManager.Parse, which is case sensitive.
          string strTestKeepCase = strCondition;
          strTestKeepCase = strTestKeepCase.TrimStart(new char[] {' '});
          strTestKeepCase = strTestKeepCase.TrimEnd(new char[] {' '});
          if (bNegate)
          {
            strTestKeepCase = strTestKeepCase.Remove(0, 1);
          }

          int skinOffset;
          int pos = strTestKeepCase.IndexOf(",");
          if (pos >= 0)
          {
            skinOffset = SkinSettings.TranslateSkinString(strTestKeepCase.Substring(16, pos - 16));
            int compareString =
              ConditionalStringParameter(strTestKeepCase.Substring(pos + 1, strTestKeepCase.Length - (pos + 2)));
            return AddMultiInfo(new GUIInfo(bNegate ? -STRING_CONTAINS : STRING_CONTAINS, skinOffset, compareString));
          }
          skinOffset = SkinSettings.TranslateSkinString(strTestKeepCase.Substring(16, strTestKeepCase.Length - 17));
          return AddMultiInfo(new GUIInfo(bNegate ? -STRING_CONTAINS : STRING_CONTAINS, skinOffset));
        }
        else if (strTest.Substring(0, 14) == "string.starts(")
        {
          // this condition uses GUIPropertyManager.Parse, which is case sensitive.
          string strTestKeepCase = strCondition;
          strTestKeepCase = strTestKeepCase.TrimStart(new char[] {' '});
          strTestKeepCase = strTestKeepCase.TrimEnd(new char[] {' '});
          if (bNegate)
          {
            strTestKeepCase = strTestKeepCase.Remove(0, 1);
          }

          int skinOffset;
          int pos = strTestKeepCase.IndexOf(",");
          if (pos >= 0)
          {
            skinOffset = SkinSettings.TranslateSkinString(strTestKeepCase.Substring(14, pos - 14));
            int compareString =
              ConditionalStringParameter(strTestKeepCase.Substring(pos + 1, strTestKeepCase.Length - (pos + 2)));
            return AddMultiInfo(new GUIInfo(bNegate ? -STRING_STARTS : STRING_STARTS, skinOffset, compareString));
          }
          skinOffset = SkinSettings.TranslateSkinString(strTestKeepCase.Substring(14, strTestKeepCase.Length - 15));
          return AddMultiInfo(new GUIInfo(bNegate ? -STRING_STARTS : STRING_STARTS, skinOffset));
        }
        else if (strTest.Substring(0, 16) == "skin.hassetting(")
        {
          int skinOffset = SkinSettings.TranslateSkinBool(strTest.Substring(16, strTest.Length - 17));
          return AddMultiInfo(new GUIInfo(bNegate ? -SKIN_BOOL : SKIN_BOOL, skinOffset));
        }
        else if (strTest.Substring(0, 14) == "skin.hastheme(")
        {
          ret = SKIN_HAS_THEME_START + ConditionalStringParameter(strTest.Substring(14, strTest.Length - 15));
        }
      }
      else if (strCategory == "window")
      {
        if (strTest == "window.isosdvisible")
        {
          ret = WINDOW_IS_OSD_VISIBLE;
        }
        else if (strTest.Substring(0, 16) == "window.isactive(")
        {
          int winID = TranslateWindowString(strTest.Substring(16, strTest.Length - 17));
          if (winID != (int)GUIWindow.Window.WINDOW_INVALID)
          {
            ret = winID;
          }
        }
        else if (strTest == "window.ismedia")
        {
          return WINDOW_IS_MEDIA;
        }
        else if (strTest.Substring(0, 17) == "window.istopmost(")
        {
          int winID = TranslateWindowString(strTest.Substring(17, strTest.Length - 18));
          if (winID != (int)GUIWindow.Window.WINDOW_INVALID)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -WINDOW_IS_TOPMOST : WINDOW_IS_TOPMOST, winID, 0));
          }
        }
        else if (strTest.Substring(0, 17) == "window.isvisible(")
        {
          int winID = TranslateWindowString(strTest.Substring(17, strTest.Length - 18));
          if (winID != (int)GUIWindow.Window.WINDOW_INVALID)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -WINDOW_IS_VISIBLE : WINDOW_IS_VISIBLE, winID, 0));
          }
        }
        else if (strTest.Substring(0, 16) == "window.previous(")
        {
          int winID = TranslateWindowString(strTest.Substring(16, strTest.Length - 17));
          if (winID != (int)GUIWindow.Window.WINDOW_INVALID)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -WINDOW_PREVIOUS : WINDOW_PREVIOUS, winID, 0));
          }
        }
        else if (strTest.Substring(0, 12) == "window.next(")
        {
          int winID = TranslateWindowString(strTest.Substring(12, strTest.Length - 13));
          if (winID != (int)GUIWindow.Window.WINDOW_INVALID)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -WINDOW_NEXT : WINDOW_NEXT, winID, 0));
          }
        }
      }
      else if (strCategory == "plugin")
      {
        if (strTest.Substring(0, 17) == "plugin.isenabled(")
        {
          // use original condition, because plugin Name is case sensitive
          string pluginName = strCondition;
          pluginName = pluginName.TrimStart(new char[] {' '});
          pluginName = pluginName.TrimEnd(new char[] {' '});
          if (bNegate)
          {
            pluginName = pluginName.Remove(0, 1);
          }

          pluginName = pluginName.Substring(17, strTest.Length - 18);

          if (pluginName != string.Empty)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -PLUGIN_IS_ENABLED : PLUGIN_IS_ENABLED, pluginName));
          }
        }
      }
      else if (strCategory == "control")
      {
        if (strTest.Substring(0, 17) == "control.hasfocus(")
        {
          int controlID = Int32.Parse(strTest.Substring(17, strTest.Length - 18));
          if (controlID != 0)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -CONTROL_HAS_FOCUS : CONTROL_HAS_FOCUS, controlID, 0));
          }
        }
        else if (strTest.Substring(0, 18) == "control.isvisible(")
        {
          int controlID = Int32.Parse(strTest.Substring(18, strTest.Length - 19));
          if (controlID != 0)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -CONTROL_IS_VISIBLE : CONTROL_IS_VISIBLE, controlID, 0));
          }
        }
        else if (strTest.Substring(0, 17) == "control.hasthumb(")
        {
          int controlID = Int32.Parse(strTest.Substring(17, strTest.Length - 18));
          if (controlID != 0)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -CONTROL_HAS_THUMB : CONTROL_HAS_THUMB, controlID, 0));
          }
        }
        else if (strTest.Substring(0, 16) == "control.hastext(")
        {
          int controlID = Int32.Parse(strTest.Substring(16, strTest.Length - 17));
          if (controlID != 0)
          {
            return AddMultiInfo(new GUIInfo(bNegate ? -CONTROL_HAS_TEXT : CONTROL_HAS_TEXT, controlID, 0));
          }
        }
      }
      else if (strCategory == "facadeview")
      {
        if (strTest == "facadeview.album")
        {
          ret = FACADEVIEW_ALBUM;
        }
        else if (strTest == "facadeview.filmstrip")
        {
          ret = FACADEVIEW_FILMSTRIP;
        }
        else if (strTest == "facadeview.largeicons")
        {
          ret = FACADEVIEW_LARGEICONS;
        }
        else if (strTest == "facadeview.list")
        {
          ret = FACADEVIEW_LIST;
        }
        else if (strTest == "facadeview.playlist")
        {
          ret = FACADEVIEW_PLAYLIST;
        }
        else if (strTest == "facadeview.smallicons")
        {
          ret = FACADEVIEW_SMALLICONS;
        }
      }
      else if (strTest.Length >= 13 && strTest.Substring(0, 13) == "controlgroup(")
      {
        int groupPos = strTest.IndexOf(")");
        int groupID = Int32.Parse(strTest.Substring(13, groupPos - 13));
        int controlID = 0;
        int controlPos = strTest.IndexOf(".hasfocus(");
        if (controlPos > 0)
        {
          controlID = Int32.Parse(strTest.Substring(controlPos + 10, strTest.Length - controlPos - 11));
        }
        if (groupID != 0)
        {
          return
            AddMultiInfo(new GUIInfo(bNegate ? -CONTROL_GROUP_HAS_FOCUS : CONTROL_GROUP_HAS_FOCUS, groupID, controlID));
        }
      }
      else if (strTest.Length >= 24 && strTest.Substring(0, 24) == "buttonscroller.hasfocus(")
      {
        int controlID = Int32.Parse(strTest.Substring(24, strTest.Length - 24));
        if (controlID != 0)
        {
          return AddMultiInfo(new GUIInfo(bNegate ? -BUTTON_SCROLLER_HAS_ICON : BUTTON_SCROLLER_HAS_ICON, controlID, 0));
        }
      }

      return bNegate ? -ret : ret;
    }

    private static int AddMultiInfo(GUIInfo info)
    {
      // check to see if we have this info already
      for (int i = 0; i < m_multiInfo.Count; i++)
      {
        if (m_multiInfo[i].Equals(info))
        {
          return i + MULTI_INFO_START;
        }
      }
      // return the new offset
      m_multiInfo.Add(info);
      return m_multiInfo.Count + MULTI_INFO_START - 1;
    }

    private static int ConditionalStringParameter(string parameter)
    {
      // check to see if we have this parameter already
      for (int i = 0; i < m_stringParameters.Count; i++)
      {
        if (parameter == m_stringParameters[i])
        {
          return (int)i;
        }
      }
      // return the new offset
      m_stringParameters.Add(parameter);
      return (int)m_stringParameters.Count - 1;
    }

    public static void Clear()
    {
      //m_currentSong.Clear();
      //m_currentMovie.Clear();
      m_CombinedValues.Clear();
    }

    private static int TranslateBooleanExpression(string expression)
    {
      CCombinedValue comb = new CCombinedValue();
      comb.m_info = expression;
      comb.m_id = COMBINED_VALUES_START + m_CombinedValues.Count;

      // operator stack
      Stack<char> save = new Stack<char>();

      string operand = "";

      for (int i = 0; i < expression.Length; i++)
      {
        if (GetOperator(expression[i]) != 0)
        {
          // cleanup any operand, translate and put into our expression list
          if (operand.Length > 0)
          {
            int iOp = TranslateSingleString(operand);
            if (iOp != 0)
            {
              comb.m_postfix.Add(iOp);
            }
            operand = "";
          }

          // handle closing parenthesis
          if (expression[i] == ']')
          {
            while (save.Count > 0)
            {
              char oper = save.Peek();
              save.Pop();

              if (oper == '[')
              {
                break;
              }

              comb.m_postfix.Add(-GetOperator(oper));
            }
          }
          else
          {
            // all other operators we pop off the stack any operator
            // that has a higher priority than the one we have.
            while (save.Count > 0 && GetOperator(save.Peek()) > GetOperator(expression[i]))
            {
              // only handle parenthesis once they're closed.
              if (save.Peek() == '[' && expression[i] != ']')
              {
                break;
              }

              comb.m_postfix.Add(-GetOperator(save.Peek())); // negative denotes operator
              save.Pop();
            }
            save.Push(expression[i]);
          }
        }
        else
        {
          operand += expression[i];
        }
      }

      if (operand.Length != 0)
      {
        comb.m_postfix.Add(TranslateSingleString(operand));
      }

      // finish up by adding any operators
      while (save.Count > 0)
      {
        comb.m_postfix.Add(-GetOperator(save.Peek()));
        save.Pop();
      }

      // test evaluate
      bool test = false;
      if (!EvaluateBooleanExpression(comb, ref test, (int)GUIWindow.Window.WINDOW_INVALID))
      {
        //Log(LOGERROR, "Error evaluating boolean expression %s", expression.c_str());
      }
      // success - add to our combined values
      m_CombinedValues.Add(comb);
      return comb.m_id;
    }

    private static int GetOperator(char ch)
    {
      if (ch == '[')
      {
        return 5;
      }
      else if (ch == ']')
      {
        return 4;
      }
      else if (ch == '!')
      {
        return OPERATOR_NOT;
      }
      else if (ch == '+')
      {
        return OPERATOR_AND;
      }
      else if (ch == '|')
      {
        return OPERATOR_OR;
      }
      else
      {
        return 0;
      }
    }

    private static bool EvaluateBooleanExpression(CCombinedValue expression, ref bool result, int dwContextWindow)
    {
      // stack to save our bool state as we go
      Stack<bool> save = new Stack<bool>();

      for (int i = 0; i < expression.m_postfix.Count; ++i)
      {
        int expr = expression.m_postfix[i];
        if (expr == -OPERATOR_NOT)
        {
          // NOT the top item on the stack
          if (save.Count < 1)
          {
            return false;
          }
          bool expra = save.Peek();
          save.Pop();
          save.Push(!expra);
        }
        else if (expr == -OPERATOR_AND)
        {
          // AND the top two items on the stack
          if (save.Count < 2)
          {
            return false;
          }
          bool right = save.Peek();
          save.Pop();
          bool left = save.Peek();
          save.Pop();
          save.Push(left && right);
        }
        else if (expr == -OPERATOR_OR)
        {
          // OR the top two items on the stack
          if (save.Count < 2)
          {
            return false;
          }
          bool right = save.Peek();
          save.Pop();
          bool left = save.Peek();
          save.Pop();
          save.Push(left || right);
        }
        else // operator
        {
          save.Push(GetBool(expr, dwContextWindow));
        }
      }
      if (save.Count != 1)
      {
        return false;
      }
      result = save.Peek();
      return true;
    }

    public static int TranslateWindowString(string strWindow)
    {
      int wWindowID = (int)GUIWindow.Window.WINDOW_INVALID;
      if (int.TryParse(strWindow, out wWindowID))
      {
        return wWindowID;
      }

      if (strWindow.Equals("home"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_HOME;
      }
      else if (strWindow.Equals("myprograms"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_FILES;
      }
      else if (strWindow.Equals("mypictures"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_PICTURES;
      }
      else if (strWindow.Equals("myfiles"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_FILES;
      }
      else if (strWindow.Equals("settings"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS;
      }
      else if (strWindow.Equals("mymusic"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_MUSIC;
      }
      else if (strWindow.Equals("mymusicfiles"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
      }
      else if (strWindow.Equals("myvideos"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VIDEOS;
      }
      else if (strWindow.Equals("systeminfo"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_SYSTEM_INFORMATION;
      }
        //else if (strWindow.Equals("guicalibration")) wWindowID = (int)GUIWindow.Window.WINDOW_SCREEN_CALIBRATION;
      else if (strWindow.Equals("screencalibration"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_UI_CALIBRATION;
      }
      else if (strWindow.Equals("mypicturessettings"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS_SLIDESHOW;
      }
        //else if (strWindow.Equals("myprogramssettings")) wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS_MYPROGRAMS;
        //else if (strWindow.Equals("myweathersettings")) wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS_MYWEATHER;
      else if (strWindow.Equals("mymusicsettings"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS_MUSIC;
      }
        //else if (strWindow.Equals("systemsettings")) wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS_SYSTEM;
        //else if (strWindow.Equals("myvideossettings")) wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS_MYVIDEOS;
        //else if (strWindow.Equals("networksettings")) wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS_NETWORK;
        //else if (strWindow.Equals("appearancesettings")) wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS_APPEARANCE;
      else if (strWindow.Equals("scripts"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_SCRIPTS;
      }
      else if (strWindow.Equals("myvideofiles"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VIDEOS;
      }
      else if (strWindow.Equals("myvideogenres"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VIDEO_GENRE;
      }
      else if (strWindow.Equals("myvideoactors"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VIDEO_ACTOR;
      }
      else if (strWindow.Equals("myvideoyears"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VIDEO_YEAR;
      }
      else if (strWindow.Equals("myvideotitles"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VIDEO_TITLE;
      }
      else if (strWindow.Equals("myvideoplaylist"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VIDEO_PLAYLIST;
      }
        //else if (strWindow.Equals("profiles")) wWindowID = (int)GUIWindow.Window.WINDOW_SETTINGS_PROFILES;
      else if (strWindow.Equals("yesnodialog"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_YES_NO;
      }
      else if (strWindow.Equals("progressdialog"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS;
      }
        //else if (strWindow.Equals("invitedialog")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_INVITE;
      else if (strWindow.Equals("virtualkeyboard"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD;
      }
        //else if (strWindow.Equals("volumebar")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_VOLUME_BAR;
        //else if (strWindow.Equals("submenu")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_SUB_MENU;
        //else if (strWindow.Equals("contextmenu")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_CONTEXT_MENU;
        //else if (strWindow.Equals("infodialog")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_KAI_TOAST;
        //else if (strWindow.Equals("hostdialog")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_HOST;
        //else if (strWindow.Equals("numericinput")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_NUMERIC;
        //else if (strWindow.Equals("gamepadinput")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_GAMEPAD;
        //else if (strWindow.Equals("shutdownmenu")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_BUTTON_MENU;
        //else if (strWindow.Equals("scandialog")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_MUSIC_SCAN;
        //else if (strWindow.Equals("mutebug")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_MUTE_BUG;
        //else if (strWindow.Equals("playercontrols")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_PLAYER_CONTROLS;
        //else if (strWindow.Equals("seekbar")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_SEEK_BAR;
        //else if (strWindow.Equals("musicosd")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_MUSIC_OSD;
        //else if (strWindow.Equals("visualisationsettings")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_VIS_SETTINGS;
        //else if (strWindow.Equals("visualisationpresetlist")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_VIS_PRESET_LIST;
        //else if (strWindow.Equals("osdvideosettings")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_VIDEO_OSD_SETTINGS;
        //else if (strWindow.Equals("osdaudiosettings")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_AUDIO_OSD_SETTINGS;
        //else if (strWindow.Equals("videobookmarks")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_VIDEO_BOOKMARKS;
        //else if (strWindow.Equals("trainersettings")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_TRAINER_SETTINGS;
        //else if (strWindow.Equals("profilesettings")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_PROFILE_SETTINGS;
        //else if (strWindow.Equals("locksettings")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_LOCK_SETTINGS;
        //else if (strWindow.Equals("networksetup")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_NETWORK_SETUP;
        //else if (strWindow.Equals("mediasource")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_MEDIA_SOURCE;
      else if (strWindow.Equals("mymusicplaylist"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST;
      }
      else if (strWindow.Equals("mymusicfiles"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
      }
        //else if (strWindow.Equals("mymusiclibrary")) wWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_NAV;
        //else if (strWindow.Equals("mymusictop100")) wWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_TOP100;
        //  else if (strWindow.Equals("virtualkeyboard")) wWindowID = (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD;
      else if (strWindow.Equals("selectdialog"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_SELECT;
      }
      else if (strWindow.Equals("musicinformation"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_INFO;
      }
      else if (strWindow.Equals("okdialog"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_OK;
      }
      else if (strWindow.Equals("movieinformation"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VIDEO_INFO;
      }
        //else if (strWindow.Equals("scriptsdebuginfo")) wWindowID = (int)GUIWindow.Window.WINDOW_SCRIPTS_INFO;
      else if (strWindow.Equals("fullscreenvideo"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO;
      }
      else if (strWindow.Equals("visualisation"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_VISUALISATION;
      }
      else if (strWindow.Equals("slideshow"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_SLIDESHOW;
      }
      else if (strWindow.Equals("filestackingdialog"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_FILESTACKING;
      }
      else if (strWindow.Equals("weather"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_WEATHER;
      }
        //else if (strWindow.Equals("xlinkkai")) wWindowID = (int)GUIWindow.Window.WINDOW_BUDDIES;
      else if (strWindow.Equals("screensaver"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_SCREENSAVER;
      }
      else if (strWindow.Equals("videoosd"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_OSD;
      }
        //else if (strWindow.Equals("videomenu")) wWindowID = (int)GUIWindow.Window.WINDOW_VIDEO_MENU;
        //else if (strWindow.Equals("filebrowser")) wWindowID = (int)GUIWindow.Window.WINDOW_DIALOG_FILE_BROWSER;
        //else if (strWindow.Equals("startup")) wWindowID = (int)GUIWindow.Window.WINDOW_STARTUP;
      else if (strWindow.Equals("startwindow"))
      {
        wWindowID = (int)GUIWindow.Window.WINDOW_HOME;
      }
        //else if (strWindow.Equals("loginscreen")) wWindowID = (int)GUIWindow.Window.WINDOW_LOGIN_SCREEN;
      else
      {
        Log.Error("Window Translator: Can't find window {0}", strWindow);
      }

      //CLog::Log(LOGDEBUG,"CButtonTranslator::TranslateWindowString(%s) returned Window ID (%i)", szWindow, wWindowID);
      return wWindowID;
    }

    // checks the condition and returns it as necessary.  Currently used
    // for toggle button controls and visibility of images.
    public static bool GetBool(int condition1, int dwContextWindow)
    {
      // check our cache
      bool result = false;
      if (IsCached(condition1, dwContextWindow, ref result))
      {
        return result;
      }

      if (condition1 >= COMBINED_VALUES_START && (condition1 - COMBINED_VALUES_START) < (int)(m_CombinedValues.Count))
      {
        CCombinedValue comb = m_CombinedValues[condition1 - COMBINED_VALUES_START];

        if (!EvaluateBooleanExpression(comb, ref result, dwContextWindow))
        {
          result = false;
        }
        //CacheBool(condition1, dwContextWindow, result);
        return result;
      }

      int condition = Math.Abs(condition1);
      bool bReturn = false;

      // GeminiServer: Ethernet Link state checking
      // Will check if the Xbox has a Ethernet Link connection! [Cable in!]
      // This can used for the skinner to switch off Network or Inter required functions
      if (condition == SYSTEM_ALWAYS_TRUE)
      {
        bReturn = true;
      }
      else if (condition == SYSTEM_ALWAYS_FALSE)
      {
        bReturn = false;
      }
      else if (condition == SYSTEM_ETHERNET_LINK_ACTIVE)
      {
        bReturn = true; //bool result;bReturn = (XNetGetEthernetLinkStatus() & XNET_ETHERNET_LINK_ACTIVE);
      }
      else if (condition > SYSTEM_IDLE_TIME_START && condition <= SYSTEM_IDLE_TIME_FINISH)
      {
        bReturn = false; //bReturn = (g_Player.GlobalIdleTime() >= condition - SYSTEM_IDLE_TIME_START);
      }
        //else if (condition >= WINDOW_ACTIVE_START && condition <= WINDOW_ACTIVE_END)// check for Window.IsActive(window)
        //  bReturn = (GUIWindowManager.ActiveWindow == condition);
      else if (condition == WINDOW_IS_MEDIA)
      {
        //GUIWindow pWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
        //bReturn = (pWindow && pWindow.IsMediaWindow());
        bReturn = false;
      }
      else if (condition == WINDOW_IS_OSD_VISIBLE)
      {
        bReturn = GUIWindowManager.IsOsdVisible;
      }
      else if (condition == PLAYER_MUTED)
      {
        bReturn = (g_Player.Volume == 0); //g_stSettings.m_bMute;
      }
      else if (condition == SYSTEM_MEDIA_DVD)
      {
        /*// we must: 1.  Check tray state.
        //          2.  Check that we actually have a disc in the drive (detection
        //              of disk type takes a while from a separate thread).
        CIoSupport TrayIO;
        int iTrayState = TrayIO.GetTrayState();
        if (iTrayState == DRIVE_CLOSED_MEDIA_PRESENT || iTrayState == TRAY_CLOSED_MEDIA_PRESENT)
          bReturn = IsDiscInDrive();
        else
          bReturn = false;
         * */
        bReturn = false;
      }
      else if (condition == SYSTEM_DVDREADY)
      {
        bReturn = false; //bReturn = DriveReady() != DRIVE_NOT_READY;
      }
      else if (condition == SYSTEM_TRAYOPEN)
      {
        bReturn = false; //bReturn = DriveReady() == DRIVE_OPEN;
      }
      else if (condition == PLAYER_SHOWINFO)
      {
        bReturn = false; //bReturn = m_playerShowInfo;
      }
      else if (condition == PLAYER_SHOWCODEC)
      {
        bReturn = false; //bReturn = m_playerShowCodec;
      }
      else if (condition >= MULTI_INFO_START && condition <= MULTI_INFO_END)
      {
        // cache return value
        result = GetMultiInfoBool(m_multiInfo[condition - MULTI_INFO_START], dwContextWindow);
        //CacheBool(condition1, dwContextWindow, result);
        return result;
      }
      else if (condition == SYSTEM_HASLOCKS)
      {
        bReturn = false; //bReturn = g_settings.m_vecProfiles[0].getLockMode() != LOCK_MODE_EVERYONE;
      }
      else if (condition == SYSTEM_ISMASTER)
      {
        bReturn = false;
        // bReturn = g_settings.m_vecProfiles[0].getLockMode() != LOCK_MODE_EVERYONE && g_passwordManager.bMasterUser;
      }
      else if (condition == SYSTEM_LOGGEDON)
      {
        bReturn = false; //bReturn = !(GUIWindowManager.ActiveWindow == WINDOW_LOGIN_SCREEN);
      }
      else if (condition == SYSTEM_HAS_LOGINSCREEN)
      {
        bReturn = false; //bReturn = g_settings.bUseLoginScreen;
      }
      else if (condition == WEATHER_IS_FETCHED)
      {
        bReturn = false; //bReturn = g_weatherManager.IsFetched();
      }
      else if (condition == SYSTEM_INTERNET_STATE)
      {
        bReturn = false; //bReturn = SystemHasInternet();
      }

      else if (condition >= 800 && condition <= 805)
      {
        bReturn = false;
        string viewmode = GUIPropertyManager.GetProperty("#facadeview.viewmode");
        if (viewmode == "album" && condition == FACADEVIEW_ALBUM)
        {
          bReturn = true;
        }
        else if (viewmode == "filmstrip" && condition == FACADEVIEW_FILMSTRIP)
        {
          bReturn = true;
        }
        else if (viewmode == "largeicons" && condition == FACADEVIEW_LARGEICONS)
        {
          bReturn = true;
        }
        else if (viewmode == "list" && condition == FACADEVIEW_LIST)
        {
          bReturn = true;
        }
        else if (viewmode == "playlist" && condition == FACADEVIEW_PLAYLIST)
        {
          bReturn = true;
        }
        else if (viewmode == "smallicons" && condition == FACADEVIEW_SMALLICONS)
        {
          bReturn = true;
        }
      }
      else if (g_Player.Playing)
      {
        switch (condition)
        {
          case PLAYER_HAS_MEDIA:
            bReturn = true;
            break;
          case PLAYER_HAS_AUDIO:
            bReturn = !g_Player.HasVideo;
            break;
          case PLAYER_HAS_VIDEO:
            bReturn = g_Player.HasVideo;
            break;
          case PLAYER_PLAYING:
            bReturn = !(g_Player.Paused && (g_Player.Speed == 1));
            break;
          case PLAYER_PAUSED:
            bReturn = g_Player.Paused;
            break;
          case PLAYER_REWINDING:
            bReturn = !g_Player.Paused && g_Player.Speed < 1;
            break;
          case PLAYER_FORWARDING:
            bReturn = !g_Player.Paused && g_Player.Speed > 1;
            break;
          case PLAYER_REWINDING_2x:
            bReturn = !g_Player.Paused && g_Player.Speed == -2;
            break;
          case PLAYER_REWINDING_4x:
            bReturn = !g_Player.Paused && g_Player.Speed == -4;
            break;
          case PLAYER_REWINDING_8x:
            bReturn = !g_Player.Paused && g_Player.Speed == -8;
            break;
          case PLAYER_REWINDING_16x:
            bReturn = !g_Player.Paused && g_Player.Speed == -16;
            break;
          case PLAYER_REWINDING_32x:
            bReturn = !g_Player.Paused && g_Player.Speed == -32;
            break;
          case PLAYER_FORWARDING_2x:
            bReturn = !g_Player.Paused && g_Player.Speed == 2;
            break;
          case PLAYER_FORWARDING_4x:
            bReturn = !g_Player.Paused && g_Player.Speed == 4;
            break;
          case PLAYER_FORWARDING_8x:
            bReturn = !g_Player.Paused && g_Player.Speed == 8;
            break;
          case PLAYER_FORWARDING_16x:
            bReturn = !g_Player.Paused && g_Player.Speed == 16;
            break;
          case PLAYER_FORWARDING_32x:
            bReturn = !g_Player.Paused && g_Player.Speed == 32;
            break;
          case PLAYER_CAN_RECORD:
            //bReturn = g_Player.m_pPlayer.CanRecord();
            bReturn = false;
            break;
          case PLAYER_RECORDING:
            //bReturn = g_Player.m_pPlayer.IsRecording();
            bReturn = false;
            break;
          case PLAYER_DISPLAY_AFTER_SEEK:
            // bReturn = GetDisplayAfterSeek();
            bReturn = false;
            break;
          case PLAYER_CACHING:
            //bReturn = g_Player.m_pPlayer.IsCaching();
            bReturn = false;
            break;
          case PLAYER_SEEKBAR:
            {
              //CGUIDialogSeekBar *seekBar = (CGUIDialogSeekBar*)GUIWindowManager.GetWindow(WINDOW_DIALOG_SEEK_BAR);
              //bReturn = seekBar ? seekBar.IsRunning() : false;
              bReturn = false;
            }
            break;
          case PLAYER_SEEKING:
            //bReturn = m_playerSeeking;
            bReturn = false;
            break;
          case PLAYER_SHOWTIME:
            //bReturn = m_playerShowTime;
            bReturn = false;
            break;
          case MUSICPM_ENABLED:
            //bReturn = g_partyModeManager.IsEnabled();
            bReturn = false;
            break;
          case AUDIOSCROBBLER_ENABLED:
            //bReturn = g_guiSettings.GetBool("lastfm.enable");
            bReturn = false;
            break;
          case VIDEOPLAYER_USING_OVERLAYS:
            bReturn = GUIGraphicsContext.Overlay;
            break;
          case VIDEOPLAYER_ISFULLSCREEN:
            bReturn = GUIGraphicsContext.IsFullScreenVideo;
            break;
          case VIDEOPLAYER_HASMENU:
            bReturn = g_Player.IsDVD;
            break;
          case PLAYLIST_ISRANDOM:
            //bReturn = g_playlistPlayer.IsShuffled(g_playlistPlayer.GetCurrentPlaylist());

            bReturn = false;
            break;
          case PLAYLIST_ISREPEAT:
            //bReturn = g_playlistPlayer.GetRepeat(g_playlistPlayer.GetCurrentPlaylist()) == PLAYLIST::REPEAT_ALL;
            bReturn = false;
            break;
          case PLAYLIST_ISREPEATONE:
            //bReturn = g_playlistPlayer.GetRepeat(g_playlistPlayer.GetCurrentPlaylist()) == PLAYLIST::REPEAT_ONE;
            bReturn = false;
            break;
          case PLAYER_HASDURATION:
            bReturn = (g_Player.Duration > 0);
            break;
          case VISUALISATION_LOCKED:
            {
/*
        CGUIMessage msg(GUI_MSG_GET_VISUALISATION, 0, 0);
        g_graphicsContext.SendMessage(msg);
        if (msg.GetLPVOID())
        {
          CVisualisation *pVis = (CVisualisation *)msg.GetLPVOID();
          bReturn = pVis.IsLocked();
        }*/
              bReturn = false;
            }
            break;
          case VISUALISATION_ENABLED:
            //bReturn = g_guiSettings.GetString("mymusic.visualisation") != "None";
            bReturn = false;
            break;
        }
      }

      // cache return value
      if (condition1 < 0)
      {
        bReturn = !bReturn;
      }
      //CacheBool(condition1, dwContextWindow, bReturn);
      return bReturn;
    }

    /// \brief Examines the multi information sent and returns true or false accordingly.
    private static bool GetMultiInfoBool(GUIInfo info, int dwContextWindow)
    {
      bool bReturn = false;
      int condition = Math.Abs(info.m_info);
      switch (condition)
      {
        case SKIN_BOOL:
          bReturn = SkinSettings.GetSkinBool(info.m_data1);
          break;
        case STRING_EQUALS:
          if (info.m_data2 != 0)
          {
            string value1 = GUIPropertyManager.Parse(SkinSettings.GetSkinString(info.m_data1)).Trim().ToLowerInvariant();
            string value2 = GUIPropertyManager.Parse(m_stringParameters[info.m_data2]).Trim().ToLowerInvariant();
            bReturn = value1.Equals(value2);
          }
          else
          {
            bReturn = (GUIPropertyManager.Parse(SkinSettings.GetSkinString(info.m_data1)).Length != 0);
          }
          break;
        case STRING_STARTS:
          if (info.m_data2 != 0)
          {
            string value1 = GUIPropertyManager.Parse(SkinSettings.GetSkinString(info.m_data1)).Trim().ToLowerInvariant();
            string value2 = GUIPropertyManager.Parse(m_stringParameters[info.m_data2]).Trim().ToLowerInvariant();
            bReturn = value1.StartsWith(value2);
          }
          else
          {
            bReturn = (GUIPropertyManager.Parse(SkinSettings.GetSkinString(info.m_data1)).Length != 0);
          }
          break;
        case STRING_CONTAINS:
          if (info.m_data2 != 0)
          {
            string value1 = GUIPropertyManager.Parse(SkinSettings.GetSkinString(info.m_data1)).Trim().ToLowerInvariant();
            string value2 = GUIPropertyManager.Parse(m_stringParameters[info.m_data2]).Trim().ToLowerInvariant();
            bReturn = value1.Contains(value2);
          }
          else
          {
            bReturn = (GUIPropertyManager.Parse(SkinSettings.GetSkinString(info.m_data1)).Length != 0);
          }
          break;
        case CONTROL_GROUP_HAS_FOCUS:
          //  GUIWindow win = GUIWindowManager.GetWindow(dwContextWindow);
          //  if (win == null) win = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
          //  if (win != null)
          //    bReturn = win.ControlGroupHasFocus(info.m_data1, info.m_data2);
          bReturn = false;

          break;
        case PLUGIN_IS_ENABLED:
          using (Settings xmlreader = new MPSettings())
          {
            bReturn = xmlreader.GetValueAsBool("plugins", info.m_stringData, false);
          }
          break;
        case CONTROL_HAS_TEXT:
          GUIWindow pWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
          if (pWindow != null)
          {
            // Note: This'll only work for unique id's
            GUILabelControl control = pWindow.GetControl(info.m_data1) as GUILabelControl;
            if (control != null)
            {
              bReturn = (control.TextWidth > 0);
            }
            else
            {
              GUIFadeLabel control2 = pWindow.GetControl(info.m_data1) as GUIFadeLabel;
              if (control2 != null)
              {
                bReturn = control2.HasText;
              }
              else
              {
                GUITextControl control3 = pWindow.GetControl(info.m_data1) as GUITextControl;
                if (control3 != null)
                {
                  bReturn = control3.HasText;
                }
                else
                {
                  GUITextScrollUpControl control4 = pWindow.GetControl(info.m_data1) as GUITextScrollUpControl;
                  if (control4 != null)
                  {
                    bReturn = control4.SubItemCount > 0;
                  }
                }
              }
            }
          }
          break;
        case CONTROL_HAS_THUMB:
          GUIWindow tWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
          if (tWindow != null)
          {
            // Note: This'll only work for unique id's
            GUIImage control = tWindow.GetControl(info.m_data1) as GUIImage;
            if (control != null)
            {
              bReturn = (control.TextureHeight > 0 && control.TextureWidth > 0);
            }
          }
          break;
        case CONTROL_IS_VISIBLE:
          GUIWindow vWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
          if (vWindow != null)
          {
            // Note: This'll only work for unique id's
            GUIControl control = vWindow.GetControl(info.m_data1);
            if (control != null)
            {
              bReturn = control.Visible;
            }
          }
          break;
        case CONTROL_HAS_FOCUS:
          GUIWindow fWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
          if (fWindow != null)
          {
            bReturn = (fWindow.GetFocusControlId() == info.m_data1);
          }
          break;
        case BUTTON_SCROLLER_HAS_ICON:
          /*
          GUIWindow pWindow = GUIWindowManager.GetWindow(dwContextWindow);
          if (null==pWindow) pWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
          if (pWindow!=null)
          {
            GUIControl *pControl = (GUIControl *)pWindow.GetControl(pWindow.GetFocusedControl());
            if (pControl && pControl.GetControlType() == GUIControl::GUIControl_BUTTONBAR)
              bReturn = ((CGUIButtonScroller *)pControl).GetActiveButtonID() == info.m_data1;
          }
           */
          return false;
        case WINDOW_NEXT:
          bReturn = (info.m_data1 == m_nextWindowID);
          break;
        case WINDOW_PREVIOUS:
          bReturn = (info.m_data1 == m_prevWindowID);
          break;
        case WINDOW_IS_VISIBLE:
          bReturn = GUIWindowManager.ActiveWindow == (info.m_data1);
          break;
        case WINDOW_IS_TOPMOST:
          bReturn = GUIWindowManager.ActiveWindow == (info.m_data1);
          break;
        case SYSTEM_HAS_ALARM:
          bReturn = false; //g_alarmClock.hasAlarm(m_stringParameters[info.m_data1]);
          break;
      }
      return (info.m_info < 0) ? !bReturn : bReturn;
    }

    private static void CacheBool(int condition, int contextWindow, bool result)
    {
      // windows have id's up to 13100 or thereabouts (ie 2^14 needed)
      // conditionals have id's up to 100000 or thereabouts (ie 2^18 needed)
      lock (typeof (GUIInfoManager))
      {
        int hash = ((contextWindow & 0x3fff) << 18) | (condition & 0x3ffff);
        m_boolCache[hash] = result;
      }
    }

    private static bool IsCached(int condition, int contextWindow, ref bool result)
    {
      // windows have id's up to 13100 or thereabouts (ie 2^14 needed)
      // conditionals have id's up to 100000 or thereabouts (ie 2^18 needed)

      lock (typeof (GUIInfoManager))
      {
        int hash = ((contextWindow & 0x3fff) << 18) | (condition & 0x3ffff);
        if (m_boolCache.ContainsKey(hash))
        {
          return m_boolCache[hash];
        }

        return false;
      }
    }

    public static void ResetCache()
    {
      lock (typeof (GUIInfoManager))
      {
        m_boolCache.Clear();
      }
    }


    /// \brief Obtains the filename of the image to show from whichever subsystem is needed
    public static string GetImage(int info, uint contextWindow)
    {
      return "";
    }

    public static string GetLabel(int info, uint contextWindow)
    {
      return "";
    }

    private static int TranslateListItem(string info)
    {
      if (info.Equals("thumb"))
      {
        return LISTITEM_THUMB;
      }
      else if (info.Equals("icon"))
      {
        return LISTITEM_ICON;
      }
      else if (info.Equals("actualicon"))
      {
        return LISTITEM_ACTUAL_ICON;
      }
      else if (info.Equals("overlay"))
      {
        return LISTITEM_OVERLAY;
      }
      else if (info.Equals("label"))
      {
        return LISTITEM_LABEL;
      }
      else if (info.Equals("label2"))
      {
        return LISTITEM_LABEL2;
      }
      else if (info.Equals("title"))
      {
        return LISTITEM_TITLE;
      }
      else if (info.Equals("tracknumber"))
      {
        return LISTITEM_TRACKNUMBER;
      }
      else if (info.Equals("artist"))
      {
        return LISTITEM_ARTIST;
      }
      else if (info.Equals("album"))
      {
        return LISTITEM_ALBUM;
      }
      else if (info.Equals("year"))
      {
        return LISTITEM_YEAR;
      }
      else if (info.Equals("genre"))
      {
        return LISTITEM_GENRE;
      }
      else if (info.Equals("director"))
      {
        return LISTITEM_DIRECTOR;
      }
      else if (info.Equals("filename"))
      {
        return LISTITEM_FILENAME;
      }
      else if (info.Equals("date"))
      {
        return LISTITEM_DATE;
      }
      else if (info.Equals("size"))
      {
        return LISTITEM_SIZE;
      }
      else if (info.Equals("rating"))
      {
        return LISTITEM_RATING;
      }
      else if (info.Equals("programcount"))
      {
        return LISTITEM_PROGRAM_COUNT;
      }
      else if (info.Equals("duration"))
      {
        return LISTITEM_DURATION;
      }
      else if (info.Equals("isselected"))
      {
        return LISTITEM_ISSELECTED;
      }
      else if (info.Equals("isplaying"))
      {
        return LISTITEM_ISPLAYING;
      }
      else if (info.Equals("plot"))
      {
        return LISTITEM_PLOT;
      }
      else if (info.Equals("plotoutline"))
      {
        return LISTITEM_PLOT_OUTLINE;
      }
      else if (info.Equals("episode"))
      {
        return LISTITEM_EPISODE;
      }
      else if (info.Equals("season"))
      {
        return LISTITEM_SEASON;
      }
      else if (info.Equals("tvshowtitle"))
      {
        return LISTITEM_TVSHOW;
      }
      else if (info.Equals("premiered"))
      {
        return LISTITEM_PREMIERED;
      }
      else if (info.Equals("comment"))
      {
        return LISTITEM_COMMENT;
      }
      else if (info.Equals("path"))
      {
        return LISTITEM_PATH;
      }
      else if (info.Equals("picturepath"))
      {
        return LISTITEM_PICTURE_PATH;
      }
      else if (info.Equals("pictureresolution"))
      {
        return LISTITEM_PICTURE_RESOLUTION;
      }
      else if (info.Equals("picturedatetime"))
      {
        return LISTITEM_PICTURE_DATETIME;
      }
      return 0;
    }
  }
}