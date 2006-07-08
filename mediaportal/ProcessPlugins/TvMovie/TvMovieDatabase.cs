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
using System.Data;
using System.Data.OleDb;
using MediaPortal.TV.Database;
using System.Collections;
using Microsoft.Win32;
using System.IO;
using System.Xml;
using System.Diagnostics;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Threading;
using MediaPortal.Utils.Services;

namespace ProcessPlugins.TvMovie
{
  class TvMovieDatabase
  {
    private OleDbConnection _databaseConnection = null;
    private bool _canceled = false;
    private ArrayList _stations = null;
    private ArrayList _channelList = null;
    private int _programsCounter = 0;

    private const string _xmlFile = "TVMovieMapping.xml";


    public delegate void ProgramsChanged(int value, int maximum, string text);
    public event ProgramsChanged OnProgramsChanged;
    public delegate void StationsChanged(int value, int maximum, string text);
    public event StationsChanged OnStationsChanged;
    protected ILog _log;

 
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

        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          mpPath = xmlreader.GetValueAsString("tvmovie", "databasepath", path);

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
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          mpPath = xmlreader.GetValueAsString("tvmovie", "databasepath", string.Empty);

        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          if (newPath == path)
            xmlwriter.SetValue("tvmovie", "databasepath", string.Empty);
          else
            xmlwriter.SetValue("tvmovie", "databasepath", newPath);
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


    public TvMovieDatabase()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();

      string dataProviderString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";

      _log.Info("TVMovie: DB path: {0}", DatabasePath);

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
        _log.Info("TVMovie: Error accessing TV Movie Clickfinder database while reading stations");
        _log.Info("TVMovie: Exception: {0}", ex);
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

      _channelList = new ArrayList();
      TVDatabase.GetChannels(ref _channelList);
    }


