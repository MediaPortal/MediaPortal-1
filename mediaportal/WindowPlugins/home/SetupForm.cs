using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using MediaPortal.GUI.Library;

namespace home
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
	public class SetupForm : System.Windows.Forms.Form , ISetupForm
	{
		private int NodeCount, FolderCount;
		private string NodeMap;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textBox5;
		private System.Windows.Forms.TextBox textBox6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.TextBox textBox4;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox listBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox chkBoxScrolling;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.TreeView treeView;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		enum TagTypes
		{
			PLUGIN			= 0,
			MENU_TAG		= 1,
			EMPTY				= 3
		};

		class ItemTag
		{
			public string				DLLName;
			public string				author;
			public string				buttonText;
			public string				pluginName;
			public string				description;
			public string				picture;
			public int					windowId=-1;
			public TagTypes			tagType;
		};

		ArrayList availablePlugins = new ArrayList();
		private System.Windows.Forms.CheckBox chkBoxFixed;
		ArrayList loadedPlugins = new ArrayList();
		bool	addPicture=false;
		string	skinName;

		private System.Windows.Forms.Button MakeMenu;
		private System.Windows.Forms.Button SaveAll;
		private System.Windows.Forms.Button CopyItem;
		private System.Windows.Forms.Button DeleteItem;
		private System.Windows.Forms.Button AddMenu;
		private System.Windows.Forms.CheckBox NoScrollSubs;
		private System.Windows.Forms.Button AddPicture;
		private System.Windows.Forms.Button SearchPicture;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button deletePicture;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.RadioButton useMenus;
		private System.Windows.Forms.RadioButton useMyPlugins;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.PictureBox pictureBox3;
		private System.Windows.Forms.GroupBox groupBox4;

		#region plugin vars	
		public bool	CanEnable()		// Indicates whether plugin can be enabled/disabled
		{
			return false;
		}
		
		public int  GetWindowId()
		{
			return (int)GUIWindow.Window.WINDOW_HOME;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText="";
			strButtonImage="";
			strButtonImageFocus="";
			strPictureImage="";
			return false;
		}

		public bool DefaultEnabled()
		{
			return true;
		}
		
		public string PluginName() 
		{
			return "Home";
		}
		
		public string Description()
		{
			return "Mediaportals home screen";
		}
		
		public string Author()
		{
			return "Gucky62/Frodo";
		}
		
		public void ShowPlugin()
		{
			ShowDialog();
		}

		public bool HasSetup()
		{
			return true;
		}
		#endregion

		public SetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			this.NodeCount = 0;
			this.FolderCount= 0;

			ImageList TreeviewIL = new ImageList();
			TreeviewIL.Images.Add(System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowPlugins.home.resources.folder.png")));
			TreeviewIL.Images.Add(System.Drawing.Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowPlugins.home.resources.node.png")));
			
			this.treeView.ImageList = TreeviewIL;
			this.treeView.HideSelection = false;
			this.treeView.ItemHeight = this.treeView.ItemHeight + 3;
			this.treeView.Indent = this.treeView.Indent + 3;

			EnumeratePlugins();
			LoadSettings();
			LoadPlugins();
			PopulateListBox();
			MakeMenu.Visible=false;

			textBox1.Text="";
			textBox2.Text="";
			textBox3.Text="";
			textBox4.Text="";
			textBox5.Text="";
			textBox6.Text="";
			
			this.treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDown);
			this.treeView.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView_DragOver);
			this.treeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_DragEnter);
			this.treeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
			this.treeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_DragDrop);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SetupForm));
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.SearchPicture = new System.Windows.Forms.Button();
			this.MakeMenu = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.textBox6 = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.SaveAll = new System.Windows.Forms.Button();
			this.DeleteItem = new System.Windows.Forms.Button();
			this.AddMenu = new System.Windows.Forms.Button();
			this.CopyItem = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.listBox = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.useMyPlugins = new System.Windows.Forms.RadioButton();
			this.useMenus = new System.Windows.Forms.RadioButton();
			this.NoScrollSubs = new System.Windows.Forms.CheckBox();
			this.chkBoxFixed = new System.Windows.Forms.CheckBox();
			this.chkBoxScrolling = new System.Windows.Forms.CheckBox();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.treeView = new System.Windows.Forms.TreeView();
			this.AddPicture = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.deletePicture = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.pictureBox3 = new System.Windows.Forms.PictureBox();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.pictureBox1);
			this.groupBox3.Location = new System.Drawing.Point(248, 8);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(216, 208);
			this.groupBox3.TabIndex = 27;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Button Image (for Menu Items)";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(8, 16);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(208, 192);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.SearchPicture);
			this.groupBox2.Controls.Add(this.MakeMenu);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.textBox5);
			this.groupBox2.Controls.Add(this.textBox6);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.textBox3);
			this.groupBox2.Controls.Add(this.textBox4);
			this.groupBox2.Controls.Add(this.textBox2);
			this.groupBox2.Controls.Add(this.textBox1);
			this.groupBox2.Location = new System.Drawing.Point(480, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(344, 208);
			this.groupBox2.TabIndex = 26;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Tag Info";
			// 
			// SearchPicture
			// 
			this.SearchPicture.Location = new System.Drawing.Point(304, 16);
			this.SearchPicture.Name = "SearchPicture";
			this.SearchPicture.Size = new System.Drawing.Size(32, 24);
			this.SearchPicture.TabIndex = 12;
			this.SearchPicture.Text = "...";
			this.SearchPicture.Visible = false;
			this.SearchPicture.Click += new System.EventHandler(this.SearchPicture_Click);
			// 
			// MakeMenu
			// 
			this.MakeMenu.Location = new System.Drawing.Point(144, 160);
			this.MakeMenu.Name = "MakeMenu";
			this.MakeMenu.Size = new System.Drawing.Size(80, 24);
			this.MakeMenu.TabIndex = 1;
			this.MakeMenu.Text = "Make Menu";
			this.MakeMenu.Click += new System.EventHandler(this.MakeMenu_Click);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(48, 120);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(80, 16);
			this.label7.TabIndex = 11;
			this.label7.Text = "Author";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(48, 144);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(80, 16);
			this.label8.TabIndex = 10;
			this.label8.Text = "Plugin Name";
			// 
			// textBox5
			// 
			this.textBox5.Location = new System.Drawing.Point(128, 112);
			this.textBox5.Name = "textBox5";
			this.textBox5.Size = new System.Drawing.Size(168, 20);
			this.textBox5.TabIndex = 9;
			this.textBox5.Text = "textBox5";
			// 
			// textBox6
			// 
			this.textBox6.Location = new System.Drawing.Point(128, 136);
			this.textBox6.Name = "textBox6";
			this.textBox6.Size = new System.Drawing.Size(168, 20);
			this.textBox6.TabIndex = 8;
			this.textBox6.Text = "textBox6";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(48, 72);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 16);
			this.label5.TabIndex = 7;
			this.label5.Text = "DLL Name";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(48, 96);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(80, 16);
			this.label6.TabIndex = 6;
			this.label6.Text = "Plugin Name";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(48, 48);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(80, 16);
			this.label4.TabIndex = 5;
			this.label4.Text = "Button Text";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(48, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 16);
			this.label3.TabIndex = 4;
			this.label3.Text = "Tag Type";
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(128, 64);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(168, 20);
			this.textBox3.TabIndex = 3;
			this.textBox3.Text = "textBox3";
			// 
			// textBox4
			// 
			this.textBox4.Location = new System.Drawing.Point(128, 88);
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(168, 20);
			this.textBox4.TabIndex = 2;
			this.textBox4.Text = "textBox4";
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(128, 40);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(168, 20);
			this.textBox2.TabIndex = 1;
			this.textBox2.Text = "textBox2";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(128, 16);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(168, 20);
			this.textBox1.TabIndex = 0;
			this.textBox1.Text = "textBox1";
			// 
			// SaveAll
			// 
			this.SaveAll.Location = new System.Drawing.Point(376, 488);
			this.SaveAll.Name = "SaveAll";
			this.SaveAll.Size = new System.Drawing.Size(88, 24);
			this.SaveAll.TabIndex = 25;
			this.SaveAll.Text = "Save";
			this.SaveAll.Click += new System.EventHandler(this.SaveAll_Click);
			// 
			// DeleteItem
			// 
			this.DeleteItem.Location = new System.Drawing.Point(376, 288);
			this.DeleteItem.Name = "DeleteItem";
			this.DeleteItem.Size = new System.Drawing.Size(88, 24);
			this.DeleteItem.TabIndex = 22;
			this.DeleteItem.Text = "Delete Item";
			this.DeleteItem.Click += new System.EventHandler(this.DeleteItem_Click);
			// 
			// AddMenu
			// 
			this.AddMenu.Location = new System.Drawing.Point(376, 352);
			this.AddMenu.Name = "AddMenu";
			this.AddMenu.Size = new System.Drawing.Size(88, 24);
			this.AddMenu.TabIndex = 21;
			this.AddMenu.Text = "Add Menu";
			this.AddMenu.Click += new System.EventHandler(this.AddMenu_Click);
			// 
			// CopyItem
			// 
			this.CopyItem.Location = new System.Drawing.Point(376, 256);
			this.CopyItem.Name = "CopyItem";
			this.CopyItem.Size = new System.Drawing.Size(88, 24);
			this.CopyItem.TabIndex = 19;
			this.CopyItem.Text = "<---";
			this.CopyItem.Click += new System.EventHandler(this.CopyItem_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(480, 224);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 16);
			this.label2.TabIndex = 18;
			this.label2.Text = "Available Plugins";
			// 
			// listBox
			// 
			this.listBox.Location = new System.Drawing.Point(480, 240);
			this.listBox.Name = "listBox";
			this.listBox.Size = new System.Drawing.Size(344, 277);
			this.listBox.TabIndex = 17;
			this.listBox.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 224);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 16);
			this.label1.TabIndex = 16;
			this.label1.Text = "Menu Structure";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.groupBox4);
			this.groupBox1.Controls.Add(this.NoScrollSubs);
			this.groupBox1.Controls.Add(this.chkBoxFixed);
			this.groupBox1.Controls.Add(this.chkBoxScrolling);
			this.groupBox1.Controls.Add(this.radioButton2);
			this.groupBox1.Controls.Add(this.radioButton1);
			this.groupBox1.Location = new System.Drawing.Point(16, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(200, 208);
			this.groupBox1.TabIndex = 15;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Home settings";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.useMyPlugins);
			this.groupBox4.Controls.Add(this.useMenus);
			this.groupBox4.Location = new System.Drawing.Point(0, 144);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(200, 64);
			this.groupBox4.TabIndex = 7;
			this.groupBox4.TabStop = false;
			// 
			// useMyPlugins
			// 
			this.useMyPlugins.Checked = true;
			this.useMyPlugins.Location = new System.Drawing.Point(24, 16);
			this.useMyPlugins.Name = "useMyPlugins";
			this.useMyPlugins.Size = new System.Drawing.Size(168, 24);
			this.useMyPlugins.TabIndex = 1;
			this.useMyPlugins.TabStop = true;
			this.useMyPlugins.Text = "Use MyPlugins (Frodo)";
			// 
			// useMenus
			// 
			this.useMenus.Location = new System.Drawing.Point(24, 40);
			this.useMenus.Name = "useMenus";
			this.useMenus.Size = new System.Drawing.Size(168, 16);
			this.useMenus.TabIndex = 0;
			this.useMenus.Text = "Use Menus (Gucky62)";
			// 
			// NoScrollSubs
			// 
			this.NoScrollSubs.Enabled = false;
			this.NoScrollSubs.Location = new System.Drawing.Point(24, 112);
			this.NoScrollSubs.Name = "NoScrollSubs";
			this.NoScrollSubs.Size = new System.Drawing.Size(168, 24);
			this.NoScrollSubs.TabIndex = 6;
			this.NoScrollSubs.Text = "NoScroll SubMenus";
			// 
			// chkBoxFixed
			// 
			this.chkBoxFixed.Location = new System.Drawing.Point(24, 88);
			this.chkBoxFixed.Name = "chkBoxFixed";
			this.chkBoxFixed.Size = new System.Drawing.Size(168, 24);
			this.chkBoxFixed.TabIndex = 3;
			this.chkBoxFixed.Text = "Fix Scroll Bar";
			// 
			// chkBoxScrolling
			// 
			this.chkBoxScrolling.Checked = true;
			this.chkBoxScrolling.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkBoxScrolling.Location = new System.Drawing.Point(24, 64);
			this.chkBoxScrolling.Name = "chkBoxScrolling";
			this.chkBoxScrolling.Size = new System.Drawing.Size(168, 24);
			this.chkBoxScrolling.TabIndex = 2;
			this.chkBoxScrolling.Text = "Scroll menu items";
			// 
			// radioButton2
			// 
			this.radioButton2.Location = new System.Drawing.Point(24, 40);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.TabIndex = 1;
			this.radioButton2.Text = "MM-DD-YYYY";
			// 
			// radioButton1
			// 
			this.radioButton1.Location = new System.Drawing.Point(24, 16);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(112, 24);
			this.radioButton1.TabIndex = 0;
			this.radioButton1.Text = "DD-MM-YYYY";
			// 
			// treeView
			// 
			this.treeView.AllowDrop = true;
			this.treeView.ImageIndex = -1;
			this.treeView.LabelEdit = true;
			this.treeView.Location = new System.Drawing.Point(16, 240);
			this.treeView.Name = "treeView";
			this.treeView.SelectedImageIndex = -1;
			this.treeView.Size = new System.Drawing.Size(344, 280);
			this.treeView.TabIndex = 14;
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
			// 
			// AddPicture
			// 
			this.AddPicture.Location = new System.Drawing.Point(376, 384);
			this.AddPicture.Name = "AddPicture";
			this.AddPicture.Size = new System.Drawing.Size(88, 24);
			this.AddPicture.TabIndex = 28;
			this.AddPicture.Text = "Link Picture";
			this.AddPicture.Click += new System.EventHandler(this.AddPicture_Click);
			// 
			// deletePicture
			// 
			this.deletePicture.Location = new System.Drawing.Point(376, 320);
			this.deletePicture.Name = "deletePicture";
			this.deletePicture.Size = new System.Drawing.Size(88, 24);
			this.deletePicture.TabIndex = 29;
			this.deletePicture.Text = "Delete Picture";
			this.deletePicture.Click += new System.EventHandler(this.deletePicture_Click);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 528);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(160, 32);
			this.label9.TabIndex = 30;
			this.label9.Text = "You can move any Menu Item with drag and drop";
			// 
			// pictureBox2
			// 
			this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
			this.pictureBox2.Location = new System.Drawing.Point(184, 528);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(16, 24);
			this.pictureBox2.TabIndex = 31;
			this.pictureBox2.TabStop = false;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(208, 528);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(160, 16);
			this.label10.TabIndex = 32;
			this.label10.Text = "is a Menu Item";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(208, 552);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(160, 16);
			this.label11.TabIndex = 34;
			this.label11.Text = "is a Plugin Item";
			// 
			// pictureBox3
			// 
			this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
			this.pictureBox3.Location = new System.Drawing.Point(184, 552);
			this.pictureBox3.Name = "pictureBox3";
			this.pictureBox3.Size = new System.Drawing.Size(16, 24);
			this.pictureBox3.TabIndex = 33;
			this.pictureBox3.TabStop = false;
			// 
			// SetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(840, 574);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.pictureBox3);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.pictureBox2);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.deletePicture);
			this.Controls.Add(this.AddPicture);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.SaveAll);
			this.Controls.Add(this.DeleteItem);
			this.Controls.Add(this.AddMenu);
			this.Controls.Add(this.CopyItem);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.listBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.treeView);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SetupForm";
			this.Text = "Home Setup";
			this.Load += new System.EventHandler(this.SetupForm_Load);
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void AddMenu_Click(object sender, System.EventArgs e)
		{
			MakeMenu.Visible=true;
			MakeMenu.Text="Make Menu";
			label3.Text="Menu Name";
			textBox1.Text="";
			textBox2.Text="";
			textBox3.Text="";
			textBox4.Text="";
			textBox5.Text="";
			textBox6.Text="";
			label4.Visible=false;
			textBox2.Visible=false;
			label5.Visible=false;
			textBox3.Visible=false;
			label6.Visible=false;
			textBox4.Visible=false;
			label7.Visible=false;
			textBox5.Visible=false;
			label8.Visible=false;
			textBox6.Visible=false;
		}

		private void MakeMenu_Click(object sender, System.EventArgs e)
		{
			MakeMenu.Visible=false;
			textBox2.Visible=true;
			label4.Visible=true;
			textBox2.Visible=true;
			label5.Visible=true;
			textBox3.Visible=true;
			label6.Visible=true;
			textBox4.Visible=true;
			label7.Visible=true;
			textBox5.Visible=true;
			label8.Visible=true;
			textBox6.Visible=true;
			if (addPicture==true) 
			{
				addPicture=false;
				TreeNode tn = new TreeNode();
				if (treeView.SelectedNode!=null) 
				{
					tn=treeView.SelectedNode;
					if (tn.Text.IndexOf("{",0)>0) 
					{						
						int l=tn.Text.IndexOf(" {",0);
						tn.Text=tn.Text.Substring(0,l);
						tn.Text=tn.Text+" {"+textBox1.Text+"}";
					} 
					else 
					{
						tn.Text=tn.Text+" {"+textBox1.Text+"}";
					}
				}			
				SearchPicture.Visible=false;
				UpdateTagInfo(textBox1.Text);
			} 
			else 
			{
			  ++this.FolderCount;
				string mName=	"("+textBox1.Text+")";
				this.treeView.Nodes.Add(new TreeNode(mName, 0, 0));			
				UpdateTagInfo(mName);
			}
		}

		private void treeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			UpdateTagInfo(treeView.SelectedNode.Text);		
		}

		private void CopyItem_Click(object sender, System.EventArgs e)
		{
			++this.NodeCount;
			this.treeView.Nodes.Add(new TreeNode(listBox.SelectedItem.ToString(), 1, 1));
		}

		private void SaveAll_Click(object sender, System.EventArgs e)
		{
			saveTree(treeView, Application.StartupPath + @"\menu.bin");
			using (AMS.Profile.Xml xmlWriter=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				int iLayout=0;
				if (radioButton2.Checked) iLayout=1;
				xmlWriter.SetValue("home","datelayout",iLayout.ToString());
				xmlWriter.SetValueAsBool("home","scroll",chkBoxScrolling.Checked);
				xmlWriter.SetValueAsBool("home","scrollfixed",chkBoxFixed.Checked);
				xmlWriter.SetValueAsBool("home","usemenus",useMenus.Checked);
				xmlWriter.SetValueAsBool("home","usemyplugins",useMyPlugins.Checked);
				xmlWriter.SetValueAsBool("home","noScrollsubs",NoScrollSubs.Checked);			
			}
			this.Close();
		}

		private void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				int iLayout = xmlreader.GetValueAsInt("home","datelayout",0);
				if (iLayout==0) radioButton1.Checked=true;
				else radioButton2.Checked=true;

				chkBoxScrolling.Checked=xmlreader.GetValueAsBool("home","scroll",true);
				chkBoxFixed.Checked=xmlreader.GetValueAsBool("home","scrollfixed",false);
				useMenus.Checked=xmlreader.GetValueAsBool("home","usemenus",false);
				useMyPlugins.Checked=xmlreader.GetValueAsBool("home","usemyplugins",true);
				NoScrollSubs.Checked=xmlreader.GetValueAsBool("home","noScrollsubs",true);
				skinName=xmlreader.GetValueAsString("skin","name","BlueTwo");
			}
			loadTree(treeView, Application.StartupPath + @"\menu.bin");
		}

		private void UpdateTagInfo(string name)
		{
			if (name.StartsWith("(")) 
			{
				string strBtnFile="";
				int l1=name.IndexOf("{",0);
				int l2=name.IndexOf("}",0);
				string strBtnText=name.Substring(1,name.IndexOf(")",0)-1);
				if(l1>0) 
				{
					strBtnFile=name.Substring(l1+1,(l2-l1)-1);
					pictureBox1.Image=Image.FromFile(System.IO.Directory.GetCurrentDirectory()+"\\skin\\"+skinName+"\\media\\"+strBtnFile,true);
				}
				label3.Text="Tag Type";
				textBox1.Text="Menu Tag";
				label4.Text="Button Text";
				textBox2.Text=strBtnText;
				label5.Text="Button Picture";
				textBox3.Text=strBtnFile;
				label6.Text="";
				textBox4.Text="";
				label7.Text="";
				textBox5.Text="";
				label8.Text="";
				textBox6.Text="";
			} 
			else 
			{
				foreach(ItemTag tag in loadedPlugins)
				{
					if (name==tag.pluginName) 
					{
						if(tag.tagType==TagTypes.PLUGIN) 
						{
							pictureBox1.Image=null;
							label3.Text="Tag Type";
							textBox1.Text="Plugin";
							label4.Text="Button Text";
							textBox2.Text=tag.buttonText;
							label5.Text="DLL Name";
							textBox3.Text=tag.DLLName;
							label6.Text="Plugin Name";
							textBox4.Text=tag.pluginName;
							label7.Text="Author";
							textBox5.Text=tag.author;
							label8.Text="Description";
							textBox6.Text=tag.description;
							groupBox3.Text=tag.picture;
						} 
						break;	
					}
				}
			}
		}

		private void DeleteItem_Click(object sender, System.EventArgs e)
		{
			TreeNode tn = new TreeNode();
			if (treeView.SelectedNode!=null) 
			{
				tn=treeView.SelectedNode;
				treeView.Nodes.Remove(tn);
			} 
			else 
			{
				MessageBox.Show( "Please select a Item in the Menu Structure!" );
			}
		}

		private void listBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			UpdateTagInfo(listBox.SelectedItem.ToString());
		}
	
		private void PopulateListBox()
		{
			foreach(ItemTag tag in loadedPlugins)
			{
				listBox.Items.Add(tag.pluginName);
			}
		}

		private void EnumeratePluginDirectory(string directory)
		{
			if(Directory.Exists(directory))
			{
				string[] files = Directory.GetFiles(directory, "*.dll");
				foreach (string file in files)
				{
					availablePlugins.Add(file);
				}
			}
		}

		private void EnumeratePlugins()
		{
			EnumeratePluginDirectory(@"plugins\windows");
			EnumeratePluginDirectory(@"plugins\subtitle");
			EnumeratePluginDirectory(@"plugins\tagreaders");
			EnumeratePluginDirectory(@"plugins\externalplayers");
			EnumeratePluginDirectory(@"plugins\process");
		}

		private void LoadPlugins()
		{
			string buttontxt, buttonimage,buttonimagefocus,picture;
			foreach(string pluginFile in availablePlugins)
			{
				try
				{
					Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);
					if(pluginAssembly != null)
					{
						Type[] exportedTypes = pluginAssembly.GetExportedTypes();
						foreach(Type type in exportedTypes)
						{
							// an abstract class cannot be instanciated
							if( type.IsAbstract ) continue;
							//
							// Try to locate the interface we're interested in
							//
							if(type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
							{ 
								if (type.FullName!="GUIHomeMenu.SetupForm" && type.FullName!="home.SetupForm")
								{
									try
									{
										//
										// Create instance of the current type
										//
										object pluginObject = (object)Activator.CreateInstance(type);
										ISetupForm pluginForm = pluginObject as ISetupForm;

										if(pluginForm != null)
										{
											ItemTag tag = new ItemTag();
											tag.pluginName=pluginForm.PluginName();
											tag.author=pluginForm.Author();
											pluginForm.GetHome(out buttontxt, out buttonimage,out buttonimagefocus,out picture);
											tag.buttonText=buttontxt;
											tag.picture=picture;
											tag.description=pluginForm.Description();
											tag.DLLName=pluginFile.Substring(pluginFile.LastIndexOf(@"\")+1);
											tag.windowId=pluginForm.GetWindowId();
											tag.tagType=TagTypes.PLUGIN;
											if (pluginForm.CanEnable() || pluginForm.DefaultEnabled()) 
											{
												loadedPlugins.Add(tag);
											}
										}
									}
									catch(Exception setupFormException)
									{
										Log.Write("Exception in plugin SetupForm loading :{0}", setupFormException.Message);
										Log.Write("Current class is :{0}", type.FullName);
									}
								}
							}
						}
					}
				}
				catch(Exception unknownException)
				{
					Log.Write("Exception in plugin loading :{0}", unknownException.Message);
				}
			}
		}

		#region Load and Save Tree

		public static int saveTree(TreeView tree, string filename)
		{
			ArrayList al = new ArrayList();
			foreach (TreeNode tn in tree.Nodes)
			{
				saveNode(tn, al);
			}
			Stream file = File.Open(filename, FileMode.Create);
			BinaryFormatter bf = new BinaryFormatter();
			try
			{
				bf.Serialize(file, al);
			}
			catch (System.Runtime.Serialization.SerializationException e)
			{
				MessageBox.Show("Serialization failed : {0}", e.Message);
				return -1; 
			}
			file.Close();

			return 0;
		}

		private static void saveNode(TreeNode tn, ArrayList al)
		{
			Hashtable ht = new Hashtable();
			ht.Add("Tag", tn.Tag);
			ht.Add("Text", (object)tn.Text);
			ht.Add("FullPath", (object)tn.FullPath);
			ht.Add("SelectedImageIndex", (object)tn.SelectedImageIndex);
			ht.Add("ImageIndex", (object)tn.ImageIndex);
			al.Add((object)ht);

			foreach (TreeNode n in tn.Nodes)
			{
				saveNode(n, al);
			}

		}

		public static int loadTree(TreeView tree, string filename)
		{
			if (File.Exists(filename))
			{
				Stream file = File.Open(filename, FileMode.Open);
				BinaryFormatter bf = new BinaryFormatter();
				object obj = null;
				try
				{
					obj = bf.Deserialize(file);
				}
				catch (System.Runtime.Serialization.SerializationException e)
				{
					MessageBox.Show("De-Serialization failed : {0}", e.Message);
					return -1;
				}
				file.Close();

				ArrayList alist = obj as ArrayList;
				foreach (object item in alist)
				{
					Hashtable ht = item as Hashtable;
					TreeNode tn = new TreeNode(ht["Text"].ToString());
					tn.Tag = ht["Tag"];
					tn.ImageIndex = Convert.ToInt32(ht["SelectedImageIndex"].ToString());
					tn.SelectedImageIndex = Convert.ToInt32(ht["SelectedImageIndex"].ToString());
					string fPath = ht["FullPath"].ToString();
					string[] parts = fPath.Split(tree.PathSeparator.ToCharArray());
					if (parts.Length > 1)
					{
						TreeNode parentNode = null;
						TreeNodeCollection nodes = tree.Nodes;
						searchNode(parts, ref parentNode, nodes);

						if (parentNode != null)
						{
							parentNode.Nodes.Add(tn);
						}
					}
					else tree.Nodes.Add(tn);
				}
				return 0;
			}
			else 
			{
				tree.Nodes.Add("Menu");				
				return -2; 
			}
		}
		
		private static void searchNode(string[] parts, ref TreeNode parentNode, TreeNodeCollection nodes)
		{
			foreach (TreeNode n in nodes)
			{
				if (n.Text.Equals(parts[parts.Length - 2].ToString()))
				{
					parentNode = n;
					return;
				}
				else searchNode(parts, ref parentNode, n.Nodes);
			}
		}
		#endregion

		private void AddPicture_Click(object sender, System.EventArgs e)
		{
			TreeNode tn = new TreeNode();
			if (treeView.SelectedNode!=null) 
			{
				tn=treeView.SelectedNode;		
				if (tn.Text.StartsWith("("))
				{
					SearchPicture.Visible=true;
					addPicture=true;
					MakeMenu.Visible=true;
					MakeMenu.Text="Make Picture";
					label3.Text="Picture Name";
					textBox1.Text="";
					textBox2.Text="";
					textBox3.Text="";
					textBox4.Text="";
					textBox5.Text="";
					textBox6.Text="";
					label4.Visible=false;
					textBox2.Visible=false;
					label5.Visible=false;
					textBox3.Visible=false;
					label6.Visible=false;
					textBox4.Visible=false;
					label7.Visible=false;
					textBox5.Visible=false;
					label8.Visible=false;
					textBox6.Visible=false;
				}
				else 
				{
					MessageBox.Show( "Pictures can only attach to a Menu Item" );
				}
			} 
			else 
			{
				MessageBox.Show( "Please select a Menu Item in the Menu Structure!" );
			}
		}

		private void SearchPicture_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.RestoreDirectory = true;
			openFileDialog1.DefaultExt=".png";		
			openFileDialog1.FileName="hover*";		
			openFileDialog1.InitialDirectory=System.IO.Directory.GetCurrentDirectory()+"\\skin\\"+skinName+"\\media";
			if( openFileDialog1.ShowDialog( this ) == DialogResult.OK )
			{
				string appName=openFileDialog1.FileName;
				pictureBox1.Image=Image.FromFile(appName,true);
				appName=System.IO.Path.GetFileName(appName);
				textBox1.Text = appName;
			}
		}

		private void deletePicture_Click(object sender, System.EventArgs e)
		{
			TreeNode tn = new TreeNode();
			if (treeView.SelectedNode!=null) 
			{
				tn=treeView.SelectedNode;
				if (tn.Text.IndexOf("{",0)>0) 
				{						
					int l=tn.Text.IndexOf(" {",0);
					tn.Text=tn.Text.Substring(0,l);
				} 
			}	
			else 
			{
				MessageBox.Show( "Please select a Menu Item in the Menu Structure!" );
			}
		}

		private void SetupForm_Load(object sender, System.EventArgs e)
		{
		
		}
		#region Drag and Drop TreeView

		private void treeView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.treeView.SelectedNode = this.treeView.GetNodeAt(e.X, e.Y);
		}
		private void treeView_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
		{
			DoDragDrop(e.Item, DragDropEffects.Move);			
		}

		private void treeView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}
		private void treeView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false) && this.NodeMap != "")
			{				
				TreeNode MovingNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
				string[] NodeIndexes = this.NodeMap.Split('|');
				TreeNodeCollection InsertCollection = this.treeView.Nodes;
				for(int i = 0; i < NodeIndexes.Length - 1; i++)
				{
					InsertCollection = InsertCollection[Int32.Parse(NodeIndexes[i])].Nodes;
				}

				if(InsertCollection != null)
				{
					InsertCollection.Insert(Int32.Parse(NodeIndexes[NodeIndexes.Length - 1]), (TreeNode)MovingNode.Clone());
					this.treeView.SelectedNode = InsertCollection[Int32.Parse(NodeIndexes[NodeIndexes.Length - 1])];
					MovingNode.Remove();
				}
			}            
		}

		private void treeView_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			TreeNode NodeOver = this.treeView.GetNodeAt(this.treeView.PointToClient(Cursor.Position));
			TreeNode NodeMoving = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
			if(NodeOver != null && (NodeOver != NodeMoving || (NodeOver.Parent != null && NodeOver.Index == (NodeOver.Parent.Nodes.Count - 1))))
			{
				int OffsetY = this.treeView.PointToClient(Cursor.Position).Y - NodeOver.Bounds.Top;
				int NodeOverImageWidth = this.treeView.ImageList.Images[NodeOver.ImageIndex].Size.Width + 8;
				Graphics g = this.treeView.CreateGraphics();
                
				if(NodeOver.ImageIndex == 1)
				{
					if(OffsetY < (NodeOver.Bounds.Height / 2))
					{
						TreeNode tnParadox = NodeOver;
						while(tnParadox.Parent != null)
						{
							if(tnParadox.Parent == NodeMoving)
							{
								this.NodeMap = "";
								return;
							}

							tnParadox = tnParadox.Parent;
						}
						TreeNode tnPlaceholderInfo = NodeOver;
						string NewNodeMap = ((int)NodeOver.Index).ToString();
						while(tnPlaceholderInfo.Parent != null)
						{
							tnPlaceholderInfo = tnPlaceholderInfo.Parent;
							NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
						}
						if(NewNodeMap == this.NodeMap)
							return;
						else
							this.NodeMap = NewNodeMap;
						this.Refresh();
						int LeftPos, RightPos;
						LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
						RightPos = this.treeView.Width - 4;

						Point[] LeftTriangle = new Point[5]{
																								 new Point(LeftPos, NodeOver.Bounds.Top - 4),
																								 new Point(LeftPos, NodeOver.Bounds.Top + 4),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Y),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Top - 1),
																								 new Point(LeftPos, NodeOver.Bounds.Top - 5)};

						Point[] RightTriangle = new Point[5]{
																									new Point(RightPos, NodeOver.Bounds.Top - 4),
																									new Point(RightPos, NodeOver.Bounds.Top + 4),
																									new Point(RightPos - 4, NodeOver.Bounds.Y),
																									new Point(RightPos - 4, NodeOver.Bounds.Top - 1),
																									new Point(RightPos, NodeOver.Bounds.Top - 5)};


						g.FillPolygon(System.Drawing.Brushes.Black, LeftTriangle);
						g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
						g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Top), new Point(RightPos, NodeOver.Bounds.Top));
					}
					else
					{
						TreeNode tnParadox = NodeOver;
						while(tnParadox.Parent != null)
						{
							if(tnParadox.Parent == NodeMoving)
							{
								this.NodeMap = "";
								return;
							}

							tnParadox = tnParadox.Parent;
						}
						TreeNode ParentDragDrop = null;
						if(NodeOver.Parent != null && NodeOver.Index == (NodeOver.Parent.Nodes.Count - 1))
						{
							int XPos = this.treeView.PointToClient(Cursor.Position).X;
							if(XPos < NodeOver.Bounds.Left)
							{
								ParentDragDrop = NodeOver.Parent;
								while(true)
								{									
									if(XPos > (ParentDragDrop.Bounds.Left - this.treeView.ImageList.Images[ParentDragDrop.ImageIndex].Size.Width))
										break;

									if(ParentDragDrop.Parent != null)
										ParentDragDrop = ParentDragDrop.Parent;
									else
										break;
								}
							}
						}
						TreeNode tnPlaceholderInfo;
						if(ParentDragDrop != null)
							tnPlaceholderInfo = ParentDragDrop; 
						else
							tnPlaceholderInfo = NodeOver;
							
						string NewNodeMap = ((int)tnPlaceholderInfo.Index + 1).ToString();
						while(tnPlaceholderInfo.Parent != null)
						{
							tnPlaceholderInfo = tnPlaceholderInfo.Parent;
							NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
						}
						if(NewNodeMap == this.NodeMap)
							return;
						else
							this.NodeMap = NewNodeMap;
						this.Refresh();
						int LeftPos, RightPos;
						if(ParentDragDrop != null)
							LeftPos = ParentDragDrop.Bounds.Left - (this.treeView.ImageList.Images[ParentDragDrop.ImageIndex].Size.Width + 8);
						else
							LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
						RightPos = this.treeView.Width - 4;

						Point[] LeftTriangle = new Point[5]{
																								 new Point(LeftPos, NodeOver.Bounds.Bottom - 4),
																								 new Point(LeftPos, NodeOver.Bounds.Bottom + 4),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Bottom),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Bottom - 1),
																								 new Point(LeftPos, NodeOver.Bounds.Bottom - 5)};

						Point[] RightTriangle = new Point[5]{
																									new Point(RightPos, NodeOver.Bounds.Bottom - 4),
																									new Point(RightPos, NodeOver.Bounds.Bottom + 4),
																									new Point(RightPos - 4, NodeOver.Bounds.Bottom),
																									new Point(RightPos - 4, NodeOver.Bounds.Bottom - 1),
																									new Point(RightPos, NodeOver.Bounds.Bottom - 5)};


						g.FillPolygon(System.Drawing.Brushes.Black, LeftTriangle);
						g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
						g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Bottom), new Point(RightPos, NodeOver.Bounds.Bottom));
					}
				}
				else
				{
					if(OffsetY < (NodeOver.Bounds.Height / 3))
					{
						TreeNode tnParadox = NodeOver;
						while(tnParadox.Parent != null)
						{
							if(tnParadox.Parent == NodeMoving)
							{
								this.NodeMap = "";
								return;
							}

							tnParadox = tnParadox.Parent;
						}
						TreeNode tnPlaceholderInfo = NodeOver;
						string NewNodeMap = ((int)NodeOver.Index).ToString();
						while(tnPlaceholderInfo.Parent != null)
						{
							tnPlaceholderInfo = tnPlaceholderInfo.Parent;
							NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
						}
						if(NewNodeMap == this.NodeMap)
							return;
						else
							this.NodeMap = NewNodeMap;
						this.Refresh();
						int LeftPos, RightPos;
						LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
						RightPos = this.treeView.Width - 4;

						Point[] LeftTriangle = new Point[5]{
																								 new Point(LeftPos, NodeOver.Bounds.Top - 4),
																								 new Point(LeftPos, NodeOver.Bounds.Top + 4),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Y),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Top - 1),
																								 new Point(LeftPos, NodeOver.Bounds.Top - 5)};

						Point[] RightTriangle = new Point[5]{
																									new Point(RightPos, NodeOver.Bounds.Top - 4),
																									new Point(RightPos, NodeOver.Bounds.Top + 4),
																									new Point(RightPos - 4, NodeOver.Bounds.Y),
																									new Point(RightPos - 4, NodeOver.Bounds.Top - 1),
																									new Point(RightPos, NodeOver.Bounds.Top - 5)};


						g.FillPolygon(System.Drawing.Brushes.Black, LeftTriangle);
						g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
						g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Top), new Point(RightPos, NodeOver.Bounds.Top));
					}
					else if((NodeOver.Parent != null && NodeOver.Index == 0) && (OffsetY > (NodeOver.Bounds.Height - (NodeOver.Bounds.Height / 3))))
					{
						TreeNode tnParadox = NodeOver;
						while(tnParadox.Parent != null)
						{
							if(tnParadox.Parent == NodeMoving)
							{
								this.NodeMap = "";
								return;
							}

							tnParadox = tnParadox.Parent;
						}
						TreeNode tnPlaceholderInfo = NodeOver;
						string NewNodeMap = ((int)NodeOver.Index + 1).ToString();
						while(tnPlaceholderInfo.Parent != null)
						{
							tnPlaceholderInfo = tnPlaceholderInfo.Parent;
							NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
						}
						if(NewNodeMap == this.NodeMap)
							return;
						else
							this.NodeMap = NewNodeMap;
						this.Refresh();
						int LeftPos, RightPos;
						LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
						RightPos = this.treeView.Width - 4;

						Point[] LeftTriangle = new Point[5]{
																								 new Point(LeftPos, NodeOver.Bounds.Bottom - 4),
																								 new Point(LeftPos, NodeOver.Bounds.Bottom + 4),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Bottom),
																								 new Point(LeftPos + 4, NodeOver.Bounds.Bottom - 1),
																								 new Point(LeftPos, NodeOver.Bounds.Bottom - 5)};

						Point[] RightTriangle = new Point[5]{
																									new Point(RightPos, NodeOver.Bounds.Bottom - 4),
																									new Point(RightPos, NodeOver.Bounds.Bottom + 4),
																									new Point(RightPos - 4, NodeOver.Bounds.Bottom),
																									new Point(RightPos - 4, NodeOver.Bounds.Bottom - 1),
																									new Point(RightPos, NodeOver.Bounds.Bottom - 5)};


						g.FillPolygon(System.Drawing.Brushes.Black, LeftTriangle);
						g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
						g.DrawLine(new System.Drawing.Pen(Color.Black, 2), new Point(LeftPos, NodeOver.Bounds.Bottom), new Point(RightPos, NodeOver.Bounds.Bottom));
					}
					else
					{
						if(NodeOver.Nodes.Count > 0)
						{
							NodeOver.Expand();
						}
						else
						{
							if(NodeMoving == NodeOver)
								return;
							TreeNode tnParadox = NodeOver;
							while(tnParadox.Parent != null)
							{
								if(tnParadox.Parent == NodeMoving)
								{
									this.NodeMap = "";
									return;
								}
								tnParadox = tnParadox.Parent;
							}
							TreeNode tnPlaceholderInfo = NodeOver;
							string NewNodeMap = ((int)NodeOver.Index).ToString();
							while(tnPlaceholderInfo.Parent != null)
							{
								tnPlaceholderInfo = tnPlaceholderInfo.Parent;
								NewNodeMap = tnPlaceholderInfo.Index + "|" + NewNodeMap;
							}
							NewNodeMap = NewNodeMap + "|0";
							if(NewNodeMap == this.NodeMap)
								return;
							else
								this.NodeMap = NewNodeMap;
							this.Refresh();
							int RightPos = NodeOver.Bounds.Right + 6;
							Point[] RightTriangle = new Point[5]{
																										new Point(RightPos, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) + 4),
																										new Point(RightPos, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) + 4),
																										new Point(RightPos - 4, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2)),
																										new Point(RightPos - 4, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) - 1),
																										new Point(RightPos, NodeOver.Bounds.Y + (NodeOver.Bounds.Height / 2) - 5)};

							this.Refresh();
							g.FillPolygon(System.Drawing.Brushes.Black, RightTriangle);
						}
					}
				}
			}
		}
		#endregion
	
	}
}
