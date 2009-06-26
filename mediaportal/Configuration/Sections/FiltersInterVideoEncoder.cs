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
using System.ComponentModel;
using MediaPortal.UserInterface.Controls;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class FiltersInterVideoEncoder : SectionSettings
  {
    private Int32 _regQuality;
    private Int32 _regMode;
    private Int32 _regFrameRate;
    private Int32 _regAspectRatio;
    private Int32 _regVideoBitRate;
    private Int32 _regVideoPeakBitRate;
    private int _regFrequecy;
    private MPComboBox comboBoxFrameRate;
    private MPGroupBox mpGroupBox1;
    private MPLabel FrameRate;
    private MPGroupBox mpGroupBox2;
    private MPComboBox comboBoxAudioBitrate;
    private MPComboBox comboBoxFrequency;
    private MPLabel SampleFrequency;
    private MPLabel AudioBitrate;
    private MPComboBox comboBoxAspectRatio;
    private MPLabel EncoderQuality;
    private MPLabel VideoFormat;
    private MPComboBox comboBoxEncoderQuality;
    private MPComboBox comboBoxVideoFormat;
    private MPLabel AspectRatio;
    private MPRadioButton radioButtonQualityGood;
    private MPRadioButton radioButtonQualityHigh;
    private MPRadioButton radioButtonModeMono;
    private MPRadioButton radioButtonModeDualChannel;
    private MPRadioButton radioButtonModeJointStereo;
    private MPRadioButton radioButtonModeStereo;
    private MPRadioButton radioButtonQualityLow;
    private MPRadioButton radioButtonQualityMedium;
    private MPGroupBox AudioQuality;
    private MPGroupBox AudioMode;
    private IContainer components = null;

    /// <summary>
    /// 
    /// </summary>
    public FiltersInterVideoEncoder()
      : this("InterVideo Encoder Filters")
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public FiltersInterVideoEncoder(string name)
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
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.AudioQuality = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonQualityLow = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonQualityMedium = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonQualityGood = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonQualityHigh = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.SampleFrequency = new MediaPortal.UserInterface.Controls.MPLabel();
      this.AudioBitrate = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxFrequency = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxAudioBitrate = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.AudioMode = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonModeStereo = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonModeDualChannel = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonModeMono = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonModeJointStereo = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.EncoderQuality = new MediaPortal.UserInterface.Controls.MPLabel();
      this.VideoFormat = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxEncoderQuality = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxVideoFormat = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.AspectRatio = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxAspectRatio = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.FrameRate = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxFrameRate = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpGroupBox2.SuspendLayout();
      this.AudioQuality.SuspendLayout();
      this.AudioMode.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.AudioQuality);
      this.mpGroupBox2.Controls.Add(this.SampleFrequency);
      this.mpGroupBox2.Controls.Add(this.AudioBitrate);
      this.mpGroupBox2.Controls.Add(this.comboBoxFrequency);
      this.mpGroupBox2.Controls.Add(this.comboBoxAudioBitrate);
      this.mpGroupBox2.Controls.Add(this.AudioMode);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(0, 183);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(472, 202);
      this.mpGroupBox2.TabIndex = 0;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Audio Encoder Settings";
      // 
      // AudioQuality
      // 
      this.AudioQuality.Controls.Add(this.radioButtonQualityLow);
      this.AudioQuality.Controls.Add(this.radioButtonQualityMedium);
      this.AudioQuality.Controls.Add(this.radioButtonQualityGood);
      this.AudioQuality.Controls.Add(this.radioButtonQualityHigh);
      this.AudioQuality.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.AudioQuality.Location = new System.Drawing.Point(223, 19);
      this.AudioQuality.Name = "AudioQuality";
      this.AudioQuality.Size = new System.Drawing.Size(224, 78);
      this.AudioQuality.TabIndex = 14;
      this.AudioQuality.TabStop = false;
      this.AudioQuality.Text = "Audio Quality";
      // 
      // radioButtonQualityLow
      // 
      this.radioButtonQualityLow.AutoSize = true;
      this.radioButtonQualityLow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonQualityLow.Location = new System.Drawing.Point(135, 42);
      this.radioButtonQualityLow.Name = "radioButtonQualityLow";
      this.radioButtonQualityLow.Size = new System.Drawing.Size(44, 17);
      this.radioButtonQualityLow.TabIndex = 9;
      this.radioButtonQualityLow.TabStop = true;
      this.radioButtonQualityLow.Text = "Low";
      this.radioButtonQualityLow.UseVisualStyleBackColor = true;
      this.radioButtonQualityLow.CheckedChanged += new System.EventHandler(this.radioButtonQualityLow_CheckedChanged);
      // 
      // radioButtonQualityMedium
      // 
      this.radioButtonQualityMedium.AutoSize = true;
      this.radioButtonQualityMedium.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonQualityMedium.Location = new System.Drawing.Point(31, 42);
      this.radioButtonQualityMedium.Name = "radioButtonQualityMedium";
      this.radioButtonQualityMedium.Size = new System.Drawing.Size(61, 17);
      this.radioButtonQualityMedium.TabIndex = 8;
      this.radioButtonQualityMedium.TabStop = true;
      this.radioButtonQualityMedium.Text = "Medium";
      this.radioButtonQualityMedium.UseVisualStyleBackColor = true;
      this.radioButtonQualityMedium.CheckedChanged +=
        new System.EventHandler(this.radioButtonQualityMedium_CheckedChanged);
      // 
      // radioButtonQualityGood
      // 
      this.radioButtonQualityGood.AutoSize = true;
      this.radioButtonQualityGood.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonQualityGood.Location = new System.Drawing.Point(135, 19);
      this.radioButtonQualityGood.Name = "radioButtonQualityGood";
      this.radioButtonQualityGood.Size = new System.Drawing.Size(50, 17);
      this.radioButtonQualityGood.TabIndex = 7;
      this.radioButtonQualityGood.TabStop = true;
      this.radioButtonQualityGood.Text = "Good";
      this.radioButtonQualityGood.UseVisualStyleBackColor = true;
      this.radioButtonQualityGood.CheckedChanged += new System.EventHandler(this.radioButtonQualityGood_CheckedChanged);
      // 
      // radioButtonQualityHigh
      // 
      this.radioButtonQualityHigh.AutoSize = true;
      this.radioButtonQualityHigh.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonQualityHigh.Location = new System.Drawing.Point(31, 19);
      this.radioButtonQualityHigh.Name = "radioButtonQualityHigh";
      this.radioButtonQualityHigh.Size = new System.Drawing.Size(46, 17);
      this.radioButtonQualityHigh.TabIndex = 4;
      this.radioButtonQualityHigh.TabStop = true;
      this.radioButtonQualityHigh.Text = "High";
      this.radioButtonQualityHigh.UseVisualStyleBackColor = true;
      this.radioButtonQualityHigh.CheckedChanged += new System.EventHandler(this.radioButtonQualityHigh_CheckedChanged);
      // 
      // SampleFrequency
      // 
      this.SampleFrequency.AutoSize = true;
      this.SampleFrequency.Location = new System.Drawing.Point(7, 106);
      this.SampleFrequency.Name = "SampleFrequency";
      this.SampleFrequency.Size = new System.Drawing.Size(103, 13);
      this.SampleFrequency.TabIndex = 3;
      this.SampleFrequency.Text = "Sampling Frequency";
      // 
      // AudioBitrate
      // 
      this.AudioBitrate.AutoSize = true;
      this.AudioBitrate.Location = new System.Drawing.Point(7, 32);
      this.AudioBitrate.Name = "AudioBitrate";
      this.AudioBitrate.Size = new System.Drawing.Size(140, 13);
      this.AudioBitrate.TabIndex = 2;
      this.AudioBitrate.Text = "Audio Bitrate (kbits/s x1000)";
      // 
      // comboBoxFrequency
      // 
      this.comboBoxFrequency.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxFrequency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxFrequency.DropDownWidth = 125;
      this.comboBoxFrequency.Items.AddRange(new object[]
                                              {
                                                "From Source",
                                                "44.1 kHz",
                                                "48 kHz",
                                                "32 kHz"
                                              });
      this.comboBoxFrequency.Location = new System.Drawing.Point(62, 135);
      this.comboBoxFrequency.Name = "comboBoxFrequency";
      this.comboBoxFrequency.Size = new System.Drawing.Size(125, 21);
      this.comboBoxFrequency.TabIndex = 0;
      this.comboBoxFrequency.SelectedIndexChanged += new System.EventHandler(this.comboBoxFrequency_SelectedIndexChanged);
      // 
      // comboBoxAudioBitrate
      // 
      this.comboBoxAudioBitrate.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxAudioBitrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxAudioBitrate.Items.AddRange(new object[]
                                                 {
                                                   "128",
                                                   "160",
                                                   "192",
                                                   "224",
                                                   "256",
                                                   "320",
                                                   "384"
                                                 });
      this.comboBoxAudioBitrate.Location = new System.Drawing.Point(62, 57);
      this.comboBoxAudioBitrate.Name = "comboBoxAudioBitrate";
      this.comboBoxAudioBitrate.Size = new System.Drawing.Size(125, 21);
      this.comboBoxAudioBitrate.TabIndex = 1;
      // 
      // AudioMode
      // 
      this.AudioMode.Controls.Add(this.radioButtonModeStereo);
      this.AudioMode.Controls.Add(this.radioButtonModeDualChannel);
      this.AudioMode.Controls.Add(this.radioButtonModeMono);
      this.AudioMode.Controls.Add(this.radioButtonModeJointStereo);
      this.AudioMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.AudioMode.Location = new System.Drawing.Point(223, 106);
      this.AudioMode.Name = "AudioMode";
      this.AudioMode.Size = new System.Drawing.Size(224, 79);
      this.AudioMode.TabIndex = 15;
      this.AudioMode.TabStop = false;
      this.AudioMode.Text = "Audio Mode";
      // 
      // radioButtonModeStereo
      // 
      this.radioButtonModeStereo.AutoSize = true;
      this.radioButtonModeStereo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonModeStereo.Location = new System.Drawing.Point(31, 19);
      this.radioButtonModeStereo.Name = "radioButtonModeStereo";
      this.radioButtonModeStereo.Size = new System.Drawing.Size(55, 17);
      this.radioButtonModeStereo.TabIndex = 10;
      this.radioButtonModeStereo.TabStop = true;
      this.radioButtonModeStereo.Text = "Stereo";
      this.radioButtonModeStereo.UseVisualStyleBackColor = true;
      this.radioButtonModeStereo.CheckedChanged += new System.EventHandler(this.radioButtonModeStereo_CheckedChanged);
      // 
      // radioButtonModeDualChannel
      // 
      this.radioButtonModeDualChannel.AutoSize = true;
      this.radioButtonModeDualChannel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonModeDualChannel.Location = new System.Drawing.Point(31, 42);
      this.radioButtonModeDualChannel.Name = "radioButtonModeDualChannel";
      this.radioButtonModeDualChannel.Size = new System.Drawing.Size(88, 17);
      this.radioButtonModeDualChannel.TabIndex = 12;
      this.radioButtonModeDualChannel.TabStop = true;
      this.radioButtonModeDualChannel.Text = "Dual Channel";
      this.radioButtonModeDualChannel.UseVisualStyleBackColor = true;
      this.radioButtonModeDualChannel.CheckedChanged +=
        new System.EventHandler(this.radioButtonModeDualChannel_CheckedChanged);
      // 
      // radioButtonModeMono
      // 
      this.radioButtonModeMono.AutoSize = true;
      this.radioButtonModeMono.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonModeMono.Location = new System.Drawing.Point(135, 42);
      this.radioButtonModeMono.Name = "radioButtonModeMono";
      this.radioButtonModeMono.Size = new System.Drawing.Size(51, 17);
      this.radioButtonModeMono.TabIndex = 13;
      this.radioButtonModeMono.TabStop = true;
      this.radioButtonModeMono.Text = "Mono";
      this.radioButtonModeMono.UseVisualStyleBackColor = true;
      this.radioButtonModeMono.CheckedChanged += new System.EventHandler(this.radioButtonModeMono_CheckedChanged);
      // 
      // radioButtonModeJointStereo
      // 
      this.radioButtonModeJointStereo.AutoSize = true;
      this.radioButtonModeJointStereo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonModeJointStereo.Location = new System.Drawing.Point(135, 19);
      this.radioButtonModeJointStereo.Name = "radioButtonModeJointStereo";
      this.radioButtonModeJointStereo.Size = new System.Drawing.Size(80, 17);
      this.radioButtonModeJointStereo.TabIndex = 11;
      this.radioButtonModeJointStereo.TabStop = true;
      this.radioButtonModeJointStereo.Text = "Joint Stereo";
      this.radioButtonModeJointStereo.UseVisualStyleBackColor = true;
      this.radioButtonModeJointStereo.CheckedChanged +=
        new System.EventHandler(this.radioButtonModeJointStereo_CheckedChanged);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.EncoderQuality);
      this.mpGroupBox1.Controls.Add(this.VideoFormat);
      this.mpGroupBox1.Controls.Add(this.comboBoxEncoderQuality);
      this.mpGroupBox1.Controls.Add(this.comboBoxVideoFormat);
      this.mpGroupBox1.Controls.Add(this.AspectRatio);
      this.mpGroupBox1.Controls.Add(this.comboBoxAspectRatio);
      this.mpGroupBox1.Controls.Add(this.FrameRate);
      this.mpGroupBox1.Controls.Add(this.comboBoxFrameRate);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 18);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 143);
      this.mpGroupBox1.TabIndex = 5;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Video Encoder Settings";
      // 
      // EncoderQuality
      // 
      this.EncoderQuality.AutoSize = true;
      this.EncoderQuality.Location = new System.Drawing.Point(239, 83);
      this.EncoderQuality.Name = "EncoderQuality";
      this.EncoderQuality.Size = new System.Drawing.Size(82, 13);
      this.EncoderQuality.TabIndex = 11;
      this.EncoderQuality.Text = "Encoder Quality";
      // 
      // VideoFormat
      // 
      this.VideoFormat.AutoSize = true;
      this.VideoFormat.Location = new System.Drawing.Point(239, 26);
      this.VideoFormat.Name = "VideoFormat";
      this.VideoFormat.Size = new System.Drawing.Size(69, 13);
      this.VideoFormat.TabIndex = 10;
      this.VideoFormat.Text = "Video Format";
      // 
      // comboBoxEncoderQuality
      // 
      this.comboBoxEncoderQuality.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxEncoderQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxEncoderQuality.FormattingEnabled = true;
      this.comboBoxEncoderQuality.Items.AddRange(new object[]
                                                   {
                                                     "Low",
                                                     "Medium",
                                                     "High"
                                                   });
      this.comboBoxEncoderQuality.Location = new System.Drawing.Point(296, 105);
      this.comboBoxEncoderQuality.Name = "comboBoxEncoderQuality";
      this.comboBoxEncoderQuality.Size = new System.Drawing.Size(121, 21);
      this.comboBoxEncoderQuality.TabIndex = 9;
      this.comboBoxEncoderQuality.SelectedIndexChanged +=
        new System.EventHandler(this.comboBoxEncoderQuality_SelectedIndexChanged);
      // 
      // comboBoxVideoFormat
      // 
      this.comboBoxVideoFormat.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxVideoFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxVideoFormat.FormattingEnabled = true;
      this.comboBoxVideoFormat.Items.AddRange(new object[]
                                                {
                                                  "NTSC",
                                                  "PAL",
                                                  "SECAM"
                                                });
      this.comboBoxVideoFormat.Location = new System.Drawing.Point(296, 51);
      this.comboBoxVideoFormat.Name = "comboBoxVideoFormat";
      this.comboBoxVideoFormat.Size = new System.Drawing.Size(121, 21);
      this.comboBoxVideoFormat.TabIndex = 8;
      this.comboBoxVideoFormat.SelectedIndexChanged +=
        new System.EventHandler(this.comboBoxVideoFormat_SelectedIndexChanged);
      // 
      // AspectRatio
      // 
      this.AspectRatio.AutoSize = true;
      this.AspectRatio.Location = new System.Drawing.Point(7, 83);
      this.AspectRatio.Name = "AspectRatio";
      this.AspectRatio.Size = new System.Drawing.Size(68, 13);
      this.AspectRatio.TabIndex = 7;
      this.AspectRatio.Text = "Aspect Ratio";
      // 
      // comboBoxAspectRatio
      // 
      this.comboBoxAspectRatio.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxAspectRatio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxAspectRatio.FormattingEnabled = true;
      this.comboBoxAspectRatio.Items.AddRange(new object[]
                                                {
                                                  "square",
                                                  "4 : 3",
                                                  "16 : 9"
                                                });
      this.comboBoxAspectRatio.Location = new System.Drawing.Point(62, 105);
      this.comboBoxAspectRatio.Name = "comboBoxAspectRatio";
      this.comboBoxAspectRatio.Size = new System.Drawing.Size(125, 21);
      this.comboBoxAspectRatio.TabIndex = 6;
      this.comboBoxAspectRatio.SelectedIndexChanged +=
        new System.EventHandler(this.comboBoxAspectRatio_SelectedIndexChanged);
      // 
      // FrameRate
      // 
      this.FrameRate.AutoSize = true;
      this.FrameRate.Location = new System.Drawing.Point(7, 26);
      this.FrameRate.Name = "FrameRate";
      this.FrameRate.Size = new System.Drawing.Size(62, 13);
      this.FrameRate.TabIndex = 5;
      this.FrameRate.Text = "Frame Rate";
      // 
      // comboBoxFrameRate
      // 
      this.comboBoxFrameRate.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxFrameRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxFrameRate.FormattingEnabled = true;
      this.comboBoxFrameRate.Items.AddRange(new object[]
                                              {
                                                "25.00 fps",
                                                "29.97 fps"
                                              });
      this.comboBoxFrameRate.Location = new System.Drawing.Point(62, 51);
      this.comboBoxFrameRate.Name = "comboBoxFrameRate";
      this.comboBoxFrameRate.Size = new System.Drawing.Size(125, 21);
      this.comboBoxFrameRate.TabIndex = 4;
      this.comboBoxFrameRate.SelectedIndexChanged += new System.EventHandler(this.comboBoxFrameRate_SelectedIndexChanged);
      // 
      // FiltersInterVideoEncoder
      // 
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "FiltersInterVideoEncoder";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.AudioQuality.ResumeLayout(false);
      this.AudioQuality.PerformLayout();
      this.AudioMode.ResumeLayout(false);
      this.AudioMode.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    public override void LoadSettings()
    {
      Int32 regValue;

      #region Video Settings

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\InterVideo\Common\Video"))
      {
        if (subkey != null)
        {
          try
          {
            regValue = (Int32) subkey.GetValue("VideoFrameRate", 3);
            switch (regValue)
            {
              case 3:
              case 4:
                comboBoxFrameRate.SelectedIndex = regValue - 3;
                break;
              default:
                comboBoxFrameRate.SelectedIndex = 0;
                break;
            }

            // also possible
            // if (regValue == 3 || regValue == 4) comboBoxFrameRate.SelectedIndex = regValue - 3;
            // else comboBoxFrameRate.SelectedIndex = 0;

            regValue = (Int32) subkey.GetValue("VideoAspectRatio", 2);
            switch (regValue)
            {
              case 1:
              case 2:
              case 3:
                comboBoxAspectRatio.SelectedIndex = regValue - 1;
                break;
              default:
                comboBoxAspectRatio.SelectedIndex = 1;
                break;
            }

            // if (regValue >= 1 && regValue <= 3) comboBoxAspectRatio.SelectedIndex = regValue - 1;
            // else comboBoxAspectRatio.SelectedIndex = 1;

            regValue = (Int32) subkey.GetValue("VideoFormat", 1);
            switch (regValue)
            {
              case 1:
                comboBoxVideoFormat.SelectedIndex = 1;
                break;
              case 2:
                comboBoxVideoFormat.SelectedIndex = 0;
                break;
              case 3:
                comboBoxVideoFormat.SelectedIndex = 2;
                break;
              default:
                comboBoxVideoFormat.SelectedIndex = 1;
                break;
            }
            //if (regValue == 2) comboBoxVideoFormat.SelectedIndex = 0;
            //if (regValue == 1) comboBoxVideoFormat.SelectedIndex = 1;
            //if (regValue == 3) comboBoxVideoFormat.SelectedIndex = 2;
            //else comboBoxVideoFormat.SelectedIndex = 1;

            regValue = (Int32) subkey.GetValue("VideoBitRate", 1);
            switch (regValue)
            {
              case 2000:
                comboBoxEncoderQuality.SelectedIndex = 0;
                break;
              case 4000:
                comboBoxEncoderQuality.SelectedIndex = 1;
                break;
              case 6000:
                comboBoxEncoderQuality.SelectedIndex = 2;
                break;
              default:
                comboBoxEncoderQuality.SelectedIndex = 1;
                break;
            }
            //if (regValue == 2000) comboBoxEncoderQuality.SelectedIndex = 0;
            //if (regValue == 4000) comboBoxEncoderQuality.SelectedIndex = 1;
            //if (regValue == 6000) comboBoxEncoderQuality.SelectedIndex = 2;
            //else comboBoxEncoderQuality.SelectedIndex = 1;
          }
          catch (Exception)
          {
          }
        }
      }

      #endregion

      #region Audio Settings

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\InterVideo\Common\Audio"))
      {
        if (subkey != null)
        {
          try
          {
            switch ((Int32) subkey.GetValue("MPEGAudioBitRate", 2))
            {
              case 128:
                comboBoxAudioBitrate.SelectedIndex = 0;
                break;
              case 160:
                comboBoxAudioBitrate.SelectedIndex = 1;
                break;
              case 192:
                comboBoxAudioBitrate.SelectedIndex = 2;
                break;
              case 224:
                comboBoxAudioBitrate.SelectedIndex = 3;
                break;
              case 256:
                comboBoxAudioBitrate.SelectedIndex = 4;
                break;
              case 320:
                comboBoxAudioBitrate.SelectedIndex = 5;
                break;
              case 384:
                comboBoxAudioBitrate.SelectedIndex = 6;
                break;
              default:
                comboBoxAudioBitrate.SelectedIndex = 2;
                break;
            }

            Int32 regSamplingFreq = (Int32) subkey.GetValue("MPEGAudioSamplingFreq", 0);
            switch (regSamplingFreq)
            {
              case 0:
              case 1:
              case 2:
              case 3:
                comboBoxFrequency.SelectedIndex = regSamplingFreq;
                break;
              default:
                comboBoxFrequency.SelectedIndex = 0;
                break;
            }

            Int32 regAudioQuality = (Int32) subkey.GetValue("MPEGAudioQuality", 75);
            radioButtonQualityHigh.Checked = (regAudioQuality == 100);
            radioButtonQualityGood.Checked = (regAudioQuality == 75);
            radioButtonQualityMedium.Checked = (regAudioQuality == 50);
            radioButtonQualityLow.Checked = (regAudioQuality == 25);

            Int32 regSystemMode = (Int32) subkey.GetValue("MPEGAudioSystemMode", 0);
            radioButtonModeStereo.Checked = (regSystemMode == 0);
            radioButtonModeJointStereo.Checked = (regSystemMode == 1);
            radioButtonModeDualChannel.Checked = (regSystemMode == 2);
            radioButtonModeMono.Checked = (regSystemMode == 3);
          }
          catch (Exception)
          {
          }
        }
      }

      #endregion
    }

    public override void SaveSettings()
    {
      #region Video Settings

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\InterVideo\Common\Video"))
      {
        if (subkey != null)
        {
          // Framerate - 25 or 29.97 fps
          subkey.SetValue("VideoFrameRate", _regFrameRate + 3);

          // Aspect Ratio - square, 4:3, 16:9
          subkey.SetValue("VideoAspectRatio", _regAspectRatio);

          // Video Format - NTSC, PAL, SECAM
          int Format;
          Int32 regValue;
          Format = comboBoxVideoFormat.SelectedIndex;
          if (Format == 0)
          {
            regValue = 2;
            subkey.SetValue("VideoFormat", regValue);
            // Force frame rate to 29.97 regardless
            subkey.SetValue("VideoFrameRate", 4);
          }
          if (Format == 1)
          {
            regValue = 1;
            subkey.SetValue("VideoFormat", regValue);
            // Force frame rate to 25 regardless
            subkey.SetValue("VideoFrameRate", 3);
          }
          if (Format == 2)
          {
            regValue = 3;
            subkey.SetValue("VideoFormat", regValue);
            // Force frame rate to 25 regardless
            subkey.SetValue("VideoFrameRate", 3);
          }

          // Encoder Quality - Low, Medium, High
          subkey.SetValue("VideoBitRate", _regVideoBitRate);
          subkey.SetValue("VideoPeakBitRate", _regVideoPeakBitRate);

          // Force Variable BitRate
          subkey.SetValue("VideoBitRateMode", 2);

          // Force Raltime Encoding - essential for on the fly encoding.
          subkey.SetValue("VideoRealtimeEncode", 1);

          // Force MPEG-2 Video - essential for DVR-MS
          subkey.SetValue("VideoMPEG2", 1);

          // Force each GOP
          subkey.SetValue("VideoSeqHdr", 1);

          // Force Width & Height based on format chosen. i.e. we set it to 720x576 for PAL.
          // The filter should adjust based on input framesize however.
          if (comboBoxVideoFormat.SelectedIndex == 0)
          {
            subkey.SetValue("VideoWidth", 720);
            subkey.SetValue("VideoHeight", 480);
          }
          if (comboBoxVideoFormat.SelectedIndex >= 1)
          {
            subkey.SetValue("VideoWidth", 720);
            subkey.SetValue("VideoHeight", 576);
          }
        }
      }

      #endregion

      #region Audio Settings

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\InterVideo\Common\Audio"))
      {
        if (subkey != null)
        {
          // Audio BitRate - 128 - 384 kbit/s
          int AudioBitrate;
          Int32 regValue;
          AudioBitrate = comboBoxAudioBitrate.SelectedIndex;
          if (AudioBitrate == 0)
          {
            regValue = 128;
            subkey.SetValue("MPEGAudioBitRate", regValue);
          }
          if (AudioBitrate == 1)
          {
            regValue = 160;
            subkey.SetValue("MPEGAudioBitRate", regValue);
          }
          if (AudioBitrate == 2)
          {
            regValue = 192;
            subkey.SetValue("MPEGAudioBitRate", regValue);
          }
          if (AudioBitrate == 3)
          {
            regValue = 224;
            subkey.SetValue("MPEGAudioBitRate", regValue);
          }
          if (AudioBitrate == 4)
          {
            regValue = 256;
            subkey.SetValue("MPEGAudioBitRate", regValue);
          }
          if (AudioBitrate == 5)
          {
            regValue = 320;
            subkey.SetValue("MPEGAudioBitRate", regValue);
          }
          if (AudioBitrate == 6)
          {
            regValue = 384;
            subkey.SetValue("MPEGAudioBitRate", regValue);
          }

          // Sample Frequency - from source, 44.1, 48 or 32 kHz
          subkey.SetValue("MPEGAudioSamplingFreq", _regFrequecy);

          // Audio Quality - Low, Medium, Good & High
          //regQuality = 75;				// default to Medium Quality
          subkey.SetValue("MPEGAudioQuality", _regQuality);

          // Audio Channels - Stereo, Joint Stereo, Dual Channel or Mono
          //regMode = 0;
          subkey.SetValue("MPEGAudioSystemMode", _regMode);

          // Force MPEG-1 Layer II for compatibility - encoder is capable of Layer III, LPCM or AC3
          subkey.SetValue("MPEGAudioSystemLayer", 2);

          // Force Realtime Encoding - essential for speed.
          subkey.SetValue("MPEGAudioRealtimeEncode", 1);
          subkey.SetValue("MPEGAudioForceNonRealtime", 0);
        }
      }

      #endregion

      #region Mux

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\InterVideo\Common\Mux"))
      {
        if (subkey != null)
        {
          // Here we just ensure the InterVideo Mux is set to MPEG2 & Realtime Encoding
          subkey.SetValue("MuxMpeg2Mode", 1);
          subkey.SetValue("MuxRealtimeEncode", 1);
        }
      }

      #endregion
    }

    private void comboBoxFrameRate_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (comboBoxFrameRate.SelectedIndex == 0)
      {
        _regFrameRate = 3;
      }

      if (comboBoxFrameRate.SelectedIndex == 1)
      {
        _regFrameRate = 4;
      }
    }

    private void comboBoxVideoFormat_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void comboBoxAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
    {
      _regAspectRatio = comboBoxAspectRatio.SelectedIndex + 1;
    }

    private void comboBoxEncoderQuality_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (comboBoxEncoderQuality.SelectedIndex)
      {
        case 0:
          _regVideoBitRate = 2000;
          _regVideoPeakBitRate = 4000;
          break;
        case 1:
          _regVideoBitRate = 4000;
          _regVideoPeakBitRate = 6000;
          break;
        case 2:
          _regVideoBitRate = 6000;
          _regVideoPeakBitRate = 8000;
          break;
        default:
          _regVideoBitRate = 4000;
          _regVideoPeakBitRate = 6000;
          break;
      }
    }

    private void comboBoxFrequency_SelectedIndexChanged(object sender, EventArgs e)
    {
      _regFrequecy = comboBoxFrequency.SelectedIndex;
    }

    private void radioButtonQualityHigh_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonQualityHigh.Checked)
      {
        _regQuality = 100;
      }
    }

    private void radioButtonQualityGood_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonQualityGood.Checked)
      {
        _regQuality = 75;
      }
    }

    private void radioButtonQualityMedium_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonQualityMedium.Checked)
      {
        _regQuality = 50;
      }
    }

    private void radioButtonQualityLow_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonQualityLow.Checked)
      {
        _regQuality = 25;
      }
    }

    private void radioButtonModeStereo_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonModeStereo.Checked)
      {
        _regMode = 0;
      }
    }

    private void radioButtonModeJointStereo_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonModeJointStereo.Checked)
      {
        _regMode = 1;
      }
    }

    private void radioButtonModeDualChannel_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonModeDualChannel.Checked)
      {
        _regMode = 2;
      }
    }

    private void radioButtonModeMono_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButtonModeMono.Checked)
      {
        _regMode = 3;
      }
    }
  }
}