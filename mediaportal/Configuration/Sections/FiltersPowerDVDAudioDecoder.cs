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
using System.ComponentModel;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class FiltersPowerDVDAudioDecoder : SectionSettings
  {
    private MPComboBox comboBoxSpeakerConfig;
    private MPComboBox comboBoxOutPutMode;
    private MPGroupBox mpGroupBox1;
    private MPLabel mpLabel1;
    private MPLabel mpLabel2;
    private IContainer components = null;

    /// <summary>
    /// 
    /// </summary>
    public FiltersPowerDVDAudioDecoder()
      : this("PowerDVD Audio Decoder") {}

    /// <summary>
    /// 
    /// </summary>
    public FiltersPowerDVDAudioDecoder(string name)
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
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxSpeakerConfig = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.comboBoxOutPutMode = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.mpLabel2);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.Controls.Add(this.comboBoxSpeakerConfig);
      this.mpGroupBox1.Controls.Add(this.comboBoxOutPutMode);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(3, 3);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(462, 144);
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
      this.comboBoxSpeakerConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSpeakerConfig.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxSpeakerConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSpeakerConfig.Items.AddRange(new object[] {
            "Headphones",
            "S/PDIF Out",
            "2 Speaker",
            "4 Speaker",
            "6 Speaker",
            "7 Speaker",
            "8 Speaker"});
      this.comboBoxSpeakerConfig.Location = new System.Drawing.Point(52, 48);
      this.comboBoxSpeakerConfig.Name = "comboBoxSpeakerConfig";
      this.comboBoxSpeakerConfig.Size = new System.Drawing.Size(365, 21);
      this.comboBoxSpeakerConfig.TabIndex = 1;
      this.comboBoxSpeakerConfig.SelectedIndexChanged += new System.EventHandler(this.comboBoxSpeakerConfig_SelectedIndexChanged);
      // 
      // comboBoxOutPutMode
      // 
      this.comboBoxOutPutMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxOutPutMode.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxOutPutMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxOutPutMode.DropDownWidth = 375;
      this.comboBoxOutPutMode.Location = new System.Drawing.Point(52, 102);
      this.comboBoxOutPutMode.Name = "comboBoxOutPutMode";
      this.comboBoxOutPutMode.Size = new System.Drawing.Size(365, 21);
      this.comboBoxOutPutMode.TabIndex = 0;
      // 
      // FiltersPowerDVDAudioDecoder
      // 
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "FiltersPowerDVDAudioDecoder";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
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
            regValue = (Int32)subkey.GetValue("AuDsInterface", 8);
            regAuDsDnmx = (Int32)subkey.GetValue("AuDsDnmx", 2);
            regAuDsChanExpand = (Int32)subkey.GetValue("AuDsChanExpand", -1);
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
          catch (Exception) {}
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
  }
}
