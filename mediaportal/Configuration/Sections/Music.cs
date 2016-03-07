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
using System.IO;
using System.Windows.Forms;
using MediaPortal.MusicPlayer.BASS;
using MediaPortal.Player;
using MediaPortal.Profile;
using Un4seen.Bass;
using Un4seen.BassAsio;
using Un4seen.BassWasapi;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class Music : SectionSettings
  {
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

    private string[] PlayerOptions = new string[]
                                       {
                                         "BASS engine",
                                         "ASIO",
                                         "WASAPI",
                                         "Internal dshow player",
                                       };

    private string[] MonoUpmix = new string[] {"None", "Stereo", "QuadraphonicPhonic", "5.1 Surround", "7.1 Surround"};
    private string[] StereoUpmix = new string[] {"None", "QuadraphonicPhonic", "5.1 Surround", "7.1 Surround"};
    private string[] QuadroPhonicUpmix = new string[] {"None", "5.1 Surround", "7.1 Surround"};
    private string[] FiveDotOneUpmix = new string[] {"None", "7.1 Surround"};

    private const string VUMeterValue0 = "none";
    private const string VUMeterValue1 = "analog";
    private const string VUMeterValue2 = "led";

    private string _soundDevice = null;
    private string _soundDeviceID = "";

    private bool _initialising = true;

    #endregion

    #region ctor

    public Music()
      : this("Music")
    {
    }

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

      // Fill the Upmix Combos
      foreach (string str in MonoUpmix)
      {
        cbUpmixMono.Items.Add(str);
      }

      foreach (string str in StereoUpmix)
      {
        cbUpmixStereo.Items.Add(str);
      }

      foreach (string str in QuadroPhonicUpmix)
      {
        cbUpmixQuadro.Items.Add(str);
      }

      foreach (string str in FiveDotOneUpmix)
      {
        cbUpmixFiveDotOne.Items.Add(str);
      }
    }

    #endregion

    #region Activation Init

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      _initialising = true;
      trackBarBuffering_Scroll(null, null);
      trackBarCrossfade_Scroll(null, null);
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
        _soundDeviceID = xmlreader.GetValueAsString("audioplayer", "sounddeviceid", "");

        string strAudioPlayer = xmlreader.GetValueAsString("audioplayer", "playerId", "0");
        int audioPlayer = (int) AudioPlayer.Bass; // Default to BASS Player
        try
        {
          audioPlayer = Convert.ToInt16(strAudioPlayer);
        }
        catch (Exception) // We end up here in the conversion Phase, where we have still a string ioncluded
        {
        }

        audioPlayerComboBox.SelectedIndex = audioPlayer;

        #region General Bass Player Settings

        // Remove the Event Handler, so that the settings for Crossfading a preserved
        GaplessPlaybackChkBox.CheckedChanged -= GaplessPlaybackChkBox_CheckedChanged;

        int crossFadeMS = xmlreader.GetValueAsInt("audioplayer", "crossfade", 4000);

        if (crossFadeMS < 0)
        {
          crossFadeMS = 4000;
        }
        else if (crossFadeMS > trackBarCrossfade.Maximum)
        {
          crossFadeMS = trackBarCrossfade.Maximum;
        }

        trackBarCrossfade.Value = crossFadeMS;

        int bufferingMS = xmlreader.GetValueAsInt("audioplayer", "buffering", 500);

        if (bufferingMS < trackBarBuffering.Minimum)
        {
          bufferingMS = trackBarBuffering.Minimum;
        }
        else if (bufferingMS > trackBarBuffering.Maximum)
        {
          bufferingMS = trackBarBuffering.Maximum;
        }

        trackBarBuffering.Value = bufferingMS;

        EnableReplayGainChkBox.Checked = xmlreader.GetValueAsBool("audioplayer", "enableReplayGain", false);
        EnableAlbumReplayGainChkBox.Checked = xmlreader.GetValueAsBool("audioplayer", "enableAlbumReplayGain", false);
        GaplessPlaybackChkBox.Checked = xmlreader.GetValueAsBool("audioplayer", "gaplessPlayback", false);
        UseSkipStepsCheckBox.Checked = xmlreader.GetValueAsBool("audioplayer", "useSkipSteps", false);
        FadeOnStartStopChkbox.Checked = xmlreader.GetValueAsBool("audioplayer", "fadeOnStartStop", true);
        StreamOutputLevelNud.Value = (decimal) xmlreader.GetValueAsInt("audioplayer", "streamOutputLevel", 100);

        cbUpmixMono.SelectedIndex = xmlreader.GetValueAsInt("audioplayer", "upMixMono", 0);
        cbUpmixStereo.SelectedIndex = xmlreader.GetValueAsInt("audioplayer", "upMixStereo", 0);
        cbUpmixQuadro.SelectedIndex = xmlreader.GetValueAsInt("audioplayer", "upMixQuadro", 0);
        cbUpmixFiveDotOne.SelectedIndex = xmlreader.GetValueAsInt("audioplayer", "upMixFiveDotOne", 0);

        chkEnableResumeSupport.Checked = xmlreader.GetValueAsBool("audioplayer", "enableResume", false);
        tbResumeAfter.Text = xmlreader.GetValueAsString("audioplayer", "resumeAfter", "0");
        cbResumeSelect.Text = xmlreader.GetValueAsString("audioplayer", "resumeSelect", "");
        tbResumeSearchValue.Text = xmlreader.GetValueAsString("audioplayer", "resumeSearch", "");

        // Re-add the previously removed Event Handler
        GaplessPlaybackChkBox.CheckedChanged += GaplessPlaybackChkBox_CheckedChanged;

        #endregion

        #region BASS ASIO

        hScrollBarBalance.Value = xmlreader.GetValueAsInt("audioplayer", "asiobalance", 0);


        #endregion

        #region BASS WASAPI

        WasapiExclusiveModeCkBox.Checked = xmlreader.GetValueAsBool("audioplayer", "wasapiExclusive", true);
        WasApiSpeakersCombo.SelectedIndex = xmlreader.GetValueAsInt("audioplayer", "wasApiSpeakers", 1);

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
            catch (Exception)
            {
          }
        }
        }

        repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "repeat", false);
        autoShuffleCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", false);

        SavePlaylistOnExitChkBox.Checked = xmlreader.GetValueAsBool("musicfiles", "savePlaylistOnExit", true);
        ResumePlaylistChkBox.Checked = xmlreader.GetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", true);
        PlaylistCurrentCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "playlistIsCurrent", true);
        PlayListUTF8CheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "savePlaylistUTF8", false);

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

        chkDisableSimilarTrackLookups.Checked = !(xmlreader.GetValueAsBool("musicmisc", "lookupSimilarTracks", true));

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

        xmlwriter.SetValue("audioplayer", "playerId", audioPlayerComboBox.SelectedIndex);
        xmlwriter.SetValue("audioplayer", "sounddevice", (soundDeviceComboBox.SelectedItem as SoundDeviceItem).Name);
        xmlwriter.SetValue("audioplayer", "sounddeviceid", (soundDeviceComboBox.SelectedItem as SoundDeviceItem).ID);

        xmlwriter.SetValue("audioplayer", "crossfade", trackBarCrossfade.Value);
        xmlwriter.SetValue("audioplayer", "buffering", trackBarBuffering.Value);
        xmlwriter.SetValueAsBool("audioplayer", "useSkipSteps", UseSkipStepsCheckBox.Checked);
        xmlwriter.SetValueAsBool("audioplayer", "fadeOnStartStop", FadeOnStartStopChkbox.Checked);
        xmlwriter.SetValueAsBool("audioplayer", "gaplessPlayback", GaplessPlaybackChkBox.Checked);
        xmlwriter.SetValueAsBool("audioplayer", "enableReplayGain", EnableReplayGainChkBox.Checked);
        xmlwriter.SetValueAsBool("audioplayer", "enableAlbumReplayGain", EnableAlbumReplayGainChkBox.Checked);
        xmlwriter.SetValue("audioplayer", "streamOutputLevel", StreamOutputLevelNud.Value);

        xmlwriter.SetValue("audioplayer", "asiobalance", hScrollBarBalance.Value);

        xmlwriter.SetValueAsBool("audioplayer", "wasapiExclusive", WasapiExclusiveModeCkBox.Checked);
        xmlwriter.SetValue("audioplayer", "wasApiSpeakers", WasApiSpeakersCombo.SelectedIndex);

        xmlwriter.SetValue("audioplayer", "upMixMono", cbUpmixMono.SelectedIndex);
        xmlwriter.SetValue("audioplayer", "upMixStereo", cbUpmixStereo.SelectedIndex);
        xmlwriter.SetValue("audioplayer", "upMixQuadro", cbUpmixQuadro.SelectedIndex);
        xmlwriter.SetValue("audioplayer", "upMixFiveDotOne", cbUpmixFiveDotOne.SelectedIndex);

        xmlwriter.SetValueAsBool("audioplayer", "enableResume", chkEnableResumeSupport.Checked);
        xmlwriter.SetValue("audioplayer", "resumeAfter", tbResumeAfter.Text);
        xmlwriter.SetValue("audioplayer", "resumeSelect", cbResumeSelect.Text);
        xmlwriter.SetValue("audioplayer", "resumeSearch", tbResumeSearchValue.Text);

        #endregion

        #region Playlist Settings

        xmlwriter.SetValue("music", "playlists", playlistFolderTextBox.Text);
        xmlwriter.SetValueAsBool("musicfiles", "repeat", repeatPlaylistCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "autoshuffle", autoShuffleCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "savePlaylistOnExit", SavePlaylistOnExitChkBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", ResumePlaylistChkBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "playlistIsCurrent", PlaylistCurrentCheckBox.Checked);
        xmlwriter.SetValueAsBool("musicfiles", "savePlaylistUTF8", PlayListUTF8CheckBox.Checked);

        //Play behaviour
        xmlwriter.SetValue("musicfiles", "selectOption", cmbSelectOption.Text.ToLowerInvariant());
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
        xmlwriter.SetValueAsBool("musicmisc", "lookupSimilarTracks", !chkDisableSimilarTrackLookups.Checked);

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
      }

    #endregion

    #region Public Methods

    public override object GetSetting(string name)
    {
      switch (name.ToLowerInvariant())
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
    /// A new Audio Device has been selected.
    /// We need to get the Sound devices supported by this device
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void audioPlayerComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      // Check if we have selected either BASS; ASIO or WASAPI and only enable Playersettings and Vis then
      bool useBassEngine = audioPlayerComboBox.SelectedIndex < 3;
      tabControlPlayerSettings.Enabled = useBassEngine;
      tabPagePlayerUpmixSettings.Enabled = useBassEngine;

      switch (audioPlayerComboBox.SelectedIndex)
      {
        case 0:
          tabPageBassPlayerSettings.Enabled = true;
          tabPageASIOPlayerSettings.Enabled = false;
          tabPageWASAPIPLayerSettings.Enabled = false;
          tabPagePlayerUpmixSettings.Enabled = true;
          tabControlPlayerSettings.SelectedTab = tabPageBassPlayerSettings;
          break;

        case 1:
          tabPageBassPlayerSettings.Enabled = true;
          tabPageASIOPlayerSettings.Enabled = true;
          tabPageWASAPIPLayerSettings.Enabled = false;
          tabPagePlayerUpmixSettings.Enabled = true;
          tabControlPlayerSettings.SelectedTab = tabPageBassPlayerSettings;
          break;

        case 2:
          tabPageBassPlayerSettings.Enabled = true;
          tabPageASIOPlayerSettings.Enabled = false;
          tabPageWASAPIPLayerSettings.Enabled = true;
          tabPagePlayerUpmixSettings.Enabled = true;
          tabControlPlayerSettings.SelectedTab = tabPageBassPlayerSettings;
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
          bool found = false;
          foreach (SoundDeviceItem item in soundDeviceComboBox.Items)
          {
            if (item.Name == _soundDevice && item.ID == _soundDeviceID)
            {
              soundDeviceComboBox.SelectedItem = item;
              found = true;
              break;
            }
          }

          if (!found)
          {
            soundDeviceComboBox.SelectedIndex = 0;
          }

        }

        // Change the Text of Item 0 in the Sound device box for Bass Player or DirectShow Player
        if (audioPlayerComboBox.SelectedIndex == 0)
        {
          soundDeviceComboBox.Items[0] = new SoundDeviceItem("Default Sound Device", "");
        }
        else if (audioPlayerComboBox.SelectedIndex == 3)
        {
          soundDeviceComboBox.Items[0] = new SoundDeviceItem("Default DirectSound Device", "");
        }
      }
    }

    /// <summary>
    /// Get the sound devices for the selected Player
    /// </summary>
    /// <param name="player">
    /// 0 - Bass Directshow
    /// 1 - ASIO
    /// 2 - WASAPI
    /// 3 - Internal Dshow Player
    /// </param>
    private void GetAvailableSoundDevices(int player)
    {
      switch (player)
      {
        case (int) AudioPlayer.Bass:
        case (int) AudioPlayer.DShow:

          // Get all available devices and add them to the combo box
          BASS_DEVICEINFO[] soundDevices = Bass.BASS_GetDeviceInfos();

          // For Directshow player, we need to have the exact wording here
          if (audioPlayerComboBox.SelectedIndex == 1)
          {
            soundDeviceComboBox.Items.Add(new SoundDeviceItem("Default DirectSound Device", ""));
          }
          else
          {
            soundDeviceComboBox.Items.Add(new SoundDeviceItem("Default Sound Device", ""));
          }

          // Fill the combo box, starting at 1 to skip the "No Sound" device
          for (int i = 1; i < soundDevices.Length; i++)
          {
            soundDeviceComboBox.Items.Add(new SoundDeviceItem(soundDevices[i].name, soundDevices[i].id));
          }

          break;

        case (int) AudioPlayer.Asio:

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
              soundDeviceComboBox.Items.Add(new SoundDeviceItem(deviceInfo.name, deviceInfo.driver));
            }
          }

          break;

        case (int) AudioPlayer.WasApi:
          // Get all available ASIO devices and add them to the combo box
          BASS_WASAPI_DEVICEINFO[] wasapiDevices = BassWasapi.BASS_WASAPI_GetDeviceInfos();
          if (wasapiDevices.Length == 0)
          {
            MessageBox.Show(this, "No WASAPI Devices available in the system.",
                            "MediaPortal - Setup", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            // Default back to BASS Player
            audioPlayerComboBox.SelectedIndex = 0;
          }
          else
          {
            foreach (BASS_WASAPI_DEVICEINFO deviceInfo in wasapiDevices)
            {
              // Only add enabled and output devices to the list
              if (deviceInfo.IsEnabled && !deviceInfo.IsInput)
              {
                soundDeviceComboBox.Items.Add(new SoundDeviceItem(deviceInfo.name, deviceInfo.id));
              }
            }
          }
          if (soundDeviceComboBox.Items.Count == 0)
          {
            // Add default sound device to avoid crash.
            soundDeviceComboBox.Items.Add(new SoundDeviceItem("Default Sound Device", ""));
            soundDeviceComboBox.Items[0] = new SoundDeviceItem("Default Sound Device", "");
          }
          break;
      }
    }

    /// <summary>
    /// Get some information for the selected sound device
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void soundDeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      int sounddevice = -1;
      BASS_DEVICEINFO[] soundDeviceDescriptions = Bass.BASS_GetDeviceInfos();
      for (int i = 0; i < soundDeviceDescriptions.Length; i++)
      {
        if (soundDeviceDescriptions[i].name == soundDeviceComboBox.Text)
        {
          sounddevice = i;
          break;
        }
      }

      // Run the following code in a Thread to avoid delays, when entering the Music screen
      new System.Threading.Thread(() =>
                      {
                        // Find out the minimum Buffer length possible
                        Bass.BASS_Free();
                        if (Bass.BASS_Init(sounddevice, 48000, BASSInit.BASS_DEVICE_LATENCY, IntPtr.Zero, Guid.Empty))
                        {
                          BASS_INFO info = Bass.BASS_GetInfo();
                          if (info != null)
                          {
                            int currentBuffer = trackBarBuffering.Value;
                            if (currentBuffer < info.minbuf)
                            {
                              trackBarBuffering.Value = info.minbuf;
                            }
                            trackBarBuffering.Minimum = info.minbuf;
                          }
                        }

                        // Detect WASAPI Speaker Setup
                        if (audioPlayerComboBox.SelectedIndex == 2)
                        {
                          Bass.BASS_Free();
                          Bass.BASS_Init(0, 48000, 0, IntPtr.Zero, Guid.Empty); // No sound device
                          BASS_WASAPI_DEVICEINFO[] wasapiDevices = BassWasapi.BASS_WASAPI_GetDeviceInfos();

                          int i = 0;
                          // Check if the WASAPI device read is amongst the one retrieved
                          for (i = 0; i < wasapiDevices.Length; i++)
                          {
                            if (wasapiDevices[i].name == soundDeviceComboBox.Text)
                            {
                              sounddevice = i;
                              break;
                            }
                          }

                          int channels = 0;

                          // Let's assume a maximum of 8 speakers attached to the device
                          for (int c = 1; c < 9; c++)
                          {
                            BASSWASAPIFormat format = BassWasapi.BASS_WASAPI_CheckFormat(sounddevice, 44100, c,
                                                                                         BASSWASAPIInit.
                                                                                           BASS_WASAPI_SHARED);

                            if (format != BASSWASAPIFormat.BASS_WASAPI_FORMAT_UNKNOWN)
                            {
                              channels = c;
                            }
                          }
                          if (channels > WasApiSpeakersCombo.SelectedIndex + 1)
                          {
                            switch (channels)
                            {
                              case 1:
                                WasApiSpeakersCombo.SelectedIndex = 0;
                                break;

                              case 2:
                                WasApiSpeakersCombo.SelectedIndex = 1;
                                break;

                              case 4:
                                WasApiSpeakersCombo.SelectedIndex = 2;
                                break;

                              case 6:
                                WasApiSpeakersCombo.SelectedIndex = 3;
                                break;

                              case 8:
                                WasApiSpeakersCombo.SelectedIndex = 4;
                                break;
                            }
                          }
                        }
                        Bass.BASS_Free();
                      }
        ).Start();
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
      double balance = (double) hScrollBarBalance.Value/100.0;
      lbBalance.Text = String.Format("{0}", balance);
    }

    private void trackBarCrossfade_Scroll(object sender, EventArgs e)
    {
      float xFadeSecs = (float) trackBarCrossfade.Value/1000f;
      CrossFadeSecondsLbl.Text = string.Format("{0:f2} Seconds", xFadeSecs);
    }

    private void trackBarBuffering_Scroll(object sender, EventArgs e)
    {
      float bufferingSecs = (float) trackBarBuffering.Value/1000f;
      BufferingSecondsLbl.Text = string.Format("{0:f2} Seconds", bufferingSecs);
    }

    private void GaplessPlaybackChkBox_CheckedChanged(object sender, EventArgs e)
    {
      bool gaplessEnabled = GaplessPlaybackChkBox.Checked;
      CrossFadingLbl.Enabled = !gaplessEnabled;
      trackBarCrossfade.Enabled = !gaplessEnabled;
      CrossFadeSecondsLbl.Enabled = !gaplessEnabled;

      if (_initialising)
      {
        _initialising = false;
        return;
      }

      if (!gaplessEnabled)
      {
        trackBarCrossfade.Value = 4000;  // Set 4 seconds as default for fading
        trackBarCrossfade_Scroll(trackBarCrossfade, new EventArgs());
        FadeOnStartStopChkbox.Checked = true;
      }
      else
      {
        trackBarCrossfade.Value = 0;
        trackBarCrossfade_Scroll(trackBarCrossfade, new EventArgs());
        FadeOnStartStopChkbox.Checked = false;
      }
    }

    #endregion

  /// <summary>
  /// Class used to display the Sound Device Information in the Combo Box 
  /// </summary>
  public class SoundDeviceItem
  {
    public string Name;
    public string ID;

    public SoundDeviceItem(string name, string id)
    {
      Name = name;
      ID = id;
    }

    public override string ToString()
    {
      // Generates the text shown in the combo box
      return Name;
    }
  }
  }
}