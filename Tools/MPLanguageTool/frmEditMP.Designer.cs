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
namespace MPLanguageTool
{
  partial class frmEditMP
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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.lID = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.splitContainer4 = new System.Windows.Forms.SplitContainer();
      this.edValue = new System.Windows.Forms.TextBox();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.splitContainer3 = new System.Windows.Forms.SplitContainer();
      this.splitContainer5 = new System.Windows.Forms.SplitContainer();
      this.edDefault = new System.Windows.Forms.TextBox();
      this.textBox2 = new System.Windows.Forms.TextBox();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.splitContainer4.Panel1.SuspendLayout();
      this.splitContainer4.Panel2.SuspendLayout();
      this.splitContainer4.SuspendLayout();
      this.splitContainer3.Panel1.SuspendLayout();
      this.splitContainer3.Panel2.SuspendLayout();
      this.splitContainer3.SuspendLayout();
      this.splitContainer5.Panel1.SuspendLayout();
      this.splitContainer5.Panel2.SuspendLayout();
      this.splitContainer5.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
      this.splitContainer1.Size = new System.Drawing.Size(543, 391);
      this.splitContainer1.SplitterDistance = 220;
      this.splitContainer1.TabIndex = 0;
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.lID);
      this.splitContainer2.Panel1.Controls.Add(this.label1);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.splitContainer4);
      this.splitContainer2.Size = new System.Drawing.Size(543, 220);
      this.splitContainer2.SplitterDistance = 31;
      this.splitContainer2.TabIndex = 0;
      // 
      // lID
      // 
      this.lID.AutoSize = true;
      this.lID.Location = new System.Drawing.Point(43, 9);
      this.lID.Name = "lID";
      this.lID.Size = new System.Drawing.Size(35, 13);
      this.lID.TabIndex = 1;
      this.lID.Text = "label2";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(24, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "ID:";
      // 
      // splitContainer4
      // 
      this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer4.Location = new System.Drawing.Point(0, 0);
      this.splitContainer4.Name = "splitContainer4";
      // 
      // splitContainer4.Panel1
      // 
      this.splitContainer4.Panel1.Controls.Add(this.edValue);
      // 
      // splitContainer4.Panel2
      // 
      this.splitContainer4.Panel2.Controls.Add(this.textBox1);
      this.splitContainer4.Size = new System.Drawing.Size(543, 185);
      this.splitContainer4.SplitterDistance = 292;
      this.splitContainer4.TabIndex = 1;
      // 
      // edValue
      // 
      this.edValue.Dock = System.Windows.Forms.DockStyle.Fill;
      this.edValue.Location = new System.Drawing.Point(0, 0);
      this.edValue.Multiline = true;
      this.edValue.Name = "edValue";
      this.edValue.Size = new System.Drawing.Size(292, 185);
      this.edValue.TabIndex = 0;
      this.edValue.WordWrap = false;
      // 
      // textBox1
      // 
      this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox1.Location = new System.Drawing.Point(0, 0);
      this.textBox1.Multiline = true;
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(247, 185);
      this.textBox1.TabIndex = 0;
      // 
      // splitContainer3
      // 
      this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer3.Location = new System.Drawing.Point(0, 0);
      this.splitContainer3.Name = "splitContainer3";
      this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer3.Panel1
      // 
      this.splitContainer3.Panel1.Controls.Add(this.splitContainer5);
      // 
      // splitContainer3.Panel2
      // 
      this.splitContainer3.Panel2.Controls.Add(this.btnCancel);
      this.splitContainer3.Panel2.Controls.Add(this.btnOK);
      this.splitContainer3.Size = new System.Drawing.Size(543, 167);
      this.splitContainer3.SplitterDistance = 119;
      this.splitContainer3.TabIndex = 0;
      // 
      // splitContainer5
      // 
      this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer5.Location = new System.Drawing.Point(0, 0);
      this.splitContainer5.Name = "splitContainer5";
      // 
      // splitContainer5.Panel1
      // 
      this.splitContainer5.Panel1.Controls.Add(this.edDefault);
      // 
      // splitContainer5.Panel2
      // 
      this.splitContainer5.Panel2.Controls.Add(this.textBox2);
      this.splitContainer5.Size = new System.Drawing.Size(543, 119);
      this.splitContainer5.SplitterDistance = 292;
      this.splitContainer5.TabIndex = 2;
      // 
      // edDefault
      // 
      this.edDefault.BackColor = System.Drawing.Color.WhiteSmoke;
      this.edDefault.Dock = System.Windows.Forms.DockStyle.Fill;
      this.edDefault.Location = new System.Drawing.Point(0, 0);
      this.edDefault.Multiline = true;
      this.edDefault.Name = "edDefault";
      this.edDefault.ReadOnly = true;
      this.edDefault.Size = new System.Drawing.Size(292, 119);
      this.edDefault.TabIndex = 1;
      this.edDefault.WordWrap = false;
      // 
      // textBox2
      // 
      this.textBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox2.Location = new System.Drawing.Point(0, 0);
      this.textBox2.Multiline = true;
      this.textBox2.Name = "textBox2";
      this.textBox2.ReadOnly = true;
      this.textBox2.Size = new System.Drawing.Size(247, 119);
      this.textBox2.TabIndex = 0;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(429, 9);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(30, 9);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 0;
      this.btnOK.Text = "Ok";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // frmEditMP
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(543, 391);
      this.Controls.Add(this.splitContainer1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frmEditMP";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Edit translation";
      this.Shown += new System.EventHandler(this.frmEdit_Shown);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel1.PerformLayout();
      this.splitContainer2.Panel2.ResumeLayout(false);
      this.splitContainer2.ResumeLayout(false);
      this.splitContainer4.Panel1.ResumeLayout(false);
      this.splitContainer4.Panel1.PerformLayout();
      this.splitContainer4.Panel2.ResumeLayout(false);
      this.splitContainer4.Panel2.PerformLayout();
      this.splitContainer4.ResumeLayout(false);
      this.splitContainer3.Panel1.ResumeLayout(false);
      this.splitContainer3.Panel2.ResumeLayout(false);
      this.splitContainer3.ResumeLayout(false);
      this.splitContainer5.Panel1.ResumeLayout(false);
      this.splitContainer5.Panel1.PerformLayout();
      this.splitContainer5.Panel2.ResumeLayout(false);
      this.splitContainer5.Panel2.PerformLayout();
      this.splitContainer5.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.Label lID;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox edValue;
    private System.Windows.Forms.SplitContainer splitContainer3;
    private System.Windows.Forms.TextBox edDefault;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.SplitContainer splitContainer4;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.SplitContainer splitContainer5;
    private System.Windows.Forms.TextBox textBox2;
  }
}