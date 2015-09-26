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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBlaster.Config
{
  partial class HauppaugeBlasterConfig
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HauppaugeBlasterConfig));
      this.pictureBoxLogo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPictureBox();
      this.labelDescription = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelInstallState = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.buttonBlastCfg = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.comboBoxTunerSelectionPort1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelTunerSelectionPort1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.pictureBoxTunerSelectionPort1 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPictureBox();
      this.pictureBoxTunerSelectionPort2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPictureBox();
      this.comboBoxTunerSelectionPort2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.labelTunerSelectionPort2 = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTunerSelectionPort1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTunerSelectionPort2)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBoxLogo
      // 
      this.pictureBoxLogo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxLogo.Image")));
      this.pictureBoxLogo.Location = new System.Drawing.Point(9, 13);
      this.pictureBoxLogo.Name = "pictureBoxLogo";
      this.pictureBoxLogo.Size = new System.Drawing.Size(201, 60);
      this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBoxLogo.TabIndex = 2;
      this.pictureBoxLogo.TabStop = false;
      // 
      // labelDescription
      // 
      this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelDescription.Location = new System.Drawing.Point(6, 81);
      this.labelDescription.Name = "labelDescription";
      this.labelDescription.Size = new System.Drawing.Size(468, 155);
      this.labelDescription.TabIndex = 0;
      this.labelDescription.Text = resources.GetString("labelDescription.Text");
      // 
      // labelInstallState
      // 
      this.labelInstallState.AutoSize = true;
      this.labelInstallState.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelInstallState.Location = new System.Drawing.Point(6, 236);
      this.labelInstallState.Name = "labelInstallState";
      this.labelInstallState.Size = new System.Drawing.Size(125, 13);
      this.labelInstallState.TabIndex = 1;
      this.labelInstallState.Text = "(IRBlast install state)";
      // 
      // buttonBlastCfg
      // 
      this.buttonBlastCfg.Location = new System.Drawing.Point(9, 376);
      this.buttonBlastCfg.Name = "buttonBlastCfg";
      this.buttonBlastCfg.Size = new System.Drawing.Size(100, 23);
      this.buttonBlastCfg.TabIndex = 6;
      this.buttonBlastCfg.Text = "Run BlastCfg";
      this.buttonBlastCfg.UseVisualStyleBackColor = true;
      this.buttonBlastCfg.Click += new System.EventHandler(this.buttonBlastCfg_Click);
      // 
      // comboBoxTunerSelectionPort1
      // 
      this.comboBoxTunerSelectionPort1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTunerSelectionPort1.DisplayMember = "Name";
      this.comboBoxTunerSelectionPort1.FormattingEnabled = true;
      this.comboBoxTunerSelectionPort1.Location = new System.Drawing.Point(78, 282);
      this.comboBoxTunerSelectionPort1.Name = "comboBoxTunerSelectionPort1";
      this.comboBoxTunerSelectionPort1.Size = new System.Drawing.Size(386, 21);
      this.comboBoxTunerSelectionPort1.TabIndex = 3;
      // 
      // labelTunerSelectionPort1
      // 
      this.labelTunerSelectionPort1.AutoSize = true;
      this.labelTunerSelectionPort1.Location = new System.Drawing.Point(6, 258);
      this.labelTunerSelectionPort1.Name = "labelTunerSelectionPort1";
      this.labelTunerSelectionPort1.Size = new System.Drawing.Size(235, 13);
      this.labelTunerSelectionPort1.TabIndex = 2;
      this.labelTunerSelectionPort1.Text = "Select an analog tuner to use with blaster port 1:";
      // 
      // pictureBoxTunerSelectionPort1
      // 
      this.pictureBoxTunerSelectionPort1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxTunerSelectionPort1.Image")));
      this.pictureBoxTunerSelectionPort1.Location = new System.Drawing.Point(26, 280);
      this.pictureBoxTunerSelectionPort1.Name = "pictureBoxTunerSelectionPort1";
      this.pictureBoxTunerSelectionPort1.Size = new System.Drawing.Size(33, 23);
      this.pictureBoxTunerSelectionPort1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBoxTunerSelectionPort1.TabIndex = 7;
      this.pictureBoxTunerSelectionPort1.TabStop = false;
      // 
      // pictureBoxTunerSelectionPort2
      // 
      this.pictureBoxTunerSelectionPort2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxTunerSelectionPort2.Image")));
      this.pictureBoxTunerSelectionPort2.Location = new System.Drawing.Point(26, 338);
      this.pictureBoxTunerSelectionPort2.Name = "pictureBoxTunerSelectionPort2";
      this.pictureBoxTunerSelectionPort2.Size = new System.Drawing.Size(33, 23);
      this.pictureBoxTunerSelectionPort2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBoxTunerSelectionPort2.TabIndex = 9;
      this.pictureBoxTunerSelectionPort2.TabStop = false;
      // 
      // comboBoxTunerSelectionPort2
      // 
      this.comboBoxTunerSelectionPort2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxTunerSelectionPort2.DisplayMember = "Name";
      this.comboBoxTunerSelectionPort2.FormattingEnabled = true;
      this.comboBoxTunerSelectionPort2.Location = new System.Drawing.Point(78, 340);
      this.comboBoxTunerSelectionPort2.Name = "comboBoxTunerSelectionPort2";
      this.comboBoxTunerSelectionPort2.Size = new System.Drawing.Size(386, 21);
      this.comboBoxTunerSelectionPort2.TabIndex = 5;
      // 
      // labelTunerSelectionPort2
      // 
      this.labelTunerSelectionPort2.AutoSize = true;
      this.labelTunerSelectionPort2.Location = new System.Drawing.Point(6, 319);
      this.labelTunerSelectionPort2.Name = "labelTunerSelectionPort2";
      this.labelTunerSelectionPort2.Size = new System.Drawing.Size(314, 13);
      this.labelTunerSelectionPort2.TabIndex = 4;
      this.labelTunerSelectionPort2.Text = "Select an analog tuner to use with blaster port 2 (HVR-22xx only):";
      // 
      // HauppaugeBlasterConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.labelTunerSelectionPort2);
      this.Controls.Add(this.pictureBoxTunerSelectionPort2);
      this.Controls.Add(this.comboBoxTunerSelectionPort2);
      this.Controls.Add(this.pictureBoxTunerSelectionPort1);
      this.Controls.Add(this.labelTunerSelectionPort1);
      this.Controls.Add(this.comboBoxTunerSelectionPort1);
      this.Controls.Add(this.buttonBlastCfg);
      this.Controls.Add(this.labelInstallState);
      this.Controls.Add(this.labelDescription);
      this.Controls.Add(this.pictureBoxLogo);
      this.Name = "HauppaugeBlasterConfig";
      this.Size = new System.Drawing.Size(480, 420);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTunerSelectionPort1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTunerSelectionPort2)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private SetupControls.UserInterfaceControls.MPPictureBox pictureBoxLogo;
    private SetupControls.UserInterfaceControls.MPLabel labelDescription;
    private SetupControls.UserInterfaceControls.MPLabel labelInstallState;
    private SetupControls.UserInterfaceControls.MPButton buttonBlastCfg;
    private SetupControls.UserInterfaceControls.MPComboBox comboBoxTunerSelectionPort1;
    private SetupControls.UserInterfaceControls.MPLabel labelTunerSelectionPort1;
    private SetupControls.UserInterfaceControls.MPPictureBox pictureBoxTunerSelectionPort1;
    private SetupControls.UserInterfaceControls.MPPictureBox pictureBoxTunerSelectionPort2;
    private SetupControls.UserInterfaceControls.MPComboBox comboBoxTunerSelectionPort2;
    private SetupControls.UserInterfaceControls.MPLabel labelTunerSelectionPort2;
  }
}