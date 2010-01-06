#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.Collections;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Util;
using MediaPortal.Video.Database;

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
    private View[,] views;
    private bool[,] sortasc;
    private VideoSort.SortMethod[,] sortby;

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

    protected override View CurrentView
    {
      get
      {
        if (handler.View != null)
        {
          if (views == null)
          {
            views = new View[handler.Views.Count,50];

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
                views[i, j] = GetViewNumber(def.DefaultView);
              }
            }
          }

          return views[handler.Views.IndexOf(handler.View), handler.CurrentLevel];
        }
        else
        {
          return View.List;
        }
      }
      set { views[handler.Views.IndexOf(handler.View), handler.CurrentLevel] = value; }
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

    protected override bool AllowView(View view)
    {
      return base.AllowView(view);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeView.Focus)
        {
          GUIListItem item = facadeView[0];
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
        GUIListItem item = facadeView[0];
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
      string view = VideoState.View;
      if (view == string.Empty)
      {
        view = ((ViewDefinition)handler.Views[0]).Name;
      }

      handler.CurrentView = view;
      base.OnPageLoad();
      LoadDirectory(currentFolder);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      currentSelectedItem = facadeView.SelectedListItemIndex;
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
      GUIListItem item = facadeView.SelectedListItem;
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
          handler.Select(movie);
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
        GUIVideoFiles.PlayMovie(movie.ID);
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = facadeView.SelectedListItem;
      int itemNo = facadeView.SelectedListItemIndex;
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
        return;
      }
      if (handler.CurrentLevelWhere == "actor")
      {
        IMDBActor actor = VideoDatabase.GetActorInfo(movie.actorId);
        if (actor != null)
        {
          dlg.Reset();
          dlg.SetHeading(498); // menu
          dlg.Add(GUILocalizeStrings.Get(368)); //IMDB

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
          }
          return;
        }
      }
      if (movie.ID < 0)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(498); // menu
      dlg.Add(GUILocalizeStrings.Get(925)); //delete
      dlg.Add(GUILocalizeStrings.Get(368)); //IMDB
      dlg.Add(GUILocalizeStrings.Get(208)); //play
      dlg.Add(GUILocalizeStrings.Get(926)); //add to playlist

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
      }
    }

    protected override void OnQueueItem(int itemIndex)
    {
      // add item 2 playlist
      GUIListItem listItem = facadeView[itemIndex];
      ArrayList files = new ArrayList();
      if (handler.CurrentLevel < handler.MaxLevels - 1)
      {
        //queue
        handler.Select(listItem.AlbumInfoTag as IMDBMovie);
        ArrayList movies = handler.Execute();
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
      GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex + 1);
    }

    #endregion

    protected override void LoadDirectory(string strNewDirectory)
    {
      GUIWaitCursor.Show();
      GUIListItem SelectedItem = facadeView.SelectedListItem;
      if (SelectedItem != null)
      {
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          m_history.Set(SelectedItem.Label, currentFolder);
        }
      }
      currentFolder = strNewDirectory;

      GUIControl.ClearControl(GetID, facadeView.GetID);

      string objectCount = string.Empty;

      ArrayList itemlist = new ArrayList();
      ArrayList movies = handler.Execute();

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
        item.Duration = movie.RunTime * 60;
        item.AlbumInfoTag = movie;
        item.Year = movie.Year;
        item.DVDLabel = movie.DVDLabel;
        item.Rating = movie.Rating;
        item.IsPlayed = movie.Watched > 0 ? true : false;

        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);

        itemlist.Add(item);
      }

      int itemIndex = 0;
      foreach (GUIListItem item in itemlist)
      {
        facadeView.Add(item);
      }

      string selectedItemLabel = m_history.Get(currentFolder);
      if (string.IsNullOrEmpty(selectedItemLabel) && facadeView.SelectedListItem != null)
      {
        selectedItemLabel = facadeView.SelectedListItem.Label;
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

      SwitchView();

      // quite ugly to loop again to search the selected item...
      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[itemIndex];
        if (item.Label == selectedItemLabel)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex);
          break;
        }
        itemIndex++;
      }
      if (currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, currentSelectedItem);
      }

      GUIWaitCursor.Hide();

      if (itemCount == 0)
      {
        btnViews.Focus = true;
      }
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
        if (File.Exists(LargeCover))
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
      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        handler.SetLabel(item.AlbumInfoTag as IMDBMovie, ref item);
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

      currentSelectedItem = facadeView.SelectedListItemIndex;
      if (currentSelectedItem > 0)
      {
        currentSelectedItem--;
      }
      LoadDirectory(currentFolder);
      if (currentSelectedItem >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, currentSelectedItem);
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

    protected override void OnInfo(int itemIndex)
    {
      GUIListItem item = facadeView[itemIndex];
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
            coverArtImage = Util.Utils.GetCoverArt(Thumbs.MovieTitle, movie.Title);
            if (File.Exists(coverArtImage))
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
          if (File.Exists(coverArtImage))
          {
            listItem.ThumbnailImage = coverArtImage;
          }
        }
      }
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      IMDBMovie movie = item.AlbumInfoTag as IMDBMovie;
      if (movie == null)
      {
        movie = new IMDBMovie();
      }
      movie.SetProperties();
      if (movie.ID >= 0)
      {
        string coverArtImage;
        coverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, movie.Title);
        if (File.Exists(coverArtImage))
        {
          facadeView.FilmstripView.InfoImageFileName = coverArtImage;
        }
      }
      else if (movie.Actor != string.Empty)
      {
        string coverArtImage;
        coverArtImage = Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, movie.Actor);
        if (File.Exists(coverArtImage))
        {
          facadeView.FilmstripView.InfoImageFileName = coverArtImage;
        }
      }
    }
  }
}