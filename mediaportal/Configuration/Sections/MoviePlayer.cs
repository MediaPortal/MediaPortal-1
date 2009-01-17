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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class MoviePlayer : SectionSettings
  {
    private MPGroupBox groupBoxExternalPlayer;
    private MPButton parametersButton;
    private MPTextBox parametersTextBox;
    private MPLabel label2;
    private MPButton fileNameButton;
    private MPTextBox fileNameTextBox;
    private MPLabel label1;
    private MPGroupBox mpGroupBox1;
    private MPComboBox audioRendererComboBox;
    private MPLabel label3;
    private MPCheckBox externalPlayerCheckBox;
    private OpenFileDialog openFileDialog;
    private MPComboBox audioCodecComboBox;
    private MPLabel label5;
    private MPComboBox videoCodecComboBox;
    private MPLabel label6;
    private IContainer components = null;
    private MPLabel mpLabel1;
    private MPComboBox h264videoCodecComboBox;
    private MPCheckBox autoDecoderSettings;
    private MPGroupBox wmvGroupBox;
    private MPCheckBox wmvCheckBox;
    private MPLabel mpLabel2;
    private MPCheckBox enableAudioDualMonoModes;
    private MPLabel labelAACDecoder;
    private MPComboBox aacAudioCodecComboBox;
    private bool _init = false;

    public MoviePlayer()
      : this("Video Player")
    {
    }

    public MoviePlayer(string name)
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
        ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.AAC);
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
        h264videoCodecComboBox.Items.AddRange(availableH264VideoFilters.ToArray());
        audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
        aacAudioCodecComboBox.Items.AddRange(availableAACAudioFilters.ToArray());
        _init = true;
        LoadSettings();
      }
      // Do always
      autoDecoderSettings.Visible = SettingsForm.AdvancedMode;
      groupBoxExternalPlayer.Visible = SettingsForm.AdvancedMode;
    }

    /// <summary>
    /// sets useability of select config depending on whether auot decoder stting option is enabled.
    /// </summary>
    public void UpdateDecoderSettings()
    {
      label5.Enabled = !autoDecoderSettings.Checked;
      label6.Enabled = !autoDecoderSettings.Checked;
      mpLabel1.Enabled = !autoDecoderSettings.Checked;
      videoCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      h264videoCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      audioCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      aacAudioCodecComboBox.Enabled = !autoDecoderSettings.Checked;
      wmvCheckBox.Enabled = !autoDecoderSettings.Checked;
    }

    /// <summary>
    /// Loads the movie player settings
    /// </summary>
    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        autoDecoderSettings.Checked = xmlreader.GetValueAsBool("movieplayer", "autodecodersettings", false);
        enableAudioDualMonoModes.Checked = xmlreader.GetValueAsBool("movieplayer", "audiodualmono", false);
        UpdateDecoderSettings();
        fileNameTextBox.Text = xmlreader.GetValueAsString("movieplayer", "path", "");
        parametersTextBox.Text = xmlreader.GetValueAsString("movieplayer", "arguments", "");
        externalPlayerCheckBox.Checked = xmlreader.GetValueAsBool("movieplayer", "internal", true);
        externalPlayerCheckBox.Checked = !externalPlayerCheckBox.Checked;
        audioRendererComboBox.SelectedItem = xmlreader.GetValueAsString("movieplayer", "audiorenderer",
                                                                        "Default DirectSound Device");
        wmvCheckBox.Checked = xmlreader.GetValueAsBool("movieplayer", "wmvaudio", false);
        // Set codecs
        string videoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
        string h264videoCodec = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
        string audioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
        string aacaudioCodec = xmlreader.GetValueAsString("movieplayer", "aacaudiocodec", "");
        if (audioCodec == string.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
          if (availableAudioFilters.Count > 0)
          {
            bool Mpeg2DecFilterFound = false;
            bool DScalerFilterFound = false;
            audioCodec = (string) availableAudioFilters[0];
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
            videoCodec = (string) availableVideoFilters[0];
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
            h264videoCodec = (string) availableH264VideoFilters[0];
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
        if (aacaudioCodec == string.Empty)
        {
          ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.AAC);
          bool AACDecFilterFound = false;
          if (availableAACAudioFilters.Count > 0)
          {
            aacaudioCodec = (string) availableAACAudioFilters[0];
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
        aacAudioCodecComboBox.Text = aacaudioCodec;
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
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("movieplayer", "autodecodersettings", autoDecoderSettings.Checked);
        xmlwriter.SetValueAsBool("movieplayer", "audiodualmono", enableAudioDualMonoModes.Checked);
        xmlwriter.SetValue("movieplayer", "path", fileNameTextBox.Text);
        xmlwriter.SetValue("movieplayer", "arguments", parametersTextBox.Text);
        xmlwriter.SetValueAsBool("movieplayer", "internal", !externalPlayerCheckBox.Checked);
        xmlwriter.SetValue("movieplayer", "audiorenderer", audioRendererComboBox.Text);
        xmlwriter.SetValueAsBool("movieplayer", "wmvaudio", wmvCheckBox.Checked);
        // Set codecs
        xmlwriter.SetValue("movieplayer", "mpeg2audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "mpeg2videocodec", videoCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "h264videocodec", h264videoCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "aacaudiocodec", aacAudioCodecComboBox.Text);
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBoxExternalPlayer = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.externalPlayerCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.parametersButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.parametersTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.fileNameButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.fileNameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelAACDecoder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.aacAudioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.enableAudioDualMonoModes = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.autoDecoderSettings = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.h264videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.wmvGroupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.wmvCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxExternalPlayer.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.wmvGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxExternalPlayer
      // 
      this.groupBoxExternalPlayer.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxExternalPlayer.Controls.Add(this.externalPlayerCheckBox);
      this.groupBoxExternalPlayer.Controls.Add(this.parametersButton);
      this.groupBoxExternalPlayer.Controls.Add(this.parametersTextBox);
      this.groupBoxExternalPlayer.Controls.Add(this.label2);
      this.groupBoxExternalPlayer.Controls.Add(this.fileNameButton);
      this.groupBoxExternalPlayer.Controls.Add(this.fileNameTextBox);
      this.groupBoxExternalPlayer.Controls.Add(this.label1);
      this.groupBoxExternalPlayer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxExternalPlayer.Location = new System.Drawing.Point(0, 292);
      this.groupBoxExternalPlayer.Name = "groupBoxExternalPlayer";
      this.groupBoxExternalPlayer.Size = new System.Drawing.Size(472, 112);
      this.groupBoxExternalPlayer.TabIndex = 1;
      this.groupBoxExternalPlayer.TabStop = false;
      this.groupBoxExternalPlayer.Text = "External player";
      // 
      // externalPlayerCheckBox
      // 
      this.externalPlayerCheckBox.AutoSize = true;
      this.externalPlayerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.externalPlayerCheckBox.Location = new System.Drawing.Point(19, 28);
      this.externalPlayerCheckBox.Name = "externalPlayerCheckBox";
      this.externalPlayerCheckBox.Size = new System.Drawing.Size(231, 17);
      this.externalPlayerCheckBox.TabIndex = 0;
      this.externalPlayerCheckBox.Text = "Use external player (replaces internal player)";
      this.externalPlayerCheckBox.UseVisualStyleBackColor = true;
      this.externalPlayerCheckBox.CheckedChanged += new System.EventHandler(this.externalPlayerCheckBox_CheckedChanged);
      // 
      // parametersButton
      // 
      this.parametersButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersButton.Location = new System.Drawing.Point(384, 84);
      this.parametersButton.Name = "parametersButton";
      this.parametersButton.Size = new System.Drawing.Size(72, 22);
      this.parametersButton.TabIndex = 6;
      this.parametersButton.Text = "List";
      this.parametersButton.UseVisualStyleBackColor = true;
      this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.BorderColor = System.Drawing.Color.Empty;
      this.parametersTextBox.Location = new System.Drawing.Point(168, 84);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(208, 20);
      this.parametersTextBox.TabIndex = 5;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 88);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 15);
      this.label2.TabIndex = 4;
      this.label2.Text = "Parameters:";
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.Location = new System.Drawing.Point(384, 60);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(72, 22);
      this.fileNameButton.TabIndex = 3;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.UseVisualStyleBackColor = true;
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // fileNameTextBox
      // 
      this.fileNameTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameTextBox.BorderColor = System.Drawing.Color.Empty;
      this.fileNameTextBox.Location = new System.Drawing.Point(168, 60);
      this.fileNameTextBox.Name = "fileNameTextBox";
      this.fileNameTextBox.Size = new System.Drawing.Size(208, 20);
      this.fileNameTextBox.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 64);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "Path/Filename:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.labelAACDecoder);
      this.mpGroupBox1.Controls.Add(this.aacAudioCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.enableAudioDualMonoModes);
      this.mpGroupBox1.Controls.Add(this.autoDecoderSettings);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.h264videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.audioRendererComboBox);
      this.mpGroupBox1.Controls.Add(this.label3);
      this.mpGroupBox1.Controls.Add(this.label6);
      this.mpGroupBox1.Controls.Add(this.audioCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.label5);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 221);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Codec Settings (internal player)";
      // 
      // labelAACDecoder
      // 
      this.labelAACDecoder.Location = new System.Drawing.Point(16, 99);
      this.labelAACDecoder.Name = "labelAACDecoder";
      this.labelAACDecoder.Size = new System.Drawing.Size(146, 17);
      this.labelAACDecoder.TabIndex = 14;
      this.labelAACDecoder.Text = "AAC audio decoder:";
      // 
      // aacAudioCodecComboBox
      // 
      this.aacAudioCodecComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.aacAudioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.aacAudioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.aacAudioCodecComboBox.Location = new System.Drawing.Point(168, 96);
      this.aacAudioCodecComboBox.Name = "aacAudioCodecComboBox";
      this.aacAudioCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.aacAudioCodecComboBox.TabIndex = 15;
      // 
      // enableAudioDualMonoModes
      // 
      this.enableAudioDualMonoModes.AutoSize = true;
      this.enableAudioDualMonoModes.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.enableAudioDualMonoModes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.enableAudioDualMonoModes.Location = new System.Drawing.Point(19, 182);
      this.enableAudioDualMonoModes.Name = "enableAudioDualMonoModes";
      this.enableAudioDualMonoModes.Size = new System.Drawing.Size(386, 30);
      this.enableAudioDualMonoModes.TabIndex = 10;
      this.enableAudioDualMonoModes.Text =
        "Enable AudioDualMono mode switching\r\n(if 1 audio stream contains 2x mono channels" +
        ", you can switch between them)";
      this.enableAudioDualMonoModes.UseVisualStyleBackColor = true;
      // 
      // autoDecoderSettings
      // 
      this.autoDecoderSettings.AutoSize = true;
      this.autoDecoderSettings.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.autoDecoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.autoDecoderSettings.Location = new System.Drawing.Point(19, 149);
      this.autoDecoderSettings.Name = "autoDecoderSettings";
      this.autoDecoderSettings.Size = new System.Drawing.Size(309, 30);
      this.autoDecoderSettings.TabIndex = 0;
      this.autoDecoderSettings.Text =
        "Automatic Decoder Settings \r\n(use with caution - knowledge of DirectShow merits r" +
        "equired)";
      this.autoDecoderSettings.UseVisualStyleBackColor = true;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(16, 52);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(146, 16);
      this.mpLabel1.TabIndex = 8;
      this.mpLabel1.Text = "H.264 video decoder:";
      // 
      // h264videoCodecComboBox
      // 
      this.h264videoCodecComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.h264videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.h264videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.h264videoCodecComboBox.Location = new System.Drawing.Point(168, 48);
      this.h264videoCodecComboBox.Name = "h264videoCodecComboBox";
      this.h264videoCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.h264videoCodecComboBox.TabIndex = 9;
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(168, 120);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioRendererComboBox.TabIndex = 7;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 124);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "Audio renderer:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 28);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(146, 16);
      this.label6.TabIndex = 0;
      this.label6.Text = "MPEG-2 video decoder:";
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 72);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioCodecComboBox.TabIndex = 3;
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 24);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.videoCodecComboBox.TabIndex = 1;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 76);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(158, 17);
      this.label5.TabIndex = 2;
      this.label5.Text = "MPEG / AC3 audio decoder:";
      // 
      // wmvGroupBox
      // 
      this.wmvGroupBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.wmvGroupBox.Controls.Add(this.mpLabel2);
      this.wmvGroupBox.Controls.Add(this.wmvCheckBox);
      this.wmvGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.wmvGroupBox.Location = new System.Drawing.Point(0, 227);
      this.wmvGroupBox.Name = "wmvGroupBox";
      this.wmvGroupBox.Size = new System.Drawing.Size(472, 62);
      this.wmvGroupBox.TabIndex = 7;
      this.wmvGroupBox.TabStop = false;
      this.wmvGroupBox.Text = "WMV playback (internal player)";
      // 
      // mpLabel2
      // 
      this.mpLabel2.Location = new System.Drawing.Point(34, 39);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(326, 16);
      this.mpLabel2.TabIndex = 10;
      this.mpLabel2.Text = "Will not be applied if Automatic Decoder Settings enabled.";
      // 
      // wmvCheckBox
      // 
      this.wmvCheckBox.AutoSize = true;
      this.wmvCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.wmvCheckBox.Location = new System.Drawing.Point(19, 19);
      this.wmvCheckBox.Name = "wmvCheckBox";
      this.wmvCheckBox.Size = new System.Drawing.Size(233, 17);
      this.wmvCheckBox.TabIndex = 0;
      this.wmvCheckBox.Text = "Use 5.1 audio playback for WMV movie files";
      this.wmvCheckBox.UseVisualStyleBackColor = true;
      // 
      // MoviePlayer
      // 
      this.Controls.Add(this.wmvGroupBox);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.groupBoxExternalPlayer);
      this.Name = "MoviePlayer";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxExternalPlayer.ResumeLayout(false);
      this.groupBoxExternalPlayer.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.wmvGroupBox.ResumeLayout(false);
      this.wmvGroupBox.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void externalPlayerCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      fileNameTextBox.Enabled =
        fileNameButton.Enabled = parametersTextBox.Enabled = parametersButton.Enabled = externalPlayerCheckBox.Checked;
    }

    /// <summary>
    /// sets the external movies player source file.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void fileNameButton_Click(object sender, EventArgs e)
    {
      using (openFileDialog = new OpenFileDialog())
      {
        openFileDialog.FileName = fileNameTextBox.Text;
        openFileDialog.CheckFileExists = true;
        openFileDialog.RestoreDirectory = true;
        openFileDialog.Filter = "exe files (*.exe)|*.exe";
        openFileDialog.FilterIndex = 0;
        openFileDialog.Title = "Select movie player";
        DialogResult dialogResult = openFileDialog.ShowDialog();
        if (dialogResult == DialogResult.OK)
        {
          fileNameTextBox.Text = openFileDialog.FileName;
        }
      }
    }

    /// <summary>
    /// sets the external movies player parameters.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void parametersButton_Click(object sender, EventArgs e)
    {
      ParameterForm parameters = new ParameterForm();
      parameters.AddParameter("%filename%", "Will be replaced by currently selected media file");
      if (parameters.ShowDialog(parametersButton) == DialogResult.OK)
      {
        parametersTextBox.Text += parameters.SelectedParameter;
      }
    }

    /// <summary>
    /// updates the useable options if the auto decoder option is enabled.
    /// </summary>
    private void autoDecoderSettings_CheckedChanged(object sender, EventArgs e)
    {
      UpdateDecoderSettings();
    }
  }
}