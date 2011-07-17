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
using System.IO;
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
  public class GUIVideoTitle : GUIVideoBaseWindow
  {
    #region Base variabeles

    private DirectoryHistory m_history = new DirectoryHistory();
    private string currentFolder = string.Empty;
    private int currentSelectedItem = -1;
    private VirtualDirectory m_directory = new VirtualDirectory();
    private Layout[,] layouts;
    private bool[,] sortasc;
    private VideoSort.SortMethod[,] sortby;
    private bool ageConfirmed = false;
    private ArrayList pins = new ArrayList();
    private int currentPin = 0;
    private ArrayList currentProtectedShare = new ArrayList();

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
      currentFolder = string.Empty;
      handler.CurrentView = "369";
      return Load(GUIGraphicsContext.Skin + @"\myvideoTitle.xml");
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

            ArrayList viewStrings = new ArrayList();
            viewStrings.Add("List");
            viewStrings.Add("Icons");
            viewStrings.Add("Big Icons");
            viewStrings.Add("Filmstrip");

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
        else
        {
          return Layout.List;
        }
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

            ArrayList sortStrings = new ArrayList();
            sortStrings.Add("Name");
            sortStrings.Add("Date");
            sortStrings.Add("Size");
            sortStrings.Add("Year");
            sortStrings.Add("Rating");
            sortStrings.Add("Label");

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
        else
        {
          return VideoSort.SortMethod.Name;
        }
      }
      set { sortby[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value; }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
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
      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      int previousWindow = GUIWindowManager.GetPreviousActiveWindow();

      if (!GUIVideoFiles.IsVideoWindow(previousWindow))
      {
        ageConfirmed = false;
        currentPin = 0;
        currentProtectedShare.Clear();
      }

      string view = VideoState.View;
      if (view == string.Empty)
      {
        view = ((ViewDefinition)handler.Views[0]).Name;
      }

      handler.CurrentView = view;
      base.OnPageLoad();

      LoadDirectory(currentFolder);
      GetSharePins(ref pins);

      SetPinLockProperties();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      currentSelectedItem = facadeLayout.SelectedListItemIndex;
      if (newWindowId == (int)Window.WINDOW_VIDEO_TITLE ||
          newWindowId == (int)Window.WINDOW_VIDEOS)
      {
        VideoState.StartWindow = newWindowId;
      }
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
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
        }
        else
        {
          IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
          ((VideoViewHandler)handler).Select(movie);
        }
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
        GUIVideoFiles.Reset(); // reset pincode

        ArrayList files = new ArrayList();
        VideoDatabase.GetFiles(movie.ID, ref files);

        if (files.Count > 1)
        {
          GUIVideoFiles._stackedMovieFiles = files;
          GUIVideoFiles._isStacked = true;
          GUIVideoFiles.MovieDuration(files);
        }
        else
        {
          GUIVideoFiles._isStacked = false;
        }
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
        Dialog_ProtectedContent(dlg);
        return;
      }
      if (handler.CurrentLevelWhere == "actor")
      {
        IMDBActor actor = VideoDatabase.GetActorInfo(movie.ActorID);
        if (actor != null)
        {
          dlg.Reset();
          dlg.SetHeading(498); // menu
          dlg.Add(GUILocalizeStrings.Get(368)); //IMDB
          if (pins.Count > 0)
          {
            if (ageConfirmed)
            {
              dlg.Add("Lock content"); //Lock content
            }
            else
            {
              dlg.Add("Unlock content"); //Unlock content
            }
          }

          dlg.DoModal(GetID);
          if (dlg.SelectedLabel == -1)
          {
            return;
          }
          switch (dlg.SelectedLabel)
          {
            case 0: // IMDB
              OnVideoArtistInfo(actor);
              break;
            case 1: // Protected content
              OnContentLock();
              break;
          }
          return;
        }
      }
      // Context menu on folders (Group names)
      if (movie.ID < 0)
      {
        Dialog_ProtectedContent(dlg);
        return;
      }
      // Context menu on movie title
      dlg.Reset();
      dlg.SetHeading(498); // menu
      dlg.Add(GUILocalizeStrings.Get(925)); //delete
      dlg.Add(GUILocalizeStrings.Get(368)); //IMDB
      dlg.Add(GUILocalizeStrings.Get(208)); //play
      dlg.Add(GUILocalizeStrings.Get(926)); //add to playlist

      if (pins.Count > 0)
      {
        if (ageConfirmed)
        {
          dlg.Add("Lock content"); //Lock content
        }
        else
        {
          dlg.Add("Unlock content"); //Unlock content
        }
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      switch (dlg.SelectedLabel)
      {
        case 0: // Delete
          OnDeleteItem(item);
          break;
        case 1: // IMDB
          OnInfo(itemNo);
          break;
        case 2: // play
          OnClick(itemNo);
          break;
        case 3: //add to playlist
          OnQueueItem(itemNo);
          break;
        case 4: //Lock or unlock content
          OnContentLock();
          break;
      }
    }

    private void Dialog_ProtectedContent(GUIDialogMenu dlg)
    {
      if (pins.Count > 0)
      {
        dlg.Reset();
        dlg.SetHeading(498); // menu

        if (ageConfirmed)
        {
          dlg.Add(GUILocalizeStrings.Get(1240)); //Lock content
        }
        else
        {
          dlg.Add(GUILocalizeStrings.Get(1241)); //Unlock content
        }
      }
      // Show menu
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      switch (dlg.SelectedLabel)
      {
        case 0: //Lock or unlock content
          OnContentLock();
          break;
      }
    }

    protected override void OnQueueItem(int itemIndex)
    {
      // add item 2 playlist
      GUIListItem listItem = facadeLayout[itemIndex];
      ArrayList files = new ArrayList();
      if (handler.CurrentLevel < handler.MaxLevels - 1)
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
            VideoDatabase.GetFiles(movie.ID, ref files);
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
        VideoDatabase.GetFiles(movie.ID, ref files);
        foreach (string file in files)
        {
          listItem.Path = file;
          AddItemToPlayList(listItem);
        }
      }
      //move to next item
      GUIControl.SelectItemControl(GetID, facadeLayout.GetID, itemIndex + 1);
    }

    #endregion

    protected override void LoadDirectory(string strNewDirectory)
    {
      GUIWaitCursor.Show();
      GUIListItem SelectedItem = facadeLayout.SelectedListItem;
      if (SelectedItem != null)
      {
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          m_history.Set(SelectedItem.Label, currentFolder);
        }
      }
      currentFolder = strNewDirectory;

      GUIControl.ClearControl(GetID, facadeLayout.GetID);

      string objectCount = string.Empty;

      ArrayList itemlist = new ArrayList();
      ArrayList movies = ((VideoViewHandler)handler).Execute();

      if (handler.CurrentLevel > 0)
      {
        GUIListItem listItem = new GUIListItem("..");
        listItem.Path = string.Empty;
        listItem.IsFolder = true;
        Util.Utils.SetDefaultIcons(listItem);
        itemlist.Add(listItem);
      }

      foreach (IMDBMovie movie in movies)
      {
        GUIListItem item = new GUIListItem();
        item.Label = movie.Title;
        if (handler.CurrentLevel + 1 < handler.MaxLevels)
        {
          item.IsFolder = true;
        }
        else
        {
          item.IsFolder = false;
        }
       
        item.Path = movie.File;

        // Protected movies validation, checks item and if it is inside protected shares.
        // If item is inside PIN protected share, checks if user validate PIN with Unlock
        // command from context menu and returns "True" if all is ok and item will be visible
        // in movie list. Non-protected item will skip check and will be always visible.
        if (!string.IsNullOrEmpty(item.Path))
        {
          if(!CheckItem(item))
            continue;
        }
        //
        item.Duration = movie.RunTime * 60;
        item.AlbumInfoTag = movie;
        item.Year = movie.Year;
        item.DVDLabel = movie.DVDLabel;
        item.Rating = movie.Rating;
        item.IsPlayed = movie.Watched > 0 ? true : false;

        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);

        itemlist.Add(item);
      }

      // Clear info for zero result
      if (itemlist.Count == 0)
      {
        GUIListItem item = new GUIListItem();
        item.Label = GUILocalizeStrings.Get(284);
        IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
        movie = new IMDBMovie();
        item.AlbumInfoTag = movie;
        movie.SetProperties(false);
        itemlist.Add(item);
      }

      int itemIndex = 0;

      foreach (GUIListItem item in itemlist)
      {
        facadeLayout.Add(item);
      }

      string selectedItemLabel = m_history.Get(currentFolder);
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

      if (handler.CurrentLevel < handler.MaxLevels)
      {
        if (handler.CurrentLevelWhere.ToLower() == "genre")
        {
          SetGenreThumbs(itemlist);
        }
        else if (handler.CurrentLevelWhere.ToLower() == "actor")
        {
          SetActorThumbs(itemlist);
        }
          //if (handler.CurrentLevelWhere.ToLower() == "title")        
        else
        {
          // Assign thumbnails also for the custom views. Bugfix for Mantis 0001471: 
          // Cover image thumbs missing in My Videos when view Selection is by "watched"
          SetIMDBThumbs(itemlist);
        }
      }
      else
      {
        SetIMDBThumbs(itemlist);
      }

      OnSort();

      SwitchLayout();
      UpdateButtonStates();

      // quite ugly to loop again to search the selected item...
      for (int i = 0; i < facadeLayout.Count; ++i)
      {
        GUIListItem item = facadeLayout[itemIndex];
        if (item.Label == selectedItemLabel)
        {
          GUIControl.SelectItemControl(GetID, facadeLayout.GetID, itemIndex);
          break;
        }
        itemIndex++;
      }
      if (currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, currentSelectedItem);
      }

      GUIWaitCursor.Hide();

      // 07.11.2010 Put in comment by Deda (it's redundant now-button Layout is already in focus state)
      //if (itemCount == 0)
      //{
      //  btnViews.Focus = true;
      //}
    }
    
    protected void SetGenreThumbs(ArrayList itemlist)
    {
      foreach (GUIListItem item in itemlist)
      {
        // get the genre somewhere since the label isn't set yet.
        IMDBMovie Movie = item.AlbumInfoTag as IMDBMovie;
        string GenreCover = Util.Utils.GetCoverArt(Thumbs.MovieGenre, Movie.SingleGenre);

        SetItemThumb(item, GenreCover);
      }
    }

    protected void SetActorThumbs(ArrayList itemlist)
    {
      foreach (GUIListItem item in itemlist)
      {
        // get the genre somewhere since the label isn't set yet.
        IMDBMovie Movie = item.AlbumInfoTag as IMDBMovie;
        string ActorCover = Util.Utils.GetCoverArt(Thumbs.MovieActors, Movie.Actor);

        SetItemThumb(item, ActorCover);
      }
    }

    protected void SetItemThumb(GUIListItem aItem, string aThumbPath)
    {
      if (!string.IsNullOrEmpty(aThumbPath))
      {
        aItem.IconImage = aThumbPath;
        aItem.IconImageBig = aThumbPath;

        // check whether there is some larger cover art
        string LargeCover = Util.Utils.ConvertToLargeCoverArt(aThumbPath);
        if (Util.Utils.FileExistsInCache(LargeCover))
        {
          aItem.ThumbnailImage = LargeCover;
        }
        else
        {
          aItem.ThumbnailImage = aThumbPath;
        }
      }
    }

    protected override void SetLabels()
    {
      base.SetLabels();
      for (int i = 0; i < facadeLayout.Count; ++i)
      {
        GUIListItem item = facadeLayout[i];
        ((VideoViewHandler)handler).SetLabel(item.AlbumInfoTag as IMDBMovie, ref item);
      }
    }

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
        // Delete covers
        FanArt.DeleteCovers(movie.Title, movie.ID);
        // Delete fanarts
        FanArt.DeleteFanarts(movie.File, movie.Title);
        VideoDatabase.DeleteMovieInfoById(movie.ID);
      }
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
          OnVideoArtistInfo(actor);
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
        return;
      }
      infoDlg.Actor = actor;
      infoDlg.DoModal(GetID);
    }

    // Changed - covers with the same movie name
    private void SetIMDBThumbs(ArrayList items)
    {
      for (int x = 0; x < items.Count; ++x)
      {
        string coverArtImage = string.Empty;
        GUIListItem listItem = (GUIListItem)items[x];
        IMDBMovie movie = listItem.AlbumInfoTag as IMDBMovie;
        if (movie != null)
        {
          if (movie.ID >= 0)
          {
            //coverArtImage = Util.Utils.GetCoverArt(Thumbs.MovieTitle, movie.Title);
            // Title suffix for problem with covers and movie with the same name
            string titleExt = movie.Title + "{" + movie.ID + "}";
            coverArtImage = Util.Utils.GetCoverArt(Thumbs.MovieTitle, titleExt);
            if (Util.Utils.FileExistsInCache(coverArtImage))
            {
              listItem.ThumbnailImage = coverArtImage;
              listItem.IconImageBig = coverArtImage;
              listItem.IconImage = coverArtImage;
            }
          }
          //else if (movie.Actor != string.Empty)
          //{            
          //  coverArtImage = MediaPortal.Util.Utils.GetCoverArt(Thumbs.MovieActors, movie.Actor);
          //  if (System.IO.File.Exists(coverArtImage))
          //  {
          //    listItem.ThumbnailImage = coverArtImage;
          //    listItem.IconImageBig = coverArtImage;
          //    listItem.IconImage = coverArtImage;
          //  }
          //}
          //else if (movie.SingleGenre != string.Empty)
          //{
          //  coverArtImage = MediaPortal.Util.Utils.GetCoverArt(Thumbs.MovieGenre, movie.SingleGenre);
          //  if (System.IO.File.Exists(coverArtImage))
          //  {
          //    listItem.ThumbnailImage = coverArtImage;
          //    listItem.IconImageBig = coverArtImage;
          //    listItem.IconImage = coverArtImage;
          //  }
          //}
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
      }
    }

    // Changed - covers with the same movie name
    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
      if (movie == null)
      {
        movie = new IMDBMovie();
      }
      movie.SetProperties(false);
      if (movie.ID >= 0)
      {
        string coverArtImage;
        //coverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, movie.Title);
        string titleExt = movie.Title + "{" + movie.ID + "}";
        coverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
        if (Util.Utils.FileExistsInCache(coverArtImage))
        {
          facadeLayout.FilmstripLayout.InfoImageFileName = coverArtImage;
        }
      }
      else if (movie.Actor != string.Empty)
      {
        string coverArtImage;
        coverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, movie.Actor);
        if (Util.Utils.FileExistsInCache(coverArtImage))
        {
          facadeLayout.FilmstripLayout.InfoImageFileName = coverArtImage;
        }
      }
    }

    // Show or hide protected content
    private void OnContentLock()
    {
      if (!ageConfirmed)
      {
        if (RequestPin())
        {
          ageConfirmed = true;
          LoadDirectory(currentFolder);
          GUIPropertyManager.SetProperty("#MyVideos.PinLocked", "false");
        }
        return;
      }

      ageConfirmed = false;
      LoadDirectory(currentFolder);
      GUIPropertyManager.SetProperty("#MyVideos.PinLocked", "true");
    }

    // Get all pins for videos protected folders
    private void GetSharePins(ref ArrayList pins)
    {
      using (Profile.Settings xmlreader = new MPSettings())
      {
        pins = new ArrayList();

        for (int index = 0; index < 128; index++)
        {
          string sharePin = String.Format("pincode{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePinData = Util.Utils.DecryptPin(xmlreader.GetValueAsString("movies", sharePin, ""));
          string sharePathData = xmlreader.GetValueAsString("movies", sharePath, "");

          if (!string.IsNullOrEmpty(sharePinData))
          {
            pins.Add(sharePinData + "|" + sharePathData);
          }
        }
      }
    }

    // Protected content PIN validation (any PIN from video protected folders is valid)
    private bool RequestPin()
    {
      bool retry = true;
      bool sucess = false;
      currentProtectedShare.Clear();
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

        foreach (string p in pins)
        {
          char[] splitter = {'|'};
          string[] pin = p.Split(splitter);

          if (iPincode != Convert.ToInt32(pin[0]))
          {
            continue;
          }
          if (iPincode == Convert.ToInt32(pin[0]))
          {
            currentProtectedShare.Add(pin[1]);
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
      return false;
    }

    // Skin properties for locked/unlocked indicator
    private void SetPinLockProperties()
    {
      if (pins.Count == 0)
      {
        GUIPropertyManager.SetProperty("#MyVideos.PinLocked", "");
      }
      else if (ageConfirmed)
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
    private bool CheckItem(GUIListItem item)
    {
      string directory = Path.GetDirectoryName(item.Path); // item path
      VirtualDirectory vDir = new VirtualDirectory();
      // Get protected share paths for videos
      vDir.LoadSettings("movies"); 

      // Check if item belongs to protected shares
      int pincode = 0;
      bool folderPinProtected = vDir.IsProtectedShare(directory, out pincode);
      
      bool success = false;

      // User unlocked share/shares with PIN and item is within protected shares
      if (folderPinProtected && ageConfirmed) 
      {
        // Iterate unlocked shares against current item path
        foreach (string share in currentProtectedShare)
        {
          if (!directory.ToLower().Contains(share.ToLower()))
          {
            continue;
          }
          else // item belongs to unlocked shares and will be displayed
          {
            success = true;
            break;
          }
        }
        // current item is not within unlocked shares, 
        // don't show item and go to the next item
        if (!success)
        {
          return false;
        }
        else // current item is within unlocked shares, show it
        {
          return true;
        }
      }
      // Nothing unlocked and item belongs to protected shares,
      // don't show item and go to the next item
      else if (folderPinProtected && !ageConfirmed)
      {
        return false;
      }
      // Item is not inside protected shares, show it
      return true;
    }
  }
}