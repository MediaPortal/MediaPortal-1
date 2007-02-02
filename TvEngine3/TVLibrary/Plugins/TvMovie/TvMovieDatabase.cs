#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.Win32;
using TvDatabase;
using TvLibrary.Log;
using Gentle.Common;
using Gentle.Framework;

namespace TvEngine
{
  class TvMovieDatabase
  {
    private OleDbConnection _databaseConnection = null;
    private bool _canceled = false;
    private ArrayList _stations = null;
    private ArrayList _channelList = null;
    private int _programsCounter = 0;
    private bool _useShortProgramDesc = false;
    private bool _extendDescription = false;
    private bool _showAudioFormat = false;
    private bool _slowImport = false;

    static string _xmlFile;


    public delegate void ProgramsChanged(int value, int maximum, string text);
    public event ProgramsChanged OnProgramsChanged;
    public delegate void StationsChanged(int value, int maximum, string text);
    public event StationsChanged OnStationsChanged;


    private struct Mapping
    {
      private string _channel;
      private string _station;
      private TimeSpan _start;
      private TimeSpan _end;


      public Mapping(string channel, string station, string start, string end)
      {
        _channel = channel;
        _station = station;
        _start = CleanInput(start);
        _end = CleanInput(end);
      }


      public string Channel
      {
        get { return _channel; }
      }


      public string Station
      {
        get { return _station; }
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
    }