    private ArrayList GetMappingList()
    {
      if (!File.Exists(_xmlFile))
      {
        _log.Info("TVMovie: Mapping file \"{0}\" does not exist", _xmlFile);
        return null;
      }
      ArrayList mappingList = new ArrayList();

      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(_xmlFile);
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
              string newChannel = (string)channel.Attributes["name"].Value;
              string newStation = (string)station.Attributes["name"].Value;

              if (CheckChannel(newChannel) && CheckStation(newStation))
                mappingList.Add(new TvMovieDatabase.Mapping(newChannel, newStation, newStart, newEnd));
            }
          }
        }
      }
      catch (System.Xml.XmlException ex)
      {
        _log.Info("TVMovie: The mapping file \"{0}\" seems to be corrupt", _xmlFile);
        _log.Info("TVMovie: {0}", ex.Message);
        return null;
      }

      if (mappingList.Count > 0)
        return mappingList;
      else
        return null;
    }


    private bool CheckChannel(string channelName)
    {
      if (_channelList != null)
        foreach (TVChannel channel in _channelList)
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
      bool useShortProgramDesc = false;
      bool showAudioFormat = false;
      bool slowImport = false;
      string sqlSelect = string.Empty;
      string audioFormat = String.Empty;

      if (_databaseConnection == null)
        return 0;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        useShortProgramDesc = xmlreader.GetValueAsBool("tvmovie", "shortprogramdesc", false);
        showAudioFormat = xmlreader.GetValueAsBool("tvmovie", "showaudioformat", false);
        slowImport = xmlreader.GetValueAsBool("tvmovie", "slowimport", false);
      }

      if (useShortProgramDesc)
      {
        if (showAudioFormat)
          sqlSelect = string.Format("SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.KurzBeschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung, TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;", stationName);
        else
          sqlSelect = string.Format("SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.KurzBeschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;", stationName);
      }
      else
        if (showAudioFormat)
          sqlSelect = string.Format("SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Beschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung, TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;", stationName);
        else
          sqlSelect = string.Format("SELECT TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Beschreibung, TVDaten.Ende, TVDaten.Originaltitel, TVDaten.Genre, TVDaten.Wiederholung, TVDaten.Interessant, TVDaten.Beginn, TVDaten.Sendung FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ([Ende]>=Now())) ORDER BY TVDaten.Beginn;", stationName);

      OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection);
      OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand);

      DataSet tvMovieTable = new DataSet();

      try
      {
        _databaseConnection.Open();
        databaseAdapter.Fill(tvMovieTable, "TVDaten");
      }
      catch (System.Data.OleDb.OleDbException ex)
      {
        _log.Info("TVMovie: Error accessing TV Movie Clickfinder database - Current import canceled, waiting for next schedule");
        _log.Info("TVMovie: Exception: {0}", ex);
        return 0;
      }
      finally
      {
        _databaseConnection.Close();
      }

      int programsCount = tvMovieTable.Tables["TVDaten"].Rows.Count;

      if (OnProgramsChanged != null)
        OnProgramsChanged(0, programsCount + 1, string.Empty);

      int counter = 0;

      foreach (DataRow guideEntry in tvMovieTable.Tables["TVDaten"].Rows)
      {
        if (_canceled)
          break;

        string channel = stationName;                                     // idChannel (table channel) ==> Senderkennung match strChannel
        string classification = guideEntry["FSK"].ToString();             // strClassification ==> FSK
        string date = guideEntry["Herstellungsjahr"].ToString();          // strDate ==> Herstellungsjahr
        string description;
        if (useShortProgramDesc)
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

        if (showAudioFormat)
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
            TVProgram epgEntry = new TVProgram();

            epgEntry.Channel = channelName.Channel;
            epgEntry.Start = Utils.datetolong(newStartDate);
            epgEntry.End = Utils.datetolong(newEndDate);
            epgEntry.Title = title;
            if (audioFormat == String.Empty)
              epgEntry.Description = description.Replace("<br>", "\n");
            else
              epgEntry.Description = "Ton: " + audioFormat + "\n" + description.Replace("<br>", "\n");
            epgEntry.Genre = genre;
            epgEntry.Classification = classification;
            epgEntry.Date = date;
            epgEntry.Episode = episode;
            if (repeat != 0)
              epgEntry.Repeat = "Repeat";
            if (starRating != -1)
              epgEntry.StarRating = string.Format("{0}/5", starRating);

            TVDatabase.UpdateProgram(epgEntry);
            if (slowImport)
              Thread.Sleep(50);
          }
        }
      }
      if (OnProgramsChanged != null)
        OnProgramsChanged(programsCount + 1, programsCount + 1, string.Empty);
      return counter;
    }


    public bool WasUpdated
    {
      get
      {
        long lastUpdate = 0;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\TVUpdate"))
          if (rkey != null)
          {
            string regLastUpdate = string.Format("{0}", rkey.GetValue("LetztesTVUpdate"));
            lastUpdate = Convert.ToInt64(regLastUpdate.Substring(8));
          }
          else
            return false;

        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          if (Convert.ToInt64(xmlreader.GetValueAsString("tvmovie", "lastupdate", "0")) == lastUpdate)
            return false;

        return true;
      }
    }


    public void Import()
    {
      ArrayList mappingList = GetMappingList();

      if (mappingList == null)
      {
        _log.Info("TVMovie: Cannot import from TV Movie database");
        return;
      }

      _log.Info("TVMovie: Importing database");

      TVDatabase.RemoveOldPrograms();

      int maximum = 0;

      foreach (string station in _stations)
        foreach (Mapping mapping in mappingList)
          if (mapping.Station == station)
          {
            maximum++;
            break;
          }
      if (OnStationsChanged != null)
        OnStationsChanged(1, maximum, string.Empty);

      int counter = 0;

      foreach (string station in _stations)
      {
        if (_canceled)
          break;

        ArrayList channelNames = new ArrayList();

        foreach (Mapping mapping in mappingList)
          if (mapping.Station == station)
            channelNames.Add(mapping);

        if (channelNames.Count > 0)
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
      }
      if (OnStationsChanged != null)
        OnStationsChanged(maximum, maximum, "Import done");

      if (!_canceled)
      {
        long lastUpdate = 0;

        using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EWE\\TVGhost\\TVUpdate"))
          if (rkey != null)
          {
            string regLastUpdate = string.Format("{0}", rkey.GetValue("LetztesTVUpdate"));
            lastUpdate = Convert.ToInt64(regLastUpdate.Substring(8));
          }

        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          xmlwriter.SetValue("tvmovie", "lastupdate", lastUpdate);

        MediaPortal.Profile.Settings.SaveCache();
      }

      _log.Info("TVMovie: Imported {0} database entries for {1} stations", _programsCounter, counter);
    }
  }

}
