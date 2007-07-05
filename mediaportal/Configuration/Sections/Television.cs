#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using MediaPortal.Util;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class Television : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButton1;
    private MediaPortal.UserInterface.Controls.MPComboBox audioCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel labelAudioRenderer;
    private MediaPortal.UserInterface.Controls.MPComboBox audioRendererComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPComboBox videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPComboBox defaultZoomModeComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private System.ComponentModel.IContainer components = null;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox4;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxTimeShiftBuffer;
    private MediaPortal.UserInterface.Controls.MPLabel label8;
    private MediaPortal.UserInterface.Controls.MPComboBox cbDeinterlace;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPCheckBox byIndexCheckBox;
    bool _init = false;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbTurnOnTimeShift;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox5;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbTurnOnTv;
    private MediaPortal.UserInterface.Controls.MPGroupBox gAllowedModes;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowNormal;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowZoom149;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowOriginal;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowZoom;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowLetterbox;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowStretch;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbAllowPanScan;
    private MediaPortal.UserInterface.Controls.MPComboBox h264videoCodecComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    string[] aspectRatio = { "normal", "original", "stretch", "zoom", "zoom149", "letterbox", "panscan" };

    public Television()
      : this("Television")
    {
    }

    public Television(string name)
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
        // Populate video and audio codecs
        //
        ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
        ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264Video);
        ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
        ArrayList availableAudioRenderers = FilterHelper.GetAudioRenderers();
        //Remove Cyberlink Muxer from the list to avoid confusion.
        if (availableVideoFilters.Contains("CyberLink MPEG Muxer"))
        {
          availableVideoFilters.Remove("CyberLink MPEG Muxer");
        }
        videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
        h264videoCodecComboBox.Items.AddRange(availableH264VideoFilters.ToArray());
        if (availableAudioFilters.Contains("CyberLink MPEG Muxer"))
        {
          availableAudioFilters.Remove("CyberLink MPEG Muxer");
        }
        audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
        audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
        _init = true;
        LoadSettings();
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
        this.h264videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
        this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.cbDeinterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
        this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.defaultZoomModeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
        this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
        this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
        this.labelAudioRenderer = new MediaPortal.UserInterface.Controls.MPLabel();
        this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
        this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
        this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.cbTurnOnTimeShift = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.textBoxTimeShiftBuffer = new MediaPortal.UserInterface.Controls.MPTextBox();
        this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
        this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.byIndexCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.cbTurnOnTv = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.gAllowedModes = new MediaPortal.UserInterface.Controls.MPGroupBox();
        this.cbAllowNormal = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.cbAllowZoom149 = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.cbAllowOriginal = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.cbAllowZoom = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.cbAllowLetterbox = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.cbAllowStretch = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.cbAllowPanScan = new MediaPortal.UserInterface.Controls.MPCheckBox();
        this.groupBox1.SuspendLayout();
        this.groupBox4.SuspendLayout();
        this.groupBox3.SuspendLayout();
        this.groupBox5.SuspendLayout();
        this.gAllowedModes.SuspendLayout();
        this.SuspendLayout();
        // 
        // groupBox1
        // 
        this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox1.Controls.Add(this.h264videoCodecComboBox);
        this.groupBox1.Controls.Add(this.mpLabel1);
        this.groupBox1.Controls.Add(this.cbDeinterlace);
        this.groupBox1.Controls.Add(this.label8);
        this.groupBox1.Controls.Add(this.defaultZoomModeComboBox);
        this.groupBox1.Controls.Add(this.label6);
        this.groupBox1.Controls.Add(this.videoCodecComboBox);
        this.groupBox1.Controls.Add(this.label5);
        this.groupBox1.Controls.Add(this.label3);
        this.groupBox1.Controls.Add(this.audioCodecComboBox);
        this.groupBox1.Controls.Add(this.labelAudioRenderer);
        this.groupBox1.Controls.Add(this.audioRendererComboBox);
        this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox1.Location = new System.Drawing.Point(0, 0);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(472, 185);
        this.groupBox1.TabIndex = 0;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Settings";
        // 
        // h264videoCodecComboBox
        // 
        this.h264videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.h264videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
        this.h264videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.h264videoCodecComboBox.Location = new System.Drawing.Point(168, 44);
        this.h264videoCodecComboBox.Name = "h264videoCodecComboBox";
        this.h264videoCodecComboBox.Size = new System.Drawing.Size(288, 21);
        this.h264videoCodecComboBox.TabIndex = 11;
        // 
        // mpLabel1
        // 
        this.mpLabel1.Location = new System.Drawing.Point(16, 48);
        this.mpLabel1.Name = "mpLabel1";
        this.mpLabel1.Size = new System.Drawing.Size(132, 17);
        this.mpLabel1.TabIndex = 10;
        this.mpLabel1.Text = "H.264 Video decoder";
        // 
        // cbDeinterlace
        // 
        this.cbDeinterlace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.cbDeinterlace.BorderColor = System.Drawing.Color.Empty;
        this.cbDeinterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cbDeinterlace.Items.AddRange(new object[] {
            "None",
            "Bob",
            "Weave",
            "Best"});
        this.cbDeinterlace.Location = new System.Drawing.Point(168, 116);
        this.cbDeinterlace.Name = "cbDeinterlace";
        this.cbDeinterlace.Size = new System.Drawing.Size(288, 21);
        this.cbDeinterlace.TabIndex = 7;
        // 
        // label8
        // 
        this.label8.Location = new System.Drawing.Point(16, 120);
        this.label8.Name = "label8";
        this.label8.Size = new System.Drawing.Size(96, 16);
        this.label8.TabIndex = 6;
        this.label8.Text = "Deinterlace mode:";
        // 
        // defaultZoomModeComboBox
        // 
        this.defaultZoomModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.defaultZoomModeComboBox.BorderColor = System.Drawing.Color.Empty;
        this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.defaultZoomModeComboBox.Items.AddRange(new object[] {
            "Normal",
            "Original Source Format",
            "Stretch",
            "Zoom",
            "Zoom 14:9",
            "4:3 Letterbox",
            "4:3 Pan and scan"});
        this.defaultZoomModeComboBox.Location = new System.Drawing.Point(168, 140);
        this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
        this.defaultZoomModeComboBox.Size = new System.Drawing.Size(288, 21);
        this.defaultZoomModeComboBox.TabIndex = 9;
        // 
        // label6
        // 
        this.label6.Location = new System.Drawing.Point(16, 144);
        this.label6.Name = "label6";
        this.label6.Size = new System.Drawing.Size(112, 16);
        this.label6.TabIndex = 8;
        this.label6.Text = "Default zoom mode:";
        // 
        // videoCodecComboBox
        // 
        this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
        this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.videoCodecComboBox.Location = new System.Drawing.Point(168, 20);
        this.videoCodecComboBox.Name = "videoCodecComboBox";
        this.videoCodecComboBox.Size = new System.Drawing.Size(288, 21);
        this.videoCodecComboBox.TabIndex = 1;
        // 
        // label5
        // 
        this.label5.Location = new System.Drawing.Point(16, 24);
        this.label5.Name = "label5";
        this.label5.Size = new System.Drawing.Size(132, 17);
        this.label5.TabIndex = 0;
        this.label5.Text = "MPEG-2 Video decoder";
        // 
        // label3
        // 
        this.label3.Location = new System.Drawing.Point(16, 72);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(88, 17);
        this.label3.TabIndex = 2;
        this.label3.Text = "Audio decoder";
        // 
        // audioCodecComboBox
        // 
        this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.audioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
        this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.audioCodecComboBox.Location = new System.Drawing.Point(168, 68);
        this.audioCodecComboBox.Name = "audioCodecComboBox";
        this.audioCodecComboBox.Size = new System.Drawing.Size(288, 21);
        this.audioCodecComboBox.TabIndex = 3;
        // 
        // labelAudioRenderer
        // 
        this.labelAudioRenderer.Location = new System.Drawing.Point(16, 96);
        this.labelAudioRenderer.Name = "labelAudioRenderer";
        this.labelAudioRenderer.Size = new System.Drawing.Size(88, 16);
        this.labelAudioRenderer.TabIndex = 4;
        this.labelAudioRenderer.Text = "Audio renderer:";
        // 
        // audioRendererComboBox
        // 
        this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
        this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.audioRendererComboBox.Location = new System.Drawing.Point(168, 92);
        this.audioRendererComboBox.Name = "audioRendererComboBox";
        this.audioRendererComboBox.Size = new System.Drawing.Size(288, 21);
        this.audioRendererComboBox.TabIndex = 5;
        // 
        // radioButton1
        // 
        this.radioButton1.AutoSize = true;
        this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.radioButton1.Location = new System.Drawing.Point(0, 0);
        this.radioButton1.Name = "radioButton1";
        this.radioButton1.Size = new System.Drawing.Size(104, 24);
        this.radioButton1.TabIndex = 0;
        this.radioButton1.UseVisualStyleBackColor = true;
        // 
        // groupBox4
        // 
        this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox4.Controls.Add(this.cbTurnOnTimeShift);
        this.groupBox4.Controls.Add(this.textBoxTimeShiftBuffer);
        this.groupBox4.Controls.Add(this.label7);
        this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox4.Location = new System.Drawing.Point(192, 244);
        this.groupBox4.Name = "groupBox4";
        this.groupBox4.Size = new System.Drawing.Size(280, 75);
        this.groupBox4.TabIndex = 3;
        this.groupBox4.TabStop = false;
        this.groupBox4.Text = "Timeshifting";
        // 
        // cbTurnOnTimeShift
        // 
        this.cbTurnOnTimeShift.AutoSize = true;
        this.cbTurnOnTimeShift.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.cbTurnOnTimeShift.Location = new System.Drawing.Point(16, 19);
        this.cbTurnOnTimeShift.Name = "cbTurnOnTimeShift";
        this.cbTurnOnTimeShift.Size = new System.Drawing.Size(230, 17);
        this.cbTurnOnTimeShift.TabIndex = 0;
        this.cbTurnOnTimeShift.Text = "Auto turn timeshift on when entering My TV ";
        this.cbTurnOnTimeShift.UseVisualStyleBackColor = true;
        this.cbTurnOnTimeShift.CheckedChanged += new System.EventHandler(this.cbTurnOnTimeShift_CheckedChanged);
        // 
        // textBoxTimeShiftBuffer
        // 
        this.textBoxTimeShiftBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.textBoxTimeShiftBuffer.BorderColor = System.Drawing.Color.Empty;
        this.textBoxTimeShiftBuffer.Location = new System.Drawing.Point(168, 43);
        this.textBoxTimeShiftBuffer.Name = "textBoxTimeShiftBuffer";
        this.textBoxTimeShiftBuffer.Size = new System.Drawing.Size(71, 20);
        this.textBoxTimeShiftBuffer.TabIndex = 2;
        this.textBoxTimeShiftBuffer.Text = "30";
        this.textBoxTimeShiftBuffer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        // 
        // label7
        // 
        this.label7.Location = new System.Drawing.Point(16, 46);
        this.label7.Name = "label7";
        this.label7.Size = new System.Drawing.Size(136, 16);
        this.label7.TabIndex = 1;
        this.label7.Text = "Timeshift buffer (minutes):";
        // 
        // groupBox3
        // 
        this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox3.Controls.Add(this.byIndexCheckBox);
        this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox3.Location = new System.Drawing.Point(192, 325);
        this.groupBox3.Name = "groupBox3";
        this.groupBox3.Size = new System.Drawing.Size(280, 53);
        this.groupBox3.TabIndex = 4;
        this.groupBox3.TabStop = false;
        this.groupBox3.Text = "Misc";
        // 
        // byIndexCheckBox
        // 
        this.byIndexCheckBox.AutoSize = true;
        this.byIndexCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.byIndexCheckBox.Location = new System.Drawing.Point(16, 21);
        this.byIndexCheckBox.Name = "byIndexCheckBox";
        this.byIndexCheckBox.Size = new System.Drawing.Size(182, 17);
        this.byIndexCheckBox.TabIndex = 0;
        this.byIndexCheckBox.Text = "Select channel by index (non-US)";
        this.byIndexCheckBox.UseVisualStyleBackColor = true;
        // 
        // groupBox5
        // 
        this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBox5.Controls.Add(this.cbTurnOnTv);
        this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.groupBox5.Location = new System.Drawing.Point(192, 191);
        this.groupBox5.Name = "groupBox5";
        this.groupBox5.Size = new System.Drawing.Size(280, 47);
        this.groupBox5.TabIndex = 2;
        this.groupBox5.TabStop = false;
        this.groupBox5.Text = "Auto on";
        // 
        // cbTurnOnTv
        // 
        this.cbTurnOnTv.AutoSize = true;
        this.cbTurnOnTv.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.cbTurnOnTv.Location = new System.Drawing.Point(16, 19);
        this.cbTurnOnTv.Name = "cbTurnOnTv";
        this.cbTurnOnTv.Size = new System.Drawing.Size(206, 17);
        this.cbTurnOnTv.TabIndex = 0;
        this.cbTurnOnTv.Text = "Auto turn TV on when entering My TV ";
        this.cbTurnOnTv.UseVisualStyleBackColor = true;
        // 
        // gAllowedModes
        // 
        this.gAllowedModes.Controls.Add(this.cbAllowNormal);
        this.gAllowedModes.Controls.Add(this.cbAllowZoom149);
        this.gAllowedModes.Controls.Add(this.cbAllowOriginal);
        this.gAllowedModes.Controls.Add(this.cbAllowZoom);
        this.gAllowedModes.Controls.Add(this.cbAllowLetterbox);
        this.gAllowedModes.Controls.Add(this.cbAllowStretch);
        this.gAllowedModes.Controls.Add(this.cbAllowPanScan);
        this.gAllowedModes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.gAllowedModes.Location = new System.Drawing.Point(0, 191);
        this.gAllowedModes.Name = "gAllowedModes";
        this.gAllowedModes.Size = new System.Drawing.Size(186, 187);
        this.gAllowedModes.TabIndex = 1;
        this.gAllowedModes.TabStop = false;
        this.gAllowedModes.Text = "Allowed Zoom Modes";
        // 
        // cbAllowNormal
        // 
        this.cbAllowNormal.AutoSize = true;
        this.cbAllowNormal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.cbAllowNormal.Location = new System.Drawing.Point(17, 24);
        this.cbAllowNormal.Name = "cbAllowNormal";
        this.cbAllowNormal.Size = new System.Drawing.Size(57, 17);
        this.cbAllowNormal.TabIndex = 0;
        this.cbAllowNormal.Text = "Normal";
        this.cbAllowNormal.UseVisualStyleBackColor = true;
        // 
        // cbAllowZoom149
        // 
        this.cbAllowZoom149.AutoSize = true;
        this.cbAllowZoom149.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.cbAllowZoom149.Location = new System.Drawing.Point(17, 93);
        this.cbAllowZoom149.Name = "cbAllowZoom149";
        this.cbAllowZoom149.Size = new System.Drawing.Size(75, 17);
        this.cbAllowZoom149.TabIndex = 3;
        this.cbAllowZoom149.Text = "14:9 Zoom";
        this.cbAllowZoom149.UseVisualStyleBackColor = true;
        // 
        // cbAllowOriginal
        // 
        this.cbAllowOriginal.AutoSize = true;
        this.cbAllowOriginal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.cbAllowOriginal.Location = new System.Drawing.Point(17, 47);
        this.cbAllowOriginal.Name = "cbAllowOriginal";
        this.cbAllowOriginal.Size = new System.Drawing.Size(131, 17);
        this.cbAllowOriginal.TabIndex = 1;
        this.cbAllowOriginal.Text = "Original Source Format";
        this.cbAllowOriginal.UseVisualStyleBackColor = true;
        // 
        // cbAllowZoom
        // 
        this.cbAllowZoom.AutoSize = true;
        this.cbAllowZoom.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.cbAllowZoom.Location = new System.Drawing.Point(17, 70);
        this.cbAllowZoom.Name = "cbAllowZoom";
        this.cbAllowZoom.Size = new System.Drawing.Size(51, 17);
        this.cbAllowZoom.TabIndex = 2;
        this.cbAllowZoom.Text = "Zoom";
        this.cbAllowZoom.UseVisualStyleBackColor = true;
        // 
        // cbAllowLetterbox
        // 
        this.cbAllowLetterbox.AutoSize = true;
        this.cbAllowLetterbox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.cbAllowLetterbox.Location = new System.Drawing.Point(17, 162);
        this.cbAllowLetterbox.Name = "cbAllowLetterbox";
        this.cbAllowLetterbox.Size = new System.Drawing.Size(86, 17);
        this.cbAllowLetterbox.TabIndex = 6;
        this.cbAllowLetterbox.Text = "4:3 Letterbox";
        this.cbAllowLetterbox.UseVisualStyleBackColor = true;
        // 
        // cbAllowStretch
        // 
        this.cbAllowStretch.AutoSize = true;
        this.cbAllowStretch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.cbAllowStretch.Location = new System.Drawing.Point(17, 116);
        this.cbAllowStretch.Name = "cbAllowStretch";
        this.cbAllowStretch.Size = new System.Drawing.Size(58, 17);
        this.cbAllowStretch.TabIndex = 4;
        this.cbAllowStretch.Text = "Stretch";
        this.cbAllowStretch.UseVisualStyleBackColor = true;
        // 
        // cbAllowPanScan
        // 
        this.cbAllowPanScan.AutoSize = true;
        this.cbAllowPanScan.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.cbAllowPanScan.Location = new System.Drawing.Point(17, 139);
        this.cbAllowPanScan.Name = "cbAllowPanScan";
        this.cbAllowPanScan.Size = new System.Drawing.Size(110, 17);
        this.cbAllowPanScan.TabIndex = 5;
        this.cbAllowPanScan.Text = "4:3 Pan and Scan";
        this.cbAllowPanScan.UseVisualStyleBackColor = true;
        // 
        // Television
        // 
        this.Controls.Add(this.gAllowedModes);
        this.Controls.Add(this.groupBox5);
        this.Controls.Add(this.groupBox3);
        this.Controls.Add(this.groupBox4);
        this.Controls.Add(this.groupBox1);
        this.Name = "Television";
        this.Size = new System.Drawing.Size(472, 408);
        this.groupBox1.ResumeLayout(false);
        this.groupBox4.ResumeLayout(false);
        this.groupBox4.PerformLayout();
        this.groupBox3.ResumeLayout(false);
        this.groupBox3.PerformLayout();
        this.groupBox5.ResumeLayout(false);
        this.groupBox5.PerformLayout();
        this.gAllowedModes.ResumeLayout(false);
        this.gAllowedModes.PerformLayout();
        this.ResumeLayout(false);

    }
    #endregion

    public override void LoadSettings()
    {
      if (_init == false) return;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        cbAllowNormal.Checked = xmlreader.GetValueAsBool("mytv", "allowarnormal", true);
        cbAllowOriginal.Checked = xmlreader.GetValueAsBool("mytv", "allowaroriginal", true);
        cbAllowStretch.Checked = xmlreader.GetValueAsBool("mytv", "allowarstretch", true);
        cbAllowZoom.Checked = xmlreader.GetValueAsBool("mytv", "allowarzoom", true);
        cbAllowZoom149.Checked = xmlreader.GetValueAsBool("mytv", "allowarzoom149", true);
        cbAllowLetterbox.Checked = xmlreader.GetValueAsBool("mytv", "allowarletterbox", true);
        cbAllowPanScan.Checked = xmlreader.GetValueAsBool("mytv", "allowarpanscan", true);

        cbTurnOnTv.Checked = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
        cbTurnOnTimeShift.Checked = xmlreader.GetValueAsBool("mytv", "autoturnontimeshifting", false);

        textBoxTimeShiftBuffer.Text = xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30).ToString();
        byIndexCheckBox.Checked = xmlreader.GetValueAsBool("mytv", "byindex", true);
        int DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 0);
        if (DeInterlaceMode < 0 || DeInterlaceMode > 3)
          DeInterlaceMode = 3;
        cbDeinterlace.SelectedIndex = DeInterlaceMode;
        //
        // Set codecs
        //
        string audioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
        string videoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
        string h264videoCodec = xmlreader.GetValueAsString("mytv", "h264videocodec", "");
        string audioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "Default DirectSound Device");

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

        if (h264videoCodec == String.Empty)
        {
          ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264Video);
          bool h264DecFilterFound = true;
          if (availableH264VideoFilters.Count > 0)
          {
            h264videoCodec = (string)availableH264VideoFilters[0];
            foreach (string filter in availableH264VideoFilters)
            {
              if (filter.Equals("CoreAVC Video Decoder"))
              {
                h264DecFilterFound = true;
              }
            }
            if (h264DecFilterFound) h264videoCodec = "CoreAVC Video Decoder";
          }
        }
        audioCodecComboBox.SelectedItem = audioCodec;
        videoCodecComboBox.SelectedItem = videoCodec;
        h264videoCodecComboBox.SelectedItem = h264videoCodec;
        audioRendererComboBox.SelectedItem = audioRenderer;

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        for (int index = 0; index < aspectRatio.Length; index++)
        {
          if (aspectRatio[index].Equals(defaultAspectRatio))
          {
            defaultZoomModeComboBox.SelectedIndex = index;
            break;
          }
        }
      }
    }


    public override void SaveSettings()
    {
      if (_init == false) return;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        if (cbDeinterlace.SelectedIndex >= 0)
          xmlwriter.SetValue("mytv", "deinterlace", cbDeinterlace.SelectedIndex.ToString());

        xmlwriter.SetValueAsBool("mytv", "autoturnontv", cbTurnOnTv.Checked);
        xmlwriter.SetValueAsBool("mytv", "autoturnontimeshifting", cbTurnOnTimeShift.Checked);
        xmlwriter.SetValueAsBool("mytv", "byindex", byIndexCheckBox.Checked);

        try
        {
          int buffer = Int32.Parse(textBoxTimeShiftBuffer.Text);
          xmlwriter.SetValue("capture", "timeshiftbuffer", buffer.ToString());
        }
        catch (Exception) { }
        xmlwriter.SetValue("mytv", "defaultar", aspectRatio[defaultZoomModeComboBox.SelectedIndex]);
        //
        // Set codecs
        //
        xmlwriter.SetValue("mytv", "audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("mytv", "videocodec", videoCodecComboBox.Text);
        xmlwriter.SetValue("mytv", "h264videocodec", h264videoCodecComboBox.Text);
        xmlwriter.SetValue("mytv", "audiorenderer", audioRendererComboBox.Text);

        xmlwriter.SetValueAsBool("mytv", "allowarnormal", cbAllowNormal.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowaroriginal", cbAllowOriginal.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarstretch", cbAllowStretch.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarzoom", cbAllowZoom.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarzoom149", cbAllowZoom149.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarletterbox", cbAllowLetterbox.Checked);
        xmlwriter.SetValueAsBool("mytv", "allowarpanscan", cbAllowPanScan.Checked);
      }
    }

    private void cbTurnOnTimeShift_CheckedChanged(object sender, EventArgs e)
    {
      //if ( cbTurnOnTimeShift.Checked )
      //  cbTurnOnTv.Checked = true;
    }

    private void cbTurnOnTv_CheckedChanged(object sender, EventArgs e)
    {
      //if ( !cbTurnOnTv.Checked )
      //  cbTurnOnTimeShift.Checked = false;
    }
  }
}