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
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.Button buttonAddGroup;
		private System.Windows.Forms.Button buttonDeleteGroup;
		private System.Windows.Forms.Button buttonEditGroup;
		private System.Windows.Forms.Button buttonGroupUp;
		private System.Windows.Forms.Button btnGroupDown;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ListView listViewGroups;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView listViewTVChannels;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ListView listViewTVGroupChannels;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.Button btnUnmap;
		private System.Windows.Forms.Button buttonMap;

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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.channelsListView = new MediaPortal.UserInterface.Controls.MPListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.btnImport = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.addButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.editButton = new System.Windows.Forms.Button();
			this.upButton = new System.Windows.Forms.Button();
			this.downButton = new System.Windows.Forms.Button();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.btnGroupDown = new System.Windows.Forms.Button();
			this.buttonGroupUp = new System.Windows.Forms.Button();
			this.buttonEditGroup = new System.Windows.Forms.Button();
			this.buttonDeleteGroup = new System.Windows.Forms.Button();
			this.buttonAddGroup = new System.Windows.Forms.Button();
			this.listViewGroups = new System.Windows.Forms.ListView();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.listViewTVChannels = new System.Windows.Forms.ListView();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.listViewTVGroupChannels = new System.Windows.Forms.ListView();
			this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
			this.btnUnmap = new System.Windows.Forms.Button();
			this.buttonMap = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
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
			this.groupBox1.Controls.Add(this.tabControl1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(464, 440);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Settings";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Location = new System.Drawing.Point(16, 16);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(440, 416);
			this.tabControl1.TabIndex = 8;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.channelsListView);
			this.tabPage1.Controls.Add(this.btnImport);
			this.tabPage1.Controls.Add(this.btnClear);
			this.tabPage1.Controls.Add(this.addButton);
			this.tabPage1.Controls.Add(this.deleteButton);
			this.tabPage1.Controls.Add(this.editButton);
			this.tabPage1.Controls.Add(this.upButton);
			this.tabPage1.Controls.Add(this.downButton);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(432, 390);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "TV Channels";
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
			this.channelsListView.Location = new System.Drawing.Point(8, 8);
			this.channelsListView.Name = "channelsListView";
			this.channelsListView.Size = new System.Drawing.Size(416, 320);
			this.channelsListView.TabIndex = 0;
			this.channelsListView.View = System.Windows.Forms.View.Details;
			this.channelsListView.DoubleClick += new System.EventHandler(this.channelsListView_DoubleClick);
			this.channelsListView.SelectedIndexChanged += new System.EventHandler(this.channelsListView_SelectedIndexChanged);
			this.channelsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.channelsListView_ItemCheck);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Channel name";
			this.columnHeader1.Width = 137;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Standard";
			this.columnHeader5.Width = 63;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Type";
			this.columnHeader4.Width = 61;
			// 
			// btnImport
			// 
			this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnImport.Location = new System.Drawing.Point(184, 344);
			this.btnImport.Name = "btnImport";
			this.btnImport.Size = new System.Drawing.Size(112, 23);
			this.btnImport.TabIndex = 6;
			this.btnImport.Text = "Import from tvguide";
			this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnClear.Location = new System.Drawing.Point(144, 344);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(32, 23);
			this.btnClear.TabIndex = 7;
			this.btnClear.Text = "Clear";
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addButton.Location = new System.Drawing.Point(16, 344);
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(32, 23);
			this.addButton.TabIndex = 1;
			this.addButton.Text = "Add";
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deleteButton.Enabled = false;
			this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.deleteButton.Location = new System.Drawing.Point(96, 344);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(40, 23);
			this.deleteButton.TabIndex = 3;
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// editButton
			// 
			this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.editButton.Enabled = false;
			this.editButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.editButton.Location = new System.Drawing.Point(56, 344);
			this.editButton.Name = "editButton";
			this.editButton.Size = new System.Drawing.Size(32, 23);
			this.editButton.TabIndex = 2;
			this.editButton.Text = "Edit";
			this.editButton.Click += new System.EventHandler(this.editButton_Click);
			// 
			// upButton
			// 
			this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.upButton.Enabled = false;
			this.upButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.upButton.Location = new System.Drawing.Point(344, 344);
			this.upButton.Name = "upButton";
			this.upButton.Size = new System.Drawing.Size(32, 23);
			this.upButton.TabIndex = 5;
			this.upButton.Text = "Up";
			this.upButton.Click += new System.EventHandler(this.upButton_Click);
			// 
			// downButton
			// 
			this.downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.downButton.Enabled = false;
			this.downButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.downButton.Location = new System.Drawing.Point(376, 344);
			this.downButton.Name = "downButton";
			this.downButton.Size = new System.Drawing.Size(40, 23);
			this.downButton.TabIndex = 4;
			this.downButton.Text = "Down";
			this.downButton.Click += new System.EventHandler(this.downButton_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.btnGroupDown);
			this.tabPage2.Controls.Add(this.buttonGroupUp);
			this.tabPage2.Controls.Add(this.buttonEditGroup);
			this.tabPage2.Controls.Add(this.buttonDeleteGroup);
			this.tabPage2.Controls.Add(this.buttonAddGroup);
			this.tabPage2.Controls.Add(this.listViewGroups);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(432, 390);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Groups";
			// 
			// btnGroupDown
			// 
			this.btnGroupDown.Location = new System.Drawing.Point(240, 344);
			this.btnGroupDown.Name = "btnGroupDown";
			this.btnGroupDown.Size = new System.Drawing.Size(48, 23);
			this.btnGroupDown.TabIndex = 5;
			this.btnGroupDown.Text = "Down";
			this.btnGroupDown.Click += new System.EventHandler(this.btnGroupDown_Click);
			// 
			// buttonGroupUp
			// 
			this.buttonGroupUp.Location = new System.Drawing.Point(200, 344);
			this.buttonGroupUp.Name = "buttonGroupUp";
			this.buttonGroupUp.Size = new System.Drawing.Size(32, 23);
			this.buttonGroupUp.TabIndex = 4;
			this.buttonGroupUp.Text = "Up";
			this.buttonGroupUp.Click += new System.EventHandler(this.buttonGroupUp_Click);
			// 
			// buttonEditGroup
			// 
			this.buttonEditGroup.Location = new System.Drawing.Point(112, 344);
			this.buttonEditGroup.Name = "buttonEditGroup";
			this.buttonEditGroup.Size = new System.Drawing.Size(40, 23);
			this.buttonEditGroup.TabIndex = 3;
			this.buttonEditGroup.Text = "Edit";
			this.buttonEditGroup.Click += new System.EventHandler(this.buttonEditGroup_Click);
			// 
			// buttonDeleteGroup
			// 
			this.buttonDeleteGroup.Location = new System.Drawing.Point(56, 344);
			this.buttonDeleteGroup.Name = "buttonDeleteGroup";
			this.buttonDeleteGroup.Size = new System.Drawing.Size(48, 23);
			this.buttonDeleteGroup.TabIndex = 2;
			this.buttonDeleteGroup.Text = "Delete";
			this.buttonDeleteGroup.Click += new System.EventHandler(this.buttonDeleteGroup_Click);
			// 
			// buttonAddGroup
			// 
			this.buttonAddGroup.Location = new System.Drawing.Point(8, 344);
			this.buttonAddGroup.Name = "buttonAddGroup";
			this.buttonAddGroup.Size = new System.Drawing.Size(40, 23);
			this.buttonAddGroup.TabIndex = 1;
			this.buttonAddGroup.Text = "Add";
			this.buttonAddGroup.Click += new System.EventHandler(this.buttonAddGroup_Click);
			// 
			// listViewGroups
			// 
			this.listViewGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																										 this.columnHeader6,
																																										 this.columnHeader7});
			this.listViewGroups.Location = new System.Drawing.Point(8, 8);
			this.listViewGroups.Name = "listViewGroups";
			this.listViewGroups.Size = new System.Drawing.Size(416, 304);
			this.listViewGroups.TabIndex = 0;
			this.listViewGroups.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Group name";
			this.columnHeader6.Width = 342;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Pincode";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.buttonMap);
			this.tabPage3.Controls.Add(this.btnUnmap);
			this.tabPage3.Controls.Add(this.listViewTVGroupChannels);
			this.tabPage3.Controls.Add(this.listViewTVChannels);
			this.tabPage3.Controls.Add(this.label3);
			this.tabPage3.Controls.Add(this.label2);
			this.tabPage3.Controls.Add(this.label1);
			this.tabPage3.Controls.Add(this.comboBox1);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(432, 390);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Map channels";
			// 
			// comboBox1
			// 
			this.comboBox1.Location = new System.Drawing.Point(40, 32);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(280, 21);
			this.comboBox1.TabIndex = 0;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Group:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "TVChannels";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(240, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(120, 16);
			this.label3.TabIndex = 3;
			this.label3.Text = "TV channels in group";
			// 
			// listViewTVChannels
			// 
			this.listViewTVChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																												 this.columnHeader8});
			this.listViewTVChannels.Location = new System.Drawing.Point(16, 112);
			this.listViewTVChannels.Name = "listViewTVChannels";
			this.listViewTVChannels.Size = new System.Drawing.Size(168, 240);
			this.listViewTVChannels.TabIndex = 4;
			this.listViewTVChannels.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "TV Channel";
			this.columnHeader8.Width = 159;
			// 
			// listViewTVGroupChannels
			// 
			this.listViewTVGroupChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																															this.columnHeader9});
			this.listViewTVGroupChannels.Location = new System.Drawing.Point(240, 112);
			this.listViewTVGroupChannels.Name = "listViewTVGroupChannels";
			this.listViewTVGroupChannels.Size = new System.Drawing.Size(168, 240);
			this.listViewTVGroupChannels.TabIndex = 5;
			this.listViewTVGroupChannels.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader9
			// 
			this.columnHeader9.Text = "TV Channel";
			this.columnHeader9.Width = 161;
			// 
			// btnUnmap
			// 
			this.btnUnmap.Location = new System.Drawing.Point(192, 224);
			this.btnUnmap.Name = "btnUnmap";
			this.btnUnmap.Size = new System.Drawing.Size(32, 23);
			this.btnUnmap.TabIndex = 6;
			this.btnUnmap.Text = "<<";
			this.btnUnmap.Click += new System.EventHandler(this.btnUnmap_Click);
			// 
			// buttonMap
			// 
			this.buttonMap.Location = new System.Drawing.Point(192, 184);
			this.buttonMap.Name = "buttonMap";
			this.buttonMap.Size = new System.Drawing.Size(32, 23);
			this.buttonMap.TabIndex = 7;
			this.buttonMap.Text = ">>";
			this.buttonMap.Click += new System.EventHandler(this.buttonMap_Click);
			// 
			// TVChannels
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "TVChannels";
			this.Size = new System.Drawing.Size(472, 448);
			this.groupBox1.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
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
			LoadGroups();
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
			SaveGroups();
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

		public void LoadGroups()
		{
			listViewGroups.Items.Clear();
			ArrayList groups = new ArrayList();
			TVDatabase.GetGroups(ref groups);
			foreach (TVGroup group in groups)
			{
				string pincode="No";
				if (group.Pincode!=0)
					pincode="Yes";
				ListViewItem listItem = new ListViewItem(new string[] { group.GroupName, pincode,} );
				listItem.Tag=group;
				listViewGroups.Items.Add(listItem);
				
			}
			UpdateGroupChannels(null,true);
		}

		private void buttonEditGroup_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			foreach(ListViewItem listItem in listViewGroups.SelectedItems)
			{
				EditGroupForm editgroup = new EditGroupForm();
				editgroup.Group = listItem.Tag as TVGroup;
				DialogResult dialogResult = editgroup.ShowDialog(this);
				if(dialogResult == DialogResult.OK)
				{
					TVGroup group = editgroup.Group;
					listItem.Tag = group;
					
					string pincode="No";
					if (group.Pincode!=0)
						pincode="Yes";

					listItem.SubItems[0].Text = group.GroupName;
					listItem.SubItems[1].Text = pincode;

					UpdateGroupChannels(group,true);
					SaveGroups();
				}
			}				
		}

		private void buttonDeleteGroup_Click(object sender, System.EventArgs e)
		{

			int itemCount = listViewGroups.SelectedItems.Count;

			for(int index = 0; index < itemCount; index++)
			{
				isDirty = true;
				ListViewItem item=listViewGroups.SelectedItems[0];
				TVGroup group=item.Tag as TVGroup;
				if(group!=null) TVDatabase.DeleteGroup(group);
				listViewGroups.Items.RemoveAt(listViewGroups.SelectedIndices[0]);
			}		

			UpdateGroupChannels(null,true);
			SaveGroups();
		}

		private void buttonAddGroup_Click(object sender, System.EventArgs e)
		{

			EditGroupForm editGroup = new EditGroupForm();
			DialogResult dialogResult = editGroup.ShowDialog(this);
			if(dialogResult == DialogResult.OK)
			{
				isDirty = true;
				TVGroup group = editGroup.Group;
				string pincode="No";
				if (group.Pincode!=0)
					pincode="Yes";
				ListViewItem listItem = new ListViewItem(new string[] { group.GroupName, pincode,} );
				listItem.Tag=group;
				listViewGroups.Items.Add(listItem);
				
				UpdateGroupChannels(group,true);

			}		

		}

		private void buttonGroupUp_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			for(int index = 0; index < listViewGroups.Items.Count; index++)
			{
				if(listViewGroups.Items[index].Selected == true)
				{
					//
					// Make sure the current index isn't smaller than the lowest index (0) in the list view
					//
					if(index > 0)
					{
						ListViewItem listItem = listViewGroups.Items[index];
						listViewGroups.Items.RemoveAt(index);
						listViewGroups.Items.Insert(index - 1, listItem);
					}
				}
			}    
			SaveGroups();
		}

		private void btnGroupDown_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			for(int index = listViewGroups.Items.Count - 1; index >= 0; index--)
			{
				if(listViewGroups.Items[index].Selected == true)
				{
					//
					// Make sure the current index isn't greater than the highest index in the list view
					//
					if(index < listViewGroups.Items.Count - 1)
					{
						ListViewItem listItem = listViewGroups.Items[index];
						listViewGroups.Items.RemoveAt(index);

						if(index + 1 < listViewGroups.Items.Count)
						{
							listViewGroups.Items.Insert(index + 1, listItem);
						}
						else
						{
							listViewGroups.Items.Add(listItem);
						}
					}
				}
			}
			SaveGroups();
		}

		private void SaveGroups()
		{
			if(isDirty == true)
			{
				for(int index = 0; index < listViewGroups.Items.Count ; index++)
				{
					ListViewItem listItem = listViewGroups.Items[index];
					TVGroup group = listItem.Tag as TVGroup;
					if (group!=null)
					{
						group.Sort=index;
						TVDatabase.AddGroup(group);
					}
				}
			}
		}

		private void buttonMap_Click(object sender, System.EventArgs e)
		{
			if (listViewTVChannels.SelectedItems==null) return;
			for(int i=0; i < listViewTVChannels.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewTVChannels.SelectedItems[i];
				TVChannel chan=(TVChannel)listItem.Tag;
				
				listItem = new ListViewItem(new string[] { chan.Name} );
				listItem.Tag=chan;
				listViewTVGroupChannels.Items.Add(listItem);
			}
			
			TVGroup group = comboBox1.SelectedItem as TVGroup;
			for(int i=listViewTVChannels.SelectedItems.Count-1; i >=0 ;i--)
			{
				ListViewItem listItem=listViewTVChannels.SelectedItems[i];
				TVChannel channel=listItem.Tag as TVChannel;
				if (group!=null && channel != null)
					TVDatabase.MapChannelToGroup(group, channel);

				listViewTVChannels.Items.Remove(listItem);
			}
		}

		private void btnUnmap_Click(object sender, System.EventArgs e)
		{
			if (listViewTVGroupChannels.SelectedItems==null) return;
			for(int i=0; i < listViewTVGroupChannels.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewTVGroupChannels.SelectedItems[i];
				TVChannel chan=(TVChannel)listItem.Tag;

				listItem = new ListViewItem(new string[] { chan.Name} );
				listItem.Tag=chan;
				listViewTVChannels.Items.Add(listItem);
			}		
			TVGroup group = comboBox1.SelectedItem as TVGroup;
			for(int i=listViewTVGroupChannels.SelectedItems.Count-1; i>=0;--i)
			{
				ListViewItem listItem=listViewTVGroupChannels.SelectedItems[i];
				TVChannel channel=listItem.Tag as TVChannel;
				if (group!=null && channel != null)
					TVDatabase.UnmapChannelFromGroup(group, channel);
				listViewTVGroupChannels.Items.Remove(listItem);
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TVGroup group = (TVGroup) comboBox1.SelectedItem;
			UpdateGroupChannels(group,false);		
		}

		void UpdateGroupChannels(TVGroup group, bool reloadgroups)
		{
			if (reloadgroups || comboBox1.Items.Count==0)
			{
				comboBox1.Items.Clear();
				ArrayList groups = new ArrayList();
				TVDatabase.GetGroups(ref groups);
				foreach (TVGroup grp in groups)
				{
					comboBox1.Items.Add(grp);
				}
				if (comboBox1.Items.Count>0)
				{
					comboBox1.SelectedIndex=0;
					group=comboBox1.SelectedItem as TVGroup;
				}
			}

			listViewTVGroupChannels.Items.Clear();
			if (group!=null)
			{
				TVDatabase.GetTVChannelsForGroup(group);
				foreach (TVChannel chan in group.tvChannels)
				{
					ListViewItem listItem = new ListViewItem(new string[] { chan.Name} );
					listItem.Tag=chan;
					listViewTVGroupChannels.Items.Add(listItem);
				}
			}

			listViewTVChannels.Items.Clear();
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			foreach (TVChannel chan in channels)
			{
				ListViewItem listItem = new ListViewItem(new string[] { chan.Name} );
				listItem.Tag=chan;
				listViewTVChannels.Items.Add(listItem);
			}
		}
	}
}

