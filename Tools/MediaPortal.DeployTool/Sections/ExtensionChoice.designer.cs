#region Copyright (C) 2005-2023 Team MediaPortal

// Copyright (C) 2005-2023 Team MediaPortal
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

namespace MediaPortal.DeployTool.Sections
{
  partial class ExtensionChoice
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
      this.linkExtensions = new System.Windows.Forms.LinkLabel();
      this.lblRecommended = new System.Windows.Forms.Label();

      this.grpLAV = new System.Windows.Forms.GroupBox();
      this.linkLAV = new System.Windows.Forms.LinkLabel();
      this.lblLAV = new System.Windows.Forms.Label();
      this.chkLAV = new System.Windows.Forms.CheckBox();
      this.pbLavFilters = new System.Windows.Forms.PictureBox();

      this.grpTitan = new System.Windows.Forms.GroupBox();
      this.linkTitan = new System.Windows.Forms.LinkLabel();
      this.lblTitan = new System.Windows.Forms.Label();
      this.chkTitan = new System.Windows.Forms.CheckBox();

      this.grpLAV.SuspendLayout();
      this.grpTitan.SuspendLayout();

      ((System.ComponentModel.ISupportInitialize)(this.pbLavFilters)).BeginInit();
      this.SuspendLayout();

      // 
      // linkExtensions
      // 
      this.linkExtensions.AutoSize = true;
      this.linkExtensions.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.linkExtensions.LinkColor = System.Drawing.Color.White;
      this.linkExtensions.Location = new System.Drawing.Point(330, 377);
      this.linkExtensions.Name = "linkExtensions";
      this.linkExtensions.Size = new System.Drawing.Size(185, 17);
      this.linkExtensions.TabIndex = 13;
      this.linkExtensions.TabStop = true;
      this.linkExtensions.Text = "Browse other extensions";
      this.linkExtensions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkExtensions_LinkClicked_1);
      // 
      // lblRecommended
      // 
      this.lblRecommended.AutoSize = true;
      this.lblRecommended.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold);
      this.lblRecommended.ForeColor = System.Drawing.Color.White;
      this.lblRecommended.Location = new System.Drawing.Point(330, 31);
      this.lblRecommended.Name = "lblRecommended";
      this.lblRecommended.Size = new System.Drawing.Size(211, 17);
      this.lblRecommended.TabIndex = 14;
      this.lblRecommended.Text = "Recommended Extensions";
      // 
      // grpLAV
      // 
      this.grpLAV.Controls.Add(this.linkLAV);
      this.grpLAV.Controls.Add(this.lblLAV);
      this.grpLAV.Controls.Add(this.chkLAV);
      this.grpLAV.Location = new System.Drawing.Point(333, 65);
      this.grpLAV.Name = "grpLAV";
      this.grpLAV.Size = new System.Drawing.Size(513, 84);
      this.grpLAV.TabIndex = 12;
      this.grpLAV.TabStop = false;
      // 
      // linkLAV
      // 
      this.linkLAV.AutoSize = true;
      this.linkLAV.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.linkLAV.LinkColor = System.Drawing.Color.White;
      this.linkLAV.Location = new System.Drawing.Point(434, 59);
      this.linkLAV.Name = "linkLAV";
      this.linkLAV.Size = new System.Drawing.Size(62, 13);
      this.linkLAV.TabIndex = 11;
      this.linkLAV.TabStop = true;
      this.linkLAV.Text = "More Info";
      this.linkLAV.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLAV_LinkClicked);
      // 
      // lblLAV
      // 
      this.lblLAV.AutoSize = true;
      this.lblLAV.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblLAV.ForeColor = System.Drawing.Color.White;
      this.lblLAV.Location = new System.Drawing.Point(31, 39);
      this.lblLAV.Name = "lblLAV";
      this.lblLAV.Size = new System.Drawing.Size(465, 13);
      this.lblLAV.TabIndex = 10;
      this.lblLAV.Text = "Install LAV Filters to enable playback of many common audio and video formats";
      // 
      // chkLAV
      // 
      this.chkLAV.AutoSize = true;
      this.chkLAV.Checked = true;
      this.chkLAV.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkLAV.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.chkLAV.ForeColor = System.Drawing.Color.White;
      this.chkLAV.Location = new System.Drawing.Point(15, 19);
      this.chkLAV.Name = "chkLAV";
      this.chkLAV.Size = new System.Drawing.Size(86, 17);
      this.chkLAV.TabIndex = 9;
      this.chkLAV.Text = "LAV Filters";
      this.chkLAV.UseVisualStyleBackColor = true;
      // 
      // pbLavFilters
      // 
      this.pbLavFilters.Image = global::MediaPortal.DeployTool.Images.LAVFilters;
      this.pbLavFilters.Location = new System.Drawing.Point(247, 70);
      this.pbLavFilters.Name = "pbLavFilters";
      this.pbLavFilters.Size = new System.Drawing.Size(80, 80);
      this.pbLavFilters.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbLavFilters.TabIndex = 15;
      this.pbLavFilters.TabStop = false;
      // 
      // grpTitan
      // 
      this.grpTitan.Controls.Add(this.linkTitan);
      this.grpTitan.Controls.Add(this.lblTitan);
      this.grpTitan.Controls.Add(this.chkTitan);
      this.grpTitan.Location = new System.Drawing.Point(53, 149);
      this.grpTitan.Name = "grpTitan";
      this.grpTitan.Size = new System.Drawing.Size(513, 70);
      this.grpTitan.TabIndex = 11;
      this.grpTitan.TabStop = false;
      // Enable or Disable Titan Extended related by displaying or not the groupbox.
      this.grpTitan.Visible = true;
      this.chkTitan.Checked = this.grpTitan.Visible;
      // 
      // linkTitan
      // 
      this.linkTitan.AutoSize = true;
      this.linkTitan.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.linkTitan.LinkColor = System.Drawing.Color.White;
      this.linkTitan.Location = new System.Drawing.Point(437, 50);
      this.linkTitan.Name = "linkTitan";
      this.linkTitan.Size = new System.Drawing.Size(52, 13);
      this.linkTitan.TabIndex = 11;
      this.linkTitan.TabStop = true;
      this.linkTitan.Text = "More Info";
      this.linkTitan.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkTitan_LinkClicked);
      // 
      // lblTitan
      // 
      this.lblTitan.AutoSize = true;
      this.lblTitan.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblTitan.ForeColor = System.Drawing.Color.White;
      this.lblTitan.Location = new System.Drawing.Point(36, 39);
      this.lblTitan.Name = "lblTitan";
      this.lblTitan.Size = new System.Drawing.Size(305, 13);
      this.lblTitan.TabIndex = 10;
      this.lblTitan.Text = "Install extension that adds support for popular third party plugins";
      // 
      // chkTitan
      // 
      this.chkTitan.AutoSize = true;
      this.chkTitan.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.chkTitan.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkTitan.ForeColor = System.Drawing.Color.White;
      this.chkTitan.Location = new System.Drawing.Point(15, 19);
      this.chkTitan.Name = "chkTitan";
      this.chkTitan.Size = new System.Drawing.Size(98, 17);
      this.chkTitan.TabIndex = 9;
      this.chkTitan.Text = "Titan Extended";
      this.chkTitan.UseVisualStyleBackColor = true;
      // 
      // ExtensionChoice
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.lblRecommended);
      this.Controls.Add(this.linkExtensions);
      this.Controls.Add(this.grpLAV);
      this.Controls.Add(this.grpTitan);
      this.Controls.Add(this.pbLavFilters);
      this.Name = "ExtensionChoice";
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.linkExtensions, 0);
      this.Controls.SetChildIndex(this.lblRecommended, 0);
      this.Controls.SetChildIndex(this.grpLAV, 0);
      this.Controls.SetChildIndex(this.pbLavFilters, 0);
      this.Controls.SetChildIndex(this.grpTitan, 0);
      this.grpLAV.ResumeLayout(false);
      this.grpLAV.PerformLayout();
      this.grpTitan.ResumeLayout(false);
      this.grpTitan.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbLavFilters)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.LinkLabel linkExtensions;
    private System.Windows.Forms.Label lblRecommended;
    private System.Windows.Forms.GroupBox grpLAV;
    private System.Windows.Forms.CheckBox chkLAV;
    private System.Windows.Forms.Label lblLAV;
    private System.Windows.Forms.LinkLabel linkLAV;
    private System.Windows.Forms.PictureBox pbLavFilters;
    private System.Windows.Forms.GroupBox grpTitan;
    private System.Windows.Forms.CheckBox chkTitan;
    private System.Windows.Forms.Label lblTitan;
    private System.Windows.Forms.LinkLabel linkTitan;
  }
}
