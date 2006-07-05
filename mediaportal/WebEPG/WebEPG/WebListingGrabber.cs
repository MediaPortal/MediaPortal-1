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
using System.Threading;
using System.Collections;
using System.Globalization;
using MediaPortal.Webepg.Profile;
using MediaPortal.TV.Database;
using MediaPortal.WebEPG;
using MediaPortal.Utils.Web;
using MediaPortal.Utils.Time;
using MediaPortal.Utils.Services;

namespace MediaPortal.EPG
{
  public enum Expect
  {
    Start,
    Morning,
    Afternoon
  }

  /// <summary>
  /// Summary description for Class1
  /// </summary>
  public class WebListingGrabber
  {
    WorldTimeZone _SiteTimeZone = null;
    HTTPRequest _listingRequest;
    HTTPRequest _requestSubURL;
    string _strID = string.Empty;
    string _strBaseDir = "";
    string _SubListingLink;
    string _strRepeat;
    string _strSubtitles;
    string _strEpNum;
    string _strEpTotal;
    string _removeProgramsList;
    string[] _strDayNames = null;
    string _strWeekDay;
    bool _grabLinked;
    bool _monthLookup;
    bool _searchRegex;
    bool _searchRemove;
    bool _dblookup = true;
    //bool _timeAdjustOnly;
    int _listingTime;
    int _linkStart;
    int _linkEnd;
    int _maxListingCount;
    int _pageStart;
    int _pageEnd;
    int _offsetStart;
    int _LastStart;
    int _grabDelay;
    int _guideDays;
    //int _addDays;
    bool _bNextDay;
    Profiler _templateProfile;
    //Parser _templateParser;
    Profiler _templateSubProfile;
    //Parser _templateSubParser;
    MediaPortal.Webepg.Profile.Xml _xmlreader;
    ArrayList _programs;
    ArrayList _dbPrograms;
    DateTime _StartGrab;
    int _dbLastProg;
    int _maxGrabDays;
    int _siteGuideDays;
    int _GrabDay;
    ILog _log;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maxGrabDays">The number of days to grab</param>
    /// <param name="baseDir">The baseDir for grabber files</param>
    public WebListingGrabber(int maxGrabDays, string baseDir)
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
      _maxGrabDays = maxGrabDays;
      _strBaseDir = baseDir;
    }

