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

namespace MediaPortal.GUI.RADIOLASTFM
{
  partial class PluginSetupForm
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginSetupForm));
      this.panelBannerAlign = new System.Windows.Forms.Panel();
      this.pictureBoxBanner = new System.Windows.Forms.PictureBox();
      this.groupBoxSettings = new System.Windows.Forms.GroupBox();
      this.checkBoxOneClickMode = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxDirectSkip = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.labelListEntries = new MediaPortal.UserInterface.Controls.MPLabel();
      this.numericUpDownListEntries = new System.Windows.Forms.NumericUpDown();
      this.checkBoxShowBallonTips = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxSubmitToProfile = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxUseTrayIcon = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.labelEngine = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxStreamPlayerType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.buttonSave = new System.Windows.Forms.Button();
      this.panelBannerAlign.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBanner)).BeginInit();
      this.groupBoxSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownListEntries)).BeginInit();
      this.SuspendLayout();
      // 
      // panelBannerAlign
      // 
      this.panelBannerAlign.Controls.Add(this.pictureBoxBanner);
      this.panelBannerAlign.Location = new System.Drawing.Point(-3, 0);
      this.panelBannerAlign.Name = "panelBannerAlign";
      this.panelBannerAlign.Size = new System.Drawing.Size(400, 75);
      this.panelBannerAlign.TabIndex = 0;
      // 
      // pictureBoxBanner
      // 
      this.pictureBoxBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBoxBanner.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxBanner.Image")));
      this.pictureBoxBanner.Location = new System.Drawing.Point(0, 0);
      this.pictureBoxBanner.Name = "pictureBoxBanner";
      this.pictureBoxBanner.Size = new System.Drawing.Size(400, 75);
      this.pictureBoxBanner.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxBanner.TabIndex = 0;
      this.pictureBoxBanner.TabStop = false;
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSettings.Controls.Add(this.checkBoxOneClickMode);
      this.groupBoxSettings.Controls.Add(this.checkBoxDirectSkip);
      this.groupBoxSettings.Controls.Add(this.labelListEntries);
      this.groupBoxSettings.Controls.Add(this.numericUpDownListEntries);
      this.groupBoxSettings.Controls.Add(this.checkBoxShowBallonTips);
      this.groupBoxSettings.Controls.Add(this.checkBoxSubmitToProfile);
      this.groupBoxSettings.Controls.Add(this.checkBoxUseTrayIcon);
      this.groupBoxSettings.Controls.Add(this.labelEngine);
      this.groupBoxSettings.Controls.Add(this.comboBoxStreamPlayerType);
      this.groupBoxSettings.Location = new System.Drawing.Point(12, 81);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(370, 184);
      this.groupBoxSettings.TabIndex = 1;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // checkBoxOneClickMode
      // 
      this.checkBoxOneClickMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxOneClickMode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxOneClickMode.Location = new System.Drawing.Point(15, 73);
      this.checkBoxOneClickMode.Name = "checkBoxOneClickMode";
      this.checkBoxOneClickMode.Size = new System.Drawing.Size(349, 17);
      this.checkBoxOneClickMode.TabIndex = 8;
      this.checkBoxOneClickMode.Text = "A click on any button directly starts the stream";
      this.checkBoxOneClickMode.UseVisualStyleBackColor = true;
      // 
      // checkBoxDirectSkip
      // 
      this.checkBoxDirectSkip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxDirectSkip.Checked = true;
      this.checkBoxDirectSkip.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxDirectSkip.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxDirectSkip.Location = new System.Drawing.Point(15, 50);
      this.checkBoxDirectSkip.Name = "checkBoxDirectSkip";
      this.checkBoxDirectSkip.Size = new System.Drawing.Size(349, 17);
      this.checkBoxDirectSkip.TabIndex = 7;
      this.checkBoxDirectSkip.Text = "Skip directly to the next track (do not show selection)";
      this.checkBoxDirectSkip.UseVisualStyleBackColor = true;
      // 
      // labelListEntries
      // 
      this.labelListEntries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelListEntries.Location = new System.Drawing.Point(79, 150);
      this.labelListEntries.Name = "labelListEntries";
      this.labelListEntries.Size = new System.Drawing.Size(285, 13);
      this.labelListEntries.TabIndex = 4;
      this.labelListEntries.Text = "Maximum number of menu entries (for tags, friends, ...)";
      // 
      // numericUpDownListEntries
      // 
      this.numericUpDownListEntries.Location = new System.Drawing.Point(15, 148);
      this.numericUpDownListEntries.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownListEntries.Name = "numericUpDownListEntries";
      this.numericUpDownListEntries.Size = new System.Drawing.Size(58, 20);
      this.numericUpDownListEntries.TabIndex = 3;
      this.numericUpDownListEntries.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
      // 
      // checkBoxShowBallonTips
      // 
      this.checkBoxShowBallonTips.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxShowBallonTips.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxShowBallonTips.Location = new System.Drawing.Point(32, 118);
      this.checkBoxShowBallonTips.Name = "checkBoxShowBallonTips";
      this.checkBoxShowBallonTips.Size = new System.Drawing.Size(332, 17);
      this.checkBoxShowBallonTips.TabIndex = 2;
      this.checkBoxShowBallonTips.Text = "Display ballon tips on song change";
      this.checkBoxShowBallonTips.UseVisualStyleBackColor = true;
      // 
      // checkBoxSubmitToProfile
      // 
      this.checkBoxSubmitToProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxSubmitToProfile.Checked = true;
      this.checkBoxSubmitToProfile.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxSubmitToProfile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxSubmitToProfile.Location = new System.Drawing.Point(15, 27);
      this.checkBoxSubmitToProfile.Name = "checkBoxSubmitToProfile";
      this.checkBoxSubmitToProfile.Size = new System.Drawing.Size(349, 17);
      this.checkBoxSubmitToProfile.TabIndex = 1;
      this.checkBoxSubmitToProfile.Text = "Add radio tracks to your profile on last.fm";
      this.checkBoxSubmitToProfile.UseVisualStyleBackColor = true;
      // 
      // checkBoxUseTrayIcon
      // 
      this.checkBoxUseTrayIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBoxUseTrayIcon.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseTrayIcon.Location = new System.Drawing.Point(15, 96);
      this.checkBoxUseTrayIcon.Name = "checkBoxUseTrayIcon";
      this.checkBoxUseTrayIcon.Size = new System.Drawing.Size(349, 17);
      this.checkBoxUseTrayIcon.TabIndex = 0;
      this.checkBoxUseTrayIcon.Text = "Show tray icon (with context menu)";
      this.checkBoxUseTrayIcon.UseVisualStyleBackColor = true;
      this.checkBoxUseTrayIcon.CheckedChanged += new System.EventHandler(this.checkBoxUseTrayIcon_CheckedChanged);
      // 
      // labelEngine
      // 
      this.labelEngine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelEngine.AutoSize = true;
      this.labelEngine.Location = new System.Drawing.Point(79, 155);
      this.labelEngine.Name = "labelEngine";
      this.labelEngine.Size = new System.Drawing.Size(142, 13);
      this.labelEngine.TabIndex = 6;
      this.labelEngine.Text = "Audio player type for streams";
      this.labelEngine.Visible = false;
      // 
      // comboBoxStreamPlayerType
      // 
      this.comboBoxStreamPlayerType.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxStreamPlayerType.FormattingEnabled = true;
      this.comboBoxStreamPlayerType.Items.AddRange(new object[] {
            "BASS",
            "WMP"});
      this.comboBoxStreamPlayerType.Location = new System.Drawing.Point(15, 152);
      this.comboBoxStreamPlayerType.Name = "comboBoxStreamPlayerType";
      this.comboBoxStreamPlayerType.Size = new System.Drawing.Size(58, 21);
      this.comboBoxStreamPlayerType.TabIndex = 5;
      this.comboBoxStreamPlayerType.Text = "BASS";
      this.comboBoxStreamPlayerType.Visible = false;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(307, 271);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // buttonSave
      // 
      this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonSave.Location = new System.Drawing.Point(226, 271);
      this.buttonSave.Name = "buttonSave";
      this.buttonSave.Size = new System.Drawing.Size(75, 23);
      this.buttonSave.TabIndex = 3;
      this.buttonSave.Text = "&OK";
      this.buttonSave.UseVisualStyleBackColor = true;
      this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
      // 
      // PluginSetupForm
      // 
      this.AcceptButton = this.buttonSave;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(394, 306);
      this.Controls.Add(this.buttonSave);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.groupBoxSettings);
      this.Controls.Add(this.panelBannerAlign);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "PluginSetupForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "My Last.fm Radio - Setup";
      this.panelBannerAlign.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBanner)).EndInit();
      this.groupBoxSettings.ResumeLayout(false);
      this.groupBoxSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownListEntries)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panelBannerAlign;
    private System.Windows.Forms.PictureBox pictureBoxBanner;
    private System.Windows.Forms.GroupBox groupBoxSettings;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonSave;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxUseTrayIcon;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxSubmitToProfile;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxShowBallonTips;
    private MediaPortal.UserInterface.Controls.MPLabel labelListEntries;
    private System.Windows.Forms.NumericUpDown numericUpDownListEntries;
    private MediaPortal.UserInterface.Controls.MPLabel labelEngine;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBoxStreamPlayerType;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDirectSkip;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxOneClickMode;
  }
}