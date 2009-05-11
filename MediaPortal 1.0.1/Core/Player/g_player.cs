#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *  Copyright (C) 2005-2009 Team MediaPortal
 *  http://www.team-mediaportal.com
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
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Playlists;
using MediaPortal.Profile;
using MediaPortal.Subtitle;
using MediaPortal.Visualization;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Cd;

namespace MediaPortal.Player
{
  public class g_Player
  {
    #region enums

    public enum MediaType
    {
      Video,
      TV,
      Radio,
      Music,
      Recording,
      Unknown
    } ;

    public enum DriveType
    {
      CD,
      DVD
    } ;

    #endregion

    #region variables

    private static MediaInfoWrapper _mediaInfo = null;
    private static int _currentStep = 0;
    private static int _currentStepIndex = -1;
    private static DateTime _seekTimer = DateTime.MinValue;
    private static IPlayer _player = null;
    private static IPlayer _prevPlayer = null;
    private static SubTitles _subs = null;
    private static bool _isInitialized = false;
    private static string _currentFilePlaying = "";
    private static MediaType _currentMedia;
    private static IPlayerFactory _factory;
    public static bool Starting = false;
    private static ArrayList _seekStepList = new ArrayList();
    private static int _seekStepTimeout;
    public static bool configLoaded = false;
    private static string[] _driveSpeedCD;
    private static string[] _driveSpeedDVD;
    private static string[] _disableCDSpeed;
    private static string[] _disableDVDSpeed;
    private static int _driveCount = 0;
    private static string _driveLetters;
    private static bool driveSpeedLoaded = false;
    private static bool driveSpeedReduced = false;
    private static bool driveSpeedControlEnabled = false;
    private static string _currentTitle = ""; //actual program metadata - usefull for tv - avoids extra DB lookups

    private static string _currentDescription = "";
    //actual program metadata - usefull for tv - avoids extra DB Lookups. 

    private static string _currentFileName = ""; //holds the actual file being played. Usefull for rtsp streams. 
    private static double[] _chapters = null;
    private static double[] _jumpPoints = null;
    private static bool _autoComSkip = false;
    private static bool _loadAutoComSkipSetting = true;

    #endregion

    #region events

    public delegate void StoppedHandler(MediaType type, int stoptime, string filename);

    public delegate void EndedHandler(MediaType type, string filename);

    public delegate void StartedHandler(MediaType type, string filename);

    public delegate void AudioTracksReadyHandler();

    public delegate void ChangedHandler(MediaType type, int stoptime, string filename);

    // when a user is already playing a file without stopping the user selects another file for playback.
    // in this case we do not receive the onstopped event.
    // so saving the resume point of a video file is not happening, since this relies on the onstopped event.
    // instead a plugin now has to listen to ChangedHandler event instead.
    public static event ChangedHandler PlayBackChanged;
    public static event StoppedHandler PlayBackStopped;
    public static event EndedHandler PlayBackEnded;
    public static event StartedHandler PlayBackStarted;
    public static event AudioTracksReadyHandler AudioTracksReady;

    #endregion

    #region Delegates

    #endregion

    #region ctor/dtor

    // singleton. Dont allow any instance of this class
    private g_Player()
    {
      _factory = new PlayerFactory();
    }

    static g_Player()
    {
    }

    public static IPlayer Player
    {
      get { return _player; }
    }

    public static IPlayerFactory Factory
    {
      get { return _factory; }
      set { _factory = value; }
    }

    public static string currentTitle
    {
      get { return _currentTitle; }
      set { _currentTitle = value; }
    }

    public static string currentFileName
    {
      get { return _currentFileName; }
      set { _currentFileName = value; }
    }

    public static string currentDescription
    {
      get { return _currentDescription; }
      set { _currentDescription = value; }
    }

    #endregion

    #region Serialisation

    /// <summary>
    /// Retrieve the CD/DVD Speed set in the config file
    /// </summary>
    public static void LoadDriveSpeed()
    {
      string speedTableCD = string.Empty;
      string speedTableDVD = string.Empty;
      string disableCD = string.Empty;
      string disableDVD = string.Empty;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        speedTableCD = xmlreader.GetValueAsString("cdspeed", "drivespeedCD", string.Empty);
        disableCD = xmlreader.GetValueAsString("cdspeed", "disableCD", string.Empty);
        speedTableDVD = xmlreader.GetValueAsString("cdspeed", "drivespeedDVD", string.Empty);
        disableDVD = xmlreader.GetValueAsString("cdspeed", "disableDVD", string.Empty);
        driveSpeedControlEnabled = xmlreader.GetValueAsBool("cdspeed", "enabled", false);
      }
      if (!driveSpeedControlEnabled)
      {
        return;
      }
      // if BASS is not the default audio engine, we need to load the CD Plugin first
      if (!BassMusicPlayer.IsDefaultMusicPlayer)
      {
        // Load the CD Plugin
        string appPath = Application.StartupPath;
        string decoderFolderPath = Path.Combine(appPath, @"musicplayer\plugins\audio decoders");
        BassRegistration.BassRegistration.Register();
        int pluginHandle = Bass.BASS_PluginLoad(decoderFolderPath + "\\basscd.dll");
      }
      // Get the number of CD/DVD drives
      _driveCount = BassCd.BASS_CD_GetDriveCount();
      StringBuilder builderDriveLetter = new StringBuilder();
      // Get Drive letters assigned
      for (int i = 0; i < _driveCount; i++)
      {
        builderDriveLetter.Append(BassCd.BASS_CD_GetInfo(i).DriveLetter);
      }
      _driveLetters = builderDriveLetter.ToString();
      if (speedTableCD == string.Empty || speedTableDVD == string.Empty)
      {
        BASS_CD_INFO cdinfo = new BASS_CD_INFO();
        StringBuilder builder = new StringBuilder();
        StringBuilder builderDisable = new StringBuilder();
        for (int i = 0; i < _driveCount; i++)
        {
          if (builder.Length != 0)
          {
            builder.Append(",");
          }
          if (builderDisable.Length != 0)
          {
            builderDisable.Append(", ");
          }
          BassCd.BASS_CD_GetInfo(i, cdinfo);
          int maxspeed = (int) (cdinfo.maxspeed / 176.4);
          builder.Append(Convert.ToInt32(maxspeed).ToString());
          builderDisable.Append("N");
        }
        speedTableCD = builder.ToString();
        speedTableDVD = builder.ToString();
        disableCD = builderDisable.ToString();
        disableDVD = builderDisable.ToString();
      }
      _driveSpeedCD = speedTableCD.Split(',');
      _driveSpeedDVD = speedTableDVD.Split(',');
      _disableCDSpeed = disableCD.Split(',');
      _disableDVDSpeed = disableDVD.Split(',');
      driveSpeedLoaded = true;
      BassMusicPlayer.ReleaseCDDrives();
    }