    public bool Initalise(string File)
    {
      string listingTemplate;

      _log.Info("WebEPG: Opening {0}", File);

      _xmlreader = new MediaPortal.Webepg.Profile.Xml(_strBaseDir + File);

      string baseUrl = _xmlreader.GetValueAsString("Listing", "BaseURL", "");
      if (baseUrl == "")
      {
        _log.Error("WebEPG: {0}: No BaseURL defined", File);
        return false;
      }

      string getQuery = _xmlreader.GetValueAsString("Listing", "SearchURL", "");
      string postQuery = _xmlreader.GetValueAsString("Listing", "PostQuery", "");
      _listingRequest = new HTTPRequest(baseUrl, getQuery, postQuery);

      _grabDelay = _xmlreader.GetValueAsInt("Listing", "GrabDelay", 500);
      _maxListingCount = _xmlreader.GetValueAsInt("Listing", "MaxCount", 0);
      _offsetStart = _xmlreader.GetValueAsInt("Listing", "OffsetStart", 0);
      _pageStart = _xmlreader.GetValueAsInt("Listing", "PageStart", 0);
      _pageEnd = _xmlreader.GetValueAsInt("Listing", "PageEnd", 0);
      _guideDays = _xmlreader.GetValueAsInt("Info", "GuideDays", 0);

      string strTimeZone = _xmlreader.GetValueAsString("Info", "TimeZone", "");
      if (strTimeZone != "")
      {
        //_timeAdjustOnly = _xmlreader.GetValueAsBool("Info", "TimeAdjustOnly", false);
        _log.Info("WebEPG: TimeZone, Local: {0}", TimeZone.CurrentTimeZone.StandardName);
        try
        {
          _log.Info("WebEPG: TimeZone, Site : {0}", strTimeZone);
          //_log.Info("[Debug] WebEPG: TimeZone, debug: {0}", _timeAdjustOnly);
          _SiteTimeZone = new WorldTimeZone(strTimeZone);
        }
        catch (ArgumentException)
        {
          _log.Error("WebEPG: TimeZone Not valid");
          _SiteTimeZone = null;
        }
      }
      else
      {
        _SiteTimeZone = null;
      }

      string ListingType = _xmlreader.GetValueAsString("Listing", "ListingType", "");

      switch (ListingType)
      {
        case "XML":
          XMLProfilerData data = new XMLProfilerData();
          data.ChannelEntry = _xmlreader.GetValueAsString("Listing", "ChannelEntry", "");
          data.StartEntry = _xmlreader.GetValueAsString("Listing", "StartEntry", "");
          data.EndEntry = _xmlreader.GetValueAsString("Listing", "EndEntry", "");
          data.TitleEntry = _xmlreader.GetValueAsString("Listing", "TitleEntry", "");
          data.SubtitleEntry = _xmlreader.GetValueAsString("Listing", "SubtitleEntry", "");
          data.DescEntry = _xmlreader.GetValueAsString("Listing", "DescEntry", "");
          data.GenreEntry = _xmlreader.GetValueAsString("Listing", "GenreEntry", "");
          data.XPath = _xmlreader.GetValueAsString("Listing", "XPath", "");
          _templateProfile = new XMLProfiler("", data);
          break;

        case "DATA":
          string strListingDelimitor = _xmlreader.GetValueAsString("Listing", "ListingDelimitor", "\n");
          string strDataDelimitor = _xmlreader.GetValueAsString("Listing", "DataDelimitor", "\t");
          listingTemplate = _xmlreader.GetValueAsString("Listing", "Template", "");
          if (listingTemplate == "")
          {
            _log.Error("WebEPG: {0}: No Template", File);
            return false;
          }
          _templateProfile = new DataProfiler(listingTemplate, strDataDelimitor[0], strListingDelimitor[0]);
          break;

        default: // HTML
          _siteGuideDays = _xmlreader.GetValueAsInt("Info", "GuideDays", 0);
          if (_siteGuideDays < _maxGrabDays)
          {
            _log.Warn("WebEPG: GrabDays {0} more than GuideDays {0}, limiting grab days", _siteGuideDays, _maxGrabDays);
            _maxGrabDays = _siteGuideDays;
          }
          string strGuideStart = _xmlreader.GetValueAsString("Listing", "Start", "<body");
          string strGuideEnd = _xmlreader.GetValueAsString("Listing", "End", "</body");
          //bool bAhrefs = _xmlreader.GetValueAsBool("Listing", "Ahrefs", false);
          string tags = _xmlreader.GetValueAsString("Listing", "Tags", "T");
          string encoding = _xmlreader.GetValueAsString("Listing", "Encoding", "");
          listingTemplate = _xmlreader.GetValueAsString("Listing", "Template", "");
          if (listingTemplate == "")
          {
            _log.Error("WebEPG: {0}: No Template", File);
            return false;
          }
          //_templateProfile = new HTMLProfiler(listingTemplate, bAhrefs, strGuideStart, strGuideEnd);
          _templateProfile = new HTMLProfiler(listingTemplate, tags, strGuideStart, strGuideEnd, encoding);

          _searchRegex = _xmlreader.GetValueAsBool("Listing", "SearchRegex", false);
          if (_searchRegex)
          {
            _searchRemove = _xmlreader.GetValueAsBool("Listing", "SearchRemove", false);
            _strRepeat = _xmlreader.GetValueAsString("Listing", "SearchRepeat", "");
            _strSubtitles = _xmlreader.GetValueAsString("Listing", "SearchSubtitles", "");
            _strEpNum = _xmlreader.GetValueAsString("Listing", "SearchEpNum", "");
            _strEpTotal = _xmlreader.GetValueAsString("Listing", "SearchEpTotal", "");
          }

          _SubListingLink = _xmlreader.GetValueAsString("Listing", "SubListingLink", "");
          if (_SubListingLink != "")
          {
            string strSubStart = _xmlreader.GetValueAsString("SubListing", "Start", "<body");
            string strSubEnd = _xmlreader.GetValueAsString("SubListing", "End", "</body");
            string subencoding = _xmlreader.GetValueAsString("SubListing", "Encoding", "");
            string subUrl = _xmlreader.GetValueAsString("SubListing", "URL", "");
            if (subUrl != "")
            {
              _requestSubURL = new HTTPRequest(subUrl);
              _requestSubURL.PostQuery = _xmlreader.GetValueAsString("SubListing", "PostQuery", "");
            }
            string Subtags = _xmlreader.GetValueAsString("SubListing", "Tags", "T");
            string sublistingTemplate = _xmlreader.GetValueAsString("SubListing", "Template", "");
            if (sublistingTemplate == "")
            {
              _log.Error("WebEPG: {0}: No SubTemplate", File);
              _SubListingLink = "";
            }
            else
            {
              _templateSubProfile = new HTMLProfiler(sublistingTemplate, Subtags, strSubStart, strSubEnd, subencoding);
            }
          }

          string firstDay = _xmlreader.GetValueAsString("DayNames", "0", "");
          if (firstDay != "" && _guideDays != 0)
          {
            _strDayNames = new string[_guideDays];
            _strDayNames[0] = firstDay;
            for (int i = 1; i < _guideDays; i++)
              _strDayNames[i] = _xmlreader.GetValueAsString("DayNames", i.ToString(), "");
          }
          break;
      }

      _monthLookup = _xmlreader.GetValueAsBool("DateTime", "Months", false);
      return true;
    }

