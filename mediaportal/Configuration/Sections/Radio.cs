using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices; 

using DShowNET;

namespace MediaPortal.Configuration.Sections
{
	public class Radio : MediaPortal.Configuration.SectionSettings
	{
		protected System.Windows.Forms.GroupBox groupBox2;
		protected System.Windows.Forms.TextBox folderNameTextBox;
		protected System.Windows.Forms.Label folderNameLabel;
		protected System.Windows.Forms.Button browseFolderButton;
		protected System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		protected System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.Label label2;
		protected System.ComponentModel.IContainer components = null;

		public Radio() : this("Radio")
		{
		}

		public Radio(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

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
		protected void InitializeComponent()
		{
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.browseFolderButton = new System.Windows.Forms.Button();
      this.folderNameTextBox = new System.Windows.Forms.TextBox();
      this.folderNameLabel = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.browseFolderButton);
      this.groupBox2.Controls.Add(this.folderNameTextBox);
      this.groupBox2.Controls.Add(this.folderNameLabel);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 56);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Internet Radio Stream Settings (asx, pls, ...)";
      // 
      // browseFolderButton
      // 
      this.browseFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.browseFolderButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.browseFolderButton.Location = new System.Drawing.Point(384, 19);
      this.browseFolderButton.Name = "browseFolderButton";
      this.browseFolderButton.Size = new System.Drawing.Size(72, 22);
      this.browseFolderButton.TabIndex = 2;
      this.browseFolderButton.Text = "Browse";
      this.browseFolderButton.Click += new System.EventHandler(this.browseFolderButton_Click);
      // 
      // folderNameTextBox
      // 
      this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.folderNameTextBox.Location = new System.Drawing.Point(168, 20);
      this.folderNameTextBox.Name = "folderNameTextBox";
      this.folderNameTextBox.Size = new System.Drawing.Size(208, 20);
      this.folderNameTextBox.TabIndex = 1;
      this.folderNameTextBox.Text = "";
      // 
      // folderNameLabel
      // 
      this.folderNameLabel.Location = new System.Drawing.Point(16, 24);
      this.folderNameLabel.Name = "folderNameLabel";
      this.folderNameLabel.Size = new System.Drawing.Size(88, 16);
      this.folderNameLabel.TabIndex = 0;
      this.folderNameLabel.Text = "Streams folder:";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Name = "label2";
      this.label2.TabIndex = 0;
      // 
      // Radio
      // 
      this.Controls.Add(this.groupBox2);
      this.Name = "Radio";
      this.Size = new System.Drawing.Size(472, 408);
      this.Load += new System.EventHandler(this.Radio_Load);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

    public override void OnSectionActivated()
    {
     
    }
		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				folderNameTextBox.Text = xmlreader.GetValueAsString("radio", "folder", "");

			}
		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("radio", "folder", folderNameTextBox.Text);
			}
		}


		protected void browseFolderButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where stream playlists will be stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
				}
			}					
		}


		private void Radio_Load(object sender, System.EventArgs e)
		{
		
		}
	}
}

