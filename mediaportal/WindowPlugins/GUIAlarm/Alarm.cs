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
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Radio.Database;

namespace MediaPortal.GUI.Alarm
{
	/// <summary>
	/// Alarm Class
	/// </summary>
	public class Alarm : IDisposable
	{
		#region Private Variables
			private static AlarmCollection _Alarms;
			private System.Windows.Forms.Timer _AlarmTimer = new System.Windows.Forms.Timer();
			private System.Windows.Forms.Timer _VolumeFadeTimer = new  System.Windows.Forms.Timer();
			private int _Id;
			private bool _Enabled;
			private string _Name;
			private DateTime _Time;
			private bool _Mon;
			private bool _Tue;
			private bool _Wed;
			private bool _Thu;
			private bool _Fri;
			private bool _Sat;
			private bool _Sun;
			private string _Sound;
			private MediaType _MediaType;
			private bool _VolumeFade;
			private GUIListItem _SelectedItem;
			private bool _Wakeup;
			private AlarmType _AlarmType;
			private string _Message;
			private int _RepeatCount;

			//constants
			private const int _MaxAlarms = 20;
		#endregion

		#region Public Enumerations
		public enum AlarmType
		{
			Once = 0,
			Recurring = 1
		}
		public enum MediaType
		{
			PlayList = 0,
			Radio = 1,
			File = 2,
			Message = 3
		}
		#endregion

		#region Constructor
		public Alarm(int id,string name,int mediaType, bool enabled,DateTime time,bool mon,bool tue,bool wed,bool thu,bool fri,bool sat,bool sun,string sound,bool volumeFade,bool wakeup,int alarmType,string message)
		{
			_Id = id;
			_Name = name;
			_MediaType = (MediaType)mediaType;
			_Enabled = enabled;
			_Time = time;
			_Mon = mon;
			_Tue = tue;
			_Wed = wed;
			_Thu = thu;
			_Fri = fri;
			_Sat = sat;
			_Sun = sun;
			_Sound = sound;
			_VolumeFade = volumeFade;
			_Wakeup = wakeup;
			_AlarmType = (AlarmType)alarmType;
			_Message = message;
			InitializeTimer();

		}

		public Alarm(int id)
		{
			_Id = id;
			_Name = GUILocalizeStrings.Get(869) + _Id.ToString();
			_Time=DateTime.Now;
		}
		#endregion

		#region Properties	
		public AlarmType AlarmOccurrenceType
		{
			get{return _AlarmType;}
			set{_AlarmType = value;}
		}
			
		public bool Wakeup
		{
			get{return _Wakeup;}
			set{_Wakeup = value;}
		}
			
		public string Name
		{
			get{return _Name;}
			set{_Name = value;}
		}
		/// <summary>
		/// Returns a string to display the days the alarm is enabled
		/// </summary>
		public string DaysEnabled
		{
			get
			{
				StringBuilder sb= new StringBuilder("-------");

				if(_Sun)
					sb.Replace("-","S",0,1);
				if(_Mon)
					sb.Replace("-","M",1,1);
				if(_Tue)
					sb.Replace("-","T",2,1);
				if(_Wed)
					sb.Replace("-","W",3,1);
				if(_Thu)
					sb.Replace("-","T",4,1);
				if(_Fri)
					sb.Replace("-","F",5,1);
				if(_Sat)
					sb.Replace("-","S",6,1);

				return sb.ToString();
			}

		}
		public MediaType AlarmMediaType
		{
			get{return _MediaType;}
			set{_MediaType = value;}
		}
		public bool Enabled
		{
			get{return _Enabled;}
			set
			{
				_Enabled = value;
				_AlarmTimer.Enabled = value;
			}
		}
		public DateTime Time
		{
			get{return _Time;}
			set{_Time = value;}
		}
		public string Sound
		{
			get{return _Sound;}
			set{_Sound = value;}
		}
		public int Id
		{
			get{return _Id;}
		}
		public bool Mon
		{
			get{return _Mon;}
			set{_Mon = value;}
		}
		public bool Tue
		{
			get{return _Tue;}
			set{_Tue = value;}
		}
		public bool Wed
		{
			get{return _Wed;}
			set{_Wed = value;}
		}
		public bool Thu
		{
			get{return _Thu;}
			set{_Thu = value;}
		}
		public bool Fri
		{
			get{return _Fri;}
			set{_Fri = value;}
		}
		public bool Sat
		{
			get{return _Sat;}
			set{_Sat = value;}
		}
		public bool Sun
		{
			get{return _Sun;}
			set{_Sun = value;}
		}
		public bool VolumeFade
		{
			get{return _VolumeFade;}
			set{_VolumeFade = value;}
		}
		public GUIListItem SelectedItem
		{
			get{return _SelectedItem;}
			set{_SelectedItem = value;}
		}	
		public string Message
		{
			get{return _Message;}
			set{_Message = value;}
		}
		#endregion