    public long GetEpochTime(DateTime dtCurTime)
    {
      DateTime dtEpochStartTime = Convert.ToDateTime("1/1/1970 8:00:00 AM");
      TimeSpan ts = dtCurTime.Subtract(dtEpochStartTime);

      long epochtime;
      epochtime = ((((((ts.Days * 24) + ts.Hours) * 60) + ts.Minutes) * 60) + ts.Seconds);
      return epochtime;
    }

    public long GetEpochDate(DateTime dtCurTime)
    {
      DateTime dtEpochStartTime = Convert.ToDateTime("1/1/1970 8:00:00 AM");
      TimeSpan ts = dtCurTime.Subtract(dtEpochStartTime);

      long epochdate;
      epochdate = (ts.Days);
      return epochdate;
    }

    private int getMonth(string month)
    {
      if (_monthLookup)
        return _xmlreader.GetValueAsInt("DateTime", month, 0);
      else
        return int.Parse(month);
    }

    private string getGenre(string genre)
    {
      return _xmlreader.GetValueAsString("GenreMap", genre, genre);
    }

    private long GetLongDateTime(DateTime dt)
    {
      long lDatetime;

      lDatetime = dt.Year;
      lDatetime *= 100;
      lDatetime += dt.Month;
      lDatetime *= 100;
      lDatetime += dt.Day;
      lDatetime *= 100;
      lDatetime += dt.Hour;
      lDatetime *= 100;
      lDatetime += dt.Minute;
      lDatetime *= 100;
      // no seconds

      return lDatetime;
    }

    private TVProgram dbProgram(string Title, long Start)
    {
      if (_dbPrograms.Count > 0)
      {
        for (int i = _dbLastProg; i < _dbPrograms.Count; i++)
        {
          TVProgram prog = (TVProgram)_dbPrograms[i];

          if (prog.Title == Title && prog.Start == Start)
          {
            _dbLastProg = i;
            return prog;
          }
        }

        for (int i = 0; i < _dbLastProg; i++)
        {
          TVProgram prog = (TVProgram)_dbPrograms[i];

          if (prog.Title == Title && prog.Start == Start)
          {
            _dbLastProg = i;
            return prog;
          }
        }
      }
      return null;
    }

