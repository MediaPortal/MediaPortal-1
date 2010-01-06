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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AxWMPLib;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Microsoft.Win32;
using WMPLib;
using _WMPOCXEvents_BufferingEventHandler = AxWMPLib._WMPOCXEvents_BufferingEventHandler;
using _WMPOCXEvents_PlayStateChangeEventHandler = AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler;

namespace MediaPortal.Player
{
  public class AudioPlayerWMP9 : IPlayer
  {
    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended
    }

    private int _bufferTime = 5000;

    private string _currentFile = "";
    private PlayState _graphState = PlayState.Init;
    private bool _isFullScreen = false;
    private bool _isCDA = false;
    private int _positionX = 10, _positionY = 10, _videoWidth = 100, _videoHeight = 100;
    private static AxWindowsMediaPlayer _wmp10Player = null;
    private bool _needUpdate = true;
    private bool _notifyPlaying = true;
    private bool _bufferCompleted = true;

    public AudioPlayerWMP9() {}

    private static void CreateInstance()
    {
      // disable auto windows mediaplayer auto cd-play
      if (_wmp10Player != null)
      {
        return;
      }
      try
      {
        UInt32 dwValue = (UInt32)0;
        using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\MediaPlayer\Preferences", true)
          )
        {
          subkey.SetValue("CDAutoPlay", (Int32)dwValue);

          // enable metadata lookup for CD's
          dwValue = (UInt32)Convert.ToInt32(subkey.GetValue("MetadataRetrieval"));
          dwValue |= 1;
          subkey.SetValue("MetadataRetrieval", (Int32)dwValue);
        }
      }
      catch (Exception) {}

      _wmp10Player = new AxWindowsMediaPlayer();


      _wmp10Player.BeginInit();
      GUIGraphicsContext.form.SuspendLayout();
      _wmp10Player.Enabled = true;

