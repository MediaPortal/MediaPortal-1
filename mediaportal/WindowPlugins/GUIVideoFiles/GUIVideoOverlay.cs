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
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TV.Database;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Container for preview window - also setting video properties like title, playtime, etc for skin access
  /// </summary>
  public class GUIVideoOverlay : GUIOverlayWindow, IRenderLayer
  {
    private bool _isFocused = false;
    private string _fileName = "";
    private string _program = "";

    [SkinControl(0)] protected GUIImage _videoRectangle = null;
    [SkinControl(1)] protected GUIVideoControl _videoWindow = null;
    [SkinControl(2)] protected GUILabelControl _labelPlayTime = null;
    [SkinControl(3)] protected GUIImage _imagePlayLogo = null;
    [SkinControl(4)] protected GUIImage _imagePauseLogo = null;
    [SkinControl(5)] protected GUIFadeLabel _labelInfo = null;
    [SkinControl(6)] protected GUIImage _labelBigPlayTime = null;
    [SkinControl(7)] protected GUIImage _imageFastForward = null;
    [SkinControl(8)] protected GUIImage _imageRewind = null;

    private string _thumbLogo = "";
    private bool _didRenderLastTime = false;

    public GUIVideoOverlay()
    {
      GetID = (int) Window.WINDOW_VIDEO_OVERLAY;
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(OnVideoChanged);
    }

    ~GUIVideoOverlay()
    {
      GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(OnVideoChanged);
    }

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.Skin + @"\videoOverlay.xml");
      GetID = (int) Window.WINDOW_VIDEO_OVERLAY;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.VideoOverlay);
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
      if (!g_Player.Playing)
      {
        _fileName = string.Empty;
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }
      if ((g_Player.IsRadio || g_Player.IsMusic))
      {
        _fileName = string.Empty;
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }
      if (!g_Player.IsVideo && !g_Player.IsDVD && !g_Player.IsTVRecording && !g_Player.IsTV)
      {
        _fileName = string.Empty;
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }

      if (g_Player.CurrentFile != _fileName)
      {
        _fileName = g_Player.CurrentFile;
        SetCurrentFile(_fileName);
      }

      if (g_Player.IsTV && (_program != GUIPropertyManager.GetProperty("#TV.View.title")) && g_Player.IsTimeShifting)
      {
        _program = GUIPropertyManager.GetProperty("#TV.View.title");
        GUIPropertyManager.SetProperty("#Play.Current.Title", GUIPropertyManager.GetProperty("#TV.View.channel"));
        GUIPropertyManager.SetProperty("#Play.Current.Genre", _program);
        GUIPropertyManager.SetProperty("#Play.Current.Year", GUIPropertyManager.GetProperty("#TV.View.genre"));
        GUIPropertyManager.SetProperty("#Play.Current.Director",
                                       GUIPropertyManager.GetProperty("#TV.View.start") + " - " +
                                       GUIPropertyManager.GetProperty("#TV.View.stop"));
      }

      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // Too late to schedule a fade out animation. We are already rendering full screen video
        //OnUpdateState(false);
        //return base.IsAnimating(AnimationType.WindowClose);
        return false;
      }
      if (GUIGraphicsContext.Calibrating)
      {
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }
      if (!GUIGraphicsContext.Overlay)
      {
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }

      OnUpdateState(true);
      return true;
    }

    public override void PostRender(float timePassed, int iLayer)
    {
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

        int speed = g_Player.Speed;
        double pos = g_Player.CurrentPosition;
        if (_imagePlayLogo != null)
        {
          _imagePlayLogo.Visible = (g_Player.Paused == false);
        }

        if (_imagePauseLogo != null)
        {
          _imagePauseLogo.Visible = false; // (g_Player.Paused == true);
        }

        if (_imageFastForward != null)
        {
          _imageFastForward.Visible = false; // (g_Player.Speed>1);
        }

        if (_imageRewind != null)
        {
          _imageRewind.Visible = false; // (g_Player.Speed<0);
        }

        if (_videoRectangle != null)
        {
          if (g_Player.Playing)
          {
            _videoRectangle.Visible = GUIGraphicsContext.ShowBackground;
          }
          else
          {
            _videoRectangle.Visible = false;
          }
        }
      }
      base.Render(timePassed);
    }


    private void OnVideoChanged()
    {
      if (_videoWindow == null)
      {
        return;
      }

      if (GUIGraphicsContext.Overlay == true && GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.IsPlaying)
        //&& GUIGraphicsContext.IsPlayingVideo && !GUIGraphicsContext.IsFullScreenVideo && !g_Player.FullScreen)
      {
        if (_videoWindow.Visible == false)
        {
          _videoWindow.Visible = true;
        }
        return;
      }
      if (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.IsPlaying)
        // && GUIGraphicsContext.IsPlayingVideo && !GUIGraphicsContext.IsFullScreenVideo && !g_Player.FullScreen)
      {
        if (_videoWindow.Visible == true)
        {
          _videoWindow.Visible = false;
        }
        return;
      }
    }

    /// <summary>
    /// Examines the current playing movie and fills in all the #tags for the skin.
    /// For movies it will look in the video database for any IMDB info
    /// For record TV programs it will look in the TVDatabase for recording info 
    /// </summary>
    /// <param name="fileName">Filename of the current playing movie</param>
    /// <remarks>
    /// Function will fill in the following tags for TV programs
    /// #Play.Current.Title, #Play.Current.Plot, #Play.Current.PlotOutline #Play.Current.File, #Play.Current.Thumb, #Play.Current.Year, #Play.Current.Channel,
    /// 
    /// Function will fill in the following tags for movies
    /// #Play.Current.Title, #Play.Current.Plot, #Play.Current.PlotOutline #Play.Current.File, #Play.Current.Thumb, #Play.Current.Year
    /// #Play.Current.Director, #cast, #dvdlabel, #imdbnumber, #Play.Current.Plot, #Play.Current.PlotOutline, #rating, #tagline, #votes, #credits
    /// </remarks>
    private void SetCurrentFile(string fileName)
    {
      GUIPropertyManager.RemovePlayerProperties();
      GUIPropertyManager.SetProperty("#Play.Current.Title", Util.Utils.GetFilename(fileName));
      GUIPropertyManager.SetProperty("#Play.Current.File", Path.GetFileName(fileName));
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", "");
      GUIPropertyManager.SetProperty("#Play.Current.IsDIVX", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsXVID", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsH264", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsMP1V", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsMP2V", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsMP4V", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsWMV", string.Empty);

      GUIPropertyManager.SetProperty("#Play.Current.IsAC3", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsDTS", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsAAC", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsMP3", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsMP2A", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsWMA", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.IsPCM", string.Empty);

      if ((g_Player.IsVideo || g_Player.IsDVD) && !g_Player.IsTV)
      {
        // Set video format (only one possible per stream)
          if (g_Player.MediaInfo != null)
          {
            if (g_Player.MediaInfo.IsDIVX)
              GUIPropertyManager.SetProperty("#Play.Current.IsDIVX", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "DivX.png"));
            else
              if (g_Player.MediaInfo.IsXVID)
                GUIPropertyManager.SetProperty("#Play.Current.IsXVID", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "xvid.png"));
              else
                if (g_Player.MediaInfo.IsH264)
                  GUIPropertyManager.SetProperty("#Play.Current.IsH264", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "h264.png"));
                else
                  if (g_Player.MediaInfo.IsMP4V)
                    GUIPropertyManager.SetProperty("#Play.Current.IsMP4V", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "mp4v.png"));
                  else
                    if (g_Player.MediaInfo.IsMP1V)
                      GUIPropertyManager.SetProperty("#Play.Current.IsMP1V", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "mp1v.png"));
                    else
                      if (g_Player.MediaInfo.IsMP2V)
                        GUIPropertyManager.SetProperty("#Play.Current.IsMP2V", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "mp2v.png"));
                      else
                        if (g_Player.MediaInfo.IsWMV)
                          GUIPropertyManager.SetProperty("#Play.Current.IsWMV", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "wmv.png"));

        // Set audio format (only one possible per stream)
        if (g_Player.MediaInfo.IsMP3)
          GUIPropertyManager.SetProperty("#Play.Current.IsMP3", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "mp3.png"));
        else
          if (g_Player.MediaInfo.IsMP2A)
            GUIPropertyManager.SetProperty("#Play.Current.IsMP2A", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "mp2a.png"));
          else
            if (g_Player.MediaInfo.IsAC3)
              GUIPropertyManager.SetProperty("#Play.Current.IsAC3", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "ac3.png"));
            else
              if (g_Player.MediaInfo.IsDTS)
                GUIPropertyManager.SetProperty("#Play.Current.IsDTS", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "dts.png"));
              else
                if (g_Player.MediaInfo.IsAAC)
                  GUIPropertyManager.SetProperty("#Play.Current.IsAAC", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "AAC.png"));
                else
                  if (g_Player.MediaInfo.IsWMA)
                    GUIPropertyManager.SetProperty("#Play.Current.IsWMA", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "wma.png"));
                  else
                    if (g_Player.MediaInfo.IsPCM)
                      GUIPropertyManager.SetProperty("#Play.Current.IsPCM", string.Format("{0}{1}{2}", GUIGraphicsContext.Skin, @"\Media\Logos\", "pcm.png"));  
          }
      }        

      if (g_Player.IsDVD)
      {
        // for dvd's the file is in the form c:\media\movies\the matrix\video_ts\video_ts.ifo
        // first strip the \video_ts\video_ts.ifo
        string lowPath = fileName.ToLower();
        int index = lowPath.IndexOf("video_ts/");
        if (index < 0)
        {
          index = lowPath.IndexOf(@"video_ts\");
        }
        if (index >= 0)
        {
          fileName = fileName.Substring(0, index);
          fileName = Util.Utils.RemoveTrailingSlash(fileName);

          // get the name by stripping the first part : c:\media\movies
          string strName = fileName;
          int pos = fileName.LastIndexOfAny(new char[] {'\\', '/'});
          if (pos >= 0 && pos + 1 < fileName.Length - 1)
          {
            strName = fileName.Substring(pos + 1);
          }
          GUIPropertyManager.SetProperty("#Play.Current.Title", strName);
          GUIPropertyManager.SetProperty("#Play.Current.File", strName);

          // construct full filename as imdb info is stored...
          fileName += @"\VIDEO_TS\VIDEO_TS.IFO";
        }
      }

      bool isLive = g_Player.IsTimeShifting;
      string extension = Path.GetExtension(fileName).ToLower();
      if (extension.Equals(".sbe") || extension.Equals(".dvr-ms") ||
          (extension.Equals(".ts") && !isLive || g_Player.IsTVRecording))
      {
        // this is a recorded movie.
        // check the TVDatabase for the description,genre,title,...
        TVRecorded recording = new TVRecorded();
        if (TVDatabase.GetRecordedTVByFilename(fileName, ref recording))
        {
          TimeSpan ts = recording.EndTime - recording.StartTime;
          string time = String.Format("{0} {1} ",
                                      Util.Utils.GetShortDayString(recording.StartTime),
                                      Util.Utils.SecondsToHMString((int) ts.TotalSeconds));
          GUIPropertyManager.SetProperty("#Play.Current.Title", recording.Title);
          GUIPropertyManager.SetProperty("#Play.Current.Plot", recording.Title + "\n" + recording.Description);
          GUIPropertyManager.SetProperty("#Play.Current.PlotOutline", recording.Description);
          GUIPropertyManager.SetProperty("#Play.Current.Genre", recording.Genre);
          GUIPropertyManager.SetProperty("#Play.Current.Year", time);
          GUIPropertyManager.SetProperty("#Play.Current.Channel", recording.Channel);
          string logo = Util.Utils.GetCoverArt(Thumbs.TVChannel, recording.Channel);
          if (!File.Exists(logo))
          {
            logo = "defaultVideoBig.png";
          }
          GUIPropertyManager.SetProperty("#Play.Current.Thumb", logo);
          _thumbLogo = logo;
          return;
        }
        else if (g_Player.currentTitle != "")
        {
          GUIPropertyManager.SetProperty("#Play.Current.Title", g_Player.currentTitle);
          GUIPropertyManager.SetProperty("#Play.Current.Plot",
                                         g_Player.currentTitle + "\n" + g_Player.currentDescription);
          GUIPropertyManager.SetProperty("#Play.Current.PlotOutline", g_Player.currentDescription);
        }
      }

      /*if (fileName.Substring(0, 4) == "rtsp")
      {
          GUIPropertyManager.SetProperty("#Play.Current.Title", g_Player.currentTitle);
          GUIPropertyManager.SetProperty("#Play.Current.Plot", g_Player.currentTitle + "\n" + g_Player.currentDescription);
          GUIPropertyManager.SetProperty("#Play.Current.PlotOutline", g_Player.currentDescription);
      }*/


      IMDBMovie movieDetails = new IMDBMovie();
      bool bMovieInfoFound = false;

      if (VideoDatabase.HasMovieInfo(fileName))
      {
        VideoDatabase.GetMovieInfo(fileName, ref movieDetails);
        bMovieInfoFound = true;
      }
      else if (File.Exists(Path.ChangeExtension(fileName, ".xml")))
      {
        MatroskaTagInfo info = MatroskaTagHandler.Fetch(Path.ChangeExtension(fileName, ".xml"));
        movieDetails.Title = info.title;
        movieDetails.Plot = info.description;
        movieDetails.Genre = info.genre;
        GUIPropertyManager.SetProperty("#Play.Current.Channel", info.channelName);
        string logo = Util.Utils.GetCoverArt(Thumbs.TVChannel, info.channelName);
        if (!File.Exists(logo))
        {
          logo = "defaultVideoBig.png";
        }
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", logo);
        _thumbLogo = logo;
        bMovieInfoFound = true;
      }
      if (bMovieInfoFound)
      {
        movieDetails.SetPlayProperties();
      }
      else if (g_Player.IsTV && g_Player.IsTimeShifting)
      {
        GUIPropertyManager.SetProperty("#Play.Current.Title", GUIPropertyManager.GetProperty("#TV.View.channel"));
        GUIPropertyManager.SetProperty("#Play.Current.Genre", GUIPropertyManager.GetProperty("#TV.View.title"));
      }
      else
      {
        GUIListItem item = new GUIListItem();
        item.IsFolder = false;
        item.Path = fileName;
        Util.Utils.SetThumbnails(ref item);
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", item.ThumbnailImage);
      }
      _thumbLogo = GUIPropertyManager.GetProperty("#Play.Current.Thumb");
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