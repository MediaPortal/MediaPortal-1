/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
	public class GUIVideoOverlay : GUIOverlayWindow
	{
		bool m_bFocused=false; 
    string m_strFile = "";
		string m_strProgram = "";
    enum Controls
    {
			CONTROL_VIDEO_RECTANGLE = 0
			,CONTROL_VIDEO_WINDOW=1
		,CONTROL_PLAYTIME = 2
      , CONTROL_PLAY_LOGO = 3
      , CONTROL_PAUSE_LOGO = 4
      , CONTROL_INFO = 5
      , CONTROL_BIG_PLAYTIME = 6
      , CONTROL_FF_LOGO = 7
      , CONTROL_RW_LOGO = 8
    };

    
		string m_strThumb="";
		public GUIVideoOverlay()
		{
			GetID = (int)GUIWindow.Window.WINDOW_VIDEO_OVERLAY;
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
			base.PreInit();
      AllocResources();
    
    }
    public override void Render(float timePassed)
    {
    }
    
    public override bool DoesPostRender()
    {
      if (!g_Player.Playing) 
      {
        m_strFile = String.Empty;
        return false;
      }
			if ( (g_Player.IsRadio || g_Player.IsMusic))
      {
        m_strFile = String.Empty;
        return false;
      }
			if (!g_Player.IsVideo && !g_Player.IsDVD && !g_Player.IsTVRecording && !g_Player.IsTV)       
			{
				m_strFile = String.Empty;
				return false;
			}

      if (g_Player.CurrentFile != m_strFile)
      {
        m_strFile = g_Player.CurrentFile;
        SetCurrentFile(m_strFile);
      }

			if ( g_Player.IsTV && (m_strProgram!=GUIPropertyManager.GetProperty("#TV.View.title")) )
			{
				m_strProgram = GUIPropertyManager.GetProperty("#TV.View.title");
				GUIPropertyManager.SetProperty("#title", GUIPropertyManager.GetProperty("#TV.View.channel"));
				GUIPropertyManager.SetProperty("#genre", m_strProgram);
				GUIPropertyManager.SetProperty("#year", GUIPropertyManager.GetProperty("#TV.View.genre"));
				GUIPropertyManager.SetProperty("#director", GUIPropertyManager.GetProperty("#TV.View.start")+" - "+GUIPropertyManager.GetProperty("#TV.View.stop"));
			}

			if (GUIGraphicsContext.IsFullScreenVideo) return false;
			if (GUIGraphicsContext.Calibrating) return false;
			if (!GUIGraphicsContext.Overlay) return false;

      return true;
    }
    
    public override void PostRender(float timePassed,int iLayer)
    {
      if (iLayer != 2) return;
			if (GUIPropertyManager.GetProperty("#thumb") != m_strThumb)
			{
				m_strFile=g_Player.CurrentFile ;
				SetCurrentFile(m_strFile);
			}

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
      base.Render(timePassed);
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
          GUIPropertyManager.SetProperty("#plot",recording.Title+"\n"+recording.Description);
          GUIPropertyManager.SetProperty("#plotoutline",recording.Description);
          GUIPropertyManager.SetProperty("#genre", recording.Genre);
          GUIPropertyManager.SetProperty("#year",strTime);
          GUIPropertyManager.SetProperty("#channel",recording.Channel);
          string strLogo = Utils.GetCoverArt(Thumbs.TVChannel,recording.Channel);
          if (!System.IO.File.Exists(strLogo))
          {
            strLogo = "defaultVideoBig.png";
          }
					GUIPropertyManager.SetProperty("#thumb", strLogo);
					m_strThumb=strLogo;
					return;
        }
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
			 movieDetails.SetProperties();
		 }
		 else if (g_Player.IsTV)
		 {
			 GUIPropertyManager.SetProperty("#title", GUIPropertyManager.GetProperty("#TV.View.channel"));
			 GUIPropertyManager.SetProperty("#genre", GUIPropertyManager.GetProperty("#TV.View.title"));
		 }
		 else
		 {
			 GUIListItem item = new GUIListItem();
			 item.IsFolder = false;
			 item.Path = strFile;
			 Utils.SetThumbnails(ref item);
			 GUIPropertyManager.SetProperty("#thumb",item.ThumbnailImage);
		 }
		 m_strThumb=GUIPropertyManager.GetProperty("#thumb");
    }
		public override bool Focused
		{
			get 
			{ 
				return m_bFocused;
			}
			set 
			{
				m_bFocused=value;
				if (m_bFocused)
				{
					GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS,GetID, 0,(int)Controls.CONTROL_VIDEO_WINDOW,0,0,null);
					OnMessage(msg);
				}
        else
        {
          foreach (GUIControl control in controlList)
          {
            control.Focus=false;
          }
        }
			}
		}
		protected override bool ShouldFocus(Action action)
		{
			return (action.wID==Action.ActionType.ACTION_MOVE_DOWN);
		}

		public override void OnAction(Action action)
		{
			
			base.OnAction (action);
			if (action.wID==Action.ActionType.ACTION_MOVE_UP)
			{
				Focused=false;
			}
		}
	}
}
