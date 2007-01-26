#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *  Copyright (C) 2005-2007 Team MediaPortal
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
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Subtitle;
using MediaPortal.Configuration;
using Un4seen.Bass.AddOn.Cd;

namespace MediaPortal.Player
{
  public class g_Player
  {
    #region enums
    public enum MediaType { Video, TV, Radio, Music, Recording };
    #endregion

    #region variables
    static int _currentStep = 0;
    static int _currentStepIndex = -1;
    static DateTime _seekTimer = DateTime.MinValue;
    static Player.IPlayer _player = null;
    static Player.IPlayer _prevPlayer = null;
    static SubTitles _subs = null;
    static bool _isInitalized = false;
    static string _currentFilePlaying = "";
    static MediaType _currentMedia;
    static Player.IPlayerFactory _factory;
    static public bool Starting = false;
    static ArrayList _seekStepList = new ArrayList();
    static int _seekStepTimeout;
    static public bool configLoaded = false;
    static string[] _driveSpeeds;
    static int _driveCount = 0;
    static string _driveLetters;
    static bool driveSpeedLoaded = false;
    static bool driveSpeedReduced = false;
    static bool driveSpeedControlEnabled = false;
    #endregion

    #region events
    public delegate void StoppedHandler(MediaType type, int stoptime, string filename);
    public delegate void EndedHandler(MediaType type, string filename);
    public delegate void StartedHandler(MediaType type, string filename);
    static public event StoppedHandler PlayBackStopped;
    static public event EndedHandler PlayBackEnded;
    static public event StartedHandler PlayBackStarted;
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
    public static Player.IPlayer Player
    {
      get { return _player; }
    }
    public static Player.IPlayerFactory Factory
    {
      get { return _factory; }
      set { _factory = value; }
    }
    #endregion

    #region Serialisation
    /// <summary>
    /// Retrieve the CD/DVD Speed set in the config file
    /// </summary>
    public static void LoadDriveSpeed()
    {
      string speedTable = String.Empty;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        speedTable = xmlreader.GetValueAsString("cdspeed", "drivespeed", String.Empty);
        driveSpeedControlEnabled = xmlreader.GetValueAsBool("cdspeed", "enabled", false);
      }

      if (!driveSpeedControlEnabled)
        return;

      // if BASS is not the default audio engine, we need to load the CD Plugin first
      if (!BassMusicPlayer.IsDefaultMusicPlayer)
      {
        // Load the CD Plugin
        string appPath = System.Windows.Forms.Application.StartupPath;
        string decoderFolderPath = System.IO.Path.Combine(appPath, @"musicplayer\plugins\audio decoders");

        BassRegistration.BassRegistration.Register();
        int pluginHandle = Un4seen.Bass.Bass.BASS_PluginLoad(decoderFolderPath + "\\basscd.dll");
      }

      // Get the number of CD/DVD drives
      _driveCount = BassCd.BASS_CD_GetDriveCount();


      StringBuilder builderDriveLetter = new StringBuilder();
      // Get Drive letters assigned
      for (int i = 0; i < _driveCount; i++)
      {
        builderDriveLetter.Append(BassCd.BASS_CD_GetDriveLetterChar(i));
      }
      _driveLetters = builderDriveLetter.ToString();

      if (speedTable == String.Empty)
      {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < _driveCount; i++)
        {
          if (builder.Length != 0)
            builder.Append(",");

          float maxspeed = BassCd.BASS_CD_GetSpeedFactor(i);
          builder.Append(Convert.ToInt32(maxspeed).ToString());
        }
        speedTable = builder.ToString();
      }

      _driveSpeeds = speedTable.Split(',');

