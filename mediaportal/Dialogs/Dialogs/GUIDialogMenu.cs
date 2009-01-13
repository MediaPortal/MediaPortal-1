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
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogMenu : GUIDialogWindow, IDialogbox, IRenderLayer
  {
    [SkinControl(2)] protected GUIButtonControl btnClose = null;
    [SkinControl(3)] protected GUIListControl listView = null;
    [SkinControl(4)] protected GUILabelControl lblHeading = null;
    [SkinControl(5)] protected GUILabelControl lblHeading2 = null;

    protected int selectedItemIndex = -1;
    protected int selectedId = -1;
    protected bool showQuickNumbers = true;
    protected string selectedItemLabel = string.Empty;
    protected ArrayList listItems = new ArrayList();
    protected DateTime keyTimer = DateTime.Now;
    protected string keySelection = string.Empty;

    public GUIDialogMenu()
    {
      GetID = (int) Window.WINDOW_DIALOG_MENU;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogMenu.xml");
    }

    public override void OnAction(Action action)
    {
      char key = (char) 0;

      // if we have a keypress or a remote button press
      if ((action.wID == Action.ActionType.ACTION_KEY_PRESSED) ||
          ((Action.ActionType.REMOTE_0 <= action.wID) && (Action.ActionType.REMOTE_9 >= action.wID)))
      {
        if (action.m_key != null)
        {
          if (action.m_key.KeyChar >= '0' && action.m_key.KeyChar <= '9')
          {
            // Get offset to item
            key = (char) action.m_key.KeyChar;
          }
        }
        else
        {
          key = ((char) ('0' + action.wID - Action.ActionType.REMOTE_0));
        }
        if (key == (char) 0)
        {
          return;
        }
        keySelection += key;
        if (keySelection.Length == listItems.Count.ToString().Length)
        {
          selectOption(keySelection);
          keySelection = string.Empty;
          return;
        }
        keyTimer = DateTime.Now;
        return;
      }

      base.OnAction(action);
    }

    public void selectOption(string keySelected)
    {
      int selected;
      try
      {
        selected = int.Parse(keySelected) - 1;
      }
      catch (Exception)
      {
        selected = -1;
      }
      if (selected >= 0 && selected < listItems.Count)
      {
        // Select dialog item
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, listView.GetID, 0, 0, null);
        OnMessage(msg);
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0, listView.GetID, selected, 0, null);
        OnMessage(msg);
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, listView.GetID, 0, 0, 0, null);
        OnMessage(msg);
      }
    }

    public override void Process()
    {
      if (keySelection == string.Empty)
      {
        return;
      }
      TimeSpan ts = DateTime.Now - keyTimer;
      if (ts.TotalMilliseconds >= 1000)
      {
        selectOption(keySelection);
        keySelection = string.Empty;
      }
    }


    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == listView)
      {
        selectedItemIndex = listView.SelectedListItemIndex;
        selectedItemLabel = listView.SelectedListItem.Label;
        int pos = selectedItemLabel.IndexOf(" ");
        if (pos > 0)
        {
          selectedItemLabel = selectedItemLabel.Substring(pos + 1);
        }
        selectedId = listView.SelectedListItem.ItemId;
        PageDestroy();
      }
      if (control == btnClose)
      {
        PageDestroy();
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      //      needRefresh = true;
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            lblHeading.Label = string.Empty;
            if (lblHeading2 != null)
            {
              lblHeading2.Label = string.Empty;
            }

            base.OnMessage(message);
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);

            listView.Clear();
            for (int i = 0; i < listItems.Count; i++)
            {
              GUIListItem pItem = (GUIListItem) listItems[i];
              listView.Add(pItem);
            }

            if (selectedItemIndex >= 0)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0, listView.GetID,
                                              selectedItemIndex, 0, null);
              OnMessage(msg);
            }
            selectedItemIndex = -1;
            selectedId = -1;
            string wszText = String.Format("{0} {1}", listItems.Count, GUILocalizeStrings.Get(127));
          }
          return true;
      }
      return base.OnMessage(message);
    }

    public override void Reset()
    {
      base.Reset();
      listItems.Clear();
      showQuickNumbers = true;
      selectedItemIndex = -1;
    }

    public override void DoModal(int dwParentId)
    {
      if (listItems.Count == 0)
      {
        PageDestroy();
        return;
      }
      base.DoModal(dwParentId);
    }


    public void Add(string strLabel)
    {
      int iItemIndex = listItems.Count + 1;
      GUIListItem pItem = new GUIListItem();
      if (showQuickNumbers)
      {
        pItem.Label = iItemIndex.ToString() + " " + strLabel;
      }
      else
      {
        pItem.Label = strLabel;
      }

      pItem.ItemId = iItemIndex;
      listItems.Add(pItem);
    }

    public void Add(GUIListItem pItem)
    {
      int iItemIndex = listItems.Count + 1;
      if (showQuickNumbers)
      {
        pItem.Label = iItemIndex.ToString() + " " + pItem.Label;
      }
      else
      {
        pItem.Label = pItem.Label;
      }

      pItem.ItemId = iItemIndex;
      listItems.Add(pItem);
    }

    public bool ShowQuickNumbers
    {
      get { return showQuickNumbers; }
      set { showQuickNumbers = value; }
    }

    public int IndexOfItem(int iLocalizedString)
    {
      int index = 0;
      foreach (GUIListItem pItem in listItems)
      {
        if (showQuickNumbers)
        {
          int labelIndex = listItems.IndexOf(pItem) + 1;
          if (pItem.Label.Equals(labelIndex.ToString() + " " + GUILocalizeStrings.Get(iLocalizedString)))
          {
            index = listItems.IndexOf(pItem);
          }
        }
        else
        {
          if (pItem.Label.Equals(GUILocalizeStrings.Get(iLocalizedString)))
          {
            index = listItems.IndexOf(pItem);
          }
        }
      }
      return index;
    }

    public int IndexOfItem(string strItemLable)
    {
      int index = 0;
      foreach (GUIListItem pItem in listItems)
      {
        if (showQuickNumbers)
        {
          int labelIndex = listItems.IndexOf(pItem) + 1;
          if (pItem.Label.Equals(labelIndex.ToString() + " " + strItemLable))
          {
            index = listItems.IndexOf(pItem);
          }
        }
        else
        {
          if (pItem.Label.Equals(strItemLable))
          {
            index = listItems.IndexOf(pItem);
          }
        }
      }
      return index;
    }


    public void AddLocalizedString(int iLocalizedString)
    {
      int iItemIndex = listItems.Count + 1;
      GUIListItem pItem = new GUIListItem();
      if (showQuickNumbers)
      {
        pItem.Label = iItemIndex.ToString() + " " + GUILocalizeStrings.Get(iLocalizedString);
        pItem.ItemId = iLocalizedString;
        listItems.Add(pItem);
      }
      else
      {
        pItem.Label = GUILocalizeStrings.Get(iLocalizedString);
        pItem.ItemId = iLocalizedString;
        listItems.Add(pItem);
      }
    }

    public override int SelectedLabel
    {
      get { return selectedItemIndex; }
      set { selectedItemIndex = value; }
    }

    public int SelectedId
    {
      get { return selectedId; }
      set { selectedId = value; }
    }

    public string SelectedLabelText
    {
      get { return selectedItemLabel; }
    }

    public virtual void SetHeading(string strLine)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      lblHeading.Label = strLine;
      if (lblHeading2 != null)
      {
        if (strLine.Length < 1)
        {
          lblHeading2.Label = string.Empty;
        }
        else
        {
          lblHeading2.Label = GUILocalizeStrings.Get(924);
        }
      }
    }

    public void SetHeading(int iString)
    {
      SetHeading(GUILocalizeStrings.Get(iString));
      selectedItemIndex = -1;
      listItems.Clear();
    }
  }
}