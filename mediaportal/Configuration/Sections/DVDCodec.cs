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
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				string audioRenderer = xmlreader.GetValueAsString("dvdplayer", "audiorenderer", "");
				string videoCodec = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
				string audioCodec = xmlreader.GetValueAsString("dvdplayer", "audiocodec", "");
				string dvdNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "");

				audioCodecComboBox.SelectedItem = audioCodec;
				audioRendererComboBox.SelectedItem = audioRenderer;
				videoCodecComboBox.SelectedItem = videoCodec;
				dvdNavigatorComboBox.SelectedItem = dvdNavigator;
			}
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("dvdplayer", "audiorenderer", audioRendererComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "videocodec", videoCodecComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "audiocodec", audioCodecComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "navigator", dvdNavigatorComboBox.Text);
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
			this.groupBox1.Size = new System.Drawing.Size(440, 152);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "General settings";
			// 
			// dvdNavigatorComboBox
			// 
			this.dvdNavigatorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dvdNavigatorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.dvdNavigatorComboBox.Location = new System.Drawing.Point(168, 102);
			this.dvdNavigatorComboBox.Name = "dvdNavigatorComboBox";
			this.dvdNavigatorComboBox.Size = new System.Drawing.Size(256, 21);
			this.dvdNavigatorComboBox.TabIndex = 7;
			// 
			// videoCodecComboBox
			// 
			this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoCodecComboBox.Location = new System.Drawing.Point(168, 77);
			this.videoCodecComboBox.Name = "videoCodecComboBox";
			this.videoCodecComboBox.Size = new System.Drawing.Size(256, 21);
			this.videoCodecComboBox.TabIndex = 6;
			// 
			// audioCodecComboBox
			// 
			this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.audioCodecComboBox.Location = new System.Drawing.Point(168, 52);
			this.audioCodecComboBox.Name = "audioCodecComboBox";
			this.audioCodecComboBox.Size = new System.Drawing.Size(256, 21);
			this.audioCodecComboBox.TabIndex = 5;
			// 
			// audioRendererComboBox
			// 
			this.audioRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.audioRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.audioRendererComboBox.Location = new System.Drawing.Point(168, 27);
			this.audioRendererComboBox.Name = "audioRendererComboBox";
			this.audioRendererComboBox.Size = new System.Drawing.Size(256, 21);
			this.audioRendererComboBox.TabIndex = 4;
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

