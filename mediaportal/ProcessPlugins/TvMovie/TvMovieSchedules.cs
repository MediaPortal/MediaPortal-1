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
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Threading;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace ProcessPlugins.TvMovie
{
  internal class TvMovieSchedules
  {
    private FileSystemWatcher _watcher = null;
    private OleDbConnection _databaseConnection = null;


    public void Start()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        if (!xmlreader.GetValueAsBool("tvmovie", "importschedules", true))
        {
          return;
        }
      }

      Log.Debug("TVMovie: Starting schedules-importer");

      string path = TvMovieDatabase.DatabasePath;
      string dbDirectory = path.Substring(0, path.LastIndexOf(@"\"));
      string dbFile = path.Substring(path.LastIndexOf(@"\") + 1);

      _watcher_Changed(null, null); // run on startup

      if (_watcher == null)
      {
        _watcher = new FileSystemWatcher(dbDirectory, dbFile);
      }

      _watcher.Changed += new FileSystemEventHandler(_watcher_Changed);
      _watcher.EnableRaisingEvents = true;
    }


    public void Stop()
    {
      Log.Debug("TVMovie: Stopping schedules-importer");

      if (_watcher != null)
      {
        _watcher.EnableRaisingEvents = false;
        _watcher.Changed -= new FileSystemEventHandler(_watcher_Changed);
        _watcher.Dispose();
      }
    }


    private void _watcher_Changed(object sender, FileSystemEventArgs e)
    {
      Log.Debug("TVMovie: Database file has been modified - checking for new schedules");

      string dataProviderString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}";

      if (TvMovieDatabase.DatabasePath != string.Empty)
      {
        dataProviderString = string.Format(dataProviderString, TvMovieDatabase.DatabasePath);
      }
      else
      {
        return;
      }

      _databaseConnection = new OleDbConnection(dataProviderString);

      string sqlSelect =
        "SELECT Markierungen.ID, Markierungen.Sendung, Markierungen.SenderKennung, Markierungen.Datum, Markierungen.Uhrzeit, Markierungen.Dauer FROM Markierungen WHERE (((Markierungen.Aufzeichnung)=True));";

      OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, _databaseConnection);
      OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand);
      DataSet tvMovieTable = new DataSet();

      bool success = false;
      do
      {
        try
        {
          _databaseConnection.Open();
          databaseAdapter.Fill(tvMovieTable, "Markierungen");
          success = true;
        }
        catch (OleDbException)
        {
          Log.Debug("TVMovie: Database is locked - retrying in 200 msec");
          Thread.Sleep(200);
        }
        finally
        {
          _databaseConnection.Close();
        }
      } while (!success);

      if (!success)
      {
        Log.Error("TVMovie: Error accessing TV Movie ClickFinder database while reading schedules");
      }

      foreach (DataRow guideEntry in tvMovieTable.Tables["Markierungen"].Rows)
      {
        string rawDuration = guideEntry["Dauer"].ToString();
        string stationName = guideEntry["Senderkennung"].ToString();

        string channelName = TvMovieDatabase.GetChannelName(stationName);

        if (channelName != string.Empty)
        {
          DateTime startDate = DateTime.Parse(guideEntry["Datum"].ToString());
          TimeSpan startTime = DateTime.Parse(guideEntry["Uhrzeit"].ToString()).TimeOfDay;
          TimeSpan duration = TimeSpan.FromMinutes(Convert.ToDouble(rawDuration.Substring(0, rawDuration.IndexOf(' '))));

          TVRecording scheduledRecording = new TVRecording();
          scheduledRecording.Channel = channelName;
          scheduledRecording.Title = guideEntry["Sendung"].ToString();
          scheduledRecording.StartTime = startDate + startTime;
          scheduledRecording.EndTime = startDate + startTime + duration;

          Log.Debug("TVMovie: Channel: {0} (CF: {1} MP: {2})", scheduledRecording.Channel, stationName, channelName);
          Log.Debug("TVMovie: Title  : {0}", scheduledRecording.Title);
          Log.Debug("TVMovie: Start  : {0}", scheduledRecording.StartTime);
          Log.Debug("TVMovie: End    : {0}", scheduledRecording.EndTime);

          if (!ScheduleExists(scheduledRecording))
          {
            int result = Recorder.AddRecording(ref scheduledRecording);
            Log.Debug("TVMovie: added schedule - {0}", result);
          }
          else
          {
            Log.Debug("TVMovie: schedule already in database");
          }
        }
        else
        {
          Log.Error("TVMovie: Station \"{0}\" is unknown because it is not mapped - schedule has not been added",
                    stationName);
        }
      }
    }


    private bool ScheduleExists(TVRecording schedule)
    {
      List<TVRecording> recs = new List<TVRecording>();
      TVDatabase.GetRecordings(ref recs);
      foreach (TVRecording recording in recs)
      {
        if (schedule.Channel == recording.Channel
            && schedule.Title == recording.Title
            && schedule.StartTime == recording.StartTime
            && schedule.EndTime == recording.EndTime)
        {
          return true;
        }
      }
      return false;
    }
  }
}