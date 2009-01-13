#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using iTunesLib;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace MediaPortal.ITunesPlayer
{
  /// <summary>
  /// Summary description for ITunesPlugin.
  /// </summary>
  [PluginIcons("ExternalPlayers.Itunes.iTunesLogo.png", "ExternalPlayers.Itunes.iTunesLogoDisabled.png")]
  public class ITunesPlugin : IExternalPlayer
  {
    private iTunesAppClass _iTunesApplication = null;
    private IITUserPlaylist _playList;
    private bool _playerIsPaused;
    private string _currentFile = string.Empty;
    private bool _started;
    private bool _ended;
    private double _duration;
    private double _currentPosition;
    private ITPlayerState _playerState;
    private DateTime _updateTimer;
    private string[] m_supportedExtensions = new string[0];
    private bool _notifyPlaying = false;

    public ITunesPlugin()
    {
    }

    public override string Description()
    {
      return "Apple iTunes media player - http://www.apple.com/itunes";
    }

    public override void ShowPlugin()
    {
      ConfigurationForm confForm = new ConfigurationForm();
      confForm.ShowDialog();
    }

    public override string PlayerName
    {
      get { return "iTunes"; }
    }

    /// <summary>
    /// This method returns the version number of the plugin
    /// </summary>
    public override string VersionNumber
    {
      get { return "1.2"; }
    }

    /// <summary>
    /// This method returns the author of the external player
    /// </summary>
    /// <returns></returns>
    public override string AuthorName
    {
      get { return "Frodo"; }
    }

    /// <summary>
    /// Returns all the extensions that the external player supports.  
    /// The return value is an array of extensions of the form: .wma, .mp3, etc...
    /// </summary>
    /// <returns>array of strings of extensions in the form: .wma, .mp3, etc..</returns>
    public override string[] GetAllSupportedExtensions()
    {
      readConfig();
      return m_supportedExtensions;
    }


    /// <summary>
    /// Returns true or false depending if the filename passed is supported or not.
    /// The filename could be just the filename or the complete path of a file.
    /// </summary>
    /// <param name="filename">a fully qualified path and filename or just the filename</param>
    /// <returns>true or false if the file is supported by the player</returns>
    public override bool SupportsFile(string filename)
    {
      readConfig();
      string ext = null;
      int dot = filename.LastIndexOf("."); // couldn't find the dot to get the extension
      if (dot == -1)
      {
        return false;
      }

      ext = filename.Substring(dot).Trim();
      if (ext.Length == 0)
      {
        return false; // no extension so return false;
      }

      ext = ext.ToLower();

      for (int i = 0; i < m_supportedExtensions.Length; i++)
      {
        if (m_supportedExtensions[i].Equals(ext))
        {
          return true;
        }
      }

      // could not match the extension, so return false;
      return false;
    }


    private void readConfig()
    {
      string strExt = null;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strExt = xmlreader.GetValueAsString("itunesplugin", "enabledextensions", "");
      }
      if (strExt != null && strExt.Length > 0)
      {
        m_supportedExtensions = strExt.Split(new char[] {':', ','});
        for (int i = 0; i < m_supportedExtensions.Length; i++)
        {
          m_supportedExtensions[i] = m_supportedExtensions[i].Trim();
        }
      }
    }


    public override bool Play(string strFile)
    {
      try
      {
        if (_iTunesApplication == null)
        {
          _iTunesApplication = new iTunesAppClass();
          _iTunesApplication.OnPlayerPlayEvent +=
            new _IiTunesEvents_OnPlayerPlayEventEventHandler(_iTunesApplication_OnPlayerPlayEvent);
          _iTunesApplication.OnPlayerStopEvent +=
            new _IiTunesEvents_OnPlayerStopEventEventHandler(_iTunesApplication_OnPlayerStopEvent);
          _iTunesApplication.OnPlayerPlayingTrackChangedEvent +=
            new _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler(
              _iTunesApplication_OnPlayerPlayingTrackChangedEvent);
          IITPlaylist playList = null;
          foreach (IITPlaylist pl in _iTunesApplication.LibrarySource.Playlists)
          {
            if (pl.Name.Equals("MediaPortalTemporaryPlaylist"))
            {
              playList = pl;
              break;
            }
          }
          if (playList == null)
          {
            _playList = (IITUserPlaylist) _iTunesApplication.CreatePlaylist("MediaPortalTemporaryPlaylist");
          }
          else
          {
            _playList = (IITUserPlaylist) playList;
          }
          _playList.SongRepeat = ITPlaylistRepeatMode.ITPlaylistRepeatModeOff;
        }

        // stop other media which might be active until now.
        if (g_Player.Playing)
        {
          g_Player.Stop();
        }

        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
        msg.Label = strFile;
        GUIWindowManager.SendThreadMessage(msg);

        _started = false;
        _ended = false;
        foreach (IITTrack track in _playList.Tracks)
        {
          track.Delete();
        }
        _playList.AddFile(strFile);
        _playList.PlayFirstTrack();

        _playerIsPaused = false;
        _currentFile = strFile;
        _duration = -1;
        _currentPosition = -1;
        _playerState = ITPlayerState.ITPlayerStateStopped;
        _updateTimer = DateTime.MinValue;

        UpdateStatus();
        _notifyPlaying = true;
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("ITunesPlugin.Play: Exception");
        Log.Error(ex);
        _notifyPlaying = false;
        _iTunesApplication = null;
      }
      return false;
    }

    private void _iTunesApplication_OnPlayerPlayingTrackChangedEvent(object iTrack)
    {
      IITTrack track = iTrack as IITTrack;
      Log.Info("ITunes:track changed track :{0} duration:{1}", track.Name, track.Duration);
      _iTunesApplication.Stop();
      _ended = true;
    }

    private void _iTunesApplication_OnPlayerStopEvent(object iTrack)
    {
      // ignore event if we're pausing
      if (_playerIsPaused)
      {
        return;
      }
      IITTrack track = iTrack as IITTrack;
      Log.Info("ITunes:playback stopped track :{0} duration:{1}", track.Name, track.Duration);

      _iTunesApplication.Stop();
      _ended = true;
    }

    private void _iTunesApplication_OnPlayerPlayEvent(object iTrack)
    {
      IITTrack track = iTrack as IITTrack;
      Log.Info("ITunes:playback started track :{0} duration:{1}", track.Name, track.Duration);
      _started = true;
    }

    public override double Duration
    {
      get
      {
        if (_iTunesApplication == null)
        {
          return 0.0d;
        }

        UpdateStatus();
        if (_started == false)
        {
          return 300;
        }
        try
        {
          return _duration;
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return 0.0d;
        }
      }
    }

    public override double CurrentPosition
    {
      get
      {
        try
        {
          if (_iTunesApplication == null)
          {
            return 0.0d;
          }
          UpdateStatus();
          if (_started == false)
          {
            return 0.0d;
          }
          return _currentPosition;
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return 0.0d;
        }
      }
    }

    public override void Pause()
    {
      if (_iTunesApplication == null)
      {
        return;
      }
      UpdateStatus();
      if (_started == false && !_playerIsPaused)
      {
        return;
      }
      try
      {
        if (Paused || !_started)
        {
          _playerIsPaused = false;
          _iTunesApplication.PlayPause();
        }
        else
        {
          _playerIsPaused = true;
          _started = false;
          _iTunesApplication.Pause();
        }
      }
      catch (Exception)
      {
        _iTunesApplication = null;
        return;
      }
    }

    public override bool Paused
    {
      get
      {
        try
        {
          if (_started == false)
          {
            return false;
          }
          return _playerIsPaused;
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return false;
        }
      }
    }

    public override bool Playing
    {
      get
      {
        try
        {
          if (_iTunesApplication == null)
          {
            return false;
          }
          UpdateStatus();
          if (_started == false)
          {
            return true;
          }
          if (Paused)
          {
            return true;
          }
          return (_playerState != ITPlayerState.ITPlayerStateStopped);
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return false;
        }
      }
    }

    public override bool Ended
    {
      get
      {
        if (_iTunesApplication == null)
        {
          return true;
        }
        try
        {
          UpdateStatus();
          if (_started == false)
          {
            return false;
          }
          if (Paused)
          {
            return false;
          }
          return (_ended);
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return true;
        }
      }
    }

    public override bool Stopped
    {
      get
      {
        try
        {
          if (_iTunesApplication == null)
          {
            return true;
          }
          UpdateStatus();
          if (_started == false)
          {
            return false;
          }
          if (Paused)
          {
            return false;
          }
          return (_ended);
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return true;
        }
      }
    }

    public override string CurrentFile
    {
      get { return _currentFile; }
    }

    public override void Stop()
    {
      if (_iTunesApplication == null)
      {
        return;
      }
      try
      {
        _iTunesApplication.Stop();
        _playerIsPaused = false;
        _started = false;
        _notifyPlaying = false;
      }
      catch (Exception)
      {
        _iTunesApplication = null;
      }
    }

    public override int Volume
    {
      get
      {
        if (_iTunesApplication == null)
        {
          return 0;
        }
        try
        {
          return _iTunesApplication.SoundVolume;
        }
        catch (Exception)
        {
          _iTunesApplication = null;
          return 0;
        }
      }
      set
      {
        if (_iTunesApplication == null || value < 0 || value > 100)
        {
          return;
        }
        _iTunesApplication.SoundVolume = value;
        try
        {
        }
        catch (Exception)
        {
          _iTunesApplication = null;
        }
      }
    }

    public override void SeekRelative(double dTime)
    {
      double dCurTime = CurrentPosition;
      dTime = dCurTime + dTime;
      if (dTime < 0.0d)
      {
        dTime = 0.0d;
      }
      if (dTime < Duration)
      {
        SeekAbsolute(dTime);
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (dTime < 0.0d)
      {
        dTime = 0.0d;
      }
      if (dTime < Duration)
      {
        //m_winampController.Position = dTime;
        if (_iTunesApplication == null)
        {
          return;
        }
        try
        {
          _iTunesApplication.PlayerPosition = (int) dTime;
        }
        catch (Exception)
        {
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      double dCurrentPos = CurrentPosition;
      double dDuration = Duration;

      double fCurPercent = (dCurrentPos/Duration)*100.0d;
      double fOnePercent = Duration/100.0d;
      fCurPercent = fCurPercent + (double) iPercentage;
      fCurPercent *= fOnePercent;
      if (fCurPercent < 0.0d)
      {
        fCurPercent = 0.0d;
      }
      if (fCurPercent < Duration)
      {
        SeekAbsolute(fCurPercent);
      }
    }

    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (iPercentage < 0)
      {
        iPercentage = 0;
      }
      if (iPercentage >= 100)
      {
        iPercentage = 100;
      }
      double fPercent = Duration/100.0f;
      fPercent *= (double) iPercentage;
      SeekAbsolute(fPercent);
    }

    private void UpdateStatus()
    {
      if (_started == false)
      {
        return;
      }
      if (_iTunesApplication == null)
      {
        return;
      }

      TimeSpan ts = DateTime.Now - _updateTimer;

      if (ts.TotalSeconds >= 1 || _duration < 0 || _started == false)
      {
        _playerState = _iTunesApplication.PlayerState;
        _duration = _iTunesApplication.CurrentTrack.Duration;
        _currentPosition = (double) _iTunesApplication.PlayerPosition;
        _updateTimer = DateTime.Now;
      }
    }

    public override void Process()
    {
      if (!Playing)
      {
        return;
      }

      if (_notifyPlaying && CurrentPosition >= 10.0)
      {
        _notifyPlaying = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC, 0, 0, 0, 0, 0, null);
        msg.Label = CurrentFile;
        GUIWindowManager.SendThreadMessage(msg);
      }
    }
  }
}