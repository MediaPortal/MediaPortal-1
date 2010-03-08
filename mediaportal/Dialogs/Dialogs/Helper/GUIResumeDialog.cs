#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  public static class GUIResumeDialog
  {
    /// <summary>
    /// Defines which item in resume dialog has been selected.
    /// </summary>
    public enum Result
    {
      /// <summary>
      /// An error happens on showing the resume dialog.
      /// Even if this happened the video should be played.
      /// </summary>
      Error,

      /// <summary>
      /// User does not want to start the video at all
      /// </summary>
      Abort,

      PlayFromBeginning,
      PlayFromLastStopTime,
      PlayFromLivePoint
    }

    /// <summary>
    /// Defines how the resume dialog is displayed.
    /// </summary>
    public enum MediaType
    {
      Video,
      DVD,
      Recording,

      /// <summary>
      /// Represents a live recording
      /// </summary>
      LiveRecording
    }

    private static string GetBeginningText(MediaType mediaType)
    {
      switch (mediaType)
      {
        case MediaType.LiveRecording:
        case MediaType.Recording:
          return GUILocalizeStrings.Get(979);

        case MediaType.DVD:
        case MediaType.Video:
        default:
          return GUILocalizeStrings.Get(1201);
      }
    }

    private static string GetLastStopTimeText(MediaType mediaType, int lastStopTime)
    {
      string text;
      
      switch (mediaType)
      {
        case MediaType.DVD:
        case MediaType.LiveRecording:
        case MediaType.Recording:
        case MediaType.Video:
        default:
          text = GUILocalizeStrings.Get(1211);
          break;
      }

      return String.Format(text, Util.Utils.SecondsToHMSString(lastStopTime));
    }

    /// <summary>
    /// Opens a menu dialog for choosing the resume option and returns the result.
    /// </summary>
    /// <param name="title">is used for the dialog's title.</param>
    /// <param name="lastStopTime">repesents the last stop time in seconds.</param>
    /// <param name="mediaType">defines for which media the dialog is displayed.
    /// Using LiveRecording displays the 'Resume from LivePoint' item.</param>
    /// <returns>Returns the result of the displayed resume dialog.</returns>
    public static Result ShowResumeDialog(string title, int lastStopTime, MediaType mediaType)
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return Result.Error;

      dlg.Reset();
      dlg.SetHeading(title);

      // add menu items
      GUIListItem itemBeginning = new GUIListItem(GetBeginningText(mediaType));
      dlg.Add(itemBeginning);

      GUIListItem itemLastStopTime = new GUIListItem(GetLastStopTimeText(mediaType, lastStopTime));
      if (lastStopTime > 0)
        dlg.Add(itemLastStopTime);

      GUIListItem itemLivePoint = new GUIListItem(GUILocalizeStrings.Get(980));
      if (mediaType == MediaType.LiveRecording)
        dlg.Add(itemLivePoint);

      // set focus to last stop time
      // itemIds 0 based, listindex (labels) 1 based
      dlg.SelectedLabel = itemLastStopTime.ItemId - 1;

      //// if dialog contains only beginning item, it is not needed to display it
      //if (lastStopTime <= 0 && mediaType != MediaType.LiveRecording)
      //  return Result.PlayFromBeginning;

      // show dialog
      dlg.DoModal(GUIWindowManager.ActiveWindow);

      // set results
      if (dlg.SelectedId == -1)
        return Result.Abort;

      if (dlg.SelectedId == itemBeginning.ItemId)
        return Result.PlayFromBeginning;
      
      if (dlg.SelectedId == itemLastStopTime.ItemId)
        return Result.PlayFromLastStopTime;

      if (dlg.SelectedId == itemLivePoint.ItemId)
        return Result.PlayFromLivePoint;

      return Result.PlayFromBeginning;
    }
  }
}