    private bool AdjustTime(ref ProgramData guideData)
    {
      int addDays = 1;

      // Day
      if (guideData.Day == 0)
      {
        guideData.Day = _StartGrab.Day;
      }
      else
      {
        if (guideData.Day != _StartGrab.Day && _listingTime != (int)Expect.Start)
        {
          _GrabDay++;
          _StartGrab = _StartGrab.AddDays(1);
          _bNextDay = false;
          _LastStart = 0;
          _listingTime = (int)Expect.Morning;
        }
      }

      // Start Time
      switch (_listingTime)
      {
        case (int)Expect.Start:
          if (_GrabDay == 0)
          {
            if (guideData.StartTime.Hour < _StartGrab.Hour)
              return false;

            if (guideData.StartTime.Hour <= 12)
            {
              _listingTime = (int)Expect.Morning;
              goto case (int)Expect.Morning;
            }

            _listingTime = (int)Expect.Afternoon;
            goto case (int)Expect.Afternoon;
          }

          if (guideData.StartTime.Hour >= 20)
            return false;				// Guide starts on pervious day ignore these listings.

          _listingTime = (int)Expect.Morning;
          goto case (int)Expect.Morning;      // Pass into Morning Code

        case (int)Expect.Morning:
          if (_LastStart > guideData.StartTime.Hour)
          {
            _listingTime = (int)Expect.Afternoon;
            //if (_bNextDay)
            //{
            //    _GrabDay++;
            //}
          }
          else
          {
            if (guideData.StartTime.Hour <= 12)
              break;						// Do nothing
          }

          // Pass into Afternoon Code
          //_LastStart = 0;
          goto case (int)Expect.Afternoon;

        case (int)Expect.Afternoon:
          if (guideData.StartTime.Hour < 12)		// Site doesn't have correct time
            guideData.StartTime.Hour += 12;     // starts again at 1:00 with "pm"

          if (_LastStart > guideData.StartTime.Hour)
          {
            guideData.StartTime.Hour -= 12;
            if (_bNextDay)
            {
              addDays++;
              _GrabDay++;
              _StartGrab = _StartGrab.AddDays(1);
              //_bNextDay = false;
            }
            else
            {
              _bNextDay = true;
            }
            _listingTime = (int)Expect.Morning;
            break;
          }

          break;

        default:
          break;
      }


      //Month
      int month;
      if (guideData.Month == "")
      {
        month = _StartGrab.Month;
      }
      else
      {
        month = getMonth(guideData.Month);
      }

      // Create DateTime
      DateTime dtStart;
      try
      {
        dtStart = new DateTime(_StartGrab.Year, month, guideData.Day, guideData.StartTime.Hour, guideData.StartTime.Minute, 0, 0);
      }
      catch
      {
        _log.Error("WebEPG: DateTime Error Program: {0}", guideData.Title);
        return false; // DateTime error
      }
      if (_bNextDay)
        dtStart = dtStart.AddDays(addDays);

      guideData.StartTime.Hour = dtStart.Hour;
      guideData.StartTime.Minute = dtStart.Minute;
      guideData.StartTime.Day = dtStart.Day;
      guideData.StartTime.Month = dtStart.Month;
      guideData.StartTime.Year = dtStart.Year;

      _LastStart = guideData.StartTime.Hour;

      if (guideData.EndTime != null)
      {
        DateTime dtEnd = new DateTime(_StartGrab.Year, month, guideData.Day, guideData.EndTime.Hour, guideData.EndTime.Minute, 0, 0);
        if (_bNextDay)
        {
          if (guideData.StartTime.Hour > guideData.EndTime.Hour)
            dtEnd = dtEnd.AddDays(addDays + 1);
          else
            dtEnd = dtEnd.AddDays(addDays);
        }
        else
        {
          if (guideData.StartTime.Hour > guideData.EndTime.Hour)
            dtEnd = dtEnd.AddDays(addDays);
        }

        guideData.EndTime.Hour = dtEnd.Hour;
        guideData.EndTime.Minute = dtEnd.Minute;
        guideData.EndTime.Day = dtEnd.Day;
        guideData.EndTime.Month = dtEnd.Month;
        guideData.EndTime.Year = dtEnd.Year;
      }

      _log.Debug("WebEPG: Guide, Program Debug: [{0} {1}]", _GrabDay, _bNextDay);

      return true;
    }

