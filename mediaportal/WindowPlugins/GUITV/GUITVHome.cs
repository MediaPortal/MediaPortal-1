using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
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
			BTN_GROUP=6,
			BTN_CHANNEL=7,
			BTN_TVONOFF=8,
			BTN_TIMESHIFTINGONOFF=9,
			BTN_SCHEDULER=10,
			BTN_RECORDINGS=11,
			BTN_SEARCH=12,
			BTN_TELETEXT=13,
			VIDEO_WINDOW=99,
      
			IMG_REC_CHANNEL=21,
			LABEL_REC_INFO=22,
			IMG_REC_RECTANGLE=23,
			IMG_REC_PIN=24

		};
		static public string 	TVChannelCovertArt=@"thumbs\tv\logos";
		static public string 	TVShowCovertArt=@"thumbs\tv\shows";
		static public string 	m_strChannel="Nederland 1";
		static public string 	m_strGroup=GUILocalizeStrings.Get(972);
		static bool     			m_bTVON=true;
		static bool     			m_bTimeShifting=true;
		
		ArrayList       			m_channels=new ArrayList();
		ArrayList       			m_groups=new ArrayList();
		TVUtil          			m_util =null;
		DateTime        			m_updateTimer=DateTime.Now;
		bool            			m_bAlwaysTimeshift=false;
		static TVGroup				currentGroup=null;
		ArrayList       			m_recordings=new ArrayList();
		DateTime						  dtlastTime=DateTime.Now;
		public TVGroup CurrentGroupSelected
		{
			get 
			{
				return currentGroup;
			}
		}
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
				m_strGroup=xmlreader.GetValueAsString("mytv","group",GUILocalizeStrings.Get(972));
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
				xmlwriter.SetValue("mytv","group",m_strGroup);
			}
		}
		#endregion


		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_RECORD:
					//record current program on current channel
					//are we watching tv?
					if (Recorder.IsViewing() || Recorder.IsTimeShifting())
					{
						string channel=Recorder.GetTVChannelName();
						//yes, are we recording this channel already ?
						if (!Recorder.IsRecordingChannel(channel))
						{
							GUIDialogMenuBottomRight pDlgOK	= (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
							if (pDlgOK!=null)
							{
								pDlgOK.SetHeading(605);//my tv
								pDlgOK.AddLocalizedString(875); //current program
								pDlgOK.AddLocalizedString(876); //till manual stop
								pDlgOK.DoModal(this.GetID);
								switch (pDlgOK.SelectedId)
								{
									case 875:
										//record current program
										Recorder.RecordNow(channel,false);
										break;

									case 876:
										//manual record
										Recorder.RecordNow(channel,true);
										break;
								}
							}
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
							// No, then stop viewing... 
							Recorder.StopViewing();
						}
					}
					GUIWindowManager.PreviousWindow();
					return;
				}

				case Action.ActionType.ACTION_SHOW_GUI:
					//switch to fullscreen TV
					if ( Recorder.IsViewing())
					{
						//if we're watching tv
						StartPlaying(false);
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					if (g_Player.Playing && g_Player.IsTVRecording)
					{
						//if we're watching a tv recording
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
						if (!g_Player.IsTVRecording)
						{
							Log.Write("TVHome:stop music/video:{0}",g_Player.CurrentFile);
							g_Player.Stop();
						}
					}
					m_util= new TVUtil();

					//if current card is watching tv then use that channel
					if (Recorder.IsViewing() || Recorder.IsTimeShifting() )
					{
						m_strChannel=Recorder.GetTVChannelName();
					}
					else if (Recorder.IsRecording() ) // else if current card is recording, then use that channel
					{
						m_strChannel=Recorder.GetTVRecording().Channel;
					}
					else if (Recorder.IsAnyCardRecording() ) // else if any card is recording
					{
						//then get & use that channel
						for (int i=0; i < Recorder.Count;++i)
						{
							if (Recorder.Get(i).IsRecording)
							{
								m_strChannel=Recorder.Get(i).CurrentTVRecording.Channel;
								break;
							}
						}
					}


					//add all groups to the group selection button					
					TVDatabase.GetGroups(ref m_groups);
					TVDatabase.GetChannels(ref m_channels);
          
					//set video window position
					GUIControl cntl = GetControl( (int)Controls.VIDEO_WINDOW);
					if (cntl!=null)
					{
						GUIGraphicsContext.VideoWindow = new Rectangle(cntl.XPosition,cntl.YPosition,cntl.Width,cntl.Height);
					}
					UpdateChannelButton();

					// start viewing tv... 
					GUIGraphicsContext.IsFullScreenVideo=false;
					ViewChannel(m_strChannel);

					UpdateStateOfButtons();
					UpdateProgressPercentageBar();

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
							Log.Write("TVHome:turn tv off");
							m_bTVON=false;
							SaveSettings();
							g_Player.Stop();
						}
						else
						{
							// tv on
							Log.Write("TVHome:turn tv on {0}",m_strChannel);
							m_bTVON=true;
							SaveSettings();
						}

						// turn tv on/off
						ViewChannelAndCheck( m_strChannel);
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
						
						ViewChannelAndCheck( m_strChannel);
					}
          
					if (iControl==(int)Controls.BTN_GROUP)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						if ((msg.Label.Length>0) && (m_strGroup!=msg.Label))
						{
							m_strChannel="";
							m_strGroup=msg.Label;
							UpdateChannelButton();
							ViewChannelAndCheck(m_strChannel);
							SaveSettings();

							UpdateStateOfButtons();
							UpdateProgressPercentageBar();
						}
					}
					if (iControl==(int)Controls.BTN_CHANNEL)
					{
						//switch to another tv channel
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						if ((msg.Label.Length>0) && (m_strChannel!=msg.Label))
						{
							m_strChannel=msg.Label;
							UpdateStateOfButtons();
							UpdateProgressPercentageBar();
							UpdateChannelButton();
							ViewChannelAndCheck(m_strChannel );
							SaveSettings();
						}
					}
					if (iControl == (int)Controls.BTN_TELETEXT)
					{
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
					}
					if (iControl == (int)Controls.BTN_RECORD)
					{
						//record now.
						//Are we recording this channel already?
						if (!Recorder.IsRecordingChannel(m_strChannel))
						{
							//no then start recording
							GUIDialogMenuBottomRight pDlgOK	= (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
							if (pDlgOK!=null)
							{
								pDlgOK.SetHeading(605);//my tv
								pDlgOK.AddLocalizedString(875); //current program
								pDlgOK.AddLocalizedString(876); //till manual stop
								pDlgOK.DoModal(this.GetID);
								switch (pDlgOK.SelectedId)
								{
									case 875:
										//record current program
										Recorder.RecordNow(m_strChannel,false);
										break;

									case 876:
										//manual record
										Recorder.RecordNow(m_strChannel,true);
										break;
								}
							}
						}
						else
						{
							if (Recorder.IsRecording())
							{
								//yes then stop recording
								Recorder.StopRecording();

								// and re-start viewing.... 
								LoadSettings();
								ViewChannel(m_strChannel);
							}
						}
						UpdateStateOfButtons();
					}
					break;

					case GUIMessage.MessageType.GUI_MSG_RESUME_TV:
					{
						LoadSettings();

						//restart viewing...  
						ViewChannel( m_strChannel);
					}
					break;
					case GUIMessage.MessageType.GUI_MSG_RECORDER_VIEW_CHANNEL:
						ViewChannel(message.Label);
						break;

					case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_VIEWING:
						m_bTVON=false;
						ViewChannel(message.Label);
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
			if (Recorder.IsRecording())
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
				bool supportstimeshifting=Recorder.DoesSupportTimeshifting();
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
					if ( Recorder.IsTimeShifting() )
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
			if (Recorder.HasTeletext())
			{
				GUIControl.ShowControl(GetID,(int)Controls.BTN_TELETEXT);
			}
			else
			{
				GUIControl.HideControl(GetID,(int)Controls.BTN_TELETEXT);
			}
			int i=0;
			int iSelected=-1;
			currentGroup=null;
			if (m_groups.Count>0)
			{
				GUIControl.ClearControl(GetID, (int)Controls.BTN_GROUP);
				TVGroup tvgroup = new TVGroup();
				tvgroup.GroupName=GUILocalizeStrings.Get(972); //all channels
					
				foreach (TVChannel channel in m_channels)
					tvgroup.tvChannels.Add(channel );
				m_groups.Add(tvgroup);

				foreach (TVGroup group in m_groups)
				{
					GUIControl.AddItemLabelControl(GetID,(int)Controls.BTN_GROUP,group.GroupName);
					if (group.GroupName==m_strGroup) 
					{
						currentGroup=group;
						iSelected=i;
					}
					++i;
				}
				if (iSelected==-1)
				{
					currentGroup=(TVGroup)m_groups[0];
					iSelected=0;
				}
			}
			else iSelected=0;

			GUIControl.SelectItemControl(GetID,(int)Controls.BTN_GROUP,iSelected);

			if (currentGroup!=null)
			{
				i=0;
				iSelected=-1;
				if (currentGroup.tvChannels.Count>0)
				{
					GUIControl.ClearControl(GetID, (int)Controls.BTN_CHANNEL);
					foreach (TVChannel chan in currentGroup.tvChannels)
					{
						GUIControl.AddItemLabelControl(GetID,(int)Controls.BTN_CHANNEL,chan.Name);
						if (chan.Name==m_strChannel) iSelected=i;
						++i;
					}
					if (iSelected==-1)
					{
						iSelected=0;
						m_strChannel=((TVChannel)currentGroup.tvChannels[0]).Name;
					}
				}
				else iSelected=0;

				GUIControl.SelectItemControl(GetID,(int)Controls.BTN_CHANNEL,iSelected);
				return;
			}

			iSelected=-1;
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
			TimeSpan ts;
			if (!g_Player.Playing)
			{
				//then try to start it
				ts=DateTime.Now-dtlastTime;
				if (ts.TotalMilliseconds>=1000)
				{
					dtlastTime=DateTime.Now;
					string fileName=Recorder.GetTimeShiftFileName();
					try
					{
						if (System.IO.File.Exists(fileName))
						{
							FileInfo info = new FileInfo(fileName);
							if (info.Length >= 100*1024)
							{
								StartPlaying(true);
							}
						}
					}
					catch(Exception){}
				}
			}

			// if we're watching tv, then set current channel to tv channel we're watching
			if (Recorder.IsViewing())
			{
				//we're watching tv. Did the tv channel change?
				if (!m_strChannel.Equals(Recorder.GetTVChannelName() ))
				{
					//yes then update GUI
					Log.Write("Previewing channel changed");
					m_strChannel=Recorder.GetTVChannelName() ;
					UpdateStateOfButtons();
					UpdateProgressPercentageBar();
					UpdateChannelButton();
				}
			}
      
			// if we're recording tv, then set current channel to tv channel we're recording 
			if (Recorder.IsRecording())
			{
				//we're recording. Did the tvchannel change?
				if (!m_strChannel.Equals(Recorder.GetTVRecording() .Channel))
				{
					//yes then update GUI
					m_strChannel=Recorder.GetTVRecording().Channel;
					UpdateStateOfButtons();
					UpdateProgressPercentageBar();
					UpdateChannelButton();
				}
				TVRecording rec=Recorder.GetTVRecording();
				if (rec!= null)
				{
					GUIImage img = (GUIImage) GetControl((int)Controls.IMG_REC_PIN);
					if (rec.RecType != TVRecording.RecordingType.Once)
						img.SetFileName("tvguide_recordserie_button.png");
					else
						img.SetFileName("tvguide_record_button.png");
				}				
				GUIControl.ShowControl(GetID,(int)Controls.IMG_REC_PIN);
			}
			else
			{
				GUIControl.HideControl(GetID,(int)Controls.IMG_REC_PIN);
			}
			ts = DateTime.Now-m_updateTimer;
			if (ts.TotalMilliseconds>500)
			{
				m_updateTimer=DateTime.Now;

				GUIControl.HideControl(GetID, (int)Controls.LABEL_REC_INFO);
				GUIControl.HideControl(GetID, (int)Controls.IMG_REC_RECTANGLE);
				GUIControl.HideControl(GetID, (int)Controls.IMG_REC_CHANNEL);
				UpdateProgressPercentageBar();
        
				UpdateStateOfButtons();
			}

		}


		/// <summary>
		/// This method will try playing the timeshifting file
		/// </summary>
		/// <param name="bCheckOnOffButton">check state of tv on/off button</param>
		static void StartPlaying(bool bCheckOnOffButton)
		{
			if (bCheckOnOffButton)
			{
				//if tv is off then do nothing
				if (!m_bTVON) return;
			}
      
			// if we're not timeshifting then do nothing
			if (!Recorder.IsTimeShifting() ) return;
      
			//get the timeshifting filename
			string strFileName=Recorder.GetTimeShiftFileName();
    
			//if we're not playing this file yet
			if (!g_Player.Playing || g_Player.IsTV==false || g_Player.CurrentFile != strFileName)
			{
				// and it exists
				if (System.IO.File.Exists(strFileName))
				{
					//then play it
					Log.Write("GUITVHome.StartPlaying() Play:{0}",strFileName);
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
		/// When called this method will switch to the previous TV channel
		/// </summary>
		static public void OnPreviousChannel()
		{

			string strChannel;
			strChannel=Recorder.TVChannelName;

			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				// where in fullscreen so delayzap channel instead of immediatly tune..
				GUIFullScreenTV	TVWindow = (GUIFullScreenTV) GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
				if (TVWindow != null) TVWindow.ZapPreviousChannel();
				return;
			}
			if(currentGroup!=null)
			{

				// get current channel name
				for (int i=0; i < currentGroup.tvChannels.Count;++i)
				{
					TVChannel chan=(TVChannel)currentGroup.tvChannels[i];
					if (String.Compare(chan.Name,strChannel,true)==0 )
					{
						//select previous channel
						int iPrev=i-1;
						if (iPrev<0)iPrev=currentGroup.tvChannels.Count-1;
						chan=(TVChannel)currentGroup.tvChannels[iPrev];

						// where in TVHome screen, so switch immediatly
						ViewChannel(chan.Name) ;
						return;
					}

				
					return;
				}

				ArrayList m_channels=new ArrayList();
				TVDatabase.GetChannels(ref m_channels);
			
				// get current channel name
				for (int i=0; i < m_channels.Count;++i)
				{
					TVChannel chan=(TVChannel)m_channels[i];
					if (String.Compare(chan.Name,strChannel,true)==0 )
					{
						//select previous channel
						int iPrev=i-1;
						if (iPrev<0) iPrev=m_channels.Count-1;
						chan=(TVChannel)m_channels[iPrev];
						// where in TVHome screen, so switch immediatly
						ViewChannel(chan.Name) ;
						return;					
					}
				}
			}
		}

		static public void ViewChannelAndCheck(string channel)
		{
			if (g_Player.Playing && g_Player.IsTVRecording) return;
			ViewChannel(channel);
			if (Recorder.TVChannelName!=channel)
			{
				GUIDialogOK pDlgOK	= (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (pDlgOK!=null)
				{
					pDlgOK.SetHeading(605);//my tv
					pDlgOK.SetLine(1,977);//there is no free card available
					pDlgOK.SetLine(2,978);//which can watch this channel
					pDlgOK.DoModal((int)GUIWindow.Window.WINDOW_TV);
				}
			}
		}
		static public void ViewChannel(string channel)
		{
			if (g_Player.Playing && g_Player.IsTVRecording) return;
			Recorder.StartViewing( channel, m_bTVON, m_bTimeShifting) ;
			if (Recorder.IsViewing())
			{
				m_strChannel=Recorder.GetTVChannelName();
			}
			m_bTimeShifting=Recorder.IsTimeShifting();
			m_bTVON=m_bTimeShifting||Recorder.IsViewing();

			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				GUIFullScreenTV	TVWindow = (GUIFullScreenTV) GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
				if (TVWindow != null) TVWindow.UpdateOSD();
			}
			StartPlaying(false);

		}

		/// <summary>
		/// When called this method will switch to the next TV channel
		/// </summary>
		static public void OnNextChannel()
		{

			string strChannel;
			strChannel=Recorder.TVChannelName;

			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				// where in fullscreen so delayzap channel instead of immediatly tune..
				GUIFullScreenTV	TVWindow = (GUIFullScreenTV) GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
				if (TVWindow != null) 
				{
					TVWindow.ZapNextChannel();
				}
				return;
			}
			if(currentGroup!=null)
			{

				// get current channel name
				for (int i=0; i < currentGroup.tvChannels.Count;++i)
				{
					TVChannel chan=(TVChannel)currentGroup.tvChannels[i];
					if (String.Compare(chan.Name,strChannel,true)==0 )
					{
						//select next channel
						int iNext=i+1;
						if (iNext>currentGroup.tvChannels.Count-1) iNext=0;
						chan=(TVChannel)currentGroup.tvChannels[iNext];

						// where in TVHome screen, so switch immediatly
						ViewChannel(chan.Name) ;
						return;
					}

				}
				return;
			}
			ArrayList m_channels=new ArrayList();
			TVDatabase.GetChannels(ref m_channels);
			
			// get current channel name
			for (int i=0; i < m_channels.Count;++i)
			{
				TVChannel chan=(TVChannel)m_channels[i];
				if (String.Compare(chan.Name,strChannel,true)==0 )
				{
					//select next channel
					int iNext=i+1;
					if (iNext>m_channels.Count-1) iNext=0;
					chan=(TVChannel)m_channels[iNext];
					// where in TVHome screen, so switch immediatly
					ViewChannel(chan.Name);
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
			if (windowId== (int)GUIWindow.Window.WINDOW_TELETEXT) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT) return true;

			return false;
		}
		static public TVGroup CurrentGroup
		{
			get
			{
				return currentGroup;
			}
			set
			{
				currentGroup=value;
				if (currentGroup!=null)
					m_strGroup=currentGroup.GroupName;
				else m_strGroup=GUILocalizeStrings.Get(972);
			}
		}
		static public bool IsTVOn
		{
			get { return m_bTVON;}
			set { m_bTVON=value;}
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
