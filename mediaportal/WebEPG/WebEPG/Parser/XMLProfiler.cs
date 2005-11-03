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
//using System.Collections.Generic;
using System.Text;
using System.Xml;
using MediaPortal.Utils.Web;
using MediaPortal.Webepg.GUI.Library;
using MediaPortal.Webepg.TV.Database;

namespace MediaPortal.WebEPG
{
	public class XMLProfiler : Profiler
	{
		XMLProfilerData m_Data;
		//XmlDocument m_xmlDoc;
		XmlNodeList m_nodeList;
		string m_strURL="";

		public XMLProfiler(string strSource, XMLProfilerData data)
		{
			m_strSource = strSource;
			m_Data = data;
			if(m_strSource != "")
				NodeProfiler();
		}

		public void SetChannelID(string id)
		{
			m_Data.ChannelID = id;
		}

		override public Profiler GetPageProfiler(string strURL)
		{
			if(strURL != m_strURL)
			{
				HTMLPage webPage = new HTMLPage(strURL);
				m_strSource = webPage.GetPage();
				m_strURL = strURL;
			}
			return new XMLProfiler(m_strSource, m_Data); 
		}

		override public MediaPortal.Utils.Web.Parser GetProfileParser(int index)
		{
			return null;
		}

		override public void GetParserData(int index, ref ParserData data)
		{
			ProgramData program = (ProgramData) data;

			XmlNode progNode = m_nodeList.Item(index);
			if(progNode != null)
			{
				XmlNode node;
				if(m_Data.TitleEntry != "" && (node = progNode.SelectSingleNode(m_Data.TitleEntry)) != null)
					program.Title = node.InnerText;

				if(m_Data.SubtitleEntry != "" && (node = progNode.SelectSingleNode(m_Data.SubtitleEntry)) != null)
					program.SubTitle = node.InnerText;

				if(m_Data.DescEntry != "" && (node = progNode.SelectSingleNode(m_Data.DescEntry)) != null)
					program.Description = node.InnerText;

				if(m_Data.GenreEntry != "" && (node = progNode.SelectSingleNode(m_Data.GenreEntry)) != null)
					program.Genre = node.InnerText;

				if(m_Data.StartEntry != "")
				{
					if((node = progNode.Attributes.GetNamedItem(m_Data.StartEntry)) != null)
						program.StartTime = GetDateTime(node.InnerText);

					if((node = progNode.SelectSingleNode(m_Data.StartEntry)) != null)
						program.StartTime = GetDateTime(node.InnerText);
				}

				if(m_Data.EndEntry != "")
				{
					if((node = progNode.Attributes.GetNamedItem(m_Data.EndEntry)) != null)
						program.EndTime = GetDateTime(node.InnerText);

					if((node = progNode.SelectSingleNode(m_Data.EndEntry)) != null)
						program.EndTime = GetDateTime(node.InnerText);
				}
			}
		}

		private int[] GetDateTime(string strDateTime)
		{
			int[] time = new int[2];
			if(strDateTime.Length > 4)
			{
				long ldate = long.Parse(strDateTime.Substring(0,14));
				ldate /=100L;
				time[1]=(int)(ldate%100L); 
				ldate /=100L;
				time[0]=(int)(ldate%100L);
			}
			else
			{
				int idate = int.Parse(strDateTime);
				time[1] = idate%100;
				time[0] = idate / 100;
			}
			return time;
		}

		private void NodeProfiler()
		{
			try
			{
				XmlDocument m_xmlDoc = new XmlDocument();
				m_xmlDoc.LoadXml(m_strSource);
				if(m_Data.ChannelEntry != "")
					m_nodeList =  m_xmlDoc.DocumentElement.SelectNodes(m_Data.XPath + "[@" + m_Data.ChannelEntry + "=\"" + m_Data.ChannelID + "\"]");
				else
					m_nodeList =  m_xmlDoc.DocumentElement.SelectNodes(m_Data.XPath);
			}
			catch(System.Xml.XmlException) // ex)
			{
				Log.WriteFile(Log.LogType.Log, true, "WebEPG: XML failed");
			}

			m_profileCount = m_nodeList.Count;
		}
    }

}
