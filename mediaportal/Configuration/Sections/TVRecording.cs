using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class TVRecording : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox folderNameTextBox;
		private System.Windows.Forms.Label folderNameLabel;
		private System.Windows.Forms.TextBox startTextBox;
		private System.Windows.Forms.TextBox endTextBox;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
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
			this.endTextBox = new System.Windows.Forms.TextBox();
			this.startTextBox = new System.Windows.Forms.TextBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.folderNameTextBox = new System.Windows.Forms.TextBox();
			this.folderNameLabel = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.endTextBox);
			this.groupBox1.Controls.Add(this.startTextBox);
			this.groupBox1.Controls.Add(this.browseButton);
			this.groupBox1.Controls.Add(this.folderNameTextBox);
			this.groupBox1.Controls.Add(this.folderNameLabel);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 200);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "TV Recording Settings";
			// 
			// endTextBox
			// 
			this.endTextBox.Location = new System.Drawing.Point(104, 120);
			this.endTextBox.MaxLength = 3;
			this.endTextBox.Name = "endTextBox";
			this.endTextBox.Size = new System.Drawing.Size(40, 20);
			this.endTextBox.TabIndex = 3;
			this.endTextBox.Text = "";
			this.endTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.endTextBox_KeyPress);
			// 
			// startTextBox
			// 
			this.startTextBox.Location = new System.Drawing.Point(104, 88);
			this.startTextBox.MaxLength = 3;
			this.startTextBox.Name = "startTextBox";
			this.startTextBox.Size = new System.Drawing.Size(40, 20);
			this.startTextBox.TabIndex = 2;
			this.startTextBox.Text = "";
			this.startTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.startTextBox_KeyPress);
			// 
			// browseButton
			// 
			this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.browseButton.Location = new System.Drawing.Point(304, 48);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(56, 20);
			this.browseButton.TabIndex = 1;
			this.browseButton.Text = "Browse";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// folderNameTextBox
			// 
			this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.folderNameTextBox.Location = new System.Drawing.Point(24, 48);
			this.folderNameTextBox.Name = "folderNameTextBox";
			this.folderNameTextBox.Size = new System.Drawing.Size(264, 20);
			this.folderNameTextBox.TabIndex = 0;
			this.folderNameTextBox.Text = "";
			// 
			// folderNameLabel
			// 
			this.folderNameLabel.Location = new System.Drawing.Point(16, 30);
			this.folderNameLabel.Name = "folderNameLabel";
			this.folderNameLabel.Size = new System.Drawing.Size(232, 18);
			this.folderNameLabel.TabIndex = 14;
			this.folderNameLabel.Text = "Folder where tv recordings will be saved:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 120);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 23);
			this.label4.TabIndex = 8;
			this.label4.Text = "Stop recording";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(144, 120);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(208, 23);
			this.label3.TabIndex = 11;
			this.label3.Text = "minute(s) after program stops";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(144, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(207, 23);
			this.label2.TabIndex = 10;
			this.label2.Text = "minute(s) before program starts";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 88);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 16);
			this.label1.TabIndex = 9;
			this.label1.Text = "Start recording";
			// 
			// TVRecording
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "TVRecording";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				folderNameTextBox.Text = xmlreader.GetValueAsString("capture", "recordingpath", "");
				startTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "prerecord", 1));
				endTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("capture", "postrecord", 1));				
			}						
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("capture", "recordingpath", folderNameTextBox.Text);
				xmlwriter.SetValue("capture", "prerecord", startTextBox.Text);
				xmlwriter.SetValue("capture", "postrecord", endTextBox.Text);
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

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where recordings should be stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
				}
			}		
		}
	}
}

