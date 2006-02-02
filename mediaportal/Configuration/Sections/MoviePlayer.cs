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

using DShowNET;
using DShowNET.Helper;
using DirectShowLib;

namespace MediaPortal.Configuration.Sections
{
	public class MoviePlayer : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private MediaPortal.UserInterface.Controls.MPButton parametersButton;
		private System.Windows.Forms.TextBox parametersTextBox;
		private System.Windows.Forms.Label label2;
		private MediaPortal.UserInterface.Controls.MPButton fileNameButton;
		private System.Windows.Forms.TextBox fileNameTextBox;
		private System.Windows.Forms.Label label1;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private System.Windows.Forms.ComboBox audioRendererComboBox;
		private System.Windows.Forms.Label label3;
		private MediaPortal.UserInterface.Controls.MPCheckBox externalPlayerCheckBox;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.ComboBox audioCodecComboBox;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox videoCodecComboBox;
    private System.Windows.Forms.Label label6;
		private System.ComponentModel.IContainer components = null;

		public MoviePlayer() : this("Movie Player")
		{
		}

		public MoviePlayer(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// 
			// Fetch available audio and video renderers
			//
			ArrayList availableAudioRenderers = FilterHelper.GetAudioRenderers(); 
      //
      // Populate video and audio codecs
      //
      ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);

      videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
      audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
      audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
    }

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				fileNameTextBox.Text = xmlreader.GetValueAsString("movieplayer", "path", "");
				parametersTextBox.Text = xmlreader.GetValueAsString("movieplayer", "arguments", "");

				externalPlayerCheckBox.Checked = xmlreader.GetValueAsBool("movieplayer", "internal", true);
				externalPlayerCheckBox.Checked = !externalPlayerCheckBox.Checked;

				audioRendererComboBox.SelectedItem = xmlreader.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
				 

        //
        // Set codecs

        audioCodecComboBox.SelectedItem = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
        videoCodecComboBox.SelectedItem = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlwriter.SetValue("movieplayer", "path", fileNameTextBox.Text);
				xmlwriter.SetValue("movieplayer", "arguments", parametersTextBox.Text);

				xmlwriter.SetValueAsBool("movieplayer", "internal", !externalPlayerCheckBox.Checked);
				
				xmlwriter.SetValue("movieplayer", "audiorenderer", audioRendererComboBox.Text);

	 
        //
        // Set codecs
        //
        xmlwriter.SetValue("movieplayer", "mpeg2audiocodec", audioCodecComboBox.Text);
        xmlwriter.SetValue("movieplayer", "mpeg2videocodec", videoCodecComboBox.Text);
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
      this.externalPlayerCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.parametersButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.parametersTextBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.fileNameButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.fileNameTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox(); 
      this.audioRendererComboBox = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.audioCodecComboBox = new System.Windows.Forms.ComboBox();
      this.videoCodecComboBox = new System.Windows.Forms.ComboBox();
      this.label5 = new System.Windows.Forms.Label();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.externalPlayerCheckBox);
      this.groupBox1.Controls.Add(this.parametersButton);
      this.groupBox1.Controls.Add(this.parametersTextBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.fileNameButton);
      this.groupBox1.Controls.Add(this.fileNameTextBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 136);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 112);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "External Player";
      // 
      // externalPlayerCheckBox
      // 
      this.externalPlayerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.externalPlayerCheckBox.Location = new System.Drawing.Point(16, 24);
      this.externalPlayerCheckBox.Name = "externalPlayerCheckBox";
      this.externalPlayerCheckBox.Size = new System.Drawing.Size(232, 16);
      this.externalPlayerCheckBox.TabIndex = 0;
      this.externalPlayerCheckBox.Text = "Use external player (replaces internal player)";
      this.externalPlayerCheckBox.CheckedChanged += new System.EventHandler(this.externalPlayerCheckBox_CheckedChanged);
      // 
      // parametersButton
      // 
      this.parametersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.parametersButton.Location = new System.Drawing.Point(384, 76);
      this.parametersButton.Name = "parametersButton";
      this.parametersButton.Size = new System.Drawing.Size(72, 22);
      this.parametersButton.TabIndex = 6;
      this.parametersButton.Text = "List";
      this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.Location = new System.Drawing.Point(168, 76);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(208, 20);
      this.parametersTextBox.TabIndex = 5;
      this.parametersTextBox.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 80);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 15);
      this.label2.TabIndex = 4;
      this.label2.Text = "Parameters:";
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.fileNameButton.Location = new System.Drawing.Point(384, 52);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(72, 22);
      this.fileNameButton.TabIndex = 3;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // fileNameTextBox
      // 
      this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameTextBox.Location = new System.Drawing.Point(168, 52);
      this.fileNameTextBox.Name = "fileNameTextBox";
      this.fileNameTextBox.Size = new System.Drawing.Size(208, 20);
      this.fileNameTextBox.TabIndex = 2;
      this.fileNameTextBox.Text = "";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 56);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 16);
      this.label1.TabIndex = 1;
      this.label1.Text = "Path/Filename:";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right))); 
      this.mpGroupBox1.Controls.Add(this.audioRendererComboBox);
      this.mpGroupBox1.Controls.Add(this.label3);
      this.mpGroupBox1.Controls.Add(this.label6);
      this.mpGroupBox1.Controls.Add(this.audioCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.videoCodecComboBox);
      this.mpGroupBox1.Controls.Add(this.label5);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox1.Location = new System.Drawing.Point(0, 0);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(472, 128);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Settings";  
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(168, 92);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioRendererComboBox.TabIndex = 7;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 96);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(88, 17);
      this.label3.TabIndex = 6;
      this.label3.Text = "Audio renderer:";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 24);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(72, 16);
      this.label6.TabIndex = 0;
      this.label6.Text = "Video codec:";
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 44);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.audioCodecComboBox.TabIndex = 3;
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 20);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(288, 21);
      this.videoCodecComboBox.TabIndex = 1;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 48);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(80, 16);
      this.label5.TabIndex = 2;
      this.label5.Text = "Audio codec:";
      // 
      // MoviePlayer
      // 
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.groupBox1);
      this.Name = "MoviePlayer";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void externalPlayerCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			fileNameTextBox.Enabled = fileNameButton.Enabled = parametersTextBox.Enabled = parametersButton.Enabled = externalPlayerCheckBox.Checked;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void fileNameButton_Click(object sender, System.EventArgs e)
		{
			using(openFileDialog = new OpenFileDialog())
			{
				openFileDialog.FileName = fileNameTextBox.Text;
				openFileDialog.CheckFileExists = true;
				openFileDialog.RestoreDirectory=true;
				openFileDialog.Filter= "exe files (*.exe)|*.exe";
				openFileDialog.FilterIndex = 0;
				openFileDialog.Title = "Select movie player";

				DialogResult dialogResult = openFileDialog.ShowDialog();

				if(dialogResult == DialogResult.OK)
				{
					fileNameTextBox.Text = openFileDialog.FileName;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void parametersButton_Click(object sender, System.EventArgs e)
		{
			ParameterForm parameters = new ParameterForm();

			parameters.AddParameter("%filename%", "Will be replaced by currently selected media file");

			if(parameters.ShowDialog(parametersButton) == DialogResult.OK)
			{
				parametersTextBox.Text += parameters.SelectedParameter;
			}
		}
	}
}

