using System;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Player;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// This class is a singleton which implements the
	/// -task scheduler to schedule, (start,stop) all tv recordings on time
	/// -a front end to other classes to control the tv capture cardsd
	/// </summary>
	public class Recorder
	{
		enum State
		{
			None,
			Initializing,
			Initialized,
			Deinitializing
		}
		static string TVChannelCovertArt=@"thumbs\tv\logos";
		static bool          m_bRecordingsChanged=false;  // flag indicating that recordings have been added/changed/removed
		//static bool          m_bTimeshifting =false;       //todo
		//static bool          m_bAlwaysTimeshift=false;  //todo
		//static bool          m_bViewing=false;  //todo
		static int           m_iPreRecordInterval =0;
		static int           m_iPostRecordInterval=0;
		static TVUtil        m_TVUtil=null;
		static string        m_strTVChannel="";
		static State         m_eState=State.None;
		static ArrayList     m_tvcards    = new ArrayList();
		static ArrayList     m_TVChannels = new ArrayList();
		static ArrayList     m_Recordings = new ArrayList();
		static string        m_strRecPath="";
		static DateTime      m_dtStart=DateTime.Now;
		static int           m_iCurrentCard=-1;

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
			CleanProperties();
			m_bRecordingsChanged=false;
    

			Log.Write("Recorder: Loading capture cards from capturecards.xml");
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
				Log.Write("Recorder: invalid capturecards.xml found! please delete it");
			}
			if (m_tvcards.Count==0) 
			{
				Log.Write("Recorder: no capture cards found. Use file->setup to setup tvcapture!");
			}
			for (int i=0; i < m_tvcards.Count;i++)
			{
				TVCaptureDevice card=(TVCaptureDevice)m_tvcards[i];
				card.ID=(i+1);
				Log.Write("Recorder:    card:{0} video device:{1} TV:{2}  record:{3} priority:{4}",
					card.ID,card.VideoDevice,card.UseForTV,card.UseForRecording,card.Priority);
			}

			m_iPreRecordInterval =0;
			m_iPostRecordInterval=0;
			//m_bAlwaysTimeshift=false;
			using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				m_iPreRecordInterval = xmlreader.GetValueAsInt("capture","prerecord", 5);
				m_iPostRecordInterval= xmlreader.GetValueAsInt("capture","postrecord", 5);
				//m_bAlwaysTimeshift   = xmlreader.GetValueAsBool("mytv","alwaystimeshift",false);
				m_strTVChannel  = xmlreader.GetValueAsString("mytv","channel","");
				m_strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
				m_strRecPath=Utils.RemoveTrailingSlash(m_strRecPath);
				if (m_strRecPath==null||m_strRecPath.Length==0) 
				{
					m_strRecPath=System.IO.Directory.GetCurrentDirectory();
					m_strRecPath=Utils.RemoveTrailingSlash(m_strRecPath);
				}
			}

			for (int i=0; i < m_tvcards.Count;++i)
			{
				try
				{
					string dir=String.Format(@"{0}\card{1}",m_strRecPath,i+1);
					System.IO.Directory.CreateDirectory(dir);
					DeleteOldTimeShiftFiles(dir);
				}
				catch(Exception){}
			}

			m_TVChannels.Clear();
			TVDatabase.GetChannels(ref m_TVChannels);

			m_Recordings.Clear();
			TVDatabase.GetRecordings(ref m_Recordings);

			m_TVUtil= new TVUtil();

			TVDatabase.OnRecordingsChanged += new TVDatabase.OnChangedHandler(Recorder.OnRecordingsChanged);
      
			GUIWindowManager.Receivers += new SendMessageHandler(Recorder.OnMessage);
			m_eState=State.Initialized;
		}//static public void Start()

		/// <summary>
		/// Stops the scheduler. It will cleanup all resources allocated and free
		/// the capture cards
		/// </summary>
		static public void Stop()
		{
			if (m_eState != State.Initialized) return;
			m_eState=State.Deinitializing;
			TVDatabase.OnRecordingsChanged -= new TVDatabase.OnChangedHandler(Recorder.OnRecordingsChanged);
			//TODO
			GUIWindowManager.Receivers -= new SendMessageHandler(Recorder.OnMessage);

			foreach (TVCaptureDevice dev in m_tvcards)
			{
				dev.Stop();
			}
			CleanProperties();
			m_bRecordingsChanged=false;
			m_eState=State.None;
		}//static public void Stop()


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

			// If the recording schedules have been changed since last time
			if (m_bRecordingsChanged)
			{
				// then get (refresh) all recordings from the database
				m_Recordings.Clear();
				m_TVChannels.Clear();
				TVDatabase.GetRecordings(ref m_Recordings);
				TVDatabase.GetChannels(ref m_TVChannels);
				m_bRecordingsChanged=false;
				foreach (TVRecording recording in m_Recordings)
				{
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
			}//if (m_bRecordingsChanged)

			// no TV cards? then we cannot record anything, so just return
			if (m_tvcards.Count==0)  return;

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
					// check which program is running 
					TVProgram prog=m_TVUtil.GetProgramAt(chan.Name,dtCurrentTime.AddMinutes(m_iPreRecordInterval) );

					// if the recording should record the tv program
					if ( rec.IsRecordingProgramAtTime(dtCurrentTime,prog,m_iPreRecordInterval, m_iPostRecordInterval) )
					{
						// yes, then record it
						if (Record(dtCurrentTime,rec,prog, m_iPreRecordInterval, m_iPostRecordInterval))
						{
							break;
						}
					}
				}
			}
   

			for (int j=0; j < m_Recordings.Count;++j)
			{
				TVRecording rec =(TVRecording)m_Recordings[j];
				// 1st check if the recording itself should b recorded
				if ( rec.IsRecordingProgramAtTime(DateTime.Now,null,m_iPreRecordInterval, m_iPostRecordInterval) )
				{
					// yes, then record it
					if ( Record(dtCurrentTime,rec,null,m_iPreRecordInterval, m_iPostRecordInterval))
					{
						// recording it
					}
				}
			}
		}//static void HandleRecordings()


		/// <summary>
		/// Starts recording the specified tv channel immediately using a reference recording
		/// When called this method starts an erference  recording on the channel specified
		/// It will record the next 2 hours.
		/// </summary>
		/// <param name="strChannel">TVchannel to record</param>
		static public void RecordNow(string strChannel)
		{
			if (strChannel==null) return;
			if (strChannel==String.Empty) return;
			if (m_eState!= State.Initialized) return;
      
			// create a new recording which records the next 2 hours...
			TVRecording tmpRec = new TVRecording();
			TVUtil util = new TVUtil();
			TVProgram program=util.GetCurrentProgram(strChannel);
			if (program!=null)
			{
				//record current playing program
				tmpRec.Start=program.Start;
				tmpRec.End=program.End;
				tmpRec.Title=program.Title;
				Log.Write("Recorder: record now:{0} program:{1}",strChannel,program.Title);
			}
			else
			{
				//no tvguide data, just record the next 2 hours
				Log.Write("Recorder: record now:{0} for next 2 hours",strChannel);
				tmpRec.Start=Utils.datetolong(DateTime.Now);
				tmpRec.End=Utils.datetolong(DateTime.Now.AddMinutes(2*60) );
				tmpRec.Title=GUILocalizeStrings.Get(413);
			}
			tmpRec.Channel=strChannel;
			tmpRec.RecType=TVRecording.RecordingType.Once;
			tmpRec.IsContentRecording=false;//make a reference recording!

			Log.Write("Recorder: start: {0} {1}",tmpRec.StartTime.ToShortDateString(), tmpRec.StartTime.ToShortTimeString());
			Log.Write("Recorder: end  : {0} {1}",tmpRec.EndTime.ToShortDateString(), tmpRec.EndTime.ToShortTimeString());

			TVDatabase.AddRecording(ref tmpRec);
		}//static public void RecordNow(string strChannel)

		/// <summary>
		/// Finds a free capture card and if found tell it to record the specified program
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
			Log.Write("Recorder: time to record a {0} on channel:{1} from {2}-{3}",rec.Title,rec.Channel, rec.StartTime.ToLongTimeString(), rec.EndTime.ToLongTimeString());
			Log.Write("Recorder: find free capture card");
			foreach (TVCaptureDevice dev in m_tvcards)
			{
				Log.Write("Recorder: Card:{0} {1} viewing:{2} recording:{3} timeshifting:{4} channel:{5}",
					dev.ID,dev.FriendlyName,dev.View,dev.IsRecording,dev.IsTimeShifting,dev.TVChannel);
			}
			if (g_Player.Playing)
			{
				Log.Write("Recorder: currently playing:{0}", g_Player.CurrentFile);
			}

			// find free device for recording
			int cardNo=0;
			int highestPrio=-1;
			int highestCard=-1;
			foreach (TVCaptureDevice dev in m_tvcards)
			{
				//is card used for recording tv?
				if (dev.UseForRecording)
				{
					// and is it not recording already?
					if (!dev.IsRecording)
					{
						//an can it receive the channel we want to record?
						if (TVDatabase.CanCardViewTVChannel(rec.Channel, dev.ID) )
						{
							// does this card have a higher priority?
							if (dev.Priority>highestPrio)
							{
								//yes then we use this card
								highestPrio=dev.Priority;
								highestCard=cardNo;
							}

							//if this card has the same priority and is already watching this channel
							//then we use this card
							if (dev.Priority==highestPrio)
							{
								if ( (dev.IsTimeShifting||dev.View==true) && dev.TVChannel==rec.Channel)
								{
									highestPrio=dev.Priority;
									highestCard=cardNo;
								}
							}
						}
					}
				}
				cardNo++;
			}

			//did we found a card available for recording
			if (highestCard>=0)
			{
				//yes then record
				cardNo=highestCard;
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[cardNo];
				Log.Write("Recorder: using capture card:{0} {1} prio:{2}", dev.ID, dev.VideoDevice,dev.Priority);
				bool viewing=(m_iCurrentCard==cardNo);
				TuneExternalChannel(rec.Channel);
				dev.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
				
				if (viewing) m_strTVChannel=rec.Channel;
				return true;
			}

			// still no device found. 
			// if we skip the pre-record interval should the new recording still be started then?
			if ( rec.IsRecordingProgramAtTime(currentTime,currentProgram,0,0) )
			{
				// yes, then find & stop any capture running which is busy post-recording
				Log.Write("Recorder: No card found, check if a card is post-recording");
				cardNo=0;
				highestPrio=-1;
				highestCard=-1;
				foreach (TVCaptureDevice dev in m_tvcards)
				{
					//is this card used for recording?
					if (dev.UseForRecording)
					{
						// is it post recording?
						if (dev.IsPostRecording)
						{
							//an can it receive the channel we want to record?
							if (TVDatabase.CanCardViewTVChannel(rec.Channel, dev.ID) )
							{
								
								// does this card have a higher priority?
								if (dev.Priority>highestPrio)
								{
									//yes then we use this card
									highestPrio=dev.Priority;
									highestCard=cardNo;
								}

								//if this card has the same priority and is already watching this channel
								//then we use this card
								if (dev.Priority==highestPrio)
								{
									if ( (dev.IsTimeShifting||dev.View==true) && dev.TVChannel==rec.Channel)
									{
										highestPrio=dev.Priority;
										highestCard=cardNo;
									}
								}
							}
						}
					}
					cardNo++;
				}
			}
			
			//did we found a card available for recording
			if (highestCard>=0)
			{
				//yes then record
				cardNo=highestCard;
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[cardNo];
				bool viewing=(m_iCurrentCard==cardNo);
				Log.Write("Recorder: capture card:{0} {1} was post-recording. Now use it for recording new program", dev.ID,dev.VideoDevice);
				dev.StopRecording();
				TuneExternalChannel(rec.Channel);
				dev.Record(rec,currentProgram,iPostRecordInterval,iPostRecordInterval);
									
				if (viewing) m_strTVChannel=rec.Channel;
				return true;
			}

			//no device free...
			Log.Write("Recorder: no capture cards are available right now for recording");
			return false;
		}//static bool Record(DateTime currentTime,TVRecording rec, TVProgram currentProgram,int iPreRecordInterval, int iPostRecordInterval)

		/// <summary>
		/// Stops all recording on the current channel. 
		/// </summary>
		static public void StopRecording()
		{
			if (m_eState!= State.Initialized) return ;
			if (m_iCurrentCard <0 || m_iCurrentCard >=m_tvcards.Count) return ;
			TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[m_iCurrentCard];
      
			if (dev.IsRecording) 
			{
				Log.Write("Recorder: Stop recording on channel:{0} capture card:{1} {2}", dev.TVChannel,dev.ID,dev.FriendlyName);
				int ID=dev.CurrentTVRecording.ID;
				for (int i=0; i < m_Recordings.Count;++i)
				{
					TVRecording rec =(TVRecording )m_Recordings[i];
					if (rec.ID==ID)
					{	
						rec.Canceled=Utils.datetolong(DateTime.Now);
						TVDatabase.ChangeRecording(ref rec);
						break;
					}
				}
				dev.StopRecording();
			}
		}//static public void StopRecording()

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
		/// Stop viewing on all cards
		/// </summary>
		static public void StopViewing()
		{
			Log.Write("Recorder: StopViewing()");
			TVCaptureDevice dev ;
			for (int i=0; i < m_tvcards.Count;++i)
			{
				dev=(TVCaptureDevice)m_tvcards[i];
				Log.Write("Recorder:Card:{0} {1} viewing:{2} recording:{3} timeshifting:{4} channel:{5}",
					dev.ID,dev.FriendlyName,dev.View,dev.IsRecording,dev.IsTimeShifting,dev.TVChannel);
			}
			if (g_Player.Playing)
			{
				Log.Write("Recorder:currently playing:{0}", g_Player.CurrentFile);
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
						Log.Write("Recorder: stop timeshifting {0} on card {1} {2}", dev.TVChannel,dev.ID,dev.FriendlyName);
						dev.StopTimeShifting();
					}
					if (dev.View) 
					{
						Log.Write("Recorder: stop viewing {0} on card {1} {2}", dev.TVChannel,dev.ID,dev.FriendlyName);
						dev.View=false;
					}
					dev.DeleteGraph();
				}
			}
			m_iCurrentCard=-1;
		}//static public void StopViewing()

		/// <summary>
		/// Property which returns the timeshifting file for the current channel
		/// </summary>
		static public string GetTimeShiftFileName()
		{
			if (m_iCurrentCard<0 || m_iCurrentCard>=m_tvcards.Count) return String.Empty;
			TVCaptureDevice dev=(TVCaptureDevice) m_tvcards[m_iCurrentCard];
			if (!dev.IsTimeShifting) return String.Empty;
			
			string FileName=String.Format(@"{0}\card{1}\live.tv",m_strRecPath, m_iCurrentCard+1);
			return FileName;
		}

		static public string GetTimeShiftFileName(int card)
		{
			string FileName=String.Format(@"{0}\card{1}\live.tv",m_strRecPath, card+1);
			return FileName;
		}


		static public void StartViewing(string channel, bool TVOnOff, bool timeshift)
		{
			if (channel==null) 
			{
				Log.Write("Recorder:Start TV viewing channel=null?");
				return ;
			}
			if (channel==String.Empty)  
			{
				Log.Write("Recorder:Start TV viewing channel=empty");
				return ;
			}
			if (m_eState!= State.Initialized)  
			{
				Log.Write("Recorder:Start TV viewing but recorder is not initalised");
				return ;
			}

			TVCaptureDevice dev;
			for (int i=0; i < m_tvcards.Count;++i)
			{
				dev=(TVCaptureDevice)m_tvcards[i];
				Log.Write("Recorder:Card:{0} {1} viewing:{2} recording:{3} timeshifting:{4} channel:{5}",
								dev.ID,dev.FriendlyName,dev.View,dev.IsRecording,dev.IsTimeShifting,dev.TVChannel);
			}
			if (g_Player.Playing)
			{
				Log.Write("Recorder:currently playing:{0}", g_Player.CurrentFile);
			}
	
			string strTimeShiftFileName;
			if (TVOnOff==false)
			{
				Log.Write("Recorder:turn TV off");
				//TV should be turned off
				if (m_iCurrentCard>=0 && m_iCurrentCard<m_tvcards.Count)
				{
					dev=(TVCaptureDevice)m_tvcards[m_iCurrentCard];
				
					Log.Write("Recorder:stop watching TV on card:{0} {1}", dev.ID, dev.FriendlyName);
					// is card currently recording?
					if (dev.IsRecording) 
					{
						Log.Write("Recorder:card is recording");
						// yes, does it support timeshifting?
						if (dev.SupportsTimeShifting)
						{
							Log.Write("Recorder:stop playing timeshifting file");
							//yes, are playing the timeshifting file?
							if (g_Player.CurrentFile==GetTimeShiftFileName(m_iCurrentCard))
							{
									g_Player.Stop();
							}//if (g_Player.CurrentFile==GetTimeShiftFileName(m_iCurrentCard))
							m_iCurrentCard=-1;
						}//if (dev.SupportsTimeShifting)
					}//if (dev.IsRecording) 
					else
					{
						//card is not recording, just stop tv
						strTimeShiftFileName=GetTimeShiftFileName(m_iCurrentCard);
						if (g_Player.CurrentFile==strTimeShiftFileName)
							g_Player.Stop();
						if (dev.IsTimeShifting)
							dev.StopTimeShifting();
						
						if (dev.View)
							dev.View=false;
						dev.DeleteGraph();
						m_iCurrentCard=-1;
					}
				}//if (m_iCurrentCard>=0 && m_iCurrentCard<m_tvcards.Count)
				return;
			}//if (TVOnOff==false)
			
			Log.Write("Recorder:Turn tv on channel:{0}", channel);

			// tv should be turned on
			// check if any card is already viewing...
			for (int i=0; i < m_tvcards.Count;++i)
			{
				dev=(TVCaptureDevice)m_tvcards[i];
				//is card already viewing ?
				if ( dev.IsTimeShifting || dev.View )
				{
					//can card view the new channel we want?
					if (TVDatabase.CanCardViewTVChannel(channel,dev.ID) )
					{
						// is it not recording ? or recording on the channel we want?
						if (!dev.IsRecording || (dev.IsRecording&& dev.TVChannel==channel ))
						{
							//yes, we found our card
							m_iCurrentCard=i;
							m_strTVChannel=channel;
							Log.Write("Recorder:card:{0} {1} is watching {2}", dev.ID, dev.FriendlyName,channel);
							// do we want timeshifting?
							if  (timeshift || dev.IsRecording)
							{
								dev.TVChannel=channel;
								if (!dev.IsRecording  && !dev.IsTimeShifting && dev.SupportsTimeShifting)
								{
									dev.StartTimeShifting();
								}

								Log.Write("Recorder:StartViewing():start viewing timeshift file of card {0} {1}", dev.ID, dev.FriendlyName);
								//yes, check if we're already playing/watching it
								strTimeShiftFileName=GetTimeShiftFileName(m_iCurrentCard);
								if (g_Player.CurrentFile!=strTimeShiftFileName)
									g_Player.Play(strTimeShiftFileName);
								return;
							}//if  (timeshift || dev.IsRecording)
							else
							{
								//we dont want timeshifting
								Log.Write("Recorder:StartViewing() view card:{0} {1} channel:{2}", dev.ID, dev.FriendlyName, dev.TVChannel);
								
								strTimeShiftFileName=GetTimeShiftFileName(m_iCurrentCard);
								if (g_Player.CurrentFile==strTimeShiftFileName)
									g_Player.Stop();
								if (dev.IsTimeShifting)
									dev.StopTimeShifting();
								
								dev.TVChannel=channel;
								dev.View=true;
								return;
							}
						}//if (!dev.IsRecording || (dev.IsRecording&& dev.TVChannel==channel ))
					}//if (TVDatabase.CanCardViewTVChannel(channel,dev.ID) )
				}//if ( (dev.IsTimeShifting||dev.View) && dev.TVChannel == channel)
			}//for (int i=0; i < m_tvcards.Count;++i)

			Log.Write("Recorder:find free card");
			//stop any other cards which are only viewing
			g_Player.Stop();
			for (int i=0; i < m_tvcards.Count;++i)
			{
				TVCaptureDevice tvcard =(TVCaptureDevice)m_tvcards[i];
				if (!tvcard.IsRecording)
				{
					//stop watching on this card
					Log.Write("Recorder: stop watching on card:{0} {1} channel:{2}", tvcard.ID,tvcard.FriendlyName,tvcard.TVChannel);
					

					if (tvcard.IsTimeShifting)
					{
						tvcard.StopTimeShifting();
					}
					if (tvcard.View)
					{
						tvcard.View=false;
					}
					tvcard.DeleteGraph();
				}
			}

			// no cards are timeshifting the channel we want.
			// Find a card which can view the channel
			int card=-1;
			int prio=-1;
			for (int i=0; i < m_tvcards.Count;++i)
			{
				dev=(TVCaptureDevice)m_tvcards[i];
				if (!dev.IsRecording)
				{
					if (TVDatabase.CanCardViewTVChannel(channel,dev.ID) )
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
				Log.Write("Recorder:No free card which can receive channel {0}", channel);
				return; // no card available
			}
			m_iCurrentCard=card;
			m_strTVChannel=channel;
			dev=(TVCaptureDevice)m_tvcards[m_iCurrentCard];
			
			Log.Write("Recorder:found card {0} {1}",dev.ID,dev.FriendlyName);

			//do we want to use timeshifting ?
			if (timeshift)
			{
				// yes, does card support it?
				if (dev.SupportsTimeShifting)
				{
					// yep, then turn timeshifting on
					Log.Write("Recorder:start timeshifting card {0} {1} channel:{2}",dev.ID,dev.FriendlyName,channel);
					TuneExternalChannel(channel);
					dev.TVChannel=channel;
					dev.StartTimeShifting();
					m_strTVChannel=channel;

					// and play the timeshift file (if its not already playing it)
					strTimeShiftFileName=GetTimeShiftFileName(m_iCurrentCard);
					if (g_Player.CurrentFile!=strTimeShiftFileName)
						g_Player.Play(strTimeShiftFileName);
					return;
				}//if (dev.SupportsTimeShifting)
			}//if (timeshift)

			//tv should be turned on without timeshifting
			//just present the overlay tv view
			// now start watching on our card
			Log.Write("Recorder: start watching on card:{0} {1} channel:{2}", dev.ID,dev.FriendlyName,channel);
			TuneExternalChannel(channel);
			dev.TVChannel=channel;
			dev.View=true;
			m_strTVChannel=channel;
		}//static public void StartViewing(string channel, bool TVOnOff, bool timeshift)

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
					card=i;
					return true;
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
		/// Sets the current tv channel tags. This function gets called when the current
		/// tv channel gets changed. It will update the corresponding skin tags 
		/// </summary>
		/// <remarks>
		/// Sets the current tags:
		/// #TV.View.channel,  #TV.View.thumb
		/// </remarks>
		static void OnTVChannelChanged()
		{
			if (m_eState!= State.Initialized) return ;
			string strLogo=Utils.GetCoverArt(TVChannelCovertArt,m_strTVChannel);
			if (!System.IO.File.Exists(strLogo))
			{
				strLogo="defaultVideoBig.png";
			}
			GUIPropertyManager.SetProperty("#TV.View.channel",m_strTVChannel);
			GUIPropertyManager.SetProperty("#TV.View.thumb",strLogo);
		}//static void OnTVChannelChanged()

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
    
		/// <summary>
		/// Property which get TV Viewing mode.
		/// if TV Viewing  mode is turned on then live tv will be shown
		/// </summary>
		static public bool View
		{
			get 
			{
				if (g_Player.Playing && g_Player.IsTV) return true;
				for (int i=0; i < m_tvcards.Count;++i)
				{
					TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
					if (dev.View) return true;
				}
				return false;
			}
		}//static public bool View



		/// <summary>
		/// Scheduler main loop. This function needs to get called on a regular basis.
		/// It will handle all scheduler tasks
		/// </summary>
		static public void Process()
		{
			if (m_eState!=State.Initialized) return;
			if (GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.Vmr9FPS<1f)
			{
				for (int i=0; i < m_tvcards.Count;++i)
				{
					TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
					dev.Process();
				}
			}

			TimeSpan ts=DateTime.Now-m_dtStart;
			if (ts.TotalMilliseconds<1000) return;
			Recorder.HandleRecordings();
			for (int i=0; i < m_tvcards.Count;++i)
			{
				TVCaptureDevice dev =(TVCaptureDevice)m_tvcards[i];
				dev.Process();
			}
			Recorder.SetProperties();
			m_dtStart=DateTime.Now;
		}//static public void Process()

		/// <summary>
		/// Updates the TV tags for the skin bases on the current tv channel, recording...
		/// </summary>
		/// <remarks>
		/// Tags updated are:
		/// #TV.View.channel, #TV.View.start,#TV.View.stop, #TV.View.genre, #TV.View.title, #TV.View.description
		/// #TV.Record.channel, #TV.Record.start,#TV.Record.stop, #TV.Record.genre, #TV.Record.title, #TV.Record.description, #TV.Record.thumb
		/// </remarks>
		static void SetProperties()
		{
			// for each tv-channel
			if (m_eState!=State.Initialized) return;

			for (int i=0; i < m_TVChannels.Count;++i)
			{
				TVChannel chan =(TVChannel)m_TVChannels[i];
				if (chan.Name.Equals(m_strTVChannel))
				{
					TVProgram prog=m_TVUtil.GetCurrentProgram(chan.Name);
					if (prog!=null)
					{
						if (!GUIPropertyManager.GetProperty("#TV.View.channel").Equals(m_strTVChannel))
						{
							OnTVChannelChanged();
						}
						GUIPropertyManager.SetProperty("#TV.View.start",prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						GUIPropertyManager.SetProperty("#TV.View.stop",prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						GUIPropertyManager.SetProperty("#TV.View.genre",prog.Genre);
						GUIPropertyManager.SetProperty("#TV.View.title",prog.Title);
						GUIPropertyManager.SetProperty("#TV.View.description",prog.Description);

					}
					else
					{
						if (!GUIPropertyManager.GetProperty("#TV.View.channel").Equals(m_strTVChannel))
						{
							OnTVChannelChanged();
						}
						GUIPropertyManager.SetProperty("#TV.View.start","");
						GUIPropertyManager.SetProperty("#TV.View.stop" ,"");
						GUIPropertyManager.SetProperty("#TV.View.genre","");
						GUIPropertyManager.SetProperty("#TV.View.title","");
						GUIPropertyManager.SetProperty("#TV.View.description","");
					}
					break;
				}//if (chan.Name.Equals(m_strTVChannel))
			}//for (int i=0; i < m_TVChannels.Count;++i)

			// handle properties...
			if (IsRecording())
			{
				TVRecording recording = CurrentTVRecording;
				TVProgram   program   = ProgramRecording;
				if (program==null)
				{
					string strLogo=Utils.GetCoverArt(TVChannelCovertArt,recording.Channel);
					if (!System.IO.File.Exists(strLogo))
					{
						strLogo="defaultVideoBig.png";
					}
					GUIPropertyManager.SetProperty("#TV.Record.thumb",strLogo);
					GUIPropertyManager.SetProperty("#TV.Record.start",recording.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
					GUIPropertyManager.SetProperty("#TV.Record.stop",recording.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
					GUIPropertyManager.SetProperty("#TV.Record.genre","");
					GUIPropertyManager.SetProperty("#TV.Record.title",recording.Title);
					GUIPropertyManager.SetProperty("#TV.Record.description","");
				}
				else
				{
					string strLogo=Utils.GetCoverArt(TVChannelCovertArt,program.Channel);
					if (!System.IO.File.Exists(strLogo))
					{
						strLogo="defaultVideoBig.png";
					}
					GUIPropertyManager.SetProperty("#TV.Record.thumb",strLogo);
					GUIPropertyManager.SetProperty("#TV.Record.channel",program.Channel);
					GUIPropertyManager.SetProperty("#TV.Record.start",program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
					GUIPropertyManager.SetProperty("#TV.Record.stop" ,program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
					GUIPropertyManager.SetProperty("#TV.Record.genre",program.Genre);
					GUIPropertyManager.SetProperty("#TV.Record.title",program.Title);
					GUIPropertyManager.SetProperty("#TV.Record.description",program.Description);
				}
			}
			else
			{
				GUIPropertyManager.SetProperty("#TV.Record.channel","");
				GUIPropertyManager.SetProperty("#TV.Record.start","");
				GUIPropertyManager.SetProperty("#TV.Record.stop" ,"");
				GUIPropertyManager.SetProperty("#TV.Record.genre","");
				GUIPropertyManager.SetProperty("#TV.Record.title","");
				GUIPropertyManager.SetProperty("#TV.Record.description","");
				GUIPropertyManager.SetProperty("#TV.Record.thumb"  ,"");
			}

			if (IsRecording())
			{
				TVProgram prog=ProgramRecording;
				DateTime dtStart,dtEnd,dtStarted;
				if (prog !=null)
				{
					dtStart=prog.StartTime;
					dtEnd=prog.EndTime;
					dtStarted=Recorder.TimeRecordingStarted;
					if (dtStarted<dtStart) dtStarted=dtStart;
					Recorder.SetProgressBarProperties(dtStart,dtStarted,dtEnd);
				}
				else 
				{
					TVRecording rec=CurrentTVRecording;
					if (rec!=null)
					{
						dtStart=rec.StartTime;
						dtEnd=rec.EndTime;
						dtStarted=Recorder.TimeRecordingStarted;
						if (dtStarted<dtStart) dtStarted=dtStart;
						Recorder.SetProgressBarProperties(dtStart,dtStarted,dtEnd);
					}
				}
			}
			else if (Recorder.View)
			{
				TVProgram prog=m_TVUtil.GetCurrentProgram(m_strTVChannel);
				if (prog!=null)
				{
					DateTime dtStart,dtEnd,dtStarted;
					dtStart=prog.StartTime;
					dtEnd=prog.EndTime;
					dtStarted=Recorder.TimeTimeshiftingStarted;
					if (dtStarted<dtStart) dtStarted=dtStart;
					Recorder.SetProgressBarProperties(dtStart,dtStarted,dtEnd);
				}
				else
				{
					// we dont have any tvguide data. 
					// so just suppose program started when timeshifting started and ends 2 hours after that
					DateTime dtStart,dtEnd,dtStarted;
					dtStart=Recorder.TimeTimeshiftingStarted;

					dtEnd=dtStart;
					dtEnd=dtEnd.AddHours(2);

					dtStarted=Recorder.TimeTimeshiftingStarted;
					if (dtStarted<dtStart) dtStarted=dtStart;
					Recorder.SetProgressBarProperties(dtStart,dtStarted,dtEnd);
				}
			}
		}//static void SetProperties()
    
		/// <summary>
		/// property which returns the date&time the recording was started
		/// </summary>
		static DateTime TimeRecordingStarted
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
		static DateTime TimeTimeshiftingStarted
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
		/// this method will update all tags for the tv progress bar
		/// </summary>
		static void SetProgressBarProperties(DateTime MovieStartTime,DateTime RecordingStarted, DateTime MovieEndTime)
		{
			TimeSpan tsMovieDuration = (MovieEndTime-MovieStartTime);
			float fMovieDurationInSecs=(float)tsMovieDuration.TotalSeconds;

			GUIPropertyManager.SetProperty("#TV.Record.duration",Utils.SecondsToShortHMSString((int)fMovieDurationInSecs));
      
			// get point where we started timeshifting/recording relative to the start of movie
			TimeSpan tsRecordingStart= (RecordingStarted-MovieStartTime)+new TimeSpan(0,0,0,(int)g_Player.ContentStart,0);
			float fRelativeRecordingStart=(float)tsRecordingStart.TotalSeconds;
			float percentRecStart = (fRelativeRecordingStart/fMovieDurationInSecs)*100.00f;
			int iPercentRecStart=(int)Math.Floor(percentRecStart);
			GUIPropertyManager.SetProperty("#TV.Record.percent1",iPercentRecStart.ToString());

			// get the point we're currently watching relative to the start of movie
			if (g_Player.Playing && g_Player.IsTV)
			{
				float fRelativeViewPoint=(float)g_Player.CurrentPosition+ fRelativeRecordingStart;
				float fPercentViewPoint = (fRelativeViewPoint / fMovieDurationInSecs)*100.00f;
				int iPercentViewPoint=(int)Math.Floor(fPercentViewPoint);
				GUIPropertyManager.SetProperty("#TV.Record.percent2",iPercentViewPoint.ToString());
				GUIPropertyManager.SetProperty("#TV.Record.current",Utils.SecondsToShortHMSString((int)fRelativeViewPoint));
			} 
			else
			{
				GUIPropertyManager.SetProperty("#TV.Record.percent2",iPercentRecStart.ToString());
				GUIPropertyManager.SetProperty("#TV.Record.current",Utils.SecondsToShortHMSString((int)fRelativeRecordingStart));
			}

			// get point the live program is now
			TimeSpan tsRelativeLivePoint= (DateTime.Now-MovieStartTime);
			float   fRelativeLiveSec=(float)tsRelativeLivePoint.TotalSeconds;
			float percentLive = (fRelativeLiveSec/fMovieDurationInSecs)*100.00f;
			int   iPercentLive=(int)Math.Floor(percentLive);
			GUIPropertyManager.SetProperty("#TV.Record.percent3",iPercentLive.ToString());
		}//static void SetProgressBarProperties(DateTime MovieStartTime,DateTime RecordingStarted, DateTime MovieEndTime)

		/// <summary>
		/// This function gets called by the TVDatabase when a recording has been
		/// added,changed or deleted. It forces the Scheduler to get update its list of
		/// recordings.
		/// </summary>
		static private void OnRecordingsChanged()
		{ 
			if (m_eState!=State.Initialized) return;
			m_bRecordingsChanged=true;
		}
		
		/// <summary>
		/// Empties/clears all tv related skin tags. Gets called during startup en shutdown of
		/// the scheduler
		/// </summary>
		static void CleanProperties()
		{
			GUIPropertyManager.SetProperty("#TV.View.channel","");
			GUIPropertyManager.SetProperty("#TV.View.thumb",  "");
			GUIPropertyManager.SetProperty("#TV.View.start","");
			GUIPropertyManager.SetProperty("#TV.View.stop", "");
			GUIPropertyManager.SetProperty("#TV.View.genre","");
			GUIPropertyManager.SetProperty("#TV.View.title","");
			GUIPropertyManager.SetProperty("#TV.View.description","");

			GUIPropertyManager.SetProperty("#TV.Record.channel","");
			GUIPropertyManager.SetProperty("#TV.Record.start","");
			GUIPropertyManager.SetProperty("#TV.Record.stop", "");
			GUIPropertyManager.SetProperty("#TV.Record.genre","");
			GUIPropertyManager.SetProperty("#TV.Record.title","");
			GUIPropertyManager.SetProperty("#TV.Record.description","");
			GUIPropertyManager.SetProperty("#TV.Record.thumb",  "");

			GUIPropertyManager.SetProperty("#TV.Record.percent1","");
			GUIPropertyManager.SetProperty("#TV.Record.percent2","");
			GUIPropertyManager.SetProperty("#TV.Record.percent3","");
			GUIPropertyManager.SetProperty("#TV.Record.duration","");
			GUIPropertyManager.SetProperty("#TV.Record.current","");
		}//static void CleanProperties()
		
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
		/// </remarks>
		static public void OnMessage(GUIMessage message)
		{
			if (message==null) return;
			switch(message.Message)
			{
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
							card.Stop();
						}
					}
					break;
			}//switch(message.Message)
		}//static public void OnMessage(GUIMessage message)

		/// <summary>
		/// This method will send a message to all 'external tuner control' plugins like USBUIRT
		/// to switch channel on the remote device
		/// </summary>
		/// <param name="strChannelName">name of channel</param>
		static void TuneExternalChannel(string strChannelName)
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
					return;
				}
			}
		}//static void TuneExternalChannel(string strChannelName)
    
		/// <summary>
		/// Returns the number of tv cards configured
		/// </summary>
		static public int Count
		{
			get { return m_tvcards.Count;}
		}

		static public TVCaptureDevice Get(int index)
		{
				return (TVCaptureDevice )m_tvcards[index];
		}
    
		/// <summary>
		/// returns the name of the current tv channel we're watching
		/// </summary>
		static public string TVChannelName
		{
			get { return m_strTVChannel;}
		}


		/// <summary>
		/// this method deleted any timeshifting files in the specified folder
		/// </summary>
		/// <param name="strPath">folder name</param>
		static void DeleteOldTimeShiftFiles(string strPath)
		{
			if (strPath==null) return;
			if (strPath==String.Empty) return;
			// Remove any trailing slashes
			strPath=Utils.RemoveTrailingSlash(strPath);

      
			// clean the TempDVR\ folder
			string strDir="";
			string[] strFiles;
			try
			{
				strDir=String.Format(@"{0}\TempDVR",strPath);
				strFiles=System.IO.Directory.GetFiles(strDir,"*.tmp");
				foreach (string strFile in strFiles)
				{
					try
					{
						System.IO.File.Delete(strFile);
					}
					catch(Exception){}
				}
			}
			catch(Exception){}

			// clean the TempSBE\ folder
			try
			{      
				strDir=String.Format(@"{0}\TempSBE",strPath);
				strFiles=System.IO.Directory.GetFiles(strDir,"*.tmp");
				foreach (string strFile in strFiles)
				{
					try
					{
						System.IO.File.Delete(strFile);
					}
					catch(Exception){}
				}
			}
			catch(Exception){}

			// delete *.tv
			try
			{      
				strDir=String.Format(@"{0}",strPath);
				strFiles=System.IO.Directory.GetFiles(strDir,"*.tv");
				foreach (string strFile in strFiles)
				{
					try
					{
						System.IO.File.Delete(strFile);
					}
					catch(Exception){}
				}
			}
			catch(Exception){}
		}//static void DeleteOldTimeShiftFiles(string strPath)
	}//public class Recorder
}//namespace MediaPortal.TV.Recording
