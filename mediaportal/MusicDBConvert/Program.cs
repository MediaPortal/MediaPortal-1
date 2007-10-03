using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MediaPortal.Database;
using MediaPortal.Music.Database;
using SQLite.NET;

namespace MusicDBConvert
{
  class Program
  {
    static SQLiteClient MusicDbClient;
    static MediaPortal.Music.Database.MusicDatabase m_dbs; 

    static void Main(string[] args)
    {
      if (File.Exists(@"Database\MusicDatabaseV10.db3"))
      {
        Console.WriteLine("Attention: New database already exists.");
        Console.WriteLine("");
        Console.WriteLine("Do You really want to continue? [y/N]");
        ConsoleKeyInfo answer = Console.ReadKey();
        if (answer.Key != ConsoleKey.Y)
          return;
      }


      Console.WriteLine("Opening / Creating New MusicDatabse. This may take a while");
      try
      {
        m_dbs = MediaPortal.Music.Database.MusicDatabase.Instance;
        MusicDbClient = new SQLiteClient(@"Database\MusicDatabaseV10.db3");
      }
      catch (Exception ex)
      { }

      int oldDbVersion = 0;
      if (File.Exists(@"Database\MusicDatabaseV9.db3"))
        oldDbVersion = 9;
      else if (File.Exists(@"Database\MusicDatabaseV8.db3"))
        oldDbVersion = 8;

      if (oldDbVersion > 0)
      {
        if (UpdateDB_to_V10(oldDbVersion))
        {
          Console.WriteLine("MusicDatabase: Old database successfully updated");
        }
        else
        {
          Console.WriteLine("MusicDatabase: Error while trying to update your database to V10. Please delete the database and start from scratch");
        }
      }
    }

    static private bool UpdateDB_to_V10(int oldDbVersion)
    {
      try
      {
        bool success = true;
        string database = String.Format(@"Database\MusicDatabaseV{0}.db3", oldDbVersion);

        string strTitle, strPath, strFileName, strAlbum, strGenre, strArtist, strSortName, strAlbumArtist;
        int iTrack, iDuration, iYear, iTimesPlayed, iRating, iFavorite, inumArtists;
        DateTime dateadded;

        SQLiteClient update_db = new SQLiteClient(database);

        DatabaseUtility.SetPragmas(update_db);
        SQLiteResultSet results;


        string strSQL = String.Empty;
        if (oldDbVersion == 8)
          strSQL = "select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist";
        else
          strSQL = "select * from song,album,genre,artist,albumartist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and song.idAlbumArtist=albumartist.idAlbumArtist";

        results = update_db.Execute(strSQL);
        
        if (results.Rows.Count < 1)
          return false;

        Console.WriteLine("Songs to convert: {0}", results.Rows.Count);
        Console.Write("Converting song: ");
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          strTitle = DatabaseUtility.Get(results, i, "strTitle");
          strPath = DatabaseUtility.Get(results, i, "strPath");
          strFileName = DatabaseUtility.Get(results, i, "strFileName");
          strPath += strFileName;
          strAlbum = DatabaseUtility.Get(results, i, "strAlbum");
          strGenre = DatabaseUtility.Get(results, i, "strGenre");
          strArtist = DatabaseUtility.Get(results, i, "strArtist");
          strSortName = DatabaseUtility.Get(results, i, "strSortName");
          iTrack = DatabaseUtility.GetAsInt(results, i, "iTrack");
          iDuration = DatabaseUtility.GetAsInt(results, i, "iDuration");
          iYear = DatabaseUtility.GetAsInt(results, i, "iYear");
          iTimesPlayed = DatabaseUtility.GetAsInt(results, i, "iTimesPlayed");
          iRating = DatabaseUtility.GetAsInt(results, i, "iRating");
          iFavorite = DatabaseUtility.GetAsInt(results, i, "favorite");
          inumArtists = DatabaseUtility.GetAsInt(results, i, "inumArtists");
          dateadded = DatabaseUtility.GetAsDateTime(results, i, "dateadded");

          strAlbumArtist = strArtist;
          if (oldDbVersion == 9)
          {
            string strTmp = DatabaseUtility.Get(results, i, "strAlbumArtist");
            if (strTmp == strArtist)
              strAlbumArtist = strArtist;
            else
              strAlbumArtist = strTmp;
          }
          else if (inumArtists > 1)
            strAlbumArtist = "Various Artists";

          DatabaseUtility.RemoveInvalidChars(ref strTitle);
          DatabaseUtility.RemoveInvalidChars(ref strPath);
          DatabaseUtility.RemoveInvalidChars(ref strAlbum);
          DatabaseUtility.RemoveInvalidChars(ref strGenre);
          DatabaseUtility.RemoveInvalidChars(ref strArtist);
          DatabaseUtility.RemoveInvalidChars(ref strSortName);
          DatabaseUtility.RemoveInvalidChars(ref strAlbumArtist);

          strSQL = String.Format("insert into tracks ( " +
                     "strPath, strArtist, strArtistSortName, strAlbumArtist, strAlbum, strGenre, " +
                     "strTitle, iTrack, iNumTracks, iDuration, iYear, iTimesPlayed, iRating, iFavorite, " +
                     "iResumeAt, iDisc, iNumDisc, iGainTrack, iPeakTrack, strLyrics, musicBrainzID, dateLastPlayed, dateadded) " +
                     "values ( " +
                     "'{0}', '{1}', '{2}', '{3}', '{4}', '{5}', " +
                     "'{6}', {7}, {8}, {9}, {10}, {11}, {12}, {13}, " +
                     "{14}, {15}, {16}, {17}, {18}, '{19}', '{20}', '{21}', '{22}' )",
                     strPath, strArtist, strSortName, strAlbumArtist, strAlbum, strGenre,
                     strTitle, iTrack, 0, iDuration, iYear, iTimesPlayed, iRating, iFavorite,
                     0, 0, 0, 0, 0, "", "", DateTime.MinValue, dateadded
                     );

          MusicDbClient.Execute(strSQL);

          AddArtist(strArtist);
          AddAlbumArtist(strAlbumArtist);
          AddGenre(strGenre);

          string msg = String.Format("{0}", i);
          Console.Write(msg);
          Console.SetCursorPosition(Console.CursorLeft - msg.Length, Console.CursorTop);
        }
        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);
        Console.WriteLine(" ");
        Console.WriteLine("Finished converting songs");

