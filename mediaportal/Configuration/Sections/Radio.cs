using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices; 

using DShowNET;

namespace MediaPortal.Configuration.Sections
{
	public class Radio : MediaPortal.Configuration.SectionSettings
	{
		protected MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		protected System.Windows.Forms.Label label2;
		protected System.Windows.Forms.RadioButton internalRadioButton;
		protected System.Windows.Forms.ComboBox deviceComboBox;
		protected System.Windows.Forms.Label label3;
		protected System.Windows.Forms.ComboBox inputComboBox;
		protected System.Windows.Forms.Button parametersButton;
		protected System.Windows.Forms.TextBox parametersTextBox;
		protected System.Windows.Forms.Label label4;
		protected System.Windows.Forms.Button fileNameButton;
		protected System.Windows.Forms.TextBox fileNameTextBox;
		protected System.Windows.Forms.Label label5;
		protected System.Windows.Forms.RadioButton externalRadioButton;
		protected System.Windows.Forms.GroupBox groupBox2;
		protected System.Windows.Forms.TextBox folderNameTextBox;
		protected System.Windows.Forms.Label folderNameLabel;
		protected System.Windows.Forms.Button browseFolderButton;
		protected System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		protected System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.ComboBox countryComboBox;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.ComboBox comboBoxAudioDevice;
    private System.Windows.Forms.ComboBox comboBoxLineInput;
		protected System.ComponentModel.IContainer components = null;

		public Radio() : this("Radio")
		{
		}

