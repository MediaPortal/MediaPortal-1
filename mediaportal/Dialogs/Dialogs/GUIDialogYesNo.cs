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
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;

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
    private int _timeOutInSeconds = 0;
    private DateTime timeStart = DateTime.Now;
    private string _btnNoLabel = GUILocalizeStrings.Get(106); //No
    private string _btnYesLabel = GUILocalizeStrings.Get(107); //Yes

    public GUIDialogYesNo()
    {
      GetID = (int)Window.WINDOW_DIALOG_YES_NO;
    }

    public override bool Init()
    {
      bool result = Load(GUIGraphicsContext.GetThemedSkinFile(@"\dialogYesNo.xml"));

      SaveDefaultBtnLabels();

      return result;
    }

    // since we save some values on init (after skin load), and LoadSkin method is not virtual,
    // also, the status whether skin was loaded is private, so we cannot get that one either
    // we need set this to false
    // otherwise, skin is not loaded until first show
    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void DoModal(int dwParentId)
    {
      timeStart = DateTime.Now;
      base.DoModal(dwParentId);
    }

    public override bool ProcessDoModal()
    {
      bool result = base.ProcessDoModal();
      TimeSpan timeElapsed = DateTime.Now - timeStart;
      if (TimeOut > 0)
      {
        if (timeElapsed.TotalSeconds >= TimeOut)
        {
          GUIMessage msgConfirm = new GUIMessage();
          msgConfirm.Message = GUIMessage.MessageType.GUI_MSG_CLICKED;
          msgConfirm.SenderControlId = m_DefaultYes ? btnYes.GetID : btnNo.GetID;
          OnMessage(msgConfirm);
          return false;
        }
      }
      return result;
    }

    public override void PageDestroy()
    {
      _timeOutInSeconds = 0;
      base.PageDestroy();
      RestoreDefaultBtnLabels();
    }

    public override void Reset()
    {
      base.Reset();
      _timeOutInSeconds = 0;
      RestoreDefaultBtnLabels();
      m_bConfirmed = false;
      m_DefaultYes = false;
      iYesKey = -1;
      iNoKey = -1;
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
            iYesKey = (int)btnYes.Label.ToLower()[0];
            iNoKey = (int)btnNo.Label.ToLower()[0];
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
      //LoadSkin();
      AllocResources();
      InitControls();

      SetControlLabel(GetID, 1, strLine);

      SetLine(1, string.Empty);
      SetLine(2, string.Empty);
      SetLine(3, string.Empty);
      SetLine(4, string.Empty);
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

    public void SetYesLabel(string label)
    {
      btnYes.Label = label;
    }

    public void SetNoLabel(string label)
    {
      btnNo.Label = label;
    }

    protected void SaveDefaultBtnLabels()
    {
      _btnNoLabel = btnNo.Label;
      _btnYesLabel = btnYes.Label;
    }

    protected void RestoreDefaultBtnLabels()
    {
      btnNo.Label = _btnNoLabel;
      btnYes.Label = _btnYesLabel;
    }

    public int TimeOut
    {
      get { return _timeOutInSeconds; }
      set { _timeOutInSeconds = value; }
    }
  }
}