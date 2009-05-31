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

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#pragma warning disable 108

namespace MediaPortal.Configuration.TVE2.Sections
{
  public class TVRecording : SectionSettings
  {
    private MPLabel label4;
    private MPLabel label3;
    private MPLabel label2;
    private MPLabel label1;
    private MPTextBox startTextBox;
    private MPTextBox endTextBox;
    private FolderBrowserDialog folderBrowserDialog;
    private MPCheckBox cbDeleteWatchedShows;
    private MPCheckBox cbAddRecordingsToMovie;
    private MPGroupBox groupBox2;
    private MPTextBox textBoxFormat;
    private MPLabel label38;
    private MPLabel label6;
    private MPLabel label7;
    private MPLabel label10;
    private MPLabel label12;
    private MPLabel label11;
    private MPLabel label9;
    private MPTabControl tabControl1;
    private MPTabPage tabPageSettings;
    private MPTabPage tabPage2;
    private MPGroupBox groupBox3;
    private MPLabel label14;
    private MPComboBox comboBoxRecording;
    private MPTextBox textBoxSample;
    private IContainer components = null;
    private MPGroupBox groupBox1;

    private string[] formatKind = {"Movies", "Series"};
    private MPTabPage tabPage1;
    private MPGroupBox groupBox4;
    private MPLabel label5;
    private MPComboBox comboDrives;
    private MPLabel lblTotalSpace;
    private MPLabel label8;
    private MPLabel label13;
    private MPLabel lblFreeDiskSpace;
    private MPLabel labelQuota;
    private TrackBar trackBar1;
    private MPLabel label15;
    private MPCheckBox cbBackToBack;
    private string[] formatString = {string.Empty, string.Empty};

    public TVRecording()
      : this("Recording")
    {
    }

