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
using System.Collections;
using System.Collections.Generic;
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogSelect : GUIDialogWindow, IComparer<GUIListItem>
  {
    private enum Controls
    {
      CONTROL_BACKGROUND = 1,
      CONTROL_NUMBEROFFILES = 2,
      CONTROL_LIST = 3,
      CONTROL_HEADING = 4,
      CONTROL_BUTTON = 5,
      CONTROL_BACKGROUNDDLG = 6
    } ;

    private bool m_bButtonPressed = false;
    private bool m_bSortAscending = true;
    private bool m_bButtonEnabled = false;
    private string m_strSelected = "";
    private ArrayList m_vecList = new ArrayList();

    public GUIDialogSelect()
    {
      GetID = (int)Window.WINDOW_DIALOG_SELECT;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogSelect.xml");
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            SetControlLabel(GetID, (int)Controls.CONTROL_HEADING, string.Empty);
            base.OnMessage(message);
            FreeResources();
            DeInitControls();
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            m_bButtonPressed = false;
            base.OnMessage(message);
            _selectedLabel = -1;
            ClearControl(GetID, (int)Controls.CONTROL_LIST);

            for (int i = 0; i < m_vecList.Count; i++)
            {
              GUIListItem pItem = (GUIListItem)m_vecList[i];
              AddListItemControl(GetID, (int)Controls.CONTROL_LIST, pItem);
            }

            string wszText = String.Format("{0} {1}", m_vecList.Count, GUILocalizeStrings.Get(127));

            SetControlLabel(GetID, (int)Controls.CONTROL_NUMBEROFFILES, wszText);

            if (m_bButtonEnabled)
            {
              EnableControl(GetID, (int)Controls.CONTROL_BUTTON);
            }
            else
            {
              DisableControl(GetID, (int)Controls.CONTROL_BUTTON);
            }
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if ((int)Controls.CONTROL_LIST == iControl)
            {
              int iAction = message.Param1;
              if ((int)Action.ActionType.ACTION_SELECT_ITEM == iAction)
              {
                _selectedLabel = GetSelectedItemNo();
                m_strSelected = GetSelectedItem().Label;
                PageDestroy();
              }
            }
            if ((int)Controls.CONTROL_BUTTON == iControl)
            {
              _selectedLabel = -1;
              m_bButtonPressed = true;
              PageDestroy();
            }
          }
          break;
      }

      return base.OnMessage(message);
    }

    public override void Reset()
    {
      base.Reset();
      m_vecList.Clear();
      m_bButtonEnabled = false;
    }

    public void Add(string strLabel)
    {
      GUIListItem pItem = new GUIListItem(strLabel);
      m_vecList.Add(pItem);
    }

    public string SelectedLabelText
    {
      get { return m_strSelected; }
    }

    public void SetHeading(string strLine)
    {
      Reset();
      LoadSkin();
      AllocResources();
      InitControls();

      SetControlLabel(GetID, (int)Controls.CONTROL_HEADING, strLine);
    }

    public void SetButtonLabel(string strLine)
    {
      SetControlLabel(GetID, (int)Controls.CONTROL_BUTTON, strLine);
    }

    public void SetHeading(int iString)
    {
      SetHeading(GUILocalizeStrings.Get(iString));
    }

    public void EnableButton(bool bOnOff)
    {
      m_bButtonEnabled = bOnOff;
    }

    public void SetButtonLabel(int iString)
    {
      SetButtonLabel(GUILocalizeStrings.Get(iString));
    }

    public bool IsButtonPressed
    {
      get { return m_bButtonPressed; }
    }

    private void Sort(bool bSortAscending /*=true*/)
    {
      m_bSortAscending = bSortAscending;
      GUIListControl list = (GUIListControl)GetControl((int)Controls.CONTROL_LIST);
      list.Sort(this);
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2)
      {
        return 0;
      }
      if (item1 == null)
      {
        return -1;
      }
      if (item2 == null)
      {
        return -1;
      }
      if (item1.IsFolder && item1.Label == "..")
      {
        return -1;
      }
      if (item2.IsFolder && item2.Label == "..")
      {
        return -1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      else if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }

      string strSize1 = "";
      string strSize2 = "";
      if (item1.FileInfo != null)
      {
        strSize1 = Util.Utils.GetSize(item1.FileInfo.Length);
      }
      if (item2.FileInfo != null)
      {
        strSize2 = Util.Utils.GetSize(item2.FileInfo.Length);
      }

      item1.Label2 = strSize1;
      item2.Label2 = strSize2;

      if (m_bSortAscending)
      {
        return String.Compare(item1.Label, item2.Label, true);
      }
      else
      {
        return String.Compare(item2.Label, item1.Label, true);
      }
    }

    private GUIListItem GetSelectedItem()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM, GetID, 0,
                                      (int)Controls.CONTROL_LIST, 0, 0, null);
      OnMessage(msg);
      return (GUIListItem)msg.Object;
    }

    private GUIListItem GetItem(int iItem)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_ITEM, GetID, 0, (int)Controls.CONTROL_LIST,
                                      iItem, 0, null);
      OnMessage(msg);
      return (GUIListItem)msg.Object;
    }

    private int GetSelectedItemNo()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0,
                                      (int)Controls.CONTROL_LIST, 0, 0, null);
      OnMessage(msg);
      int iItem = msg.Param1;
      return iItem;
    }

    private int GetItemCount()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEMS, GetID, 0, (int)Controls.CONTROL_LIST, 0, 0,
                                      null);
      OnMessage(msg);
      return msg.Param1;
    }

    private void ClearControl(int iWindowId, int iControlId)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, iWindowId, 0, iControlId, 0, 0, null);
      OnMessage(msg);
    }

    private void AddListItemControl(int iWindowId, int iControlId, GUIListItem item)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, iWindowId, 0, iControlId, 0, 0, item);
      OnMessage(msg);
    }
  }
}