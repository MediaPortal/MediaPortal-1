using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
namespace MediaPortal.GUI.TV
{
	/// <summary>v
	/// Summary description for Class1.
	/// </summary>
	public class  GUITVHome : GUIWindow, ISetupForm
	{
		enum Controls
		{
			BTN_TVGUIDE=2,
      BTN_RECORD=3,
      BTN_CARD=4,
      BTN_CHANNEL=6,
      BTN_TVONOFF=7,
      BTN_TIMESHIFTINGONOFF=8,
			BTN_SCHEDULER=9,
			BTN_RECORDINGS=10,
			VIDEO_WINDOW=99,
      IMG_CURRENT_CHANNEL=12,
      LABEL_PROGRAM_TITLE=13,
      LABEL_PROGRAM_TIME=14,
      LABEL_PROGRAM_DESCRIPTION=15,
      
      IMG_REC_CHANNEL=21,
      LABEL_REC_INFO=22,
      IMG_REC_RECTANGLE=23,
      IMG_REC_PIN=24

		};
    static public string TVChannelCovertArt=@"thumbs\tv\logos";
		string          m_strChannel="Nederland 1";
		bool            m_bTVON=true;
    bool            m_bTimeShifting=true;
    ArrayList       m_channels=new ArrayList();
    TVUtil          m_util =null;
    DateTime        m_updateTimer=DateTime.Now;
    bool            m_bAlwaysTimeshift=false;
    static int      m_iCurrentCard=0;
    ArrayList       m_recordings=new ArrayList();

		public  GUITVHome()
		{	
			GetID=(int)GUIWindow.Window.WINDOW_TV;
		}
    ~GUITVHome()
    {	
      
    }
		public override bool Init()
		{
      System.IO.Directory.CreateDirectory(@"thumbs\tv");
      System.IO.Directory.CreateDirectory(@"thumbs\tv\logos");
      bool bResult= Load (GUIGraphicsContext.Skin+@"\mytvhome.xml");
      LoadSettings();
      return bResult;
		}

    
    #region Serialisation
    void LoadSettings()
    {
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        m_strChannel=xmlreader.GetValueAsString("mytv","channel","");
        m_bTVON=xmlreader.GetValueAsBool("mytv","tvon",true);
        m_bTimeShifting=xmlreader.GetValueAsBool("mytv","timeshifting",true);
        m_bAlwaysTimeshift   = xmlreader.GetValueAsBool("mytv","alwaystimeshift",false);
      }
    }

    void SaveSettings()
    {
      using (AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("mytv","channel",m_strChannel);
        xmlwriter.SetValueAsBool("mytv","tvon",m_bTVON);
        xmlwriter.SetValueAsBool("mytv","timeshifting",m_bTimeShifting);
      }
    }
    #endregion


		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_PREVIOUS_MENU:
				{
          
          if (! g_Player.Playing)
          {
            if (GUIGraphicsContext.ShowBackground)
            {
              // stop timeshifting & viewing... 
              
              Recorder.StopViewing();
            }
          }
          GUIWindowManager.PreviousWindow();
					return;
				}

