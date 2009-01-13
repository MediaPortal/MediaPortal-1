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
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogYesNo : GUIDialogWindow
  {
    [SkinControl(10)] protected GUIButtonControl btnNo = null;
    [SkinControl(11)] protected GUIButtonControl btnYes = null;

    private bool m_bConfirmed = false;
    private bool m_DefaultYes = false;
    private int iYesKey = -1;
    private int iNoKey = -1;
    //bool needRefresh = false;
    private DateTime vmr7UpdateTimer = DateTime.Now;

    public GUIDialogYesNo()
    {
      GetID = (int) Window.WINDOW_DIALOG_YES_NO;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\dialogYesNo.xml");
    }


    public override void OnAction(Action action)
    {
      //needRefresh = true;
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        m_DefaultYes = false;
        base.OnAction(action);
        return;
      }

      if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
      {
        if (action.m_key != null)
        {
          // Yes or No key
          if (action.m_key.KeyChar == iYesKey)
          {
            m_bConfirmed = true;
            PageDestroy();
            m_DefaultYes = false;
            return;
          }

          if (action.m_key.KeyChar == iNoKey)
          {
            m_bConfirmed = false;
            PageDestroy();
            m_DefaultYes = false;
            return;
          }
        }
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      //needRefresh = true;
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            SetControlLabel(GetID, 1, string.Empty);
            base.OnMessage(message);
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            m_bConfirmed = false;
            base.OnMessage(message);
            if (m_DefaultYes)
            {
              GUIControl.FocusControl(GetID, btnYes.GetID);
            }
            iYesKey = (int) btnYes.Label.ToLower()[0];
            iNoKey = (int) btnNo.Label.ToLower()[0];
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;

            if (btnYes == null)
            {
              m_bConfirmed = true;
              PageDestroy();
              m_DefaultYes = false;
              return true;
            }
            if (iControl == btnNo.GetID)
            {
              m_bConfirmed = false;
              PageDestroy();
              m_DefaultYes = false;
              return true;
            }
            if (iControl == btnYes.GetID)
            {
              m_bConfirmed = true;
              PageDestroy();
              m_DefaultYes = false;
              return true;
            }
          }
          break;
      }

      return base.OnMessage(message);
    }


    public bool IsConfirmed
    {
      get { return m_bConfirmed; }
    }

    public void SetHeading(string strLine)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      SetControlLabel(GetID, 1, strLine);

      //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1, 0, 0, null);
      //msg.Label = strLine;
      //OnMessage(msg);
      SetLine(1, string.Empty);
      SetLine(2, string.Empty);
      SetLine(3, string.Empty);
    }

    public void SetHeading(int iString)
    {
      if (iString == 0)
      {
        SetHeading(string.Empty);
      }
      else
      {
        SetHeading(GUILocalizeStrings.Get(iString));
      }
    }

    public void SetLine(int iLine, string strLine)
    {
      if (iLine <= 0)
      {
        return;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1 + iLine, 0, 0, null);
      msg.Label = strLine;
      OnMessage(msg);
    }

    public void SetLine(int iLine, int iString)
    {
      if (iLine <= 0)
      {
        return;
      }
      if (iString == 0)
      {
        SetLine(iLine, string.Empty);
      }
      else
      {
        SetLine(iLine, GUILocalizeStrings.Get(iString));
      }
    }

    public void SetDefaultToYes(bool bYesNo)
    {
      m_DefaultYes = bYesNo;
    }
  }
}