using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices; 

using DShowNET;
using DShowNET.Device;
using DirectX.Capture;
using MediaPortal.TV.Recording;
using MediaPortal.GUI.Library;
using TVCapture;
namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for EditCaptureCardForm.
	/// </summary>
	public class EditCaptureCardForm : System.Windows.Forms.Form
	{
    bool    m_bMPEG2=false;
    bool    m_bISMCE=false;
		bool		m_bIsBDA=false;
#if (UseCaptureCardDefinitions)
		static CaptureCardDefinitions mCaptureCardDefinitions = CaptureCardDefinitions.Instance;
#endif
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cardComboBox;
		private System.Windows.Forms.CheckBox useRecordingCheckBox;
		private System.Windows.Forms.CheckBox useWatchingCheckBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//
		// Private members
		//
		ArrayList captureFormats = new ArrayList();
		private System.Windows.Forms.ComboBox frameSizeComboBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox frameRateTextBox;
		private System.Windows.Forms.Label label6;
		ArrayList propertyPages = new ArrayList();
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox videoCompressorComboBox;
    private System.Windows.Forms.ComboBox audioCompressorComboBox;
    private System.Windows.Forms.Button setupButton;
    private System.Windows.Forms.ComboBox filterComboBox;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.ComboBox audioDeviceComboBox;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.ComboBox comboBoxLineInput;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.TrackBar trackRecording;
    private System.Windows.Forms.Label lblRecordingLevel;
    private Size m_size = new Size(0,0);
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.TextBox textBoxName;
		
    bool acceptuserinput=false;
		private System.Windows.Forms.Button buttonAutotune;
		
		/// <summary>
		/// 
		/// </summary>
		public EditCaptureCardForm(int cardId)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// Setup combo boxes and controls
			//
			ArrayList availableVideoDevices = FilterHelper.GetVideoInputDevices();
			ArrayList availableVideoDeviceMonikers	= FilterHelper.GetVideoInputDeviceMonikers();
      ArrayList availableAudioDevices = FilterHelper.GetAudioInputDevices();
      ArrayList availableVideoCompressors = FilterHelper.GetVideoCompressors();
      ArrayList availableAudioCompressors = FilterHelper.GetAudioCompressors();
        
      FilterHelper.GetMPEG2VideoEncoders( availableVideoCompressors);
      FilterHelper.GetMPEG2AudioEncoders(availableAudioCompressors);
			for (int i=0; i < availableVideoDevices.Count;++i)
			{
			//	Log.Write("device:{0} id:{1}", availableVideoDevices[i].ToString(), availableVideoDeviceMonikers[i].ToString());
			}


#if (!UseCaptureCardDefinitions)
			cardComboBox.Items.AddRange(availableVideoDevices.ToArray());
#endif
      audioDeviceComboBox.Items.AddRange(availableAudioDevices.ToArray());
      videoCompressorComboBox.Items.AddRange(availableVideoCompressors.ToArray());
      audioCompressorComboBox.Items.AddRange(availableAudioCompressors.ToArray());

      comboBoxLineInput.Items.Clear();

			if(availableVideoDevices.Count == 0)
			{
				MessageBox.Show("No video device was found, you won't be able to configure a capture card", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
				useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = filterComboBox.Enabled = cardComboBox.Enabled = okButton.Enabled = setupButton.Enabled = audioCompressorComboBox.Enabled = audioDeviceComboBox.Enabled = videoCompressorComboBox.Enabled = false;
        comboBoxLineInput.Enabled	= false;
        trackRecording.Enabled		= false;
				acceptuserinput						= false;
			}
      else
        acceptuserinput=true;

#if (UseCaptureCardDefinitions)
			Log.Write("UseCaptureCardDefinitions");
#else
			Log.Write("not using UseCaptureCardDefinitions");
#endif
#if (UseCaptureCardDefinitions)
			// #MW#
			// Load capture card definitions, and only display those cards that are supported by MP
			// This might mean that altough capture cards are present, they're not supported, and thus
			// not displayed ;-)
			//
			// So:
			//	match cards & put in list
			//	if list is empty, inform user and block further input
			//	else continue


			bool addEmpty=true;
			foreach (string ccDevId in CaptureCardDefinitions.CaptureCards.Keys)
			{
				CaptureCardDefinition ccd = CaptureCardDefinitions.CaptureCards[ccDevId] as CaptureCardDefinition;
				for (int i = 0; i < availableVideoDevices.Count; i++)
				{
					if (((string)(availableVideoDevices[i]) == ccd.CaptureName) &&
						((availableVideoDeviceMonikers[i]).ToString().IndexOf(ccd.DeviceId) > -1 )) addEmpty=false;

					//Log.Write("{0}:{1}", (string)(availableVideoDevices[i]), (availableVideoDeviceMonikers[i]).ToString() );
				}
			}

			Log.Write("compare");
			foreach (string ccDevId in CaptureCardDefinitions.CaptureCards.Keys)
			{
				CaptureCardDefinition ccd = CaptureCardDefinitions.CaptureCards[ccDevId] as CaptureCardDefinition;
				for (int i = 0; i < availableVideoDevices.Count; i++)
				{
					bool add=false;
					if (ccd.CaptureName==String.Empty) 
					{
						if (addEmpty)
							add=true;
					}
					else
					{
						if (((string)(availableVideoDevices[i]) == ccd.CaptureName) &&
							((availableVideoDeviceMonikers[i]).ToString().IndexOf(ccd.DeviceId) > -1 )) add=true;
					}
					if (add)
					{
						TVCaptureDevice cd		= new TVCaptureDevice();
						cd.VideoDeviceMoniker = availableVideoDeviceMonikers[i].ToString();
						if (ccd.CaptureName!=String.Empty) 
						{
							cd.VideoDevice				= ccd.CaptureName;
							cd.LoadDefinitions();
							cd.IsBDACard					= ccd.Capabilities.IsBDADevice;
							cd.IsMCECard					= ccd.Capabilities.IsMceDevice;
							cd.SupportsMPEG2			= ccd.Capabilities.IsMpeg2Device;
						}
						else
						{
							cd.VideoDevice				= (string)availableVideoDevices[i];
							cd.CommercialName			= ccd.CommercialName;
							cd.CaptureName			  = cd.VideoDevice;
							cd.DeviceId						= ccd.DeviceId;
						}
						Log.Write("adding {0} id:{1} bda:{2} mce:{3} mpeg2:{4}", cd.CaptureName,cd.IsBDACard,cd.IsMCECard,cd.SupportsMPEG2);
						ComboBoxCaptureCard cbcc = new ComboBoxCaptureCard(cd);
						cardComboBox.Items.Add(cbcc); //availableVideoDevices[i]);
					}
				}
			}

			if (cardComboBox.Items.Count == 0)
			{
				MessageBox.Show("No supported video device was found, you won't be able to configure a capture card", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
				useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = filterComboBox.Enabled = cardComboBox.Enabled = okButton.Enabled = setupButton.Enabled = audioCompressorComboBox.Enabled = audioDeviceComboBox.Enabled = videoCompressorComboBox.Enabled = false;
				comboBoxLineInput.Enabled = false;
				trackRecording.Enabled    = false;
				acceptuserinput           = false;
			}
			else
				acceptuserinput						= true;
#endif
			SetupCaptureFormats();
			frameSizeComboBox.Items.AddRange(captureFormats.ToArray());

      textBoxName.Text=String.Format("card{0}", cardId);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lblRecordingLevel = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.trackRecording = new System.Windows.Forms.TrackBar();
			this.label10 = new System.Windows.Forms.Label();
			this.comboBoxLineInput = new System.Windows.Forms.ComboBox();
			this.audioDeviceComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.setupButton = new System.Windows.Forms.Button();
			this.filterComboBox = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.audioCompressorComboBox = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.videoCompressorComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.frameRateTextBox = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.frameSizeComboBox = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.useRecordingCheckBox = new System.Windows.Forms.CheckBox();
			this.useWatchingCheckBox = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cardComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.buttonAutotune = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackRecording)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.lblRecordingLevel);
			this.groupBox1.Controls.Add(this.label11);
			this.groupBox1.Controls.Add(this.trackRecording);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.comboBoxLineInput);
			this.groupBox1.Controls.Add(this.audioDeviceComboBox);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.setupButton);
			this.groupBox1.Controls.Add(this.filterComboBox);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.audioCompressorComboBox);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.videoCompressorComboBox);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.frameRateTextBox);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.frameSizeComboBox);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.useRecordingCheckBox);
			this.groupBox1.Controls.Add(this.useWatchingCheckBox);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.cardComboBox);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(456, 434);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Capture Card Settings";
			// 
			// lblRecordingLevel
			// 
			this.lblRecordingLevel.Location = new System.Drawing.Point(272, 392);
			this.lblRecordingLevel.Name = "lblRecordingLevel";
			this.lblRecordingLevel.Size = new System.Drawing.Size(64, 23);
			this.lblRecordingLevel.TabIndex = 50;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(16, 392);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(88, 32);
			this.label11.TabIndex = 49;
			this.label11.Text = "Audio input recording level";
			// 
			// trackRecording
			// 
			this.trackRecording.Location = new System.Drawing.Point(120, 384);
			this.trackRecording.Maximum = 100;
			this.trackRecording.Name = "trackRecording";
			this.trackRecording.Size = new System.Drawing.Size(136, 42);
			this.trackRecording.TabIndex = 48;
			this.trackRecording.TickFrequency = 10;
			this.trackRecording.Value = 80;
			this.trackRecording.ValueChanged += new System.EventHandler(this.trackRecording_ValueChanged);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(16, 112);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(104, 16);
			this.label10.TabIndex = 47;
			this.label10.Text = "Line input";
			// 
			// comboBoxLineInput
			// 
			this.comboBoxLineInput.Enabled = false;
			this.comboBoxLineInput.Location = new System.Drawing.Point(120, 112);
			this.comboBoxLineInput.Name = "comboBoxLineInput";
			this.comboBoxLineInput.Size = new System.Drawing.Size(320, 21);
			this.comboBoxLineInput.TabIndex = 46;
			// 
			// audioDeviceComboBox
			// 
			this.audioDeviceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.audioDeviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.audioDeviceComboBox.Enabled = false;
			this.audioDeviceComboBox.ItemHeight = 13;
			this.audioDeviceComboBox.Location = new System.Drawing.Point(120, 88);
			this.audioDeviceComboBox.Name = "audioDeviceComboBox";
			this.audioDeviceComboBox.Size = new System.Drawing.Size(320, 21);
			this.audioDeviceComboBox.TabIndex = 44;
			this.audioDeviceComboBox.SelectedIndexChanged += new System.EventHandler(this.audioDeviceComboBox_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 88);
			this.label2.Name = "label2";
			this.label2.TabIndex = 43;
			this.label2.Text = "Audio device";
			// 
			// setupButton
			// 
			this.setupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.setupButton.Enabled = false;
			this.setupButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.setupButton.Location = new System.Drawing.Point(366, 192);
			this.setupButton.Name = "setupButton";
			this.setupButton.Size = new System.Drawing.Size(75, 21);
			this.setupButton.TabIndex = 42;
			this.setupButton.Text = "Setup";
			this.setupButton.Click += new System.EventHandler(this.setupButton_Click);
			// 
			// filterComboBox
			// 
			this.filterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.filterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.filterComboBox.Enabled = false;
			this.filterComboBox.Location = new System.Drawing.Point(120, 192);
			this.filterComboBox.Name = "filterComboBox";
			this.filterComboBox.Size = new System.Drawing.Size(240, 21);
			this.filterComboBox.TabIndex = 41;
			this.filterComboBox.SelectedIndexChanged += new System.EventHandler(this.filterComboBox_SelectedIndexChanged);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 200);
			this.label9.Name = "label9";
			this.label9.TabIndex = 40;
			this.label9.Text = "Device property";
			// 
			// audioCompressorComboBox
			// 
			this.audioCompressorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.audioCompressorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.audioCompressorComboBox.Enabled = false;
			this.audioCompressorComboBox.ItemHeight = 13;
			this.audioCompressorComboBox.Location = new System.Drawing.Point(120, 256);
			this.audioCompressorComboBox.Name = "audioCompressorComboBox";
			this.audioCompressorComboBox.Size = new System.Drawing.Size(320, 21);
			this.audioCompressorComboBox.TabIndex = 39;
			this.audioCompressorComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.audioCompressorComboBox_KeyPress);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 256);
			this.label5.Name = "label5";
			this.label5.TabIndex = 38;
			this.label5.Text = "Audio compressor";
			// 
			// videoCompressorComboBox
			// 
			this.videoCompressorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.videoCompressorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoCompressorComboBox.Enabled = false;
			this.videoCompressorComboBox.ItemHeight = 13;
			this.videoCompressorComboBox.Location = new System.Drawing.Point(120, 224);
			this.videoCompressorComboBox.Name = "videoCompressorComboBox";
			this.videoCompressorComboBox.Size = new System.Drawing.Size(320, 21);
			this.videoCompressorComboBox.TabIndex = 37;
			this.videoCompressorComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.videoCompressorComboBox_KeyPress);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 232);
			this.label3.Name = "label3";
			this.label3.TabIndex = 36;
			this.label3.Text = "Video compressor";
			// 
			// frameRateTextBox
			// 
			this.frameRateTextBox.Enabled = false;
			this.frameRateTextBox.Location = new System.Drawing.Point(120, 360);
			this.frameRateTextBox.MaxLength = 3;
			this.frameRateTextBox.Name = "frameRateTextBox";
			this.frameRateTextBox.Size = new System.Drawing.Size(40, 20);
			this.frameRateTextBox.TabIndex = 27;
			this.frameRateTextBox.Text = "25";
			this.frameRateTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frameRateTextBox_KeyPress);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 360);
			this.label6.Name = "label6";
			this.label6.TabIndex = 20;
			this.label6.Text = "Framerate";
			// 
			// frameSizeComboBox
			// 
			this.frameSizeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.frameSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.frameSizeComboBox.Enabled = false;
			this.frameSizeComboBox.ItemHeight = 13;
			this.frameSizeComboBox.Location = new System.Drawing.Point(120, 336);
			this.frameSizeComboBox.Name = "frameSizeComboBox";
			this.frameSizeComboBox.Size = new System.Drawing.Size(320, 21);
			this.frameSizeComboBox.TabIndex = 19;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 336);
			this.label7.Name = "label7";
			this.label7.TabIndex = 18;
			this.label7.Text = "Framesize";
			// 
			// useRecordingCheckBox
			// 
			this.useRecordingCheckBox.Checked = true;
			this.useRecordingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.useRecordingCheckBox.Enabled = false;
			this.useRecordingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useRecordingCheckBox.Location = new System.Drawing.Point(152, 304);
			this.useRecordingCheckBox.Name = "useRecordingCheckBox";
			this.useRecordingCheckBox.Size = new System.Drawing.Size(112, 24);
			this.useRecordingCheckBox.TabIndex = 15;
			this.useRecordingCheckBox.Text = "Use for recording";
			// 
			// useWatchingCheckBox
			// 
			this.useWatchingCheckBox.Checked = true;
			this.useWatchingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.useWatchingCheckBox.Enabled = false;
			this.useWatchingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useWatchingCheckBox.Location = new System.Drawing.Point(32, 304);
			this.useWatchingCheckBox.Name = "useWatchingCheckBox";
			this.useWatchingCheckBox.Size = new System.Drawing.Size(112, 24);
			this.useWatchingCheckBox.TabIndex = 14;
			this.useWatchingCheckBox.Text = "Use for watching";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 288);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 16);
			this.label4.TabIndex = 13;
			this.label4.Text = "Purpose";
			// 
			// cardComboBox
			// 
			this.cardComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cardComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cardComboBox.Location = new System.Drawing.Point(120, 56);
			this.cardComboBox.Name = "cardComboBox";
			this.cardComboBox.Size = new System.Drawing.Size(320, 21);
			this.cardComboBox.TabIndex = 1;
			this.cardComboBox.SelectedIndexChanged += new System.EventHandler(this.cardComboBox_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 64);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			this.label1.Text = "Video device";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(16, 144);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(424, 40);
			this.label8.TabIndex = 45;
			this.label8.Text = "To configure properties that are specific for your capture device, select the pro" +
				"perty in the dropdown list below and press the \'Setup\' button. [Note: it depends" +
				" on your TV-card if these settings are saved!!]";
			// 
			// textBoxName
			// 
			this.textBoxName.Location = new System.Drawing.Point(0, 0);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.TabIndex = 0;
			this.textBoxName.Text = "";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(0, 0);
			this.label12.Name = "label12";
			this.label12.TabIndex = 0;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(389, 450);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(309, 450);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// buttonAutotune
			// 
			this.buttonAutotune.Location = new System.Drawing.Point(192, 448);
			this.buttonAutotune.Name = "buttonAutotune";
			this.buttonAutotune.TabIndex = 3;
			this.buttonAutotune.Text = "Autotune";
			this.buttonAutotune.Click += new System.EventHandler(this.buttonAutotune_Click);
			// 
			// EditCaptureCardForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(474, 482);
			this.Controls.Add(this.buttonAutotune);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MinimumSize = new System.Drawing.Size(480, 296);
			this.Name = "EditCaptureCardForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "EditCaptureCardForm";
			this.Load += new System.EventHandler(this.EditCaptureCardForm_Load);
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackRecording)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void okButton_Click(object sender, System.EventArgs e)
		{
			if (videoCompressorComboBox.Enabled)
			{
				if (videoCompressorComboBox.SelectedItem==null)
				{
					MessageBox.Show("No video compressor selected.Please choose a video compressor", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;					
				}
			}
			if (audioCompressorComboBox.Enabled)
			{
				if (audioCompressorComboBox.SelectedItem==null)
				{
					MessageBox.Show("No audio compressor selected.Please choose an audio compressor", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;					
				}
			}
			if (audioDeviceComboBox.Enabled)
			{
				if (audioDeviceComboBox.SelectedItem==null)
				{
					MessageBox.Show("No audio device selected.Please choose an audio device", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;					
				}
			}
			if (comboBoxLineInput.Enabled)
			{
				if (comboBoxLineInput.SelectedItem==null)
				{
					MessageBox.Show("No line input selected.Please choose a line input", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;					
				}
			}
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Hide();
		}


		private void SetupPropertyPages(TVCaptureDevice capture )
		{
			//
			// Clear any previous items
			//
			filterComboBox.Items.Clear();

			if (capture != null) 
			{
				if(capture.PropertyPages != null)
				{
					foreach(PropertyPage page in capture.PropertyPages)
					{
						filterComboBox.Items.Add(page.Name);
					}
				}
			}
		}

		private void SetupCaptureFormats()
		{
			int[][] resolution = new int[][]{ new int[] { 320, 240 },
				new int[] { 352, 240 },
				new int[] { 352, 288 },
				new int[] { 640, 240 },
				new int[] { 640, 288 },
				new int[] { 640, 480 },
				new int[] { 704, 576 },
				new int[] { 720, 240 },
				new int[] { 720, 288 },
				new int[] { 720, 480 },
				new int[] { 720, 576 } };

      captureFormats.Clear();
			for(int index = 0; index < resolution.Length; index++)
			{
				CaptureFormat format = new CaptureFormat();
				format.Width = resolution[index][0]; 
				format.Height = resolution[index][1];
				format.Description = String.Format("{0}x{1}", format.Width, format.Height);
				captureFormats.Add(format);
			}
		}

		private void frameRateTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
      if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}		
		}

		private void cardComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
      if (!acceptuserinput) return;
      FillInAll();
    }

		/// <summary>
		/// #MW#
		/// Fill in all the appropriate fields. The selected card is already checked as a card that
		/// is supported, so definitions might already been loaded.
		/// The createcapturedevice() method builds up a graph, without the connections in order to
		/// display the property pages belonging to the required filters.
		/// 
		/// This was done using the Capture class, which did a mix of the (My)SinkGraph class used by the
		/// TvCapture stuff. For the time being, a kind of hack is used to re-use parts of the sinkgraph
		/// code that builds the graph according to the cards definitions. In the future, a redesign should
		/// be done to remove all obsolete code and refactor the whole capture tv/radio thingy...
		/// 
		/// Depending on the cards capabilities, the rest of the fields should be enabled/disabled
		/// 
		/// Display should give the descriptive name, ie PVR150MCE, PVR350 etc.
		/// </summary>
    void FillInAll()
    {
			// #MW#
			// Load card and set properties (double work again) to make the creation of the capture device
			// work, ie that one can load the definitions held in the TVCaptureDevice to create the graph, ie
			// load the filters and connect the pins...
			//
			//_mTvCaptureDevice.FriendlyName = "";
			// card should be set already by caller...

      //
      // Setup frame sizes
      //
      TVCaptureDevice capture = CaptureCard;

      //
      // Update controls
      //
      if(capture != null)
      {
        trackRecording.Enabled=frameSizeComboBox.Enabled = frameRateTextBox.Enabled = audioDeviceComboBox.Enabled = audioCompressorComboBox.Enabled = videoCompressorComboBox.Enabled = frameRateTextBox.Enabled = frameSizeComboBox.Enabled = audioCompressorComboBox.Enabled=comboBoxLineInput.Enabled=!capture.SupportsTimeShifting;
      }
      else
      {
        trackRecording.Enabled=frameSizeComboBox.Enabled = frameRateTextBox.Enabled = audioDeviceComboBox.Enabled = audioCompressorComboBox.Enabled = videoCompressorComboBox.Enabled = frameRateTextBox.Enabled = frameSizeComboBox.Enabled = audioCompressorComboBox.Enabled=comboBoxLineInput.Enabled=false;
		  if(cardComboBox.SelectedIndex!=-1) 
			  if(cardComboBox.SelectedItem.ToString()=="B2C2 MPEG-2 Source")
			  {
				  frameSizeComboBox.Enabled =true;
			  }
	  }

      useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = filterComboBox.Enabled = setupButton.Enabled = cardComboBox.Text.Length > 0;

      //
      // Setup line input combo box
      //
      comboBoxLineInput.Items.Clear();

      if(capture != null)
      {
				if (capture.CreateGraph())
				{
					//
					// Clear combo box
					//
					frameSizeComboBox.Items.Clear();

					//
					// Loop through available frame sizes and try to assign them to the card, if we succeed we
					// know the card supports the size.
					//
				
					if (capture.CreateGraph())
					{
						SetupPropertyPages(capture);
						foreach(CaptureFormat format in captureFormats)
						{
							Size frameSize = new Size(format.Width, format.Height);
							if (capture.SupportsFrameSize(frameSize))
							{	
								//
								// Card supports the current frame size
								//
								frameSizeComboBox.Items.Add(format);
							}
						}
					}

					IBaseFilter audioDevice=capture.AudiodeviceFilter;
					if (audioDevice!=null)
					{
						int hr=0;
						IEnumPins pinEnum;
						hr=audioDevice.EnumPins(out pinEnum);
						if( (hr == 0) && (pinEnum != null) )
						{
							pinEnum.Reset();
							IPin[] pins = new IPin[1];
							int f;
							do
							{
								// Get the next pin
								hr = pinEnum.Next( 1, pins, out f );
								if( (hr == 0) && (pins[0] != null) )
								{
									PinDirection pinDir;
									pins[0].QueryDirection(out pinDir);
									if (pinDir==PinDirection.Input)
									{
										PinInfo info;
										pins[0].QueryPinInfo(out info);
										comboBoxLineInput.Items.Add(info.name);
									}
									Marshal.ReleaseComObject( pins[0] );
								}
							}
							while( hr == 0 );
						}
					}
					
					capture.DeleteGraph();
				}
      }

      // select the correct framesize
      if (frameSizeComboBox.Items.Count>0)
        frameSizeComboBox.SelectedIndex=0;

      for (int i=0; i < frameSizeComboBox.Items.Count;++i)
      {
        CaptureFormat fmt =(CaptureFormat)frameSizeComboBox.Items[i];
        if (m_size.Width==fmt.Width && m_size.Height==fmt.Height)
        {
          frameSizeComboBox.SelectedIndex = i;
          break;
        }
      }
    }

		private void setupButton_Click(object sender, System.EventArgs e)
		{
			if(filterComboBox.SelectedItem != null)
			{
				string propertyPageName = (string)filterComboBox.SelectedItem;
				TVCaptureDevice capture = CaptureCard;
				if(capture != null)
				{
					if (capture.CreateGraph())
					{
						if(capture.PropertyPages != null)
						{
							foreach(PropertyPage page in capture.PropertyPages)
							{
								if(propertyPageName.Equals(page.Name))
								{
									//
									// Display property page
									//
									page.Show(this);

									//
									// Save settings
									//
									//capture.SaveSettings(cardId);
									break;
								}
							}
						}

						capture.DeleteGraph();
					}
				}
			}
		}

		private void filterComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			setupButton.Enabled = filterComboBox.SelectedItem != null;
		}

    private void videoCompressorComboBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if(e.KeyChar == (char)Keys.Delete || e.KeyChar == (char)Keys.Back)
      {
        videoCompressorComboBox.SelectedItem = null;
        videoCompressorComboBox.Text = String.Empty;
      }
    }

    private void audioCompressorComboBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if(e.KeyChar == (char)Keys.Delete || e.KeyChar == (char)Keys.Back)
      {
        audioCompressorComboBox.SelectedItem = null;
        audioCompressorComboBox.Text = String.Empty;
      }    
    }

    private void EditCaptureCardForm_Load(object sender, System.EventArgs e)
    {
      FillInAll();    
    }
  
    private void audioDeviceComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (!acceptuserinput) return;
      FillInAll();        
    }

    private void trackRecording_ValueChanged(object sender, System.EventArgs e)
    {
      lblRecordingLevel.Text = String.Format("{0}%", trackRecording.Value);
    }

		private void buttonAutotune_Click(object sender, System.EventArgs e)
		{
			AnalogTVTuningForm dialog = new AnalogTVTuningForm();
			ITuning tuning=GraphFactory.CreateTuning(CaptureCard);
			if (tuning!=null)
			{
				dialog.Tuning=tuning;
				dialog.Card=CaptureCard;
				dialog.ShowDialog(this);
			}
			else
			{
				MessageBox.Show("This device does not support auto tuning", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}


		public TVCaptureDevice CaptureCard
		{
			get 
			{
				if (cardComboBox.SelectedItem==null) return null;
#if (UseCaptureCardDefinitions)
				TVCaptureDevice card = (cardComboBox.SelectedItem as ComboBoxCaptureCard).CaptureDevice;
				card.DeviceType=card.DeviceId;
				m_bMPEG2=card.SupportsMPEG2;
				m_bIsBDA=card.IsBDACard;
				m_bISMCE=card.IsMCECard;
#else
				TVCaptureDevice card = new TVCaptureDevice();

				card.VideoDevice = cardComboBox.Text;
#endif

				card.UseForRecording = useRecordingCheckBox.Checked;
				card.UseForTV	= useWatchingCheckBox.Checked;
				
        if(frameSizeComboBox.SelectedItem != null)
        {
          CaptureFormat fmt = (CaptureFormat)frameSizeComboBox.SelectedItem;
          card.FrameSize = new Size(fmt.Width, fmt.Height);

          if(frameRateTextBox.Text.Length > 0)
          {
            card.FrameRate = Int32.Parse(frameRateTextBox.Text);
          }
        }

        card.VideoCompressor = videoCompressorComboBox.Text;
        card.AudioCompressor = audioCompressorComboBox.Text;
        card.AudioDevice = audioDeviceComboBox.Text;
        card.AudioInputPin = comboBoxLineInput.Text;

				// #MW. Why was this in #if/#endif???
				// Now failed to copy the mpeg2/mce settings...
				// And mpeg2/mce cards always set to false, ie software cards!!!
				//#if (UseCaptureCardDefinitions)
#if (UseCaptureCardDefinitions)
				if (card.DeviceId.ToLower()=="s/w") {m_bMPEG2=false; m_bISMCE=false;}
				if (card.DeviceId.ToLower()=="hw") {m_bMPEG2=true; m_bISMCE=false;}
				if (card.DeviceId.ToLower()=="mce") {m_bMPEG2=true; m_bISMCE=true;}
				card.IsBDACard=m_bIsBDA;
#else
				card.SupportsMPEG2 = m_bMPEG2;
				card.IsMCECard     = m_bISMCE;
#endif
				card.RecordingLevel = trackRecording.Value;
        card.FriendlyName   = textBoxName.Text;
				return card;
			}

			set
			{
        acceptuserinput=false;
				TVCaptureDevice card = value as TVCaptureDevice;

				if(card != null)
				{
          textBoxName.Text=card.FriendlyName;
#if (UseCaptureCardDefinitions)
					if (card.DeviceId!=null)
					{
						ComboBoxCaptureCard cbcc = new ComboBoxCaptureCard(card);
						card.DeviceType=card.DeviceId;
						cardComboBox.SelectedItem					= cbcc;
					}
					else
					{
						card.DeviceType=card.DeviceType;
						for (int i=0; i < cardComboBox.Items.Count;++i)
						{
							ComboBoxCaptureCard cd= (ComboBoxCaptureCard)cardComboBox.Items[i];
							if (cd.CaptureDevice.DeviceId==card.DeviceType)
							{
								if (card.VideoDeviceMoniker==cd.VideoDeviceMoniker)
								{
									cardComboBox.SelectedIndex=i;
									break;
								}
							}
						}
					}
#else
          cardComboBox.SelectedItem = card.VideoDevice;
#endif
          audioDeviceComboBox.SelectedItem  = card.AudioDevice          ;
					useRecordingCheckBox.Checked = card.UseForRecording;
          useWatchingCheckBox.Checked = card.UseForTV;
          frameRateTextBox.Text = card.FrameRate.ToString();
          trackRecording.Value=card.RecordingLevel  ;

          videoCompressorComboBox.SelectedItem = card.VideoCompressor;
          audioCompressorComboBox.SelectedItem = card.AudioCompressor;
          audioDeviceComboBox.SelectedItem = card.AudioDevice;
          
          comboBoxLineInput.Text = card.AudioInputPin;
          trackRecording_ValueChanged(null,null);
					m_size=new Size(card.FrameSize.Width,card.FrameSize.Height);

          FillInAll(); // fill all framerates & audio in types...


          comboBoxLineInput.Text = card.AudioInputPin;
          trackRecording_ValueChanged(null,null);
          
          // select the correct framesize
          for (int i=0; i < frameSizeComboBox.Items.Count;++i)
          {
            CaptureFormat fmt =(CaptureFormat)frameSizeComboBox.Items[i];
            
            if (card.FrameSize.Width==fmt.Width && card.FrameSize.Height==fmt.Height)
            {
              frameSizeComboBox.SelectedIndex = i;
              break;
            }
          }
          //select correct audio line input
          for (int i=0; i < comboBoxLineInput.Items.Count;++i)
          {
            string input = (string)comboBoxLineInput.Items[i];
            if (card.AudioInputPin==input)
            {
              comboBoxLineInput.SelectedIndex = i;
              break;
            }
          }
        }
        acceptuserinput=true;
			}
		}
	}

	public class CaptureFormat
	{
		public int Width;
		public int Height;
		public string Description;

		public override string ToString()
		{
			return Description;
		}
	}
}
public class ComboBoxCaptureCard
{
	private TVCaptureDevice _mCaptureDevice;

	public ComboBoxCaptureCard(TVCaptureDevice pCaptureDevice)
	{
		this._mCaptureDevice = pCaptureDevice;
	}

	public TVCaptureDevice CaptureDevice
	{
		get {return _mCaptureDevice;}
	}
#if (UseCaptureCardDefinitions)
	public string VideoDeviceMoniker
	{
		get {return _mCaptureDevice.VideoDeviceMoniker;}
	}

	public string CaptureName
	{
		get {return _mCaptureDevice.CaptureName;}
	}

	// Display a more readable name by adding the commercial name of the card to the capture device
	public override string ToString()
	{
		return (this.CaptureName + " (" + this._mCaptureDevice.CommercialName + ")");
	}

	public override bool Equals(object obj)
	{
//		return this.ToString().Equals((obj as ComboBoxCaptureCard).ToString());
		return VideoDeviceMoniker.Equals((obj as ComboBoxCaptureCard).VideoDeviceMoniker);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
#endif
}
