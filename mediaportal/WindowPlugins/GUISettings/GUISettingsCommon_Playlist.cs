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
using MediaPortal.GUI.Music;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  public class GUISettingsPlaylist : GUIInternalWindow
  {
    [SkinControl(2)] protected GUIButtonControl btnMusicplaylist= null;
    [SkinControl(3)] protected GUICheckButton btnMusicrepeatplaylist = null;
    [SkinControl(4)] protected GUICheckButton btnMusicautoshuffle = null;
    [SkinControl(5)] protected GUICheckButton btnMusicsavecurrentasdefault = null;
    [SkinControl(6)] protected GUICheckButton btnMusicloaddefault= null;
    [SkinControl(7)] protected GUICheckButton btnMusicplaylistscreen = null;

    [SkinControl(8)] protected GUIButtonControl btnVideosplaylist = null;
    [SkinControl(9)] protected GUICheckButton btnVideosrepeatplaylist = null;


    private string _musicPlayListFolder = string.Empty;
    private string _videosPlayListFolder = string.Empty;
    private string _section = string.Empty;

    // Folder browser
    private ArrayList _folders = new ArrayList(); // Collection of local drives
    private string _userNetFolder = string.Empty; // user defined network resource
    private ArrayList _folderHistory = new ArrayList(); // Holds directory items from directoryBrowserGUILisCtrl
    private Int32 _folderLvl = 0; // Current directory lvl in directory browser
    private int _selectedLabelIndex;

    
    public GUISettingsPlaylist()
    {
      GetID = (int)Window.WINDOW_SETTINGS_PLAYLIST;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_Common_Playlist.xml");
    }

    public string Section
    {
      get { return _section; }
      set { _section = value; }
    }

    #region serialization

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new MPSettings())
      {
        // Music
        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";
        _musicPlayListFolder = xmlreader.GetValueAsString("music", "playlists", playListFolder);

        if (string.Compare(_musicPlayListFolder, playListFolder) == 0)
        {
          if (Directory.Exists(playListFolder) == false)
          {
            try
            {
              Directory.CreateDirectory(playListFolder);
            }
            catch (Exception) { }
          }
        }
        
        btnMusicrepeatplaylist.Selected = xmlreader.GetValueAsBool("musicfiles", "repeat", false);
        btnMusicautoshuffle.Selected = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", false);
        btnMusicsavecurrentasdefault.Selected = xmlreader.GetValueAsBool("musicfiles", "savePlaylistOnExit", true);
        btnMusicloaddefault.Selected = xmlreader.GetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", true);
        btnMusicplaylistscreen.Selected= xmlreader.GetValueAsBool("musicfiles", "playlistIsCurrent", true);
        
        // Videos
        _videosPlayListFolder = xmlreader.GetValueAsString("movies", "playlists", playListFolder);
        btnVideosrepeatplaylist.Selected = xmlreader.GetValueAsBool("movies", "repeat", true);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new MPSettings())
      {
        // Music
        switch (_section)
        {
          case "music":
            xmlwriter.SetValue("music", "playlists", _musicPlayListFolder);
            xmlwriter.SetValueAsBool("musicfiles", "repeat", btnMusicrepeatplaylist.Selected);
            xmlwriter.SetValueAsBool("musicfiles", "autoshuffle", btnMusicautoshuffle.Selected);
            xmlwriter.SetValueAsBool("musicfiles", "savePlaylistOnExit", btnMusicsavecurrentasdefault.Selected);
            xmlwriter.SetValueAsBool("musicfiles", "resumePlaylistOnMusicEnter", btnMusicloaddefault.Selected);
            xmlwriter.SetValueAsBool("musicfiles", "playlistIsCurrent", btnMusicplaylistscreen.Selected );
            break;

          case "movies":
            xmlwriter.SetValue("movies", "playlists", _videosPlayListFolder);
            xmlwriter.SetValueAsBool("movies", "repeat", btnVideosrepeatplaylist.Selected);
            break;
        }
      }
    }

    #endregion

    #region overrides
    
    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      
      switch (_section)
      {
        case "music":
          GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(300045));
          break;

        case "movies":
          GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(300046));
          break;
      }

      LoadSettings();
      UpdateControls();
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
      if (control == btnVideosplaylist || control == btnMusicplaylist)
      {
        OnAddPath();
        SettingsChanged(true);
      }
      if (control == btnMusicautoshuffle || control == btnMusicloaddefault || control == btnMusicplaylistscreen ||
          control == btnMusicrepeatplaylist || control == btnMusicsavecurrentasdefault || 
          control == btnVideosrepeatplaylist)
      {
        SettingsChanged(true);
      }
    }

    #endregion

    // Skin properties update
    private void SetProperties()
    {
      GUIPropertyManager.SetProperty("#musicPlaylist", _musicPlayListFolder);
      GUIPropertyManager.SetProperty("#videosPlaylist", _videosPlayListFolder);

      switch (_section)
      {
        case "music":
          GUIPropertyManager.SetProperty("#playlisttype", GUILocalizeStrings.Get(300047));
          break;

        case "movies":
          GUIPropertyManager.SetProperty("#playlisttype", GUILocalizeStrings.Get(300048));
          break;
      }
    }

    private void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return;
      }
      keyboard.Reset();
      keyboard.Text = strLine;

      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
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

        GetStringFromKeyboard(ref netShare);
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
          GetStringFromKeyboard(ref netShare);
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
    
    #region Add folder

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
      }

      // Show dialog menu
      dlg.MarkSelectedItemOnButton = true;
      dlg.DoModal(GetID);

      // Folder is selected - return
      if (dlg.IsButtonPressed)
      {
        GUIListItem path = (GUIListItem)_folders[dlg.SelectedItemLabelIndexNoFocus];

        switch (_section)
        {
          case "music":
            _musicPlayListFolder = path.Label2;
            break;

          case "movies":
            _videosPlayListFolder = path.Label2;
            break;
        }
        
        // Reset browsing history
        ClearFolders();
        SetProperties();
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
          return;
        }
      }

      // Browse folders further
      _selectedLabelIndex = dlg.SelectedLabel;
      GUIListItem selectedItem = (GUIListItem)_folders[dlg.SelectedItemLabelIndexNoFocus];
      GetFolders(selectedItem);
      OnAddPath();
    }
    
    #endregion

    private void UpdateControls()
    {
      switch (_section)
      {
        case "music":
          btnVideosplaylist.IsVisible = false;
          btnVideosrepeatplaylist.IsVisible = false;

          btnMusicautoshuffle.IsVisible = true;
          btnMusicloaddefault.IsVisible = true;
          btnMusicplaylist.IsVisible = true;
          btnMusicplaylistscreen.IsVisible = true;
          btnMusicrepeatplaylist.IsVisible = true;
          btnMusicsavecurrentasdefault.IsVisible = true;

          btnMusicplaylist.Focus = true;
          btnVideosplaylist.Focus = false;

          break;

        case "movies":
          btnVideosplaylist.IsVisible = true;
          btnVideosrepeatplaylist.IsVisible = true;

          btnMusicautoshuffle.IsVisible = false;
          btnMusicloaddefault.IsVisible = false;
          btnMusicplaylist.IsVisible = false;
          btnMusicplaylistscreen.IsVisible = false;
          btnMusicrepeatplaylist.IsVisible = false;
          btnMusicsavecurrentasdefault.IsVisible = false;

          btnMusicplaylist.Focus = false;
          btnVideosplaylist.Focus = true;

          break;
      }
    }

    private void SettingsChanged(bool settingsChanged)
    {
      GUISettings.SettingsChanged = settingsChanged;
    }

  }
}