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
    private enum ControlIDs
    {
      LBL_CAPTION = 1,
      IMG_COVERART = 112,
      LBL_TRACK_NAME = 113,
      LBL_ALBUM_NAME = 114,
      LBL_ALBUM_YEAR = 115,
      LBL_ARTIST_NAME = 6,
      IMG_TRACK_PROGRESS_BG = 117,
      PROG_TRACK = 118,
      REMAIN_TRACK = 110,      

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
      // Transport Buttons
      //BTN_BACK = 30,
      //BTN_PREVIOUS = 31,
      //BTN_PLAY = 32,
      //BTN_PAUSE = 33,
      //BTN_STOP = 34,
      //BTN_NEXT = 35,
    }

    #region Properties

    public GUIMusicBaseWindow MusicWindow
    {
      set { _MusicWindow = value; }
    }

    #endregion

    [SkinControlAttribute((int)ControlIDs.LBL_CAPTION)]         protected GUILabelControl LblCaption = null;
    [SkinControlAttribute((int)ControlIDs.IMG_COVERART)]        protected GUIImage ImgCoverArt = null;
//    [SkinControlAttribute((int)ControlIDs.LBL_TRACK_NAME)]      protected GUIFadeLabel LblTrackName = null;
//    [SkinControlAttribute((int)ControlIDs.LBL_ALBUM_NAME)]      protected GUIFadeLabel LblAlbumName = null;
//    [SkinControlAttribute((int)ControlIDs.LBL_ALBUM_YEAR)]      protected GUILabelControl LblAlbumYear = null;
//    [SkinControlAttribute((int)ControlIDs.LBL_ARTIST_NAME)]     protected GUIFadeLabel LblArtistName = null;
    [SkinControlAttribute((int)ControlIDs.PROG_TRACK)]          protected GUIProgressControl ProgTrack = null;
    [SkinControlAttribute((int)ControlIDs.IMG_TRACK_PROGRESS_BG)] protected GUIImage ImgTrackProgressBkGrnd = null;
    [SkinControlAttribute((int)ControlIDs.LBL_UP_NEXT)]         protected GUILabelControl LblUpNext = null;
//    [SkinControlAttribute((int)ControlIDs.LBL_NEXT_TRACK_NAME)] protected GUIFadeLabel LblNextTrackName = null;
//    [SkinControlAttribute((int)ControlIDs.LBL_NEXT_ALBUM_NAME)] protected GUIFadeLabel LblNextAlbumName = null;
//    [SkinControlAttribute((int)ControlIDs.LBL_NEXT_ARTIST_NAME)]  protected GUIFadeLabel LblNextArtistName = null;

    //[SkinControlAttribute((int)ControlIDs.BTN_BACK)]    //protected GUIButtonControl BtnBack = null;
    //[SkinControlAttribute((int)ControlIDs.BTN_PREVIOUS)]    //protected GUIButtonControl BtnPrevious = null;
    //[SkinControlAttribute((int)ControlIDs.BTN_PLAY)]    //protected GUIButtonControl BtnPlay = null;
    //[SkinControlAttribute((int)ControlIDs.BTN_PAUSE)]    //protected GUIButtonControl BtnPause = null;
    //[SkinControlAttribute((int)ControlIDs.BTN_STOP)]    //protected GUIButtonControl BtnStop = null;
    //[SkinControlAttribute((int)ControlIDs.BTN_NEXT)]    //protected GUIButtonControl BtnNext = null;

