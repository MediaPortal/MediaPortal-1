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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using MediaPortal.TV.Database;
using MediaPortal.Util;

#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{
  public class TVProgramGuide : MediaPortal.Configuration.SectionSettings
  {
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox3;
    private MediaPortal.UserInterface.Controls.MPCheckBox useColorCheckBox;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private MediaPortal.UserInterface.Controls.MPLabel label3;
    private MediaPortal.UserInterface.Controls.MPComboBox GrabbercomboBox;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPTextBox AdvancedDaystextBox;
    private MediaPortal.UserInterface.Controls.MPButton parametersButton;
    private MediaPortal.UserInterface.Controls.MPTextBox parametersTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label7;
    private MediaPortal.UserInterface.Controls.MPTextBox daysToKeepTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label6;
    private MediaPortal.UserInterface.Controls.MPButton browseButton;
    private MediaPortal.UserInterface.Controls.MPTextBox folderNameTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel folderNameLabel;
    private MediaPortal.UserInterface.Controls.MPTextBox compensateTextBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox useTimeZoneCheckBox;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    protected MediaPortal.UserInterface.Controls.MPRadioButton advancedRadioButton;
    protected MediaPortal.UserInterface.Controls.MPRadioButton basicRadioButton;
    private MediaPortal.UserInterface.Controls.MPCheckBox createScheduleCheckBox;
    private MediaPortal.UserInterface.Controls.MPLabel label9;
    private MediaPortal.UserInterface.Controls.MPLabel label10;
    private MediaPortal.UserInterface.Controls.MPTextBox hoursTextBox;
    private MediaPortal.UserInterface.Controls.MPTextBox dayIntervalTextBox;
    private MediaPortal.UserInterface.Controls.MPTextBox minutesTextBox;
    private MediaPortal.UserInterface.Controls.MPButton RunGrabberButton;
    private MediaPortal.UserInterface.Controls.MPTextBox UserTextBox;
    private MediaPortal.UserInterface.Controls.MPTextBox PasswordTextBox;
    private MediaPortal.UserInterface.Controls.MPLabel label11;
    private MediaPortal.UserInterface.Controls.MPLabel label12;
    private MediaPortal.UserInterface.Controls.MPButton DeleteTaskButton;
    private MediaPortal.UserInterface.Controls.MPLabel label13;
    private MediaPortal.UserInterface.Controls.MPButton btnUpdateTvGuide;
    private System.ComponentModel.IContainer components = null;
    bool OldTimeZoneCompensation = false;
    private MediaPortal.UserInterface.Controls.MPButton btnClearTVDatabase;
    private MediaPortal.UserInterface.Controls.MPTextBox textBoxMinutes;
    private MediaPortal.UserInterface.Controls.MPLabel label2;
    private MediaPortal.UserInterface.Controls.MPLabel label15;
    int OldTimeZoneOffsetHours = 0;
    private MediaPortal.UserInterface.Controls.MPTabControl tabControl1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage1;
    private MediaPortal.UserInterface.Controls.MPTabPage tabPage2;
    private MediaPortal.UserInterface.Controls.MPLabel label14;
    private MediaPortal.UserInterface.Controls.MPLabel label16;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPButton button3;
    private MediaPortal.UserInterface.Controls.MPButton button2;
    private System.Windows.Forms.TreeView treeView1;
    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox4;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox5;
    private MediaPortal.UserInterface.Controls.MPCheckBox runGrabberLowPriorityCheckBox;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbGrabDVBEPG;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel1;
    int OldTimeZoneOffsetMins = 0;

    public TVProgramGuide()
      : this("Program Guide")
    {
    }

    public TVProgramGuide(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      // Disable if TVE3
      if (System.IO.File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        this.Enabled = false;
      }
      //
      // Setup grabbers
      //
      SetupGrabbers();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.useColorCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxMinutes = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.btnClearTVDatabase = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnUpdateTvGuide = new MediaPortal.UserInterface.Controls.MPButton();
      this.RunGrabberButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.advancedRadioButton = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.compensateTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.useTimeZoneCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.browseButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.folderNameTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.folderNameLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.daysToKeepTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.parametersButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.parametersTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.AdvancedDaystextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.GrabbercomboBox = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.basicRadioButton = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.label10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.dayIntervalTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.minutesTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.hoursTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.createScheduleCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.UserTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.PasswordTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.DeleteTaskButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.label13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbGrabDVBEPG = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.button3 = new MediaPortal.UserInterface.Controls.MPButton();
      this.button2 = new MediaPortal.UserInterface.Controls.MPButton();
      this.treeView1 = new System.Windows.Forms.TreeView();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label16 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.runGrabberLowPriorityCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.label14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBox3.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.SuspendLayout();
      // 
      // useColorCheckBox
      // 
      this.useColorCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.useColorCheckBox.AutoSize = true;
      this.useColorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useColorCheckBox.Location = new System.Drawing.Point(16, 23);
      this.useColorCheckBox.Name = "useColorCheckBox";
      this.useColorCheckBox.Size = new System.Drawing.Size(199, 17);
      this.useColorCheckBox.TabIndex = 0;
      this.useColorCheckBox.Text = "Use colors for genres in the TV guide";
      this.useColorCheckBox.UseVisualStyleBackColor = true;
      // 
      // label15
      // 
      this.label15.Location = new System.Drawing.Point(232, 88);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(40, 16);
      this.label15.TabIndex = 8;
      this.label15.Text = "Hours";
      this.label15.Click += new System.EventHandler(this.label15_Click);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(160, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(8, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = ":";
      // 
      // textBoxMinutes
      // 
      this.textBoxMinutes.Location = new System.Drawing.Point(200, 84);
      this.textBoxMinutes.Name = "textBoxMinutes";
      this.textBoxMinutes.Size = new System.Drawing.Size(32, 20);
      this.textBoxMinutes.TabIndex = 7;
      this.textBoxMinutes.Text = "0";
      // 
      // btnClearTVDatabase
      // 
      this.btnClearTVDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClearTVDatabase.Location = new System.Drawing.Point(272, 12);
      this.btnClearTVDatabase.Name = "btnClearTVDatabase";
      this.btnClearTVDatabase.Size = new System.Drawing.Size(144, 40);
      this.btnClearTVDatabase.TabIndex = 1;
      this.btnClearTVDatabase.Text = "Remove all programs from TV database";
      this.btnClearTVDatabase.UseVisualStyleBackColor = true;
      this.btnClearTVDatabase.Click += new System.EventHandler(this.btnClearTVDatabase_Click);
      // 
      // btnUpdateTvGuide
      // 
      this.btnUpdateTvGuide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnUpdateTvGuide.Location = new System.Drawing.Point(272, 76);
      this.btnUpdateTvGuide.Name = "btnUpdateTvGuide";
      this.btnUpdateTvGuide.Size = new System.Drawing.Size(144, 36);
      this.btnUpdateTvGuide.TabIndex = 9;
      this.btnUpdateTvGuide.Text = "Update TV database with time zone compensation";
      this.btnUpdateTvGuide.UseVisualStyleBackColor = true;
      this.btnUpdateTvGuide.Click += new System.EventHandler(this.btnUpdateTvGuide_Click);
      // 
      // RunGrabberButton
      // 
      this.RunGrabberButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.RunGrabberButton.Location = new System.Drawing.Point(320, 192);
      this.RunGrabberButton.Name = "RunGrabberButton";
      this.RunGrabberButton.Size = new System.Drawing.Size(96, 22);
      this.RunGrabberButton.TabIndex = 11;
      this.RunGrabberButton.Text = "Run Grabber";
      this.RunGrabberButton.UseVisualStyleBackColor = true;
      this.RunGrabberButton.Click += new System.EventHandler(this.RunGrabberButton_Click);
      // 
      // advancedRadioButton
      // 
      this.advancedRadioButton.AutoSize = true;
      this.advancedRadioButton.Enabled = false;
      this.advancedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.advancedRadioButton.Location = new System.Drawing.Point(16, 100);
      this.advancedRadioButton.Name = "advancedRadioButton";
      this.advancedRadioButton.Size = new System.Drawing.Size(158, 17);
      this.advancedRadioButton.TabIndex = 6;
      this.advancedRadioButton.Text = "Advanced Single Day Grabs";
      this.advancedRadioButton.UseVisualStyleBackColor = true;
      this.advancedRadioButton.CheckedChanged += new System.EventHandler(this.advancedRadioButton_CheckedChanged);
      // 
      // compensateTextBox
      // 
      this.compensateTextBox.Location = new System.Drawing.Point(168, 84);
      this.compensateTextBox.MaxLength = 3;
      this.compensateTextBox.Name = "compensateTextBox";
      this.compensateTextBox.Size = new System.Drawing.Size(24, 20);
      this.compensateTextBox.TabIndex = 5;
      this.compensateTextBox.Text = "0";
      // 
      // useTimeZoneCheckBox
      // 
      this.useTimeZoneCheckBox.AutoSize = true;
      this.useTimeZoneCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.useTimeZoneCheckBox.Location = new System.Drawing.Point(16, 56);
      this.useTimeZoneCheckBox.Name = "useTimeZoneCheckBox";
      this.useTimeZoneCheckBox.Size = new System.Drawing.Size(223, 17);
      this.useTimeZoneCheckBox.TabIndex = 3;
      this.useTimeZoneCheckBox.Text = "Use time zone information from XMLTV file";
      this.useTimeZoneCheckBox.UseVisualStyleBackColor = true;
      this.useTimeZoneCheckBox.CheckedChanged += new System.EventHandler(this.useTimeZoneCheckBox_CheckedChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 88);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(152, 16);
      this.label1.TabIndex = 4;
      this.label1.Text = "Compensate time zone with:";
      // 
      // browseButton
      // 
      this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.browseButton.Location = new System.Drawing.Point(344, 19);
      this.browseButton.Name = "browseButton";
      this.browseButton.Size = new System.Drawing.Size(72, 22);
      this.browseButton.TabIndex = 2;
      this.browseButton.Text = "Browse";
      this.browseButton.UseVisualStyleBackColor = true;
      this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
      // 
      // folderNameTextBox
      // 
      this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.folderNameTextBox.Location = new System.Drawing.Point(128, 20);
      this.folderNameTextBox.Name = "folderNameTextBox";
      this.folderNameTextBox.Size = new System.Drawing.Size(208, 20);
      this.folderNameTextBox.TabIndex = 1;
      // 
      // folderNameLabel
      // 
      this.folderNameLabel.Location = new System.Drawing.Point(16, 24);
      this.folderNameLabel.Name = "folderNameLabel";
      this.folderNameLabel.Size = new System.Drawing.Size(104, 16);
      this.folderNameLabel.TabIndex = 0;
      this.folderNameLabel.Text = "Path to tvguide.xml:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 156);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(104, 16);
      this.label6.TabIndex = 9;
      this.label6.Text = "Days to download:";
      // 
      // daysToKeepTextBox
      // 
      this.daysToKeepTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.daysToKeepTextBox.Enabled = false;
      this.daysToKeepTextBox.Location = new System.Drawing.Point(128, 132);
      this.daysToKeepTextBox.MaxLength = 3;
      this.daysToKeepTextBox.Name = "daysToKeepTextBox";
      this.daysToKeepTextBox.Size = new System.Drawing.Size(288, 20);
      this.daysToKeepTextBox.TabIndex = 8;
      this.daysToKeepTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.daysToKeepTextBox_KeyPress);
      // 
      // parametersButton
      // 
      this.parametersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersButton.Enabled = false;
      this.parametersButton.Location = new System.Drawing.Point(344, 43);
      this.parametersButton.Name = "parametersButton";
      this.parametersButton.Size = new System.Drawing.Size(72, 22);
      this.parametersButton.TabIndex = 4;
      this.parametersButton.Text = "List";
      this.parametersButton.UseVisualStyleBackColor = true;
      this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.Enabled = false;
      this.parametersTextBox.Location = new System.Drawing.Point(128, 44);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(208, 20);
      this.parametersTextBox.TabIndex = 3;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(-112, 80);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(80, 16);
      this.label7.TabIndex = 37;
      this.label7.Text = "Parameters";
      // 
      // AdvancedDaystextBox
      // 
      this.AdvancedDaystextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.AdvancedDaystextBox.Enabled = false;
      this.AdvancedDaystextBox.Location = new System.Drawing.Point(128, 156);
      this.AdvancedDaystextBox.Name = "AdvancedDaystextBox";
      this.AdvancedDaystextBox.Size = new System.Drawing.Size(288, 20);
      this.AdvancedDaystextBox.TabIndex = 10;
      this.AdvancedDaystextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AdvancedDaystextBox_KeyPress);
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 132);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(120, 16);
      this.label4.TabIndex = 7;
      this.label4.Text = "Days to keep in guide:";
      // 
      // GrabbercomboBox
      // 
      this.GrabbercomboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.GrabbercomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.GrabbercomboBox.Location = new System.Drawing.Point(128, 20);
      this.GrabbercomboBox.Name = "GrabbercomboBox";
      this.GrabbercomboBox.Size = new System.Drawing.Size(288, 21);
      this.GrabbercomboBox.TabIndex = 1;
      this.GrabbercomboBox.SelectedIndexChanged += new System.EventHandler(this.GrabbercomboBox_SelectedIndexChanged);
      this.GrabbercomboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.GrabbercomboBox_KeyPress);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 24);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(64, 16);
      this.label3.TabIndex = 0;
      this.label3.Text = "Grabber:";
      // 
      // basicRadioButton
      // 
      this.basicRadioButton.AutoSize = true;
      this.basicRadioButton.Enabled = false;
      this.basicRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.basicRadioButton.Location = new System.Drawing.Point(16, 76);
      this.basicRadioButton.Name = "basicRadioButton";
      this.basicRadioButton.Size = new System.Drawing.Size(118, 17);
      this.basicRadioButton.TabIndex = 5;
      this.basicRadioButton.Text = "Basic Multiday Grab";
      this.basicRadioButton.UseVisualStyleBackColor = true;
      this.basicRadioButton.CheckedChanged += new System.EventHandler(this.basicRadioButton_CheckedChanged);
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(280, 24);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(40, 16);
      this.label10.TabIndex = 6;
      this.label10.Text = "day(s)";
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(208, 24);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(34, 16);
      this.label9.TabIndex = 4;
      this.label9.Text = "Every";
      // 
      // dayIntervalTextBox
      // 
      this.dayIntervalTextBox.Location = new System.Drawing.Point(248, 21);
      this.dayIntervalTextBox.Name = "dayIntervalTextBox";
      this.dayIntervalTextBox.Size = new System.Drawing.Size(32, 20);
      this.dayIntervalTextBox.TabIndex = 5;
      this.dayIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.dayIntervalTextBox_KeyPress);
      // 
      // minutesTextBox
      // 
      this.minutesTextBox.Location = new System.Drawing.Point(168, 21);
      this.minutesTextBox.Name = "minutesTextBox";
      this.minutesTextBox.Size = new System.Drawing.Size(32, 20);
      this.minutesTextBox.TabIndex = 3;
      this.minutesTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.minutesTextBox_KeyPress);
      // 
      // hoursTextBox
      // 
      this.hoursTextBox.Location = new System.Drawing.Point(128, 21);
      this.hoursTextBox.Name = "hoursTextBox";
      this.hoursTextBox.Size = new System.Drawing.Size(32, 20);
      this.hoursTextBox.TabIndex = 1;
      this.hoursTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.hoursTextBox_KeyPress);
      // 
      // createScheduleCheckBox
      // 
      this.createScheduleCheckBox.AutoSize = true;
      this.createScheduleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.createScheduleCheckBox.Location = new System.Drawing.Point(16, 23);
      this.createScheduleCheckBox.Name = "createScheduleCheckBox";
      this.createScheduleCheckBox.Size = new System.Drawing.Size(97, 17);
      this.createScheduleCheckBox.TabIndex = 0;
      this.createScheduleCheckBox.Text = "Create Task at:";
      this.createScheduleCheckBox.UseVisualStyleBackColor = true;
      this.createScheduleCheckBox.CheckedChanged += new System.EventHandler(this.createScheduleCheckBox_CheckedChanged);
      // 
      // UserTextBox
      // 
      this.UserTextBox.Location = new System.Drawing.Point(96, 52);
      this.UserTextBox.Name = "UserTextBox";
      this.UserTextBox.Size = new System.Drawing.Size(120, 20);
      this.UserTextBox.TabIndex = 8;
      // 
      // PasswordTextBox
      // 
      this.PasswordTextBox.Location = new System.Drawing.Point(296, 52);
      this.PasswordTextBox.Name = "PasswordTextBox";
      this.PasswordTextBox.Size = new System.Drawing.Size(120, 20);
      this.PasswordTextBox.TabIndex = 10;
      // 
      // label11
      // 
      this.label11.Location = new System.Drawing.Point(16, 56);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(80, 16);
      this.label11.TabIndex = 7;
      this.label11.Text = "User account:";
      // 
      // label12
      // 
      this.label12.Location = new System.Drawing.Point(232, 56);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(64, 16);
      this.label12.TabIndex = 9;
      this.label12.Text = "Password:";
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.createScheduleCheckBox);
      this.groupBox3.Controls.Add(this.label10);
      this.groupBox3.Controls.Add(this.label9);
      this.groupBox3.Controls.Add(this.dayIntervalTextBox);
      this.groupBox3.Controls.Add(this.minutesTextBox);
      this.groupBox3.Controls.Add(this.hoursTextBox);
      this.groupBox3.Controls.Add(this.label11);
      this.groupBox3.Controls.Add(this.label12);
      this.groupBox3.Controls.Add(this.PasswordTextBox);
      this.groupBox3.Controls.Add(this.UserTextBox);
      this.groupBox3.Controls.Add(this.DeleteTaskButton);
      this.groupBox3.Controls.Add(this.label13);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(16, 248);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(432, 120);
      this.groupBox3.TabIndex = 1;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Windows Task Scheduler Settings";
      // 
      // DeleteTaskButton
      // 
      this.DeleteTaskButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.DeleteTaskButton.Location = new System.Drawing.Point(320, 88);
      this.DeleteTaskButton.Name = "DeleteTaskButton";
      this.DeleteTaskButton.Size = new System.Drawing.Size(96, 22);
      this.DeleteTaskButton.TabIndex = 12;
      this.DeleteTaskButton.Text = "Delete Task";
      this.DeleteTaskButton.UseVisualStyleBackColor = true;
      this.DeleteTaskButton.Click += new System.EventHandler(this.DeleteTaskButton_Click);
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(16, 78);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(192, 32);
      this.label13.TabIndex = 11;
      this.label13.Text = "Note: Windows does not run a task if no password is assigned to the user.";
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 408);
      this.tabControl1.TabIndex = 0;
      this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.groupBox4);
      this.tabPage1.Controls.Add(this.groupBox2);
      this.tabPage1.Controls.Add(this.groupBox1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(464, 382);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "General";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox4.Controls.Add(this.useColorCheckBox);
      this.groupBox4.Controls.Add(this.btnClearTVDatabase);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox4.Location = new System.Drawing.Point(16, 312);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(432, 56);
      this.groupBox4.TabIndex = 2;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Miscellaneous";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.mpLabel1);
      this.groupBox2.Controls.Add(this.cbGrabDVBEPG);
      this.groupBox2.Controls.Add(this.listView1);
      this.groupBox2.Controls.Add(this.button3);
      this.groupBox2.Controls.Add(this.button2);
      this.groupBox2.Controls.Add(this.treeView1);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(16, 144);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(432, 160);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = " DVB-EPG Grabber";
      // 
      // mpLabel1
      // 
      this.mpLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mpLabel1.Location = new System.Drawing.Point(305, 24);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(116, 84);
      this.mpLabel1.TabIndex = 5;
      this.mpLabel1.Text = "Grab EPG from DVB - This option should not be selected when using the analogue fu" +
          "nction on hybrid cards";
      // 
      // cbGrabDVBEPG
      // 
      this.cbGrabDVBEPG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.cbGrabDVBEPG.AutoSize = true;
      this.cbGrabDVBEPG.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbGrabDVBEPG.Location = new System.Drawing.Point(288, 24);
      this.cbGrabDVBEPG.Name = "cbGrabDVBEPG";
      this.cbGrabDVBEPG.Size = new System.Drawing.Size(13, 12);
      this.cbGrabDVBEPG.TabIndex = 4;
      this.cbGrabDVBEPG.TextAlign = System.Drawing.ContentAlignment.TopLeft;
      this.cbGrabDVBEPG.UseVisualStyleBackColor = true;
      // 
      // listView1
      // 
      this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
      this.listView1.FullRowSelect = true;
      this.listView1.HideSelection = false;
      this.listView1.Location = new System.Drawing.Point(16, 22);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(240, 21);
      this.listView1.TabIndex = 0;
      this.listView1.TabStop = false;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Languages to grab";
      this.columnHeader1.Width = 219;
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button3.Location = new System.Drawing.Point(344, 120);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(72, 22);
      this.button3.TabIndex = 3;
      this.button3.Text = "None";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button2
      // 
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.Location = new System.Drawing.Point(264, 120);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(72, 22);
      this.button2.TabIndex = 2;
      this.button2.Text = "All";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // treeView1
      // 
      this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.treeView1.CheckBoxes = true;
      this.treeView1.Location = new System.Drawing.Point(16, 42);
      this.treeView1.Name = "treeView1";
      this.treeView1.Size = new System.Drawing.Size(240, 102);
      this.treeView1.TabIndex = 1;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.folderNameTextBox);
      this.groupBox1.Controls.Add(this.browseButton);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.useTimeZoneCheckBox);
      this.groupBox1.Controls.Add(this.compensateTextBox);
      this.groupBox1.Controls.Add(this.textBoxMinutes);
      this.groupBox1.Controls.Add(this.label15);
      this.groupBox1.Controls.Add(this.label16);
      this.groupBox1.Controls.Add(this.folderNameLabel);
      this.groupBox1.Controls.Add(this.btnUpdateTvGuide);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(16, 16);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(432, 120);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = " XMLTV ";
      // 
      // label16
      // 
      this.label16.Location = new System.Drawing.Point(192, 91);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(8, 9);
      this.label16.TabIndex = 6;
      this.label16.Text = ":";
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.groupBox5);
      this.tabPage2.Controls.Add(this.label7);
      this.tabPage2.Controls.Add(this.groupBox3);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(464, 382);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Scheduler";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // groupBox5
      // 
      this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox5.Controls.Add(this.runGrabberLowPriorityCheckBox);
      this.groupBox5.Controls.Add(this.GrabbercomboBox);
      this.groupBox5.Controls.Add(this.label3);
      this.groupBox5.Controls.Add(this.label14);
      this.groupBox5.Controls.Add(this.parametersButton);
      this.groupBox5.Controls.Add(this.RunGrabberButton);
      this.groupBox5.Controls.Add(this.label6);
      this.groupBox5.Controls.Add(this.daysToKeepTextBox);
      this.groupBox5.Controls.Add(this.basicRadioButton);
      this.groupBox5.Controls.Add(this.parametersTextBox);
      this.groupBox5.Controls.Add(this.AdvancedDaystextBox);
      this.groupBox5.Controls.Add(this.advancedRadioButton);
      this.groupBox5.Controls.Add(this.label4);
      this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox5.Location = new System.Drawing.Point(16, 16);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(432, 224);
      this.groupBox5.TabIndex = 0;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "Grabber Settings";
      // 
      // runGrabberLowPriorityCheckBox
      // 
      this.runGrabberLowPriorityCheckBox.AutoSize = true;
      this.runGrabberLowPriorityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.runGrabberLowPriorityCheckBox.Location = new System.Drawing.Point(16, 192);
      this.runGrabberLowPriorityCheckBox.Name = "runGrabberLowPriorityCheckBox";
      this.runGrabberLowPriorityCheckBox.Size = new System.Drawing.Size(166, 17);
      this.runGrabberLowPriorityCheckBox.TabIndex = 12;
      this.runGrabberLowPriorityCheckBox.Text = "Run grabber with lower priority";
      this.runGrabberLowPriorityCheckBox.UseVisualStyleBackColor = true;
      // 
      // label14
      // 
      this.label14.Location = new System.Drawing.Point(16, 48);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(64, 16);
      this.label14.TabIndex = 2;
      this.label14.Text = "Parameters:";
      // 
      // TVProgramGuide
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "TVProgramGuide";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.groupBox5.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion

    private void SetupGrabbers()
    {
      GrabbercomboBox.Items.Add("tv_grab_de_tvtoday");
      GrabbercomboBox.Items.Add("tv_grab_dk");
      GrabbercomboBox.Items.Add("tv_grab_es");
      GrabbercomboBox.Items.Add("tv_grab_es_digital");
      GrabbercomboBox.Items.Add("tv_grab_fi");
      GrabbercomboBox.Items.Add("tv_grab_fr");
      GrabbercomboBox.Items.Add("tv_grab_hr");
      GrabbercomboBox.Items.Add("tv_grab_huro");
      GrabbercomboBox.Items.Add("tv_grab_it");
      GrabbercomboBox.Items.Add("tv_grab_it_lt");
      GrabbercomboBox.Items.Add("tv_grab_na_dd");
      GrabbercomboBox.Items.Add("tv_grab_nl");
      GrabbercomboBox.Items.Add("tv_grab_nl_wolf");
      GrabbercomboBox.Items.Add("tv_grab_no_gfeed");
      GrabbercomboBox.Items.Add("tv_grab_pt");
      GrabbercomboBox.Items.Add("tv_grab_se");
      GrabbercomboBox.Items.Add("tv_grab_se_swedb");
      GrabbercomboBox.Items.Add("tv_grab_uk_bleb");
      GrabbercomboBox.Items.Add("tv_grab_uk_rt");
      GrabbercomboBox.Items.Add("TVguide.xml File");
    }

    public override void LoadSettings()
    {
      FillInEPGLanguages();

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        cbGrabDVBEPG.Checked = xmlreader.GetValueAsBool("xmltv", "epgdvb", false);
        useColorCheckBox.Checked = xmlreader.GetValueAsBool("xmltv", "colors", false);
        useTimeZoneCheckBox.Checked = xmlreader.GetValueAsBool("xmltv", "usetimezone", true);
        OldTimeZoneCompensation = useTimeZoneCheckBox.Checked;
        OldTimeZoneOffsetHours = xmlreader.GetValueAsInt("xmltv", "timezonecorrectionhours", 0);
        OldTimeZoneOffsetMins = xmlreader.GetValueAsInt("xmltv", "timezonecorrectionmins", 0);

        compensateTextBox.Text = OldTimeZoneOffsetHours.ToString();
        textBoxMinutes.Text = OldTimeZoneOffsetMins.ToString();

        string strDir = System.IO.Directory.GetCurrentDirectory();
        strDir += @"\xmltv";
        folderNameTextBox.Text = xmlreader.GetValueAsString("xmltv", "folder", strDir);

        GrabbercomboBox.SelectedItem = xmlreader.GetValueAsString("xmltv", "grabber", "");
        AdvancedDaystextBox.Text = xmlreader.GetValueAsString("xmltv", "days", "1,2,3,5");
        parametersTextBox.Text = xmlreader.GetValueAsString("xmltv", "args", "");
        daysToKeepTextBox.Text = xmlreader.GetValueAsString("xmltv", "daystokeep", "7");
        advancedRadioButton.Checked = xmlreader.GetValueAsBool("xmltv", "advanced", false);
        runGrabberLowPriorityCheckBox.Checked = xmlreader.GetValueAsBool("xmltv", "lowpriority", false);
        basicRadioButton.Checked = !advancedRadioButton.Checked;
        btnUpdateTvGuide.Enabled = useTimeZoneCheckBox.Checked;

        string langGrabText = xmlreader.GetValueAsString("epg-grabbing", "grabLanguages", "");
        string[] langs = langGrabText.Split(new char[] { '/' });
        if (langs != null)
        {
          foreach (string language in langs)
            foreach (TreeNode tn in treeView1.Nodes)
            {
              string tag = (string)tn.Tag;
              if (tag != null)
              {
                if (tag == language)
                {
                  tn.Checked = true;
                  break;
                }
              }
            }
        }

      }
      short[] taskSettings = new short[3];
      string userAccount = null;
      int index = 0;
      bool taskExists = TaskScheduler.GetTask(ref taskSettings, ref userAccount);
      hoursTextBox.Text = taskSettings[0].ToString();
      if (hoursTextBox.Text.Length == 1) hoursTextBox.Text = "0" + taskSettings[0];
      minutesTextBox.Text = taskSettings[1].ToString();
      if (minutesTextBox.Text.Length == 1) minutesTextBox.Text = "0" + taskSettings[1];
      dayIntervalTextBox.Text = taskSettings[2].ToString();
      if (userAccount != null && userAccount != "")
      {
        index = userAccount.IndexOf(@"\");
        if (index > 0) index++;
        UserTextBox.Text = userAccount.Substring(index);
      }
      if (taskExists)
      {
        DeleteTaskButton.Enabled = true;
      }
      else
      {
        DeleteTaskButton.Enabled = false;
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("xmltv", "epgdvb", cbGrabDVBEPG.Checked);
        xmlwriter.SetValueAsBool("xmltv", "colors", useColorCheckBox.Checked);
        xmlwriter.SetValueAsBool("xmltv", "usetimezone", useTimeZoneCheckBox.Checked);

        xmlwriter.SetValue("xmltv", "timezonecorrectionhours", compensateTextBox.Text);
        xmlwriter.SetValue("xmltv", "timezonecorrectionmins", textBoxMinutes.Text);
        xmlwriter.SetValue("xmltv", "folder", folderNameTextBox.Text);

        xmlwriter.SetValue("xmltv", "grabber", GrabbercomboBox.Text);
        xmlwriter.SetValue("xmltv", "daystokeep", daysToKeepTextBox.Text);
        xmlwriter.SetValueAsBool("xmltv", "advanced", advancedRadioButton.Checked);
        xmlwriter.SetValueAsBool("xmltv", "lowpriority", runGrabberLowPriorityCheckBox.Checked);
        xmlwriter.SetValue("xmltv", "days", AdvancedDaystextBox.Text);
        xmlwriter.SetValue("xmltv", "args", parametersTextBox.Text);
        string langGrabText = "";
        foreach (TreeNode tn in treeView1.Nodes)
        {
          if (tn.Checked == true)
            langGrabText += ((string)tn.Tag) + "/";
        }

        xmlwriter.SetValue("epg-grabbing", "grabLanguages", langGrabText);

      }

      if (createScheduleCheckBox.Checked)
      {
        int hours = 0, minutes = 0, days = 1;
        try
        {
          hours = System.Convert.ToInt32(hoursTextBox.Text);
          minutes = System.Convert.ToInt32(minutesTextBox.Text);
          days = System.Convert.ToInt32(dayIntervalTextBox.Text);

        }
        catch (Exception)
        {
        }
        if (hours > 23) hours = 23;
        if (hours < 0) hours = 0;
        if (minutes > 59) minutes = 59;
        if (minutes < 0) minutes = 0;
        if (days < 1) days = 1;

        TaskScheduler.CreateTask((short)hours,
                                  (short)minutes,
                                  (short)days,
                                  UserTextBox.Text, PasswordTextBox.Text);
      }
    }

    private void compensateTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, '-' and backspace.
      //
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8 && e.KeyChar != '-')
      {
        e.Handled = true;
      }
    }

    private void browseButton_Click(object sender, System.EventArgs e)
    {
      using (folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = "Select the folder where the XMLTV data is stored";
        folderBrowserDialog.ShowNewFolderButton = true;
        folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
        DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

        if (dialogResult == DialogResult.OK)
        {
          folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }

    private void daysToKeepTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, '-' and backspace.
      //
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void GrabbercomboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      basicRadioButton.Enabled = advancedRadioButton.Enabled = AdvancedDaystextBox.Enabled = daysToKeepTextBox.Enabled = parametersButton.Enabled = parametersTextBox.Enabled = (GrabbercomboBox.SelectedItem != null);
      parametersTextBox.Text = "";

      if (GrabbercomboBox.Text == "tv_grab_fi")
        daysToKeepTextBox.Text = "10";
      else if (GrabbercomboBox.Text == "tv_grab_uk_rt")
        daysToKeepTextBox.Text = "14";
      else if (GrabbercomboBox.Text == "tv_grab_huro")
        daysToKeepTextBox.Text = "8";
      else if ((GrabbercomboBox.Text == "tv_grab_es") | (GrabbercomboBox.Text == "tv_grab_es_digital") | (GrabbercomboBox.Text == "tv_grab_pt"))
        daysToKeepTextBox.Text = "3";
      else if ((GrabbercomboBox.Text == "tv_grab_se") | (GrabbercomboBox.Text == "tv_grab_se_swedb"))
        daysToKeepTextBox.Text = "5";
      else
        daysToKeepTextBox.Text = "7";

      if (advancedRadioButton.Enabled == true || basicRadioButton.Enabled == true)
      {
        AdvancedDaystextBox.Enabled = advancedRadioButton.Checked;
        daysToKeepTextBox.Enabled = basicRadioButton.Checked;
      }
      if (GrabbercomboBox.Text == "TVguide.xml File")
      {
        advancedRadioButton.Enabled = false;
        basicRadioButton.Enabled = false;
        AdvancedDaystextBox.Enabled = false;
        daysToKeepTextBox.Enabled = false;
        parametersTextBox.Enabled = false;
        parametersButton.Enabled = false;
      }
    }

    private void GrabbercomboBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if (e.KeyChar == (char)System.Windows.Forms.Keys.Delete || e.KeyChar == (char)System.Windows.Forms.Keys.Back)
      {
        GrabbercomboBox.SelectedItem = null;
        GrabbercomboBox.Text = string.Empty;
      }
    }

    private void parametersButton_Click(object sender, System.EventArgs e)
    {
      parametersTextBox.Text = "";
      ParameterForm parameters = new ParameterForm();

      if (GrabbercomboBox.Text == ("tv_grab_dk") | GrabbercomboBox.Text == ("tv_grab_es") | GrabbercomboBox.Text == ("tv_grab_es_digital")
        | GrabbercomboBox.Text == ("tv_grab_fi") | GrabbercomboBox.Text == ("tv_grab_hr") | GrabbercomboBox.Text == ("tv_grab_huro") | GrabbercomboBox.Text == ("tv_grab_no_gfeed")
        | GrabbercomboBox.Text == ("tv_grab_pt") | GrabbercomboBox.Text == ("tv_grab_se") | GrabbercomboBox.Text == ("tv_grab_nl_wolf")
        | (GrabbercomboBox.Text == "tv_grab_se_swedb") | (GrabbercomboBox.Text == "tv_grab_uk_bleb") | (GrabbercomboBox.Text == "tv_grab_uk_rt"))
      {
        parameters.AddParameter("", "No options available for this grabber");
      }
      else if (GrabbercomboBox.Text == ("tv_grab_fr") | GrabbercomboBox.Text == ("tv_grab_it") | GrabbercomboBox.Text == ("tv_grab_nl"))
      {
        parameters.AddParameter("--slow", "Fetch full program details (but takes longer)");
      }
      else if (GrabbercomboBox.Text == "tv_grab_de_tvtoday")
      {
        parameters.AddParameter("--slow", "Fetch full program details (but takes longer)");
        parameters.AddParameter("--nosqueezeout", "Don't parse program descriptions for adiitional information (actors,director,etc");
        parameters.AddParameter("--slow --nosqueezeout", "Fetch full program details and don't parse descriptions");
      }
      else if (GrabbercomboBox.Text == "tv_grab_na_dd")
      {
        parameters.AddParameter("--auto-config add", "Appends new channels to the config file");
        parameters.AddParameter("--auto-config ignore", "Ignore new channels");
        parameters.AddParameter("--old-chan-id", "Use old tv_grab_na style channel ids");
        parameters.AddParameter("--old-chan-id --auto-config add", "Old tv_grab_na style channel ids and append new channels");
        parameters.AddParameter("--old-chan-id --auto-config ignore", "Old tv_grab_na style channel ids and ignore new channels");
      }
      else if (GrabbercomboBox.Text == "tv_grab_it_lt")
      {
        parameters.AddParameter("--password-file", "Use password file - tv_grab_it_lt_password.txt in XMLTV folder");
        parameters.AddParameter("--slow", "Fetch full program details (but takes longer)");
        parameters.AddParameter("--slow --password-file", "Use password file  - tv_grab_it_lt_password.txt in XMLTV folder and fetch full program details");
      }

      if (parameters.ShowDialog(parametersButton) == DialogResult.OK)
      {
        parametersTextBox.Text += parameters.SelectedParameter;
      }
    }

    private void AdvancedDaystextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if (e.KeyChar == ',')
      {
        if (AdvancedDaystextBox.Text.EndsWith(","))
        {
          e.Handled = true;
          return;
        }
      }

      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8 && e.KeyChar != ',')
      {
        e.Handled = true;
      }
    }

    protected void basicRadioButton_CheckedChanged(object sender, System.EventArgs e)
    {
      if (basicRadioButton.Enabled == true)
      {
        AdvancedDaystextBox.Enabled = false;
        daysToKeepTextBox.Enabled = true;
      }
    }

    protected void advancedRadioButton_CheckedChanged(object sender, System.EventArgs e)
    {
      if (advancedRadioButton.Enabled == true)
      {
        AdvancedDaystextBox.Enabled = true;
        daysToKeepTextBox.Enabled = false;
      }
    }
    protected void createScheduleCheckBox_CheckedChanged(object sender, System.EventArgs e)
    {
      if (createScheduleCheckBox.Checked)
      {
        hoursTextBox.Enabled = true;
        minutesTextBox.Enabled = true;
        dayIntervalTextBox.Enabled = true;
        UserTextBox.Enabled = true;
        PasswordTextBox.Enabled = true;
      }
      else
      {
        hoursTextBox.Enabled = false;
        minutesTextBox.Enabled = false;
        dayIntervalTextBox.Enabled = false;
        UserTextBox.Enabled = false;
        PasswordTextBox.Enabled = false;
      }
    }
    private void DeleteTaskButton_Click(object sender, System.EventArgs e)
    {
      TaskScheduler.DeleteTask();
      hoursTextBox.Text = "01";
      minutesTextBox.Text = "00";
      dayIntervalTextBox.Text = "1";
      UserTextBox.Text = "";
      PasswordTextBox.Text = "";
      createScheduleCheckBox.Checked = false;
      DeleteTaskButton.Enabled = false;
    }
    private void dayIntervalTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }
    private void hoursTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }
    private void minutesTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      //
      // Allow only numbers, and backspace.
      //
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void RunGrabberButton_Click(object sender, System.EventArgs e)
    {
      SaveSettings();
      if ((File.Exists(folderNameTextBox.Text + @"\xmltv.exe")) | (GrabbercomboBox.Text == "TVguide.xml File"))
      {
        SetupGrabber.LaunchGuideScheduler();
      }
      else
      {
        MessageBox.Show("XMLTV.exe cannot be found in the directory you have setup as the XMLTV folder." + "\n\n" + "Ensure that you have installed the XMLTV application, and that the XMLTV folder" + "\n" + "setting points to the directory where XMLTV.exe is installed" + "\n" + "XMLTV can be downloaded from http://sourceforge.net/projects/xmltv",
                        "MediaPortal Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void groupBox2_Enter(object sender, System.EventArgs e)
    {

    }

    private void btnUpdateTvGuide_Click(object sender, System.EventArgs e)
    {
      if (!useTimeZoneCheckBox.Checked) return;
      try
      {
        int iNewTimeZoneCompensationHours = Int32.Parse(compensateTextBox.Text);
        int iNewTimeZoneCompensationMins = Int32.Parse(textBoxMinutes.Text);
        if (iNewTimeZoneCompensationHours == OldTimeZoneOffsetHours &&
            iNewTimeZoneCompensationMins == OldTimeZoneOffsetMins) return;
        int oldoffset = OldTimeZoneOffsetHours * 60 + OldTimeZoneOffsetMins;
        int newoffset = iNewTimeZoneCompensationHours * 60 + iNewTimeZoneCompensationMins;

        int offset = newoffset - oldoffset;

        TVDatabase.OffsetProgramsByMinutes(offset);
        OldTimeZoneOffsetHours = iNewTimeZoneCompensationHours;
        OldTimeZoneOffsetMins = iNewTimeZoneCompensationMins;
        MessageBox.Show("TVDatabase is updated with new timezone offset",
          "MediaPortal Configuration", MessageBoxButtons.OK, MessageBoxIcon.Information);
        SaveSettings();

      }
      catch (Exception)
      {
      }
    }

    private void useTimeZoneCheckBox_CheckedChanged(object sender, System.EventArgs e)
    {
      btnUpdateTvGuide.Enabled = useTimeZoneCheckBox.Checked;
    }

    private void btnClearTVDatabase_Click(object sender, System.EventArgs e)
    {
      TVDatabase.RemovePrograms();
      MessageBox.Show("All programs are removed from the tv database",
        "MediaPortal Configuration", MessageBoxButtons.OK, MessageBoxIcon.Information);

      // provoke tvmovie to re-import the database, too
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        xmlwriter.SetValue("tvmovie", "lastupdate", 1);
    }

    private void label15_Click(object sender, System.EventArgs e)
    {

    }

    void FillInEPGLanguages()
    {
      if (treeView1 != null)
      {
        if (treeView1.Nodes.Count < 1)
        {
          MediaPortal.TV.Recording.DVBSections dvbSections = new MediaPortal.TV.Recording.DVBSections();
          ArrayList codes = new ArrayList();
          codes = dvbSections.GetLanguageCodes();
          int n = 0;
          treeView1.Nodes.Clear();
          foreach (string code in codes)
          {
            TreeNode tn = new TreeNode(MediaPortal.TV.Recording.DVBSections.GetLanguageFromCode(code) + " (" + code + ")");
            tn.Tag = code;
            n++;
            treeView1.Nodes.Add(tn);
          }
        }
      }
    }
    private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (tabControl1.SelectedIndex == 0)
      {
        FillInEPGLanguages();
      }
    }

    private void button2_Click(object sender, System.EventArgs e)
    {
      if (treeView1 != null)
      {
        foreach (TreeNode tn in treeView1.Nodes)
          tn.Checked = true;
      }

    }

    private void button3_Click(object sender, System.EventArgs e)
    {
      if (treeView1 != null)
      {
        foreach (TreeNode tn in treeView1.Nodes)
          tn.Checked = false;
      }

    }

  }
}


