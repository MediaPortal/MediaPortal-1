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
      public string grabber;
      public bool Linked;
      public int linkStart;
      public int linkEnd;
      public int copies;
      public bool iscopy;
      public bool isMerged;
    }

    struct MergeChannelLocation
    {
      public int mergeNum;
      public int mergeChannel;
    }

    struct MergeInfo
    {
      public int count;
      public string name;
      public MergeChannelData[] channels;
    }

    struct MergeChannelData
    {
      public int startHour;
      public int startMin;
      public int endHour;
      public int endMin;
      public ArrayList programs;
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
      ArrayList programs;
      ArrayList channels = new ArrayList();
      MergeInfo[] mergedList = null;
      Hashtable mergedChannels = new Hashtable();
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

      int MergeCount = m_xmlreader.GetValueAsInt("MergeChannels", "Count", 0);

      if (MergeCount > 0)
      {
        mergedList = new MergeInfo[MergeCount];

        for (int i = 1; i <= MergeCount; i++)
        {
          MergeInfo merged = new MergeInfo();
          merged.count = m_xmlreader.GetValueAsInt("Merge" + i.ToString(), "Channels", 0);
          merged.name = m_xmlreader.GetValueAsString("Merge" + i.ToString(), "DisplayName", "");
          merged.channels = new MergeChannelData[merged.count];
          for (int c = 1; c <= merged.count; c++)
          {
            string channelId = m_xmlreader.GetValueAsString("Merge" + i.ToString(), "Channel" + c.ToString(), "");
            MergeChannelData mergeTime = new MergeChannelData();
            MergeChannelLocation location = new MergeChannelLocation();
            location.mergeChannel = c;
            location.mergeNum = i;
            mergeTime.startHour = m_xmlreader.GetValueAsInt("Merge" + i.ToString(), "StartHour" + c.ToString(), 0); ;
            mergeTime.startMin = m_xmlreader.GetValueAsInt("Merge" + i.ToString(), "StartMin" + c.ToString(), 0); ;
            mergeTime.endHour = m_xmlreader.GetValueAsInt("Merge" + i.ToString(), "EndHour" + c.ToString(), 0); ;
            mergeTime.endMin = m_xmlreader.GetValueAsInt("Merge" + i.ToString(), "EndMin" + c.ToString(), 0); ;
            merged.channels[c] = mergeTime;
            mergedChannels.Add(channelId, location);
          }
          mergedList[i] = merged;
        }
      }

      channelCount = m_xmlreader.GetValueAsInt("ChannelMap", "Count", 0);

      for (int i = 1; i <= channelCount; i++)
      {
        GrabberInfo channel = new GrabberInfo();
        channel.copies = 0;
        channel.iscopy = false;
        channel.isMerged = false;
        channel.name = m_xmlreader.GetValueAsString(i.ToString(), "DisplayName", "");
        channel.id = m_xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
        channel.grabber = m_xmlreader.GetValueAsString(i.ToString(), "Grabber1", "");
        channel.Linked = m_xmlreader.GetValueAsBool(i.ToString(), "Grabber1-Linked", false);
        if(channel.Linked)
        {
          channel.linkStart = m_xmlreader.GetValueAsInt(i.ToString(), "Grabber1-Start", 18);
          channel.linkEnd = m_xmlreader.GetValueAsInt(i.ToString(), "Grabber1-End", 23);
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

        MergeChannelLocation mergedPos;
        if (mergedChannels.Contains(channel.id))
        {
          mergedPos = (MergeChannelLocation)mergedChannels[channel.id];
          channel.isMerged = true;
          xmltv.WriteChannel("merged"+ mergedPos.mergeNum.ToString(), mergedList[mergedPos.mergeNum].name);
        }
        else
        {
          xmltv.WriteChannel(channel.id, channel.name);
        }
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
          if(channel.grabber != grabberLast)
            initResult = m_EPGGrabber.Initalise(channel.grabber);

          grabberLast = channel.grabber;

          if (initResult)
          {
            programs = m_EPGGrabber.GetGuide(channel.id, channel.Linked, channel.linkStart, channel.linkEnd);
            if(programs != null )
            {
              if (channel.isMerged)
              {
                MergeChannelLocation mergedPos = (MergeChannelLocation)mergedChannels[channel.id];
                mergedList[mergedPos.mergeNum].channels[mergedPos.mergeChannel].programs = programs;
              }
              else
              {
                programCount = programs.Count;  // is this neeeded?
                for (int c = 0; c <= channel.copies; c++)
                {
                  for (int p = 0; p < programCount; p++)
                  {
                    xmltv.WriteProgram((TVProgram)programs[p], c);
                  }
                }
              }
            }
          }
          else
          {
            log.Info("WebEPG: Grabber failed for: {0}", channel.name);
          }
        }

      }

      xmltv.Close();

      return true;

    }

    private void mergePrograms(ref ArrayList programs, ref GrabberInfo channel, int channelOffset)
    {
      // if the channel is merged, remove unneeded programs regarding to their time
      //int _index = 0;
      //bool done = false;
      //int _count = programs.Count;
      //while (!done)
      //{ 
      //  TVProgram prog = (TVProgram)programs[_index];
      //  prog.Channel = channel.id;
      //  programs[_index] = prog; //apply changes
      //  if (channel.mergedChannelsCount > 1) 
      //  {
      //    int _progStart = prog.StartTime.Hour * 10000 + prog.StartTime.Minute*100;
      //    int _progEnd = prog.EndTime.Hour * 10000 + prog.EndTime.Minute*100;
      //    int _mergeStart = channel.mergeStart[channelOffset].Hour * 10000 + channel.mergeStart[channelOffset].Minute * 100;
      //    int _mergeEnd = channel.mergeEnd[channelOffset].Hour * 10000 + channel.mergeEnd[channelOffset].Minute * 100;
      //    if (_mergeEnd <= _mergeStart) _mergeEnd += 240000; // ie start=23:00, end 1:00  
      //    if (_progEnd <= _progStart) _progEnd += 240000; // add 24h for testing purpose
      //    if (_progStart < _mergeStart || _progStart >= _mergeEnd) // program is out of time bounds
      //    {
      //      programs.RemoveAt(_index); // then remove it
      //      _count -= 1;
      //      _index -= 1;
      //    }
      //    if (_progStart >= _mergeStart && _progStart <= _mergeEnd && _progEnd > _mergeEnd)
      //    { // sets program end to mergeEnd
      //      prog.End = 0;
      //      programs[_index] = prog;
           
      //    }
      //  }
      //    _index += 1;
      //    if (_index == _count) done = true;
      //}

    }
  }
}
