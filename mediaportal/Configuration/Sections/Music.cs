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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MediaPortal.MusicPlayer.BASS;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Visualization;
using Un4seen.Bass;
using BassVis_Api;
using Un4seen.BassAsio;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class Music : SectionSettings
  {
    private delegate void LoadVisualizationListDelegate(List<VisualizationInfo> vizPluginsInfo);

    #region Variables

    private const string JumpToValue0 = "none";
    private const string JumpToValue1 = "nowPlayingAlways";
    private const string JumpToValue2 = "nowPlayingMultipleItems";
    private const string JumpToValue3 = "currentPlaylistAlways";
    private const string JumpToValue4 = "currentPlaylistMultipleItems";
    private const string JumpToValue5 = "fullscreenAlways";
    private const string JumpToValue6 = "fullscreenMultipleItems";

    private const string JumpToOption0 = "None";
    private const string JumpToOption1 = "Now Playing [always]";
    private const string JumpToOption2 = "Now Playing [if multiple items]";
    private const string JumpToOption3 = "Current playlist [always]";
    private const string JumpToOption4 = "Current playlist [if multiple items]";
    private const string JumpToOption5 = "Fullscreen [always] (internal music player only)";
    private const string JumpToOption6 = "Fullscreen [if multiple items] (internal music player only)";

    private string[] JumpToValues = new string[]
                                      {
                                        JumpToValue0,
                                        JumpToValue1,
                                        JumpToValue2,
                                        JumpToValue3,
                                        JumpToValue4,
                                        JumpToValue5,
                                        JumpToValue6,
                                      };

    private string[] JumpToOptions = new string[]
                                       {
                                         JumpToOption0,
                                         JumpToOption1,
                                         JumpToOption2,
                                         JumpToOption3,
                                         JumpToOption4,
                                         JumpToOption5,
                                         JumpToOption6,
                                       };

    private string[] autoPlayOptions = new string[]
                                         {
                                           "Autoplay, never ask",
                                           "Don't autoplay, never ask",
                                           "Ask every time a CD is inserted"
                                         };

    private string[] PlayerOptions = new string[]
                                       {
                                         "BASS engine",
                                         "ASIO",
                                         "WasAPI",
                                         "Internal dshow player",
                                       };

    private const string LyricsValue0 = "never";
    private const string LyricsValue1 = "asOverlay";
    private const string LyricsValue2 = "asVisualCue";

    private const string LyricsOption0 = "Never";
    private const string LyricsOption1 = "Display as an overlay";
    private const string LyricsOption2 = "Show visual cue that lyrics are available";

    private string[] ShowLyricsOptions = new string[]
                                           {
                                             LyricsOption0,
                                             LyricsOption1,
                                             LyricsOption2
                                           };

    private const string VUMeterValue0 = "none";
    private const string VUMeterValue1 = "analog";
    private const string VUMeterValue2 = "led";

    private IVisualizationManager IVizMgr = null;
    private VisualizationInfo VizPluginInfo = null;
    private bool VisualizationsInitialized = false;
    private BASSVIS_PARAM _visParam = null;

    private string _soundDevice = null;

    #endregion

    #region ctor

    public Music()
      : this("Music") {}

    public Music(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Set available media players
      audioPlayerComboBox.Items.Clear();
      audioPlayerComboBox.Items.AddRange(PlayerOptions);

      PlayNowJumpToCmbBox.Items.Clear();
      PlayNowJumpToCmbBox.Items.AddRange(JumpToOptions);

      ShowLyricsCmbBox.Items.Clear();
      ShowLyricsCmbBox.Items.AddRange(ShowLyricsOptions);
    }

    #endregion

    #region Activation Init

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      hScrollBarBuffering_ValueChanged(null, null);
      hScrollBarCrossFade_ValueChanged(null, null);
      audioPlayerComboBox_SelectedIndexChanged(null, null);
      GaplessPlaybackChkBox_CheckedChanged(null, null);
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      audioPlayerComboBox.Enabled = SettingsForm.AdvancedMode;
    }

    #endregion

    #region Load / Save Settings

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        // Player Settings
        // Get first the sound device, so that it is available, when updating the combo
        _soundDevice = xmlreader.GetValueAsString("audioplayer", "sounddevice", "None");

        string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "player", "0");
        int audioPlayer = (int)AudioPlayer.Bass; // Default to BASS Player
        try
        {
          audioPlayer = Convert.ToInt16(strAudioPlayer);
        }
        catch (Exception) // We end up here in the conversion Phase, where we have still a string ioncluded
        { }

        audioPlayerComboBox.SelectedIndex = audioPlayer;

        #region General Bass Player Settings 

        int crossFadeMS = xmlreader.GetValueAsInt("audioplayer", "crossfade", 4000);

        if (crossFadeMS < 0)
        {
          crossFadeMS = 4000;
        }

        else if (crossFadeMS > hScrollBarCrossFade.Maximum)
        {
          crossFadeMS = hScrollBarCrossFade.Maximum;
        }

        hScrollBarCrossFade.Value = crossFadeMS;

        int bufferingMS = xmlreader.GetValueAsInt("audioplayer", "buffering", 5000);

        if (bufferingMS < hScrollBarBuffering.Minimum)
        {
          bufferingMS = hScrollBarBuffering.Minimum;
        }

        else if (bufferingMS > hScrollBarBuffering.Maximum)
        {
          bufferingMS = hScrollBarBuffering.Maximum;
        }

        hScrollBarBuffering.Value = bufferingMS;

        GaplessPlaybackChkBox.Checked = xmlreader.GetValueAsBool("audioplayer", "gaplessPlayback", false);
        FadeOnStartStopChkbox.Checked = xmlreader.GetValueAsBool("audioplayer", "fadeOnStartStop", true);
        StreamOutputLevelNud.Value = (decimal)xmlreader.GetValueAsInt("audioplayer", "streamOutputLevel", 85);

        #endregion

        #region BASS ASIO

        hScrollBarBalance.Value = xmlreader.GetValueAsInt("audioplayer", "asiobalance", 0);


        #endregion

        #region Visualization Settings
        int vizType = xmlreader.GetValueAsInt("musicvisualization", "vizType", (int)VisualizationInfo.PluginType.None);
        string vizName = xmlreader.GetValueAsString("musicvisualization", "name", "None");
        string vizPath = xmlreader.GetValueAsString("musicvisualization", "path", "");
        string vizClsid = xmlreader.GetValueAsString("musicvisualization", "clsid", "");
        int vizPreset = xmlreader.GetValueAsInt("musicvisualization", "preset", 0);

        if (vizType == (int)VisualizationInfo.PluginType.None
            && vizName == "None")
        {
          VizPluginInfo = new VisualizationInfo("None", true);
        }

        else
        {
          VizPluginInfo = new VisualizationInfo((VisualizationInfo.PluginType)vizType, vizPath, vizName, vizClsid,
                                                vizPreset);
        }

        int fps = xmlreader.GetValueAsInt("musicvisualization", "fps", 25);

        if (fps < (int)VisualizationFpsNud.Minimum)
        {
          fps = (int)VisualizationFpsNud.Minimum;
        }

        else if (fps > VisualizationFpsNud.Maximum)
        {
          fps = (int)VisualizationFpsNud.Maximum;
        }

        VisualizationFpsNud.Value = fps;

        EnableStatusOverlaysChkBox.Checked = xmlreader.GetValueAsBool("musicvisualization", "enableStatusOverlays",
                                                                      false);
        ShowTrackInfoChkBox.Checked = xmlreader.GetValueAsBool("musicvisualization", "showTrackInfo", true);
        EnableStatusOverlaysChkBox_CheckedChanged(null, null);

        #endregion

        #region Playlist Settings
        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";
        playlistFolderTextBox.Text = xmlreader.GetValueAsString("music", "playlists", playListFolder);

        if (string.Compare(playlistFolderTextBox.Text, playListFolder) == 0)
        {
          if (Directory.Exists(playListFolder) == false)
          {
            try
            {
              Directory.CreateDirectory(playListFolder);
            }
            catch (Exception) {}
          }
        }

        repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "repeat", false);
        autoShuffleCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", false);

        SavePlaylistOnExitChkBox.Checked = xmlreader.GetValueAsBool("musicfiles", "savePlaylistOnExit", true);
        ResumePlaylistChkBox.Checked = xmlreader.GetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", true);
        PlaylistCurrentCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "playlistIsCurrent", true);

        String strSelectOption = xmlreader.GetValueAsString("musicfiles", "selectOption", "play");
        cmbSelectOption.Text = strSelectOption == "play" ? "Play" : "Queue";
        chkAddAllTracks.Checked = xmlreader.GetValueAsBool("musicfiles", "addall", true);
        #endregion

        #region Misc Settings
        string playNowJumpTo = xmlreader.GetValueAsString("music", "playnowjumpto", JumpToValue0);

        switch (playNowJumpTo)
        {
          case JumpToValue0:
            PlayNowJumpToCmbBox.Text = JumpToOptions[0];
            break;

          case JumpToValue1:
            PlayNowJumpToCmbBox.Text = JumpToOptions[1];
            break;

          case JumpToValue2:
            PlayNowJumpToCmbBox.Text = JumpToOptions[2];
            break;

          case JumpToValue3:
            PlayNowJumpToCmbBox.Text = JumpToOptions[3];
            break;

          case JumpToValue4:
            PlayNowJumpToCmbBox.Text = JumpToOptions[4];
            break;

          case JumpToValue5:
            PlayNowJumpToCmbBox.Text = JumpToOptions[5];
            break;

          case JumpToValue6:
            PlayNowJumpToCmbBox.Text = JumpToOptions[6];
            break;

          default:
            PlayNowJumpToCmbBox.Text = JumpToOptions[0];
            break;
        }

        string showLyrics = xmlreader.GetValueAsString("musicmisc", "lyrics", LyricsValue0);

        switch (showLyrics)
        {
          case LyricsValue0:
            ShowLyricsCmbBox.Text = ShowLyricsOptions[0];
            break;

          case LyricsValue1:
            ShowLyricsCmbBox.Text = ShowLyricsOptions[1];
            break;

          case LyricsValue2:
            ShowLyricsCmbBox.Text = ShowLyricsOptions[2];
            break;

          default:
            ShowLyricsCmbBox.Text = ShowLyricsOptions[0];
            break;
        }

        ShowVizInNowPlayingChkBox.Checked = xmlreader.GetValueAsBool("musicmisc", "showVisInNowPlaying", false);
        checkBoxDisableCoverLookups.Checked = !(xmlreader.GetValueAsBool("musicmisc", "fetchlastfmcovers", true));
        checkBoxDisableAlbumLookups.Checked = !(xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtopalbums", true));
        checkBoxDisableTagLookups.Checked = !(xmlreader.GetValueAsBool("musicmisc", "fetchlastfmtracktags", true));
        checkBoxSwitchArtistOnLastFMSubmit.Checked = xmlreader.GetValueAsBool("musicmisc", "switchArtistOnLastFMSubmit",
                                                                              false);

        string vuMeter = xmlreader.GetValueAsString("musicmisc", "vumeter", "none");

        switch (vuMeter)
        {
          case VUMeterValue0:
            radioButtonVUNone.Checked = true;
            break;

          case VUMeterValue1:
            radioButtonVUAnalog.Checked = true;
            break;

          case VUMeterValue2:
            radioButtonVULed.Checked = true;
            break;

          default:
            radioButtonVUNone.Checked = true;
            break;
        }
        #endregion
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        #region Player Settings
        xmlwriter.SetValue("audioplayer", "player", audioPlayerComboBox.SelectedIndex);
        xmlwriter.SetValue("audioplayer", "sounddevice", soundDeviceComboBox.Text);

        xmlwriter.SetValue("audioplayer", "crossfade", hScrollBarCrossFade.Value);
        xmlwriter.SetValue("audioplayer", "buffering", hScrollBarBuffering.Value);
        xmlwriter.SetValueAsBool("audioplayer", "fadeOnStartStop", FadeOnStartStopChkbox.Checked);
        xmlwriter.SetValueAsBool("audioplayer", "gaplessPlayback", GaplessPlaybackChkBox.Checked);
        xmlwriter.SetValue("audioplayer", "streamOutputLevel", StreamOutputLevelNud.Value);

        xmlwriter.SetValue("audioplayer", "asiobalance", hScrollBarBalance.Value);
        #endregion

        #region Visualization Settings
        if (IVizMgr != null && VisualizationsCmbBox.SelectedIndex > 0) // Something else than "None" selected
        {
          List<VisualizationInfo> vizPluginsInfo = IVizMgr.VisualizationPluginsInfo;
          int selIndex = VisualizationsCmbBox.SelectedIndex;

          if (selIndex < 0 || selIndex >= vizPluginsInfo.Count)
          {
            selIndex = 0;
          }

          xmlwriter.SetValue("musicvisualization", "name", vizPluginsInfo[selIndex].Name);
          xmlwriter.SetValue("musicvisualization", "vizType",
                             ((int)vizPluginsInfo[selIndex].VisualizationType).ToString());
          xmlwriter.SetValue("musicvisualization", "path", vizPluginsInfo[selIndex].FilePath);
          xmlwriter.SetValue("musicvisualization", "clsid", vizPluginsInfo[selIndex].CLSID);
          xmlwriter.SetValue("musicvisualization", "preset", vizPluginsInfo[selIndex].PresetIndex.ToString());
          xmlwriter.SetValueAsBool("musicfiles", "doVisualisation", true);
        }
        else if (VizPluginInfo.VisualizationType != VisualizationInfo.PluginType.None)
          // This is the case, when we started Config without activating the Vis Tab
        {
          xmlwriter.SetValue("musicvisualization", "name", VizPluginInfo.Name);
          xmlwriter.SetValue("musicvisualization", "vizType", ((int)VizPluginInfo.VisualizationType).ToString());
          xmlwriter.SetValue("musicvisualization", "path", VizPluginInfo.FilePath);
          xmlwriter.SetValue("musicvisualization", "clsid", VizPluginInfo.CLSID);
          xmlwriter.SetValue("musicvisualization", "preset", VizPluginInfo.PresetIndex.ToString());
          xmlwriter.SetValueAsBool("musicfiles", "doVisualisation", true);
        }
        else
        {
          xmlwriter.SetValue("musicvisualization", "name", "");
          xmlwriter.SetValue("musicvisualization", "vizType", 0);
          xmlwriter.SetValue("musicvisualization", "path", "");
          xmlwriter.SetValue("musicvisualization", "clsid", "");
          xmlwriter.SetValue("musicvisualization", "preset", "");
          xmlwriter.SetValueAsBool("musicfiles", "doVisualisation", false);
        }

        xmlwriter.SetValue("musicvisualization", "fps", VisualizationFpsNud.Value);

        xmlwriter.SetValueAsBool("musicvisualization", "enableStatusOverlays", EnableStatusOverlaysChkBox.Checked);
        xmlwriter.SetValueAsBool("musicvisualization", "showTrackInfo", ShowTrackInfoChkBox.Checked);
        #endregion

        #region Playlist Settings
        xmlwriter.SetValue("music", "playlists", playlistFolderTextBox.Text);
        xmlwriter.SetValueAsBool("musicfiles", "repeat", repeatPlaylistCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "autoshuffle", autoShuffleCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "savePlaylistOnExit", SavePlaylistOnExitChkBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", ResumePlaylistChkBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "playlistIsCurrent", PlaylistCurrentCheckBox.Checked);

        //Play behaviour
        xmlwriter.SetValue("musicfiles", "selectOption", cmbSelectOption.Text.ToLower());
        xmlwriter.SetValueAsBool("musicfiles", "addall", chkAddAllTracks.Checked);
        #endregion

        #region Misc Settings
        string playNowJumpTo = string.Empty;

        switch (PlayNowJumpToCmbBox.Text)
        {
          case JumpToOption0:
            playNowJumpTo = JumpToValue0;
            break;

          case JumpToOption1:
            playNowJumpTo = JumpToValue1;
            break;

          case JumpToOption2:
            playNowJumpTo = JumpToValue2;
            break;

          case JumpToOption3:
            playNowJumpTo = JumpToValue3;
            break;

          case JumpToOption4:
            playNowJumpTo = JumpToValue4;
            break;

          case JumpToOption5:
            playNowJumpTo = JumpToValue5;
            break;

          case JumpToOption6:
            playNowJumpTo = JumpToValue6;
            break;

          default:
            playNowJumpTo = JumpToValue0;
            break;
        }

        xmlwriter.SetValue("music", "playnowjumpto", playNowJumpTo);

        string showLyrics = string.Empty;

        switch (ShowLyricsCmbBox.Text)
        {
          case LyricsOption0:
            showLyrics = LyricsValue0;
            break;

          case LyricsOption1:
            showLyrics = LyricsValue1;
            break;

          case LyricsOption2:
            showLyrics = LyricsValue2;
            break;
        }

        xmlwriter.SetValue("musicmisc", "lyrics", showLyrics);
        xmlwriter.SetValueAsBool("musicmisc", "showVisInNowPlaying", ShowVizInNowPlayingChkBox.Checked);
        xmlwriter.SetValueAsBool("musicmisc", "fetchlastfmcovers", !checkBoxDisableCoverLookups.Checked);
        xmlwriter.SetValueAsBool("musicmisc", "fetchlastfmtopalbums", !checkBoxDisableAlbumLookups.Checked);
        xmlwriter.SetValueAsBool("musicmisc", "fetchlastfmtracktags", !checkBoxDisableTagLookups.Checked);
        xmlwriter.SetValueAsBool("musicmisc", "switchArtistOnLastFMSubmit", checkBoxSwitchArtistOnLastFMSubmit.Checked);

        string vuMeter = VUMeterValue0;

        if (radioButtonVUAnalog.Checked)
        {
          vuMeter = VUMeterValue1;
        }
        else if (radioButtonVULed.Checked)
        {
          vuMeter = VUMeterValue2;
        }

        xmlwriter.SetValue("musicmisc", "vumeter", vuMeter);
        
        #endregion
      }

      // Make sure we shut down the viz engine
      if (IVizMgr != null)
      {
        IVizMgr.Stop();
        IVizMgr.ShutDown();
      }
    }

    #endregion

    #region Public Methods

    public override object GetSetting(string name)
    {
      switch (name.ToLower())
      {
        case "audioplayer":
          return audioPlayerComboBox.SelectedItem.ToString();
      }

      return null;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Allows selection of the Playlist folder
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void playlistButton_Click(object sender, EventArgs e)
    {
      using (folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = "Select the folder where music playlists will be stored";
        folderBrowserDialog.ShowNewFolderButton = true;
        folderBrowserDialog.SelectedPath = playlistFolderTextBox.Text;
        DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          playlistFolderTextBox.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }

    /// <summary>
    /// A new Tab Page has been selected
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MusicSettingsTabCtl_SelectedIndexChanged(object sender, EventArgs e)
    {
      // If the user has selected the Vis Tab, then we need to see, if we have BASS Player active
      if (MusicSettingsTabCtl.SelectedTab.Equals(VisualizationsTabPg))
      {
        if (audioPlayerComboBox.SelectedIndex < 3)
        {
          VisualizationsTabPg.Enabled = true;

          if (!VisualizationsInitialized)
          {
            Application.DoEvents();

            InitializeVizEngine();
          }
        }

        else
        {
          VisualizationsTabPg.Enabled = false;
          MessageBox.Show(this, "Visualization settings are only available with the BASS music player.",
                          "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
    }

    /// <summary>
    /// A new Audio Device has been selected.
    /// We need to get the Sound devices supported by this device
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void audioPlayerComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      // Check if we have selected either BASS; ASIO or WasAPI and only enable Playersettings and Vis then
      bool useBassEngine = audioPlayerComboBox.SelectedIndex < 3;
      tabControlPlayerSettings.Enabled = useBassEngine;
      groupBoxVizOptions.Enabled = useBassEngine;

      switch (audioPlayerComboBox.SelectedIndex)
      {
        case 0:
          tabPageBassPlayerSettings.Enabled = true;
          tabPageASIOPlayerSettings.Enabled = false;
          tabPageWASAPIPLayerSettings.Enabled = false;
          tabControlPlayerSettings.SelectedTab = tabPageBassPlayerSettings;
          break;

        case 1:
          tabPageBassPlayerSettings.Enabled = true;
          tabPageASIOPlayerSettings.Enabled = true;
          tabPageWASAPIPLayerSettings.Enabled = false;
          tabControlPlayerSettings.SelectedTab = tabPageASIOPlayerSettings;
          break;

        case 2:
          tabPageBassPlayerSettings.Enabled = true;
          tabPageASIOPlayerSettings.Enabled = false;
          tabPageWASAPIPLayerSettings.Enabled = true;
          tabControlPlayerSettings.SelectedTab = tabPageWASAPIPLayerSettings;
          break;
      }

      soundDeviceComboBox.Items.Clear();
      GetAvailableSoundDevices(audioPlayerComboBox.SelectedIndex);

      if (soundDeviceComboBox.Items.Count > 0)
      {
        // On first usage, we don't have any sound device
        if (_soundDevice == "None")
        {
          soundDeviceComboBox.SelectedIndex = 0;
        }
        else
        {
          soundDeviceComboBox.SelectedItem = _soundDevice;
        }

        // Change the Text of Item 0 in the Sound device box for Bass Player or DirectShow Player
        if (audioPlayerComboBox.SelectedIndex == 0)
        {
          soundDeviceComboBox.Items[0] = "Default Sound Device";
        }
        else if (audioPlayerComboBox.SelectedIndex == 3)
        {
          soundDeviceComboBox.Items[0] = "Default DirectSound Device";
        }
      }
    }

    /// <summary>
    /// Get the sound devices for the selected Player
    /// </summary>
    /// <param name="player">
    /// 0 - Bass Directshow
    /// 1 - ASIO
    /// 2 - WasApi
    /// 3 - Internal Dshow Player
    /// </param>
    private void GetAvailableSoundDevices(int player)
    {
      switch (player)
      {
        case (int)AudioPlayer.Bass:
        case (int)AudioPlayer.DShow: 

          // Get all available devices and add them to the combo box
          BASS_DEVICEINFO[] soundDevices = Bass.BASS_GetDeviceInfos();

          // For Directshow player, we need to have the exact wording here
          if (audioPlayerComboBox.SelectedIndex == 1)
          {
            soundDeviceComboBox.Items.Add("Default DirectSound Device");
          }
          else
          {
            soundDeviceComboBox.Items.Add("Default Sound Device");
          }

          // Fill the combo box, starting at 1 to skip the "No Sound" device
          for (int i = 1; i < soundDevices.Length; i++)
          {
            soundDeviceComboBox.Items.Add(soundDevices[i].name);
          }

          break;

        case (int)AudioPlayer.Asio: 

          // Get all available ASIO devices and add them to the combo box
          BASS_ASIO_DEVICEINFO[] asioDevices = BassAsio.BASS_ASIO_GetDeviceInfos();
          if (asioDevices.Length == 0)
          {
            MessageBox.Show(this, "No ASIO Devices available in the system.",
                            "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            
            // Default back to BASS Player
            audioPlayerComboBox.SelectedIndex = 0;
          }
          else
          {
            foreach (BASS_ASIO_DEVICEINFO deviceInfo in asioDevices)
            {
              soundDeviceComboBox.Items.Add(deviceInfo.name);  
            }
          }

          break;

        case (int)AudioPlayer.WasApi:
          break;
      }
    }

    /// <summary>
    /// Show the ASIO Devices Control Panel
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btAsioDeviceSettings_Click(object sender, EventArgs e)
    {
      // Free ASIO and reinit it again
      BassAsio.BASS_ASIO_Free();
      if (BassAsio.BASS_ASIO_Init(soundDeviceComboBox.SelectedIndex, BASSASIOInit.BASS_ASIO_DEFAULT))
      {
        if (!BassAsio.BASS_ASIO_ControlPanel())
        {
          MessageBox.Show(this, "Selected ASIO device does not have a Control Panel",
                          "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
      else
      {
        MessageBox.Show(this, "Error initialising the selected ASIO device",
                        "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    /// <summary>
    /// Panning changed for ASIO Channel
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void hScrollBarBalance_ValueChanged(object sender, EventArgs e)
    {
      double balance = (double)hScrollBarBalance.Value / 100.0;
      lbBalance.Text = String.Format("{0}", balance);
    }

    private void hScrollBarCrossFade_ValueChanged(object sender, EventArgs e)
    {
      float xFadeSecs = 0;

      if (hScrollBarCrossFade.Value > 0)
      {
        xFadeSecs = (float)hScrollBarCrossFade.Value / 1000f;
      }

      CrossFadeSecondsLbl.Text = string.Format("{0:f2} Seconds", xFadeSecs);
    }

    private void hScrollBarBuffering_ValueChanged(object sender, EventArgs e)
    {
      float bufferingSecs = 0;

      if (hScrollBarBuffering.Value > 0)
      {
        bufferingSecs = (float)hScrollBarBuffering.Value / 1000f;
      }

      BufferingSecondsLbl.Text = string.Format("{0:f2} Seconds", bufferingSecs);
    }

    private void VisualizationsCmbBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      int selIndex = VisualizationsCmbBox.SelectedIndex;

      if (VisualizationsCmbBox.Items.Count <= 1 || selIndex < 0)
      {
        return;
      }

      if (IVizMgr == null)
      {
        return;
      }

      VizPluginInfo = (VisualizationInfo)VisualizationsCmbBox.SelectedItem;

      if (VizPluginInfo == null || VizPluginInfo.IsDummyPlugin)
      {
        return;
      }

      VizPresetsCmbBox.Items.Clear();

      if (VizPluginInfo.VisualizationType == VisualizationInfo.PluginType.Winamp)
      {
        groupBoxWinampVis.Enabled = true;
      }
      else
      {
        groupBoxWinampVis.Enabled = false;
      }

      if (VizPluginInfo.HasPresets)
      {
        VizPresetsCmbBox.Items.AddRange(VizPluginInfo.PresetNames.ToArray());
        //SuppressVisualizationRestart = true;
        VizPresetsCmbBox.SelectedIndex = VizPluginInfo.PresetIndex;
        VizPresetsCmbBox.Enabled = VizPresetsCmbBox.Items.Count > 1;
        //SuppressVisualizationRestart = false;
      }
      else
      {
        VizPresetsCmbBox.Enabled = false;
      }
    }

    private void VizPresetsCmbBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (VizPluginInfo == null)
      {
        return;
      }

      if (VizPresetsCmbBox.SelectedIndex == -1)
      {
        return;
      }

      int selIndex = VizPresetsCmbBox.SelectedIndex;

      if (selIndex < 0 || selIndex >= VizPluginInfo.PresetCount)
      {
        selIndex = 0;
      }

      VizPluginInfo.PresetIndex = selIndex;
    }

    private void VisualizationFpsNud_ValueChanged(object sender, EventArgs e)
    {
      if (IVizMgr != null)
      {
        IVizMgr.TargetFPS = (int)VisualizationFpsNud.Value;
      }
    }

    private void GaplessPlaybackChkBox_CheckedChanged(object sender, EventArgs e)
    {
      bool gaplessEnabled = GaplessPlaybackChkBox.Checked;
      CrossFadingLbl.Enabled = !gaplessEnabled;
      hScrollBarCrossFade.Enabled = !gaplessEnabled;
      CrossFadeSecondsLbl.Enabled = !gaplessEnabled;
    }

    private void EnableStatusOverlaysChkBox_CheckedChanged(object sender, EventArgs e)
    {
      ShowTrackInfoChkBox.Enabled = EnableStatusOverlaysChkBox.Checked;
    }

    private void InitializeVizEngine()
    {
      //System.Diagnostics.Debugger.Launch();
      Cursor.Current = Cursors.WaitCursor;

      BassAudioEngine bassEngine = BassMusicPlayer.Player;

      IVizMgr = bassEngine.IVizManager;
      List<VisualizationInfo> vizPluginsInfo = null;

      if (IVizMgr != null)
      {
        vizPluginsInfo = IVizMgr.VisualizationPluginsInfo;
      }

      LoadVisualizationList(vizPluginsInfo);
    }

    private void LoadVisualizationList(List<VisualizationInfo> vizPluginsInfo)
    {
      // If we're already populated the list we don't need to do it again so bail out
      if (VisualizationsInitialized)
      {
        return;
      }

      if (InvokeRequired)
      {
        LoadVisualizationListDelegate d = new LoadVisualizationListDelegate(LoadVisualizationList);
        Invoke(d, vizPluginsInfo);
        return;
      }

      VisualizationsCmbBox.Items.Clear();

      if (IVizMgr == null || vizPluginsInfo.Count == 0)
      {
        VisualizationsCmbBox.Items.Add(new VisualizationInfo("None", true));
        VisualizationsCmbBox.SelectedIndex = 0;
        return;
      }

      VisualizationsInitialized = true;
      int selectedIndex = -1;

      for (int i = 0; i < vizPluginsInfo.Count; i++)
      {
        VisualizationInfo pluginInfo = vizPluginsInfo[i];

        if (pluginInfo.IsIdenticalTo(VizPluginInfo, true))
        {
          selectedIndex = i;
        }

        VisualizationsCmbBox.Items.Add(pluginInfo);
      }

      if (selectedIndex == -1 && VisualizationsCmbBox.Items.Count > 0)
      {
        selectedIndex = 0;
      }
      VisualizationsCmbBox.SelectedIndex = selectedIndex;
    }

    private void btWinampConfig_Click(object sender, EventArgs e)
    {
      if (_visParam != null)
      {
        // Free first the previous winamp plugin
        BassVis.BASSVIS_Quit(_visParam);
      }
      _visParam = new BASSVIS_PARAM(BASSVISKind.BASSVISKIND_WINAMP);
      BassVis.BASSVIS_Init(BASSVISKind.BASSVISKIND_WINAMP, MediaPortal.GUI.Library.GUIGraphicsContext.form.Handle);
      int tmpVis = BassVis.BASSVIS_GetPluginHandle(BASSVISKind.BASSVISKIND_WINAMP, VizPluginInfo.FilePath);
      if (tmpVis != 0)
      {
        int numModules = BassVis.BASSVIS_GetModulePresetCount(_visParam, VizPluginInfo.FilePath);
        BassVis.BASSVIS_Config(_visParam, 0);
      }
    }

    #endregion
  }
}