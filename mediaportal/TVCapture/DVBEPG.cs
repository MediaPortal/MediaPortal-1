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
		}
		DVBSections		m_sections=new DVBSections();
		int				m_cardType=0;
		
		public enum EPGCard
		{
			Invalid=0,
			TechnisatStarCards,
			BDACards,
			Unknown
		}
		//
		// commits epg-data to database
		//
		public int SetEITToDatabase(DVBSections.EITDescr data,string channelName,int eventKind)
		{
			try
			{
				int retVal=0;
				TVProgram tv=new TVProgram();
				System.DateTime date=new DateTime(data.starttime_y,data.starttime_m,data.starttime_d,data.starttime_hh,data.starttime_mm,data.starttime_ss);
				date=date.ToLocalTime();
				System.DateTime dur=new DateTime();
				dur=date;
				dur=dur.AddHours((double)data.duration_hh);
				dur=dur.AddMinutes((double)data.duration_mm);
				dur=dur.AddSeconds((double)data.duration_ss);
				tv.Channel=channelName;
				tv.Genre=data.genere_text;
				tv.Title=data.event_name;
				tv.Description=data.event_item_text;


				if(tv.Title=="" || tv.Title=="n.a.") 
				{
					Log.Write("epg: entrie without title found");
					return 0;
				}
				long checkStart=0;
				long checkEnd=0;
				// for check
				checkStart=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute+2,date.Second);
				checkEnd=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute-2,dur.Second);
				//
				tv.Start=GetLongFromDate(date.Year,date.Month,date.Day,date.Hour,date.Minute,date.Second);
				tv.End=GetLongFromDate(dur.Year,dur.Month,dur.Day,dur.Hour,dur.Minute,dur.Second);
				ArrayList programsInDatabase = new ArrayList();
				TVDatabase.GetProgramsPerChannel(tv.Channel,checkStart,checkEnd,ref programsInDatabase);
				if(programsInDatabase.Count==0 && channelName!="")
				{
					int programID=TVDatabase.AddProgram(tv);
					//TVDatabase.RemoveOverlappingPrograms();
					if(programID!=-1)
					{
						retVal= 1;
					}
				}
				else
					Log.Write("epg-grab: FAILED to add to database: {0} : {1}",tv.Start,tv.End);
				return retVal;
			}
			catch(Exception ex)
			{
				Log.Write("epg-grab: FAILED to add to database. message:{0}",ex.Message);
				return 0;
			}
		}
		//
		// returns long-value from sep. date
		//
		private long GetLongFromDate(int year,int mon,int day,int hour,int min,int sec)
		{
			string longVal="";
			string yr=year.ToString();
			string mo=mon.ToString();
			string da=day.ToString();
			string h=hour.ToString();
			string m=min.ToString();
			string s=sec.ToString();
			if(mo.Length==1)
				mo="0"+mo;
			if(da.Length==1)
				da="0"+da;
			if(h.Length==1)
				h="0"+h;
			if(m.Length==1)
				m="0"+m;
			if(s.Length==1)
				s="0"+s;
			longVal=yr+mo+da+h+m+s;
			return (long)Convert.ToUInt64(longVal);
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

			eitList=m_sections.GetEITSchedule(0x50,filter,ref lastTab);
			tableList.Add(eitList);
			if(lastTab>0x50)
			{
				for(int tab=0x51;tab<lastTab;tab++)
				{
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

					if(serviceID!=0)
					{
						if(eit.program_number==serviceID)
							eventsCount+=SetEITToDatabase(eit,progName,0x50);
					}
					else
						eventsCount+=SetEITToDatabase(eit,progName,0x50);
					n++;
				}

		
			GC.Collect();
			return 	eventsCount;

		}
	}// class
}// namespace
