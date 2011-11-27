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
  public class FiltersPowerDVDDecoder : SectionSettings
  {
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
    public FiltersPowerDVDDecoder()
      : this("PowerDVD Decoder") {}

    /// <summary>
    /// 
    /// </summary>
    public FiltersPowerDVDDecoder(string name)
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
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxH264DeInterlace = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.checkBoxUIUseH264HVA = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox2.SuspendLayout();
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
      this.mpGroupBox2.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(462, 118);
      this.mpGroupBox2.TabIndex = 3;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "MPEG-2 Video Decoder Settings";
      // 
      // comboBoxDeInterlace
      // 
      this.comboBoxDeInterlace.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxDeInterlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDeInterlace.FormattingEnabled = true;
      this.comboBoxDeInterlace.Items.AddRange(new object[] {
            "Auto-select",
            "Force bob",
            "Force weave"});
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
      // mpGroupBox3
      // 
      this.mpGroupBox3.Controls.Add(this.comboBoxH264DeInterlace);
      this.mpGroupBox3.Controls.Add(this.mpLabel6);
      this.mpGroupBox3.Controls.Add(this.mpLabel5);
      this.mpGroupBox3.Controls.Add(this.checkBoxUIUseH264HVA);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(6, 124);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(462, 119);
      this.mpGroupBox3.TabIndex = 4;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "H.264 Video Decoder Settings";
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
      // FiltersPowerDVDDecoder
      // 
      this.Controls.Add(this.mpGroupBox3);
      this.Controls.Add(this.mpGroupBox2);
      this.Name = "FiltersPowerDVDDecoder";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox3.ResumeLayout(false);
      this.mpGroupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    public override void LoadSettings()
    {
      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\CLVSD\MediaPortal"))
      {
        if (subkey != null)
        {
          try
          {
            Int32 regUIUseHVA = (Int32)subkey.GetValue("UIUseHVA", 1);
            if (regUIUseHVA == 1)
            {
              checkBoxUIUseHVA.Checked = true;
            }
            else
            {
              checkBoxUIUseHVA.Checked = false;
            }
            Int32 regUIVMode = (Int32)subkey.GetValue("UIVMode", 0);
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
          catch (Exception) {}
        }
      }

      using (RegistryKey subkey = Registry.CurrentUser.CreateSubKey(@"Software\Cyberlink\Common\cl264dec\MediaPortal"))
      {
        if (subkey != null)
        {
          try
          {
            Int32 regH264UIUseHVA = (Int32)subkey.GetValue("UIUseHVA", 1);
            if (regH264UIUseHVA == 1)
            {
              checkBoxUIUseH264HVA.Checked = true;
            }
            else
            {
              checkBoxUIUseH264HVA.Checked = false;
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
          using (Settings xmlwriter = new MPSettings())
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