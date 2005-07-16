using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Management; 

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
      this.groupBox1.SuspendLayout();
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
      this.endTextBox.Text = "";
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
      this.startTextBox.Text = "";
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
      // TVRecording
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "TVRecording";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				startTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "prerecord", 5));
				endTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "postrecord", 5));				
				cbDeleteWatchedShows.Checked= xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
				cbAddRecordingsToMovie.Checked= xmlreader.GetValueAsBool("capture", "addrecordingstomoviedatabase", true);

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


		
	}
}

