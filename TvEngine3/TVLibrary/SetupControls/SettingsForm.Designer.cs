#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

namespace SetupControls
{
  partial class SettingsForm
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
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
    protected void InitializeComponent()
    {
      this.sectionTree = new System.Windows.Forms.TreeView();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.holderPanel = new System.Windows.Forms.Panel();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.applyButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.helpToolStripSplitButton = new System.Windows.Forms.ToolStripSplitButton();
      this.updateHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.configToolStripSplitButton = new System.Windows.Forms.ToolStripSplitButton();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // sectionTree
      // 
      this.sectionTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.sectionTree.FullRowSelect = true;
      this.sectionTree.HideSelection = false;
      this.sectionTree.HotTracking = true;
      this.sectionTree.Indent = 19;
      this.sectionTree.ItemHeight = 16;
      this.sectionTree.Location = new System.Drawing.Point(16, 28);
      this.sectionTree.Name = "sectionTree";
      this.sectionTree.Size = new System.Drawing.Size(184, 455);
      this.sectionTree.TabIndex = 3;
      this.sectionTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      this.sectionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.sectionTree_BeforeSelect);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(625, 506);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(546, 506);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 0;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(216, 28);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(484, 24);
      this.headerLabel.TabIndex = 4;
      this.headerLabel.TabStop = false;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // holderPanel
      // 
      this.holderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.holderPanel.AutoScroll = true;
      this.holderPanel.BackColor = System.Drawing.SystemColors.Control;
      this.holderPanel.Location = new System.Drawing.Point(216, 58);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(484, 425);
      this.holderPanel.TabIndex = 5;
      this.holderPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.holderPanel_Paint);
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 496);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(700, 2);
      this.beveledLine1.TabIndex = 6;
      this.beveledLine1.TabStop = false;
      // 
      // applyButton
      // 
      this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.applyButton.Location = new System.Drawing.Point(466, 506);
      this.applyButton.Name = "applyButton";
      this.applyButton.Size = new System.Drawing.Size(75, 23);
      this.applyButton.TabIndex = 7;
      this.applyButton.TabStop = false;
      this.applyButton.Text = "&Apply";
      this.applyButton.UseVisualStyleBackColor = true;
      this.applyButton.Visible = false;
      this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // linkLabel1
      // 
      this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(13, 511);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(113, 13);
      this.linkLabel1.TabIndex = 8;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "Donate to MediaPortal";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // toolStrip1
      // 
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripSplitButton,
            this.configToolStripSplitButton});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(716, 25);
      this.toolStrip1.TabIndex = 13;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // helpToolStripSplitButton
      // 
      this.helpToolStripSplitButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.helpToolStripSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateHelpToolStripMenuItem});
      this.helpToolStripSplitButton.Image = global::SetupControls.Properties.Resources.icon_help;
      this.helpToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.helpToolStripSplitButton.Name = "helpToolStripSplitButton";
      this.helpToolStripSplitButton.Size = new System.Drawing.Size(60, 22);
      this.helpToolStripSplitButton.Text = "Help";
      this.helpToolStripSplitButton.ToolTipText = "Opens the online wiki page for the active configuration section.";
      this.helpToolStripSplitButton.ButtonClick += new System.EventHandler(this.helpToolStripSplitButton_ButtonClick);
      // 
      // updateHelpToolStripMenuItem
      // 
      this.updateHelpToolStripMenuItem.Image = global::SetupControls.Properties.Resources.icon_refresh;
      this.updateHelpToolStripMenuItem.Name = "updateHelpToolStripMenuItem";
      this.updateHelpToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.updateHelpToolStripMenuItem.Text = "Update Help";
      this.updateHelpToolStripMenuItem.ToolTipText = "Online update for the help references file. Use it if an incorrect wiki page was " +
          "opened.";
      this.updateHelpToolStripMenuItem.Click += new System.EventHandler(this.updateHelpToolStripMenuItem_Click);
      // 
      // configToolStripSplitButton
      // 
      this.configToolStripSplitButton.Image = global::SetupControls.Properties.Resources.icon_folder;
      this.configToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.configToolStripSplitButton.Name = "configToolStripSplitButton";
      this.configToolStripSplitButton.Size = new System.Drawing.Size(131, 22);
      this.configToolStripSplitButton.Text = "Open Log directory";
      this.configToolStripSplitButton.ButtonClick += new System.EventHandler(this.configToolStripSplitButton_ButtonClick);
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScroll = true;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(716, 537);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.linkLabel1);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.sectionTree);
      this.Name = "SettingsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Settings";
      this.Load += new System.EventHandler(this.SettingsForm_Load);
      this.Closed += new System.EventHandler(this.SettingsForm_Closed);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    private MediaPortal.UserInterface.Controls.MPButton okButton;
    private MediaPortal.UserInterface.Controls.MPButton applyButton;
    private MediaPortal.UserInterface.Controls.MPBeveledLine beveledLine1;
    protected System.Windows.Forms.TreeView sectionTree;
    protected System.Windows.Forms.Panel holderPanel;
    private MediaPortal.UserInterface.Controls.MPGradientLabel headerLabel;
    private System.Windows.Forms.LinkLabel linkLabel1;
    protected System.Windows.Forms.ToolStrip toolStrip1;
    protected System.Windows.Forms.ToolStripSplitButton helpToolStripSplitButton;
    protected System.Windows.Forms.ToolStripSplitButton configToolStripSplitButton;
    protected System.Windows.Forms.ToolStripMenuItem updateHelpToolStripMenuItem;
  }
}