        case Action.ActionType.ACTION_SHOW_GUI:
          if ( Recorder.IsCardViewing(m_iCurrentCard) )
          {
            StartPlaying(false);
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
          }
        break;
			}
			base.OnAction(action);
		}

		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          base.OnMessage(message);
          TVDatabase.GetRecordings(ref m_recordings);
					if (g_Player.Playing && !g_Player.IsTV)
					{
						g_Player.Stop();
					}
          m_util= new TVUtil();
          if (Recorder.IsCardViewing(m_iCurrentCard)||Recorder.IsCardTimeShifting(m_iCurrentCard) )
          {
            m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard);
          }
          else if (Recorder.IsCardRecording(m_iCurrentCard) )
          {
            m_strChannel=Recorder.GetTVRecording(m_iCurrentCard).Channel;
          }
          
          
          GUIControl.ClearControl(GetID,(int)Controls.BTN_CARD);
          for (int x=0; x < Recorder.Count; ++x)
          {
              string CardName =Recorder.GetFriendlyNameForCard(x);
              GUIControl.AddItemLabelControl(GetID,(int)Controls.BTN_CARD,CardName );
          }
          if (Recorder.Count<=1)
            GUIControl.DisableControl(GetID,(int)Controls.BTN_CARD);

          if (m_iCurrentCard<Recorder.Count)
          {
            GUIControl.SelectItemControl(GetID,(int)Controls.BTN_CARD,m_iCurrentCard);
          }
					
          TVDatabase.GetChannels(ref m_channels);
          GUIControl.ClearControl(GetID,(int)Controls.BTN_CHANNEL);
          int i=0;
          if (m_channels.Count>0)
          {
            foreach (TVChannel chan in m_channels)
            {
              GUIControl.AddItemLabelControl(GetID,(int)Controls.BTN_CHANNEL,chan.Name);
              ++i;
            }
          }
          // start viewing... 
          Recorder.StartViewing(m_iCurrentCard,m_strChannel, m_bTVON, m_bTimeShifting);
          m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
          m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
			
					GUIControl cntl = GetControl( (int)Controls.VIDEO_WINDOW);
					if (cntl!=null)
					{
						GUIGraphicsContext.IsFullScreenVideo=false;
						GUIGraphicsContext.VideoWindow = new Rectangle(cntl.XPosition,cntl.YPosition,cntl.Width,cntl.Height);
					}
          
          UpdateButtons();
          UpdateCurrentProgram();
          UpdateChannel();

          return true;
				}

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          m_recordings.Clear();
          base.OnMessage(message);
          m_util=null;
          
          SaveSettings();

          return true;
				}
				
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					int iControl=message.SenderControlId;

          if (iControl==(int)Controls.BTN_TVONOFF)
          {
            if (message.Param1==0) 
            {
              m_bTVON=false;
              SaveSettings();
              g_Player.Stop();
            }
            else
            {
              m_bTVON=true;
              SaveSettings();
            }

            // turn tv on/off
            Recorder.StartViewing(m_iCurrentCard, m_strChannel, m_bTVON,m_bTimeShifting);
            
            m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
            m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
            StartPlaying(true);
          }

          if (iControl==(int)Controls.BTN_TIMESHIFTINGONOFF)
          {
            if (message.Param1==0) 
            {
              //turn timeshifting off 
              m_bTimeShifting=false;
            }
            else
            {
              //turn timeshifting on 
              m_bTimeShifting=true;
            }
            SaveSettings();
            Recorder.StartViewing(m_iCurrentCard, Recorder.GetTVChannelName(m_iCurrentCard), m_bTVON,m_bTimeShifting);

            
            m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
            m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
          }
          
					if (iControl==(int)Controls.BTN_CHANNEL)
					{
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            GUIGraphicsContext.SendMessage(msg);         
            if (msg.Label.Length>0)
            {
              m_strChannel=msg.Label;
              UpdateButtons();
              UpdateCurrentProgram();
              UpdateChannel();
              Recorder.StartViewing(m_iCurrentCard,m_strChannel , m_bTVON,m_bTimeShifting);
              SaveSettings();
              m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard);
              m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
              m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
            }
					}
          if (iControl == (int)Controls.BTN_CARD)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            GUIGraphicsContext.SendMessage(msg);
            
            m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard);
            m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
            m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
              
            m_iCurrentCard=msg.Param1;
            if (!Recorder.IsCardViewing(m_iCurrentCard))
            {
              Recorder.StartViewing(m_iCurrentCard,m_strChannel , m_bTVON,m_bTimeShifting);
            }

            UpdateButtons();
            UpdateCurrentProgram();
            UpdateChannel();
          }

          if (iControl == (int)Controls.BTN_RECORD)
          {
            if (!Recorder.IsCardRecording(m_iCurrentCard))
            {
              Recorder.RecordNow(m_strChannel);
            }
            else
            {
              Recorder.StopRecording(m_iCurrentCard);

              // re-start viewing.... 
              LoadSettings();
              Recorder.StartViewing(m_iCurrentCard, Recorder.GetTVChannelName(m_iCurrentCard), m_bTVON,m_bTimeShifting);

              m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard);
              m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
              m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
            }
            UpdateButtons();
          }
        break;

        case GUIMessage.MessageType.GUI_MSG_RESUME_TV:
        {
          LoadSettings();

          //restart viewing...  
          Recorder.StartViewing(m_iCurrentCard, m_strChannel, m_bTVON,m_bTimeShifting);
          
          m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard);
          m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
          m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);

          StartPlaying(true);
        }
        break;
			}
			return base.OnMessage(message);
		}

    void UpdateButtons()
    {
      if (Recorder.IsCardRecording(m_iCurrentCard))
      {
        GUIControl.DisableControl(GetID,(int)Controls.BTN_TVONOFF);
        GUIControl.DisableControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
        GUIControl.SelectControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
        GUIControl.SetControlLabel(GetID, (int)Controls.BTN_RECORD, GUILocalizeStrings.Get(629));
      }
      else
      {
        GUIControl.EnableControl(GetID,(int)Controls.BTN_TVONOFF);
        GUIControl.SetControlLabel(GetID, (int)Controls.BTN_RECORD, GUILocalizeStrings.Get(601));
      
        bool supportstimeshifting=Recorder.DoesCardSupportTimeshifting(m_iCurrentCard);
        if (m_bTVON==false || supportstimeshifting==false)
        {
          GUIControl.DisableControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
          GUIControl.DeSelectControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
        }
        else if (supportstimeshifting)
        {
          GUIControl.EnableControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);

          if ( Recorder.IsCardTimeShifting(m_iCurrentCard) )
          {
            GUIControl.SelectControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
          }
          else
          {
            GUIControl.DeSelectControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
          }
        }
        if (m_bTVON)
          GUIControl.SelectControl(GetID,(int)Controls.BTN_TVONOFF);
        else
          GUIControl.DeSelectControl(GetID,(int)Controls.BTN_TVONOFF);

      }
    }
    void UpdateChannel()
    {
      int i=0;
      int iSelected=-1;
      if (m_channels.Count>0)
      {
        foreach (TVChannel chan in m_channels)
        {
          GUIControl.AddItemLabelControl(GetID,(int)Controls.BTN_CHANNEL,chan.Name);
          if (chan.Name==m_strChannel) iSelected=i;
          ++i;
        }
        if (iSelected==-1)
        {
          iSelected=0;
        }
      }
      else iSelected=0;
      GUIControl.SelectItemControl(GetID,(int)Controls.BTN_CHANNEL,iSelected);
		}


      
    void UpdateCurrentProgram()
    {
      int iStep=0;
      try
      {
        if (m_util!=null)
        {
          TVProgram prog=m_util.GetCurrentProgram(m_strChannel);
          iStep=1;
          if (prog!=null) 
          {
            iStep=2;
            TimeSpan ts=prog.EndTime-prog.StartTime;
            double iTotalSecs=ts.TotalSeconds;
            ts=DateTime.Now-prog.StartTime;
            double iCurSecs=ts.TotalSeconds;
            double fPercent = ((double)iCurSecs) / ((double)iTotalSecs);
            fPercent *=100.0d;
            iStep=3;
            GUIPropertyManager.SetProperty("#TV.View.Percentage", ((int)fPercent).ToString());
            iStep=4;
          }
          iStep=5;
        }
      }
      catch (Exception)
      {
        Log.Write("grrrr:{0}",iStep);
      }
    }

    public override void Process()
    {
      if (g_Player.Playing && g_Player.DoesOwnRendering) return;
      System.Threading.Thread.Sleep(50);
      if (!g_Player.Playing)
      {
        StartPlaying(true);
      }


      //show/hide 'record pin' if current program is being recorded or not
      bool bRecording=false;
      TVProgram program=m_util.GetCurrentProgram(m_strChannel);
      if (program!=null)
      {
        foreach (TVRecording record in m_recordings)
        {
          if (record.IsRecordingProgram(program) ) 
          {
            bRecording=true;
            break;
          }
        }
      }

      if (bRecording)
      {
        GUIControl.ShowControl(GetID,(int)Controls.IMG_REC_PIN);
      }
      else
      {
        GUIControl.HideControl(GetID,(int)Controls.IMG_REC_PIN);
      }

      // if previewing, then set current channel to channel we're previewing
      // if recording, then set current channel to channel we're recording and ask player to
      // start playing the recording
      if (Recorder.IsCardViewing(m_iCurrentCard))
      {
        if (!m_strChannel.Equals(Recorder.GetTVChannelName(m_iCurrentCard) ))
        {
          Log.Write("Previewing channel changed");
          m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard) ;
          UpdateButtons();
          UpdateCurrentProgram();
          UpdateChannel();
        }
      }
      else if (Recorder.IsCardRecording(m_iCurrentCard))
      {
        if (!m_strChannel.Equals(Recorder.GetTVRecording(m_iCurrentCard) .Channel))
        {
          m_strChannel=Recorder.GetTVRecording(m_iCurrentCard).Channel;
          UpdateButtons();
          UpdateCurrentProgram();
          UpdateChannel();
        }
      }


      TimeSpan ts = DateTime.Now-m_updateTimer;
      if (ts.TotalMilliseconds>500)
      {
        m_updateTimer=DateTime.Now;
        if (Recorder.IsCardRecording(m_iCurrentCard))
        {  
          GUIControl.ShowControl(GetID, (int)Controls.LABEL_REC_INFO);
          GUIControl.ShowControl(GetID, (int)Controls.IMG_REC_RECTANGLE);
          GUIControl.ShowControl(GetID, (int)Controls.IMG_REC_CHANNEL);
        }
        else 
        {
          GUIControl.HideControl(GetID, (int)Controls.LABEL_REC_INFO);
          GUIControl.HideControl(GetID, (int)Controls.IMG_REC_RECTANGLE);
          GUIControl.HideControl(GetID, (int)Controls.IMG_REC_CHANNEL);
        }

        UpdateCurrentProgram();
        
        UpdateButtons();
      }
    }


    void StartPlaying(bool bCheckOnOffButton)
    {
      if (bCheckOnOffButton)
      {
        if (!m_bTVON) return;
      }
      
      
      if (Recorder.IsCardTimeShifting(m_iCurrentCard) )
      {
        string strFileName=Recorder.GetTimeShiftFileName(m_iCurrentCard);
      
        if (!g_Player.Playing || g_Player.IsTV==false || g_Player.CurrentFile != strFileName)
        {
          if (System.IO.File.Exists(strFileName))
          {
            g_Player.Play(strFileName);
          }
          else 
          {
            m_bTVON=false;
          }
        }
      }
    }

    static public int GetCurrentCard()
    {
      return m_iCurrentCard;
    }
      
		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

		public string PluginName()
		{
			return "My TV";
		}

		public bool DefaultEnabled()
		{
			return true;
		}

		public int GetWindowId()
		{
			return GetID;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			// TODO:  Add GUITVHome.GetHome implementation
			strButtonText = GUILocalizeStrings.Get(605);
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "";
			return true;
		}

		public string Author()
		{
			return "Frodo";
		}

		public string Description()
		{
			return "My TV plugin for watching & recording tv";
		}

    public bool HasSetup()
    {
      return false;
    }
		public void ShowPlugin()
		{
		}
		#endregion
	}
}
