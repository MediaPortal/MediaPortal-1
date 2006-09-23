#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.TagReader;
using MediaPortal.Util;

namespace MediaPortal.GUI.RADIOLASTFM
{
  public class GUIRadioLastFM : GUIWindow, ISetupForm, IShowPlugin
  {
    private enum SkinControlIDs
    {
      BTN_START_STREAM = 10,
      LIST_TRACK_TAGS = 55,
      IMG_ARTIST_ART = 112,
    }

    [SkinControlAttribute((int)SkinControlIDs.BTN_START_STREAM)]    protected GUIButtonControl btnStartStream = null;
    [SkinControlAttribute((int)SkinControlIDs.LIST_TRACK_TAGS)]     protected GUIListControl facadeTrackTags = null;
    [SkinControlAttribute((int)SkinControlIDs.IMG_ARTIST_ART)]      protected GUIImage imgArtistArt = null;

    private AudioscrobblerUtils InfoScrobbler = null;
    private StreamControl LastFMStation = null;
    private NotifyIcon _trayBallonSongChange = null;
    private ScrobblerUtilsRequest _lastTrackTagRequest;
    private ScrobblerUtilsRequest _lastArtistCoverRequest;

    // constructor
    public GUIRadioLastFM()
    {
      GetID = (int)GUIWindow.Window.WINDOW_RADIO_LASTFM;
    }


    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\MyRadioLastFM.xml");

      LastFMStation = new StreamControl();
      InfoScrobbler = AudioscrobblerUtils.Instance;      

      if (_trayBallonSongChange == null)
      {
        String BaseDir = Config.Get(Config.Dir.Base);
        _trayBallonSongChange = new NotifyIcon();
        if (System.IO.File.Exists(BaseDir + @"BallonRadio.ico"))
          _trayBallonSongChange.Icon = new Icon(BaseDir + @"BallonRadio.ico");
        else
          _trayBallonSongChange.Icon = SystemIcons.Information;

        //if (System.IO.File.Exists(BaseDir + @"BallonTrack.ico"))
        //  _trayBallonSongChange.BalloonTipIcon = new ToolTipIcon(BaseDir + @"BallonTrack.ico");

        _trayBallonSongChange.Text = "MediaPortal Last.fm Radio";
        _trayBallonSongChange.Visible = false;
      }

      //g_Player.PlayBackStarted += new g_Player.StartedHandler(g_Player_PlayBackStarted);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(PlayBackStoppedHandler);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(PlayBackEndedHandler);

      LastFMStation.StreamSongChanged += new StreamControl.SongChangedHandler(LastFMStation_StreamSongChanged);
      return bResult;
    }

