#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using MediaPortal.Utils.Time;
using MediaPortal.Utils.Web;
using MediaPortal.WebEPG;
using MediaPortal.WebEPG.config;

namespace MediaPortal.EPG
{
  /// <summary>
  /// Gets the EPG data from one or more web sites.
  /// </summary>
  public class WebEPG
  {
    #region Private Structs

    // hold information from config file.
    private struct grabInfo
    {
      public string id;
      public string name;
      public bool merged;
      public string grabber;
      public bool linked;
      public TimeRange linkTime;
    }

    #endregion

    #region Variables

    private WebListingGrabber _epgGrabber;
    private ILog _log;
    private string _configFile;
    private string _xmltvDirectory;
    private string _baseDirectory;
    private WebepgConfigFile _config;
    private Dictionary<string, List<grabInfo>> _grabList;

    #endregion

    #region Constructors/Destructors

    public WebEPG(string configFile, string xmltvDirectory, string baseDirectory)
    {
      // get logging service
      _log = GlobalServiceProvider.Get<ILog>();


      _log.Info(LogType.WebEPG, "Assembly versions:");
      _log.Info(LogType.WebEPG,
                this.GetType().Assembly.GetName().Name + " " + this.GetType().Assembly.GetName().Version.ToString());
      _log.Info(LogType.WebEPG,
                _log.GetType().Assembly.GetName().Name + " " + _log.GetType().Assembly.GetName().Version.ToString());
      // set config directories and files.
      _configFile = configFile;
      _xmltvDirectory = xmltvDirectory;
      _baseDirectory = baseDirectory;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the importation process
    /// </summary>
    /// <returns>bool - sucess/fail</returns>
    public bool Import()
    {
      if (!LoadConfig())
      {
        return false;
      }

      // Open XMLTV output file
      if (!Directory.Exists(_xmltvDirectory))
      {
        Directory.CreateDirectory(_xmltvDirectory);
      }

      XMLTVExport xmltv = new XMLTVExport(_xmltvDirectory);
      xmltv.Open();

      // Collect HttpStatistic
      IHttpStatistics httpStats;
      if (GlobalServiceProvider.IsRegistered<IHttpStatistics>())
      {
        httpStats = GlobalServiceProvider.Get<IHttpStatistics>();
      }
      else
      {
        httpStats = new HttpStatistics();
        GlobalServiceProvider.Add<IHttpStatistics>(httpStats);
      }

      _log.Info(LogType.WebEPG, "WebEPG: Loading Channel Config");
      _grabList = new Dictionary<string, List<grabInfo>>();
      // for each channel write info xmltv file.
      foreach (ChannelMap channel in _config.Channels)
      {
        if (channel.id == null && channel.merged == null)
        {
          _log.Warn(LogType.WebEPG, " Ignoring Channel Name: {0} - No Channel id", channel.displayName);
          continue;
        }

        if (channel.merged == null || channel.merged.Count == 0)
        {
          if (channel.grabber != null)
          {
            _log.Debug(LogType.WebEPG, " Loading Channel {0} ID: {1}", channel.displayName, channel.id);
            xmltv.WriteChannel(channel.id, channel.displayName);

            grabInfo grab = new grabInfo();
            grab.name = channel.displayName;
            grab.id = channel.id;
            grab.grabber = channel.grabber;
            grab.merged = false;
            grab.linked = true;
            grab.linkTime = new TimeRange("00:00", "23:00");

            if (!_grabList.ContainsKey(channel.id))
            {
              List<grabInfo> grabs = new List<grabInfo>();
              grabs.Add(grab);
              _grabList.Add(channel.id, grabs);
            }
            else
            {
              _grabList[channel.id].Add(grab);
            }
          }
          else
          {
            _log.Warn(LogType.WebEPG, " Ignoring Channel Name: {0} - No Grabber id", channel.displayName);
          }
        }
        else
        {
          _log.Debug(LogType.WebEPG, " Loading Merged Channel {0}", channel.displayName);
          xmltv.WriteChannel("[Merged]", channel.displayName);

          foreach (MergedChannel merged in channel.merged)
          {
            if (merged.grabber != null)
            {
              grabInfo grab = new grabInfo();
              grab.name = channel.displayName;
              grab.id = merged.id;
              grab.grabber = merged.grabber;
              grab.merged = true;
              grab.linked = true;
              grab.linkTime = new TimeRange(merged.start, merged.end);
              _log.Debug(LogType.WebEPG, "  Loading Merged Sub-channel: {0} Time range: {1}", merged.id,
                         grab.linkTime.ToString());

              if (!_grabList.ContainsKey(merged.id))
              {
                List<grabInfo> grabs = new List<grabInfo>();
                grabs.Add(grab);
                _grabList.Add(merged.id, grabs);
              }
              else
              {
                _grabList[merged.id].Add(grab);
              }
            }
            else
            {
              _log.Warn(LogType.WebEPG, "  Ignoring Merged Sub-channel: {0}/{1} - No Grabber id", channel.displayName,
                        merged.id);
            }
          }
        }
      }

      //// for each merged channel write info in xmltv file.
      //if (_mergedList != null)
      //{
      //  for (int i = 0; i < _mergedList.Length; i++)
      //  {
      //    xmltv.WriteChannel("merged" + i.ToString(), _mergedList[i].name);
      //  }
      //}


      string grabberLast = "";
      List<TVProgram> programs = null;
      bool initResult = false;
      _epgGrabber = new WebListingGrabber(_baseDirectory + "\\WebEPG\\grabbers\\");

      // For each channel get listing
      int i = 1;
      foreach (string channelid in _grabList.Keys)
      {
        _log.Info(LogType.WebEPG, "WebEPG: Getting Channel ID: {0}", channelid);
        _log.Info(LogType.WebEPG, "        [{0} of {1}]", i++, _grabList.Count);

        if (_grabList[channelid].Count > 0)
        {
          if (_grabList[channelid][0].grabber != grabberLast)
          {
            initResult = _epgGrabber.Initalise(_grabList[channelid][0].grabber, _config.Info.GrabDays);
          }

          grabberLast = _grabList[channelid][0].grabber;


          // Get channel lising
          if (initResult)
          {
            programs = _epgGrabber.GetGuide(channelid, _grabList[channelid][0].linked, _grabList[channelid][0].linkTime);
          }

          if (programs != null)
          {
            // write programs
            foreach (grabInfo grab in _grabList[channelid])
            {
              if (grab.merged)
              {
                _log.Info(LogType.WebEPG, "WebEPG: Writing Merged Channel Part to XMLTV: {0}", grab.name);
                _log.Info(LogType.WebEPG, "        [{0}]", grab.linkTime);

                for (int p = 0; p < programs.Count; p++)
                {
                  if (grab.linkTime.IsInRange(programs[p].Start))
                  {
                    xmltv.WriteProgram(programs[p], grab.name, true);
                  }
                }
              }
              else
              {
                _log.Info(LogType.WebEPG, "WebEPG: Writing Channel to XMLTV: {0}", grab.name);

                for (int p = 0; p < programs.Count; p++)
                {
                  xmltv.WriteProgram(programs[p], grab.name, false);
                }
              }
            }
          }
          else
          {
            foreach (grabInfo grab in _grabList[channelid])
            {
              _log.Info(LogType.WebEPG, "WebEPG: Grabber failed for: {0}", grab.name);
            }
          }
        }
      }

      xmltv.Close();

      // log Http statistics
      for (int h = 0; h < httpStats.Count; h++)
      {
        SiteStatistics site = httpStats.GetbyIndex(h);
        _log.Info(LogType.WebEPG, "HTTP Statistics: {0}", site.ToString());
        httpStats.Clear(site.Site);
      }

      return true;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads the config.
    /// </summary>
    /// <returns>bool - success/fail</returns>
    private bool LoadConfig()
    {
      if (!File.Exists(_configFile))
      {
        _log.Info(LogType.WebEPG, "File not found: {0}", _configFile);
        return false;
      }

      _log.Info(LogType.WebEPG, "Loading Config File: {0}", _configFile);
      try
      {
        XmlSerializer s = new XmlSerializer(typeof (WebepgConfigFile));
        TextReader r = new StreamReader(_configFile);
        _config = (WebepgConfigFile) s.Deserialize(r);
        r.Close();
      }
      catch (InvalidOperationException ex)
      {
        _log.Error(LogType.WebEPG, "WebEPG: Error loading config: {0}", ex.Message);
        return false;
      }

      return true;
      //string grabberDir;
      //int maxGrabDays;
      //int channelCount;

      //Hashtable idList = new Hashtable();

      //_channels = new ArrayList();

      //_xmlreader = new MediaPortal.Webepg.Profile.Xml(_configFile);
      //maxGrabDays = _xmlreader.GetValueAsInt("General", "MaxDays", 1);
      //grabberDir = _xmlreader.GetValueAsString("General", "GrabberDir", _baseDirectory + "\\WebEPG\\grabbers\\");
      //_epgGrabber = new WebListingGrabber(maxGrabDays, grabberDir);

      //int AuthCount = _xmlreader.GetValueAsInt("AuthSites", "Count", 0);
      //if (AuthCount > 0)
      //{
      //  HTTPAuth authService = new HTTPAuth();
      //  for (int i = 1; i <= AuthCount; i++)
      //  {
      //    string site = _xmlreader.GetValueAsString("Auth" + i.ToString(), "Site", "");
      //    string login = _xmlreader.GetValueAsString("Auth" + i.ToString(), "Login", "");
      //    string password = _xmlreader.GetValueAsString("Auth" + i.ToString(), "Password", "");
      //    NetworkCredential auth = new NetworkCredential(login, password);
      //    authService.Add(site, auth);
      //  }
      //  GlobalServiceProvider.Add<IHttpAuthentication>(authService);
      //}

      //int MergeCount = _xmlreader.GetValueAsInt("MergeChannels", "Count", 0);

      //if (MergeCount > 0)
      //{
      //  _mergedList = new MergeInfo[MergeCount];

      //  for (int i = 1; i <= MergeCount; i++)
      //  {
      //    MergeInfo merged = new MergeInfo();
      //    merged.count = _xmlreader.GetValueAsInt("Merge" + i.ToString(), "Channels", 0);
      //    merged.name = _xmlreader.GetValueAsString("Merge" + i.ToString(), "DisplayName", "");
      //    merged.channels = new MergeChannelData[merged.count];
      //    for (int c = 1; c <= merged.count; c++)
      //    {
      //      string channelId = _xmlreader.GetValueAsString("Merge" + i.ToString(), "Channel" + c.ToString(), "");
      //      MergeChannelData mergeTime = new MergeChannelData();
      //      MergeChannelLocation location = new MergeChannelLocation();
      //      location.mergeChannel = c - 1;
      //      location.mergeNum = i - 1;
      //      string start = _xmlreader.GetValueAsString("Merge" + i.ToString(), "Start" + c.ToString(), "0:0");
      //      string end = _xmlreader.GetValueAsString("Merge" + i.ToString(), "End" + c.ToString(), "0:0");
      //      mergeTime.time = new TimeRange(start, end);
      //      merged.channels[c - 1] = mergeTime;
      //      _mergedChannels.Add(channelId, location);
      //    }
      //    _mergedList[i - 1] = merged;
      //  }
      //}

      //channelCount = _xmlreader.GetValueAsInt("ChannelMap", "Count", 0);

      //for (int i = 1; i <= channelCount; i++)
      //{
      //  GrabberInfo channel = new GrabberInfo();
      //  channel.copies = 0;
      //  channel.iscopy = false;
      //  channel.isMerged = false;
      //  channel.name = _xmlreader.GetValueAsString(i.ToString(), "DisplayName", "");
      //  channel.id = _xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
      //  channel.grabber = _xmlreader.GetValueAsString(i.ToString(), "Grabber1", "");
      //  channel.Linked = _xmlreader.GetValueAsBool(i.ToString(), "Grabber1-Linked", false);
      //  if (channel.Linked)
      //  {
      //    channel.linkStart = _xmlreader.GetValueAsInt(i.ToString(), "Grabber1-Start", 18);
      //    channel.linkEnd = _xmlreader.GetValueAsInt(i.ToString(), "Grabber1-End", 23);
      //  }

      //  object obj = idList[channel.id];
      //  if (obj == null)
      //  {
      //    idList.Add(channel.id, i - 1);
      //  }
      //  else
      //  {
      //    int pos = (int)obj;
      //    GrabberInfo orig = (GrabberInfo)_channels[pos];
      //    orig.copies++;
      //    channel.id += orig.copies.ToString();
      //    channel.copies = pos;
      //    channel.iscopy = true;
      //    _channels.RemoveAt(pos);
      //    _channels.Insert(pos, orig);
      //  }

      //  if (_mergedChannels.Contains(channel.id))
      //    channel.isMerged = true;

      //  _channels.Add(channel);
      //}

      //return true;
    }

    #endregion
  }
}