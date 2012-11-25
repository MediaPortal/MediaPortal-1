#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

namespace MediaPortal.ProcessPlugins.LastFMScrobbler
{
  partial class LastFMConfig
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
      this.btnAuthenticate = new System.Windows.Forms.Button();
      this.btnSecond = new System.Windows.Forms.Button();
      this.chkAutoDJ = new System.Windows.Forms.CheckBox();
      this.numRandomness = new System.Windows.Forms.NumericUpDown();
      ((System.ComponentModel.ISupportInitialize)(this.numRandomness)).BeginInit();
      this.SuspendLayout();
      // 
      // btnAuthenticate
      // 
      this.btnAuthenticate.Location = new System.Drawing.Point(93, 42);
      this.btnAuthenticate.Name = "btnAuthenticate";
      this.btnAuthenticate.Size = new System.Drawing.Size(99, 23);
      this.btnAuthenticate.TabIndex = 0;
      this.btnAuthenticate.Text = "Authenticate";
      this.btnAuthenticate.UseVisualStyleBackColor = true;
      this.btnAuthenticate.Click += new System.EventHandler(this.btnAuthenticate_Click);
      // 
      // btnSecond
      // 
      this.btnSecond.Enabled = false;
      this.btnSecond.Location = new System.Drawing.Point(93, 101);
      this.btnSecond.Name = "btnSecond";
      this.btnSecond.Size = new System.Drawing.Size(99, 23);
      this.btnSecond.TabIndex = 1;
      this.btnSecond.Text = "Second Step";
      this.btnSecond.UseVisualStyleBackColor = true;
      this.btnSecond.Click += new System.EventHandler(this.btnSecond_Click);
      // 
      // chkAutoDJ
      // 
      this.chkAutoDJ.AutoSize = true;
      this.chkAutoDJ.Checked = true;
      this.chkAutoDJ.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkAutoDJ.Location = new System.Drawing.Point(50, 163);
      this.chkAutoDJ.Name = "chkAutoDJ";
      this.chkAutoDJ.Size = new System.Drawing.Size(94, 17);
      this.chkAutoDJ.TabIndex = 2;
      this.chkAutoDJ.Text = "Auto DJ Mode";
      this.chkAutoDJ.UseVisualStyleBackColor = true;
      this.chkAutoDJ.CheckedChanged += new System.EventHandler(this.chkAutoDJ_CheckedChanged);
      // 
      // numRandomness
      // 
      this.numRandomness.Location = new System.Drawing.Point(50, 187);
      this.numRandomness.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
      this.numRandomness.Name = "numRandomness";
      this.numRandomness.Size = new System.Drawing.Size(120, 20);
      this.numRandomness.TabIndex = 3;
      this.numRandomness.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.numRandomness.ValueChanged += new System.EventHandler(this.numRandomness_ValueChanged);
      // 
      // LastFMConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.numRandomness);
      this.Controls.Add(this.chkAutoDJ);
      this.Controls.Add(this.btnSecond);
      this.Controls.Add(this.btnAuthenticate);
      this.Name = "LastFMConfig";
      this.Text = "LastFMConfig";
      ((System.ComponentModel.ISupportInitialize)(this.numRandomness)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnAuthenticate;
    private System.Windows.Forms.Button btnSecond;
    private System.Windows.Forms.CheckBox chkAutoDJ;
    private System.Windows.Forms.NumericUpDown numRandomness;
  }
}