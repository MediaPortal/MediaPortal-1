using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class Radio : MediaPortal.Configuration.SectionSettings
	{
		protected MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		protected System.Windows.Forms.Label label2;
		protected System.Windows.Forms.RadioButton internalRadioButton;
		protected System.Windows.Forms.ComboBox deviceComboBox;
		protected System.Windows.Forms.Label label1;
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
			deviceComboBox.Items.AddRange(availableDevices.ToArray());			

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
      this.parametersButton = new System.Windows.Forms.Button();
      this.parametersTextBox = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.fileNameButton = new System.Windows.Forms.Button();
      this.fileNameTextBox = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.externalRadioButton = new System.Windows.Forms.RadioButton();
      this.inputComboBox = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.deviceComboBox = new System.Windows.Forms.ComboBox();
      this.internalRadioButton = new System.Windows.Forms.RadioButton();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.browseFolderButton = new System.Windows.Forms.Button();
      this.folderNameTextBox = new System.Windows.Forms.TextBox();
      this.folderNameLabel = new System.Windows.Forms.Label();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.countryComboBox = new System.Windows.Forms.ComboBox();
      this.label6 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
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
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.deviceComboBox);
      this.groupBox1.Controls.Add(this.internalRadioButton);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 248);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Player Settings";
      // 
      // parametersButton
      // 
      this.parametersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.parametersButton.Location = new System.Drawing.Point(368, 206);
      this.parametersButton.Name = "parametersButton";
      this.parametersButton.Size = new System.Drawing.Size(56, 20);
      this.parametersButton.TabIndex = 13;
      this.parametersButton.Text = "List";
      this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
      // 
      // parametersTextBox
      // 
      this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.parametersTextBox.Location = new System.Drawing.Point(96, 206);
      this.parametersTextBox.Name = "parametersTextBox";
      this.parametersTextBox.Size = new System.Drawing.Size(265, 20);
      this.parametersTextBox.TabIndex = 12;
      this.parametersTextBox.Text = "";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 209);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(80, 23);
      this.label4.TabIndex = 11;
      this.label4.Text = "Parameters";
      // 
      // fileNameButton
      // 
      this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.fileNameButton.Location = new System.Drawing.Point(368, 181);
      this.fileNameButton.Name = "fileNameButton";
      this.fileNameButton.Size = new System.Drawing.Size(56, 20);
      this.fileNameButton.TabIndex = 10;
      this.fileNameButton.Text = "Browse";
      this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
      // 
      // fileNameTextBox
      // 
      this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.fileNameTextBox.Location = new System.Drawing.Point(96, 181);
      this.fileNameTextBox.Name = "fileNameTextBox";
      this.fileNameTextBox.Size = new System.Drawing.Size(265, 20);
      this.fileNameTextBox.TabIndex = 9;
      this.fileNameTextBox.Text = "";
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 184);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(80, 23);
      this.label5.TabIndex = 8;
      this.label5.Text = "Filename";
      // 
      // externalRadioButton
      // 
      this.externalRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.externalRadioButton.Location = new System.Drawing.Point(16, 155);
      this.externalRadioButton.Name = "externalRadioButton";
      this.externalRadioButton.TabIndex = 7;
      this.externalRadioButton.Text = "External radio";
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
      this.inputComboBox.Location = new System.Drawing.Point(168, 93);
      this.inputComboBox.Name = "inputComboBox";
      this.inputComboBox.Size = new System.Drawing.Size(256, 21);
      this.inputComboBox.TabIndex = 6;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 97);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(150, 23);
      this.label3.TabIndex = 5;
      this.label3.Text = "Input source";
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(168, 32);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(256, 32);
      this.label1.TabIndex = 4;
      this.label1.Text = "Please note that the only supported devices are the Hauppauge 350 and USB2.";
      // 
      // deviceComboBox
      // 
      this.deviceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.deviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.deviceComboBox.Location = new System.Drawing.Point(168, 69);
      this.deviceComboBox.Name = "deviceComboBox";
      this.deviceComboBox.Size = new System.Drawing.Size(256, 21);
      this.deviceComboBox.TabIndex = 3;
      // 
      // internalRadioButton
      // 
      this.internalRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.internalRadioButton.Location = new System.Drawing.Point(16, 24);
      this.internalRadioButton.Name = "internalRadioButton";
      this.internalRadioButton.TabIndex = 2;
      this.internalRadioButton.Text = "Internal radio";
      this.internalRadioButton.CheckedChanged += new System.EventHandler(this.internalRadioButton_CheckedChanged);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 72);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(150, 23);
      this.label2.TabIndex = 1;
      this.label2.Text = "Radio device";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.browseFolderButton);
      this.groupBox2.Controls.Add(this.folderNameTextBox);
      this.groupBox2.Controls.Add(this.folderNameLabel);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(8, 264);
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
      this.browseFolderButton.Location = new System.Drawing.Point(369, 27);
      this.browseFolderButton.Name = "browseFolderButton";
      this.browseFolderButton.Size = new System.Drawing.Size(56, 20);
      this.browseFolderButton.TabIndex = 13;
      this.browseFolderButton.Text = "Browse";
      this.browseFolderButton.Click += new System.EventHandler(this.browseFolderButton_Click);
      // 
      // folderNameTextBox
      // 
      this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.folderNameTextBox.Location = new System.Drawing.Point(96, 27);
      this.folderNameTextBox.Name = "folderNameTextBox";
      this.folderNameTextBox.Size = new System.Drawing.Size(265, 20);
      this.folderNameTextBox.TabIndex = 12;
      this.folderNameTextBox.Text = "";
      // 
      // folderNameLabel
      // 
      this.folderNameLabel.Location = new System.Drawing.Point(16, 30);
      this.folderNameLabel.Name = "folderNameLabel";
      this.folderNameLabel.Size = new System.Drawing.Size(80, 23);
      this.folderNameLabel.TabIndex = 11;
      this.folderNameLabel.Text = "Playlist folder";
      // 
      // countryComboBox
      // 
      this.countryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.countryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.countryComboBox.Location = new System.Drawing.Point(168, 117);
      this.countryComboBox.MaxDropDownItems = 16;
      this.countryComboBox.Name = "countryComboBox";
      this.countryComboBox.Size = new System.Drawing.Size(256, 21);
      this.countryComboBox.Sorted = true;
      this.countryComboBox.TabIndex = 17;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 121);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(150, 23);
      this.label6.TabIndex = 16;
      this.label6.Text = "Country";
      // 
      // Radio
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "Radio";
      this.Size = new System.Drawing.Size(456, 448);
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
		}

		protected void internalRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			countryComboBox.Enabled = deviceComboBox.Enabled = inputComboBox.Enabled = true;
			fileNameTextBox.Enabled = fileNameButton.Enabled = parametersButton.Enabled = parametersTextBox.Enabled = false;
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

	}
}

