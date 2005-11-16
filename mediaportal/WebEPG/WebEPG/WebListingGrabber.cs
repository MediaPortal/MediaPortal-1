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
using System.Net;
using System.Web;
using System.Text;
using System.Threading;
using System.Collections;
using System.Globalization;
using MediaPortal.Webepg.Profile;
using MediaPortal.Webepg.GUI.Library;
using MediaPortal.Webepg.TV.Database;
using MediaPortal.WebEPG;
using MediaPortal.Utils.Web;

namespace MediaPortal.EPG
{

		public enum Expect
		{
			Start,
			Morning,
			Afternoon
		}

        /// <summary>
        /// Summary description for Class1
        /// </summary>
        public class WebListingGrabber
        {
            string m_strURLbase = string.Empty;
			string m_strSubURL = string.Empty;
            string m_strURLsearch = string.Empty;
            string m_strID = string.Empty;
            string m_strBaseDir = "";
			string m_SubListingLink;
			string m_strRepeat;
			string m_strSubtitles;
			string m_strEpNum;
			string m_strEpTotal;
			string[] m_strDayNames = null;
			bool m_grabLinked;
            bool m_monthLookup;
			bool m_searchRegex;
			bool m_searchRemove;
			int m_listingTime;
			int m_linkStart;
			int m_linkEnd;
            int m_maxListingCount;
			int m_offsetStart;
			int m_LastStart;
			int m_grabDelay;
			int m_guideDays;
			//int m_addDays;
			bool m_bNextDay;			
            Profiler m_templateProfile;
			//Parser m_templateParser;
			Profiler m_templateSubProfile;
			//Parser m_templateSubParser;
            MediaPortal.Webepg.Profile.Xml m_xmlreader;
            ArrayList m_programs;
            DateTime m_StartGrab;
            int m_MaxGrabDays;
            int m_GrabDay;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="maxGrabDays">The number of days to grab</param>
			/// <param name="baseDir">The baseDir for grabber files</param>
            public WebListingGrabber(int maxGrabDays, string baseDir)
            {
                m_MaxGrabDays = maxGrabDays;
                m_strBaseDir = baseDir;
            }

