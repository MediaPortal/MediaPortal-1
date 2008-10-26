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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
      BTN_CHOOSE_ARTIST = 15,
      BTN_CHOOSE_TAG = 20,
      BTN_CHOOSE_FRIEND = 30,
      BTN_SUBMIT_PROFILE = 35,
      BTN_DISCOVERY_MODE = 40,
      LIST_TRACK_TAGS = 55,
      IMG_ARTIST_ART = 112,
    }

    [SkinControlAttribute((int)SkinControlIDs.BTN_START_STREAM)]    protected GUIButtonControl btnStartStream = null;
    [SkinControlAttribute((int)SkinControlIDs.BTN_CHOOSE_ARTIST)]   protected GUIButtonControl btnChooseArtist = null;
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
    private List<string> _usersTopArtists = null;
    private List<string> _usersOwnTags = null;
    private List<string> _usersFriends = null;
    private List<string> _similarArtistCache = null;
    private List<string> _trackTagsCache = null;
    private List<Song> _radioTrackList = null;
    private Song _streamSong = null;
    private ScrobblerUtilsRequest _lastTrackTagRequest;
    private ScrobblerUtilsRequest _lastArtistCoverRequest;
    private ScrobblerUtilsRequest _lastSimilarArtistRequest;
    private ScrobblerUtilsRequest _lastUsersTopArtistsRequest;
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
      Thread LoadThread = new Thread(new ThreadStart(Worker_LoadSettings));
      LoadThread.IsBackground = true;
      LoadThread.Priority = ThreadPriority.AboveNormal;
      LoadThread.Name = "Last.fm init";
      LoadThread.Start();
    }

    private void Worker_LoadSettings()
    {
      GUIWaitCursor.Show();
      if (!LastFMStation.IsInit || LastFMStation.AccountUser != AudioscrobblerBase.Username)
      {
        LastFMStation.LoadConfig();
        btnSubmitProfile.Selected = AudioscrobblerBase.IsSubmittingRadioSongs;
      }
      else
        GUIWaitCursor.Hide();
    }

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
      _usersTopArtists = new List<string>(_configListEntryCount);
      _usersOwnTags = new List<string>(_configListEntryCount);
      _usersFriends = new List<string>(_configListEntryCount);
      _radioTrackList = new List<Song>(5);
      _similarArtistCache = new List<string>(5);
      _trackTagsCache = new List<string>(5);
      _streamSong = new Song();

      if (_configShowTrayIcon)
        InitTrayIcon();

      g_Player.PlayBackStarted += new g_Player.StartedHandler(PlayBackStartedHandler);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(PlayBackStoppedHandler);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(PlayBackEndedHandler);

      LastFMStation.RadioSettingsSuccess += new StreamControl.RadioSettingsLoaded(OnRadioSettingsSuccess);
      LastFMStation.RadioSettingsError += new StreamControl.RadioSettingsFailed(OnRadioSettingsError);

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

      if (_usersTopArtists.Count < 1)
        btnChooseArtist.Label = GUILocalizeStrings.Get(34032);

      if (_usersOwnTags.Count < 1)
      {
        // btnChooseTag.Disabled = true;
        btnChooseTag.Label = GUILocalizeStrings.Get(34030);
      }
      if (_usersFriends.Count < 1)
      {
        // btnChooseFriend.Disabled = true;
        btnChooseFriend.Label = GUILocalizeStrings.Get(34031);
      }

      GUIPropertyManager.SetProperty("#trackduration", " ");
      string ThumbFileName = String.Empty;

      if (AudioscrobblerBase.CurrentPlayingSong != null && AudioscrobblerBase.CurrentPlayingSong.Artist != String.Empty)
      {
        // If we leave and reenter the plugin try to set the correct duration
        GUIPropertyManager.SetProperty("#trackduration", Util.Utils.SecondsToHMSString(AudioscrobblerBase.CurrentPlayingSong.Duration));
        ThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, AudioscrobblerBase.CurrentPlayingSong.Artist);
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
      if (_lastUsersTopArtistsRequest != null)
        InfoScrobbler.RemoveRequest(_lastUsersTopArtistsRequest);
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
      if (control == btnChooseArtist)
      {
        OnSelectArtist();
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
        AudioscrobblerBase.IsSubmittingRadioSongs = btnSubmitProfile.Selected;

      base.OnClicked(controlId, control, actionType);
    }

    /// <summary>
    /// Displays a virtual keyboard
    /// </summary>
    /// <param name="aDefaultText">a text which will be preselected</param>
    /// <returns>the text entered by the user</returns>
    private string GetInputFromUser(string aDefaultText)
    {
      string searchterm = aDefaultText;
      try
      {
        VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
        if (keyboard == null)
          return searchterm;

        keyboard.Reset();
        keyboard.Text = searchterm;
        keyboard.DoModal(GetID); // show it...
        searchterm = keyboard.Text;
      }
      catch (Exception kex)
      {
        Log.Error("GUIRadioLastFM: VirtualKeyboard error - {0}", kex.Message);
      }
      return searchterm;
    }

    private void OnSelectArtist()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(34039); // Tune into chosen artist(s): 

      dlg.Add(GUILocalizeStrings.Get(34062)); // Enter artist(s)...
      for (int i = 0; i < _usersTopArtists.Count; i++)
      {
        dlg.Add(_usersTopArtists[i]);
        // position on the previous tag
        if (_usersTopArtists[i] == btnChooseArtist.Label)
          dlg.SelectedLabel = i + 1; // that dialog starts with 1!? and we've added a manual item as well
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;

      // Open keyboard to enter own tags
      if (dlg.SelectedId == 1)
      {
        string SearchText = GetInputFromUser(btnChooseArtist.Label);

        if (!String.IsNullOrEmpty(SearchText))
          btnChooseArtist.Label = SearchText;
        else
          btnChooseArtist.Label = GUILocalizeStrings.Get(34032); // No artists
      }
      else
        btnChooseArtist.Label = _usersTopArtists[dlg.SelectedId - 2];

      GUIPropertyManager.SetProperty("#selecteditem", btnChooseArtist.Label);
    }

    private void OnSelectTag()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(33013); // tracks suiting configured tag

      dlg.Add(GUILocalizeStrings.Get(34060)); // Enter tag...
      for (int i = 0; i < _usersOwnTags.Count; i++)
      {
        dlg.Add(_usersOwnTags[i]);
        // position on the previous tag
        if (_usersOwnTags[i] == btnChooseTag.Label)
          dlg.SelectedLabel = i + 1; // that dialog starts with 1!? and we've added a manual item as well
      }

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
      for (int i = 0; i < _usersFriends.Count; i++)
      {
        dlg.Add(_usersFriends[i]);
        // position on the previous tag
        if (_usersFriends[i] == btnChooseFriend.Label)
          dlg.SelectedLabel = i + 1; // that dialog starts with 1!? and we've added a manual item as well
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;

      // Open keyboard to enter own friends
      if (dlg.SelectedId == 1)
      {
        string SearchText = GetInputFromUser(String.Empty); // (btnChooseFriend.Label);

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
      string desiredArtist = String.Empty;
      string desiredTag = String.Empty;
      string desiredFriend = String.Empty;      
      StreamType TuneIntoSelected = LastFMStation.CurrentTuneType;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(34001);                   // Start Stream
      
      dlg.Add(GUILocalizeStrings.Get(34040));  // 1 - Recommendation radio

      if (btnChooseArtist.Label != String.Empty)
        desiredArtist = GUILocalizeStrings.Get(34039) + btnChooseArtist.Label; // 2 - Tune into chosen artist(s)
      else
        desiredTag = GUILocalizeStrings.Get(34063);                            // No artist has been chosen yet
      dlg.Add(desiredArtist);

      if (btnChooseTag.Label != String.Empty)
        desiredTag = GUILocalizeStrings.Get(34041) + btnChooseTag.Label;       // 3 - Tune into chosen Tag: 
      else
        desiredTag = GUILocalizeStrings.Get(34042);                            // No tag has been chosen yet
      dlg.Add(desiredTag);

      if (btnChooseFriend.Label != String.Empty)
        desiredFriend = GUILocalizeStrings.Get(34043) + btnChooseFriend.Label; // 4 - Personal radio of: 
      else
        desiredFriend = GUILocalizeStrings.Get(34045);                         // No Friend has been chosen yet
      dlg.Add(desiredFriend);

      if (btnChooseFriend.Label != String.Empty)
        desiredFriend = GUILocalizeStrings.Get(34044) + btnChooseFriend.Label; // 5 - Loved tracks of: 
      else
        desiredFriend = GUILocalizeStrings.Get(34045);                         // No Friend has been chosen yet
      dlg.Add(desiredFriend);

      dlg.Add(GUILocalizeStrings.Get(34048));      // 6 - My neighbour radio
      dlg.Add(GUILocalizeStrings.Get(34049));      // 7 - My web playlist
      if (isSubscriber)
      {
        dlg.Add(GUILocalizeStrings.Get(34046));    // 8 - My personal radio
        dlg.Add(GUILocalizeStrings.Get(34047));    // 9 - My loved tracks
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
          // bail out if no artists available
          if (btnChooseArtist.Label == GUILocalizeStrings.Get(34032))
            return;
          TuneIntoSelected = StreamType.Artist;
          LastFMStation.StreamsUser = LastFMStation.AccountUser;
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

        case StreamType.Artist:
          LastFMStation.TuneIntoArtists(BuildListFromString(btnChooseArtist.Label));
          break;

        case StreamType.Personal:
          LastFMStation.TuneIntoPersonalRadio(LastFMStation.StreamsUser);
          break;

        case StreamType.Loved:
          LastFMStation.TuneIntoLovedTracks(LastFMStation.StreamsUser);
          break;

        case StreamType.Tags:          
          LastFMStation.TuneIntoTags(BuildListFromString(btnChooseTag.Label));
          break;

        case StreamType.Neighbours:
          LastFMStation.TuneIntoNeighbourRadio(LastFMStation.StreamsUser);
          break;

        case StreamType.Playlist:
          LastFMStation.TuneIntoWebPlaylist(LastFMStation.StreamsUser);
          break;
      }
      //// The official client does also fetch 2 lists...
      //RebuildStreamList();
      StartPlaybackNow();
    }

    private void StartPlaybackNow()
    {
      if (g_Player.Playing)
      {
        Log.Debug("GUIRadioLastFM: StartPlaybackNow is stopping current playback");
        g_Player.Stop();
      }

      //if (LastFMStation.CurrentStreamState == StreamPlaybackState.initialized || LastFMStation.CurrentStreamState == StreamPlaybackState.nocontent)
      //{
      if (RebuildStreamList())
      {
        if (PlayPlayListStreams(_radioTrackList[0]))
        {
          LastFMStation.CurrentPlaybackType = PlaybackType.PlaylistPlayer;
          return;
        }
        else
          PlayBackFailedHandler();
      }
      else
        PlayBackNoMoreContentHandler();
      //}
      //else
      //  Log.Info("GUIRadioLastFM: Didn't start LastFM radio because stream state is {0}", LastFMStation.CurrentStreamState.ToString());
    }

    private string BuildStringFromList(List<string> aList)
    {
      string result = String.Empty;
      foreach (string singleString in aList)
      {
        result += singleString + " ";
      }
      result = result.TrimEnd(new char[] { ' ' });
      return result;
    }

    private List<string> BuildListFromString(string aConcatString)
    {
      List<string> ResultList = new List<string>();
      try
      {
        Array SingleStrings = aConcatString.Split(new char[] { '.', ',' });
        foreach (string strTag in SingleStrings)
        {
          ResultList.Add(strTag.Trim());
        }
      }
      catch (Exception ex)
      {
        Log.Warn("GUIRadioLastFM: Could not split given strings - {0}, {1}", aConcatString, ex.Message);
        ResultList.Add(aConcatString);
      }
      return ResultList;
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

      if ((int)LastFMStation.CurrentStreamState < 3)
        dlg.AddLocalizedString(34001);
      else // Only show these options while playing something      
      {
        dlg.AddLocalizedString(34010);        // Love
        dlg.AddLocalizedString(34011);        // Ban
        dlg.AddLocalizedString(34012);        // Skip

        if (AudioscrobblerBase.CurrentPlayingSong != null)
          dlg.AddLocalizedString(33040);  // copy IRC spam
        if (_similarArtistCache.Count > 0)
          dlg.AddLocalizedString(34016);    // Play current similar artist(s)
        if (_trackTagsCache.Count > 0)
          dlg.AddLocalizedString(34017);  // Play current similar tag(s)
      }
      dlg.AddLocalizedString(34015);    // Reload tags/friends

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
        return;

      switch (dlg.SelectedId)
      {
        case 34001:
          SetupRadioStream();
          break;        // Start / Switch stream
        case 34010:     // Love
          OnLoveClicked();
          break;
        case 34011:     // Ban
          OnBanClicked();
          break;
        case 34012:     // Skip
          OnSkipClicked();
          break;
        case 33040:    // IRC spam          
          try
          {
            if (AudioscrobblerBase.CurrentPlayingSong != null)
            {
              string tmpTrack = AudioscrobblerBase.CurrentPlayingSong.Track > 0 ? (Convert.ToString(AudioscrobblerBase.CurrentPlayingSong.Track) + ". ") : String.Empty;
              Clipboard.SetDataObject(@"/me is listening on last.fm: " + AudioscrobblerBase.CurrentPlayingSong.Artist + " [" + AudioscrobblerBase.CurrentPlayingSong.Album + "] - " + tmpTrack + AudioscrobblerBase.CurrentPlayingSong.Title, true);
            }
          }
          catch (Exception ex)
          {
            Log.Error("GUIRadioLastFM: could not copy song spam to clipboard - {0}", ex.Message);
          }
          break;
        case 34016:   // similar artists
          OnPlaySimilarArtistsClicked();
          break;
        case 34017:   // similar tags
          OnPlaySimilarTagsClicked();
          break;
        case 34015:
          btnChooseTag.Label = GUILocalizeStrings.Get(34030);
          btnChooseFriend.Label = GUILocalizeStrings.Get(34031);
          OnRadioSettingsSuccess();
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

      _streamSong = aStreamSong;

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

      GUIWaitCursor.Hide();
      return g_Player.Playing;
    }

    #endregion

    #region Internet Lookups

    bool GetXSPFPlaylist()
    {
      _radioTrackList.Clear();
      _radioTrackList = InfoScrobbler.getRadioPlaylist(@"http://ws.audioscrobbler.com/radio/xspf.php?sk=" + AudioscrobblerBase.RadioSession + "&discovery=" + Convert.ToString(LastFMStation.DiscoveryEnabledInt) + "&desktop=" + AudioscrobblerBase.ClientFakeVersion);

      Log.Debug("GUIRadioLastFM: Parsed XSPF Playlist for current radio stream");

      for (int i = 0; i < _radioTrackList.Count; i++)
      {
        Log.Debug("GUIRadioLastFM: Track {0} : {1}", Convert.ToString(i + 1), _radioTrackList[i].ToLastFMString());
      }

      return (_radioTrackList.Count > 0);
    }

    private void UpdateUsersTopArtists(string _serviceUser)
    {
      GeneralFeedRequest request = new GeneralFeedRequest(
        lastFMFeed.topartists,
        _serviceUser,
        new GeneralFeedRequest.GeneralFeedRequestHandler(OnUpdateUsersTopArtistsCompleted));
      _lastUsersTopArtistsRequest = request;
      InfoScrobbler.AddRequest(request);
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
      if (_trackArtist != String.Empty)
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
        _similarArtistCache.Clear();
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
      _trackTagsCache.Clear();
      InfoScrobbler.AddRequest(request);
    }

    public void OnUpdateArtistCoverCompleted(ArtistInfoRequest request, Song song)
    {
      if (request.Equals(_lastArtistCoverRequest))
      {
        string ThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, AudioscrobblerBase.CurrentPlayingSong.Artist);
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
        string propertyTags = String.Empty;

        for (int i = 0; i < SimilarArtists.Count; i++)
        {
          // some artist names might be very long - reduce the number of tags then
          if (propertyTags.Length > 50)
            break;

          propertyTags += SimilarArtists[i].Artist + "   ";
          _similarArtistCache.Add(SimilarArtists[i].Artist);

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
        string propertyTags = String.Empty;

        for (int i = 0; i < TagTracks.Count; i++)
        {
          // some tags might be very long - reduce the number of tags then
          if (propertyTags.Length > 50)
            break;

          propertyTags += TagTracks[i].Genre + "   ";
          _trackTagsCache.Add(TagTracks[i].Genre);

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

    public void OnUpdateUsersTopArtistsCompleted(GeneralFeedRequest request, List<Song> FeedItems)
    {
      if (request.Equals(_lastUsersTopArtistsRequest))
      {
        if (_usersTopArtists != null)
          _usersTopArtists.Clear();
        for (int i = 0; i < FeedItems.Count; i++)
        {
          _usersTopArtists.Add(FeedItems[i].Artist);
          if (i == _configListEntryCount - 1)
            break;
        }
        if (_usersTopArtists.Count > 0)
        {
          // btnChooseArtist.Disabled = false;
          btnChooseArtist.Label = _usersTopArtists[0];
        }
      }
      else
        Log.Warn("NowPlaying.OnUpdateUsersTopArtistsCompleted: unexpected response for request: {0}", request.Type);
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
          // btnChooseTag.Disabled = false;
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
          // btnChooseFriend.Disabled = false;
          btnChooseFriend.Label = _usersFriends[0];
        }
      }
      else
        Log.Warn("NowPlaying.OnUpdateUsersFriendsCompleted: unexpected response for request: {0}", request.Type);
    }

    private void OnPlaybackStopped()
    {
      LastFMStation.CurrentStreamState = StreamPlaybackState.initialized;

      SetArtistThumb(String.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.TrackTags", String.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.SimilarArtists", String.Empty);
      GUIPropertyManager.SetProperty("#trackduration", " ");
      GUIPropertyManager.SetProperty("#currentplaytime", String.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", String.Empty);

      //reset the TrayIcon
      ShowSongTrayBallon(GUILocalizeStrings.Get(34050), " ", 1, false); // Stream stopped
    }

    #endregion

    #region Handlers

    private void OnRadioSettingsSuccess()
    {
      UpdateUsersTopArtists(LastFMStation.AccountUser);
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

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void SetAudioScrobblerTrackThread(object aParam)
    {
      string aStreamFile = aParam.ToString();
      Log.Debug("GUIRadioLastFM: Set Audioscrobbler track for {0}", aStreamFile);

      AudioscrobblerBase.CurrentPlayingSong = _streamSong;
      AudioscrobblerBase.CurrentPlayingSong.Source = SongSource.L;

      RebuildStreamList();
    }

    private void OnPlaybackStarted()
    {
      SetArtistThumb(String.Empty);
      GUITextureManager.CleanupThumbs();
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.TrackTags", String.Empty);
      GUIPropertyManager.SetProperty("#Play.Current.Lastfm.SimilarArtists", String.Empty);      

      if (_lastTrackTagRequest != null)
        InfoScrobbler.RemoveRequest(_lastTrackTagRequest);
      if (_lastArtistCoverRequest != null)
        InfoScrobbler.RemoveRequest(_lastArtistCoverRequest);

      if (_streamSong != null)
      {
        if (_streamSong.Artist != String.Empty)
        {
          UpdateArtistInfo(_streamSong.Artist);
          GUIPropertyManager.SetProperty("#Play.Current.Artist", _streamSong.Artist);

          if (_streamSong.Title != String.Empty)
          {
            UpdateTrackTagsInfo(_streamSong.Artist, _streamSong.Title);

            GUIPropertyManager.SetProperty("#Play.Current.Title", _streamSong.Title);
            GUIPropertyManager.SetProperty("#Play.Current.Album", _streamSong.Album);
            GUIPropertyManager.SetProperty("#Play.Current.Genre", _streamSong.Genre);
            // GUIPropertyManager.SetProperty("#Play.Current.Thumb", _streamSong.Comment);
            GUIPropertyManager.SetProperty("#trackduration", Util.Utils.SecondsToHMSString(_streamSong.Duration));

            OnUpdateNotifyBallon();
          }
        }       

      }
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

    private void PlayBackStartedHandler(g_Player.MediaType type, string filename)
    {
      if (!Util.Utils.IsLastFMStream(filename) || (LastFMStation.CurrentStreamState != StreamPlaybackState.streaming && LastFMStation.CurrentStreamState != StreamPlaybackState.starting))
        return;

      OnPlaybackStarted();

      Thread stateThread = new Thread(new ParameterizedThreadStart(SetAudioScrobblerTrackThread));
      stateThread.IsBackground = true;
      stateThread.Name = "Last.FM event";
      stateThread.Start((object)filename);
    }

    private void PlayBackStoppedHandler(g_Player.MediaType type, int stoptime, string filename)
    {
      if (!Util.Utils.IsLastFMStream(filename) || LastFMStation.CurrentStreamState != StreamPlaybackState.streaming)
        return;
      OnPlaybackStopped();
    }

    private void PlayBackEndedHandler(g_Player.MediaType type, string filename)
    {
      if (!Util.Utils.IsLastFMStream(filename) || LastFMStation.CurrentStreamState != StreamPlaybackState.streaming)
        return;

      try
      {
        g_Player.Stop();

        Log.Debug("GUIRadioLastFM: PlayBackEnded for this selection - trying restart...");
        if (RebuildStreamList())
        {
          if (PlayPlayListStreams(_radioTrackList[0]))
          {
            LastFMStation.CurrentPlaybackType = PlaybackType.PlaylistPlayer;
            return;
          }
        }

        OnPlaybackStopped();
        PlayBackNoMoreContentHandler();
      }
      catch (Exception ex)
      {
        Log.Warn("GUIRadioLastFM: Error in PlayBackEndedHandler - {0}", ex.Message);
      }
    }

    private void PlayBackNoMoreContentHandler()
    {
      LastFMStation.CurrentStreamState = StreamPlaybackState.nocontent;
      ShowSongTrayBallon(GUILocalizeStrings.Get(34051), GUILocalizeStrings.Get(34052), 15, true); // Stream ended, No more content or bad connection

      GUIDialogOK msgdlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (msgdlg == null)
        return;
      msgdlg.SetHeading(34050); // No stream active
      msgdlg.SetLine(1, GUILocalizeStrings.Get(34052)); // Playback of selected stream failed
      msgdlg.DoModal(GetID);

      Log.Info("GUIRadioLastFM: No more content for this selection or interrupted stream..");
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

    private static void OnLoveClicked()
    {
      // LastFMStation.SendControlCommand(StreamControls.lovetrack);
      AudioscrobblerBase.CurrentSubmitSong.AudioscrobblerAction = SongAction.L;
      AudioscrobblerBase.DoLoveTrackNow();
    }

    private void OnBanClicked()
    {
      // LastFMStation.SendControlCommand(StreamControls.bantrack);
      AudioscrobblerBase.CurrentSubmitSong.AudioscrobblerAction = SongAction.B;
      AudioscrobblerBase.DoBanTrackNow();
      OnSkipHandler(false);
    }

    private void OnSkipClicked()
    {
      // Only mark tracks as skipped if it has not been played long enough for a regular submit
      if (AudioscrobblerBase.CurrentSubmitSong.AudioScrobblerStatus == SongStatus.Loaded)
        AudioscrobblerBase.CurrentSubmitSong.AudioscrobblerAction = SongAction.S;
      OnSkipHandler(false);
    }


    private void OnPlaySimilarArtistsClicked()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(34016); // // Play current similar tags

      dlg.Add(GUILocalizeStrings.Get(188)); // Select all
      for (int i = 0; i < _similarArtistCache.Count; i++)
      {
        dlg.Add(_similarArtistCache[i]);
        // position on the current artist
        if (_similarArtistCache[i] == AudioscrobblerBase.CurrentPlayingSong.Artist)
          dlg.SelectedLabel = i + 1; // that dialog starts with 1!? and we've added a manual item as well
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;

      if (dlg.SelectedId == 1)
      {
        btnChooseArtist.Label = BuildStringFromList(_similarArtistCache);
        LastFMStation.TuneIntoArtists(_similarArtistCache);
      }
      else
      {
        btnChooseArtist.Label = _similarArtistCache[dlg.SelectedId - 2];
        LastFMStation.TuneIntoArtists(BuildListFromString(_similarArtistCache[dlg.SelectedId - 2]));
      }

      // The official client does also fetch 2 lists...
      RebuildStreamList();
      StartPlaybackNow();
    }

    private void OnPlaySimilarTagsClicked()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(34017); // // Play current similar tags

      dlg.Add(GUILocalizeStrings.Get(188)); // Select all
      for (int i = 0; i < _trackTagsCache.Count; i++)
      {
        dlg.Add(_trackTagsCache[i]);
        // position on the previous tag
        if (_trackTagsCache[i] == AudioscrobblerBase.CurrentPlayingSong.Genre)
          dlg.SelectedLabel = i + 1; // that dialog starts with 1!? and we've added a manual item as well
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;

      if (dlg.SelectedId == 1)
      {
        btnChooseTag.Label = BuildStringFromList(_trackTagsCache);
        LastFMStation.TuneIntoTags(_trackTagsCache);
      }
      else
      {
        btnChooseTag.Label = _trackTagsCache[dlg.SelectedId - 2];
        LastFMStation.TuneIntoTags(BuildListFromString(_trackTagsCache[dlg.SelectedId - 2]));
      }

      // The official client does also fetch 2 lists...
      RebuildStreamList();
      StartPlaybackNow();
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

        if (notifyTitle == String.Empty)
          notifyTitle = "MediaPortal";
        _trayBallonSongChange.BalloonTipTitle = notifyTitle;
        if (notifyMessage_ == String.Empty)
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
    
    private void OnUpdateNotifyBallon()
    {
      // Send msg for Ballon Tip on song change
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_BALLONTIP_SONGCHANGE, 0, 0, 0, 0, 0, null);
      msg.Label = _streamSong.Title;
      msg.Label2 = _streamSong.Artist + " (" + _streamSong.Album + ")";
      msg.Param1 = 5;
      GUIGraphicsContext.SendMessage(msg);
      msg = null;

      Log.Info("GUIRadioLastFM: Current track: {0} [{1}] - {2} ({3})", _streamSong.Artist, _streamSong.Album, _streamSong.Title, Util.Utils.SecondsToHMSString(_streamSong.Duration));
    }

    // love
    private void Tray_menuItem1_Click(object Sender, EventArgs e)
    {
      if ((int)LastFMStation.CurrentStreamState > 3) // starting
      {
        OnLoveClicked();
      }
    }

    // ban
    private void Tray_menuItem2_Click(object Sender, EventArgs e)
    {
      if ((int)LastFMStation.CurrentStreamState > 3)
      {
        OnBanClicked();
      }
    }

    // skip
    private void Tray_menuItem3_Click(object Sender, EventArgs e)
    {
      if ((int)LastFMStation.CurrentStreamState > 3)
      {
        OnSkipClicked();
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

      //string refString = String.Empty;
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
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
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