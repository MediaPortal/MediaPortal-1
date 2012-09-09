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
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.Video
{
  internal class SelectBDHandler : ISelectBDHandler
  {
    public bool IsBDDirectory(string path)
    {
      if (File.Exists(path + @"\BDMV\index.bdmv"))
      {
        return true;
      }
      return false;
    }

    public string GetFolderVideoFile(string path)
    {
      if (string.IsNullOrEmpty(path))
        return string.Empty;

      // IFind first movie file in folder
      string strExtension = Path.GetExtension(path).ToLowerInvariant();
      if (VirtualDirectory.IsImageFile(strExtension))
      {
        return path;
      }
      else
      {
        if (VirtualDirectories.Instance.Movies.IsRemote(path))
        {
          return string.Empty;
        }
        if (!path.EndsWith(@"\"))
        {
          path = path + @"\";
        }
        string[] strDirs = null;
        try
        {
          strDirs = Directory.GetDirectories(path, "bdmv");
        }
        catch (Exception) {}
        if (strDirs != null)
        {
          if (strDirs.Length == 1)
          {
            Log.Debug("GUIVideoFiles: BD folder detected - {0}", strDirs[0]);
            return String.Format(@"{0}\index.bdmv", strDirs[0]);
          }
          else
          {
            return string.Empty;
          }
        }
        
        string[] strFiles = null;
        try
        {
          strFiles = Directory.GetFiles(path);
        }
        catch (Exception) {}
        if (strFiles != null)
        {
          for (int i = 0; i < strFiles.Length; ++i)
          {
            string extensionension = Path.GetExtension(strFiles[i]);
            if (VirtualDirectory.IsImageFile(extensionension))
            {
              if (DaemonTools.IsEnabled)
              {
                return strFiles[i];
              }
              continue;
            }
            if (VirtualDirectory.IsValidExtension(strFiles[i], Util.Utils.VideoExtensions, false))
            {
              // Skip hidden files
              if ((File.GetAttributes(strFiles[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
              {
                continue;
              }
              return strFiles[i];
            }
          }
        }
      }
      return string.Empty;
    }

    // Image play method
    public bool OnPlayBD(String drive, int parentId)
    {
      Log.Info("SelectBDHandler: OnPlayBD() playing BD {0}", drive);

      string currentFile = g_Player.CurrentFile;

      if (g_Player.Playing && Util.Utils.IsISOImage(currentFile) && IsBDPlayList(ref currentFile))
      //if (g_Player.Playing && IsBDPlayList(ref currentFile))
      {
        return true;
      }

      if (Util.Utils.getDriveType(drive) == 5) //cd or dvd drive
      {
        string driverLetter = drive.Substring(0, 1);
        string fileName = String.Format(@"{0}:\BDMV\index.bdmv", driverLetter);
        
        if (File.Exists(fileName))
        {
          IMDBMovie movieDetails = new IMDBMovie();
          
          //string name = DaemonTools._MountedIsoFile;

          int idFileImg = VideoDatabase.GetFileId(fileName);
          int idMovieImg = VideoDatabase.GetMovieId(fileName);


          int timeMovieStopped = 0;
          byte[] resumeData = null;

          if ((idMovieImg >= 0) && (idFileImg >= 0))
          {
            timeMovieStopped = VideoDatabase.GetMovieStopTimeAndResumeData(idFileImg, out resumeData);

            if (timeMovieStopped > 0)
            {
              string title = Path.GetFileName(fileName);
              VideoDatabase.GetMovieInfoById(idMovieImg, ref movieDetails);

              if (movieDetails.Title != string.Empty)
              {
                title = movieDetails.Title;
              }

              GUIResumeDialog.Result result =
                GUIResumeDialog.ShowResumeDialog(title, timeMovieStopped,
                                                 GUIResumeDialog.MediaType.DVD);

              if (result == GUIResumeDialog.Result.Abort)
                return false;

              if (result == GUIResumeDialog.Result.PlayFromBeginning)
                timeMovieStopped = 0;
            }
          }

          if (g_Player.Playing)
          {
            g_Player.Stop();
          }

          g_Player.PlayBD(drive + @"\BDMV\index.bdmv");
          g_Player.ShowFullScreenWindow();

          if (g_Player.Playing && timeMovieStopped > 0)
          {
            g_Player.SeekAbsolute(timeMovieStopped);
          }
          return true;
        }
      }
      //no disc in drive...
      GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      dlgOk.SetHeading(3); //my videos
      Log.Error("SelectBDHandler: OnPlayBD() Plz Insert Disk");
      dlgOk.SetLine(1, 219); //no disc
      dlgOk.DoModal(parentId);
      return true;
    }
    
    public string GetBDFolderName(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
        return string.Empty;

      string lowPath = fileName.ToLowerInvariant();
      int index = lowPath.IndexOf("bdmv/");
      if (index < 0)
      {
        index = lowPath.IndexOf(@"bdmv\");
      }
      if (index >= 0)
      {
        fileName = fileName.Substring(0, index);
        fileName = Util.Utils.RemoveTrailingSlash(fileName);

        // get the name by stripping the first part : c:\media\movies
        int pos = fileName.LastIndexOfAny(new char[] { '\\', '/' });
        if (pos >= 0 && pos + 1 < fileName.Length - 1)
        {
          return fileName.Substring(pos + 1);
        }
      }
      return Path.GetFileName(fileName);
    }

    public bool IsBDPlayList(ref string filename)
    {

      if (string.IsNullOrEmpty(filename))
      {
        filename = string.Empty;
        return false;
      }
      
      // Check if is BD playlist 
      if (filename.ToLowerInvariant().Contains(@"\bdmv\") && filename.ToLowerInvariant().Contains(".mpls"))
      {
        int index = filename.ToLowerInvariant().LastIndexOf(@"\playlist");
        string name = filename.Remove(index);
        name = name + @"\index.bdmv";
        if (File.Exists(name))
        {
          filename = name;
          return true;
        }
      }
      return false;
    }
  }
}
