using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.Win32;

using SQLite.NET;
using MediaPortal.TV.Database;
using DShowNET;

namespace MediaPortal.Configuration.Sections
{
	public class TVChannels : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button editButton;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private MediaPortal.UserInterface.Controls.MPListView channelsListView;
		private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Button downButton;
    private System.Windows.Forms.Button upButton;
    private System.Windows.Forms.ColumnHeader columnHeader4;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.Button btnImport;
    private System.Windows.Forms.Button btnClear;
		static bool reloadList=false;

		//
		// Private members
		//
		bool isDirty = false;

		public TVChannels() : this("Channels")
		{
		}

		public TVChannels(string name) : base(name)
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
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.btnClear = new System.Windows.Forms.Button();
			this.btnImport = new System.Windows.Forms.Button();
			this.upButton = new System.Windows.Forms.Button();
			this.downButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.editButton = new System.Windows.Forms.Button();
			this.addButton = new System.Windows.Forms.Button();
			this.channelsListView = new MediaPortal.UserInterface.Controls.MPListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Frequency (MHz)";
			this.columnHeader3.Width = 94;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Channel";
			this.columnHeader2.Width = 57;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.btnClear);
			this.groupBox1.Controls.Add(this.btnImport);
			this.groupBox1.Controls.Add(this.upButton);
			this.groupBox1.Controls.Add(this.downButton);
			this.groupBox1.Controls.Add(this.deleteButton);
			this.groupBox1.Controls.Add(this.editButton);
			this.groupBox1.Controls.Add(this.addButton);
			this.groupBox1.Controls.Add(this.channelsListView);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(464, 440);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Settings";
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnClear.Location = new System.Drawing.Point(136, 360);
			this.btnClear.Name = "btnClear";
			this.btnClear.TabIndex = 7;
			this.btnClear.Text = "Clear";
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// btnImport
			// 
			this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnImport.Location = new System.Drawing.Point(16, 360);
			this.btnImport.Name = "btnImport";
			this.btnImport.Size = new System.Drawing.Size(112, 23);
			this.btnImport.TabIndex = 6;
			this.btnImport.Text = "Import from tvguide";
			this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
			// 
			// upButton
			// 
			this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.upButton.Enabled = false;
			this.upButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.upButton.Location = new System.Drawing.Point(340, 392);
			this.upButton.Name = "upButton";
			this.upButton.Size = new System.Drawing.Size(48, 23);
			this.upButton.TabIndex = 5;
			this.upButton.Text = "Up";
			this.upButton.Click += new System.EventHandler(this.upButton_Click);
			// 
			// downButton
			// 
			this.downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.downButton.Enabled = false;
			this.downButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.downButton.Location = new System.Drawing.Point(392, 392);
			this.downButton.Name = "downButton";
			this.downButton.Size = new System.Drawing.Size(48, 23);
			this.downButton.TabIndex = 4;
			this.downButton.Text = "Down";
			this.downButton.Click += new System.EventHandler(this.downButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deleteButton.Enabled = false;
			this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.deleteButton.Location = new System.Drawing.Point(176, 392);
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
			this.editButton.Location = new System.Drawing.Point(96, 392);
			this.editButton.Name = "editButton";
			this.editButton.TabIndex = 2;
			this.editButton.Text = "Edit";
			this.editButton.Click += new System.EventHandler(this.editButton_Click);
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addButton.Location = new System.Drawing.Point(16, 392);
			this.addButton.Name = "addButton";
			this.addButton.TabIndex = 1;
			this.addButton.Text = "Add";
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// channelsListView
			// 
			this.channelsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.channelsListView.CheckBoxes = true;
			this.channelsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																											 this.columnHeader1,
																																											 this.columnHeader2,
																																											 this.columnHeader3,
																																											 this.columnHeader5,
																																											 this.columnHeader4});
			this.channelsListView.FullRowSelect = true;
			this.channelsListView.HideSelection = false;
			this.channelsListView.Location = new System.Drawing.Point(16, 24);
			this.channelsListView.Name = "channelsListView";
			this.channelsListView.Size = new System.Drawing.Size(424, 320);
			this.channelsListView.TabIndex = 0;
			this.channelsListView.View = System.Windows.Forms.View.Details;
			this.channelsListView.DoubleClick += new System.EventHandler(this.channelsListView_DoubleClick);
			this.channelsListView.SelectedIndexChanged += new System.EventHandler(this.channelsListView_SelectedIndexChanged);
			this.channelsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.channelsListView_ItemCheck);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Channel name";
			this.columnHeader1.Width = 146;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Standard";
			this.columnHeader5.Width = 63;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Type";
			// 
			// TVChannels
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "TVChannels";
			this.Size = new System.Drawing.Size(472, 448);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		private void addButton_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			EditTVChannelForm editChannel = new EditTVChannelForm();

			DialogResult dialogResult = editChannel.ShowDialog(this);

			if(dialogResult == DialogResult.OK)
			{
				TelevisionChannel editedChannel = editChannel.Channel;

				ListViewItem listItem = new ListViewItem(new string[] { editedChannel.Name, 
																		editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString(),
																		editedChannel.Frequency.ToString(Frequency.Format.MegaHerz),
                                    GetStandardName(editedChannel.standard),
                                    editedChannel.External ? "External" : "Internal"
																	  } );
				listItem.Tag = editedChannel;

				channelsListView.Items.Add(listItem);
			}		
		}

    private string GetStandardName(AnalogVideoStandard standard)
    {
      string name = standard.ToString();
      name = name.Replace("_", " ");
      return name == "None" ? "Default" : name;
    }

		private void editButton_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			foreach(ListViewItem listItem in channelsListView.SelectedItems)
			{
				EditTVChannelForm editChannel = new EditTVChannelForm();
				editChannel.Channel = listItem.Tag as TelevisionChannel;

				DialogResult dialogResult = editChannel.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					TelevisionChannel editedChannel = editChannel.Channel;
					listItem.Tag = editedChannel;

					listItem.SubItems[0].Text = editedChannel.Name;
					listItem.SubItems[1].Text = editedChannel.External ? String.Format("{0}/{1}", editedChannel.Channel, editedChannel.ExternalTunerChannel) : editedChannel.Channel.ToString();
					listItem.SubItems[2].Text = editedChannel.Frequency.ToString(Frequency.Format.MegaHerz);
          listItem.SubItems[3].Text = GetStandardName(editedChannel.standard);
          listItem.SubItems[4].Text = editedChannel.External ? "External" : "Internal";
				}
			}		
		}

		private void deleteButton_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			int itemCount = channelsListView.SelectedItems.Count;

			for(int index = 0; index < itemCount; index++)
			{
				channelsListView.Items.RemoveAt(channelsListView.SelectedIndices[0]);
			}
		}

		public override object GetSetting(string name)
		{
			switch(name)
			{
				case "channel.highest":
					return HighestChannelNumber;
			}

			return null;
		}


		private int HighestChannelNumber
		{
			get 
			{
				int highestChannelNumber = 0;

				foreach(ListViewItem item in channelsListView.Items)
				{
					TelevisionChannel channel = item.Tag as TelevisionChannel;

					if(channel != null)
					{
						if(channel.Channel < (int)ExternalInputs.svhs && channel.Channel > highestChannelNumber)
							highestChannelNumber = channel.Channel;
					}
				}

				return highestChannelNumber;
			}
		}

		public override void LoadSettings()
		{
			LoadTVChannels();
		}

		public override void SaveSettings()
		{
			if (reloadList)
			{
				LoadTVChannels();
				reloadList=false;
				isDirty=true;
			}
			SaveTVChannels();
		}

		private void SaveTVChannels()
		{
			if(isDirty == true)
			{
				SectionSettings section = SectionSettings.GetSection("Television");

				if(section != null)
				{
					int countryCode = (int)section.GetSetting("television.country");

					RegistryKey registryKey = Registry.LocalMachine;

					string[] registryLocations = new string[] { String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-1", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS{0}-0", countryCode),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-1"),
																  String.Format(@"Software\Microsoft\TV System Services\TVAutoTune\TS0-0")
															  };

					//
					// Start by removing any old tv channels from the database and from the registry.
					// Information stored in the registry is the channel frequency.
					//
					ArrayList channels = new ArrayList();
					TVDatabase.GetChannels(ref channels);

					if(channels != null && channels.Count > 0)
					{
						foreach(MediaPortal.TV.Database.TVChannel channel in channels)
						{
							bool found=false;
							foreach(ListViewItem listItem in channelsListView.Items)
							{
								TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;
								if (channel.Name.ToLower() == tvChannel.Name.ToLower())
								{
									found=true;
									break;
								}
							}
							if (!found)
								TVDatabase.RemoveChannel(channel.Name);
						}

						//
						// Remove channel frequencies from the registry
						//
						for(int index = 0; index < registryLocations.Length; index++)
						{
							registryKey = Registry.LocalMachine;
							registryKey = registryKey.CreateSubKey(registryLocations[index]);

							for(int channelIndex = 0; channelIndex < 200; channelIndex++)
							{
								registryKey.DeleteValue(channelIndex.ToString(), false);
							}

							registryKey.Close();
						}
					}

					//
					// Add current channels
					//
					TVDatabase.GetChannels(ref channels);
					foreach(ListViewItem listItem in channelsListView.Items)
					{
						MediaPortal.TV.Database.TVChannel channel = new MediaPortal.TV.Database.TVChannel();
						TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;

						if(tvChannel != null)
						{
							channel.Name = tvChannel.Name;
							channel.Number = tvChannel.Channel;
              channel.VisibleInGuide = tvChannel.VisibleInGuide;
							channel.Country=tvChannel.Country;
							
							//
							// Calculate frequency
							//
							if(tvChannel.Frequency.Herz < 1000)
								tvChannel.Frequency.Herz *= 1000000L;

							channel.Frequency = tvChannel.Frequency.Herz;
              
              channel.External = tvChannel.External;
              channel.ExternalTunerChannel = tvChannel.ExternalTunerChannel;
              channel.TVStandard = tvChannel.standard;

							//does channel already exists in database?
							bool exists=false;
							foreach (TVChannel chan in channels)
							{
								if (chan.Name.ToLower() == channel.Name.ToLower())
								{
									exists=true;
									break;
								}
							}
							
							if (exists)
							{
								TVDatabase.UpdateChannel(channel, listItem.Index);
							}
							else
							{
								TVDatabase.AddChannel(channel);

								//
								// Set the sort order
								//
								TVDatabase.SetChannelSort(channel.Name, listItem.Index);
							}
						}
					}

					//
					// Add frequencies to the registry
					//
					for(int index = 0; index < registryLocations.Length; index++)
					{
						registryKey = Registry.LocalMachine;
						registryKey = registryKey.CreateSubKey(registryLocations[index]);

						foreach(ListViewItem listItem in channelsListView.Items)
						{
							TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;

							if(tvChannel != null)
							{
                //
                // Don't add frequency to the registry if it has no frequency or if we have the predefined
                // channels for Composite and SVIDEO
                //
                if(tvChannel.Frequency.Herz > 0 && 
                  tvChannel.Channel != (int)ExternalInputs.svhs && 
                  tvChannel.Channel != (int)ExternalInputs.cvbs1 &&
                  tvChannel.Channel != (int)ExternalInputs.cvbs2)
                {
                  registryKey.SetValue(tvChannel.Channel.ToString(), (int)tvChannel.Frequency.Herz);
                }
							}
						}

						registryKey.Close();
					}
				}
			}
		}

    private void AddChannel(ref ArrayList channels, string strName, int iNumber)
    {
      isDirty = true;

      TVChannel channel = new TVChannel();
      channel.Number=iNumber;
      channel.Name  =strName;
      channels.Add(channel);
    }

		/// <summary>
		/// 
		/// </summary>
		private void LoadTVChannels()
		{
      channelsListView.Items.Clear();
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);

			foreach(TVChannel channel in channels)
			{
				TelevisionChannel tvChannel = new TelevisionChannel();

				tvChannel.Channel	= channel.Number;
				tvChannel.Name		= channel.Name;
				tvChannel.Frequency	= channel.Frequency;
        tvChannel.External = channel.External;
        tvChannel.ExternalTunerChannel = channel.ExternalTunerChannel;
        tvChannel.VisibleInGuide = channel.VisibleInGuide;
				tvChannel.Country=channel.Country;
        tvChannel.standard = channel.TVStandard;
				ListViewItem listItem = new ListViewItem(new string[] { tvChannel.Name, 
																		tvChannel.External ? String.Format("{0}/{1}", tvChannel.Channel, tvChannel.ExternalTunerChannel) : tvChannel.Channel.ToString(),
																		tvChannel.Frequency.ToString(Frequency.Format.MegaHerz),
                                    GetStandardName(tvChannel.standard),
                                    tvChannel.External ? "External" : "Internal"
																	  } );

        listItem.Checked = tvChannel.VisibleInGuide;

				listItem.Tag = tvChannel;

				channelsListView.Items.Add(listItem);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void channelsListView_DoubleClick(object sender, System.EventArgs e)
		{
			editButton_Click(sender, e);		
		}

    private void MoveSelectionDown()
    {
      isDirty = true;

      for(int index = channelsListView.Items.Count - 1; index >= 0; index--)
      {
        if(channelsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't greater than the highest index in the list view
          //
          if(index < channelsListView.Items.Count - 1)
          {
            ListViewItem listItem = channelsListView.Items[index];
            channelsListView.Items.RemoveAt(index);

            if(index + 1 < channelsListView.Items.Count)
            {
              channelsListView.Items.Insert(index + 1, listItem);
            }
            else
            {
              channelsListView.Items.Add(listItem);
            }
          }
        }
      }
    }

    private void MoveSelectionUp()
    {
      isDirty = true;

      for(int index = 0; index < channelsListView.Items.Count; index++)
      {
        if(channelsListView.Items[index].Selected == true)
        {
          //
          // Make sure the current index isn't smaller than the lowest index (0) in the list view
          //
          if(index > 0)
          {
            ListViewItem listItem = channelsListView.Items[index];
            channelsListView.Items.RemoveAt(index);
            channelsListView.Items.Insert(index - 1, listItem);
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

    private void channelsListView_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      deleteButton.Enabled = editButton.Enabled = upButton.Enabled = downButton.Enabled = (channelsListView.SelectedItems.Count > 0);
    }

    private void channelsListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
    {
      isDirty = true;

      //
      // Fetch checked item
      //
      if(e.Index < channelsListView.Items.Count)
      {
        TelevisionChannel tvChannel = channelsListView.Items[e.Index].Tag as TelevisionChannel;

        tvChannel.VisibleInGuide = (e.NewValue == System.Windows.Forms.CheckState.Checked);

        channelsListView.Items[e.Index].Tag = tvChannel;
      }
    }

    private void btnImport_Click(object sender, System.EventArgs e)
    {
      using(AMS.Profile.Xml  xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strTVGuideFile=xmlreader.GetValueAsString("xmltv","folder","xmltv");
        strTVGuideFile=RemoveTrailingSlash(strTVGuideFile);
        strTVGuideFile+=@"\tvguide.xml";
        XMLTVImport import = new XMLTVImport();
        bool bSucceeded=import.Import(strTVGuideFile,true);
        if (bSucceeded)
        {
          string strtext=String.Format("Imported:{0} channels\r{1} programs\r{2}", 
                                import.ImportStats.Channels,
                                import.ImportStats.Programs,
                                import.ImportStats.Status);
          MessageBox.Show(this,strtext,"tvguide",MessageBoxButtons.OK,MessageBoxIcon.Error);

          isDirty =true;
          LoadTVChannels();
        }
        else
        {
          string strError=String.Format("Error importing tvguide from:\r{0}\rerror:{1}",
                              strTVGuideFile,import.ImportStats.Status);
          MessageBox.Show(this,strError,"Error importing tvguide",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }
      }
    }
    string RemoveTrailingSlash(string strLine)
    {
      string strPath=strLine;
      while (strPath.Length>0)
      {
        if ( strPath[strPath.Length-1]=='\\' || strPath[strPath.Length-1]=='/')
        {
          strPath=strPath.Substring(0,strPath.Length-1);
        }
        else break;
      }
      return strPath;
    }

    private void btnClear_Click(object sender, System.EventArgs e)
    {
      DialogResult result=MessageBox.Show(this,"Are you sure you want to delete all channels?","Delete channels",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
      if (result!=DialogResult.Yes) return;
      channelsListView.Items.Clear();
      isDirty =true;
      SaveTVChannels();
      
    }

    private void buttonAutoTune_Click(object sender, System.EventArgs e)
    {
      isDirty =true;
      SaveTVChannels();
      AnalogTVTuningForm form = new AnalogTVTuningForm();
      form.ShowDialog(this);


      isDirty =true;
      LoadTVChannels();
    
    }
		static public void UpdateList()
		{
			reloadList=false;
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (reloadList)
			{
				reloadList=false;
				LoadTVChannels();
			}
			base.OnPaint (e);
		}

	}
}

