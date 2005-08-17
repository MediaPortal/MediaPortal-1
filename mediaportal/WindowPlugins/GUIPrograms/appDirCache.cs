/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Collections;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Programs.Utils;
using SQLite.NET;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for appItemDirCache.
  /// </summary>
  public class appItemDirCache: AppItem
  {
    GUIDialogProgress progressDialog = null;

    public appItemDirCache(SQLiteClient initSqlDB): base(initSqlDB){}

    private void ShowProgressDialog()
    {
      progressDialog = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      progressDialog.ShowWaitCursor = true;
      progressDialog.SetHeading(GUILocalizeStrings.Get(13014));
      progressDialog.SetLine(0, GUILocalizeStrings.Get(13014)); 
      progressDialog.SetLine(1, "");
      progressDialog.SetLine(2, "");
      progressDialog.StartModal(GetID);
      progressDialog.Progress();
    }

    private void CloseProgressDialog()
    {
      progressDialog.Close();
    }

    private string GetThumbsFile(GUIListItem guiFile, string fileTitle)
    {
      string thumbFolder = "";
      if (imageDirs.Length > 0)
      {
        string mainImgFolder = "";

        foreach (string imgFolder in imageDirs)
        {
          if (System.IO.Directory.Exists(imgFolder))
          {
            mainImgFolder = imgFolder;
          }
        }

        if ("" != mainImgFolder)
        {
          string curDir = mainImgFolder + "\\";
          string filenameNoExtension = mainImgFolder + "\\" + guiFile.Label;
          filenameNoExtension = Path.ChangeExtension(filenameNoExtension, null);
          filenameNoExtension = Path.GetFileNameWithoutExtension(filenameNoExtension);

          string[] exactMatchesJPG = Directory.GetFiles(curDir, filenameNoExtension + "*.jpg");
          string[] exactMatchesGIF = Directory.GetFiles(curDir, filenameNoExtension + "*.gif");
          string[] exactMatchesPNG = Directory.GetFiles(curDir, filenameNoExtension + "*.png");
          if (exactMatchesJPG.Length > 0)
          {
            thumbFolder = exactMatchesJPG[0];
          }
          else if (exactMatchesGIF.Length > 0)
          {
            thumbFolder = exactMatchesGIF[0];
          }
          else if (exactMatchesPNG.Length > 0)
          {
            thumbFolder = exactMatchesPNG[0];
          }
          else
          {
            // no exact match found! Redo with near matches!
            string[] nearMatchesJPG = Directory.GetFiles(curDir, fileTitle + "*.jpg");
            string[] nearMatchesGIF = Directory.GetFiles(curDir, fileTitle + "*.gif");
            string[] nearMatchesPNG = Directory.GetFiles(curDir, fileTitle + "*.png");
            if (nearMatchesJPG.Length > 0)
            {
              thumbFolder = nearMatchesJPG[0];
            }
            else if (nearMatchesGIF.Length > 0)
            {
              thumbFolder = nearMatchesGIF[0];
            }
            else if (nearMatchesPNG.Length > 0)
            {
              thumbFolder = nearMatchesPNG[0];
            }
          }
        }
      }
      return thumbFolder;
    }


    private void ImportFileItem(GUIListItem guiFile)
    {
      FileItem curFile = new FileItem(sqlDB);
      curFile.FileID =  - 1; // to force an INSERT statement when writing the item
      curFile.AppID = this.AppID;
      curFile.Title = guiFile.Label;
      curFile.Title = curFile.TitleNormalized;
      curFile.Filename = guiFile.Path;
      if (this.UseQuotes)
      {
        curFile.Filename = "\"" + curFile.Filename + "\"";
      }
      curFile.Filepath = Path.GetDirectoryName(guiFile.Path);
      curFile.Imagefile = GetThumbsFile(guiFile, curFile.TitleNormalized);
      // not imported properties => set default values
      curFile.ManualFilename = "";
      curFile.LastTimeLaunched = DateTime.MinValue;
      curFile.LaunchCount = 0;
      curFile.Write();
    }

    private void WriteFolderItem(string directoryPath)
    {
      FileItem curFile = new FileItem(sqlDB);
      curFile.FileID =  - 1;
      curFile.AppID = this.AppID;
      curFile.Filename = directoryPath;
      curFile.Title = Path.GetFileNameWithoutExtension(directoryPath);
      curFile.Filepath = Path.GetDirectoryName(directoryPath);
      curFile.IsFolder = true;
      curFile.ManualFilename = "";
      curFile.LastTimeLaunched = DateTime.MinValue;
      curFile.LaunchCount = 0;
      curFile.Write();
    }


    private void UpdateProgressDialog(GUIListItem guiFile, bool mpGuiMode)
    {
      if (mpGuiMode)
      {
        progressDialog.SetLine(2, String.Format("{0} {1}", GUILocalizeStrings.Get(13005), guiFile.Label)); // "last imported file {0}"
        progressDialog.Progress();
      }
      SendRefreshInfo(String.Format("{0} {1}", GUILocalizeStrings.Get(13005), guiFile.Label));
    }

    private void ImportDirectory(string curPath, bool mpGuiMode)
    {
      VirtualDirectory virtDir = new VirtualDirectory();
      ProgramUtils.SetFileExtensions(virtDir, ValidExtensions);
/*
 *       ArrayList dirExtensions = new ArrayList(this.ValidExtensions.Split(','));
      virtDir.SetExtensions(dirExtensions);
*/      

      // read files
      ArrayList arrFiles = virtDir.GetDirectory(curPath);
      foreach (GUIListItem file in arrFiles)
      {
        if (!file.IsFolder)
        {
          ImportFileItem(file);
          UpdateProgressDialog(file, mpGuiMode);
        }
      }

      //read subdirectories
      try
      {
        string[] directories = Directory.GetDirectories(curPath);
        foreach (string directory in directories)
        {
          WriteFolderItem(directory);
          // recursively call importer for every subdirectory
          ImportDirectory(directory, mpGuiMode);
        }
      }
      catch 
      {
        // Ignore
      }

    }

    private void DoDirCacheImport(bool mpGuiMode)
    {
      if (sqlDB == null)
        return ;
      if (this.AppID < 0)
        return ;
      if (this.SourceType != myProgSourceType.DIRCACHE)
        return ;
      if (mpGuiMode)
      {
        ShowProgressDialog();
      }
      try
      {
        ValidExtensions = ValidExtensions.Replace(" ", "");
        ImportDirectory(this.FileDirectory, mpGuiMode);
      }
      finally
      {
        if (mpGuiMode)
        {
          CloseProgressDialog();
        }
      }

    }


    override public void LoadFiles()
    {
      // load Files and fill Files-arraylist here!
      if (fileList == null)
      {
        fileList = new Filelist(sqlDB);
      }
      else
      {
        fileList.Clear();
      }
      fileList.Load(AppID, FileDirectory);
      filesAreLoaded = true;
    }


    override public string CurrentFilePath()
    {
      if (Files.Filepath != "")
      {
        return Files.Filepath;
      }
      else
      {
        return base.CurrentFilePath();
      }
    }


    override public string DefaultFilepath()
    {
      return this.FileDirectory;
    }

    override public bool RefreshButtonVisible()
    {
      return true;
    }


    override public bool FileBrowseAllowed()
    {
      return true;
    }

    override public bool ProfileLoadingAllowed()
    {
      return true;
    }

    override public void Refresh(bool mpGuiMode)
    {
      base.Refresh(mpGuiMode);
      DeleteFiles();
      DoDirCacheImport(mpGuiMode);
      FixFileLinks();
      LoadFiles();
    }
  }

}
