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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using DShowNET.MPSA;
using DShowNET.MPTSWriter;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

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
    string _epgTvChannelName = String.Empty;
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

    #region public methods

    public void GrabEPG(string tvChannelName, bool epg)
    {
      _epgTvChannelName = tvChannelName;
      _timeoutTimer = DateTime.Now;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        bool enabled = xmlreader.GetValueAsBool("xmltv", "epgdvb", true);
        if (!enabled)
        {
          Log.WriteFile(Log.LogType.EPG, "epg-grab: EPG grabber disabled");
          return;
        }
      }

      _grabEPG = epg;
      if (Network == NetworkType.ATSC)
      {
        if (_atscInterface != null)
        {
          _currentState = State.Grabbing;
          Log.WriteFile(Log.LogType.EPG, "epg-grab: start ATSC grabber:{0}", _epgTvChannelName);
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
            Log.WriteFile(Log.LogType.EPG, "epg-grab: start EPG grabber:{0}", _epgTvChannelName);
            _epgInterface.GrabEPG();
            _timeoutTimer = DateTime.Now;
          }
          else
          {
            Log.WriteFile(Log.LogType.EPG, "epg-grab: start EPG grabber:epginterface=null");
          }
        }
        else
        {
          _currentState = State.Grabbing;
          if (_mhwInterface != null)
          {
            Log.WriteFile(Log.LogType.EPG, "epg-grab: start MHW grabber:{0}", _epgTvChannelName);
            _mhwInterface.GrabMHW();
            _timeoutTimer = DateTime.Now;
          }
          else
          {
            Log.WriteFile(Log.LogType.EPG, "epg-grab: start MHW grabber:MHWinterface=null");
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
            Log.Write("epg-grab: timeout...");
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
                  Log.Write("epg-grab: ATSC EPG ready...");
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
                Log.Write("epg-grab: EPG ready...");
                _timeoutTimer = DateTime.Now;
                _currentState = State.Parsing;
              }
            }
            else if (_mhwInterface != null && !_grabEPG)
            {
              _mhwInterface.IsMHWReady(out ready);
              if (ready)
              {
                Log.Write("epg-grab: MHW ready...");
                _timeoutTimer = DateTime.Now;
                if (_epgInterface != null)
                {
                  short titleCount;
                  _mhwInterface.GetMHWTitleCount(out titleCount);
                  if (titleCount <= 0)
                  {
                    Log.Write("epg-grab: no MHW events, try DVB EPG:{0}", _epgTvChannelName);
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
              Log.Write("epg-grab: ATSC parsing...");
              ParseATSC();
            }
            else if (_grabEPG)
            {
              Log.Write("epg-grab: EPG parsing...");
              ParseEPG();
            }
            else
            {
              Log.Write("epg-grab: MHW parsing...");
              ParseMHW();
            }

            _currentState = State.Updating;
            break;
        }
      }
      catch (Exception ex)
      {
        Log.Write("epg-grab: exception :{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
    }

    public void Reset()
    {
      Log.WriteFile(Log.LogType.EPG, "epg-grab: reset");
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
      Log.WriteFile(Log.LogType.EPG, "atsc-epg: received {0} titles", titleCount);
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
          title = String.Empty;

        if (description == null)
          description = String.Empty;
        if (title == String.Empty || title == "n.a.")
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
      Log.WriteFile(Log.LogType.EPG, "mhw-grab: received {0} programs", titleCount);
      for (int i = 0; i < titleCount; ++i)
      {
        short id = 0, transportid = 0, networkid = 0, channelnr = 0, channelid = 0, programid = 0, themeid = 0, PPV = 0, duration = 0;
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
      Log.WriteFile(Log.LogType.EPG, "epg-grab: received epg for {0} channels", channelCount);
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
              title = Marshal.PtrToStringAnsi(ptrTitle);
              description = Marshal.PtrToStringAnsi(ptrDesc);
              string language = String.Empty;
              language += (char)((languageId >> 16) & 0xff);
              language += (char)((languageId >> 8) & 0xff);
              language += (char)((languageId) & 0xff);
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
      Log.WriteFile(Log.LogType.EPG, "epg: update database");
      try
      {
        System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;
        Log.WriteFile(Log.LogType.EPG, "epg: remove old programs");
        TVDatabase.RemoveOldPrograms();
        Log.WriteFile(Log.LogType.EPG, "epg: old programs removed");

        _epgChannels = new List<EpgChannelUpdate>();
        List<EPGChannel> events = _listChannels;
        _listChannels = null;
        Log.WriteFile(Log.LogType.EPG, "epg-grab: updating tv database:{0}", events.Count);
        TVDatabase.SupressEvents = true;
        string languagesToGrab = String.Empty;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          languagesToGrab = xmlreader.GetValueAsString("epg-grabbing", "grabLanguages", "");
        }

        Log.WriteFile(Log.LogType.EPG, "epg-grab: adding new programs");
        foreach (EPGChannel channel in events)
        {
          _timeoutTimer = DateTime.Now;
          if (channel.TvChannel == null) continue;
          if (channel.TvChannel.LastDateTimeEpgGrabbed >= DateTime.Now.AddHours(-2))
          {
            if (String.Compare(channel.TvChannel.Name, _epgTvChannelName, true) != 0)
            {
              Log.WriteFile(Log.LogType.EPG, "epg-grab: skip channel:{0} last update was:{1} {2} ",
                    channel.TvChannel.Name,
                    channel.TvChannel.LastDateTimeEpgGrabbed.ToShortDateString(),
                    channel.TvChannel.LastDateTimeEpgGrabbed.ToShortTimeString());
              continue;
            }
          }
          Log.WriteFile(Log.LogType.EPG, "epg-grab: process:'{0}' ", channel.TvChannel.Name);
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
                  if (lang == String.Empty) continue;
                  // Smirnoff: made this case-insensitive as per bug reports
                  if (string.Compare(epgLang.Language, lang, true) == 0) grabLanguage = true;
                  if (epgLang.Language == String.Empty) grabLanguage = true;
                }
              }
              else grabLanguage = true;
              if (!grabLanguage)
              {
                Log.WriteFile(Log.LogType.EPG, "epg-grab: disregard language:'{0}' {1} {2} {3}-{4} {5}",
                        epgLang.Language,
                        channel.TvChannel.Name,
                        epgEvent.StartTime.ToLongDateString(),
                        epgEvent.StartTime.ToLongTimeString(), epgEvent.EndTime.ToLongTimeString(), epgLang.Title);
                continue;
              }
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
              //Log.WriteFile(Log.LogType.EPG, "epg-grab: add:'{0}' {1} {2} {3}-{4} {5}",
              //          epgLang.Language,
              //          channel.TvChannel.Name,
              //          epgEvent.StartTime.ToLongDateString(),
              //          epgEvent.StartTime.ToLongTimeString(), epgEvent.EndTime.ToLongTimeString(), epgLang.Title);
              //              if (desc.Length>0) 
              //                Log.WriteFile(Log.LogType.EPG, "epg-grab:     {0}", desc);
              TVDatabase.UpdateProgram(tv);
              OnChannelEvent(tv.Channel, tv.StartTime, tv.EndTime);
            }
          }
        }

        UpdateChannels();
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.EPG, "epg-grab: exception:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      TVDatabase.SupressEvents = false;
      OnDone();
      Log.WriteFile(Log.LogType.EPG, "epg-grab: done");
      _currentState = State.Done;
    }
    #endregion

    #region MHW
    void MhwBackgroundWorker(object sender, DoWorkEventArgs e)
    {
      try
      {
        Log.WriteFile(Log.LogType.EPG, "mhw-grab: updating tv database");
        _epgChannels = new List<EpgChannelUpdate>();
        System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;
        TVDatabase.RemoveOldPrograms();
        List<MHWEvent> events = _listMhwEvents;
        _listMhwEvents = null;
        TVDatabase.SupressEvents = true;
        events.Sort(new MHWEventComparer());

        List<MHWEvent> channelCache = new List<MHWEvent>();
        foreach (MHWEvent mhwEvent in events)
        {
          TVChannel tvChannel = null;
          bool found = false;
          foreach (MHWEvent chan in channelCache)
          {
            if (chan.NetworkId == mhwEvent.NetworkId &&
                chan.TransportId == mhwEvent.NetworkId &&
                chan.ServiceId == mhwEvent.ServiceId)
            {
              tvChannel = chan.TvChannel;
              found = true;
              break;
            }
          }
          if (!found)
          {
            tvChannel = mhwEvent.TvChannel;
            channelCache.Add(mhwEvent);
          }
          if (tvChannel == null) continue;
          if (tvChannel.LastDateTimeEpgGrabbed >= DateTime.Now.AddHours(-2))
          {
            if (String.Compare(tvChannel.Name, _epgTvChannelName, true) != 0)
            {
              Log.WriteFile(Log.LogType.EPG, "epg-grab: skip channel:{0} last update was:{1} {2} ",
                    tvChannel.Name,
                    tvChannel.LastDateTimeEpgGrabbed.ToShortDateString(),
                    tvChannel.LastDateTimeEpgGrabbed.ToShortTimeString());
              continue;
            }
          }


          _timeoutTimer = DateTime.Now;
          TVProgram tv = new TVProgram();
          tv.Start = Util.Utils.datetolong(mhwEvent.StartTime);
          tv.End = Util.Utils.datetolong(mhwEvent.EndTime);
          tv.Channel = tvChannel.Name;
          if (mhwEvent.Languages.Count > 0)
          {
            tv.Genre = mhwEvent.Genre;
            tv.Title = mhwEvent.Languages[0].Title;
            tv.Description = mhwEvent.Languages[0].Description;

            Log.WriteFile(Log.LogType.EPG, "mhw-grab: add: {0} {1} {2}-{3} {4}",
                      tvChannel.Name, mhwEvent.StartTime.ToLongDateString(), mhwEvent.StartTime.ToLongTimeString(), mhwEvent.EndTime.ToLongTimeString(), mhwEvent.Languages[0].Title);

            OnChannelEvent(tv.Channel, tv.StartTime, tv.EndTime);
            TVDatabase.UpdateProgram(tv);
          }
        }
        UpdateChannels();
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.EPG, true, "mhw-grab: exception:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      finally
      {
        TVDatabase.SupressEvents = false;
        OnDone();
        _currentState = State.Done;
        Log.WriteFile(Log.LogType.EPG, "mhw-grab: updating tv database done");
      }
    }
    #endregion


    #region ATSC
    void AtscBackgroundWorker(object sender, DoWorkEventArgs e)
    {
      try
      {
        Log.WriteFile(Log.LogType.EPG, "atsc-grab: updating tv database");
        _epgChannels = new List<EpgChannelUpdate>();
        System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;
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

            Log.WriteFile(Log.LogType.EPG, "atsc-grab: add: {0} {1} {2}-{3} {4}",
                      atscEvent.TvChannel.Name, atscEvent.StartTime.ToLongDateString(), atscEvent.StartTime.ToLongTimeString(), atscEvent.EndTime.ToLongTimeString(), atscEvent.Languages[0].Title);
            TVDatabase.UpdateProgram(tv);
            OnChannelEvent(tv.Channel, tv.StartTime, tv.EndTime);
          }

        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.EPG, "epg-grab: exception:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      UpdateChannels();
      TVDatabase.SupressEvents = false;
      OnDone();
      _currentState = State.Done;
      Log.WriteFile(Log.LogType.EPG, "atsc-grab: updating tv database done");
    }
    #endregion

    void OnChannelEvent(string channelName, DateTime timeStart, DateTime timeEnd)
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

      EpgChannelUpdate newChan = new EpgChannelUpdate(channelName);
      if (timeEnd > DateTime.Now)
      {
        newChan.NewEvent(timeStart);
        newChan.NewEvent(timeEnd);
      }
      _epgChannels.Add(newChan);
    }

    void UpdateChannels()
    {
      if (_epgChannels == null) return;
      if (_epgChannels.Count == 0) return;
      Log.WriteFile(Log.LogType.EPG, "epg-grab: update {0} channels",_epgChannels.Count);

      List<TVChannel> listChannels = new List<TVChannel>();
      TVDatabase.GetChannels(ref listChannels);
      foreach (EpgChannelUpdate ch in _epgChannels)
      {
        ch.Update(ref listChannels);
      }
      _epgChannels.Clear();
    }

    void OnDone()
    {
      Log.WriteFile(Log.LogType.EPG, "epg-grab: OnDone({0})", _epgTvChannelName);
      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel ch in channels)
      { 
        if (String.Compare(ch.Name, _epgTvChannelName, true) == 0)
        {
          Log.WriteFile(Log.LogType.EPG, "epg-grab: set last update for {0}", _epgTvChannelName);
          ch.LastDateTimeEpgGrabbed = DateTime.Now;
          TVDatabase.UpdateChannel(ch, ch.Sort); 
          return;
        }
      }
    }
    #endregion
  }

}
