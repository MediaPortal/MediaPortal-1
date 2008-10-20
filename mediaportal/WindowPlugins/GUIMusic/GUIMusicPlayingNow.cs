#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#region usings

using System;
using System.Collections;
using System.Collections.Generic;
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
using MediaPortal.Configuration;

#endregion

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Displays more information about the current running track
  /// Window added by Steve5 (why not using the MusicBaseWindow?)
  /// Internet lookups added by rtv
  /// </summary>
  public class GUIMusicPlayingNow : GUIWindow
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

      BEST_TAG_TRACKS = 28,
      BEST_ALBUM_TRACKS = 29,

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

    [SkinControlAttribute((int)ControlIDs.LBL_CAPTION)]
    protected GUILabelControl LblCaption = null;
    [SkinControlAttribute((int)ControlIDs.IMG_COVERART)]
    protected GUIImage ImgCoverArt = null;
    [SkinControlAttribute((int)ControlIDs.PROG_TRACK)]
    protected GUIProgressControl ProgTrack = null;
    [SkinControlAttribute((int)ControlIDs.IMG_TRACK_PROGRESS_BG)]
    protected GUIImage ImgTrackProgressBkGrnd = null;
    [SkinControlAttribute((int)ControlIDs.LBL_UP_NEXT)]
    protected GUILabelControl LblUpNext = null;
    [SkinControlAttribute((int)ControlIDs.LIST_TAG_INFO)]
    protected GUIListControl facadeTagInfo = null;
    [SkinControlAttribute((int)ControlIDs.LIST_ALBUM_INFO)]
    protected GUIListControl facadeAlbumInfo = null;
    [SkinControlAttribute((int)ControlIDs.BEST_ALBUM_TRACKS)]
    protected GUIFadeLabel LblBestAlbumTracks = null;
    [SkinControlAttribute((int)ControlIDs.BEST_TAG_TRACKS)]
    protected GUIFadeLabel LblBestTagTracks = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_UNKNOWN_TRACK1)]
    protected GUIImageList ImgListUnknownTrack1 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_UNKNOWN_TRACK2)]
    protected GUIImageList ImgListUnknownTrack2 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_UNKNOWN_TRACK3)]
    protected GUIImageList ImgListUnknownTrack3 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_EXISTENT_TRACK1)]
    protected GUIImageList ImgListExistingTrack1 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_EXISTENT_TRACK2)]
    protected GUIImageList ImgListExistingTrack2 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_EXISTENT_TRACK3)]
    protected GUIImageList ImgListExistingTrack3 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_KNOWN_TRACK1)]
    protected GUIImageList ImgListKnownTrack1 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_KNOWN_TRACK2)]
    protected GUIImageList ImgListKnownTrack2 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_KNOWN_TRACK3)]
    protected GUIImageList ImgListKnownTrack3 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_FAMOUS_TRACK1)]
    protected GUIImageList ImgListFamousTrack1 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_FAMOUS_TRACK2)]
    protected GUIImageList ImgListFamousTrack2 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_FAMOUS_TRACK3)]
    protected GUIImageList ImgListFamousTrack3 = null;

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
    private DateTime LastUpdateTime = DateTime.Now;
    private TimeSpan UpdateInterval = new TimeSpan(0, 0, 1);
    private GUIMusicBaseWindow _MusicWindow = null;
    private AudioscrobblerUtils InfoScrobbler = null;
    private ScrobblerUtilsRequest _lastAlbumRequest;
    private ScrobblerUtilsRequest _lastArtistRequest;
    private ScrobblerUtilsRequest _lastTagRequest;
    private System.Timers.Timer ImageChangeTimer = null;
    private List<String> ImagePathContainer = null;
    private bool UseID3 = false;
    private bool _trackChanged = true;
    private bool _doArtistLookups = true;
    private bool _doAlbumLookups = true;
    private bool _doTrackTagLookups = true;
    private bool _usingBassEngine = false;
    private bool _showVisualization = false;
    private bool _enqueueDefault = true;
    private bool _preventMonitorPowerDown = false;
    private object _imageMutex = null;

    #endregion

    #region Constructor

    public GUIMusicPlayingNow()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW;
      PlaylistPlayer = PlayListPlayer.SingletonPlayer;
      ImagePathContainer = new List<string>();
      _imageMutex = new object();

      g_Player.PlayBackStarted += new g_Player.StartedHandler(g_Player_PlayBackStarted);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(g_Player_PlayBackEnded);

      // GUIWindowManager.Receivers += new SendMessageHandler(this.OnThreadMessage);

      LoadSettings();
    }

    #endregion

    #region Serialisation

    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string VizName = "None";
        bool ShowViz = false;
        VizName = xmlreader.GetValueAsString("musicvisualization", "name", "None");
        ShowViz = xmlreader.GetValueAsBool("musicmisc", "showVisInNowPlaying", false);

        UseID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);

        _doArtistLookups = xmlreader.GetValueAsBool("musicmisc", "fetchlastfmthumbs", true);
        _doAlbumLookups = xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtopalbums", true);
        _doTrackTagLookups = xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtracktags", true);
        _enqueueDefault = xmlreader.GetValueAsBool("musicmisc", "enqueuenext", true);
        _preventMonitorPowerDown = xmlreader.GetValueAsBool("musicmisc", "preventmonitorpowerdown", false);

        if (ShowViz && VizName != "None")
          _showVisualization = true;
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

    void g_Player_PlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (!ControlsInitialized || type != g_Player.MediaType.Music)
        return;

      // Allow Monitor to turn off again
      if (_preventMonitorPowerDown)
        Util.Win32API.AllowMonitorPowerdown();

      if (GUIWindowManager.ActiveWindow == GetID)
      {
        Log.Debug("GUIMusicPlayingNow: g_Player_PlayBackEnded for {0}", filename);

        //PlayList currentPlaylist = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
        //if (currentPlaylist.AllPlayed())
        if (!g_Player.Playing && NextTrackTag == null)
        {
          //Log.Debug("GUIMusicPlayingNow: All playlist items played - returning to previous window");
          Action action = new Action();
          action.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
          GUIGraphicsContext.OnAction(action);
        }
      }
    }

    void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (!ControlsInitialized || type != g_Player.MediaType.Music)
        return;

      // Allow Monitor to turn off again
      if (_preventMonitorPowerDown)
        Util.Win32API.AllowMonitorPowerdown();

      if (GUIWindowManager.ActiveWindow == GetID)
      {
        Log.Debug("GUIMusicPlayingNow: g_Player_PlayBackStopped for {0} - stoptime: {1}", filename, stoptime);
        Action action = new Action();
        action.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
        GUIGraphicsContext.OnAction(action);
      }
    }

    void g_Player_PlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (!ControlsInitialized || type != g_Player.MediaType.Music)
        return;

      Log.Debug("GUIMusicPlayingNow: g_Player_PlayBackStarted for {0}", filename);
      // Remove pending requests from the request queue
      if (_lastAlbumRequest != null)
        InfoScrobbler.RemoveRequest(_lastAlbumRequest);
      if (_lastArtistRequest != null)
        InfoScrobbler.RemoveRequest(_lastArtistRequest);
      if (_lastTagRequest != null)
        InfoScrobbler.RemoveRequest(_lastTagRequest);


      ImagePathContainer.Clear();
      ClearVisualizationImages();

      CurrentTrackFileName = filename;
      NextTrackFileName = PlaylistPlayer.GetNext();
      GetTrackTags();

      CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);

      if (CurrentThumbFileName.Length < 1)   // no LOCAL Thumb found because user has bad settings -> check if there is a folder.jpg in the share
      {
        CurrentThumbFileName = Util.Utils.GetFolderThumb(CurrentTrackFileName);
        if (!System.IO.File.Exists(CurrentThumbFileName))
          CurrentThumbFileName = string.Empty;
      }

      if (CurrentThumbFileName.Length > 0)
      {
        // let us test if there is a larger cover art image
        string strLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(CurrentThumbFileName);
        if (System.IO.File.Exists(strLarge))
          CurrentThumbFileName = strLarge;
        AddImageToImagePathContainer(CurrentThumbFileName);
      }

      UpdateImagePathContainer();
      UpdateTrackInfo();
      UpdateTrackPosition();

      if (GUIWindowManager.ActiveWindow == GetID)
      {
        SetVisualizationWindow();
      }
    }

    #endregion

    #region Overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\MyMusicPlayingNow.xml");
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
          facadeTagInfo.Clear();
          UpdateTagInfo();
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
                    AddInfoTrackToPlaylist(facadeAlbumInfo.SelectedListItem, true);
                  else
                  {
                    Log.Info("GUIMusicPlayingNow: Could not add {0} from top album tracks because it was not found in your collection!");
                    GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                    if (dlg == null)
                      break;
                    //dlg. Reset();
                    dlg.SetHeading(502);     // Unable to complete action
                    dlg.SetLine(2, 33032);   // This album's top track could not
                    dlg.SetLine(3, 33033);   // be found in your collection!

                    dlg.DoModal(GetID);
                  }
                }
              }
            }
            if (facadeTagInfo != null)
            {
              if (message.SenderControlId == (int)ControlIDs.LIST_TAG_INFO) // listbox
              {
                if ((int)Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
                {
                  AddInfoTrackToPlaylist(facadeTagInfo.SelectedListItem, true);
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

      facadeAlbumInfo.Clear();
      facadeTagInfo.Clear();
      ImagePathContainer.Clear();

      _trackChanged = true;

      ToggleTopTrackRatings(false, PopularityRating.unknown);

      GUIPropertyManager.SetProperty("#currentmodule", String.Format("{0}/{1}", GUILocalizeStrings.Get(100005), GUILocalizeStrings.Get(4540)));
      LblUpNext.Label = GUILocalizeStrings.Get(4541);

      if (LblUpNext.Label.Length == 0)
        LblUpNext.Label = "Playing next";

      if (GUIPropertyManager.GetProperty("#Play.Next.Title") == string.Empty)
        LblUpNext.Visible = false;

      if (LblBestAlbumTracks != null)
        LblBestAlbumTracks.Visible = false;
      if (LblBestTagTracks != null)
        LblBestTagTracks.Visible = false;
      ControlsInitialized = true;

      InfoScrobbler = AudioscrobblerUtils.Instance;

      if (ImageChangeTimer == null)
      {
        ImageChangeTimer = new System.Timers.Timer();
        ImageChangeTimer.Interval = 3600 * 1000;
        ImageChangeTimer.Elapsed += new ElapsedEventHandler(OnImageTimerTickEvent);
        ImageChangeTimer.Start();
      }

      UpdateImagePathContainer();

      if (g_Player.Playing)
      {
        g_Player_PlayBackStarted(g_Player.MediaType.Music, g_Player.CurrentFile);

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

      // Prevent Monitor from being turned off, while in Now Playing
      if (_preventMonitorPowerDown)
        Util.Win32API.PreventMonitorPowerdown();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      // Reset the Monitor Power Down State
      if (_preventMonitorPowerDown)
        Util.Win32API.AllowMonitorPowerdown();

      // Remove pending requests from the request queue
      InfoScrobbler.RemoveRequest(_lastAlbumRequest);
      InfoScrobbler.RemoveRequest(_lastArtistRequest);
      InfoScrobbler.RemoveRequest(_lastTagRequest);

      ImageChangeTimer.Stop();

      if (ImgCoverArt != null)
        ImgCoverArt.FreeResources();

      GC.Collect();
      ControlsInitialized = false;

      // Make sure we clear any images we added so we revert back the coverart image
      ClearVisualizationImages();

      base.OnPageDestroy(new_windowId);
    }

    public override void Render(float timePassed)
    {
      base.Render(timePassed);

      if (!g_Player.Playing || !g_Player.IsMusic)
        return;

      TimeSpan ts = DateTime.Now - LastUpdateTime;

      if (ts >= UpdateInterval)
      {
        LastUpdateTime = DateTime.Now;
        UpdateTrackInfo();
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      MusicDatabase dbs;

      if (dlg == null)
        return;

      dlg.Reset();
      dlg.SetHeading(498);                // Menu

      dlg.AddLocalizedString(930);        //Add to favorites

      if (CurrentTrackTag.Album != null)
        dlg.AddLocalizedString(33041);

      //dlg.AddLocalizedString(928);        // Find Coverart
      // dlg.AddLocalizedString(4521);       // Show Album Info

      if (IsCdTrack(CurrentTrackFileName))
        dlg.AddLocalizedString(4554);   // Lookup CD info

      if (CurrentTrackTag != null)
        dlg.AddLocalizedString(33040);  // copy IRC spam

      dlg.DoModal(GetID);

      // The ImgCoverArt visibility gets restored when a context menu is popped up.
      // So we need to reset the visualization window visibility is restored after
      // the context menu is dismissed
      SetVisualizationWindow();

      if (dlg.SelectedId == -1)
        return;

      switch (dlg.SelectedId)
      {
        case 928:       // Find Coverart
          if (_MusicWindow != null)
          {
            string albumFolderPath = System.IO.Path.GetDirectoryName(CurrentTrackFileName);

            _MusicWindow.FindCoverArt(false, CurrentTrackTag.Artist, CurrentTrackTag.Album, albumFolderPath, CurrentTrackTag, -1);
            CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);

            if (CurrentThumbFileName.Length > 0)
            {
              // let us test if there is a larger cover art image
              string strLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(CurrentThumbFileName);
              if (System.IO.File.Exists(strLarge))
                CurrentThumbFileName = strLarge;
              AddImageToImagePathContainer(CurrentThumbFileName);

              if (_usingBassEngine)
                BassMusicPlayer.Player.VisualizationWindow.CoverArtImagePath = CurrentThumbFileName;

              UpdateImagePathContainer();
            }
          }
          break;

        case 4521:      // Show Album Info
          if (_MusicWindow != null)
          {
            string albumFolderPath = System.IO.Path.GetDirectoryName(CurrentTrackFileName);
            if (_MusicWindow != null)
              _MusicWindow.ShowAlbumInfo(GetID, false, CurrentTrackTag.Artist, CurrentTrackTag.Album, albumFolderPath, CurrentTrackTag);
          }
          break;

        case 4554:      // Lookup CD info
          MusicTag tag = new MusicTag();
          GetCDInfoFromFreeDB(CurrentTrackFileName, tag);
          GetTrackTags();
          break;

        case 33040:    // IRC spam
          try
          {
            if (CurrentTrackTag != null)
            {
              string tmpTrack = CurrentTrackTag.Track > 0 ? (Convert.ToString(CurrentTrackTag.Track) + ". ") : string.Empty;
              Clipboard.SetDataObject(@"/me is listening to " + CurrentTrackTag.Artist + " [" + CurrentTrackTag.Album + "] - " + tmpTrack + CurrentTrackTag.Title, true);
            }
          }
          catch (Exception ex)
          {
            Log.Error("GUIMusicPlayingNow: could not copy song spam to clipboard - {0}", ex.Message);
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
              return;
            if (currentSong.Id < 0)
              return;
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
                    AddSongToPlaylist(ref song, _enqueueDefault);
                  }
                }
                OnSongInserted();
              }
            }
          }
          catch (Exception ex)
          {
            Log.Error("GUIMusicPlayingNow: error while adding album tracks for {0} - {1}", CurrentTrackTag.Album, ex.Message);
          }
          break;
      }
    }

    #endregion

    #region Public methods

    public void OnUpdateAlbumInfoCompleted(AlbumInfoRequest request, List<Song> AlbumTracks)
    {
      if (request.Equals(_lastAlbumRequest))
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
            ratingBase = AlbumSongRating / Convert.ToSingle(AlbumTracks[0].TimesPlayed);
          //else
          //  ratingBase = 0.01f;

          // avoid division by zero
          AlbumSongRating = AlbumSongRating > 0 ? AlbumSongRating : 1;

          for (int i = 0; i < AlbumTracks.Count; i++)
          {
            float rating = 0;

            if (i == 0)
              AlbumTracks[i].Rating = 11;
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
              break;
          }
        }
        if (facadeAlbumInfo.Count > 0)
        {
          int popularity = AlbumTracks[0].TimesPlayed;

          // only display stars if list is filled
          if (facadeAlbumInfo.Count == DISPLAY_LISTITEM_COUNT)
          {
            if (popularity > 40000)
              ToggleTopTrackRatings(true, PopularityRating.famous);
            else
              if (popularity > 10000)
                ToggleTopTrackRatings(true, PopularityRating.known);
              else
                if (popularity > 2500)
                  ToggleTopTrackRatings(true, PopularityRating.existent);
                else
                  ToggleTopTrackRatings(true, PopularityRating.unknown);
          }

          CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);
          if (CurrentThumbFileName.Length > 0)
          {
            // let us test if there is a larger cover art image
            string strLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(CurrentThumbFileName);
            if (System.IO.File.Exists(strLarge))
              CurrentThumbFileName = strLarge;

            AddImageToImagePathContainer(CurrentThumbFileName);
          }
          if (LblBestAlbumTracks != null)
            LblBestAlbumTracks.Visible = true;

          UpdateImagePathContainer();

          GUIControl.FocusControl(GetID, ((int)ControlIDs.LIST_ALBUM_INFO));
        }
      }
      else
      {
        Log.Warn("GUIMusicPlayingNow: OnUpdateAlbumInfoCompleted: unexpected responsetype for request: {0}", request.Type);
      }
    }

    public void OnUpdateArtistInfoCompleted(ArtistInfoRequest request, Song song)
    {
      if (request.Equals(_lastArtistRequest))
      {
        CurrentThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, Util.Utils.MakeFileName(CurrentTrackTag.Artist));
        if (CurrentThumbFileName.Length > 0)
        {
          // let us test if there is a larger cover art image
          string strLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(CurrentThumbFileName);
          if (System.IO.File.Exists(strLarge))
            CurrentThumbFileName = strLarge;

          AddImageToImagePathContainer(CurrentThumbFileName);
          UpdateImagePathContainer();
        }
      }
      else
      {
        Log.Warn("NowPlaying.OnUpdateArtistInfoCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    public void OnUpdateTagInfoCompleted(TagInfoRequest request, List<Song> TagTracks)
    {
      if (request.Equals(_lastTagRequest))
      {
        GUIListItem item = null;
        // lock (_infoUpdateMutex)
        {
          facadeTagInfo.Clear();

          for (int i = 0; i < TagTracks.Count; i++)
          {
            item = new GUIListItem(TagTracks[i].ToShortString());
            item.Label = TagTracks[i].Artist + " - " + TagTracks[i].Title;
            //item.Label2 = " (" + GUILocalizeStrings.Get(931) + ": " + Convert.ToString(TagTracks[i].TimesPlayed) + ")";
            //item.Label = TagTracks[i].Artist;
            //item.Label2 = TagTracks[i].Title;

            item.MusicTag = TagTracks[i].ToMusicTag();

            facadeTagInfo.Add(item);

            // display 3 items only
            if (facadeTagInfo.Count == DISPLAY_LISTITEM_COUNT)
              break;
          }

          if (facadeTagInfo.Count > 0)
          {
            if (LblBestTagTracks != null)
            {
              LblBestTagTracks.Label = GUILocalizeStrings.Get(33031) + TagTracks[0].Genre;
              LblBestTagTracks.Visible = true;
            }
            if (facadeAlbumInfo == null || facadeAlbumInfo.Count == 0)
              GUIControl.FocusControl(GetID, ((int)ControlIDs.LIST_TAG_INFO));
          }
        }
      }
      else
      {
        Log.Warn("NowPlaying.OnUpdateTagInfoCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    #endregion

    #region Private methods

    private void UpdateImagePathContainer()
    {
      if (ImagePathContainer.Count <= 0)
      {
        try
        {
          ImgCoverArt.SetFileName(GUIGraphicsContext.Skin + @"\media\missing_coverart.png");
        }
        catch (Exception ex)
        {
          Log.Debug("GUIMusicPlayingNow: could not set default image - {0}", ex.Message);
        }
      }

      if (g_Player.Duration > 0 && ImagePathContainer.Count > 1)
        //  ImageChangeTimer.Interval = (g_Player.Duration * 1000) / (ImagePathContainer.Count * 8); // change each cover 8x
        ImageChangeTimer.Interval = 15 * 1000;  // change covers every 15 seconds
      else
        ImageChangeTimer.Interval = 3600 * 1000;

      ImageChangeTimer.Stop();
      ImageChangeTimer.Start();
    }

    private bool AddImageToImagePathContainer(string newImage)
    {
      lock (_imageMutex)
      {
        string ImagePath = Convert.ToString(newImage);
        if (ImagePath.IndexOf(@"missing_coverart") > 0)// && (ImagePathContainer.Count > 0))
        {
          Log.Debug("GUIMusicPlayingNow: Found placeholder - not inserting image {0}", ImagePath);
          return false;
        }

        // Check if we should let the visualization window handle image flipping
        if (_usingBassEngine && _showVisualization)
        {
          Visualization.VisualizationWindow vizWindow = BassMusicPlayer.Player.VisualizationWindow;

          if (vizWindow != null)
          {
            if (System.IO.File.Exists(ImagePath))
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
            return false;

          if (System.IO.File.Exists(ImagePath))
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
              FlipPictures();
          }
        }

        return success;
      }
    }

    private void FlipPictures()
    {
      // Check if we should let the visualization window handle image flipping
      if (_usingBassEngine && _showVisualization)
        return;

      if (ImgCoverArt != null)
      {
        if (ImagePathContainer.Count > 0)
        {
          if (ImagePathContainer.Count > 1)
          {
            int currentImage = 0;
            // get the next image
            foreach (string image in ImagePathContainer)
            {
              currentImage++;
              if (ImgCoverArt.FileName == image)
                break;
            }
            if (currentImage < ImagePathContainer.Count)
              ImgCoverArt.SetFileName(ImagePathContainer[currentImage]);
            else
              // start loop again
              ImgCoverArt.SetFileName(ImagePathContainer[0]);
          }
          else
            ImgCoverArt.SetFileName(ImagePathContainer[0]);
        }
        ImgCoverArt.FreeResources();
        ImgCoverArt.AllocResources();
      }
    }

    private void OnImageTimerTickEvent(object trash_, ElapsedEventArgs args_)
    {
      FlipPictures();
    }

    private void AddInfoTrackToPlaylist(GUIListItem chosenTrack_, bool enqueueNext_)
    {
      try
      {
        PlayList currentPlaylist = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
        if (currentPlaylist.Count > 0)
        {
          MusicDatabase mdb = MusicDatabase.Instance;
          Song addSong = new Song();
          MusicTag tempTag = new MusicTag();
          tempTag = (MusicTag)chosenTrack_.MusicTag;

          if (mdb.GetSongByMusicTagInfo(tempTag.Artist, tempTag.Album, tempTag.Title, true, ref addSong))
          {
            if (AddSongToPlaylist(ref addSong, _enqueueDefault))
              Log.Info("GUIMusicPlayingNow: Song inserted: {0} - {1}", addSong.Artist, addSong.Title);
          }
          else
            Log.Info("GUIMusicPlayingNow: DB lookup for Artist: {0} Title: {1} unsuccessful", tempTag.Artist, tempTag.Title);
        }
        else
        // not using playlists here...
        {
          Log.Warn("GUIMusicPlayingNow: You have to use a playlist to add songs to it - Press PLAY on a song or folder!");
          GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
          if (dlg == null)
            return;
          //dlg. Reset();
          dlg.SetHeading(502);     // Unable to complete action
          dlg.SetLine(2, 33008);   // There is no playlist active 
          dlg.SetLine(3, 33009);   // to insert or add the track!

          dlg.DoModal(GetID);
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
        RatingValue = -1;

      CurrentTrackTag.Rating = RatingValue;
      GUIPropertyManager.SetProperty("#Play.Current.Rating", (Convert.ToDecimal(2 * CurrentTrackTag.Rating + 1)).ToString());

      MusicDatabase dbs = MusicDatabase.Instance;
      string strFile = g_Player.CurrentFile;

      dbs.SetRating(strFile, RatingValue);
      Log.Info("GUIMusicPlayingNow: Set rating for song {0} to {1}", System.IO.Path.GetFileName(g_Player.CurrentFile), Convert.ToString(RatingValue));
    }

    private string CleanTagString(string tagField)
    {
      int dotIndex = 0;
      string outString = string.Empty;

      outString = Convert.ToString(tagField);
      outString = Util.Utils.MakeFileName(outString);

      dotIndex = outString.IndexOf(@"\");
      if (dotIndex > 0)
        outString = outString.Remove(dotIndex);

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
          return;
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
          return;
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
    }

    /// <summary>
    /// Updates the "similar tags" info for the current track playing.
    /// The tag info is fetched asynchronously by adding a request onto the request queue of the AudioScrobblerUtils
    /// class. The response will be received via callback by a delegate (OnUpdateTagInfoCompleted).
    /// </summary>
    private void UpdateTagInfo()
    {
      string CurrentArtist = CleanTagString(CurrentTrackTag.Artist);
      string CurrentTrack = CleanTagString(CurrentTrackTag.Title);
      if (_doTrackTagLookups)
      {
        if (CurrentTrackTag == null)
          return;
        if (CurrentTrackTag.Artist == string.Empty || CurrentTrackTag.Title == string.Empty)
        {
          Log.Warn("GUIMusicPlayingNow: current tag invalid for tag info lookup. File: {0}", g_Player.CurrentFile);
          return;
        }

        if (LblBestTagTracks != null)
          LblBestTagTracks.Visible = false;

        TagInfoRequest request = new TagInfoRequest(
                                        CurrentArtist,
                                        CurrentTrack,
                                        true,
                                        false,
                                        true,
                                        new TagInfoRequest.TagInfoRequestHandler(OnUpdateTagInfoCompleted)
                                        );
        _lastTagRequest = request;
        InfoScrobbler.AddRequest(request);
      }
    }

    private void UpdateTrackInfo()
    {
      UpdateTrackPosition();

      if (CurrentTrackTag == null)
        _trackChanged = true;
      else
        if (CurrentTrackTag.Title != GUIPropertyManager.GetProperty("#Play.Current.Title"))
          _trackChanged = true;

      // only update if necessary
      if (_trackChanged)
      {
        if (CurrentTrackTag != null)
        {
          bool InfoNeeded = false;

          if (PreviousTrackTag != null)
          {
            if (CurrentTrackTag.Artist != PreviousTrackTag.Artist)
              UpdateArtistInfo();
            else
            {
              CurrentThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, Util.Utils.MakeFileName(CurrentTrackTag.Artist));
              if (CurrentThumbFileName.Length > 0)
              {
                // let us test if there is a larger cover art image
                string strLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(CurrentThumbFileName);
                if (System.IO.File.Exists(strLarge))
                  CurrentThumbFileName = strLarge;

                AddImageToImagePathContainer(CurrentThumbFileName);
                UpdateImagePathContainer();
              }
            }

            if (CurrentTrackTag.Album != PreviousTrackTag.Album || facadeTagInfo.Count < 1)
            {
              InfoNeeded = true;
              if (LblBestAlbumTracks != null)
                LblBestAlbumTracks.Visible = false;
              if (LblBestTagTracks != null)
                LblBestTagTracks.Visible = false;
              facadeAlbumInfo.Clear();
              facadeTagInfo.Clear();
              ToggleTopTrackRatings(false, PopularityRating.unknown);
            }
          }
          else
          {
            UpdateArtistInfo();
            InfoNeeded = true;
          }

          string strTrack = String.Format("{0} {1}", GUILocalizeStrings.Get(435), CurrentTrackTag.Track);   //	"Track"
          if (CurrentTrackTag.Track <= 0)
            strTrack = string.Empty;

          string strYear = String.Format("{0} {1}", GUILocalizeStrings.Get(436), CurrentTrackTag.Year); //	"Year: "
          if (CurrentTrackTag.Year <= 1900)
            strYear = string.Empty;

          GUIPropertyManager.SetProperty("#Play.Current.Title", CurrentTrackTag.Title);
          GUIPropertyManager.SetProperty("#Play.Current.Track", strTrack);
          GUIPropertyManager.SetProperty("#Play.Current.Album", CurrentTrackTag.Album);
          GUIPropertyManager.SetProperty("#Play.Current.Artist", CurrentTrackTag.Artist);
          GUIPropertyManager.SetProperty("#Play.Current.Genre", CurrentTrackTag.Genre);
          GUIPropertyManager.SetProperty("#Play.Current.Year", strYear);
          GUIPropertyManager.SetProperty("#Play.Current.Rating", (Convert.ToDecimal(2 * CurrentTrackTag.Rating + 1)).ToString());
          GUIPropertyManager.SetProperty("#duration", MediaPortal.Util.Utils.SecondsToHMSString(CurrentTrackTag.Duration));

          if (InfoNeeded)
          {
            UpdateAlbumInfo();
            UpdateTagInfo();
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#Play.Current.Title", GUILocalizeStrings.Get(4543));
          GUIPropertyManager.SetProperty("#Play.Current.Track", string.Empty);
          GUIPropertyManager.SetProperty("#Play.Current.Album", string.Empty);
          GUIPropertyManager.SetProperty("#Play.Current.Artist", string.Empty);
          GUIPropertyManager.SetProperty("#Play.Current.Genre", string.Empty);
          GUIPropertyManager.SetProperty("#duration", "0");
          GUIPropertyManager.SetProperty("#Play.Current.Rating", "0");
          GUIPropertyManager.SetProperty("#Play.Current.Year", string.Empty);

          if (PlaylistPlayer == null)
            if (PlaylistPlayer.GetCurrentItem() == null)
              GUIPropertyManager.SetProperty("#Play.Current.Title", GUILocalizeStrings.Get(4542));
            else
              GUIPropertyManager.SetProperty("#Play.Next.Title", GUILocalizeStrings.Get(4542));
        }

        UpdateNextTrackInfo();
        GUITextureManager.CleanupThumbs();

        _trackChanged = false;
      }
    }

    private void UpdateNextTrackInfo()
    {
      if (NextTrackTag != null)
      {
        LblUpNext.Visible = true;
        string strNextTrack = String.Format("{0} {1}", GUILocalizeStrings.Get(435), NextTrackTag.Track);   //	"Track: "
        if (NextTrackTag.Track <= 0)
          strNextTrack = string.Empty;

        string strYear = String.Format("{0} {1}", GUILocalizeStrings.Get(436), NextTrackTag.Year); //	"Year: "
        if (NextTrackTag.Year <= 1900)
          strYear = string.Empty;

        GUIPropertyManager.SetProperty("#Play.Next.Title", NextTrackTag.Title);
        GUIPropertyManager.SetProperty("#Play.Next.Track", strNextTrack);
        GUIPropertyManager.SetProperty("#Play.Next.Album", NextTrackTag.Album);
        GUIPropertyManager.SetProperty("#Play.Next.Artist", NextTrackTag.Artist);
        GUIPropertyManager.SetProperty("#Play.Next.Genre", NextTrackTag.Genre);
        GUIPropertyManager.SetProperty("#Play.Next.Year", strYear);
        GUIPropertyManager.SetProperty("#Play.Next.Rating", (Convert.ToDecimal(2 * NextTrackTag.Rating + 1)).ToString());
      }
      else
      {
        LblUpNext.Visible = false;
        GUIPropertyManager.SetProperty("#Play.Next.Title", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Track", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Album", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Artist", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Genre", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Year", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Rating", "0");
      }
    }

    private void UpdateTrackPosition()
    {
      if (g_Player.Playing)
      {
        double trackDuration = g_Player.Duration;
        double curTrackPostion = g_Player.CurrentPosition;

        int progPrecent = (int)(curTrackPostion / trackDuration * 100d);

        this.ProgTrack.Percentage = progPrecent;
        ProgTrack.Visible = ProgTrack.Percentage > 0;
      }
    }

    private void GetTrackTags()
    {
      bool isCurSongCdTrack = IsCdTrack(CurrentTrackFileName);
      bool isNextSongCdTrack = IsCdTrack(NextTrackFileName);
      MusicDatabase dbs = MusicDatabase.Instance;

      if (CurrentTrackTag != null)
        PreviousTrackTag = CurrentTrackTag;

      if (!isCurSongCdTrack)
      {
        // CurrentTrackTag = GetTrackTag(dbs, CurrentTrackFileName, UseID3);
        // always use the tagreader now if the info is not in the database
        // since some people use settings which do not represent the results they expect
        CurrentTrackTag = GetTrackTag(dbs, CurrentTrackFileName, true);
      }

      if (!isNextSongCdTrack)
      {
        //NextTrackFileName = PlaylistPlayer.GetNext();
        NextTrackTag = GetTrackTag(dbs, NextTrackFileName, true);
      }

      if (isCurSongCdTrack || isNextSongCdTrack)
      {
        PlayList curPlaylist = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

        int iCurItemIndex = PlaylistPlayer.CurrentSong;
        PlayListItem curPlaylistItem = curPlaylist[iCurItemIndex];

        if (curPlaylistItem == null)
        {
          CurrentTrackTag = null;
          NextTrackTag = null;
          return;
        }

        int playListItemCount = curPlaylist.Count;
        int nextItemIndex = 0;

        if (iCurItemIndex < playListItemCount - 1)
          nextItemIndex = iCurItemIndex + 1;

        PlayListItem nextPlaylistItem = curPlaylist[nextItemIndex];

        if (isCurSongCdTrack)
          CurrentTrackTag = (MusicTag)curPlaylistItem.MusicTag;

        if (isNextSongCdTrack && nextPlaylistItem != null)
          NextTrackTag = (MusicTag)nextPlaylistItem.MusicTag;

        // There's no MusicTag info in the Playlist so check is we have a valid 
        // GUIMusicFiles.MusicCD object
        if ((CurrentTrackTag == null || NextTrackTag == null) && GUIMusicFiles.MusicCD != null)
        {
          int curCDTrackNum = GetCDATrackNumber(CurrentTrackFileName);
          int nextCDTrackNum = GetCDATrackNumber(NextTrackFileName);

          if (curCDTrackNum < GUIMusicFiles.MusicCD.Tracks.Length)
          {
            MediaPortal.Freedb.CDTrackDetail curTrack = GUIMusicFiles.MusicCD.getTrack(curCDTrackNum);
            CurrentTrackTag = GetTrackTag(curTrack);
          }
          if (nextCDTrackNum < GUIMusicFiles.MusicCD.Tracks.Length)
          {
            MediaPortal.Freedb.CDTrackDetail nextTrack = GUIMusicFiles.MusicCD.getTrack(nextCDTrackNum);
            NextTrackTag = GetTrackTag(nextTrack);
          }
        }
      }
    }

    private MusicTag GetTrackTag(MediaPortal.Freedb.CDTrackDetail cdTrack)
    {
      if (GUIMusicFiles.MusicCD == null)
        return null;
      MusicTag tag = new MusicTag();
      tag.Artist = GUIMusicFiles.MusicCD.Artist;
      tag.Album = GUIMusicFiles.MusicCD.Title;
      tag.Genre = GUIMusicFiles.MusicCD.Genre;
      tag.Year = GUIMusicFiles.MusicCD.Year;
      tag.Duration = cdTrack.Duration;
      tag.Title = cdTrack.Title;
      return tag;
    }

    private MusicTag GetTrackTag(MusicDatabase dbs, string strFile, bool useID3)
    {
      MusicTag tag = null;
      Song song = new Song();
      bool bFound = dbs.GetSongByFileName(strFile, ref song);
      if (!bFound)
      {
        if (useID3)
        {
          tag = TagReader.TagReader.ReadTag(strFile);
          if (tag != null && tag.Title != GUILocalizeStrings.Get(4543)) // Track information not available
            return tag;
        }
        // tagreader failed or not using it
        song.Title = System.IO.Path.GetFileNameWithoutExtension(strFile);
        song.Artist = string.Empty;
        song.Album = string.Empty;
      }

      tag = new MusicTag();
      tag = song.ToMusicTag();

      return tag;
    }

    private bool IsCdTrack(string fileName)
    {
      return System.IO.Path.GetExtension(CurrentTrackFileName).ToLower() == ".cda";
    }

    private void GetCDInfoFromFreeDB(string path, MusicTag tag)
    {
      try
      {
        // check internet connectivity
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (null != pDlgOK && !Util.Win32API.IsConnectedToInternet())
        {
          pDlgOK.SetHeading(703);
          pDlgOK.SetLine(1, 703);
          pDlgOK.SetLine(2, string.Empty);
          pDlgOK.DoModal(GetID);
          return;
        }
        else if (!Util.Win32API.IsConnectedToInternet())
        {
          return;
        }
        string m_strDiscId = string.Empty;
        int m_iSelectedAlbum = 0;

        Freedb.FreeDBHttpImpl freedb = new Freedb.FreeDBHttpImpl();
        char driveLetter = System.IO.Path.GetFullPath(path).ToCharArray()[0];

        // try finding it in the database
        string strPathName, strCDROMPath;

        //int_20h fake the path with the cdInfo
        strPathName = driveLetter + ":/" + freedb.GetCDDBDiscIDInfo(driveLetter, '+');
        strCDROMPath = strPathName + "+" + System.IO.Path.GetFileName(path);

        Song song = new Song();
        bool bFound = false;

        try
        {
          freedb.Connect(); // should be replaced with the Connect that receives a http freedb site...
          Freedb.CDInfo[] cds = freedb.GetDiscInfo(driveLetter);
          if (cds != null)
          {
            // freedb returned one album
            if (cds.Length == 1)
            {
              GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[0].Category, cds[0].DiscId);
              m_strDiscId = cds[0].DiscId;
            }

          // freedb returned more than one album
            else if (cds.Length > 1)
            {
              if (m_strDiscId == cds[0].DiscId)
              {
                GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[m_iSelectedAlbum].Category, cds[m_iSelectedAlbum].DiscId);
              }

              else
              {
                m_strDiscId = cds[0].DiscId;
                //show dialog with all albums found
                string szText = GUILocalizeStrings.Get(181);
                GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
                if (null != pDlg)
                {
                  pDlg.Reset();
                  pDlg.SetHeading(szText);
                  for (int j = 0; j < cds.Length; j++)
                  {
                    Freedb.CDInfo info = cds[j];
                    pDlg.Add(info.Title);
                  }
                  pDlg.DoModal(GetID);

                  // and wait till user selects one
                  m_iSelectedAlbum = pDlg.SelectedLabel;
                  if (m_iSelectedAlbum < 0)
                    return;
                  GUIMusicFiles.MusicCD = freedb.GetDiscDetails(cds[m_iSelectedAlbum].Category, cds[m_iSelectedAlbum].DiscId);
                }
              }
            }
          }
          freedb.Disconnect();
          //if (GUIMusicFiles.MusicCD == null) bCDDAFailed = true;
        }

        catch (Exception)
        {
          GUIMusicFiles.MusicCD = null;
        }

        if (!bFound && GUIMusicFiles.MusicCD != null) // if musicCD was configured correctly...
        {
          int trackno = GetCDATrackNumber(path);
          Freedb.CDTrackDetail track = GUIMusicFiles.MusicCD.getTrack(trackno);

          tag = new MusicTag();
          tag.Album = GUIMusicFiles.MusicCD.Title;
          tag.Genre = GUIMusicFiles.MusicCD.Genre;
          if (track == null)
          {
            // prob hidden track									
            tag.Artist = GUIMusicFiles.MusicCD.Artist;
            tag.Duration = -1;
            tag.Title = string.Empty;
            tag.Track = -1;
          }
          else
          {
            tag.Artist = track.Artist == null ? GUIMusicFiles.MusicCD.Artist : track.Artist;
            tag.Duration = track.Duration;
            tag.Title = track.Title;
            tag.Track = track.TrackNumber;
          }
        }
        else if (bFound)
        {
          tag = new MusicTag();
          tag = song.ToMusicTag();
        }

      }// end of try
      catch (Exception e)
      {
        // log the problem...
        Log.Error("GUIMusicPlayingNow: GetCDInfoFromFreeDB: {0}", e.ToString());
      }
    }

    private void OnSongInserted()
    {
      _trackChanged = true;
      NextTrackFileName = PlaylistPlayer.GetNext();
      GetTrackTags();
      UpdateTrackInfo();
    }

    private bool AddSongToPlaylist(ref GUIListItem song, bool enqueueNext_)
    {
      PlayList playlist = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      if (playlist == null)
        return false;

      //add to playlist
      PlayListItem playlistItem = new PlayListItem();
      playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Audio;
      StringBuilder sb = new StringBuilder();
      MusicTag tmptag = new MusicTag();
      tmptag = (MusicTag)song.MusicTag;

      playlistItem.FileName = song.Path;
      sb.Append(tmptag.Track);
      sb.Append(". ");
      sb.Append(tmptag.Artist);
      sb.Append(" - ");
      sb.Append(tmptag.Title);
      playlistItem.Description = sb.ToString();
      playlistItem.Duration = tmptag.Duration;

      playlistItem.MusicTag = tmptag;

      if (enqueueNext_)
        PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Insert(playlistItem, PlaylistPlayer.CurrentSong);
      else
        PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playlistItem);

      OnSongInserted();
      return true;
    }

    /// <summary>
    /// Add or enque a song to the current playlist - call OnSongInserted() after this!!!
    /// </summary>
    /// <param name="song">the song to add</param>
    /// <param name="enqueueNext_">if true the songs is inserted after the current track, otherwise added to the end of the list</param>
    /// <returns>if the action was successful</returns>
    private bool AddSongToPlaylist(ref Song song, bool enqueueNext_)
    {
      PlayList playlist = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      if (playlist == null)
        return false;

      //add to playlist
      PlayListItem playlistItem = new PlayListItem();
      playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Audio;
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

      if (enqueueNext_)
        PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Insert(playlistItem, PlaylistPlayer.CurrentSong);
      else
        PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playlistItem);

      OnSongInserted();
      return true;
    }

    private int GetCDATrackNumber(string strFile)
    {
      string strTrack = string.Empty;
      int pos = strFile.IndexOf(".cda");
      if (pos >= 0)
      {
        pos--;
        while (Char.IsDigit(strFile[pos]) && pos > 0)
        {
          strTrack = strFile[pos] + strTrack;
          pos--;
        }
      }

      try
      {
        int iTrack = Convert.ToInt32(strTrack);
        return iTrack;
      }
      catch (Exception)
      {
      }

      return 1;
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
          g_Player_PlayBackStarted(g_Player.MediaType.Music, g_Player.CurrentFile);
        }
      }
    }

    private void SetVisualizationWindow()
    {
      if (!ControlsInitialized || !_showVisualization || !_usingBassEngine)
        return;

      // the solution below doesn't rotate e.g. artist pics.

      //if (ImgCoverArt != null)
      //{
      //  GUIGraphicsContext.VideoWindow = new Rectangle(ImgCoverArt.XPosition, ImgCoverArt.YPosition, ImgCoverArt.Width, ImgCoverArt.Height);
      //  return;
      //}

      Visualization.VisualizationWindow vizWindow = BassMusicPlayer.Player.VisualizationWindow;

      if (vizWindow != null)
      {
        vizWindow.Visible = false;

        int width = ImgCoverArt.RenderWidth;
        int height = ImgCoverArt.RenderHeight;

        //int width = ImgCoverArt.Width;
        //int height = ImgCoverArt.Height;

        System.Drawing.Size vizSize = new System.Drawing.Size(width, height);
        System.Drawing.Point vizLoc = new System.Drawing.Point((int)ImgCoverArt.Location.X, (int)ImgCoverArt.Location.Y);
        vizWindow.Size = vizSize;
        vizWindow.Location = vizLoc;
        vizWindow.Visible = true;

        GUIGraphicsContext.VideoWindow = new System.Drawing.Rectangle(vizLoc, vizSize);
      }
    }

    private void ClearVisualizationImages()
    {
      if (!_usingBassEngine || !_showVisualization)
        return;

      Visualization.VisualizationWindow vizWindow = BassMusicPlayer.Player.VisualizationWindow;

      if (vizWindow != null)
        vizWindow.ClearImages();
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

    #endregion
  }
}