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

        PRNG rand = new PRNG();
        
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

    public void GetSongsByFilter(string aSQL, out List<Song> aSongs, bool aArtistTable, bool aAlbumartistTable, bool aSongTable, bool aGenreTable)
    {
      aSongs = new List<Song>();
      try
      {
        SQLiteResultSet results = MusicDatabase.DirectExecute(aSQL);
        Song song;

        for (int i = 0 ; i < results.Rows.Count ; i++)
        {
          song = new Song();
          SQLiteResultSet.Row fields = results.Rows[i];
          if (aArtistTable && !aSongTable)
          {
            song.Artist = fields.fields[1];
          }
          if (aAlbumartistTable && !aSongTable)
          {
            song.AlbumArtist = fields.fields[1];                        
          }
          if (aGenreTable && !aSongTable)
          {
            song.Genre = fields.fields[1];            
          }
          if (aSongTable)
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

    public void GetSongsByIndex(string aSQL, out List<Song> aSongs, int aLevel, bool aArtistTable, bool aAlbumTable, bool aAlbumartistTable, bool aSongTable, bool aGenreTable)
    {
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
                if (!aSongTable)
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
          if (aArtistTable && !aSongTable)
          {
            song.Artist = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (aAlbumTable && !aSongTable)
          {
            song.Album = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (aAlbumartistTable && !aSongTable)
          {
            song.AlbumArtist = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (aGenreTable && !aSongTable)
          {
            song.Genre = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (aSongTable)
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

        string strSQL = String.Format("SELECT * FROM tracks WHERE strArtist LIKE '{0}'", strArtist);

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

        string sql = string.Format("SELECT * FROM tracks WHERE strAlbumArtist LIKE '{0}' ORDER BY strAlbum asc", strAlbumArtist);
        GetSongsByFilter(sql, out aSongList, false, false, true, false);

        List<AlbumInfo> albums = new List<AlbumInfo>();
        GetAllAlbums(ref albums);

        foreach (Song song in aSongList)
        {
          foreach (AlbumInfo album in albums)
          {
            if (song.Album.Equals(album.Album))
            {
              // When we have a Various Artist album, we need to keep the original Artist id, to find songs
              // no longer possible
              //if (album.AlbumArtist == variousArtists)
              //  song.artistId = aAlbumArtistId;
              song.Artist = album.AlbumArtist;
              break;
            }
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

    public bool GetSongsByAlbumArtistAlbum(string aAlbumArtist, string aAlbum, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();

        string strAlbumArtist = aAlbumArtist;
        string strAlbum = aAlbum;
        DatabaseUtility.RemoveInvalidChars(ref strAlbumArtist);
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        string sql = string.Format("SELECT * FROM tracks WHERE strAlbumArtist LIKE '{0}' AND strAlbum = '{1}' order by iTrack asc", strAlbumArtist, strAlbum);
        ModifyAlbumQueryForVariousArtists(ref sql, strAlbumArtist, strAlbum);
        GetSongsByFilter(sql, out aSongList, true, true, true, false);

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    // Handle "Various Artists" cases where the artist id is different for 
    // many/most/all of the album tracks
    private void ModifyAlbumQueryForVariousArtists(ref string aSQL, string aAlbumartist, string aAlbum)
    {
      try
      {
        // Replace occurances of multiple space chars with single space
        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\s+");
        string temp = r.Replace(aSQL, " ");
        aSQL = temp;

        long idVariousArtists = GetVariousArtistsId();

        //if (aAlbumartistId == idVariousArtists)
        //{
        //  aSQL = aSQL.Replace("and song.idAlbumArtist=", "and album.idAlbumArtist=");
        //}
      }

      catch { }
    }

    public bool GetSongsByGenre(string aGenre, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();

        string strGenre = aGenre;
        DatabaseUtility.RemoveInvalidChars(ref strGenre);

        string sql = string.Format("SELECT * FROM tracks WHERE strGenre='{0}' order by strTitle asc", strGenre);
        GetSongsByFilter(sql, out aSongList, true, true, true, true);

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

    public bool GetSongsByAlbumID(int aAlbumId, ref List<Song> aSongList)
    {
      try
      {
        aSongList.Clear();
        if (null == MusicDbClient)
          return false;

        string strSQL;
        strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and album.idAlbum={0} and path.idPath=song.idPath order by song.iTrack", aAlbumId);
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          Song song = new Song();
          song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
          song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
          song.Favorite = DatabaseUtility.GetAsInt(results, i, "tracks.iFavorite") != 0;
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.FileName = strFileName;

          aSongList.Add(song);
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

    public bool GetVariousArtistsAlbums(ref List<AlbumInfo> aAlbumInfoList)
    {
      try
      {
        aAlbumInfoList.Clear();
        if (null == MusicDbClient)
          return false;

        string variousArtists = GUILocalizeStrings.Get(340);

        if (variousArtists.Length == 0)
          variousArtists = "Various Artists";

        long idVariousArtists = GetVariousArtistsId();

        string strSQL;
        strSQL = String.Format("select * from album where album.idAlbumArtist='{0}'", idVariousArtists);
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idAlbumArtist");  //album.IdAlbum contains IdAlbumArtist
          album.AlbumArtist = variousArtists;
          aAlbumInfoList.Add(album);
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

    public string GetAlbumPath(int aAlbumArtistId, int aAlbumId)
    {
      try
      {
        if (null == MusicDbClient)
          return string.Empty;

        //string sql = string.Format("select * from song, path where song.idPath=path.idPath and song.idArtist='{0}' and  song.idAlbum='{1}'  limit 1", nArtistId, nAlbumId);
        string sql = string.Format("select path.strPath from song,path,album where song.idPath=path.idPath and song.idAlbum=album.idAlbum and album.idAlbumArtist='{0}' and  album.idAlbum='{1}'  limit 1", aAlbumArtistId, aAlbumId);
        SQLiteResultSet results = MusicDbClient.Execute(sql);

        if (results.Rows.Count > 0)
        {
          string sPath = DatabaseUtility.Get(results, 0, "path.strPath");
          sPath += DatabaseUtility.Get(results, 0, "song.strFileName");

          return sPath;
        }
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return string.Empty;
    }

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

    public int AddAlbumInfo(AlbumInfo aAlbumInfo)
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
          return -1;
        int lGenreId = -1;
        //int lPathId   = AddPath(album1.Path);
        int lArtistId = -1;
        int lAlbumArtistId = -1;
        int lAlbumId = -1;

        strSQL = String.Format("delete  from albuminfo where idAlbum={0} ", lAlbumId);
        MusicDbClient.Execute(strSQL);

        strSQL = String.Format("insert into albuminfo (idAlbumInfo,idAlbum,idArtist,idGenre,strTones,strStyles,strReview,strImage,iRating,iYear,strTracks) values(NULL,{0},{1},{2},'{3}','{4}','{5}','{6}',{7},{8},'{9}' )",
                            lAlbumId, lArtistId, lGenreId,
                            album.Tones,
                            album.Styles,
                            album.Review,
                            album.Image,
                            album.Rating,
                            album.Year,
                            album.Tracks);
        MusicDbClient.Execute(strSQL);

        int lAlbumInfoId = MusicDbClient.LastInsertID();
        return lAlbumInfoId;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return -1;
    }

    public int AddArtistInfo(ArtistInfo aArtistInfo)
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
          return -1;
        int lArtistId = -1;

        //strSQL=String.Format("delete artistinfo where idArtist={0} ", lArtistId);
        //m_db.Execute(strSQL);
        strSQL = String.Format("select * from artistinfo where idArtist={0}", lArtistId);
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          strSQL = String.Format("delete artistinfo where idArtist={0} ", lArtistId);
          MusicDbClient.Execute(strSQL);
        }

        strSQL = String.Format("insert into artistinfo (idArtistInfo,idArtist,strBorn,strYearsActive,strGenres,strTones,strStyles,strInstruments,strImage,strAMGBio, strAlbums,strCompilations,strSingles,strMisc) values(NULL,{0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}' )",
          lArtistId,
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

        int lArtistInfoId = MusicDbClient.LastInsertID();
        return lArtistInfoId;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return -1;
    }

    public void DeleteAlbumInfo(string aAlbumName)
    {
      string strAlbum = aAlbumName;
      DatabaseUtility.RemoveInvalidChars(ref strAlbum);
      string strSQL = String.Format("select * from albuminfo,album where albuminfo.idAlbum=album.idAlbum and album.strAlbum like '{0}'", strAlbum);
      SQLiteResultSet results;
      results = MusicDbClient.Execute(strSQL);
      if (results.Rows.Count != 0)
      {
        int iAlbumId = DatabaseUtility.GetAsInt(results, 0, "albuminfo.idAlbum");
        strSQL = String.Format("delete from albuminfo where albuminfo.idAlbum={0}", iAlbumId);
        MusicDbClient.Execute(strSQL);
      }
    }

    public void DeleteArtistInfo(string aArtistName)
    {
      string strArtist = aArtistName;
      DatabaseUtility.RemoveInvalidChars(ref strArtist);
      string strSQL = String.Format("select * from artist where artist.strArtist like '{0}'", strArtist);
      SQLiteResultSet results;
      results = MusicDbClient.Execute(strSQL);
      if (results.Rows.Count != 0)
      {
        int iArtistId = DatabaseUtility.GetAsInt(results, 0, "idArtist");
        strSQL = String.Format("delete from artistinfo where artistinfo.idArtist={0}", iArtistId);
        MusicDbClient.Execute(strSQL);
      }
    }

    public bool GetAlbumInfo(int aAlbumId, ref AlbumInfo aAlbumInfo)
    {
      try
      {
        string strSQL;
        strSQL = String.Format("select * from albuminfo,album,genre,artist where albuminfo.idAlbum=album.idAlbum and albuminfo.idGenre=genre.idGenre and albuminfo.idArtist=artist.idArtist and album.idAlbum ={0}", aAlbumId);
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
        strSQL = String.Format("select * from artist,artistinfo where artist.idArtist=artistinfo.idArtist and artist.strArtist like '{0}'", strArtist);
        SQLiteResultSet results;
        results = MusicDbClient.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          aArtistInfo.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
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
    
    public void CheckVariousArtistsAndCoverArt()
    {
      if (_albumCache.Count <= 0)
        return;

      foreach (AlbumInfoCache album in _albumCache)
      {
        int lAlbumId = album.idAlbum;
        int lAlbumArtistId = album.idAlbumArtist;
        bool bVarious = false;
        ArrayList songs = new ArrayList();
        GetSongsByAlbum(album.Album, ref songs);
        if (_scanForVariousArtists && songs.Count > 1)
        {
          //	Are the artists of this album all the same
          for (int i = 0 ; i < (int)songs.Count - 1 ; i++)
          {
            Song song = (Song)songs[i];
            Song song1 = (Song)songs[i + 1];
            if (song.Artist != song1.Artist)
            {
              string variousArtists = GUILocalizeStrings.Get(340);
              
              // HW - Changed to compile correct. Do we still need it?
              //lAlbumArtistId = AddAlbumArtist(variousArtists);
              bVarious = true;
              break;
            }
          }
        }

        if (bVarious)
        {
          string strSQL;
          strSQL = String.Format("update album set idAlbumArtist={0} where idAlbum={1}", lAlbumArtistId, album.idAlbum);
          MusicDbClient.Execute(strSQL);
        }
        /*
                string strTempCoverArt;
                string strCoverArt;
                CUtil::GetAlbumThumb(album.strAlbum+album.strPath, strTempCoverArt, true);
                //	Was the album art of this album read during scan?
                if (CUtil::ThumbCached(strTempCoverArt))
                {
                  //	Yes.
                  //	Copy as permanent directory thumb
                  CUtil::GetAlbumThumb(album.strPath, strCoverArt);
                  ::CopyFile(strTempCoverArt, strCoverArt, false);

                  //	And move as permanent thumb for files and directory, where
                  //	album and path is known
                  CUtil::GetAlbumThumb(album.strAlbum+album.strPath, strCoverArt);
                  ::MoveFileEx(strTempCoverArt, strCoverArt, MOVEFILE_REPLACE_EXISTING);
                }*/
      }

      _albumCache.Clear();
    }

    /// <summary>
    /// Checks if songs of a given Album ID have different artists.
    /// Used by MusicShareWatcher
    /// </summary>
    /// <param name="albumId"></param>
    public void CheckVariousArtists(string aAlbum)
    {
      int lAlbumArtistId = 0;
      int lAlbumId = 0;
      bool bVarious = false;
      ArrayList songs = new ArrayList();
      GetSongsByAlbum(aAlbum, ref songs);
      if (_scanForVariousArtists && songs.Count > 1)
      {
        //	Are the artists of this album all the same
        for (int i = 0 ; i < (int)songs.Count - 1 ; i++)
        {
          Song song = (Song)songs[i];
          Song song1 = (Song)songs[i + 1];
          if (song.Artist != song1.Artist)
          {
            string variousArtists = GUILocalizeStrings.Get(340);
            
            // HW - Changed to compile correct. Do we still need it? 
            //lAlbumArtistId = AddAlbumArtist(variousArtists);
            lAlbumId = song.albumId;
            bVarious = true;
            break;
          }
        }

        if (bVarious)
        {
          string strSQL;
          strSQL = String.Format("update album set idAlbumArtist={0} where idAlbum={1}", lAlbumArtistId, lAlbumId);
          MusicDbClient.Execute(strSQL);
        }
      }
    }

    public int GetVariousArtistsId()
    {
      int idVariousArtists = -1;

      if (_scanForVariousArtists)
      {
        string variousArtists = GUILocalizeStrings.Get(340);

        if (variousArtists.Length == 0)
          variousArtists = "Various Artists";

        // HW - Changed to compile correct. Do we still need it?
        //idVariousArtists = AddAlbumArtist(variousArtists);
      }

      return idVariousArtists;
    }
  }
}