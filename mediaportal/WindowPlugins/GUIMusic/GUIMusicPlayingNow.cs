#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using System.Drawing;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;

namespace MediaPortal.GUI.Music
{
  public class GUIMusicPlayingNow : GUIWindow
  {
    private enum PopularityRating : int
    {
      unknown = 0,
      existent = 1,
      known =2,
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
      Elapsed, CountDown
    }

    #region Properties

    public GUIMusicBaseWindow MusicWindow
    {
      set { _MusicWindow = value; }
    }

    #endregion

    [SkinControlAttribute((int)ControlIDs.LBL_CAPTION)]              protected GUILabelControl LblCaption = null;
    [SkinControlAttribute((int)ControlIDs.IMG_COVERART)]             protected GUIImage ImgCoverArt = null;
    [SkinControlAttribute((int)ControlIDs.PROG_TRACK)]               protected GUIProgressControl ProgTrack = null;
    [SkinControlAttribute((int)ControlIDs.IMG_TRACK_PROGRESS_BG)]    protected GUIImage ImgTrackProgressBkGrnd = null;
    [SkinControlAttribute((int)ControlIDs.LBL_UP_NEXT)]              protected GUILabelControl LblUpNext = null;
    [SkinControlAttribute((int)ControlIDs.LIST_TAG_INFO)]            protected GUIListControl facadeTagInfo = null;
    [SkinControlAttribute((int)ControlIDs.LIST_ALBUM_INFO)]          protected GUIListControl facadeAlbumInfo = null;
    [SkinControlAttribute((int)ControlIDs.BEST_ALBUM_TRACKS)]        protected GUIFadeLabel LblBestAlbumTracks = null;
    [SkinControlAttribute((int)ControlIDs.BEST_TAG_TRACKS)]          protected GUIFadeLabel LblBestTagTracks = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_UNKNOWN_TRACK1)]   protected GUIImageList ImgListUnknownTrack1 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_UNKNOWN_TRACK2)]   protected GUIImageList ImgListUnknownTrack2 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_UNKNOWN_TRACK3)]   protected GUIImageList ImgListUnknownTrack3 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_EXISTENT_TRACK1)]  protected GUIImageList ImgListExistingTrack1 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_EXISTENT_TRACK2)]  protected GUIImageList ImgListExistingTrack2 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_EXISTENT_TRACK3)]  protected GUIImageList ImgListExistingTrack3 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_KNOWN_TRACK1)]     protected GUIImageList ImgListKnownTrack1 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_KNOWN_TRACK2)]     protected GUIImageList ImgListKnownTrack2 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_KNOWN_TRACK3)]     protected GUIImageList ImgListKnownTrack3 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_FAMOUS_TRACK1)]    protected GUIImageList ImgListFamousTrack1 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_FAMOUS_TRACK2)]    protected GUIImageList ImgListFamousTrack2 = null;
    [SkinControlAttribute((int)ControlIDs.IMGLIST_FAMOUS_TRACK3)]    protected GUIImageList ImgListFamousTrack3 = null;

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


    public GUIMusicPlayingNow()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW;
      PlaylistPlayer = PlayListPlayer.SingletonPlayer;
      ImagePathContainer = new List<string>();

      g_Player.PlayBackStarted += new g_Player.StartedHandler(g_Player_PlayBackStarted);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(g_Player_PlayBackEnded);

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        UseID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);
        _showVisualization = xmlreader.GetValueAsBool("musicmisc", "showVisInNowPlaying", false);
        _doArtistLookups = xmlreader.GetValueAsBool("musicmisc", "fetchlastfmthumbs", true);
        _doAlbumLookups = xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtopalbums", true);
        _doTrackTagLookups = xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtracktags", true);
      }

      _usingBassEngine = BassMusicPlayer.IsDefaultMusicPlayer;
    }

    void g_Player_PlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (!ControlsInitialized)
        return;
    }

    void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (!ControlsInitialized)
        return;

      if (GUIWindowManager.ActiveWindow == GetID)
      {
        Action action = new Action();
        action.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
        GUIGraphicsContext.OnAction(action);
      }
    }

    void g_Player_PlayBackStarted(g_Player.MediaType type, string filename)
    {
      Log.Debug("GUIMusicPlayingNow: g_Player_PlayBackStarted for {0}", filename);
      if (!ControlsInitialized)
        return;

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

      if (CurrentThumbFileName.Length > 0)
        AddImageToImagePathContainer(CurrentThumbFileName);

      UpdateImagePathContainer();
      UpdateTrackInfo();
      UpdateTrackPosition();
      SetVisualizationWindow();
    }

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
          {
            if (GUIWindowManager.ActiveWindow == GetID)
            {
              Action act = new Action();
              act.wID = Action.ActionType.ACTION_PREVIOUS_MENU;
              GUIGraphicsContext.OnAction(act);
            }
            break;
          }

        // Since a ACTION_STOP action clears the player and CurrentPlaylistType type
        // we need a way to restart playback after an ACTION_STOP has been received
        case Action.ActionType.ACTION_MUSIC_PLAY:
        case Action.ActionType.ACTION_NEXT_ITEM:
        case Action.ActionType.ACTION_PAUSE:
        case Action.ActionType.ACTION_PREV_ITEM:
          {
            //if (PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC)
            if ((PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC) &&
                (PlaylistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_MUSIC_TEMP))
            {
              LoadAndStartPlayList();
            }
            break;
          }
      }
    }

    private void UpdateImagePathContainer()
    {
      if (ImagePathContainer.Count <= 0)
      {
        AddImageToImagePathContainer(GUIGraphicsContext.Skin + @"\media\missing_coverart.png");
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
      string ImagePath = Convert.ToString(newImage);
      // Check if we should let the visualization window handle image flipping
      if (_usingBassEngine && _showVisualization)
      {
        Log.Debug("GUIMusicPlayingNow: adding image to visualization - {0}", ImagePath);
        Visualization.VisualizationWindow vizWindow = BassMusicPlayer.Player.VisualizationWindow;

        if (vizWindow != null)
        {
          if (System.IO.File.Exists(ImagePath))
          {
            try
            {
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
        Log.Debug("GUIMusicPlayingNow: adding image to container - {0}", ImagePath);
        if (ImagePathContainer.Contains(ImagePath))
          return false;

        // check for placeholder
        int indexDel = 0;
        bool found = false;
        foreach (string pic in ImagePathContainer)
        {
          indexDel++;
          if (pic.IndexOf("missing_coverart.png") > 0)
          {
            found = true;
            break;
          }
        }
        if (found)
          ImagePathContainer.RemoveAt(indexDel - 1);

        if (System.IO.File.Exists(ImagePath))
        {
          try
          {
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
        MusicDatabase mdb = new MusicDatabase();
        MusicTag listTag = new MusicTag();
        List<GUIListItem> guiListItemList = new List<GUIListItem>();
        GUIListItem queueItem = new GUIListItem();

        listTag = (MusicTag)chosenTrack_.MusicTag;
        guiListItemList.Add(chosenTrack_);

        if (mdb.GetSongs(2, listTag.Title, ref guiListItemList))
        {
          MusicTag tempTag = new MusicTag();

          foreach (GUIListItem alternativeSong in guiListItemList)
          {
            tempTag = GetTrackTag(mdb, alternativeSong.Path, false);
            if (tempTag != null && tempTag.Artist != String.Empty)
            {
              if (tempTag.Artist.ToUpperInvariant() == listTag.Artist.ToUpperInvariant())
              {
                queueItem = alternativeSong;
                queueItem.MusicTag = tempTag;
              }
            }
          }
          if (queueItem != null && queueItem.MusicTag != null)
            if (AddSongToPlaylist(ref queueItem, true))
              Log.Info("GUIMusicPlayingNow: Song inserted: {0} - {1}", listTag.Artist, listTag.Title);
        }
        else
          Log.Debug("GUIMusicPlayingNow: DB lookup for Song {0} unsuccessful", listTag.Artist + " - " + listTag.Title);
      }
      catch (Exception ex)
      {
        Log.Debug("GUIMusicPlayingNow: DB lookup for Song failed - {0}", ex.Message);
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
                  AddInfoTrackToPlaylist(facadeAlbumInfo.SelectedListItem, true);
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
        //case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS:
        //  {
        //    //_currentPlaying = message.Label;
        //    if (GUIWindowManager.ActiveWindow == GetID)
        //    {
        //      //facadeAlbumInfo.OnMessage(message); // really needed?
        //      //facadeTagInfo.OnMessage(message);
        //    }
        //  }
        //  break;
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

      if (GUIPropertyManager.GetProperty("#Play.Next.Title") == String.Empty)
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

      if (ImgCoverArt != null)
      {
        GUIGraphicsContext.VideoWindow = new Rectangle(ImgCoverArt.XPosition, ImgCoverArt.YPosition, ImgCoverArt.Width, ImgCoverArt.Height);
      }
  
      if (g_Player.Playing)
      {
        g_Player_PlayBackStarted(g_Player.MediaType.Music, g_Player.CurrentFile);

        //SetVisualizationWindow();
      }
      else
      {
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
      GC.Collect();
      base.OnPageDestroy(new_windowId);
      ControlsInitialized = false;

      // Make sure we clear any images we added so we revert back the coverart image
      ClearVisualizationImages();
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

      if (dlg == null)
        return;

      dlg.Reset();
      dlg.SetHeading(924);                // Menu

      dlg.AddLocalizedString(930);        //Add to favorites

      //dlg.AddLocalizedString(928);        // Find Coverart
      //dlg.AddLocalizedString(4521);       // Show Album Info

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
              AddImageToImagePathContainer(CurrentThumbFileName);

            if (_usingBassEngine)
              BassMusicPlayer.Player.VisualizationWindow.CoverArtImagePath = CurrentThumbFileName;

            UpdateImagePathContainer();
          }
          break;


        case 4521:      // Show Album Info
          if (_MusicWindow != null)
          {
            string albumFolderPath = System.IO.Path.GetDirectoryName(CurrentTrackFileName);
            if (_MusicWindow != null)
              _MusicWindow.ShowAlbumInfo(GetID, false, CurrentTrackTag.Artist, CurrentTrackTag.Album, albumFolderPath, CurrentTrackTag, -1);
          }
            break;


        case 4554:      // Lookup CD info
          {
            MusicTag tag = new MusicTag();
            GetCDInfoFromFreeDB(CurrentTrackFileName, tag);
            GetTrackTags();
            break;
          }
        case 33040:    // IRC spam
          {
            try
            {
              if (CurrentTrackTag != null)
              {
                string tmpTrack = CurrentTrackTag.Track > 0 ? (Convert.ToString(CurrentTrackTag.Track) + ". ") : String.Empty;
                Clipboard.SetDataObject(@"/me is listening to " + CurrentTrackTag.Artist + " [" + CurrentTrackTag.Album + "] - " + tmpTrack + CurrentTrackTag.Title, true);
              }
              break;
            }
            catch (Exception ex)
            {
              Log.Error("GUIMusicPlayingNow: could not copy song spam to clipboard - {0}", ex.Message);
              break;
            }
          }
        case 930: // add to favorites
          {
            MusicDatabase dbs = new MusicDatabase();
            Song currentSong = new Song();
            string strFile = g_Player.Player.CurrentFile;

            bool songFound = dbs.GetSongByFileName(strFile, ref currentSong);
            if (songFound)
            {
              if (currentSong == null)
                return;
              if (currentSong.songId < 0)
                return;
              currentSong.Favorite = true;
              dbs.SetFavorite(currentSong);
            }
            break;
          }
      }
    }

    private MusicTag BuildMusicTagFromSong(Song Song_)
    {
      MusicTag tmpTag = new MusicTag();

      tmpTag.Title = Song_.Title;
      tmpTag.Album = Song_.Album;
      tmpTag.Artist = Song_.Artist;
      tmpTag.Duration = Song_.Duration;
      tmpTag.Genre = Song_.Genre;
      tmpTag.Track = Song_.Track;
      tmpTag.Year = Song_.Year;
      tmpTag.Rating = Song_.Rating;

      return tmpTag;
    }

    private string CleanTagString(string tagField)
    {
      int dotIndex = 0;
      string outString = String.Empty;

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
        if (CurrentTrackTag.Artist == String.Empty || CurrentTrackTag.Album == String.Empty)
        {
          Log.Warn("GUIMusicPlayingNow: current tag invalid for album info lookup. File: {0}", g_Player.CurrentFile);
          return;
        }
        AlbumInfoRequest request = new AlbumInfoRequest(
                                        CurrentArtist,
                                        CurrentAlbum,
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
        if (CurrentTrackTag.Artist == String.Empty)
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
        if (CurrentTrackTag.Artist == String.Empty || CurrentTrackTag.Title == String.Empty)
        {
          Log.Warn("GUIMusicPlayingNow: current tag invalid for tag info lookup. File: {0}", g_Player.CurrentFile);
          return;
        }
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
              CurrentThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, Util.Utils.FilterFileName(CurrentTrackTag.Artist));
              if (CurrentThumbFileName.Length > 0)
              {
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
            strTrack = String.Empty;

          string strYear = String.Format("{0} {1}", GUILocalizeStrings.Get(436), CurrentTrackTag.Year); //	"Year: "
          if (CurrentTrackTag.Year <= 1900)
            strYear = String.Empty;

          GUIPropertyManager.SetProperty("#Play.Current.Title", CurrentTrackTag.Title);
          GUIPropertyManager.SetProperty("#Play.Current.Track", strTrack);
          GUIPropertyManager.SetProperty("#Play.Current.Album", CurrentTrackTag.Album);
          GUIPropertyManager.SetProperty("#Play.Current.Artist", CurrentTrackTag.Artist);
          GUIPropertyManager.SetProperty("#Play.Current.Genre", CurrentTrackTag.Genre);
          GUIPropertyManager.SetProperty("#Play.Current.Year", strYear);

          if (g_Player.Playing)
            GUIPropertyManager.SetProperty("#duration", Convert.ToString(g_Player.Duration));

          //if (ImgListNextRating != null)
          //{
          //  int rating = CurrentTrackTag.Rating;
          //  ImgListRating.Percentage = rating;
          //}
          if (InfoNeeded)
          {
            UpdateAlbumInfo();
            UpdateTagInfo();
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#Play.Current.Title", GUILocalizeStrings.Get(4543));
          GUIPropertyManager.SetProperty("#Play.Current.Track", String.Empty);
          GUIPropertyManager.SetProperty("#duration", String.Empty);

          if (PlaylistPlayer == null)
            if (PlaylistPlayer.GetCurrentItem() == null)
              GUIPropertyManager.SetProperty("#Play.Current.Title", GUILocalizeStrings.Get(4542));
            else
              GUIPropertyManager.SetProperty("#Play.Next.Title", GUILocalizeStrings.Get(4542));
        }

        if (NextTrackTag != null)
        {
          LblUpNext.Visible = true;
          string strNextTrack = String.Format("{0} {1}", GUILocalizeStrings.Get(435), NextTrackTag.Track);   //	"Track: "
          if (NextTrackTag.Track <= 0)
            strNextTrack = String.Empty;

          string strYear = String.Format("{0} {1}", GUILocalizeStrings.Get(436), NextTrackTag.Year); //	"Year: "
          if (NextTrackTag.Year <= 1900)
            strYear = String.Empty;

          GUIPropertyManager.SetProperty("#Play.Next.Title", NextTrackTag.Title);
          GUIPropertyManager.SetProperty("#Play.Next.Track", strNextTrack);
          GUIPropertyManager.SetProperty("#Play.Next.Album", NextTrackTag.Album);
          GUIPropertyManager.SetProperty("#Play.Next.Artist", NextTrackTag.Artist);
          GUIPropertyManager.SetProperty("#Play.Next.Genre", NextTrackTag.Genre);
          GUIPropertyManager.SetProperty("#Play.Next.Year", strYear);
        }
        else
        {
          LblUpNext.Visible = false;
          GUIPropertyManager.SetProperty("#Play.Next.Title", String.Empty);
          GUIPropertyManager.SetProperty("#Play.Next.Track", String.Empty);
          GUIPropertyManager.SetProperty("#Play.Next.Album", String.Empty);
          GUIPropertyManager.SetProperty("#Play.Next.Artist", String.Empty);
          GUIPropertyManager.SetProperty("#Play.Next.Genre", String.Empty);
          GUIPropertyManager.SetProperty("#Play.Next.Year", String.Empty);
        }
        _trackChanged = false;
      }
    }

    private void UpdateTrackPosition()
    {
      double trackDuration = g_Player.Duration;
      double curTrackPostion = g_Player.CurrentPosition;

      int progPrecent = (int)(curTrackPostion / trackDuration * 100d);

      this.ProgTrack.Percentage = progPrecent;
      ProgTrack.Visible = ProgTrack.Percentage > 0;
    }

    private void GetTrackTags()
    {
      bool isCurSongCdTrack = IsCdTrack(CurrentTrackFileName);
      bool isNextSongCdTrack = IsCdTrack(NextTrackFileName);
      MusicDatabase dbs = new MusicDatabase();

      if (CurrentTrackTag != null)
        PreviousTrackTag = CurrentTrackTag;

      if (!isCurSongCdTrack)
        CurrentTrackTag = GetTrackTag(dbs, CurrentTrackFileName, UseID3);

      if (!isNextSongCdTrack)
        NextTrackTag = GetTrackTag(dbs, NextTrackFileName, UseID3);

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
          tag = TagReader.TagReader.ReadTag(strFile);
      }
      else
      {
        tag = new MusicTag();
        tag = BuildMusicTagFromSong(song);
      }
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
          pDlgOK.SetLine(2, String.Empty);
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
            tag.Title = String.Empty;
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
          tag = BuildMusicTagFromSong(song);
        }

      }// end of try
      catch (Exception e)
      {
        // log the problem...
        Log.Error("GUIMusicPlayingNow: GetCDInfoFromFreeDB: {0}", e.ToString());
      }
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

      _trackChanged = true;
      NextTrackFileName = PlaylistPlayer.GetNext();
      GetTrackTags();
      UpdateTrackInfo();
      return true;
    }

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

      playlistItem.MusicTag = BuildMusicTagFromSong(song);

      if (enqueueNext_)
        PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Insert(playlistItem, PlaylistPlayer.CurrentSong);
      else
        PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playlistItem);

      _trackChanged = true;
      NextTrackFileName = PlaylistPlayer.GetNext();
      GetTrackTags();
      UpdateTrackInfo();
      return true;
    }

    int GetCDATrackNumber(string strFile)
    {
      string strTrack = String.Empty;
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
      if (!ControlsInitialized || !_usingBassEngine || !_showVisualization)
        return;

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

    public void OnUpdateAlbumInfoCompleted(AlbumInfoRequest request, List<Song> AlbumTracks)
    {
      if (request.Equals(_lastAlbumRequest))
      {
        GUIListItem item = null;
        facadeAlbumInfo.Clear();
        ToggleTopTrackRatings(false, PopularityRating.unknown);
        //GUIPropertyManager.SetProperty("#Lastfm.Rating.AlbumTrack1", "0");
        //GUIPropertyManager.SetProperty("#Lastfm.Rating.AlbumTrack2", "0");
        //GUIPropertyManager.SetProperty("#Lastfm.Rating.AlbumTrack3", "0");

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

            item.MusicTag = BuildMusicTagFromSong(AlbumTracks[i]);

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

          //if (CurrentThumbFileName == GUIGraphicsContext.Skin + @"\media\missing_coverart.png")
          //{
          CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);
          if (CurrentThumbFileName.Length > 0)
            AddImageToImagePathContainer(CurrentThumbFileName);

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
        CurrentThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, Util.Utils.FilterFileName(CurrentTrackTag.Artist));
        if (CurrentThumbFileName.Length > 0)
        {
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

            item.MusicTag = BuildMusicTagFromSong(TagTracks[i]);

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

  }
}