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
using System.ComponentModel;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class DVDCodec : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPLabel audioRendererLabel;
    private MPLabel audioCodecLabel;
    private MPLabel videoCodecLabel;
    private MPLabel dvdNavigatorLabel;
    private MPComboBox audioRendererComboBox;
    private MPComboBox audioCodecComboBox;
    private MPComboBox videoCodecComboBox;
    private MPComboBox dvdNavigatorComboBox;
    private MPCheckBox checkBoxAC3;
    private MPCheckBox checkBoxDXVA;
    private IContainer components = null;
    private MPLabel mpLabel1;
    private MPButton configAudioRenderer;
    private MPButton configDVDAudio;
    private MPButton configDVDVideo;
    private MPButton configDVDNav;
    private bool _init = false;

    /// <summary>
    /// 
    /// </summary>
    public DVDCodec()
      : this("DVD Discs/Images Codecs") {}

    /// <summary>
    /// 
    /// </summary>
    public DVDCodec(string name)
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
        // Fetch available DirectShow filters
        ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
        //Remove Muxer's from the list to avoid confusion.
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
        ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
        //Remove Muxer's from Audio decoder list to avoid confusion.
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
        // Fetch available DVD navigators
        ArrayList availableDVDNavigators = FilterHelper.GetDVDNavigators();
        // Fetch available Audio Renderers
        ArrayList availableAudioRenderers = FilterHelper.GetAudioRenderers();
        // Populate combo boxes
        audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
        videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
        dvdNavigatorComboBox.Items.AddRange(availableDVDNavigators.ToArray());
        audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
        _init = true;
        LoadSettings();
      }
    }

    /// <summary>
    /// Load DVD codec & options
    /// </summary>
    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }
      Log.Info("load dvd");
      using (Settings xmlreader = new MPSettings())
      {
        string audioRenderer = xmlreader.GetValueAsString("dvdplayer", "audiorenderer", "Default DirectSound Device");
        string videoCodec = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
        string audioCodec = xmlreader.GetValueAsString("dvdplayer", "audiocodec", "");
        string dvdNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
        checkBoxAC3.Checked = xmlreader.GetValueAsBool("dvdplayer", "ac3", false);
        checkBoxDXVA.Checked = xmlreader.GetValueAsBool("dvdplayer", "turnoffdxva", false);
        audioRendererComboBox.SelectedItem = audioRenderer;
        dvdNavigatorComboBox.SelectedItem = dvdNavigator;

        if (videoCodec == string.Empty)
        {
          ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
          videoCodec = SetCodecBox(availableVideoFilters, "LAV Video Decoder", "DScaler Mpeg2 Video Decoder", "");
        }
        if (audioCodec == string.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
          audioCodec = SetCodecBox(availableAudioFilters, "LAV Audio Decoder", "DScaler Audio Decoder", "ffdshow Audio Decoder");
        }

        audioCodecComboBox.Text = audioCodec;
        videoCodecComboBox.Text = videoCodec;
        CheckBoxValid(audioCodecComboBox);
        CheckBoxValid(videoCodecComboBox);
        CheckBoxValid(dvdNavigatorComboBox);
        CheckBoxValid(audioRendererComboBox);
      }
      Log.Info("load dvd done");
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
    /// Save DVD codec & options
    /// </summary>
    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("dvdplayer", "audiorenderer", audioRendererComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "videocodec", videoCodecComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "navigator", dvdNavigatorComboBox.Text);
        xmlwriter.SetValueAsBool("dvdplayer", "ac3", checkBoxAC3.Checked);
        xmlwriter.SetValueAsBool("dvdplayer", "turnoffdxva", checkBoxDXVA.Checked);
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.configAudioRenderer = new MediaPortal.UserInterface.Controls.MPButton();
      this.configDVDAudio = new MediaPortal.UserInterface.Controls.MPButton();
      this.configDVDVideo = new MediaPortal.UserInterface.Controls.MPButton();
      this.configDVDNav = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxDXVA = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.audioRendererLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.videoCodecLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxAC3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.dvdNavigatorComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.dvdNavigatorLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioCodecLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.configAudioRenderer);
      this.groupBox1.Controls.Add(this.configDVDAudio);
      this.groupBox1.Controls.Add(this.configDVDVideo);
      this.groupBox1.Controls.Add(this.configDVDNav);
      this.groupBox1.Controls.Add(this.mpLabel1);
      this.groupBox1.Controls.Add(this.checkBoxDXVA);
      this.groupBox1.Controls.Add(this.audioRendererLabel);
      this.groupBox1.Controls.Add(this.audioRendererComboBox);
      this.groupBox1.Controls.Add(this.videoCodecLabel);
      this.groupBox1.Controls.Add(this.videoCodecComboBox);
      this.groupBox1.Controls.Add(this.checkBoxAC3);
      this.groupBox1.Controls.Add(this.dvdNavigatorComboBox);
      this.groupBox1.Controls.Add(this.audioCodecComboBox);
      this.groupBox1.Controls.Add(this.dvdNavigatorLabel);
      this.groupBox1.Controls.Add(this.audioCodecLabel);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(6, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(462, 226);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings Decoder";
      // 
      // configAudioRenderer
      // 
      this.configAudioRenderer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configAudioRenderer.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configAudioRenderer.Location = new System.Drawing.Point(422, 120);
      this.configAudioRenderer.Name = "configAudioRenderer";
      this.configAudioRenderer.Size = new System.Drawing.Size(35, 21);
      this.configAudioRenderer.TabIndex = 73;
      this.configAudioRenderer.UseVisualStyleBackColor = true;
      this.configAudioRenderer.Click += new System.EventHandler(this.configAudioRenderer_Click);
      // 
      // configDVDAudio
      // 
      this.configDVDAudio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configDVDAudio.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configDVDAudio.Location = new System.Drawing.Point(422, 96);
      this.configDVDAudio.Name = "configDVDAudio";
      this.configDVDAudio.Size = new System.Drawing.Size(35, 21);
      this.configDVDAudio.TabIndex = 72;
      this.configDVDAudio.UseVisualStyleBackColor = true;
      this.configDVDAudio.Click += new System.EventHandler(this.configDVDAudio_Click);
      // 
      // configDVDVideo
      // 
      this.configDVDVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configDVDVideo.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configDVDVideo.Location = new System.Drawing.Point(422, 72);
      this.configDVDVideo.Name = "configDVDVideo";
      this.configDVDVideo.Size = new System.Drawing.Size(35, 21);
      this.configDVDVideo.TabIndex = 71;
      this.configDVDVideo.UseVisualStyleBackColor = true;
      this.configDVDVideo.Click += new System.EventHandler(this.configDVDVideo_Click);
      // 
      // configDVDNav
      // 
      this.configDVDNav.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.configDVDNav.Image = global::MediaPortal.Configuration.Properties.Resources.codec_screwdriver;
      this.configDVDNav.Location = new System.Drawing.Point(422, 24);
      this.configDVDNav.Name = "configDVDNav";
      this.configDVDNav.Size = new System.Drawing.Size(35, 21);
      this.configDVDNav.TabIndex = 70;
      this.configDVDNav.UseVisualStyleBackColor = true;
      this.configDVDNav.Click += new System.EventHandler(this.configDVDNav_Click);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(119, 49);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(281, 13);
      this.mpLabel1.TabIndex = 10;
      this.mpLabel1.Text = "Note: Use corresponding decoders with chosen Navigator";
      // 
      // checkBoxDXVA
      // 
      this.checkBoxDXVA.AutoSize = true;
      this.checkBoxDXVA.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDXVA.Location = new System.Drawing.Point(19, 185);
      this.checkBoxDXVA.Name = "checkBoxDXVA";
      this.checkBoxDXVA.Size = new System.Drawing.Size(333, 17);
      this.checkBoxDXVA.TabIndex = 6;
      this.checkBoxDXVA.Text = "Turn off DxVA (use this option if you have DVD navigation issues)";
      this.checkBoxDXVA.UseVisualStyleBackColor = true;
      // 
      // audioRendererLabel
      // 
      this.audioRendererLabel.Location = new System.Drawing.Point(17, 124);
      this.audioRendererLabel.Name = "audioRendererLabel";
      this.audioRendererLabel.Size = new System.Drawing.Size(88, 18);
      this.audioRendererLabel.TabIndex = 4;
      this.audioRendererLabel.Text = "Audio renderer:";
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(122, 120);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(295, 21);
      this.audioRendererComboBox.Sorted = true;
      this.audioRendererComboBox.TabIndex = 4;
      // 
      // videoCodecLabel
      // 
      this.videoCodecLabel.Location = new System.Drawing.Point(17, 77);
      this.videoCodecLabel.Name = "videoCodecLabel";
      this.videoCodecLabel.Size = new System.Drawing.Size(99, 16);
      this.videoCodecLabel.TabIndex = 0;
      this.videoCodecLabel.Text = "DVD video :";
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(122, 72);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.videoCodecComboBox.Sorted = true;
      this.videoCodecComboBox.TabIndex = 2;
      // 
      // checkBoxAC3
      // 
      this.checkBoxAC3.AutoSize = true;
      this.checkBoxAC3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAC3.Location = new System.Drawing.Point(19, 156);
      this.checkBoxAC3.Name = "checkBoxAC3";
      this.checkBoxAC3.Size = new System.Drawing.Size(275, 17);
      this.checkBoxAC3.TabIndex = 5;
      this.checkBoxAC3.Text = "Use AC3 filter (for some soundcards using SPDIF out)";
      this.checkBoxAC3.UseVisualStyleBackColor = true;
      // 
      // dvdNavigatorComboBox
      // 
      this.dvdNavigatorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dvdNavigatorComboBox.BorderColor = System.Drawing.Color.Empty;
      this.dvdNavigatorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.dvdNavigatorComboBox.Location = new System.Drawing.Point(122, 24);
      this.dvdNavigatorComboBox.Name = "dvdNavigatorComboBox";
      this.dvdNavigatorComboBox.Size = new System.Drawing.Size(295, 21);
      this.dvdNavigatorComboBox.Sorted = true;
      this.dvdNavigatorComboBox.TabIndex = 1;
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(122, 96);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(295, 21);
      this.audioCodecComboBox.Sorted = true;
      this.audioCodecComboBox.TabIndex = 3;
      // 
      // dvdNavigatorLabel
      // 
      this.dvdNavigatorLabel.Location = new System.Drawing.Point(17, 28);
      this.dvdNavigatorLabel.Name = "dvdNavigatorLabel";
      this.dvdNavigatorLabel.Size = new System.Drawing.Size(88, 15);
      this.dvdNavigatorLabel.TabIndex = 2;
      this.dvdNavigatorLabel.Text = "DVD Navigator:";
      // 
      // audioCodecLabel
      // 
      this.audioCodecLabel.Location = new System.Drawing.Point(17, 99);
      this.audioCodecLabel.Name = "audioCodecLabel";
      this.audioCodecLabel.Size = new System.Drawing.Size(117, 17);
      this.audioCodecLabel.TabIndex = 6;
      this.audioCodecLabel.Text = "DVD audio :";
      // 
      // DVDCodec
      // 
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.groupBox1);
      this.Name = "DVDCodec";
      this.Size = new System.Drawing.Size(472, 391);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

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
                  DirectShowPropertyPage page = new DirectShowPropertyPage((DsDevice)device);
                  page.Show(this);

                  // Rename Configuration subkey to MediaPortal to apply Cyberlink setting
                  RegConfigtoMP(@"Software\Cyberlink\Common\clcvd");
                  RegConfigtoMP(@"Software\Cyberlink\Common\cl264dec");
                  RegConfigtoMP(@"Software\Cyberlink\Common\CLVSD");
                  RegConfigtoMP(@"Software\Cyberlink\Common\CLAud");
                }
                else if (!selection.StartsWith("DVD Navigator"))
                {
                  DirectShowPropertyPage page = new DirectShowPropertyPage((DsDevice)device);
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

    private void configDVDNav_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, dvdNavigatorComboBox.Text);
    }

    private void configDVDVideo_Click(object sender, System.EventArgs e)
    {
      ConfigCodecSection(sender, e, videoCodecComboBox.Text);
    }

    private void configDVDAudio_Click(object sender, EventArgs e)
    {
      ConfigCodecSection(sender, e, audioCodecComboBox.Text);
    }

    private void configAudioRenderer_Click(object sender, EventArgs e)
    {
      ConfigAudioRendererCodecSection(sender, e, audioRendererComboBox.Text);
    }
  }
}