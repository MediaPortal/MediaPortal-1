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
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Picture.Database;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  public class GUISettingsPicturesDatabase : GUIInternalWindow
  {
    [SkinControl(2)] protected GUIListControl lcFolders = null;

    [SkinControl(3)] protected GUIButtonControl btnScanDatabase = null;
    [SkinControl(4)] protected GUIButtonControl btnResetDatabase = null;

    
    private bool _noLargeThumbnails;
    private int _scanShare = 0;
    private Thread _scanThread = null;

    private String _defaultShare;
    private bool _rememberLastFolder;
    private bool _addOpticalDiskDrives;
    private bool _autoSwitchRemovableDrives;

    private ShareData FolderInfo(GUIListItem item)
    {
      ShareData folderInfo = item.AlbumInfoTag as ShareData;
      return folderInfo;
    }

    public GUISettingsPicturesDatabase()
    {
      GetID = (int)Window.WINDOW_SETTINGS_PICTURESDATABASE; //1012
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_MyPictures_Database.xml"));
    }

    #region Serialization

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        _noLargeThumbnails = xmlreader.GetValueAsBool("thumbnails", "picturenolargethumbondemand", true);
        
        lcFolders.Clear();
        _scanShare = 0;
        SettingsSharesHelper settingsSharesHelper = new SettingsSharesHelper();
        // Load share settings
        settingsSharesHelper.LoadSettings("pictures");

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
              item.Label2 = GUILocalizeStrings.Get(193); // Scan
              _scanShare++;
            }
            item.OnItemSelected += OnItemSelected;
            item.Label = FolderInfo(item).Folder;

            item.Path = FolderInfo(item).Folder;
            lcFolders.Add(item);
          }
        }
        _defaultShare = xmlreader.GetValueAsString("pictures", "default", "");
        _rememberLastFolder = xmlreader.GetValueAsBool("pictures", "rememberlastfolder", false);
        _addOpticalDiskDrives = xmlreader.GetValueAsBool("pictures", "AddOpticalDiskDrives", true);
        _autoSwitchRemovableDrives = xmlreader.GetValueAsBool("pictures", "SwitchRemovableDrives", true);
      }
    }

    private void SaveSettings()
    {
      SettingsSharesHelper settingsSharesHelper = new SettingsSharesHelper();
      settingsSharesHelper.ShareListControl = lcFolders.ListItems;

      settingsSharesHelper.RememberLastFolder = _rememberLastFolder;
      settingsSharesHelper.AddOpticalDiskDrives = _addOpticalDiskDrives;
      settingsSharesHelper.SwitchRemovableDrives = _autoSwitchRemovableDrives;
      settingsSharesHelper.DefaultShare = _defaultShare;

      settingsSharesHelper.SaveSettings("pictures");
  }

    #endregion

    #region Overrides
    
    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101012));
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

      if (control == btnScanDatabase)
      {
        if (_scanShare == 0)
        {
          GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(GUILocalizeStrings.Get(1020)); // Information
          dlgOk.SetLine(1, GUILocalizeStrings.Get(300004)); // Nothing to scan
          dlgOk.SetLine(2, GUILocalizeStrings.Get(300005)); //Please select folder(s) for scan
          dlgOk.DoModal(GetID);
          return;
        }
        OnScanDatabase();
      }

      if (control == btnResetDatabase)
      {
        OnResetDatabase();
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
    
    private void OnScanDatabase()
    {
      ThreadStart ts = new ThreadStart(OnScanDatabaseThread);
      _scanThread = new Thread(ts);
      _scanThread.Name = "PicturesScan";
      _scanThread.Start();
    }

    private void OnScanDatabaseThread()
    {
      try 
      {
        ArrayList paths = new ArrayList();

        SetStatus(GUILocalizeStrings.Get(300061)); // Starting scan...

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
          string fullPath = item.Path;

          if (Directory.Exists(fullPath))
          {
            paths.Add(fullPath);
          }
        }

        // get all pictures from the path
        ArrayList availableFiles = new ArrayList();
        foreach (string path in paths)
        {
          CountFiles(path, ref availableFiles);
        }

        int count = 1;
        int totalFiles = availableFiles.Count;


        Log.Info("PictureDatabase: Beginning picture database reorganization and thumbnail generation...");

        // treat each picture file one by one
        EnableControls(false);
        foreach (string file in availableFiles)
        {
          Log.Info("Scanning file: {0}", file);
          // create thumb if not created and add file to db if not already there         
          CreateThumbsAndAddPictureToDB(file);
          SetStatus(String.Format("{0}/{1} thumbnails generated", count, totalFiles));
          count++;
        }

        Log.Info("PictureDatabase: Database reorganization and thumbnail generation finished");

        SetStatus(String.Format("Finished. {0} files processsed", totalFiles));
        EnableControls(true);

        GUIDialogNotify dlgNotify =
        (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (null != dlgNotify)
        {
          dlgNotify.SetHeading(GUILocalizeStrings.Get(1020)); // Information
          dlgNotify.SetText(GUILocalizeStrings.Get(300062)); // Scan finsihed
          dlgNotify.DoModal(GetID);
        }

      }
      catch (Exception)
      {}
    }

    private void CreateThumbsAndAddPictureToDB(string file)
    {
      int iRotate = PictureDatabase.GetRotation(file);
      if (iRotate == -1)
      {
        Log.Debug("PictureDatabase: Database is not available. File {0} has not been added", file);
      }

      string thumbnailImage = String.Format(@"{0}\{1}.jpg", Thumbs.Pictures,
                                            Util.Utils.EncryptLine(file));
      
      if (!File.Exists(thumbnailImage))
      {
        if (Util.Picture.CreateThumbnail(file, thumbnailImage, (int)Thumbs.ThumbResolution,
                                         (int)Thumbs.ThumbResolution, iRotate, Thumbs.SpeedThumbsSmall))
        {
          Log.Debug("PictureDatabase: Creation of missing thumb successful for {0}", file);
        }
      }

      if (!_noLargeThumbnails)
      {
        thumbnailImage = String.Format(@"{0}\{1}L.jpg", Thumbs.Pictures, Util.Utils.EncryptLine(file));
        
        if (!File.Exists(thumbnailImage))
        {
          if (Util.Picture.CreateThumbnail(file, thumbnailImage, (int)Thumbs.ThumbLargeResolution,
                                           (int)Thumbs.ThumbLargeResolution, iRotate,
                                           Thumbs.SpeedThumbsLarge))
          {
            Log.Debug("PictureDatabase: Creation of missing large thumb successful for {0}", file);
          }
        }
      }
    }

    private static void CountFiles(string path, ref ArrayList availableFiles)
    {
      //
      // Count the files in the current directory
      //
      try
      {
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.PictureExtensions);
        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(path, true);
        foreach (GUIListItem item in items)
        {
          if (item.IsFolder)
          {
            if (item.Label != "..")
            {
              CountFiles(item.Path, ref availableFiles);
            }
          }
          else
          {
            availableFiles.Add(item.Path);
          }
        }
      }
      catch (Exception e)
      {
        Log.Info("Exception counting files:{0}", e);
        // Ignore
      }
    }

    private void SetStatus(string status)
    {
      GUIPropertyManager.SetProperty("#scanstatus", status);
    }

    private void OnResetDatabase()
    {
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
      if (dlgYesNo != null)
      {
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(927)); // Warning
        dlgYesNo.SetLine(1, GUILocalizeStrings.Get(300026)); //Are you sure...
        dlgYesNo.DoModal(GetID);
        
        if (dlgYesNo.IsConfirmed)
        {
          string database = Config.GetFile(Config.Dir.Database, "PictureDatabase.db3");
          if (File.Exists(database))
          {
            PictureDatabase.Dispose();
            try
            {
              File.Delete(database);
            }
            catch (Exception) {}
            finally
            {
              PictureDatabase.ReOpen();
            }
          }
        }
      }
    }

    private void SetProperties()
    {
      GUIPropertyManager.SetProperty("#scanstatus", string.Empty);
    }

    private void EnableControls(bool enable)
    {
      if (enable)
      {
        lcFolders.IsEnabled = true;
        btnScanDatabase.IsEnabled = true;
        btnResetDatabase.IsEnabled = true;
      }
      else
      {
        lcFolders.IsEnabled = false;
        btnScanDatabase.IsEnabled = false;
        btnResetDatabase.IsEnabled = false;
      }
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item != null)
      {

      }
    }
  }
}
