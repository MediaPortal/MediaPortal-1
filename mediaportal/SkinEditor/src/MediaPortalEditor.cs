using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Threading;
using System.Reflection;
using Crownwood.Magic.Common;
using Crownwood.Magic.Controls;
using Crownwood.Magic.Docking;
using Crownwood.Magic.Menus;

using Mpe.Controls;
using Mpe.Designers;
using Mpe.Forms;

namespace Mpe {
	/// <summary>
	/// Summary description for SkinEditorForm
	/// </summary>
	public class MediaPortalEditor : System.Windows.Forms.Form, MpeGlobal {

		#region Variables
		private VisualStyle style;
		private DockingManager dockManager;
		private Crownwood.Magic.Menus.MenuControl topMenu;
		private Crownwood.Magic.Menus.MenuCommand fileMenu;
		private Crownwood.Magic.Menus.MenuCommand editMenu;
		private Crownwood.Magic.Menus.MenuCommand viewMenu;
		private Crownwood.Magic.Menus.MenuCommand helpMenu;
		private Crownwood.Magic.Controls.TabControl tabManager;
		
		private System.Windows.Forms.ImageList menuImageList;
		private System.Windows.Forms.Timer initTimer;
		private System.Windows.Forms.ImageList serviceImageList;
		private System.Windows.Forms.ImageList tabImageList;
		private System.Windows.Forms.ImageList statusBarIcons;
		private System.ComponentModel.IContainer components;
		
		private MpePropertyManager propertyManager;
		private MpeHelpManager helpManager;
		private MpeHelpBrowser helpBrowser;
		private MpeExplorer skinExplorer;
		private MpeStatusBar statusBar;
		
		private MpeParser skinParser;
		private int selectedDesignerIndex;
		private object clipboard;
		private MpePreferences preferences;

		private bool cancelCommand;

		private static MediaPortalEditor self;
		#endregion

		#region Contructors
		public MediaPortalEditor() {
			InitializeComponent();
			Text = "MediaPortalEditor";

			// Initialize style options
			style = VisualStyle.IDE;
			SetStyle(ControlStyles.DoubleBuffer,true);
			SetStyle(ControlStyles.AllPaintingInWmPaint,true);

			// Create the docking and tab manager
			tabManager = new Crownwood.Magic.Controls.TabControl();
			tabManager.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiDocument;
			tabManager.ClosePressed += new EventHandler(OnDesignerClosed);
			tabManager.SelectionChanged += new EventHandler(OnDesignerIndexChanged);
			tabManager.Dock = DockStyle.Fill;
			tabManager.Style = style;
			tabManager.IDEPixelBorder = true;
			tabManager.ShowClose = false;
			tabManager.ImageList = tabImageList;
			Controls.Add(tabManager);

			dockManager = new DockingManager(this, style);
			dockManager.InnerControl = tabManager;
			dockManager.TabControlCreated += new DockingManager.TabControlCreatedHandler(OnTabControlCreated);
			dockManager.ContentHidden += new DockingManager.ContentHandler(OnContentHidden);
			dockManager.ContentShown += new DockingManager.ContentHandler(OnContentShown);

			// Create Status Bar
			statusBar = CreateStatusBar();
			dockManager.OuterControl = statusBar;

			// Create Menu and Toolbar
			topMenu = CreateTopMenu();

			// Create Skin Tree
			skinExplorer = new MpeExplorer(this);
			Content c = dockManager.Contents.Add(skinExplorer, "Explorer", serviceImageList, 0);
			dockManager.AddContentWithState(c, State.DockLeft);

			// Create Properties
			propertyManager = new MpePropertyManager(this);
			c = dockManager.Contents.Add(propertyManager, "Properties", serviceImageList, 1);
			dockManager.AddContentWithState(c, State.DockRight);

			// Create Help Manager and Browser
			helpManager = new MpeHelpManager(this);
			c = dockManager.Contents.Add(helpManager, "Help", serviceImageList, 2);
			dockManager.AddContentWithState(c, State.DockBottom);
			helpBrowser = new MpeHelpBrowser(this);

			// Setup Tab Designers
			selectedDesignerIndex = -1;

			// Static Self Reference
			self = this;
		}
		#endregion

