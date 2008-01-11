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

namespace ProcessPlugins.TvMovie
{
  partial class TvMovieSettings
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TvMovieSettings));
      this.treeViewChannels = new System.Windows.Forms.TreeView();
      this.treeViewStations = new System.Windows.Forms.TreeView();
      this.buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxMapping = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.panelTimeSpan = new System.Windows.Forms.Panel();
      this.maskedTextBoxTimeStart = new System.Windows.Forms.MaskedTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.maskedTextBoxTimeEnd = new System.Windows.Forms.MaskedTextBox();
      this.listView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.listView2 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.pictureBox2 = new System.Windows.Forms.PictureBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.mpTabControl = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageMapChannels = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.labelNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPageSettings = new System.Windows.Forms.TabPage();
      this.groupBoxPerformance = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxSlowImport = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxGeneral = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxAdditionalInfo = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxShowAudioFormat = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxUseShortDesc = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpBeveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.checkBoxImportSchedules = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxMapping.SuspendLayout();
      this.panelTimeSpan.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.mpTabControl.SuspendLayout();
      this.tabPageMapChannels.SuspendLayout();
      this.tabPageSettings.SuspendLayout();
      this.groupBoxPerformance.SuspendLayout();
      this.groupBoxGeneral.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeViewChannels
      // 
      this.treeViewChannels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.treeViewChannels.HideSelection = false;
      this.treeViewChannels.Location = new System.Drawing.Point(16, 48);
      this.treeViewChannels.Name = "treeViewChannels";
      this.treeViewChannels.ShowNodeToolTips = true;
      this.treeViewChannels.ShowPlusMinus = false;
      this.treeViewChannels.ShowRootLines = false;
      this.treeViewChannels.Size = new System.Drawing.Size(240, 193);
      this.treeViewChannels.Sorted = true;
      this.treeViewChannels.TabIndex = 1;
      this.treeViewChannels.DoubleClick += new System.EventHandler(this.treeViewStations_DoubleClick);
      this.treeViewChannels.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewChannels_AfterSelect);
      // 
      // treeViewStations
      // 
      this.treeViewStations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.treeViewStations.HideSelection = false;
      this.treeViewStations.Location = new System.Drawing.Point(288, 48);
      this.treeViewStations.Name = "treeViewStations";
      this.treeViewStations.ShowNodeToolTips = true;
      this.treeViewStations.ShowPlusMinus = false;
      this.treeViewStations.ShowRootLines = false;
      this.treeViewStations.Size = new System.Drawing.Size(168, 216);
      this.treeViewStations.Sorted = true;
      this.treeViewStations.TabIndex = 2;
      this.treeViewStations.DoubleClick += new System.EventHandler(this.listBoxTvMovieChannels_DoubleClick);
      // 
      // buttonOk
      // 
      this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOk.Location = new System.Drawing.Point(376, 448);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 4;
      this.buttonOk.Text = "&OK";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // groupBoxMapping
      // 
      this.groupBoxMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMapping.Controls.Add(this.panelTimeSpan);
      this.groupBoxMapping.Controls.Add(this.treeViewChannels);
      this.groupBoxMapping.Controls.Add(this.treeViewStations);
      this.groupBoxMapping.Controls.Add(this.listView1);
      this.groupBoxMapping.Controls.Add(this.listView2);
      this.groupBoxMapping.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMapping.Location = new System.Drawing.Point(16, 16);
      this.groupBoxMapping.Name = "groupBoxMapping";
      this.groupBoxMapping.Size = new System.Drawing.Size(472, 280);
      this.groupBoxMapping.TabIndex = 5;
      this.groupBoxMapping.TabStop = false;
      this.groupBoxMapping.Text = "Map Channels to TV Movie Stations";
      // 
      // panelTimeSpan
      // 
      this.panelTimeSpan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.panelTimeSpan.AutoSize = true;
      this.panelTimeSpan.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.panelTimeSpan.Controls.Add(this.maskedTextBoxTimeStart);
      this.panelTimeSpan.Controls.Add(this.label1);
      this.panelTimeSpan.Controls.Add(this.maskedTextBoxTimeEnd);
      this.panelTimeSpan.Location = new System.Drawing.Point(64, 247);
      this.panelTimeSpan.Name = "panelTimeSpan";
      this.panelTimeSpan.Size = new System.Drawing.Size(139, 27);
      this.panelTimeSpan.TabIndex = 7;
      this.panelTimeSpan.Visible = false;
      // 
      // maskedTextBoxTimeStart
      // 
      this.maskedTextBoxTimeStart.AsciiOnly = true;
      this.maskedTextBoxTimeStart.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
      this.maskedTextBoxTimeStart.Location = new System.Drawing.Point(0, 4);
      this.maskedTextBoxTimeStart.Mask = "90:00";
      this.maskedTextBoxTimeStart.Name = "maskedTextBoxTimeStart";
      this.maskedTextBoxTimeStart.PromptChar = '0';
      this.maskedTextBoxTimeStart.RejectInputOnFirstFailure = true;
      this.maskedTextBoxTimeStart.Size = new System.Drawing.Size(56, 20);
      this.maskedTextBoxTimeStart.TabIndex = 7;
      this.maskedTextBoxTimeStart.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.maskedTextBoxTimeStart.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
      this.maskedTextBoxTimeStart.ValidatingType = typeof(System.DateTime);
      this.maskedTextBoxTimeStart.Validated += new System.EventHandler(this.maskedTextBoxTimeStart_Validated);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(63, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(10, 13);
      this.label1.TabIndex = 7;
      this.label1.Text = "-";
      // 
      // maskedTextBoxTimeEnd
      // 
      this.maskedTextBoxTimeEnd.AsciiOnly = true;
      this.maskedTextBoxTimeEnd.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
      this.maskedTextBoxTimeEnd.Location = new System.Drawing.Point(80, 4);
      this.maskedTextBoxTimeEnd.Mask = "90:00";
      this.maskedTextBoxTimeEnd.Name = "maskedTextBoxTimeEnd";
      this.maskedTextBoxTimeEnd.PromptChar = '0';
      this.maskedTextBoxTimeEnd.RejectInputOnFirstFailure = true;
      this.maskedTextBoxTimeEnd.Size = new System.Drawing.Size(56, 20);
      this.maskedTextBoxTimeEnd.TabIndex = 8;
      this.maskedTextBoxTimeEnd.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.maskedTextBoxTimeEnd.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
      this.maskedTextBoxTimeEnd.ValidatingType = typeof(System.DateTime);
      this.maskedTextBoxTimeEnd.Validated += new System.EventHandler(this.maskedTextBoxTimeEnd_Validated);
      // 
      // listView1
      // 
      this.listView1.AllowDrop = true;
      this.listView1.AllowRowReorder = true;
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listView1.Location = new System.Drawing.Point(16, 27);
      this.listView1.Name = "listView1";
      this.listView1.Scrollable = false;
      this.listView1.Size = new System.Drawing.Size(240, 24);
      this.listView1.TabIndex = 8;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "MediaPortal Channels";
      this.columnHeader1.Width = 262;
      // 
      // listView2
      // 
      this.listView2.AllowDrop = true;
      this.listView2.AllowRowReorder = true;
      this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.listView2.Location = new System.Drawing.Point(288, 27);
      this.listView2.Name = "listView2";
      this.listView2.Scrollable = false;
      this.listView2.Size = new System.Drawing.Size(168, 24);
      this.listView2.TabIndex = 9;
      this.listView2.UseCompatibleStateImageBehavior = false;
      this.listView2.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "TV Movie Stations";
      this.columnHeader2.Width = 179;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.Location = new System.Drawing.Point(456, 448);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 6;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // pictureBox2
      // 
      this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pictureBox2.Image = global::ProcessPlugins.Properties.Resources.tvmovie4_topgrafikrepeat_nw;
      this.pictureBox2.Location = new System.Drawing.Point(24, 16);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new System.Drawing.Size(508, 45);
      this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBox2.TabIndex = 8;
      this.pictureBox2.TabStop = false;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::ProcessPlugins.Properties.Resources.tvmovie4_topgrafik_nw;
      this.pictureBox1.Location = new System.Drawing.Point(8, 16);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(200, 45);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox1.TabIndex = 7;
      this.pictureBox1.TabStop = false;
      // 
      // mpTabControl
      // 
      this.mpTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpTabControl.Controls.Add(this.tabPageMapChannels);
      this.mpTabControl.Controls.Add(this.tabPageSettings);
      this.mpTabControl.Location = new System.Drawing.Point(16, 72);
      this.mpTabControl.Name = "mpTabControl";
      this.mpTabControl.SelectedIndex = 0;
      this.mpTabControl.Size = new System.Drawing.Size(512, 352);
      this.mpTabControl.TabIndex = 9;
      // 
      // tabPageMapChannels
      // 
      this.tabPageMapChannels.Controls.Add(this.labelNote);
      this.tabPageMapChannels.Controls.Add(this.groupBoxMapping);
      this.tabPageMapChannels.Location = new System.Drawing.Point(4, 22);
      this.tabPageMapChannels.Name = "tabPageMapChannels";
      this.tabPageMapChannels.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageMapChannels.Size = new System.Drawing.Size(504, 326);
      this.tabPageMapChannels.TabIndex = 0;
      this.tabPageMapChannels.Text = "Map Channels";
      this.tabPageMapChannels.UseVisualStyleBackColor = true;
      // 
      // labelNote
      // 
      this.labelNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelNote.AutoSize = true;
      this.labelNote.Location = new System.Drawing.Point(266, 301);
      this.labelNote.Name = "labelNote";
      this.labelNote.Size = new System.Drawing.Size(223, 13);
      this.labelNote.TabIndex = 6;
      this.labelNote.Text = "Note: Use doubleclick to map/unmap stations";
      this.labelNote.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // tabPageSettings
      // 
      this.tabPageSettings.Controls.Add(this.groupBoxPerformance);
      this.tabPageSettings.Controls.Add(this.groupBoxGeneral);
      this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageSettings.Name = "tabPageSettings";
      this.tabPageSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageSettings.Size = new System.Drawing.Size(504, 326);
      this.tabPageSettings.TabIndex = 1;
      this.tabPageSettings.Text = "Settings";
      this.tabPageSettings.UseVisualStyleBackColor = true;
      // 
      // groupBoxPerformance
      // 
      this.groupBoxPerformance.Controls.Add(this.checkBoxSlowImport);
      this.groupBoxPerformance.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxPerformance.Location = new System.Drawing.Point(16, 152);
      this.groupBoxPerformance.Name = "groupBoxPerformance";
      this.groupBoxPerformance.Size = new System.Drawing.Size(472, 56);
      this.groupBoxPerformance.TabIndex = 13;
      this.groupBoxPerformance.TabStop = false;
      this.groupBoxPerformance.Text = "Performance";
      // 
      // checkBoxSlowImport
      // 
      this.checkBoxSlowImport.AutoSize = true;
      this.checkBoxSlowImport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxSlowImport.Location = new System.Drawing.Point(16, 26);
      this.checkBoxSlowImport.Name = "checkBoxSlowImport";
      this.checkBoxSlowImport.Size = new System.Drawing.Size(381, 17);
      this.checkBoxSlowImport.TabIndex = 11;
      this.checkBoxSlowImport.Text = "Slower import (uses less processing power to solve possible video stuttering)";
      this.checkBoxSlowImport.UseVisualStyleBackColor = true;
      // 
      // groupBoxGeneral
      // 
      this.groupBoxGeneral.Controls.Add(this.checkBoxImportSchedules);
      this.groupBoxGeneral.Controls.Add(this.checkBoxAdditionalInfo);
      this.groupBoxGeneral.Controls.Add(this.checkBoxShowAudioFormat);
      this.groupBoxGeneral.Controls.Add(this.checkBoxUseShortDesc);
      this.groupBoxGeneral.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneral.Location = new System.Drawing.Point(16, 16);
      this.groupBoxGeneral.Name = "groupBoxGeneral";
      this.groupBoxGeneral.Size = new System.Drawing.Size(472, 128);
      this.groupBoxGeneral.TabIndex = 0;
      this.groupBoxGeneral.TabStop = false;
      this.groupBoxGeneral.Text = "General";
      // 
      // checkBoxAdditionalInfo
      // 
      this.checkBoxAdditionalInfo.AutoSize = true;
      this.checkBoxAdditionalInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAdditionalInfo.Location = new System.Drawing.Point(16, 48);
      this.checkBoxAdditionalInfo.Name = "checkBoxAdditionalInfo";
      this.checkBoxAdditionalInfo.Size = new System.Drawing.Size(222, 17);
      this.checkBoxAdditionalInfo.TabIndex = 13;
      this.checkBoxAdditionalInfo.Text = "Put additional info into the description field";
      this.checkBoxAdditionalInfo.UseVisualStyleBackColor = true;
      this.checkBoxAdditionalInfo.CheckedChanged += new System.EventHandler(this.checkBoxAdditionalInfo_CheckedChanged);
      // 
      // checkBoxShowAudioFormat
      // 
      this.checkBoxShowAudioFormat.AutoSize = true;
      this.checkBoxShowAudioFormat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxShowAudioFormat.Location = new System.Drawing.Point(16, 72);
      this.checkBoxShowAudioFormat.Name = "checkBoxShowAudioFormat";
      this.checkBoxShowAudioFormat.Size = new System.Drawing.Size(112, 17);
      this.checkBoxShowAudioFormat.TabIndex = 12;
      this.checkBoxShowAudioFormat.Text = "Show audio format";
      this.checkBoxShowAudioFormat.UseVisualStyleBackColor = true;
      // 
      // checkBoxUseShortDesc
      // 
      this.checkBoxUseShortDesc.AutoSize = true;
      this.checkBoxUseShortDesc.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseShortDesc.Location = new System.Drawing.Point(16, 26);
      this.checkBoxUseShortDesc.Name = "checkBoxUseShortDesc";
      this.checkBoxUseShortDesc.Size = new System.Drawing.Size(128, 17);
      this.checkBoxUseShortDesc.TabIndex = 11;
      this.checkBoxUseShortDesc.Text = "Use short descriptions";
      this.checkBoxUseShortDesc.UseVisualStyleBackColor = true;
      this.checkBoxUseShortDesc.CheckedChanged += new System.EventHandler(this.checkBoxUseShortDesc_CheckedChanged);
      // 
      // mpBeveledLine1
      // 
      this.mpBeveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpBeveledLine1.Location = new System.Drawing.Point(16, 438);
      this.mpBeveledLine1.Name = "mpBeveledLine1";
      this.mpBeveledLine1.Size = new System.Drawing.Size(508, 2);
      this.mpBeveledLine1.TabIndex = 10;
      // 
      // openFileDialog
      // 
      this.openFileDialog.DefaultExt = "mdb";
      this.openFileDialog.FileName = "tvdaten.mdb";
      this.openFileDialog.Filter = "TV Movie Database|*.mdb";
      this.openFileDialog.RestoreDirectory = true;
      // 
      // checkBoxImportSchedules
      // 
      this.checkBoxImportSchedules.AutoSize = true;
      this.checkBoxImportSchedules.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxImportSchedules.Location = new System.Drawing.Point(16, 96);
      this.checkBoxImportSchedules.Name = "checkBoxImportSchedules";
      this.checkBoxImportSchedules.Size = new System.Drawing.Size(298, 17);
      this.checkBoxImportSchedules.TabIndex = 16;
      this.checkBoxImportSchedules.Text = "Import recording schedules from ClickFinder (Experimental)";
      this.checkBoxImportSchedules.UseVisualStyleBackColor = true;
      // 
      // TvMovieSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(540, 478);
      this.Controls.Add(this.pictureBox2);
      this.Controls.Add(this.mpBeveledLine1);
      this.Controls.Add(this.mpTabControl);
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOk);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximumSize = new System.Drawing.Size(548, 5680);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(548, 402);
      this.Name = "TvMovieSettings";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Map TV Movie stations";
      this.TopMost = true;
      this.groupBoxMapping.ResumeLayout(false);
      this.groupBoxMapping.PerformLayout();
      this.panelTimeSpan.ResumeLayout(false);
      this.panelTimeSpan.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.mpTabControl.ResumeLayout(false);
      this.tabPageMapChannels.ResumeLayout(false);
      this.tabPageMapChannels.PerformLayout();
      this.tabPageSettings.ResumeLayout(false);
      this.groupBoxPerformance.ResumeLayout(false);
      this.groupBoxPerformance.PerformLayout();
      this.groupBoxGeneral.ResumeLayout(false);
      this.groupBoxGeneral.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TreeView treeViewChannels;
    private System.Windows.Forms.TreeView treeViewStations;
    private MediaPortal.UserInterface.Controls.MPButton buttonOk;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMapping;
    private MediaPortal.UserInterface.Controls.MPButton buttonCancel;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private System.Windows.Forms.MaskedTextBox maskedTextBoxTimeEnd;
    private System.Windows.Forms.MaskedTextBox maskedTextBoxTimeStart;
    private System.Windows.Forms.Panel panelTimeSpan;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.PictureBox pictureBox2;
    private MediaPortal.UserInterface.Controls.MPListView listView2;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private MediaPortal.UserInterface.Controls.MPTabControl mpTabControl;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageMapChannels;
    private MediaPortal.UserInterface.Controls.MPBeveledLine mpBeveledLine1;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxUseShortDesc;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxShowAudioFormat;
    private System.Windows.Forms.TabPage tabPageSettings;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGeneral;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxPerformance;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxSlowImport;
    private MediaPortal.UserInterface.Controls.MPLabel labelNote;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAdditionalInfo;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxImportSchedules;
  }
}

