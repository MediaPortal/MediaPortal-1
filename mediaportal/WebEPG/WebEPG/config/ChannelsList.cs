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
using System.Xml;
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.EPG.config
{
	/// <summary>
	/// Summary description for config.
	/// </summary>
	public class ChannelsList
	{
		string m_strGrabberDir;
		string m_strChannelsFile;

		SortedList m_ChannelList = null;

		public ChannelsList(string BaseDir)
		{
			m_strGrabberDir = BaseDir + "\\grabbers\\";
			m_strChannelsFile = BaseDir + "\\channels\\channels.xml";
		}

		public string[] GetCountries()
		{
			string[] countryList = null;

			if(System.IO.Directory.Exists(m_strGrabberDir))
			{
				System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(m_strGrabberDir); 
				System.IO.DirectoryInfo[] dirList = dir.GetDirectories();
				if(dirList.Length > 0)
				{
					countryList = new string[dirList.Length];
					for(int i=0; i < dirList.Length; i++)
					{     
						//LOAD FOLDERS
						System.IO.DirectoryInfo g = dirList[i];
						countryList[i] = g.Name;
					}
				}
			}
			return countryList;
		}


		public SortedList GetChannelsList()
		{
			string[] CountryList = GetCountries();

			LoadAllChannels();
			for(int c=0; c < CountryList.Length; c++)
				LoadGrabbers(CountryList[c]);

			return m_ChannelList;
		}


		public SortedList GetChannelList(string country)
		{
			LoadChannels(country);

			if(m_ChannelList != null)
				LoadGrabbers(country);

			return m_ChannelList;
		}

		public ChannelInfo[] GetChannelArray(string country)
		{
			ChannelInfo[] ChannelArray = null;

			GetChannelList(country);
			
			if(m_ChannelList != null)
			{
				IDictionaryEnumerator Enumerator = m_ChannelList.GetEnumerator();

				ChannelArray = new ChannelInfo[m_ChannelList.Count];
				int i=0;

				while (Enumerator.MoveNext())
				{
					ChannelInfo channel = (ChannelInfo) Enumerator.Value;
					ChannelArray[i++] = channel;
				}
			}

			return ChannelArray;
		}

		private void LoadChannels(string country)
		{
			if(System.IO.File.Exists(m_strChannelsFile))
			{
				//Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Loading Existing channels.xml");
				MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(m_strChannelsFile);
				int channelCount = xmlreader.GetValueAsInt(country, "TotalChannels", 0);	

				if(channelCount > 0)
				{
					if(m_ChannelList == null)
						m_ChannelList = new SortedList();

					for (int ich = 0; ich < channelCount; ich++)
					{
						int channelIndex = xmlreader.GetValueAsInt(country, ich.ToString(), 0);
						ChannelInfo channel = new ChannelInfo();
						channel.ChannelID = xmlreader.GetValueAsString(channelIndex.ToString(), "ChannelID", "");
						channel.FullName = xmlreader.GetValueAsString(channelIndex.ToString(), "FullName", "");
						if(channel.FullName == "")
							channel.FullName = channel.ChannelID;
						if(channel.ChannelID != "")
							m_ChannelList.Add(channel.ChannelID, channel);
					}
				}
			}
		}

		private void LoadAllChannels()
		{
			if(System.IO.File.Exists(m_strChannelsFile))
			{
				//Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Loading Existing channels.xml");
				MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(m_strChannelsFile);
				int channelCount = xmlreader.GetValueAsInt("ChannelInfo", "TotalChannels", 0);	

				if(channelCount > 0)
				{
					if(m_ChannelList == null)
						m_ChannelList = new SortedList();

					for (int ich = 0; ich < channelCount; ich++)
					{
						ChannelInfo channel = new ChannelInfo();
						channel.ChannelID = xmlreader.GetValueAsString(ich.ToString(), "ChannelID", "");
						channel.FullName = xmlreader.GetValueAsString(ich.ToString(), "FullName", "");
						if(channel.FullName == "")
							channel.FullName = channel.ChannelID;
						if(channel.ChannelID != "" && m_ChannelList[channel.ChannelID] == null)
							m_ChannelList.Add(channel.ChannelID, channel);
					}
				}
			}
		}


		private void LoadGrabbers(string country)
		{
			if(System.IO.Directory.Exists(m_strGrabberDir + country) && m_ChannelList != null)
			{
			System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(m_strGrabberDir + country); 
			//Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Directory: {0}", Location);
			GrabberInfo gInfo;
				foreach (System.IO.FileInfo file in dir.GetFiles("*.xml"))
				{
					gInfo = new GrabberInfo();
					XmlDocument xml=new XmlDocument();
					XmlNodeList channelList;
					try 
					{
						//Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: File: {0}", file.Name);
						xml.Load(file.FullName);
						channelList = xml.DocumentElement.SelectNodes("/profile/section/entry");
  				
						XmlNode entryNode = xml.DocumentElement.SelectSingleNode("section[@name=\"Info\"]/entry[@name=\"GuideDays\"]");
						if (entryNode!=null)
							gInfo.GrabDays = int.Parse(entryNode.InnerText);
						entryNode = xml.DocumentElement.SelectSingleNode("section[@name=\"Info\"]/entry[@name=\"SiteDescription\"]");
						if (entryNode!=null)
							gInfo.SiteDesc = entryNode.InnerText;
						entryNode = xml.DocumentElement.SelectSingleNode("section[@name=\"Listing\"]/entry[@name=\"SubListingLink\"]");
						gInfo.Linked = false;
						if (entryNode!=null)
							gInfo.Linked = true;
					} 
					catch(System.Xml.XmlException) // ex) 
					{
						//Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: File open failed - XML error");
						return;
					}
				
					string GrabberSite = file.Name.Replace(".xml", "");
					GrabberSite = GrabberSite.Replace("_", ".");

					gInfo.GrabberID=file.Directory.Name + "\\" + file.Name;
					gInfo.GrabberName = GrabberSite;
					gInfo.Country = file.Directory.Name;
//					hGrabberInfo.Add(gInfo.GrabberID, gInfo);

//					if(CountryList[file.Directory.Name] == null)
//						CountryList.Add(file.Directory.Name, new SortedList());

					foreach (XmlNode nodeChannel in channelList)
					{
						if (nodeChannel.Attributes!=null)
						{
							XmlNode id = nodeChannel.ParentNode.Attributes.Item(0);
							if(id.InnerXml == "ChannelList")
							{
								id = nodeChannel.Attributes.Item(0);
								//idList.Add(id.InnerXml);

								ChannelInfo info = (ChannelInfo) m_ChannelList[id.InnerXml];
								if(info != null) // && info.GrabberList[gInfo.GrabberID] != null)
								{
									if(info.GrabberList == null)
										info.GrabberList = new SortedList();
									if(info.GrabberList[gInfo.GrabberID] == null)
										info.GrabberList.Add(gInfo.GrabberID, gInfo);
								}
								else
								{
									info = new ChannelInfo();
									info.ChannelID = id.InnerXml;
									info.FullName = info.ChannelID;
									info.GrabberList = new SortedList();
									info.GrabberList.Add(gInfo.GrabberID, gInfo);
									m_ChannelList.Add(info.ChannelID, info);
								}
							}
						}
					}
				}
			}
		}
	}
}
