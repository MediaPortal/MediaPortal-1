#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.Collections;
using System.ComponentModel;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

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
    private bool _init = false;

    /// <summary>
    /// 
    /// </summary>
    public DVDCodec()
      : this("DVD Codecs")
    {
    }

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
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string audioRenderer = xmlreader.GetValueAsString("dvdplayer", "audiorenderer", "Default DirectSound Device");
        string videoCodec = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
        string audioCodec = xmlreader.GetValueAsString("dvdplayer", "audiocodec", "");
        string dvdNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
        checkBoxAC3.Checked = xmlreader.GetValueAsBool("dvdplayer", "ac3", false);
        checkBoxDXVA.Checked = xmlreader.GetValueAsBool("dvdplayer", "turnoffdxva", false);
        audioRendererComboBox.SelectedItem = audioRenderer;
        dvdNavigatorComboBox.SelectedItem = dvdNavigator;
        if (audioCodec == string.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
          if (availableAudioFilters.Count > 0)
          {
            bool Mpeg2DecFilterFound = true;
            bool DScalerFilterFound = true;
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
        if (videoCodec == string.Empty)
        {
          ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
          bool Mpeg2DecFilterFound = false;
          bool DScalerFilterFound = false;
          if (availableVideoFilters.Count > 0)
          {
            videoCodec = (string) availableVideoFilters[0];
            foreach (string filter in availableVideoFilters)
            {
              if (filter.Equals("MPC - MPEG-2 Video Decoder (Gabest)"))
              {
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
        audioCodecComboBox.Text = audioCodec;
        videoCodecComboBox.Text = videoCodec;
      }
      Log.Info("load dvd done");
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
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
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
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(469, 216);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(165, 52);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(281, 13);
      this.mpLabel1.TabIndex = 10;
      this.mpLabel1.Text = "Note: Use corresponding decoders with chosen Navigator";
      // 
      // checkBoxDXVA
      // 
      this.checkBoxDXVA.AutoSize = true;
      this.checkBoxDXVA.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDXVA.Location = new System.Drawing.Point(19, 186);
      this.checkBoxDXVA.Name = "checkBoxDXVA";
      this.checkBoxDXVA.Size = new System.Drawing.Size(333, 17);
      this.checkBoxDXVA.TabIndex = 9;
      this.checkBoxDXVA.Text = "Turn off DxVA (use this option if you have DVD navigation issues)";
      this.checkBoxDXVA.UseVisualStyleBackColor = true;
      // 
      // audioRendererLabel
      // 
      this.audioRendererLabel.Location = new System.Drawing.Point(16, 121);
      this.audioRendererLabel.Name = "audioRendererLabel";
      this.audioRendererLabel.Size = new System.Drawing.Size(88, 18);
      this.audioRendererLabel.TabIndex = 4;
      this.audioRendererLabel.Text = "Audio renderer:";
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(168, 118);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(285, 21);
      this.audioRendererComboBox.TabIndex = 5;
      // 
      // videoCodecLabel
      // 
      this.videoCodecLabel.Location = new System.Drawing.Point(16, 73);
      this.videoCodecLabel.Name = "videoCodecLabel";
      this.videoCodecLabel.Size = new System.Drawing.Size(117, 16);
      this.videoCodecLabel.TabIndex = 0;
      this.videoCodecLabel.Text = "DVD video decoder:";
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 70);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(285, 21);
      this.videoCodecComboBox.TabIndex = 1;
      // 
      // checkBoxAC3
      // 
      this.checkBoxAC3.AutoSize = true;
      this.checkBoxAC3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAC3.Location = new System.Drawing.Point(19, 160);
      this.checkBoxAC3.Name = "checkBoxAC3";
      this.checkBoxAC3.Size = new System.Drawing.Size(275, 17);
      this.checkBoxAC3.TabIndex = 8;
      this.checkBoxAC3.Text = "Use AC3 filter (for some soundcards using SPDIF out)";
      this.checkBoxAC3.UseVisualStyleBackColor = true;
      // 
      // dvdNavigatorComboBox
      // 
      this.dvdNavigatorComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.dvdNavigatorComboBox.BorderColor = System.Drawing.Color.Empty;
      this.dvdNavigatorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.dvdNavigatorComboBox.Location = new System.Drawing.Point(168, 25);
      this.dvdNavigatorComboBox.Name = "dvdNavigatorComboBox";
      this.dvdNavigatorComboBox.Size = new System.Drawing.Size(285, 21);
      this.dvdNavigatorComboBox.TabIndex = 3;
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 94);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(285, 21);
      this.audioCodecComboBox.TabIndex = 7;
      // 
      // dvdNavigatorLabel
      // 
      this.dvdNavigatorLabel.Location = new System.Drawing.Point(16, 28);
      this.dvdNavigatorLabel.Name = "dvdNavigatorLabel";
      this.dvdNavigatorLabel.Size = new System.Drawing.Size(88, 15);
      this.dvdNavigatorLabel.TabIndex = 2;
      this.dvdNavigatorLabel.Text = "DVD Navigator:";
      // 
      // audioCodecLabel
      // 
      this.audioCodecLabel.Location = new System.Drawing.Point(16, 97);
      this.audioCodecLabel.Name = "audioCodecLabel";
      this.audioCodecLabel.Size = new System.Drawing.Size(117, 17);
      this.audioCodecLabel.TabIndex = 6;
      this.audioCodecLabel.Text = "DVD audio decoder:";
      // 
      // DVDCodec
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "DVDCodec";
      this.Size = new System.Drawing.Size(472, 391);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion
  }
}