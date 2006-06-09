/*
  *	Copyright (C) 2005-2006 Team MediaPortal
  *	http://www.team-mediaportal.com
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
using System.Collections;
using MediaPortal.Webepg.Profile;
using MediaPortal.TV.Database;
using MediaPortal.WebEPG;
using MediaPortal.Utils;
using MediaPortal.Utils.Services;

namespace MediaPortal.EPG
{
  public class WebEPG
  {
    MediaPortal.Webepg.Profile.Xml m_xmlreader;
    WebListingGrabber m_EPGGrabber;

    struct GrabberInfo
    {
      public string id;
      public string name;
      public string[] grabber;
      public string[] mergeId;
      public bool[] Linked;
      public int[] linkStart;
      public int[] linkEnd;
      public ProgramDateTime[] mergeStart;
      public ProgramDateTime[] mergeEnd;
      public int copies;
      public bool iscopy;
      public int mergedChannelsCount;
    }

    public WebEPG()
    {
    }

    public bool Import()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      ILog log = services.Get<ILog>();

      string grabberLast = "";
      string grabberDir;
      bool initResult = false;
      int maxGrabDays;
      int channelCount;
      int programCount;

      string configFile = Environment.CurrentDirectory + "\\WebEPG\\WebEPG.xml";

      if (!File.Exists(configFile))
      {
        log.Info("File not found: WebEPG.xml");
        return false;
      }

      //TVProgram program;
      ArrayList programs = new ArrayList();
      ArrayList programsArray = new ArrayList();
      ArrayList channels = new ArrayList();
      Hashtable idList = new Hashtable();
      XMLTVExport xmltv = new XMLTVExport(Environment.CurrentDirectory + "\\xmltv\\");

      xmltv.Open();

      log.Info("Loading ChannelMap: WebEPG.xml");

      m_xmlreader = new MediaPortal.Webepg.Profile.Xml(configFile);
      maxGrabDays = m_xmlreader.GetValueAsInt("General", "MaxDays", 1);
      grabberDir = m_xmlreader.GetValueAsString("General", "GrabberDir", Environment.CurrentDirectory + "\\WebEPG\\grabbers\\");
      m_EPGGrabber = new WebListingGrabber(maxGrabDays, grabberDir);

      int AuthCount = m_xmlreader.GetValueAsInt("AuthSites", "Count", 0);

      for (int i = 1; i <= AuthCount; i++)
      {
        string site = m_xmlreader.GetValueAsString("Auth" + i.ToString(), "Site", "");
        string login = m_xmlreader.GetValueAsString("Auth" + i.ToString(), "Login", "");
        string password = m_xmlreader.GetValueAsString("Auth" + i.ToString(), "Password", "");
        NetworkCredential auth = new NetworkCredential(login, password);
        MediaPortal.Utils.Web.HTTPAuth.Add(site, auth);
      }

      channelCount = m_xmlreader.GetValueAsInt("ChannelMap", "Count", 0);

      for (int i = 1; i <= channelCount; i++)
      {
        GrabberInfo channel = new GrabberInfo();
        channel.copies = 0;
        channel.iscopy = false;
        channel.name = m_xmlreader.GetValueAsString(i.ToString(), "DisplayName", "");
        channel.mergedChannelsCount = m_xmlreader.GetValueAsInt(i.ToString(), "mergedChannelsCount", 1);
        channel.id = m_xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
        // create the needed arrays with the needed number of elements
        channel.mergeId = new string[channel.mergedChannelsCount];
        channel.grabber = new string[channel.mergedChannelsCount];
        channel.Linked = new bool[channel.mergedChannelsCount];
        channel.linkStart = new int[channel.mergedChannelsCount];
        channel.linkEnd = new int[channel.mergedChannelsCount];
        channel.mergeStart = new ProgramDateTime[channel.mergedChannelsCount];
        for (int x = 0; x < channel.mergedChannelsCount; x++)
        {
          channel.mergeStart[x] = new ProgramDateTime();
        }
        channel.mergeEnd = new ProgramDateTime[channel.mergedChannelsCount];
        for (int x = 0; x < channel.mergedChannelsCount; x++)
        {
          channel.mergeEnd[x] = new ProgramDateTime();
        }
        //
        for (int j = 0; j < channel.mergedChannelsCount; j++)
        {
          int _grabnum = j + 1;
          string strGrabberNum = _grabnum.ToString();
          ProgramData timeGrabber = new ProgramData(); // only used for conversion purpose
          channel.mergeId[j] = m_xmlreader.GetValueAsString(i.ToString(), "mergeChannelID" + strGrabberNum, "");
          timeGrabber.SetElement("<#START>", m_xmlreader.GetValueAsString(i.ToString(), "mergeStart" + strGrabberNum, "00:00"));
          channel.mergeStart[j].Hour = timeGrabber.StartTime.Hour;
          channel.mergeStart[j].Minute = timeGrabber.StartTime.Minute;
          timeGrabber.SetElement("<#START>", m_xmlreader.GetValueAsString(i.ToString(), "mergeEnd" + strGrabberNum, "00:00"));
          channel.mergeEnd[j].Hour = timeGrabber.StartTime.Hour;
          channel.mergeEnd[j].Minute = timeGrabber.StartTime.Minute;
          channel.grabber[j] = m_xmlreader.GetValueAsString(i.ToString(), "Grabber" + strGrabberNum, "");
          channel.Linked[j] = m_xmlreader.GetValueAsBool(i.ToString(), "Grabber" + strGrabberNum + "-Linked", false);
          if (channel.Linked[j])
          {
            channel.linkStart[j] = m_xmlreader.GetValueAsInt(i.ToString(), "Grabber" + strGrabberNum + "-Start", 18);
            channel.linkEnd[j] = m_xmlreader.GetValueAsInt(i.ToString(), "Grabber" + strGrabberNum + "-End", 23);
          }
        }
        object obj = idList[channel.id];
        if (obj == null)
        {
          idList.Add(channel.id, i - 1);
        }
        else
        {
          int pos = (int)obj;
          GrabberInfo orig = (GrabberInfo)channels[pos];
          orig.copies++;
          channel.id += orig.copies.ToString();
          channel.copies = pos;
          channel.iscopy = true;
          channels.RemoveAt(pos);
          channels.Insert(pos, orig);
        }
        channels.Add(channel);
        xmltv.WriteChannel(channel.id, channel.name);
      }

      for (int i = 1; i <= channelCount; i++)
      {
        log.Info("WebEPG: Getting Channel {0} of {1}", i, channelCount);
        GrabberInfo channel = (GrabberInfo)channels[i - 1];

        if (channel.iscopy)
        {
          log.Info("WebEPG: Channel is a copy of Channel {0}", channel.copies + 1);

        }
        else
        {
          for (int j = 0; j < channel.mergedChannelsCount; j++)
          {
            if (channel.grabber[j] != grabberLast)
              initResult = m_EPGGrabber.Initalise(channel.grabber[j]);

            grabberLast = channel.grabber[j];

            if (initResult)
            {
              if (channel.mergedChannelsCount > 1)
              {
                programsArray.Add(m_EPGGrabber.GetGuide(channel.mergeId[j], channel.Linked[j], channel.linkStart[j], channel.linkEnd[j]));
              }
              else
              {
                programsArray.Add(m_EPGGrabber.GetGuide(channel.id, channel.Linked[j], channel.linkStart[j], channel.linkEnd[j]));
              }
              programs = (ArrayList)programsArray[j];

              // merge programs
              mergePrograms(ref programs, ref channel, j);

              if (programs != null)
              {
                programCount = programs.Count;
                for (int c = 0; c <= channel.copies; c++)
                {
                  for (int p = 0; p < programCount; p++)
                  {
                    xmltv.WriteProgram((TVProgram)programs[p], c);
                  }
                }
              }
            }
            else
            {
              log.Info("WebEPG: Grabber failed for: {0}", channel.name);
            }
          }
          programsArray.Clear();
        }

      }

      xmltv.Close();

      return true;

    }
    private void mergePrograms(ref ArrayList programs, ref GrabberInfo channel, int channelOffset)
    {
      // if the channel is merged, remove unneeded programs regarding to their time
      int _index = 0;
      bool done = false;
      int _count = programs.Count;
      while (!done)
      { 
        TVProgram prog = (TVProgram)programs[_index];
        prog.Channel = channel.id;
        programs[_index] = prog; //apply changes
        if (channel.mergedChannelsCount > 1) 
        {
          int _progStart = prog.StartTime.Hour * 10000 + prog.StartTime.Minute*100;
          int _progEnd = prog.EndTime.Hour * 10000 + prog.EndTime.Minute*100;
          int _mergeStart = channel.mergeStart[channelOffset].Hour * 10000 + channel.mergeStart[channelOffset].Minute * 100;
          int _mergeEnd = channel.mergeEnd[channelOffset].Hour * 10000 + channel.mergeEnd[channelOffset].Minute * 100;
          if (_mergeEnd <= _mergeStart) _mergeEnd += 240000; // ie start=23:00, end 1:00  
          if (_progEnd <= _progStart) _progEnd += 240000; // add 24h for testing purpose
          if (_progStart < _mergeStart || _progStart >= _mergeEnd) // program is out of time bounds
          {
            programs.RemoveAt(_index); // then remove it
            _count -= 1;
            _index -= 1;
          }
          if (_progStart >= _mergeStart && _progStart <= _mergeEnd && _progEnd > _mergeEnd)
          { // sets program end to mergeEnd
            prog.End = prog.End / 1000000 + channel.mergeEnd[channelOffset].Hour * 10000 + channel.mergeEnd[channelOffset].Minute * 100;
            programs[_index] = prog;
           
          }
        }
          _index += 1;
          if (_index == _count-1) done = true;
      }

    }
  }
}
