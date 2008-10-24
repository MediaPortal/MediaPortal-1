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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.TagReader;
using MediaPortal.Util;
using MediaPortal.Playlists;

namespace MediaPortal.GUI.RADIOLASTFM
{
  [PluginIcons("WindowPlugins.GUIRadioLastFM.BallonRadio.gif", "WindowPlugins.GUIRadioLastFM.BallonRadioDisabled.gif")]
  public class GUIRadioLastFM : GUIWindow, ISetupForm, IShowPlugin
  {
    #region Variables

    private enum SkinControlIDs
    {
      BTN_START_STREAM = 10,
      BTN_CHOOSE_TAG = 20,
      BTN_CHOOSE_FRIEND = 30,
      BTN_SUBMIT_PROFILE = 35,
      BTN_DISCOVERY_MODE = 40,
      LIST_TRACK_TAGS = 55,
      IMG_ARTIST_ART = 112,      
    }

    [SkinControlAttribute((int)SkinControlIDs.BTN_START_STREAM)]    protected GUIButtonControl btnStartStream = null;
    [SkinControlAttribute((int)SkinControlIDs.BTN_CHOOSE_TAG)]      protected GUIButtonControl btnChooseTag = null;
    [SkinControlAttribute((int)SkinControlIDs.BTN_CHOOSE_FRIEND)]   protected GUIButtonControl btnChooseFriend = null;
    [SkinControlAttribute((int)SkinControlIDs.BTN_SUBMIT_PROFILE)]  protected GUIToggleButtonControl btnSubmitProfile = null;
    [SkinControlAttribute((int)SkinControlIDs.BTN_DISCOVERY_MODE)]  protected GUIToggleButtonControl btnDiscoveryMode = null;
    //[SkinControlAttribute((int)SkinControlIDs.LIST_TRACK_TAGS)]     protected GUIListControl facadeTrackTags = null;
    [SkinControlAttribute((int)SkinControlIDs.IMG_ARTIST_ART)]      protected GUIImage imgArtistArt = null;

    private PlayListPlayer PlaylistPlayer = null;
    private AudioscrobblerUtils InfoScrobbler = null;
    private StreamControl LastFMStation = null;
    private NotifyIcon _trayBallonSongChange = null;
    private bool _configShowTrayIcon = true;
    private bool _configShowBallonTips = true;
    private bool _configDirectSkip = false;
    private int _configListEntryCount = 12;
    private List<string> _usersOwnTags = null;
    private List<string> _usersFriends = null;
    private List<Song> _radioTrackList = null;
    private ScrobblerUtilsRequest _lastTrackTagRequest;
    private ScrobblerUtilsRequest _lastArtistCoverRequest;
    private ScrobblerUtilsRequest _lastSimilarArtistRequest;
    private ScrobblerUtilsRequest _lastUsersTagsRequest;
    private ScrobblerUtilsRequest _lastUsersFriendsRequest;

    #endregion

    #region Constructor

    public GUIRadioLastFM()
    {
      GetID = (int)GUIWindow.Window.WINDOW_RADIO_LASTFM;
    }

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      GUIWaitCursor.Show();
      BackgroundWorker worker = new BackgroundWorker();
      worker.DoWork += new DoWorkEventHandler(Worker_LoadSettings);
      worker.RunWorkerAsync();
    }

    private void Worker_LoadSettings(object sender, DoWorkEventArgs e)
    {
      System.Threading.Thread.CurrentThread.Name = "LastFm";
      if (!LastFMStation.IsInit)
      {
        LastFMStation.LoadConfig();
        btnSubmitProfile.Selected = AudioscrobblerBase.SubmitRadioSongs;
      }
      else
        GUIWaitCursor.Hide();
    }

    //private void SaveSettings()
    //{
    //  using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
    //  {
    //  }
    //}

    #endregion