		#region Properties
		public static MpeGlobal Global {
			get {
				return self;
			}
		}
		public object Clipboard {
			get {
				return clipboard;
			}
			set {
				clipboard = value;
			}
		}
		public MpePropertyManager PropertyManager {
			get {
				return propertyManager;
			}
		}
		public MpeStatusBar StatusBar {
			get {
				return statusBar;
			}
		}
		public MpeExplorer Explorer {
			get {
				return skinExplorer;
			}
		}
		public MpeParser Parser {
			get {
				return skinParser;
			}
			set {
				skinParser = value;
			}
		}
		public MpePreferences Preferences {
			get {
				return preferences;
			}
		}
		#endregion

		#region Methods
		protected MpeStatusBar CreateStatusBar() {
			// Convert the statusBarIcons (ImageList) into an Array of Images
			MpeStatusBar statusbar = new MpeStatusBar(statusBarIcons);
			statusbar.Dock = DockStyle.Bottom;
			Controls.Add(statusbar);			
			return statusbar;
		}
		protected Crownwood.Magic.Menus.MenuControl CreateTopMenu() {
			Crownwood.Magic.Menus.MenuControl menu = new Crownwood.Magic.Menus.MenuControl();
			menu.Style = style;
			menu.MultiLine = false;
		
			fileMenu = new MenuCommand("&File");
			editMenu = new MenuCommand("&Edit");
			helpMenu = new MenuCommand("&Help");
			viewMenu = new MenuCommand("&View");
			menu.MenuCommands.AddRange(new MenuCommand[]{ fileMenu, editMenu, viewMenu, helpMenu });
			// File
			MenuCommand newMenu = new MenuCommand("&New...", Shortcut.CtrlN, new EventHandler(OnMenuNew));
			MenuCommand openMenu = new MenuCommand("&Open...", menuImageList, 0, Shortcut.CtrlO, new EventHandler(OnMenuOpen));
			MenuCommand closeMenu = new MenuCommand("&Close", new EventHandler(OnMenuClose));
			closeMenu.Enabled = false;
			MenuCommand exitMenu = new MenuCommand("E&xit", new EventHandler(OnMenuExit));
			MenuCommand saveMenu = new MenuCommand("&Save", menuImageList, 1, Shortcut.CtrlS, new EventHandler(OnMenuSave));
			saveMenu.Enabled = false;
			MenuCommand saveAllMenu = new MenuCommand("Save A&ll", menuImageList, 2, Shortcut.CtrlShiftS, new EventHandler(OnMenuSaveAll));
			saveAllMenu.Enabled = false;
			fileMenu.MenuCommands.AddRange(new MenuCommand[] { newMenu, openMenu, new MenuCommand("-"), saveMenu, saveAllMenu, new MenuCommand("-"), closeMenu, new MenuCommand("-"), exitMenu });

			// Edit
			MenuCommand cutMenu = new MenuCommand("Cu&t", Shortcut.CtrlX, new EventHandler(OnMenuCut));
			MenuCommand copyMenu = new MenuCommand("&Copy", Shortcut.CtrlC, new EventHandler(OnMenuCopy));
			MenuCommand pasteMenu = new MenuCommand("&Paste", Shortcut.CtrlV, new EventHandler(OnMenuPaste));
			MenuCommand preferencesMenu = new MenuCommand("&Preferences", new EventHandler(OnMenuPreferences));
			editMenu.MenuCommands.AddRange(new MenuCommand[] { cutMenu, copyMenu, pasteMenu, new MenuCommand("-"), preferencesMenu });

			// View
			MenuCommand skinExplorerMenu = new MenuCommand("&Explorer", menuImageList, 4, Shortcut.F3, new EventHandler(OnMenuView));
			skinExplorerMenu.Tag = "Explorer";
			MenuCommand propertiesMenu = new MenuCommand("&Properties", menuImageList, 3, Shortcut.F4, new EventHandler(OnMenuView));
			propertiesMenu.Tag = "Properties";
			viewMenu.MenuCommands.AddRange(new MenuCommand[] { skinExplorerMenu, propertiesMenu });

			// Help
			MenuCommand aboutMenu = new MenuCommand("&About", new EventHandler(OnMenuAbout));
			MenuCommand helpCommand = new MenuCommand("&Help", menuImageList, 5, new EventHandler(OnMenuView));
			helpCommand.Tag = "Help";
			helpMenu.MenuCommands.AddRange(new MenuCommand[] { helpCommand, new MenuCommand("-"), aboutMenu });

			menu.Dock = DockStyle.Top;
			Controls.Add(menu);

			return menu;
		}

