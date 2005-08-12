using System;
using System.Collections;
using System.Runtime.InteropServices;
using DShowNET;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Summary description for EpgGrabber.
	/// </summary>
	public class EpgGrabber
	{
		class CachedChannel
		{
			public TVChannel chan;
			public int			 serviceId;
			public int			 transportId;
			public long			 lastProgramTime;
		}
		enum State
		{
			Idle,
			Grabbing,
			Parsing
		}
		#region variables
		IEPGGrabber			epgInterface=null;
		IATSCGrabber		atscInterface=null;
		IMHWGrabber			mhwInterface=null;
		IStreamAnalyzer analyzerInterface=null;
		NetworkType			networkType;
		bool						grabEPG=false;
		
		int							epgChannel=0;
		uint						mhwEvent=0;
		State						currentState=State.Idle;
		ArrayList       mhwChannelCache;
		#endregion

		#region properties
		public IEPGGrabber EPGInterface
		{
			get { return epgInterface;}
			set { epgInterface=value;}
		}
		public IStreamAnalyzer AnalyzerInterface
		{
			get { return analyzerInterface;}
			set { analyzerInterface=value;}
		}
		public IMHWGrabber MHWInterface
		{
			get { return mhwInterface;}
			set { mhwInterface=value;}
		}
		public IATSCGrabber ATSCInterface
		{
			get { return atscInterface;}
			set { atscInterface=value;}
		}
		public NetworkType Network
		{
			get { return networkType;}
			set { networkType=value;}
		}
		#endregion

		#region public methods
		
		public void GrabEPG(bool epg)
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				bool enabled = xmlreader.GetValueAsBool("xmltv", "epgdvb", true);
				if (!enabled) return;
			}

			grabEPG=epg;
			if (Network==NetworkType.ATSC)
			{
				if (ATSCInterface!=null)
				{
					currentState=State.Grabbing;
					Log.WriteFile(Log.LogType.EPG,"epg-grab: start ATSC grabber");
					ATSCInterface.GrabATSC();
				}
			}
			else
			{
				if (grabEPG)
				{
					currentState=State.Grabbing;
					epgChannel=0;
					Log.WriteFile(Log.LogType.EPG,"epg-grab: start EPG grabber");
					if (EPGInterface!=null)
						EPGInterface.GrabEPG();
				}
				else
				{
					mhwEvent=0;
					mhwChannelCache=new ArrayList();
					currentState=State.Grabbing;
					Log.WriteFile(Log.LogType.EPG,"epg-grab: start MHW grabber");
					if (MHWInterface!=null)
						MHWInterface.GrabMHW();
				}
			}
			
		}
		public void Process()
		{
			bool ready=false;
			switch (currentState)
			{
				case State.Grabbing:
					if (Network==NetworkType.ATSC)
					{
						if (ATSCInterface!=null )
						{
							ATSCInterface.IsATSCReady(out ready);
							if (ready) currentState=State.Parsing;
						}
					}
					else if (EPGInterface!=null && grabEPG)
					{
						EPGInterface.IsEPGReady(out ready);
						if (ready) currentState=State.Parsing;
					}
					else if (MHWInterface!=null && !grabEPG)
					{
						MHWInterface.IsMHWReady(out ready);
						if (ready) currentState=State.Parsing;
					}
				break;

				case State.Parsing:
					if (Network==NetworkType.ATSC)
					{
						ParseATSC();
						Log.WriteFile(Log.LogType.EPG,"epg-grab: ATSC done");
						currentState=State.Idle;
					}
					else if (grabEPG)
					{
						ParseEPG();
					}
					else
					{
						ParseMHW();
					}
				break;
			}
		}
		#endregion

		#region private methods

		#region ATSC
		void ParseATSC()
		{
			if (AnalyzerInterface==null) return;
			try
			{
				Log.WriteFile(Log.LogType.EPG,"atsc-epg: atsc ready");
				ushort titleCount;
				ATSCInterface.GetATSCTitleCount(out titleCount);
				Log.WriteFile(Log.LogType.EPG,"atsc-epg: atsc titles:{0}",titleCount);
				if (titleCount<=0) return;
				for (short i=0; i < titleCount; ++i)
				{
					Int16 source_id, length_in_mins;
					uint starttime;
					IntPtr ptrTitle,ptrDescription;
					ATSCInterface.GetATSCTitle(i,out source_id, out starttime,out length_in_mins, out ptrTitle,out ptrDescription);
					string title,description;
					title=Marshal.PtrToStringAnsi(ptrTitle);
					description=Marshal.PtrToStringAnsi(ptrDescription);
					if (title==null) title="";
					if (description==null) description="";
					title=title.Trim();
					description=description.Trim();

					if (title.Length==0) continue;

					// get channel info
					DVBSections.ChannelInfo chi=new MediaPortal.TV.Recording.DVBSections.ChannelInfo();
					DVBSections sections = new DVBSections();
					UInt16 len=0;
					int hr=0;
					hr=AnalyzerInterface.GetCISize(ref len);					
					IntPtr mmch=Marshal.AllocCoTaskMem(len);
					hr=AnalyzerInterface.GetChannel((UInt16)source_id,mmch);
					chi=sections.GetChannelInfo(mmch);
					Marshal.FreeCoTaskMem(mmch);


					// find channel in tvdatabase
					ArrayList channels = new ArrayList();
					TVDatabase.GetChannels(ref channels);
					foreach (TVChannel chan in channels)
					{
						int symbolrate=0,innerFec=0,modulation=0,physicalChannel=0;
						int minorChannel=0,majorChannel=0; 
						int frequency=-1,ONID=-1,TSID=-1,SID=-1;
						int audioPid=-1, videoPid=-1, teletextPid=-1, pmtPid=-1, pcrPid=-1;
						string providerName;
						int audio1,audio2,audio3,ac3Pid;
						string audioLanguage,audioLanguage1,audioLanguage2,audioLanguage3;
						bool HasEITPresentFollow,HasEITSchedule;
						TVDatabase.GetATSCTuneRequest(chan.ID,out physicalChannel,out providerName,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3, out minorChannel,out majorChannel,out HasEITPresentFollow,out HasEITSchedule,out pcrPid);
						if (minorChannel!=chi.minorChannel || majorChannel!=chi.majorChannel) continue;
						
						//got tv channel, now calculate start time
						DateTime programStartTimeUTC = new DateTime(1980,1,6,0,0,0,0);
						programStartTimeUTC.AddSeconds(starttime);
						DateTime programStartTime=programStartTimeUTC.ToLocalTime();

						//add epg event to database
						TVProgram tv=new TVProgram();
						tv.Start=Util.Utils.datetolong(programStartTime);
						tv.End=Util.Utils.datetolong(programStartTime.AddMinutes(length_in_mins));
						tv.Channel=chan.Name;
						tv.Genre=String.Empty;
						tv.Title=title;
						tv.Description=description;
						if(tv.Title==null)
							tv.Title=String.Empty;

						if(tv.Description==null)
							tv.Description=String.Empty;

						if(tv.Description==String.Empty)
							tv.Description=title;

						if(tv.Title==String.Empty)
							tv.Title=title;

						if(tv.Title==String.Empty || tv.Title=="n.a.") 
						{
							continue;
						}

						Log.WriteFile(Log.LogType.EPG,"atsc-grab: {0} {1}-{2} {3}", tv.Channel,tv.Start,tv.End,tv.Title);
						ArrayList programsInDatabase = new ArrayList();
						TVDatabase.GetProgramsPerChannel(tv.Channel,tv.Start+1,tv.End-1,ref programsInDatabase);
						if(programsInDatabase.Count==0)
						{
							TVDatabase.AddProgram(tv);
						}
					}
				}
				Log.WriteFile(Log.LogType.EPG,"atsc-epg: atsc done");
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Error,true,"atsc-epg: Exception while parsing atsc:{0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
		}
		#endregion

		#region MHW
		void ParseMHW()
		{
			try
			{
				short titleCount;
				MHWInterface.GetMHWTitleCount(out titleCount);
				if (mhwEvent==0)
				{
					Log.WriteFile(Log.LogType.EPG,"mhw-epg: mhw ready");
					Log.WriteFile(Log.LogType.EPG,"mhw-epg: mhw titles:{0}",titleCount);

					if (titleCount<=0) 
					{
						mhwChannelCache=null;
						currentState=State.Idle;
						return;
					}

					//cache all channels...
					ArrayList channels = new ArrayList();
					TVDatabase.GetChannels(ref channels);
					int freq, symbolrate,innerFec,modulation, ONID, TSID, SID,pcrPid;
					int audioPid, videoPid, teletextPid, pmtPid,bandWidth;
					int audio1, audio2, audio3, ac3Pid;
					string audioLanguage,  audioLanguage1, audioLanguage2, audioLanguage3;
					bool HasEITPresentFollow,HasEITSchedule;
					string provider="";
					foreach (TVChannel chan in channels)
					{
						switch (Network)
						{
							case NetworkType.DVBC:
								TVDatabase.GetDVBCTuneRequest(chan.ID,out provider,out freq, out symbolrate,out innerFec,out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3,out HasEITPresentFollow,out HasEITSchedule,out pcrPid);
								if (SID>=0 && TSID>=0)
								{
									TVProgram lastProg=TVDatabase.GetLastProgramForChannel(chan);
									CachedChannel cachedCh = new CachedChannel();
									cachedCh.serviceId=SID;
									cachedCh.transportId=TSID;
									cachedCh.chan=chan;
									cachedCh.lastProgramTime=lastProg.End;
									mhwChannelCache.Add(cachedCh);
								}
								break;
								case NetworkType.DVBS:
								{
									DVBChannel ch=new DVBChannel();
									if(TVDatabase.GetSatChannel(chan.ID,1,ref ch)==true)//only television
									{
										TVProgram lastProg=TVDatabase.GetLastProgramForChannel(chan);
										CachedChannel cachedCh = new CachedChannel();
										cachedCh.serviceId=ch.ProgramNumber;
										cachedCh.transportId=ch.TransportStreamID;
										cachedCh.chan=chan;
										cachedCh.lastProgramTime=lastProg.End;
										mhwChannelCache.Add(cachedCh);

									}
								}
								break;
							case NetworkType.DVBT:
								TVDatabase.GetDVBTTuneRequest(chan.ID,out provider,out freq, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3,out HasEITPresentFollow,out HasEITSchedule,out pcrPid);
								if (SID>=0 && TSID>=0)
								{
									TVProgram lastProg=TVDatabase.GetLastProgramForChannel(chan);
									CachedChannel cachedCh = new CachedChannel();
									cachedCh.serviceId=SID;
									cachedCh.transportId=TSID;
									cachedCh.chan=chan;
									cachedCh.lastProgramTime=lastProg.End;
									mhwChannelCache.Add(cachedCh);
								}
								break;
						}
					}//foreach (TVChannel chan in channels)
				}

				if (titleCount<=0) 
				{
					mhwChannelCache=null;
					currentState=State.Idle;
					return;
				}

				for (short i=0; i < 100; ++i)
				{
					if (mhwEvent>=titleCount)
					{
						Log.WriteFile(Log.LogType.EPG,"epg-grab: MHW done");
						currentState=State.Idle;
						mhwChannelCache=null;
						return;
					}
					short id=0,transportid=0, networkid=0, channelnr=0, channelid=0, programid=0, themeid=0, PPV=0,duration=0;
					byte summaries=0;
					uint datestart=0,timestart=0; 
					IntPtr ptrTitle,ptrProgramName;
					IntPtr ptrChannelName,ptrSummary, ptrTheme;
					MHWInterface.GetMHWTitle((short)mhwEvent,ref id, ref transportid, ref networkid, ref channelnr, ref programid, ref themeid, ref PPV, ref summaries, ref duration, ref datestart,ref timestart, out ptrTitle,out ptrProgramName);
					MHWInterface.GetMHWChannel(channelnr,ref channelid,ref networkid, ref transportid,out ptrChannelName);
					MHWInterface.GetMHWSummary(programid, out ptrSummary);
					MHWInterface.GetMHWTheme(themeid, out ptrTheme);

					mhwEvent++;
					string channelName,title,programName,summary,theme;
					channelName=Marshal.PtrToStringAnsi(ptrChannelName);
					title=Marshal.PtrToStringAnsi(ptrTitle);
					programName=Marshal.PtrToStringAnsi(ptrProgramName);
					summary=Marshal.PtrToStringAnsi(ptrSummary);
					theme=Marshal.PtrToStringAnsi(ptrTheme);

					if (channelName==null) channelName="";
					if (title==null) title="";
					if (programName==null) programName="";
					if (summary==null) summary="";
					if (theme==null) theme="";
					channelName=channelName.Trim();
					title=title.Trim();
					programName=programName.Trim();
					summary=summary.Trim();
					theme=theme.Trim();

					uint d1=datestart;
					uint m=timestart&0xff;
					uint h1=(timestart>>16)&0xff;
					DateTime programStartTime=new DateTime(System.DateTime.Now.Ticks);
					DateTime dayStart=new DateTime(System.DateTime.Now.Ticks);
					dayStart=dayStart.Subtract(new TimeSpan(1,dayStart.Hour,dayStart.Minute,dayStart.Second,dayStart.Millisecond));
					int day=(int)dayStart.DayOfWeek;
				
					programStartTime=dayStart;
					int minVal=(int)((d1-day)*86400+h1*3600+m*60);
					if(minVal<21600)
						minVal+=604800;

					programStartTime=programStartTime.AddSeconds(minVal);
					
					bool foundChannel=false;
					long lastProgramDate=0;
					foreach (CachedChannel ch in mhwChannelCache)
					{
						if (ch.serviceId==channelid && ch.transportId==transportid)
						{
							foundChannel=true;
							channelName=ch.chan.Name;
							lastProgramDate=ch.lastProgramTime;
							break;
						}
					}
					if (!foundChannel) 
					{
						Log.WriteFile(Log.LogType.EPG,"mhw-epg: unknown channel cid:{0:X} tsid:{1:X} ONID:{2:X}",channelid,transportid,networkid);
						continue;
					}

					TVProgram tv=new TVProgram();
					tv.Start=Util.Utils.datetolong(programStartTime);
					tv.End=Util.Utils.datetolong(programStartTime.AddMinutes(duration));
					tv.Channel=channelName;
					tv.Genre=theme;
					tv.Title=title;
					tv.Description=summary;
					if(tv.Title==null)
						tv.Title=String.Empty;

					if(tv.Description==null)
						tv.Description=String.Empty;

					if(tv.Description==String.Empty)
						tv.Description=title;

					if(tv.Title==String.Empty)
						tv.Title=title;

					if(tv.Title==String.Empty || tv.Title=="n.a.") 
					{
						continue;
					}

					if(lastProgramDate <= tv.Start)
					{
						Log.WriteFile(Log.LogType.EPG,"mhw-grab: {0} {1}-{2} {3}", tv.Channel,tv.Start,tv.End,tv.Title);
						TVDatabase.AddProgram(tv);
					}
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Error,true,"mhw-epg: Exception while parsing MHW:{0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
		}
		#endregion

		#region EPG
		int getUTC(int val)
		{
			if ((val&0xF0)>=0xA0)
				return 0;
			if ((val&0xF)>=0xA)
				return 0;
			return ((val&0xF0)>>4)*10+(val&0xF);
		}


		void ParseEPG()
		{
			try
			{
				string m_languagesToGrab=String.Empty;
				using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					m_languagesToGrab=xmlreader.GetValueAsString("epg-grabbing","grabLanguages","");
				}
				uint channelCount=0;
				EPGInterface.GetEPGChannelCount(out channelCount);
				if (epgChannel >=channelCount)
				{
					Log.WriteFile(Log.LogType.EPG,"epg-grab: EPG done");
					currentState=State.Idle;
					return;
				}
				if (epgChannel==0)
					Log.WriteFile(Log.LogType.EPG,"epg-grab: received epg for {0} channels", channelCount);
				ushort networkid=0;
				ushort transportid=0;
				ushort serviceid=0;
				uint eventCount=0;
				string provider;
				EPGInterface.GetEPGChannel((uint)epgChannel,ref networkid, ref transportid, ref serviceid);
				TVChannel channel=TVDatabase.GetTVChannelByStream(Network==NetworkType.ATSC,Network==NetworkType.DVBT,Network==NetworkType.DVBC,Network==NetworkType.DVBS,networkid,transportid,serviceid, out provider);
				if (channel==null) 
				{
					Log.WriteFile(Log.LogType.EPG,"epg-grab: Unknown channel NetworkId:{0} transportid:{1} serviceid:{2}",networkid,transportid,serviceid);
					epgChannel++;
					return;
				}
				TVProgram lastProgram=TVDatabase.GetLastProgramForChannel(channel);
				Log.WriteFile(Log.LogType.EPG,"epg-grab: Last program in database for {0} :{1}-{2}",
											channel.Name,lastProgram.Start,lastProgram.End);
				int curChannel=epgChannel;
				epgChannel++;
				EPGInterface.GetEPGEventCount((uint)curChannel,out eventCount);
				for (int i=0; i < eventCount;++i)
				{
					uint start_time_MJD=0,start_time_UTC=0,duration=0,languageId=0;
					string title,description,genre;
					IntPtr ptrTitle=IntPtr.Zero;
					IntPtr ptrDesc=IntPtr.Zero;
					IntPtr ptrGenre=IntPtr.Zero;
					EPGInterface.GetEPGEvent((uint)curChannel,(uint)i,out languageId,out start_time_MJD, out start_time_UTC, out duration,out ptrTitle,out ptrDesc, out ptrGenre);
					title=Marshal.PtrToStringAnsi(ptrTitle);
					description=Marshal.PtrToStringAnsi(ptrDesc);
					genre=Marshal.PtrToStringAnsi(ptrGenre);
					string language  = String.Empty;
					language += (char)((languageId>>16)&0xff);
					language += (char)((languageId>>8)&0xff);
					language += (char)((languageId)&0xff);
					bool grabLanguage=false;
					if(m_languagesToGrab!="")
					{
						string[] langs=m_languagesToGrab.Split(new char[]{'/'});
						foreach(string lang in langs)
						{
							if(lang==String.Empty) continue;
							if (language==lang) grabLanguage=true;
							if (language==String.Empty) grabLanguage=true;
						}
					}
					else grabLanguage=true;


					int duration_hh = getUTC((int) ((duration >> 16) )& 255);
					int duration_mm = getUTC((int) ((duration >> 8) )& 255);
					int duration_ss = 0;//getUTC((int) (duration )& 255);
					int starttime_hh = getUTC((int) ((start_time_UTC >> 16) )& 255);
					int starttime_mm =getUTC((int) ((start_time_UTC >> 8) )& 255);
					int starttime_ss =0;//getUTC((int) (start_time_UTC )& 255);

					if (starttime_hh>23) starttime_hh=23;
					if (starttime_mm>59) starttime_mm=59;
					if (starttime_ss>59) starttime_ss=59;

					if (duration_hh>23) duration_hh=23;
					if (duration_mm>59) duration_mm=59;
					if (duration_ss>59) duration_ss=59;

					// convert the julian date
					int year = (int) ((start_time_MJD - 15078.2) / 365.25);
					int month = (int) ((start_time_MJD - 14956.1 - (int)(year * 365.25)) / 30.6001);
					int day = (int) (start_time_MJD - 14956 - (int)(year * 365.25) - (int)(month * 30.6001));
					int k = (month == 14 || month == 15) ? 1 : 0;
					year += 1900+ k; // start from year 1900, so add that here
					month = month - 1 - k * 12;
					int starttime_y=year;
					int starttime_m=month;
					int starttime_d=day;

					try
					{
						DateTime dtUTC = new DateTime(starttime_y,starttime_m,starttime_d,starttime_hh,starttime_mm,starttime_ss,0);
						DateTime dtStart=dtUTC.ToLocalTime();
						DateTime dtEnd=dtStart.AddHours(duration_hh);
						dtEnd=dtEnd.AddMinutes(duration_mm);
						dtEnd=dtEnd.AddSeconds(duration_ss);

						TVProgram tv=new TVProgram();
						tv.Start=Util.Utils.datetolong(dtStart);
						tv.End=Util.Utils.datetolong(dtEnd);
						tv.Channel=channel.Name;
						tv.Genre=genre;
						tv.Title=title;
						tv.Description=description;
						if(tv.Title==null)
							tv.Title=String.Empty;

						if(tv.Description==null)
							tv.Description=String.Empty;

						if(tv.Description==String.Empty)
							tv.Description=title;

						if(tv.Title==String.Empty)
							tv.Title=title;

						if(tv.Title==String.Empty || tv.Title=="n.a.") 
						{
							return;
						}

						if (!grabLanguage) 
						{
							Log.WriteFile(Log.LogType.EPG,"epg-grab: disregard language: {0} {1}-{2} {3} {4}", tv.Channel,tv.Start,tv.End,tv.Title,language);
							return;
						}
						if (lastProgram.End <=tv.Start)
						{
							Log.WriteFile(Log.LogType.EPG,"epg-grab: add: {0} {1}-{2} {3} {4}", tv.Channel,tv.Start,tv.End,tv.Title,language);
							TVDatabase.AddProgram(tv);
						}
					}
					catch(Exception)
					{
						Log.WriteFile(Log.LogType.EPG,"epg-grab:invalid date:{0}-{1}-{2}",starttime_y,starttime_m,starttime_d);
					}
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.EPG,"epg-grab:{0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);
			}
		}


		#endregion
		#endregion
	}

}
