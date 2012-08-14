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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  public class GUISettingsMusicDatabase : GUIInternalWindow
  {
    [SkinControl(2)] protected GUIListControl lcFolders = null;

    [SkinControl(3)] protected GUICheckButton btnStripartistprefixes = null; // Done
    [SkinControl(4)] protected GUICheckButton btnTreatFolderAsAlbum = null; // Done

    [SkinControl(5)] protected GUICheckButton btnUseFolderThumbs = null; //Done
    [SkinControl(6)] protected GUICheckButton btnUseAllImages = null; //Done
    [SkinControl(7)] protected GUICheckButton btnExtractthumbs = null; //Done
    [SkinControl(8)] protected GUICheckButton btnCreategenrethumbs = null; //Done
    [SkinControl(9)] protected GUICheckButton btnCreateartistthumbs = null; //Done
    [SkinControl(10)] protected GUICheckButton btnCreateMissingFolderThumbs = null; //Done

    [SkinControl(11)] protected GUIButtonControl btnDateAdded = null; // Done

    [SkinControl(12)] protected GUICheckButton btnUpdateSinceLastImport = null;
    [SkinControl(13)] protected GUICheckButton btnMonitorShares = null; // DOne

    [SkinControl(14)] protected GUIButtonControl btnUpdateDatabase = null; // Done

    private MusicDatabase m_dbs = MusicDatabase.Instance;
    
    private string _updateSinceLastImport = string.Empty;
    private string _prefixes = string.Empty;
    private int _dateAddedSelectedIndex = 0;
    private ArrayList _dateAdded = new ArrayList();
    private Thread _scanThread = null;
    private bool _scanRunning = false;
    private int _scanShare = 0;

    private String _defaultShare;
    private bool _rememberLastFolder;
    private bool _addOpticalDiskDrives;
    private bool _autoSwitchRemovableDrives;

    private ShareData FolderInfo(GUIListItem item)
    {
      ShareData folderInfo = item.AlbumInfoTag as ShareData;
      return folderInfo;
    }

    public GUISettingsMusicDatabase()
    {
      GetID = (int)Window.WINDOW_SETTINGS_MUSICDATABASE; //1011
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\Settings_MyMusic_Database.xml"));
    }

    #region Serialization

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        btnExtractthumbs.Selected = xmlreader.GetValueAsBool("musicfiles", "extractthumbs", false);
        btnCreateartistthumbs.Selected = xmlreader.GetValueAsBool("musicfiles", "createartistthumbs", false);
        btnCreategenrethumbs.Selected = xmlreader.GetValueAsBool("musicfiles", "creategenrethumbs", true);
        btnUseFolderThumbs.Selected = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", true);
        btnUseAllImages.Selected = xmlreader.GetValueAsBool("musicfiles", "useAllImages",
                                                             btnUseFolderThumbs.Selected);
        btnTreatFolderAsAlbum.Selected = xmlreader.GetValueAsBool("musicfiles", "treatFolderAsAlbum", false);

        if (btnTreatFolderAsAlbum.Selected)
        {
          btnCreateMissingFolderThumbs.IsEnabled = true;
        }
        else
        {
          btnCreateMissingFolderThumbs.IsEnabled = false;
        }

        btnCreateMissingFolderThumbs.Selected = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs",
                                                                     btnTreatFolderAsAlbum.Selected);
        btnMonitorShares.Selected = xmlreader.GetValueAsBool("musicfiles", "monitorShares", false);
        btnUpdateSinceLastImport.Selected = xmlreader.GetValueAsBool("musicfiles", "updateSinceLastImport", true);
        _updateSinceLastImport = String.Format("Only update files after {0}",
                                                           xmlreader.GetValueAsString("musicfiles", "lastImport",
                                                                                      "1900-01-01 00:00:00"));
        btnStripartistprefixes.Selected = xmlreader.GetValueAsBool("musicfiles", "stripartistprefixes", false);
        _prefixes = xmlreader.GetValueAsString("musicfiles", "artistprefixes", "The, Les, Die");
        _dateAddedSelectedIndex = xmlreader.GetValueAsInt("musicfiles", "dateadded", 0);

        lcFolders.Clear();
        _scanShare = 0;
        SettingsSharesHelper settingsSharesHelper = new SettingsSharesHelper();
        // Load share settings
        settingsSharesHelper.LoadSettings("music");
        
        foreach (GUIListItem item in settingsSharesHelper.ShareListControl)
        {
          string driveLetter = FolderInfo(item).Folder.Substring(0, 3).ToUpper();

          if (Util.Utils.getDriveType(driveLetter) == 3 ||
              Util.Utils.getDriveType(driveLetter) == 4)
          {
            item.IsPlayed = false;

            if (FolderInfo(item).ScanShare)
            {
              item.IsPlayed = true;
              item.Label2 = GUILocalizeStrings.Get(193);
              _scanShare++;
            }
            item.OnItemSelected += OnItemSelected;
            item.Label = FolderInfo(item).Folder;
            
            item.Path = FolderInfo(item).Folder;
            lcFolders.Add(item);
          }
        }
        _defaultShare = xmlreader.GetValueAsString("music", "default", "");
        _rememberLastFolder = xmlreader.GetValueAsBool("music", "rememberlastfolder", false);
        _addOpticalDiskDrives = xmlreader.GetValueAsBool("music", "AddOpticalDiskDrives", true);
        _autoSwitchRemovableDrives = xmlreader.GetValueAsBool("music", "SwitchRemovableDrives", true);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("musicfiles", "extractthumbs", btnExtractthumbs.Selected);
        xmlwriter.SetValueAsBool("musicfiles", "createartistthumbs", btnCreateartistthumbs.Selected);
        xmlwriter.SetValueAsBool("musicfiles", "creategenrethumbs", btnCreategenrethumbs.Selected);
        xmlwriter.SetValueAsBool("musicfiles", "useFolderThumbs", btnUseFolderThumbs.Selected);
        xmlwriter.SetValueAsBool("musicfiles", "useAllImages", btnUseAllImages.Selected);
        xmlwriter.SetValueAsBool("musicfiles", "createMissingFolderThumbs", btnCreateMissingFolderThumbs.Selected);
        xmlwriter.SetValueAsBool("musicfiles", "treatFolderAsAlbum", btnTreatFolderAsAlbum.Selected);
        xmlwriter.SetValueAsBool("musicfiles", "monitorShares", btnMonitorShares.Selected);
        xmlwriter.SetValueAsBool("musicfiles", "updateSinceLastImport", btnUpdateSinceLastImport.Selected);
        xmlwriter.SetValueAsBool("musicfiles", "stripartistprefixes", btnStripartistprefixes.Selected);
        xmlwriter.SetValue("musicfiles", "artistprefixes", _prefixes);
        xmlwriter.SetValue("musicfiles", "dateadded", _dateAddedSelectedIndex);

        SettingsSharesHelper settingsSharesHelper = new SettingsSharesHelper();
        settingsSharesHelper.ShareListControl = lcFolders.ListItems;

        settingsSharesHelper.RememberLastFolder = _rememberLastFolder;
        settingsSharesHelper.AddOpticalDiskDrives = _addOpticalDiskDrives;
        settingsSharesHelper.SwitchRemovableDrives = _autoSwitchRemovableDrives;
        settingsSharesHelper.DefaultShare = _defaultShare;

        settingsSharesHelper.SaveSettings("music");
      }
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101011));
      _dateAdded.Clear();
      _dateAdded.AddRange(new object[]
                                      {
                                        "Current Date",
                                        "Creation Date",
                                        "Last Write Date"
                                      });
      LoadSettings();
      SetProperties();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == lcFolders)
      {
        if (lcFolders.SelectedListItem.IsPlayed)
        {
          lcFolders.SelectedListItem.Label2 = "";
          lcFolders.SelectedListItem.IsPlayed = false;
          FolderInfo(lcFolders.SelectedListItem).ScanShare = false;
          _scanShare--;
          
        }
        else
        {
          lcFolders.SelectedListItem.Label2 = GUILocalizeStrings.Get(193); // Scan
          lcFolders.SelectedListItem.IsPlayed = true;
          FolderInfo(lcFolders.SelectedListItem).ScanShare = true;
          _scanShare++;
       }
      }

      if (control == btnStripartistprefixes)
      {
        if (btnStripartistprefixes.Selected)
        {
          OnStripArtistsPrefixes();
          SettingsChanged(true);
        }
      }

      if (control == btnTreatFolderAsAlbum)
      {
        if (btnTreatFolderAsAlbum.Selected)
        {
          btnCreateMissingFolderThumbs.IsEnabled = true;
        }
        else
        {
          btnCreateMissingFolderThumbs.IsEnabled = false;
        }
        SettingsChanged(true);
      }

      if (control == btnDateAdded)
      {
        OnDateAdded();
      }

      if (control == btnUpdateDatabase)
      {
        if (_scanShare == 0)
        {
          GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(GUILocalizeStrings.Get(1020)); // Information
          dlgOk.SetLine(1, GUILocalizeStrings.Get(300004)); // Nothing to scan
          dlgOk.SetLine(2, GUILocalizeStrings.Get(300005)); //Please select folder(s) for scan.
          dlgOk.DoModal(GetID);
          return;
        }
        OnUpdateDatabase();
      }

    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    #endregion

    private void OnStripArtistsPrefixes()
    {
      string txt = _prefixes;
      GetStringFromKeyboard(ref txt);

      if (!string.IsNullOrEmpty(txt))
      {
        _prefixes = txt;
      }

      SetProperties();
    }

    private void OnDateAdded()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // Menu

      foreach (string option in _dateAdded)
      {
        dlg.Add(option);
      }

      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      _dateAddedSelectedIndex = dlg.SelectedLabel;
      
      SetProperties();
    }

    private void OnUpdateDatabase()
    {
      ThreadStart ts = new ThreadStart(FolderScanThread);
      _scanThread = new Thread(ts);
      _scanThread.Name = "MusicScan";
      _scanThread.Start();
    }
    
    private void FolderScanThread()
    {
      _scanRunning = true;
      ArrayList shares = new ArrayList();
      ArrayList scanShares = new ArrayList();
      
      foreach (GUIListItem item in lcFolders.ListItems)
      {
        if (item.IsPlayed)
        {
          scanShares.Add(item);
        }
      }

      for (int index = 0; index < _scanShare; index++)
      {
        GUIListItem item = (GUIListItem)scanShares[index];
        string path = item.Path;
        if (Directory.Exists(path))
        {
          try
          {
            string driveName = path.Substring(0, 1);
            if (path.StartsWith(@"\\"))
            {
              // we have the path in unc notation
              driveName = path;
            }

            ulong freeBytesAvailable = Util.Utils.GetFreeDiskSpace(driveName);

            if (freeBytesAvailable > 0)
            {
              ulong diskSpace = freeBytesAvailable / 1048576;
              if (diskSpace > 100) // > 100MB left for creation of thumbs, etc
              {
                Log.Info("MusicDatabase: adding share {0} for scanning - available disk space: {1} MB", path,
                         diskSpace.ToString());
                shares.Add(path);
              }
              else
              {
                Log.Warn("MusicDatabase: NOT scanning share {0} because of low disk space: {1} MB", path,
                         diskSpace.ToString());
              }
            }
          }
          catch (Exception)
          {
            // Drive not ready, etc
          }
        }
      }
      MusicDatabase.DatabaseReorgChanged += new MusicDBReorgEventHandler(SetStatus);
      EnableControls(false);
      // Now create a Settings Object with the Settings checked to pass to the Import
      MusicDatabaseSettings setting = new MusicDatabaseSettings();
      setting.CreateMissingFolderThumb = btnCreateMissingFolderThumbs.Selected;
      setting.ExtractEmbeddedCoverArt = btnUseAllImages.Selected;
      setting.StripArtistPrefixes = btnStripartistprefixes.Selected;
      setting.TreatFolderAsAlbum = btnTreatFolderAsAlbum.Selected;
      setting.UseFolderThumbs = btnUseFolderThumbs.Selected;
      setting.UseAllImages = btnUseAllImages.Selected;
      setting.CreateArtistPreviews = btnCreateartistthumbs.Selected;
      setting.CreateGenrePreviews = btnCreategenrethumbs.Selected;
      setting.UseLastImportDate = btnUpdateSinceLastImport.Selected;
      setting.ExcludeHiddenFiles = false;
      setting.DateAddedValue = _dateAddedSelectedIndex;

      try
      {
        m_dbs.MusicDatabaseReorg(shares, setting);
      }
      catch (Exception ex)
      {
        Log.Error("Folder Scan: Exception during processing: ", ex.Message);
        _scanRunning = false;
      }

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _updateSinceLastImport = String.Format("Only update files after {0}",
                                                           xmlreader.GetValueAsString("musicfiles", "lastImport",
                                                                                      "1900-01-01 00:00:00"));
      }

      _scanRunning = false;
      EnableControls(true);
      SetProperties();

      GUIDialogNotify dlgNotify =
        (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (null != dlgNotify)
      {
        dlgNotify.SetHeading(GUILocalizeStrings.Get(1020)); // Information
        dlgNotify.SetText(GUILocalizeStrings.Get(300024)); // Scan finished
        dlgNotify.DoModal(GetID);
      }
    }

    private void SetStatus(object sender, DatabaseReorgEventArgs e)
    {
      GUIPropertyManager.SetProperty("#scanstatus", e.phase);
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

    private void SetProperties()
    {
      GUIPropertyManager.SetProperty("#dateadded", GUILocalizeStrings.Get(300025) + " " + (string)_dateAdded[_dateAddedSelectedIndex]);
      GUIPropertyManager.SetProperty("#prefixes", _prefixes);
      GUIPropertyManager.SetProperty("#updatesincelastimprot", _updateSinceLastImport);
      GUIPropertyManager.SetProperty("#scanstatus", string.Empty);
    }
    
    private void EnableControls(bool enable)
    {
      if (enable)
      {
        lcFolders.IsEnabled = true;
        btnStripartistprefixes.IsEnabled = true;
        btnTreatFolderAsAlbum.IsEnabled = true;
        //
        btnUseFolderThumbs.IsEnabled = true;
        btnUseAllImages.IsEnabled = true;
        btnExtractthumbs.IsEnabled = true;
        btnCreategenrethumbs.IsEnabled = true;
        btnCreateartistthumbs.IsEnabled = true;
        btnCreateMissingFolderThumbs.IsEnabled = true;
        //
        btnDateAdded.IsEnabled = true;
        //
        btnUpdateSinceLastImport.IsEnabled = true;
        btnMonitorShares.IsEnabled = true;
        //
        btnUpdateDatabase.IsEnabled = true;
      }
      else
      {
        lcFolders.IsEnabled = false;
        btnStripartistprefixes.IsEnabled = false;
        btnTreatFolderAsAlbum.IsEnabled = false;
        //
        btnUseFolderThumbs.IsEnabled = false;
        btnUseAllImages.IsEnabled = false;
        btnExtractthumbs.IsEnabled = false;
        btnCreategenrethumbs.IsEnabled = false;
        btnCreateartistthumbs.IsEnabled = false;
        btnCreateMissingFolderThumbs.IsEnabled = false;
        //
        btnDateAdded.IsEnabled = false;
        //
        btnUpdateSinceLastImport.IsEnabled = false; ;
        btnMonitorShares.IsEnabled = false;
        //
        btnUpdateDatabase.IsEnabled = false;
      }
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item != null)
      {

      }
    }

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
  }
}