		protected void SaveConfiguration() {
			StatusBar.Info("Saving configuration...");
			dockManager.SaveConfigToFile("config.xml");
		}
		protected void LoadConfiguration() {
			StatusBar.Info("Loading configuration...");
			if (File.Exists("config.xml")) {
				try {
					dockManager.LoadConfigFromFile("config.xml");
				} catch (Exception ee) {
					MpeLog.Debug(ee);
					MpeLog.Warn(ee);
				}
			}
		}
		protected void SavePreferences() {
			if (preferences != null) {
				preferences.WindowPosition = Location;
				preferences.WindowSize = Size;
				preferences.WindowState = WindowState;
				preferences.Save();
			}
		}
		protected void LoadPreferences() {
			if (preferences != null) {
				preferences.Load();
				WindowState = preferences.WindowState;
				Location = preferences.WindowPosition;
				Size = preferences.WindowSize;
				MpeLog.Threshold = preferences.LogLevel;
				Global.StatusBar.Refresh();
			}
		}
		protected void OpenSkin(DirectoryInfo skinDir) {
			try {
				Cursor = Cursors.WaitCursor;
				if (skinExplorer.IsSkinLoaded) {
					cancelCommand = false;
					OnMenuClose(null,null);
					if (cancelCommand)
						return;
				}
				skinExplorer.LoadSkin(skinDir);
				fileMenu.MenuCommands["&Close"].Enabled = true;
				Text = "MediaPortalEditor - Skin - [" + skinDir.Name + "]";
			} catch (MpeExplorerException ee) {
				MpeLog.Debug(ee);
				MpeLog.Error(ee);
			} finally {
				Cursor = Cursors.Default;
			}	
		}
		/// <summary>
		/// This method returns an array of designers whose resources have been modifeid.
		/// </summary>
		/// <returns>Array of Designers whose resources have been modified</returns>
		protected MpeDesigner[] GetModifiedDesigners() {
			if (tabManager.TabPages.Count == 0)
				return new MpeDesigner[0];
			ArrayList list = new ArrayList();
			for (int i = 0; i < tabManager.TabPages.Count; i++) {
				if (tabManager.TabPages[i].Title.EndsWith("*")) {
					list.Add(tabManager.TabPages[i].Tag);
				}
			}
			return (MpeDesigner[])list.ToArray(typeof(MpeDesigner));
		}
		public void AddDesigner(MpeDesigner designer) {
			try {
				designer.Initialize();
			} catch (DesignerException ee) {
				MpeLog.Debug(ee);
				MpeLog.Warn(ee);
				PropertyManager.SelectedResource = null;
				MessageBox.Show(this,ee.Message,"Designer Initialization Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
				return;
			}
			if (tabManager.TabPages[designer.ResourceName] == null) {
				Crownwood.Magic.Controls.TabPage newTab = new Crownwood.Magic.Controls.TabPage(designer.ResourceName, (Control)designer, 4);
				if (designer is MpeControlDesigner) {
					newTab.ImageIndex = 0;
				} else if (designer is FontDesigner) {
					newTab.ImageIndex = 1;
				} else if (designer is MpeImageDesigner) {
					newTab.ImageIndex = 2;
				} else if (designer is MpeScreenDesigner) {
					newTab.ImageIndex = 3;
				} else if (designer is MpeStringDesigner) {
					newTab.ImageIndex = 4;
				} else if (designer is MpeHelpBrowser) {
					newTab.ImageIndex = 5;
				}
				newTab.Tag = designer;
				tabManager.TabPages.Add(newTab);
				tabManager.SelectedTab = newTab;
				tabManager.ShowClose = true;
			} else {
				tabManager.SelectedTab = tabManager.TabPages[designer.ResourceName];
			}
		}
		public bool IsResourceOpen(string name) {
			if (tabManager.TabPages[name] != null || tabManager.TabPages[name + "*"] != null) {
				fileMenu.MenuCommands["&Close"].Enabled = true;
				return true;
			}
			fileMenu.MenuCommands["&Close"].Enabled = false;
			return false;
		}
		public void ToggleDesignerStatus(string name, bool modified) {
			if (tabManager.TabPages[name] != null) {
				tabManager.TabPages[name].Title = name + "*";
				fileMenu.MenuCommands["&Save"].Enabled = true;
				fileMenu.MenuCommands["Save A&ll"].Enabled = true;
			} else if (tabManager.TabPages[name + "*"] != null) {
				tabManager.TabPages[name + "*"].Title = name;
				fileMenu.MenuCommands["&Save"].Enabled = false;
				fileMenu.MenuCommands["Save A&ll"].Enabled = false;
			}
		}
		public void FocusPropertyManager() {
			dockManager.ShowContent(dockManager.Contents["Properties"]);
			dockManager.Contents["Properties"].BringToFront();
		}
		public void FocusSkinExplorer() {
			dockManager.ShowContent(dockManager.Contents["Explorer"]);
			dockManager.Contents["Explorer"].BringToFront();
		}
		public void ShowHelp(FileInfo file) {
			if (helpBrowser == null) {
				MpeLog.Info("Creating help browser...");
				helpBrowser = new MpeHelpBrowser(this);
			}
			AddDesigner(helpBrowser);
			try {
				Cursor = Cursors.WaitCursor;
				helpBrowser.ShowHelp(file);
			} catch (Exception e) {
				MessageBox.Show(this, e.Message, "Help Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} finally {
				Cursor = Cursors.Default;
			}
		}
		private void CopyFiles(DirectoryInfo source, DirectoryInfo destination) {
			MpeLog.Info("Copying files [" + source.FullName + "]");
			FileInfo[] files = source.GetFiles();
			MpeLog.Progress(0, files.Length, 0);
			for (int i = 0; i < files.Length; i++) {
				FileInfo f = files[i].CopyTo(destination.FullName + "\\" + files[i].Name);
				MpeLog.Debug("Created [" + f.FullName + "]", i);
			}
			DirectoryInfo[] dirs = source.GetDirectories();
			for (int i = 0; i < dirs.Length; i++) {
				DirectoryInfo destdir = destination.CreateSubdirectory(dirs[i].Name);
				CopyFiles(dirs[i], destdir);
			}
		}
		#endregion

		#region Event Handlers
		protected void OnLoad(object sender, System.EventArgs e) {
			Size = new Size(1024,768);
			CenterToScreen();
			Cursor = Cursors.WaitCursor;
			// Load Editor Configuration
			LoadConfiguration();
			// Continue Initialization
			MpeLog.Info("Initializing...");
			initTimer.Interval = 100;
			initTimer.Start();
		}
		protected void OnInitEditor(object sender, System.EventArgs e) {
			initTimer.Stop();

			// Create Preferences
			try {
				preferences = new MpePreferences("preferences.xml"); 
			} catch {
				MessageBox.Show(this, "Invalid preferences file!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
			}

			// Load Preferences
			try {
				LoadPreferences();
			} catch (Exception ee) {
				MpeLog.Debug(ee.Message);
			}
			
			while (preferences.MediaPortalDir == null) {
				MessageBox.Show(this,"You must set your MediaPortal directory in order to continue.","MediaPortalEditor",MessageBoxButtons.OK,MessageBoxIcon.Information);
				MpePreferenceForm form = new MpePreferenceForm();
				if (form.ShowDialog(this) == DialogResult.Cancel) {
					DialogResult r = MessageBox.Show(this, "You must set your MediaPortal directory.  Click Retry" + Environment.NewLine + "to select the directory, or Cancel to exit the application.", "Invalid Directory", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
					if (r == DialogResult.Cancel) {
						Close();
					} else {
						initTimer.Start();
						return;
					}
				}
			}
			MpeLog.Info("Media Portal Editor");
			Cursor = Cursors.Default;
		}
		protected void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			MpeDesigner[] designers = GetModifiedDesigners();
			if (designers.Length == 0) {
				Cursor = Cursors.Default;
				return;
			}
			MpeSaveForm form = new MpeSaveForm(designers, tabImageList);
			DialogResult result = form.ShowDialog(this);
			if (result == DialogResult.Cancel) {
				e.Cancel = true;
				return;
			} else if (result == DialogResult.Yes) {
				for (int i = 0; i < form.SelectedDesigners.Length; i++) {
					try {
						form.SelectedDesigners[i].Save();
					} catch (Exception ee) {
						MpeLog.Debug(ee);
						MpeLog.Error(ee);
					}
				}
			} else if (result == DialogResult.No) {
				for (int i = 0; i < form.SelectedDesigners.Length; i++) {
					try {
						form.SelectedDesigners[i].Cancel();
					} catch (Exception ee) {
						MpeLog.Debug(ee);
						MpeLog.Error(ee);
					}
				}
			}
		}
		protected void OnClosed(object sender, System.EventArgs e) {
			try {
				SaveConfiguration();
				SavePreferences();
			} catch (Exception ee) {
				MpeLog.Warn(ee);
			}
		}
		protected void OnDesignerIndexChanged(object source, EventArgs args) {
			if (selectedDesignerIndex != -1) {
				MpeDesigner d = (MpeDesigner)tabManager.TabPages[selectedDesignerIndex].Tag;
				if (d != null) {
					try {
						d.Pause();
					} catch (Exception ee) {
						MpeLog.Error(ee);
					}
				}
			}
			if (tabManager.SelectedTab != null) {
				MpeDesigner designer = (MpeDesigner)tabManager.SelectedTab.Tag;
				selectedDesignerIndex = tabManager.SelectedIndex;
				try {
					designer.Resume();
				} catch (Exception ee) {
					MpeLog.Error(ee);
				}
				if (tabManager.SelectedTab.Title.EndsWith("*")) {
					fileMenu.MenuCommands["&Save"].Enabled = true;
				} else {
					fileMenu.MenuCommands["&Save"].Enabled = false;
				}
			}
		}

		protected void OnDesignerClosed(object source, EventArgs args) {
			Cursor = Cursors.WaitCursor;
			MpeDesigner designer = (MpeDesigner)tabManager.SelectedTab.Tag;
			if (tabManager.SelectedTab.Title.EndsWith("*")) {
				MpeSaveForm form = new MpeSaveForm(new MpeDesigner[1] { designer }, tabImageList);
				DialogResult result = form.ShowDialog(this);
				if (result == DialogResult.Cancel) {
					Cursor = Cursors.Default;
					return;
				} else if (result == DialogResult.Yes) {
					OnMenuSave(source, args);
				} else if (result == DialogResult.No) {
					designer.Cancel();
				}
			}
			selectedDesignerIndex = -1;
			tabManager.TabPages.Remove(tabManager.SelectedTab);
			try {
				designer.Destroy();
				if (designer is MpeHelpBrowser)
					helpBrowser = null;
			} catch (Exception e) {
				MpeLog.Debug(e);
				MpeLog.Error(e);
			}
			if (tabManager.TabPages.Count == 0)
				tabManager.ShowClose = false;
			Cursor = Cursors.Default;
		}
		protected void OnTabControlCreated(Crownwood.Magic.Controls.TabControl tabControl) {	
			tabControl.PositionTop = false;
			tabControl.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiForm;
		}
		protected void OnContentHidden(Content c, EventArgs cea) {
			/*for (int i = 0; viewMenu != null && i < viewMenu.MenuCommands.Count; i++) {
				if (((string)(viewMenu.MenuCommands[i].Tag)).Equals(c.Title)) {
					viewMenu.MenuCommands[i].Checked = false;
				}
			}*/
		}
		protected void OnContentShown(Content c, EventArgs cea) {
			/*for (int i = 0; viewMenu != null && i < viewMenu.MenuCommands.Count; i++) {
				if (((string)(viewMenu.MenuCommands[i].Tag)).Equals(c.Title)) {
					viewMenu.MenuCommands[i].Checked = true;
				}
			}*/
		}
		protected void OnMenuView(object source, EventArgs e) {
			MenuCommand cmd = (MenuCommand)source;
			Content c = dockManager.Contents[(string)cmd.Tag];
			if (c != null) {
				dockManager.ShowContent(c);
				c.BringToFront();
			}
		}
		protected void OnMenuNew(object source, EventArgs e) {
			MpeSkinBrowserDialog f = new MpeSkinBrowserDialog();
			DialogResult result = f.ShowDialog(this);
			if (result == DialogResult.OK) {
				try {
					DirectoryInfo skinDir = f.NewSkinDir;
					skinDir.Create();
					MpeLog.Info("Creating new skin [" + skinDir.FullName + "]");
					CopyFiles(f.SelectedSkinDir, skinDir);
					skinDir.Refresh();
					MpeLog.Info("Created new skin [" + skinDir.Name + "]");
					OpenSkin(skinDir);
				} catch (Exception ee) {
					MessageBox.Show(this, ee.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}
		}
		
		protected void OnMenuHelp(object source, EventArgs e) {
			//
		}
		protected void OnMenuOpen(object source, EventArgs e) {
			MpeSkinBrowserDialog f = new MpeSkinBrowserDialog(MpeSkinBrowserMode.Open);
			DialogResult result = f.ShowDialog(this);
			if (result == DialogResult.OK) {
				OpenSkin(f.SelectedSkinDir);
			}
		}
		protected void OnMenuClose(object source, EventArgs e) {
			if (skinExplorer.IsSkinLoaded == false) {
				return;
			}
			if (DialogResult.No == MessageBox.Show(this,"Are you sure you want to close the current skin?", "Close Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) {
				cancelCommand = true;
				return;
			}
			MpeDesigner[] designers = GetModifiedDesigners();
			if (designers.Length > 0) {
				MpeSaveForm form = new MpeSaveForm(designers, tabImageList);
				DialogResult result = form.ShowDialog(this);
				if (result == DialogResult.Cancel) {
					return;
				} else if (result == DialogResult.Yes) {
					for (int i = 0; i < form.SelectedDesigners.Length; i++) {
						try {
							form.SelectedDesigners[i].Save();
						} catch (Exception ee) {
							MpeLog.Debug(ee);
							MpeLog.Error(ee);
						}
					}
				} else if (result == DialogResult.No) {
					for (int i = 0; i < form.SelectedDesigners.Length; i++) {
						try {
							form.SelectedDesigners[i].Cancel();
						} catch (Exception ee) {
							MpeLog.Debug(ee);
							MpeLog.Error(ee);
						}
					}
				}
			}
			try {
				selectedDesignerIndex = -1;
				for (int i = 0; i < tabManager.TabPages.Count; i++) {
					MpeDesigner designer = (MpeDesigner)tabManager.TabPages[i].Tag;
					designer.Destroy();
				}
				tabManager.TabPages.Clear();
			} catch {
				//
			}
			skinExplorer.CloseSkin();
			fileMenu.MenuCommands["&Close"].Enabled = false;
			Text = "MediaPortalEditor";
			MpeLog.Info("Skin closed");
		}
		protected void OnMenuSave(object source, EventArgs e) {
			if (tabManager.SelectedTab != null) {
				try {
					MpeDesigner designer = (MpeDesigner)tabManager.SelectedTab.Tag;
					designer.Save();
				} catch (Exception ee) {
					MpeLog.Debug(ee);
					MpeLog.Error(ee);
				}
			}
		}
		protected void OnMenuSaveAll(object source, EventArgs e) {
			MpeDesigner[] designers = GetModifiedDesigners();
			for (int i = 0; i < designers.Length; i++) {
				try {
					designers[i].Save();
				} catch (Exception ee) {
					MpeLog.Debug(ee);
					MpeLog.Error(ee);
				}
			}
		}
		protected void OnMenuExit(object source, EventArgs e) {
			Close();
		}
		protected void OnMenuPreferences(object source, EventArgs e) {
			new MpePreferenceForm().ShowDialog(this);
		}
		protected void OnMenuAbout(object source, EventArgs e) {
			new MpeAboutForm().ShowDialog(this);
		}
		
		protected void OnMenuCopy(object source, EventArgs e) {
			if (tabManager.SelectedTab != null && tabManager.SelectedTab.Tag is MpeResourceDesigner) {
				MpeResourceDesigner d = (MpeResourceDesigner)tabManager.SelectedTab.Tag;
				d.CopyControl();
			}
		}
		protected void OnMenuPaste(object source, EventArgs e) {
			if (tabManager.SelectedTab != null && tabManager.SelectedTab.Tag is MpeResourceDesigner) {
				MpeResourceDesigner d = (MpeResourceDesigner)tabManager.SelectedTab.Tag;
				d.PasteControl();
			}
		}
		protected void OnMenuCut(object source, EventArgs e) {
			if (tabManager.SelectedTab != null && tabManager.SelectedTab.Tag is MpeResourceDesigner) {
				MpeResourceDesigner d = (MpeResourceDesigner)tabManager.SelectedTab.Tag;
				d.CutControl();
			}
		}
		#endregion
		
		#region Windows Form Designer Generated Code
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		/// <summary>
		/// Required method for Designer support 
		/// Do not modify the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MediaPortalEditor));
			this.menuImageList = new System.Windows.Forms.ImageList(this.components);
			this.initTimer = new System.Windows.Forms.Timer(this.components);
			this.tabImageList = new System.Windows.Forms.ImageList(this.components);
			this.serviceImageList = new System.Windows.Forms.ImageList(this.components);
			this.statusBarIcons = new System.Windows.Forms.ImageList(this.components);
			// 
			// menuImageList
			// 
			this.menuImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.menuImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.menuImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("menuImageList.ImageStream")));
			this.menuImageList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// initTimer
			// 
			this.initTimer.Tick += new System.EventHandler(this.OnInitEditor);
			// 
			// tabImageList
			// 
			this.tabImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.tabImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.tabImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("tabImageList.ImageStream")));
			this.tabImageList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// serviceImageList
			// 
			this.serviceImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.serviceImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.serviceImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("serviceImageList.ImageStream")));
			this.serviceImageList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// statusBarIcons
			// 
			this.statusBarIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.statusBarIcons.ImageSize = new System.Drawing.Size(16, 16);
			this.statusBarIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("statusBarIcons.ImageStream")));
			this.statusBarIcons.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// MediaPortalEditor
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Gray;
			this.ClientSize = new System.Drawing.Size(704, 518);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MediaPortalEditor";
			this.Text = "Media Portal - Skin Editor";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.OnClosing);
			this.Load += new System.EventHandler(this.OnLoad);
			this.Closed += new System.EventHandler(this.OnClosed);

		}
		#endregion

		#region Main
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			
			try {
				Application.Run(new MediaPortalEditor());
			} catch (Exception e) {
				MpeLog.Debug(e);
				MpeLog.Error(e);
			}
		}
		#endregion				
	}

}
