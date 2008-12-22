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
using System.Runtime.InteropServices;
using System.ComponentModel;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using DShowNET.MPSA;
using MediaPortal.GUI.Library;
using MediaPortal.Radio.Database;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.Configuration;
using System.Threading;

namespace MediaPortal.TV.Epg
{
  /// <summary>
  /// EPG Grabber
  /// this class receives the EPG data for DVB from the MPSA directshow filter
  /// and will update the TV database with all new programs found
  /// </summary>
  public class EpgGrabber
  {

    #region enums
    enum State
    {
      Idle,
      Grabbing,
      Parsing,
      Updating,
      Done
    }
    #endregion

    #region variables
    IEPGGrabber _epgInterface = null;
    IATSCGrabber _atscInterface = null;
    IMHWGrabber _mhwInterface = null;
    IStreamAnalyzer _analyzerInterface = null;
    NetworkType _networkType;
    bool _grabEPG = false;
    DateTime _timeoutTimer = DateTime.Now;

    State _currentState = State.Idle;
    List<EPGChannel> _listChannels;
    List<MHWEvent> _listMhwEvents;
    List<ATSCEvent> _listAtscEvents;
    List<EpgChannelUpdate> _epgChannels;
    string _epgTvChannelName = string.Empty;
    #endregion

    #region properties
    public IEPGGrabber EPGInterface
    {
      get { return _epgInterface; }
      set { _epgInterface = value; }
    }
    public IStreamAnalyzer AnalyzerInterface
    {
      get { return _analyzerInterface; }
      set { _analyzerInterface = value; }
    }
    public IMHWGrabber MHWInterface
    {
      get { return _mhwInterface; }
      set { _mhwInterface = value; }
    }
    public IATSCGrabber ATSCInterface
    {
      get { return _atscInterface; }
      set { _atscInterface = value; }
    }
    public NetworkType Network
    {
      get { return _networkType; }
      set { _networkType = value; }
    }
    #endregion

    #region Constructors
    public EpgGrabber()
    {
    }

    #endregion

    #region public methods

    public void GrabEPG(string tvChannelName, bool epg)
    {
      _epgTvChannelName = tvChannelName;
      _timeoutTimer = DateTime.Now;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        bool enabled = xmlreader.GetValueAsBool("xmltv", "epgdvb", true);
        if (!enabled)
        {
          Log.WriteFile(LogType.EPG, "epg-grab: EPG grabber disabled");
          return;
        }
      }

      _grabEPG = epg;
      if (Network == NetworkType.ATSC)
      {
        if (_atscInterface != null)
        {
          _currentState = State.Grabbing;
          Log.WriteFile(LogType.EPG, "epg-grab: start ATSC grabber:{0}", _epgTvChannelName);
          _atscInterface.GrabATSC();
          _timeoutTimer = DateTime.Now;
        }
      }
      else
      {
        if (_grabEPG)
        {
          _currentState = State.Grabbing;
          if (_epgInterface != null)
          {
            Log.WriteFile(LogType.EPG, "epg-grab: start EPG grabber:{0}", _epgTvChannelName);
            _epgInterface.GrabEPG();
            _timeoutTimer = DateTime.Now;
          }
          else
          {
            Log.WriteFile(LogType.EPG, "epg-grab: start EPG grabber:epginterface=null");
          }
        }
        else
        {
          _currentState = State.Grabbing;
          if (_mhwInterface != null)
          {
            Log.WriteFile(LogType.EPG, "epg-grab: start MHW grabber:{0}", _epgTvChannelName);
            _mhwInterface.GrabMHW();
            _timeoutTimer = DateTime.Now;
          }
          else
          {
            Log.WriteFile(LogType.EPG, "epg-grab: start MHW grabber:MHWinterface=null");
          }
        }
      }
    }

