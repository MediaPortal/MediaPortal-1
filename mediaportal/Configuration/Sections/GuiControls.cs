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
  public class GuiControls : SectionSettings
  {
    private MPGroupBox mpGroupBoxScrollSettings;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown VerticalScrollSpeedUpDown;
    private System.Windows.Forms.NumericUpDown HorizontalScrollSpeedUpDown;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label labelLoopDelayMs;
    private System.Windows.Forms.NumericUpDown listLoopDelayUpDown;
    private System.Windows.Forms.Label labelLoopDelay;
    private IContainer components = null;

    public GuiControls()
      : this("Control settings") {}

    public GuiControls(string name)
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
        listLoopDelayUpDown.Value = xmlreader.GetValueAsInt("gui", "listLoopDelay", 100);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("gui", "ScrollSpeedRight", HorizontalScrollSpeedUpDown.Value);
        xmlwriter.SetValue("gui", "ScrollSpeedDown", VerticalScrollSpeedUpDown.Value);
        xmlwriter.SetValue("gui", "listLoopDelay", listLoopDelayUpDown.Value);
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.labelLoopDelayMs = new System.Windows.Forms.Label();
      this.listLoopDelayUpDown = new System.Windows.Forms.NumericUpDown();
      this.labelLoopDelay = new System.Windows.Forms.Label();
      this.mpGroupBoxScrollSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.VerticalScrollSpeedUpDown)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.HorizontalScrollSpeedUpDown)).BeginInit();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.listLoopDelayUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // mpGroupBoxScrollSettings
      // 
      this.mpGroupBoxScrollSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBoxScrollSettings.Controls.Add(this.label3);
      this.mpGroupBoxScrollSettings.Controls.Add(this.label1);
      this.mpGroupBoxScrollSettings.Controls.Add(this.VerticalScrollSpeedUpDown);
      this.mpGroupBoxScrollSettings.Controls.Add(this.HorizontalScrollSpeedUpDown);
      this.mpGroupBoxScrollSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBoxScrollSettings.Location = new System.Drawing.Point(6, 0);
      this.mpGroupBoxScrollSettings.Name = "mpGroupBoxScrollSettings";
      this.mpGroupBoxScrollSettings.Size = new System.Drawing.Size(462, 50);
      this.mpGroupBoxScrollSettings.TabIndex = 7;
      this.mpGroupBoxScrollSettings.TabStop = false;
      this.mpGroupBoxScrollSettings.Text = "Scroll behavior";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(182, 22);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(104, 13);
      this.label3.TabIndex = 9;
      this.label3.Text = "Vertical scroll speed:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 22);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(116, 13);
      this.label1.TabIndex = 8;
      this.label1.Text = "Horizontal scroll speed:";
      // 
      // VerticalScrollSpeedUpDown
      // 
      this.VerticalScrollSpeedUpDown.Location = new System.Drawing.Point(293, 20);
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
      this.HorizontalScrollSpeedUpDown.Location = new System.Drawing.Point(130, 20);
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
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.labelLoopDelayMs);
      this.groupBox1.Controls.Add(this.listLoopDelayUpDown);
      this.groupBox1.Controls.Add(this.labelLoopDelay);
      this.groupBox1.Location = new System.Drawing.Point(6, 56);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(462, 50);
      this.groupBox1.TabIndex = 16;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "List behavior";
      // 
      // labelLoopDelayMs
      // 
      this.labelLoopDelayMs.AutoSize = true;
      this.labelLoopDelayMs.Location = new System.Drawing.Point(207, 22);
      this.labelLoopDelayMs.Name = "labelLoopDelayMs";
      this.labelLoopDelayMs.Size = new System.Drawing.Size(63, 13);
      this.labelLoopDelayMs.TabIndex = 17;
      this.labelLoopDelayMs.Text = "milliseconds";
      // 
      // listLoopDelayUpDown
      // 
      this.listLoopDelayUpDown.Location = new System.Drawing.Point(149, 20);
      this.listLoopDelayUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
      this.listLoopDelayUpDown.Name = "listLoopDelayUpDown";
      this.listLoopDelayUpDown.Size = new System.Drawing.Size(52, 20);
      this.listLoopDelayUpDown.TabIndex = 15;
      this.listLoopDelayUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.listLoopDelayUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
      // 
      // labelLoopDelay
      // 
      this.labelLoopDelay.AutoSize = true;
      this.labelLoopDelay.Location = new System.Drawing.Point(6, 22);
      this.labelLoopDelay.Name = "labelLoopDelay";
      this.labelLoopDelay.Size = new System.Drawing.Size(138, 13);
      this.labelLoopDelay.TabIndex = 16;
      this.labelLoopDelay.Text = "Loop delay for scrolling lists:";
      // 
      // GuiControls
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.mpGroupBoxScrollSettings);
      this.Name = "GuiControls";
      this.Size = new System.Drawing.Size(472, 408);
      this.mpGroupBoxScrollSettings.ResumeLayout(false);
      this.mpGroupBoxScrollSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.VerticalScrollSpeedUpDown)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.HorizontalScrollSpeedUpDown)).EndInit();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.listLoopDelayUpDown)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion
  }
}