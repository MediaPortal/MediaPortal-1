#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *      Copyright (C) 2005-2009 Team MediaPortal
 *      http://www.team-mediaportal.com
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
using System.Collections.Generic;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Service class to display DVD selection dialog
  /// </summary>
  internal class SelectDVDHandler : ISelectDVDHandler
  {
    public string ShowSelectDVDDialog(int parentId)
    {
      Log.Info("SelectDVDHandler: ShowSelectDVDDialog()");

      //check if dvd is inserted
      List<GUIListItem> rootDrives = VirtualDirectories.Instance.Movies.GetRootExt();

      for (int i = rootDrives.Count - 1; i >= 0; i--)
      {
        GUIListItem item = (GUIListItem) rootDrives[i];
        if (Util.Utils.getDriveType(item.Path) == 5) //cd or dvd drive
        {
          string driverLetter = item.Path.Substring(0, 1);
          string fileName = String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", driverLetter);
          if (!File.Exists(fileName))
          {
            rootDrives.RemoveAt(i);
          }
        }
        else
        {
          rootDrives.RemoveAt(i);
        }
      }

      if (rootDrives.Count > 0)
      {
        try
        {
          if (rootDrives.Count == 1)
          {
            GUIListItem ritem = (GUIListItem) rootDrives[0];
            return ritem.Path; // Only one DVD available, play it!
          }
          SetIMDBThumbs(rootDrives, false, true);
          // Display a dialog with all drives to select from
          GUIDialogSelect2 dlgSel =
            (GUIDialogSelect2) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_SELECT2);
          if (null == dlgSel)
          {
            Log.Info("SelectDVDHandler: Could not open dialog, defaulting to first drive found");
            GUIListItem ritem = (GUIListItem) rootDrives[0];
            return ritem.Path;
          }
          dlgSel.Reset();
          dlgSel.SetHeading(196); // Choose movie
          for (int i = 0; i < rootDrives.Count; i++)
          {
            GUIListItem dlgItem = new GUIListItem();
            dlgItem = (GUIListItem) rootDrives[i];
            Log.Debug("SelectDVDHandler: adding list item of possible playback location - {0}", dlgItem.Path);
            dlgSel.Add(dlgItem);
          }
          dlgSel.DoModal(parentId);

          if (dlgSel.SelectedLabel != -1)
          {
            return dlgSel.SelectedLabelText.Substring(1, 2);
          }
          else
          {
            return null;
          }
        }
        catch (Exception ex)
        {
          Log.Warn("SelectDVDHandler: could not determine dvd path - {0},{1}", ex.Message, ex.StackTrace);
          return null;
        }
      }
      //no disc in drive...
      GUIDialogOK dlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_OK);
      dlgOk.SetHeading(3); //my videos
      dlgOk.SetLine(1, 219); //no disc
      dlgOk.DoModal(parentId);
      Log.Info("SelectDVDHandler: did not find a movie");
      return null;
    }

    public bool OnPlayDVD(String drive, int parentId)
    {
      Log.Info("SelectDVDHandler: OnPlayDVD() playing DVD {0}", drive);
      if (g_Player.Playing && g_Player.IsDVD)
      {
        if (g_Player.CurrentFile.Equals(drive + @"\VIDEO_TS\VIDEO_TS.IFO"))
        {
          return true;
        }
        else
        {
          g_Player.Stop();
        }
      }
      if (g_Player.Playing && !g_Player.IsDVD)
      {
        g_Player.Stop();
      }
      if (Util.Utils.getDriveType(drive) == 5) //cd or dvd drive
      {
        string driverLetter = drive.Substring(0, 1);
        string fileName = String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", driverLetter);
        if (!VirtualDirectories.Instance.Movies.RequestPin(fileName))
        {
          return false;
        }
        if (File.Exists(fileName))
        {
          IMDBMovie movieDetails = new IMDBMovie();
          VideoDatabase.GetMovieInfo(fileName, ref movieDetails);
          int idFile = VideoDatabase.GetFileId(fileName);
          int idMovie = VideoDatabase.GetMovieId(fileName);
          int timeMovieStopped = 0;
          byte[] resumeData = null;
          if ((idMovie >= 0) && (idFile >= 0))
          {
            timeMovieStopped = VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData);
            //Log.Info("GUIVideoFiles: OnPlayBackStopped for DVD - idFile={0} timeMovieStopped={1} resumeData={2}", idFile, timeMovieStopped, resumeData);
            if (timeMovieStopped > 0)
            {
              string title = Path.GetFileName(fileName);
              VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);
              if (movieDetails.Title != string.Empty)
              {
                title = movieDetails.Title;
              }

              GUIDialogYesNo dlgYesNo =
                (GUIDialogYesNo) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_YES_NO);
              if (null == dlgYesNo)
              {
                return false;
              }
              dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
              dlgYesNo.SetLine(1, title);
              dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + Util.Utils.SecondsToHMSString(timeMovieStopped));
              dlgYesNo.SetDefaultToYes(true);
              dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

              if (!dlgYesNo.IsConfirmed)
              {
                timeMovieStopped = 0;
              }
            }
          }

          g_Player.PlayDVD(drive + @"\VIDEO_TS\VIDEO_TS.IFO");
          if (g_Player.Playing && timeMovieStopped > 0)
          {
            if (g_Player.IsDVD)
            {
              g_Player.Player.SetResumeState(resumeData);
            }
            else
            {
              Log.Debug("SelectDVDHandler.OnPlayDVD - skipping");
              g_Player.SeekAbsolute(timeMovieStopped);
            }
          }
          return true;
        }
      }
      //no disc in drive...
      GUIDialogOK dlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_OK);
      dlgOk.SetHeading(3); //my videos
      dlgOk.SetLine(1, 219); //no disc
      dlgOk.DoModal(parentId);
      return false;
    }

    public void SetIMDBThumbs(IList items, bool markWatchedFiles, bool eachMovieHasDedicatedFolder)
    {
      GUIListItem pItem;
      IMDBMovie movieDetails = new IMDBMovie();

      for (int x = 0; x < items.Count; x++)
      {
        string strThumb = string.Empty;
        string strLargeThumb = string.Empty;
        pItem = (GUIListItem) items[x];
        string file = string.Empty;
        bool isFolderPinProtected = (pItem.IsFolder && IsFolderPinProtected(pItem.Path));

        if (pItem.IsFolder)
        {
          if (pItem.Label == "..")
          {
            continue;
          }

          if (isFolderPinProtected)
          {
            // hide maybe rated content
            Util.Utils.SetDefaultIcons(pItem);
            continue;
          }

            // If this is enabled you'll see the thumb of the first movie in that dir - but if you put serveral movies into that dir you'll be irritated...          
          else
          {
            if (eachMovieHasDedicatedFolder)
            {
              file = GetFolderVideoFile(pItem.Path);
            }
          }
        }
        else if (!pItem.IsFolder ||
                 (pItem.IsFolder && VirtualDirectory.IsImageFile(Path.GetExtension(pItem.Path).ToLower())))
        {
          file = pItem.Path;
        }
        else
        {
          continue;
        }


        if (!string.IsNullOrEmpty(file))
        {
          byte[] resumeData = null;
          int fileId = VideoDatabase.GetFileId(file);
          int id = VideoDatabase.GetMovieInfo(file, ref movieDetails);
          bool foundWatched = false;

          if (id >= 0)
          {
            if (Util.Utils.IsDVD(pItem.Path))
            {
              pItem.Label = String.Format("({0}:) {1}", pItem.Path.Substring(0, 1), movieDetails.Title);
            }
            strThumb = Util.Utils.GetCoverArt(Thumbs.MovieTitle, movieDetails.Title);

            if (movieDetails.Watched > 0 && markWatchedFiles)
            {
              foundWatched = true;
            }
          }
          // do not double check
          if (!foundWatched && markWatchedFiles)
          {
            if (fileId >= 0)
            {
              if (VideoDatabase.GetMovieStopTime(fileId) > 0)
              {
                foundWatched = true;
              }
              else
              {
                int stops = VideoDatabase.GetMovieStopTimeAndResumeData(fileId, out resumeData);
                if (resumeData != null || stops > 0)
                {
                  foundWatched = true;
                }
              }
            }
          }
          if (!pItem.IsFolder)
          {
            pItem.IsPlayed = foundWatched;
          }


          if (!File.Exists(strThumb) || string.IsNullOrEmpty(strThumb))
          {
            strThumb = string.Format(@"{0}\{1}", Thumbs.MovieTitle,
                                     Util.Utils.MakeFileName(Util.Utils.SplitFilename(Path.ChangeExtension(file, ".jpg"))));
            if (!File.Exists(strThumb))
            {
              continue;
            }
          }

          pItem.ThumbnailImage = strThumb;
          pItem.IconImageBig = strThumb;
          pItem.IconImage = strThumb;

          strThumb = Util.Utils.ConvertToLargeCoverArt(strThumb);
          if (File.Exists(strThumb))
          {
            pItem.ThumbnailImage = strThumb;
          }
        } // <-- file == empty
      } // of for (int x = 0; x < items.Count; ++x)
    }

    private bool IsFolderPinProtected(string folder)
    {
      int pinCode = 0;
      return VirtualDirectories.Instance.Movies.IsProtectedShare(folder, out pinCode);
    }

    public string GetFolderVideoFile(string path)
    {
      // IFind first movie file in folder
      string strExtension = Path.GetExtension(path).ToLower();
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
          strDirs = Directory.GetDirectories(path, "video_ts");
        }
        catch (Exception)
        {
        }
        if (strDirs != null)
        {
          if (strDirs.Length == 1)
          {
            Log.Debug("GUIVideoFiles: DVD folder detected - {0}", strDirs[0]);
            return String.Format(@"{0}\VIDEO_TS.IFO", strDirs[0]);
          }
        }
        string[] strFiles = null;
        try
        {
          strFiles = Directory.GetFiles(path);
        }
        catch (Exception)
        {
        }
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
  }
}