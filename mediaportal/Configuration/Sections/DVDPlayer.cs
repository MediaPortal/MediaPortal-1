using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class DVDPlayer : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox fileNameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button fileNameButton;
		private System.Windows.Forms.Button parametersButton;
		private System.Windows.Forms.TextBox parametersTextBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox internalPlayerCheckBox;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private MediaPortal.UserInterface.Controls.MPCheckBox autoPlayCheckBox;
		private System.ComponentModel.IContainer components = null;

		public DVDPlayer() : this("DVD Player")
		{
		}

		public DVDPlayer(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				fileNameTextBox.Text = xmlreader.GetValueAsString("dvdplayer", "path", @"");
				parametersTextBox.Text = xmlreader.GetValueAsString("dvdplayer","arguments", "");
        autoPlayCheckBox.Checked=xmlreader.GetValueAsBool("dvdplayer", "autoplay", true);

				//
				// Fake a check changed to force a CheckChanged event
				//
				internalPlayerCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "internal", true);
				internalPlayerCheckBox.Checked = !internalPlayerCheckBox.Checked;
			}
		}

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("dvdplayer", "path", fileNameTextBox.Text);
        xmlwriter.SetValue("dvdplayer","arguments", parametersTextBox.Text);

        xmlwriter.SetValueAsBool("dvdplayer", "internal", !internalPlayerCheckBox.Checked);
        xmlwriter.SetValueAsBool("dvdplayer", "autoplay", autoPlayCheckBox.Checked);
      }
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void internalPlayerCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			fileNameTextBox.Enabled = parametersTextBox.Enabled = fileNameButton.Enabled = parametersButton.Enabled = internalPlayerCheckBox.Checked;
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.internalPlayerCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.parametersButton = new System.Windows.Forms.Button();
      this.parametersTextBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.fileNameButton = new System.Windows.Forms.Button();
      this.fileNameTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.button2 = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.autoPlayCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // internalPlayerCheckBox
      // 
      this.internalPlayerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.internalPlayerCheckBox.Location = new System.Drawing.Point(16, 24);
      this.internalPlayerCheckBox.Name = "internalPlayerCheckBox";
      this.internalPlayerCheckBox.Size = new System.Drawing.Size(200, 24);
      this.internalPlayerCheckBox.TabIndex = 0;
      this.internalPlayerCheckBox.Text = "Use external player";
      this.internalPlayerCheckBox.CheckedChanged += new System.EventHandler(this.internalPlayerCheckBox_CheckedChanged);
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.internalPlayerCheckBox);
      this.mpGroupBox1.Controls.Add(this.parametersButton);
      this.mpGroupBox1.Controls.Add(this.parametersTextBox);
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.Controls.Add(this.fileNameButton);
      this.mpGroupBox1.Controls.Add(this.fileNameTextBox);
      this.mpGroupBox1.Controls.Add(this.label1);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox1.Location = new System.Drawing.Point(8, 8);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(440, 120);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "General settings";
      // 
      // parametersButton
      // 
      this.parametersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.parametersButton.Location = new System.Drawing.Point(366, 78);
      this.parametersButton.Name = "parametersButton";
      this.parametersButton.Size = new System.Drawing.Size(58, 20);
      this.parametersButton.TabIndex = 5;
      this.parametersButton.Text = "List";
      this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.Location = new System.Drawing.Point(96, 78);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(264, 20);
      this.parametersTextBox.TabIndex = 4;
      this.parametersTextBox.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 81);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(80, 23);
      this.label2.TabIndex = 3;
      this.label2.Text = "Parameters";
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.fileNameButton.Location = new System.Drawing.Point(366, 53);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(58, 20);
      this.fileNameButton.TabIndex = 2;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // fileNameTextBox
      // 
      this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameTextBox.Location = new System.Drawing.Point(96, 53);
      this.fileNameTextBox.Name = "fileNameTextBox";
      this.fileNameTextBox.Size = new System.Drawing.Size(264, 20);
      this.fileNameTextBox.TabIndex = 1;
      this.fileNameTextBox.Text = "";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 56);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "Filename";
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(0, 0);
      this.button2.Name = "button2";
      this.button2.TabIndex = 0;
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(0, 0);
      this.textBox1.Name = "textBox1";
      this.textBox1.TabIndex = 0;
      this.textBox1.Text = "";
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.autoPlayCheckBox);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox2.Location = new System.Drawing.Point(8, 136);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(440, 64);
      this.mpGroupBox2.TabIndex = 3;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Autoplay";
      // 
      // autoPlayCheckBox
      // 
      this.autoPlayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.autoPlayCheckBox.Location = new System.Drawing.Point(16, 24);
      this.autoPlayCheckBox.Name = "autoPlayCheckBox";
      this.autoPlayCheckBox.Size = new System.Drawing.Size(264, 24);
      this.autoPlayCheckBox.TabIndex = 8;
      this.autoPlayCheckBox.Text = "Autoplay DVDs";
      // 
      // DVDPlayer
      // 
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Name = "DVDPlayer";
      this.Size = new System.Drawing.Size(456, 440);
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void parametersButton_Click(object sender, System.EventArgs e)
		{
			ParameterForm parameters = new ParameterForm();

			parameters.AddParameter("%filename%", "This will be replaced by the selected media file");

			if(parameters.ShowDialog(parametersButton) == DialogResult.OK)
			{
				parametersTextBox.Text += parameters.SelectedParameter;
			}		
		}

		private void fileNameButton_Click(object sender, System.EventArgs e)
		{
			using(openFileDialog = new OpenFileDialog())
			{
				openFileDialog.FileName = fileNameTextBox.Text;
				openFileDialog.CheckFileExists = true;
				openFileDialog.RestoreDirectory=true;
				openFileDialog.Filter= "exe files (*.exe)|*.exe";
				openFileDialog.FilterIndex = 0;
				openFileDialog.Title = "Select DVD player";

				DialogResult dialogResult = openFileDialog.ShowDialog();

				if(dialogResult == DialogResult.OK)
				{
					fileNameTextBox.Text = openFileDialog.FileName;
				}
			}		
		}
	}
}

