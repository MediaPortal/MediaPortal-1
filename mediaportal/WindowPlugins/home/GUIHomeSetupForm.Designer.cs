#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

namespace MediaPortal.GUI.Home
{
  partial class GUIHomeSetupForm
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
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabSettings = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.tboxTest = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.btnYearNumber = new System.Windows.Forms.Button();
      this.btnYearText = new System.Windows.Forms.Button();
      this.btnMonthNumber = new System.Windows.Forms.Button();
      this.btnMonthText = new System.Windows.Forms.Button();
      this.btnDayNumber = new System.Windows.Forms.Button();
      this.btnDayText = new System.Windows.Forms.Button();
      this.cboxFormat = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.chkBoxAnimation = new System.Windows.Forms.CheckBox();
      this.chkBoxUseMyPlugins = new System.Windows.Forms.CheckBox();
      this.chkboxFixScrollbar = new System.Windows.Forms.CheckBox();
      this.tabMenuSort = new System.Windows.Forms.TabPage();
      this.tvMenu = new System.Windows.Forms.TreeView();
      this.buDown = new System.Windows.Forms.Button();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.laName = new System.Windows.Forms.Label();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.buUp = new System.Windows.Forms.Button();
      this.checkBoxShowSeconds = new System.Windows.Forms.CheckBox();
      this.tabControl1.SuspendLayout();
      this.tabSettings.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabMenuSort.SuspendLayout();
      this.groupBox4.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnOK.Location = new System.Drawing.Point(84, 433);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 2;
      this.btnOK.Text = "&OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(280, 433);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 3;
      this.btnCancel.Text = "&Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabSettings);
      this.tabControl1.Controls.Add(this.tabMenuSort);
      this.tabControl1.Location = new System.Drawing.Point(12, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(424, 413);
      this.tabControl1.TabIndex = 4;
      // 
      // tabSettings
      // 
      this.tabSettings.Controls.Add(this.groupBox2);
      this.tabSettings.Controls.Add(this.groupBox1);
      this.tabSettings.Location = new System.Drawing.Point(4, 22);
      this.tabSettings.Name = "tabSettings";
      this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabSettings.Size = new System.Drawing.Size(416, 387);
      this.tabSettings.TabIndex = 0;
      this.tabSettings.Text = "Settings";
      this.tabSettings.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.checkBoxShowSeconds);
      this.groupBox2.Controls.Add(this.tboxTest);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.groupBox3);
      this.groupBox2.Controls.Add(this.cboxFormat);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Location = new System.Drawing.Point(15, 111);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(386, 264);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Date + Time Settings";
      // 
      // tboxTest
      // 
      this.tboxTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tboxTest.BackColor = System.Drawing.Color.Cornsilk;
      this.tboxTest.Enabled = false;
      this.tboxTest.Location = new System.Drawing.Point(64, 66);
      this.tboxTest.Name = "tboxTest";
      this.tboxTest.ReadOnly = true;
      this.tboxTest.Size = new System.Drawing.Size(300, 20);
      this.tboxTest.TabIndex = 4;
      this.tboxTest.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 69);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(28, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Test";
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.btnYearNumber);
      this.groupBox3.Controls.Add(this.btnYearText);
      this.groupBox3.Controls.Add(this.btnMonthNumber);
      this.groupBox3.Controls.Add(this.btnMonthText);
      this.groupBox3.Controls.Add(this.btnDayNumber);
      this.groupBox3.Controls.Add(this.btnDayText);
      this.groupBox3.Location = new System.Drawing.Point(19, 101);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(345, 121);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Parameters  ( click on the text to create your own format )";
      // 
      // btnYearNumber
      // 
      this.btnYearNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnYearNumber.FlatAppearance.BorderSize = 0;
      this.btnYearNumber.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnYearNumber.Location = new System.Drawing.Point(185, 88);
      this.btnYearNumber.Name = "btnYearNumber";
      this.btnYearNumber.Size = new System.Drawing.Size(154, 22);
      this.btnYearNumber.TabIndex = 5;
      this.btnYearNumber.Text = "<YY>  year as short number";
      this.btnYearNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnYearNumber.UseVisualStyleBackColor = true;
      this.btnYearNumber.Click += new System.EventHandler(this.btnYearNumber_Click);
      // 
      // btnYearText
      // 
      this.btnYearText.FlatAppearance.BorderSize = 0;
      this.btnYearText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnYearText.Location = new System.Drawing.Point(11, 87);
      this.btnYearText.Name = "btnYearText";
      this.btnYearText.Size = new System.Drawing.Size(168, 23);
      this.btnYearText.TabIndex = 4;
      this.btnYearText.Text = "<Year> year as long number";
      this.btnYearText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnYearText.UseVisualStyleBackColor = true;
      this.btnYearText.Click += new System.EventHandler(this.btnYearText_Click);
      // 
      // btnMonthNumber
      // 
      this.btnMonthNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnMonthNumber.FlatAppearance.BorderSize = 0;
      this.btnMonthNumber.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnMonthNumber.Location = new System.Drawing.Point(185, 59);
      this.btnMonthNumber.Name = "btnMonthNumber";
      this.btnMonthNumber.Size = new System.Drawing.Size(140, 22);
      this.btnMonthNumber.TabIndex = 3;
      this.btnMonthNumber.Text = "<MM>  month as number";
      this.btnMonthNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnMonthNumber.UseVisualStyleBackColor = true;
      this.btnMonthNumber.Click += new System.EventHandler(this.btnMonthNumber_Click);
      // 
      // btnMonthText
      // 
      this.btnMonthText.FlatAppearance.BorderSize = 0;
      this.btnMonthText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnMonthText.Location = new System.Drawing.Point(11, 58);
      this.btnMonthText.Name = "btnMonthText";
      this.btnMonthText.Size = new System.Drawing.Size(136, 23);
      this.btnMonthText.TabIndex = 2;
      this.btnMonthText.Text = "<Month>  month as text";
      this.btnMonthText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnMonthText.UseVisualStyleBackColor = true;
      this.btnMonthText.Click += new System.EventHandler(this.btnMonthText_Click);
      // 
      // btnDayNumber
      // 
      this.btnDayNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDayNumber.FlatAppearance.BorderSize = 0;
      this.btnDayNumber.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDayNumber.Location = new System.Drawing.Point(185, 30);
      this.btnDayNumber.Name = "btnDayNumber";
      this.btnDayNumber.Size = new System.Drawing.Size(140, 22);
      this.btnDayNumber.TabIndex = 1;
      this.btnDayNumber.Text = "<DD>  day as number";
      this.btnDayNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnDayNumber.UseVisualStyleBackColor = true;
      this.btnDayNumber.Click += new System.EventHandler(this.btnDayNumber_Click);
      // 
      // btnDayText
      // 
      this.btnDayText.FlatAppearance.BorderSize = 0;
      this.btnDayText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDayText.Location = new System.Drawing.Point(11, 29);
      this.btnDayText.Name = "btnDayText";
      this.btnDayText.Size = new System.Drawing.Size(136, 23);
      this.btnDayText.TabIndex = 0;
      this.btnDayText.Text = "<Day>     day as text";
      this.btnDayText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnDayText.UseVisualStyleBackColor = true;
      this.btnDayText.Click += new System.EventHandler(this.btnDayText_Click);
      // 
      // cboxFormat
      // 
      this.cboxFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cboxFormat.FormattingEnabled = true;
      this.cboxFormat.Location = new System.Drawing.Point(64, 25);
      this.cboxFormat.Name = "cboxFormat";
      this.cboxFormat.Size = new System.Drawing.Size(300, 21);
      this.cboxFormat.TabIndex = 1;
      this.cboxFormat.SelectedIndexChanged += new System.EventHandler(this.cboxFormat_SelectedIndexChanged);
      this.cboxFormat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboxFormat_KeyPress);
      this.cboxFormat.TextUpdate += new System.EventHandler(this.cboxFormat_TextUpdate);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 28);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(39, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Format";
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.chkBoxAnimation);
      this.groupBox1.Controls.Add(this.chkBoxUseMyPlugins);
      this.groupBox1.Controls.Add(this.chkboxFixScrollbar);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(389, 87);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Home Settings";
      // 
      // chkBoxAnimation
      // 
      this.chkBoxAnimation.AutoSize = true;
      this.chkBoxAnimation.Location = new System.Drawing.Point(207, 31);
      this.chkBoxAnimation.Name = "chkBoxAnimation";
      this.chkBoxAnimation.Size = new System.Drawing.Size(113, 17);
      this.chkBoxAnimation.TabIndex = 2;
      this.chkBoxAnimation.Text = "Enable Animations";
      this.chkBoxAnimation.UseVisualStyleBackColor = true;
      // 
      // chkBoxUseMyPlugins
      // 
      this.chkBoxUseMyPlugins.AutoSize = true;
      this.chkBoxUseMyPlugins.Location = new System.Drawing.Point(22, 54);
      this.chkBoxUseMyPlugins.Name = "chkBoxUseMyPlugins";
      this.chkBoxUseMyPlugins.Size = new System.Drawing.Size(108, 17);
      this.chkBoxUseMyPlugins.TabIndex = 1;
      this.chkBoxUseMyPlugins.Text = "Show My PlugIns";
      this.chkBoxUseMyPlugins.UseVisualStyleBackColor = true;
      // 
      // chkboxFixScrollbar
      // 
      this.chkboxFixScrollbar.AutoSize = true;
      this.chkboxFixScrollbar.Location = new System.Drawing.Point(22, 31);
      this.chkboxFixScrollbar.Name = "chkboxFixScrollbar";
      this.chkboxFixScrollbar.Size = new System.Drawing.Size(87, 17);
      this.chkboxFixScrollbar.TabIndex = 0;
      this.chkboxFixScrollbar.Text = "Fix Scroll Bar";
      this.chkboxFixScrollbar.UseVisualStyleBackColor = true;
      // 
      // tabMenuSort
      // 
      this.tabMenuSort.Controls.Add(this.tvMenu);
      this.tabMenuSort.Controls.Add(this.buDown);
      this.tabMenuSort.Controls.Add(this.groupBox4);
      this.tabMenuSort.Controls.Add(this.buUp);
      this.tabMenuSort.Location = new System.Drawing.Point(4, 22);
      this.tabMenuSort.Name = "tabMenuSort";
      this.tabMenuSort.Padding = new System.Windows.Forms.Padding(3);
      this.tabMenuSort.Size = new System.Drawing.Size(416, 387);
      this.tabMenuSort.TabIndex = 1;
      this.tabMenuSort.Text = "Menu Setup";
      this.tabMenuSort.UseVisualStyleBackColor = true;
      // 
      // tvMenu
      // 
      this.tvMenu.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tvMenu.FullRowSelect = true;
      this.tvMenu.HideSelection = false;
      this.tvMenu.Location = new System.Drawing.Point(49, 6);
      this.tvMenu.Name = "tvMenu";
      this.tvMenu.Size = new System.Drawing.Size(235, 254);
      this.tvMenu.TabIndex = 7;
      this.tvMenu.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvMenu_AfterSelect);
      // 
      // buDown
      // 
      this.buDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buDown.Location = new System.Drawing.Point(327, 84);
      this.buDown.Name = "buDown";
      this.buDown.Size = new System.Drawing.Size(54, 23);
      this.buDown.TabIndex = 4;
      this.buDown.Text = "Down";
      this.buDown.UseVisualStyleBackColor = true;
      this.buDown.Click += new System.EventHandler(this.buDown_Click);
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox4.Controls.Add(this.laName);
      this.groupBox4.Controls.Add(this.pictureBox1);
      this.groupBox4.Location = new System.Drawing.Point(18, 266);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(382, 115);
      this.groupBox4.TabIndex = 2;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Edit Menu Item";
      // 
      // laName
      // 
      this.laName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.laName.AutoSize = true;
      this.laName.Location = new System.Drawing.Point(14, 30);
      this.laName.Name = "laName";
      this.laName.Size = new System.Drawing.Size(35, 13);
      this.laName.TabIndex = 1;
      this.laName.Text = "Name";
      // 
      // pictureBox1
      // 
      this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox1.Location = new System.Drawing.Point(276, 10);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(100, 99);
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // buUp
      // 
      this.buUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buUp.Location = new System.Drawing.Point(327, 39);
      this.buUp.Name = "buUp";
      this.buUp.Size = new System.Drawing.Size(53, 23);
      this.buUp.TabIndex = 3;
      this.buUp.Text = "Up";
      this.buUp.UseVisualStyleBackColor = true;
      this.buUp.Click += new System.EventHandler(this.buUp_Click);
      // 
      // checkBoxShowSeconds
      // 
      this.checkBoxShowSeconds.AutoSize = true;
      this.checkBoxShowSeconds.Location = new System.Drawing.Point(19, 236);
      this.checkBoxShowSeconds.Name = "checkBoxShowSeconds";
      this.checkBoxShowSeconds.Size = new System.Drawing.Size(193, 17);
      this.checkBoxShowSeconds.TabIndex = 3;
      this.checkBoxShowSeconds.Text = "Use long time format (with seconds)";
      this.checkBoxShowSeconds.UseVisualStyleBackColor = true;
      // 
      // GUIHomeSetupForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(448, 472);
      this.Controls.Add(this.tabControl1);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "GUIHomeSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = " Home - Setup";
      this.Load += new System.EventHandler(this.GUIHomeSetupForm_Load);
      this.tabControl1.ResumeLayout(false);
      this.tabSettings.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabMenuSort.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabSettings;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.TextBox tboxTest;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Button btnYearNumber;
    private System.Windows.Forms.Button btnYearText;
    private System.Windows.Forms.Button btnMonthNumber;
    private System.Windows.Forms.Button btnMonthText;
    private System.Windows.Forms.Button btnDayNumber;
    private System.Windows.Forms.Button btnDayText;
    private System.Windows.Forms.ComboBox cboxFormat;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox chkBoxAnimation;
    private System.Windows.Forms.CheckBox chkBoxUseMyPlugins;
    private System.Windows.Forms.CheckBox chkboxFixScrollbar;
    private System.Windows.Forms.TabPage tabMenuSort;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.Button buDown;
    private System.Windows.Forms.Button buUp;
    private System.Windows.Forms.TreeView tvMenu;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Label laName;
    private System.Windows.Forms.CheckBox checkBoxShowSeconds;
  }
}