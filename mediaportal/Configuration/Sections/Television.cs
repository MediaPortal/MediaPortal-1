using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DShowNET;
using DShowNET.Device;

namespace MediaPortal.Configuration.Sections
{
	public class Television : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButton1;
		private MediaPortal.UserInterface.Controls.MPCheckBox alwaysTimeShiftCheckBox;
		private System.Windows.Forms.ComboBox rendererComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.ComboBox audioCodecComboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox videoCodecComboBox;
		private System.Windows.Forms.Label label5;
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
		private System.Windows.Forms.ComboBox countryComboBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox inputComboBox;
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.IContainer components = null;

		public Television() : this("Television")
		{
		}		

		public Television(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Populate the video renderer combobox
			//
			rendererComboBox.Items.AddRange(VideoRenderers.List);

			//
			// Populate the country combobox
			//
			countryComboBox.Items.AddRange(TunerCountries.Countries);

			//
			// Populate video and audio codecs
			//
			ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.MPEG2);
			ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.MPEG2_Audio);

			videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
			audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
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
      this.rendererComboBox = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.alwaysTimeShiftCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.audioCodecComboBox = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.videoCodecComboBox = new System.Windows.Forms.ComboBox();
      this.label5 = new System.Windows.Forms.Label();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.countryComboBox = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.inputComboBox = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.rendererComboBox);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.alwaysTimeShiftCheckBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 96);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "General Settings";
      // 
      // rendererComboBox
      // 
      this.rendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.rendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.rendererComboBox.Location = new System.Drawing.Point(168, 52);
      this.rendererComboBox.Name = "rendererComboBox";
      this.rendererComboBox.Size = new System.Drawing.Size(256, 21);
      this.rendererComboBox.TabIndex = 10;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(150, 23);
      this.label2.TabIndex = 9;
      this.label2.Text = "Video renderer";
      // 
      // alwaysTimeShiftCheckBox
      // 
      this.alwaysTimeShiftCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.alwaysTimeShiftCheckBox.Location = new System.Drawing.Point(16, 24);
      this.alwaysTimeShiftCheckBox.Name = "alwaysTimeShiftCheckBox";
      this.alwaysTimeShiftCheckBox.Size = new System.Drawing.Size(280, 24);
      this.alwaysTimeShiftCheckBox.TabIndex = 6;
      this.alwaysTimeShiftCheckBox.Text = "Always use timeshifting";
      // 
      // radioButton1
      // 
      this.radioButton1.Location = new System.Drawing.Point(0, 0);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.TabIndex = 0;
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.audioCodecComboBox);
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.Controls.Add(this.videoCodecComboBox);
      this.groupBox3.Controls.Add(this.label5);
      this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox3.Location = new System.Drawing.Point(8, 112);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(440, 96);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "MPEG2 Codec Settings";
      // 
      // audioCodecComboBox
      // 
      this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.audioCodecComboBox.Location = new System.Drawing.Point(168, 51);
      this.audioCodecComboBox.Name = "audioCodecComboBox";
      this.audioCodecComboBox.Size = new System.Drawing.Size(256, 21);
      this.audioCodecComboBox.TabIndex = 9;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 55);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(144, 23);
      this.label3.TabIndex = 8;
      this.label3.Text = "Audio codec";
      // 
      // videoCodecComboBox
      // 
      this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.videoCodecComboBox.Location = new System.Drawing.Point(168, 26);
      this.videoCodecComboBox.Name = "videoCodecComboBox";
      this.videoCodecComboBox.Size = new System.Drawing.Size(256, 21);
      this.videoCodecComboBox.TabIndex = 7;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 30);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(144, 23);
      this.label5.TabIndex = 6;
      this.label5.Text = "Video codec";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.countryComboBox);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.inputComboBox);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(8, 216);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(440, 96);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Tuner Settings";
      // 
      // countryComboBox
      // 
      this.countryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.countryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.countryComboBox.Location = new System.Drawing.Point(168, 51);
      this.countryComboBox.MaxDropDownItems = 16;
      this.countryComboBox.Name = "countryComboBox";
      this.countryComboBox.Size = new System.Drawing.Size(256, 21);
      this.countryComboBox.Sorted = true;
      this.countryComboBox.TabIndex = 12;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 55);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(150, 23);
      this.label4.TabIndex = 11;
      this.label4.Text = "Country";
      // 
      // inputComboBox
      // 
      this.inputComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.inputComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.inputComboBox.Items.AddRange(new object[] {
                                                       "Antenna",
                                                       "Cable"});
      this.inputComboBox.Location = new System.Drawing.Point(168, 26);
      this.inputComboBox.Name = "inputComboBox";
      this.inputComboBox.Size = new System.Drawing.Size(256, 21);
      this.inputComboBox.TabIndex = 8;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 30);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(150, 23);
      this.label1.TabIndex = 7;
      this.label1.Text = "Input source";
      // 
      // Television
      // 
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox1);
      this.Name = "Television";
      this.Size = new System.Drawing.Size(456, 448);
      this.groupBox1.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				alwaysTimeShiftCheckBox.Checked = xmlreader.GetValueAsBool("mytv", "alwaystimeshift", false);
				inputComboBox.SelectedItem = xmlreader.GetValueAsString("capture", "tuner", "Antenna");

				//
				// Set video renderer
				//
				int videoRenderer = xmlreader.GetValueAsInt("mytv", "vmr9", 0);

				if(videoRenderer >= 0 && videoRenderer <= VideoRenderers.List.Length)
					rendererComboBox.SelectedItem = VideoRenderers.List[videoRenderer];


				//
				// We can't set the SelectedItem here as the items in the combobox are of TunerCountry type.
				//
				countryComboBox.Text = xmlreader.GetValueAsString("capture", "countryname", "");

				//
				// Set codecs
				//
				audioCodecComboBox.SelectedItem = xmlreader.GetValueAsString("mytv", "audiocodec", "");
				videoCodecComboBox.SelectedItem = xmlreader.GetValueAsString("mytv", "videocodec", "");
			}			
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("mytv", "alwaystimeshift", alwaysTimeShiftCheckBox.Checked);
				xmlwriter.SetValue("capture", "tuner", inputComboBox.Text);

				for(int index = 0; index < VideoRenderers.List.Length; index++)
				{
					if(VideoRenderers.List[index].Equals(rendererComboBox.Text))
					{
						xmlwriter.SetValue("mytv", "vmr9", index);
					}
				}

        if(countryComboBox.Text.Length > 0)
        {
          xmlwriter.SetValue("capture", "countryname", countryComboBox.Text);
        }

				//
				// Set codecs
				//
				xmlwriter.SetValue("mytv", "audiocodec", audioCodecComboBox.Text);
				xmlwriter.SetValue("mytv", "videocodec", videoCodecComboBox.Text);

			}
		}

		public override object GetSetting(string name)
		{
			switch(name)
			{
				case "television.country" :
				{
					int countryId = 0;

					if(countryComboBox.SelectedItem != null)
					{
						TunerCountry tunerCountry = countryComboBox.SelectedItem as TunerCountry;
						countryId = tunerCountry.Id;
					}

					return countryId;
				}

        case "television.countryname" :
          return countryComboBox.Text;
      }

			return null;
		}

    public override void OnSectionActivated()
    {
      //
      // Check if the radio section has changed the country selection
      //
      SectionSettings section = SectionSettings.GetSection("Radio");

      if(section != null)
      {
        //
        // The television section has been loaded
        //
        string selectedCountry = (string)section.GetSetting("radio.countryname");

        if(selectedCountry.Length > 0 && countryComboBox.Text.Equals(selectedCountry) == false)
        {
          //
          // We have other country selection, change our country
          //
          countryComboBox.Text = selectedCountry;
        }
      }
    }
	}
}

