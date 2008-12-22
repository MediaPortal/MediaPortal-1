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
  partial class WizardForm
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
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.nextButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.backButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.topPanel = new System.Windows.Forms.Panel();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.infoLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.topicLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.holderPanel = new System.Windows.Forms.Panel();
      this.topPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(536, 520);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(72, 22);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // nextButton
      // 
      this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.nextButton.Location = new System.Drawing.Point(456, 520);
      this.nextButton.Name = "nextButton";
      this.nextButton.Size = new System.Drawing.Size(72, 22);
      this.nextButton.TabIndex = 0;
      this.nextButton.Text = "&Next >";
      this.nextButton.UseVisualStyleBackColor = true;
      this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
      // 
      // backButton
      // 
      this.backButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.backButton.Location = new System.Drawing.Point(376, 520);
      this.backButton.Name = "backButton";
      this.backButton.Size = new System.Drawing.Size(72, 22);
      this.backButton.TabIndex = 5;
      this.backButton.Text = "< &Back";
      this.backButton.UseVisualStyleBackColor = true;
      this.backButton.Click += new System.EventHandler(this.backButton_Click);
      // 
      // topPanel
      // 
      this.topPanel.BackColor = System.Drawing.SystemColors.Window;
      this.topPanel.Controls.Add(this.pictureBox1);
      this.topPanel.Controls.Add(this.infoLabel);
      this.topPanel.Controls.Add(this.topicLabel);
      this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.topPanel.Location = new System.Drawing.Point(0, 0);
      this.topPanel.Name = "topPanel";
      this.topPanel.Size = new System.Drawing.Size(618, 72);
      this.topPanel.TabIndex = 2;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::SetupTv.Properties.Resources.wizard_header;
      this.pictureBox1.Location = new System.Drawing.Point(528, 14);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(70, 48);
      this.pictureBox1.TabIndex = 2;
      this.pictureBox1.TabStop = false;
      // 
      // infoLabel
      // 
      this.infoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.infoLabel.Location = new System.Drawing.Point(8, 26);
      this.infoLabel.Name = "infoLabel";
      this.infoLabel.Size = new System.Drawing.Size(512, 40);
      this.infoLabel.TabIndex = 1;
      this.infoLabel.Text = "Information information information information information";
      // 
      // topicLabel
      // 
      this.topicLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.topicLabel.Location = new System.Drawing.Point(8, 8);
      this.topicLabel.Name = "topicLabel";
      this.topicLabel.Size = new System.Drawing.Size(272, 23);
      this.topicLabel.TabIndex = 0;
      this.topicLabel.Text = "Topic";
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 72);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(618, 1);
      this.panel1.TabIndex = 7;
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 73);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(618, 1);
      this.panel2.TabIndex = 3;
      // 
      // holderPanel
      // 
      this.holderPanel.Location = new System.Drawing.Point(16, 88);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(584, 408);
      this.holderPanel.TabIndex = 4;
      // 
      // WizardForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(618, 552);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.topPanel);
      this.Controls.Add(this.backButton);
      this.Controls.Add(this.nextButton);
      this.Controls.Add(this.cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "WizardForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "WizardForm";
      this.Load += new System.EventHandler(this.WizardForm_Load);
      this.topPanel.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    private MediaPortal.UserInterface.Controls.MPButton nextButton;
    private MediaPortal.UserInterface.Controls.MPButton backButton;
    private System.Windows.Forms.Panel topPanel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Panel holderPanel;
    private MediaPortal.UserInterface.Controls.MPLabel topicLabel;
    private MediaPortal.UserInterface.Controls.MPLabel infoLabel;
    private System.Windows.Forms.PictureBox pictureBox1;
  }
}