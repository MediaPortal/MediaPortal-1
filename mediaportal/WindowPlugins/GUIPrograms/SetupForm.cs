using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using MediaPortal.GUI.Library;
using ProgramsDatabase;
using Programs.Utils;

namespace WindowPlugins.GUIPrograms
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
	public class SetupForm : System.Windows.Forms.Form, ISetupForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TreeView appTree;

		private Applist apps = ProgramDatabase.AppList;
		// create setting-tabs
		private AppSettingsDirBrowse sectionDirBrowse = new AppSettingsDirBrowse();
		private AppSettingsDirCache sectionDirCache = new AppSettingsDirCache();
		private AppSettingsMyFileIni sectionMyFileIni = new AppSettingsMyFileIni();
		private AppSettingsMyFileMeedio sectionMyFileMeedio = new AppSettingsMyFileMeedio();
		private AppSettingsFilelauncher sectionFilelauncher = new AppSettingsFilelauncher();
		private AppSettingsGrouper sectionGrouper = new AppSettingsGrouper();
		private AppSettingsRoot sectionRoot = new AppSettingsRoot();
		private System.Windows.Forms.GroupBox gbDetails;
		private System.Windows.Forms.Panel holderPanel;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.StatusBarPanel StatusPanel;
		private System.Windows.Forms.StatusBarPanel ActionPanel;
		private System.Windows.Forms.ToolBar toolBarMenu;
		private System.Windows.Forms.ToolBarButton buttonAddChild;
		private System.Windows.Forms.ToolBarButton buttonDelete;
		private System.Windows.Forms.ToolBarButton buttonUp;
		private System.Windows.Forms.ToolBarButton buttonDown;
		private System.Windows.Forms.ContextMenu popupAddChild;
		private System.Windows.Forms.ToolBarButton sep1;
		private System.Windows.Forms.ToolBarButton sep2;
		private System.Windows.Forms.ToolBarButton sep3;
		private System.Windows.Forms.ToolBarButton sep4;
		private System.Windows.Forms.MenuItem menuDirBrowse;
		private System.Windows.Forms.MenuItem menuDirCache;
		private System.Windows.Forms.MenuItem menuMyFile;
		private System.Windows.Forms.MenuItem menuMLFFile;
		private System.Windows.Forms.MenuItem menuFileLauncher;
		private System.Windows.Forms.MenuItem menuGrouper;
		// pointer to currently displayed sheet
		private AppSettings pageCurrentSettings = null;

		public SetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

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

		public bool	CanEnable()		// Indicates whether plugin can be enabled/disabled
		{
			return true;
		}
		
		public bool HasSetup()
		{
			return true;
		}
		public int GetWindowId()
		{
			return (int)GUIWindow.Window.WINDOW_FILES;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText=GUILocalizeStrings.Get(0);;
			strButtonImage="";
			strButtonImageFocus="";
			strPictureImage="";
			return true;
		}

		public string PluginName() 
		{
			return "My Programs";
		}
		public string Description()
		{
			return "A Program Launching Plugin";
		}
		public string Author()
		{
			return "waeberd/Domi_Fan";
		}
		public void ShowPlugin()
		{
			ShowDialog();
		}
		public bool DefaultEnabled()
		{
			return false;
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.appTree = new System.Windows.Forms.TreeView();
			this.gbDetails = new System.Windows.Forms.GroupBox();
			this.holderPanel = new System.Windows.Forms.Panel();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.StatusPanel = new System.Windows.Forms.StatusBarPanel();
			this.ActionPanel = new System.Windows.Forms.StatusBarPanel();
			this.toolBarMenu = new System.Windows.Forms.ToolBar();
			this.buttonAddChild = new System.Windows.Forms.ToolBarButton();
			this.popupAddChild = new System.Windows.Forms.ContextMenu();
			this.menuDirBrowse = new System.Windows.Forms.MenuItem();
			this.menuDirCache = new System.Windows.Forms.MenuItem();
			this.menuMyFile = new System.Windows.Forms.MenuItem();
			this.menuMLFFile = new System.Windows.Forms.MenuItem();
			this.menuFileLauncher = new System.Windows.Forms.MenuItem();
			this.menuGrouper = new System.Windows.Forms.MenuItem();
			this.sep1 = new System.Windows.Forms.ToolBarButton();
			this.buttonDelete = new System.Windows.Forms.ToolBarButton();
			this.sep2 = new System.Windows.Forms.ToolBarButton();
			this.buttonUp = new System.Windows.Forms.ToolBarButton();
			this.sep3 = new System.Windows.Forms.ToolBarButton();
			this.buttonDown = new System.Windows.Forms.ToolBarButton();
			this.sep4 = new System.Windows.Forms.ToolBarButton();
			this.gbDetails.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StatusPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ActionPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// appTree
			// 
			this.appTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.appTree.HideSelection = false;
			this.appTree.ImageIndex = -1;
			this.appTree.Indent = 19;
			this.appTree.ItemHeight = 16;
			this.appTree.LabelEdit = true;
			this.appTree.Location = new System.Drawing.Point(8, 40);
			this.appTree.Name = "appTree";
			this.appTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
																				new System.Windows.Forms.TreeNode("my Programs", new System.Windows.Forms.TreeNode[] {
																																										 new System.Windows.Forms.TreeNode("whazzz up")})});
			this.appTree.SelectedImageIndex = -1;
			this.appTree.Size = new System.Drawing.Size(224, 448);
			this.appTree.TabIndex = 8;
			this.appTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
			this.appTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.appTree_BeforeSelect);
			this.appTree.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.appTree_AfterLabelEdit);
			this.appTree.DragEnter += new System.Windows.Forms.DragEventHandler(this.appTree_DragEnter);
			this.appTree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.appTree_ItemDrag);
			this.appTree.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.appTree_BeforeLabelEdit);
			this.appTree.DragDrop += new System.Windows.Forms.DragEventHandler(this.appTree_DragDrop);
			// 
			// gbDetails
			// 
			this.gbDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.gbDetails.Controls.Add(this.holderPanel);
			this.gbDetails.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.gbDetails.Location = new System.Drawing.Point(240, 40);
			this.gbDetails.Name = "gbDetails";
			this.gbDetails.Size = new System.Drawing.Size(424, 448);
			this.gbDetails.TabIndex = 11;
			this.gbDetails.TabStop = false;
			this.gbDetails.Text = "Details";
			// 
			// holderPanel
			// 
			this.holderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.holderPanel.Location = new System.Drawing.Point(4, 16);
			this.holderPanel.Name = "holderPanel";
			this.holderPanel.Size = new System.Drawing.Size(412, 428);
			this.holderPanel.TabIndex = 11;
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 494);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						  this.StatusPanel,
																						  this.ActionPanel});
			this.statusBar1.ShowPanels = true;
			this.statusBar1.Size = new System.Drawing.Size(666, 24);
			this.statusBar1.SizingGrip = false;
			this.statusBar1.TabIndex = 12;
			// 
			// StatusPanel
			// 
			this.StatusPanel.Text = "Ready";
			// 
			// ActionPanel
			// 
			this.ActionPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.ActionPanel.Width = 566;
			// 
			// toolBarMenu
			// 
			this.toolBarMenu.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBarMenu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.toolBarMenu.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						   this.buttonAddChild,
																						   this.sep1,
																						   this.buttonDelete,
																						   this.sep2,
																						   this.buttonUp,
																						   this.sep3,
																						   this.buttonDown,
																						   this.sep4});
			this.toolBarMenu.ButtonSize = new System.Drawing.Size(60, 36);
			this.toolBarMenu.Divider = false;
			this.toolBarMenu.DropDownArrows = true;
			this.toolBarMenu.Location = new System.Drawing.Point(0, 0);
			this.toolBarMenu.Name = "toolBarMenu";
			this.toolBarMenu.ShowToolTips = true;
			this.toolBarMenu.Size = new System.Drawing.Size(666, 27);
			this.toolBarMenu.TabIndex = 13;
			this.toolBarMenu.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBarMenu.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBarMenu_ButtonClick);
			// 
			// buttonAddChild
			// 
			this.buttonAddChild.DropDownMenu = this.popupAddChild;
			this.buttonAddChild.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			this.buttonAddChild.Text = "&Add Child";
			// 
			// popupAddChild
			// 
			this.popupAddChild.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						  this.menuDirBrowse,
																						  this.menuDirCache,
																						  this.menuMyFile,
																						  this.menuMLFFile,
																						  this.menuFileLauncher,
																						  this.menuGrouper});
			// 
			// menuDirBrowse
			// 
			this.menuDirBrowse.Index = 0;
			this.menuDirBrowse.Text = "Directory-Browse";
			this.menuDirBrowse.Click += new System.EventHandler(this.menuDirBrowse_Click);
			// 
			// menuDirCache
			// 
			this.menuDirCache.Index = 1;
			this.menuDirCache.Text = "Directory-Cache";
			this.menuDirCache.Click += new System.EventHandler(this.menuDirCache_Click);
			// 
			// menuMyFile
			// 
			this.menuMyFile.Index = 2;
			this.menuMyFile.Text = "*.my File Importer";
			this.menuMyFile.Click += new System.EventHandler(this.menuMyFile_Click);
			// 
			// menuMLFFile
			// 
			this.menuMLFFile.Index = 3;
			this.menuMLFFile.Text = "*.mlf File Importer";
			this.menuMLFFile.Click += new System.EventHandler(this.menuMLFFile_Click);
			// 
			// menuFileLauncher
			// 
			this.menuFileLauncher.Index = 4;
			this.menuFileLauncher.Text = "Filelauncher";
			this.menuFileLauncher.Click += new System.EventHandler(this.menuFileLauncher_Click);
			// 
			// menuGrouper
			// 
			this.menuGrouper.Enabled = false;
			this.menuGrouper.Index = 5;
			this.menuGrouper.Text = "Grouper";
			this.menuGrouper.Click += new System.EventHandler(this.menuGrouper_Click);
			// 
			// sep1
			// 
			this.sep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// buttonDelete
			// 
			this.buttonDelete.Text = "Delete...";
			// 
			// sep2
			// 
			this.sep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// buttonUp
			// 
			this.buttonUp.Text = "&Up";
			// 
			// sep3
			// 
			this.sep3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// buttonDown
			// 
			this.buttonDown.Text = "&Down";
			// 
			// sep4
			// 
			this.sep4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// SetupForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(666, 518);
			this.Controls.Add(this.toolBarMenu);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.gbDetails);
			this.Controls.Add(this.appTree);
			this.Name = "SetupForm";
			this.Text = "my Programs Setup";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.SetupForm_Closing);
			this.Load += new System.EventHandler(this.SetupForm_Load);
			this.gbDetails.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.StatusPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ActionPanel)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void DoAddChild(myProgSourceType newSourceType)
		{
			AppItem newApp = new AppItem(ProgramDatabase.m_db);
			apps.Add(newApp);
			newApp.Position = apps.GetMaxPosition() + 10;
			newApp.Title = "New item";
			newApp.SourceType = newSourceType;
			newApp.Write();
			int newAppID = newApp.AppID;
			apps.LoadAll();
			updateTree();
			appTree.SelectedNode = appTree.Nodes[0].Nodes[appTree.Nodes[0].Nodes.Count - 1];
		}

		private void SelectNodeOfAppID(int appID)
		{
			// todo:
		}

		private TreeNode GetNodeOfApp(AppItem TargetApp)
		{
			TreeNode res = null;
			foreach(TreeNode node in appTree.Nodes)
			{
				if ((AppItem)(node.Tag) == TargetApp)
				{
					res = node;
					break;
				}
			}
			return res;
		}

		private string GetSelectedIndexPath()
		{
			string res = "";
			string sep = "";
			TreeNode curNode = appTree.SelectedNode;
			while (curNode != null)
			{
				res = String.Format("{0}", curNode.Index) + sep + res;
				sep = ";";
				curNode = curNode.Parent;
			}
			return res;
		}

		private TreeNode GetNodeOfIndexPath(string IndexPath)
		{
			TreeNode res = null;
			int nIndex = -1;
			ArrayList Indexes = new ArrayList( IndexPath.Split( ';' ) );
			foreach (string strIndex in Indexes)
			{
				if (res == null) 
				{
					// first entry => always select root node and go on iterating
					res = appTree.Nodes[0];
				}
				else
				{
					nIndex = ProgramUtils.StrToIntDef(strIndex, -1);
					if ((nIndex >= 0) && (nIndex <= res.Nodes.Count - 1))
					{
						res = res.Nodes[nIndex];
					}
					else
					{
						// problem: return null and exit!
						res = null;
						break;
					}
				}
			}
			return res;
		}
	

		private void updateTree()
		{
			string IndexPath = GetSelectedIndexPath();
			appTree.BeginUpdate();
			appTree.Nodes.Clear();
			appTree.Nodes.Add(new TreeNode("my Programs"));
			foreach( AppItem app in apps )
			{
				TreeNode curNode = new TreeNode(app.Title);
				curNode.Tag = app;
				appTree.Nodes[0].Nodes.Add(curNode);
			}
			appTree.ExpandAll();
			appTree.EndUpdate();
			TreeNode newFocus = GetNodeOfIndexPath(IndexPath);
			if (newFocus != null)
			{
				appTree.SelectedNode = newFocus;
			}
			else
			{
				appTree.SelectedNode = appTree.Nodes[0];
			}

	}

		private void btnDelete_Click(object sender, System.EventArgs e)
		{
			DoDelete();
		}

		private void DoDelete()
		{
			AppItem app = GetSelectedAppNode();
			if (app != null)
			{
				DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this application item?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if(dialogResult == DialogResult.Yes)
				{
					app.Delete();
					apps.LoadAll();
					updateTree();
				}
			}
		}

		private AppItem GetPrevAppNode()
		{
			AppItem res = null;
			if (appTree.SelectedNode != null)
			{
				if (appTree.SelectedNode.PrevNode != null)
				{
					if (appTree.SelectedNode.PrevNode.Tag != null)
					{
						res = (AppItem)appTree.SelectedNode.PrevNode.Tag;
					}
				}
			}
			return res;
		}

		private AppItem GetNextAppNode()
		{
			AppItem res = null;
			if (appTree.SelectedNode != null)
			{
				if (appTree.SelectedNode.NextNode != null)
				{
					if (appTree.SelectedNode.NextNode.Tag != null)
					{
						res = (AppItem)appTree.SelectedNode.NextNode.Tag;
					}
				}
			}
			return res;
		}

		private AppItem GetSelectedAppNode()
		{
			AppItem res = null;
			if (appTree.SelectedNode != null)
			{
				if (appTree.SelectedNode.Tag != null)
				{
					res = (AppItem)appTree.SelectedNode.Tag;
				}
			}
			return res;
		}


		private void SetupForm_Load(object sender, System.EventArgs e)
		{
			apps.LoadAll(); // we need all apps, whether enabled or not => reload with LoadALL
			updateTree();
		}

		private void DoUp()
		{
			AppItem curApp = GetSelectedAppNode();
			AppItem prevApp = GetPrevAppNode();
			int n = -1;
			if ((curApp != null) && (prevApp != null))
			{
				n = curApp.Position;
				curApp.Position = prevApp.Position;
				prevApp.Position = n;
				curApp.Write();
				prevApp.Write();
				apps.LoadAll(); // poor man's refresh.... Load all and display all.....
				updateTree();
				if (appTree.SelectedNode.PrevNode != null)
				{
					appTree.SelectedNode = appTree.SelectedNode.PrevNode;
				}
			}
		}


		private void DoDown()
		{
			AppItem curApp = GetSelectedAppNode();
			AppItem nextApp = GetNextAppNode();
			int n = -1;
			if ((curApp != null) && (nextApp != null))
			{
				n = curApp.Position;
				curApp.Position = nextApp.Position;
				nextApp.Position = n;
				curApp.Write();
				nextApp.Write();
				apps.LoadAll(); // poor man's refresh.... Load all and display all.....
				updateTree();
				if (appTree.SelectedNode.NextNode != null)
				{
					appTree.SelectedNode = appTree.SelectedNode.NextNode;
				}
			}
		}


		private void ShowFiles()
		{
			AppItem app = GetSelectedAppNode();
			if (app != null)
			{
				if (app.FileEditorAllowed())
				{
					FileEditor frmFiles = new FileEditor();
					frmFiles.CurApp = app;
					frmFiles.ShowDialog( this );
				}
				else
				{
					System.Windows.Forms.MessageBox.Show("File-editing is not possible for this application (wrong mode)!");
				}
			}
		}

		private void btnFiles_Click(object sender, System.EventArgs e)
		{
			ShowFiles();
		}

		private void FileEditClick(object sender, System.EventArgs e)
		{
			ShowFiles();
		}

		private void RefreshClick(object sender, System.EventArgs e)
		{
			DoRefresh();
		}

		private void UpClick(object sender, System.EventArgs e)
		{
			DoUp();
		}

		private void DownClick(object sender, System.EventArgs e)
		{
			DoDown();
		}



		private void RefreshInfo(string Message)
		{
			ActionPanel.Text = Message;
		}

		private void DoRefresh()
		{
			AppItem curApp = GetSelectedAppNode();
			if (curApp != null) 
			{
				StatusPanel.Text = "Import running....";
				curApp.OnRefreshInfo += new AppItem.RefreshInfoEventHandler(RefreshInfo);
				try
				{
					curApp.Refresh(false);
				}
				finally
				{
					curApp.OnRefreshInfo += new AppItem.RefreshInfoEventHandler(RefreshInfo);
					StatusPanel.Text = "Ready";
					ActionPanel.Text = "";
				}
			}
		}

		private AppSettings GetCurrentSettingsPage()
		{
			AppSettings res = null;
			AppItem curApp = GetSelectedAppNode();
			if (curApp != null) 
			{
				switch (curApp.SourceType)
				{
					case myProgSourceType.DIRBROWSE:
						res = sectionDirBrowse;
						break;
					case myProgSourceType.DIRCACHE:
						res = sectionDirCache;
						break;
					case myProgSourceType.MYFILEINI:
						res = sectionMyFileIni;
						break;
					case myProgSourceType.MYFILEMEEDIO:
						res = sectionMyFileMeedio;
						break;
					case myProgSourceType.MYGAMESDIRECT:
						res = null; // todo:
						break;
					case myProgSourceType.FILELAUNCHER:
						res = sectionFilelauncher;
						break;
//					case myProgSourceType.GROUPER:
//						res = sectionGrouper;
//						break;
				}
			}
			else
			{
				res = sectionRoot;
			}
		return res;
		}

		private void SyncPanel(AppSettings pageSettings)
		{
			holderPanel.Controls.Clear();
			if (pageSettings != null)
			{
				holderPanel.Controls.Add(pageSettings);
				AppItem curApp = GetSelectedAppNode();
				if (curApp != null) 
				{
					pageSettings.AppObj2Form(curApp);
				}
				else
				{
//					pageSettings.Applist2Form(apps);
				}
				pageSettings.SetBounds(0, 0, holderPanel.Width, holderPanel.Height);
			}
		}


		private void appTree_AfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
		{
			if (e.Label != null)
			{
				if (e.Node.Tag != null)
				{
					e.Node.EndEdit(false);
					AppItem curApp = (AppItem)e.Node.Tag;
					curApp.Title = e.Label;
					curApp.Write();
				}
				else
				{
					e.CancelEdit = true;
				}
			}
		}

		private void appTree_BeforeLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
		{
			// disallow edit for root node.....
			if (e.Node.Tag == null)
			{
				e.CancelEdit = true;
			}
		}

		private void appTree_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			AppItem curApp = this.GetSelectedAppNode();
			pageCurrentSettings = GetCurrentSettingsPage();
			if ((curApp != null) && (pageCurrentSettings != null))
			{
				if (pageCurrentSettings.EntriesOK(curApp))
				{
					pageCurrentSettings.OnFileEditClick -= new System.EventHandler(this.FileEditClick);
					pageCurrentSettings.OnRefreshClick -= new System.EventHandler(this.RefreshClick);
					pageCurrentSettings.OnUpClick -= new System.EventHandler(this.UpClick);
					pageCurrentSettings.OnDownClick -= new System.EventHandler(this.DownClick);
					pageCurrentSettings.Form2AppObj(curApp);
					curApp.Write();
				}
				else 
				{
					e.Cancel = true;
				}
			}
		}


		private void sectionTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			pageCurrentSettings = GetCurrentSettingsPage();
			if (pageCurrentSettings != null)
			{
				pageCurrentSettings.OnFileEditClick += new System.EventHandler(this.FileEditClick);
				pageCurrentSettings.OnRefreshClick += new System.EventHandler(this.RefreshClick);
				pageCurrentSettings.OnUpClick += new System.EventHandler(this.UpClick);
				pageCurrentSettings.OnDownClick += new System.EventHandler(this.DownClick);
				SyncPanel(pageCurrentSettings);
			}
			SyncButtons();
		}

		private void SyncButtons()
		{
			AppItem curApp = this.GetSelectedAppNode();
			if ((curApp == null) && (appTree.SelectedNode == appTree.Nodes[0]))
			{
				// select buttons for root node
				
				buttonAddChild.Enabled = true;
				buttonDelete.Enabled = false;
				buttonUp.Enabled = false;
				buttonDown.Enabled = false;
			}
			else if (curApp != null)
			{
				// select buttons for app node
				buttonAddChild.Enabled = false;
				buttonDelete.Enabled = true;
				buttonUp.Enabled = (GetPrevAppNode() != null);
				buttonDown.Enabled = (GetNextAppNode() != null);
			}

		}

		private void appTree_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			TreeNode NewNode;

			if(e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
			{
				Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
				TreeNode DestinationNode = ((TreeView)sender).GetNodeAt(pt);
				NewNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
				DestinationNode.Nodes.Add((TreeNode) NewNode.Clone());
				DestinationNode.Expand();
				//Remove Original Node
				NewNode.Remove();
			}
		}

		private void appTree_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move; 
		}

		private void appTree_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
		{
			DoDragDrop(e.Item, DragDropEffects.Move); 		
		}

		private void toolBarMenu_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(toolBarMenu.Buttons.IndexOf(e.Button))
			{
				case 0:
					//DoAddChild();
					popupAddChild.Show(toolBarMenu, new System.Drawing.Point(0, toolBarMenu.Height));
					break; 
				case 2:
					DoDelete();
					break; 
				case 4:
					DoUp();
					break; 
				case 6:
					DoDown();
					break; 
			}
		}


		private void menuDirBrowse_Click(object sender, System.EventArgs e)
		{
			DoAddChild(myProgSourceType.DIRBROWSE);
		}

		private void menuDirCache_Click(object sender, System.EventArgs e)
		{
			DoAddChild(myProgSourceType.DIRCACHE);
		}

		private void menuMyFile_Click(object sender, System.EventArgs e)
		{
			DoAddChild(myProgSourceType.MYFILEINI);
		}

		private void menuMLFFile_Click(object sender, System.EventArgs e)
		{
			DoAddChild(myProgSourceType.MYFILEMEEDIO);
		}

		private void menuFileLauncher_Click(object sender, System.EventArgs e)
		{
			DoAddChild(myProgSourceType.FILELAUNCHER);
		}

		private void menuGrouper_Click(object sender, System.EventArgs e)
		{
			// todo:
		//	DoAddChild(myProgSourceType.GROUPER);
		}

		private void SetupForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			AppItem curApp = this.GetSelectedAppNode();
			pageCurrentSettings = GetCurrentSettingsPage();
			if ((curApp != null) && (pageCurrentSettings != null))
			{
				if (pageCurrentSettings.EntriesOK(curApp))
				{
					pageCurrentSettings.OnFileEditClick -= new System.EventHandler(this.FileEditClick);
					pageCurrentSettings.OnRefreshClick -= new System.EventHandler(this.RefreshClick);
					pageCurrentSettings.OnUpClick -= new System.EventHandler(this.UpClick);
					pageCurrentSettings.OnDownClick -= new System.EventHandler(this.DownClick);
					pageCurrentSettings.Form2AppObj(curApp);
					curApp.Write();
				}
				else 
				{
					e.Cancel = true;
				}
			}
		}

	}
}
