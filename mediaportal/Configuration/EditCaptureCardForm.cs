using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using DShowNET;
using DShowNET.Device;

using DirectX.Capture;

using MediaPortal.TV.Recording;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for EditCaptureCardForm.
	/// </summary>
	public class EditCaptureCardForm : System.Windows.Forms.Form
	{
    bool    m_bMPEG2=false;
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

		int cardId = 0;

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
      ArrayList availableAudioDevices = FilterHelper.GetAudioInputDevices();
      ArrayList availableVideoCompressors = FilterHelper.GetVideoCompressors();
      ArrayList availableAudioCompressors = FilterHelper.GetAudioCompressors();
        
      cardComboBox.Items.AddRange(availableVideoDevices.ToArray());
      audioDeviceComboBox.Items.AddRange(availableAudioDevices.ToArray());
      videoCompressorComboBox.Items.AddRange(availableVideoCompressors.ToArray());
      audioCompressorComboBox.Items.AddRange(availableAudioCompressors.ToArray());

			if(availableVideoDevices.Count == 0)
			{
				MessageBox.Show("No video device was found, you won't be able to configure a capture card", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
				useRecordingCheckBox.Enabled = useWatchingCheckBox.Enabled = filterComboBox.Enabled = cardComboBox.Enabled = okButton.Enabled = setupButton.Enabled = audioCompressorComboBox.Enabled = audioDeviceComboBox.Enabled = videoCompressorComboBox.Enabled = false;
			}

			SetupCaptureFormats();
			frameSizeComboBox.Items.AddRange(captureFormats.ToArray());


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
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
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
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(456, 328);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Capture Card Settings";
      this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
      // 
      // audioDeviceComboBox
      // 
      this.audioDeviceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.audioDeviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioDeviceComboBox.ItemHeight = 13;
      this.audioDeviceComboBox.Location = new System.Drawing.Point(120, 53);
      this.audioDeviceComboBox.Name = "audioDeviceComboBox";
      this.audioDeviceComboBox.Size = new System.Drawing.Size(320, 21);
      this.audioDeviceComboBox.TabIndex = 44;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 56);
      this.label2.Name = "label2";
      this.label2.TabIndex = 43;
      this.label2.Text = "Audio device";
      // 
      // setupButton
      // 
      this.setupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.setupButton.Enabled = false;
      this.setupButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.setupButton.Location = new System.Drawing.Point(366, 79);
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
      this.filterComboBox.Location = new System.Drawing.Point(120, 79);
      this.filterComboBox.Name = "filterComboBox";
      this.filterComboBox.Size = new System.Drawing.Size(240, 21);
      this.filterComboBox.TabIndex = 41;
      this.filterComboBox.SelectedIndexChanged += new System.EventHandler(this.filterComboBox_SelectedIndexChanged);
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(16, 83);
      this.label9.Name = "label9";
      this.label9.TabIndex = 40;
      this.label9.Text = "Filter";
      // 
      // audioCompressorComboBox
      // 
      this.audioCompressorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCompressorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCompressorComboBox.ItemHeight = 13;
      this.audioCompressorComboBox.Location = new System.Drawing.Point(120, 147);
      this.audioCompressorComboBox.Name = "audioCompressorComboBox";
      this.audioCompressorComboBox.Size = new System.Drawing.Size(320, 21);
      this.audioCompressorComboBox.TabIndex = 39;
      this.audioCompressorComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.audioCompressorComboBox_KeyPress);
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 150);
      this.label5.Name = "label5";
      this.label5.TabIndex = 38;
      this.label5.Text = "Audio compressor";
      // 
      // videoCompressorComboBox
      // 
      this.videoCompressorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCompressorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCompressorComboBox.ItemHeight = 13;
      this.videoCompressorComboBox.Location = new System.Drawing.Point(120, 120);
      this.videoCompressorComboBox.Name = "videoCompressorComboBox";
      this.videoCompressorComboBox.Size = new System.Drawing.Size(320, 21);
      this.videoCompressorComboBox.TabIndex = 37;
      this.videoCompressorComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.videoCompressorComboBox_KeyPress);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 123);
      this.label3.Name = "label3";
      this.label3.TabIndex = 36;
      this.label3.Text = "Video compressor";
      // 
      // frameRateTextBox
      // 
      this.frameRateTextBox.Enabled = false;
      this.frameRateTextBox.Location = new System.Drawing.Point(120, 292);
      this.frameRateTextBox.MaxLength = 3;
      this.frameRateTextBox.Name = "frameRateTextBox";
      this.frameRateTextBox.Size = new System.Drawing.Size(40, 20);
      this.frameRateTextBox.TabIndex = 27;
      this.frameRateTextBox.Text = "25";
      this.frameRateTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frameRateTextBox_KeyPress);
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 295);
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
      this.frameSizeComboBox.Location = new System.Drawing.Point(120, 267);
      this.frameSizeComboBox.Name = "frameSizeComboBox";
      this.frameSizeComboBox.Size = new System.Drawing.Size(320, 21);
      this.frameSizeComboBox.TabIndex = 19;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 270);
      this.label7.Name = "label7";
      this.label7.TabIndex = 18;
      this.label7.Text = "Framesize";
      // 
      // useRecordingCheckBox
      // 
      this.useRecordingCheckBox.Checked = true;
      this.useRecordingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.useRecordingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useRecordingCheckBox.Location = new System.Drawing.Point(32, 227);
      this.useRecordingCheckBox.Name = "useRecordingCheckBox";
      this.useRecordingCheckBox.Size = new System.Drawing.Size(248, 24);
      this.useRecordingCheckBox.TabIndex = 15;
      this.useRecordingCheckBox.Text = "Use for recording";
      // 
      // useWatchingCheckBox
      // 
      this.useWatchingCheckBox.Checked = true;
      this.useWatchingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
      this.useWatchingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.useWatchingCheckBox.Location = new System.Drawing.Point(32, 203);
      this.useWatchingCheckBox.Name = "useWatchingCheckBox";
      this.useWatchingCheckBox.Size = new System.Drawing.Size(248, 24);
      this.useWatchingCheckBox.TabIndex = 14;
      this.useWatchingCheckBox.Text = "Use for watching";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 187);
      this.label4.Name = "label4";
      this.label4.TabIndex = 13;
      this.label4.Text = "Purpose";
      // 
      // cardComboBox
      // 
      this.cardComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cardComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cardComboBox.Location = new System.Drawing.Point(120, 26);
      this.cardComboBox.Name = "cardComboBox";
      this.cardComboBox.Size = new System.Drawing.Size(320, 21);
      this.cardComboBox.TabIndex = 1;
      this.cardComboBox.SelectedIndexChanged += new System.EventHandler(this.cardComboBox_SelectedIndexChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 29);
      this.label1.Name = "label1";
      this.label1.TabIndex = 0;
      this.label1.Text = "Video device";
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(389, 344);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(309, 344);
      this.okButton.Name = "okButton";
      this.okButton.TabIndex = 2;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // EditCaptureCardForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(474, 376);
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

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private Capture CreateCaptureDevice()
		{
			Capture capture = null;
			DShowNET.Filter videoDevice = null;

			string selectedVideoDeviceName = (string)cardComboBox.SelectedItem;

      if(selectedVideoDeviceName != null)
      {
        //
        // Find the selected video capture device
        //
        Filters filters = new Filters();
        foreach(Filter filter in filters.VideoInputDevices)
        {
          if(selectedVideoDeviceName.Equals(filter.Name))
          {
            //
            // The device was found
            //
            videoDevice = filter;
            break;
          }
        }
      
        //
        // Create new capture
        //
        try
        {
          capture = new Capture(videoDevice, null);
          capture.LoadSettings(cardId);
          m_bMPEG2=capture.SupportsTimeShifting;
        }
        catch
        {
          return null;
        }
        
        if (m_bMPEG2)
        {
          videoCompressorComboBox.Enabled=false;
          audioCompressorComboBox.Enabled=false;
          audioDeviceComboBox.Enabled=false;
          frameRateTextBox.Enabled=false;
          frameSizeComboBox.Enabled=false;
        }
        else
        {
          videoCompressorComboBox.Enabled=true;
          audioCompressorComboBox.Enabled=true;
          audioDeviceComboBox.Enabled=true;
          frameRateTextBox.Enabled=true;
          frameSizeComboBox.Enabled=true;
        }
      }

      return capture;
		}

		private void SetupPropertyPages()
		{
			//
			// Clear any previous items
			//
			filterComboBox.Items.Clear();

			Capture capture = CreateCaptureDevice();

			if (capture != null) 
			{
				if(capture.PropertyPages != null)
				{
					foreach(PropertyPage page in capture.PropertyPages)
					{
						filterComboBox.Items.Add(page.Name);
					}
				}

				capture.Stop();
				capture.Dispose();
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
			SetupPropertyPages();
		}

		private void setupButton_Click(object sender, System.EventArgs e)
		{
			if(filterComboBox.SelectedItem != null)
			{
				string propertyPageName = (string)filterComboBox.SelectedItem;

				Capture capture = CreateCaptureDevice();

				if(capture != null)
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
								capture.SaveSettings(0);
								break;
							}
						}
					}

					capture.Stop();
					capture.Dispose();
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

    private void groupBox1_Enter(object sender, System.EventArgs e)
    {
    
    }

    private void EditCaptureCardForm_Load(object sender, System.EventArgs e)
    {

      SetupPropertyPages();    
    }

		public TVCaptureDevice CaptureCard
		{
			get 
			{
				TVCaptureDevice card = new TVCaptureDevice();

				card.VideoDevice = cardComboBox.Text;

				card.UseForRecording	= useRecordingCheckBox.Checked;
				card.UseForTV			= useWatchingCheckBox.Checked;
				
        CaptureFormat fmt = (CaptureFormat)frameSizeComboBox.SelectedItem;
				card.FrameSize = new Size(fmt.Width, fmt.Height);
				card.FrameRate = Int32.Parse(frameRateTextBox.Text);

        card.VideoCompressor = videoCompressorComboBox.Text;
        card.AudioCompressor = audioCompressorComboBox.Text;
        card.AudioDevice = audioDeviceComboBox.Text;
        card.SupportsMPEG2=m_bMPEG2;

				return card;
			}

			set
			{
				TVCaptureDevice card = value as TVCaptureDevice;

				if(card != null)
				{
					cardComboBox.SelectedItem = card.VideoDevice;
					useRecordingCheckBox.Checked = card.UseForRecording;
					useWatchingCheckBox.Checked = card.UseForTV;
				
          int i=0;
          foreach (CaptureFormat fmt in captureFormats)
          {
            if (card.FrameSize.Width==fmt.Width && card.FrameSize.Height==fmt.Height)
            {
              frameSizeComboBox.SelectedIndex=i;
              break;
            }
            ++i;
          }
					frameRateTextBox.Text = card.FrameRate.ToString();
          videoCompressorComboBox.SelectedItem = card.VideoCompressor;
          audioCompressorComboBox.SelectedItem = card.AudioCompressor;
          audioDeviceComboBox.SelectedItem = card.AudioDevice;
          m_bMPEG2=card.SupportsMPEG2;
        }
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
