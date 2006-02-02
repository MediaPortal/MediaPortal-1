#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

namespace MediaPortal.Configuration.Sections
{
	public class Music : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.Label label1;
		private MediaPortal.UserInterface.Controls.MPButton playlistButton;
		private System.Windows.Forms.TextBox playlistFolderTextBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox autoShuffleCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox repeatPlaylistCheckBox;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private MediaPortal.UserInterface.Controls.MPCheckBox showID3CheckBox;
		private System.Windows.Forms.ComboBox audioPlayerComboBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label2;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
    private System.Windows.Forms.ComboBox autoPlayComboBox;
    private Label labelAutoPlay;
		private System.ComponentModel.IContainer components = null;

    string[] autoPlayOptions = new string[] { "Autoplay, never ask", "Don't autoplay, never ask", "Ask every time a CD is inserted" };

		public Music() : this("Music")
		{
		}

		public Music(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Set available media players
			//
			audioPlayerComboBox.Items.AddRange(new string[] { "Windows Media Player 9",
																"DirectShow" });

      autoPlayComboBox.Items.Clear();
      autoPlayComboBox.Items.AddRange(autoPlayOptions);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "repeat", true);
				showID3CheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "showid3", false);
				autoShuffleCheckBox.Checked = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", true);

				string playListFolder=Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				playListFolder+=@"\My Playlists";
				playlistFolderTextBox.Text = xmlreader.GetValueAsString("music", "playlists", playListFolder);

				if(string.Compare(playlistFolderTextBox.Text, playListFolder) == 0)
				{
					if(System.IO.Directory.Exists(playListFolder) == false)
					{
						try
						{
							System.IO.Directory.CreateDirectory(playListFolder);
						}
						catch(Exception){}
					}
				}

				audioPlayerComboBox.Text = xmlreader.GetValueAsString("audioplayer", "player", "Windows Media Player 9");

        string autoPlayText = xmlreader.GetValueAsString("audioplayer", "autoplay", "Yes");

        switch (autoPlayText)
        {
          case "No": autoPlayComboBox.Text = autoPlayOptions[1];
            break;
          case "Ask": autoPlayComboBox.Text = autoPlayOptions[2];
            break;
          default: autoPlayComboBox.Text = autoPlayOptions[0];
            break;
        }
      }
		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("musicfiles", "repeat", repeatPlaylistCheckBox.Checked);
				xmlwriter.SetValueAsBool("musicfiles", "showid3", showID3CheckBox.Checked);
				xmlwriter.SetValueAsBool("musicfiles", "autoshuffle", autoShuffleCheckBox.Checked);

				xmlwriter.SetValue("music", "playlists", playlistFolderTextBox.Text);

				xmlwriter.SetValue("audioplayer", "player", audioPlayerComboBox.Text);

        string autoPlayText;

        if (autoPlayComboBox.Text == autoPlayOptions[1])
          autoPlayText = "No";
        else if (autoPlayComboBox.Text == autoPlayOptions[2])
          autoPlayText = "Ask";
        else
          autoPlayText = "Yes";

        xmlwriter.SetValue("audioplayer", "autoplay", autoPlayText);
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
      this.autoShuffleCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.repeatPlaylistCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.playlistButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.playlistFolderTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.showID3CheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.audioPlayerComboBox = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.autoPlayComboBox = new System.Windows.Forms.ComboBox();
      this.labelAutoPlay = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.mpGroupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.autoShuffleCheckBox);
      this.groupBox1.Controls.Add(this.repeatPlaylistCheckBox);
      this.groupBox1.Controls.Add(this.playlistButton);
      this.groupBox1.Controls.Add(this.playlistFolderTextBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 84);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 104);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Playlist Settings";
      // 
      // autoShuffleCheckBox
      // 
      this.autoShuffleCheckBox.AutoSize = true;
      this.autoShuffleCheckBox.Location = new System.Drawing.Point(168, 44);
      this.autoShuffleCheckBox.Name = "autoShuffleCheckBox";
      this.autoShuffleCheckBox.Size = new System.Drawing.Size(121, 17);
      this.autoShuffleCheckBox.TabIndex = 1;
      this.autoShuffleCheckBox.Text = "Auto shuffle playlists";
      // 
      // repeatPlaylistCheckBox
      // 
      this.repeatPlaylistCheckBox.AutoSize = true;
      this.repeatPlaylistCheckBox.Location = new System.Drawing.Point(168, 20);
      this.repeatPlaylistCheckBox.Name = "repeatPlaylistCheckBox";
      this.repeatPlaylistCheckBox.Size = new System.Drawing.Size(221, 17);
      this.repeatPlaylistCheckBox.TabIndex = 0;
      this.repeatPlaylistCheckBox.Text = "Repeat/loop music playlists (m3u, b4, pls)";
      // 
      // playlistButton
      // 
      this.playlistButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.playlistButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.playlistButton.Location = new System.Drawing.Point(384, 68);
      this.playlistButton.Name = "playlistButton";
      this.playlistButton.Size = new System.Drawing.Size(72, 22);
      this.playlistButton.TabIndex = 4;
      this.playlistButton.Text = "Browse";
      this.playlistButton.Click += new System.EventHandler(this.playlistButton_Click);
      // 
      // playlistFolderTextBox
      // 
      this.playlistFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.playlistFolderTextBox.Location = new System.Drawing.Point(168, 68);
      this.playlistFolderTextBox.Name = "playlistFolderTextBox";
      this.playlistFolderTextBox.Size = new System.Drawing.Size(208, 20);
      this.playlistFolderTextBox.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(15, 72);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(101, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Music playlist folder:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.label2);
      this.mpGroupBox1.Controls.Add(this.showID3CheckBox);
      this.mpGroupBox1.Controls.Add(this.audioPlayerComboBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 80);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "General Settings";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(106, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Internal music player:";
      // 
      // showID3CheckBox
      // 
      this.showID3CheckBox.AutoSize = true;
      this.showID3CheckBox.Location = new System.Drawing.Point(168, 20);
      this.showID3CheckBox.Name = "showID3CheckBox";
      this.showID3CheckBox.Size = new System.Drawing.Size(291, 17);
      this.showID3CheckBox.TabIndex = 0;
      this.showID3CheckBox.Text = "Load ID3 tags from file if it\'s not in music database (slow)";
      this.showID3CheckBox.CheckedChanged += new System.EventHandler(this.showID3CheckBox_CheckedChanged);
      // 
      // audioPlayerComboBox
      // 
      this.audioPlayerComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.audioPlayerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioPlayerComboBox.Location = new System.Drawing.Point(168, 44);
      this.audioPlayerComboBox.Name = "audioPlayerComboBox";
      this.audioPlayerComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioPlayerComboBox.TabIndex = 2;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(0, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(100, 23);
      this.label4.TabIndex = 0;
      // 
      // mpGroupBox2
      // 
      this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox2.Controls.Add(this.labelAutoPlay);
      this.mpGroupBox2.Controls.Add(this.autoPlayComboBox);
      this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox2.Location = new System.Drawing.Point(0, 192);
      this.mpGroupBox2.Name = "mpGroupBox2";
      this.mpGroupBox2.Size = new System.Drawing.Size(472, 64);
      this.mpGroupBox2.TabIndex = 2;
      this.mpGroupBox2.TabStop = false;
      this.mpGroupBox2.Text = "Autoplay";
      // 
      // autoPlayComboBox
      // 
      this.autoPlayComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.autoPlayComboBox.Location = new System.Drawing.Point(168, 24);
      this.autoPlayComboBox.Name = "autoPlayComboBox";
      this.autoPlayComboBox.Size = new System.Drawing.Size(288, 21);
      this.autoPlayComboBox.TabIndex = 1;
      // 
      // labelAutoPlay
      // 
      this.labelAutoPlay.AutoSize = true;
      this.labelAutoPlay.Location = new System.Drawing.Point(16, 28);
      this.labelAutoPlay.Name = "labelAutoPlay";
      this.labelAutoPlay.Size = new System.Drawing.Size(69, 13);
      this.labelAutoPlay.TabIndex = 0;
      this.labelAutoPlay.Text = "Autoplay CD:";
      // 
      // Music
      // 
      this.Controls.Add(this.mpGroupBox2);
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.groupBox1);
      this.Name = "Music";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      this.mpGroupBox2.ResumeLayout(false);
      this.mpGroupBox2.PerformLayout();
      this.ResumeLayout(false);

    }
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void playlistButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where music playlists will be stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = playlistFolderTextBox.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					playlistFolderTextBox.Text = folderBrowserDialog.SelectedPath;
				}
			}		
		}

    private void showID3CheckBox_CheckedChanged(object sender, System.EventArgs e)
    {
    
    }
		private void Yestext_Click(object sender, System.EventArgs e)
		{
		
		}

		private void label3_Click(object sender, System.EventArgs e)
		{
		
		}

	}
}

