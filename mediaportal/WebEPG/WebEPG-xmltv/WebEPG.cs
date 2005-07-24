using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Collections;
using MediaPortal.Util;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.EPG
{
	public class WebEPG
	{
		MediaPortal.Profile.Xml m_xmlreader;
		WebListingGrabber m_EPGGrabber;

		struct GrabberInfo
		{
			public string id;
			public string name;
			public string grabber;
			public bool Linked;
			public int linkStart;
			public int linkEnd;
		}

		public WebEPG()
		{
		}

		public bool Import()
		{

			string grabberLast = "";
			string grabberDir;
			bool initResult = false;
			int maxGrabDays;
			int channelCount;
			int programCount;

			if (!File.Exists("WebEPG.xml")) 
			{
				Log.WriteFile(Log.LogType.Log, false, "File not found: WebEPG.xml");
				return false;
			}

			//TVProgram program;
			ArrayList programs;
			ArrayList channels = new ArrayList();
			XMLTVExport xmltv = new XMLTVExport();

			xmltv.Open();

			Log.WriteFile(Log.LogType.Log, false, "Loading ChannelMap: WebEPG.xml");

			m_xmlreader = new MediaPortal.Profile.Xml("WebEPG.xml");
			maxGrabDays = m_xmlreader.GetValueAsInt("General", "MaxDays", 1);
			grabberDir = m_xmlreader.GetValueAsString("General", "GrabberDir", Environment.CurrentDirectory + "\\grabbers\\");
			m_EPGGrabber = new WebListingGrabber(maxGrabDays, grabberDir);
           
			channelCount = m_xmlreader.GetValueAsInt("ChannelMap", "Count", 0);

			for (int i = 1; i <= channelCount; i++)
			{
				GrabberInfo channel = new GrabberInfo();
				channel.id = m_xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
				channel.name = m_xmlreader.GetValueAsString(i.ToString(), "DisplayName", "");
				channel.grabber = m_xmlreader.GetValueAsString(i.ToString(), "Grabber1", "");
				channel.Linked = m_xmlreader.GetValueAsBool(i.ToString(), "Grabber1-Linked", false);
				if(channel.Linked)
				{
					channel.linkStart = m_xmlreader.GetValueAsInt(i.ToString(), "Grabber1-Start", 18);
					channel.linkEnd = m_xmlreader.GetValueAsInt(i.ToString(), "Grabber1-End", 23);
				}
				channels.Add(channel);
				xmltv.WriteChannel(channel.id, channel.name);
			}

			for (int i = 1; i <= channelCount; i++)
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG: Getting Channel {0} of {1}", i, channelCount);
				GrabberInfo channel = (GrabberInfo) channels[i-1];

				if(channel.grabber != grabberLast)
					initResult = m_EPGGrabber.Initalise(channel.grabber);

				grabberLast = channel.grabber;

				if (initResult)
				{
					programs = m_EPGGrabber.GetGuide(channel.id, channel.Linked, channel.linkStart, channel.linkEnd);
					if(programs != null )
					{
						programCount = programs.Count;
						for (int p = 0; p < programCount; p++)
						{
							xmltv.WriteProgram((TVProgram) programs[p]);
						}
					}
				}
				else
				{
					Log.WriteFile(Log.LogType.Log, false, "WebEPG: Grabber failed for: {0}", channel.name);
				}

			}

			xmltv.Close();

			return true;
        
		}
	}  
}
