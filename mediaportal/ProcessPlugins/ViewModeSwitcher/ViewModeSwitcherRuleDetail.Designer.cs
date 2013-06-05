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
  partial class ViewModeSwitcherRuleDetail
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
      this.cbEnabled = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cb_EnableLBDetection = new System.Windows.Forms.CheckBox();
      this.label9 = new System.Windows.Forms.Label();
      this.txbOverScan = new System.Windows.Forms.TextBox();
      this.cmbViewMode = new System.Windows.Forms.ComboBox();
      this.txbARTo = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.txbARFrom = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.txbMaxHeight = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.txbMinHeight = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.txbMaxWidth = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.txbMinWidth = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.txbName = new System.Windows.Forms.TextBox();
      this.lblRulename = new System.Windows.Forms.Label();
      this.cbViewModeSwitchEnabled = new System.Windows.Forms.CheckBox();
      this.cbOverScanEnabled = new System.Windows.Forms.CheckBox();
      this.bOK = new System.Windows.Forms.Button();
      this.bCancel = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // cbEnabled
      // 
      this.cbEnabled.AutoSize = true;
      this.cbEnabled.Location = new System.Drawing.Point(12, 19);
      this.cbEnabled.Name = "cbEnabled";
      this.cbEnabled.Size = new System.Drawing.Size(65, 17);
      this.cbEnabled.TabIndex = 0;
      this.cbEnabled.Text = "Enabled";
      this.cbEnabled.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.cb_EnableLBDetection);
      this.groupBox1.Controls.Add(this.label9);
      this.groupBox1.Controls.Add(this.txbOverScan);
      this.groupBox1.Controls.Add(this.cbEnabled);
      this.groupBox1.Controls.Add(this.cmbViewMode);
      this.groupBox1.Controls.Add(this.txbARTo);
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.txbARFrom);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.txbMaxHeight);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.txbMinHeight);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.txbMaxWidth);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.txbMinWidth);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.txbName);
      this.groupBox1.Controls.Add(this.lblRulename);
      this.groupBox1.Controls.Add(this.cbViewModeSwitchEnabled);
      this.groupBox1.Controls.Add(this.cbOverScanEnabled);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(390, 260);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      // 
      // cb_EnableLBDetection
      // 
      this.cb_EnableLBDetection.AutoSize = true;
      this.cb_EnableLBDetection.Location = new System.Drawing.Point(12, 203);
      this.cb_EnableLBDetection.Name = "cb_EnableLBDetection";
      this.cb_EnableLBDetection.Size = new System.Drawing.Size(214, 17);
      this.cb_EnableLBDetection.TabIndex = 11;
      this.cb_EnableLBDetection.Text = "This rule enables the letterbox detection";
      this.cb_EnableLBDetection.UseVisualStyleBackColor = true;
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(269, 227);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(28, 13);
      this.label9.TabIndex = 23;
      this.label9.Text = "pixel";
      // 
      // txbOverScan
      // 
      this.txbOverScan.Enabled = false;
      this.txbOverScan.Location = new System.Drawing.Point(200, 224);
      this.txbOverScan.Name = "txbOverScan";
      this.txbOverScan.Size = new System.Drawing.Size(63, 20);
      this.txbOverScan.TabIndex = 10;
      // 
      // cmbViewMode
      // 
      this.cmbViewMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbViewMode.Enabled = false;
      this.cmbViewMode.FormattingEnabled = true;
      this.cmbViewMode.Location = new System.Drawing.Point(253, 178);
      this.cmbViewMode.Name = "cmbViewMode";
      this.cmbViewMode.Size = new System.Drawing.Size(121, 21);
      this.cmbViewMode.TabIndex = 8;
      // 
      // txbARTo
      // 
      this.txbARTo.Location = new System.Drawing.Point(268, 61);
      this.txbARTo.Name = "txbARTo";
      this.txbARTo.Size = new System.Drawing.Size(92, 20);
      this.txbARTo.TabIndex = 2;
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(246, 64);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(16, 13);
      this.label8.TabIndex = 16;
      this.label8.Text = "to";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(133, 64);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(27, 13);
      this.label7.TabIndex = 15;
      this.label7.Text = "from";
      // 
      // txbARFrom
      // 
      this.txbARFrom.Location = new System.Drawing.Point(166, 61);
      this.txbARFrom.Name = "txbARFrom";
      this.txbARFrom.Size = new System.Drawing.Size(74, 20);
      this.txbARFrom.TabIndex = 1;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(133, 45);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(63, 13);
      this.label6.TabIndex = 13;
      this.label6.Text = "Aspect ratio";
      // 
      // txbMaxHeight
      // 
      this.txbMaxHeight.Location = new System.Drawing.Point(243, 142);
      this.txbMaxHeight.Name = "txbMaxHeight";
      this.txbMaxHeight.Size = new System.Drawing.Size(58, 20);
      this.txbMaxHeight.TabIndex = 6;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(157, 145);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(83, 13);
      this.label4.TabIndex = 11;
      this.label4.Text = "Maximum height";
      // 
      // txbMinHeight
      // 
      this.txbMinHeight.Location = new System.Drawing.Point(243, 116);
      this.txbMinHeight.Name = "txbMinHeight";
      this.txbMinHeight.Size = new System.Drawing.Size(58, 20);
      this.txbMinHeight.TabIndex = 5;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(157, 119);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(80, 13);
      this.label5.TabIndex = 9;
      this.label5.Text = "Minimum height";
      // 
      // txbMaxWidth
      // 
      this.txbMaxWidth.Location = new System.Drawing.Point(88, 142);
      this.txbMaxWidth.Name = "txbMaxWidth";
      this.txbMaxWidth.Size = new System.Drawing.Size(58, 20);
      this.txbMaxWidth.TabIndex = 4;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(6, 145);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(79, 13);
      this.label3.TabIndex = 7;
      this.label3.Text = "Maximum width";
      // 
      // txbMinWidth
      // 
      this.txbMinWidth.Location = new System.Drawing.Point(88, 116);
      this.txbMinWidth.Name = "txbMinWidth";
      this.txbMinWidth.Size = new System.Drawing.Size(58, 20);
      this.txbMinWidth.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 119);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(76, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Minimum width";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 93);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(276, 13);
      this.label1.TabIndex = 4;
      this.label1.Text = "This rule only applies if the following requirements are met";
      // 
      // txbName
      // 
      this.txbName.Location = new System.Drawing.Point(12, 61);
      this.txbName.MaxLength = 20;
      this.txbName.Name = "txbName";
      this.txbName.Size = new System.Drawing.Size(100, 20);
      this.txbName.TabIndex = 0;
      // 
      // lblRulename
      // 
      this.lblRulename.AutoSize = true;
      this.lblRulename.Location = new System.Drawing.Point(6, 45);
      this.lblRulename.Name = "lblRulename";
      this.lblRulename.Size = new System.Drawing.Size(58, 13);
      this.lblRulename.TabIndex = 3;
      this.lblRulename.Text = "Rule name";
      // 
      // cbViewModeSwitchEnabled
      // 
      this.cbViewModeSwitchEnabled.AutoSize = true;
      this.cbViewModeSwitchEnabled.Location = new System.Drawing.Point(12, 180);
      this.cbViewModeSwitchEnabled.Name = "cbViewModeSwitchEnabled";
      this.cbViewModeSwitchEnabled.Size = new System.Drawing.Size(235, 17);
      this.cbViewModeSwitchEnabled.TabIndex = 7;
      //this.cbViewModeSwitchEnabled.Text = "This rule switches to the following viewmode";
      this.cbViewModeSwitchEnabled.Text = "Enable Auto LB            Switch to viewmode";
      this.cbViewModeSwitchEnabled.UseVisualStyleBackColor = true;
      this.cbViewModeSwitchEnabled.CheckedChanged += new System.EventHandler(this.cbViewModeSwitchEnabled_CheckedChanged);
      // 
      // cbOverScanEnabled
      // 
      this.cbOverScanEnabled.AutoSize = true;
      this.cbOverScanEnabled.Location = new System.Drawing.Point(12, 226);
      this.cbOverScanEnabled.Name = "cbOverScanEnabled";
      this.cbOverScanEnabled.Size = new System.Drawing.Size(182, 17);
      this.cbOverScanEnabled.TabIndex = 9;
      this.cbOverScanEnabled.Text = "This rule uses following overscan";
      this.cbOverScanEnabled.UseVisualStyleBackColor = true;
      this.cbOverScanEnabled.CheckedChanged += new System.EventHandler(this.cbOverScanEnabled_CheckedChanged);
      // 
      // bOK
      // 
      this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.bOK.Location = new System.Drawing.Point(370, 278);
      this.bOK.Name = "bOK";
      this.bOK.Size = new System.Drawing.Size(36, 23);
      this.bOK.TabIndex = 1;
      this.bOK.Text = "&OK";
      this.bOK.UseVisualStyleBackColor = true;
      this.bOK.Click += new System.EventHandler(this.bOK_Click);
      // 
      // bCancel
      // 
      this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.bCancel.Location = new System.Drawing.Point(316, 278);
      this.bCancel.Name = "bCancel";
      this.bCancel.Size = new System.Drawing.Size(48, 23);
      this.bCancel.TabIndex = 0;
      this.bCancel.Text = "&Cancel";
      this.bCancel.UseVisualStyleBackColor = true;
      this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
      // 
      // ViewModeSwitcherRuleDetail
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(418, 307);
      this.Controls.Add(this.bCancel);
      this.Controls.Add(this.bOK);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "ViewModeSwitcherRuleDetail";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "ViewModeSwitcherRuleDetail";
      this.Load += new System.EventHandler(this.ViewModeSwitcherRuleDetail_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.CheckBox cbEnabled;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lblRulename;
    private System.Windows.Forms.TextBox txbName;
    private System.Windows.Forms.TextBox txbMaxHeight;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox txbMinHeight;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox txbMaxWidth;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox txbMinWidth;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txbARTo;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox txbARFrom;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Button bOK;
    private System.Windows.Forms.Button bCancel;
    private System.Windows.Forms.ComboBox cmbViewMode;
    private System.Windows.Forms.CheckBox cbOverScanEnabled;
    private System.Windows.Forms.CheckBox cbViewModeSwitchEnabled;
    private System.Windows.Forms.TextBox txbOverScan;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.CheckBox cb_EnableLBDetection;
  }
}