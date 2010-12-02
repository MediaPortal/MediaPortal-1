#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;
using WindowPlugins;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for GUIMusicBaseWindow.
  /// </summary>
  public class GUIMusicBaseWindow : WindowPluginBase
  {
    #region enums

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

    protected MusicSort.SortMethod currentSortMethod = MusicSort.SortMethod.Name;
    protected string m_strPlayListPath = string.Empty;
    private bool _autoShuffleOnLoad = false;

    protected MusicDatabase m_database;
    
    protected List<Share> _shareList = new List<Share>();

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
      "%duration%", "%year%", "%filesize%", "%duration%", "%duration%",
      "%duration%"
        , "%duration%", "%duration%", "%filesize%", "%rating%"
    };

    private string[] _sortTags1 = new string[20];
    private string[] _sortTags2 = new string[20];
    protected PlayListPlayer playlistPlayer;

    protected PlayNowJumpToType PlayNowJumpTo = PlayNowJumpToType.None;
    protected bool UsingInternalMusicPlayer = false;

    protected bool PlayAllOnSingleItemPlayNow = true;
    protected string _currentPlaying = string.Empty;

    #endregion

    #region SkinControls

    [SkinControl(8)] protected GUIButtonControl btnSearch = null;
    [SkinControl(12)] protected GUIButtonControl btnPlayCd = null;
    [SkinControl(10)] protected GUIButtonControl btnSavedPlaylists = null;

    #endregion

    #region Constructor / Destructor

    public GUIMusicBaseWindow()
    {
      if (m_database == null)
      {
        m_database = MusicDatabase.Instance;
      }

      playlistPlayer = PlayListPlayer.SingletonPlayer;

      using (Profile.Settings xmlreader = new Profile.MPSettings())
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

    protected override void LoadSettings()
    {
      base.LoadSettings();
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        MusicState.StartWindow = xmlreader.GetValueAsInt("music", "startWindow", GetID);
        MusicState.View = xmlreader.GetValueAsString("music", "startview", string.Empty);
        _createMissingFolderThumbCache = xmlreader.GetValueAsBool("thumbnails", "musicfolderondemand", true);
        _createMissingFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs", false);
        _useFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", true);
        _showSortButton = xmlreader.GetValueAsBool("musicfiles", "showSortButton", true);

        String strPlayMode = xmlreader.GetValueAsString("musicfiles", "playmode", "play");
        if (strPlayMode == "playlist")
        {
          MusicState.CurrentPlayMode = MusicState.PlayMode.PLAYLIST_MODE;
        }
        else
        {
          MusicState.CurrentPlayMode = MusicState.PlayMode.PLAY_MODE;
        }
        
        PlayAllOnSingleItemPlayNow = xmlreader.GetValueAsBool("musicfiles", "addall", true);

        int defaultSort = (int)MusicSort.SortMethod.Name;
        
        if ((handler != null) && (handler.View != null) && (handler.View.Filters != null) &&
            (handler.View.Filters.Count > 0))
        {
          FilterDefinition def = (FilterDefinition)handler.View.Filters[0];
          defaultSort = (int)GetSortMethod(def.DefaultSort);
        }
        
        currentSortMethod = (MusicSort.SortMethod)xmlreader.GetValueAsInt(SerializeName, "sortmethod", defaultSort);

        for (int i = 0; i < _sortModes.Length; ++i)
        {
          _sortTags1[i] = xmlreader.GetValueAsString("mymusic", _sortModes[i] + "1", _defaultSortTags1[i]);
          _sortTags2[i] = xmlreader.GetValueAsString("mymusic", _sortModes[i] + "2", _defaultSortTags2[i]);
        }

        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";

        m_strPlayListPath = xmlreader.GetValueAsString("music", "playlists", playListFolder);
        m_strPlayListPath = Util.Utils.RemoveTrailingSlash(m_strPlayListPath);

        _shareList.Clear();
        
        string strDefault = xmlreader.GetValueAsString("music", "default", string.Empty);
        for (int i = 0; i < VirtualDirectory.MaxSharesCount; i++)
        {
          string strShareName = String.Format("sharename{0}", i);
          string strSharePath = String.Format("sharepath{0}", i);
          string strPincode = String.Format("pincode{0}", i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);
          string shareViewPath = String.Format("shareview{0}", i);

          Share share = new Share();
          share.Name = xmlreader.GetValueAsString("music", strShareName, string.Empty);
          share.Path = xmlreader.GetValueAsString("music", strSharePath, string.Empty);
          share.Pincode = xmlreader.GetValueAsInt("music", strPincode, -1);

          share.IsFtpShare = xmlreader.GetValueAsBool("music", shareType, false);
          share.FtpServer = xmlreader.GetValueAsString("music", shareServer, string.Empty);
          share.FtpLoginName = xmlreader.GetValueAsString("music", shareLogin, string.Empty);
          share.FtpPassword = xmlreader.GetValueAsString("music", sharePwd, string.Empty);
          share.FtpPort = xmlreader.GetValueAsInt("music", sharePort, 21);
          share.FtpFolder = xmlreader.GetValueAsString("music", remoteFolder, "/");
          share.DefaultLayout = (Layout)xmlreader.GetValueAsInt("music", shareViewPath, (int)Layout.List);

          if (share.Name.Length > 0)
          {
            if (strDefault == share.Name)
            {
              share.Default = true;
            }
            _shareList.Add(share);
          }
          else
          {
            break;
          }
        }
      }
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
        case "disc#":
          return MusicSort.SortMethod.DiscID;
      }
      if (!string.IsNullOrEmpty(s))
      {
        Log.Error("GUIMusicBaseWindow::GetSortMethod: Unknown String - " + s);
      }
      return MusicSort.SortMethod.Name;
    }

    protected override void SaveSettings()
    {
      base.SaveSettings();
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue(SerializeName, "sortmethod", (int)currentSortMethod);
      }
    }

    #endregion
    
    protected override bool AllowLayout(Layout layout)
    {
      if (layout == Layout.Playlist)
      {
        return false;
      }
      return true;
    }

    protected virtual MusicSort.SortMethod CurrentSortMethod
    {
      get { return currentSortMethod; }
      set { currentSortMethod = value; }
    }

    protected override string SerializeName
    {
      get { return "musicbase"; }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_MUSIC_PLAYLIST);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_IMPORT_TRACK)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeTrack(facadeLayout, GetID);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_IMPORT_DISC)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeDisc(facadeLayout, GetID);
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
        facadeLayout.OnMessage(message);
      }
      return base.OnMessage(message);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == btnSavedPlaylists)
      {
        OnShowSavedPlaylists(m_strPlayListPath);
      }
    }

    protected override void UpdateButtonStates()
    {
      base.UpdateButtonStates();
      if (GetID == (int)Window.WINDOW_MUSIC_GENRE)
      {
        GUIPropertyManager.SetProperty("#currentmodule",
                                       String.Format("{0}/{1}", GUILocalizeStrings.Get(100005),
                                                     handler.LocalizedCurrentView));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentmodule", GetModuleName());
      }

      string strLine = string.Empty;

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
      }

      if (null != facadeLayout)
        facadeLayout.EnableScrollLabel = CurrentSortMethod == MusicSort.SortMethod.AlbumArtist ||
                                         CurrentSortMethod == MusicSort.SortMethod.Filename ||
                                         CurrentSortMethod == MusicSort.SortMethod.Album ||
                                         CurrentSortMethod == MusicSort.SortMethod.Artist ||
                                         CurrentSortMethod == MusicSort.SortMethod.Title ||
                                         CurrentSortMethod == MusicSort.SortMethod.Track ||
                                         CurrentSortMethod == MusicSort.SortMethod.Year ||
                                         CurrentSortMethod == MusicSort.SortMethod.Name
                                         ;

    }

    protected void OnSetRating(int itemNumber)
    {
      GUIListItem item = facadeLayout[itemNumber];
      if (item == null)
      {
        return;
      }
      MusicTag tag = item.MusicTag as MusicTag;
      GUIDialogSetRating dialog = (GUIDialogSetRating)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_RATING);
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
          item = facadeLayout[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }

      if (dialog.Result == GUIDialogSetRating.ResultCode.Next)
      {
        while (itemNumber + 1 < facadeLayout.Count)
        {
          itemNumber++;
          item = facadeLayout[itemNumber];
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
      // Update current playing (might have changed when other window was running)
      PlayListItem currentItem = playlistPlayer.GetCurrentItem();
      if (currentItem != null)
      {
        _currentPlaying = currentItem.FileName;
      }

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
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("music", "startWindow", MusicState.StartWindow.ToString());
        xmlwriter.SetValue("music", "startview", MusicState.View);
      }
      base.OnPageDestroy(newWindowId);
    }

    protected void LoadPlayList(string strPlayList)
    {
      LoadPlayList(strPlayList, true);
    }

    protected void LoadPlayList(string strPlayList, bool startPlayback)
    {
      IPlayListIO loader = PlayListFactory.CreateIO(strPlayList);
      if (loader == null)
      {
        return;
      }
      PlayList playlist = new PlayList();

      if (!Util.Utils.FileExistsInCache(strPlayList))
      {
        Log.Info("Playlist: Skipping non-existing Playlist file: {0}", strPlayList);
        return;
      }

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
      if (playlist.Count == 1 && startPlayback)
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
        if (Util.Utils.FileExistsInCache(playListItem.FileName) || playListItem.Type == PlayListItem.PlayListItemType.AudioStream)
        {
          playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playListItem);
        }
        else
        {
          Log.Info("Playlist: File {0} no longer exists. Skipping item.", playListItem.FileName);
        }
      }

      // if we got a playlist
      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0 && startPlayback)
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
          GUIWindowManager.ActivateWindow((int)Window.WINDOW_MUSIC_PLAYLIST);
        }
      }
    }

    private void TellUserSomethingWentWrong()
    {
      GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
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
      facadeLayout.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));
      UpdateButtonStates();
      SelectCurrentItem();
    }

    protected virtual void SetLabels()
    {
      MusicSort.SortMethod method = CurrentSortMethod;
      TimeSpan totalPlayingTime = new TimeSpan();

      for (int i = 0; i < facadeLayout.Count; ++i)
      {
        GUIListItem item = facadeLayout[i];
        MusicTag tag = (MusicTag)item.MusicTag;
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
            FilterDefinition filter = (FilterDefinition)handler.View.Filters[handler.CurrentLevel];
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
            line1 = _sortTags1[(int)MusicSort.SortMethod.Artist]; // Use Artist sort string for AlbumArtist
            line2 = _sortTags2[(int)MusicSort.SortMethod.Artist];
          }
          else
          {
            line1 = _sortTags1[(int)method];
            line2 = _sortTags2[(int)method];
          }
          line1 = Util.Utils.ReplaceTag(line1, "%track%", trackNr);
          line2 = Util.Utils.ReplaceTag(line2, "%track%", trackNr);
          line1 = Util.Utils.ReplaceTag(line1, "%filesize%", fileSize);
          line2 = Util.Utils.ReplaceTag(line2, "%filesize%", fileSize);
          if (handler.View != null)
          {
            FilterDefinition tempfilter = (FilterDefinition)handler.View.Filters[handler.CurrentLevel];
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
      }

      int iTotalItems = facadeLayout.Count;
      if (facadeLayout.Count > 0)
      {
        GUIListItem rootItem = facadeLayout[0];
        if (rootItem.Label == "..")
        {
          iTotalItems--;
        }
      }

      //set object count label, total duration
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(iTotalItems));

      if (totalPlayingTime.TotalSeconds > 0)
      {
        GUIPropertyManager.SetProperty("#totalduration", Util.Utils.SecondsToHMSString((int)totalPlayingTime.TotalSeconds));
      }
      else
      {
        GUIPropertyManager.SetProperty("#totalduration", string.Empty);
      }
    }

    protected virtual void OnRetrieveCoverArt(GUIListItem item)
    {
      Util.Utils.SetDefaultIcons(item);
      if (item.Label == "..")
      {
        return;
      }
      MusicTag tag = (MusicTag)item.MusicTag;
      string strThumb = GetCoverArt(item.IsFolder, item.Path, tag);
      if (strThumb != string.Empty)
      {
        item.ThumbnailImage = strThumb;
        item.IconImageBig = strThumb;
        item.IconImage = strThumb;

        // let us test if there is a larger cover art image
        string strLarge = Util.Utils.ConvertToLargeCoverArt(strThumb);
        if (Util.Utils.FileExistsInCache(strLarge))
        {
          item.ThumbnailImage = strLarge;
        }
      }
    }
    
    public static string GetCoverArt(bool isfolder, string filename, MusicTag tag)
    {
      if (isfolder)
      {
        string strFolderThumb = string.Empty;
        strFolderThumb = Util.Utils.GetLocalFolderThumbForDir(filename);

        if (Util.Utils.FileExistsInCache(strFolderThumb))
        {
          return strFolderThumb;
        }
        else
        {
          if (_createMissingFolderThumbCache)
          {
            FolderThumbCacher thumbworker = new FolderThumbCacher(filename, false);
          }
        }
        return string.Empty;
      }

      string strAlbumName = string.Empty;
      string strArtistName = string.Empty;
      if (tag != null)
      {
        if (tag.Album.Length > 0)
        {
          strAlbumName = tag.Album;
        }
        if (tag.Artist.Length > 0)
        {
          strArtistName = tag.Artist;
        }
      }
      if (!isfolder && strAlbumName.Length == 0 && strArtistName.Length == 0 && !Util.Utils.IsAVStream(filename))
      {
        FileInfo fI = new FileInfo(filename);
        string dir = fI.Directory.FullName;

        if (dir.Length > 0)
        {
          string strFolderThumb = string.Empty;
          strFolderThumb = Util.Utils.GetLocalFolderThumbForDir(dir);

          if (Util.Utils.FileExistsInCache(strFolderThumb))
          {
            return strFolderThumb;
          }
          else
          {
            if (_createMissingFolderThumbCache)
            {
              FolderThumbCacher thumbworker = new FolderThumbCacher(dir, false);
            }
          }
        }
        
        return string.Empty;
      }

      // use covert art thumbnail for albums
      string strThumb = Util.Utils.GetAlbumThumbName(strArtistName, strAlbumName);
      if (Util.Utils.FileExistsInCache(strThumb))
      {
        if (_createMissingFolderThumbs && _createMissingFolderThumbCache)
        {
          string folderThumb = Util.Utils.GetFolderThumb(filename);
          if (!Util.Utils.FileExistsInCache(folderThumb))
          {
            FolderThumbCreator thumbCreator = new FolderThumbCreator(filename, tag);
          }
        }
        return strThumb;
      }

      return tag.CoverArtFile;
      //return string.Empty;
    }
    
    protected override void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
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

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
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

    public static string GetArtistCoverArtName(string artist)
    {
      return Util.Utils.GetCoverArtName(Thumbs.MusicArtists, artist);
    }

    protected virtual void ShowArtistInfo(string artistName, string albumName)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);

      bool bSaveDb = true;
      bool bDisplayErr = false;

      ArtistInfo artist = new ArtistInfo();
      MusicArtistInfo artistInfo = new MusicArtistInfo();
      if (m_database.GetArtistInfo(artistName, ref artist))
      {
        artistInfo.Set(artist);
      }
      else
      {
        if (null != pDlgOK && !Win32API.IsConnectedToInternet())
        {
          pDlgOK.SetHeading(703);
          pDlgOK.SetLine(1, 703);
          pDlgOK.SetLine(2, string.Empty);
          pDlgOK.DoModal(GetID);
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
        
        // find artist info
        AllmusicSiteScraper scraper = new AllmusicSiteScraper();
        if (scraper.FindInfo(AllmusicSiteScraper.SearchBy.Artists, artistName))
        {
          // did we find at least 1 artist?
          if (scraper.IsMultiple())
          {
            // let user choose one
            int iSelectedArtist = 0;
            string szText = GUILocalizeStrings.Get(181);
            GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
            if (null != pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(szText);
              foreach (string selectedArtist in scraper.GetItemsFound())
              {
                pDlg.Add(selectedArtist);
              }
              pDlg.DoModal(GetID);

              // and wait till user selects one
              iSelectedArtist = pDlg.SelectedLabel;
              if (iSelectedArtist < 0)
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
            if (!scraper.FindInfoByIndex(iSelectedArtist))
            {
              if (null != dlgProgress)
              {
                dlgProgress.Close();
              }
              return;
            }
          }

          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(60);
            dlgProgress.Progress();
          }

          // Now we have either a Single hit or a selected Artist
          // Parse it
          if (artistInfo.Parse(scraper.GetHtmlContent()))
          {
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
          }
          else
          {
            bDisplayErr = true;
          }
        }
        else
        {
          bDisplayErr = true;
        }
      }

      if (null != dlgProgress)
      {
        dlgProgress.Close();
      }

      if (!bDisplayErr)
      {
        // ok, show Artist info
        GUIMusicArtistInfo pDlgArtistInfo =
          (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_ARTIST_INFO);
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
      else
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
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);

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
      GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      AlbumInfo albuminfo = new AlbumInfo();
      MusicAlbumInfo album = new MusicAlbumInfo();

      GUICoverArtGrabberResults guiCoverGrabberResults =
        (GUICoverArtGrabberResults)GUIWindowManager.GetWindow((int)Window.WINDOW_MUSIC_COVERART_GRABBER_RESULTS);

      if (null != guiCoverGrabberResults)
      {
        guiCoverGrabberResults.SearchMode = GUICoverArtGrabberResults.SearchDepthMode.Album;
        GUIDialogProgress dlgProgress =
          (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);

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
          facadeLayout.RefreshCoverArt();
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
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);

      bool bDisplayErr = false;
      AlbumInfo album = new AlbumInfo();
      MusicAlbumInfo albumInfo = new MusicAlbumInfo();
      if (m_database.GetAlbumInfo(albumName, artistName, ref album))
      {
        albumInfo.Set(album);
      }
      else
      {
        if (null != pDlgOK && !Win32API.IsConnectedToInternet())
        {
          pDlgOK.SetHeading(703);
          pDlgOK.SetLine(1, 703);
          pDlgOK.SetLine(2, string.Empty);
          pDlgOK.DoModal(GetID);
          return;
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

        // find album info
        AllmusicSiteScraper scraper = new AllmusicSiteScraper();
        if (scraper.FindAlbumInfo(albumName, artistName, tag.Year))
        {
          if (dlgProgress != null)
          {
            dlgProgress.SetPercentage(30);
            dlgProgress.Progress();
            dlgProgress.Close();
          }
          // Did we find multiple albums?
          int iSelectedAlbum = 0;
          if (scraper.IsMultiple())
          {
            string szText = GUILocalizeStrings.Get(181);
            GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
            if (null != pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(szText);
              foreach (MusicAlbumInfo foundAlbum in scraper.GetAlbumsFound())
              {
                pDlg.Add(string.Format("{0} - {1}", foundAlbum.Title, foundAlbum.Artist));
              }
              pDlg.DoModal(parentWindowID);

              // and wait till user selects one
              iSelectedAlbum = pDlg.SelectedLabel;
              if (iSelectedAlbum < 0)
              {
                return;
              }
            }
            // ok, now show dialog we're downloading the album info
            MusicAlbumInfo selectedAlbum = scraper.GetAlbumsFound()[iSelectedAlbum];
            if (null != dlgProgress)
            {
              dlgProgress.Reset();
              dlgProgress.SetHeading(185);
              dlgProgress.SetLine(1, selectedAlbum.Title2);
              dlgProgress.SetLine(2, selectedAlbum.Artist);
              dlgProgress.StartModal(parentWindowID);
              dlgProgress.ShowProgressBar(true);
              dlgProgress.SetPercentage(40);
              dlgProgress.Progress();
            }

            if (!scraper.FindInfoByIndex(iSelectedAlbum))
            {
              if (null != dlgProgress)
              {
                dlgProgress.Close();
              }
              return;
            }
          }

          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(60);
            dlgProgress.Progress();
          }

          // Now we have either a Single hit or a selected Artist
          // Parse it
          if (albumInfo.Parse(scraper.GetHtmlContent()))
          {
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(80);
              dlgProgress.Progress();
            }
            // set album title and artist from musicinfotag, not the one we got from allmusic.com
            albumInfo.Title = albumName;
            albumInfo.Artist = artistName;

            // set path, needed to store album in database
            albumInfo.AlbumPath = strPath;
            album = new AlbumInfo();
            album.Album = albumInfo.Title;
            album.Artist = albumInfo.Artist;
            album.Genre = albumInfo.Genre;
            album.Tones = albumInfo.Tones;
            album.Styles = albumInfo.Styles;
            album.Review = albumInfo.Review;
            album.Image = albumInfo.ImageURL;
            album.Rating = albumInfo.Rating;
            album.Tracks = albumInfo.Tracks;
            try
            {
              album.Year = Int32.Parse(albumInfo.DateOfRelease);
            }
            catch (Exception) {}

            // save to database
            m_database.AddAlbumInfo(album);
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(100);
              dlgProgress.Progress();
              dlgProgress.Close();
            }

            if (isFolder)
            {
              // if there's an album thumb
              string thumb = Util.Utils.GetAlbumThumbName(albumInfo.Artist, albumInfo.Title);
              // use the better one
              thumb = Util.Utils.ConvertToLargeCoverArt(thumb);
              // to create a folder.jpg from it
              if (Util.Utils.FileExistsInCache(thumb) && _createMissingFolderThumbs)
              {
                try
                {
                  string folderjpg = Util.Utils.GetFolderThumbForDir(strPath);
                  Util.Utils.FileDelete(folderjpg);
                  File.Copy(thumb, folderjpg);
                }
                catch (Exception) { }
              }
            }
          }
          else
          {
            bDisplayErr = true;
          }
        }
        else
        {
          bDisplayErr = true;
        }
      }

      if (null != dlgProgress)
      {
        dlgProgress.Close();
      }

      if (!bDisplayErr)
      {
        GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_MUSIC_INFO);
        if (null != pDlgAlbumInfo)
        {
          pDlgAlbumInfo.Album = albumInfo;
          pDlgAlbumInfo.Tag = tag;

          pDlgAlbumInfo.DoModal(parentWindowID);
          if (pDlgAlbumInfo.NeedsRefresh)
          {
            m_database.DeleteAlbumInfo(albumName, artistName);
            ShowAlbumInfo(isFolder, artistName, albumName, strPath, tag);
            return;
          }
        }
        else
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

          if (Util.Utils.FileExistsInCache(folderjpg))
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
          if (Util.Utils.FileExistsInCache(thumbPath))
          {
            File.Delete(thumbPath);
          }

          if (Util.Picture.CreateThumbnail(coverImg, thumbPath, (int)Thumbs.ThumbResolution,
                                           (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
          {
            result = true;
            Util.Picture.CreateThumbnail(coverImg, thumbPath, (int)Thumbs.ThumbLargeResolution,
                                         (int)Thumbs.ThumbLargeResolution, 0, Thumbs.SpeedThumbsLarge);
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

      bool bHasThumbnailImage = Util.Utils.FileExistsInCache(Util.Utils.GetAlbumThumbName(ArtistName, AlbumName));

      if (!checkAlbumFolder)
      {
        return bHasThumbnailImage;
      }

      string path = Path.GetDirectoryName(albumPath);
      bool bHasAlbumFolderImage = Util.Utils.FileExistsInCache(Path.Combine(path, "folder.jpg"));

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

      string sWhere = ((FilterDefinition)view.Filters[viewLevel]).Where;

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

            GUIWindowManager.ActivateWindow((int)Window.WINDOW_MUSIC_PLAYING_NOW);
            return true;
          }

        case PlayNowJumpToType.NowPlayingMultipleItems:
          {
            if (playlistItemCount <= 1)
            {
              return false;
            }

            GUIWindowManager.ActivateWindow((int)Window.WINDOW_MUSIC_PLAYING_NOW);
            return true;
          }

        case PlayNowJumpToType.CurrentPlaylistAlways:
          {
            if (playlistItemCount < 1)
            {
              return false;
            }

            GUIWindowManager.ActivateWindow((int)Window.WINDOW_MUSIC_PLAYLIST);
            return true;
          }

        case PlayNowJumpToType.CurrentPlaylistMultipleItems:
          {
            if (playlistItemCount <= 1)
            {
              return false;
            }

            GUIWindowManager.ActivateWindow((int)Window.WINDOW_MUSIC_PLAYLIST);
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

            GUIWindowManager.ActivateWindow((int)Window.WINDOW_FULLSCREEN_MUSIC);
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

            GUIWindowManager.ActivateWindow((int)Window.WINDOW_FULLSCREEN_MUSIC);
            GUIGraphicsContext.IsFullScreenVideo = true;
            return true;
          }

        default:
          return false;
      }
    }

    protected virtual void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      MusicTag tag = (MusicTag)item.MusicTag;
      if (tag != null)
      {
        // none text values default to 0 and datetime.minvalue so
        // set the appropriate properties to string.empty
        
        // Duration
        string strDuration = tag.Duration <= 0 ? string.Empty : MediaPortal.Util.Utils.SecondsToHMSString(tag.Duration);
        // Track
        string strTrack = tag.Track <= 0 ? string.Empty : tag.Track.ToString();
        // Year
        string strYear = tag.Year <= 1900 ? string.Empty : tag.Year.ToString();
        // Rating
        string strRating = (Convert.ToDecimal(2 * tag.Rating + 1)).ToString();
        // Bitrate
        string strBitrate = tag.BitRate <= 0 ? string.Empty : tag.BitRate.ToString();
        // Disc ID
        string strDicsID = tag.DiscID <= 0 ? string.Empty : tag.DiscID.ToString();
        // Disc Total
        string strDiscTotal = tag.DiscTotal <= 0 ? string.Empty : tag.DiscTotal.ToString();
        // Times played
        string strTimesPlayed = tag.TimesPlayed <= 0 ? string.Empty : tag.TimesPlayed.ToString();
        // track total
        string strTrackTotal = tag.TrackTotal <= 0 ? string.Empty : tag.TrackTotal.ToString();
        // BPM
        string strBPM = tag.BPM <= 0 ? string.Empty : tag.BPM.ToString();
        // Channels
        string strChannels = tag.Channels <= 0 ? string.Empty : tag.Channels.ToString();
        // Sample Rate
        string strSampleRate = tag.SampleRate <=0 ? string.Empty : tag.SampleRate.ToString();
        // Date Last Played
        string strDateLastPlayed = tag.DateTimePlayed == DateTime.MinValue ? string.Empty : tag.DateTimePlayed.ToShortDateString();
        // Date Added
        string strDateAdded = tag.DateTimeModified == DateTime.MinValue ? string.Empty : tag.DateTimeModified.ToShortDateString();

        if(item.IsFolder)
        {
          GUIPropertyManager.SetProperty("#title", string.Empty);
          GUIPropertyManager.SetProperty("#track", string.Empty);
          GUIPropertyManager.SetProperty("#rating", string.Empty);
          GUIPropertyManager.SetProperty("#duration", string.Empty);
          GUIPropertyManager.SetProperty("#artist", string.Empty);
          GUIPropertyManager.SetProperty("#bitRate", string.Empty);
          GUIPropertyManager.SetProperty("#comment", string.Empty);
          GUIPropertyManager.SetProperty("#composer", string.Empty);
          GUIPropertyManager.SetProperty("#conductor", string.Empty);
          GUIPropertyManager.SetProperty("#lyrics", string.Empty);
          GUIPropertyManager.SetProperty("#timesplayed", string.Empty);
          GUIPropertyManager.SetProperty("#filetype", string.Empty);
          GUIPropertyManager.SetProperty("#codec", string.Empty);
          GUIPropertyManager.SetProperty("#bitratemode", string.Empty);
          GUIPropertyManager.SetProperty("#bpm", string.Empty);
          GUIPropertyManager.SetProperty("#channels", string.Empty);
          GUIPropertyManager.SetProperty("#samplerate", string.Empty);
        }
        else
        {
          GUIPropertyManager.SetProperty("#title", tag.Title);
          GUIPropertyManager.SetProperty("#track", strTrack);
          GUIPropertyManager.SetProperty("#rating", strRating);
          GUIPropertyManager.SetProperty("#duration", strDuration);
          GUIPropertyManager.SetProperty("#artist", tag.Artist);
          GUIPropertyManager.SetProperty("#bitRate", strBitrate);
          GUIPropertyManager.SetProperty("#comment", tag.Comment);
          GUIPropertyManager.SetProperty("#composer", tag.Composer);
          GUIPropertyManager.SetProperty("#conductor", tag.Conductor);
          GUIPropertyManager.SetProperty("#lyrics", tag.Lyrics);
          GUIPropertyManager.SetProperty("#timesplayed", strTimesPlayed);
          GUIPropertyManager.SetProperty("#filetype", tag.FileType);
          GUIPropertyManager.SetProperty("#codec", tag.Codec);
          GUIPropertyManager.SetProperty("#bitratemode", tag.BitRateMode);
          GUIPropertyManager.SetProperty("#bpm", strBPM);
          GUIPropertyManager.SetProperty("#channels", strChannels);
          GUIPropertyManager.SetProperty("#samplerate", strSampleRate);
        }

        GUIPropertyManager.SetProperty("#album", tag.Album);
        GUIPropertyManager.SetProperty("#genre", tag.Genre);
        GUIPropertyManager.SetProperty("#year", strYear);
        GUIPropertyManager.SetProperty("#albumArtist", tag.AlbumArtist);
        GUIPropertyManager.SetProperty("#discid", strDicsID);
        GUIPropertyManager.SetProperty("#disctotal", strDiscTotal);
        GUIPropertyManager.SetProperty("#trackTotal", strTrackTotal);
        GUIPropertyManager.SetProperty("#datelastplayed", strDateLastPlayed);
        GUIPropertyManager.SetProperty("#dateadded", strDateAdded);
      }
      else
      {
        GUIPropertyManager.SetProperty("#title", string.Empty);
        GUIPropertyManager.SetProperty("#track", string.Empty);
        GUIPropertyManager.SetProperty("#album", string.Empty);
        GUIPropertyManager.SetProperty("#artist", string.Empty);
        GUIPropertyManager.SetProperty("#genre", string.Empty);
        GUIPropertyManager.SetProperty("#year", string.Empty);
        GUIPropertyManager.SetProperty("#rating", string.Empty);
        GUIPropertyManager.SetProperty("#duration", string.Empty);
        GUIPropertyManager.SetProperty("#albumArtist", string.Empty);
        GUIPropertyManager.SetProperty("#bitRate", string.Empty);
        GUIPropertyManager.SetProperty("#comment", string.Empty);
        GUIPropertyManager.SetProperty("#composer", string.Empty);
        GUIPropertyManager.SetProperty("#conductor", string.Empty);
        GUIPropertyManager.SetProperty("#discid", string.Empty);
        GUIPropertyManager.SetProperty("#disctotal", string.Empty);
        GUIPropertyManager.SetProperty("#lyrics", string.Empty);
        GUIPropertyManager.SetProperty("#timesplayed", string.Empty);
        GUIPropertyManager.SetProperty("#trackTotal", string.Empty);
        GUIPropertyManager.SetProperty("#filetype", string.Empty);
        GUIPropertyManager.SetProperty("#codec", string.Empty);
        GUIPropertyManager.SetProperty("#bitratemode", string.Empty);
        GUIPropertyManager.SetProperty("#bpm", string.Empty);
        GUIPropertyManager.SetProperty("#channels", string.Empty);
        GUIPropertyManager.SetProperty("#samplerate", string.Empty);
        GUIPropertyManager.SetProperty("#datelastplayed", string.Empty);
        GUIPropertyManager.SetProperty("#dateadded", string.Empty);
      }
      
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null)
      {
        return;
      }

      if (item.Label == "..")
      {
        filmstrip.InfoImageFileName = string.Empty;
        return;
      }
      else
      {
        filmstrip.InfoImageFileName = item.ThumbnailImage;
      }
    }
    
    
    #region playlist management
    
    /// <summary>
    /// Just helper method to turn the enum in MusicState into
    /// an actual PlayListType
    /// </summary>
    /// <returns></returns>
    protected PlayListType GetPlayListType()
    { // we used to also use PLAYLIST_MUSIC_TEMP
      // this method is here incase we want to implement
      // multiple playlists again
      return PlayListType.PLAYLIST_MUSIC;
    }
    
    /// <summary>
    /// Genric code to add a list of playlist items to the current playlist
    /// It is up to the classes extending this class to convert to
    /// PlayListItem before submitting
    /// </summary>
    /// <param name="pItems">A list of PlayListItem to be added</param>
    /// <param name="clearPlaylist">If True then current playlist will be cleared</param>
    protected void AddItemsToPlaylist(List<PlayListItem> pItems, bool clearPlaylist)
    {
      PlayList pl = playlistPlayer.GetPlaylist(GetPlayListType());
      playlistPlayer.CurrentPlaylistType = GetPlayListType();
      int iStartFrom = 0; // where should we start in playlist
      
      // clear the playlist if required
      if (clearPlaylist)
      {
        pl.Clear();
        playlistPlayer.Reset();
      }
      
      //if not clearing the playlist and playback is stopped
      //then start at the end of the playlist
      int iCurrentItemCount = pl.Count;
      
      if(!(g_Player.Playing && g_Player.IsMusic))
      { // nothing is playing or something other than music is playing
        // and we are in playlist mode so may want to just add tracks to existing
        // playlist but start from the track we have just added
        if(iCurrentItemCount > 0)
        {
          // we are in playlist mode (play mode will have cleared list already)
          // and playlist has existing items but is not playing
          // so check with user if they want to clear the current playlist
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
          if (dlgYesNo != null)
          { // need localised strings here
            dlgYesNo.SetHeading("Existing Playlist Detected");
            dlgYesNo.SetLine(1, "Do you want to clear the current playlist?");
            dlgYesNo.DoModal(GetID);
            if (dlgYesNo.IsConfirmed)
            { // clear playlist as requested by user
              pl.Clear();
              playlistPlayer.Reset();
            }
            else
            { // start from end of playlist
              iStartFrom = iCurrentItemCount;
            }
          }
        }
      }
      
      foreach (PlayListItem pItem in pItems)
      { // actually add items to the playlist
        pl.Add(pItem);
      }
      
      if (iCurrentItemCount == 0)
      { // if playlist has been cleared before calling this
        // or there is nothing in existing playlist then
        // start playback
        if (!facadeLayout.SelectedListItem.IsFolder && PlayAllOnSingleItemPlayNow)
        {
          // we are here is we are in tracks listing and playlist was empty
          // we are adding multiple tracks to playlist so need to ensure
          // playback starts on selected item
          int iSelectedItem = facadeLayout.SelectedListItemIndex;
          if (facadeLayout[0].Label == "..")
          { // if facade has ".." parent then ignore this
            iSelectedItem = iSelectedItem - 1;
          }
          if (iSelectedItem > 0)
          { // playback was not started from first track
            // so ensure playlist starts from selected track
            iStartFrom = iSelectedItem;
          }
        }
        // start playlist in
        playlistPlayer.Play(iStartFrom);
      }
      else if(!(g_Player.Playing && g_Player.IsMusic))
      { // if either nothing is playing or what is playing is not music
        // then start playlist at appropriate point
        // only get here if playlist was not empty before adding tracks
        // hence we are in playlist mode and only need to start if not
        // already playing
        playlistPlayer.Play(iStartFrom);
      }
      
      DoPlayNowJumpTo(pItems.Count);
      
    }
    
    protected void InsertItemsToPlaylist(List<PlayListItem> pItems)
    {
      PlayList pl = playlistPlayer.GetPlaylist(GetPlayListType());
      playlistPlayer.CurrentPlaylistType = GetPlayListType();
      int iStartFrom = 0; // where should we start in playlist if needed
      //if not clearing the playlist and playback is stopped
      //then start at the end of the playlist
      int iCurrentItemCount = pl.Count;

      
      if(!(g_Player.Playing && g_Player.IsMusic))
      { // nothing is playing or something other than music is playing
        if(iCurrentItemCount > 0)
        {
          // we are in playlist mode (play mode will have cleared list already)
          // and playlist has existing items but is not playing
          // so check with user if they want to clear the current playlist
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
          if (dlgYesNo != null)
          { // need localised strings here
            dlgYesNo.SetHeading("Existing Playlist Detected");
            dlgYesNo.SetLine(1, "Do you want to clear the current playlist?");
            dlgYesNo.DoModal(GetID);
            if (dlgYesNo.IsConfirmed)
            { // clear playlist as requested by user
              pl.Clear();
              playlistPlayer.Reset();
              iStartFrom = 0;
            }
            else
            {
              iStartFrom = iCurrentItemCount;
            }
          }
        }
      }


      int index = Math.Max(playlistPlayer.CurrentSong, 0);
      for(int i = 0; i < pItems.Count; i++)
      {
        pl.Insert(pItems[i], index + i);
      }
      
      // if either nothing is playing or what is playing
      // is not music then start playlist at appropriate
      // point
      if(!(g_Player.Playing && g_Player.IsMusic))
      { // start playing if not already started
        playlistPlayer.Play(iStartFrom);
      }
      
      DoPlayNowJumpTo(pItems.Count);
    }
    
    #endregion
    
  }
}