//    [SkinControlAttribute((int)ControlIDs.IMGLIST_RATING)]      protected GUIImageList ImgListRating = null;
//    [SkinControlAttribute((int)ControlIDs.IMGLIST_NEXTRATING)]  protected GUIImageList ImgListNextRating = null;
    [SkinControlAttribute((int)ControlIDs.LIST_TAG_INFO)]       protected GUIListControl facadeTagInfo = null;
    [SkinControlAttribute((int)ControlIDs.LIST_ALBUM_INFO)]     protected GUIListControl facadeAlbumInfo = null;
    [SkinControlAttribute((int)ControlIDs.BEST_ALBUM_TRACKS)]   protected GUILabelControl LblBestAlbumTracks = null;
    [SkinControlAttribute((int)ControlIDs.BEST_TAG_TRACKS)]     protected GUILabelControl LblBestTagTracks = null;

    public enum TrackProgressType { Elapsed, CountDown };

    private bool ControlsInitialized = false;
    private PlayListPlayer PlaylistPlayer = null;
    private string CurrentThumbFileName = string.Empty;
    private string CurrentTrackFileName = string.Empty;
    private string NextTrackFileName = string.Empty;
    private MusicTag CurrentTrackTag = null;
    private MusicTag NextTrackTag = null;
    private DateTime LastUpdateTime = DateTime.Now;
    private TimeSpan UpdateInterval = new TimeSpan(0, 0, 1);
    private GUIMusicBaseWindow _MusicWindow = null;
    private AudioscrobblerUtils InfoScrobbler = null;    
    private Thread AlbumInfoThread;
    private Thread TagInfoThread;
    private bool UseID3 = false;
    private bool _trackChanged = true;


    public GUIMusicPlayingNow()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW;
      PlaylistPlayer = PlayListPlayer.SingletonPlayer;

      g_Player.PlayBackStarted += new g_Player.StartedHandler(g_Player_PlayBackStarted);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(g_Player_PlayBackEnded);

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        UseID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);
      }
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
      if (!ControlsInitialized)
        return;

      CurrentTrackFileName = filename;
      NextTrackFileName = PlaylistPlayer.GetNext();
      GetTrackTags();

      CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);

      if (CurrentThumbFileName.Length == 0)
        CurrentThumbFileName = GUIGraphicsContext.Skin + @"\media\missing_coverart.png";

      ImgCoverArt.SetFileName(CurrentThumbFileName);
      UpdateTrackInfo();
      UpdateTrackPosition();
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
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS:
          {
            //_currentPlaying = message.Label;
            if (GUIWindowManager.ActiveWindow == GetID)
            {
              //facadeAlbumInfo.OnMessage(message); // really needed?
              //facadeTagInfo.OnMessage(message);
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
      //facadeAlbumInfo.Focusable = false;
      //facadeTagInfo.Focusable = false;

      _trackChanged = true;

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

      InfoScrobbler = new AudioscrobblerUtils();

      g_Player_PlayBackStarted(g_Player.MediaType.Music, g_Player.CurrentFile);
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      base.OnPageDestroy(new_windowId);
      ControlsInitialized = false;
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
      dlg.AddLocalizedString(928);        // Find Coverart
      dlg.AddLocalizedString(4521);       // Show Album Info

      if (IsCdTrack(CurrentTrackFileName))
        dlg.AddLocalizedString(4554);   // Lookup CD info

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
        return;

      switch (dlg.SelectedId)
      {
        case 928:       // Find Coverart
          {
            string albumFolderPath = System.IO.Path.GetDirectoryName(CurrentTrackFileName);
            _MusicWindow.FindCoverArt(false, CurrentTrackTag.Artist, CurrentTrackTag.Album, albumFolderPath, CurrentTrackTag, -1);
            CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);

            if (CurrentThumbFileName.Length == 0)
              CurrentThumbFileName = GUIGraphicsContext.Skin + @"\media\missing_coverart.png";

            ImgCoverArt.SetFileName(CurrentThumbFileName);
            break;
          }

        case 4521:      // Show Album Info
          {
            string albumFolderPath = System.IO.Path.GetDirectoryName(CurrentTrackFileName);
            _MusicWindow.ShowAlbumInfo(GetID, false, CurrentTrackTag.Artist, CurrentTrackTag.Album, albumFolderPath, CurrentTrackTag, -1);
            break;
          }

        case 4554:      // Lookup CD info
          {
            MusicTag tag = new MusicTag();
            GetCDInfoFromFreeDB(CurrentTrackFileName, tag);
            GetTrackTags();
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

    private void StartTagInfoThread()
    {
      TagInfoThread = new Thread(new ThreadStart(UpdateTagInfoThread));
      // allow windows to kill the thread if the main app was closed
      TagInfoThread.IsBackground = true;
      TagInfoThread.Priority = ThreadPriority.BelowNormal;
      TagInfoThread.Start();
    }

    private void StartAlbumInfoThread()
    {
      AlbumInfoThread = new Thread(new ThreadStart(UpdateAlbumInfoThread));
      // allow windows to kill the thread if the main app was closed
      AlbumInfoThread.IsBackground = true;
      AlbumInfoThread.Priority = ThreadPriority.BelowNormal;
      AlbumInfoThread.Start();
    }

    private void UpdateTagInfoThread()
    {
      GUIListItem item = null;
      List<Song> TagTracks = new List<Song>();

      TagTracks = InfoScrobbler.getTagInfo(CurrentTrackTag.Artist, CurrentTrackTag.Title, true, false, true);

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
        if (i >= 2)
          break;
      }

      if (TagTracks.Count > 0)
      {
        if (LblBestTagTracks != null)
        {
          LblBestTagTracks.Label = GUILocalizeStrings.Get(33031) + TagTracks[0].Genre;
          LblBestTagTracks.Visible = true;
        }
        //GUIControl.FocusControl(GetID, ((int)ControlIDs.LIST_TAG_INFO));
      }
    }
    
    private void UpdateAlbumInfoThread()
    {
      GUIListItem item = null;
      List<Song> AlbumTracks = new List<Song>();

      AlbumTracks = InfoScrobbler.getAlbumInfo(CurrentTrackTag.Artist, CurrentTrackTag.Album, true);

      for (int i = 0; i < AlbumTracks.Count; i++)
      {
        item = new GUIListItem(AlbumTracks[i].ToShortString());
        item.Label = AlbumTracks[i].Title;
        item.Label2 = " (" + GUILocalizeStrings.Get(931) + ": " + Convert.ToString(AlbumTracks[i].TimesPlayed) + ")";

        item.MusicTag = BuildMusicTagFromSong(AlbumTracks[i]);

        facadeAlbumInfo.Add(item);

        // display 3 items only
        if (i >= 2)
          break;
      }

      if (AlbumTracks.Count > 0)
      {
        if (CurrentThumbFileName == GUIGraphicsContext.Skin + @"\media\missing_coverart.png")
        {
          CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);
          if (CurrentThumbFileName.Length > 0)
            ImgCoverArt.SetFileName(CurrentThumbFileName);
        }
        if (LblBestAlbumTracks != null)
          LblBestAlbumTracks.Visible = true;

        GUIControl.FocusControl(GetID, ((int)ControlIDs.LIST_ALBUM_INFO));
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

          if (CurrentTrackTag.Album != GUIPropertyManager.GetProperty("#Play.Current.Album") || facadeTagInfo.Count < 1)
          {
            InfoNeeded = true;
            if (LblBestAlbumTracks != null)
              LblBestAlbumTracks.Visible = false;
            if (LblBestTagTracks != null)
              LblBestTagTracks.Visible = false;
            facadeAlbumInfo.Clear();
            facadeTagInfo.Clear();
          }

          GUIPropertyManager.SetProperty("#Play.Current.Title", CurrentTrackTag.Title);
          GUIPropertyManager.SetProperty("#Play.Current.Album", CurrentTrackTag.Album);
          GUIPropertyManager.SetProperty("#Play.Current.Artist", CurrentTrackTag.Artist);
          GUIPropertyManager.SetProperty("#Play.Current.Genre", CurrentTrackTag.Genre);
          if (CurrentTrackTag.Year > 0)
            GUIPropertyManager.SetProperty("#Play.Current.Year", CurrentTrackTag.Year.ToString());
          else
            GUIPropertyManager.SetProperty("#Play.Current.Year", "");

          if (g_Player.Playing)
            GUIPropertyManager.SetProperty("#duration", Convert.ToString(g_Player.Duration));

          //if (ImgListNextRating != null)
          //{
          //  int rating = CurrentTrackTag.Rating;
          //  ImgListRating.Percentage = rating;
          //}
          if (InfoNeeded)
          {
            StartAlbumInfoThread();
            StartTagInfoThread();
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#Play.Current.Title", GUILocalizeStrings.Get(4543));
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
          GUIPropertyManager.SetProperty("#Play.Next.Title", NextTrackTag.Title);
          GUIPropertyManager.SetProperty("#Play.Next.Album", NextTrackTag.Album);
          GUIPropertyManager.SetProperty("#Play.Next.Artist", NextTrackTag.Artist);
          GUIPropertyManager.SetProperty("#Play.Next.Genre", NextTrackTag.Genre);
          if (NextTrackTag.Year > 0)
            GUIPropertyManager.SetProperty("#Play.Next.Year", NextTrackTag.Year.ToString());
          else
            GUIPropertyManager.SetProperty("#Play.Next.Year", String.Empty);
        }
        else
        {
          LblUpNext.Visible = false;
          GUIPropertyManager.SetProperty("#Play.Next.Title", String.Empty);
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
          if (CurrentTrackTag != null)
          {
            CurrentThumbFileName = GUIMusicFiles.GetCoverArt(false, CurrentTrackFileName, CurrentTrackTag);
            if (CurrentThumbFileName.Length == 0)
              CurrentThumbFileName = GUIGraphicsContext.Skin + @"\media\missing_coverart.png";
            ImgCoverArt.SetFileName(CurrentThumbFileName);
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
        tag.Album = song.Album;
        tag.Artist = song.Artist;
        tag.Duration = song.Duration;
        tag.Genre = song.Genre;
        tag.Title = song.Title;
        tag.Track = song.Track;
        tag.Year = song.Year;
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
          tag.Album = song.Album;
          tag.Artist = song.Artist;
          tag.Genre = song.Genre;
          tag.Duration = song.Duration;
          tag.Title = song.Title;
          tag.Track = song.Track;
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
  }
}