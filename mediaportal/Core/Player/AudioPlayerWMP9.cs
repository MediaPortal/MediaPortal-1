#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MediaPortal.TagReader;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using DirectShowLib;
using MediaPortal.Utils.Services;

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
    string _currentFile = "";
    PlayState _graphState = PlayState.Init;
    bool _isFullScreen = false;
    int _positionX = 10, _positionY = 10, _videoWidth = 100, _videoHeight = 100;
    static AxWMPLib.AxWindowsMediaPlayer _wmp10Player = null;
    bool _needUpdate = true;
    bool _notifyPlaying = true;
    bool _bufferCompleted = true;
    protected ILog _log;

    public AudioPlayerWMP9()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    static void CreateInstance()
    {
      // disable auto windows mediaplayer auto cd-play
      if (_wmp10Player != null) return;
      try
      {
        UInt32 dwValue = (UInt32)0;
        using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\MediaPlayer\Preferences", true))
        {
          subkey.SetValue("CDAutoPlay", (Int32)dwValue);

          // enable metadata lookup for CD's
          dwValue = (UInt32)Convert.ToInt32(subkey.GetValue("MetadataRetrieval"));
          dwValue |= 1;
          subkey.SetValue("MetadataRetrieval", (Int32)dwValue);
        }
      }
      catch (Exception) { }

      _wmp10Player = new AxWMPLib.AxWindowsMediaPlayer();


      _wmp10Player.BeginInit();
      GUIGraphicsContext.form.SuspendLayout();
      _wmp10Player.Enabled = true;

      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Resource1));
      _wmp10Player.Location = new System.Drawing.Point(8, 16);
      _wmp10Player.Name = "axWindowsMediaPlayer1";
      _wmp10Player.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
      _wmp10Player.Size = new System.Drawing.Size(264, 240);
      _wmp10Player.TabIndex = 0;
      GUIGraphicsContext.form.Controls.Add(_wmp10Player);


      try
      {
        _wmp10Player.EndInit();
      }
      catch (COMException)
      {
      }

      _wmp10Player.uiMode = "none";
      _wmp10Player.windowlessVideo = true;

      _wmp10Player.enableContextMenu = false;
      _wmp10Player.Ctlenabled = false;
      _wmp10Player.ClientSize = new Size(0, 0);
      _wmp10Player.Visible = false;
      GUIGraphicsContext.form.ResumeLayout(false);


    }

    static public ArrayList GetCDTracks()
    {
      GUIListItem item;
      ArrayList list = new ArrayList();
      item = new GUIListItem();
      item.IsFolder = true;
      item.Label = "..";
      item.Label2 = "";
      item.Path = "";
      MediaPortal.Util.Utils.SetDefaultIcons(item);
      MediaPortal.Util.Utils.SetThumbnails(ref item);
      list.Add(item);

      CreateInstance();
      if (_wmp10Player.cdromCollection.count <= 0) return list;
      if (_wmp10Player.cdromCollection.count <= 0) return list;


      WMPLib.IWMPCdrom cdrom = _wmp10Player.cdromCollection.Item(0);

      if (cdrom == null) return list;
      if (cdrom.Playlist == null) return list;


      for (int iTrack = 0; iTrack < cdrom.Playlist.count; iTrack++)
      {
        try
        {
          MusicTag tag = new MusicTag();
          WMPLib.IWMPMedia media = cdrom.Playlist.get_Item(iTrack);
          item = new GUIListItem();
          item.IsFolder = false;
          item.Label = media.name;
          item.Label2 = "";
          item.Path = String.Format("cdda:{0}", iTrack);
          item.FileInfo = null;

          for (int i = 0; i < media.attributeCount; ++i)
          {
            string strAttr = media.getAttributeName(i);
            string strValue = media.getItemInfo(strAttr);
            if (String.Compare("album", strAttr, true) == 0) tag.Album = strValue;
            if (String.Compare("actor", strAttr, true) == 0) tag.Artist = strValue;
            if (String.Compare("artist", strAttr, true) == 0) tag.Artist = strValue;
            if (String.Compare("style", strAttr, true) == 0) tag.Genre = strValue;
            if (String.Compare("releasedate", strAttr, true) == 0)
            {
              try
              {
                tag.Year = Convert.ToInt32(strValue.Substring(0, 4));
              }
              catch (Exception)
              {
              }
            }
          }
          tag.Title = media.name;
          tag.Duration = (int)media.duration;
          tag.Track = iTrack + 1;
          //tag.Comment  =
          //tag.Year     =
          //tag.Genre    =
          item.MusicTag = tag;
          list.Add(item);
        }
        catch (Exception)
        {
        }
      }
      return list;
    }

    public override bool Play(string strFile)
    {
      _graphState = PlayState.Init;
      _currentFile = strFile;

      if (!GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
      {
        _log.Info("AudioPlayerWMP9: Disabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }

      _notifyPlaying = true;
      GC.Collect();
      CreateInstance();

      if (_wmp10Player == null) return false;
      if (_wmp10Player.cdromCollection == null) return false;
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;

      _wmp10Player.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(OnPlayStateChange);

      _wmp10Player.Buffering += new AxWMPLib._WMPOCXEvents_BufferingEventHandler(OnBuffering);

      //_wmp10Player.enableContextMenu = false;
      //_wmp10Player.Ctlenabled = false;
      if (strFile.IndexOf("cdda:") >= 0)
      {
        string strTrack = strFile.Substring(5);
        int iTrack = Convert.ToInt32(strTrack);
        if (_wmp10Player.cdromCollection.count <= 0) return false;
        if (_wmp10Player.cdromCollection.Item(0).Playlist == null) return false;
        if (iTrack > _wmp10Player.cdromCollection.Item(0).Playlist.count) return false;
        _wmp10Player.currentMedia = _wmp10Player.cdromCollection.Item(0).Playlist.get_Item(iTrack - 1);
        if (_wmp10Player.currentMedia == null) return false;

        _log.Info("Audioplayer:play track:{0}/{1}", iTrack, _wmp10Player.cdromCollection.Item(0).Playlist.count);
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

        if (_wmp10Player.cdromCollection.count <= 0) return false;
        string strDrive = strFile.Substring(0, 1);
        strDrive += ":";
        int iCdRomDriveNr = 0;
        while ((_wmp10Player.cdromCollection.Item(iCdRomDriveNr).driveSpecifier != strDrive) && (iCdRomDriveNr < _wmp10Player.cdromCollection.count))
        {
          iCdRomDriveNr++;
        }

        int iTrack = Convert.ToInt32(strTrack);
        if (_wmp10Player.cdromCollection.Item(iCdRomDriveNr).Playlist == null) return false;
        int tracks = _wmp10Player.cdromCollection.Item(iCdRomDriveNr).Playlist.count;
        if (iTrack > tracks) return false;
        _wmp10Player.currentMedia = _wmp10Player.cdromCollection.Item(iCdRomDriveNr).Playlist.get_Item(iTrack - 1);
        if (_wmp10Player.currentMedia == null) return false;
        /*
        string strStart=strFile.Substring(0,2)+@"\";
        int ipos=strFile.LastIndexOf("+");
        if (ipos >0) strStart += strFile.Substring(ipos+1);
        strFile=strStart;
        _currentFile=strFile;
        _log.Info("Audioplayer:play {0}", strFile);*/
        //_wmp10Player.URL=strFile;
        _currentFile = strFile;
      }
      else
      {
        _log.Info("Audioplayer:play {0}", strFile);
        _wmp10Player.URL = strFile;
      }
      _wmp10Player.Ctlcontrols.play();
      _wmp10Player.ClientSize = new Size(0, 0);
      _wmp10Player.Visible = false;

      if (_wmp10Player.URL.StartsWith("http") || _wmp10Player.URL.StartsWith("mms"))
      {
        try
        {
          _bufferCompleted = false;
          using (WaitCursor waitcursor = new WaitCursor())
          {
            GUIGraphicsContext.Overlay = false;
            while (_bufferCompleted != true)
            {
              {
                _graphState = PlayState.Playing;
                GUIWindowManager.Process();
              }
            }
          }
          GUIGraphicsContext.Overlay = true;

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
        catch (Exception ex)
        {
          _log.Error("audioplayer - play internetstream: {0}", ex);
          return false;
        }
      }
      else
      {

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
    }

    private void OnPlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
    {
      if (_wmp10Player == null) return;
      switch (_wmp10Player.playState)
      {
        case WMPLib.WMPPlayState.wmppsStopped:
          SongEnded(false);
          break;
      }
    }

    private void OnBuffering(object sender, AxWMPLib._WMPOCXEvents_BufferingEvent e)
    {
      if (e.start)
      {
        _bufferCompleted = false;
      }
      if (!e.start)
      {
        _bufferCompleted = true;
      }
    }

    void SongEnded(bool bManualStop)
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped

      if (!MediaPortal.Util.Utils.IsAudio(_currentFile))
        GUIGraphicsContext.IsFullScreenVideo = false;
      _log.Info("Audioplayer:ended {0} {1}", _currentFile, bManualStop);
      _currentFile = "";
      if (_wmp10Player != null)
      {
        _bufferCompleted = true;
        _wmp10Player.ClientSize = new Size(0, 0);
        _wmp10Player.Visible = false;
        _wmp10Player.PlayStateChange -= new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(OnPlayStateChange);
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
          catch (Exception)
          {
          }
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
        catch (Exception)
        {
        }
        return 0.0d;
      }
    }

    public override void Pause()
    {
      if (_wmp10Player == null) return;
      if (_graphState == PlayState.Paused)
      {
        _graphState = PlayState.Playing;
        _wmp10Player.Ctlcontrols.play();
      }
      else if (_graphState == PlayState.Playing)
      {
        _wmp10Player.Ctlcontrols.pause();
        if (_wmp10Player.playState == WMPLib.WMPPlayState.wmppsPaused)
          _graphState = PlayState.Paused;
      }
    }

    public override bool Paused
    {
      get
      {
        return (_graphState == PlayState.Paused);
      }
    }

    public override bool Playing
    {
      get
      {
        return (_graphState == PlayState.Playing || _graphState == PlayState.Paused);
      }
    }

    public override bool Stopped
    {
      get
      {
        return (_graphState == PlayState.Init);
      }
    }

    public override string CurrentFile
    {
      get { return _currentFile; }
    }

    public override void Stop()
    {
      if (_wmp10Player == null) return;
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

        if (_wmp10Player == null) return 100;
        return _wmp10Player.settings.volume;
      }
      set
      {

        if (_wmp10Player == null) return;
        if (_wmp10Player.settings.volume != value)
        {
          _wmp10Player.settings.volume = value;
        }
      }
    }


    public override bool HasVideo
    {
      get { return true; }
    }


    #region IDisposable Members

    public override void Release()
    {

      if (_wmp10Player == null) return;
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
      get
      {
        return GUIGraphicsContext.IsFullScreenVideo;
      }
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
      if (!Playing) return;
      if (_wmp10Player == null) return;
      if (GUIGraphicsContext.BlankScreen || (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.IsFullScreenVideo == false))
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

    public override void SetVideoWindow()
    {

      if (_wmp10Player == null) return;
      if (GUIGraphicsContext.IsFullScreenVideo != _isFullScreen)
      {
        _isFullScreen = GUIGraphicsContext.IsFullScreenVideo;
        _needUpdate = true;
      }
      if (!_needUpdate) return;
      _needUpdate = false;


      if (_isFullScreen)
      {
        _log.Info("AudioPlayer:Fullscreen");

        _positionX = GUIGraphicsContext.OverScanLeft;
        _positionY = GUIGraphicsContext.OverScanTop;
        _videoWidth = GUIGraphicsContext.OverScanWidth;
        _videoHeight = GUIGraphicsContext.OverScanHeight;

        _wmp10Player.Location = new Point(0, 0);
        _wmp10Player.ClientSize = new System.Drawing.Size(GUIGraphicsContext.Width, GUIGraphicsContext.Height);
        _wmp10Player.Size = new System.Drawing.Size(GUIGraphicsContext.Width, GUIGraphicsContext.Height);

        _videoRectangle = new Rectangle(0, 0, _wmp10Player.ClientSize.Width, _wmp10Player.ClientSize.Height);
        _sourceRectangle = _videoRectangle;

        //_wmp10Player.fullScreen=true;
        _wmp10Player.stretchToFit = true;
        _log.Info("AudioPlayer:done");
        return;
      }
      else
      {

        _wmp10Player.ClientSize = new System.Drawing.Size(_videoWidth, _videoHeight);
        _wmp10Player.Location = new Point(_positionX, _positionY);

        _videoRectangle = new Rectangle(_positionX, _positionY, _wmp10Player.ClientSize.Width, _wmp10Player.ClientSize.Height);
        _sourceRectangle = _videoRectangle;
        //_log.Info("AudioPlayer:set window:({0},{1})-({2},{3})",_positionX,_positionY,_positionX+_wmp10Player.ClientSize.Width,_positionY+_wmp10Player.ClientSize.Height);
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
      if (_wmp10Player == null) return;
      if (_graphState != PlayState.Init)
      {

        double dCurTime = CurrentPosition;
        dTime = dCurTime + dTime;
        if (dTime < 0.0d) dTime = 0.0d;
        if (dTime < Duration)
        {
          _wmp10Player.Ctlcontrols.currentPosition = dTime;
        }
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (_wmp10Player == null) return;
      if (_graphState != PlayState.Init)
      {
        if (dTime < 0.0d) dTime = 0.0d;
        if (dTime < Duration)
        {
          _wmp10Player.Ctlcontrols.currentPosition = dTime;
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (_wmp10Player == null) return;
      if (_graphState != PlayState.Init)
      {
        double dCurrentPos = CurrentPosition;
        double dDuration = Duration;

        double fCurPercent = (dCurrentPos / Duration) * 100.0d;
        double fOnePercent = Duration / 100.0d;
        fCurPercent = fCurPercent + (double)iPercentage;
        fCurPercent *= fOnePercent;
        if (fCurPercent < 0.0d) fCurPercent = 0.0d;
        if (fCurPercent < Duration)
        {
          _wmp10Player.Ctlcontrols.currentPosition = fCurPercent;
        }
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (_wmp10Player == null) return;
      if (_graphState != PlayState.Init)
      {
        if (iPercentage < 0) iPercentage = 0;
        if (iPercentage >= 100) iPercentage = 100;
        double fPercent = Duration / 100.0f;
        fPercent *= (double)iPercentage;
        _wmp10Player.Ctlcontrols.currentPosition = fPercent;
      }
    }
    public override int Speed
    {
      get
      {
        if (_graphState == PlayState.Init) return 1;
        if (_wmp10Player == null) return 1;
        return (int)_wmp10Player.settings.rate;
      }
      set
      {
        if (_wmp10Player == null) return;
        if (_graphState != PlayState.Init)
        {
          if (value < 0)
          {
            _wmp10Player.Ctlcontrols.currentPosition += (double)value;
          }
          else
          {
            try
            {
              _wmp10Player.settings.rate = (double)value;
            }
            catch (Exception)
            {
            }
          }
        }
      }
    }
  }
}