    private void AdjustTimeZone(ProgramData guideData, ref TVProgram program)
    {
      // Start Time
      DateTime dtStart = new DateTime(guideData.StartTime.Year, guideData.StartTime.Month, guideData.StartTime.Day, guideData.StartTime.Hour, guideData.StartTime.Minute, 0);

      // Check TimeZone
      if (_SiteTimeZone != null && !_SiteTimeZone.IsLocalTimeZone())
      {
        _log.Debug("WebEPG: TimeZone, Adjusting from start Guide Time: {0} {1}", dtStart.ToShortTimeString(), dtStart.ToShortDateString());
        dtStart = _SiteTimeZone.ToLocalTime(dtStart);
        //if (_timeAdjustOnly)
        //  dtStart = new DateTime(guideData.StartTime.Year, guideData.StartTime.Month, guideData.StartTime.Day, dtStart.Hour, dtStart.Minute, 0);
        _log.Debug("WebEPG: TimeZone, Adjusting to   start Local Time: {0} {1}", dtStart.ToShortTimeString(), dtStart.ToShortDateString());
      }

      program.Start = GetLongDateTime(dtStart);

      // End Time
      if (guideData.EndTime != null)
      {
        DateTime dtEnd = new DateTime(guideData.EndTime.Year, guideData.EndTime.Month, guideData.EndTime.Day, guideData.EndTime.Hour, guideData.EndTime.Minute, 0);

        // Check TimeZone
        if (_SiteTimeZone != null && !_SiteTimeZone.IsLocalTimeZone())
        {
          _log.Debug("WebEPG: TimeZone, Adjusting from end Guide Time: {0} {1}", dtEnd.ToShortTimeString(), dtEnd.ToShortDateString());
          dtEnd = _SiteTimeZone.ToLocalTime(dtEnd);
          //if (_timeAdjustOnly)
          //  dtEnd = new DateTime(guideData.EndTime.Year, guideData.EndTime.Month, guideData.EndTime.Day, dtEnd.Hour, dtEnd.Minute, 0, 0);
          _log.Debug("WebEPG: TimeZone, Adjusting to   end Local Time: {0} {1}", dtEnd.ToShortTimeString(), dtEnd.ToShortDateString());
        }

        program.End = GetLongDateTime(dtEnd);

        _log.Info("WebEPG: Guide, Program Info: {0} / {1} - {2}", program.Start, program.End, guideData.Title);
      }
      else
      {
        _log.Info("WebEPG: Guide, Program Info: {0} - {1}", program.Start, guideData.Title);
      }
    }

