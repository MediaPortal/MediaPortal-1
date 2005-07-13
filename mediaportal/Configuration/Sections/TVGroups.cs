using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml;
using MWCommon;
using MWControls;
using Microsoft.Win32;
using MediaPortal.Configuration.Controls;

using SQLite.NET;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using DShowNET;

namespace MediaPortal.Configuration.Sections
{
	public class TVGroups : MediaPortal.Configuration.SectionSettings
	{
		public class ComboCard
		{
			public string FriendlyName;
			public string VideoDevice;
			public int    ID;
			public override string ToString()
			{
				return String.Format("{0} - {1}", FriendlyName, VideoDevice);
			}
		};
		private System.ComponentModel.IContainer components = null;
		static bool reloadList=false;
		private System.Windows.Forms.OpenFileDialog XMLOpenDialog;
		private System.Windows.Forms.SaveFileDialog XMLSaveDialog;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Button btnGroupDown;
		private System.Windows.Forms.Button buttonGroupUp;
		private System.Windows.Forms.Button buttonEditGroup;
		private System.Windows.Forms.Button buttonDeleteGroup;
		private System.Windows.Forms.Button buttonAddGroup;
		private System.Windows.Forms.ListView listViewGroups;
		private System.Windows.Forms.TabPage tabPage3;
		private MWControls.MWTreeView treeViewChannels;
		private System.Windows.Forms.Button btnGrpChnDown;
		private System.Windows.Forms.Button btnGrpChnUp;
		private System.Windows.Forms.Button buttonMap;
		private System.Windows.Forms.Button btnUnmap;
		private System.Windows.Forms.ListView listViewTVGroupChannels;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ColumnHeader columnHeader9;

		//
		// Private members
		//
		bool isDirty = false;

		public TVGroups() : this("TV Channel Groups")
		{
		}

