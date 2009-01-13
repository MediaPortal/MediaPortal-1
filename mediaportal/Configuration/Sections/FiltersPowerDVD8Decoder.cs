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
using System.ComponentModel;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class FiltersPowerDVD8Decoder : SectionSettings
  {
    private MPComboBox comboBoxSpeakerConfig;
    private MPComboBox comboBoxOutPutMode;
    private MPGroupBox mpGroupBox1;
    private MPLabel mpLabel1;
    private MPLabel mpLabel2;
    private MPGroupBox mpGroupBox2;
    private MPComboBox comboBoxDeInterlace;
    private MPLabel mpLabel4;
    private MPLabel mpLabel3;
    private MPCheckBox checkBoxUIUseHVA;
    private MPComboBox comboBoxH264DeInterlace;
    private MPLabel mpLabel6;
    private MPGroupBox mpGroupBox3;
    private MPLabel mpLabel5;
    private MPCheckBox checkBoxUIUseH264HVA;
    private IContainer components = null;

    /// <summary>
    /// 
    /// </summary>
    public FiltersPowerDVD8Decoder()
      : this("PowerDVD 8 Decoder Settings")
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public FiltersPowerDVD8Decoder(string name)
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
      this.comboBoxDeInterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxUIUseHVA = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxSpeakerConfig = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxOutPutMode = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxH264DeInterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxUIUseH264HVA = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Controls.Add(this.comboBoxDeInterlace);
      this.mpGroupBox2.Controls.Add(this.mpLabel4);
      this.mpGroupBox2.Controls.Add(this.mpLabel3);
      this.mpGroupBox2.Controls.Add(this.checkBoxUIUseHVA);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(0, 6);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(472, 118);
      this.mpGroupBox2.TabIndex = 3;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "MPEG-2 Video Decoder Settings";
      // 
      // comboBoxDeInterlace
      // 
      this.comboBoxDeInterlace.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxDeInterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDeInterlace.FormattingEnabled = true;
      this.comboBoxDeInterlace.Items.AddRange(new object[]
                                                {
                                                  "Auto-select",
                                                  "Force bob",
                                                  "Force weave"
                                                });
      this.comboBoxDeInterlace.Location = new System.Drawing.Point(52, 85);
      this.comboBoxDeInterlace.Name = "comboBoxDeInterlace";
      this.comboBoxDeInterlace.Size = new System.Drawing.Size(198, 21);
      this.comboBoxDeInterlace.TabIndex = 3;
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(22, 60);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(104, 13);
      this.mpLabel4.TabIndex = 2;
      this.mpLabel4.Text = "De-Interlace Options";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(188, 21);
      this.mpLabel3.MaximumSize = new System.Drawing.Size(270, 0);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(256, 26);
      this.mpLabel3.TabIndex = 1;
      this.mpLabel3.Text = "Recommended to reduce CPU utilization. If selected De-Interlace options are contr" +
                           "olled by VMR9.";
      // 
      // checkBoxUIUseHVA
      // 
      this.checkBoxUIUseHVA.AutoSize = true;
      this.checkBoxUIUseHVA.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUIUseHVA.Location = new System.Drawing.Point(22, 30);
      this.checkBoxUIUseHVA.Name = "checkBoxUIUseHVA";
      this.checkBoxUIUseHVA.Size = new System.Drawing.Size(149, 17);
      this.checkBoxUIUseHVA.TabIndex = 0;
      this.checkBoxUIUseHVA.Text = "Use Hardware Accelerator";
      this.checkBoxUIUseHVA.UseVisualStyleBackColor = true;
      this.checkBoxUIUseHVA.CheckedChanged += new System.EventHandler(this.UIUseHVA_CheckedChanged);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.comboBoxSpeakerConfig);
      this.mpGroupBox1.Controls.Add(this.comboBoxOutPutMode);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 255);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 144);
      this.mpGroupBox1.TabIndex = 2;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Audio Decoder Settings";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(19, 81);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(69, 13);
      this.mpLabel2.TabIndex = 3;
      this.mpLabel2.Text = "Output Mode";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(19, 26);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(109, 13);
      this.mpLabel1.TabIndex = 2;
      this.mpLabel1.Text = "Speaker Environment";
      // 
      // comboBoxSpeakerConfig
      // 
      this.comboBoxSpeakerConfig.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSpeakerConfig.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxSpeakerConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSpeakerConfig.Items.AddRange(new object[]
                                                  {
                                                    "Headphones",
                                                    "S/PDIF Out",
                                                    "2 Speaker",
                                                    "4 Speaker",
                                                    "6 Speaker",
                                                    "7 Speaker",
                                                    "8 Speaker"
                                                  });
      this.comboBoxSpeakerConfig.Location = new System.Drawing.Point(52, 48);
      this.comboBoxSpeakerConfig.Name = "comboBoxSpeakerConfig";
      this.comboBoxSpeakerConfig.Size = new System.Drawing.Size(375, 21);
      this.comboBoxSpeakerConfig.TabIndex = 1;
      this.comboBoxSpeakerConfig.SelectedIndexChanged +=
        new System.EventHandler(this.comboBoxSpeakerConfig_SelectedIndexChanged);
      // 
      // comboBoxOutPutMode
      // 
      this.comboBoxOutPutMode.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxOutPutMode.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxOutPutMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxOutPutMode.DropDownWidth = 375;
      this.comboBoxOutPutMode.Location = new System.Drawing.Point(52, 102);
      this.comboBoxOutPutMode.Name = "comboBoxOutPutMode";
      this.comboBoxOutPutMode.Size = new System.Drawing.Size(375, 21);
      this.comboBoxOutPutMode.TabIndex = 0;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Controls.Add(this.comboBoxH264DeInterlace);
      this.mpGroupBox3.Controls.Add(this.mpLabel6);
      this.mpGroupBox3.Controls.Add(this.mpLabel5);
      this.mpGroupBox3.Controls.Add(this.checkBoxUIUseH264HVA);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(0, 130);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(472, 119);
      this.mpGroupBox3.TabIndex = 4;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "H.264 Video Decoder Settings";
      // 
      // comboBoxH264DeInterlace
      // 
      this.comboBoxH264DeInterlace.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxH264DeInterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxH264DeInterlace.FormattingEnabled = true;
      this.comboBoxH264DeInterlace.Items.AddRange(new object[]
                                                    {
                                                      "Auto-select",
                                                      "Force bob",
                                                      "Force weave"
                                                    });
      this.comboBoxH264DeInterlace.Location = new System.Drawing.Point(52, 87);
      this.comboBoxH264DeInterlace.Name = "comboBoxH264DeInterlace";
      this.comboBoxH264DeInterlace.Size = new System.Drawing.Size(198, 21);
      this.comboBoxH264DeInterlace.TabIndex = 5;
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(22, 62);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(104, 13);
      this.mpLabel6.TabIndex = 4;
      this.mpLabel6.Text = "De-Interlace Options";
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(188, 21);
      this.mpLabel5.MaximumSize = new System.Drawing.Size(270, 0);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(256, 26);
      this.mpLabel5.TabIndex = 4;
      this.mpLabel5.Text = "Recommended to reduce CPU utilization. If selected De-Interlace options are contr" +
                           "olled by VMR9.";
      // 
      // checkBoxUIUseH264HVA
      // 
      this.checkBoxUIUseH264HVA.AutoSize = true;
      this.checkBoxUIUseH264HVA.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUIUseH264HVA.Location = new System.Drawing.Point(22, 30);
      this.checkBoxUIUseH264HVA.Name = "checkBoxUIUseH264HVA";
      this.checkBoxUIUseH264HVA.Size = new System.Drawing.Size(149, 17);
      this.checkBoxUIUseH264HVA.TabIndex = 4;
      this.checkBoxUIUseH264HVA.Text = "Use Hardware Accelerator";
      this.checkBoxUIUseH264HVA.UseVisualStyleBackColor = true;
      this.checkBoxUIUseH264HVA.CheckedChanged += new System.EventHandler(this.mpCheckBox1_CheckedChanged);
      // 
      // PowerDVD7DecoderFilters
      // 
      this.Controls.Add(this.mpGroupBox3);
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "PowerDVD7DecoderFilters";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.ResumeLayout(false);
    }

    #endregion

    public override void LoadSettings()
    {
      int iIndex = 0;
      Int32 regValue;
      Int32 regAuDsDnmx;
      Int32 regAuDsChanExpand;
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\CLAud\MediaPortal"))
      {
        if (subkey != null)
        {
          try
          {
            regValue = (Int32) subkey.GetValue("AuDsInterface", 8);
            regAuDsDnmx = (Int32) subkey.GetValue("AuDsDnmx", 2);
            regAuDsChanExpand = (Int32) subkey.GetValue("AuDsChanExpand", -1);
            switch (regValue)
            {
                // Headphones (comboBoxSpeakerConfig index 0)
              case 20000:

                #region Headphones

                comboBoxSpeakerConfig.SelectedIndex = 0;
                comboBoxOutPutMode.Items.Clear();
                comboBoxOutPutMode.Enabled = true;
                comboBoxOutPutMode.Items.AddRange(new object[]
                                                    {
                                                      "Stereo",
                                                      "Dolby Surround Downmix",
                                                      "Dolby Headphone",
                                                      "TruSurroundXT Headphone",
                                                      "CyberLink Headphone"
                                                    });
                if (regAuDsDnmx == 2)
                {
                  iIndex = 0;
                }
                if (regAuDsDnmx == 8)
                {
                  iIndex = 1;
                }
                if (regAuDsDnmx == 200)
                {
                  iIndex = 2;
                }
                if (regAuDsDnmx == 4000)
                {
                  iIndex = 3;
                }
                if (regAuDsDnmx == 8000)
                {
                  iIndex = 4;
                }
                comboBoxOutPutMode.SelectedIndex = iIndex;

                #endregion

                break;
                //SPDIF Out (comboBoxSpeakerConfig index 1)
              case 4:

                #region SPDIF

                comboBoxSpeakerConfig.SelectedIndex = 1;
                comboBoxOutPutMode.Items.Clear();
                comboBoxOutPutMode.Enabled = false;

                #endregion

                break;
                //2 Speaker (comboBoxSpeakerConfig index 2)
              case 8:

                #region 2 Speaker

                comboBoxSpeakerConfig.SelectedIndex = 2;
                comboBoxOutPutMode.Items.Clear();
                comboBoxOutPutMode.Enabled = true;
                comboBoxOutPutMode.Items.AddRange(new object[]
                                                    {
                                                      "Stereo",
                                                      "Dolby Surround Compatible Downmix",
                                                      "TruSurroundXT",
                                                      "Dolby Virtual Speaker",
                                                      "CyberLink Virtual Speaker"
                                                    });
                if (regAuDsDnmx == 2)
                {
                  iIndex = 0;
                }
                if (regAuDsDnmx == 8)
                {
                  iIndex = 1;
                }
                if (regAuDsDnmx == 400)
                {
                  iIndex = 2;
                }
                if (regAuDsDnmx == 2000)
                {
                  iIndex = 3;
                }
                if (regAuDsDnmx == 10000)
                {
                  iIndex = 4;
                }
                comboBoxOutPutMode.SelectedIndex = iIndex;

                #endregion

                break;
                //4 Speaker (comboBoxSpeakerConfig index 3)
              case 10:

                #region 4 speaker

                comboBoxSpeakerConfig.SelectedIndex = 3;
                comboBoxOutPutMode.Items.Clear();
                comboBoxOutPutMode.Enabled = true;
                comboBoxOutPutMode.Items.AddRange(new object[]
                                                    {
                                                      "No Effect",
                                                      "Dolby Pro Logic IIx",
                                                      "CLMEI-2"
                                                    });
                if (regAuDsChanExpand == 1)
                {
                  iIndex = 0;
                }
                if (regAuDsChanExpand == 2)
                {
                  iIndex = 1;
                }
                if (regAuDsChanExpand == 4)
                {
                  iIndex = 2;
                }
                comboBoxOutPutMode.SelectedIndex = iIndex;

                #endregion

                break;
                //6 Speaker (comboBoxSpeakerConfig index 4)
              case 20:

                #region 6 speaker

                comboBoxSpeakerConfig.SelectedIndex = 4;
                comboBoxOutPutMode.Items.Clear();
                comboBoxOutPutMode.Enabled = true;
                comboBoxOutPutMode.Items.AddRange(new object[]
                                                    {
                                                      "No Effect",
                                                      "Dolby Pro Logic IIx",
                                                      "CLMEI-2"
                                                    });
                if (regAuDsChanExpand == 1)
                {
                  iIndex = 0;
                }
                if (regAuDsChanExpand == 2)
                {
                  iIndex = 1;
                }
                if (regAuDsChanExpand == 4)
                {
                  iIndex = 2;
                }
                comboBoxOutPutMode.SelectedIndex = iIndex;

                #endregion

                break;
                //7 Speaker (comboBoxSpeakerConfig index 5)
              case 40:

                #region 7 speaker

                comboBoxSpeakerConfig.SelectedIndex = 5;
                comboBoxOutPutMode.Items.Clear();
                comboBoxOutPutMode.Enabled = true;
                comboBoxOutPutMode.Items.AddRange(new object[]
                                                    {
                                                      "No Effect",
                                                      "Dolby Pro Logic IIx",
                                                      "CLMEI-2"
                                                    });
                if (regAuDsChanExpand == 1)
                {
                  iIndex = 0;
                }
                if (regAuDsChanExpand == 2)
                {
                  iIndex = 1;
                }
                if (regAuDsChanExpand == 4)
                {
                  iIndex = 2;
                }
                comboBoxOutPutMode.SelectedIndex = iIndex;

                #endregion

                break;
                //8 Speaker (comboBoxSpeakerConfig index 6)
              case 80:

                #region 8 speaker

                comboBoxSpeakerConfig.SelectedIndex = 6;
                comboBoxOutPutMode.Items.Clear();
                comboBoxOutPutMode.Enabled = true;
                comboBoxOutPutMode.Items.AddRange(new object[]
                                                    {
                                                      "No Effect",
                                                      "Dolby Pro Logic IIx",
                                                      "CLMEI-2"
                                                    });
                if (regAuDsChanExpand == 1)
                {
                  iIndex = 0;
                }
                if (regAuDsChanExpand == 2)
                {
                  iIndex = 1;
                }
                if (regAuDsChanExpand == 4)
                {
                  iIndex = 2;
                }
                comboBoxOutPutMode.SelectedIndex = iIndex;

                #endregion

                break;
            }
          }
          catch (Exception)
          {
          }
        }
      }

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\CLVSD\MediaPortal"))
      {
        if (subkey != null)
        {
          try
          {
            Int32 regUIUseHVA = (Int32) subkey.GetValue("UIUseHVA", 1);
            if (regUIUseHVA == 1)
            {
              checkBoxUIUseHVA.Checked = true;
            }
            else
            {
              checkBoxUIUseHVA.Checked = false;
            }
            Int32 regUIVMode = (Int32) subkey.GetValue("UIVMode", 0);
            if (regUIVMode == 1)
            {
              comboBoxDeInterlace.SelectedIndex = 0;
            }
            if (regUIVMode == 2)
            {
              comboBoxDeInterlace.SelectedIndex = 1;
            }
            if (regUIVMode == 3)
            {
              comboBoxDeInterlace.SelectedIndex = 2;
            }
            else
            {
              comboBoxDeInterlace.SelectedIndex = 0;
            }
          }
          catch (Exception)
          {
          }
        }
      }

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\cl264dec\MediaPortal"))
      {
        if (subkey != null)
        {
          try
          {
            Int32 regH264UIUseHVA = (Int32) subkey.GetValue("UIUseHVA", 1);
            if (regH264UIUseHVA == 1)
            {
              checkBoxUIUseH264HVA.Checked = true;
            }
            else
            {
              checkBoxUIUseH264HVA.Checked = false;
            }
            Int32 regH264UIVMode = (Int32) subkey.GetValue("UIVMode", 0);
            if (regH264UIVMode == 1)
            {
              comboBoxH264DeInterlace.SelectedIndex = 0;
            }
            if (regH264UIVMode == 2)
            {
              comboBoxH264DeInterlace.SelectedIndex = 1;
            }
            if (regH264UIVMode == 3)
            {
              comboBoxH264DeInterlace.SelectedIndex = 2;
            }
            else
            {
              comboBoxH264DeInterlace.SelectedIndex = 0;
            }
          }
          catch (Exception)
          {
          }
        }
      }
    }

    public override void SaveSettings()
    {
      Int32 regValue;
      int OutPutModeIndex;
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\CLAud\MediaPortal"))
      {
        if (subkey != null)
        {
          switch (comboBoxSpeakerConfig.SelectedIndex)
          {
              // Headphones
            case 0:

              #region Headphones

              regValue = 20000;
              subkey.SetValue("AuDsInterface", regValue);
              OutPutModeIndex = comboBoxOutPutMode.SelectedIndex;
              if (OutPutModeIndex == 0)
              {
                regValue = 2;
                subkey.SetValue("AuDsDnmx", regValue);
                regValue = 1;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              if (OutPutModeIndex == 1)
              {
                regValue = 8;
                subkey.SetValue("AuDsDnmx", regValue);
              }
              if (OutPutModeIndex == 2)
              {
                regValue = 200;
                subkey.SetValue("AuDsDnmx", regValue);
              }
              if (OutPutModeIndex == 3)
              {
                regValue = 4000;
                subkey.SetValue("AuDsDnmx", regValue);
              }
              if (OutPutModeIndex == 4)
              {
                regValue = 8000;
                subkey.SetValue("AuDsDnmx", regValue);
              }

              #endregion

              break;
              // SPDIF Out 
            case 1:

              #region SPDIF

              regValue = 4;
              subkey.SetValue("AuDsInterface", regValue);
              regValue = 2;
              subkey.SetValue("AuDsDnmx", regValue);

              #endregion

              break;
              //2 Speakers
            case 2:

              #region 2 Speakers

              regValue = 8;
              subkey.SetValue("AuDsInterface", regValue);
              OutPutModeIndex = comboBoxOutPutMode.SelectedIndex;
              if (OutPutModeIndex == 0)
              {
                regValue = 2;
                subkey.SetValue("AuDsDnmx", regValue);
                regValue = 1;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              if (OutPutModeIndex == 1)
              {
                regValue = 8;
                subkey.SetValue("AuDsDnmx", regValue);
              }
              if (OutPutModeIndex == 2)
              {
                regValue = 400;
                subkey.SetValue("AuDsDnmx", regValue);
              }
              if (OutPutModeIndex == 3)
              {
                regValue = 2000;
                subkey.SetValue("AuDsDnmx", regValue);
              }
              if (OutPutModeIndex == 4)
              {
                regValue = 10000;
                subkey.SetValue("AuDsDnmx", regValue);
              }

              #endregion

              break;
              //4 Speakers
            case 3:

              #region 4 Speakers

              regValue = 10;
              subkey.SetValue("AuDsInterface", regValue);
              regValue = 40;
              subkey.SetValue("AuDsDnmx", regValue);

              OutPutModeIndex = comboBoxOutPutMode.SelectedIndex;
              //No effect 
              if (OutPutModeIndex == 0)
              {
                regValue = 1;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              //Dolby Prologic 2
              if (OutPutModeIndex == 1)
              {
                regValue = 2;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              //CLMEI-2
              if (OutPutModeIndex == 2)
              {
                regValue = 4;
                subkey.SetValue("AuDsChanExpand", regValue);
              }

              #endregion

              break;
              //6 Speakers
            case 4:

              #region 6 Speakers

              regValue = 20;
              subkey.SetValue("AuDsInterface", regValue);
              regValue = 80;
              subkey.SetValue("AuDsDnmx", regValue);

              OutPutModeIndex = comboBoxOutPutMode.SelectedIndex;
              //No effect 
              if (OutPutModeIndex == 0)
              {
                regValue = 1;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              //Dolby Prologic 2
              if (OutPutModeIndex == 1)
              {
                regValue = 2;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              //CLMEI-2
              if (OutPutModeIndex == 2)
              {
                regValue = 4;
                subkey.SetValue("AuDsChanExpand", regValue);
              }

              #endregion

              break;
              //7 Speakers
            case 5:

              #region 7 Speakers

              regValue = 40;
              subkey.SetValue("AuDsInterface", regValue);
              regValue = 800;
              subkey.SetValue("AuDsDnmx", regValue);

              OutPutModeIndex = comboBoxOutPutMode.SelectedIndex;
              //No effect 
              if (OutPutModeIndex == 0)
              {
                regValue = 1;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              //Dolby Prologic 2
              if (OutPutModeIndex == 1)
              {
                regValue = 2;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              //CLMEI-2
              if (OutPutModeIndex == 2)
              {
                regValue = 4;
                subkey.SetValue("AuDsChanExpand", regValue);
              }

              #endregion

              break;
              //8 Speakers
            case 6:

              #region 8 Speakers

              regValue = 80;
              subkey.SetValue("AuDsInterface", regValue);
              regValue = 1000;
              subkey.SetValue("AuDsDnmx", regValue);

              OutPutModeIndex = comboBoxOutPutMode.SelectedIndex;
              //No effect 
              if (OutPutModeIndex == 0)
              {
                regValue = 1;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              //Dolby Prologic 2
              if (OutPutModeIndex == 1)
              {
                regValue = 2;
                subkey.SetValue("AuDsChanExpand", regValue);
              }
              //CLMEI-2
              if (OutPutModeIndex == 2)
              {
                regValue = 4;
                subkey.SetValue("AuDsChanExpand", regValue);
              }

              #endregion

              break;
          }
        }
      }

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\CLVSD\MediaPortal"))
      {
        if (subkey != null)
        {
          Int32 regUIUseHVA;
          if (checkBoxUIUseHVA.Checked)
          {
            regUIUseHVA = 1;
          }
          else
          {
            regUIUseHVA = 0;
          }
          subkey.SetValue("UIUseHVA", regUIUseHVA);
          using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            xmlwriter.SetValue("videocodec", "cyberlink", regUIUseHVA);
          }
          int DeInterlace;
          Int32 regDeInterlace;
          DeInterlace = comboBoxDeInterlace.SelectedIndex;
          if (DeInterlace == 0)
          {
            regDeInterlace = 1;
            subkey.SetValue("UIVMode", regDeInterlace);
          }
          if (DeInterlace == 1)
          {
            regDeInterlace = 2;
            subkey.SetValue("UIVMode", regDeInterlace);
          }
          if (DeInterlace == 2)
          {
            regDeInterlace = 3;
            subkey.SetValue("UIVMode", regDeInterlace);
          }
        }
      }

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\cl264dec\MediaPortal"))
      {
        if (subkey != null)
        {
          Int32 regH264UIUseHVA;
          if (checkBoxUIUseH264HVA.Checked)
          {
            regH264UIUseHVA = 1;
          }
          else
          {
            regH264UIUseHVA = 0;
          }
          subkey.SetValue("UIUseHVA", regH264UIUseHVA);
          int H264DeInterlace;
          Int32 regH264DeInterlace;
          H264DeInterlace = comboBoxH264DeInterlace.SelectedIndex;
          if (H264DeInterlace == 0)
          {
            regH264DeInterlace = 1;
            subkey.SetValue("UIVMode", regH264DeInterlace);
          }
          if (H264DeInterlace == 1)
          {
            regH264DeInterlace = 2;
            subkey.SetValue("UIVMode", regH264DeInterlace);
          }
          if (H264DeInterlace == 2)
          {
            regH264DeInterlace = 3;
            subkey.SetValue("UIVMode", regH264DeInterlace);
          }
        }
      }
    }

    private void comboBoxSpeakerConfig_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (comboBoxSpeakerConfig.SelectedIndex)
      {
          // Headphones
        case 0:

          #region Headphones

          comboBoxOutPutMode.Items.Clear();
          comboBoxOutPutMode.Enabled = true;
          comboBoxOutPutMode.Items.AddRange(new object[]
                                              {
                                                "Stereo",
                                                "Dolby Surround Downmix",
                                                "Dolby Headphone",
                                                "TruSurroundXT Headphone",
                                                "CyberLink Headphone"
                                              });

          #endregion

          break;
          // SPDIF Out 
        case 1:

          #region SPDIF

          comboBoxOutPutMode.Items.Clear();
          comboBoxOutPutMode.Enabled = false;

          #endregion

          break;
          //2 Speaker
        case 2:

          #region 2 speakers

          comboBoxOutPutMode.Items.Clear();
          comboBoxOutPutMode.Enabled = true;
          comboBoxOutPutMode.Items.AddRange(new object[]
                                              {
                                                "Stereo",
                                                "Dolby Surround Compatible Downmix",
                                                "TruSurroundXT",
                                                "Dolby Virtual Speaker",
                                                "CyberLink Virtual Speaker"
                                              });

          #endregion

          break;
          //4, 6, 7, 8 Speaker
        case 3:
        case 4:
        case 5:
        case 6:

          #region 4,6,7 & 8

          comboBoxOutPutMode.Items.Clear();
          comboBoxOutPutMode.Enabled = true;
          comboBoxOutPutMode.Items.AddRange(new object[]
                                              {
                                                "No Effect",
                                                "Dolby Pro Logic IIx",
                                                "CLMEI-2"
                                              });

          #endregion

          break;
      }
    }

    private void UIUseHVA_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxUIUseHVA.Checked == true)
      {
        comboBoxDeInterlace.Enabled = false;
      }
      if (checkBoxUIUseHVA.Checked == false)
      {
        comboBoxDeInterlace.Enabled = true;
      }
    }

    private void mpCheckBox1_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxUIUseH264HVA.Checked == true)
      {
        comboBoxH264DeInterlace.Enabled = false;
      }
      if (checkBoxUIUseH264HVA.Checked == false)
      {
        comboBoxH264DeInterlace.Enabled = true;
      }
    }
  }
}