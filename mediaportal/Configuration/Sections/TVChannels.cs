using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.Win32;

using SQLite.NET;
using MediaPortal.TV.Database;

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
      this.upButton = new System.Windows.Forms.Button();
      this.downButton = new System.Windows.Forms.Button();
      this.deleteButton = new System.Windows.Forms.Button();
      this.editButton = new System.Windows.Forms.Button();
      this.addButton = new System.Windows.Forms.Button();
      this.channelsListView = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Frequency override (MHz)";
      this.columnHeader3.Width = 198;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Channel";
      this.columnHeader2.Width = 76;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.upButton);
      this.groupBox1.Controls.Add(this.downButton);
      this.groupBox1.Controls.Add(this.deleteButton);
      this.groupBox1.Controls.Add(this.editButton);
      this.groupBox1.Controls.Add(this.addButton);
      this.groupBox1.Controls.Add(this.channelsListView);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(456, 432);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Settings";
      // 
      // upButton
      // 
      this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
      this.channelsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                       this.columnHeader1,
                                                                                       this.columnHeader2,
                                                                                       this.columnHeader3});
      this.channelsListView.FullRowSelect = true;
      this.channelsListView.HideSelection = false;
      this.channelsListView.Location = new System.Drawing.Point(16, 24);
      this.channelsListView.Name = "channelsListView";
      this.channelsListView.Size = new System.Drawing.Size(424, 360);
      this.channelsListView.TabIndex = 0;
      this.channelsListView.View = System.Windows.Forms.View.Details;
      this.channelsListView.DoubleClick += new System.EventHandler(this.channelsListView_DoubleClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Channel name";
      this.columnHeader1.Width = 146;
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
																		editedChannel.Channel.ToString(),
																		editedChannel.Frequency.ToString(Frequency.Format.MegaHerz) 
																	  } );
				listItem.Tag = editedChannel;

				channelsListView.Items.Add(listItem);
			}		
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
					listItem.SubItems[1].Text = editedChannel.Channel.ToString();
					listItem.SubItems[2].Text = editedChannel.Frequency.ToString(Frequency.Format.MegaHerz);
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
						if(channel.Channel < 1000 && channel.Channel > highestChannelNumber)
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
					// Start by removing existing channels from the database and from the registry.
					// Information stored in the registry is the channel frequency.
					//
					ArrayList channels = new ArrayList();
					TVDatabase.GetChannels(ref channels);

					if(channels != null && channels.Count > 0)
					{
						foreach(MediaPortal.TV.Database.TVChannel channel in channels)
						{
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
					foreach(ListViewItem listItem in channelsListView.Items)
					{
						MediaPortal.TV.Database.TVChannel channel = new MediaPortal.TV.Database.TVChannel();
						TelevisionChannel tvChannel = listItem.Tag as TelevisionChannel;

						if(tvChannel != null)
						{
							channel.Name = tvChannel.Name;
							channel.Number = tvChannel.Channel;
							
							//
							// Calculate frequency
							//
							if(tvChannel.Frequency.Herz < 1000)
								tvChannel.Frequency.Herz *= 1000000L;

							channel.Frequency = tvChannel.Frequency.Herz;

							//
							// Finally add the channel
							//
							TVDatabase.AddChannel(channel);

              //
              // Set the sort order
              //
              TVDatabase.SetChannelSort(channel.Name, listItem.Index);
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
                  tvChannel.Channel != 1000 && 
                  tvChannel.Channel != 1001 &&
                  tvChannel.Channel != 1002)
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
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
      bool bCVBS1=false;
      bool bCVBS2=false;
      bool bSVHS=false;
      foreach(TVChannel channel in channels)
      {
        if (channel.Number==1000) bSVHS=true;
        if (channel.Number==1001) bCVBS1=true;
        if (channel.Number==1002) bCVBS2=true;
      }
      if (!bSVHS)
        AddChannel(ref channels,"SVHS",1000);
      if (!bCVBS1)
        AddChannel(ref channels,"Composite #1",1001);
      if (!bCVBS2)
        AddChannel(ref channels,"Composite #2",1002);


			foreach(TVChannel channel in channels)
			{
				TelevisionChannel tvChannel = new TelevisionChannel();

				tvChannel.Channel	= channel.Number;
				tvChannel.Name		= channel.Name;
				tvChannel.Frequency	= channel.Frequency;

				ListViewItem listItem = new ListViewItem(new string[] { tvChannel.Name, 
																		tvChannel.Channel.ToString(),
																		tvChannel.Frequency.ToString(Frequency.Format.MegaHerz) 
																	  } );

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
	}
}

