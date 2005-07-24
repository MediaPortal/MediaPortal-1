using System;
//using System.Collections.Generic;
using System.Text;
using System.Xml;
using MediaPortal.WebEPGUtils;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.EPG
{
	public class XMLProfiler : Profiler
	{
		XMLProfilerData m_Data;
		XmlDocument m_xmlDoc;
		XmlNodeList m_nodeList;

		public XMLProfiler(string strSource, XMLProfilerData data)
		{
			m_strSource = strSource;
			m_Data = data;
			if(m_strSource != "")
				NodeProfiler();
		}

		override public Profiler GetPageProfiler(string strURL)
		{
			HTMLPage webPage = new HTMLPage(strURL, false);
			return new XMLProfiler(webPage.SubPage(), m_Data); 
		}

		override public Parser GetProfileParser(int index)
		{
//			Parser profileParser = new Parser(m_subProfile[index,1]*2 - 1);
//
//			int startTag = m_subProfile[index,0];
//			int sourceStart = this.m_arrayTagPos[startTag,0];
//			int sourceLength = this.m_arrayTagPos[startTag,1] - sourceStart + 1;
//			string element = PreProcess(this.m_strSource.Substring(sourceStart, sourceLength));
//			profileParser.Add(element);
//
//			for(int i=0; i < (m_subProfile[index,1] - 1); i++)
//			{
//				sourceStart = this.m_arrayTagPos[startTag+i, 1] + 1;
//				sourceLength = this.m_arrayTagPos[startTag+i+1, 0] - sourceStart;
//				element = PreProcess(this.m_strSource.Substring(sourceStart, sourceLength));
//				profileParser.Add(element);
//
//				sourceStart = this.m_arrayTagPos[startTag+i+1,0];
//				sourceLength = this.m_arrayTagPos[startTag+i+1,1] - sourceStart + 1;
//				element = PreProcess(this.m_strSource.Substring(sourceStart, sourceLength));
//				profileParser.Add(element);
//			}
//
//			return profileParser;
			return null;
		}

		override public ProgramData GetProgramData(int index)
		{
			ProgramData program = new ProgramData();

			XmlNode progNode = m_nodeList.Item(index);
			if(progNode != null)
			{
				XmlNode node;
				if((node = progNode.SelectSingleNode(m_Data.TitleEntry)) != null)
					program.Title = node.InnerText;

				if((node = progNode.SelectSingleNode(m_Data.SubtitleEntry)) != null)
					program.SubTitle = node.InnerText;

				if((node = progNode.SelectSingleNode(m_Data.DescEntry)) != null)
					program.Description = node.InnerText;

				if((node = progNode.SelectSingleNode(m_Data.GenreEntry)) != null)
					program.Genre = node.InnerText;

				if((node = progNode.SelectSingleNode(m_Data.StartEntry)) != null)
					program.StartTime = GetDateTime(node.InnerText);

				if((node = progNode.SelectSingleNode(m_Data.EndEntry)) != null)
					program.EndTime = GetDateTime(node.InnerText);
			}

			return program;

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
			//m_xmlDoc.Load("D:\\Zen\\EPG\\dev\\Sample-source\\d1-page.xml");
			try
			{
				XmlDocument m_xmlDoc = new XmlDocument();
				m_xmlDoc.LoadXml(m_strSource);
				m_nodeList =  m_xmlDoc.DocumentElement.SelectNodes(m_Data.XPath);
			}
			catch(System.Xml.XmlException ex)
			{
				Log.WriteFile(Log.LogType.Log, true, "WebEPG: XML failed");
			}

			m_profileCount = m_nodeList.Count;
		}
    }

}
