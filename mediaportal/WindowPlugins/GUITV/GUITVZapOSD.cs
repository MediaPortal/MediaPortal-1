using System;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// 
  /// </summary>
  /// 

  public class GUITVZAPOSD: GUIWindow
  {
    enum Controls 
    {

	  LABEL_CURRENT_CHANNEL=35,
      LABEL_ONTV_NOW=36,
      LABEL_ONTV_NEXT=37,
      PROGRESS_BAR=1,
      REC_LOGO=39,
      LABEL_CURRENT_TIME=100,

  OSD_VIDEOPROGRESS =1
  , OSD_TIMEINFO =100
  , TV_LOGO=10
    };

	  bool m_bNeedRefresh=false;
    DateTime m_dateTime=DateTime.Now;
    TVUtil m_util=null;
    ArrayList m_channels = new ArrayList();

    public GUITVZAPOSD()
    {
		
    }

    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\tvZAPOSD.xml");
      GetID=(int)GUIWindow.Window.WINDOW_TVZAPOSD;
      return bResult;
    }


    public override bool SupportsDelayedLoad
    {
      get { return false;}
    }    

    public override void Render()
    {
      UpdateProgressBar();
      SetVideoProgress();			// get the percentage of playback complete so far
      Get_TimeInfo();				// show the time elapsed/total playing time
      base.Render();		// render our controls to the screen
    }
    void HideControl (int dwSenderId, int dwControlID) 
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_HIDDEN,GetID, dwSenderId, dwControlID,0,0,null); 
      OnMessage(msg); 
    }
    void ShowControl (int dwSenderId, int dwControlID) 
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_VISIBLE,GetID, dwSenderId, dwControlID,0,0,null); 
      OnMessage(msg); 
    }

    void FocusControl (int dwSenderId, int dwControlID, int dwParam) 
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_SETFOCUS,GetID, dwSenderId, dwControlID, dwParam,0,null); 
      OnMessage(msg); 
    }


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_SHOW_OSD:
        {
          return;
        }
		    

				case Action.ActionType.ACTION_NEXT_CHANNEL:
				{
					OnNextChannel();
					return;
				}
				
				case Action.ActionType.ACTION_PREV_CHANNEL:
				{
					OnPreviousChannel();
					return;
				}
			}

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:	// fired when OSD is hidden
        {
          //if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(true);
          // following line should stay. Problems with OSD not
          // appearing are already fixed elsewhere
          FreeResources();
          return true;
        }
		    

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:	// fired when OSD is shown
        {
          // following line should stay. Problems with OSD not
          // appearing are already fixed elsewhere
          AllocResources();
          // if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(false);
          ResetAllControls();							// make sure the controls are positioned relevant to the OSD Y offset
          m_bNeedRefresh=false;
          m_dateTime=DateTime.Now;
          m_channels.Clear();
          TVDatabase.GetChannels(ref m_channels);
          SetCurrentChannelLogo();
          return true;
        }
      		    

      }
      return base.OnMessage(message);
    }



    void SetVideoProgress()
    {
    
      if (g_Player.Playing)
      {
       //   double fPercentage=g_Player.CurrentPosition / g_Player.Duration;
       //       GUIProgressControl pControl = (GUIProgressControl)GetControl((int)Controls.OSD_VIDEOPROGRESS);
       //     if (null!=pControl) pControl.Percentage=(int)(100*fPercentage);			// Update our progress bar accordingly ...

        //int iValue=g_Player.Volume;
        //GUISliderControl pSlider = GetControl((int)Controls.OSD_VOLUMESLIDER) as GUISliderControl;
        //if (null!=pSlider) pSlider.Percentage=iValue;			// Update our progress bar accordingly ...
      }
    }

    void Get_TimeInfo()
    {
      if (m_util==null)
      {
        m_util=new TVUtil();
      }
      string strChannel=GetChannelName();
      string strTime=strChannel;
      TVProgram prog=m_util.GetCurrentProgram(strChannel);
      if( prog!=null) 
      {
     
        strTime=String.Format("{0}-{1}", 
          prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
          prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
      }
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0,(int)Controls.OSD_TIMEINFO,0,0,null);
      msg.Label=strTime; 
      OnMessage(msg);			// ask our label to update it's caption
    }

    void Handle_ControlSetting(int iControlID, long wID)
    {
    
      string strMovie=g_Player.CurrentFile;
      //      CVideoDatabase dbs;
      //    VECBOOKMARKS bookmarks;

    }

   

    public override void	 ResetAllControls()
    {
      //reset all
      bool bOffScreen=false;
      int iCalibrationY=GUIGraphicsContext.OSDOffset;
      int iTop = GUIGraphicsContext.OverScanTop;
      int iMin=0;

      foreach (CPosition pos in m_vecPositions)
      {
        pos.control.SetPosition((int)pos.XPos,(int)pos.YPos+iCalibrationY);
      }
      foreach (CPosition pos in m_vecPositions)
      {
        GUIControl pControl= pos.control;

        int dwPosY=pControl.YPosition;
        if (pControl.IsVisible)
        {
          if ( dwPosY < iTop)
          {
            int iSize=iTop-dwPosY;
            if ( iSize > iMin) iMin=iSize;
            bOffScreen=true;
          }
        }
      }
      if (bOffScreen) 
      {

        foreach (CPosition pos in m_vecPositions)
        {
          GUIControl pControl= pos.control;
          int dwPosX=pControl.XPosition;
          int dwPosY=pControl.YPosition;
          if ( dwPosY < (int)100)
          {
            dwPosY+=Math.Abs(iMin);
            pControl.SetPosition(dwPosX,dwPosY);
          }
        }
      }
      base.ResetAllControls();
    }


    public override bool NeedRefresh()
    {
      if (m_bNeedRefresh) 
      {
        m_bNeedRefresh=false;
        return true;
      }
      return false;
    }
    
    void OnPreviousChannel()
    {
      if (!Recorder.View) return;
      string strChannel=Recorder.TVChannelName;
      for (int i=0; i < m_channels.Count;++i)
      {
        TVChannel chan=(TVChannel)m_channels[i];
        if (strChannel==""||String.Compare(chan.Name,strChannel,true)==0 )
        {
          int iPrev=i-1;
          if (iPrev<0) iPrev=m_channels.Count-1;
          chan=(TVChannel)m_channels[iPrev];
					GUITVHome.ViewChannel(chan.Name);
					SetCurrentChannelLogo();
					m_dateTime = DateTime.Now; 
          return;
        }
      }
    }
    
    void OnNextChannel()
    {
      if (!Recorder.View) return;
      string strChannel=Recorder.TVChannelName;
      for (int i=0; i < m_channels.Count;++i)
      {
        TVChannel chan=(TVChannel)m_channels[i];
        if (strChannel==""||String.Compare(chan.Name,strChannel,true)==0 )
        {
          int iNext=i+1;
          if (iNext>m_channels.Count-1) iNext=0;
          chan=(TVChannel)m_channels[iNext];
          
          GUITVHome.ViewChannel(chan.Name) ;

					SetCurrentChannelLogo();
					m_dateTime = DateTime.Now; 
          return;
        }
      }
    }

		public void UpdateChannelInfo()
		{
			SetCurrentChannelLogo();
		}


    void SetCurrentChannelLogo()
    {
      string strChannel=GetChannelName();
      string strLogo=Utils.GetCoverArt(GUITVHome.TVChannelCovertArt,strChannel);
      if (System.IO.File.Exists(strLogo))
      {
		  
        GUIImage img=GetControl((int)Controls.TV_LOGO) as GUIImage;
        if (img!=null)
        {
          img.SetFileName(strLogo);
          //img.SetPosition(GUIGraphicsContext.OverScanLeft, GUIGraphicsContext.OverScanTop);
          m_bNeedRefresh=true;
        }
        ShowControl(GetID,(int)Controls.TV_LOGO);
      }
      else
      {
        HideControl(GetID,(int)Controls.TV_LOGO);
      }
      ShowPrograms();
    }

	  string GetChannelName()
	  {
		  string strChannel=Recorder.TVChannelName;
		  GUIFullScreenTV	TVWindow = (GUIFullScreenTV) GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);

		  if (!Recorder.View && Recorder.IsRecording())
		  {
			  strChannel=Recorder.CurrentTVRecording.Channel;
		  }
		  if (!(TVWindow.ZapChannel==""))
		  {
			  strChannel=TVWindow.ZapChannel;
		  }
      return strChannel;
    }
    void ShowPrograms()
    {
      GUITextControl cntlNow=GetControl((int)Controls.LABEL_ONTV_NOW) as GUITextControl;
      GUITextControl cntlNext=GetControl((int)Controls.LABEL_ONTV_NEXT) as GUITextControl;
      GUILabelControl cntlTime=GetControl((int)Controls.LABEL_CURRENT_TIME) as GUILabelControl;
      GUILabelControl cntlCurrentChannel=GetControl((int)Controls.LABEL_CURRENT_CHANNEL) as GUILabelControl;

      if (cntlNow!=null) cntlNow.EnableUpDown=false;
      if (cntlNext!=null) cntlNext.EnableUpDown=false;

      GUIMessage msg;

      // Set recorder status
      if (Recorder.IsRecordingChannel(GetChannelName()))
      {
        ShowControl(GetID, (int)Controls.REC_LOGO);
      }
      else
      {
        HideControl(GetID, (int)Controls.REC_LOGO);
      }

      if (cntlCurrentChannel!=null)
      {
        msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, cntlCurrentChannel.GetID,0,0,null); 
        msg.Label=GetChannelName();
        cntlCurrentChannel.OnMessage(msg);
      }

      if (cntlNow!=null)
      {
        msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID,0, cntlNow.GetID,0,0,null); 
        cntlNow.OnMessage(msg);
		
      }
      if (cntlNext!=null)
      {
        msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID,0, cntlNext.GetID,0,0,null); 
        cntlNext.OnMessage(msg);
      }

      TVUtil util = new TVUtil();
      TVProgram prog=util.GetProgramAt(GetChannelName(),m_dateTime);
      
      if (prog!=null)
      {
        string strTime=String.Format("{0}-{1}", 
          prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
          prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
        
        if (cntlTime!=null) 
        {
          msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, cntlTime.GetID,0,0,null); 
          msg.Label=strTime;
          cntlTime.OnMessage(msg);
        }
        
        strTime=String.Format("{0}", prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));        
        if (cntlNow!=null)
        {
          msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, cntlNow.GetID,0,0,null); 
          msg.Label=prog.Title;
          cntlNow.OnMessage(msg);
          GUIPropertyManager.SetProperty("#TV.View.start", strTime);
        }

        // next program
        prog=util.GetProgramAt(GetChannelName(),prog.EndTime.AddMinutes(1));
        if (prog!=null)
        {
          strTime=String.Format("{0} ", 
            prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
        
          if (cntlNext!=null)
          {
            msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, cntlNext.GetID,0,0,null); 
            msg.Label=prog.Title;
            cntlNext.OnMessage(msg);
			GUIPropertyManager.SetProperty("#TV.View.stop", strTime);
          }
        }
      }
      else if (cntlTime!=null)
      {
        msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID,0, cntlTime.GetID,0,0,null); 
        msg.Label="";
        cntlTime.OnMessage(msg);
      }
      UpdateProgressBar();
    }

	  void UpdateProgressBar()
	  {
		  double fPercent;
		  if (g_Player.Playing==false)
		  {
			  if (m_util==null)
			  {
				  m_util=new TVUtil();
			  }
			  TVProgram prog=m_util.GetCurrentProgram(GetChannelName());
			  if (prog==null) return;
			  string strTime=String.Format("{0}-{1}", 
				  prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
				  prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

			  TimeSpan ts=prog.EndTime-prog.StartTime;
			  double iTotalSecs=ts.TotalSeconds;
			  ts=DateTime.Now-prog.StartTime; 
			  double iCurSecs=ts.TotalSeconds;
			  fPercent = ((double)iCurSecs) / ((double)iTotalSecs);
			  fPercent *=100.0d;
			  GUIPropertyManager.SetProperty("#TV.View.Percentage", ((int)fPercent).ToString());

		  }
	  }
  }
}
  