    public ArrayList Stations
    {
      get { return _stations; }
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


    public static string DatabasePath
    {
      get
      {
        string path = string.Empty;
        string mpPath = string.Empty;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
          if (rkey != null)
            path = string.Format("{0}", rkey.GetValue("DBDatei"));

        TvBusinessLayer layer = new TvBusinessLayer();
        mpPath = layer.GetSetting("TvMoviedatabasepath", path).Value;

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

        string mpPath = string.Empty;
        TvBusinessLayer layer = new TvBusinessLayer();

        mpPath = layer.GetSetting("TvMoviedatabasepath", string.Empty).Value;
        Setting setting = layer.GetSetting("TvMovieEnabled");

        if (newPath == path)
          setting.Value = string.Empty;
        else
          setting.Value = newPath;

        setting.Persist();
      }
    }


    public ArrayList GetChannels()
    {
      ArrayList tvChannels = new ArrayList();
      IList allChannels = Channel.ListAll();
      foreach (Channel channel in allChannels)
      {
        if (channel.IsTv)
          tvChannels.Add(channel);
      }
      return tvChannels;
    }


    public static void ReorganizeTvMovie()
    {
      Process updateProcess = new Process();

      using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
      {
        if (rkey != null)
        {
          updateProcess.StartInfo.FileName = string.Format("{0}\\comptvdb.exe", rkey.GetValue("ProgrammPath"));
          updateProcess.StartInfo.Arguments = DatabasePath;
        }
        else
          return;
      }

      updateProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

      updateProcess.Start();
      updateProcess.WaitForExit();
    }


    private void UpdateTvMovie()
    {
      Process updateProcess = new Process();

      using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
        if (rkey != null)
          updateProcess.StartInfo.FileName = string.Format("{0}\\tvupdate.exe", rkey.GetValue("ProgrammPath"));
        else
          return;

      updateProcess.Start();
      updateProcess.WaitForExit();
    }


    public void Connect()
    {
      TvBusinessLayer layer = new TvBusinessLayer();

      _useShortProgramDesc = layer.GetSetting("TvMovieShortProgramDesc", "false").Value == "true";
      _extendDescription = layer.GetSetting("TvMovieExtendDescription", "false").Value == "true";
      _showAudioFormat = layer.GetSetting("TvMovieShowAudioFormat", "false").Value == "true";
      _slowImport = layer.GetSetting("TvMovieSlowImport", "false").Value == "true";

      _xmlFile = String.Format(@"{0}\MediaPortal TV Server\TVMovieMapping.xml", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

      string dataProviderString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";

      //Log.Debug("TVMovie: DB path: {0}", DatabasePath);

      if (DatabasePath != string.Empty)
        dataProviderString = string.Format(dataProviderString, DatabasePath);
      else
        return;

      _databaseConnection = new OleDbConnection(dataProviderString);

      string sqlSelect = "SELECT Sender.SenderKennung FROM Sender WHERE (((Sender.Favorit)=-1)) ORDER BY Sender.SenderKennung DESC;";

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

      _stations = new ArrayList();
      foreach (DataRow sender in tvMovieTable.Tables["Sender"].Rows)
        _stations.Add(sender["Senderkennung"]);

      _channelList = GetChannels();
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


    private bool CheckChannel(string channelName)
    {
      if (_channelList != null)
        foreach (Channel channel in _channelList)
          if (channel.Name == channelName)
            return true;

      return false;
    }


    private bool CheckStation(string stationName)
    {
      if (_stations != null)
        foreach (string station in _stations)
          if (station == stationName)
            return true;

      return false;
    }


    bool CheckEntry(ref DateTime progStart, ref DateTime progEnd, TimeSpan timeSharingStart, TimeSpan timeSharingEnd)
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
    /// <returns></returns>
    private string BuildAudioDescription(bool audioDesc, bool dolbyDigital, bool dolbySurround, bool dolby, bool stereo, bool dualAudio)
    {
      string audioFormat = String.Empty;

      if (dolbyDigital)
        audioFormat = "Dolby Digital";
      if (dolbySurround)
        audioFormat = "Dolby Surround";
      if (dolby)
        audioFormat = "Dolby 2.0";
      if (stereo)
        audioFormat = "Stereo";
      if (dualAudio)
        audioFormat = "Mehrkanal-Ton";

      return audioFormat;
    }


    private int ImportStation(string stationName, ArrayList channelNames)
    {
      //Log.Debug("TVMovie: ImportStation({0})", stationName);

      string sqlSelect = string.Empty;
      string audioFormat = String.Empty;

      if (_databaseConnection == null)
        return 0;

      if (_useShortProgramDesc)
      {
        if (_showAudioFormat)
          sqlSelect = string.Format("SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.KurzBeschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung, TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;", stationName);
        else
          sqlSelect = string.Format("SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.KurzBeschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;", stationName);
      }
      else
        if (_showAudioFormat)
          sqlSelect = string.Format("SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Beschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung, TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;", stationName);
        else
          sqlSelect = string.Format("SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Beschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;", stationName);

      OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection);
      OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand);

      DataSet tvMovieTable = new DataSet();

      //Log.Debug("TVMovie: Getting data for station");

      try
      {
        _databaseConnection.Open();
        databaseAdapter.Fill(tvMovieTable, "TVDaten");
      }
      catch (System.Data.OleDb.OleDbException ex)
      {
        Log.Error("TVMovie: Error accessing TV Movie Clickfinder database - Current import canceled, waiting for next schedule");
        Log.Error("TVMovie: Exception: {0}", ex);
        return 0;
      }
      finally
      {
        _databaseConnection.Close();
      }

      //Log.Debug("TVMovie: Getting data for station done");

      int programsCount = tvMovieTable.Tables["TVDaten"].Rows.Count;

      if (OnProgramsChanged != null)
        OnProgramsChanged(0, programsCount + 1, string.Empty);

      int counter = 0;

      //Log.Debug("TVMovie: Importing data for station");

      foreach (DataRow guideEntry in tvMovieTable.Tables["TVDaten"].Rows)
      {
        if (_canceled)
          break;

        string channel = stationName;                                     // idChannel (table channel) ==> Senderkennung match strChannel
        string classification = guideEntry["FSK"].ToString();             // strClassification ==> FSK
        string date = guideEntry["Herstellungsjahr"].ToString();          // strDate ==> Herstellungsjahr
        string description;
        if (_useShortProgramDesc)
          description = guideEntry["KurzBeschreibung"].ToString();
        else
          description = guideEntry["Beschreibung"].ToString();            // strDescription ==> Beschreibung
        DateTime end = DateTime.Parse(guideEntry["Ende"].ToString());     // iEndTime ==> Ende  (15.06.2006 22:45:00 ==> 20060615224500)
        string episode = guideEntry["Originaltitel"].ToString();          // strEpisodeName ==> Originaltitel
        //string episodeNum;                                              // strEpisodeNum ==> "unknown"
        //string episodePart;                                             // strEpisodePart ==> "unknown"
        string genre = guideEntry["Genre"].ToString();                    // idGenre (table genre) Genre match strGenre
        int repeat = Convert.ToInt16(guideEntry["Wiederholung"]);         // strRepeat ==> Wiederholung "Repeat" / "unknown"
        //string seriesNum;                                               // strSeriesNum ==> "unknown"
        int starRating = Convert.ToInt16(guideEntry["Interessant"]) - 1;  // strStarRating ==> Interessant + "/5"
        DateTime start = DateTime.Parse(guideEntry["Beginn"].ToString()); // iStartTime ==> Beginn (15.06.2006 22:45:00 ==> 20060615224500)
        string title = guideEntry["Sendung"].ToString();                  // strTitle ==> Sendung

        if (_showAudioFormat)
        {
          bool audioDesc = Convert.ToBoolean(guideEntry["Audiodescription"]);     // strAudioDesc ==> Tonformat "Stereo"
          bool dolbyDigital = Convert.ToBoolean(guideEntry["DolbyDigital"]);
          bool dolbySuround = Convert.ToBoolean(guideEntry["DolbySuround"]);
          bool dolby = Convert.ToBoolean(guideEntry["Dolby"]);
          bool stereo = Convert.ToBoolean(guideEntry["Stereo"]);
          bool dualAudio = Convert.ToBoolean(guideEntry["Zweikanalton"]);
          audioFormat = BuildAudioDescription(audioDesc, dolbyDigital, dolbySuround, dolby, stereo, dualAudio);
        }

        if (OnProgramsChanged != null)
          OnProgramsChanged(counter, programsCount + 1, title);

        counter++;

        foreach (Mapping channelName in channelNames)
        {
          DateTime newStartDate = start;
          DateTime newEndDate = end;

          if (!CheckEntry(ref newStartDate, ref newEndDate, channelName.Start, channelName.End))
          {
            Channel progChannel = null;
            IList allChannels = Channel.ListAll();
            foreach (Channel ch in allChannels)
            {
              if (ch.Name == channelName.Channel)
              {
                progChannel = ch;
                break;
              }
            }
            Program prog = new Program(progChannel.IdChannel, newStartDate, newEndDate, title, description, genre, false);

            if (audioFormat == String.Empty)
              prog.Description = description.Replace("<br>", "\n");
            else
              prog.Description = "Ton: " + audioFormat + "\n" + description.Replace("<br>", "\n");

            //prog.Classification = classification;
            //prog.Date = date;
            //prog.Episode = episode;
            //if (repeat != 0)
            //  prog.Repeat = "Repeat";
            //if (starRating != -1)
            //  prog.StarRating = string.Format("{0}/5", starRating);

            if (_extendDescription)
            {
              StringBuilder sb = new StringBuilder();

              if (episode != String.Empty)
                sb.Append("Folge: " + episode + "\n");
              if (starRating != -1)
                sb.Append("Wertung: " + string.Format("{0}/5", starRating) + "\n");
              sb.Append(prog.Description + "\n");
              if (classification != String.Empty && classification != "0")
                sb.Append("FSK: " + classification + "\n");
              if (date != String.Empty)
                sb.Append("Jahr: " + date + "\n");

              prog.Description = sb.ToString();
            }

            //TVDatabase.SupressEvents = true;   // Bav - testing if this is root of powerscheduler problems
            prog.Persist();
            if (_slowImport)
              Thread.Sleep(50);
          }
        }
      }

      //Log.Debug("TVMovie: Importing data for station done");

      if (OnProgramsChanged != null)
        OnProgramsChanged(programsCount + 1, programsCount + 1, string.Empty);
      return counter;
    }


    public bool WasUpdated
    {
      get
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        if (Convert.ToInt64(layer.GetSetting("TvMovieLastUpdate", "0").Value) == LastUpdate)
          return false;

        return true;
      }
    }

