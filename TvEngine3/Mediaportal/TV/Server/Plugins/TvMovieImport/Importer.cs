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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TvMovieImport
{
  internal class Importer
  {
    private bool _isImportRunning = false;
    private bool _isImportCancelled = false;

    public delegate void ShowProgressHandler(string status, ImportStats stats);

    private class MappedChannel
    {
      public int ChannelId;
      public string ChannelName;
      public readonly ProgramList Programs = new ProgramList();
    }

    private static OleDbConnection GetDatabaseConnection()
    {
      string databasePath = SettingsManagement.GetValue(TvMovieImportSetting.DatabaseFile, TvMovieProperty.DatabasePath);
      if (string.IsNullOrEmpty(databasePath) || !File.Exists(databasePath))
      {
        Log.Error("TV Movie import: database path is not valid, path = {0}", databasePath);
        return null;
      }

      string dataProviderString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Mode=Share Deny None;Jet OLEDB:Engine Type=5;Jet OLEDB:Database Locking Mode=1;";
      dataProviderString = string.Format(dataProviderString, databasePath);

      OleDbConnection connection = null;
      try
      {
        connection = new OleDbConnection(dataProviderString);
        connection.Open();
      }
      catch (Exception ex)
      {
        Log.Error(ex, "TV Movie import: failed to connect to database, path = {0}", databasePath);
        if (connection != null)
        {
          connection.Dispose();
        }
      }
      return connection;
    }

    public static IList<string> GetTvMovieDatabaseChannelList()
    {
      List<string> channelNames = new List<string>();
      OleDbConnection dbConnection = GetDatabaseConnection();
      if (dbConnection == null)
      {
        return channelNames;
      }

      using (var tvMovieTable = new DataSet("Sender")) 
      {
        try
        {
          // columns:
          // ID = identifier
          // SenderKennung = name
          // Bezeichnung = description
          // *Webseite = URL for broadcaster's website
          // *SortNrTVMovie = unknown sort order
          // *Zeichen = logo file name
          // [* = according to old code, not always present]
          using (var dbCommand = new OleDbCommand("SELECT * FROM Sender WHERE Favorit = true AND GueltigBis >= Now() ORDER BY Bezeichnung ASC;", dbConnection))
          {
            using (var dbAdapter = new OleDbDataAdapter(dbCommand))
            {
              dbAdapter.FillSchema(tvMovieTable, SchemaType.Source, "Sender");
              dbAdapter.Fill(tvMovieTable);
              foreach (DataRow sender in tvMovieTable.Tables["Table"].Rows)
              {
                channelNames.Add(sender["SenderKennung"].ToString());
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex, "TV Movie import: failed to retrieve channels from database");
          return channelNames;
        }
        finally
        {
          dbConnection.Close();
          dbConnection.Dispose();
        }
      }
      return channelNames;
    }

    public bool Import(bool checkForNewData, ShowProgressHandler showProgress, ref ImportStats stats)
    {
      Log.Info("TV Movie import: import TV Movie database");

      _isImportRunning = true;
      try
      {
        showProgress("updating TV Movie database", stats);
        UpdateTvMovieDatabase();

        if (_isImportCancelled)
        {
          showProgress("import cancelled", stats);
          return false;
        }

        Log.Debug("TV Movie import: reading channels");
        showProgress("loading channel list", stats);
        IList<Channel> tempChannels = ChannelManagement.GetAllChannelsWithExternalId();
        IList<Channel> allDbChannelsWithGuideChannelMappings = new List<Channel>(tempChannels.Count);
        foreach (Channel channel in tempChannels)
        {
          if (TvMovieImportId.HasTvMovieMapping(channel.ExternalId))
          {
            allDbChannelsWithGuideChannelMappings.Add(channel);
          }
        }

        // TV Movie ID => mapped channels (programs etc.)
        Dictionary<string, IList<MappedChannel>> allMappedChannelsByGuideChannelId = new Dictionary<string, IList<MappedChannel>>();
        IList<string> tvMovieChannels = GetTvMovieDatabaseChannelList();
        int iChannel = 0;
        foreach (string tvmChannel in tvMovieChannels)
        {
          stats.ChannelCountTvmDb++;
          iChannel++;

          string id = TvMovieImportId.GetQualifiedIdForChannel(tvmChannel);

          // a guide channel can be mapped to more than one DB channel
          bool isMapped = false;
          foreach (Channel dbChannel in allDbChannelsWithGuideChannelMappings)
          {
            if (!dbChannel.ExternalId.Equals(id))
            {
              continue;
            }
            isMapped = true;

            MappedChannel mappedChannel = new MappedChannel();
            mappedChannel.ChannelId = dbChannel.IdChannel;
            mappedChannel.ChannelName = dbChannel.Name;

            IList<MappedChannel> dbChannelsMappedToGuideChannel;
            if (!allMappedChannelsByGuideChannelId.TryGetValue(tvmChannel, out dbChannelsMappedToGuideChannel))
            {
              dbChannelsMappedToGuideChannel = new List<MappedChannel>(5);
              allMappedChannelsByGuideChannelId.Add(tvmChannel, dbChannelsMappedToGuideChannel);
            }
            dbChannelsMappedToGuideChannel.Add(mappedChannel);

            Log.Debug("  channel #{0}, ID = {1}, name = {2}, DB ID = {3}", iChannel, dbChannel.ExternalId, dbChannel.Name, dbChannel.IdChannel);
          }

          if (!isMapped)
          {
            stats.ChannelCountTvmDbUnmapped++;
          }

          showProgress("loading channel list", stats);
        }

        if (_isImportCancelled)
        {
          showProgress("import cancelled", stats);
          return false;
        }

        OleDbConnection dbConnection = GetDatabaseConnection();
        if (dbConnection == null)
        {
          showProgress("failed to connect to TV Movie database", stats);
          return false;
        }

        try
        {
          // Remove programs that have already shown from the DB.
          Log.Debug("TV Movie import: removing expired DB programs");
          showProgress("removing old programs", stats);
          ProgramManagement.DeleteOldPrograms();

          if (_isImportCancelled)
          {
            showProgress("import cancelled", stats);
            return false;
          }

          Log.Debug("TV Movie import: reading programmes");
          showProgress("loading programs", stats);

          IDictionary<string, ProgramCategory> dbCategories = new Dictionary<string, ProgramCategory>();
          foreach (var programCategory in ProgramCategoryManagement.ListAllProgramCategories())
          {
            dbCategories.Add(programCategory.Category, programCategory);
          }

          // SenderKennung == channel name
          // Sendung == program title
          // Kurzkritik == brief review (critic)
          // KurzBeschreibung == short description
          // Beschreibung == description
          // Zweikanalton == dual mono audio
          // FSK == classification
          // Herstellungsjahr == production year
          // Regie == director
          // Darsteller == actors
          // Interessant == star rating
          // Bewertungen == detailed rating, for example "Spaß=1;Action=3;Erotik=1;Spannung=3;Anspruch=0;Gefühl=2"
          // Dauer == duration
          // Herstellungsland == production country
          // Wiederholung == repeating
          // Other columns: F16zu9 [???], untertitel [episode name???]
          StringBuilder sql = new StringBuilder();
          sql.Append("SELECT TVDaten.SenderKennung, TVDaten.Beginn, TVDaten.Ende, TVDaten.Sendung, TVDaten.Genre, TVDaten.Kurzkritik, TVDaten.KurzBeschreibung, TVDaten.Beschreibung");
          sql.Append(", TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton");
          sql.Append(", TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Originaltitel, TVDaten.Regie, TVDaten.Darsteller");
          sql.Append(", TVDaten.Interessant, TVDaten.Bewertungen");
          sql.Append(", TVDaten.live, TVDaten.Dauer, TVDaten.Herstellungsland, TVDaten.Wiederholung");
          sql.Append(" FROM TVDaten WHERE TVDaten.SenderKennung = \"{0}\" AND TVDaten.Ende >= #{1}# ORDER BY TVDaten.Beginn;");

          foreach (var mappedChannelSet in allMappedChannelsByGuideChannelId)
          {
            // Check that this TV Movie channel is mapped to at least one DB
            // channel. If not, there is no point in retrieving the programmes
            // for this channel.
            if (mappedChannelSet.Value == null || mappedChannelSet.Value.Count == 0)
            {
              continue;
            }

            using (var dbCommand = new OleDbCommand(string.Format(sql.ToString(), mappedChannelSet.Key, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), dbConnection))
            {
              // Use a transaction to try to ensure the main application doesn't
              // try to change the DB contents while we're importing.
              using (var dbTransaction = dbConnection.BeginTransaction(IsolationLevel.ReadCommitted))
              {
                try
                {
                  dbCommand.Transaction = dbTransaction;
                  using (var reader = dbCommand.ExecuteReader(CommandBehavior.SequentialAccess))
                  {
                    ImportGuideChannel(reader, mappedChannelSet.Value, dbCategories, showProgress, ref stats);
                    reader.Close();
                  }
                  dbTransaction.Commit();
                  dbConnection.Close();
                }
                catch (Exception ex)
                {
                  dbTransaction.Rollback();
                  Log.Error(ex, "TV Movie import: failed to retreve program from TV Movie database, TV Movie channel = {0}", mappedChannelSet.Key);
                  break;
                }
              }
            }

            if (_isImportCancelled)
            {
              showProgress("import cancelled", stats);
              return false;
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex, "TV Movie import: failed to retrieve programs from TV Movie database");
        }
        finally
        {
          dbConnection.Close();
          dbConnection.Dispose();
        }

        Log.Debug("TV Movie import: sorting and filtering programs");
        showProgress("sorting programs", stats);
        DateTime dtStartDate = DateTime.Today;

        foreach (IList<MappedChannel> mappedChannelSet in allMappedChannelsByGuideChannelId.Values)
        {
          foreach (MappedChannel mappedChannel in mappedChannelSet)
          {
            if (mappedChannel.Programs.Count == 0)
            {
              continue;
            }

            // Be sure that we don't have any overlapping programs within the
            // set of programs we're about to import.
            mappedChannel.Programs.SortIfNeeded();
            mappedChannel.Programs.FixEndTimes();
            mappedChannel.Programs.RemoveOverlappingPrograms();

            // Don't import programs which have already ended.
            for (int i = 0; i < mappedChannel.Programs.Count; ++i)
            {
              var prog = mappedChannel.Programs[i];
              if (prog.EndTime <= dtStartDate)
              {
                mappedChannel.Programs.RemoveAt(i);
                i--;
              }
            }

            if (_isImportCancelled)
            {
              showProgress("import cancelled", stats);
              return false;
            }

            stats.ChannelCountTveDb++;
            stats.ProgramCountTveDb += mappedChannel.Programs.Count;
            showProgress("storing programs", stats);
            Log.Info("TV Movie import: inserting {0} programs for {1}", mappedChannel.Programs.Count, mappedChannel.ChannelName);
            ProgramManagement.InsertPrograms(mappedChannel.Programs, EpgDeleteBeforeImportOption.ProgramsOnSameChannel, ThreadPriority.BelowNormal);
          }
        }

        Log.Debug("TV Movie import: TV Movie database import completed");
        return true;
      }
      catch (Exception ex)
      {
        showProgress("unexpected error, check error log for details", stats);
        Log.Error(ex, "TV Movie import: failed to import TV Movie database");
      }
      finally
      {
        _isImportRunning = false;
      }

      return false;
    }

    private void ImportGuideChannel(OleDbDataReader tvmDbReader, IList<MappedChannel> mappedChannels, IDictionary<string, ProgramCategory> dbCategories,
                                            ShowProgressHandler showProgress, ref ImportStats stats)
    {
      while (tvmDbReader.Read())
      {
        stats.ProgramCountTvmDb++;
        if (stats.ProgramCountTvmDb % 100 == 0)
        {
          if (_isImportCancelled)
          {
            showProgress("import cancelled", stats);
            return;
          }
          showProgress("loading programs", stats);
        }

        DateTime start;
        DateTime end;
        try
        {
          start = DateTime.Parse(tvmDbReader[1].ToString()); // eg. 15.06.2006 22:45:00
          end = DateTime.Parse(tvmDbReader[2].ToString());
        }
        catch (Exception ex)
        {
          Log.Error(ex, "TV Movie import: failed to parse program start and/or end times");
          continue;
        }

        string title = tvmDbReader[3].ToString();
        string episodeName = tvmDbReader[16].ToString();
        if (string.IsNullOrEmpty(episodeName) || title.Contains(episodeName))
        {
          episodeName = null;
        }

        ProgramCategory programCategory = null;
        string genre = tvmDbReader[4].ToString();
        if (!string.IsNullOrEmpty(genre) && !dbCategories.TryGetValue(genre, out programCategory))
        {
          programCategory = new ProgramCategory { Category = genre };
          programCategory = ProgramCategoryManagement.AddCategory(programCategory);
          dbCategories[genre] = programCategory;
        }

        string shortDescription = tvmDbReader[6].ToString();
        string description = tvmDbReader[7].ToString();
        if (string.IsNullOrEmpty(description))
        {
          description = shortDescription;
        }
        else if (!description.Contains(shortDescription))
        {
          description = string.Format("{0}{1}{2}", shortDescription, Environment.NewLine, description);
        }
        description = description.Replace("<br>", Environment.NewLine);

        int minimumAge;
        string classification = null;
        if (int.TryParse(tvmDbReader[14].ToString(), out minimumAge))
        {
          classification = string.Format("FSK {0}", minimumAge);
        }

        string productionYearString = tvmDbReader[15].ToString();
        int productionYear = -1;
        if (productionYearString != null && productionYearString.Length == 4)
        {
          if (!int.TryParse(productionYearString, out productionYear))
          {
            productionYear = -1;
          }
        }

        string director = tvmDbReader[17].ToString();
        string actorsString = tvmDbReader[18].ToString();
        string[] actors = new string[0];
        if (actorsString != null)
        {
          actors = actorsString.Split(';');
        }

        // Seems to be:
        // 0 = no rating
        // 1..7 = rated, DVB-compatible???
        // This may be wrong. The old code was confusing!
        decimal starRating = -1;
        decimal starRatingMaximum = -1;
        int tempStarRating;
        if (int.TryParse(tvmDbReader[19].ToString(), out tempStarRating) && tempStarRating != 0)
        {
          starRating = (tempStarRating + 1) / 4;
          starRatingMaximum = 4;
        }

        bool live = false;
        if (!bool.TryParse(tvmDbReader[21].ToString(), out live))
        {
          live = false;
        }

        string productionCountry = tvmDbReader[23].ToString();
        if (productionCountry != null)
        {
          productionCountry = productionCountry.Trim();
        }

        bool isRepeat = false;
        if (!bool.TryParse(tvmDbReader[24].ToString(), out isRepeat))
        {
          isRepeat = false;
        }

        foreach (MappedChannel mappedChannel in mappedChannels)
        {
          Program prog = ProgramFactory.CreateProgram(mappedChannel.ChannelId, start, end, title);
          prog.Description = description;
          if (episodeName != null)
          {
            prog.EpisodeName = episodeName;
          }
          if (isRepeat)
          {
            prog.IsPreviouslyShown = isRepeat;
          }
          if (programCategory != null)
          {
            prog.IdProgramCategory = programCategory.IdProgramCategory;
          }
          if (classification != null)
          {
            prog.Classification = classification;
          }
          prog.IsLive = live;
          if (productionYear > 0)
          {
            prog.ProductionYear = productionYear;
          }
          if (!string.IsNullOrEmpty(productionCountry))
          {
            prog.ProductionCountry = productionCountry;
          }
          if (starRating >= 0)
          {
            prog.StarRating = starRating;
            prog.StarRatingMaximum = starRatingMaximum;
          }

          // Actors, example...
          // Bernd Schramm (Buster der Hund);Sandra Schwarzhaupt (Gwendolyn die Katze);Joachim Kemmer (Tortellini der Hahn);Mario Adorf (Fred der Esel);Katharina Thalbach (die Erbin);Peer Augustinski (Dr. Gier);Klausjürgen Wussow (Der Erzähler);Hartmut Engler (Hund Buster);Bert Henry (Drehbuch);Georg Reichel (Drehbuch);Dagmar Kekule (Drehbuch);Peter Wolf (Musik);Dagmar Kekulé (Drehbuch)
          foreach (string actor in actors)
          {
            int idx = actor.IndexOf('(');
            if (idx != -1)
            {
              prog.ProgramCredits.Add(new ProgramCredit { Person = actor.Substring(0, idx).Trim(), Role = "actor" });
            }
            else
            {
              prog.ProgramCredits.Add(new ProgramCredit { Person = actor, Role = "actor" });
            }
          }
          if (!string.IsNullOrEmpty(director))
          {
            prog.ProgramCredits.Add(new ProgramCredit { Person = director, Role = "director" });
          }

          mappedChannel.Programs.Add(prog);
        }
      }
    }

    public void CancelImport()
    {
      if (_isImportRunning)
      {
        this.LogInfo("TV Movie import: cancelling import...");
      }
      _isImportCancelled = true;
    }

    public static bool UpdateTvMovieDatabase()
    {
      Log.Debug("TV Movie import: update TV Movie database");
      string updaterPath = TvMovieProperty.UpdaterPath;
      if (!File.Exists(updaterPath))
      {
        Log.Warn("TV Movie import: TvUpToDate executable not found, path = {0}", updaterPath);
        return true;    // assume an update is required
      }

      bool isUpdateRequired = false;
      Stopwatch benchClock = new Stopwatch();
      try
      {
        // Check whether TvUpToDate is already running. It could have been
        // started by TV Movie.
        Process[] processes = Process.GetProcessesByName("tvuptodate");
        if (processes != null && processes.Length > 0)
        {
          Log.Warn("TV Movie import: TvUpToDate already running, wait for completion");
          processes[0].WaitForExit();
          Log.Debug("TV Movie import: external update completed");
          return true;
        }

        benchClock.Start();
        ProcessStartInfo startInfo = new ProcessStartInfo("tvuptodate.exe");
        startInfo.Arguments = "/hidden";
        startInfo.FileName = updaterPath;
        startInfo.WorkingDirectory = Path.GetDirectoryName(updaterPath);
        Process p = Process.Start(startInfo);
        p.PriorityClass = ProcessPriorityClass.BelowNormal;
        p.WaitForExit();
        Log.Debug("TV Movie import: update completed");
        isUpdateRequired = benchClock.ElapsedMilliseconds > 20000;
      }
      catch (Exception ex)
      {
        Log.Error(ex, "TV Movie import: update failed");
        isUpdateRequired = true;    // force update
      }
      finally
      {
        benchClock.Stop();
      }

      return isUpdateRequired;
    }
  }
}