        Console.WriteLine("Converting ArtistInfo");

        strSQL = "select * from artist, artistinfo where artistinfo.idartist = artist.idartist";
        results = update_db.Execute(strSQL);

        if (results.Rows.Count < 1)
          return true;

        string strBorn, strYearsActive, strGenres, strTones, strStyles, strInstruments,  strImage, strAMGBio, strAlbums, strCompilations, strSingles, strMisc;
        Console.WriteLine("ArtistInfo to convert: {0}", results.Rows.Count);
        Console.Write("Converting ArtistInfo: ");
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          strArtist = DatabaseUtility.Get(results, i, "strArtist");
          strBorn = DatabaseUtility.Get(results, i, "strBorn");
          strYearsActive = DatabaseUtility.Get(results, i, "strYearsActive");
          strGenres = DatabaseUtility.Get(results, i, "strGenres");
          strTones = DatabaseUtility.Get(results, i, "strTones");
          strStyles = DatabaseUtility.Get(results, i, "strStyles");
          strInstruments = DatabaseUtility.Get(results, i, "strInstruments");
          strImage = DatabaseUtility.Get(results, i, "strImage");
          strAMGBio = DatabaseUtility.Get(results, i, "strAmgBio");
          strAlbums = DatabaseUtility.Get(results, i, "strAlbums");
          strCompilations = DatabaseUtility.Get(results, i, "strCompilations");
          strSingles = DatabaseUtility.Get(results, i, "strSingles");
          strMisc = DatabaseUtility.Get(results, i, "strMisc");

          DatabaseUtility.RemoveInvalidChars(ref strArtist);
          DatabaseUtility.RemoveInvalidChars(ref strBorn);
          DatabaseUtility.RemoveInvalidChars(ref strYearsActive);
          DatabaseUtility.RemoveInvalidChars(ref strGenres);
          DatabaseUtility.RemoveInvalidChars(ref strTones);
          DatabaseUtility.RemoveInvalidChars(ref strStyles);
          DatabaseUtility.RemoveInvalidChars(ref strInstruments);
          DatabaseUtility.RemoveInvalidChars(ref strImage);
          DatabaseUtility.RemoveInvalidChars(ref strAMGBio);
          DatabaseUtility.RemoveInvalidChars(ref strAlbums);
          DatabaseUtility.RemoveInvalidChars(ref strCompilations);
          DatabaseUtility.RemoveInvalidChars(ref strSingles);
          DatabaseUtility.RemoveInvalidChars(ref strMisc);

          strSQL = String.Format("insert into artistinfo ( " +
                     "strArtist, strBorn, strYearsActive, strGenres, strTones, strStyles, " +
                     "strInstruments, strImage, strAmgBio, strAlbums, strCompilations, strSingles, strMisc) " +
                     "values ( " +
                     "'{0}', '{1}', '{2}', '{3}', '{4}', '{5}', " +
                     "'{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}' )", 
                     strArtist, strBorn, strYearsActive, strGenres, strTones, strStyles,
                     strInstruments, strImage, strAMGBio, strAlbums, strCompilations, strSingles, strMisc
                     );

          MusicDbClient.Execute(strSQL);

