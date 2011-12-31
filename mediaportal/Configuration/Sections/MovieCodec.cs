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
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class MovieCodec : SectionSettings
  {
    private bool _init = false;

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
        //ArrayList availableFileSyncFilters = FilterHelper.GetFilterSplitter();
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
        //Add Cyberlink PDVD10 Filter
        while (availableH264VideoFilters.Contains("CyberLink Video Decoder (PDVD10)"))
        {
          availableVC1VideoFilters.Add("CyberLink Video Decoder (PDVD10)");
          availableVC1IVideoFilters.Add("CyberLink Video Decoder (PDVD10)");
          break;
        }
        while (availableH264VideoFilters.Contains("CyberLink Video Decoder (PDVD11)"))
        {
          availableVC1VideoFilters.Add("CyberLink Video Decoder (PDVD11)");
          availableVC1IVideoFilters.Add("CyberLink Video Decoder (PDVD11)");
          break;
        }
        while (availableXVIDVideoFilters.Contains("CyberLink Video Decoder (PDVD11)"))
        {
          availableXVIDVideoFilters.Remove("CyberLink Video Decoder (PDVD11)");
          break;
        }
        while (availableXVIDVideoFilters.Contains("CyberLink Video Decoder (PDVD10)"))
        {
          availableXVIDVideoFilters.Remove("CyberLink Video Decoder (PDVD10)");
          break;
        }
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
        h264videoCodecComboBox.Items.AddRange(availableH264VideoFilters.ToArray());
        xvidvideoCodecComboBox.Items.AddRange(availableXVIDVideoFilters.ToArray());
        audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
        aacAudioCodecComboBox.Items.AddRange(availableAACAudioFilters.ToArray());
        _init = true;
        LoadSettings();
      }
      // Do always
      autoDecoderSettings.Visible = SettingsForm.AdvancedMode;
      ForceSourceSplitter.Visible = SettingsForm.AdvancedMode;
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
        UpdateDecoderSettings();
        audioRendererComboBox.SelectedItem = xmlreader.GetValueAsString("movieplayer", "audiorenderer",
                                                                        "Default DirectSound Device");
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
        if (audioCodec == string.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
          if (availableAudioFilters.Count > 0)
          {
            bool Mpeg2DecFilterFound = false;
            bool DScalerFilterFound = false;
            audioCodec = (string)availableAudioFilters[0];
            foreach (string filter in availableAudioFilters)
            {
              if (filter.Equals("MPC - MPA Decoder Filter"))
              {
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Audio Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound)
            {
              audioCodec = "MPC - MPA Decoder Filter";
            }
            else if (DScalerFilterFound)
            {
              audioCodec = "DScaler Audio Decoder";
            }
          }
        }
        Log.Info("  - videoCodec =(" + videoCodec + ")");
        if (videoCodec == string.Empty)
        {
          ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
          bool Mpeg2DecFilterFound = false;
          bool DScalerFilterFound = false;
          Log.Info(" - availableVideoFilters.Count = " + availableVideoFilters.Count.ToString());
          if (availableVideoFilters.Count > 0)
          {
            videoCodec = (string)availableVideoFilters[0];
            foreach (string filter in availableVideoFilters)
            {
              Log.Info(" - filter = (" + filter + ")");
              if (filter.Equals("MPC - MPEG-2 Video Decoder (Gabest)"))
              {
                Log.Info(" - MPC - MPEG-2 Video Decoder (Gabest)");
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Mpeg2 Video Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound)
            {
              videoCodec = "MPC - MPEG-2 Video Decoder (Gabest)";
            }
            else if (DScalerFilterFound)
            {
              videoCodec = "DScaler Mpeg2 Video Decoder";
            }
          }
        }
        if (h264videoCodec == string.Empty)
        {
          ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
          bool H264DecFilterFound = false;
          if (availableH264VideoFilters.Count > 0)
          {
            h264videoCodec = (string)availableH264VideoFilters[0];
            foreach (string filter in availableH264VideoFilters)
            {
              if (filter.Equals("CoreAVC Video Decoder"))
              {
                H264DecFilterFound = true;
              }
            }
            if (H264DecFilterFound)
            {
              h264videoCodec = "CoreAVC Video Decoder";
            }
          }
        }
        if (vc1videoCodec == string.Empty)
        {
          ArrayList availableVC1VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.VC1);
          bool VC1DecFilterFound = false;
          if (availableVC1VideoFilters.Count > 0)
          {
            vc1videoCodec = (string)availableVC1VideoFilters[0];
            foreach (string filter in availableVC1VideoFilters)
            {
              if (filter.Equals("CoreAVC Video Decoder"))
              {
                VC1DecFilterFound = true;
              }
            }
            if (VC1DecFilterFound)
            {
              vc1videoCodec = "CoreAVC Video Decoder";
            }
          }
        }
        if (vc1ivideoCodec == string.Empty)
        {
          ArrayList availableVC1VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.VC1);
          bool VC1IDecFilterFound = false;
          if (availableVC1VideoFilters.Count > 0)
          {
            vc1videoCodec = (string)availableVC1VideoFilters[0];
            foreach (string filter in availableVC1VideoFilters)
            {
              if (filter.Equals("CoreAVC Video Decoder"))
              {
                VC1IDecFilterFound = true;
              }
            }
            if (VC1IDecFilterFound)
            {
              vc1ivideoCodec = "CoreAVC Video Decoder";
            }
          }
        }
        if (aacaudioCodec == string.Empty)
        {
          ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.AAC);
          bool AACDecFilterFound = false;
          if (availableAACAudioFilters.Count > 0)
          {
            aacaudioCodec = (string)availableAACAudioFilters[0];
            foreach (string filter in availableAACAudioFilters)
            {
              if (filter.Equals("MONOGRAM ACC Decoder"))
              {
                AACDecFilterFound = true;
              }
            }
            if (AACDecFilterFound)
            {
              aacaudioCodec = "MONOGRAM ACC Decoder";
            }
          }
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
      }
    }

    /// <summary>
    /// updates the useable options if the auto decoder option is enabled.
    /// </summary>
    private void autoDecoderSettings_CheckedChanged(object sender, EventArgs e)
    {
      UpdateDecoderSettings();
      Startup._automaticMovieCodec = autoDecoderSettings.Checked;
    }

    /// <summary>
    /// updates the useable options if the force source splitter option is enabled.
    /// </summary>
    private void ForceSourceSplitter_CheckedChanged(object sender, EventArgs e)
    {
      UpdateDecoderSettings();
      Startup._automaticMovieFilter = ForceSourceSplitter.Checked;
    }

    private void videoCodecComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      /*
      h264videoCodecComboBox.SelectedIndexChanged -= h264videoCodecComboBox_SelectedIndexChanged;
      if (videoCodecComboBox.Text.Contains(Windows7Codec))
      {
        h264videoCodecComboBox.SelectedItem = Windows7Codec;
      }
      else
      {
        if (h264videoCodecComboBox.Text.Contains(Windows7Codec))
        {
          for (int i = 0; i < h264videoCodecComboBox.Items.Count; i++)
          {
            string listedCodec = h264videoCodecComboBox.Items[i].ToString();
            if (listedCodec == Windows7Codec) continue;
            h264videoCodecComboBox.SelectedItem = listedCodec;
            break;
          }
        }
      }
      h264videoCodecComboBox.SelectedIndexChanged += h264videoCodecComboBox_SelectedIndexChanged;
    */
    }

    private void h264videoCodecComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      /*
      videoCodecComboBox.SelectedIndexChanged -= videoCodecComboBox_SelectedIndexChanged;
      if (h264videoCodecComboBox.Text.Contains(Windows7Codec))
      {
        videoCodecComboBox.SelectedItem = Windows7Codec;
      }
      else
      {
        if (videoCodecComboBox.Text.Contains(Windows7Codec))
        {
          for (int i = 0; i < videoCodecComboBox.Items.Count; i++)
          {
            string listedCodec = videoCodecComboBox.Items[i].ToString();
            if (listedCodec == Windows7Codec) continue;
            videoCodecComboBox.SelectedItem = listedCodec;
            break;
          }
        }
      }
      videoCodecComboBox.SelectedIndexChanged += videoCodecComboBox_SelectedIndexChanged;
    */
    }

    private void vc1videoCodecComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      /*
      videoCodecComboBox.SelectedIndexChanged -= videoCodecComboBox_SelectedIndexChanged;
      if (vc1videoCodecComboBox.Text.Contains(Windows7Codec))
      {
        videoCodecComboBox.SelectedItem = Windows7Codec;
      }
      else
      {
        if (videoCodecComboBox.Text.Contains(Windows7Codec))
        {
          for (int i = 0; i < videoCodecComboBox.Items.Count; i++)
          {
            string listedCodec = videoCodecComboBox.Items[i].ToString();
            if (listedCodec == Windows7Codec) continue;
            videoCodecComboBox.SelectedItem = listedCodec;
            break;
          }
        }
      }
      videoCodecComboBox.SelectedIndexChanged += videoCodecComboBox_SelectedIndexChanged;
    */
    }

    private void vc1ivideoCodecComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      /*
      videoCodecComboBox.SelectedIndexChanged -= videoCodecComboBox_SelectedIndexChanged;
      if (vc1videoCodecComboBox.Text.Contains(Windows7Codec))
      {
        videoCodecComboBox.SelectedItem = Windows7Codec;
      }
      else
      {
        if (videoCodecComboBox.Text.Contains(Windows7Codec))
        {
          for (int i = 0; i < videoCodecComboBox.Items.Count; i++)
          {
            string listedCodec = videoCodecComboBox.Items[i].ToString();
            if (listedCodec == Windows7Codec) continue;
            videoCodecComboBox.SelectedItem = listedCodec;
            break;
          }
        }
      }
      videoCodecComboBox.SelectedIndexChanged += videoCodecComboBox_SelectedIndexChanged;
    */
    }

    private void SplitterComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpLabel5_Click(object sender, EventArgs e)
    {

    }
  }
}