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
		#region variables
		IEPGGrabber epgInterface=null;
		IMHWGrabber mhwInterface=null;
		NetworkType networkType;
		bool        grabEPG=false;
		bool        isGrabbing=false;
		#endregion

		#region properties
		public IEPGGrabber EPGInterface
		{
			get { return epgInterface;}
			set { epgInterface=value;}
		}
		public IMHWGrabber MHWInterface
		{
			get { return mhwInterface;}
			set { mhwInterface=value;}
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
			grabEPG=epg;
			if (grabEPG)
			{
				Log.WriteFile(Log.LogType.EPG,"epg-grab: start EPG grabber");
				if (EPGInterface!=null)
					EPGInterface.GrabEPG();
			}
			else
			{
				Log.WriteFile(Log.LogType.EPG,"epg-grab: start MHW grabber");
				if (MHWInterface!=null)
					MHWInterface.GrabMHW();
			}
			isGrabbing=true;
		}
		public void Process()
		{
			if (!isGrabbing) return;
			bool ready=false;
			if (EPGInterface!=null && grabEPG)
			{
				EPGInterface.IsEPGReady(out ready);
				if (ready)
				{
					ParseEPG();
					isGrabbing=false;
				}
			}
			
			if (MHWInterface!=null && !grabEPG)
			{
				MHWInterface.IsMHWReady(out ready);
				if (ready)
				{
					ParseMHW();
					isGrabbing=false;
				}
			}
		}
		#endregion

		#region private methods

		#region MHW
		void ParseMHW()
		{
			try
			{
				Log.WriteFile(Log.LogType.EPG,"mhw-epg: mhw ready");
				short titleCount;
				MHWInterface.GetMHWTitleCount(out titleCount);
				Log.WriteFile(Log.LogType.EPG,"mhw-epg: mhw titles:{0}",titleCount);
				if (titleCount<=0) return;
				for (short i=0; i < titleCount; ++i)
				{
					short id=0,transportid=0, networkid=0, channelid=0, programid=0, themeid=0, PPV=0,duration=0;
					byte summaries=0;
					uint datestart=0,timestart=0; 
					IntPtr ptrTitle,ptrProgramName;
					IntPtr ptrChannelName,ptrSummary, ptrTheme;
					MHWInterface.GetMHWTitle(i,ref id, ref transportid, ref networkid, ref channelid, ref programid, ref themeid, ref PPV, ref summaries, ref duration, ref datestart,ref timestart, out ptrTitle,out ptrProgramName);
					MHWInterface.GetMHWChannel(channelid,ref networkid, ref transportid,out ptrChannelName);
					MHWInterface.GetMHWSummary(programid, out ptrSummary);
					MHWInterface.GetMHWTheme(themeid, out ptrTheme);

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
					Log.WriteFile( Log.LogType.EPG,"mhw-epg: channel:{0} time:{1} {2} duration:{3} program:{4} title:{5} theme:{6} summary:{7} onid:{8} tsid:{9}",
						channelName,programStartTime.ToShortTimeString(),programStartTime.ToShortDateString(),
						duration,programName,title,theme,summary, networkid, transportid);

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
			string m_languagesToGrab=String.Empty;
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_languagesToGrab=xmlreader.GetValueAsString("epg-grabbing","grabLanguages","");
			}
			uint channelCount=0;
			Log.WriteFile(Log.LogType.EPG,"epg-grab: EPG ready");
			EPGInterface.GetEPGChannelCount(out channelCount);
			Log.WriteFile(Log.LogType.EPG,"epg-grab: received epg for {0} channels", channelCount);
			for (uint i=0; i < channelCount;++i)
			{
				ushort networkid=0;
				ushort transportid=0;
				ushort serviceid=0;
				uint eventCount=0;
				EPGInterface.GetEPGChannel(i,ref networkid, ref transportid, ref serviceid);
				TVChannel channel=TVDatabase.GetTVChannelByStream(Network==NetworkType.ATSC,Network==NetworkType.DVBT,Network==NetworkType.DVBC,Network==NetworkType.DVBS,networkid,transportid,serviceid);
				if (channel==null) 
				{
					Log.WriteFile(Log.LogType.EPG,"epg-grab: Unknown channel NetworkId:{0} transportid:{1} serviceid:{2}",networkid,transportid,serviceid);
					continue;
				}
				Log.WriteFile(Log.LogType.EPG,"epg-grab: Channel:{0}",channel.Name);
				
				EPGInterface.GetEPGEventCount(i,out eventCount);
				//Log.Write("epg-grab: channel:{0} onid:{1} tsid:{2} sid:{3} events:{4}", i,networkid,transportid,serviceid,eventCount);
				for (uint x=0; x < eventCount;++x)
				{
					uint start_time_MJD=0,start_time_UTC=0,duration=0,languageId=0;
					string title,description,genre;
					IntPtr ptrTitle=IntPtr.Zero;
					IntPtr ptrDesc=IntPtr.Zero;
					IntPtr ptrGenre=IntPtr.Zero;
					EPGInterface.GetEPGEvent(i,x,out languageId,out start_time_MJD, out start_time_UTC, out duration,out ptrTitle,out ptrDesc, out ptrGenre);
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

					if (!grabLanguage) continue;

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
							continue;
						}

						Log.WriteFile(Log.LogType.EPG,"epg-grab: {0} {1}-{2} {3} {4}", tv.Channel,tv.Start,tv.End,tv.Title,language);
						ArrayList programsInDatabase = new ArrayList();
						TVDatabase.GetProgramsPerChannel(tv.Channel,tv.Start+1,tv.End-1,ref programsInDatabase);
						if(programsInDatabase.Count==0)
						{
							TVDatabase.AddProgram(tv);
						}
					}
					catch(Exception)
					{
						Log.Write("epg-grab: invalid date: year:{0} month:{1}, day:{2} {3}:{4}:{5}", year,month,day,starttime_hh,starttime_mm,starttime_ss);
						Log.Write("{0:X} {1:X}", start_time_MJD,start_time_UTC);
					}
				}
			}
			Log.WriteFile(Log.LogType.EPG,"epg-grab: EPG done");
		}


		#endregion
		#endregion
	}

}