    private TVProgram GetProgram(Profiler guideProfile, int index)
    {
      //Parser Listing = guideProfile.GetProfileParser(index);

      TVProgram program = new TVProgram();
      HTMLProfiler htmlProf = null;
      if (guideProfile is HTMLProfiler)
      {
        htmlProf = (HTMLProfiler)guideProfile;

        if (_searchRegex)
        {
          string repeat = htmlProf.SearchRegex(index, _strRepeat, _searchRemove);
          string subtitles = htmlProf.SearchRegex(index, _strSubtitles, _searchRemove);
          string epNum = htmlProf.SearchRegex(index, _strEpNum, _searchRemove);
          string epTotal = htmlProf.SearchRegex(index, _strEpTotal, _searchRemove);
        }
      }
      ProgramData guideData = new ProgramData();
      ParserData data = (ParserData)guideData;
      guideProfile.GetParserData(index, ref data); //_templateParser.GetProgram(Listing);

      if (guideData.StartTime == null || guideData.Title == "")
        return null;

      if (guideData.IsProgram(_removeProgramsList))
        return null;

      _log.Debug("WebEPG: Guide, Program title: {0}", guideData.Title);
      _log.Debug("WebEPG: Guide, Program start: {0}:{1} - {2}/{3}/{4}", guideData.StartTime.Hour, guideData.StartTime.Minute, guideData.StartTime.Day, guideData.StartTime.Month, guideData.StartTime.Year);
      if (guideData.EndTime != null)
        _log.Debug("WebEPG: Guide, Program end  : {0}:{1} - {2}/{3}/{4}", guideData.EndTime.Hour, guideData.EndTime.Minute, guideData.EndTime.Day, guideData.EndTime.Month, guideData.EndTime.Year);
      _log.Debug("WebEPG: Guide, Program desc.: {0}", guideData.Description);
      _log.Debug("WebEPG: Guide, Program genre: {0}", guideData.Genre);

      program.Channel = _strID;
      program.Title = guideData.Title;

      // Adjust Time
      if (guideData.StartTime.Day == 0 && guideData.StartTime.Month == 0 && guideData.StartTime.Year == 0)
      {
        if (!AdjustTime(ref guideData))
          return null;
      }

      //Adjust TimeZone
      AdjustTimeZone(guideData, ref program);

      // Check TV db if program exists
      if (_dblookup)
      {
        TVProgram dbProg = dbProgram(program.Title, program.Start);
        if (dbProg != null)
        {
          _log.Info("WebEPG: Program in db copying it");
          dbProg.Channel = _strID;
          return dbProg;
        }
      }

      if (guideData.Description != "")
        program.Description = guideData.Description;

      if (guideData.Genre != "")
        program.Genre = getGenre(guideData.Genre);

      // SubLink
      if (_grabLinked && _SubListingLink != ""
         && guideData.StartTime.Hour >= _linkStart
         && guideData.StartTime.Hour <= _linkEnd
         && htmlProf != null)
      {
        HTTPRequest sublinkRequest;
        if (_requestSubURL != null)
          sublinkRequest = new HTTPRequest(_requestSubURL);
        else
          sublinkRequest = new HTTPRequest(_listingRequest.Url);


        if (htmlProf.GetHyperLink(index, _SubListingLink, ref sublinkRequest))
        {
          _log.Info("WebEPG: Reading {0}", sublinkRequest.ToString());
          Thread.Sleep(_grabDelay);
          Profiler SubProfile = _templateSubProfile.GetPageProfiler(sublinkRequest);
          int Count = 0;
          if (SubProfile != null)
            Count = SubProfile.subProfileCount();
          else
            _log.Error("Linked page error");

          if (Count > 0)
          {
            _log.Debug("Count {0}", Count);
            ProgramData SubData = new ProgramData();
            ParserData refdata = (ParserData)SubData;
            SubProfile.GetParserData(0, ref refdata);

            if (SubData.IsProgram(_removeProgramsList))
              return null;

            //if (program.EndTime == null && SubData.EndTime != null)
            //{
            //  program.EndTime = SubData.EndTime;
            //  _log.Info("[Debug] WebEPG: Guide, Program end  : {0}:{1} - {2}/{3}/{4}", SubData.EndTime.Hour, guideData.EndTime.Minute, SubData.EndTime.Day, SubData.EndTime.Month, guideData.EndTime.Year);
            //}

            if (SubData.Description != "")
            {
              program.Description = SubData.Description;
              _log.Debug("WebEPG: Guide, Program desc.: {0}", program.Description);
            }

            if (SubData.Genre != "")
            {
              program.Genre = getGenre(SubData.Genre);
              _log.Debug("WebEPG: Guide, Program genre: {0}", program.Genre);
            }

            if (SubData.SubTitle != "")
              program.Episode = SubData.SubTitle;
          }
          else
          {
            _log.Warn("No information found on linked page");
          }
        }

      }

      return program;
    }

