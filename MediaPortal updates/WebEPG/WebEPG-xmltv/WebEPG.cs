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
using MediaPortal.Utils.Time;
using MediaPortal.Utils.Services;

namespace MediaPortal.EPG
{
  public class WebEPG
  {
    private MediaPortal.Webepg.Profile.Xml _xmlreader;
    private WebListingGrabber _epgGrabber;
    private ILog _log;
    private string _configFile;
    private string _xmltvDirectory;
    private ArrayList _channels = null;
    private MergeInfo[] _mergedList = null;
    private Hashtable _mergedChannels = new Hashtable();

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
      public TimeRange time;
      public ArrayList programs;
    }

    public WebEPG()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();

      _configFile = Environment.CurrentDirectory + "\\WebEPG\\WebEPG.xml";
      _xmltvDirectory = Environment.CurrentDirectory + "\\xmltv\\";
    }

    public bool Import()
    {
      if (!LoadConfig())
        return false;


      XMLTVExport xmltv = new XMLTVExport(_xmltvDirectory);

      xmltv.Open();

      for (int i = 0; i < _channels.Count; i++)
      {
        GrabberInfo channel = (GrabberInfo)_channels[i];
        if (!_mergedChannels.Contains(channel.id))
          xmltv.WriteChannel(channel.id, channel.name);
      }

      if (_mergedList != null)
      {
        for (int i = 0; i < _mergedList.Length; i++)
        {
          xmltv.WriteChannel("merged" + i.ToString(), _mergedList[i].name);
        }
      }

      string grabberLast = "";
      ArrayList programs;
      ArrayList mergedPrograms;
      bool initResult = false;

      for (int i = 0; i < _channels.Count; i++)
      {
        _log.Info("WebEPG: Getting Channel {0} of {1}", i + 1, _channels.Count);
        GrabberInfo channel = (GrabberInfo)_channels[i];

        if (channel.iscopy)
        {
          _log.Info("WebEPG: Channel is a copy of Channel {0}", channel.copies + 1);

        }
        else
        {
          if (channel.grabber != grabberLast)
            initResult = _epgGrabber.Initalise(channel.grabber);

          grabberLast = channel.grabber;

          if (initResult)
          {
            programs = _epgGrabber.GetGuide(channel.id, channel.Linked, channel.linkStart, channel.linkEnd);
            if (programs != null)
            {
              if (channel.isMerged)
              {
                MergeChannelLocation mergedPos = (MergeChannelLocation)_mergedChannels[channel.id];
                _mergedList[mergedPos.mergeNum].channels[mergedPos.mergeChannel].programs = programs;
              }
              else
              {
                for (int c = 0; c <= channel.copies; c++)
                {
                  for (int p = 0; p < programs.Count; p++)
                  {
                    xmltv.WriteProgram((TVProgram)programs[p], c);
                  }
                }
              }
            }
          }
          else
          {
            _log.Info("WebEPG: Grabber failed for: {0}", channel.name);
          }
        }
      }

      if (_mergedList != null && _mergedList.Length > 0)
      {
        for (int i = 0; i < _mergedList.Length; i++)
        {
          for (int c = 0; c < _mergedList[i].channels.Length; c++)
          {
            programs = _mergedList[i].channels[c].programs;
            for (int p = 0; p < programs.Count; p++)
            {
              TVProgram program = (TVProgram) programs[p];
              program.Channel = "merged" + i.ToString();
              if (_mergedList[i].channels[c].time.IsInRange(program.Start))
                xmltv.WriteProgram(program, 0);
            }
          }
        }
      }

      xmltv.Close();

      return true;

    }

    private bool LoadConfig()
    {
      string grabberDir;
      int maxGrabDays;
      int channelCount;

      if (!File.Exists(_configFile))
      {
        _log.Info("File not found: WebEPG.xml");
        return false;
      }

      Hashtable idList = new Hashtable();

      _channels = new ArrayList();

      _log.Info("Loading ChannelMap: WebEPG.xml");

      _xmlreader = new MediaPortal.Webepg.Profile.Xml(_configFile);
      maxGrabDays = _xmlreader.GetValueAsInt("General", "MaxDays", 1);
      grabberDir = _xmlreader.GetValueAsString("General", "GrabberDir", Environment.CurrentDirectory + "\\WebEPG\\grabbers\\");
      _epgGrabber = new WebListingGrabber(maxGrabDays, grabberDir);

      int AuthCount = _xmlreader.GetValueAsInt("AuthSites", "Count", 0);

      for (int i = 1; i <= AuthCount; i++)
      {
        string site = _xmlreader.GetValueAsString("Auth" + i.ToString(), "Site", "");
        string login = _xmlreader.GetValueAsString("Auth" + i.ToString(), "Login", "");
        string password = _xmlreader.GetValueAsString("Auth" + i.ToString(), "Password", "");
        NetworkCredential auth = new NetworkCredential(login, password);
        MediaPortal.Utils.Web.HTTPAuth.Add(site, auth);
      }

      int MergeCount = _xmlreader.GetValueAsInt("MergeChannels", "Count", 0);

      if (MergeCount > 0)
      {
        _mergedList = new MergeInfo[MergeCount];

        for (int i = 1; i <= MergeCount; i++)
        {
          MergeInfo merged = new MergeInfo();
          merged.count = _xmlreader.GetValueAsInt("Merge" + i.ToString(), "Channels", 0);
          merged.name = _xmlreader.GetValueAsString("Merge" + i.ToString(), "DisplayName", "");
          merged.channels = new MergeChannelData[merged.count];
          for (int c = 1; c <= merged.count; c++)
          {
            string channelId = _xmlreader.GetValueAsString("Merge" + i.ToString(), "Channel" + c.ToString(), "");
            MergeChannelData mergeTime = new MergeChannelData();
            MergeChannelLocation location = new MergeChannelLocation();
            location.mergeChannel = c - 1;
            location.mergeNum = i - 1;
            string start = _xmlreader.GetValueAsString("Merge" + i.ToString(), "Start" + c.ToString(), "0:0");
            string end = _xmlreader.GetValueAsString("Merge" + i.ToString(), "End" + c.ToString(), "0:0");
            mergeTime.time = new TimeRange(start, end);
            merged.channels[c - 1] = mergeTime;
            _mergedChannels.Add(channelId, location);
          }
          _mergedList[i - 1] = merged;
        }
      }

      channelCount = _xmlreader.GetValueAsInt("ChannelMap", "Count", 0);

      for (int i = 1; i <= channelCount; i++)
      {
        GrabberInfo channel = new GrabberInfo();
        channel.copies = 0;
        channel.iscopy = false;
        channel.isMerged = false;
        channel.name = _xmlreader.GetValueAsString(i.ToString(), "DisplayName", "");
        channel.id = _xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
        channel.grabber = _xmlreader.GetValueAsString(i.ToString(), "Grabber1", "");
        channel.Linked = _xmlreader.GetValueAsBool(i.ToString(), "Grabber1-Linked", false);
        if (channel.Linked)
        {
          channel.linkStart = _xmlreader.GetValueAsInt(i.ToString(), "Grabber1-Start", 18);
          channel.linkEnd = _xmlreader.GetValueAsInt(i.ToString(), "Grabber1-End", 23);
        }

        object obj = idList[channel.id];
        if (obj == null)
        {
          idList.Add(channel.id, i - 1);
        }
        else
        {
          int pos = (int)obj;
          GrabberInfo orig = (GrabberInfo)_channels[pos];
          orig.copies++;
          channel.id += orig.copies.ToString();
          channel.copies = pos;
          channel.iscopy = true;
          _channels.RemoveAt(pos);
          _channels.Insert(pos, orig);
        }

        if (_mergedChannels.Contains(channel.id))
          channel.isMerged = true;

        _channels.Add(channel);
      }

      return true;
    }
  }
}
