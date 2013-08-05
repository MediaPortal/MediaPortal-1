#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace ProcessPlugins.ViewModeSwitcher
{
  partial class ViewModeSwitcherConfig
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
            this.bOK = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbDisableLBForVideo = new System.Windows.Forms.CheckBox();
            this.cbDisableForVideo = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.fbosUpDown = new System.Windows.Forms.NumericUpDown();
            this.cmbFBViewMode = new System.Windows.Forms.ComboBox();
            this.cbUseFallbackRule = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cbVerboseLog = new System.Windows.Forms.CheckBox();
            this.bDelete = new System.Windows.Forms.Button();
            this.bModify = new System.Windows.Forms.Button();
            this.cbShowSwitchMsg = new System.Windows.Forms.CheckBox();
            this.dg_RuleSets = new System.Windows.Forms.DataGridView();
            this.ColEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColRuleName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColARFrom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColARTo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMinWidth = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMaxWidth = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMinHeight = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMaxHeight = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColViewMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColOverscan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColEnableLBDetect = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColAutoCrop = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMaxCrop = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bCancel = new System.Windows.Forms.Button();
            this.linkLabelForum = new System.Windows.Forms.LinkLabel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label16 = new System.Windows.Forms.Label();
            this.numDetectInterval = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.numSymLimit = new System.Windows.Forms.NumericUpDown();
            this.numMaxCropLimit = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.numBlackLevAve = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numBlackLevel = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numBBdetHeight = new System.Windows.Forms.NumericUpDown();
            this.label18 = new System.Windows.Forms.Label();
            this.cbDisableLBGlobaly = new System.Windows.Forms.CheckBox();
            this.numBBdetWidth = new System.Windows.Forms.NumericUpDown();
            this.bImport = new System.Windows.Forms.Button();
            this.bExport = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cbEnableAdvanced = new System.Windows.Forms.CheckBox();
            this.bLoadDefaults = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fbosUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dg_RuleSets)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDetectInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSymLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxCropLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlackLevAve)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlackLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBBdetHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBBdetWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // bOK
            // 
            this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bOK.Location = new System.Drawing.Point(672, 461);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(44, 23);
            this.bOK.TabIndex = 1;
            this.bOK.Text = "&OK";
            this.bOK.UseVisualStyleBackColor = true;
            this.bOK.Click += new System.EventHandler(this.bOK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cbDisableLBForVideo);
            this.groupBox1.Controls.Add(this.cbDisableForVideo);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.fbosUpDown);
            this.groupBox1.Controls.Add(this.cmbFBViewMode);
            this.groupBox1.Controls.Add(this.cbUseFallbackRule);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.cbVerboseLog);
            this.groupBox1.Controls.Add(this.bDelete);
            this.groupBox1.Controls.Add(this.bModify);
            this.groupBox1.Controls.Add(this.cbShowSwitchMsg);
            this.groupBox1.Controls.Add(this.dg_RuleSets);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(770, 286);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // cbDisableLBForVideo
            // 
            this.cbDisableLBForVideo.AutoSize = true;
            this.cbDisableLBForVideo.Location = new System.Drawing.Point(219, 239);
            this.cbDisableLBForVideo.Name = "cbDisableLBForVideo";
            this.cbDisableLBForVideo.Size = new System.Drawing.Size(164, 17);
            this.cbDisableLBForVideo.TabIndex = 14;
            this.cbDisableLBForVideo.Text = "Disable BB detect for non-TV";
            this.cbDisableLBForVideo.Visible = false;
            // 
            // cbDisableForVideo
            // 
            this.cbDisableForVideo.AutoSize = true;
            this.cbDisableForVideo.Location = new System.Drawing.Point(219, 216);
            this.cbDisableForVideo.Name = "cbDisableForVideo";
            this.cbDisableForVideo.Size = new System.Drawing.Size(114, 17);
            this.cbDisableForVideo.TabIndex = 13;
            this.cbDisableForVideo.Text = "Disable for non-TV";
            this.cbDisableForVideo.Visible = false;
            this.cbDisableForVideo.CheckedChanged += new System.EventHandler(this.cbDisableForVideo_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(216, 263);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Fallback ZoomMode:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(464, 263);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Fallback overscan:";
            // 
            // fbosUpDown
            // 
            this.fbosUpDown.Location = new System.Drawing.Point(567, 259);
            this.fbosUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.fbosUpDown.Name = "fbosUpDown";
            this.fbosUpDown.Size = new System.Drawing.Size(41, 20);
            this.fbosUpDown.TabIndex = 10;
            // 
            // cmbFBViewMode
            // 
            this.cmbFBViewMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFBViewMode.Location = new System.Drawing.Point(327, 258);
            this.cmbFBViewMode.Name = "cmbFBViewMode";
            this.cmbFBViewMode.Size = new System.Drawing.Size(121, 21);
            this.cmbFBViewMode.TabIndex = 9;
            // 
            // cbUseFallbackRule
            // 
            this.cbUseFallbackRule.AutoSize = true;
            this.cbUseFallbackRule.Location = new System.Drawing.Point(6, 262);
            this.cbUseFallbackRule.Name = "cbUseFallbackRule";
            this.cbUseFallbackRule.Size = new System.Drawing.Size(190, 17);
            this.cbUseFallbackRule.TabIndex = 8;
            this.cbUseFallbackRule.Text = "Use fallback rule if no match found";
            this.cbUseFallbackRule.UseVisualStyleBackColor = true;
            this.cbUseFallbackRule.CheckedChanged += new System.EventHandler(this.cbUseFallbackRule_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(461, 215);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(91, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "&Add New Rule";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.bAdd_Click);
            // 
            // cbVerboseLog
            // 
            this.cbVerboseLog.AutoSize = true;
            this.cbVerboseLog.Location = new System.Drawing.Point(6, 216);
            this.cbVerboseLog.Name = "cbVerboseLog";
            this.cbVerboseLog.Size = new System.Drawing.Size(137, 17);
            this.cbVerboseLog.TabIndex = 0;
            this.cbVerboseLog.Text = "Enable verbose logging";
            this.cbVerboseLog.Visible = false;
            // 
            // bDelete
            // 
            this.bDelete.Location = new System.Drawing.Point(664, 215);
            this.bDelete.Name = "bDelete";
            this.bDelete.Size = new System.Drawing.Size(100, 23);
            this.bDelete.TabIndex = 2;
            this.bDelete.Text = "&Delete Selected";
            this.bDelete.UseVisualStyleBackColor = true;
            this.bDelete.Click += new System.EventHandler(this.bDelete_Click);
            // 
            // bModify
            // 
            this.bModify.Location = new System.Drawing.Point(558, 215);
            this.bModify.Name = "bModify";
            this.bModify.Size = new System.Drawing.Size(100, 23);
            this.bModify.TabIndex = 1;
            this.bModify.Text = "&Modify Selected";
            this.bModify.UseVisualStyleBackColor = true;
            this.bModify.Click += new System.EventHandler(this.bModify_Click);
            // 
            // cbShowSwitchMsg
            // 
            this.cbShowSwitchMsg.AutoSize = true;
            this.cbShowSwitchMsg.Location = new System.Drawing.Point(6, 239);
            this.cbShowSwitchMsg.Name = "cbShowSwitchMsg";
            this.cbShowSwitchMsg.Size = new System.Drawing.Size(194, 17);
            this.cbShowSwitchMsg.TabIndex = 3;
            this.cbShowSwitchMsg.Text = "Show message when rule is applied";
            this.cbShowSwitchMsg.UseVisualStyleBackColor = true;
            // 
            // dg_RuleSets
            // 
            this.dg_RuleSets.AllowUserToAddRows = false;
            this.dg_RuleSets.AllowUserToDeleteRows = false;
            this.dg_RuleSets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dg_RuleSets.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dg_RuleSets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dg_RuleSets.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColEnabled,
            this.ColRuleName,
            this.ColARFrom,
            this.ColARTo,
            this.ColMinWidth,
            this.ColMaxWidth,
            this.ColMinHeight,
            this.ColMaxHeight,
            this.ColViewMode,
            this.ColOverscan,
            this.ColEnableLBDetect,
            this.ColAutoCrop,
            this.ColMaxCrop});
            this.dg_RuleSets.Location = new System.Drawing.Point(6, 16);
            this.dg_RuleSets.MultiSelect = false;
            this.dg_RuleSets.Name = "dg_RuleSets";
            this.dg_RuleSets.ReadOnly = true;
            this.dg_RuleSets.RowHeadersVisible = false;
            this.dg_RuleSets.Size = new System.Drawing.Size(758, 193);
            this.dg_RuleSets.TabIndex = 0;
            this.dg_RuleSets.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dg_RuleSets_CellContentClick);
            // 
            // ColEnabled
            // 
            this.ColEnabled.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColEnabled.HeaderText = "Enabled";
            this.ColEnabled.Name = "ColEnabled";
            this.ColEnabled.ReadOnly = true;
            this.ColEnabled.Width = 52;
            // 
            // ColRuleName
            // 
            this.ColRuleName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColRuleName.HeaderText = "Name";
            this.ColRuleName.Name = "ColRuleName";
            this.ColRuleName.ReadOnly = true;
            this.ColRuleName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColRuleName.Width = 41;
            // 
            // ColARFrom
            // 
            this.ColARFrom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColARFrom.HeaderText = "AR From";
            this.ColARFrom.Name = "ColARFrom";
            this.ColARFrom.ReadOnly = true;
            this.ColARFrom.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColARFrom.Width = 54;
            // 
            // ColARTo
            // 
            this.ColARTo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColARTo.HeaderText = "AR To";
            this.ColARTo.Name = "ColARTo";
            this.ColARTo.ReadOnly = true;
            this.ColARTo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColARTo.Width = 44;
            // 
            // ColMinWidth
            // 
            this.ColMinWidth.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColMinWidth.HeaderText = "Width From";
            this.ColMinWidth.Name = "ColMinWidth";
            this.ColMinWidth.ReadOnly = true;
            this.ColMinWidth.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColMinWidth.Width = 67;
            // 
            // ColMaxWidth
            // 
            this.ColMaxWidth.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColMaxWidth.HeaderText = "Width To";
            this.ColMaxWidth.Name = "ColMaxWidth";
            this.ColMaxWidth.ReadOnly = true;
            this.ColMaxWidth.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColMaxWidth.Width = 57;
            // 
            // ColMinHeight
            // 
            this.ColMinHeight.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColMinHeight.HeaderText = "Height From";
            this.ColMinHeight.Name = "ColMinHeight";
            this.ColMinHeight.ReadOnly = true;
            this.ColMinHeight.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColMinHeight.Width = 70;
            // 
            // ColMaxHeight
            // 
            this.ColMaxHeight.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColMaxHeight.HeaderText = "Height To";
            this.ColMaxHeight.Name = "ColMaxHeight";
            this.ColMaxHeight.ReadOnly = true;
            this.ColMaxHeight.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColMaxHeight.Width = 60;
            // 
            // ColViewMode
            // 
            this.ColViewMode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColViewMode.HeaderText = "ZoomMode";
            this.ColViewMode.Name = "ColViewMode";
            this.ColViewMode.ReadOnly = true;
            this.ColViewMode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColViewMode.Width = 67;
            // 
            // ColOverscan
            // 
            this.ColOverscan.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColOverscan.HeaderText = "Overscan";
            this.ColOverscan.Name = "ColOverscan";
            this.ColOverscan.ReadOnly = true;
            this.ColOverscan.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColOverscan.Width = 59;
            // 
            // ColEnableLBDetect
            // 
            this.ColEnableLBDetect.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColEnableLBDetect.HeaderText = "Init BB Det";
            this.ColEnableLBDetect.Name = "ColEnableLBDetect";
            this.ColEnableLBDetect.ReadOnly = true;
            this.ColEnableLBDetect.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColEnableLBDetect.Width = 64;
            // 
            // ColAutoCrop
            // 
            this.ColAutoCrop.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColAutoCrop.HeaderText = "Cont BB Det";
            this.ColAutoCrop.Name = "ColAutoCrop";
            this.ColAutoCrop.ReadOnly = true;
            this.ColAutoCrop.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColAutoCrop.Width = 72;
            // 
            // ColMaxCrop
            // 
            this.ColMaxCrop.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ColMaxCrop.HeaderText = "MaxCrop";
            this.ColMaxCrop.Name = "ColMaxCrop";
            this.ColMaxCrop.ReadOnly = true;
            this.ColMaxCrop.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColMaxCrop.Width = 55;
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(726, 461);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(50, 23);
            this.bCancel.TabIndex = 0;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // linkLabelForum
            // 
            this.linkLabelForum.AutoSize = true;
            this.linkLabelForum.Location = new System.Drawing.Point(128, 466);
            this.linkLabelForum.Name = "linkLabelForum";
            this.linkLabelForum.Size = new System.Drawing.Size(266, 13);
            this.linkLabelForum.TabIndex = 7;
            this.linkLabelForum.TabStop = true;
            this.linkLabelForum.Text = "Click here to jump to the related MediaPortal Wiki page";
            this.linkLabelForum.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelForum_LinkClicked);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.numDetectInterval);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.numSymLimit);
            this.groupBox2.Controls.Add(this.numMaxCropLimit);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.numBlackLevAve);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.numBlackLevel);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.numBBdetHeight);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.cbDisableLBGlobaly);
            this.groupBox2.Controls.Add(this.numBBdetWidth);
            this.groupBox2.Location = new System.Drawing.Point(12, 313);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(770, 81);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Black Bar (BB) detection options";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(6, 39);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(150, 13);
            this.label16.TabIndex = 51;
            this.label16.Text = "Detection interval (0.25s units)";
            this.label16.Visible = false;
            // 
            // numDetectInterval
            // 
            this.numDetectInterval.Location = new System.Drawing.Point(44, 57);
            this.numDetectInterval.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.numDetectInterval.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numDetectInterval.Name = "numDetectInterval";
            this.numDetectInterval.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.numDetectInterval.Size = new System.Drawing.Size(43, 20);
            this.numDetectInterval.TabIndex = 50;
            this.numDetectInterval.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numDetectInterval.Visible = false;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(243, 11);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(59, 13);
            this.label15.TabIndex = 49;
            this.label15.Text = "Processing";
            this.label15.Visible = false;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(355, 11);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(145, 13);
            this.label14.TabIndex = 48;
            this.label14.Text = "Search limits (% of frame size)";
            this.label14.Visible = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(148, 61);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(122, 13);
            this.label13.TabIndex = 47;
            this.label13.Text = "Symmetry check limit (%)";
            this.label13.Visible = false;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(186, 35);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(86, 13);
            this.label12.TabIndex = 45;
            this.label12.Text = "MaxCrop limit (%)";
            this.label12.Visible = false;
            // 
            // numSymLimit
            // 
            this.numSymLimit.Location = new System.Drawing.Point(280, 57);
            this.numSymLimit.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numSymLimit.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numSymLimit.Name = "numSymLimit";
            this.numSymLimit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.numSymLimit.Size = new System.Drawing.Size(43, 20);
            this.numSymLimit.TabIndex = 46;
            this.numSymLimit.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numSymLimit.Visible = false;
            // 
            // numMaxCropLimit
            // 
            this.numMaxCropLimit.Location = new System.Drawing.Point(280, 31);
            this.numMaxCropLimit.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numMaxCropLimit.Name = "numMaxCropLimit";
            this.numMaxCropLimit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.numMaxCropLimit.Size = new System.Drawing.Size(43, 20);
            this.numMaxCropLimit.TabIndex = 44;
            this.numMaxCropLimit.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.numMaxCropLimit.Visible = false;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(519, 61);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(86, 13);
            this.label11.TabIndex = 43;
            this.label11.Text = "For 25% of pixels";
            this.label11.Visible = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(524, 35);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(81, 13);
            this.label10.TabIndex = 42;
            this.label10.Text = "For single pixels";
            this.label10.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(661, 61);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(99, 13);
            this.label9.TabIndex = 41;
            this.label9.Text = "(4 - 255, default 16)";
            this.label9.Visible = false;
            // 
            // numBlackLevAve
            // 
            this.numBlackLevAve.Location = new System.Drawing.Point(612, 57);
            this.numBlackLevAve.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numBlackLevAve.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numBlackLevAve.Name = "numBlackLevAve";
            this.numBlackLevAve.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.numBlackLevAve.Size = new System.Drawing.Size(43, 20);
            this.numBlackLevAve.TabIndex = 40;
            this.numBlackLevAve.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numBlackLevAve.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(357, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(99, 13);
            this.label8.TabIndex = 39;
            this.label8.Text = "Left/right height (%)";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label8.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(661, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 35;
            this.label1.Text = "(4 - 255, default 32)\r\n";
            this.label1.Visible = false;
            // 
            // numBlackLevel
            // 
            this.numBlackLevel.Location = new System.Drawing.Point(612, 31);
            this.numBlackLevel.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numBlackLevel.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numBlackLevel.Name = "numBlackLevel";
            this.numBlackLevel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.numBlackLevel.Size = new System.Drawing.Size(43, 20);
            this.numBlackLevel.TabIndex = 34;
            this.numBlackLevel.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numBlackLevel.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(345, 61);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(111, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "Top/bottom width (%) ";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label7.Visible = false;
            // 
            // numBBdetHeight
            // 
            this.numBBdetHeight.Location = new System.Drawing.Point(461, 31);
            this.numBBdetHeight.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numBBdetHeight.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numBBdetHeight.Name = "numBBdetHeight";
            this.numBBdetHeight.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.numBBdetHeight.Size = new System.Drawing.Size(43, 20);
            this.numBBdetHeight.TabIndex = 37;
            this.numBBdetHeight.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numBBdetHeight.Visible = false;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(578, 11);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(110, 13);
            this.label18.TabIndex = 33;
            this.label18.Text = "Black level thresholds";
            this.label18.Visible = false;
            // 
            // cbDisableLBGlobaly
            // 
            this.cbDisableLBGlobaly.AutoSize = true;
            this.cbDisableLBGlobaly.Location = new System.Drawing.Point(6, 19);
            this.cbDisableLBGlobaly.Name = "cbDisableLBGlobaly";
            this.cbDisableLBGlobaly.Size = new System.Drawing.Size(170, 17);
            this.cbDisableLBGlobaly.TabIndex = 0;
            this.cbDisableLBGlobaly.Text = "Disable all Black Bar detection";
            this.cbDisableLBGlobaly.UseVisualStyleBackColor = true;
            // 
            // numBBdetWidth
            // 
            this.numBBdetWidth.Location = new System.Drawing.Point(461, 57);
            this.numBBdetWidth.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numBBdetWidth.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numBBdetWidth.Name = "numBBdetWidth";
            this.numBBdetWidth.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.numBBdetWidth.Size = new System.Drawing.Size(43, 20);
            this.numBBdetWidth.TabIndex = 36;
            this.numBBdetWidth.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numBBdetWidth.Visible = false;
            // 
            // bImport
            // 
            this.bImport.Location = new System.Drawing.Point(543, 461);
            this.bImport.Name = "bImport";
            this.bImport.Size = new System.Drawing.Size(88, 23);
            this.bImport.TabIndex = 35;
            this.bImport.Text = "Import Settings";
            this.bImport.UseVisualStyleBackColor = true;
            this.bImport.Click += new System.EventHandler(this.bImport_Click);
            // 
            // bExport
            // 
            this.bExport.Location = new System.Drawing.Point(444, 461);
            this.bExport.Name = "bExport";
            this.bExport.Size = new System.Drawing.Size(93, 23);
            this.bExport.TabIndex = 36;
            this.bExport.Text = "Export Settings";
            this.bExport.UseVisualStyleBackColor = true;
            this.bExport.Click += new System.EventHandler(this.bExport_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 421);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(402, 13);
            this.label4.TabIndex = 37;
            this.label4.Text = "Rules are checked from top to bottom, and checking stops at the first matching ru" +
    "le.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(15, 397);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 15);
            this.label5.TabIndex = 38;
            this.label5.Text = "Notes";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 434);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(548, 13);
            this.label6.TabIndex = 39;
            this.label6.Text = "Rules with negative AR values are only used for letterbox/pillarbox video detecti" +
    "on (when BB detection is enabled).";
            // 
            // cbEnableAdvanced
            // 
            this.cbEnableAdvanced.AutoSize = true;
            this.cbEnableAdvanced.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbEnableAdvanced.Location = new System.Drawing.Point(586, 403);
            this.cbEnableAdvanced.Name = "cbEnableAdvanced";
            this.cbEnableAdvanced.Size = new System.Drawing.Size(182, 19);
            this.cbEnableAdvanced.TabIndex = 40;
            this.cbEnableAdvanced.Text = "Show Advanced Settings";
            this.cbEnableAdvanced.UseVisualStyleBackColor = true;
            this.cbEnableAdvanced.CheckedChanged += new System.EventHandler(this.cbEnableAdvanced_CheckedChanged);
            // 
            // bLoadDefaults
            // 
            this.bLoadDefaults.Location = new System.Drawing.Point(14, 461);
            this.bLoadDefaults.Name = "bLoadDefaults";
            this.bLoadDefaults.Size = new System.Drawing.Size(88, 23);
            this.bLoadDefaults.TabIndex = 41;
            this.bLoadDefaults.Text = "Load Defaults";
            this.bLoadDefaults.UseVisualStyleBackColor = true;
            this.bLoadDefaults.Click += new System.EventHandler(this.bLoadDefaults_Click);
            // 
            // ViewModeSwitcherConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(794, 496);
            this.Controls.Add(this.bLoadDefaults);
            this.Controls.Add(this.cbEnableAdvanced);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.bExport);
            this.Controls.Add(this.bImport);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.linkLabelForum);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.bOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximumSize = new System.Drawing.Size(1000, 530);
            this.MinimumSize = new System.Drawing.Size(810, 530);
            this.Name = "ViewModeSwitcherConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ViewModeSwitcherConfig";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fbosUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dg_RuleSets)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDetectInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSymLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxCropLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlackLevAve)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBlackLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBBdetHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBBdetWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button bOK;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.DataGridView dg_RuleSets;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button bDelete;
    private System.Windows.Forms.Button bModify;
    private System.Windows.Forms.Button bCancel;
    private System.Windows.Forms.CheckBox cbShowSwitchMsg;
    private System.Windows.Forms.CheckBox cbVerboseLog;
    private System.Windows.Forms.LinkLabel linkLabelForum;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox cbDisableLBGlobaly;
    private System.Windows.Forms.Label label18;
    private System.Windows.Forms.CheckBox cbUseFallbackRule;
    private System.Windows.Forms.ComboBox cmbFBViewMode;
    private System.Windows.Forms.Button bImport;
    private System.Windows.Forms.Button bExport;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.SaveFileDialog saveFileDialog;
    private System.Windows.Forms.NumericUpDown numBlackLevel;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.NumericUpDown fbosUpDown;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.NumericUpDown numBBdetHeight;
    private System.Windows.Forms.NumericUpDown numBBdetWidth;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.NumericUpDown numBlackLevAve;
    private System.Windows.Forms.CheckBox cbDisableForVideo;
    private System.Windows.Forms.CheckBox cbDisableLBForVideo;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.NumericUpDown numMaxCropLimit;
    private System.Windows.Forms.CheckBox cbEnableAdvanced;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.NumericUpDown numSymLimit;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.NumericUpDown numDetectInterval;
    private System.Windows.Forms.Button bLoadDefaults;
    private System.Windows.Forms.DataGridViewCheckBoxColumn ColEnabled;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColRuleName;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColARFrom;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColARTo;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMinWidth;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMaxWidth;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMinHeight;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMaxHeight;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColViewMode;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColOverscan;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColEnableLBDetect;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColAutoCrop;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMaxCrop;
  }
}