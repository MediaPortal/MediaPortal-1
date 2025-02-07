#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvEngine
{

  #region TVMChannel struct

  public struct TVMChannel
  {
    private string fID;
    private string fSenderKennung;
    private string fBezeichnung;
    private string fWebseite;
    private string fSortNrTVMovie;
    private string fZeichen;

    public TVMChannel(string aID, string aSenderKennung, string aBezeichnung, string aWebseite, string aSortNrTVMovie,
                      string aZeichen)
    {
      fID = aID;
      fSenderKennung = aSenderKennung;
      fBezeichnung = aBezeichnung;
      fWebseite = aWebseite;
      fSortNrTVMovie = aSortNrTVMovie;
      fZeichen = aZeichen;
    }

    public string TvmId
    {
      get { return fID; }
    }

    public string TvmEpgChannel
    {
      get { return fSenderKennung; }
    }

    public string TvmEpgDescription
    {
      get { return fBezeichnung; }
    }

    public string TvmWebLink
    {
      get { return fWebseite; }
    }

    public string TvmSortId
    {
      get { return fSortNrTVMovie; }
    }

    public string TvmZeichen
    {
      get { return fZeichen; }
    }
  }

  #endregion

  internal class TvMovieDatabase
  {
    #region Members

    private OleDbConnection _databaseConnection = null;
    private bool _canceled = false;
    private List<TVMChannel> _tvmEpgChannels;
    private List<Program> _tvmEpgProgs = new List<Program>(500);
    private List<Channel> _channelList = null;
    private int _programsCounter = 0;
    private bool _useShortProgramDesc = false;
    private bool _extendDescription = true;
    private bool _showRatings = true;
    private bool _showAudioFormat = false;
    private bool _slowImport = true;
    private int _actorCount = 5;
    private bool _showLive = true;
    private bool _showRepeat = false;

    private static string _xmlFile;
    private static TvBusinessLayer _tvbLayer;

    #endregion

    #region Events

    public delegate void ProgramsChanged(int value, int maximum, string text);

    //public event ProgramsChanged OnProgramsChanged;
    public delegate void StationsChanged(int value, int maximum, string text);

    public event StationsChanged OnStationsChanged;

    #endregion

    #region Mapping struct

    private struct Mapping
    {
      private string _mpChannel;
      private string _tvmEpgChannel;
      private int _mpIdChannel;
      private TimeSpan _start;
      private TimeSpan _end;

      public Mapping(string mpChannel, int mpIdChannel, string tvmChannel, string start, string end)
      {
        _mpChannel = mpChannel;
        _mpIdChannel = mpIdChannel;
        _tvmEpgChannel = tvmChannel;
        _start = CleanInput(start);
        _end = CleanInput(end);
      }

      #region struct properties

      public string Channel
      {
        get { return _mpChannel; }
      }

      public int IdChannel
      {
        get { return _mpIdChannel; }
      }

      public string TvmEpgChannel
      {
        get { return _tvmEpgChannel; }
      }

      public TimeSpan Start
      {
        get { return _start; }
      }

      public TimeSpan End
      {
        get { return _end; }
      }

      private static TimeSpan CleanInput(string input)
      {
        int hours = 0;
        int minutes = 0;
        input = input.Trim();
        int index = input.IndexOf(':');
        if (index > 0)
          hours = Convert.ToInt16(input.Substring(0, index));
        if (index + 1 < input.Length)
          minutes = Convert.ToInt16(input.Substring(index + 1));

        if (hours > 23)
          hours = 0;

        if (minutes > 59)
          minutes = 0;

        return new TimeSpan(hours, minutes, 0);
      }

      #endregion
    }

    #endregion

    #region Properties

    public List<TVMChannel> Stations
    {
      get { return _tvmEpgChannels; }
    }

    public bool Canceled
    {
      get { return _canceled; }
      set { _canceled = value; }
    }

    public int Programs
    {
      get { return _programsCounter; }
    }

    public static TvBusinessLayer TvBLayer
    {
      get
      {
        if (_tvbLayer == null)
          _tvbLayer = new TvBusinessLayer();
        return _tvbLayer;
      }
    }

    #endregion

    #region Public functions

    public List<Channel> GetChannels()
    {
      List<Channel> tvChannels = new List<Channel>();
      try
      {
        IList<Channel> allChannels = Channel.ListAll();
        foreach (Channel channel in allChannels)
        {
          if (channel.IsTv && channel.VisibleInGuide)
            tvChannels.Add(channel);
        }
      }
      catch (Exception ex)
      {
        Log.Info("TVMovie: Exception in GetChannels: {0}\n{1}", ex.Message, ex.StackTrace);
      }
      return tvChannels;
    }

    public bool Connect()
    {
      LoadMemberSettings();

      using (TvMovieDatabaseConnection conn = new TvMovieDatabaseConnection())
      {
        if (conn.Open(TvMovie.DatabasePath))
        {
          _tvmEpgChannels = conn.GetChannels();
          if (_tvmEpgChannels == null)
            return false;
        }
        else
          return false;
      }

      _channelList = GetChannels();
      return true;
    }

    public bool NeedsImport
    {
      get
      {
        try
        {
          TimeSpan restTime = new TimeSpan(Convert.ToInt32(TvBLayer.GetSetting("TvMovieRestPeriod", "24").Value), 0, 0);
          DateTime lastUpdated = Convert.ToDateTime(TvBLayer.GetSetting("TvMovieLastUpdate", "0").Value);
          //        if (Convert.ToInt64(TvBLayer.GetSetting("TvMovieLastUpdate", "0").Value) == LastUpdate)
          if (lastUpdated >= (DateTime.Now - restTime))
          {
            return false;
          }
          else
          {
            Log.Debug("TVMovie: Last update was at {0} - new import scheduled", Convert.ToString(lastUpdated));
            return true;
          }
        }
        catch (Exception ex)
        {
          Log.Info("TVMovie: An error occured checking the last import time {0}", ex.Message);
          Log.Write(ex);
          return true;
        }
      }
    }

    /// <summary>
    /// Loops through all channel to find mappings and finally import EPG to MP's DB
    /// </summary>
    public void Import()
    {
      if (_canceled)
        return;

      List<Mapping> mappingList = GetMappingList();
      if (mappingList == null || mappingList.Count < 1)
      {
        Log.Info("TVMovie: Cannot import from TV Movie database - no mappings found");
        return;
      }

      DateTime ImportStartTime = DateTime.Now;
      Log.Debug("TVMovie: Importing database");
      int maximum = 0;

      foreach (TVMChannel tvmChan in _tvmEpgChannels)
        foreach (Mapping mapping in mappingList)
          if (mapping.TvmEpgChannel == tvmChan.TvmEpgChannel)
          {
            maximum++;
            break;
          }

      Log.Debug("TVMovie: Calculating stations done");

      // setting update time of epg import to avoid that the background thread triggers another import
      // if the process lasts longer than the timer's update check interval
      Setting setting = TvBLayer.GetSetting("TvMovieLastUpdate");
      setting.Value = DateTime.Now.ToString();
      setting.Persist();

      Log.Debug("TVMovie: Mapped {0} stations for EPG import", Convert.ToString(maximum));
      int counter = 0;

      _tvmEpgProgs.Clear();

      // get all tv channels from MP DB via gentle.net
      IList<Channel> allChannels = Channel.ListAll();

      using (TvMovieDatabaseConnection conn = new TvMovieDatabaseConnection())
      {
        if (conn.Open(TvMovie.DatabasePath))
        {
          foreach (TVMChannel station in _tvmEpgChannels)
          {
            if (_canceled)
              return;

            Log.Info("TVMovie: Searching time share mappings for station: {0}", station.TvmEpgDescription);
            // get all tv movie channels
            List<Mapping> channelNames = new List<Mapping>();

            foreach (Mapping mapping in mappingList)
              if (mapping.TvmEpgChannel == station.TvmEpgChannel)
                channelNames.Add(mapping);

            if (channelNames.Count > 0)
            {
              try
              {
                string display = String.Empty;
                foreach (Mapping channelName in channelNames)
                  display += string.Format("{0}  /  ", channelName.Channel);

                display = display.Substring(0, display.Length - 5);
                if (OnStationsChanged != null)
                  OnStationsChanged(counter, maximum, display);
                counter++;

                Log.Info("TVMovie: Importing {3} time frame(s) for MP channel [{0}/{1}] - {2}", Convert.ToString(counter),
                         Convert.ToString(maximum), display, Convert.ToString(channelNames.Count));

                _tvmEpgProgs.Clear();


                List<string[]> result = conn.GetChannelData(station.TvmEpgChannel);
                if (result == null)
                  return;

                int iCnt = 0;

                result.ForEach(data =>
                {
                  ImportSingleChannelData(channelNames, allChannels, iCnt,
                                    data[0], data[1], data[2], data[3], data[4], data[5],
                                    data[6], data[7], data[8], data[9], data[10],
                                    data[11], data[12], data[13],
                                    data[14], data[15], data[16],
                                    data[17], data[18], data[19], data[20],
                                    data[21], data[22], data[23], data[24]);
                  iCnt++;
                  }
                );

                _programsCounter += iCnt;

                ThreadPriority importPrio = _slowImport ? ThreadPriority.BelowNormal : ThreadPriority.AboveNormal;
                if (_slowImport)
                  Thread.Sleep(32);

                // make a copy of this list because Insert it done in syncronized threads - therefore the object reference would cause multiple/missing entries
                List<Program> InsertCopy = new List<Program>(_tvmEpgProgs);
                int debugCount = TvBLayer.InsertPrograms(InsertCopy, DeleteBeforeImportOption.OverlappingPrograms,
                                                         importPrio);
                Log.Info("TVMovie: Inserted {0} programs", debugCount);
              }
              catch (Exception ex)
              {
                Log.Info("TVMovie: Error inserting programs - {0}", ex.StackTrace);
              }
            }
          }

          Log.Debug("TVMovie: Waiting for database to be updated...");
          TvBLayer.WaitForInsertPrograms();
          Log.Debug("TVMovie: Database update finished.");


          if (OnStationsChanged != null)
            OnStationsChanged(maximum, maximum, "Import done");

          if (!_canceled)
          {
            try
            {
              setting = TvBLayer.GetSetting("TvMovieLastUpdate");
              setting.Value = DateTime.Now.ToString();
              setting.Persist();

              TimeSpan ImportDuration = (DateTime.Now - ImportStartTime);
              Log.Debug("TVMovie: Imported {0} database entries for {1} stations in {2} seconds", _programsCounter, counter,
                        Convert.ToString(ImportDuration.TotalSeconds));
            }
            catch (Exception)
            {
              Log.Info("TVMovie: Error updating the database with last import date");
            }
          }
        }
      }
      GC.Collect();
    }

    #endregion

    #region Private functions

    private void LoadMemberSettings()
    {
      _useShortProgramDesc = TvBLayer.GetSetting("TvMovieShortProgramDesc", "false").Value == "true";
      _extendDescription = TvBLayer.GetSetting("TvMovieExtendDescription", "true").Value == "true";
      _showRatings = TvBLayer.GetSetting("TvMovieShowRatings", "true").Value == "true";
      _showAudioFormat = TvBLayer.GetSetting("TvMovieShowAudioFormat", "false").Value == "true";
      _slowImport = TvBLayer.GetSetting("TvMovieSlowImport", "true").Value == "true";
      _actorCount = Convert.ToInt32(TvBLayer.GetSetting("TvMovieLimitActors", "5").Value);
      _showLive = TvBLayer.GetSetting("TvMovieShowLive", "true").Value == "true";
      _showRepeat = TvBLayer.GetSetting("TvMovieShowRepeating", "false").Value == "true";
      _xmlFile = String.Format(@"{0}\TVMovieMapping.xml", PathManager.GetDataPath);
    }

    // sqlb.Append(", TVDaten.live, TVDaten.Dauer, TVDaten.Herstellungsland,TVDaten.Wiederholung");
    /// <summary>
    /// Takes a DataRow worth of EPG Details to persist them in MP's program table
    /// </summary>
    private void ImportSingleChannelData(List<Mapping> channelNames, IList<Channel> allChannels, int aCounter,
                                         string SenderKennung, string Beginn, string Ende, string Sendung, string Genre,
                                         string Kurzkritik, string KurzBeschreibung, string Beschreibung,
                                         string Audiodescription, string DolbySuround, string Stereo,
                                         string DolbyDigital, string Dolby, string Zweikanalton,
                                         string FSK, string Herstellungsjahr, string Originaltitel, string Regie,
                                         string Darsteller, string Interessant, string Bewertungen,
                                         string Live, string Dauer, string Herstellungsland, string Wiederholung)
    {
      string channel = SenderKennung;
      DateTime end = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
      DateTime start = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
      string classification = String.Empty;
      int parentalRating = 0;
      string date = String.Empty;
      string duration = String.Empty;
      string origin = String.Empty;
      string episode = String.Empty;
      int starRating = -1;
      string detailedRating = String.Empty;
      string director = String.Empty;
      string actors = String.Empty;
      string audioFormat = String.Empty;
      bool live = false;
      bool repeating = false;
      try
      {
        end = DateTime.Parse(Ende); // iEndTime ==> Ende  (15.06.2006 22:45:00 ==> 20060615224500)
        start = DateTime.Parse(Beginn); // iStartTime ==> Beginn (15.06.2006 22:45:00 ==> 20060615224500)
        live = Convert.ToBoolean(Live);
        repeating = Convert.ToBoolean(Wiederholung);
      }
      catch (Exception ex2)
      {
        Log.Info("TVMovie: Error parsing EPG time data - {0}", ex2.ToString());
      }

      string title = Sendung;
      // indicate live programs
      if (_showLive)
      {
        if (live)
          title += " (LIVE)";
      }
      // indicate repeatings
      if (_showRepeat)
      {
        if (repeating)
          title += " (Wdh.)";
      }

      string genre = Genre;
      string shortCritic = Kurzkritik;

      if (_extendDescription)
      {
        classification = FSK;
        int.TryParse(FSK, out parentalRating);
        date = Herstellungsjahr;
        episode = Originaltitel;
        director = Regie;
        actors = Darsteller;
        duration = Dauer;
        origin = Herstellungsland;
        //int repeat = Convert.ToInt16(guideEntry["Wiederholung"]);         // strRepeat ==> Wiederholung "Repeat" / "unknown"      
      }

      if (_showRatings)
      {
        starRating = Convert.ToInt16(Interessant) - 1;
        detailedRating = Bewertungen;
      }

      short EPGStarRating = -1;
      switch (starRating)
      {
        case 0:
          EPGStarRating = 2;
          break;
        case 1:
          EPGStarRating = 4;
          break;
        case 2:
          EPGStarRating = 6;
          break;
        case 3:
          EPGStarRating = 8;
          break;
        case 4:
          EPGStarRating = 10;
          break;
        case 5:
          EPGStarRating = 8;
          break;
        case 6:
          EPGStarRating = 10;
          break;
        default:
          EPGStarRating = -1;
          break;
      }

      foreach (Mapping channelMap in channelNames)
      {
        DateTime newStartDate = start;
        DateTime newEndDate = end;

        if (!CheckTimesharing(ref newStartDate, ref newEndDate, channelMap.Start, channelMap.End))
        {
          Channel progChannel = null;
          foreach (Channel MpChannel in allChannels)
          {
            if (MpChannel.IdChannel == channelMap.IdChannel)
            {
              progChannel = MpChannel;
              break;
            }
          }
          DateTime OnAirDate = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
          if (date.Length > 0 && date != @"-")
          {
            try
            {
              OnAirDate = DateTime.Parse(String.Format("01.01.{0} 00:00:00", date));
            }
            catch (Exception)
            {
              Log.Info("TVMovie: Invalid year for OnAirDate - {0}", date);
            }
          }

          string shortDescription = KurzBeschreibung;
          string description;
          if (_useShortProgramDesc)
            description = shortDescription;
          else
          {
            description = Beschreibung;
            // If short desc has info but "long" desc has not
            if (description.Length < shortDescription.Length)
              description = shortDescription;
          }

          description = description.Replace("<br>", "\n");

          if (_extendDescription)
          {
            StringBuilder sb = new StringBuilder();
            // Avoid duplicate episode title
            if ((episode != String.Empty) && (episode != Sendung))
            {
              if (!String.IsNullOrEmpty(duration))
                sb.AppendFormat("Folge: {0} ({1})\n", episode, duration);
              else
                sb.AppendFormat("Folge: {0}\n", episode);
            }

            if (!String.IsNullOrEmpty(date))
            {
              if (!String.IsNullOrEmpty(origin))
                sb.AppendFormat("Aus: {0} {1}\n", origin, date);
              else
                sb.AppendFormat("Jahr: {0}\n", date);
            }

            if (starRating != -1 && _showRatings)
            {
              //sb.Append("Wertung: " + string.Format("{0}/5", starRating) + "\n");
              sb.Append("Wertung: ");
              if (shortCritic.Length > 1)
              {
                sb.Append(shortCritic);
                sb.Append(" - ");
              }
              sb.Append(BuildRatingDescription(starRating));
              if (detailedRating.Length > 0)
                sb.Append(BuildDetailedRatingDescription(detailedRating));
            }
            if (!String.IsNullOrEmpty(description))
            {
              sb.Append(description);
              sb.Append("\n");
            }
            if (director.Length > 0)
              sb.AppendFormat("Regie: {0}\n", director);
            if (actors.Length > 0)
              sb.Append(BuildActorsDescription(actors));
            if (!String.IsNullOrEmpty(classification) && classification != "0")
              sb.AppendFormat("FSK: {0}\n", classification);

            description = sb.ToString();
          }
          else
          {
            if (_showRatings)
              if (shortCritic.Length > 1)
                description = shortCritic + "\n" + description;
          }

          if (_showAudioFormat)
          {
            audioFormat = BuildAudioDescription(Convert.ToBoolean(Audiodescription),
                                                Convert.ToBoolean(DolbyDigital),
                                                Convert.ToBoolean(DolbySuround),
                                                Convert.ToBoolean(Dolby),
                                                Convert.ToBoolean(Stereo),
                                                Convert.ToBoolean(Zweikanalton));

            if (!String.IsNullOrEmpty(audioFormat))
              description += "Ton: " + audioFormat;
          }

          Program prog = new Program(progChannel.IdChannel, newStartDate, newEndDate, title, description, genre,
                                     Program.ProgramState.None, OnAirDate, String.Empty, String.Empty, episode,
                                     String.Empty, EPGStarRating, classification, parentalRating);

          _tvmEpgProgs.Add(prog);

          if (_slowImport && aCounter % 2 == 0)
            Thread.Sleep(20);
        }
      }
    }

    /// <summary>
    /// Retrieve all channel-mappings from TvMovieMapping table
    /// </summary>
    /// <returns></returns>
    private List<Mapping> GetMappingList()
    {
      List<Mapping> mappingList = new List<Mapping>();
      try
      {
        IList<TvMovieMapping> mappingDb = TvMovieMapping.ListAll();
        foreach (TvMovieMapping mapping in mappingDb)
        {
          try
          {
            string newStart = mapping.TimeSharingStart;
            string newEnd = mapping.TimeSharingEnd;
            string newStation = mapping.StationName;
            string newChannel = Channel.Retrieve(mapping.IdChannel).DisplayName;
            int newIdChannel = mapping.IdChannel;

            mappingList.Add(new Mapping(newChannel, newIdChannel, newStation, newStart, newEnd));
          }
          catch (Exception)
          {
            Log.Info("TVMovie: Error loading mappings - make sure tv channel: {0} (ID: {1}) still exists!",
                     mapping.StationName, mapping.IdChannel);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("TVMovie: Error in GetMappingList - {0}\n{1}", ex.Message, ex.StackTrace);
      }
      return mappingList;
    }

    /// <summary>
    /// Determines whether an entry is valid for a timesharing station 
    /// </summary>
    private bool CheckTimesharing(ref DateTime progStart, ref DateTime progEnd, TimeSpan timeSharingStart,
                                  TimeSpan timeSharingEnd)
    {
      if (timeSharingStart == timeSharingEnd)
        return false;

      DateTime stationStart = progStart.Date + timeSharingStart;
      DateTime stationEnd = progStart.Date + timeSharingEnd;

      if (stationStart > progStart && progEnd <= stationStart)
        stationStart = stationStart.AddDays(-1);
      else if (timeSharingEnd < timeSharingStart)
        stationEnd = stationEnd.AddDays(1);

      if (progStart >= stationStart && progStart < stationEnd && progEnd > stationEnd)
        progEnd = stationEnd;

      if (progStart <= stationStart && progEnd > stationStart && progEnd < stationEnd)
        progStart = stationStart;

      if ((progEnd <= stationEnd) && (progStart >= stationStart))
        return false;

      return true;
    }

    /// <summary>
    /// passing the TV movie sound bool params this method returns the audio format as string
    /// </summary>
    /// <param name="audioDesc"></param>
    /// <param name="dolbyDigital"></param>
    /// <param name="dolbySuround"></param>
    /// <param name="dolby"></param>
    /// <param name="stereo"></param>
    /// <param name="dualAudio"></param>
    /// <returns>plain text audio format</returns>
    private string BuildAudioDescription(bool audioDesc, bool dolbyDigital, bool dolbySurround, bool dolby, bool stereo,
                                         bool dualAudio)
    {
      StringBuilder sb = new StringBuilder();
      if (dolbyDigital)
        sb.AppendLine("Dolby Digital");
      if (dolbySurround)
        sb.AppendLine("Dolby Surround");
      if (dolby)
        sb.AppendLine("Dolby");
      if (stereo)
        sb.AppendLine("Stereo");
      if (dualAudio)
        sb.AppendLine("Mehrkanal-Ton");

      string result = sb.ToString().Replace(Environment.NewLine, ",");
      // Remove trailing comma
      if (result.EndsWith(","))
        result = result.Remove(result.Length - 1);
      return result;
    }

    /// <summary>
    /// Translates the numeric db values for rating into readable text
    /// </summary>
    /// <param name="dbRating"></param>
    /// <returns>One word indicating the rating</returns>
    private string BuildRatingDescription(int dbRating)
    {
      string TVMovieRating = String.Empty;

      switch (dbRating)
      {
        case 0:
          TVMovieRating = "uninteressant";
          break;
        case 1:
          TVMovieRating = "durchschnittlich";
          break;
        case 2:
          TVMovieRating = "empfehlenswert";
          break;
        case 3:
          TVMovieRating = "Tages-Tipp!";
          break;
        case 4:
          TVMovieRating = "Blockbuster!";
          break;
        case 5:
          TVMovieRating = "Genre-Tipp";
          break;
        case 6:
          TVMovieRating = "Genre-Highlight!";
          break;
        default:
          TVMovieRating = "---";
          break;
      }

      return TVMovieRating + "\n";
    }

    /// <summary>
    /// Formats the db rating into nice text
    /// </summary>
    /// <param name="dbDetailedRating"></param>
    /// <returns></returns>
    private string BuildDetailedRatingDescription(string dbDetailedRating)
    {
      // "Spa�=1;Action=3;Erotik=1;Spannung=3;Anspruch=0"
      int posidx = 0;
      string detailedRating = String.Empty;
      StringBuilder strb = new StringBuilder();

      if (dbDetailedRating != String.Empty)
      {
        posidx = dbDetailedRating.IndexOf(@"Spa�=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 5] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 6));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Action=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 7] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 8));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Erotik=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 7] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 8));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Spannung=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 9] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 10));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Anspruch=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 9] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 10));
            strb.Append("\n");
          }
        }
        posidx = dbDetailedRating.IndexOf(@"Gef�hl=");
        if (posidx > 0)
        {
          if (dbDetailedRating[posidx + 7] != '0')
          {
            strb.Append(dbDetailedRating.Substring(posidx, 8));
            strb.Append("\n");
          }
        }
        detailedRating = strb.ToString();
      }

      return detailedRating;
    }

    /// <summary>
    /// Formats the actors text into a string suitable for the description field
    /// </summary>
    /// <param name="dbActors"></param>
    /// <returns></returns>
    private string BuildActorsDescription(string dbActors)
    {
      StringBuilder strb = new StringBuilder();
      // Mit: Bernd Schramm (Buster der Hund);Sandra Schwarzhaupt (Gwendolyn die Katze);Joachim Kemmer (Tortellini der Hahn);Mario Adorf (Fred der Esel);Katharina Thalbach (die Erbin);Peer Augustinski (Dr. Gier);Klausj�rgen Wussow (Der Erz�hler);Hartmut Engler (Hund Buster);Bert Henry (Drehbuch);Georg Reichel (Drehbuch);Dagmar Kekule (Drehbuch);Peter Wolf (Musik);Dagmar Kekul� (Drehbuch)
      strb.Append("Mit: ");
      if (_actorCount < 1)
      {
        strb.Append(dbActors);
        strb.Append("\n");
      }
      else
      {
        string[] splitActors = dbActors.Split(';');
        if (splitActors != null && splitActors.Length > 0)
        {
          for (int i = 0; i < splitActors.Length; i++)
          {
            if (i < _actorCount)
            {
              strb.Append(splitActors[i]);
              strb.Append("\n");
            }
            else
              break;
          }
        }
      }

      return strb.ToString();
    }

    private long datetolong(DateTime dt)
    {
      try
      {
        long iSec = 0; //(long)dt.Second;
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
      catch (Exception) {}
      return 0;
    }

    /// <summary>
    /// Launches TV Movie's own internet update tool
    /// </summary>
    /// <returns>Number of seconds needed for the update</returns>
    public long LaunchTVMUpdater(bool aHideUpdater)
    {
      long UpdateDuration = 0;
      string UpdaterPath = Path.Combine(TvMovie.TVMovieProgramPath, @"tvuptodate.exe");
      if (File.Exists(UpdaterPath))
      {
        Stopwatch BenchClock = new Stopwatch();

        try
        {
          BenchClock.Start();

          // check whether e.g. tv movie itself already started an update
          Process[] processes = Process.GetProcessesByName("tvuptodate");
          if (processes.Length > 0)
          {
            processes[0].WaitForExit(1200000);
            BenchClock.Stop();
            UpdateDuration = (BenchClock.ElapsedMilliseconds / 1000);
            Log.Info("TVMovie: tvuptodate was already running - waited {0} seconds for internet update to finish",
                     Convert.ToString(UpdateDuration));
            return UpdateDuration;
          }

          ProcessStartInfo startInfo = new ProcessStartInfo("tvuptodate.exe");
          if (aHideUpdater)
            startInfo.Arguments = "/hidden";
          startInfo.FileName = UpdaterPath;
          // replaced with startInfo.Arguments = "/hidden" | flokel | 11.01.09
          // startInfo.WindowStyle = aHideUpdater ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
          startInfo.WorkingDirectory = Path.GetDirectoryName(UpdaterPath);
          Process UpdateProcess = Process.Start(startInfo);
          UpdateProcess.PriorityClass = ProcessPriorityClass.BelowNormal;

          UpdateProcess.WaitForExit(1200000); // do not wait longer than 20 minutes for the internet update

          BenchClock.Stop();
          UpdateDuration = (BenchClock.ElapsedMilliseconds / 1000);
          Log.Info("TVMovie: tvuptodate finished internet update in {0} seconds", Convert.ToString(UpdateDuration));
        }
        catch (Exception ex)
        {
          BenchClock.Stop();
          UpdateDuration = (BenchClock.ElapsedMilliseconds / 1000);
          Log.Info("TVMovie: LaunchTVMUpdater failed: {0}", ex.Message);
        }
      }
      else
      {
        Log.Info("TVMovie: tvuptodate.exe not found in default location: {0}", UpdaterPath);
        UpdateDuration = 30; // workaround for systems without tvuptodate
      }

      return UpdateDuration;
    }

    #endregion
  }

  // class
}