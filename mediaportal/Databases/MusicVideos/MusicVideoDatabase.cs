#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Database;
using SQLite.NET;
using System.Xml;
using System.IO;
using MediaPortal.MusicVideos;
using MediaPortal.Utils.Services;

namespace MediaPortal.MusicVideos.Database
{
  public class MusicVideoDatabase
  {
    public enum MusicVideoTypes : int
    {
      TOP100, NEW, GENRE, FAVORITE,
    };
    private SQLiteClient m_db;

    private static MusicVideoDatabase Instance;
    private ILog moLog;
    private IConfig _config;

    private MusicVideoDatabase()
    {
      ServiceProvider loServices = GlobalServiceProvider.Instance;
      moLog = loServices.Get<ILog>();
      _config = loServices.Get<IConfig>();
      bool dbExists;
      try
      {
        // Open database
        try
        {
          System.IO.Directory.CreateDirectory("database");
        }
        catch (Exception) { }
        dbExists = System.IO.File.Exists(_config.Get(Config.Options.DatabasePath) + "MusicVideoDatabaseV3.db3");
        m_db = new SQLiteClient(_config.Get(Config.Options.DatabasePath) + "MusicVideoDatabaseV3.db3");

        MediaPortal.Database.DatabaseUtility.SetPragmas(m_db);
         
        if (!dbExists)
        {
          CreateTables();
        }
        if (arePlaylistTablesCreated() == false)
        {
            createPlayListTables();
        }
      }
      catch (SQLiteException ex)
      {
        moLog.Info("database exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public static MusicVideoDatabase getInstance()
    {
      if (Instance == null)
      {
        Instance = new MusicVideoDatabase();
      }
      return Instance;
    }
      private bool arePlaylistTablesCreated()
      {
          if(m_db==null){
            return false;
          }
          SQLiteResultSet loResultSet = m_db.Execute("SELECT name FROM sqlite_master WHERE type='table' AND name='PLAYLIST'");
          if (loResultSet.Rows.Count > 0) return true;
          return false;
      }
      private void createPlayListTables()
      {
          if (m_db == null)
          {
              return;
          }
          try
          {
              m_db.Execute("CREATE TABLE PLAYLIST_VIDEOS(PLAY_INDX integer,SONG_NM text,SONG_ID text,ARTIST_NM text,ARTIST_ID text,COUNTRY text,PLAYLIST_ID integer)\n");
              m_db.Execute("CREATE TABLE PLAYLIST(PLAYLIST_ID integer primary key,PLAYLIST_NM text)\n");
          }
          catch (Exception e)
          {
              moLog.Info(e.ToString());
          }

      }
    private void CreateTables()
    {
      if (m_db == null)
      {
        return;
      }
      try
      {
        m_db.Execute("CREATE TABLE FAVORITE_VIDEOS(SONG_NM text,SONG_ID text,ARTIST_NM text,ARTIST_ID text,COUNTRY text,FAVORITE_ID integer)\n");
        m_db.Execute("CREATE TABLE FAVORITE(FAVORITE_ID integer primary key,FAVORITE_NM text)\n");
        //m_db.Execute("CREATE TABLE MUSIC_VDO_CAT(MUSIC_VDO_CAT_ID integer primary key,MUSIC_VDO_CAT_NM text,CTRY_NM text,MUSIC_VDO_TYP_ID integer)\n");
        //m_db.Execute("CREATE TABLE MUSIC_VDO_TYP(MUSIC_VDO_TYP_ID integer primary key,MUSIC_VDO_TYP_NM text)\n");
        //m_db.Execute("CREATE TABLE MUSIC_VDO(MUSIC_VDO_ID integer primary key,MUSIC_VDO_SONG_NM text,MUSIC_VDO_SONG_ID text,MUSIC_VDO_ARTIST_NM text,MUSIC_VDO_ARTIST_ID text,MUSIC_VDO_CAT_ID integer)\n");
        //addVideoType("TOP100");
        //addVideoType("NEW");
        //addVideoType("GENRE");
        //addVideoType("FAVORITE");
        //createVideoCat("Default", MusicVideoTypes.FAVORITE, "");
        List<YahooVideo> loFavoriteVideos = parseOldFavoriteFile();
        createFavorite("Default");
        if (loFavoriteVideos.Count > 0)
        {

          foreach (YahooVideo loVideo in loFavoriteVideos)
          {
            addFavoriteVideo("Default", loVideo);
          }
        }
      }
      catch (Exception e)
      {
        moLog.Info(e.ToString());
      }
    }

    public bool createFavorite(string fsName)
    {
        ArrayList loFavList = getFavorites();
        foreach (String lsFavName in loFavList)
        {
            if (lsFavName.Equals(fsName))
            {
                return false;
            }
        }
      string lsSQL = String.Format("insert into FAVORITE(FAVORITE_NM) VALUES('{0}')", fsName.Replace("'", "''"));
      m_db.Execute(lsSQL);
      if (m_db.ChangedRows() > 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    }
      
    public bool DeleteFavorite(string fsName)
    {
      string lsSQL = String.Format(" delete from FAVORITE where FAVORITE_NM='{0}'", fsName.Replace("'", "''"));
      m_db.Execute(lsSQL);
      if (m_db.ChangedRows() > 0)
      {
        return true;
      }
      else
      {
        return false;
      }

    }
      

    public bool updateFavorite(string fsOldName, String fsNewName)
    {
      string lsSQL = String.Format("update FAVORITE set FAVORITE_NM = '{0}' where FAVORITE_NM= '{1}'", fsNewName.Replace("'", "''"), fsOldName.Replace("'", "''"));
      m_db.Execute(lsSQL);
      if (m_db.ChangedRows() > 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool addFavoriteVideo(string fsFavoriteNm, YahooVideo foVideo)
    {
      //get the favorite id
      string lsSQL = String.Format("select FAVORITE_ID from FAVORITE where FAVORITE_NM='{0}'", fsFavoriteNm.Replace("'", "''"));
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);

      string lsFavID = (String)loResultSet.GetColumn(0)[0];

     //check if the video is already in the favorite list
      lsSQL = string.Format("select SONG_ID from FAVORITE_VIDEOS where SONG_ID='{0}' AND COUNTRY='{1}' and FAVORITE_ID=''", foVideo.songId, foVideo.countryId, lsFavID);
      loResultSet = m_db.Execute(lsSQL);
      if (loResultSet.Rows.Count > 0)
      {
          return false;
      }
      lsSQL = string.Format("insert into FAVORITE_VIDEOS(SONG_NM,SONG_ID,ARTIST_NM,ARTIST_ID,COUNTRY,FAVORITE_ID)VALUES('{0}','{1}','{2}','{3}','{4}','{5}')", foVideo.songName.Replace("'", "''"), foVideo.songId, foVideo.artistName.Replace("'", "''"), foVideo.artistId, foVideo.countryId, lsFavID);
      m_db.Execute(lsSQL);
      if (m_db.ChangedRows() > 0)
      {
        return true;
      }
      else
      {
        return false;
      }

    }
      

    public bool removeFavoriteVideo(YahooVideo foVideo, string fsFavoriteNm)
    {
      string lsSQL = String.Format("select FAVORITE_ID from FAVORITE where FAVORITE_NM='{0}'", fsFavoriteNm.Replace("'", "''"));
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);

      string lsFavID = (String)loResultSet.GetColumn(0)[0];
      //moLog.Info("fav id = {0}",lsFavID);
      //moLog.Info("song id = {0}", foVideo.songId);
      lsSQL = string.Format("delete from FAVORITE_VIDEOS where SONG_ID='{0}' and FAVORITE_ID = {1}", foVideo.songId, lsFavID);
      m_db.Execute(lsSQL);
      if (m_db.ChangedRows() > 0)
      {
        return true;
      }
      else
      {
        return false;
      }

    }
     

    public ArrayList getFavorites()
    {
      //createFavorite("Default2");
      string lsSQL = string.Format("select favorite_nm from favorite");
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
      return loResultSet.GetColumn(0);
    }

    
    public List<YahooVideo> getFavoriteVideos(string fsFavoriteNm)
    {
      List<YahooVideo> loFavoriteList = new List<YahooVideo>();
      string lsSQL = String.Format("select FAVORITE_ID from FAVORITE where FAVORITE_NM='{0}'", fsFavoriteNm.Replace("'", "''"));
      SQLiteResultSet loResultSet = m_db.Execute(lsSQL);

      string lsFavID = (String)loResultSet.GetColumn(0)[0];
      lsSQL = string.Format("select SONG_NM,SONG_ID,ARTIST_NM,ARTIST_ID,COUNTRY from FAVORITE_VIDEOS where FAVORITE_ID={0}", lsFavID);
      loResultSet = m_db.Execute(lsSQL);

      foreach (ArrayList loRow in loResultSet.RowsList)
      {
        YahooVideo loVideo = new YahooVideo();
        IEnumerator en = loRow.GetEnumerator();
        en.MoveNext();
        loVideo.songName = (String)en.Current;
        en.MoveNext();
        loVideo.songId = (String)en.Current;
        en.MoveNext();
        loVideo.artistName = (String)en.Current;
        en.MoveNext();
        loVideo.artistId = (String)en.Current;
        en.MoveNext();
        loVideo.countryId = (String)en.Current;
        loFavoriteList.Add(loVideo);

      }

      return loFavoriteList;
    }
      private List<YahooVideo> parseOldFavoriteFile()
      {
          XmlTextReader loXmlreader = null;
          List<YahooVideo> loFavoriteList = new List<YahooVideo>();
          //moFavoriteTable = new Dictionary<string,List<YahooVideo>>();
          //string lsCurrentName = msDefaultFavoriteName;
          try
          {
            loXmlreader = new XmlTextReader(_config.Get(Config.Options.ConfigPath) + "MusicVideoFavorites.xml");
              YahooVideo loVideo;

              while (loXmlreader.Read())
              {
                  if (loXmlreader.NodeType == XmlNodeType.Element && loXmlreader.Name == "SONG")
                  {
                      loVideo = new YahooVideo();
                      loVideo.artistId = loXmlreader.GetAttribute("ArtistId");
                      loVideo.artistName = loXmlreader.GetAttribute("Artist");
                      loVideo.songId = loXmlreader.GetAttribute("SongId");
                      loVideo.songName = loXmlreader.GetAttribute("SongTitle");
                      loVideo.countryId = loXmlreader.GetAttribute("CtryId");
                      moLog.Info("found favorite:{0}", loVideo.ToString());
                      loFavoriteList.Add(loVideo);
                  }
              }
              loXmlreader.Close();
          }
          catch (Exception e) { moLog.Info(e.ToString()); }
          finally
          {
              loXmlreader.Close();
              moLog.Info("old parse closed.");
          }
          return loFavoriteList;
      }

    public bool addPlayListVideo(string fsPlaylistNm, YahooVideo foVideo, int fiIndex)
    {
        //get the favorite id
        string lsSQL = String.Format("select PLAYLIST_ID from PLAYLIST where PLAYLIST_NM='{0}'", fsPlaylistNm.Replace("'", "''"));
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);

        string lsPlaylistID = (String)loResultSet.GetColumn(0)[0];

        lsSQL = string.Format("insert into PLAYLIST_VIDEOS(PLAY_INDX,SONG_NM,SONG_ID,ARTIST_NM,ARTIST_ID,COUNTRY,PLAYLIST_ID)VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", fiIndex, foVideo.songName.Replace("'", "''"), foVideo.songId, foVideo.artistName.Replace("'", "''"), foVideo.artistId, foVideo.countryId, lsPlaylistID);
        m_db.Execute(lsSQL);
        if (m_db.ChangedRows() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public bool removePlaylistVideo(YahooVideo foVideo, string fsPlaylistNm)
    {
        string lsSQL = String.Format("select PLAYLIST_ID from PLAYLIST where PLAYLIST_NM='{0}'", fsPlaylistNm.Replace("'", "''"));
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);

        string lsPlaylistID = (String)loResultSet.GetColumn(0)[0];
        //moLog.Info("fav id = {0}",lsFavID);
        //moLog.Info("song id = {0}", foVideo.songId);
        lsSQL = string.Format("delete from PLAYLIST_VIDEOS where SONG_ID='{0}' and PLAYLIST_ID = {1}", foVideo.songId, lsPlaylistID);
        m_db.Execute(lsSQL);
        if (m_db.ChangedRows() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public ArrayList getPlaylists()
    {
        //createFavorite("Default2");
        string lsSQL = string.Format("select playlist_nm from playlist");
        SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
        return loResultSet.GetColumn(0);
    }

      public List<YahooVideo> getPlayListVideos(string fsPlaylistNm)
      {
          List<YahooVideo> loPlaylist = new List<YahooVideo>();
          string lsSQL = String.Format("select PLAYLIST_ID from PLAYLIST where PLAYLIST_NM='{0}'", fsPlaylistNm.Replace("'", "''"));
          SQLiteResultSet loResultSet = m_db.Execute(lsSQL);

          string lsPlaylistID = (String)loResultSet.GetColumn(0)[0];
          lsSQL = string.Format("select SONG_NM,SONG_ID,ARTIST_NM,ARTIST_ID,COUNTRY from PLAYLIST_VIDEOS where PLAYLIST_ID={0} order by PLAY_INDX", lsPlaylistID);
          loResultSet = m_db.Execute(lsSQL);

          foreach (ArrayList loRow in loResultSet.RowsList)
          {
              YahooVideo loVideo = new YahooVideo();
              IEnumerator en = loRow.GetEnumerator();
              en.MoveNext();
              loVideo.songName = (String)en.Current;
              en.MoveNext();
              loVideo.songId = (String)en.Current;
              en.MoveNext();
              loVideo.artistName = (String)en.Current;
              en.MoveNext();
              loVideo.artistId = (String)en.Current;
              en.MoveNext();
              loVideo.countryId = (String)en.Current;
              loPlaylist.Add(loVideo);

          }

          return loPlaylist;
      }
      public bool DeletePlaylist(string fsName)
      {
          string lsSQL = String.Format("select PLAYLIST_ID from PLAYLIST where PLAYLIST_NM='{0}'", fsName.Replace("'", "''"));
          SQLiteResultSet loResultSet = m_db.Execute(lsSQL);
          if (loResultSet.RowsList.Count == 0) { return true; }
          string lsPlaylistID = (String)loResultSet.GetColumn(0)[0];
          lsSQL = String.Format(" delete from PLAYLIST where PLAYLIST_NM='{0}'", fsName.Replace("'", "''"));
          m_db.Execute(lsSQL);
          if (m_db.ChangedRows() > 0)
          {
              lsSQL = String.Format(" delete from PLAYLIST_VIDEOS where PLAYLIST_ID='{0}'",lsPlaylistID);
              loResultSet = m_db.Execute(lsSQL);
              //loResultSet.
              return true;
          }
          else
          {
              return false;
          }

      }

      public bool createPlayList(string fsName)
      {
          //ArrayList loList = getPlaylists();
          //foreach (String lsName in loList)
          //{
          //    if (lsName.Equals(fsName))
          //    {
          DeletePlaylist(fsName);                  
          //    }
          //}
          string lsSQL = String.Format("insert into PLAYLIST(PLAYLIST_NM) VALUES('{0}')", fsName.Replace("'", "''"));
          m_db.Execute(lsSQL);
          if (m_db.ChangedRows() > 0)
          {
              return true;
          }
          else
          {
              return false;
          }
      }



    
  }
}
