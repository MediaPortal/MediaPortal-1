using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using DShowNET;

using System.Globalization;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for EditTVChannelForm.
	/// </summary>
	public class EditTVChannelForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.TextBox channelTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox frequencyTextBox;
		private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button advancedButton;
    private System.Windows.Forms.GroupBox advancedGroupBox;
    private System.Windows.Forms.TextBox externalChannelTextBox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox typeComboBox;
    private System.Windows.Forms.ComboBox inputComboBox;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.ComboBox comboTvStandard;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EditTVChannelForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// Fetch highest channel number
			//
			SectionSettings channelSection = SectionSettings.GetSection("Channels");
			int highestChannelNumber = (int)channelSection.GetSetting("channel.highest");

			channelTextBox.Text = String.Format("{0}", highestChannelNumber + 1);

      //
      // Set size of window
      //
      typeComboBox.Text = "Internal";
      comboTvStandard.Text = "Default";
      advancedGroupBox.Visible = false;
      this.Height = 208;
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
      this.comboTvStandard = new System.Windows.Forms.ComboBox();
      this.label7 = new System.Windows.Forms.Label();
      this.frequencyTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.channelTextBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.advancedButton = new System.Windows.Forms.Button();
      this.advancedGroupBox = new System.Windows.Forms.GroupBox();
      this.inputComboBox = new System.Windows.Forms.ComboBox();
      this.label6 = new System.Windows.Forms.Label();
      this.typeComboBox = new System.Windows.Forms.ComboBox();
      this.label5 = new System.Windows.Forms.Label();
      this.externalChannelTextBox = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.advancedGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.comboTvStandard);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.frequencyTextBox);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.channelTextBox);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.nameTextBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(360, 136);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Channel Settings";
      // 
      // comboTvStandard
      // 
      this.comboTvStandard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboTvStandard.Items.AddRange(new object[] {
                                                         "Default",
                                                         "NTSC M",
                                                         "NTSC M J",
                                                         "NTSC 433",
                                                         "PAL B",
                                                         "PAL D",
                                                         "PAL G",
                                                         "PAL H",
                                                         "PAL I",
                                                         "PAL M",
                                                         "PAL N",
                                                         "PAL 60",
                                                         "SECAM B",
                                                         "SECAM D",
                                                         "SECAM G",
                                                         "SECAM H",
                                                         "SECAM K",
                                                         "SECAM K1",
                                                         "SECAM L",
                                                         "SECAM L1",
                                                         "PAL N COMBO"});
      this.comboTvStandard.Location = new System.Drawing.Point(120, 93);
      this.comboTvStandard.Name = "comboTvStandard";
      this.comboTvStandard.Size = new System.Drawing.Size(224, 21);
      this.comboTvStandard.TabIndex = 12;
      this.comboTvStandard.SelectedIndexChanged += new System.EventHandler(this.comboTvStandard_SelectedIndexChanged);
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 96);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(64, 16);
      this.label7.TabIndex = 11;
      this.label7.Text = "Standard:";
      // 
      // frequencyTextBox
      // 
      this.frequencyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.frequencyTextBox.Location = new System.Drawing.Point(120, 69);
      this.frequencyTextBox.MaxLength = 10;
      this.frequencyTextBox.Name = "frequencyTextBox";
      this.frequencyTextBox.Size = new System.Drawing.Size(224, 20);
      this.frequencyTextBox.TabIndex = 9;
      this.frequencyTextBox.Text = "0";
      this.frequencyTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frequencyTextBox_KeyPress);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 72);
      this.label1.Name = "label1";
      this.label1.TabIndex = 10;
      this.label1.Text = "Frequency";
      // 
      // channelTextBox
      // 
      this.channelTextBox.Location = new System.Drawing.Point(120, 45);
      this.channelTextBox.MaxLength = 4;
      this.channelTextBox.Name = "channelTextBox";
      this.channelTextBox.Size = new System.Drawing.Size(40, 20);
      this.channelTextBox.TabIndex = 7;
      this.channelTextBox.Text = "0";
      this.channelTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.channelTextBox_KeyPress);
      this.channelTextBox.TextChanged += new System.EventHandler(this.channelTextBox_TextChanged);
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 48);
      this.label3.Name = "label3";
      this.label3.TabIndex = 8;
      this.label3.Text = "Channel";
      // 
      // nameTextBox
      // 
      this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.nameTextBox.Location = new System.Drawing.Point(120, 21);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(224, 20);
      this.nameTextBox.TabIndex = 5;
      this.nameTextBox.Text = "";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 24);
      this.label2.Name = "label2";
      this.label2.TabIndex = 6;
      this.label2.Text = "Name";
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(293, 287);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(213, 287);
      this.okButton.Name = "okButton";
      this.okButton.TabIndex = 2;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // advancedButton
      // 
      this.advancedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.advancedButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.advancedButton.Location = new System.Drawing.Point(8, 288);
      this.advancedButton.Name = "advancedButton";
      this.advancedButton.TabIndex = 3;
      this.advancedButton.Text = "Advanced >>";
      this.advancedButton.Click += new System.EventHandler(this.advancedButton_Click);
      // 
      // advancedGroupBox
      // 
      this.advancedGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.advancedGroupBox.Controls.Add(this.inputComboBox);
      this.advancedGroupBox.Controls.Add(this.label6);
      this.advancedGroupBox.Controls.Add(this.typeComboBox);
      this.advancedGroupBox.Controls.Add(this.label5);
      this.advancedGroupBox.Controls.Add(this.externalChannelTextBox);
      this.advancedGroupBox.Controls.Add(this.label4);
      this.advancedGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.advancedGroupBox.Location = new System.Drawing.Point(8, 152);
      this.advancedGroupBox.Name = "advancedGroupBox";
      this.advancedGroupBox.Size = new System.Drawing.Size(360, 128);
      this.advancedGroupBox.TabIndex = 4;
      this.advancedGroupBox.TabStop = false;
      this.advancedGroupBox.Text = "Advanced Settings";
      // 
      // inputComboBox
      // 
      this.inputComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.inputComboBox.Enabled = false;
      this.inputComboBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.inputComboBox.Items.AddRange(new object[] {
                                                       "Composite #1",
                                                       "Composite #2",
                                                       "SVHS"});
      this.inputComboBox.Location = new System.Drawing.Point(120, 54);
      this.inputComboBox.Name = "inputComboBox";
      this.inputComboBox.Size = new System.Drawing.Size(224, 21);
      this.inputComboBox.TabIndex = 13;
      this.inputComboBox.SelectedIndexChanged += new System.EventHandler(this.inputComboBox_SelectedIndexChanged);
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 58);
      this.label6.Name = "label6";
      this.label6.TabIndex = 12;
      this.label6.Text = "Input";
      // 
      // typeComboBox
      // 
      this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.typeComboBox.Items.AddRange(new object[] {
                                                      "Internal",
                                                      "External"});
      this.typeComboBox.Location = new System.Drawing.Point(120, 28);
      this.typeComboBox.Name = "typeComboBox";
      this.typeComboBox.Size = new System.Drawing.Size(224, 21);
      this.typeComboBox.TabIndex = 11;
      this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.typeComboBox_SelectedIndexChanged);
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 32);
      this.label5.Name = "label5";
      this.label5.TabIndex = 10;
      this.label5.Text = "Type";
      // 
      // externalChannelTextBox
      // 
      this.externalChannelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.externalChannelTextBox.Enabled = false;
      this.externalChannelTextBox.Location = new System.Drawing.Point(120, 80);
      this.externalChannelTextBox.Name = "externalChannelTextBox";
      this.externalChannelTextBox.Size = new System.Drawing.Size(224, 20);
      this.externalChannelTextBox.TabIndex = 7;
      this.externalChannelTextBox.Text = "";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 84);
      this.label4.Name = "label4";
      this.label4.TabIndex = 8;
      this.label4.Text = "External channel";
      // 
      // EditTVChannelForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(376, 318);
      this.Controls.Add(this.advancedGroupBox);
      this.Controls.Add(this.advancedButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(384, 208);
      this.Name = "EditTVChannelForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "EditTVChannelForm";
      this.Load += new System.EventHandler(this.EditTVChannelForm_Load);
      this.groupBox1.ResumeLayout(false);
      this.advancedGroupBox.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		private void channelTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}
		}

		private void frequencyTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//
			// Make sure we only type one comma or dot
			//
			if(e.KeyChar == '.' || e.KeyChar == ',')
			{
				if(frequencyTextBox.Text.IndexOfAny(new char[] {',','.'}) >= 0)
				{
					e.Handled = true;
					return;
				}
			}
			
			if(char.IsNumber(e.KeyChar) == false && (e.KeyChar != 8 && e.KeyChar != '.' && e.KeyChar != ','))
			{
				e.Handled = true;
			}		
		}

		private void okButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Hide();
		}

    private void channelTextBox_TextChanged(object sender, System.EventArgs e)
    {
      if(channelTextBox.Text.Length > 0)
      {
        int channel = Int32.Parse(channelTextBox.Text);
        frequencyTextBox.Enabled = (channel > 0 && channel < 1000);
      }
    }    

    bool advancedShowing = false;

    private void advancedButton_Click(object sender, System.EventArgs e)
    {
      if(advancedShowing == false)
      {
        //
        // Change button text
        //
        advancedButton.Text = "<< Simple";
        advancedGroupBox.Visible = true;

        this.Height = 336;
      }
      else
      {
        //
        // Change button text
        //
        advancedButton.Text = "Advanced >>";
        advancedGroupBox.Visible = false;

        this.Height = 208;
      }

      //
      // Toggle
      //
      advancedShowing = !advancedShowing;
    }

    private void typeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      externalChannelTextBox.Enabled = inputComboBox.Enabled = typeComboBox.Text.Equals("External");
      channelTextBox.Enabled = frequencyTextBox.Enabled = !externalChannelTextBox.Enabled;
    }

    private void inputComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      switch(inputComboBox.Text)
      {
        case "SVHS":
          channelTextBox.Text = "1000";
          break;

        case "Composite #1":
          channelTextBox.Text = "1001";
          break;

        case "Composite #2":
          channelTextBox.Text = "1002";
          break;
      }
    }

    private void comboTvStandard_SelectedIndexChanged(object sender, System.EventArgs e)
    {
    }

    private void EditTVChannelForm_Load(object sender, System.EventArgs e)
    {
    
    }

		public TelevisionChannel Channel
		{
			get 
			{
				TelevisionChannel channel = new TelevisionChannel();

				channel.Name = nameTextBox.Text;
				channel.Channel = Convert.ToInt32(channelTextBox.Text.Length > 0 ? channelTextBox.Text : "0");

				try
				{

					if(frequencyTextBox.Text.IndexOfAny(new char[] { ',','.' }) >= 0)
					{
						char[] separators = new char[] {'.', ','};

						for(int index = 0; index < separators.Length; index++)
						{
							try
							{
								frequencyTextBox.Text = frequencyTextBox.Text.Replace(',', separators[index]);
								frequencyTextBox.Text = frequencyTextBox.Text.Replace('.', separators[index]);

								//
								// MegaHerz
								//
								channel.Frequency = Convert.ToDouble(frequencyTextBox.Text.Length > 0 ? frequencyTextBox.Text : "0", CultureInfo.InvariantCulture);

								break;
							}
							catch
							{
								//
								// Failed to convert, try next separator
								//
							}
						}
					}
					else
					{
            //
            // Herz
            //
            if(frequencyTextBox.Text.Length > 3)
            {
              channel.Frequency = Convert.ToInt32(frequencyTextBox.Text.Length > 0 ? frequencyTextBox.Text : "0");
            }
            else
            {
              channel.Frequency = Convert.ToDouble(frequencyTextBox.Text.Length > 0 ? frequencyTextBox.Text : "0", CultureInfo.InvariantCulture);
            }					
          }
				}
				catch
				{
					channel.Frequency = 0;
				}

        //
        // Fetch advanced settings
        //
        channel.External = typeComboBox.Text.Equals("External");
        channel.ExternalTunerChannel = externalChannelTextBox.Text;

        if(channel.External)
        {
          channel.Frequency = 0;
        }

        string standard=comboTvStandard.Text;
        if (standard=="Default") channel.standard = AnalogVideoStandard.None;
        if (standard=="NTSC M") channel.standard = AnalogVideoStandard.NTSC_M;
        if (standard=="NTSC M J") channel.standard = AnalogVideoStandard.NTSC_M_J;
        if (standard=="NTSC 433") channel.standard = AnalogVideoStandard.NTSC_433;
        if (standard=="PAL B") channel.standard = AnalogVideoStandard.PAL_B;
        if (standard=="PAL D") channel.standard = AnalogVideoStandard.PAL_D;
        if (standard=="PAL G") channel.standard = AnalogVideoStandard.PAL_G;
        if (standard=="PAL H") channel.standard = AnalogVideoStandard.PAL_H;
        if (standard=="PAL I") channel.standard = AnalogVideoStandard.PAL_I;
        if (standard=="PAL M") channel.standard = AnalogVideoStandard.PAL_M;
        if (standard=="PAL N") channel.standard = AnalogVideoStandard.PAL_N;
        if (standard=="PAL 60") channel.standard = AnalogVideoStandard.PAL_60;
        if (standard=="SECAM B") channel.standard = AnalogVideoStandard.SECAM_B;
        if (standard=="SECAM D") channel.standard = AnalogVideoStandard.SECAM_D;
        if (standard=="SECAM G") channel.standard = AnalogVideoStandard.SECAM_G;
        if (standard=="SECAM H") channel.standard = AnalogVideoStandard.SECAM_H;
        if (standard=="SECAM K") channel.standard = AnalogVideoStandard.SECAM_K;
        if (standard=="SECAM K1") channel.standard = AnalogVideoStandard.SECAM_K1;
        if (standard=="SECAM L") channel.standard = AnalogVideoStandard.SECAM_L;
        if (standard=="SECAM L1") channel.standard = AnalogVideoStandard.SECAM_L1;
        if (standard=="PAL N COMBO") channel.standard = AnalogVideoStandard.PAL_N_COMBO;
				return channel;
			}

			set
			{
				TelevisionChannel channel = value as TelevisionChannel;

				if(channel != null)
				{
					nameTextBox.Text = channel.Name;
					channelTextBox.Text = channel.Channel.ToString();
					frequencyTextBox.Text = channel.Frequency.ToString();

          typeComboBox.Text = channel.External ? "External" : "Internal";
          externalChannelTextBox.Text = channel.ExternalTunerChannel;

          if(channel.External == true)
          {
            switch(channel.Channel)
            {
              case 1000:
                inputComboBox.Text = "SVHS";
                break;

              case 1001:
                inputComboBox.Text = "Composite #1";
                break;

              case 1002:
                inputComboBox.Text = "Composite #2";
                break;
            }
          }

          //
          // Disable boxes for static channels
          //
          if(channel.Name.Equals("Composite #1") || channel.Name.Equals("Composite #2") || channel.Name.Equals("SVHS"))
          {
            comboTvStandard.Enabled = nameTextBox.Enabled = channelTextBox.Enabled = frequencyTextBox.Enabled = advancedButton.Enabled = false;
          }
          comboTvStandard.SelectedIndex=0;
          if ( channel.standard == AnalogVideoStandard.None) comboTvStandard.SelectedIndex=0;
          if ( channel.standard == AnalogVideoStandard.NTSC_M) comboTvStandard.SelectedIndex=1;
          if ( channel.standard == AnalogVideoStandard.NTSC_M_J) comboTvStandard.SelectedIndex=2;
          if ( channel.standard == AnalogVideoStandard.NTSC_433) comboTvStandard.SelectedIndex=3;
          if ( channel.standard == AnalogVideoStandard.PAL_B) comboTvStandard.SelectedIndex=4;
          if ( channel.standard == AnalogVideoStandard.PAL_D) comboTvStandard.SelectedIndex=5;
          if ( channel.standard == AnalogVideoStandard.PAL_G) comboTvStandard.SelectedIndex=6;
          if ( channel.standard == AnalogVideoStandard.PAL_H) comboTvStandard.SelectedIndex=7;
          if ( channel.standard == AnalogVideoStandard.PAL_I) comboTvStandard.SelectedIndex=8;
          if ( channel.standard == AnalogVideoStandard.PAL_M) comboTvStandard.SelectedIndex=9;
          if ( channel.standard == AnalogVideoStandard.PAL_N) comboTvStandard.SelectedIndex=10;
          if ( channel.standard == AnalogVideoStandard.PAL_60) comboTvStandard.SelectedIndex=11;
          if ( channel.standard == AnalogVideoStandard.SECAM_B) comboTvStandard.SelectedIndex=12;
          if ( channel.standard == AnalogVideoStandard.SECAM_D) comboTvStandard.SelectedIndex=13;
          if ( channel.standard == AnalogVideoStandard.SECAM_G) comboTvStandard.SelectedIndex=14;
          if ( channel.standard == AnalogVideoStandard.SECAM_H) comboTvStandard.SelectedIndex=15;
          if ( channel.standard == AnalogVideoStandard.SECAM_K) comboTvStandard.SelectedIndex=16;
          if ( channel.standard == AnalogVideoStandard.SECAM_K1) comboTvStandard.SelectedIndex=17;
          if ( channel.standard == AnalogVideoStandard.SECAM_L) comboTvStandard.SelectedIndex=18;
          if ( channel.standard == AnalogVideoStandard.SECAM_L1) comboTvStandard.SelectedIndex=19;
          if ( channel.standard == AnalogVideoStandard.PAL_N_COMBO) comboTvStandard.SelectedIndex=20;
        }
			}
		}
	}

	public class TelevisionChannel
	{
		public string Name = String.Empty;
		public int Channel = 0;
		public Frequency Frequency = new Frequency(0);
    public bool External = false;
    public string ExternalTunerChannel = String.Empty;
    public bool VisibleInGuide = true;
    public AnalogVideoStandard standard=AnalogVideoStandard.None;
	}

	public class Frequency
	{
		public enum Format
		{
			Herz,
			MegaHerz
		}

		public Frequency(long herz)
		{
			this.herz = herz;
		}

		private long herz = 0;

		public long Herz
		{
			get { return herz; }
			set { herz = value; 
				if(herz <= 1000)
					herz *= (int)1000000d;
			}
		}

		public double MegaHerz
		{
			get { return (double)herz / 1000000d; }
		}

		public static implicit operator Frequency(int herz)
		{
			return new Frequency(herz);
		}

		public static implicit operator Frequency(long herz)
		{
			return new Frequency(herz);
		}

		public static implicit operator Frequency(double megaHerz)
		{
			return new Frequency((long)(megaHerz * (1000000d)));
		}

		public string ToString(Format format)
		{
			string result = String.Empty;

			try
			{
				switch(format)
				{
					case Format.Herz:
						result = String.Format("{0}", Herz);
						break;

					case Format.MegaHerz:
						result = String.Format("{0:#,###0.000}", MegaHerz);
						break;
				}
			}
			catch
			{
				//
				// Failed to convert
				//
			}

			return result;
		}

		public override string ToString()
		{
			return ToString(Format.MegaHerz);
		}
	}

}
