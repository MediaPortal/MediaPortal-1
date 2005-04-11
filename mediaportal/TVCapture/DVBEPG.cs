using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung für DVBEPG.
	/// </summary>
	public class DVBEPG
	{
		public DVBEPG(int card)
		{
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
			m_cardType=card;
			m_networkType=NetworkType.DVBS;
		}
		public DVBEPG(int card, NetworkType networkType)
		{
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
			m_cardType=card;
			m_networkType=networkType;
		}
		DVBSections		m_sections=new DVBSections();
		int				m_cardType=0;
		string			m_channelName="";
		string			m_languagesToGrab="";
		ArrayList		m_summariesBuffer=new ArrayList();
		ArrayList		m_titleBuffer=new ArrayList();
		ArrayList		m_namesBuffer=new ArrayList();
		ArrayList		m_themeBuffer=new ArrayList();
		ArrayList		m_summarieBuffer=new ArrayList();
		NetworkType		m_networkType;
		ArrayList		m_streamBuffer=new ArrayList();
		bool			m_isGrabbing=false;
		int				m_grabLen=0;
		int				m_savedData=0;
		byte[]			m_mhwEpgBuffer;
		int				m_mhwTable=0;
		int				m_mhwCurrentPid=0;
		
		// mhw
		public struct Programm 
		{
			public int		ID;
			public int		ChannelID;
			public int		ThemeID;
			public int		PPV;
			public DateTime	Time;
			public bool		Summaries;
			public int		Duration;
			public string	Title;
			public int		ProgrammID;
			public string	ProgrammName;
			public int		TransportStreamID;
			public int		NetworkID;
		};
		//
		public struct MHWChannel
		{
			public int		NetworkID;
			public int		TransponderID;
			public int		ChannelID;
			public string	ChannelName;
		};
		public struct Summarie
		{
			public int		ProgramID;// its the programm-id of epg, not an channel id
			public string	Description;
		}
		// mhw end
		public enum EPGCard
		{
			Invalid=0,
			TechnisatStarCards,
			BDACards,
			Unknown,
			ChannelName
		}
		//
		//
		public string Languages
		{
			get
			{
				return m_languagesToGrab;
			}
			set
			{
				m_languagesToGrab=value;
			}
		}
		//
		// commits epg-data to database
		//

		public bool GrabState
		{
			get{return m_isGrabbing;}
			set
			{ 
				if(value==true)
				{
					m_isGrabbing=true;
					m_streamBuffer.Clear();
					m_savedData=0;
					m_mhwTable=0;
					m_mhwCurrentPid=0;
				}
				else
				{
					m_isGrabbing=false;
					m_mhwEpgBuffer=new byte[m_savedData];
					int counter=0;
					foreach(byte[] data in m_streamBuffer)
					{
						Array.Copy(data,0,m_mhwEpgBuffer,counter,data.Length);
						counter+=data.Length;
					}
					if(m_mhwCurrentPid==0xd3 && m_mhwTable==0x91)
						ParseChannels(m_mhwEpgBuffer);
					if(m_mhwCurrentPid==0xd2 && m_mhwTable==0x90)
						ParseTitles(m_mhwEpgBuffer);
					if(m_mhwCurrentPid==0xd3 && m_mhwTable==0x90)
						ParseSummaries(m_mhwEpgBuffer);

					counter=0;
				}
			}
		}
		//
		//
		//
		public int CurrentPid
		{
			get{return m_mhwCurrentPid;}
			set{m_mhwCurrentPid=value;}
		}
		public int MHWTable
		{
			get{return m_mhwTable;}
			set{m_mhwTable=value;}
		}
		public int GrabbingLength
		{
			get{return m_grabLen;}
			set{if(m_isGrabbing)m_grabLen=value;}
		}
		public void ClearBuffer()
		{
			if(m_streamBuffer!=null)
				m_streamBuffer.Clear();
			if(m_namesBuffer!=null)
				m_namesBuffer.Clear();
			if(m_titleBuffer!=null)
				m_titleBuffer.Clear();
			if(m_summarieBuffer!=null)
				m_summarieBuffer.Clear();

			m_grabLen=0;
			m_mhwTable=0;
			m_mhwCurrentPid=0;
		}
		public int SetEITToDatabase(DVBSections.EITDescr data,string channelName,int eventKind)
		{
			try
			{
				int retVal=0;
				//
				//
				if(data.extendedEventUseable==false && data.shortEventUseable==false)
				{
					Log.Write("epg-grabbing: event IGNORED by language selection");
					return 0;
				}
				
				//
				Log.Write("start *********************************************");
				TVProgram tv=new TVProgram();
				long chStart=0;
				long chEnd=0;

				if(data.isMHWEvent==false)
				{
					System.DateTime date=new DateTime(data.starttime_y,data.starttime_m,data.starttime_d,data.starttime_hh,data.starttime_mm,data.starttime_ss);
					date=date.ToLocalTime();
					System.DateTime dur=new DateTime(date.Ticks);
					dur=dur.AddSeconds((double)data.duration_ss);
					dur=dur.AddMinutes((double)data.duration_mm);
					dur=dur.AddHours((double)data.duration_hh);
					System.DateTime chStartDate=new DateTime((long)date.Ticks);
					chStartDate=chStartDate.AddMinutes(1);
					System.DateTime chEndDate=new DateTime((long)dur.Ticks-60000);
					chStart=GetLongFromDate(chStartDate.Year,chStartDate.Month,chStartDate.Day,chStartDate.Hour,chStartDate.Minute,chStartDate.Second);
					chEnd=GetLongFromDate(chEndDate.Year,chEndDate.Month,chEndDate.Day,chEndDate.Hour,chEndDate.Minute,chEndDate.Second);
					//
					//
					tv.Start=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
					tv.End=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
				}
				else
				{
					DateTime date=data.mhwStartTime;
					System.DateTime dur=new DateTime(date.Ticks);
					dur=dur.AddMinutes((double)data.duration_mm);
					System.DateTime chStartDate=new DateTime((long)date.Ticks);
					chStartDate=chStartDate.AddMinutes(1);
					System.DateTime chEndDate=new DateTime((long)dur.Ticks-60000);
					chStart=GetLongFromDate(chStartDate.Year,chStartDate.Month,chStartDate.Day,chStartDate.Hour,chStartDate.Minute,chStartDate.Second);
					chEnd=GetLongFromDate(chEndDate.Year,chEndDate.Month,chEndDate.Day,chEndDate.Hour,chEndDate.Minute,chEndDate.Second);
					//
					//
					tv.Start=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
					tv.End=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
				}
				tv.Channel=channelName;
				tv.Genre=data.genere_text;

				tv.Title=data.event_item;
				tv.Description=data.event_item_text;
				//
				Log.Write(" ");
				Log.Write("epg-grabbing: short-event-use={0}",data.shortEventUseable==true?"yes":"no");
				Log.Write("epg-grabbing: extended-event-use={0}",data.extendedEventUseable==true?"yes":"no");
				Log.Write("epg-grabbing: short-event-complete={0}",data.shortEventComplete==true?"yes":"no");
				Log.Write("epg-grabbing: extended-event-complete={0}",data.extendedEventComplete==true?"yes":"no");
				Log.Write(" ");
				//
				if(tv.Title==null)
					tv.Title="";

				if(tv.Description==null)
					tv.Description="";

				if(tv.Description=="")
					tv.Description=data.event_text;

				if(tv.Title=="")
					tv.Title=data.event_name;

				if(tv.Description.Length<2)
				{
					tv.Title=data.event_name;
					tv.Description=data.event_text;
					Log.Write("epg-grab: used short description");
				}
				//
				if(data.eeLanguageCode!=null)
					Log.Write("epg-grab: language-code={0}",data.eeLanguageCode);
				
				if(data.seLanguageCode!=null)
					Log.Write("epg-grab: language-code={0}",data.seLanguageCode);
				
				if(tv.Title=="" || tv.Title=="n.a.") 
				{
					Log.Write("epg: entrie without title found");
					return 0;
				}

				//
				// for check
				//
				ArrayList programsInDatabase = new ArrayList();
				TVDatabase.GetProgramsPerChannel(tv.Channel,chStart,chEnd,ref programsInDatabase);
				if(channelName=="")
				{
					Log.Write("epg-grab: FAILED no channel-name: {0} : {1}",tv.Start,tv.End);
					return 0;
				}
				if(programsInDatabase.Count==0)
				{
					int programID=TVDatabase.AddProgram(tv);
					//TVDatabase.RemoveOverlappingPrograms();
					if(programID!=-1)
					{
						retVal= 1;
					}
					else
						Log.Write("epg-grab: FAILED (program id==-1): {0} : {1}",tv.Start,tv.End);

				}
				else
					Log.Write("epg-grab: SKIPPED already exists in database: {0} : {1}",tv.Start,tv.End);
				Log.Write("end ****************************************");
				return retVal;
			}
			catch(Exception ex)
			{
				Log.Write("epg-grab: FAILED to add to database. message:{0} stack:{1} source:{2}",ex.Message,ex.StackTrace,ex.Source);
				return 0;
			}
		}
		public string ChannelName
		{
			get{return m_channelName;}
			set{m_channelName=value;}
		}
		//
		// returns long-value from sep. date
		//
		private long GetLongFromDate(int year,int mon,int day,int hour,int min,int sec)
		{
			
			string longStringA=String.Format("{0:0000}{1:00}{2:00}",year,mon,day);
			string longStringB=String.Format("{0:00}{1:00}{2:00}",hour,min,sec);
			//Log.Write("epg-grab: string-value={0}",longStringA+longStringB);
			return (long)Convert.ToUInt64(longStringA+longStringB);
		}
		//
		//
		//

		public int GetEPG(DShowNET.IBaseFilter filter,int serviceID)
		{
			// there must be an ts (card tuned) to get eit
			// if serviceID!=0 only those services are grabbed
			// else all epg for all services found on act. ts will go to database

			if(m_cardType==(int)EPGCard.Invalid || m_cardType==(int)EPGCard.Unknown)
				return 0;

			int			eventsCount=0;
			ArrayList	eitList=new ArrayList();
			ArrayList	tableList=new ArrayList();
			int			lastTab=0;
			int			dummyTab=0;

			m_sections.Timeout=750;
			Log.Write("epg-grab: grabbing table {0}",80);
			eitList=m_sections.GetEITSchedule(0x50,filter,ref lastTab);
			tableList.Add(eitList);
			
			if(lastTab>0x5F)
				lastTab=0x50;

			if(lastTab>0x50)
			{
				for(int tab=0x51;tab<lastTab;tab++)
				{
					Log.Write("epg-grab: grabbing table {0}",tab);
					eitList.Clear();
					eitList=m_sections.GetEITSchedule(tab,filter,ref dummyTab);
					if(eitList.Count>0)
						tableList.Add(eitList);
				}
			}
			//
			int n=0;
			foreach(ArrayList eitData in tableList)
				foreach(DVBSections.EITDescr eit in eitData)
				{
					// the progName must be get from the database
					// to submitt to correct channel
					string progName="";
				
					switch(m_cardType)
					{
						case (int)EPGCard.TechnisatStarCards:
							progName=TVDatabase.GetSatChannelName(eit.program_number,eit.ts_id);
							Log.Write("epg-grab: counter={0} text:{1} start: {2}.{3}.{4} {5}:{6}:{7} duration: {8}:{9}:{10}",n,eit.event_name,eit.starttime_d,eit.starttime_m,eit.starttime_y,eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,eit.duration_hh,eit.duration_mm,eit.duration_ss);
							break;

						case (int)EPGCard.BDACards:
						{
							ArrayList channels = new ArrayList();
							TVDatabase.GetChannels(ref channels);
							int freq, symbolrate,innerFec,modulation, ONID, TSID, SID;
							int audioPid, videoPid, teletextPid, pmtPid;
							string provider="";
							foreach (TVChannel chan in channels)
							{
								switch (m_networkType)
								{
									case NetworkType.DVBC:
										TVDatabase.GetDVBCTuneRequest(chan.ID,out provider,out freq, out symbolrate,out innerFec,out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid);
										if (eit.program_number==SID && eit.ts_id==TSID)
										{
											progName=chan.Name;
											Log.Write("epg-grab: DVBC counter={0} text:{1} start: {2}.{3}.{4} {5}:{6}:{7} duration: {8}:{9}:{10} {11}",n,eit.event_name,eit.starttime_d,eit.starttime_m,eit.starttime_y,eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,eit.duration_hh,eit.duration_mm,eit.duration_ss,chan.Name);
										}
										break;
									case NetworkType.DVBS:
										progName=TVDatabase.GetSatChannelName(eit.program_number,eit.ts_id);
										break;
									case NetworkType.DVBT:
										TVDatabase.GetDVBTTuneRequest(chan.ID,out provider,out freq, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid);
										if (eit.program_number==SID && eit.ts_id==TSID)
										{
											Log.Write("epg-grab: DVBT counter={0} text:{1} start: {2}.{3}.{4} {5}:{6}:{7} duration: {8}:{9}:{10} {11}",n,eit.event_name,eit.starttime_d,eit.starttime_m,eit.starttime_y,eit.starttime_hh,eit.starttime_mm,eit.starttime_ss,eit.duration_hh,eit.duration_mm,eit.duration_ss,chan.Name);
											progName=chan.Name;
										}
										break;
								}
								if (progName!=String.Empty) break;
							}//foreach (TVChannel chan in channels)
						}
							break;

						case (int)EPGCard.ChannelName:
							progName=m_channelName;
							break;
					}
					if(progName==null)
					{
						Log.Write("epg-grab: FAILED name is NULL");
						continue;
					}
				
					if(progName=="")
					{
						Log.Write("epg-grab: FAILED empty name service-id:{0}",eit.program_number);
						continue;
					}
					DVBSections.EITDescr eit2DB=new MediaPortal.TV.Recording.DVBSections.EITDescr();
					eit2DB=eit;
					if(m_languagesToGrab!="")
					{
						eit2DB.extendedEventUseable=false;
						eit2DB.shortEventUseable=false;
					}
					else
					{
						eit2DB.extendedEventUseable=true;
						eit2DB.shortEventUseable=true;
					}

					if(m_languagesToGrab!="")
					{
						string[] langs=m_languagesToGrab.Split(new char[]{'/'});
						foreach(string lang in langs)
						{
							if(lang=="")
								continue;
							Log.Write("epg-grabbing: language selected={0}",lang);
							string codeEE="";
							string codeSE="";

							string eitItem=eit.event_item_text;
							if(eitItem==null)
								eitItem="";

							if(eit.eeLanguageCode!=null)
							{
								Log.Write("epg-grabbing: e-event-lang={0}",eit.eeLanguageCode);
								codeEE=eit.eeLanguageCode.ToLower();
								if(codeEE.Length==3)
								{
									if(lang.ToLower().Equals(codeEE))
									{
										eit2DB.extendedEventUseable=true;
										break;
									}
								}
							}

							if(eit.seLanguageCode!=null)
							{
								Log.Write("epg-grabbing: s-event-lang={0}",eit.seLanguageCode);
								codeSE=eit.seLanguageCode.ToLower();
								if(codeSE.Length==3)
								{
									if(lang.ToLower().Equals(codeSE))
									{
										eit2DB.shortEventUseable=true;
										break;
									}

								}

							}

							

						}
					}

					if(serviceID!=0)
					{
						if(eit.program_number==serviceID)
							eventsCount+=SetEITToDatabase(eit2DB,progName,0x50);
					}
					else
						eventsCount+=SetEITToDatabase(eit2DB,progName,0x50);
					n++;
				}
		
			GC.Collect();
			return 	eventsCount;

		}//public int GetEPG(DShowNET.IBaseFilter filter,int serviceID)
		public void SaveData(byte[] data,TSHelperTools.TSHeader header)
		{
			if(m_isGrabbing==true)
			{
				m_streamBuffer.Add(data);
				m_savedData+=data.Length;
				if(m_savedData>=m_grabLen && m_mhwCurrentPid==0xd3 && m_mhwTable==0x91)
					this.GrabState=false; // save what we got
				if(m_savedData>=0 && m_mhwCurrentPid==0xd3 && m_mhwTable==0x90)
					this.GrabState=false;
				if(m_savedData>=0xb8 && m_mhwCurrentPid==0xd2 && m_mhwTable==0x90)
					this.GrabState=false; // save what we got
				if(m_mhwCurrentPid==0xd3 && m_mhwTable!=0x91 && m_mhwTable!=0x90)
					this.GrabState=false;
				if(m_mhwCurrentPid==0xd2 && m_mhwTable!=0x90)
					this.GrabState=false;

			}
		}

		void ParseChannels(byte[] data1)
		{
			
			if(m_namesBuffer==null)
				return;
			if(m_namesBuffer.Count>0)
				return; // already got channles table
			
			Log.Write("mhw-epg: start parse channels for mhw",m_namesBuffer.Count);

			byte[] data=new byte[m_grabLen];
			Array.Copy(data1,5,data,0,m_grabLen);
			for(int n=0;n<m_grabLen;n+=22)
			{
				if(m_namesBuffer.Count>=(m_grabLen/22))
					break;
				MHWChannel ch=new MHWChannel();
				ch.NetworkID=(data[n]<<8)+data[n+1];
				ch.TransponderID=(data[n+2]<<8)+data[n+3];
				ch.ChannelID=(data[n+4]<<8)+data[n+5];
				ch.ChannelName=System.Text.Encoding.ASCII.GetString(data,n+6,16);
				ch.ChannelName=ch.ChannelName.Trim();

				if(m_namesBuffer.Contains(ch)==false)
					m_namesBuffer.Add(ch);
				if(m_namesBuffer.Count>8)
				{
					int a=0;
				}
			}// for(int n=0
			Log.Write("mhw-epg: found {0} channels for mhw",m_namesBuffer.Count);
		}
		void ParseSummaries(byte[] data1)
		{
			
//			if(m_summarieBuffer==null)
//				return;
//			if(m_namesBuffer.Count>0)
//				return; // already got channles table
//			
//			Log.Write("mhw-epg: start parse summaries for mhw",m_namesBuffer.Count);
//
//			byte[] data=new byte[data1.Length];
//			Array.Copy(data1,1,data,0,data1.Length-1);
//			for(int n=0;n<16384;)
//			{
//				if(((int)(data[n+1] & 0x70)!=0x70) || ((int)(data[n] & 0x90)!=0x90))
//				{
//					n++;
//					continue;
//				}
//				int len=((data[n+1]-0x70)<<8)+data[n+2];
//				Summarie sum=new Summarie();
//				sum.ProgramID=(data[n+3]<<24)+(data[n+4]<<16)+(data[n+5]<<8)+data[n+6];
//				sum.Description="";
//				int offset=11+(data[n+10]*7);
//				sum.Description=System.Text.Encoding.ASCII.GetString(data,offset+n,(len+3)-offset);
//				if(m_summarieBuffer.Contains(sum)==false)
//					m_summarieBuffer.Add(sum);
//				n+=len;
//			}
//
//			Log.Write("mhw-epg: found {0} summaries for mhw",m_namesBuffer.Count);
		}
		void ParseTitles(byte[] data)
		{

			for(int n=1;n<184;n+=46)
			{
				Programm prg=new Programm();
				if(data[n+3]==0xff)
					continue;
				if(n>=184)
					continue;
				
				prg.ChannelID=(data[n+3])-1;
				prg.ThemeID=data[n+4];
				int h=data[n+5] & 0x1F;
				int d=(data[n+5] & 0xE0)>>5;
				prg.Summaries=((data[n+6]>>7) & 0x1)==0?false:true;
				int m=data[n+6] >>2;
				prg.Duration=((data[n+9]<<8)+data[n+10]);// minutes
				prg.Title=System.Text.Encoding.ASCII.GetString(data,n+11,23);
				prg.Title=prg.Title.Trim();
				prg.PPV=(data[n+34]<<24)+(data[n+35]<<16)+(data[n+36]<<8)+data[n+37];
				prg.ID=(data[n+38]<<24)+(data[n+39]<<16)+(data[n+40]<<8)+data[n+41];
				// get time
				int d1=d;
				if(d>1 && d<7)
				{
					int f=1;
				}
				int h1=h;
				if (d1 == 7)
					d1 = 0;
				if (h1>15)
					h1 = h1-4;
				else if (h1>7)
					h1 = h1-2;
				else
					d1= (d1==6) ? 0 : d1+1;

				prg.Time=new DateTime(System.DateTime.Now.Ticks);
				DateTime dayStart=new DateTime(System.DateTime.Now.Ticks);
				dayStart=dayStart.Subtract(new TimeSpan(1,dayStart.Hour,dayStart.Minute,dayStart.Second,dayStart.Millisecond));
				int day=(int)dayStart.DayOfWeek;
				
				prg.Time=dayStart;
				int minVal=(d1-day)*86400+h1*3600+m*60;
				if(minVal<21600)
					minVal+=604800;

				//prg.Time=(d1-yesterday)*86400+h1*3600+m*60;
				prg.Time=prg.Time.AddSeconds(minVal);
//				prg.Time=prg.Time.AddHours(h1);
//				prg.Time=prg.Time.AddMinutes(m);
				
				if(m_namesBuffer==null)
					continue;
				if(m_namesBuffer.Count==0)
					continue;
				//
				
				if(m_titleBuffer.Contains(prg)==false && prg.ChannelID<m_namesBuffer.Count)
				{
					prg.ProgrammID=((MHWChannel)m_namesBuffer[prg.ChannelID]).ChannelID;
					prg.ProgrammName=((MHWChannel)m_namesBuffer[prg.ChannelID]).ChannelName;
					prg.TransportStreamID=((MHWChannel)m_namesBuffer[prg.ChannelID]).TransponderID;
					prg.NetworkID=((MHWChannel)m_namesBuffer[prg.ChannelID]).NetworkID;
					m_titleBuffer.Add(prg);
					if(m_titleBuffer.Count>=200)
					{
						SubmittMHW(); // commit to database and empty buffer
					}
				}

			}
		}

		void SubmittMHW()
		{
			foreach(Programm prg in m_titleBuffer)
			{
				DVBSections.EITDescr eit=new MediaPortal.TV.Recording.DVBSections.EITDescr();
				string channelName=TVDatabase.GetSatChannelName(prg.ProgrammID,prg.TransportStreamID);
				eit.event_name=prg.Title;
				eit.program_number=prg.ProgrammID;
				eit.event_text="unknown";
				eit.genere_text="unknown";
				eit.duration_mm=prg.Duration;
				eit.isMHWEvent=true;
				eit.shortEventUseable=true;
				eit.mhwStartTime=prg.Time;
				SetEITToDatabase(eit,channelName,0);

			}
			Log.Write("mhw-epg: submitted {0} programs to database",m_titleBuffer.Count);
			m_titleBuffer.Clear();
		}
	}// class
}// namespace
