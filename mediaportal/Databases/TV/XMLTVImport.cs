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
			TVDatabase.SupressEvents=true;
      bool bUseTimeZone=false;
      int iTimeZoneCorrection=0;
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        bUseTimeZone=xmlreader.GetValueAsBool("xmltv", "usetimezone",true);
        iTimeZoneCorrection=xmlreader.GetValueAsInt("xmltv", "timezonecorrection",0);
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
				  Log.Write("  {0} is not a valid xml file");
				  xml=null;
				  m_bImport=false;
				  TVDatabase.SupressEvents=false;
				  return false;
			  }
			  XmlNodeList channelList=xml.DocumentElement.SelectNodes("/tv/channel");
			  if (channelList==null || channelList.Count==0)
			  {
				  m_strErrorMessage="No channels found";
				  Log.Write("  {0} does not contain any channels");
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
        TVDatabase.RemoveChannel("Composite #1");
        TVDatabase.RemoveChannel("Composite #2");
			  TVDatabase.RemoveChannel("SVHS");

			  TVChannel chanComposite=new TVChannel();
			  chanComposite.Number=1001;
			  chanComposite.Name="Composite #1";
			  chanComposite.Frequency=0;
			  TVDatabase.AddChannel(chanComposite);

			  TVChannel chanComposite2=new TVChannel();
			  chanComposite2.Number=1002;
			  chanComposite2.Name="Composite #2";
			  chanComposite2.Frequency=0;
			  TVDatabase.AddChannel(chanComposite2);
	        
			  TVChannel chanSVHS=new TVChannel();
			  chanSVHS.Number=1000;
			  chanSVHS.Name="SVHS";
			  chanSVHS.Frequency=0;
			  TVDatabase.AddChannel(chanSVHS);
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
	                
							  chan.XMLId=nodeId.InnerText;
							  chan.Name=nodeName.InnerText;
							  chan.Number=0;
							  chan.Frequency=0;
							  TVDatabase.AddChannel(chan);

							  ChannelPrograms newProgChan = new ChannelPrograms();
							  newProgChan.strName=chan.Name;
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
										  string strURL = nodeSrc.InnerText;
										  string strLogoPng=Utils.GetCoverArtName(@"tv\logos",chan.Name);
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
							  Log.Write("  channel#{0} doesnt contain an displayname",iChannel);
						  }
					  }
					  else
					  {
						  Log.Write("  channel#{0} doesnt contain an id",iChannel);
					  }
				  }
				  else
				  {
					  Log.Write("  channel#{0} doesnt contain an id",iChannel);
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
			      //- <rating system="BBFC">
			      //  <value>U</value> 
			      //  </rating>
			      //- <star-rating>
			      //  <value>4/5</value> 
			      //  </star-rating>
			      //  <date>1956</date> 

					  if (nodeStart!=null && nodeChannel!=null && nodeTitle!=null)
					  {
						  if (nodeStart.InnerText!=null && nodeChannel.InnerText!=null && nodeTitle.InnerText!=null)
						  {
							  string strDescription="";
							  string strCategory="-";
				        string strEpisode="";
				        string strRepeat="";
				          if (nodeRepeat!=null)
				          {
						        strRepeat="Repeat";
				          }
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
							  iHour -= utcOff.Hours;
							  iMin -= utcOff.Minutes;
							  iHour += iTimeZoneCorrection;
							  dtStart=dtStart.AddHours( iHour);
							  dtStart=dtStart.AddMinutes( iMin );
							  iStart=Utils.datetolong(dtStart);

							  // correct program endtime
							  DateTime dtEnd=Utils.longtodate(iStop);
							  iHour=(iEndTimeOffset/100);
							  iMin=iEndTimeOffset-(iHour*100);
							  iHour -= utcOff.Hours;
							  iMin -= utcOff.Minutes;
							  iHour += iTimeZoneCorrection;
							  dtEnd=dtEnd.AddHours( iHour);
							  dtEnd=dtEnd.AddMinutes( iMin );
							  iStop=Utils.datetolong(dtEnd);

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
								  Log.Write("Unknown TV channel xmlid:{0}", nodeChannel.InnerText);
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
				}
				
							  TVProgram prog=new TVProgram();
							  prog.Description=strDescription;
							  prog.Start=iStart;
							  prog.End=iStop;
							  prog.Title=strTitle;
							  prog.Episode=strEpisode;
							  prog.Genre=strCategory;
							  prog.Repeat=strRepeat;
							  prog.Channel=strChannelName;
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
						  //Log.Write("Add program :{0,-20} {1} {2} {3}",prog.Channel, prog.StartTime.ToShortDateString(),prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat) , prog.Title); 

				TVDatabase.UpdateProgram(prog);
//				TVDatabase.AddProgram(prog);
						  if (prog.StartTime < m_stats.StartTime) m_stats.StartTime=prog.StartTime;
						  if (prog.EndTime > m_stats.EndTime) m_stats.EndTime=prog.EndTime;
						  m_stats.Programs++;
						  if (bShowProgress && ShowProgress!=null && (m_stats.Programs%100)==0 ) ShowProgress(m_stats);
					  }
				  }
			  }
			  TVDatabase.CommitTransaction();
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
			  Log.Write("xmltv data file was not found");
		  }
      }
      catch(Exception ex)
      {
        m_strErrorMessage=String.Format("Invalid XML file:{0}",ex.Message);
        m_stats.Status=String.Format("invalid XML file:{0}", ex.Message);
        Log.Write("XML tv import error loading {0} err:{1} ", strFileName, ex.Message);
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
    
    #region Sort Members


    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      TVProgram item1=(TVProgram)x;
      TVProgram item2=(TVProgram)y;
      if (item1==null) return -1;
      if (item2==null) return -1;

      if (item1.Start>item2.Start) return 1;
      if (item1.Start<item2.Start) return -1;
      return 0;
    }
    #endregion


	}
}
