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

using System;
using System.Drawing;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.ExtensionMethods;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIMusicOverlay : GUIInternalOverlayWindow, IRenderLayer
  {
    #region Enums

    private enum PlayBackType : int
    {
      NORMAL = 0,
      GAPLESS = 1,
      CROSSFADE = 2
    }

    #endregion

    #region <skin> Variables

    [SkinControl(0)] protected GUIImage _videoRectangle = null;
    [SkinControl(1)] protected GUIImage _thumbImage = null;
    [SkinControl(2)] protected GUILabelControl _labelPlayTime = null;
    [SkinControl(3)] protected GUIImage _imagePlayLogo = null;
    [SkinControl(4)] protected GUIImage _imagePauseLogo = null;
    [SkinControl(5)] protected GUIFadeLabel _labelInfo = null;
    [SkinControl(6)] protected GUIImage _labelBigPlayTime = null;
    [SkinControl(7)] protected GUIImage _imageFastForward = null;
    [SkinControl(8)] protected GUIImage _imageRewind = null;
    [SkinControl(9)] protected GUIVideoControl _videoWindow = null;
    [SkinControl(10)] protected GUIImage _imageNormal = null;
    [SkinControl(11)] protected GUIImage _imageGapless = null;
    [SkinControl(12)] protected GUIImage _imageCrossfade = null;

    #endregion

    #region Variables

    private bool _isFocused = false;
    private string _fileName = string.Empty;
    private string _thumbLogo = string.Empty;
    private bool _useBassEngine = false;
    private bool _didRenderLastTime = false;
    private bool _visualisationEnabled = true;
    private bool _stripArtistPrefixes = false;
    private bool _settingVisEnabled = true;
    protected bool _playlistIsCurrent;
    private PlayListPlayer playlistPlayer;

    #endregion

    #region delegates

    protected delegate void PlaybackChangedDelegate(g_Player.MediaType type, string filename);

    #endregion

    #region Constructors/Destructors

    public GUIMusicOverlay()
    {
      GetID = (int)Window.WINDOW_MUSIC_OVERLAY;
      playlistPlayer = PlayListPlayer.SingletonPlayer;
      _useBassEngine = BassMusicPlayer.IsDefaultMusicPlayer;
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _settingVisEnabled = xmlreader.GetValueAsBool("musicfiles", "doVisualisation", false) && _useBassEngine;
        _visualisationEnabled = _settingVisEnabled;
        _stripArtistPrefixes = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);
        _playlistIsCurrent = xmlreader.GetValueAsBool("musicfiles", "playlistIsCurrent", true);
      }

      g_Player.PlayBackStarted += OnPlayBackStarted;
      g_Player.PlayBackEnded += OnPlayBackEnded;

      playlistPlayer.PlaylistChanged += OnPlaylistChanged;
    }

    #endregion

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.GetThemedSkinFile(@"\musicOverlay.xml"));
      GetID = (int)Window.WINDOW_MUSIC_OVERLAY;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MusicOverlay);
      return result;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void PreInit()
    {
      base.PreInit();
      AllocResources();
    }

    public override void Render(float timePassed) {}

    private void OnUpdateState(bool render)
    {
      if (_didRenderLastTime != render)
      {
        _didRenderLastTime = render;
        if (render)
        {
          QueueAnimation(AnimationType.WindowOpen);
        }
        else
        {
          QueueAnimation(AnimationType.WindowClose);
        }
      }
    }

    public override bool DoesPostRender()
    {
      if (!g_Player.Playing ||
          g_Player.IsVideo || g_Player.IsDVD || g_Player.IsTVRecording || g_Player.IsTV ||
          (!g_Player.IsRadio && !g_Player.IsMusic))
      {
        _fileName = string.Empty;
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }

      if (g_Player.Playing)
      {
        if (g_Player.CurrentFile.Contains(".tsbuffer") ||
            g_Player.CurrentFile.StartsWith("rtsp://", StringComparison.InvariantCultureIgnoreCase))
        {
          PlayListItem pitem = playlistPlayer.GetCurrentItem();
          if (pitem != null)
          {
            if (pitem.FileName != _fileName)
            {
              _fileName = pitem.FileName;
              _visualisationEnabled = false;
            }
          }
          else
          {
            if (g_Player.CurrentFile != _fileName)
            {
              _fileName = g_Player.CurrentFile;
              _visualisationEnabled = false;
            }
          }
        }
        else if (g_Player.CurrentFile != _fileName)
        {
          _fileName = g_Player.CurrentFile;
          if (_settingVisEnabled)
          {
            _visualisationEnabled = true;
          }
        }
      }

      if (GUIGraphicsContext.IsFullScreenVideo || GUIGraphicsContext.Calibrating)
      {
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }

      if (!GUIGraphicsContext.Overlay)
      {
        OnUpdateState(false);

        if ((_videoWindow != null) &&
            (GUIGraphicsContext.VideoWindow.Equals(new Rectangle(_videoWindow.XPosition, _videoWindow.YPosition,
                                                                 _videoWindow.Width, _videoWindow.Height))))
        {
          return base.IsAnimating(AnimationType.WindowClose);
        }
        else
        {
          if ((_videoRectangle != null) &&
              (GUIGraphicsContext.VideoWindow.Equals(new Rectangle(_videoRectangle.XPosition, _videoRectangle.YPosition,
                                                                   _videoRectangle.Width, _videoRectangle.Height))))
          {
            return base.IsAnimating(AnimationType.WindowClose);
          }
        }
        return false;
        // no final animation when the video window has changed, this happens most likely when a new window opens
      }
      OnUpdateState(true);
      return true;
    }

    public override void PostRender(float timePassed, int iLayer)
    {
      if (GUIWindowManager.IsRouted)
      {
        return;
      }
      if (iLayer != 2)
      {
        return;
      }
      if (!base.IsAnimating(AnimationType.WindowClose))
      {
        if (GUIPropertyManager.GetProperty("#Play.Current.Thumb") != _thumbLogo)
        {
          _fileName = g_Player.CurrentFile;
        }

        long lPTS1 = (long)(g_Player.CurrentPosition);
        int hh = (int)(lPTS1 / 3600) % 100;
        int mm = (int)((lPTS1 / 60) % 60);
        int ss = (int)((lPTS1 / 1) % 60);

        int iSpeed = g_Player.Speed;
        if (hh == 0 && mm == 0 && ss < 5)
        {
          if (iSpeed < 1)
          {
            iSpeed = 1;
            g_Player.Speed = iSpeed;
            g_Player.SeekAbsolute(0.0d);
          }
        }

        if (_imagePlayLogo != null)
        {
          _imagePlayLogo.Visible = ((g_Player.Paused == false) && (g_Player.Playing));
        }

        if (_imagePauseLogo != null)
        {
          _imagePauseLogo.Visible = (g_Player.Paused == true);
        }

        if (_imageFastForward != null)
        {
          _imageFastForward.Visible = (g_Player.Speed > 1);
        }

        if (_imageRewind != null)
        {
          _imageRewind.Visible = (g_Player.Speed < 0);
        }

        if (_imageNormal != null)
        {
          _imageNormal.Visible = (g_Player.PlaybackType == (int)PlayBackType.NORMAL);
        }

        if (_imageGapless != null)
        {
          _imageGapless.Visible = (g_Player.PlaybackType == (int)PlayBackType.GAPLESS);
        }

        if (_imageCrossfade != null)
        {
          _imageCrossfade.Visible = (g_Player.PlaybackType == (int)PlayBackType.CROSSFADE);
        }

        if (_videoWindow != null)
        {
          _videoWindow.Visible = _visualisationEnabled; // switch it of when we do not have any vizualisation
        }

        if (_videoRectangle != null)
        {
          if (g_Player.Playing) // && !_videoWindow.SetVideoWindow)
          {
            _videoRectangle.Visible = GUIGraphicsContext.ShowBackground;
          }
          else
          {
            _videoRectangle.Visible = false;
          }
        }

        // this is called serveral times per second!
        if (_videoWindow != null)
        {
          // Old code for overlay support? Commented out to fix Mantis:
          // 0001682: Music visualization it's off position when using UI Calibration
          //SetVideoWindow(new Rectangle(_videoWindow.XPosition, _videoWindow.YPosition, _videoWindow.Width, _videoWindow.Height));
        }
          // still needed?
        else if (_videoRectangle != null) // to be compatible to the old version
        {
          SetVideoWindow(new Rectangle(_videoRectangle.XPosition, _videoRectangle.YPosition, _videoRectangle.Width,
                                       _videoRectangle.Height));
        }
        else
        {
          // @ Bav: _videoWindow == null here -> System.NullReferenceException
          //SetVideoWindow(new Rectangle());
          Rectangle dummyRect = new Rectangle();
          if (!dummyRect.Equals(GUIGraphicsContext.VideoWindow))
          {
            GUIGraphicsContext.VideoWindow = dummyRect;
          }

          if (_videoWindow != null)
          {
            _videoWindow.SetVideoWindow = false; // avoid flickering if visualization is turned off
          }
        }
      }
      base.Render(timePassed);
    }

    private void SetVideoWindow(Rectangle newRect)
    {
      if (_visualisationEnabled && _videoWindow != null)
      {
        _videoWindow.SetVideoWindow = true;
        if (!newRect.Equals(GUIGraphicsContext.VideoWindow))
        {
          GUIGraphicsContext.VideoWindow = newRect;
        }
      }
    }

    public override bool Focused
    {
      get { return _isFocused; }
      set
      {
        _isFocused = value;
        if (_isFocused)
        {
          if (_videoWindow != null)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, (int)_videoWindow.GetID,
                                            0, 0, null);
            OnMessage(msg);
          }
        }
        else
        {
          foreach (GUIControl control in controlList)
          {
            control.Focus = false;
          }
        }
      }
    }

    protected override bool ShouldFocus(Action action)
    {
      return (action.wID == Action.ActionType.ACTION_MOVE_DOWN);
    }

    public override void OnAction(Action action)
    {
      base.OnAction(action);
      if ((action.wID == Action.ActionType.ACTION_MOVE_UP) ||
          (action.wID == Action.ActionType.ACTION_MOVE_RIGHT))
      {
        Focused = false;
      }
    }

    private MusicTag GetTag(string fileName)
    {
      MusicTag tag = null;

      // efforts only for important track
      bool isCurrent = (g_Player.CurrentFile == fileName);

      PlayListItem item = null;

      if (isCurrent)
      {
        item = playlistPlayer.GetCurrentItem();
      }
      else
      {
        item = playlistPlayer.GetNextItem();
      }

      if (item != null)
      {
        tag = (MusicTag)item.MusicTag;
      }

      if (tag == null)
      {
        tag = TagReader.TagReader.ReadTag(fileName);

        if (tag != null)
        {
          tag.Artist = Util.Utils.FormatMultiItemMusicStringTrim(tag.Artist, _stripArtistPrefixes);
        }
      }

      return tag;
    }

    private static void SetCurrentSkinProperties(MusicTag tag, String fileName)
    {
      var thumb = string.Empty;
      if (tag != null)
      {
        string strThumb = Util.Utils.GetAlbumThumbName(tag.Artist, tag.Album);
        if (Util.Utils.FileExistsInCache(strThumb))
        {
          thumb = strThumb;
        }

        // no succes with album cover try folder cache
        if (string.IsNullOrEmpty(thumb))
        {
          thumb = Util.Utils.TryEverythingToGetFolderThumbByFilename(fileName, false);
        }

        // let us test if there is a larger cover art image
        string strLarge = Util.Utils.ConvertToLargeCoverArt(thumb);
        if (Util.Utils.FileExistsInCache(strLarge))
        {
          thumb = strLarge;
        }

        GUIPropertyManager.SetProperty("#Play.Current.Thumb", thumb);

        // non-text values default to 0 and datetime.minvalue so
        // set the appropriate properties to string.empty

        // Duration
        string strDuration = tag.Duration <= 0
                               ? string.Empty
                               : MediaPortal.Util.Utils.SecondsToHMSString(tag.Duration);
        // Track
        string strTrack = tag.Track <= 0 ? string.Empty : tag.Track.ToString();
        // Year
        string strYear = tag.Year <= 1900 ? string.Empty : tag.Year.ToString();
        // Rating
        string strRating = (Convert.ToDecimal(2 * tag.Rating + 1)).ToString();
        // Bitrate
        string strBitrate = tag.BitRate <= 0 ? string.Empty : tag.BitRate.ToString();
        // Disc ID
        string strDiscID = tag.DiscID <= 0 ? string.Empty : tag.DiscID.ToString();
        // Disc Total
        string strDiscTotal = tag.DiscTotal <= 0 ? string.Empty : tag.DiscTotal.ToString();
        // Times played
        string strTimesPlayed = tag.TimesPlayed <= 0
                                  ? string.Empty
                                  : tag.TimesPlayed.ToString();
        // track total
        string strTrackTotal = tag.TrackTotal <= 0 ? string.Empty : tag.TrackTotal.ToString();
        // BPM
        string strBPM = tag.BPM <= 0 ? string.Empty : tag.BPM.ToString();
        // Channels
        string strChannels = tag.Channels <= 0 ? string.Empty : tag.Channels.ToString();
        // Sample Rate
        string strSampleRate = tag.SampleRate <= 0 ? string.Empty : tag.SampleRate.ToString();
        // Date Last Played
        string strDateLastPlayed = tag.DateTimePlayed == DateTime.MinValue
                                     ? string.Empty
                                     : tag.DateTimePlayed.ToShortDateString();
        // Date Added
        string strDateAdded = tag.DateTimeModified == DateTime.MinValue
                                ? string.Empty
                                : tag.DateTimeModified.ToShortDateString();

        GUIPropertyManager.SetProperty("#Play.Current.Title", tag.Title);
        GUIPropertyManager.SetProperty("#Play.Current.Track", strTrack);
        GUIPropertyManager.SetProperty("#Play.Current.Album", tag.Album);
        GUIPropertyManager.SetProperty("#Play.Current.Artist", tag.Artist);
        GUIPropertyManager.SetProperty("#Play.Current.Genre", tag.Genre);
        GUIPropertyManager.SetProperty("#Play.Current.Year", strYear);
        GUIPropertyManager.SetProperty("#Play.Current.Rating", strRating);
        GUIPropertyManager.SetProperty("#Play.Current.Duration", strDuration);
        GUIPropertyManager.SetProperty("#duration", strDuration);
        GUIPropertyManager.SetProperty("#Play.Current.AlbumArtist", tag.AlbumArtist);
        GUIPropertyManager.SetProperty("#Play.Current.BitRate", strBitrate);
        GUIPropertyManager.SetProperty("#Play.Current.Comment", tag.Comment);
        GUIPropertyManager.SetProperty("#Play.Current.Composer", tag.Composer);
        GUIPropertyManager.SetProperty("#Play.Current.Conductor", tag.Conductor);
        GUIPropertyManager.SetProperty("#Play.Current.DiscID", strDiscID);
        GUIPropertyManager.SetProperty("#Play.Current.DiscTotal", strDiscTotal);
        GUIPropertyManager.SetProperty("#Play.Current.Lyrics", tag.Lyrics);
        GUIPropertyManager.SetProperty("#Play.Current.TimesPlayed", strTimesPlayed);
        GUIPropertyManager.SetProperty("#Play.Current.TrackTotal", strTrackTotal);
        GUIPropertyManager.SetProperty("#Play.Current.FileType", tag.FileType);
        GUIPropertyManager.SetProperty("#Play.Current.Codec", tag.Codec);
        GUIPropertyManager.SetProperty("#Play.Current.BitRateMode", tag.BitRateMode);
        GUIPropertyManager.SetProperty("#Play.Current.BPM", strBPM);
        GUIPropertyManager.SetProperty("#Play.Current.Channels", strChannels);
        GUIPropertyManager.SetProperty("#Play.Current.SampleRate", strSampleRate);
        GUIPropertyManager.SetProperty("#Play.Current.DateLastPlayed", strDateLastPlayed);
        GUIPropertyManager.SetProperty("#Play.Current.DateAdded", strDateAdded);

        var albumInfo = new AlbumInfo();
        if (MusicDatabase.Instance.GetAlbumInfo(tag.Album, tag.AlbumArtist, ref albumInfo))
        {
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Review", albumInfo.Review);
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Rating", albumInfo.Rating.ToString());
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Genre", albumInfo.Genre);
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Styles", albumInfo.Styles);
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Tones", albumInfo.Tones);
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Year", albumInfo.Year.ToString());
        }
        else
        {
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Review", String.Empty);
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Rating", String.Empty);
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Genre", String.Empty);
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Styles", String.Empty);
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Tones", String.Empty);
          GUIPropertyManager.SetProperty("#Play.AlbumInfo.Year", String.Empty);
        }
        var artistInfo = new ArtistInfo();
        if (MusicDatabase.Instance.GetArtistInfo(tag.Artist, ref artistInfo))
        {
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Bio", artistInfo.AMGBio);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Born", artistInfo.Born);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Genres", artistInfo.Genres);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Instruments", artistInfo.Instruments);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Styles", artistInfo.Styles);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Tones", artistInfo.Tones);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.YearsActive", artistInfo.YearsActive);
        }
        else
        {
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Bio", String.Empty);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Born", String.Empty);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Genres", String.Empty);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Instruments", String.Empty);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Styles", String.Empty);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.Tones", String.Empty);
          GUIPropertyManager.SetProperty("#Play.ArtistInfo.YearsActive", String.Empty);
        }
      }
      else
      {
        // there is no current track so blank all properties
        GUIPropertyManager.RemovePlayerProperties();
        GUIPropertyManager.SetProperty("#Play.Current.Title", GUILocalizeStrings.Get(4543));
      }
    }

    private static void SetNextSkinProperties(MusicTag tag, String fileName)
    {
      var thumb = string.Empty;
      if (tag != null)
      {
        string strThumb = Util.Utils.GetAlbumThumbName(tag.Artist, tag.Album);
        if (Util.Utils.FileExistsInCache(strThumb))
        {
          thumb = strThumb;
        }

        // no succes with album cover try folder cache
        if (string.IsNullOrEmpty(thumb))
        {
          thumb = Util.Utils.TryEverythingToGetFolderThumbByFilename(fileName, false);
        }

        // let us test if there is a larger cover art image
        string strLarge = Util.Utils.ConvertToLargeCoverArt(thumb);
        if (Util.Utils.FileExistsInCache(strLarge))
        {
          thumb = strLarge;
        }

        if (!string.IsNullOrEmpty(thumb))
        {
          GUIPropertyManager.SetProperty("#Play.Next.Thumb", thumb);
        }

        // Duration
        string strDuration = tag.Duration <= 0
                       ? string.Empty
                       : MediaPortal.Util.Utils.SecondsToHMSString(tag.Duration);

        // Track
        string strNextTrack = tag.Track <= 0 ? string.Empty : tag.Track.ToString();

        // Year
        string strYear = tag.Year <= 1900 ? string.Empty : tag.Year.ToString();

        // Rating
        string strRating = (Convert.ToDecimal(2 * tag.Rating + 1)).ToString();

        GUIPropertyManager.SetProperty("#Play.Next.Duration", strDuration);
        GUIPropertyManager.SetProperty("#Play.Next.Title", tag.Title);
        GUIPropertyManager.SetProperty("#Play.Next.Track", strNextTrack);
        GUIPropertyManager.SetProperty("#Play.Next.Album", tag.Album);
        GUIPropertyManager.SetProperty("#Play.Next.Artist", tag.Artist);
        GUIPropertyManager.SetProperty("#Play.Next.Genre", tag.Genre);
        GUIPropertyManager.SetProperty("#Play.Next.Year", strYear);
        GUIPropertyManager.SetProperty("#Play.Next.Rating", strRating);
        GUIPropertyManager.SetProperty("#Play.Next.AlbumArtist", tag.AlbumArtist);
        GUIPropertyManager.SetProperty("#Play.Next.BitRate", tag.BitRate.ToString());
        GUIPropertyManager.SetProperty("#Play.Next.Comment", tag.Comment);
        GUIPropertyManager.SetProperty("#Play.Next.Composer", tag.Composer);
        GUIPropertyManager.SetProperty("#Play.Next.Conductor", tag.Conductor);
        GUIPropertyManager.SetProperty("#Play.Next.DiscID", tag.DiscID.ToString());
        GUIPropertyManager.SetProperty("#Play.Next.DiscTotal", tag.DiscTotal.ToString());
        GUIPropertyManager.SetProperty("#Play.Next.Lyrics", tag.Lyrics);
        GUIPropertyManager.SetProperty("#Play.Next.TimesPlayed", tag.TimesPlayed.ToString());
        GUIPropertyManager.SetProperty("#Play.Next.TrackTotal", tag.TrackTotal.ToString());
        GUIPropertyManager.SetProperty("#Play.Next.FileType", tag.FileType);
        GUIPropertyManager.SetProperty("#Play.Next.Codec", tag.Codec);
        GUIPropertyManager.SetProperty("#Play.Next.BitRateMode", tag.BitRateMode);
        GUIPropertyManager.SetProperty("#Play.Next.BPM", tag.BPM.ToString());
        GUIPropertyManager.SetProperty("#Play.Next.Channels", tag.Channels.ToString());
        GUIPropertyManager.SetProperty("#Play.Next.SampleRate", tag.SampleRate.ToString());
        GUIPropertyManager.SetProperty("#Play.Next.DateLastPlayed", tag.DateTimePlayed.ToShortDateString());
        GUIPropertyManager.SetProperty("#Play.Next.DateAdded", tag.DateTimeModified.ToShortDateString());
      }
      else
      {
        GUIPropertyManager.SetProperty("#Play.Next.Thumb", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Title", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Track", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Album", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Artist", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Genre", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Year", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Rating", "0");
        GUIPropertyManager.SetProperty("#Play.Next.AlbumArtist", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.BitRate", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Comment", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Composer", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Conductor", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.DiscID", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.DiscTotal", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Lyrics", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.TimesPlayed", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.TrackTotal", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.FileType", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Codec", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.BitRateMode", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.BPM", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.Channels", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.SampleRate", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.DateLastPlayed", string.Empty);
        GUIPropertyManager.SetProperty("#Play.Next.DateAdded", string.Empty);
      }
    }

    #region playback Events

    private void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }

      GUIGraphicsContext.form.BeginInvoke(new PlaybackChangedDelegate(DoOnEnded), new object[] {type, filename});
    }

    private void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Music && type != g_Player.MediaType.Radio)
      {
        return;
      }

      GUIGraphicsContext.form.BeginInvoke(new PlaybackChangedDelegate(DoOnStarted), new object[] {type, filename});
    }

    private void OnPlaylistChanged(PlayListType nPlayList, PlayList playlist)
    {
      // changes to the current track are dealt with by g_player events
      // but user can udpate the playlist without firing g_player events
      // make sure the next track details shown are correct
      if ((_playlistIsCurrent && nPlayList == PlayListType.PLAYLIST_MUSIC) ||
          (!_playlistIsCurrent && nPlayList == PlayListType.PLAYLIST_MUSIC_TEMP))
      {
        var nextFilename = playlistPlayer.GetNext();
        if (!string.IsNullOrEmpty(nextFilename))
        {
          var tag = GetTag(nextFilename);
          SetNextSkinProperties(tag, nextFilename);
        }
        else
        {
          SetNextSkinProperties(null, string.Empty);
        }
      }
    }


    private void DoOnStarted(g_Player.MediaType type, string filename)
    {
      var isInternetStream = Util.Utils.IsAVStream(filename);
      MusicTag tag;

      if (string.IsNullOrEmpty(filename))
      {
        return;
      }
      // last.fm radio sets properties manually therefore do not overwrite them.
      if (Util.Utils.IsLastFMStream(filename))
      {
        return;
      }

      // Radio Properties are set already in Play routine
      if (g_Player.IsRadio)
      {
        return;
      }

      // When Playing an Internet Stream, via BASS, skin properties are set during the Play method in BassAudio.cs
      if (BassMusicPlayer.IsDefaultMusicPlayer && isInternetStream)
      {
        return;
      }
      else
      {
        tag = GetTag(filename);
      }

      SetCurrentSkinProperties(tag, filename);

      if (isInternetStream)
      {
        SetNextSkinProperties(null, string.Empty);
        return;
      }

      // Show Information of Next File in Playlist
      string nextFilename = playlistPlayer.GetNext();
      if (nextFilename == string.Empty)
      {
        SetNextSkinProperties(null, string.Empty);
        return;
      }
      tag = GetTag(nextFilename);
      SetNextSkinProperties(tag, nextFilename);
    }


    private void DoOnEnded(g_Player.MediaType type, string filename)
    {
      if (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio)
      {
        GUIPropertyManager.RemovePlayerProperties();
      }
    }

    #endregion

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return DoesPostRender();
    }

    public void RenderLayer(float timePassed)
    {
      PostRender(timePassed, 2);
    }

    #endregion

    public override void Dispose()
    {
      _videoRectangle.SafeDispose();
      _thumbImage.SafeDispose();
      _labelPlayTime.SafeDispose();
      _imagePlayLogo.SafeDispose();
      _imagePauseLogo.SafeDispose();
      _labelInfo.SafeDispose();
      _labelBigPlayTime.SafeDispose();
      _imageFastForward.SafeDispose();
      _imageRewind.SafeDispose();
      _videoWindow.SafeDispose();
      _imageNormal.SafeDispose();
      _imageGapless.SafeDispose();
      _imageCrossfade.SafeDispose();
      base.Dispose();
    }
  }
}