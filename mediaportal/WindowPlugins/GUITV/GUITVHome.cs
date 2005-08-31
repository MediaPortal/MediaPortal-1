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
		#region variables
		enum Controls
		{
			IMG_REC_CHANNEL=21,
			LABEL_REC_INFO=22,
			IMG_REC_RECTANGLE=23,

		};

		static bool     			m_bTVON=true;
		static bool     			m_bTimeShifting=true;
		static ChannelNavigator		m_navigator;
		
		DateTime        			m_updateTimer=DateTime.Now;
		bool            			autoTurnOnTv=false;
		bool									settingsLoaded=false;
		DateTime						  dtlastTime=DateTime.Now;

		[SkinControlAttribute(2)]			protected GUIButtonControl btnTvGuide=null;
		[SkinControlAttribute(3)]			protected GUIButtonControl btnRecord=null;
		[SkinControlAttribute(6)]			protected GUISelectButtonControl btnGroup=null;
		[SkinControlAttribute(7)]			protected GUISelectButtonControl btnChannel=null;
		[SkinControlAttribute(8)]			protected GUIToggleButtonControl btnTvOnOff=null;
		[SkinControlAttribute(9)]			protected GUIToggleButtonControl btnTimeshiftingOnOff=null;
		[SkinControlAttribute(13)]		protected GUIButtonControl btnTeletext=null;
		[SkinControlAttribute(24)]		protected GUIImage imgRecordingIcon=null;
		[SkinControlAttribute(99)]		protected GUIVideoControl videoWindow=null;
		#endregion		

		public  GUITVHome()
		{	
			GetID=(int)GUIWindow.Window.WINDOW_TV;
		}

    
		#region Serialisation
		void LoadSettings()
		{
			if (settingsLoaded) return;
			settingsLoaded=true;
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_navigator.LoadSettings(xmlreader);
				m_bTVON=xmlreader.GetValueAsBool("mytv","tvon",true);
				m_bTimeShifting=xmlreader.GetValueAsBool("mytv","timeshifting",true);
				autoTurnOnTv   = xmlreader.GetValueAsBool("mytv","autoturnontv",false);

				string strValue=xmlreader.GetValueAsString("mytv","defaultar","normal");
				if (strValue.Equals("zoom")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
				if (strValue.Equals("stretch")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
				if (strValue.Equals("normal")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
				if (strValue.Equals("original")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
				if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
				if (strValue.Equals("panscan")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;
			}
		}

		void SaveSettings()
		{
			using (MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_navigator.SaveSettings(xmlwriter);
				xmlwriter.SetValueAsBool("mytv","tvon",m_bTVON);
				xmlwriter.SetValueAsBool("mytv","timeshifting",m_bTimeShifting);
			}
		}
		#endregion

		#region overrides
		public override bool Init()
		{
			bool bResult= Load (GUIGraphicsContext.Skin+@"\mytvhome.xml");
			// Create the channel navigator (it will load groups and channels)
			m_navigator = new ChannelNavigator();
			LoadSettings();
			return bResult;
		}

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
						TVProgram prog=Navigator.GetTVChannel(channel).CurrentProgram;
						if (!Recorder.IsRecordingChannel(channel))
						{
							if (GUIGraphicsContext.IsFullScreenVideo)
							{
								Recorder.RecordNow(channel,true);
								if (Recorder.IsRecordingChannel(channel))
								{
									GUIDialogNotify dlgNotify	= (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
									if (dlgNotify	!=null)
									{
										string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,channel);
										dlgNotify.Reset();
										dlgNotify.ClearAll();
										dlgNotify.SetImage( strLogo);
										dlgNotify.SetHeading(GUILocalizeStrings.Get(1446));//recording started
										if (prog!=null)
										{
											dlgNotify.SetText( String.Format("{0} {1}-{2}",
												prog.Title,		
												prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
												prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat) ) );	
										}
										else
										{
											dlgNotify.SetText(GUILocalizeStrings.Get(736));//no tvguide data available
										}
										dlgNotify.TimeOut=5;

										dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
									}
								}
								return;
							}

							
							if (prog!=null)
							{
								GUIDialogMenuBottomRight pDlgOK	= (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
								if (pDlgOK!=null)
								{
									pDlgOK.Reset();
									pDlgOK.SetHeading(605);//my tv
									pDlgOK.AddLocalizedString(875); //current program
									pDlgOK.AddLocalizedString(876); //till manual stop
									pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
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
						else //if (!Recorder.IsRecordingChannel(channel))
						{
							Recorder.StopRecording(Recorder.CurrentTVRecording);
							if (GUIGraphicsContext.IsFullScreenVideo)
							{
								GUIDialogNotify dlgNotify	= (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
								if (dlgNotify	!=null)
								{
									string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,channel);
									dlgNotify.Reset();
									dlgNotify.ClearAll();
									dlgNotify.SetImage( strLogo);
									dlgNotify.SetHeading(GUILocalizeStrings.Get(1447));//recording stopped
									if (prog!=null)
									{
										dlgNotify.SetText( String.Format("{0} {1}-{2}",
											prog.Title,		
											prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
											prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat) ) );	
									}
									else
									{
										dlgNotify.SetText(GUILocalizeStrings.Get(736));//no tvguide data available
									}
									dlgNotify.TimeOut=5;

									dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
								}
							}
						}
					}
					break;

				case Action.ActionType.ACTION_PREV_CHANNEL:
					GUITVHome.OnPreviousChannel();
					break;
				case Action.ActionType.ACTION_PAGE_DOWN:
					GUITVHome.OnPreviousChannel();
					break;
        
				case Action.ActionType.ACTION_NEXT_CHANNEL:
					GUITVHome.OnNextChannel();
					break;
				case Action.ActionType.ACTION_PAGE_UP:
					GUITVHome.OnNextChannel();
					break;

        case Action.ActionType.ACTION_LAST_VIEWED_CHANNEL:  // mPod
          GUITVHome.OnLastViewedChannel();
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
					GUIWindowManager.ShowPreviousWindow();
					return;
				}

				case Action.ActionType.ACTION_SHOW_GUI:
					if ( !g_Player.Playing && Recorder.IsViewing())
					{
						//if we're watching tv
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing  && g_Player.IsTV && !g_Player.IsTVRecording)
					{
						//if we're watching a tv recording
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing&&g_Player.HasVideo)
					{
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
					}
					break;
			}
			base.OnAction(action);
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			/*
			if (g_Player.Playing && !g_Player.IsTV)
			{
				if (!g_Player.IsTVRecording)
				{
					Log.Write("TVHome:stop music/video:{0}",g_Player.CurrentFile);
					g_Player.Stop();
				}
			}
			*/		

			//set video window position
			if (videoWindow!=null)
			{
				GUIGraphicsContext.VideoWindow = new Rectangle(videoWindow.XPosition,videoWindow.YPosition,videoWindow.Width,videoWindow.Height);
			}

			// start viewing tv... 
			GUIGraphicsContext.IsFullScreenVideo=false;
				
			Log.Write("tv home init:{0}",Navigator.CurrentChannel);
			ViewChannel(Navigator.CurrentChannel);
			UpdateChannelButton();
			UpdateStateOfButtons();
			UpdateProgressPercentageBar();
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			
					
          
			SaveSettings();
			//if we're switching to another plugin
			if ( !GUITVHome.IsTVWindow(newWindowId) )
			{
				//and we're not playing which means we dont timeshift tv
				if (Recorder.IsViewing() && ! (Recorder.IsTimeShifting()||Recorder.IsRecording()) )
				{
					// and we dont want tv in the background
					if (GUIGraphicsContext.ShowBackground)
					{
						// then stop timeshifting & viewing... 
						Recorder.StopViewing();
					}
				}
			}


			base.OnPageDestroy (newWindowId);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==btnTvOnOff)
			{
				if (Recorder.IsViewing())
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

					//stop playing anything
					if (g_Player.Playing)
					{
						if (g_Player.IsTV && !g_Player.IsTVRecording)
						{
							//already playing tv...
						}
						else
						{
							g_Player.Stop();
						}
					}
					SaveSettings();
				}

				// turn tv on/off
				ViewChannelAndCheck(Navigator.CurrentChannel);
				UpdateChannelButton();
				UpdateStateOfButtons();
				UpdateProgressPercentageBar();
			}

			if (control==btnTimeshiftingOnOff)
			{
				//turn timeshifting off 
				m_bTimeShifting=!Recorder.IsTimeShifting();
				Log.Write("tv home timeshift onoff:{0}",m_bTimeShifting);
				SaveSettings();
				ViewChannelAndCheck(Navigator.CurrentChannel);
				UpdateChannelButton();
				UpdateStateOfButtons();
				UpdateProgressPercentageBar();
			}

			if (control==btnGroup)
			{
				string channel = btnGroup.SelectedLabel;
				if ((channel.Length > 0) && (Navigator.CurrentGroup.GroupName != channel))
				{
					// Change current group and switch to first channel in group
					Navigator.SetCurrentGroup(channel);
					if(Navigator.CurrentGroup.tvChannels.Count > 0) 
					{
						TVChannel chan = (TVChannel)Navigator.CurrentGroup.tvChannels[0];
						Log.Write("tv home btngroup:{0}",chan.Name);
						ViewChannelAndCheck(chan.Name);
						Navigator.UpdateCurrentChannel();
					}

					UpdateStateOfButtons();
					UpdateProgressPercentageBar();
					UpdateChannelButton();
					SaveSettings();
				}
			}
			if (control == btnTeletext)
			{
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
				return;
			}

			if (control== btnRecord)
			{
				OnRecord();
			}
			if (control==btnChannel)
			{
				//switch to another tv channel
				string channel    =btnChannel.SelectedLabel;
				if ((channel.Length > 0) && (Navigator.CurrentChannel != channel))
				{
					m_bTVON=true;
					Log.Write("tv home btnchan:{0}",channel);
					ViewChannelAndCheck(channel);
					Navigator.UpdateCurrentChannel();

					UpdateStateOfButtons();
					UpdateProgressPercentageBar();
					UpdateChannelButton();
					SaveSettings();
				}
			}
			base.OnClicked (controlId, control, actionType);
		}


		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_RESUME_TV:
				{
					LoadSettings();
					if (autoTurnOnTv)
					{
						//restart viewing...  
						m_bTVON=true;
						Log.Write("tv home msg resume tv:{0}",Navigator.CurrentChannel);
						ViewChannel(Navigator.CurrentChannel);
					}
				}
				break;
				case GUIMessage.MessageType.GUI_MSG_RECORDER_VIEW_CHANNEL:
					Log.Write("tv home msg view chan:{0}",message.Label);
					ViewChannel(message.Label);
					Navigator.UpdateCurrentChannel();
					break;

				case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_VIEWING:
					m_bTVON=false;
					Log.Write("tv home msg stop chan:{0}",message.Label);
					ViewChannel(message.Label);
					Navigator.UpdateCurrentChannel();
					break;
			}
			return base.OnMessage(message);
		}

		public override void Process()
		{ 
			if (GUIGraphicsContext.InVmr9Render) return;
			//if we're not playing the timeshifting file
			TimeSpan ts;
			if (Recorder.IsTimeShifting() || Recorder.IsRecording())
			{
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
			}

			// Let the navigator zap channel if needed
			Navigator.CheckChannelChange();

			// Update navigator with information from the Recorder
			// TODO: This should ideally be handled using events. Recorder should fire an event
			// when the current channel changes. This is a temporary workaround //Vic
			string currchan = Navigator.CurrentChannel;		// Remember current channel
			Navigator.UpdateCurrentChannel();
			bool channelchanged = currchan != Navigator.CurrentChannel;

			UpdateStateOfButtons();
			UpdateProgressPercentageBar();
			// Has the channel changed?
			if(channelchanged)
			{
				UpdateChannelButton();
			}
      
			// if we're recording tv, update gui with info
			if (Recorder.IsRecording())
			{
				TVRecording rec=Recorder.GetTVRecording();
				if (rec!= null)
				{	
					if (rec.RecType != TVRecording.RecordingType.Once)
						imgRecordingIcon.SetFileName(Thumbs.TvRecordingSeriesIcon);
					else
						imgRecordingIcon.SetFileName(Thumbs.TvRecordingIcon);
				}				
				imgRecordingIcon.IsVisible=true;
			}
			else
			{
				imgRecordingIcon.IsVisible=false;
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

		#endregion
		void OnRecord()
		{
			//record now.
			//Are we recording this channel already?
			if (!Recorder.IsRecordingChannel(Navigator.CurrentChannel))
			{
				//no then start recording
				TVProgram prog=Navigator.CurrentTVChannel.CurrentProgram;
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
					Navigator.UpdateCurrentChannel();
					Recorder.StopRecording();

					// and re-start viewing.... 
					Log.Write("tv home stoprecording chan:{0}",Navigator.CurrentChannel);
					ViewChannel(Navigator.CurrentChannel);
					Navigator.UpdateCurrentChannel();
				}
			}
			UpdateStateOfButtons();
		}

		/// <summary>
		/// Update the state of the following buttons
		/// - tv on/off
		/// - timeshifting on/off
		/// - record now
		/// </summary>
		void UpdateStateOfButtons()
		{
			btnTvOnOff.Selected=Recorder.IsViewing();;
			//are we recording a tv program?
			if (Recorder.IsRecording())
			{
				//yes then disable the timeshifting on/off buttons
				//and change the Record Now button into Stop Record
				btnTimeshiftingOnOff.Disabled=true;
				btnTimeshiftingOnOff.Selected=true;
				btnRecord.Label=GUILocalizeStrings.Get(629);//stop record
			}
			else
			{
				//nop. then change the Record Now button
				//to Record Now
				btnRecord.Label=GUILocalizeStrings.Get(601);// record
      
				//is current card is not supporting timeshifting
				bool supportstimeshifting=Recorder.DoesSupportTimeshifting();
				if (btnTvOnOff.Selected==false || supportstimeshifting==false)
				{
					//then disable the timeshifting button
					btnTimeshiftingOnOff.Disabled=true;
					btnTimeshiftingOnOff.Selected=false;
				}
				else if (supportstimeshifting)
				{
					//enable the timeshifting button
					btnTimeshiftingOnOff.Disabled=false;
					// set state of timeshifting button
					if ( Recorder.IsTimeShifting() )
					{
						btnTimeshiftingOnOff.Selected=true;
					}
					else
					{
						btnTimeshiftingOnOff.Selected=false;
					}
				}
			}
		}

		// updates the channel button so it shows the currently selected tv channel
		void UpdateChannelButton()
		{
			btnTeletext.IsVisible=Recorder.HasTeletext();
			
			// Update group button
			int i=0;
//			currentGroup=null;
			GUIControl.ClearControl(GetID, btnGroup.GetID);
			foreach (TVGroup group in Navigator.Groups)
			{
				GUIControl.AddItemLabelControl(GetID,btnGroup.GetID,group.GroupName);
				if (group == Navigator.CurrentGroup) 
				{
					GUIControl.SelectItemControl(GetID, btnGroup.GetID, i);
				}
				++i;
			}

			// Update channel button
			i=0;
			GUIControl.ClearControl(GetID, btnChannel.GetID);
			foreach (TVChannel chan in Navigator.CurrentGroup.tvChannels)
			{
				GUIControl.AddItemLabelControl(GetID,btnChannel.GetID,chan.Name);
				if (chan.Name==Navigator.CurrentChannel)
				{
					GUIControl.SelectItemControl(GetID, btnChannel.GetID, i);
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
			try
			{
				if (Navigator.CurrentTVChannel==null)
				{
					GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
					return;
				}
				//get current tv program
				TVProgram prog=Navigator.CurrentTVChannel.CurrentProgram;
				if (prog==null) 
				{
					GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
					return;
				}
				TimeSpan ts=prog.EndTime-prog.StartTime;
				if (ts.TotalSeconds<=0)
				{
					GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
					return;
				}
				double iTotalSecs=ts.TotalSeconds;
				ts=DateTime.Now-prog.StartTime;
				double iCurSecs=ts.TotalSeconds;
				double fPercent = ((double)iCurSecs) / ((double)iTotalSecs);
				fPercent *=100.0d;
				GUIPropertyManager.SetProperty("#TV.View.Percentage", ((int)fPercent).ToString());
			}
			catch (Exception ex)
			{
				Log.Write("grrrr:{0}",ex.Source, ex.StackTrace);
			}
		}

		/// <summary>
		/// this method is called periodicaly by MP
		/// as long as this window is shown
		/// It will check if anything has changed like
		/// tv channel switched or recording started/stopped
		/// and will update the GUI
		/// </summary>

		/// <summary>
		/// This method will try playing the timeshifting file
		/// </summary>
		/// <param name="bCheckOnOffButton">check state of tv on/off button</param>
		static void StartPlaying(bool bCheckOnOffButton)
		{
			if (bCheckOnOffButton)
			{
				//if tv is off then do nothing
				if (!Recorder.IsViewing()) return;
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
			Log.Write("GUITVHome:OnPreviousChannel()");
			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				// where in fullscreen so delayzap channel instead of immediatly tune..
				GUIFullScreenTV	TVWindow = (GUIFullScreenTV) GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
				if (TVWindow != null) TVWindow.ZapPreviousChannel();
				return;
			}

			// Zap to previous channel immediately
			Navigator.ZapToPreviousChannel(false);
		}

		static public void ViewChannelAndCheck(string channel)
		{
			if (g_Player.Playing)
			{
				if (g_Player.IsTVRecording) return;
				if (g_Player.IsVideo) return;
				if (g_Player.IsDVD) return;
				if ( (g_Player.IsMusic && g_Player.HasVideo) ) return;
			}
			ViewChannel(channel);
			if (Recorder.TVChannelName!=channel && m_bTVON)
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
			if (g_Player.Playing)
			{
				if (g_Player.IsTVRecording) return;
				if (g_Player.IsVideo) return;
				if (g_Player.IsDVD) return;
				if ( (g_Player.IsMusic && g_Player.HasVideo) ) return;
			}
			if (m_bTVON)
				Log.Write("GUITVHome.ViewChannel(): View channel={0} ts:{1}", channel, m_bTimeShifting);
			else
				Log.Write("GUITVHome.ViewChannel(): turn tv off");

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
			Log.Write("GUITVHome:OnNextChannel()");
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
			Navigator.ZapToNextChannel(false);
		}

		/// <summary>
    /// When called this method will switch to the last viewed TV channel   // mPod
    /// </summary>
    static public void OnLastViewedChannel()
    {
      Navigator.ZapToLastViewedChannel();
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
			if (windowId== (int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TV_CONFLICTS) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_MAIN) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_AUTO) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS_STATUS) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_SETTINGS) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TV_NO_SIGNAL) return true;
			if (windowId== (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO) return true;

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
			strButtonImage = String.Empty;
			strButtonImageFocus = String.Empty;
			strPictureImage = String.Empty;
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
		private string		m_currentchannel = String.Empty;
		private DateTime	m_zaptime;
		private long			m_zapdelay;
		private string		m_zapchannel = null;
		private int				m_zapgroup = -1;
		private string    lastViewedChannel = null; // saves the last viewed Channel  // mPod
		private TVChannel m_currentTvChannel=null;
		private ArrayList channels = new ArrayList();
		#endregion

		#region Constructors

		public ChannelNavigator()
		{
			// Load all groups
			ArrayList groups = new ArrayList();
			TVDatabase.GetGroups(ref groups); // Put groups in a local variable to ensure the "All" group is first always

			channels.Clear();
			// Add a group containing all channels
			TVDatabase.GetChannels(ref channels); // Load all channels
			TVGroup tvgroup = new TVGroup();
			tvgroup.GroupName = GUILocalizeStrings.Get(972); //all channels
			foreach (TVChannel channel in channels)
				tvgroup.tvChannels.Add(channel);
			m_groups.Add(tvgroup);

			m_groups.AddRange(groups); // Add rest of the groups to the end of the list
			TVDatabase.OnChannelsChanged+=new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(OnChannelsChanged);
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
		/// Gets the channel that we currently watch.
		/// Returns empty string if there is no current channel.
		/// </summary>
		public TVChannel CurrentTVChannel
		{
			get { return m_currentTvChannel; }
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
		/// Gets the configured zap delay (in milliseconds).
		/// </summary>
		public long ZapDelay
		{
			get { return m_zapdelay; }
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
		public bool CheckChannelChange()
		{
			if (GUIGraphicsContext.InVmr9Render) return false;
			UpdateCurrentChannel();

			// Zapping to another group or channel?
			if (m_zapgroup != -1 || m_zapchannel != null)
			{
				// Time to zap?
				if (DateTime.Now >= m_zaptime)
				{
					// Zapping to another group?
					if(m_zapgroup != -1 && m_zapgroup != m_currentgroup)
					{
						// Change current group and zap to the first channel of the group
						m_currentgroup = m_zapgroup;
						if(CurrentGroup.tvChannels.Count > 0) 
						{
							TVChannel chan = (TVChannel)CurrentGroup.tvChannels[0];
							m_zapchannel = chan.Name;
						}
					}
					m_zapgroup = -1;

					lastViewedChannel = m_currentchannel;
					// Zap to desired channel
					Log.Write("Channel change:{0}",m_zapchannel);
					GUITVHome.ViewChannel(m_zapchannel);
					m_zapchannel = null;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Changes the current channel group.
		/// </summary>
		/// <param name="groupname">The name of the group to change to.</param>
		public void SetCurrentGroup(string groupname)
		{
			m_currentgroup = GetGroupIndex(groupname);
		}

		/// <summary>
		/// Ensures that the navigator has the correct current channel (retrieved from the Recorder).
		/// </summary>
		public void UpdateCurrentChannel()
		{
			string newChannel=String.Empty;
			//if current card is watching tv then use that channel
			if (Recorder.IsViewing() || Recorder.IsTimeShifting())
			{
				newChannel = Recorder.GetTVChannelName();
			}
			else if (Recorder.IsRecording())
			{ // else if current card is recording, then use that channel
				newChannel = Recorder.GetTVRecording().Channel;
			}
			else if (Recorder.IsAnyCardRecording())
			{ // else if any card is recording
				//then get & use that channel
				for (int i = 0; i < Recorder.Count; ++i)
				{
					if (Recorder.Get(i).IsRecording)
					{
						newChannel = Recorder.Get(i).CurrentTVRecording.Channel;
					}
				}
			}
			if (newChannel==String.Empty)
				newChannel=m_currentchannel ;
			if (m_currentchannel != newChannel && newChannel!=String.Empty)
			{
				m_currentchannel = newChannel;
				m_currentTvChannel=GetTVChannel(m_currentchannel);
			}
		}

		/// <summary>
		/// Changes the current channel after a specified delay.
		/// </summary>
		/// <param name="channelName">The channel to switch to.</param>
		/// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
		public void ZapToChannel(string channelName, bool useZapDelay)
		{
			m_zapchannel = channelName;

			if(useZapDelay)
				m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
			else
				m_zaptime = DateTime.Now;
		}

		/// <summary>
		/// Changes the current channel after a specified delay.
		/// </summary>
		/// <param name="channelNr">The nr of the channel to change to.</param>
		/// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
		public void ZapToChannel(int channelNr, bool useZapDelay)
		{
			ArrayList channels = CurrentGroup.tvChannels;
			channelNr--;
			if (channelNr >= 0 && channelNr < channels.Count)
			{
				TVChannel chan = (TVChannel) channels[channelNr];
				ZapToChannel(chan.Name, useZapDelay);
			}
		}

		/// <summary>
		/// Changes to the next channel in the current group.
		/// </summary>
		/// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
		public void ZapToNextChannel(bool useZapDelay)
		{
			string currentChan=String.Empty;
			int currindex;
			if (m_zapchannel == null)
			{
				currindex = GetChannelIndex(CurrentChannel);
				currentChan=CurrentChannel;
			}
			else
			{
				currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel
				currentChan=CurrentChannel;
			}
			// Step to next channel
			currindex++;
			if(currindex >= CurrentGroup.tvChannels.Count)
				currindex = 0;
			TVChannel chan = (TVChannel) CurrentGroup.tvChannels[currindex];
			m_zapchannel = chan.Name;

			Log.Write("Navigator:ZapNext {0}->{1}", currentChan,m_zapchannel);
			if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
			{				
				if(useZapDelay)
					m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
				else
					m_zaptime = DateTime.Now;
			}
			else
			{
				m_zaptime = DateTime.Now;
			}
		}

		/// <summary>
		/// Changes to the previous channel in the current group.
		/// </summary>
		/// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
		public void ZapToPreviousChannel(bool useZapDelay)
		{
			string currentChan=String.Empty;
			int currindex;
			if (m_zapchannel == null)
			{
				currentChan=CurrentChannel;
				currindex = GetChannelIndex(CurrentChannel);
			}
			else
			{
				currentChan=m_zapchannel;
				currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel
			}
			// Step to previous channel
			currindex--;
			if(currindex < 0)
				currindex = CurrentGroup.tvChannels.Count - 1;

			TVChannel chan = (TVChannel) CurrentGroup.tvChannels[currindex];
			m_zapchannel = chan.Name;

			Log.Write("Navigator:ZapPrevious {0}->{1}", currentChan,m_zapchannel);
			if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
			{				
				if(useZapDelay)
					m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
				else
					m_zaptime = DateTime.Now;
			}
			else
			{
				m_zaptime = DateTime.Now;
			}
		}

		/// <summary>
		/// Changes to the next channel group.
		/// </summary>
		public void ZapToNextGroup(bool useZapDelay) 
		{
			if(m_zapgroup == -1)
				m_zapgroup = m_currentgroup + 1;
			else
				m_zapgroup = m_zapgroup + 1;			// Zap from last zap group

			if(m_zapgroup >= m_groups.Count)
				m_zapgroup = 0;

			if(useZapDelay)
				m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
			else
				m_zaptime = DateTime.Now;
		}

		/// <summary>
		/// Changes to the previous channel group.
		/// </summary>
		public void ZapToPreviousGroup(bool useZapDelay) 
		{
			if(m_zapgroup == -1)
				m_zapgroup = m_currentgroup - 1;
			else
				m_zapgroup = m_zapgroup - 1;

			if(m_zapgroup < 0)
				m_zapgroup = m_groups.Count - 1;

			if(useZapDelay)
				m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
			else
				m_zaptime = DateTime.Now;
		}

    /// <summary>
    /// Zaps to the last viewed Channel (without ZapDelay).  // mPod
    /// </summary>
    public void ZapToLastViewedChannel()
    {
      m_zapchannel = lastViewedChannel;
      m_zaptime = DateTime.Now;
    }
		#endregion

		#region Private methods

		void OnChannelsChanged()
		{
			// Load all groups
			if (GUIGraphicsContext.DX9Device==null) return;
			ArrayList groups = new ArrayList();
			TVDatabase.GetGroups(ref groups); // Put groups in a local variable to ensure the "All" group is first always

			channels.Clear();
			m_groups.Clear();
			// Add a group containing all channels
			TVDatabase.GetChannels(ref channels); // Load all channels
			TVGroup tvgroup = new TVGroup();
			tvgroup.GroupName = GUILocalizeStrings.Get(972); //all channels
			foreach (TVChannel channel in channels)
				tvgroup.tvChannels.Add(channel);
			m_groups.Add(tvgroup);
			m_groups.AddRange(groups); // Add rest of the groups to the end of the list

			if (m_currentchannel.Trim()==String.Empty)
			{
				TVGroup group=(TVGroup )m_groups[m_currentgroup];
				m_currentchannel=((TVChannel)group.tvChannels[0]).Name;
			}
			m_currentTvChannel=GetTVChannel(m_currentchannel);

		}

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
		public TVChannel GetTVChannel(string channelName)
		{
			foreach (TVChannel chan in channels)
			{
				if (chan.Name==channelName) return chan;
			}
			return null;
		}

		#endregion

		#region Serialization

		public void LoadSettings(MediaPortal.Profile.Xml xmlreader)
		{
			m_currentchannel = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
			m_zapdelay = 1000 * xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2);
			string groupname = xmlreader.GetValueAsString("mytv", "group", GUILocalizeStrings.Get(972));
			m_currentgroup = GetGroupIndex(groupname);
			if(m_currentgroup < 0 || m_currentgroup >= m_groups.Count)		// Group no longer exists?
				m_currentgroup = 0;

			if (m_currentchannel.Trim()==String.Empty)
			{
				TVGroup group=(TVGroup )m_groups[m_currentgroup];
				m_currentchannel=((TVChannel)group.tvChannels[0]).Name;
			}

			m_currentTvChannel=GetTVChannel(m_currentchannel);
		}

		public void SaveSettings(MediaPortal.Profile.Xml xmlwriter)
		{
			if (m_currentchannel.Trim() != String.Empty)
				xmlwriter.SetValue("mytv", "channel", m_currentchannel);
			
			if (CurrentGroup.GroupName.Trim() != String.Empty)
				xmlwriter.SetValue("mytv", "group", CurrentGroup.GroupName);
		}

		#endregion
	}

	#endregion
}
