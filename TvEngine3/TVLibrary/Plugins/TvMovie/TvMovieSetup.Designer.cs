#region Copyright (C) 2006-2007 Team MediaPortal

/* 
 *	Copyright (C) 2006-2007 Team MediaPortal
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

namespace SetupTv.Sections
{
  partial class TvMovieSetup
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
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.tabControlTvMovie = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageSettings = new System.Windows.Forms.TabPage();
      this.groupBoxDescriptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxAdditionalInfo = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxShowAudioFormat = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxUseShortDesc = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxEnableTvMovie = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.progressBarImportItem = new System.Windows.Forms.ProgressBar();
      this.progressBarImportTotal = new System.Windows.Forms.ProgressBar();
      this.buttonImportNow = new System.Windows.Forms.Button();
      this.groupBoxImportTime = new System.Windows.Forms.GroupBox();
      this.checkBoxSlowImport = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.radioButton7d = new System.Windows.Forms.RadioButton();
      this.radioButton2d = new System.Windows.Forms.RadioButton();
      this.radioButton24h = new System.Windows.Forms.RadioButton();
      this.radioButton12h = new System.Windows.Forms.RadioButton();
      this.radioButton6h = new System.Windows.Forms.RadioButton();
      this.checkBoxEnableImport = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBoxInfos = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.linkLabelInfo = new System.Windows.Forms.LinkLabel();
      this.labelInfo = new System.Windows.Forms.Label();
      this.tabPageMapChannels = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.labelNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxMapping = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.panelTimeSpan = new System.Windows.Forms.Panel();
      this.maskedTextBoxTimeStart = new System.Windows.Forms.MaskedTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.maskedTextBoxTimeEnd = new System.Windows.Forms.MaskedTextBox();
      this.treeViewChannels = new System.Windows.Forms.TreeView();
      this.treeViewStations = new System.Windows.Forms.TreeView();
      this.listView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.listView2 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.checkBoxShowRatings = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlTvMovie.SuspendLayout();
      this.tabPageSettings.SuspendLayout();
      this.groupBoxDescriptions.SuspendLayout();
      this.groupBoxEnableTvMovie.SuspendLayout();
      this.groupBoxImportTime.SuspendLayout();
      this.groupBoxInfos.SuspendLayout();
      this.tabPageMapChannels.SuspendLayout();
      this.groupBoxMapping.SuspendLayout();
      this.panelTimeSpan.SuspendLayout();
      this.SuspendLayout();
      // 
      // openFileDialog
      // 
      this.openFileDialog.DefaultExt = "mdb";
      this.openFileDialog.FileName = "tvdaten.mdb";
      this.openFileDialog.Filter = "TV Movie Database|*.mdb";
      this.openFileDialog.RestoreDirectory = true;
      // 
      // tabControlTvMovie
      // 
      this.tabControlTvMovie.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControlTvMovie.Controls.Add(this.tabPageSettings);
      this.tabControlTvMovie.Controls.Add(this.tabPageMapChannels);
      this.tabControlTvMovie.Location = new System.Drawing.Point(0, 0);
      this.tabControlTvMovie.Name = "tabControlTvMovie";
      this.tabControlTvMovie.SelectedIndex = 0;
      this.tabControlTvMovie.Size = new System.Drawing.Size(464, 384);
      this.tabControlTvMovie.TabIndex = 0;
      this.tabControlTvMovie.SelectedIndexChanged += new System.EventHandler(this.tabControlTvMovie_SelectedIndexChanged);
      // 
      // tabPageSettings
      // 
      this.tabPageSettings.Controls.Add(this.groupBoxDescriptions);
      this.tabPageSettings.Controls.Add(this.groupBoxEnableTvMovie);
      this.tabPageSettings.Controls.Add(this.groupBoxInfos);
      this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageSettings.Name = "tabPageSettings";
      this.tabPageSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageSettings.Size = new System.Drawing.Size(456, 358);
      this.tabPageSettings.TabIndex = 1;
      this.tabPageSettings.Text = "Settings";
      this.tabPageSettings.UseVisualStyleBackColor = true;
      // 
      // groupBoxDescriptions
      // 
      this.groupBoxDescriptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxDescriptions.Controls.Add(this.checkBoxShowRatings);
      this.groupBoxDescriptions.Controls.Add(this.checkBoxAdditionalInfo);
      this.groupBoxDescriptions.Controls.Add(this.checkBoxShowAudioFormat);
      this.groupBoxDescriptions.Controls.Add(this.checkBoxUseShortDesc);
      this.groupBoxDescriptions.Enabled = false;
      this.groupBoxDescriptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxDescriptions.Location = new System.Drawing.Point(16, 175);
      this.groupBoxDescriptions.Name = "groupBoxDescriptions";
      this.groupBoxDescriptions.Size = new System.Drawing.Size(424, 103);
      this.groupBoxDescriptions.TabIndex = 4;
      this.groupBoxDescriptions.TabStop = false;
      this.groupBoxDescriptions.Text = "Descriptions";
      // 
      // checkBoxAdditionalInfo
      // 
      this.checkBoxAdditionalInfo.AutoSize = true;
      this.checkBoxAdditionalInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxAdditionalInfo.Location = new System.Drawing.Point(16, 40);
      this.checkBoxAdditionalInfo.Name = "checkBoxAdditionalInfo";
      this.checkBoxAdditionalInfo.Size = new System.Drawing.Size(294, 17);
      this.checkBoxAdditionalInfo.TabIndex = 1;
      this.checkBoxAdditionalInfo.Text = "Display additional info like Episode, FSK, Year, Actors etc";
      this.checkBoxAdditionalInfo.UseVisualStyleBackColor = true;
      // 
      // checkBoxShowAudioFormat
      // 
      this.checkBoxShowAudioFormat.AutoSize = true;
      this.checkBoxShowAudioFormat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxShowAudioFormat.Location = new System.Drawing.Point(16, 80);
      this.checkBoxShowAudioFormat.Name = "checkBoxShowAudioFormat";
      this.checkBoxShowAudioFormat.Size = new System.Drawing.Size(171, 17);
      this.checkBoxShowAudioFormat.TabIndex = 2;
      this.checkBoxShowAudioFormat.Text = "Show audio format (if available)";
      this.checkBoxShowAudioFormat.UseVisualStyleBackColor = true;
      // 
      // checkBoxUseShortDesc
      // 
      this.checkBoxUseShortDesc.AutoSize = true;
      this.checkBoxUseShortDesc.Checked = true;
      this.checkBoxUseShortDesc.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxUseShortDesc.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseShortDesc.Location = new System.Drawing.Point(16, 20);
      this.checkBoxUseShortDesc.Name = "checkBoxUseShortDesc";
      this.checkBoxUseShortDesc.Size = new System.Drawing.Size(204, 17);
      this.checkBoxUseShortDesc.TabIndex = 0;
      this.checkBoxUseShortDesc.Text = "Use short program descriptions (faster)";
      this.checkBoxUseShortDesc.UseVisualStyleBackColor = true;
      // 
      // groupBoxEnableTvMovie
      // 
      this.groupBoxEnableTvMovie.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxEnableTvMovie.Controls.Add(this.progressBarImportItem);
      this.groupBoxEnableTvMovie.Controls.Add(this.progressBarImportTotal);
      this.groupBoxEnableTvMovie.Controls.Add(this.buttonImportNow);
      this.groupBoxEnableTvMovie.Controls.Add(this.groupBoxImportTime);
      this.groupBoxEnableTvMovie.Controls.Add(this.checkBoxEnableImport);
      this.groupBoxEnableTvMovie.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxEnableTvMovie.Location = new System.Drawing.Point(16, 8);
      this.groupBoxEnableTvMovie.Name = "groupBoxEnableTvMovie";
      this.groupBoxEnableTvMovie.Size = new System.Drawing.Size(424, 161);
      this.groupBoxEnableTvMovie.TabIndex = 0;
      this.groupBoxEnableTvMovie.TabStop = false;
      this.groupBoxEnableTvMovie.Text = "TV Movie ClickFinder EPG importer";
      // 
      // progressBarImportItem
      // 
      this.progressBarImportItem.Location = new System.Drawing.Point(114, 139);
      this.progressBarImportItem.Name = "progressBarImportItem";
      this.progressBarImportItem.Size = new System.Drawing.Size(293, 10);
      this.progressBarImportItem.TabIndex = 60;
      // 
      // progressBarImportTotal
      // 
      this.progressBarImportTotal.Location = new System.Drawing.Point(114, 123);
      this.progressBarImportTotal.Name = "progressBarImportTotal";
      this.progressBarImportTotal.Size = new System.Drawing.Size(293, 10);
      this.progressBarImportTotal.TabIndex = 59;
      // 
      // buttonImportNow
      // 
      this.buttonImportNow.Location = new System.Drawing.Point(16, 123);
      this.buttonImportNow.Name = "buttonImportNow";
      this.buttonImportNow.Size = new System.Drawing.Size(75, 26);
      this.buttonImportNow.TabIndex = 7;
      this.buttonImportNow.Text = "Import now!";
      this.buttonImportNow.UseVisualStyleBackColor = true;
      this.buttonImportNow.Click += new System.EventHandler(this.buttonImportNow_Click);
      // 
      // groupBoxImportTime
      // 
      this.groupBoxImportTime.Controls.Add(this.checkBoxSlowImport);
      this.groupBoxImportTime.Controls.Add(this.radioButton7d);
      this.groupBoxImportTime.Controls.Add(this.radioButton2d);
      this.groupBoxImportTime.Controls.Add(this.radioButton24h);
      this.groupBoxImportTime.Controls.Add(this.radioButton12h);
      this.groupBoxImportTime.Controls.Add(this.radioButton6h);
      this.groupBoxImportTime.Enabled = false;
      this.groupBoxImportTime.Location = new System.Drawing.Point(16, 43);
      this.groupBoxImportTime.Name = "groupBoxImportTime";
      this.groupBoxImportTime.Size = new System.Drawing.Size(391, 69);
      this.groupBoxImportTime.TabIndex = 2;
      this.groupBoxImportTime.TabStop = false;
      this.groupBoxImportTime.Text = "Import newer database after";
      // 
      // checkBoxSlowImport
      // 
      this.checkBoxSlowImport.AutoSize = true;
      this.checkBoxSlowImport.Checked = true;
      this.checkBoxSlowImport.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxSlowImport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxSlowImport.Location = new System.Drawing.Point(15, 43);
      this.checkBoxSlowImport.Name = "checkBoxSlowImport";
      this.checkBoxSlowImport.Size = new System.Drawing.Size(245, 17);
      this.checkBoxSlowImport.TabIndex = 7;
      this.checkBoxSlowImport.Text = "Slower import (less cpu - better while using MP)";
      this.checkBoxSlowImport.UseVisualStyleBackColor = true;
      // 
      // radioButton7d
      // 
      this.radioButton7d.AutoSize = true;
      this.radioButton7d.Location = new System.Drawing.Point(299, 18);
      this.radioButton7d.Name = "radioButton7d";
      this.radioButton7d.Size = new System.Drawing.Size(56, 17);
      this.radioButton7d.TabIndex = 4;
      this.radioButton7d.Text = "7 days";
      this.radioButton7d.UseVisualStyleBackColor = true;
      // 
      // radioButton2d
      // 
      this.radioButton2d.AutoSize = true;
      this.radioButton2d.Location = new System.Drawing.Point(234, 18);
      this.radioButton2d.Name = "radioButton2d";
      this.radioButton2d.Size = new System.Drawing.Size(56, 17);
      this.radioButton2d.TabIndex = 3;
      this.radioButton2d.Text = "2 days";
      this.radioButton2d.UseVisualStyleBackColor = true;
      // 
      // radioButton24h
      // 
      this.radioButton24h.AutoSize = true;
      this.radioButton24h.Checked = true;
      this.radioButton24h.Location = new System.Drawing.Point(159, 18);
      this.radioButton24h.Name = "radioButton24h";
      this.radioButton24h.Size = new System.Drawing.Size(66, 17);
      this.radioButton24h.TabIndex = 2;
      this.radioButton24h.TabStop = true;
      this.radioButton24h.Text = "24 hours";
      this.radioButton24h.UseVisualStyleBackColor = true;
      // 
      // radioButton12h
      // 
      this.radioButton12h.AutoSize = true;
      this.radioButton12h.Location = new System.Drawing.Point(84, 18);
      this.radioButton12h.Name = "radioButton12h";
      this.radioButton12h.Size = new System.Drawing.Size(66, 17);
      this.radioButton12h.TabIndex = 1;
      this.radioButton12h.Text = "12 hours";
      this.radioButton12h.UseVisualStyleBackColor = true;
      // 
      // radioButton6h
      // 
      this.radioButton6h.AutoSize = true;
      this.radioButton6h.Location = new System.Drawing.Point(15, 18);
      this.radioButton6h.Name = "radioButton6h";
      this.radioButton6h.Size = new System.Drawing.Size(60, 17);
      this.radioButton6h.TabIndex = 0;
      this.radioButton6h.TabStop = true;
      this.radioButton6h.Text = "6 hours";
      this.radioButton6h.UseVisualStyleBackColor = true;
      // 
      // checkBoxEnableImport
      // 
      this.checkBoxEnableImport.AutoSize = true;
      this.checkBoxEnableImport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxEnableImport.Location = new System.Drawing.Point(16, 20);
      this.checkBoxEnableImport.Name = "checkBoxEnableImport";
      this.checkBoxEnableImport.Size = new System.Drawing.Size(207, 17);
      this.checkBoxEnableImport.TabIndex = 0;
      this.checkBoxEnableImport.Text = "Enable import from TV Movie database";
      this.checkBoxEnableImport.UseVisualStyleBackColor = true;
      this.checkBoxEnableImport.CheckedChanged += new System.EventHandler(this.checkBoxEnableImport_CheckedChanged);
      // 
      // groupBoxInfos
      // 
      this.groupBoxInfos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxInfos.Controls.Add(this.linkLabelInfo);
      this.groupBoxInfos.Controls.Add(this.labelInfo);
      this.groupBoxInfos.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxInfos.Location = new System.Drawing.Point(16, 284);
      this.groupBoxInfos.Name = "groupBoxInfos";
      this.groupBoxInfos.Size = new System.Drawing.Size(424, 65);
      this.groupBoxInfos.TabIndex = 3;
      this.groupBoxInfos.TabStop = false;
      this.groupBoxInfos.Text = "Info";
      // 
      // linkLabelInfo
      // 
      this.linkLabelInfo.AutoSize = true;
      this.linkLabelInfo.Location = new System.Drawing.Point(16, 41);
      this.linkLabelInfo.Name = "linkLabelInfo";
      this.linkLabelInfo.Size = new System.Drawing.Size(119, 13);
      this.linkLabelInfo.TabIndex = 1;
      this.linkLabelInfo.TabStop = true;
      this.linkLabelInfo.Text = "Click here to read more.";
      this.linkLabelInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelInfo_LinkClicked);
      // 
      // labelInfo
      // 
      this.labelInfo.AutoSize = true;
      this.labelInfo.Location = new System.Drawing.Point(16, 21);
      this.labelInfo.Name = "labelInfo";
      this.labelInfo.Size = new System.Drawing.Size(333, 13);
      this.labelInfo.TabIndex = 0;
      this.labelInfo.Text = "TV Movie ClickFinder is an EPG application for German TV channels.";
      // 
      // tabPageMapChannels
      // 
      this.tabPageMapChannels.Controls.Add(this.labelNote);
      this.tabPageMapChannels.Controls.Add(this.groupBoxMapping);
      this.tabPageMapChannels.Location = new System.Drawing.Point(4, 22);
      this.tabPageMapChannels.Name = "tabPageMapChannels";
      this.tabPageMapChannels.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageMapChannels.Size = new System.Drawing.Size(456, 358);
      this.tabPageMapChannels.TabIndex = 0;
      this.tabPageMapChannels.Text = "Map Channels";
      this.tabPageMapChannels.UseVisualStyleBackColor = true;
      // 
      // labelNote
      // 
      this.labelNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelNote.AutoSize = true;
      this.labelNote.Location = new System.Drawing.Point(218, 333);
      this.labelNote.Name = "labelNote";
      this.labelNote.Size = new System.Drawing.Size(223, 13);
      this.labelNote.TabIndex = 1;
      this.labelNote.Text = "Note: Use doubleclick to map/unmap stations";
      this.labelNote.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
      this.groupBoxMapping.Size = new System.Drawing.Size(424, 312);
      this.groupBoxMapping.TabIndex = 0;
      this.groupBoxMapping.TabStop = false;
      this.groupBoxMapping.Text = "Map channels to TV Movie stations";
      // 
      // panelTimeSpan
      // 
      this.panelTimeSpan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.panelTimeSpan.AutoSize = true;
      this.panelTimeSpan.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.panelTimeSpan.Controls.Add(this.maskedTextBoxTimeStart);
      this.panelTimeSpan.Controls.Add(this.label1);
      this.panelTimeSpan.Controls.Add(this.maskedTextBoxTimeEnd);
      this.panelTimeSpan.Location = new System.Drawing.Point(64, 279);
      this.panelTimeSpan.Name = "panelTimeSpan";
      this.panelTimeSpan.Size = new System.Drawing.Size(139, 27);
      this.panelTimeSpan.TabIndex = 4;
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
      this.maskedTextBoxTimeStart.TabIndex = 0;
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
      this.label1.TabIndex = 1;
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
      this.maskedTextBoxTimeEnd.TabIndex = 2;
      this.maskedTextBoxTimeEnd.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.maskedTextBoxTimeEnd.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludePromptAndLiterals;
      this.maskedTextBoxTimeEnd.ValidatingType = typeof(System.DateTime);
      this.maskedTextBoxTimeEnd.Validated += new System.EventHandler(this.maskedTextBoxTimeEnd_Validated);
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
      this.treeViewChannels.Size = new System.Drawing.Size(216, 225);
      this.treeViewChannels.Sorted = true;
      this.treeViewChannels.TabIndex = 1;
      this.treeViewChannels.DoubleClick += new System.EventHandler(this.treeViewChannels_DoubleClick);
      this.treeViewChannels.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewChannels_AfterSelect);
      // 
      // treeViewStations
      // 
      this.treeViewStations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.treeViewStations.HideSelection = false;
      this.treeViewStations.Location = new System.Drawing.Point(240, 48);
      this.treeViewStations.Name = "treeViewStations";
      this.treeViewStations.ShowNodeToolTips = true;
      this.treeViewStations.ShowPlusMinus = false;
      this.treeViewStations.ShowRootLines = false;
      this.treeViewStations.Size = new System.Drawing.Size(168, 248);
      this.treeViewStations.Sorted = true;
      this.treeViewStations.TabIndex = 3;
      this.treeViewStations.DoubleClick += new System.EventHandler(this.treeViewStations_DoubleClick);
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
      this.listView1.Size = new System.Drawing.Size(216, 24);
      this.listView1.TabIndex = 0;
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
      this.listView2.Location = new System.Drawing.Point(240, 27);
      this.listView2.Name = "listView2";
      this.listView2.Scrollable = false;
      this.listView2.Size = new System.Drawing.Size(168, 24);
      this.listView2.TabIndex = 2;
      this.listView2.UseCompatibleStateImageBehavior = false;
      this.listView2.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "TV Movie Stations";
      this.columnHeader2.Width = 179;
      // 
      // checkBoxShowRatings
      // 
      this.checkBoxShowRatings.AutoSize = true;
      this.checkBoxShowRatings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxShowRatings.Location = new System.Drawing.Point(16, 60);
      this.checkBoxShowRatings.Name = "checkBoxShowRatings";
      this.checkBoxShowRatings.Size = new System.Drawing.Size(126, 17);
      this.checkBoxShowRatings.TabIndex = 3;
      this.checkBoxShowRatings.Text = "Show program ratings";
      this.checkBoxShowRatings.UseVisualStyleBackColor = true;
      // 
      // TvMovieSetup
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControlTvMovie);
      this.Name = "TvMovieSetup";
      this.Size = new System.Drawing.Size(467, 388);
      this.tabControlTvMovie.ResumeLayout(false);
      this.tabPageSettings.ResumeLayout(false);
      this.groupBoxDescriptions.ResumeLayout(false);
      this.groupBoxDescriptions.PerformLayout();
      this.groupBoxEnableTvMovie.ResumeLayout(false);
      this.groupBoxEnableTvMovie.PerformLayout();
      this.groupBoxImportTime.ResumeLayout(false);
      this.groupBoxImportTime.PerformLayout();
      this.groupBoxInfos.ResumeLayout(false);
      this.groupBoxInfos.PerformLayout();
      this.tabPageMapChannels.ResumeLayout(false);
      this.tabPageMapChannels.PerformLayout();
      this.groupBoxMapping.ResumeLayout(false);
      this.groupBoxMapping.PerformLayout();
      this.panelTimeSpan.ResumeLayout(false);
      this.panelTimeSpan.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControlTvMovie;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPageMapChannels;
    private MediaPortal.UserInterface.Controls.MPLabel labelNote;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxMapping;
    private System.Windows.Forms.Panel panelTimeSpan;
    private System.Windows.Forms.MaskedTextBox maskedTextBoxTimeStart;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private System.Windows.Forms.MaskedTextBox maskedTextBoxTimeEnd;
    private System.Windows.Forms.TreeView treeViewChannels;
    private System.Windows.Forms.TreeView treeViewStations;
    private MediaPortal.UserInterface.Controls.MPListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private MediaPortal.UserInterface.Controls.MPListView listView2;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.TabPage tabPageSettings;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableImport;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxInfos;
    private System.Windows.Forms.LinkLabel linkLabelInfo;
    private System.Windows.Forms.Label labelInfo;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxEnableTvMovie;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxDescriptions;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAdditionalInfo;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxShowAudioFormat;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxUseShortDesc;
    private System.Windows.Forms.GroupBox groupBoxImportTime;
    private System.Windows.Forms.RadioButton radioButton24h;
    private System.Windows.Forms.RadioButton radioButton12h;
    private System.Windows.Forms.RadioButton radioButton6h;
    private System.Windows.Forms.RadioButton radioButton7d;
    private System.Windows.Forms.RadioButton radioButton2d;
    private System.Windows.Forms.Button buttonImportNow;
    private System.Windows.Forms.ProgressBar progressBarImportItem;
    private System.Windows.Forms.ProgressBar progressBarImportTotal;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxSlowImport;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxShowRatings;
  }
}
