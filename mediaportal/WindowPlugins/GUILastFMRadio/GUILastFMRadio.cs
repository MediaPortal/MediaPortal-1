using System;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.LastFM;

using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.LastFMRadio
{

  [PluginIcons("WindowPlugins.GUILastFMRadio.BallonRadio.gif", "WindowPlugins.GUILastFMRadio.BallonRadioDisabled.gif")]
  public class GUILastFMRadio : GUIWindow, ISetupForm
  {

    private enum SkinControls
    {
      USER_RECOMMENDED_BUTTON = 10,
      USER_MIX_BUTTON = 11,
      USER_LIBRARY_BUTTON = 12,
      ARTIST_BUTTON = 13,
      TAG_BUTTON = 14
    }

    [SkinControlAttribute((int)SkinControls.USER_RECOMMENDED_BUTTON)] protected GUIButtonControl BtnUserRecomended = null;
    [SkinControlAttribute((int)SkinControls.USER_MIX_BUTTON)] protected GUIButtonControl BtnUserMix = null;
    [SkinControlAttribute((int)SkinControls.USER_LIBRARY_BUTTON)] protected GUIButtonControl BtnUserLibrary = null;
    [SkinControlAttribute((int)SkinControls.ARTIST_BUTTON)] protected GUIButtonControl BtnArtist = null;
    [SkinControlAttribute((int)SkinControls.TAG_BUTTON)] protected GUIButtonControl BtnTag = null;

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
      return 5678;
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
        return 5678;
      }

      set
      {
      }
    }

    #endregion

    GUILastFMRadio()
    {
      g_Player.PlayBackEnded += OnPlayBackEnded;
      g_Player.PlayBackChanged += OnPlayBackChanged;
    }

    #region overrides

    public override bool Init()
    {
      _playlistPlayer = PlayListPlayer.SingletonPlayer;
      var a = new LastFMLibrary(); //TODO this is just making _SK get loaded.   No need to actual instansiate
      return Load(GUIGraphicsContext.Skin + @"\lastFmRadio.xml");
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

      base.OnClicked(controlId, control, actionType);
    }

    #endregion

    #region last.fm code

    private static void TuneToStation(string strStation)
    {

      Log.Debug("Attempting to Tune to last.fm station: {0}", strStation);

      // Clear playlist and start playback
      var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_LAST_FM;
      pl.Clear();
      _playlistPlayer.Reset();

      LastFMLibrary.TuneRadio(strStation);
      AddMoreTracks();
    }

    private static void AddMoreTracks()
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
          Duration = lastFMTrack.Duration
        };

        var a = new PlayListItem
        {
          Type = PlayListItem.PlayListItemType.Audio,
          FileName = lastFMTrack.TrackURL,
          Description = lastFMTrack.ArtistName + " - " + lastFMTrack.TrackTitle,
          Duration = lastFMTrack.Duration,
          MusicTag = tag
        };
        Log.Info("Artist: {0} :Title: {1} :URL: {2}", lastFMTrack.ArtistName, lastFMTrack.TrackTitle, lastFMTrack.TrackURL);

        pl.Add(a);
      }
      
      _playlistPlayer.Play(0);      
    }

    #endregion

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

    private static void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }
      DoOnChanged();
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
    private static void OnPlayBackChanged(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }
      DoOnChanged();
    }

    private static void DoOnChanged()
    {
      if (_playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_LAST_FM)
      {
        return;
      }
      var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      var currentTrackIndex = _playlistPlayer.CurrentSong;
      var plRemainingTracks = pl.Count - currentTrackIndex - 1;

      if (plRemainingTracks < 5)
      {
        AddMoreTracks();
      }


    }

    #endregion

  }
}
