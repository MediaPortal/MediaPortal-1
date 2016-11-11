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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Config
{
  partial class UsbUirtConfig
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
      this.dataGridViewConfig = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridView();
      this.dataGridViewColumnTunerId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnTunerName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnUsbUirt = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn();
      this.dataGridViewColumnTransmitZone = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn();
      this.dataGridViewColumnSetTopBoxProfile = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn();
      this.dataGridViewColumnPowerControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewCheckBoxColumn();
      this.buttonTest = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.channelNumberUpDownTest = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPChannelNumberUpDown();
      this.buttonLearn = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConfig)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownTest)).BeginInit();
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
            this.dataGridViewColumnUsbUirt,
            this.dataGridViewColumnTransmitZone,
            this.dataGridViewColumnSetTopBoxProfile,
            this.dataGridViewColumnPowerControl});
      this.dataGridViewConfig.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
      this.dataGridViewConfig.Location = new System.Drawing.Point(6, 6);
      this.dataGridViewConfig.MultiSelect = false;
      this.dataGridViewConfig.Name = "dataGridViewConfig";
      this.dataGridViewConfig.RowHeadersVisible = false;
      this.dataGridViewConfig.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViewConfig.Size = new System.Drawing.Size(465, 375);
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
      this.dataGridViewColumnTunerName.FillWeight = 50F;
      this.dataGridViewColumnTunerName.HeaderText = "Tuner Name";
      this.dataGridViewColumnTunerName.Name = "dataGridViewColumnTunerName";
      this.dataGridViewColumnTunerName.ReadOnly = true;
      // 
      // dataGridViewColumnUsbUirt
      // 
      this.dataGridViewColumnUsbUirt.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.dataGridViewColumnUsbUirt.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
      this.dataGridViewColumnUsbUirt.HeaderText = "USB-UIRT";
      this.dataGridViewColumnUsbUirt.Name = "dataGridViewColumnUsbUirt";
      this.dataGridViewColumnUsbUirt.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.dataGridViewColumnUsbUirt.Width = 83;
      // 
      // dataGridViewColumnTransmitZone
      // 
      this.dataGridViewColumnTransmitZone.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.dataGridViewColumnTransmitZone.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
      this.dataGridViewColumnTransmitZone.HeaderText = "Zone";
      this.dataGridViewColumnTransmitZone.Name = "dataGridViewColumnTransmitZone";
      this.dataGridViewColumnTransmitZone.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.dataGridViewColumnTransmitZone.Width = 57;
      // 
      // dataGridViewColumnSetTopBoxProfile
      // 
      this.dataGridViewColumnSetTopBoxProfile.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.dataGridViewColumnSetTopBoxProfile.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
      this.dataGridViewColumnSetTopBoxProfile.FillWeight = 50F;
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
      this.buttonTest.UseVisualStyleBackColor = true;
      this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
      // 
      // channelNumberUpDownTest
      // 
      this.channelNumberUpDownTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.channelNumberUpDownTest.DecimalPlaces = 3;
      this.channelNumberUpDownTest.Location = new System.Drawing.Point(325, 390);
      this.channelNumberUpDownTest.Maximum = new decimal(new int[] {
            65535999,
            0,
            0,
            196608});
      this.channelNumberUpDownTest.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.channelNumberUpDownTest.Name = "channelNumberUpDownTest";
      this.channelNumberUpDownTest.Size = new System.Drawing.Size(80, 20);
      this.channelNumberUpDownTest.TabIndex = 2;
      this.channelNumberUpDownTest.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.channelNumberUpDownTest.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // buttonLearn
      // 
      this.buttonLearn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonLearn.Location = new System.Drawing.Point(238, 387);
      this.buttonLearn.Name = "buttonLearn";
      this.buttonLearn.Size = new System.Drawing.Size(60, 23);
      this.buttonLearn.TabIndex = 1;
      this.buttonLearn.Text = "&Learn";
      this.buttonLearn.UseVisualStyleBackColor = true;
      this.buttonLearn.Click += new System.EventHandler(this.buttonLearn_Click);
      // 
      // UsbUirtConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.buttonLearn);
      this.Controls.Add(this.channelNumberUpDownTest);
      this.Controls.Add(this.buttonTest);
      this.Controls.Add(this.dataGridViewConfig);
      this.Name = "UsbUirtConfig";
      this.Size = new System.Drawing.Size(480, 420);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConfig)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.channelNumberUpDownTest)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private SetupControls.UserInterfaceControls.MPButton buttonTest;
    private SetupControls.UserInterfaceControls.MPChannelNumberUpDown channelNumberUpDownTest;
    private SetupControls.UserInterfaceControls.MPButton buttonLearn;
    private SetupControls.UserInterfaceControls.MPDataGridView dataGridViewConfig;
    private SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn dataGridViewColumnTunerId;
    private SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn dataGridViewColumnTunerName;
    private SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn dataGridViewColumnUsbUirt;
    private SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn dataGridViewColumnTransmitZone;
    private SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn dataGridViewColumnSetTopBoxProfile;
    private SetupControls.UserInterfaceControls.MPDataGridViewCheckBoxColumn dataGridViewColumnPowerControl;
  }
}