    public void Process()
    {
      try
      {
        if (_currentState != State.Idle)
        {
          TimeSpan ts = DateTime.Now - _timeoutTimer;
          if (ts.TotalMinutes >= 10)
          {
            Log.Info("epg-grab: timeout...");
            OnDone();
            _currentState = State.Done;
          }
        }
        bool ready = false;
        switch (_currentState)
        {
          case State.Grabbing:
            if (Network == NetworkType.ATSC)
            {
              if (_atscInterface != null)
              {
                _atscInterface.IsATSCReady(out ready);
                if (ready)
                {
                  Log.Info("epg-grab: ATSC EPG ready...");
                  _timeoutTimer = DateTime.Now;
                  _currentState = State.Parsing;
                }
              }
            }
            else if (_epgInterface != null && _grabEPG)
            {
              _epgInterface.IsEPGReady(out ready);
              uint count;
              _epgInterface.GetEPGChannelCount(out count);
              if (ready)
              {
                Log.Info("epg-grab: EPG ready...");
                _timeoutTimer = DateTime.Now;
                _currentState = State.Parsing;
              }
            }
            else if (_mhwInterface != null && !_grabEPG)
            {
              _mhwInterface.IsMHWReady(out ready);
              if (ready)
              {
                Log.Info("epg-grab: MHW ready...");
                _timeoutTimer = DateTime.Now;
                if (_epgInterface != null)
                {
                  short titleCount;
                  _mhwInterface.GetMHWTitleCount(out titleCount);
                  if (titleCount <= 0)
                  {
                    Log.Info("epg-grab: no MHW events, try DVB EPG:{0}", _epgTvChannelName);
                    _grabEPG = true;
                    _epgInterface.GrabEPG();
                  }
                  else
                  {
                    _currentState = State.Parsing;
                  }
                }
                else
                {
                  _currentState = State.Parsing;
                }
              }
            }
            break;

          case State.Parsing:
            if (Network == NetworkType.ATSC)
            {
              Log.Info("epg-grab: ATSC parsing...");
              ParseATSC();
            }
            else if (_grabEPG)
            {
              Log.Info("epg-grab: EPG parsing...");
              ParseEPG();
            }
            else
            {
              Log.Info("epg-grab: MHW parsing...");
              ParseMHW();
            }

            _currentState = State.Updating;
            break;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void Reset()
    {
      Log.WriteFile(LogType.EPG, "epg-grab: reset");
      _currentState = State.Idle;
    }
    public bool Done
    {
      get
      {
        return _currentState == State.Done;
      }
    }
    #endregion

    #region private methods

    #region ATSC
    void ParseATSC()
    {
      _listAtscEvents = new List<ATSCEvent>();
      if (_analyzerInterface == null) return;
      ushort titleCount;
      _atscInterface.GetATSCTitleCount(out titleCount);
      Log.WriteFile(LogType.EPG, "atsc-epg: received {0} titles", titleCount);
      for (short i = 0; i < titleCount; ++i)
      {
        Int16 source_id, length_in_mins;
        uint starttime;
        IntPtr ptrTitle, ptrDescription;
        _atscInterface.GetATSCTitle(i, out source_id, out starttime, out length_in_mins, out ptrTitle, out ptrDescription);
        string title, description;
        title = Marshal.PtrToStringAnsi(ptrTitle);
        description = Marshal.PtrToStringAnsi(ptrDescription);
        if (title == null) title = "";
        if (description == null) description = "";
        title = title.Trim();
        description = description.Trim();

        if (title.Length == 0) continue;
        DateTime programStartTimeUTC = new DateTime(1980, 1, 6, 0, 0, 0, 0);
        programStartTimeUTC.AddSeconds(starttime);
        DateTime programStartTime = programStartTimeUTC.ToLocalTime();

        if (title == null)
          title = string.Empty;

        if (description == null)
          description = string.Empty;
        if (title == string.Empty || title == "n.a.")
        {
          continue;
        }


        // get channel info
        DVBSections.ChannelInfo chi = new MediaPortal.TV.Recording.DVBSections.ChannelInfo();
        DVBSections sections = new DVBSections();
        UInt16 len = 0;
        int hr = 0;
        hr = _analyzerInterface.GetCISize(ref len);
        IntPtr mmch = Marshal.AllocCoTaskMem(len);
        hr = _analyzerInterface.GetChannel((UInt16)source_id, mmch);
        chi = sections.GetChannelInfo(mmch);
        Marshal.FreeCoTaskMem(mmch);

        ATSCEvent atscEvent = new ATSCEvent(Network, chi.majorChannel, chi.minorChannel, title, description, "", programStartTime, programStartTime.AddMinutes(length_in_mins));
        _listAtscEvents.Add(atscEvent);

      }
      BackgroundWorker atscWorker = new BackgroundWorker();
      atscWorker.DoWork += new DoWorkEventHandler(this.AtscBackgroundWorker);
      atscWorker.RunWorkerAsync();
    }
    #endregion

    #region MHW
    void ParseMHW()
    {
      _listMhwEvents = new List<MHWEvent>();
      short titleCount;
      _mhwInterface.GetMHWTitleCount(out titleCount);
      Log.WriteFile(LogType.EPG, "mhw-grab: received {0} programs", titleCount);
      for (int i = 0; i < titleCount; ++i)
      {
        ushort id = 0, transportid = 0, networkid = 0, channelnr = 0, channelid = 0, PPV = 0, duration = 0;
        short programid = 0, themeid = 0;
        byte summaries = 0;
        uint datestart = 0, timestart = 0;
        IntPtr ptrTitle, ptrProgramName;
        IntPtr ptrChannelName, ptrSummary, ptrTheme;
        _mhwInterface.GetMHWTitle((short)i, ref id, ref transportid, ref networkid, ref channelnr, ref programid, ref themeid, ref PPV, ref summaries, ref duration, ref datestart, ref timestart, out ptrTitle, out ptrProgramName);
        _mhwInterface.GetMHWChannel(channelnr, ref channelid, ref networkid, ref transportid, out ptrChannelName);
        _mhwInterface.GetMHWSummary(programid, out ptrSummary);
        _mhwInterface.GetMHWTheme(themeid, out ptrTheme);

        string channelName, title, programName, summary, theme;
        channelName = Marshal.PtrToStringAnsi(ptrChannelName);
        title = Marshal.PtrToStringAnsi(ptrTitle);
        programName = Marshal.PtrToStringAnsi(ptrProgramName);
        summary = Marshal.PtrToStringAnsi(ptrSummary);
        theme = Marshal.PtrToStringAnsi(ptrTheme);

        if (channelName == null) channelName = "";
        if (title == null) title = "";
        if (programName == null) programName = "";
        if (summary == null) summary = "";
        if (theme == null) theme = "";
        channelName = channelName.Trim();
        title = title.Trim();
        programName = programName.Trim();
        summary = summary.Trim();
        theme = theme.Trim();

        uint d1 = datestart;
        uint m = timestart & 0xff;
        uint h1 = (timestart >> 16) & 0xff;
        DateTime programStartTime = new DateTime(System.DateTime.Now.Ticks);
        DateTime dayStart = new DateTime(System.DateTime.Now.Ticks);
        dayStart = dayStart.Subtract(new TimeSpan(1, dayStart.Hour, dayStart.Minute, dayStart.Second, dayStart.Millisecond));
        int day = (int)dayStart.DayOfWeek;

        programStartTime = dayStart;
        int minVal = (int)((d1 - day) * 86400 + h1 * 3600 + m * 60);
        if (minVal < 21600)
          minVal += 604800;

        programStartTime = programStartTime.AddSeconds(minVal);
        MHWEvent mhwEvent = new MHWEvent(Network, networkid, channelid, transportid, title, summary, theme, programStartTime, programStartTime.AddMinutes(duration));
        _listMhwEvents.Add(mhwEvent);
      }
      BackgroundWorker mhwWorker = new BackgroundWorker();
      mhwWorker.DoWork += new DoWorkEventHandler(this.MhwBackgroundWorker);
      mhwWorker.RunWorkerAsync();
    }
    #endregion

    #region DVB EPG
    int getUTC(int val)
    {
      if ((val & 0xF0) >= 0xA0)
        return 0;
      if ((val & 0xF) >= 0xA)
        return 0;
      return ((val & 0xF0) >> 4) * 10 + (val & 0xF);
    }


    void ParseEPG()
    {
      _listChannels = new List<EPGChannel>();
      uint channelCount = 0;
      ushort networkid = 0;
      ushort transportid = 0;
      ushort serviceid = 0;
      _epgInterface.GetEPGChannelCount(out channelCount);
      Log.WriteFile(LogType.EPG, "epg-grab: received epg for {0} channels", channelCount);
      for (uint x = 0; x < channelCount; ++x)
      {
        _epgInterface.GetEPGChannel((uint)x, ref networkid, ref transportid, ref serviceid);
        EPGChannel channel = new EPGChannel(Network, networkid, serviceid, transportid);

        uint eventCount = 0;
        _epgInterface.GetEPGEventCount((uint)x, out eventCount);
        for (uint i = 0; i < eventCount; ++i)
        {
          uint start_time_MJD = 0, start_time_UTC = 0, duration = 0, languageId = 0, languageCount = 0;
          string title, description, genre;
          IntPtr ptrTitle = IntPtr.Zero;
          IntPtr ptrDesc = IntPtr.Zero;
          IntPtr ptrGenre = IntPtr.Zero;
          _epgInterface.GetEPGEvent((uint)x, (uint)i, out languageCount, out start_time_MJD, out start_time_UTC, out duration, out ptrGenre);
          genre = Marshal.PtrToStringAnsi(ptrGenre);

          int duration_hh = getUTC((int)((duration >> 16)) & 255);
          int duration_mm = getUTC((int)((duration >> 8)) & 255);
          int duration_ss = 0;//getUTC((int) (duration )& 255);
          int starttime_hh = getUTC((int)((start_time_UTC >> 16)) & 255);
          int starttime_mm = getUTC((int)((start_time_UTC >> 8)) & 255);
          int starttime_ss = 0;//getUTC((int) (start_time_UTC )& 255);

          if (starttime_hh > 23) starttime_hh = 23;
          if (starttime_mm > 59) starttime_mm = 59;
          if (starttime_ss > 59) starttime_ss = 59;

          if (duration_hh > 23) duration_hh = 23;
          if (duration_mm > 59) duration_mm = 59;
          if (duration_ss > 59) duration_ss = 59;

          // convert the julian date
          int year = (int)((start_time_MJD - 15078.2) / 365.25);
          int month = (int)((start_time_MJD - 14956.1 - (int)(year * 365.25)) / 30.6001);
          int day = (int)(start_time_MJD - 14956 - (int)(year * 365.25) - (int)(month * 30.6001));
          int k = (month == 14 || month == 15) ? 1 : 0;
          year += 1900 + k; // start from year 1900, so add that here
          month = month - 1 - k * 12;
          int starttime_y = year;
          int starttime_m = month;
          int starttime_d = day;

          try
          {
            DateTime dtUTC = new DateTime(starttime_y, starttime_m, starttime_d, starttime_hh, starttime_mm, starttime_ss, 0);
            DateTime dtStart = dtUTC.ToLocalTime();
            DateTime dtEnd = dtStart.AddHours(duration_hh);
            dtEnd = dtEnd.AddMinutes(duration_mm);
            dtEnd = dtEnd.AddSeconds(duration_ss);
            EPGEvent newEvent = new EPGEvent(genre, dtStart, dtEnd);
            for (int z = 0; z < languageCount; ++z)
            {
              _epgInterface.GetEPGLanguage((uint)x, (uint)i, (uint)z, out languageId, out ptrTitle, out ptrDesc);
              //Below rem'd out to support czech subtitles see further below
              //title = Marshal.PtrToStringAnsi(ptrTitle);
              //description = Marshal.PtrToStringAnsi(ptrDesc);
              string language = string.Empty;
              language += (char)((languageId >> 16) & 0xff);
              language += (char)((languageId >> 8) & 0xff);
              language += (char)((languageId) & 0xff);
              if (language.ToLower() == "cze" || language.ToLower() == "ces")
              {
                title = Iso6937ToUnicode.Convert( ptrTitle );
                description = Iso6937ToUnicode.Convert( ptrDesc );
              }
              else
              {
                title = DvbTextConverter.Convert(ptrTitle, language);
                description = DvbTextConverter.Convert(ptrDesc, language);
              }
              newEvent.Languages.Add(new EPGLanguage(language, title, description));
            }
            channel.AddEvent(newEvent);
          }
          catch (Exception)
          {
          }
        }//for (uint i = 0; i < eventCount; ++i)
        _listChannels.Add(channel);
      }//for (uint x = 0; x < channelCount; ++x)

      BackgroundWorker epgWorker = new BackgroundWorker();
      epgWorker.DoWork += new DoWorkEventHandler(this.EpgBackgroundWorker);
      epgWorker.RunWorkerAsync();

    }


    #endregion
    #endregion

    #region BackgroundWorkers
    #region DVB EPG
    void EpgBackgroundWorker(object sender, DoWorkEventArgs e)
    {
      Log.WriteFile(LogType.EPG, "epg: update database");
      try
      {
        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
        Thread.CurrentThread.Name = "EPG updater";
        Log.WriteFile(LogType.EPG, "epg: remove old programs");
        TVDatabase.RemoveOldPrograms();
        RadioDatabase.RemoveOldPrograms();
        Log.WriteFile(LogType.EPG, "epg: old programs removed");

        _epgChannels = new List<EpgChannelUpdate>();
        List<EPGChannel> events = _listChannels;
        _listChannels = null;
        Log.WriteFile(LogType.EPG, "epg-grab: updating EPG:{0}", events.Count);
        TVDatabase.SupressEvents = true;
        string languagesToGrab = string.Empty;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          languagesToGrab = xmlreader.GetValueAsString("epg-grabbing", "grabLanguages", "");
        }

        Log.WriteFile(LogType.EPG, "epg-grab: adding new programs");
        foreach (EPGChannel channel in events)
        {
          _timeoutTimer = DateTime.Now;
          if (channel.TvChannel == null && channel.RadioStation == null) continue;
          if (channel.TvChannel != null)
          {
            if (channel.TvChannel.LastDateTimeEpgGrabbed >= DateTime.Now.AddHours(-2))
            {
              if (String.Compare(channel.TvChannel.Name, _epgTvChannelName, true) != 0)
              {
                Log.WriteFile(LogType.EPG, "epg-grab: skip channel:{0} last update was:{1} {2} ",
                      channel.TvChannel.Name,
                      channel.TvChannel.LastDateTimeEpgGrabbed.ToShortDateString(),
                      channel.TvChannel.LastDateTimeEpgGrabbed.ToShortTimeString());
                continue;
              }
            }
            Log.WriteFile(LogType.EPG, "epg-grab: process:'{0}' ", channel.TvChannel.Name);
          }

          if (channel.RadioStation != null)
          {
            if (channel.RadioStation.LastDateTimeEpgGrabbed >= DateTime.Now.AddHours(-2))
            {
              continue;
            }
            Log.WriteFile(LogType.EPG, "epg-grab: process:'{0}' ", channel.RadioStation.Name);
          }

          channel.Sort();
          foreach (EPGEvent epgEvent in channel.EpgEvents)
          {
            foreach (EPGLanguage epgLang in epgEvent.Languages)
            {
              bool grabLanguage = false;
              if (languagesToGrab != "")
              {
                string[] langs = languagesToGrab.Split(new char[] { '/' });
                foreach (string lang in langs)
                {
                  if (lang == string.Empty) continue;
                  // Smirnoff: made this case-insensitive as per bug reports
                  if (string.Compare(epgLang.Language, lang, true) == 0) grabLanguage = true;
                  if (epgLang.Language == string.Empty) grabLanguage = true;
                }
              }
              else grabLanguage = true;
              if (!grabLanguage)
              {
                //Log.WriteFile(LogType.EPG,"epg-grab: disregard language:'{0}' {1} {2} {3}-{4} {5}",
                //        epgLang.Language,
                //        channel.TvChannel.Name,
                //        epgEvent.StartTime.ToLongDateString(),
                //        epgEvent.StartTime.ToLongTimeString(), epgEvent.EndTime.ToLongTimeString(), epgLang.Title);
                continue;
              }
              if (channel.TvChannel != null)
              {
                TVProgram tv = new TVProgram();
                tv.Start = Util.Utils.datetolong(epgEvent.StartTime);
                tv.End = Util.Utils.datetolong(epgEvent.EndTime);
                tv.Channel = channel.TvChannel.Name;
                tv.Genre = epgEvent.Genre;
                tv.Title = epgLang.Title;
                tv.Description = epgLang.Description;

                //string desc = epgLang.Description;
                //if (desc.Length>0) desc = desc.Replace('\r', ' ');
                //              if (desc.Length > 0) desc = desc.Replace('\n', ' ');
                //Log.WriteFile(LogType.EPG,"epg-grab: add:'{0}' {1} {2} {3}-{4} {5}",
                //          epgLang.Language,
                //          channel.TvChannel.Name,
                //          epgEvent.StartTime.ToLongDateString(),
                //          epgEvent.StartTime.ToLongTimeString(), epgEvent.EndTime.ToLongTimeString(), epgLang.Title);
                //              if (desc.Length>0) 
                //                Log.WriteFile(LogType.EPG,"epg-grab:     {0}", desc);
                TVDatabase.UpdateProgram(tv);
                OnChannelEvent(true, tv.Channel, tv.StartTime, tv.EndTime);
              }
              else if (channel.RadioStation != null)
              {
                TVProgram tv = new TVProgram();
                tv.Start = Util.Utils.datetolong(epgEvent.StartTime);
                tv.End = Util.Utils.datetolong(epgEvent.EndTime);
                tv.Channel = channel.RadioStation.Name;
                tv.Genre = epgEvent.Genre;
                tv.Title = epgLang.Title;
                tv.Description = epgLang.Description;
                RadioDatabase.UpdateProgram(tv);
                OnChannelEvent(false, tv.Channel, tv.StartTime, tv.EndTime);
              }
            }
          }
        }

        UpdateChannels();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      TVDatabase.SupressEvents = false;
      OnDone();
      Log.WriteFile(LogType.EPG, "epg-grab: done");
      _currentState = State.Done;
    }
    #endregion

    #region MHW
    void MhwBackgroundWorker(object sender, DoWorkEventArgs e)
    {
      try
      {
        Log.WriteFile(LogType.EPG, "mhw-grab: updating tv database");
        _epgChannels = new List<EpgChannelUpdate>();
        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "EPG MHW";
        TVDatabase.RemoveOldPrograms();
        List<MHWEvent> events = _listMhwEvents;
        _listMhwEvents = null;
        TVDatabase.SupressEvents = true;
        events.Sort(new MHWEventComparer());

        List<MHWEvent> channelCache = new List<MHWEvent>();
        foreach (MHWEvent mhwEvent in events)
        {
          TVChannel tvChannel = null;
          RadioStation radioStation = null;
          bool found = false;
          foreach (MHWEvent chan in channelCache)
          {
            if (chan.NetworkId == mhwEvent.NetworkId &&
                chan.TransportId == mhwEvent.NetworkId &&
                chan.ServiceId == mhwEvent.ServiceId)
            {
              tvChannel = chan.TvChannel;
              radioStation = chan.RadioStation;
              found = true;
              break;
            }
          }
          if (!found)
          {
            tvChannel = mhwEvent.TvChannel;
            radioStation = mhwEvent.RadioStation;
            channelCache.Add(mhwEvent);
          }
          if (tvChannel == null && radioStation == null) continue;
          if (tvChannel != null)
          {
            if (tvChannel.LastDateTimeEpgGrabbed >= DateTime.Now.AddHours(-2))
            {
              if (String.Compare(tvChannel.Name, _epgTvChannelName, true) != 0)
              {
                Log.WriteFile(LogType.EPG, "epg-grab: skip channel:{0} last update was:{1} {2} ",
                      tvChannel.Name,
                      tvChannel.LastDateTimeEpgGrabbed.ToShortDateString(),
                      tvChannel.LastDateTimeEpgGrabbed.ToShortTimeString());
                continue;
              }
            }
          }
          if (radioStation != null)
          {
            if (radioStation.LastDateTimeEpgGrabbed >= DateTime.Now.AddHours(-2))
            {

              Log.WriteFile(LogType.EPG, "epg-grab: skip channel:{0} last update was:{1} {2} ",
                    radioStation.Name,
                    radioStation.LastDateTimeEpgGrabbed.ToShortDateString(),
                    radioStation.LastDateTimeEpgGrabbed.ToShortTimeString());
              continue;
            }
          }


          _timeoutTimer = DateTime.Now;
          TVProgram tv = new TVProgram();
          tv.Start = Util.Utils.datetolong(mhwEvent.StartTime);
          tv.End = Util.Utils.datetolong(mhwEvent.EndTime);
          if (tvChannel != null)
            tv.Channel = tvChannel.Name;
          else
            tv.Channel = radioStation.Name;
          if (mhwEvent.Languages.Count > 0)
          {
            tv.Genre = mhwEvent.Genre;
            tv.Title = mhwEvent.Languages[0].Title;
            tv.Description = mhwEvent.Languages[0].Description;

            //Log.WriteFile(LogType.EPG,"mhw-grab: add: {0} {1} {2}-{3} {4}",
            //          tv.Channel, mhwEvent.StartTime.ToLongDateString(), mhwEvent.StartTime.ToLongTimeString(), mhwEvent.EndTime.ToLongTimeString(), mhwEvent.Languages[0].Title);

            OnChannelEvent( (tvChannel != null), tv.Channel, tv.StartTime, tv.EndTime);
            if (tvChannel!=null)
              TVDatabase.UpdateProgram(tv);
            else
              RadioDatabase.UpdateProgram(tv);
          }
        }
        UpdateChannels();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      finally
      {
        TVDatabase.SupressEvents = false;
        OnDone();
        _currentState = State.Done;
        Log.WriteFile(LogType.EPG, "mhw-grab: updating tv database done");
      }
    }
    #endregion


    #region ATSC
    void AtscBackgroundWorker(object sender, DoWorkEventArgs e)
    {
      try
      {
        Log.WriteFile(LogType.EPG, "atsc-grab: updating tv database");
        _epgChannels = new List<EpgChannelUpdate>();
        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
        Thread.CurrentThread.Name = "EPG ATSC";
        TVDatabase.RemoveOldPrograms();
        List<ATSCEvent> events = _listAtscEvents;
        _listAtscEvents = null;
        TVDatabase.SupressEvents = true;

        foreach (ATSCEvent atscEvent in events)
        {
          _timeoutTimer = DateTime.Now;
          if (atscEvent.TvChannel == null) continue;
          TVProgram tv = new TVProgram();
          tv.Start = Util.Utils.datetolong(atscEvent.StartTime);
          tv.End = Util.Utils.datetolong(atscEvent.EndTime);
          tv.Channel = atscEvent.TvChannel.Name;
          tv.Genre = atscEvent.Genre;
          if (atscEvent.Languages.Count > 0)
          {
            tv.Title = atscEvent.Languages[0].Title;
            tv.Description = atscEvent.Languages[0].Description;

            Log.WriteFile(LogType.EPG, "atsc-grab: add: {0} {1} {2}-{3} {4}",
                      atscEvent.TvChannel.Name, atscEvent.StartTime.ToLongDateString(), atscEvent.StartTime.ToLongTimeString(), atscEvent.EndTime.ToLongTimeString(), atscEvent.Languages[0].Title);
            TVDatabase.UpdateProgram(tv);
            OnChannelEvent(true, tv.Channel, tv.StartTime, tv.EndTime);
          }

        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      UpdateChannels();
      TVDatabase.SupressEvents = false;
      OnDone();
      _currentState = State.Done;
      Log.WriteFile(LogType.EPG, "atsc-grab: updating tv database done");
    }
    #endregion

    void OnChannelEvent(bool isTv, string channelName, DateTime timeStart, DateTime timeEnd)
    {
      foreach (EpgChannelUpdate ch in _epgChannels)
      {
        if (ch.ChannelName == channelName)
        {
          if (timeEnd < DateTime.Now) return;
          ch.NewEvent(timeStart);
          ch.NewEvent(timeEnd);
          return;
        }
      }

      EpgChannelUpdate newChan = new EpgChannelUpdate(isTv, channelName);
      if (timeEnd > DateTime.Now)
      {
        newChan.NewEvent(timeStart);
        newChan.NewEvent(timeEnd);
      }
      _epgChannels.Add(newChan);
    }

    void UpdateChannels()
    {
      List<EpgChannelUpdate> updates = _epgChannels;
      _epgChannels = new List<EpgChannelUpdate>();
      if (updates == null) return;
      if (updates.Count == 0) return;
      Log.WriteFile(LogType.EPG, "epg-grab: update {0} channels", updates.Count);

      List<TVChannel> listChannels = new List<TVChannel>();
      ArrayList stations = new ArrayList();
      TVDatabase.GetChannels(ref listChannels);
      if (listChannels == null) return;
      if (listChannels.Count == 0) return;
      foreach (EpgChannelUpdate ch in updates)
      {
        ch.Update(ref listChannels, ref stations);
      }
      updates.Clear();
    }

    void OnDone()
    {
      Log.WriteFile(LogType.EPG, "epg-grab: OnDone({0})", _epgTvChannelName);
      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel ch in channels)
      {
        if (String.Compare(ch.Name, _epgTvChannelName, true) == 0)
        {
          Log.WriteFile(LogType.EPG, "epg-grab: set last update for {0}", _epgTvChannelName);
          ch.LastDateTimeEpgGrabbed = DateTime.Now;
          TVDatabase.UpdateChannel(ch, ch.Sort);
          return;
        }
      }
    }
    #endregion
  }

}
