using System;
using System.Drawing;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Player;
namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITVOverlay:GUIWindow
	{
    enum Controls
    {
      CONTROL_VIDEO_RECT=0,
      CONTROL_VIDEO_WINDOW=1,
        CONTROL_PLAYTIME		=2
      , CONTROL_PLAY_LOGO   =3
      , CONTROL_REC_LOGO  =4
      , CONTROL_INFO			  =5
      , CONTROL_BIG_PLAYTIME =6
      , CONTROL_FF_LOGO  =7
      , CONTROL_RW_LOGO  =8
    };

    //TVUtil m_util=null;
    public GUITVOverlay()
		{
		}

    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\tvOverlay.xml");
      GetID=(int)GUIWindow.Window.WINDOW_TV_OVERLAY;
      return bResult;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false;}
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
			return false;/*
      if (Recorder.IsRecording==false && Recorder.View==false) return  false;
			if (g_Player.Playing && !g_Player.IsTV) return false;
			if (g_Player.IsTVRecording) return false;

			if (m_util==null)
			{
				m_util=new TVUtil();
			}

	 		if (Recorder.View)
			{
				string strChannel=Recorder.TVChannelName;
				TVProgram prog=m_util.GetCurrentProgram(strChannel);
				if( prog!=null) 
				{
					SetProgramInfo(prog);
				}
			}
			else if (Recorder.IsRecording)
			{
				TVRecording rec=Recorder.CurrentTVRecording;
				TVProgram prog=Recorder.ProgramRecording;
				
				if (prog!=null)
				{
					SetProgramInfo(prog);
				}
				else
				{
					SetRecordingInfo(rec);
				}
			}

			if (GUIGraphicsContext.IsFullScreenVideo) return  false;
			if (GUIGraphicsContext.Calibrating) return  false;
			if (GUIGraphicsContext.Overlay==false) return false;

      return true;
*/    }

    public override void PostRender(int iLayer)
    {
      if (iLayer!=2) return;
      HideControl((int)Controls.CONTROL_PLAYTIME); 
      HideControl((int)Controls.CONTROL_PLAY_LOGO); 
      HideControl((int)Controls.CONTROL_REC_LOGO); 
      HideControl( (int)Controls.CONTROL_FF_LOGO); 
      HideControl( (int)Controls.CONTROL_RW_LOGO); 


			if (Recorder.View)
			{
				ShowControl( (int)Controls.CONTROL_PLAYTIME);
				ShowControl( (int)Controls.CONTROL_PLAY_LOGO); 
				ShowControl( (int)Controls.CONTROL_VIDEO_WINDOW);
			}
			else if (Recorder.IsRecording)
			{
				ShowControl( (int)Controls.CONTROL_PLAYTIME);
				ShowControl( (int)Controls.CONTROL_REC_LOGO); 
				HideControl( (int)Controls.CONTROL_VIDEO_WINDOW);
			}
      if (GUIGraphicsContext.ShowBackground) 
        ShowControl( (int)Controls.CONTROL_VIDEO_RECT);
      else
        HideControl( (int)Controls.CONTROL_VIDEO_RECT);
      base.Render();
    }

    
  
    void ShowControl(int iControl)
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_VISIBLE, GetID,0, iControl,0,0,null); 
      OnMessage(msg); 
    }

    void HideControl(int iControl)
    {
      GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_HIDDEN, GetID,0, iControl,0,0,null); 
      OnMessage(msg); 
    }

    void SetProgramInfo(TVProgram prog)
    {
      //channel/title/time/logo
      string strTime=String.Format("{0}-{1}", 
        prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
        prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));


			TimeSpan ts = DateTime.Now-prog.StartTime;
			GUIPropertyManager.SetProperty("#TV.current", Utils.SecondsToHMSString((int)ts.TotalSeconds));
			GUIPropertyManager.SetProperty("#TV.start", GUIPropertyManager.GetProperty("#TV.Record.start"));
			GUIPropertyManager.SetProperty("#TV.stop", GUIPropertyManager.GetProperty("#TV.Record.stop"));
      GUIPropertyManager.SetProperty("#channel", prog.Channel );
      GUIPropertyManager.SetProperty("#title", prog.Title );
      
      SetChannelLogo(prog.Channel);
			GUIPropertyManager.Changed=true;
    }

    void SetRecordingInfo(TVRecording rec)
    {
      string strTime=String.Format("{0}-{1}", 
        rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
        rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

      TimeSpan ts = DateTime.Now-rec.StartTime;
      GUIPropertyManager.SetProperty("#TV.current", Utils.SecondsToHMSString((int)ts.TotalSeconds));
			GUIPropertyManager.SetProperty("#TV.start", GUIPropertyManager.GetProperty("#TV.Record.start"));
			GUIPropertyManager.SetProperty("#TV.stop", GUIPropertyManager.GetProperty("#TV.Record.stop"));
      GUIPropertyManager.SetProperty("#channel", rec.Channel );
      GUIPropertyManager.SetProperty("#title", rec.Title );
      
			SetChannelLogo(rec.Channel);
			GUIPropertyManager.Changed=true;
    }


    void SetChannelLogo(string m_strChannel)
    {
      if (!Recorder.IsRecording)
      {
				GUIPropertyManager.SetProperty("#thumb","blue_rectangle_video.png");
      }
      else
      {
        string strLogo=Utils.GetCoverArt(GUITVHome.TVChannelCovertArt,m_strChannel);
        if (System.IO.File.Exists(strLogo))
        {
          GUIPropertyManager.SetProperty("#thumb",strLogo); 
        }
        else
        {
          GUIPropertyManager.SetProperty("#thumb","defaultVideoBig.png");
        }
      }
    }

	}
}
