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
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;



namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITVGuide : GUIWindow
	{
		const int MaxDaysInGuide=30;
		const int RowID=1000;
		const int ColID=10;
		enum Controls
		{
			PANEL_BACKGROUND=2,
			SPINCONTROL_DAY=6,
			SPINCONTROL_TIME_INTERVAL=8,
			CHANNEL_IMAGE_TEMPLATE=7,
			CHANNEL_LABEL_TEMPLATE=18,
			LABEL_GENRE_TEMPLATE=23,
			LABEL_TITLE_TEMPLATE=24,
			VERTICAL_LINE=25,
			LABEL_TITLE_DARK_TEMPLATE=26,
			LABEL_GENRE_DARK_TEMPLATE=30,
      CHANNEL_TEMPLATE=20, // Channel rectangle and row height
      
			HORZ_SCROLLBAR=28,
			VERT_SCROLLBAR=29,
			LABEL_TIME1=40, // first and template
			IMG_CHAN1=50, 
			IMG_CHAN1_LABEL=70,
			IMG_TIME1=90, // first and template
			IMG_REC_PIN=31
		};

		[SkinControlAttribute(98)]		protected GUIImage videoBackground;
		[SkinControlAttribute(99)]		protected GUIVideoControl videoWindow;
    
		DateTime                            m_dtTime=DateTime.Now;
		int                                 m_iChannelOffset=0;
		ArrayList                           m_channels=new ArrayList();
		ArrayList                           m_recordings=new ArrayList();
		int                                 m_iBlockTime=30; // steps of 30 minutes
		int                                 m_iChannels=5;
		int                                 m_iBlocks=4;
		int                                 m_iCursorX=0;
		int                                 m_iCursorY=0;
		string                              m_strCurrentTitle=String.Empty;
		string                              m_strCurrentTime=String.Empty;
		string                              m_strCurrentChannel=String.Empty;
		long                                m_iCurrentStartTime=0;
		long                                m_iCurrentEndTime=0;
		TVProgram                           m_currentProgram=null;
		static string                       m_strTVGuideFile;
		static System.IO.FileSystemWatcher  m_watcher; 
		bool                                m_bNeedUpdate=false;
		DateTime                            m_dtStartTime=DateTime.Now;
		bool                                m_bUseColors=false;
		ArrayList                           colors = new ArrayList();
		bool                                m_bSingleChannel=false;
		int                                 m_iProgramOffset=0;
		int                                 m_iTotalPrograms=0;
    int                                 m_iSingleChannel=0;
    bool                                m_bUseChannelLogos=false;
		ArrayList                           notifies = new ArrayList();
		int																	m_cursorXBackup=0;
		int																	m_cursorYBackup=0;
		int																	m_channelOffsetBack=0;

		DateTime  m_timeKeyPressed=DateTime.Now;
		string    m_strInput=String.Empty;
    
		public  GUITVGuide()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TVGUIDE;

			colors.Add(Color.Red);
			colors.Add(Color.Green);
			colors.Add(Color.Blue);
			colors.Add(Color.Cyan);
			colors.Add(Color.Magenta);
			colors.Add(Color.DarkBlue);
			colors.Add(Color.Brown);
			colors.Add(Color.Fuchsia);
			colors.Add(Color.Khaki);
			colors.Add(Color.SteelBlue);
			colors.Add(Color.SaddleBrown);
			colors.Add(Color.Chocolate);
			colors.Add(Color.DarkMagenta);
			colors.Add(Color.DarkSeaGreen);
			colors.Add(Color.Coral);
			colors.Add(Color.DarkGray);
			colors.Add(Color.DarkOliveGreen);
			colors.Add(Color.DarkOrange);
			colors.Add(Color.ForestGreen);
			colors.Add(Color.Honeydew);
			colors.Add(Color.Gray);
			colors.Add(Color.Tan);
			colors.Add(Color.Silver);
			colors.Add(Color.SeaShell);
			colors.Add(Color.RosyBrown);
			colors.Add(Color.Peru);
			colors.Add(Color.OldLace);
		}
    
		public override bool Init()
		{
			m_strTVGuideFile="xmltv";
			using(MediaPortal.Profile.Xml  xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_strTVGuideFile=xmlreader.GetValueAsString("xmltv","folder","xmltv");
				m_strTVGuideFile=Utils.RemoveTrailingSlash(m_strTVGuideFile);
				m_bUseColors=xmlreader.GetValueAsBool("xmltv", "colors",false);
			}

			// Create a new FileSystemWatcher and set its properties.
			try
			{
				System.IO.Directory.CreateDirectory(m_strTVGuideFile);
			}
			catch(Exception){}
			m_watcher = new FileSystemWatcher();
			m_watcher.Path=m_strTVGuideFile;
			m_watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			m_watcher.Filter = "*.xml";
			// Add event handlers.
			m_watcher.Changed += new FileSystemEventHandler(OnChanged);
			m_watcher.Created += new FileSystemEventHandler(OnChanged);
			m_watcher.Renamed += new RenamedEventHandler(OnRenamed);
			m_watcher.EnableRaisingEvents = true;

			m_strTVGuideFile+=@"\tvguide.xml";

			CheckNewTVGuide();
			// check if there's a new TVguide.xml
			try
			{
				ArrayList channels = new ArrayList();
				ArrayList programs = new ArrayList();
				TVDatabase.GetChannels(ref channels);
				if (channels.Count==0) 
				{
					StartImportXML();
				}
				channels=null;
			}
			catch (Exception)
			{
			}
    
			TVDatabase.OnProgramsChanged+=new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_OnProgramsChanged);
			TVDatabase.OnNotifiesChanged+=new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(TVDatabase_OnNotifiesChanged);
			ConflictManager.OnConflictsUpdated+=new MediaPortal.TV.Recording.ConflictManager.OnConflictsUpdatedHandler(ConflictManager_OnConflictsUpdated);
			return Load (GUIGraphicsContext.Skin+@"\mytvguide.xml");
		}

		void CheckNewTVGuide()
		{
			bool bImport=false;
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string strTmp=String.Empty;
				strTmp=xmlreader.GetValueAsString("tvguide","date",String.Empty);
				if (System.IO.File.Exists(m_strTVGuideFile ) )
				{
					string strFileTime=System.IO.File.GetLastWriteTime(m_strTVGuideFile).ToString();
					if (strTmp != strFileTime)
					{
						bImport=true;
					}
				}
			}
			if (bImport) 
			{
				StartImportXML();
			}

		}
    
		#region Serialisation
		void LoadSettings()
		{
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_strCurrentChannel=xmlreader.GetValueAsString("tvguide","channel",String.Empty);
				m_iCursorY=xmlreader.GetValueAsInt("tvguide", "ypos",0);
				m_iChannelOffset=xmlreader.GetValueAsInt("tvguide", "yoffset",0);
			}
		}

		void SaveSettings()
		{
			using (MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("tvguide","channel",m_strCurrentChannel);
				xmlwriter.SetValue("tvguide", "ypos",m_iCursorY.ToString());
				xmlwriter.SetValue("tvguide", "yoffset",m_iChannelOffset.ToString());
			}
		}
		#endregion

		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_PREVIOUS_MENU:
				{
					GUIWindowManager.ShowPreviousWindow();
					return;
				}
				case Action.ActionType.ACTION_KEY_PRESSED:
					if (action.m_key!=null)
						OnKeyCode((char)action.m_key.KeyChar);
					break;

        case Action.ActionType.ACTION_SELECT_ITEM:
          if (GetFocusControlId() == 1)
          {
            if (m_iCursorX == 0)
            {
              OnSwitchMode();
            }
            else
            {
              OnRecord();
            }
          }
					break;

				case Action.ActionType.ACTION_MOUSE_MOVE:
				{
					int x=(int)action.fAmount1;
					int y=(int)action.fAmount2;
					foreach (GUIControl control in controlList)
					{
						if (control.GetID>=(int)Controls.IMG_CHAN1+0 && control.GetID<=(int)Controls.IMG_CHAN1+m_iChannels)
						{
							if (x>=control.XPosition && x < control.XPosition+control.Width)
							{
								if (y>=control.YPosition && y < control.YPosition+control.Height)
								{
									UnFocus();
									m_iCursorY=control.GetID-(int)Controls.IMG_CHAN1;
									m_iCursorX=0;

                  if (m_iSingleChannel != m_iCursorY + m_iChannelOffset) Update(false);
									UpdateCurrentProgram();
									UpdateHorizontalScrollbar();
									UpdateVerticalScrollbar();
									return;
								}
							}
						}
						if (control.GetID>=100)
						{
							if (x>=control.XPosition && x < control.XPosition+control.Width)
							{
								if (y>=control.YPosition && y < control.YPosition+control.Height)
								{
									int iControlId=control.GetID;
									if (iControlId>=100)
									{
										iControlId-=100;
										int iCursorY=(iControlId/RowID);
										iControlId-=iCursorY*RowID;
										if (iControlId%ColID==0)
										{
											int iCursorX=(iControlId/ColID)+1;
											if (iCursorY!=m_iCursorY || iCursorX!=m_iCursorX)
											{
												UnFocus();
												m_iCursorY=iCursorY;
												m_iCursorX=iCursorX;
												UpdateCurrentProgram();
												SetFocus();
												UpdateHorizontalScrollbar();
												UpdateVerticalScrollbar();
												return;
											}
											return;
										}
									}
								}
							}
						}
					}
					UnFocus();
					m_iCursorX=-1;
					m_iCursorY=-1;
					base.OnAction(action);
				}
					break;

				case Action.ActionType.ACTION_TVGUIDE_RESET:
					m_iCursorX=0;
					m_dtTime=DateTime.Now;
					Update(false);
					break;

					
        case Action.ActionType.ACTION_CONTEXT_MENU:
				{
					if (m_iCursorX>=0&& m_iCursorY>=0)
					{
						if (m_iCursorX==0)
						{
							OnSwitchMode();
							return;              
						}
						else
						{
							ShowContextMenu();
						}
					}
					else
					{
						action.wID=Action.ActionType.ACTION_SELECT_ITEM;
						GUIWindowManager.OnAction(action);
					}
				}
					break;

				case Action.ActionType.ACTION_PAGE_UP:
					OnPageUp();
					break;
        
				case Action.ActionType.ACTION_PAGE_DOWN:
					OnPageDown();
					break;

				case Action.ActionType.ACTION_MOVE_LEFT:
				{
					if (m_iCursorY>=0)
					{
						OnLeft();
						UpdateHorizontalScrollbar();
						return;
					}
				}
					break;
				case Action.ActionType.ACTION_MOVE_RIGHT:
				{
					if (m_iCursorY>=0)
					{
						OnRight();
						UpdateHorizontalScrollbar();
						return;
					}
				}
					break;
				case Action.ActionType.ACTION_MOVE_UP:
				{
					if (m_iCursorY>=0)
					{
						OnUp();
						UpdateVerticalScrollbar();
						return;
					}
				}
					break;
				case Action.ActionType.ACTION_MOVE_DOWN:
				{
					if (m_iCursorY>=0)
					{
						OnDown();
						UpdateVerticalScrollbar();
					}
					else 
					{
						m_iCursorY=0;
						SetFocus();
						UpdateVerticalScrollbar();
					}
					return;
				}
					//break;
				case Action.ActionType.ACTION_SHOW_INFO:
				{
					ShowContextMenu();
				}
					break;
				case Action.ActionType.ACTION_INCREASE_TIMEBLOCK:
				{
					m_iBlockTime+=15;
					Update(false);
					SetFocus();
				}
					break;
				case Action.ActionType.ACTION_DECREASE_TIMEBLOCK:
				{
					if (m_iBlockTime>15) m_iBlockTime-=15;
					Update(false);
					SetFocus();
				}
					break;
				case Action.ActionType.ACTION_DEFAULT_TIMEBLOCK:
				{
					m_iBlockTime=30;
					Update(false);
					SetFocus();
				}
					break;

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

		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_PERCENTAGE_CHANGED:
					if (message.SenderControlId== (int)Controls.HORZ_SCROLLBAR)
					{
						m_bNeedUpdate=true;
						float fPercentage = (float)message.Param1;
						fPercentage /= 100.0f;
						fPercentage *= 24.0f ;
						fPercentage *= 60.0f ;
						m_dtTime = new DateTime(m_dtTime.Year,m_dtTime.Month,m_dtTime.Day,0,0,0,0);
						m_dtTime=m_dtTime.AddMinutes((int)fPercentage);
					}

					if (message.SenderControlId== (int)Controls.VERT_SCROLLBAR)
					{
						m_bNeedUpdate=true;
						float fPercentage = (float)message.Param1;
						fPercentage /= 100.0f;
						if (m_bSingleChannel)
						{
							fPercentage *= (float)m_iTotalPrograms;
							int iChan=(int)fPercentage;
							m_iChannelOffset=0;
							m_iCursorY=0;
							while (iChan >=m_iChannels) 
							{
								iChan -=m_iChannels;
								m_iChannelOffset+=m_iChannels;
							}
							m_iCursorY=iChan;
						}
						else
						{
							fPercentage *= (float)m_channels.Count;
							int iChan=(int)fPercentage;
							m_iChannelOffset=0;
							m_iCursorY=0;
							while (iChan >=m_iChannels) 
							{
								iChan -=m_iChannels;
								m_iChannelOffset+=m_iChannels;
							}
							m_iCursorY=iChan;
						}
					}
					break;

				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					base.OnMessage(message);
					SaveSettings();
					m_recordings.Clear();
					if ( !GUITVHome.IsTVWindow(message.Param1) )
					{
						if (! g_Player.Playing)
						{
							if (GUIGraphicsContext.ShowBackground)
							{
								// stop timeshifting & viewing... 
	              
								Recorder.StopViewing();
							}
						}
					}
					notifies=null;
					m_channels=null;
					m_recordings=null;
					m_currentProgram=null;

					return true;
				}

				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
				{
					base.OnMessage(message);
					notifies=new ArrayList();
					m_channels=new ArrayList();
					m_recordings=new ArrayList();
					
          LoadSettings();
					TVDatabase.GetNotifies(notifies,false);

					GUIControl cntlPanel = GetControl((int)Controls.PANEL_BACKGROUND);
          GUIImage cntlChannelTemplate = (GUIImage)GetControl((int)Controls.CHANNEL_TEMPLATE);

          int iHeight=cntlPanel.Height;
          int iItemHeight=cntlChannelTemplate.Height;
					m_iChannels= (int)(((float)iHeight) / ((float)iItemHeight) );

          UnFocus();
					m_currentProgram=null;
					m_iCursorX=0;
					m_iCursorY=0;
					m_iChannelOffset=0;
          m_bSingleChannel=false;
          m_bUseChannelLogos=false;
					if (Recorder.IsViewing( ) )
					{
						m_strCurrentChannel= Recorder.GetTVChannelName(   );
						GetChannels();
						for (int i=0; i < m_channels.Count;i++)
						{
							TVChannel chan=(TVChannel)m_channels[i];
							if (chan.Name.Equals(m_strCurrentChannel))
							{
								m_iCursorY=i;
								break;
							}
						}
					}
					while (m_iCursorY >= m_iChannels) 
					{
						m_iCursorY -=m_iChannels;
						m_iChannelOffset+=m_iChannels;

					}
					CheckNewTVGuide();
					m_dtTime=DateTime.Now;

					GUISpinControl cntlDay=GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;
					if (cntlDay!=null)
					{
						cntlDay.Reset();
            cntlDay.SetRange(0, MaxDaysInGuide-1);
						for (int iDay=0; iDay < MaxDaysInGuide; iDay++)
						{
							DateTime dtTemp=m_dtTime.AddDays(iDay);
							string day;
							switch (dtTemp.DayOfWeek)
							{
								case DayOfWeek.Monday :	day = GUILocalizeStrings.Get(657);	break;
								case DayOfWeek.Tuesday :	day = GUILocalizeStrings.Get(658);	break;
								case DayOfWeek.Wednesday :	day = GUILocalizeStrings.Get(659);	break;
								case DayOfWeek.Thursday :	day = GUILocalizeStrings.Get(660);	break;
								case DayOfWeek.Friday :	day = GUILocalizeStrings.Get(661);	break;
								case DayOfWeek.Saturday :	day = GUILocalizeStrings.Get(662);	break;
								default:	day = GUILocalizeStrings.Get(663);	break;
							}
							day=String.Format("{0} {1}-{2}",day,dtTemp.Day, dtTemp.Month);
							cntlDay.AddLabel(day,iDay);
						}
					}
					GUISpinControl cntlTimeInterval=GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
					if (cntlTimeInterval!=null)
					{
						for (int i=1; i <= 4; i++) cntlTimeInterval.AddLabel(String.Empty,i);	
						cntlTimeInterval.Value=1;
					}
					Update(true);
					SetFocus();
					if (m_currentProgram!=null)
					{
						m_dtStartTime=m_currentProgram.StartTime;
					}
					UpdateCurrentProgram();

					Log.Write("turn tv on");
					GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV,GetID,0,0,0,null);
					msg.SendToTargetWindow=true;
					GUIWindowManager.SendThreadMessage(msg);

					return true;
				}
					//break;

				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.SPINCONTROL_DAY)
					{
						GUISpinControl cntlDay=GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;
						int iDay=cntlDay.Value;
						
						m_dtTime=DateTime.Now;
						m_dtTime = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,m_dtTime.Hour,m_dtTime.Minute,0,0);
						m_dtTime=m_dtTime.AddDays(iDay);
						Update(false);
						SetFocus();
						return true;
					}
					if (iControl==(int)Controls.SPINCONTROL_TIME_INTERVAL)
					{
						GUISpinControl cntlTimeInt=GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL) as GUISpinControl;
						int iInterval=(cntlTimeInt.Value)+1;
            if (iInterval>4) iInterval=4;
						m_iBlockTime=iInterval*15;
						Update(false);
						SetFocus();
						return true;
					}
					if (iControl>=100)
					{
						OnRecord();
						Update(false);
						SetFocus();
					}
					else if (m_iCursorX==0)
					{
						OnSwitchMode();
					}
					break;

			}
			return base.OnMessage(message);;
		}

		void Update(bool selectCurrentShow)
		{
			lock (this)
			{
				if ( GUIWindowManager.ActiveWindow != this.GetID)
				{
					return;
				}

				GUISpinControl cntlDay=GetControl((int)Controls.SPINCONTROL_DAY) as GUISpinControl;

				// Find first day in TVGuide and set spincontrol position 
				int iDay=m_dtTime.DayOfYear - DateTime.Now.DayOfYear;
				for ( ; iDay<0 ; ++iDay)
				{
					m_dtTime = m_dtTime.AddDays(1.0);
				}
				for ( ; iDay>=MaxDaysInGuide ; --iDay)
				{
					m_dtTime = m_dtTime.AddDays(-1.0);
				}
				cntlDay.Value=iDay;

				int xpos,ypos;
				GUIControl			cntlPanel   = GetControl((int)Controls.PANEL_BACKGROUND);
				GUIImage				cntlChannelImg = (GUIImage)GetControl((int)Controls.CHANNEL_IMAGE_TEMPLATE);
				GUILabelControl cntlChannelLabel = (GUILabelControl) GetControl((int)Controls.CHANNEL_LABEL_TEMPLATE);
				GUILabelControl labelTime= (GUILabelControl) GetControl((int)Controls.LABEL_TIME1);
				GUIImage				cntlHeaderBkgImg = (GUIImage)GetControl((int)Controls.IMG_TIME1);
				GUIImage        cntlChannelTemplate = (GUIImage)GetControl((int)Controls.CHANNEL_TEMPLATE);
				
				m_bUseChannelLogos = cntlChannelImg!=null;
				if (m_bUseChannelLogos) cntlChannelImg.IsVisible=false;
				cntlChannelLabel.IsVisible=false;
				cntlHeaderBkgImg.IsVisible=false;
				labelTime.IsVisible=false;
				cntlChannelTemplate.IsVisible=false;
				int iLabelWidth=(cntlPanel.XPosition+cntlPanel.Width-labelTime.XPosition)/4; 

				// add labels for time blocks 1-4
				int iHour,iMin;
				iMin=m_dtTime.Minute;
				m_dtTime=m_dtTime.AddMinutes(-iMin);
				iMin = (iMin/m_iBlockTime)*m_iBlockTime;
				m_dtTime=m_dtTime.AddMinutes(iMin);
	      
				DateTime dt=new DateTime();
				dt=m_dtTime;

				for (int iLabel=0; iLabel <4; iLabel++)
				{
					xpos=iLabel*iLabelWidth + labelTime.XPosition;
					ypos=labelTime.YPosition;

					GUIImage img=GetControl((int)Controls.IMG_TIME1+iLabel) as GUIImage;
					if (img==null)
					{
						img = new GUIImage(GetID,(int)Controls.IMG_TIME1+iLabel,xpos,ypos,iLabelWidth-4,cntlHeaderBkgImg.RenderHeight,cntlHeaderBkgImg.FileName,0x0);
						img.AllocResources();
						GUIControl cntl2=(GUIControl)img;
						Add(ref cntl2);
					}
	        
					img.IsVisible=!m_bSingleChannel;
					img.Width=iLabelWidth-4;
					img.Height=cntlHeaderBkgImg.RenderHeight;
					img.SetFileName(cntlHeaderBkgImg.FileName);
					img.SetPosition(xpos,ypos);
					img.DoUpdate();

					GUILabelControl label = GetControl((int)Controls.LABEL_TIME1+iLabel) as GUILabelControl;
					if (label==null)
					{
						label=new GUILabelControl(GetID,(int)Controls.LABEL_TIME1+iLabel,xpos,ypos,iLabelWidth,cntlHeaderBkgImg.RenderHeight,labelTime.FontName, String.Empty,labelTime.TextColor, GUIControl.Alignment.ALIGN_CENTER,false);
						label.AllocResources();
						GUIControl cntl=(GUIControl)label;
						this.Add(ref cntl);
					}
					iHour=dt.Hour;
					iMin=dt.Minute;
					string strTime=dt.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
					label.Label=strTime;
					dt=dt.AddMinutes(m_iBlockTime);

					label.TextAlignment=GUIControl.Alignment.ALIGN_CENTER;
					label.IsVisible=!m_bSingleChannel;
					label.Width=iLabelWidth;
					label.Height=cntlHeaderBkgImg.RenderHeight;
					label.FontName=labelTime.FontName;
					label.TextColor=labelTime.TextColor;
					label.SetPosition(xpos,ypos);
				}

				// add channels...
				int iHeight=cntlPanel.Height+cntlPanel.YPosition-cntlChannelTemplate.YPosition;
				int iItemHeight=cntlChannelTemplate.Height;

				m_iChannels= (int)(((float)iHeight) / ((float)iItemHeight) );
				for (int iChan=0; iChan <m_iChannels; ++iChan)
				{
					xpos=cntlChannelTemplate.XPosition;
					ypos=cntlChannelTemplate.YPosition + iChan*iItemHeight;

					//this.Remove((int)Controls.IMG_CHAN1+iChan);
					GUIButton3PartControl imgBut = GetControl((int)Controls.IMG_CHAN1+iChan) as GUIButton3PartControl;
					if (imgBut==null)
					{
						string strChannelImageFileName=String.Empty;
						if (m_bUseChannelLogos) strChannelImageFileName = cntlChannelImg.FileName;
						imgBut = new GUIButton3PartControl(GetID,(int)Controls.IMG_CHAN1+iChan,xpos,ypos,
							cntlChannelTemplate.Width-2, cntlChannelTemplate.Height-2,
							"tvguide_button_selected_left.png",
							"tvguide_button_selected_middle.png",
							"tvguide_button_selected_right.png",
							"tvguide_button_light_left.png",
							"tvguide_button_light_middle.png",
							"tvguide_button_light_right.png",
							strChannelImageFileName);
						imgBut.AllocResources();
						GUIControl cntl=(GUIControl)imgBut;
						Add(ref cntl);
					}

					if (m_bUseChannelLogos) imgBut.TexutureIcon=cntlChannelImg.FileName;
					imgBut.Width=cntlChannelTemplate.Width-2;//labelTime.XPosition-cntlChannelImg.XPosition;
					imgBut.Height=cntlChannelTemplate.Height-2;//iItemHeight-2;
					imgBut.SetPosition(xpos,ypos);
					imgBut.FontName1=cntlChannelLabel.FontName;
					imgBut.TextColor1=cntlChannelLabel.TextColor;
					imgBut.Label1=String.Empty;
					imgBut.RenderLeft=false;
					imgBut.RenderRight=false;

					if (m_bUseChannelLogos)
					{
						imgBut.IconOffsetX=cntlChannelImg.XPosition;
						imgBut.IconOffsetY=cntlChannelImg.YPosition;
						imgBut.IconWidth=cntlChannelImg.RenderWidth;
						imgBut.IconHeight=cntlChannelImg.RenderHeight;
						imgBut.IconKeepAspectRatio=cntlChannelImg.KeepAspectRatio;
						imgBut.IconCentered=cntlChannelImg.Centered;
						imgBut.IconZoom=cntlChannelImg.Zoom;
					}
					imgBut.TextOffsetX1=cntlChannelLabel.XPosition;
					imgBut.TextOffsetY1=cntlChannelLabel.YPosition;
					imgBut.ColourDiffuse=0xffffffff;
					imgBut.DoUpdate();
				}

				UpdateHorizontalScrollbar();
				UpdateVerticalScrollbar();

				GetChannels();


				string day;
				switch (m_dtTime.DayOfWeek)
				{
					case DayOfWeek.Monday :	day = GUILocalizeStrings.Get(657);	break;
					case DayOfWeek.Tuesday :	day = GUILocalizeStrings.Get(658);	break;
					case DayOfWeek.Wednesday :	day = GUILocalizeStrings.Get(659);	break;
					case DayOfWeek.Thursday :	day = GUILocalizeStrings.Get(660);	break;
					case DayOfWeek.Friday :	day = GUILocalizeStrings.Get(661);	break;
					case DayOfWeek.Saturday :	day = GUILocalizeStrings.Get(662);	break;
					default:	day = GUILocalizeStrings.Get(663);	break;
				}
				day=String.Format("{0} {1}-{2}",day,m_dtTime.Day, m_dtTime.Month);
				GUIPropertyManager.SetProperty("#TV.Guide.Day", day);

				//2004 03 31 22 20 00
				string strStart=String.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", 
					m_dtTime.Year,m_dtTime.Month,m_dtTime.Day,
					m_dtTime.Hour,m_dtTime.Minute,0);
				DateTime dtStop=new DateTime();
				dtStop=m_dtTime;			
				dtStop=dtStop.AddMinutes( m_iBlocks*m_iBlockTime - 1);
				iMin=dtStop.Minute;
				string strEnd=String.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:00}", 
					dtStop.Year,dtStop.Month,dtStop.Day,
					dtStop.Hour,iMin,0);

				long iStart=Int64.Parse(strStart);
				long iEnd=Int64.Parse(strEnd);

	      
				m_recordings.Clear();
				TVDatabase.GetRecordings(ref m_recordings);

				if (m_iChannelOffset>m_channels.Count)
				{
					m_iChannelOffset=0;
					m_iCursorY=0;
				}

				for (int i=0; i < controlList.Count;++i)
				{
					GUIControl cntl=(GUIControl)controlList[i];
					if (cntl.GetID>=100)
					{
						cntl.IsVisible=false;
					}
				}

				if (m_bSingleChannel)
				{
					if (m_iCursorX==0)
					{
						//m_iSingleChannel=m_iCursorY + m_iChannelOffset;
						//if (m_iSingleChannel >= m_channels.Count) m_iSingleChannel-=m_channels.Count;
						//GUIButton3PartControl img=(GUIButton3PartControl)GetControl(m_iCursorY+(int)Controls.IMG_CHAN1);
						//if (null!=img) m_strCurrentChannel=img.Label1;
					}
					TVChannel channel=(TVChannel)m_channels[m_iSingleChannel];
					RenderSingleChannel(channel);
				}
				else
				{
					int chan=m_iChannelOffset ;
					for (int iChannel=0; iChannel <m_iChannels; iChannel++)
					{
						if (chan < m_channels.Count)
						{
							TVChannel channel=(TVChannel)m_channels[chan];
							RenderChannel(iChannel,channel,iStart,iEnd,selectCurrentShow);
						}
						chan++;
						if (chan >= m_channels.Count) chan=0;
					}

					// update selected channel 
					m_iSingleChannel=m_iCursorY + m_iChannelOffset;
					if (m_iSingleChannel >= m_channels.Count) m_iSingleChannel-=m_channels.Count;
					GUIButton3PartControl img=(GUIButton3PartControl)GetControl(m_iCursorY+(int)Controls.IMG_CHAN1);
					if (null!=img) m_strCurrentChannel=img.Label1;
				}
				UpdateVerticalScrollbar();
			}
		}

		void SetProperties()
		{
			if (m_channels==null) return;
			if (m_channels.Count==0) return;
			if (m_iCursorX==0 || m_currentProgram==null)
			{
				int channel=m_iCursorY+m_iChannelOffset;
				while (channel >= m_channels.Count) channel -=m_channels.Count;
				if (channel<0) channel=0;
				TVChannel chan=(TVChannel)m_channels[channel];
				string strChannel=chan.Name;
				if (strChannel==null) return;
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,strChannel);
				GUIPropertyManager.SetProperty("#TV.Guide.Title",String.Empty);
				GUIPropertyManager.SetProperty("#TV.Guide.Time",String.Empty);
				GUIPropertyManager.SetProperty("#TV.Guide.Description",String.Empty);
				GUIPropertyManager.SetProperty("#TV.Guide.Genre",String.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodeName",String.Empty);        
        GUIPropertyManager.SetProperty("#TV.Guide.SeriesNumber",String.Empty);        
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodeNumber",String.Empty);        
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodePart",String.Empty);        
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodeDetail",String.Empty);        
        GUIPropertyManager.SetProperty("#TV.Guide.Date",String.Empty);        
        GUIPropertyManager.SetProperty("#TV.Guide.StarRating",String.Empty);        
        GUIPropertyManager.SetProperty("#TV.Guide.Classification",String.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.Duration",String.Empty);
        GUIPropertyManager.SetProperty("#TV.Guide.TimeFromNow",String.Empty);
				if (!System.IO.File.Exists(strLogo))
				{
					strLogo="defaultVideoBig.png";
				}
				GUIPropertyManager.SetProperty("#TV.Guide.thumb",strLogo);
				m_iCurrentStartTime=0;
				m_iCurrentEndTime=0;
				m_strCurrentTitle=String.Empty;
				m_strCurrentTime=String.Empty;
				m_strCurrentChannel=strChannel;
				GUIControl.HideControl(GetID, (int)Controls.IMG_REC_PIN);
			}
			else if (m_currentProgram!=null) 
			{
				
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,m_currentProgram.Channel);
				string strTime=String.Format("{0}-{1}", 
					m_currentProgram.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
					m_currentProgram.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
              
				GUIPropertyManager.SetProperty("#TV.Guide.Title",m_currentProgram.Title);
				GUIPropertyManager.SetProperty("#TV.Guide.Time",strTime);
				GUIPropertyManager.SetProperty("#TV.Guide.Description",m_currentProgram.Description);
				GUIPropertyManager.SetProperty("#TV.Guide.Genre",m_currentProgram.Genre);
        GUIPropertyManager.SetProperty("#TV.Guide.Duration",m_currentProgram.Duration);
        GUIPropertyManager.SetProperty("#TV.Guide.TimeFromNow",m_currentProgram.TimeFromNow);
        if (m_currentProgram.Episode=="-") GUIPropertyManager.SetProperty("#TV.Guide.EpisodeName",String.Empty);
        else GUIPropertyManager.SetProperty("#TV.Guide.EpisodeName",m_currentProgram.Episode);        
        if (m_currentProgram.SeriesNum=="-") GUIPropertyManager.SetProperty("#TV.Guide.SeriesNumber",String.Empty);
        else GUIPropertyManager.SetProperty("#TV.Guide.SeriesNumber",m_currentProgram.SeriesNum);        
        if (m_currentProgram.EpisodeNum=="-") GUIPropertyManager.SetProperty("#TV.Guide.EpisodeNumber",String.Empty);
        else GUIPropertyManager.SetProperty("#TV.Guide.EpisodeNumber",m_currentProgram.EpisodeNum);        
        if(m_currentProgram.EpisodePart=="-") GUIPropertyManager.SetProperty("#TV.Guide.EpisodePart",String.Empty);
        else GUIPropertyManager.SetProperty("#TV.Guide.EpisodePart",m_currentProgram.EpisodePart);        
        if (m_currentProgram.Date=="-") GUIPropertyManager.SetProperty("#TV.Guide.Date",String.Empty);
        else GUIPropertyManager.SetProperty("#TV.Guide.Date",m_currentProgram.Date);        
        if (m_currentProgram.StarRating=="-") GUIPropertyManager.SetProperty("#TV.Guide.StarRating",String.Empty);
        else GUIPropertyManager.SetProperty("#TV.Guide.StarRating",m_currentProgram.StarRating);        
        if (m_currentProgram.Classification=="-") GUIPropertyManager.SetProperty("#TV.Guide.Classification",String.Empty);
        else GUIPropertyManager.SetProperty("#TV.Guide.Classification",m_currentProgram.Classification); 
        GUIPropertyManager.SetProperty("#TV.Guide.EpisodeDetail",m_currentProgram.EpisodeDetails);
				if (!System.IO.File.Exists(strLogo))
				{
					strLogo="defaultVideoBig.png";
				}
				GUIPropertyManager.SetProperty("#TV.Guide.thumb",strLogo);
				m_iCurrentStartTime=m_currentProgram.Start;
				m_iCurrentEndTime=m_currentProgram.End;
				m_strCurrentTitle=m_currentProgram.Title;
				m_strCurrentTime=strTime;
				m_strCurrentChannel=m_currentProgram.Channel;

				bool bRecording=false;
				bool bSeries=false;
				bool bConflict=false;
				if (m_recordings!=null)
				{
					foreach (TVRecording record in m_recordings)
					{
						if (record.IsRecordingProgram(m_currentProgram,true) ) 
						{	
							if (ConflictManager.IsConflict(record))
								bConflict=true;
							if (record.RecType !=TVRecording.RecordingType.Once)
								bSeries=true;
							bRecording=true;
							break;
						}
					}
				}
				if (bRecording)
				{
					GUIImage img=(GUIImage)GetControl((int)Controls.IMG_REC_PIN);

					if (bConflict)
						img.SetFileName(Thumbs.TvConflictRecordingIcon);
					else if (bSeries)
						img.SetFileName(Thumbs.TvRecordingSeriesIcon);
					else
						img.SetFileName(Thumbs.TvRecordingIcon);
					GUIControl.ShowControl(GetID, (int)Controls.IMG_REC_PIN);
				}
				else
					GUIControl.HideControl(GetID, (int)Controls.IMG_REC_PIN);
			}
		}//void SetProperties()

		void RenderSingleChannel(TVChannel channel)
		{
      int chan=m_iChannelOffset ;
      for (int iChannel=0; iChannel <m_iChannels; iChannel++)
			{
        if (chan < m_channels.Count)    
				{
					TVChannel tvChan=(TVChannel)m_channels[chan];
          
					string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,tvChan.Name);                   
					if (System.IO.File.Exists(strLogo))
					{
						GUIButton3PartControl img=GetControl(iChannel+(int)Controls.IMG_CHAN1) as GUIButton3PartControl;
						if (img!=null) 
						{
							if (m_bUseChannelLogos) img.TexutureIcon=strLogo;       
							img.Label1=tvChan.Name;
							img.IsVisible=true;
						}
					}
					else
					{
						GUIButton3PartControl img=GetControl(iChannel+(int)Controls.IMG_CHAN1) as GUIButton3PartControl;
						if (img!=null) 
						{
							if (m_bUseChannelLogos) img.TexutureIcon="defaultVideoBig.png";
							img.Label1=tvChan.Name;
							img.IsVisible=true;
						}
					}
				}
        chan++;
        if (chan >= m_channels.Count) chan=0;
			}

			ArrayList programs=new ArrayList();
			DateTime dtStart=DateTime.Now;
			DateTime dtEnd  =dtStart.AddDays(30);
			long iStart=Utils.datetolong(dtStart);
			long iEnd=Utils.datetolong(dtEnd);
			TVDatabase.GetProgramsPerChannel(channel.Name,iStart,iEnd,ref programs);
			m_iTotalPrograms=programs.Count;
			if (m_iTotalPrograms==0) m_iTotalPrograms=m_iChannels;

      // ichan = number of rows
			for (int ichan=0; ichan < m_iChannels;++ichan)
			{
				GUIButton3PartControl imgCh=GetControl(ichan+(int)Controls.IMG_CHAN1) as GUIButton3PartControl;
				imgCh.TexutureIcon="";

				int iStartXPos=GetControl(0+(int)Controls.LABEL_TIME1).XPosition;              
				int height=GetControl((int)Controls.IMG_CHAN1+1).YPosition;
				height-=GetControl((int)Controls.IMG_CHAN1).YPosition;
				int width=GetControl((int)Controls.LABEL_TIME1+1).XPosition;
				width-=GetControl((int)Controls.LABEL_TIME1).XPosition;
   
				int iTotalWidth=width*m_iBlocks;

				TVProgram program;
				int offset=m_iProgramOffset;
				if (offset+ichan < programs.Count)
					program=(TVProgram)programs[offset+ichan];
				else
				{
					program = new TVProgram();
					if (ichan==0)
					{
						program.Start=Utils.datetolong(DateTime.Now);
						program.End=Utils.datetolong(DateTime.Now);
						program.Title="-";
						program.Genre="-";
					}
					program.Channel=channel.Name;
				}

				int ypos=GetControl(ichan+(int)Controls.IMG_CHAN1).YPosition;
				int iControlId=100+ichan*RowID+0*ColID;
				GUIButton3PartControl img =(GUIButton3PartControl)GetControl(iControlId);

				if (img==null)
				{
					img = new GUIButton3PartControl(GetID,iControlId,iStartXPos,ypos,iTotalWidth,height-2,
						"tvguide_button_selected_left.png",
						"tvguide_button_selected_middle.png",
						"tvguide_button_selected_right.png",
						"tvguide_button_light_left.png",
						"tvguide_button_light_middle.png",
						"tvguide_button_light_right.png",
						String.Empty);
					img.AllocResources();
					img.ColourDiffuse=GetColorForGenre(program.Genre);
					GUIControl cntl=(GUIControl)img;
					Add(ref cntl);
				}
				else
				{
					img.TexutureFocusLeftName="tvguide_button_selected_left.png";
					img.TexutureFocusMidName="tvguide_button_selected_middle.png";
					img.TexutureFocusRightName="tvguide_button_selected_right.png";
					img.TexutureNoFocusLeftName="tvguide_button_light_left.png";
					img.TexutureNoFocusMidName="tvguide_button_light_middle.png";
					img.TexutureNoFocusRightName="tvguide_button_light_right.png";
					img.Focus=false;
					img.SetPosition(iStartXPos,ypos);
					img.Width=iTotalWidth;
					img.ColourDiffuse=GetColorForGenre(program.Genre);
					img.IsVisible=true;
					img.DoUpdate();
				}
				img.RenderLeft=false;
				img.RenderRight=false;
				bool bRecording=false;
				bool bSeries=false;
				bool bConflict=false;
				foreach (TVRecording record in m_recordings)
				{
					if (record.IsRecordingProgram(program,true) ) 
					{
						if (ConflictManager.IsConflict(record))
							bConflict=true;
						if (record.RecType !=TVRecording.RecordingType.Once)
							bSeries=true;
						bRecording=true;
						break;
					}
				}

				img.Data=program.Clone();
				img.ColourDiffuse=GetColorForGenre(program.Genre);
				height=height-10;
				height/=2;
				int iWidth=iTotalWidth;
				if (iWidth>10) iWidth -=10;
				else iWidth=1;

				DateTime dt=DateTime.Now;

				img.TextOffsetX1=5;
				img.TextOffsetY1=5;
				img.FontName1="font13";
				img.TextColor1=0xffffffff;

				string strTimeSingle=String.Empty;
				string strTime=String.Empty;
				
				img.Label1=program.Title;
				if (program.Start != 0)
				{
					strTimeSingle=String.Format("{0}", 
						program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
				
					strTime=String.Format("{0}-{1}", 
						program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
						program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));				

					if (program.StartTime.Date != DateTime.Now.Date)
					{
						img.Label1=String.Format("{0} {1}",Utils.GetShortDayString(program.StartTime),program.Title);
					}
				}
				GUILabelControl labelTemplate;
				if (program.IsRunningAt(dt))
				{
					labelTemplate=GetControl((int)Controls.LABEL_TITLE_DARK_TEMPLATE) as GUILabelControl;
				}
				else
					labelTemplate=GetControl((int)Controls.LABEL_TITLE_TEMPLATE) as GUILabelControl;
            
				if (labelTemplate!=null) 
				{
					img.FontName1=labelTemplate.FontName;
					img.TextColor1=labelTemplate.TextColor;
				}
				img.TextOffsetX2=5;
				img.TextOffsetY2=img.Height/2;
				img.FontName2="font13";
				img.TextColor2=0xffffffff;			
				img.Label2="";
				if (program.IsRunningAt(dt))
				{
					img.TextColor2=0xff101010;			
					labelTemplate=GetControl((int)Controls.LABEL_GENRE_DARK_TEMPLATE) as GUILabelControl;
				}
				else 
					labelTemplate=GetControl((int)Controls.LABEL_GENRE_TEMPLATE) as GUILabelControl;

				if (labelTemplate!=null) 
				{
					img.FontName2=labelTemplate.FontName;
					img.TextColor2=labelTemplate.TextColor;
					img.Label2=program.Genre;
				}
				imgCh.Label1=strTimeSingle;
				imgCh.TexutureIcon="";

				if (program.IsRunningAt(dt))
				{
					img.TexutureNoFocusLeftName="tvguide_button_left.png";
					img.TexutureNoFocusMidName="tvguide_button_middle.png";
					img.TexutureNoFocusRightName="tvguide_button_right.png";
				}

				img.SetPosition(img.XPosition, img.YPosition);

				img.TexutureIcon=String.Empty;
				if (HasNotify(program))
					img.TexutureIcon=Thumbs.TvNotifyIcon;
				if (bRecording)
				{
					if (bConflict)
						img.TexutureIcon=Thumbs.TvConflictRecordingIcon;
					else if (bSeries)
						img.TexutureIcon=Thumbs.TvRecordingSeriesIcon;
					else
						img.TexutureIcon=Thumbs.TvRecordingIcon;
				}
			}
		}//void RenderSingleChannel(TVChannel channel)

		void RenderChannel(int iChannel,TVChannel channel, long iStart, long iEnd, bool selectCurrentShow)
		{
			string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,channel.Name);
			if (System.IO.File.Exists(strLogo))
			{
				GUIButton3PartControl img=GetControl(iChannel+(int)Controls.IMG_CHAN1) as GUIButton3PartControl;
				if (img!=null) 
				{
					if (m_bUseChannelLogos) img.TexutureIcon=strLogo;       
					img.Label1=channel.Name;
					img.IsVisible=true;
				}
			}
			else
			{
				GUIButton3PartControl img=GetControl(iChannel+(int)Controls.IMG_CHAN1) as GUIButton3PartControl;
				if (img!=null) 
				{
          if (m_bUseChannelLogos) img.TexutureIcon="defaultVideoBig.png";
					img.Label1=channel.Name;
					img.IsVisible=true;
				}
			}


			ArrayList programs=new ArrayList();
			TVDatabase.GetProgramsPerChannel(channel.Name,iStart,iEnd,ref programs);
			if (programs.Count==0)
			{
				DateTime dt=Utils.longtodate(iEnd);
				//dt=dt.AddMinutes(m_iBlockTime);
				long iProgEnd=Utils.datetolong(dt);
				TVProgram prog = new TVProgram();
				prog.Start=iStart;
				prog.End=iProgEnd;
				prog.Channel=channel.Name;
				prog.Title=GUILocalizeStrings.Get(736);//no tvguide data
				programs.Add(prog);
			}
			if (programs.Count>0)
			{
				int iProgram=0;
				foreach (TVProgram program in programs)
				{
					string strTitle=program.Title;
					bool bStartsBefore=false;
					bool bEndsAfter=false;
					if (program.End<=iStart) continue;
					if (program.Start<iStart) bStartsBefore=true;
					if (program.End>iEnd) bEndsAfter=true;
          
					if( iProgram==m_iCursorX-1 && iChannel==m_iCursorY)
					{

						m_currentProgram=program;
						SetProperties();
					}
					int width=GetControl((int)Controls.LABEL_TIME1+1).XPosition;
					width-=GetControl((int)Controls.LABEL_TIME1).XPosition;

					int height=GetControl((int)Controls.IMG_CHAN1+1).YPosition;
					height-=GetControl((int)Controls.IMG_CHAN1).YPosition;

					DateTime dtBlokStart=new DateTime();
					dtBlokStart=m_dtTime;
					dtBlokStart=dtBlokStart.AddMilliseconds(-dtBlokStart.Millisecond);
					dtBlokStart=dtBlokStart.AddSeconds(-dtBlokStart.Second);

					bool bSeries=false;
					bool bRecording=false;
					bool bConflict=false;
					foreach (TVRecording record in m_recordings)
					{
						if (record.IsRecordingProgram(program,true) ) 
						{
							if (ConflictManager.IsConflict(record))
								bConflict=true;
							if (record.RecType !=TVRecording.RecordingType.Once)
								bSeries=true;
							bRecording=true;
							break;
						}
					}

					int iStartXPos=0;
					int iEndXPos=0;
					for (int iBlok=0; iBlok < m_iBlocks; iBlok++)
					{
						float fWidthEnd=(float)width;
						DateTime dtBlokEnd=dtBlokStart.AddSeconds(m_iBlockTime*60);
						if ( program.RunningAt(dtBlokStart, dtBlokEnd))
						{
							dtBlokEnd=dtBlokStart.AddSeconds(m_iBlockTime*60);
							if (program.EndTime<dtBlokEnd)
							{
								TimeSpan dtSpan=dtBlokEnd-program.EndTime;
								int iEndMin=m_iBlockTime-(dtSpan.Minutes);
                
								fWidthEnd=( ((float)iEndMin) / ((float)m_iBlockTime) )*((float)(width));
								if (bEndsAfter) fWidthEnd=(float)width;
							}

							if (iStartXPos==0)
							{
								TimeSpan ts=program.StartTime-dtBlokStart;
								int iStartMin=ts.Minutes;
                if (ts.Seconds==59) iStartMin+=1;
								float fWidth=( ((float)iStartMin) / ((float)m_iBlockTime) )*((float)(width));

								if (bStartsBefore) fWidth=0;

								iStartXPos=GetControl(iBlok+(int)Controls.LABEL_TIME1).XPosition;              
								iStartXPos += (int)fWidth;
								iEndXPos=GetControl(iBlok+(int)Controls.LABEL_TIME1).XPosition+(int)fWidthEnd;              
							}
							else
							{
								iEndXPos=GetControl(iBlok+(int)Controls.LABEL_TIME1).XPosition+(int)fWidthEnd;              
							}
						}
						dtBlokStart=dtBlokStart.AddMinutes(m_iBlockTime);
					}

					if (iStartXPos>=0)
					{
						int ypos=GetControl(iChannel+(int)Controls.IMG_CHAN1).YPosition;
						int iControlId=100+iChannel*RowID+iProgram*ColID;
						GUIButton3PartControl img =(GUIButton3PartControl)GetControl(iControlId);
						int iWidth=iEndXPos-iStartXPos;
						if (iWidth > 3) iWidth-=3;
						else iWidth=1;
						if (img==null)
						{
							img = new GUIButton3PartControl(GetID,iControlId,iStartXPos,ypos,iWidth,height-2,
								"tvguide_button_selected_left.png",
								"tvguide_button_selected_middle.png",
								"tvguide_button_selected_right.png",
								"tvguide_button_light_left.png",
								"tvguide_button_light_middle.png",
								"tvguide_button_light_right.png",
								String.Empty);        
							img.AllocResources();
							GUIControl cntl=(GUIControl)img;
							Add(ref cntl);
						}
						else
						{
							img.TexutureFocusLeftName="tvguide_button_selected_left.png";
							img.TexutureFocusMidName="tvguide_button_selected_middle.png";
							img.TexutureFocusRightName="tvguide_button_selected_right.png";
							img.TexutureNoFocusLeftName="tvguide_button_light_left.png";
							img.TexutureNoFocusMidName="tvguide_button_light_middle.png";
							img.TexutureNoFocusRightName="tvguide_button_light_right.png";
							img.Focus=false;
							img.SetPosition(iStartXPos,ypos);
							img.Width=iWidth;
							img.IsVisible=true;
							img.DoUpdate();
						}
						img.RenderLeft=false;
						img.RenderRight=false;

						img.TexutureIcon=String.Empty;
						if (HasNotify(program))
							img.TexutureIcon=Thumbs.TvNotifyIcon;
						if (bRecording)
						{
							if (bConflict)
								img.TexutureIcon=Thumbs.TvConflictRecordingIcon;
							else if (bSeries)
								img.TexutureIcon=Thumbs.TvRecordingSeriesIcon;
							else
								img.TexutureIcon=Thumbs.TvRecordingIcon;
						}

						img.Data=program.Clone();
						img.ColourDiffuse=GetColorForGenre(program.Genre);
						height=height-10;
						height/=2;
						iWidth=iEndXPos-iStartXPos;
						if (iWidth>10) iWidth -=10;
						else iWidth=1;

						DateTime dt=DateTime.Now;

						img.TextOffsetX1=5;
						img.TextOffsetY1=5;
						img.FontName1="font13";
						img.TextColor1=0xffffffff;
						img.Label1=strTitle;
						GUILabelControl labelTemplate;
						if (program.IsRunningAt(dt))
						{
							labelTemplate=GetControl((int)Controls.LABEL_TITLE_DARK_TEMPLATE) as GUILabelControl;
						}
						else
							labelTemplate=GetControl((int)Controls.LABEL_TITLE_TEMPLATE) as GUILabelControl;
            
						if (labelTemplate!=null) 
						{
							img.FontName1=labelTemplate.FontName;
							img.TextColor1=labelTemplate.TextColor;
							img.TextColor2=labelTemplate.TextColor;
						}
						img.TextOffsetX2=5;
						img.TextOffsetY2=img.Height/2;
						img.FontName2="font13";
						img.TextColor2=0xffffffff;						

						if (program.IsRunningAt(dt))
							labelTemplate=GetControl((int)Controls.LABEL_GENRE_DARK_TEMPLATE) as GUILabelControl;
						else 
							labelTemplate=GetControl((int)Controls.LABEL_GENRE_TEMPLATE) as GUILabelControl;
						if (labelTemplate!=null) 
						{
							img.FontName2=labelTemplate.FontName;
							img.TextColor2=labelTemplate.TextColor;
              img.Label2=program.Genre;
						}

						if (program.IsRunningAt(dt))
						{
							img.TexutureNoFocusLeftName="tvguide_button_left.png";
							img.TexutureNoFocusMidName="tvguide_button_middle.png";
							img.TexutureNoFocusRightName="tvguide_button_right.png";
							if( selectCurrentShow && iChannel==m_iCursorY)
							{
								m_iCursorX=iProgram+1;
								m_currentProgram=program;
								m_dtStartTime=program.StartTime;
								SetProperties();
							}
						}

						if (bEndsAfter)
						{
							img.RenderRight=true;

							img.TexutureFocusRightName="tvguide_arrow_selected_right.png";
							img.TexutureNoFocusRightName="tvguide_arrow_light_right.png";
							if (program.IsRunningAt(dt))
							{
								img.TexutureNoFocusRightName="tvguide_arrow_right.png";
							}
						}
						if (bStartsBefore)
						{
							img.RenderLeft=true;
							img.TexutureFocusLeftName="tvguide_arrow_selected_left.png";
							img.TexutureNoFocusLeftName="tvguide_arrow_light_left.png";
							if (program.IsRunningAt(dt))
							{
								img.TexutureNoFocusLeftName="tvguide_arrow_left.png";
							}
						}

						img.SetPosition(img.XPosition, img.YPosition);
						img.DoUpdate();
						iProgram++;
					}
				}
			}
		}//void RenderChannel(int iChannel,TVChannel channel, long iStart, long iEnd, bool selectCurrentShow)

		int ProgramCount(int iChannel)
		{
			int iProgramCount=0;
			for (int iProgram=0; iProgram <m_iBlocks*5; ++iProgram)
			{
				int iControlId=100+iChannel*RowID+iProgram*ColID;
				GUIControl cntl= GetControl(iControlId);
				if ( cntl!=null && cntl.IsVisible) iProgramCount++;
				else return iProgramCount;
			}
			return iProgramCount;
		}

    
		void OnDown()
		{
			UnFocus();
			if (m_iCursorY<0)
			{
				m_iCursorX=0;
				m_iCursorY=0;
				SetFocus();
				GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL).Focus=false;
				return;
			}

			if (m_bSingleChannel)
			{
        if (m_iCursorY+1 < m_iChannels)
        {
          m_iCursorY++;
          Update(false);
        }
        else 
        {
					 
          if (m_iCursorY + m_iProgramOffset+1 < m_iTotalPrograms)
          {
            m_iProgramOffset++;
            Update(false);
          }
        }
				SetFocus();
        
				UpdateCurrentProgram();
				SetProperties();
				return;
			}

			if (m_iCursorX==0)
			{
				if (m_iCursorY+1 < m_iChannels)
				{
					m_iCursorY++;
          Update(false);
				}
				else
				{
					m_iChannelOffset++;
					if ( m_iChannelOffset>0 && m_iChannelOffset >= m_channels.Count)  m_iChannelOffset-=m_channels.Count;
          Update(false);
				}
				SetFocus();
				SetProperties();
				return;
			}
			int iCurY=m_iCursorY;
			int iCurOff=m_iChannelOffset;
			int iX1,iX2;
			//      int iNewWidth=0;
			int iControlId=100+m_iCursorY*RowID+(m_iCursorX-1)*ColID;
			GUIControl control=GetControl(iControlId);
			if (control==null) return;
			iX1=control.XPosition;
			iX2=control.XPosition+control.Width;

			bool bOK=false;
      int iMaxSearch=m_channels.Count;
			while (!bOK && (iMaxSearch>0))
			{
        iMaxSearch--;
				if (m_iCursorY+1 < m_iChannels )
				{
					m_iCursorY++;
				}
				else
				{
					m_iChannelOffset++;
					if (m_iChannelOffset>0 && m_iChannelOffset>=m_channels.Count)  m_iChannelOffset -=m_channels.Count;
					Update(false);
				}

				for (int x=1; x < ColID; x++)
				{
					iControlId=100+m_iCursorY*RowID+(x-1)*ColID;
					control=GetControl(iControlId);
					if (control !=null) 
					{
						TVProgram prog=(TVProgram)control.Data;
						if (x==1 && m_dtStartTime  < prog.StartTime ||m_bSingleChannel)
						{
							m_iCursorX=x;
							bOK=true;
							break;
						}

						if (m_dtStartTime >= prog.StartTime && m_dtStartTime < prog.EndTime)
						{
							m_iCursorX=x;
							bOK=true;
							break;
						}
					}
				}
			}
			if (!bOK)
			{
				m_iCursorY=iCurY;
				m_iChannelOffset=iCurOff;
			}
			if (iCurOff==m_iChannelOffset)
			{
				Correct();
				UpdateCurrentProgram();
				return;
			}
      
			Correct();
			Update(false);  
			SetFocus();
		}

		void OnUp()
		{
			UnFocus();
			if (!m_bSingleChannel && m_iCursorX==0 && m_iCursorY==0 && m_iChannelOffset==0)
			{
				m_iCursorY=-1;
				GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL).Focus=true;
				return;
			}
			if (m_bSingleChannel)
			{
				if (m_iCursorY>0)
				{
					m_iCursorY--;
				}
				else if (m_iProgramOffset>0)
				{
					m_iProgramOffset--;
				}

        Update(false);
				
				SetFocus();
				UpdateCurrentProgram();
				SetProperties();
				return;
			}

			if (m_iCursorX==0)
			{
				if (m_iCursorY==0)
				{
					if (m_iChannelOffset>0) 
					{
						m_iChannelOffset--;
						Update(false);
					}
				}
				else 
				{
					m_iCursorY--;
          Update(false);
				}
				SetFocus();
				SetProperties();
				return;
			}
			int iCurY=m_iCursorY;
			int iCurOff=m_iChannelOffset;
      
			int iX1,iX2;
			int iControlId=100+m_iCursorY*RowID+(m_iCursorX-1)*ColID;
			GUIControl control=GetControl(iControlId);
			if (control==null) return;
			iX1=control.XPosition;
			iX2=control.XPosition+control.Width;

			bool bOK=false;
      int iMaxSearch=m_channels.Count;
      while (!bOK && (iMaxSearch>0))
      {
        iMaxSearch--;
				if (m_iCursorY==0)
				{
					if (m_iChannelOffset>0) 
					{
						m_iChannelOffset--;
						Update(false);
					}
					else break;
				}
				else 
				{
					m_iCursorY--;
				}

				for (int x=1; x < ColID; x++)
				{
					iControlId=100+m_iCursorY*RowID+(x-1)*ColID;
					control=GetControl(iControlId);
					if (control !=null) 
					{
						TVProgram prog=(TVProgram)control.Data;
						if (x==1 && m_dtStartTime  < prog.StartTime||m_bSingleChannel )
						{
							m_iCursorX=x;
							bOK=true;
							break;
						}
						if (m_dtStartTime >= prog.StartTime && m_dtStartTime < prog.EndTime)
						{
							m_iCursorX=x;
							bOK=true;
							break;
						}
					}
					else
					{
						break;
					}
				}
        
			}
			if (!bOK)
			{
				m_iCursorY=iCurY;
				m_iChannelOffset=iCurOff;
			}

			if (iCurOff==m_iChannelOffset)
			{
				Correct();
				UpdateCurrentProgram();
				return;
			}

			Correct();
			Update(false);
			SetFocus();
		}

		void OnLeft()
		{
			if (m_iCursorY<0) return;
			UnFocus();
			if (m_iCursorX==0)
			{
				m_dtTime=m_dtTime.AddMinutes(-m_iBlockTime);
        // Check new day
        int iDay=m_dtTime.DayOfYear - DateTime.Now.DayOfYear;
        if (iDay<0)
          m_dtTime=m_dtTime.AddMinutes(+m_iBlockTime);
			}
			else 
			{
				if (m_iCursorX==1)
				{
					m_iCursorX=0;
          
					SetFocus();
					SetProperties();
					return;
				}
				m_iCursorX--;
				Correct();
				UpdateCurrentProgram();
				if (m_currentProgram!=null) m_dtStartTime=m_currentProgram.StartTime;
				return;
			}
			Correct();
			Update(false);
			SetFocus();
			if (m_currentProgram!=null) m_dtStartTime=m_currentProgram.StartTime;
		}

		void UpdateCurrentProgram()
		{
			if (m_iCursorY<0) return;
			if (m_iCursorX<0) return;
			if (m_iCursorX==0)
			{
				SetProperties();
				SetFocus();
				return;
			}
			int iControlId=100+m_iCursorY*RowID+(m_iCursorX-1)*ColID;
			GUIButton3PartControl img=(GUIButton3PartControl)GetControl(iControlId);
			if (null!=img)
			{
				SetFocus();
				m_currentProgram=(TVProgram)img.Data;
				SetProperties();
          
			}
		}

		void OnRight()
		{
			if (m_iCursorY<0) return;
			UnFocus();
			if (m_iCursorX < ProgramCount(m_iCursorY) )
			{
				m_iCursorX++;
				Correct();
				UpdateCurrentProgram();
				if (m_currentProgram!=null) m_dtStartTime=m_currentProgram.StartTime;
				return;
			}
			else
			{
				m_dtTime=m_dtTime.AddMinutes(m_iBlockTime);
        // Check new day
        int iDay=m_dtTime.DayOfYear - DateTime.Now.DayOfYear;
        if (iDay>=MaxDaysInGuide)
          m_dtTime=m_dtTime.AddMinutes(-m_iBlockTime);
			}
			Correct();
			Update(false);      
			SetFocus();
			if (m_currentProgram!=null) m_dtStartTime=m_currentProgram.StartTime;
		}
    
		void UnFocus()
		{
			if (m_iCursorY<0) return;
			if (m_iCursorX==0)
			{
				int controlid=(int)Controls.IMG_CHAN1+m_iCursorY;
				GUIControl.UnfocusControl(GetID,controlid);
			}
			else
			{
				Correct();
				int iControlId=100+m_iCursorY*RowID+(m_iCursorX-1)*ColID;
				GUIButton3PartControl img=GetControl(iControlId) as GUIButton3PartControl;
				if (null!=img && img.IsVisible)
				{
					if (m_currentProgram!=null)
						img.ColourDiffuse=GetColorForGenre(m_currentProgram.Genre);
				}
				GUIControl.UnfocusControl(GetID,iControlId);
			}
		}
		void SetFocus()
		{
			if (m_iCursorY<0) return;
			if (m_iCursorX==0)
			{
				int controlid=(int)Controls.IMG_CHAN1+m_iCursorY;
				GUIControl.FocusControl(GetID,controlid);
			}
			else
			{
				Correct();
				int iControlId=100+m_iCursorY*RowID+(m_iCursorX-1)*ColID;
				GUIButton3PartControl img=GetControl(iControlId) as GUIButton3PartControl;
				if (null!=img && img.IsVisible)
				{
					img.ColourDiffuse=0xffffffff;
					m_currentProgram=img.Data as TVProgram;
					SetProperties();
				}
				GUIControl.FocusControl(GetID,iControlId);
			}
		}

		void Correct()
		{
			int iControlId;
			if (m_iCursorX<0)
				m_iCursorX=0;
			if (m_iCursorX>0)
			{
				while (m_iCursorX>0)
				{
					iControlId=100+m_iCursorY*RowID+(m_iCursorX-1)*ColID;
					GUIControl cntl=GetControl(iControlId);
					if (cntl==null) m_iCursorX--;
					else if (!cntl.IsVisible) m_iCursorX--;
					else break;
				}
			}
			if (m_iCursorY<0)
				m_iCursorY=0;
			if (!m_bSingleChannel)
			{
				while (m_iCursorY>0)
				{
					iControlId=100+m_iCursorY*RowID+(0)*ColID;
					GUIControl cntl=GetControl(iControlId);
					if (cntl==null) m_iCursorY--;
					else if (!cntl.IsVisible) m_iCursorY--;
					else break;
				}
			}
		}

		void Import()
		{
			GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			if (dlgProgress!=null)
			{
				dlgProgress.SetHeading(606);
				dlgProgress.SetLine(1,String.Empty);
				dlgProgress.SetLine(2,String.Empty);
				dlgProgress.StartModal(GetID);
				dlgProgress.Progress();
			}

			XMLTVImport import = new XMLTVImport();
			import.ShowProgress +=new MediaPortal.TV.Database.XMLTVImport.ShowProgressHandler(import_ShowProgress);
			bool bSucceeded=import.Import(m_strTVGuideFile,true);
			if (dlgProgress!=null)
				dlgProgress.Close();

			GUIDialogOK pDlgOK	= (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			if (pDlgOK!=null)
			{
				int iChannels=import.ImportStats.Channels;
				int iPrograms=import.ImportStats.Programs;
				string strChannels=GUILocalizeStrings.Get(627)+ iChannels.ToString();
				string strPrograms=GUILocalizeStrings.Get(628)+ iPrograms.ToString();
				pDlgOK.SetHeading(606);
				pDlgOK.SetLine(1,strChannels+ " "+strPrograms);
				pDlgOK.SetLine(2,import.ImportStats.StartTime.ToShortDateString() + " - " + import.ImportStats.EndTime.ToShortDateString());
				if (!bSucceeded)
				{
					pDlgOK.SetLine(1,608);
					pDlgOK.SetLine(2,import.ErrorMessage);
				}
				else
				{
					pDlgOK.SetHeading(606);
					pDlgOK.SetLine(1,609);
				}
				pDlgOK.DoModal(GetID);
			}
			m_iChannelOffset=0;
			m_bSingleChannel=false;
			m_iChannelOffset=0;
			m_iCursorX=0;
			m_iCursorY=0;
			Update(false);
			SetFocus();
		}

		// Define the event handlers.
		private static void OnChanged(object source, FileSystemEventArgs e)
		{
			if (String.Compare(e.Name,"tvguide.xml",true)==0)
				StartImportXML();
		}
		private static void OnRenamed(object source, RenamedEventArgs e)
		{
			if (String.Compare(e.Name,"tvguide.xml",true)==0)
				StartImportXML();
		}
		static void StartImportXML()
		{
			try
			{
				//check if file can be opened for reading....
				Encoding fileEncoding = Encoding.Default;				
				FileStream streamIn = File.Open(m_strTVGuideFile,FileMode.Open,FileAccess.Read,FileShare.Read);
				StreamReader fileIn = new StreamReader(streamIn, fileEncoding, true);
				fileIn.Close();
				streamIn.Close();
			}
			catch(Exception)
			{
				return;
			}

			m_watcher.EnableRaisingEvents = false;
			Thread workerThread =new Thread( new ThreadStart(ThreadFunctionImportTVGuide)); 
			workerThread.Priority=ThreadPriority.BelowNormal;
			workerThread.Start();
		} 

		static void ThreadFunctionImportTVGuide()
		{
			Log.Write(@"detected new tvguide ->import new tvguide");
			try
			{
				XMLTVImport import= new XMLTVImport();
				import.Import(m_strTVGuideFile,false);
			}
			catch(Exception)
			{
			}

			try
			{
				//
				// Make sure the file exists before we try to do any processing, thus if the file doesn't
				// exist we we'll save ourselves from getting a file not found exception.
				//
				if(File.Exists(m_strTVGuideFile))
				{
					using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
					{
						string strFileTime=System.IO.File.GetLastWriteTime(m_strTVGuideFile).ToString();
						xmlreader.SetValue("tvguide","date",strFileTime);
					}
				}
			
			}
			catch (Exception)
			{
			}
			m_watcher.EnableRaisingEvents = true;
			Log.Write(@"import done");
		}

		public override void Process()
		{
			OnKeyTimeout();
			if (m_bNeedUpdate)
			{
				m_bNeedUpdate=false;
				Update(false);
				SetFocus();
			}

			GUIImage vertLine=GetControl((int)Controls.VERTICAL_LINE) as GUIImage;
			if (vertLine!=null)
			{
				if (m_bSingleChannel)
				{
					vertLine.IsVisible=false;
				}
				else
				{
					vertLine.IsVisible=true;
        
					DateTime dateNow=DateTime.Now.Date;
					DateTime datePrev=m_dtTime.Date;
					TimeSpan ts=dateNow-datePrev;
					if (ts.TotalDays==1)
					{
						m_dtTime=DateTime.Now;
					}
          
          
					if (m_dtTime.Date.Equals(DateTime.Now.Date) )
					{
						int iStartX = GetControl((int)Controls.LABEL_TIME1).XPosition;
						int iWidth  = GetControl((int)Controls.LABEL_TIME1+1).XPosition - iStartX;
						iWidth*=4;

						int iMin=m_dtTime.Minute;
						int iStartTime=m_dtTime.Hour*60+iMin;
						int iCurTime=DateTime.Now.Hour*60+DateTime.Now.Minute;
						if (iCurTime>=iStartTime)
							iCurTime-=iStartTime;
						else
							iCurTime=24*60+iCurTime-iStartTime;

						int iTimeWidth= (m_iBlocks*m_iBlockTime);
						float fpos=((float)iCurTime) / ((float)(iTimeWidth));
						fpos*=(float)iWidth;
						fpos+=(float)iStartX;
						vertLine.IsVisible=true;
						vertLine.XPosition=(int)fpos;
						vertLine.Select(0);
					}
					else vertLine.IsVisible=false;
				}
			}
		}

		public override void Render(float timePassed)
		{
			lock (this)
			{
				GUIImage vertLine=GetControl((int)Controls.VERTICAL_LINE) as GUIImage;
				base.Render (timePassed);
				if (vertLine!=null)
					vertLine.Render(timePassed);
			}
		}

		void ShowContextMenu()
		{
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				
				if (m_strCurrentChannel.Length>0)
					dlg.AddLocalizedString( 938);// View this channel

				dlg.AddLocalizedString( 939);// Switch mode
				if (m_currentProgram!=null && m_strCurrentChannel.Length>0 && m_strCurrentTitle.Length>0)
				{
					dlg.AddLocalizedString( 264);// Record
				}
				dlg.AddLocalizedString( 937);// Reload tvguide
				dlg.AddLocalizedString( 971);// Group

				dlg.DoModal( GetID);
				if (dlg.SelectedLabel==-1) return;
				switch (dlg.SelectedId)
				{
					case 971: //group
						dlg.Reset();
						dlg.SetHeading(GUILocalizeStrings.Get(971));//Group
						foreach (TVGroup group in GUITVHome.Navigator.Groups)
						{
							dlg.Add(group.GroupName);
						}
						dlg.DoModal( GetID);
						if (dlg.SelectedLabel==-1) return;
						GUITVHome.Navigator.SetCurrentGroup(dlg.SelectedLabelText);
						GetChannels();
						Update(false);
						SetFocus();
					break;

					case 937: //import tvguide
						Import();
						Update(false);
						SetFocus();
					break;
					
					case 938: // view channel
						if (g_Player.Playing && g_Player.IsTVRecording)
						{
							g_Player.Stop();
						}
						int count=0;
						try
						{
							if (VMR9Util.g_vmr9!=null)
								VMR9Util.g_vmr9.Enable(false);

							while (GUIGraphicsContext.InVmr9Render && count++ < 10) System.Threading.Thread.Sleep(100);
							GUITVHome.IsTVOn=true;
							GUITVHome.ViewChannel(m_strCurrentChannel);
							if (Recorder.IsViewing())
							{
								GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
							}
						}
						finally
						{
							if (VMR9Util.g_vmr9!=null)
								VMR9Util.g_vmr9.Enable(true);
						}
						return;
						

					case 939: // switch mode
						OnSwitchMode();
						break;
					
					case 264: // record
						OnRecord();
						break;
				}
			}
		}
		void OnSwitchMode()
		{
			UnFocus();
			m_bSingleChannel=!m_bSingleChannel;							
			if (m_bSingleChannel)
			{
				m_cursorXBackup=m_iCursorX;
				m_cursorYBackup=m_iCursorY;
				m_channelOffsetBack=m_iChannelOffset;

				m_iProgramOffset=m_iCursorX=m_iCursorY=0;
			}
			else
			{
				//focus current channel
				m_iCursorX=0;
				m_iCursorY=m_cursorYBackup;
				m_iChannelOffset=m_channelOffsetBack;
			}
			Update(true);
			SetFocus();
		}
		void OnRecord()
		{
			if (m_currentProgram==null) return;
			if (m_strCurrentChannel.Length<=0) return;
			if (m_strCurrentTitle.Length<=0) return;
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Recording type
				if (m_strCurrentChannel.Length>0)
					dlg.AddLocalizedString( 938);// View this channel

				bool isrecording=false;
				foreach (TVRecording rec1 in m_recordings)
				{
					if (rec1.IsRecordingProgram(m_currentProgram,true))
					{
						isrecording=true;
						break;
					}
				}
				TVNotify notification=null;
				ArrayList notifies = new ArrayList();
				TVDatabase.GetNotifies(notifies,false);
				bool showNotify=true;
				foreach (TVNotify notify in notifies)
				{
					if (notify.Program.ID==m_currentProgram.ID)
					{
						showNotify=false;
						notification=notify;
						break;
					}
				}
				if (showNotify)
					dlg.AddLocalizedString(1014);//Notify me when program begins
				else
					dlg.AddLocalizedString(1015);//Don't notify me when program begins
				//610=None
				//611=Record once
				//612=Record everytime on this channel
				//613=Record everytime on every channel
				//614=Record every week at this time
				//615=Record every day at this time
				if (isrecording)
				{
					dlg.AddLocalizedString(610);
				}
				else
				{
					for (int i=611; i <= 615; ++i)
					{
						dlg.AddLocalizedString(i);
					}
					dlg.AddLocalizedString( 672);// 672=Record Mon-Fri
				}
				
				dlg.DoModal( GetID);
				if (dlg.SelectedLabel==-1) return;
				TVRecording rec=new TVRecording();
				rec.Title=m_strCurrentTitle;
				rec.Channel=m_strCurrentChannel;
				rec.Start=m_iCurrentStartTime;
				rec.End=m_iCurrentEndTime;
				switch (dlg.SelectedId)
				{
					case 1015: // dont notify me
						TVDatabase.DeleteNotify(notification);
						return;
					case 1014: // notify me
						TVNotify notify = new TVNotify();
						notify.Program=m_currentProgram;
						TVDatabase.AddNotify(notify);
						return;

					case 938: // view this channel:
						if (g_Player.Playing && g_Player.IsTVRecording)
						{
							g_Player.Stop();
						}
						try
						{
							if (VMR9Util.g_vmr9!=null)
								VMR9Util.g_vmr9.Enable(false);

							int count=0;
							while (GUIGraphicsContext.InVmr9Render && count++ < 10) System.Threading.Thread.Sleep(100);
							GUITVHome.IsTVOn=true;
							GUITVHome.ViewChannel(m_strCurrentChannel);
							if (Recorder.IsViewing())
							{
								GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
							}
						}
						finally
						{
							if (VMR9Util.g_vmr9!=null)
								VMR9Util.g_vmr9.Enable(true);
						}
						return;

					case 610://none
						foreach (TVRecording rec1 in m_recordings)
						{
							if (rec1.IsRecordingProgram(m_currentProgram,true))
							{
								if (rec1.RecType != TVRecording.RecordingType.Once)
								{
									//delete specific series
									rec1.CanceledSeries.Add(m_currentProgram.Start);
									TVDatabase.AddCanceledSerie(rec1,m_currentProgram.Start);
								}
								else
								{
									//cancel recording
									rec1.Canceled=Utils.datetolong(DateTime.Now);
									TVDatabase.UpdateRecording(rec1,TVDatabase.RecordingChange.Canceled);
								}
								Recorder.StopRecording(rec1);
							}
						}
						Update(false);
						SetFocus();
						return;
					case 611://once
						rec.RecType=TVRecording.RecordingType.Once;
						break;
					case 612://everytime, this channel
						rec.RecType=TVRecording.RecordingType.EveryTimeOnThisChannel;
						break;
					case 613://everytime, all channels
						rec.RecType=TVRecording.RecordingType.EveryTimeOnEveryChannel;
						break;
					case 614://weekly
						rec.RecType=TVRecording.RecordingType.Weekly;
						break;
					case 615://daily
						rec.RecType=TVRecording.RecordingType.Daily;
						break;
					case 672://Mo-Fi
						rec.RecType=TVRecording.RecordingType.WeekDays;
						break;
				}
				Recorder.AddRecording(ref rec);

				//check if this program is interrupted (for example by a news bulletin)
				//ifso ask the user if he wants to record the 2nd part also
				ArrayList programs=new ArrayList();
				DateTime dtStart=rec.EndTime.AddMinutes(1);
				DateTime dtEnd  =dtStart.AddHours(3);
				long iStart=Utils.datetolong(dtStart);
				long iEnd=Utils.datetolong(dtEnd);
				TVDatabase.GetProgramsPerChannel(rec.Channel,iStart,iEnd,ref programs);
				if (programs.Count>=2)
				{
					TVProgram next=programs[0] as TVProgram;
					TVProgram nextNext=programs[1] as TVProgram;
					if (nextNext.Title==rec.Title)
					{
						TimeSpan ts=next.EndTime-next.StartTime;
						if (ts.TotalMinutes<=40)
						{
							//
							GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
							dlgYesNo.SetHeading(1012);//This program will be interrupted by
							dlgYesNo.SetLine(1,next.Title);
							dlgYesNo.SetLine(2,1013);//Would you like to record the second part also?
							dlgYesNo.DoModal(GetID);
							if (dlgYesNo.IsConfirmed) 
							{
								rec.Start=nextNext.Start;
								rec.End=nextNext.End;
								rec.ID=-1;
								Recorder.AddRecording(ref rec);
							}
						}
					}
				}
				
				Update(false);
				SetFocus();
			}
		}
		void CheckRecordingConflicts()
		{
			/*
			ConflictManager manager = new ConflictManager();
			ArrayList conflicts = new ArrayList();
			manager.CheckRecordingsForConflicts(ref conflicts);
			if (conflicts.Count==0) return; // no conflicts 
			foreach (ConflictManager.RecordingConflict conflict in conflicts)
			{
				//present conflict on GUI somehow...				
				//Log.Write("Recording on channel {0} title:{1} conflicts with the following shows:",conflict.Recording.Channel,conflict.Recording.Title);
				//foreach (ConflictManager.CardAllocation alloc in conflict.CardAllocations)
				//{
					//Log.Write("  card:{0} channel{1} title:{2}",alloc.ID,alloc.Recording.Channel,alloc.Recording.Title);
				//}
			}*/
		}

		void OnPageUp()
		{
			m_dtTime=m_dtTime.AddDays(1.0);
			Update(false);
			SetFocus();
		}

		void OnPageDown()
		{
			m_dtTime=m_dtTime.AddDays(-1.0);
			Update(false);
			SetFocus();
		}

		long GetColorForGenre(string strGenre)
		{
			if (!m_bUseColors) return Color.White.ToArgb();
			ArrayList genres = new ArrayList();
			TVDatabase.GetGenres(ref genres);

			strGenre=strGenre.ToLower();
			for (int i=0; i < genres.Count; ++i)
			{
				if (String.Compare(strGenre,(string)genres[i],true)==0)
				{
					Color col=(Color)colors[i % colors.Count];
					return col.ToArgb();
				}
			}
			return Color.White.ToArgb();
		}

		public override int GetFocusControlId()
		{
			if (m_iCursorY>=0) return 1;
			if (GetControl((int)Controls.SPINCONTROL_DAY).Focus==true) return (int)Controls.SPINCONTROL_DAY;
			if (GetControl((int)Controls.SPINCONTROL_TIME_INTERVAL).Focus==true) return (int)Controls.SPINCONTROL_TIME_INTERVAL;
			return -1;
		}

    
		void OnKeyTimeout()
		{
			if (m_strInput.Length==0) return;
			TimeSpan ts=DateTime.Now-m_timeKeyPressed;
			if (ts.TotalMilliseconds>=1000)
			{
				// change channel
				int iChannel=Int32.Parse(m_strInput);
				ChangeChannelNr(iChannel);
				m_strInput=String.Empty;
			}
		}

		void OnKeyCode(char chKey)
		{
			if(chKey>='0'&& chKey <='9') //Make sure it's only for the remote
			{
				TimeSpan ts=DateTime.Now-m_timeKeyPressed;
				if (m_strInput.Length>=2||ts.TotalMilliseconds>=800)
				{
					m_strInput=String.Empty;
				}
				m_timeKeyPressed=DateTime.Now;
				if (chKey=='0' && m_strInput.Length==0) return;
				m_strInput+= chKey;
				if (m_strInput.Length==2)
				{
					// change channel
					int iChannel=Int32.Parse(m_strInput);
					ChangeChannelNr(iChannel);
  				
				}
			}
		}

		void ChangeChannelNr(int iChannelNr)
		{
			iChannelNr--;
			if (iChannelNr>=0 && iChannelNr < m_channels.Count)
			{
        UnFocus();
				m_iChannelOffset=0;
				m_iCursorY=0;
        
        // Last page adjust (To get a full page channel listing)
        if (iChannelNr > m_channels.Count-m_iChannels+1) 
        {
          m_iChannelOffset = m_channels.Count- m_iChannels;
          iChannelNr = iChannelNr - m_iChannelOffset;
        }

				while (iChannelNr >=m_iChannels) 
				{
					iChannelNr -=m_iChannels;
					m_iChannelOffset+=m_iChannels;
				}
				m_iCursorY=iChannelNr;
        
				Update(false);
				SetFocus();
			}
		}
		void GetChannels()
		{
			m_channels.Clear();
			foreach (TVChannel chan in GUITVHome.Navigator.CurrentGroup.tvChannels)
			{
				if (chan.VisibleInGuide)
				{
					m_channels.Add(chan);
				}
			}

			if (m_channels.Count==0)
			{
				TVChannel newChannel = new TVChannel();
				newChannel.Name=GUILocalizeStrings.Get(911);
				for (int i=0; i < 10;++i)
					m_channels.Add(newChannel);
			}
		}


		private void import_ShowProgress(MediaPortal.TV.Database.XMLTVImport.Stats stats)
		{
			GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			if (dlgProgress!=null)
			{
				int iChannels=stats.Channels;
				int iPrograms=stats.Programs;
				string strChannels=GUILocalizeStrings.Get(627)+ iChannels.ToString();
				string strPrograms=GUILocalizeStrings.Get(628)+ iPrograms.ToString();
				dlgProgress.SetLine(1,stats.Status);
				dlgProgress.SetLine(2,strChannels+ " "+strPrograms);
				dlgProgress.Progress();
			}
		}
		private void UpdateVerticalScrollbar()
		{
			int channel=m_iCursorY+m_iChannelOffset;
			while (channel>0 && channel >= m_channels.Count) channel -=m_channels.Count;
			float current=(float)(m_iCursorY+m_iChannelOffset);
			float total=(float)m_channels.Count;

			if (m_bSingleChannel)
			{
				current=(float)(m_iCursorY+m_iChannelOffset);
				total=(float)m_iTotalPrograms;
			}
			if (total==0) total=m_iChannels;

			float percentage=(current/total)*100.0f;
			if (percentage < 0) percentage=0;
			if (percentage >100) percentage=100;
			GUIverticalScrollbar scrollbar= GetControl((int)Controls.VERT_SCROLLBAR) as GUIverticalScrollbar;
			if (scrollbar!=null)
			{
				scrollbar.Percentage=percentage;
			}
		}

		private void UpdateHorizontalScrollbar()
		{
			GUIHorizontalScrollbar scrollbar = GetControl((int)Controls.HORZ_SCROLLBAR) as GUIHorizontalScrollbar;
			if (scrollbar!=null)
			{
				float percentage=(float)m_dtTime.Hour*60+m_dtTime.Minute+(float)m_iBlockTime*((float)m_dtTime.Hour/24.0f);
				percentage/=(24.0f*60.0f);
				percentage*=100.0f;
				if (percentage < 0) percentage=0;
				if (percentage >100) percentage=100;
				if (m_bSingleChannel) percentage=0;

				if ( (int)percentage != (int)scrollbar.Percentage )
				{
					scrollbar.Percentage=percentage;
				}
			}
		}
		private bool HasNotify(TVProgram program)
		{
			for (int i=0; i < notifies.Count;++i)
			{
				TVNotify notify=(TVNotify)notifies[i];
				if (notify.Program.ID==program.ID) return true;
			}
			return false;
		}

		private void TVDatabase_OnNotifiesChanged()
		{
			if (notifies!=null)
			{
				notifies.Clear();
				TVDatabase.GetNotifies(notifies,false);
				m_bNeedUpdate=true;
			}
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
					
			if ( !GUITVHome.IsTVWindow(newWindowId) )
			{
				if (Recorder.IsViewing() && ! (Recorder.IsTimeShifting()||Recorder.IsRecording()) )
				{
					if (GUIGraphicsContext.ShowBackground)
					{
						// stop timeshifting & viewing... 
	              
						Recorder.StopViewing();
					}
				}
			}
		}

		private void ConflictManager_OnConflictsUpdated()
		{
				m_bNeedUpdate=true;
		}

		private void TVDatabase_OnProgramsChanged()
		{
			m_bNeedUpdate=true;
		}
	}
}