            public bool Initalise(string File)
            {
				string listingTemplate;

                Log.WriteFile(Log.LogType.Log, false, "WebEPG: Opening {0}", File);

                m_xmlreader = new MediaPortal.Webepg.Profile.Xml(m_strBaseDir + File);

                m_strURLbase = m_xmlreader.GetValueAsString("Listing", "BaseURL", "");
                if (m_strURLbase == "")
                {
                    Log.WriteFile(Log.LogType.Log, false, "WebEPG: {0}: No BaseURL defined", File);
                    return false;
                }

                m_strURLsearch = m_xmlreader.GetValueAsString("Listing", "SearchURL", "");
				m_grabDelay = m_xmlreader.GetValueAsInt("Listing", "GrabDelay", 500);
                m_maxListingCount = m_xmlreader.GetValueAsInt("Listing", "MaxCount", 0);
				m_offsetStart = m_xmlreader.GetValueAsInt("Listing", "OffsetStart", 0);
				m_guideDays = m_xmlreader.GetValueAsInt("Info", "GuideDays", 0);

                string ListingType = m_xmlreader.GetValueAsString("Listing", "ListingType", "");

				switch(ListingType)
				{
					case "XML":
						XMLProfilerData data = new XMLProfilerData();
						data.ChannelEntry = m_xmlreader.GetValueAsString("Listing", "ChannelEntry", "");
						data.StartEntry = m_xmlreader.GetValueAsString("Listing", "StartEntry", "");
						data.EndEntry = m_xmlreader.GetValueAsString("Listing", "EndEntry", "");
						data.TitleEntry = m_xmlreader.GetValueAsString("Listing", "TitleEntry", "");
						data.SubtitleEntry = m_xmlreader.GetValueAsString("Listing", "SubtitleEntry", "");
						data.DescEntry = m_xmlreader.GetValueAsString("Listing", "DescEntry", "");
						data.GenreEntry = m_xmlreader.GetValueAsString("Listing", "GenreEntry", "");
						data.XPath = m_xmlreader.GetValueAsString("Listing", "XPath", "");
						m_templateProfile = new XMLProfiler("", data);
						break;

					case "DATA":
						string strListingDelimitor = m_xmlreader.GetValueAsString("Listing", "ListingDelimitor", "\n");
						string strDataDelimitor = m_xmlreader.GetValueAsString("Listing", "DataDelimitor", "\t");
						listingTemplate = m_xmlreader.GetValueAsString("Listing", "Template", "");
						if (listingTemplate == "")
						{
							Log.WriteFile(Log.LogType.Log, true, "WebEPG: {0}: No Template", File);
							return false;
						}
						m_templateProfile = new DataProfiler(listingTemplate, strDataDelimitor[0], strListingDelimitor[0]);
						break;

					default: // HTML
						string strGuideStart = m_xmlreader.GetValueAsString("Listing", "Start", "<body");
						string strGuideEnd = m_xmlreader.GetValueAsString("Listing", "End", "</body");
						//bool bAhrefs = m_xmlreader.GetValueAsBool("Listing", "Ahrefs", false);
						string tags = m_xmlreader.GetValueAsString("Listing", "Tags", "T");
						string encoding = m_xmlreader.GetValueAsString("Listing", "Encoding", "");
						listingTemplate = m_xmlreader.GetValueAsString("Listing", "Template", "");
						if (listingTemplate == "")
						{
							Log.WriteFile(Log.LogType.Log, true, "WebEPG: {0}: No Template", File);
							return false;
						}
						//m_templateProfile = new HTMLProfiler(listingTemplate, bAhrefs, strGuideStart, strGuideEnd);
						m_templateProfile = new HTMLProfiler(listingTemplate, tags, strGuideStart, strGuideEnd, encoding);

						m_searchRegex = m_xmlreader.GetValueAsBool("Listing", "SearchRegex", false);
						if(m_searchRegex)
						{
							m_searchRemove = m_xmlreader.GetValueAsBool("Listing", "SearchRemove", false);
							m_strRepeat = m_xmlreader.GetValueAsString("Listing", "SearchRepeat", "");
							m_strSubtitles = m_xmlreader.GetValueAsString("Listing", "SearchSubtitles", "");
							m_strEpNum = m_xmlreader.GetValueAsString("Listing", "SearchEpNum", "");
							m_strEpTotal = m_xmlreader.GetValueAsString("Listing", "SearchEpTotal", "");
						}

						m_SubListingLink = m_xmlreader.GetValueAsString("Listing", "SubListingLink", "");
						if(m_SubListingLink != "")
						{
							string strSubStart = m_xmlreader.GetValueAsString("SubListing", "Start", "<body");
							string strSubEnd = m_xmlreader.GetValueAsString("SubListing", "End", "</body");
							string subencoding = m_xmlreader.GetValueAsString("SubListing", "Encoding", "");
							m_strSubURL = m_xmlreader.GetValueAsString("SubListing", "URL", "");
							string Subtags = m_xmlreader.GetValueAsString("SubListing", "Tags", "T");
							string sublistingTemplate = m_xmlreader.GetValueAsString("SubListing", "Template", "");
							if (sublistingTemplate == "")
							{
								Log.WriteFile(Log.LogType.Log, true, "WebEPG: {0}: No SubTemplate", File);
								m_SubListingLink="";
							}
							else
							{
								m_templateSubProfile = new HTMLProfiler(sublistingTemplate, Subtags, strSubStart, strSubEnd, subencoding);
							}
						}

						string firstDay = m_xmlreader.GetValueAsString("DayNames", "0", "");
						if(firstDay != "" && m_guideDays != 0)
						{
							m_strDayNames = new string[m_guideDays];
							m_strDayNames[0] = firstDay;
							for(int i=1; i < m_guideDays; i++)
								m_strDayNames[i] = m_xmlreader.GetValueAsString("DayNames", i.ToString(), "");
						}
						break;
                }

                m_monthLookup = m_xmlreader.GetValueAsBool("DateTime", "Months", false);
                return true;
            }

