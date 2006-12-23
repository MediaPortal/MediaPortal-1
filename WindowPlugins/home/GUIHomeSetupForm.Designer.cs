#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.chkBoxUseMyPlugins = new System.Windows.Forms.CheckBox();
      this.chkboxFixScrollbar = new System.Windows.Forms.CheckBox();
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
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.chkBoxAnimation = new System.Windows.Forms.CheckBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.chkBoxAnimation);
      this.groupBox1.Controls.Add(this.chkBoxUseMyPlugins);
      this.groupBox1.Controls.Add(this.chkboxFixScrollbar);
      this.groupBox1.Location = new System.Drawing.Point(15, 20);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(389, 87);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Home Settings";
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
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.tboxTest);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.groupBox3);
      this.groupBox2.Controls.Add(this.cboxFormat);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Location = new System.Drawing.Point(18, 126);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(386, 264);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Date + Time Settings";
      // 
      // tboxTest
      // 
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
      this.groupBox3.Controls.Add(this.btnYearNumber);
      this.groupBox3.Controls.Add(this.btnYearText);
      this.groupBox3.Controls.Add(this.btnMonthNumber);
      this.groupBox3.Controls.Add(this.btnMonthText);
      this.groupBox3.Controls.Add(this.btnDayNumber);
      this.groupBox3.Controls.Add(this.btnDayText);
      this.groupBox3.Location = new System.Drawing.Point(19, 115);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(345, 131);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Parameters  ( click on the text to create your own format )";
      // 
      // btnYearNumber
      // 
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
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(65, 413);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 2;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(261, 413);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 3;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
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
      // GUIHomeSetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(419, 453);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "GUIHomeSetupForm";
      this.Text = " Home Setup";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox chkboxFixScrollbar;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Button btnDayText;
    private System.Windows.Forms.ComboBox cboxFormat;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnMonthNumber;
    private System.Windows.Forms.Button btnMonthText;
    private System.Windows.Forms.Button btnDayNumber;
    private System.Windows.Forms.Button btnYearNumber;
    private System.Windows.Forms.Button btnYearText;
    private System.Windows.Forms.TextBox tboxTest;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox chkBoxUseMyPlugins;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.CheckBox chkBoxAnimation;
  }
}