using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using AMS.Profile;
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
//		static public string 	m_strChannel="Nederland 1";
//		static public string 	m_strGroup=GUILocalizeStrings.Get(972);
		static bool     			m_bTVON=true;
		static bool     			m_bTimeShifting=true;
		static ChannelNavigator		m_navigator;
		
//		ArrayList       			m_channels=new ArrayList();
//		ArrayList       			m_groups=new ArrayList();
		TVUtil          			m_util =null;
		DateTime        			m_updateTimer=DateTime.Now;
		bool            			m_bAlwaysTimeshift=false;
//		static TVGroup				currentGroup=null;
		ArrayList       			m_recordings=new ArrayList();
		DateTime						  dtlastTime=DateTime.Now;
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
			// Create the channel navigator (it will load groups and channels)
			m_navigator = new ChannelNavigator();
			LoadSettings();
			return bResult;
		}

    
		#region Serialisation
		void LoadSettings()
		{
			using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				m_navigator.LoadSettings(xmlreader);
				m_bTVON=xmlreader.GetValueAsBool("mytv","tvon",true);
				m_bTimeShifting=xmlreader.GetValueAsBool("mytv","timeshifting",true);
				m_bAlwaysTimeshift   = xmlreader.GetValueAsBool("mytv","alwaystimeshift",false);
			}
		}

		void SaveSettings()
		{
			using (AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				m_navigator.SaveSettings(xmlwriter);
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
					//are we watching tv?
					if (Recorder.IsViewing() || Recorder.IsTimeShifting())
					{
						string channel=Recorder.GetTVChannelName();
						//yes, are we recording this channel already ?
						if (!Recorder.IsRecordingChannel(channel))
						{
							TVProgram prog=m_util.GetCurrentProgram(channel);
							if (prog!=null)
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
							else
							{
								Recorder.RecordNow(channel,true);
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

					//set video window position
					GUIControl cntl = GetControl( (int)Controls.VIDEO_WINDOW);
					if (cntl!=null)
					{
						GUIGraphicsContext.VideoWindow = new Rectangle(cntl.XPosition,cntl.YPosition,cntl.Width,cntl.Height);
					}
					UpdateChannelButton();

					// start viewing tv... 
					GUIGraphicsContext.IsFullScreenVideo=false;
					ViewChannel(Navigator.CurrentChannel);

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
							Log.Write("TVHome:turn tv on {0}", Navigator.CurrentChannel);
							m_bTVON=true;
							SaveSettings();
						}

						// turn tv on/off
						ViewChannelAndCheck(Navigator.CurrentChannel);
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
						
						ViewChannelAndCheck(Navigator.CurrentChannel);
					}
          
					if (iControl==(int)Controls.BTN_GROUP)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						if ((msg.Label.Length > 0) && (Navigator.CurrentGroup.GroupName != msg.Label))
						{
							// Change current group and switch to first channel in group
							Navigator.SetCurrentGroup(msg.Label);
							if(Navigator.CurrentGroup.tvChannels.Count > 0) {
								TVChannel chan = (TVChannel)Navigator.CurrentGroup.tvChannels[0];
								ViewChannelAndCheck(chan.Name);
								Navigator.UpdateCurrentChannel();
							}

							UpdateStateOfButtons();
							UpdateProgressPercentageBar();
							UpdateChannelButton();
							SaveSettings();
						}
					}
					if (iControl==(int)Controls.BTN_CHANNEL)
					{
						//switch to another tv channel
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);         
						if ((msg.Label.Length > 0) && (Navigator.CurrentChannel != msg.Label))
						{
							ViewChannelAndCheck(msg.Label);
							Navigator.UpdateCurrentChannel();

							UpdateStateOfButtons();
							UpdateProgressPercentageBar();
							UpdateChannelButton();
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
						if (!Recorder.IsRecordingChannel(Navigator.CurrentChannel))
						{
							//no then start recording
							TVProgram prog=m_util.GetCurrentProgram(Navigator.CurrentChannel);
							if (prog!=null)
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
											Recorder.RecordNow(Navigator.CurrentChannel,false);
											break;

										case 876:
											//manual record
											Recorder.RecordNow(Navigator.CurrentChannel,true);
											break;
									}
								}
							}
							else
							{
								//manual record
								Recorder.RecordNow(Navigator.CurrentChannel,true);
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
								ViewChannel(Navigator.CurrentChannel);
								Navigator.UpdateCurrentChannel();
							}
						}
						UpdateStateOfButtons();
					}
					break;

					case GUIMessage.MessageType.GUI_MSG_RESUME_TV:
					{
						LoadSettings();

						//restart viewing...  
						ViewChannel(Navigator.CurrentChannel);
					}
					break;
					case GUIMessage.MessageType.GUI_MSG_RECORDER_VIEW_CHANNEL:
						ViewChannel(message.Label);
						Navigator.UpdateCurrentChannel();
						break;

					case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_VIEWING:
						m_bTVON=false;
						ViewChannel(message.Label);
						Navigator.UpdateCurrentChannel();
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
			// Update group button
			int i=0;
