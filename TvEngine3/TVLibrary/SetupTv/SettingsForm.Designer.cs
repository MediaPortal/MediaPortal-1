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

namespace SetupTv
{
  partial class SetupTvSettingsForm
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
      if (disposing)
      {
        /*if(components != null)
				{
					components.Dispose();
				}*/
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
      this.sectionTree = new System.Windows.Forms.TreeView();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.holderPanel = new System.Windows.Forms.Panel();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.applyButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonViewLog = new MediaPortal.UserInterface.Controls.MPButton();
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
      this.sectionTree.Location = new System.Drawing.Point(16, 16);
      this.sectionTree.Name = "sectionTree";
      this.sectionTree.Size = new System.Drawing.Size(184, 460);
      this.sectionTree.TabIndex = 3;
      this.sectionTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      this.sectionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.sectionTree_BeforeSelect);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(520, 519);
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
      this.okButton.Location = new System.Drawing.Point(441, 519);
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
      this.headerLabel.Location = new System.Drawing.Point(216, 16);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(522, 24);
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
      this.holderPanel.Location = new System.Drawing.Point(216, 48);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(522, 455);
      this.holderPanel.TabIndex = 5;
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 509);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(688, 2);
      this.beveledLine1.TabIndex = 6;
      this.beveledLine1.TabStop = false;
      // 
      // applyButton
      // 
      this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.applyButton.Location = new System.Drawing.Point(361, 519);
      this.applyButton.Name = "applyButton";
      this.applyButton.Size = new System.Drawing.Size(75, 23);
      this.applyButton.TabIndex = 7;
      this.applyButton.TabStop = false;
      this.applyButton.Text = "&Apply";
      this.applyButton.UseVisualStyleBackColor = true;
      this.applyButton.Visible = false;
      this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // buttonViewLog
      // 
      this.buttonViewLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonViewLog.Location = new System.Drawing.Point(600, 519);
      this.buttonViewLog.Name = "buttonViewLog";
      this.buttonViewLog.Size = new System.Drawing.Size(96, 23);
      this.buttonViewLog.TabIndex = 2;
      this.buttonViewLog.Text = "View &log files";
      this.buttonViewLog.UseVisualStyleBackColor = true;
      this.buttonViewLog.Click += new System.EventHandler(this.buttonViewLog_Click);
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.AutoScroll = true;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(754, 550);
      this.Controls.Add(this.buttonViewLog);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.sectionTree);
      this.Controls.Add(this.linkLabel1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "SettingsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Settings";
      this.Closed += new System.EventHandler(this.SettingsForm_Closed);
      this.Load += new System.EventHandler(this.SettingsForm_Load);
      this.ResumeLayout(false);
      // 
      // linkLabel1
      // 
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left))));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(16, 523);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(115, 13);
      this.linkLabel1.TabIndex = 8;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "Donate to MediaPortal";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
    }

    #endregion
  }
}