		public TVGroups(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
			treeViewChannels.MultiSelect=TreeViewMultiSelect.MultiSameBranchAndLevel;
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TVGroups));
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.XMLOpenDialog = new System.Windows.Forms.OpenFileDialog();
			this.XMLSaveDialog = new System.Windows.Forms.SaveFileDialog();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.btnGroupDown = new System.Windows.Forms.Button();
			this.buttonGroupUp = new System.Windows.Forms.Button();
			this.buttonEditGroup = new System.Windows.Forms.Button();
			this.buttonDeleteGroup = new System.Windows.Forms.Button();
			this.buttonAddGroup = new System.Windows.Forms.Button();
			this.listViewGroups = new System.Windows.Forms.ListView();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.treeViewChannels = new MWControls.MWTreeView();
			this.btnGrpChnDown = new System.Windows.Forms.Button();
			this.btnGrpChnUp = new System.Windows.Forms.Button();
			this.buttonMap = new System.Windows.Forms.Button();
			this.btnUnmap = new System.Windows.Forms.Button();
			this.listViewTVGroupChannels = new System.Windows.Forms.ListView();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
			this.tabControl1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// XMLOpenDialog
			// 
			this.XMLOpenDialog.DefaultExt = "xml";
			this.XMLOpenDialog.FileName = "ChannelList";
			this.XMLOpenDialog.Filter = "xml|*.xml";
			this.XMLOpenDialog.InitialDirectory = ".";
			this.XMLOpenDialog.Title = "Open....";
			// 
			// XMLSaveDialog
			// 
			this.XMLSaveDialog.CreatePrompt = true;
			this.XMLSaveDialog.DefaultExt = "xml";
			this.XMLSaveDialog.FileName = "ChannelList";
			this.XMLSaveDialog.Filter = "xml|*.xml";
			this.XMLSaveDialog.InitialDirectory = ".";
			this.XMLSaveDialog.Title = "Save to....";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Location = new System.Drawing.Point(8, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(440, 416);
			this.tabControl1.TabIndex = 9;
			// 
			// tabPage2
			// 
			this.tabPage2.AutoScroll = true;
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
			// 
			// buttonGroupUp
			// 
			this.buttonGroupUp.Location = new System.Drawing.Point(200, 344);
			this.buttonGroupUp.Name = "buttonGroupUp";
			this.buttonGroupUp.Size = new System.Drawing.Size(32, 23);
			this.buttonGroupUp.TabIndex = 4;
			this.buttonGroupUp.Text = "Up";
			// 
			// buttonEditGroup
			// 
			this.buttonEditGroup.Location = new System.Drawing.Point(112, 344);
			this.buttonEditGroup.Name = "buttonEditGroup";
			this.buttonEditGroup.Size = new System.Drawing.Size(40, 23);
			this.buttonEditGroup.TabIndex = 3;
			this.buttonEditGroup.Text = "Edit";
			// 
			// buttonDeleteGroup
			// 
			this.buttonDeleteGroup.Location = new System.Drawing.Point(56, 344);
			this.buttonDeleteGroup.Name = "buttonDeleteGroup";
			this.buttonDeleteGroup.Size = new System.Drawing.Size(48, 23);
			this.buttonDeleteGroup.TabIndex = 2;
			this.buttonDeleteGroup.Text = "Delete";
			// 
			// buttonAddGroup
			// 
			this.buttonAddGroup.Location = new System.Drawing.Point(8, 344);
			this.buttonAddGroup.Name = "buttonAddGroup";
			this.buttonAddGroup.Size = new System.Drawing.Size(40, 23);
			this.buttonAddGroup.TabIndex = 1;
			this.buttonAddGroup.Text = "Add";
			// 
			// listViewGroups
			// 
			this.listViewGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																										 this.columnHeader6,
																																										 this.columnHeader7});
			this.listViewGroups.FullRowSelect = true;
			this.listViewGroups.HideSelection = false;
			this.listViewGroups.Location = new System.Drawing.Point(8, 8);
			this.listViewGroups.Name = "listViewGroups";
			this.listViewGroups.Size = new System.Drawing.Size(416, 304);
			this.listViewGroups.TabIndex = 0;
			this.listViewGroups.View = System.Windows.Forms.View.Details;
			// 
			// tabPage3
			// 
			this.tabPage3.AutoScroll = true;
			this.tabPage3.Controls.Add(this.treeViewChannels);
			this.tabPage3.Controls.Add(this.btnGrpChnDown);
			this.tabPage3.Controls.Add(this.btnGrpChnUp);
			this.tabPage3.Controls.Add(this.buttonMap);
			this.tabPage3.Controls.Add(this.btnUnmap);
			this.tabPage3.Controls.Add(this.listViewTVGroupChannels);
			this.tabPage3.Controls.Add(this.label3);
			this.tabPage3.Controls.Add(this.label2);
			this.tabPage3.Controls.Add(this.label1);
			this.tabPage3.Controls.Add(this.comboBox1);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(432, 390);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Map channels";
			this.tabPage3.Visible = false;
			// 
			// treeViewChannels
			// 
			this.treeViewChannels.FullRowSelect = true;
			this.treeViewChannels.ImageIndex = -1;
			this.treeViewChannels.Location = new System.Drawing.Point(16, 88);
			this.treeViewChannels.Name = "treeViewChannels";
			this.treeViewChannels.SelectedImageIndex = -1;
			this.treeViewChannels.Size = new System.Drawing.Size(168, 248);
			this.treeViewChannels.Sorted = true;
			this.treeViewChannels.TabIndex = 10;
			// 
			// btnGrpChnDown
			// 
			this.btnGrpChnDown.Location = new System.Drawing.Point(304, 344);
			this.btnGrpChnDown.Name = "btnGrpChnDown";
			this.btnGrpChnDown.Size = new System.Drawing.Size(56, 23);
			this.btnGrpChnDown.TabIndex = 9;
			this.btnGrpChnDown.Text = "Down";
			// 
			// btnGrpChnUp
			// 
			this.btnGrpChnUp.Location = new System.Drawing.Point(264, 344);
			this.btnGrpChnUp.Name = "btnGrpChnUp";
			this.btnGrpChnUp.Size = new System.Drawing.Size(32, 23);
			this.btnGrpChnUp.TabIndex = 8;
			this.btnGrpChnUp.Text = "Up";
			// 
			// buttonMap
			// 
			this.buttonMap.Location = new System.Drawing.Point(192, 184);
			this.buttonMap.Name = "buttonMap";
			this.buttonMap.Size = new System.Drawing.Size(32, 23);
			this.buttonMap.TabIndex = 7;
			this.buttonMap.Text = ">>";
			// 
			// btnUnmap
			// 
			this.btnUnmap.Location = new System.Drawing.Point(192, 224);
			this.btnUnmap.Name = "btnUnmap";
			this.btnUnmap.Size = new System.Drawing.Size(32, 23);
			this.btnUnmap.TabIndex = 6;
			this.btnUnmap.Text = "<<";
			// 
			// listViewTVGroupChannels
			// 
			this.listViewTVGroupChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																															this.columnHeader9});
			this.listViewTVGroupChannels.FullRowSelect = true;
			this.listViewTVGroupChannels.HideSelection = false;
			this.listViewTVGroupChannels.Location = new System.Drawing.Point(240, 88);
			this.listViewTVGroupChannels.Name = "listViewTVGroupChannels";
			this.listViewTVGroupChannels.Size = new System.Drawing.Size(168, 240);
			this.listViewTVGroupChannels.TabIndex = 5;
			this.listViewTVGroupChannels.View = System.Windows.Forms.View.Details;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(240, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(120, 16);
			this.label3.TabIndex = 3;
			this.label3.Text = "TV channels in group";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(128, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "TVGroups available";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Group:";
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.Location = new System.Drawing.Point(40, 32);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(280, 21);
			this.comboBox1.TabIndex = 0;
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
			// columnHeader9
			// 
			this.columnHeader9.Text = "TV Channel";
			this.columnHeader9.Width = 161;
			// 
			// TVGroups
			// 
			this.Controls.Add(this.tabControl1);
			this.Name = "TVGroups";
			this.Size = new System.Drawing.Size(472, 448);
			this.Load += new System.EventHandler(this.TVGroups_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

    private string GetStandardName(AnalogVideoStandard standard)
    {
      string name = standard.ToString();
      name = name.Replace("_", " ");
      return name == "None" ? "Default" : name;
    }



		public override void LoadSettings()
		{
			LoadTVGroups();
			LoadGroups();
		}

		public override void SaveSettings()
		{
			if (reloadList)
			{
				LoadTVGroups();
				LoadGroups();
				reloadList=false;
				isDirty=true;
			}
			SaveTVGroups();
			SaveGroups();
		}

		private void SaveTVGroups()
		{
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
		private void LoadTVGroups()
		{
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

    

		static public void UpdateList()
		{
			reloadList=true;
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (reloadList)
			{
				reloadList=false;
				LoadTVGroups();
				LoadGroups();
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
					TVDatabase.DeleteGroup(group);
					group.ID=-1;
					
					string pincode="No";
					if (group.Pincode!=0)
						pincode="Yes";

					listItem.SubItems[0].Text = group.GroupName;
					listItem.SubItems[1].Text = pincode;
					TVDatabase.AddGroup(group);

					SaveTVGroups();
					SaveGroups();
					UpdateGroupChannels(group,true);
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

			SaveTVGroups();
			SaveGroups();
			UpdateGroupChannels(null,true);
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
				
				SaveGroups();
				LoadGroups();

				SaveTVGroups();
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
			if (treeViewChannels.SelNodes==null) return;
			Hashtable htSelNodes = treeViewChannels.SelNodes.Clone() as Hashtable;
			treeViewChannels.SelNodes=null;
			foreach(MWTreeNodeWrapper node in htSelNodes.Values)
			{
				TVChannel chan=node.Node.Tag as TVChannel;
				if (chan==null) return;
				TVGroup group = comboBox1.SelectedItem as TVGroup;
				ListViewItem listItem = new ListViewItem(new string[] { chan.Name} );
				listItem.Tag=chan;
				listViewTVGroupChannels.Items.Add(listItem);
				if (group!=null && chan != null)
					TVDatabase.MapChannelToGroup(group, chan);
				treeViewChannels.Nodes.Remove(node.Node);
			}

		}

		private void btnUnmap_Click(object sender, System.EventArgs e)
		{
			if (listViewTVGroupChannels.SelectedItems==null) return;
			for(int i=0; i < listViewTVGroupChannels.SelectedItems.Count;++i)
			{
				ListViewItem listItem=listViewTVGroupChannels.SelectedItems[i];
				TVChannel chan=(TVChannel)listItem.Tag;

				foreach (TreeNode node in treeViewChannels.Nodes)
				{ 
					if (node.Text==chan.ProviderName)
					{
						TreeNode subnode = new TreeNode(chan.Name);
						subnode.Tag=chan;
						node.Nodes.Add(subnode);
					}
				}
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

			ArrayList groupChannels = new ArrayList();
			listViewTVGroupChannels.Items.Clear();
			if (group!=null)
			{
				TVDatabase.GetTVChannelsForGroup(group);
				foreach (TVChannel chan in group.tvChannels)
				{
					ListViewItem listItem = new ListViewItem(new string[] { chan.Name} );
					listItem.Tag=chan;
					listViewTVGroupChannels.Items.Add(listItem);
					groupChannels.Add(chan);
				}
			}

			//fill in treeview with provider/channels
			string lastProvider="";
			TreeNode node=null;
			treeViewChannels.Nodes.Clear();
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannelsByProvider(ref channels);
			foreach (TVChannel chan in channels)
			{
				bool add=true;
				foreach (TVChannel grpChan in groupChannels)
				{
					if (grpChan.Name == chan.Name)
					{
						add=false;
						break;
					}
				}
				if (add)
				{
					if (lastProvider!=chan.ProviderName)
					{
						lastProvider=chan.ProviderName;
						if(node!=null)
							treeViewChannels.Nodes.Add(node);
						node=new TreeNode(chan.ProviderName);
						node.Tag="";
					}
					TreeNode nodeChan = new TreeNode(chan.Name);
					nodeChan.Tag=chan;
					node.Nodes.Add(nodeChan);
				}
			}
			if(node!=null && node.Nodes.Count>0)
				treeViewChannels.Nodes.Add(node);
		}

		private void listViewTVGroupChannels_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			switch (listViewTVGroupChannels.Sorting)
			{
				case SortOrder.Ascending: listViewTVGroupChannels.Sorting = SortOrder.Descending; break;
				case SortOrder.Descending: listViewTVGroupChannels.Sorting = SortOrder.Ascending; break;
				case SortOrder.None: listViewTVGroupChannels.Sorting = SortOrder.Ascending; break;
			}	
			listViewTVGroupChannels.Sort();
			listViewTVGroupChannels.Update();
		
		}


		private void btnGrpChnUp_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			for(int index = 0; index < listViewTVGroupChannels.Items.Count; index++)
			{
				if(listViewTVGroupChannels.Items[index].Selected == true)
				{
					//
					// Make sure the current index isn't smaller than the lowest index (0) in the list view
					//
					if(index > 0)
					{
						ListViewItem listItem = listViewTVGroupChannels.Items[index];
						listViewTVGroupChannels.Items.RemoveAt(index);
						listViewTVGroupChannels.Items.Insert(index - 1, listItem);
					}
				}
			}    
			TVGroup group = (TVGroup) comboBox1.SelectedItem;
			TVDatabase.DeleteChannelsFromGroup(group);
			for(int index = 0; index < listViewTVGroupChannels.Items.Count; index++)
			{
				group.tvChannels.Clear();
				ListViewItem listItem = listViewTVGroupChannels.Items[index];
				group.tvChannels.Add (listItem.Tag);
				TVDatabase.MapChannelToGroup(group, (TVChannel)listItem.Tag);
			}
		}

		private void btnGrpChnDown_Click(object sender, System.EventArgs e)
		{
			isDirty = true;

			for(int index = listViewTVGroupChannels.Items.Count - 1; index >= 0; index--)
			{
				if(listViewTVGroupChannels.Items[index].Selected == true)
				{
					//
					// Make sure the current index isn't greater than the highest index in the list view
					//
					if(index < listViewTVGroupChannels.Items.Count - 1)
					{
						ListViewItem listItem = listViewTVGroupChannels.Items[index];
						listViewTVGroupChannels.Items.RemoveAt(index);

						if(index + 1 < listViewTVGroupChannels.Items.Count)
						{
							listViewTVGroupChannels.Items.Insert(index + 1, listItem);
						}
						else
						{
							listViewTVGroupChannels.Items.Add(listItem);
						}
					}
				}
			}
			
			TVGroup group = (TVGroup) comboBox1.SelectedItem;
			TVDatabase.DeleteChannelsFromGroup(group);
			for(int index = 0; index < listViewTVGroupChannels.Items.Count; index++)
			{
				group.tvChannels.Clear();
				ListViewItem listItem = listViewTVGroupChannels.Items[index];
				group.tvChannels.Add (listItem.Tag);
				TVDatabase.MapChannelToGroup(group, (TVChannel)listItem.Tag);
			}
		
		}


		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SaveSettings();
			LoadSettings();			
		}

		private void TVGroups_Load(object sender, System.EventArgs e)
		{
		
		}
		public override void OnSectionActivated()
		{
			base.OnSectionActivated ();
			LoadSettings();
		}

	}
}

