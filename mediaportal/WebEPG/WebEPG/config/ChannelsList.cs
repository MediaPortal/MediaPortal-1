/*
  *	Copyright (C) 2005 Team MediaPortal
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
using System.Xml;
using System.Collections;
using MediaPortal.Webepg.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.EPG.config
{
  /// <summary>
  /// Summary description for config.
  /// </summary>
  public class ChannelsList : IComparer
  {
    string _strGrabberDir;
    string _strChannelsFile;
    const int MAX_COST = 2;

    SortedList _ChannelList = null;
    string _Country;

    public ChannelsList(string BaseDir)
    {
      _strGrabberDir = BaseDir + "\\grabbers\\";
      _strChannelsFile = BaseDir + "\\channels\\channels.xml";
    }

    public string[] GetCountries()
    {
      string[] countryList = null;

      if(System.IO.Directory.Exists(_strGrabberDir))
      {
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(_strGrabberDir);
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

      return _ChannelList;
    }


    public SortedList GetChannelList(string country)
    {
      LoadChannels(country);

      if(_ChannelList != null)
        LoadGrabbers(country);

      return _ChannelList;
    }

    public ArrayList GetChannelArrayList(string country)
    {
      //ChannelInfo[] ChannelArray = null;
      ArrayList ChannelArray = null;

      GetChannelList(country);

      if(_ChannelList != null)
      {
        IDictionaryEnumerator Enumerator = _ChannelList.GetEnumerator();

        //ChannelArray = new ChannelInfo[_ChannelList.Count];
        //int i=0;

        ChannelArray = new ArrayList();

        while (Enumerator.MoveNext())
        {
          ChannelInfo channel = (ChannelInfo) Enumerator.Value;
          //ChannelArray[i++] = channel;
          ChannelArray.Add(channel);
        }

        ChannelArray.Sort(this);
      }

      return ChannelArray;
    }

    public int FindChannel(string Name, string country)
    {
      ArrayList channels = GetChannelArrayList(country);

      int retChan = -1;

      if (channels == null)
        return retChan;

      ChannelInfo ch;

      int bestCost;
      int cost;

      bestCost = MAX_COST + 1;
      for (int i = 0; i < channels.Count; i++)
      {
        ch = (ChannelInfo)channels[i];
        if (Name == ch.FullName)
        {
          retChan = i;
          break;
        }

        if (ch.FullName.IndexOf(Name) != -1)
        {
          retChan = i;
          break;
        }

        cost = Levenshtein.Match(Name, ch.FullName);

        if (cost < bestCost)
        {
          retChan = i;
          bestCost = cost;
        }
      }

      return retChan;
    }

    public ChannelInfo[] FindChannels(string[] NameList, string country)
    {
      ArrayList channels = GetChannelArrayList(country);

      ChannelInfo[] retList = new ChannelInfo[NameList.Length];

      if (channels == null)
        return retList;

      ChannelInfo ch;

      int bestCost;
      int cost;

      for(int k = 0; k < NameList.Length; k++)
      {
        bestCost = MAX_COST + 1;
        for(int i = 0; i < channels.Count; i++)
        {
          ch = (ChannelInfo) channels[i];
          if(NameList[k] == ch.FullName)
          {
            retList[k] = ch;
            break;
          }

          if (ch.FullName.IndexOf(NameList[k]) != -1)
          {
            retList[k] = ch;
            break;
          }

          cost = Levenshtein.Match(NameList[k], ch.FullName);

          if (cost < bestCost)
          {
            retList[k] = ch;
            bestCost = cost;
          }
        }
      }
      return retList;
    }

    private void LoadChannels(string country)
    {
      if (country != _Country)
        _ChannelList = new SortedList();
      else
        return;

      if(System.IO.File.Exists(_strChannelsFile))
      {
        //Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Loading Existing channels.xml");
        MediaPortal.Webepg.Profile.Xml xmlreader = new MediaPortal.Webepg.Profile.Xml(_strChannelsFile);
        int channelCount = xmlreader.GetValueAsInt(country, "TotalChannels", 0);

        if(channelCount > 0)
        {
          if(_ChannelList == null)
            _ChannelList = new SortedList();

          for (int ich = 0; ich < channelCount; ich++)
          {
            int channelIndex = xmlreader.GetValueAsInt(country, ich.ToString(), 0);
            ChannelInfo channel = new ChannelInfo();
            channel.ChannelID = xmlreader.GetValueAsString(channelIndex.ToString(), "ChannelID", "");
            channel.FullName = xmlreader.GetValueAsString(channelIndex.ToString(), "FullName", "");
            if(channel.FullName == "")
              channel.FullName = channel.ChannelID;
            if(channel.ChannelID != "")
              _ChannelList.Add(channel.ChannelID, channel);
          }

          _Country = country;
        }
      }
    }

    private void LoadAllChannels()
    {
      if(System.IO.File.Exists(_strChannelsFile))
      {
        //Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Loading Existing channels.xml");
        MediaPortal.Webepg.Profile.Xml xmlreader = new MediaPortal.Webepg.Profile.Xml(_strChannelsFile);
        int channelCount = xmlreader.GetValueAsInt("ChannelInfo", "TotalChannels", 0);

        if(channelCount > 0)
        {
          if(_ChannelList == null)
            _ChannelList = new SortedList();

          for (int ich = 0; ich < channelCount; ich++)
          {
            ChannelInfo channel = new ChannelInfo();
            channel.ChannelID = xmlreader.GetValueAsString(ich.ToString(), "ChannelID", "");
            channel.FullName = xmlreader.GetValueAsString(ich.ToString(), "FullName", "");
            if(channel.FullName == "")
              channel.FullName = channel.ChannelID;
            if(channel.ChannelID != "" && _ChannelList[channel.ChannelID] == null)
              _ChannelList.Add(channel.ChannelID, channel);
          }
        }
      }
    }


    private void LoadGrabbers(string country)
    {
      if(System.IO.Directory.Exists(_strGrabberDir + country) && _ChannelList != null)
      {
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(_strGrabberDir + country);
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

                ChannelInfo info = (ChannelInfo) _ChannelList[id.InnerXml];
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
                  _ChannelList.Add(info.ChannelID, info);
                }
              }
            }
          }
        }
      }
    }
    #region IComparer Members

    public int Compare(object x, object y)
    {
      ChannelInfo ch1 = (ChannelInfo) x;
      ChannelInfo ch2 = (ChannelInfo) y;
      return String.Compare(ch1.FullName, ch2.FullName, true);
    }

    #endregion
  }
}
