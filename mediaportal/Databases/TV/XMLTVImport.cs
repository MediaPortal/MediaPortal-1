#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Util;
//using MediaPortal.Dialogs;

namespace MediaPortal.TV.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class XMLTVImport : IComparer
  {
    public delegate void ShowProgressHandler(Stats stats);

    public event ShowProgressHandler ShowProgress;

    private class ChannelPrograms
    {
      public string strName;
      public string XMLId;
      public ArrayList programs = new ArrayList();
    } ;

    public class Stats
    {
      private string m_strStatus = "";
      private int m_iPrograms = 0;
      private int m_iChannels = 0;
      private DateTime m_startTime = DateTime.Now;
      private DateTime m_EndTime = DateTime.Now;

      public string Status
      {
        get { return m_strStatus; }
        set { m_strStatus = value; }
      }

      public int Programs
      {
        get { return m_iPrograms; }
        set { m_iPrograms = value; }
      }

      public int Channels
      {
        get { return m_iChannels; }
        set { m_iChannels = value; }
      }

      public DateTime StartTime
      {
        get { return m_startTime; }
        set { m_startTime = value; }
      }

      public DateTime EndTime
      {
        get { return m_EndTime; }
        set { m_EndTime = value; }
      }
    } ;

    private string m_strErrorMessage = "";
    private Stats m_stats = new Stats();
    private int _backgroundDelay = 0;

    private static bool m_bImport = false;

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
      get { return m_strErrorMessage; }
    }

    public Stats ImportStats
    {
      get { return m_stats; }
    }

    public bool Import(string strFileName, bool bShowProgress)
    {
      if (m_bImport == true)
      {
        m_strErrorMessage = GUILocalizeStrings.Get(766); //"already importing..."
        return false;
      }
      m_bImport = true;
      HTMLUtil htmlUtil = new HTMLUtil();
      TVDatabase.SupressEvents = true;
      bool bUseTimeZone = false;
      int iTimeZoneCorrection = 0;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        bUseTimeZone = xmlreader.GetValueAsBool("xmltv", "usetimezone", true);
        int hours = xmlreader.GetValueAsInt("xmltv", "timezonecorrectionhours", 0);
        int mins = xmlreader.GetValueAsInt("xmltv", "timezonecorrectionmins", 0);
        iTimeZoneCorrection = hours*60 + mins;
      }

      m_stats.Status = GUILocalizeStrings.Get(645);
      m_stats.Channels = 0;
      m_stats.Programs = 0;
      m_stats.StartTime = DateTime.Now;
      m_stats.EndTime = new DateTime(1971, 11, 6);
      if (bShowProgress && ShowProgress != null)
      {
        ShowProgress(m_stats);
      }
      ArrayList Programs = new ArrayList();
      try
      {
        Log.Info("xmltv import {0}", strFileName);

        //
        // Make sure the file exists before we try to do any processing
        //
        if (File.Exists(strFileName))
        {
          XmlDocument xml = new XmlDocument();
          xml.Load(strFileName);
          if (xml.DocumentElement == null)
          {
            m_strErrorMessage = GUILocalizeStrings.Get(767); //"Invalid XMLTV file"
            Log.Error("  {0} is not a valid xml file");
            xml = null;
            m_bImport = false;
            TVDatabase.SupressEvents = false;
            return false;
          }
          XmlNodeList channelList = xml.DocumentElement.SelectNodes("/tv/channel");
          if (channelList == null || channelList.Count == 0)
          {
            m_strErrorMessage = GUILocalizeStrings.Get(768); // "No channels found"
            Log.Error("  {0} does not contain any channels");
            xml = null;
            m_bImport = false;
            TVDatabase.SupressEvents = false;
            return false;
          }


          m_stats.Status = GUILocalizeStrings.Get(642);
          if (bShowProgress && ShowProgress != null)
          {
            ShowProgress(m_stats);
          }

          TVDatabase.BeginTransaction();
          TVDatabase.ClearCache();
          TVDatabase.RemoveOldPrograms();

          ArrayList tvchannels = new ArrayList();
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
                {
                  nodeName = nodeChannel.SelectSingleNode("Display-Name");
                }
                XmlNode nodeIcon = nodeChannel.SelectSingleNode("icon");
                if (nodeName != null && nodeName.InnerText != null)
                {
                  TVChannel chan = new TVChannel();

                  //parse name of channel to see if it contains a channel number
                  string number = string.Empty;
                  for (int i = 0; i < nodeName.InnerText.Length; ++i)
                  {
                    if (Char.IsDigit(nodeName.InnerText[i]))
                    {
                      number += nodeName.InnerText[i];
                    }
                    else
                    {
                      break;
                    }
                  }
                  if (number == string.Empty)
                  {
                    for (int i = 0; i < nodeId.InnerText.Length; ++i)
                    {
                      if (Char.IsDigit(nodeId.InnerText[i]))
                      {
                        number += nodeId.InnerText[i];
                      }
                      else
                      {
                        break;
                      }
                    }
                  }
                  int channelNo = 0;
                  if (number != string.Empty)
                  {
                    channelNo = Int32.Parse(number);
                  }
                  chan.XMLId = nodeId.InnerText;
                  chan.Name = htmlUtil.ConvertHTMLToAnsi(nodeName.InnerText);
                  chan.Number = channelNo;
                  chan.Frequency = 0;

                  int idTvChannel;
                  string strTvChannel;
                  if (TVDatabase.GetEPGMapping(nodeId.InnerText, out idTvChannel, out strTvChannel))
                  {
                    chan.ID = idTvChannel;
                    chan.Name = strTvChannel;
                  }
                  else
                  {
                    TVDatabase.AddChannel(chan);
                    TVDatabase.MapEPGChannel(chan.ID, chan.Name, nodeId.InnerText);
                  }

                  ChannelPrograms newProgChan = new ChannelPrograms();
                  newProgChan.strName = htmlUtil.ConvertHTMLToAnsi(chan.Name);
                  newProgChan.XMLId = chan.XMLId;
                  Programs.Add(newProgChan);

                  //Log.Info("  channel#{0} xmlid:{1} name:{2} dbsid:{3}",iChannel,chan.XMLId,chan.Name,chan.ID);
                  tvchannels.Add(chan);
                  if (nodeIcon != null)
                  {
                    if (nodeIcon.Attributes != null)
                    {
                      XmlNode nodeSrc = nodeIcon.Attributes.GetNamedItem("src");
                      if (nodeSrc != null)
                      {
                        string strURL = htmlUtil.ConvertHTMLToAnsi(nodeSrc.InnerText);
                        string strLogoPng =
                          Util.Utils.GetCoverArtName(Config.GetSubFolder(Config.Dir.Thumbs, @"tv\logos"), chan.Name);
                        if (!File.Exists(strLogoPng))
                        {
                          Util.Utils.DownLoadImage(strURL, strLogoPng, ImageFormat.Png);
                        }
                      }
                    }
                  }
                  m_stats.Channels++;
                  if (bShowProgress && ShowProgress != null)
                  {
                    ShowProgress(m_stats);
                  }
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

          int iProgram = 0;
          m_stats.Status = GUILocalizeStrings.Get(643);
          if (bShowProgress && ShowProgress != null)
          {
            ShowProgress(m_stats);
          }

          // get offset between local time & UTC
          TimeSpan utcOff = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

          // take in account daylightsavings 
          bool bIsDayLightSavings = TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now);
          if (bIsDayLightSavings)
          {
            utcOff = utcOff.Add(new TimeSpan(0, -1, 0, 0, 0));
          }

          Log.Info("Current timezone:{0}", TimeZone.CurrentTimeZone.StandardName);
          Log.Info("Offset with UTC {0:00}:{1:00} DaylightSavings:{2}", utcOff.Hours, utcOff.Minutes,
                   bIsDayLightSavings.ToString());
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
                  string strDescription = "";
                  string strCategory = "-";
                  string strEpisode = "";
                  string strRepeat = "";
                  string strSerEpNum = "";
                  string strDate = "";
                  string strSeriesNum = "";
                  string strEpisodeNum = "";
                  string strEpisodePart = "";
                  string strStarRating = "";
                  string strClasification = "";

                  if (nodeRepeat != null)
                  {
                    strRepeat = "Repeat";
                  }

                  string strTitle = nodeTitle.InnerText;
                  long iStart = 0;
                  if (nodeStart.InnerText.Length >= 14)
                  {
                    if (Char.IsDigit(nodeStart.InnerText[12]) && Char.IsDigit(nodeStart.InnerText[13]))
                    {
                      iStart = Int64.Parse(nodeStart.InnerText.Substring(0, 14)); //20040331222000
                    }
                    else
                    {
                      iStart = 100*Int64.Parse(nodeStart.InnerText.Substring(0, 12)); //200403312220
                    }
                  }
                  else if (nodeStart.InnerText.Length >= 12)
                  {
                    iStart = 100*Int64.Parse(nodeStart.InnerText.Substring(0, 12)); //200403312220
                  }


                  long iStop = iStart;
                  if (nodeStop != null && nodeStop.InnerText != null)
                  {
                    if (nodeStop.InnerText.Length >= 14)
                    {
                      if (Char.IsDigit(nodeStop.InnerText[12]) && Char.IsDigit(nodeStop.InnerText[13]))
                      {
                        iStop = Int64.Parse(nodeStop.InnerText.Substring(0, 14)); //20040331222000
                      }
                      else
                      {
                        iStop = 100*Int64.Parse(nodeStop.InnerText.Substring(0, 12)); //200403312220
                      }
                    }
                    else if (nodeStop.InnerText.Length >= 12)
                    {
                      iStop = 100*Int64.Parse(nodeStop.InnerText.Substring(0, 12)); //200403312220
                    }
                  }
                  iStart = CorrectIllegalDateTime(iStart);
                  iStop = CorrectIllegalDateTime(iStop);
                  string strTimeZoneStart = "";
                  string strTimeZoneEnd = "";
                  int iStartTimeOffset = 0;
                  int iEndTimeOffset = 0;
                  if (nodeStart.InnerText.Length > 14)
                  {
                    strTimeZoneStart = nodeStart.InnerText.Substring(14);
                    strTimeZoneStart = strTimeZoneStart.Trim();
                    strTimeZoneEnd = strTimeZoneStart;
                  }
                  if (nodeStop != null)
                  {
                    if (nodeStop.InnerText.Length > 14)
                    {
                      strTimeZoneEnd = nodeStop.InnerText.Substring(14);
                      strTimeZoneEnd = strTimeZoneEnd.Trim();
                    }
                  }

                  // are we using the timezone information from the XMLTV file
                  if (!bUseTimeZone)
                  {
                    // no
                    iStartTimeOffset = 0;
                    iEndTimeOffset = 0;
                  }
                  else
                  {
                    // yes, then get the start/end timeoffsets
                    iStartTimeOffset = GetTimeOffset(strTimeZoneStart);
                    iEndTimeOffset = GetTimeOffset(strTimeZoneEnd);
                  }

                  // add timezone correction
                  // correct program starttime
                  DateTime dtStart = Util.Utils.longtodate(iStart);
                  int iHour = (iStartTimeOffset/100);
                  int iMin = iStartTimeOffset - (iHour*100);
                  //iHour -= utcOff.Hours;
                  //iMin -= utcOff.Minutes;
                  dtStart = dtStart.AddHours(iHour);
                  dtStart = dtStart.AddMinutes(iMin);
                  dtStart = dtStart.AddMinutes(iTimeZoneCorrection);
                  iStart = Util.Utils.datetolong(dtStart);

                  if (nodeStop != null && nodeStop.InnerText != null)
                  {
                    // correct program endtime
                    DateTime dtEnd = Util.Utils.longtodate(iStop);
                    iHour = (iEndTimeOffset/100);
                    iMin = iEndTimeOffset - (iHour*100);
                    //			  iHour -= utcOff.Hours;
                    //			  iMin -= utcOff.Minutes;
                    dtEnd = dtEnd.AddHours(iHour);
                    dtEnd = dtEnd.AddMinutes(iMin);
                    dtEnd = dtEnd.AddMinutes(iTimeZoneCorrection);
                    iStop = Util.Utils.datetolong(dtEnd);
                  }
                  else
                  {
                    iStop = iStart;
                  }

                  int iChannelId = -1;
                  string strChannelName = "";
                  if (nodeChannel.InnerText.Length > 0)
                  {
                    foreach (TVChannel chan in tvchannels)
                    {
                      if (chan.XMLId == nodeChannel.InnerText)
                      {
                        strChannelName = chan.Name;
                        iChannelId = chan.ID;
                        break;
                      }
                    }
                  }
                  if (iChannelId < 0)
                  {
                    Log.Error("Unknown TV channel xmlid:{0}", nodeChannel.InnerText);
                    continue;
                  }

                  if (nodeCategory != null && nodeCategory.InnerText != null)
                  {
                    strCategory = nodeCategory.InnerText;
                  }
                  if (nodeDescription != null && nodeDescription.InnerText != null)
                  {
                    strDescription = nodeDescription.InnerText;
                  }
                  if (nodeEpisode != null && nodeEpisode.InnerText != null)
                  {
                    strEpisode = nodeEpisode.InnerText;
                    if (strTitle.Length == 0)
                    {
                      strTitle = nodeEpisode.InnerText;
                    }
                  }
                  if (nodeEpisodeNum != null && nodeEpisodeNum.InnerText != null)
                  {
                    if (nodeEpisodeNum.Attributes.GetNamedItem("system").InnerText == "xmltv_ns")
                    {
                      strSerEpNum = htmlUtil.ConvertHTMLToAnsi(nodeEpisodeNum.InnerText.Replace(" ", ""));
                      int pos = 0;
                      int Epos = 0;
                      pos = strSerEpNum.IndexOf(".", pos);
                      if (pos == 0) //na_dd grabber only gives '..0/2' etc
                      {
                        Epos = pos;
                        pos = strSerEpNum.IndexOf(".", pos + 1);
                        strEpisodeNum = strSerEpNum.Substring(Epos + 1, (pos - 1) - Epos);
                        strEpisodePart = strSerEpNum.Substring(pos + 1, strSerEpNum.Length - (pos + 1));
                        if (strEpisodePart.IndexOf("/", 0) != -1)
                          // danish guide gives: episode-num system="xmltv_ns"> . 113 . </episode-num>
                        {
                          if (strEpisodePart.Substring(2, 1) == "1")
                          {
                            strEpisodePart = "";
                          }
                          else
                          {
                            int p = 0;
                            int t = 0;

                            if (Convert.ToInt32(strEpisodePart.Substring(0, 1)) == 0)
                            {
                              p = Convert.ToInt32(strEpisodePart.Substring(0, 1)) + 1;
                              t = Convert.ToInt32(strEpisodePart.Substring(2, 1));
                              strEpisodePart = Convert.ToString(p) + "/" + Convert.ToString(t);
                            }
                          }
                        }
                      }
                      else if (pos > 0)
                      {
                        strSeriesNum = strSerEpNum.Substring(0, pos);
                        Epos = pos;
                        pos = strSerEpNum.IndexOf(".", pos + 1);
                        strEpisodeNum = strSerEpNum.Substring(Epos + 1, (pos - 1) - Epos);
                        strEpisodePart = strSerEpNum.Substring(pos + 1, strSerEpNum.Length - (pos + 1));
                        if (strEpisodePart.IndexOf("/", 0) != -1)
                        {
                          if (strEpisodePart.Substring(2, 1) == "1")
                          {
                            strEpisodePart = "";
                          }
                          else
                          {
                            int p = 0;
                            int t = 0;
                            if (Convert.ToInt32(strEpisodePart.Substring(0, 1)) == 0)
                            {
                              p = Convert.ToInt32(strEpisodePart.Substring(0, 1)) + 1;
                            }
                            else
                            {
                              p = Convert.ToInt32(strEpisodePart.Substring(0, 1));
                            }
                            t = Convert.ToInt32(strEpisodePart.Substring(2, 1));
                            strEpisodePart = Convert.ToString(p) + "/" + Convert.ToString(t);
                          }
                        }
                      }
                      else
                      {
                        strSeriesNum = strSerEpNum;
                        strEpisodeNum = "";
                        strEpisodePart = "";
                      }
                    }
                  }
                  if (nodeDate != null && nodeDate.InnerText != null)
                  {
                    strDate = nodeDate.InnerText;
                  }
                  if (nodeStarRating != null && nodeStarRating.InnerText != null)
                  {
                    strStarRating = nodeStarRating.InnerText;
                  }
                  if (nodeClasification != null && nodeClasification.InnerText != null)
                  {
                    strClasification = nodeClasification.InnerText;
                  }

                  TVProgram prog = new TVProgram();
                  prog.Description = htmlUtil.ConvertHTMLToAnsi(strDescription);
                  prog.Start = iStart;
                  prog.End = iStop;
                  prog.Title = htmlUtil.ConvertHTMLToAnsi(strTitle);
                  prog.Episode = htmlUtil.ConvertHTMLToAnsi(strEpisode);
                  prog.Genre = htmlUtil.ConvertHTMLToAnsi(strCategory);
                  prog.Repeat = htmlUtil.ConvertHTMLToAnsi(strRepeat);
                  prog.Channel = htmlUtil.ConvertHTMLToAnsi(strChannelName);
                  prog.Date = strDate;
                  prog.SeriesNum = htmlUtil.ConvertHTMLToAnsi(strSeriesNum);
                  prog.EpisodeNum = htmlUtil.ConvertHTMLToAnsi(strEpisodeNum);
                  prog.EpisodePart = htmlUtil.ConvertHTMLToAnsi(strEpisodePart);
                  prog.StarRating = htmlUtil.ConvertHTMLToAnsi(strStarRating);
                  prog.Classification = htmlUtil.ConvertHTMLToAnsi(strClasification);
                  m_stats.Programs++;
                  if (bShowProgress && ShowProgress != null && (m_stats.Programs%100) == 0)
                  {
                    ShowProgress(m_stats);
                  }
                  foreach (ChannelPrograms progChan in Programs)
                  {
                    if (String.Compare(progChan.strName, strChannelName, true) == 0)
                    {
                      progChan.programs.Add(prog);
                    }
                  }
                }
              }
            }
            iProgram++;
          }
          m_stats.Programs = 0;
          m_stats.Status = GUILocalizeStrings.Get(644);
          if (bShowProgress && ShowProgress != null)
          {
            ShowProgress(m_stats);
          }
          DateTime dtStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
          //dtStartDate=dtStartDate.AddDays(-4);

          foreach (ChannelPrograms progChan in Programs)
          {
            progChan.programs.Sort(this);
            for (int i = 0; i < progChan.programs.Count; ++i)
            {
              TVProgram prog = (TVProgram) progChan.programs[i];
              if (prog.Start == prog.End)
              {
                if (i + 1 < progChan.programs.Count)
                {
                  TVProgram progNext = (TVProgram) progChan.programs[i + 1];
                  prog.End = progNext.Start;
                }
              }
            }
            RemoveOverlappingPrograms(ref progChan.programs); // be sure that we do not have any overlapping

            for (int i = 0; i < progChan.programs.Count; ++i)
            {
              TVProgram prog = (TVProgram) progChan.programs[i];
              // dont import programs which have already ended...
              if (prog.EndTime > dtStartDate)
              {
                Thread.Sleep(_backgroundDelay);
                Log.WriteFile(LogType.EPG, "epg-import :{0,-20} {1} {2}-{3} {4}",
                              prog.Channel,
                              prog.StartTime.ToShortDateString(),
                              prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                              prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                              prog.Title);

                TVDatabase.UpdateProgram(prog);
                if (prog.StartTime < m_stats.StartTime)
                {
                  m_stats.StartTime = prog.StartTime;
                }
                if (prog.EndTime > m_stats.EndTime)
                {
                  m_stats.EndTime = prog.EndTime;
                }
                m_stats.Programs++;
                if (bShowProgress && ShowProgress != null && (m_stats.Programs%100) == 0)
                {
                  ShowProgress(m_stats);
                }
              }
            }
          }
          TVDatabase.CommitTransaction();
          //TVDatabase.RemoveOverlappingPrograms();
          Programs.Clear();
          Programs = null;
          xml = null;
          m_bImport = false;
          TVDatabase.SupressEvents = false;
          if (iProgram > 0)
          {
            return true;
          }
          m_strErrorMessage = GUILocalizeStrings.Get(769); //"No programs found"
          return false;
        }
        else
        {
          m_strErrorMessage = GUILocalizeStrings.Get(770); // "No xmltv file found"
          m_stats.Status = m_strErrorMessage;
          Log.Info("xmltv data file was not found");
        }
      }
      catch (Exception ex)
      {
        m_strErrorMessage = String.Format("Invalid XML file:{0}", ex.Message);
        m_stats.Status = String.Format("invalid XML file:{0}", ex.Message);
        Log.Error("XML tv import error loading {0} err:{1} ", strFileName, ex.Message);
        TVDatabase.RollbackTransaction();
      }
      Programs.Clear();
      Programs = null;
      m_bImport = false;
      TVDatabase.SupressEvents = false;
      return false;
    }

    private int GetTimeOffset(string strTimeZone)
    {
      // timezone can b in format:
      // GMT +0100 or GMT -0500
      // or just +0300
      if (strTimeZone.Length == 0)
      {
        return 0;
      }
      strTimeZone = strTimeZone.ToLower();

      // just ignore GMT offsets, since we're calculating everything from GMT anyway
      if (strTimeZone.IndexOf("gmt") >= 0)
      {
        int ipos = strTimeZone.IndexOf("gmt");
        strTimeZone = strTimeZone.Substring(ipos + "GMT".Length);
      }

      strTimeZone = strTimeZone.Trim();
      if (strTimeZone[0] == '+' || strTimeZone[0] == '-')
      {
        string strOff = strTimeZone.Substring(1);
        try
        {
          int iOff = Int32.Parse(strOff);
          if (strTimeZone[0] == '-')
          {
            return -iOff;
          }
          else
          {
            return iOff;
          }
        }
        catch (Exception)
        {
        }
      }
      return 0;
    }

    private long CorrectIllegalDateTime(long datetime)
    {
      //format : 20050710245500
      long orgDateTime = datetime;
      long sec = datetime%100;
      datetime /= 100;
      long min = datetime%100;
      datetime /= 100;
      long hour = datetime%100;
      datetime /= 100;
      long day = datetime%100;
      datetime /= 100;
      long month = datetime%100;
      datetime /= 100;
      long year = datetime;
      DateTime dt = new DateTime((int) year, (int) month, (int) day, 0, 0, 0);
      dt = dt.AddHours(hour);
      dt = dt.AddMinutes(min);
      dt = dt.AddSeconds(sec);


      long newDateTime = Util.Utils.datetolong(dt);
      if (sec < 0 || sec > 59 ||
          min < 0 || min > 59 ||
          hour < 0 || hour >= 24 ||
          day < 0 || day > 31 ||
          month < 0 || month > 12)
      {
        Log.WriteFile(LogType.EPG, true, "epg-import:tvguide.xml contains invalid date/time :{0} converted it to:{1}",
                      orgDateTime, newDateTime);
      }

      return newDateTime;
    }

    public void RemoveOverlappingPrograms(ref ArrayList TVPrograms)
    {
      try
      {
        if (TVPrograms.Count == 0)
        {
          return;
        }
        TVPrograms.Sort(this);
        TVProgram prevProg = (TVProgram) TVPrograms[0];
        for (int i = 1; i < TVPrograms.Count; i++)
        {
          TVProgram newProg = (TVProgram) TVPrograms[i];
          if (newProg.Start < prevProg.End) // we have an overlap here
          {
            // let us find out which one is the correct one
            if (newProg.Start > prevProg.Start) // newProg will create hole -> delete it
            {
              TVPrograms.Remove(newProg);
              i--; // stay at the same position
              continue;
            }

            List<TVProgram> prevList = new List<TVProgram>();
            List<TVProgram> newList = new List<TVProgram>();
            prevList.Add(prevProg);
            newList.Add(newProg);
            TVProgram syncPrev = prevProg;
            TVProgram syncProg = newProg;
            for (int j = i + 1; j < TVPrograms.Count; j++)
            {
              TVProgram syncNew = (TVProgram) TVPrograms[j];
              if (syncPrev.End == syncNew.Start)
              {
                prevList.Add(syncNew);
                syncPrev = syncNew;
                if (syncNew.Start > syncProg.End)
                {
                  // stop point reached => delete TVPrograms in newList
                  foreach (TVProgram Prog in newList)
                  {
                    TVPrograms.Remove(Prog);
                  }
                  i = j - 1;
                  prevProg = syncPrev;
                  newList.Clear();
                  prevList.Clear();
                  break;
                }
              }
              else if (syncProg.End == syncNew.Start)
              {
                newList.Add(syncNew);
                syncProg = syncNew;
                if (syncNew.Start > syncPrev.End)
                {
                  // stop point reached => delete TVPrograms in prevList
                  foreach (TVProgram Prog in prevList)
                  {
                    TVPrograms.Remove(Prog);
                  }
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
              foreach (TVProgram Prog in prevList)
              {
                TVPrograms.Remove(Prog);
              }
              i = TVPrograms.Count;
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

    public void FillInMissingDataFromDB(ref ArrayList TVPrograms, ArrayList dbEPG)
    {
      TVPrograms.Sort(this);
      dbEPG.Sort(this);
      TVProgram prevProg = (TVProgram) TVPrograms[0];
      for (int i = 1; i < TVPrograms.Count; i++)
      {
        TVProgram newProg = (TVProgram) TVPrograms[i];
        if (newProg.Start > prevProg.End) // we have a gab here
        {
          // try to find data in the database
          foreach (TVProgram dbProg in dbEPG)
          {
            if ((dbProg.Start >= prevProg.End) && (dbProg.End <= newProg.Start))
            {
              TVPrograms.Insert(i, dbProg.Clone());
              i++;
              prevProg = dbProg;
            }
            if (dbProg.Start >= newProg.End)
            {
              break; // no more data available
            }
          }
        }
        prevProg = newProg;
      }
    }

    #region Sort Members

    public int Compare(object x, object y)
    {
      if (x == y)
      {
        return 0;
      }
      TVProgram item1 = (TVProgram) x;
      TVProgram item2 = (TVProgram) y;
      if (item1 == null)
      {
        return -1;
      }
      if (item2 == null)
      {
        return -1;
      }

      if (item1.Channel != item2.Channel)
      {
        return String.Compare(item1.Channel, item2.Channel, true);
      }
      if (item1.Start > item2.Start)
      {
        return 1;
      }
      if (item1.Start < item2.Start)
      {
        return -1;
      }
      return 0;
    }

    #endregion
  }
}