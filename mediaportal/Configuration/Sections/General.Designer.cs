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

namespace MediaPortal.Configuration.Sections
{
  partial class General
  {
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

    #region Vom Komponenten-Designer generierter Code

    /// <summary> 
    /// Erforderliche Methode für die Designerunterstützung. 
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpThreadPriority = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lbDebug = new System.Windows.Forms.Label();
      this.groupBoxGeneralSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelPriority = new System.Windows.Forms.Label();
      this.cbDebug = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lbScreen = new System.Windows.Forms.Label();
      this.cbScreen = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.settingsCheckedListBox = new System.Windows.Forms.CheckedListBox();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // mpThreadPriority
      // 
      this.mpThreadPriority.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpThreadPriority.BorderColor = System.Drawing.Color.Empty;
      this.mpThreadPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.mpThreadPriority.FormattingEnabled = true;
      this.mpThreadPriority.Items.AddRange(new object[] {
            "High",
            "AboveNormal",
            "Normal",
            "BelowNormal"});
      this.mpThreadPriority.Location = new System.Drawing.Point(329, 372);
      this.mpThreadPriority.MinimumSize = new System.Drawing.Size(100, 0);
      this.mpThreadPriority.Name = "mpThreadPriority";
      this.mpThreadPriority.Size = new System.Drawing.Size(131, 21);
      this.mpThreadPriority.TabIndex = 2;
      // 
      // lbDebug
      // 
      this.lbDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lbDebug.AutoSize = true;
      this.lbDebug.Location = new System.Drawing.Point(6, 375);
      this.lbDebug.Name = "lbDebug";
      this.lbDebug.Size = new System.Drawing.Size(73, 13);
      this.lbDebug.TabIndex = 3;
      this.lbDebug.Text = "Log verbosity:";
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGeneralSettings.Controls.Add(this.labelPriority);
      this.groupBoxGeneralSettings.Controls.Add(this.mpThreadPriority);
      this.groupBoxGeneralSettings.Controls.Add(this.lbDebug);
      this.groupBoxGeneralSettings.Controls.Add(this.cbDebug);
      this.groupBoxGeneralSettings.Controls.Add(this.lbScreen);
      this.groupBoxGeneralSettings.Controls.Add(this.cbScreen);
      this.groupBoxGeneralSettings.Controls.Add(this.settingsCheckedListBox);
      this.groupBoxGeneralSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(3, 3);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(466, 399);
      this.groupBoxGeneralSettings.TabIndex = 1;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "General settings";
      // 
      // labelPriority
      // 
      this.labelPriority.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelPriority.AutoSize = true;
      this.labelPriority.Location = new System.Drawing.Point(242, 375);
      this.labelPriority.Name = "labelPriority";
      this.labelPriority.Size = new System.Drawing.Size(81, 13);
      this.labelPriority.TabIndex = 1;
      this.labelPriority.Text = "Process priority:";
      // 
      // cbDebug
      // 
      this.cbDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cbDebug.BorderColor = System.Drawing.Color.Empty;
      this.cbDebug.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDebug.FormattingEnabled = true;
      this.cbDebug.Items.AddRange(new object[] {
            "Error",
            "Warning",
            "Information",
            "Debug"});
      this.cbDebug.Location = new System.Drawing.Point(85, 372);
      this.cbDebug.MinimumSize = new System.Drawing.Size(100, 0);
      this.cbDebug.Name = "cbDebug";
      this.cbDebug.Size = new System.Drawing.Size(131, 21);
      this.cbDebug.TabIndex = 4;
      // 
      // lbScreen
      // 
      this.lbScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lbScreen.AutoSize = true;
      this.lbScreen.Location = new System.Drawing.Point(6, 349);
      this.lbScreen.Name = "lbScreen";
      this.lbScreen.Size = new System.Drawing.Size(67, 13);
      this.lbScreen.TabIndex = 5;
      this.lbScreen.Text = "Start screen:";
      // 
      // cbScreen
      // 
      this.cbScreen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbScreen.BorderColor = System.Drawing.Color.Empty;
      this.cbScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbScreen.Enabled = false;
      this.cbScreen.FormattingEnabled = true;
      this.cbScreen.Location = new System.Drawing.Point(85, 346);
      this.cbScreen.MinimumSize = new System.Drawing.Size(100, 0);
      this.cbScreen.Name = "cbScreen";
      this.cbScreen.Size = new System.Drawing.Size(375, 21);
      this.cbScreen.TabIndex = 6;
      // 
      // settingsCheckedListBox
      // 
      this.settingsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.settingsCheckedListBox.CheckOnClick = true;
      this.settingsCheckedListBox.Items.AddRange(new object[] {
            "Start with basic home screen",
            "Start MediaPortal in fullscreen mode",
            "Use alternative fullscreen Splashscreen (only if started in fullscreen mode)",
            "Autosize window mode to skin (breaks teletext)",
            "Keep MediaPortal always on top",
            "Hide taskbar in fullscreen mode",
            "Autostart MediaPortal on Windows startup",
            "Minimize to tray on start up",
            "Minimize to tray on GUI exit",
            "Show special mouse controls (scrollbars, etc)",
            "Hide file extensions like .mp3, .avi, .mpg,...",
            "Enable skin sound effects",
            "Enable animations / transitions",
            "Disable tray area\'s balloon tips (for all applications)",
            "Turn off monitor when blanking screen",
            "Turn monitor / tv on when resuming from standby",
            "Allow S3 standby although wake up devices are present",
            "Apply workaround to fix MediaPortal freezing on resume on some systems",
            "Restart MediaPortal on resume (avoids stuttering playback with nvidia)",
            "Show last active module when starting / resuming from standby",
            "Automatically skip commercials for videos with ComSkip data available",
            "Use screenselector to choose on which screen MP should start",
            "Allow remember last focused item on supported window/skin"});
      this.settingsCheckedListBox.Location = new System.Drawing.Point(6, 20);
      this.settingsCheckedListBox.Name = "settingsCheckedListBox";
      this.settingsCheckedListBox.Size = new System.Drawing.Size(454, 319);
      this.settingsCheckedListBox.TabIndex = 0;
      this.settingsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.settingsCheckedListBox_ItemCheck);
      // 
      // General
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBoxGeneralSettings);
      this.Name = "General";
      this.Size = new System.Drawing.Size(472, 402);
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPComboBox mpThreadPriority;
    private System.Windows.Forms.Label lbDebug;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGeneralSettings;
    private System.Windows.Forms.Label labelPriority;
    private MediaPortal.UserInterface.Controls.MPComboBox cbDebug;
    private System.Windows.Forms.CheckedListBox settingsCheckedListBox;
    private MediaPortal.UserInterface.Controls.MPComboBox cbScreen;
    private System.Windows.Forms.Label lbScreen;
  }
}