			public long GetEpochTime(DateTime dtCurTime) 
			{ 
				DateTime dtEpochStartTime = Convert.ToDateTime("1/1/1970 8:00:00 AM"); 
				TimeSpan ts = dtCurTime.Subtract(dtEpochStartTime); 

				long epochtime; 
				epochtime = ((((((ts.Days * 24) + ts.Hours) * 60) + ts.Minutes) * 60) + ts.Seconds); 
				return epochtime; 
			} 

			private int getMonth(string month)
			{
				if (m_monthLookup)
					return m_xmlreader.GetValueAsInt("DateTime", month, 0);
				else
					return int.Parse(month);
			}

			private string getGenre(string genre)
			{
				return m_xmlreader.GetValueAsString("GenreMap", genre, genre);
			}

			private long GetLongDateTime(DateTime dt)
			{
				long lDatetime;

				lDatetime = dt.Year;
				lDatetime *= 100;
				lDatetime += dt.Month;
				lDatetime *= 100;
				lDatetime += dt.Day;
				lDatetime *= 100;
				lDatetime += dt.Hour;
				lDatetime *= 100;
				lDatetime += dt.Minute;
				lDatetime *= 100;
				// no seconds

				return lDatetime;
			}

			private TVProgram GetProgram(Profiler guideProfile, int index)
			{
				//Parser Listing = guideProfile.GetProfileParser(index);
				int addDays = 1;
				TVProgram program = new TVProgram();
				HTMLProfiler htmlProf = null;
				if(guideProfile is HTMLProfiler) 
				{
					htmlProf = (HTMLProfiler) guideProfile;

					if(m_searchRegex)
					{
						string repeat = htmlProf.SearchRegex(index, m_strRepeat, m_searchRemove);
						string subtitles = htmlProf.SearchRegex(index, m_strSubtitles, m_searchRemove);
						string epNum = htmlProf.SearchRegex(index, m_strEpNum, m_searchRemove);
						string epTotal  = htmlProf.SearchRegex(index, m_strEpTotal, m_searchRemove);
					}
				}
				ProgramData guideData = new ProgramData();
				ParserData data = (ParserData) guideData;
				guideProfile.GetParserData(index, ref data); //m_templateParser.GetProgram(Listing);

				if(guideData.StartTime == null || guideData.Title == "")
					return null;

				program.Channel = m_strID;
				program.Title = guideData.Title;
				int month;

				if (guideData.Month == "")
				{
					month = m_StartGrab.Month;
				}
				else
				{
					month = getMonth(guideData.Month);
				}
				
				if (guideData.Day == 0)
				{
					guideData.Day = m_StartGrab.Day;
				}
				else
				{
                    if (guideData.Day != m_StartGrab.Day && m_listingTime != (int) Expect.Start)
					{
						m_GrabDay++;
						m_StartGrab = m_StartGrab.AddDays(1);
						m_bNextDay=false;
						m_LastStart=0;
						m_listingTime = (int) Expect.Morning;
					}
				}

				//Log.WriteFile(Log.LogType.Log, false, "WebEPG: {0}:{1}/{2}", guideData.StartTime[0], guideData.StartTime[1], guideData.Day);
				// Adjust Time 
				switch(m_listingTime)
				{
					case (int) Expect.Start:
						if(guideData.StartTime[0] >= 20)
							return null;				// Guide starts on pervious day ignore these listings.

						m_listingTime = (int) Expect.Morning;
						goto case (int)Expect.Morning;      // Pass into Morning Code

					case (int) Expect.Morning:
						if(m_LastStart > guideData.StartTime[0])
						{
							m_listingTime = (int) Expect.Afternoon;
                            //if (m_bNextDay)
                            //{
                            //    m_GrabDay++;
                            //} 
						}
						else
						{
							if(guideData.StartTime[0] <= 12)
								break;						// Do nothing
						}

						// Pass into Afternoon Code
						//m_LastStart = 0;
						goto case (int)Expect.Afternoon;

					case (int) Expect.Afternoon:
						if(guideData.StartTime[0] < 12)		// Site doesn't have correct time
							guideData.StartTime[0] += 12;	// starts again at 1:00 with "pm"

						if(m_LastStart > guideData.StartTime[0])
						{
							guideData.StartTime[0] -= 12;
							if(m_bNextDay)
							{
								addDays++;
								m_GrabDay++;
								m_StartGrab = m_StartGrab.AddDays(1);
								//m_bNextDay = false;
							} 
							else
							{
								m_bNextDay = true;
							}
							m_listingTime = (int) Expect.Morning;
							break;
						}

						break;

					default:
						break;
				}

				DateTime dtStart = new DateTime(m_StartGrab.Year, month, guideData.Day, guideData.StartTime[0], guideData.StartTime[1], 0, 0);
				if(m_bNextDay)
					dtStart = dtStart.AddDays(addDays);
				program.Start = GetLongDateTime(dtStart);
				m_LastStart = guideData.StartTime[0];

				if (guideData.EndTime != null)
				{
					DateTime dtEnd = new DateTime(m_StartGrab.Year, month, guideData.Day, guideData.EndTime[0], guideData.EndTime[1], 0, 0);
					if(m_bNextDay)
					{
						if(guideData.StartTime[0] > guideData.EndTime[0])
							dtEnd = dtEnd.AddDays(addDays+1);
						else
							dtEnd = dtEnd.AddDays(addDays);
					}
					else
					{
						if(guideData.StartTime[0] > guideData.EndTime[0])
							dtEnd = dtEnd.AddDays(addDays);
					}
					program.End = GetLongDateTime(dtEnd);

                    Log.WriteFile(Log.LogType.Log, false, "WebEPG: {0}:{1}/{2}-{3}:{4}/{5} [{6} {7}] - {8}", guideData.StartTime[0], guideData.StartTime[1], dtStart.Day, guideData.EndTime[0], guideData.EndTime[1], dtEnd.Day, m_GrabDay.ToString(), m_bNextDay.ToString(), guideData.Title);
				}
				else
				{
					Log.WriteFile(Log.LogType.Log, false, "WebEPG: {0}:{1}/{2} [{3} {4}] - {5}", guideData.StartTime[0], guideData.StartTime[1], dtStart.Day, m_GrabDay.ToString(), m_bNextDay.ToString(), guideData.Title);
				}
				
				if (guideData.Description != "")
					program.Description = guideData.Description;

				if (guideData.Genre != "")
					program.Genre = getGenre(guideData.Genre);

				if(m_grabLinked && m_SubListingLink != "" 
					&& guideData.StartTime[0] >= m_linkStart 
					&& guideData.StartTime[0] <= m_linkEnd
					&& htmlProf != null)
				{
					string linkURL; 
					if(m_strSubURL != "")
						linkURL = m_strSubURL;
					else
						linkURL = m_strURLbase;

					string strLinkURL = htmlProf.GetHyperLink(index, m_SubListingLink, linkURL);

					if(strLinkURL != "")
					{
						Log.WriteFile(Log.LogType.Log, false, "WebEPG: Reading {0}", strLinkURL);
						Thread.Sleep(m_grabDelay);
						Profiler SubProfile = m_templateSubProfile.GetPageProfiler(strLinkURL); 
						int Count = SubProfile.subProfileCount(); 

						if(Count > 0)
						{
							ProgramData SubData =  new ProgramData();
							ParserData refdata = (ParserData) SubData;
							SubProfile.GetParserData(0, ref refdata);
							
							if (SubData.Description != "")
								program.Description = SubData.Description;

							if (SubData.Genre != "")
								program.Genre = getGenre(SubData.Genre);

							if (SubData.SubTitle != "")
								program.Episode = SubData.SubTitle;
						}
					}

				}

				return program;
			}

