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
  partial class Gui
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
      this.groupBoxGuiSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.settingsCheckedListBox = new System.Windows.Forms.CheckedListBox();
      this.groupBoxGuiSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxGuiSettings
      // 
      this.groupBoxGuiSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGuiSettings.Controls.Add(this.settingsCheckedListBox);
      this.groupBoxGuiSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGuiSettings.Location = new System.Drawing.Point(3, 3);
      this.groupBoxGuiSettings.Name = "groupBoxGuiSettings";
      this.groupBoxGuiSettings.Size = new System.Drawing.Size(466, 137);
      this.groupBoxGuiSettings.TabIndex = 1;
      this.groupBoxGuiSettings.TabStop = false;
      this.groupBoxGuiSettings.Text = "GUI settings";
      // 
      // settingsCheckedListBox
      // 
      this.settingsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.settingsCheckedListBox.CheckOnClick = true;
      this.settingsCheckedListBox.Items.AddRange(new object[] {
            "Allow remember last focused item on supported window/skin",
            "Autosize window mode to skin dimensions",
            "Hide file extensions like .mp3, .avi, .mpg,...",
            "Enable file existence cache (improves performance on some systems)",
            "Enable skin sound effects",
            "Set loop delay when scrolling lists",
            "Show special mouse controls (scrollbars, etc)"});
      this.settingsCheckedListBox.Location = new System.Drawing.Point(6, 20);
      this.settingsCheckedListBox.Name = "settingsCheckedListBox";
      this.settingsCheckedListBox.Size = new System.Drawing.Size(454, 109);
      this.settingsCheckedListBox.TabIndex = 0;
      // 
      // Gui
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBoxGuiSettings);
      this.Name = "Gui";
      this.Size = new System.Drawing.Size(472, 402);
      this.groupBoxGuiSettings.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGuiSettings;
    private System.Windows.Forms.CheckedListBox settingsCheckedListBox;
  }
}
