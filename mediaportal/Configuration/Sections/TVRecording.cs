/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Management;
using MediaPortal.Util;

namespace MediaPortal.Configuration.Sections
{
	public class TVRecording : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox startTextBox;
		private System.Windows.Forms.TextBox endTextBox;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.CheckBox cbDeleteWatchedShows;
		private System.Windows.Forms.CheckBox cbAddRecordingsToMovie;
    private GroupBox groupBox2;
    private Label label42;
    private Label label41;
    private Label label40;
    private TextBox textBoxRecFileFormat;
    private Label label39;
    private Label label38;
    private TextBox textBoxRecDirectoryFormat;
    private Label label5;
    private Label labelSample;
    private Label label6;
		private System.ComponentModel.IContainer components = null;

		public TVRecording() : this("Recording")
		{
		}

		public TVRecording(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbAddRecordingsToMovie = new System.Windows.Forms.CheckBox();
      this.endTextBox = new System.Windows.Forms.TextBox();
      this.startTextBox = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.cbDeleteWatchedShows = new System.Windows.Forms.CheckBox();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.labelSample = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.label42 = new System.Windows.Forms.Label();
      this.label41 = new System.Windows.Forms.Label();
      this.label40 = new System.Windows.Forms.Label();
      this.textBoxRecFileFormat = new System.Windows.Forms.TextBox();
      this.label39 = new System.Windows.Forms.Label();
      this.label38 = new System.Windows.Forms.Label();
      this.textBoxRecDirectoryFormat = new System.Windows.Forms.TextBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.cbAddRecordingsToMovie);
      this.groupBox1.Controls.Add(this.endTextBox);
      this.groupBox1.Controls.Add(this.startTextBox);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.cbDeleteWatchedShows);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 136);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // cbAddRecordingsToMovie
      // 
      this.cbAddRecordingsToMovie.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbAddRecordingsToMovie.Location = new System.Drawing.Point(16, 104);
      this.cbAddRecordingsToMovie.Name = "cbAddRecordingsToMovie";
      this.cbAddRecordingsToMovie.Size = new System.Drawing.Size(184, 16);
      this.cbAddRecordingsToMovie.TabIndex = 7;
      this.cbAddRecordingsToMovie.Text = "Add recordings to movie database";
      // 
      // endTextBox
      // 
      this.endTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.endTextBox.Location = new System.Drawing.Point(112, 44);
      this.endTextBox.MaxLength = 3;
      this.endTextBox.Name = "endTextBox";
      this.endTextBox.Size = new System.Drawing.Size(176, 20);
      this.endTextBox.TabIndex = 4;
      this.endTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.endTextBox_KeyPress);
      // 
      // startTextBox
      // 
      this.startTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.startTextBox.Location = new System.Drawing.Point(112, 20);
      this.startTextBox.MaxLength = 3;
      this.startTextBox.Name = "startTextBox";
      this.startTextBox.Size = new System.Drawing.Size(176, 20);
      this.startTextBox.TabIndex = 1;
      this.startTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.startTextBox_KeyPress);
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 48);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(88, 16);
      this.label4.TabIndex = 3;
      this.label4.Text = "Stop recording";
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Location = new System.Drawing.Point(296, 48);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(160, 16);
      this.label3.TabIndex = 5;
      this.label3.Text = "minute(s) after program ends.";
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.Location = new System.Drawing.Point(296, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(168, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "minute(s) before program starts.";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(88, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Start recording";
      // 
      // cbDeleteWatchedShows
      // 
      this.cbDeleteWatchedShows.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cbDeleteWatchedShows.Location = new System.Drawing.Point(16, 80);
      this.cbDeleteWatchedShows.Name = "cbDeleteWatchedShows";
      this.cbDeleteWatchedShows.Size = new System.Drawing.Size(232, 16);
      this.cbDeleteWatchedShows.TabIndex = 6;
      this.cbDeleteWatchedShows.Text = "Automaticly delete recordings after watching";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.labelSample);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.label42);
      this.groupBox2.Controls.Add(this.label41);
      this.groupBox2.Controls.Add(this.label40);
      this.groupBox2.Controls.Add(this.textBoxRecFileFormat);
      this.groupBox2.Controls.Add(this.label39);
      this.groupBox2.Controls.Add(this.label38);
      this.groupBox2.Controls.Add(this.textBoxRecDirectoryFormat);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(0, 144);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 216);
      this.groupBox2.TabIndex = 63;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Filenames:";
      // 
      // labelSample
      // 
      this.labelSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.labelSample.Location = new System.Drawing.Point(80, 104);
      this.labelSample.Name = "labelSample";
      this.labelSample.Size = new System.Drawing.Size(376, 40);
      this.labelSample.TabIndex = 67;
      this.labelSample.Text = "label4";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(16, 104);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(50, 13);
      this.label6.TabIndex = 66;
      this.label6.Text = "Example:";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(80, 24);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(368, 13);
      this.label5.TabIndex = 65;
      this.label5.Text = "Leave empty for classic naming. Use blockquotes [] to specify optional fields.";
      // 
      // label42
      // 
      this.label42.AutoSize = true;
      this.label42.Location = new System.Drawing.Point(352, 160);
      this.label42.Name = "label42";
      this.label42.Size = new System.Drawing.Size(97, 39);
      this.label42.TabIndex = 64;
      this.label42.Text = "%date% = date\r\n%start% = start time\r\n%end% = end time";
      // 
      // label41
      // 
      this.label41.AutoSize = true;
      this.label41.Location = new System.Drawing.Point(184, 160);
      this.label41.Name = "label41";
      this.label41.Size = new System.Drawing.Size(147, 39);
      this.label41.TabIndex = 63;
      this.label41.Text = "%series% = series number\r\n%episode% = episode number\r\n%part% = episode part";
      // 
      // label40
      // 
      this.label40.AutoSize = true;
      this.label40.Location = new System.Drawing.Point(40, 160);
      this.label40.Name = "label40";
      this.label40.Size = new System.Drawing.Size(127, 39);
      this.label40.TabIndex = 62;
      this.label40.Text = "%channel% = chnl. name\r\n%title% = title\r\n%name% = episode name";
      // 
      // textBoxRecFileFormat
      // 
      this.textBoxRecFileFormat.Location = new System.Drawing.Point(80, 48);
      this.textBoxRecFileFormat.Name = "textBoxRecFileFormat";
      this.textBoxRecFileFormat.Size = new System.Drawing.Size(376, 20);
      this.textBoxRecFileFormat.TabIndex = 61;
      this.textBoxRecFileFormat.TextChanged += new System.EventHandler(this.textBoxRecFileFormat_TextChanged);
      // 
      // label39
      // 
      this.label39.AutoSize = true;
      this.label39.Location = new System.Drawing.Point(16, 72);
      this.label39.Name = "label39";
      this.label39.Size = new System.Drawing.Size(60, 13);
      this.label39.TabIndex = 60;
      this.label39.Text = "Directories:";
      // 
      // label38
      // 
      this.label38.AutoSize = true;
      this.label38.Location = new System.Drawing.Point(16, 48);
      this.label38.Name = "label38";
      this.label38.Size = new System.Drawing.Size(31, 13);
      this.label38.TabIndex = 59;
      this.label38.Text = "Files:";
      // 
      // textBoxRecDirectoryFormat
      // 
      this.textBoxRecDirectoryFormat.Location = new System.Drawing.Point(80, 72);
      this.textBoxRecDirectoryFormat.Name = "textBoxRecDirectoryFormat";
      this.textBoxRecDirectoryFormat.Size = new System.Drawing.Size(376, 20);
      this.textBoxRecDirectoryFormat.TabIndex = 58;
      this.textBoxRecDirectoryFormat.TextChanged += new System.EventHandler(this.textBoxRecDirectoryFormat_TextChanged);
      // 
      // TVRecording
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "TVRecording";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }
		#endregion

    void ShowExample()
    {
      string channel = "ABC";
      string title = "Friends";
      string episode = "Joey's Birthday";
      string seriesNum = "4";
      string episodeNum = "32";
      string episodePart = "part 1 of 1";
      DateTime startDate = DateTime.Parse("20.12.2005 20:15");
      DateTime endDate = DateTime.Parse("20.12.2005 20:45");

      string strName = string.Empty;
      string strDirectory = string.Empty;

      string recDirFormat = textBoxRecDirectoryFormat.Text;

      strName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}",
                                  channel, title,
                                  startDate.Year, startDate.Month, startDate.Day,
                                  startDate.Hour,
                                  startDate.Minute,
                                  DateTime.Now.Minute, DateTime.Now.Second);

      if (recDirFormat != string.Empty)
      {
        strDirectory = Utils.ReplaceTag(recDirFormat, "%channel%", channel, "unknown");
        strDirectory = Utils.ReplaceTag(strDirectory, "%title%", title, "unknown");
        strDirectory = Utils.ReplaceTag(strDirectory, "%name%", episode, "unknown");
        strDirectory = Utils.ReplaceTag(strDirectory, "%series%", seriesNum, "unknown");
        strDirectory = Utils.ReplaceTag(strDirectory, "%episode%", episodeNum, "unknown");
        strDirectory = Utils.ReplaceTag(strDirectory, "%part%", episodePart, "unknown");
        strDirectory = Utils.ReplaceTag(strDirectory, "%date%", startDate.ToShortDateString(), "unknown");
        strDirectory = Utils.ReplaceTag(strDirectory, "%start%", startDate.ToShortTimeString(), "unknown");
        strDirectory = Utils.ReplaceTag(strDirectory, "%end%", endDate.ToShortTimeString(), "unknown");
        strDirectory = Utils.MakeDirectoryPath(strDirectory);
      }

      string recFileFormat = textBoxRecFileFormat.Text;

      if (recFileFormat != string.Empty)
      {
        strName = Utils.ReplaceTag(recFileFormat, "%channel%", channel, "unknown");
        strName = Utils.ReplaceTag(strName, "%title%", title, "unknown");
        strName = Utils.ReplaceTag(strName, "%name%", episode, "unknown");
        strName = Utils.ReplaceTag(strName, "%series%", seriesNum, "unknown");
        strName = Utils.ReplaceTag(strName, "%episode%", episodeNum, "unknown");
        strName = Utils.ReplaceTag(strName, "%part%", episodePart, "unknown");
        strName = Utils.ReplaceTag(strName, "%date%", startDate.ToShortDateString(), "unknown");
        strName = Utils.ReplaceTag(strName, "%start%", startDate.ToShortTimeString(), "unknown");
        strName = Utils.ReplaceTag(strName, "%end%", endDate.ToShortTimeString(), "unknown");
        strName = Utils.MakeFileName(strName);
      }

      labelSample.Text = strDirectory;
      if (strDirectory != string.Empty)
        labelSample.Text += "\\";
      labelSample.Text += strName + ".dvr-ms";
    }

		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				startTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "prerecord", 5));
				endTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "postrecord", 5));				
				cbDeleteWatchedShows.Checked= xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
				cbAddRecordingsToMovie.Checked= xmlreader.GetValueAsBool("capture", "addrecordingstomoviedatabase", true);
        textBoxRecFileFormat.Text = xmlreader.GetValueAsString("capture", "recordingsfileformat", string.Empty);
        textBoxRecDirectoryFormat.Text = xmlreader.GetValueAsString("capture", "recordingsdirectoryformat", string.Empty);
			}		
		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("capture", "prerecord", startTextBox.Text);
				xmlwriter.SetValue("capture", "postrecord", endTextBox.Text);

				xmlwriter.SetValueAsBool("capture", "deletewatchedshows", cbDeleteWatchedShows.Checked);
				xmlwriter.SetValueAsBool("capture", "addrecordingstomoviedatabase", cbAddRecordingsToMovie.Checked);

        xmlwriter.SetValue("capture", "recordingsfileformat", textBoxRecFileFormat.Text);
        xmlwriter.SetValue("capture", "recordingsdirectoryformat", textBoxRecDirectoryFormat.Text);
			}
		}

		private void startTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}
		}

		private void endTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}
		}

    private void textBoxRecFileFormat_TextChanged(object sender, EventArgs e)
    {
      ShowExample();
    }

    private void textBoxRecDirectoryFormat_TextChanged(object sender, EventArgs e)
    {
      ShowExample();
    }
	}
}

