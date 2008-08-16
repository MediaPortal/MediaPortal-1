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

namespace MediaPortal.Configuration.Sections
{
  partial class GeneralStartupDelay
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.nudDelay = new System.Windows.Forms.NumericUpDown();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.cbWaitForTvService = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.nudDelay);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.cbWaitForTvService);
      this.groupBox1.Location = new System.Drawing.Point(17, 63);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(302, 95);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // nudDelay
      // 
      this.nudDelay.Location = new System.Drawing.Point(115, 51);
      this.nudDelay.Name = "nudDelay";
      this.nudDelay.Size = new System.Drawing.Size(56, 20);
      this.nudDelay.TabIndex = 7;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(177, 53);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(53, 13);
      this.label2.TabIndex = 6;
      this.label2.Text = "second(s)";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(27, 53);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(87, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "Delay startup for ";
      // 
      // cbWaitForTvService
      // 
      this.cbWaitForTvService.AutoSize = true;
      this.cbWaitForTvService.Enabled = false;
      this.cbWaitForTvService.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbWaitForTvService.Location = new System.Drawing.Point(30, 25);
      this.cbWaitForTvService.Name = "cbWaitForTvService";
      this.cbWaitForTvService.Size = new System.Drawing.Size(242, 17);
      this.cbWaitForTvService.TabIndex = 4;
      this.cbWaitForTvService.Text = "Wait until TvServer has started (autodetected)";
      this.cbWaitForTvService.UseVisualStyleBackColor = true;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(14, 14);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(322, 13);
      this.label5.TabIndex = 5;
      this.label5.Text = "Here you can set some conditions to delay the start of MediaPortal.";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(14, 36);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(299, 13);
      this.label6.TabIndex = 6;
      this.label6.Text = "This can be usefull if you start MP with Windows automatically";
      // 
      // GeneralStartupDelay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.groupBox1);
      this.Name = "GeneralStartupDelay";
      this.Size = new System.Drawing.Size(347, 297);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.NumericUpDown nudDelay;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbWaitForTvService;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;

  }
}