    /// <summary>
    /// Read the configuration file to get the skip steps
    /// </summary>
    public static ArrayList LoadSettings()
    {
      ArrayList StepArray = new ArrayList();
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strFromXml = xmlreader.GetValueAsString("movieplayer", "skipsteps",
                                                       "15,30,60,180,300,600,900,1800,3600,7200");
        if (strFromXml == string.Empty) // config after wizard run 1st
        {
          strFromXml = "15,30,60,180,300,600,900,1800,3600,7200";
          Log.Info("g_player - creating new Skip-Settings {0}", "");
        }
        else if (OldStyle(strFromXml))
        {
          strFromXml = ConvertToNewStyle(strFromXml);
        }
        foreach (string token in strFromXml.Split(new char[] { ',', ';', ' ' }))
        {
          if (token == string.Empty)
          {
            continue;
          }
          else
          {
            StepArray.Add(Convert.ToInt32(token));
          }
        }
        _seekStepList = StepArray;
        string timeout = (xmlreader.GetValueAsString("movieplayer", "skipsteptimeout", "1500"));
        if (timeout == string.Empty)
        {
          _seekStepTimeout = 1500;
        }
        else
        {
          _seekStepTimeout = Convert.ToInt16(timeout);
        }
      }
      configLoaded = true;
      return StepArray; // Sorted list of step times
    }

    private static bool OldStyle(string strSteps)
    {
      int count = 0;
      bool foundOtherThanZeroOrOne = false;
      foreach (string token in strSteps.Split(new char[] { ',', ';', ' ' }))
      {
        if (token == string.Empty)
        {
          continue;
        }
        int curInt = Convert.ToInt16(token);
        if (curInt != 0 && curInt != 1)
        {
          foundOtherThanZeroOrOne = true;
        }
        count++;
      }
      return (count == 16 && !foundOtherThanZeroOrOne);
    }

    private static string ConvertToNewStyle(string strSteps)
    {
      int count = 0;
      string newStyle = string.Empty;
      foreach (string token in strSteps.Split(new char[] { ',', ';', ' ' }))
      {
        if (token == string.Empty)
        {
          count++;
          continue;
        }
        int curInt = Convert.ToInt16(token);
        count++;
        if (curInt == 1)
        {
          switch (count)
          {
            case 1:
              newStyle += "5,";
              break;
            case 2:
              newStyle += "15,";
              break;
            case 3:
              newStyle += "30,";
              break;
            case 4:
              newStyle += "45,";
              break;
            case 5:
              newStyle += "60,";
              break;
            case 6:
              newStyle += "180,";
              break;
            case 7:
              newStyle += "300,";
              break;
            case 8:
              newStyle += "420,";
              break;
            case 9:
              newStyle += "600,";
              break;
            case 10:
              newStyle += "900,";
              break;
            case 11:
              newStyle += "1800,";
              break;
            case 12:
              newStyle += "2700,";
              break;
            case 13:
              newStyle += "3600,";
              break;
            case 14:
              newStyle += "5400,";
              break;
            case 15:
              newStyle += "7200,";
              break;
            case 16:
              newStyle += "10800,";
              break;
            default:
              break; // Do nothing
          }
        }
      }
      return (newStyle == string.Empty ? string.Empty : newStyle.Substring(0, newStyle.Length - 1));
    }

    /// <summary>
    /// Changes the speed of a drive to the value set in configuration
    /// </summary>
    /// <param name="strFile"></param>
    private static void ChangeDriveSpeed(string strFile, DriveType drivetype)
    {
      if (!driveSpeedLoaded)
      {
        LoadDriveSpeed();
      }
      if (!driveSpeedControlEnabled)
      {
        return;
      }
      try
      {
        // is the DVD inserted in a Drive for which we need to control the speed
        string rootPath = Path.GetPathRoot(strFile);
        string speed = string.Empty;
        if (rootPath != null)
        {
          if (rootPath.Length > 1)
          {
            int driveindex = _driveLetters.IndexOf(rootPath.Substring(0, 1));
            if (driveindex > -1 && driveindex < _driveSpeedCD.Length)
            {
              if (drivetype == DriveType.CD && _disableCDSpeed[driveindex] == "N")
              {
                speed = _driveSpeedCD[driveindex];
              }
              else if (drivetype == DriveType.DVD && _disableDVDSpeed[driveindex] == "N")
              {
                speed = _driveSpeedDVD[driveindex];
              }
              else
              {
                return;
              }
              BassCd.BASS_CD_SetSpeed(driveindex, Convert.ToSingle(speed));
              Log.Info("g_player: Playback Speed on Drive {0} reduced to {1}", rootPath.Substring(0, 1), speed);
              driveSpeedReduced = true;
            }
          }
        }
      }
      catch (Exception)
      {
      }
    }

    #endregion

    #region public members

    internal static void OnAudioTracksReady()
    {
      if (AudioTracksReady != null) // FIXME: the event handler might not be set if TV plugin is not installed! 
      {
        AudioTracksReady();
      }
    }

    //called when current playing file is stopped
    private static void OnChanged(string newFile)
    {
      if (newFile == null || newFile.Length == 0)
      {
        return;
      }

      if (!newFile.Equals(CurrentFile))
      {
        //yes, then raise event 
        Log.Info("g_Player.OnChanged()");
        if (PlayBackChanged != null)
        {
          PlayBackChanged(_currentMedia, (int) CurrentPosition, CurrentFile);
        }
      }
    }

    //called when current playing file is stopped
    private static void OnStopped()
    {
      //check if we're playing
      if (Playing && PlayBackStopped != null)
      {
        //yes, then raise event 
        Log.Info("g_Player.OnStopped()");
        if (PlayBackStopped != null)
        {
          PlayBackStopped(_currentMedia, (int) CurrentPosition, CurrentFile);
          _mediaInfo = null;
        }
      }
      
    }

    //called when current playing file is stopped
    private static void OnEnded()
    {
      //check if we're playing
      if (PlayBackEnded != null)
      {
        //yes, then raise event 
        Log.Info("g_Player.OnEnded()");
        RefreshRateChanger.AdaptRefreshRate();
        PlayBackEnded(_currentMedia, _currentFilePlaying);
        _mediaInfo = null;
      }      
    }

    //called when starting playing a file
    private static void OnStarted()
    {
      //check if we're playing
      if (_player == null)
      {
        return;
      }
      if (_player.Playing)
      {
        //yes, then raise event 
        _currentMedia = MediaType.Music;
        if (_player.IsTV)
        {
          _currentMedia = MediaType.TV;
          if (!_player.IsTimeShifting)
          {
            _currentMedia = MediaType.Recording;
          }
        }
        else if (_player.IsRadio)
        {
          _currentMedia = MediaType.Radio;
        }
        else if (_player.HasVideo)
        {
          if (_player.ToString() != "MediaPortal.Player.BassAudioEngine")
          {
            _currentMedia = MediaType.Video;
          }
        }
        Log.Info("g_Player.OnStarted() {0} media:{1}", _currentFilePlaying, _currentMedia.ToString());
        if (PlayBackStarted != null)
        {
          PlayBackStarted(_currentMedia, _currentFilePlaying);
        }
      }
    }

    public static void PauseGraph()
    {
      if (_player != null)
      {
        _player.PauseGraph();
      }
    }

    public static void ContinueGraph()
    {
      if (_player != null)
      {
        _player.ContinueGraph();
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void doStop(bool keepTimeShifting, bool keepExclusiveModeOn)
    {
      RefreshRateChanger.ResetRefreshRateState();

      if (driveSpeedReduced)
      {
        // Set the CD/DVD Speed back to Max Speed
        BASS_CD_INFO cdinfo = new BASS_CD_INFO();
        for (int i = 0; i < _driveCount; i++)
        {
          BassCd.BASS_CD_GetInfo(i, cdinfo);
          int maxspeed = (int) (cdinfo.maxspeed / 176.4);
          BassCd.BASS_CD_SetSpeed(i, maxspeed);
          BassCd.BASS_CD_Release(i);
        }
      }
      if (_player != null)
      {
        Log.Debug("g_Player.doStop() keepTimeShifting = {0} keepExclusiveModeOn = {1}", keepTimeShifting,
                  keepExclusiveModeOn);
        OnStopped();

        //since plugins could stop playback, we need to make sure that _player is not null.
        if (_player == null)
        {
          return;
        }

        GUIGraphicsContext.ShowBackground = true;
        if (!keepTimeShifting && !keepExclusiveModeOn)
        {
          Log.Debug("g_Player.doStop() - stop");
          _player.Stop();
        }
        else if (keepExclusiveModeOn)
        {
          Log.Debug("g_Player.doStop() - stop, keep exclusive mode on");
          _player.Stop(true);
        }
        else
        {
          Log.Debug("g_Player.doStop() - StopAndKeepTimeShifting");
          _player.StopAndKeepTimeShifting();
        }
        if (GUIGraphicsContext.form != null)
        {
          GUIGraphicsContext.form.Invalidate(true);
        }
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendThreadMessage(msg);
        GUIGraphicsContext.IsFullScreenVideo = false;
        GUIGraphicsContext.IsPlaying = false;
        GUIGraphicsContext.IsPlayingVideo = false;
        CachePlayer();
        _chapters = null;
        _jumpPoints = null;

        RefreshRateChanger.AdaptRefreshRate();
      }
    }

    public static void StopAndKeepTimeShifting()
    {
      doStop(true, false);
    }

    public static void Stop(bool keepExclusiveModeOn)
    {
      Log.Info("g_Player.Stop() - keepExclusiveModeOn = {0}", keepExclusiveModeOn);
      if (keepExclusiveModeOn)
      {
        doStop(false, true);
      }
      else
      {
        Stop();
      }
    }

    public static void Stop()
    {
      // we have to save the fullscreen status of the tv3 plugin for later use for the lastactivemodulefullscreen feature.
      bool currentmodulefullscreen = (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_TVFULLSCREEN ||
                                      GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC ||
                                      GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO ||
                                      GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
      GUIPropertyManager.SetProperty("#currentmodulefullscreenstate", Convert.ToString(currentmodulefullscreen));
      doStop(false, false);
    }

    private static void CachePlayer()
    {
      if (_player == null)
      {
        return;
      }
      if (_player.SupportsReplay)
      {
        _prevPlayer = _player;
        _player = null;
      }
      else
      {
        _player.Release();
        _player = null;
        _prevPlayer = null;
      }
    }

    public static void Pause()
    {
      if (_player != null)
      {
        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        _player.Speed = 1; //default back to 1x speed.


        _player.Pause();
        if (VMR9Util.g_vmr9 != null)
        {
          if (_player.Paused)
          {
            VMR9Util.g_vmr9.SetRepaint();
          }
        }
      }
    }

    public static bool OnAction(Action action)
    {
      if (_player != null)
      {
        if (!_player.IsDVD && _chapters != null)
        {
          switch (action.wID)
          {
            case Action.ActionType.ACTION_NEXT_CHAPTER:
              JumpToNextChapter();
              return true;
            case Action.ActionType.ACTION_PREV_CHAPTER:
              JumpToPrevChapter();
              return true;
          }
        }
        return _player.OnAction(action);
      }
      return false;
    }

    public static bool IsCDA
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return _player.IsCDA;
      }
    }

    public static bool IsDVD
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return _player.IsDVD;
      }
    }

    public static bool IsDVDMenu
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return _player.IsDVDMenu;
      }
    }

    public static bool HasChapters
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        if (_chapters == null)
        {
          return false;
        }
        return true;
      }
    }

    public static bool IsTV
    {
      get
      {
        if (RefreshRateChanger.RefreshRateChangePending &&
            RefreshRateChanger.RefreshRateChangeMediaType == RefreshRateChanger.MediaType.TV)
        {
          return true;
        }
        if (_player == null)
        {
          return false;
        }
        return _player.IsTV;
      }
    }

    public static bool IsTVRecording
    {
      get
      {
        if (RefreshRateChanger.RefreshRateChangePending &&
            RefreshRateChanger.RefreshRateChangeMediaType == RefreshRateChanger.MediaType.Recording)
        {
          return true;
        }
        if (_player == null)
        {
          return false;
        }
        return (_currentMedia == MediaType.Recording);
      }
    }

    public static bool IsTimeShifting
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return _player.IsTimeShifting;
      }
    }

    public static void Release()
    {
      if (_player != null)
      {
        _player.Stop();
        CachePlayer();
      }
    }

    public static bool PlayDVD()
    {
      return PlayDVD("");
    }

    public static bool PlayDVD(string strPath)
    {
      try
      {
        _mediaInfo = new MediaInfoWrapper(strPath);
        Starting = true;

        RefreshRateChanger.AdaptRefreshRate(strPath, RefreshRateChanger.MediaType.Video);
        if (RefreshRateChanger.RefreshRateChangePending)
        {
          TimeSpan ts = DateTime.Now - RefreshRateChanger.RefreshRateChangeExecutionTime;
          //_refreshrateChangeExecutionTime;
          if (ts.TotalSeconds > RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX)
          {
            Log.Info(
              "g_Player.PlayDVD - waited {0}s for refreshrate change, but it never took place (check your config). Proceeding with playback.",
              RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX);
            RefreshRateChanger.ResetRefreshRateState();
          }
          else
          {
            return true;
          }
        }

        // Stop the BASS engine to avoid problems with Digital Audio
        BassMusicPlayer.Player.FreeBass();
        ChangeDriveSpeed(strPath, DriveType.DVD);
        //stop playing radio
        GUIMessage msgRadio = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msgRadio);
        //stop timeshifting tv
        //GUIMessage msgTv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
        //GUIWindowManager.SendMessage(msgTv);
        Log.Info("g_Player.PlayDVD()");
        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        _subs = null;
        if (_player != null)
        {
          GUIGraphicsContext.ShowBackground = true;
          OnChanged(strPath);
          OnStopped();
          if (_player != null)
          {
            _player.Stop();
          }
          CachePlayer();
          GUIGraphicsContext.form.Invalidate(true);
          _player = null;
        }
        if (Util.Utils.PlayDVD())
        {
          return true;
        }
        _isInitialized = true;
        int iUseVMR9 = 0;
        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          iUseVMR9 = xmlreader.GetValueAsInt("dvdplayer", "vmr9", 0);
        }
        _player = new DVDPlayer9();
        _player = CachePreviousPlayer(_player);
        bool bResult = _player.Play(strPath);
        if (!bResult)
        {
          Log.Error("g_Player.PlayDVD():failed to play");
          _player.Release();
          _player = null;
          _subs = null;
          GC.Collect();
          GC.Collect();
          GC.Collect();
          Log.Info("dvdplayer:bla");
        }
        else if (_player.Playing)
        {
          _isInitialized = false;
          if (!_player.IsTV)
          {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
          _currentFilePlaying = _player.CurrentFile;
          OnStarted();
          return true;
        }
        Log.Info("dvdplayer:sendmsg");
        //show dialog:unable to play dvd,
        GUIWindowManager.ShowWarning(722, 723, -1);
      }
      finally
      {
        Starting = false;
      }
      return false;
    }

    public static bool PlayAudioStream(string strURL)
    {
      return PlayAudioStream(strURL, false);
    }

    // GUIMusicVideos and GUITRailer called this function, altough they want to play a Video.
    // When BASS is enabled this resulted in no Picture being shown. they should now indicate that a Video is to be played.
    public static bool PlayAudioStream(string strURL, bool isMusicVideo)
    {
      try
      {
        _mediaInfo = null;
        string strAudioPlayer = string.Empty;
        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "Internal dshow player");
        }
        Starting = true;
        //stop radio
        GUIMessage msgRadio = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msgRadio);
        PlayListType currentList = PlayListPlayer.SingletonPlayer.CurrentPlaylistType;
        if (isMusicVideo)
        {
          // Clear any temp. playlists before starting playback
          if (currentList == PlayListType.PLAYLIST_MUSIC_TEMP || currentList == PlayListType.PLAYLIST_VIDEO_TEMP)
          {
            PlayListPlayer.SingletonPlayer.GetPlaylist(currentList).Clear();
            PlayListPlayer.SingletonPlayer.Reset();
          }
        }
        //stop timeshifting tv
        //GUIMessage msgTv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
        //GUIWindowManager.SendMessage(msgTv);
        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        _isInitialized = true;
        _subs = null;
        Log.Info("g_Player.PlayAudioStream({0})", strURL);
        if (_player != null)
        {
          GUIGraphicsContext.ShowBackground = true;
          OnChanged(strURL);
          OnStopped();
          if (_player != null)
          {
            _player.Stop();
          }
          CachePlayer();
          GUIGraphicsContext.form.Invalidate(true);
          _player = null;
        }

        if (strAudioPlayer == "BASS engine" && !isMusicVideo)
        {
          if (BassMusicPlayer.BassFreed)
          {
            BassMusicPlayer.Player.InitBass();
          }
          _player = BassMusicPlayer.Player;
        }
        else
        {
          _player = new AudioPlayerWMP9();
        }
        _player = CachePreviousPlayer(_player);
        bool bResult = _player.Play(strURL);
        if (!bResult)
        {
          Log.Info("player:ended");
          _player.Release();
          _player = null;
          _subs = null;
          GC.Collect();
          GC.Collect();
          GC.Collect();
        }
        else if (_player.Playing)
        {
          _currentFilePlaying = _player.CurrentFile;
          OnStarted();
        }
        _isInitialized = false;
        if (!bResult)
        {
          UnableToPlay(strURL, MediaType.Unknown);
        }
        return bResult;
      }
      finally
      {
        Starting = false;
      }
    }

    //Added by juvinious 19/02/2005
    public static bool PlayVideoStream(string strURL)
    {
      return PlayVideoStream(strURL, "");
    }

    public static bool PlayVideoStream(string strURL, string streamName)
    {
      try
      {
        _mediaInfo = null;
        Starting = true;
        //stop radio
        GUIMessage msgRadio = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msgRadio);
        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        if (string.IsNullOrEmpty(strURL))
        {
          UnableToPlay(strURL, MediaType.Unknown);
          return false;
        }

        _isInitialized = true;
        _subs = null;
        Log.Info("g_Player.PlayVideoStream({0})", strURL);
        if (_player != null)
        {
          GUIGraphicsContext.ShowBackground = true;
          OnChanged(strURL);
          OnStopped();
          if (_player != null)
          {
            _player.Stop();
          }
          CachePlayer();
          GUIGraphicsContext.form.Invalidate(true);
          _player = null;
          GC.Collect();
          GC.Collect();
          GC.Collect();
          GC.Collect();
        }

        //int iUseVMR9inMYMovies = 0;
        //using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        //{
        //  iUseVMR9inMYMovies = xmlreader.GetValueAsInt("movieplayer", "vmr9", 1);
        //}
        //if (iUseVMR9inMYMovies == 0)
        //  _player = new Player.VideoPlayerVMR7();
        //else
        _player = new VideoPlayerVMR9();
        _player = CachePreviousPlayer(_player);
        bool isPlaybackPossible;
        if (streamName != null)
        {
          if (streamName != "")
          {
            isPlaybackPossible = _player.PlayStream(strURL, streamName);
          }
          else
          {
            isPlaybackPossible = _player.Play(strURL);
          }
        }
        else
        {
          isPlaybackPossible = _player.Play(strURL);
        }
        if (!isPlaybackPossible)
        {
          Log.Info("player:ended");
          _player.Release();
          _player = null;
          _subs = null;
          GC.Collect();
          GC.Collect();
          GC.Collect();
          //2nd try
          _player = new VideoPlayerVMR9();
          isPlaybackPossible = _player.Play(strURL);
          if (!isPlaybackPossible)
          {
            Log.Info("player2:ended");
            _player.Release();
            _player = null;
            _subs = null;
            GC.Collect();
            GC.Collect();
            GC.Collect();
          }
        }
        else if (_player.Playing)
        {
          _currentFilePlaying = _player.CurrentFile;
          OnStarted();
        }
        _isInitialized = false;
        if (!isPlaybackPossible)
        {
          UnableToPlay(strURL, MediaType.Unknown);
        }
        return isPlaybackPossible;
      }
      finally
      {
        Starting = false;
      }
    }

    private static IPlayer CachePreviousPlayer(IPlayer newPlayer)
    {
      IPlayer player = newPlayer;
      if (newPlayer != null)
      {
        if (_prevPlayer != null)
        {
          if (_prevPlayer.GetType() == newPlayer.GetType())
          {
            if (_prevPlayer.SupportsReplay)
            {
              player = _prevPlayer;
              _prevPlayer = null;
            }
          }
        }
        if (_prevPlayer != null)
        {
          _prevPlayer.Release();
          _prevPlayer = null;
        }
      }
      return player;
    }

    public static bool Play(string strFile, MediaType type)
    {
      try
      {
        _mediaInfo = new MediaInfoWrapper(strFile);

        Starting = true;
        ChangeDriveSpeed(strFile, DriveType.CD);
        //stop radio
        if (!Util.Utils.IsLiveRadio(strFile))
        {
          GUIMessage msgRadio = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(msgRadio);
        }
        if (!Util.Utils.IsLiveTv(strFile) && !Util.Utils.IsLiveRadio(strFile))
        {
          //file is not a live tv file
          //so tell recorder to stop timeshifting live-tv
          //Log.Info("player: file is not live tv, so stop timeshifting:{0}", strFile);
          //GUIMessage msgTv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
          //GUIWindowManager.SendMessage(msgTv);
        }

        int t1 = (int) type;
        RefreshRateChanger.MediaType t2 = (RefreshRateChanger.MediaType) t1;

        if (Util.Utils.IsLiveTv(strFile) || Util.Utils.IsRTSP(strFile))
        {
          // liveTV needs the refreshrate change done here.
          RefreshRateChanger.AdaptRefreshRate(strFile, t2);

          if (RefreshRateChanger.RefreshRateChangePending)
          {
            TimeSpan ts = DateTime.Now - RefreshRateChanger.RefreshRateChangeExecutionTime;
            if (ts.TotalSeconds > RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX)
            {
              Log.Info(
                "g_Player.Play - waited {0}s for refreshrate change, but it never took place (check your config). Proceeding with playback.",
                RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX);
              RefreshRateChanger.ResetRefreshRateState();
            }
            else
            {
              return true;
            }
          }
        }        

        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        if (string.IsNullOrEmpty(strFile))
        {
          UnableToPlay(strFile, type);
          return false;
        }
        _isInitialized = true;
        _subs = null;
        Log.Info("g_Player.Play({0} {1})", strFile, type);
        if (_player != null)
        {
          GUIGraphicsContext.ShowBackground = true;
          OnChanged(strFile);
          OnStopped();
          if (_player != null)
          {
            _player.Stop();
          }
          CachePlayer();
          _player = null;
          GC.Collect();
          GC.Collect();
          GC.Collect();
          GC.Collect();
          //?? ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.NETDEVFX.v20.de/cpref2/html/M_System_GC_Collect_1_804c5d7d.htm
        }

        if (!Util.Utils.IsAVStream(strFile) && Util.Utils.IsVideo(strFile))
        {
          RefreshRateChanger.AdaptRefreshRate(strFile, t2);

          if (RefreshRateChanger.RefreshRateChangePending)
          {
            TimeSpan ts = DateTime.Now - RefreshRateChanger.RefreshRateChangeExecutionTime;
            if (ts.TotalSeconds > RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX)
            {
              Log.Info(
                "g_Player.Play - waited {0}s for refreshrate change, but it never took place (check your config). Proceeding with playback.",
                RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX);
              RefreshRateChanger.ResetRefreshRateState();
            }
            else
            {
              return true;
            }
          }

          if (Util.Utils.PlayMovie(strFile))
          {
            _isInitialized = false;
            UnableToPlay(strFile, type);
            return false;
          }
          string extension = Path.GetExtension(strFile).ToLower();
          if (extension == ".ifo" || extension == ".vob")
          {
            int iUseVMR9 = 0;
            using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
              iUseVMR9 = xmlreader.GetValueAsInt("dvdplayer", "vmr9", 0);
            }
            _player = new DVDPlayer9();
            _player = CachePreviousPlayer(_player);
            bool _isPlaybackPossible = _player.Play(strFile);
            if (!_isPlaybackPossible)
            {
              Log.Info("player:ended");
              _player.Release();
              _player = null;
              _subs = null;
              GC.Collect();
              GC.Collect();
              GC.Collect();
            }
            else if (_player.Playing)
            {
              _currentFilePlaying = _player.CurrentFile;
              OnStarted();
              _isInitialized = false;
              GUIGraphicsContext.IsFullScreenVideo = true;
              GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            }
            _isInitialized = false;
            if (!_isPlaybackPossible)
            {
              UnableToPlay(strFile, type);
            }
            return _isPlaybackPossible;
          }
        }
        _player = _factory.Create(strFile, type);
        if (_player != null)
        {
          LoadChapters(strFile);
          _player = CachePreviousPlayer(_player);
          bool bResult = _player.Play(strFile);
          if (!bResult)
          {
            Log.Info("player:ended");
            if (_player != null)
            {
              _player.Release();
            }
            _player = null;
            _subs = null;
            GC.Collect();
            GC.Collect();
            GC.Collect();
          }
          else if (_player.Playing)
          {
            _currentFilePlaying = _player.CurrentFile;
            if (_chapters == null)
            {
              _chapters = _player.Chapters;
            }
            OnStarted();
          }
          _isInitialized = false;
          if (!bResult)
          {
            UnableToPlay(strFile, type);
          }
          return bResult;
        }
        _isInitialized = false;
      }
      finally
      {
        Starting = false;
      }
      UnableToPlay(strFile, type);
      return false;
    }

    public static bool Play(string strFile)
    {
      try
      {
        // when we are watching TV and suddenly decides to watch a video, we want to make sure that the TV is stopped.
        /*if (g_Player.IsTimeShifting && g_Player.IsTV)
        {
          Log.Info("player: Stopping timeshifting.");
          g_Player.Stop();
        }
        */

        _mediaInfo = new MediaInfoWrapper(strFile);

        Starting = true;
        ChangeDriveSpeed(strFile, DriveType.CD);
        //stop radio
        if (!Util.Utils.UsingTvServer)
        {
          if (!Util.Utils.IsLiveRadio(strFile))
          {
            GUIMessage msgRadio = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
            GUIWindowManager.SendMessage(msgRadio);
          }
          if (!Util.Utils.IsLiveTv(strFile) && !Util.Utils.IsLiveRadio(strFile))
          {
            //file is not a live tv file
            //so tell recorder to stop timeshifting live-tv
            //Log.Info("player: file is not live tv, so stop timeshifting:{0}", strFile);
            //GUIMessage msgTv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
            //GUIWindowManager.SendMessage(msgTv);
          }
        }

        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        if (string.IsNullOrEmpty(strFile))
        {
          UnableToPlay(strFile, MediaType.Unknown);
          return false;
        }
        _isInitialized = true;
        _subs = null;
        Log.Debug("g_Player.Play({0})", strFile);
        if (_player != null)
        {
          GUIGraphicsContext.ShowBackground = true;
          OnChanged(strFile);
          OnStopped();

          //SV 
          // If we're using the internal music player and cross-fading is enabled
          // we don't want a hard stop here as it will break cross-fading
          //_player.Stop();
          //CachePlayer();
          //_player = null;
          if (!_player.IsExternal)
          {
            GC.Collect();
            GC.Collect();
            GC.Collect();
            GC.Collect();
          }
          //?? ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.NETDEVFX.v20.de/cpref2/html/M_System_GC_Collect_1_804c5d7d.htm
          bool doStop = true;
          if (Util.Utils.IsAudio(strFile))
          {
            if (BassMusicPlayer.IsDefaultMusicPlayer && BassMusicPlayer.Player.Playing)
            {
              doStop = !BassMusicPlayer.Player.CrossFadingEnabled;
            }
          }
          if (doStop)
          {
            if (_player != null)
            {
              _player.Stop();
            }

            bool isExternal = _player.IsExternal;
            CachePlayer();
            _player = null;
            if (!isExternal)
            {
              GC.Collect();
              GC.Collect();
              GC.Collect();
              GC.Collect();
            }
          }
        }

        if (!Util.Utils.IsAVStream(strFile) && Util.Utils.IsVideo(strFile))
        {
          // Free BASS to avoid problems with Digital Audio, when watching movies
          if (BassMusicPlayer.IsDefaultMusicPlayer)
          {
            BassMusicPlayer.Player.FreeBass();
          }

          RefreshRateChanger.AdaptRefreshRate(strFile, RefreshRateChanger.MediaType.Unknown);
          if (RefreshRateChanger.RefreshRateChangePending)
          {
            TimeSpan ts = DateTime.Now - RefreshRateChanger.RefreshRateChangeExecutionTime;
            if (ts.TotalSeconds > RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX)
            {
              Log.Warn(
                "g_Player.Play - waited {0}s for refreshrate change, but it never took place (check your config). Proceeding with playback.",
                RefreshRateChanger.WAIT_FOR_REFRESHRATE_RESET_MAX);
              RefreshRateChanger.ResetRefreshRateState();
            }
            else
            {
              return true;
            }
          }
          bool bInternal = true;
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            bInternal = xmlreader.GetValueAsBool("movieplayer", "internal", true);
          }
          if (Util.Utils.PlayMovie(strFile)) // external player used
          {            
            return true;
          }
          else if(!bInternal) // external player error
          {
            _isInitialized = false;
            UnableToPlay(strFile, MediaType.Unknown);
            return false;
          }
          string extension = Path.GetExtension(strFile).ToLower();
          if (extension == ".ifo" || extension == ".vob")
          {
            int iUseVMR9 = 0;
            using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
              iUseVMR9 = xmlreader.GetValueAsInt("dvdplayer", "vmr9", 0);
            }
            _player = new DVDPlayer9();
            _player = CachePreviousPlayer(_player);
            bool _isPlaybackPossible = _player.Play(strFile);
            if (!_isPlaybackPossible)
            {
              Log.Debug("g_player: Player returned");
              _player.Release();
              _player = null;
              _subs = null;
              GC.Collect();
              GC.Collect();
              GC.Collect();
            }
            else if (_player.Playing)
            {
              _currentFilePlaying = _player.CurrentFile;
              OnStarted();
              _isInitialized = false;
              GUIGraphicsContext.IsFullScreenVideo = true;
              GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            }
            _isInitialized = false;
            if (!_isPlaybackPossible)
            {
              UnableToPlay(strFile, MediaType.Unknown);
            }
            return _isPlaybackPossible;
          }
        }

        _player = _factory.Create(strFile);
        if (_player != null)
        {
          LoadChapters(strFile);
          _player = CachePreviousPlayer(_player);
          bool bResult = _player.Play(strFile);
          if (!bResult)
          {
            Log.Debug("g_player: Player returned");
            _player.Release();
            _player = null;
            _subs = null;
            GC.Collect();
            GC.Collect();
            GC.Collect();
          }
          else if (_player.Playing)
          {
            if (_chapters == null)
            {
              _chapters = _player.Chapters;
            }
            _currentFilePlaying = _player.CurrentFile;
            OnStarted();
          }
          _isInitialized = false;
          if (!bResult)
          {
            UnableToPlay(strFile, MediaType.Unknown);
          }
          return bResult;
        }
        _isInitialized = false;
      }
      finally
      {
        Starting = false;
      }
      UnableToPlay(strFile, MediaType.Unknown);
      return false;
    }

    public static bool IsExternalPlayer
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return _player.IsExternal;
      }
    }

    public static bool IsRadio
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return (_currentMedia == MediaType.Radio);
      }
    }

    public static bool IsMusic
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return (_currentMedia == MediaType.Music);
      }
    }

    public static bool Playing
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        if (_isInitialized)
        {
          return false;
        }
        bool bResult = _player.Playing;
        return bResult;
      }
    }

    public static int PlaybackType
    {
      get
      {
        if (_player == null)
        {
          return -1;
        }
        return _player.PlaybackType;
      }
    }

    public static bool Paused
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return _player.Paused;
      }
    }

    public static bool Stopped
    {
      get
      {
        if (_isInitialized)
        {
          return false;
        }
        if (_player == null)
        {
          return false;
        }
        bool bResult = _player.Stopped;
        return bResult;
      }
    }

    public static int Speed
    {
      get
      {
        if (_player == null)
        {
          return 1;
        }
        return _player.Speed;
      }
      set
      {
        if (_player == null)
        {
          return;
        }
        _player.Speed = value;
        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
      }
    }

    public static string CurrentFile
    {
      get
      {
        if (_player == null)
        {
          return "";
        }
        return _player.CurrentFile;
      }
    }

    public static int Volume
    {
      get
      {
        if (_player == null)
        {
          return -1;
        }
        return _player.Volume;
      }
      set
      {
        if (_player != null)
        {
          _player.Volume = value;
        }
      }
    }

    public static Geometry.Type ARType
    {
      get { return GUIGraphicsContext.ARType; }
      set
      {
        if (_player != null)
        {
          _player.ARType = value;
        }
      }
    }

    public static int PositionX
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.PositionX;
      }
      set
      {
        if (_player != null)
        {
          _player.PositionX = value;
        }
      }
    }

    public static int PositionY
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.PositionY;
      }
      set
      {
        if (_player != null)
        {
          _player.PositionY = value;
        }
      }
    }

    public static int RenderWidth
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.RenderWidth;
      }
      set
      {
        if (_player != null)
        {
          _player.RenderWidth = value;
        }
      }
    }

    public static bool Visible
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return _player.Visible;
      }
      set
      {
        if (_player != null)
        {
          _player.Visible = value;
        }
      }
    }

    public static int RenderHeight
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.RenderHeight;
      }
      set
      {
        if (_player != null)
        {
          _player.RenderHeight = value;
        }
      }
    }

    public static double Duration
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.Duration;
      }
    }

    public static double CurrentPosition
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.CurrentPosition;
      }
    }

    public static double StreamPosition
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.StreamPosition;
      }
    }

    public static double ContentStart
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.ContentStart;
      }
    }

    public static bool FullScreen
    {
      get
      {
        if (_player == null)
        {
          return GUIGraphicsContext.IsFullScreenVideo;
        }
        return _player.FullScreen;
      }
      set
      {
        if (_player != null)
        {
          _player.FullScreen = value;
        }
      }
    }

    public static int Width
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.Width;
      }
    }

    public static int Height
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.Height;
      }
    }

    public static void SeekRelative(double dTime)
    {
      if (_player == null)
      {
        return;
      }
      _player.SeekRelative(dTime);
      _currentStep = 0;
      _currentStepIndex = -1;
      _seekTimer = DateTime.MinValue;
      GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msgUpdate);
    }

    public static void StepNow()
    {
      if (_currentStep != 0 && _player != null)
      {
        if (_currentStep < 0 || (_player.CurrentPosition+4 < _player.Duration) || !IsTV)
        {
          double dTime = (int)_currentStep + _player.CurrentPosition;
          Log.Debug("g_Player.StepNow() - Preparing to seek to {0}:{1}", _player.CurrentPosition, _player.Duration);
          if (!IsTV && (dTime > _player.Duration)) dTime = _player.Duration - 5;  
          if (IsTV && (dTime+3 > _player.Duration)) dTime = _player.Duration - 3 ; // Margin for live Tv
          if (dTime < 0) dTime = 0d;

          Log.Debug("g_Player.StepNow() - Preparing to seek to {0}:{1}:{2} isTv {3}", (int)(dTime / 3600d), (int)((dTime % 3600d) / 60d), (int)(dTime % 60d),IsTV);
          _player.SeekAbsolute(dTime);
          Speed = Speed;
          GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
          GUIGraphicsContext.SendMessage(msgUpdate);
        }
      }
      _currentStep = 0;
      _currentStepIndex = -1;
      _seekTimer = DateTime.MinValue;
    }

    /// <summary>
    /// This function returns the localized time units for "Step" (seconds) in human readable format.
    /// </summary>
    /// <param name="Step"></param>
    /// <returns></returns>
    public static string GetSingleStep(int Step)
    {
      if (Step >= 0)
      {
        if (Step >= 3600)
        {
          // check for 'full' hours
          if ((Convert.ToSingle(Step) / 3600) > 1 && (Convert.ToSingle(Step) / 3600) != 2 &&
              (Convert.ToSingle(Step) / 3600) != 3)
          {
            return "+ " + Convert.ToString(Step / 60) + " " + GUILocalizeStrings.Get(2998); // "min"
          }
          else
          {
            return "+ " + Convert.ToString(Step / 3600) + " " + GUILocalizeStrings.Get(2997); // "hrs"
          }
        }
        else if (Step >= 60)
        {
          return "+ " + Convert.ToString(Step / 60) + " " + GUILocalizeStrings.Get(2998); // "min"
        }
        else
        {
          return "+ " + Convert.ToString(Step) + " " + GUILocalizeStrings.Get(2999); // "sec"
        }
      }
      else // back = negative
      {
        if (Step <= -3600)
        {
          if ((Convert.ToSingle(Step) / 3600) < -1 && (Convert.ToSingle(Step) / 3600) != -2 &&
              (Convert.ToSingle(Step) / 3600) != -3)
          {
            return "- " + Convert.ToString(Math.Abs(Step / 60)) + " " + GUILocalizeStrings.Get(2998); // "min"
          }
          else
          {
            return "- " + Convert.ToString(Math.Abs(Step / 3600)) + " " + GUILocalizeStrings.Get(2997); // "hrs"
          }
        }
        else if (Step <= -60)
        {
          return "- " + Convert.ToString(Math.Abs(Step / 60)) + " " + GUILocalizeStrings.Get(2998); // "min"
        }
        else
        {
          return "- " + Convert.ToString(Math.Abs(Step)) + " " + GUILocalizeStrings.Get(2999); // "sec"
        }
      }
    }

    public static string GetStepDescription()
    {
      if (_player == null)
      {
        return "";
      }
      int m_iTimeToStep = (int) _currentStep;
      if (m_iTimeToStep == 0)
      {
        return "";
      }
      _player.Process();
      if (_player.CurrentPosition + m_iTimeToStep <= 0)
      {
        return GUILocalizeStrings.Get(773); // "START"
      }
      if (_player.CurrentPosition + m_iTimeToStep >= _player.Duration)
      {
        return GUILocalizeStrings.Get(774); // "END"
      }
      return GetSingleStep(_currentStep);
    }

    public static int GetSeekStep(out bool bStart, out bool bEnd)
    {
      bStart = false;
      bEnd = false;
      if (_player == null)
      {
        return 0;
      }
      int m_iTimeToStep = (int) _currentStep;
      if (_player.CurrentPosition + m_iTimeToStep <= 0)
      {
        bStart = true; //start
      }
      if (_player.CurrentPosition + m_iTimeToStep >= _player.Duration)
      {
        bEnd = true;
      }
      return m_iTimeToStep;
    }

    public static void SeekStep(bool bFF)
    {
      if (!configLoaded)
      {
        _seekStepList = LoadSettings();
        Log.Info("g_Player loading seekstep config {0}", ""); // Convert.ToString(_seekStepList[0]));
      }
      if (bFF)
      {
        if (_currentStep < 0)
        {
          _currentStepIndex--; // E.g. from -30 to -15 
          if (_currentStepIndex == -1)
          {
            _currentStep = 0; // Reached middle, no stepping
          }
          else
          {
            _currentStep = -1 * Convert.ToInt32(_seekStepList[_currentStepIndex]);
          }
        }
        else
        {
          _currentStepIndex++; // E.g. from 15 to 30
          if (_currentStepIndex >= _seekStepList.Count)
          {
            _currentStepIndex--; // Reached maximum step, don't change _currentStep
          }
          else
          {
            _currentStep = Convert.ToInt32(_seekStepList[_currentStepIndex]);
          }
        }
      }
      else
      {
        if (_currentStep <= 0)
        {
          _currentStepIndex++; // E.g. from -15 to -30
          if (_currentStepIndex >= _seekStepList.Count)
          {
            _currentStepIndex--; // Reached maximum step, don't change _currentStep
          }
          else
          {
            _currentStep = -1 * Convert.ToInt32(_seekStepList[_currentStepIndex]);
          }
        }
        else
        {
          _currentStepIndex--; // E.g. from 30 to 15
          if (_currentStepIndex == -1)
          {
            _currentStep = 0; // Reached middle, no stepping
          }
          else
          {
            _currentStep = Convert.ToInt32(_seekStepList[_currentStepIndex]);
          }
        }
      }
      _seekTimer = DateTime.Now;
    }

    public static void SeekRelativePercentage(int iPercentage)
    {
      if (_player == null)
      {
        return;
      }
      _player.SeekRelativePercentage(iPercentage);
      GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msgUpdate);
      _currentStep = 0;
      _currentStepIndex = -1;
      _seekTimer = DateTime.MinValue;
    }

    public static void SeekAbsolute(double dTime)
    {
      if (_player == null)
      {
        return;
      }
      Log.Debug("g_Player.SeekAbsolute() - Preparing to seek to {0}:{1}:{2}", (int) (dTime / 3600d),
                (int) ((dTime % 3600d) / 60d), (int) (dTime % 60d));
      _player.SeekAbsolute(dTime);
      GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msgUpdate);
      _currentStep = 0;
      _currentStepIndex = -1;
      _seekTimer = DateTime.MinValue;
      Speed = Speed;
    }

    public static void SeekAsolutePercentage(int iPercentage)
    {
      if (_player == null)
      {
        return;
      }
      _player.SeekAsolutePercentage(iPercentage);
      GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msgUpdate);
      _currentStep = 0;
      _currentStepIndex = -1;
      _seekTimer = DateTime.MinValue;
    }

    public static bool HasVideo
    {
      get
      {
        if (RefreshRateChanger.RefreshRateChangePending)
        {
          return true;
        }
        if (_player == null)
        {
          return false;
        }
        return _player.HasVideo;
      }
    }

    public static bool IsVideo
    {
      get
      {
        if (RefreshRateChanger.RefreshRateChangePending &&
            RefreshRateChanger.RefreshRateChangeMediaType == RefreshRateChanger.MediaType.Video)
        {
          return true;
        }
        if (_player == null)
        {
          return false;
        }
        if (_currentMedia == MediaType.Video)
        {
          return true;
        }
        return false;
      }
    }

    public static bool HasSubs
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return (_subs != null);
      }
    }

    public static void RenderSubtitles()
    {
      if (_player == null)
      {
        return;
      }
      if (_subs == null)
      {
        return;
      }
      if (HasSubs)
      {
        _subs.Render(_player.CurrentPosition);
      }
    }

    public static void WndProc(ref Message m)
    {
      if (_player == null)
      {
        return;
      }
      _player.WndProc(ref m);
    }

    public static void Process()
    {
      if (GUIGraphicsContext.Vmr9Active && VMR9Util.g_vmr9 != null && !GUIGraphicsContext.InVmr9Render)
      {
        VMR9Util.g_vmr9.Process();
        VMR9Util.g_vmr9.Repaint();
      }
      if (_player == null)
      {
        return;
      }
      _player.Process();
      if (_player.Initializing)
      {
        return;
      }
      if (!_player.Playing)
      {
        Log.Info("g_Player.Process() player stopped...");
        if (_player.Ended)
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendThreadMessage(msg);
          OnEnded();
          return;
        }
        Stop();
      }
      else
      {
        if (_currentStep != 0)
        {
          TimeSpan ts = DateTime.Now - _seekTimer;
          if (ts.TotalMilliseconds > _seekStepTimeout)
          {
            StepNow();
          }
        }
        else if (_autoComSkip && _jumpPoints != null && _player.Speed == 1)
        {
          double currentPos = _player.CurrentPosition;
          foreach (double jumpFrom in _jumpPoints)
          {
            if (jumpFrom != 0 && currentPos <= jumpFrom + 1.0 && currentPos >= jumpFrom - 0.1)
            {
              Log.Debug("g_Player.Process() - Current Position: {0}, JumpPoint: {1}", currentPos, jumpFrom);

              JumpToNextChapter();
              break;
            }
          }
        }
      }
    }

    public static VideoStreamFormat GetVideoFormat()
    {
      if (_player == null)
      {
        return new VideoStreamFormat();
      }
      else
      {
        return _player.GetVideoFormat();
      }
    }

    public static eAudioDualMonoMode GetAudioDualMonoMode()
    {
      if (_player == null)
      {
        return eAudioDualMonoMode.UNSUPPORTED;
      }
      else
      {
        return _player.GetAudioDualMonoMode();
      }
    }

    public static bool SetAudioDualMonoMode(eAudioDualMonoMode mode)
    {
      if (_player == null)
      {
        return false;
      }
      else
      {
        return _player.SetAudioDualMonoMode(mode);
      }
    }

    public static void OnZapping(int info)
    {
      if (_player == null)
      {
        return ;
      }
      else
      {
        _player.OnZapping(info);
        return;
      }
    }


    #region subtitle/audio stream selection

    /// <summary>
    /// Property which returns the total number of audio streams available
    /// </summary>
    public static int AudioStreams
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.AudioStreams;
      }
    }

    /// <summary>
    /// Property to get/set the current audio stream
    /// </summary>
    public static int CurrentAudioStream
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.CurrentAudioStream;
      }
      set
      {
        if (_player != null)
        {
          _player.CurrentAudioStream = value;
        }
      }
    }

    /// <summary>
    /// Property to get the name for an audio stream
    /// </summary>
    public static string AudioLanguage(int iStream)
    {
      if (_player == null)
      {
        return Strings.Unknown;
      }

      string stream = _player.AudioLanguage(iStream);
      return Util.Utils.TranslateLanguageString(stream);
    }

    /// <summary>
    /// Property to get the type of an audio stream
    /// </summary>
    public static string AudioType(int iStream)
    {
      if (_player == null)
      {
        return Strings.Unknown;
      }

      string stream = _player.AudioType(iStream);
      return stream;
    }

    /// <summary>
    /// Property to get the total number of subtitle streams
    /// </summary>
    public static int SubtitleStreams
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.SubtitleStreams;
      }
    }

    /// <summary>
    /// Property to get/set the current subtitle stream
    /// </summary>
    public static int CurrentSubtitleStream
    {
      get
      {
        if (_player == null)
        {
          return 0;
        }
        return _player.CurrentSubtitleStream;
      }
      set
      {
        if (_player != null)
        {
          _player.CurrentSubtitleStream = value;
        }
      }
    }

    /// <summary>
    /// Property to get/set the name for a subtitle stream
    /// </summary>
    public static string SubtitleLanguage(int iStream)
    {
      if (_player == null)
      {
        return Strings.Unknown;
      }

      string stream = _player.SubtitleLanguage(iStream);
      return Util.Utils.TranslateLanguageString(stream);
    }

    #endregion

    public static bool EnableSubtitle
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return _player.EnableSubtitle;
      }
      set
      {
        if (_player == null)
        {
          return;
        }
        _player.EnableSubtitle = value;
      }
    }

    public static void SetVideoWindow()
    {
      if (_player == null)
      {
        return;
      }
      _player.SetVideoWindow();
    }

    public static void Init()
    {
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(OnVideoWindowChanged);
      GUIGraphicsContext.OnGammaContrastBrightnessChanged +=
        new VideoGammaContrastBrightnessHandler(OnGammaContrastBrightnessChanged);
    }

    private static void OnGammaContrastBrightnessChanged()
    {
      if (!Playing)
      {
        return;
      }
      if (!HasVideo)
      {
        return;
      }
      if (_player == null)
      {
        return;
      }
      _player.Contrast = GUIGraphicsContext.Contrast;
      _player.Brightness = GUIGraphicsContext.Brightness;
      _player.Gamma = GUIGraphicsContext.Gamma;
    }

    private static void OnVideoWindowChanged()
    {
      if (!Playing)
      {
        return;
      }
      if (!HasVideo)
      {
        return;
      }
      FullScreen = GUIGraphicsContext.IsFullScreenVideo;
      ARType = GUIGraphicsContext.ARType;
      if (!FullScreen)
      {
        PositionX = GUIGraphicsContext.VideoWindow.Left;
        PositionY = GUIGraphicsContext.VideoWindow.Top;
        RenderWidth = GUIGraphicsContext.VideoWindow.Width;
        RenderHeight = GUIGraphicsContext.VideoWindow.Height;
      }
      bool inTV = false;
      int windowId = GUIWindowManager.ActiveWindow;
      if (windowId == (int) GUIWindow.Window.WINDOW_TV ||
          windowId == (int) GUIWindow.Window.WINDOW_TVGUIDE ||
          windowId == (int) GUIWindow.Window.WINDOW_SEARCHTV ||
          windowId == (int) GUIWindow.Window.WINDOW_SCHEDULER ||
          windowId == (int) GUIWindow.Window.WINDOW_RECORDEDTV)
      {
        inTV = true;
      }
      Visible = (FullScreen || GUIGraphicsContext.Overlay ||
                 windowId == (int) GUIWindow.Window.WINDOW_SCHEDULER || inTV);
      SetVideoWindow();
    }

    /// <summary>
    /// returns video window rectangle
    /// </summary>
    public static Rectangle VideoWindow
    {
      get
      {
        if (_player == null)
        {
          return new Rectangle(0, 0, 0, 0);
        }
        return _player.VideoWindow;
      }
    }

    /// <summary>
    /// returns video source rectangle displayed
    /// </summary>
    public static Rectangle SourceWindow
    {
      get
      {
        if (_player == null)
        {
          return new Rectangle(0, 0, 0, 0);
        }
        return _player.SourceWindow;
      }
    }

    public static int GetHDC()
    {
      if (_player == null)
      {
        return 0;
      }
      return _player.GetHDC();
    }

    public static void ReleaseHDC(int HDC)
    {
      if (_player == null)
      {
        return;
      }
      _player.ReleaseHDC(HDC);
    }

    public static bool CanSeek
    {
      get
      {
        if (_player == null)
        {
          return false;
        }
        return (_player.CanSeek() && !_player.IsDVDMenu);
      }
    }

    public static MediaInfoWrapper MediaInfo
    {
      get
      {
        return _mediaInfo;
      }
    }

    /// <summary>
    /// Switches to the next audio stream.
    /// 
    /// Calls are directly pushed to the embedded player. And care 
    /// is taken not to do multiple calls to the player.
    /// </summary>
    public static void SwitchToNextAudio()
    {
      if (_player != null)
      {
        // take current stream and number of
        int streams = _player.AudioStreams;
        int current = _player.CurrentAudioStream;
        int next = current;
        bool success = false;
        // Loop over the stream, so we skip the disabled streams
        // stops if the loop is over the current stream again.
        do
        {
          // if next stream is greater then the amount of stream
          // take first
          if (++next >= streams)
          {
            next = 0;
          }
          // set the next stream
          _player.CurrentAudioStream = next;
          // if the stream is set in, stop the loop
          if (next == _player.CurrentAudioStream)
          {
            success = true;
          }
        } while ((next != current) && (success == false));
        if (success == false)
        {
          Log.Info("g_Player: Failed to switch to next audiostream.");
        }
      }
    }

    public static void SwitchToNextSubtitle()
    {
      if (EnableSubtitle)
      {
        if (CurrentSubtitleStream < SubtitleStreams - 1)
        {
          CurrentSubtitleStream++;
        }
        else
        {
          EnableSubtitle = false;
        }
      }
      else
      {
        CurrentSubtitleStream = 0;
        EnableSubtitle = true;
      }
    }

    private static bool IsFileUsedbyAnotherProcess(string file)
    {
      try
      {
        using (new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
        { }
      }
      catch (System.IO.IOException exp)
      {
        Log.Error("g_Player.LoadChapters() - {0}", exp.ToString());
        return true;
      }
      return false;
    }

    private static bool LoadChapters(string videoFile)
    {
      _chapters = null;
      _jumpPoints = null;

      try
      {
        string chapterFile = Path.ChangeExtension(videoFile, ".txt");

        if (!File.Exists(chapterFile) || IsFileUsedbyAnotherProcess(chapterFile))
        {
          return false;
        }

        Log.Debug("g_Player.LoadChapters() - Chapter file found for video \"{0}\"", videoFile);

        if (_loadAutoComSkipSetting)
        {
          using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            _autoComSkip = xmlreader.GetValueAsBool("comskip", "automaticskip", false);
          }

          Log.Debug("g_Player.LoadChapters() - Automatic ComSkip mode is {0}", _autoComSkip ? "on." : "off.");

          _loadAutoComSkipSetting = false;
        }

        ArrayList chapters = new ArrayList();
        ArrayList jumps = new ArrayList();

        using (StreamReader file = new StreamReader(chapterFile))
        {
          string line = file.ReadLine();

          int fps;
          if (!int.TryParse(line.Substring(line.LastIndexOf(' ') + 1), out fps))
          {
            Log.Warn("g_Player.LoadChapters() - Invalid chapter file \"{0}\"", chapterFile);
            return false;
          }

          double framesPerSecond = fps / 100.0;
          int time;

          while (!file.EndOfStream)
          {
            line = file.ReadLine();
            if (String.IsNullOrEmpty(line))
            {
              continue;
            }

            string[] tokens = line.Split(new char[] { '\t' });
            if (tokens.Length != 2)
            {
              continue;
            }

            if (int.TryParse(tokens[0], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out time))
            {
              jumps.Add(time / framesPerSecond);
            }

            if (int.TryParse(tokens[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out time))
            {
              chapters.Add(time / framesPerSecond);
            }
          }
        }
        if (chapters.Count == 0)
        {
          Log.Warn("g_Player.LoadChapters() - No chapters found in file \"{0}\"", chapterFile);
          return false;
        }

        _chapters = new double[chapters.Count];
        chapters.CopyTo(_chapters);

        if (jumps.Count > 0)
        {
          _jumpPoints = new double[jumps.Count];
          jumps.CopyTo(_jumpPoints);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("g_Player.LoadChapters() - {0}", ex.ToString());
        return false;
      }
    }

    private static double NextChapterTime(double currentPos)
    {
      if (_chapters != null)
      {
        for (int index = 0; index < _chapters.Length; index++)
        {
          if (currentPos < _chapters[index])
          {
            return _chapters[index];
          }
        }
      }

      return -1; // no skip
    }

    private static double PreviousChapterTime(double currentPos)
    {
      if (_chapters != null)
      {
        for (int index = _chapters.Length - 1; index >= 0; index--)
        {
          if (_chapters[index] < currentPos - 5.0)
          {
            return _chapters[index];
          }
        }
      }

      return 0;
    }

    public static bool JumpToNextChapter()
    {
      if (!Playing)
      {
        return false;
      }

      double nextChapter = NextChapterTime(_player.CurrentPosition);
      Log.Debug("g_Player.JumpNextChapter() - Current Position: {0}, Next Chapter: {1}", _player.CurrentPosition,
                nextChapter);

      if (nextChapter > 0 && nextChapter < _player.Duration)
      {
        SeekAbsolute(nextChapter);
        return true;
      }

      return false;
    }

    public static bool JumpToPrevChapter()
    {
      if (!Playing)
      {
        return false;
      }

      double prevChapter = PreviousChapterTime(_player.CurrentPosition);
      Log.Debug("g_Player.JumpPrevChapter() - Current Position: {0}, Previous Chapter: {1}", _player.CurrentPosition,
                prevChapter);

      if (prevChapter >= 0 && prevChapter < _player.Duration)
      {
        SeekAbsolute(prevChapter);
        return true;
      }

      return false;
    }

    public static void UnableToPlay(string FileName, MediaType type)
    {
      try
      {
        _mediaInfo = new MediaInfoWrapper(FileName);
        
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CODEC_MISSING, 0, 0, 0, 0, 0, null);
        msg.Label = string.Format("{0}: {1}", GUILocalizeStrings.Get(1451), Path.GetFileName(FileName));
        msg.Label2 = string.IsNullOrEmpty(_mediaInfo.VideoCodec) ? string.Empty : string.Format("Video codec: {0}", _mediaInfo.VideoCodec);
        msg.Label3 = string.Format("Audio codec: {0}", _mediaInfo.AudioCodec);
        GUIGraphicsContext.SendMessage(msg);
      }
      catch (Exception ex)
      {
        Log.Error("g_player: Error notifying user about unsuccessful playback of {0} - {1}", FileName, ex.ToString());
      }
    }

    #endregion

    #region FullScreenWindow

    public delegate bool ShowFullScreenWindowHandler();

    private static ShowFullScreenWindowHandler _showFullScreenWindowTV = ShowFullScreenWindowTVDefault;
    private static ShowFullScreenWindowHandler _showFullScreenWindowVideo = ShowFullScreenWindowVideoDefault;
    private static ShowFullScreenWindowHandler _showFullScreenWindowOther = ShowFullScreenWindowOtherDefault;

    /// <summary>
    /// This handler gets called by ShowFullScreenWindow.
    /// It should handle Fullscreen-TV.
    /// By default, it is set to ShowFullScreenWindowTVDefault.
    /// </summary>
    public static ShowFullScreenWindowHandler ShowFullScreenWindowTV
    {
      get { return _showFullScreenWindowTV; }
      set
      {
        _showFullScreenWindowTV = value;
        Log.Debug("g_player: Setting ShowFullScreenWindowTV to {0}", value);
      }
    }

    /// <summary>
    /// This handler gets called by ShowFullScreenWindow.
    /// It should handle general Fullscreen-Video.
    /// By default, it is set to ShowFullScreenWindowVideoDefault.
    /// </summary>
    public static ShowFullScreenWindowHandler ShowFullScreenWindowVideo
    {
      get { return _showFullScreenWindowVideo; }
      set
      {
        _showFullScreenWindowVideo = value;
        Log.Debug("g_player: Setting ShowFullScreenWindowVideo to {0}", value);
      }
    }

    /// <summary>
    /// This handler gets called by ShowFullScreenOther.
    /// It should handle general Fullscreen.
    /// By default, it is set to ShowFullScreenWindowOtherDefault.
    /// </summary>
    public static ShowFullScreenWindowHandler ShowFullScreenWindowOther
    {
      get { return _showFullScreenWindowOther; }
      set
      {
        _showFullScreenWindowOther = value;
        Log.Debug("g_player: Setting ShowFullScreenWindowOther to {0}", value);
      }
    }

    /// <summary>
    /// The default handler does only work if a player is active.
    /// However, GUITVHome and TvPlugin.TVHOME, both set their
    /// own hook to enable fullscreen tv for non-player live
    /// TV.
    /// </summary>
    /// <returns></returns>
    public static bool ShowFullScreenWindowTVDefault()
    {
      if (Playing && IsTV && !IsTVRecording)
      {
        // close e.g. the tv guide dialog (opened from context menu)
        Action actionCloseDialog = new Action(Action.ActionType.ACTION_CLOSE_DIALOG, 0, 0);
        GUIGraphicsContext.OnAction(actionCloseDialog);

        // watching TV
        if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_TVFULLSCREEN)
        {
          return true;
        }
        Log.Info("g_Player: ShowFullScreenWindow switching to fullscreen tv");
        GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_TVFULLSCREEN);
        GUIGraphicsContext.IsFullScreenVideo = true;
        return true;
      }
      return false;
    }

    public static bool ShowFullScreenWindowVideoDefault()
    {
      // If current player has no video, then fail
      if (!HasVideo)
      {
        return false;
      }
      // are we playing music and got the fancy BassMusicPlayer?
      if (IsMusic && BassMusicPlayer.IsDefaultMusicPlayer)
      {
        if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC)
        {
          return true;
        }

        // When we don't have any Visualisation, switch to Now Playing, instead of showing a black screen
        if (BassMusicPlayer.Player.IVizManager.CurrentVisualizationType == VisualizationInfo.PluginType.None)
        {
          if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
          {
            return true;
          }

          Log.Info("g_Player: ShowFullScreenWindow: No Visualisation defined. Switching to Now Playing");
          GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW);
          BassMusicPlayer.Player.VisualizationWindow.Size = new Size(1, 1); // Hide the Vis Window
          return true;
        }

        Log.Info("g_Player: ShowFullScreenWindow switching to fullscreen music");
        GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC);
      }
      else
      {
        if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO)
        {
          return true;
        }
        Log.Info("g_Player: ShowFullScreenWindow switching to fullscreen video");
        GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
      }
      GUIGraphicsContext.IsFullScreenVideo = true;
      return true;
    }

    public static bool ShowFullScreenWindowOtherDefault()
    {
      return false;
    }

    /// <summary>
    /// This function opens a fullscreen window for the current
    /// player. It returns whether a fullscreen window could
    /// be opened.
    /// 
    /// It tries the three handlers in this order:
    ///  - ShowFullScreenWindowTV
    ///  - ShowFullScreenWindowVideo
    ///  - ShowFullScreenWindowOther
    /// 
    /// The idea is to have a central location for deciding what window to
    /// open for fullscreen.
    /// </summary>
    /// <returns></returns>
    public static bool ShowFullScreenWindow()
    {
      Log.Debug("g_Player: ShowFullScreenWindow");

      if (RefreshRateChanger.RefreshRateChangePending)
      {
        RefreshRateChanger.RefreshRateChangeFullscreenVideo = true;
        return true;
      }
      // does window allow switch to fullscreen?
      GUIWindow win = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if (!win.FullScreenVideoAllowed)
      {
        Log.Error("g_Player: ShowFullScreenWindow not allowed by current window");
        return false;
      }
      // try TV
      if (_showFullScreenWindowTV != null && _showFullScreenWindowTV())
      {
        return true;
      }
      // try Video
      if (_showFullScreenWindowVideo != null && _showFullScreenWindowVideo())
      {
        return true;
      }
      // try Other
      if (_showFullScreenWindowOther != null && _showFullScreenWindowOther())
      {
        return true;
      }

      Log.Debug("g_Player: ShowFullScreenWindow cannot switch to fullscreen");
      return false;
    }

    #endregion

    #region private members

    #endregion
  }
}