    private long LastUpdate
    {
      get
      {
        long lastUpdate = 0;

        TvBusinessLayer layer = new TvBusinessLayer();
        if (layer.GetSetting("TvMovieUseDatabaseDate", "false").Value == "true")
          {
            FileInfo mpFi = new FileInfo(DatabasePath);
            DateTime dbUpdate = mpFi.LastWriteTime;
            lastUpdate = Convert.ToInt64(string.Format("{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}", dbUpdate.Year, dbUpdate.Month, dbUpdate.Day, dbUpdate.Hour, dbUpdate.Minute, dbUpdate.Second));
          }
          else
          {
            using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\TVUpdate"))
              if (rkey != null)
              {
                string regLastUpdate = string.Format("{0}", rkey.GetValue("LetztesTVUpdate"));
                lastUpdate = Convert.ToInt64(regLastUpdate.Substring(8));
              }
          }

        return lastUpdate;
      }
    }

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

      Log.Debug("TVMovie: Importing database");

      //Log.Debug("TVMovie: Removal of old EPG data");
      TvBusinessLayer layer = new TvBusinessLayer();
      //layer.RemoveOldPrograms();
      //Log.Debug("TVMovie: Removal done");

      if (_canceled)
        return;

      int maximum = 0;

      //Log.Debug("TVMovie: Calculating stations");
      foreach (string station in _stations)
        foreach (Mapping mapping in mappingList)
          if (mapping.Station == station)
          {
            maximum++;
            break;
          }

