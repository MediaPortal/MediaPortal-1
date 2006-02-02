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

namespace MediaPortal.Configuration.Sections
{

  public class PowerDVD6AudioFilter : MediaPortal.Configuration.SectionSettings
  {
    private ComboBox comboBoxSpeakerConfig;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private GroupBox groupBox2;
    private ComboBox comboBoxOutPutMode;
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// 
    /// </summary>
    public PowerDVD6AudioFilter()
      : this("PowerDVD 6 Audio Decoder")
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public PowerDVD6AudioFilter(string name)
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
      this.comboBoxSpeakerConfig = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxOutPutMode = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // comboBoxSpeakerConfig
      // 
      this.comboBoxSpeakerConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxSpeakerConfig.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSpeakerConfig.Items.AddRange(new object[] {
            "Headphones",
            "S/PDIF Out",
            "2 Speaker",
            "4 Speaker",
            "6 Speaker",
            "7 Speaker",
            "8 Speaker"});
      this.comboBoxSpeakerConfig.Location = new System.Drawing.Point(6, 19);
      this.comboBoxSpeakerConfig.Name = "comboBoxSpeakerConfig";
      this.comboBoxSpeakerConfig.Size = new System.Drawing.Size(460, 21);
      this.comboBoxSpeakerConfig.TabIndex = 1;
      this.comboBoxSpeakerConfig.SelectedIndexChanged += new System.EventHandler(this.comboBoxSpeakerConfig_SelectedIndexChanged);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.comboBoxSpeakerConfig);
      this.groupBox1.Location = new System.Drawing.Point(0, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 63);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Speaker enviroment";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.comboBoxOutPutMode);
      this.groupBox2.Location = new System.Drawing.Point(0, 87);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 68);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Output mode";
      // 
      // comboBoxOutPutMode
      // 
      this.comboBoxOutPutMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxOutPutMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxOutPutMode.DropDownWidth = 460;
      this.comboBoxOutPutMode.Location = new System.Drawing.Point(3, 19);
      this.comboBoxOutPutMode.Name = "comboBoxOutPutMode";
      this.comboBoxOutPutMode.Size = new System.Drawing.Size(463, 21);
      this.comboBoxOutPutMode.TabIndex = 0;
      // 
      // PowerDVD6AudioFilter
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "PowerDVD6AudioFilter";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    public override void LoadSettings()
    {
      int iIndex = 0;
      Int32 regValue;
      Int32 regAuDsDnmx;
      Int32 regAuDsChanExpand;

      RegistryKey hkcu = Registry.CurrentUser;
      RegistryKey subkey = hkcu.CreateSubKey(@"Software\Cyberlink\Common\CLAud\MediaPortal");

      if (subkey != null)
      {
        try
        {
          regValue = (Int32)subkey.GetValue("AuDsInterface");
          regAuDsDnmx = (Int32)subkey.GetValue("AuDsDnmx");
          regAuDsChanExpand = (Int32)subkey.GetValue("AuDsChanExpand");

          switch (regValue)
          {
            // Headphones (comboBoxSpeakerConfig index 0)
            case 2000:
              #region Headphones
              comboBoxSpeakerConfig.SelectedIndex = 0;

              comboBoxOutPutMode.Items.Clear();
              comboBoxOutPutMode.Enabled = true;
              comboBoxOutPutMode.Items.AddRange(new object[] {
                                                    "Stereo",
                                                    "Dolby surround downmix",
                                                    "Dolby headphone",
                                                    "Cyberlink headphone"});
              if (regAuDsDnmx == 2) iIndex = 0;
              if (regAuDsDnmx == 8) iIndex = 1;
              if (regAuDsDnmx == 200) iIndex = 2;
              if (regAuDsDnmx == 8000) iIndex = 3;
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
              comboBoxOutPutMode.Items.AddRange(new object[] {
                                                    "Stereo",
                                                    "Dolby surround downmix",
                                                    "Dolby virtual speaker",
                                                    "Cyberlink virtual speaker"});
              if (regAuDsDnmx == 2) iIndex = 0;
              if (regAuDsDnmx == 8) iIndex = 1;
              if (regAuDsDnmx == 2000) iIndex = 2;
              if (regAuDsDnmx == 10000) iIndex = 3;
              comboBoxOutPutMode.SelectedIndex = iIndex;
              #endregion
              break;
            //4 Speaker (comboBoxSpeakerConfig index 3)
            case 10:
              #region 4 speaker
              comboBoxSpeakerConfig.SelectedIndex = 3;
              comboBoxOutPutMode.Items.Clear();
              comboBoxOutPutMode.Enabled = true;
              comboBoxOutPutMode.Items.AddRange(new object[] {
                                                    "No Effect",
                                                    "Dolby Pro Logic IIx",
                                                    "CLMEI2"});
              if (regAuDsChanExpand == 1) iIndex = 0;
              if (regAuDsChanExpand == 2) iIndex = 1;
              if (regAuDsChanExpand == 4) iIndex = 2;
              comboBoxOutPutMode.SelectedIndex = iIndex;
              #endregion
              break;
            //6 Speaker (comboBoxSpeakerConfig index 4)
            case 20:
              #region 6 speaker
              comboBoxSpeakerConfig.SelectedIndex = 4;
              comboBoxOutPutMode.Items.Clear();
              comboBoxOutPutMode.Enabled = true;
              comboBoxOutPutMode.Items.AddRange(new object[] {
                                                    "No Effect",
                                                    "Dolby Pro Logic IIx",
                                                    "CLMEI2"});
              if (regAuDsChanExpand == 1) iIndex = 0;
              if (regAuDsChanExpand == 2) iIndex = 1;
              if (regAuDsChanExpand == 4) iIndex = 2;
              comboBoxOutPutMode.SelectedIndex = iIndex;
              #endregion
              break;
            //7 Speaker (comboBoxSpeakerConfig index 5)
            case 40:
              #region 7 speaker
              comboBoxSpeakerConfig.SelectedIndex = 5;
              comboBoxOutPutMode.Items.Clear();
              comboBoxOutPutMode.Enabled = true;
              comboBoxOutPutMode.Items.AddRange(new object[] {
                                                    "No Effect",
                                                    "Dolby Pro Logic IIx",
                                                    "CLMEI2"});
              if (regAuDsChanExpand == 1) iIndex = 0;
              if (regAuDsChanExpand == 2) iIndex = 1;
              if (regAuDsChanExpand == 4) iIndex = 2;
              comboBoxOutPutMode.SelectedIndex = iIndex;
              #endregion
              break;
            //8 Speaker (comboBoxSpeakerConfig index 6)
            case 80:
              #region 8 speaker
              comboBoxSpeakerConfig.SelectedIndex = 6;
              comboBoxOutPutMode.Items.Clear();
              comboBoxOutPutMode.Enabled = true;
              comboBoxOutPutMode.Items.AddRange(new object[] {
                                                    "No Effect",
                                                    "Dolby Pro Logic IIx",
                                                    "CLMEI2"});
              if (regAuDsChanExpand == 1) iIndex = 0;
              if (regAuDsChanExpand == 2) iIndex = 1;
              if (regAuDsChanExpand == 4) iIndex = 2;
              comboBoxOutPutMode.SelectedIndex = iIndex;
              #endregion
              break;
          }


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
      Int32 regValue;
      int OutPutModeIndex;
      RegistryKey hkcu = Registry.CurrentUser;
      RegistryKey subkey = hkcu.CreateSubKey(@"Software\Cyberlink\Common\CLAud\MediaPortal");
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
              regValue = 8000;
              subkey.SetValue("AuDsDnmx", regValue);
            }

            #endregion
            break;
          //4, 6, 7, 8 Speaker
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

        /*				Int32 regValue;
if (checkBoxDynamicRange.Checked) regValue=1;
else regValue=0;
subkey.SetValue("Dynamic Range Control",regValue);

				
if (checkBoxMPEGOverSPDIF.Checked) regValue=1;
else regValue=0;
subkey.SetValue("MPEG Audio over SPDIF",regValue);

if (checkBoxSPDIF.Checked) regValue=1;
else regValue=0;
subkey.SetValue("Use SPDIF for AC3 & DTS",regValue);

regValue=Int32.Parse(textBoxAudioOffset.Text);
subkey.SetValue("SPDIF Audio Time Offset",regValue);

regValue=comboBoxSpeakerConfig.SelectedIndex;
        subkey.SetValue("Speaker Config",regValue);
*/
        subkey.Close();
      }
    }

    private void comboBoxSpeakerConfig_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      switch (comboBoxSpeakerConfig.SelectedIndex)
      {
        // Headphones
        case 0:
          #region Headphones
          comboBoxOutPutMode.Items.Clear();
          comboBoxOutPutMode.Enabled = true;
          comboBoxOutPutMode.Items.AddRange(new object[] {
                                                    "Stereo",
                                                    "Dolby surround downmix",
                                                    "Dolby headphone",
                                                    "Cyberlink headphone"});

          #endregion
          break;
        // SPDIF Out 
        case 1:
          comboBoxOutPutMode.Items.Clear();
          comboBoxOutPutMode.Enabled = false;
          break;
        //2 Speaker
        case 2:
          #region 2 speakers
          comboBoxOutPutMode.Items.Clear();
          comboBoxOutPutMode.Enabled = true;
          comboBoxOutPutMode.Items.AddRange(new object[] {
                                                    "Stereo",
                                                    "Dolby surround downmix",
                                                    "Dolby virtual speaker",
                                                    "Cyberlink virtual speaker"});
          #endregion
          break;

        //4, 6, 7, 8 Speaker
        case 3:
        case 4:
        case 5:
        case 6:
          comboBoxOutPutMode.Items.Clear();
          comboBoxOutPutMode.Enabled = true;
          comboBoxOutPutMode.Items.AddRange(new object[] {
                                                    "No Effect",
                                                    "Dolby Pro Logic IIx",
                                                    "CLMEI2"});
          break;

      }
    }

  }
}

