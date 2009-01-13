#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
using System.Globalization;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Util;

namespace MediaPortal.Dialogs
{
  public class GUIDialogTVConflict : GUIDialogWindow
  {
    #region Enums

    private enum Controls
    {
      LIST = 3,
      HEADING = 4,
      BUTTON_NEW_REC = 11,
      BUTTON_CONFLICT_REC = 12,
      BUTTON_KEEP_CONFLICT = 13,
      BUTTON_CONFLICT_EPISODE = 14
    } ;

    #endregion

    #region Variables

    // Private Variables
    // Protected Variables
    protected bool _conflictingEpisodes = false;
    // Public Variables

    #endregion

    #region Constructors/Destructors

    public GUIDialogTVConflict()
    {
      GetID = (int) Window.WINDOW_DIALOG_TVCONFLICT;
    }

    #endregion

    #region Properties

    public bool ConflictingEpisodes
    {
      get { return _conflictingEpisodes; }
      set { _conflictingEpisodes = value; }
    }

    #endregion

    #region Public Methods

    public void SetHeading(string HeadingText)
    {
      SetControlLabel(GetID, (int) Controls.HEADING, HeadingText);
    }

    public void AddConflictRecordings(List<TVRecording> conflicts)
    {
      if ((conflicts == null) || (conflicts.Count < 1))
      {
        return;
      }
      GUIListControl list = (GUIListControl) GetControl((int) Controls.LIST);

      if (list != null)
      {
        foreach (TVRecording conflict in conflicts)
        {
          GUIListItem item = new GUIListItem(conflict.Title);
          item.Label2 = GetRecordingDateTime(conflict);
          item.Label3 = conflict.Channel;
          item.TVTag = conflict;
          AddConflictRecording(item);
        }
      }
    }

    public void AddConflictRecording(GUIListItem item)
    {
      string logo = Util.Utils.GetCoverArt(Thumbs.TVChannel, item.Label3);
      if (!File.Exists(logo))
      {
        logo = "defaultVideoBig.png";
      }
      item.ThumbnailImage = logo;
      item.IconImageBig = logo;
      item.IconImage = logo;
      item.OnItemSelected += new GUIListItem.ItemSelectedHandler(OnListItemSelected);

      GUIListControl list = (GUIListControl) GetControl((int) Controls.LIST);
      if (list != null)
      {
        list.Add(item);
      }
    }

    #endregion

    #region Private Methods

    private void OnListItemSelected(GUIListItem item, GUIControl parent)
    {
      if ((item == null) || (item.TVTag == null))
      {
        return;
      }
      // to be implemented
    }

    private string GetRecordingDateTime(TVRecording rec)
    {
      return String.Format("{0} {1} - {2}",
                           Util.Utils.GetShortDayString(rec.StartTime),
                           rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                           rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
    }

    #endregion

    #region <Base class> Overloads

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogTVConflict.xml");
    }

    public override void Reset()
    {
      base.Reset();
      ConflictingEpisodes = false;
      GUIListControl list = (GUIListControl) GetControl((int) Controls.LIST);
      if (list != null)
      {
        list.Clear();
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl = message.SenderControlId;
          if ((int) Controls.BUTTON_NEW_REC == iControl)
          {
            SelectedLabel = 0;
            PageDestroy();
          }
          else if ((int) Controls.BUTTON_CONFLICT_REC == iControl)
          {
            SelectedLabel = 1;
            PageDestroy();
          }
          else if ((int) Controls.BUTTON_KEEP_CONFLICT == iControl)
          {
            SelectedLabel = 2;
            PageDestroy();
          }
          else if ((int) Controls.BUTTON_CONFLICT_EPISODE == iControl)
          {
            SelectedLabel = 3;
            PageDestroy();
          }
          break;
      }
      return base.OnMessage(message);
    }

    public override void DoModal(int ParentID)
    {
      if (_conflictingEpisodes)
      {
        ShowControl(GetID, (int) Controls.BUTTON_CONFLICT_EPISODE);
      }
      else
      {
        HideControl(GetID, (int) Controls.BUTTON_CONFLICT_EPISODE);
      }

      base.DoModal(ParentID);
    }

    #endregion
  }
}