    #region BaseWindow Members

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\MyRadioLastFM.xml");

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _configShowTrayIcon = xmlreader.GetValueAsBool("audioscrobbler", "showtrayicon", true);
        _configShowBallonTips = xmlreader.GetValueAsBool("audioscrobbler", "showballontips", true);
        _configDirectSkip = xmlreader.GetValueAsBool("audioscrobbler", "directskip", false);
        _configListEntryCount = xmlreader.GetValueAsInt("audioscrobbler", "listentrycount", 12);
      }

      PlaylistPlayer = PlayListPlayer.SingletonPlayer;
      LastFMStation = new StreamControl();
      InfoScrobbler = AudioscrobblerUtils.Instance;
      _usersOwnTags = new List<string>();
      _usersFriends = new List<string>();
      _radioTrackList = new List<Song>();

      if (_configShowTrayIcon)
        InitTrayIcon();

      g_Player.PlayBackStarted += new g_Player.StartedHandler(PlayBackStartedHandler);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(PlayBackStoppedHandler);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(PlayBackEndedHandler);

      LastFMStation.RadioSettingsSuccess += new StreamControl.RadioSettingsLoaded(OnRadioSettingsSuccess);
      LastFMStation.RadioSettingsError += new StreamControl.RadioSettingsFailed(OnRadioSettingsError);

      BassMusicPlayer.Player.LastFMSync += new BassAudioEngine.LastFMSyncReceived(OnLastFMSyncReceived);

      LastFMStation.StreamSongChanged += new StreamControl.SongChangedHandler(OnLastFMStation_StreamSongChanged);

      return bResult;
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_NEXT_ITEM && (int)LastFMStation.CurrentStreamState > 2)
      {
        OnSkipHandler(false);
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      if (_trayBallonSongChange != null)
        _trayBallonSongChange.Visible = true;

      if (_usersOwnTags.Count < 1)
      {
        btnChooseTag.Disabled = true;
        btnChooseTag.Label = GUILocalizeStrings.Get(34030);
      }
      if (_usersFriends.Count < 1)
      {
        btnChooseFriend.Disabled = true;
        btnChooseFriend.Label = GUILocalizeStrings.Get(34031);
      }

      GUIPropertyManager.SetProperty("#trackduration", " ");
      string ThumbFileName = string.Empty;

      if (LastFMStation.CurrentTrackTag != null && LastFMStation.CurrentTrackTag.Artist != string.Empty)
      {
        // If we leave and reenter the plugin try to set the correct duration
        GUIPropertyManager.SetProperty("#trackduration", Util.Utils.SecondsToHMSString(LastFMStation.CurrentTrackTag.Duration));
        ThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, LastFMStation.CurrentTrackTag.Artist);
      }

      SetArtistThumb(ThumbFileName);
      LoadSettings();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if (_trayBallonSongChange != null)
        _trayBallonSongChange.Visible = false;

      base.OnPageDestroy(newWindowId);
    }

    public override void DeInit()
    {
      if (_trayBallonSongChange != null)
      {
        _trayBallonSongChange.Visible = false;
        _trayBallonSongChange = null;
      }
      if (_lastTrackTagRequest != null)
        InfoScrobbler.RemoveRequest(_lastTrackTagRequest);
      if (_lastArtistCoverRequest != null)
        InfoScrobbler.RemoveRequest(_lastArtistCoverRequest);
      if (_lastSimilarArtistRequest != null)
        InfoScrobbler.RemoveRequest(_lastSimilarArtistRequest);
      if (_lastUsersTagsRequest != null)
        InfoScrobbler.RemoveRequest(_lastUsersTagsRequest);
      if (_lastUsersFriendsRequest != null)
        InfoScrobbler.RemoveRequest(_lastUsersFriendsRequest);

      base.DeInit();
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnStartStream)
      {
        SetupRadioStream();
      }
      if (control == btnChooseTag)
      {
        OnSelectTag();
      }
      if (control == btnChooseFriend)
      {
        OnSelectFriend();
      }
      if (control == btnDiscoveryMode)
        LastFMStation.DiscoveryMode = btnDiscoveryMode.Selected;
      if (control == btnSubmitProfile)
        AudioscrobblerBase.SubmitRadioSongs = btnSubmitProfile.Selected;

      base.OnClicked(controlId, control, actionType);
    }

    /// <summary>
    /// Displays a virtual keyboard
    /// </summary>
    /// <param name="aDefaultText">a text which will be preselected</param>
    /// <returns>the text entered by the user</returns>
    private string GetInputFromUser(string aDefaultText)
    {
      string searchterm = String.Empty; //aDefaultText;
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (keyboard == null)
        return searchterm;
      
      keyboard.Reset();
      keyboard.Text = searchterm;
      keyboard.DoModal(GetID); // show it...
      searchterm = keyboard.Text;

      return searchterm;
    }

    private void OnSelectTag()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(33013); // tracks suiting configured tag

      dlg.Add(GUILocalizeStrings.Get(34060)); // Enter tag...
      foreach (string ownTag in _usersOwnTags)
        dlg.Add(ownTag);

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;

      // Open keyboard to enter own tags
      if (dlg.SelectedId == 1)
      {
        string SearchText = GetInputFromUser(btnChooseTag.Label);

        if (!String.IsNullOrEmpty(SearchText))
          btnChooseTag.Label = SearchText;
        else
          btnChooseTag.Label = GUILocalizeStrings.Get(34030);
      }
      else
        btnChooseTag.Label = _usersOwnTags[dlg.SelectedId - 2];

      GUIPropertyManager.SetProperty("#selecteditem", btnChooseTag.Label);
    }

    private void OnSelectFriend()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(33016); // tracks your friends like

      dlg.Add(GUILocalizeStrings.Get(34061)); // Enter a username...
      foreach (string Friend in _usersFriends)
        dlg.Add(Friend);

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;

      // Open keyboard to enter own friends
      if (dlg.SelectedId == 1)
      {
        string SearchText = GetInputFromUser(btnChooseFriend.Label);

        if (!String.IsNullOrEmpty(SearchText))
          btnChooseFriend.Label = SearchText;
        else
          btnChooseFriend.Label = GUILocalizeStrings.Get(34031);
      }
      else
        btnChooseFriend.Label = _usersFriends[dlg.SelectedId - 2];

      GUIPropertyManager.SetProperty("#selecteditem", btnChooseFriend.Label);
    }

    private void SetupRadioStream()
    {
      bool isSubscriber = LastFMStation.IsSubscriber;
      string desiredTag = string.Empty;
      string desiredFriend = string.Empty;
      StreamType TuneIntoSelected = LastFMStation.CurrentTuneType;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(34001);                   // Start Stream
      // 1
      dlg.Add(GUILocalizeStrings.Get(34040));  // Recommendation radio
      // 2
      dlg.Add("MediaPortal User's group radio");

      // 3
      if (btnChooseTag.Label != string.Empty)
        desiredTag = GUILocalizeStrings.Get(34041) + btnChooseTag.Label;  // Tune into chosen Tag: 
      else
        desiredTag = GUILocalizeStrings.Get(34042);                       // No tag has been chosen yet
      dlg.Add(desiredTag);

      // 4
      if (btnChooseFriend.Label != string.Empty)
        desiredFriend = GUILocalizeStrings.Get(34043) + btnChooseFriend.Label; // Personal radio of: 
      else
        desiredFriend = GUILocalizeStrings.Get(34045); // No Friend has been chosen yet
      dlg.Add(desiredFriend);

      // 5
      if (btnChooseFriend.Label != string.Empty)
        desiredFriend = GUILocalizeStrings.Get(34044) + btnChooseFriend.Label; // Loved tracks of: 
      else
        desiredFriend = GUILocalizeStrings.Get(34045); // No Friend has been chosen yet
      dlg.Add(desiredFriend);

      // 6
      dlg.Add(GUILocalizeStrings.Get(34048));      // My neighbour radio
      // 7
      dlg.Add(GUILocalizeStrings.Get(34049));      // My web playlist

      if (isSubscriber)
      {
        // 8
        dlg.Add(GUILocalizeStrings.Get(34046)); // My personal radio
        // 9
        dlg.Add(GUILocalizeStrings.Get(34047)); // My loved tracks
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;

      // dlg starts with 1...
      switch (dlg.SelectedId)
      {
        case 1:
          TuneIntoSelected = StreamType.Recommended;
          LastFMStation.StreamsUser = LastFMStation.AccountUser;
          break;
        case 2:
          TuneIntoSelected = StreamType.Group;
          LastFMStation.StreamsUser = "MediaPortal Users";
          break;
        case 3:
          // bail out if no tags available
          if (btnChooseTag.Label == GUILocalizeStrings.Get(34030))
            return;
          TuneIntoSelected = StreamType.Tags;
          break;
        case 4:
          // bail out if no friends have been made
          if (btnChooseFriend.Label == GUILocalizeStrings.Get(34031))
            return;
          TuneIntoSelected = StreamType.Personal;
          LastFMStation.StreamsUser = btnChooseFriend.Label;
          break;
        case 5:
          // bail out if no friends have been made
          if (btnChooseFriend.Label == GUILocalizeStrings.Get(34031))
            return;
          TuneIntoSelected = StreamType.Loved;
          LastFMStation.StreamsUser = btnChooseFriend.Label;
          break;
        case 6:
          TuneIntoSelected = StreamType.Neighbours;
          LastFMStation.StreamsUser = LastFMStation.AccountUser;
          break;
        case 7:
          TuneIntoSelected = StreamType.Playlist;
          LastFMStation.StreamsUser = LastFMStation.AccountUser;
          break;
        case 8:
          TuneIntoSelected = StreamType.Personal;
          LastFMStation.StreamsUser = LastFMStation.AccountUser;
          break;
        case 9:
          TuneIntoSelected = StreamType.Loved;
          LastFMStation.StreamsUser = LastFMStation.AccountUser;
          break;
        default:
          return;
      }

      if (LastFMStation.CurrentStreamState != StreamPlaybackState.offline)
      {
        //OnPlaybackStopped();
        g_Player.Stop();
      }

      switch (TuneIntoSelected)
      {
        case StreamType.Recommended:
          LastFMStation.TuneIntoRecommendedRadio(LastFMStation.StreamsUser);
          break;

        case StreamType.Group:
          LastFMStation.TuneIntoGroupRadio(LastFMStation.StreamsUser);
          break;

        case StreamType.Personal:
          LastFMStation.TuneIntoPersonalRadio(LastFMStation.StreamsUser);
          break;

        case StreamType.Loved:
          LastFMStation.TuneIntoLovedTracks(LastFMStation.StreamsUser);
          break;

        case StreamType.Tags:
          List<string> MyTags = new List<string>();
          try
          {
            Array SingleTags = btnChooseTag.Label.Split(new char[] { '.', ',' });
            foreach (string strTag in SingleTags)
            {
              MyTags.Add(strTag.Trim());
            }
          }
          catch (Exception ex)
          {
            Log.Warn("GUIRadioLastFM: Could not split given tags - {0}, {1}", btnChooseTag.Label, ex.Message);
            MyTags.Add(btnChooseTag.Label);
          }          
          //MyTags.Add("melodic death metal");
          LastFMStation.TuneIntoTags(MyTags);
          break;

        case StreamType.Neighbours:
          LastFMStation.TuneIntoNeighbourRadio(LastFMStation.StreamsUser);
          break;

        case StreamType.Playlist:
          LastFMStation.TuneIntoWebPlaylist(LastFMStation.StreamsUser);
          break;
      }

      if (LastFMStation.CurrentStreamState == StreamPlaybackState.initialized)
      {
        //if (LastFMStation.PlayStream())
        //{
        //  LastFMStation.CurrentPlaybackType = PlaybackType.Continuously;
        //}
        //else
        //{

        if (RebuildStreamList())
        {
          if (PlayPlayListStreams(_radioTrackList[0]))
          {
            LastFMStation.CurrentPlaybackType = PlaybackType.PlaylistPlayer;
            // Log.Warn("GUIRadio: Fallback to playlist mode was needed to start the stream.");
            return;
          }
        }
      }
      else
        Log.Info("GUIRadioLastFM: Didn't start LastFM radio because stream state is {0}", LastFMStation.CurrentStreamState.ToString());

      PlayBackFailedHandler();
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_SHOW_BALLONTIP_SONGCHANGE:
          if (_configShowBallonTips)
            ShowSongTrayBallon(message.Label, message.Label2, message.Param1, true);
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnShowContextMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
        return;

      dlg.Reset();
      dlg.SetHeading(498);                  // Menu

      dlg.AddLocalizedString(34010);        // Love
      dlg.AddLocalizedString(34011);        // Ban
      dlg.AddLocalizedString(34012);        // Skip

      if (LastFMStation.CurrentTrackTag != null)
        dlg.AddLocalizedString(33040);  // copy IRC spam

      dlg.AddLocalizedString(34015);    // Reload tags/friends

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
        return;

      switch (dlg.SelectedId)
      {
        case 34010:     // Love
          LastFMStation.SendControlCommand(StreamControls.lovetrack);
          AudioscrobblerBase.CurrentSong.AudioscrobblerAction = SongAction.L;
          break;
        case 34011:     // Ban
          LastFMStation.SendControlCommand(StreamControls.bantrack);
          AudioscrobblerBase.CurrentSong.AudioscrobblerAction = SongAction.B;
          OnSkipHandler(false);
          break;
        case 34012:     // Skip
          OnSkipHandler(false);
          break;
        case 33040:    // IRC spam          
          try
          {
            if (LastFMStation.CurrentTrackTag != null)
            {
              string tmpTrack = LastFMStation.CurrentTrackTag.Track > 0 ? (Convert.ToString(LastFMStation.CurrentTrackTag.Track) + ". ") : string.Empty;
              Clipboard.SetDataObject(@"/me is listening on last.fm: " + LastFMStation.CurrentTrackTag.Artist + " [" + LastFMStation.CurrentTrackTag.Album + "] - " + tmpTrack + LastFMStation.CurrentTrackTag.Title, true);
            }
          }
          catch (Exception ex)
          {
            Log.Error("GUIRadioLastFM: could not copy song spam to clipboard - {0}", ex.Message);
          }
          break;
        case 34015:
          btnChooseTag.Label = GUILocalizeStrings.Get(34030);
          btnChooseFriend.Label = GUILocalizeStrings.Get(34031);
          OnRadioSettingsSuccess();
          //UpdateUsersTags(LastFMStation.AccountUser);
          //UpdateUsersFriends(LastFMStation.AccountUser);
          break;
      }
    }

    #endregion

    #region Playlist functions

    private bool RebuildStreamList()
    {
      return GetXSPFPlaylist();
    }

    private bool AddStreamSongToPlaylist(ref Song song)
    {
      PlayList playlist = PlaylistPlayer.GetPlaylist(PlayListType.PLAYLIST_RADIO_STREAMS);
      if (playlist == null || song == null)
        return false;

      // We only want one item at each time since the links invalidate and therefore 
      // automatic advancement of playlist items does not make any sense.
      playlist.Clear();

      //add to playlist
      PlayListItem playlistItem = new PlayListItem();
      playlistItem.Type = Playlists.PlayListItem.PlayListItemType.Audio;
      StringBuilder sb = new StringBuilder();

      playlistItem.FileName = song.URL;

      sb.Append(song.Artist);
      sb.Append(" - ");
      sb.Append(song.Title);
      playlistItem.Description = sb.ToString();
      playlistItem.Duration = song.Duration;

      playlistItem.MusicTag = song.ToMusicTag();

      playlist.Add(playlistItem);

      return true;
    }

    public bool PlayPlayListStreams(Song aStreamSong)
    {
      GUIWaitCursor.Show();
      if (g_Player.Playing)
      {
        g_Player.Stop();
      }
      LastFMStation.CurrentStreamState = StreamPlaybackState.starting;

      Song addSong = aStreamSong.Clone();
      AddStreamSongToPlaylist(ref addSong);

      PlaylistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_RADIO_STREAMS;
      PlayList Playlist = PlaylistPlayer.GetPlaylist(PlaylistPlayer.CurrentPlaylistType);

      if (Playlist == null)
        return false;

      // Avoid an endless loop when a song cannot be played.
      PlaylistPlayer.RepeatPlaylist = false;

      // I found out, you have to send "Cookie: Session=[sessionID]" in the header of the request of the MP3 file. 
      PlaylistPlayer.Play(0);

      LastFMStation.CurrentStreamState = StreamPlaybackState.streaming;
      //LastFMStation.ToggleRecordToProfile(false);    
      LastFMStation.ToggleDiscoveryMode(LastFMStation.DiscoveryMode);

      AudioscrobblerBase.CurrentSong = aStreamSong;
      AudioscrobblerBase.CurrentSong.Source = SongSource.L;

      GUIWaitCursor.Hide();
      return g_Player.Playing;
    }

    #endregion

    #region Internet Lookups

    bool GetXSPFPlaylist()
    {
      _radioTrackList.Clear();
      _radioTrackList = InfoScrobbler.getRadioPlaylist(@"http://ws.audioscrobbler.com/radio/xspf.php?sk=" + AudioscrobblerBase.RadioSession + "&discovery=" + Convert.ToString(LastFMStation.DiscoveryEnabledInt) + "&desktop=1.3.1.1");

      Log.Debug("GUIRadioLastFM: Parsed XSPF Playlist for current radio stream");

      for (int i = 0; i < _radioTrackList.Count; i++)
      {
        Log.Debug("GUIRadioLastFM: Track {0} : {1}", Convert.ToString(i + 1), _radioTrackList[i].ToLastFMString());
      }

      return (_radioTrackList.Count > 0);
    }

    private void UpdateUsersFriends(string _serviceUser)
    {
      UsersFriendsRequest request = new UsersFriendsRequest(
              _serviceUser,
              new UsersFriendsRequest.UsersFriendsRequestHandler(OnUpdateUsersFriendsCompleted));
      _lastUsersFriendsRequest = request;
      InfoScrobbler.AddRequest(request);
    }

    private void UpdateUsersTags(string _serviceUser)
    {
      UsersTagsRequest request = new UsersTagsRequest(
              _serviceUser,
              new UsersTagsRequest.UsersTagsRequestHandler(OnUpdateUsersTagsCompleted));
      _lastUsersTagsRequest = request;
      InfoScrobbler.AddRequest(request);
    }

    private void UpdateArtistInfo(string _trackArtist)
    {
      if (_trackArtist == null)
        return;
      if (_trackArtist != string.Empty)
      {
        ArtistInfoRequest request = new ArtistInfoRequest(
                      _trackArtist,
                      new ArtistInfoRequest.ArtistInfoRequestHandler(OnUpdateArtistCoverCompleted));
        _lastArtistCoverRequest = request;
        InfoScrobbler.AddRequest(request);

        SimilarArtistRequest request2 = new SimilarArtistRequest(
                      _trackArtist,
                      false,
                      new SimilarArtistRequest.SimilarArtistRequestHandler(OnUpdateSimilarArtistsCompleted));
        _lastSimilarArtistRequest = request2;
        InfoScrobbler.AddRequest(request2);
      }
    }

    private void UpdateTrackTagsInfo(string _trackArtist, string _trackTitle)
    {
      TagsForTrackRequest request = new TagsForTrackRequest(
                      _trackArtist,
                      _trackTitle,
                      new TagsForTrackRequest.TagsForTrackRequestHandler(OnUpdateTrackTagsInfoCompleted));
      _lastTrackTagRequest = request;
      InfoScrobbler.AddRequest(request);
    }

    public void OnUpdateArtistCoverCompleted(ArtistInfoRequest request, Song song)
    {
      if (request.Equals(_lastArtistCoverRequest))
      {
        string ThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, LastFMStation.CurrentTrackTag.Artist);
        // If the download was unsuccessful or disabled in config then do not remove a possibly present placeholder by specifing a not existing file
        if (File.Exists(ThumbFileName))
        {
          SetArtistThumb(ThumbFileName);
        }
      }
      else
      {
        Log.Warn("NowPlaying.OnUpdateArtistInfoCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    public void OnUpdateSimilarArtistsCompleted(SimilarArtistRequest request2, List<Song> SimilarArtists)
    {
      if (request2.Equals(_lastSimilarArtistRequest))
      {
        string propertyTags = string.Empty;

        for (int i = 0; i < SimilarArtists.Count; i++)
        {
          // some artist names might be very long - reduce the number of tags then
          if (propertyTags.Length > 50)
            break;

          propertyTags += SimilarArtists[i].Artist + "   ";

          // display 5 items only
          if (i >= 4)
            break;
        }
        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.SimilarArtists", propertyTags);
      }
      else
      {
        Log.Warn("NowPlaying.OnUpdateSimilarArtistsCompleted: unexpected response for request: {0}", request2.Type);
      }
    }

    public void OnUpdateTrackTagsInfoCompleted(TagsForTrackRequest request, List<Song> TagTracks)
    {
      if (request.Equals(_lastTrackTagRequest))
      {
        string propertyTags = string.Empty;

        for (int i = 0; i < TagTracks.Count; i++)
        {
          // some tags might be very long - reduce the number of tags then
          if (propertyTags.Length > 50)
            break;

          propertyTags += TagTracks[i].Genre + "   ";

          // display 5 items only
          if (i >= 4)
            break;
        }
        GUIPropertyManager.SetProperty("#Play.Current.Lastfm.TrackTags", propertyTags);
      }
      else
      {
        Log.Warn("NowPlaying.OnUpdateTrackTagsInfoCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    public void OnUpdateUsersTagsCompleted(UsersTagsRequest request, List<Song> FeedItems)
    {
      if (request.Equals(_lastUsersTagsRequest))
      {
        if (_usersOwnTags != null)
          _usersOwnTags.Clear();
        for (int i = 0; i < FeedItems.Count; i++)
        {
          _usersOwnTags.Add(FeedItems[i].Artist);
          if (i == _configListEntryCount - 1)
            break;
        }
        if (_usersOwnTags.Count > 0)
        {
          btnChooseTag.Disabled = false;
          btnChooseTag.Label = _usersOwnTags[0];
        }
      }
      else
        Log.Warn("NowPlaying.OnUpdateUsersTagsCompleted: unexpected response for request: {0}", request.Type);
    }

    public void OnUpdateUsersFriendsCompleted(UsersFriendsRequest request, List<Song> FeedItems)
    {
      if (request.Equals(_lastUsersFriendsRequest))
      {
        if (_usersFriends != null)
          _usersFriends.Clear();
        for (int i = 0; i < FeedItems.Count; i++)
        {
          _usersFriends.Add(FeedItems[i].Artist);
          if (i == _configListEntryCount - 1)
            break;
        }
        if (_usersFriends.Count > 0)
        {
          btnChooseFriend.Disabled = false;
          btnChooseFriend.Label = _usersFriends[0];
        }
      }
      else
        Log.Warn("NowPlaying.OnUpdateUsersFriendsCompleted: unexpected response for request: {0}", request.Type);
    }

    private void OnPlaybackStopped()
    {
      LastFMStation.CurrentTrackTag.Clear();
      LastFMStation.CurrentStreamState = StreamPlaybackState.initialized;

      SetArtistThumb(string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.TrackTags", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.SimilarArtists", string.Empty);
      GUIPropertyManager.SetProperty("#trackduration", " ");
      GUIPropertyManager.SetProperty("#currentplaytime", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", string.Empty);

      //reset the TrayIcon
      ShowSongTrayBallon(GUILocalizeStrings.Get(34050), " ", 1, false); // Stream stopped
    }

    #endregion

    #region Handlers

    private void OnRadioSettingsSuccess()
    {
      UpdateUsersTags(LastFMStation.AccountUser);
      UpdateUsersFriends(LastFMStation.AccountUser);
      GUIWaitCursor.Hide();

      btnDiscoveryMode.Disabled = !LastFMStation.IsSubscriber;
      btnDiscoveryMode.Visible = true;
      btnStartStream.Selected = true;
    }

    private void OnRadioSettingsError()
    {
      GUIWaitCursor.Hide();

      GUIDialogOK msgdlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (msgdlg == null)
        return;
      msgdlg.SetHeading(GUILocalizeStrings.Get(34054)); // Radio handshake failed!
      msgdlg.SetLine(1, GUILocalizeStrings.Get(34055)); // Streams might be temporarily unavailable
      msgdlg.DoModal(GetID);

      btnDiscoveryMode.Disabled = true;
      btnStartStream.Selected = false;
    }

    private void OnLastFMSyncReceived(object trash, DateTime syncTime)
    {
      SetArtistThumb(string.Empty);
      GUITextureManager.CleanupThumbs();
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.TrackTags", string.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.SimilarArtists", string.Empty);

      LastFMStation.UpdateNowPlaying(false);
    }

    private void OnSkipHandler(bool directSkip)
    {
      if (_radioTrackList.Count < 1)
        RebuildStreamList();

      if (_radioTrackList.Count > 0)
      {
        if (_configDirectSkip || directSkip)
          PlayPlayListStreams(_radioTrackList[0]);
        else
        {
          GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (dlg == null)
            return;
          dlg.Reset();
          dlg.SetHeading(498); // menu
          foreach (Song Track in _radioTrackList)
            dlg.Add(Track.ToLastFMString());

          dlg.DoModal(GetID);
          if (dlg.SelectedId == -1)
            return;

          PlayPlayListStreams(_radioTrackList[dlg.SelectedId - 1]);
        }
        LastFMStation.CurrentPlaybackType = PlaybackType.PlaylistPlayer;
      }
      else
        Log.Warn("GUIRadioLastFM: OnSkipHandler - No tracks to choose from!");
    }

    private void OnLastFMStation_StreamSongChanged(MusicTag newCurrentSong, DateTime startTime)
    {
      if (_lastTrackTagRequest != null)
        InfoScrobbler.RemoveRequest(_lastTrackTagRequest);
      if (_lastArtistCoverRequest != null)
        InfoScrobbler.RemoveRequest(_lastArtistCoverRequest);

      if (LastFMStation.CurrentTrackTag != null)
      {
        if (LastFMStation.CurrentTrackTag.Artist != string.Empty)
        {
          UpdateArtistInfo(newCurrentSong.Artist);

          if (LastFMStation.CurrentTrackTag.Title != string.Empty)
            UpdateTrackTagsInfo(LastFMStation.CurrentTrackTag.Artist, LastFMStation.CurrentTrackTag.Title);
        }

        RebuildStreamList();
      }
    }

    private void PlayBackStartedHandler(g_Player.MediaType type, string filename)
    {
      if (!Util.Utils.IsLastFMStream(filename) || (LastFMStation.CurrentStreamState != StreamPlaybackState.streaming && LastFMStation.CurrentStreamState != StreamPlaybackState.starting))
        return;

      Log.Debug("GUIRadioLastFM: PlayBackStartedHandler for {0} - sending sync", filename);
      OnLastFMSyncReceived(null, DateTime.Now);
    }

    private void PlayBackStoppedHandler(g_Player.MediaType type, int stoptime, string filename)
    {
      if (!String.IsNullOrEmpty(filename))
      {
        if (!Util.Utils.IsLastFMStream(filename) || LastFMStation.CurrentStreamState != StreamPlaybackState.streaming)
          return;
      }
      OnPlaybackStopped();
    }

    private void PlayBackEndedHandler(g_Player.MediaType type, string filename)
    {
      if (!String.IsNullOrEmpty(filename))
      {
        if (!Util.Utils.IsLastFMStream(filename) || LastFMStation.CurrentStreamState != StreamPlaybackState.streaming)
          return;
      }

      Log.Debug("GUIRadioLastFM: PlayBackEnded for this selection - trying restart...");
      if (PlayPlayListStreams(_radioTrackList[0]))
      {
        LastFMStation.CurrentPlaybackType = PlaybackType.PlaylistPlayer;
        return;
      }

      OnPlaybackStopped();

      ShowSongTrayBallon(GUILocalizeStrings.Get(34051), GUILocalizeStrings.Get(34052), 15, true); // Stream ended, No more content or bad connection

      Log.Info("GUIRadioLastFM: No more content for this selection or interrupted stream..");
      LastFMStation.CurrentStreamState = StreamPlaybackState.nocontent;
      //dlg.AddLocalizedString(930);        //Add to favorites
    }

    private void PlayBackFailedHandler()
    {
      GUIDialogOK msgdlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (msgdlg == null)
        return;
      msgdlg.SetHeading(34050); // No stream active
      msgdlg.SetLine(1, GUILocalizeStrings.Get(34053)); // Playback of selected stream failed
      msgdlg.DoModal(GetID);
    }

    #endregion

    #region Utils

    private void ShowSongTrayBallon(string notifyTitle, string notifyMessage_, int showSeconds_, bool popup_)
    {
      if (_trayBallonSongChange != null)
      {
        // Length may only be 64 chars
        if (notifyTitle.Length > 63)
          notifyTitle = notifyTitle.Remove(63);
        if (notifyMessage_.Length > 63)
          notifyMessage_ = notifyMessage_.Remove(63);

        // XP hides "inactive" icons therefore change the text
        string IconText = "MP Last.fm radio\n" + notifyMessage_ + " - " + notifyTitle;
        if (IconText.Length > 63)
          IconText = IconText.Remove(60) + "..";
        _trayBallonSongChange.Text = IconText;
        _trayBallonSongChange.Visible = true;

        if (notifyTitle == string.Empty)
          notifyTitle = "MediaPortal";
        _trayBallonSongChange.BalloonTipTitle = notifyTitle;
        if (notifyMessage_ == string.Empty)
          notifyMessage_ = IconText;
        _trayBallonSongChange.BalloonTipText = notifyMessage_;
        if (popup_)
          _trayBallonSongChange.ShowBalloonTip(showSeconds_);
      }
    }

    private void InitTrayIcon()
    {
      if (_trayBallonSongChange == null)
      {
        ContextMenu contextMenuLastFM = new ContextMenu();
        MenuItem menuItem1 = new MenuItem();
        MenuItem menuItem2 = new MenuItem();
        MenuItem menuItem3 = new MenuItem();

        // Initialize contextMenuLastFM
        contextMenuLastFM.MenuItems.AddRange(new MenuItem[] { menuItem1, menuItem2, menuItem3 });

        // Initialize menuItem1
        menuItem1.Index = 0;
        menuItem1.Text = GUILocalizeStrings.Get(34010); // Love
        menuItem1.Click += new System.EventHandler(Tray_menuItem1_Click);
        // Initialize menuItem2
        menuItem2.Index = 1;
        menuItem2.Text = GUILocalizeStrings.Get(34011); // Ban
        menuItem2.Click += new System.EventHandler(Tray_menuItem2_Click);
        // Initialize menuItem3
        menuItem3.Index = 2;
        menuItem3.Text = GUILocalizeStrings.Get(34012); // Skip
        //menuItem3.Break = true;
        menuItem3.DefaultItem = true;
        menuItem3.Click += new System.EventHandler(Tray_menuItem3_Click);

        _trayBallonSongChange = new NotifyIcon();
        _trayBallonSongChange.ContextMenu = contextMenuLastFM;

        if (System.IO.File.Exists(Config.GetFile(Config.Dir.Base, @"BallonRadio.ico")))
          _trayBallonSongChange.Icon = new Icon(Config.GetFile(Config.Dir.Base, @"BallonRadio.ico"));
        else
          _trayBallonSongChange.Icon = SystemIcons.Information;

        _trayBallonSongChange.Text = "MediaPortal Last.fm Radio";
        _trayBallonSongChange.Visible = false;
      }
    }

    // skip
    private void Tray_menuItem1_Click(object Sender, EventArgs e)
    {
      if ((int)LastFMStation.CurrentStreamState > 2)
      {
        LastFMStation.SendControlCommand(StreamControls.lovetrack);
      }
    }

    // ban
    private void Tray_menuItem2_Click(object Sender, EventArgs e)
    {
      if ((int)LastFMStation.CurrentStreamState > 2)
      {
        LastFMStation.SendControlCommand(StreamControls.bantrack);
      }
    }

    // love
    private void Tray_menuItem3_Click(object Sender, EventArgs e)
    {
      if ((int)LastFMStation.CurrentStreamState > 2)
      {
        OnSkipHandler(true);
      }
    }

    private void SetArtistThumb(string artistThumbPath_)
    {
      string thumb = artistThumbPath_;

      if (thumb.Length <= 0)
        thumb = GUIGraphicsContext.Skin + @"\media\missing_coverart.png";
      else
      {
        // let us test if there is a larger cover art image
        string strLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(thumb);
        if (System.IO.File.Exists(strLarge))
        {
          thumb = strLarge;
        }
      }

      //string refString = string.Empty;
      //Util.Utils.GetQualifiedFilename(thumb, ref refString);
      GUIPropertyManager.SetProperty("#Play.Current.ArtistThumb", thumb);

      if (imgArtistArt != null)
      {
        imgArtistArt.SetFileName(thumb);
        imgArtistArt.FreeResources();
        imgArtistArt.AllocResources();
      }
    }

    #endregion

    #region ISetupForm Members

    public int GetWindowId()
    {
      return GetID;
    }

    public string PluginName()
    {
      return "Last.fm Radio";
    }

    public string Description()
    {
      return "Listen to radio streams on last.fm";
    }

    public string Author()
    {
      return "rtv";
    }

    public bool CanEnable()
    {
      //bool AudioScrobblerOn = false;
      //using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      //{
      //  AudioScrobblerOn = xmlreader.GetValueAsBool("plugins", "Audioscrobbler", false);
      //}
      //return AudioScrobblerOn;
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(34000);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = "hover_my radio.png"; //hover_LastFmRadio.png
      return true;
    }

    // show the setup dialog
    public void ShowPlugin()
    {
      PluginSetupForm lastfmsetup = new PluginSetupForm();
      lastfmsetup.ShowDialog();
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion
  }
}