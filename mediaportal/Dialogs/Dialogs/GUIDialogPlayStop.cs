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

using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// Dialog that react to STOP remote command and take it as a YES 
  /// </summary>
  public class GUIDialogPlayStop : GUIDialogWindow
  {
    [SkinControl(10)] protected GUIButtonControl btnPlay = null;
    [SkinControl(11)] protected GUIButtonControl btnStop = null;

    private bool m_bConfirmed = false;
    private bool m_DefaultStop = false;

    public GUIDialogPlayStop()
    {
      GetID = (int) Window.WINDOW_DIALOG_PLAY_STOP;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\dialogPlayStop.xml");
    }

    public override void OnAction(Action action)
    {
      //needRefresh = true;
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||
          action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        m_DefaultStop = false;
        base.OnAction(action);
        return;
      }
      //
      // WARNING: Even if STOP is a semantic NO, as we look from the user point of view, here it's a YES
      //
      // es. "Do you really want to STOP play LiveTv ?" -> YES I want to STOP
      //
      if (action.wID == Action.ActionType.ACTION_STOP || action.m_key.KeyChar == 'b')
      {
        m_bConfirmed = true;
        PageDestroy();
        m_DefaultStop = false;
        return;
      }
      //
      // WARNING: See above comment to understand why PLAY, that is a semantic YES, here is a NO
      //
      if (action.wID == Action.ActionType.ACTION_PLAY || action.m_key.KeyChar == 'p')
      {
        m_bConfirmed = false;
        PageDestroy();
        m_DefaultStop = false;
        return;
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
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
            if (m_DefaultStop)
            {
              GUIControl.FocusControl(GetID, btnStop.GetID);
            }
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;

            if (btnStop == null)
            {
              m_bConfirmed = true;
              PageDestroy();
              m_DefaultStop = false;
              return true;
            }
            if (iControl == btnPlay.GetID)
            {
              m_bConfirmed = false;
              PageDestroy();
              m_DefaultStop = false;
              return true;
            }
            if (iControl == btnStop.GetID)
            {
              m_bConfirmed = true;
              PageDestroy();
              m_DefaultStop = false;
              return true;
            }
          }
          break;
      }

      return base.OnMessage(message);
    }


    public bool IsStopConfirmed
    {
      get { return m_bConfirmed; }
    }

    public void SetHeading(string strLine)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      SetControlLabel(GetID, 1, strLine);

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

    public void SetDefaultToStop(bool bPlayStop)
    {
      m_DefaultStop = bPlayStop;
    }
  }
}