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

using System;
using System.Drawing;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Video.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using MediaPortal.TagReader;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Radio.Database;
using MediaPortal.Music.Database;


namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIMusicOverlay : GUIOverlayWindow, IRenderLayer
  {
    [SkinControlAttribute(0)]    protected GUIImage        _videoRectangle = null;
    [SkinControlAttribute(1)]    protected GUIImage        _thumbImage = null;
    [SkinControlAttribute(2)]    protected GUILabelControl _labelPlayTime = null;
    [SkinControlAttribute(3)]    protected GUIImage        _imagePlayLogo = null;
    [SkinControlAttribute(4)]    protected GUIImage        _imagePauseLogo = null;
    [SkinControlAttribute(5)]    protected GUIFadeLabel    _labelInfo = null;
    [SkinControlAttribute(6)]    protected GUIImage        _labelBigPlayTime = null;
    [SkinControlAttribute(7)]    protected GUIImage        _imageFastForward = null;
    [SkinControlAttribute(8)]    protected GUIImage        _imageRewind = null;
    [SkinControlAttribute(9)]    protected GUIVideoControl _videoWindow = null;

    bool _isFocused = false;
    string _fileName           = String.Empty;
    string _thumbLogo          = String.Empty;
    bool _useBassEngine        = false;
    bool _didRenderLastTime    = false;
    bool _visualisationEnabled = true;
    bool _useID3               = false;
    PlayListPlayer playlistPlayer;


    public GUIMusicOverlay()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_OVERLAY;
      playlistPlayer = PlayListPlayer.SingletonPlayer;
      _useBassEngine = BassMusicPlayer.IsDefaultMusicPlayer;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _visualisationEnabled = xmlreader.GetValueAsBool("musicfiles", "doVisualisation", true);
        _useID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);
      }
    }

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\musicOverlay.xml");
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_OVERLAY;
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

    void OnUpdateState(bool render)
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
          (!g_Player.IsRadio && !g_Player.IsMusic) )
      {
        _fileName = String.Empty;
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }
      
      if ((g_Player.Playing) && (g_Player.CurrentFile != _fileName))
      {
        _fileName = g_Player.CurrentFile;
        SetCurrentFile(_fileName);
      }

      if ((Recorder.IsRadio()) && (Recorder.RadioStationName() != _fileName))
      {
        _fileName = Recorder.RadioStationName();
        SetCurrentFile(_fileName);
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
            (GUIGraphicsContext.VideoWindow.Equals(new Rectangle(_videoWindow.XPosition, _videoWindow.YPosition, _videoWindow.Width, _videoWindow.Height))))
          return base.IsAnimating(AnimationType.WindowClose);
        else
        {
          if ((_videoRectangle != null) &&
            (GUIGraphicsContext.VideoWindow.Equals(new Rectangle(_videoRectangle.XPosition, _videoRectangle.YPosition, _videoRectangle.Width, _videoRectangle.Height))))
            return base.IsAnimating(AnimationType.WindowClose);
        }
        return false;   // no final animation when the video window has changed, this happens most likely when a new window opens
      }
      
      OnUpdateState(true);
      return true;
    }

    public override void PostRender(float timePassed, int iLayer)
    {
      if (iLayer != 2) return;
      if (!base.IsAnimating(AnimationType.WindowClose))
      {
        if (GUIPropertyManager.GetProperty("#Play.Current.Thumb") != _thumbLogo)
        {
          _fileName = g_Player.CurrentFile;
          SetCurrentFile(_fileName);
        }

        long lPTS1 = (long)(g_Player.CurrentPosition);
        int hh = (int)(lPTS1 / 3600) % 100;
        int mm = (int)((lPTS1 / 60) % 60);
        int ss = (int)((lPTS1 / 1) % 60);

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
          _imagePlayLogo.Visible = ((g_Player.Paused == false) && (g_Player.Speed == 0));

        if (_imagePauseLogo != null)
          _imagePauseLogo.Visible = ((g_Player.Paused == true) && (g_Player.Speed == 0));

        if (_imageFastForward != null)
          _imageFastForward.Visible = (g_Player.Speed > 1);

        if (_imageRewind != null)
          _imageRewind.Visible = (g_Player.Speed < 0);

        if (_videoRectangle != null)
          _videoRectangle.Visible = GUIGraphicsContext.ShowBackground;

        if (_videoWindow != null && _visualisationEnabled)
        {
          SetVideoWindow(new Rectangle(_videoWindow.XPosition, _videoWindow.YPosition, _videoWindow.Width, _videoWindow.Height));
        }
        else
          if (_videoRectangle != null && _visualisationEnabled)  // to be compatible to the old version
        {
          SetVideoWindow(new Rectangle(_videoRectangle.XPosition, _videoRectangle.YPosition, _videoRectangle.Width, _videoRectangle.Height));
        }
        else
        {
          SetVideoWindow(new Rectangle());
        }
      }
      base.Render(timePassed);
    }

    void SetVideoWindow(Rectangle newRect)
    {
      if (!newRect.Equals(GUIGraphicsContext.VideoWindow))
        GUIGraphicsContext.VideoWindow = newRect;
    }


    void SetCurrentFile(string fileName)
    {
      if ((fileName == null) || (fileName == String.Empty))
        return;
      // last.fm radio sets properties manually therefore do not overwrite them.
      if (fileName.Contains(@"/last.mp3?"))
        return;

      GUIPropertyManager.RemovePlayerProperties();
      GUIPropertyManager.SetProperty("#Play.Current.Title", MediaPortal.Util.Utils.GetFilename(fileName));
      GUIPropertyManager.SetProperty("#Play.Current.File", System.IO.Path.GetFileName(fileName));

      MusicTag tag = null;
      _thumbLogo = String.Empty;
      tag = GetInfo(fileName, out _thumbLogo);

      GUIPropertyManager.SetProperty("#Play.Current.Thumb", _thumbLogo);
      if (tag != null)
      {
        string strText = GUILocalizeStrings.Get(437);	//	"Duration"
        string strDuration = String.Format("{0} {1}", strText, MediaPortal.Util.Utils.SecondsToHMSString(tag.Duration));
        if (tag.Duration <= 0)
          strDuration = String.Empty;

        strText = GUILocalizeStrings.Get(435);	//	"Track"
        string strTrack = String.Format("{0} {1}", strText, tag.Track);
        if (tag.Track <= 0)
          strTrack = String.Empty;

        strText = GUILocalizeStrings.Get(436);	//	"Year"
        string strYear = String.Format("{0} {1}", strText, tag.Year);
        if (tag.Year <= 1900)
          strYear = String.Empty;

        GUIPropertyManager.SetProperty("#Play.Current.Genre", tag.Genre);
        GUIPropertyManager.SetProperty("#Play.Current.Comment", tag.Comment);
        GUIPropertyManager.SetProperty("#Play.Current.Title", tag.Title);
        GUIPropertyManager.SetProperty("#Play.Current.Artist", tag.Artist);
        GUIPropertyManager.SetProperty("#Play.Current.Album", tag.Album);
        GUIPropertyManager.SetProperty("#Play.Current.Track", strTrack);
        GUIPropertyManager.SetProperty("#Play.Current.Year", strYear);
        GUIPropertyManager.SetProperty("#Play.Current.Duration", strDuration);
      }

      // Show Information of Next File in Playlist
      fileName = playlistPlayer.GetNext();
      if (fileName == String.Empty)
      {
        // fix high cpu load due to constant checking
        //m_strThumb = (string)GUIPropertyManager.GetProperty("#Play.Current.Thumb");
        return;
      }
      tag = null;
      string thumb = String.Empty;
      tag = GetInfo(fileName, out thumb);

      GUIPropertyManager.SetProperty("#Play.Next.Thumb", thumb);
      try
      {
        GUIPropertyManager.SetProperty("#Play.Next.File", System.IO.Path.GetFileName(fileName));
        GUIPropertyManager.SetProperty("#Play.Next.Title", MediaPortal.Util.Utils.GetFilename(fileName));
      }
      catch (Exception) { }

      if (tag != null)
      {
        string strText = GUILocalizeStrings.Get(437);	//	"Duration"
        string strDuration = String.Format("{0}{1}", strText, MediaPortal.Util.Utils.SecondsToHMSString(tag.Duration));
        if (tag.Duration <= 0)
          strDuration = String.Empty;

        strText = GUILocalizeStrings.Get(435);	//	"Track"
        string strTrack = String.Format("{0}{1}", strText, tag.Track);
        if (tag.Track <= 0)
          strTrack = String.Empty;

        strText = GUILocalizeStrings.Get(436);	//	"Year"
        string strYear = String.Format("{0}{1}", strText, tag.Year);
        if (tag.Year <= 1900)
          strYear = String.Empty;

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
      get
      {
        return _isFocused;
      }
      set
      {
        _isFocused = value;
        if (_isFocused)
        {
          if (_videoWindow != null)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, (int)_videoWindow.GetID, 0, 0, null);
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

    MusicTag GetInfo(string fileName, out string thumb)
    {
      string skin = GUIGraphicsContext.Skin;
      thumb = String.Empty;
      MusicTag tag = null;
      Song song = new Song();
      bool bFound = false;
      MusicDatabase dbs = new MusicDatabase();
      bFound = dbs.GetSongByFileName(fileName, ref song);

      if (!bFound)
      {
        // no id3tag in the music database, check if we should re-scan for id3 tags
        if (_useID3)
        {
          //yes, then try reading the tag from the file
          tag = TagReader.TagReader.ReadTag(fileName);
        }
        if (tag == null)
        {
          // if we're playing a radio
          if (Recorder.IsRadio())
          {
            tag = new MusicTag();
            string cover = MediaPortal.Util.Utils.GetCoverArt(Thumbs.Radio, Recorder.RadioStationName());
            if (cover != String.Empty)
              thumb = cover;
            tag.Title = Recorder.RadioStationName();
          }
          if (g_Player.IsRadio)
          {
            // then check which radio station we're playing
            tag = new MusicTag();
            ArrayList stations = new ArrayList();
            RadioDatabase.GetStations(ref stations);
            string strFName = g_Player.CurrentFile;
            foreach (RadioStation station in stations)
            {
              string coverart;
              if (strFName.IndexOf(".radio") > 0)
              {
                string strChan = System.IO.Path.GetFileNameWithoutExtension(strFName);
                if (station.Frequency.ToString().Equals(strChan))
                {
                  // got it, check if it has a thumbnail
                  tag.Title = station.Name;
                  coverart = MediaPortal.Util.Utils.GetCoverArt(Thumbs.Radio, station.Name);
                  if (coverart != String.Empty)
                    thumb = coverart;
                }
              }
              else
              {
                if (station.URL.Equals(strFName))
                {
                  tag.Title = station.Name;
                  coverart = MediaPortal.Util.Utils.GetCoverArt(Thumbs.Radio, station.Name);
                  if (coverart != String.Empty)
                    thumb = coverart;
                }
              }
            } //foreach (RadioStation station in stations)
          } //if (g_Player.IsRadio)
        } //if (tag==null)

        // if all fail check playlist for information
        if (tag == null)
        {
          PlayListItem item = playlistPlayer.GetCurrentItem();
          if (item != null)
            tag = (MusicTag)item.MusicTag;
        }
      }// if (!bFound )
      else
      {
        // got the music tag
        tag = new MusicTag();
        tag.Album = song.Album;
        tag.Artist = song.Artist;
        tag.Duration = song.Duration;
        tag.Genre = song.Genre;
        tag.Title = song.Title;
        tag.Track = song.Track;
        tag.Year = song.Year;
      }
      if (tag != null)
      {
        if (tag.Album.Length > 0)
        {
          string strThumb = GUIMusicFiles.GetCoverArt(false, fileName, tag);          
          if (strThumb != String.Empty)
          {
            thumb = strThumb;
            // let us test if there is a larger cover art image
            string strLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(thumb);
            if (System.IO.File.Exists(strLarge))
            {
              thumb = strLarge;
            }
          }
        }
        else
        {
          // no album info but Artist is known
          if (tag.Artist.Length > 0)
          {
            //string strThumb = GUIMusicFiles.GetCoverArt(false, fileName, tag);
            string strThumb = Util.Utils.GetCoverArt(Thumbs.MusicArtists, tag.Artist);
            if (strThumb != String.Empty)
            {
              thumb = strThumb;
              // let us test if there is a larger cover art image
              string strLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(thumb);
              if (System.IO.File.Exists(strLarge))
              {
                thumb = strLarge;
              }
            }
          }
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
