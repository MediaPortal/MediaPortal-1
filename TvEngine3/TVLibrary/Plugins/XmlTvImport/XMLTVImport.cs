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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;
using System.IO;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Implementations;

using Gentle.Common;
using Gentle.Framework;
namespace TvEngine
{
  class XMLTVImport : IComparer
  {
    public delegate void ShowProgressHandler(Stats stats);
    public event ShowProgressHandler ShowProgress;

    class ChannelPrograms
    {
      public string Name;
      public string ExternalId;
      public ArrayList programs = new ArrayList();
    };

    public class Stats
    {
      string _status = "";
      int _programs = 0;
      int _channels = 0;
      DateTime _startTime = DateTime.Now;
      DateTime _endTime = DateTime.Now;
      public string Status
      {
        get { return _status; }
        set { _status = value; }
      }
      public int Programs
      {
        get { return _programs; }
        set { _programs = value; }
      }
      public int Channels
      {
        get { return _channels; }
        set { _channels = value; }
      }
      public DateTime StartTime
      {
        get { return _startTime; }
        set { _startTime = value; }
      }
      public DateTime EndTime
      {
        get { return _endTime; }
        set { _endTime = value; }
      }
    };

    string _errorMessage = "";
    Stats _status = new Stats();
    int _backgroundDelay = 0;

    static bool _isImporting = false;
    public XMLTVImport()
      : this(0)
    {
    }

    public XMLTVImport(int backgroundDelay)
    {

      _backgroundDelay = backgroundDelay;
    }

    public string ErrorMessage
    {
      get { return _errorMessage; }
    }

    public Stats ImportStats
    {
      get { return _status; }
    }
    public bool Import(string fileName, bool showProgress)
    {
      Dictionary<int, DateTime> lastProgramForChannel = new Dictionary<int, DateTime>();
      IList channels = Channel.ListAll();
      foreach (Channel ch in channels)
      {
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(TvDatabase.Program));
        sb.AddConstraint(Operator.Equals, "idChannel", ch.IdChannel);
        sb.AddOrderByField(false, "starttime");
        sb.SetRowLimit(1);
        SqlStatement stmt = sb.GetStatement(true);
        IList programsInDbs = ObjectFactory.GetCollection(typeof(TvDatabase.Program), stmt.Execute());

        DateTime lastProgram = DateTime.MinValue;
        if (programsInDbs.Count > 0)
        {
          TvDatabase.Program p = (TvDatabase.Program)programsInDbs[0];
          lastProgram = p.EndTime;
        }
        lastProgramForChannel[ch.IdChannel] = lastProgram;
      }

      _errorMessage = "";
      if (_isImporting == true)
      {
        _errorMessage = "already importing...";
        return false;
      }
      _isImporting = true;

      //TVDatabase.SupressEvents = true;
      bool useTimeZone = false;
      int timeZoneCorrection = 0;
      TvBusinessLayer layer = new TvBusinessLayer();
      useTimeZone = layer.GetSetting("xmlTvUseTimeZone", "true").Value == "true";
      int hours = Int32.Parse(layer.GetSetting("xmlTvTimeZoneHours", "0").Value);
      int mins = Int32.Parse(layer.GetSetting("xmlTvTimeZoneMins", "0").Value);
      timeZoneCorrection = hours * 60 + mins;


