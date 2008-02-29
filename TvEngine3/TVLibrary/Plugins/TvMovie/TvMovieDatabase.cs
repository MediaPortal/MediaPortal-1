#region Copyright (C) 2006-2008 Team MediaPortal

/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using Microsoft.Win32;

using Gentle.Common;
using Gentle.Framework;

using MySql.Data.MySqlClient;

using TvDatabase;
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

    public TVMChannel(string aID, string aSenderKennung, string aBezeichnung, string aWebseite, string aSortNrTVMovie)
    {
      fID = aID;
      fSenderKennung = aSenderKennung;
      fBezeichnung = aBezeichnung;
      fWebseite = aWebseite;
      fSortNrTVMovie = aSortNrTVMovie;
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
  }
  #endregion

  class TvMovieDatabase
  {
    #region Members
    private OleDbConnection _databaseConnection = null;
    private bool _canceled = false;
    private List<TVMChannel> _tvmEpgChannels;
    private List<Program> _tvmEpgProgs = new List<Program>(200);
    private ArrayList _channelList = null;
    private int _programsCounter = 0;
    private bool _useShortProgramDesc = false;
    private bool _extendDescription = false;
    private bool _showRatings = false;
    private bool _showAudioFormat = false;
    private bool _slowImport = false;
    private int _actorCount = 5;

    private static string _xmlFile;
    private static TvBusinessLayer _tvbLayer;
    #endregion

    #region Events
    public delegate void ProgramsChanged(int value, int maximum, string text);
    public event ProgramsChanged OnProgramsChanged;
    public delegate void StationsChanged(int value, int maximum, string text);
    public event StationsChanged OnStationsChanged;
    #endregion

    #region Mapping struct
    private struct Mapping
    {
      private string _mpChannel;
      private string _tvmEpgChannel;
      private TimeSpan _start;
      private TimeSpan _end;

      public Mapping(string mpChannel, string tvmChannel, string start, string end)
      {
        _mpChannel = mpChannel;
        _tvmEpgChannel = tvmChannel;
        _start = CleanInput(start);
        _end = CleanInput(end);
      }

      #region struct properties
      public string Channel
      {
        get { return _mpChannel; }
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

    public static string TVMovieProgramPath
    {
      get
      {
        string path = string.Empty;
        string mpPath = string.Empty;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
          if (rkey != null)
            path = string.Format("{0}", rkey.GetValue("ProgrammPath"));

        mpPath = TvBLayer.GetSetting("TvMovieInstallPath", path).Value;

        if (File.Exists(mpPath))
          return mpPath;

        return path;
      }
    }

    public static string DatabasePath
    {
      get
      {
        string path = string.Empty;
        string mpPath = string.Empty;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
          if (rkey != null)
            path = string.Format("{0}", rkey.GetValue("DBDatei"));

        mpPath = TvBLayer.GetSetting("TvMoviedatabasepath", path).Value;

        if (File.Exists(mpPath))
          return mpPath;

        return path;
      }
      set
      {
        string path = string.Empty;

        string newPath = value;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
          if (rkey != null)
            path = string.Format("{0}", rkey.GetValue("DBDatei"));

        if (!File.Exists(newPath))
          newPath = path;

        string mpPath = TvBLayer.GetSetting("TvMoviedatabasepath", string.Empty).Value;
        Setting setting = TvBLayer.GetSetting("TvMovieEnabled");

        if (newPath == path)
          setting.Value = string.Empty;
        else
          setting.Value = newPath;

        setting.Persist();
      }
    }
    #endregion

    #region Public functions
    public ArrayList GetChannels()
    {
      ArrayList tvChannels = new ArrayList();
      IList allChannels = Channel.ListAll();
      foreach (Channel channel in allChannels)
      {
        if (channel.IsTv && channel.VisibleInGuide)
          tvChannels.Add(channel);
      }
      return tvChannels;
    }

    public void Connect()
    {
      LoadMemberSettings();

      string dataProviderString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Mode=Share Deny None;Jet OLEDB:Engine Type=5;Jet OLEDB:Database Locking Mode=1;";
      if (DatabasePath != string.Empty)
        dataProviderString = string.Format(dataProviderString, DatabasePath);
      else
        return;

      _databaseConnection = new OleDbConnection(dataProviderString);

      string sqlSelect = "SELECT ID, SenderKennung, Bezeichnung, Webseite, SortNrTVMovie FROM Sender WHERE (Favorit = true) AND (GueltigBis >=Now()) ORDER BY Bezeichnung ASC;";

      OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection);
      OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand);
      DataSet tvMovieTable = new DataSet();

      try
      {
        _databaseConnection.Open();
        databaseAdapter.Fill(tvMovieTable, "Sender");
      }
      catch (System.Data.OleDb.OleDbException ex)
      {
        Log.Error("TVMovie: Error accessing TV Movie Clickfinder database while reading stations");
        Log.Error("TVMovie: Exception: {0}", ex);
        _canceled = true;
        return;
      }
      finally
      {
        _databaseConnection.Close();
      }

      _tvmEpgChannels = new List<TVMChannel>();
      foreach (DataRow sender in tvMovieTable.Tables["Sender"].Rows)
      {
        TVMChannel current = new TVMChannel(sender["ID"].ToString(),
                                            sender["SenderKennung"].ToString(),
                                            sender["Bezeichnung"].ToString(),
                                            sender["Webseite"].ToString(),
                                            sender["SortNrTVMovie"].ToString()
                                            );
        _tvmEpgChannels.Add(current);
      }
      _channelList = GetChannels();
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
          Log.Error("TVMovie: An error occured checking the last import time {0}", ex.Message);
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

      ArrayList mappingList = GetMappingList();
      if (mappingList == null)
      {
        Log.Error("TVMovie: Cannot import from TV Movie database");
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

      foreach (TVMChannel station in _tvmEpgChannels)
      {
        if (_canceled)
          return;

        Log.Info("TVMovie: Searching time share mappings for station: {0}", station.TvmEpgDescription);

        // get all tv movie channels
        List<Mapping> channelNames = new List<Mapping>();
        // get all tv channels
        IList allChannels = Channel.ListAll();

        foreach (Mapping mapping in mappingList)
          if (mapping.TvmEpgChannel == station.TvmEpgChannel)
            channelNames.Add(mapping);

        if (channelNames.Count > 0)
        {
          try
          {
            string display = string.Empty;
            foreach (Mapping channelName in channelNames)
              display += string.Format("{0}  /  ", channelName.Channel);

            display = display.Substring(0, display.Length - 5);
            if (OnStationsChanged != null)
              OnStationsChanged(counter, maximum, display);
            counter++;
            
            Log.Info("TVMovie: Importing {3} time frame(s) for MP channel [{0}/{1}] - {2}", Convert.ToString(counter), Convert.ToString(maximum), display, Convert.ToString(channelNames.Count));
            
            _tvmEpgProgs.Clear();
            
            _programsCounter += ImportStation(station.TvmEpgChannel, channelNames, allChannels);

            ThreadPriority importPrio = _slowImport ? ThreadPriority.BelowNormal : ThreadPriority.AboveNormal;
            if (_slowImport)
              Thread.Sleep(30);

            // make a copy of this list because Insert it done in syncronized threads - therefore the object reference would cause multiple/missing entries
            List<Program> InsertCopy = new List<Program>(_tvmEpgProgs);
            int debugCount = TvBLayer.InsertPrograms(InsertCopy, importPrio);
            Log.Info("TVMovie: Inserted {0} programs", debugCount);
          }
          catch (Exception ex)
          {
            Log.Info("TVMovie: Error inserting programs - {0}", ex.StackTrace);
          }
        }
      }
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
          Log.Debug("TVMovie: Imported {0} database entries for {1} stations in {2} seconds", _programsCounter, counter, Convert.ToString(ImportDuration.TotalSeconds));
        }
        catch (Exception)
        {
          Log.Error("TVMovie: Error updating the database with last import date");
        }
      }
      GC.Collect();
    }
    #endregion

    #region Private functions
    private void LoadMemberSettings()
    {
      _useShortProgramDesc = TvBLayer.GetSetting("TvMovieShortProgramDesc", "true").Value == "true";
      _extendDescription = TvBLayer.GetSetting("TvMovieExtendDescription", "false").Value == "true";
      _showRatings = TvBLayer.GetSetting("TvMovieShowRatings", "false").Value == "true";
      _showAudioFormat = TvBLayer.GetSetting("TvMovieShowAudioFormat", "false").Value == "true";
      _slowImport = TvBLayer.GetSetting("TvMovieSlowImport", "false").Value == "true";
      _actorCount = Convert.ToInt32(TvBLayer.GetSetting("TvMovieLimitActors", "5").Value);

      _xmlFile = String.Format(@"{0}\MediaPortal TV Server\TVMovieMapping.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    }

    private int ImportStation(string stationName, List<Mapping> channelNames, IList allChannels)
    {
      int counter = 0;
      bool useGentle = false;
      string sqlSelect = string.Empty;
      StringBuilder sqlb = new StringBuilder();

      // UNUSED: F16zu9 , live , untertitel , Dauer , Wiederholung
      sqlb.Append("SELECT TVDaten.SenderKennung, TVDaten.Beginn, TVDaten.Ende, TVDaten.Sendung, TVDaten.Genre, TVDaten.Kurzkritik, TVDaten.KurzBeschreibung, TVDaten.Beschreibung");
      sqlb.Append(", TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton");
      sqlb.Append(", TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Originaltitel, TVDaten.Regie, TVDaten.Darsteller");
      sqlb.Append(", TVDaten.Interessant, TVDaten.Bewertungen");
      sqlb.Append(" FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;");

      sqlSelect = string.Format(sqlb.ToString(), stationName);
      OleDbTransaction databaseTransaction = null;
      OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection);

      foreach (Mapping map in channelNames)
        if (map.TvmEpgChannel == stationName)
        {
          Log.Debug("TVMovie: Purging old programs for channel {0}", map.Channel);
          ClearPrograms(map.Channel);
          if (_slowImport)
            Thread.Sleep(75);
        }

      try
      {
        int programsCount = 0;
        _databaseConnection.Open();
        // The main app might change epg details while importing
        databaseTransaction = _databaseConnection.BeginTransaction(IsolationLevel.ReadCommitted);
        databaseCommand.Transaction = databaseTransaction;
        OleDbDataReader reader = databaseCommand.ExecuteReader(CommandBehavior.SequentialAccess);

        while (reader.Read())
        {
          ImportSingleChannelData(channelNames, allChannels, useGentle,
                                    reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString(), reader[5].ToString(), reader[6].ToString(), reader[7].ToString(),
                                    reader[8].ToString(), reader[9].ToString(), reader[10].ToString(), reader[11].ToString(), reader[12].ToString(), reader[13].ToString(),
                                    reader[14].ToString(), reader[15].ToString(), reader[16].ToString(), reader[17].ToString(), reader[18].ToString(), reader[19].ToString(), reader[20].ToString()
                                  );
          programsCount++;
          counter++;
        }
        databaseTransaction.Commit();
        reader.Close();

        //if (OnProgramsChanged != null)
        //  OnProgramsChanged(programsCount + 1, programsCount + 1, string.Empty);
      }
      catch (System.Data.OleDb.OleDbException ex)
      {
        databaseTransaction.Rollback();
        Log.Error("TVMovie: Error accessing TV Movie Clickfinder database - import of current station canceled");
        Log.Error("TVMovie: Exception: {0}", ex);
        return 0;
      }
      catch (Exception ex1)
      {
        databaseTransaction.Rollback();
        Log.Error("TVMovie: Exception: {0}", ex1);
        return 0;
      }
      finally
      {
        _databaseConnection.Close();
      }

      return counter;
    }

    /// <summary>
    /// Takes a DataRow worth of EPG Details to persist them in MP's program table
    /// </summary>
    private void ImportSingleChannelData(List<Mapping> channelNames, IList allChannels, bool useGentlePersist,
                                         string SenderKennung, string Beginn, string Ende, string Sendung, string Genre, string Kurzkritik, string KurzBeschreibung, string Beschreibung,
                                         string Audiodescription, string DolbySuround, string Stereo, string DolbyDigital, string Dolby, string Zweikanalton,
                                         string FSK, string Herstellungsjahr, string Originaltitel, string Regie, string Darsteller, string Interessant, string Bewertungen)
    {
      // Log.Info("TVMovie: Import program: {0} - {1}", Beginn, Sendung);

      string channel = SenderKennung;
      DateTime end = DateTime.MinValue;
      DateTime start = DateTime.MinValue;
      string classification = string.Empty;
      string date = string.Empty;
      string episode = string.Empty;
      int starRating = -1;
      string detailedRating = string.Empty;
      string director = string.Empty;
      string actors = string.Empty;
      string audioFormat = string.Empty;
      try
      {
        end = DateTime.Parse(Ende);     // iEndTime ==> Ende  (15.06.2006 22:45:00 ==> 20060615224500)
        start = DateTime.Parse(Beginn); // iStartTime ==> Beginn (15.06.2006 22:45:00 ==> 20060615224500)
      }
      catch (Exception ex2)
      {
        Log.Error("TVMovie: Error parsing EPG time data - {0},{1}", ex2.Message, ex2.StackTrace);
      }

      string title = Sendung;
      string shortDescription = KurzBeschreibung;
      string description;
      if (_useShortProgramDesc)
        description = shortDescription;
      else
      {
        description = Beschreibung;
        if (description.Length < shortDescription.Length)
          description = shortDescription;
      }

      string genre = Genre;
      string shortCritic = Kurzkritik;

      if (_extendDescription)
      {
        classification = FSK;
        date = Herstellungsjahr;
        episode = Originaltitel;
        director = Regie;
        actors = Darsteller;
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
          EPGStarRating = 2; break;
        case 1:
          EPGStarRating = 4; break;
        case 2:
          EPGStarRating = 6; break;
        case 3:
          EPGStarRating = 8; break;
        case 4:
          EPGStarRating = 10; break;
        case 5:
          EPGStarRating = 8; break;
        case 6:
          EPGStarRating = 10; break;
        default:
          EPGStarRating = -1; break;
      }

      if (_showAudioFormat)
      {
        bool audioDesc = Convert.ToBoolean(Audiodescription);
        bool dolbyDigital = Convert.ToBoolean(DolbyDigital);
        bool dolbySuround = Convert.ToBoolean(DolbySuround);
        bool dolby = Convert.ToBoolean(Dolby);
        bool stereo = Convert.ToBoolean(Stereo);
        bool dualAudio = Convert.ToBoolean(Zweikanalton);
        audioFormat = BuildAudioDescription(audioDesc, dolbyDigital, dolbySuround, dolby, stereo, dualAudio);
      }

      foreach (Mapping channelName in channelNames)
      {
        DateTime newStartDate = start;
        DateTime newEndDate = end;

        if (!CheckEntry(ref newStartDate, ref newEndDate, channelName.Start, channelName.End))
        {
          Channel progChannel = null;
          foreach (Channel ch in allChannels)
          {
            if (ch.Name == channelName.Channel)
            {
              progChannel = ch;
              break;
            }
          }
          DateTime OnAirDate = DateTime.MinValue;
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

          if (audioFormat == String.Empty)
            description = description.Replace("<br>", "\n");
          else
            description = "Ton: " + audioFormat + "\n" + description.Replace("<br>", "\n");

          if (_extendDescription)
          {
            StringBuilder sb = new StringBuilder();
            if (episode != String.Empty)
              sb.Append("Folge: " + episode + "\n");

            if (starRating != -1 && _showRatings)
            {
              //sb.Append("Wertung: " + string.Format("{0}/5", starRating) + "\n");
              sb.Append("Wertung: ");
              if (shortCritic.Length > 1)
                sb.Append(shortCritic + " - ");

              sb.Append(BuildRatingDescription(starRating));
              if (detailedRating.Length > 0)
                sb.Append(BuildDetailedRatingDescription(detailedRating));
            }

            if (!string.IsNullOrEmpty(description))
              sb.Append(description + "\n");

            if (director.Length > 0)
              sb.Append("Regie: " + director + "\n");
            if (actors.Length > 0)
              sb.Append(BuildActorsDescription(actors));
            if (classification != String.Empty && classification != "0")
              sb.Append("FSK: " + classification + "\n");
            if (date != String.Empty)
              sb.Append("Jahr: " + date + "\n");

            description = sb.ToString();
          }
          else
          {
            if (_showRatings)
              if (shortCritic.Length > 1)
                description = shortCritic + "\n" + description;
          }

          Program prog = new Program(progChannel.IdChannel, newStartDate, newEndDate, title, description, genre, false, OnAirDate, string.Empty, string.Empty, EPGStarRating, classification, 0);
          if (useGentlePersist)          
            prog.Persist();
          
          _tvmEpgProgs.Add(prog);
          if (_slowImport)
            Thread.Sleep(10);
        }
      }
    }

    private ArrayList GetMappingList()
    {
      IList mappingDb = TvMovieMapping.ListAll();
      ArrayList mappingList = new ArrayList();

      foreach (TvMovieMapping mapping in mappingDb)
      {
        string newStart = mapping.TimeSharingStart;
        string newEnd = mapping.TimeSharingEnd;
        string newChannel = Channel.Retrieve(mapping.IdChannel).Name;
        string newStation = mapping.StationName;

        mappingList.Add(new TvMovieDatabase.Mapping(newChannel, newStation, newStart, newEnd));
      }

      return mappingList;
    }

    private bool CheckEntry(ref DateTime progStart, ref DateTime progEnd, TimeSpan timeSharingStart, TimeSpan timeSharingEnd)
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
    private string BuildAudioDescription(bool audioDesc, bool dolbyDigital, bool dolbySurround, bool dolby, bool stereo, bool dualAudio)
    {
      string audioFormat = String.Empty;

      if (dolbyDigital)
        audioFormat = "Dolby Digital";
      if (dolbySurround)
        audioFormat = "Dolby Surround";
      if (dolby)
        audioFormat = "Dolby";
      if (stereo)
        audioFormat = "Stereo";
      if (dualAudio)
        audioFormat = "Mehrkanal-Ton";

      return audioFormat;
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
      // "Spaß=1;Action=3;Erotik=1;Spannung=3;Anspruch=0"
      int posidx = 0;
      string detailedRating = string.Empty;
      StringBuilder strb = new StringBuilder();

      if (dbDetailedRating != String.Empty)
      {
        posidx = dbDetailedRating.IndexOf(@"Spaß=");
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
        posidx = dbDetailedRating.IndexOf(@"Gefühl=");
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
      // Mit: Bernd Schramm (Buster der Hund);Sandra Schwarzhaupt (Gwendolyn die Katze);Joachim Kemmer (Tortellini der Hahn);Mario Adorf (Fred der Esel);Katharina Thalbach (die Erbin);Peer Augustinski (Dr. Gier);Klausjürgen Wussow (Der Erzähler);Hartmut Engler (Hund Buster);Bert Henry (Drehbuch);Georg Reichel (Drehbuch);Dagmar Kekule (Drehbuch);Peter Wolf (Musik);Dagmar Kekulé (Drehbuch)
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

    /// <summary>
    /// Delete ALL programs for the given channel
    /// </summary>
    /// <param name="channel">The channel name</param>
    private void ClearPrograms(string channel)
    {
      try
      {
        Channel progChannel = null;
        IList allChannels = Channel.ListAll();
        foreach (Channel ch in allChannels)
        {
          if (ch.Name == channel)
          {
            progChannel = ch;
            break;
          }
        }
        TvBLayer.RemoveAllPrograms(progChannel.IdChannel);
      }
      catch (Exception ex)
      {
        Log.Error("TvMovieDatabase: ClearPrograms failed - {0}", ex.Message);
      }
    }

    /// <summary>
    /// Launches TV Movie's own internet update tool
    /// </summary>
    /// <returns>Number of seconds needed for the update</returns>
    public long LaunchTVMUpdater()
    {
      long UpdateDuration = 0;
      string UpdaterPath = Path.Combine(TVMovieProgramPath, @"tvuptodate.exe");
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
            processes[0].WaitForExit(600000);
            BenchClock.Stop();
            UpdateDuration = (BenchClock.ElapsedMilliseconds / 1000);
            Log.Info("TVMovie: tvuptodate was already running - waited {0} seconds for internet update to finish", Convert.ToString(UpdateDuration));
            return UpdateDuration;
          }

          ProcessStartInfo startInfo = new ProcessStartInfo("tvuptodate.exe");
          //startInfo.Arguments = "";
          startInfo.FileName = UpdaterPath;
          startInfo.WindowStyle = ProcessWindowStyle.Normal;
          startInfo.WorkingDirectory = Path.GetDirectoryName(UpdaterPath);

          Process UpdateProcess = Process.Start(startInfo);
          UpdateProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
          UpdateProcess.WaitForExit(600000); // do not wait longer than 10 minutes for the internet update

          BenchClock.Stop();
          UpdateDuration = (BenchClock.ElapsedMilliseconds / 1000);
          Log.Info("TVMovie: tvuptodate finished internet update in {0} seconds", Convert.ToString(UpdateDuration));
        }
        catch (Exception ex)
        {
          BenchClock.Stop();
          UpdateDuration = (BenchClock.ElapsedMilliseconds / 1000);
          Log.Error("TVMovie: LaunchTVMUpdater failed: {0}", ex.Message);
        }
      }
      else
        Log.Info("TVMovie: tvuptodate.exe not found in default location: {0}", UpdaterPath);

      return UpdateDuration;
    }
    #endregion

  } // class
}
