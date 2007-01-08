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
      dlgOk.SetHeading(3);//my videos
      dlgOk.SetLine(1, 219);//no disc
      dlgOk.DoModal(parentId);
      Log.Info("SelectDVDHandler: did not find a movie");
      return null;
    }

  }
}