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
		private System.Windows.Forms.Button autoTuneButton;
		private MediaPortal.UserInterface.Controls.MPListView channelsListView;
		private System.ComponentModel.IContainer components = null;

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

			//
			// Hide the auto tune button
			//
			autoTuneButton.Visible = false;
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
			this.autoTuneButton = new System.Windows.Forms.Button();
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
			this.columnHeader3.Width = 134;
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
			this.groupBox1.Controls.Add(this.autoTuneButton);
			this.groupBox1.Controls.Add(this.deleteButton);
			this.groupBox1.Controls.Add(this.editButton);
			this.groupBox1.Controls.Add(this.addButton);
			this.groupBox1.Controls.Add(this.channelsListView);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(384, 352);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Settings";
			// 
			// autoTuneButton
			// 
			this.autoTuneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.autoTuneButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoTuneButton.Location = new System.Drawing.Point(295, 312);
			this.autoTuneButton.Name = "autoTuneButton";
			this.autoTuneButton.TabIndex = 4;
			this.autoTuneButton.Text = "Auto Tune";
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.deleteButton.Location = new System.Drawing.Point(176, 312);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.TabIndex = 3;
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// editButton
			// 
			this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.editButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.editButton.Location = new System.Drawing.Point(96, 312);
			this.editButton.Name = "editButton";
			this.editButton.TabIndex = 2;
			this.editButton.Text = "Edit";
			this.editButton.Click += new System.EventHandler(this.editButton_Click);
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addButton.Location = new System.Drawing.Point(16, 312);
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
			this.channelsListView.Size = new System.Drawing.Size(352, 280);
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
			this.Size = new System.Drawing.Size(400, 368);
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
						if(channel.Channel > highestChannelNumber)
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
						// Remove channel frquencies from the registry
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
								registryKey.SetValue(tvChannel.Channel.ToString(), (int)tvChannel.Frequency.Herz);
							}
						}

						registryKey.Close();
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void LoadTVChannels()
		{
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);

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
	}
}

