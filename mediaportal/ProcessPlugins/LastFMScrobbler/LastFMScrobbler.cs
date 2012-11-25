#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Globalization;
using System.Linq;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Profile;
using MediaPortal.LastFM;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;

namespace MediaPortal.ProcessPlugins.LastFMScrobbler
{

  [PluginIcons("ProcessPlugins.LastFMScrobbler.LastFMScrobbler.gif",
               "ProcessPlugins.LastFMScrobbler.LastFMSscrobblerDisabled.gif")]
  public class LastFMScrobbler : IPlugin, ISetupForm
  {
    private class PlaybackStop
    {
      public string Filename;
      public int Stoptime;
    }

    private static BackgroundWorker _announceWorker;
    private static BackgroundWorker _scrobbleWorker;
    private static readonly Random Randomizer = new Random();
    private static int _randomness = 100;
    private static bool _autoDJEnabled = true;

    private static void LoadSettings()
    {
      using (var xmlreader = new MPSettings())
      {
        _autoDJEnabled = xmlreader.GetValueAsBool("lastfm:test", "autoDJ", true);
        _randomness = xmlreader.GetValueAsInt("lastfm:test", "randomness", 100);
      }
    }

    #region IPlugin members

    public void Start()
    {
      Log.Info("LastFMScrobbler: Starting");
      g_Player.PlayBackStarted += OnPlayBackStarted;
      g_Player.PlayBackEnded += OnPlayBackEnded;
      g_Player.PlayBackChanged += OnPlayBackChanged;
      g_Player.PlayBackStopped += OnPlayBackStopped;
    }

    public void Stop()
    {
      Log.Info("LastFMScrobbler: Stopped");
    }

    #endregion

    #region ISetupForm methods

    /// <summary>
    ///   Determines whether this plugin can be enabled.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this plugin can be enabled; otherwise, <c>false</c>.
    /// </returns>
    public bool CanEnable()
    {
      return true;
    }

    /// <summary>
    ///   Determines whether this plugin has setup.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this plugin has setup; otherwise, <c>false</c>.
    /// </returns>
    public bool HasSetup()
    {
      return true;
    }

    /// <summary>
    ///   Gets the plugin name.
    /// </summary>
    /// <returns>The plugin name.</returns>
    public string PluginName()
    {
      return "LastFMScrobblerTest";
    }

    /// <summary>
    ///   Defaults enabled.
    /// </summary>
    /// <returns>true if this plugin is enabled by default, otherwise false.</returns>
    public bool DefaultEnabled()
    {
      return true;
    }

    /// <summary>
    ///   Gets the window id.
    /// </summary>
    /// <returns>The window id.</returns>
    public int GetWindowId()
    {
      return 0;
    }

    /// <summary>
    ///   Gets the plugin author.
    /// </summary>
    /// <returns>The plugin author.</returns>
    public string Author()
    {
      return "Jameson_uk";
    }

    /// <summary>
    ///   Gets the description of the plugin.
    /// </summary>
    /// <returns>The plugin description.</returns>
    public string Description()
    {
      return
        "Test version of new Last.FM Scrobbler";
    }

    /// <summary>
    ///   Shows the plugin configuration.
    /// </summary>
    public void ShowPlugin()
    {
      //      using (var config = new LastFMConfig())
      //      {
      var config = new LastFMConfig();
      config.Show();
      //      }
    }

    /// <summary>
    ///   Gets the home screen details for the plugin.
    /// </summary>
    /// <param name = "strButtonText">The button text.</param>
    /// <param name = "strButtonImage">The button image.</param>
    /// <param name = "strButtonImageFocus">The button image focus.</param>
    /// <param name = "strPictureImage">The picture image.</param>
    /// <returns>true if the plugin can be seen, otherwise false.</returns>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = strButtonImage = strButtonImageFocus = strPictureImage = String.Empty;
      return false;
    }

    #endregion ISetupForm methods

