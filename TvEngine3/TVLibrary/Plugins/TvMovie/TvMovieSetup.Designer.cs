#region Copyright (C) 2006-2009 Team MediaPortal

/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
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
      this.components = new System.ComponentModel.Container();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.tabControlTvMovie = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageSettings = new System.Windows.Forms.TabPage();
      this.linkLabelInfo = new System.Windows.Forms.LinkLabel();
      this.groupBoxEnableTvMovie = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBoxInstallMethod = new System.Windows.Forms.GroupBox();
      this.buttonBrowse = new System.Windows.Forms.Button();
      this.lbDbPath = new System.Windows.Forms.Label();
      this.rbManual = new System.Windows.Forms.RadioButton();
      this.rbLocal = new System.Windows.Forms.RadioButton();
      this.tbDbPath = new System.Windows.Forms.TextBox();
      this.groupBoxImportTime = new System.Windows.Forms.GroupBox();
      this.progressBarImportTotal = new System.Windows.Forms.ProgressBar();
      this.buttonImportNow = new System.Windows.Forms.Button();
      this.checkBoxSlowImport = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.radioButton7d = new System.Windows.Forms.RadioButton();
      this.radioButton2d = new System.Windows.Forms.RadioButton();
      this.radioButton24h = new System.Windows.Forms.RadioButton();
      this.radioButton12h = new System.Windows.Forms.RadioButton();
      this.radioButton6h = new System.Windows.Forms.RadioButton();
      this.checkBoxEnableImport = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPageImportOptions = new System.Windows.Forms.TabPage();
      this.groupBoxDescriptions = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.checkBoxLimitActors = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxShowRatings = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxAdditionalInfo = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxShowAudioFormat = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxUseShortDesc = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPageMapChannels = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.labelNote = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxMapping = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.panelTimeSpan = new System.Windows.Forms.Panel();
      this.maskedTextBoxTimeStart = new System.Windows.Forms.MaskedTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.maskedTextBoxTimeEnd = new System.Windows.Forms.MaskedTextBox();
      this.treeViewMpChannels = new System.Windows.Forms.TreeView();
      this.treeViewTvMStations = new System.Windows.Forms.TreeView();
      this.imageListTvmStations = new System.Windows.Forms.ImageList(this.components);
      this.listView1 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.listView2 = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.fileDialogDb = new System.Windows.Forms.OpenFileDialog();
      this.numericUpDownActorCount = new System.Windows.Forms.NumericUpDown();
      this.checkBoxShowLive = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.checkBoxShowRepeat = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlTvMovie.SuspendLayout();
      this.tabPageSettings.SuspendLayout();
      this.groupBoxEnableTvMovie.SuspendLayout();
      this.groupBoxInstallMethod.SuspendLayout();
      this.groupBoxImportTime.SuspendLayout();
      this.tabPageImportOptions.SuspendLayout();
      this.groupBoxDescriptions.SuspendLayout();
      this.tabPageMapChannels.SuspendLayout();
      this.groupBoxMapping.SuspendLayout();
      this.panelTimeSpan.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownActorCount)).BeginInit();
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
      this.tabControlTvMovie.Controls.Add(this.tabPageImportOptions);
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
      this.tabPageSettings.Controls.Add(this.linkLabelInfo);
      this.tabPageSettings.Controls.Add(this.groupBoxEnableTvMovie);
      this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageSettings.Name = "tabPageSettings";
      this.tabPageSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageSettings.Size = new System.Drawing.Size(456, 358);
      this.tabPageSettings.TabIndex = 1;
      this.tabPageSettings.Text = "Settings";
      this.tabPageSettings.UseVisualStyleBackColor = true;
      // 
      // linkLabelInfo
      // 
      this.linkLabelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabelInfo.AutoSize = true;
      this.linkLabelInfo.LinkArea = new System.Windows.Forms.LinkArea(0, 20);
      this.linkLabelInfo.Location = new System.Drawing.Point(32, 338);
      this.linkLabelInfo.Name = "linkLabelInfo";
      this.linkLabelInfo.Size = new System.Drawing.Size(352, 17);
      this.linkLabelInfo.TabIndex = 5;
      this.linkLabelInfo.TabStop = true;
      this.linkLabelInfo.Text = "TV Movie ClickFinder is an EPG application for German TV channels.";
      this.linkLabelInfo.UseCompatibleTextRendering = true;
      this.linkLabelInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelInfo_LinkClicked);
      // 
      // groupBoxEnableTvMovie
      // 
      this.groupBoxEnableTvMovie.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxEnableTvMovie.Controls.Add(this.groupBoxInstallMethod);
      this.groupBoxEnableTvMovie.Controls.Add(this.groupBoxImportTime);
      this.groupBoxEnableTvMovie.Controls.Add(this.checkBoxEnableImport);
      this.groupBoxEnableTvMovie.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxEnableTvMovie.Location = new System.Drawing.Point(16, 8);
      this.groupBoxEnableTvMovie.Name = "groupBoxEnableTvMovie";
      this.groupBoxEnableTvMovie.Size = new System.Drawing.Size(424, 262);
      this.groupBoxEnableTvMovie.TabIndex = 0;
      this.groupBoxEnableTvMovie.TabStop = false;
      this.groupBoxEnableTvMovie.Text = "TV Movie ClickFinder EPG importer";
      // 
      // groupBoxInstallMethod
      // 
      this.groupBoxInstallMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxInstallMethod.Controls.Add(this.buttonBrowse);
      this.groupBoxInstallMethod.Controls.Add(this.lbDbPath);
      this.groupBoxInstallMethod.Controls.Add(this.rbManual);
      this.groupBoxInstallMethod.Controls.Add(this.rbLocal);
      this.groupBoxInstallMethod.Controls.Add(this.tbDbPath);
      this.groupBoxInstallMethod.Enabled = false;
      this.groupBoxInstallMethod.Location = new System.Drawing.Point(16, 55);
      this.groupBoxInstallMethod.Name = "groupBoxInstallMethod";
      this.groupBoxInstallMethod.Size = new System.Drawing.Size(391, 77);
      this.groupBoxInstallMethod.TabIndex = 61;
      this.groupBoxInstallMethod.TabStop = false;
      this.groupBoxInstallMethod.Text = "Installation type";
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(351, 45);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(21, 20);
      this.buttonBrowse.TabIndex = 65;
      this.buttonBrowse.Text = "...";
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // lbDbPath
      // 
      this.lbDbPath.AutoSize = true;
      this.lbDbPath.Location = new System.Drawing.Point(12, 48);
      this.lbDbPath.Name = "lbDbPath";
      this.lbDbPath.Size = new System.Drawing.Size(77, 13);
      this.lbDbPath.TabIndex = 64;
      this.lbDbPath.Text = "Database path";
      // 
      // rbManual
      // 
      this.rbManual.AutoSize = true;
      this.rbManual.Location = new System.Drawing.Point(234, 19);
      this.rbManual.Name = "rbManual";
      this.rbManual.Size = new System.Drawing.Size(137, 17);
      this.rbManual.TabIndex = 63;
      this.rbManual.Text = "Manually import from file";
      this.rbManual.UseVisualStyleBackColor = true;
      // 
      // rbLocal
      // 
      this.rbLocal.AutoSize = true;
      this.rbLocal.Checked = true;
      this.rbLocal.Location = new System.Drawing.Point(15, 19);
      this.rbLocal.Name = "rbLocal";
      this.rbLocal.Size = new System.Drawing.Size(147, 17);
      this.rbLocal.TabIndex = 62;
      this.rbLocal.TabStop = true;
      this.rbLocal.Text = "Clickfinder installed locally";
      this.rbLocal.UseVisualStyleBackColor = true;
      this.rbLocal.CheckedChanged += new System.EventHandler(this.rbLocal_CheckedChanged);
      // 
      // tbDbPath
      // 
      this.tbDbPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbDbPath.Enabled = false;
      this.tbDbPath.Location = new System.Drawing.Point(98, 45);
      this.tbDbPath.Name = "tbDbPath";
      this.tbDbPath.Size = new System.Drawing.Size(245, 20);
      this.tbDbPath.TabIndex = 61;
      // 
      // groupBoxImportTime
      // 
      this.groupBoxImportTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxImportTime.Controls.Add(this.progressBarImportTotal);
      this.groupBoxImportTime.Controls.Add(this.buttonImportNow);
      this.groupBoxImportTime.Controls.Add(this.checkBoxSlowImport);
      this.groupBoxImportTime.Controls.Add(this.radioButton7d);
      this.groupBoxImportTime.Controls.Add(this.radioButton2d);
      this.groupBoxImportTime.Controls.Add(this.radioButton24h);
      this.groupBoxImportTime.Controls.Add(this.radioButton12h);
      this.groupBoxImportTime.Controls.Add(this.radioButton6h);
      this.groupBoxImportTime.Enabled = false;
      this.groupBoxImportTime.Location = new System.Drawing.Point(16, 147);
      this.groupBoxImportTime.Name = "groupBoxImportTime";
      this.groupBoxImportTime.Size = new System.Drawing.Size(391, 100);
      this.groupBoxImportTime.TabIndex = 2;
      this.groupBoxImportTime.TabStop = false;
      this.groupBoxImportTime.Text = "Import newer database after";
      // 
      // progressBarImportTotal
      // 
      this.progressBarImportTotal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarImportTotal.Location = new System.Drawing.Point(98, 68);
      this.progressBarImportTotal.Name = "progressBarImportTotal";
      this.progressBarImportTotal.Size = new System.Drawing.Size(274, 21);
      this.progressBarImportTotal.TabIndex = 61;
      // 
      // buttonImportNow
      // 
      this.buttonImportNow.Location = new System.Drawing.Point(15, 68);
      this.buttonImportNow.Name = "buttonImportNow";
      this.buttonImportNow.Size = new System.Drawing.Size(75, 21);
      this.buttonImportNow.TabIndex = 60;
      this.buttonImportNow.Text = "Import now!";
      this.buttonImportNow.UseVisualStyleBackColor = true;
      this.buttonImportNow.Click += new System.EventHandler(this.buttonImportNow_Click);
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
      // tabPageImportOptions
      // 
      this.tabPageImportOptions.Controls.Add(this.groupBoxDescriptions);
      this.tabPageImportOptions.Location = new System.Drawing.Point(4, 22);
      this.tabPageImportOptions.Name = "tabPageImportOptions";
      this.tabPageImportOptions.Size = new System.Drawing.Size(456, 358);
      this.tabPageImportOptions.TabIndex = 2;
      this.tabPageImportOptions.Text = "Import options";
      this.tabPageImportOptions.UseVisualStyleBackColor = true;
      // 
      // groupBoxDescriptions
      // 
      this.groupBoxDescriptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxDescriptions.Controls.Add(this.checkBoxShowRepeat);
      this.groupBoxDescriptions.Controls.Add(this.checkBoxShowLive);
      this.groupBoxDescriptions.Controls.Add(this.numericUpDownActorCount);
      this.groupBoxDescriptions.Controls.Add(this.checkBoxLimitActors);
      this.groupBoxDescriptions.Controls.Add(this.checkBoxShowRatings);
      this.groupBoxDescriptions.Controls.Add(this.checkBoxAdditionalInfo);
      this.groupBoxDescriptions.Controls.Add(this.checkBoxShowAudioFormat);
      this.groupBoxDescriptions.Controls.Add(this.checkBoxUseShortDesc);
      this.groupBoxDescriptions.Enabled = false;
      this.groupBoxDescriptions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxDescriptions.Location = new System.Drawing.Point(16, 8);
      this.groupBoxDescriptions.Name = "groupBoxDescriptions";
      this.groupBoxDescriptions.Size = new System.Drawing.Size(424, 180);
      this.groupBoxDescriptions.TabIndex = 5;
      this.groupBoxDescriptions.TabStop = false;
      this.groupBoxDescriptions.Text = "Descriptions";
      // 
      // checkBoxLimitActors
      // 
      this.checkBoxLimitActors.AutoSize = true;
      this.checkBoxLimitActors.Checked = true;
      this.checkBoxLimitActors.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxLimitActors.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxLimitActors.Location = new System.Drawing.Point(31, 63);
      this.checkBoxLimitActors.Name = "checkBoxLimitActors";
      this.checkBoxLimitActors.Size = new System.Drawing.Size(181, 17);
      this.checkBoxLimitActors.TabIndex = 4;
      this.checkBoxLimitActors.Text = "Limit actors - show a maximum of ";
      this.checkBoxLimitActors.UseVisualStyleBackColor = true;
      // 
      // checkBoxShowRatings
      // 
      this.checkBoxShowRatings.AutoSize = true;
      this.checkBoxShowRatings.Checked = true;
      this.checkBoxShowRatings.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxShowRatings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxShowRatings.Location = new System.Drawing.Point(16, 86);
      this.checkBoxShowRatings.Name = "checkBoxShowRatings";
      this.checkBoxShowRatings.Size = new System.Drawing.Size(126, 17);
      this.checkBoxShowRatings.TabIndex = 3;
      this.checkBoxShowRatings.Text = "Show program ratings";
      this.checkBoxShowRatings.UseVisualStyleBackColor = true;
      // 
      // checkBoxAdditionalInfo
      // 
      this.checkBoxAdditionalInfo.AutoSize = true;
      this.checkBoxAdditionalInfo.Checked = true;
      this.checkBoxAdditionalInfo.CheckState = System.Windows.Forms.CheckState.Checked;
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
      this.checkBoxShowAudioFormat.Location = new System.Drawing.Point(16, 106);
      this.checkBoxShowAudioFormat.Name = "checkBoxShowAudioFormat";
      this.checkBoxShowAudioFormat.Size = new System.Drawing.Size(117, 17);
      this.checkBoxShowAudioFormat.TabIndex = 2;
      this.checkBoxShowAudioFormat.Text = "Show audio formats";
      this.checkBoxShowAudioFormat.UseVisualStyleBackColor = true;
      // 
      // checkBoxUseShortDesc
      // 
      this.checkBoxUseShortDesc.AutoSize = true;
      this.checkBoxUseShortDesc.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxUseShortDesc.Location = new System.Drawing.Point(16, 20);
      this.checkBoxUseShortDesc.Name = "checkBoxUseShortDesc";
      this.checkBoxUseShortDesc.Size = new System.Drawing.Size(204, 17);
      this.checkBoxUseShortDesc.TabIndex = 0;
      this.checkBoxUseShortDesc.Text = "Use short program descriptions (faster)";
      this.checkBoxUseShortDesc.UseVisualStyleBackColor = true;
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
      this.tabPageMapChannels.Text = "Map channels";
      this.tabPageMapChannels.UseVisualStyleBackColor = true;
      // 
      // labelNote
      // 
      this.labelNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelNote.AutoSize = true;
      this.labelNote.Location = new System.Drawing.Point(217, 342);
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
      this.groupBoxMapping.Controls.Add(this.treeViewMpChannels);
      this.groupBoxMapping.Controls.Add(this.treeViewTvMStations);
      this.groupBoxMapping.Controls.Add(this.listView1);
      this.groupBoxMapping.Controls.Add(this.listView2);
      this.groupBoxMapping.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMapping.Location = new System.Drawing.Point(16, 8);
      this.groupBoxMapping.Name = "groupBoxMapping";
      this.groupBoxMapping.Size = new System.Drawing.Size(424, 331);
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
      this.panelTimeSpan.Location = new System.Drawing.Point(64, 298);
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
      // treeViewMpChannels
      // 
      this.treeViewMpChannels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.treeViewMpChannels.HideSelection = false;
      this.treeViewMpChannels.Location = new System.Drawing.Point(12, 48);
      this.treeViewMpChannels.Name = "treeViewMpChannels";
      this.treeViewMpChannels.ShowNodeToolTips = true;
      this.treeViewMpChannels.Size = new System.Drawing.Size(216, 244);
      this.treeViewMpChannels.Sorted = true;
      this.treeViewMpChannels.TabIndex = 1;
      this.treeViewMpChannels.DoubleClick += new System.EventHandler(this.treeViewChannels_DoubleClick);
      this.treeViewMpChannels.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewChannels_AfterSelect);
      // 
      // treeViewTvMStations
      // 
      this.treeViewTvMStations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.treeViewTvMStations.HideSelection = false;
      this.treeViewTvMStations.ImageIndex = 0;
      this.treeViewTvMStations.ImageList = this.imageListTvmStations;
      this.treeViewTvMStations.Indent = 35;
      this.treeViewTvMStations.ItemHeight = 24;
      this.treeViewTvMStations.Location = new System.Drawing.Point(236, 48);
      this.treeViewTvMStations.Name = "treeViewTvMStations";
      this.treeViewTvMStations.SelectedImageIndex = 0;
      this.treeViewTvMStations.ShowNodeToolTips = true;
      this.treeViewTvMStations.ShowPlusMinus = false;
      this.treeViewTvMStations.ShowRootLines = false;
      this.treeViewTvMStations.Size = new System.Drawing.Size(172, 270);
      this.treeViewTvMStations.Sorted = true;
      this.treeViewTvMStations.TabIndex = 3;
      this.treeViewTvMStations.DoubleClick += new System.EventHandler(this.treeViewStations_DoubleClick);
      // 
      // imageListTvmStations
      // 
      this.imageListTvmStations.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
      this.imageListTvmStations.ImageSize = new System.Drawing.Size(32, 22);
      this.imageListTvmStations.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // listView1
      // 
      this.listView1.AllowDrop = true;
      this.listView1.AllowRowReorder = true;
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listView1.Location = new System.Drawing.Point(12, 27);
      this.listView1.Name = "listView1";
      this.listView1.Scrollable = false;
      this.listView1.Size = new System.Drawing.Size(216, 24);
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "TV Service channels";
      this.columnHeader1.Width = 262;
      // 
      // listView2
      // 
      this.listView2.AllowDrop = true;
      this.listView2.AllowRowReorder = true;
      this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
      this.listView2.Location = new System.Drawing.Point(236, 27);
      this.listView2.Name = "listView2";
      this.listView2.Scrollable = false;
      this.listView2.Size = new System.Drawing.Size(172, 24);
      this.listView2.TabIndex = 2;
      this.listView2.UseCompatibleStateImageBehavior = false;
      this.listView2.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "TV Movie stations";
      this.columnHeader2.Width = 179;
      // 
      // fileDialogDb
      // 
      this.fileDialogDb.FileName = "TVDaten.mdb";
      this.fileDialogDb.RestoreDirectory = true;
      this.fileDialogDb.Title = "Please enter the path to TV movie\'s database";
      // 
      // numericUpDownActorCount
      // 
      this.numericUpDownActorCount.Location = new System.Drawing.Point(218, 63);
      this.numericUpDownActorCount.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
      this.numericUpDownActorCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownActorCount.Name = "numericUpDownActorCount";
      this.numericUpDownActorCount.Size = new System.Drawing.Size(37, 20);
      this.numericUpDownActorCount.TabIndex = 6;
      this.numericUpDownActorCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.numericUpDownActorCount.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
      // 
      // checkBoxShowLive
      // 
      this.checkBoxShowLive.AutoSize = true;
      this.checkBoxShowLive.Checked = true;
      this.checkBoxShowLive.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxShowLive.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxShowLive.Location = new System.Drawing.Point(16, 129);
      this.checkBoxShowLive.Name = "checkBoxShowLive";
      this.checkBoxShowLive.Size = new System.Drawing.Size(175, 17);
      this.checkBoxShowLive.TabIndex = 7;
      this.checkBoxShowLive.Text = "Append \"(LIVE)\" to program title";
      this.checkBoxShowLive.UseVisualStyleBackColor = true;
      // 
      // checkBoxShowRepeat
      // 
      this.checkBoxShowRepeat.AutoSize = true;
      this.checkBoxShowRepeat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxShowRepeat.Location = new System.Drawing.Point(16, 152);
      this.checkBoxShowRepeat.Name = "checkBoxShowRepeat";
      this.checkBoxShowRepeat.Size = new System.Drawing.Size(178, 17);
      this.checkBoxShowRepeat.TabIndex = 8;
      this.checkBoxShowRepeat.Text = "Append \"(Wdh.)\" to program title";
      this.checkBoxShowRepeat.UseVisualStyleBackColor = true;
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
      this.tabPageSettings.PerformLayout();
      this.groupBoxEnableTvMovie.ResumeLayout(false);
      this.groupBoxEnableTvMovie.PerformLayout();
      this.groupBoxInstallMethod.ResumeLayout(false);
      this.groupBoxInstallMethod.PerformLayout();
      this.groupBoxImportTime.ResumeLayout(false);
      this.groupBoxImportTime.PerformLayout();
      this.tabPageImportOptions.ResumeLayout(false);
      this.groupBoxDescriptions.ResumeLayout(false);
      this.groupBoxDescriptions.PerformLayout();
      this.tabPageMapChannels.ResumeLayout(false);
      this.tabPageMapChannels.PerformLayout();
      this.groupBoxMapping.ResumeLayout(false);
      this.groupBoxMapping.PerformLayout();
      this.panelTimeSpan.ResumeLayout(false);
      this.panelTimeSpan.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownActorCount)).EndInit();
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
    private System.Windows.Forms.TreeView treeViewMpChannels;
    private System.Windows.Forms.TreeView treeViewTvMStations;
    private MediaPortal.UserInterface.Controls.MPListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private MediaPortal.UserInterface.Controls.MPListView listView2;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.TabPage tabPageSettings;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxEnableImport;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxEnableTvMovie;
    private System.Windows.Forms.GroupBox groupBoxImportTime;
    private System.Windows.Forms.RadioButton radioButton24h;
    private System.Windows.Forms.RadioButton radioButton12h;
    private System.Windows.Forms.RadioButton radioButton6h;
    private System.Windows.Forms.RadioButton radioButton7d;
    private System.Windows.Forms.RadioButton radioButton2d;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxSlowImport;
    private System.Windows.Forms.LinkLabel linkLabelInfo;
    private System.Windows.Forms.ImageList imageListTvmStations;
    private System.Windows.Forms.ProgressBar progressBarImportTotal;
    private System.Windows.Forms.Button buttonImportNow;
    private System.Windows.Forms.TabPage tabPageImportOptions;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxDescriptions;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxLimitActors;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxShowRatings;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxAdditionalInfo;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxShowAudioFormat;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxUseShortDesc;
    private System.Windows.Forms.GroupBox groupBoxInstallMethod;
    private System.Windows.Forms.RadioButton rbManual;
    private System.Windows.Forms.RadioButton rbLocal;
    private System.Windows.Forms.TextBox tbDbPath;
    private System.Windows.Forms.Label lbDbPath;
    private System.Windows.Forms.Button buttonBrowse;
    private System.Windows.Forms.OpenFileDialog fileDialogDb;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxShowRepeat;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxShowLive;
    private System.Windows.Forms.NumericUpDown numericUpDownActorCount;
  }
}
