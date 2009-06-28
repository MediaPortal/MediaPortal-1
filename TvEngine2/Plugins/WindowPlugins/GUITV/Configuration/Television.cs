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

using System;
using System.Collections;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.TVE2.Sections
{
  public class Television : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPRadioButton radioButton1;
    private MPComboBox audioCodecComboBox;
    private MPLabel labelAudioRenderer;
    private MPComboBox audioRendererComboBox;
    private MPLabel labelAudioDecoder;
    private MPComboBox videoCodecComboBox;
    private MPLabel labelMPEG2Decoder;
    private MPComboBox defaultZoomModeComboBox;
    private MPLabel label6;
    private MPGroupBox groupBox4;
    private MPLabel label7;
    private MPTextBox textBoxTimeShiftBuffer;
    private MPLabel label8;
    private MPComboBox cbDeinterlace;
    private MPGroupBox groupBox3;
    private MPCheckBox byIndexCheckBox;
    private MPCheckBox showChannelNumberCheckBox;
    private Label lblChanNumMaxLen;
    private NumericUpDown channelNumberMaxLengthNumUpDn;
    private bool _init;
    private MPCheckBox cbTurnOnTimeShift;
    private MPGroupBox groupBox5;
    private MPCheckBox cbTurnOnTv;
    private MPGroupBox gAllowedModes;
    private MPCheckBox cbAllowNormal;
    private MPCheckBox cbAllowZoom149;
    private MPCheckBox cbAllowOriginal;
    private MPCheckBox cbAllowZoom;
    private MPCheckBox cbAllowLetterbox;
    private MPCheckBox cbAllowStretch;
    private MPCheckBox cbAllowNonLinearStretch;
    private MPCheckBox cbAutoFullscreen;
    public int pluginVersion;

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
        //Remove Muxer's from the Video decoder list to avoid confusion.
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
        ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
        availableH264VideoFilters.Sort();
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
        ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.LATMAAC);
        availableAACAudioFilters.Sort();
        ArrayList availableAudioRenderers = FilterHelper.GetAudioRenderers();
        availableAudioRenderers.Sort();
        videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
        audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
        audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
        _init = true;
        //
        // Load all available aspect ratio
        //
        defaultZoomModeComboBox.Items.Clear();
        foreach (Geometry.Type item in Enum.GetValues(typeof(Geometry.Type)))
        {
          defaultZoomModeComboBox.Items.Add(Util.Utils.GetAspectRatio(item));
        }
        //
        // Change aspect ratio labels to the current core proj description
        //
        cbAllowNormal.Text = Util.Utils.GetAspectRatio(Geometry.Type.Normal);
        cbAllowOriginal.Text = Util.Utils.GetAspectRatio(Geometry.Type.Original);
        cbAllowZoom.Text = Util.Utils.GetAspectRatio(Geometry.Type.Zoom);
        cbAllowZoom149.Text = Util.Utils.GetAspectRatio(Geometry.Type.Zoom14to9);
        cbAllowStretch.Text = Util.Utils.GetAspectRatio(Geometry.Type.Stretch);
        cbAllowNonLinearStretch.Text = Util.Utils.GetAspectRatio(Geometry.Type.NonLinearStretch);
        cbAllowLetterbox.Text = Util.Utils.GetAspectRatio(Geometry.Type.LetterBox43);
        LoadSettings();
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.audioCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbDeinterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.defaultZoomModeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.videoCodecComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelMPEG2Decoder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAudioDecoder = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelAudioRenderer = new MediaPortal.UserInterface.Controls.MPLabel();
      this.audioRendererComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.radioButton1 = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbTurnOnTimeShift = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.textBoxTimeShiftBuffer = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.byIndexCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.showChannelNumberCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.channelNumberMaxLengthNumUpDn = new System.Windows.Forms.NumericUpDown();
      this.lblChanNumMaxLen = new System.Windows.Forms.Label();
      this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbAutoFullscreen = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbTurnOnTv = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.gAllowedModes = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbAllowNormal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowZoom149 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowOriginal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowZoom = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowLetterbox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowStretch = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbAllowNonLinearStretch = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberMaxLengthNumUpDn)).BeginInit();
      this.groupBox5.SuspendLayout();
      this.gAllowedModes.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.audioCodecComboBox);
      this.groupBox1.Controls.Add(this.cbDeinterlace);
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.defaultZoomModeComboBox);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.videoCodecComboBox);
      this.groupBox1.Controls.Add(this.labelMPEG2Decoder);
      this.groupBox1.Controls.Add(this.labelAudioDecoder);
      this.groupBox1.Controls.Add(this.labelAudioRenderer);
      this.groupBox1.Controls.Add(this.audioRendererComboBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 168);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 43);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioCodecComboBox.TabIndex = 5;
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
      this.cbDeinterlace.Location = new System.Drawing.Point(168, 97);
      this.cbDeinterlace.Name = "cbDeinterlace";
      this.cbDeinterlace.Size = new System.Drawing.Size(288, 21);
      this.cbDeinterlace.TabIndex = 9;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(16, 100);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(146, 17);
      this.label8.TabIndex = 8;
      this.label8.Text = "Fallback de-interlace mode:";
      // 
      // defaultZoomModeComboBox
      // 
      this.defaultZoomModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultZoomModeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultZoomModeComboBox.Location = new System.Drawing.Point(168, 124);
      this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
      this.defaultZoomModeComboBox.Size = new System.Drawing.Size(288, 21);
      this.defaultZoomModeComboBox.TabIndex = 11;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 127);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(112, 16);
      this.label6.TabIndex = 10;
      this.label6.Text = "Default zoom mode:";
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.BorderColor = System.Drawing.Color.Empty;
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 16);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.videoCodecComboBox.TabIndex = 1;
      // 
      // labelMPEG2Decoder
      // 
      this.labelMPEG2Decoder.Location = new System.Drawing.Point(16, 20);
      this.labelMPEG2Decoder.Name = "labelMPEG2Decoder";
      this.labelMPEG2Decoder.Size = new System.Drawing.Size(132, 17);
      this.labelMPEG2Decoder.TabIndex = 0;
      this.labelMPEG2Decoder.Text = "MPEG-2 Video decoder:";
      // 
      // labelAudioDecoder
      // 
      this.labelAudioDecoder.Location = new System.Drawing.Point(16, 46);
      this.labelAudioDecoder.Name = "labelAudioDecoder";
      this.labelAudioDecoder.Size = new System.Drawing.Size(157, 18);
      this.labelAudioDecoder.TabIndex = 4;
      this.labelAudioDecoder.Text = "MPEG / AC3 audio decoder:";
      // 
      // labelAudioRenderer
      // 
      this.labelAudioRenderer.Location = new System.Drawing.Point(16, 73);
      this.labelAudioRenderer.Name = "labelAudioRenderer";
      this.labelAudioRenderer.Size = new System.Drawing.Size(88, 16);
      this.labelAudioRenderer.TabIndex = 6;
      this.labelAudioRenderer.Text = "Audio renderer:";
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.BorderColor = System.Drawing.Color.Empty;
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(168, 70);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioRendererComboBox.TabIndex = 7;
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
      this.groupBox4.Location = new System.Drawing.Point(192, 341);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(277, 71);
      this.groupBox4.TabIndex = 2;
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
      this.textBoxTimeShiftBuffer.Size = new System.Drawing.Size(68, 20);
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
      this.groupBox3.Controls.Add(this.showChannelNumberCheckBox);
      this.groupBox3.Controls.Add(this.channelNumberMaxLengthNumUpDn);
      this.groupBox3.Controls.Add(this.lblChanNumMaxLen);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(192, 191);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(277, 94);
      this.groupBox3.TabIndex = 3;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Channel numbers";
      // 
      // byIndexCheckBox
      // 
      this.byIndexCheckBox.AutoSize = true;
      this.byIndexCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.byIndexCheckBox.Location = new System.Drawing.Point(17, 20);
      this.byIndexCheckBox.Name = "byIndexCheckBox";
      this.byIndexCheckBox.Size = new System.Drawing.Size(182, 17);
      this.byIndexCheckBox.TabIndex = 0;
      this.byIndexCheckBox.Text = "Select channel by index (non-US)";
      this.byIndexCheckBox.UseVisualStyleBackColor = true;
      // 
      // showChannelNumberCheckBox
      // 
      this.showChannelNumberCheckBox.AutoSize = true;
      this.showChannelNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.showChannelNumberCheckBox.Location = new System.Drawing.Point(17, 43);
      this.showChannelNumberCheckBox.Name = "showChannelNumberCheckBox";
      this.showChannelNumberCheckBox.Size = new System.Drawing.Size(135, 17);
      this.showChannelNumberCheckBox.TabIndex = 1;
      this.showChannelNumberCheckBox.Text = "Show channel numbers";
      this.showChannelNumberCheckBox.UseVisualStyleBackColor = true;
      // 
      // channelNumberMaxLengthNumUpDn
      // 
      this.channelNumberMaxLengthNumUpDn.AutoSize = true;
      this.channelNumberMaxLengthNumUpDn.Location = new System.Drawing.Point(178, 66);
      this.channelNumberMaxLengthNumUpDn.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.channelNumberMaxLengthNumUpDn.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.channelNumberMaxLengthNumUpDn.Name = "channelNumberMaxLengthNumUpDn";
      this.channelNumberMaxLengthNumUpDn.Size = new System.Drawing.Size(42, 20);
      this.channelNumberMaxLengthNumUpDn.TabIndex = 3;
      this.channelNumberMaxLengthNumUpDn.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
      // 
      // lblChanNumMaxLen
      // 
      this.lblChanNumMaxLen.AutoSize = true;
      this.lblChanNumMaxLen.Location = new System.Drawing.Point(31, 68);
      this.lblChanNumMaxLen.Name = "lblChanNumMaxLen";
      this.lblChanNumMaxLen.Size = new System.Drawing.Size(141, 13);
      this.lblChanNumMaxLen.TabIndex = 2;
      this.lblChanNumMaxLen.Text = "Channel number max. length";
      // 
      // groupBox5
      // 
      this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.cbAutoFullscreen);
      this.groupBox5.Controls.Add(this.cbTurnOnTv);
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox5.Location = new System.Drawing.Point(192, 291);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(277, 44);
      this.groupBox5.TabIndex = 4;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "When entering the TV screen:";
      // 
      // cbAutoFullscreen
      // 
      this.cbAutoFullscreen.AutoSize = true;
      this.cbAutoFullscreen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAutoFullscreen.Location = new System.Drawing.Point(98, 19);
      this.cbAutoFullscreen.Name = "cbAutoFullscreen";
      this.cbAutoFullscreen.Size = new System.Drawing.Size(152, 17);
      this.cbAutoFullscreen.TabIndex = 1;
      this.cbAutoFullscreen.Text = "Directly show fullscreen TV";
      this.cbAutoFullscreen.UseVisualStyleBackColor = true;
      this.cbAutoFullscreen.CheckedChanged += new System.EventHandler(this.cbAutoFullscreen_CheckedChanged);
      // 
      // cbTurnOnTv
      // 
      this.cbTurnOnTv.AutoSize = true;
      this.cbTurnOnTv.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbTurnOnTv.Location = new System.Drawing.Point(6, 19);
      this.cbTurnOnTv.Name = "cbTurnOnTv";
      this.cbTurnOnTv.Size = new System.Drawing.Size(78, 17);
      this.cbTurnOnTv.TabIndex = 0;
      this.cbTurnOnTv.Text = "Turn on TV";
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
      this.gAllowedModes.Controls.Add(this.cbAllowNonLinearStretch);
      this.gAllowedModes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gAllowedModes.Location = new System.Drawing.Point(0, 189);
      this.gAllowedModes.Name = "gAllowedModes";
      this.gAllowedModes.Size = new System.Drawing.Size(186, 187);
      this.gAllowedModes.TabIndex = 1;
      this.gAllowedModes.TabStop = false;
      this.gAllowedModes.Text = "Allowed zoom modes";
      // 
      // cbAllowNormal
      // 
      this.cbAllowNormal.AutoSize = true;
      this.cbAllowNormal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowNormal.Location = new System.Drawing.Point(17, 24);
      this.cbAllowNormal.Name = "cbAllowNormal";
      this.cbAllowNormal.Size = new System.Drawing.Size(151, 17);
      this.cbAllowNormal.TabIndex = 0;
      this.cbAllowNormal.Text = "Normal (aspect auto mode)";
      this.cbAllowNormal.UseVisualStyleBackColor = true;
      // 
      // cbAllowZoom149
      // 
      this.cbAllowZoom149.AutoSize = true;
      this.cbAllowZoom149.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowZoom149.Location = new System.Drawing.Point(17, 93);
      this.cbAllowZoom149.Name = "cbAllowZoom149";
      this.cbAllowZoom149.Size = new System.Drawing.Size(73, 17);
      this.cbAllowZoom149.TabIndex = 3;
      this.cbAllowZoom149.Text = "14:9 zoom";
      this.cbAllowZoom149.UseVisualStyleBackColor = true;
      // 
      // cbAllowOriginal
      // 
      this.cbAllowOriginal.AutoSize = true;
      this.cbAllowOriginal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowOriginal.Location = new System.Drawing.Point(17, 47);
      this.cbAllowOriginal.Name = "cbAllowOriginal";
      this.cbAllowOriginal.Size = new System.Drawing.Size(126, 17);
      this.cbAllowOriginal.TabIndex = 1;
      this.cbAllowOriginal.Text = "Original source format";
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
      this.cbAllowStretch.Size = new System.Drawing.Size(107, 17);
      this.cbAllowStretch.TabIndex = 4;
      this.cbAllowStretch.Text = "Fullscreen stretch";
      this.cbAllowStretch.UseVisualStyleBackColor = true;
      // 
      // cbAllowNonLinearStretch
      // 
      this.cbAllowNonLinearStretch.AutoSize = true;
      this.cbAllowNonLinearStretch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAllowNonLinearStretch.Location = new System.Drawing.Point(17, 139);
      this.cbAllowNonLinearStretch.Name = "cbAllowNonLinearStretch";
      this.cbAllowNonLinearStretch.Size = new System.Drawing.Size(140, 17);
      this.cbAllowNonLinearStretch.TabIndex = 5;
      this.cbAllowNonLinearStretch.Text = "Non-linear stretch && crop";
      this.cbAllowNonLinearStretch.UseVisualStyleBackColor = true;
      // 
      // Television
      // 
      this.Controls.Add(this.gAllowedModes);
      this.Controls.Add(this.groupBox5);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox4);
      this.Controls.Add(this.groupBox1);
      this.Name = "Television";
      this.Size = new System.Drawing.Size(472, 427);
      this.groupBox1.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberMaxLengthNumUpDn)).EndInit();
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.gAllowedModes.ResumeLayout(false);
      this.gAllowedModes.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlreader = new MPSettings())
      {
        cbAllowNormal.Checked = xmlreader.GetValueAsBool("mytve2", "allowarnormal", true);
        cbAllowOriginal.Checked = xmlreader.GetValueAsBool("mytve2", "allowaroriginal", true);
        cbAllowZoom.Checked = xmlreader.GetValueAsBool("mytve2", "allowarzoom", true);
        cbAllowZoom149.Checked = xmlreader.GetValueAsBool("mytve2", "allowarzoom149", true);
        cbAllowStretch.Checked = xmlreader.GetValueAsBool("mytve2", "allowarstretch", true);
        cbAllowNonLinearStretch.Checked = xmlreader.GetValueAsBool("mytve2", "allowarnonlinear", true);
        cbAllowLetterbox.Checked = xmlreader.GetValueAsBool("mytve2", "allowarletterbox", true);

        cbTurnOnTv.Checked = xmlreader.GetValueAsBool("mytve2", "autoturnontv", false);
        cbAutoFullscreen.Checked = xmlreader.GetValueAsBool("mytve2", "autofullscreen", false);
        cbTurnOnTimeShift.Checked = xmlreader.GetValueAsBool("mytve2", "autoturnontimeshifting", false);

        textBoxTimeShiftBuffer.Text = xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30).ToString();
        byIndexCheckBox.Checked = xmlreader.GetValueAsBool("mytve2", "byindex", true);
        showChannelNumberCheckBox.Checked = xmlreader.GetValueAsBool("mytve2", "showchannelnumber", false);

        int channelNumberMaxLen = xmlreader.GetValueAsInt("mytve2", "channelnumbermaxlength", 3);
        channelNumberMaxLengthNumUpDn.Value = channelNumberMaxLen;


        int DeInterlaceMode = xmlreader.GetValueAsInt("mytve2", "deinterlace", 0);
        if (DeInterlaceMode < 0 || DeInterlaceMode > 3)
        {
          DeInterlaceMode = 3;
        }
        cbDeinterlace.SelectedIndex = DeInterlaceMode;
        //
        // Set codecs
        //
        string audioCodec = xmlreader.GetValueAsString("mytve2", "audiocodec", "");
        string videoCodec = xmlreader.GetValueAsString("mytve2", "videocodec", "");
        string audioRenderer = xmlreader.GetValueAsString("mytve2", "audiorenderer", "Default DirectSound Device");

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
        audioRendererComboBox.Text = audioRenderer;

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("mytve2", "defaultar", defaultZoomModeComboBox.Items[0].ToString());
        foreach (Geometry.Type item in Enum.GetValues(typeof(Geometry.Type)))
        {
          string currentAspectRatio = Util.Utils.GetAspectRatio(item);
          if (defaultAspectRatio == currentAspectRatio)
          {
            defaultZoomModeComboBox.SelectedItem = currentAspectRatio;
            break;
          }
        }
      }
    }


    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        if (cbDeinterlace.SelectedIndex >= 0)
        {
          xmlwriter.SetValue("mytve2", "deinterlace", cbDeinterlace.SelectedIndex.ToString());
        }

        xmlwriter.SetValueAsBool("mytve2", "autoturnontv", cbTurnOnTv.Checked);
        xmlwriter.SetValueAsBool("mytve2", "autofullscreen", cbAutoFullscreen.Checked);
        xmlwriter.SetValueAsBool("mytve2", "autoturnontimeshifting", cbTurnOnTimeShift.Checked);
        xmlwriter.SetValueAsBool("mytve2", "byindex", byIndexCheckBox.Checked);
        xmlwriter.SetValueAsBool("mytve2", "showchannelnumber", showChannelNumberCheckBox.Checked);
        xmlwriter.SetValue("mytve2", "channelnumbermaxlength", channelNumberMaxLengthNumUpDn.Value);

        try
        {
          int buffer = Int32.Parse(textBoxTimeShiftBuffer.Text);
          xmlwriter.SetValue("capture", "timeshiftbuffer", buffer.ToString());
        }
        catch
        {
        }
        xmlwriter.SetValue("mytve2", "defaultar", defaultZoomModeComboBox.SelectedItem);
        //
        // Set codecs
        //
        xmlwriter.SetValue("mytve2", "audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("mytve2", "videocodec", videoCodecComboBox.Text);
        xmlwriter.SetValue("mytve2", "audiorenderer", audioRendererComboBox.Text);

        xmlwriter.SetValueAsBool("mytve2", "allowarnormal", cbAllowNormal.Checked);
        xmlwriter.SetValueAsBool("mytve2", "allowaroriginal", cbAllowOriginal.Checked);
        xmlwriter.SetValueAsBool("mytve2", "allowarzoom", cbAllowZoom.Checked);
        xmlwriter.SetValueAsBool("mytve2", "allowarzoom149", cbAllowZoom149.Checked);
        xmlwriter.SetValueAsBool("mytve2", "allowarstretch", cbAllowStretch.Checked);
        xmlwriter.SetValueAsBool("mytve2", "allowarnonlinear", cbAllowNonLinearStretch.Checked);
        xmlwriter.SetValueAsBool("mytve2", "allowarletterbox", cbAllowLetterbox.Checked);
      }
    }

    private void cbTurnOnTimeShift_CheckedChanged(object sender, EventArgs e)
    {
      //if ( cbTurnOnTimeShift.Checked )
      //  cbTurnOnTv.Checked = true;
    }

    private void cbAutoFullscreen_CheckedChanged(object sender, EventArgs e)
    {
      if (cbAutoFullscreen.Checked)
      {
        cbTurnOnTv.Checked = true;
      }
    }
  }
}
