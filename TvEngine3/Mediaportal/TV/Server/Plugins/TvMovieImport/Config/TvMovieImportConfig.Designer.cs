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

using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.Plugins.TvMovieImport.Config
{
  partial class TvMovieImportConfig
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
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.tabPageMappings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBoxMapping = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelMappingChannelGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.comboBoxMappingsChannelGroup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPComboBox();
      this.checkBoxMappingsPartialMatch = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPCheckBox();
      this.textBoxMappingsAction = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.labelMappingProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelMappingAction = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.progressBarMappingsProgress = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPProgressBar();
      this.buttonMappingsSave = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.buttonMappingsLoad = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.dataGridViewMappings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridView();
      this.dataGridViewColumnId = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnTuningChannel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.dataGridViewColumnGuideChannel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewComboBoxColumn();
      this.dataGridViewColumnMatchType = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDataGridViewTextBoxColumn();
      this.tabPageSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabPage();
      this.groupBoxSettings = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelDatabaseFile = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.dateTimePickerUpdateTimeBetweenEnd = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDateTimePicker();
      this.buttonBrowse = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.textBoxDatabaseFile = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTextBox();
      this.buttonImport = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPButton();
      this.radioButtonUpdateTimeStartup = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.labelUpdateTimeBetween = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.radioButtonUpdateTimeBetween = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPRadioButton();
      this.labelUpdateTime = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.numericUpDownUpdateTimeFrequency = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPNumericUpDown();
      this.labelUpdateTimeHours = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.dateTimePickerUpdateTimeBetweenStart = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPDateTimePicker();
      this.groupBoxImportStatus = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPGroupBox();
      this.labelImportStatusValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelImportStatusProgramCountsValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelImportStatusChannelCountsValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelImportStatusDateTimeValue = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelImportStatusLabel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelImportStatusProgramCountsLabel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelImportStatusChannelCountsLabel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.labelImportStatusDateTimeLabel = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLabel();
      this.linkLabelInfo = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPLinkLabel();
      this.tabControl = new Mediaportal.TV.Server.SetupControls.UserInterfaceControls.MPTabControl();
      this.tabPageMappings.SuspendLayout();
      this.groupBoxMapping.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMappings)).BeginInit();
      this.tabPageSettings.SuspendLayout();
      this.groupBoxSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownUpdateTimeFrequency)).BeginInit();
      this.groupBoxImportStatus.SuspendLayout();
      this.tabControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // openFileDialog
      // 
      this.openFileDialog.FileName = "TVDaten.mdb";
      this.openFileDialog.Filter = "ClickFinder Databases (*.mdb)|*.mdb|All Files|*.*";
      this.openFileDialog.Title = "Select TV Movie ClickFinder\'s database.";
      // 
      // tabPageMappings
      // 
      this.tabPageMappings.Controls.Add(this.groupBoxMapping);
      this.tabPageMappings.Controls.Add(this.dataGridViewMappings);
      this.tabPageMappings.Location = new System.Drawing.Point(4, 22);
      this.tabPageMappings.Name = "tabPageMappings";
      this.tabPageMappings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageMappings.Size = new System.Drawing.Size(472, 394);
      this.tabPageMappings.TabIndex = 0;
      this.tabPageMappings.Text = "Mappings";
      // 
      // groupBoxMapping
      // 
      this.groupBoxMapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMapping.Controls.Add(this.labelMappingChannelGroup);
      this.groupBoxMapping.Controls.Add(this.comboBoxMappingsChannelGroup);
      this.groupBoxMapping.Controls.Add(this.checkBoxMappingsPartialMatch);
      this.groupBoxMapping.Controls.Add(this.textBoxMappingsAction);
      this.groupBoxMapping.Controls.Add(this.labelMappingProgress);
      this.groupBoxMapping.Controls.Add(this.labelMappingAction);
      this.groupBoxMapping.Controls.Add(this.progressBarMappingsProgress);
      this.groupBoxMapping.Controls.Add(this.buttonMappingsSave);
      this.groupBoxMapping.Controls.Add(this.buttonMappingsLoad);
      this.groupBoxMapping.Location = new System.Drawing.Point(6, 6);
      this.groupBoxMapping.Name = "groupBoxMapping";
      this.groupBoxMapping.Size = new System.Drawing.Size(460, 85);
      this.groupBoxMapping.TabIndex = 2;
      this.groupBoxMapping.TabStop = false;
      // 
      // labelMappingChannelGroup
      // 
      this.labelMappingChannelGroup.AutoSize = true;
      this.labelMappingChannelGroup.Location = new System.Drawing.Point(10, 14);
      this.labelMappingChannelGroup.Name = "labelMappingChannelGroup";
      this.labelMappingChannelGroup.Size = new System.Drawing.Size(39, 13);
      this.labelMappingChannelGroup.TabIndex = 0;
      this.labelMappingChannelGroup.Text = "Group:";
      // 
      // comboBoxMappingsChannelGroup
      // 
      this.comboBoxMappingsChannelGroup.FormattingEnabled = true;
      this.comboBoxMappingsChannelGroup.Location = new System.Drawing.Point(55, 11);
      this.comboBoxMappingsChannelGroup.Name = "comboBoxMappingsChannelGroup";
      this.comboBoxMappingsChannelGroup.Size = new System.Drawing.Size(153, 21);
      this.comboBoxMappingsChannelGroup.TabIndex = 1;
      // 
      // checkBoxMappingsPartialMatch
      // 
      this.checkBoxMappingsPartialMatch.AutoSize = true;
      this.checkBoxMappingsPartialMatch.Location = new System.Drawing.Point(13, 38);
      this.checkBoxMappingsPartialMatch.Name = "checkBoxMappingsPartialMatch";
      this.checkBoxMappingsPartialMatch.Size = new System.Drawing.Size(137, 17);
      this.checkBoxMappingsPartialMatch.TabIndex = 2;
      this.checkBoxMappingsPartialMatch.Text = "Enable partial matching.";
      // 
      // textBoxMappingsAction
      // 
      this.textBoxMappingsAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxMappingsAction.BackColor = System.Drawing.SystemColors.Control;
      this.textBoxMappingsAction.Location = new System.Drawing.Point(334, 40);
      this.textBoxMappingsAction.Name = "textBoxMappingsAction";
      this.textBoxMappingsAction.Size = new System.Drawing.Size(105, 20);
      this.textBoxMappingsAction.TabIndex = 6;
      // 
      // labelMappingProgress
      // 
      this.labelMappingProgress.AutoSize = true;
      this.labelMappingProgress.Location = new System.Drawing.Point(277, 63);
      this.labelMappingProgress.Name = "labelMappingProgress";
      this.labelMappingProgress.Size = new System.Drawing.Size(51, 13);
      this.labelMappingProgress.TabIndex = 7;
      this.labelMappingProgress.Text = "Progress:";
      // 
      // labelMappingAction
      // 
      this.labelMappingAction.AutoSize = true;
      this.labelMappingAction.Location = new System.Drawing.Point(279, 43);
      this.labelMappingAction.Name = "labelMappingAction";
      this.labelMappingAction.Size = new System.Drawing.Size(40, 13);
      this.labelMappingAction.TabIndex = 5;
      this.labelMappingAction.Text = "Action:";
      // 
      // progressBarMappingsProgress
      // 
      this.progressBarMappingsProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBarMappingsProgress.Location = new System.Drawing.Point(334, 66);
      this.progressBarMappingsProgress.Name = "progressBarMappingsProgress";
      this.progressBarMappingsProgress.Size = new System.Drawing.Size(105, 10);
      this.progressBarMappingsProgress.TabIndex = 8;
      // 
      // buttonMappingsSave
      // 
      this.buttonMappingsSave.Location = new System.Drawing.Point(79, 56);
      this.buttonMappingsSave.Name = "buttonMappingsSave";
      this.buttonMappingsSave.Size = new System.Drawing.Size(60, 23);
      this.buttonMappingsSave.TabIndex = 4;
      this.buttonMappingsSave.Text = "&Save";
      this.buttonMappingsSave.Click += new System.EventHandler(this.buttonMappingsSave_Click);
      // 
      // buttonMappingsLoad
      // 
      this.buttonMappingsLoad.Location = new System.Drawing.Point(13, 56);
      this.buttonMappingsLoad.Name = "buttonMappingsLoad";
      this.buttonMappingsLoad.Size = new System.Drawing.Size(60, 23);
      this.buttonMappingsLoad.TabIndex = 3;
      this.buttonMappingsLoad.Text = "&Load";
      this.buttonMappingsLoad.Click += new System.EventHandler(this.buttonMappingsLoad_Click);
      // 
      // dataGridViewMappings
      // 
      this.dataGridViewMappings.AllowUserToAddRows = false;
      this.dataGridViewMappings.AllowUserToDeleteRows = false;
      this.dataGridViewMappings.AllowUserToOrderColumns = true;
      this.dataGridViewMappings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGridViewMappings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dataGridViewMappings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewColumnId,
            this.dataGridViewColumnTuningChannel,
            this.dataGridViewColumnGuideChannel,
            this.dataGridViewColumnMatchType});
      this.dataGridViewMappings.Location = new System.Drawing.Point(6, 97);
      this.dataGridViewMappings.MultiSelect = false;
      this.dataGridViewMappings.Name = "dataGridViewMappings";
      dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.dataGridViewMappings.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
      this.dataGridViewMappings.RowHeadersVisible = false;
      this.dataGridViewMappings.Size = new System.Drawing.Size(460, 287);
      this.dataGridViewMappings.TabIndex = 3;
      // 
      // dataGridViewColumnId
      // 
      this.dataGridViewColumnId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.dataGridViewColumnId.HeaderText = "ID";
      this.dataGridViewColumnId.Name = "dataGridViewColumnId";
      this.dataGridViewColumnId.ReadOnly = true;
      this.dataGridViewColumnId.Width = 43;
      // 
      // dataGridViewColumnTuningChannel
      // 
      this.dataGridViewColumnTuningChannel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
      this.dataGridViewColumnTuningChannel.DefaultCellStyle = dataGridViewCellStyle4;
      this.dataGridViewColumnTuningChannel.HeaderText = "Tuning Channel";
      this.dataGridViewColumnTuningChannel.Name = "dataGridViewColumnTuningChannel";
      this.dataGridViewColumnTuningChannel.ReadOnly = true;
      this.dataGridViewColumnTuningChannel.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      // 
      // dataGridViewColumnGuideChannel
      // 
      this.dataGridViewColumnGuideChannel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
      this.dataGridViewColumnGuideChannel.DefaultCellStyle = dataGridViewCellStyle5;
      this.dataGridViewColumnGuideChannel.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
      this.dataGridViewColumnGuideChannel.HeaderText = "Guide Channel";
      this.dataGridViewColumnGuideChannel.Name = "dataGridViewColumnGuideChannel";
      this.dataGridViewColumnGuideChannel.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGridViewColumnGuideChannel.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      // 
      // dataGridViewColumnMatchType
      // 
      this.dataGridViewColumnMatchType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
      this.dataGridViewColumnMatchType.HeaderText = "*";
      this.dataGridViewColumnMatchType.Name = "dataGridViewColumnMatchType";
      this.dataGridViewColumnMatchType.ReadOnly = true;
      this.dataGridViewColumnMatchType.ToolTipText = "Type of match. White = already mapped, Green = exact, Yellow = partial, Red = non" +
          "e";
      this.dataGridViewColumnMatchType.Width = 36;
      // 
      // tabPageSettings
      // 
      this.tabPageSettings.Controls.Add(this.groupBoxSettings);
      this.tabPageSettings.Controls.Add(this.groupBoxImportStatus);
      this.tabPageSettings.Controls.Add(this.linkLabelInfo);
      this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageSettings.Name = "tabPageSettings";
      this.tabPageSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageSettings.Size = new System.Drawing.Size(472, 394);
      this.tabPageSettings.TabIndex = 1;
      this.tabPageSettings.Text = "Settings";
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxSettings.Controls.Add(this.labelDatabaseFile);
      this.groupBoxSettings.Controls.Add(this.dateTimePickerUpdateTimeBetweenEnd);
      this.groupBoxSettings.Controls.Add(this.buttonBrowse);
      this.groupBoxSettings.Controls.Add(this.textBoxDatabaseFile);
      this.groupBoxSettings.Controls.Add(this.buttonImport);
      this.groupBoxSettings.Controls.Add(this.radioButtonUpdateTimeStartup);
      this.groupBoxSettings.Controls.Add(this.labelUpdateTimeBetween);
      this.groupBoxSettings.Controls.Add(this.radioButtonUpdateTimeBetween);
      this.groupBoxSettings.Controls.Add(this.labelUpdateTime);
      this.groupBoxSettings.Controls.Add(this.numericUpDownUpdateTimeFrequency);
      this.groupBoxSettings.Controls.Add(this.labelUpdateTimeHours);
      this.groupBoxSettings.Controls.Add(this.dateTimePickerUpdateTimeBetweenStart);
      this.groupBoxSettings.Location = new System.Drawing.Point(6, 38);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(460, 181);
      this.groupBoxSettings.TabIndex = 1;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // labelDatabaseFile
      // 
      this.labelDatabaseFile.AutoSize = true;
      this.labelDatabaseFile.Location = new System.Drawing.Point(3, 16);
      this.labelDatabaseFile.Name = "labelDatabaseFile";
      this.labelDatabaseFile.Size = new System.Drawing.Size(109, 13);
      this.labelDatabaseFile.TabIndex = 0;
      this.labelDatabaseFile.Text = "ClickFinder database:";
      // 
      // dateTimePickerUpdateTimeBetweenEnd
      // 
      this.dateTimePickerUpdateTimeBetweenEnd.Format = System.Windows.Forms.DateTimePickerFormat.Time;
      this.dateTimePickerUpdateTimeBetweenEnd.Location = new System.Drawing.Point(199, 91);
      this.dateTimePickerUpdateTimeBetweenEnd.Name = "dateTimePickerUpdateTimeBetweenEnd";
      this.dateTimePickerUpdateTimeBetweenEnd.ShowUpDown = true;
      this.dateTimePickerUpdateTimeBetweenEnd.Size = new System.Drawing.Size(95, 20);
      this.dateTimePickerUpdateTimeBetweenEnd.TabIndex = 9;
      this.dateTimePickerUpdateTimeBetweenEnd.Value = new System.DateTime(2014, 8, 16, 18, 0, 0, 0);
      this.dateTimePickerUpdateTimeBetweenEnd.ValueChanged += new System.EventHandler(this.dateTimePickerUpdateTimeBetweenEnd_ValueChanged);
      // 
      // buttonBrowse
      // 
      this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonBrowse.Location = new System.Drawing.Point(430, 30);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new System.Drawing.Size(24, 23);
      this.buttonBrowse.TabIndex = 2;
      this.buttonBrowse.Text = "...";
      this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
      // 
      // textBoxDatabaseFile
      // 
      this.textBoxDatabaseFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxDatabaseFile.Location = new System.Drawing.Point(6, 32);
      this.textBoxDatabaseFile.Name = "textBoxDatabaseFile";
      this.textBoxDatabaseFile.Size = new System.Drawing.Size(418, 20);
      this.textBoxDatabaseFile.TabIndex = 1;
      // 
      // buttonImport
      // 
      this.buttonImport.Location = new System.Drawing.Point(6, 146);
      this.buttonImport.Name = "buttonImport";
      this.buttonImport.Size = new System.Drawing.Size(75, 23);
      this.buttonImport.TabIndex = 11;
      this.buttonImport.Text = "&Import Now";
      this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
      // 
      // radioButtonUpdateTimeStartup
      // 
      this.radioButtonUpdateTimeStartup.AutoSize = true;
      this.radioButtonUpdateTimeStartup.Location = new System.Drawing.Point(9, 117);
      this.radioButtonUpdateTimeStartup.Name = "radioButtonUpdateTimeStartup";
      this.radioButtonUpdateTimeStartup.Size = new System.Drawing.Size(210, 17);
      this.radioButtonUpdateTimeStartup.TabIndex = 10;
      this.radioButtonUpdateTimeStartup.Text = "When the TV service starts or resumes.";
      // 
      // labelUpdateTimeBetween
      // 
      this.labelUpdateTimeBetween.AutoSize = true;
      this.labelUpdateTimeBetween.Location = new System.Drawing.Point(172, 95);
      this.labelUpdateTimeBetween.Name = "labelUpdateTimeBetween";
      this.labelUpdateTimeBetween.Size = new System.Drawing.Size(25, 13);
      this.labelUpdateTimeBetween.TabIndex = 8;
      this.labelUpdateTimeBetween.Text = "and";
      // 
      // radioButtonUpdateTimeBetween
      // 
      this.radioButtonUpdateTimeBetween.AutoSize = true;
      this.radioButtonUpdateTimeBetween.Checked = true;
      this.radioButtonUpdateTimeBetween.Location = new System.Drawing.Point(9, 93);
      this.radioButtonUpdateTimeBetween.Name = "radioButtonUpdateTimeBetween";
      this.radioButtonUpdateTimeBetween.Size = new System.Drawing.Size(66, 17);
      this.radioButtonUpdateTimeBetween.TabIndex = 6;
      this.radioButtonUpdateTimeBetween.TabStop = true;
      this.radioButtonUpdateTimeBetween.Text = "Between";
      this.radioButtonUpdateTimeBetween.CheckedChanged += new System.EventHandler(this.radioUpdateTimeBetween_CheckedChanged);
      // 
      // labelUpdateTime
      // 
      this.labelUpdateTime.AutoSize = true;
      this.labelUpdateTime.Location = new System.Drawing.Point(3, 65);
      this.labelUpdateTime.Name = "labelUpdateTime";
      this.labelUpdateTime.Size = new System.Drawing.Size(169, 13);
      this.labelUpdateTime.TabIndex = 3;
      this.labelUpdateTime.Text = "Update the guide data once every";
      // 
      // numericUpDownUpdateTimeFrequency
      // 
      this.numericUpDownUpdateTimeFrequency.Location = new System.Drawing.Point(174, 63);
      this.numericUpDownUpdateTimeFrequency.Maximum = new decimal(new int[] {
            672,
            0,
            0,
            0});
      this.numericUpDownUpdateTimeFrequency.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownUpdateTimeFrequency.Name = "numericUpDownUpdateTimeFrequency";
      this.numericUpDownUpdateTimeFrequency.Size = new System.Drawing.Size(40, 20);
      this.numericUpDownUpdateTimeFrequency.TabIndex = 4;
      this.numericUpDownUpdateTimeFrequency.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
      // 
      // labelUpdateTimeHours
      // 
      this.labelUpdateTimeHours.AutoSize = true;
      this.labelUpdateTimeHours.Location = new System.Drawing.Point(215, 65);
      this.labelUpdateTimeHours.Name = "labelUpdateTimeHours";
      this.labelUpdateTimeHours.Size = new System.Drawing.Size(42, 13);
      this.labelUpdateTimeHours.TabIndex = 5;
      this.labelUpdateTimeHours.Text = "hour(s):";
      // 
      // dateTimePickerUpdateTimeBetweenStart
      // 
      this.dateTimePickerUpdateTimeBetweenStart.Format = System.Windows.Forms.DateTimePickerFormat.Time;
      this.dateTimePickerUpdateTimeBetweenStart.Location = new System.Drawing.Point(76, 91);
      this.dateTimePickerUpdateTimeBetweenStart.Name = "dateTimePickerUpdateTimeBetweenStart";
      this.dateTimePickerUpdateTimeBetweenStart.ShowUpDown = true;
      this.dateTimePickerUpdateTimeBetweenStart.Size = new System.Drawing.Size(95, 20);
      this.dateTimePickerUpdateTimeBetweenStart.TabIndex = 7;
      this.dateTimePickerUpdateTimeBetweenStart.Value = new System.DateTime(2014, 8, 15, 17, 0, 0, 0);
      this.dateTimePickerUpdateTimeBetweenStart.ValueChanged += new System.EventHandler(this.dateTimePickerUpdateTimeBetweenStart_ValueChanged);
      // 
      // groupBoxImportStatus
      // 
      this.groupBoxImportStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxImportStatus.Controls.Add(this.labelImportStatusValue);
      this.groupBoxImportStatus.Controls.Add(this.labelImportStatusProgramCountsValue);
      this.groupBoxImportStatus.Controls.Add(this.labelImportStatusChannelCountsValue);
      this.groupBoxImportStatus.Controls.Add(this.labelImportStatusDateTimeValue);
      this.groupBoxImportStatus.Controls.Add(this.labelImportStatusLabel);
      this.groupBoxImportStatus.Controls.Add(this.labelImportStatusProgramCountsLabel);
      this.groupBoxImportStatus.Controls.Add(this.labelImportStatusChannelCountsLabel);
      this.groupBoxImportStatus.Controls.Add(this.labelImportStatusDateTimeLabel);
      this.groupBoxImportStatus.Location = new System.Drawing.Point(6, 269);
      this.groupBoxImportStatus.Name = "groupBoxImportStatus";
      this.groupBoxImportStatus.Size = new System.Drawing.Size(460, 119);
      this.groupBoxImportStatus.TabIndex = 2;
      this.groupBoxImportStatus.TabStop = false;
      this.groupBoxImportStatus.Text = "Import Status";
      // 
      // labelImportStatusValue
      // 
      this.labelImportStatusValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelImportStatusValue.Location = new System.Drawing.Point(148, 48);
      this.labelImportStatusValue.Name = "labelImportStatusValue";
      this.labelImportStatusValue.Size = new System.Drawing.Size(303, 13);
      this.labelImportStatusValue.TabIndex = 3;
      this.labelImportStatusValue.Text = "(import status)";
      // 
      // labelImportStatusProgramCountsValue
      // 
      this.labelImportStatusProgramCountsValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelImportStatusProgramCountsValue.Location = new System.Drawing.Point(148, 93);
      this.labelImportStatusProgramCountsValue.Name = "labelImportStatusProgramCountsValue";
      this.labelImportStatusProgramCountsValue.Size = new System.Drawing.Size(303, 13);
      this.labelImportStatusProgramCountsValue.TabIndex = 7;
      this.labelImportStatusProgramCountsValue.Text = "(import program counts)";
      // 
      // labelImportStatusChannelCountsValue
      // 
      this.labelImportStatusChannelCountsValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.labelImportStatusChannelCountsValue.Location = new System.Drawing.Point(148, 70);
      this.labelImportStatusChannelCountsValue.Name = "labelImportStatusChannelCountsValue";
      this.labelImportStatusChannelCountsValue.Size = new System.Drawing.Size(303, 13);
      this.labelImportStatusChannelCountsValue.TabIndex = 5;
      this.labelImportStatusChannelCountsValue.Text = "(import channel counts)";
      // 
      // labelImportStatusDateTimeValue
      // 
      this.labelImportStatusDateTimeValue.AutoSize = true;
      this.labelImportStatusDateTimeValue.Location = new System.Drawing.Point(148, 26);
      this.labelImportStatusDateTimeValue.Name = "labelImportStatusDateTimeValue";
      this.labelImportStatusDateTimeValue.Size = new System.Drawing.Size(89, 13);
      this.labelImportStatusDateTimeValue.TabIndex = 1;
      this.labelImportStatusDateTimeValue.Text = "(import date/time)";
      // 
      // labelImportStatusLabel
      // 
      this.labelImportStatusLabel.AutoSize = true;
      this.labelImportStatusLabel.Location = new System.Drawing.Point(3, 48);
      this.labelImportStatusLabel.Name = "labelImportStatusLabel";
      this.labelImportStatusLabel.Size = new System.Drawing.Size(70, 13);
      this.labelImportStatusLabel.TabIndex = 2;
      this.labelImportStatusLabel.Text = "Status/result:";
      // 
      // labelImportStatusProgramCountsLabel
      // 
      this.labelImportStatusProgramCountsLabel.AutoSize = true;
      this.labelImportStatusProgramCountsLabel.Location = new System.Drawing.Point(3, 93);
      this.labelImportStatusProgramCountsLabel.Name = "labelImportStatusProgramCountsLabel";
      this.labelImportStatusProgramCountsLabel.Size = new System.Drawing.Size(110, 13);
      this.labelImportStatusProgramCountsLabel.TabIndex = 6;
      this.labelImportStatusProgramCountsLabel.Text = "Total program counts:";
      // 
      // labelImportStatusChannelCountsLabel
      // 
      this.labelImportStatusChannelCountsLabel.AutoSize = true;
      this.labelImportStatusChannelCountsLabel.Location = new System.Drawing.Point(3, 70);
      this.labelImportStatusChannelCountsLabel.Name = "labelImportStatusChannelCountsLabel";
      this.labelImportStatusChannelCountsLabel.Size = new System.Drawing.Size(110, 13);
      this.labelImportStatusChannelCountsLabel.TabIndex = 4;
      this.labelImportStatusChannelCountsLabel.Text = "Total channel counts:";
      // 
      // labelImportStatusDateTimeLabel
      // 
      this.labelImportStatusDateTimeLabel.AutoSize = true;
      this.labelImportStatusDateTimeLabel.Location = new System.Drawing.Point(3, 26);
      this.labelImportStatusDateTimeLabel.Name = "labelImportStatusDateTimeLabel";
      this.labelImportStatusDateTimeLabel.Size = new System.Drawing.Size(57, 13);
      this.labelImportStatusDateTimeLabel.TabIndex = 0;
      this.labelImportStatusDateTimeLabel.Text = "Date/time:";
      // 
      // linkLabelInfo
      // 
      this.linkLabelInfo.AutoSize = true;
      this.linkLabelInfo.LinkArea = new System.Windows.Forms.LinkArea(0, 20);
      this.linkLabelInfo.Location = new System.Drawing.Point(6, 9);
      this.linkLabelInfo.Name = "linkLabelInfo";
      this.linkLabelInfo.Size = new System.Drawing.Size(352, 17);
      this.linkLabelInfo.TabIndex = 0;
      this.linkLabelInfo.TabStop = true;
      this.linkLabelInfo.Text = "TV Movie ClickFinder is an EPG application for German TV channels.";
      this.linkLabelInfo.UseCompatibleTextRendering = true;
      this.linkLabelInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelInfo_LinkClicked);
      // 
      // tabControl
      // 
      this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl.Controls.Add(this.tabPageSettings);
      this.tabControl.Controls.Add(this.tabPageMappings);
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(480, 420);
      this.tabControl.TabIndex = 0;
      // 
      // TvMovieImportConfig
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl);
      this.Name = "TvMovieImportConfig";
      this.Size = new System.Drawing.Size(480, 420);
      this.tabPageMappings.ResumeLayout(false);
      this.groupBoxMapping.ResumeLayout(false);
      this.groupBoxMapping.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMappings)).EndInit();
      this.tabPageSettings.ResumeLayout(false);
      this.tabPageSettings.PerformLayout();
      this.groupBoxSettings.ResumeLayout(false);
      this.groupBoxSettings.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownUpdateTimeFrequency)).EndInit();
      this.groupBoxImportStatus.ResumeLayout(false);
      this.groupBoxImportStatus.PerformLayout();
      this.tabControl.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private MPTabPage tabPageMappings;
    private MPTabPage tabPageSettings;
    private MPButton buttonBrowse;
    private MPLabel labelDatabaseFile;
    private MPTextBox textBoxDatabaseFile;
    private MPLinkLabel linkLabelInfo;
    private MPTabControl tabControl;
    private MPGroupBox groupBoxMapping;
    private MPLabel labelMappingChannelGroup;
    private MPComboBox comboBoxMappingsChannelGroup;
    private MPCheckBox checkBoxMappingsPartialMatch;
    private MPTextBox textBoxMappingsAction;
    private MPLabel labelMappingProgress;
    private MPLabel labelMappingAction;
    private MPProgressBar progressBarMappingsProgress;
    private MPButton buttonMappingsSave;
    private MPButton buttonMappingsLoad;
    private MPDataGridView dataGridViewMappings;
    private MPDataGridViewTextBoxColumn dataGridViewColumnId;
    private MPDataGridViewTextBoxColumn dataGridViewColumnTuningChannel;
    private MPDataGridViewComboBoxColumn dataGridViewColumnGuideChannel;
    private MPDataGridViewTextBoxColumn dataGridViewColumnMatchType;
    private MPLabel labelUpdateTimeHours;
    private MPNumericUpDown numericUpDownUpdateTimeFrequency;
    private MPLabel labelUpdateTimeBetween;
    private MPDateTimePicker dateTimePickerUpdateTimeBetweenEnd;
    private MPRadioButton radioButtonUpdateTimeStartup;
    private MPRadioButton radioButtonUpdateTimeBetween;
    private MPDateTimePicker dateTimePickerUpdateTimeBetweenStart;
    private MPLabel labelUpdateTime;
    private MPButton buttonImport;
    private MPGroupBox groupBoxSettings;
    private MPGroupBox groupBoxImportStatus;
    private MPLabel labelImportStatusValue;
    private MPLabel labelImportStatusProgramCountsValue;
    private MPLabel labelImportStatusChannelCountsValue;
    private MPLabel labelImportStatusDateTimeValue;
    private MPLabel labelImportStatusLabel;
    private MPLabel labelImportStatusProgramCountsLabel;
    private MPLabel labelImportStatusChannelCountsLabel;
    private MPLabel labelImportStatusDateTimeLabel;
  }
}
