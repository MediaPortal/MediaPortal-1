using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace MediaPortal.Configuration.Sections
{
	public class Movies : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Button fileNameButton;
		private System.Windows.Forms.TextBox folderNameTextBox;
		private System.Windows.Forms.Label folderNameLabel;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private MediaPortal.UserInterface.Controls.MPCheckBox repeatPlaylistCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox showSubtitlesCheckBox;
		private System.Windows.Forms.Button subtitlesButton;
		private System.Windows.Forms.TextBox subtitlesFontTextBox;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.FontDialog fontDialog;
		private System.ComponentModel.IContainer components = null;

		string fontName;
		string fontColor;
		bool fontIsBold;
		private System.Windows.Forms.TextBox dropShadowTextBox;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
		private System.Windows.Forms.TextBox displayTimoutTextBox;
		private System.Windows.Forms.Label label5;
		int fontSize;
    private System.Windows.Forms.ComboBox defaultZoomModeComboBox;
    private System.Windows.Forms.Label label1;

    string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };

		public Movies() : this("Movies")
		{
		}

		public Movies(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
		}

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("movies", "repeat", true);
				folderNameTextBox.Text = xmlreader.GetValueAsString("movies", "playlists", "");

				showSubtitlesCheckBox.Checked = xmlreader.GetValueAsBool("subtitles", "enabled", true);

				dropShadowTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("subtitles", "shadow", 5));
				displayTimoutTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 0));

				//
				// Get font settings
				//
				fontName	= xmlreader.GetValueAsString("subtitles", "fontface", "Arial");
				fontColor	= xmlreader.GetValueAsString("subtitles", "color", "ffffff");
				fontIsBold	= xmlreader.GetValueAsBool("subtitles", "bold", true);
				fontSize	= xmlreader.GetValueAsInt("subtitles", "fontsize", 18);

				subtitlesFontTextBox.Text  = String.Format("{0} {1}{2}", fontName, fontSize, fontIsBold ? ", Bold" : "");

				//
				// Try to parse the specified color into a valid color
				//
				if (fontColor != null && fontColor.Length > 0)
				{
					try
					{
						int rgbColor = Int32.Parse(fontColor, NumberStyles.HexNumber);
						subtitlesFontTextBox.BackColor = Color.Black;
						subtitlesFontTextBox.ForeColor = Color.FromArgb(rgbColor);
					}
					catch {}
				}

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("movieplayer","defaultar", "original");

        for(int index = 0; index < aspectRatio.Length; index++)
        {
          if(aspectRatio[index].Equals(defaultAspectRatio))
          {
            defaultZoomModeComboBox.SelectedIndex = index;
            break;
          }
        }
      }		
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("movies", "repeat", repeatPlaylistCheckBox.Checked);
				xmlwriter.SetValue("movies", "playlists", folderNameTextBox.Text);

				xmlwriter.SetValueAsBool("subtitles", "enabled", showSubtitlesCheckBox.Checked);
				xmlwriter.SetValue("subtitles", "shadow", dropShadowTextBox.Text);
				
				xmlwriter.SetValue("movieplayer", "osdtimeout", displayTimoutTextBox.Text);

				xmlwriter.SetValue("subtitles", "fontface", fontName);
				xmlwriter.SetValue("subtitles", "color", fontColor);
				xmlwriter.SetValueAsBool("subtitles", "bold", fontIsBold);
				xmlwriter.SetValue("subtitles", "fontsize", fontSize);
      
        xmlwriter.SetValue("movieplayer","defaultar", aspectRatio[defaultZoomModeComboBox.SelectedIndex]);
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.fileNameButton = new System.Windows.Forms.Button();
      this.folderNameTextBox = new System.Windows.Forms.TextBox();
      this.repeatPlaylistCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.folderNameLabel = new System.Windows.Forms.Label();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.dropShadowTextBox = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.showSubtitlesCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.subtitlesButton = new System.Windows.Forms.Button();
      this.subtitlesFontTextBox = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.fontDialog = new System.Windows.Forms.FontDialog();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.displayTimoutTextBox = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.defaultZoomModeComboBox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.defaultZoomModeComboBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.fileNameButton);
      this.groupBox1.Controls.Add(this.folderNameTextBox);
      this.groupBox1.Controls.Add(this.repeatPlaylistCheckBox);
      this.groupBox1.Controls.Add(this.folderNameLabel);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 128);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "General settings";
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.fileNameButton.Location = new System.Drawing.Point(366, 53);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(56, 20);
      this.fileNameButton.TabIndex = 10;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // folderNameTextBox
      // 
      this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.folderNameTextBox.Location = new System.Drawing.Point(96, 53);
      this.folderNameTextBox.Name = "folderNameTextBox";
      this.folderNameTextBox.Size = new System.Drawing.Size(264, 20);
      this.folderNameTextBox.TabIndex = 9;
      this.folderNameTextBox.Text = "";
      // 
      // repeatPlaylistCheckBox
      // 
      this.repeatPlaylistCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.repeatPlaylistCheckBox.Location = new System.Drawing.Point(16, 24);
      this.repeatPlaylistCheckBox.Name = "repeatPlaylistCheckBox";
      this.repeatPlaylistCheckBox.Size = new System.Drawing.Size(264, 24);
      this.repeatPlaylistCheckBox.TabIndex = 7;
      this.repeatPlaylistCheckBox.Text = "Repeat playlists";
      // 
      // folderNameLabel
      // 
      this.folderNameLabel.Location = new System.Drawing.Point(16, 56);
      this.folderNameLabel.Name = "folderNameLabel";
      this.folderNameLabel.Size = new System.Drawing.Size(80, 23);
      this.folderNameLabel.TabIndex = 0;
      this.folderNameLabel.Text = "Playlist folder";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.dropShadowTextBox);
      this.mpGroupBox1.Controls.Add(this.label4);
      this.mpGroupBox1.Controls.Add(this.showSubtitlesCheckBox);
      this.mpGroupBox1.Controls.Add(this.subtitlesButton);
      this.mpGroupBox1.Controls.Add(this.subtitlesFontTextBox);
      this.mpGroupBox1.Controls.Add(this.label6);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox1.Location = new System.Drawing.Point(8, 144);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(440, 120);
      this.mpGroupBox1.TabIndex = 3;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Subtitles";
      // 
      // dropShadowTextBox
      // 
      this.dropShadowTextBox.Location = new System.Drawing.Point(160, 78);
      this.dropShadowTextBox.Name = "dropShadowTextBox";
      this.dropShadowTextBox.Size = new System.Drawing.Size(40, 20);
      this.dropShadowTextBox.TabIndex = 25;
      this.dropShadowTextBox.Text = "";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 81);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(150, 23);
      this.label4.TabIndex = 24;
      this.label4.Text = "Drop shadow (pixels)";
      // 
      // showSubtitlesCheckBox
      // 
      this.showSubtitlesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.showSubtitlesCheckBox.Location = new System.Drawing.Point(16, 24);
      this.showSubtitlesCheckBox.Name = "showSubtitlesCheckBox";
      this.showSubtitlesCheckBox.Size = new System.Drawing.Size(264, 24);
      this.showSubtitlesCheckBox.TabIndex = 23;
      this.showSubtitlesCheckBox.Text = "Show subtitles";
      // 
      // subtitlesButton
      // 
      this.subtitlesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.subtitlesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.subtitlesButton.Location = new System.Drawing.Point(368, 53);
      this.subtitlesButton.Name = "subtitlesButton";
      this.subtitlesButton.Size = new System.Drawing.Size(56, 20);
      this.subtitlesButton.TabIndex = 18;
      this.subtitlesButton.Text = "Browse";
      this.subtitlesButton.Click += new System.EventHandler(this.subtitlesButton_Click);
      // 
      // subtitlesFontTextBox
      // 
      this.subtitlesFontTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.subtitlesFontTextBox.Location = new System.Drawing.Point(160, 53);
      this.subtitlesFontTextBox.Name = "subtitlesFontTextBox";
      this.subtitlesFontTextBox.ReadOnly = true;
      this.subtitlesFontTextBox.Size = new System.Drawing.Size(200, 20);
      this.subtitlesFontTextBox.TabIndex = 17;
      this.subtitlesFontTextBox.Text = "";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 56);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(150, 23);
      this.label6.TabIndex = 16;
      this.label6.Text = "Subtitle display font";
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.displayTimoutTextBox);
      this.mpGroupBox2.Controls.Add(this.label5);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox2.Location = new System.Drawing.Point(8, 272);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(440, 72);
      this.mpGroupBox2.TabIndex = 4;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "OnScreen Display (OSD)";
      // 
      // displayTimoutTextBox
      // 
      this.displayTimoutTextBox.Location = new System.Drawing.Point(160, 27);
      this.displayTimoutTextBox.Name = "displayTimoutTextBox";
      this.displayTimoutTextBox.Size = new System.Drawing.Size(40, 20);
      this.displayTimoutTextBox.TabIndex = 16;
      this.displayTimoutTextBox.Text = "";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 30);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(150, 23);
      this.label5.TabIndex = 15;
      this.label5.Text = "Display timeout (seconds)";
      // 
      // defaultZoomModeComboBox
      // 
      this.defaultZoomModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultZoomModeComboBox.Items.AddRange(new object[] {
                                                                 "Normal",
                                                                 "Original Source Format",
                                                                 "Stretch",
                                                                 "Zoom",
                                                                 "4:3 Letterbox",
                                                                 "4:3 Pan and scan"});
      this.defaultZoomModeComboBox.Location = new System.Drawing.Point(168, 83);
      this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
      this.defaultZoomModeComboBox.Size = new System.Drawing.Size(256, 21);
      this.defaultZoomModeComboBox.TabIndex = 34;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 87);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(150, 23);
      this.label1.TabIndex = 33;
      this.label1.Text = "Default zoom mode";
      // 
      // Movies
      // 
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.groupBox1);
      this.Name = "Movies";
      this.Size = new System.Drawing.Size(456, 440);
      this.groupBox1.ResumeLayout(false);
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
		private void fileNameButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where movie playlists will be stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void subtitlesButton_Click(object sender, System.EventArgs e)
		{
			using(fontDialog = new FontDialog())
			{
				fontDialog.AllowScriptChange = false;
				fontDialog.ShowColor = true;
				fontDialog.FontMustExist = true;
				fontDialog.ShowEffects = true;

				fontDialog.Font = new Font(fontName, (float)fontSize, fontIsBold ? FontStyle.Bold : FontStyle.Regular);

				if(fontColor != null && fontColor.Length > 0)
					fontDialog.Color = subtitlesFontTextBox.BackColor;

				DialogResult dialogResult = fontDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					fontName	= fontDialog.Font.Name;
					fontSize	= (int)fontDialog.Font.Size;
					fontIsBold	= fontDialog.Font.Style == FontStyle.Bold;
					fontColor	= String.Format("{0:x}", fontDialog.Color.ToArgb());

					subtitlesFontTextBox.Text  = String.Format("{0} {1}{2}", fontName, fontSize, fontIsBold ? ", Bold" : "");

					//
					// Try to parse the specified color into a valid color
					//
					if (fontColor != null && fontColor.Length > 0)
					{
						try
						{
							int rgbColor = Int32.Parse(fontColor, NumberStyles.HexNumber);
							subtitlesFontTextBox.BackColor = Color.Black;
							subtitlesFontTextBox.ForeColor = Color.FromArgb(rgbColor);
						}
						catch {}
					}

				}
			}
		}
	}
}