//			currentGroup=null;
			GUIControl.ClearControl(GetID, (int)Controls.BTN_GROUP);
			foreach (TVGroup group in Navigator.Groups)
			{
				GUIControl.AddItemLabelControl(GetID,(int)Controls.BTN_GROUP,group.GroupName);
				if (group == Navigator.CurrentGroup) 
				{
					GUIControl.SelectItemControl(GetID, (int)Controls.BTN_GROUP, i);
				}
				++i;
			}

			// Update channel button
			i=0;
			GUIControl.ClearControl(GetID, (int)Controls.BTN_CHANNEL);
			foreach (TVChannel chan in Navigator.CurrentGroup.tvChannels)
			{
				GUIControl.AddItemLabelControl(GetID,(int)Controls.BTN_CHANNEL,chan.Name);
				if (chan.Name==Navigator.CurrentChannel)
				{
					GUIControl.SelectItemControl(GetID, (int)Controls.BTN_CHANNEL, i);
				}
				++i;
			}
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
					TVProgram prog=m_util.GetCurrentProgram(Navigator.CurrentChannel);
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
								StartPlaying(true);
						}
					}
					catch(Exception){}
				}
			}

			// Let the navigator zap channel if needed
			Navigator.CheckChannelChange();

			// Update navigator with information from the Recorder
			// TODO: This should ideally be handled using events. Recorder should fire an event
			// when the current channel changes. This is a temporary workaround //Vic
			string currchan = Navigator.CurrentChannel;		// Remember current channel
			Navigator.UpdateCurrentChannel();
			bool channelchanged = currchan != Navigator.CurrentChannel;

			// Has the channel changed?
			if(channelchanged)
			{
				UpdateStateOfButtons();
				UpdateProgressPercentageBar();
				UpdateChannelButton();
			}
      
			// if we're recording tv, update gui with info
			if (Recorder.IsRecording())
			{
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
			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				// where in fullscreen so delayzap channel instead of immediatly tune..
				GUIFullScreenTV	TVWindow = (GUIFullScreenTV) GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
				if (TVWindow != null) TVWindow.ZapPreviousChannel();
				return;
			}

			// Zap to previous channel immediately
			Navigator.ZapToPreviousChannel(0);
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
			Log.Write("GUITVHome.ViewChannel(): View channel=" + channel);

			if (g_Player.Playing && g_Player.IsTVRecording) return;
			Recorder.StartViewing( channel, m_bTVON, m_bTimeShifting) ;
			if (Recorder.IsViewing())
			{
				Navigator.UpdateCurrentChannel();
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

			// Zap to next channel immediately
			Navigator.ZapToNextChannel(0);
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

		static public bool IsTVOn
		{
			get { return m_bTVON;}
			set { m_bTVON=value;}
		}

		/// <summary>
		/// Gets the channel navigator that can be used for channel zapping.
		/// </summary>
		static public ChannelNavigator Navigator
		{
			get { return m_navigator; }
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

	#region ChannelNavigator class

	/// <summary>
	/// Handles the logic for channel zapping. This is used by the different GUI modules in the TV section.
	/// </summary>
	public class ChannelNavigator
	{
		#region Private members

		private ArrayList	m_groups = new ArrayList(); // Contains all channel groups (including an "all channels" group)
		private int				m_currentgroup = 0;
		private string		m_currentchannel = string.Empty;
		private DateTime	m_zaptime;
		private long			m_zapdelay;
		private string		m_zapchannel = null;
		private int				m_zapgroup = -1;
		private string    m_strGroup="";
		#endregion

		#region Constructors

		public ChannelNavigator()
		{
			// Load all groups
			ArrayList groups = new ArrayList();
			TVDatabase.GetGroups(ref groups); // Put groups in a local variable to ensure the "All" group is first always

			// Add a group containing all channels
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels); // Load all channels
			TVGroup tvgroup = new TVGroup();
			tvgroup.GroupName = GUILocalizeStrings.Get(972); //all channels
			foreach (TVChannel channel in channels)
				tvgroup.tvChannels.Add(channel);
			m_groups.Add(tvgroup);

			m_groups.AddRange(groups); // Add rest of the groups to the end of the list
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Gets the channel that we currently watch.
		/// Returns empty string if there is no current channel.
		/// </summary>
		public string CurrentChannel
		{
			get { return m_currentchannel; }
		}

		/// <summary>
		/// Gets the currently active channel group.
		/// </summary>
		public TVGroup CurrentGroup
		{
			get { return (TVGroup) m_groups[m_currentgroup]; }
		}

		/// <summary>
		/// Gets the list of channel groups.
		/// </summary>
		public ArrayList Groups
		{
			get { return m_groups; }
		}

		/// <summary>
		/// Gets the channel that we will zap to. Contains the current channel if not zapping to anything.
		/// </summary>
		public string ZapChannel
		{
			get
			{
				if (m_zapchannel == null)
					return m_currentchannel;
				return m_zapchannel;
			}
		}

		/// <summary>
		/// Gets the group that we will zap to. Contains the current group name if not zapping to anything.
		/// </summary>
		public string ZapGroupName
		{
			get
			{
				if(m_zapgroup == -1)
					return CurrentGroup.GroupName;
				return ((TVGroup)m_groups[m_zapgroup]).GroupName;
			}
		}
		#endregion

		#region Public methods

		/// <summary>
		/// Checks if it is time to zap to a different channel. This is called during Process().
		/// </summary>
		public void CheckChannelChange()
		{
			// Zapping to another group or channel?
			if (m_zapgroup != -1 || m_zapchannel != null)
			{
				// Time to zap?
				TimeSpan ts = DateTime.Now - m_zaptime;
				if (ts.TotalMilliseconds > m_zapdelay)
				{
					// Zapping to another group?
					if(m_zapgroup != -1)
					{
						// Change current group and zap to the first channel of the group
						m_currentgroup = m_zapgroup;
						m_strGroup=CurrentGroup.GroupName;
						if(CurrentGroup.tvChannels.Count > 0) 
						{
							TVChannel chan = (TVChannel)CurrentGroup.tvChannels[0];
							m_zapchannel = chan.Name;
						}
						m_zapgroup = -1;
					}

					// Zap to desired channel
					GUITVHome.ViewChannel(m_zapchannel);
					m_zapchannel = null;
				}
			}
		}

		/// <summary>
		/// Changes the current channel group.
		/// </summary>
		/// <param name="groupname">The name of the group to change to.</param>
		public void SetCurrentGroup(string groupname)
		{
			m_currentgroup = GetGroupIndex(groupname);
			m_strGroup=groupname;
		}

		/// <summary>
		/// Ensures that the navigator has the correct current channel (retrieved from the Recorder).
		/// </summary>
		public void UpdateCurrentChannel()
		{
			//if current card is watching tv then use that channel
			if (Recorder.IsViewing() || Recorder.IsTimeShifting())
			{
				m_currentchannel = Recorder.GetTVChannelName();
			}
			else if (Recorder.IsRecording())
			{ // else if current card is recording, then use that channel
				m_currentchannel = Recorder.GetTVRecording().Channel;
			}
			else if (Recorder.IsAnyCardRecording())
			{ // else if any card is recording
				//then get & use that channel
				for (int i = 0; i < Recorder.Count; ++i)
				{
					if (Recorder.Get(i).IsRecording)
					{
						m_currentchannel = Recorder.Get(i).CurrentTVRecording.Channel;
					}
				}
			}
		}

		/// <summary>
		/// Changes the current channel after a specified delay.
		/// </summary>
		/// <param name="channelName">The channel to switch to.</param>
		/// <param name="zapdelay">The number of milliseconds to wait before zapping.</param>
		public void ZapToChannel(string channelName, long zapdelay)
		{
			m_zapchannel = channelName;
			m_zapdelay = zapdelay;
			m_zaptime = DateTime.Now;
		}

		/// <summary>
		/// Changes the current channel after a specified delay.
		/// </summary>
		/// <param name="channelNr">The nr of the channel to change to.</param>
		/// <param name="zapdelay">The number of milliseconds to wait before zapping.</param>
		public void ZapToChannel(int channelNr, long zapdelay)
		{
			ArrayList channels = CurrentGroup.tvChannels;
			channelNr--;
			if (channelNr >= 0 && channelNr < CurrentGroup.tvChannels.Count)
			{
				TVChannel chan = (TVChannel) CurrentGroup.tvChannels[channelNr];
				ZapToChannel(chan.Name, zapdelay);
			}
		}

		/// <summary>
		/// Changes to the next channel in the current group.
		/// </summary>
		/// <param name="zapdelay">The delay before zapping.</param>
		public void ZapToNextChannel(long zapdelay)
		{
			int currindex;
			if (m_zapchannel == null)
				currindex = GetChannelIndex(CurrentChannel);
			else
				currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel

			int nextindex = (currindex + 1) % CurrentGroup.tvChannels.Count;
			TVChannel chan = (TVChannel) CurrentGroup.tvChannels[nextindex];
			m_zapchannel = chan.Name;
			m_zapdelay = zapdelay;
		}

		/// <summary>
		/// Changes to the previous channel in the current group.
		/// </summary>
		/// <param name="zapdelay">The delay before zapping.</param>
		public void ZapToPreviousChannel(long zapdelay)
		{
			int currindex;
			if (m_zapchannel == null)
				currindex = GetChannelIndex(CurrentChannel);
			else
				currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel

			int previndex = (currindex - 1) % CurrentGroup.tvChannels.Count;
			TVChannel chan = (TVChannel) CurrentGroup.tvChannels[previndex];
			m_zapchannel = chan.Name;
			m_zapdelay = zapdelay;
		}

		/// <summary>
		/// Changes to the next channel group.
		/// </summary>
		public void ZapToNextGroup(long zapdelay) 
		{
			if(m_zapgroup == -1)
				m_zapgroup = (m_currentgroup + 1) % m_groups.Count;
			else
				m_zapgroup = (m_zapgroup + 1) % m_groups.Count;
			m_zapdelay = zapdelay;
			m_zaptime = DateTime.Now;
		}

		/// <summary>
		/// Changes to the previous channel group.
		/// </summary>
		public void ZapToPreviousGroup(long zapdelay) 
		{
			if(m_zapgroup == -1)
				m_zapgroup = (m_currentgroup - 1) % m_groups.Count;
			else
				m_zapgroup = (m_zapgroup - 1) % m_groups.Count;
			m_zapdelay = zapdelay;
			m_zaptime = DateTime.Now;
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Retrieves the index of the current channel.
		/// </summary>
		/// <returns></returns>
		private int GetChannelIndex(string channelName)
		{
			for (int i = 0; i < CurrentGroup.tvChannels.Count; i++)
			{
				TVChannel chan = (TVChannel) CurrentGroup.tvChannels[i];
				if (chan.Name == channelName)
					return i;
			}
			return 0; // Not found, return first channel index
		}

		/// <summary>
		/// Retrieves the index of the group with the specified name.
		/// </summary>
		/// <param name="groupname"></param>
		/// <returns></returns>
		private int GetGroupIndex(string groupname)
		{
			for (int i = 0; i < m_groups.Count; i++)
			{
				TVGroup group = (TVGroup) m_groups[i];
				if (group.GroupName == groupname)
					return i;
			}
			return -1;
		}

		#endregion

		#region Serialization

		public void LoadSettings(Xml xmlreader)
		{
			m_currentchannel = xmlreader.GetValueAsString("mytv", "channel", "");
			m_strGroup=xmlreader.GetValueAsString("mytv", "group", GUILocalizeStrings.Get(972));
			for (int i=0; i < m_groups.Count;++i)
			{
				TVGroup group=(TVGroup)m_groups[i];
				if (m_strGroup==group.GroupName)
				{
					m_currentgroup=i;
					break;
				}
			}
		}

		public void SaveSettings(Xml xmlwriter)
		{
			xmlwriter.SetValue("mytv", "channel", m_currentchannel);
			xmlwriter.SetValue("mytv", "group", m_strGroup);
		}

		#endregion
	}

	#endregion
}