    #region g_player events

    /// <summary>
    /// Handle event fired by MP player.  Something has started playing so check if it is music then
    /// announce track as now playing on last.fm website
    /// </summary>
    /// <param name="type">MediaType of item that has started</param>
    /// <param name="filename">filename of item that has started</param>
    private static void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }

      _announceWorker = new BackgroundWorker();
      _announceWorker.DoWork += AnnounceWorkerDoWork;
      _announceWorker.RunWorkerAsync(filename);
    }

    /// <summary>
    /// Handle event fired by MP player.  Playback has ended; things have come naturally to the end 
    /// (eg. last track on album has finished and no more items in playlist)
    /// </summary>
    /// <param name="type">MediaType of item that has ended</param>
    /// <param name="filename">filename of item that has ended</param>
    private static void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }

      DoOnChangedOrStopped(9999, filename);
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

      DoOnChangedOrStopped(stoptime, filename);
    }

    /// <summary>
    /// Handle event fired by MP player.
    /// Playback has stopped; user has pressed stop mid way through a track
    /// </summary>
    /// <param name="type">MediaType of track that was stopped</param>
    /// <param name="stoptime">Number of seconds item has played for before it was stopped</param>
    /// <param name="filename">filename of item that was stopped</param>
    private static void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }

      DoOnChangedOrStopped(stoptime, filename);
    }

    #endregion

    #region Background Workers

    /// <summary>
    /// Background worker to announce now playing details on last.fm website
    /// Also triggers Auto DJ mode adding similar track to the playlist
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">Argument should be the path of a file as a string</param>
    private static void AnnounceWorkerDoWork(object sender, DoWorkEventArgs e)
    {
      // Get song details and announce on last.fm
      var pl = PlayListPlayer.SingletonPlayer.GetPlaylist(PlayListPlayer.SingletonPlayer.CurrentPlaylistType);
      var plI = pl.First(plItem => plItem.FileName == (string)e.Argument);
      if (plI == null || plI.MusicTag == null)
      {
        Log.Info("Unable to announce song: {0}  as it does not exist in the playlist");
        return;
      }

      var currentSong = (MusicTag)plI.MusicTag;
      
      LastFMLibrary.UpdateNowPlaying(currentSong.Artist, currentSong.Title, currentSong.Album,
                         currentSong.Duration.ToString(CultureInfo.InvariantCulture));
      if (_autoDJEnabled)
      {
        AutoDJ(currentSong.Title, currentSong.Artist);
      }
    }

    /// <summary>
    /// Call backgound worker for scrobbling when track ends
    /// whether this is due to user skipping, user stopping or playback naturally coming to an end
    /// </summary>
    /// <param name="stoptime">Number of seconds through track was when it was stopped</param>
    /// <param name="filename">filename of track that was stopped</param>
    private static void DoOnChangedOrStopped(int stoptime, string filename)
    {
      var playbackStop = new PlaybackStop { Filename = filename, Stoptime = stoptime };
      _scrobbleWorker = new BackgroundWorker();
      _scrobbleWorker.DoWork += ScrobbleWorkerDoWork;
      _scrobbleWorker.RunWorkerAsync(playbackStop);
    }

    /// <summary>
    /// Background worker to scrobble track to last.fm
    /// Validates the rules for submission such as track > 30 seconds and more than 50% played
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">Instance of internal class PlaybackStop which contains stoptime and filename</param>
    private static void ScrobbleWorkerDoWork(object sender, DoWorkEventArgs e)
    {
      var filename = ((PlaybackStop)e.Argument).Filename;
      var stoptime = ((PlaybackStop)e.Argument).Stoptime;

      // get song details and if appropriate scrobble to last.fm

      var pl = PlayListPlayer.SingletonPlayer.GetPlaylist(PlayListPlayer.SingletonPlayer.CurrentPlaylistType);
      var plI = pl.First(plItem => plItem.FileName == filename);
      if (plI == null || plI.MusicTag == null)
      {
        Log.Info("Unable to scrobble song: {0}  as it does not exist in the playlist");
        return;
      }

      var currentSong = (MusicTag)plI.MusicTag;

      /*
      var currentSong = new Song();
      MusicDatabase.Instance.GetSongByFileName(filename, ref currentSong);

      if (string.IsNullOrEmpty(currentSong.FileName))
      {
        Log.Info("[MIKE] Scobble: Song is not in database: {0}", filename);
        return;
      }
       */

      if (currentSong.Duration < 30)
      { // last.fm say not to scrobble songs that last less than 30 seconds
        return;
      }
      if (stoptime < 240 && stoptime < (currentSong.Duration / 2))
      { // last.fm say only to scrobble is more than 4 minutes has been listned to or 
        // at least hald the duration of the song
        return;
      }

      LastFMLibrary.Scrobble(currentSong.Artist, currentSong.Title, currentSong.Album);
    }

    #endregion

    #region playlist / AutoDJ

    /// <summary>
    /// Checks if an item with the same file path already exists in the current playlist
    /// </summary>
    /// <param name="fileName">full file path of item to be checked</param>
    /// <returns>True is file exists in current playlist</returns>
    public static bool InPlaylist(string fileName)
    {
      var pl = PlayListPlayer.SingletonPlayer.GetPlaylist(PlayListPlayer.SingletonPlayer.CurrentPlaylistType);
      return pl.Any(plItem => plItem.FileName == fileName);
    }

    /// <summary>
    /// Handles AutoDJ details of identifying similar tracks and choosing which to add to playlist
    /// </summary>
    /// <param name="strArtist">Artist of track</param>
    /// <param name="strTrack">Name of track</param>
    public static void AutoDJ(string strArtist, string strTrack)
    {
      var tracks = LastFMLibrary.GetSimilarTracks(strArtist, strTrack);
      var dbTracks = LastFMLibrary.GetSimilarTracksInDatabase(tracks);
      if (dbTracks.Count > 0)
      {
        AutoDJAddToPlaylist(dbTracks);
      }
      else
      {
        Log.Info("Auto DJ: No similar local tracks found for {0} - {1}", strArtist, strTrack);
      }
    }

    /// <summary>
    /// Chooses a song from a list to add to the current playlist
    /// </summary>
    /// <param name="dbTracks">List of tracks to select from</param>
    public static void AutoDJAddToPlaylist(List<Song> dbTracks)
    {
      //Pick one of the available tracks to add to playlist
      var pl = PlayListPlayer.SingletonPlayer.GetPlaylist(PlayListPlayer.SingletonPlayer.CurrentPlaylistType);
      var currentTrackIndex = PlayListPlayer.SingletonPlayer.CurrentSong;
      var plRemainingTracks = pl.Count - currentTrackIndex - 1;
      var numTracksWanted = 1;  // by default just add a single track
      if (plRemainingTracks < 5)
      {
        // less than five more tracks in playlist so try and build up a reserve
        numTracksWanted = 5 - plRemainingTracks;
      }

      // might not actually be five tracks available in the database
      var numTracksToAdd = Math.Min(numTracksWanted, dbTracks.Count);

      Log.Info("Auto DJ: Matched {0} local songs.  Attempting to add {1} tracks", dbTracks.Count, numTracksToAdd);

      for (var i = 0; i < numTracksToAdd; i++)
      {
        var maxSize = Math.Min(dbTracks.Count, _randomness);
        var trackNo = Randomizer.Next(0, maxSize);
        pl.Add(dbTracks[trackNo].ToPlayListItem());
        Log.Info("Auto DJ: Added to playlist: {0} - {1}", dbTracks[trackNo].Artist, dbTracks[trackNo].Title);
        dbTracks.RemoveAt(trackNo);  // remove song after adding to playlist to prevent the same sone being added twice
      }
    }

    #endregion
  }
}
