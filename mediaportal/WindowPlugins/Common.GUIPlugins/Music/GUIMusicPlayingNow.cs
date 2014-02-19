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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.Util;
using MediaPortal.Visualization;
using MediaPortal.LastFM;
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

    private enum ControlIDs
    {
      LBL_CAPTION = 1,
      BTN_LASTFM_LOVE = 30,
      BTN_LASTFM_BAN = 31,
      IMG_COVERART = 112,
      LIST_SIMILAR_TRACKS = 155,
      PROG_TRACK = 118,
      LBL_UP_NEXT = 20,
      VUMETER_LEFT = 999,
      VUMETER_RIGHT = 998,
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
    [SkinControl((int)ControlIDs.LBL_UP_NEXT)] protected GUILabelControl LblUpNext = null;
    [SkinControl((int)ControlIDs.VUMETER_LEFT)] protected GUIImage VUMeterLeft = null;
    [SkinControl((int)ControlIDs.VUMETER_RIGHT)] protected GUIImage VUMeterRight = null;
    [SkinControl((int)ControlIDs.LIST_SIMILAR_TRACKS)] protected GUIListControl lstSimilarTracks = null;
    [SkinControl((int)ControlIDs.BTN_LASTFM_LOVE)] protected GUIButtonControl btnLastFMLove = null;
    [SkinControl((int)ControlIDs.BTN_LASTFM_BAN)] protected GUIButtonControl btnLastFMBan = null;

    #endregion

    #region Event delegates

    protected delegate void PlaybackChangedDelegate(g_Player.MediaType type, string filename);
    protected delegate void UpdateSimilarTracksDelegate(string filename, MusicTag tag);
    protected delegate void TimerElapsedDelegate();

    #endregion

    #region Variables

    private bool ControlsInitialized = false;
    private PlayListPlayer PlaylistPlayer = null;
    private string CurrentThumbFileName = string.Empty;
    private string CurrentTrackFileName = string.Empty;
    private MusicTag PreviousTrackTag = null;
    private MusicTag CurrentTrackTag = null;
    private MusicTag NextTrackTag = null;
    private GUIMusicBaseWindow _MusicWindow = null;
    private Timer ImageChangeTimer = null;
    private Timer VUMeterTimer = null;
    private List<String> ImagePathContainer = null;
    private bool _trackChanged = true;
    private bool _usingBassEngine = false;
    private bool _showVisualization = false;
    private object _imageMutex = null;
    private string _vuMeter = "none";
    private static readonly Random Randomizer = new Random();
    private bool _lookupSimilarTracks;

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
        _vuMeter = xmlreader.GetValueAsString("musicmisc", "vumeter", "none");
        _lookupSimilarTracks = xmlreader.GetValueAsBool("musicmisc", "lookupSimilarTracks", true);

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
      UpdateSimilarTracks(filename);
    }

    private void DoOnStarted(g_Player.MediaType type, string filename)
    {
      Log.Debug("GUIMusicPlayingNow: g_Player_PlayBackStarted for {0}", filename);

      ImagePathContainer.Clear();
      ClearVisualizationImages();

      CurrentTrackFileName = filename;
      GetTrackTags();

      CurrentThumbFileName = GUIMusicBaseWindow.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);

      if (string.IsNullOrEmpty(CurrentThumbFileName))
        // no LOCAL Thumb found because user has bad settings -> check if there is a folder.jpg in the share
      {
        CurrentThumbFileName = Util.Utils.GetFolderThumb(CurrentTrackFileName);
        if (!Util.Utils.FileExistsInCache(CurrentThumbFileName))
        {
          CurrentThumbFileName = string.Empty;
        }
      }

      if (!string.IsNullOrEmpty(CurrentThumbFileName))
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
      if (_vuMeter.ToLowerInvariant() == "analog")
      {
        success = Load(GUIGraphicsContext.GetThemedSkinFile(@"\MyMusicPlayingNowAnVu.xml"));
      }
      else if (_vuMeter.ToLowerInvariant() == "led")
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
          if ((PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC) &&
              (PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC_TEMP) &&
              (PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_LAST_FM))
          {
            LoadAndStartPlayList();
          }
          break;

        case Action.ActionType.ACTION_SHOW_INFO:
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
            if (message.SenderControlId == (int)ControlIDs.LIST_SIMILAR_TRACKS)
            {
              if ((int) Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
              {
                var song = (Song) lstSimilarTracks.SelectedListItem.AlbumInfoTag;
                AddSongToPlaylist(ref song);
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

      ImagePathContainer.Clear();

      _trackChanged = true;

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

      ControlsInitialized = true;

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
          _vuMeter.ToLowerInvariant() != "none")
      {
        VUMeterTimer = new Timer();
        VUMeterTimer.Interval = 10;
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

    protected override void OnPageDestroy(int newWindowId)
    {
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

      base.OnPageDestroy(newWindowId);
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

      if (PluginManager.IsPluginNameEnabled2("LastFMScrobbler"))
      {
        dlg.AddLocalizedString(34010); //last.fm love
        dlg.AddLocalizedString(34011); //last.fm ban
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
              _MusicWindow.ShowAlbumInfo(GetID, CurrentTrackTag.Artist, CurrentTrackTag.Album);
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
        if (ImagePath.IndexOf(@"missing_coverart") > 0)
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

    /// <summary>
    /// Updates the artist info for the current track playing.
    /// </summary>
    private void UpdateArtistInfo()
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
          UpdateArtistInfo();
        }

        UpdateNextTrackInfo();

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

      bool isInternetStream = Util.Utils.IsAVStream(CurrentTrackFileName) && !Util.Utils.IsLastFMStream(CurrentTrackFileName);
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
      GetTrackTags();
      UpdateTrackInfo();
    }

    /// <summary>
    /// Add or enque a song to the current playlist - call OnSongInserted() after this!!!
    /// </summary>
    /// <param name="song">the song to add</param>
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

      VisualizationWindow vizWindow = BassMusicPlayer.Player.VisualizationWindow;

      if (vizWindow != null)
      {
        vizWindow.Visible = false;

        int width = ImgCoverArt.RenderWidth;
        int height = ImgCoverArt.RenderHeight;

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

    #endregion

    #region last.fm integration

    private void DoLastFMLove()
    {
      string dlgText = GUILocalizeStrings.Get(34010) + " : " + CurrentTrackTag.Title;

      try
      {
        LastFMLibrary.LoveTrack(CurrentTrackTag.Artist, CurrentTrackTag.Title);
      }
      catch (Exception ex)
      {
        Log.Error("Error in DoLastFMLove");
        Log.Error(ex);
        dlgText = GUILocalizeStrings.Get(1025) + "\n" + dlgText; // prepend "An Error has occurred"
      }

      var dlgNotifyLastFM = (GUIDialogNotifyLastFM)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_LASTFM);
      dlgNotifyLastFM.SetHeading(GUILocalizeStrings.Get(34000)); // last.FM
      dlgNotifyLastFM.SetText(dlgText);
      dlgNotifyLastFM.TimeOut = 2;
      dlgNotifyLastFM.DoModal(GetID);
      
    }

    private void DoLastFMBan()
    {
      string dlgText = GUILocalizeStrings.Get(34011) + " : " + CurrentTrackTag.Title;

      try
      {
        LastFMLibrary.BanTrack(CurrentTrackTag.Artist, CurrentTrackTag.Title);
      }
      catch (Exception ex)
      {
        Log.Error("Error in DoLastFMBan");
        Log.Error(ex);
        dlgText = GUILocalizeStrings.Get(1025) + "\n" + dlgText; // prepend "An Error has occurred"
      }

      var dlgNotifyLastFM = (GUIDialogNotifyLastFM)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_LASTFM);
      dlgNotifyLastFM.SetHeading(GUILocalizeStrings.Get(34000)); // last.FM
      dlgNotifyLastFM.SetText(dlgText);
      dlgNotifyLastFM.TimeOut = 2;
      dlgNotifyLastFM.DoModal(GetID);
    }

    private void UpdateSimilarTracks(string filename)
    {
      if (!_lookupSimilarTracks) return;

      lstSimilarTracks.Clear();

      var worker = new BackgroundWorker();
      worker.DoWork += (obj, e) => UpdateSimilarTrackWorker(filename, CurrentTrackTag);
      worker.RunWorkerAsync();      
    }

    private void UpdateSimilarTrackWorker(string filename, MusicTag tag)
    {
      if (tag == null) return;

      List<LastFMSimilarTrack> tracks;
      try
      {
        tracks = LastFMLibrary.GetSimilarTracks(tag.Title, tag.Artist);
      }
      catch (Exception ex)
      {
        Log.Error("Error getting similar tracks in now playing");
        Log.Error(ex);
        return;
      }

      var dbTracks = GetSimilarTracksInDatabase(tracks);

      for (var i = 0; i < 3; i++)
      {
        if (dbTracks.Count > 0)
        {
          var trackNo = Randomizer.Next(0, dbTracks.Count);
          var song = dbTracks[trackNo];

          var t = song.ToMusicTag();
          var item = new GUIListItem
                       {
                         AlbumInfoTag = song,
                         MusicTag = tag,
                         IsFolder = false,
                         Label = song.Title,
                         Path = song.FileName
                       };
          item.AlbumInfoTag = song;
          item.MusicTag = t;
          GUIMusicBaseWindow.SetTrackLabels(ref item, MusicSort.SortMethod.Album);
          dbTracks.RemoveAt(trackNo); // remove song after adding to playlist to prevent the same sone being added twice

          if (g_Player.currentFileName != filename) return; // track has changed since request so ignore

          lstSimilarTracks.Add(item);
        }
      }
    }

    /// <summary>
    /// Takes a list of tracks supplied by last.fm and matches them to tracks in the database
    /// </summary>
    /// <param name="tracks">List of last FM tracks to check</param>
    /// <returns>List of matched songs from input that exist in the users database</returns>
    private static List<Song> GetSimilarTracksInDatabase(List<LastFMSimilarTrack> tracks)
    {
      //TODO: this code now exists in both last.fm scrobbler and here.   Need to combine the code (without creating a circular 
      // list contains songs which exist in users collection
      var dbTrackListing = new List<Song>();

      //identify which are available in users collection (ie. we can use they for auto DJ mode)
      foreach (var strSql in tracks.Select(track => String.Format("select * from tracks where strartist like '%| {0} |%' and strTitle = '{1}'",
                                                                  DatabaseUtility.RemoveInvalidChars(track.ArtistName),
                                                                  DatabaseUtility.RemoveInvalidChars(track.TrackTitle))))
      {
        List<Song> trackListing;
        MusicDatabase.Instance.GetSongsBySQL(strSql, out trackListing);

        dbTrackListing.AddRange(trackListing);
      }

      return dbTrackListing;
    }

    #endregion
  }
}