            private bool GetListing(string strURL, int offset, string strChannel)
            {
				Profiler guideProfile;
                bool bMore = false;
                int listingCount = 0;

                strURL = strURL.Replace("#LIST_OFFSET", offset.ToString());

                Log.WriteFile(Log.LogType.Log, false, "WebEPG: Reading {0}{1}", m_strURLbase, strURL);

				if(m_templateProfile is XMLProfiler)
				{
					XMLProfiler templateProfile = (XMLProfiler) m_templateProfile;
					templateProfile.SetChannelID(strChannel);
				}
				guideProfile = m_templateProfile.GetPageProfiler(m_strURLbase + strURL);
                if(guideProfile != null)
                    listingCount = guideProfile.subProfileCount();

				if(listingCount == 0) // && m_maxListingCount == 0)
				{
                    if (m_maxListingCount == 0 || (m_maxListingCount != 0 && offset == 0))
                    {
                        Log.WriteFile(Log.LogType.Log, true, "WebEPG: No Listings Found");
                        m_GrabDay++;
                    }
                    else
                    {
                        Log.WriteFile(Log.LogType.Log, false, "WebEPG: Listing Count 0");
                    }
                    //m_GrabDay++;
				}
				else
				{
					Log.WriteFile(Log.LogType.Log, false, "WebEPG: Listing Count {0}", listingCount);
					
					if (listingCount == m_maxListingCount)
						bMore = true;

					for (int i = 0; i < listingCount; i++)
					{
						TVProgram program = GetProgram(guideProfile, i);
						if (program != null)
						{
							m_programs.Add(program);
						}
					}

					if (m_GrabDay > m_MaxGrabDays)
						bMore = false;
				}

                return bMore;
            }


