using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using DShowNET;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// EPG Grabber
  /// this class receives the EPG data for DVB from the MPSA directshow filter
  /// and will update the TV database with all new programs found
  /// </summary>
  public class EpgGrabber 
  {
    #region EPGEvent class
    class EPGLanguage
    {
      private string _title;
      private string _description;
      private string _language;
      public EPGLanguage(string language, string title, string description)
      {
        _title = title;
        _description = description;
        _language = language;
      }
      public string Language
      {
        get { return _language; }
      }
      public string Title
      {
        get { return _title; }
      }
      public string Description
      {
        get { return _description; }
      }
    }
    class EPGEvent
    {
      private string _genre;
      private DateTime _startTime;
      private DateTime _endTime;
      List<EPGLanguage> _listLanguages = new List<EPGLanguage>();
      public EPGEvent(string genre, DateTime startTime, DateTime endTime)
      {
        _genre = genre;
        _startTime = startTime;
        _endTime = endTime;
      }
      public string Genre
      {
        get { return _genre; }
      }
      public DateTime StartTime
      {
        get { return _startTime; }
      }
      public DateTime EndTime
      {
        get { return _endTime; }
      }
      public List<EPGLanguage> Languages
      {
        get { return _listLanguages; }
      }
    }

    #endregion

    #region EPGChannel class
    class EPGChannel : IComparer<EPGEvent>
    {
      private TVChannel _tvChannel = null;
      private int _networkId;
      private int _serviceId;
      private int _transportId;
      private NetworkType _networkType;
      List<EPGEvent> _listEvents = new List<EPGEvent>();

      public EPGChannel(NetworkType networkType, int networkId, int serviceId, int transportId)
      {
        _networkType = networkType;
        _networkId = networkId;
        _serviceId = serviceId;
        _transportId = transportId;
      }
      public NetworkType Network
      {
        get { return _networkType; }
      }
      public int NetworkId
      {
        get { return _networkId; }
      }
      public int ServiceId
      {
        get { return _serviceId; }
      }
      public int TransportId
      {
        get { return _transportId; }
      }
      public TVChannel TvChannel
      {
        get
        {
          if (_tvChannel == null)
          {
            string provider;
            _tvChannel = TVDatabase.GetTVChannelByStream(Network == NetworkType.ATSC, Network == NetworkType.DVBT, Network == NetworkType.DVBC, Network == NetworkType.DVBS, _networkId, _transportId, _serviceId, out provider);
            if (_tvChannel == null)
              Log.WriteFile(Log.LogType.EPG, "epg-grab: unknown channel: network id:{0} service id:{1} transport id:{2}", NetworkId, ServiceId, TransportId);
            else
              Log.WriteFile(Log.LogType.EPG, "epg-grab: channel:{0} events:{1}", _tvChannel.Name, _listEvents.Count);
          }
          return _tvChannel;
        }
      }
      public void Sort()
      {
        _listEvents.Sort(this);
      }
      public void AddEvent(EPGEvent epgEvent)
      {
        _listEvents.Add(epgEvent);
      }
      public List<EPGEvent> EpgEvents
      {
        get
        {
          return _listEvents;
        }
      }

      public int Compare(EPGEvent show1, EPGEvent show2)
      {
        if (show1.StartTime < show2.StartTime) return -1;
        if (show1.StartTime > show2.StartTime) return 1;
        return 0;
      }
    }
    #endregion

    #region MHWEvent class
    class MHWEvent : EPGEvent
    {
      private TVChannel _tvChannel = null;
      private int _networkId;
      private int _serviceId;
      private int _transportId;
      private NetworkType _networkType;

      public MHWEvent(NetworkType networkType, int networkId, int serviceId, int transportId, string title, string description, string genre, DateTime startTime, DateTime endTime)
        : base(genre, startTime, endTime)
      {
        _networkType = networkType;
        _networkId = networkId;
        _serviceId = serviceId;
        _transportId = transportId;
        Languages.Add(new EPGLanguage(String.Empty,title,description));
      }
      public NetworkType Network
      {
        get { return _networkType; }
      }
      public int NetworkId
      {
        get { return _networkId; }
      }
      public int ServiceId
      {
        get { return _serviceId; }
      }
      public int TransportId
      {
        get { return _transportId; }
      }
      public TVChannel TvChannel
      {
        get
        {
          if (_tvChannel == null)
          {
            string provider;
            _tvChannel = TVDatabase.GetTVChannelByStream(Network == NetworkType.ATSC, Network == NetworkType.DVBT, Network == NetworkType.DVBC, Network == NetworkType.DVBS, _networkId, _transportId, _serviceId, out provider);
          }
          return _tvChannel;
        }
      }
    }

    class MHWEventComparer : IComparer<MediaPortal.TV.Recording.EpgGrabber.MHWEvent>
    {
      public int Compare(MediaPortal.TV.Recording.EpgGrabber.MHWEvent show1,  MediaPortal.TV.Recording.EpgGrabber.MHWEvent show2)
      {
        if (show1.NetworkId < show2.NetworkId) return -1;
        if (show1.NetworkId > show2.NetworkId) return 1;

        if (show1.ServiceId < show2.ServiceId) return -1;
        if (show1.ServiceId > show2.ServiceId) return 1;

        if (show1.TransportId < show2.TransportId) return -1;
        if (show1.TransportId > show2.TransportId) return 1;

        if (show1.StartTime < show2.StartTime) return -1;
        if (show1.StartTime > show2.StartTime) return 1;
        return 0;
      }
    }
    #endregion

    #region ATSCEvent class
    class ATSCEvent : EPGEvent
    {
      private TVChannel _tvChannel = null;
      private int _majorChannel;
      private int _minorChannel;
      private NetworkType _networkType;

      public ATSCEvent(NetworkType networkType, int majorChannel, int minorChannel, string title, string description, string genre, DateTime startTime, DateTime endTime)
        : base(genre, startTime, endTime)
      {
        _networkType = networkType;
        _majorChannel = majorChannel;
        _minorChannel = minorChannel;
        Languages.Add(new EPGLanguage("", title, description));
      }
      public NetworkType Network
      {
        get { return _networkType; }
      }
      public int MajorChannel
      {
        get { return _majorChannel; }
      }
      public int MinorChannel
      {
        get { return _minorChannel; }
      }

      public TVChannel TvChannel
      {
        get
        {
          if (_tvChannel == null)
          {
            List<TVChannel> channels = new List<TVChannel>();
            TVDatabase.GetChannels(ref channels);
            foreach (TVChannel ch in channels)
            {
              int symbolrate = 0, innerFec = 0, modulation = 0, physicalChannel = 0;
              int minorChannel = 0, majorChannel = 0;
              int frequency = -1, ONID = -1, TSID = -1, SID = -1;
              int audioPid = -1, videoPid = -1, teletextPid = -1, pmtPid = -1, pcrPid = -1;
              string providerName;
              int audio1, audio2, audio3, ac3Pid;
              string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
              bool HasEITPresentFollow, HasEITSchedule;
              TVDatabase.GetATSCTuneRequest(ch.ID, out physicalChannel, out providerName, out frequency, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out minorChannel, out majorChannel, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
              if (MajorChannel == majorChannel && MinorChannel == minorChannel)
              {
                _tvChannel = ch;
                return _tvChannel;
              }
            }
          }
          return _tvChannel;
        }
      }
    }
    #endregion

    #region TvChannelEpg class
    class TvChannelEpg
    {
      string _channelName;
      DateTime _firstEvent;
      DateTime _lastEvent;

      public TvChannelEpg(string channelName)
      {
        _channelName = channelName;
        _firstEvent=DateTime.MinValue;
        _lastEvent=DateTime.MinValue;
      }
      public string ChannelName
      {
        get { return _channelName; }
      }
      public void NewEvent(DateTime time)
      {
        if (_firstEvent == DateTime.MinValue)
          _firstEvent = time;
        if (_lastEvent == DateTime.MinValue)
          _lastEvent = time;
        if (time < _firstEvent)
          _firstEvent = time;
        if (time > _lastEvent)
          _lastEvent = time;
      }

      public void Update()
      {
        if (_firstEvent==DateTime.MinValue) return;
        if (_lastEvent==DateTime.MinValue) return;

        TimeSpan ts = _lastEvent - _firstEvent;
        int hours = (int)(ts.TotalHours - 2f);
        if (hours < 0) return;
        List<TVChannel> listChannels = new List<TVChannel>();
        TVDatabase.GetChannels(ref listChannels);
        foreach (TVChannel ch in listChannels)
        {
          if (ch.Name == _channelName)
          {
            Log.WriteFile(Log.LogType.EPG, "epg: channel:{0} received epg for : {1} days, {2} hours, {3} minutes", _channelName, ts.Days, ts.Hours, ts.Minutes);
            ch.EpgHours = hours;
            TVDatabase.UpdateChannel(ch,ch.Sort);
            return;
          }
        }
      }
    }
    #endregion

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
    List<TvChannelEpg> _epgChannels;
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

    public void GrabEPG(bool epg)
    {
      _timeoutTimer = DateTime.Now;
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        bool enabled = xmlreader.GetValueAsBool("xmltv", "epgdvb", true);
        if (!enabled) return;
      }

      _grabEPG = epg;
      if (Network == NetworkType.ATSC)
      {
        if (_atscInterface != null)
        {
          _currentState = State.Grabbing;
          Log.WriteFile(Log.LogType.EPG, "epg-grab: start ATSC grabber");
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
            Log.WriteFile(Log.LogType.EPG, "epg-grab: start EPG grabber");
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
          Log.WriteFile(Log.LogType.EPG, "epg-grab: start MHW grabber");
          if (_mhwInterface != null)
          {
            _mhwInterface.GrabMHW();
            _timeoutTimer = DateTime.Now;
          }
        }
      }

    }
    public void Process()
    {
      if (_currentState != State.Idle)
      {
        TimeSpan ts = DateTime.Now - _timeoutTimer;
        if (ts.TotalMinutes >= 1)
        {
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
              _timeoutTimer = DateTime.Now;
              _currentState = State.Parsing;
            }
          }
          else if (_mhwInterface != null && !_grabEPG)
          {
            _mhwInterface.IsMHWReady(out ready);
            if (ready)
            {
              _timeoutTimer = DateTime.Now;
              _currentState = State.Parsing;
            }
          }
          break;

        case State.Parsing:
          if (Network == NetworkType.ATSC)
          {
            ParseATSC();
          }
          else if (_grabEPG)
          {
            ParseEPG();
          }
          else
          {
            ParseMHW();
          }

          _currentState = State.Updating;
          break;
      }
    }
    public void Reset()
    {
      _currentState = State.Idle;
    }
    public bool Done
    {
      get { return _currentState == State.Done; }
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
          uint start_time_MJD = 0, start_time_UTC = 0, duration = 0, languageId = 0, languageCount=0;
          string title, description, genre;
          IntPtr ptrTitle = IntPtr.Zero;
          IntPtr ptrDesc = IntPtr.Zero;
          IntPtr ptrGenre = IntPtr.Zero;
          _epgInterface.GetEPGEvent((uint)x, (uint)i, out languageCount, out start_time_MJD, out start_time_UTC, out duration,  out ptrGenre);
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
      _epgChannels = new List<TvChannelEpg>();
      System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;
      TVDatabase.RemoveOldPrograms();
      List<EPGChannel> events = _listChannels;
      _listChannels = null;
      Log.WriteFile(Log.LogType.EPG, "epg-grab: updating tv database:{0}", events.Count);
      TVDatabase.SupressEvents = true;
      string languagesToGrab = String.Empty;
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        languagesToGrab = xmlreader.GetValueAsString("epg-grabbing", "grabLanguages", "");
      }

      
      foreach (EPGChannel channel in events)
      {
        _timeoutTimer = DateTime.Now;
        if (channel.TvChannel == null) continue;
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

            Log.WriteFile(Log.LogType.EPG, "epg-grab: add:'{0}' {1} {2} {3}-{4} {5}",
                      epgLang.Language,
                      channel.TvChannel.Name,
                      epgEvent.StartTime.ToLongDateString(),
                      epgEvent.StartTime.ToLongTimeString(), epgEvent.EndTime.ToLongTimeString(), epgLang.Title);
            TVDatabase.UpdateProgram(tv);
            OnChannelEvent(tv.Channel, tv.StartTime, tv.EndTime);
          }
        }
      }
      Log.WriteFile(Log.LogType.EPG, "epg-grab: done");

      UpdateChannels();
      TVDatabase.SupressEvents = false;
      _currentState = State.Done;
    }
    #endregion

    #region MHW
    void MhwBackgroundWorker(object sender, DoWorkEventArgs e)
    {
      _epgChannels = new List<TvChannelEpg>();
      System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;
      TVDatabase.RemoveOldPrograms();
      List<MHWEvent> events = _listMhwEvents;
      _listMhwEvents = null;
      Log.WriteFile(Log.LogType.EPG, "mhw-grab: updating tv database");
      TVDatabase.SupressEvents = true;
      events.Sort( new MHWEventComparer());

      List<MHWEvent> channelCache = new List<MHWEvent>();
      foreach (MHWEvent mhwEvent in events)
      { 
        TVChannel tvChannel=null;
        bool found=false;
        foreach (MHWEvent chan in channelCache)
        {
          if (chan.NetworkId==mhwEvent.NetworkId &&
              chan.TransportId==mhwEvent.NetworkId &&
              chan.ServiceId==mhwEvent.ServiceId)
          {
            tvChannel=chan.TvChannel;
            found=true;
            break;
          }
        }
        if (!found)
        {
          tvChannel=mhwEvent.TvChannel;
          channelCache.Add( mhwEvent);
        }
        if (tvChannel==null) continue;

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
      TVDatabase.SupressEvents = false;
      _currentState = State.Done;

    }
    #endregion


    #region ATSC
    void AtscBackgroundWorker(object sender, DoWorkEventArgs e)
    {
      _epgChannels = new List<TvChannelEpg>();
      System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;
      TVDatabase.RemoveOldPrograms();
      List<ATSCEvent> events = _listAtscEvents;
      _listAtscEvents = null;
      Log.WriteFile(Log.LogType.EPG, "atsc-grab: updating tv database");
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
      UpdateChannels();
      TVDatabase.SupressEvents = false;
      _currentState = State.Done;
    }
    #endregion

    void OnChannelEvent(string channelName, DateTime timeStart, DateTime timeEnd)
    {
      foreach (TvChannelEpg ch in _epgChannels)
      {
        if (ch.ChannelName == channelName)
        {
          ch.NewEvent(timeStart);
          ch.NewEvent(timeEnd);
          return;
        }
      }
      TvChannelEpg newChan = new TvChannelEpg(channelName);
      newChan.NewEvent(timeStart);
      newChan.NewEvent(timeEnd);
      _epgChannels.Add(newChan);
    }

    void UpdateChannels()
    {
      foreach (TvChannelEpg ch in _epgChannels)
      {
        ch.Update();
      }
      _epgChannels.Clear();
    }

    #endregion
  }

}
