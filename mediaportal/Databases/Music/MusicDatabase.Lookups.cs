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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using SQLite.NET;

namespace MediaPortal.Music.Database
{
  public partial class MusicDatabase
  {
    #region Variables

    private string _strSongSelect = "select Song.*, Album.*, " +
                                    "( select group_concat(aname, ' | ') from (select distinct(Artist.ArtistName) as aname from artist join artistsong on artistsong.idsong = song.IdSong and artistsong.idartist = artist.IdArtist)) as Artist," +
                                    "( select Artist.ArtistName from artist join albumartist on albumartist.idalbum = Album.IdAlbum and albumartist.idartist = artist.idArtist) as AlbumArtist," +
                                    "( select group_concat(genrename, ' | ') from (select distinct genrename from Genre join genresong on genresong.idsong = song.idSong and genresong.idgenre = genre.idGenre order by genrename)) as Genre," +
                                    "( select group_concat(composername, ' | ') from (select distinct artist.artistname as composername from artist join composersong on composersong.idsong = song.idSong and composersong.idcomposer = artist.idArtist order by composername)) as composer," +
                                    "(share.ShareName || folder.FolderName || song.FileName) as Path ";

    private string _strSongFrom = "from Song ";

    private string _strSongJoin = "join Album on Album.IdAlbum = Song.IdAlbum " +
                            "join folder on folder.idFolder = song.idfolder " +
                            "join share on share.IdShare = folder.IdShare " +
                            "join artistsong on artistsong.idsong = song.Idsong and artistsong.idartist = artist.idartist ";

    #endregion

    public bool AssignAllSongFieldsFromResultSet(ref Song aSong, SQLiteResultSet aResult, int aRow)
    {
      if (aSong == null || aResult == null || aResult.Rows.Count < 1)
      {
        return false;
      }
      aSong.Id = DatabaseUtility.GetAsInt(aResult, aRow, "IdSong");
      aSong.FileName = DatabaseUtility.Get(aResult, aRow, "Path");
      aSong.Artist = DatabaseUtility.Get(aResult, aRow, "Artist").Trim(trimChars);
      aSong.AlbumArtist = DatabaseUtility.Get(aResult, aRow, "AlbumArtist").Trim(trimChars);
      aSong.AlbumId = DatabaseUtility.GetAsInt(aResult, aRow, "IdAlbum");
      aSong.Album = DatabaseUtility.Get(aResult, aRow, "AlbumName");
      aSong.AlbumSort = DatabaseUtility.Get(aResult, aRow, "AlbumSortName");
      aSong.Genre = DatabaseUtility.Get(aResult, aRow, "Genre").Trim(trimChars);
      aSong.Title = DatabaseUtility.Get(aResult, aRow, "Title");
      aSong.TitleSort = DatabaseUtility.Get(aResult, aRow, "TitleSort");
      aSong.Track = DatabaseUtility.GetAsInt(aResult, aRow, "Track");
      aSong.TrackTotal = DatabaseUtility.GetAsInt(aResult, aRow, "TrackCount");
      aSong.Duration = DatabaseUtility.GetAsInt(aResult, aRow, "Duration");
      aSong.Year = DatabaseUtility.GetAsInt(aResult, aRow, "Year");
      aSong.TimesPlayed = DatabaseUtility.GetAsInt(aResult, aRow, "TimesPlayed");
      aSong.Rating = DatabaseUtility.GetAsInt(aResult, aRow, "Rating");
      aSong.Favorite = DatabaseUtility.GetAsInt(aResult, aRow, "Favorite") != 0;
      aSong.ResumeAt = DatabaseUtility.GetAsInt(aResult, aRow, "ResumeAt");
      aSong.DiscId = DatabaseUtility.GetAsInt(aResult, aRow, "Disc");
      aSong.DiscTotal = DatabaseUtility.GetAsInt(aResult, aRow, "DiscCount");
      aSong.Lyrics = DatabaseUtility.Get(aResult, aRow, "Lyrics");
      aSong.Composer = DatabaseUtility.Get(aResult, aRow, "Composer").Trim(trimChars);
      aSong.Conductor = DatabaseUtility.Get(aResult, aRow, "Conductor").Trim(trimChars);
      aSong.Comment = DatabaseUtility.Get(aResult, aRow, "Comment").Trim(trimChars);
      aSong.Copyright = DatabaseUtility.Get(aResult, aRow, "Copyright").Trim(trimChars);
      aSong.AmazonId = DatabaseUtility.Get(aResult, aRow, "AmazonId").Trim(trimChars);
      aSong.Grouping = DatabaseUtility.Get(aResult, aRow, "Grouping").Trim(trimChars);
      aSong.MusicBrainzArtistId = DatabaseUtility.Get(aResult, aRow, "MusicBrainzArtistId").Trim(trimChars);
      aSong.MusicBrainzDiscId = DatabaseUtility.Get(aResult, aRow, "MusicBrainzDiscId").Trim(trimChars);
      aSong.MusicBrainzReleaseArtistId = DatabaseUtility.Get(aResult, aRow, "MusicBrainzReleaseArtistId").Trim(trimChars);
      aSong.MusicBrainzReleaseCountry = DatabaseUtility.Get(aResult, aRow, "MusicBrainzReleaseCountry").Trim(trimChars);
      aSong.MusicBrainzReleaseId = DatabaseUtility.Get(aResult, aRow, "MusicBrainzReleaseId").Trim(trimChars);
      aSong.MusicBrainzReleaseStatus = DatabaseUtility.Get(aResult, aRow, "MusicBrainzReleaseStatus").Trim(trimChars);
      aSong.MusicBrainzReleaseTrackId = DatabaseUtility.Get(aResult, aRow, "MusicBrainzReleaseTrackId").Trim(trimChars);
      aSong.MusicBrainzReleaseType = DatabaseUtility.Get(aResult, aRow, "MusicBrainzReleaseType").Trim(trimChars);
      aSong.MusicIpid = DatabaseUtility.Get(aResult, aRow, "MusicIpid").Trim(trimChars);
      aSong.ReplayGainAlbum = DatabaseUtility.Get(aResult, aRow, "ReplayGainAlbum").Trim(trimChars);
      aSong.ReplayGainAlbumPeak = DatabaseUtility.Get(aResult, aRow, "ReplayGainAlbumPeak").Trim(trimChars);
      aSong.ReplayGainTrack = DatabaseUtility.Get(aResult, aRow, "ReplayGainTrack").Trim(trimChars);
      aSong.ReplayGainTrackPeak = DatabaseUtility.Get(aResult, aRow, "ReplayGainTrackPeak").Trim(trimChars);
      aSong.FileType = DatabaseUtility.Get(aResult, aRow, "FileType").Trim(trimChars);
      aSong.Codec = DatabaseUtility.Get(aResult, aRow, "Codec").Trim(trimChars);
      aSong.BitRateMode = DatabaseUtility.Get(aResult, aRow, "BitRateMode").Trim(trimChars);
      aSong.BPM = DatabaseUtility.GetAsInt(aResult, aRow, "BPM");
      aSong.BitRate = DatabaseUtility.GetAsInt(aResult, aRow, "BitRate");
      aSong.Channels = DatabaseUtility.GetAsInt(aResult, aRow, "Channels");
      aSong.SampleRate = DatabaseUtility.GetAsInt(aResult, aRow, "SampleRate");
      try
      {
        aSong.DateTimePlayed = DatabaseUtility.GetAsDateTime(aResult, aRow, "DateLastPlayed");
        aSong.DateTimeModified = DatabaseUtility.GetAsDateTime(aResult, aRow, "DateAdded");
      }
      catch (Exception ex)
      {
        Log.Warn("MusicDatabase Lookup: Exception parsing date fields: {0} stack: {1}", ex.Message, ex.StackTrace);
      }
      return true;
    }