      ComponentResourceManager resources = new ComponentResourceManager(typeof (Resource1));
      _wmp10Player.Location = new Point(8, 16);
      _wmp10Player.Name = "axWindowsMediaPlayer1";
      _wmp10Player.OcxState = ((AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
      _wmp10Player.Size = new Size(264, 240);
      _wmp10Player.TabIndex = 0;

      GUIGraphicsContext.form.Controls.Add(_wmp10Player);

      try
      {
        _wmp10Player.EndInit();
      }
      catch (COMException) {}

      _wmp10Player.uiMode = "none";
      _wmp10Player.windowlessVideo = true;

      _wmp10Player.enableContextMenu = false;
      _wmp10Player.Ctlenabled = false;
      _wmp10Player.ClientSize = new Size(0, 0);
      _wmp10Player.Visible = false;
      GUIGraphicsContext.form.ResumeLayout(false);
    }


    public override bool Play(string strFile)
    {
      _isCDA = false;
      _graphState = PlayState.Init;
      _currentFile = strFile;

      Log.Info("AudioPlayerWMP9: Disabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msg);

      _notifyPlaying = true;
      GC.Collect();
      CreateInstance();

      LoadStreamingSettings();

      if (_wmp10Player == null)
      {
        return false;
      }
      if (_wmp10Player.cdromCollection == null)
      {
        return false;
      }
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;

      _wmp10Player.PlayStateChange += new _WMPOCXEvents_PlayStateChangeEventHandler(OnPlayStateChange);

      _wmp10Player.Buffering += new _WMPOCXEvents_BufferingEventHandler(OnBuffering);

      //_wmp10Player.enableContextMenu = false;
      //_wmp10Player.Ctlenabled = false;
      if (strFile.IndexOf("cdda:") >= 0)
      {
        string strTrack = strFile.Substring(5);
        int iTrack = Convert.ToInt32(strTrack);
        if (_wmp10Player.cdromCollection.count <= 0)
        {
          return false;
        }
        if (_wmp10Player.cdromCollection.Item(0).Playlist == null)
        {
          return false;
        }
        if (iTrack > _wmp10Player.cdromCollection.Item(0).Playlist.count)
        {
          return false;
        }
        _wmp10Player.currentMedia = _wmp10Player.cdromCollection.Item(0).Playlist.get_Item(iTrack - 1);
        if (_wmp10Player.currentMedia == null)
        {
          return false;
        }
        _isCDA = true;
        Log.Info("Audioplayer: play track:{0}/{1}", iTrack, _wmp10Player.cdromCollection.Item(0).Playlist.count);
      }
      else if (strFile.IndexOf(".cda") >= 0)
      {
        string strTrack = "";
        int pos = strFile.IndexOf(".cda");
        if (pos >= 0)
        {
          pos--;
          while (Char.IsDigit(strFile[pos]) && pos > 0)
          {
            strTrack = strFile[pos] + strTrack;
            pos--;
          }
        }

        if (_wmp10Player.cdromCollection.count <= 0)
        {
          return false;
        }
        string strDrive = strFile.Substring(0, 1);
        strDrive += ":";
        int iCdRomDriveNr = 0;
        while ((_wmp10Player.cdromCollection.Item(iCdRomDriveNr).driveSpecifier != strDrive) &&
               (iCdRomDriveNr < _wmp10Player.cdromCollection.count))
        {
          iCdRomDriveNr++;
        }

        int iTrack = Convert.ToInt32(strTrack);
        if (_wmp10Player.cdromCollection.Item(iCdRomDriveNr).Playlist == null)
        {
          return false;
        }
        int tracks = _wmp10Player.cdromCollection.Item(iCdRomDriveNr).Playlist.count;
        if (iTrack > tracks)
        {
          return false;
        }
        _wmp10Player.currentMedia = _wmp10Player.cdromCollection.Item(iCdRomDriveNr).Playlist.get_Item(iTrack - 1);
        if (_wmp10Player.currentMedia == null)
        {
          return false;
        }
        /*
        string strStart=strFile.Substring(0,2)+@"\";
        int ipos=strFile.LastIndexOf("+");
        if (ipos >0) strStart += strFile.Substring(ipos+1);
        strFile=strStart;
        _currentFile=strFile;
        Log.Info("Audioplayer:play {0}", strFile);*/
        //_wmp10Player.URL=strFile;
        _currentFile = strFile;
        _isCDA = true;
      }
      else
      {
        Log.Info("Audioplayer:play {0}", strFile);
        _wmp10Player.URL = strFile;
      }
      _wmp10Player.Ctlcontrols.play();
      _wmp10Player.ClientSize = new Size(0, 0);
      _wmp10Player.Visible = false;

      // When file is internetstream
      if (_wmp10Player.URL.StartsWith("http") || _wmp10Player.URL.StartsWith("mms") ||
          _wmp10Player.URL.StartsWith("HTTP") || _wmp10Player.URL.StartsWith("MMS"))
      {
        _bufferCompleted = false;
        using (WaitCursor waitcursor = new WaitCursor())
        {
          GUIGraphicsContext.Overlay = false;
          while (_bufferCompleted != true)
          {
            {
              // if true then could not load stream 
              if (_wmp10Player.playState.Equals(WMPPlayState.wmppsReady))
              {
                _bufferCompleted = true;
              }
              if (GUIGraphicsContext.Overlay)
              {
                GUIGraphicsContext.Overlay = false;
              }
              _graphState = PlayState.Playing;
              GUIWindowManager.Process();
            }
          }
          GUIGraphicsContext.Overlay = true;
        }
        if (_bufferCompleted && _wmp10Player.playState.Equals(WMPPlayState.wmppsReady))
        {
          Log.Info("Audioplayer: failed to load {0}", strFile);
          return false;
        }
      }

      GUIMessage msgPb = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
      msgPb.Label = strFile;

      GUIWindowManager.SendThreadMessage(msgPb);
      _graphState = PlayState.Playing;
      GC.Collect();
      _needUpdate = true;
      _isFullScreen = GUIGraphicsContext.IsFullScreenVideo;
      _positionX = GUIGraphicsContext.VideoWindow.Left;
      _positionY = GUIGraphicsContext.VideoWindow.Top;
      _videoWidth = GUIGraphicsContext.VideoWindow.Width;
      _videoHeight = GUIGraphicsContext.VideoWindow.Height;

      SetVideoWindow();

      return true;
    }

    private void OnPlayStateChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
    {
      if (_wmp10Player == null)
      {
        return;
      }
      switch (_wmp10Player.playState)
      {
        case WMPPlayState.wmppsStopped:
          SongEnded(false);
          break;
      }
    }

    private void LoadStreamingSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _bufferTime = xmlreader.GetValueAsInt("general", "streamingbuffer", 5000);
      }
    }

