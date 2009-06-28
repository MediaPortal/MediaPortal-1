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
using System.Collections;
using System.Drawing;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Util;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIMusicOverlay : GUIOverlayWindow, IRenderLayer
  {
    #region Enums

    private enum PlayBackType : int
    {
      NORMAL = 0,
      GAPLESS = 1,
      CROSSFADE = 2
    }

    #endregion

    #region <skin> Variables

    [SkinControl(0)] protected GUIImage _videoRectangle = null;
    [SkinControl(1)] protected GUIImage _thumbImage = null;
    [SkinControl(2)] protected GUILabelControl _labelPlayTime = null;
    [SkinControl(3)] protected GUIImage _imagePlayLogo = null;
    [SkinControl(4)] protected GUIImage _imagePauseLogo = null;
    [SkinControl(5)] protected GUIFadeLabel _labelInfo = null;
    [SkinControl(6)] protected GUIImage _labelBigPlayTime = null;
    [SkinControl(7)] protected GUIImage _imageFastForward = null;
    [SkinControl(8)] protected GUIImage _imageRewind = null;
    [SkinControl(9)] protected GUIVideoControl _videoWindow = null;
    [SkinControl(10)] protected GUIImage _imageNormal = null;
    [SkinControl(11)] protected GUIImage _imageGapless = null;
    [SkinControl(12)] protected GUIImage _imageCrossfade = null;

    #endregion

    #region Variables

    private bool _isFocused = false;
    private string _fileName = string.Empty;
    private string _thumbLogo = string.Empty;
    private bool _useBassEngine = false;
    private bool _didRenderLastTime = false;
    private bool _visualisationEnabled = true;
    private bool _useID3 = false;
    private bool _settingVisEnabled = true;
    private PlayListPlayer playlistPlayer;

    #endregion

    #region Constructors/Destructors

    public GUIMusicOverlay()
    {
      GetID = (int) Window.WINDOW_MUSIC_OVERLAY;
      playlistPlayer = PlayListPlayer.SingletonPlayer;
      _useBassEngine = BassMusicPlayer.IsDefaultMusicPlayer;
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _settingVisEnabled = xmlreader.GetValueAsBool("musicfiles", "doVisualisation", false) && _useBassEngine;
        _visualisationEnabled = _settingVisEnabled;
        _useID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);
      }
    }

    #endregion

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\musicOverlay.xml");
      GetID = (int) Window.WINDOW_MUSIC_OVERLAY;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MusicOverlay);
      return result;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void PreInit()
    {
      base.PreInit();
      AllocResources();
    }

    public override void Render(float timePassed)
    {
    }

    private void OnUpdateState(bool render)
    {
      if (_didRenderLastTime != render)
      {
        _didRenderLastTime = render;
        if (render)
        {
          QueueAnimation(AnimationType.WindowOpen);
        }
        else
        {
          QueueAnimation(AnimationType.WindowClose);
        }
      }
    }

    public override bool DoesPostRender()
    {
      if (!g_Player.Playing ||
          g_Player.IsVideo || g_Player.IsDVD || g_Player.IsTVRecording || g_Player.IsTV ||
          (!g_Player.IsRadio && !g_Player.IsMusic))
      {
        _fileName = string.Empty;
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }

      if (g_Player.Playing)
      {
        if (g_Player.CurrentFile.Contains(".tsbuffer")) // timeshifting via TVServer ?
        {
          PlayListItem pitem = playlistPlayer.GetCurrentItem();
          if (pitem != null)
          {
            if (pitem.FileName != _fileName)
            {
              _fileName = pitem.FileName;
              _visualisationEnabled = false;
              SetCurrentFile(_fileName);
            }
          }
          else
          {
            if (g_Player.CurrentFile != _fileName)
            {
              _fileName = g_Player.CurrentFile;
              _visualisationEnabled = false;
              SetCurrentFile(_fileName);
            }
          }
        }
        else if (g_Player.CurrentFile != _fileName)
        {
          _fileName = g_Player.CurrentFile;
          if (_settingVisEnabled)
          {
            _visualisationEnabled = true;
          }
          SetCurrentFile(_fileName);
        }
      }

      if (GUIGraphicsContext.IsFullScreenVideo || GUIGraphicsContext.Calibrating)
      {
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }

      if (!GUIGraphicsContext.Overlay)
      {
        OnUpdateState(false);

        if ((_videoWindow != null) &&
            (GUIGraphicsContext.VideoWindow.Equals(new Rectangle(_videoWindow.XPosition, _videoWindow.YPosition,
                                                                 _videoWindow.Width, _videoWindow.Height))))
        {
          return base.IsAnimating(AnimationType.WindowClose);
        }
        else
        {
          if ((_videoRectangle != null) &&
              (GUIGraphicsContext.VideoWindow.Equals(new Rectangle(_videoRectangle.XPosition, _videoRectangle.YPosition,
                                                                   _videoRectangle.Width, _videoRectangle.Height))))
          {
            return base.IsAnimating(AnimationType.WindowClose);
          }
        }
        return false;
          // no final animation when the video window has changed, this happens most likely when a new window opens
      }
      OnUpdateState(true);
      return true;
    }

    public override void PostRender(float timePassed, int iLayer)
    {
      if (GUIWindowManager.IsRouted)
      {
        return;
      }
      if (iLayer != 2)
      {
        return;
      }
      if (!base.IsAnimating(AnimationType.WindowClose))
      {
        if (GUIPropertyManager.GetProperty("#Play.Current.Thumb") != _thumbLogo)
        {
          _fileName = g_Player.CurrentFile;
          SetCurrentFile(_fileName);
        }

        long lPTS1 = (long) (g_Player.CurrentPosition);
        int hh = (int) (lPTS1/3600)%100;
        int mm = (int) ((lPTS1/60)%60);
        int ss = (int) ((lPTS1/1)%60);

        int iSpeed = g_Player.Speed;
        if (hh == 0 && mm == 0 && ss < 5)
        {
          if (iSpeed < 1)
          {
            iSpeed = 1;
            g_Player.Speed = iSpeed;
            g_Player.SeekAbsolute(0.0d);
          }
        }

        if (_imagePlayLogo != null)
        {
          _imagePlayLogo.Visible = ((g_Player.Paused == false) && (g_Player.Playing));
        }

        if (_imagePauseLogo != null)
        {
          _imagePauseLogo.Visible = (g_Player.Paused == true);
        }

        if (_imageFastForward != null)
        {
          _imageFastForward.Visible = (g_Player.Speed > 1);
        }

        if (_imageRewind != null)
        {
          _imageRewind.Visible = (g_Player.Speed < 0);
        }

        if (_imageNormal != null)
        {
          _imageNormal.Visible = (g_Player.PlaybackType == (int) PlayBackType.NORMAL);
        }

        if (_imageGapless != null)
        {
          _imageGapless.Visible = (g_Player.PlaybackType == (int) PlayBackType.GAPLESS);
        }

        if (_imageCrossfade != null)
        {
          _imageCrossfade.Visible = (g_Player.PlaybackType == (int) PlayBackType.CROSSFADE);
        }

        if (_videoWindow != null)
        {
          _videoWindow.Visible = _visualisationEnabled; // switch it of when we do not have any vizualisation
        }

        if (_videoRectangle != null)
        {
          if (g_Player.Playing) // && !_videoWindow.SetVideoWindow)
          {
            _videoRectangle.Visible = GUIGraphicsContext.ShowBackground;
          }
          else
          {
            _videoRectangle.Visible = false;
          }
        }

        // this is called serveral times per second!
        if (_videoWindow != null)
        {
          // Old code for overlay support? Commented out to fix Mantis:
          // 0001682: Music visualization it's off position when using UI Calibration
          //SetVideoWindow(new Rectangle(_videoWindow.XPosition, _videoWindow.YPosition, _videoWindow.Width, _videoWindow.Height));
        }
          // still needed?
        else if (_videoRectangle != null) // to be compatible to the old version
        {
          SetVideoWindow(new Rectangle(_videoRectangle.XPosition, _videoRectangle.YPosition, _videoRectangle.Width,
                                       _videoRectangle.Height));
        }
        else
        {
          // @ Bav: _videoWindow == null here -> System.NullReferenceException
          //SetVideoWindow(new Rectangle());
          Rectangle dummyRect = new Rectangle();
          if (!dummyRect.Equals(GUIGraphicsContext.VideoWindow))
          {
            GUIGraphicsContext.VideoWindow = dummyRect;
          }

          if (_videoWindow != null)
          {
            _videoWindow.SetVideoWindow = false; // avoid flickering if visualization is turned off
          }
        }
      }
      base.Render(timePassed);
    }

    private void SetVideoWindow(Rectangle newRect)
    {
      if (_visualisationEnabled && _videoWindow != null)
      {
        _videoWindow.SetVideoWindow = true;
        if (!newRect.Equals(GUIGraphicsContext.VideoWindow))
        {
          GUIGraphicsContext.VideoWindow = newRect;
        }
      }
    }


    private void SetCurrentFile(string fileName)
    {
      if ((fileName == null) || (fileName == string.Empty))
      {
        return;
      }
      // last.fm radio sets properties manually therefore do not overwrite them.
      if (Util.Utils.IsLastFMStream(fileName))
      {
        return;
      }

      // When Playing an Internet Stream, via BASS, we set the properties inside the BASS audio engine to be able to detect track changes
      if (BassMusicPlayer.IsDefaultMusicPlayer &&
          (fileName.ToLower().StartsWith("http") || fileName.ToLower().StartsWith("mms")))
      {
        return;
      }

      GUIPropertyManager.RemovePlayerProperties();
      GUIPropertyManager.SetProperty("#Play.Current.Title", Util.Utils.GetFilename(fileName));
      GUIPropertyManager.SetProperty("#Play.Current.File", Path.GetFileName(fileName));

      MusicTag tag = null;
      _thumbLogo = string.Empty;
      tag = GetInfo(fileName, out _thumbLogo);

      GUIPropertyManager.SetProperty("#Play.Current.Thumb", _thumbLogo);
      if (tag != null)
      {
        string strText = GUILocalizeStrings.Get(437); //	"Duration"
        string strDuration = String.Format("{0} {1}", strText, Util.Utils.SecondsToHMSString(tag.Duration));
        if (tag.Duration <= 0)
        {
          strDuration = string.Empty;
        }

        strText = GUILocalizeStrings.Get(435); //	"Track"
        string strTrack = String.Format("{0} {1}", strText, tag.Track);
        if (tag.Track <= 0)
        {
          strTrack = string.Empty;
        }

        strText = GUILocalizeStrings.Get(436); //	"Year"
        string strYear = String.Format("{0} {1}", strText, tag.Year);
        if (tag.Year <= 1900)
        {
          strYear = string.Empty;
        }

        GUIPropertyManager.SetProperty("#Play.Current.Genre", tag.Genre);
        GUIPropertyManager.SetProperty("#Play.Current.Comment", tag.Comment);
        GUIPropertyManager.SetProperty("#Play.Current.Title", tag.Title);
        GUIPropertyManager.SetProperty("#Play.Current.Artist", tag.Artist);
        GUIPropertyManager.SetProperty("#Play.Current.Album", tag.Album);
        GUIPropertyManager.SetProperty("#Play.Current.Track", strTrack);
        GUIPropertyManager.SetProperty("#Play.Current.Year", strYear);
        GUIPropertyManager.SetProperty("#Play.Current.Duration", strDuration);
        GUIPropertyManager.SetProperty("#duration", Util.Utils.SecondsToHMSString(tag.Duration));
      }

      // Show Information of Next File in Playlist
      fileName = playlistPlayer.GetNext();
      if (fileName == string.Empty)
      {
        // fix high cpu load due to constant checking
        //m_strThumb = (string)GUIPropertyManager.GetProperty("#Play.Current.Thumb");
        return;
      }
      tag = null;
      string thumb = string.Empty;
      tag = GetInfo(fileName, out thumb);

      GUIPropertyManager.SetProperty("#Play.Next.Thumb", thumb);
      try
      {
        GUIPropertyManager.SetProperty("#Play.Next.File", Path.GetFileName(fileName));
        GUIPropertyManager.SetProperty("#Play.Next.Title", Util.Utils.GetFilename(fileName));
      }
      catch (Exception)
      {
      }

      if (tag != null)
      {
        string strText = GUILocalizeStrings.Get(437); //	"Duration"
        string strDuration = String.Format("{0}{1}", strText, Util.Utils.SecondsToHMSString(tag.Duration));
        if (tag.Duration <= 0)
        {
          strDuration = string.Empty;
        }

        strText = GUILocalizeStrings.Get(435); //	"Track"
        string strTrack = String.Format("{0}{1}", strText, tag.Track);
        if (tag.Track <= 0)
        {
          strTrack = string.Empty;
        }

        strText = GUILocalizeStrings.Get(436); //	"Year"
        string strYear = String.Format("{0}{1}", strText, tag.Year);
        if (tag.Year <= 1900)
        {
          strYear = string.Empty;
        }

        GUIPropertyManager.SetProperty("#Play.Next.Genre", tag.Genre);
        GUIPropertyManager.SetProperty("#Play.Next.Comment", tag.Comment);
        GUIPropertyManager.SetProperty("#Play.Next.Title", tag.Title);
        GUIPropertyManager.SetProperty("#Play.Next.Artist", tag.Artist);
        GUIPropertyManager.SetProperty("#Play.Next.Album", tag.Album);
        GUIPropertyManager.SetProperty("#Play.Next.Track", strTrack);
        GUIPropertyManager.SetProperty("#Play.Next.Year", strYear);
        GUIPropertyManager.SetProperty("#Play.Next.Duration", strDuration);
      }
    }

    public override bool Focused
    {
      get { return _isFocused; }
      set
      {
        _isFocused = value;
        if (_isFocused)
        {
          if (_videoWindow != null)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, (int) _videoWindow.GetID,
                                            0, 0, null);
            OnMessage(msg);
          }
        }
        else
        {
          foreach (GUIControl control in controlList)
          {
            control.Focus = false;
          }
        }
      }
    }

    protected override bool ShouldFocus(Action action)
    {
      return (action.wID == Action.ActionType.ACTION_MOVE_DOWN);
    }

    public override void OnAction(Action action)
    {
      base.OnAction(action);
      if ((action.wID == Action.ActionType.ACTION_MOVE_UP) ||
          (action.wID == Action.ActionType.ACTION_MOVE_RIGHT))
      {
        Focused = false;
      }
    }

    private MusicTag GetInfo(string fileName, out string thumb)
    {
      string skin = GUIGraphicsContext.Skin;
      thumb = string.Empty;
      MusicTag tag = null;

      tag = TagReader.TagReader.ReadTag(fileName);

      if (g_Player.IsRadio)
      {
        // then check which radio station we're playing
        tag = new MusicTag();
        string strFName = g_Player.CurrentFile;
        string coverart;
        // check if radio via TVPlugin
        if (strFName.EndsWith(".tsbuffer", StringComparison.InvariantCultureIgnoreCase) ||
            strFName.StartsWith("rtsp://", StringComparison.InvariantCultureIgnoreCase))
        {
          // yes
          string strChan = GUIPropertyManager.GetProperty("#Play.Current.ArtistThumb");
          tag.Title = strChan;
          coverart = Util.Utils.GetCoverArt(Thumbs.Radio, strChan);
          if (coverart != string.Empty)
          {
            thumb = coverart;
          }
          else
          {
            thumb = string.Empty;
          }
        }
      } //if (g_Player.IsRadio)


      // efforts only for important track
      bool isCurrent = (g_Player.CurrentFile == fileName);

      // check playlist for information
      if (tag == null)
      {
        PlayListItem item = null;

        if (isCurrent)
        {
          item = playlistPlayer.GetCurrentItem();
        }
        else
        {
          item = playlistPlayer.GetNextItem();
        }

        if (item != null)
        {
          tag = (MusicTag) item.MusicTag;
        }
      }

      string strThumb = string.Empty;

      if (tag != null)
      {
        strThumb = Util.Utils.GetAlbumThumbName(tag.Artist, tag.Album);
        if (File.Exists(strThumb))
        {
          thumb = strThumb;
        }
      }

      // no succes with album cover try folder cache
      if (string.IsNullOrEmpty(thumb))
      {
        thumb = Util.Utils.TryEverythingToGetFolderThumbByFilename(fileName, false);
      }

      if (isCurrent)
      {
        // let us test if there is a larger cover art image
        string strLarge = Util.Utils.ConvertToLargeCoverArt(thumb);
        if (File.Exists(strLarge))
        {
          //Log.Debug("GUIMusicOverlay: using larger thumb - {0}", strLarge);
          thumb = strLarge;
        }
      }

      return tag;
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return DoesPostRender();
    }

    public void RenderLayer(float timePassed)
    {
      PostRender(timePassed, 2);
    }

    #endregion
  }
}