            public ArrayList GetGuide(string strChannelID,  bool Linked, int linkStart, int linkEnd)
            {
                m_strID = strChannelID;
				m_grabLinked = Linked;
				m_linkStart = linkStart;
				m_linkEnd = linkEnd;
                int offset = 0;

                string searchID = m_xmlreader.GetValueAsString("ChannelList", strChannelID, "");
				CultureInfo culture = new CultureInfo("en-US");


                if (searchID == "")
                {
                    Log.WriteFile(Log.LogType.Log, true, "WebEPG: ChannelId: {0} not found!", strChannelID);
                    return null;
                }

                m_programs = new ArrayList();

                string strURLid = m_strURLsearch.Replace("#ID", searchID);
                string strURL;

                Log.WriteFile(Log.LogType.Log, false, "WebEPG: ChannelId: {0}", strChannelID);

                m_GrabDay = 0;
				m_StartGrab = DateTime.Now;

                while (m_GrabDay < m_MaxGrabDays)
                {
                    strURL = strURLid;
					if(m_strDayNames != null)
						strURL = strURL.Replace("#DAY_NAME", m_strDayNames[m_GrabDay]);

                    strURL = strURL.Replace("#DAY_OFFSET", (m_GrabDay+m_offsetStart).ToString());
					strURL = strURL.Replace("#EPOCH_TIME", GetEpochTime(m_StartGrab).ToString());
                    strURL = strURL.Replace("#YYYY", m_StartGrab.Year.ToString());
                    strURL = strURL.Replace("#MM", String.Format("{0:00}", m_StartGrab.Month));
                    strURL = strURL.Replace("#_M", m_StartGrab.Month.ToString());
					strURL = strURL.Replace("#MONTH", m_StartGrab.ToString("MMMM", culture));
                    strURL = strURL.Replace("#DD", String.Format("{0:00}", m_StartGrab.Day));
                    strURL = strURL.Replace("#_D", m_StartGrab.Day.ToString());
					strURL = strURL.Replace("#WEEKDAY", m_StartGrab.ToString("dddd", culture));

                    offset = 0;
					m_LastStart=0;
					m_bNextDay = false;
					m_listingTime = (int) Expect.Start;

                    while (GetListing(strURL, offset, searchID))
                    {
						Thread.Sleep(m_grabDelay);
                        if (m_maxListingCount == 0)
                            break;
                        offset += m_maxListingCount;
                    }
                    //m_GrabDay++;
                    if (strURL != strURLid)
                    {
                        m_StartGrab = m_StartGrab.AddDays(1);
                        m_GrabDay++;
                    }
                    else
                    {
                        if (strURL.IndexOf("#LIST_OFFSET") == -1)
                            break;
                    }
                }
             
                return m_programs;
            }
        }
}