      if (OnStationsChanged != null)
        OnStationsChanged(1, maximum, string.Empty);
      Log.Debug("TVMovie: Calculating stations done");

      ArrayList channelList = new ArrayList();
      foreach (Mapping mapping in mappingList)
        if (channelList.IndexOf(mapping.Channel) == -1)
        {
          channelList.Add(mapping.Channel);
          Log.Debug("TVMovie: adding channel {0} - ClearPrograms", mapping.Channel);
          ClearPrograms(mapping.Channel);
        }

      Log.Debug("TVMovie: Mapped {0} stations for EPG import", Convert.ToString(maximum));

      int counter = 0;

      //Log.Debug("TVMovie: Importing stations");

      foreach (string station in _stations)
      {
        if (_canceled)
          return;

        ArrayList channelNames = new ArrayList();

        foreach (Mapping mapping in mappingList)
          if (mapping.Station == station)
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
            _programsCounter += ImportStation(station, channelNames);
          }
          catch (Exception ex)
          {
            Log.Error("TVMovie: Error importing EPG - {0},{1}", ex.Message, ex.StackTrace);
          }
        }
      }
      if (OnStationsChanged != null)
        OnStationsChanged(maximum, maximum, "Import done");

      //Log.Debug("TVMovie: Importing stations done");

      //Log.Debug("TVMovie: Setting last update time stamp");

      if (!_canceled)
      {
        long lastUpdate = 0;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\TVUpdate"))
          if (rkey != null)
          {
            string regLastUpdate = string.Format("{0}", rkey.GetValue("LetztesTVUpdate"));
            lastUpdate = Convert.ToInt64(regLastUpdate.Substring(8));
          }

        Setting setting = layer.GetSetting("TvMovieLastUpdate");
        setting.Value = lastUpdate.ToString();
        setting.Persist();

        //Log.Debug("TVMovie: Setting last update time stamp done");

        Log.Debug("TVMovie: Imported {0} database entries for {1} stations", _programsCounter, counter);
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

    void ClearPrograms(string channel)
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

      SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Delete, typeof(Program));
      sb.AddConstraint(String.Format("idChannel = '{0}'", progChannel.IdChannel));
      SqlStatement stmt = sb.GetStatement(true);
      ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
    }
  }

}
