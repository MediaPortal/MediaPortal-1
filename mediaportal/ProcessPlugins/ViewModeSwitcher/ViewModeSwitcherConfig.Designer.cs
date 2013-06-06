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
      this.ColAutoCrop = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColViewMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColMaxCrop = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColOverscan = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColEnableLBDetect = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.bCancel = new System.Windows.Forms.Button();
      this.linkLabelForum = new System.Windows.Forms.LinkLabel();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.numBlackLevel = new System.Windows.Forms.NumericUpDown();
      this.label18 = new System.Windows.Forms.Label();
      this.cbDisableLBGlobaly = new System.Windows.Forms.CheckBox();
      this.bImport = new System.Windows.Forms.Button();
      this.bExport = new System.Windows.Forms.Button();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
      this.fbosUpDown = new System.Windows.Forms.NumericUpDown();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dg_RuleSets)).BeginInit();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numBlackLevel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fbosUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // bOK
      // 
      this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bOK.Location = new System.Drawing.Point(738, 461);
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
      // cmbFBViewMode
      // 
      this.cmbFBViewMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbFBViewMode.Location = new System.Drawing.Point(186, 260);
      this.cmbFBViewMode.Name = "cmbFBViewMode";
      this.cmbFBViewMode.Size = new System.Drawing.Size(121, 21);
      this.cmbFBViewMode.TabIndex = 9;
      // 
      // cbUseFallbackRule
      // 
      this.cbUseFallbackRule.AutoSize = true;
      this.cbUseFallbackRule.Location = new System.Drawing.Point(6, 262);
      this.cbUseFallbackRule.Name = "cbUseFallbackRule";
      this.cbUseFallbackRule.Size = new System.Drawing.Size(174, 17);
      this.cbUseFallbackRule.TabIndex = 8;
      this.cbUseFallbackRule.Text = "Fallback rule if no match found:";
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
      this.cbShowSwitchMsg.Size = new System.Drawing.Size(276, 17);
      this.cbShowSwitchMsg.TabIndex = 3;
      this.cbShowSwitchMsg.Text = "Show message when rule is applied (good for testing)";
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
            this.ColAutoCrop,
            this.ColViewMode,
            this.ColMaxCrop,
            this.ColOverscan,
            this.ColEnableLBDetect});
      this.dg_RuleSets.Location = new System.Drawing.Point(6, 16);
      this.dg_RuleSets.MultiSelect = false;
      this.dg_RuleSets.Name = "dg_RuleSets";
      this.dg_RuleSets.ReadOnly = true;
      this.dg_RuleSets.RowHeadersVisible = false;
      this.dg_RuleSets.Size = new System.Drawing.Size(758, 193);
      this.dg_RuleSets.TabIndex = 0;
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
      this.ColRuleName.Width = 60;
      // 
      // ColARFrom
      // 
      this.ColARFrom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColARFrom.HeaderText = "AR From";
      this.ColARFrom.Name = "ColARFrom";
      this.ColARFrom.ReadOnly = true;
      this.ColARFrom.Width = 73;
      // 
      // ColARTo
      // 
      this.ColARTo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColARTo.HeaderText = "AR To";
      this.ColARTo.Name = "ColARTo";
      this.ColARTo.ReadOnly = true;
      this.ColARTo.Width = 63;
      // 
      // ColMinWidth
      // 
      this.ColMinWidth.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColMinWidth.HeaderText = "MinWidth";
      this.ColMinWidth.Name = "ColMinWidth";
      this.ColMinWidth.ReadOnly = true;
      this.ColMinWidth.Width = 77;
      // 
      // ColMaxWidth
      // 
      this.ColMaxWidth.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColMaxWidth.HeaderText = "MaxWidth";
      this.ColMaxWidth.Name = "ColMaxWidth";
      this.ColMaxWidth.ReadOnly = true;
      this.ColMaxWidth.Width = 80;
      // 
      // ColMinHeight
      // 
      this.ColMinHeight.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColMinHeight.HeaderText = "MinHeight";
      this.ColMinHeight.Name = "ColMinHeight";
      this.ColMinHeight.ReadOnly = true;
      this.ColMinHeight.Width = 80;
      // 
      // ColMaxHeight
      // 
      this.ColMaxHeight.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColMaxHeight.HeaderText = "MaxHeight";
      this.ColMaxHeight.Name = "ColMaxHeight";
      this.ColMaxHeight.ReadOnly = true;
      this.ColMaxHeight.Width = 83;
      // 
      // ColAutoCrop
      // 
      this.ColAutoCrop.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      //this.ColAutoCrop.HeaderText = "ChangeAR";
      this.ColAutoCrop.HeaderText = "Auto LB";
      this.ColAutoCrop.Name = "ColAutoCrop";
      this.ColAutoCrop.ReadOnly = true;
      this.ColAutoCrop.Width = 84;
      // 
      // ColViewMode
      // 
      this.ColViewMode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColViewMode.HeaderText = "ViewMode";
      this.ColViewMode.Name = "ColViewMode";
      this.ColViewMode.ReadOnly = true;
      this.ColViewMode.Width = 82;
      // 
      // ColMaxCrop
      // 
      this.ColMaxCrop.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      //this.ColMaxCrop.HeaderText = "ChangeOS";
      this.ColMaxCrop.HeaderText = "Max crop";
      this.ColMaxCrop.Name = "ColMaxCrop";
      this.ColMaxCrop.ReadOnly = true;
      this.ColMaxCrop.Width = 84;
      // 
      // ColOverscan
      // 
      this.ColOverscan.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColOverscan.HeaderText = "Overscan";
      this.ColOverscan.Name = "ColOverscan";
      this.ColOverscan.ReadOnly = true;
      this.ColOverscan.Width = 78;
      // 
      // ColEnableLBDetect
      // 
      this.ColEnableLBDetect.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColEnableLBDetect.HeaderText = "LB Detection";
      this.ColEnableLBDetect.Name = "ColEnableLBDetect";
      this.ColEnableLBDetect.ReadOnly = true;
      this.ColEnableLBDetect.Width = 94;
      // 
      // bCancel
      // 
      this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bCancel.Location = new System.Drawing.Point(682, 461);
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
      this.linkLabelForum.Location = new System.Drawing.Point(12, 466);
      this.linkLabelForum.Name = "linkLabelForum";
      this.linkLabelForum.Size = new System.Drawing.Size(282, 13);
      this.linkLabelForum.TabIndex = 7;
      this.linkLabelForum.TabStop = true;
      this.linkLabelForum.Text = "Click here to jump to the related MediaPortal forums thread";
      this.linkLabelForum.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelForum_LinkClicked);
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Controls.Add(this.numBlackLevel);
      this.groupBox2.Controls.Add(this.label18);
      this.groupBox2.Controls.Add(this.cbDisableLBGlobaly);
      this.groupBox2.Location = new System.Drawing.Point(12, 316);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(770, 78);
      this.groupBox2.TabIndex = 8;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Letterbox Options";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(163, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(105, 13);
      this.label1.TabIndex = 35;
      this.label1.Text = "(16 - 255, default 40)";
      // 
      // numBlackLevel
      // 
      this.numBlackLevel.Location = new System.Drawing.Point(114, 46);
      this.numBlackLevel.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.numBlackLevel.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
      this.numBlackLevel.Name = "numBlackLevel";
      this.numBlackLevel.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.numBlackLevel.Size = new System.Drawing.Size(43, 20);
      this.numBlackLevel.TabIndex = 34;
      this.numBlackLevel.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
      // 
      // label18
      // 
      this.label18.AutoSize = true;
      this.label18.Location = new System.Drawing.Point(6, 48);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(102, 13);
      this.label18.TabIndex = 33;
      this.label18.Text = "Black level treshold:";
      // 
      // cbDisableLBGlobaly
      // 
      this.cbDisableLBGlobaly.AutoSize = true;
      this.cbDisableLBGlobaly.Location = new System.Drawing.Point(6, 19);
      this.cbDisableLBGlobaly.Name = "cbDisableLBGlobaly";
      this.cbDisableLBGlobaly.Size = new System.Drawing.Size(311, 17);
      this.cbDisableLBGlobaly.TabIndex = 0;
      this.cbDisableLBGlobaly.Text = "Disable letterbox detection globally (even if enabled in a rule)";
      this.cbDisableLBGlobaly.UseVisualStyleBackColor = true;
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
      // fbosUpDown
      // 
      this.fbosUpDown.Location = new System.Drawing.Point(426, 261);
      this.fbosUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
      this.fbosUpDown.Name = "fbosUpDown";
      this.fbosUpDown.Size = new System.Drawing.Size(41, 20);
      this.fbosUpDown.TabIndex = 10;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(323, 263);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(97, 13);
      this.label2.TabIndex = 11;
      this.label2.Text = "Fallback overscan:";
      // 
      // ViewModeSwitcherConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(794, 496);
      this.Controls.Add(this.bExport);
      this.Controls.Add(this.bImport);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.linkLabelForum);
      this.Controls.Add(this.bCancel);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.bOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "ViewModeSwitcherConfig";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "ViewModeSwitcherConfig";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dg_RuleSets)).EndInit();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numBlackLevel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fbosUpDown)).EndInit();
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
    private System.Windows.Forms.DataGridViewCheckBoxColumn ColEnabled;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColRuleName;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColARFrom;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColARTo;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMinWidth;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMaxWidth;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMinHeight;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMaxHeight;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColAutoCrop;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColViewMode;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColMaxCrop;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColOverscan;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColEnableLBDetect;
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
  }
}