      _status.Status = "Loading XML file";
      _status.Channels = 0;
      _status.Programs = 0;
      _status.StartTime = DateTime.Now;
      _status.EndTime = new DateTime(1971, 11, 6);
      if (showProgress && ShowProgress != null) ShowProgress(_status);
      ArrayList Programs = new ArrayList();
      try
      {
        Log.WriteFile("xmltv import {0}", fileName);

        //
        // Make sure the file exists before we try to do any processing
        //
        if (File.Exists(fileName))
        {
          XmlDocument xml = new XmlDocument();
          xml.Load(fileName);
          if (xml.DocumentElement == null)
          {
            _errorMessage = "Invalid XMLTV file";
            Log.Error("  {0} is not a valid xml file");
            xml = null;
            _isImporting = false;
            //            TVDatabase.SupressEvents = false;
            return false;
          }
          XmlNodeList channelList = xml.DocumentElement.SelectNodes("/tv/channel");
          if (channelList == null || channelList.Count == 0)
          {
            _errorMessage = "No channels found";
            Log.Error("  {0} does not contain any channels");
            xml = null;
            _isImporting = false;
            //            TVDatabase.SupressEvents = false;
            return false;
          }


          _status.Status = "Loading TV channels";
          if (showProgress && ShowProgress != null) ShowProgress(_status);


          layer.RemoveOldPrograms();

          ArrayList tvchannels = new ArrayList();
          IList allChannels = Channel.ListAll();
          int iChannel = 0;
          foreach (XmlNode nodeChannel in channelList)
          {
            if (nodeChannel.Attributes != null)
            {
              XmlNode nodeId = nodeChannel.Attributes.GetNamedItem("id");
              if (nodeId != null && nodeId.InnerText != null && nodeId.InnerText.Length > 0)
              {
                XmlNode nodeName = nodeChannel.SelectSingleNode("display-name");
                if (nodeName == null)
                  nodeName = nodeChannel.SelectSingleNode("Display-Name");
                XmlNode nodeIcon = nodeChannel.SelectSingleNode("icon");
                if (nodeName != null && nodeName.InnerText != null)
                {

                  //parse name of channel to see if it contains a channel number
                  string number = String.Empty;
                  for (int i = 0; i < nodeName.InnerText.Length; ++i)
                  {
                    if (Char.IsDigit(nodeName.InnerText[i]))
                    {
                      number += nodeName.InnerText[i];
                    }
                    else break;
                  }
                  if (number == String.Empty)
                  {
                    for (int i = 0; i < nodeId.InnerText.Length; ++i)
                    {
                      if (Char.IsDigit(nodeId.InnerText[i]))
                      {
                        number += nodeId.InnerText[i];
                      }
                      else break;
                    }
                  }
                  int channelNo = 0;
                  if (number != String.Empty)
                    channelNo = Int32.Parse(number);

                  Channel chan = null;
                  foreach (Channel ch in allChannels)
                  {
                    if (ch.Name == nodeName.InnerText)
                    {
                      chan = ch;
                      break;
                    }
                  }
                  if (chan == null)
                  {
                    chan = new Channel(ConvertHTMLToAnsi(nodeName.InnerText), false, true, 0, Schedule.MinSchedule, false, Schedule.MinSchedule, 10000, true, nodeId.InnerText);
                    chan.Persist();
                    AnalogChannel tuningDetail = new AnalogChannel();
                    tuningDetail.ChannelNumber = channelNo;
                    tuningDetail.IsTv = true;
                    tuningDetail.IsRadio = false;
                    tuningDetail.Name = chan.Name;
                    tuningDetail.Frequency = 0;
                    layer.AddTuningDetails(chan, tuningDetail);
                  }
                  else
                  {
                    chan.ExternalId = nodeId.InnerText;
                    chan.Persist();
                  }

                  ChannelPrograms newProgChan = new ChannelPrograms();
                  newProgChan.Name = chan.Name;
                  newProgChan.ExternalId = chan.ExternalId;
                  Programs.Add(newProgChan);

                  Log.WriteFile("  channel#{0} xmlid:{1} name:{2} dbsid:{3}", iChannel, chan.ExternalId, chan.Name, chan.IdChannel);
                  tvchannels.Add(chan);
                  /*
                  if (nodeIcon != null)
                  {
                    if (nodeIcon.Attributes != null)
                    {
                      XmlNode nodeSrc = nodeIcon.Attributes.GetNamedItem("src");
                      if (nodeSrc != null)
                      {
                        string strURL = ConvertHTMLToAnsi(nodeSrc.InnerText);
                        string strLogoPng = GetCoverArtName(Config.GetSubFolder(Config.Dir.Thumbs, @"tv\logos"), chan.Name);
                        if (!System.IO.File.Exists(strLogoPng))
                        {
                          DownLoadImage(strURL, strLogoPng, System.Drawing.Imaging.ImageFormat.Png);
                        }
                      }
                    }
                  }*/
                  _status.Channels++;
                  if (showProgress && ShowProgress != null) ShowProgress(_status);
                }
                else
                {
                  Log.Error("  channel#{0} doesnt contain an displayname", iChannel);
                }
              }
              else
              {
                Log.Error("  channel#{0} doesnt contain an id", iChannel);
              }
            }
            else
            {
              Log.Error("  channel#{0} doesnt contain an id", iChannel);
            }
            iChannel++;
          }

          allChannels = Channel.ListAll();
          int programIndex = 0;
          _status.Status = "Loading TV programs";
          if (showProgress && ShowProgress != null) ShowProgress(_status);

          // get offset between local time & UTC
          TimeSpan utcOff = System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

          // take in account daylightsavings 
          bool bIsDayLightSavings = System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now);
          if (bIsDayLightSavings) utcOff = utcOff.Add(new TimeSpan(0, -1, 0, 0, 0));

          Log.WriteFile("Current timezone:{0}", System.TimeZone.CurrentTimeZone.StandardName);
          Log.WriteFile("Offset with UTC {0:00}:{1:00} DaylightSavings:{2}", utcOff.Hours, utcOff.Minutes, bIsDayLightSavings.ToString());
          XmlNodeList programsList = xml.DocumentElement.SelectNodes("/tv/programme");

          foreach (XmlNode programNode in programsList)
          {
            if (programNode.Attributes != null)
            {
              XmlNode nodeStart = programNode.Attributes.GetNamedItem("start");
              XmlNode nodeStop = programNode.Attributes.GetNamedItem("stop");
              XmlNode nodeChannel = programNode.Attributes.GetNamedItem("channel");
              XmlNode nodeTitle = programNode.SelectSingleNode("title");
              XmlNode nodeCategory = programNode.SelectSingleNode("category");
              XmlNode nodeDescription = programNode.SelectSingleNode("desc");
              XmlNode nodeEpisode = programNode.SelectSingleNode("sub-title");
              XmlNode nodeRepeat = programNode.SelectSingleNode("previously-shown");
              XmlNode nodeEpisodeNum = programNode.SelectSingleNode("episode-num");
              XmlNode nodeDate = programNode.SelectSingleNode("date");
              XmlNode nodeStarRating = programNode.SelectSingleNode("star-rating");
              XmlNode nodeClasification = programNode.SelectSingleNode("rating");

              if (nodeStart != null && nodeChannel != null && nodeTitle != null)
              {
                if (nodeStart.InnerText != null && nodeChannel.InnerText != null && nodeTitle.InnerText != null)
                {
                  string description = "";
                  string category = "-";
                  string episode = "";
                  string repeat = "";
                  string serEpNum = "";
                  string date = "";
                  string seriesNum = "";
                  string episodeNum = "";
                  string episodePart = "";
                  string starRating = "";
                  string clasification = "";

                  if (nodeRepeat != null) repeat = "Repeat";

                  string title = nodeTitle.InnerText;
                  long startDate = 0;
                  if (nodeStart.InnerText.Length >= 14)
                  {
                    if (Char.IsDigit(nodeStart.InnerText[12]) && Char.IsDigit(nodeStart.InnerText[13]))
                      startDate = Int64.Parse(nodeStart.InnerText.Substring(0, 14));//20040331222000
                    else
                      startDate = 100 * Int64.Parse(nodeStart.InnerText.Substring(0, 12));//200403312220
                  }
                  else if (nodeStart.InnerText.Length >= 12)
                  {
                    startDate = 100 * Int64.Parse(nodeStart.InnerText.Substring(0, 12));//200403312220
                  }


                  long stopDate = startDate;
                  if (nodeStop != null && nodeStop.InnerText != null)
                  {
                    if (nodeStop.InnerText.Length >= 14)
                    {
                      if (Char.IsDigit(nodeStop.InnerText[12]) && Char.IsDigit(nodeStop.InnerText[13]))
                        stopDate = Int64.Parse(nodeStop.InnerText.Substring(0, 14));//20040331222000
                      else
                        stopDate = 100 * Int64.Parse(nodeStop.InnerText.Substring(0, 12));//200403312220
                    }
                    else if (nodeStop.InnerText.Length >= 12)
                    {
                      stopDate = 100 * Int64.Parse(nodeStop.InnerText.Substring(0, 12));//200403312220
                    }
                  }
                  startDate = CorrectIllegalDateTime(startDate);
                  stopDate = CorrectIllegalDateTime(stopDate);
                  string timeZoneStart = "";
                  string timeZoneEnd = "";
                  int startTimeOffset = 0;
                  int endTimeOffset = 0;
                  if (nodeStart.InnerText.Length > 14)
                  {
                    timeZoneStart = nodeStart.InnerText.Substring(14);
                    timeZoneStart = timeZoneStart.Trim();
                    timeZoneEnd = timeZoneStart;
                  }
                  if (nodeStop != null)
                  {
                    if (nodeStop.InnerText.Length > 14)
                    {
                      timeZoneEnd = nodeStop.InnerText.Substring(14);
                      timeZoneEnd = timeZoneEnd.Trim();
                    }
                  }

                  // are we using the timezone information from the XMLTV file
                  if (!useTimeZone)
                  {
                    // no
                    startTimeOffset = 0;
                    endTimeOffset = 0;
                  }
                  else
                  {
                    // yes, then get the start/end timeoffsets
                    startTimeOffset = GetTimeOffset(timeZoneStart);
                    endTimeOffset = GetTimeOffset(timeZoneEnd);
                  }

                  // add timezone correction
                  // correct program starttime
                  DateTime dateTimeStart = longtodate(startDate);
                  int hour = (startTimeOffset / 100);
                  int minutes = startTimeOffset - (hour * 100);
                  //iHour -= utcOff.Hours;
                  //iMin -= utcOff.Minutes;
                  dateTimeStart = dateTimeStart.AddHours(hour);
                  dateTimeStart = dateTimeStart.AddMinutes(minutes);
                  dateTimeStart = dateTimeStart.AddMinutes(timeZoneCorrection);
                  startDate = datetolong(dateTimeStart);

                  if (nodeStop != null && nodeStop.InnerText != null)
                  {
                    // correct program endtime
                    DateTime dateTimeEnd = longtodate(stopDate);
                    hour = (endTimeOffset / 100);
                    minutes = endTimeOffset - (hour * 100);
                    //			  iHour -= utcOff.Hours;
                    //			  iMin -= utcOff.Minutes;
                    dateTimeEnd = dateTimeEnd.AddHours(hour);
                    dateTimeEnd = dateTimeEnd.AddMinutes(minutes);
                    dateTimeEnd = dateTimeEnd.AddMinutes(timeZoneCorrection);
                    stopDate = datetolong(dateTimeEnd);
                  }
                  else stopDate = startDate;

                  int channelId = -1;
                  string channelName = "";
                  if (nodeChannel.InnerText.Length > 0)
                  {
                    foreach (Channel chan in tvchannels)
                    {
                      if (chan.ExternalId == nodeChannel.InnerText)
                      {
                        channelName = chan.Name;
                        channelId = chan.IdChannel;
                        break;
                      }
                    }
                  }
                  if (channelId < 0)
                  {
                    Log.Error("Unknown TV channel xmlid:{0}", nodeChannel.InnerText);
                    continue;
                  }

                  if (nodeCategory != null && nodeCategory.InnerText != null)
                  {
                    category = nodeCategory.InnerText;
                  }
                  if (nodeDescription != null && nodeDescription.InnerText != null)
                  {
                    description = nodeDescription.InnerText;
                  }
                  if (nodeEpisode != null && nodeEpisode.InnerText != null)
                  {
                    episode = nodeEpisode.InnerText;
                    if (title.Length == 0)
                      title = nodeEpisode.InnerText;
                  }
                  if (nodeEpisodeNum != null && nodeEpisodeNum.InnerText != null)
                  {
                    if (nodeEpisodeNum.Attributes.GetNamedItem("system").InnerText == "xmltv_ns")
                    {
                      serEpNum = ConvertHTMLToAnsi(nodeEpisodeNum.InnerText.Replace(" ", ""));
                      int pos = 0;
                      int Epos = 0;
                      pos = serEpNum.IndexOf(".", pos);
                      if (pos == 0) //na_dd grabber only gives '..0/2' etc
                      {
                        Epos = pos;
                        pos = serEpNum.IndexOf(".", pos + 1);
                        episodeNum = serEpNum.Substring(Epos + 1, (pos - 1) - Epos);
                        episodePart = serEpNum.Substring(pos + 1, serEpNum.Length - (pos + 1));
                        if (episodePart.IndexOf("/", 0) != -1)// danish guide gives: episode-num system="xmltv_ns"> . 113 . </episode-num>
                        {
                          if (episodePart.Substring(2, 1) == "1") episodePart = "";
                          else
                          {
                            int p = 0;
                            int t = 0;

                            if (Convert.ToInt32(episodePart.Substring(0, 1)) == 0)
                            {
                              p = Convert.ToInt32(episodePart.Substring(0, 1)) + 1;
                              t = Convert.ToInt32(episodePart.Substring(2, 1));
                              episodePart = Convert.ToString(p) + "/" + Convert.ToString(t);
                            }
                          }
                        }
                      }
                      else if (pos > 0)
                      {
                        seriesNum = serEpNum.Substring(0, pos);
                        Epos = pos;
                        pos = serEpNum.IndexOf(".", pos + 1);
                        episodeNum = serEpNum.Substring(Epos + 1, (pos - 1) - Epos);
                        episodePart = serEpNum.Substring(pos + 1, serEpNum.Length - (pos + 1));
                        if (episodePart.IndexOf("/", 0) != -1)
                        {
                          if (episodePart.Substring(2, 1) == "1") episodePart = "";
                          else
                          {
                            int p = 0;
                            int t = 0;
                            if (Convert.ToInt32(episodePart.Substring(0, 1)) == 0)
                            {
                              p = Convert.ToInt32(episodePart.Substring(0, 1)) + 1;
                            }
                            else
                            {
                              p = Convert.ToInt32(episodePart.Substring(0, 1));
                            }
                            t = Convert.ToInt32(episodePart.Substring(2, 1));
                            episodePart = Convert.ToString(p) + "/" + Convert.ToString(t);
                          }
                        }
                      }
                      else
                      {
                        seriesNum = serEpNum;
                        episodeNum = "";
                        episodePart = "";
                      }
                    }
                  }
                  if (nodeDate != null && nodeDate.InnerText != null)
                  {
                    date = nodeDate.InnerText;
                  }
                  if (nodeStarRating != null && nodeStarRating.InnerText != null)
                  {
                    starRating = nodeStarRating.InnerText;
                  }
                  if (nodeClasification != null && nodeClasification.InnerText != null)
                  {
                    clasification = nodeClasification.InnerText;
                  }

                  Channel channel = null;
                  foreach (Channel ch in allChannels)
                  {
                    if (ch.Name == channelName)
                    {
                      channel = ch;
                      break;
                    }
                  }
                  Program prog = new Program(channel.IdChannel, longtodate(startDate), longtodate(stopDate), title, description, category, false);
                  //prog.Description = ConvertHTMLToAnsi(strDescription);
                  //prog.StartTime = iStart;
                  //prog.EndTime = iStop;
                  //prog.Title = ConvertHTMLToAnsi(strTitle);
                  //prog.Genre = ConvertHTMLToAnsi(strCategory);
                  //prog.Channel = ConvertHTMLToAnsi(strChannelName);
                  //prog.Date = strDate;
                  //prog.Episode = ConvertHTMLToAnsi(strEpisode);
                  //prog.Repeat = ConvertHTMLToAnsi(strRepeat);
                  //prog.SeriesNum = ConvertHTMLToAnsi(strSeriesNum);
                  //prog.EpisodeNum = ConvertHTMLToAnsi(strEpisodeNum);
                  //prog.EpisodePart = ConvertHTMLToAnsi(strEpisodePart);
                  //prog.StarRating = ConvertHTMLToAnsi(strStarRating);
                  //prog.Classification = ConvertHTMLToAnsi(strClasification);
                  _status.Programs++;
                  if (showProgress && ShowProgress != null && (_status.Programs % 100) == 0) ShowProgress(_status);
                  foreach (ChannelPrograms progChan in Programs)
                  {
                    if (String.Compare(progChan.Name, channelName, true) == 0)
                    {
                      progChan.programs.Add(prog);
                    }
                  }
                }
              }
            }
            programIndex++;
          }
          _status.Programs = 0;
          _status.Status = "Sorting TV programs";
          if (showProgress && ShowProgress != null) ShowProgress(_status);
          DateTime dtStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
          //dtStartDate=dtStartDate.AddDays(-4);

          foreach (ChannelPrograms progChan in Programs)
          {
            progChan.programs.Sort(this);
            for (int i = 0; i < progChan.programs.Count; ++i)
            {
              Program prog = (Program)progChan.programs[i];
              if (prog.StartTime == prog.EndTime)
              {
                if (i + 1 < progChan.programs.Count)
                {
                  Program progNext = (Program)progChan.programs[i + 1];
                  prog.EndTime = progNext.StartTime;
                }
              }
            }
            RemoveOverlappingPrograms(ref progChan.programs); // be sure that we do not have any overlapping

            for (int i = 0; i < progChan.programs.Count; ++i)
            {
              Program prog = (Program)progChan.programs[i];
              // dont import programs which have already ended...
              if (prog.EndTime > dtStartDate)
              {
                Thread.Sleep(_backgroundDelay);


                if (lastProgramForChannel.ContainsKey(prog.IdChannel))
                {
                  DateTime lastProgramDate = lastProgramForChannel[prog.IdChannel];
                  if (prog.StartTime >= lastProgramDate)
                  {
                    prog.Persist();
                  }
                }
                else
                {
                  prog.Persist();
                }
                if (prog.StartTime < _status.StartTime) _status.StartTime = prog.StartTime;
                if (prog.EndTime > _status.EndTime) _status.EndTime = prog.EndTime;
                _status.Programs++;
                if (showProgress && ShowProgress != null && (_status.Programs % 100) == 0) ShowProgress(_status);
              }
            }
          }
          //TVDatabase.RemoveOverlappingPrograms();
          Programs.Clear();
          Programs = null;
          xml = null;
          _isImporting = false;
          //          TVDatabase.SupressEvents = false;
          if (programIndex > 0)
          {
            _errorMessage = "File imported successfully";
            return true;
          }
          _errorMessage = "No programs found";
          return false;
        }
        else
        {
          _errorMessage = "No xmltv file found";
          _status.Status = _errorMessage;
          Log.Error("xmltv data file was not found");
        }
      }
      catch (Exception ex)
      {
        _errorMessage = String.Format("Invalid XML file:{0}", ex.Message);
        _status.Status = String.Format("invalid XML file:{0}", ex.Message);
        Log.Error("XML tv import error loading {0} err:{1} ", fileName, ex.Message);
        //TVDatabase.RollbackTransaction();
      }
      Programs.Clear();
      Programs = null;
      _isImporting = false;
      //      TVDatabase.SupressEvents = false;
      return false;
    }

    int GetTimeOffset(string timeZone)
    {
      // timezone can b in format:
      // GMT +0100 or GMT -0500
      // or just +0300
      if (timeZone.Length == 0) return 0;
      timeZone = timeZone.ToLower();

      // just ignore GMT offsets, since we're calculating everything from GMT anyway
      if (timeZone.IndexOf("gmt") >= 0)
      {
        int ipos = timeZone.IndexOf("gmt");
        timeZone = timeZone.Substring(ipos + "GMT".Length);
      }

      timeZone = timeZone.Trim();
      if (timeZone[0] == '+' || timeZone[0] == '-')
      {
        string strOff = timeZone.Substring(1);
        try
        {
          int iOff = Int32.Parse(strOff);
          if (timeZone[0] == '-') return -iOff;
          else return iOff;
        }
        catch (Exception)
        {
        }
      }
      return 0;
    }

    long CorrectIllegalDateTime(long datetime)
    {
      //format : 20050710245500
      long orgDateTime = datetime;
      long sec = datetime % 100; datetime /= 100;
      long min = datetime % 100; datetime /= 100;
      long hour = datetime % 100; datetime /= 100;
      long day = datetime % 100; datetime /= 100;
      long month = datetime % 100; datetime /= 100;
      long year = datetime;
      DateTime dt = new DateTime((int)year, (int)month, (int)day, 0, 0, 0);
      dt = dt.AddHours(hour);
      dt = dt.AddMinutes(min);
      dt = dt.AddSeconds(sec);


      long newDateTime = datetolong(dt);
      if (sec < 0 || sec > 59 ||
        min < 0 || min > 59 ||
        hour < 0 || hour >= 24 ||
        day < 0 || day > 31 ||
        month < 0 || month > 12)
      {
        //Log.WriteFile(LogType.EPG, true, "epg-import:tvguide.xml contains invalid date/time :{0} converted it to:{1}",
        //              orgDateTime, newDateTime);
      }

      return newDateTime;
    }

    public void RemoveOverlappingPrograms(ref ArrayList Programs)
    {
      try
      {
        if (Programs.Count == 0) return;
        Programs.Sort(this);
        Program prevProg = (Program)Programs[0];
        for (int i = 1; i < Programs.Count; i++)
        {
          Program newProg = (Program)Programs[i];
          if (newProg.StartTime < prevProg.EndTime)   // we have an overlap here
          {
            // let us find out which one is the correct one
            if (newProg.StartTime > prevProg.StartTime)  // newProg will create hole -> delete it
            {
              Programs.Remove(newProg);
              i--;                              // stay at the same position
              continue;
            }

            List<Program> prevList = new List<Program>();
            List<Program> newList = new List<Program>();
            prevList.Add(prevProg);
            newList.Add(newProg);
            Program syncPrev = prevProg;
            Program syncProg = newProg;
            for (int j = i + 1; j < Programs.Count; j++)
            {
              Program syncNew = (Program)Programs[j];
              if (syncPrev.EndTime == syncNew.StartTime)
              {
                prevList.Add(syncNew);
                syncPrev = syncNew;
                if (syncNew.StartTime > syncProg.EndTime)
                {
                  // stop point reached => delete Programs in newList
                  foreach (Program Prog in newList) Programs.Remove(Prog);
                  i = j - 1;
                  prevProg = syncPrev;
                  newList.Clear();
                  prevList.Clear();
                  break;
                }
              }
              else if (syncProg.EndTime == syncNew.StartTime)
              {
                newList.Add(syncNew);
                syncProg = syncNew;
                if (syncNew.StartTime > syncPrev.EndTime)
                {
                  // stop point reached => delete Programs in prevList
                  foreach (Program Prog in prevList) Programs.Remove(Prog);
                  i = j - 1;
                  prevProg = syncProg;
                  newList.Clear();
                  prevList.Clear();
                  break;
                }
              }
            }
            // check if a stop point was reached => if not delete newList
            if (newList.Count > 0)
            {
              foreach (Program Prog in prevList) Programs.Remove(Prog);
              i = Programs.Count;
              break;
            }
          }
          prevProg = newProg;
        }
      }
      catch (Exception ex)
      {
        Log.Error("XML tv import error:{1} ", ex.Message);
      }
    }

    public void FillInMissingDataFromDB(ref ArrayList Programs, ArrayList dbEPG)
    {
      Programs.Sort(this);
      dbEPG.Sort(this);
      Program prevProg = (Program)Programs[0];
      for (int i = 1; i < Programs.Count; i++)
      {
        Program newProg = (Program)Programs[i];
        if (newProg.StartTime > prevProg.EndTime)   // we have a gab here
        {
          // try to find data in the database
          foreach (Program dbProg in dbEPG)
          {
            if ((dbProg.StartTime >= prevProg.EndTime) && (dbProg.EndTime <= newProg.StartTime))
            {
              Programs.Insert(i, dbProg.Clone());
              i++;
              prevProg = dbProg;
            }
            if (dbProg.StartTime >= newProg.EndTime) break; // no more data available
          }
        }
        prevProg = newProg;
      }
    }



    public long datetolong(DateTime dt)
    {
      try
      {
        long iSec = 0;//(long)dt.Second;
        long iMin = (long)dt.Minute;
        long iHour = (long)dt.Hour;
        long iDay = (long)dt.Day;
        long iMonth = (long)dt.Month;
        long iYear = (long)dt.Year;

        long lRet = (iYear);
        lRet = lRet * 100L + iMonth;
        lRet = lRet * 100L + iDay;
        lRet = lRet * 100L + iHour;
        lRet = lRet * 100L + iMin;
        lRet = lRet * 100L + iSec;
        return lRet;
      }
      catch (Exception)
      {
      }
      return 0;
    }
    public DateTime longtodate(long ldate)
    {
      try
      {
        if (ldate < 0) return DateTime.MinValue;
        int year, month, day, hour, minute, sec;
        sec = (int)(ldate % 100L); ldate /= 100L;
        minute = (int)(ldate % 100L); ldate /= 100L;
        hour = (int)(ldate % 100L); ldate /= 100L;
        day = (int)(ldate % 100L); ldate /= 100L;
        month = (int)(ldate % 100L); ldate /= 100L;
        year = (int)ldate;
        DateTime dt = new DateTime(year, month, day, hour, minute, 0, 0);
        return dt;
      }
      catch (Exception)
      {
      }
      return DateTime.Now;
    }

    public string ConvertHTMLToAnsi(string html)
    {
      string strippedHtml = String.Empty;
      ConvertHTMLToAnsi(html, out strippedHtml);
      return strippedHtml;
    }
    public void ConvertHTMLToAnsi(string html, out string strippedHtml)
    {
      strippedHtml = "";
      //	    int i=0; 
      if (html.Length == 0)
      {
        strippedHtml = "";
        return;
      }
      //int iAnsiPos=0;
      StringWriter writer = new StringWriter();

      System.Web.HttpUtility.HtmlDecode(html, writer);

      String DecodedString = writer.ToString();
      strippedHtml = DecodedString.Replace("<br>", "\n");
      if (true)
        return;
    }
    #region Sort Members


    public int Compare(object x, object y)
    {
      if (x == y) return 0;
      Program item1 = (Program)x;
      Program item2 = (Program)y;
      if (item1 == null) return -1;
      if (item2 == null) return -1;

      if (item1.IdChannel != item2.IdChannel)
      {
        return String.Compare(item1.ReferencedChannel().Name, item2.ReferencedChannel().Name, true);
      }
      if (item1.StartTime > item2.StartTime) return 1;
      if (item1.StartTime < item2.StartTime) return -1;
      return 0;
    }
    #endregion


  }
}