    private void OnBuffering(object sender, _WMPOCXEvents_BufferingEvent e)
    {
      Log.Debug("Audioplayer: bandWidth: {0}", _wmp10Player.network.bandWidth);
      Log.Debug("Audioplayer: bitRate: {0}", _wmp10Player.network.bitRate);
      Log.Debug("Audioplayer: receivedPackets: {0}", _wmp10Player.network.receivedPackets);
      Log.Debug("Audioplayer: receptionQuality: {0}", _wmp10Player.network.receptionQuality);

      _wmp10Player.network.bufferingTime = _bufferTime;

      if (e.start)
      {
        _bufferCompleted = false;
        Log.Debug("Audioplayer: bandWidth: {0}", _wmp10Player.network.bandWidth);
        Log.Debug("Audioplayer: receptionQuality: {0}", _wmp10Player.network.receptionQuality);
      }
      if (!e.start)
      {
        _bufferCompleted = true;
      }
    }

    private void SongEnded(bool bManualStop)
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped

      if (!Util.Utils.IsAudio(_currentFile))
      {
        GUIGraphicsContext.IsFullScreenVideo = false;
      }
      Log.Info("Audioplayer:ended {0} {1}", _currentFile, bManualStop);
      _currentFile = "";
      _isCDA = false;

