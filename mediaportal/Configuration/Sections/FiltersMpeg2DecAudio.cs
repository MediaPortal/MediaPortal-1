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
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using DShowNET;
using DirectShowLib;
using MediaPortal.GUI.Library;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class FiltersMPEG2DecAudio : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonPcm16Bit;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonPcm24Bit;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonPcm32Bit;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonIeee;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxNormalize;
    private System.Windows.Forms.TrackBar trackBarBoost;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonAc3Speakers;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonAc3Spdif;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAc3DynamicRange;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxAc3SpeakerConfig;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAc3Lfe;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDtsSpeakerConfig;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDtsDynamicRange;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonDtsSpdif;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonDtsSpeakers;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDtsLfe;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAacDownmix;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxDtsDecoderSettings;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxFormat;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxAc3DecoderSettings;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxAacDecoderSettings;
    private MediaPortal.UserInterface.Controls.MPLabel labelBoost;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAacDynamic;
    private Label labelBoostValue;
    private System.ComponentModel.IContainer components = null;


    /// <summary>
    /// 
    /// </summary>
    public FiltersMPEG2DecAudio()
      : this("MPA Decoder")
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public FiltersMPEG2DecAudio(string name)
      : base(name)
    {

      // This call is required by the Windows Form Designer.
      InitializeComponent();
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
      this.groupBoxAc3DecoderSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonAc3Spdif = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonAc3Speakers = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.checkBoxAc3Lfe = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.comboBoxAc3SpeakerConfig = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxAc3DynamicRange = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxFormat = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelBoostValue = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labelBoost = new MediaPortal.UserInterface.Controls.MPLabel();
      this.radioButtonPcm16Bit = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPcm24Bit = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPcm32Bit = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonIeee = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.checkBoxNormalize = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.trackBarBoost = new System.Windows.Forms.TrackBar();
      this.groupBoxDtsDecoderSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonDtsSpdif = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonDtsSpeakers = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.checkBoxDtsLfe = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.comboBoxDtsSpeakerConfig = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxDtsDynamicRange = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxAacDecoderSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxAacDynamic = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxAacDownmix = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxAc3DecoderSettings.SuspendLayout();
      this.groupBoxFormat.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarBoost)).BeginInit();
      this.groupBoxDtsDecoderSettings.SuspendLayout();
      this.groupBoxAacDecoderSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxAc3DecoderSettings
      // 
      this.groupBoxAc3DecoderSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAc3DecoderSettings.Controls.Add(this.radioButtonAc3Spdif);
      this.groupBoxAc3DecoderSettings.Controls.Add(this.radioButtonAc3Speakers);
      this.groupBoxAc3DecoderSettings.Controls.Add(this.checkBoxAc3Lfe);
      this.groupBoxAc3DecoderSettings.Controls.Add(this.comboBoxAc3SpeakerConfig);
      this.groupBoxAc3DecoderSettings.Controls.Add(this.checkBoxAc3DynamicRange);
      this.groupBoxAc3DecoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAc3DecoderSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBoxAc3DecoderSettings.Location = new System.Drawing.Point(0, 0);
      this.groupBoxAc3DecoderSettings.Name = "groupBoxAc3DecoderSettings";
      this.groupBoxAc3DecoderSettings.Size = new System.Drawing.Size(472, 104);
      this.groupBoxAc3DecoderSettings.TabIndex = 0;
      this.groupBoxAc3DecoderSettings.TabStop = false;
      this.groupBoxAc3DecoderSettings.Text = "AC3 Decoder Settings";
      // 
      // radioButtonAc3Spdif
      // 
      this.radioButtonAc3Spdif.AutoSize = true;
      this.radioButtonAc3Spdif.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonAc3Spdif.Location = new System.Drawing.Point(16, 48);
      this.radioButtonAc3Spdif.Name = "radioButtonAc3Spdif";
      this.radioButtonAc3Spdif.Size = new System.Drawing.Size(60, 17);
      this.radioButtonAc3Spdif.TabIndex = 3;
      this.radioButtonAc3Spdif.Text = "S/PDIF";
      this.radioButtonAc3Spdif.UseVisualStyleBackColor = true;
      this.radioButtonAc3Spdif.CheckedChanged += new System.EventHandler(this.radioButtonAC3SPDIF_CheckedChanged);
      // 
      // radioButtonAc3Speakers
      // 
      this.radioButtonAc3Speakers.AutoSize = true;
      this.radioButtonAc3Speakers.Checked = true;
      this.radioButtonAc3Speakers.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonAc3Speakers.Location = new System.Drawing.Point(16, 24);
      this.radioButtonAc3Speakers.Name = "radioButtonAc3Speakers";
      this.radioButtonAc3Speakers.Size = new System.Drawing.Size(123, 17);
      this.radioButtonAc3Speakers.TabIndex = 0;
      this.radioButtonAc3Speakers.TabStop = true;
      this.radioButtonAc3Speakers.Text = "Decode to speakers:";
      this.radioButtonAc3Speakers.UseVisualStyleBackColor = true;
      // 
      // checkBoxAc3Lfe
      // 
      this.checkBoxAc3Lfe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxAc3Lfe.AutoSize = true;
      this.checkBoxAc3Lfe.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAc3Lfe.Location = new System.Drawing.Point(413, 24);
      this.checkBoxAc3Lfe.Name = "checkBoxAc3Lfe";
      this.checkBoxAc3Lfe.Size = new System.Drawing.Size(43, 17);
      this.checkBoxAc3Lfe.TabIndex = 2;
      this.checkBoxAc3Lfe.Text = "LFE";
      this.checkBoxAc3Lfe.UseVisualStyleBackColor = true;
      // 
      // comboBoxAc3SpeakerConfig
      // 
      this.comboBoxAc3SpeakerConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxAc3SpeakerConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxAc3SpeakerConfig.Items.AddRange(new object[] {
            "Dual Mono",
            "Mono",
            "Stereo",
            "3 Front",
            "2 Front + 1 Rear",
            "3 Front + 1 Rear",
            "2 Front + 2 Rear",
            "3 Front + 2 Rear",
            "Channel 1",
            "Channel 2",
            "Dolby Stereo"});
      this.comboBoxAc3SpeakerConfig.Location = new System.Drawing.Point(168, 20);
      this.comboBoxAc3SpeakerConfig.Name = "comboBoxAc3SpeakerConfig";
      this.comboBoxAc3SpeakerConfig.Size = new System.Drawing.Size(240, 21);
      this.comboBoxAc3SpeakerConfig.TabIndex = 1;
      // 
      // checkBoxAc3DynamicRange
      // 
      this.checkBoxAc3DynamicRange.AutoSize = true;
      this.checkBoxAc3DynamicRange.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAc3DynamicRange.Location = new System.Drawing.Point(16, 72);
      this.checkBoxAc3DynamicRange.Name = "checkBoxAc3DynamicRange";
      this.checkBoxAc3DynamicRange.Size = new System.Drawing.Size(136, 17);
      this.checkBoxAc3DynamicRange.TabIndex = 4;
      this.checkBoxAc3DynamicRange.Text = "Dynamic Range Control";
      this.checkBoxAc3DynamicRange.UseVisualStyleBackColor = true;
      // 
      // groupBoxFormat
      // 
      this.groupBoxFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxFormat.Controls.Add(this.labelBoostValue);
      this.groupBoxFormat.Controls.Add(this.labelBoost);
      this.groupBoxFormat.Controls.Add(this.radioButtonPcm16Bit);
      this.groupBoxFormat.Controls.Add(this.radioButtonPcm24Bit);
      this.groupBoxFormat.Controls.Add(this.radioButtonPcm32Bit);
      this.groupBoxFormat.Controls.Add(this.radioButtonIeee);
      this.groupBoxFormat.Controls.Add(this.checkBoxNormalize);
      this.groupBoxFormat.Controls.Add(this.trackBarBoost);
      this.groupBoxFormat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxFormat.Location = new System.Drawing.Point(0, 224);
      this.groupBoxFormat.Name = "groupBoxFormat";
      this.groupBoxFormat.Size = new System.Drawing.Size(472, 121);
      this.groupBoxFormat.TabIndex = 2;
      this.groupBoxFormat.TabStop = false;
      this.groupBoxFormat.Text = "AC3/AAC/DTS/LPCM Format";
      // 
      // labelBoostValue
      // 
      this.labelBoostValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelBoostValue.Location = new System.Drawing.Point(336, 51);
      this.labelBoostValue.Name = "labelBoostValue";
      this.labelBoostValue.Size = new System.Drawing.Size(26, 23);
      this.labelBoostValue.TabIndex = 7;
      this.labelBoostValue.Text = "0";
      this.labelBoostValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // labelBoost
      // 
      this.labelBoost.Location = new System.Drawing.Point(16, 56);
      this.labelBoost.Name = "labelBoost";
      this.labelBoost.Size = new System.Drawing.Size(40, 16);
      this.labelBoost.TabIndex = 4;
      this.labelBoost.Text = "Boost:";
      // 
      // radioButtonPcm16Bit
      // 
      this.radioButtonPcm16Bit.AutoSize = true;
      this.radioButtonPcm16Bit.Checked = true;
      this.radioButtonPcm16Bit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonPcm16Bit.Location = new System.Drawing.Point(16, 24);
      this.radioButtonPcm16Bit.Name = "radioButtonPcm16Bit";
      this.radioButtonPcm16Bit.Size = new System.Drawing.Size(76, 17);
      this.radioButtonPcm16Bit.TabIndex = 0;
      this.radioButtonPcm16Bit.TabStop = true;
      this.radioButtonPcm16Bit.Text = "PCM 16 bit";
      this.radioButtonPcm16Bit.UseVisualStyleBackColor = true;
      // 
      // radioButtonPcm24Bit
      // 
      this.radioButtonPcm24Bit.AutoSize = true;
      this.radioButtonPcm24Bit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonPcm24Bit.Location = new System.Drawing.Point(104, 24);
      this.radioButtonPcm24Bit.Name = "radioButtonPcm24Bit";
      this.radioButtonPcm24Bit.Size = new System.Drawing.Size(76, 17);
      this.radioButtonPcm24Bit.TabIndex = 1;
      this.radioButtonPcm24Bit.Text = "PCM 24 bit";
      this.radioButtonPcm24Bit.UseVisualStyleBackColor = true;
      // 
      // radioButtonPcm32Bit
      // 
      this.radioButtonPcm32Bit.AutoSize = true;
      this.radioButtonPcm32Bit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonPcm32Bit.Location = new System.Drawing.Point(192, 24);
      this.radioButtonPcm32Bit.Name = "radioButtonPcm32Bit";
      this.radioButtonPcm32Bit.Size = new System.Drawing.Size(76, 17);
      this.radioButtonPcm32Bit.TabIndex = 2;
      this.radioButtonPcm32Bit.Text = "PCM 32 bit";
      this.radioButtonPcm32Bit.UseVisualStyleBackColor = true;
      // 
      // radioButtonIeee
      // 
      this.radioButtonIeee.AutoSize = true;
      this.radioButtonIeee.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonIeee.Location = new System.Drawing.Point(288, 24);
      this.radioButtonIeee.Name = "radioButtonIeee";
      this.radioButtonIeee.Size = new System.Drawing.Size(74, 17);
      this.radioButtonIeee.TabIndex = 3;
      this.radioButtonIeee.Text = "IEEE Float";
      this.radioButtonIeee.UseVisualStyleBackColor = true;
      // 
      // checkBoxNormalize
      // 
      this.checkBoxNormalize.AutoSize = true;
      this.checkBoxNormalize.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxNormalize.Location = new System.Drawing.Point(16, 89);
      this.checkBoxNormalize.Name = "checkBoxNormalize";
      this.checkBoxNormalize.Size = new System.Drawing.Size(70, 17);
      this.checkBoxNormalize.TabIndex = 6;
      this.checkBoxNormalize.Text = "Normalize";
      this.checkBoxNormalize.UseVisualStyleBackColor = true;
      // 
      // trackBarBoost
      // 
      this.trackBarBoost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.trackBarBoost.Location = new System.Drawing.Point(48, 52);
      this.trackBarBoost.Maximum = 100;
      this.trackBarBoost.Name = "trackBarBoost";
      this.trackBarBoost.Size = new System.Drawing.Size(292, 45);
      this.trackBarBoost.TabIndex = 5;
      this.trackBarBoost.TickStyle = System.Windows.Forms.TickStyle.None;
      this.trackBarBoost.Scroll += new System.EventHandler(this.trackBarBoost_Scroll);
      // 
      // groupBoxDtsDecoderSettings
      // 
      this.groupBoxDtsDecoderSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxDtsDecoderSettings.Controls.Add(this.radioButtonDtsSpdif);
      this.groupBoxDtsDecoderSettings.Controls.Add(this.radioButtonDtsSpeakers);
      this.groupBoxDtsDecoderSettings.Controls.Add(this.checkBoxDtsLfe);
      this.groupBoxDtsDecoderSettings.Controls.Add(this.comboBoxDtsSpeakerConfig);
      this.groupBoxDtsDecoderSettings.Controls.Add(this.checkBoxDtsDynamicRange);
      this.groupBoxDtsDecoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxDtsDecoderSettings.Location = new System.Drawing.Point(0, 112);
      this.groupBoxDtsDecoderSettings.Name = "groupBoxDtsDecoderSettings";
      this.groupBoxDtsDecoderSettings.Size = new System.Drawing.Size(472, 104);
      this.groupBoxDtsDecoderSettings.TabIndex = 1;
      this.groupBoxDtsDecoderSettings.TabStop = false;
      this.groupBoxDtsDecoderSettings.Text = "DTS Decoder Settings";
      // 
      // radioButtonDtsSpdif
      // 
      this.radioButtonDtsSpdif.AutoSize = true;
      this.radioButtonDtsSpdif.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonDtsSpdif.Location = new System.Drawing.Point(16, 48);
      this.radioButtonDtsSpdif.Name = "radioButtonDtsSpdif";
      this.radioButtonDtsSpdif.Size = new System.Drawing.Size(60, 17);
      this.radioButtonDtsSpdif.TabIndex = 3;
      this.radioButtonDtsSpdif.Text = "S/PDIF";
      this.radioButtonDtsSpdif.UseVisualStyleBackColor = true;
      this.radioButtonDtsSpdif.CheckedChanged += new System.EventHandler(this.radioButtonDTSSPDIF_CheckedChanged);
      // 
      // radioButtonDtsSpeakers
      // 
      this.radioButtonDtsSpeakers.AutoSize = true;
      this.radioButtonDtsSpeakers.Checked = true;
      this.radioButtonDtsSpeakers.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonDtsSpeakers.Location = new System.Drawing.Point(16, 24);
      this.radioButtonDtsSpeakers.Name = "radioButtonDtsSpeakers";
      this.radioButtonDtsSpeakers.Size = new System.Drawing.Size(123, 17);
      this.radioButtonDtsSpeakers.TabIndex = 0;
      this.radioButtonDtsSpeakers.TabStop = true;
      this.radioButtonDtsSpeakers.Text = "Decode to speakers:";
      this.radioButtonDtsSpeakers.UseVisualStyleBackColor = true;
      // 
      // checkBoxDtsLfe
      // 
      this.checkBoxDtsLfe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxDtsLfe.AutoSize = true;
      this.checkBoxDtsLfe.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDtsLfe.Location = new System.Drawing.Point(413, 24);
      this.checkBoxDtsLfe.Name = "checkBoxDtsLfe";
      this.checkBoxDtsLfe.Size = new System.Drawing.Size(43, 17);
      this.checkBoxDtsLfe.TabIndex = 2;
      this.checkBoxDtsLfe.Text = "LFE";
      this.checkBoxDtsLfe.UseVisualStyleBackColor = true;
      // 
      // comboBoxDtsSpeakerConfig
      // 
      this.comboBoxDtsSpeakerConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxDtsSpeakerConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDtsSpeakerConfig.Items.AddRange(new object[] {
            "Mono",
            "Dual Mono",
            "Stereo",
            "3 Front",
            "2 Front + 1 Rear",
            "3 Front + 1 Rear",
            "2 Front + 2 Rear",
            "3 Front + 2 Rear"});
      this.comboBoxDtsSpeakerConfig.Location = new System.Drawing.Point(168, 20);
      this.comboBoxDtsSpeakerConfig.Name = "comboBoxDtsSpeakerConfig";
      this.comboBoxDtsSpeakerConfig.Size = new System.Drawing.Size(240, 21);
      this.comboBoxDtsSpeakerConfig.TabIndex = 1;
      // 
      // checkBoxDtsDynamicRange
      // 
      this.checkBoxDtsDynamicRange.AutoSize = true;
      this.checkBoxDtsDynamicRange.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDtsDynamicRange.Location = new System.Drawing.Point(16, 72);
      this.checkBoxDtsDynamicRange.Name = "checkBoxDtsDynamicRange";
      this.checkBoxDtsDynamicRange.Size = new System.Drawing.Size(136, 17);
      this.checkBoxDtsDynamicRange.TabIndex = 4;
      this.checkBoxDtsDynamicRange.Text = "Dynamic Range Control";
      this.checkBoxDtsDynamicRange.UseVisualStyleBackColor = true;
      // 
      // groupBoxAacDecoderSettings
      // 
      this.groupBoxAacDecoderSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAacDecoderSettings.Controls.Add(this.checkBoxAacDynamic);
      this.groupBoxAacDecoderSettings.Controls.Add(this.checkBoxAacDownmix);
      this.groupBoxAacDecoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAacDecoderSettings.Location = new System.Drawing.Point(0, 353);
      this.groupBoxAacDecoderSettings.Name = "groupBoxAacDecoderSettings";
      this.groupBoxAacDecoderSettings.Size = new System.Drawing.Size(472, 52);
      this.groupBoxAacDecoderSettings.TabIndex = 3;
      this.groupBoxAacDecoderSettings.TabStop = false;
      this.groupBoxAacDecoderSettings.Text = "AAC Decoder Settings";
      // 
      // checkBoxAacDynamic
      // 
      this.checkBoxAacDynamic.AutoSize = true;
      this.checkBoxAacDynamic.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAacDynamic.Location = new System.Drawing.Point(192, 24);
      this.checkBoxAacDynamic.Name = "checkBoxAacDynamic";
      this.checkBoxAacDynamic.Size = new System.Drawing.Size(136, 17);
      this.checkBoxAacDynamic.TabIndex = 5;
      this.checkBoxAacDynamic.Text = "Dynamic Range Control";
      this.checkBoxAacDynamic.UseVisualStyleBackColor = true;
      // 
      // checkBoxAacDownmix
      // 
      this.checkBoxAacDownmix.AutoSize = true;
      this.checkBoxAacDownmix.Checked = true;
      this.checkBoxAacDownmix.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAacDownmix.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAacDownmix.Location = new System.Drawing.Point(16, 24);
      this.checkBoxAacDownmix.Name = "checkBoxAacDownmix";
      this.checkBoxAacDownmix.Size = new System.Drawing.Size(111, 17);
      this.checkBoxAacDownmix.TabIndex = 0;
      this.checkBoxAacDownmix.Text = "Downmix to stereo";
      this.checkBoxAacDownmix.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(104, 64);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(48, 16);
      this.label2.TabIndex = 7;
      this.label2.Text = "Boost:";
      // 
      // MPEG2DecAudioFilter
      // 
      this.Controls.Add(this.groupBoxDtsDecoderSettings);
      this.Controls.Add(this.groupBoxAc3DecoderSettings);
      this.Controls.Add(this.groupBoxFormat);
      this.Controls.Add(this.groupBoxAacDecoderSettings);
      this.Name = "MPEG2DecAudioFilter";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxAc3DecoderSettings.ResumeLayout(false);
      this.groupBoxAc3DecoderSettings.PerformLayout();
      this.groupBoxFormat.ResumeLayout(false);
      this.groupBoxFormat.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarBoost)).EndInit();
      this.groupBoxDtsDecoderSettings.ResumeLayout(false);
      this.groupBoxDtsDecoderSettings.PerformLayout();
      this.groupBoxAacDecoderSettings.ResumeLayout(false);
      this.groupBoxAacDecoderSettings.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    public override void OnSectionActivated()
    {
      groupBoxAacDecoderSettings.Visible = groupBoxFormat.Visible = SettingsForm.AdvancedMode;
      base.OnSectionActivated();
    }

    public override void LoadSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Gabest\Filters\MPEG Audio Decoder"))
        if (subkey != null)
        {
          try
          {
            int regValue;

            regValue = (int)subkey.GetValue("AacSpeakerConfig");
            if (regValue == 1)
              checkBoxAacDownmix.Checked = true;
            else
              checkBoxAacDownmix.Checked = false;

            regValue = (int)subkey.GetValue("AacDynamicRangeControl");
            if (regValue == 1)
              checkBoxAacDynamic.Checked = true;
            else
              checkBoxAacDynamic.Checked = false;

            regValue = (int)subkey.GetValue("Ac3DynamicRangeControl");
            if (regValue == 1)
              checkBoxAc3DynamicRange.Checked = true;
            else
              checkBoxAc3DynamicRange.Checked = false;

            regValue = (int)subkey.GetValue("DtsDynamicRangeControl");
            if (regValue == 1)
              checkBoxDtsDynamicRange.Checked = true;
            else
              checkBoxDtsDynamicRange.Checked = false;

            regValue = (int)subkey.GetValue("Normalize");
            if (regValue == 1)
              checkBoxNormalize.Checked = true;
            else
              checkBoxNormalize.Checked = false;

            regValue = (int)subkey.GetValue("Ac3SpeakerConfig");
            if (regValue < 27)
            {
              if (regValue > 11)
              {
                regValue = (regValue - 16); // LFE enabled
                checkBoxAc3Lfe.Checked = true;
              }
              else
                checkBoxAc3Lfe.Checked = false;

              if (regValue > -1)
              {
                comboBoxAc3SpeakerConfig.SelectedIndex = (int)regValue;
                radioButtonAc3Speakers.Checked = (regValue >= 0);
              }
              else
              {
                regValue = (regValue * -1);
                comboBoxAc3SpeakerConfig.SelectedIndex = (int)regValue;
                radioButtonAc3Spdif.Checked = true;
              }
            }
            else
              radioButtonAc3Spdif.Checked = true;

            /// Evaluating DTS Settings
            /// If S/PDIF is enabled Gabest adds a negative sign to the speaker selection
            /// therefore he can "remember" which settings have been used earlier
            regValue = (int)subkey.GetValue("DtsSpeakerConfig");
            if (regValue < 139)
            {
              if (regValue > 11)
              {
                regValue = (regValue - 128);
                checkBoxDtsLfe.Checked = true;
              }
              else
                checkBoxDtsLfe.Checked = false;

              if (regValue > -1)
              {
                if ((int)regValue < 3) comboBoxDtsSpeakerConfig.SelectedIndex = (int)regValue;
                else comboBoxDtsSpeakerConfig.SelectedIndex = (int)regValue - 2;
                radioButtonDtsSpeakers.Checked = (regValue >= 0);
              }
              else
              {
                regValue = (regValue * -1);
                if ((int)regValue < 3) comboBoxDtsSpeakerConfig.SelectedIndex = (int)regValue;
                else comboBoxDtsSpeakerConfig.SelectedIndex = (int)regValue - 2;
                radioButtonDtsSpdif.Checked = true;
              }
            }
            else
              radioButtonDtsSpdif.Checked = true;

            regValue = (int)subkey.GetValue("Boost");
            trackBarBoost.Value = (int)regValue;
            labelBoostValue.Text = trackBarBoost.Value.ToString();


            regValue = (int)subkey.GetValue("SampleFormat");
            switch (regValue)
            {
              case 0:
                radioButtonPcm16Bit.Checked = true;
                break;
              case 1:
                radioButtonPcm24Bit.Checked = true;
                break;
              case 2:
                radioButtonPcm32Bit.Checked = true;
                break;
              case 3:
                radioButtonIeee.Checked = true;
                break;
              default:
                radioButtonPcm16Bit.Checked = true;
                break;
            }
          }
          catch (Exception ex)
          {
            Log.Info("Exception while loading MPA settings: {0}", ex.Message);
          }
        }
    }

    public override void SaveSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Gabest\Filters\MPEG Audio Decoder"))
        if (subkey != null)
        {
          try
          {
            Int32 regValue;

            if (checkBoxAacDownmix.Checked)
              regValue = 1;
            else
              regValue = 0;
            subkey.SetValue("AacSpeakerConfig", regValue, RegistryValueKind.DWord);

            if (checkBoxAacDynamic.Checked)
              regValue = 1;
            else
              regValue = 0;
            subkey.SetValue("AacDynamicRangeControl", regValue, RegistryValueKind.DWord);

            if (checkBoxAc3DynamicRange.Checked)
              regValue = 1;
            else
              regValue = 0;
            subkey.SetValue("Ac3DynamicRangeControl", regValue, RegistryValueKind.DWord);

            if (checkBoxDtsDynamicRange.Checked)
              regValue = 1;
            else
              regValue = 0;
            subkey.SetValue("DtsDynamicRangeControl", regValue, RegistryValueKind.DWord);

            if (checkBoxNormalize.Checked)
              regValue = 1;
            else
              regValue = 0;
            subkey.SetValue("Normalize", regValue, RegistryValueKind.DWord);

            /// not using unchecked leads to Registry ArgumentException as this is near UInt32.MaxValue
            /// but Gabest expects this and Windows doesn't moan ;-)
            if (radioButtonAc3Spdif.Checked)
              subkey.SetValue("Ac3SpeakerConfig", unchecked((Int32)4294967289), RegistryValueKind.DWord);
            else
            {
              if (comboBoxAc3SpeakerConfig.SelectedIndex == -1) // for missing first time defaults of MPA
                comboBoxAc3SpeakerConfig.SelectedIndex = 2;
              if (checkBoxAc3Lfe.Checked)
                subkey.SetValue("Ac3SpeakerConfig", (int)(comboBoxAc3SpeakerConfig.SelectedIndex + 16), RegistryValueKind.DWord);
              else
                subkey.SetValue("Ac3SpeakerConfig", (int)comboBoxAc3SpeakerConfig.SelectedIndex, RegistryValueKind.DWord);
            }


            float SPDIFSetting;
            int DTSRegValue;
            int DTSConfig = comboBoxDtsSpeakerConfig.SelectedIndex;
            if (DTSConfig < 3)
              DTSRegValue = DTSConfig;
            else
              DTSRegValue = (DTSConfig + 2);

            if (radioButtonDtsSpdif.Checked)
            {
              if (DTSRegValue >= 0)
                DTSRegValue = (DTSRegValue * -1);
              SPDIFSetting = Convert.ToSingle(DTSRegValue);
              //MediaPortal.GUI.Library.Log.Info("DEBUG: Write S/PDIF-DTSSpeakerConfig: {0}", SPDIFSetting.ToString());
              //This didn't work but Gabest uses a type overflow well knowing that the values subtract from MaxValue after this
              //subkey.SetValue("DtsSpeakerConfig", BitConverter.ToInt32(BitConverter.GetBytes(SPDIFSetting), 0), RegistryValueKind.DWord);
              subkey.SetValue("DtsSpeakerConfig", unchecked((Int32)4294967287), RegistryValueKind.DWord);
            }
            else
            {
              if (DTSRegValue == -1) // for missing first time defaults of MPA
                DTSRegValue = 2;
              if (checkBoxDtsLfe.Checked)              
                subkey.SetValue("DtsSpeakerConfig", (DTSRegValue + 128), RegistryValueKind.DWord);
              else
                subkey.SetValue("DtsSpeakerConfig", DTSRegValue, RegistryValueKind.DWord);
            }

            subkey.SetValue("Boost", trackBarBoost.Value);

            if (radioButtonPcm16Bit.Checked)
              regValue = 0;

            if (radioButtonPcm24Bit.Checked)
              regValue = 1;

            if (radioButtonPcm32Bit.Checked)
              regValue = 2;

            if (radioButtonIeee.Checked)
              regValue = 3;

            subkey.SetValue("SampleFormat", regValue, RegistryValueKind.DWord);
          }
          catch (Exception ex)
          {
            Log.Info("Exception while writing MPA settings: {0}", ex.Message);
          }
        }
        else
          Log.Info("Registry access error while trying to write MPA settings.");
    }


    private void radioButtonAC3SPDIF_CheckedChanged(object sender, System.EventArgs e)
    {
      if (radioButtonAc3Spdif.Checked)
      {
        comboBoxAc3SpeakerConfig.Enabled = false;
        checkBoxAc3Lfe.Enabled = false;
      }
      else
      {
        comboBoxAc3SpeakerConfig.Enabled = true;
        checkBoxAc3Lfe.Enabled = true;
      }

    }

    private void radioButtonDTSSPDIF_CheckedChanged(object sender, System.EventArgs e)
    {
      if (radioButtonDtsSpdif.Checked)
      {
        comboBoxDtsSpeakerConfig.Enabled = false;
        checkBoxDtsLfe.Enabled = false;
      }
      else
      {
        comboBoxDtsSpeakerConfig.Enabled = true;
        checkBoxDtsLfe.Enabled = true;
      }
    }

    private void trackBarBoost_Scroll(object sender, EventArgs e)
    {
      labelBoostValue.Text = trackBarBoost.Value.ToString();
    }
  }
}