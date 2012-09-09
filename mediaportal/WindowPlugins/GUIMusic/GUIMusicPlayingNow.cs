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

#region usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Util;
using MediaPortal.Visualization;
using Action = MediaPortal.GUI.Library.Action;
using Timer = System.Timers.Timer;

#endregion

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Displays more information about the current running track
  /// Window added by Steve5 (why not using the MusicBaseWindow?)
  /// Internet lookups added by rtv
  /// </summary>
  public class GUIMusicPlayingNow : GUIInternalWindow
  {
    #region Enums

    private enum PopularityRating : int
    {
      unknown = 0,
      existent = 1,
      known = 2,
      famous = 3,
    }

    private enum ControlIDs
    {
      LBL_CAPTION = 1,
      IMG_COVERART = 112,
      //LBL_TRACK_NAME = 113,
      //LBL_ALBUM_NAME = 114,
      //LBL_ALBUM_YEAR = 115,
      //LBL_ARTIST_NAME = 6,
      IMG_TRACK_PROGRESS_BG = 117,
      PROG_TRACK = 118,
      //REMAIN_TRACK = 110,

      IMGLIST_RATING = 141,
      IMGLIST_NEXTRATING = 142,

      LBL_UP_NEXT = 20,
      LBL_NEXT_TRACK_NAME = 121,
      LBL_NEXT_ALBUM_NAME = 122,
      LBL_NEXT_ARTIST_NAME = 123,

      BEST_SIMILAR_TRACKS = 28,
      BEST_ALBUM_TRACKS = 29,

      BTN_LASTFM_LOVE = 30,
      BTN_LASTFM_BAN = 31,

      LIST_TAG_INFO = 155,
      LIST_ALBUM_INFO = 166,

      IMGLIST_UNKNOWN_TRACK1 = 77,
      IMGLIST_UNKNOWN_TRACK2 = 78,
      IMGLIST_UNKNOWN_TRACK3 = 79,
      IMGLIST_EXISTENT_TRACK1 = 80,
      IMGLIST_EXISTENT_TRACK2 = 81,
      IMGLIST_EXISTENT_TRACK3 = 82,
      IMGLIST_KNOWN_TRACK1 = 83,
      IMGLIST_KNOWN_TRACK2 = 84,
      IMGLIST_KNOWN_TRACK3 = 85,
      IMGLIST_FAMOUS_TRACK1 = 87,
      IMGLIST_FAMOUS_TRACK2 = 88,
      IMGLIST_FAMOUS_TRACK3 = 89,

      LBL_FORCE_FOCUS = 888,

      VUMETER_LEFT = 999,
      VUMETER_RIGHT = 998,
    }

    public enum TrackProgressType
    {
      Elapsed,
      CountDown
    }

    #endregion

    #region Properties

    public GUIMusicBaseWindow MusicWindow
    {
      set { _MusicWindow = value; }
    }

    #endregion

    #region Skin controls

    [SkinControl((int)ControlIDs.LBL_CAPTION)] protected GUILabelControl LblCaption = null;
    [SkinControl((int)ControlIDs.IMG_COVERART)] protected GUIImage ImgCoverArt = null;
    [SkinControl((int)ControlIDs.PROG_TRACK)] protected GUIProgressControl ProgTrack = null;
    [SkinControl((int)ControlIDs.IMG_TRACK_PROGRESS_BG)] protected GUIImage ImgTrackProgressBkGrnd = null;
    [SkinControl((int)ControlIDs.LBL_UP_NEXT)] protected GUILabelControl LblUpNext = null;
    [SkinControl((int)ControlIDs.LIST_TAG_INFO)] protected GUIListControl facadeSimilarTrackInfo = null;
    [SkinControl((int)ControlIDs.LIST_ALBUM_INFO)] protected GUIListControl facadeAlbumInfo = null;
    [SkinControl((int)ControlIDs.BEST_ALBUM_TRACKS)] protected GUIFadeLabel LblBestAlbumTracks = null;
    [SkinControl((int)ControlIDs.BEST_SIMILAR_TRACKS)] protected GUIFadeLabel LblBestSimilarTracks = null;
    [SkinControl((int)ControlIDs.IMGLIST_UNKNOWN_TRACK1)] protected GUIImageList ImgListUnknownTrack1 = null;
    [SkinControl((int)ControlIDs.IMGLIST_UNKNOWN_TRACK2)] protected GUIImageList ImgListUnknownTrack2 = null;
    [SkinControl((int)ControlIDs.IMGLIST_UNKNOWN_TRACK3)] protected GUIImageList ImgListUnknownTrack3 = null;
    [SkinControl((int)ControlIDs.IMGLIST_EXISTENT_TRACK1)] protected GUIImageList ImgListExistingTrack1 = null;
    [SkinControl((int)ControlIDs.IMGLIST_EXISTENT_TRACK2)] protected GUIImageList ImgListExistingTrack2 = null;
    [SkinControl((int)ControlIDs.IMGLIST_EXISTENT_TRACK3)] protected GUIImageList ImgListExistingTrack3 = null;
    [SkinControl((int)ControlIDs.IMGLIST_KNOWN_TRACK1)] protected GUIImageList ImgListKnownTrack1 = null;
    [SkinControl((int)ControlIDs.IMGLIST_KNOWN_TRACK2)] protected GUIImageList ImgListKnownTrack2 = null;
    [SkinControl((int)ControlIDs.IMGLIST_KNOWN_TRACK3)] protected GUIImageList ImgListKnownTrack3 = null;
    [SkinControl((int)ControlIDs.IMGLIST_FAMOUS_TRACK1)] protected GUIImageList ImgListFamousTrack1 = null;
    [SkinControl((int)ControlIDs.IMGLIST_FAMOUS_TRACK2)] protected GUIImageList ImgListFamousTrack2 = null;
    [SkinControl((int)ControlIDs.IMGLIST_FAMOUS_TRACK3)] protected GUIImageList ImgListFamousTrack3 = null;
    [SkinControl((int)ControlIDs.VUMETER_LEFT)] protected GUIImage VUMeterLeft = null;
    [SkinControl((int)ControlIDs.VUMETER_RIGHT)] protected GUIImage VUMeterRight = null;
    [SkinControl((int)ControlIDs.LBL_FORCE_FOCUS)] protected GUILabelControl lblForceFocus = null;
    [SkinControl((int)ControlIDs.BTN_LASTFM_LOVE)] protected GUIButtonControl btnLastFMLove = null;
    [SkinControl((int)ControlIDs.BTN_LASTFM_BAN)] protected GUIButtonControl btnLastFMBan = null;

    #endregion

    #region Event delegates

    protected delegate void PlaybackChangedDelegate(g_Player.MediaType type, string filename);

    protected delegate void TimerElapsedDelegate();

    protected delegate void AlbumInfoCompletedDelegate(AlbumInfoRequest request, List<Song> AlbumTracks);

    protected delegate void ArtistInfoCompletedDelegate(ArtistInfoRequest request, Song song);

    protected delegate void TagInfoCompletedDelegate(TagInfoRequest request, List<Song> TagTracks);

    #endregion

    #region Variables

    private const int DISPLAY_LISTITEM_COUNT = 3;

    private bool ControlsInitialized = false;
    private PlayListPlayer PlaylistPlayer = null;
    private string CurrentThumbFileName = string.Empty;
    private string CurrentTrackFileName = string.Empty;
    private string NextTrackFileName = string.Empty;
    private MusicTag PreviousTrackTag = null;
    private MusicTag CurrentTrackTag = null;
    private MusicTag NextTrackTag = null;
    private GUIMusicBaseWindow _MusicWindow = null;
    private AudioscrobblerUtils InfoScrobbler = null;
    private ScrobblerUtilsRequest _lastAlbumRequest;
    private ScrobblerUtilsRequest _lastArtistRequest;
    private ScrobblerUtilsRequest _lastTagRequest;
    private Timer ImageChangeTimer = null;
    private Timer VUMeterTimer = null;
    private List<String> ImagePathContainer = null;
    private bool _trackChanged = true;
    private bool _doArtistLookups = true;
    private bool _doAlbumLookups = true;
    private bool _doTrackTagLookups = true;
    private bool _audioscrobblerEnabled = false;
    private bool _usingBassEngine = false;
    private bool _showVisualization = false;
    private object _imageMutex = null;
    private string _vuMeter = "none";

    #endregion

    #region Constructor

    public GUIMusicPlayingNow()
    {
      GetID = (int)Window.WINDOW_MUSIC_PLAYING_NOW;
      PlaylistPlayer = PlayListPlayer.SingletonPlayer;
      ImagePathContainer = new List<string>();
      _imageMutex = new object();

      g_Player.PlayBackStarted += OnPlayBackStarted;
      g_Player.PlayBackStopped += OnPlayBackStopped;
      g_Player.PlayBackEnded += OnPlayBackEnded;

      LoadSettings();
    }

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        string VizName = "None";
        bool ShowViz = false;
        VizName = xmlreader.GetValueAsString("musicvisualization", "name", "None");
        ShowViz = xmlreader.GetValueAsBool("musicmisc", "showVisInNowPlaying", false);

        _doArtistLookups = xmlreader.GetValueAsBool("musicmisc", "fetchlastfmcovers", true);
        _doAlbumLookups = xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtopalbums", true);
        _doTrackTagLookups = xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtracktags", true);
        _vuMeter = xmlreader.GetValueAsString("musicmisc", "vumeter", "none");

        _audioscrobblerEnabled = xmlreader.GetValueAsBool("plugins", "Audioscrobbler", false);
        if (btnLastFMLove != null)
        {
          btnLastFMLove.Label = GUILocalizeStrings.Get(34010); // love
        }
        if (btnLastFMBan != null)
        {
          btnLastFMBan.Label = GUILocalizeStrings.Get(34011); // ban
        }

        if (ShowViz && VizName != "None")
        {
          _showVisualization = true;
        }
        else
        {
          _showVisualization = false;
          Log.Debug("GUIMusicPlayingNow: Viz disabled - ShowViz {0}, VizName {1}", Convert.ToString(ShowViz), VizName);
        }
      }

      _usingBassEngine = BassMusicPlayer.IsDefaultMusicPlayer;
    }

    #endregion

    #region Events

    private void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (!ControlsInitialized || type != g_Player.MediaType.Music)
      {
        return;
      }

      GUIGraphicsContext.form.Invoke(new PlaybackChangedDelegate(DoOnEnded), new object[] {type, filename});
    }

    private void DoOnEnded(g_Player.MediaType type, string filename)
    {
      if (GUIWindowManager.ActiveWindow == GetID)
      {
        Log.Debug("GUIMusicPlayingNow: g_Player_PlayBackEnded for {0}", filename);

        if (!g_Player.Playing && NextTrackTag == null)
        {
          Log.Debug("GUIMusicPlayingNow: All playlist items played - returning to previous window");
          Action action = new Action();
          action.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
          GUIGraphicsContext.OnAction(action);
        }
      }
    }

    private void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (!ControlsInitialized || type != g_Player.MediaType.Music)
      {
        return;
      }

      if (GUIWindowManager.ActiveWindow == GetID)
      {
        Log.Debug("GUIMusicPlayingNow: g_Player_PlayBackStopped for {0} - stoptime: {1}", filename, stoptime);
        Action action = new Action();
        action.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
        GUIGraphicsContext.OnAction(action);
      }
    }

    /// <summary>
    /// A song change happened on an Internet Stream
    /// </summary>
    /// <param name="sender"></param>
    private void OnInternetStreamSongChanged(object sender)
    {
      if (!ControlsInitialized)
      {
        return;
      }

      GUIGraphicsContext.form.Invoke(new PlaybackChangedDelegate(DoOnStarted),
                                     new object[] {g_Player.MediaType.Music, BassMusicPlayer.Player.CurrentFile});
    }


    private void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (!ControlsInitialized || type != g_Player.MediaType.Music)
      {
        return;
      }

      GUIGraphicsContext.form.Invoke(new PlaybackChangedDelegate(DoOnStarted), new object[] {type, filename});
    }

    private void DoOnStarted(g_Player.MediaType type, string filename)
    {
      Log.Debug("GUIMusicPlayingNow: g_Player_PlayBackStarted for {0}", filename);
      // Remove pending requests from the request queue
      if (_lastAlbumRequest != null)
      {
        InfoScrobbler.RemoveRequest(_lastAlbumRequest);
      }
      if (_lastArtistRequest != null)
      {
        InfoScrobbler.RemoveRequest(_lastArtistRequest);
      }
      if (_lastTagRequest != null)
      {
        InfoScrobbler.RemoveRequest(_lastTagRequest);
      }

      ImagePathContainer.Clear();
      ClearVisualizationImages();

      CurrentTrackFileName = filename;
      NextTrackFileName = PlaylistPlayer.GetNext();
      GetTrackTags();

      CurrentThumbFileName = GUIMusicBaseWindow.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);

      if (CurrentThumbFileName.Length < 1)
        // no LOCAL Thumb found because user has bad settings -> check if there is a folder.jpg in the share
      {
        CurrentThumbFileName = Util.Utils.GetFolderThumb(CurrentTrackFileName);
        if (!Util.Utils.FileExistsInCache(CurrentThumbFileName))
        {
          CurrentThumbFileName = string.Empty;
        }
      }

      if (CurrentThumbFileName.Length > 0)
      {
        // let us test if there is a larger cover art image
        string strLarge = Util.Utils.ConvertToLargeCoverArt(CurrentThumbFileName);
        if (Util.Utils.FileExistsInCache(strLarge))
        {
          CurrentThumbFileName = strLarge;
        }
        AddImageToImagePathContainer(CurrentThumbFileName);
      }

      UpdateImagePathContainer();
      UpdateTrackInfo();

      if (GUIWindowManager.ActiveWindow == GetID)
      {
        SetVisualizationWindow();
      }
    }

    #endregion

    #region Overrides

    /// <summary>
    /// We must not do a delayed loading, because we need to know if a VUMeter skin file exists, 
    /// so that we can fallback to the default Now Playing skin, if something goes wrong
    /// </summary>
    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override bool Init()
    {
      bool success = false;

      // Load the various Music NowPlaying files
      // we might have:
      // 1. A standard Now Playing to which we fall back
      // 2. a Now Playing with Analog VUMeter
      // 3. a Now Playing with Led VUMeter
      if (_vuMeter.ToLower() == "analog")
      {
        success = Load(GUIGraphicsContext.GetThemedSkinFile(@"\MyMusicPlayingNowAnVu.xml"));
      }
      else if (_vuMeter.ToLower() == "led")
      {
        success = Load(GUIGraphicsContext.GetThemedSkinFile(@"\MyMusicPlayingNowLedVu.xml"));
      }

      if (!success)
      {
        _vuMeter = "none";
        success = Load(GUIGraphicsContext.GetThemedSkinFile(@"\MyMusicPlayingNow.xml"));
      }

      return success;
    }

    public override void OnAction(Action action)
    {
      base.OnAction(action);
      switch (action.wID)
      {
        case Action.ActionType.ACTION_STOP:

          if (GUIWindowManager.ActiveWindow == GetID)
          {
            Action act = new Action();
            act.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
            GUIGraphicsContext.OnAction(act);
          }
          break;

          // Since a ACTION_STOP action clears the player and CurrentPlaylistType type
          // we need a way to restart playback after an ACTION_STOP has been received
        case Action.ActionType.ACTION_MUSIC_PLAY:
        case Action.ActionType.ACTION_NEXT_ITEM:
        case Action.ActionType.ACTION_PAUSE:
        case Action.ActionType.ACTION_PREV_ITEM:
          //if (PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC)
          if ((PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC) &&
              (PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC_TEMP))
          {
            LoadAndStartPlayList();
          }
          break;

        case Action.ActionType.ACTION_SHOW_INFO:
          //OnShowContextMenu();
          facadeSimilarTrackInfo.Clear();
          UpdateSimilarTrackInfo();
          FlipPictures();
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          switch (action.m_key.KeyChar)
          {
            case (int)Keys.D0:
              UpdateCurrentTrackRating(0);
              break;
            case (int)Keys.D1:
              UpdateCurrentTrackRating(1);
              break;
            case (int)Keys.D2:
              UpdateCurrentTrackRating(2);
              break;
            case (int)Keys.D3:
              UpdateCurrentTrackRating(3);
              break;
            case (int)Keys.D4:
              UpdateCurrentTrackRating(4);
              break;
            case (int)Keys.D5:
              UpdateCurrentTrackRating(5);
              break;
              // do not act on _every_ key
              //default:
              //  UpdateCurrentTrackRating(-1);
              //  break;
          }
          break;

        case Action.ActionType.ACTION_LASTFM_LOVE:
          DoLastFMLove();
          break;

        case Action.ActionType.ACTION_LASTFM_BAN:
          DoLastFMBan();
          break;
      }
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            if (facadeAlbumInfo != null)
            {
              if (message.SenderControlId == (int)ControlIDs.LIST_ALBUM_INFO) // listbox
              {
                if ((int)Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
                {
                  if (!facadeAlbumInfo.SelectedListItem.IsPlayed)
                  {
                    AddInfoTrackToPlaylist(facadeAlbumInfo.SelectedListItem);
                  }
                  else
                  {
                    Log.Info(
                      "GUIMusicPlayingNow: Could not add {0} from top album tracks because it was not found in your collection!");
                    GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
                    if (dlg == null)
                    {
                      break;
                    }
                    //dlg. Reset();
                    dlg.SetHeading(502); // Unable to complete action
                    dlg.SetLine(2, 33032); // This album's top track could not
                    dlg.SetLine(3, 33033); // be found in your collection!

                    dlg.DoModal(GetID);
                  }
                }
              }
            }
            if (facadeSimilarTrackInfo != null)
            {
              if (message.SenderControlId == (int)ControlIDs.LIST_TAG_INFO) // listbox
              {
                if ((int)Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
                {
                  AddInfoTrackToPlaylist(facadeSimilarTrackInfo.SelectedListItem);
                }
              }
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      // Get notification, that an Internet Stream has changed
      // Moved out of the constructor, since it would cause loading of BASS in the Main thread,
      // because it is called by the Plugin Manager
      BassMusicPlayer.Player.InternetStreamSongChanged += OnInternetStreamSongChanged;

      if (facadeAlbumInfo != null)
      {
        facadeAlbumInfo.Clear();
      }
      if (facadeSimilarTrackInfo != null)
      {
        facadeSimilarTrackInfo.Clear();
      }
      ImagePathContainer.Clear();

      _trackChanged = true;

      ToggleTopTrackRatings(false, PopularityRating.unknown);

      GUIPropertyManager.SetProperty("#currentmodule",
                                     String.Format("{0}/{1}", GUILocalizeStrings.Get(100005),
                                                   GUILocalizeStrings.Get(4540)));
      if (LblUpNext != null)
      {
        LblUpNext.Label = GUILocalizeStrings.Get(4541);
      }

      if (GUIPropertyManager.GetProperty("#Play.Next.Title") == string.Empty && LblUpNext != null)
      {
        LblUpNext.Visible = false;
      }

      if (LblBestAlbumTracks != null)
      {
        LblBestAlbumTracks.Visible = false;
      }
      if (LblBestSimilarTracks != null)
      {
        LblBestSimilarTracks.Visible = false;
      }
      ControlsInitialized = true;

      InfoScrobbler = AudioscrobblerUtils.Instance;

      if (ImageChangeTimer == null)
      {
        ImageChangeTimer = new Timer();
        ImageChangeTimer.Interval = 3600 * 1000;
        ImageChangeTimer.Elapsed += new ElapsedEventHandler(OnImageTimerTickEvent);
        ImageChangeTimer.Start();
      }

      // Start the VUMeter Update Timer, when it is enabled in skin file
      GUIPropertyManager.SetProperty("#VUMeterL", @"VU1.png");
      GUIPropertyManager.SetProperty("#VUMeterR", @"VU1.png");
      if (VUMeterTimer == null && _usingBassEngine &&
          _vuMeter.ToLower() != "none")
      {
        VUMeterTimer = new Timer();
        VUMeterTimer.Interval = 200;
        VUMeterTimer.Elapsed += new ElapsedEventHandler(OnVUMterTimerTickEvent);
        VUMeterTimer.Start();
      }

      UpdateImagePathContainer();

      if (g_Player.Playing)
      {
        OnPlayBackStarted(g_Player.MediaType.Music, g_Player.CurrentFile);

        SetVisualizationWindow();
      }
      else
      {
        CurrentTrackTag = null;
        NextTrackTag = null;
        UpdateTrackInfo();
        ClearVisualizationImages();
        // notify user what he's lost here?
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      // Remove pending requests from the request queue
      InfoScrobbler.RemoveRequest(_lastAlbumRequest);
      InfoScrobbler.RemoveRequest(_lastArtistRequest);
      InfoScrobbler.RemoveRequest(_lastTagRequest);

      ImageChangeTimer.Stop();

      if (VUMeterTimer != null)
      {
        VUMeterTimer.Stop();
        VUMeterTimer = null;
      }

      if (ImgCoverArt != null)
      {
        ImgCoverArt.Dispose();
      }

      GC.Collect();
      ControlsInitialized = false;

      // Make sure we clear any images we added so we revert back the coverart image
      ClearVisualizationImages();

      BassMusicPlayer.Player.InternetStreamSongChanged -= OnInternetStreamSongChanged;

      base.OnPageDestroy(new_windowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnLastFMLove)
      {
        DoLastFMLove();
      }
      if (control == btnLastFMBan)
      {
        DoLastFMBan();
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      MusicDatabase dbs;

      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(498); // Menu

      dlg.AddLocalizedString(930); //Add to favorites

      if (CurrentTrackTag.Album != null)
      {
        dlg.AddLocalizedString(33041);
      }

      if (_audioscrobblerEnabled)
      {
        dlg.AddLocalizedString(34010); // love
        dlg.AddLocalizedString(34011); // ban
      }

      dlg.DoModal(GetID);

      // The ImgCoverArt visibility gets restored when a context menu is popped up.
      // So we need to reset the visualization window visibility is restored after
      // the context menu is dismissed
      SetVisualizationWindow();

      if (dlg.SelectedId == -1)
      {
        return;
      }

      switch (dlg.SelectedId)
      {
        case 928: // Find Coverart
          if (_MusicWindow != null)
          {
            string albumFolderPath = Path.GetDirectoryName(CurrentTrackFileName);

            _MusicWindow.FindCoverArt(false, CurrentTrackTag.Artist, CurrentTrackTag.Album, albumFolderPath,
                                      CurrentTrackTag, -1);
            CurrentThumbFileName = GUIMusicBaseWindow.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);

            if (CurrentThumbFileName.Length > 0)
            {
              // let us test if there is a larger cover art image
              string strLarge = Util.Utils.ConvertToLargeCoverArt(CurrentThumbFileName);
              if (Util.Utils.FileExistsInCache(strLarge))
              {
                CurrentThumbFileName = strLarge;
              }
              AddImageToImagePathContainer(CurrentThumbFileName);

              if (_usingBassEngine)
              {
                BassMusicPlayer.Player.VisualizationWindow.CoverArtImagePath = CurrentThumbFileName;
              }

              UpdateImagePathContainer();
            }
          }
          break;

        case 4521: // Show Album Info
          if (_MusicWindow != null)
          {
            string albumFolderPath = Path.GetDirectoryName(CurrentTrackFileName);
            if (_MusicWindow != null)
            {
              _MusicWindow.ShowAlbumInfo(GetID, CurrentTrackTag.Artist, CurrentTrackTag.Album, albumFolderPath, CurrentTrackTag);
            }
          }
          break;

        case 930: // add to favorites
          dbs = MusicDatabase.Instance;
          Song currentSong = new Song();
          string strFile = g_Player.Player.CurrentFile;

          bool songFound = dbs.GetSongByFileName(strFile, ref currentSong);
          if (songFound)
          {
            if (currentSong == null)
            {
              return;
            }
            if (currentSong.Id < 0)
            {
              return;
            }
            currentSong.Favorite = true;
            dbs.SetFavorite(currentSong);
          }
          break;

        case 33041: //Play this album
          try
          {
            if (CurrentTrackTag != null)
            {
              dbs = MusicDatabase.Instance;
              ArrayList albumSongs = new ArrayList();
              String strAlbum = CurrentTrackTag.Album;

              bool albumSongsFound = dbs.GetSongsByAlbum(strAlbum, ref albumSongs);

              if (albumSongsFound)
              {
                for (int i = albumSongs.Count - 1; i >= 0; i--)
                {
                  Song song = (Song)albumSongs[i];
                  if (song.Title != CurrentTrackTag.Title && song.Artist == CurrentTrackTag.Artist)
                  {
                    AddSongToPlaylist(ref song);
                  }
                }
                OnSongInserted();
              }
            }
          }
          catch (Exception ex)
          {
            Log.Error("GUIMusicPlayingNow: error while adding album tracks for {0} - {1}", CurrentTrackTag.Album,
                      ex.Message);
          }
          break;

        case 34010: //love 
          DoLastFMLove();
          break;
        case 34011: //ban
          DoLastFMBan();
          break;
      }
    }

    #endregion

    #region Public methods

    public void OnUpdateAlbumInfoCompleted(AlbumInfoRequest request, List<Song> AlbumTracks)
    {
      if (request.Equals(_lastAlbumRequest))
      {
        GUIGraphicsContext.form.Invoke(new AlbumInfoCompletedDelegate(DoUpdateAlbumInfo),
                                       new object[] {request, AlbumTracks});
      }
      else
      {
        Log.Warn("GUIMusicPlayingNow: OnUpdateAlbumInfoCompleted: unexpected responsetype for request: {0}",
                 request.Type);
      }
    }

    private void DoUpdateAlbumInfo(AlbumInfoRequest request, List<Song> AlbumTracks)
    {
      GUIListItem item = null;
      facadeAlbumInfo.Clear();
      ToggleTopTrackRatings(false, PopularityRating.unknown);

      if (AlbumTracks.Count > 0)
      {
        // get total ratings
        float AlbumSongRating = 0;
        float ratingBase = 0;

        foreach (Song Song in AlbumTracks)
        {
          AlbumSongRating += Convert.ToSingle(Song.TimesPlayed);
        }
        // set % rating
        if (AlbumTracks[0].TimesPlayed > 0)
        {
          ratingBase = AlbumSongRating / Convert.ToSingle(AlbumTracks[0].TimesPlayed);
        }
        //else
        //  ratingBase = 0.01f;

        // avoid division by zero
        AlbumSongRating = AlbumSongRating > 0 ? AlbumSongRating : 1;

        for (int i = 0; i < AlbumTracks.Count; i++)
        {
          float rating = 0;

          if (i == 0)
          {
            AlbumTracks[i].Rating = 11;
          }
          else
          {
            rating = (int)(ratingBase * Convert.ToSingle(AlbumTracks[i].TimesPlayed));
            AlbumTracks[i].Rating = (int)(rating * 10 / AlbumSongRating);
          }

          item = new GUIListItem(AlbumTracks[i].ToShortString());
          item.Label = AlbumTracks[i].Title;
          //item.Label2 = " (" + GUILocalizeStrings.Get(931) + ": " + Convert.ToString(AlbumTracks[i].TimesPlayed) + ")";
          //item.Label2 = " (" + GUILocalizeStrings.Get(931) + ": " + Convert.ToString(AlbumTracks[i].Rating) + ")";

          item.MusicTag = AlbumTracks[i].ToMusicTag();
          item.IsPlayed = AlbumTracks[i].URL == "local" ? false : true;

          facadeAlbumInfo.Add(item);

          string currentTrackRatingProperty = "#Lastfm.Rating.AlbumTrack" + Convert.ToString(i + 1);
          try
          {
            GUIPropertyManager.SetProperty(currentTrackRatingProperty, Convert.ToString(AlbumTracks[i].Rating));
          }
          catch (Exception ex)
          {
            Log.Warn("GUIMusicPlayingNow: Could not set last.fm rating - {0}", ex.Message);
            break;
          }

          // display 3 items only
          if (facadeAlbumInfo.Count == DISPLAY_LISTITEM_COUNT)
          {
            break;
          }
        }
      }
      if (facadeAlbumInfo.Count > 0)
      {
        int popularity = AlbumTracks[0].TimesPlayed;

        // only display stars if list is filled
        if (facadeAlbumInfo.Count == DISPLAY_LISTITEM_COUNT)
        {
          if (popularity > 40000)
          {
            ToggleTopTrackRatings(true, PopularityRating.famous);
          }
          else if (popularity > 10000)
          {
            ToggleTopTrackRatings(true, PopularityRating.known);
          }
          else if (popularity > 2500)
          {
            ToggleTopTrackRatings(true, PopularityRating.existent);
          }
          else
          {
            ToggleTopTrackRatings(true, PopularityRating.unknown);
          }
        }

        CurrentThumbFileName = GUIMusicBaseWindow.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);
        if (CurrentThumbFileName.Length > 0)
        {
          // let us test if there is a larger cover art image
          string strLarge = Util.Utils.ConvertToLargeCoverArt(CurrentThumbFileName);
          if (Util.Utils.FileExistsInCache(strLarge))
          {
            CurrentThumbFileName = strLarge;
          }

          AddImageToImagePathContainer(CurrentThumbFileName);
        }
        if (LblBestAlbumTracks != null)
        {
          LblBestAlbumTracks.Visible = true;
        }

        UpdateImagePathContainer();

        // previously focus was set all the time
        // to maintain previous functionality set focus if
        // defauly skin control is present
        if (lblForceFocus != null)
        {
          GUIControl.FocusControl(GetID, ((int)ControlIDs.LIST_ALBUM_INFO));
        }
      }
    }

    public void OnUpdateArtistInfoCompleted(ArtistInfoRequest request, Song song)
    {
      if (request.Equals(_lastArtistRequest))
      {
        GUIGraphicsContext.form.Invoke(new ArtistInfoCompletedDelegate(DoUpdateArtistInfo), new object[] {request, song});
      }
      else
      {
        Log.Warn("NowPlaying.OnUpdateArtistInfoCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    private void DoUpdateArtistInfo(ArtistInfoRequest request, Song song)
    {
      // artist tag can contain multiple artists and 
      // will be separated by " | " so split by | then trim
      // so we will add one thumb for artist
      String[] strArtists = CurrentTrackTag.Artist.Split('|');
      foreach (String strArtist in strArtists)
      {
        CurrentThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists,
                                                          Util.Utils.MakeFileName(strArtist.Trim()));
        if (CurrentThumbFileName.Length > 0)
        {
          // let us test if there is a larger cover art image
          string strLarge = Util.Utils.ConvertToLargeCoverArt(CurrentThumbFileName);
          if (Util.Utils.FileExistsInCache(strLarge))
          {
            CurrentThumbFileName = strLarge;
          }

          AddImageToImagePathContainer(CurrentThumbFileName);
          UpdateImagePathContainer();
        }
      }
    }

    public void OnUpdateSimilarTrackInfoCompleted(TagInfoRequest request, List<Song> TagTracks)
    {
      if (request.Equals(_lastTagRequest))
      {
        GUIGraphicsContext.form.Invoke(new TagInfoCompletedDelegate(DoUpdateSimilarTrackInfo), new object[] {request, TagTracks});
      }
      else
      {
        Log.Warn("NowPlaying.OnUpdateSimilarTrackInfoCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    public void DoUpdateSimilarTrackInfo(TagInfoRequest request, List<Song> TagTracks)
    {
      GUIListItem item = null;
      {
        facadeSimilarTrackInfo.Clear();

        for (int i = 0; i < TagTracks.Count; i++)
        {
          item = new GUIListItem(TagTracks[i].ToShortString());
          item.Label = TagTracks[i].Artist + " - " + TagTracks[i].Title;
          //item.Label2 = " (" + GUILocalizeStrings.Get(931) + ": " + Convert.ToString(TagTracks[i].TimesPlayed) + ")";
          //item.Label = TagTracks[i].Artist;
          //item.Label2 = TagTracks[i].Title;

          item.MusicTag = TagTracks[i].ToMusicTag();

          facadeSimilarTrackInfo.Add(item);

          // display 3 items only
          if (facadeSimilarTrackInfo.Count == DISPLAY_LISTITEM_COUNT)
          {
            break;
          }
        }

        if (facadeSimilarTrackInfo.Count > 0)
        {
          if (LblBestSimilarTracks != null)
          {
            LblBestSimilarTracks.Label = GUILocalizeStrings.Get(33031) + TagTracks[0].Genre;
            LblBestSimilarTracks.Visible = true;
          }

          if (facadeAlbumInfo == null || facadeAlbumInfo.Count == 0)
          {
            // previously focus was set all the time
            // to maintain previous functionality set focus if
            // defauly skin control is present
            if (lblForceFocus != null)
            {
              GUIControl.FocusControl(GetID, ((int)ControlIDs.LIST_TAG_INFO));
            }
          }
        }
      }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// The VUMeter Timer has elapsed.
    /// Set the VUMeter Properties, currently only used by XFace
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnVUMterTimerTickEvent(object sender, ElapsedEventArgs e)
    {
      double dbLevelL = 0.0;
      double dbLevelR = 0.0;
      BassMusicPlayer.Player.RMS(out dbLevelL, out dbLevelR);

      // Raise the level with factor 1.5 so that the VUMeter shows more activity
      dbLevelL += Math.Abs(dbLevelL * 0.5);
      dbLevelR += Math.Abs(dbLevelR * 0.5);

      //Console.WriteLine("{0} {1}",(int)dbLevelL, (int)dbLevelR);

      string file = "VU1.png";
      if ((int)dbLevelL < -15)
      {
        file = "VU1.png";
      }
      else if ((int)dbLevelL < -10)
      {
        file = "VU2.png";
      }
      else if ((int)dbLevelL < -8)
      {
        file = "VU3.png";
      }
      else if ((int)dbLevelL < -7)
      {
        file = "VU4.png";
      }
      else if ((int)dbLevelL < -6)
      {
        file = "VU5.png";
      }
      else if ((int)dbLevelL < -5)
      {
        file = "VU6.png";
      }
      else if ((int)dbLevelL < -4)
      {
        file = "VU7.png";
      }
      else if ((int)dbLevelL < -3)
      {
        file = "VU8.png";
      }
      else if ((int)dbLevelL < -2)
      {
        file = "VU9.png";
      }
      else if ((int)dbLevelL < -1)
      {
        file = "VU10.png";
      }
      else if ((int)dbLevelL < 0)
      {
        file = "VU11.png";
      }
      else if ((int)dbLevelL < 1)
      {
        file = "VU12.png";
      }
      else if ((int)dbLevelL < 2)
      {
        file = "VU13.png";
      }
      else if ((int)dbLevelL < 3)
      {
        file = "VU14.png";
      }
      else
      {
        file = "VU15.png";
      }
      GUIPropertyManager.SetProperty("#VUMeterL", Path.Combine(VUMeterLeft.ImagePath, file));

      if ((int)dbLevelR < -15)
      {
        file = "VU1.png";
      }
      else if ((int)dbLevelR < -10)
      {
        file = "VU2.png";
      }
      else if ((int)dbLevelR < -8)
      {
        file = "VU3.png";
      }
      else if ((int)dbLevelR < -7)
      {
        file = "VU4.png";
      }
      else if ((int)dbLevelR < -6)
      {
        file = "VU5.png";
      }
      else if ((int)dbLevelR < -5)
      {
        file = "VU6.png";
      }
      else if ((int)dbLevelR < -4)
      {
        file = "VU7.png";
      }
      else if ((int)dbLevelR < -3)
      {
        file = "VU8.png";
      }
      else if ((int)dbLevelR < -2)
      {
        file = "VU9.png";
      }
      else if ((int)dbLevelR < -1)
      {
        file = "VU10.png";
      }
      else if ((int)dbLevelR < 0)
      {
        file = "VU11.png";
      }
      else if ((int)dbLevelR < 1)
      {
        file = "VU12.png";
      }
      else if ((int)dbLevelR < 2)
      {
        file = "VU13.png";
      }
      else if ((int)dbLevelR < 3)
      {
        file = "VU14.png";
      }
      else
      {
        file = "VU15.png";
      }
      GUIPropertyManager.SetProperty("#VUMeterR", Path.Combine(VUMeterRight.ImagePath, file));
    }

    private void UpdateImagePathContainer()
    {
      if (ImagePathContainer.Count <= 0)
      {
        try
        {
          ImgCoverArt.SetFileName(GUIGraphicsContext.GetThemedSkinFile(@"\media\missing_coverart.png"));
        }
        catch (Exception ex)
        {
          Log.Debug("GUIMusicPlayingNow: could not set default image - {0}", ex.Message);
        }
      }

      if (g_Player.Duration > 0 && ImagePathContainer.Count > 1)
      {
        //  ImageChangeTimer.Interval = (g_Player.Duration * 1000) / (ImagePathContainer.Count * 8); // change each cover 8x
        ImageChangeTimer.Interval = 15 * 1000; // change covers every 15 seconds
      }
      else
      {
        ImageChangeTimer.Interval = 3600 * 1000;
      }

      ImageChangeTimer.Stop();
      ImageChangeTimer.Start();
    }

    private bool AddImageToImagePathContainer(string newImage)
    {
      lock (_imageMutex)
      {
        string ImagePath = Convert.ToString(newImage);
        if (ImagePath.IndexOf(@"missing_coverart") > 0) // && (ImagePathContainer.Count > 0))
        {
          Log.Debug("GUIMusicPlayingNow: Found placeholder - not inserting image {0}", ImagePath);
          return false;
        }

        // Check if we should let the visualization window handle image flipping
        if (_usingBassEngine && _showVisualization)
        {
          VisualizationWindow vizWindow = BassMusicPlayer.Player.VisualizationWindow;

          if (vizWindow != null)
          {
            if (Util.Utils.FileExistsInCache(ImagePath))
            {
              try
              {
                Log.Debug("GUIMusicPlayingNow: adding image to visualization - {0}", ImagePath);
                vizWindow.AddImage(ImagePath);
                return true;
              }
              catch (Exception ex)
              {
                Log.Error("GUIMusicPlayingNow: error adding image ({0}) - {1}", ImagePath, ex.Message);
              }
            }
            else
            {
              Log.Warn("GUIMusicPlayingNow: could not use image - {0}", ImagePath);
              return false;
            }
          }
        }

        bool success = false;
        if (ImagePathContainer != null)
        {
          if (ImagePathContainer.Contains(ImagePath))
          {
            return false;
          }

          if (Util.Utils.FileExistsInCache(ImagePath))
          {
            try
            {
              Log.Debug("GUIMusicPlayingNow: adding image to container - {0}", ImagePath);
              ImagePathContainer.Add(ImagePath);
              success = true;
            }
            catch (Exception ex)
            {
              Log.Error("GUIMusicPlayingNow: error adding image ({0}) - {1}", ImagePath, ex.Message);
            }

            // display the first pic automatically
            if (ImagePathContainer.Count == 1)
            {
              FlipPictures();
            }
          }
        }

        return success;
      }
    }

    private void FlipPictures()
    {
      // Check if we should let the visualization window handle image flipping
      if (_usingBassEngine && _showVisualization)
      {
        return;
      }

      if (ImgCoverArt != null)
      {
        if (ImagePathContainer.Count > 1)
        {
          int currentImage = 0;
          // get the next image
          foreach (string image in ImagePathContainer)
          {
            currentImage++;
            if (ImgCoverArt.FileName == image)
            {
              break;
            }
          }

          if (currentImage < ImagePathContainer.Count)
          {
            ImgCoverArt.SetFileName(ImagePathContainer[currentImage]);
          }
          else
          {
            // start loop again
            ImgCoverArt.SetFileName(ImagePathContainer[0]);
          }
        }
        else
        {
          ImgCoverArt.SetFileName(ImagePathContainer[0]);
        }
      }
    }

    private void OnImageTimerTickEvent(object trash_, ElapsedEventArgs args_)
    {
      GUIGraphicsContext.form.Invoke(new TimerElapsedDelegate(FlipPictures));
    }

    private void AddInfoTrackToPlaylist(GUIListItem chosenTrack_)
    {
      try
      {
        MusicDatabase mdb = MusicDatabase.Instance;
        Song addSong = new Song();
        MusicTag tempTag = new MusicTag();
        tempTag = (MusicTag)chosenTrack_.MusicTag;

        if (mdb.GetSongByMusicTagInfo(tempTag.Artist, tempTag.Album, tempTag.Title, true, ref addSong))
        {
          if (AddSongToPlaylist(ref addSong))
          {
            Log.Info("GUIMusicPlayingNow: Song inserted: {0} - {1}", addSong.Artist, addSong.Title);
          }
        }
        else
        {
          Log.Info("GUIMusicPlayingNow: DB lookup for Artist: {0} Title: {1} unsuccessful", tempTag.Artist,
                   tempTag.Title);
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIMusicPlayingNow: DB lookup for Song failed - {0}", ex.Message);
      }
    }

    private void UpdateCurrentTrackRating(int RatingValue)
    {
      if (RatingValue < 0 || RatingValue > 5)
      {
        RatingValue = -1;
      }

      CurrentTrackTag.Rating = RatingValue;
      GUIPropertyManager.SetProperty("#Play.Current.Rating",
                                     (Convert.ToDecimal(2 * CurrentTrackTag.Rating + 1)).ToString());

      MusicDatabase dbs = MusicDatabase.Instance;
      string strFile = g_Player.CurrentFile;

      dbs.SetRating(strFile, RatingValue);
      Log.Info("GUIMusicPlayingNow: Set rating for song {0} to {1}", Path.GetFileName(g_Player.CurrentFile),
               Convert.ToString(RatingValue));
    }

    private static string CleanTagString(string tagField)
    {
      int dotIndex = 0;
      string outString = string.Empty;

      outString = Convert.ToString(tagField);
      outString = Util.Utils.MakeFileName(outString);

      dotIndex = outString.IndexOf(@"\");
      if (dotIndex > 0)
      {
        outString = outString.Remove(dotIndex);
      }

      return outString;
    }

    /// <summary>
    /// Updates the album info for the current track playing.
    /// The album info is fetched asynchronously by adding a request onto the request queue of the AudioScrobblerUtils
    /// class. The response will be received via callback by a delegate (OnUpdateAlbumInfoCompleted).
    /// </summary>
    private void UpdateAlbumInfo()
    {
      string CurrentArtist = CleanTagString(CurrentTrackTag.Artist);
      string CurrentAlbum = CleanTagString(CurrentTrackTag.Album);
      if (_doAlbumLookups)
      {
        if (CurrentTrackTag == null)
        {
          return;
        }
        if (CurrentTrackTag.Artist == string.Empty || CurrentTrackTag.Album == string.Empty)
        {
          Log.Warn("GUIMusicPlayingNow: current tag invalid for album info lookup. File: {0}", g_Player.CurrentFile);
          return;
        }
        AlbumInfoRequest request = new AlbumInfoRequest(
          CurrentArtist,
          CurrentAlbum,
          true,
          true,
          new AlbumInfoRequest.AlbumInfoRequestHandler(OnUpdateAlbumInfoCompleted)
          );
        _lastAlbumRequest = request;
        InfoScrobbler.AddRequest(request);
      }
    }

    /// <summary>
    /// Updates the artist info for the current track playing.
    /// The artist info is fetched asynchronously by adding a request onto the request queue of the AudioScrobblerUtils
    /// class. The response will be received via callback by a delegate (OnUpdateArtistInfoCompleted).
    /// </summary>
    private void UpdateArtistInfo()
    {
      string CurrentArtist = CleanTagString(CurrentTrackTag.Artist);
      if (_doArtistLookups)
      {
        if (CurrentTrackTag == null)
        {
          return;
        }
        if (CurrentTrackTag.Artist == string.Empty)
        {
          Log.Warn("GUIMusicPlayingNow: current tag invalid for artist info lookup. File: {0}", g_Player.CurrentFile);
          return;
        }
        ArtistInfoRequest request = new ArtistInfoRequest(
          CurrentArtist,
          new ArtistInfoRequest.ArtistInfoRequestHandler(OnUpdateArtistInfoCompleted)
          );
        _lastArtistRequest = request;
        InfoScrobbler.AddRequest(request);
      }
      else
      {
        // if internet lookups are disabled just attempt to load local thumb
        DoUpdateArtistInfo(null, null);
      }
    }

    /// <summary>
    /// Updates the "similar tags" info for the current track playing.
    /// The tag info is fetched asynchronously by adding a request onto the request queue of the AudioScrobblerUtils
    /// class. The response will be received via callback by a delegate (OnUpdateSimilarTrackInfoCompleted).
    /// </summary>
    private void UpdateSimilarTrackInfo()
    {
      string CurrentArtist = CleanTagString(CurrentTrackTag.Artist);
      string CurrentTrack = CleanTagString(CurrentTrackTag.Title);
      if (_doTrackTagLookups)
      {
        if (CurrentTrackTag == null)
        {
          return;
        }
        if (CurrentTrackTag.Artist == string.Empty || CurrentTrackTag.Title == string.Empty)
        {
          Log.Warn("GUIMusicPlayingNow: current tag invalid for tag info lookup. File: {0}", g_Player.CurrentFile);
          return;
        }

        if (LblBestSimilarTracks != null)
        {
          LblBestSimilarTracks.Visible = false;
        }

        TagInfoRequest request = new TagInfoRequest(
          CurrentArtist,
          CurrentTrack,
          true,
          false,
          true,
          new TagInfoRequest.TagInfoRequestHandler(OnUpdateSimilarTrackInfoCompleted)
          );
        _lastTagRequest = request;
        InfoScrobbler.AddRequest(request);
      }
    }

    private void UpdateTrackInfo()
    {
      if (PreviousTrackTag == null)
      {
        _trackChanged = true;
      }
      else if (CurrentTrackTag.Title != GUIPropertyManager.GetProperty("#Play.Current.Title"))
      {
        _trackChanged = true;
      }

      // only update if necessary
      if (_trackChanged)
      {
        if (CurrentTrackTag != null)
        {
          if (PreviousTrackTag != null)
          {
            if (CurrentTrackTag.Artist != PreviousTrackTag.Artist)
            {
              // Based on config settings this will attempt to lookup
              // artist thumbs from last.fm
              UpdateArtistInfo();
            }
            else
            {
              // if artist has not changed then no point attempting
              // to lookup artist thumbs as this would have been done
              // for first track for this artist so just jump to 
              // loading current track thumbs
              DoUpdateArtistInfo(null, null);
            }

            if (CurrentTrackTag.Album != PreviousTrackTag.Album || (facadeAlbumInfo != null && facadeAlbumInfo.Count < 1))
            {
              // album has changed or we don't have top tracks for album so try and update
              if (LblBestAlbumTracks != null)
              {
                LblBestAlbumTracks.Visible = false;
              }
              facadeAlbumInfo.Clear();
              ToggleTopTrackRatings(false, PopularityRating.unknown);
              UpdateAlbumInfo();
            }
          }
          else
          {
            // there is no previous track so update all info
            // similar tracks will be updated anyway for all track changes
            if (LblBestAlbumTracks != null)
            {
              LblBestAlbumTracks.Visible = false;
            }
            if (facadeAlbumInfo != null)
            {
              facadeAlbumInfo.Clear();
            }
            ToggleTopTrackRatings(false, PopularityRating.unknown);
            UpdateArtistInfo();
            UpdateAlbumInfo();
          }

          // track has changed so always look up similar tracks
          if (LblBestSimilarTracks != null)
          {
            LblBestSimilarTracks.Visible = false;
          }
          if (facadeSimilarTrackInfo != null)
          {
            facadeSimilarTrackInfo.Clear();
          }
          UpdateSimilarTrackInfo();
        }

        UpdateNextTrackInfo();
        // GUITextureManager.CleanupThumbs();

        _trackChanged = false;
      }
    }

    private void UpdateNextTrackInfo()
    {
      if (NextTrackTag != null)
      {
        // This should be removed in favor of skin conditions...
        if (LblUpNext != null)
        {
          LblUpNext.Visible = true;
        }
      }
      else
      {
        if (LblUpNext != null)
        {
          LblUpNext.Visible = false;
        }
      }
    }

    private void GetTrackTags()
    {
      if (CurrentTrackTag != null)
      {
        PreviousTrackTag = CurrentTrackTag;
      }

      bool isInternetStream = Util.Utils.IsAVStream(CurrentTrackFileName);
      if (isInternetStream && _usingBassEngine)
      {
        NextTrackTag = null;
        return;
      }

      PlayListItem currentItem = PlaylistPlayer.GetCurrentItem();
      PlayListItem nextItem = PlaylistPlayer.GetNextItem();
      if (currentItem != null)
      {
        CurrentTrackTag = (MusicTag)currentItem.MusicTag;
      }
      else
      {
        CurrentTrackTag = null;
      }

      if (nextItem != null)
      {
        NextTrackTag = (MusicTag)nextItem.MusicTag;
      }
      else
      {
        NextTrackTag = null;
      }

    }

    private void OnSongInserted()
    {
      _trackChanged = true;
      NextTrackFileName = PlaylistPlayer.GetNext();
      GetTrackTags();
      UpdateTrackInfo();
    }

    /// <summary>
    /// Add or enque a song to the current playlist - call OnSongInserted() after this!!!
    /// </summary>
    /// <param name="song">the song to add</param>
    /// <param name="enqueueNext_">if true the songs is inserted after the current track, otherwise added to the end of the list</param>
    /// <returns>if the action was successful</returns>
    private bool AddSongToPlaylist(ref Song song)
    {
      PlayList playlist;
      if (PlaylistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_TEMP)
      {
        playlist = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC_TEMP);
      }
      else
      {
        playlist = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      }
      
      if (playlist == null)
      {
        return false;
      }

      //add to playlist
      PlayListItem playlistItem = new PlayListItem();
      playlistItem.Type = PlayListItem.PlayListItemType.Audio;
      StringBuilder sb = new StringBuilder();

      playlistItem.FileName = song.FileName;
      sb.Append(song.Track);
      sb.Append(". ");
      sb.Append(song.Artist);
      sb.Append(" - ");
      sb.Append(song.Title);
      playlistItem.Description = sb.ToString();
      playlistItem.Duration = song.Duration;

      playlistItem.MusicTag = song.ToMusicTag();
      playlist.Add(playlistItem);

      OnSongInserted();
      return true;
    }

    private void LoadAndStartPlayList()
    {
      PlaylistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;

      if (g_Player.CurrentFile == "")
      {
        PlayList playList = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

        if (playList != null && playList.Count > 0)
        {
          this.CurrentTrackFileName = playList[0].FileName;
          PlaylistPlayer.Play(0);
          OnPlayBackStarted(g_Player.MediaType.Music, g_Player.CurrentFile);
        }
      }
    }

    private void SetVisualizationWindow()
    {
      if (!ControlsInitialized || !_showVisualization || !_usingBassEngine)
      {
        return;
      }

      // the solution below doesn't rotate e.g. artist pics.

      //if (ImgCoverArt != null)
      //{
      //  GUIGraphicsContext.VideoWindow = new Rectangle(ImgCoverArt.XPosition, ImgCoverArt.YPosition, ImgCoverArt.Width, ImgCoverArt.Height);
      //  return;
      //}

      VisualizationWindow vizWindow = BassMusicPlayer.Player.VisualizationWindow;

      if (vizWindow != null)
      {
        vizWindow.Visible = false;

        int width = ImgCoverArt.RenderWidth;
        int height = ImgCoverArt.RenderHeight;

        //int width = ImgCoverArt.Width;
        //int height = ImgCoverArt.Height;

        Size vizSize = new Size(width, height);
        float vizX = (float)ImgCoverArt.Location.X;
        float vizY = (float)ImgCoverArt.Location.Y;
        GUIGraphicsContext.Correct(ref vizX, ref vizY);
        Point vizLoc = new Point((int)vizX, (int)vizY);
        vizWindow.Size = vizSize;
        vizWindow.Location = vizLoc;
        vizWindow.Visible = true;

        GUIGraphicsContext.VideoWindow = new Rectangle(vizLoc, vizSize);
      }
    }

    private void ClearVisualizationImages()
    {
      if (!_usingBassEngine || !_showVisualization)
      {
        return;
      }

      VisualizationWindow vizWindow = BassMusicPlayer.Player.VisualizationWindow;

      if (vizWindow != null)
      {
        vizWindow.ClearImages();
      }
    }

    private void ToggleTopTrackRatings(bool showStars_, PopularityRating starType_)
    {
      try
      {
        if (showStars_)
        {
          switch (starType_)
          {
            case PopularityRating.unknown:
              if (ImgListUnknownTrack1 != null && ImgListUnknownTrack1 != null && ImgListUnknownTrack1 != null)
              {
                ImgListUnknownTrack1.Visible = true;
                ImgListUnknownTrack2.Visible = true;
                ImgListUnknownTrack3.Visible = true;
              }
              break;
            case PopularityRating.existent:
              if (ImgListExistingTrack1 != null && ImgListExistingTrack2 != null && ImgListExistingTrack3 != null)
              {
                ImgListExistingTrack1.Visible = true;
                ImgListExistingTrack2.Visible = true;
                ImgListExistingTrack3.Visible = true;
              }
              break;
            case PopularityRating.known:
              if (ImgListKnownTrack1 != null && ImgListKnownTrack2 != null && ImgListKnownTrack3 != null)
              {
                ImgListKnownTrack1.Visible = true;
                ImgListKnownTrack2.Visible = true;
                ImgListKnownTrack3.Visible = true;
              }
              break;
            case PopularityRating.famous:
              if (ImgListFamousTrack1 != null && ImgListFamousTrack2 != null && ImgListFamousTrack3 != null)
              {
                ImgListFamousTrack1.Visible = true;
                ImgListFamousTrack2.Visible = true;
                ImgListFamousTrack3.Visible = true;
              }
              break;
          }
        }
        else // hide ALL stars
        {
          ImgListUnknownTrack1.Visible = false;
          ImgListUnknownTrack2.Visible = false;
          ImgListUnknownTrack3.Visible = false;
          ImgListExistingTrack1.Visible = false;
          ImgListExistingTrack2.Visible = false;
          ImgListExistingTrack3.Visible = false;
          ImgListKnownTrack1.Visible = false;
          ImgListKnownTrack2.Visible = false;
          ImgListKnownTrack3.Visible = false;
          ImgListFamousTrack1.Visible = false;
          ImgListFamousTrack2.Visible = false;
          ImgListFamousTrack3.Visible = false;
        }
      }
      catch (Exception ex)
      {
        Log.Warn("GUIMusicPlayingNow: Could not toggle rating stars - {0}", ex.Message);
      }
    }

    private void DoLastFMLove()
    {
      if (_audioscrobblerEnabled)
      {
        AudioscrobblerBase.DoLoveTrackNow();
        GUIDialogNotifyLastFM dlgNotifyLastFM =
          (GUIDialogNotifyLastFM)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_LASTFM);
        dlgNotifyLastFM.SetHeading(GUILocalizeStrings.Get(34000)); // last.FM
        string dlgText = GUILocalizeStrings.Get(34010) + " : " + AudioscrobblerBase.CurrentPlayingSong.Title;
        dlgNotifyLastFM.SetText(dlgText);
        dlgNotifyLastFM.TimeOut = 2;
        dlgNotifyLastFM.DoModal(GetID);
      }
    }

    private void DoLastFMBan()
    {
      if (_audioscrobblerEnabled)
      {
        AudioscrobblerBase.DoBanTrackNow();
        GUIDialogNotifyLastFM dlgNotifyLastFM =
          (GUIDialogNotifyLastFM)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_LASTFM);
        dlgNotifyLastFM.SetHeading(GUILocalizeStrings.Get(34000)); // last.FM
        string dlgText = GUILocalizeStrings.Get(34011) + " : " + AudioscrobblerBase.CurrentPlayingSong.Title;
        dlgNotifyLastFM.SetText(dlgText);
        dlgNotifyLastFM.TimeOut = 2;
        dlgNotifyLastFM.DoModal(GetID);
      }
    }

    #endregion
  }
}