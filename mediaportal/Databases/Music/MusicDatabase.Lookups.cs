#region Copyright (C) 2007 Team MediaPortal

/* 
 *	Copyright (C) 2007 Team MediaPortal
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

namespace MediaPortal.Music.Database
{
  #region Usings
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Text.RegularExpressions;

  using SQLite.NET;

  using MediaPortal.Database;
  using MediaPortal.GUI.Library;
  using MediaPortal.Util;
  #endregion

  public partial class MusicDatabase
  {
    public bool AssignAllSongFieldsFromResultSet(ref Song aSong, SQLiteResultSet aResult, int aRow)
    {
      if (aSong == null || aResult == null || aResult.Rows.Count < 1)
        return false;

      aSong.Id = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.idTrack");
      aSong.FileName = DatabaseUtility.Get(aResult, aRow, "tracks.strPath");
      aSong.Artist = DatabaseUtility.Get(aResult, aRow, "tracks.strArtist");
      aSong.AlbumArtist = DatabaseUtility.Get(aResult, aRow, "tracks.strAlbumArtist");
      aSong.Album = DatabaseUtility.Get(aResult, aRow, "tracks.strAlbum");
      aSong.Genre = DatabaseUtility.Get(aResult, aRow, "tracks.strGenre");
      aSong.Title = DatabaseUtility.Get(aResult, aRow, "tracks.strTitle");
      aSong.Track = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iTrack");
      aSong.TrackTotal = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iNumTracks");
      aSong.Duration = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iDuration");
      aSong.Year = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iYear");
      aSong.TimesPlayed = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iTimesPlayed");
      aSong.Rating = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iRating");
      aSong.Favorite = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iFavorite") != 0;
      aSong.ResumeAt = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iResumeAt");
      aSong.DiscId = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iDisc");
      aSong.DiscTotal = DatabaseUtility.GetAsInt(aResult, aRow, "tracks.iNumDisc");
      //aSong.GainTrack ?
      //aSong.PeakTrack ?
      aSong.Lyrics = DatabaseUtility.Get(aResult, aRow, "tracks.strLyrics");
      aSong.MusicBrainzID = DatabaseUtility.Get(aResult, aRow, "tracks.musicBrainzID");
      try
      {
        aSong.DateTimePlayed = DatabaseUtility.GetAsDateTime(aResult, aRow, "tracks.dateLastPlayed");
        aSong.DateTimeModified = DatabaseUtility.GetAsDateTime(aResult, aRow, "tracks.dateAdded");
      }
      catch (Exception ex)
      {
        Log.Warn("MusicDatabase Lookup: Exception parsing date fields: {0} stack: {1}", ex.Message, ex.StackTrace);
      }
      return true;
    }


    public void SetFavorite(Song aSong)
    {
      try
      {
        if (aSong.Id == -1)
          return;

        int iFavorite = 0;
        if (aSong.Favorite)
          iFavorite = 1;
        string strSQL = String.Format("UPDATE tracks SET iFavorite={0} WHERE idTrack={1}", iFavorite, aSong.Id);
        MusicDatabase.DirectExecute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetRating(string aFilename, int aRating)
    {
      if (string.IsNullOrEmpty(aFilename))
        return;

      try
      {
        Song song = new Song();
        string strFileName = aFilename;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        strSQL = String.Format("UPDATE tracks SET iRating={0} WHERE strPath='{1}'", aRating, strFileName);
          
        MusicDatabase.DirectExecute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public int GetTotalFavorites()
    {
      try
      {
        int NumOfSongs;
        strSQL = String.Format("SELECT count(*) FROM tracks WHERE iFavorite > 0");
        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
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
          return 0;

        string strSQL;
        string strArtist = aArtist;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);
        double AVGPlayCount;

        strSQL = String.Format("select avg(iTimesPlayed) from tracks where strArtist like '%{0}%'", strArtist);
        SQLiteResultSet result = MusicDatabase.DirectExecute(strSQL);
        
        Double.TryParse(result.Rows[0].fields[0], System.Globalization.NumberStyles.Number, new System.Globalization.CultureInfo("en-US"), out AVGPlayCount);
        
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

        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);

        maxIDSong = DatabaseUtility.GetAsInt(results, 0, 0);
        rndIDSong = rand.Next(0, maxIDSong);

        strSQL = String.Format("SELECT * FROM tracks WHERE idTrack={0}", rndIDSong);

        results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count > 0)
        {
          if (AssignAllSongFieldsFromResultSet(ref aSong, results, 0))
            return true;
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

    public void GetSongsByFilter(string aSQL, out List<Song> aSongs, string filter)
    {
      Log.Debug("SQL Filter: {0}", aSQL);
      aSongs = new List<Song>();
      try
      {
        SQLiteResultSet results = MusicDatabase.DirectExecute(aSQL);
        Song song;

        for (int i = 0 ; i < results.Rows.Count ; i++)
        {
          song = new Song();
          SQLiteResultSet.Row fields = results.Rows[i];
          int columnIndex = 0;
          if (filter == "artist")
          {
            columnIndex = (int)results.ColumnIndices["strArtist"];
            song.Artist = fields.fields[columnIndex];
          }
          if (filter == "albumartist")
          {
            columnIndex = (int)results.ColumnIndices["strAlbumArtist"];
            song.AlbumArtist = fields.fields[columnIndex];                        
          }
          if (filter == "album")
          {
            columnIndex = (int)results.ColumnIndices["strAlbum"];
            song.Album = fields.fields[columnIndex];
            columnIndex = (int)results.ColumnIndices["strAlbumArtist"];
            song.AlbumArtist = fields.fields[columnIndex];
            // Set the Pathname for Cover Art Retrieval
            columnIndex = (int)results.ColumnIndices["strPath"];
            song.FileName = String.Format("{0}\\",System.IO.Path.GetDirectoryName(fields.fields[columnIndex]));
          }
          if (filter == "genre")
          {
            columnIndex = (int)results.ColumnIndices["strGenre"];
            song.Genre = fields.fields[columnIndex];            
          }
          if (filter == "tracks")
          {
            AssignAllSongFieldsFromResultSet(ref song, results, i);
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
      Log.Debug("SQL Index: {0}", aSQL);
      aSongs = new List<Song>();
      try
      {
        SQLiteResultSet results = MusicDatabase.DirectExecute(aSQL);
        Song song;

        int specialCharCount = 0;
        bool appendedSpecialChar = false;

        for (int i = 0 ; i < results.Rows.Count ; i++)
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
              continue;

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

        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        if (AssignAllSongFieldsFromResultSet(ref aSong, results, 0))
          return true;
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Lookups: GetSongByFileName failed - strFileName: {2} - err:{0} stack:{1}", ex.Message, ex.StackTrace, strFileName);
        Open();
      }

      return false;
    }

    public bool GetSongByTitle(string aTitle, ref Song aSong)
    {
      try
      {
        aSong.Clear();
        string strTitle = aTitle;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);

        string strSQL = String.Format("SELECT * FROM tracks WHERE strTitle='{0}'", strTitle);

        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        if (AssignAllSongFieldsFromResultSet(ref aSong, results, 0))
          return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByArtist(string aArtist, ref List<Song> aSongList)
    {
      try
      {
        string strArtist = aArtist;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);

        aSongList.Clear();

        string strSQL = String.Format("SELECT * FROM tracks WHERE strArtist LIKE '%{0}'", strArtist);

        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          Song song = new Song();

          if (AssignAllSongFieldsFromResultSet(ref song, results, i))
            aSongList.Add(song);
        }
        if (aSongList.Count > 0)
          return true;
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
      string strSQL = String.Empty;
      try
      {
        aSongList.Clear();

        if (string.IsNullOrEmpty(aPath))
          return false;

        string strPath = aPath;
        //	musicdatabase always stores directories without a slash at the end
        strPath = Utils.RemoveTrailingSlash(strPath);
        DatabaseUtility.RemoveInvalidChars(ref strPath);

        strSQL = String.Format("select * from tracks where strPath like '{0}%'", strPath);
        
        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          SongMap songmap = new SongMap();
          Song song = new Song();

          AssignAllSongFieldsFromResultSet(ref song, results, i);

          songmap.m_song = song;
          songmap.m_strPath = song.FileName;

          aSongList.Add(songmap);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Lookups: GetSongsByPath failed: {0} exception err: {1} stack: {2}", strSQL, ex.Message, ex.StackTrace);
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
          variousArtists = "Various Artists";

        string sql = string.Format("SELECT * FROM tracks WHERE strAlbumArtist LIKE '%{0}' ORDER BY strAlbum asc", strAlbumArtist);
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

        string sql = string.Format("SELECT * FROM tracks WHERE strAlbumArtist LIKE '%{0}' AND strAlbum = '{1}' order by iTrack asc", strAlbumArtist, strAlbum);
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

    public bool GetSongsByGenre(string aGenre, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();

        string strGenre = aGenre;
        DatabaseUtility.RemoveInvalidChars(ref strGenre);

        string sql = string.Format("SELECT * FROM tracks WHERE strGenre like '%{0}' order by strTitle asc", strGenre);
        GetSongsByFilter(sql, out aSongList, "genre");

        return true;
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
        SQLiteResultSet results = MusicDatabase.DirectExecute(sql);

        for (int i = 0 ; i < results.Rows.Count ; i++)
        {
          song = new Song();
          if (AssignAllSongFieldsFromResultSet(ref song, results, i))
            aSongList.Add(song);
        }

        if (aSongList.Count > 0)
          return true;
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

        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          Song song = new Song();
          if (AssignAllSongFieldsFromResultSet(ref song, results, i))
            songs.Add(song);
        }

        if (songs.Count > 0)
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

        string strSQL = String.Empty;
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

        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          string strFileName = DatabaseUtility.Get(results, i, "strPath");
          GUIListItem item = new GUIListItem();
          item.IsFolder = false;
          item.Label = MediaPortal.Util.Utils.GetFilename(strFileName);
          item.Label2 = String.Empty;
          item.Label3 = String.Empty;
          item.Path = strFileName;
          item.FileInfo = new FileInformation(strFileName, item.IsFolder);
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          MediaPortal.Util.Utils.SetThumbnails(ref item);
          aListOfListitemSongs.Add(item);
        }

        if (aListOfListitemSongs.Count > 0)
          return true;
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
    /// <param name="aSearchKind">0 = starts with, 1 = contains, 2 = ends with, 3 = contains exact</param>
    /// <param name="aArtist">The song title you're searching</param>
    /// <param name="aArtistArray">An array to be filled with results</param>
    /// <returns>Whether search was successful</returns>
    public bool GetArtists(int aSearchKind, string aArtist, ref ArrayList aArtistArray)
    {
      try
      {
        aArtistArray.Clear();
        string strArtist = aArtist;
        //DatabaseUtility.RemoveInvalidChars(ref strArtist);

        string strSQL = String.Empty;
        switch (aSearchKind)
        {
          case 0:
            strSQL = String.Format("SELECT * FROM artist where strArtist LIKE '{0}%' ", strArtist);
            break;
          case 1:
            strSQL = String.Format("SELECT * FROM artist where strArtist LIKE '%{0}%' ", strArtist);
            break;
          case 2:
            strSQL = String.Format("SELECT * FROM artist where strArtist LIKE '%{0}' ", strArtist);
            break;
          case 3:
            strSQL = String.Format("SELECT * FROM artist where strArtist LIKE '{0}' ", strArtist);
            break;
          case 4:
            strArtist.Replace('ä', '%');
            strArtist.Replace('ö', '%');
            strArtist.Replace('ü', '%');
            strArtist.Replace('/', '%');
            strArtist.Replace('-', '%');
            strSQL = String.Format("SELECT * FROM artist where strArtist LIKE '%{0}%' ", strArtist);
            break;
          default:
            return false;
        }

        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          string addArtist = DatabaseUtility.Get(results, i, "strArtist");
          aArtistArray.Add(addArtist);
        }

        if (aArtistArray.Count > 0)
          return true;
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

        string strSQL = String.Format("SELECT * FROM artist");
        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          string strArtist = DatabaseUtility.Get(results, i, "strArtist");
          aArtistArray.Add(strArtist);
        }

        if (aArtistArray.Count > 0)
          return true;
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
        strSQL = String.Format("SELECT distinct(strAlbum), strAlbumArtist FROM tracks");
        
        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "strAlbum");
          album.AlbumArtist = DatabaseUtility.Get(results, i, "strAlbumArtist");
          
          aAlbumInfoList.Add(album);
        }
        if (aAlbumInfoList.Count > 0)
          return true;
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

        string strSQL = "SELECT distinct(strAlbum), * FROM tracks where strAlbum like ";
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
       
        SQLiteResultSet results = MusicDatabase.DirectExecute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "strAlbum");
          album.AlbumArtist = DatabaseUtility.Get(results, i, "strAlbumArtist");
          //album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idAlbumArtist");  //album.IdAlbum contains IdAlbumArtist
          aAlbumArray.Add(album);
        }
        if (aAlbumArray.Count > 0)
          return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    //public bool GetVariousArtistsAlbums(ref List<AlbumInfo> aAlbumInfoList)
    //{
    //  try
    //  {
    //    aAlbumInfoList.Clear();
    //    if (null == MusicDbClient)
    //      return false;

    //    string variousArtists = GUILocalizeStrings.Get(340);

    //    if (variousArtists.Length == 0)
    //      variousArtists = "Various Artists";

    //    long idVariousArtists = GetVariousArtistsId();

    //    string strSQL;
    //    strSQL = String.Format("select * from album where album.idAlbumArtist='{0}'", idVariousArtists);
    //    SQLiteResultSet results;
    //    results = MusicDbClient.Execute(strSQL);
    //    if (results.Rows.Count == 0)
    //      return false;
    //    for (int i = 0 ; i < results.Rows.Count ; ++i)
    //    {
    //      AlbumInfo album = new AlbumInfo();
    //      album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
    //      album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idAlbumArtist");  //album.IdAlbum contains IdAlbumArtist
    //      album.AlbumArtist = variousArtists;
    //      aAlbumInfoList.Add(album);
    //    }
    //    return true;
    //  }
    //  catch (Exception ex)
    //  {
    //    Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
    //    Open();
    //  }

    //  return false;
    //}

    public void ResetTop100()
    {
      try
      {
        string strSQL = String.Format("UPDATE tracks SET iTimesPlayed=0");
        MusicDatabase.DirectExecute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public bool IncrTop100CounterByFileName(string aFileName)
    {
      try
      {
        Song song = new Song();
        string strFileName = aFileName;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        strSQL = String.Format("SELECT * from tracks WHERE strPath = '{0}'", strFileName);

        SQLiteResultSet results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        int idSong = DatabaseUtility.GetAsInt(results, 0, "idTrack");
        int iTimesPlayed = DatabaseUtility.GetAsInt(results, 0, "iTimesPlayed");

        strSQL = String.Format("UPDATE tracks SET iTimesPlayed={0} where idTrack='{1}'", ++iTimesPlayed, idSong);
        if (MusicDatabase.DirectExecute(strSQL).Rows.Count > 0)
        {
          Log.Debug("MusicDatabase: increased playcount for song {1} to {0}", Convert.ToString(iTimesPlayed), aFileName);
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

    public void AddAlbumInfo(AlbumInfo aAlbumInfo)
    {
      string strSQL;
      try
      {
        AlbumInfo album = aAlbumInfo.Clone();
        string strTmp;
        //				strTmp = album.Album; DatabaseUtility.RemoveInvalidChars(ref strTmp); album.Album = strTmp;
        //				strTmp = album.Genre; DatabaseUtility.RemoveInvalidChars(ref strTmp); album.Genre = strTmp;
        //				strTmp = album.Artist; DatabaseUtility.RemoveInvalidChars(ref strTmp); album.Artist = strTmp;
        strTmp = album.Tones;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Tones = strTmp;
        strTmp = album.Styles;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Styles = strTmp;
        strTmp = album.Review;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Review = strTmp;
        strTmp = album.Image;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Image = strTmp;
        strTmp = album.Tracks;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Tracks = strTmp;
        //strTmp=album.Path  ;RemoveInvalidChars(ref strTmp);album.Path=strTmp;

        if (null == MusicDbClient)
          return;

        strSQL = String.Format("delete from albuminfo where strAlbum like '{0}' and strArtist like '{1}'", album.Album, album.Artist);
        MusicDbClient.Execute(strSQL);

        strSQL = String.Format("insert into albuminfo (strAlbum,strArtist, strTones,strStyles,strReview,strImage,iRating,iYear,strTracks) values('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}')",
                            album.Album,
                            album.Artist,
                            album.Tones,
                            album.Styles,
                            album.Review,
                            album.Image,
                            album.Rating,
                            album.Year,
                            album.Tracks);
        
        MusicDbClient.Execute(strSQL);

        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public void AddArtistInfo(ArtistInfo aArtistInfo)
    {
      string strSQL;
      try
      {
        ArtistInfo artist = aArtistInfo.Clone();
        string strTmp;
        //strTmp = artist.Artist; DatabaseUtility.RemoveInvalidChars(ref strTmp); artist.Artist = strTmp;
        strTmp = artist.Born;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Born = strTmp;
        strTmp = artist.YearsActive;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.YearsActive = strTmp;
        strTmp = artist.Genres;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Genres = strTmp;
        strTmp = artist.Instruments;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Instruments = strTmp;
        strTmp = artist.Tones;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Tones = strTmp;
        strTmp = artist.Styles;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Styles = strTmp;
        strTmp = artist.AMGBio;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.AMGBio = strTmp;
        strTmp = artist.Image;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Image = strTmp;
        strTmp = artist.Albums;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Albums = strTmp;
        strTmp = artist.Compilations;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Compilations = strTmp;
        strTmp = artist.Singles;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Singles = strTmp;
        strTmp = artist.Misc;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Misc = strTmp;

        if (null == MusicDbClient)
          return;

        strSQL = String.Format("delete from artistinfo where strArtist like '{0}'", artist.Artist);
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);

        strSQL = String.Format("insert into artistinfo (strArtist,strBorn,strYearsActive,strGenres,strTones,strStyles,strInstruments,strImage,strAMGBio, strAlbums,strCompilations,strSingles,strMisc) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}' )",
          artist.Artist,
          artist.Born,
          artist.YearsActive,
          artist.Genres,
          artist.Tones,
          artist.Styles,
          artist.Instruments,
          artist.Image,
          artist.AMGBio,
          artist.Albums,
          artist.Compilations,
          artist.Singles,
          artist.Misc);

        MusicDbClient.Execute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public void DeleteAlbumInfo(string aAlbumName, string aArtistName)
    {
      string strAlbum = aAlbumName;
      DatabaseUtility.RemoveInvalidChars(ref strAlbum);
      string strArtist = aArtistName;
      DatabaseUtility.RemoveInvalidChars(ref strArtist);
      SQLiteResultSet results;
      results = MusicDbClient.Execute(strSQL);
      strSQL = String.Format("delete from albuminfo where strAlbum like '{0} ' and strArtist like '{1}'", strAlbum, strArtist);
      MusicDbClient.Execute(strSQL);
    }

    public void DeleteArtistInfo(string aArtistName)
    {
      string strArtist = aArtistName;
      DatabaseUtility.RemoveInvalidChars(ref strArtist);
      SQLiteResultSet results;
      strSQL = String.Format("delete from artistinfo where strArtist like '{0}'", strArtist);
      MusicDbClient.Execute(strSQL);
    }

    public bool GetAlbumInfo(string aAlbumName, string aArtistName, ref AlbumInfo aAlbumInfo)
    {
      try
      {
        string strSQL;
        strSQL = String.Format("select * from albuminfo where strArtist like '{0}' and strAlbum  like '{1}'", aArtistName, aAlbumName);
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
    
    public int GetArtistId(string aArtist)
    {
      try
      {
        if (MusicDbClient == null)
          return -1;

        string strSQL;
        strSQL = String.Format("select distinct artist.idArtist from artist where artist.strArtist='{0}'", aArtist);
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
          return -1;

        return DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return -1;
    }

    public bool GetGenres(ref ArrayList genres)
    {
      try
      {
        genres.Clear();

        if (MusicDbClient == null)
          return false;

        string strSQL;
        strSQL = String.Format("select * from genre");
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strGenre = DatabaseUtility.Get(results, i, "strGenre");
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

    public bool GetGenres(int searchKind, string strGenere1, ref ArrayList genres)
    {
      try
      {
        genres.Clear();
        string strGenere = strGenere1;

        if (MusicDbClient == null)
          return false;

        string strSQL = String.Empty;
        switch (searchKind)
        {
          case 0:
            strSQL = String.Format("select * from genre where strGenre like '{0}%'", strGenere);
            break;
          case 1:
            strSQL = String.Format("select * from genre where strGenre like '%{0}%'", strGenere);
            break;
          case 2:
            strSQL = String.Format("select * from genre where strGenre like '%{0}'", strGenere);
            break;
          case 3:
            strSQL = String.Format("select * from genre where strGenre like '{0}'", strGenere);
            break;
          default:
            return false;
        }
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strGenre = DatabaseUtility.Get(results, i, "strGenre");
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

    //public void CheckVariousArtistsAndCoverArt()
    //{
    //  if (_albumCache.Count <= 0)
    //    return;

    //  foreach (AlbumInfoCache album in _albumCache)
    //  {
    //    int lAlbumId = album.idAlbum;
    //    int lAlbumArtistId = album.idAlbumArtist;
    //    bool bVarious = false;
    //    ArrayList songs = new ArrayList();
    //    GetSongsByAlbum(album.Album, ref songs);
    //    if (_scanForVariousArtists && songs.Count > 1)
    //    {
    //      //	Are the artists of this album all the same
    //      for (int i = 0 ; i < (int)songs.Count - 1 ; i++)
    //      {
    //        Song song = (Song)songs[i];
    //        Song song1 = (Song)songs[i + 1];
    //        if (song.Artist != song1.Artist)
    //        {
    //          string variousArtists = GUILocalizeStrings.Get(340);
              
    //          // HW - Changed to compile correct. Do we still need it?
    //          //lAlbumArtistId = AddAlbumArtist(variousArtists);
    //          bVarious = true;
    //          break;
    //        }
    //      }
    //    }

    //    if (bVarious)
    //    {
    //      string strSQL;
    //      strSQL = String.Format("update album set idAlbumArtist={0} where idAlbum={1}", lAlbumArtistId, album.idAlbum);
    //      MusicDbClient.Execute(strSQL);
    //    }
    //    /*
    //            string strTempCoverArt;
    //            string strCoverArt;
    //            CUtil::GetAlbumThumb(album.strAlbum+album.strPath, strTempCoverArt, true);
    //            //	Was the album art of this album read during scan?
    //            if (CUtil::ThumbCached(strTempCoverArt))
    //            {
    //              //	Yes.
    //              //	Copy as permanent directory thumb
    //              CUtil::GetAlbumThumb(album.strPath, strCoverArt);
    //              ::CopyFile(strTempCoverArt, strCoverArt, false);

    //              //	And move as permanent thumb for files and directory, where
    //              //	album and path is known
    //              CUtil::GetAlbumThumb(album.strAlbum+album.strPath, strCoverArt);
    //              ::MoveFileEx(strTempCoverArt, strCoverArt, MOVEFILE_REPLACE_EXISTING);
    //            }*/
    //  }

    //  _albumCache.Clear();
    //}

    /// <summary>
    /// Checks if songs of a given Album ID have different artists.
    /// Used by MusicShareWatcher
    /// </summary>
    /// <param name="albumId"></param>
    //public void CheckVariousArtists(string aAlbum)
    //{
    //  int lAlbumArtistId = 0;
    //  int lAlbumId = 0;
    //  bool bVarious = false;
    //  ArrayList songs = new ArrayList();
    //  GetSongsByAlbum(aAlbum, ref songs);
    //  if (_scanForVariousArtists && songs.Count > 1)
    //  {
    //    //	Are the artists of this album all the same
    //    for (int i = 0 ; i < (int)songs.Count - 1 ; i++)
    //    {
    //      Song song = (Song)songs[i];
    //      Song song1 = (Song)songs[i + 1];
    //      if (song.Artist != song1.Artist)
    //      {
    //        string variousArtists = GUILocalizeStrings.Get(340);
            
    //        // HW - Changed to compile correct. Do we still need it? 
    //        //lAlbumArtistId = AddAlbumArtist(variousArtists);
    //        lAlbumId = song.albumId;
    //        bVarious = true;
    //        break;
    //      }
    //    }

    //    if (bVarious)
    //    {
    //      string strSQL;
    //      strSQL = String.Format("update album set idAlbumArtist={0} where idAlbum={1}", lAlbumArtistId, lAlbumId);
    //      MusicDbClient.Execute(strSQL);
    //    }
    //  }
    //}
  }
}