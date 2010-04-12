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

using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogSelect2 : GUIDialogWindow
  {
    private enum Controls
    {
      CONTROL_BACKGROUND = 1,
      CONTROL_LIST = 3,
      CONTROL_HEADING = 4,
      CONTROL_BACKGROUNDDLG = 6
    } ;

    private string m_strSelected = "";
    private ArrayList m_vecList = new ArrayList();

    public GUIDialogSelect2()
    {
      GetID = (int)Window.WINDOW_DIALOG_SELECT2;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogSelect2.xml");
    }

    public override bool OnMessage(GUIMessage message)
    {
      //      needRefresh = true;
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            SetControlLabel(GetID, (int)Controls.CONTROL_HEADING, string.Empty);
            base.OnMessage(message);
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            ClearControl(GetID, (int)Controls.CONTROL_LIST);

            for (int i = 0; i < m_vecList.Count; i++)
            {
              GUIListItem pItem = (GUIListItem)m_vecList[i];
              AddListItemControl(GetID, (int)Controls.CONTROL_LIST, pItem);
            }

            if (_selectedLabel >= 0)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,
                                              (int)Controls.CONTROL_LIST, _selectedLabel, 0, null);
              OnMessage(msg);
            }
            _selectedLabel = -1;
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if ((int)Controls.CONTROL_LIST == iControl)
            {
              _selectedLabel = GetSelectedItemNo();
              m_strSelected = GetSelectedItem().Label;
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
      m_vecList.DisposeAndClearList();
    }

    public void Add(string strLabel)
    {
      GUIListItem pItem = new GUIListItem(strLabel);
      m_vecList.Add(pItem);
    }

    public void Add(GUIListItem pItem)
    {
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


    public void SetHeading(int iString)
    {
      SetHeading(GUILocalizeStrings.Get(iString));
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