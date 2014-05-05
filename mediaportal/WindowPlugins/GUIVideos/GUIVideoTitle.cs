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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIVideoTitle : GUIVideoBaseWindow, IMDB.IProgress
  {
    #region Base variabeles

    private DirectoryHistory m_history = new DirectoryHistory();
    private string currentFolder = string.Empty;
    private int currentSelectedItem = -1;
    private VirtualDirectory m_directory = new VirtualDirectory();
    private Layout[,] layouts;
    private bool[,] sortasc;
    private VideoSort.SortMethod[,] sortby;
    private bool _ageConfirmed = false;
    private ArrayList _protectedShares = new ArrayList();
    private int _currentPin = 0;
    private ArrayList _currentProtectedShare = new ArrayList();

    private bool _movieInfoBeforePlay;
    private bool _playClicked;
    private bool _scanning = false;
    private ArrayList _conflictFiles = new ArrayList();
    private int _scanningFileNumber = 1;
    private int _scanningFileTotal = 1;
    private Thread _setThumbs;
    private ArrayList _threadGUIItems = new ArrayList();
    // Search movie/actor
    private static bool _searchMovie = false;
    private static bool _searchActor = false;
    private static string _searchMovieDbField = string.Empty;
    private static string _searchMovieString = string.Empty;
    private static string _searchActorString = string.Empty;
    private static string _currentViewHistory = string.Empty;
    private static string _currentBaseView = string.Empty; // lvl 0 view name (origin view which can be drilled down liek genres, index, years..))
    // Last View lvl postion on back from VideoInfo screen
    private int _currentLevel = 0;
    
    #endregion

    public GUIVideoTitle()
    {
      GetID = (int)Window.WINDOW_VIDEO_TITLE;

      m_directory.AddDrives();
      m_directory.SetExtensions(Util.Utils.VideoExtensions);
    }

    #region overrides

    public override bool Init()
    {
      using (Profile.Settings xmlreader = new MPSettings())
      {
        _movieInfoBeforePlay = xmlreader.GetValueAsBool("moviedatabase", "movieinfobeforeplay", false);
      }

      currentFolder = string.Empty;
      handler.CurrentView = "369";
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\myvideoTitle.xml"));
    }

    protected override string SerializeName
    {
      get { return "myvideo" + handler.CurrentView; }
    }

    protected override Layout CurrentLayout
    {
      get
      {
        if (handler.View != null)
        {
          if (layouts == null)
          {
            layouts = new Layout[handler.Views.Count,50];

            for (int i = 0; i < handler.Views.Count; ++i)
            {
              for (int j = 0; j < handler.Views[i].Filters.Count; ++j)
              {
                FilterDefinition def = (FilterDefinition)handler.Views[i].Filters[j];
                layouts[i, j] = GetLayoutNumber(def.DefaultView);
              }
            }
          }

          return layouts[handler.Views.IndexOf(handler.View), handler.CurrentLevel];
        }
        return Layout.List;
      }
      set { layouts[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value; }
    }

    protected override bool CurrentSortAsc
    {
      get
      {
        if (handler.View != null)
        {
          if (sortasc == null)
          {
            sortasc = new bool[handler.Views.Count,50];

            for (int i = 0; i < handler.Views.Count; ++i)
            {
              for (int j = 0; j < handler.Views[i].Filters.Count; ++j)
              {
                FilterDefinition def = (FilterDefinition)handler.Views[i].Filters[j];
                sortasc[i, j] = def.SortAscending;
              }
            }
          }

          return sortasc[handler.Views.IndexOf(handler.View), handler.CurrentLevel];
        }

        return true;
      }
      set { sortasc[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value; }
    }

    protected override VideoSort.SortMethod CurrentSortMethod
    {
      get
      {
        if (handler.View != null)
        {
          if (sortby == null)
          {
            sortby = new VideoSort.SortMethod[handler.Views.Count,50];
            // !!!!!!!!Sort items must be the same as in VideoSort.cs -> public enum SortMethod
            ArrayList sortStrings = new ArrayList();
            sortStrings.AddRange(Enum.GetNames(typeof(VideoSort.SortMethod)));
            
            for (int i = 0; i < handler.Views.Count; ++i)
            {
              for (int j = 0; j < handler.Views[i].Filters.Count; ++j)
              {
                FilterDefinition def = (FilterDefinition)handler.Views[i].Filters[j];
                int defaultSort = sortStrings.IndexOf(def.DefaultSort);

                if (defaultSort != -1)
                {
                  sortby[i, j] = (VideoSort.SortMethod)defaultSort;
                }
                else
                {
                  sortby[i, j] = VideoSort.SortMethod.Name;
                }
              }
            }
          }

          return sortby[handler.Views.IndexOf(handler.View), handler.CurrentLevel];
        }
        return VideoSort.SortMethod.Name;
      }
      set { sortby[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value; }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        // Reset search result on back
        if ((_searchActor && handler.CurrentLevel == 0) || _searchMovie)
        {
          OnResetSearch();
          return;
        }
        
        if (facadeLayout.Focus)
        {
          GUIListItem item = facadeLayout[0];
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              if (handler.CurrentLevel > 0)
              {
                handler.CurrentLevel--;
                LoadDirectory(item.Path);
                return;
              }
            }
          }
        }
      }
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeLayout[0];
        if (item != null)
        {
          if (item.IsFolder && item.Label == "..")
          {
            handler.CurrentLevel--;
            LoadDirectory(item.Path);
          }
        }
        return;
      }
      if (action.wID == Action.ActionType.ACTION_PLAY || action.wID == Action.ActionType.ACTION_MUSIC_PLAY)
      {
        _playClicked = true;
        OnClick(facadeLayout.SelectedListItemIndex);
        return;
      }
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      int previousWindow = GUIWindowManager.GetPreviousActiveWindow();
      
      // Reset parameters if previous window is not one of video windows
      if (!GUIVideoFiles.IsVideoWindow(previousWindow))
      {
        _ageConfirmed = false;
        _currentPin = 0;
        _currentProtectedShare.Clear();
        _searchMovieDbField = string.Empty;
        _searchMovieString = string.Empty;
        _searchActorString = string.Empty;
        _searchMovie = false;
        _searchActor = false;
        _currentLevel = 0;
      }

      string view = VideoState.View;
      
      if (view == string.Empty)
      {
        view = handler.Views[0].Name;
      }

      handler.CurrentView = view;
      // Resume view lvl position (back from VideoInfo window)
      handler.CurrentLevel = _currentLevel;
      _currentBaseView = handler.CurrentLevelWhere.ToLowerInvariant();

      // Set views
      if (btnViews != null)
      {
        InitViewSelections();
      }

      LoadDirectory(currentFolder);
      GetProtectedShares(ref _protectedShares);

      SetPinLockProperties();
    }
    
    protected override void OnPageDestroy(int newWindowId)
    {
      if (_setThumbs != null && _setThumbs.IsAlive)
      {
        _setThumbs.Abort();
      }

      currentSelectedItem = facadeLayout.SelectedListItemIndex;
      if (newWindowId == (int)Window.WINDOW_VIDEO_TITLE ||
          newWindowId == (int)Window.WINDOW_VIDEOS)
      {
        VideoState.StartWindow = newWindowId;
      }
      _currentLevel = handler.CurrentLevel;

      ReleaseResources();

      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClick(int itemIndex)
    {
      GUIListItem item = facadeLayout.SelectedListItem;
      if (item == null)
      {
        return;
      }
      if (item.IsFolder)
      {
        currentSelectedItem = -1;
        if (item.Label == "..")
        {
          handler.CurrentLevel--;
          
          if (_searchMovie)
          {
            OnResetSearch();
          }
        }
        else
        {
          IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
          ((VideoViewHandler)handler).Select(movie);
        }
        currentFolder = item.Label;
        LoadDirectory(item.Path);
      }
      else
      {
        IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
        if (movie == null)
        {
          return;
        }
        if (movie.ID < 0)
        {
          return;
        }
        // MovieInfo before play
        if (_movieInfoBeforePlay && !_playClicked)
        {
          OnInfo(itemIndex);
          return;
        }
        _playClicked = false; // Reset playclick variable
        GUIVideoFiles.Reset(); // reset pincode

        ArrayList files = new ArrayList();
        VideoDatabase.GetFilesForMovie(movie.ID, ref files);

        if (files.Count > 1)
        {
          GUIVideoFiles.StackedMovieFiles = files;
          GUIVideoFiles.IsStacked = true;
        }
        else
        {
          GUIVideoFiles.IsStacked = false;
        }
        
        GUIVideoFiles.MovieDuration(files, false);
        GUIVideoFiles.PlayMovie(movie.ID, false);
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = facadeLayout.SelectedListItem;
      int itemNo = facadeLayout.SelectedListItemIndex;
      
      if (item == null)
      {
        return;
      }
      
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }

      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
      
      if (movie == null)
      {
        DialogProtectedContent(dlg);
        return;
      }

      // Actor group view
      if (handler.CurrentLevelWhere == "actor" || handler.CurrentLevelWhere == "director")
      {
        dlg.Reset();
        dlg.SetHeading(498); // menu
        
        IMDBActor actor = VideoDatabase.GetActorInfo(movie.ActorID);
        
        dlg.AddLocalizedString(368); //IMDB

        dlg.AddLocalizedString(926); //add to playlist

        if (_protectedShares.Count > 0)
        {
          if (_ageConfirmed)
          {
            dlg.AddLocalizedString(1240); //Lock content
          }
          else
          {
            dlg.AddLocalizedString(1241); //Unlock content
          }
        }
        
        if (handler.CurrentLevelWhere == "director")
        {
          dlg.AddLocalizedString(1268); // Search director
        }
        else
        {
          dlg.AddLocalizedString(1295); // Search actor
        }
        
        dlg.AddLocalizedString(1262); // Update grabber scripts
        dlg.AddLocalizedString(1307); // Update internal grabber scripts
        dlg.AddLocalizedString(1263); // Set default grabber
        dlg.DoModal(GetID);

        if (dlg.SelectedId == -1)
        {
          return;
        }

        switch (dlg.SelectedId)
        {
          case 368: // IMDB
            OnVideoArtistInfo(actor);
            break;
          case 926: //add to playlist
            OnQueueItem(itemNo);
            break;
          case 1240: // Protected content
          case 1241: // Protected content
            OnContentLock();
            break;
          case 1295: // Search actor
          case 1268: // Search director
            OnSearchActor();
            break;
          case 1262: // Update grabber scripts
            GUIVideoFiles.UpdateGrabberScripts(false);
            break;
          case 1307: // Update internal grabber scripts
            GUIVideoFiles.UpdateGrabberScripts(true);
            break;
        }
        return;
      }

      // Context menu on folders (Group names)
      if (movie.ID < 0)
      {
        DialogProtectedContent(dlg);
        return;
      }

      // Context menu on movie title
      dlg.Reset();
      dlg.SetHeading(498); // menu

      if (handler.CurrentLevelWhere == "title" ||
          handler.CurrentLevelWhere == "recently added" ||
          handler.CurrentLevelWhere == "recently watched" ||
          handler.CurrentLevelWhere == "user groups")
      {
        dlg.AddLocalizedString(208); //play
        dlg.AddLocalizedString(368); //IMDB
        dlg.AddLocalizedString(1304); //Make nfo file
        dlg.AddLocalizedString(1306); //Make nfo files
        dlg.AddLocalizedString(926); //add to playlist

        if (!movie.IsEmpty)
        {
          if (item.IsPlayed)
          {
            dlg.AddLocalizedString(830); //Reset watched status
          }
          else
          {
            dlg.AddLocalizedString(1260); // Set watched status
          }
        }

        if (CurrentBaseView == "user groups")
        {
          dlg.AddLocalizedString(1272); //Add new usergroup

          ArrayList userGroups = new ArrayList();
          ArrayList movieUserGroups = new ArrayList();
          VideoDatabase.GetUserGroups(userGroups);
          VideoDatabase.GetMovieUserGroups(movie.ID, movieUserGroups);

          // Add movie to user group if there is available user groups for that movie
          if (movieUserGroups.Count < userGroups.Count)
          {
            dlg.AddLocalizedString(1270); //add movie to usergroup
          }

          if (handler.CurrentLevel > 0)
          {
            dlg.AddLocalizedString(1271); //remove from usergroup
          }
        }

        dlg.AddLocalizedString(118); //rename title
        dlg.AddLocalizedString(1308); //Rename sort title
        dlg.AddLocalizedString(925); //delete
      }

      if ((handler.CurrentLevelWhere == "title" ||
           handler.CurrentLevelWhere == "recently added" ||
           handler.CurrentLevelWhere == "recently watched") && facadeLayout.Count > 1 ||
           handler.CurrentLevelWhere == "user groups")
      {
        dlg.AddLocalizedString(1293); //Search movie
      }
      
      if (_protectedShares.Count > 0)
      {
        if (_ageConfirmed)
        {
          dlg.AddLocalizedString(1240); //Lock content
        }
        else
        {
          dlg.AddLocalizedString(1241); //Unlock content
        }
      }
      
      dlg.AddLocalizedString(1262); // Update grabber scripts
      dlg.AddLocalizedString(1307); // Update internal grabber scripts
      dlg.AddLocalizedString(1263); // Set default grabber

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 118: // Rename title
          OnRenameTitle(itemNo);
          break;
        
        case 925: // Delete
          OnDeleteItem(item);
          break;
        
        case 368: // IMDB
          OnInfo(itemNo);
          break;
        
        case 208: // play
          _playClicked = true; // Override movieinfo before play
          OnClick(itemNo);
          break;
        
        case 926: //add to playlist
          OnQueueItem(itemNo);
          break;
        case 1240: //Lock content
        case 1241: //Unlock content
          OnContentLock();
          break;
        
        case 1293: //Search movie
          OnSearchMovie();
          break;
        
        case 1262: // Update grabber scripts
          GUIVideoFiles.UpdateGrabberScripts(false);
          break;

        case 1307: // Update internal grabber scripts
          GUIVideoFiles.UpdateGrabberScripts(true);
          break;
        
        case 1263: // Set default grabber
          GUIVideoFiles.SetDefaultGrabber();
          break;
        
        case 1270: // Add to user group
          OnAddToUserGroup(movie, itemNo);
          break;
        
        case 1271: // Remove from user group
          OnRemoveFromUserGroup(movie, itemNo);
          break;
        
        case 1272: // Add user group
          OnAddUserGroup();
          break;
        
        case 1308: // Rename sort title
          OnChangeSortTitle(movie, itemNo);
          break;
        
        case 1304: // Make nfo file
          OnCreateNfoFile(movie.ID);
          break;
        
        case 1306: // Make nfo files
          OnCreateNfoFiles();
          break;
        
        case 830: // Reset watched status
          movie.Watched = 0;
          VideoDatabase.SetWatched(movie);
          item.IsPlayed = false;
          break;

        case 1260: // Set watched status
          movie.Watched = 1;
          VideoDatabase.SetWatched(movie);
          item.IsPlayed = true;
          break;
      }
    }
    
    protected override void OnQueueItem(int itemIndex)
    {
      // add item 2 playlist
      GUIListItem listItem = facadeLayout[itemIndex];

      IMDBMovie movieCheck = listItem.AlbumInfoTag as IMDBMovie;
      ArrayList files = new ArrayList();
      
      if (handler.CurrentLevel < handler.MaxLevels - 1 && (movieCheck == null || movieCheck.IsEmpty))
      {
        //queue
        ((VideoViewHandler)handler).Select(listItem.AlbumInfoTag as IMDBMovie);
        ArrayList movies = ((VideoViewHandler)handler).Execute();
        handler.CurrentLevel--;

        foreach (IMDBMovie movie in movies)
        {
          if (movie.ID > 0)
          {
            GUIListItem item = new GUIListItem();
            item.Path = movie.File;
            item.Label = movie.Title;
            item.Duration = movie.RunTime * 60;
            item.IsFolder = false;
            VideoDatabase.GetFilesForMovie(movie.ID, ref files);
            foreach (string file in files)
            {
              item.AlbumInfoTag = movie;
              item.Path = file;
              AddItemToPlayList(item);
            }
          }
        }
      }
      else
      {
        IMDBMovie movie = listItem.AlbumInfoTag as IMDBMovie;
        if (movie != null) VideoDatabase.GetFilesForMovie(movie.ID, ref files);

        foreach (string file in files)
        {
          listItem.Path = file;
          AddItemToPlayList(listItem);
        }
      }
      //move to next item
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, itemIndex + 1);
    }

    protected override void SetView(int selectedViewId)
    {
      // Set current view before change (reset search variables)
      _currentViewHistory = handler.CurrentLevelWhere.ToLowerInvariant();
      // Set new view
      base.SetView(selectedViewId);
      _currentBaseView = handler.CurrentLevelWhere.ToLowerInvariant();
    }

    protected override void OnInfo(int itemIndex)
    {
      GUIListItem item = facadeLayout[itemIndex];

      if (item == null)
      {
        return;
      }

      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

      if (movie == null)
      {
        return;
      }

      if (movie.ID >= 0)
      {
        GUIVideoInfo videoInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_VIDEO_INFO);
        videoInfo.Movie = movie;
        videoInfo.FolderForThumbs = string.Empty;
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_VIDEO_INFO);
      }

      // F3 key actor info action
      if (movie.ActorID >= 0)
      {
        IMDBActor actor = VideoDatabase.GetActorInfo(movie.ActorID);

        if (actor != null)
        {
          string restriction = "30"; // Refresh every month actor info and movies

          TimeSpan ts = new TimeSpan(Convert.ToInt32(restriction), 0, 0, 0);
          DateTime searchDate = DateTime.Today - ts;
          DateTime lastUpdate;

          if (DateTime.TryParse(actor.LastUpdate, out lastUpdate))
          {
            if (searchDate > lastUpdate)
            {
              if (VideoDatabase.CheckActorImdbId(actor.IMDBActorID))
              {
                actor = IMDBFetcher.FetchMovieActor(this, movie, actor.IMDBActorID, movie.ActorID);
              }
              else
              {
                actor = IMDBFetcher.FetchMovieActor(this, movie, actor.Name, movie.ActorID);
              }
            }
          }

          OnVideoArtistInfo(actor);
        }
        else
        {
          string actorImdbId = VideoDatabase.GetActorImdbId(movie.ActorID);

          if (VideoDatabase.CheckActorImdbId(actorImdbId))
          {
            actor = IMDBFetcher.FetchMovieActor(this, movie, actorImdbId, movie.ActorID);
          }
          else
          {
            actor = IMDBFetcher.FetchMovieActor(this, movie, movie.Actor, movie.ActorID);
          }

          if (actor != null)
          {
            OnVideoArtistInfo(actor);
          }
        }
      }
    }
    
    protected override void LoadDirectory(string strNewDirectory)
    {
      GUIWaitCursor.Show();
      currentFolder = strNewDirectory;
      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      ArrayList itemlist = new ArrayList();
      ArrayList movies = new ArrayList();

      if (_searchMovie)
      {
        string sql = "SELECT DISTINCT " +
                     "movieinfo.idMovie," +
                     "movieinfo.idDirector," +
                     "movieinfo.strDirector," +
                     "movieinfo.strPlotOutline," +
                     "movieinfo.strPlot," +
                     "movieinfo.strTagLine," +
                     "movieinfo.strVotes," +
                     "movieinfo.fRating," +
                     "movieinfo.strCast," +
                     "movieinfo.strCredits," +
                     "movieinfo.iYear," +
                     "movieinfo.strGenre," +
                     "movieinfo.strPictureURL," +
                     "movieinfo.strTitle," +
                     "movieinfo.IMDBID," +
                     "movieinfo.mpaa," +
                     "movieinfo.runtime," +
                     "movieinfo.iswatched," +
                     "movieinfo.strUserReview," +
                     "movieinfo.strFanartURL," +
                     "movieinfo.dateAdded," +
                     "movieinfo.dateWatched," +
                     "movieinfo.studios," +
                     "movieinfo.country," +
                     "movieinfo.language," +
                     "movieinfo.lastupdate, " +
			               "movieinfo.strSortTitle " +
                     "FROM movieinfo " +
                     "INNER JOIN actorlinkmovie ON actorlinkmovie.idMovie = movieinfo.idMovie " +
                     "INNER JOIN actors ON actors.idActor = actorlinkmovie.idActor " +
                     "WHERE "+ _searchMovieDbField + " LIKE '%" + _searchMovieString + "%' " +
                     "ORDER BY movieinfo.strTitle ASC";

        VideoDatabase.GetMoviesByFilter(sql, out movies, false, true, false, false);
      }
      else if (_searchActor && handler.CurrentLevelWhere != "title")
      {
        string sql = string.Empty;
        
        if (handler.CurrentLevelWhere == "director")
        {
          sql = "SELECT idActor, strActor, imdbActorId FROM actors INNER JOIN movieinfo ON movieinfo.idDirector = actors.idActor WHERE strActor LIKE '%" 
                + _searchActorString + 
                "%' ORDER BY strActor ASC";
        }
        else
        {
          sql = "SELECT * FROM actors WHERE strActor LIKE '%" + _searchActorString + "%' ORDER BY strActor ASC";
        }
        
        VideoDatabase.GetMoviesByFilter(sql, out movies, true, false, false, false);
      }
      else
      {
        movies = ((VideoViewHandler)handler).Execute();
      }

      GUIControl.ClearControl(GetID, facadeLayout.GetID);
      SwitchLayout();

      if (handler.CurrentLevel > 0)
      {
        GUIListItem listItem = new GUIListItem("..");
        listItem.Path = string.Empty;
        listItem.IsFolder = true;
        Util.Utils.SetDefaultIcons(listItem);
        listItem.OnItemSelected += OnItemSelected;
        itemlist.Add(listItem);
        SetLabel(listItem);
        ((VideoViewHandler)handler).SetLabel(listItem.AlbumInfoTag as IMDBMovie, ref listItem);
        facadeLayout.Add(listItem);
      }

      VirtualDirectory vDir = new VirtualDirectory();
      // Get protected share paths for videos
      vDir.LoadSettings("movies");

      foreach (IMDBMovie movie in movies)
      {
        GUIListItem item = new GUIListItem();
        item.Label = movie.Title;

        if (handler.CurrentLevelWhere != "user groups")
        {
          if (handler.CurrentLevel + 1 < handler.MaxLevels)
          {
            item.IsFolder = true;
          }
          else
          {
            item.IsFolder = false;
          }
        }
        else
        {
          if (string.IsNullOrEmpty(movie.Title) && handler.CurrentLevel + 1 < handler.MaxLevels)
          {
            item.IsFolder = true;
            item.IsRemote = true;
          }
          else
          {
            item.IsFolder = false;
          }
        }

        item.Path = movie.File;

        // Protected movies validation, checks item and if it is inside protected shares.
        // If item is inside PIN protected share, checks if user validate PIN with Unlock
        // command from context menu and returns "True" if all is ok and item will be visible
        // in movie list. Non-protected item will skip check and will be always visible.
        if (!string.IsNullOrEmpty(item.Path))
        {
          if (!IsItemPinProtected(item, vDir))
            continue;
        }
        //
        item.Duration = movie.RunTime * 60;
        item.AlbumInfoTag = movie;
        item.Year = movie.Year;
        item.DVDLabel = movie.DVDLabel;
        item.Rating = movie.Rating;
        item.IsPlayed = movie.Watched > 0;

        try
        {
          if (item.Path.ToUpperInvariant().Contains(@"\VIDEO_TS"))
          {
            item.Label3 = MediaTypes.DVD.ToString() + " #" + movie.WatchedCount;;
          }
          else if (item.Path.ToUpperInvariant().Contains(@"\BDMV"))
          {
            item.Label3 = MediaTypes.BD.ToString() + " #" + movie.WatchedCount;
          }
          else if (VirtualDirectory.IsImageFile(Path.GetExtension(item.Path)))
          {
            item.Label3 = MediaTypes.ISO.ToString() + " #" + movie.WatchedCount; ;
          }
          else
          {
            item.Label3 = movie.WatchedPercent + "% #" + movie.WatchedCount;
          }
        }
        catch (Exception){}
        
        item.OnItemSelected += OnItemSelected;
        SetLabel(item);
        ((VideoViewHandler)handler).SetLabel(item.AlbumInfoTag as IMDBMovie, ref item);
        // Movie/group content list skin property will read from musictag
        item.MusicTag = SetMovieListGroupedBy(item);
        facadeLayout.Add(item);
        itemlist.Add(item);
      }

      // Sort
      facadeLayout.Sort(new VideoSort(CurrentSortMethod, CurrentSortAsc));
      int itemIndex = 0;
      string viewFolder = SetItemViewHistory();
      string selectedItemLabel = m_history.Get(viewFolder);
      
      if (string.IsNullOrEmpty(selectedItemLabel) && facadeLayout.SelectedListItem != null)
      {
        selectedItemLabel = facadeLayout.SelectedListItem.Label;
      }

      int itemCount = itemlist.Count;
      
      if (itemlist.Count > 0)
      {
        GUIListItem rootItem = (GUIListItem)itemlist[0];
        if (rootItem.Label == "..")
        {
          itemCount--;
        }
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(itemCount));

      // Clear info for zero result
      if (itemlist.Count == 0)
      {
        GUIListItem item = new GUIListItem();
        item.Label = GUILocalizeStrings.Get(284);
        IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
        movie = new IMDBMovie();
        item.AlbumInfoTag = movie;
        movie.SetProperties(false, string.Empty);
        itemlist.Add(item);
        facadeLayout.Add(item);
      }

      bool itemSelected = false;
      
      if (handler.CurrentLevel < handler.MaxLevels)
      {
        for (int i = 0; i < facadeLayout.Count; ++i)
        {
          GUIListItem item = facadeLayout[itemIndex];
          
          if (item.Label == selectedItemLabel)
          {
            currentSelectedItem = itemIndex;
            itemSelected = true;
            break;
          }

          itemIndex++;
        }
        
        switch (handler.CurrentLevelWhere.ToLowerInvariant())
        {
          case "genre":
            SetGenreThumbs(itemlist);
            break;

          case "user groups":
            SetUserGroupsThumbs(itemlist);
            break;

          case "actor":
          case "director":
            SetActorThumbs(itemlist);
            break;

          case "year":
            SetYearThumbs(itemlist);
            break;

          case "actorindex":
          case "directorindex":
          case "titleindex":
            foreach (GUIListItem itemAbc in itemlist)
            {
              itemAbc.IconImageBig = @"alpha\" + itemAbc.Label + ".png";
              itemAbc.IconImage = @"alpha\" + itemAbc.Label + ".png";
              itemAbc.ThumbnailImage = @"alpha\" + itemAbc.Label + ".png";
            }
            break;

          default:
            // Assign thumbnails also for the custom views. Bugfix for Mantis 0001471: 
            // Cover image thumbs missing in My Videos when view Selection is by "watched"
            SetIMDBThumbs(itemlist);
            break;
        }
      }
      else
      {
        SetIMDBThumbs(itemlist);
      }

      if (itemSelected)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
      }
      else
      {
        SelectCurrentItem();
      }

      UpdateButtonStates();
      GUIWaitCursor.Hide();
    }
    
    // Scan for new movies for selected folder in configuration
    protected override void OnSearchNew()
    {
      int maximumShares = 128;
      ArrayList availablePaths = new ArrayList();
      bool _useOnlyNfoScraper = false;

      using (Profile.Settings xmlreader = new MPSettings())
      {
        _useOnlyNfoScraper = xmlreader.GetValueAsBool("moviedatabase", "useonlynfoscraper", false);

        for (int index = 0; index < maximumShares; index++)
        {
          string sharePath = String.Format("sharepath{0}", index);
          string shareDir = xmlreader.GetValueAsString("movies", sharePath, "");
          string shareScan = String.Format("sharescan{0}", index);
          bool shareScanData = xmlreader.GetValueAsBool("movies", shareScan, true);

          if (shareScanData && shareDir != string.Empty)
          {
            availablePaths.Add(shareDir);
          }
        }

        if (!_useOnlyNfoScraper)
        {
          IMDBFetcher.ScanIMDB(this, availablePaths, true, true, true, false);
        }
        else
        {
          ArrayList nfoFiles = new ArrayList();
          
          foreach (string availablePath in availablePaths)
          {
            GetNfoFiles(availablePath, ref nfoFiles);
          }
          
          IMDBFetcher fetcher = new IMDBFetcher(this);
          fetcher.FetchNfo(nfoFiles, true, false);
        }
        // Send global message that movie is refreshed/scanned
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VIDEOINFO_REFRESH, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
        currentSelectedItem = facadeLayout.SelectedListItemIndex;

        if (currentSelectedItem > 0)
        {
          currentSelectedItem--;
        }

        LoadDirectory(currentFolder);

        if (currentSelectedItem >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
        }
      }
    }
    
    #endregion

    private void SetLabel(GUIListItem item)
    {
      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

      if (movie != null && movie.ID > 0 && (!item.IsFolder || CurrentSortMethod == VideoSort.SortMethod.NameAll))
      {
        if (CurrentSortMethod == VideoSort.SortMethod.Name || CurrentSortMethod == VideoSort.SortMethod.NameAll)
        {
          if (item.IsFolder)
          {
            item.Label2 = string.Empty;
          }
          else
          {
            // Show real movie duration (from video file)
            int mDuration = movie.Duration;

            if (mDuration <= 0)
            {
              item.Label2 = Util.Utils.SecondsToHMString(movie.RunTime*60);
            }
            else
            {
              item.Label2 = Util.Utils.SecondsToHMString(mDuration);
            }
          }
        }
        else if (CurrentSortMethod == VideoSort.SortMethod.Year)
        {
          item.Label2 = movie.Year.ToString();
        }
        else if (CurrentSortMethod == VideoSort.SortMethod.Rating)
        {
          item.Label2 = movie.Rating.ToString();
        }
        else if (CurrentSortMethod == VideoSort.SortMethod.Label)
        {
          item.Label2 = movie.DVDLabel;
        }
        else if (CurrentSortMethod == VideoSort.SortMethod.Size)
        {
          if (item.FileInfo != null)
          {
            item.Label2 = Util.Utils.GetSize(item.FileInfo.Length);
          }
          else
          {
            item.Label2 = Util.Utils.SecondsToHMString(movie.RunTime * 60);
          }
        }
      }
      else
      {
        string strSize1 = string.Empty, strDate = string.Empty;

        if (item.FileInfo != null && !item.IsFolder)
        {
          strSize1 = Util.Utils.GetSize(item.FileInfo.Length);
        }

        if (item.FileInfo != null && !item.IsFolder)
        {
          if (CurrentSortMethod == VideoSort.SortMethod.Modified)
            strDate = item.FileInfo.ModificationTime.ToShortDateString() + " " +
                      item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          else
            strDate = item.FileInfo.CreationTime.ToShortDateString() + " " +
                      item.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        }
        if (CurrentSortMethod == VideoSort.SortMethod.Name)
        {
          item.Label2 = strSize1;
        }
        else if (CurrentSortMethod == VideoSort.SortMethod.Created || CurrentSortMethod == VideoSort.SortMethod.Date || CurrentSortMethod == VideoSort.SortMethod.Modified)
        {
          item.Label2 = strDate;
        }
        else
        {
          item.Label2 = strSize1;
        }
      }
    }

    private void DialogProtectedContent(GUIDialogMenu dlg)
    {
      dlg.Reset();
      dlg.SetHeading(498); // menu

      if (_protectedShares.Count > 0)
      {
        if (_ageConfirmed)
        {
          dlg.AddLocalizedString(1240); //Lock content
        }
        else
        {
          dlg.AddLocalizedString(1241); //Unlock content
        }
      }

      dlg.AddLocalizedString(926); //add to playlist

      if (handler.CurrentLevelWhere == "actor" && facadeLayout.Count > 1)
      {
        dlg.AddLocalizedString(1295); //Search actor
      }

      if (handler.CurrentLevelWhere == "director" && facadeLayout.Count > 1)
      {
        dlg.AddLocalizedString(1268); // Search director
      }
      else if ((handler.CurrentLevelWhere == "title" ||
                handler.CurrentLevelWhere == "recently added" ||
                handler.CurrentLevelWhere == "recently watched") && facadeLayout.Count > 1 ||
                handler.CurrentLevelWhere == "user groups")
      {
        dlg.AddLocalizedString(1293); //Search movie

        if (handler.CurrentLevelWhere == "user groups")
        {
          dlg.AddLocalizedString(1272); //Add usergroup
          dlg.AddLocalizedString(1273); //Remove selected usergroup
        }
      }

      dlg.AddLocalizedString(1262); // Update grabber scripts
      dlg.AddLocalizedString(1307); // Update internal grabber scripts
      dlg.AddLocalizedString(1263); // Set default grabber
      // Show menu
      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 926: //add to playlist
          OnQueueItem(facadeLayout.SelectedListItemIndex);
          break;
        case 1240: //Lock content
        case 1241: //Unlock content
          OnContentLock();
          break;
        case 1293: //Search movie
          OnSearchMovie();
          break;
        case 1295: //Search actor
        case 1268: // Search director
          OnSearchActor();
          break;
        case 1263: // Set deault grabber script
          GUIVideoFiles.SetDefaultGrabber();
          break;
        case 1262: // Update grabber scripts
          GUIVideoFiles.UpdateGrabberScripts(false);
          break;
        case 1307: // Update internal grabber scripts
          GUIVideoFiles.UpdateGrabberScripts(true);
          break;
        case 1272: // Add user group
          OnAddUserGroup();
          break;
        case 1273: // Remove user group
          GUIListItem item = facadeLayout.SelectedListItem;

          if (item == null)
          {
            return;
          }

          OnRemoveUserGroup(item.Label);
          break;
        
      }
    }

    private void SelectItem()
    {
      if (currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
      }
    }

    #region SetThumbs

    protected void SetGenreThumbs(ArrayList itemlist)
    {
      foreach (GUIListItem item in itemlist)
      {
        // get the genre somewhere since the label isn't set yet.
        IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

        if (movie != null) 
        {
          string genreCover = Util.Utils.GetCoverArt(Thumbs.MovieGenre, movie.SingleGenre);
          SetItemThumb(item, genreCover);
        }
      }
    }

    protected void SetUserGroupsThumbs(ArrayList itemlist)
    {
      ArrayList movies = new ArrayList();
      
      foreach (GUIListItem item in itemlist)
      {
        IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
        
        if (movie != null)
        {
          if (movie.Title == string.Empty)
          {
            string usergroupCover = Util.Utils.GetCoverArt(Thumbs.MovieUserGroups, movie.SingleUserGroup);

            if (File.Exists(usergroupCover))
            {
              SetItemThumb(item, usergroupCover);
            }
            else
            {
              //ArrayList mList = new ArrayList();
              //VideoDatabase.GetMoviesByUserGroup(movie.SingleUserGroup, ref mList);
              //IMDBMovie cMovie = GetRandomMovie(mList);

              //if ( cMovie != null)
              //{
              //  string titleExt = cMovie.Title + "{" + cMovie.ID + "}";
              //  usergroupCover = Util.Utils.GetCoverArt(Thumbs.MovieTitle, titleExt);
              //  SetItemThumb(item, usergroupCover);
              //}
            }
          }
          else
          {
            movies.Add(item);
          }
        }
      }
      
      if (movies.Count > 0)
      {
        SetIMDBThumbs(movies);
      }
    }
    
    protected void SetActorThumbs(ArrayList itemlist)
    {
      if (_setThumbs != null && _setThumbs.IsAlive)
      {
        _setThumbs.Abort();
        _setThumbs = null;
      }

      _threadGUIItems.Clear();
      _threadGUIItems.AddRange(itemlist);
      _setThumbs = new Thread(ThreadSetActorsThumbs);
      _setThumbs.Priority = ThreadPriority.Lowest;
      _setThumbs.IsBackground = true;
      _setThumbs.Start();
    }

    private void ThreadSetActorsThumbs()
    {
      try
      {
        foreach (GUIListItem item in _threadGUIItems)
        {
          // get the actors somewhere since the label isn't set yet.
          IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

          if (movie != null)
          {
            string actorCover = Util.Utils.GetCoverArt(Thumbs.MovieActors, movie.ActorID.ToString());
            SetItemThumb(item, actorCover);
          }
        }
        SelectItem();
      }
      catch (ThreadAbortException)
      {
        Log.Info("GUIVideoTitle: Thread SetActorsThumbs aborted.");
      }
    }
    
    protected void SetYearThumbs(ArrayList itemlist)
    {
      foreach (GUIListItem item in itemlist)
      {
        IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

        if (movie != null) 
        {
          string yearCover = Util.Utils.GetCoverArt(Thumbs.MovieYear, movie.Year.ToString());
          SetItemThumb(item, yearCover);
        }
      }
    }
    
    private void SetIMDBThumbs(ArrayList items)
    {
      if (_setThumbs != null && _setThumbs.IsAlive)
      {
        _setThumbs.Abort();
        _setThumbs = null;
      }

      _threadGUIItems.Clear();
      _threadGUIItems.AddRange(items);
      _setThumbs = new Thread(ThreadSetIMDBThumbs);
      _setThumbs.Priority = ThreadPriority.Lowest;
      _setThumbs.IsBackground = true;
      _setThumbs.Start();
    }
    
    private void ThreadSetIMDBThumbs()
    {
      try
      {
        for (int x = 0; x < _threadGUIItems.Count; ++x)
        {
          string coverArtImage = string.Empty;
          GUIListItem listItem = (GUIListItem)_threadGUIItems[x];
          IMDBMovie movie = listItem.AlbumInfoTag as IMDBMovie;

          if (movie != null)
          {
            if (movie.ID >= 0)
            {
              string titleExt = movie.Title + "{" + movie.ID + "}";
              coverArtImage = Util.Utils.GetCoverArt(Thumbs.MovieTitle, titleExt);
              
              if (Util.Utils.FileExistsInCache(coverArtImage))
              {
                listItem.ThumbnailImage = coverArtImage;
                listItem.IconImageBig = coverArtImage;
                listItem.IconImage = coverArtImage;
              }
            }
          }
          // let's try to assign better covers
          if (!string.IsNullOrEmpty(coverArtImage))
          {
            coverArtImage = Util.Utils.ConvertToLargeCoverArt(coverArtImage);
            if (Util.Utils.FileExistsInCache(coverArtImage))
            {
              listItem.ThumbnailImage = coverArtImage;
            }
          }
          else
          {
            SetDefaultIcon(listItem);
          }
        }
        SelectItem();
      }
      catch (ThreadAbortException)
      {
        Log.Info("GUIVideoTitle: Thread SetIMDBThumbs aborted.");
      }
    }

    protected void SetItemThumb(GUIListItem aItem, string aThumbPath)
    {
      try
      {
        if (!string.IsNullOrEmpty(aThumbPath))
        {
          aItem.IconImage = aThumbPath;
          aItem.IconImageBig = aThumbPath;

          // check whether there is some larger cover art
          string largeCover = Util.Utils.ConvertToLargeCoverArt(aThumbPath);
          
          if (Util.Utils.FileExistsInCache(largeCover))
          {
            aItem.ThumbnailImage = largeCover;
          }
          else
          {
            aItem.ThumbnailImage = aThumbPath;
          }
        }
        else
        {
          SetDefaultIcon(aItem);
        }
      }
      catch (Exception) { }
    }

    private void SetDefaultIcon(GUIListItem listItem)
    {
      if (listItem.Label != "..")
      {
        switch (handler.CurrentLevelWhere.ToLowerInvariant())
        {
          case "title":
            listItem.IconImageBig = "defaultVideoBig.png";
            listItem.IconImage = "defaultVideo.png";
            listItem.ThumbnailImage = "defaultVideoBig.png";
            break;

          case "actor":
          case "director":
            listItem.IconImageBig = "defaultActorBig.png";
            listItem.IconImage = "defaultActor.png";
            listItem.ThumbnailImage = "defaultActorBig.png";
            break;

          case "genre":
            listItem.IconImageBig = "defaultGenreBig.png";
            listItem.IconImage = "defaultGenre.png";
            listItem.ThumbnailImage = "defaultGenreBig.png";
            break;

          case "user groups":
            break;

          case "year":
            listItem.IconImageBig = "defaultYearBig.png";
            listItem.IconImage = "defaultYear.png";
            listItem.ThumbnailImage = "defaultYearBig.png";
            break;

          case "actorindex":
          case "directorindex":
          case "titleindex":
            break;

          default: // For user custom views
            listItem.IconImageBig = "defaultGroupBig.png";
            listItem.IconImage = "defaultGroup.png";
            listItem.ThumbnailImage = "defaultGroupBig.png";
            break;
        }
      }
    }

    #endregion
    
    private void OnDeleteItem(GUIListItem item)
    {
      if (item.IsRemote)
      {
        return;
      }

      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

      if (movie == null)
      {
        return;
      }

      if (movie.ID < 0)
      {
        return;
      }

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }

      dlgYesNo.SetHeading(GUILocalizeStrings.Get(925));
      dlgYesNo.SetLine(1, movie.Title);
      dlgYesNo.SetLine(2, string.Empty);
      dlgYesNo.SetLine(3, string.Empty);
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }

      DoDeleteItem(item);
      currentSelectedItem = facadeLayout.SelectedListItemIndex;

      if (currentSelectedItem > 0)
      {
        currentSelectedItem--;
      }

      LoadDirectory(currentFolder);

      if (currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
      }
    }

    private void DoDeleteItem(GUIListItem item)
    {
      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

      if (movie == null)
      {
        return;
      }

      if (movie.ID < 0)
      {
        return;
      }

      if (item.IsFolder)
      {
        return;
      }

      if (!item.IsRemote)
      {
        VideoDatabase.DeleteMovieInfoById(movie.ID);
      }
    }
    
    private void OnVideoArtistInfo(IMDBActor actor)
    {
      GUIVideoArtistInfo infoDlg =
        (GUIVideoArtistInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_VIDEO_ARTIST_INFO);

      if (infoDlg == null)
      {
        return;
      }

      if (actor == null)
      {
        OnInfo(facadeLayout.SelectedListItemIndex);
        return;
      }

      infoDlg.Actor = actor;
      ArrayList movies = new ArrayList();
      IMDBMovie movie = new IMDBMovie();
      VideoDatabase.GetMoviesByActor(actor.Name, ref movies);
      
      if (movies.Count > 0)
      {
        Random rnd = new Random();

        for (int i = movies.Count - 1; i > 0; i--)
        {
          int position = rnd.Next(i + 1);
          object temp = movies[i];
          movies[i] = movies[position];
          movies[position] = temp;
        }

        movie = (IMDBMovie)movies[0];
      }

      m_history.Set(facadeLayout.SelectedListItem.Label, currentFolder);
      infoDlg.Movie = movie;
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_VIDEO_ARTIST_INFO);
    }
    
    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIPropertyManager.SetProperty("#groupmovielist", string.Empty);
      string strView = string.Empty;
      int currentViewlvl = 0;
      
      if (handler != null)
      {
        strView = handler.CurrentLevelWhere.ToLowerInvariant();
        currentViewlvl = handler.CurrentLevel;

        if (handler.CurrentLevel > 0)
        {
          FilterDefinition defCurrent = (FilterDefinition) handler.View.Filters[handler.CurrentLevel - 1];
          string selectedValue = defCurrent.SelectedValue;
          Int32 iSelectedValue;
          
          if (Int32.TryParse(selectedValue, out iSelectedValue))
          {
            if (strView == "actor" || strView == "director")
            {
              selectedValue = VideoDatabase.GetActorNameById(iSelectedValue);
            }

            if (strView == "genre")
            {
              selectedValue = VideoDatabase.GetGenreById(iSelectedValue);
            }

            if (strView == "user groups")
            {
              selectedValue = VideoDatabase.GetUserGroupById(iSelectedValue);
            }
          }

          GUIPropertyManager.SetProperty("#currentmodule",
                                         String.Format("{0}/{1} - {2}", GUILocalizeStrings.Get(100006),
                                                       handler.LocalizedCurrentView, selectedValue));
        }
      }

      if (item.Label == "..")
      {
        IMDBMovie notMovie = new IMDBMovie();
        notMovie.IsEmpty = true;
        notMovie.SetProperties(true, string.Empty);
        IMDBActor notActor = new IMDBActor();
        notActor.SetProperties();
        return;
      }
      
      // Set current item if thumb thread is working (thread can still update thumbs while user changed
      // item) thus preventing sudden jump to initial selected item before thread start
      if (_setThumbs != null && _setThumbs.IsAlive)
      {
        currentSelectedItem = facadeLayout.SelectedListItemIndex;
      }

      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
      
      if (movie == null)
      {
        movie = new IMDBMovie();
      }
      
      if (!string.IsNullOrEmpty(movie.VideoFileName))
      {
        movie.SetProperties(false, movie.VideoFileName);
      }
      else
      {
        switch (strView)
        {
          case "actorindex":
          case "directorindex":
          case "titleindex":
            movie.IsEmpty = true;
            movie.SetProperties(false, string.Empty);
            break;

          default:
            movie.SetProperties(false, string.Empty);
            break;
        }
        
        // Set title properties for other views (year, genres..)
        if (!string.IsNullOrEmpty(item.Label))
        {
          GUIPropertyManager.SetProperty("#title", item.Label);

          if (item.MusicTag != null)
          {
            GUIPropertyManager.SetProperty("#groupmovielist", item.MusicTag.ToString());
          }
        }
      }
      
      IMDBActor actor = VideoDatabase.GetActorInfo(movie.ActorID);
      
      if (actor != null)
      {
        actor.SetProperties();
      }
      else
      {
        actor = new IMDBActor();
        actor.SetProperties();
      }

      if (movie.ID >= 0)
      {
        string titleExt = movie.Title + "{" + movie.ID + "}";
        string coverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
        
        if (Util.Utils.FileExistsInCache(coverArtImage))
        {
          facadeLayout.FilmstripLayout.InfoImageFileName = coverArtImage;
        }
      }
      
      if (movie.Actor != string.Empty)
      {
        GUIPropertyManager.SetProperty("#title", movie.Actor);
        string coverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, movie.ActorID.ToString());
        
        if (Util.Utils.FileExistsInCache(coverArtImage))
        {
          facadeLayout.FilmstripLayout.InfoImageFileName = coverArtImage;
        }
      }
      
      // Random movieId by view (for FA) for selected group
      ArrayList mList = new ArrayList();
      GetItemViewHistory(strView, mList, currentViewlvl);
    }

    // Set property value for #groupmovielist
    private string SetMovieListGroupedBy(GUIListItem item)
    {
      string strMovies = string.Empty;
      string where = string.Empty;
      string value = string.Empty;
      string sql = string.Empty;
      string view = handler.CurrentLevelWhere.ToLowerInvariant();
      string groupDescription = string.Empty;
      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;

      switch (view)
      {
        case "genre":
        strMovies = VideoDatabase.GetMovieTitlesByGenre(item.Label);
          break;
      
        case "user groups":
          int grpId = VideoDatabase.GetUserGroupId(item.Label);
          groupDescription = VideoDatabase.GetUserGroupDescriptionById(grpId);
          strMovies = VideoDatabase.GetMovieTitlesByUserGroup(grpId);

          if (!string.IsNullOrEmpty(groupDescription))
          {
            groupDescription += ("\n\n" + GUILocalizeStrings.Get(342) + ":\n"); //Movies
          }
          break;
      
        case "actor":
          if (movie != null)
          {
            strMovies = VideoDatabase.GetMovieTitlesByActor(movie.ActorID);
          }
          break;

        case "director":
          if (movie != null)
          {
            strMovies = VideoDatabase.GetMovieTitlesByDirector(movie.ActorID);
          }
          break;
        
        case "year":
          strMovies = VideoDatabase.GetMovieTitlesByYear(item.Label);
          break;
        
        case"actorindex":
          value = DatabaseUtility.RemoveInvalidChars(item.Label);
          where = SetWhere(value, "strActor");
          sql = "SELECT strActor FROM actors " + where +
                     "AND idActor NOT IN (SELECT idDirector FROM movieinfo) GROUP BY strActor ORDER BY strActor ASC";
          strMovies = VideoDatabase.GetMovieTitlesByIndex(sql);
          break;
      
        case "directorindex":
          value = DatabaseUtility.RemoveInvalidChars(item.Label);
          where = SetWhere(value, "strActor");
          sql = "SELECT strActor FROM actors INNER JOIN movieinfo ON movieinfo.idDirector = actors.idActor " + where + 
                     "GROUP BY strActor ORDER BY strActor ASC";
          strMovies = VideoDatabase.GetMovieTitlesByIndex(sql);
          break;
        
        case "titleindex":
          value = DatabaseUtility.RemoveInvalidChars(item.Label);
          where = SetWhere(value, "strTitle");
          sql = "SELECT strTitle FROM movieinfo " + where +
                     "GROUP BY strTitle ORDER BY strTitle ASC ";
          strMovies = VideoDatabase.GetMovieTitlesByIndex(sql);
          break;
      }

      if (!string.IsNullOrEmpty(groupDescription))
      {
        strMovies = groupDescription + strMovies;
      }
      
      return strMovies;
    }

    private string SetWhere(string value, string field)
    {
      string where;
      string nWordChar = VideoDatabase.NonwordCharacters();

      if (Regex.Match(value, @"\W|\d").Success)
      {
        where =
          @"WHERE SUBSTR(" + field + @",1,1) IN (" + nWordChar + ") ";
      }
      else
      {
        where = @"WHERE SUBSTR(" + field + ",1,1) = '" + value + "' ";
      }

      return where;
    }

    // Show or hide protected content
    private void OnContentLock()
    {
      if (!_ageConfirmed)
      {
        if (RequestPin())
        {
          _ageConfirmed = true;
          LoadDirectory(currentFolder);
          GUIPropertyManager.SetProperty("#MyVideos.PinLocked", "false");
        }
        return;
      }

      _ageConfirmed = false;
      LoadDirectory(currentFolder);
      GUIPropertyManager.SetProperty("#MyVideos.PinLocked", "true");
    }

    private void OnSearchMovie()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        return;
      }

      // Context menu on movie title
      dlg.Reset();
      dlg.SetHeading(498); // menu

      dlg.AddLocalizedString(1281);// Add("By Movie Title");
      dlg.AddLocalizedString(1282);// Add("By Director Name");
      dlg.AddLocalizedString(1283);// Add("By Actor Name");
      dlg.AddLocalizedString(1284);// Add("By Actor Role");
      dlg.AddLocalizedString(1285);// Add("By Year");
      dlg.AddLocalizedString(1286);// Add("By Certification (MPAA rating)");
        
      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      
      switch (dlg.SelectedLabel)
      {
        case 0:
          _searchMovieDbField = "movieInfo.strTitle";
          break;
        case 1:
          _searchMovieDbField = "movieInfo.strDirector";
          break;
        case 2:
          _searchMovieDbField = "actors.strActor";
          break;
        case 3:
          _searchMovieDbField = "actorlinkmovie.strRole";
          break;
        case 4:
          _searchMovieDbField = "movieInfo.iYear";
          break;
        case 5:
          _searchMovieDbField = "movieInfo.mpaa";
          break;
      }

      VirtualKeyboard.GetKeyboard(ref _searchMovieString, GetID);
      _searchMovie = true;
      LoadDirectory(currentFolder);
    }

    private void OnSearchActor()
    {
      VirtualKeyboard.GetKeyboard(ref _searchActorString, GetID);
      _searchActor = true;
      LoadDirectory(currentFolder);
    }

    private void OnResetSearch()
    {
      _searchMovieDbField = string.Empty;
      _searchMovieString = string.Empty;
      _searchActorString = string.Empty;
      _searchMovie = false;
      _searchActor = false;
      LoadDirectory(currentFolder);
    }

    private void OnRenameTitle(int itemIndex)
    {
      GUIListItem item = facadeLayout[itemIndex];
      if (item == null)
      {
        return;
      }
      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
      
      if (movie == null)
      {
        return;
      }
      
      if (movie.ID >= 0)
      {
        string movieTitle = movie.Title;
        VirtualKeyboard.GetKeyboard(ref movieTitle, GetID);

        if (string.IsNullOrEmpty(movieTitle) || movieTitle.Trim() == movie.Title)
          return;

        // Rename cover thumbs
        string oldTitleExt = movie.Title + "{" + movie.ID + "}";
        string newTitleExt = movieTitle + "{" + movie.ID + "}";
        string oldSmallThumb = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, oldTitleExt);
        string oldLargeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, oldTitleExt);
        string newSmallThumb = Util.Utils.GetCoverArtName(Thumbs.MovieTitle, newTitleExt);
        string newLargeThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, newTitleExt);

        if (File.Exists(oldSmallThumb))
        {
          try
          {
            File.Copy(oldSmallThumb, newSmallThumb);
            File.Delete(oldSmallThumb);
          }
          catch (Exception) { }
        }
        if (File.Exists(oldLargeThumb))
        {
          try
          {
            File.Copy(oldLargeThumb, newLargeThumb);
            File.Delete(oldLargeThumb);
          }
          catch (Exception) { }
        }

        movie.Title = movieTitle;
        // update db
        bool error;
        string errorMessage = string.Empty;
        string sql = string.Format("UPDATE movieinfo SET strTitle = '{0}' WHERE idMovie = {1}", movieTitle, movie.ID);
        VideoDatabase.ExecuteSql(sql, out error, out errorMessage);

        if (error)
        {
          return;
        }

        // updateitem
        facadeLayout[itemIndex].AlbumInfoTag = movie;
        facadeLayout[itemIndex].Label = movieTitle;
        // Update thumbs for selected item
        facadeLayout[itemIndex].ThumbnailImage = newLargeThumb;
        facadeLayout[itemIndex].IconImageBig = newSmallThumb;
        facadeLayout[itemIndex].IconImage = newSmallThumb;

        // Update sort
        facadeLayout.Sort(new VideoSort(CurrentSortMethod, CurrentSortAsc));
        itemIndex = 0;

        for (int i = 0; i < facadeLayout.Count; ++i)
        {
          GUIListItem lItem = facadeLayout[itemIndex];

          if (item.Label == lItem.Label)
          {
            currentSelectedItem = itemIndex;
            break;
          }

          itemIndex++;
        }

        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, itemIndex);
      }
    }

    private void OnAddToUserGroup(IMDBMovie movie, int itemIndex)
    {
      ArrayList movieUserGroups = new ArrayList();
      ArrayList userGroups = new ArrayList();
      VideoDatabase.GetMovieUserGroups(movie.ID, movieUserGroups);
      VideoDatabase.GetUserGroups(userGroups);

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null || userGroups.Count == 0 || userGroups.Count == movieUserGroups.Count)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(498); // menu

      foreach (string userGroup in userGroups)
      {
        if (!movieUserGroups.Contains(userGroup))
        {
          dlg.Add(userGroup);
        }
      }
      
      dlg.DoModal(GetID);
      
      if (dlg.SelectedId == -1)
      {
        return;
      }
      
      VideoDatabase.AddUserGroupToMovie(movie.ID, VideoDatabase.AddUserGroup(dlg.SelectedLabelText));

      currentSelectedItem = itemIndex;

      LoadDirectory(currentFolder);

      if (currentSelectedItem >= facadeLayout.ListLayout.ListItems.Count)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
      }
      else
      {
        currentSelectedItem--;

        if (currentSelectedItem >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
        }
      }
    }

    private void OnRemoveFromUserGroup(IMDBMovie movie, int itemIndex)
    {
      string group = m_history.Get("user groups");
      VideoDatabase.RemoveUserGroupFromMovie(movie.ID, VideoDatabase.AddUserGroup(group));
      
      currentSelectedItem = itemIndex;
      
      LoadDirectory(currentFolder);
      
      if (currentSelectedItem >= facadeLayout.ListLayout.ListItems.Count)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
      }
      else
      {
        currentSelectedItem --;

        if (currentSelectedItem >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
        }
      }
    }

    private void OnAddUserGroup()
    {
      string newGroup = string.Empty;
      VirtualKeyboard.GetKeyboard(ref newGroup, GetID);

      if (string.IsNullOrEmpty(newGroup))
      {
        return;
      }

      VideoDatabase.AddUserGroup(newGroup);
      LoadDirectory(currentFolder);
    }

    private void OnRemoveUserGroup(string group)
    {
      VideoDatabase.DeleteUserGroup(group);
      LoadDirectory(currentFolder);
    }

    private void OnChangeSortTitle(IMDBMovie movie, int itemIndex)
    {
      GUIListItem currentItem = facadeLayout[itemIndex];
      
      if (currentItem == null)
      {
        return;
      }
      
      if (movie == null)
      {
        return;
      }

      string movieSortTitle = movie.SortTitle;
      VirtualKeyboard.GetKeyboard(ref movieSortTitle, GetID);

      if (string.IsNullOrEmpty(movieSortTitle) || movieSortTitle.Trim() == movie.Title)
      {
        return;
      }

      movie.SortTitle = movieSortTitle;
      // update db
      bool error;
      string errorMessage = string.Empty;
      string sql = string.Format("UPDATE movieinfo SET strSortTitle = '{0}' WHERE idMovie = {1}", movieSortTitle, movie.ID);
      VideoDatabase.ExecuteSql(sql, out error, out  errorMessage);

      if (error)
      {
        return;
      }

      // updateitem
      facadeLayout[itemIndex].AlbumInfoTag = movie;

      if (movie.ID >= 0)
      {
        // Update sort
        facadeLayout.Sort(new VideoSort(CurrentSortMethod, CurrentSortAsc));
        itemIndex = 0;
        
        for (int i = 0; i < facadeLayout.Count; ++i)
        {
          GUIListItem item = facadeLayout[itemIndex];

          if (item.Label == currentItem.Label)
          {
            currentSelectedItem = itemIndex;
            break;
          }

          itemIndex++;
        }

        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, itemIndex);
      }
    }

    private void OnCreateNfoFile(int movieId)
    {
      if (movieId > 0)
      {
        VideoDatabase.MakeNfo(movieId);

        // Notify user that new fanart download failed
        GUIDialogNotify dlgNotify =
          (GUIDialogNotify)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_NOTIFY);
        if (null != dlgNotify)
        {
          dlgNotify.SetHeading(GUILocalizeStrings.Get(1304));
          dlgNotify.SetText(GUILocalizeStrings.Get(1305));
          dlgNotify.DoModal(GetID);
        }
      }
    }

    private void OnCreateNfoFiles()
    {
      if (facadeLayout != null)
      {
        // Initialize progress bar
        GUIDialogProgress progressDialog =
          (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
        progressDialog.Reset();
        progressDialog.SetHeading(GUILocalizeStrings.Get(1312)); // Exporting movies...
        progressDialog.ShowProgressBar(true);
        progressDialog.SetLine(1, GUILocalizeStrings.Get(1313)); // Creating nfo file
        progressDialog.SetLine(2, GUILocalizeStrings.Get(1314)); // Working...
        progressDialog.StartModal(GUIWindowManager.ActiveWindow);
        int percent = 0;
        int moviesCount = facadeLayout.ListLayout.ListItems.Count;

        foreach (GUIListItem item in facadeLayout.ListLayout.ListItems)
        {
          if (progressDialog.IsCanceled)
          {
            break;
          }

          IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
          
          if (movie != null)
          {
            progressDialog.SetLine(1, GUILocalizeStrings.Get(1315) + movie.Title); // Creating nfo for:
            progressDialog.SetLine(2, GUILocalizeStrings.Get(1314)); // Working
            progressDialog.SetPercentage(percent);
            percent += 100 / (moviesCount - 1);
            progressDialog.Progress();
            VideoDatabase.MakeNfo(movie.ID);
          }
        }

        progressDialog.Close();
      }
    }
    
    // Get all shares and pins for protected video folders
    private void GetProtectedShares(ref ArrayList shares)
    {
      using (Profile.Settings xmlreader = new MPSettings())
      {
        shares = new ArrayList();

        for (int index = 0; index < 128; index++)
        {
          string sharePin = String.Format("pincode{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePinData = Util.Utils.DecryptPin(xmlreader.GetValueAsString("movies", sharePin, ""));
          string sharePathData = xmlreader.GetValueAsString("movies", sharePath, "");

          if (!string.IsNullOrEmpty(sharePinData))
          {
            shares.Add(sharePinData + "|" + sharePathData);
          }
        }
      }
    }

    // Protected content PIN validation (any PIN from video protected folders is valid)
    private bool RequestPin()
    {
      bool retry = true;
      bool sucess = false;
      _currentProtectedShare.Clear();

      while (retry)
      {
        GUIMessage msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
        GUIWindowManager.SendMessage(msgGetPassword);
        int iPincode = -1;

        try
        {
          iPincode = Int32.Parse(msgGetPassword.Label);
        }
        catch (Exception) {}

        foreach (string p in _protectedShares)
        {
          char[] splitter = {'|'};
          string[] pin = p.Split(splitter);

          if (iPincode != Convert.ToInt32(pin[0]))
          {
            _currentPin = iPincode;
            continue;
          }

          if (iPincode == Convert.ToInt32(pin[0]))
          {
            _currentPin = iPincode;
            _currentProtectedShare.Add(pin[1]);
            sucess = true;
          }
        }

        if (sucess)
          return true;

        GUIMessage msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0,
                                                     0);
        GUIWindowManager.SendMessage(msgWrongPassword);

        if (!(bool)msgWrongPassword.Object)
        {
          retry = false;
        }
        else
        {
          retry = true;
        }
      }

      _currentPin = 0;
      return false;
    }

    // Skin properties for locked/unlocked indicator
    private void SetPinLockProperties()
    {
      if (_protectedShares.Count == 0)
      {
        GUIPropertyManager.SetProperty("#MyVideos.PinLocked", "");
      }
      else if (_ageConfirmed)
      {
        GUIPropertyManager.SetProperty("#MyVideos.PinLocked", "false");
      }
      else
      {
        GUIPropertyManager.SetProperty("#MyVideos.PinLocked", "true");
      }
    }

    // Check if item is pin protected and if it exists within unlocked shares
    // Returns true if item is valid or if item is not within protected shares
    private bool IsItemPinProtected(GUIListItem item, VirtualDirectory vDir)
    {
      string directory = Path.GetDirectoryName(item.Path); // item path

      if (directory != null)
      {
        //VirtualDirectory vDir = new VirtualDirectory();
        //// Get protected share paths for videos
        //vDir.LoadSettings("movies");

        // Check if item belongs to protected shares
        int pincode = 0;
        bool folderPinProtected = vDir.IsProtectedShare(directory, out pincode);

        bool success = false;

        // User unlocked share/shares with PIN and item is within protected shares
        if (folderPinProtected && _ageConfirmed)
        {
          // Iterate unlocked shares against current item path
          foreach (string share in _currentProtectedShare)
          {
          if (!directory.ToUpperInvariant().Contains(share.ToUpperInvariant()))
            {
              continue;
            }
            success = true;
            break;
          }

          // current item is not within unlocked shares, 
          // don't show item and go to the next item
          if (!success)
          {
            return false;
          }
          return true;
        }

        // Nothing unlocked and item belongs to protected shares,
        // don't show item and go to the next item
        if (folderPinProtected && !_ageConfirmed)
        {
          return false;
        }
      }

      // Item is not inside protected shares, show it
      return true;
    }

    // Set movieID skin property (locked movies will be discarded)
    private void SetRandomMovieId(ArrayList mList)
    {
      if (mList.Count == 0)
      {
        GUIPropertyManager.SetProperty("#movieid", "-1");
        return;
      }
      try
      {
        ArrayList movies = new ArrayList(mList);
        ArrayList pShares = new ArrayList();

        foreach (string p in _protectedShares)
        {
          char[] splitter = { '|' };
          string[] pin = p.Split(splitter);

          // Only add shares which are unlocked
          if (Convert.ToInt32(pin[0]) == _currentPin)
          {
            pShares.Add(pin[1]);
          }
        }

        // Do not show fanart for unlocked protected movies
        foreach (IMDBMovie m in movies)
        {
          string directory = Path.GetDirectoryName(m.VideoFilePath);

          if (string.IsNullOrEmpty(directory))
          {
            continue;
          }

          VirtualDirectory vDir = new VirtualDirectory();
          vDir.LoadSettings("movies");
          int pincode = 0;
          bool folderPinProtected = vDir.IsProtectedShare(directory, out pincode);
          
          // No PIN entered, remove all protected conetnt
          if (folderPinProtected && !_ageConfirmed)
          {
            mList.Remove(m);
            continue;
          }

          // PIN entered, check for corresponding shares
          if (folderPinProtected && _ageConfirmed)
          {
            bool found = false;

            foreach (string share in pShares)
            {
              if (directory.ToLowerInvariant().Contains(share.ToLowerInvariant()))
              {
                // Movie belongs to unlocked share
                found = true;
                break;
              }
            }

            // If movie is not from unlocked shares, don't show fanart
            if (!found)
            {
              mList.Remove(m);
            }
          }
        }

        if (mList.Count > 0)
        {
          Random rnd = new Random();
          int r = rnd.Next(mList.Count);
          IMDBMovie movieDetails = (IMDBMovie)mList[r];
          GUIPropertyManager.SetProperty("#movieid", movieDetails.ID.ToString());
        }
        else
        {
          GUIPropertyManager.SetProperty("#movieid", "-1");
        }
      }
      catch (Exception)
      {
        GUIPropertyManager.SetProperty("#movieid", "-1");
      }
    }

    private IMDBMovie GetRandomMovie(ArrayList mList)
    {
      try
      {
        ArrayList movies = new ArrayList(mList);
        ArrayList pShares = new ArrayList();

        foreach (string p in _protectedShares)
        {
          char[] splitter = { '|' };
          string[] pin = p.Split(splitter);
          // Only add shares which are unlocked
          if (Convert.ToInt32(pin[0]) == _currentPin)
          {
            pShares.Add(pin[1]);
          }
        }

        // Do not show fanart for unlocked protected movies
        foreach (IMDBMovie m in movies)
        {
          ArrayList files = new ArrayList();
          VideoDatabase.GetFilesForMovie(m.ID, ref files);

          if (string.IsNullOrEmpty(files[0].ToString()))
          {
            continue;
          }

          string directory = Path.GetDirectoryName(files[0].ToString());

          if (string.IsNullOrEmpty(directory))
            continue;

          VirtualDirectory vDir = new VirtualDirectory();
          vDir.LoadSettings("movies");
          int pincode = 0;
          bool folderPinProtected = vDir.IsProtectedShare(directory, out pincode);

          // No PIN entered, remove all protected conetnt
          if (folderPinProtected && !_ageConfirmed)
          {
            mList.Remove(m);
            continue;
          }

          // PIN entered, check for corresponding shares
          if (folderPinProtected && _ageConfirmed)
          {
            bool found = false;

            foreach (string share in pShares)
            {
              if (directory.ToLowerInvariant().Contains(share.ToLowerInvariant()))
              {
                // Movie belongs to unlocked share
                found = true;
                break;
              }
            }
            // If movie is not from unlocked shares, don't show fanart
            if (!found)
            {
              mList.Remove(m);
            }
          }
        }

        if (mList.Count > 0)
        {
          Random rnd = new Random();
          int r = rnd.Next(mList.Count);
          IMDBMovie movieDetails = (IMDBMovie)mList[r];
          return movieDetails;
        }
        else
        {
          return null;
        }
      }
      catch (Exception)
      {
        return null;
      }
    }

    // Set selected item position in history of current view 
    // (when user switch view and get back item position will be restored)
    private string SetItemViewHistory()
    {
      string viewFolder = handler.CurrentLevelWhere.ToLowerInvariant();
      return viewFolder;
    }

    // Restore selected item postion on current view (if user was been here and switched view)
    // Set random movieId for group view (FA handler can use that)
    private void GetItemViewHistory(string view, ArrayList mList, int currentLvl)
    {
      if (facadeLayout.SelectedListItem != null && !string.IsNullOrEmpty(facadeLayout.SelectedListItem.Label))
      {
        string selectedLabel = facadeLayout.SelectedListItem.Label;

        if (!string.IsNullOrEmpty(view))
        {
          switch (view)
          {
            case "genre":
              m_history.Set(selectedLabel, view);
              VideoDatabase.GetRandomMoviesByGenre(selectedLabel, ref mList, 1);
              SetRandomMovieId(mList);
              break;

            case "user groups":
              m_history.Set(selectedLabel, view);
              IMDBMovie movie = facadeLayout.SelectedListItem.AlbumInfoTag as IMDBMovie;
              if (movie == null || movie.ID == -1)
              {
                VideoDatabase.GetRandomMoviesByUserGroup(selectedLabel, ref mList, 1);
                SetRandomMovieId(mList);
              }
              break;

            case "actor":
            case "director":
              m_history.Set(selectedLabel, view);
              VideoDatabase.GetRandomMoviesByActor(selectedLabel, ref mList, 1);
              SetRandomMovieId(mList);
              break;

            case "year":
              m_history.Set(selectedLabel, view);
              VideoDatabase.GetRandomMoviesByYear(selectedLabel, ref mList, 1);
              SetRandomMovieId(mList);
              break;

            case "recently added":
              if (currentLvl == 0)
              {
                m_history.Set(selectedLabel, view);
              }
              break;

            case "recently watched":
              if (currentLvl == 0)
              {
                m_history.Set(selectedLabel, view);
              }
              break;

            case "watched":
              if (currentLvl == 0)
              {
                m_history.Set(selectedLabel, view);
              }
              break;

            case "unwatched":
              if (currentLvl == 0)
              {
                m_history.Set(selectedLabel, view);
              }
              break;

            case "titleindex":
              if (currentLvl == 0)
              {
                string where = SetWhere(selectedLabel, "strTitle");
                string sql = "SELECT * FROM movieinfo " + where +
                             "GROUP BY strTitle ORDER BY RANDOM() LIMIT 1";

                VideoDatabase.GetMoviesByFilter(sql, out mList, false, true, false, false);
                SetRandomMovieId(mList);
                m_history.Set(selectedLabel, view);
              }
              break;

            default:
              m_history.Set(selectedLabel, view);
              break;
          }
        }
      }
    }

    private void GetNfoFiles(string path, ref ArrayList nfoFiles)
    {
      string[] files = Directory.GetFiles(path, "*.nfo", SearchOption.AllDirectories);
      var sortedFiles = files.OrderBy(f => f);

      foreach (string file in sortedFiles)
      {
        nfoFiles.Add(file);
      }
    }

    private void ReleaseResources()
    {
      if (facadeLayout != null)
      {
        facadeLayout.Clear();
      }
    }
    
    #region Get/Set

    public static bool IsMovieSearch
    {
      get { return _searchMovie; }
      set { _searchMovie = value; }
    }

    public static bool IsActorSearch
    {
      get { return _searchActor; }
      set { _searchActor = value; }
    }

    public static string MovieSearchDbFieldString
    {
      get { return _searchMovieDbField; }
      set { _searchMovieDbField = value; }
    }

    public static string MovieSearchString
    {
      get { return _searchMovieString; }
      set { _searchMovieString = value; }
    }

    public static string ActorSearchString
    {
      get { return _searchActorString; }
      set { _searchActorString = value; }
    }

    public static string CurrentViewHistory
    {
      get { return _currentViewHistory; }
    }

    public static string CurrentBaseView
    {
      get { return _currentBaseView; }
    }

    #endregion
    
    #region IMDB.IProgress

    public bool OnDisableCancel(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if (pDlgProgress.IsInstance(fetcher))
      {
        pDlgProgress.DisableCancel(true);
      }
      return true;
    }

    public void OnProgress(string line1, string line2, string line3, int percent)
    {
      if (!GUIWindowManager.IsRouted)
      {
        return;
      }
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.ShowProgressBar(true);
      pDlgProgress.SetLine(1, line1);
      pDlgProgress.SetLine(2, line2);
      if (percent > 0)
      {
        pDlgProgress.SetPercentage(percent);
      }
      pDlgProgress.Progress();
    }

    public bool OnSearchStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're busy querying www.imdb.com
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(197), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(197);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnSearchStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnSearchEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnMovieNotFound(IMDBFetcher fetcher)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        return false;
      }
      // show dialog...
      GUIDialogOK pDlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      pDlgOk.SetHeading(195);
      pDlgOk.SetLine(1, fetcher.MovieName);
      pDlgOk.SetLine(2, string.Empty);
      pDlgOk.DoModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the movie info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(198), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(198);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      //pDlgProgress.SetLine(0, strMovieName);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnDetailsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnDetailsEnd(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if ((pDlgProgress != null) && (pDlgProgress.IsInstance(fetcher)))
      {
        pDlgProgress.Close();
      }
      return true;
    }

    public bool OnActorsStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the actor info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(986), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(1301);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      //pDlgProgress.SetLine(0, strMovieName);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnActorInfoStarting(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      // show dialog that we're downloading the actor info
      String heading;
      if (_scanning)
      {
        heading = String.Format("{0}:{1}/{2}", GUILocalizeStrings.Get(986), _scanningFileNumber, _scanningFileTotal);
      }
      else
      {
        heading = GUILocalizeStrings.Get(1302);
      }
      pDlgProgress.Reset();
      pDlgProgress.SetHeading(heading);
      //pDlgProgress.SetLine(0, strMovieName);
      pDlgProgress.SetLine(1, fetcher.MovieName);
      pDlgProgress.SetLine(2, string.Empty);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
      return true;
    }

    public bool OnActorsStarted(IMDBFetcher fetcher)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      pDlgProgress.SetObject(fetcher);
      pDlgProgress.DoModal(GUIWindowManager.ActiveWindow);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

    public bool OnActorsEnd(IMDBFetcher fetcher)
    {
      return true;
    }

    public bool OnDetailsNotFound(IMDBFetcher fetcher)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        return false;
      }
      // show dialog...
      GUIDialogOK pDlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      // show dialog...
      pDlgOk.SetHeading(195);
      pDlgOk.SetLine(1, fetcher.MovieName);
      pDlgOk.SetLine(2, string.Empty);
      pDlgOk.DoModal(GUIWindowManager.ActiveWindow);
      return false;
    }

    public bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        movieName = string.Empty;
        return false;
      }
      movieName = fetcher.MovieName;
      if (VirtualKeyboard.GetKeyboard(ref movieName, GetID))
      {
        if (movieName == string.Empty)
        {
          return false;
        }
        return true;
      }
      movieName = string.Empty;
      return false;
    }

    public bool OnSelectMovie(IMDBFetcher fetcher, out int selectedMovie)
    {
      if (_scanning)
      {
        _conflictFiles.Add(fetcher.Movie);
        selectedMovie = -1;
        return false;
      }
      GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      // more then 1 movie found
      // ask user to select 1
      pDlgSelect.Reset();
      pDlgSelect.SetHeading(196); //select movie
      for (int i = 0; i < fetcher.Count; ++i)
      {
        pDlgSelect.Add(fetcher[i].Title);
      }
      pDlgSelect.EnableButton(true);
      pDlgSelect.SetButtonLabel(413); // manual
      pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

      // and wait till user selects one
      selectedMovie = pDlgSelect.SelectedLabel;
      if (pDlgSelect.IsButtonPressed)
      {
        return true;
      }
      if (selectedMovie == -1)
      {
        return false;
      }
      return true;
    }

    public bool OnSelectActor(IMDBFetcher fetcher, out int selectedActor)
    {
      GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      // more then 1 actor found
      // ask user to select 1
      pDlgSelect.SetHeading(GUILocalizeStrings.Get(1310)); //select actor
      pDlgSelect.Reset();
      for (int i = 0; i < fetcher.Count; ++i)
      {
        pDlgSelect.Add(fetcher[i].Title);
      }
      pDlgSelect.EnableButton(false);
      pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

      // and wait till user selects one
      selectedActor = pDlgSelect.SelectedLabel;
      if (selectedActor != -1)
      {
        return true;
      }
      return false;
    }

    public bool OnScanStart(int total)
    {
      _scanning = true;
      _conflictFiles.Clear();
      _scanningFileTotal = total;
      _scanningFileNumber = 1;
      return true;
    }

    public bool OnScanEnd()
    {
      _scanning = false;
      if (_conflictFiles.Count > 0)
      {
        GUIDialogSelect pDlgSelect = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
        // more than 1 movie found
        // ask user to select 1
        do
        {
          pDlgSelect.Reset();
          pDlgSelect.SetHeading(892); //select movie
          for (int i = 0; i < _conflictFiles.Count; ++i)
          {
            IMDBMovie currentMovie = (IMDBMovie)_conflictFiles[i];
            string strFileName = string.Empty;
            string path = currentMovie.Path;
            string filename = currentMovie.File;
            if (path != string.Empty)
            {
              if (path.EndsWith(@"\"))
              {
                path = path.Substring(0, path.Length - 1);
                currentMovie.Path = path;
              }
              if (filename.StartsWith(@"\"))
              {
                filename = filename.Substring(1);
                currentMovie.File = filename;
              }
              strFileName = path + @"\" + filename;
            }
            else
            {
              strFileName = filename;
            }
            pDlgSelect.Add(strFileName);
          }
          pDlgSelect.EnableButton(true);
          pDlgSelect.SetButtonLabel(4517); // manual
          pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

          // and wait till user selects one
          int selectedMovie = pDlgSelect.SelectedLabel;
          if (pDlgSelect.IsButtonPressed)
          {
            break;
          }
          if (selectedMovie == -1)
          {
            break;
          }
          IMDBMovie movieDetails = (IMDBMovie)_conflictFiles[selectedMovie];
          string searchText = movieDetails.Title;
          if (searchText == string.Empty)
          {
            searchText = movieDetails.SearchString;
          }
          if (VirtualKeyboard.GetKeyboard(ref searchText, GetID))
          {
            if (searchText != string.Empty)
            {
              movieDetails.SearchString = searchText;
              if (IMDBFetcher.GetInfoFromIMDB(this, ref movieDetails, false, true))
              {
                if (movieDetails != null)
                {
                  _conflictFiles.RemoveAt(selectedMovie);
                }
              }
            }
          }
        } while (_conflictFiles.Count > 0);
      }
      return true;
    }

    public bool OnScanIterating(int count)
    {
      _scanningFileNumber = count;
      return true;
    }

    public bool OnScanIterated(int count)
    {
      _scanningFileNumber = count;
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      if (pDlgProgress.IsCanceled)
      {
        return false;
      }
      return true;
    }

   #endregion
  }
}