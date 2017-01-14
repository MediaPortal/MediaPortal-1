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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Config
{
  partial class MicrosoftBlasterConfig
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MicrosoftBlasterConfig));
      this.dataGridViewConfig = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridView();
      this.dataGridViewColumnTunerId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnTunerName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnTransceiver = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn();
      this.dataGridViewColumnTransmitPort = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn();
      this.dataGridViewColumnSetTopBoxProfile = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn();
      this.dataGridViewColumnPowerControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewCheckBoxColumn();
      this.buttonTest = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.channelNumberUpDown = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPChannelNumberUpDown();
      this.buttonLearn = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.pictureBoxLogo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConfig)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
      this.SuspendLayout();
      // 
      // dataGridViewConfig
      // 
      this.dataGridViewConfig.AllowUserToAddRows = false;
      this.dataGridViewConfig.AllowUserToDeleteRows = false;
      this.dataGridViewConfig.AllowUserToOrderColumns = true;
      this.dataGridViewConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGridViewConfig.ColumnHeadersHeight = 36;
      this.dataGridViewConfig.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dataGridViewConfig.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewColumnTunerId,
            this.dataGridViewColumnTunerName,
            this.dataGridViewColumnTransceiver,
            this.dataGridViewColumnTransmitPort,
            this.dataGridViewColumnSetTopBoxProfile,
            this.dataGridViewColumnPowerControl});
      this.dataGridViewConfig.Location = new System.Drawing.Point(6, 54);
      this.dataGridViewConfig.MultiSelect = false;
      this.dataGridViewConfig.Name = "dataGridViewConfig";
      this.dataGridViewConfig.RowHeadersVisible = false;
      this.dataGridViewConfig.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViewConfig.Size = new System.Drawing.Size(465, 327);
      this.dataGridViewConfig.TabIndex = 0;
      this.dataGridViewConfig.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridViewConfig_CurrentCellDirtyStateChanged);
      this.dataGridViewConfig.SelectionChanged += new System.EventHandler(this.dataGridViewConfig_SelectionChanged);
      // 
      // dataGridViewColumnTunerId
      // 
      this.dataGridViewColumnTunerId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
      this.dataGridViewColumnTunerId.HeaderText = "Tuner ID";
      this.dataGridViewColumnTunerId.MinimumWidth = 45;
      this.dataGridViewColumnTunerId.Name = "dataGridViewColumnTunerId";
      this.dataGridViewColumnTunerId.ReadOnly = true;
      this.dataGridViewColumnTunerId.Width = 45;
      // 
      // dataGridViewColumnTunerName
      // 
      this.dataGridViewColumnTunerName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.dataGridViewColumnTunerName.FillWeight = 33F;
      this.dataGridViewColumnTunerName.HeaderText = "Tuner Name";
      this.dataGridViewColumnTunerName.Name = "dataGridViewColumnTunerName";
      this.dataGridViewColumnTunerName.ReadOnly = true;
      // 
      // dataGridViewColumnTransceiver
      // 
      this.dataGridViewColumnTransceiver.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.dataGridViewColumnTransceiver.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
      this.dataGridViewColumnTransceiver.FillWeight = 33F;
      this.dataGridViewColumnTransceiver.HeaderText = "Transceiver";
      this.dataGridViewColumnTransceiver.Name = "dataGridViewColumnTransceiver";
      this.dataGridViewColumnTransceiver.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      // 
      // dataGridViewColumnTransmitPort
      // 
      this.dataGridViewColumnTransmitPort.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.dataGridViewColumnTransmitPort.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
      this.dataGridViewColumnTransmitPort.HeaderText = "Port";
      this.dataGridViewColumnTransmitPort.Name = "dataGridViewColumnTransmitPort";
      this.dataGridViewColumnTransmitPort.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.dataGridViewColumnTransmitPort.Width = 51;
      // 
      // dataGridViewColumnSetTopBoxProfile
      // 
      this.dataGridViewColumnSetTopBoxProfile.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.dataGridViewColumnSetTopBoxProfile.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
      this.dataGridViewColumnSetTopBoxProfile.FillWeight = 33F;
      this.dataGridViewColumnSetTopBoxProfile.HeaderText = "Set Top Box Profile";
      this.dataGridViewColumnSetTopBoxProfile.Name = "dataGridViewColumnSetTopBoxProfile";
      this.dataGridViewColumnSetTopBoxProfile.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGridViewColumnSetTopBoxProfile.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      // 
      // dataGridViewColumnPowerControl
      // 
      this.dataGridViewColumnPowerControl.HeaderText = "Power Control";
      this.dataGridViewColumnPowerControl.Name = "dataGridViewColumnPowerControl";
      this.dataGridViewColumnPowerControl.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.dataGridViewColumnPowerControl.Width = 50;
      // 
      // buttonTest
      // 
      this.buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTest.Location = new System.Drawing.Point(411, 387);
      this.buttonTest.Name = "buttonTest";
      this.buttonTest.Size = new System.Drawing.Size(60, 23);
      this.buttonTest.TabIndex = 3;
      this.buttonTest.Text = "&Test";
      this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
      // 
      // channelNumberUpDown
      // 
      this.channelNumberUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.channelNumberUpDown.Location = new System.Drawing.Point(325, 390);
      this.channelNumberUpDown.Name = "channelNumberUpDown";
      this.channelNumberUpDown.Size = new System.Drawing.Size(80, 20);
      this.channelNumberUpDown.TabIndex = 2;
      // 
      // buttonLearn
      // 
      this.buttonLearn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLearn.Location = new System.Drawing.Point(238, 387);
      this.buttonLearn.Name = "buttonLearn";
      this.buttonLearn.Size = new System.Drawing.Size(60, 23);
      this.buttonLearn.TabIndex = 1;
      this.buttonLearn.Text = "&Learn";
      this.buttonLearn.Click += new System.EventHandler(this.buttonLearn_Click);
      // 
      // pictureBoxLogo
      // 
      this.pictureBoxLogo.BackColor = System.Drawing.SystemColors.Window;
      this.pictureBoxLogo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxLogo.Image")));
      this.pictureBoxLogo.Location = new System.Drawing.Point(6, 4);
      this.pictureBoxLogo.Name = "pictureBoxLogo";
      this.pictureBoxLogo.Size = new System.Drawing.Size(399, 44);
      this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBoxLogo.TabIndex = 8;
      this.pictureBoxLogo.TabStop = false;
      // 
      // MicrosoftBlasterConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.pictureBoxLogo);
      this.Controls.Add(this.buttonLearn);
      this.Controls.Add(this.channelNumberUpDown);
      this.Controls.Add(this.buttonTest);
      this.Controls.Add(this.dataGridViewConfig);
      this.Name = "MicrosoftBlasterConfig";
      this.Size = new System.Drawing.Size(480, 420);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConfig)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private SetupControls.UserInterfaceControls.MPButton buttonTest;
    private SetupControls.UserInterfaceControls.MPChannelNumberUpDown channelNumberUpDown;
    private SetupControls.UserInterfaceControls.MPButton buttonLearn;
    private SetupControls.UserInterfaceControls.MPPictureBox pictureBoxLogo;
    private SetupControls.UserInterfaceControls.MPDataGridView dataGridViewConfig;
    private SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn dataGridViewColumnTunerId;
    private SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn dataGridViewColumnTunerName;
    private SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn dataGridViewColumnTransceiver;
    private SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn dataGridViewColumnTransmitPort;
    private SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn dataGridViewColumnSetTopBoxProfile;
    private SetupControls.UserInterfaceControls.MPDataGridViewCheckBoxColumn dataGridViewColumnPowerControl;
  }
}