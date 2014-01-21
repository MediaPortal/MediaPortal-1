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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Music.Amazon;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Util;
using WindowPlugins;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

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

    protected MusicSort.SortMethod currentSortMethod = MusicSort.SortMethod.Name;
    protected string m_strPlayListPath = string.Empty;
    private bool _autoShuffleOnLoad = false;

    protected MusicDatabase m_database;

    protected List<Share> _shareList = new List<Share>();

    private static readonly string defaultTrackTag = "[%track%. ][%artist% - ][%title%]";

    private static readonly string[] _sortModes = {
                                                    "Name", "Date", "Size", "Track", "Duration", "Title", "Artist",
                                                    "Album", "Filename",
                                                    "Rating", "Album Artist", "Year", "DiscID", "Composer", "Times Played"
                                                  };

    private static readonly string[] _defaultSortTags1 = {
                                                           defaultTrackTag, defaultTrackTag, defaultTrackTag,
                                                           defaultTrackTag,
                                                           defaultTrackTag, defaultTrackTag, defaultTrackTag,
                                                           defaultTrackTag,
                                                           defaultTrackTag, defaultTrackTag, defaultTrackTag,
                                                           defaultTrackTag,
                                                           defaultTrackTag, defaultTrackTag, defaultTrackTag
                                                         };

    private static readonly string[] _defaultSortTags2 = {
                                                           "%duration%", "%date%", "%filesize%", "%duration%",
                                                           "%duration%",
                                                           "%duration%", "%duration%", "%duration%", "%filesize%",
                                                           "%rating%",
                                                           "%duration%", "%year%", "%disc#%", "%duration%", "%timesplayed%"
                                                         };

    private static string[] _sortTags1 = new string[20];
    private static string[] _sortTags2 = new string[20];
    protected PlayListPlayer playlistPlayer;

    protected PlayNowJumpToType PlayNowJumpTo = PlayNowJumpToType.None;
    protected bool UsingInternalMusicPlayer = false;

    protected string _currentPlaying = string.Empty;
    protected string _selectOption = string.Empty;
    protected bool _addAllOnSelect;
    protected bool _playlistIsCurrent;

    protected bool _resumeEnabled = false;
    protected int _resumeAfter = 0;
    protected string _resumeSelect = "";
    protected string _resumeSearch = "";

    protected static BackgroundWorker bw;
    protected static bool defaultPlaylistLoaded = false;
    protected static bool ignorePlaylistChange = false;

    protected delegate void ReplacePlaylistDelegate(PlayList newPlaylist);
    protected delegate void StartPlayingPlaylistDelegate();

    protected bool _strippedPrefixes;

    #endregion

    #region SkinControls

    [SkinControl(8)] protected GUIButtonControl btnSearch = null;
    [SkinControl(12)] protected GUIButtonControl btnPlayCd = null;
    [SkinControl(10)] protected GUIButtonControl btnSavedPlaylists = null;
    [SkinControl(18)] protected GUICheckButton btnAutoDJ;

    #endregion

    #region Constructor / Destructor

    public GUIMusicBaseWindow()
    {
      if (m_database == null)
      {
        m_database = MusicDatabase.Instance;
      }

      playlistPlayer = PlayListPlayer.SingletonPlayer;

      playlistPlayer.PlaylistChanged += new PlayListPlayer.PlaylistChangedEventHandler(playlistPlayer_PlaylistChanged); 
      playlistPlayer.PlaylistChanged += playlistPlayer_PlaylistChanged;
      g_Player.PlayBackChanged += OnPlaybackChangedOrStopped;
      g_Player.PlayBackStopped += OnPlaybackChangedOrStopped;

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        string playNowJumpTo = xmlreader.GetValueAsString("music", "playnowjumpto", "none");

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
        MusicState.AutoDJEnabled = xmlreader.GetValueAsBool("lastfm:test", "autoDJ", false);
        _createMissingFolderThumbCache = xmlreader.GetValueAsBool("thumbnails", "musicfolderondemand", true);
        _createMissingFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs", false);
        _useFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", true);

        _selectOption = xmlreader.GetValueAsString("musicfiles", "selectOption", "play");
        _addAllOnSelect = xmlreader.GetValueAsBool("musicfiles", "addall", true);
        _playlistIsCurrent = xmlreader.GetValueAsBool("musicfiles", "playlistIsCurrent", true);

        _strippedPrefixes = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);

        for (int i = 0; i < _sortModes.Length; ++i)
        {
          _sortTags1[i] = xmlreader.GetValueAsString("mymusic", _sortModes[i] + "1", _defaultSortTags1[i]);
          _sortTags2[i] = xmlreader.GetValueAsString("mymusic", _sortModes[i] + "2", _defaultSortTags2[i]);
        }

        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";

        m_strPlayListPath = xmlreader.GetValueAsString("music", "playlists", playListFolder);
        m_strPlayListPath = Util.Utils.RemoveTrailingSlash(m_strPlayListPath);

        _resumeEnabled = xmlreader.GetValueAsBool("audioplayer", "enableResume", false);
        _resumeAfter = Convert.ToInt32(xmlreader.GetValueAsString("audioplayer", "resumeAfter", "0"));
        _resumeSelect = xmlreader.GetValueAsString("audioplayer", "resumeSelect", "");
        _resumeSearch = xmlreader.GetValueAsString("audioplayer", "resumeSearch", "");

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
      switch (s.Trim().ToLowerInvariant())
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
        case "composer":
          return MusicSort.SortMethod.Composer;
        case "timesplayed":
          return MusicSort.SortMethod.TimesPlayed;
      }
      if (!string.IsNullOrEmpty(s))
      {
        Log.Error("GUIMusicBaseWindow::GetSortMethod: Unknown String - " + s);
      }
      return MusicSort.SortMethod.Name;
    }

    #endregion

    /// <summary>
    /// Invoked when a song is Stopped or Changed to next song
    /// If "Resume" is configured and the meta matches the resume settings then the Resume time is stored in the DB
    /// </summary>
    /// <param name="type"></param>
    /// <param name="stoptime"></param>
    /// <param name="filename"></param>
    void OnPlaybackChangedOrStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }

      if (_resumeEnabled)
      {
        Song song = new Song();
        // We might have reached the end of a song, then clear the resumeAt time
        m_database.GetSongByFileName(filename, ref song);
        int endTime = song.Duration - MusicPlayer.BASS.Config.CrossFadeIntervalMs / 1000;
        if (song.Id > -1 && (endTime == stoptime || stoptime < _resumeAfter && song.ResumeAt > 0))
        {
          song.ResumeAt = 0;
          m_database.SetResume(song);
          return;
        }

        if (stoptime > _resumeAfter)
        {
          Log.Info("GUIMusic: Song stopped at {0} seconds with resume support enabled", stoptime);
          if (_resumeSelect.Length > 0)
          {
            MusicTag tag = null;
            // We found a valid song, if the id is > -1
            if (song.Id > -1)
            {
              tag = song.ToMusicTag();
            }
            else
            {
              // read tag from file
              tag = m_database.GetTag(filename);
            }

            string value = "";
            switch (_resumeSelect)
            {
              case "Genre":
                value = tag.Genre;
                break;

              case "Title":
                value = tag.Title;
                break;

              case "Filename":
                value = tag.FileName;
                break;

              case "Album":
                value = tag.Album;
                break;

              case "Artist":
                value = tag.Artist;
                break;

              case "AlbumArtist":
                value = tag.AlbumArtist;
                break;

              case "Composer":
                value = tag.Composer;
                break;

              case "Conductor":
                value = tag.Conductor;
                break;
            }

            if (!value.Contains(_resumeSearch))
            {
              Log.Info("GUIMusic: Tags not matching selection criteria. No resumetime stored.");
              return;
            }
          }

          if (song.Id == -1)
          {
            // No Song found. Let's add it to the database and then retrieve it
            Log.Debug("GUIMusic: Song not found in database. Add to database");
            m_database.AddSong(filename);
            m_database.GetSongByFileName(filename, ref song);
          }

          song.ResumeAt = stoptime;
          m_database.SetResume(song);
        }
      }
    }

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
      if (action.wID == Action.ActionType.ACTION_IMPORT_TRACK)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeTrack(facadeLayout, GetID);
        return;
      }
      if (action.wID == Action.ActionType.ACTION_IMPORT_DISC)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeDisc(facadeLayout, GetID);
        return;
      }
      if (action.wID == Action.ActionType.ACTION_CANCEL_IMPORT)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.Cancel();
        return;
      }
      if (action.wID == Action.ActionType.ACTION_QUEUE_ITEM)
      {
        // if playlist screen shows current playlist or user is playing music using background playlist
        // then queue track in that list
        if (_playlistIsCurrent || playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_TEMP)
        {
          AddSelectionToCurrentPlaylist(false, false);
        }
        else
        {
          // user is playing a playlist so add track to the playlist
          AddSelectionToPlaylist();
        }
        return;
      }
      if (action.wID == Action.ActionType.ACTION_ADD_TO_PLAYLIST)
      {
        AddSelectionToPlaylist();
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
      if (control == btnAutoDJ)
      {
        MusicState.AutoDJEnabled = !MusicState.AutoDJEnabled;
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
          strLine = GUILocalizeStrings.Get(366);
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
        case MusicSort.SortMethod.DiscID:
          strLine = GUILocalizeStrings.Get(1205);
          break;
        case MusicSort.SortMethod.Composer:
          strLine = GUILocalizeStrings.Get(1208);
          break;
        case MusicSort.SortMethod.TimesPlayed:
          strLine = GUILocalizeStrings.Get(1209);
          break;
      }

      if (btnSortBy != null)
      {
        btnSortBy.Label = GUILocalizeStrings.Get(96) + strLine;
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

      if (btnAutoDJ != null)
      {
        btnAutoDJ.Selected = MusicState.AutoDJEnabled;
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
        xmlwriter.SetValueAsBool("lastfm:test", "autoDJ", MusicState.AutoDJEnabled);
      }
      base.OnPageDestroy(newWindowId);
    }

    protected void LoadPlayList(string strPlayList)
    {
      LoadPlayList(strPlayList, true);
    }

    protected void LoadPlayList(string strPlayList, bool startPlayback)
    {
      LoadPlayList(strPlayList, startPlayback, false, false);
    }

    protected void LoadPlayList(string strPlayList, bool startPlayback, bool isAsynch)
    {
      LoadPlayList(strPlayList, startPlayback, isAsynch, false);
    }

    protected void LoadPlayList(string strPlayList, bool startPlayback, bool isAsynch, bool defaultLoad)
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
        if (isAsynch && defaultLoad) // we might not be in GUI yet! we have asynch and default load because we might want to use asynch loading from gui button too, later!
          throw new Exception(string.Format("Unable to load Playlist file: {0}", strPlayList)); // exception is handled in backgroundworker
        else
          TellUserSomethingWentWrong();
        return;
      }

      if (_autoShuffleOnLoad)
      {
        playlist.Shuffle();
      }

      playlistPlayer.CurrentPlaylistName = Path.GetFileNameWithoutExtension(strPlayList);
      if (playlist.Count == 1 && startPlayback)
      {
        Log.Info("GUIMusic:Play: play single playlist item - {0}", playlist[0].FileName);
        // Default to type Music, when a playlist has been selected from My Music
        g_Player.Play(playlist[0].FileName, g_Player.MediaType.Music);
        return;
      }

      if (null != bw && isAsynch && bw.CancellationPending)
        return;

      // clear current playlist
      //playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();

      Song song = new Song();
      PlayList newPlaylist = new PlayList();

      // add each item of the playlist to the playlistplayer
      for (int i = 0; i < playlist.Count; ++i)
      {
        if (null != bw && isAsynch && bw.CancellationPending)
          return;

        PlayListItem playListItem = playlist[i];
        if (m_database.GetSongByFileName(playListItem.FileName, ref song))
        {
          MusicTag tag = new MusicTag();
          tag = song.ToMusicTag();
          playListItem.MusicTag = tag;
        }
        if (Util.Utils.FileExistsInCache(playListItem.FileName) ||
            playListItem.Type == PlayListItem.PlayListItemType.AudioStream)
        {
          newPlaylist.Add(playListItem);
        }
        else
        {
          Log.Info("Playlist: File {0} no longer exists. Skipping item.", playListItem.FileName);
        }
      }

      if (null != bw && isAsynch && bw.CancellationPending)
        return;

      ReplacePlaylist(newPlaylist);

      if (startPlayback)
        StartPlayingPlaylist();
    }

    private void StartPlayingPlaylist()
    {
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        StartPlayingPlaylistDelegate d = StartPlayingPlaylist;
        GUIGraphicsContext.form.Invoke(d);
      }

      // if we got a playlist
      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0)
      {
        // then get 1st song
        PlayList playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
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

    private void ReplacePlaylist(PlayList newPlaylist)
    {
      if (GUIGraphicsContext.form.InvokeRequired)
      {
        ReplacePlaylistDelegate d = ReplacePlaylist;
        GUIGraphicsContext.form.Invoke(d, newPlaylist);
      }

      try
      {
        ignorePlaylistChange = true;
        playlistPlayer.ReplacePlaylist(PlayListType.PLAYLIST_MUSIC, newPlaylist);

        if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC)
          playlistPlayer.Reset();
      }
      finally
      {
        ignorePlaylistChange = false;
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
      facadeLayout.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));
      UpdateButtonStates();
      SelectCurrentItem();
    }

    public static bool SetTrackLabels(ref GUIListItem item, MusicSort.SortMethod CurrentSortMethod)
    {
      if (item.MusicTag == null)
      {
        return false;
      }

      MusicTag tag = (MusicTag)item.MusicTag;

      string trackNr = tag.Track > 0 ? String.Format("{0:##00}", tag.Track) : string.Empty;
      string fileSize = Util.Utils.GetSize(item.Size);
      string year = tag.Year >= 1900 ? tag.Year.ToString() : string.Empty;
      string filename = Util.Utils.GetFilename(item.Path);
      string duration = Util.Utils.SecondsToHMSString(tag.Duration);
      string rating = tag.Rating.ToString();
      string discID = tag.DiscID > 0 ? tag.DiscID.ToString() : string.Empty;
      string date = tag.DateTimeModified.ToShortDateString();
      string timesPlayed = tag.TimesPlayed.ToString(); // 0 is ok here as it is possible song never played

      string line1, line2;
      if (CurrentSortMethod == MusicSort.SortMethod.AlbumArtist)
      {
        line1 = _sortTags1[(int)MusicSort.SortMethod.Artist]; // Use Artist sort string for AlbumArtist
        line2 = _sortTags2[(int)MusicSort.SortMethod.Artist];
      }
      else
      {
        line1 = _sortTags1[(int)CurrentSortMethod];
        line2 = _sortTags2[(int)CurrentSortMethod];
      }
      line1 = Util.Utils.ReplaceTag(line1, "%track%", trackNr);
      line2 = Util.Utils.ReplaceTag(line2, "%track%", trackNr);
      line1 = Util.Utils.ReplaceTag(line1, "%filesize%", fileSize);
      line2 = Util.Utils.ReplaceTag(line2, "%filesize%", fileSize);
      line1 = Util.Utils.ReplaceTag(line1, "%albumartist%", tag.AlbumArtist);
      line2 = Util.Utils.ReplaceTag(line2, "%albumartist%", tag.AlbumArtist);
      line1 = Util.Utils.ReplaceTag(line1, "%artist%", tag.Artist);
      line2 = Util.Utils.ReplaceTag(line2, "%artist%", tag.Artist);
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
      line1 = Util.Utils.ReplaceTag(line1, "%disc#%", discID);
      line2 = Util.Utils.ReplaceTag(line2, "%disc#%", discID);
      line1 = Util.Utils.ReplaceTag(line1, "%timesplayed%", timesPlayed);
      line2 = Util.Utils.ReplaceTag(line2, "%timesplayed%", timesPlayed);
      line1 = Util.Utils.ReplaceTag(line1, "%timesplayed%", timesPlayed);
      line2 = Util.Utils.ReplaceTag(line2, "%timesplayed%", timesPlayed);
      item.Label = line1;
      item.Label2 = line2;

      return true;
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
      string strAlbumName = string.Empty;
      string strArtistName = string.Empty;
      if (tag != null)
      {
        if (!string.IsNullOrEmpty(tag.Album))
        {
          strAlbumName = tag.Album;
        }
        if (!string.IsNullOrEmpty(tag.Artist))
        {
          strArtistName = tag.Artist;
        }
      }

      // attempt to pick up album thumb if already scanned 
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

      // attempt to load folder.jpg
      if (!Util.Utils.IsAVStream(filename))
      {
        string strFolderThumb = string.Empty;
        if (isfolder)
        {
          strFolderThumb = Util.Utils.GetLocalFolderThumbForDir(filename);
        }
        else
        {
          strFolderThumb = Util.Utils.GetLocalFolderThumb(filename);
        }

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
      }

      //TODO: consider lookup of embedded artwork

      return string.Empty;
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
      dlg.AddLocalizedString(366); // year
      dlg.AddLocalizedString(367); // rating
      dlg.AddLocalizedString(267); // duration
      dlg.AddLocalizedString(105); // size
      dlg.AddLocalizedString(104); // date
      dlg.AddLocalizedString(1205); // disc#
      dlg.AddLocalizedString(1208); // composer
      dlg.AddLocalizedString(1209); // times played

      switch (CurrentSortMethod)
      {
        case MusicSort.SortMethod.Name:
          dlg.SelectedLabel = 0;  // Value is the order in which the option was added to the menu above.
          break;
        case MusicSort.SortMethod.Artist:
        case MusicSort.SortMethod.AlbumArtist:
          dlg.SelectedLabel = 1;
          break;
        case MusicSort.SortMethod.Album:
          dlg.SelectedLabel = 2;
          break;
        case MusicSort.SortMethod.Track:
          dlg.SelectedLabel = 3;
          break;
        case MusicSort.SortMethod.Title:
          dlg.SelectedLabel = 4;
          break;
        case MusicSort.SortMethod.Filename:
          dlg.SelectedLabel = 5;
          break;
        case MusicSort.SortMethod.Rating:
          dlg.SelectedLabel = 6;
          break;
        case MusicSort.SortMethod.Duration:
          dlg.SelectedLabel = 7;
          break;
        case MusicSort.SortMethod.Size:
          dlg.SelectedLabel = 8;
          break;
        case MusicSort.SortMethod.Date:
          dlg.SelectedLabel = 9;
          break;
      }

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
        case 366:
          CurrentSortMethod = MusicSort.SortMethod.Year;
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
        case 1205:
          CurrentSortMethod = MusicSort.SortMethod.DiscID;
          break;
        case 1208:
          CurrentSortMethod = MusicSort.SortMethod.Composer;
          break;
        case 1209:
          CurrentSortMethod = MusicSort.SortMethod.TimesPlayed;
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
      Log.Debug("Looking up Artist: {0}", albumName);

      var dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      var pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);

      var errorEncountered = true;
      var artist = new ArtistInfo();
      var artistInfo = new MusicArtistInfo();
      if (m_database.GetArtistInfo(artistName, ref artist))
      {
        // we already have artist info in database so just use that
        artistInfo.Set(artist);
        errorEncountered = false;
      }
      else
      {
        // lookup artist details

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

        var scraper = new AllmusicSiteScraper();
        List<AllMusicArtistMatch> artists;
        if (scraper.GetArtists(artistName, out artists))
        {
          var selectedMatch = new AllMusicArtistMatch();
          if (artists.Count == 1)
          {
            // only have single match so no need to ask user
            Log.Debug("Single Artist Match Found");
            selectedMatch = artists[0];
            errorEncountered = false;
          }
          else
          {
            // need to get user to choose which one to use
            Log.Debug("Muliple Artist Match Found ({0}) prompting user", artists.Count);
            var pDlg = (GUIDialogSelect2) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_SELECT2);
            if (null != pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(GUILocalizeStrings.Get(1303));
              foreach (var i in artists.Select(artistMatch => new GUIListItem
                {
                  Label = artistMatch.Artist + " - " + artistMatch.Genre,
                  Label2 = artistMatch.YearsActive,
                  Path = artistMatch.ArtistUrl,
                  IconImage = artistMatch.ImageUrl
                }))
              {
                pDlg.Add(i);
              }
              pDlg.DoModal(GetID);

              // and wait till user selects one
              var iSelectedMatch = pDlg.SelectedLabel;
              if (iSelectedMatch < 0)
              {
                return;
              }
              selectedMatch = artists[iSelectedMatch];
            }

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
          }
          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(60);
            dlgProgress.Progress();
          }
          if (artistInfo.Parse(selectedMatch.ArtistUrl))
          {
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(80);
              dlgProgress.Progress();
            }
            // set values to actual artist to ensure they match track data
            // rather than values that might be returned from allmusic.com
            artistInfo.Artist = artistName;
            m_database.AddArtistInfo(artistInfo.Get());
            errorEncountered = false;
          }
        }
      }

      if (null != dlgProgress)
      {
        dlgProgress.SetPercentage(100);
        dlgProgress.Progress();
        dlgProgress.Close();
        dlgProgress = null;
      }

      if (!errorEncountered)
      {
        var pDlgArtistInfo = (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_ARTIST_INFO);
        if (null != pDlgArtistInfo)
        {
          pDlgArtistInfo.Artist = artistInfo;
          pDlgArtistInfo.DoModal(GetID);

          if (pDlgArtistInfo.NeedsRefresh)
          {
            m_database.DeleteArtistInfo(artistInfo.Artist);
            ShowArtistInfo(artistName, albumName);
          }
        }
      }
      else
      {
        Log.Debug("Unable to get artist details");

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

    protected override void OnInfo(int iItem)
    {
      var pItem = facadeLayout[iItem];
      if (pItem == null)
      {
        return;
      }
      var song = pItem.AlbumInfoTag as Song;
      if (song == null)
      {
        return;
      }
      else if (song.Album != "")
      {
        var artist = song.Artist;
        if (_strippedPrefixes)
        {
          artist = Util.Utils.UndoArtistPrefix(song.Artist);
        }
        ShowAlbumInfo(artist, song.Album);
      }
      else if (!string.IsNullOrEmpty(song.Artist))
      {
        var artist = song.Artist;
        if (_strippedPrefixes)
        {
          artist = Util.Utils.UndoArtistPrefix(song.Artist);
        }

        ShowArtistInfo(artist, song.Album);
      }
      else if (!string.IsNullOrEmpty(song.AlbumArtist))
      {
        var artist = song.AlbumArtist;
        if (_strippedPrefixes)
        {
          artist = Util.Utils.UndoArtistPrefix(song.AlbumArtist);
        }

        ShowArtistInfo(artist, song.Album);
      }
      facadeLayout.RefreshCoverArt();
    }

    protected void ShowAlbumInfo(string artistName, string albumName)
    {
      ShowAlbumInfo(GetID, artistName, albumName);
    }

    public void ShowAlbumInfo(int parentWindowID, string artistName, string albumName)
    {
      Log.Debug("Searching for album: {0} - {1}", albumName, artistName);

      var dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      var pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);

      var errorEncountered = true;
      var album = new AlbumInfo();
      var albumInfo = new MusicAlbumInfo();
      if (m_database.GetAlbumInfo(albumName, artistName, ref album))
      {
        // we already have album info in database so just use that
        albumInfo.Set(album);
        errorEncountered = false;
      }
      else
      {// lookup details.  start with artist

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
          dlgProgress.SetHeading(326);
          dlgProgress.SetLine(1, albumName);
          dlgProgress.SetLine(2, artistName);
          dlgProgress.SetPercentage(0);
          dlgProgress.StartModal(GetID);
          dlgProgress.Progress();
          dlgProgress.ShowProgressBar(true);
        }

        var scraper = new AllmusicSiteScraper();
        List<AllMusicArtistMatch> artists;
        var selectedMatch = new AllMusicArtistMatch();

        if (scraper.GetArtists(artistName, out artists))
        {
          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(20);
            dlgProgress.Progress();
          }
          if (artists.Count == 1)
          {
            // only have single match so no need to ask user
            Log.Debug("Single Artist Match Found");
            selectedMatch = artists[0];
          }
          else
          {
            // need to get user to choose which one to use
            Log.Debug("Muliple Artist Match Found ({0}) prompting user", artists.Count);
            var pDlg = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT2);
            if (null != pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(GUILocalizeStrings.Get(1303));

              foreach (var i in artists.Select(artistMatch => new GUIListItem
                                                                {
                                                                  Label = artistMatch.Artist + " - " + artistMatch.Genre,
                                                                  Label2 = artistMatch.YearsActive,
                                                                  Path = artistMatch.ArtistUrl,
                                                                  IconImage = artistMatch.ImageUrl
                                                                }))
              {
                pDlg.Add(i);
              }
              pDlg.DoModal(GetID);

              // and wait till user selects one
              var iSelectedMatch = pDlg.SelectedLabel;
              if (iSelectedMatch < 0)
              {
                return;
              }
              selectedMatch = artists[iSelectedMatch];
            }

            if (null != dlgProgress)
            {
              dlgProgress.Reset();
              dlgProgress.SetHeading(326);
              dlgProgress.SetLine(1, albumName);
              dlgProgress.SetLine(2, artistName);
              dlgProgress.SetPercentage(40);
              dlgProgress.StartModal(GetID);
              dlgProgress.ShowProgressBar(true);
              dlgProgress.Progress();
            }
          }

          string strAlbumUrl;
          if (scraper.GetAlbumUrl(albumName, selectedMatch.ArtistUrl, out strAlbumUrl))
          {
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(60);
              dlgProgress.Progress();
            }
            if (albumInfo.Parse(strAlbumUrl))
            {
              if (null != dlgProgress)
              {
                dlgProgress.SetPercentage(80);
                dlgProgress.Progress();
              }
              // set values to actual artist and album name to ensure they match track data
              // rather than values that might be returned from allmusic.com
              albumInfo.Artist = artistName;
              albumInfo.Title = albumName;
              m_database.AddAlbumInfo(albumInfo.Get());
              errorEncountered = false;
            }

          }
        }
      }

      if (null != dlgProgress)
      {
        dlgProgress.SetPercentage(100);
        dlgProgress.Progress();
        dlgProgress.Close();
        dlgProgress = null;
      }

      if (!errorEncountered)
      {
        var pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)Window.WINDOW_MUSIC_INFO);
        if (null != pDlgAlbumInfo)
        {
          pDlgAlbumInfo.Album = albumInfo;

          pDlgAlbumInfo.DoModal(parentWindowID);
          if (pDlgAlbumInfo.NeedsRefresh)
          {
            m_database.DeleteAlbumInfo(albumName, artistName);
            ShowAlbumInfo(parentWindowID, artistName, albumName);
            return;
          }
        }
      }
      else
      {
        Log.Debug("No Album Found");

        if (null != pDlgOK)
        {
          pDlgOK.SetHeading(187);
          pDlgOK.SetLine(1, 187);
          pDlgOK.SetLine(2, string.Empty);
          pDlgOK.DoModal(GetID);
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

      switch (sWhere.ToLowerInvariant())
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
        string strDiscID = tag.DiscID <= 0 ? string.Empty : tag.DiscID.ToString();
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
        string strSampleRate = tag.SampleRate <= 0 ? string.Empty : tag.SampleRate.ToString();
        // Date Last Played
        string strDateLastPlayed = tag.DateTimePlayed == DateTime.MinValue
                                     ? string.Empty
                                     : tag.DateTimePlayed.ToShortDateString();
        // Date Added
        string strDateAdded = tag.DateTimeModified == DateTime.MinValue
                                ? string.Empty
                                : tag.DateTimeModified.ToShortDateString();

        if (item.IsFolder || item.Label == "..")
        {
          GUIPropertyManager.SetProperty("#music.title", string.Empty);
          GUIPropertyManager.SetProperty("#music.track", string.Empty);
          GUIPropertyManager.SetProperty("#music.rating", string.Empty);
          GUIPropertyManager.SetProperty("#music.duration", string.Empty);
          GUIPropertyManager.SetProperty("#music.artist", string.Empty);
          GUIPropertyManager.SetProperty("#music.bitRate", string.Empty);
          GUIPropertyManager.SetProperty("#music.comment", string.Empty);
          GUIPropertyManager.SetProperty("#music.composer", string.Empty);
          GUIPropertyManager.SetProperty("#music.conductor", string.Empty);
          GUIPropertyManager.SetProperty("#music.lyrics", string.Empty);
          GUIPropertyManager.SetProperty("#music.timesplayed", string.Empty);
          GUIPropertyManager.SetProperty("#music.filetype", string.Empty);
          GUIPropertyManager.SetProperty("#music.codec", string.Empty);
          GUIPropertyManager.SetProperty("#music.bitratemode", string.Empty);
          GUIPropertyManager.SetProperty("#music.bpm", string.Empty);
          GUIPropertyManager.SetProperty("#music.channels", string.Empty);
          GUIPropertyManager.SetProperty("#music.samplerate", string.Empty);
        }
        else
        {
          GUIPropertyManager.SetProperty("#music.title", tag.Title);
          GUIPropertyManager.SetProperty("#music.track", strTrack);
          GUIPropertyManager.SetProperty("#music.rating", strRating);
          GUIPropertyManager.SetProperty("#music.duration", strDuration);
          GUIPropertyManager.SetProperty("#music.artist", tag.Artist);
          GUIPropertyManager.SetProperty("#music.bitRate", strBitrate);
          GUIPropertyManager.SetProperty("#music.comment", tag.Comment);
          GUIPropertyManager.SetProperty("#music.composer", tag.Composer);
          GUIPropertyManager.SetProperty("#music.conductor", tag.Conductor);
          GUIPropertyManager.SetProperty("#music.lyrics", tag.Lyrics);
          GUIPropertyManager.SetProperty("#music.timesplayed", strTimesPlayed);
          GUIPropertyManager.SetProperty("#music.filetype", tag.FileType);
          GUIPropertyManager.SetProperty("#music.codec", tag.Codec);
          GUIPropertyManager.SetProperty("#music.bitratemode", tag.BitRateMode);
          GUIPropertyManager.SetProperty("#music.bpm", strBPM);
          GUIPropertyManager.SetProperty("#music.channels", strChannels);
          GUIPropertyManager.SetProperty("#music.samplerate", strSampleRate);
        }

        GUIPropertyManager.SetProperty("#music.album", tag.Album);
        GUIPropertyManager.SetProperty("#music.genre", tag.Genre);
        GUIPropertyManager.SetProperty("#music.year", strYear);
        GUIPropertyManager.SetProperty("#music.albumArtist", tag.AlbumArtist);
        GUIPropertyManager.SetProperty("#music.discid", strDiscID);
        GUIPropertyManager.SetProperty("#music.disctotal", strDiscTotal);
        GUIPropertyManager.SetProperty("#music.trackTotal", strTrackTotal);
        GUIPropertyManager.SetProperty("#music.datelastplayed", strDateLastPlayed);
        GUIPropertyManager.SetProperty("#music.dateadded", strDateAdded);

        // see if we have album info
        AlbumInfo _albumInfo = new AlbumInfo();
        if (MusicDatabase.Instance.GetAlbumInfo(tag.Album, tag.AlbumArtist, ref _albumInfo))
        {
          GUIPropertyManager.SetProperty("#AlbumInfo.Review", _albumInfo.Review);
          GUIPropertyManager.SetProperty("#AlbumInfo.Rating", _albumInfo.Rating.ToString());
          GUIPropertyManager.SetProperty("#AlbumInfo.Genre", _albumInfo.Genre);
          GUIPropertyManager.SetProperty("#AlbumInfo.Styles", _albumInfo.Styles);
          GUIPropertyManager.SetProperty("#AlbumInfo.Tones", _albumInfo.Tones);
          GUIPropertyManager.SetProperty("#AlbumInfo.Year", _albumInfo.Year.ToString());
        }
        else
        {
          GUIPropertyManager.SetProperty("#AlbumInfo.Review", string.Empty);
          GUIPropertyManager.SetProperty("#AlbumInfo.Rating", string.Empty);
          GUIPropertyManager.SetProperty("#AlbumInfo.Genre", string.Empty);
          GUIPropertyManager.SetProperty("#AlbumInfo.Styles", string.Empty);
          GUIPropertyManager.SetProperty("#AlbumInfo.Tones", string.Empty);
          GUIPropertyManager.SetProperty("#AlbumInfo.Year", string.Empty);
        }

        // see if we have artist info
        ArtistInfo _artistInfo = new ArtistInfo();
        String strArtist;
        if (string.IsNullOrEmpty(tag.Artist))
        {
          strArtist = tag.AlbumArtist;
        }
        else
        {
          strArtist = tag.Artist;
        }
        if (MusicDatabase.Instance.GetArtistInfo(strArtist, ref _artistInfo))
        {
          GUIPropertyManager.SetProperty("#ArtistInfo.Bio", _artistInfo.AMGBio);
          GUIPropertyManager.SetProperty("#ArtistInfo.Born", _artistInfo.Born);
          GUIPropertyManager.SetProperty("#ArtistInfo.Genres", _artistInfo.Genres);
          GUIPropertyManager.SetProperty("#ArtistInfo.Instruments", _artistInfo.Instruments);
          GUIPropertyManager.SetProperty("#ArtistInfo.Styles", _artistInfo.Styles);
          GUIPropertyManager.SetProperty("#ArtistInfo.Tones", _artistInfo.Tones);
          GUIPropertyManager.SetProperty("#ArtistInfo.YearsActive", _artistInfo.YearsActive);
        }
        else
        {
          GUIPropertyManager.SetProperty("#ArtistInfo.Bio", string.Empty);
          GUIPropertyManager.SetProperty("#ArtistInfo.Born", string.Empty);
          GUIPropertyManager.SetProperty("#ArtistInfo.Genres", string.Empty);
          GUIPropertyManager.SetProperty("#ArtistInfo.Instruments", string.Empty);
          GUIPropertyManager.SetProperty("#ArtistInfo.Styles", string.Empty);
          GUIPropertyManager.SetProperty("#ArtistInfo.Tones", string.Empty);
          GUIPropertyManager.SetProperty("#ArtistInfo.YearsActive", string.Empty);
        }
      }
      else
      {
        GUIPropertyManager.SetProperty("#music.title", string.Empty);
        GUIPropertyManager.SetProperty("#music.track", string.Empty);
        GUIPropertyManager.SetProperty("#music.album", string.Empty);
        GUIPropertyManager.SetProperty("#music.artist", string.Empty);
        GUIPropertyManager.SetProperty("#music.genre", string.Empty);
        GUIPropertyManager.SetProperty("#music.year", string.Empty);
        GUIPropertyManager.SetProperty("#music.rating", string.Empty);
        GUIPropertyManager.SetProperty("#music.duration", string.Empty);
        GUIPropertyManager.SetProperty("#music.albumArtist", string.Empty);
        GUIPropertyManager.SetProperty("#music.bitRate", string.Empty);
        GUIPropertyManager.SetProperty("#music.comment", string.Empty);
        GUIPropertyManager.SetProperty("#music.composer", string.Empty);
        GUIPropertyManager.SetProperty("#music.conductor", string.Empty);
        GUIPropertyManager.SetProperty("#music.discid", string.Empty);
        GUIPropertyManager.SetProperty("#music.disctotal", string.Empty);
        GUIPropertyManager.SetProperty("#music.lyrics", string.Empty);
        GUIPropertyManager.SetProperty("#music.timesplayed", string.Empty);
        GUIPropertyManager.SetProperty("#music.trackTotal", string.Empty);
        GUIPropertyManager.SetProperty("#music.filetype", string.Empty);
        GUIPropertyManager.SetProperty("#music.codec", string.Empty);
        GUIPropertyManager.SetProperty("#music.bitratemode", string.Empty);
        GUIPropertyManager.SetProperty("#music.bpm", string.Empty);
        GUIPropertyManager.SetProperty("#music.channels", string.Empty);
        GUIPropertyManager.SetProperty("#music.samplerate", string.Empty);
        GUIPropertyManager.SetProperty("#music.datelastplayed", string.Empty);
        GUIPropertyManager.SetProperty("#music.dateadded", string.Empty);
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
    /// Adds the songs for the selected GUIListItem and determines
    /// what tracks need to be added to the playlist
    /// </summary>
    /// <param name="clearPlaylist">If True then current playlist will be cleared</param>
    /// <param name="addAllTracks">Whether to add all tracks in folder</param>
    protected virtual void AddSelectionToCurrentPlaylist(bool clearPlaylist, bool addAllTracks) { }

    /// <summary>
    /// Adds songs to the playlist without affecting what is playing
    /// </summary>
    protected virtual void AddSelectionToPlaylist() { }


    /// <summary>
    /// Just helper method to turn the enum in MusicState into
    /// an actual PlayListType
    /// </summary>
    /// <returns></returns>
    protected PlayListType GetPlayListType()
    {
      if (_playlistIsCurrent)
      {
        return PlayListType.PLAYLIST_MUSIC;
      }
      return PlayListType.PLAYLIST_MUSIC_TEMP;
    }

    /// <summary>
    /// Genric code to add a list of playlist items to the current playlist
    /// It is up to the classes extending this class to convert to
    /// PlayListItem before submitting
    /// </summary>
    /// <param name="pItems">A list of PlayListItem to be added</param>
    /// <param name="clearPlaylist">If True then current playlist will be cleared</param>
    /// <param name="addAllTracks">Whether to add all tracks in folder to playlist</param>
    protected void AddItemsToCurrentPlaylist(List<PlayListItem> pItems, bool clearPlaylist, bool addAllTracks)
    {
      PlayList pl = playlistPlayer.GetPlaylist(GetPlayListType());
      playlistPlayer.CurrentPlaylistType = GetPlayListType();
      int iStartFrom = 0; // where should we start in playlist
      int resumeAt = 0;

      // clear the playlist if required
      if (clearPlaylist)
      {
        pl.Clear();
        playlistPlayer.Reset();
      }

      //if not clearing the playlist and playback is stopped
      //then start at the end of the playlist
      int iCurrentItemCount = pl.Count;

      foreach (PlayListItem pItem in pItems)
      {
        // actually add items to the playlist
        pl.Add(pItem);
      }

      // If Resume has been enabled we need to check te first item for resume information
      if (_resumeEnabled && facadeLayout.SelectedListItem != null)
      {
        GUIListItem item = facadeLayout.SelectedListItem;
        if (!item.IsFolder)
        {
          Song song = new Song();
          if (m_database.GetSongByFileName(item.Path, ref song))
          {
            if (song.ResumeAt > 0)
            {
              resumeAt = song.ResumeAt;
              GUIResumeDialog.Result result =
                GUIResumeDialog.ShowResumeDialog(song.Title, resumeAt,
                                                 GUIResumeDialog.MediaType.Recording);

              if (result == GUIResumeDialog.Result.Abort)
              {
                return;
              }

              if (result == GUIResumeDialog.Result.PlayFromBeginning)
              {
                resumeAt = 0;
              }
            }
          }
        }
      }

      // not null check is needed here because we can play a CD from menu button
      // without a facade item being selected
      if (iCurrentItemCount == 0 && facadeLayout != null && facadeLayout.SelectedListItem != null)
      {
        // if playlist has been cleared before calling this
        // or there is nothing in existing playlist then
        // start playback
        if (!facadeLayout.SelectedListItem.IsFolder && pItems.Count > 1)
        {
          // we are here is we are in tracks listing and playlist was empty
          // we are adding multiple tracks to playlist so need to ensure
          // playback starts on selected item
          int iSelectedItem = facadeLayout.SelectedListItemIndex;
          int numberOfFolders = facadeLayout.Count - pl.Count;
          iSelectedItem = iSelectedItem - numberOfFolders;
          if (iSelectedItem > 0)
          {
            // playback was not started from first track
            // so ensure playlist starts from selected track
            iStartFrom = iSelectedItem;
          }
        }
        // start playlist in
        playlistPlayer.Play(iStartFrom);
      }
      else if (!(g_Player.Playing && g_Player.IsMusic))
      {
        // if either nothing is playing or what is playing is not music
        // then start playlist at appropriate point
        // only get here if playlist was not empty before adding tracks
        // hence we are in playlist mode and only need to start if not
        // already playing
        playlistPlayer.Play(iStartFrom);
      }

      // Position the player, if we need to Resume
      if (g_Player.Playing && resumeAt > 0)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_POSITION, 0, 0, 0, 0, 0, null);
        msg.Param1 = resumeAt;
        GUIGraphicsContext.SendMessage(msg);
      }

      DoPlayNowJumpTo(pItems.Count);
    }

    /// <summary>
    /// When playlist is not current playlist (current tracks are added to TEMP playlist)
    /// Then allow users to add tracks to the playlist without affecting playback
    /// </summary>
    /// <param name="pItems">Items to add to the playlist</param>
    protected void AddItemsToPlaylist(List<PlayListItem> pItems)
    {
      PlayList pl = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      foreach (PlayListItem pItem in pItems)
      {
        // actually add items to the playlist
        pl.Add(pItem);
      }

      if (facadeLayout.SelectedListItemIndex < facadeLayout.Count - 1)
      {
        GUIControl.SelectItemControl(GetID, facadeLayout.GetID, facadeLayout.SelectedListItemIndex + 1);
      }
    }

    protected void InsertItemsToPlaylist(List<PlayListItem> pItems)
    {
      PlayList pl = playlistPlayer.GetPlaylist(GetPlayListType());
      playlistPlayer.CurrentPlaylistType = GetPlayListType();
      int iStartFrom = 0; // where should we start in playlist if needed
      //if not clearing the playlist and playback is stopped
      //then start at the end of the playlist
      int iCurrentItemCount = pl.Count;

      int index = Math.Max(playlistPlayer.CurrentSong, 0);
      for (int i = 0; i < pItems.Count; i++)
      {
        pl.Insert(pItems[i], index + i);
      }

      // if either nothing is playing or what is playing
      // is not music then start playlist at appropriate
      // point
      if (!(g_Player.Playing && g_Player.IsMusic))
      {
        // start playing if not already started
        playlistPlayer.Play(iStartFrom);
      }

      DoPlayNowJumpTo(pItems.Count);
    }

    void playlistPlayer_PlaylistChanged(PlayListType nPlayList, PlayList playlist)
    {
      if (null != bw && nPlayList == GetPlayListType() && !ignorePlaylistChange && bw.IsBusy && !bw.CancellationPending)
        bw.CancelAsync();
    }

    #endregion
  }
}