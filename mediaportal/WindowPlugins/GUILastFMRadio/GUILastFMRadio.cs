using System;
using System.IO;
using System.Linq;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Music;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.LastFM;

using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

namespace MediaPortal.GUI.LastFMRadio
{

  [PluginIcons("WindowPlugins.GUILastFMRadio.BallonRadio.gif", "WindowPlugins.GUILastFMRadio.BallonRadioDisabled.gif")]
  public class GUILastFMRadio : GUIWindow, ISetupForm
  {

    private enum SkinControls
    {
      USER_RECOMMENDED_BUTTON = 2,
      USER_MIX_BUTTON = 11,
      USER_LIBRARY_BUTTON = 12,
      ARTIST_BUTTON = 13,
      TAG_BUTTON = 14,
      PLAYLIST_CONTROL = 50
    }

    [SkinControlAttribute((int)SkinControls.USER_RECOMMENDED_BUTTON)] protected GUIButtonControl BtnUserRecomended = null;
    [SkinControlAttribute((int)SkinControls.USER_MIX_BUTTON)] protected GUIButtonControl BtnUserMix = null;
    [SkinControlAttribute((int)SkinControls.USER_LIBRARY_BUTTON)] protected GUIButtonControl BtnUserLibrary = null;
    [SkinControlAttribute((int)SkinControls.ARTIST_BUTTON)] protected GUIButtonControl BtnArtist = null;
    [SkinControlAttribute((int)SkinControls.TAG_BUTTON)] protected GUIButtonControl BtnTag = null;
    [SkinControlAttribute((int)SkinControls.PLAYLIST_CONTROL)] protected GUIFacadeControl PlaylistControl = null;

    private static PlayListPlayer _playlistPlayer;

    #region ISetupForm Members

    // Returns the name of the plugin which is shown in the plugin menu
    public string PluginName()
    {
      return "Last.fm Radio Test";
    }

    // Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
      return "Last.fm Radio Test";
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public string Author()
    {
      return "Jameson_uk";
    }

    // show the setup dialog
    public void ShowPlugin()
    {
      //MessageBox.Show("Nothing to configure, this is just an example");
    }

    // Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return true;
    }

    // Get Windows-ID
    public int GetWindowId()
    {
      // WindowID of windowplugin belonging to this setup
      // enter your own unique code
      return 56789;
    }

    // Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return true;
    }

    // indicates if a plugin has it's own setup screen
    public bool HasSetup()
    {
      return false;
    }

    /// <summary>
    /// If the plugin should have it's own button on the main menu of Mediaportal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true : plugin needs it's own button on home
    /// false : plugin does not need it's own button on home</returns>

    public bool GetHome(out string strButtonText, out string strButtonImage,
                        out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = PluginName();
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
      return true;
    }

    // With GetID it will be an window-plugin / otherwise a process-plugin
    // Enter the id number here again
    public override int GetID
    {
      get
      {
        return 56789;
      }
    }

    #endregion

    public GUILastFMRadio()
    {
      g_Player.PlayBackEnded += OnPlayBackEnded;
      g_Player.PlayBackChanged += OnPlayBackChanged;
      g_Player.PlayBackStopped += OnPlayBackStopped;
    }

    #region overrides

    public override bool Init()
    {
      _playlistPlayer = PlayListPlayer.SingletonPlayer;
      var a = new LastFMLibrary(); //TODO this is just making _SK get loaded.   No need to actual instansiate
      return Load(GUIGraphicsContext.Skin + @"\lastFmRadio.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      PlaylistControl.CurrentLayout = Layout.Playlist;
      PlaylistControl.Clear();
      LoadPlaylist();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (controlId == (int)SkinControls.USER_RECOMMENDED_BUTTON)
      {
        TuneToStation("lastfm://user/" + LastFMLibrary.CurrentUser + "/recommended");
      }
      if (controlId == (int)SkinControls.USER_MIX_BUTTON)
      {
        TuneToStation("lastfm://user/" + LastFMLibrary.CurrentUser + "/mix");
      }
      if (controlId == (int)SkinControls.USER_LIBRARY_BUTTON)
      {
        TuneToStation("lastfm://user/" + LastFMLibrary.CurrentUser + "/library");
      }
      if (controlId == (int)SkinControls.ARTIST_BUTTON)
      {
        string strArtist = GetInputFromUser("Select Artist");
        if(!string.IsNullOrEmpty(strArtist))
        {
          TuneToStation("lastfm://artist/" + strArtist +"/similarartists"); 
        }
      }
      if (controlId == (int)SkinControls.TAG_BUTTON)
      {
        string strTag = GetInputFromUser("Select Tag");
        if (!string.IsNullOrEmpty(strTag))
        {
          TuneToStation("lastfm://globaltags/" + strTag);
        }
      }
      if (controlId == (int)SkinControls.PLAYLIST_CONTROL)
      {
        _playlistPlayer.Play(PlaylistControl.SelectedListItemIndex);
      }

      base.OnClicked(controlId, control, actionType);
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOVE_SELECTED_ITEM_UP:
          MovePlayListItemUp();
          break;
        case Action.ActionType.ACTION_MOVE_SELECTED_ITEM_DOWN:
          MovePlayListItemDown();
          break;
        case Action.ActionType.ACTION_DELETE_SELECTED_ITEM:
          DeletePlayListItem();
          LoadPlaylist();
          break;
      }

      base.OnAction(action);
    }

    #endregion

    #region last.fm code

    private void TuneToStation(string strStation)
    {

      Log.Debug("Attempting to Tune to last.fm station: {0}", strStation);

      // Clear playlist and start playback
      var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      pl.Clear();
      _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_LAST_FM;
      _playlistPlayer.Reset();

      LastFMLibrary.TuneRadio(strStation);
      AddMoreTracks();
      _playlistPlayer.Play(0);
    }

    private void AddMoreTracks()
    {
      var z = LastFMLibrary.GetRadioPlaylist();
      foreach (var lastFMTrack in z)
      {
        var tag = new MusicTag
        {
          AlbumArtist = lastFMTrack.ArtistName,
          Artist = lastFMTrack.ArtistName,
          Title = lastFMTrack.TrackTitle,
          FileName = lastFMTrack.TrackURL,
          Duration = lastFMTrack.Duration,
          Lyrics =   lastFMTrack.ImageURL
          //TODO: this should not be lyrics
        };

        var pli = new PlayListItem
        {
          Type = PlayListItem.PlayListItemType.Audio,
          FileName = lastFMTrack.TrackURL,
          Description = lastFMTrack.ArtistName + " - " + lastFMTrack.TrackTitle,
          Duration = lastFMTrack.Duration,
          MusicTag = tag
        };

        Log.Info("Artist: {0} :Title: {1} :URL: {2}", lastFMTrack.ArtistName, lastFMTrack.TrackTitle, lastFMTrack.TrackURL);

        var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
        pl.Add(pli);

      }
      LoadPlaylist();
    }

    #endregion

    protected virtual void OnRetrieveCoverArt(GUIListItem item)
    {
      Util.Utils.SetDefaultIcons(item);
      if (item.Label == "..")
      {
        return;
      }
      MusicTag tag = (MusicTag)item.MusicTag;
      string strThumb = GUIMusicBaseWindow.GetCoverArt(item.IsFolder, item.Path, tag);
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
      else
      {
        //TODO: Only for testing
        // need to remove this as downloading thumb in main thread
        // also I think temp resources are not cleaned up
        // There is also a check in music overlay so that this will
        // not override thumb set here (#Play.Current.Thumb)
        string temporaryFilename = Path.GetTempFileName() + ".jpg";
        Util.Utils.DownLoadAndCacheImage(tag.Lyrics, temporaryFilename);
        if (item.Path == g_Player.currentFileName)
        {
          GUIPropertyManager.SetProperty("#Play.Current.Thumb", temporaryFilename);
        }
        item.ThumbnailImage = temporaryFilename;
        item.IconImageBig = temporaryFilename;
        item.IconImage = temporaryFilename;
      }
    }

    /// <summary>
    /// Displays a virtual keyboard
    /// </summary>
    /// <param name="aDefaultText">a text which will be preselected</param>
    /// <returns>the text entered by the user</returns>
    private string GetInputFromUser(string aDefaultText)
    {
      string searchterm = aDefaultText;
      var keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
      keyboard.Reset();
      keyboard.IsSearchKeyboard = false;
      keyboard.Text = searchterm;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        searchterm = keyboard.Text;
      }
      return searchterm;
    }

    #region g_player events

    private void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }
      DoOnEnded();
    }

    /// <summary>
    /// Handle event fired by MP player.
    /// Playback has changed; this event signifies that the existing item has been changed
    /// this could be because that song has ended and playback has moved to next item in playlist
    /// or could be because user has skipped tracks
    /// </summary>
    /// <param name="type">MediaType of item that was playing</param>
    /// <param name="stoptime">Number of seconds item has played for when it stopped</param>
    /// <param name="filename">filename of item that was playing</param>
    private void OnPlayBackChanged(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }
      DoOnChanged();
    }

    /// <summary>
    /// Handle event fired by MP player.
    /// Playback has stopped; user has pressed stop mid way through a track
    /// </summary>
    /// <param name="type">MediaType of track that was stopped</param>
    /// <param name="stoptime">Number of seconds item has played for before it was stopped</param>
    /// <param name="filename">filename of item that was stopped</param>
    private void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }

      DoOnEnded();
    }

    private void DoOnChanged()
    {
      if (_playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_LAST_FM)
      {
        return;
      }
      var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      var currentTrackIndex = _playlistPlayer.CurrentSong;

      for (int i = currentTrackIndex - 1; i >= 0; i--)
      {
        RemovePlayListItem(i);
      }

      if (pl.Count< 5)
      {
        AddMoreTracks();
      }
    }

    private void DoOnEnded()
    {
      if (_playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_LAST_FM)
      {
        return;
      }
      var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      pl.Clear();
      _playlistPlayer.Reset();
      LoadPlaylist();
      
    }

    private void LoadPlaylist()
    {
      PlaylistControl.Clear();
      var playlist = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      for (int i = 0; i < playlist.Count; i++)
      {
        PlayListItem pli = playlist[i];

        GUIListItem pItem = new GUIListItem(pli.Description);

        MusicTag tag = (MusicTag) pli.MusicTag;
        bool dirtyTag = false;
        if (tag != null)
        {
          pItem.MusicTag = tag;
          if (tag.Title == ("unknown") || tag.Title.IndexOf("unknown") > 0 || tag.Title == string.Empty)
          {
            dirtyTag = true;
          }
        }
        else
        {
          dirtyTag = true;
        }

        if (tag != null && !dirtyTag)
        {
          string duration = Util.Utils.SecondsToHMSString(tag.Duration);
          pItem.Label = string.Format("{0} - {1}", tag.Artist, tag.Title);
          pItem.Label2 = duration;
          pItem.MusicTag = pli.MusicTag;
        }

        pItem.Path = pli.FileName;
        pItem.IsFolder = false;

        if (pli.Played)
        {
          pItem.Shaded = true;
        }

        Util.Utils.SetDefaultIcons(pItem);

        pItem.OnRetrieveArt += OnRetrieveCoverArt;
        //pItem.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);

        PlaylistControl.Add(pItem);
      }
    }

    #endregion

    #region playlist changes

    private void MovePlayListItemUp()
    {
      if (_playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_LAST_FM;
      }

      if (_playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_LAST_FM
          || PlaylistControl.CurrentLayout != GUIFacadeControl.Layout.Playlist
          || PlaylistControl.PlayListLayout == null)
      {
        return;
      }

      int iItem = PlaylistControl.SelectedListItemIndex;

      PlayList playList = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      playList.MovePlayListItemUp(iItem);
      int selectedIndex = PlaylistControl.MoveItemUp(iItem, true);

      if (iItem == _playlistPlayer.CurrentSong)
      {
        _playlistPlayer.CurrentSong = selectedIndex;
      }

      PlaylistControl.SelectedListItemIndex = selectedIndex;
    }

    private void MovePlayListItemDown()
    {
      if (_playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_LAST_FM;
      }

      if (_playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_LAST_FM
          || PlaylistControl.CurrentLayout != Layout.Playlist
          || PlaylistControl.PlayListLayout == null)
      {
        return;
      }

      int iItem = PlaylistControl.SelectedListItemIndex;
      PlayList playList = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);

      playList.MovePlayListItemDown(iItem);
      int selectedIndex = PlaylistControl.MoveItemDown(iItem, true);

      if (iItem == _playlistPlayer.CurrentSong)
      {
        _playlistPlayer.CurrentSong = selectedIndex;
      }
    }

    private void DeletePlayListItem()
    {
      if (_playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_NONE)
      {
        _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_LAST_FM;
      }

      if (_playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_LAST_FM
          || PlaylistControl.CurrentLayout != Layout.Playlist
          || PlaylistControl.PlayListLayout == null)
      {
        return;
      }

      int iItem = PlaylistControl.SelectedListItemIndex;

      string currentFile = g_Player.CurrentFile;
      GUIListItem item = PlaylistControl[iItem];
      RemovePlayListItem(iItem);

      if (currentFile.Length > 0 && currentFile == item.Path)
      {
        string nextTrackPath = _playlistPlayer.GetNext();

        if (nextTrackPath.Length == 0)
        {
          g_Player.Stop();
        }

        else
        {
          if (iItem == PlaylistControl.Count)
          {
            _playlistPlayer.Play(iItem - 1);
          }

          else
          {
            _playlistPlayer.PlayNext();
          }
        }
      }

      if (PlaylistControl.Count == 0)
      {
        g_Player.Stop();
      }

      else
      {
        PlaylistControl.PlayListLayout.SelectedListItemIndex = iItem;
      }

    }

    private void RemovePlayListItem(int iItem)
    {
      GUIListItem pItem = PlaylistControl[iItem];
      if (pItem == null)
      {
        return;
      }
      string strFileName = pItem.Path;

      _playlistPlayer.Remove(PlayListType.PLAYLIST_LAST_FM, strFileName);

      LoadPlaylist();

      GUIControl.SelectItemControl(GetID, PlaylistControl.GetID, iItem);
    }

    #endregion

  }
}
