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
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.DatabaseViews;
using MediaPortal.Music.Database;
using SQLite.NET;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for MusicViewHandler.
  /// </summary>
  public class MusicViewHandler : DatabaseViewHandler
  {
    #region Variables

    private readonly string _defaultMusicViews = Path.Combine(DefaultsDirectory, "MusicViews.xml");
    private readonly string _customMusicViews = Config.GetFile(Config.Dir.Config, "MusicViews.xml");

    private int _previousLevel = 0;

    private readonly MusicDatabase _database;
    private Song _currentSong = null; // holds the current Song selected in the list

    private const string StrSongSelect = "select Song.*, Album.*, " +
                                    "( select group_concat(aname, ' | ') from (select distinct(Artist.ArtistName) as aname from artist join artistsong on artistsong.idsong = song.IdSong and artistsong.idartist = artist.IdArtist)) as Artist," +
                                    "( select Artist.ArtistName from artist join albumartist on albumartist.idalbum = Album.IdAlbum and albumartist.idartist = artist.idArtist) as AlbumArtist," +
                                    "( select group_concat(genrename, ' | ') from (select distinct genrename from Genre join genresong on genresong.idsong = song.idSong and genresong.idgenre = genre.idGenre order by genrename)) as Genre," +
                                    "( select group_concat(composername, ' | ') from (select distinct artist.artistname as composername from artist join composersong on composersong.idsong = song.idSong and composersong.idcomposer = artist.idArtist order by composername)) as composer," +
                                    "(share.ShareName || folder.FolderName || song.FileName) as Path ";

    private const string StrSongFrom = "from Song ";

    private const string StrSongJoin = "join Album on Album.IdAlbum = Song.IdAlbum " +
                            "join folder on folder.idFolder = song.idfolder " +
                            "join share on share.IdShare = folder.IdShare " +
                            "join artistsong on artistsong.idsong = song.Idsong and artistsong.idartist = artist.idartist ";

    private string _whereClause = "";
    private string _orderClause = "";
    private string _filterClause = "";

    #endregion

    # region Properties

    /// <summary>
    /// Returns the Previous View Level
    /// </summary>
    public int PreviousLevel
    {
      get { return _previousLevel; }
    }
    
    /// <summary>
    /// Returns the Current View Level
    /// </summary>
    public override string CurrentView
    {
      get
      {
        return base.CurrentView;
      }
      set
      {
        _previousLevel = -1;
        base.CurrentView = value;
      }
    }

    #endregion

    #region Ctor

    public MusicViewHandler()
    {
      if (!File.Exists(_customMusicViews))
      {
        File.Copy(_defaultMusicViews, _customMusicViews);
      }

      try
      {
        using (FileStream fileStream = new FileInfo(_customMusicViews).OpenRead())
        {
          var serializer = new XmlSerializer(typeof(List<DatabaseViewDefinition>));
          views = (List<DatabaseViewDefinition>)serializer.Deserialize(fileStream);
          fileStream.Close();
        }
      }
      catch (Exception) { }

      _database = MusicDatabase.Instance;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Restores the Current view
    /// </summary>
    /// <param name="view"></param>
    /// <param name="level"></param>
    public void Restore(DatabaseViewDefinition view, int level)
    {
      currentView = view;
      currentLevel = level;
    }

    /// <summary>
    /// Returns the Current View
    /// </summary>
    /// <returns></returns>
    public DatabaseViewDefinition GetView()
    {
      return currentView;
    }
    
    /// <summary>
    /// Selects the current item
    /// </summary>
    /// <param name="song"></param>
    public void Select(Song song)
    {
      if (currentView.Levels.Count == 0)
      {
        CurrentView = song.Title;
      }
      else
      {
        var level = currentView.Levels[CurrentLevel];
        level.SelectedValue = GetFieldValue(song, level.Selection);
        level.SelectedId = song.SelectedId;
        if (currentLevel < currentView.Levels.Count)
        {
          currentLevel++;
        }
      }
      _currentSong = song;
    }

    /// <summary>
    /// Executes a query with the selected value
    /// </summary>
    /// <param name="songs"></param>
    /// <returns></returns>
    public bool Execute(out List<Song> songs)
    {
      if (currentLevel < 0)
      {
        _previousLevel = -1;
        songs = new List<Song>();
        return false;
      }

      songs = new List<Song>();

      // We're on a root view, so just list the subviews
      if (currentView.Levels.Count == 0)
      {
        foreach (DatabaseViewDefinition subview in currentView.SubViews)
        {
          Song song = new Song();
          song.Title = subview.LocalizedName;
          songs.Add(song);
        }

        _previousLevel = currentLevel;
        return true;
      }

      _whereClause = "";
      _orderClause = "";
      _filterClause = "";
      var level = currentView.Levels[CurrentLevel];

      if (!string.IsNullOrEmpty(currentView.Parent))
      {
        var parentFilter = new List<DatabaseFilterDefinition>();
        foreach (DatabaseViewDefinition view in views)
        {
          if (currentView.Parent == view.LocalizedName)
          {
            parentFilter = view.Filters;
            break;
          }
        }
        if (parentFilter.Count > 0)
        {
          BuildFilter(parentFilter, ref _filterClause);
        }
      }

      for (int i = 0; i < CurrentLevel; ++i)
      {
        BuildWhere(currentView.Levels[i], ref _whereClause, i);
        BuildFilter(currentView.Levels[i].Filters, ref _filterClause);
      }
      BuildOrder(currentView.Levels[CurrentLevel], ref _orderClause);

      if (CurrentLevel > 0)
      {
        // When grouping is active, we need to omit the "where ";
        if (!_whereClause.Trim().StartsWith("group ") && _whereClause.Trim() != "" && !_whereClause.StartsWith(" where"))
        {
          _whereClause = " where " + _whereClause;
        }
      }

      //execute the query
      var sql = "";
      if (CurrentLevel == 0)
      {
        var levelRoot = currentView.Levels[0];
        string selection = levelRoot.Selection.ToLower();
        string table = GetTable(selection);
        

        var currentLevelFilter = "";
        if (currentView.Filters.Count > 0)
        {
          BuildFilter(currentView.Filters, ref currentLevelFilter);
        }
        _filterClause += currentLevelFilter;

        sql = BuildTopLevelQuery(table);
        
        _database.GetSongsByFilter(sql, out songs, table);
      }
      else if (CurrentLevel < MaxLevels - 1)
      {
        var defCurrent = currentView.Levels[CurrentLevel];
        string table = GetTable(defCurrent.Selection.ToLower());

        sql = BuildLowerLevelQuery(table);

        _database.GetSongsByFilter(sql, out songs, table);

      }
      else
      {
        
      }

      if (songs.Count == 1 && level.SkipLevel)
      {
        if (currentLevel < MaxLevels - 1)
        {
          if (_previousLevel < currentLevel)
          {
            var fd = currentView.Levels[currentLevel];
            fd.SelectedValue = GetFieldValue(songs[0], fd.Selection);
            currentLevel = currentLevel + 1;
          }
          else
          {
            currentLevel = currentLevel - 1;
          }
          if (!Execute(out songs))
          {
            return false;
          }
        }
      }

      _previousLevel = currentLevel;

      return true;
    }

    #endregion

    #region Private Methods

    private string BuildTopLevelQuery(string table)
    {
      string sql = string.Empty;

      // TODO: Handle the Special Case of a Top Level View with an Index

      switch (table.ToLower())
        DatabaseUtility.RemoveInvalidChars(ref selectedValue);

      {
        case "artist":
          break;

        case "albumartist":
          sql = "select distinct Artist.IdArtist, Artist.ArtistName from artist, album join albumartist on albumartist.idalbum = Album.IdAlbum and albumartist.IdArtist = artist.idartist";
          break;

        case "genre":
        case "composer":
          break;

        case "album":
          break;
      }

      if (!string.IsNullOrEmpty(_whereClause) || !string.IsNullOrEmpty(_filterClause))
      {
        sql += " where ";
      }
      if (!string.IsNullOrEmpty(_whereClause))
      {
        sql += _whereClause;
      }
      if (!string.IsNullOrEmpty(_orderClause))
      {
        sql += _orderClause;
      }

      return sql;
    }

    private string BuildLowerLevelQuery(string table)
    {
      string sql = string.Empty;

      // TODO: Handle the Special Case of a Lower Level View with an Index

      switch (table.ToLower())
      {
        case "artist":
          break;

        case "albumartist":
          break;

        case "genre":
        case "composer":
          break;

        case "album":
          sql = "select distinct Album.IdALbum, Album.ALbumNAme, Album.AlbumSortName, Year from Album join AlbumArtist on AlbumArtist.IdAlbum = Album.IdAlbum";
          break;
      }

      if (!string.IsNullOrEmpty(_whereClause))
      {
        sql += _whereClause;
      }
      if (!string.IsNullOrEmpty(_orderClause))
      {
        sql += _orderClause;
      }

      return sql;
    }


    private void BuildWhere(DatabaseFilterLevel level, ref string _whereClause, int currentLevel)
    {
      if (level.Selection.ToLower().EndsWith("index"))
      {
      }
      else
      {
        if (_whereClause != "")
        {
          _whereClause += " and ";
        }

        string selectedValue = level.SelectedValue;
        DatabaseUtility.RemoveInvalidChars(ref selectedValue);

        // we don't store "unknown" into the datbase, so let's correct empty values
        if (selectedValue == "unknown")
        {
          selectedValue = "";
        }

        if (level.SelectedId > -1)
        {
          _whereClause += String.Format("{0} = {1} ", GetIndexField(level.Selection), level.SelectedId);
        }
        else
        {
          // use like for case insensitivity
          _whereClause += String.Format("{0} like '{1}'", GetField(level.Selection), selectedValue);
        }
      }
    }

    private void BuildFilter(List<DatabaseFilterDefinition> filters, ref string filterClause)
    {
      if (filters.Count == 0)
      {
        return;
      }

      if (filterClause != "")
      {
        filterClause += " AND ";
      }

      filterClause += "(";
      foreach (DatabaseFilterDefinition filter in filters)
      {
        filterClause += string.Format("{0} {1} {2}", GetField(filter.Where), GetSqlOperator(filter.SqlOperator), GetFormattedFilterValue(filter));
        if (!string.IsNullOrEmpty(filter.AndOr))
        {
          filterClause += string.Format(" {0}", filter.AndOr);
        }
      }
      filterClause += ")";
    }

    private void BuildOrder(DatabaseFilterLevel filter, ref string orderClause)
    {
      string[] sortFields = GetSortField(filter).Split('|');
      orderClause = " order by ";
      for (int i = 0; i < sortFields.Length; i++)
      {
        if (i > 0)
        {
          orderClause += ", ";
        }
        orderClause += sortFields[i];
        if (!filter.SortAscending)
        {
          orderClause += " desc";
        }
        else
        {
          orderClause += " asc";
        }
      }
    }

    /// <summary>
    /// Check, if this is a field with multiple values, for which we need to compare with Like %value% instead of equals
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private bool IsMultipleValueField(string field)
    {
      switch (field)
      {
        case "strArtist":
        case "strAlbumArtist":
        case "strGenre":
        case "strComposer":
          return true;

        default:
          return false;
      }
    }

    private string GetTable(string selection)
    {
      switch (selection.ToLower())
      {
        case "artist":
        case "artistindex":
          return "artist";

        case "albumartist":
        case "albumartistindex":
          return "albumartist";

        case "composer":
        case "composerindex":
          return "composer";

        case "genre":
        case "genreindex":
          return "genre";

        case "album":
          return "album";

        default:
          return "tracks";
      }
    }

    private string GetField(string selection)
    {
      switch (selection.ToLower())
      {
        case "path":
          return "strPath";

        case "artist":
        case "artistindex":
        case "albumartist":
        case "albumartistindex":
        case "composer":
        case "composerindex":
        case "conductor":
        case "conductorindex":
          return "Artist.ArtistName";

        case "album":
          return "AlbumName";

        case "genre":
        case "genreindex":
          return "GenreName";

        case "title":
          return "Title";


        case "year":
          return "Year";

        case "track#":
          return "iTrack";

        case "numtracks":
          return "iNumTracks";

        case "timesplayed":
          return "iTimesPlayed";

        case "rating":
          return "iRating";

        case "favorites":
          return "iFavorite";

        case "dateadded":
          return "dateAdded";

        case "datelastplayed":
          return "dateLastPlayed";

        case "disc#":
          return "iDisc";

        case "numdiscs":
          return "iNumDiscs";

        case "duration":
          return "iDuration";

        case "resumeat":
          return "iResumeAt";

        case "lyrics":
          return "strLyrics";

        case "comment":
          return "strComment";

        case "filetype":
          return "strFileType";

        case "fullcodec":
          return "strFullCodec";

        case "bitratemode":
          return "strBitRateMode";

        case "bpm":
          return "iBPM";

        case "bitrate":
          return "iBitRate";

        case "channels":
          return "iChannels";

        case "samplerate":
          return "iSampleRate";
      }

      return null;
    }

    private string GetIndexField(string selection)
    {
      switch (selection.ToLower())
      {
        case "artist":
        case "artistindex":
        case "albumartist":
        case "albumartistindex":
        case "composer":
        case "composerindex":
        case "conductor":
        case "conductorindex":
          return "IdArtist";

        case "album":
          return "IdAlbum";

        case "genre":
        case "genreindex":
          return "IdGenre";

      }

      return null;
    }

    public static string GetFieldValue(Song song, string selection)
    {
      switch (selection.ToLower())
      {
        case "path":
          return Path.GetDirectoryName(song.FileName);

        case "artist":
        case "artistindex":
          return song.Artist;

        case "albumartist":
        case "albumartistindex":
          return song.AlbumArtist;

        case "album":
          return song.Album;

        case "genre":
        case "genreindex":
          return song.Genre;

        case "title":
          return song.Title;

        case "composer":
        case "composerindex":
          return song.Composer;

        case "conductor":
        case "conductorindex":
          return song.Conductor;

        case "year":
          return song.Year.ToString();

        case "track#":
          return song.Track.ToString();

        case "numtracks":
          return song.TrackTotal.ToString();

        case "timesplayed":
          return song.TimesPlayed.ToString();

        case "rating":
          return song.Rating.ToString();

        case "favorites":
          {
            if (song.Favorite)
            {
              return "1";
            }
            return "0";
          }

        case "dateadded":
          return song.DateTimeModified.ToShortDateString();

        case "datelastplayed":
          return song.DateTimePlayed.ToShortDateString();

        case "disc#":
          return song.DiscId.ToString();

        case "numdiscs":
          return song.DiscTotal.ToString();

        case "duration":
          return song.Duration.ToString();

        case "resumeat":
          return song.ResumeAt.ToString();

        case "lyrics":
          return song.Lyrics;

        case "comment":
          return song.Comment;

        case "filetype":
          return song.FileType;

        case "fullcodec":
          return song.Codec;

        case "bitratemode":
          return song.BitRateMode;

        case "bpm":
          return song.BPM.ToString();

        case "bitrate":
          return song.BitRate.ToString();

        case "channels":
          return song.Channels.ToString();

        case "samplerate":
          return song.SampleRate.ToString();
      }

      return "";
    }

    private string GetSortField(DatabaseFilterLevel filter)
    {
      // Don't allow anything else but the fieldnames itself on Multiple Fields
      if (filter.Selection.ToLower() == "artist" || filter.Selection.ToLower() == "albumartist" || 
        filter.Selection.ToLower() == "genre" || filter.Selection.ToLower() == "composer")
      {
        return GetField(filter.Selection);
      }

      if (filter.SortBy == "Date")
      {
        return GetField("dateadded");
      }
      if (filter.SortBy == "Year")
      {
        return GetField("year");
      }
      if (filter.SortBy == "Name")
      {
        return GetField("title");
      }
      if (filter.SortBy == "Duration")
      {
        return "iDuration";
      }
      if (filter.SortBy == "disc#")
      {
        return "iDisc";
      }
      if (filter.SortBy == "Track")
      {
        return "iDisc|iTrack"; // We need to sort on Discid + Track
      }
      return GetField(filter.Selection);
    }

    private string GetSqlOperator(string sqloperator)
    {
      switch (sqloperator.ToLower())
      {
        case "equals":
        case "contains":
        case "starts":
        case "ends":
          return "like";

        case "not equals":
        case "not contains":
        case "not starts":
        case "not ends":
          return "not like";

        case "greater than":
          return ">";

        case "greater equals":
          return ">=";

        case "less than":
          return "<";

        case "less equals":
          return "<=";

        case "in":
          return "in";

        case "not in":
          return "not in";
      }
      return "like";
    }

    private string GetFormattedFilterValue(DatabaseFilterDefinition filter)
    {
      string filterValue = string.Empty;

      switch (filter.SqlOperator.ToLower())
      {
        case "equals": 
        case "not equals":
        case "greater than":
        case "greater equals":
        case "less than":
        case "less equals":
          filterValue = string.Format("'{0}'",filter.WhereValue);
          break;

        case "contains":
        case "not contains":
          filterValue = string.Format("'%{0}%'", filter.WhereValue);
          break;

        case "starts":
        case "not starts":
          filterValue = string.Format("'{0}%'", filter.WhereValue);
          break;

        case "ends":
        case "not ends":
          filterValue = string.Format("'%{0}'", filter.WhereValue);
          break;

        case "in":
        case "not in":
          string[] splitValues = filterValue.Split(',');
          filterValue = "(";

          foreach (string splitValue in splitValues)
          {
            filterValue += string.Format("'{0}'", splitValue);
            filterValue += " ,";
          }
          // Remove the Last ","
          filterValue = filterValue.Substring(0, filterValue.Length - 1);
          filterValue += ")";
          break;
      }

      return filterValue;
    }

    protected override string GetLocalizedViewLevel(string lvlName)
    {
      string localizedLevelName = string.Empty;

      switch (lvlName)
      {
        case "artist":
          localizedLevelName = GUILocalizeStrings.Get(133);
          break;
        case "albumartist":
          localizedLevelName = GUILocalizeStrings.Get(528);
          break;
        case "album":
          localizedLevelName = GUILocalizeStrings.Get(132);
          break;
        case "genre":
          localizedLevelName = GUILocalizeStrings.Get(135);
          break;
        case "year":
          localizedLevelName = GUILocalizeStrings.Get(987);
          break;
        case "composer":
          localizedLevelName = GUILocalizeStrings.Get(1214);
          break;
        case "conductor":
          localizedLevelName = GUILocalizeStrings.Get(1215);
          break;
        case "disc#":
          localizedLevelName = GUILocalizeStrings.Get(1216);
          break;
        case "title":
        case "timesplayed":
        case "rating":
        case "favorites":
        case "recently added":
        case "track":
          localizedLevelName = GUILocalizeStrings.Get(1052);
          break;
        default:
          localizedLevelName = lvlName;
          break;
      }

      return localizedLevelName;
    }

    #endregion
  }
}