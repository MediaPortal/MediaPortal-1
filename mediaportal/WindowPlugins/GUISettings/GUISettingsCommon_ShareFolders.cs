#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIShareFolders : GUIInternalWindow
  {
    [SkinControl(3)] protected GUIButtonControl btnAdd = null;
    [SkinControl(4)] protected GUIButtonControl btnRemove = null;
    [SkinControl(5)] protected GUIButtonControl btnEdit = null;
    [SkinControl(6)] protected GUIButtonControl btnReset = null;

    [SkinControl(40)] protected GUICheckButton btnRemeberLastFolder = null;
    [SkinControl(41)] protected GUICheckButton btnAddOpticalDrives = null;
    [SkinControl(42)] protected GUICheckButton btnAutoSwitchRemovables = null;
    [SkinControl(43)] protected GUIListControl videosShareListcontrol = null;

    private ArrayList _layouts = new ArrayList(); // Layouts for shares
    private GUIListItem _shareFolderListItem = new GUIListItem(); // Selected share from listcontrol
    // Fields for current or new folder
    private string _folderName = string.Empty; // Selected or new directory name
    private string _folderPin = string.Empty; // Selected or new directory PIN
    private string _folderPath = string.Empty; // Selected or new directory path

    private bool _folderCreateThumbs = true; // Future use
    private bool _folderEachFolderIsMovie = false; // Future use

    private string _folderDefaultLayout = "List";
    private int _folderDefaultLayoutIndex = 0; // Currrent selected folder layout
    private string _section = string.Empty;
    private bool _error = false; // error check flag (prevents save of bad folder data)
    // Folder browser
    private ArrayList _folders = new ArrayList(); // Collection of local drives
    private string _userNetFolder = string.Empty; // user defined network resource
    private ArrayList _folderHistory = new ArrayList(); // Holds directory items from directoryBrowserGUILisCtrl
    private Int32 _folderLvl = 0; // Current directory lvl in directory browser

    private int _selectedOption;
    private int _selectedLabelIndex;
    private string _defaultShare = string.Empty;
    private bool _globalVideoThumbsEnaled;

    // Comparer of GUIListItems by label
    private class GUIListItemSort : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        GUIListItem info1 = (GUIListItem)x;
        GUIListItem info2 = (GUIListItem)y;
        return String.Compare(info1.Label, info2.Label, true);
      }

      #endregion
    }

    private ShareData FolderInfo(GUIListItem item)
    {
      ShareData folderInfo = item.AlbumInfoTag as ShareData;
      return folderInfo;
    }

    public GUIShareFolders()
    {
      GetID = (int)Window.WINDOW_SETTINGS_FOLDERS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_Common_ShareFolders.xml");
    }

    public string Section
    {
      get { return _section; }
      set { _section = value; }
    }

    #region serialization

    private void LoadSettings()
    {
      videosShareListcontrol.Clear();
      SettingsSharesHelper settingsSharesHelper = new SettingsSharesHelper();
      // Load share settings
      settingsSharesHelper.LoadSettings(_section);

      // ToggleButtons
      btnAddOpticalDrives.Selected = settingsSharesHelper.AddOpticalDiskDrives;
      btnRemeberLastFolder.Selected = settingsSharesHelper.RememberLastFolder;
      btnAutoSwitchRemovables.Selected = settingsSharesHelper.SwitchRemovableDrives;


      foreach (var item in settingsSharesHelper.ShareListControl)
      {
        item.OnItemSelected += OnItemSelected;
        videosShareListcontrol.Add(item);

        if (item.IsPlayed)
        {
          _defaultShare = FolderInfo(item).Name;
        }
      }

      Sort();

      if (videosShareListcontrol.Count > 0)
      {
        videosShareListcontrol.SelectedListItemIndex = 0;
        _shareFolderListItem = videosShareListcontrol.SelectedListItem;
        _folderDefaultLayoutIndex = SettingsSharesHelper.ProperDefaultFromLayout(FolderInfo(_shareFolderListItem).DefaultLayout);
      }

      using (Profile.Settings xmlreader = new MPSettings())
      {
        _globalVideoThumbsEnaled = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
      }
    }

    private void SaveSettings()
    {
      // Sort by folder name (GUIListItem.label)
      Sort();

      // Toggle buttons
      SettingsSharesHelper settingsSharesHelper = new SettingsSharesHelper();
      settingsSharesHelper.AddOpticalDiskDrives = btnAddOpticalDrives.Selected;
      settingsSharesHelper.RememberLastFolder = btnRemeberLastFolder.Selected;
      settingsSharesHelper.SwitchRemovableDrives = btnAutoSwitchRemovables.Selected;
      settingsSharesHelper.DefaultShare = _defaultShare;

      settingsSharesHelper.ShareListControl = videosShareListcontrol.ListItems;
      settingsSharesHelper.SaveSettings(_section);
    }

    #endregion

    #region overrides

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      
      string module = string.Empty;
      switch (_section)
      {
        case "movies":
          module = GUILocalizeStrings.Get(300050);//Videos - Folders
          break;
        case "music":
          module = GUILocalizeStrings.Get(300051); //Music - Folders
          break;
        case "pictures":
          module = GUILocalizeStrings.Get(300052); //Pictures - Folders
          break;
      }
      GUIPropertyManager.SetProperty("#currentmodule", module);

      _layouts.Clear();
      _layouts.AddRange(new object[]
                                      {
                                        "List",
                                        "Small Icons",
                                        "Big Icons",
                                        "Big Icons List",
                                        "Filmstrip",
                                        "Cover Flow"
                                      });
      LoadSettings();
      Update();
      SetProperties();
      _userNetFolder = GUILocalizeStrings.Get(145); // Network
      _folderHistory = new ArrayList();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      // Default folder (select/deselect on click) - settings saved
      if (control == videosShareListcontrol)
      {
        OnSetDefaultFolder();
      }
      // Add new folder
      if (control == btnAdd)
      {
        // reset menu position
        _selectedOption = -1;
        // reset folder browser
        ClearFolders();
        // Clear folder info
        ClearFolderInfoData();
        _userNetFolder = GUILocalizeStrings.Get(145); // Network

        // Show menu
        OnAddEditFolder();

        // Define new folder
        GUIListItem item = new GUIListItem();
        // Watch for last parameter (my version with selective thumbs will use that)
        ShareData shareData = new ShareData("", "", "", true);
        item.AlbumInfoTag = shareData;
        _shareFolderListItem = item;
        // Check new data
        CheckCurrentShareData();

        if (_error)
        {
          _error = false;
          GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(257);
          dlgOk.SetLine(1, GUILocalizeStrings.Get(300053)); // Error in folder data
          dlgOk.SetLine(2, GUILocalizeStrings.Get(300054)); // Name or path couldn't be empty
          dlgOk.DoModal(GUIWindowManager.ActiveWindow);
          return;
        }

        // Prepare folder info data
        FolderInfo(_shareFolderListItem).DefaultLayout = SettingsSharesHelper.ProperLayoutFromDefault(_folderDefaultLayoutIndex);
        FolderInfo(_shareFolderListItem).Name = _folderName;
        FolderInfo(_shareFolderListItem).Folder = _folderPath;
        FolderInfo(_shareFolderListItem).CreateThumbs = _folderCreateThumbs;
        FolderInfo(_shareFolderListItem).EachFolderIsMovie = _folderEachFolderIsMovie;
        FolderInfo(_shareFolderListItem).PinCode = _folderPin;
        // Almost forgot this, needed for proper sort :)
        _shareFolderListItem.Label = _folderName;
        _shareFolderListItem.OnItemSelected += OnItemSelected;

        // Add new folder in list
        videosShareListcontrol.Add(_shareFolderListItem);
        GUIListItem newItem = _shareFolderListItem;
        Sort();
        int index = videosShareListcontrol.ListItems.IndexOf(newItem);
        videosShareListcontrol.SelectedListItemIndex = index;
      }
      // Edit folder
      if (control == btnEdit)
      {
        string name = _folderName;
        string path = _folderPath;

        // reset menu position
        _selectedOption = -1;

        OnAddEditFolder();

        // Check new data
        CheckCurrentShareData();

        if (_error)
        {
          _error = false;
          GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(257);
          dlgOk.SetLine(1, GUILocalizeStrings.Get(300053)); // Error in folder data
          dlgOk.SetLine(2, GUILocalizeStrings.Get(300054)); // Name or path couldn't be empty
          dlgOk.DoModal(GUIWindowManager.ActiveWindow);
          _folderName = name;
          _folderPath = path;
          return;
        }
        
        // Update changes
        FolderInfo(_shareFolderListItem).Name = _folderName;
        _shareFolderListItem.Label = _folderName;
        FolderInfo(_shareFolderListItem).Folder = _folderPath;
        FolderInfo(_shareFolderListItem).PinCode = _folderPin;
        FolderInfo(_shareFolderListItem).CreateThumbs = _folderCreateThumbs;
        FolderInfo(_shareFolderListItem).EachFolderIsMovie = _folderEachFolderIsMovie;
        FolderInfo(_shareFolderListItem).DefaultLayout = SettingsSharesHelper.ProperLayoutFromDefault(_folderDefaultLayoutIndex);
        // Add changes to a listitem
        videosShareListcontrol.SelectedListItem.AlbumInfoTag = _shareFolderListItem.AlbumInfoTag;
        videosShareListcontrol.SelectedListItem.Label = _folderName;
        // Sort list
        GUIListItem newItem = _shareFolderListItem;
        Sort();
        int index = videosShareListcontrol.ListItems.IndexOf(newItem);
        videosShareListcontrol.SelectedListItemIndex = index;
      }
      // Remove folder - settings saved
      if (control == btnRemove)
      {
        OnRemoveFolder();
      }
      // Reset folders - settings saved
      if (control == btnReset)
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
        if (dlgYesNo != null)
        {
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(927)); // Warning
          dlgYesNo.SetLine(1, GUILocalizeStrings.Get(300055)); // this will delete folders
          dlgYesNo.DoModal(GetID);

          if (dlgYesNo.IsConfirmed)
          {
            OnResetFolders();
          }
        }
      }
    }

    #endregion

    // Set current or new directory information
    private void Update()
    {
      _folderName = FolderInfo(_shareFolderListItem).Name;
      _folderPath = FolderInfo(_shareFolderListItem).Folder;
      _folderPin = FolderInfo(_shareFolderListItem).PinCode;
      _folderCreateThumbs = FolderInfo(_shareFolderListItem).CreateThumbs;
      _folderEachFolderIsMovie = FolderInfo(_shareFolderListItem).EachFolderIsMovie;
      _folderDefaultLayout = FolderInfo(_shareFolderListItem).DefaultLayout.ToString();
      _folderDefaultLayoutIndex = SettingsSharesHelper.ProperDefaultFromLayout(FolderInfo(_shareFolderListItem).DefaultLayout);

    }

    // Skin properties update
    private void SetProperties()
    {
      GUIPropertyManager.SetProperty("#folderName", _folderName);
      GUIPropertyManager.SetProperty("#folder", _folderPath);
      
      if (!string.IsNullOrEmpty(_folderPin))
      {
        GUIPropertyManager.SetProperty("#pinCode", "****");
      }
      else
      {
        GUIPropertyManager.SetProperty("#pinCode", "");
      }
      
      GUIPropertyManager.SetProperty("#layout", _folderDefaultLayout);

      if (_section == "movies")
      {
        string strValue = string.Empty;
        
        if (_globalVideoThumbsEnaled)
        {
          strValue = GUILocalizeStrings.Get(200032);
          
          if (_folderCreateThumbs)
          {
            strValue = GUILocalizeStrings.Get(200031);
          }
          GUIPropertyManager.SetProperty("#createThumb", strValue);
        }
        else
        {
          GUIPropertyManager.SetProperty("#createThumb", strValue = GUILocalizeStrings.Get(300224)); // Disabled globally
        }
        
        strValue = GUILocalizeStrings.Get(200032);
        
        if (_folderEachFolderIsMovie)
        {
          strValue = GUILocalizeStrings.Get(200031);
        }
        GUIPropertyManager.SetProperty("#eachFolderIsMovie", strValue);
      }
      else
      {
        GUIPropertyManager.SetProperty("#createThumb", GUILocalizeStrings.Get(394)); // N/A
        GUIPropertyManager.SetProperty("#eachFolderIsMovie", GUILocalizeStrings.Get(394));
      }
    }

    /// <summary>
    /// Get user txt. If maxLenght >0, string is limited to that value.
    /// </summary>
    /// <param name="strLine"></param>
    /// <param name="maxLenght">String lenght limitation. Less than 1 - No limit.</param>
    private void GetStringFromKeyboard(ref string strLine, int maxLenght)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return;
      }
      keyboard.Reset();
      keyboard.Text = strLine;

      if (maxLenght > 0)
      {
        keyboard.SetMaxLength(maxLenght);
      }

      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item != null)
      {
        _shareFolderListItem = item;
        Update();
        SetProperties();
      }
    }
    
    // Clear folder info data (needed for adding new folder)
    private void ClearFolderInfoData()
    {
      _folderName = string.Empty;
      _folderPath = string.Empty;
      _folderEachFolderIsMovie = false;
      _folderCreateThumbs = false;
      _folderDefaultLayout = (string)_layouts[0];
      _folderDefaultLayoutIndex = 0;
      _folderPin = string.Empty;
    }

    // Extra check before save settings (prevents saving wrong data for folders)
    private void CheckCurrentShareData()
    {
      if (string.IsNullOrEmpty(_folderName))
      {
        _error = true;
      }
      if (string.IsNullOrEmpty(_folderPath))
      {
        _error = true;
      }
      if (string.IsNullOrEmpty(_folderDefaultLayout))
      {
        _error = true;
      }
    }

    // Reset directoryBrowser and parameters related to directoryBrowser
    private void ClearFolders()
    {
      _folderLvl = 0;
      _folderHistory.Clear();
      _folders.Clear();
    }

    // Folders browser
    private void GetDrives()
    {
      ArrayList logicalDrives = new ArrayList();
      logicalDrives.AddRange(Environment.GetLogicalDrives());

      foreach (string logicalDrive in logicalDrives)
      {
        GUIListItem drive = new GUIListItem();
        drive.Label = logicalDrive;
        drive.Label2 = logicalDrive;
        _folders.Add(drive);
      }

      GUIListItem networkDrive = new GUIListItem();
      networkDrive.Label = _userNetFolder;
      networkDrive.Label2 = _userNetFolder;
      _folders.Add(networkDrive);
    }

    private void GetFolders(GUIListItem selectedItem)
    {
      try
      {
        // Network first start
        if (selectedItem.Label2 == GUILocalizeStrings.Get(145))
        {
          string netShare = @"\\";
          GetNetworkFolders(netShare);
          return;
        }

        // Check for browsing entered network resource e.g.: \\myNeSrv
        if (selectedItem.Label2.StartsWith(@"\\"))
        {
          if (selectedItem.Label2.LastIndexOf(@"\") < 2)
          {
            GetNetworkFolders(selectedItem.Label2);
          }
        }

        if (selectedItem.Label2 == "..")
        {
          GetFolderHistory();
          return;
        }
        else
        {
          // Go to subdirectories
          string[] directories = Directory.GetDirectories(selectedItem.Label2);
          SetFolders(directories);
        }
      }
      catch (Exception)
      {

      }
    }

    /// <summary>
    /// Network share enumeration from netShare parameter resource
    /// </summary>
    /// <param name="netShare">Network resource</param>
    private void GetNetworkFolders(string netShare)
    {
      ArrayList netComputers = NetShareCollection.GetComputersOnNetwork();
      
      if (netComputers == null || netComputers.Count == 0)
      {
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        dlgOk.SetHeading(GUILocalizeStrings.Get(1020));
        dlgOk.SetLine(1, GUILocalizeStrings.Get(300056)); //No network resources found.
        dlgOk.SetLine(2, GUILocalizeStrings.Get(300057)); // Try manual search.
        dlgOk.DoModal(GetID);

        GetStringFromKeyboard(ref netShare, -1);
      }
      else
      {
        GUIDialogSelect dlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(924); // Menu
        dlg.EnableButton(true);
        dlg.SetButtonLabel(413); // manual

        // Add list to dlg menu
        foreach (string netWrkst in netComputers)
        {
          dlg.Add(netWrkst);
        }
        // Show dialog menu
        dlg.DoModal(GetID);

        if (dlg.IsButtonPressed)
        {
          GetStringFromKeyboard(ref netShare, -1);
        }
        else if (dlg.SelectedLabel == -1)
        {
          return;
        }
        else
        {
          netShare = dlg.SelectedLabelText;
        }
      }

      if (string.IsNullOrEmpty(netShare) || !netShare.StartsWith(@"\\") || (netShare.StartsWith(@"\\") && netShare.Length <= 2))
      {
        netShare = GUILocalizeStrings.Get(145);
        return;
      }
      // Get selected network resource shared folders
      _userNetFolder = netShare;
      NetShareCollection netShares = NetShareCollection.GetShares(netShare);

      SetFolderHistory();
      _folders.Clear();
      GUIListItem goBack = new GUIListItem();
      goBack.Label = "..";
      goBack.Label2 = "..";
      _folders.Add(goBack);

      foreach (NetShare share in netShares)
      {
        if (share.IsFileSystem && share.ShareType == ShareType.Disk)
        {
          GUIListItem netFolder = new GUIListItem();
          string nFolder = Path.GetFileName(share.Root.FullName);
          netFolder.Label = nFolder.ToUpperInvariant();
          netFolder.Label2 = share.Root.FullName;
          _folders.Add(netFolder);
        }
      }
    }

    /// <summary>
    /// Add directories as GUIListItems into folderBrowser GUIListControl.
    /// System and hidden folders will be filtered out.
    /// </summary>
    /// <param name="directories"></param>
    private void SetFolders(string[] directories)
    {
      SetFolderHistory();
      _folders.Clear();
      GUIListItem goBack = new GUIListItem();
      goBack.Label = "..";
      goBack.Label2 = "..";
      _folders.Add(goBack);

      foreach (string dir in directories)
      {
        DirectoryInfo di = new DirectoryInfo(dir);

        if ((di.Attributes & FileAttributes.Directory) == FileAttributes.Directory &&
            (di.Attributes & FileAttributes.System) != FileAttributes.System &&
            (di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
        {
          GUIListItem folder = new GUIListItem();
          string path = Path.GetFileName(dir);
          if (path != null) folder.Label = path.ToUpperInvariant();
          folder.Label2 = dir;
          _folders.Add(folder);
        }
      }
    }

    /// <summary>
    /// // Add current GUIListItems directories from folderBrowser in directory history ArrayList
    /// </summary>
    private void SetFolderHistory()
    {
      if (_folderLvl == 0)
      {
        _folderHistory.Clear();
      }

      foreach (GUIListItem item in _folders)
      {
        item.IsPlayed = false;
        item.Duration = _folderLvl; // holds directory lvl

        if (_folders.IndexOf(item) == _selectedLabelIndex)
        {
          item.IsPlayed = true;
        }

        _folderHistory.Add(item);
      }
      _folderLvl++;
    }

    /// <summary>
    /// Get folder history. Items equal and above current directory lvl will be deleted from history.
    /// </summary>
    private void GetFolderHistory()
    {
      _folderLvl--;
      _folders.Clear();

      ArrayList tmp = new ArrayList(_folderHistory);

      foreach (GUIListItem item in tmp)
      {
        int itemLvl = item.Duration;

        if (itemLvl == _folderLvl)
        {
          _folders.Add(item);
        }

        if (itemLvl >= _folderLvl)
        {
          _folderHistory.Remove(item);
        }
      }
    }

    #region Set default folder

    private void OnSetDefaultFolder()
    {
      if (videosShareListcontrol.SelectedListItem.IsPlayed)
      {
        videosShareListcontrol.SelectedListItem.IsPlayed = false;
        _defaultShare = string.Empty;
        return;
      }

      foreach (GUIListItem share in videosShareListcontrol.ListItems)
      {
        share.IsPlayed = false;
      }

      videosShareListcontrol.SelectedListItem.IsPlayed = true;
      _defaultShare = _folderName;
    }

    #endregion

    #region AddEdit Folder

    // Main folders (drives, net shares)
    private void OnAddEditFolder()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // Menu

      dlg.AddLocalizedString(300009); // Name
      dlg.AddLocalizedString(300058); // Path
      if (_section == "movies")
      {
        if (_globalVideoThumbsEnaled)
        {
          dlg.AddLocalizedString(109); // Create thumbs
        }
        dlg.AddLocalizedString(300221); // Each folder is movie
      }
      //dlg.AddLocalizedString(1374); // layout
      dlg.AddLocalizedString(300059);// Pin

      if (_selectedOption != -1)
        dlg.SelectedLabel = _selectedOption;

      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        return;
      }

      _selectedOption = dlg.SelectedLabel;

      switch (dlg.SelectedId)
      {
        case 300009:
          OnAddName();
          break;
        case 300058:
          _selectedLabelIndex = -1;
          OnAddPath();
          break;
        case 109:
          OnThumb();
          break;
        case 300221:
          OnMovieFolder();
          break;
        //case 1374:
        //  OnAddLayout();
        //  break;
        case 300059:
          OnAddPin();
          break;
      }
    }

    // Sub methods

    private void OnAddName()
    {
      string sName = _folderName;
      GetStringFromKeyboard(ref sName, -1);

      // If user edit folder and did nothing, return
      if (sName == _folderName)
      {
        OnAddEditFolder();
        return;
      }

      if (!string.IsNullOrEmpty(sName))
      {
        // Don't allow empty or equal existing name
        foreach (GUIListItem item in videosShareListcontrol.ListItems)
        {
          if (FolderInfo(item).Name.Equals(sName, StringComparison.InvariantCultureIgnoreCase))
          {
            GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
            if (dlgOk != null)
            {
              dlgOk.SetHeading(GUILocalizeStrings.Get(257));
              dlgOk.SetLine(1, GUILocalizeStrings.Get(300013)); // Name can't be empty or must be unique!
              dlgOk.DoModal(GetID);
              OnAddEditFolder();
              return;
            }
          }
        }
        _folderName = sName;
      }

      OnAddEditFolder();
    }

    private void OnAddPath()
    {
      GUIDialogSelect dlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(300049); // Folder browser
      dlg.EnableButton(true);
      dlg.SetButtonLabel(424); // manual

      // Get drive list
      if (_folderLvl == 0)
      {
        ClearFolders();
        GetDrives();
      }
      // Add list to dlg menu
      foreach (GUIListItem drive in _folders)
      {
        dlg.Add(drive.Label);

        if (drive.IsPlayed)
        {
          dlg.SelectedLabel = _folders.IndexOf(drive);
        }
      }

      // Show dialog menu
      dlg.MarkSelectedItemOnButton = true;
      dlg.DoModal(GetID);

      // Folder is selected - return
      if (dlg.IsButtonPressed)
      {
        GUIListItem item = (GUIListItem)_folders[dlg.SelectedItemLabelIndexNoFocus];
        _folderPath = item.Label2;
        // Reset browsing history
        ClearFolders();
        OnAddEditFolder();
        return;
      }

      //ESC pressed, go back folder or if it's lvl=0 (Drives) return 
      if (dlg.SelectedLabel == -1)
      {
        if (_folderLvl > 0)
        {
          GetFolderHistory();
          OnAddPath();
          return;
        }
        else
        {
          ClearFolders();
          _userNetFolder = GUILocalizeStrings.Get(145); // Network
          OnAddEditFolder();
          return;
        }
      }

      // Browse folders further
      _selectedLabelIndex = dlg.SelectedLabel;
      GUIListItem selectedItem = (GUIListItem)_folders[dlg.SelectedItemLabelIndexNoFocus];
      GetFolders(selectedItem);
      OnAddPath();
    }

    private void OnAddLayout()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu

        foreach (string layout in _layouts)
        {
          dlg.Add(layout);
        }

        if (_folderDefaultLayoutIndex >= 0)
        {
          dlg.SelectedLabel = _folderDefaultLayoutIndex;
        }

        dlg.DoModal(GetID);

        if (dlg.SelectedId == -1)
        {
          OnAddEditFolder();
          return;
        }

        _folderDefaultLayout = dlg.SelectedLabelText;
        _folderDefaultLayoutIndex = dlg.SelectedLabel;
      }
      OnAddEditFolder();
    }

    private void OnAddPin()
    {
      GetStringFromKeyboard(ref _folderPin, 4);

      int number;
      if (!Int32.TryParse(_folderPin, out number))
      {
        _folderPin = string.Empty;
      }

      OnAddEditFolder();
    }

    private void OnThumb()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        OnAddEditFolder();
        return;
      }

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      int selected = 0;
      
      if (!_folderCreateThumbs)
      {
        selected = 1;
      }

      dlg.Add(GUILocalizeStrings.Get(200031)); //Yes
      dlg.Add(GUILocalizeStrings.Get(200032)); // No

      dlg.SelectedLabel = selected;

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel < 0)
      {
        OnAddEditFolder();
        return;
      }

      if (dlg.SelectedLabel == 0)
      {
        _folderCreateThumbs = true;
      }
      else
      {
        _folderCreateThumbs = false;
      }
      
      OnAddEditFolder();
    }

    private void OnMovieFolder()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        OnAddEditFolder();
        return;
      }

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      int selected = 1;

      if (_folderEachFolderIsMovie)
      {
        selected = 0;
      }

      dlg.Add(GUILocalizeStrings.Get(200031)); //Yes
      dlg.Add(GUILocalizeStrings.Get(200032)); // No

      dlg.SelectedLabel = selected;

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel < 0)
      {
        OnAddEditFolder();
        return;
      }

      if (dlg.SelectedLabel == 0)
      {
        _folderEachFolderIsMovie = true;
      }
      else
      {
        _folderEachFolderIsMovie = false;
      }

      OnAddEditFolder();
    }

    #endregion

    #region Remove folder

    private void OnRemoveFolder()
    {
      int sIndex = videosShareListcontrol.SelectedListItemIndex;
      videosShareListcontrol.RemoveItem(sIndex);

      SaveSettings();

      if (sIndex > 0)
      {
        sIndex--;
        videosShareListcontrol.SelectedListItemIndex = sIndex;
      }

      Update();
      SetProperties();
    }

    #endregion

    #region Reset folders

    private void OnResetFolders()
    {
      videosShareListcontrol.Clear();
      SettingsSharesHelper sh = new SettingsSharesHelper();
      sh.SetDefaultDrives(_section, btnAddOpticalDrives.Selected);
      LoadSettings();
      Update();
      SetProperties();
    }

    #endregion

    private void Sort()
    {
      ArrayList aItems = new ArrayList(videosShareListcontrol.ListItems);
      aItems.Sort(new GUIListItemSort());
      videosShareListcontrol.Clear();

      foreach (GUIListItem item in aItems)
      {
        videosShareListcontrol.Add(item);
      }
    }
  }
}