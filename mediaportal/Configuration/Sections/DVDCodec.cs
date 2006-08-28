#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using System.Runtime.InteropServices;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.Util;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{

  public class DVDCodec : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPLabel audioRendererLabel;
    private MediaPortal.UserInterface.Controls.MPLabel audioCodecLabel;
    private MediaPortal.UserInterface.Controls.MPLabel videoCodecLabel;
    private MediaPortal.UserInterface.Controls.MPLabel dvdNavigatorLabel;
    private MediaPortal.UserInterface.Controls.MPComboBox audioRendererComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox audioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPComboBox dvdNavigatorComboBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAC3;
    //private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxRGB;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDXVA;
    private System.ComponentModel.IContainer components = null;
    bool _init = false;
    /// <summary>
    /// 
    /// </summary>
    public DVDCodec()
      : this("DVD Codec")
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
        //
        // Fetch available DirectShow filters
        //
        ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
        ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
        //
        // Fetch available DVD navigators
        //
        ArrayList availableDVDNavigators = FilterHelper.GetDVDNavigators();

        //
        // Fetch available Audio Renderers
        //
        ArrayList availableAudioRenderers = FilterHelper.GetAudioRenderers();

        //
        // Populate combo boxes
        //
        audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
        videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
        dvdNavigatorComboBox.Items.AddRange(availableDVDNavigators.ToArray());
        audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
        _init = true;
        LoadSettings();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      if (_init == false) return;
      Log.Info("load dvd");
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        string audioRenderer = xmlreader.GetValueAsString("dvdplayer", "audiorenderer", "Default DirectSound Device");
        string videoCodec = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
        string audioCodec = xmlreader.GetValueAsString("dvdplayer", "audiocodec", "");
        string dvdNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
        checkBoxAC3.Checked = xmlreader.GetValueAsBool("dvdplayer", "ac3", false);
        //checkBoxRGB.Checked = xmlreader.GetValueAsBool("dvdplayer", "usergbmode", false);
        checkBoxDXVA.Checked = xmlreader.GetValueAsBool("dvdplayer", "turnoffdxva", false);

        audioRendererComboBox.SelectedItem = audioRenderer;
        dvdNavigatorComboBox.SelectedItem = dvdNavigator;

        if (audioCodec == String.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
          if (availableAudioFilters.Count > 0)
          {
            bool Mpeg2DecFilterFound = true;
            bool DScalerFilterFound = true;
            audioCodec = (string)availableAudioFilters[0];
            foreach (string filter in availableAudioFilters)
            {
              if (filter.Equals("MPA Decoder Filter"))
              {
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Audio Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound) audioCodec = "MPA Decoder Filter";
            else if (DScalerFilterFound) audioCodec = "DScaler Audio Decoder";
          }
        }

        if (videoCodec == String.Empty)
        {
          ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
          bool Mpeg2DecFilterFound = true;
          bool DScalerFilterFound = true;
          if (availableVideoFilters.Count > 0)
          {
            videoCodec = (string)availableVideoFilters[0];
            foreach (string filter in availableVideoFilters)
            {
              if (filter.Equals("MPV Decoder Filter"))
              {
                Mpeg2DecFilterFound = true;
              }
              if (filter.Equals("DScaler Mpeg2 Video Decoder"))
              {
                DScalerFilterFound = true;
              }
            }
            if (Mpeg2DecFilterFound) videoCodec = "MPV Decoder Filter";
            else if (DScalerFilterFound) videoCodec = "DScaler Mpeg2 Video Decoder";
          }
        }

        audioCodecComboBox.SelectedItem = audioCodec;
        videoCodecComboBox.SelectedItem = videoCodec;

      }
      Log.Info("load dvd done");
    }

    public override void SaveSettings()
    {
      if (_init == false) return;

      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        xmlwriter.SetValue("dvdplayer", "audiorenderer", audioRendererComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "videocodec", videoCodecComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "navigator", dvdNavigatorComboBox.Text);
        xmlwriter.SetValueAsBool("dvdplayer", "ac3", checkBoxAC3.Checked);
        //xmlwriter.SetValueAsBool("dvdplayer", "usergbmode", checkBoxRGB.Checked);
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
      this.checkBoxDXVA = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxAC3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.dvdNavigatorComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.dvdNavigatorLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.videoCodecLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioCodecLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioRendererLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.checkBoxDXVA);
      this.groupBox1.Controls.Add(this.checkBoxAC3);
      this.groupBox1.Controls.Add(this.dvdNavigatorComboBox);
      this.groupBox1.Controls.Add(this.videoCodecComboBox);
      this.groupBox1.Controls.Add(this.audioCodecComboBox);
      this.groupBox1.Controls.Add(this.audioRendererComboBox);
      this.groupBox1.Controls.Add(this.dvdNavigatorLabel);
      this.groupBox1.Controls.Add(this.videoCodecLabel);
      this.groupBox1.Controls.Add(this.audioCodecLabel);
      this.groupBox1.Controls.Add(this.audioRendererLabel);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(469, 204);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // checkBoxDXVA
      // 
      this.checkBoxDXVA.AutoSize = true;
      this.checkBoxDXVA.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDXVA.Location = new System.Drawing.Point(19, 165);
      this.checkBoxDXVA.Name = "checkBoxDXVA";
      this.checkBoxDXVA.Size = new System.Drawing.Size(333, 17);
      this.checkBoxDXVA.TabIndex = 1;
      this.checkBoxDXVA.Text = "Turn off DxVA (use this option if you have DVD navigation issues)";
      this.checkBoxDXVA.UseVisualStyleBackColor = true;
      this.checkBoxDXVA.CheckedChanged += new System.EventHandler(this.checkBoxDXVA_CheckedChanged);
      // 
      // checkBoxAC3
      // 
      this.checkBoxAC3.AutoSize = true;
      this.checkBoxAC3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAC3.Location = new System.Drawing.Point(19, 139);
      this.checkBoxAC3.Name = "checkBoxAC3";
      this.checkBoxAC3.Size = new System.Drawing.Size(275, 17);
      this.checkBoxAC3.TabIndex = 9;
      this.checkBoxAC3.Text = "Use AC3 filter (for some soundcards using SPDIF out)";
      this.checkBoxAC3.UseVisualStyleBackColor = true;
      // 
      // dvdNavigatorComboBox
      // 
      this.dvdNavigatorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dvdNavigatorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.dvdNavigatorComboBox.Location = new System.Drawing.Point(168, 48);
      this.dvdNavigatorComboBox.Name = "dvdNavigatorComboBox";
      this.dvdNavigatorComboBox.Size = new System.Drawing.Size(285, 21);
      this.dvdNavigatorComboBox.TabIndex = 3;
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 24);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(285, 21);
      this.videoCodecComboBox.TabIndex = 1;
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 97);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(285, 21);
      this.audioCodecComboBox.TabIndex = 8;
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(168, 73);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(285, 21);
      this.audioRendererComboBox.TabIndex = 6;
      // 
      // dvdNavigatorLabel
      // 
      this.dvdNavigatorLabel.Location = new System.Drawing.Point(16, 52);
      this.dvdNavigatorLabel.Name = "dvdNavigatorLabel";
      this.dvdNavigatorLabel.Size = new System.Drawing.Size(88, 15);
      this.dvdNavigatorLabel.TabIndex = 2;
      this.dvdNavigatorLabel.Text = "DVD Navigator:";
      // 
      // videoCodecLabel
      // 
      this.videoCodecLabel.Location = new System.Drawing.Point(16, 28);
      this.videoCodecLabel.Name = "videoCodecLabel";
      this.videoCodecLabel.Size = new System.Drawing.Size(72, 16);
      this.videoCodecLabel.TabIndex = 0;
      this.videoCodecLabel.Text = "Video codec:";
      // 
      // audioCodecLabel
      // 
      this.audioCodecLabel.Location = new System.Drawing.Point(16, 101);
      this.audioCodecLabel.Name = "audioCodecLabel";
      this.audioCodecLabel.Size = new System.Drawing.Size(72, 17);
      this.audioCodecLabel.TabIndex = 7;
      this.audioCodecLabel.Text = "Audio codec:";
      // 
      // audioRendererLabel
      // 
      this.audioRendererLabel.Location = new System.Drawing.Point(16, 77);
      this.audioRendererLabel.Name = "audioRendererLabel";
      this.audioRendererLabel.Size = new System.Drawing.Size(88, 18);
      this.audioRendererLabel.TabIndex = 5;
      this.audioRendererLabel.Text = "Audio renderer:";
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

      private void groupBox1_Enter(object sender, EventArgs e)
      {

      }

      private void checkBoxDXVA_CheckedChanged(object sender, EventArgs e)
      {

      }
  }
}

