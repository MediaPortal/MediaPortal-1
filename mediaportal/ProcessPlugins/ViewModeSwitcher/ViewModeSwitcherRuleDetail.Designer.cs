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
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
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
            this.cbAutoCropEnabled = new System.Windows.Forms.CheckBox();
            this.cbMaxCropEnabled = new System.Windows.Forms.CheckBox();
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
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
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
            this.groupBox1.Controls.Add(this.cbAutoCropEnabled);
            this.groupBox1.Controls.Add(this.cbMaxCropEnabled);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(390, 260);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(341, 136);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(33, 13);
            this.label16.TabIndex = 30;
            this.label16.Text = "pixels";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(341, 110);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(33, 13);
            this.label15.TabIndex = 29;
            this.label15.Text = "pixels";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(9, 136);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 13);
            this.label14.TabIndex = 28;
            this.label14.Text = "Video Height";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(9, 110);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(65, 13);
            this.label13.TabIndex = 27;
            this.label13.Text = "Video Width";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(9, 164);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(243, 15);
            this.label12.TabIndex = 26;
            this.label12.Text = "...and it then applies these settings...";
            this.label12.Click += new System.EventHandler(this.label12_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(191, 235);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(53, 13);
            this.label11.TabIndex = 25;
            this.label11.Text = "Overscan";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(179, 190);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(63, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "Zoom mode";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cb_EnableLBDetection
            // 
            this.cb_EnableLBDetection.AutoSize = true;
            this.cb_EnableLBDetection.Location = new System.Drawing.Point(7, 212);
            this.cb_EnableLBDetection.Name = "cb_EnableLBDetection";
            this.cb_EnableLBDetection.Size = new System.Drawing.Size(100, 17);
            this.cb_EnableLBDetection.TabIndex = 11;
            this.cb_EnableLBDetection.Text = "Initial BB detect";
            this.cb_EnableLBDetection.UseVisualStyleBackColor = true;
            this.cb_EnableLBDetection.CheckedChanged += new System.EventHandler(this.cb_EnableLBDetection_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(319, 235);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(33, 13);
            this.label9.TabIndex = 23;
            this.label9.Text = "pixels";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // txbOverScan
            // 
            this.txbOverScan.Location = new System.Drawing.Point(250, 232);
            this.txbOverScan.Name = "txbOverScan";
            this.txbOverScan.Size = new System.Drawing.Size(63, 20);
            this.txbOverScan.TabIndex = 10;
            this.txbOverScan.TextChanged += new System.EventHandler(this.txbOverScan_TextChanged);
            // 
            // cmbViewMode
            // 
            this.cmbViewMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbViewMode.FormattingEnabled = true;
            this.cmbViewMode.Location = new System.Drawing.Point(248, 187);
            this.cmbViewMode.Name = "cmbViewMode";
            this.cmbViewMode.Size = new System.Drawing.Size(121, 21);
            this.cmbViewMode.TabIndex = 8;
            // 
            // txbARTo
            // 
            this.txbARTo.Location = new System.Drawing.Point(277, 81);
            this.txbARTo.Name = "txbARTo";
            this.txbARTo.Size = new System.Drawing.Size(92, 20);
            this.txbARTo.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(255, 84);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(16, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "to";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(142, 84);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(27, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "from";
            // 
            // txbARFrom
            // 
            this.txbARFrom.Location = new System.Drawing.Point(175, 81);
            this.txbARFrom.Name = "txbARFrom";
            this.txbARFrom.Size = new System.Drawing.Size(74, 20);
            this.txbARFrom.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 84);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(122, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Video Aspect Ratio (AR)";
            // 
            // txbMaxHeight
            // 
            this.txbMaxHeight.Location = new System.Drawing.Point(277, 133);
            this.txbMaxHeight.Name = "txbMaxHeight";
            this.txbMaxHeight.Size = new System.Drawing.Size(58, 20);
            this.txbMaxHeight.TabIndex = 6;
            this.txbMaxHeight.TextChanged += new System.EventHandler(this.txbMaxHeight_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(255, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "to";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // txbMinHeight
            // 
            this.txbMinHeight.Location = new System.Drawing.Point(175, 133);
            this.txbMinHeight.Name = "txbMinHeight";
            this.txbMinHeight.Size = new System.Drawing.Size(58, 20);
            this.txbMinHeight.TabIndex = 5;
            this.txbMinHeight.TextChanged += new System.EventHandler(this.txbMinHeight_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(255, 110);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(16, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "to";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // txbMaxWidth
            // 
            this.txbMaxWidth.Location = new System.Drawing.Point(277, 107);
            this.txbMaxWidth.Name = "txbMaxWidth";
            this.txbMaxWidth.Size = new System.Drawing.Size(58, 20);
            this.txbMaxWidth.TabIndex = 4;
            this.txbMaxWidth.TextChanged += new System.EventHandler(this.txbMaxWidth_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(142, 136);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "from";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // txbMinWidth
            // 
            this.txbMinWidth.Location = new System.Drawing.Point(175, 107);
            this.txbMinWidth.Name = "txbMinWidth";
            this.txbMinWidth.Size = new System.Drawing.Size(58, 20);
            this.txbMinWidth.TabIndex = 3;
            this.txbMinWidth.TextChanged += new System.EventHandler(this.txbMinWidth_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(142, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "from";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(346, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "This rule is used if all the conditions below are met...";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // txbName
            // 
            this.txbName.Location = new System.Drawing.Point(175, 17);
            this.txbName.MaxLength = 20;
            this.txbName.Name = "txbName";
            this.txbName.Size = new System.Drawing.Size(100, 20);
            this.txbName.TabIndex = 0;
            // 
            // lblRulename
            // 
            this.lblRulename.AutoSize = true;
            this.lblRulename.Location = new System.Drawing.Point(111, 20);
            this.lblRulename.Name = "lblRulename";
            this.lblRulename.Size = new System.Drawing.Size(58, 13);
            this.lblRulename.TabIndex = 3;
            this.lblRulename.Text = "Rule name";
            // 
            // cbAutoCropEnabled
            // 
            this.cbAutoCropEnabled.AutoSize = true;
            this.cbAutoCropEnabled.Location = new System.Drawing.Point(7, 189);
            this.cbAutoCropEnabled.Name = "cbAutoCropEnabled";
            this.cbAutoCropEnabled.Size = new System.Drawing.Size(132, 17);
            this.cbAutoCropEnabled.TabIndex = 7;
            this.cbAutoCropEnabled.Text = "Continuous BB detect ";
            this.cbAutoCropEnabled.UseVisualStyleBackColor = true;
            this.cbAutoCropEnabled.CheckedChanged += new System.EventHandler(this.cbAutoCropEnabled_CheckedChanged);
            // 
            // cbMaxCropEnabled
            // 
            this.cbMaxCropEnabled.AutoSize = true;
            this.cbMaxCropEnabled.Location = new System.Drawing.Point(7, 235);
            this.cbMaxCropEnabled.Name = "cbMaxCropEnabled";
            this.cbMaxCropEnabled.Size = new System.Drawing.Size(130, 17);
            this.cbMaxCropEnabled.TabIndex = 9;
            this.cbMaxCropEnabled.Text = "Maximise BB cropping";
            this.cbMaxCropEnabled.UseVisualStyleBackColor = true;
            this.cbMaxCropEnabled.CheckedChanged += new System.EventHandler(this.cbMaxCropEnabled_CheckedChanged);
            // 
            // bOK
            // 
            this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOK.Location = new System.Drawing.Point(309, 278);
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
            this.bCancel.Location = new System.Drawing.Point(353, 278);
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
            this.CancelButton = this.bCancel;
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
    private System.Windows.Forms.CheckBox cbMaxCropEnabled;
    private System.Windows.Forms.CheckBox cbAutoCropEnabled;
    private System.Windows.Forms.TextBox txbOverScan;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.CheckBox cb_EnableLBDetection;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.Label label15;
  }
}