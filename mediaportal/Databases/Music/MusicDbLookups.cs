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
    public int AddArtist(string strArtist1)
    {
      string strSQL;
      try
      {
        string strArtist = strArtist1;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);

        if (null == MusicDB)
          return -1;
        string name2 = strArtist1.ToLower().Trim();
        name2 = Regex.Replace(name2, @"[\W]*", string.Empty);
        foreach (CArtistCache artist in _artistCache)
        {
          string name1 = artist.strArtist.ToLower().Trim();
          name1 = Regex.Replace(name1, @"[\W]*", string.Empty);
          if (name1.Equals(name2))
            return artist.idArtist;
        }
        strSQL = String.Format("select * from artist where strArtist like '{0}'", strArtist);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into artist (idArtist, strArtist) values( NULL, '{0}' )", strArtist);
          MusicDB.Execute(strSQL);
          CArtistCache artist = new CArtistCache();
          artist.idArtist = MusicDB.LastInsertID();
          artist.strArtist = strArtist1;
          _artistCache.Add(artist);
          return artist.idArtist;
        }
        else
        {
          CArtistCache artist = new CArtistCache();
          artist.idArtist = DatabaseUtility.GetAsInt(results, 0, "idArtist");
          artist.strArtist = strArtist1;
          _artistCache.Add(artist);
          return artist.idArtist;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int AddPath(string strPath1)
    {
      string strSQL;
      try
      {
        if (strPath1 == null)
          return -1;
        if (strPath1.Length == 0)
          return -1;
        string strPath = strPath1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);
        DatabaseUtility.RemoveInvalidChars(ref strPath);

        if (null == MusicDB)
          return -1;

        foreach (CPathCache path in _pathCache)
          if (path.strPath == strPath1)
            return path.idPath;

        SQLiteResultSet results;
        strSQL = String.Format("select * from path where strPath like '{0}'", strPath);
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into path (idPath, strPath) values ( NULL, '{0}' )", strPath);
          MusicDB.Execute(strSQL);

          CPathCache path = new CPathCache();
          path.idPath = MusicDB.LastInsertID();
          path.strPath = strPath1;
          _pathCache.Add(path);
          return path.idPath;
        }
        else
        {
          CPathCache path = new CPathCache();
          path.idPath = DatabaseUtility.GetAsInt(results, 0, "idPath");
          path.strPath = strPath1;
          _pathCache.Add(path);
          return path.idPath;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int AddAlbumArtist(string strAlbumArtist1)
    {
      string strSQL;
      try
      {
        string strAlbumArtist = strAlbumArtist1;
        DatabaseUtility.RemoveInvalidChars(ref strAlbumArtist);

        if (null == MusicDB)
          return -1;
        string name2 = strAlbumArtist1.ToLower().Trim();
        name2 = Regex.Replace(name2, @"[\W]*", string.Empty);
        foreach (CAlbumArtistCache albumartist in _albumartistCache)
        {
          string name1 = albumartist.strAlbumArtist.ToLower().Trim();
          name1 = Regex.Replace(name1, @"[\W]*", string.Empty);
          if (name1.Equals(name2))
            return albumartist.idAlbumArtist;
        }
        strSQL = String.Format("select * from albumartist where strAlbumArtist like '{0}'", strAlbumArtist);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into albumartist (idAlbumArtist, strAlbumArtist) values( NULL, '{0}' )", strAlbumArtist);
          MusicDB.Execute(strSQL);
          CAlbumArtistCache albumartist = new CAlbumArtistCache();
          albumartist.idAlbumArtist = MusicDB.LastInsertID();
          albumartist.strAlbumArtist = strAlbumArtist1;
          _albumartistCache.Add(albumartist);
          return albumartist.idAlbumArtist;
        }
        else
        {
          CAlbumArtistCache albumartist = new CAlbumArtistCache();
          albumartist.idAlbumArtist = DatabaseUtility.GetAsInt(results, 0, "idAlbumArtist");
          albumartist.strAlbumArtist = strAlbumArtist1;
          _albumartistCache.Add(albumartist);
          return albumartist.idAlbumArtist;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int AddGenre(string strGenre1)
    {
      string strSQL;
      try
      {
        string strGenre = strGenre1;
        DatabaseUtility.RemoveInvalidChars(ref strGenre);
        if (String.Compare(strGenre.Substring(0, 1), "(") == 0)
        {
          /// We have the strange codes!
          /// Lets loop for the strange codes up to a of length X and delete them
          ///
          bool FixedTheCode = false;
          for (int i = 1 ; (i < 10 && i < strGenre.Length & !FixedTheCode) ; ++i)
          {
            if (String.Compare(strGenre.Substring(i, 1), ")") == 0)
            {
              ///Third position had the other end
              strGenre = strGenre.Substring(i + 1, (strGenre.Length - i - 1));
              FixedTheCode = true;
            }
          }
          //Log.Debug("Genre {0} changed to {1}", strGenre1, strGenre);
        }

        if (null == MusicDB)
          return -1;
        foreach (CGenreCache genre in _genreCache)
        {
          if (genre.strGenre == strGenre1)
            return genre.idGenre;
        }
        strSQL = String.Format("select * from genre where strGenre like '{0}'", strGenre);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into genre (idGenre, strGenre) values( NULL, '{0}' )", strGenre);
          MusicDB.Execute(strSQL);

          CGenreCache genre = new CGenreCache();
          genre.idGenre = MusicDB.LastInsertID();
          genre.strGenre = strGenre1;
          _genreCache.Add(genre);
          return genre.idGenre;
        }
        else
        {
          CGenreCache genre = new CGenreCache();
          genre.idGenre = DatabaseUtility.GetAsInt(results, 0, "idGenre");
          genre.strGenre = strGenre1;
          _genreCache.Add(genre);
          return genre.idGenre;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void SetFavorite(Song song)
    {
      try
      {
        if (song.songId < 0)
          return;
        int iFavorite = 0;
        if (song.Favorite)
          iFavorite = 1;
        string strSQL = String.Format("update song set favorite={0} where idSong={1}", iFavorite, song.songId);
        MusicDB.Execute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetRating(string filename, int rating)
    {
      if (filename == string.Empty || filename.Length == 0)
        return;

      try
      {
        Song song = new Song();
        string strFileName = filename;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        string strPath, strFName;
        DatabaseUtility.Split(strFileName, out strPath, out strFName);
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);

        if (null == MusicDB)
          return;

        string strSQL;
        ulong dwCRC;
        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        dwCRC = crc.calc(filename);

        strSQL = String.Format("select * from song,path where song.idPath=path.idPath and dwFileNameCRC like '{0}' and strPath like '{1}'",
          dwCRC,
          strPath);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return;
        int idSong = DatabaseUtility.GetAsInt(results, 0, "song.idSong");

        strSQL = String.Format("update song set iRating={0} where idSong={1}",
          rating, idSong);
        MusicDB.Execute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public void GetSongsByFilter(string sql, out List<Song> songs, bool artistTable, bool albumTable, bool albumartistTable, bool songTable, bool genreTable)
    {
      songs = new List<Song>();
      try
      {
        if (null == MusicDB)
          return;

        SQLiteResultSet results = MusicDB.Execute(sql);
        Song song;

        for (int i = 0 ; i < results.Rows.Count ; i++)
        {
          song = new Song();
          SQLiteResultSet.Row fields = results.Rows[i];
          if (artistTable && !songTable)
          {
            song.Artist = fields.fields[1];
            song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
            //Log.Write ("artisttable and not songtable, artistid={0}",song.artistId);
          }
          if (albumTable && !songTable)
          {
            song.Album = fields.fields[2];
            song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
            song.albumartistId = DatabaseUtility.GetAsInt(results, i, "album.idAlbumArtist");

            //if (fields.fields.Count >= 5)
            //    song.Artist = fields.fields[4];
            if (fields.fields.Count >= 6)
              song.Artist = fields.fields[5];
          }
          if (albumartistTable && !songTable)
          {
            song.AlbumArtist = fields.fields[1];
            song.albumartistId = DatabaseUtility.GetAsInt(results, i, "album.idAlbumArtist");
            //Log.Write ("albumartisttable and not songtable, albumartistid={0}",song.albumartistId);
          }
          if (genreTable && !songTable)
          {
            song.Genre = fields.fields[1];
            song.genreId = DatabaseUtility.GetAsInt(results, i, "song.idGenre");
            //Log.Write ("genretable and not songtable, genreid={0}",song.genreId);
          }
          if (songTable)
          {
            song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
            song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
            song.AlbumArtist = DatabaseUtility.Get(results, i, "album.strAlbumArtist");
            song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
            song.artistId = DatabaseUtility.GetAsInt(results, i, "song.idArtist");
            song.albumartistId = DatabaseUtility.GetAsInt(results, i, "song.idAlbumArtist");
            song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
            song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
            song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
            song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
            song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
            song.Favorite = DatabaseUtility.GetAsInt(results, i, "song.favorite") != 0;
            song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
            song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
            DatabaseUtility.GetAsInt(results, i, "song.idAlbum");
            DatabaseUtility.GetAsInt(results, i, "song.idGenre");
            string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
            strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
            song.FileName = strFileName;
            //Log.Write ("Song table with albumid={0}, artistid={1},songid={2}, strFilename={3}",song.albumId,song.artistId,song.songId,song.FileName);
          }
          songs.Add(song);
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetSongsByIndex(string sql, out List<Song> songs, int level, bool artistTable, bool albumTable, bool albumartistTable, bool songTable, bool genreTable)
    {
      songs = new List<Song>();
      try
      {
        if (null == MusicDB)
          return;

        SQLiteResultSet results = MusicDB.Execute(sql);
        Song song;

        int specialCharCount = 0;
        bool appendedSpecialChar = false;

        for (int i = 0 ; i < results.Rows.Count ; i++)
        {
          SQLiteResultSet.Row fields = results.Rows[i];


          // Check for special characters to group them on Level 0 of a list
          if (level == 0)
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
                if (!songTable)
                {
                  song.Artist = "#";
                  song.Album = "#";
                  song.AlbumArtist = "#";
                  song.Genre = "#";
                }
                song.Title = "#";
                song.Duration = specialCharCount;
                songs.Add(song);
              }
            }
          }
          song = new Song();
          if (artistTable && !songTable)
          {
            song.Artist = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (albumTable && !songTable)
          {
            song.Album = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (albumartistTable && !songTable)
          {
            song.AlbumArtist = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (genreTable && !songTable)
          {
            song.Genre = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (songTable)
          {
            song.Title = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          songs.Add(song);
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int AddAlbum(string strAlbum1, int lAlbumArtistId)
    {
      return AddAlbum(strAlbum1, lAlbumArtistId, -1);
    }

    public int AddAlbum(string strAlbum1, int lAlbumArtistId, int lPathId)
    {
      string strSQL;
      try
      {
        string strAlbum = strAlbum1;
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        if (null == MusicDB)
          return -1;
        string name2 = strAlbum.ToLower().Trim();
        name2 = Regex.Replace(name2, @"[\W]*", string.Empty);
        foreach (AlbumInfoCache album in _albumCache)
        {
          string name1 = album.Album.ToLower().Trim();
          name1 = Regex.Replace(name1, @"[\W]*", string.Empty);

          if (lPathId != -1)
          {
            if (name1.Equals(name2) && album.idPath == lPathId)
              return album.idAlbum;
          }
          else
          {
            if (name1.Equals(name2) && album.idAlbumArtist == lAlbumArtistId)
              return album.idAlbum;
          }
        }

        strSQL = String.Format("select * from album where strAlbum like '{0}'", strAlbum);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);

        if ((lPathId == -1 && lAlbumArtistId != DatabaseUtility.GetAsInt64(results, 0, "idAlbumArtist")) ||
            (lPathId != -1 && lPathId != DatabaseUtility.GetAsInt64(results, 0, "idPath")))
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into album (idAlbum, strAlbum, idAlbumArtist) values( NULL, '{0}', {1})", strAlbum, lAlbumArtistId);
          MusicDB.Execute(strSQL);

          AlbumInfoCache album = new AlbumInfoCache();
          album.idAlbum = MusicDB.LastInsertID();
          album.Album = strAlbum1;
          album.idAlbumArtist = lAlbumArtistId;
          album.idPath = lPathId;

          _albumCache.Add(album);
          return album.idAlbum;
        }
        else
        {
          AlbumInfoCache album = new AlbumInfoCache();
          album.idAlbum = DatabaseUtility.GetAsInt(results, 0, "idAlbum");
          album.Album = strAlbum1;
          album.idAlbumArtist = DatabaseUtility.GetAsInt(results, 0, "idAlbumArtist");
          album.idPath = lPathId;

          _albumCache.Add(album);
          return album.idAlbum;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void AddSong(Song song1, bool bCheck)
    {
      //Log.Error("database.AddSong {0} {1} {2}  {3}", song1.FileName,song1.Album, song1.Artist, song1.Title);
      string strSQL;
      try
      {
        Song song = song1.Clone();
        string strTmp;
        //Log.Write ("MusicDatabaseReorg: Going to AddSong {0}",song.FileName);

        //        strTmp = song.Album; DatabaseUtility.RemoveInvalidChars(ref strTmp); song.Album = strTmp;
        //        strTmp = song.Genre; DatabaseUtility.RemoveInvalidChars(ref strTmp); song.Genre = strTmp;
        //        strTmp = song.Artist; DatabaseUtility.RemoveInvalidChars(ref strTmp); song.Artist = strTmp;
        strTmp = song.Title;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        song.Title = strTmp;

        // SourceForge Patch 1442438 (hwahrmann) Part 1 of 4
        //strTmp = song.FileName; DatabaseUtility.RemoveInvalidChars(ref strTmp); song.FileName = strTmp;
        // \1442438

        string strPath, strFileName;

        DatabaseUtility.Split(song.FileName, out strPath, out strFileName);

        // SourceForge Patch 1442438 (hwahrmann) Part 2 of 4
        DatabaseUtility.RemoveInvalidChars(ref strFileName);
        // \1442438

        if (null == MusicDB)
          return;
        int lGenreId = AddGenre(song.Genre);
        int lArtistId = AddArtist(song.Artist);
        int lAlbumArtistId = AddAlbumArtist(song.AlbumArtist);
        int lPathId = AddPath(strPath);

        // SV
        //int lAlbumId = AddAlbum(song.Album, lArtistId);

        int lAlbumId = -1;

        if (_treatFolderAsAlbum)
          lAlbumId = AddAlbum(song.Album, /*lArtistId,*/ lAlbumArtistId, lPathId);
        else
          lAlbumId = AddAlbum(song.Album, /*lArtistId,*/ lAlbumArtistId);
        // \SV

        //Log.Write ("Getting a CRC for {0}",song.FileName);

        ulong dwCRC = 0;
        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        dwCRC = crc.calc(song1.FileName);
        SQLiteResultSet results;

        //Log.Write ("MusicDatabaseReorg: CRC for {0} = {1}",song.FileName,dwCRC);
        if (bCheck)
        {
          strSQL = String.Format("select * from song where idAlbum={0} AND idGenre={1} AND idArtist={2} AND dwFileNameCRC like '{3}' AND strTitle='{4}'",
                                lAlbumId, lGenreId, lArtistId, dwCRC, song.Title);
          //Log.Write (strSQL);
          try
          {
            results = MusicDB.Execute(strSQL);

            song1.albumId = lAlbumId;
            song1.artistId = lArtistId;
            song1.genreId = lGenreId;

            if (results.Rows.Count != 0)
            {
              song1.songId = DatabaseUtility.GetAsInt(results, 0, "idSong");
              return;
            }
          }
          catch (Exception)
          {
            Log.Error("MusicDatabaseReorg: Executing query failed");
          }
        } //End if

        int iFavorite = 0;
        if (song.Favorite)
          iFavorite = 1;

        crc.Init(CRCTool.CRCCode.CRC32);
        dwCRC = crc.calc(song1.FileName);

        Log.Info("Song {0} will be added with CRC {1}", strFileName, dwCRC);

        strSQL = String.Format("insert into song (idSong,idArtist,idAlbum,idGenre,idPath,strTitle,iTrack,iDuration,iYear,dwFileNameCRC,strFileName,iTimesPlayed,iRating,favorite) values(NULL,{0},{1},{2},{3},'{4}',{5},{6},{7},'{8}','{9}',{10},{11},{12})",
                    lArtistId, lAlbumId, lGenreId, lPathId,
                    song.Title,
                    song.Track, song.Duration, song.Year,
                    dwCRC,
                    strFileName, 0, song.Rating, iFavorite);
        song1.songId = MusicDB.LastInsertID();


        MusicDB.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    private int AddSong(string MusicFileName, string MusicFilePath)
    {
      SQLiteResultSet results;

      int idPath = AddPath(MusicFilePath);
      int idArtist = AddArtist(Strings.Unknown);
      int idAlbumArtist = AddAlbumArtist(Strings.Unknown);

      int idAlbum = -1;

      if (_treatFolderAsAlbum)
        idAlbum = AddAlbum(Strings.Unknown, idAlbumArtist, idPath);

      else
        idAlbum = AddAlbum(Strings.Unknown, idAlbumArtist);

      int idGenre = AddGenre(Strings.Unknown);

      /// Here we are gonna make a CRC code to add to the database
      /// This coded is used for searching on the filename
      string fname = MusicFilePath;
      if (!fname.EndsWith(@"\"))
        fname = fname + @"\";
      fname += MusicFileName;

      ulong dwCRC = 0;
      CRCTool crc = new CRCTool();
      crc.Init(CRCTool.CRCCode.CRC32);
      dwCRC = crc.calc(fname);

      strSQL = String.Format("insert into song (idArtist,idAlbum,idGenre,idPath,iTrack,iDuration,iYear,dwFileNameCRC,strFileName,iTimesPlayed,iRating,favorite) values ({0},{1},{2},{3},{4},{5},{6},{7},{9}{8}{9},{10},{11},{12})", idArtist, idAlbum, idGenre, idPath, 0, 0, 0, dwCRC, MusicFileName, Convert.ToChar(34), 0, 0, 0);
      //Log.Write (strSQL);
      try
      {
        //Log.Write ("Musicdatabasereorg: Insert {0}{1} into the database",MusicFilePath,MusicFileName);
        results = MusicDB.Execute(strSQL);
        if (results == null)
        {
          Log.Info("Musicdatabasereorg: Insert of song {0}{1} failed", MusicFilePath, MusicFileName);
          return (int)Errors.ERROR_REORG_SONGS;
        }
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: Insert of song {0}{1} failed", MusicFilePath, MusicFileName);
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_SONGS;
      }

      //Log.Write ("Musicdatabasereorg: Insert of song {0}{1} success",MusicFilePath,MusicFileName);
      return (int)Errors.ERROR_OK;
    }

    public void DeleteSong(string strFileName, bool bCheck)
    {
      try
      {
        int lGenreId = -1;
        int lArtistId = -1;
        int lPathId = -1;
        int lAlbumId = -1;
        int lSongId = -1;

        // SourceForge Patch 1442438 (hwahrmann) Part 3 of 4
        //DatabaseUtility.RemoveInvalidChars(ref strFileName);
        //string strPath, strFName;
        //DatabaseUtility.Split(strFileName, out strPath, out strFName);
        // \1442438

        if (null == MusicDB)
          return;

        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        ulong dwCRC = crc.calc(strFileName);

        // SourceForge Patch 1442438 (hwahrmann) Part 4 of 4
        DatabaseUtility.RemoveInvalidChars(ref strFileName);
        string strPath, strFName;
        DatabaseUtility.Split(strFileName, out strPath, out strFName);
        // \1442438

        string strSQL;
        strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and dwFileNameCRC like '{0}' and strPath like '{1}'",
          dwCRC,
          strPath);

        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          lArtistId = DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
          lAlbumId = DatabaseUtility.GetAsInt(results, 0, "album.idAlbum");
          lGenreId = DatabaseUtility.GetAsInt(results, 0, "genre.idGenre");
          lPathId = DatabaseUtility.GetAsInt(results, 0, "path.idPath");
          lSongId = DatabaseUtility.GetAsInt(results, 0, "song.idSong");

          // Delete
          strSQL = String.Format("delete from song where song.idSong={0}", lSongId);
          MusicDB.Execute(strSQL);

          if (bCheck)
          {
            // Check albums
            strSQL = String.Format("select * from song where song.idAlbum={0}", lAlbumId);
            results = MusicDB.Execute(strSQL);
            if (results.Rows.Count == 0)
            {
              // Delete album with no songs
              strSQL = String.Format("delete from album where idAlbum={0}", lAlbumId);
              MusicDB.Execute(strSQL);

              // Delete album info
              strSQL = String.Format("delete from albuminfo where idAlbum={0}", lAlbumId);
              MusicDB.Execute(strSQL);
            }

            // Check artists
            strSQL = String.Format("select * from song where song.idArtist={0}", lArtistId);
            results = MusicDB.Execute(strSQL);
            if (results.Rows.Count == 0)
            {
              // Delete artist with no songs
              strSQL = String.Format("delete from artist where idArtist={0}", lArtistId);
              MusicDB.Execute(strSQL);

              // Delete artist info
              strSQL = String.Format("delete from artistinfo where idArtist={0}", lArtistId);
              MusicDB.Execute(strSQL);
            }

            // Check path
            strSQL = String.Format("select * from song where song.idPath={0}", lPathId);
            results = MusicDB.Execute(strSQL);
            if (results.Rows.Count == 0)
            {
              // Delete path with no songs
              strSQL = String.Format("delete from path where idPath={0}", lPathId);
              MusicDB.Execute(strSQL);

              // remove from cache
              //foreach (CPathCache path in _pathCache)
              //{
              //    if (path.idPath == lPathId)
              //    {
              //        int iIndex = _pathCache.IndexOf(path);
              //        if (iIndex != -1)
              //        {
              //            _pathCache.RemoveAt(iIndex);
              //        }
              //    }
              //}

              // remove from cache 
              for (int i = 0 ; i < _pathCache.Count ; i++)
              {
                CPathCache path = (CPathCache)_pathCache[i];
                if (path.idPath == lPathId)
                {
                  _pathCache.RemoveAt(i);
                  break;
                }
              }
            }

            // Check genre
            strSQL = String.Format("select * from song where song.idGenre={0}", lGenreId);
            results = MusicDB.Execute(strSQL);
            if (results.Rows.Count == 0)
            {
              // delete genre with no songs
              strSQL = String.Format("delete from genre where idGenre={0}", lGenreId);
              MusicDB.Execute(strSQL);
            }
          }
        }
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
        if (null == MusicDB)
          return 0;

        string strSQL;
        int NumOfSongs;
        strSQL = String.Format("select count(*) from song where favorite > 0");
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        SQLiteResultSet.Row row = results.Rows[0];
        NumOfSongs = Int32.Parse(row.fields[0]);
        return NumOfSongs;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public double GetAveragePlayCountForArtist(string artist_)
    {
      try
      {
        if (null == MusicDB || artist_.Length == 0)
          return 0;

        string strSQL;
        string strArtist = artist_;
        double AVGPlayCount;

        DatabaseUtility.RemoveInvalidChars(ref strArtist);
        strSQL = String.Format("select avg(iTimesPlayed) from song where strfilename like '%{0}%'", strArtist);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        SQLiteResultSet.Row row = results.Rows[0];
        // needed for any other country with different decimal separator
        //AVGPlayCount = Double.Parse( row.fields[0], NumberStyles.Number, new CultureInfo("en-US"));
        Double.TryParse(row.fields[0], System.Globalization.NumberStyles.Number, new System.Globalization.CultureInfo("en-US"), out AVGPlayCount);
        return AVGPlayCount;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public bool GetRandomSong(ref Song song)
    {
      try
      {
        song.Clear();

        if (null == MusicDB)
          return false;

        PRNG rand = new PRNG();
        string strSQL;
        int maxIDSong, rndIDSong;
        strSQL = String.Format("select * from song ORDER BY idSong DESC LIMIT 1");
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        maxIDSong = DatabaseUtility.GetAsInt(results, 0, "idSong");
        rndIDSong = rand.Next(0, maxIDSong);

        strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and idSong={0}", rndIDSong);

        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          song.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, 0, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, 0, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, 0, "song.iYear");
          song.Title = DatabaseUtility.Get(results, 0, "song.strTitle");
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, 0, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, 0, "song.iRating");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, 0, "song.favorite"))) != 0;
          song.songId = DatabaseUtility.GetAsInt(results, 0, "song.idSong");
          song.artistId = DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, 0, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, 0, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, 0, "path.strPath");
          strFileName += DatabaseUtility.Get(results, 0, "song.strFileName");
          song.FileName = strFileName;
          return true;
        }
        else
        {
          GetRandomSong(ref song);
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

    public bool GetSongByFileName(string strFileName1, ref Song song)
    {
      string strFileName = strFileName1;
      string strPath = String.Empty;
      string strFName = String.Empty;
      try
      {
        song.Clear();

        DatabaseUtility.RemoveInvalidChars(ref strFileName);
        DatabaseUtility.Split(strFileName, out strPath, out strFName);

        if (null == MusicDB)
          return false;

        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        ulong dwCRC = crc.calc(strFileName1);

        string strSQL;
        //strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and dwFileNameCRC like '{0}' and strPath like '{1}'",
        //                      dwCRC,
        //                      strPath);

        strSQL = String.Format("SELECT * FROM song AS s INNER JOIN artist AS a ON s.idArtist = a.idArtist INNER JOIN album AS b ON s.idAlbum = b.idAlbum INNER JOIN genre AS g ON s.idGenre = g.idGenre INNER JOIN path AS p ON s.idPath = p.idPath  where dwFileNameCRC like '{0}' and strPath like '{1}'", dwCRC, strPath);

        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        song.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
        song.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
        song.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
        song.Track = DatabaseUtility.GetAsInt(results, 0, "song.iTrack");
        song.Duration = DatabaseUtility.GetAsInt(results, 0, "song.iDuration");
        song.Year = DatabaseUtility.GetAsInt(results, 0, "song.iYear");
        song.Title = DatabaseUtility.Get(results, 0, "song.strTitle");
        song.TimesPlayed = DatabaseUtility.GetAsInt(results, 0, "song.iTimesPlayed");
        song.Rating = DatabaseUtility.GetAsInt(results, 0, "song.iRating");
        song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, 0, "song.favorite"))) != 0;
        song.songId = DatabaseUtility.GetAsInt(results, 0, "song.idSong");
        song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, 0, "song.favorite"))) != 0;
        song.artistId = DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
        song.albumId = DatabaseUtility.GetAsInt(results, 0, "album.idAlbum");
        song.genreId = DatabaseUtility.GetAsInt(results, 0, "genre.idGenre");
        song.FileName = strFileName1;
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception for string: {2} / path: {3} - err:{0} stack:{1}", ex.Message, ex.StackTrace, strFileName1, strPath);
        Open();
      }

      return false;
    }

    public bool GetSongByTitle(string strTitle1, ref Song song)
    {
      try
      {
        song.Clear();
        string strTitle = strTitle1;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);

        if (null == MusicDB)
          return false;

        string strSQL;
        strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle='{0}'", strTitle);

        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        song.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
        song.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
        song.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
        song.Track = DatabaseUtility.GetAsInt(results, 0, "song.iTrack");
        song.Duration = DatabaseUtility.GetAsInt(results, 0, "song.iDuration");
        song.Year = DatabaseUtility.GetAsInt(results, 0, "song.iYear");
        song.Title = DatabaseUtility.Get(results, 0, "song.strTitle");
        song.TimesPlayed = DatabaseUtility.GetAsInt(results, 0, "song.iTimesPlayed");
        song.Rating = DatabaseUtility.GetAsInt(results, 0, "song.iRating");
        song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, 0, "song.favorite"))) != 0;
        ;
        song.songId = DatabaseUtility.GetAsInt(results, 0, "song.idSong");
        song.artistId = DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
        song.albumId = DatabaseUtility.GetAsInt(results, 0, "album.idAlbum");
        song.genreId = DatabaseUtility.GetAsInt(results, 0, "genre.idGenre");
        string strFileName = DatabaseUtility.Get(results, 0, "path.strPath");
        strFileName += DatabaseUtility.Get(results, 0, "song.strFileName");
        song.FileName = strFileName;
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }
    
    public bool GetSongsByArtist(string strArtist1, ref ArrayList songs)
    {
      try
      {
        string strArtist = strArtist1;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);

        songs.Clear();
        if (null == MusicDB)
          return false;

        string strSQL;
        strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and artist.strArtist like '{0}'", strArtist);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
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
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          song.FileName = strFileName;
          songs.Add(song);
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

    public bool GetSongsByPathId(int nPathId, ref List<Song> songs)
    {
      songs.Clear();
      if (null == MusicDB)
        return false;

      string strSQL;
      strSQL = String.Format("select * from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and path.idPath='{0}'", nPathId);
      SQLiteResultSet results;
      results = MusicDB.Execute(strSQL);
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
        song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
        song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
        song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
        string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
        strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
        song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
        song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
        song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
        song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
        song.FileName = strFileName;
        songs.Add(song);
      }

      return true;
    }

    public bool GetSongsByPath2(string strPath1, ref List<SongMap> songs)
    {
      //Log.Write ("GetSongsByPath2 {0} ",strPath1);

      string strSQL = String.Empty;
      try
      {
        songs.Clear();
        if (strPath1 == null)
          return false;
        if (strPath1.Length == 0)
          return false;
        string strPath = strPath1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        if (null == MusicDB)
          return false;

        strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName,song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and path.strPath like '{0}'", strPath);
        //strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName,song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and path.strPath like '{0}' order by song.iTrack asc", strPath);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          SongMap songmap = new SongMap();
          Song song = new Song();
          song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
          song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.FileName = strFileName;
          songmap.m_song = song;
          songmap.m_strPath = song.FileName;

          songs.Add(songmap);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("GetSongsByPath2:musicdatabase  {0} exception err:{1} stack:{2}", strSQL, ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByArtist(int nArtistId, ref List<Song> songs)
    {
      try
      {
        songs.Clear();

        if (null == MusicDB)
          return false;

        string temp = "select * from song,album,genre,artist,path where song.idPath=path.idPath ";
        temp += "and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist  ";
        temp += "and song.idArtist='{0}' order by strTitle asc";

        string sql = string.Format(temp, nArtistId);
        GetSongsByFilter(sql, out songs, true, true, true, true, true);

        return true;
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByAlbumArtist(int nAlbumArtistId, ref List<Song> songs)
    {
      try
      {
        songs.Clear();

        if (null == MusicDB)
          return false;

        // Get the id of "Various Artists"
        string variousArtists = GUILocalizeStrings.Get(340);
        if (variousArtists.Length == 0)
          variousArtists = "Various Artists";

        string temp = "select distinct album.* from song,album,genre,artist,albumartist,path where song.idPath=path.idPath";
        temp += " and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and song.idAlbumArtist=albumartist.idAlbumArtist";
        temp += " and song.idAlbumArtist='{0}'  order by strAlbum asc";

        string sql = string.Format(temp, nAlbumArtistId);
        GetSongsByFilter(sql, out songs, false, true, false, false, false);

        List<AlbumInfo> albums = new List<AlbumInfo>();
        GetAllAlbums(ref albums);

        foreach (Song song in songs)
        {
          foreach (AlbumInfo album in albums)
          {
            if (song.Album.Equals(album.Album))
            {
              // When we have a Various Artist album, we need to keep the original Artist id, to find songs
              if (album.AlbumArtist == variousArtists)
                song.artistId = nAlbumArtistId;
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

    public bool GetSongsByAlbumArtistAlbum(int nAlbumArtistId, int nAlbumId, ref List<Song> songs)
    {
      try
      {
        songs.Clear();

        if (null == MusicDB)
          return false;

        string temp = "select * from song,album,genre,artist,albumartist,path where song.idPath=path.idPath ";
        temp += "and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and song.idAlbumArtist=albumartist.idAlbumArtist  ";
        temp += "and song.idAlbumArtist='{0}' and  album.idAlbum='{1}'  order by iTrack asc";

        string sql = string.Format(temp, nAlbumArtistId, nAlbumId);
        ModifyAlbumQueryForVariousArtists(ref sql, nAlbumArtistId, nAlbumId);
        GetSongsByFilter(sql, out songs, true, true, true, true, true);

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
    private void ModifyAlbumQueryForVariousArtists(ref string sOrigSql, int albumartistId, int albumId)
    {
      try
      {
        // Replace occurances of multiple space chars with single space
        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\s+");
        string temp = r.Replace(sOrigSql, " ");
        sOrigSql = temp;

        long idVariousArtists = GetVariousArtistsId();

        if (albumartistId == idVariousArtists)
        {
          sOrigSql = sOrigSql.Replace("and song.idAlbumArtist=", "and album.idAlbumArtist=");
        }
      }

      catch { }
    }

    public bool GetSongsByGenre(int nGenreId, ref List<Song> songs)
    {
      try
      {
        songs.Clear();

        if (null == MusicDB)
          return false;

        string temp = "select * from song,album,genre,artist,path where song.idPath=path.idPath ";
        temp += "and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre ";
        temp += "and song.idArtist=artist.idArtist  and  genre.idGenre='{0}'  order by strTitle asc";
        string sql = string.Format(temp, nGenreId);
        GetSongsByFilter(sql, out songs, true, true, true, true, true);

        return true;
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByYear(int nYear, ref List<Song> songs)
    {
      try
      {
        songs.Clear();

        if (null == MusicDB)
          return false;

        string temp = "select * from song,album,genre,artist,path where song.idPath=path.idPath ";
        temp += "and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and ";
        temp += "song.idArtist=artist.idArtist  and  song.iYear='{0}'  order by strTitle asc";

        string sql = string.Format(temp, nYear);
        GetSongsByFilter(sql, out songs, true, true, true, true, true);

        return true;
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByAlbum(string strAlbum1, ref ArrayList songs)
    {
      try
      {
        string strAlbum = strAlbum1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 

        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        songs.Clear();
        if (null == MusicDB)
          return false;

        string strSQL;
        strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and album.strAlbum like '{0}' and path.idPath=song.idPath order by song.iTrack", strAlbum);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
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
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");

          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.FileName = strFileName;

          songs.Add(song);
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

    public bool GetSongsByAlbumID(int albumId, ref List<Song> songs)
    {
      try
      {
        songs.Clear();
        if (null == MusicDB)
          return false;

        string strSQL;
        strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and album.idAlbum={0} and path.idPath=song.idPath order by song.iTrack", albumId);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
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
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");

          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.FileName = strFileName;

          songs.Add(song);
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
    /// <param name="searchKind">0 = starts with, 1 = contains, 2 = ends with, 3 = contains exact</param>
    /// <param name="strTitle1">The song title you're searching</param>
    /// <param name="songs">A list to be filled with results</param>
    /// <returns>Whether search was successful</returns>
    public bool GetSongs(int searchKind, string strTitle1, ref List<GUIListItem> songs)
    {
      try
      {
        songs.Clear();
        string strTitle = strTitle1;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);
        if (null == MusicDB)
          return false;

        string strSQL = String.Empty;
        switch (searchKind)
        {
          case 0:
            strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '{0}%'", strTitle);
            break;
          case 1:
            strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '%{0}%'", strTitle);
            break;
          case 2:
            strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '%{0}'", strTitle);
            break;
          case 3:
            strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '{0}'", strTitle);
            break;
          default:
            return false;
        }

        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          GUIListItem item = new GUIListItem();
          item.IsFolder = false;
          item.Label = MediaPortal.Util.Utils.GetFilename(strFileName);
          item.Label2 = String.Empty;
          item.Label3 = String.Empty;
          item.Path = strFileName;
          item.FileInfo = new FileInformation(strFileName, item.IsFolder);
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          MediaPortal.Util.Utils.SetThumbnails(ref item);
          songs.Add(item);
        }
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
    /// <param name="searchKind">0 = starts with, 1 = contains, 2 = ends with, 3 = contains exact</param>
    /// <param name="strArtist1">The song title you're searching</param>
    /// <param name="artists">An array to be filled with results</param>
    /// <returns>Whether search was successful</returns>
    public bool GetArtists(int searchKind, string strArtist1, ref ArrayList artists)
    {
      try
      {
        artists.Clear();
        string strArtist2 = strArtist1;
        if (null == MusicDB)
          return false;

        string strSQL = String.Empty;
        switch (searchKind)
        {
          case 0:
            strSQL = String.Format("select * from artist where strArtist like '{0}%' ", strArtist2);
            break;
          case 1:
            strSQL = String.Format("select * from artist where strArtist like '%{0}%' ", strArtist2);
            break;
          case 2:
            strSQL = String.Format("select * from artist where strArtist like '%{0}' ", strArtist2);
            break;
          case 3:
            strSQL = String.Format("select * from artist where strArtist like '{0}' ", strArtist2);
            break;
          case 4:
            strArtist2.Replace('ä', '%');
            strArtist2.Replace('ö', '%');
            strArtist2.Replace('ü', '%');
            strArtist2.Replace('/', '%');
            strArtist2.Replace('-', '%');
            strSQL = String.Format("select * from artist where strArtist like '%{0}%' ", strArtist2);
            break;
          default:
            return false;
        }

        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          string strArtist = DatabaseUtility.Get(results, i, "strArtist");
          artists.Add(strArtist);
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

    public bool GetAllArtists(ref ArrayList artists)
    {
      try
      {
        artists.Clear();

        if (null == MusicDB)
          return false;


        string strSQL;
        strSQL = String.Format("select * from artist");
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          string strArtist = DatabaseUtility.Get(results, i, "strArtist");
          artists.Add(strArtist);
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

    public bool GetAllAlbums(ref List<AlbumInfo> albums)
    {
      try
      {
        albums.Clear();
        if (null == MusicDB)
          return false;

        string strSQL;
        strSQL = String.Format("select * from album,albumartist where album.idAlbumArtist=albumartist.idAlbumArtist");
        //strSQL=String.Format("select distinct album.idAlbum, album.idArtist, album.strAlbum, artist.idArtist, artist.strArtist from album,artist,song where song.idArtist=artist.idArtist and song.idAlbum=album.idAlbum");
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          album.AlbumArtist = DatabaseUtility.Get(results, i, "albumartist.strAlbumArtist");
          album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idAlbumArtist");  //album.IdAlbum contains IdAlbumArtist
          albums.Add(album);
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

    public bool GetAlbums(int searchKind, string strAlbum1, ref ArrayList albums)
    {
      try
      {
        string strAlbum = strAlbum1;
        albums.Clear();
        if (null == MusicDB)
          return false;

        string strSQL = "select * from album,albumartist where album.idAlbumArtist=albumartist.idAlbumArtist and album.strAlbum like ";
        switch (searchKind)
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
        //strSQL=String.Format("select distinct album.idAlbum, album.idArtist, album.strAlbum, artist.idArtist, artist.strArtist from album,artist,song where song.idArtist=artist.idArtist and song.idAlbum=album.idAlbum");
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          album.AlbumArtist = DatabaseUtility.Get(results, i, "albumartist.strAlbumArtist");
          album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idAlbumArtist");  //album.IdAlbum contains IdAlbumArtist
          albums.Add(album);
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

    public bool GetVariousArtistsAlbums(ref List<AlbumInfo> albums)
    {
      try
      {
        albums.Clear();
        if (null == MusicDB)
          return false;

        string variousArtists = GUILocalizeStrings.Get(340);

        if (variousArtists.Length == 0)
          variousArtists = "Various Artists";

        long idVariousArtists = GetVariousArtistsId();

        string strSQL;
        strSQL = String.Format("select * from album where album.idAlbumArtist='{0}'", idVariousArtists);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0 ; i < results.Rows.Count ; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idAlbumArtist");  //album.IdAlbum contains IdAlbumArtist
          album.AlbumArtist = variousArtists;
          albums.Add(album);
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

    public string GetAlbumPath(int nAlbumArtistId, int nAlbumId)
    {
      try
      {
        if (null == MusicDB)
          return string.Empty;

        //string sql = string.Format("select * from song, path where song.idPath=path.idPath and song.idArtist='{0}' and  song.idAlbum='{1}'  limit 1", nArtistId, nAlbumId);
        string sql = string.Format("select path.strPath from song,path,album where song.idPath=path.idPath and song.idAlbum=album.idAlbum and album.idAlbumArtist='{0}' and  album.idAlbum='{1}'  limit 1", nAlbumArtistId, nAlbumId);
        SQLiteResultSet results = MusicDB.Execute(sql);

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
        string strSQL = String.Format("update song set iTimesPlayed=0");
        MusicDB.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public bool IncrTop100CounterByFileName(string strFileName1)
    {
      try
      {
        Song song = new Song();
        string strFileName = strFileName1;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        string strPath, strFName;
        DatabaseUtility.Split(strFileName, out strPath, out strFName);
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);

        if (null == MusicDB)
          return false;

        string strSQL;
        ulong dwCRC;
        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        dwCRC = crc.calc(strFileName1);

        strSQL = String.Format("select * from song,path where song.idPath=path.idPath and dwFileNameCRC like '{0}' and strPath like '{1}'",
                            dwCRC,
                            strPath);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        int idSong = DatabaseUtility.GetAsInt(results, 0, "song.idSong");
        int iTimesPlayed = DatabaseUtility.GetAsInt(results, 0, "song.iTimesPlayed");

        strSQL = String.Format("update song set iTimesPlayed={0} where idSong={1}",
                              ++iTimesPlayed, idSong);
        MusicDB.Execute(strSQL);
        Log.Debug("MusicDatabase: increased playcount for song {1} to {0}", Convert.ToString(iTimesPlayed), strFileName1);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public int AddAlbumInfo(AlbumInfo album1)
    {
      string strSQL;
      try
      {
        AlbumInfo album = album1.Clone();
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

        if (null == MusicDB)
          return -1;
        int lGenreId = AddGenre(album1.Genre);
        //int lPathId   = AddPath(album1.Path);
        int lArtistId = AddArtist(album1.Artist);
        int lAlbumArtistId = AddAlbumArtist(album1.AlbumArtist);
        int lAlbumId = AddAlbum(album1.Album, /*lArtistId,*/ lAlbumArtistId);

        strSQL = String.Format("delete  from albuminfo where idAlbum={0} ", lAlbumId);
        MusicDB.Execute(strSQL);

        strSQL = String.Format("insert into albuminfo (idAlbumInfo,idAlbum,idArtist,idGenre,strTones,strStyles,strReview,strImage,iRating,iYear,strTracks) values(NULL,{0},{1},{2},'{3}','{4}','{5}','{6}',{7},{8},'{9}' )",
                            lAlbumId, lArtistId, lGenreId,
                            album.Tones,
                            album.Styles,
                            album.Review,
                            album.Image,
                            album.Rating,
                            album.Year,
                            album.Tracks);
        MusicDB.Execute(strSQL);

        int lAlbumInfoId = MusicDB.LastInsertID();
        return lAlbumInfoId;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return -1;

    }

    public int AddArtistInfo(ArtistInfo artist1)
    {
      string strSQL;
      try
      {
        ArtistInfo artist = artist1.Clone();
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

        if (null == MusicDB)
          return -1;
        int lArtistId = AddArtist(artist.Artist);

        //strSQL=String.Format("delete artistinfo where idArtist={0} ", lArtistId);
        //m_db.Execute(strSQL);
        strSQL = String.Format("select * from artistinfo where idArtist={0}", lArtistId);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          strSQL = String.Format("delete artistinfo where idArtist={0} ", lArtistId);
          MusicDB.Execute(strSQL);
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
        MusicDB.Execute(strSQL);

        int lArtistInfoId = MusicDB.LastInsertID();
        return lArtistInfoId;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return -1;

    }

    public void DeleteAlbumInfo(string strAlbumName1)
    {
      string strAlbum = strAlbumName1;
      DatabaseUtility.RemoveInvalidChars(ref strAlbum);
      string strSQL = String.Format("select * from albuminfo,album where albuminfo.idAlbum=album.idAlbum and album.strAlbum like '{0}'", strAlbum);
      SQLiteResultSet results;
      results = MusicDB.Execute(strSQL);
      if (results.Rows.Count != 0)
      {
        int iAlbumId = DatabaseUtility.GetAsInt(results, 0, "albuminfo.idAlbum");
        strSQL = String.Format("delete from albuminfo where albuminfo.idAlbum={0}", iAlbumId);
        MusicDB.Execute(strSQL);
      }
    }

    public void DeleteArtistInfo(string strArtistName1)
    {
      string strArtist = strArtistName1;
      DatabaseUtility.RemoveInvalidChars(ref strArtist);
      string strSQL = String.Format("select * from artist where artist.strArtist like '{0}'", strArtist);
      SQLiteResultSet results;
      results = MusicDB.Execute(strSQL);
      if (results.Rows.Count != 0)
      {
        int iArtistId = DatabaseUtility.GetAsInt(results, 0, "idArtist");
        strSQL = String.Format("delete from artistinfo where artistinfo.idArtist={0}", iArtistId);
        MusicDB.Execute(strSQL);
      }
    }

    public bool GetAlbumInfo(int albumId, ref AlbumInfo album)
    {
      try
      {
        string strSQL;
        strSQL = String.Format("select * from albuminfo,album,genre,artist where albuminfo.idAlbum=album.idAlbum and albuminfo.idGenre=genre.idGenre and albuminfo.idArtist=artist.idArtist and album.idAlbum ={0}", albumId);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          album.Rating = DatabaseUtility.GetAsInt(results, 0, "albuminfo.iRating");
          album.Year = DatabaseUtility.GetAsInt(results, 0, "albuminfo.iYear");
          album.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
          album.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
          album.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
          album.Image = DatabaseUtility.Get(results, 0, "albuminfo.strImage");
          album.Review = DatabaseUtility.Get(results, 0, "albuminfo.strReview");
          album.Styles = DatabaseUtility.Get(results, 0, "albuminfo.strStyles");
          album.Tones = DatabaseUtility.Get(results, 0, "albuminfo.strTones");
          album.Tracks = DatabaseUtility.Get(results, 0, "albuminfo.strTracks");
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

    public bool GetArtistInfo(string strArtist1, ref ArtistInfo artist)
    {
      try
      {
        string strArtist = strArtist1;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);
        string strSQL;
        strSQL = String.Format("select * from artist,artistinfo where artist.idArtist=artistinfo.idArtist and artist.strArtist like '{0}'", strArtist);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          artist.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
          artist.Born = DatabaseUtility.Get(results, 0, "artistinfo.strBorn");
          artist.YearsActive = DatabaseUtility.Get(results, 0, "artistinfo.strYearsActive");
          artist.Genres = DatabaseUtility.Get(results, 0, "artistinfo.strGenres");
          artist.Styles = DatabaseUtility.Get(results, 0, "artistinfo.strStyles");
          artist.Tones = DatabaseUtility.Get(results, 0, "artistinfo.strTones");
          artist.Instruments = DatabaseUtility.Get(results, 0, "artistinfo.strInstruments");
          artist.Image = DatabaseUtility.Get(results, 0, "artistinfo.strImage");
          artist.AMGBio = DatabaseUtility.Get(results, 0, "artistinfo.strAMGBio");
          artist.Albums = DatabaseUtility.Get(results, 0, "artistinfo.strAlbums");
          artist.Compilations = DatabaseUtility.Get(results, 0, "artistinfo.strCompilations");
          artist.Singles = DatabaseUtility.Get(results, 0, "artistinfo.strSingles");
          artist.Misc = DatabaseUtility.Get(results, 0, "artistinfo.strMisc");
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
    
    public int GetArtistId(string strArtist)
    {
      try
      {
        if (MusicDB == null)
          return -1;

        string strSQL;
        strSQL = String.Format("select distinct artist.idArtist from artist where artist.strArtist='{0}'", strArtist);
        SQLiteResultSet results;
        results = MusicDB.Execute(strSQL);
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
              lAlbumArtistId = AddAlbumArtist(variousArtists);
              bVarious = true;
              break;
            }
          }
        }

        if (bVarious)
        {
          string strSQL;
          strSQL = String.Format("update album set idAlbumArtist={0} where idAlbum={1}", lAlbumArtistId, album.idAlbum);
          MusicDB.Execute(strSQL);
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
    public void CheckVariousArtists(string strAlbum)
    {
      int lAlbumArtistId = 0;
      int lAlbumId = 0;
      bool bVarious = false;
      ArrayList songs = new ArrayList();
      GetSongsByAlbum(strAlbum, ref songs);
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
            lAlbumArtistId = AddAlbumArtist(variousArtists);
            lAlbumId = song.albumId;
            bVarious = true;
            break;
          }
        }

        if (bVarious)
        {
          string strSQL;
          strSQL = String.Format("update album set idAlbumArtist={0} where idAlbum={1}", lAlbumArtistId, lAlbumId);
          MusicDB.Execute(strSQL);
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

        idVariousArtists = AddAlbumArtist(variousArtists);
      }

      return idVariousArtists;
    }
  }
}