    private bool GetListing(HTTPRequest request, int offset, string strChannel, out bool error)
    {
      Profiler guideProfile;
      int listingCount = 0;
      bool bMore = false;
      error = false;

      request.ReplaceTag("[LIST_OFFSET]", (offset * _maxListingCount).ToString());
      request.ReplaceTag("[PAGE_OFFSET]", (offset + _pageStart).ToString());

      _log.Info("WebEPG: Reading {0}", request.ToString());

      if (_templateProfile is XMLProfiler)
      {
        XMLProfiler templateProfile = (XMLProfiler)_templateProfile;
        templateProfile.SetChannelID(strChannel);
      }
      guideProfile = _templateProfile.GetPageProfiler(request);
      if (guideProfile != null)
        listingCount = guideProfile.subProfileCount();

      if (listingCount == 0) // && _maxListingCount == 0)
      {
        if (_maxListingCount == 0 || (_maxListingCount != 0 && offset == 0))
        {
          _log.Info("WebEPG: No Listings Found");
          _GrabDay++;
          error = true;
        }
        else
        {
          _log.Info("WebEPG: Listing Count 0");
        }
        //_GrabDay++;
      }
      else
      {
        _log.Info("WebEPG: Listing Count {0}", listingCount);

        if (listingCount == _maxListingCount) // || _pageStart + offset < _pageEnd)
          bMore = true;

        for (int i = 0; i < listingCount; i++)
        {
          TVProgram program = GetProgram(guideProfile, i);
          if (program != null)
          {
            _programs.Add(program);
          }
        }

        if (_GrabDay > _maxGrabDays)
          bMore = false;
      }

      return bMore;
    }

    public ArrayList GetGuide(string strChannelID, bool Linked, int linkStart, int linkEnd)
    {
      return GetGuide(strChannelID, Linked, linkStart, linkEnd, DateTime.Now);
    }