		#region Private Methods
		
		/// <summary>
		/// Initializes the timer object
		/// </summary>
		private void InitializeTimer()
		{
			_AlarmTimer.Tick += new EventHandler(OnTimer);
			_AlarmTimer.Interval = 1000; //second	
			_VolumeFadeTimer.Tick += new EventHandler(OnTimer);
			_VolumeFadeTimer.Interval = 3000; //3 seconds
		
			if(_Enabled)
				_AlarmTimer.Enabled = true;
		}

		/// <summary>
		/// Executes on the interval of the timer object.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimer(Object sender, EventArgs e)
		{
			if(sender == _AlarmTimer)
			{
				if(DateTime.Now.Hour == _Time.Hour && DateTime.Now.Minute == _Time.Minute)
				{
					if(_AlarmType == AlarmType.Recurring && IsDayEnabled() || _AlarmType == AlarmType.Once)
					{
						Log.Write("Alarm {0} fired at {1}",_Name,DateTime.Now);

						if (!GUIGraphicsContext.IsFullScreenVideo)
						{
							Play();
							//enable fade timer if selected
							if(_VolumeFade)
							{
								g_Player.Volume = 0;
								_VolumeFadeTimer.Enabled = true;
							}

							//activate the my alarm window. (handles snooze button)
							//TODO:Handle this globally somehow??
							//GUIWindowManager.ActivateWindow(GUIAlarm.WindowAlarm);			
						}
						//display the notify message
						DisplayNotifyMessage();		

						//disable the timer.
						_AlarmTimer.Enabled = false;
					}
					
				}
			}
			if(sender == _VolumeFadeTimer)
			{
				if(g_Player.Volume < 99)
				{
					g_Player.Volume +=1;

				}
				else
				{
					_VolumeFadeTimer.Enabled = false;
				}
			}
			
			
		}

