using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class TVProgramGuide : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox folderNameTextBox;
		private System.Windows.Forms.Label folderNameLabel;
		private System.Windows.Forms.TextBox compensateTextBox;
		private System.Windows.Forms.Label label2;
		private MediaPortal.UserInterface.Controls.MPCheckBox useTimeZoneCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox useColorCheckBox;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.ComponentModel.IContainer components = null;

		public TVProgramGuide() : this("Program Guide")
		{
		}

		public TVProgramGuide(string name) : base(name)
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
			this.useColorCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.compensateTextBox = new System.Windows.Forms.TextBox();
			this.useTimeZoneCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.folderNameTextBox = new System.Windows.Forms.TextBox();
			this.folderNameLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.useColorCheckBox);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 64);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "General Settings";
			// 
			// useColorCheckBox
			// 
			this.useColorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useColorCheckBox.Location = new System.Drawing.Point(16, 24);
			this.useColorCheckBox.Name = "useColorCheckBox";
			this.useColorCheckBox.Size = new System.Drawing.Size(308, 24);
			this.useColorCheckBox.TabIndex = 0;
			this.useColorCheckBox.Text = "Use colors in program guide";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.compensateTextBox);
			this.groupBox2.Controls.Add(this.useTimeZoneCheckBox);
			this.groupBox2.Controls.Add(this.browseButton);
			this.groupBox2.Controls.Add(this.folderNameTextBox);
			this.groupBox2.Controls.Add(this.folderNameLabel);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 80);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 128);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "XMLTV Settings";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(211, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 23);
			this.label2.TabIndex = 27;
			this.label2.Text = "hour(s)";
			// 
			// compensateTextBox
			// 
			this.compensateTextBox.Location = new System.Drawing.Point(168, 85);
			this.compensateTextBox.MaxLength = 3;
			this.compensateTextBox.Name = "compensateTextBox";
			this.compensateTextBox.Size = new System.Drawing.Size(40, 20);
			this.compensateTextBox.TabIndex = 26;
			this.compensateTextBox.Text = "";
			this.compensateTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.compensateTextBox_KeyPress);
			// 
			// useTimeZoneCheckBox
			// 
			this.useTimeZoneCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useTimeZoneCheckBox.Location = new System.Drawing.Point(16, 24);
			this.useTimeZoneCheckBox.Name = "useTimeZoneCheckBox";
			this.useTimeZoneCheckBox.Size = new System.Drawing.Size(240, 24);
			this.useTimeZoneCheckBox.TabIndex = 17;
			this.useTimeZoneCheckBox.Text = "Use time zone information from XMLTV";
			// 
			// browseButton
			// 
			this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.browseButton.Location = new System.Drawing.Point(369, 53);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(56, 20);
			this.browseButton.TabIndex = 16;
			this.browseButton.Text = "Browse";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// folderNameTextBox
			// 
			this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.folderNameTextBox.Location = new System.Drawing.Point(96, 53);
			this.folderNameTextBox.Name = "folderNameTextBox";
			this.folderNameTextBox.Size = new System.Drawing.Size(265, 20);
			this.folderNameTextBox.TabIndex = 15;
			this.folderNameTextBox.Text = "";
			// 
			// folderNameLabel
			// 
			this.folderNameLabel.Location = new System.Drawing.Point(16, 56);
			this.folderNameLabel.Name = "folderNameLabel";
			this.folderNameLabel.Size = new System.Drawing.Size(80, 23);
			this.folderNameLabel.TabIndex = 14;
			this.folderNameLabel.Text = "XMLTV folder";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 88);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(150, 23);
			this.label1.TabIndex = 6;
			this.label1.Text = "Compensate time zone with";
			// 
			// ProgramGuide
			// 
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "ProgramGuide";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				useColorCheckBox.Checked = xmlreader.GetValueAsBool("xmltv", "colors", true);

				useTimeZoneCheckBox.Checked = xmlreader.GetValueAsBool("xmltv", "usetimezone", true);
				compensateTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("xmltv", "timezonecorrection", 0));

				folderNameTextBox.Text = xmlreader.GetValueAsString("xmltv", "folder", "");
			}						
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("xmltv", "colors", useColorCheckBox.Checked);
				xmlwriter.SetValueAsBool("xmltv", "usetimezone", useTimeZoneCheckBox.Checked);

				xmlwriter.SetValue("xmltv", "timezonecorrection", compensateTextBox.Text);
				xmlwriter.SetValue("xmltv", "folder", folderNameTextBox.Text);
			}
		}

		private void compensateTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
      //
      // Allow only numbers, '-' and backspace.
      //
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8 && e.KeyChar != '-')
			{
				e.Handled = true;
			}
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where the XMLTV data is stored";
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

