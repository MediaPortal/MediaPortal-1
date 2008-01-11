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

namespace MediaPortal.ControlDevices
{
  partial class LearnControlPanel
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButton5 = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpCheckBox1 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpCodeList = new MediaPortal.UserInterface.Controls.MPListView();
      this.mpComboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpButton2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpCheckBox3 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBox2 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpTextBox1 = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpCheckBox2 = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpButton4 = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpButton3 = new MediaPortal.UserInterface.Controls.MPButton();
      this.mpGroupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.mpGroupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.mpButton5);
      this.mpGroupBox1.Controls.Add(this.mpCheckBox1);
      this.mpGroupBox1.Controls.Add(this.mpButton1);
      this.mpGroupBox1.Controls.Add(this.mpCodeList);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(3, 3);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(286, 339);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Codes";
      this.mpGroupBox1.Enter += new System.EventHandler(this.mpGroupBox1_Enter);
      // 
      // mpButton5
      // 
      this.mpButton5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButton5.Location = new System.Drawing.Point(87, 310);
      this.mpButton5.Name = "mpButton5";
      this.mpButton5.Size = new System.Drawing.Size(75, 23);
      this.mpButton5.TabIndex = 3;
      this.mpButton5.Text = "Remove";
      this.mpButton5.UseVisualStyleBackColor = true;
      // 
      // mpCheckBox1
      // 
      this.mpCheckBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mpCheckBox1.AutoSize = true;
      this.mpCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBox1.Location = new System.Drawing.Point(179, 316);
      this.mpCheckBox1.Name = "mpCheckBox1";
      this.mpCheckBox1.Size = new System.Drawing.Size(101, 17);
      this.mpCheckBox1.TabIndex = 2;
      this.mpCheckBox1.Text = "Hide unselected";
      this.mpCheckBox1.UseVisualStyleBackColor = true;
      this.mpCheckBox1.CheckedChanged += new System.EventHandler(this.mpCheckBox1_CheckedChanged);
      // 
      // mpButton1
      // 
      this.mpButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mpButton1.Location = new System.Drawing.Point(6, 310);
      this.mpButton1.Name = "mpButton1";
      this.mpButton1.Size = new System.Drawing.Size(75, 23);
      this.mpButton1.TabIndex = 1;
      this.mpButton1.Text = "Add";
      this.mpButton1.UseVisualStyleBackColor = true;
      // 
      // mpCodeList
      // 
      this.mpCodeList.AllowDrop = true;
      this.mpCodeList.AllowRowReorder = true;
      this.mpCodeList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpCodeList.Location = new System.Drawing.Point(6, 19);
      this.mpCodeList.Name = "mpCodeList";
      this.mpCodeList.Size = new System.Drawing.Size(274, 284);
      this.mpCodeList.TabIndex = 0;
      this.mpCodeList.UseCompatibleStateImageBehavior = false;
      // 
      // mpComboBox1
      // 
      this.mpComboBox1.BorderColor = System.Drawing.Color.Empty;
      this.mpComboBox1.FormattingEnabled = true;
      this.mpComboBox1.Location = new System.Drawing.Point(10, 15);
      this.mpComboBox1.Name = "mpComboBox1";
      this.mpComboBox1.Size = new System.Drawing.Size(150, 21);
      this.mpComboBox1.TabIndex = 3;
      // 
      // mpButton2
      // 
      this.mpButton2.Location = new System.Drawing.Point(166, 14);
      this.mpButton2.Name = "mpButton2";
      this.mpButton2.Size = new System.Drawing.Size(51, 23);
      this.mpButton2.TabIndex = 4;
      this.mpButton2.Text = "Edit";
      this.mpButton2.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.mpCheckBox3);
      this.mpGroupBox2.Controls.Add(this.mpLabel2);
      this.mpGroupBox2.Controls.Add(this.mpLabel1);
      this.mpGroupBox2.Controls.Add(this.mpTextBox2);
      this.mpGroupBox2.Controls.Add(this.mpTextBox1);
      this.mpGroupBox2.Controls.Add(this.mpCheckBox2);
      this.mpGroupBox2.Controls.Add(this.mpComboBox1);
      this.mpGroupBox2.Controls.Add(this.mpButton2);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox2.Location = new System.Drawing.Point(298, 3);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(228, 127);
      this.mpGroupBox2.TabIndex = 1;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Code sets";
      // 
      // mpCheckBox3
      // 
      this.mpCheckBox3.AutoSize = true;
      this.mpCheckBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBox3.Location = new System.Drawing.Point(105, 96);
      this.mpCheckBox3.Name = "mpCheckBox3";
      this.mpCheckBox3.Size = new System.Drawing.Size(73, 17);
      this.mpCheckBox3.TabIndex = 10;
      this.mpCheckBox3.Text = "Output set";
      this.mpCheckBox3.UseVisualStyleBackColor = true;
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(10, 70);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(35, 13);
      this.mpLabel2.TabIndex = 9;
      this.mpLabel2.Text = "Name";
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(10, 46);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(18, 13);
      this.mpLabel1.TabIndex = 8;
      this.mpLabel1.Text = "ID";
      // 
      // mpTextBox2
      // 
      this.mpTextBox2.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBox2.Location = new System.Drawing.Point(53, 70);
      this.mpTextBox2.Name = "mpTextBox2";
      this.mpTextBox2.Size = new System.Drawing.Size(164, 20);
      this.mpTextBox2.TabIndex = 7;
      // 
      // mpTextBox1
      // 
      this.mpTextBox1.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBox1.Location = new System.Drawing.Point(53, 43);
      this.mpTextBox1.Name = "mpTextBox1";
      this.mpTextBox1.Size = new System.Drawing.Size(164, 20);
      this.mpTextBox1.TabIndex = 6;
      // 
      // mpCheckBox2
      // 
      this.mpCheckBox2.AutoSize = true;
      this.mpCheckBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpCheckBox2.Location = new System.Drawing.Point(13, 96);
      this.mpCheckBox2.Name = "mpCheckBox2";
      this.mpCheckBox2.Size = new System.Drawing.Size(65, 17);
      this.mpCheckBox2.TabIndex = 5;
      this.mpCheckBox2.Text = "Input set";
      this.mpCheckBox2.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox3
      // 
      this.mpGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox3.Controls.Add(this.mpButton4);
      this.mpGroupBox3.Controls.Add(this.mpButton3);
      this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox3.Location = new System.Drawing.Point(298, 136);
      this.mpGroupBox3.Name = "mpGroupBox3";
      this.mpGroupBox3.Size = new System.Drawing.Size(228, 81);
      this.mpGroupBox3.TabIndex = 2;
      this.mpGroupBox3.TabStop = false;
      this.mpGroupBox3.Text = "Commands";
      // 
      // mpButton4
      // 
      this.mpButton4.Location = new System.Drawing.Point(7, 50);
      this.mpButton4.Name = "mpButton4";
      this.mpButton4.Size = new System.Drawing.Size(210, 23);
      this.mpButton4.TabIndex = 1;
      this.mpButton4.Text = "Clear Codes";
      this.mpButton4.UseVisualStyleBackColor = true;
      // 
      // mpButton3
      // 
      this.mpButton3.Location = new System.Drawing.Point(7, 20);
      this.mpButton3.Name = "mpButton3";
      this.mpButton3.Size = new System.Drawing.Size(210, 23);
      this.mpButton3.TabIndex = 0;
      this.mpButton3.Text = "Learn Codes";
      this.mpButton3.UseVisualStyleBackColor = true;
      // 
      // mpGroupBox4
      // 
      this.mpGroupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox4.Location = new System.Drawing.Point(298, 223);
      this.mpGroupBox4.Name = "mpGroupBox4";
      this.mpGroupBox4.Size = new System.Drawing.Size(228, 119);
      this.mpGroupBox4.TabIndex = 3;
      this.mpGroupBox4.TabStop = false;
      this.mpGroupBox4.Text = "Code";
      // 
      // LearnControlPanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mpGroupBox4);
      this.Controls.Add(this.mpGroupBox3);
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "LearnControlPanel";
      this.Size = new System.Drawing.Size(532, 358);
      this.Load += new System.EventHandler(this.USBUIRTLearnControl_Load);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.mpGroupBox3.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBox1;
    private MediaPortal.UserInterface.Controls.MPButton mpButton1;
    private MediaPortal.UserInterface.Controls.MPListView mpCodeList;
    private MediaPortal.UserInterface.Controls.MPButton mpButton2;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
    private MediaPortal.UserInterface.Controls.MPButton mpButton4;
    private MediaPortal.UserInterface.Controls.MPButton mpButton3;
    private MediaPortal.UserInterface.Controls.MPButton mpButton5;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel2;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBox2;
    private MediaPortal.UserInterface.Controls.MPTextBox mpTextBox1;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBox2;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox4;
    private MediaPortal.UserInterface.Controls.MPCheckBox mpCheckBox3;

  }
}
