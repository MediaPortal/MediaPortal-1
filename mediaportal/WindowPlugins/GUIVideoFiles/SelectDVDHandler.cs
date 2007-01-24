#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *      Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Collections;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Services;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Service class to display DVD selection dialog
  /// </summary>
  class SelectDVDHandler : ISelectDVDHandler
  {
    public string ShowSelectDVDDialog(int parentId)
    {
      //check if dvd is inserted
      ArrayList rootDrives = VirtualDirectories.Instance.Movies.GetRoot();

      for (int i = rootDrives.Count - 1; i >= 0; i--)
      {
        GUIListItem item = (GUIListItem)rootDrives[i];
        if (Util.Utils.getDriveType(item.Path) == 5) //cd or dvd drive
        {
          string driverLetter = item.Path.Substring(0, 1);
          string fileName = String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", driverLetter);
          if (!System.IO.File.Exists(fileName))
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
          // Display a dialog with all drives to select from
          GUIVideoFiles videoFiles = (GUIVideoFiles)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEOS);
          if (null == videoFiles)
            return null;

          videoFiles.SetIMDBThumbs(rootDrives);

          GUIDialogSelect2 dlgSel = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT2);
          dlgSel.Reset();
          for (int i = 0; i < rootDrives.Count; i++)
          {
            GUIListItem dlgItem = new GUIListItem();
            dlgItem = (GUIListItem)rootDrives[i];
            Log.Debug("SelectDVDHandler: adding path of possible playback location - {0}", dlgItem.Path);
            dlgSel.Add(dlgItem.Path);
          }
          dlgSel.SetHeading(196); // Choose movie
          dlgSel.DoModal(parentId);

          if (dlgSel.SelectedLabel != -1)
          {
            return dlgSel.SelectedLabelText; //.Substring(1, 2);
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
      dlgOk.SetHeading(3);//my videos
      dlgOk.SetLine(1, 219);//no disc
      dlgOk.DoModal(parentId);
      Log.Info("SelectDVDHandler: did not find a movie");
      return null;
    }

  }
}