    #region Serialisation
    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {

      }
      if (!LastFMStation.IsInit)
        LastFMStation.LoadConfig();
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
      }
    }
    #endregion


    #region BaseWindow Members
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_NEXT_ITEM && (int)LastFMStation.CurrentStreamState > 2)
      {
        LastFMStation.SendControlCommand(StreamControls.skiptrack);
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      if (_trayBallonSongChange != null)
        _trayBallonSongChange.Visible = true;

      if (facadeTrackTags != null)
        facadeTrackTags.Focusable = false;

      LoadSettings();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if (_trayBallonSongChange != null)
        _trayBallonSongChange.Visible = false;

      SaveSettings();
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

      base.DeInit();
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnStartStream)
      {
        bool isSubscriber = LastFMStation.IsSubscriber;
        StreamType TuneIntoSelected = LastFMStation.CurrentTuneType;

        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);

        if (dlg == null)
          return;

        dlg.Reset();
        dlg.SetHeading(34001);                // Start Stream
        dlg.Add("Recommendation radio");
        //dlg.Add("Neighbour radio");
        // temporary
        dlg.Add("Tune into Tag: Cover");
        dlg.Add("MediaPortal User's group radio");

        if (isSubscriber)
          dlg.Add("Personal radio");
        //....

        dlg.DoModal(GetID);

        if (dlg.SelectedId == -1)
          return;

        // this will be buttons soon containing chosen users
        switch (dlg.SelectedLabelText)
        {
          case "Recommendation radio":
            TuneIntoSelected = StreamType.Recommended;
            LastFMStation.StreamsUser = LastFMStation.AccountUser;
            break;
          //case "Neighbour radio":
          //  TuneIntoSelected = StreamType.Personal;
          //  LastFMStation.StreamsUser. 
          case "Tune into Tag: Cover":
            TuneIntoSelected = StreamType.Tags;
            //LastFMStation.StreamsUser = LastFMStation.AccountUser;
            break;
          case "MediaPortal User's group radio":
            TuneIntoSelected = StreamType.Group;
            LastFMStation.StreamsUser = "MediaPortal Users";
            break;
          case "Personal radio":
            TuneIntoSelected = StreamType.Personal;
            LastFMStation.StreamsUser = LastFMStation.AccountUser;
            break;
        }
        if (LastFMStation.CurrentStreamState == StreamPlaybackState.nocontent)
          LastFMStation.CurrentStreamState = StreamPlaybackState.initialized;

        g_Player.Stop();
        // LastFMStation.CurrentTuneType = TuneIntoSelected;
        switch (TuneIntoSelected)
        {
          case StreamType.Recommended:
            LastFMStation.TuneIntoRecommendedRadio(LastFMStation.StreamsUser);
            break;            

          case StreamType.Group:
            LastFMStation.TuneIntoGroupRadio(LastFMStation.StreamsUser);
            break;

          case StreamType.Personal:
            LastFMStation.TuneIntoPersonalRadio(LastFMStation.AccountUser);
            break;

          case StreamType.Tags:
            List<String> MyTags = new List<string>();
            MyTags.Add("cover");
            //MyTags.Add("melodic death metal");
            LastFMStation.TuneIntoTags(MyTags);
            break;
        }


        if (LastFMStation.CurrentStreamState == StreamPlaybackState.initialized)
        {
          LastFMStation.PlayStream();
        }
        else
          Log.Info("GUIRadio: Didn't start LastFM radio because stream state is {0}", LastFMStation.CurrentStreamState.ToString());
      }
      base.OnClicked(controlId, control, actionType);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_SHOW_BALLONTIP_SONGCHANGE:
          ShowSongTrayBallon(message.Label, message.Label2, message.Param1);
          break;
      }
      return base.OnMessage(message);
    }
    #endregion    
    

    #region Internet Lookups
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
        InfoScrobbler.AddRequestAsync(request);
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
        String ThumbFileName = Util.Utils.GetCoverArtName(Thumbs.MusicArtists, Util.Utils.FilterFileName(LastFMStation.CurrentTrackTag.Artist));
        if (ThumbFileName.Length > 0)
        {
          imgArtistArt.SetFileName(ThumbFileName);
        }
      }
      else
      {
        Log.Warn("NowPlaying.OnUpdateArtistInfoCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    public void OnUpdateTrackTagsInfoCompleted(TagsForTrackRequest request, List<Song> TagTracks)
    {
      if (request.Equals(_lastTrackTagRequest))
      {
        GUIListItem item = null;
        //TagTracks = InfoScrobbler.getTagsForTrack(_trackArtist, _trackTitle);
        {
          facadeTrackTags.Clear();

          for (int i = 0; i < TagTracks.Count; i++)
          {
            item = new GUIListItem(TagTracks[i].ToShortString());
            item.Label = TagTracks[i].Genre;

            facadeTrackTags.Add(item);

            // display 3 items only
            if (i >= 2)
              break;
          }
        }
      }
      else
      {
        Log.Warn("NowPlaying.OnUpdateTagInfoCompleted: unexpected response for request: {0}", request.Type);
      }
    }

    #endregion


    #region Handlers
    void LastFMStation_StreamSongChanged(MusicTag newCurrentSong, DateTime startTime)
    {
      if (_lastTrackTagRequest != null)
        InfoScrobbler.RemoveRequest(_lastTrackTagRequest);
      if (_lastArtistCoverRequest != null)
        InfoScrobbler.RemoveRequestAsync(_lastArtistCoverRequest);

      if (LastFMStation.CurrentTrackTag != null)
        if (LastFMStation.CurrentTrackTag.Artist != String.Empty)
        {
          UpdateArtistInfo(newCurrentSong.Artist);

          if (LastFMStation.CurrentTrackTag.Title != String.Empty)
            UpdateTrackTagsInfo(LastFMStation.CurrentTrackTag.Artist, LastFMStation.CurrentTrackTag.Title);
        }
    }

    protected void PlayBackStoppedHandler(g_Player.MediaType type, int stoptime, string filename)
    {
      if (!filename.Contains(@"last.fm/last.mp3") || LastFMStation.CurrentStreamState != StreamPlaybackState.streaming)
        return;

      LastFMStation.CurrentStreamState = StreamPlaybackState.initialized;
    }

    protected void PlayBackEndedHandler(g_Player.MediaType type, string filename)
    {
      if (!filename.Contains(@"last.fm/last.mp3") || LastFMStation.CurrentStreamState != StreamPlaybackState.streaming)
        return;

      //GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);

      //if (dlg == null)
      //  return;

      //dlg.Reset();
      //dlg.SetHeading(924);                // Menu
      //dlg.Add("No more content for this selection");

      //dlg.DoModal(GetID);

      //if (dlg.SelectedId == -1)
      //  return;
      Log.Info("GUIRadio: No more content for this stream");
      LastFMStation.CurrentStreamState = StreamPlaybackState.nocontent;
      //dlg.AddLocalizedString(930);        //Add to favorites
    }  

    #endregion


    #region Utils
    void ShowSongTrayBallon(String notifyTitle, String notifyMessage_, int showSeconds_)
    {
      if (_trayBallonSongChange != null)
      {
        // XP hides "inactive" icons therefore change the text
        _trayBallonSongChange.Text = "MediaPortal";
        _trayBallonSongChange.Text = "MediaPortal Last.fm Radio";
        _trayBallonSongChange.Visible = true;

        _trayBallonSongChange.BalloonTipTitle = notifyTitle;
        _trayBallonSongChange.BalloonTipText = notifyMessage_;
        _trayBallonSongChange.ShowBalloonTip(showSeconds_);

        // needs some sleep or it will vanish inmediately
        //_trayBallonSongChange.Visible = false;
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
      return "My Last.fm Radio";
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
      bool AudioScrobblerOn = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        AudioScrobblerOn = xmlreader.GetValueAsBool("plugins", "Audioscrobbler", false);
      }
      return AudioScrobblerOn;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public bool HasSetup()
    {
      return true;
    }
    #endregion


    #region IShowPlugin Members
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(34000);
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = "hover_my radio.png";
      return true;
    }

    public bool ShowDefaultHome()
    {
      return false;
    }

    public void ShowPlugin()
    {
      MessageBox.Show("Nothing to setup now. \nPlaying your recommendation radio.");
    }
    #endregion
  }
}