    public TVRecording(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
      comboDrives.SelectedIndexChanged += new EventHandler(comboDrives_SelectedIndexChanged);


      // TODO: Add any initialization after the InitializeComponent call

      // Disable if TVE3
      if (File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        this.Enabled = false;
      }
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
      this.cbAddRecordingsToMovie = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.endTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.startTextBox = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbDeleteWatchedShows = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxSample = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label14 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxRecording = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.label7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxFormat = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label38 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tabControl1 = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.tabPageSettings = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbBackToBack = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabPage2 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.tabPage1 = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.groupBox4 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.labelQuota = new MediaPortal.UserInterface.Controls.MPLabel();
      this.trackBar1 = new System.Windows.Forms.TrackBar();
      this.label15 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblFreeDiskSpace = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.lblTotalSpace = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboDrives = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPageSettings.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.groupBox4.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.trackBar1)).BeginInit();
      this.SuspendLayout();
      // 
      // cbAddRecordingsToMovie
      // 
      this.cbAddRecordingsToMovie.AutoSize = true;
      this.cbAddRecordingsToMovie.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbAddRecordingsToMovie.Location = new System.Drawing.Point(19, 134);
      this.cbAddRecordingsToMovie.Name = "cbAddRecordingsToMovie";
      this.cbAddRecordingsToMovie.Size = new System.Drawing.Size(185, 17);
      this.cbAddRecordingsToMovie.TabIndex = 7;
      this.cbAddRecordingsToMovie.Text = "Add recordings to movie database";
      this.cbAddRecordingsToMovie.UseVisualStyleBackColor = true;
      // 
      // endTextBox
      // 
      this.endTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.endTextBox.BorderColor = System.Drawing.Color.Empty;
      this.endTextBox.Location = new System.Drawing.Point(94, 44);
      this.endTextBox.MaxLength = 3;
      this.endTextBox.Name = "endTextBox";
      this.endTextBox.Size = new System.Drawing.Size(26, 20);
      this.endTextBox.TabIndex = 4;
      this.endTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.endTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.endTextBox_KeyPress);
      // 
      // startTextBox
      // 
      this.startTextBox.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.startTextBox.BorderColor = System.Drawing.Color.Empty;
      this.startTextBox.Location = new System.Drawing.Point(94, 20);
      this.startTextBox.MaxLength = 3;
      this.startTextBox.Name = "startTextBox";
      this.startTextBox.Size = new System.Drawing.Size(26, 20);
      this.startTextBox.TabIndex = 1;
      this.startTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.startTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.startTextBox_KeyPress);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(16, 48);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(76, 13);
      this.label4.TabIndex = 3;
      this.label4.Text = "Stop recording";
      // 
      // label3
      // 
      this.label3.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(125, 48);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(143, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "minute(s) after program ends.";
      // 
      // label2
      // 
      this.label2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(125, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(154, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "minute(s) before program starts.";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(76, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Start recording";
      // 
      // cbDeleteWatchedShows
      // 
      this.cbDeleteWatchedShows.AutoSize = true;
      this.cbDeleteWatchedShows.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbDeleteWatchedShows.Location = new System.Drawing.Point(19, 110);
      this.cbDeleteWatchedShows.Name = "cbDeleteWatchedShows";
      this.cbDeleteWatchedShows.Size = new System.Drawing.Size(240, 17);
      this.cbDeleteWatchedShows.TabIndex = 6;
      this.cbDeleteWatchedShows.Text = "Automatically delete recordings after watching";
      this.cbDeleteWatchedShows.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.groupBox1);
      this.groupBox2.Controls.Add(this.textBoxSample);
      this.groupBox2.Controls.Add(this.label14);
      this.groupBox2.Controls.Add(this.comboBoxRecording);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.textBoxFormat);
      this.groupBox2.Controls.Add(this.label38);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox2.Location = new System.Drawing.Point(16, 16);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(432, 336);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label12);
      this.groupBox1.Controls.Add(this.label10);
      this.groupBox1.Controls.Add(this.label9);
      this.groupBox1.Controls.Add(this.label11);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox1.Location = new System.Drawing.Point(80, 152);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(336, 168);
      this.groupBox1.TabIndex = 21;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Available Tags";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(256, 24);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(66, 130);
      this.label12.TabIndex = 15;
      this.label12.Text =
        "start day\r\nstart month\r\nstart year\r\nstart hours\r\nstart minutes\r\nend day\r\nend mont" +
        "h\r\nend year\r\nend hours\r\nend minutes";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(90, 24);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(82, 130);
      this.label10.TabIndex = 13;
      this.label10.Text =
        "channel name\r\ntitle\r\nepisode name\r\ngenre\r\nseries number\r\nepisode number\r\nepisode " +
        "part\r\ndate\r\nstart time\r\nend time";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(16, 24);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(70, 130);
      this.label9.TabIndex = 12;
      this.label9.Text =
        "%channel% =\r\n%title% =\r\n%name% =\r\n%genre% =\r\n%series% =\r\n%episode% =\r\n%part% =\r\n%" +
        "date% =\r\n%start% =\r\n%end% =\r\n";
      this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(168, 24);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(81, 130);
      this.label11.TabIndex = 14;
      this.label11.Text =
        "%startday% =\r\n%startmonth% =\r\n%startyear% =\r\n%starthh% =\r\n%startmm% =\r\n%endday% =" +
        "\r\n%endmonth% =\r\n%endyear% =\r\n%endhh% =\r\n%endmm% =\r\n";
      this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // textBoxSample
      // 
      this.textBoxSample.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxSample.BackColor = System.Drawing.SystemColors.ControlLight;
      this.textBoxSample.BorderColor = System.Drawing.Color.Empty;
      this.textBoxSample.Cursor = System.Windows.Forms.Cursors.Default;
      this.textBoxSample.Location = new System.Drawing.Point(80, 112);
      this.textBoxSample.Name = "textBoxSample";
      this.textBoxSample.ReadOnly = true;
      this.textBoxSample.Size = new System.Drawing.Size(336, 20);
      this.textBoxSample.TabIndex = 19;
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(16, 24);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(59, 13);
      this.label14.TabIndex = 18;
      this.label14.Text = "Recording:";
      // 
      // comboBoxRecording
      // 
      this.comboBoxRecording.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxRecording.FormattingEnabled = true;
      this.comboBoxRecording.Items.AddRange(new object[]
                                              {
                                                "Movies",
                                                "Series"
                                              });
      this.comboBoxRecording.Location = new System.Drawing.Point(80, 20);
      this.comboBoxRecording.Name = "comboBoxRecording";
      this.comboBoxRecording.Size = new System.Drawing.Size(88, 21);
      this.comboBoxRecording.TabIndex = 17;
      this.comboBoxRecording.SelectedIndexChanged += new System.EventHandler(this.comboBoxRecording_SelectedIndexChanged);
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(80, 48);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(327, 26);
      this.label7.TabIndex = 0;
      this.label7.Text = "A recording is a movie, if the recording is a manual or single type.\r\nUse blockqu" +
                         "otes [ ] to specify optional fields and \\ for relative paths.";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(16, 116);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(45, 13);
      this.label6.TabIndex = 5;
      this.label6.Text = "Sample:";
      // 
      // textBoxFormat
      // 
      this.textBoxFormat.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFormat.BorderColor = System.Drawing.Color.Empty;
      this.textBoxFormat.Location = new System.Drawing.Point(80, 84);
      this.textBoxFormat.Name = "textBoxFormat";
      this.textBoxFormat.Size = new System.Drawing.Size(336, 20);
      this.textBoxFormat.TabIndex = 4;
      this.textBoxFormat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFormat_KeyPress);
      this.textBoxFormat.TextChanged += new System.EventHandler(this.textBoxFormat_TextChanged);
      // 
      // label38
      // 
      this.label38.AutoSize = true;
      this.label38.Location = new System.Drawing.Point(16, 88);
      this.label38.Name = "label38";
      this.label38.Size = new System.Drawing.Size(42, 13);
      this.label38.TabIndex = 3;
      this.label38.Text = "Format:";
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPageSettings);
      this.tabControl1.Controls.Add(this.tabPage2);
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Location = new System.Drawing.Point(0, 8);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(472, 400);
      this.tabControl1.TabIndex = 2;
      // 
      // tabPageSettings
      // 
      this.tabPageSettings.Controls.Add(this.groupBox3);
      this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
      this.tabPageSettings.Name = "tabPageSettings";
      this.tabPageSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageSettings.Size = new System.Drawing.Size(464, 374);
      this.tabPageSettings.TabIndex = 0;
      this.tabPageSettings.Text = "Settings";
      this.tabPageSettings.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.cbBackToBack);
      this.groupBox3.Controls.Add(this.cbAddRecordingsToMovie);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Controls.Add(this.endTextBox);
      this.groupBox3.Controls.Add(this.cbDeleteWatchedShows);
      this.groupBox3.Controls.Add(this.startTextBox);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.Controls.Add(this.label4);
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox3.Location = new System.Drawing.Point(16, 16);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(432, 163);
      this.groupBox3.TabIndex = 1;
      this.groupBox3.TabStop = false;
      // 
      // cbBackToBack
      // 
      this.cbBackToBack.AutoSize = true;
      this.cbBackToBack.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.cbBackToBack.Location = new System.Drawing.Point(19, 74);
      this.cbBackToBack.Name = "cbBackToBack";
      this.cbBackToBack.Size = new System.Drawing.Size(380, 17);
      this.cbBackToBack.TabIndex = 8;
      this.cbBackToBack.Text = "Ignore pre/post recording within block for back-to-back recordings";
      this.cbBackToBack.UseVisualStyleBackColor = true;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.groupBox2);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(464, 374);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Custom Paths and Filenames";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.groupBox4);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(464, 374);
      this.tabPage1.TabIndex = 2;
      this.tabPage1.Text = "Disk quota";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.labelQuota);
      this.groupBox4.Controls.Add(this.trackBar1);
      this.groupBox4.Controls.Add(this.label15);
      this.groupBox4.Controls.Add(this.lblFreeDiskSpace);
      this.groupBox4.Controls.Add(this.label13);
      this.groupBox4.Controls.Add(this.lblTotalSpace);
      this.groupBox4.Controls.Add(this.label8);
      this.groupBox4.Controls.Add(this.label5);
      this.groupBox4.Controls.Add(this.comboDrives);
      this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox4.Location = new System.Drawing.Point(16, 16);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(432, 334);
      this.groupBox4.TabIndex = 0;
      this.groupBox4.TabStop = false;
      // 
      // labelQuota
      // 
      this.labelQuota.AutoSize = true;
      this.labelQuota.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold,
                                                     System.Drawing.GraphicsUnit.Point, ((byte) (0)));
      this.labelQuota.Location = new System.Drawing.Point(234, 134);
      this.labelQuota.Name = "labelQuota";
      this.labelQuota.Size = new System.Drawing.Size(48, 13);
      this.labelQuota.TabIndex = 8;
      this.labelQuota.Text = "label16";
      // 
      // trackBar1
      // 
      this.trackBar1.Location = new System.Drawing.Point(30, 162);
      this.trackBar1.Maximum = 100;
      this.trackBar1.Name = "trackBar1";
      this.trackBar1.Size = new System.Drawing.Size(340, 40);
      this.trackBar1.TabIndex = 7;
      this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(27, 134);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(201, 13);
      this.label15.TabIndex = 6;
      this.label15.Text = "Delete recordings when there is less then";
      // 
      // lblFreeDiskSpace
      // 
      this.lblFreeDiskSpace.AutoSize = true;
      this.lblFreeDiskSpace.Location = new System.Drawing.Point(118, 95);
      this.lblFreeDiskSpace.Name = "lblFreeDiskSpace";
      this.lblFreeDiskSpace.Size = new System.Drawing.Size(41, 13);
      this.lblFreeDiskSpace.TabIndex = 5;
      this.lblFreeDiskSpace.Text = "label15";
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(27, 95);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(85, 13);
      this.label13.TabIndex = 4;
      this.label13.Text = "Free diskspace::";
      // 
      // lblTotalSpace
      // 
      this.lblTotalSpace.AutoSize = true;
      this.lblTotalSpace.Location = new System.Drawing.Point(118, 68);
      this.lblTotalSpace.Name = "lblTotalSpace";
      this.lblTotalSpace.Size = new System.Drawing.Size(49, 13);
      this.lblTotalSpace.TabIndex = 3;
      this.lblTotalSpace.Text = "total disk";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(27, 68);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(85, 13);
      this.label8.TabIndex = 2;
      this.label8.Text = "Total diskspace:";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(27, 23);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(35, 13);
      this.label5.TabIndex = 1;
      this.label5.Text = "Drive:";
      // 
      // comboDrives
      // 
      this.comboDrives.BorderColor = System.Drawing.Color.Empty;
      this.comboDrives.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboDrives.FormattingEnabled = true;
      this.comboDrives.Location = new System.Drawing.Point(78, 20);
      this.comboDrives.Name = "comboDrives";
      this.comboDrives.Size = new System.Drawing.Size(121, 21);
      this.comboDrives.TabIndex = 0;
      // 
      // TVRecording
      // 
      this.Controls.Add(this.tabControl1);
      this.Name = "TVRecording";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tabPageSettings.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.trackBar1)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    private class Example
    {
      public string Channel;
      public string Title;
      public string Episode;
      public string SeriesNum;
      public string EpisodeNum;
      public string EpisodePart;
      public DateTime StartDate;
      public DateTime EndDate;
      public string Genre;

      public Example(string channel, string title, string episode, string seriesNum, string episodeNum,
                     string episodePart, string genre, DateTime startDate, DateTime endDate)
      {
        Channel = channel;
        Title = title;
        Episode = episode;
        SeriesNum = seriesNum;
        EpisodeNum = episodeNum;
        EpisodePart = episodePart;
        Genre = genre;
        StartDate = startDate;
        EndDate = endDate;
      }
    }


    private string ShowExample(string strInput, int recType)
    {
      string strName = string.Empty;
      string strDirectory = string.Empty;
      Example[] example = new Example[2];
      example[0] = new Example("ProSieben", "Philadelphia", "unknown", "unknown", "unknown", "unknown", "Drama",
                               new DateTime(2005, 12, 23, 20, 15, 0), new DateTime(2005, 12, 23, 22, 45, 0));
      example[1] = new Example("ABC", "Friends", "Joey's Birthday", "4", "32", "part 1 of 1", "Comedy",
                               new DateTime(2005, 12, 23, 20, 15, 0), new DateTime(2005, 12, 23, 20, 45, 0));
      string strDefaultName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}",
                                            example[recType].Channel, example[recType].Title,
                                            example[recType].StartDate.Year, example[recType].StartDate.Month,
                                            example[recType].StartDate.Day,
                                            example[recType].StartDate.Hour,
                                            example[recType].StartDate.Minute,
                                            DateTime.Now.Minute, DateTime.Now.Second);
      if (strInput != string.Empty)
      {
        strInput = Util.Utils.ReplaceTag(strInput, "%channel%", example[recType].Channel, "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%title%", example[recType].Title, "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%name%", example[recType].Episode, "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%series%", example[recType].SeriesNum, "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%episode%", example[recType].EpisodeNum, "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%part%", example[recType].EpisodePart, "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%date%", example[recType].StartDate.ToShortDateString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%start%", example[recType].StartDate.ToShortTimeString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%end%", example[recType].EndDate.ToShortTimeString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%genre%", example[recType].Genre, "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startday%", example[recType].StartDate.Day.ToString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startmonth%", example[recType].StartDate.Month.ToString(),
                                         "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startyear%", example[recType].StartDate.Year.ToString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%starthh%", example[recType].StartDate.Hour.ToString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startmm%", example[recType].StartDate.Minute.ToString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%endday%", example[recType].EndDate.Day.ToString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%endmonth%", example[recType].EndDate.Month.ToString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%startyear%", example[recType].EndDate.Year.ToString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%endhh%", example[recType].EndDate.Hour.ToString(), "unknown");
        strInput = Util.Utils.ReplaceTag(strInput, "%endmm%", example[recType].EndDate.Minute.ToString(), "unknown");

        int index = strInput.LastIndexOf('\\');
        switch (index)
        {
          case -1:
            strName = strInput;
            break;
          case 0:
            strName = strInput.Substring(1);
            break;
          default:
            {
              strDirectory = "\\" + strInput.Substring(0, index);
              strName = strInput.Substring(index + 1);
            }
            break;
        }

        strDirectory = Util.Utils.MakeDirectoryPath(strDirectory);
        strName = Util.Utils.MakeFileName(strName);
      }
      if (strName == string.Empty)
      {
        strName = strDefaultName;
      }
      string strReturn = strDirectory;
      if (strDirectory != string.Empty)
      {
        strReturn += "\\";
      }
      strReturn += strName + ".dvr-ms";
      return strReturn;
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        startTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "prerecord", 5));
        endTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "postrecord", 5));
        cbDeleteWatchedShows.Checked = xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
        cbAddRecordingsToMovie.Checked = xmlreader.GetValueAsBool("capture", "addrecordingstomoviedatabase", true);
        formatString[0] = xmlreader.GetValueAsString("capture", "moviesformat", string.Empty);
        formatString[1] = xmlreader.GetValueAsString("capture", "seriesformat", string.Empty);
        cbBackToBack.Checked = xmlreader.GetValueAsBool("mytv", "automaticbacktoback", false);
      }
      comboBoxRecording.SelectedIndex = 0;
      textBoxSample.Text = ShowExample(formatString[comboBoxRecording.SelectedIndex], comboBoxRecording.SelectedIndex);


      comboDrives.Items.Clear();
      for (char drive = 'a'; drive <= 'z'; drive++)
      {
        string driveLetter = String.Format("{0}:", drive);
        if (Util.Utils.getDriveType(driveLetter) == 3)
        {
          comboDrives.Items.Add(driveLetter);
        }
      }
      comboDrives.SelectedIndex = 0;
      UpdateDriveInfo(false);
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("capture", "prerecord", startTextBox.Text);
        xmlwriter.SetValue("capture", "postrecord", endTextBox.Text);

        xmlwriter.SetValueAsBool("mytv", "automaticbacktoback", cbBackToBack.Checked);
        xmlwriter.SetValueAsBool("capture", "deletewatchedshows", cbDeleteWatchedShows.Checked);
        xmlwriter.SetValueAsBool("capture", "addrecordingstomoviedatabase", cbAddRecordingsToMovie.Checked);

        xmlwriter.SetValue("capture", "moviesformat", formatString[0]);
        xmlwriter.SetValue("capture", "seriesformat", formatString[1]);
      }
      UpdateDriveInfo(true);
    }

    private void startTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void endTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void textBoxFormat_TextChanged(object sender, EventArgs e)
    {
      formatString[comboBoxRecording.SelectedIndex] = textBoxFormat.Text;
      textBoxSample.Text = ShowExample(textBoxFormat.Text, comboBoxRecording.SelectedIndex);
    }

    private void comboBoxRecording_SelectedIndexChanged(object sender, EventArgs e)
    {
      textBoxFormat.Text = formatString[comboBoxRecording.SelectedIndex];
    }

    private void textBoxFormat_KeyPress(object sender, KeyPressEventArgs e)
    {
      if ((e.KeyChar == '/') || (e.KeyChar == ':') || (e.KeyChar == '*') ||
          (e.KeyChar == '?') || (e.KeyChar == '\"') || (e.KeyChar == '<') ||
          (e.KeyChar == '>') || (e.KeyChar == '|'))
      {
        e.Handled = true;
      }
    }

    private void UpdateDriveInfo(bool save)
    {
      string drive = (string) comboDrives.SelectedItem;
      ulong freeSpace = Util.Utils.GetFreeDiskSpace(drive);
      long totalSpace = Util.Utils.GetDiskSize(drive);
      lblFreeDiskSpace.Text = Util.Utils.GetSize((long) freeSpace);
      lblTotalSpace.Text = Util.Utils.GetSize((long) totalSpace);
      if (lblTotalSpace.Text == "0")
      {
        lblTotalSpace.Text = "Not available - WMI service not available";
      }

      if (save)
      {
        float percent = (float) trackBar1.Value;
        percent /= 100f;
        float quota = percent*((float) totalSpace);
        if (quota < (52428800f)) //50MB
        {
          quota = (52428800f); //50MB
          percent = (quota/((float) totalSpace))*100f;
          try
          {
            trackBar1.Value = (int) percent;
          }
          catch (ArgumentOutOfRangeException)
          {
            trackBar1.Value = 0;
          }
        }
        labelQuota.Text = Util.Utils.GetSize((long) quota);
        using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          long longQuota = (long) quota;
          longQuota /= 1024; // kbyte
          xmlwriter.SetValue("freediskspace", drive[0].ToString(), longQuota.ToString());
        }
      }
      else
      {
        using (Settings xmlReader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          string quotaText = xmlReader.GetValueAsString("freediskspace", drive[0].ToString(), "51200");
          float quota = (float) Int32.Parse(quotaText);
          if (quota < 51200)
          {
            quota = 51200f;
          }
          quota *= 1024f; //kbyte
          labelQuota.Text = Util.Utils.GetSize((long) quota);

          float percent = (quota/((float) totalSpace))*100f;
          try
          {
            trackBar1.Value = (int) percent;
          }
          catch (ArgumentOutOfRangeException)
          {
            trackBar1.Value = 0;
          }
        }
      }
    }

    private void trackBar1_Scroll(object sender, EventArgs e)
    {
      UpdateDriveInfo(true);
    }

    private void comboDrives_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateDriveInfo(false);
    }
  }
}