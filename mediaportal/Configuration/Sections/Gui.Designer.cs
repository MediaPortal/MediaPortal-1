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
      this.homeComboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.settingsCheckedListBox = new System.Windows.Forms.CheckedListBox();
      this.groupBoxSkin = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.panelFitImage = new System.Windows.Forms.Panel();
      this.previewPictureBox = new System.Windows.Forms.PictureBox();
      this.listViewAvailableSkins = new System.Windows.Forms.ListView();
      this.colName = new System.Windows.Forms.ColumnHeader();
      this.colVersion = new System.Windows.Forms.ColumnHeader();
      this.groupBoxGuiSettings.SuspendLayout();
      this.groupBoxSkin.SuspendLayout();
      this.panelFitImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBoxGuiSettings
      // 
      this.groupBoxGuiSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGuiSettings.Controls.Add(this.homeComboBox);
      this.groupBoxGuiSettings.Controls.Add(this.mpLabel1);
      this.groupBoxGuiSettings.Controls.Add(this.settingsCheckedListBox);
      this.groupBoxGuiSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGuiSettings.Location = new System.Drawing.Point(6, 203);
      this.groupBoxGuiSettings.Name = "groupBoxGuiSettings";
      this.groupBoxGuiSettings.Size = new System.Drawing.Size(462, 190);
      this.groupBoxGuiSettings.TabIndex = 1;
      this.groupBoxGuiSettings.TabStop = false;
      this.groupBoxGuiSettings.Text = "GUI settings";
      // 
      // homeComboBox
      // 
      this.homeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.homeComboBox.BorderColor = System.Drawing.Color.Empty;
      this.homeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.homeComboBox.Items.AddRange(new object[] {
            "Classic and Basic, prefer Classic",
            "Classic and Basic, prefer Basic",
            "only Classic Home",
            "only Basic Home"});
      this.homeComboBox.Location = new System.Drawing.Point(108, 124);
      this.homeComboBox.Name = "homeComboBox";
      this.homeComboBox.Size = new System.Drawing.Size(315, 21);
      this.homeComboBox.TabIndex = 11;
      // 
      // mpLabel1
      // 
      this.mpLabel1.Location = new System.Drawing.Point(6, 127);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(96, 16);
      this.mpLabel1.TabIndex = 10;
      this.mpLabel1.Text = "Home Screen:";
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
            "Show special mouse controls (scrollbars, etc)"});
      this.settingsCheckedListBox.Location = new System.Drawing.Point(6, 20);
      this.settingsCheckedListBox.Name = "settingsCheckedListBox";
      this.settingsCheckedListBox.Size = new System.Drawing.Size(450, 94);
      this.settingsCheckedListBox.TabIndex = 0;
      // 
      // groupBoxSkin
      // 
      this.groupBoxSkin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSkin.Controls.Add(this.linkLabel1);
      this.groupBoxSkin.Controls.Add(this.panelFitImage);
      this.groupBoxSkin.Controls.Add(this.listViewAvailableSkins);
      this.groupBoxSkin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxSkin.Location = new System.Drawing.Point(6, 0);
      this.groupBoxSkin.Name = "groupBoxSkin";
      this.groupBoxSkin.Size = new System.Drawing.Size(462, 197);
      this.groupBoxSkin.TabIndex = 4;
      this.groupBoxSkin.TabStop = false;
      this.groupBoxSkin.Text = "Skin Selection";
      // 
      // linkLabel1
      // 
      this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(16, 167);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(131, 13);
      this.linkLabel1.TabIndex = 10;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "more new and hot skins ...";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // panelFitImage
      // 
      this.panelFitImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.panelFitImage.Controls.Add(this.previewPictureBox);
      this.panelFitImage.Location = new System.Drawing.Point(223, 22);
      this.panelFitImage.Name = "panelFitImage";
      this.panelFitImage.Size = new System.Drawing.Size(222, 123);
      this.panelFitImage.TabIndex = 5;
      // 
      // previewPictureBox
      // 
      this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.previewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.previewPictureBox.Image = global::MediaPortal.Configuration.Properties.Resources.mplogo;
      this.previewPictureBox.Location = new System.Drawing.Point(0, 0);
      this.previewPictureBox.Name = "previewPictureBox";
      this.previewPictureBox.Size = new System.Drawing.Size(222, 123);
      this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.previewPictureBox.TabIndex = 5;
      this.previewPictureBox.TabStop = false;
      // 
      // listViewAvailableSkins
      // 
      this.listViewAvailableSkins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listViewAvailableSkins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colVersion});
      this.listViewAvailableSkins.FullRowSelect = true;
      this.listViewAvailableSkins.HideSelection = false;
      this.listViewAvailableSkins.Location = new System.Drawing.Point(15, 22);
      this.listViewAvailableSkins.MultiSelect = false;
      this.listViewAvailableSkins.Name = "listViewAvailableSkins";
      this.listViewAvailableSkins.Size = new System.Drawing.Size(200, 142);
      this.listViewAvailableSkins.TabIndex = 3;
      this.listViewAvailableSkins.UseCompatibleStateImageBehavior = false;
      this.listViewAvailableSkins.View = System.Windows.Forms.View.Details;
      this.listViewAvailableSkins.SelectedIndexChanged += new System.EventHandler(this.listViewAvailableSkins_SelectedIndexChanged);
      // 
      // colName
      // 
      this.colName.Text = "Name";
      this.colName.Width = 140;
      // 
      // colVersion
      // 
      this.colVersion.Text = "Version";
      this.colVersion.Width = 56;
      // 
      // Gui
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBoxSkin);
      this.Controls.Add(this.groupBoxGuiSettings);
      this.Name = "Gui";
      this.Size = new System.Drawing.Size(472, 402);
      this.groupBoxGuiSettings.ResumeLayout(false);
      this.groupBoxSkin.ResumeLayout(false);
      this.groupBoxSkin.PerformLayout();
      this.panelFitImage.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGuiSettings;
    private System.Windows.Forms.CheckedListBox settingsCheckedListBox;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxSkin;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.Panel panelFitImage;
    private System.Windows.Forms.PictureBox previewPictureBox;
    private System.Windows.Forms.ListView listViewAvailableSkins;
    private System.Windows.Forms.ColumnHeader colName;
    private System.Windows.Forms.ColumnHeader colVersion;
    private MediaPortal.UserInterface.Controls.MPComboBox homeComboBox;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
  }
}
