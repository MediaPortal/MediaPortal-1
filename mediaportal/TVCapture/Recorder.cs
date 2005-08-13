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
using System.Globalization;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Management; 
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// This class is a singleton which implements the
	/// -task scheduler to schedule, (start,stop) all tv recordings on time
	/// -a front end to other classes to control the tv capture cardsd
	/// </summary>
	public class Recorder
	{

		#region variables
		enum State
		{
			None,
			Initializing,
			Initialized,
			Deinitializing
		}
		
		static bool          m_bRecordingsChanged=false;  // flag indicating that recordings have been added/changed/removed
		static bool          m_bNotifiesChanged=false;  // flag indicating that notifies have been added/changed/removed
		static int           m_iPreRecordInterval =0;
		static int           m_iPostRecordInterval=0;
		
		static string        m_strTVChannel=String.Empty;

		static State         m_eState=State.None;
		static ArrayList     m_tvcards    = new ArrayList();
		static ArrayList     m_TVChannels = new ArrayList();
		static ArrayList     m_Recordings = new ArrayList();
		static ArrayList     m_Notifies   = new ArrayList();
		
		static DateTime      m_dtStart=DateTime.Now;
		static DateTime      m_dtProgresBar=DateTime.Now;
		static int           m_iCurrentCard=-1;
		static int           m_preRecordingWarningTime=2;
		static VMR9OSD			 m_osd = new VMR9OSD();
		static bool          m_useVMR9Zap=false;
		
		#endregion

		#region delegates and events
		public delegate void OnTvChannelChangeHandler(string tvChannelName);
		public delegate void OnTvRecordingChangedHandler();
		public delegate void OnTvRecordingHandler(string recordingFilename, TVRecording recording, TVProgram program);
		static public event OnTvChannelChangeHandler    OnTvChannelChanged=null;
		static public event OnTvRecordingChangedHandler OnTvRecordingChanged=null;
		static public event OnTvRecordingHandler			  OnTvRecordingStarted=null;
		static public event OnTvRecordingHandler				OnTvRecordingEnded=null;
		#endregion

		#region initialisation
		/// <summary>
		/// singleton. Dont allow any instance of this class so make the constructor private
		/// </summary>
		private Recorder()
		{
		}

		static Recorder()
		{
		}

		/// <summary>
		/// This method will Start the scheduler. It
		/// Loads the capture cards from capturecards.xml (made by the setup)
		/// Loads the recordings (programs scheduled to record) from the tvdatabase
		/// Loads the TVchannels from the tvdatabase
		/// </summary>
		static public void Start()
		{
			if (m_eState!=State.None) return;
			m_eState=State.Initializing;
			RecorderProperties.Init();
			m_bRecordingsChanged=false;
    
			Log.WriteFile(Log.LogType.Recorder,"Recorder: Loading capture cards from capturecards.xml");
			m_tvcards.Clear();
			try
			{
				using (Stream r = File.Open(@"capturecards.xml", FileMode.Open, FileAccess.Read))
				{
					SoapFormatter c = new SoapFormatter();
					m_tvcards = (ArrayList)c.Deserialize(r);
					r.Close();
				} 
			}
			catch(Exception)
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder: invalid capturecards.xml found! please delete it");
			}
			if (m_tvcards.Count==0) 
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder: no capture cards found. Use file->setup to setup tvcapture!");
			}
			for (int i=0; i < m_tvcards.Count;i++)
			{
				TVCaptureDevice card=(TVCaptureDevice)m_tvcards[i];
				card.ID=(i+1);
				card.OnTvRecordingEnded+=new MediaPortal.TV.Recording.TVCaptureDevice.OnTvRecordingHandler(card_OnTvRecordingEnded);
				card.OnTvRecordingStarted+=new MediaPortal.TV.Recording.TVCaptureDevice.OnTvRecordingHandler(card_OnTvRecordingStarted);
				Log.WriteFile(Log.LogType.Recorder,"Recorder:    card:{0} video device:{1} TV:{2}  record:{3} priority:{4}",
															card.ID,card.VideoDevice,card.UseForTV,card.UseForRecording,card.Priority);
			}

			m_iPreRecordInterval =0;
			m_iPostRecordInterval=0;
			//m_bAlwaysTimeshift=false;
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_iPreRecordInterval = xmlreader.GetValueAsInt("capture","prerecord", 5);
				m_iPostRecordInterval= xmlreader.GetValueAsInt("capture","postrecord", 5);
				//m_bAlwaysTimeshift   = xmlreader.GetValueAsBool("mytv","alwaystimeshift",false);
				TVChannelName  = xmlreader.GetValueAsString("mytv","channel",String.Empty);
				m_useVMR9Zap=xmlreader.GetValueAsBool("general","useVMR9ZapOSD",false);
				m_preRecordingWarningTime=xmlreader.GetValueAsInt("mytv","recordwarningtime",2);
			}

			for (int i=0; i < m_tvcards.Count;++i)
			{
				try
				{
					TVCaptureDevice dev = (TVCaptureDevice)m_tvcards[i];
					string dir=String.Format(@"{0}\card{1}",dev.RecordingPath,i+1);
					System.IO.Directory.CreateDirectory(dir);
					DiskManagement.DeleteOldTimeShiftFiles(dir);
				}
				catch(Exception){}
			}

			DiskManagement.ImportDvrMsFiles();

			m_TVChannels.Clear();
			TVDatabase.GetChannels(ref m_TVChannels);

			m_Recordings.Clear();
			m_Notifies.Clear();
			TVDatabase.GetRecordings(ref m_Recordings);
			TVDatabase.GetNotifies(m_Notifies,true);

			TVDatabase.OnRecordingsChanged += new TVDatabase.OnRecordingChangedHandler(Recorder.OnRecordingsChanged);
			TVDatabase.OnNotifiesChanged+=new MediaPortal.TV.Database.TVDatabase.OnChangedHandler(Recorder.OnNotifiesChanged);
      
			GUIWindowManager.Receivers += new SendMessageHandler(Recorder.OnMessage);
			m_eState=State.Initialized;

			m_osd.Mute=false;
			GUIWindow win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
			if(win!=null)
				win.SetObject(m_osd);
			win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
			if(win!=null)
				win.SetObject(m_osd);
		}//static public void Start()

		/// <summary>
		/// Stops the scheduler. It will cleanup all resources allocated and free
		/// the capture cards
		/// </summary>
		static public void Stop()
		{
			if (m_eState != State.Initialized) return;
			m_eState=State.Deinitializing;
			TVDatabase.OnRecordingsChanged -= new TVDatabase.OnRecordingChangedHandler(Recorder.OnRecordingsChanged);
			//TODO
			GUIWindowManager.Receivers -= new SendMessageHandler(Recorder.OnMessage);

			foreach (TVCaptureDevice dev in m_tvcards)
			{
				dev.Stop();
			}
			RecorderProperties.Clean();
			m_bRecordingsChanged=false;
			m_eState=State.None;
		}//static public void Stop()

		#endregion

		#region recording
		/// <summary>
		/// Checks if a recording should be started and ifso starts the recording
		/// This function gets called on a regular basis by the scheduler. It will
		/// look if any of the recordings needs to be started. Ifso it will
		/// find a free tvcapture card and start the recording
		/// </summary>
		static void HandleRecordings()
		{ 
			if (m_eState!= State.Initialized) return;
			DateTime dtCurrentTime=DateTime.Now;
			// no TV cards? then we cannot record anything, so just return
			if (m_tvcards.Count==0)  return;
			if (GUIWindowManager.IsRouted) return;

			// If the recording schedules have been changed since last time
			if (m_bRecordingsChanged)
			{
				// then get (refresh) all recordings from the database
				ArrayList oldRecs=(ArrayList)m_Recordings.Clone();
				m_Recordings.Clear();
				m_TVChannels.Clear();
				TVDatabase.GetRecordings(ref m_Recordings);
				TVDatabase.GetChannels(ref m_TVChannels);
				m_bRecordingsChanged=false;
				foreach (TVRecording recording in m_Recordings)
				{
					foreach (TVRecording oldrec in oldRecs)
					{
						if (oldrec.ID==recording.ID)
						{
							recording.IsAnnouncementSend=oldrec.IsAnnouncementSend;
							break;
						}
					}

					for (int i=0; i < m_tvcards.Count;++i)
					{
						TVCaptureDevice dev=(TVCaptureDevice )m_tvcards[i];
						if (dev.IsRecording)
						{
							if (dev.CurrentTVRecording.ID==recording.ID)
							{
								dev.CurrentTVRecording=recording;
							}//if (dev.CurrentTVRecording.ID==recording.ID)
						}//if (dev.IsRecording)
					}//for (int i=0; i < m_tvcards.Count;++i)
				}//foreach (TVRecording recording in m_Recordings)
				oldRecs=null;
			}//if (m_bRecordingsChanged)

			int card;
			for (int i=0; i < m_TVChannels.Count;++i)
			{
				TVChannel chan =(TVChannel)m_TVChannels[i];
				// get all programs running for this TV channel
				// between  (now-4 hours) - (now+iPostRecordInterval+3 hours)
				DateTime dtStart=dtCurrentTime.AddHours(-4);
				DateTime dtEnd=dtCurrentTime.AddMinutes(m_iPostRecordInterval+3*60);
				long iStartTime=Utils.datetolong(dtStart);
				long iEndTime=Utils.datetolong(dtEnd);
            
				// for each TV recording scheduled
				for (int j=0; j < m_Recordings.Count;++j)
				{
					TVRecording rec =(TVRecording)m_Recordings[j];
					if (rec.Canceled>0) continue;
					if (rec.IsDone()) continue;
					if (rec.RecType==TVRecording.RecordingType.EveryTimeOnEveryChannel || chan.Name==rec.Channel)
					{
						if (!IsRecordingSchedule(rec, out card)) 
						{
							// check which program is running 
							TVProgram prog=chan.GetProgramAt(dtCurrentTime.AddMinutes(1+m_iPreRecordInterval) );

							// if the recording should record the tv program
							if ( rec.IsRecordingProgramAtTime(dtCurrentTime,prog,m_iPreRecordInterval, m_iPostRecordInterval) )
							{
								// yes, then record it
								if (Record(dtCurrentTime,rec,prog, m_iPreRecordInterval, m_iPostRecordInterval))
								{
									break;
								}
							}
							else
							{
								if (!rec.IsAnnouncementSend)
								{
									DateTime dtTime=DateTime.Now.AddMinutes(m_preRecordingWarningTime);
									TVProgram prog2Min=chan.GetProgramAt(dtTime.AddMinutes(1+m_iPreRecordInterval) );

									// if the recording should record the tv program
									if ( rec.IsRecordingProgramAtTime(dtTime,prog2Min,m_iPreRecordInterval, m_iPostRecordInterval) )
									{
										Log.WriteFile(Log.LogType.Recorder,"Recorder: Send announcement for recording:{0}",rec.ToString());
										rec.IsAnnouncementSend=true;
										GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING,0,0,0,0,0,null);
										msg.Object=rec;
										msg.Object2=prog2Min;
										GUIGraphicsContext.SendMessage( msg );
									}
								}
							}
						}
					}
				}
			}
   

			for (int j=0; j < m_Recordings.Count;++j)
			{
				TVRecording rec =(TVRecording)m_Recordings[j];
				if (rec.Canceled>0) continue;
				if (rec.IsDone()) continue;

				// 1st check if the recording itself should b recorded
				if ( rec.IsRecordingProgramAtTime(DateTime.Now,null,m_iPreRecordInterval, m_iPostRecordInterval) )
				{
					if (!IsRecordingSchedule(rec, out card)) 
					{
						// yes, then record it
						if ( Record(dtCurrentTime,rec,null,m_iPreRecordInterval, m_iPostRecordInterval))
						{
							// recording it
						}
					}
				}
				else
				{
					if (!rec.IsAnnouncementSend)
					{
						DateTime dtTime=DateTime.Now.AddMinutes(m_preRecordingWarningTime);
						// if the recording should record the tv program
						if ( rec.IsRecordingProgramAtTime(dtTime,null,m_iPreRecordInterval, m_iPostRecordInterval) )
						{
							rec.IsAnnouncementSend=true;
							Log.WriteFile(Log.LogType.Recorder,"Recorder: Send announcement for recording:{0}",rec.ToString());
							GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING,0,0,0,0,0,null);
							msg.Object=rec;
							msg.Object2=null;
							GUIGraphicsContext.SendMessage( msg);
						}
					}
				}
			}
		}//static void HandleRecordings()


		static public bool NeedChannelSwitchForRecording(TVRecording rec)
		{
			if (IsViewing() && TVChannelName==rec.Channel) return false;
			//check if there's another card which is free
			for (int i=0; i < m_tvcards.Count;++i)
			{
				TVCaptureDevice dev = m_tvcards[i] as TVCaptureDevice;
				if (!dev.IsRecording  && !dev.IsTimeShifting && !dev.View)
				{
					if (TVDatabase.CanCardViewTVChannel(rec.Channel, dev.ID) || m_tvcards.Count==1 )
						return false;
				}
			}
			

			return true;
		}

		/// <summary>
		/// Starts recording the specified tv channel immediately using a reference recording
		/// When called this method starts an erference  recording on the channel specified
		/// It will record the next 2 hours.
		/// </summary>
		/// <param name="strChannel">TVchannel to record</param>
		static public void RecordNow(string strChannel, bool manualStop)
		{
			if (strChannel==null) return;
			if (strChannel==String.Empty) return;
			if (m_eState!= State.Initialized) return;
      
			// create a new recording which records the next 2 hours...
			TVRecording tmpRec = new TVRecording();
			
			tmpRec.Channel=strChannel;
			tmpRec.RecType=TVRecording.RecordingType.Once;

			TVProgram program=null;
			for (int i=0; i < m_TVChannels.Count;++i)
			{
				TVChannel chan =(TVChannel)m_TVChannels[i];
				if (chan.Name.Equals(strChannel))
				{
					program=chan.CurrentProgram;
					break;
				}
			}

			if (program!=null && !manualStop)
			{
				//record current playing program
				tmpRec.Start=program.Start;
				tmpRec.End=program.End;
				tmpRec.Title=program.Title;
				tmpRec.IsContentRecording=false;//make a reference recording! (record from timeshift buffer)
				Log.WriteFile(Log.LogType.Recorder,"Recorder:record now:{0} program:{1}",strChannel,program.Title);
			}
			else
			{
				//no tvguide data, just record the next 2 hours
				Log.WriteFile(Log.LogType.Recorder,"Recorder:record now:{0} for next 4 hours",strChannel);
				tmpRec.Start=Utils.datetolong(DateTime.Now);
				tmpRec.End=Utils.datetolong(DateTime.Now.AddMinutes(4*60) );
				tmpRec.Title=GUILocalizeStrings.Get(413);
				if (program!=null)
					tmpRec.Title=program.Title;
				tmpRec.IsContentRecording=true;//make a content recording! (record from now)
			}

			Log.WriteFile(Log.LogType.Recorder,"Recorder:   start: {0} {1}",tmpRec.StartTime.ToShortDateString(), tmpRec.StartTime.ToShortTimeString());
			Log.WriteFile(Log.LogType.Recorder,"Recorder:   end  : {0} {1}",tmpRec.EndTime.ToShortDateString(), tmpRec.EndTime.ToShortTimeString());

			AddRecording(ref tmpRec);
			m_dtStart=new DateTime(1971,6,11,0,0,0,0);
		}//static public void RecordNow(string strChannel)
		
		static public int AddRecording(ref TVRecording rec)
		{
			ArrayList recs = new ArrayList();
			TVDatabase.GetRecordings(ref recs);
			recs.Sort( new TVRecording.PriorityComparer(true));
			int prio=Int32.MaxValue;
			foreach (TVRecording recording in recs)
			{
				if (prio!=recording.Priority)
				{
					recording.Priority=prio;
					TVDatabase.SetRecordingPriority(recording);
				}
				prio--;
			}
			rec.Priority=prio;
			return TVDatabase.AddRecording(ref rec);
		}

		/// <summary>
		/// Find a capture card we can use to start a new recording
		/// </summary>
		/// <param name="recordingChannel">Channel we need to record</param>
		/// <param name="terminatePostRecording">
		/// false: use algoritm 1 ( see below)
		/// true: use algoritm 2 ( see below)
		/// </param>
		/// <returns>
		/// -1 : no card found
		/// else card which can be used for recording</returns>
		/// <remarks>
		/// MP will first use the following algorithm to find a card to use for the recording:
		///		- card must be able to record the selected channel
		///		- card must be free (or viewing the channel we need to record)
		///		- of all cards found it will use the one with the highest priority
		///		
		///	if no card is found then MP will try to use the following algorithm:
		///		- card must be able to record the selected channel
		///		- card must be free  (or viewing the channel we need to record) or postrecording on any channel !!!
		///		- of all cards found it will use the one with the highest priority
		///	
		///	Note. If the determined card is in use and the user is currently watching different channel on it
		///	then the one we need to record then MP will look if there are other cards available with maybe have a
		///	lower priority. reason for this is that we want to prevent the situation where the user
		///	is watching channel A, and then when the recording starts on channel B the user suddenly 
		///	sees channel B
		/// </remarks>
		static private int FindFreeCardForRecording(string recordingChannel,bool stopRecordingsWithLowerPriority, int recordingPrio)
		{
			if (m_iCurrentCard>=0 && m_iCurrentCard< m_tvcards.Count)
			{
				TVCaptureDevice dev=(TVCaptureDevice )m_tvcards[m_iCurrentCard];
				if (dev.View && !dev.IsRecording)
				{
					if (dev.TVChannel==recordingChannel)
					{
						if (dev.UseForRecording) return m_iCurrentCard;
					}
				}
			}
			int cardNo=0;
			int highestPrio=-1;
			int highestCard=-1;
			for (int loop=0; loop <= 1; loop++)
			{
				highestPrio=-1;
				highestCard=-1;
				cardNo=0;
				foreach (TVCaptureDevice dev in m_tvcards)
				{
					//if we may use the  card for recording tv?
					if (dev.UseForRecording)
					{
						// and is it not recording already?
						// or recording a show which has lower priority then the one we need to record?
						if (!dev.IsRecording || (dev.IsRecording && stopRecordingsWithLowerPriority && dev.CurrentTVRecording.Priority<recordingPrio) )
						{
							//and can it receive the channel we want to record?
							if (TVDatabase.CanCardViewTVChannel(recordingChannel, dev.ID) || m_tvcards.Count==1 )
							{
								// does this card has the highest priority?
								if (dev.Priority>highestPrio)
								{
									//yes then we use this card
									//but do we want to use it?
									//if the user is using this card to watch tv on another channel
									//then we prefer to use another tuner for the recording
									bool preferCard=false;
									if (m_iCurrentCard==cardNo)
									{
										//user is watching tv on this tuner
										if (loop>=1) 
										{
											//first loop didnt find any other free card,
											//so no other choice then to use this one.
											preferCard=true; 
										}
										else
										{
											//is user watching same channel as we wanna record?
											if (dev.IsTimeShifting && dev.TVChannel==recordingChannel) 
											{
												//yes, then he wont notice anything, so we can use the card
												preferCard=true;
											}
										}
									}
									else
									{
										//user is not using this tuner, so we can use this card
										preferCard=true;
									}

									if (preferCard)
									{
										highestPrio=dev.Priority;
										highestCard=cardNo;
									}
								}//if (dev.Priority>highestPrio)

								//if this card has the same priority and is already watching this channel
								//then we use this card
								if (dev.Priority==highestPrio)
								{
									if ( (dev.IsTimeShifting||dev.View==true) && dev.TVChannel==recordingChannel)
									{
										highestPrio=dev.Priority;
										highestCard=cardNo;
									}
								}
							}//if (TVDatabase.CanCardViewTVChannel(rec.Channel, dev.ID) || m_tvcards.Count==1 )
						}//if (!dev.IsRecording)
					}//if (dev.UseForRecording)
					cardNo++;
				}//foreach (TVCaptureDevice dev in m_tvcards)
				if (highestCard>=0)
				{
					return highestCard;
				}
			}//for (int loop=0; loop <= 1; loop++)
			return -1;
		}

		/// <summary>
		/// Start recording a new program
		/// </summary>
		/// <param name="currentTime"></param>
		/// <param name="rec">TVRecording to record <seealso cref="MediaPortal.TV.Database.TVRecording"/></param>
		/// <param name="currentProgram">TVprogram to record <seealso cref="MediaPortal.TV.Database.TVProgram"/> (can be null)</param>
		/// <param name="iPreRecordInterval">Pre record interval in minutes</param>
		/// <param name="iPostRecordInterval">Post record interval in minutes</param>
		/// <returns>true if recording has been started</returns>
		static bool Record(DateTime currentTime,TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)
		{
			if (rec==null) return false;
			if (m_eState!= State.Initialized) return false;
			if (iPreRecordInterval<0) iPreRecordInterval=0;
			if (iPostRecordInterval<0) iPostRecordInterval=0;

			// Check if we're already recording this...
			foreach (TVCaptureDevice dev in m_tvcards)
			{
				if (dev.IsRecording)
				{
					if (dev.CurrentTVRecording.ID==rec.ID) return false;
				}
			}

			// not recording this yet
			Log.WriteFile(Log.LogType.Recorder,"Recorder: time to record '{0}' on channel:{1} from {2}-{3} id:{4} priority:{5} quality:{6} {7}",rec.Title,rec.Channel, rec.StartTime.ToLongTimeString(), rec.EndTime.ToLongTimeString(),rec.ID, rec.Priority,rec.Quality.ToString(),rec.RecType.ToString());
			Log.WriteFile(Log.LogType.Recorder,"Recorder:  find free capture card");
			LogTvStatistics();

			// find free card we can use for recording
			int cardNo=FindFreeCardForRecording(rec.Channel,false,rec.Priority);
			if (cardNo<0)
			{
				// no card found. 
				Log.WriteFile(Log.LogType.Recorder,"Recorder:  No card found, check if a card is recording a show which has a lower priority then priority:{0}", rec.Priority);
				cardNo=FindFreeCardForRecording(rec.Channel,true,rec.Priority);
				if (cardNo<0)
				{
					Log.WriteFile(Log.LogType.Recorder,"Recorder:  no recordings have a lower priority then priority:{0}", rec.Priority);
					return false;
				}
			}

			if (cardNo<0) 
			{
				GUIDialogMenuBottomRight pDlgOK	= GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT) as GUIDialogMenuBottomRight;
				pDlgOK.Reset();
				pDlgOK.SetHeading(879);//Recording Conflict
				pDlgOK.SetHeadingRow2( GUILocalizeStrings.Get(880) + " " + rec.Channel);
				pDlgOK.SetHeadingRow3(881);
				int	cardWithLowestPriority=-1;
				int	lowestPriority=TVRecording.HighestPriority;
				int count=0;
				for (int i=0; i < m_tvcards.Count;i++)
				{
					TVCaptureDevice dev = (TVCaptureDevice)m_tvcards[i];
					if (!dev.IsRecording) continue;
					if (dev.CurrentTVRecording.Channel == rec.Channel)
					{
						if (dev.IsPostRecording) return false;
					}

					GUIListItem item = new GUIListItem();
					item.Label= dev.CurrentTVRecording.Title;
					string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,dev.CurrentTVRecording.Channel);                   
					if (System.IO.File.Exists(strLogo))
					{										
						item.IconImage = strLogo;							
					}
					pDlgOK.Add(item);
					int prio=dev.CurrentTVRecording.Priority;
					if (prio < lowestPriority)
					{
						cardWithLowestPriority=i;
						lowestPriority=prio;
					}
					count++;
				}
				
				if (count>0)
				{
					pDlgOK.TimeOut=60;
					pDlgOK.DoModal( GUIWindowManager.ActiveWindow);
					if (pDlgOK.TimedOut)
					{
						cardNo=cardWithLowestPriority;
						TVCaptureDevice dev = (TVCaptureDevice)m_tvcards[cardNo];
						Log.WriteFile(Log.LogType.Recorder,"Recorder: Canceled recording:{0} priority:{1} on card:{2}",
													 dev.CurrentTVRecording.ToString(),
													 dev.CurrentTVRecording.Priority,
													 dev.ID);
						StopRecording(dev.CurrentTVRecording);
					}
					else
					{
						int selectedIndex=pDlgOK.SelectedLabel;
						if (selectedIndex>=0)
						{
							for (int i=0; i < m_tvcards.Count;++i)
							{
								TVCaptureDevice dev = (TVCaptureDevice)m_tvcards[i];
								if (dev.IsRecording)
								{
									if (count==selectedIndex)
									{
										cardNo=i;
										Log.WriteFile(Log.LogType.Recorder,"Recorder: User canceled recording:{0} priority:{1} on card:{2}",
											dev.CurrentTVRecording.ToString(),
											dev.CurrentTVRecording.Priority,
											dev.ID);
										StopRecording(dev.CurrentTVRecording);
										break;
									}
									count++;
								}
							}
						}
					}
				}
				if (cardNo<0)
				{					Log.WriteFile(Log.LogType.Recorder,"Recorder:  no card available for recording");
					return false;//no card free
				}
			}
			TVCaptureDevice card =(TVCaptureDevice)m_tvcards[cardNo];
			Log.WriteFile(Log.LogType.Recorder,"Recorder:  using card:{0} prio:{1}", card.ID,card.Priority);
			if (card.IsRecording)
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:  card:{0} was recording. Now use it for recording new program", card.ID);
				Log.WriteFile(Log.LogType.Recorder,"Recorder: Stop recording card:{0} channel:{1}",card.ID, card.TVChannel);
				if (card.CurrentTVRecording.RecType==TVRecording.RecordingType.Once)
				{
					card.CurrentTVRecording.Canceled=Utils.datetolong(DateTime.Now);
				}
				else
				{
					long datetime=Utils.datetolong(DateTime.Now);
					TVProgram prog=card.CurrentProgramRecording;
					if (prog!=null) datetime=Utils.datetolong(prog.StartTime);
					card.CurrentTVRecording.CanceledSeries.Add(datetime);
				}
				TVDatabase.UpdateRecording(card.CurrentTVRecording,TVDatabase.RecordingChange.Canceled);
				card.StopRecording();
			}
			
			TuneExternalChannel(rec.Channel,false);
			card.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
			
			if (m_iCurrentCard==cardNo) 
			{
				TVChannelName=rec.Channel;
				StartViewing(rec.Channel,true,true);
			}
			m_dtStart=new DateTime(1971,6,11,0,0,0,0);
			return true;
		}//static bool Record(DateTime currentTime,TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)


		static public void StopRecording(TVRecording rec)
		{
			if (m_eState!= State.Initialized) return ;
			if (rec==null) return;
			for (int card=0; card < m_tvcards.Count;++card)
			{
				TVCaptureDevice dev =(TVCaptureDevice )m_tvcards[card];
				if (dev.IsRecording)
				{
					if (dev.CurrentTVRecording.ID==rec.ID) 
					{
						if (rec.RecType==TVRecording.RecordingType.Once)
						{
							Log.WriteFile(Log.LogType.Recorder,"Recorder: Stop recording card:{0} channel:{1}",dev.ID, dev.TVChannel);
							rec.Canceled=Utils.datetolong(DateTime.Now);
						}
						else
						{
							Log.WriteFile(Log.LogType.Recorder,"Recorder: Stop serie of recording card:{0} channel:{1}",dev.ID, dev.TVChannel);
							long datetime=Utils.datetolong(DateTime.Now);
							TVProgram prog=dev.CurrentProgramRecording;
							if (prog!=null) datetime=Utils.datetolong(prog.StartTime);
							rec.CanceledSeries.Add(datetime);
							rec.Canceled=0;
						}
						TVDatabase.UpdateRecording(rec,TVDatabase.RecordingChange.Canceled);
						dev.StopRecording();

						//if we're not viewing this card
						if (m_iCurrentCard!=card)
						{
							//then stop card
							dev.Stop();
						}
					}
				}
			}
			m_dtStart=new DateTime(1971,6,11,0,0,0,0);
		}
		/// <summary>
		/// Stops all recording on the current channel. 
		/// </summary>
		/// <remarks>
		/// Only stops recording. timeshifting wont be stopped so user can continue to watch the channel
		/// </remarks>
		static public void StopRecording()
		{
			if (m_eState!= State.Initialized) return ;
			if (m_iCurrentCard <0 || m_iCurrentCard >=m_tvcards.Count) return ;
			TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];
      
			if (dev.IsRecording) 
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder: Stop recording card:{0} channel:{1}",dev.ID, dev.TVChannel);
				int ID=dev.CurrentTVRecording.ID;

				if (dev.CurrentTVRecording.RecType==TVRecording.RecordingType.Once)
				{
					dev.CurrentTVRecording.Canceled=Utils.datetolong(DateTime.Now);
				}
				else
				{
					long datetime=Utils.datetolong(DateTime.Now);
					TVProgram prog=dev.CurrentProgramRecording;
					if (prog!=null) datetime=Utils.datetolong(prog.StartTime);
					dev.CurrentTVRecording.CanceledSeries.Add(datetime);
				}
				TVDatabase.UpdateRecording(dev.CurrentTVRecording,TVDatabase.RecordingChange.Canceled);
				dev.StopRecording();
			}
			m_dtStart=new DateTime(1971,6,11,0,0,0,0);
		}//static public void StopRecording()

		#endregion

		#region Properties
		/// <summary>
		/// Property which returns if any card is recording
		/// </summary>
		static public bool IsAnyCardRecording()
		{
			foreach (TVCaptureDevice dev in m_tvcards)
			{
				if (dev.IsRecording) return true;
			}
			return false;
		}//static public bool IsAnyCardRecording()

		/// <summary>
		/// Property which returns if any card is recording the specified channel
		/// </summary>
		static public bool IsRecordingChannel(string channel)
		{
			if (m_eState!= State.Initialized) return false;
			
			foreach (TVCaptureDevice dev in m_tvcards)
			{
				if (dev.IsRecording && dev.CurrentTVRecording.Channel==channel) return true;
			}
			return false;
		}//static public bool IsRecordingChannel(string channel)


		/// <summary>
		/// Property which returns if current card is recording
		/// </summary>
		static public bool IsRecording()
		{
			if (m_eState!= State.Initialized) return false;
			if (m_iCurrentCard<0 || m_iCurrentCard >= m_tvcards.Count) return false;
			
			TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
			return dev.IsRecording;
		}//static public bool IsRecording()

		/// <summary>
		/// Property which returns if current channel has teletext or not
		/// </summary>
		static public bool HasTeletext()
		{
			if (m_eState!= State.Initialized) return false;
			if (m_iCurrentCard<0 || m_iCurrentCard >= m_tvcards.Count) return false;
			TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
			return dev.HasTeletext;
		}

		/// <summary>
		/// Property which returns if current card supports timeshifting
		/// </summary>
		static public bool DoesSupportTimeshifting()
		{
			if (m_eState!= State.Initialized) return false;
			if (m_iCurrentCard<0 || m_iCurrentCard >= m_tvcards.Count) return false;
			
			TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
			return dev.SupportsTimeShifting;
		}//static public bool DoesSupportTimeshifting()

		static public string GetFriendlyNameForCard(int card)
		{
			if (m_eState!= State.Initialized) return String.Empty;
			if (card <0 || card >=m_tvcards.Count) return String.Empty;
			TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[card];
			return dev.FriendlyName;
		}//static public string GetFriendlyNameForCard(int card)

		/// <summary>
		/// Returns the Channel name of the channel we're currently watching
		/// </summary>
		/// <returns>
		/// Returns the Channel name of the channel we're currently watching
		/// </returns>
		static public string GetTVChannelName()
		{
			if (m_eState!= State.Initialized) return String.Empty;
			if (m_iCurrentCard <0 || m_iCurrentCard >=m_tvcards.Count) return String.Empty;
			
			TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
			return dev.TVChannel;
		}//static public string GetTVChannelName()
    
		/// <summary>
		/// Returns the TV Recording we're currently recording
		/// </summary>
		/// <returns>
		/// </returns>
		static public TVRecording GetTVRecording()
		{
			if (m_eState!= State.Initialized) return null;
			if (m_iCurrentCard <0 || m_iCurrentCard >=m_tvcards.Count) return null;
			
			TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
			if (dev.IsRecording) return dev.CurrentTVRecording;
			return null;
		}//static public TVRecording GetTVRecording()

		/// <summary>
		/// Checks if a tvcapture card is recording the TVRecording specified
		/// </summary>
		/// <param name="rec">TVRecording <seealso cref="MediaPortal.TV.Database.TVRecording"/></param>
		/// <returns>true if a card is recording the specified TVRecording, else false</returns>
		static public bool IsRecordingSchedule(TVRecording rec, out int card)
		{
			card=-1;
			if (rec==null) return false;
			if (m_eState!= State.Initialized) return false;
			for (int i=0; i < m_tvcards.Count;++i)
			{
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
				if (dev.IsRecording && dev.CurrentTVRecording!=null&&dev.CurrentTVRecording.ID==rec.ID) 
				{
					if (rec.Series==false)
					{
						card=i;
						return true;
					}

					//check start/end times
					if ( rec.StartTime <= DateTime.Now && rec.EndTime >= rec.StartTime)
					{
						card=i;
						return true;
					}
				}
			}
			return false;
		}//static public bool IsRecordingSchedule(TVRecording rec, out int card)
    
		/// <summary>
		/// Property which returns the current program being recorded. If no programs are being recorded at the moment
		/// it will return null;
		/// </summary>
		/// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
		static public TVProgram ProgramRecording
		{
			get
			{
				if (m_eState!= State.Initialized) return null;
				if (m_iCurrentCard< 0 || m_iCurrentCard>=m_tvcards.Count) return null;
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];
				if (dev.IsRecording) return dev.CurrentProgramRecording;
				return null;
			}
		}//static public TVProgram ProgramRecording

		/// <summary>
		/// Property which returns the current TVRecording being recorded. 
		/// If no recordings are being recorded at the moment
		/// it will return null;
		/// </summary>
		/// <seealso cref="MediaPortal.TV.Database.TVRecording"/>
		static public TVRecording CurrentTVRecording
		{
			get
			{
				if (m_eState!= State.Initialized) return null;
				if (m_iCurrentCard< 0 || m_iCurrentCard>=m_tvcards.Count) return null;
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];
				if (dev.IsRecording) return dev.CurrentTVRecording;
				return null;
			}
		}//static public TVRecording CurrentTVRecording
    

		/// <summary>
		/// Returns true if we're timeshifting
		/// </summary>
		/// <returns></returns>
		static public bool IsTimeShifting()
		{
			if (m_eState!= State.Initialized) return false;
			if (m_iCurrentCard <0 || m_iCurrentCard >=m_tvcards.Count) return false;
			TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];
			if (dev.IsTimeShifting) return true;
			return false;
		}//static public bool IsTimeShifting()

		/// <summary>
		/// Returns true if we're watching live tv
		/// </summary>
		/// <returns></returns>
		static public bool IsViewing()
		{
			if (m_eState!= State.Initialized) return false;
			if (m_iCurrentCard <0 || m_iCurrentCard >=m_tvcards.Count) return false;
			TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];
			if (dev.View) return true;
			if (dev.IsTimeShifting)
			{
				if (g_Player.Playing && g_Player.CurrentFile == GetTimeShiftFileName(m_iCurrentCard))
					return true;
			}
			return false;
		}//static public bool IsViewing()

		static public bool IsCardViewing(int cardId)
		{
			if (m_eState!= State.Initialized) return false;
			if (m_iCurrentCard <0 || m_iCurrentCard >=m_tvcards.Count) return false;
			TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];
			if (dev.ID!=cardId) return false;
			if (dev.View) return true;
			if (dev.IsTimeShifting)
			{
				if (g_Player.Playing && g_Player.CurrentFile == GetTimeShiftFileName(m_iCurrentCard))
					return true;
			}
			return false;
		}//static public bool IsViewing()
 
		/// <summary>
		/// Property which get TV Viewing mode.
		/// if TV Viewing  mode is turned on then live tv will be shown
		/// </summary>
		static public bool View
		{
			get 
			{
				if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording) return true;
				for (int i=0; i < m_tvcards.Count;++i)
				{
					TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
					if (dev.View) return true;
				}
				return false;
			}
		}//static public bool View

		/// <summary>
		/// property which returns the date&time the recording was started
		/// </summary>
		static public DateTime TimeRecordingStarted
		{
			get 
			{ 
				if (m_iCurrentCard< 0 || m_iCurrentCard>=m_tvcards.Count) return DateTime.Now;
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];
				if (dev.IsRecording)
				{
					return dev.TimeRecordingStarted;
				}
				return DateTime.Now;
			}
		}

		/// <summary>
		/// property which returns the date&time that timeshifting  was started
		/// </summary>
		static public DateTime TimeTimeshiftingStarted
		{
			get 
			{ 
				if (m_iCurrentCard< 0 || m_iCurrentCard>=m_tvcards.Count) return DateTime.Now;
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];
				if (!dev.IsRecording && dev.IsTimeShifting)
				{
					return dev.TimeShiftingStarted;
				}
				return DateTime.Now;
			}
		}
    
		/// <summary>
		/// Returns the number of tv cards configured
		/// </summary>
		static public int Count
		{
			get { return m_tvcards.Count;}
		}

		static public TVCaptureDevice Get(int index)
		{
			if (index < 0 || index >= m_tvcards.Count) return null;
			return m_tvcards[index] as TVCaptureDevice;
		}
    
		/// <summary>
		/// returns the name of the current tv channel we're watching
		/// </summary>
		static public string TVChannelName
		{
			get { return m_strTVChannel;}
			set { 
				if (value!=m_strTVChannel)
				{
					m_strTVChannel=value;
					if (OnTvChannelChanged!=null)
						OnTvChannelChanged(m_strTVChannel);
					//SetZapOSDData(m_strTVChannel);
				}
			}
		}

		// this sets the channel to render the osd
		static void SetZapOSDData(string channelName)
		{
			if (m_eState!= State.Initialized) return ;
			if (m_iCurrentCard <0 || m_iCurrentCard >=m_tvcards.Count) return ;
			TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];

			TVChannel channel=null;
			foreach (TVChannel chan in m_TVChannels)
			{
				if (chan.Name==channelName)
				{
					channel=chan;
					break;
				}
			}
			if (channel==null) return;

			if(GUIWindowManager.ActiveWindow!=(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
				return;
			if(m_osd!=null && channel!=null && m_useVMR9Zap==true)
			{
					int level=dev.SignalStrength;
					int quality=dev.SignalQuality;
					m_osd.RenderZapOSD(channel,quality,level);
			}
		}
		static public int SignalStrength
		{
			get 
			{
				if (m_eState!= State.Initialized) return 0;
				if (m_iCurrentCard<0 || m_iCurrentCard >= m_tvcards.Count) return 0;
			
				TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
				return dev.SignalStrength;
			}
		}
		static public int SignalQuality
		{
			get 
			{
				if (m_eState!= State.Initialized) return 0;
				if (m_iCurrentCard<0 || m_iCurrentCard >= m_tvcards.Count) return 0;
			
				TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
				return dev.SignalQuality;
			}
		}

		static public string GetRecordingFileName(TVRecording rec)
		{
			int card;
			if (!IsRecordingSchedule(rec, out card) ) return String.Empty;
			TVCaptureDevice dev = m_tvcards[card] as TVCaptureDevice;
			
			return dev.RecordingFileName;
		}

		/// <summary>
		/// Property which returns the timeshifting file for the current channel
		/// </summary>
		static public string GetTimeShiftFileName()
		{
			if (m_iCurrentCard<0 || m_iCurrentCard>=m_tvcards.Count) return String.Empty;
			TVCaptureDevice dev=(TVCaptureDevice) m_tvcards[m_iCurrentCard];
			if (!dev.IsTimeShifting) return String.Empty;
			
			string FileName=String.Format(@"{0}\card{1}\{2}",dev.RecordingPath, m_iCurrentCard+1,dev.TimeShiftFileName);
			return FileName;
		}

		static public string GetTimeShiftFileName(int card)
		{
			if (card<0 || card>=m_tvcards.Count) return String.Empty;
			TVCaptureDevice dev=(TVCaptureDevice) m_tvcards[card];
			string FileName=String.Format(@"{0}\card{1}\{2}",dev.RecordingPath, card+1,dev.TimeShiftFileName);
			return FileName;
		}




		#endregion

		#region Radio
		static public bool IsRadio()
		{
			for (int i=0; i < m_tvcards.Count;++i)
			{
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
				if (dev.IsRadio)
				{
					return true;
				}
			}
			return false;
		}

		static public string RadioStationName()
		{
			for (int i=0; i < m_tvcards.Count;++i)
			{
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
				if (dev.IsRadio)
				{
					return dev.RadioStation;
				}
			}
			return string.Empty;
		}
		
		static public void StopRadio()
		{	
			foreach (TVCaptureDevice dev in m_tvcards)
			{
				if (dev.IsRadio)
				{
					Log.WriteFile(Log.LogType.Recorder,"Recorder:StopRadio() stop radio on card:{0}", dev.ID);
					dev.Stop();
				}
			}
			if (g_Player.Playing && g_Player.IsRadio)
			{
				g_Player.Stop();
			}
			m_dtStart=new DateTime(1971,6,11,0,0,0,0);
		}

		static public void StartRadio(string radioStationName)
		{
			if (radioStationName==null) 
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:StartRadio() listening radioStation=null?");
				return ;
			}
			if (radioStationName==String.Empty)  
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:StartRadio() listening radioStation=empty");
				return ;
			}
			if (m_eState!= State.Initialized)  
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:StartRadio() but recorder is not initalised");
				return ;
			}
			g_Player.Stop();
			StopViewing();
			Log.WriteFile(Log.LogType.Recorder,"Recorder:StartRadio():{0}",radioStationName);
			RadioStation radiostation;
			if (!RadioDatabase.GetStation( radioStationName,out radiostation) )
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:StartRadio()  unknown station:{0}", radioStationName);
				return ;
			}

			for (int i=0; i < m_tvcards.Count;++i)
			{
				TVCaptureDevice tvcard =(TVCaptureDevice)m_tvcards[i];
				if (!tvcard.IsRecording)
				{
					if (RadioDatabase.CanCardTuneToStation(radioStationName, tvcard.ID)  || m_tvcards.Count==1)
					{
						for (int x=0; x < m_tvcards.Count;++x)
						{
							TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[x];
							if (i!=x)
							{
								if (dev.IsRadio)
								{
									dev.Stop();
								}
							}
						}
						Log.WriteFile(Log.LogType.Recorder,"Recorder:StartRadio()  start on card:{0} station:{1}",
															tvcard.ID,radioStationName);
						tvcard.StartRadio(radiostation)	;
						m_dtStart=new DateTime(1971,6,11,0,0,0,0);;
						return;
					}
				}
			}
			Log.WriteFile(Log.LogType.Recorder,"Recorder:StartRadio()  no free card which can listen to radio channel:{0}", radioStationName);
		}

		#endregion

		#region TV watching
		/// <summary>
		/// Stop viewing on all cards
		/// </summary>
		static public void StopViewing()
		{
			Log.WriteFile(Log.LogType.Recorder,"Recorder:StopViewing()");
			TVCaptureDevice dev ;
			for (int i=0; i < m_tvcards.Count;++i)
			{
				dev=(TVCaptureDevice)m_tvcards[i];
				Log.WriteFile(Log.LogType.Recorder,"Recorder:  Card:{0} viewing:{1} recording:{2} timeshifting:{3} channel:{4}",
					dev.ID,dev.View,dev.IsRecording,dev.IsTimeShifting,dev.TVChannel);
			}
			if (g_Player.Playing)
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:  currently playing:{0}", g_Player.CurrentFile);
			}
			// stop any playing..
			if (g_Player.Playing && g_Player.IsTV) 
			{
				g_Player.Stop();
			}

			// stop any card viewing..
			for (int i=0; i < m_tvcards.Count;++i)
			{
				dev =(TVCaptureDevice)m_tvcards[i];
				if (!dev.IsRecording)
				{
					if (dev.IsTimeShifting)
					{
						Log.WriteFile(Log.LogType.Recorder,"Recorder:  stop timeshifting card {0} channel:{1}",dev.ID, dev.TVChannel);
						dev.StopTimeShifting();
					}
					if (dev.View) 
					{
						Log.WriteFile(Log.LogType.Recorder,"Recorder:  stop viewing card {0} channel:{1}",dev.ID, dev.TVChannel);
						dev.View=false;
					}
					dev.DeleteGraph();
				}
			}
			m_iCurrentCard=-1;
			m_dtStart=new DateTime(1971,6,11,0,0,0,0);
		}//static public void StopViewing()


		/// <summary>
		/// Turns of watching TV /radio on all capture cards
		/// </summary>
		/// <param name="exceptCard">
		/// index in m_tvcards so 0<= exceptCard< m_tvcards.Count
		/// if exceptCard==-1 then tv/radio is turned on all cards
		/// else this tells which card should be ignored and not turned off 
		/// </param>
		/// <remarks>
		/// Only viewing is stopped. If a card is recording then this wont be stopped
		/// </remarks>
		static private void TurnTvOff(int exceptCard)
		{
			m_dtStart=new DateTime(1971,6,11,0,0,0,0);
			for (int i=0; i< m_tvcards.Count;++i)
			{
				if (i==exceptCard) continue;

				TVCaptureDevice dev = (TVCaptureDevice)m_tvcards[i];
				string strTimeShiftFileName=GetTimeShiftFileName(i);
				if (dev.SupportsTimeShifting)
				{
					if (g_Player.CurrentFile==strTimeShiftFileName)
					{
						Log.WriteFile(Log.LogType.Recorder,"Recorder:  stop playing timeshifting file for card:{0}",dev.ID);
						g_Player.Stop();
					}
				}

				//if card is not recording, then stop the card
				if (!dev.IsRecording)
				{
					if (dev.IsTimeShifting || dev.View || dev.IsRadio)
					{
						Log.WriteFile(Log.LogType.Recorder,"Recorder:  stop card:{0}", dev.ID);
						dev.Stop();
					}
				}
			}
		}

		/// <summary>
		/// Start watching TV.
		/// </summary>
		/// <param name="channel">name of the tv channel</param>
		/// <param name="TVOnOff">
		/// true : turn tv on (start watching)
		/// false: turn tv off (stop watching)
		/// </param>
		/// <param name="timeshift">
		/// true: use timeshifting if possible
		/// false: dont use timeshifting
		/// </param>
		/// <remarks>
		/// The following algorithm is used to determine which tuner card will be used:
		/// 1. if a card is already recording the channel requested then that card will be used for viewing
		///    by just starting to play the timeshift buffer of the card
		/// 2. if a card is already timeshifting (on same or other channel) and it can also view
		///    the channel requested, then that card will be used for viewing
		/// else MP will determine which card:
		///   - is free
		///   - has the highest priority
		///   - and can view the selected tv channel
		/// if it finds a card matching these criteria it will start viewing on the card found
		/// </remarks>
		static public void StartViewing(string channel, bool TVOnOff, bool timeshift)
		{
			// checks
			if (channel==null) 
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:Start TV viewing channel=null?");
				return ;
			}
			if (channel==String.Empty)  
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:Start TV viewing channel=empty");
				return ;
			}
			if (m_eState!= State.Initialized)  
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:Start TV viewing but recorder is not initalised");
				return ;
			}

			Log.WriteFile(Log.LogType.Recorder,"Recorder:StartViewing() channel:{0} tvon:{1} timeshift:{2} vmr9:{3}",
										channel,TVOnOff,timeshift, GUIGraphicsContext.Vmr9Active);
			TVCaptureDevice dev;
			LogTvStatistics();
	
			string strTimeShiftFileName;
			if (TVOnOff==false)
			{
				TurnTvOff(-1);
				TVChannelName=String.Empty;
				m_iCurrentCard=-1;
				return;
			}
			
			Log.WriteFile(Log.LogType.Recorder,"Recorder:  Turn tv on channel:{0}", channel);

			int cardNo=-1;
			// tv should be turned on
			// check if any card is already tuned to this channel...
			for (int i=0; i < m_tvcards.Count;++i)
			{
				dev=(TVCaptureDevice)m_tvcards[i];
				//is card already viewing ?
				if ( dev.IsTimeShifting || dev.View )
				{
					//can card view the new channel we want?
					if (TVDatabase.CanCardViewTVChannel(channel,dev.ID)  || m_tvcards.Count==1 )
					{
						// is it not recording ? or is it recording the channel we want to watch ?
						if (!dev.IsRecording || (dev.IsRecording&& dev.TVChannel==channel ))
						{
							if (dev.IsRecording)
							{
								cardNo=i;
								break;
							}
							cardNo=i;
						}
					}
				}
			}
			if (cardNo>=0)
			{
				dev = (TVCaptureDevice)m_tvcards[cardNo];
				Log.WriteFile(Log.LogType.Recorder,"Recorder:  Found card:{0}", dev.ID);
						
				//stop viewing on any other card
				TurnTvOff(cardNo);

				m_iCurrentCard=cardNo;
				TVChannelName=channel;

				// do we want timeshifting?
				if  (timeshift || dev.IsRecording)
				{
					//yes
					strTimeShiftFileName=GetTimeShiftFileName(m_iCurrentCard);
					if (g_Player.CurrentFile!=strTimeShiftFileName)
					{
						g_Player.Stop();
					}
					if (dev.TVChannel!=channel)
					{
						TuneExternalChannel(channel,true);
						dev.TVChannel=channel;
					}
					if (!dev.IsRecording  && !dev.IsTimeShifting && dev.SupportsTimeShifting)
					{
						Log.WriteFile(Log.LogType.Recorder,"Recorder:  start timeshifting on card:{0}", dev.ID);
						dev.StartTimeShifting();
					}

					//yes, check if we're already playing/watching it
					strTimeShiftFileName=GetTimeShiftFileName(m_iCurrentCard);
					if (g_Player.CurrentFile!=strTimeShiftFileName)
					{
						Log.WriteFile(Log.LogType.Recorder,"Recorder:  start viewing timeshift file of card {0}", dev.ID);
						g_Player.Play(strTimeShiftFileName);
					}
					m_dtStart=new DateTime(1971,6,11,0,0,0,0);
					return;
				}//if  (timeshift || dev.IsRecording)
				else
				{
					//we dont want timeshifting
					strTimeShiftFileName=GetTimeShiftFileName(m_iCurrentCard);
					if (g_Player.CurrentFile==strTimeShiftFileName)
						g_Player.Stop();
					if (dev.IsTimeShifting)
					{
						Log.WriteFile(Log.LogType.Recorder,"Recorder:  stop timeshifting on card:{0}", dev.ID);
						dev.StopTimeShifting();
					}
					if (dev.TVChannel!=channel)
					{
						TuneExternalChannel(channel,true);
						dev.TVChannel=channel;
					}
					dev.View=true;
					m_dtStart=new DateTime(1971,6,11,0,0,0,0);
					return;
				}
			}//if (cardNo>=0)

			Log.WriteFile(Log.LogType.Recorder,"Recorder:  find free card");

			TurnTvOff(-1);

			// no cards are timeshifting the channel we want.
			// Find a card which can view the channel
			int card=-1;
			int prio=-1;
			for (int i=0; i < m_tvcards.Count;++i)
			{
				dev=(TVCaptureDevice)m_tvcards[i];
				if (!dev.IsRecording)
				{
					if (TVDatabase.CanCardViewTVChannel(channel,dev.ID)  || m_tvcards.Count==1)
					{
						if (dev.Priority>prio)
						{
							card=i;
							prio=dev.Priority;
						}
					}
				}
			}

			if (card < 0) 
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:  No free card which can receive channel [{0}]", channel);
				return; // no card available
			}

			m_iCurrentCard=card;
			TVChannelName=channel;
			dev=(TVCaptureDevice)m_tvcards[m_iCurrentCard];
			
			Log.WriteFile(Log.LogType.Recorder,"Recorder:  found free card {0} prio:{1} name:{2}",dev.ID,dev.Priority,dev.CommercialName);

			//do we want to use timeshifting ?
			if (timeshift)
			{
				// yep, then turn timeshifting on
				strTimeShiftFileName=GetTimeShiftFileName(m_iCurrentCard);
				if (g_Player.CurrentFile!=strTimeShiftFileName)
				{
					g_Player.Stop();
				}
				// yes, does card support it?
				if (dev.SupportsTimeShifting)
				{
					Log.WriteFile(Log.LogType.Recorder,"Recorder:  start timeshifting card {0} channel:{1}",dev.ID,channel);
					TuneExternalChannel(channel,true);
					dev.TVChannel=channel;
					dev.StartTimeShifting();
					TVChannelName=channel;

					// and play the timeshift file (if its not already playing it)
					strTimeShiftFileName=GetTimeShiftFileName(m_iCurrentCard);
					if (g_Player.CurrentFile!=strTimeShiftFileName)
					{
						Log.WriteFile(Log.LogType.Recorder,"Recorder:  currentfile:{0} newfile:{1}", g_Player.CurrentFile,strTimeShiftFileName);
						g_Player.Play(strTimeShiftFileName);
					}
					m_dtStart=new DateTime(1971,6,11,0,0,0,0);
					return;
				}//if (dev.SupportsTimeShifting)
			}//if (timeshift)

			//tv should be turned on without timeshifting
			//just present the overlay tv view
			// now start watching on our card
			Log.WriteFile(Log.LogType.Recorder,"Recorder:  start watching on card:{0} channel:{1}", dev.ID,channel);
			TuneExternalChannel(channel,true);
			dev.TVChannel=channel;
			dev.View=true;
			TVChannelName=channel;
			m_dtStart=new DateTime(1971,6,11,0,0,0,0);
		}//static public void StartViewing(string channel, bool TVOnOff, bool timeshift)

		#endregion

		#region Process and properties
		static void ProcessCards()
		{
			for (int i=0; i < m_tvcards.Count;++i)
			{
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
				dev.Process();
				if (m_iCurrentCard==i)
				{
					if (dev.IsTimeShifting && !dev.IsRecording)
					{
						if (!g_Player.Playing)
						{
							dev.Stop();
							m_iCurrentCard=-1;
						}
					}
				}
			}
		}
		/// <summary>
		/// Scheduler main loop. This function needs to get called on a regular basis.
		/// It will handle all scheduler tasks
		/// </summary>
		static public void Process()
		{
			if (m_eState!=State.Initialized) return;
			if (GUIGraphicsContext.InVmr9Render) return;
			ProcessCards();
			
			TimeSpan ts=DateTime.Now-m_dtProgresBar;
			if (ts.TotalMilliseconds>10000)
			{
				RecorderProperties.UpdateRecordingProperties();
				m_dtProgresBar=DateTime.Now;
			}

			ts=DateTime.Now-m_dtStart;
			if (ts.TotalMilliseconds<30000) return;
			Recorder.HandleRecordings();
			DiskManagement.CheckRecordingDiskSpace();
			Recorder.HandleNotifies();
			m_dtStart=DateTime.Now;
		}//static public void Process()
    
		

		#endregion

		#region Helper functions
		/// <summary>
		/// This function gets called by the TVDatabase when a recording has been
		/// added,changed or deleted. It forces the Scheduler to get update its list of
		/// recordings.
		/// </summary>
		static private void OnRecordingsChanged(TVDatabase.RecordingChange change)
		{ 
			if (m_eState!=State.Initialized) return;
			m_bRecordingsChanged=true;
			m_dtStart=new DateTime(1971,11,6,20,0,0,0);
		}
		
		/// <summary>
		/// Handles incoming messages from other modules
		/// </summary>
		/// <param name="message">message received</param>
		/// <remarks>
		/// Supports the following messages:
		///  GUI_MSG_RECORDER_ALLOC_CARD 
		///  When received the scheduler will release/free all resources for the
		///  card specified so other assemblies can use it
		///  
		///  GUI_MSG_RECORDER_FREE_CARD
		///  When received the scheduler will alloc the resources for the
		///  card specified. Its send when other assemblies dont need the card anymore
		///  
		///  GUI_MSG_RECORDER_STOP_TIMESHIFT
		///  When received the scheduler will stop timeshifting.
		///  
		///  GUI_MSG_RECORDER_STOP_TV
		///  When received the scheduler will stop viewing tv on any card.
		///  
		///  GUI_MSG_RECORDER_STOP_RADIO
		///  When received the scheduler will stop listening radio on any card.
		/// </remarks>
		static public void OnMessage(GUIMessage message)
		{
			if (message==null) return;
			switch(message.Message)
			{
					
				case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TV:
					StopViewing();
					break;
				case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_RADIO:
					StopRadio();
				break;
				case GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO:
					StartRadio(message.Label);
				break;

				case GUIMessage.MessageType.GUI_MSG_RECORDER_ALLOC_CARD:
					// somebody wants to allocate a capture card
					// if possible, lets release it
					foreach (TVCaptureDevice card in m_tvcards)
					{
						if (card.VideoDevice.Equals(message.Label))
						{
							if (!card.IsRecording)
							{
								card.Stop();
								card.Allocated=true;
								return;
							}
						}
					}
					break;

					
				case GUIMessage.MessageType.GUI_MSG_RECORDER_FREE_CARD:
					// somebody wants to allocate a capture card
					// if possible, lets release it
					foreach (TVCaptureDevice card in m_tvcards)
					{
						if (card.VideoDevice.Equals(message.Label))
						{
							if (card.Allocated)
							{
								card.Allocated=false;
								return;
							}
						}
					}
					break;

				case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TIMESHIFT:
					foreach (TVCaptureDevice card in m_tvcards)
					{
						if (!card.IsRecording)
						{
							if (card.IsTimeShifting)
							{
							  Log.WriteFile(Log.LogType.Recorder,"Recorder: stop timeshifting on card:{0} channel:{1}",
																	card.ID,card.TVChannel);								
								card.Stop();
							}
						}
					}
					break;
			}//switch(message.Message)
		}//static public void OnMessage(GUIMessage message)


		/// <summary>
		/// Shows in the log file which cards are in use and what they are doing
		/// Also logs which file is currently being played
		/// </summary>
		static private void LogTvStatistics()
		{
			TVCaptureDevice dev;
			for (int i=0; i < m_tvcards.Count;++i)
			{
				dev=(TVCaptureDevice)m_tvcards[i];
				if (!dev.IsRecording)
				{
					Log.WriteFile(Log.LogType.Recorder,"Recorder:  Card:{0} viewing:{1} recording:{2} timeshifting:{3} channel:{4}",
						dev.ID,dev.View,dev.IsRecording,dev.IsTimeShifting,dev.TVChannel);
				}
				else
				{
					Log.WriteFile(Log.LogType.Recorder,"Recorder:  Card:{0} viewing:{1} recording:{2} timeshifting:{3} channel:{4} id:{5}",
						dev.ID,dev.View,dev.IsRecording,dev.IsTimeShifting,dev.TVChannel,dev.CurrentTVRecording.ID);
				}
			}
			if (g_Player.Playing)
			{
				Log.WriteFile(Log.LogType.Recorder,"Recorder:  currently playing:{0}", g_Player.CurrentFile);
			}
		}
		

		/// <summary>
		/// This method will send a message to all 'external tuner control' plugins like USBUIRT
		/// to switch channel on the remote device
		/// </summary>
		/// <param name="strChannelName">name of channel</param>
		static void TuneExternalChannel(string strChannelName, bool isViewing)
		{
			if (strChannelName==null) return;
			if (strChannelName==String.Empty) return;
			foreach (TVChannel chan in m_TVChannels)
			{
				if (chan.Name.Equals(strChannelName))
				{
					if (chan.External)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL,0,0,0,0,0,null);
						msg.Label=chan.ExternalTunerChannel;
						GUIWindowManager.SendThreadMessage(msg);
					}
					break;
				}
			}
			if (isViewing)
				SetZapOSDData(strChannelName);
		}//static void TuneExternalChannel(string strChannelName)
    
		#endregion


		#region audiostream selection
		static public int GetAudioLanguage()
		{
			if (m_eState!= State.Initialized) return -1;
			if (m_iCurrentCard<0 || m_iCurrentCard >= m_tvcards.Count) return -1;
			TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
			
			return dev.GetAudioLanguage();
		}

		static public void SetAudioLanguage(int audioPid)
		{
			if (m_eState!= State.Initialized) return;
			if (m_iCurrentCard<0 || m_iCurrentCard >= m_tvcards.Count) return;
			TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
			
			dev.SetAudioLanguage(audioPid);
		}

		static public ArrayList GetAudioLanguageList()
		{
			if (m_eState!= State.Initialized) return null;
			if (m_iCurrentCard<0 || m_iCurrentCard >= m_tvcards.Count) return null;
			TVCaptureDevice dev= (TVCaptureDevice)m_tvcards[m_iCurrentCard];
									
			return dev.GetAudioLanguageList();
		}
		#endregion

		private static void card_OnTvRecordingEnded(string recordingFileName, TVRecording recording, TVProgram program)
		{
			Log.WriteFile(Log.LogType.Recorder,"Recorder: recording ended '{0}' on channel:{1} from {2}-{3} id:{4} priority:{5} quality:{6}",recording.Title,recording.Channel, recording.StartTime.ToLongTimeString(), recording.EndTime.ToLongTimeString(),recording.ID, recording.Priority,recording.Quality.ToString());
			if (OnTvRecordingEnded!=null)
				OnTvRecordingEnded(recordingFileName,recording, program);
			if (OnTvRecordingChanged!=null)
				OnTvRecordingChanged();
		}

		private static void card_OnTvRecordingStarted(string recordingFileName, TVRecording recording, TVProgram program)
		{
			Log.WriteFile(Log.LogType.Recorder,"Recorder: recording started '{0}' on channel:{1} from {2}-{3} id:{4} priority:{5} quality:{6}",recording.Title,recording.Channel, recording.StartTime.ToLongTimeString(), recording.EndTime.ToLongTimeString(),recording.ID, recording.Priority,recording.Quality.ToString());
			if (OnTvRecordingStarted!=null)
				OnTvRecordingStarted(recordingFileName,recording, program);
			if (OnTvRecordingChanged!=null)
				OnTvRecordingChanged();
		}
		private static void OnNotifiesChanged()
		{
			m_bNotifiesChanged=true;
		}
		static void HandleNotifies()
		{
			if (m_bNotifiesChanged)
			{
				m_Notifies.Clear();
				TVDatabase.GetNotifies(m_Notifies,true);
				m_bNotifiesChanged=false;
			}
			DateTime dt5Mins=DateTime.Now.AddMinutes(5);
			for (int i=0; i < m_Notifies.Count;++i)
			{
				TVNotify notify = (TVNotify)m_Notifies[i];
				if ( dt5Mins> notify.Program.StartTime)
				{
					TVDatabase.DeleteNotify(notify);
					GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM,0,0,0,0,0,null);
					msg.Object=notify.Program;
					GUIGraphicsContext.SendMessage( msg );
					msg=null;
				}
			}
		}
	}//public class Recorder
}//namespace MediaPortal.TV.Recording