		public Radio(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Populate available devices
			//
			ArrayList availableDevices = FilterHelper.GetVideoInputDevices();
			ArrayList availableAudioDevices = FilterHelper.GetAudioInputDevices();

			deviceComboBox.Items.AddRange(availableDevices.ToArray());
			comboBoxAudioDevice.Items.Add("");
			comboBoxAudioDevice.Items.AddRange(availableAudioDevices.ToArray());			
			comboBoxLineInput.Items.Clear();

			//
			// Populate the country combobox
			//
			countryComboBox.Items.AddRange(TunerCountries.Countries);
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
		protected void InitializeComponent()
		{
			this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.comboBoxLineInput = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.comboBoxAudioDevice = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.countryComboBox = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.parametersButton = new System.Windows.Forms.Button();
			this.parametersTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.fileNameButton = new System.Windows.Forms.Button();
			this.fileNameTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.externalRadioButton = new System.Windows.Forms.RadioButton();
			this.inputComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.deviceComboBox = new System.Windows.Forms.ComboBox();
			this.internalRadioButton = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.browseFolderButton = new System.Windows.Forms.Button();
			this.folderNameTextBox = new System.Windows.Forms.TextBox();
			this.folderNameLabel = new System.Windows.Forms.Label();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.comboBoxLineInput);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.comboBoxAudioDevice);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.countryComboBox);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.parametersButton);
			this.groupBox1.Controls.Add(this.parametersTextBox);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.fileNameButton);
			this.groupBox1.Controls.Add(this.fileNameTextBox);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.externalRadioButton);
			this.groupBox1.Controls.Add(this.inputComboBox);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.deviceComboBox);
			this.groupBox1.Controls.Add(this.internalRadioButton);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 296);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Player Settings";
			// 
			// comboBoxLineInput
			// 
			this.comboBoxLineInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxLineInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxLineInput.Location = new System.Drawing.Point(168, 152);
			this.comboBoxLineInput.MaxDropDownItems = 16;
			this.comboBoxLineInput.Name = "comboBoxLineInput";
			this.comboBoxLineInput.Size = new System.Drawing.Size(256, 21);
			this.comboBoxLineInput.Sorted = true;
			this.comboBoxLineInput.TabIndex = 5;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 157);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(136, 16);
			this.label7.TabIndex = 20;
			this.label7.Text = "Line input";
			// 
			// comboBoxAudioDevice
			// 
			this.comboBoxAudioDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxAudioDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAudioDevice.Location = new System.Drawing.Point(168, 128);
			this.comboBoxAudioDevice.MaxDropDownItems = 16;
			this.comboBoxAudioDevice.Name = "comboBoxAudioDevice";
			this.comboBoxAudioDevice.Size = new System.Drawing.Size(256, 21);
			this.comboBoxAudioDevice.Sorted = true;
			this.comboBoxAudioDevice.TabIndex = 4;
			this.comboBoxAudioDevice.SelectedIndexChanged += new System.EventHandler(this.comboBoxAudioDevice_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 133);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(136, 16);
			this.label1.TabIndex = 18;
			this.label1.Text = "Audio device";
			// 
			// countryComboBox
			// 
			this.countryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.countryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.countryComboBox.Location = new System.Drawing.Point(168, 102);
			this.countryComboBox.MaxDropDownItems = 16;
			this.countryComboBox.Name = "countryComboBox";
			this.countryComboBox.Size = new System.Drawing.Size(256, 21);
			this.countryComboBox.Sorted = true;
			this.countryComboBox.TabIndex = 3;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 106);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(150, 23);
			this.label6.TabIndex = 16;
			this.label6.Text = "Country";
			// 
			// parametersButton
			// 
			this.parametersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.parametersButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.parametersButton.Location = new System.Drawing.Point(368, 248);
			this.parametersButton.Name = "parametersButton";
			this.parametersButton.Size = new System.Drawing.Size(56, 20);
			this.parametersButton.TabIndex = 10;
			this.parametersButton.Text = "List";
			this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
			// 
			// parametersTextBox
			// 
			this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.parametersTextBox.Location = new System.Drawing.Point(96, 248);
			this.parametersTextBox.Name = "parametersTextBox";
			this.parametersTextBox.Size = new System.Drawing.Size(265, 20);
			this.parametersTextBox.TabIndex = 9;
			this.parametersTextBox.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 248);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(80, 23);
			this.label4.TabIndex = 11;
			this.label4.Text = "Parameters";
			// 
			// fileNameButton
			// 
			this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.fileNameButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.fileNameButton.Location = new System.Drawing.Point(368, 224);
			this.fileNameButton.Name = "fileNameButton";
			this.fileNameButton.Size = new System.Drawing.Size(56, 20);
			this.fileNameButton.TabIndex = 8;
			this.fileNameButton.Text = "Browse";
			this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
			// 
			// fileNameTextBox
			// 
			this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.fileNameTextBox.Location = new System.Drawing.Point(96, 224);
			this.fileNameTextBox.Name = "fileNameTextBox";
			this.fileNameTextBox.Size = new System.Drawing.Size(265, 20);
			this.fileNameTextBox.TabIndex = 7;
			this.fileNameTextBox.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 224);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 23);
			this.label5.TabIndex = 8;
			this.label5.Text = "Filename";
			// 
			// externalRadioButton
			// 
			this.externalRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.externalRadioButton.Location = new System.Drawing.Point(16, 192);
			this.externalRadioButton.Name = "externalRadioButton";
			this.externalRadioButton.Size = new System.Drawing.Size(144, 24);
			this.externalRadioButton.TabIndex = 6;
			this.externalRadioButton.Text = "Use External radio tuner";
			this.externalRadioButton.CheckedChanged += new System.EventHandler(this.externalRadioButton_CheckedChanged);
			// 
			// inputComboBox
			// 
			this.inputComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.inputComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.inputComboBox.Items.AddRange(new object[] {
																											 "Antenna",
																											 "Cable"});
			this.inputComboBox.Location = new System.Drawing.Point(168, 78);
			this.inputComboBox.Name = "inputComboBox";
			this.inputComboBox.Size = new System.Drawing.Size(256, 21);
			this.inputComboBox.TabIndex = 2;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 82);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(150, 23);
			this.label3.TabIndex = 5;
			this.label3.Text = "Input source";
			// 
			// deviceComboBox
			// 
			this.deviceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.deviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.deviceComboBox.Location = new System.Drawing.Point(168, 54);
			this.deviceComboBox.Name = "deviceComboBox";
			this.deviceComboBox.Size = new System.Drawing.Size(256, 21);
			this.deviceComboBox.TabIndex = 1;
			this.deviceComboBox.SelectedIndexChanged += new System.EventHandler(this.deviceComboBox_SelectedIndexChanged);
			// 
			// internalRadioButton
			// 
			this.internalRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.internalRadioButton.Location = new System.Drawing.Point(16, 24);
			this.internalRadioButton.Name = "internalRadioButton";
			this.internalRadioButton.Size = new System.Drawing.Size(144, 24);
			this.internalRadioButton.TabIndex = 0;
			this.internalRadioButton.Text = "Use Internal radio tuner";
			this.internalRadioButton.CheckedChanged += new System.EventHandler(this.internalRadioButton_CheckedChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 57);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(150, 23);
			this.label2.TabIndex = 1;
			this.label2.Text = "Radio tuner card";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.browseFolderButton);
			this.groupBox2.Controls.Add(this.folderNameTextBox);
			this.groupBox2.Controls.Add(this.folderNameLabel);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 312);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 72);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Stream Settings";
			// 
			// browseFolderButton
			// 
			this.browseFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.browseFolderButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.browseFolderButton.Location = new System.Drawing.Point(360, 40);
			this.browseFolderButton.Name = "browseFolderButton";
			this.browseFolderButton.Size = new System.Drawing.Size(56, 20);
			this.browseFolderButton.TabIndex = 1;
			this.browseFolderButton.Text = "Browse";
			this.browseFolderButton.Click += new System.EventHandler(this.browseFolderButton_Click);
			// 
			// folderNameTextBox
			// 
			this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.folderNameTextBox.Location = new System.Drawing.Point(40, 40);
			this.folderNameTextBox.Name = "folderNameTextBox";
			this.folderNameTextBox.Size = new System.Drawing.Size(304, 20);
			this.folderNameTextBox.TabIndex = 0;
			this.folderNameTextBox.Text = "";
			// 
			// folderNameLabel
			// 
			this.folderNameLabel.Location = new System.Drawing.Point(16, 16);
			this.folderNameLabel.Name = "folderNameLabel";
			this.folderNameLabel.Size = new System.Drawing.Size(240, 16);
			this.folderNameLabel.TabIndex = 11;
			this.folderNameLabel.Text = "Folder where internet streams are stored:";
			// 
			// Radio
			// 
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "Radio";
			this.Size = new System.Drawing.Size(456, 448);
			this.Load += new System.EventHandler(this.Radio_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

    public override void OnSectionActivated()
    {
      //
      // Check if the television section has changed the country selection
      //
      SectionSettings section = SectionSettings.GetSection("Television");

      if(section != null)
      {
        //
        // The television section has been loaded
        //
        string selectedCountry = (string)section.GetSetting("television.countryname");

        if(selectedCountry.Length > 0 && countryComboBox.Text.Equals(selectedCountry) == false)
        {
          //
          // We have other country selection, change our country
          //
          countryComboBox.Text = selectedCountry;
        }
      }
    }

		protected void externalRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			countryComboBox.Enabled = deviceComboBox.Enabled = inputComboBox.Enabled = false;
			fileNameTextBox.Enabled = fileNameButton.Enabled = parametersButton.Enabled = parametersTextBox.Enabled = true;
			comboBoxAudioDevice.Enabled=false;
			comboBoxLineInput.Enabled=false;
		}

		protected void internalRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			countryComboBox.Enabled = deviceComboBox.Enabled = inputComboBox.Enabled = true;
			fileNameTextBox.Enabled = fileNameButton.Enabled = parametersButton.Enabled = parametersTextBox.Enabled = false;
			comboBoxAudioDevice.Enabled=true;
			comboBoxLineInput.Enabled=true;
		}

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				folderNameTextBox.Text = xmlreader.GetValueAsString("radio", "folder", "");

				internalRadioButton.Checked = xmlreader.GetValueAsBool("radio", "internal", true);
				externalRadioButton.Checked = !internalRadioButton.Checked;

				inputComboBox.SelectedItem = xmlreader.GetValueAsString("radio", "tuner", "Antenna");
				deviceComboBox.SelectedItem = xmlreader.GetValueAsString("radio", "device", "");

			    fileNameTextBox.Text = xmlreader.GetValueAsString("radio", "player", "");
				parametersTextBox.Text = xmlreader.GetValueAsString("radio", "args", "");

				countryComboBox.Text = xmlreader.GetValueAsString("capture", "countryname", "");

				comboBoxAudioDevice.SelectedItem=xmlreader.GetValueAsString("radio", "audiodevice", "");
				comboBoxLineInput.SelectedItem=xmlreader.GetValueAsString("radio", "lineinput", "");

				if (comboBoxAudioDevice.SelectedIndex<0 && comboBoxAudioDevice.Items.Count>0)
				{
				comboBoxAudioDevice.SelectedIndex=0;
				}
			}
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("radio", "folder", folderNameTextBox.Text);
				xmlwriter.SetValueAsBool("radio", "internal", internalRadioButton.Checked);
				xmlwriter.SetValue("radio", "tuner", inputComboBox.Text);
				xmlwriter.SetValue("radio", "device", deviceComboBox.Text);
				xmlwriter.SetValue("radio", "player", fileNameTextBox.Text);
				xmlwriter.SetValue("radio", "args", parametersTextBox.Text);

				if(countryComboBox.Text.Length > 0)
				{
				xmlwriter.SetValue("capture", "countryname", countryComboBox.Text);
				}
		        
				if (comboBoxAudioDevice.Text.Length>0)
				xmlwriter.SetValue("radio", "audiodevice", comboBoxAudioDevice.Text);

				if (comboBoxLineInput.Text.Length>0)
				xmlwriter.SetValue("radio", "lineinput", comboBoxLineInput.Text);
		        
			}
		}

		public override object GetSetting(string name)
		{
			switch(name)
			{
				case "radio.internal":
					return internalRadioButton.Checked;

				case "radio.device":
					return deviceComboBox.SelectedItem;

				case "radio.tuner":
					return inputComboBox.SelectedItem;

        case "radio.country":
        {
          int countryId = 0;

          if(countryComboBox.SelectedItem != null)
          {
            TunerCountry tunerCountry = countryComboBox.SelectedItem as TunerCountry;
            countryId = tunerCountry.Id;
          }

          return countryId;
        }

        case "radio.countryname":
          return countryComboBox.Text;
      }

			return String.Empty;
		}

		protected void browseFolderButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where stream playlists will be stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
				}
			}					
		}

		protected void parametersButton_Click(object sender, System.EventArgs e)
		{
			ParameterForm parameters = new ParameterForm();
			
			parameters.AddParameter("%MHZ%", "Will be replaced by frequency in form: 104.5");
			parameters.AddParameter("%mhz%", "Will be replaced by frequency in form: 104,5");
			parameters.AddParameter("%HZ%", "Will be replaced by frequency in form: 104500000");

			if(parameters.ShowDialog(parametersButton) == DialogResult.OK)
			{
				parametersTextBox.Text += parameters.SelectedParameter;
			}		
		}

		protected void fileNameButton_Click(object sender, System.EventArgs e)
		{
			using(openFileDialog = new OpenFileDialog())
			{
				openFileDialog.FileName = fileNameTextBox.Text;
				openFileDialog.CheckFileExists = true;
				openFileDialog.RestoreDirectory=true;
				openFileDialog.Filter= "exe files (*.exe)|*.exe";
				openFileDialog.FilterIndex = 0;
				openFileDialog.Title = "Select radio player";

				DialogResult dialogResult = openFileDialog.ShowDialog();

				if(dialogResult == DialogResult.OK)
				{
					fileNameTextBox.Text = openFileDialog.FileName;
				}
			}		
		}

    private void comboBoxAudioDevice_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      FillLineInputs();
      if (comboBoxLineInput.Items.Count>0)
      {
        comboBoxLineInput.SelectedIndex=0;
      }
    }

    void FillLineInputs()
    {
      comboBoxLineInput.Items.Clear();
      string Device=comboBoxAudioDevice.Text;
	  if (Device==String.Empty) return;
      Filters filters = new Filters();
      foreach (Filter filter in filters.AudioInputDevices)
      {
        if (filter.Name.Equals(Device))
        {
          try
          {
            IBaseFilter audioDevice = Marshal.BindToMoniker(filter.MonikerString) as IBaseFilter;
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
            Marshal.ReleaseComObject(audioDevice);
          }
          catch(Exception)
          {
          }
          return;
        }
      }
    }

		private void deviceComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (deviceComboBox.Text=="Hauppauge WinTV PVR USB2 Encoder")
			{
				comboBoxAudioDevice.SelectedIndex=0;
				comboBoxAudioDevice.Enabled = false;
				comboBoxLineInput.Enabled = false;
			}
			else 
			{
				if (comboBoxAudioDevice.Items.Count >0)
					comboBoxAudioDevice.SelectedIndex=0;
				comboBoxAudioDevice.Enabled = true;
				comboBoxLineInput.Enabled = true;
			}
		}

		private void Radio_Load(object sender, System.EventArgs e)
		{
		
		}
	}
}

