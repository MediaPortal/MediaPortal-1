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
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;
using MediaPortal.Util;
using Microsoft.Win32;

namespace ProcessPlugins.TvMovie
{
  internal class TvMovieDatabase
  {
    private static OleDbConnection _databaseConnection = null;
    private static bool _canceled = false;
    private static ArrayList _stationsList = null;
    private static ArrayList _channelList = null;
    private static ArrayList _mappingList = null;
    private int _programsCounter = 0;
    private bool _useShortProgramDesc = false;
    private bool _extendDescription = false;
    private bool _showAudioFormat = false;
    private bool _slowImport = false;

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
        {
          hours = Convert.ToInt16(input.Substring(0, index));
        }
        if (index + 1 < input.Length)
        {
          minutes = Convert.ToInt16(input.Substring(index + 1));
        }

        if (hours > 23)
        {
          hours = 0;
        }

        if (minutes > 59)
        {
          minutes = 0;
        }

        return new TimeSpan(hours, minutes, 0);
      }
    }


    public ArrayList Stations
    {
      get { return GetStationsList(); }
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
        {
          if (rkey != null)
          {
            path = string.Format("{0}", rkey.GetValue("DBDatei"));
          }
        }

        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          mpPath = xmlreader.GetValueAsString("tvmovie", "databasepath", path);
        }

        if (File.Exists(mpPath))
        {
          return mpPath;
        }

        return path;
      }
      set
      {
        string path = string.Empty;

        string newPath = value;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
        {
          if (rkey != null)
          {
            path = string.Format("{0}", rkey.GetValue("DBDatei"));
          }
        }

        if (!File.Exists(newPath))
        {
          newPath = path;
        }

        string mpPath = string.Empty;
        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          mpPath = xmlreader.GetValueAsString("tvmovie", "databasepath", string.Empty);
        }

        using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          if (newPath == path)
          {
            xmlwriter.SetValue("tvmovie", "databasepath", string.Empty);
          }
          else
          {
            xmlwriter.SetValue("tvmovie", "databasepath", newPath);
          }
        }
      }
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
        {
          return;
        }
      }

      updateProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

      updateProcess.Start();
      updateProcess.WaitForExit();
    }


    private void UpdateTvMovie()
    {
      Process updateProcess = new Process();

      using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\Gemeinsames"))
      {
        if (rkey != null)
        {
          updateProcess.StartInfo.FileName = string.Format("{0}\\tvupdate.exe", rkey.GetValue("ProgrammPath"));
        }
        else
        {
          return;
        }
      }

      updateProcess.Start();
      updateProcess.WaitForExit();
    }


    public TvMovieDatabase()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _useShortProgramDesc = xmlreader.GetValueAsBool("tvmovie", "shortprogramdesc", false);
        _extendDescription = xmlreader.GetValueAsBool("tvmovie", "extenddescription", false);
        _showAudioFormat = xmlreader.GetValueAsBool("tvmovie", "showaudioformat", false);
        _slowImport = xmlreader.GetValueAsBool("tvmovie", "slowimport", false);
      }
    }


    private static ArrayList GetStationsList()
    {
      if (_stationsList == null)
      {
        string dataProviderString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";

        //Log.Debug("TVMovie: DB path: {0}", DatabasePath);

        if (DatabasePath != string.Empty)
        {
          dataProviderString = string.Format(dataProviderString, DatabasePath);
        }
        else
        {
          return null;
        }

        _databaseConnection = new OleDbConnection(dataProviderString);

        string sqlSelect =
          "SELECT Sender.SenderKennung FROM Sender WHERE (((Sender.Favorit)=-1)) ORDER BY Sender.SenderKennung DESC;";

        OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection);
        OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand);
        DataSet tvMovieTable = new DataSet();

        try
        {
          _databaseConnection.Open();
          databaseAdapter.Fill(tvMovieTable, "Sender");
        }
        catch (OleDbException ex)
        {
          Log.Error("TVMovie: Error accessing TV Movie ClickFinder database while reading stations");
          Log.Error("TVMovie: Exception: {0}", ex);
          _canceled = true;
          return null;
        }
        finally
        {
          _databaseConnection.Close();
        }

        _stationsList = new ArrayList();
        foreach (DataRow sender in tvMovieTable.Tables["Sender"].Rows)
        {
          _stationsList.Add(sender["Senderkennung"]);
        }
      }
      return _stationsList;
    }


    private static ArrayList GetChannelList()
    {
      if (_channelList == null)
      {
        _channelList = new ArrayList();
        TVDatabase.GetChannels(ref _channelList);
      }
      return _channelList;
    }


    private static ArrayList GetMappingList()
    {
      if (_mappingList == null)
      {
        string xmlFile = Config.GetFile(Config.Dir.Config, "TVMovieMapping.xml");
        if (!File.Exists(xmlFile))
        {
          Log.Error("TVMovie: Mapping file \"{0}\" does not exist", xmlFile);
          return null;
        }
        _mappingList = new ArrayList();

        try
        {
          XmlDocument doc = new XmlDocument();
          doc.Load(xmlFile);
          XmlNodeList listChannels = doc.DocumentElement.SelectNodes("/channellist/channel");
          foreach (XmlNode channel in listChannels)
          {
            XmlNodeList listStations = channel.SelectNodes("station");
            foreach (XmlNode station in listStations)
            {
              if (station != null)
              {
                XmlNode timesharing = station.SelectSingleNode("timesharing");
                string newStart = timesharing.Attributes["start"].Value;
                string newEnd = timesharing.Attributes["end"].Value;
                string newChannel = (string) channel.Attributes["name"].Value;
                string newStation = (string) station.Attributes["name"].Value;
                int newChannelId = TVDatabase.GetChannelId(newChannel);

                if (CheckChannel(newChannel) && CheckStation(newStation))
                {
                  _mappingList.Add(new Mapping(newChannel, newStation, newStart, newEnd));
                }
              }
            }
          }
        }
        catch (XmlException ex)
        {
          Log.Error("TVMovie: The mapping file \"{0}\" seems to be corrupt", xmlFile);
          Log.Error("TVMovie: {0}", ex.Message);
          return null;
        }
        catch (Exception)
        {
          Log.Error("EX");
        }
      }

      if (_mappingList.Count > 0)
      {
        return _mappingList;
      }
      else
      {
        return null;
      }
    }


    public static string GetChannelName(string stationName)
    {
      ArrayList mappingList = GetMappingList();

      foreach (Mapping mapping in mappingList)
      {
        if (mapping.Station == stationName)
        {
          return mapping.Channel;
        }
      }
      return string.Empty;
    }


    private static bool CheckChannel(string channelName)
    {
      ArrayList channelList = GetChannelList();

      if (channelList != null)
      {
        foreach (TVChannel channel in channelList)
        {
          if (channel.Name == channelName)
          {
            return true;
          }
        }
      }

      return false;
    }


    private static bool CheckStation(string stationName)
    {
      ArrayList stationsList = GetStationsList();

      if (stationsList != null)
      {
        foreach (string station in stationsList)
        {
          if (station == stationName)
          {
            return true;
          }
        }
      }

      return false;
    }


    private bool CheckEntry(ref DateTime progStart, ref DateTime progEnd, TimeSpan timeSharingStart,
                            TimeSpan timeSharingEnd)
    {
      if (timeSharingStart == timeSharingEnd)
      {
        return false;
      }

      DateTime stationStart = progStart.Date + timeSharingStart;
      DateTime stationEnd = progStart.Date + timeSharingEnd;

      if (stationStart > progStart && progEnd <= stationStart)
      {
        stationStart = stationStart.AddDays(-1);
      }
      else if (timeSharingEnd < timeSharingStart)
      {
        stationEnd = stationEnd.AddDays(1);
      }

      if (progStart >= stationStart && progStart < stationEnd && progEnd > stationEnd)
      {
        progEnd = stationEnd;
      }

      if (progStart <= stationStart && progEnd > stationStart && progEnd < stationEnd)
      {
        progStart = stationStart;
      }

      if ((progEnd <= stationEnd) && (progStart >= stationStart))
      {
        return false;
      }

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
    private string BuildAudioDescription(bool audioDesc, bool dolbyDigital, bool dolbySurround, bool dolby, bool stereo,
                                         bool dualAudio)
    {
      string audioFormat = string.Empty;

      if (dolbyDigital)
      {
        audioFormat = "Dolby Digital";
      }
      if (dolbySurround)
      {
        audioFormat = "Dolby Surround";
      }
      if (dolby)
      {
        audioFormat = "Dolby 2.0";
      }
      if (stereo)
      {
        audioFormat = "Stereo";
      }
      if (dualAudio)
      {
        audioFormat = "Mehrkanal-Ton";
      }

      return audioFormat;
    }


    private int ImportStation(string stationName, ArrayList channelNames)
    {
      //Log.Debug("TVMovie: ImportStation({0})", stationName);

      string sqlSelect = string.Empty;
      string audioFormat = string.Empty;

      if (_databaseConnection == null)
      {
        return 0;
      }

      if (_useShortProgramDesc)
      {
        if (_showAudioFormat)
        {
          sqlSelect =
            string.Format(
              "SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.KurzBeschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung, TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;",
              stationName);
        }
        else
        {
          sqlSelect =
            string.Format(
              "SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.KurzBeschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;",
              stationName);
        }
      }
      else if (_showAudioFormat)
      {
        sqlSelect =
          string.Format(
            "SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Beschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung, TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;",
            stationName);
      }
      else
      {
        sqlSelect =
          string.Format(
            "SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Beschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;",
            stationName);
      }

      OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection);
      OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand);

      DataSet tvMovieTable = new DataSet();

      //Log.Debug("TVMovie: Getting data for station");

      try
      {
        _databaseConnection.Open();
        databaseAdapter.Fill(tvMovieTable, "TVDaten");
      }
      catch (OleDbException ex)
      {
        Log.Error(
          "TVMovie: Error accessing TV Movie Clickfinder database - Current import canceled, waiting for next schedule");
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
      {
        OnProgramsChanged(0, programsCount + 1, string.Empty);
      }

      int counter = 0;

      //Log.Debug("TVMovie: Importing data for station");

      foreach (DataRow guideEntry in tvMovieTable.Tables["TVDaten"].Rows)
      {
        if (_canceled)
        {
          break;
        }

        //string channel = stationName;                                     // idChannel (table channel) ==> Senderkennung match strChannel
        string classification = guideEntry["FSK"].ToString(); // strClassification ==> FSK
        string date = guideEntry["Herstellungsjahr"].ToString(); // strDate ==> Herstellungsjahr
        string description;
        if (_useShortProgramDesc)
        {
          description = guideEntry["KurzBeschreibung"].ToString();
        }
        else
        {
          description = guideEntry["Beschreibung"].ToString(); // strDescription ==> Beschreibung
        }
        DateTime end = DateTime.Parse(guideEntry["Ende"].ToString());
          // iEndTime ==> Ende  (15.06.2006 22:45:00 ==> 20060615224500)
        string episode = guideEntry["Originaltitel"].ToString(); // strEpisodeName ==> Originaltitel
        //string episodeNum;                                              // strEpisodeNum ==> "unknown"
        //string episodePart;                                             // strEpisodePart ==> "unknown"
        string genre = guideEntry["Genre"].ToString(); // idGenre (table genre) Genre match strGenre
        int repeat = Convert.ToInt16(guideEntry["Wiederholung"]); // strRepeat ==> Wiederholung "Repeat" / "unknown"
        //string seriesNum;                                               // strSeriesNum ==> "unknown"
        int starRating = Convert.ToInt16(guideEntry["Interessant"]) - 1; // strStarRating ==> Interessant + "/5"
        DateTime start = DateTime.Parse(guideEntry["Beginn"].ToString());
          // iStartTime ==> Beginn (15.06.2006 22:45:00 ==> 20060615224500)
        string title = guideEntry["Sendung"].ToString(); // strTitle ==> Sendung

        if (_showAudioFormat)
        {
          bool audioDesc = Convert.ToBoolean(guideEntry["Audiodescription"]); // strAudioDesc ==> Tonformat "Stereo"
          bool dolbyDigital = Convert.ToBoolean(guideEntry["DolbyDigital"]);
          bool dolbySuround = Convert.ToBoolean(guideEntry["DolbySuround"]);
          bool dolby = Convert.ToBoolean(guideEntry["Dolby"]);
          bool stereo = Convert.ToBoolean(guideEntry["Stereo"]);
          bool dualAudio = Convert.ToBoolean(guideEntry["Zweikanalton"]);
          audioFormat = BuildAudioDescription(audioDesc, dolbyDigital, dolbySuround, dolby, stereo, dualAudio);
        }

        if (OnProgramsChanged != null)
        {
          OnProgramsChanged(counter, programsCount + 1, title);
        }

        counter++;

        foreach (Mapping channelName in channelNames)
        {
          DateTime newStartDate = start;
          DateTime newEndDate = end;

          if (!CheckEntry(ref newStartDate, ref newEndDate, channelName.Start, channelName.End))
          {
            TVProgram epgEntry = new TVProgram();

            epgEntry.Channel = channelName.Channel;
            epgEntry.Start = Utils.datetolong(newStartDate);
            epgEntry.End = Utils.datetolong(newEndDate);
            epgEntry.Title = title;
            if (audioFormat == string.Empty)
            {
              epgEntry.Description = description.Replace("<br>", "\n");
            }
            else
            {
              epgEntry.Description = "Ton: " + audioFormat + "\n" + description.Replace("<br>", "\n");
            }
            epgEntry.Genre = genre;
            epgEntry.Classification = classification;
            epgEntry.Date = date;
            epgEntry.Episode = episode;
            if (repeat != 0)
            {
              epgEntry.Repeat = "Repeat";
            }
            if (starRating != -1)
            {
              epgEntry.StarRating = string.Format("{0}/5", starRating);
            }

            if (_extendDescription)
            {
              StringBuilder sb = new StringBuilder();

              if (epgEntry.Episode != string.Empty)
              {
                sb.Append("Folge: " + epgEntry.Episode + "\n");
              }
              if (starRating != -1)
              {
                sb.Append("Wertung: " + string.Format("{0}/5", starRating) + "\n");
              }
              sb.Append(epgEntry.Description + "\n");
              if (epgEntry.Classification != string.Empty && epgEntry.Classification != "0")
              {
                sb.Append("FSK: " + epgEntry.Classification + "\n");
              }
              if (epgEntry.Date != string.Empty)
              {
                sb.Append("Jahr: " + epgEntry.Date + "\n");
              }

              epgEntry.Description = sb.ToString();
            }

            TVDatabase.SupressEvents = true; // Bav - testing if this is root of powerscheduler problems
            TVDatabase.UpdateProgram(epgEntry);
            if (_slowImport)
            {
              Thread.Sleep(50);
            }
          }
        }
      }

      //Log.Debug("TVMovie: Importing data for station done");

      if (OnProgramsChanged != null)
      {
        OnProgramsChanged(programsCount + 1, programsCount + 1, string.Empty);
      }
      return counter;
    }


    public bool WasUpdated
    {
      get
      {
        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          if (Convert.ToInt64(xmlreader.GetValueAsString("tvmovie", "lastupdate", "0")) == LastUpdate)
          {
            return false;
          }
        }

        return true;
      }
    }

    private long LastUpdate
    {
      get
      {
        long lastUpdate = 0;

        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          if (xmlreader.GetValueAsBool("tvmovie", "usedatabasedate", true))
          {
            FileInfo mpFi = new FileInfo(DatabasePath);
            DateTime dbUpdate = mpFi.LastWriteTime;
            lastUpdate =
              Convert.ToInt64(string.Format("{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}", dbUpdate.Year, dbUpdate.Month,
                                            dbUpdate.Day, dbUpdate.Hour, dbUpdate.Minute, dbUpdate.Second));
          }
          else
          {
            using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\TVUpdate"))
            {
              if (rkey != null)
              {
                string regLastUpdate = string.Format("{0}", rkey.GetValue("LetztesTVUpdate"));
                lastUpdate = Convert.ToInt64(regLastUpdate.Substring(8));
              }
            }
          }
        }

        return lastUpdate;
      }
    }

    public void Import()
    {
      if (_canceled)
      {
        return;
      }

      ArrayList mappingList = GetMappingList();

      if (mappingList == null)
      {
        Log.Error("TVMovie: Cannot import from TV Movie database");
        return;
      }

      Log.Debug("TVMovie: Importing database");

      //Log.Debug("TVMovie: Removal of old EPG data");
      TVDatabase.RemoveOldPrograms();
      //Log.Debug("TVMovie: Removal done");

      if (_canceled)
      {
        return;
      }

      int maximum = 0;

      ArrayList stationsList = GetStationsList();

      //Log.Debug("TVMovie: Calculating stations");
      foreach (string station in stationsList)
      {
        foreach (Mapping mapping in mappingList)
        {
          if (mapping.Station == station)
          {
            maximum++;
            break;
          }
        }
      }
      if (OnStationsChanged != null)
      {
        OnStationsChanged(1, maximum, string.Empty);
      }
      //Log.Debug("TVMovie: Calculating stations done");

      int counter = 0;

      //Log.Debug("TVMovie: Importing stations");

      foreach (string station in stationsList)
      {
        if (_canceled)
        {
          return;
        }

        ArrayList channelNames = new ArrayList();

        foreach (Mapping mapping in mappingList)
        {
          if (mapping.Station == station)
          {
            channelNames.Add(mapping);
          }
        }

        if (channelNames.Count > 0)
        {
          string display = string.Empty;
          foreach (Mapping channelName in channelNames)
          {
            display += string.Format("{0}  /  ", channelName.Channel);
          }

          display = display.Substring(0, display.Length - 5);
          if (OnStationsChanged != null)
          {
            OnStationsChanged(counter, maximum, display);
          }
          counter++;
          _programsCounter += ImportStation(station, channelNames);
        }
      }
      if (OnStationsChanged != null)
      {
        OnStationsChanged(maximum, maximum, "Import done");
      }

      //Log.Debug("TVMovie: Importing stations done");

      //Log.Debug("TVMovie: Setting last update time stamp");

      if (!_canceled)
      {
        using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValue("tvmovie", "lastupdate", LastUpdate);
        }

        Settings.SaveCache();

        //Log.Debug("TVMovie: Setting last update time stamp done");

        Log.Debug("TVMovie: Imported {0} database entries for {1} stations", _programsCounter, counter);
      }
    }
  }
}