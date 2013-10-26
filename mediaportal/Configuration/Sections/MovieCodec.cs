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
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using Microsoft.Win32;

namespace MediaPortal.Configuration.Sections
{
  public partial class MovieCodec : SectionSettings
  {
    private bool _init = false;
    private bool settingLAVSlitter;

    /// <summary>
    /// 
    /// </summary>
    public MovieCodec()
      : this("Video Codecs") {}

    /// <summary>
    /// 
    /// </summary>
    public MovieCodec(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        // Fetch available audio and video renderers
        ArrayList availableAudioRenderers = FilterHelper.GetAudioRenderers();
        // Populate video and audio codecs
        ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
        ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
        ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
        ArrayList availableVC1VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.VC1);
        ArrayList availableVC1IVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.VC1);
        ArrayList availableXVIDVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.XVID);
        ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.AAC);
        ArrayList availableFileSyncFilters = FilterHelper.GetFilters(MediaType.Stream, MediaSubType.Null);
        ArrayList availableVC1CyberlinkVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.CyberlinkVC1);
        ArrayList availableVC1ICyberlinkVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.CyberlinkVC1);
        ArrayList availableSourcesFilters = FilterHelper.GetFilterSource();
        //Remove Cyberlink Muxer from the list to avoid newbie user confusion.
        while (availableVideoFilters.Contains("CyberLink MPEG Muxer"))
        {
          availableVideoFilters.Remove("CyberLink MPEG Muxer");
        }
        while (availableVideoFilters.Contains("Ulead MPEG Muxer"))
        {
          availableVideoFilters.Remove("Ulead MPEG Muxer");
        }
        while (availableVideoFilters.Contains("PDR MPEG Muxer"))
        {
          availableVideoFilters.Remove("PDR MPEG Muxer");
        }
        while (availableVideoFilters.Contains("Nero Mpeg2 Encoder"))
        {
          availableVideoFilters.Remove("Nero Mpeg2 Encoder");
        }
        availableVideoFilters.Sort();
        videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
        while (availableAudioFilters.Contains("CyberLink MPEG Muxer"))
        {
          availableAudioFilters.Remove("CyberLink MPEG Muxer");
        }
        while (availableAudioFilters.Contains("Ulead MPEG Muxer"))
        {
          availableAudioFilters.Remove("Ulead MPEG Muxer");
        }
        while (availableAudioFilters.Contains("PDR MPEG Muxer"))
        {
          availableAudioFilters.Remove("PDR MPEG Muxer");
        }
        while (availableAudioFilters.Contains("Nero Mpeg2 Encoder"))
        {
          availableAudioFilters.Remove("Nero Mpeg2 Encoder");
        }
        availableAudioFilters.Sort();
        audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());

        while (availableXVIDVideoFilters.Contains("ArcSoft Video Decoder"))
        {
          availableXVIDVideoFilters.Remove("ArcSoft Video Decoder");
          break;
        }
        /*while (availableVC1VideoFilters.Contains("WMVideo Decoder DMO"))
        {
          availableVC1VideoFilters.Remove("WMVideo Decoder DMO");
        }*/
        availableVC1VideoFilters.Sort();
        while (availableVC1IVideoFilters.Contains("MPC - Video decoder"))
        {
          availableVC1IVideoFilters.Remove("MPC - Video decoder");
          break;
        }
        while (availableVC1IVideoFilters.Contains("WMVideo Decoder DMO"))
        {
          availableVC1IVideoFilters.Remove("WMVideo Decoder DMO");
          break;
        }
        while (availableFileSyncFilters.Contains("Haali Media Splitter (AR)"))
        {
          availableSourcesFilters.Add("Haali Media Splitter");
          break;
        }

        availableVC1IVideoFilters.Sort();
        availableFileSyncFilters.Sort();
        availableSourcesFilters.Sort();
        SplitterComboBox.Items.AddRange(availableSourcesFilters.ToArray());
        SplitterFileComboBox.Items.AddRange(availableFileSyncFilters.ToArray());
        vc1videoCodecComboBox.Items.AddRange(availableVC1VideoFilters.ToArray());
        vc1ivideoCodecComboBox.Items.AddRange(availableVC1IVideoFilters.ToArray());
        vc1videoCodecComboBox.Items.AddRange(availableVC1CyberlinkVideoFilters.ToArray());
        vc1ivideoCodecComboBox.Items.AddRange(availableVC1ICyberlinkVideoFilters.ToArray());
        h264videoCodecComboBox.Items.AddRange(availableH264VideoFilters.ToArray());
        xvidvideoCodecComboBox.Items.AddRange(availableXVIDVideoFilters.ToArray());
        audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
        aacAudioCodecComboBox.Items.AddRange(availableAACAudioFilters.ToArray());
        _init = true;
        LoadSettings();
      }
      // Do always
      autoDecoderSettings.Visible = SettingsForm.AdvancedMode;
      ForceSourceSplitter.Visible = true;
    }

    /// <summary>
    /// sets useability of select config depending on whether auot decoder stting option is enabled.
    /// </summary>
    public void UpdateDecoderSettings()
    {
      label5.Enabled = !autoDecoderSettings.Checked;
      label6.Enabled = !autoDecoderSettings.Checked;
      mpLabel1.Enabled = !autoDecoderSettings.Checked;
      mpLabel2.Enabled = !autoDecoderSettings.Checked;
      videoCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      h264videoCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      vc1ivideoCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      vc1videoCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      xvidvideoCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      audioCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      aacAudioCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      SplitterComboBox.Enabled = !autoDecoderSettings.Checked && ForceSourceSplitter.Checked;
      SplitterFileComboBox.Enabled = !autoDecoderSettings.Checked && ForceSourceSplitter.Checked;
      if (autoDecoderSettings.Checked)
      {
        ForceSourceSplitter.Enabled = false;
        ForceSourceSplitter.Checked = false;
      }
      else if (!ForceSourceSplitter.Checked)
      {
        ForceSourceSplitter.Enabled = true;
      }
    }

    /// <summary>
    /// Loads the movie player settings
    /// </summary>
    public override void LoadSettings()
    {
      if (!_init)
      {
        return;
      }
      using (Settings xmlreader = new MPSettings())
      {
        autoDecoderSettings.Checked = xmlreader.GetValueAsBool("movieplayer", "autodecodersettings", false);
        ForceSourceSplitter.Checked = xmlreader.GetValueAsBool("movieplayer", "forcesourcesplitter", false);
        mpCheckBoxTS.Checked = xmlreader.GetValueAsBool("movieplayer", "usemoviecodects", false);
        UpdateDecoderSettings();
        audioRendererComboBox.SelectedItem = xmlreader.GetValueAsString("movieplayer", "audiorenderer",
                                                                        "Default DirectSound Device");
        // Set Source Splitter check for first init to true.
        string CheckSourceSplitter = xmlreader.GetValueAsString("movieplayer", "forcesourcesplitter", "");

        // Set codecs
        string videoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
        string h264videoCodec = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
        string vc1ivideoCodec = xmlreader.GetValueAsString("movieplayer", "vc1ivideocodec", "");
        string vc1videoCodec = xmlreader.GetValueAsString("movieplayer", "vc1videocodec", "");
        string xvidvideoCodec = xmlreader.GetValueAsString("movieplayer", "xvidvideocodec", "");
        string audioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
        string aacaudioCodec = xmlreader.GetValueAsString("movieplayer", "aacaudiocodec", "");
        string splitterFilter = xmlreader.GetValueAsString("movieplayer", "splitterfilter", "");
        string splitterFileFilter = xmlreader.GetValueAsString("movieplayer", "splitterfilefilter", "");
        settingLAVSlitter = xmlreader.GetValueAsBool("movieplayer", "settinglavplitter", false);

        if (videoCodec == string.Empty)
        {
          ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
          videoCodec = SetCodecBox(availableVideoFilters, "LAV Video Decoder", "DScaler Mpeg2 Video Decoder", "");
        }
        if (h264videoCodec == string.Empty)
        {
          ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
          h264videoCodec = SetCodecBox(availableH264VideoFilters, "LAV Video Decoder", "CoreAVC Video Decoder", "");
        }
        if (vc1videoCodec == string.Empty)
        {
          ArrayList availableVC1VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.VC1);
          vc1videoCodec = SetCodecBox(availableVC1VideoFilters, "LAV Video Decoder", "", "");
        }
        if (vc1ivideoCodec == string.Empty)
        {
          ArrayList availableVC1VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.VC1);
          vc1ivideoCodec = SetCodecBox(availableVC1VideoFilters, "LAV Video Decoder", "", "");
        }
        if (xvidvideoCodec == string.Empty)
        {
          ArrayList availableXVIDVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.XVID);
          xvidvideoCodec = SetCodecBox(availableXVIDVideoFilters, "LAV Video Decoder", "DivX Decoder Filter", "");
        }
        if (audioCodec == string.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
          audioCodec = SetCodecBox(availableAudioFilters, "LAV Audio Decoder", "DScaler Audio Decoder", "ffdshow Audio Decoder");
        }
        if (aacaudioCodec == string.Empty)
        {
          ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.AAC);
          aacaudioCodec = SetCodecBox(availableAACAudioFilters, "LAV Audio Decoder", "MONOGRAM ACC Decoder", "ffdshow Audio Decoder");
        }
        if (splitterFilter == string.Empty)
        {
          ArrayList availableSourcesFilters = FilterHelper.GetFilterSource();
          splitterFilter = SetCodecBox(availableSourcesFilters, "LAV Splitter Source", "", "");
        }
        if (splitterFileFilter == string.Empty)
        {
          ArrayList availableFileSyncFilters = FilterHelper.GetFilters(MediaType.Stream, MediaSubType.Null);
          splitterFileFilter = SetCodecBox(availableFileSyncFilters, "LAV Splitter", "", "");
        }

        if (CheckSourceSplitter == string.Empty && (splitterFilter == "LAV Splitter Source" || splitterFileFilter == "LAV Splitter"))
        {
          ForceSourceSplitter.Checked = true;
        }

        // Enable WMV WMA codec for LAV suite (setting will be change only on first run and if lav is set as default splitter)
        if (!settingLAVSlitter && (splitterFilter == "LAV Splitter Source" || splitterFileFilter == "LAV Splitter"))
        {
          EnableWmvWmaLAVSettings(@"Software\\LAV\\Splitter\\Formats", "asf");
          EnableWmvWmaLAVSettings(@"Software\\LAV\\Audio\\Formats", "wma");
          EnableWmvWmaLAVSettings(@"Software\\LAV\\Audio\\Formats", "wmalossless");
          settingLAVSlitter = true;
        }

        audioCodecComboBox.Text = audioCodec;
        videoCodecComboBox.Text = videoCodec;
        h264videoCodecComboBox.Text = h264videoCodec;
        vc1ivideoCodecComboBox.Text = vc1ivideoCodec;
        vc1videoCodecComboBox.Text = vc1videoCodec;
        xvidvideoCodecComboBox.Text = xvidvideoCodec;
        aacAudioCodecComboBox.Text = aacaudioCodec;
        SplitterComboBox.Text = splitterFilter;
        SplitterFileComboBox.Text = splitterFileFilter;
        CheckBoxValid(audioCodecComboBox);
        CheckBoxValid(videoCodecComboBox);
        CheckBoxValid(h264videoCodecComboBox);
        CheckBoxValid(vc1videoCodecComboBox);
        CheckBoxValid(vc1ivideoCodecComboBox);
        CheckBoxValid(xvidvideoCodecComboBox);
        CheckBoxValid(aacAudioCodecComboBox);
        CheckBoxValid(audioRendererComboBox);
        CheckBoxValid(SplitterComboBox);
        CheckBoxValid(SplitterFileComboBox);
      }
    }

    private string SetCodecBox(ArrayList availableFilters, String FilterCodec1, String FilterCodec2, String FilterCodec3)
    {
      bool filterCodec1 = false;
      bool filterCodec2 = false;
      bool filterCodec3 = false;
      string Codec = "";

      if (availableFilters.Count > 0)
      {
        Codec = (string)availableFilters[0];
        foreach (string filter in availableFilters)
        {
          if (filter.Equals(FilterCodec1))
          {
            filterCodec1 = true;
          }
          else if (filter.Equals(FilterCodec2))
          {
            filterCodec2 = true;
          }
          else if (filter.Equals(FilterCodec3))
          {
            filterCodec3 = true;
          }
        }
        if (filterCodec1)
        {
          return FilterCodec1;
        }
        else if (filterCodec2)
        {
          return FilterCodec2;
        }
        else if (filterCodec3)
        {
          return FilterCodec3;
        }
      }
      return Codec;
    }

    /// <summary>
    /// Check Combobox count
    /// </summary>
    public override void CheckBoxValid(MPComboBox ComboBox)
    {
      if (ComboBox.Items.Count == 1)
      {
        ComboBox.Enabled = false;
      }
    }

    /// <summary>
    /// Saves movie player settings and codec info.
    /// </summary>
    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("movieplayer", "autodecodersettings", autoDecoderSettings.Checked);
        xmlwriter.SetValueAsBool("movieplayer", "forcesourcesplitter", ForceSourceSplitter.Checked);
        xmlwriter.SetValueAsBool("movieplayer", "usemoviecodects", mpCheckBoxTS.Checked);
        xmlwriter.SetValue("movieplayer", "audiorenderer", audioRendererComboBox.Text);
        // Set codecs
        xmlwriter.SetValue("movieplayer", "mpeg2audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "mpeg2videocodec", videoCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "h264videocodec", h264videoCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "vc1ivideocodec", vc1ivideoCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "vc1videocodec", vc1videoCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "xvidvideocodec", xvidvideoCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "aacaudiocodec", aacAudioCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "splitterfilter", SplitterComboBox.Text);
        xmlwriter.SetValue("movieplayer", "splitterfilefilter", SplitterFileComboBox.Text);
        xmlwriter.SetValueAsBool("movieplayer", "settinglavplitter", settingLAVSlitter);
      }
    }

    /// <summary>
    /// updates the useable options if the auto decoder option is enabled.
    /// </summary>
    private void autoDecoderSettings_CheckedChanged(object sender, EventArgs e)
    {
      UpdateDecoderSettings();
      Startup._automaticMovieCodec = autoDecoderSettings.Checked;
      if (!ForceSourceSplitter.Checked)
      {
        ForceSourceSplitter.Checked = true;
      }
    }

    /// <summary>
    /// updates the useable options if the force source splitter option is enabled.
    /// </summary>
    private void ForceSourceSplitter_CheckedChanged(object sender, EventArgs e)
    {
      UpdateDecoderSettings();
      Startup._automaticMovieFilter = ForceSourceSplitter.Checked;
      if ((SplitterComboBox.Text == "File Source (Async.)" || SplitterComboBox.Text == "File Source (URL)") && ForceSourceSplitter.Checked)
      {
        SplitterFileComboBox.Enabled = true;
      }
      else
      {
        SplitterFileComboBox.Enabled = false;
      }
    }

    private void RegMPtoConfig(string subkeysource)
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(subkeysource))
      {
        if (subkey != null)
        {
          RegistryUtilities.RenameSubKey(subkey, @"MediaPortal",
                                         @"Configuration");
        }
      }
    }

    private void RegConfigtoMP(string subkeysource)
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(subkeysource))
      {
        if (subkey != null)
        {
          RegistryUtilities.RenameSubKey(subkey, @"Configuration",
                                         @"MediaPortal");
        }
      }
    }

    private bool EnableWmvWmaLAVSettings(string subkeysource, string valueKey)
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(subkeysource))
      {
        if (subkey != null)
        {
          try
          {
            subkey.SetValue(valueKey, unchecked((int)0x000000001),
                            RegistryValueKind.DWord);
            return true;
          }
          catch (Exception)
          {
            return false;
          }
        }
      }
      return false;
    }

    private void ConfigCodecSection(object sender, EventArgs e, string selection)
    {
      foreach (DsDevice device in DsDevice.GetDevicesOfCat(DirectShowLib.FilterCategory.LegacyAmFilterCategory))
      {
        try
        {
          if (device.Name != null)
          {
            {
              if (selection.Equals(device.Name))
              {
                if (selection.Contains("CyberLink"))
                {
                  // Rename MediaPortal subkey to Configuration for Cyberlink take setting
                  RegMPtoConfig(@"Software\Cyberlink\Common\clcvd");
                  RegMPtoConfig(@"Software\Cyberlink\Common\cl264dec");
                  RegMPtoConfig(@"Software\Cyberlink\Common\CLVSD");
                  RegMPtoConfig(@"Software\Cyberlink\Common\CLAud");

                  // Show Codec page Setting
                  DirectShowPropertyPage page = new DirectShowPropertyPage((DsDevice) device);
                  page.Show(this);

                  // Rename Configuration subkey to MediaPortal to apply Cyberlink setting
                  RegConfigtoMP(@"Software\Cyberlink\Common\clcvd");
                  RegConfigtoMP(@"Software\Cyberlink\Common\cl264dec");
                  RegConfigtoMP(@"Software\Cyberlink\Common\CLVSD");
                  RegConfigtoMP(@"Software\Cyberlink\Common\CLAud");
                }
                else
                {
                  DirectShowPropertyPage page = new DirectShowPropertyPage((DsDevice) device);
                  page.Show(this);
                }
              }
            }
          }
        }
        catch (Exception)
        {
        }
      }
    }

    private void ConfigAudioRendererCodecSection(object sender, EventArgs e, string selection)
    {
      foreach (DsDevice device in DsDevice.GetDevicesOfCat(DirectShowLib.FilterCategory.AudioRendererCategory))
      {
        try
        {
          if (device.Name != null)
          {
            {
              if (selection.Equals(device.Name))
              {
                DirectShowPropertyPage page = new DirectShowPropertyPage((DsDevice)device);
                page.Show(this);
              }
            }
          }
        }
        catch (Exception)
        {
        }
      }
    }

    private void configMPEG_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, videoCodecComboBox.Text);
    }

    private void configH264_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, h264videoCodecComboBox.Text);
    }

    private void configVC1_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, vc1videoCodecComboBox.Text);
    }

    private void configVC1i_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, vc1ivideoCodecComboBox.Text);
    }

    private void configDivxXvid_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, xvidvideoCodecComboBox.Text);
    }

    private void configMPEGAudio_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, audioCodecComboBox.Text);
    }

    private void configAACAudio_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, aacAudioCodecComboBox.Text);
    }

    private void configAudioRenderer_Click(object sender, EventArgs e)
    {
      ConfigAudioRendererCodecSection(sender, e, audioRendererComboBox.Text);
    }

    private void configSplitterSource_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, SplitterComboBox.Text);
    }

    private void configSplitterSync_Click(object sender, EventArgs e)
    {
      if (SplitterFileComboBox.Enabled)
      {
        ConfigCodecSection(sender, e, SplitterFileComboBox.Text);
      }
    }

    private void SplitterComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if ((SplitterComboBox.Text == "File Source (Async.)" || SplitterComboBox.Text == "File Source (URL)") && ForceSourceSplitter.Checked)
      {
        SplitterFileComboBox.Enabled = true;
      }
      else
      {
        SplitterFileComboBox.Enabled = false;
      }
    }
  }
}