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
			try
			{
      System.IO.Directory.CreateDirectory(@"thumbs\tv");
      System.IO.Directory.CreateDirectory(@"thumbs\tv\logos");
			}
			catch(Exception){}
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
        case Action.ActionType.ACTION_RECORD:
					//record current program on current channel
          int card=GetCurrentCard();

					//are we watching tv?
          if (Recorder.IsCardViewing(card) || Recorder.IsCardTimeShifting(card))
          {
						//yes, are we not recording yet
            if (!Recorder.IsCardRecording(card))
            {
							//nop, then record current program
              string channel=Recorder.GetTVChannelName(card);
              Recorder.RecordNow(channel);
            }
          }
        break;

        case Action.ActionType.ACTION_PREV_CHANNEL:
          GUITVHome.OnPreviousChannel();
        break;
        
        case Action.ActionType.ACTION_NEXT_CHANNEL:
          GUITVHome.OnNextChannel();
          break;

				case Action.ActionType.ACTION_PREVIOUS_MENU:
				{
					// goto home 
          // are we watching tv & doing timeshifting
          if (! g_Player.Playing)
          {
						//yes, do we want tv as background
            if (GUIGraphicsContext.ShowBackground)
            {
              // No, then stop timeshifting & viewing... 
              Recorder.StopViewing();
            }
          }
          GUIWindowManager.PreviousWindow();
					return;
				}

        case Action.ActionType.ACTION_SHOW_GUI:
					//switch to fullscreen TV
					//but only if we're watching tv
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

					//if we're already watching tv or recording, then get the current tv channel
          if (Recorder.IsCardViewing(m_iCurrentCard)||Recorder.IsCardTimeShifting(m_iCurrentCard) )
          {
            m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard);
          }
          else if (Recorder.IsCardRecording(m_iCurrentCard) )
          {
            m_strChannel=Recorder.GetTVRecording(m_iCurrentCard).Channel;
          }
          
          //add all tv capture cards to the cards selection button
          GUIControl.ClearControl(GetID,(int)Controls.BTN_CARD);
          for (int x=0; x < Recorder.Count; ++x)
          {
              string CardName =Recorder.GetFriendlyNameForCard(x);
              GUIControl.AddItemLabelControl(GetID,(int)Controls.BTN_CARD,CardName );
          }
					//if we only have 1 tv capture card then disable the cards selection button
          if (Recorder.Count<=1)
            GUIControl.DisableControl(GetID,(int)Controls.BTN_CARD);

					// set card button to current selected card
          if (m_iCurrentCard<Recorder.Count)
          {
            GUIControl.SelectItemControl(GetID,(int)Controls.BTN_CARD,m_iCurrentCard);
          }
					
					//add all tv channels to the channel selection button
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
					// if no channel selected then set channel button to the first
					// channel found
          if (m_strChannel==String.Empty && m_channels.Count>0)
          {
            foreach (TVChannel channel in m_channels)
            {
              if (channel.Number<10000)
              {
                m_strChannel=channel.Name;
                break;
              }
            }
          }
			
					//set video window position
					GUIControl cntl = GetControl( (int)Controls.VIDEO_WINDOW);
					if (cntl!=null)
					{
						GUIGraphicsContext.VideoWindow = new Rectangle(cntl.XPosition,cntl.YPosition,cntl.Width,cntl.Height);
					}

          // start viewing tv... 
					GUIGraphicsContext.IsFullScreenVideo=false;
					Recorder.StartViewing(m_iCurrentCard,m_strChannel, m_bTVON, m_bTimeShifting);

          m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
          m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
          
          UpdateStateOfButtons();
          UpdateProgressPercentageBar();
          UpdateChannelButton();

          return true;
				}

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          m_recordings.Clear();
          base.OnMessage(message);
          m_util=null;
          
          SaveSettings();
					//if we're switching to another plugin
					if ( !GUITVHome.IsTVWindow(message.Param1) )
					{
						//and we're not playing which means we dont timeshift tv
						if (! g_Player.Playing)
						{
							// and we dont want tv in the background
							if (GUIGraphicsContext.ShowBackground)
							{
								// then stop timeshifting & viewing... 
								Recorder.StopViewing();
							}
						}
					}

          return true;
				}
				
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					int iControl=message.SenderControlId;

          if (iControl==(int)Controls.BTN_TVONOFF)
          {
						//switch tv on/off
            if (message.Param1==0) 
            {
							//tv off
              m_bTVON=false;
              SaveSettings();
              g_Player.Stop();
            }
            else
            {
							// tv on
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
						//turn timeshifting on/off
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
						//switch to another tv channel
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            OnMessage(msg);         
            if (msg.Label.Length>0)
            {
              m_strChannel=msg.Label;
              UpdateStateOfButtons();
              UpdateProgressPercentageBar();
              UpdateChannelButton();
              Recorder.StartViewing(m_iCurrentCard,m_strChannel , m_bTVON,m_bTimeShifting);
              SaveSettings();
              m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard);
              m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
              m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
            }
					}
          if (iControl == (int)Controls.BTN_CARD)
          {
						//switch to another tv capture card
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
            OnMessage(msg);
            
            m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard);
            m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
            m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
              
            m_iCurrentCard=msg.Param1;
            if (!Recorder.IsCardViewing(m_iCurrentCard))
            {
              Recorder.StartViewing(m_iCurrentCard,m_strChannel , m_bTVON,m_bTimeShifting);
            }

            UpdateStateOfButtons();
            UpdateProgressPercentageBar();
            UpdateChannelButton();
          }

          if (iControl == (int)Controls.BTN_RECORD)
          {
						//record now.

						//Are we recording already?
            if (!Recorder.IsCardRecording(m_iCurrentCard))
            {
							//no then start recording
              Recorder.RecordNow(m_strChannel);
            }
            else
            {
							//yes then stop recording
              Recorder.StopRecording(m_iCurrentCard);

              // and re-start viewing.... 
              LoadSettings();
              Recorder.StartViewing(m_iCurrentCard, Recorder.GetTVChannelName(m_iCurrentCard), m_bTVON,m_bTimeShifting);

              m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard);
              m_bTimeShifting=Recorder.IsCardTimeShifting(m_iCurrentCard);
              m_bTVON=m_bTimeShifting||Recorder.IsCardViewing(m_iCurrentCard);
            }
            UpdateStateOfButtons();
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

		/// <summary>
		/// Update the state of the following buttons
		/// - tv on/off
		/// - timeshifting on/off
		/// - record now
		/// </summary>
    void UpdateStateOfButtons()
    {
			//are we recording a tv program?
      if (Recorder.IsCardRecording(m_iCurrentCard))
      {
				//yes then disable the tv on/off and timeshifting on/off buttons
				//and change the Record Now button into Stop Record
        GUIControl.DisableControl(GetID,(int)Controls.BTN_TVONOFF);
        GUIControl.DisableControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
        GUIControl.SelectControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
        GUIControl.SetControlLabel(GetID, (int)Controls.BTN_RECORD, GUILocalizeStrings.Get(629));
      }
      else
      {
				//nop. then enable the tv on/off button and change the Record Now button
				//to Record Now
        GUIControl.EnableControl(GetID,(int)Controls.BTN_TVONOFF);
        GUIControl.SetControlLabel(GetID, (int)Controls.BTN_RECORD, GUILocalizeStrings.Get(601));
      
				//is tv turned off or is the current card not supporting timeshifting
        bool supportstimeshifting=Recorder.DoesCardSupportTimeshifting(m_iCurrentCard);
        if (m_bTVON==false || supportstimeshifting==false)
        {
					//then disable the timeshifting button
          GUIControl.DisableControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
          GUIControl.DeSelectControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
        }
        else if (supportstimeshifting)
        {
					//enable the timeshifting button
          GUIControl.EnableControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);

					// set state of timeshifting button
          if ( Recorder.IsCardTimeShifting(m_iCurrentCard) )
          {
            GUIControl.SelectControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
          }
          else
          {
            GUIControl.DeSelectControl(GetID,(int)Controls.BTN_TIMESHIFTINGONOFF);
          }
        }

				//set state of TV on/off button
        if (m_bTVON)
          GUIControl.SelectControl(GetID,(int)Controls.BTN_TVONOFF);
        else
          GUIControl.DeSelectControl(GetID,(int)Controls.BTN_TVONOFF);
      }
    }

		// updates the channel button so it shows the currently selected tv channel
    void UpdateChannelButton()
    {
      int i=0;
      int iSelected=-1;
      if (m_channels.Count>0)
      {
				GUIControl.ClearControl(GetID, (int)Controls.BTN_CHANNEL);
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


		/// <summary>
		/// Update the the progressbar in the GUI which shows
		/// how much of the current tv program has elapsed
		/// </summary>
    void UpdateProgressPercentageBar()
    {
      int iStep=0;
      try
      {
        if (m_util!=null)
        {
					//get current tv program
          TVProgram prog=m_util.GetCurrentProgram(m_strChannel);
          if (prog!=null) 
          {
            TimeSpan ts=prog.EndTime-prog.StartTime;
            double iTotalSecs=ts.TotalSeconds;
            ts=DateTime.Now-prog.StartTime;
            double iCurSecs=ts.TotalSeconds;
            double fPercent = ((double)iCurSecs) / ((double)iTotalSecs);
            fPercent *=100.0d;
            GUIPropertyManager.SetProperty("#TV.View.Percentage", ((int)fPercent).ToString());
          }
        }
      }
      catch (Exception)
      {
        Log.Write("grrrr:{0}",iStep);
      }
    }

		/// <summary>
		/// this method is called periodicaly by MP
		/// as long as this window is shown
		/// It will check if anything has changed like
		/// tv channel switched or recording started/stopped
		/// and will update the GUI
		/// </summary>
    public override void Process()
    { 
			//if we're not playing the timeshifting file
      if (!g_Player.Playing)
      {
				//then try to start it
        StartPlaying(true);
      }

      // if we're watching tv, then set current channel to tv channel we're watching
      if (Recorder.IsCardViewing(m_iCurrentCard))
      {
				//we're watching tv. Did the tv channel change?
        if (!m_strChannel.Equals(Recorder.GetTVChannelName(m_iCurrentCard) ))
        {
					//yes then update GUI
          Log.Write("Previewing channel changed");
          m_strChannel=Recorder.GetTVChannelName(m_iCurrentCard) ;
          UpdateStateOfButtons();
          UpdateProgressPercentageBar();
          UpdateChannelButton();
        }
      }
      
			// if we're recording tv, then set current channel to tv channel we're recording 
			if (Recorder.IsCardRecording(m_iCurrentCard))
			{
				//we're recording. Did the tvchannel change?
				if (!m_strChannel.Equals(Recorder.GetTVRecording(m_iCurrentCard) .Channel))
				{
					//yes then update GUI
					m_strChannel=Recorder.GetTVRecording(m_iCurrentCard).Channel;
					UpdateStateOfButtons();
					UpdateProgressPercentageBar();
					UpdateChannelButton();
				}
				GUIControl.ShowControl(GetID,(int)Controls.IMG_REC_PIN);
			}
			else
			{
				GUIControl.HideControl(GetID,(int)Controls.IMG_REC_PIN);
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

        UpdateProgressPercentageBar();
        
        UpdateStateOfButtons();
      }
    }


		/// <summary>
		/// This method will try playing the timeshifting file
		/// </summary>
		/// <param name="bCheckOnOffButton">check state of tv on/off button</param>
    void StartPlaying(bool bCheckOnOffButton)
    {
      if (bCheckOnOffButton)
      {
				//if tv is off then do nothing
        if (!m_bTVON) return;
      }
      
      // if we're not timeshifting then do nothing
      if (!Recorder.IsCardTimeShifting(m_iCurrentCard) ) return;
      
			//get the timeshifting filename
      string strFileName=Recorder.GetTimeShiftFileName(m_iCurrentCard);
    
			//if we're not playing this file yet
      if (!g_Player.Playing || g_Player.IsTV==false || g_Player.CurrentFile != strFileName)
      {
				// and it exists
        if (System.IO.File.Exists(strFileName))
        {
					//then play it
          g_Player.Play(strFileName);
        }
        else 
        {
					// file does not exists. turn off tv
          m_bTVON=false;
        }
      }
    }

		/// <summary>
		/// Returns the current tv capture card number we're looking at
		/// </summary>
		/// <returns></returns>
    static public int GetCurrentCard()
    {
      return m_iCurrentCard;
    }

		/// <summary>
		/// When called this method will switch to the previous TV channel
		/// </summary>
		static public void OnPreviousChannel()
    {	
			ArrayList m_channels=new ArrayList();
      TVDatabase.GetChannels(ref m_channels);
      string strChannel=Recorder.TVChannelName;
      for (int i=0; i < m_channels.Count;++i)
      {
        TVChannel chan=(TVChannel)m_channels[i];
        if (String.Compare(chan.Name,strChannel,true)==0 )
        {
          int iPrev=i-1;
          if (iPrev<0) iPrev=m_channels.Count-1;
          chan=(TVChannel)m_channels[iPrev];
					
          int card=GUITVHome.GetCurrentCard();
          Recorder.StartViewing(card, chan.Name, Recorder.IsCardViewing(card), Recorder.IsCardTimeShifting(card)) ;

					if (GUIGraphicsContext.IsFullScreenVideo)
					{
						GUIFullScreenTV	TVWindow = (GUIFullScreenTV) GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
						if (TVWindow != null) TVWindow.UpdateOSD();
					}
					return;
        }
      }
    }
    
		/// <summary>
		/// When called this method will switch to the next TV channel
		/// </summary>
    static public void OnNextChannel()
    {
			// get list of all channels
      ArrayList m_channels=new ArrayList();
      TVDatabase.GetChannels(ref m_channels);

			// get current channel name
      string strChannel=Recorder.TVChannelName;
      for (int i=0; i < m_channels.Count;++i)
      {
        TVChannel chan=(TVChannel)m_channels[i];
        if (String.Compare(chan.Name,strChannel,true)==0 )
        {
					//select next channel
          int iNext=i+1;
          if (iNext>m_channels.Count-1) iNext=0;
          chan=(TVChannel)m_channels[iNext];

					//and view that
          int card=GUITVHome.GetCurrentCard();
          Recorder.StartViewing(card, chan.Name, Recorder.IsCardViewing(card), Recorder.IsCardTimeShifting(card)) ;
					
					if (GUIGraphicsContext.IsFullScreenVideo)
					{
						GUIFullScreenTV	TVWindow = (GUIFullScreenTV) GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
						if (TVWindow != null) TVWindow.UpdateOSD();
					}
          return;
        }
      }
    }

		/// <summary>
		/// Returns true if the specified window belongs to the my tv plugin
		/// </summary>
		/// <param name="windowId">id of window</param>
		/// <returns>
		/// true: belongs to the my tv plugin
		/// false: does not belong to the my tv plugin</returns>
		static public bool IsTVWindow(int windowId)
		{
			if (windowId== (int)GUIWindow.Window.WINDOW_TV) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TVFULLSCREEN) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TVGUIDE) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_RECORDEDTV) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_SCHEDULER) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_SEARCHTV) return true;
			return false;
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
