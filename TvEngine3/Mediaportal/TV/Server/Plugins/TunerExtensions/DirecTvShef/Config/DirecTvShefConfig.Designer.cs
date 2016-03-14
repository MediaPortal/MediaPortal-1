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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.Config
{
  partial class DirecTvShefConfig
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DirecTvShefConfig));
      this.dataGridViewConfig = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridView();
      this.buttonTest = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.pictureBoxLogo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPictureBox();
      this.buttonSelectGenieMini = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.dataGridViewColumnTunerId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnTunerName = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnSetTopBoxIpAddress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnGenieMini = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewColumnPowerControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewCheckBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConfig)).BeginInit();
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
            this.dataGridViewColumnSetTopBoxIpAddress,
            this.dataGridViewColumnGenieMini,
            this.dataGridViewColumnPowerControl});
      this.dataGridViewConfig.Location = new System.Drawing.Point(6, 67);
      this.dataGridViewConfig.MultiSelect = false;
      this.dataGridViewConfig.Name = "dataGridViewConfig";
      this.dataGridViewConfig.RowHeadersVisible = false;
      this.dataGridViewConfig.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dataGridViewConfig.Size = new System.Drawing.Size(465, 314);
      this.dataGridViewConfig.TabIndex = 0;
      // 
      // buttonTest
      // 
      this.buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonTest.Location = new System.Drawing.Point(421, 387);
      this.buttonTest.Name = "buttonTest";
      this.buttonTest.Size = new System.Drawing.Size(50, 23);
      this.buttonTest.TabIndex = 2;
      this.buttonTest.Text = "&Test";
      this.buttonTest.UseVisualStyleBackColor = true;
      this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
      // 
      // pictureBoxLogo
      // 
      this.pictureBoxLogo.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxLogo.Image")));
      this.pictureBoxLogo.Location = new System.Drawing.Point(6, 6);
      this.pictureBoxLogo.Name = "pictureBoxLogo";
      this.pictureBoxLogo.Size = new System.Drawing.Size(185, 55);
      this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBoxLogo.TabIndex = 2;
      this.pictureBoxLogo.TabStop = false;
      // 
      // buttonSelectGenieMini
      // 
      this.buttonSelectGenieMini.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonSelectGenieMini.Location = new System.Drawing.Point(315, 387);
      this.buttonSelectGenieMini.Name = "buttonSelectGenieMini";
      this.buttonSelectGenieMini.Size = new System.Drawing.Size(100, 23);
      this.buttonSelectGenieMini.TabIndex = 1;
      this.buttonSelectGenieMini.Text = "&Select Genie Mini";
      this.buttonSelectGenieMini.UseVisualStyleBackColor = true;
      this.buttonSelectGenieMini.Click += new System.EventHandler(this.buttonSelectGenieMini_Click);
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
      this.dataGridViewColumnTunerName.FillWeight = 40F;
      this.dataGridViewColumnTunerName.HeaderText = "Tuner Name";
      this.dataGridViewColumnTunerName.Name = "dataGridViewColumnTunerName";
      this.dataGridViewColumnTunerName.ReadOnly = true;
      // 
      // dataGridViewColumnSetTopBoxIpAddress
      // 
      this.dataGridViewColumnSetTopBoxIpAddress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.dataGridViewColumnSetTopBoxIpAddress.FillWeight = 30F;
      this.dataGridViewColumnSetTopBoxIpAddress.HeaderText = "Receiver IP Address";
      this.dataGridViewColumnSetTopBoxIpAddress.Name = "dataGridViewColumnSetTopBoxIpAddress";
      this.dataGridViewColumnSetTopBoxIpAddress.Width = 84;
      // 
      // dataGridViewColumnGenieMini
      // 
      this.dataGridViewColumnGenieMini.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.dataGridViewColumnGenieMini.FillWeight = 30F;
      this.dataGridViewColumnGenieMini.HeaderText = "Genie Mini";
      this.dataGridViewColumnGenieMini.Name = "dataGridViewColumnGenieMini";
      this.dataGridViewColumnGenieMini.ReadOnly = true;
      this.dataGridViewColumnGenieMini.Width = 76;
      // 
      // dataGridViewColumnPowerControl
      // 
      this.dataGridViewColumnPowerControl.HeaderText = "Power Control";
      this.dataGridViewColumnPowerControl.Name = "dataGridViewColumnPowerControl";
      this.dataGridViewColumnPowerControl.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.dataGridViewColumnPowerControl.Width = 50;
      // 
      // DirecTvShefConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.buttonSelectGenieMini);
      this.Controls.Add(this.pictureBoxLogo);
      this.Controls.Add(this.buttonTest);
      this.Controls.Add(this.dataGridViewConfig);
      this.Name = "DirecTvShefConfig";
      this.Size = new System.Drawing.Size(480, 420);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConfig)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private SetupControls.UserInterfaceControls.MPDataGridView dataGridViewConfig;
    private SetupControls.UserInterfaceControls.MPButton buttonTest;
    private SetupControls.UserInterfaceControls.MPPictureBox pictureBoxLogo;
    private SetupControls.UserInterfaceControls.MPButton buttonSelectGenieMini;
    private SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn dataGridViewColumnTunerId;
    private SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn dataGridViewColumnTunerName;
    private SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn dataGridViewColumnSetTopBoxIpAddress;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewColumnGenieMini;
    private SetupControls.UserInterfaceControls.MPDataGridViewCheckBoxColumn dataGridViewColumnPowerControl;
  }
}