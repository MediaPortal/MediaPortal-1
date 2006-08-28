#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
  public class SetupForm : Form, ISetupForm, IShowPlugin
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;
    private TreeView appTree;

    private Applist apps = ProgramDatabase.AppList;
    // create setting-tabs
    private AppSettingsDirBrowse sectionDirBrowse = new AppSettingsDirBrowse();
    private AppSettingsDirCache sectionDirCache = new AppSettingsDirCache();
    private AppSettingsMyFileIni sectionMyFileIni = new AppSettingsMyFileIni();
    private AppSettingsMyFileMeedio sectionMyFileMeedio = new AppSettingsMyFileMeedio();
    private AppSettingsFilelauncher sectionFilelauncher = new AppSettingsFilelauncher();
    private AppSettingsMameDirect sectionMameDirect = new AppSettingsMameDirect();
    private AppSettingsGamebase sectionGamebase = new AppSettingsGamebase();
    private AppSettingsGrouper sectionGrouper = new AppSettingsGrouper();
    private AppSettingsRoot sectionRoot = new AppSettingsRoot();
    private AppFilesView filesView = new AppFilesView();
    private ProgramViews programsView = new ProgramViews();
    private AppFilesImportProgress filesProgress = new AppFilesImportProgress();
    private ToolBar toolBarMenu;
    private ToolBarButton buttonAddChild;
    private ToolBarButton buttonDelete;
    private ToolBarButton buttonUp;
    private ToolBarButton buttonDown;
    private ContextMenu popupAddChild;
    private ToolBarButton sep1;
    private ToolBarButton sep2;
    private ToolBarButton sep3;
    private ToolBarButton sep4;
    private MenuItem menuDirBrowse;
    private MenuItem menuDirCache;
    private MenuItem menuMyFile;
    private MenuItem menuFileLauncher;
    private MenuItem menuGrouper;
    private TabControl detailsTabControl;
    private TabPage detailsPage;
    private TabPage filesPage;
    private Panel holderPanel;
    private ToolBarButton sep5;
    private ContextMenu popupTools;
    private MenuItem MenuItemChangeSourceType;
    private MenuItem SourceTypeToDirBrowse;
    private MenuItem SourceTypeToMyIni;
    private MenuItem SourceTypeToMLF;
    private MenuItem SourceTypeToDirCache;
    private MenuItem SourceTypeToFilelauncher;
    private ToolBarButton buttonTools;
    private Panel holderPanelFiles;
    private MenuItem menuItem2;
    private MenuItem menuItemReadFromProfile;
    // pointer to currently displayed sheet
    private AppSettings pageCurrentSettings = null;
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.MenuItem menuMlfFile;
    private System.Windows.Forms.MenuItem menuMAMEDirect;
    private MediaPortal.UserInterface.Controls.MPTabPage viewsPage;
    private System.Windows.Forms.Panel holderPanelViews;
    private System.Windows.Forms.MenuItem menuGamebaseImporter;
    private System.Windows.Forms.MenuItem SourceTypeToGamebase;
    private bool m_ProfilesLoaded = false;

    public SetupForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      filesView.OnRefreshClick += new EventHandler(this.RefreshClick);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    public bool CanEnable() // Indicates whether plugin can be enabled/disabled
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
      string strText = ProgramSettings.ReadSetting(ProgramUtils.cPLUGINTITLE);
      if ((strText != "") && (strText != null))
      {
        strButtonText = strText;
      }
      else
      {
        strButtonText = GUILocalizeStrings.Get(0);
      }
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return true;
    }

    public string PluginName()
    {
      return "My Programs";
    }
    public string Description()
    {
      return "Launches external documents and applications";
    }
    public string Author()
    {
      return "waeberd";
    }
    public void ShowPlugin()
    {
      ShowDialog();
    }
    public bool DefaultEnabled()
    {
      return false;
    }

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SetupForm));
      this.appTree = new System.Windows.Forms.TreeView();
      this.toolBarMenu = new System.Windows.Forms.ToolBar();
      this.buttonAddChild = new System.Windows.Forms.ToolBarButton();
      this.popupAddChild = new System.Windows.Forms.ContextMenu();
      this.menuDirCache = new System.Windows.Forms.MenuItem();
      this.menuFileLauncher = new System.Windows.Forms.MenuItem();
      this.menuGrouper = new System.Windows.Forms.MenuItem();
      this.menuMyFile = new System.Windows.Forms.MenuItem();
      this.menuDirBrowse = new System.Windows.Forms.MenuItem();
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.menuMAMEDirect = new System.Windows.Forms.MenuItem();
      this.menuMlfFile = new System.Windows.Forms.MenuItem();
      this.sep1 = new System.Windows.Forms.ToolBarButton();
      this.buttonDelete = new System.Windows.Forms.ToolBarButton();
      this.sep2 = new System.Windows.Forms.ToolBarButton();
      this.buttonUp = new System.Windows.Forms.ToolBarButton();
      this.sep3 = new System.Windows.Forms.ToolBarButton();
      this.buttonDown = new System.Windows.Forms.ToolBarButton();
      this.sep4 = new System.Windows.Forms.ToolBarButton();
      this.buttonTools = new System.Windows.Forms.ToolBarButton();
      this.popupTools = new System.Windows.Forms.ContextMenu();
      this.MenuItemChangeSourceType = new System.Windows.Forms.MenuItem();
      this.SourceTypeToDirBrowse = new System.Windows.Forms.MenuItem();
      this.SourceTypeToDirCache = new System.Windows.Forms.MenuItem();
      this.SourceTypeToMyIni = new System.Windows.Forms.MenuItem();
      this.SourceTypeToMLF = new System.Windows.Forms.MenuItem();
      this.SourceTypeToFilelauncher = new System.Windows.Forms.MenuItem();
      this.menuItemReadFromProfile = new System.Windows.Forms.MenuItem();
      this.menuItem2 = new System.Windows.Forms.MenuItem();
      this.sep5 = new System.Windows.Forms.ToolBarButton();
      this.detailsTabControl = new MediaPortal.UserInterface.Controls.MPTabControl();
      this.detailsPage = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.holderPanel = new System.Windows.Forms.Panel();
      this.filesPage = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.holderPanelFiles = new System.Windows.Forms.Panel();
      this.viewsPage = new MediaPortal.UserInterface.Controls.MPTabPage();
      this.holderPanelViews = new System.Windows.Forms.Panel();
      this.menuGamebaseImporter = new System.Windows.Forms.MenuItem();
      this.SourceTypeToGamebase = new System.Windows.Forms.MenuItem();
      this.detailsTabControl.SuspendLayout();
      this.detailsPage.SuspendLayout();
      this.filesPage.SuspendLayout();
      this.viewsPage.SuspendLayout();
      this.SuspendLayout();
      // 
      // appTree
      // 
      this.appTree.AllowDrop = true;
      this.appTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
      this.appTree.HideSelection = false;
      this.appTree.HotTracking = true;
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
      this.appTree.Size = new System.Drawing.Size(224, 468);
      this.appTree.TabIndex = 8;
      this.appTree.DragOver += new System.Windows.Forms.DragEventHandler(this.appTree_DragOver);
      this.appTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      this.appTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.appTree_BeforeSelect);
      this.appTree.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.appTree_AfterLabelEdit);
      this.appTree.DragEnter += new System.Windows.Forms.DragEventHandler(this.appTree_DragEnter);
      this.appTree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.appTree_ItemDrag);
      this.appTree.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.appTree_BeforeLabelEdit);
      this.appTree.DragDrop += new System.Windows.Forms.DragEventHandler(this.appTree_DragDrop);
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
                                                                                   this.sep4,
                                                                                   this.buttonTools,
                                                                                   this.sep5});
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
                                                                                  this.menuDirCache,
                                                                                  this.menuFileLauncher,
                                                                                  this.menuGrouper,
                                                                                  this.menuMyFile,
                                                                                  this.menuDirBrowse,
                                                                                  this.menuItem1});
      // 
      // menuDirCache
      // 
      this.menuDirCache.Index = 0;
      this.menuDirCache.Text = "Directory-Cache";
      this.menuDirCache.Click += new System.EventHandler(this.menuDirCache_Click);
      // 
      // menuFileLauncher
      // 
      this.menuFileLauncher.Index = 1;
      this.menuFileLauncher.Text = "Filelauncher";
      this.menuFileLauncher.Click += new System.EventHandler(this.menuFileLauncher_Click);
      // 
      // menuGrouper
      // 
      this.menuGrouper.Index = 2;
      this.menuGrouper.Text = "Grouper";
      this.menuGrouper.Click += new System.EventHandler(this.menuGrouper_Click);
      // 
      // menuMyFile
      // 
      this.menuMyFile.Index = 3;
      this.menuMyFile.Text = "*.my File Importer";
      this.menuMyFile.Click += new System.EventHandler(this.menuMyFile_Click);
      // 
      // menuDirBrowse
      // 
      this.menuDirBrowse.Index = 4;
      this.menuDirBrowse.Text = "Directory-Browse";
      this.menuDirBrowse.Click += new System.EventHandler(this.menuDirBrowse_Click);
      // 
      // menuItem1
      // 
      this.menuItem1.Index = 5;
      this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                              this.menuMAMEDirect,
                                                                              this.menuGamebaseImporter,
                                                                              this.menuMlfFile});
      this.menuItem1.Text = "Extended";
      // 
      // menuMAMEDirect
      // 
      this.menuMAMEDirect.Index = 0;
      this.menuMAMEDirect.Text = "MAME Direct Importer";
      this.menuMAMEDirect.Click += new System.EventHandler(this.menuMAMEDirect_Click);
      // 
      // menuMlfFile
      // 
      this.menuMlfFile.Index = 2;
      this.menuMlfFile.Text = "*.mlf File Importer";
      this.menuMlfFile.Click += new System.EventHandler(this.menuMLFFile_Click);
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
      // buttonTools
      // 
      this.buttonTools.DropDownMenu = this.popupTools;
      this.buttonTools.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
      this.buttonTools.Text = "Tools";
      // 
      // popupTools
      // 
      this.popupTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                               this.MenuItemChangeSourceType,
                                                                               this.menuItemReadFromProfile});
      this.popupTools.Popup += new System.EventHandler(this.popupTools_Popup);
      // 
      // MenuItemChangeSourceType
      // 
      this.MenuItemChangeSourceType.Index = 0;
      this.MenuItemChangeSourceType.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                             this.SourceTypeToDirBrowse,
                                                                                             this.SourceTypeToDirCache,
                                                                                             this.SourceTypeToMyIni,
                                                                                             this.SourceTypeToGamebase,
                                                                                             this.SourceTypeToMLF,
                                                                                             this.SourceTypeToFilelauncher});
      this.MenuItemChangeSourceType.Text = "Change Source Type to";
      this.MenuItemChangeSourceType.Select += new System.EventHandler(this.MenuItemChangeSourceType_Select);
      // 
      // SourceTypeToDirBrowse
      // 
      this.SourceTypeToDirBrowse.Index = 0;
      this.SourceTypeToDirBrowse.Text = "Directory-Browse";
      this.SourceTypeToDirBrowse.Click += new System.EventHandler(this.SourceTypeToDirBrowse_Click);
      // 
      // SourceTypeToDirCache
      // 
      this.SourceTypeToDirCache.Index = 1;
      this.SourceTypeToDirCache.Text = "Directory-Cache";
      this.SourceTypeToDirCache.Click += new System.EventHandler(this.SourceTypeToDirCache_Click);
      // 
      // SourceTypeToMyIni
      // 
      this.SourceTypeToMyIni.Index = 2;
      this.SourceTypeToMyIni.Text = "*.my File Importer";
      this.SourceTypeToMyIni.Click += new System.EventHandler(this.SourceTypeToMyIni_Click);
      // 
      // SourceTypeToMLF
      // 
      this.SourceTypeToMLF.Index = 4;
      this.SourceTypeToMLF.Text = "*.mlf File Importer";
      this.SourceTypeToMLF.Click += new System.EventHandler(this.SourceTypeToMLF_Click);
      // 
      // SourceTypeToFilelauncher
      // 
      this.SourceTypeToFilelauncher.Index = 5;
      this.SourceTypeToFilelauncher.Text = "Filelauncher";
      this.SourceTypeToFilelauncher.Click += new System.EventHandler(this.SourceTypeToFilelauncher_Click);
      // 
      // menuItemReadFromProfile
      // 
      this.menuItemReadFromProfile.Index = 1;
      this.menuItemReadFromProfile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                            this.menuItem2});
      this.menuItemReadFromProfile.Text = "Read From Profile";
      // 
      // menuItem2
      // 
      this.menuItem2.Index = 0;
      this.menuItem2.Text = "<dummy>";
      // 
      // sep5
      // 
      this.sep5.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
      // 
      // detailsTabControl
      // 
      this.detailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.detailsTabControl.Controls.Add(this.detailsPage);
      this.detailsTabControl.Controls.Add(this.filesPage);
      this.detailsTabControl.Controls.Add(this.viewsPage);
      this.detailsTabControl.Location = new System.Drawing.Point(240, 40);
      this.detailsTabControl.Name = "detailsTabControl";
      this.detailsTabControl.SelectedIndex = 0;
      this.detailsTabControl.Size = new System.Drawing.Size(416, 468);
      this.detailsTabControl.TabIndex = 14;
      this.detailsTabControl.SelectedIndexChanged += new System.EventHandler(this.DetailsTabControl_SelectedIndexChanged);
      // 
      // detailsPage
      // 
      this.detailsPage.Controls.Add(this.holderPanel);
      this.detailsPage.Location = new System.Drawing.Point(4, 22);
      this.detailsPage.Name = "detailsPage";
      this.detailsPage.Size = new System.Drawing.Size(408, 442);
      this.detailsPage.TabIndex = 0;
      this.detailsPage.Text = "Details";
      // 
      // holderPanel
      // 
      this.holderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.holderPanel.Location = new System.Drawing.Point(3, 3);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(397, 436);
      this.holderPanel.TabIndex = 12;
      // 
      // filesPage
      // 
      this.filesPage.Controls.Add(this.holderPanelFiles);
      this.filesPage.Location = new System.Drawing.Point(4, 22);
      this.filesPage.Name = "filesPage";
      this.filesPage.Size = new System.Drawing.Size(408, 442);
      this.filesPage.TabIndex = 1;
      this.filesPage.Text = "Files";
      // 
      // holderPanelFiles
      // 
      this.holderPanelFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.holderPanelFiles.Location = new System.Drawing.Point(6, 3);
      this.holderPanelFiles.Name = "holderPanelFiles";
      this.holderPanelFiles.Size = new System.Drawing.Size(397, 436);
      this.holderPanelFiles.TabIndex = 13;
      // 
      // viewsPage
      // 
      this.viewsPage.Controls.Add(this.holderPanelViews);
      this.viewsPage.Location = new System.Drawing.Point(4, 22);
      this.viewsPage.Name = "viewsPage";
      this.viewsPage.Size = new System.Drawing.Size(408, 442);
      this.viewsPage.TabIndex = 2;
      this.viewsPage.Text = "Views";
      // 
      // holderPanelViews
      // 
      this.holderPanelViews.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.holderPanelViews.Location = new System.Drawing.Point(6, 3);
      this.holderPanelViews.Name = "holderPanelViews";
      this.holderPanelViews.Size = new System.Drawing.Size(397, 436);
      this.holderPanelViews.TabIndex = 14;
      // 
      // menuGamebaseImporter
      // 
      this.menuGamebaseImporter.Index = 1;
      this.menuGamebaseImporter.Text = "GAMEBASE Importer";
      this.menuGamebaseImporter.Click += new System.EventHandler(this.menuGamebase_Click);
      // 
      // SourceTypeToGamebase
      // 
      this.SourceTypeToGamebase.Index = 3;
      this.SourceTypeToGamebase.Text = "Gamebase Importer";
      this.SourceTypeToGamebase.Click += new System.EventHandler(this.SourceTypeToGamebase_Click);
      // 
      // SetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(666, 514);
      this.Controls.Add(this.detailsTabControl);
      this.Controls.Add(this.toolBarMenu);
      this.Controls.Add(this.appTree);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "SetupForm";
      this.Text = "my Programs Setup";
      this.Closing += new System.ComponentModel.CancelEventHandler(this.SetupForm_Closing);
      this.Load += new System.EventHandler(this.SetupForm_Load);
      this.detailsTabControl.ResumeLayout(false);
      this.detailsPage.ResumeLayout(false);
      this.filesPage.ResumeLayout(false);
      this.viewsPage.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void DoAddChild(myProgSourceType newSourceType)
    {
      if (SaveAppItem())
      {
        AppItem newApp = ApplicationFactory.AppFactory.GetAppItem(ProgramDatabase.sqlDB, newSourceType);
        apps.Add(newApp);
        newApp.FatherID = GetSelectedAppID();
        newApp.Position = apps.GetMaxPosition(newApp.FatherID) + 10;
        newApp.Title = "New item";
        newApp.SourceType = newSourceType;
        newApp.Write();
        apps.LoadAll();
        updateTree();
        // the selected node has a new child => change selection to last child added
        if (appTree.SelectedNode != null)
          if (appTree.SelectedNode.Nodes.Count > 0)
          {
            appTree.SelectedNode = appTree.SelectedNode.Nodes[appTree.SelectedNode.Nodes.Count - 1];
          }
      }
    }

    private void DoModifySourceType(myProgSourceType newSourceType)
    {
      AppItem curApp = GetSelectedAppItem();
      if (curApp == null)
      {
        return;
      }

      // brute force.... change, save and go into edit mode.....
      curApp.SourceType = newSourceType;
      curApp.Write();
      apps.LoadAll();
      updateTree();
    }

    /*
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
     */

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
      ArrayList Indexes = new ArrayList(IndexPath.Split(';'));
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


    private void AttachChildren(TreeNode Parent, int FatherID)
    {
      foreach (AppItem app in apps.appsOfFatherID(FatherID))
      {
        TreeNode curNode = new TreeNode(app.Title);
        curNode.Tag = app;
        Parent.Nodes.Add(curNode);
        AttachChildren(curNode, app.AppID); // recursive call
      }

    }

    private void updateTree()
    {
      string IndexPath = GetSelectedIndexPath();
      appTree.BeginUpdate();
      appTree.Nodes.Clear();

      // add root
      appTree.Nodes.Add(new TreeNode("my Programs"));
      AttachChildren(appTree.Nodes[0], -1);

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


    private void DoDelete()
    {
      AppItem app = GetSelectedAppItem();
      if (app != null)
      {
        DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this application item?", "Information", MessageBoxButtons.YesNo,
          MessageBoxIcon.Question);
        if (dialogResult == DialogResult.Yes)
        {
          app.Delete();
          apps.LoadAll();
          updateTree();
        }
      }
    }

    private AppItem GetPrevAppItem()
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

    private AppItem GetNextAppItem()
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

    private AppItem GetAppItemOfTreeNode(TreeNode curNode)
    {
      AppItem res = null;
      if (curNode != null)
      {
        if (curNode.Tag != null)
        {
          res = (AppItem)curNode.Tag;
        }
      }
      return res;
    }

    private AppItem GetSelectedAppItem()
    {
      return GetAppItemOfTreeNode(appTree.SelectedNode);
    }

    private int GetSelectedAppID()
    {
      int res = -1;
      AppItem curApp = GetSelectedAppItem();
      if (curApp != null)
      {
        res = curApp.AppID;
      }
      return res;
    }



    private void AttachFilesView()
    {
      holderPanelFiles.Controls.Clear();
      holderPanelFiles.Controls.Add(filesView);
      filesView.SetBounds(0, 0, holderPanelFiles.Width, holderPanelFiles.Height);
      filesView.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left;
    }

    void AttachProgramsView()
    {
      holderPanelViews.Controls.Clear();
      holderPanelViews.Controls.Add(programsView);
      programsView.SetBounds(0, 0, holderPanelViews.Width, holderPanelViews.Height);
      programsView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    }

    private void AttachImportRunningView()
    {
      holderPanelFiles.Controls.Clear();
      holderPanelFiles.Controls.Add(filesProgress);
      filesProgress.SetBounds(0, 0, holderPanelFiles.Width, holderPanelFiles.Height);
      filesProgress.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left;
    }

    private void SetupForm_Load(object sender, EventArgs e)
    {
      AttachFilesView();
      apps.LoadAll(); // we need all apps, whether enabled or not => reload with LoadALL
      AttachProgramsView();
      updateTree();
    }

    private void DoUp()
    {
      AppItem curApp = GetSelectedAppItem();
      AppItem prevApp = GetPrevAppItem();
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
      AppItem curApp = GetSelectedAppItem();
      AppItem nextApp = GetNextAppItem();
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


    private void RefreshClick(object sender, EventArgs e)
    {
      DoRefresh();
    }

    private void UpClick(object sender, EventArgs e)
    {
      DoUp();
    }

    private void DownClick(object sender, EventArgs e)
    {
      DoDown();
    }



    private void BlockControls()
    {
      appTree.Enabled = false;
      toolBarMenu.Enabled = false;
    }


    private void UnblockControls()
    {
      appTree.Enabled = true;
      toolBarMenu.Enabled = true;
    }

    private void DoRefresh()
    {
      bool DoIt = false;
      AppItem curApp = GetSelectedAppItem();
      if (curApp != null)
      {
        if (curApp.Files.Count > 0)
        {
          DialogResult dialogResult = MessageBox.Show("Are you sure to reimport?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
          DoIt = (dialogResult == DialogResult.Yes);
        }
        else
        {
          DoIt = true;
        }

        if (DoIt)
        {
          try
          {
            AttachImportRunningView();
            BlockControls();
            filesProgress.CurApp = curApp;
            filesProgress.RunImport();
          }
          finally
          {
            AttachFilesView();
            UnblockControls();
          }
        }
      }
    }

    private AppSettings GetCurrentSettingsPage()
    {
      AppSettings res = null;
      AppItem curApp = GetSelectedAppItem();
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
          case myProgSourceType.MAMEDIRECT:
            res = sectionMameDirect;
            break;
          case myProgSourceType.FILELAUNCHER:
            res = sectionFilelauncher;
            break;
          case myProgSourceType.GROUPER:
            res = sectionGrouper;
            break;
          case myProgSourceType.GAMEBASE:
            res = sectionGamebase;
            break;
        }
      }
      else
      {
        res = sectionRoot;
      }
      return res;
    }

    bool TabIsDisplayed(TabPage view)
    {
      bool res = false;
      foreach (Control curCtl in detailsTabControl.Controls)
      {
        if (curCtl == view)
        {
          res = true;
          break;
        }
      }
      return res;
    }
    private void AddFilesPage(AppItem curApp)
    {
      if (!TabIsDisplayed(filesPage))
      {
        detailsTabControl.Controls.Add(this.filesPage);
      }
      filesView.Refresh(curApp);
    }

    private void AddViewsPage()
    {
      if (!TabIsDisplayed(viewsPage))
      {
        detailsTabControl.Controls.Add(this.viewsPage);
      }
      viewsPage.Refresh();
    }

    private void RemoveFilesPage()
    {
      if (TabIsDisplayed(filesPage))
      {
        detailsTabControl.Controls.Remove(this.filesPage);
      }
    }

    void RemoveViewsPage()
    {
      if (TabIsDisplayed(viewsPage))
      {
        detailsTabControl.Controls.Remove(this.viewsPage);
      }
    }


    private void SyncPanel(AppSettings pageSettings)
    {
      holderPanel.Controls.Clear();

      if (pageSettings != null)
      {
        holderPanel.Controls.Add(pageSettings);
        AppItem curApp = GetSelectedAppItem();
        pageSettings.AppObj2Form(curApp);
        pageSettings.SetBounds(0, 0, holderPanel.Width, holderPanel.Height);
        if (curApp != null)
        {
          RemoveViewsPage();
          if (curApp.FileEditorAllowed())
          {
            AddFilesPage(curApp);
          }
          else
          {
            RemoveFilesPage();
          }
        }
        else
        {
          // special treatment for root node
          RemoveFilesPage();
          AddViewsPage();
        }
      }
    }


    private void appTree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
      if (e.Label != null)
      {
        if (e.Node.Tag != null)
        {
          e.Node.EndEdit(false);
          AppItem curApp = (AppItem)e.Node.Tag;
          curApp.Title = e.Label;
          curApp.Write();
          SyncDetails();
        }
        else
        {
          e.CancelEdit = true;
        }
      }
    }

    private void appTree_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
      // disallow edit for root node.....
      if (e.Node.Tag == null)
      {
        e.CancelEdit = true;
      }
    }

    private void appTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
    {
      if (!SaveAppItem())
      {
        e.Cancel = true;
      }
    }

    private bool SaveAppItem()
    {
      bool res = true;
      AppItem curApp = this.GetSelectedAppItem();
      // curApp can be NULL for the root Element..... so don't catch this!
      pageCurrentSettings = GetCurrentSettingsPage();
      if (pageCurrentSettings != null)
      {
        if (pageCurrentSettings.EntriesOK(curApp))
        {
          pageCurrentSettings.OnUpClick -= new EventHandler(this.UpClick);
          pageCurrentSettings.OnDownClick -= new EventHandler(this.DownClick);
          pageCurrentSettings.Form2AppObj(curApp);
          if (curApp != null)
          {
            curApp.Write();
            appTree.SelectedNode.Text = curApp.Title;
            res = true;
          }
        }
        else
        {
          // some of the entries are invalid
          res = false;
        }
      }
      return res;
    }


    private void sectionTree_AfterSelect(object sender, TreeViewEventArgs e)
    {
      SyncDetails();
    }

    private void SyncDetails()
    {
      pageCurrentSettings = GetCurrentSettingsPage();
      if (pageCurrentSettings != null)
      {
        pageCurrentSettings.OnUpClick += new EventHandler(this.UpClick);
        pageCurrentSettings.OnDownClick += new EventHandler(this.DownClick);
      }
      SyncPanel(pageCurrentSettings);
      SyncButtons();
    }

    private void SyncButtons()
    {
      if (appTree.SelectedNode == null)
      {
        // play safe and disable all......
        buttonAddChild.Enabled = false;
        buttonDelete.Enabled = false;
        buttonUp.Enabled = false;
        buttonDown.Enabled = false;
      }
      else
        if (NodeAcceptsChildren(appTree.SelectedNode))
        {
          buttonAddChild.Enabled = true;
          buttonDelete.Enabled = false;
          if (appTree.SelectedNode != appTree.Nodes[0])
          {
            AppItem curApp = GetSelectedAppItem();
            if (curApp != null)
            {
              buttonDelete.Enabled = (apps.appsOfFatherID(curApp.AppID).Count == 0); // groupitems are only deletable if no children exist
            }
          }
          buttonUp.Enabled = (appTree.SelectedNode != appTree.Nodes[0]) && (GetPrevAppItem() != null);
          buttonDown.Enabled = (appTree.SelectedNode != appTree.Nodes[0]) && (GetNextAppItem() != null);
        }
        else
        {
          buttonAddChild.Enabled = false;
          buttonUp.Enabled = (GetPrevAppItem() != null);
          buttonDown.Enabled = (GetNextAppItem() != null);
          buttonDelete.Enabled = true;
        }

    }


    private bool NodeAcceptsChildren(TreeNode node)
    {
      bool res = false;
      if (node == appTree.Nodes[0])
      {
        res = true;
      }
      else
      {
        AppItem curApp = GetSelectedAppItem();
        if (curApp != null)
        {
          res = curApp.SubItemsAllowed();
        }
      }
      return res;
    }

    private void appTree_DragDrop(object sender, DragEventArgs e)
    {
      // Retrieve the client coordinates of the drop location.
      Point targetPoint = appTree.PointToClient(new Point(e.X, e.Y));

      // Retrieve the node at the drop location.
      TreeNode targetNode = appTree.GetNodeAt(targetPoint);

      // Retrieve the node that was dragged.
      TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

      AppItem targetApp = GetAppItemOfTreeNode(targetNode);
      AppItem draggedApp = GetAppItemOfTreeNode(draggedNode);

      bool validTarget = NodeAcceptsChildren(targetNode);

      // Confirm that the node at the drop location is not 
      // the dragged node or a descendant of the dragged node.
      if (!draggedNode.Equals(targetNode) && !ContainsNode(draggedNode, targetNode) && validTarget)
      {
        // If it is a move operation, remove the node from its current 
        // location and add it to the node at the drop location.
        if (e.Effect == DragDropEffects.Move)
        {
          draggedNode.Remove();
          targetNode.Nodes.Add(draggedNode);
          if (targetApp == null)
          {
            draggedApp.FatherID = -1;
          }
          else
          {
            draggedApp.FatherID = targetApp.AppID;
          }
          draggedApp.Write();
        }

        // If it is a copy operation, clone the dragged node 
        // and add it to the node at the drop location.
        else if (e.Effect == DragDropEffects.Copy)
        {
          TreeNode newNode = (TreeNode)draggedNode.Clone();
          AppItem newApp = GetAppItemOfTreeNode(newNode);
          newApp = apps.CloneAppItem(newApp);
          if (targetApp == null)
          {
            newApp.FatherID = -1;
          }
          else
          {
            newApp.FatherID = targetApp.AppID;
          }
          newApp.Title = newApp.Title + "*";
          newApp.Position = apps.GetMaxPosition(newApp.FatherID) + 10;
          newApp.Write();
          newNode.Tag = newApp;
          newNode.Text = newApp.Title; // refresh caption
          targetNode.Nodes.Add(newNode);

        }

        // Expand the node at the location 
        // to show the dropped node.
        targetNode.Expand();
      }
    }

    private void appTree_DragEnter(object sender, DragEventArgs e)
    {
      e.Effect = e.AllowedEffect;
    }

    private void appTree_ItemDrag(object sender, ItemDragEventArgs e)
    {
      // Move the dragged node when the left mouse button is used.
      if (e.Button == MouseButtons.Left)
      {
        DoDragDrop(e.Item, DragDropEffects.Move);
      }

      // Copy the dragged node when the right mouse button is used.
      else if (e.Button == MouseButtons.Right)
      {
        DoDragDrop(e.Item, DragDropEffects.Copy);
      }
    }

    // Determine whether one node is a parent 
    // or ancestor of a second node.
    private bool ContainsNode(TreeNode node1, TreeNode node2)
    {
      // Check the parent node of the second node.
      if (node2.Parent == null)
        return false;
      if (node2.Parent.Equals(node1))
        return true;

      // If the parent node is not null or equal to the first node, 
      // call the ContainsNode method recursively using the parent of 
      // the second node.
      return ContainsNode(node1, node2.Parent);
    }



    private void toolBarMenu_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
    {
      switch (toolBarMenu.Buttons.IndexOf(e.Button))
      {
        // todo: avoid magic ints here..... use consts!
        case 0:
          popupAddChild.Show(toolBarMenu, new Point(0, toolBarMenu.Height));
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
        case 8:
          popupTools.Show(toolBarMenu, new Point(buttonTools.Rectangle.Left, toolBarMenu.Height));
          break;

      }
    }


    private void menuDirBrowse_Click(object sender, EventArgs e)
    {
      DoAddChild(myProgSourceType.DIRBROWSE);
    }

    private void menuDirCache_Click(object sender, EventArgs e)
    {
      DoAddChild(myProgSourceType.DIRCACHE);
    }

    private void menuMyFile_Click(object sender, EventArgs e)
    {
      DoAddChild(myProgSourceType.MYFILEINI);
    }

    private void menuMLFFile_Click(object sender, EventArgs e)
    {
      DoAddChild(myProgSourceType.MYFILEMEEDIO);
    }

    private void menuGamebase_Click(object sender, EventArgs e)
    {
      DoAddChild(myProgSourceType.GAMEBASE);
    }

    private void menuFileLauncher_Click(object sender, EventArgs e)
    {
      DoAddChild(myProgSourceType.FILELAUNCHER);
    }

    private void menuGrouper_Click(object sender, EventArgs e)
    {
      DoAddChild(myProgSourceType.GROUPER);
    }

    private void menuMAMEDirect_Click(object sender, System.EventArgs e)
    {
      DoAddChild(myProgSourceType.MAMEDIRECT);
    }

    private void SetupForm_Closing(object sender, CancelEventArgs e)
    {
      AppItem curApp = this.GetSelectedAppItem();
      pageCurrentSettings = GetCurrentSettingsPage();
      if (pageCurrentSettings != null)
      {
        if (pageCurrentSettings.EntriesOK(curApp))
        {
          pageCurrentSettings.OnUpClick -= new EventHandler(this.UpClick);
          pageCurrentSettings.OnDownClick -= new EventHandler(this.DownClick);
          pageCurrentSettings.Form2AppObj(curApp);
          if (curApp != null)
          {
            curApp.Write();
          }
        }
        else
        {
          e.Cancel = true;
        }
      }
    }

    private void appTree_DragOver(object sender, DragEventArgs e)
    {
      // Retrieve the client coordinates of the mouse position.
      Point targetPoint = appTree.PointToClient(new Point(e.X, e.Y));

      // Select the node at the mouse position.
      appTree.SelectedNode = appTree.GetNodeAt(targetPoint);
    }

    private void popupTools_Popup(object sender, EventArgs e)
    {
      AppItem curApp = this.GetSelectedAppItem();
      if ((curApp == null) || (curApp.SourceType == myProgSourceType.GROUPER))
      {
        this.MenuItemChangeSourceType.Enabled = false;
      }
      else
      {
        this.MenuItemChangeSourceType.Enabled = true;
      }

      this.menuItemReadFromProfile.Enabled = false;
      if ((curApp != null) && (curApp.ProfileLoadingAllowed()))
      {
        if (File.Exists(Config.Get(Config.Dir.Config) + "ProgramSettingProfiles.xml"))
        {
          this.menuItemReadFromProfile.Enabled = true;
          FillProfileMenu();
        }
      }

    }

    private void SourceTypeToDirBrowse_Click(object sender, EventArgs e)
    {
      this.DoModifySourceType(myProgSourceType.DIRBROWSE);
    }

    private void SourceTypeToDirCache_Click(object sender, EventArgs e)
    {
      this.DoModifySourceType(myProgSourceType.DIRCACHE);
    }

    private void SourceTypeToMyIni_Click(object sender, EventArgs e)
    {
      this.DoModifySourceType(myProgSourceType.MYFILEINI);
    }

    private void SourceTypeToMLF_Click(object sender, EventArgs e)
    {
      this.DoModifySourceType(myProgSourceType.MYFILEMEEDIO);
    }

    private void SourceTypeToGamebase_Click(object sender, EventArgs e)
    {
      this.DoModifySourceType(myProgSourceType.GAMEBASE);
    }

    private void SourceTypeToFilelauncher_Click(object sender, EventArgs e)
    {
      this.DoModifySourceType(myProgSourceType.FILELAUNCHER);
    }


    private void MenuItemChangeSourceType_Select(object sender, EventArgs e)
    {
      AppItem curApp = this.GetSelectedAppItem();
      if (curApp == null)
      {
        return;
      }

      this.SourceTypeToDirBrowse.Enabled = true;
      this.SourceTypeToDirCache.Enabled = true;
      this.SourceTypeToMyIni.Enabled = true;
      this.SourceTypeToMLF.Enabled = true;
      this.SourceTypeToFilelauncher.Enabled = true;
      this.SourceTypeToGamebase.Enabled = true;

      switch (curApp.SourceType)
      {
        case myProgSourceType.DIRBROWSE:
          SourceTypeToDirBrowse.Enabled = false;
          break;
        case myProgSourceType.DIRCACHE:
          this.SourceTypeToDirCache.Enabled = false;
          break;
        case myProgSourceType.MYFILEINI:
          this.SourceTypeToMyIni.Enabled = false;
          break;
        case myProgSourceType.MYFILEMEEDIO:
          this.SourceTypeToMLF.Enabled = false;
          break;
        case myProgSourceType.FILELAUNCHER:
          this.SourceTypeToFilelauncher.Enabled = false;
          break;
        case myProgSourceType.GAMEBASE:
          this.SourceTypeToGamebase.Enabled = false;
          break;
      }
    }

    private void DetailsTabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
      // save current app if switching to file-tab
      if (detailsTabControl.SelectedIndex == 1)
      {
        if (!SaveAppItem())
        {
          detailsTabControl.SelectedIndex = 0;
        }
      }
    }

    private void FillProfileMenu()
    {
      if (!m_ProfilesLoaded)
      {
        menuItemReadFromProfile.MenuItems.Clear();

        XmlDocument document = new XmlDocument();
        document.Load(Config.Get(Config.Dir.Config) + "ProgramSettingProfiles.xml");
        XmlElement rootElement = document.DocumentElement;

        if (rootElement != null && rootElement.Name.Equals("profiles"))
        {
          //
          // Fetch global actions
          //
          XmlNodeList nodeList = rootElement.SelectNodes("/profiles/profile");

          foreach (XmlNode node in nodeList)
          {
            XmlNode idNode = node.SelectSingleNode("id");
            XmlNode titleNode = node.SelectSingleNode("title");

            taggedMenuItem newMenu = new taggedMenuItem(titleNode.InnerText);
            newMenu.Tag = Convert.ToInt32(idNode.InnerText.Length > 0 ? idNode.InnerText : "0");
            newMenu.XmlTag = node;
            newMenu.Click += new EventHandler(this.ProfileItem_Click);
            menuItemReadFromProfile.MenuItems.Add(newMenu);
          }
        }

        m_ProfilesLoaded = true;
      }
    }


    private void ProfileItem_Click(object sender, EventArgs e)
    {
      AppItem curApp = GetSelectedAppItem();
      XmlNode profileNode = ((taggedMenuItem)sender).XmlTag;
      pageCurrentSettings = GetCurrentSettingsPage();
      //Log.Info("huhu1");
      if (pageCurrentSettings != null)
      {
        // create a pseudo-appitem and read the values from the xml node
        AppItem tempApp = new AppItem(curApp.db);
        tempApp.LoadFromXmlProfile(profileNode);
        pageCurrentSettings.LoadFromAppItem(tempApp);
      }
    }

  }
}
