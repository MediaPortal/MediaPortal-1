using System;
using MediaPortal.GUI.Library;
using MediaPortal.Video.Database;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.TV.Database;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class GUIVideoOverlay : GUIWindow
	{
    const string ThumbsFolder=@"thumbs\Videos\Title";
    string m_strFile = "";
    enum Controls
    {
			CONTROL_VIDEO_RECTANGLE = 0
      ,CONTROL_PLAYTIME = 2
      , CONTROL_PLAY_LOGO = 3
      , CONTROL_PAUSE_LOGO = 4
      , CONTROL_INFO = 5
      , CONTROL_BIG_PLAYTIME = 6
      , CONTROL_FF_LOGO = 7
      , CONTROL_RW_LOGO = 8
    };

    string TVChannelCovertArt=@"thumbs\tv\logos";
		public GUIVideoOverlay()
		{
		}

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\videoOverlay.xml");
      GetID = (int)GUIWindow.Window.WINDOW_VIDEO_OVERLAY;
      return bResult;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }    
    public override void PreInit()
    {
      AllocResources();
    
    }
    public override void Render()
    {
    }
    
    public override bool DoesPostRender()
    {
      if (!g_Player.Playing) 
      {
        m_strFile = String.Empty;
        return false;
      }
			if ( !(g_Player.IsRadio && g_Player.HasVideo))
			{
				if (!g_Player.IsVideo && !g_Player.IsDVD && !g_Player.IsTVRecording && !g_Player.IsTV)       
				{
					m_strFile = String.Empty;
					return false;
				}
			}

      if (g_Player.CurrentFile != m_strFile)
      {
        m_strFile = g_Player.CurrentFile;
        SetCurrentFile(m_strFile);
      }

			if (GUIGraphicsContext.IsFullScreenVideo) return false;
			if (GUIGraphicsContext.Calibrating) return false;
			if (!GUIGraphicsContext.Overlay) return false;

      return true;
    }
    
    public override void PostRender(int iLayer)
    {
      if (iLayer != 2) return;
      int iSpeed = g_Player.Speed;
			double dPos = g_Player.CurrentPosition;
      if (dPos < 5d)
      {
        if (iSpeed < 1)
        {
          iSpeed = 1;
          g_Player.Speed = iSpeed;
          g_Player.SeekAbsolute(0.0d);
        }
      }

      HideControl((int)Controls.CONTROL_PLAY_LOGO);
      HideControl((int)Controls.CONTROL_PAUSE_LOGO);
      HideControl((int)Controls.CONTROL_FF_LOGO);
      HideControl((int)Controls.CONTROL_RW_LOGO);
      if (g_Player.Paused)
      {
        ShowControl((int)Controls.CONTROL_PAUSE_LOGO);
      }
      else
      {
        iSpeed = g_Player.Speed;
        if (iSpeed > 1)
        {
          ShowControl((int)Controls.CONTROL_FF_LOGO);
        }
        else if (iSpeed < 0)
        {
          ShowControl((int)Controls.CONTROL_RW_LOGO);
        }
        else
        {
          ShowControl((int)Controls.CONTROL_PLAY_LOGO);
        }
      }

			if (GUIGraphicsContext.ShowBackground)
			{
				ShowControl((int)Controls.CONTROL_VIDEO_RECTANGLE);
			}
			else
			{
				HideControl((int)Controls.CONTROL_VIDEO_RECTANGLE);
			}
      base.Render();
    }

    
  
    void ShowControl(int iControl)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, GetID, 0, iControl, 0, 0, null);
      OnMessage(msg);
    }

    void HideControl(int iControl)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN, GetID, 0, iControl, 0, 0, null);
      OnMessage(msg);
    }

    /// <summary>
    /// Examines the current playing movie and fills in all the #tags for the skin.
    /// For movies it will look in the video database for any IMDB info
    /// For record TV programs it will look in the TVDatabase for recording info 
    /// </summary>
    /// <param name="strFile">Filename of the current playing movie</param>
    /// <remarks>
    /// Function will fill in the following tags for TV programs
    /// #title, #plot, #plotoutline #file, #thumb, #year, #channel,
    /// 
    /// Function will fill in the following tags for movies
    /// #title, #plot, #plotoutline #file, #thumb, #year
    /// #director, #cast, #dvdlabel, #imdbnumber, #plot, #plotoutline, #rating, #tagline, #votes, #credits
    /// </remarks>
    void SetCurrentFile(string strFile)
    {
      GUIPropertyManager.RemovePlayerProperties();
      GUIPropertyManager.SetProperty("#title", System.IO.Path.GetFileName(strFile));
      GUIPropertyManager.SetProperty("#file",System.IO.Path.GetFileName(strFile));
      GUIPropertyManager.SetProperty("#thumb","");

      if (g_Player.IsDVD)
      {
        // for dvd's the file is in the form c:\media\movies\the matrix\video_ts\video_ts.ifo
        // first strip the \video_ts\video_ts.ifo
        string lowPath=strFile.ToLower();
        int index=lowPath.IndexOf("video_ts/");
        if (index < 0) index=lowPath.IndexOf(@"video_ts\");
        if (index >=0)
        {
          strFile=strFile.Substring(0,index);
          strFile=Utils.RemoveTrailingSlash(strFile);

          // get the name by stripping the first part : c:\media\movies
          string strName=strFile;
          int pos=strFile.LastIndexOfAny( new char[] {'\\','/'} );
          if (pos>=0 && pos+1 < strFile.Length-1) strName=strFile.Substring(pos+1);
          GUIPropertyManager.SetProperty("#title", strName);
          GUIPropertyManager.SetProperty("#file", strName);

          // construct full filename as imdb info is stored...
          strFile+=@"\VIDEO_TS\VIDEO_TS.IFO";
        }
      }
      
      string strExt = System.IO.Path.GetExtension(strFile).ToLower();
      if (strExt.Equals(".sbe") || strExt.Equals(".dvr-ms") )
      {
        // this is a recorded movie.
        // check the TVDatabase for the description,genre,title,...
        TVRecorded recording = new TVRecorded();
        if (TVDatabase.GetRecordedTVByFilename(strFile, ref recording))
        {
          TimeSpan ts = recording.EndTime - recording.StartTime;
          string strTime = String.Format("{0} {1} ", 
                                Utils.GetShortDayString(recording.StartTime) , 
                                Utils.SecondsToHMString((int)ts.TotalSeconds));
          GUIPropertyManager.SetProperty("#title",recording.Title);
          GUIPropertyManager.SetProperty("#plot",recording.Description);
          GUIPropertyManager.SetProperty("#plotoutline",recording.Description);
          GUIPropertyManager.SetProperty("#genre", recording.Genre);
          GUIPropertyManager.SetProperty("#year",strTime);
          GUIPropertyManager.SetProperty("#channel",recording.Channel);
          string strLogo = Utils.GetCoverArt(TVChannelCovertArt,recording.Channel);
          if (!System.IO.File.Exists(strLogo))
          {
            strLogo = "defaultVideoBig.png";
          }
          GUIPropertyManager.SetProperty("#thumb", strLogo);
        }
        return;
      }

			IMDBMovie movieDetails = new IMDBMovie();
      bool bMovieInfoFound = false;
      
      if (VideoDatabase.HasMovieInfo(strFile))
      {
        VideoDatabase.GetMovieInfo(strFile, ref movieDetails);
        bMovieInfoFound = true;
      }
      if (bMovieInfoFound)
      {
        GUIPropertyManager.SetProperty("#title", movieDetails.Title);
        
        string wsGenre = GUILocalizeStrings.Get(174);
        string wsTmp = String.Format("{0} {1}", wsGenre, movieDetails.Genre);
        GUIPropertyManager.SetProperty("#genre",wsTmp);
        
        string wsYear = GUILocalizeStrings.Get(201);
        wsTmp = String.Format("{0} {1}", wsYear, movieDetails.Year);
        GUIPropertyManager.SetProperty("#year",wsTmp);
				
        string wsDirector = GUILocalizeStrings.Get(199);
        wsTmp = String.Format("{0} {1}", wsDirector, movieDetails.Director);
        GUIPropertyManager.SetProperty("#director",wsTmp);
				
        GUIPropertyManager.SetProperty("#cast",movieDetails.Cast);
        GUIPropertyManager.SetProperty("#dvdlabel",movieDetails.DVDLabel);
        GUIPropertyManager.SetProperty("#imdbnumber", movieDetails.IMDBNumber);
        GUIPropertyManager.SetProperty("#plot", movieDetails.Plot);
        GUIPropertyManager.SetProperty("#plotoutline", movieDetails.PlotOutline);
        GUIPropertyManager.SetProperty("#rating", movieDetails.Rating.ToString());
        GUIPropertyManager.SetProperty("#tagline", movieDetails.TagLine);
        GUIPropertyManager.SetProperty("#votes", movieDetails.Votes);
        GUIPropertyManager.SetProperty("#credits", movieDetails.WritingCredits);
        string strThumb;
        strThumb = Utils.GetCoverArt(ThumbsFolder,movieDetails.Title);
        if (System.IO.File.Exists(strThumb))
        {
          GUIPropertyManager.SetProperty("#thumb",strThumb);
        }
      }
      else
      {
        GUIListItem item = new GUIListItem();
        item.IsFolder = false;
        item.Path = strFile;
        Utils.SetThumbnails(ref item);
        GUIPropertyManager.SetProperty("#thumb",item.ThumbnailImage);
      }
    }
	}
}
