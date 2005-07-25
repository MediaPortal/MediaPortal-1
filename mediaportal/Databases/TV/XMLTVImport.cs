using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Net;
//using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.TV.Database
{
	/// <summary>
	/// 
	/// </summary>
	public class XMLTVImport: IComparer
	{
    public delegate void ShowProgressHandler(Stats stats);
    public event ShowProgressHandler ShowProgress;

    class ChannelPrograms
    {
      public string strName;
      public string XMLId;
      public ArrayList programs = new ArrayList();
    };

		public class Stats
		{
      string m_strStatus="";
			int m_iPrograms=0;
			int m_iChannels=0;
			DateTime m_startTime=DateTime.Now;
			DateTime m_EndTime=DateTime.Now;
      public string Status
      {
        get { return m_strStatus;}
        set { m_strStatus=value;}
      }
			public int Programs
			{
				get { return m_iPrograms;}
				set { m_iPrograms=value;}
			}
			public int Channels
			{
				get { return m_iChannels;}
				set { m_iChannels=value;}
			}
			public DateTime StartTime
			{
				get { return m_startTime;}
				set { m_startTime=value;}
			}
			public DateTime EndTime
			{
				get { return m_EndTime;}
				set { m_EndTime=value;}
			}
		};
    
    string m_strErrorMessage="";
		Stats  m_stats = new Stats();
    
    static bool   m_bImport=false;      
    public XMLTVImport()
		{
		}

    public string ErrorMessage
    {
      get { return m_strErrorMessage;}
    }

		public Stats ImportStats
		{
			get { return m_stats;}
		}
    public bool Import(string strFileName, bool bShowProgress)
    {
      if (m_bImport==true)
      {
        m_strErrorMessage="already importing...";
        return false;
      }
      m_bImport=true;
			HTMLUtil htmlUtil = new HTMLUtil();
			TVDatabase.SupressEvents=true;
      bool bUseTimeZone=false;
      int iTimeZoneCorrection=0;
      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        bUseTimeZone=xmlreader.GetValueAsBool("xmltv", "usetimezone",true);
        int hours=xmlreader.GetValueAsInt("xmltv", "timezonecorrectionhours", 0);
        int mins=xmlreader.GetValueAsInt("xmltv", "timezonecorrectionmins", 0);
        iTimeZoneCorrection=hours*60+mins;

      }

      m_stats.Status=GUILocalizeStrings.Get(645);
			m_stats.Channels=0;
			m_stats.Programs=0;
			m_stats.StartTime=DateTime.Now;
			m_stats.EndTime=new DateTime(1971,11,6);
      if (bShowProgress && ShowProgress!=null) ShowProgress(m_stats);
      ArrayList Programs=new ArrayList();
      try
      {
		  Log.Write("xmltv import {0}", strFileName);
		  
		  //
		  // Make sure the file exists before we try to do any processing
		  //
		  if(File.Exists(strFileName))
		  {
			  XmlDocument xml=new XmlDocument();
			  xml.Load(strFileName);
			  if (xml.DocumentElement==null)
			  {
				  m_strErrorMessage="Invalid XMLTV file";
				  Log.WriteFile(Log.LogType.Log,true,"  {0} is not a valid xml file");
				  xml=null;
				  m_bImport=false;
				  TVDatabase.SupressEvents=false;
				  return false;
			  }
			  XmlNodeList channelList=xml.DocumentElement.SelectNodes("/tv/channel");
			  if (channelList==null || channelList.Count==0)
			  {
				  m_strErrorMessage="No channels found";
				  Log.WriteFile(Log.LogType.Log,true,"  {0} does not contain any channels");
				  xml=null;
				  m_bImport=false;
				  TVDatabase.SupressEvents=false;
				  return false;
			  }
	        
	        
			  m_stats.Status=GUILocalizeStrings.Get(642);
			  if (bShowProgress && ShowProgress!=null) ShowProgress(m_stats);
	        
			  TVDatabase.BeginTransaction();
        TVDatabase.ClearCache();
        TVDatabase.RemoveOldPrograms();
	        
			  ArrayList tvchannels = new ArrayList();
			  int iChannel=0;
			  foreach (XmlNode nodeChannel in channelList)
			  {
				  if (nodeChannel.Attributes!=null)
				  {
					  XmlNode nodeId=nodeChannel.Attributes.GetNamedItem("id");
					  if (nodeId!=null&&nodeId.InnerText!=null && nodeId.InnerText.Length>0)
					  {
						  XmlNode nodeName=nodeChannel.SelectSingleNode("display-name");
              if (nodeName==null)
                nodeName=nodeChannel.SelectSingleNode("Display-Name");
						  XmlNode nodeIcon=nodeChannel.SelectSingleNode("icon");
						  if (nodeName!=null && nodeName.InnerText!=null)
						  {
							  TVChannel chan=new TVChannel();
	                
								//parse name of channel to see if it contains a channel number
								string number=String.Empty;
								for (int i=0; i < nodeName.InnerText.Length;++i)
								{
									if (Char.IsDigit(nodeName.InnerText[i]))
									{
										number += nodeName.InnerText[i];
									}
									else break;
								}
								if (number==String.Empty)
								{
									for (int i=0; i < nodeId.InnerText.Length;++i)
									{
										if (Char.IsDigit(nodeId.InnerText[i]))
										{
											number += nodeId.InnerText[i];
										}
										else break;
									}
								}
								int channelNo=0;
								if (number!=String.Empty)
									channelNo=Int32.Parse(number);
							  chan.XMLId=nodeId.InnerText;
							  chan.Name=htmlUtil.ConvertHTMLToAnsi(nodeName.InnerText);
							  chan.Number=channelNo;
							  chan.Frequency=0;

								int		 idTvChannel;
								string strTvChannel;
								if (TVDatabase.GetEPGMapping(nodeId.InnerText, out idTvChannel, out strTvChannel))
								{
									chan.ID=idTvChannel;
									chan.Name=strTvChannel;
								}
								else
								{
									TVDatabase.AddChannel(chan);
									TVDatabase.MapEPGChannel(chan.ID,chan.Name,nodeId.InnerText);
								}

							  ChannelPrograms newProgChan = new ChannelPrograms();
							  newProgChan.strName=htmlUtil.ConvertHTMLToAnsi(chan.Name);
							  newProgChan.XMLId=chan.XMLId;
							  Programs.Add(newProgChan);

							  //Log.Write("  channel#{0} xmlid:{1} name:{2} dbsid:{3}",iChannel,chan.XMLId,chan.Name,chan.ID);
							  tvchannels.Add(chan);
							  if (nodeIcon!=null)
							  {
								  if (nodeIcon.Attributes!=null)
								  {
									  XmlNode nodeSrc=nodeIcon.Attributes.GetNamedItem("src");
									  if (nodeSrc !=null)
									  {
										  string strURL = htmlUtil.ConvertHTMLToAnsi(nodeSrc.InnerText);
										  string strLogoPng=Utils.GetCoverArtName(@"thumbs\tv\logos",chan.Name);
										  if (!System.IO.File.Exists(strLogoPng) )
										  {
											  Utils.DownLoadImage(strURL,strLogoPng,System.Drawing.Imaging.ImageFormat.Png);
										  }
									  }
								  }
							  }
							  m_stats.Channels++;
							  if (bShowProgress && ShowProgress!=null) ShowProgress(m_stats);
						  }
						  else
						  {
							  Log.WriteFile(Log.LogType.Log,true,"  channel#{0} doesnt contain an displayname",iChannel);
						  }
					  }
					  else
					  {
						  Log.WriteFile(Log.LogType.Log,true,"  channel#{0} doesnt contain an id",iChannel);
					  }
				  }
				  else
				  {
					  Log.WriteFile(Log.LogType.Log,true,"  channel#{0} doesnt contain an id",iChannel);
				  }
				  iChannel++;
			  }
	        
			  int iProgram=0;
			  m_stats.Status=GUILocalizeStrings.Get(643);
			  if (bShowProgress && ShowProgress!=null) ShowProgress(m_stats);

			  // get offset between local time & UTC
			  TimeSpan utcOff=System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
	                
			  // take in account daylightsavings 
			  bool bIsDayLightSavings=System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now);
			  if (bIsDayLightSavings) utcOff=utcOff.Add (new TimeSpan(0,-1,0,0,0));

			  Log.Write("Current timezone:{0}",System.TimeZone.CurrentTimeZone.StandardName);
			  Log.Write("Offset with UTC {0:00}:{1:00} DaylightSavings:{2}",utcOff.Hours,utcOff.Minutes,bIsDayLightSavings.ToString());
			  XmlNodeList programsList = xml.DocumentElement.SelectNodes("/tv/programme");
	        
			  foreach (XmlNode programNode in programsList)
			  {
				  if (programNode.Attributes!=null)
				  {
					  XmlNode nodeStart=programNode.Attributes.GetNamedItem("start");
					  XmlNode nodeStop=programNode.Attributes.GetNamedItem("stop");
					  XmlNode nodeChannel=programNode.Attributes.GetNamedItem("channel");
					  XmlNode nodeTitle=programNode.SelectSingleNode("title");
					  XmlNode nodeCategory=programNode.SelectSingleNode("category");
					  XmlNode nodeDescription=programNode.SelectSingleNode("desc");
			      XmlNode nodeEpisode=programNode.SelectSingleNode("sub-title");
			      XmlNode nodeRepeat=programNode.SelectSingleNode("previously-shown");
            XmlNode nodeEpisodeNum=programNode.SelectSingleNode("episode-num");
            XmlNode nodeDate=programNode.SelectSingleNode("date");
            XmlNode nodeStarRating=programNode.SelectSingleNode("star-rating");
            XmlNode nodeClasification=programNode.SelectSingleNode("rating");

					  if (nodeStart!=null && nodeChannel!=null && nodeTitle!=null)
					  {
						  if (nodeStart.InnerText!=null && nodeChannel.InnerText!=null && nodeTitle.InnerText!=null)
						  {
							  string strDescription="";
							  string strCategory="-";
				        string strEpisode="";
				        string strRepeat="";
                string strSerEpNum="";
                string strDate="";
                string strSeriesNum="";
                string strEpisodeNum="";
                string strEpisodePart="";
                string strStarRating="";
                string strClasification="";

				        if (nodeRepeat!=null)strRepeat="Repeat";
				         
							  string strTitle=nodeTitle.InnerText;
							  long iStart=0;
                if (nodeStart.InnerText.Length>=14)
                {
                  if (Char.IsDigit(nodeStart.InnerText[12]) && Char.IsDigit(nodeStart.InnerText[13]))
                    iStart=Int64.Parse(nodeStart.InnerText.Substring(0,14));//20040331222000
                  else
                    iStart=100*Int64.Parse(nodeStart.InnerText.Substring(0,12));//200403312220
                }
                else if (nodeStart.InnerText.Length>=12)
                {
                  iStart=100*Int64.Parse(nodeStart.InnerText.Substring(0,12));//200403312220
                }


							  long iStop=iStart;
							  if (nodeStop!=null && nodeStop.InnerText!=null)
							  {
                  if (nodeStop.InnerText.Length>=14)
                  {
                    if (Char.IsDigit(nodeStop.InnerText[12]) && Char.IsDigit(nodeStop.InnerText[13]))
                      iStop=Int64.Parse(nodeStop.InnerText.Substring(0,14));//20040331222000
                    else 
                      iStop=100*Int64.Parse(nodeStop.InnerText.Substring(0,12));//200403312220
                  }
                  else if (nodeStop.InnerText.Length>=12)
                  {
                    iStop=100*Int64.Parse(nodeStop.InnerText.Substring(0,12));//200403312220
                  }
							  }
								iStart=CorrectIllegalDateTime(iStart);
								iStop=CorrectIllegalDateTime(iStop);
							  string strTimeZoneStart="";
							  string strTimeZoneEnd="";
							  int iStartTimeOffset=0;
							  int iEndTimeOffset=0;
							  if (nodeStart.InnerText.Length>14)
							  {
								  strTimeZoneStart=nodeStart.InnerText.Substring(14);
								  strTimeZoneStart=strTimeZoneStart.Trim();
								  strTimeZoneEnd=strTimeZoneStart;
							  }
							  if (nodeStop!=null)
							  {
								  if (nodeStop.InnerText.Length>14)
								  {
									  strTimeZoneEnd=nodeStop.InnerText.Substring(14);
									  strTimeZoneEnd=strTimeZoneEnd.Trim();
								  }
							  }

							  // are we using the timezone information from the XMLTV file
							  if (!bUseTimeZone)
							  {
								  // no
								  iStartTimeOffset=0;
								  iEndTimeOffset=0;
							  }
							  else
							  {
								  // yes, then get the start/end timeoffsets
								  iStartTimeOffset = GetTimeOffset(strTimeZoneStart);
								  iEndTimeOffset   = GetTimeOffset(strTimeZoneEnd);
							  }

							  // add timezone correction
							  // correct program starttime
							  DateTime dtStart=Utils.longtodate(iStart);
							  int iHour=(iStartTimeOffset/100);
							  int iMin=iStartTimeOffset-(iHour*100);
							  //iHour -= utcOff.Hours;
							  //iMin -= utcOff.Minutes;
							  dtStart=dtStart.AddHours( iHour);
                dtStart=dtStart.AddMinutes( iMin );
                dtStart=dtStart.AddMinutes( iTimeZoneCorrection );
							  iStart=Utils.datetolong(dtStart);

								if (nodeStop!=null && nodeStop.InnerText!=null)
								{
									// correct program endtime
									DateTime dtEnd=Utils.longtodate(iStop);
									iHour=(iEndTimeOffset/100);
									iMin=iEndTimeOffset-(iHour*100);
									//			  iHour -= utcOff.Hours;
									//			  iMin -= utcOff.Minutes;
									dtEnd=dtEnd.AddHours( iHour);
									dtEnd=dtEnd.AddMinutes( iMin );
									dtEnd=dtEnd.AddMinutes( iTimeZoneCorrection );
									iStop=Utils.datetolong(dtEnd);
								}
								else iStop=iStart;

							  int iChannelId=-1;
							  string strChannelName="";
							  if ( nodeChannel.InnerText.Length > 0)
							  {
								  foreach (TVChannel chan in tvchannels)
								  {
									  if (chan.XMLId==nodeChannel.InnerText) 
									  {
										  strChannelName=chan.Name;
										  iChannelId=chan.ID;
										  break;
									  }
								  }
							  }
							  if (iChannelId<0)
							  {
								  Log.WriteFile(Log.LogType.Log,true,"Unknown TV channel xmlid:{0}", nodeChannel.InnerText);
								  continue;
							  }
	              
							  if (nodeCategory!=null && nodeCategory.InnerText!=null)
							  {
								  strCategory=nodeCategory.InnerText;
							  }
							  if (nodeDescription!=null && nodeDescription.InnerText!=null)
							  {
								  strDescription=nodeDescription.InnerText;
							  }
								if (nodeEpisode!=null && nodeEpisode.InnerText!=null)
								{
									strEpisode=nodeEpisode.InnerText;
									if (strTitle.Length==0)
										strTitle=nodeEpisode.InnerText;
								}
								if (nodeEpisodeNum!=null && nodeEpisodeNum.InnerText!=null)
								{
                  if (nodeEpisodeNum.Attributes.GetNamedItem("system").InnerText=="xmltv_ns")
                  {
									strSerEpNum=htmlUtil.ConvertHTMLToAnsi(nodeEpisodeNum.InnerText.Replace(" ",""));
									int pos=0;
									int Epos=0;
									pos = strSerEpNum.IndexOf(".",pos);
                    if (pos==0) //na_dd grabber only gives '..0/2' etc
                    {
                      Epos=pos;
                      pos = strSerEpNum.IndexOf(".",pos+1);
                      strEpisodeNum=strSerEpNum.Substring(Epos+1,(pos-1)-Epos);
                      strEpisodePart=strSerEpNum.Substring(pos+1,strSerEpNum.Length-(pos+1));
                      if (strEpisodePart.IndexOf("/",0)!=-1)// danish guide gives: episode-num system="xmltv_ns"> . 113 . </episode-num>
                      {
                        if (strEpisodePart.Substring(2,1)=="1") strEpisodePart = "";
                        else
                        {
                          int p=0;
                          int t=0;

                            if (Convert.ToInt32(strEpisodePart.Substring(0,1))==0)
                            {
                              p = Convert.ToInt32(strEpisodePart.Substring(0,1))+1;
                              t = Convert.ToInt32(strEpisodePart.Substring(2,1));
                              strEpisodePart = Convert.ToString(p)+"/"+Convert.ToString(t);                        }
                            }
                      }
                    }
                    else if (pos>0)
                    {
                      strSeriesNum= strSerEpNum.Substring(0,pos);
                      Epos=pos;
                      pos = strSerEpNum.IndexOf(".",pos+1);
                      strEpisodeNum=strSerEpNum.Substring(Epos+1,(pos-1)-Epos);
                      strEpisodePart=strSerEpNum.Substring(pos+1,strSerEpNum.Length-(pos+1));
                      if (strEpisodePart.IndexOf("/",0)!=-1)
                      {
                        if (strEpisodePart.Substring(2,1)=="1") strEpisodePart = "";
                        else
                        {
                          int p=0;
                          int t=0;
                          if (Convert.ToInt32(strEpisodePart.Substring(0,1))==0)
                          {
                            p = Convert.ToInt32(strEpisodePart.Substring(0,1))+1;
                          }
                          else
                          {
                            p = Convert.ToInt32(strEpisodePart.Substring(0,1));
                          }
                          t = Convert.ToInt32(strEpisodePart.Substring(2,1));
                          strEpisodePart = Convert.ToString(p)+"/"+Convert.ToString(t);
                        }
                      }
                    }
                    else
                    {
                      strSeriesNum = strSerEpNum;
                      strEpisodeNum = "";
                      strEpisodePart = "";
                    }
                  }
								}
								if (nodeDate!=null && nodeDate.InnerText!=null)
								{
									strDate=nodeDate.InnerText;
								}
								if (nodeStarRating!=null && nodeStarRating.InnerText!=null)
								{
									strStarRating = nodeStarRating.InnerText;
								}
								if (nodeClasification!=null && nodeClasification.InnerText!=null)
								{
									strClasification = nodeClasification.InnerText;
								}
								 
							  TVProgram prog=new TVProgram();
							  prog.Description=htmlUtil.ConvertHTMLToAnsi(strDescription);
							  prog.Start=iStart;
							  prog.End=iStop;
							  prog.Title=htmlUtil.ConvertHTMLToAnsi(strTitle);
							  prog.Episode=htmlUtil.ConvertHTMLToAnsi(strEpisode);
							  prog.Genre=htmlUtil.ConvertHTMLToAnsi(strCategory);
							  prog.Repeat=htmlUtil.ConvertHTMLToAnsi(strRepeat);
							  prog.Channel=htmlUtil.ConvertHTMLToAnsi(strChannelName);
                prog.Date=strDate;
                prog.SeriesNum=htmlUtil.ConvertHTMLToAnsi(strSeriesNum);
                prog.EpisodeNum=htmlUtil.ConvertHTMLToAnsi(strEpisodeNum);
                prog.EpisodePart=htmlUtil.ConvertHTMLToAnsi(strEpisodePart);
                prog.StarRating=htmlUtil.ConvertHTMLToAnsi(strStarRating);
                prog.Classification=htmlUtil.ConvertHTMLToAnsi(strClasification);
							  m_stats.Programs++;
							  if (bShowProgress && ShowProgress!=null && (m_stats.Programs%100)==0 ) ShowProgress(m_stats);
							  foreach (ChannelPrograms progChan in Programs)
							  {
								  if (String.Compare(progChan.strName,strChannelName,true)==0)
								  {
									  progChan.programs.Add(prog);

								  }
								  
							  }
						  }
					  }
				  }
				  iProgram++;
			  }
			  m_stats.Programs=0;
			  m_stats.Status=GUILocalizeStrings.Get(644);
			  if (bShowProgress && ShowProgress!=null) ShowProgress(m_stats);
			  DateTime dtStartDate=new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,0,0,0,0);
			  //dtStartDate=dtStartDate.AddDays(-4);

			  foreach (ChannelPrograms progChan in Programs)
			  {
				  progChan.programs.Sort( this);
				  for (int i=0; i < progChan.programs.Count;++i)
				  {
					  TVProgram prog = (TVProgram)progChan.programs[i];
					  if (prog.Start==prog.End)
					  {
						  if (i+1 < progChan.programs.Count)
						  {
							  TVProgram progNext=(TVProgram)progChan.programs[i+1];
							  prog.End=progNext.Start;
						  }
					  }
					  // dont import programs which have already ended...
					  if (prog.EndTime > dtStartDate)
					  {
						  Log.WriteFile(Log.LogType.EPG,false,"epg-import :{0,-20} {1} {2}-{3} {4}",
												prog.Channel, 
								        prog.StartTime.ToShortDateString(),
												prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat) , 
												prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat) , 
								        prog.Title); 

							TVDatabase.UpdateProgram(prog);
						  if (prog.StartTime < m_stats.StartTime) m_stats.StartTime=prog.StartTime;
						  if (prog.EndTime > m_stats.EndTime) m_stats.EndTime=prog.EndTime;
						  m_stats.Programs++;
						  if (bShowProgress && ShowProgress!=null && (m_stats.Programs%100)==0 ) ShowProgress(m_stats);
					  }
				  }
			  }
			  TVDatabase.CommitTransaction();
				TVDatabase.RemoveOverlappingPrograms();
			  Programs.Clear();
			  Programs=null;
			  xml=null;
			  m_bImport=false;
			  TVDatabase.SupressEvents=false;
			  if (iProgram>0) 
			  {
				  return true;
			  }
			  m_strErrorMessage="No programs found";
			  return false;
		  }
		  else
		  {
        m_strErrorMessage="No xmltv file found";
        m_stats.Status=m_strErrorMessage;
        Log.Write("xmltv data file was not found");
		  }
      }
      catch(Exception ex)
      {
        m_strErrorMessage=String.Format("Invalid XML file:{0}",ex.Message);
        m_stats.Status=String.Format("invalid XML file:{0}", ex.Message);
        Log.WriteFile(Log.LogType.Log,true,"XML tv import error loading {0} err:{1} ", strFileName, ex.Message);
        TVDatabase.RollbackTransaction();
      }
			Programs.Clear();
      Programs=null;
			m_bImport=false;
			TVDatabase.SupressEvents=false;
			return false;
    }

    int GetTimeOffset(string strTimeZone)
    {
      // timezone can b in format:
      // GMT +0100 or GMT -0500
      // or just +0300
      if (strTimeZone.Length==0) return 0;
      strTimeZone=strTimeZone.ToLower();

      // just ignore GMT offsets, since we're calculating everything from GMT anyway
      if (strTimeZone.IndexOf("gmt")>=0)
      {
        int ipos=strTimeZone.IndexOf("gmt");
        strTimeZone=strTimeZone.Substring(ipos+"GMT".Length);
      }

      strTimeZone=strTimeZone.Trim();
      if (strTimeZone[0]=='+' || strTimeZone[0]=='-')
      {
        string strOff=strTimeZone.Substring(1);
        try
        {
          int iOff=Int32.Parse(strOff);
          if (strTimeZone[0]=='-') return -iOff;
          else return iOff;
        }
        catch (Exception)
        {
        }
      }
      return 0;
    }

		long CorrectIllegalDateTime(long datetime)
		{
			//format : 20050710245500
			long orgDateTime=datetime;
			long sec=datetime % 100; datetime /=100;
			long min=datetime % 100; datetime /=100;
			long hour=datetime % 100; datetime /=100;
			long day=datetime % 100; datetime /=100;
			long month=datetime % 100; datetime /=100;
			long year=datetime ; 
			DateTime dt = new DateTime((int)year,(int)month,(int)day,0,0,0);
			dt=dt.AddHours(hour);
			dt=dt.AddMinutes(min);
			dt=dt.AddSeconds(sec);


			long newDateTime=Utils.datetolong(dt);
			if (sec<0 || sec>59 ||
				min<0 || min>59 ||
				hour<0 || hour >=24 ||
				day <0 || day>31 ||
				month <0 || month > 12)
			{
				Log.WriteFile(Log.LogType.EPG,true,"epg-import:tvguide.xml contains invalid date/time :{0} converted it to:{1}", 
					            orgDateTime, newDateTime);
			}

			return newDateTime;
		}
    #region Sort Members


    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      TVProgram item1=(TVProgram)x;
      TVProgram item2=(TVProgram)y;
      if (item1==null) return -1;
      if (item2==null) return -1;

			if (item1.Channel != item2.Channel)
			{
				return String.Compare(item1.Channel,item2.Channel,true);
			}
      if (item1.Start>item2.Start) return 1;
      if (item1.Start<item2.Start) return -1;
      return 0;
    }
    #endregion


	}
}
