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

	public class MPEG2DecAudioFilter : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton radioPCM16Bit;
		private System.Windows.Forms.RadioButton radioButtonPCM24Bit;
		private System.Windows.Forms.RadioButton radioButtonPCM32Bit;
		private System.Windows.Forms.RadioButton radioButtonIEEE;
		private System.Windows.Forms.CheckBox checkBoxNormalize;
		private System.Windows.Forms.TrackBar trackBarBoost;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.RadioButton radioButtonAC3Speakers;
		private System.Windows.Forms.RadioButton radioButtonAC3SPDIF;
		private System.Windows.Forms.CheckBox checkBoxAC3DynamicRange;
		private System.Windows.Forms.ComboBox comboBoxAC3SpeakerConfig;
		private System.Windows.Forms.CheckBox checkBoxAC3LFE;
		private System.Windows.Forms.ComboBox comboBoxDTSSpeakerConfig;
		private System.Windows.Forms.CheckBox checkBoxDTSDynamicRange;
		private System.Windows.Forms.RadioButton radioButtonDTSSPDIF;
		private System.Windows.Forms.RadioButton radioButtonDTSSpeakers;
		private System.Windows.Forms.CheckBox checkBoxDTSLFE;
		private System.Windows.Forms.CheckBox checkBoxAACDownmix;
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 
		/// </summary>
		public MPEG2DecAudioFilter() : this("MPEG/AC3/DTS/LPCM Audio Decoder")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public MPEG2DecAudioFilter(string name) : base(name)
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
		private void InitializeComponent()
		{
			this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.radioPCM16Bit = new System.Windows.Forms.RadioButton();
			this.radioButtonPCM24Bit = new System.Windows.Forms.RadioButton();
			this.radioButtonPCM32Bit = new System.Windows.Forms.RadioButton();
			this.radioButtonIEEE = new System.Windows.Forms.RadioButton();
			this.checkBoxNormalize = new System.Windows.Forms.CheckBox();
			this.trackBarBoost = new System.Windows.Forms.TrackBar();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.radioButtonAC3Speakers = new System.Windows.Forms.RadioButton();
			this.radioButtonAC3SPDIF = new System.Windows.Forms.RadioButton();
			this.checkBoxAC3DynamicRange = new System.Windows.Forms.CheckBox();
			this.comboBoxAC3SpeakerConfig = new System.Windows.Forms.ComboBox();
			this.checkBoxAC3LFE = new System.Windows.Forms.CheckBox();
			this.comboBoxDTSSpeakerConfig = new System.Windows.Forms.ComboBox();
			this.checkBoxDTSDynamicRange = new System.Windows.Forms.CheckBox();
			this.radioButtonDTSSPDIF = new System.Windows.Forms.RadioButton();
			this.radioButtonDTSSpeakers = new System.Windows.Forms.RadioButton();
			this.label4 = new System.Windows.Forms.Label();
			this.checkBoxDTSLFE = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.checkBoxAACDownmix = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarBoost)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.checkBoxAACDownmix);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.checkBoxDTSLFE);
			this.groupBox1.Controls.Add(this.comboBoxDTSSpeakerConfig);
			this.groupBox1.Controls.Add(this.checkBoxDTSDynamicRange);
			this.groupBox1.Controls.Add(this.radioButtonDTSSPDIF);
			this.groupBox1.Controls.Add(this.radioButtonDTSSpeakers);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.checkBoxAC3LFE);
			this.groupBox1.Controls.Add(this.comboBoxAC3SpeakerConfig);
			this.groupBox1.Controls.Add(this.checkBoxAC3DynamicRange);
			this.groupBox1.Controls.Add(this.radioButtonAC3SPDIF);
			this.groupBox1.Controls.Add(this.radioButtonAC3Speakers);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.trackBarBoost);
			this.groupBox1.Controls.Add(this.checkBoxNormalize);
			this.groupBox1.Controls.Add(this.radioButtonIEEE);
			this.groupBox1.Controls.Add(this.radioButtonPCM32Bit);
			this.groupBox1.Controls.Add(this.radioButtonPCM24Bit);
			this.groupBox1.Controls.Add(this.radioPCM16Bit);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 424);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "MPEG/AC3/DTS/LPCM Audio Decoder";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(400, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Output sample format of the MPEG Audio, LPCMS, AC3, DTS, AAC Decoders:";
			// 
			// radioPCM16Bit
			// 
			this.radioPCM16Bit.Location = new System.Drawing.Point(32, 48);
			this.radioPCM16Bit.Name = "radioPCM16Bit";
			this.radioPCM16Bit.Size = new System.Drawing.Size(80, 16);
			this.radioPCM16Bit.TabIndex = 1;
			this.radioPCM16Bit.Text = "PCM 16 bit";
			// 
			// radioButtonPCM24Bit
			// 
			this.radioButtonPCM24Bit.Location = new System.Drawing.Point(120, 48);
			this.radioButtonPCM24Bit.Name = "radioButtonPCM24Bit";
			this.radioButtonPCM24Bit.Size = new System.Drawing.Size(80, 16);
			this.radioButtonPCM24Bit.TabIndex = 2;
			this.radioButtonPCM24Bit.Text = "PCM 24 bit";
			// 
			// radioButtonPCM32Bit
			// 
			this.radioButtonPCM32Bit.Location = new System.Drawing.Point(208, 48);
			this.radioButtonPCM32Bit.Name = "radioButtonPCM32Bit";
			this.radioButtonPCM32Bit.Size = new System.Drawing.Size(80, 16);
			this.radioButtonPCM32Bit.TabIndex = 3;
			this.radioButtonPCM32Bit.Text = "PCM 32 bit";
			// 
			// radioButtonIEEE
			// 
			this.radioButtonIEEE.Location = new System.Drawing.Point(296, 48);
			this.radioButtonIEEE.Name = "radioButtonIEEE";
			this.radioButtonIEEE.Size = new System.Drawing.Size(80, 16);
			this.radioButtonIEEE.TabIndex = 4;
			this.radioButtonIEEE.Text = "IEEE float";
			// 
			// checkBoxNormalize
			// 
			this.checkBoxNormalize.Location = new System.Drawing.Point(32, 88);
			this.checkBoxNormalize.Name = "checkBoxNormalize";
			this.checkBoxNormalize.Size = new System.Drawing.Size(80, 16);
			this.checkBoxNormalize.TabIndex = 5;
			this.checkBoxNormalize.Text = "Normalize";
			// 
			// trackBarBoost
			// 
			this.trackBarBoost.Location = new System.Drawing.Point(184, 80);
			this.trackBarBoost.Maximum = 100;
			this.trackBarBoost.Name = "trackBarBoost";
			this.trackBarBoost.Size = new System.Drawing.Size(200, 45);
			this.trackBarBoost.TabIndex = 6;
			this.trackBarBoost.TickStyle = System.Windows.Forms.TickStyle.None;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(128, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 16);
			this.label2.TabIndex = 7;
			this.label2.Text = "Boost:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 128);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(136, 16);
			this.label3.TabIndex = 8;
			this.label3.Text = "AC3 Decoder settings:";
			// 
			// radioButtonAC3Speakers
			// 
			this.radioButtonAC3Speakers.Location = new System.Drawing.Point(40, 152);
			this.radioButtonAC3Speakers.Name = "radioButtonAC3Speakers";
			this.radioButtonAC3Speakers.Size = new System.Drawing.Size(136, 16);
			this.radioButtonAC3Speakers.TabIndex = 9;
			this.radioButtonAC3Speakers.Text = "Decode to speakers:";
			// 
			// radioButtonAC3SPDIF
			// 
			this.radioButtonAC3SPDIF.Location = new System.Drawing.Point(40, 176);
			this.radioButtonAC3SPDIF.Name = "radioButtonAC3SPDIF";
			this.radioButtonAC3SPDIF.Size = new System.Drawing.Size(104, 16);
			this.radioButtonAC3SPDIF.TabIndex = 10;
			this.radioButtonAC3SPDIF.Text = "S/PDIF";
			// 
			// checkBoxAC3DynamicRange
			// 
			this.checkBoxAC3DynamicRange.Location = new System.Drawing.Point(40, 200);
			this.checkBoxAC3DynamicRange.Name = "checkBoxAC3DynamicRange";
			this.checkBoxAC3DynamicRange.Size = new System.Drawing.Size(144, 16);
			this.checkBoxAC3DynamicRange.TabIndex = 11;
			this.checkBoxAC3DynamicRange.Text = "Dynamic Range Control";
			// 
			// comboBoxAC3SpeakerConfig
			// 
			this.comboBoxAC3SpeakerConfig.Items.AddRange(new object[] {
																																	"Mono",
																																	"Dual Mono",
																																	"Stereo",
																																	"Dolby Stereo",
																																	"3 Front",
																																	"2 Front + 1 Rear",
																																	"3 Front + 1 Rear",
																																	"2 Front + 2 Rear",
																																	"3 Front + 2 Rear",
																																	"Channel 1",
																																	"Channel 2",
																																	""});
			this.comboBoxAC3SpeakerConfig.Location = new System.Drawing.Point(192, 152);
			this.comboBoxAC3SpeakerConfig.Name = "comboBoxAC3SpeakerConfig";
			this.comboBoxAC3SpeakerConfig.Size = new System.Drawing.Size(121, 21);
			this.comboBoxAC3SpeakerConfig.TabIndex = 12;
			this.comboBoxAC3SpeakerConfig.Text = "Stereo";
			// 
			// checkBoxAC3LFE
			// 
			this.checkBoxAC3LFE.Location = new System.Drawing.Point(320, 152);
			this.checkBoxAC3LFE.Name = "checkBoxAC3LFE";
			this.checkBoxAC3LFE.Size = new System.Drawing.Size(72, 16);
			this.checkBoxAC3LFE.TabIndex = 13;
			this.checkBoxAC3LFE.Text = "LFE";
			// 
			// comboBoxDTSSpeakerConfig
			// 
			this.comboBoxDTSSpeakerConfig.Items.AddRange(new object[] {
																																	"Mono",
																																	"Dual Mono",
																																	"Stereo",
																																	"Dolby Stereo",
																																	"3 Front",
																																	"2 Front + 1 Rear",
																																	"3 Front + 1 Rear",
																																	"2 Front + 2 Rear",
																																	"3 Front + 2 Rear",
																																	"Channel 1",
																																	"Channel 2",
																																	""});
			this.comboBoxDTSSpeakerConfig.Location = new System.Drawing.Point(192, 256);
			this.comboBoxDTSSpeakerConfig.Name = "comboBoxDTSSpeakerConfig";
			this.comboBoxDTSSpeakerConfig.Size = new System.Drawing.Size(121, 21);
			this.comboBoxDTSSpeakerConfig.TabIndex = 18;
			this.comboBoxDTSSpeakerConfig.Text = "Stereo";
			// 
			// checkBoxDTSDynamicRange
			// 
			this.checkBoxDTSDynamicRange.Location = new System.Drawing.Point(40, 304);
			this.checkBoxDTSDynamicRange.Name = "checkBoxDTSDynamicRange";
			this.checkBoxDTSDynamicRange.Size = new System.Drawing.Size(144, 16);
			this.checkBoxDTSDynamicRange.TabIndex = 17;
			this.checkBoxDTSDynamicRange.Text = "Dynamic Range Control";
			// 
			// radioButtonDTSSPDIF
			// 
			this.radioButtonDTSSPDIF.Location = new System.Drawing.Point(40, 280);
			this.radioButtonDTSSPDIF.Name = "radioButtonDTSSPDIF";
			this.radioButtonDTSSPDIF.Size = new System.Drawing.Size(104, 16);
			this.radioButtonDTSSPDIF.TabIndex = 16;
			this.radioButtonDTSSPDIF.Text = "S/PDIF";
			// 
			// radioButtonDTSSpeakers
			// 
			this.radioButtonDTSSpeakers.Location = new System.Drawing.Point(40, 256);
			this.radioButtonDTSSpeakers.Name = "radioButtonDTSSpeakers";
			this.radioButtonDTSSpeakers.Size = new System.Drawing.Size(136, 16);
			this.radioButtonDTSSpeakers.TabIndex = 15;
			this.radioButtonDTSSpeakers.Text = "Decode to speakers:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 232);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(136, 16);
			this.label4.TabIndex = 14;
			this.label4.Text = "DTS Decoder settings:";
			// 
			// checkBoxDTSLFE
			// 
			this.checkBoxDTSLFE.Location = new System.Drawing.Point(320, 256);
			this.checkBoxDTSLFE.Name = "checkBoxDTSLFE";
			this.checkBoxDTSLFE.Size = new System.Drawing.Size(72, 16);
			this.checkBoxDTSLFE.TabIndex = 19;
			this.checkBoxDTSLFE.Text = "LFE";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(24, 336);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(136, 16);
			this.label5.TabIndex = 20;
			this.label5.Text = "AAC Decoder settings:";
			// 
			// checkBoxAACDownmix
			// 
			this.checkBoxAACDownmix.Location = new System.Drawing.Point(40, 360);
			this.checkBoxAACDownmix.Name = "checkBoxAACDownmix";
			this.checkBoxAACDownmix.Size = new System.Drawing.Size(128, 16);
			this.checkBoxAACDownmix.TabIndex = 21;
			this.checkBoxAACDownmix.Text = "Downmix to stereo";
			// 
			// MPEG2DecAudioFilter
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "MPEG2DecAudioFilter";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackBarBoost)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

	}
}

