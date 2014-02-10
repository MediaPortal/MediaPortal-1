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
using MediaPortal.Player;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Service class to display DVD selection dialog
  /// </summary>
  public class SelectDVDHandler : ISelectDVDHandler
  {
    public string ShowSelectDVDDialog(int parentId)
    {
      return ShowSelectDriveDialog(parentId, true);
    }

    public string ShowSelectDriveDialog(int parentId, bool DVDonly)
    {
      Log.Info("SelectDVDHandler: ShowSelectDVDDialog()");

      //check if dvd is inserted
      List<GUIListItem> rootDrives = VirtualDirectories.Instance.Movies.GetRootExt();

      for (int i = rootDrives.Count - 1; i >= 0; i--)
      {
        GUIListItem item = (GUIListItem)rootDrives[i];
        if (Util.Utils.getDriveType(item.Path) == 5) //cd or dvd drive
        {
          string driverLetter = item.Path.Substring(0, 1);
          string fileName = DVDonly
                              ? String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", driverLetter)
                              : String.Format(@"{0}:\", driverLetter);
          if (DVDonly && !File.Exists(fileName))
          {
            rootDrives.RemoveAt(i);
          }
          else if (!DVDonly && !Directory.Exists(fileName))
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
            GUIListItem ritem = (GUIListItem)rootDrives[0];
            return ritem.Path; // Only one DVD available, play it!
          }
          SetIMDBThumbs(rootDrives, false);
          // Display a dialog with all drives to select from
          GUIDialogSelect2 dlgSel =
            (GUIDialogSelect2)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT2);
          if (null == dlgSel)
          {
            Log.Info("SelectDVDHandler: Could not open dialog, defaulting to first drive found");
            GUIListItem ritem = (GUIListItem)rootDrives[0];
            return ritem.Path;
          }
          dlgSel.Reset();
          dlgSel.SetHeading(DVDonly ? 196 : 2201); // Choose movie | select source
          for (int i = 0; i < rootDrives.Count; i++)
          {
            GUIListItem dlgItem = new GUIListItem();
            dlgItem = (GUIListItem)rootDrives[i];
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
      GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      dlgOk.SetHeading(1020); //information
      Log.Error("SelectDVDHandler: ShowSelectDriveDialog - Plz Insert Disk");
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
            timeMovieStopped = VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData, g_Player.SetResumeBDTitleState);
            //Log.Info("GUIVideoFiles: OnPlayBackStopped for DVD - idFile={0} timeMovieStopped={1} resumeData={2}", idFile, timeMovieStopped, resumeData);
            if (timeMovieStopped > 0)
            {
              string title = Path.GetFileName(fileName);
              VideoDatabase.GetMovieInfoById(idMovie, ref movieDetails);
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

          g_Player.PlayDVD(drive + @"\VIDEO_TS\VIDEO_TS.IFO");
          g_Player.ShowFullScreenWindow();
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
      GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      dlgOk.SetHeading(3); //my videos
      Log.Error("SelectDVDHandler: OnPlayDVD() Plz Insert Disk (ShowSelectDriveDialog)");
      dlgOk.SetLine(1, 219); //no disc
      dlgOk.DoModal(parentId);
      return false;
    }

    // Changed - cover for movies with the same name
    public void SetIMDBThumbs(IList items, bool markWatchedFiles)
    {
      try
      {
        GUIListItem pItem;
        ISelectBDHandler selectBdHandler;
        bool dedicatedMovieFolderChecked = false;
        bool isDedicatedMovieFolder = false;
        
        if (GlobalServiceProvider.IsRegistered<ISelectBDHandler>())
        {
          selectBdHandler = GlobalServiceProvider.Get<ISelectBDHandler>();
        }
        else
        {
          selectBdHandler = new SelectBDHandler();
          GlobalServiceProvider.Add<ISelectBDHandler>(selectBdHandler);
        }

        for (int x = 0; x < items.Count; x++)
        {
          string strThumb = string.Empty;
          pItem = (GUIListItem)items[x];
          string file = string.Empty;
          bool isFolderPinProtected = (pItem.IsFolder && IsFolderPinProtected(pItem.Path));
          IMDBMovie movie = pItem.AlbumInfoTag as IMDBMovie;

          if (movie == null)
          {
            IMDBMovie.SetMovieData(pItem);
            movie = pItem.AlbumInfoTag as IMDBMovie;
          }

          // Check for everymovieinitsownfolder only once for all items (defined share is the same for all)
          if (!dedicatedMovieFolderChecked && !string.IsNullOrEmpty(pItem.Path))
          {
            if (!pItem.IsRemote && !VirtualDirectories.Instance.Movies.IsRootShare(pItem.Path))
            {
              dedicatedMovieFolderChecked = true;
              isDedicatedMovieFolder = Util.Utils.IsFolderDedicatedMovieFolder(pItem.Path);
            }
          }

          // Skip DVD & BD backup folder
          if (pItem.IsFolder && !pItem.IsBdDvdFolder)
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
            if (!pItem.IsRemote && isDedicatedMovieFolder)
            {
              string[] strFiles = null;

              try
              {
                strFiles = Directory.GetFiles(pItem.Path);
              }
              catch (Exception) { }

              if (strFiles != null)
              {
                for (int i = 0; i < strFiles.Length; ++i)
                {
                  string extension = Path.GetExtension(strFiles[i]);

                  if (VirtualDirectory.IsImageFile(extension))
                  {
                    if (DaemonTools.IsEnabled)
                    {
                      file = strFiles[i];
                      break;
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
                    file = strFiles[i];
                    break;
                  }
                }
              }
            }
          }
          // Check for DVD folder
          else if (pItem.IsBdDvdFolder && IsDvdDirectory(pItem.Path))
          {
            file = GetFolderVideoFile(pItem.Path);
          }
          // Check for BD folder
          else if (pItem.IsBdDvdFolder && selectBdHandler.IsBDDirectory(pItem.Path))
          {
            file = selectBdHandler.GetFolderVideoFile(pItem.Path);
          }
          else if (!pItem.IsFolder ||
                   (pItem.IsFolder && VirtualDirectory.IsImageFile(Path.GetExtension(pItem.Path).ToLowerInvariant())))
          {
            file = pItem.Path;
          }
          else
          {
            continue;
          }

          if (!string.IsNullOrEmpty(file))
          {
            int id = movie.ID;

            // Set thumb for movies
            if (id > 0 && !movie.IsEmpty)
            {
              if (Util.Utils.IsDVD(pItem.Path))
              {
                pItem.Label = String.Format("({0}:) {1}", pItem.Path.Substring(0, 1), movie.Title);
              }

              string titleExt = movie.Title + "{" + id + "}";
              strThumb = Util.Utils.GetCoverArt(Thumbs.MovieTitle, titleExt);
            }

            if (!Util.Utils.FileExistsInCache(strThumb) || string.IsNullOrEmpty(strThumb))
            {
              string fPic = string.Empty;
              string fPicTbn = string.Empty;
              string path = pItem.Path;
              Util.Utils.RemoveStackEndings(ref path);
              Util.Utils.RemoveStackEndings(ref file);
              string jpgExt = ".jpg";
              string tbnExt = ".tbn";
              string folderJpg = @"\folder.jpg";

              if (pItem.IsBdDvdFolder)
              {
                fPic = string.Concat(pItem.Path,@"\", Path.GetFileNameWithoutExtension(path), jpgExt);
                fPicTbn = string.Concat(pItem.Path, @"\", Path.GetFileNameWithoutExtension(path), tbnExt);
              }
              else
              {
                fPic = Path.ChangeExtension(file, jpgExt);
                fPicTbn = Path.ChangeExtension(file, tbnExt);
              }

              if (File.Exists(fPic))
              {
                strThumb = fPic;
              }
              else if (File.Exists(fPicTbn))
              {
                strThumb = fPicTbn;
              }
              else
              {
                if (!pItem.IsFolder && isDedicatedMovieFolder)
                {
                  fPic = Path.GetDirectoryName(pItem.Path) + folderJpg;

                  if (File.Exists(fPic))
                  {
                    strThumb = fPic;
                  }
                  else
                  {
                    continue;
                  }
                }
                else
                {
                  continue;
                }
              }
            }
            
            pItem.ThumbnailImage = strThumb;
            pItem.IconImageBig = strThumb;
            pItem.IconImage = strThumb;

            strThumb = Util.Utils.ConvertToLargeCoverArt(strThumb);
            
            if (Util.Utils.FileExistsInCache(strThumb))
            {
              pItem.ThumbnailImage = strThumb;
            }
            
            movie = null;
          } // <-- file == empty
        } // of for (int x = 0; x < items.Count; ++x)
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
      }
      catch (Exception ex)
      {
        Log.Error("SelectDVDHandler: SetIMDbThumbs: {0}", ex.Message);
      }
    }
    
    public bool IsDvdDirectory(string path)
    {
      if (File.Exists(path + @"\VIDEO_TS\VIDEO_TS.IFO"))
      {
        return true;
      }
      return false;
    }

    private bool IsFolderPinProtected(string folder)
    {
      int pinCode = 0;
      return VirtualDirectories.Instance.Movies.IsProtectedShare(folder, out pinCode);
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
          strDirs = Directory.GetDirectories(path, "video_ts");
        }
        catch (Exception) {}
        if (strDirs != null)
        {
          if (strDirs.Length == 1)
          {
            Log.Debug("GUIVideoFiles: DVD folder detected - {0}", strDirs[0]);
            return String.Format(@"{0}\VIDEO_TS.IFO", strDirs[0]);
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
    
  }
}