		/// <summary>
		/// Displays the configured notification message.
		/// </summary>
		void DisplayNotifyMessage()
		{
			if(_Message.Length != 0)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY,0,0,0,0,0,0);
				msg.Label=string.Format("{0} - {1}", this.Name,this.Time.ToShortTimeString());
				msg.Label2= this.Message;
				msg.Label3= String.Format("{0}\\{1}",GUIGraphicsContext.Skin,"Media\\dialog_information.png");
				GUIGraphicsContext.SendMessage(msg);
			}
		}

		

		/// <summary>
		/// Checks if the current dayofweek for the alarm is enabled
		/// </summary>
		/// <returns>true if current dayofweek is enabled</returns>
		private bool IsDayEnabled()
		{
			switch(DateTime.Now.DayOfWeek)
			{
				case DayOfWeek.Monday:
					return _Mon;
				case DayOfWeek.Tuesday:
					return _Tue;
				case DayOfWeek.Wednesday:
					return _Wed;
				case DayOfWeek.Thursday:
					return _Thu;
				case DayOfWeek.Friday:
					return _Fri;
				case DayOfWeek.Saturday:
					return _Sat;
				case DayOfWeek.Sunday:
					return _Sun;
			}
			return false;
		}

		/// <summary>
		/// Returns if the day parameter is enabled for this alarm
		/// </summary>
		/// <param name="day">day to check</param>
		/// <returns>True if day passed in is enabled</returns>
		private bool IsDayEnabled(DayOfWeek day)
		{
			switch(day)
			{
				case DayOfWeek.Monday:
					return _Mon;
				case DayOfWeek.Tuesday:
					return _Tue;
				case DayOfWeek.Wednesday:
					return _Wed;
				case DayOfWeek.Thursday:
					return _Thu;
				case DayOfWeek.Friday:
					return _Fri;
				case DayOfWeek.Saturday:
					return _Sat;
				case DayOfWeek.Sunday:
					return _Sun;
			}
			return false;

		}

		/// <summary>
		/// Plays the selected media type
		/// </summary>
		private void Play()
		{
			switch(_MediaType)
			{
				case MediaType.PlayList:
					if(PlayListFactory.IsPlayList(_Sound))
					{
						PlayList playlist = PlayListFactory.Create(Alarm.PlayListPath + "\\" + _Sound);
						if(playlist==null) return;
						if(!playlist.Load(Alarm.PlayListPath + "\\" +  _Sound))
						{
							ShowErrorDialog();
							return;
						}
						if(playlist.Count == 1)
						{
							g_Player.Play(playlist[0].FileName);
							g_Player.Volume=99;
							return;
						}
						for(int i=0; i<playlist.Count; ++i)
						{
							PlayList.PlayListItem playListItem = playlist[i];
							PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Add(playListItem);
						}
						if(PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Count>0)
						{
							PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
							PlayListPlayer.Reset();
						
							PlayListPlayer.Play(0);
							g_Player.Volume=99;
						
						}
					}
					else
					{
						ShowErrorDialog();
					}
					break;
				case MediaType.Radio:
					ArrayList stations = new ArrayList();
					RadioDatabase.GetStations(ref stations);
					foreach (RadioStation station in stations)
					{
						if(station.Name == _Sound)
						{ 	
							if (station.URL.Length<5)
							{
								// FM radio
								GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO, (int)GUIWindow.Window.WINDOW_RADIO, 0, 0, 0, 0, null);
								msg.Label = station.Name;
								GUIGraphicsContext.SendMessage(msg);
							}
							else
							{
								// internet radio stream
								g_Player.PlayAudioStream(station.URL);
							}
							break;
						}
					}
					break;
				case MediaType.File:
					if(Alarm.AlarmSoundPath.Length != 0 &&  _Sound.Length!=0)
					{
						try
						{
							_RepeatCount= 0;
							g_Player.Play(Alarm.AlarmSoundPath + "\\" +  _Sound);
							g_Player.Volume=99;
							
							//add playback end handler if file <= repeat seconds in configuration
							if(g_Player.Duration <= Alarm.RepeatSeconds)
								g_Player.PlayBackEnded += new MediaPortal.Player.g_Player.EndedHandler(g_Player_PlayBackEnded);
						
						}
						catch(System.Runtime.InteropServices.COMException)
						{
							ShowErrorDialog();
						}
					}
					else
					{
						ShowErrorDialog();
					}
					
				
					break;
				case MediaType.Message:
					//do not play any media, message only
					break;
			}

		}

		/// <summary>
		/// Shows the Error Dialog
		/// </summary>
		private void ShowErrorDialog()
		{
			GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			if(dlgOK !=null)
			{
				dlgOK.SetHeading(6);
				dlgOK.SetLine(1,477);
				dlgOK.SetLine(2,"");
				dlgOK.DoModal(GUIAlarm.WindowAlarm);
			}
			return;
		}

		/// <summary>
		/// Handles the playback ended event to loop the sound file if necessary
		/// </summary>
		/// <param name="type"></param>
		/// <param name="filename"></param>
		private void g_Player_PlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
		{
			//play file again, increment loop counter
			if(_RepeatCount <= RepeatCount)
			{
				g_Player.Play(Alarm.AlarmSoundPath + "\\" +  _Sound);
				_RepeatCount +=1;
			}

		}
		#endregion

		#region IDisposable Members
			public void Dispose()
			{
				_AlarmTimer.Enabled=false;
				_AlarmTimer.Dispose();
				_VolumeFadeTimer.Dispose();
			} 
		#endregion

		#region Static Methods

		/// <summary>
		/// Loads all of the alarms from the profile xml
		/// </summary>
		/// <returns>ArrayList of Alarm Objects</returns>
		public static void LoadAll()
		{
			AlarmCollection Alarms = new AlarmCollection();

			using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				for (int i=0; i < _MaxAlarms; i++)
				{
					string NameTag=String.Format("alarmName{0}",i);
					string MediaTypeTag=String.Format("alarmMediaType{0}",i);
					string TimeTag=String.Format("alarmTime{0}",i);
					string EnabledTag=String.Format("alarmEnabled{0}",i);
					string MonTag =  String.Format("alarmMon{0}",i);
					string TueTag =  String.Format("alarmTue{0}",i);
					string WedTag =  String.Format("alarmWed{0}",i);
					string ThuTag =  String.Format("alarmThu{0}",i);
					string FriTag =  String.Format("alarmFri{0}",i);
					string SatTag =  String.Format("alarmSat{0}",i);
					string SunTag =  String.Format("alarmSun{0}",i);
					string SoundTag =  String.Format("alarmSound{0}",i);
					string VolumeFadeTag = String.Format("alarmVolumeFade{0}",i);
					string WakeUpPCTag = String.Format("alarmWakeUpPC{0}",i);
					string AlarmTypeTag = String.Format("alarmType{0}",i);
					string MessageTag = String.Format("alarmMessage{0}",i);

					string AlarmName=xmlreader.GetValueAsString("alarm",NameTag,"");

					if (AlarmName.Length>0)
					{
						bool AlarmEnabled=xmlreader.GetValueAsBool("alarm",EnabledTag,false);
						int AlarmMediaType =xmlreader.GetValueAsInt("alarm",MediaTypeTag,1);
						DateTime AlarmTime = DateTime.Parse(xmlreader.GetValueAsString("alarm",TimeTag,string.Empty));
						bool AlarmMon = xmlreader.GetValueAsBool("alarm",MonTag,false);
						bool AlarmTue = xmlreader.GetValueAsBool("alarm",TueTag,false);
						bool AlarmWed = xmlreader.GetValueAsBool("alarm",WedTag,false);
						bool AlarmThu = xmlreader.GetValueAsBool("alarm",ThuTag,false);
						bool AlarmFri = xmlreader.GetValueAsBool("alarm",FriTag,false);
						bool AlarmSat = xmlreader.GetValueAsBool("alarm",SatTag,false);
						bool AlarmSun = xmlreader.GetValueAsBool("alarm",SunTag,false);
						string AlarmSound = xmlreader.GetValueAsString("alarm",SoundTag,string.Empty);
						bool AlarmVolumeFade = xmlreader.GetValueAsBool("alarm",VolumeFadeTag,false);
						bool WakeUpPC = xmlreader.GetValueAsBool("alarm",WakeUpPCTag,false);
						int AlarmType = xmlreader.GetValueAsInt("alarm",AlarmTypeTag,1);
						string Message = xmlreader.GetValueAsString("alarm",MessageTag,string.Empty);

								
						Alarm objAlarm = new Alarm(i,AlarmName,AlarmMediaType,AlarmEnabled,AlarmTime,
							AlarmMon,AlarmTue,AlarmWed,AlarmThu,
							AlarmFri,AlarmSat,AlarmSun,AlarmSound,AlarmVolumeFade,WakeUpPC,AlarmType,Message);

						Alarms.Add(objAlarm);
					}
				}	
			}
			_Alarms = Alarms;

		}
		/// <summary>
		/// Saves an alarm to the configuration file
		/// </summary>
		/// <param name="alarmToSave">Alarm object to save</param>
		/// <returns></returns>
		public static bool SaveAlarm(Alarm alarmToSave)
		{
			int id = alarmToSave.Id;
				
			using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
					
				xmlwriter.SetValue("alarm","alarmName"+id,alarmToSave.Name);
				xmlwriter.SetValue("alarm","alarmMediaType"+id,(int)alarmToSave.AlarmMediaType);
				xmlwriter.SetValueAsBool("alarm","alarmEnabled"+id,alarmToSave.Enabled);
				xmlwriter.SetValue("alarm","alarmTime"+id,alarmToSave.Time);
				xmlwriter.SetValueAsBool("alarm","alarmMon"+id,alarmToSave.Mon);   
				xmlwriter.SetValueAsBool("alarm","alarmTue"+id,alarmToSave.Tue);   
				xmlwriter.SetValueAsBool("alarm","alarmWed"+id,alarmToSave.Wed);   
				xmlwriter.SetValueAsBool("alarm","alarmThu"+id,alarmToSave.Thu);   
				xmlwriter.SetValueAsBool("alarm","alarmFri"+id,alarmToSave.Fri);   
				xmlwriter.SetValueAsBool("alarm","alarmSat"+id,alarmToSave.Sat); 
				xmlwriter.SetValueAsBool("alarm","alarmSun"+id,alarmToSave.Sun); 
				xmlwriter.SetValue("alarm","alarmSound"+id,alarmToSave.Sound);
				xmlwriter.SetValueAsBool("alarm","alarmVolumeFade"+id,alarmToSave.VolumeFade); 
				xmlwriter.SetValueAsBool("alarm","alarmWakeUpPC"+id,alarmToSave.Wakeup); 
				xmlwriter.SetValue("alarm","alarmType"+id,(int)alarmToSave.AlarmOccurrenceType);
				xmlwriter.SetValue("alarm","alarmMessage"+id,alarmToSave.Message);
			}
			return true;
		
				
			
		}

		/// <summary>
		/// Deletes an alarm from the configuration file
		/// </summary>
		/// <param name="id">Id of alarm to be deleted</param>
		/// <returns>true if suceeded</returns>
		public static bool DeleteAlarm(int id)
		{
			using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.RemoveEntry("alarm","alarmName"+id);
				xmlwriter.RemoveEntry("alarm","alarmEnabled"+id);
				xmlwriter.RemoveEntry("alarm","alarmTime"+id);
				xmlwriter.RemoveEntry("alarm","alarmMon"+id);   
				xmlwriter.RemoveEntry("alarm","alarmTue"+id);   
				xmlwriter.RemoveEntry("alarm","alarmWed"+id);   
				xmlwriter.RemoveEntry("alarm","alarmThu"+id);   
				xmlwriter.RemoveEntry("alarm","alarmFri"+id);   
				xmlwriter.RemoveEntry("alarm","alarmSat"+id); 
				xmlwriter.RemoveEntry("alarm","alarmSun"+id); 
				xmlwriter.RemoveEntry("alarm","alarmSound"+id);
				xmlwriter.RemoveEntry("alarm","alarmMediaType"+id);
				xmlwriter.RemoveEntry("alarm","alarmVolumeFade"+id);
				xmlwriter.RemoveEntry("alarm","alarmWakeUpPC"+id);
				xmlwriter.RemoveEntry("alarm","alarmType"+id);
				xmlwriter.RemoveEntry("alarm","alarmMessage"+id);
			}
			return true;
		} 

		/// <summary>
		/// Gets the next black Id for a new alarm
		/// </summary>
		/// <returns>Integer Id</returns>
		public static int GetNextId
		{
			get
			{
				string tempText;
				for (int i=0; i < _MaxAlarms; i++)
				{
					using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
					{
						tempText = xmlreader.GetValueAsString("alarm","alarmName"+i,"");
						if (tempText.Length == 0)
						{
							return i;
						}
					}	
				}
				return -1;
			}
		}

		/// <summary>
		/// Gets the icon based on the current alarm media type
		/// </summary>
		public string GetIcon
		{
			get
			{
				switch(_MediaType)
				{
					case MediaType.File:
						return "defaultAudio.png";
					case MediaType.PlayList:
						return "DefaultPlaylist.png";
					case MediaType.Radio:
					{
						string thumb=Utils.GetCoverArt(Thumbs.Radio,this.Sound);
						if (thumb.Length != 0) return thumb;
						return "DefaultMyradio.png";	
					}
					case MediaType.Message:
					{
						return "dialog_information.png";
					}	
				}
				return string.Empty;
			}
		}
			

		/// <summary>
		/// Refreshes the loaded alarms from the config file
		/// </summary>
		public static void RefreshAlarms()
		{
			if(_Alarms != null)
			{
				foreach(Alarm a in _Alarms)
				{
					a.Dispose();
				}
				_Alarms.Clear();
			
				//Load all the alarms 
				Alarm.LoadAll();
			}
		}
			
		#endregion

		#region Static Properties
		/// <summary>
		/// Gets / Sets the loaded alarms
		/// </summary>
		public static AlarmCollection LoadedAlarms  
		{
			get{return _Alarms;}
		}
		/// <summary>
		/// Gets the alarms sound path from the configuration file
		/// </summary>
		public static string AlarmSoundPath
		{
			get
			{ 
				using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					return  Utils.RemoveTrailingSlash(xmlreader.GetValueAsString("alarm","alarmSoundsFolder",""));
				}
			}
		}
		/// <summary>
		/// Gets the playlist path from the configuration file
		/// </summary>
		public static string PlayListPath
		{

			get
			{ 
				using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					return  Utils.RemoveTrailingSlash(xmlreader.GetValueAsString("music","playlists",""));
				}
			}
		}
		/// <summary>
		/// Gets the snooze time from the configuration file
		/// </summary>
		public static int SnoozeTime
		{
			get
			{ 
				using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					return xmlreader.GetValueAsInt("alarm","alarmSnoozeTime",5);
				}
			}
		}

		/// <summary>
		/// Gets the configured message display length
		/// </summary>
		public static int MessageDisplayLength
		{
			get
			{ 
				using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					return xmlreader.GetValueAsInt("alarm","alarmMessageDisplayLength",10);
				}
			}
		}

		/// <summary>
		/// Gets the configured duration to qualify to repeat the playing file
		/// </summary>
		public static int RepeatSeconds
		{
			get
			{ 
				using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					return xmlreader.GetValueAsInt("alarm","alarmRepeatSeconds",120);
				}
			}
		}

		/// <summary>
		/// Gets the configured count to repeat the file
		/// </summary>
		public static int RepeatCount
		{
			get
			{ 
				using(MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					return xmlreader.GetValueAsInt("alarm","alarmRepeatCount",5);
				}
			}
		}
		
		#endregion

		#region PowerScheduler Interface Implementation
			/// <summary>
			/// Powersheduler implimentation, returns true if the plugin can allow hibernation
			/// </summary>
			public bool CanHibernate
			{
				get
				{
					if(!GUIGraphicsContext.IsFullScreenVideo || !GUIGraphicsContext.IsPlaying)
					{
						return true;
					}
					else
					{
						return false;
					}
				}		
			}


			/// <summary>
			/// Gets the DateTime for the next active alarm to wake up the pc.
			/// </summary>
			/// <param name="alarms">ArrayList of loaded alarms</param>
			/// <returns>alarm object</returns>
			public static DateTime GetNextAlarmDateTime(DateTime earliestStartTime)
			{	
				if (_Alarms==null) return new DateTime(2100,1,1,1,0,0,0);
				//timespan to search.
				DateTime NextStartTime = new DateTime();//=  DateTime.Now.AddMonths(1);
				DateTime tmpNextStartTime = new DateTime();
								
				foreach(Alarm a in _Alarms)
				{
					//alarm must be enabled and set to wake up the pc.
					if(a.Enabled && a.Wakeup)
					{	
						switch(a.AlarmOccurrenceType)
						{
							case AlarmType.Once:
								tmpNextStartTime = a.Time;
								break;
							case AlarmType.Recurring:
								//check if alarm has passed
								if(a.Time.Ticks < DateTime.Now.Ticks)
								{
									//alarm has passed, loop through the next 7 days to 
									//find the next enabled day for the alarm
									for(int i=1; i < 8; i++)
									{
										DateTime DateToCheck = DateTime.Now.AddDays(i);

										if(a.IsDayEnabled(DateToCheck.DayOfWeek))
										{
											//found next enabled day
											tmpNextStartTime = DateToCheck;	
											break;
										}
									}
								}
								else
								{
									//alarm has not passed
									tmpNextStartTime = a.Time;
								}
								break;
						}

						if (tmpNextStartTime.Ticks > earliestStartTime.Ticks)
						{
							NextStartTime = new DateTime(tmpNextStartTime.Ticks);
						}
					}
						
				}
					
				return NextStartTime;
			}

			
		#endregion

	
	}
}