      if (_wmp10Player != null)
      {
        _bufferCompleted = true;
        _wmp10Player.ClientSize = new Size(0, 0);
        _wmp10Player.Visible = false;
        _wmp10Player.PlayStateChange -= new _WMPOCXEvents_PlayStateChangeEventHandler(OnPlayStateChange);
      }
      //GUIGraphicsContext.IsFullScreenVideo=false;
      GUIGraphicsContext.IsPlaying = false;
      if (!bManualStop)
      {
        _graphState = PlayState.Ended;
      }
      else
      {
        _graphState = PlayState.Init;
      }
      GC.Collect();
    }


    public override bool Ended
    {
      get { return _graphState == PlayState.Ended; }
    }

    public override double Duration
    {
      get
      {
        if (_graphState != PlayState.Init && _wmp10Player != null)
        {
          try
          {
            return _wmp10Player.currentMedia.duration;
          }
          catch (Exception) {}
        }
        return 0.0d;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        try
        {
          return _wmp10Player.Ctlcontrols.currentPosition;
        }
        catch (Exception) {}
        return 0.0d;
      }
    }

    public override void Pause()
    {
      if (_wmp10Player == null)
      {
        return;
      }
      if (_graphState == PlayState.Paused)
      {
        _graphState = PlayState.Playing;
        _wmp10Player.Ctlcontrols.play();
      }
      else if (_graphState == PlayState.Playing)
      {
        _wmp10Player.Ctlcontrols.pause();
        if (_wmp10Player.playState == WMPPlayState.wmppsPaused)
        {
          _graphState = PlayState.Paused;
        }
      }
    }

    public override bool Paused
    {
      get { return (_graphState == PlayState.Paused); }
    }

    public override bool Playing
    {
      get { return (_graphState == PlayState.Playing || _graphState == PlayState.Paused); }
    }

    public override bool Stopped
    {
      get { return (_graphState == PlayState.Init); }
    }

    public override string CurrentFile
    {
      get { return _currentFile; }
    }

    public override void Stop()
    {
      if (_wmp10Player == null)
      {
        return;
      }
      if (_graphState != PlayState.Init)
      {
        _wmp10Player.Ctlcontrols.stop();
        _wmp10Player.ClientSize = new Size(0, 0);
        _wmp10Player.Visible = false;
        SongEnded(true);
      }
    }

    public override int Volume
    {
      get
      {
        if (_wmp10Player == null)
        {
          return 100;
        }
        return _wmp10Player.settings.volume;
      }
      set
      {
        if (_wmp10Player == null)
        {
          return;
        }
        if (_wmp10Player.settings.volume != value)
        {
          _wmp10Player.settings.volume = value;
        }
      }
    }


    public override bool HasVideo
    {
      get { return false; }
    }

    public override bool HasViz
    {
      get { return true; }
    }

    public override bool IsCDA
    {
      get { return _isCDA; }
    }

    #region IDisposable Members

    public override void Release()
    {
      if (_wmp10Player == null)
      {
        return;
      }
      _wmp10Player.ClientSize = new Size(0, 0);
      _wmp10Player.Visible = false;
      /*
      try
      {
        GUIGraphicsContext.form.Controls.Remove(_wmp10Player);
      }
      catch (Exception) { }
      _wmp10Player.Dispose();
      _wmp10Player = null;
       * */
    }

    #endregion

    public override bool FullScreen
    {
      get { return GUIGraphicsContext.IsFullScreenVideo; }
      set
      {
        if (value != _isFullScreen)
        {
          _isFullScreen = value;
          _needUpdate = true;
        }
      }
    }

    public override int PositionX
    {
      get { return _positionX; }
      set
      {
        if (value != _positionX)
        {
          _positionX = value;
          _needUpdate = true;
        }
      }
    }

    public override int PositionY
    {
      get { return _positionY; }
      set
      {
        if (value != _positionY)
        {
          _positionY = value;
          _needUpdate = true;
        }
      }
    }

    public override int RenderWidth
    {
      get { return _videoWidth; }
      set
      {
        if (value != _videoWidth)
        {
          _videoWidth = value;
          _needUpdate = true;
        }
      }
    }

    public override int RenderHeight
    {
      get { return _videoHeight; }
      set
      {
        if (value != _videoHeight)
        {
          _videoHeight = value;
          _needUpdate = true;
        }
      }
    }

    public override void Process()
    {
      if (!Playing)
      {
        return;
      }
      if (_wmp10Player == null)
      {
        return;
      }
      if (GUIGraphicsContext.BlankScreen ||
          (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.IsFullScreenVideo == false))
      {
        if (_wmp10Player.Visible)
        {
          _wmp10Player.ClientSize = new Size(0, 0);
          _wmp10Player.Visible = false;
          //_wmp10Player.uiMode = "invisible";
        }
      }
      else if (!_wmp10Player.Visible)
      {
        _needUpdate = true;
        SetVideoWindow();
        //_wmp10Player.uiMode = "none";
        _wmp10Player.Visible = true;
      }


      if (CurrentPosition >= 10.0)
      {
        if (_notifyPlaying)
        {
          _notifyPlaying = false;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYING_10SEC, 0, 0, 0, 0, 0, null);
          msg.Label = CurrentFile;
          GUIWindowManager.SendThreadMessage(msg);
        }
      }
    }

    public delegate void SafeInvoke();

    public override void SetVideoWindow()
    {
      if (_wmp10Player == null)
      {
        return;
      }
      if (GUIGraphicsContext.IsFullScreenVideo != _isFullScreen)
      {
        _isFullScreen = GUIGraphicsContext.IsFullScreenVideo;
        _needUpdate = true;
      }
      if (!_needUpdate)
      {
        return;
      }
      _needUpdate = false;

      if (_isFullScreen)
      {
        Log.Info("AudioPlayer:Fullscreen");

        _positionX = GUIGraphicsContext.OverScanLeft;
        _positionY = GUIGraphicsContext.OverScanTop;
        _videoWidth = GUIGraphicsContext.OverScanWidth;
        _videoHeight = GUIGraphicsContext.OverScanHeight;

        SafeInvoke si = new SafeInvoke(delegate()
                                         {
                                           _wmp10Player.Location = new Point(0, 0);
                                           _wmp10Player.ClientSize = new Size(GUIGraphicsContext.Width,
                                                                              GUIGraphicsContext.Height);
                                           _wmp10Player.Size = new Size(GUIGraphicsContext.Width,
                                                                        GUIGraphicsContext.Height);
                                           _wmp10Player.stretchToFit = true;
                                         });

        if (_wmp10Player.InvokeRequired)
        {
          IAsyncResult iar = _wmp10Player.BeginInvoke(si);
          iar.AsyncWaitHandle.WaitOne();
        }
        else
        {
          si();
        }

        _videoRectangle = new Rectangle(0, 0, _wmp10Player.ClientSize.Width, _wmp10Player.ClientSize.Height);
        _sourceRectangle = _videoRectangle;

        //_wmp10Player.fullScreen=true;
        Log.Info("AudioPlayer:done");
        return;
      }
      else
      {
        SafeInvoke si = new SafeInvoke(delegate()
                                         {
                                           _wmp10Player.ClientSize = new Size(_videoWidth, _videoHeight);
                                           _wmp10Player.Location = new Point(_positionX, _positionY);
                                         });
        if (_wmp10Player.InvokeRequired)
        {
          IAsyncResult iar = _wmp10Player.BeginInvoke(si);
          iar.AsyncWaitHandle.WaitOne();
        }
        else
        {
          si();
        }
        _videoRectangle = new Rectangle(_positionX, _positionY, _wmp10Player.ClientSize.Width,
                                        _wmp10Player.ClientSize.Height);
        _sourceRectangle = _videoRectangle;
        //Log.Info("AudioPlayer:set window:({0},{1})-({2},{3})",_positionX,_positionY,_positionX+_wmp10Player.ClientSize.Width,_positionY+_wmp10Player.ClientSize.Height);
      }
      //_wmp10Player.uiMode = "none";
      //_wmp10Player.windowlessVideo = true;
      //_wmp10Player.enableContextMenu = false;
      //_wmp10Player.Ctlenabled = false;
      GUIGraphicsContext.form.Controls[0].Enabled = false;
    }

    /*
        public override int AudioStreams
        {
          get { return _wmp10Player.Ctlcontrols.audioLanguageCount;}
        }
        public override int CurrentAudioStream
        {
          get { return _wmp10Player.Ctlcontrols.currentAudioLanguage;}
          set { _wmp10Player.Ctlcontrols.currentAudioLanguage=value;}
        }
        public override string AudioLanguage(int iStream)
        {
          return _wmp10Player.controls.getLanguageName(iStream);
        }
    */

    public override void SeekRelative(double dTime)
    {
      if (_wmp10Player == null)
      {
        return;
      }
      if (_graphState != PlayState.Init)
      {
        double dCurTime = CurrentPosition;
        dTime = dCurTime + dTime;
        if (dTime < 0.0d)
        {
          dTime = 0.0d;
        }
        if (dTime < Duration)
        {
          _wmp10Player.Ctlcontrols.currentPosition = dTime;
        }
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (_wmp10Player == null)
      {
        return;
      }
      if (_graphState != PlayState.Init)
      {
        if (dTime < 0.0d)
        {
          dTime = 0.0d;
        }
        if (dTime < Duration)
        {
          _wmp10Player.Ctlcontrols.currentPosition = dTime;
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (_wmp10Player == null)
      {
        return;
      }
      if (_graphState != PlayState.Init)
      {
        double dCurrentPos = CurrentPosition;
        double dDuration = Duration;

        double fCurPercent = (dCurrentPos / Duration) * 100.0d;
        double fOnePercent = Duration / 100.0d;
        fCurPercent = fCurPercent + (double)iPercentage;
        fCurPercent *= fOnePercent;
        if (fCurPercent < 0.0d)
        {
          fCurPercent = 0.0d;
        }
        if (fCurPercent < Duration)
        {
          _wmp10Player.Ctlcontrols.currentPosition = fCurPercent;
        }
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (_wmp10Player == null)
      {
        return;
      }
      if (_graphState != PlayState.Init)
      {
        if (iPercentage < 0)
        {
          iPercentage = 0;
        }
        if (iPercentage >= 100)
        {
          iPercentage = 100;
        }
        double fPercent = Duration / 100.0f;
        fPercent *= (double)iPercentage;
        _wmp10Player.Ctlcontrols.currentPosition = fPercent;
      }
    }

    public override int Speed
    {
      get
      {
        if (_graphState == PlayState.Init)
        {
          return 1;
        }
        if (_wmp10Player == null)
        {
          return 1;
        }
        return (int)_wmp10Player.settings.rate;
      }
      set
      {
        if (_wmp10Player == null)
        {
          return;
        }
        if (_graphState != PlayState.Init)
        {
          if (value < 0)
          {
            _wmp10Player.Ctlcontrols.currentPosition += (double)value;
            VMR9Util.g_vmr9.EVRProvidePlaybackRate(1.0);
          }
          else
          {
            try
            {
              _wmp10Player.settings.rate = (double)value;
            }
            catch (Exception) {}
            VMR9Util.g_vmr9.EVRProvidePlaybackRate((double)value);
          }
        }
      }
    }
  }
}