      driveSpeedLoaded = true;
    }

    /// <summary>
    /// Read the configuration file to get the skip steps
    /// </summary>
    public static ArrayList LoadSettings()
    {
      ArrayList StepArray = new ArrayList();

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strFromXml = xmlreader.GetValueAsString("movieplayer", "skipsteps", "15,30,60,180,300,600,900,1800,3600,7200");
        if (strFromXml == String.Empty) // config after wizard run 1st
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
            continue;
          else
            StepArray.Add(Convert.ToInt32(token));
        }
        _seekStepList = StepArray;

        string timeout = (xmlreader.GetValueAsString("movieplayer", "skipsteptimeout", "1500"));

        if (timeout == string.Empty)
          _seekStepTimeout = 1500;
        else
          _seekStepTimeout = Convert.ToInt16(timeout);
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
        if (token == string.Empty) continue;
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
      string newStyle = String.Empty;
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
            case 1: newStyle += "5,"; break;
            case 2: newStyle += "15,"; break;
            case 3: newStyle += "30,"; break;
            case 4: newStyle += "45,"; break;
            case 5: newStyle += "60,"; break;
            case 6: newStyle += "180,"; break;
            case 7: newStyle += "300,"; break;
            case 8: newStyle += "420,"; break;
            case 9: newStyle += "600,"; break;
            case 10: newStyle += "900,"; break;
            case 11: newStyle += "1800,"; break;
            case 12: newStyle += "2700,"; break;
            case 13: newStyle += "3600,"; break;
            case 14: newStyle += "5400,"; break;
            case 15: newStyle += "7200,"; break;
            case 16: newStyle += "10800,"; break;
            default: break; // Do nothing
          }
        }
      }
      return (newStyle == String.Empty ? String.Empty : newStyle.Substring(0, newStyle.Length - 1));
    }


    /// <summary>
    /// Changes the speed of a drive to the value set in configuration
    /// </summary>
    /// <param name="strFile"></param>
    private static void ChangeDriveSpeed(string strFile)
    {
      if (!driveSpeedLoaded)
        LoadDriveSpeed();

      if (!driveSpeedControlEnabled)
        return;

      try
      {
        // is the DVD inserted in a Drive for which we need to control the speed
        string rootPath = System.IO.Path.GetPathRoot(strFile);
        if (rootPath != null)
        {
          if (rootPath.Length > 1)
          {
            int driveindex = _driveLetters.IndexOf(rootPath.Substring(0, 1));
            if (driveindex > -1 && driveindex < _driveSpeeds.Length)
            {
              BassCd.BASS_CD_SetSpeed(driveindex, Convert.ToSingle(_driveSpeeds[driveindex]));

              driveSpeedReduced = true;
            }
          }
        }
      }
      catch (Exception)
      { }
    }
    #endregion


    #region public members

    //called when current playing file is stopped
    static void OnStopped()
    {
      //check if we're playing
      if (g_Player.Playing && PlayBackStopped != null)
      {
        //yes, then raise event 
        Log.Info("g_Player.OnStopped()");
        PlayBackStopped(_currentMedia, (int)g_Player.CurrentPosition, g_Player.CurrentFile);
      }
    }

    //called when current playing file is stopped
    static void OnEnded()
    {
      //check if we're playing
      if (PlayBackEnded != null)
      {
        //yes, then raise event 
        Log.Info("g_Player.OnEnded()");

        PlayBackEnded(_currentMedia, _currentFilePlaying);
      }
    }
    //called when starting playing a file
    static void OnStarted()
    {
      //check if we're playing
      if (_player == null) return;
      if (_player.Playing)
      {
        //yes, then raise event 
        _currentMedia = MediaType.Music;
        if (_player.IsTV)
        {
          _currentMedia = MediaType.TV;
          if (!_player.IsTimeShifting)
            _currentMedia = MediaType.Recording;
        }
        else if (_player.IsRadio)
        {
          _currentMedia = MediaType.Radio;
        }
        else if (_player.HasVideo)
        {
          if (!MediaPortal.Util.Utils.IsAudio(_currentFilePlaying))
          {
            _currentMedia = MediaType.Video;
          }
        }
        Log.Info("g_Player.OnStarted() {0} media:{1}", _currentFilePlaying, _currentMedia.ToString());
        if (PlayBackStarted != null)
          PlayBackStarted(_currentMedia, _currentFilePlaying);
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

    public static void Stop()
    {
      if (driveSpeedReduced)
      {
        // Set the CD/DVD Speed back to Max Speed
        BASS_CD_INFO cdinfo = new BASS_CD_INFO();

        for (int i = 0; i < _driveCount; i++)
        {
          BassCd.BASS_CD_GetInfo(i, cdinfo);
          int maxspeed = (int)(cdinfo.maxspeed / 176.4);
          BassCd.BASS_CD_SetSpeed(i, maxspeed);
        }
      }

      if (_player != null)
      {
        Log.Info("g_Player.Stop()");
        OnStopped();
        GUIGraphicsContext.ShowBackground = true;
        _player.Stop();
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
      }
    }

    static void CachePlayer()
    {
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
        _player.Pause();
        if (VMR9Util.g_vmr9 != null)
        {
          if (_player.Paused) VMR9Util.g_vmr9.SetRepaint();
        }
      }
    }
    public static bool OnAction(Action action)
    {
      if (_player != null)
      {
        return _player.OnAction(action);
      }
      return false;
    }

    public static bool IsCDA
    {
      get
      {
        if (_player == null) return false;
        return _player.IsCDA;
      }
    }

    public static bool IsDVD
    {
      get
      {
        if (_player == null) return false;
        return _player.IsDVD;
      }
    }

    public static bool IsDVDMenu
    {
      get
      {
        if (_player == null) return false;
        return _player.IsDVDMenu;
      }
    }

    public static bool IsTV
    {
      get
      {
        if (_player == null) return false;
        return _player.IsTV;
      }
    }
    public static bool IsTVRecording
    {
      get
      {
        if (_player == null) return false;
        return (_currentMedia == MediaType.Recording);
      }
    }
    public static bool IsTimeShifting
    {
      get
      {
        if (_player == null) return false;
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
        Starting = true;

        // Stop the BASS engine to avoid problems with Digital Audio
        BassMusicPlayer.Player.FreeBass();

        ChangeDriveSpeed(strPath);

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
          OnStopped();
          _player.Stop();
          CachePlayer();
          GUIGraphicsContext.form.Invalidate(true);
          _player = null;
        }

        if (MediaPortal.Util.Utils.PlayDVD())
        {
          return true;
        }
        _isInitalized = true;
        int iUseVMR9 = 0;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
          GC.Collect(); GC.Collect(); GC.Collect();
          Log.Info("dvdplayer:bla");
        }
        else if (_player.Playing)
        {
          _isInitalized = false;
          if (!_player.IsTV)
          {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
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
      try
      {
        Starting = true;
        //stop radio
        GUIMessage msgRadio = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msgRadio);

        //stop timeshifting tv
        //GUIMessage msgTv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
        //GUIWindowManager.SendMessage(msgTv);

        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        _isInitalized = true;
        _subs = null;
        Log.Info("g_Player.PlayAudioStream({0})", strURL);
        if (_player != null)
        {
          GUIGraphicsContext.ShowBackground = true;
          OnStopped();
          _player.Stop();
          CachePlayer();
          GUIGraphicsContext.form.Invalidate(true);
          _player = null;
        }
        _player = new AudioPlayerWMP9();
        _player = CachePreviousPlayer(_player);

        bool bResult = _player.Play(strURL);
        if (!bResult)
        {
          Log.Info("player:ended");
          _player.Release();
          _player = null;
          _subs = null;
          GC.Collect(); GC.Collect(); GC.Collect();
        }
        else if (_player.Playing)
        {
          _currentFilePlaying = _player.CurrentFile;
          OnStarted();
        }
        _isInitalized = false;
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
      try
      {
        Starting = true;

        //stop radio
        GUIMessage msgRadio = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msgRadio);

        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        if (strURL == null) return false;
        if (strURL.Length == 0) return false;
        _isInitalized = true;
        _subs = null;
        Log.Info("g_Player.PlayVideoStream({0})", strURL);
        if (_player != null)
        {
          GUIGraphicsContext.ShowBackground = true;
          OnStopped();
          _player.Stop();
          CachePlayer();
          GUIGraphicsContext.form.Invalidate(true);
          _player = null;
          GC.Collect(); GC.Collect(); GC.Collect(); GC.Collect();
        }
        //int iUseVMR9inMYMovies = 0;
        //using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        //{
        //  iUseVMR9inMYMovies = xmlreader.GetValueAsInt("movieplayer", "vmr9", 1);
        //}
        //if (iUseVMR9inMYMovies == 0)
        //  _player = new Player.VideoPlayerVMR7();
        //else
        _player = new Player.VideoPlayerVMR9();

        _player = CachePreviousPlayer(_player);
        bool isPlaybackPossible = _player.Play(strURL);
        if (!isPlaybackPossible)
        {
          Log.Info("player:ended");
          _player.Release();
          _player = null;
          _subs = null;
          GC.Collect(); GC.Collect(); GC.Collect();
          //2nd try

          _player = new Player.VideoPlayerVMR9();
          isPlaybackPossible = _player.Play(strURL);
          if (!isPlaybackPossible)
          {
            Log.Info("player2:ended");
            _player.Release();
            _player = null;
            _subs = null;
            GC.Collect(); GC.Collect(); GC.Collect();
          }
        }
        else if (_player.Playing)
        {
          _currentFilePlaying = _player.CurrentFile;
          OnStarted();
        }
        _isInitalized = false;
        return isPlaybackPossible;
      }
      finally
      {
        Starting = false;
      }
    }

    static IPlayer CachePreviousPlayer(IPlayer newPlayer)
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
        Starting = true;

        ChangeDriveSpeed(strFile);

        //stop radio
        if (!MediaPortal.Util.Utils.IsLiveRadio(strFile))
        {
          GUIMessage msgRadio = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(msgRadio);
        }

        if (!MediaPortal.Util.Utils.IsLiveTv(strFile) && !MediaPortal.Util.Utils.IsLiveRadio(strFile))
        {
          //file is not a live tv file
          //so tell recorder to stop timeshifting live-tv
          //Log.Info("player: file is not live tv, so stop timeshifting:{0}", strFile);
          //GUIMessage msgTv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
          //GUIWindowManager.SendMessage(msgTv);
        }

        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        if (strFile == null) return false;
        if (strFile.Length == 0) return false;
        _isInitalized = true;
        _subs = null;
        Log.Info("g_Player.Play({0} {1})", strFile, type);
        if (_player != null)
        {
          GUIGraphicsContext.ShowBackground = true;
          OnStopped();
          _player.Stop();
          CachePlayer();
          _player = null;
          GC.Collect(); GC.Collect(); GC.Collect(); GC.Collect(); //?? ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.NETDEVFX.v20.de/cpref2/html/M_System_GC_Collect_1_804c5d7d.htm
        }
        if (!MediaPortal.Util.Utils.IsAVStream(strFile) && MediaPortal.Util.Utils.IsVideo(strFile))
        {
          if (MediaPortal.Util.Utils.PlayMovie(strFile))
          {
            _isInitalized = false;
            return false;
          }
          string extension = System.IO.Path.GetExtension(strFile).ToLower();
          if (extension == ".ifo" || extension == ".vob")
          {

            int iUseVMR9 = 0;
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
              GC.Collect(); GC.Collect(); GC.Collect();
            }
            else if (_player.Playing)
            {
              _currentFilePlaying = _player.CurrentFile;
              OnStarted();

              _isInitalized = false;
              GUIGraphicsContext.IsFullScreenVideo = true;
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            }
            _isInitalized = false;
            return _isPlaybackPossible;
          }
        }
        _player = _factory.Create(strFile, type);
        if (_player != null)
        {
          _player = CachePreviousPlayer(_player);
          bool bResult = _player.Play(strFile);
          if (!bResult)
          {
            Log.Info("player:ended");
            _player.Release();
            _player = null;
            _subs = null;
            GC.Collect(); GC.Collect(); GC.Collect();
          }
          else if (_player.Playing)
          {
            _currentFilePlaying = _player.CurrentFile;
            OnStarted();
          }
          _isInitalized = false;
          return bResult;
        }
        _isInitalized = false;
      }
      finally
      {
        Starting = false;
      }
      return false;
    }
    public static bool Play(string strFile)
    {
      try
      {
        Starting = true;

        ChangeDriveSpeed(strFile);

        //stop radio
        if (!MediaPortal.Util.Utils.IsLiveRadio(strFile))
        {
          GUIMessage msgRadio = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(msgRadio);
        }

        if (!MediaPortal.Util.Utils.IsLiveTv(strFile) && !MediaPortal.Util.Utils.IsLiveRadio(strFile))
        {
          //file is not a live tv file
          //so tell recorder to stop timeshifting live-tv
          //Log.Info("player: file is not live tv, so stop timeshifting:{0}", strFile);
          //GUIMessage msgTv = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT, 0, 0, 0, 0, 0, null);
          //GUIWindowManager.SendMessage(msgTv);
        }

        _currentStep = 0;
        _currentStepIndex = -1;
        _seekTimer = DateTime.MinValue;
        if (strFile == null) return false;
        if (strFile.Length == 0) return false;
        _isInitalized = true;
        _subs = null;
        Log.Info("g_Player.Play({0})", strFile);
        if (_player != null)
        {
          GUIGraphicsContext.ShowBackground = true;
          OnStopped();

          //SV 
          // If we're using the internal music player and cross-fading is enabled
          // we don't want a hard stop here as it will break cross-fading

          //_player.Stop();
          //CachePlayer();
          //_player = null;
          GC.Collect(); GC.Collect(); GC.Collect(); GC.Collect(); //?? ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.NETDEVFX.v20.de/cpref2/html/M_System_GC_Collect_1_804c5d7d.htm

          bool doStop = true;

          if (MediaPortal.Util.Utils.IsAudio(strFile))
          {
            if (BassMusicPlayer.IsDefaultMusicPlayer && BassMusicPlayer.Player.Playing)
              doStop = !BassMusicPlayer.Player.CrossFadingEnabled;
          }

          if (doStop)
          {
            _player.Stop();

            CachePlayer();
            _player = null;
            GC.Collect(); GC.Collect(); GC.Collect(); GC.Collect();
          }
        }
        if (!MediaPortal.Util.Utils.IsAVStream(strFile) && MediaPortal.Util.Utils.IsVideo(strFile))
        {
          // Free BASS to avoid problems with Digital Audio, when watching movies
          if (BassMusicPlayer.IsDefaultMusicPlayer)
          {
            BassMusicPlayer.Player.FreeBass();
          }

          if (MediaPortal.Util.Utils.PlayMovie(strFile))
          {
            _isInitalized = false;
            return false;
          }
          string extension = System.IO.Path.GetExtension(strFile).ToLower();
          if (extension == ".ifo" || extension == ".vob")
          {

            int iUseVMR9 = 0;
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
              GC.Collect(); GC.Collect(); GC.Collect();
            }
            else if (_player.Playing)
            {
              _currentFilePlaying = _player.CurrentFile;
              OnStarted();

              _isInitalized = false;
              GUIGraphicsContext.IsFullScreenVideo = true;
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
            }
            _isInitalized = false;
            return _isPlaybackPossible;
          }
        }
        _player = _factory.Create(strFile);
        if (_player != null)
        {
          _player = CachePreviousPlayer(_player);
          bool bResult = _player.Play(strFile);
          if (!bResult)
          {
            Log.Info("player:ended");
            _player.Release();
            _player = null;
            _subs = null;
            GC.Collect(); GC.Collect(); GC.Collect();
          }
          else if (_player.Playing)
          {
            _currentFilePlaying = _player.CurrentFile;
            OnStarted();
          }
          _isInitalized = false;
          return bResult;
        }
        _isInitalized = false;
      }
      finally
      {
        Starting = false;
      }
      return false;
    }

    public static bool IsExternalPlayer
    {
      get
      {
        if (_player == null) return false;
        return _player.IsExternal;
      }
    }

    public static bool IsRadio
    {
      get
      {
        if (_player == null) return false;
        return (_currentMedia == MediaType.Radio);
      }
    }

    public static bool IsMusic
    {
      get
      {
        if (_player == null) return false;
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
        if (_isInitalized)
        {
          return false;
        }
        bool bResult = _player.Playing;
        return bResult;
      }
    }

    public static bool Paused
    {
      get
      {
        if (_player == null) return false;
        return _player.Paused;
      }
    }
    public static bool Stopped
    {
      get
      {
        if (_isInitalized) return false;
        if (_player == null) return false;
        bool bResult = _player.Stopped;
        return bResult;
      }
    }

    public static int Speed
    {
      get
      {
        if (_player == null) return 1;
        return _player.Speed;
      }
      set
      {
        if (_player == null) return;
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
        if (_player == null) return "";
        return _player.CurrentFile;
      }
    }

    static public int Volume
    {
      get
      {
        if (_player == null) return 0;
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

    static public int PositionX
    {
      get
      {
        if (_player == null) return 0;
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

    static public int PositionY
    {
      get
      {
        if (_player == null) return 0;
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

    static public int RenderWidth
    {
      get
      {
        if (_player == null) return 0;
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
    static public bool Visible
    {
      get
      {
        if (_player == null) return false;
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
    static public int RenderHeight
    {
      get
      {
        if (_player == null) return 0;
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

    static public double Duration
    {
      get
      {
        if (_player == null) return 0;
        return _player.Duration;
      }
    }

    static public double CurrentPosition
    {
      get
      {
        if (_player == null) return 0;
        return _player.CurrentPosition;
      }
    }
    static public double ContentStart
    {
      get
      {
        if (_player == null) return 0;
        return _player.ContentStart;
      }
    }

    static public bool FullScreen
    {
      get
      {
        if (_player == null) return GUIGraphicsContext.IsFullScreenVideo;
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
    static public int Width
    {
      get
      {
        if (_player == null) return 0;
        return _player.Width;
      }
    }

    static public int Height
    {
      get
      {
        if (_player == null) return 0;
        return _player.Height;
      }
    }
    static public void SeekRelative(double dTime)
    {
      if (_player == null) return;
      _player.SeekRelative(dTime);
      _currentStep = 0;
      _currentStepIndex = -1;
      _seekTimer = DateTime.MinValue;
      GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msgUpdate);

    }

    static public void StepNow()
    {
      if (_currentStep != 0 && _player != null)
      {
        double dTime = (int)_currentStep + _player.CurrentPosition;
        if (dTime < 0) dTime = 0d;
        if (dTime > _player.Duration) dTime = _player.Duration - 5;
        _player.SeekAbsolute(dTime);
        GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
        GUIGraphicsContext.SendMessage(msgUpdate);
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
    static public string GetSingleStep(int Step)
    {
      if (Step >= 0)
      {
        if (Step >= 3600)
        {
          // check for 'full' hours
          if ((Convert.ToSingle(Step) / 3600) > 1 && (Convert.ToSingle(Step) / 3600) != 2 && (Convert.ToSingle(Step) / 3600) != 3)
            return "+ " + Convert.ToString(Step / 60) + " " + GUILocalizeStrings.Get(2998);// "min"
          else
            return "+ " + Convert.ToString(Step / 3600) + " " + GUILocalizeStrings.Get(2997);// "hrs"
        }
        else
          if (Step >= 60)
            return "+ " + Convert.ToString(Step / 60) + " " + GUILocalizeStrings.Get(2998);// "min"
          else
            return "+ " + Convert.ToString(Step) + " " + GUILocalizeStrings.Get(2999);// "sec"
      }
      else // back = negative
      {
        if (Step <= -3600)
        {
          if ((Convert.ToSingle(Step) / 3600) < -1 && (Convert.ToSingle(Step) / 3600) != -2 && (Convert.ToSingle(Step) / 3600) != -3)
            return "- " + Convert.ToString(Math.Abs(Step / 60)) + " " + GUILocalizeStrings.Get(2998);// "min"
          else
            return "- " + Convert.ToString(Math.Abs(Step / 3600)) + " " + GUILocalizeStrings.Get(2997);// "hrs"
        }
        else
          if (Step <= -60)
            return "- " + Convert.ToString(Math.Abs(Step / 60)) + " " + GUILocalizeStrings.Get(2998);// "min"
          else
            return "- " + Convert.ToString(Math.Abs(Step)) + " " + GUILocalizeStrings.Get(2999);// "sec"
      }
    }

    static public string GetStepDescription()
    {
      if (_player == null) return "";
      int m_iTimeToStep = (int)_currentStep;
      if (m_iTimeToStep == 0) return "";
      _player.Process();
      if (_player.CurrentPosition + m_iTimeToStep <= 0) return GUILocalizeStrings.Get(773);// "START"
      if (_player.CurrentPosition + m_iTimeToStep >= _player.Duration) return GUILocalizeStrings.Get(774);// "END"
      return GetSingleStep(_currentStep);
    }
    static public int GetSeekStep(out bool bStart, out bool bEnd)
    {
      bStart = false;
      bEnd = false;
      if (_player == null) return 0;
      int m_iTimeToStep = (int)_currentStep;
      if (_player.CurrentPosition + m_iTimeToStep <= 0) bStart = true;//start
      if (_player.CurrentPosition + m_iTimeToStep >= _player.Duration) bEnd = true;
      return m_iTimeToStep;
    }
    static public void SeekStep(bool bFF)
    {
      if (!configLoaded)
      {
        _seekStepList = LoadSettings();
        Log.Info("g_Player loading seekstep config {0}", "");// Convert.ToString(_seekStepList[0]));
      }

      if (bFF)
      {
        if (_currentStep < 0)
        {
          _currentStepIndex--; // E.g. from -30 to -15 
          if (_currentStepIndex == -1)
            _currentStep = 0; // Reached middle, no stepping
          else
            _currentStep = -1 * Convert.ToInt32(_seekStepList[_currentStepIndex]);
        }
        else
        {
          _currentStepIndex++; // E.g. from 15 to 30
          if (_currentStepIndex >= _seekStepList.Count)
            _currentStepIndex--; // Reached maximum step, don't change _currentStep
          else
            _currentStep = Convert.ToInt32(_seekStepList[_currentStepIndex]);
        }
      }
      else
      {
        if (_currentStep <= 0)
        {
          _currentStepIndex++; // E.g. from -15 to -30
          if (_currentStepIndex >= _seekStepList.Count)
            _currentStepIndex--; // Reached maximum step, don't change _currentStep
          else
            _currentStep = -1 * Convert.ToInt32(_seekStepList[_currentStepIndex]);
        }
        else
        {
          _currentStepIndex--; // E.g. from 30 to 15
          if (_currentStepIndex == -1)
            _currentStep = 0; // Reached middle, no stepping
          else
            _currentStep = Convert.ToInt32(_seekStepList[_currentStepIndex]);
        }
      }
      _seekTimer = DateTime.Now;
    }

    static public void SeekRelativePercentage(int iPercentage)
    {
      if (_player == null) return;
      _player.SeekRelativePercentage(iPercentage);
      GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msgUpdate);

      _currentStep = 0;
      _currentStepIndex = -1;
      _seekTimer = DateTime.MinValue;
    }

    static public void SeekAbsolute(double dTime)
    {
      if (_player == null) return;
      _player.SeekAbsolute(dTime);
      GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msgUpdate);

      _currentStep = 0;
      _currentStepIndex = -1;
      _seekTimer = DateTime.MinValue;
    }

    static public void SeekAsolutePercentage(int iPercentage)
    {
      if (_player == null) return;
      _player.SeekAsolutePercentage(iPercentage);
      GUIMessage msgUpdate = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYER_POSITION_CHANGED, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msgUpdate);

      _currentStep = 0;
      _currentStepIndex = -1;
      _seekTimer = DateTime.MinValue;
    }
    static public bool HasVideo
    {
      get
      {
        if (_player == null) return false;
        return _player.HasVideo;
      }
    }
    static public bool IsVideo
    {
      get
      {
        if (_player == null) return false;
        if (_currentMedia == MediaType.Video) return true;
        return false;
      }
    }

    static public bool HasSubs
    {
      get
      {
        if (_player == null) return false;
        return (_subs != null);
      }
    }
    static public void RenderSubtitles()
    {
      if (_player == null) return;
      if (_subs == null) return;
      if (HasSubs)
      {
        _subs.Render(_player.CurrentPosition);
      }
    }
    static public void WndProc(ref Message m)
    {
      if (_player == null) return;
      _player.WndProc(ref m);
    }


    static public void Process()
    {
      if (GUIGraphicsContext.InVmr9Render) return;
      if (GUIGraphicsContext.Vmr9Active && VMR9Util.g_vmr9 != null)
      {
        VMR9Util.g_vmr9.Process();
        VMR9Util.g_vmr9.Repaint();

      }
      if (_player == null) return;
      _player.Process();
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
      }
    }

    static public int AudioStreams
    {
      get
      {
        if (_player == null) return 0;
        return _player.AudioStreams;
      }
    }
    static public int CurrentAudioStream
    {
      get
      {
        if (_player == null) return 0;
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
    static public string AudioLanguage(int iStream)
    {
      if (_player == null) return Strings.Unknown;
      return _player.AudioLanguage(iStream);
    }

    static public int SubtitleStreams
    {
      get
      {
        if (_player == null) return 0;
        return _player.SubtitleStreams;
      }
    }
    static public int CurrentSubtitleStream
    {
      get
      {
        if (_player == null) return 0;
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
    static public void SetVideoWindow()
    {
      if (_player == null) return;
      _player.SetVideoWindow();
    }

    static public string SubtitleLanguage(int iStream)
    {
      if (_player == null) return Strings.Unknown;
      return _player.SubtitleLanguage(iStream);
    }
    static public bool EnableSubtitle
    {
      get
      {
        if (_player == null) return false;
        return _player.EnableSubtitle;
      }
      set
      {
        if (_player == null) return;
        _player.EnableSubtitle = value;
      }
    }

    public static void Init()
    {
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(g_Player.OnVideoWindowChanged);
      GUIGraphicsContext.OnGammaContrastBrightnessChanged += new VideoGammaContrastBrightnessHandler(g_Player.OnGammaContrastBrightnessChanged);
    }

    static void OnGammaContrastBrightnessChanged()
    {
      if (!Playing) return;
      if (!HasVideo) return;
      if (_player == null) return;
      _player.Contrast = GUIGraphicsContext.Contrast;
      _player.Brightness = GUIGraphicsContext.Brightness;
      _player.Gamma = GUIGraphicsContext.Gamma;
    }

    static void OnVideoWindowChanged()
    {
      if (!Playing) return;
      if (!HasVideo) return;

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
      if (windowId == (int)GUIWindow.Window.WINDOW_TV ||
          windowId == (int)GUIWindow.Window.WINDOW_TVGUIDE ||
          windowId == (int)GUIWindow.Window.WINDOW_SEARCHTV ||
          windowId == (int)GUIWindow.Window.WINDOW_SCHEDULER ||
          windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTV)
        inTV = true;
      Visible = (FullScreen || GUIGraphicsContext.Overlay ||
          windowId == (int)GUIWindow.Window.WINDOW_SCHEDULER || inTV);
      SetVideoWindow();
    }

    /// <summary>
    /// returns video window rectangle
    /// </summary>
    static public Rectangle VideoWindow
    {
      get
      {
        if (_player == null) return new Rectangle(0, 0, 0, 0);
        return _player.VideoWindow;
      }
    }

    /// <summary>
    /// returns video source rectangle displayed
    /// </summary>
    static public Rectangle SourceWindow
    {
      get
      {
        if (_player == null) return new Rectangle(0, 0, 0, 0);
        return _player.SourceWindow;
      }
    }
    static public int GetHDC()
    {
      if (_player == null) return 0;
      return _player.GetHDC();
    }

    static public void ReleaseHDC(int HDC)
    {
      if (_player == null) return;
      _player.ReleaseHDC(HDC);
    }

    static public bool CanSeek
    {
      get
      {
        if (_player == null) return false;
        return (_player.CanSeek() && !_player.IsDVDMenu);
      }
    }

    /// <summary>
    /// Switches to the next audio stream.
    /// 
    /// Calls are directly pushed to the embedded player. And care 
    /// is taken not to do multiple calls to the player.
    /// </summary>
    static public void SwitchToNextAudio()
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

    static public void SwitchToNextSubtitle()
    {
      if (EnableSubtitle)
      {
        if (SubtitleStreams > 1)
          if (CurrentSubtitleStream < SubtitleStreams - 1)
            CurrentSubtitleStream++;
          else
          {
            EnableSubtitle = false;
            CurrentSubtitleStream = 0;
          }
      }
      else
      {
        CurrentSubtitleStream = 0;
        EnableSubtitle = true;
      }
    }

    #endregion

  }
}
