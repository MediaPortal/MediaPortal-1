#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.ComponentModel;
using MediaPortal.UserInterface.Controls;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class FiltersDScalerVideo : SectionSettings
  {
    private MPGroupBox groupBox1;
    private MPLabel label3;
    private MPLabel label5;
    private MPLabel label2;
    private MPLabel label4;
    private MPLabel label6;
    private MPTextBox tbVideoOffset;
    private MPCheckBox cbAspectRatio;
    private MPCheckBox cbForcedSubtitles;
    private MPComboBox cbDeinterlace;
    private MPCheckBox cbSmoothing;
    private MPComboBox cbDVBAR;
    private MPCheckBox cbHardcodePal;
    private MPComboBox cbIDCT;
    private MPComboBox cbColorSpace;
    private MPCheckBox cbAnalogBlanking;
    private IContainer components = null;

    /// <summary>
    /// 
    /// </summary>
    public FiltersDScalerVideo()
      : this("DScaler Video Decoder") {}

    /// <summary>
    /// 
    /// </summary>
    public FiltersDScalerVideo(string name)
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
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbAnalogBlanking = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbColorSpace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbIDCT = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbHardcodePal = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbDVBAR = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbVideoOffset = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.cbAspectRatio = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbForcedSubtitles = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbDeinterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.cbSmoothing = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.cbAnalogBlanking);
      this.groupBox1.Controls.Add(this.cbColorSpace);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.cbIDCT);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.cbHardcodePal);
      this.groupBox1.Controls.Add(this.cbDVBAR);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.tbVideoOffset);
      this.groupBox1.Controls.Add(this.cbAspectRatio);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.cbForcedSubtitles);
      this.groupBox1.Controls.Add(this.cbDeinterlace);
      this.groupBox1.Controls.Add(this.cbSmoothing);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 288);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // cbAnalogBlanking
      // 
      this.cbAnalogBlanking.Checked = true;
      this.cbAnalogBlanking.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAnalogBlanking.Location = new System.Drawing.Point(16, 224);
      this.cbAnalogBlanking.Name = "cbAnalogBlanking";
      this.cbAnalogBlanking.Size = new System.Drawing.Size(136, 16);
      this.cbAnalogBlanking.TabIndex = 12;
      this.cbAnalogBlanking.Text = "Do analog blanking";
      // 
      // cbColorSpace
      // 
      this.cbColorSpace.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.cbColorSpace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbColorSpace.Items.AddRange(new object[]
                                         {
                                           "YV12",
                                           "YUY2"
                                         });
      this.cbColorSpace.Location = new System.Drawing.Point(168, 92);
      this.cbColorSpace.Name = "cbColorSpace";
      this.cbColorSpace.Size = new System.Drawing.Size(288, 21);
      this.cbColorSpace.TabIndex = 7;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 96);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(128, 16);
      this.label6.TabIndex = 6;
      this.label6.Text = "Output colorspace:";
      // 
      // cbIDCT
      // 
      this.cbIDCT.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.cbIDCT.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbIDCT.Items.AddRange(new object[]
                                   {
                                     "Reference",
                                     "MMX only",
                                     "Accelerated"
                                   });
      this.cbIDCT.Location = new System.Drawing.Point(168, 68);
      this.cbIDCT.Name = "cbIDCT";
      this.cbIDCT.Size = new System.Drawing.Size(288, 21);
      this.cbIDCT.TabIndex = 5;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 72);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(128, 16);
      this.label4.TabIndex = 4;
      this.label4.Text = "IDCT to use:";
      // 
      // cbHardcodePal
      // 
      this.cbHardcodePal.Location = new System.Drawing.Point(16, 200);
      this.cbHardcodePal.Name = "cbHardcodePal";
      this.cbHardcodePal.Size = new System.Drawing.Size(184, 16);
      this.cbHardcodePal.TabIndex = 11;
      this.cbHardcodePal.Text = "Hardcode for PAL with FFdshow";
      // 
      // cbDVBAR
      // 
      this.cbDVBAR.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDVBAR.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDVBAR.Items.AddRange(new object[]
                                    {
                                      "16:9 Display",
                                      "4:3 Display Center cut out",
                                      "4:3 Display Letterbox"
                                    });
      this.cbDVBAR.Location = new System.Drawing.Point(168, 44);
      this.cbDVBAR.Name = "cbDVBAR";
      this.cbDVBAR.Size = new System.Drawing.Size(288, 21);
      this.cbDVBAR.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(128, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "DVB Aspect preference:";
      // 
      // tbVideoOffset
      // 
      this.tbVideoOffset.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.tbVideoOffset.Location = new System.Drawing.Point(168, 252);
      this.tbVideoOffset.Name = "tbVideoOffset";
      this.tbVideoOffset.Size = new System.Drawing.Size(288, 20);
      this.tbVideoOffset.TabIndex = 14;
      this.tbVideoOffset.Text = "0";
      // 
      // cbAspectRatio
      // 
      this.cbAspectRatio.Checked = true;
      this.cbAspectRatio.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbAspectRatio.Location = new System.Drawing.Point(16, 176);
      this.cbAspectRatio.Name = "cbAspectRatio";
      this.cbAspectRatio.Size = new System.Drawing.Size(168, 16);
      this.cbAspectRatio.TabIndex = 10;
      this.cbAspectRatio.Text = "Use accurate Aspect Ratios";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 256);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(144, 16);
      this.label5.TabIndex = 13;
      this.label5.Text = "Video delay offset (msec.):";
      // 
      // cbForcedSubtitles
      // 
      this.cbForcedSubtitles.Checked = true;
      this.cbForcedSubtitles.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbForcedSubtitles.Location = new System.Drawing.Point(16, 128);
      this.cbForcedSubtitles.Name = "cbForcedSubtitles";
      this.cbForcedSubtitles.Size = new System.Drawing.Size(136, 16);
      this.cbForcedSubtitles.TabIndex = 8;
      this.cbForcedSubtitles.Text = "Display Forced Subtitles";
      // 
      // cbDeinterlace
      // 
      this.cbDeinterlace.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDeinterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDeinterlace.Items.AddRange(new object[]
                                          {
                                            "Automatic",
                                            "Force Weave",
                                            "Force Bob"
                                          });
      this.cbDeinterlace.Location = new System.Drawing.Point(168, 20);
      this.cbDeinterlace.Name = "cbDeinterlace";
      this.cbDeinterlace.Size = new System.Drawing.Size(288, 21);
      this.cbDeinterlace.TabIndex = 1;
      // 
      // cbSmoothing
      // 
      this.cbSmoothing.Checked = true;
      this.cbSmoothing.CheckState = System.Windows.Forms.CheckState.Checked;
      this.cbSmoothing.Location = new System.Drawing.Point(16, 152);
      this.cbSmoothing.Name = "cbSmoothing";
      this.cbSmoothing.Size = new System.Drawing.Size(136, 16);
      this.cbSmoothing.TabIndex = 9;
      this.cbSmoothing.Text = "3:2 Playback smoothing";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 24);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(96, 16);
      this.label3.TabIndex = 0;
      this.label3.Text = "Deinterlace mode:";
      // 
      // DScalerVideoFilter
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "DScalerVideoFilter";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    public override void LoadSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\DScaler5\MpegVideo Filter"))
      {
        if (subkey != null)
        {
          try
          {
            Int32 regValue = (Int32)subkey.GetValue("3:2 playback smoothing");
            if (regValue == 1)
            {
              cbSmoothing.Checked = true;
            }
            else
            {
              cbSmoothing.Checked = false;
            }

            regValue = (Int32)subkey.GetValue("Display Forced Subtitles");
            if (regValue == 1)
            {
              cbForcedSubtitles.Checked = true;
            }
            else
            {
              cbForcedSubtitles.Checked = false;
            }

            regValue = (Int32)subkey.GetValue("Use accurate aspect ratios");
            if (regValue == 1)
            {
              cbAspectRatio.Checked = true;
            }
            else
            {
              cbAspectRatio.Checked = false;
            }

            regValue = (Int32)subkey.GetValue("Hardcode for PAL with ffdshow");
            if (regValue == 1)
            {
              cbHardcodePal.Checked = true;
            }
            else
            {
              cbHardcodePal.Checked = false;
            }

            regValue = (Int32)subkey.GetValue("Do Analog Blanking");
            if (regValue == 1)
            {
              cbAnalogBlanking.Checked = true;
            }
            else
            {
              cbAnalogBlanking.Checked = false;
            }

            regValue = (Int32)subkey.GetValue("Video Delay");
            tbVideoOffset.Text = regValue.ToString();

            regValue = (Int32)subkey.GetValue("Deinterlace Mode");
            cbDeinterlace.SelectedIndex = regValue;

            regValue = (Int32)subkey.GetValue("DVB Aspect Preferences");
            cbDVBAR.SelectedIndex = regValue;

            regValue = (Int32)subkey.GetValue("IDCT to Use");
            cbIDCT.SelectedIndex = regValue;

            regValue = (Int32)subkey.GetValue("Colour space to output");
            cbColorSpace.SelectedIndex = regValue;
          }
          catch (Exception) {}
        }
      }
    }

    public override void SaveSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\DScaler5\MpegVideo Filter"))
      {
        if (subkey != null)
        {
          Int32 regValue;
          if (cbSmoothing.Checked)
          {
            regValue = 1;
          }
          else
          {
            regValue = 0;
          }
          subkey.SetValue("3:2 playback smoothing", regValue);

          if (cbForcedSubtitles.Checked)
          {
            regValue = 1;
          }
          else
          {
            regValue = 0;
          }
          subkey.SetValue("Display Forced Subtitles", regValue);

          if (cbAspectRatio.Checked)
          {
            regValue = 1;
          }
          else
          {
            regValue = 0;
          }
          subkey.SetValue("Use accurate aspect ratios", regValue);

          if (cbHardcodePal.Checked)
          {
            regValue = 1;
          }
          else
          {
            regValue = 0;
          }
          subkey.SetValue("Hardcode for PAL with ffdshow", regValue);

          if (cbAnalogBlanking.Checked)
          {
            regValue = 1;
          }
          else
          {
            regValue = 0;
          }
          subkey.SetValue("Do Analog Blanking", regValue);

          regValue = (Int32)Int32.Parse(tbVideoOffset.Text);
          subkey.SetValue("Video Delay", regValue);

          subkey.SetValue("Deinterlace Mode", (Int32)cbDeinterlace.SelectedIndex);

          subkey.SetValue("DVB Aspect Preferences", (Int32)cbDVBAR.SelectedIndex);

          subkey.SetValue("IDCT to Use", (Int32)cbIDCT.SelectedIndex);

          subkey.SetValue("Colour space to output", (Int32)cbColorSpace.SelectedIndex);
        }
      }
    }
  }
}