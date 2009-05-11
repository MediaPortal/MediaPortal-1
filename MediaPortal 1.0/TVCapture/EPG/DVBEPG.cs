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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Zusammenfassung für DVBEPG.
  /// </summary>
  public class DVBEPG
  {
    public class EITComparer : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        DVBSections.EITDescr desc1 = (DVBSections.EITDescr)x;
        DVBSections.EITDescr desc2 = (DVBSections.EITDescr)y;
        //first sort on channel
        long lNumber1 = desc1.program_number * 100000 + desc1.ts_id;
        long lNumber2 = desc2.program_number * 100000 + desc2.ts_id;
        if (lNumber1 < lNumber2) return -1;
        if (lNumber1 > lNumber2) return 1;
        //next on start time
        DateTime date1 = new DateTime(desc1.starttime_y, desc1.starttime_m, desc1.starttime_d, desc1.starttime_hh, desc1.starttime_mm, desc1.starttime_ss);
        DateTime date2 = new DateTime(desc2.starttime_y, desc2.starttime_m, desc2.starttime_d, desc2.starttime_hh, desc2.starttime_mm, desc2.starttime_ss);
        if (date1 < date2) return -1;
        if (date1 > date2) return 1;
        return 0;
      }

      #endregion

    }

    public DVBEPG()
    {

      m_cardType = (int)EPGCard.Invalid;
    }

    public DVBEPG(int card)
    {

      m_cardType = card;
      m_networkType = NetworkType.DVBS;

    }
    public DVBEPG(int card, NetworkType networkType)
    {
      //
      // TODO: Fügen Sie hier die Konstruktorlogik hinzu
      //
      m_cardType = card;
      m_networkType = networkType;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        m_languagesToGrab = xmlreader.GetValueAsString("epg-grabbing", "grabLanguages", "");
      }
    }
    //public event SendCounter GiveFeedback;
    DVBSections m_sections = new DVBSections();
    int m_cardType = 0;
    string m_channelName = "";
    string m_languagesToGrab = "";
    ArrayList m_titleBuffer = new ArrayList();
    ArrayList m_namesBuffer = new ArrayList();
    ArrayList m_themeBuffer = new ArrayList();
    ArrayList m_summaryBuffer = new ArrayList();
    NetworkType m_networkType;
    ArrayList m_streamBuffer = new ArrayList();
    int m_addsToDatabase = 0;
    int m_mhwChannelsCount = 0;
    bool m_titlesParsing = false;
    bool m_summaryParsing = false;
    bool m_channelsParsing = false;
    int m_channelGrabLen = 0;
    MediaPortal.UserInterface.Controls.MPLabel m_mhwChannels;
    MediaPortal.UserInterface.Controls.MPLabel m_mhwTitles;
    bool m_isLocked = false;

    // mhw
    public struct Programm
    {
      public int ID;
      public int ChannelID;
      public int ThemeID;
      public int PPV;
      public DateTime Time;
      public bool Summaries;
      public int Duration;
      public string Title;
      public int ProgrammID;
      public string ProgrammName;
      public int TransportStreamID;
      public int NetworkID;
    };
    //
    public struct MHWTheme
    {
      public int ThemeIndex;
      public string ThemeText;
    }
    public struct MHWChannel
    {
      public int NetworkID;
      public int TransponderID;
      public int ChannelID;
      public string ChannelName;
    };
    public struct Summary
    {
      public int ProgramID;// its the programm-id of epg, not an channel id
      public string Description;
    }
    // mhw end
    public enum EPGCard
    {
      Invalid = 0,
      TechnisatStarCards,
      TTPremiumCards,
      BDACards,
      Unknown,
      ChannelName
    }
    //
    //
    public string Languages
    {
      get
      {
        return m_languagesToGrab;
      }
      set
      {
        m_languagesToGrab = value;
      }
    }
    //
    // commits epg-data to database
    //
    public bool Locked
    {
      get { return m_isLocked; }
    }
    public MediaPortal.UserInterface.Controls.MPLabel LabelChannels
    {
      set { m_mhwChannels = value; }
    }
    public MediaPortal.UserInterface.Controls.MPLabel LabelTitles
    {
      set { m_mhwTitles = value; }
    }
    public bool ChannelsReady
    {
      get { return m_mhwChannelsCount > 0 ? true : false; }
    }
    public int ChannelsGrabLen
    {
      get { return m_channelGrabLen; }
      set { m_channelGrabLen = value; }
    }
    public bool SummaryParsing
    {
      get { return m_summaryParsing; }
      set { m_summaryParsing = value; }
    }
    public bool ChannelsParsing
    {
      get { return m_channelsParsing; }
      set
      {

        m_channelsParsing = value;
        if (m_namesBuffer.Count > 0)
          m_channelsParsing = false;
      }
    }
    public bool TitlesParsing
    {
      get { return m_titlesParsing; }
    }
    public int GetAdditionsToDB
    {
      get { return m_addsToDatabase; }
    }

    public void GetMHWBuffer(ref ArrayList channels, ref ArrayList titles, ref ArrayList themes, ref ArrayList summaries)
    {
      lock (m_namesBuffer.SyncRoot)
      {
        channels = (ArrayList)m_namesBuffer.Clone();
      }
      lock (m_titleBuffer.SyncRoot)
      {
        titles = (ArrayList)m_titleBuffer.Clone();
      }
      lock (m_themeBuffer.SyncRoot)
      {
        themes = (ArrayList)m_themeBuffer.Clone();
      }
      lock (m_summaryBuffer.SyncRoot)
      {
        summaries = (ArrayList)m_summaryBuffer.Clone();
      }
    }
    public void SetMHWBuffer(ArrayList channels, ArrayList titles, ArrayList themes, ArrayList summaries)
    {
      lock (m_namesBuffer.SyncRoot)
      {
        lock (channels.SyncRoot)
        {
          m_namesBuffer = (ArrayList)channels.Clone();
        }
      }
      lock (m_titleBuffer.SyncRoot)
      {
        lock (titles.SyncRoot)
        {
          m_titleBuffer = (ArrayList)titles.Clone();
        }
      }
      lock (m_themeBuffer.SyncRoot)
      {
        lock (themes.SyncRoot)
        {
          m_themeBuffer = (ArrayList)themes.Clone();
        }
      }
      lock (m_summaryBuffer.SyncRoot)
      {
        lock (summaries.SyncRoot)
        {
          m_summaryBuffer = (ArrayList)summaries.Clone();
        }
      }
    }
    //
    //
    //
    public int ChannelCount
    {
      get { return m_namesBuffer.Count; }
    }

    public int ClearBuffer()
    {
      m_streamBuffer = new ArrayList();
      m_namesBuffer = new ArrayList();
      m_titleBuffer = new ArrayList();
      m_summaryBuffer = new ArrayList();
      m_themeBuffer = new ArrayList();
      m_mhwChannelsCount = 0;
      m_titlesParsing = false;
      m_summaryParsing = false;
      m_channelsParsing = false;
      m_isLocked = false;
      return 0;
    }
    public int GetEPG(out DateTime reGrabTime)
    {

      int count = SubmittMHW(out reGrabTime);
      ClearBuffer();
      return count;
    }
    public int SetEITToDatabase(DVBSections.EITDescr data, string channelName, int eventKind, out DateTime dateProgramEnd)
    {
      try
      {
        dateProgramEnd = DateTime.MinValue;
        int retVal = 0;
        //
        //
        if (data.extendedEventUseable == false && data.shortEventUseable == false)
        {
          //Log.Info("epg-grabbing: event IGNORED by language selection");
          return 0;
        }

        //
        TVProgram tv = new TVProgram();

        System.DateTime datestart;
        try
        {
          datestart = new DateTime(data.starttime_y, data.starttime_m, data.starttime_d, data.starttime_hh, data.starttime_mm, data.starttime_ss);
        }
        catch
        {
          return 0;
        }
        DateTime dateend = datestart.AddSeconds(data.duration_ss);
        dateend = dateend.AddMinutes(data.duration_mm);
        dateend = dateend.AddHours(data.duration_hh);
        tv.Start = Util.Utils.datetolong(datestart);
        tv.End = Util.Utils.datetolong(dateend);
        dateProgramEnd = dateend;

        tv.Channel = channelName;
        tv.Genre = data.genere_text;

        tv.Title = data.event_item;
        tv.Description = data.event_item_text;
        //
        if (tv.Title == null)
          tv.Title = string.Empty;

        if (tv.Description == null)
          tv.Description = string.Empty;

        if (tv.Description == string.Empty)
          tv.Description = data.event_text;

        if (tv.Title == string.Empty)
          tv.Title = data.event_name;

        //
        if (tv.Title == string.Empty || tv.Title == "n.a.")
        {
          //Log.Info("epg: entrie without title found");
          dateProgramEnd = DateTime.MinValue;
          return 0;
        }

        //
        // for check
        //
        if (channelName == string.Empty)
        {
          //Log.Info("epg-grab: FAILED no channel-name: {0} : {1}",tv.Start,tv.End);
          dateProgramEnd = DateTime.MinValue;
          return 0;
        }


        Log.WriteFile(LogType.EPG, "epg-grab: {0} {1}-{2} {3}", tv.Channel, tv.Start, tv.End, tv.Title);
        ArrayList programsInDatabase = new ArrayList();
        TVDatabase.GetProgramsPerChannel(tv.Channel, tv.Start + 1, tv.End - 1, ref programsInDatabase);
        if (programsInDatabase.Count == 0)
        {
          int programID = TVDatabase.AddProgram(tv);
          if (programID != -1)
          {
            retVal = 1;
          }
        }
        else
        {
          retVal = 0;
        }
        return retVal;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        dateProgramEnd = DateTime.MinValue;
        return 0;
      }
    }
    public string ChannelName
    {
      get { return m_channelName; }
      set { m_channelName = value; }
    }
    //
    // returns long-value from sep. date
    //
    private long GetLongFromDate(int year, int mon, int day, int hour, int min, int sec)
    {

      string longStringA = String.Format("{0:0000}{1:00}{2:00}", year, mon, day);
      string longStringB = String.Format("{0:00}{1:00}{2:00}", hour, min, sec);
      //Log.Info("epg-grab: string-value={0}",longStringA+longStringB);
      return (long)Convert.ToUInt64(longStringA + longStringB);
    }
    //
    //
    //

    //
    public int GetEPG(ArrayList epgData, int serviceID, out DateTime reGrabTime)
    {
      reGrabTime = DateTime.MinValue;
      if (m_cardType == (int)EPGCard.Invalid || m_cardType == (int)EPGCard.Unknown)
        return 0;

      int eventsCount = 0;
      ArrayList eitList = new ArrayList();
      ArrayList tableList = new ArrayList();
      DVBSections tmpSections = new DVBSections();
      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);

      DateTime[] reGrabTimes = new DateTime[channels.Count];
      for (int i = 0; i < reGrabTimes.Length; ++i)
      {
        reGrabTimes[i] = new DateTime();
        reGrabTimes[i] = DateTime.MinValue;
      }
      eitList = tmpSections.GetEITSchedule(epgData);

      TVDatabase.RemoveOldPrograms();

      int n = 0;
      if (eitList == null)
        return 0;

      eitList.Sort(new EITComparer());
      Hashtable tableChannels = new Hashtable();

      //find all channel names
      foreach (DVBSections.EITDescr eit in eitList)
      {
        string progName = "";

        switch (m_cardType)
        {
          case (int)EPGCard.TechnisatStarCards:
            progName = TVDatabase.GetSatChannelName(eit.program_number, eit.org_network_id);
            break;

          case (int)EPGCard.BDACards:
            {
              int freq, symbolrate, innerFec, modulation, ONID, TSID, SID;
              int audioPid, videoPid, teletextPid, pmtPid, bandWidth;
              int audio1, audio2, audio3, ac3Pid, pcrPid;
              string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
              bool HasEITPresentFollow, HasEITSchedule;
              string provider = "";
              foreach (TVChannel chan in channels)
              {
                switch (m_networkType)
                {
                  case NetworkType.DVBC:
                    TVDatabase.GetDVBCTuneRequest(chan.ID, out provider, out freq, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
                    if (eit.program_number == SID && eit.ts_id == TSID)
                    {
                      progName = chan.Name;
                    }
                    break;
                  case NetworkType.DVBS:
                    progName = TVDatabase.GetSatChannelName(eit.program_number, eit.ts_id);
                    break;
                  case NetworkType.DVBT:
                    TVDatabase.GetDVBTTuneRequest(chan.ID, out provider, out freq, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
                    if (eit.program_number == SID && eit.ts_id == TSID)
                    {
                      progName = chan.Name;
                    }
                    break;
                }
                if (progName != string.Empty) break;
              }//foreach (TVChannel chan in channels)
            }
            break;

          case (int)EPGCard.ChannelName:
            progName = m_channelName;
            break;
        }
        if (progName != null)
        {
          tableChannels[(eit.program_number * 100000 + eit.ts_id)] = progName;
        }
      }

      foreach (DVBSections.EITDescr eit in eitList)
      {
        int id = (eit.program_number * 100000 + eit.ts_id);
        if (!tableChannels.ContainsKey(id)) continue;
        string progName = (string)tableChannels[id];
        DVBSections.EITDescr eit2DB = new MediaPortal.TV.Recording.DVBSections.EITDescr();
        eit2DB = eit;
        if (m_languagesToGrab != "")
        {
          eit2DB.extendedEventUseable = false;
          eit2DB.shortEventUseable = false;
        }
        else
        {
          eit2DB.extendedEventUseable = true;
          eit2DB.shortEventUseable = true;
        }

        if (m_languagesToGrab != "")
        {
          string[] langs = m_languagesToGrab.Split(new char[] { '/' });
          foreach (string lang in langs)
          {
            if (lang == "")
              continue;
            //Log.Info("epg-grabbing: language selected={0}",lang);
            string codeEE = "";
            string codeSE = "";

            string eitItem = eit.event_item_text;
            if (eitItem == null)
              eitItem = "";

            if (eit.eeLanguageCode != null)
            {
              //Log.Info("epg-grabbing: e-event-lang={0}",eit.eeLanguageCode);
              codeEE = eit.eeLanguageCode.ToLower();
              if (codeEE.Length == 3)
              {
                if (lang.ToLower().Equals(codeEE))
                {
                  eit2DB.extendedEventUseable = true;
                  break;
                }
              }
            }

            if (eit.seLanguageCode != null)
            {
              //Log.Info("epg-grabbing: s-event-lang={0}",eit.seLanguageCode);
              codeSE = eit.seLanguageCode.ToLower();
              if (codeSE.Length == 3)
              {
                if (lang.ToLower().Equals(codeSE))
                {
                  eit2DB.shortEventUseable = true;
                  break;
                }

              }

            }


          }
        }

        DateTime dateProgramEnd = DateTime.MinValue;
        if (serviceID != 0)
        {
          if (eit.program_number == serviceID)
            eventsCount += SetEITToDatabase(eit2DB, progName, 0x50, out dateProgramEnd);
        }
        else
          eventsCount += SetEITToDatabase(eit2DB, progName, 0x50, out dateProgramEnd);

        if (dateProgramEnd != DateTime.MinValue)
        {
          for (int i = 0; i < channels.Count; ++i)
          {
            TVChannel chan = channels[i];
            if (chan.Name.Equals(progName))
            {
              if (dateProgramEnd > reGrabTimes[i])
              {
                reGrabTimes[i] = dateProgramEnd;
              }
              break;
            }
          }
        }
        n++;
      }

      GC.Collect();
      for (int i = 0; i < reGrabTimes.Length; ++i)
      {
        TVChannel ch = channels[i];
        if (reGrabTimes[i] != DateTime.MinValue)
        {
          if (reGrabTimes[i] < reGrabTime || reGrabTime == DateTime.MinValue)
            reGrabTime = reGrabTimes[i];
        }
      }
      return eventsCount;

    }//public int GetEPG(DirectShowLib.IBaseFilter filter,int serviceID)

    //
    //
    public void ParseChannels(byte[] data)
    {

      if (m_namesBuffer == null)
        return; // error
      if (m_namesBuffer.Count > 0)
        return; // already got channles table

      int dataLen = data.Length;
      Log.WriteFile(LogType.EPG, "mhw-epg: start parse channels for mhw", m_namesBuffer.Count);
      lock (m_namesBuffer.SyncRoot)
      {
        for (int n = 4; n < dataLen; n += 22)
        {
          if (m_namesBuffer.Count >= ((dataLen - 3) / 22))
            break;
          MHWChannel ch = new MHWChannel();
          ch.NetworkID = (data[n] << 8) + data[n + 1];
          ch.TransponderID = (data[n + 2] << 8) + data[n + 3];
          ch.ChannelID = (data[n + 4] << 8) + data[n + 5];
          ch.ChannelName = System.Text.Encoding.ASCII.GetString(data, n + 6, 16);
          ch.ChannelName = ch.ChannelName.Trim();
          m_namesBuffer.Add(ch);
          Log.WriteFile(LogType.EPG, "mhw-epg: added channel {0} to mhw channels table", ch.ChannelName);
        }// for(int n=0
        //Log.Info("mhw-epg: found {0} channels for mhw",m_namesBuffer.Count);
        m_mhwChannelsCount = m_namesBuffer.Count;
      }
    }

    public void ParseThemes(byte[] data)
    {

      if (m_themeBuffer == null)
        return; // error
      if (m_themeBuffer.Count > 0)
        return; // already got channles table

      int dataLen = data.Length;
      lock (m_themeBuffer.SyncRoot)
      {
        try
        {
          int themesIndex = 3;
          int themesNames = 19;
          int theme = 0;
          int val = 0;
          int count = (dataLen - 19) / 15;
          for (int i = 0; i < count; i++)
          {
            if (data[themesIndex + theme] == i)	/* New theme */
            {
              val = (val + 15) & 0xF0;
              theme++;
            }
            MHWTheme th = new MHWTheme();
            th.ThemeText = System.Text.Encoding.ASCII.GetString(data, themesNames, 15);
            th.ThemeText = th.ThemeText.Trim();
            th.ThemeIndex = val;
            m_themeBuffer.Add(th);
            Log.WriteFile(LogType.EPG, "mhw-epg: theme '{0}' with id 0x{1:X} found", th.ThemeText, th.ThemeIndex);
            val++;
            themesNames += 15;
          }
        }
        catch { }
      }// lock
    }
    //
    //
    //

    public void ParseSummaries(byte[] data)
    {

      if (m_summaryBuffer == null)
        return;

      lock (m_summaryBuffer.SyncRoot)
      {
        int dataLen = ((data[1] - 0x70) << 8) + data[2];
        int n = 0;
        Summary sum = new Summary();
        sum.ProgramID = (data[n + 3] << 24) + (data[n + 4] << 16) + (data[n + 5] << 8) + data[n + 6];
        sum.Description = "";
        n += 11 + (data[n + 10] * 7);
        sum.Description = System.Text.Encoding.ASCII.GetString(data, n, dataLen - n);
        if (SummaryExists(sum.ProgramID) == false && sum.ProgramID != -1)
        {
          m_summaryBuffer.Add(sum);
        }//if(m_summaryBuffer.Contains(sum)==false)
      }
    }
    public void ParseTitles(byte[] data)
    {
      //foreach(byte[] data in m_progtabBuffer)

      lock (m_titleBuffer.SyncRoot)
      {

        Programm prg = new Programm();
        if (data[3] == 0xff)
          return;
        prg.ChannelID = (data[3]) - 1;
        prg.ThemeID = data[4];
        int h = data[5] & 0x1F;
        int d = (data[5] & 0xE0) >> 5;
        prg.Summaries = (data[6] & 0x80) == 0 ? false : true;
        int m = data[6] >> 2;
        prg.Duration = ((data[9] << 8) + data[10]);// minutes
        prg.Title = System.Text.Encoding.ASCII.GetString(data, 11, 23);
        prg.Title = prg.Title.Trim();
        prg.PPV = (data[34] << 24) + (data[35] << 16) + (data[36] << 8) + data[37];
        prg.ID = (data[38] << 24) + (data[39] << 16) + (data[40] << 8) + data[41];
        // get time
        int d1 = d;
        int h1 = h;
        if (d1 == 7)
          d1 = 0;
        if (h1 > 15)
          h1 = h1 - 4;
        else if (h1 > 7)
          h1 = h1 - 2;
        else
          d1 = (d1 == 6) ? 0 : d1 + 1;

        prg.Time = new DateTime(System.DateTime.Now.Ticks);
        DateTime dayStart = new DateTime(System.DateTime.Now.Ticks);
        dayStart = dayStart.Subtract(new TimeSpan(1, dayStart.Hour, dayStart.Minute, dayStart.Second, dayStart.Millisecond));
        int day = (int)dayStart.DayOfWeek;

        prg.Time = dayStart;
        int minVal = (d1 - day) * 86400 + h1 * 3600 + m * 60;
        if (minVal < 21600)
          minVal += 604800;

        prg.Time = prg.Time.AddSeconds(minVal);
        if (prg.Time.Hour == 18 && prg.Time.Minute == 25 && prg.Time.Day == 20)
        {
          //					int a=0;
        }
        if (ProgramExists(prg.ID) == false)
        {
          m_titleBuffer.Add(prg);
        }

      }
    }

    int SubmittMHW(out DateTime reGrabTime)
    {
      reGrabTime = new DateTime();
      int count = 0;
      if (m_namesBuffer == null)
        return 0;
      if (m_namesBuffer.Count < 1)
        return 0;

      List<TVChannel> channels1 = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels1);
      DateTime[] reGrabTimes = new DateTime[channels1.Count];
      for (int i = 0; i < reGrabTimes.Length; ++i)
      {
        reGrabTimes[i] = new DateTime();
        reGrabTimes[i] = DateTime.MinValue;
      }
      TVDatabase.RemoveOldPrograms();

      lock (m_titleBuffer.SyncRoot)
      {
        Log.WriteFile(LogType.EPG, "mhw-epg: count of programms={0}", m_titleBuffer.Count);
        Log.WriteFile(LogType.EPG, "mhw-epg: buffer contains {0} summaries now", m_summaryBuffer.Count);
        ArrayList list = new ArrayList();
        foreach (Programm prg in m_titleBuffer)
        {
          DVBSections.EITDescr eit = new MediaPortal.TV.Recording.DVBSections.EITDescr();
          string channelName = string.Empty;
          if (prg.ChannelID >= m_namesBuffer.Count || prg.ChannelID < 0)
          {
            list.Add(prg);
            continue;
          }
          int programID = ((MHWChannel)m_namesBuffer[prg.ChannelID]).ChannelID;
          int tsID = ((MHWChannel)m_namesBuffer[prg.ChannelID]).NetworkID;
          switch (m_cardType)
          {
            case (int)EPGCard.TechnisatStarCards:
              channelName = TVDatabase.GetSatChannelName(programID, tsID);
              break;

            case (int)EPGCard.BDACards:
              {
                ArrayList channels = new ArrayList();
                TVDatabase.GetChannels(ref channels);
                int freq, symbolrate, innerFec, modulation, ONID, TSID, SID, pcrPid;
                int audioPid, videoPid, teletextPid, pmtPid, bandWidth;
                int audio1, audio2, audio3, ac3Pid;
                string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
                bool HasEITPresentFollow, HasEITSchedule;
                string provider = "";
                foreach (TVChannel chan in channels)
                {
                  switch (m_networkType)
                  {
                    case NetworkType.DVBC:
                      TVDatabase.GetDVBCTuneRequest(chan.ID, out provider, out freq, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
                      if (programID == SID && tsID == TSID)
                      {
                        channelName = chan.Name;
                      }
                      break;
                    case NetworkType.DVBS:
                      channelName = TVDatabase.GetSatChannelName(programID, tsID);
                      break;
                    case NetworkType.DVBT:
                      TVDatabase.GetDVBTTuneRequest(chan.ID, out provider, out freq, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
                      if (programID == SID && tsID == TSID)
                      {
                        channelName = chan.Name;
                      }
                      break;
                  }
                  if (channelName != string.Empty) break;
                }//foreach (TVChannel chan in channels)
              }
              break;

            case (int)EPGCard.ChannelName:
              break;
          }
          if (channelName == "")
          {
            list.Add(prg);
            //m_titleBuffer.Remove(prg);// remove if it is in database
            continue;
          }
          eit.event_name = prg.Title;
          eit.program_number = programID;
          eit.event_text = GetSummaryByPrgID(prg.ID);
          if (eit.event_text == "")
            eit.event_text = String.Format("0x{0:X}", prg.ID);
          eit.genere_text = GetThemeText(prg.ThemeID);
          eit.duration_mm = prg.Duration;
          eit.isMHWEvent = true;
          eit.shortEventUseable = true;
          eit.mhwStartTime = prg.Time;
          eit.starttime_y = prg.Time.Year;
          eit.starttime_m = prg.Time.Month;
          eit.starttime_d = prg.Time.Day;
          eit.starttime_hh = prg.Time.Hour;
          eit.starttime_mm = prg.Time.Minute;
          eit.starttime_ss = prg.Time.Second;

          DateTime dateProgramEnd;
          int result = SetEITToDatabase(eit, channelName, 0, out dateProgramEnd);
          if (result == 1)
          {
            count++;
            list.Add(prg);
            //m_titleBuffer.Remove(prg);// remove if it is in database
          }
          if (result == -2)
            list.Add(prg);

          if (dateProgramEnd != DateTime.MinValue)
          {
            for (int i = 0; i < channels1.Count; ++i)
            {
              TVChannel chan = channels1[i];
              if (chan.Name.Equals(channelName))
              {
                if (dateProgramEnd > reGrabTimes[i])
                {
                  reGrabTimes[i] = dateProgramEnd;
                }
                break;
              }
            }
          }
        }


        foreach (Programm prg in list)
        {
          m_titleBuffer.Remove(prg);
        }
        m_titleBuffer.TrimToSize();

        if (count > 0)
        {
          Log.WriteFile(LogType.EPG, "mhw-epg: added {0} entries to database", m_addsToDatabase);
          Log.WriteFile(LogType.EPG, "mhw-epg: titles buffer contains {0} objects", m_titleBuffer.Count);
          Log.WriteFile(LogType.EPG, "mhw-epg: summaries buffer contains {0} objects", m_summaryBuffer.Count);
        }
        //m_titleBuffer.Clear();
        for (int i = 0; i < reGrabTimes.Length; ++i)
        {
          TVChannel ch = channels1[i];
          if (reGrabTimes[i] != DateTime.MinValue)
          {
            if (reGrabTimes[i] < reGrabTime || reGrabTime == DateTime.MinValue)
              reGrabTime = reGrabTimes[i];
          }
        }

        return count;
      }

    }
    //
    string GetSummaryByPrgID(int id)
    {
      if (m_summaryBuffer == null)
        return "";
      if (m_summaryBuffer.Count < 1)
        return "";
      lock (m_summaryBuffer.SyncRoot)
      {
        foreach (Summary sum in m_summaryBuffer)
        {
          if (sum.ProgramID == id)
            return sum.Description;
        }
      }
      return "";
    }
    string GetThemeText(int themeID)
    {
      if (m_themeBuffer == null)
        return Strings.Unknown;
      if (m_themeBuffer.Count < 1)
        return Strings.Unknown;
      lock (m_themeBuffer.SyncRoot)
      {
        foreach (MHWTheme th in m_themeBuffer)
        {
          if (th.ThemeIndex == themeID)
            return th.ThemeText;
        }
        return Strings.Unknown;
      }
    }
    bool ProgramExists(int prgID)
    {
      lock (m_titleBuffer.SyncRoot)
      {
        foreach (Programm prg in m_titleBuffer)
        {
          if (prg.ID == prgID)
            return true;
        }
      }
      return false;
    }
    bool SummaryExists(int prgID)
    {
      lock (m_summaryBuffer.SyncRoot)
      {
        foreach (Summary sum in m_summaryBuffer)
        {
          if (sum.ProgramID == prgID)
            return true;
        }
      }
      return false;
    }

  }// class
}// namespace