    public ArrayList GetGuide(string strChannelID, bool Linked, int linkStart, int linkEnd, DateTime startDateTime)
    {
      _strID = strChannelID;
      _grabLinked = Linked;
      _linkStart = linkStart;
      _linkEnd = linkEnd;
      int offset = 0;

      string searchID = _xmlreader.GetValueAsString("ChannelList", strChannelID, "");
      string searchLang = _xmlreader.GetValueAsString("Listing", "SearchLanguage", "en-US");
      _strWeekDay = _xmlreader.GetValueAsString("Listing", "WeekdayString", "dddd");
      CultureInfo culture = new CultureInfo(searchLang);

      _removeProgramsList = _xmlreader.GetValueAsString("RemovePrograms", "*", "");
      if (_removeProgramsList != "")
        _removeProgramsList += ";";
      string chanRemovePrograms = _xmlreader.GetValueAsString("RemovePrograms", strChannelID, "");
      if (chanRemovePrograms != "")
      {
        _removeProgramsList += chanRemovePrograms;
        _removeProgramsList += ";";
      }

      if (searchID == "")
      {
        _log.Info("WebEPG: ChannelId: {0} not found!", strChannelID);
        return null;
      }

      _programs = new ArrayList();

      HTTPRequest channelRequest = new HTTPRequest(_listingRequest);
      channelRequest.ReplaceTag("[ID]", searchID);
      HTTPRequest pageRequest;

      _log.Info("WebEPG: ChannelId: {0}", strChannelID);

      _GrabDay = 0;
      _StartGrab = startDateTime;
      _log.Debug("WebEPG: Grab Start {0} {1}", _StartGrab.ToShortTimeString(), _StartGrab.ToShortDateString());
      int requestedStartDay = startDateTime.Subtract(DateTime.Now).Days;
      if (requestedStartDay > 0)
      {
        if (requestedStartDay > _siteGuideDays)
        {
          _log.Error("WebEPG: Trying to grab pass guide days");
          return null;
        }

        if (requestedStartDay + _maxGrabDays > _siteGuideDays)
        {
          _maxGrabDays = _siteGuideDays - requestedStartDay;
          _log.Warn("WebEPG: Grab days more than Guide days, limiting to {0}", _maxGrabDays);
        }

        _GrabDay = requestedStartDay;
        if (_GrabDay > _maxGrabDays)
          _maxGrabDays = _GrabDay + _maxGrabDays;
      }

      //TVDatabase.BeginTransaction();
      //TVDatabase.ClearCache();
      //TVDatabase.RemoveOldPrograms();

      int dbChannelId;
      string dbChannelName;
      _dbPrograms = new ArrayList();
      _dbLastProg = 0;

      try
      {
        if (TVDatabase.GetEPGMapping(strChannelID, out dbChannelId, out dbChannelName)) // (nodeId.InnerText, out idTvChannel, out strTvChannel);
        {
          DateTime endGrab = _StartGrab.AddDays(_maxGrabDays + 1);
          DateTime startGrab = _StartGrab.AddHours(-1);
          TVDatabase.GetProgramsPerChannel(dbChannelName, GetLongDateTime(startGrab), GetLongDateTime(endGrab), ref _dbPrograms);
        }
      }
      catch (Exception)
      {
        _log.Error("WebEPG: Database failed, disabling db lookup");
        _dblookup = false;
      }


      while (_GrabDay < _maxGrabDays)
      {
        pageRequest = new HTTPRequest(channelRequest);
        if (_strDayNames != null)
          pageRequest.ReplaceTag("[DAY_NAME]", _strDayNames[_GrabDay]);

        pageRequest.ReplaceTag("[DAY_OFFSET]", (_GrabDay + _offsetStart).ToString());
        pageRequest.ReplaceTag("[EPOCH_TIME]", GetEpochTime(_StartGrab).ToString());
        pageRequest.ReplaceTag("[EPOCH_DATE]", GetEpochDate(_StartGrab).ToString());
        pageRequest.ReplaceTag("[DAYOFYEAR]", _StartGrab.DayOfYear.ToString());
        pageRequest.ReplaceTag("[YYYY]", _StartGrab.Year.ToString());
        pageRequest.ReplaceTag("[MM]", String.Format("{0:00}", _StartGrab.Month));
        pageRequest.ReplaceTag("[_M]", _StartGrab.Month.ToString());
        pageRequest.ReplaceTag("[MONTH]", _StartGrab.ToString("MMMM", culture));
        pageRequest.ReplaceTag("[DD]", String.Format("{0:00}", _StartGrab.Day));
        pageRequest.ReplaceTag("[_D]", _StartGrab.Day.ToString());
        pageRequest.ReplaceTag("[WEEKDAY]", _StartGrab.ToString(_strWeekDay, culture));

        offset = 0;
        _LastStart = 0;
        _bNextDay = false;
        _listingTime = (int)Expect.Start;

        bool error;
        while (GetListing(new HTTPRequest(pageRequest), offset, searchID, out error))
        {
          Thread.Sleep(_grabDelay);
          if (_maxListingCount == 0)
            break;
          offset++; // += _maxListingCount;
        }
        if (error)
        {
          _log.Error("WebEPG: ChannelId: {0} grabber error", strChannelID);
          break;
        }
        //_GrabDay++;
        if (channelRequest != pageRequest)
        {
          _StartGrab = _StartGrab.AddDays(1);
          _GrabDay++;
        }
        else
        {
          if (!pageRequest.HasTag("[LIST_OFFSET]"))
            break;
        }
      }

      return _programs;
    }
  }
}
