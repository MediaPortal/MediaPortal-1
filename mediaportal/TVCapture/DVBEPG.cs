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

		NetworkType m_networkType;
		
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
				System.DateTime date=new DateTime(data.starttime_y,data.starttime_m,data.starttime_d,data.starttime_hh,data.starttime_mm,data.starttime_ss);
				date=date.ToLocalTime();
				System.DateTime dur=new DateTime(date.Ticks);
				dur=dur.AddSeconds((double)data.duration_ss);
				dur=dur.AddMinutes((double)data.duration_mm);
				dur=dur.AddHours((double)data.duration_hh);
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
				long chStart=0;
				long chEnd=0;
				System.DateTime chStartDate=new DateTime((long)date.Ticks);
				chStartDate=chStartDate.AddMinutes(1);
				System.DateTime chEndDate=new DateTime((long)dur.Ticks-60000);
				chStart=GetLongFromDate(chStartDate.Year,chStartDate.Month,chStartDate.Day,chStartDate.Hour,chStartDate.Minute,chStartDate.Second);
				chEnd=GetLongFromDate(chEndDate.Year,chEndDate.Month,chEndDate.Day,chEndDate.Hour,chEndDate.Minute,chEndDate.Second);
				//
				//
				tv.Start=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
				tv.End=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
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
		public void GetMHWEPG(DShowNET.IBaseFilter filter)
		{
			m_sections.GetMHWData(filter);

		}
	}// class
}// namespace
