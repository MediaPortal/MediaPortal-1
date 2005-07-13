using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using System.Runtime.InteropServices;

using DShowNET;
using DShowNET.Device;

namespace MediaPortal.Configuration.Sections
{

	public class DVDCodec : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Label audioRendererLabel;
		private System.Windows.Forms.Label audioCodecLabel;
		private System.Windows.Forms.Label videoCodecLabel;
		private System.Windows.Forms.Label dvdNavigatorLabel;
		private System.Windows.Forms.ComboBox audioRendererComboBox;
		private System.Windows.Forms.ComboBox audioCodecComboBox;
		private System.Windows.Forms.ComboBox videoCodecComboBox;
		private System.Windows.Forms.ComboBox dvdNavigatorComboBox;
    private System.Windows.Forms.CheckBox checkBoxAC3;
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 
		/// </summary>
		public DVDCodec() : this("DVD Codec")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public DVDCodec(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Fetch available DirectShow filters
			//
			ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.MPEG2);
			ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.MPEG2_Audio);

			//
			// Fetch available DVD navigators
			//
			ArrayList availableDVDNavigators = FilterHelper.GetDVDNavigators();

			//
			// Fetch available Audio Renderers
			//
			ArrayList availableAudioRenderers = FilterHelper.GetAudioRenderers();

			//
			// Populate combo boxes
			//
			audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
			videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
			dvdNavigatorComboBox.Items.AddRange(availableDVDNavigators.ToArray());
			audioRendererComboBox.Items.AddRange(availableAudioRenderers.ToArray());
		}

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string audioRenderer = xmlreader.GetValueAsString("dvdplayer", "audiorenderer", "Default DirectSound Device");
				string videoCodec = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
				string audioCodec = xmlreader.GetValueAsString("dvdplayer", "audiocodec", "");
				string dvdNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
        checkBoxAC3.Checked= xmlreader.GetValueAsBool("dvdplayer", "ac3", false);
				
				audioRendererComboBox.SelectedItem = audioRenderer;
				dvdNavigatorComboBox.SelectedItem = dvdNavigator;

        if (audioCodec==String.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.MPEG2_Audio);
					if (availableAudioFilters.Count>0)
					{
						bool Mpeg2DecFilterFound=true;
						bool DScalerFilterFound=true;
						audioCodec=(string)availableAudioFilters[0];
						foreach (string filter in availableAudioFilters)
						{
							if (filter.Equals("MPEG/AC3/DTS/LPCM Audio Decoder"))
							{
								Mpeg2DecFilterFound=true;
							}
							if (filter.Equals("DScaler Audio Decoder"))
							{
								DScalerFilterFound=true;
							}
						}
						if (Mpeg2DecFilterFound) audioCodec="MPEG/AC3/DTS/LPCM Audio Decoder";
						else if (DScalerFilterFound) audioCodec="DScaler Audio Decoder";
					}
        }
				
				if (videoCodec==String.Empty)
				{
					ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.MPEG2);
					bool Mpeg2DecFilterFound=true;
					bool DScalerFilterFound=true;
					if (availableVideoFilters.Count>0)
					{
						videoCodec=(string)availableVideoFilters[0];
						foreach (string filter in availableVideoFilters)
						{
							if (filter.Equals("Mpeg2Dec Filter"))
							{
								Mpeg2DecFilterFound=true;
							}
							if (filter.Equals("DScaler Mpeg2 Video Decoder"))
							{
								DScalerFilterFound=true;
							}
						}
						if (Mpeg2DecFilterFound) videoCodec="Mpeg2Dec Filter";
						else if (DScalerFilterFound) videoCodec="DScaler Mpeg2 Video Decoder";
					}
				}

        audioCodecComboBox.SelectedItem = audioCodec;
        videoCodecComboBox.SelectedItem = videoCodec;

			}
		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("dvdplayer", "audiorenderer", audioRendererComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "videocodec", videoCodecComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "audiocodec", audioCodecComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "navigator", dvdNavigatorComboBox.Text);
        xmlwriter.SetValueAsBool("dvdplayer", "ac3", checkBoxAC3.Checked);
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
      this.checkBoxAC3 = new System.Windows.Forms.CheckBox();
      this.dvdNavigatorComboBox = new System.Windows.Forms.ComboBox();
      this.videoCodecComboBox = new System.Windows.Forms.ComboBox();
      this.audioCodecComboBox = new System.Windows.Forms.ComboBox();
      this.audioRendererComboBox = new System.Windows.Forms.ComboBox();
      this.dvdNavigatorLabel = new System.Windows.Forms.Label();
      this.videoCodecLabel = new System.Windows.Forms.Label();
      this.audioCodecLabel = new System.Windows.Forms.Label();
      this.audioRendererLabel = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.checkBoxAC3);
      this.groupBox1.Controls.Add(this.dvdNavigatorComboBox);
      this.groupBox1.Controls.Add(this.videoCodecComboBox);
      this.groupBox1.Controls.Add(this.audioCodecComboBox);
      this.groupBox1.Controls.Add(this.audioRendererComboBox);
      this.groupBox1.Controls.Add(this.dvdNavigatorLabel);
      this.groupBox1.Controls.Add(this.videoCodecLabel);
      this.groupBox1.Controls.Add(this.audioCodecLabel);
      this.groupBox1.Controls.Add(this.audioRendererLabel);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 184);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "General settings";
      // 
      // checkBoxAC3
      // 
      this.checkBoxAC3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxAC3.Location = new System.Drawing.Point(16, 136);
      this.checkBoxAC3.Name = "checkBoxAC3";
      this.checkBoxAC3.Size = new System.Drawing.Size(392, 24);
      this.checkBoxAC3.TabIndex = 4;
      this.checkBoxAC3.Text = "Use AC3 filter (needed for some soundcards if using SPDIF out)";
      // 
      // dvdNavigatorComboBox
      // 
      this.dvdNavigatorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.dvdNavigatorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.dvdNavigatorComboBox.Location = new System.Drawing.Point(168, 102);
      this.dvdNavigatorComboBox.Name = "dvdNavigatorComboBox";
      this.dvdNavigatorComboBox.Size = new System.Drawing.Size(256, 21);
      this.dvdNavigatorComboBox.TabIndex = 3;
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 77);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(256, 21);
      this.videoCodecComboBox.TabIndex = 2;
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 52);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(256, 21);
      this.audioCodecComboBox.TabIndex = 1;
      // 
      // audioRendererComboBox
      // 
      this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioRendererComboBox.Location = new System.Drawing.Point(168, 27);
      this.audioRendererComboBox.Name = "audioRendererComboBox";
      this.audioRendererComboBox.Size = new System.Drawing.Size(256, 21);
      this.audioRendererComboBox.TabIndex = 0;
      // 
      // dvdNavigatorLabel
      // 
      this.dvdNavigatorLabel.Location = new System.Drawing.Point(16, 105);
      this.dvdNavigatorLabel.Name = "dvdNavigatorLabel";
      this.dvdNavigatorLabel.Size = new System.Drawing.Size(150, 23);
      this.dvdNavigatorLabel.TabIndex = 3;
      this.dvdNavigatorLabel.Text = "DVD Navigator";
      // 
      // videoCodecLabel
      // 
      this.videoCodecLabel.Location = new System.Drawing.Point(16, 80);
      this.videoCodecLabel.Name = "videoCodecLabel";
      this.videoCodecLabel.Size = new System.Drawing.Size(150, 23);
      this.videoCodecLabel.TabIndex = 2;
      this.videoCodecLabel.Text = "Video codec";
      // 
      // audioCodecLabel
      // 
      this.audioCodecLabel.Location = new System.Drawing.Point(16, 55);
      this.audioCodecLabel.Name = "audioCodecLabel";
      this.audioCodecLabel.Size = new System.Drawing.Size(150, 23);
      this.audioCodecLabel.TabIndex = 1;
      this.audioCodecLabel.Text = "Audio codec";
      // 
      // audioRendererLabel
      // 
      this.audioRendererLabel.Location = new System.Drawing.Point(16, 30);
      this.audioRendererLabel.Name = "audioRendererLabel";
      this.audioRendererLabel.Size = new System.Drawing.Size(150, 23);
      this.audioRendererLabel.TabIndex = 0;
      this.audioRendererLabel.Text = "Audio renderer";
      // 
      // DVDCodec
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "DVDCodec";
      this.Size = new System.Drawing.Size(456, 448);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion
	}
}

