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
using System.IO;
using System.Collections;
using System.Diagnostics;
using MediaPortal.Player;
using SQLite.NET;
using Programs.Utils;
using MediaPortal.Ripper;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using WindowPlugins.GUIPrograms;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for appDirBrowse.
  /// </summary>
  public class appItemDirBrowse: ProgramsDatabase.AppItem
  {
    VirtualDirectory curDirectory = new VirtualDirectory();
    ProgramComparer pc = new ProgramComparer(); // slightly hacky: pc replaces the base.dbPc object....

    public appItemDirBrowse(SQLiteClient initSqlDB): base(initSqlDB){}

    override public void LoadFiles()
    {
      // nothing to load, because directory is directly displayed
      // no FileItems!
    }


    override public bool FileEditorAllowed()
    {
      return false; // no editor allowed!
    }

    override public bool FileAddAllowed()
    {
      return false; // and of course, no file adding allowed!
    }

    override public bool FilesCanBeFavourites()
    {
      return false; // no files, no links!
    }

    override public bool ProfileLoadingAllowed()
    {
      return true;
    }

    String GetFolderThumb(String fileName)
    {
      string folderThumb = "";
      if (imageDirs.Length > 0)
      {
        string mainImgDir = imageDirs[0];
        folderThumb = mainImgDir + "\\" + fileName;
        folderThumb = Path.ChangeExtension(folderThumb, ".jpg");
        if (!System.IO.File.Exists(folderThumb))
        {
          folderThumb = Path.ChangeExtension(folderThumb, ".gif");
        }
        if (!System.IO.File.Exists(folderThumb))
        {
          folderThumb = Path.ChangeExtension(folderThumb, ".png");
        }
        if (!System.IO.File.Exists(folderThumb))
        {
          folderThumb = mainImgDir + "\\default.png";
        }
        if (!System.IO.File.Exists(folderThumb))
        {
          folderThumb = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
        }
      }
      else
      {
        folderThumb = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
      }
      return folderThumb;
    }

    public override string GetCurThumb(GUIListItem item)
    {
      string res = GetFolderThumb(item.Label);
      if (res != GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png")
      {
        return res;
      }
      else
      {
        return "";
      }
    }


    int LoadDirectory(string newDirectory, GUIFacadeControl facadeView)
    {
      ProgramUtils.SetFileExtensions(curDirectory, ValidExtensions);
/*
      ValidExtensions = ValidExtensions.Replace(" ", "");
      ArrayList extensions = new ArrayList(this.ValidExtensions.Split(','));
      // allow spaces between extensions...
      curDirectory.SetExtensions(extensions);
*/      
      ArrayList curFiles = curDirectory.GetDirectory(newDirectory);

      int totalItems = 0;
      foreach (GUIListItem file in curFiles)
      {
        Utils.SetDefaultIcons(file);
        if (file.IsFolder)
        {
          file.ThumbnailImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
          file.IconImageBig = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
          file.IconImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderNF.png";
        }
        else
        {
          string folderThumb = GetFolderThumb(file.Label);
          file.ThumbnailImage = folderThumb;
          file.IconImageBig = folderThumb;
          file.IconImage = folderThumb;
        }

        if (file.Label != ProgramUtils.cBackLabel)
        {
          file.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(file_OnItemSelected);
          facadeView.Add(file);
          totalItems++;
        }
      }

      return totalItems;
    }

    private void file_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null)
        return ;
      string thumbName = "";
      if ((item.ThumbnailImage != GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png") && (item.ThumbnailImage != ""))
      {
        // only show big thumb if there is really one....
        thumbName = item.ThumbnailImage;
      }
      filmstrip.InfoImageFileName = thumbName;
    }


    override public int DisplayFiles(string Filepath, GUIFacadeControl facadeView)
    {
      int Total = 0;
      if (Filepath == "")
      {
        // normal: load the main filelist of the application
        Total = LoadDirectory(this.FileDirectory, facadeView);
      }
      else
      {
        // subfolder is activated: load the filelist of the subfolder
        Total = LoadDirectory(Filepath, facadeView);
      }
      return Total;
    }


    override public string DefaultFilepath()
    {
      return this.FileDirectory;
    }

    override public void LaunchFile(GUIListItem item)
    {
      bool bUseGenericPlayer = (Filename.ToUpper() == "%PLAY%") || (Filename.ToUpper() == "%PLAYAUDIOSTREAM%") || (Filename.ToUpper() == "%PLAYVIDEOSTREAM%");

      string curFilename = item.Path;
      ProcessStartInfo procStart = new ProcessStartInfo();
      if (Filename != "")
      {
        procStart.FileName = this.Filename; // application
        procStart.Arguments = this.Arguments;
        if (UseQuotes)
        {
          curFilename = " \"" + item.Path + "\"";
        }
        if (procStart.Arguments.IndexOf("%FILE%") ==  - 1)
        {
          // no placeholder found => default handling: add the fileitem as the last argument
          procStart.Arguments = procStart.Arguments + curFilename;
        }
        else
        {
          // placeholder found => replace the placeholder by the correct filename
          procStart.Arguments = procStart.Arguments.Replace("%FILE%", curFilename);
        }
      }
      else
      {
        // application has no filename given => simply ShellExecute the item....
        procStart.FileName = item.Path;
      }
      procStart.WorkingDirectory = Startupdir;
      if (procStart.WorkingDirectory.IndexOf("%FILEDIR%") !=  - 1)
      {
        procStart.WorkingDirectory = procStart.WorkingDirectory.Replace("%FILEDIR%", Path.GetDirectoryName(item.Path));
      }
      procStart.UseShellExecute = UseShellExecute;
      procStart.WindowStyle = this.WindowStyle;
      try
      {
        DoPreLaunch();
        if (bUseGenericPlayer)
        {
          LaunchGenericPlayer(Filename, item.Path);
        }
        else
        {
          AutoPlay.StopListening();
          if (g_Player.Playing)
          {
            g_Player.Stop();
          }
          Utils.StartProcess(procStart, WaitForExit);
          GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters);
          AutoPlay.StartListening();
        }
      }
      catch (Exception ex)
      {
        Log.Write("myPrograms: error launching program\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n  stack: {3} {4} {5}", procStart.FileName,
          procStart.Arguments, procStart.WorkingDirectory, ex.Message, ex.Source, ex.StackTrace);
      }
      finally
      {
        DoPostLaunch();
      }

    }

    override public void OnInfo(GUIListItem item, ref bool isOverviewVisible, ref ProgramInfoAction modalResult, ref int selectedFileID)
    {
      // no info screen for directory items
    }

    override public void OnSort(GUIFacadeControl view, bool bDoSwitch)
    {
      // todo: polymorph it! pc => dbPc
      if (bDoSwitch)
      {
        pc.updateState();
      }
      view.Sort(pc);
    }

    override public void OnSortToggle(GUIFacadeControl view)
    {
      pc.bAsc = (!pc.bAsc);
      view.Sort(pc);
    }

    override public string CurrentSortTitle()
    {
      return pc.currentSortMethodAsText;
    }

    override public bool GetCurrentSortIsAscending()
    {
      return pc.bAsc;
    }

    override public void SetCurrentSortIndex(int newValue)
    {
      pc.currentSortMethodIndex = newValue;
    }

    override public void SetCurrentSortIsAscending(bool newValue)
    {
      pc.bAsc = newValue;
    }

    override public int GetCurrentSortIndex()
    {
      return pc.currentSortMethodIndex;
    }


  }
}
