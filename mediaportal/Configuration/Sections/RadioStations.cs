using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DShowNET;
using DirectX.Capture;

using SQLite.NET;
using MediaPortal.Radio.Database;

namespace MediaPortal.Configuration.Sections
{
	public class RadioStations : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Button autoTuneButton;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button editButton;
		private System.Windows.Forms.Button addButton;
		private MediaPortal.UserInterface.Controls.MPListView stationsListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Button upButton;
    private System.Windows.Forms.Button downButton;

		//
		// Private members
		//
		bool isDirty = false;
    ListViewItem currentlyCheckedItem = null;

		public RadioStations() : this("Stations")
		{
		}

		public RadioStations(string name) : base(name)
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

		public override void OnSectionActivated()
		{
			//
			// Fetch radio settings
			//
			SectionSettings radioSection = SectionSettings.GetSection("Radio");

			bool internalRadioEnabled = (bool)radioSection.GetSetting("radio.internal");

			autoTuneButton.Enabled = internalRadioEnabled;
		}


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.upButton = new System.Windows.Forms.Button();
      this.downButton = new System.Windows.Forms.Button();
      this.autoTuneButton = new System.Windows.Forms.Button();
      this.deleteButton = new System.Windows.Forms.Button();
      this.editButton = new System.Windows.Forms.Button();
      this.addButton = new System.Windows.Forms.Button();
      this.stationsListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Frequency";
      this.columnHeader3.Width = 54;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.upButton);
      this.groupBox1.Controls.Add(this.downButton);
      this.groupBox1.Controls.Add(this.autoTuneButton);
      this.groupBox1.Controls.Add(this.deleteButton);
      this.groupBox1.Controls.Add(this.editButton);
      this.groupBox1.Controls.Add(this.addButton);
      this.groupBox1.Controls.Add(this.stationsListView);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(456, 416);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // upButton
      // 
      this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.upButton.Enabled = false;
      this.upButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.upButton.Location = new System.Drawing.Point(340, 376);
      this.upButton.Name = "upButton";
      this.upButton.Size = new System.Drawing.Size(48, 23);
      this.upButton.TabIndex = 7;
      this.upButton.Text = "Up";
      this.upButton.Click += new System.EventHandler(this.upButton_Click);
      // 
      // downButton
      // 
      this.downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.downButton.Enabled = false;
      this.downButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.downButton.Location = new System.Drawing.Point(392, 376);
      this.downButton.Name = "downButton";
      this.downButton.Size = new System.Drawing.Size(48, 23);
      this.downButton.TabIndex = 6;
      this.downButton.Text = "Down";
      this.downButton.Click += new System.EventHandler(this.downButton_Click);
      // 
      // autoTuneButton
      // 
      this.autoTuneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.autoTuneButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.autoTuneButton.Location = new System.Drawing.Point(256, 376);
      this.autoTuneButton.Name = "autoTuneButton";
      this.autoTuneButton.TabIndex = 4;
      this.autoTuneButton.Text = "Auto Tune";
      this.autoTuneButton.Click += new System.EventHandler(this.autoTuneButton_Click);
      // 
      // deleteButton
      // 
      this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.deleteButton.Enabled = false;
      this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.deleteButton.Location = new System.Drawing.Point(176, 376);
      this.deleteButton.Name = "deleteButton";
      this.deleteButton.TabIndex = 3;
      this.deleteButton.Text = "Delete";
      this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
      // 
      // editButton
      // 
      this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.editButton.Enabled = false;
      this.editButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.editButton.Location = new System.Drawing.Point(96, 376);
      this.editButton.Name = "editButton";
      this.editButton.TabIndex = 2;
      this.editButton.Text = "Edit";
      this.editButton.Click += new System.EventHandler(this.editButton_Click);
      // 
      // addButton
      // 
      this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.addButton.Location = new System.Drawing.Point(16, 376);
      this.addButton.Name = "addButton";
      this.addButton.TabIndex = 1;
      this.addButton.Text = "Add";
      this.addButton.Click += new System.EventHandler(this.addButton_Click);
      // 
      // stationsListView
      // 
      this.stationsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.stationsListView.CheckBoxes = true;
      this.stationsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                       this.columnHeader1,
                                                                                       this.columnHeader2,
                                                                                       this.columnHeader3,
                                                                                       this.columnHeader4,
                                                                                       this.columnHeader5,
                                                                                       this.columnHeader6});
      this.stationsListView.FullRowSelect = true;
      this.stationsListView.HideSelection = false;
      this.stationsListView.Location = new System.Drawing.Point(16, 24);
      this.stationsListView.Name = "stationsListView";
      this.stationsListView.Size = new System.Drawing.Size(424, 344);
      this.stationsListView.TabIndex = 0;
      this.stationsListView.View = System.Windows.Forms.View.Details;
      this.stationsListView.DoubleClick += new System.EventHandler(this.stationsListView_DoubleClick);
      this.stationsListView.SelectedIndexChanged += new System.EventHandler(this.stationsListView_SelectedIndexChanged);
      this.stationsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.stationsListView_ItemCheck);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Type";
      this.columnHeader1.Width = 65;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Station name";
      this.columnHeader2.Width = 117;
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Genre";
      this.columnHeader4.Width = 72;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Bitrate";
      this.columnHeader5.Width = 42;
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "Server";
      this.columnHeader6.Width = 70;
      // 
      // RadioStations
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "RadioStations";
      this.Size = new System.Drawing.Size(472, 432);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		private void addButton_Click(object sender, EventArgs e)
		{
			isDirty = true;

			EditRadioStationForm editStation = new EditRadioStationForm();

			DialogResult dialogResult = editStation.ShowDialog(this);

			if(dialogResult == DialogResult.OK)
			{
				ListViewItem listItem = new ListViewItem(new string[] { editStation.Station.Type, 
																		editStation.Station.Name,
																		editStation.Station.Frequency.ToString(Frequency.Format.MegaHerz),
																		editStation.Station.Genre, 
																		editStation.Station.Bitrate.ToString(),
																		editStation.Station.URL 
																	  } );

				listItem.Tag = editStation.Station;

				stationsListView.Items.Add(listItem);
			}
		}

		private void editButton_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			foreach(ListViewItem listItem in stationsListView.SelectedItems)
			{
				EditRadioStationForm editStation = new EditRadioStationForm();
				editStation.Station = listItem.Tag as RadioStation;

				DialogResult dialogResult = editStation.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					listItem.Tag = editStation.Station;

					//
					// Remove URL if we have a normal radio station
					//
					if(editStation.Station.Type.Equals("Radio"))
						editStation.Station.URL = String.Empty;

					listItem.SubItems[0].Text = editStation.Station.Type;
					listItem.SubItems[1].Text = editStation.Station.Name;
					listItem.SubItems[2].Text = editStation.Station.Frequency.ToString(Frequency.Format.MegaHerz);
					listItem.SubItems[3].Text = editStation.Station.Genre;
					listItem.SubItems[4].Text = editStation.Station.Bitrate.ToString();
					listItem.SubItems[5].Text = editStation.Station.URL;
				}
			}
		}

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			int itemCount = stationsListView.SelectedItems.Count;

			for(int index = 0; index < itemCount; index++)
			{
				stationsListView.Items.RemoveAt(stationsListView.SelectedIndices[0]);
			}
		}

		private void autoTuneButton_Click(object sender, System.EventArgs e)
		{
			if(stationsListView.Items.Count > 0)
			{
				DialogResult dialogResult = MessageBox.Show("Do you want to remove all current stations before you continue with the auto tuning?", "MediaPortal Settings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

				switch(dialogResult)
				{
					case DialogResult.Yes:
						//
						// Remove previously created stations
						//
						break;

					case DialogResult.No:
						//
						// Perform the auto tuning
						//
						AutoTuneStations();
						break;

					case DialogResult.Cancel:
						//
						// Don't do anything
						//
						break;
				}
			}
			else
			{
				//
				// We have no previous stations, perform auto tuning
				//
				AutoTuneStations();
			}
		}

		private void AutoTuneStations()
		{
			isDirty = true;

			Capture captureDevice = SetupCaptureDevice();

			if(captureDevice != null)
			{
				RadioAutoTuningForm radioTuning = new RadioAutoTuningForm(captureDevice);

				DialogResult dialogResult = radioTuning.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					ArrayList tunedItems = radioTuning.TunedItems;

					//
					// Add the tuned items to the list
					//
					foreach(RadioStation radioStation in tunedItems)
					{
						ListViewItem listItem = new ListViewItem(new string[] { radioStation.Type, 
																				  radioStation.Name,
																				  radioStation.Frequency.ToString(Frequency.Format.MegaHerz),
																				  radioStation.Genre,
																				  radioStation.Bitrate.ToString(),
																				  radioStation.URL
																			  } );
						listItem.Tag = radioStation;

						stationsListView.Items.Add(listItem);
					}
				}

				//
				// Dispose capture device
				//
				captureDevice.Dispose();
			}
			else
			{
				MessageBox.Show("No internal tuner was found, please check your tuner settings.", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);				
			}
		}

		private Capture SetupCaptureDevice()
		{
			Capture	captureDevice = null;

			//
			// Fetch devices, we're going to try to find the currently selected device
			//
			ArrayList availableVideoDevices = FilterHelper.GetVideoInputDevices();

			//
			// Fetch settings from the radio section
			//
			SectionSettings radioSection = SectionSettings.GetSection("Radio");
			string	selectedDevice = (string)radioSection.GetSetting("radio.device");
			string	selectedTuner = (string)radioSection.GetSetting("radio.tuner");

			int		selectedCountry = (int)radioSection.GetSetting("radio.country");

			//
			// Find the selected device
			//
      Filters filters = new Filters();
			foreach(Filter device in filters.VideoInputDevices)
			{
				if(device.Name.Equals(selectedDevice))
				{
					captureDevice = new Capture(device, null);
				}
			}

			if(captureDevice != null)
			{
        if (captureDevice.VideoSources==null) return null;
				//
				// Lookup available tuners
				//
				foreach(CrossbarSource source in captureDevice.VideoSources)
				{
					if(source.IsTuner)
					{
						captureDevice.VideoSource = source;
						break;
					}
				}

				captureDevice.FixCrossbarRouting(true);

				if(captureDevice.Tuner != null)
				{
					//
					// We have a valid device
					//
					captureDevice.Tuner.AudioMode = DirectX.Capture.Tuner.AMTunerModeType.FMRadio;
					captureDevice.Tuner.Mode = DShowNET.AMTunerModeType.FMRadio;

					if (selectedTuner.Equals("Antenna"))
						captureDevice.Tuner.InputType = DirectX.Capture.TunerInputType.Antenna;
					else
						captureDevice.Tuner.InputType = DirectX.Capture.TunerInputType.Cable;
	        
					captureDevice.Tuner.Country = selectedCountry;
					captureDevice.Tuner.TuningSpace = 66;
					captureDevice.AudioPreview = true;
				}
			}

			return captureDevice;
		}

		public override void LoadSettings()
		{
			LoadRadioStations();
		}

		public override void SaveSettings()
		{
			SaveRadioStations();
		}

		private void SaveRadioStations()
		{
			if(isDirty == true)
			{
        //
				// Start by removing the currently available stations from the database
				//
				RadioDatabase.RemoveStations();

				foreach(ListViewItem listItem in stationsListView.Items)
				{
					MediaPortal.Radio.Database.RadioStation station = new MediaPortal.Radio.Database.RadioStation();
					RadioStation radioStation = listItem.Tag as RadioStation;

					station.Name	= radioStation.Name;
					station.Genre	= radioStation.Genre;
					station.BitRate	= radioStation.Bitrate;
					station.URL		= radioStation.URL;

					//
					// Calculate the frequency for this station
					//
					if(radioStation.Frequency.Herz < 1000)
						radioStation.Frequency.Herz *= 1000000L;

          station.Frequency = radioStation.Frequency.Herz;
					station.Channel = listItem.Index;

					//
					// Save station
					//
					if(station.Frequency != 0 || station.URL.Length > 0)
					{
						RadioDatabase.AddStation(ref station);
					}

          //
          // Save default station
          //
          using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
          {
            if(listItem.Checked == true)
            {
              xmlwriter.SetValue("myradio", "default", station.Name);
            }
            else
            {
              xmlwriter.SetValue("myradio", "default", "");
            }
          }
				}
			}
		}

		private void LoadRadioStations()
		{
      string defaultStation = string.Empty;

      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        defaultStation = xmlreader.GetValueAsString("myradio", "default", "");
      }

			ArrayList stations = new ArrayList();
			RadioDatabase.GetStations(ref stations);

			foreach(MediaPortal.Radio.Database.RadioStation station in stations)
			{
				RadioStation radioStation = new RadioStation();

				radioStation.Type = station.URL.Length == 0 ? "Radio" : "Stream";
				radioStation.Name = station.Name;
				radioStation.Frequency = station.Frequency;
				radioStation.Genre = station.Genre;
				radioStation.Bitrate = station.BitRate;
				radioStation.URL = station.URL;

				ListViewItem listItem = new ListViewItem(new string[] { radioStation.Type, 
																		radioStation.Name,
																		radioStation.Frequency.ToString(Frequency.Format.MegaHerz),
																		radioStation.Genre,
																		radioStation.Bitrate.ToString(),
																		radioStation.URL
																	  } );

        //
        // Check default station
        //
        listItem.Checked = radioStation.Name.Equals(defaultStation);

				listItem.Tag = radioStation;

				stationsListView.Items.Add(listItem);
			}
		}

		private void stationsListView_DoubleClick(object sender, System.EventArgs e)
		{
			editButton_Click(sender, e);
		}

    private void MoveSelectionDown()
    {
      isDirty = true;

      for(int index = stationsListView.Items.Count - 1; index >= 0; index--)
      {
        if(stationsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't greater than the highest index in the list view
          //
          if(index < stationsListView.Items.Count - 1)
          {
            ListViewItem listItem = stationsListView.Items[index];
            stationsListView.Items.RemoveAt(index);

            if(index + 1 < stationsListView.Items.Count)
            {
              stationsListView.Items.Insert(index + 1, listItem);
            }
            else
            {
              stationsListView.Items.Add(listItem);
            }
          }
        }
      }
    }

    private void MoveSelectionUp()
    {
      isDirty = true;

      for(int index = 0; index < stationsListView.Items.Count; index++)
      {
        if(stationsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't smaller than the lowest index (0) in the list view
          //
          if(index > 0)
          {
            ListViewItem listItem = stationsListView.Items[index];
            stationsListView.Items.RemoveAt(index);
            stationsListView.Items.Insert(index - 1, listItem);
          }
        }
      }    
    }

    private void upButton_Click(object sender, System.EventArgs e)
    {
      MoveSelectionUp();
    }

    private void downButton_Click(object sender, System.EventArgs e)
    {
      MoveSelectionDown();
    }

    private void stationsListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      deleteButton.Enabled = editButton.Enabled = upButton.Enabled = downButton.Enabled = (stationsListView.SelectedItems.Count > 0);
    }

    private void stationsListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
    {
      isDirty = true;

      if(e.NewValue == CheckState.Checked)
      {
        //
        // Check if the new selected item is the same as the current one
        //
        if(stationsListView.Items[e.Index] != currentlyCheckedItem)
        {
          //
          // We have a new selection
          //
          if(currentlyCheckedItem != null)
            currentlyCheckedItem.Checked = false;
          currentlyCheckedItem = stationsListView.Items[e.Index];
        }
      }

      if(e.NewValue == CheckState.Unchecked)
      {
        //
        // Check if the new selected item is the same as the current one
        //
        if(stationsListView.Items[e.Index] == currentlyCheckedItem)
        {
          currentlyCheckedItem = null;
        }
      }        
    }
  }
}