          string msg = String.Format("{0}", i);
          Console.Write(msg);
          Console.SetCursorPosition(Console.CursorLeft - msg.Length, Console.CursorTop);
        }

        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);
        Console.WriteLine("Finished converting ARtistInfo");

        Console.WriteLine("Converting AlbumInfo");

        strSQL = "select * from artist, album, albuminfo where albuminfo.idartist = artist.idartist and albuminfo.idalbum = album.idalbum";
        results = update_db.Execute(strSQL);

        if (results.Rows.Count < 1)
          return true;

        string strReview, strTracks;
        Console.WriteLine("AlbumInfo to convert: {0}", results.Rows.Count);
        Console.Write("Converting AlbumInfo: ");
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          strArtist = DatabaseUtility.Get(results, i, "strArtist");
          strAlbum = DatabaseUtility.Get(results, i, "strAlbum");
          strTones = DatabaseUtility.Get(results, i, "strTones");
          strStyles = DatabaseUtility.Get(results, i, "strStyles");
          strReview = DatabaseUtility.Get(results, i, "strReview");
          strImage = DatabaseUtility.Get(results, i, "strImage");
          strTracks = DatabaseUtility.Get(results, i, "strTracks");
          iYear = DatabaseUtility.GetAsInt(results, i, "iYear");
          iRating = DatabaseUtility.GetAsInt(results, i, "iRating");

          DatabaseUtility.RemoveInvalidChars(ref strArtist);
          DatabaseUtility.RemoveInvalidChars(ref strAlbum);
          DatabaseUtility.RemoveInvalidChars(ref strTones);
          DatabaseUtility.RemoveInvalidChars(ref strStyles);
          DatabaseUtility.RemoveInvalidChars(ref strReview);
          DatabaseUtility.RemoveInvalidChars(ref strImage);
          DatabaseUtility.RemoveInvalidChars(ref strTracks);

          strSQL = String.Format("insert into albuminfo ( " +
                     "strArtist, strAlbum, strTones, strStyles, " +
                     "strReview, strImage, strtracks, iYear, iRating) " +
                     "values ( " +
                     "'{0}', '{1}', '{2}', '{3}', '{4}', '{5}', " +
                     "'{6}', '{7}', '{8}', {9}, {10} )",
                     strArtist, strAlbum, strTones, strStyles,
                     strReview, strImage, strTracks, iYear, iRating
                     );

          MusicDbClient.Execute(strSQL);

          string msg = String.Format("{0}", i);
          Console.Write(msg);
          Console.SetCursorPosition(Console.CursorLeft - msg.Length, Console.CursorTop);
        }
        update_db.Close();
        return success;
      }
      catch (Exception ex)
      {
        Console.WriteLine("musicdatabase: Error updating V8 to V9: {0} stack: {1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    /// <summary>
    /// Add the artist to the Artist table, to allow us having mutiple artists per song
    /// </summary>
    /// <param name="strArtist"></param>
    /// <returns></returns>
    static private void AddArtist(string strArtist)
    {
      try
      {
        if (null == MusicDbClient)
          return;

        string strSQL;
        SQLiteResultSet results;

        // split up the artist, in case we've got multiple artists
        string[] artists = strArtist.Split(new char[] { ';', '|' });
        foreach (string artist in artists)
        {
          strSQL = String.Format("select idArtist from artist where strArtist = '{0}'", strArtist.Trim());
          results = MusicDbClient.Execute(strSQL);
          if (results.Rows.Count < 1)
          {
            // Insert the Artist
            strSQL = String.Format("insert into artist (strArtist) values ('{0}')", artist.Trim());
            MusicDbClient.Execute(strSQL);
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Console.WriteLine("Musicdatabase Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return;
    }

    /// <summary>
    /// Add the albumartist to the AlbumArtist table, to allow us having mutiple artists per song
    /// </summary>
    /// <param name="strArtist"></param>
    /// <returns></returns>
    static private void AddAlbumArtist(string strAlbumArtist)
    {
      try
      {
        if (null == MusicDbClient)
          return;

        string strSQL;
        SQLiteResultSet results;

        // split up the albumartist, in case we've got multiple albumartists
        string[] artists = strAlbumArtist.Split(new char[] { ';', '|' });
        foreach (string artist in artists)
        {
          strSQL = String.Format("select idAlbumArtist from albumartist where strAlbumArtist = '{0}'", artist.Trim());
          results = MusicDbClient.Execute(strSQL);
          if (results.Rows.Count < 1)
          {
            // Insert the AlbumArtist
            strSQL = String.Format("insert into albumartist (strAlbumArtist) values ('{0}')", artist.Trim());
            MusicDbClient.Execute(strSQL);
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Console.WriteLine("Musicdatabase Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return;
    }

    /// <summary>
    /// Add the genre to the Genre Table, to allow maultiple Genres per song
    /// </summary>
    /// <param name="strGenre"></param>
    static private void AddGenre(string strGenre)
    {
      try
      {
        string strSQL;
        SQLiteResultSet results;

        // split up the artist, in case we've got multiple artists
        string[] genres = strGenre.Split(new char[] { ';', '|' });
        foreach (string genre in genres)
        {
          strSQL = String.Format("select idGenre from genre where upper(strGenre) = '{0}'", genre.Trim().ToUpperInvariant());
          results = MusicDbClient.Execute(strSQL);
          if (results.Rows.Count < 1)
          {
            // Insert the Genre
            strSQL = String.Format("insert into genre (strGenre) values ('{0}')", genre.Trim());
            MusicDbClient.Execute(strSQL);
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Console.WriteLine("Musicdatabase Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return;
    }
  }
}