    /// <summary>
    /// Returns the Last Import Date from the Database
    /// </summary>
    /// <returns></returns>
    public DateTime GetLastImportDate()
    {
      DateTime lastImportDate;

      strSQL = "select Value from Configuration where Parameter = 'LastImport'";
      SQLiteResultSet results = ExecuteQuery(strSQL);
      try
      {
        lastImportDate = DateTime.ParseExact(results.Rows[0].fields[0], "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
      }
      catch (Exception)
      {
        lastImportDate = DateTime.ParseExact("1900-01-01 00:00:00", "yyyy-M-d H:m:s", CultureInfo.InvariantCulture);
      }
      return lastImportDate;
    }

    /// <summary>
    /// Get the amount of Songs with a Favorite indicator > 0
    /// </summary>
    /// <returns></returns>
    public int GetTotalFavorites()
    {
      try
      {
        strSQL = String.Format("SELECT count(*) FROM Song WHERE Favorite > 0");
        var numSongs = ExecuteScalar(strSQL);
        if (numSongs == null)
        {
          return 0;
        }

        return (int)numSongs;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public string GetLastFMUser()
    {
      strSQL = @"select strUsername from lastfmusers";
      var results = DirectExecute(strSQL);
      if (results.Rows.Count == 0) return string.Empty;
      var row = results.Rows[0];
      return row.fields[0];
    }

    public string GetLastFMSK()
    {
      strSQL = @"select strSK from lastfmusers";
      var results = DirectExecute(strSQL);
      if (results.Rows.Count == 0) return string.Empty;
      var row = results.Rows[0];
      return row.fields[0];
    }

    public int GetTotalSongs()
    {
      try
      {
        int NumOfSongs;
        strSQL = String.Format("SELECT count(idTrack) FROM tracks");
        SQLiteResultSet results = DirectExecute(strSQL);
        SQLiteResultSet.Row row = results.Rows[0];
        NumOfSongs = Int32.Parse(results.Rows[0].fields[0]);

        return NumOfSongs;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public double GetAveragePlayCountForArtist(string aArtist)
    {
      try
      {
        if (string.IsNullOrEmpty(aArtist))
        {
          return 0;
        }

        string strSQL;
        string strArtist = aArtist;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);
        double AVGPlayCount;

        strSQL = String.Format("select avg(iTimesPlayed) from tracks where strArtist like '%| {0} |%'", strArtist);
        SQLiteResultSet result = DirectExecute(strSQL);

        Double.TryParse(result.Rows[0].fields[0], NumberStyles.Number, new CultureInfo("en-US"), out AVGPlayCount);

        return AVGPlayCount;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public bool GetRandomSong(ref Song aSong)
    {
      try
      {
        aSong.Clear();

        PseudoRandomNumberGenerator rand = new PseudoRandomNumberGenerator();

        int maxIDSong, rndIDSong;
        string strSQL = String.Format("SELECT max(idTrack) FROM tracks");

        SQLiteResultSet results = DirectExecute(strSQL);

        maxIDSong = DatabaseUtility.GetAsInt(results, 0, 0);
        rndIDSong = rand.Next(0, maxIDSong);

        strSQL = String.Format("SELECT * FROM tracks WHERE idTrack={0}", rndIDSong);

        results = DirectExecute(strSQL);
        if (results.Rows.Count > 0)
        {
          if (AssignAllSongFieldsFromResultSet(ref aSong, results, 0))
          {
            return true;
          }
        }
        else
        {
          GetRandomSong(ref aSong);
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }


    public void GetSongsBySQL(string aSQL, out List<Song> aSongs)
    {
      aSongs = new List<Song>();
      try
      {
        SQLiteResultSet results = DirectExecute(aSQL);
        Song song = null;

        for (int i = 0; i < results.Rows.Count; i++)
        {
          song = new Song();
          SQLiteResultSet.Row fields = results.Rows[i];

          AssignAllSongFieldsFromResultSet(ref song, results, i);
          aSongs.Add(song);
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetSongsByFilter(string aSQL, out List<Song> aSongs, string aFilter)
    {
      GetSongsByFilter(aSQL, out aSongs, aFilter, string.Empty, false);
    }

    public void GetSongsByFilter(string aSQL, out List<Song> aSongs, string aFilter, string aSearchField, bool aGrouping)
    {
      Log.Debug("MusicDatabase: GetSongsByFilter - SQL: {0}, Filter: {1}", aSQL, aFilter);
      aSongs = new List<Song>();
      //if (string.IsNullOrEmpty(filter))
      //  return;
      try
      {
        aSongs.Clear();

        var results = ExecuteQuery(aSQL);
        for (int i = 0; i < results.Rows.Count; i++)
        {
          var song = new Song();
          var fields = results.Rows[i];
          int columnIndex = 0;
          if (aFilter == "artist")
          {
            columnIndex = (int)results.ColumnIndices["strArtist"];
            song.Artist = fields.fields[columnIndex].Trim(trimChars);
          }
          else if (aFilter == "albumartist")
          {
            song.SelectedId = Convert.ToInt32(results.Rows[i].fields[0]);
            song.AlbumArtist = results.Rows[i].fields[1].Trim(trimChars);
          }
          else if (aFilter == "album")
          {
            AssignAllSongFieldsFromResultSet(ref song, results, i);
          }
          else if (aFilter == "genre")
          {
            AssignAllSongFieldsFromResultSet(ref song, results, i);
            columnIndex = (int)results.ColumnIndices["strGenre"];
            song.Genre = fields.fields[columnIndex].Trim(trimChars);
          }
          else if (aFilter == "composer")
          {
            AssignAllSongFieldsFromResultSet(ref song, results, i);
            columnIndex = (int)results.ColumnIndices["strComposer"];
            song.Composer = fields.fields[columnIndex].Trim(trimChars);
          }
          else if (aFilter == "tracks")
          {
            AssignAllSongFieldsFromResultSet(ref song, results, i);
          }

          // Now set the fields when we had grouping
          if (aGrouping && aSearchField != string.Empty)
          {
            if (aSearchField.ToLower() == "rating")
            {
              song.Rating = Convert.ToInt32(fields.fields[0].Trim(trimChars));
            }
            else if (aSearchField.ToLower() == "year")
            {
              song.Year = Convert.ToInt32(fields.fields[0].Trim(trimChars));
            }

            // When we have grouping active, the second field contains the number of items found which shall be put into duration
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }

          aSongs.Add(song);
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetSongsByIndex(string aSQL, out List<Song> aSongs, int aLevel, string filter)
    {
      Log.Debug("MusicDatabase: GetSongsByIndex - SQL: {0}, Level: {1}, Filter: {2}", aSQL, aLevel, filter);
      aSongs = new List<Song>();
      try
      {
        SQLiteResultSet results = DirectExecute(aSQL);
        Song song;

        int specialCharCount = 0;
        bool appendedSpecialChar = false;

        for (int i = 0; i < results.Rows.Count; i++)
        {
          SQLiteResultSet.Row fields = results.Rows[i];

          // Check for special characters to group them on Level 0 of a list
          if (aLevel == 0)
          {
            char ch = ' ';
            if (fields.fields[0] != "")
            {
              ch = fields.fields[0][0];
            }
            bool founddSpecialChar = false;
            if (ch < 'A')
            {
              specialCharCount += Convert.ToInt16(fields.fields[1]);
              founddSpecialChar = true;
            }

            if (founddSpecialChar && i < results.Rows.Count - 1)
            {
              continue;
            }

            // Now we've looped through all Chars < A let's add the song
            if (!appendedSpecialChar)
            {
              appendedSpecialChar = true;
              if (specialCharCount > 0)
              {
                song = new Song();
                if (filter != "tracks")
                {
                  song.Artist = "#";
                  song.Album = "#";
                  song.AlbumArtist = "#";
                  song.Genre = "#";
                  song.Composer = "#";
                }
                song.Title = "#";
                song.Duration = specialCharCount;
                aSongs.Add(song);
              }
            }
          }

          song = new Song();
          if (filter == "artist")
          {
            song.Artist = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (filter == "album")
          {
            song.Album = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (filter == "albumartist")
          {
            song.AlbumArtist = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (filter == "genre")
          {
            song.Genre = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (filter == "composer")
          {
            song.Composer = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (filter == "tracks")
          {
            song.Title = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          aSongs.Add(song);
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public bool GetSongByFileName(string aFileName, ref Song aSong)
    {
      string strFileName = aFileName;

      try
      {
        aSong.Clear();

        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        string strSQL = String.Format("SELECT * FROM tracks WHERE strPath = '{0}'", strFileName);

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        if (AssignAllSongFieldsFromResultSet(ref aSong, results, 0))
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Lookups: GetSongByFileName failed - strFileName: {2} - err:{0} stack:{1}", ex.Message,
                  ex.StackTrace, strFileName);
        Open();
      }

      return false;
    }

    public bool GetSongByMusicTagInfo(string aArtist, string aAlbum, string aTitle, bool inexactFallback, ref Song aSong)
    {
      //Log.Debug("MusicDatabase: GetSongByMusicTagInfo - Artist: {0}, Album: {1}, Title: {2}, Nearest match: {3}", aArtist, aAlbum, aTitle, Convert.ToString(inexactFallback));
      aSong.Clear();

      // we have all info - try exact
      if (!string.IsNullOrEmpty(aArtist) && !string.IsNullOrEmpty(aAlbum) && !string.IsNullOrEmpty(aTitle))
      {
        if (GetSongByArtistAlbumTitle(aArtist, aAlbum, aTitle, ref aSong))
        {
          return true;
        }
        else if (!inexactFallback)
        {
          return false;
        }
      }

      // An artist may have the same title in different versions (e.g. live, EP) on different discs - therefore this is the 2nd best option
      if (!string.IsNullOrEmpty(aAlbum) && !string.IsNullOrEmpty(aTitle))
      {
        if (GetSongByAlbumTitle(aAlbum, aTitle, ref aSong))
        {
          return true;
        }
        else if (!inexactFallback)
        {
          return false;
        }
      }

      // Maybe the album was spelled different on last.fm or is mistagged in the local collection
      if (!string.IsNullOrEmpty(aArtist) && !string.IsNullOrEmpty(aTitle))
      {
        if (GetSongByArtistTitle(aArtist, aTitle, ref aSong))
        {
          return true;
        }
        else if (!inexactFallback)
        {
          return false;
        }
      }

      // Make sure we get at least one / some usable results
      if (!string.IsNullOrEmpty(aTitle))
      {
        if (GetSongByTitle(aTitle, ref aSong))
        {
          return true;
        }
        else
        {
          return false;
        }
      }

      Log.Debug(
        "MusicDatabase: GetSongByMusicTagInfo did not get usable params! Artist: {0}, Album: {1}, Title: {2}, Nearest Match: {3}",
        aArtist, aAlbum, aTitle, Convert.ToString(inexactFallback));
      return false;
    }

    private bool GetSongByArtistAlbumTitle(string aArtist, string aAlbum, string aTitle, ref Song aSong)
    {
      try
      {
        aSong.Clear();
        string strTitle = aTitle;
        string strAlbum = aAlbum;
        string strArtist = aArtist;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);
        DatabaseUtility.RemoveInvalidChars(ref strArtist);

        string strSQL =
          String.Format(
            "SELECT * FROM tracks WHERE strArtist LIKE '%| {0} |%' AND strAlbum LIKE '{1}%' AND strTitle LIKE '{2}'",
            strArtist, strAlbum, strTitle);

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        if (results.Rows.Count > 1)
        {
          Log.Debug(
            "MusicDatabase: Lookups: GetSongByArtistAlbumTitle found multiple results ({3}) for {0} - {1} - {2}",
            strArtist, strAlbum, strTitle, Convert.ToString(results.Rows.Count));
        }

        if (AssignAllSongFieldsFromResultSet(ref aSong, results, 0))
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    private bool GetSongByAlbumTitle(string aAlbum, string aTitle, ref Song aSong)
    {
      try
      {
        aSong.Clear();
        string strTitle = aTitle;
        string strAlbum = aAlbum;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        string strSQL = String.Format("SELECT * FROM tracks WHERE strAlbum LIKE '{0}' AND strTitle LIKE '{1}'", strAlbum,
                                      strTitle);

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        if (results.Rows.Count > 1)
        {
          Log.Debug("MusicDatabase: Lookups: GetSongByAlbumTitle found multiple results ({2}) for {0} - {1}", strAlbum,
                    strTitle, Convert.ToString(results.Rows.Count));
        }

        if (AssignAllSongFieldsFromResultSet(ref aSong, results, 0))
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    private bool GetSongByArtistTitle(string aArtist, string aTitle, ref Song aSong)
    {
      try
      {
        aSong.Clear();
        string strTitle = aTitle;
        string strArtist = aArtist;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);
        DatabaseUtility.RemoveInvalidChars(ref strArtist);

        string strSQL = String.Format("SELECT * FROM tracks WHERE strArtist LIKE '%| {0} |%' AND strTitle LIKE '{1}'",
                                      strArtist, strTitle);

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        if (results.Rows.Count > 1)
        {
          Log.Debug("MusicDatabase: Lookups: GetSongByArtistTitle found multiple results ({2}) for {0} - {1}", strArtist,
                    strTitle, Convert.ToString(results.Rows.Count));
        }

        if (AssignAllSongFieldsFromResultSet(ref aSong, results, 0))
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    private bool GetSongByTitle(string aTitle, ref Song aSong)
    {
      try
      {
        aSong.Clear();
        string strTitle = aTitle;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);

        string strSQL = String.Format("SELECT * FROM tracks WHERE strTitle LIKE '{0}'", strTitle);

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        if (results.Rows.Count > 1)
        {
          Log.Debug("MusicDatabase: Lookups: GetSongByTitle found multiple results ({0}) for {1}",
                    Convert.ToString(results.Rows.Count), strTitle);
        }

        if (AssignAllSongFieldsFromResultSet(ref aSong, results, 0))
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    /// <summary>
    /// Gets Songs by Artist
    /// </summary>
    /// <param name="aArtist"></param>
    /// <param name="aSongList"></param>
    /// <returns></returns>
    public bool GetSongsByArtist(string aArtist, ref List<Song> aSongList)
    {
      return GetSongsByArtist(aArtist, ref aSongList, false);
    }

    /// <summary>
    /// Gets Songs by Artist
    /// </summary>
    /// <param name="aArtist"></param>
    /// <param name="aSongList"></param>
    /// <returns></returns>
    private bool GetSongsByArtist(string aArtist, ref List<Song> aSongList, bool aGroupAlbum)
    {
      try
      {
        string strSQL = string.Empty;

        string strArtist = DatabaseUtility.RemoveInvalidChars(aArtist);

        strSQL = string.Format("select idartist from artist where artistname like '{0}'", strArtist);
        var idArtist = ExecuteScalar(strSQL);
        if (idArtist == null)
        {
          return false;
        }

        strSQL = string.Format("{0} {1},Artist {2} where artist.idartist = {3}", _strSongSelect, _strSongFrom, _strSongJoin, idArtist);
        if (aGroupAlbum)
        {
          strSQL += " group by AlbumName Order by Year Desc";
        }

        var results = ExecuteQuery(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        aSongList.Clear();
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          var song = new Song();

          if (AssignAllSongFieldsFromResultSet(ref song, results, i))
          {
            aSongList.Add(song);
          }
        }
        if (aSongList.Count > 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByPath(string aPath, ref List<SongMap> aSongList)
    {
      string strSQL = string.Empty;
      try
      {
        aSongList.Clear();

        if (string.IsNullOrEmpty(aPath))
        {
          return false;
        }

        string strPath = DatabaseUtility.RemoveInvalidChars(aPath);
        if (!strPath.EndsWith(@"\"))
        {
          strPath += @"\";
        }

        // The underscore is treated as special symbol in a like clause, which produces wrong results
        // we need to escape it and use the sql escape clause  escape '\x0001'
        strPath = strPath.Replace("_", "\x0001_");
        strSQL = string.Format("{0} {1},Artist {2} " +
                                  "where (share.Sharename || folder.foldername) like '{3}' escape '\x0001'",
                                  _strSongSelect, _strSongFrom, _strSongJoin, strPath);

        var results = ExecuteQuery(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          var songmap = new SongMap();
          var song = new Song();

          AssignAllSongFieldsFromResultSet(ref song, results, i);

          songmap.m_song = song;
          songmap.m_strPath = song.FileName;

          aSongList.Add(songmap);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Lookups: GetSongsByPath failed: {0} exception err: {1} stack: {2}", strSQL, ex.Message,
                  ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByComposer(string aComposer, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();

        string strComposer = aComposer;
        DatabaseUtility.RemoveInvalidChars(ref strComposer);

        string sql = string.Format("SELECT * FROM tracks WHERE strComposer LIKE '%| {0} |%' ORDER BY strAlbum asc",
                                   strComposer);
        GetSongsByFilter(sql, out aSongList, "tracks");
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByConductor(string aConductor, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();

        string strConductor = aConductor;
        DatabaseUtility.RemoveInvalidChars(ref strConductor);

        string sql = string.Format("SELECT * FROM tracks WHERE strConductor LIKE '%| {0} |%' ORDER BY strAlbum asc",
                                   strConductor);
        GetSongsByFilter(sql, out aSongList, "tracks");
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByAlbumArtist(string aAlbumArtist, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();

        string strAlbumArtist = aAlbumArtist;
        DatabaseUtility.RemoveInvalidChars(ref strAlbumArtist);

        // Get the id of "Various Artists"
        string variousArtists = GUILocalizeStrings.Get(340);
        if (variousArtists.Length == 0)
        {
          variousArtists = "Various Artists";
        }

        string sql = string.Format("SELECT * FROM tracks WHERE strAlbumArtist LIKE '%| {0} |%' ORDER BY strAlbum asc",
                                   strAlbumArtist);
        GetSongsByFilter(sql, out aSongList, "tracks");
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByAlbumArtistAlbum(string aAlbumArtist, string aAlbum, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();

        string strAlbumArtist = aAlbumArtist;
        string strAlbum = aAlbum;
        DatabaseUtility.RemoveInvalidChars(ref strAlbumArtist);
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        string sql =
          string.Format(
            "SELECT * FROM tracks WHERE strAlbumArtist LIKE '%| {0} |%' AND strAlbum LIKE '{1}' order by iDisc asc, iTrack asc",
            strAlbumArtist, strAlbum);
        GetSongsByFilter(sql, out aSongList, "tracks");

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    /// <summary>
    /// Retrieve Songs by Genre
    /// </summary>
    /// <param name="aGenre"></param>
    /// <param name="aSongList"></param>
    /// <returns></returns>
    public bool GetSongsByGenre(string aGenre, ref List<Song> aSongList)
    {
      return GetSongsByGenre(aGenre, ref aSongList, false);
    }

    /// <summary>
    /// Retrieve Songs by Genre
    /// </summary>
    /// <param name="aGenre"></param>
    /// <param name="aSongList"></param>
    /// <returns></returns>
    private bool GetSongsByGenre(string aGenre, ref List<Song> aSongList, bool aGroupAlbums)
    {
      try
      {
        aSongList.Clear();

        string strSQL = string.Empty;
        var strGenre = DatabaseUtility.RemoveInvalidChars(aGenre);

        strSQL = string.Format("select idgenre from genre where genrename like '{0}'", strGenre);
        var idGenre = ExecuteScalar(strSQL);
        if (idGenre == null)
        {
          return false;
        }

        strSQL = string.Format("{0} {1},Genre {2} where genre.idgenre = {3}", _strSongSelect, _strSongFrom, _strSongJoin, idGenre);
        if (aGroupAlbums)
        {
          strSQL += " group by AlbumName Order by Year Desc";
        }

        var results = ExecuteQuery(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        aSongList.Clear();
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          var song = new Song();

          if (AssignAllSongFieldsFromResultSet(ref song, results, i))
          {
            aSongList.Add(song);
          }
        }
        if (aSongList.Count > 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByYear(int aYear, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();

        string sql = string.Format("SELECT * FROM tracks WHERE iYear='{0}' order by strTitle asc", aYear);

        Song song;
        SQLiteResultSet results = DirectExecute(sql);

        for (int i = 0; i < results.Rows.Count; i++)
        {
          song = new Song();
          if (AssignAllSongFieldsFromResultSet(ref song, results, i))
          {
            aSongList.Add(song);
          }
        }

        if (aSongList.Count > 0)
        {
          return true;
        }
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByAlbum(string aAlbum, ref ArrayList songs)
    {
      try
      {
        string strAlbum = aAlbum;
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        songs.Clear();

        string strSQL = String.Format("SELECT * FROM tracks WHERE strAlbum like '{0}'", strAlbum);

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          Song song = new Song();
          if (AssignAllSongFieldsFromResultSet(ref song, results, i))
          {
            songs.Add(song);
          }
        }

        if (songs.Count > 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByAlbumArtistAlbumDisc(string aAlbumArtist, string aAlbum, int discNo, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();

        string strAlbumArtist = aAlbumArtist;
        string strAlbum = aAlbum;
        DatabaseUtility.RemoveInvalidChars(ref strAlbumArtist);
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        string sql =
          string.Format(
            "SELECT * FROM tracks WHERE strAlbumArtist LIKE '%| {0} |%' AND strAlbum LIKE '{1}' AND iDisc = {2} order by iDisc asc, iTrack asc",
            strAlbumArtist, strAlbum, discNo);
        GetSongsByFilter(sql, out aSongList, "tracks");

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    /// <summary>
    /// Fetches songs with title pattern using the given precision
    /// </summary>
    /// <param name="aSearchKind">0 = starts with, 1 = contains, 2 = ends with, 3 = contains exact</param>
    /// <param name="aTitle">The song title you're searching</param>
    /// <param name="aListOfListitemSongs">A list to be filled with results</param>
    /// <returns>Whether search was successful</returns>
    public bool GetSongs(int aSearchKind, string aTitle, ref List<GUIListItem> aListOfListitemSongs)
    {
      try
      {
        aListOfListitemSongs.Clear();
        string strTitle = aTitle;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);

        string strSQL = string.Empty;
        switch (aSearchKind)
        {
          case 0:
            strSQL = String.Format("SELECT * FROM tracks WHERE strTitle LIKE '{0}%'", strTitle);
            break;
          case 1:
            strSQL = String.Format("SELECT * FROM tracks WHERE strTitle LIKE '%{0}%'", strTitle);
            break;
          case 2:
            strSQL = String.Format("SELECT * FROM tracks WHERE strTitle LIKE '%{0}'", strTitle);
            break;
          case 3:
            strSQL = String.Format("SELECT * FROM tracks WHERE strTitle LIKE '{0}'", strTitle);
            break;
          default:
            return false;
        }

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strFileName = DatabaseUtility.Get(results, i, "strPath");
          GUIListItem item = new GUIListItem();
          item.IsFolder = false;
          item.Label = Util.Utils.GetFilename(strFileName);
          item.Label2 = string.Empty;
          item.Label3 = string.Empty;
          item.Path = strFileName;
          item.FileInfo = new FileInformation(strFileName, item.IsFolder);
          Util.Utils.SetDefaultIcons(item);
          Util.Utils.SetThumbnails(ref item);
          aListOfListitemSongs.Add(item);
        }

        if (aListOfListitemSongs.Count > 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: exception on song retrieval - {0}", ex.Message);
        Open();
      }
      return false;
    }

    /// <summary>
    /// Fetches artists by name using the given precision
    /// </summary>
    /// <param name="aSearchKind">0 = starts with, 1 = contains, 2 = ends with, 3 = exact, 4 = clean before</param>
    /// <param name="aArtist">The song title you're searching</param>
    /// <param name="aArtistArray">An array to be filled with results</param>
    /// <returns>Whether search was successful</returns>
    public bool GetArtists(int aSearchKind, string aArtist, ref ArrayList aArtistArray)
    {
      try
      {
        aArtistArray.Clear();
        string strArtist = aArtist;
        DatabaseUtility.RemoveInvalidChars(ref strArtist); // <-- who commented that before (rtv)?

        string strSQL = string.Empty;
        switch (aSearchKind)
        {
          case 0:
            strSQL = String.Format("SELECT * FROM artist WHERE strArtist LIKE '{0}%' ", strArtist);
            break;
          case 1:
            strSQL = String.Format("SELECT * FROM artist WHERE strArtist LIKE '%{0}%' ", strArtist);
            break;
          case 2:
            strSQL = String.Format("SELECT * FROM artist WHERE strArtist LIKE '%{0}' ", strArtist);
            break;
          case 3:
            strSQL = String.Format("SELECT * FROM artist WHERE strArtist LIKE '{0}' ", strArtist);
            break;
          case 4:
            strArtist = strArtist.Replace('ä', '%');
            strArtist = strArtist.Replace('ö', '%');
            strArtist = strArtist.Replace('ü', '%');
            strArtist = strArtist.Replace('/', '%');
            strArtist = strArtist.Replace('-', '%');
            strArtist = strArtist.Replace("%%", "%");
            strSQL = String.Format("SELECT * FROM artist where strArtist LIKE '%{0}%' ", strArtist);
            break;
          default:
            return false;
        }

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string addArtist = DatabaseUtility.Get(results, i, "strArtist");
          aArtistArray.Add(addArtist);
        }

        if (aArtistArray.Count > 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetAllArtists(ref ArrayList aArtistArray)
    {
      try
      {
        aArtistArray.Clear();

        string strSQL = String.Format("SELECT DISTINCT ArtistName FROM Artist ORDER BY ArtistName");
        SQLiteResultSet results = ExecuteQuery(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strArtist = DatabaseUtility.Get(results, i, "ArtistName");
          aArtistArray.Add(strArtist.Trim(trimChars));
        }

        if (aArtistArray.Count > 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetAllAlbums(ref List<AlbumInfo> aAlbumInfoList)
    {
      try
      {
        aAlbumInfoList.Clear();

        string strSQL;
        strSQL =
          String.Format(
            "SELECT strAlbum, strAlbumArtist, strArtist, iYear FROM tracks GROUP BY strAlbum ORDER BY strAlbumArtist, strAlbum, iYear");

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "strAlbum");
          album.AlbumArtist = DatabaseUtility.Get(results, i, "strAlbumArtist");
          album.Artist = DatabaseUtility.Get(results, i, "strArtist");

          aAlbumInfoList.Add(album);
        }
        if (aAlbumInfoList.Count > 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetAlbums(int aSearchKind, string aAlbum, ref ArrayList aAlbumArray)
    {
      try
      {
        string strAlbum = aAlbum;
        aAlbumArray.Clear();

        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        string strSQL = "SELECT * FROM tracks WHERE strAlbum LIKE ";
        switch (aSearchKind)
        {
          case 0:
            strSQL += String.Format("'{0}%'", strAlbum);
            break;
          case 1:
            strSQL += String.Format("'%{0}%'", strAlbum);
            break;
          case 2:
            strSQL += String.Format("'%{0}'", strAlbum);
            break;
          case 3:
            strSQL += String.Format("'{0}'", strAlbum);
            break;
          default:
            return false;
        }
        strSQL += " GROUP BY strAlbum";

        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "strAlbum");
          album.AlbumArtist = DatabaseUtility.Get(results, i, "strAlbumArtist");
          album.Artist = DatabaseUtility.Get(results, i, "strArtist");
          //album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idAlbumArtist");  //album.IdAlbum contains IdAlbumArtist
          aAlbumArray.Add(album);
        }
        if (aAlbumArray.Count > 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetAlbumInfo(string aAlbumName, string aArtistName, ref AlbumInfo aAlbumInfo)
    {
      try
      {
        string strArtist = aArtistName;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);

        string strAlbum = aAlbumName;
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        string strSQL;
        strSQL = String.Format("select * from albuminfo where strArtist like '{0}%' and strAlbum  like '{1}'", strArtist,
                               strAlbum);
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          aAlbumInfo.Rating = DatabaseUtility.GetAsInt(results, 0, "albuminfo.iRating");
          aAlbumInfo.Year = DatabaseUtility.GetAsInt(results, 0, "albuminfo.iYear");
          aAlbumInfo.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
          aAlbumInfo.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
          aAlbumInfo.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
          aAlbumInfo.Image = DatabaseUtility.Get(results, 0, "albuminfo.strImage");
          aAlbumInfo.Review = DatabaseUtility.Get(results, 0, "albuminfo.strReview");
          aAlbumInfo.Styles = DatabaseUtility.Get(results, 0, "albuminfo.strStyles");
          aAlbumInfo.Tones = DatabaseUtility.Get(results, 0, "albuminfo.strTones");
          aAlbumInfo.Tracks = DatabaseUtility.Get(results, 0, "albuminfo.strTracks");
          //album.Path   = DatabaseUtility.Get(results,0,"path.strPath");
          return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetArtistInfo(string aArtist, ref ArtistInfo aArtistInfo)
    {
      try
      {
        string strArtist = aArtist;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);
        string strSQL;
        strSQL = String.Format("select * from artistinfo where strArtist like '{0}'", strArtist);
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          aArtistInfo.Artist = DatabaseUtility.Get(results, 0, "artistinfo.strArtist");
          aArtistInfo.Born = DatabaseUtility.Get(results, 0, "artistinfo.strBorn");
          aArtistInfo.YearsActive = DatabaseUtility.Get(results, 0, "artistinfo.strYearsActive");
          aArtistInfo.Genres = DatabaseUtility.Get(results, 0, "artistinfo.strGenres");
          aArtistInfo.Styles = DatabaseUtility.Get(results, 0, "artistinfo.strStyles");
          aArtistInfo.Tones = DatabaseUtility.Get(results, 0, "artistinfo.strTones");
          aArtistInfo.Instruments = DatabaseUtility.Get(results, 0, "artistinfo.strInstruments");
          aArtistInfo.Image = DatabaseUtility.Get(results, 0, "artistinfo.strImage");
          aArtistInfo.AMGBio = DatabaseUtility.Get(results, 0, "artistinfo.strAMGBio");
          aArtistInfo.Albums = DatabaseUtility.Get(results, 0, "artistinfo.strAlbums");
          aArtistInfo.Compilations = DatabaseUtility.Get(results, 0, "artistinfo.strCompilations");
          aArtistInfo.Singles = DatabaseUtility.Get(results, 0, "artistinfo.strSingles");
          aArtistInfo.Misc = DatabaseUtility.Get(results, 0, "artistinfo.strMisc");
          return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    [Obsolete]
    public int GetArtistId(string aArtist)
    {
      try
      {
        string strArtist = aArtist;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);

        var strSQL = string.Format("select DISTINCT artist.idArtist from artist where artist.strArtist LIKE '{0}'",
          strArtist);
        var results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return -1;
        }

        return DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return -1;
    }

    /// <summary>
    /// Retrieve the Genres from the Genre Table
    /// </summary>
    /// <param name="genres"></param>
    /// <returns></returns>
    public bool GetGenres(ref ArrayList genres)
    {
      try
      {
        genres.Clear();
        
        var strSQL = string.Format("SELECT GenreName FROM genre ORDER BY GenreName");
        var results = ExecuteQuery(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          var strGenre = DatabaseUtility.Get(results, i, "GenreName");
          genres.Add(strGenre);
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    /// <summary>
    /// Get Genres based on specific search patterm
    /// </summary>
    /// <param name="searchKind"></param>
    /// <param name="aGenre"></param>
    /// <param name="aGenreArray"></param>
    /// <returns></returns>
    public bool GetGenres(int searchKind, string aGenre, ref ArrayList aGenreArray)
    {
      try
      {
        aGenreArray.Clear();

        var strGenre = DatabaseUtility.RemoveInvalidChars(aGenre);

        string strSQL = string.Empty;
        switch (searchKind)
        {
          case 0:
            strSQL = String.Format("select GenreName from genre where GenreName like '{0}%'", strGenre);
            break;
          case 1:
            strSQL = String.Format("select GenreName from genre where GenreName like '%{0}%'", strGenre);
            break;
          case 2:
            strSQL = String.Format("select GenreName from genre where GenreName like '%{0}'", strGenre);
            break;
          case 3:
            strSQL = String.Format("select GenreName from genre where GenreName like '{0}'", strGenre);
            break;
          default:
            return false;
        }

        var results = ExecuteQuery(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string tmpGenre = DatabaseUtility.Get(results, i, "GenreName");
          if (!string.IsNullOrEmpty(tmpGenre))
          {
            aGenreArray.Add(tmpGenre);
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    /// <summary>
    /// Returns filename of songs which have a rating
    /// Needed by the MusicRatingUpdater plugin
    /// </summary>
    /// <param name="aSongs"></param>
    /// <returns></returns>
    public bool GetSongsWithRating(ref List<Song> aSongs)
    {
      try
      {
        aSongs.Clear();

        string strSQL = "select strPath, iRating from tracks where iRating > 0";
        SQLiteResultSet results = DirectExecute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }

        Song song = null;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          song = new Song();
          AssignAllSongFieldsFromResultSet(ref song, results, i);
          aSongs.Add(song);
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }
  }
}