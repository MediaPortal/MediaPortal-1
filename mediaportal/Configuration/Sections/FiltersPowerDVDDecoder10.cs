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
  public class FiltersPowerDVDDecoder10 : SectionSettings
  {
    private MPComboBox comboBoxH264DeInterlace;
    private MPLabel mpLabel6;
    private MPGroupBox mpGroupBox3;
    private MPLabel mpLabel5;
    private MPCheckBox checkBoxUIUseH264HVA;
    private MPCheckBox checkBoxUIUseHAM;
    private MPCheckBox checkBoxUIUseSW;
    private IContainer components = null;

    /// <summary>
    /// 
    /// </summary>
    public FiltersPowerDVDDecoder10()
      : this("PowerDVD Video 10/11") {}

    /// <summary>
    /// 
    /// </summary>
    public FiltersPowerDVDDecoder10(string name)
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
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxUIUseSW = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxUIUseHAM = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.comboBoxH264DeInterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxUIUseH264HVA = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Controls.Add(this.checkBoxUIUseSW);
      this.mpGroupBox3.Controls.Add(this.checkBoxUIUseHAM);
      this.mpGroupBox3.Controls.Add(this.comboBoxH264DeInterlace);
      this.mpGroupBox3.Controls.Add(this.mpLabel6);
      this.mpGroupBox3.Controls.Add(this.mpLabel5);
      this.mpGroupBox3.Controls.Add(this.checkBoxUIUseH264HVA);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(462, 221);
      this.mpGroupBox3.TabIndex = 4;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "H.264/VC-1/MPEG Video Decoder Settings (PDVD10/11)";
      // 
      // checkBoxUIUseSW
      // 
      this.checkBoxUIUseSW.AutoSize = true;
      this.checkBoxUIUseSW.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUIUseSW.Location = new System.Drawing.Point(22, 80);
      this.checkBoxUIUseSW.Name = "checkBoxUIUseSW";
      this.checkBoxUIUseSW.Size = new System.Drawing.Size(64, 17);
      this.checkBoxUIUseSW.TabIndex = 7;
      this.checkBoxUIUseSW.Text = "Use SW";
      this.checkBoxUIUseSW.UseVisualStyleBackColor = true;
      this.checkBoxUIUseSW.CheckedChanged += new System.EventHandler(this.checkBoxUIUseSW_CheckedChanged);
      // 
      // checkBoxUIUseHAM
      // 
      this.checkBoxUIUseHAM.AutoSize = true;
      this.checkBoxUIUseHAM.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUIUseHAM.Location = new System.Drawing.Point(22, 56);
      this.checkBoxUIUseHAM.Name = "checkBoxUIUseHAM";
      this.checkBoxUIUseHAM.Size = new System.Drawing.Size(70, 17);
      this.checkBoxUIUseHAM.TabIndex = 6;
      this.checkBoxUIUseHAM.Text = "Use HAM";
      this.checkBoxUIUseHAM.UseVisualStyleBackColor = true;
      this.checkBoxUIUseHAM.CheckedChanged += new System.EventHandler(this.checkBoxUIUseHAM_CheckedChanged);
      // 
      // comboBoxH264DeInterlace
      // 
      this.comboBoxH264DeInterlace.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxH264DeInterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxH264DeInterlace.FormattingEnabled = true;
      this.comboBoxH264DeInterlace.Items.AddRange(new object[] {
            "Auto-select",
            "Force bob",
            "Force weave"});
      this.comboBoxH264DeInterlace.Location = new System.Drawing.Point(52, 143);
      this.comboBoxH264DeInterlace.Name = "comboBoxH264DeInterlace";
      this.comboBoxH264DeInterlace.Size = new System.Drawing.Size(198, 21);
      this.comboBoxH264DeInterlace.TabIndex = 5;
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(22, 118);
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
      this.checkBoxUIUseH264HVA.Size = new System.Drawing.Size(75, 17);
      this.checkBoxUIUseH264HVA.TabIndex = 4;
      this.checkBoxUIUseH264HVA.Text = "Use DXVA";
      this.checkBoxUIUseH264HVA.UseVisualStyleBackColor = true;
      this.checkBoxUIUseH264HVA.CheckedChanged += new System.EventHandler(this.mpCheckBox1_CheckedChanged);
      // 
      // FiltersPowerDVDDecoder10
      // 
      this.Controls.Add(this.mpGroupBox3);
      this.Name = "FiltersPowerDVDDecoder10";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    public override void LoadSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\clcvd\MediaPortal"))
      {
        if (subkey != null)
        {
          try
          {
            Int32 regH264UIUseHVA = (Int32)subkey.GetValue("UIUseHVA", 1);
            if (regH264UIUseHVA == 1)
            {
              checkBoxUIUseH264HVA.Checked = true;
              checkBoxUIUseHAM.Checked = false;
              checkBoxUIUseSW.Checked = false;
            }
            else if (regH264UIUseHVA == 2)
            {
              checkBoxUIUseH264HVA.Checked = false;
              checkBoxUIUseHAM.Checked = true;
              checkBoxUIUseSW.Checked = false;
            }
            else if (regH264UIUseHVA == 0)
            {
              checkBoxUIUseH264HVA.Checked = false;
              checkBoxUIUseHAM.Checked = false;
              checkBoxUIUseSW.Checked = true;
            }
            Int32 regH264UIVMode = (Int32)subkey.GetValue("UIVMode", 0);
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
          catch (Exception) {}
        }
      }
    }

    public override void SaveSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\clcvd\MediaPortal"))
      {
        if (subkey != null)
        {
          Int32 regH264UIUseHVA;

          if (checkBoxUIUseH264HVA.Checked)
          {
            regH264UIUseHVA = 1;
            subkey.SetValue("UIUseHVA", regH264UIUseHVA);
          }
          else if (checkBoxUIUseHAM.Checked)
          {
            regH264UIUseHVA = 2;
            subkey.SetValue("UIUseHVA", regH264UIUseHVA);
          }
          else if (checkBoxUIUseSW.Checked)
          {
            regH264UIUseHVA = 0;
            subkey.SetValue("UIUseHVA", regH264UIUseHVA);
          }
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

    private void mpCheckBox1_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxUIUseH264HVA.Checked == true)
      {
        comboBoxH264DeInterlace.Enabled = false;
        checkBoxUIUseHAM.Checked = false;
        checkBoxUIUseSW.Checked = false;
      }
      else if (checkBoxUIUseH264HVA.Checked == false && checkBoxUIUseHAM.Checked == false)
      {
        comboBoxH264DeInterlace.Enabled = true;
      }  
    }

    private void checkBoxUIUseHAM_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxUIUseHAM.Checked == true)
      {
        comboBoxH264DeInterlace.Enabled = false;
        checkBoxUIUseH264HVA.Checked = false;
        checkBoxUIUseSW.Checked = false;
      }
      else if (checkBoxUIUseHAM.Checked == false && checkBoxUIUseH264HVA.Checked == false)
      {
        comboBoxH264DeInterlace.Enabled = true;
      }
    }

    private void checkBoxUIUseSW_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxUIUseSW.Checked == true)
      {
        checkBoxUIUseHAM.Checked = false;
        checkBoxUIUseH264HVA.Checked = false;
      }
    }
  }
}
