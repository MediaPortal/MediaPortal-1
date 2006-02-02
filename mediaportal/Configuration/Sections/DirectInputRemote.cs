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
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.InputDevices;

namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// Summary description for DirectInputRemote.
  /// </summary>
  public class DirectInputRemote : SectionSettings
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    private DirectInputHandler diHandler = null;
    private MediaPortal.UserInterface.Controls.MPButton buttonDefault;
    private MediaPortal.UserInterface.Controls.MPButton btnRunControlPanel;
    private System.Windows.Forms.Label lblDInputDevice;
    private System.Windows.Forms.ComboBox cbDevices;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbI;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbSettings;
    private MediaPortal.UserInterface.Controls.MPButton btnMapping;
    private System.Windows.Forms.Label lblDelayMS;
    private System.Windows.Forms.NumericUpDown numDelay;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbButtonCombos;
    private MediaPortal.UserInterface.Controls.MPButton btnLearn;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label lblComboKill;
    private System.Windows.Forms.TextBox txtComboClose;
    private System.Windows.Forms.TextBox txtComboKill;
    private CheckBox cbEnable;
    private TabControl tabControl1;
    private TabPage tabPageDirectInputDevices;
    private Label txtMonitor;

    private System.Windows.Forms.TextBox lastEdit;

    public DirectInputRemote()
      : this("Direct Input")
    {
    }


    public DirectInputRemote(string name)
      : base(name)
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call
      diHandler = new DirectInputHandler();
      diHandler.DoSendActions = false; // only debug/display actions
      diHandler.OnStateChangeText += new DirectInputHandler.diStateChangeText(StateChangeAsText);
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      diHandler.OnStateChangeText -= new DirectInputHandler.diStateChangeText(StateChangeAsText);
      diHandler.Stop();
      diHandler = null;
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.cbEnable = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.gbI = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.txtMonitor = new System.Windows.Forms.Label();
      this.gbSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btnRunControlPanel = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDefault = new MediaPortal.UserInterface.Controls.MPButton();
      this.numDelay = new System.Windows.Forms.NumericUpDown();
      this.cbDevices = new System.Windows.Forms.ComboBox();
      this.lblDelayMS = new System.Windows.Forms.Label();
      this.lblDInputDevice = new System.Windows.Forms.Label();
      this.btnMapping = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.gbButtonCombos = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.btnLearn = new MediaPortal.UserInterface.Controls.MPButton();
      this.label2 = new System.Windows.Forms.Label();
      this.lblComboKill = new System.Windows.Forms.Label();
      this.txtComboClose = new System.Windows.Forms.TextBox();
      this.txtComboKill = new System.Windows.Forms.TextBox();
      this.tabPageDirectInputDevices = new System.Windows.Forms.TabPage();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.gbI.SuspendLayout();
      this.gbSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numDelay)).BeginInit();
      this.groupBox1.SuspendLayout();
      this.gbButtonCombos.SuspendLayout();
      this.tabPageDirectInputDevices.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.SuspendLayout();
      // 
      // cbEnable
      // 
      this.cbEnable.AutoSize = true;
      this.cbEnable.Location = new System.Drawing.Point(16, 24);
      this.cbEnable.Name = "cbEnable";
      this.cbEnable.Size = new System.Drawing.Size(276, 17);
      this.cbEnable.TabIndex = 0;
      this.cbEnable.Text = "Use Direct Input devices (Gamepads, Joysticks, etc.)";
      this.cbEnable.CheckedChanged += new System.EventHandler(this.cbEnable_CheckedChanged);
      // 
      // gbI
      // 
      this.gbI.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbI.Controls.Add(this.txtMonitor);
      this.gbI.Location = new System.Drawing.Point(12, 240);
      this.gbI.Name = "gbI";
      this.gbI.Size = new System.Drawing.Size(440, 124);
      this.gbI.TabIndex = 3;
      this.gbI.TabStop = false;
      this.gbI.Text = "Information";
      // 
      // txtMonitor
      // 
      this.txtMonitor.AutoSize = true;
      this.txtMonitor.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtMonitor.Location = new System.Drawing.Point(16, 24);
      this.txtMonitor.Name = "txtMonitor";
      this.txtMonitor.Size = new System.Drawing.Size(180, 12);
      this.txtMonitor.TabIndex = 0;
      this.txtMonitor.Text = "No information available.";
      // 
      // gbSettings
      // 
      this.gbSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbSettings.Controls.Add(this.btnRunControlPanel);
      this.gbSettings.Controls.Add(this.buttonDefault);
      this.gbSettings.Controls.Add(this.numDelay);
      this.gbSettings.Controls.Add(this.cbDevices);
      this.gbSettings.Controls.Add(this.lblDelayMS);
      this.gbSettings.Controls.Add(this.lblDInputDevice);
      this.gbSettings.Enabled = false;
      this.gbSettings.Location = new System.Drawing.Point(12, 72);
      this.gbSettings.Name = "gbSettings";
      this.gbSettings.Size = new System.Drawing.Size(440, 72);
      this.gbSettings.TabIndex = 1;
      this.gbSettings.TabStop = false;
      this.gbSettings.Text = "Settings";
      // 
      // btnRunControlPanel
      // 
      this.btnRunControlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRunControlPanel.Location = new System.Drawing.Point(352, 21);
      this.btnRunControlPanel.Name = "btnRunControlPanel";
      this.btnRunControlPanel.Size = new System.Drawing.Size(72, 22);
      this.btnRunControlPanel.TabIndex = 2;
      this.btnRunControlPanel.Text = "Control Panel";
      this.btnRunControlPanel.Click += new System.EventHandler(this.btnRunControlPanel_Click);
      // 
      // buttonDefault
      // 
      this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDefault.Location = new System.Drawing.Point(352, 45);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(72, 22);
      this.buttonDefault.TabIndex = 5;
      this.buttonDefault.Text = "&Reset";
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // numDelay
      // 
      this.numDelay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.numDelay.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.numDelay.Location = new System.Drawing.Point(152, 44);
      this.numDelay.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
      this.numDelay.Name = "numDelay";
      this.numDelay.Size = new System.Drawing.Size(192, 20);
      this.numDelay.TabIndex = 4;
      this.numDelay.Value = new decimal(new int[] {
            150,
            0,
            0,
            0});
      this.numDelay.ValueChanged += new System.EventHandler(this.numDelay_ValueChanged);
      // 
      // cbDevices
      // 
      this.cbDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDevices.Location = new System.Drawing.Point(152, 20);
      this.cbDevices.Name = "cbDevices";
      this.cbDevices.Size = new System.Drawing.Size(192, 21);
      this.cbDevices.TabIndex = 1;
      this.cbDevices.SelectedIndexChanged += new System.EventHandler(this.cbDevices_SelectedIndexChanged);
      // 
      // lblDelayMS
      // 
      this.lblDelayMS.AutoSize = true;
      this.lblDelayMS.Location = new System.Drawing.Point(16, 48);
      this.lblDelayMS.Name = "lblDelayMS";
      this.lblDelayMS.Size = new System.Drawing.Size(71, 13);
      this.lblDelayMS.TabIndex = 3;
      this.lblDelayMS.Text = "Delay (msec):";
      // 
      // lblDInputDevice
      // 
      this.lblDInputDevice.AutoSize = true;
      this.lblDInputDevice.Location = new System.Drawing.Point(16, 24);
      this.lblDInputDevice.Name = "lblDInputDevice";
      this.lblDInputDevice.Size = new System.Drawing.Size(100, 13);
      this.lblDInputDevice.TabIndex = 0;
      this.lblDInputDevice.Text = "Direct Input device:";
      // 
      // btnMapping
      // 
      this.btnMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnMapping.Location = new System.Drawing.Point(352, 20);
      this.btnMapping.Name = "btnMapping";
      this.btnMapping.Size = new System.Drawing.Size(72, 22);
      this.btnMapping.TabIndex = 1;
      this.btnMapping.Text = "Mapping";
      this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.btnMapping);
      this.groupBox1.Controls.Add(this.cbEnable);
      this.groupBox1.Location = new System.Drawing.Point(12, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 56);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // gbButtonCombos
      // 
      this.gbButtonCombos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.gbButtonCombos.Controls.Add(this.btnLearn);
      this.gbButtonCombos.Controls.Add(this.label2);
      this.gbButtonCombos.Controls.Add(this.lblComboKill);
      this.gbButtonCombos.Controls.Add(this.txtComboClose);
      this.gbButtonCombos.Controls.Add(this.txtComboKill);
      this.gbButtonCombos.Enabled = false;
      this.gbButtonCombos.Location = new System.Drawing.Point(12, 152);
      this.gbButtonCombos.Name = "gbButtonCombos";
      this.gbButtonCombos.Size = new System.Drawing.Size(440, 80);
      this.gbButtonCombos.TabIndex = 2;
      this.gbButtonCombos.TabStop = false;
      this.gbButtonCombos.Text = "Button Combos";
      // 
      // btnLearn
      // 
      this.btnLearn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLearn.Location = new System.Drawing.Point(352, 33);
      this.btnLearn.Name = "btnLearn";
      this.btnLearn.Size = new System.Drawing.Size(72, 22);
      this.btnLearn.TabIndex = 4;
      this.btnLearn.Text = "&Learn";
      this.btnLearn.Click += new System.EventHandler(this.btnLearn_Click);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(76, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Close process:";
      // 
      // lblComboKill
      // 
      this.lblComboKill.AutoSize = true;
      this.lblComboKill.Location = new System.Drawing.Point(16, 24);
      this.lblComboKill.Name = "lblComboKill";
      this.lblComboKill.Size = new System.Drawing.Size(63, 13);
      this.lblComboKill.TabIndex = 0;
      this.lblComboKill.Text = "Kill process:";
      // 
      // txtComboClose
      // 
      this.txtComboClose.Location = new System.Drawing.Point(152, 44);
      this.txtComboClose.Name = "txtComboClose";
      this.txtComboClose.Size = new System.Drawing.Size(192, 20);
      this.txtComboClose.TabIndex = 3;
      this.txtComboClose.Enter += new System.EventHandler(this.txtComboClose_Enter);
      // 
      // txtComboKill
      // 
      this.txtComboKill.Location = new System.Drawing.Point(152, 20);
      this.txtComboKill.Name = "txtComboKill";
      this.txtComboKill.Size = new System.Drawing.Size(192, 20);
      this.txtComboKill.TabIndex = 1;
      this.txtComboKill.Enter += new System.EventHandler(this.txtComboKill_Enter);
      // 
      // tabPageDirectInputDevices
      // 
      this.tabPageDirectInputDevices.Controls.Add(this.groupBox1);
      this.tabPageDirectInputDevices.Controls.Add(this.gbI);
      this.tabPageDirectInputDevices.Controls.Add(this.gbButtonCombos);
      this.tabPageDirectInputDevices.Controls.Add(this.gbSettings);
      this.tabPageDirectInputDevices.Location = new System.Drawing.Point(4, 22);
      this.tabPageDirectInputDevices.Name = "tabPageDirectInputDevices";
      this.tabPageDirectInputDevices.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageDirectInputDevices.Size = new System.Drawing.Size(464, 374);
      this.tabPageDirectInputDevices.TabIndex = 0;
      this.tabPageDirectInputDevices.Text = "Direct Input Devices";
      this.tabPageDirectInputDevices.UseVisualStyleBackColor = true;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPageDirectInputDevices);
      this.tabControl1.Location = new System.Drawing.Point(0, 8);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 400);
      this.tabControl1.TabIndex = 0;
      // 
      // DirectInputRemote
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "DirectInputRemote";
      this.Size = new System.Drawing.Size(472, 408);
      this.Load += new System.EventHandler(this.DirectInputRemote_Load);
      this.gbI.ResumeLayout(false);
      this.gbI.PerformLayout();
      this.gbSettings.ResumeLayout(false);
      this.gbSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numDelay)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.gbButtonCombos.ResumeLayout(false);
      this.gbButtonCombos.PerformLayout();
      this.tabPageDirectInputDevices.ResumeLayout(false);
      this.tabControl1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void cbDevices_SelectedIndexChanged(object sender, EventArgs e)
    {
      int index = cbDevices.SelectedIndex;
      if (index >= 0)
      {
        diHandler.SelectDevice(diHandler.DeviceGUIDs[index].ToString());
        txtMonitor.Text = "No information available.";
      }
    }

    void SyncCombo()
    {
      cbDevices.Items.Clear();
      foreach (string deviceName in diHandler.DeviceNames)
      {
        cbDevices.Items.Add(deviceName);
      }
    }

    private void cbEnable_CheckedChanged(object sender, EventArgs e)
    {
      diHandler.Active = cbEnable.Checked;
      gbSettings.Enabled = cbEnable.Checked;
      gbButtonCombos.Enabled = cbEnable.Checked;
      if (cbEnable.Checked)
      {
        SyncCombo();
      }
      else
      {
        txtMonitor.Text = "No information available.";
      }
    }

    void StateChangeAsText(object sender, string stateText)
    {
      txtMonitor.Text = stateText;
    }

    private void btnRunControlPanel_Click(object sender, System.EventArgs e)
    {
      diHandler.RunControlPanel();
    }

    private void DirectInputRemote_Load(object sender, System.EventArgs e)
    {
      cbEnable.Checked = diHandler.Active;
      cbDevices.SelectedIndex = diHandler.SelectedDeviceIndex;
      numDelay.Value = diHandler.Delay;
      txtComboKill.Text = diHandler.ButtonComboKill;
      txtComboClose.Text = diHandler.ButtonComboClose;
    }


    public override void LoadSettings()
    {
      diHandler.LoadSettings();
    }

    public override void SaveSettings()
    {
      diHandler.ButtonComboClose = txtComboClose.Text;
      diHandler.ButtonComboKill = txtComboKill.Text;
      diHandler.SaveSettings();
    }

    private void btnMapping_Click(object sender, System.EventArgs e)
    {
      InputMappingForm dlg = new InputMappingForm("DirectInput");
      dlg.ShowDialog(this);
    }

    private void buttonDefault_Click(object sender, System.EventArgs e)
    {
      numDelay.Value = (decimal)150;
    }

    private void numDelay_ValueChanged(object sender, System.EventArgs e)
    {
      diHandler.Delay = (int)numDelay.Value;
    }

    private void btnLearn_Click(object sender, System.EventArgs e)
    {
      if (lastEdit == txtComboKill)
      {
        txtComboKill.Text = diHandler.GetCurrentButtonCombo();
      }
      else if (lastEdit == txtComboClose)
      {
        txtComboClose.Text = diHandler.GetCurrentButtonCombo();

      }
    }

    private void txtComboKill_Enter(object sender, System.EventArgs e)
    {
      lastEdit = txtComboKill;
    }

    private void txtComboClose_Enter(object sender, System.EventArgs e)
    {
      lastEdit = txtComboClose;
    }
  }
}
