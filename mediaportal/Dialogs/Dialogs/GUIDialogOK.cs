#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
  /// Shows a dialog box with an OK button  
  /// </summary>
  public class GUIDialogOK : GUIDialogWindow
  {
    [SkinControl(10)] protected GUIButtonControl btnNo = null;
    [SkinControl(11)] protected GUIButtonControl btnYes = null;
    private bool m_bConfirmed = false;

    public GUIDialogOK()
    {
      GetID = (int)Window.WINDOW_DIALOG_OK;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\dialogOK.xml");
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

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;

            if (btnYes == null)
            {
              m_bConfirmed = true;
              PageDestroy();
              return true;
            }
            if (iControl == btnNo.GetID)
            {
              m_bConfirmed = false;
              PageDestroy();
              return true;
            }
            if (iControl == btnYes.GetID)
            {
              m_bConfirmed = true;
              PageDestroy();
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
      SetLine(iLine, GUILocalizeStrings.Get(iString));
    }
  }
}