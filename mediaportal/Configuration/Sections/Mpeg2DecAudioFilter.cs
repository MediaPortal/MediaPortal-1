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
using Microsoft.Win32;

using System.Runtime.InteropServices;

using DShowNET;
using DirectShowLib;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{

  public class MPEG2DecAudioFilter : MediaPortal.Configuration.SectionSettings
  {
    //private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioPCM16Bit;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonPCM24Bit;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonPCM32Bit;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonIEEE;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxNormalize;
    private System.Windows.Forms.TrackBar trackBarBoost;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    //private MediaPortal.UserInterface.Controls.MPLabel label3;
    //private MediaPortal.UserInterface.Controls.MPLabel label5;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonAC3Speakers;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonAC3SPDIF;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAC3DynamicRange;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxAC3SpeakerConfig;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAC3LFE;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDTSSpeakerConfig;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDTSDynamicRange;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonDTSSPDIF;
    private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonDTSSpeakers;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDTSLFE;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAACDownmix;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox4;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox5;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// 
    /// </summary>
    public MPEG2DecAudioFilter()
      : this("MPEG/AC3/DTS/LPCM Audio Decoder")
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public MPEG2DecAudioFilter(string name)
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
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonAC3SPDIF = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonAC3Speakers = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.checkBoxAC3LFE = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.comboBoxAC3SpeakerConfig = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxAC3DynamicRange = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.radioPCM16Bit = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPCM24Bit = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPCM32Bit = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonIEEE = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.checkBoxNormalize = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.trackBarBoost = new System.Windows.Forms.TrackBar();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonDTSSPDIF = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonDTSSpeakers = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.checkBoxDTSLFE = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.comboBoxDTSSpeakerConfig = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.checkBoxDTSDynamicRange = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxAACDownmix = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox4.SuspendLayout();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarBoost)).BeginInit();
      this.groupBox2.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox4.Controls.Add(this.radioButtonAC3SPDIF);
      this.groupBox4.Controls.Add(this.radioButtonAC3Speakers);
      this.groupBox4.Controls.Add(this.checkBoxAC3LFE);
      this.groupBox4.Controls.Add(this.comboBoxAC3SpeakerConfig);
      this.groupBox4.Controls.Add(this.checkBoxAC3DynamicRange);
      this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.groupBox4.Location = new System.Drawing.Point(0, 0);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(472, 104);
      this.groupBox4.TabIndex = 0;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "AC3 Decoder Settings";
      // 
      // radioButtonAC3SPDIF
      // 
      this.radioButtonAC3SPDIF.Location = new System.Drawing.Point(16, 48);
      this.radioButtonAC3SPDIF.Name = "radioButtonAC3SPDIF";
      this.radioButtonAC3SPDIF.Size = new System.Drawing.Size(56, 16);
      this.radioButtonAC3SPDIF.TabIndex = 3;
      this.radioButtonAC3SPDIF.Text = "S/PDIF";
      this.radioButtonAC3SPDIF.CheckedChanged += new System.EventHandler(this.radioButtonAC3SPDIF_CheckedChanged);
      // 
      // radioButtonAC3Speakers
      // 
      this.radioButtonAC3Speakers.Checked = true;
      this.radioButtonAC3Speakers.Location = new System.Drawing.Point(16, 24);
      this.radioButtonAC3Speakers.Name = "radioButtonAC3Speakers";
      this.radioButtonAC3Speakers.Size = new System.Drawing.Size(120, 16);
      this.radioButtonAC3Speakers.TabIndex = 0;
      this.radioButtonAC3Speakers.TabStop = true;
      this.radioButtonAC3Speakers.Text = "Decode to speakers:";
      this.radioButtonAC3Speakers.CheckedChanged += new System.EventHandler(this.radioButtonAC3Speakers_CheckedChanged);
      // 
      // checkBoxAC3LFE
      // 
      this.checkBoxAC3LFE.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxAC3LFE.Location = new System.Drawing.Point(416, 24);
      this.checkBoxAC3LFE.Name = "checkBoxAC3LFE";
      this.checkBoxAC3LFE.Size = new System.Drawing.Size(40, 16);
      this.checkBoxAC3LFE.TabIndex = 2;
      this.checkBoxAC3LFE.Text = "LFE";
      // 
      // comboBoxAC3SpeakerConfig
      // 
      this.comboBoxAC3SpeakerConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxAC3SpeakerConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxAC3SpeakerConfig.Items.AddRange(new object[] {
                                                                  "Mono",
                                                                  "Dual Mono",
                                                                  "Stereo",
                                                                  "Dolby Stereo",
                                                                  "3 Front",
                                                                  "2 Front + 1 Rear",
                                                                  "3 Front + 1 Rear",
                                                                  "2 Front + 2 Rear",
                                                                  "3 Front + 2 Rear",
                                                                  "Channel 1",
                                                                  "Channel 2",
                                                                  ""});
      this.comboBoxAC3SpeakerConfig.Location = new System.Drawing.Point(168, 20);
      this.comboBoxAC3SpeakerConfig.Name = "comboBoxAC3SpeakerConfig";
      this.comboBoxAC3SpeakerConfig.Size = new System.Drawing.Size(240, 21);
      this.comboBoxAC3SpeakerConfig.TabIndex = 1;
      // 
      // checkBoxAC3DynamicRange
      // 
      this.checkBoxAC3DynamicRange.Location = new System.Drawing.Point(16, 72);
      this.checkBoxAC3DynamicRange.Name = "checkBoxAC3DynamicRange";
      this.checkBoxAC3DynamicRange.Size = new System.Drawing.Size(144, 16);
      this.checkBoxAC3DynamicRange.TabIndex = 4;
      this.checkBoxAC3DynamicRange.Text = "Dynamic Range Control";
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.label4);
      this.groupBox3.Controls.Add(this.radioPCM16Bit);
      this.groupBox3.Controls.Add(this.radioButtonPCM24Bit);
      this.groupBox3.Controls.Add(this.radioButtonPCM32Bit);
      this.groupBox3.Controls.Add(this.radioButtonIEEE);
      this.groupBox3.Controls.Add(this.checkBoxNormalize);
      this.groupBox3.Controls.Add(this.trackBarBoost);
      this.groupBox3.Location = new System.Drawing.Point(0, 224);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(472, 112);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "AC3/AAC/DTS/LPCM Format";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 56);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(40, 16);
      this.label4.TabIndex = 4;
      this.label4.Text = "Boost:";
      // 
      // radioPCM16Bit
      // 
      this.radioPCM16Bit.Checked = true;
      this.radioPCM16Bit.Location = new System.Drawing.Point(16, 24);
      this.radioPCM16Bit.Name = "radioPCM16Bit";
      this.radioPCM16Bit.Size = new System.Drawing.Size(80, 16);
      this.radioPCM16Bit.TabIndex = 0;
      this.radioPCM16Bit.TabStop = true;
      this.radioPCM16Bit.Text = "PCM 16 bit";
      // 
      // radioButtonPCM24Bit
      // 
      this.radioButtonPCM24Bit.Location = new System.Drawing.Point(104, 24);
      this.radioButtonPCM24Bit.Name = "radioButtonPCM24Bit";
      this.radioButtonPCM24Bit.Size = new System.Drawing.Size(80, 16);
      this.radioButtonPCM24Bit.TabIndex = 1;
      this.radioButtonPCM24Bit.Text = "PCM 24 bit";
      // 
      // radioButtonPCM32Bit
      // 
      this.radioButtonPCM32Bit.Location = new System.Drawing.Point(192, 24);
      this.radioButtonPCM32Bit.Name = "radioButtonPCM32Bit";
      this.radioButtonPCM32Bit.Size = new System.Drawing.Size(80, 16);
      this.radioButtonPCM32Bit.TabIndex = 2;
      this.radioButtonPCM32Bit.Text = "PCM 32 bit";
      // 
      // radioButtonIEEE
      // 
      this.radioButtonIEEE.Location = new System.Drawing.Point(288, 24);
      this.radioButtonIEEE.Name = "radioButtonIEEE";
      this.radioButtonIEEE.Size = new System.Drawing.Size(80, 16);
      this.radioButtonIEEE.TabIndex = 3;
      this.radioButtonIEEE.Text = "IEEE float";
      // 
      // checkBoxNormalize
      // 
      this.checkBoxNormalize.Location = new System.Drawing.Point(16, 86);
      this.checkBoxNormalize.Name = "checkBoxNormalize";
      this.checkBoxNormalize.Size = new System.Drawing.Size(80, 16);
      this.checkBoxNormalize.TabIndex = 6;
      this.checkBoxNormalize.Text = "Normalize";
      // 
      // trackBarBoost
      // 
      this.trackBarBoost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.trackBarBoost.Location = new System.Drawing.Point(48, 52);
      this.trackBarBoost.Maximum = 100;
      this.trackBarBoost.Name = "trackBarBoost";
      this.trackBarBoost.Size = new System.Drawing.Size(200, 45);
      this.trackBarBoost.TabIndex = 5;
      this.trackBarBoost.TickStyle = System.Windows.Forms.TickStyle.None;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.radioButtonDTSSPDIF);
      this.groupBox2.Controls.Add(this.radioButtonDTSSpeakers);
      this.groupBox2.Controls.Add(this.checkBoxDTSLFE);
      this.groupBox2.Controls.Add(this.comboBoxDTSSpeakerConfig);
      this.groupBox2.Controls.Add(this.checkBoxDTSDynamicRange);
      this.groupBox2.Location = new System.Drawing.Point(0, 112);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 104);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "DTS Decoder Settings";
      // 
      // radioButtonDTSSPDIF
      // 
      this.radioButtonDTSSPDIF.Location = new System.Drawing.Point(16, 48);
      this.radioButtonDTSSPDIF.Name = "radioButtonDTSSPDIF";
      this.radioButtonDTSSPDIF.Size = new System.Drawing.Size(56, 16);
      this.radioButtonDTSSPDIF.TabIndex = 3;
      this.radioButtonDTSSPDIF.Text = "S/PDIF";
      this.radioButtonDTSSPDIF.CheckedChanged += new System.EventHandler(this.radioButtonDTSSPDIF_CheckedChanged);
      // 
      // radioButtonDTSSpeakers
      // 
      this.radioButtonDTSSpeakers.Checked = true;
      this.radioButtonDTSSpeakers.Location = new System.Drawing.Point(16, 24);
      this.radioButtonDTSSpeakers.Name = "radioButtonDTSSpeakers";
      this.radioButtonDTSSpeakers.Size = new System.Drawing.Size(120, 16);
      this.radioButtonDTSSpeakers.TabIndex = 0;
      this.radioButtonDTSSpeakers.TabStop = true;
      this.radioButtonDTSSpeakers.Text = "Decode to speakers:";
      this.radioButtonDTSSpeakers.CheckedChanged += new System.EventHandler(this.radioButtonDTSSpeakers_CheckedChanged);
      // 
      // checkBoxDTSLFE
      // 
      this.checkBoxDTSLFE.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxDTSLFE.Location = new System.Drawing.Point(416, 24);
      this.checkBoxDTSLFE.Name = "checkBoxDTSLFE";
      this.checkBoxDTSLFE.Size = new System.Drawing.Size(40, 16);
      this.checkBoxDTSLFE.TabIndex = 2;
      this.checkBoxDTSLFE.Text = "LFE";
      // 
      // comboBoxDTSSpeakerConfig
      // 
      this.comboBoxDTSSpeakerConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxDTSSpeakerConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDTSSpeakerConfig.Items.AddRange(new object[] {
                                                                  "Mono",
                                                                  "Dual Mono",
                                                                  "Stereo",
                                                                  "3 Front",
                                                                  "2 Front + 1 Rear",
                                                                  "3 Front + 1 Rear",
                                                                  "2 Front + 2 Rear",
                                                                  "3 Front + 2 Rear"});
      this.comboBoxDTSSpeakerConfig.Location = new System.Drawing.Point(168, 20);
      this.comboBoxDTSSpeakerConfig.Name = "comboBoxDTSSpeakerConfig";
      this.comboBoxDTSSpeakerConfig.Size = new System.Drawing.Size(240, 21);
      this.comboBoxDTSSpeakerConfig.TabIndex = 1;
      // 
      // checkBoxDTSDynamicRange
      // 
      this.checkBoxDTSDynamicRange.Location = new System.Drawing.Point(16, 72);
      this.checkBoxDTSDynamicRange.Name = "checkBoxDTSDynamicRange";
      this.checkBoxDTSDynamicRange.Size = new System.Drawing.Size(136, 16);
      this.checkBoxDTSDynamicRange.TabIndex = 4;
      this.checkBoxDTSDynamicRange.Text = "Dynamic Range Control";
      // 
      // groupBox5
      // 
      this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.checkBoxAACDownmix);
      this.groupBox5.Location = new System.Drawing.Point(0, 344);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(472, 56);
      this.groupBox5.TabIndex = 3;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "AAC Decoder Settings";
      // 
      // checkBoxAACDownmix
      // 
      this.checkBoxAACDownmix.Checked = true;
      this.checkBoxAACDownmix.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAACDownmix.Location = new System.Drawing.Point(16, 24);
      this.checkBoxAACDownmix.Name = "checkBoxAACDownmix";
      this.checkBoxAACDownmix.Size = new System.Drawing.Size(128, 16);
      this.checkBoxAACDownmix.TabIndex = 0;
      this.checkBoxAACDownmix.Text = "Downmix to stereo";
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
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox4);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox5);
      this.Name = "MPEG2DecAudioFilter";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox4.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarBoost)).EndInit();
      this.groupBox2.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    public override void LoadSettings()
    {
      RegistryKey hkcu = Registry.CurrentUser;
      RegistryKey subkey = hkcu.CreateSubKey(@"Software\MediaPortal\Mpeg Audio Filter");
      if (subkey != null)
      {
        try
        {
          Int32 regValue = (Int32)subkey.GetValue("AAC Downmix");
          if (regValue == 1) checkBoxAACDownmix.Checked = true;
          else checkBoxAACDownmix.Checked = false;

          regValue = (Int32)subkey.GetValue("AC3 Dynamic Range");
          if (regValue == 1) checkBoxAC3DynamicRange.Checked = true;
          else checkBoxAC3DynamicRange.Checked = false;

          regValue = (Int32)subkey.GetValue("AC3 LFE");
          if (regValue == 1) checkBoxAC3LFE.Checked = true;
          else checkBoxAC3LFE.Checked = false;

          regValue = (Int32)subkey.GetValue("DTS Dynamic Range");
          if (regValue == 1) checkBoxDTSDynamicRange.Checked = true;
          else checkBoxDTSDynamicRange.Checked = false;

          regValue = (Int32)subkey.GetValue("DTS LFE");
          if (regValue == 1) checkBoxDTSLFE.Checked = true;
          else checkBoxDTSLFE.Checked = false;

          regValue = (Int32)subkey.GetValue("Normalize");
          if (regValue == 1) checkBoxNormalize.Checked = true;
          else checkBoxNormalize.Checked = false;

          regValue = (Int32)subkey.GetValue("AC3 Speaker Config");
          comboBoxAC3SpeakerConfig.SelectedIndex = regValue;

          regValue = (Int32)subkey.GetValue("DTS Speaker Config");
          comboBoxDTSSpeakerConfig.SelectedIndex = regValue;


          regValue = (Int32)subkey.GetValue("Boost");
          trackBarBoost.Value = regValue;

          regValue = (Int32)subkey.GetValue("Output Format");
          radioPCM16Bit.Checked = (regValue == 0);
          radioButtonPCM24Bit.Checked = (regValue == 1);
          radioButtonPCM32Bit.Checked = (regValue == 2);
          radioButtonIEEE.Checked = (regValue == 3);

          regValue = (Int32)subkey.GetValue("AC3Decoder");
          radioButtonAC3Speakers.Checked = (regValue == 0);
          radioButtonAC3SPDIF.Checked = (regValue == 1);


          regValue = (Int32)subkey.GetValue("DTSDecoder");
          radioButtonDTSSpeakers.Checked = (regValue == 0);
          radioButtonDTSSPDIF.Checked = (regValue == 1);
        }
        catch (Exception)
        {
        }
        finally
        {
          subkey.Close();
        }
      }
    }
    public override void SaveSettings()
    {
      RegistryKey hkcu = Registry.CurrentUser;
      RegistryKey subkey = hkcu.CreateSubKey(@"Software\MediaPortal\Mpeg Audio Filter");
      if (subkey != null)
      {
        Int32 regValue;
        if (checkBoxAACDownmix.Checked) regValue = 1;
        else regValue = 0;
        subkey.SetValue("AAC Downmix", regValue);

        if (checkBoxAC3DynamicRange.Checked) regValue = 1;
        else regValue = 0;
        subkey.SetValue("AC3 Dynamic Range", regValue);

        if (checkBoxAC3LFE.Checked) regValue = 1;
        else regValue = 0;
        subkey.SetValue("AC3 LFE", regValue);

        if (checkBoxDTSDynamicRange.Checked) regValue = 1;
        else regValue = 0;
        subkey.SetValue("DTS Dynamic Range", regValue);

        if (checkBoxDTSLFE.Checked) regValue = 1;
        else regValue = 0;
        subkey.SetValue("DTS LFE", regValue);

        if (checkBoxNormalize.Checked) regValue = 1;
        else regValue = 0;
        subkey.SetValue("Normalize", regValue);

        subkey.SetValue("AC3 Speaker Config", comboBoxAC3SpeakerConfig.SelectedIndex);
        subkey.SetValue("DTS Speaker Config", comboBoxDTSSpeakerConfig.SelectedIndex);
        subkey.SetValue("Boost", trackBarBoost.Value);

        if (radioPCM16Bit.Checked) regValue = 0;
        if (radioButtonPCM24Bit.Checked) regValue = 1;
        if (radioButtonPCM32Bit.Checked) regValue = 2;
        if (radioButtonIEEE.Checked) regValue = 3;
        subkey.SetValue("Output Format", regValue);

        if (radioButtonAC3Speakers.Checked) regValue = 0;
        if (radioButtonAC3SPDIF.Checked) regValue = 1;
        subkey.SetValue("AC3Decoder", regValue);

        if (radioButtonDTSSpeakers.Checked) regValue = 0;
        if (radioButtonDTSSPDIF.Checked) regValue = 1;
        subkey.SetValue("DTSDecoder", regValue);

        subkey.Close();
      }
    }

    private void radioButtonAC3Speakers_CheckedChanged(object sender, System.EventArgs e)
    {
      if (radioButtonAC3Speakers.Checked)
      {
        //comboBoxAC3SpeakerConfig.Enabled=true;
        //checkBoxAC3LFE.Enabled=true;
      }

    }

    private void radioButtonAC3SPDIF_CheckedChanged(object sender, System.EventArgs e)
    {
      if (radioButtonAC3SPDIF.Checked)
      {
        //comboBoxAC3SpeakerConfig.Enabled=false;
        //checkBoxAC3LFE.Enabled=false;
      }
    }

    private void radioButtonDTSSpeakers_CheckedChanged(object sender, System.EventArgs e)
    {
      if (radioButtonDTSSpeakers.Checked)
      {
        //comboBoxDTSSpeakerConfig.Enabled=true;
        //checkBoxDTSLFE.Enabled=true;
      }
    }

    private void radioButtonDTSSPDIF_CheckedChanged(object sender, System.EventArgs e)
    {
      if (radioButtonDTSSPDIF.Checked)
      {
        //comboBoxDTSSpeakerConfig.Enabled=false;
        //checkBoxDTSLFE.Enabled=false;
      }
    }

  }
}

