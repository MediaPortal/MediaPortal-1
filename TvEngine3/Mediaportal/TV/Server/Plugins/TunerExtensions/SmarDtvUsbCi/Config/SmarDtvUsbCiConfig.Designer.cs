#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.Config
{
  partial class SmarDtvUsbCiConfig
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SmarDtvUsbCiConfig));
      this.pictureBoxTuner = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPPictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTuner)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBoxTuner
      // 
      this.pictureBoxTuner.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxTuner.Image")));
      this.pictureBoxTuner.Location = new System.Drawing.Point(33, 3);
      this.pictureBoxTuner.Name = "pictureBoxTuner";
      this.pictureBoxTuner.Size = new System.Drawing.Size(100, 50);
      this.pictureBoxTuner.TabIndex = 0;
      this.pictureBoxTuner.TabStop = false;
      // 
      // SmarDtvUsbCiConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.pictureBoxTuner);
      this.Name = "SmarDtvUsbCiConfig";
      this.Size = new System.Drawing.Size(480, 420);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTuner)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private SetupControls.UserInterfaceControls.MPPictureBox pictureBoxTuner;
  }
}
