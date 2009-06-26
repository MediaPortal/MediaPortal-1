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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Music.Amazon;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Util;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for GUIMusicBaseWindow.
  /// </summary>
  public class GUIMusicBaseWindow : GUIWindow
  {
    #region enums

    protected enum Level
    {
      Root,
      Sub
    }

    public enum View
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
      Albums = 3,
      FilmStrip = 4,
      PlayList = 5
    }

    protected enum PlayNowJumpToType //SV Added by SteveV 2006-09-07
    {
      None = 0,
      NowPlayingAlways,
      NowPlayingMultipleItems,
      CurrentPlaylistAlways,
      CurrentPlaylistMultipleItems,
      FullscreenIfVizEnabledAlways,
      FullscreenIfVizEnabledMultipleItems,
    }

    #endregion

    #region Base variables

    public static bool _createMissingFolderThumbCache = true;
    public static bool _createMissingFolderThumbs = false;
    public bool _useFolderThumbs = true;
    public bool _showSortButton = false;

    protected View currentView = View.List;
    protected View currentViewRoot = View.List;
    protected MusicSort.SortMethod currentSortMethod = MusicSort.SortMethod.Name;
    protected MusicSort.SortMethod currentSortMethodRoot = MusicSort.SortMethod.Name;
    protected bool m_bSortAscending;
    protected bool m_bSortAscendingRoot;
    protected string m_strPlayListPath = string.Empty;
    private bool m_bUseID3 = false;
    private bool _autoShuffleOnLoad = false;


    protected MusicViewHandler handler;
    protected MusicDatabase m_database;

    private const string defaultTrackTag = "[%track%. ][%artist% - ][%title%]";
    private const string albumTrackTag = "[%track%. ][%artist% - ][%title%]";

    private string[] _sortModes = {
                                    "Name", "Date", "Size", "Track", "Duration", "Title", "Artist", "Album", "Filename",
                                    "Rating"
                                  };

    private string[] _defaultSortTags1 = {
                                           defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag,
                                           defaultTrackTag, defaultTrackTag, defaultTrackTag, albumTrackTag,
                                           defaultTrackTag, defaultTrackTag
                                         };

    private string[] _defaultSortTags2 = {
                                           "%duration%", "%year%", "%filesize%", "%duration%", "%duration%", "%duration%"
                                           , "%duration%", "%duration%", "%filesize%", "%rating%"
                                         };

    private string[] _sortTags1 = new string[20];
    private string[] _sortTags2 = new string[20];
    protected PlayListPlayer playlistPlayer;

    protected PlayNowJumpToType PlayNowJumpTo = PlayNowJumpToType.None;
    protected bool UsingInternalMusicPlayer = false;

    protected bool PlayAllOnSingleItemPlayNow = false;
    protected string _currentPlaying = string.Empty;

    #endregion

    #region SkinControls

    [SkinControl(50)] protected GUIFacadeControl facadeView = null;
    [SkinControl(2)] protected GUIButtonControl btnViewAs = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(6)] protected GUIButtonControl btnViews = null;

    [SkinControl(8)] protected GUIButtonControl btnSearch = null;
    [SkinControl(12)] protected GUIButtonControl btnPlayCd = null;
    [SkinControl(10)] protected GUIButtonControl btnSavedPlaylists = null;

    #endregion

    #region Constructor / Destructor

    public GUIMusicBaseWindow()
    {
      playlistPlayer = PlayListPlayer.SingletonPlayer;

      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string playNowJumpTo = xmlreader.GetValueAsString("musicmisc", "playnowjumpto", "none");

        switch (playNowJumpTo)
        {
          case "none":
            PlayNowJumpTo = PlayNowJumpToType.None;
            break;

          case "nowPlayingAlways":
            PlayNowJumpTo = PlayNowJumpToType.NowPlayingAlways;
            break;

          case "nowPlayingMultipleItems":
            PlayNowJumpTo = PlayNowJumpToType.NowPlayingMultipleItems;
            break;

          case "currentPlaylistAlways":
            PlayNowJumpTo = PlayNowJumpToType.CurrentPlaylistAlways;
            break;

          case "currentPlaylistMultipleItems":
            PlayNowJumpTo = PlayNowJumpToType.CurrentPlaylistMultipleItems;
            break;

          case "fullscreenAlways":
            PlayNowJumpTo = PlayNowJumpToType.FullscreenIfVizEnabledAlways;
            break;

          case "fullscreenMultipleItems":
            PlayNowJumpTo = PlayNowJumpToType.FullscreenIfVizEnabledMultipleItems;
            break;

          default:
            PlayNowJumpTo = PlayNowJumpToType.None;
            break;
        }

        _autoShuffleOnLoad = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", false);
      }

      UsingInternalMusicPlayer = BassMusicPlayer.IsDefaultMusicPlayer;
    }

    #endregion

    #region Serialisation

    protected virtual void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _createMissingFolderThumbCache = xmlreader.GetValueAsBool("thumbnails", "musicfolderondemand", true);
        _createMissingFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs", false);
        _useFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", true);
        _showSortButton = xmlreader.GetValueAsBool("musicfiles", "showSortButton", true);


        int defaultView = (int) View.List;
        int defaultSort = (int) MusicSort.SortMethod.Name;
        bool defaultAscending = true;
        if ((handler != null) && (handler.View != null) && (handler.View.Filters != null) &&
            (handler.View.Filters.Count > 0))
        {
          FilterDefinition def = (FilterDefinition) handler.View.Filters[0];
          defaultView = (int) GetViewNumber(def.DefaultView);
          defaultSort = (int) GetSortMethod(def.DefaultSort);
          defaultAscending = def.SortAscending;
        }
        currentView = (View) xmlreader.GetValueAsInt(SerializeName, "view", defaultView);
        currentViewRoot = (View) xmlreader.GetValueAsInt(SerializeName, "viewroot", defaultView);

        currentSortMethod = (MusicSort.SortMethod) xmlreader.GetValueAsInt(SerializeName, "sortmethod", defaultSort);
        currentSortMethodRoot =
          (MusicSort.SortMethod) xmlreader.GetValueAsInt(SerializeName, "sortmethodroot", defaultSort);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", defaultAscending);
        m_bSortAscendingRoot = xmlreader.GetValueAsBool(SerializeName, "sortascroot", defaultAscending);
        m_bUseID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);

        for (int i = 0; i < _sortModes.Length; ++i)
        {
          _sortTags1[i] = xmlreader.GetValueAsString("mymusic", _sortModes[i] + "1", _defaultSortTags1[i]);
          _sortTags2[i] = xmlreader.GetValueAsString("mymusic", _sortModes[i] + "2", _defaultSortTags2[i]);
        }

        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";

        m_strPlayListPath = xmlreader.GetValueAsString("music", "playlists", playListFolder);
        m_strPlayListPath = Util.Utils.RemoveTrailingSlash(m_strPlayListPath);
      }
      SwitchView();
    }

    protected MusicSort.SortMethod GetSortMethod(string s)
    {
      switch (s.Trim().ToLower())
      {
        case "name":
          return MusicSort.SortMethod.Name;
        case "date":
          return MusicSort.SortMethod.Date;
        case "size":
          return MusicSort.SortMethod.Size;
        case "track":
          return MusicSort.SortMethod.Track;
        case "duration":
          return MusicSort.SortMethod.Duration;
        case "title":
          return MusicSort.SortMethod.Title;
        case "artist":
          return MusicSort.SortMethod.Artist;
        case "album":
          return MusicSort.SortMethod.Album;
        case "filename":
          return MusicSort.SortMethod.Filename;
        case "albumartist":
          return MusicSort.SortMethod.AlbumArtist;
        case "rating":
          return MusicSort.SortMethod.Rating;
        case "year":
          return MusicSort.SortMethod.Year;
      }
      if (!string.IsNullOrEmpty(s))
      {
        Log.Error("GUIMusicBaseWindow::GetSortMethod: Unknown String - " + s);
      }
      return MusicSort.SortMethod.Name;
    }

    protected View GetViewNumber(string s)
    {
      switch (s.Trim().ToLower())
      {
        case "list":
          return View.List;
        case "icons":
          return View.Icons;
        case "big icons":
          return View.LargeIcons;
        case "largeicons":
          return View.LargeIcons;
        case "albums":
          return View.Albums;
        case "filmstrip":
          return View.FilmStrip;
        case "playlist":
          return View.PlayList;
      }
      if (!string.IsNullOrEmpty(s))
      {
        Log.Error("GUIMusicBaseWindow::GetViewNumber: Unknown String - " + s);
      }
      return View.List;
    }

    protected virtual void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue(SerializeName, "view", (int) currentView);
        xmlwriter.SetValue(SerializeName, "viewroot", (int) currentViewRoot);
        xmlwriter.SetValue(SerializeName, "sortmethod", (int) currentSortMethod);
        xmlwriter.SetValue(SerializeName, "sortmethodroot", (int) currentSortMethodRoot);
        xmlwriter.SetValueAsBool(SerializeName, "sortasc", m_bSortAscending);
        xmlwriter.SetValueAsBool(SerializeName, "sortascroot", m_bSortAscendingRoot);
      }
    }

    #endregion

    protected bool UseID3
    {
      get { return m_bUseID3; }
      set { m_bUseID3 = value; }
    }

    protected virtual bool AllowView(View view)
    {
      if (view == View.PlayList)
      {
        return false;
      }
      return true;
    }

    protected virtual bool AllowSortMethod(MusicSort.SortMethod method)
    {
      return true;
    }

    protected virtual View CurrentView
    {
      get { return currentView; }
      set { currentView = value; }
    }

    protected virtual MusicSort.SortMethod CurrentSortMethod
    {
      get { return currentSortMethod; }
      set { currentSortMethod = value; }
    }

    protected virtual bool CurrentSortAsc
    {
      get { return m_bSortAscending; }
      set { m_bSortAscending = value; }
    }

    protected virtual string SerializeName
    {
      get { return "musicbase"; }
    }

    protected bool ViewByIcon
    {
      get
      {
        if (CurrentView != View.List)
        {
          return true;
        }
        return false;
      }
    }

    protected bool ViewByLargeIcon
    {
      get
      {
        if (CurrentView == View.LargeIcons)
        {
          return true;
        }
        return false;
      }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYLIST);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_IMPORT_TRACK)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeTrack(facadeView, GetID);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_IMPORT_DISC)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeDisc(facadeView, GetID);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_CANCEL_IMPORT)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.Cancel();
        return;
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS)
      {
        _currentPlaying = message.Label;
        facadeView.OnMessage(message);
      }
      return base.OnMessage(message);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnViewAs)
      {
        bool shouldContinue = false;
        do
        {
          shouldContinue = false;
          switch (CurrentView)
          {
            case View.List:
              CurrentView = View.PlayList;
              if (!AllowView(CurrentView) || facadeView.PlayListView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.Playlist;
              }
              break;

            case View.PlayList:
              CurrentView = View.Icons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
              }
              break;

            case View.Icons:
              CurrentView = View.LargeIcons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
              }
              break;

            case View.LargeIcons:
              CurrentView = View.Albums;
              if (!AllowView(CurrentView) || facadeView.AlbumListView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.AlbumView;
              }
              break;

            case View.Albums:
              CurrentView = View.FilmStrip;
              if (!AllowView(CurrentView) || facadeView.FilmstripView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
              }
              break;

            case View.FilmStrip:
              CurrentView = View.List;
              if (!AllowView(CurrentView) || facadeView.ListView == null)
              {
                shouldContinue = true;
              }
              else
              {
                facadeView.View = GUIFacadeControl.ViewMode.List;
              }
              break;
          }
        } while (shouldContinue);

        SelectCurrentItem();
        GUIControl.FocusControl(GetID, controlId);
        return;
      } //if (control == btnViewAs)

      if (control == btnSortBy)
      {
        OnShowSort();
      }

      if (control == btnViews)
      {
        OnShowViews();
      }

      if (control == btnSavedPlaylists)
      {
        OnShowSavedPlaylists(m_strPlayListPath);
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int iItem = (int) msg.Param1;
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnInfo(iItem);
        }
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(iItem);
        }
        if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
        {
          OnQueueItem(iItem);
        }
      }
    }

    protected void SelectCurrentItem()
    {
      int iItem = facadeView.SelectedListItemIndex;
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      }
      UpdateButtonStates();
    }

    protected virtual void UpdateButtonStates()
    {
      GUIPropertyManager.SetProperty("#view", handler.LocalizedCurrentView);
      if (GetID == (int) Window.WINDOW_MUSIC_GENRE)
      {
        GUIPropertyManager.SetProperty("#currentmodule",
                                       String.Format("{0}/{1}", GUILocalizeStrings.Get(100005),
                                                     handler.LocalizedCurrentView));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
      }

      facadeView.IsVisible = false;
      facadeView.IsVisible = true;
      GUIControl.FocusControl(GetID, facadeView.GetID);

      string strLine = string.Empty;
      View view = CurrentView;
      switch (view)
      {
        case View.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case View.LargeIcons:
          strLine = GUILocalizeStrings.Get(417);
          break;
        case View.Albums:
          strLine = GUILocalizeStrings.Get(529);
          break;
        case View.FilmStrip:
          strLine = GUILocalizeStrings.Get(733);
          break;
        case View.PlayList:
          strLine = GUILocalizeStrings.Get(101);
          break;
      }
      btnViewAs.Label = strLine;

      switch (CurrentSortMethod)
      {
        case MusicSort.SortMethod.Name:
          strLine = GUILocalizeStrings.Get(103);
          break;
        case MusicSort.SortMethod.Date:
          strLine = GUILocalizeStrings.Get(104);
          break;
        case MusicSort.SortMethod.Year:
          strLine = GUILocalizeStrings.Get(104); // Also display Date for Year
          break;
        case MusicSort.SortMethod.Size:
          strLine = GUILocalizeStrings.Get(105);
          break;
        case MusicSort.SortMethod.Track:
          strLine = GUILocalizeStrings.Get(266);
          break;
        case MusicSort.SortMethod.Duration:
          strLine = GUILocalizeStrings.Get(267);
          break;
        case MusicSort.SortMethod.Title:
          strLine = GUILocalizeStrings.Get(268);
          break;
        case MusicSort.SortMethod.Artist:
          strLine = GUILocalizeStrings.Get(269);
          break;
        case MusicSort.SortMethod.Album:
          strLine = GUILocalizeStrings.Get(270);
          break;
        case MusicSort.SortMethod.Filename:
          strLine = GUILocalizeStrings.Get(363);
          break;
        case MusicSort.SortMethod.Rating:
          strLine = GUILocalizeStrings.Get(367);
          break;
        case MusicSort.SortMethod.AlbumArtist:
          strLine = GUILocalizeStrings.Get(269); // Also display Artist for AlbumArtist
          break;
      }

      if (btnSortBy != null)
      {
        btnSortBy.Label = strLine;
        btnSortBy.IsAscending = CurrentSortAsc;
      }
    }

    protected virtual void OnClick(int item)
    {
    }

    protected virtual void OnQueueItem(int item)
    {
    }

    protected void OnSetRating(int itemNumber)
    {
      GUIListItem item = facadeView[itemNumber];
      if (item == null)
      {
        return;
      }
      MusicTag tag = item.MusicTag as MusicTag;
      GUIDialogSetRating dialog = (GUIDialogSetRating) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_RATING);
      if (tag != null)
      {
        dialog.Rating = tag.Rating;
        dialog.SetTitle(String.Format("{0}-{1}", tag.Artist, tag.Title));
      }
      dialog.FileName = item.Path;
      dialog.DoModal(GetID);
      if (tag != null)
      {
        tag.Rating = dialog.Rating;
      }
      m_database.SetRating(item.Path, dialog.Rating);
      if (dialog.Result == GUIDialogSetRating.ResultCode.Previous)
      {
        while (itemNumber > 0)
        {
          itemNumber--;
          item = facadeView[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }

      if (dialog.Result == GUIDialogSetRating.ResultCode.Next)
      {
        while (itemNumber + 1 < facadeView.Count)
        {
          itemNumber++;
          item = facadeView[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }
    }

    protected override void OnPageLoad()
    {
      // watch if we're still playing a last.fm radio stream
      //if (g_Player.Playing)
      //  if (Util.Utils.IsLastFMStream(g_Player.CurrentFile))
      //    g_Player.Stop();

      if (m_database == null)
      {
        m_database = MusicDatabase.Instance;
      }

      if (handler == null)
      {
        handler = new MusicViewHandler();
      }

      LoadSettings();

      if (btnSortBy != null)
      {
        btnSortBy.SortChanged += new SortEventHandler(SortChanged);
      }

      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      // Save view
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("music", "startWindow", MusicState.StartWindow.ToString());
        xmlwriter.SetValue("music", "startview", MusicState.View);
      }
      base.OnPageDestroy(newWindowId);
    }

    protected void LoadPlayList(string strPlayList)
    {
      IPlayListIO loader = PlayListFactory.CreateIO(strPlayList);
      if (loader == null)
      {
        return;
      }
      PlayList playlist = new PlayList();

      if (!loader.Load(playlist, strPlayList))
      {
        TellUserSomethingWentWrong();
        return;
      }

      if (_autoShuffleOnLoad)
      {
        //PseudoRandomNumberGenerator r = new PseudoRandomNumberGenerator();
        //int shuffleCount = r.Next() % 50;
        //for (int i = 0; i < shuffleCount; ++i)
        {
          playlist.Shuffle();
        }
      }

      playlistPlayer.CurrentPlaylistName = Path.GetFileNameWithoutExtension(strPlayList);
      if (playlist.Count == 1)
      {
        Log.Info("GUIMusic:Play: play single playlist item - {0}", playlist[0].FileName);
        // Default to type Music, when a playlist has been selected from My Music
        g_Player.Play(playlist[0].FileName, g_Player.MediaType.Music);
        return;
      }

      // clear current playlist
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();

      Song song = new Song();
      // add each item of the playlist to the playlistplayer
      for (int i = 0; i < playlist.Count; ++i)
      {
        PlayListItem playListItem = playlist[i];
        m_database.GetSongByFileName(playListItem.FileName, ref song);
        MusicTag tag = new MusicTag();
        tag = song.ToMusicTag();
        playListItem.MusicTag = tag;
        playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playListItem);
      }

      // if we got a playlist
      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0)
      {
        // then get 1st song
        playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
        PlayListItem item = playlist[0];

        // and start playing it
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Reset();
        playlistPlayer.Play(0);

        // and activate the playlist window if its not activated yet
        if (GetID == GUIWindowManager.ActiveWindow)
        {
          GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYLIST);
        }
      }
    }

    private void TellUserSomethingWentWrong()
    {
      GUIDialogOK dlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(6);
        dlgOK.SetLine(1, 477);
        dlgOK.SetLine(2, string.Empty);
        dlgOK.DoModal(GetID);
      }
    }

    protected virtual void OnSort()
    {
      SetLabels();
      facadeView.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));
      UpdateButtonStates();
    }

    protected virtual void SetLabels()
    {
      MusicSort.SortMethod method = CurrentSortMethod;
      TimeSpan totalPlayingTime = new TimeSpan();

      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        MusicTag tag = (MusicTag) item.MusicTag;
        if (tag != null)
        {
          string trackNr = String.Format("{0:##00}", tag.Track);
          string fileSize = Util.Utils.GetSize(item.Size);
          string year = tag.Year.ToString();
          string filename = Util.Utils.GetFilename(item.Path);

          // For an index view, don't translate the duration
          string duration = "";

          // eliminates bug mentioned in http://forum.team-mediaportal.com/mymusic_list_shows_song_length_just_full-t28125.html
          // testing on MusicState.Startwindow is a bit dirty, but there is no other way to determine Shares View
          // Handler.View is null when program starts but is never set another time
          // so it is not possible to check if shares view is selected
          if ((handler.View != null) && (MusicState.StartWindow != 501))
          {
            FilterDefinition filter = (FilterDefinition) handler.View.Filters[handler.CurrentLevel];
            if (filter.SqlOperator != "group")
            {
              switch (CurrentSortMethod)
              {
                case MusicSort.SortMethod.Name:
                  duration = Util.Utils.SecondsToHMSString(tag.Duration);
                  break;
                case MusicSort.SortMethod.Date:
                  duration = Convert.ToString(tag.Duration);
                  break;
                case MusicSort.SortMethod.Size:
                  duration = Convert.ToString(tag.Duration);
                  break;
                case MusicSort.SortMethod.Track:
                  duration = Util.Utils.SecondsToHMSString(tag.Duration);
                  break;
                case MusicSort.SortMethod.Duration:
                  duration = Util.Utils.SecondsToHMSString(tag.Duration);
                  break;
                case MusicSort.SortMethod.Title:
                  duration = Util.Utils.SecondsToHMSString(tag.Duration);
                  break;
                case MusicSort.SortMethod.Artist:
                  duration = Util.Utils.SecondsToHMSString(tag.Duration);
                  break;
                case MusicSort.SortMethod.Album:
                  duration = Util.Utils.SecondsToHMSString(tag.Duration);
                  break;
                case MusicSort.SortMethod.Filename:
                  duration = Convert.ToString(tag.Duration);
                  break;
                case MusicSort.SortMethod.Rating:
                  duration = Convert.ToString(tag.Duration);
                  break;
                case MusicSort.SortMethod.AlbumArtist:
                  duration = Util.Utils.SecondsToHMSString(tag.Duration);
                  break;
              }

              if (tag.Duration > 0)
              {
                totalPlayingTime = totalPlayingTime.Add(new TimeSpan(0, 0, tag.Duration));
              }
            }
            else
            {
              duration = Convert.ToString(tag.Duration);
            }
          }
          else
          {
            duration = Util.Utils.SecondsToHMSString(tag.Duration);

            if (tag.Duration > 0)
            {
              totalPlayingTime = totalPlayingTime.Add(new TimeSpan(0, 0, tag.Duration));
            }
          }

          string rating = tag.Rating.ToString();
          if (tag.Track <= 0)
          {
            trackNr = "";
          }
          if (tag.Year < 1900)
          {
            year = "";
          }

          string date = "";
          if (item.FileInfo != null)
          {
            date = item.FileInfo.ModificationTime.ToShortDateString() + " " +
                   item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          }
          ;

          string line1, line2;
          if (method == MusicSort.SortMethod.AlbumArtist)
          {
            line1 = _sortTags1[(int) MusicSort.SortMethod.Artist]; // Use Artist sort string for AlbumArtist
            line2 = _sortTags2[(int) MusicSort.SortMethod.Artist];
          }
          else
          {
            line1 = _sortTags1[(int) method];
            line2 = _sortTags2[(int) method];
          }
          line1 = Util.Utils.ReplaceTag(line1, "%track%", trackNr);
          line2 = Util.Utils.ReplaceTag(line2, "%track%", trackNr);
          line1 = Util.Utils.ReplaceTag(line1, "%filesize%", fileSize);
          line2 = Util.Utils.ReplaceTag(line2, "%filesize%", fileSize);
          if (handler.View != null)
          {
            FilterDefinition tempfilter = (FilterDefinition) handler.View.Filters[handler.CurrentLevel];
            if (tempfilter.Where == "albumartist")
            {
              line1 = Util.Utils.ReplaceTag(line1, "%artist%", tag.AlbumArtist);
              line2 = Util.Utils.ReplaceTag(line2, "%artist%", tag.AlbumArtist);
            }
            else
            {
              line1 = Util.Utils.ReplaceTag(line1, "%artist%", tag.Artist);
              line2 = Util.Utils.ReplaceTag(line2, "%artist%", tag.Artist);
            }
          }
          else
          {
            line1 = Util.Utils.ReplaceTag(line1, "%artist%", tag.Artist);
            line2 = Util.Utils.ReplaceTag(line2, "%artist%", tag.Artist);
          }
          line1 = Util.Utils.ReplaceTag(line1, "%album%", tag.Album);
          line2 = Util.Utils.ReplaceTag(line2, "%album%", tag.Album);
          line1 = Util.Utils.ReplaceTag(line1, "%title%", tag.Title);
          line2 = Util.Utils.ReplaceTag(line2, "%title%", tag.Title);
          line1 = Util.Utils.ReplaceTag(line1, "%year%", year);
          line2 = Util.Utils.ReplaceTag(line2, "%year%", year);
          line1 = Util.Utils.ReplaceTag(line1, "%filename%", filename);
          line2 = Util.Utils.ReplaceTag(line2, "%filename%", filename);
          line1 = Util.Utils.ReplaceTag(line1, "%rating%", rating);
          line2 = Util.Utils.ReplaceTag(line2, "%rating%", rating);
          line1 = Util.Utils.ReplaceTag(line1, "%duration%", duration);
          line2 = Util.Utils.ReplaceTag(line2, "%duration%", duration);
          line1 = Util.Utils.ReplaceTag(line1, "%date%", date);
          line2 = Util.Utils.ReplaceTag(line2, "%date%", date);
          item.Label = line1;
          item.Label2 = line2;
        }
        /*
        if (tag.Title.Length > 0)
        {
          if (tag.Artist.Length > 0)
          {
            if (tag.Track > 0)
              item.Label = String.Format("{0:00}. {1} - {2}", tag.Track, tag.Artist, tag.Title);
            else
              item.Label = String.Format("{0} - {1}", tag.Artist, tag.Title);
          }
          else
          {
            if (tag.Track > 0)
              item.Label = String.Format("{0:00}. {1} ", tag.Track, tag.Title);
            else
              item.Label = String.Format("{0}", tag.Title);
          }
          if (method == MusicSort.SortMethod.Album)
          {
            if (tag.Album.Length > 0 && tag.Title.Length > 0)
            {
              item.Label = String.Format("{0} - {1}", tag.Album, tag.Title);
            }
          }
          if (method == MusicSort.SortMethod.Rating)
          {
            item.Label2 = String.Format("{0}", tag.Rating);
          }
        }
      }


      if (method == MusicSort.SortMethod.Size || method == MusicSort.SortMethod.Filename)
      {
        if (item.IsFolder) item.Label2 = string.Empty;
        else
        {
          if (item.Size > 0)
          {
            item.Label2 = MediaPortal.Util.Utils.GetSize(item.Size);
          }
          if (method == MusicSort.SortMethod.Filename)
          {
            item.Label = MediaPortal.Util.Utils.GetFilename(item.Path);
          }
        }
      }
      else if (method == MusicSort.SortMethod.Date)
      {
        if (item.FileInfo != null)
        {
          item.Label2 = item.FileInfo.ModificationTime.ToShortDateString() + " " + item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        }
      }
      else if (method != MusicSort.SortMethod.Rating)
      {
        if (tag != null)
        {
          int nDuration = tag.Duration;
          if (nDuration > 0)
          {
            item.Label2 = MediaPortal.Util.Utils.SecondsToHMSString(nDuration);
          }
        }
      }*/
      }

      int iTotalItems = facadeView.Count;
      if (facadeView.Count > 0)
      {
        GUIListItem rootItem = facadeView[0];
        if (rootItem.Label == "..")
        {
          iTotalItems--;
        }
      }

      //set object count label
      if (totalPlayingTime.TotalSeconds > 0)
      {
        GUIPropertyManager.SetProperty("#itemcount",
                                       Util.Utils.GetSongCountLabel(iTotalItems, (int) totalPlayingTime.TotalSeconds));
      }
      else
      {
        GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(iTotalItems));
      }
    }

    protected void SwitchView()
    {
      switch (CurrentView)
      {
        case View.List:
          facadeView.View = GUIFacadeControl.ViewMode.List;
          break;
        case View.Icons:
          facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
          break;
        case View.LargeIcons:
          facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
          break;
        case View.Albums:
          facadeView.View = GUIFacadeControl.ViewMode.AlbumView;
          break;
        case View.FilmStrip:
          facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
          break;
        case View.PlayList:
          facadeView.View = GUIFacadeControl.ViewMode.Playlist;
          break;
      }

      UpdateButtonStates(); // Ensure "View: xxxx" button label is updated to suit
    }

    protected bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.Reset();
      keyboard.Text = playlistPlayer.CurrentPlaylistName;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
        playlistPlayer.CurrentPlaylistName = keyboard.Text;
        return true;
      }
      return false;
    }

    protected virtual void OnRetrieveCoverArt(GUIListItem item)
    {
      Util.Utils.SetDefaultIcons(item);
      if (item.Label == "..")
      {
        return;
      }
      MusicTag tag = (MusicTag) item.MusicTag;
      string strThumb = GUIMusicFiles.GetCoverArt(item.IsFolder, item.Path, tag);
      if (strThumb != string.Empty)
      {
        item.ThumbnailImage = strThumb;
        item.IconImageBig = strThumb;
        item.IconImage = strThumb;

        // let us test if there is a larger cover art image
        string strLarge = Util.Utils.ConvertToLargeCoverArt(strThumb);
        if (File.Exists(strLarge))
        {
          item.ThumbnailImage = strLarge;
        }
      }
    }

    protected void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(495); // Sort options

      dlg.AddLocalizedString(103); // name
      dlg.AddLocalizedString(269); // artist
      dlg.AddLocalizedString(270); // album
      dlg.AddLocalizedString(266); // track
      dlg.AddLocalizedString(268); // title
      dlg.AddLocalizedString(363); // filename
      dlg.AddLocalizedString(367); // rating
      dlg.AddLocalizedString(267); // duration
      dlg.AddLocalizedString(105); // size
      dlg.AddLocalizedString(104); // date

      // !!! this does not work yet, because we need to change
      //       the order of MusicSort.SortMethod items OR
      //       the order which the methods are added to the dialog above
      // set the focus to currently used sort method
      //dlg.SelectedLabel = (int)CurrentSortMethod;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 103:
          CurrentSortMethod = MusicSort.SortMethod.Name;
          break;
        case 269:
          CurrentSortMethod = MusicSort.SortMethod.Artist;
          break;
        case 270:
          CurrentSortMethod = MusicSort.SortMethod.Album;
          break;
        case 266:
          CurrentSortMethod = MusicSort.SortMethod.Track;
          break;
        case 268:
          CurrentSortMethod = MusicSort.SortMethod.Title;
          break;
        case 363:
          CurrentSortMethod = MusicSort.SortMethod.Filename;
          break;
        case 367:
          CurrentSortMethod = MusicSort.SortMethod.Rating;
          break;
        case 267:
          CurrentSortMethod = MusicSort.SortMethod.Duration;
          break;
        case 105:
          CurrentSortMethod = MusicSort.SortMethod.Size;
          break;
        case 104:
          CurrentSortMethod = MusicSort.SortMethod.Date;
          break;
        default:
          CurrentSortMethod = MusicSort.SortMethod.Name;
          break;
      }

      OnSort();
      if (btnSortBy != null)
      {
        GUIControl.FocusControl(GetID, btnSortBy.GetID);
      }
    }

    protected void OnShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(499); // Views menu

      dlg.AddLocalizedString(134); // Shares
      foreach (ViewDefinition view in handler.Views)
      {
        dlg.Add(view.LocalizedName);
      }

      //dlg.AddLocalizedString(4540); // Now playing

      // set the focus to currently used view
      if (this.GetID == (int) Window.WINDOW_MUSIC_FILES)
      {
        dlg.SelectedLabel = 0;
      }
      else if (this.GetID == (int) Window.WINDOW_MUSIC_GENRE)
      {
        dlg.SelectedLabel = handler.CurrentViewIndex + 1;
      }

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 134: // Shares
          {
            //ViewDefinition selectedView = (ViewDefinition)handler.Views[dlg.SelectedLabel - 1];
            //handler.CurrentView = selectedView.Name;
            //MusicState.View = selectedView.Name;
            int nNewWindow = (int) Window.WINDOW_MUSIC_FILES;
            MusicState.StartWindow = nNewWindow;
            if (nNewWindow != GetID)
            {
              GUIWindowManager.ReplaceWindow(nNewWindow);
            }
          }
          break;

        case 4540: // Now playing
          {
            int nPlayingNowWindow = (int) Window.WINDOW_MUSIC_PLAYING_NOW;

            GUIMusicPlayingNow guiPlayingNow = (GUIMusicPlayingNow) GUIWindowManager.GetWindow(nPlayingNowWindow);

            if (guiPlayingNow != null)
            {
              guiPlayingNow.MusicWindow = this;
              GUIWindowManager.ActivateWindow(nPlayingNowWindow);
            }
          }
          break;

        default: // a db view
          {
            ViewDefinition selectedView = (ViewDefinition) handler.Views[dlg.SelectedLabel - 1];
            handler.CurrentView = selectedView.Name;
            MusicState.View = selectedView.Name;
            int nNewWindow = (int) Window.WINDOW_MUSIC_GENRE;
            if (GetID != nNewWindow)
            {
              MusicState.StartWindow = nNewWindow;
              if (nNewWindow != GetID)
              {
                GUIWindowManager.ReplaceWindow(nNewWindow);
              }
            }
            else
            {
              LoadDirectory(string.Empty);
              if (facadeView.Count <= 0)
              {
                GUIControl.FocusControl(GetID, btnViewAs.GetID);
              }
            }
          }
          break;
      }
    }

    protected void OnShowSavedPlaylists(string _directory)
    {
      VirtualDirectory _virtualDirectory = new VirtualDirectory();
      _virtualDirectory.AddExtension(".m3u");
      _virtualDirectory.AddExtension(".pls");
      _virtualDirectory.AddExtension(".b4s");
      _virtualDirectory.AddExtension(".wpl");

      List<GUIListItem> itemlist = _virtualDirectory.GetDirectoryExt(_directory);
      if (_directory == m_strPlayListPath)
      {
        itemlist.RemoveAt(0);
      }

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(983); // Saved Playlists

      foreach (GUIListItem item in itemlist)
      {
        Util.Utils.SetDefaultIcons(item);
        dlg.Add(item);
      }

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      GUIListItem selectItem = itemlist[dlg.SelectedLabel];
      if (selectItem.IsFolder)
      {
        OnShowSavedPlaylists(selectItem.Path);
        return;
      }

      GUIWaitCursor.Show();
      LoadPlayList(selectItem.Path);
      GUIWaitCursor.Hide();
    }

    protected virtual void LoadDirectory(string path)
    {
    }

    public static string GetArtistCoverArtName(string artist)
    {
      return Util.Utils.GetCoverArtName(Thumbs.MusicArtists, artist);
    }

    private void OnInfoFile(GUIListItem item)
    {
    }

    private void OnInfoFolder(GUIListItem item)
    {
    }

    protected virtual void OnFindCoverArt(int iItem)
    {
    }

    protected virtual void OnInfo(int iItem)
    {
      GUIListItem pItem = facadeView[iItem];
      if (pItem == null)
      {
        return;
      }
      Song song = pItem.AlbumInfoTag as Song;
      if (song == null)
      {
        if (!pItem.IsFolder)
        {
          if (pItem.Path != string.Empty)
          {
            OnInfoFile(pItem);
          }
        }
        else
        {
          if (pItem.Path != string.Empty)
          {
            OnInfoFolder(pItem);
          }
        }
        facadeView.RefreshCoverArt();
        return;
      }
      else if (song.Album != "")
      {
        ShowAlbumInfo(false, song.Artist, song.Album, song.FileName, pItem.MusicTag as MusicTag);
      }
      else if (song.Artist != "")
      {
        ShowArtistInfo(song.Artist, song.Album);
      }
      else if (song.AlbumArtist != "")
      {
        ShowArtistInfo(song.AlbumArtist, song.Album);
      }
      facadeView.RefreshCoverArt();
    }

    protected virtual void ShowArtistInfo(string artistName, string albumName)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);

      // check cache
      bool bSaveDb = true;
      ArtistInfo artistinfo = new ArtistInfo();
      if (m_database.GetArtistInfo(artistName, ref artistinfo))
      {
        List<Song> songs = new List<Song>();
        MusicArtistInfo artist = new MusicArtistInfo();
        artist.Set(artistinfo);

        // ok, show Artist info
        GUIMusicArtistInfo pDlgArtistInfo =
          (GUIMusicArtistInfo) GUIWindowManager.GetWindow((int) Window.WINDOW_ARTIST_INFO);
        if (null != pDlgArtistInfo)
        {
          pDlgArtistInfo.Artist = artist;
          pDlgArtistInfo.DoModal(GetID);

          if (pDlgArtistInfo.NeedsRefresh)
          {
            m_database.DeleteArtistInfo(artist.Artist);
            ShowArtistInfo(artistName, albumName);
            return;
          }
        }
        return;
      }


      if (null != pDlgOK && !Win32API.IsConnectedToInternet())
      {
        pDlgOK.SetHeading(703);
        pDlgOK.SetLine(1, 703);
        pDlgOK.SetLine(2, string.Empty);
        pDlgOK.DoModal(GetID);
        return;
      }
      else if (!Win32API.IsConnectedToInternet())
      {
        return;
      }

      // show dialog box indicating we're searching the artist
      if (dlgProgress != null)
      {
        dlgProgress.Reset();
        dlgProgress.SetHeading(320);
        dlgProgress.SetLine(1, artistName);
        dlgProgress.SetLine(2, string.Empty);
        dlgProgress.SetPercentage(0);
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
        dlgProgress.ShowProgressBar(true);
      }
      bool bDisplayErr = false;

      // find artist info
      AllmusicSiteScraper scraper = new AllmusicSiteScraper();
      if (scraper.FindInfo(AllmusicSiteScraper.SearchBy.Artists, artistName))
      {
        if (dlgProgress != null)
        {
          dlgProgress.Close();
        }
        // did we found at least 1 album?
        if (scraper.IsMultiple())
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum = 0;
          string[] artistsFound = scraper.GetItemsFound();
          //show dialog with all albums found
          string szText = GUILocalizeStrings.Get(181);
          GUIDialogSelect pDlg = (GUIDialogSelect) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_SELECT);
          if (null != pDlg)
          {
            pDlg.Reset();
            pDlg.SetHeading(szText);
            for (int i = 0; i < artistsFound.Length; ++i)
            {
              pDlg.Add(artistsFound[i]);
            }
            pDlg.DoModal(GetID);

            // and wait till user selects one
            iSelectedAlbum = pDlg.SelectedLabel;
            if (iSelectedAlbum < 0)
            {
              return;
            }
          }

          // ok, now show dialog we're downloading the artist info
          if (null != dlgProgress)
          {
            dlgProgress.Reset();
            dlgProgress.SetHeading(320);
            dlgProgress.SetLine(1, artistName);
            dlgProgress.SetLine(2, string.Empty);
            dlgProgress.SetPercentage(40);
            dlgProgress.StartModal(GetID);
            dlgProgress.ShowProgressBar(true);
            dlgProgress.Progress();
          }

          // download the artist info
          if (scraper.FindInfoByIndex(iSelectedAlbum))
          {
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(60);
              dlgProgress.Progress();
            }
            MusicArtistInfo artistInfo = new MusicArtistInfo();
            if (artistInfo.Parse(scraper.GetHtmlContent()))
            {
              if (null != dlgProgress)
              {
                dlgProgress.SetPercentage(80);
                dlgProgress.Progress();
              }
              // if the artist selected from allmusic.com does not match
              // the one from the file, override the one from the allmusic
              // with the one from the file so the info is correct in the
              // database...
              if (!artistInfo.Artist.Equals(artistName))
              {
                artistInfo.Artist = artistName;
              }

              if (bSaveDb)
              {
                m_database.AddArtistInfo(artistInfo.Get());
              }
              if (null != dlgProgress)
              {
                dlgProgress.SetPercentage(100);
                dlgProgress.Progress();
                dlgProgress.Close();
                dlgProgress = null;
              }

              // ok, show Artist info
              GUIMusicArtistInfo pDlgArtistInfo =
                (GUIMusicArtistInfo) GUIWindowManager.GetWindow((int) Window.WINDOW_ARTIST_INFO);
              if (null != pDlgArtistInfo)
              {
                pDlgArtistInfo.Artist = artistInfo;
                pDlgArtistInfo.DoModal(GetID);

                if (pDlgArtistInfo.NeedsRefresh)
                {
                  m_database.DeleteArtistInfo(artistInfo.Artist);
                  ShowArtistInfo(artistName, albumName);
                  return;
                }
              }
            }
          }

          if (null != dlgProgress)
          {
            dlgProgress.Close();
          }
        }
        else // single
        {
          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(40);
            dlgProgress.Progress();
          }
          MusicArtistInfo artistInfo = new MusicArtistInfo();
          if (artistInfo.Parse(scraper.GetHtmlContent()))
          {
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(60);
              dlgProgress.Progress();
            }
            // if the artist selected from allmusic.com does not match
            // the one from the file, override the one from the allmusic
            // with the one from the file so the info is correct in the
            // database...
            if (!artistInfo.Artist.Equals(artistName))
            {
              artistInfo.Artist = artistName;
            }

            if (bSaveDb)
            {
              // save to database
              m_database.AddArtistInfo(artistInfo.Get());
            }

            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(100);
              dlgProgress.Progress();
              dlgProgress.Close();
              dlgProgress = null;
            }
            // ok, show Artist info
            GUIMusicArtistInfo pDlgArtistInfo =
              (GUIMusicArtistInfo) GUIWindowManager.GetWindow((int) Window.WINDOW_ARTIST_INFO);
            if (null != pDlgArtistInfo)
            {
              pDlgArtistInfo.Artist = artistInfo;
              pDlgArtistInfo.DoModal(GetID);

              if (pDlgArtistInfo.NeedsRefresh)
              {
                m_database.DeleteArtistInfo(artistInfo.Artist);
                ShowArtistInfo(artistName, albumName);
                return;
              }
            }
          }
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr = true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null != dlgProgress)
        {
          dlgProgress.Close();
        }
        if (null != pDlgOK)
        {
          pDlgOK.SetHeading(702);
          pDlgOK.SetLine(1, 702);
          pDlgOK.SetLine(2, string.Empty);
          pDlgOK.DoModal(GetID);
        }
      }
    }

    public void FindCoverArt(bool isFolder, string artistName, string albumName, string strPath, MusicTag tag,
                             int albumId)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);

      if (null != pDlgOK && !Win32API.IsConnectedToInternet())
      {
        pDlgOK.SetHeading(703);
        pDlgOK.SetLine(1, 703);
        pDlgOK.SetLine(2, string.Empty);
        pDlgOK.DoModal(GetID);

        //throw new Exception("no internet");
        return;
      }

      else if (!Win32API.IsConnectedToInternet())
      {
        //throw new Exception("no internet");
        return;
      }

      bool bDisplayErr = false;
      GUIDialogOK dlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      AlbumInfo albuminfo = new AlbumInfo();
      MusicAlbumInfo album = new MusicAlbumInfo();

      GUICoverArtGrabberResults guiCoverGrabberResults =
        (GUICoverArtGrabberResults) GUIWindowManager.GetWindow((int) Window.WINDOW_MUSIC_COVERART_GRABBER_RESULTS);

      if (null != guiCoverGrabberResults)
      {
        guiCoverGrabberResults.SearchMode = GUICoverArtGrabberResults.SearchDepthMode.Album;
        GUIDialogProgress dlgProgress =
          (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);

        if (dlgProgress != null)
        {
          dlgProgress.Reset();
          dlgProgress.SetHeading(185);
          dlgProgress.SetLine(1, albumName);
          dlgProgress.SetLine(2, artistName);
          dlgProgress.SetLine(3, string.Empty);
          dlgProgress.StartModal(GetID);
        }

        guiCoverGrabberResults.GetAlbumCovers(artistName, albumName, strPath, GetID, true);
        guiCoverGrabberResults.DoModal(GetID);
        albuminfo = guiCoverGrabberResults.SelectedAlbum;

        if (GUICoverArtGrabberResults.CancelledByUser)
        {
          string line1Text = GUILocalizeStrings.Get(4507);

          if (line1Text.Length == 0)
          {
            line1Text = "Cover art grabber aborted by user";
          }

          string caption = GUILocalizeStrings.Get(4511);

          if (caption.Length == 0)
          {
            caption = "Cover Art Grabber Done";
          }

          if (null != dlgOk)
          {
            dlgOk.SetHeading(caption);
            dlgOk.SetLine(1, line1Text);
            dlgOk.SetLine(2, string.Empty);
            dlgOk.DoModal(GetID);
          }
        }

        else if (albuminfo != null)
        {
          // the GUICoverArtGrabberResults::SelectedAlbum AlbumInfo object contains 
          // the Artist and Album name returned by the Amazon Webservice which may not
          // match our original artist and album.  We want to use the original artist
          // and album name...

          albuminfo.Artist = artistName;
          albuminfo.Album = albumName;
          SaveCoverArtImage(albuminfo, strPath, true, true);
          facadeView.RefreshCoverArt();
        }

        else
        {
          bDisplayErr = true;
        }
      }

      if (bDisplayErr)
      {
        if (null != dlgOk)
        {
          dlgOk.SetHeading(187);
          dlgOk.SetLine(1, 187);
          dlgOk.SetLine(2, string.Empty);
          dlgOk.DoModal(GetID);
        }
      }
    }

    protected void ShowAlbumInfo(bool isFolder, string artistName, string albumName, string strPath, MusicTag tag)
    {
      ShowAlbumInfo(GetID, isFolder, artistName, albumName, strPath, tag);
    }

    public void ShowAlbumInfo(int parentWindowID, bool isFolder, string artistName, string albumName, string strPath,
                              MusicTag tag)
    {
      // check cache
      GUIDialogOK dlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress =
        (GUIDialogProgress) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_PROGRESS);
      AlbumInfo albuminfo = new AlbumInfo();
      if (m_database.GetAlbumInfo(albumName, artistName, ref albuminfo))
      {
        List<Song> songs = new List<Song>();
        MusicAlbumInfo album = new MusicAlbumInfo();
        album.Set(albuminfo);

        GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo) GUIWindowManager.GetWindow((int) Window.WINDOW_MUSIC_INFO);
        if (null != pDlgAlbumInfo)
        {
          pDlgAlbumInfo.Album = album;
          pDlgAlbumInfo.Tag = tag;

          //pDlgAlbumInfo.DoModal(GetID);
          pDlgAlbumInfo.DoModal(parentWindowID);
          if (pDlgAlbumInfo.NeedsRefresh)
          {
            m_database.DeleteAlbumInfo(albumName, artistName);
            ShowAlbumInfo(isFolder, artistName, albumName, strPath, tag);
          }
          return;
        }
      }

      // show dialog box indicating we're searching the album
      if (dlgProgress != null)
      {
        dlgProgress.Reset();
        dlgProgress.SetHeading(185);
        dlgProgress.SetLine(1, albumName);
        dlgProgress.SetLine(2, artistName);
        dlgProgress.SetLine(3, tag.Year.ToString());
        dlgProgress.SetPercentage(0);
        //dlgProgress.StartModal(GetID);
        dlgProgress.StartModal(parentWindowID);
        dlgProgress.ShowProgressBar(true);
        dlgProgress.Progress();
      }
      bool bDisplayErr = false;

      // find album info
      MusicInfoScraper scraper = new MusicInfoScraper();
      if (scraper.FindAlbuminfo(albumName, artistName, tag.Year))
      {
        if (dlgProgress != null)
        {
          dlgProgress.SetPercentage(30);
          dlgProgress.Progress();
          dlgProgress.Close();
        }
        // did we found at least 1 album?
        int iAlbumCount = scraper.Count;
        if (iAlbumCount >= 1)
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum = 0;
          if (iAlbumCount > 1)
          {
            //show dialog with all albums found
            string szText = GUILocalizeStrings.Get(181);
            GUIDialogSelect pDlg = (GUIDialogSelect) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_SELECT);
            if (null != pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(szText);
              for (int i = 0; i < iAlbumCount; ++i)
              {
                MusicAlbumInfo info = scraper[i];
                pDlg.Add(info.Title2);
              }
              //pDlg.DoModal(GetID);
              pDlg.DoModal(parentWindowID);

              // and wait till user selects one
              iSelectedAlbum = pDlg.SelectedLabel;
              if (iSelectedAlbum < 0)
              {
                return;
              }
            }
          }

          // ok, now show dialog we're downloading the album info
          MusicAlbumInfo album = scraper[iSelectedAlbum];
          if (null != dlgProgress)
          {
            dlgProgress.Reset();
            dlgProgress.SetHeading(185);
            dlgProgress.SetLine(1, album.Title2);
            dlgProgress.SetLine(2, album.Artist);
            //dlgProgress.StartModal(GetID);
            dlgProgress.StartModal(parentWindowID);
            dlgProgress.ShowProgressBar(true);
            dlgProgress.SetPercentage(40);
            dlgProgress.Progress();
          }

          // download the album info
          bool bLoaded = album.Loaded;
          if (!bLoaded)
          {
            bLoaded = album.Load();
          }
          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(70);
            dlgProgress.Progress();
          }
          if (bLoaded)
          {
            // set album title from musicinfotag, not the one we got from allmusic.com
            album.Title = albumName;
            // set path, needed to store album in database
            album.AlbumPath = strPath;
            albuminfo = new AlbumInfo();
            albuminfo.Album = album.Title;
            albuminfo.Artist = album.Artist;
            albuminfo.Genre = album.Genre;
            albuminfo.Tones = album.Tones;
            albuminfo.Styles = album.Styles;
            albuminfo.Review = album.Review;
            albuminfo.Image = album.ImageURL;
            albuminfo.Rating = album.Rating;
            albuminfo.Tracks = album.Tracks;
            try
            {
              albuminfo.Year = Int32.Parse(album.DateOfRelease);
            }
            catch (Exception)
            {
            }
            //albuminfo.Path   = album.AlbumPath;
            // save to database
            m_database.AddAlbumInfo(albuminfo);
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(100);
              dlgProgress.Progress();
              dlgProgress.Close();
            }

            // ok, show album info
            GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo) GUIWindowManager.GetWindow((int) Window.WINDOW_MUSIC_INFO);
            if (null != pDlgAlbumInfo)
            {
              pDlgAlbumInfo.Album = album;
              pDlgAlbumInfo.Tag = tag;

              //pDlgAlbumInfo.DoModal(GetID);
              pDlgAlbumInfo.DoModal(parentWindowID);
              if (pDlgAlbumInfo.NeedsRefresh)
              {
                m_database.DeleteAlbumInfo(album.Title, album.Artist);
                ShowAlbumInfo(isFolder, artistName, albumName, strPath, tag);
                return;
              }
              if (isFolder) // || _dirsAreAlbums)
              {
                // if there's an album thumb
                string thumb = Util.Utils.GetAlbumThumbName(album.Artist, album.Title);
                // use the better one
                thumb = Util.Utils.ConvertToLargeCoverArt(thumb);
                // to create a folder.jpg from it
                if (File.Exists(thumb) && _createMissingFolderThumbs)
                {
                  try
                  {
                    string folderjpg = Util.Utils.GetFolderThumbForDir(strPath);
                      // String.Format(@"{0}\folder.jpg", MediaPortal.Util.Utils.RemoveTrailingSlash(strPath));
                    Util.Utils.FileDelete(folderjpg);
                    File.Copy(thumb, folderjpg);
                    // cache the new folder.jpg so the user does not have to rescan the collection
                    FolderThumbCacher thumbworker = new FolderThumbCacher(strPath, true);
                  }
                  catch (Exception)
                  {
                  }
                }
              }
            }
          }
          else
          {
            // failed 2 download album info
            bDisplayErr = true;
          }
        }
        else
        {
          // no albums found
          bDisplayErr = true;
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr = true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null != dlgProgress)
        {
          dlgProgress.Close();
        }
        if (null != dlgOk)
        {
          dlgOk.SetHeading(187);
          dlgOk.SetLine(1, 187);
          dlgOk.SetLine(2, string.Empty);
          //dlgOk.DoModal(GetID);
          dlgOk.DoModal(parentWindowID);
        }
      }
    }

    protected bool SaveCoverArtImage(AlbumInfo albumInfo, string aAlbumSharePath, bool aSaveToAlbumShare,
                                     bool aSaveToAlbumThumbFolder)
    {
      bool result = false;
      bool isCdOrDVD = Util.Utils.IsDVD(aAlbumSharePath);
      string sharePath = Util.Utils.RemoveTrailingSlash(aAlbumSharePath);

      try
      {
        Image coverImg = AmazonWebservice.GetImageFromURL(albumInfo.Image);
        string thumbPath = Util.Utils.GetAlbumThumbName(albumInfo.Artist, albumInfo.Album);

        if (thumbPath.Length == 0 || coverImg == null)
        {
          return false;
        }

        //if (bSaveToThumbsFolder)
        if (aSaveToAlbumShare && !isCdOrDVD)
        {
          string folderjpg = String.Format(@"{0}\folder.jpg", sharePath);

          if (File.Exists(folderjpg))
          {
            File.Delete(folderjpg);
          }

          coverImg.Save(folderjpg);
          File.SetAttributes(folderjpg, File.GetAttributes(folderjpg) | FileAttributes.Hidden);
          // no need to check for that option as it is the user's decision.   if (_createMissingFolderThumbCache)         
          FolderThumbCacher thumbworker = new FolderThumbCacher(sharePath, true);
          result = true;
        }

        if (aSaveToAlbumThumbFolder || isCdOrDVD)
        {
          if (File.Exists(thumbPath))
          {
            File.Delete(thumbPath);
          }

          if (Util.Picture.CreateThumbnail(coverImg, thumbPath, (int) Thumbs.ThumbResolution,
                                           (int) Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
          {
            result = true;
            Util.Picture.CreateThumbnail(coverImg, thumbPath, (int) Thumbs.ThumbLargeResolution,
                                         (int) Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
          }
        }
      }

      catch
      {
        result = false;
      }

      return result;
    }

    public static bool CoverArtExists(string ArtistName, string AlbumName, string albumPath, bool checkAlbumFolder)
    {
      if (ArtistName.Length == 0 || AlbumName.Length == 0)
      {
        return false;
      }

      bool bHasThumbnailImage = File.Exists(Util.Utils.GetAlbumThumbName(ArtistName, AlbumName));

      if (!checkAlbumFolder)
      {
        return bHasThumbnailImage;
      }

      string path = Path.GetDirectoryName(albumPath);
      bool bHasAlbumFolderImage = File.Exists(Path.Combine(path, "folder.jpg"));

      return bHasThumbnailImage && bHasAlbumFolderImage;
    }

    protected virtual void AddSongToFavorites(GUIListItem item)
    {
      Song song = item.AlbumInfoTag as Song;
      if (song == null)
      {
        return;
      }
      if (song.Id < 0)
      {
        return;
      }
      song.Favorite = true;
      m_database.SetFavorite(song);
    }

    private void SortChanged(object sender, SortEventArgs args)
    {
      this.CurrentSortAsc = args.Order != SortOrder.Descending;

      OnSort();
      UpdateButtonStates();
    }

    protected virtual bool IsSortableView(ViewDefinition view, int viewLevel)
    {
      if (view == null || viewLevel < 0 || viewLevel >= view.Filters.Count)
      {
        return false;
      }

      string sWhere = ((FilterDefinition) view.Filters[viewLevel]).Where;

      if (sWhere.Length == 0)
      {
        return true;
      }

      switch (sWhere.ToLower())
      {
        case "timesplayed":
          return false;

        default:
          return true;
      }
    }

    //SV Added by SteveV 2006-09-07
    protected bool DoPlayNowJumpTo(int playlistItemCount)
    {
      switch (PlayNowJumpTo)
      {
        case PlayNowJumpToType.NowPlayingAlways:
          {
            if (playlistItemCount < 1)
            {
              return false;
            }

            GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYING_NOW);
            return true;
          }

        case PlayNowJumpToType.NowPlayingMultipleItems:
          {
            if (playlistItemCount <= 1)
            {
              return false;
            }

            GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYING_NOW);
            return true;
          }

        case PlayNowJumpToType.CurrentPlaylistAlways:
          {
            if (playlistItemCount < 1)
            {
              return false;
            }

            GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYLIST);
            return true;
          }

        case PlayNowJumpToType.CurrentPlaylistMultipleItems:
          {
            if (playlistItemCount <= 1)
            {
              return false;
            }

            GUIWindowManager.ActivateWindow((int) Window.WINDOW_MUSIC_PLAYLIST);
            return true;
          }

        case PlayNowJumpToType.FullscreenIfVizEnabledAlways:
          {
            if (playlistItemCount < 1)
            {
              return false;
            }

            if (!UsingInternalMusicPlayer || !g_Player.IsMusic || !g_Player.Playing)
            {
              return false;
            }

            GUIWindowManager.ActivateWindow((int) Window.WINDOW_FULLSCREEN_MUSIC);
            GUIGraphicsContext.IsFullScreenVideo = true;
            return true;
          }

        case PlayNowJumpToType.FullscreenIfVizEnabledMultipleItems:
          {
            if (playlistItemCount <= 1)
            {
              return false;
            }

            if (!UsingInternalMusicPlayer || !g_Player.IsMusic || !g_Player.Playing)
            {
              return false;
            }

            GUIWindowManager.ActivateWindow((int) Window.WINDOW_FULLSCREEN_MUSIC);
            GUIGraphicsContext.IsFullScreenVideo = true;
            return true;
          }

        default:
          return false;
      }
    }
  }
}