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

using System;
using System.ComponentModel;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class GuiScrollSpeed : SectionSettings
  {
    private MPGroupBox mpGroupBoxScrollSettings;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown VerticalScrollSpeedUpDown;
    private System.Windows.Forms.NumericUpDown HorizontalScrollSpeedUpDown;
    private IContainer components = null;

    public GuiScrollSpeed()
      : this("Home Screen") {}

    public GuiScrollSpeed(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }


    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        HorizontalScrollSpeedUpDown.Value = xmlreader.GetValueAsInt("gui", "ScrollSpeedRight", 1);
        VerticalScrollSpeedUpDown.Value = xmlreader.GetValueAsInt("gui", "ScrollSpeedDown", 4);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("gui", "ScrollSpeedRight", HorizontalScrollSpeedUpDown.Value);
        xmlwriter.SetValue("gui", "ScrollSpeedDown", VerticalScrollSpeedUpDown.Value);
      }
    }

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpGroupBoxScrollSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.VerticalScrollSpeedUpDown = new System.Windows.Forms.NumericUpDown();
      this.HorizontalScrollSpeedUpDown = new System.Windows.Forms.NumericUpDown();
      this.mpGroupBoxScrollSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.VerticalScrollSpeedUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.HorizontalScrollSpeedUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // mpGroupBoxScrollSettings
      // 
      this.mpGroupBoxScrollSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBoxScrollSettings.Controls.Add(this.label3);
      this.mpGroupBoxScrollSettings.Controls.Add(this.label1);
      this.mpGroupBoxScrollSettings.Controls.Add(this.VerticalScrollSpeedUpDown);
      this.mpGroupBoxScrollSettings.Controls.Add(this.HorizontalScrollSpeedUpDown);
      this.mpGroupBoxScrollSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxScrollSettings.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBoxScrollSettings.Name = "mpGroupBoxScrollSettings";
      this.mpGroupBoxScrollSettings.Size = new System.Drawing.Size(460, 70);
      this.mpGroupBoxScrollSettings.TabIndex = 7;
      this.mpGroupBoxScrollSettings.TabStop = false;
      this.mpGroupBoxScrollSettings.Text = "Scroll Options";
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(7, 41);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(103, 13);
      this.label3.TabIndex = 9;
      this.label3.Text = "Vertical Scroll speed";
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(7, 18);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(115, 13);
      this.label1.TabIndex = 8;
      this.label1.Text = "Horizontal Scroll speed";
      // 
      // VerticalScrollSpeedUpDown
      // 
      this.VerticalScrollSpeedUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.VerticalScrollSpeedUpDown.Location = new System.Drawing.Point(128, 39);
      this.VerticalScrollSpeedUpDown.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.VerticalScrollSpeedUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.VerticalScrollSpeedUpDown.Name = "VerticalScrollSpeedUpDown";
      this.VerticalScrollSpeedUpDown.Size = new System.Drawing.Size(28, 20);
      this.VerticalScrollSpeedUpDown.TabIndex = 7;
      this.VerticalScrollSpeedUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // HorizontalScrollSpeedUpDown
      // 
      this.HorizontalScrollSpeedUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.HorizontalScrollSpeedUpDown.Location = new System.Drawing.Point(128, 16);
      this.HorizontalScrollSpeedUpDown.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.HorizontalScrollSpeedUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.HorizontalScrollSpeedUpDown.Name = "HorizontalScrollSpeedUpDown";
      this.HorizontalScrollSpeedUpDown.Size = new System.Drawing.Size(28, 20);
      this.HorizontalScrollSpeedUpDown.TabIndex = 6;
      this.HorizontalScrollSpeedUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // GuiScrollSpeed
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.mpGroupBoxScrollSettings);
      this.Name = "GuiScrollSpeed";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBoxScrollSettings.ResumeLayout(false);
      this.mpGroupBoxScrollSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.VerticalScrollSpeedUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.HorizontalScrollSpeedUpDown)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion
  }
}