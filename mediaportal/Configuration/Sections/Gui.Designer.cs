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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.labelLoopDelayMs = new System.Windows.Forms.Label();
      this.listLoopDelayUpDown = new System.Windows.Forms.NumericUpDown();
      this.labelLoopDelay = new System.Windows.Forms.Label();
      this.settingsCheckedListBox = new System.Windows.Forms.CheckedListBox();
      this.groupBoxGuiSettings.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.listLoopDelayUpDown)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxGuiSettings
      // 
      this.groupBoxGuiSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGuiSettings.Controls.Add(this.groupBox1);
      this.groupBoxGuiSettings.Controls.Add(this.settingsCheckedListBox);
      this.groupBoxGuiSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGuiSettings.Location = new System.Drawing.Point(6, 0);
      this.groupBoxGuiSettings.Name = "groupBoxGuiSettings";
      this.groupBoxGuiSettings.Size = new System.Drawing.Size(462, 396);
      this.groupBoxGuiSettings.TabIndex = 1;
      this.groupBoxGuiSettings.TabStop = false;
      this.groupBoxGuiSettings.Text = "GUI settings";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.labelLoopDelayMs);
      this.groupBox1.Controls.Add(this.listLoopDelayUpDown);
      this.groupBox1.Controls.Add(this.labelLoopDelay);
      this.groupBox1.Location = new System.Drawing.Point(9, 135);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(445, 52);
      this.groupBox1.TabIndex = 15;
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
      this.listLoopDelayUpDown.Enabled = false;
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
      this.settingsCheckedListBox.Size = new System.Drawing.Size(450, 109);
      this.settingsCheckedListBox.TabIndex = 0;
      this.settingsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.settingsCheckedListBox_ItemCheck);
      // 
      // Gui
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBoxGuiSettings);
      this.Name = "Gui";
      this.Size = new System.Drawing.Size(472, 402);
      this.groupBoxGuiSettings.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.listLoopDelayUpDown)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGuiSettings;
    private System.Windows.Forms.CheckedListBox settingsCheckedListBox;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.NumericUpDown listLoopDelayUpDown;
    private System.Windows.Forms.Label labelLoopDelay;
    private System.Windows.Forms.Label labelLoopDelayMs;
  }
}
