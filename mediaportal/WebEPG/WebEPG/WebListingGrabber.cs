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
using MediaPortal.Util;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.WebEPGUtils;

namespace MediaPortal.EPG
{
        /// <summary>
        /// Summary description for Class1
        /// </summary>
        public class WebListingGrabber
        {
            string m_strURLbase = string.Empty;
            string m_strURLsearch = string.Empty;
            string m_strID = string.Empty;
            string m_strBaseDir = "";
			string m_SubListingLink;
			string m_strRepeat;
			string m_strSubtitles;
			string m_strEpNum;
			string m_strEpTotal;
			bool m_grabLinked;
            bool m_monthLookup;
			bool m_searchRegex;
			bool m_searchRemove;
			int m_linkStart;
			int m_linkEnd;
            int m_maxListingCount;
			int m_offsetStart;
			int m_LastStart;
			int m_grabDelay;
			bool m_bNextDay;			
            Profiler m_templateProfile;
			//Parser m_templateParser;
			Profiler m_templateSubProfile;
			//Parser m_templateSubParser;
            MediaPortal.Profile.Xml m_xmlreader;
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

                m_xmlreader = new MediaPortal.Profile.Xml(m_strBaseDir + File);

                m_strURLbase = m_xmlreader.GetValueAsString("Listing", "BaseURL", "");
                if (m_strURLbase == "")
                {
                    Log.WriteFile(Log.LogType.Log, false, "WebEPG: {0}: No BaseURL defined", File);
                    return false;
                }

                m_strURLsearch = m_xmlreader.GetValueAsString("Listing", "SearchURL", "");
				m_grabDelay = m_xmlreader.GetValueAsInt("Listing", "GrabDelay", 1000);
                m_maxListingCount = m_xmlreader.GetValueAsInt("Listing", "MaxCount", 0);
				m_offsetStart = m_xmlreader.GetValueAsInt("Listing", "OffsetStart", 0);

                string ListingType = m_xmlreader.GetValueAsString("Listing", "ListingType", "");

				switch(ListingType)
				{
					case "XML":
						XMLProfilerData data = new XMLProfilerData();
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
						m_templateProfile = new Profiler(listingTemplate, strDataDelimitor[0], strListingDelimitor[0]);
						break;

					default: // HTML
						string strGuideStart = m_xmlreader.GetValueAsString("Listing", "Start", "<body");
						string strGuideEnd = m_xmlreader.GetValueAsString("Listing", "End", "</body");
						bool bAhrefs = m_xmlreader.GetValueAsBool("Listing", "Ahrefs", false);
						listingTemplate = m_xmlreader.GetValueAsString("Listing", "Template", "");
						if (listingTemplate == "")
						{
							Log.WriteFile(Log.LogType.Log, true, "WebEPG: {0}: No Template", File);
							return false;
						}
						m_templateProfile = new HTMLProfiler(listingTemplate, bAhrefs, strGuideStart, strGuideEnd);

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
							bool bSubAhrefs = m_xmlreader.GetValueAsBool("SubListing", "Ahrefs", false);
							string sublistingTemplate = m_xmlreader.GetValueAsString("SubListing", "Template", "");
							if (sublistingTemplate == "")
							{
								Log.WriteFile(Log.LogType.Log, true, "WebEPG: {0}: No SubTemplate", File);
								m_SubListingLink="";
							}
							else
							{
								m_templateSubProfile = new HTMLProfiler(sublistingTemplate, bSubAhrefs, strSubStart, strSubEnd);
							}
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
				ProgramData guideData =	guideProfile.GetProgramData(index); //m_templateParser.GetProgram(Listing);

				if(guideData == null || guideData.StartTime == null || guideData.Title == "")
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
					if (guideData.Day != m_StartGrab.Day)
					{
						m_GrabDay++;
						m_StartGrab = m_StartGrab.AddDays(1);
						m_bNextDay=false;
						m_LastStart=0;
					}
				}

				if(!m_bNextDay && m_LastStart > guideData.StartTime[0])
					m_bNextDay=true;

				DateTime dtStart = new DateTime(m_StartGrab.Year, month, guideData.Day, guideData.StartTime[0], guideData.StartTime[1], 0, 0);
				if(m_bNextDay)
					dtStart = dtStart.AddDays(1);
				program.Start = GetLongDateTime(dtStart);
				m_LastStart = guideData.StartTime[0];

				if (guideData.EndTime != null)
				{
					if(!m_bNextDay && guideData.StartTime[0] > guideData.EndTime[0])
						m_bNextDay=true;
					DateTime dtEnd = new DateTime(m_StartGrab.Year, month, guideData.Day, guideData.EndTime[0], guideData.EndTime[1], 0, 0);
					if(m_bNextDay)
						dtEnd = dtEnd.AddDays(1);
					program.End = GetLongDateTime(dtEnd);
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
					string strLinkURL = htmlProf.GetHyperLink(index, m_SubListingLink);
//					source = guideProfile.GetSource(index);
//
//					int pos=0;
//					string strLinkURL="";
//					while((pos = source.IndexOf("<a href=", pos))!=-1)
//					{
//						pos+=9;
//						int endIndex = source.IndexOf("\"", pos);
//						if(endIndex != -1)
//						{
//							strLinkURL = source.Substring(pos, endIndex-pos);
//							if(strLinkURL.IndexOf(m_SubListingLink) != -1)
//								break;
//						}
//						strLinkURL="";
//
//					}
					if(strLinkURL != "")
					{
						string link = m_strURLbase;
						if(strLinkURL.ToLower().IndexOf("http") != -1)
							link = strLinkURL;
						else
							link += strLinkURL;
						Log.WriteFile(Log.LogType.Log, false, "WebEPG: Reading {0}", link);
						Thread.Sleep(m_grabDelay);
						Profiler SubProfile = m_templateSubProfile.GetPageProfiler(link); 
						int Count = SubProfile.subProfileCount(); 

						if(Count > 0)
						{
							ProgramData SubData = SubProfile.GetProgramData(0);
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

            private bool GetListing(string strURL, int offset)
            {
				Profiler guideProfile;
                bool bMore = false;
                int listingCount;

                strURL = strURL.Replace("#LIST_OFFSET", offset.ToString());

                Log.WriteFile(Log.LogType.Log, false, "WebEPG: Reading {0}{1}", m_strURLbase, strURL);

				guideProfile = m_templateProfile.GetPageProfiler(m_strURLbase + strURL); 
				listingCount = guideProfile.subProfileCount();

				if(listingCount == 0)
				{
					Log.WriteFile(Log.LogType.Log, true, "WebEPG: No Listings Found");
					m_GrabDay++;
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
					m_bNextDay=false;

                    while (GetListing(strURL, offset))
                    {
						Thread.Sleep(m_grabDelay);
                        if (m_maxListingCount == 0)
                            break;
                        offset += m_maxListingCount;
                    }

                    if (strURL != strURLid)
                    {
                        m_StartGrab = m_StartGrab.AddDays(1);
                        m_GrabDay++;
                    }
                }
             
                return m_programs;
            }
        }
}
