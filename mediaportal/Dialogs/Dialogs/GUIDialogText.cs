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

using System.Drawing;
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIDialogText : GUIDialogWindow
  {
    [SkinControl(2)] protected GUIButtonControl btnClose = null;
    [SkinControl(3)] protected GUITextControl txtArea = null;
    [SkinControl(4)] protected GUILabelControl lblHeading = null;
    [SkinControl(5)] protected GUIImage imgLogo = null;


    public GUIDialogText()
    {
      GetID = (int) Window.WINDOW_DIALOG_TEXT;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogText.xml");
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_MOVE_DOWN)
      {
        // Frig the action to pretend its a page down
        action.wID = Action.ActionType.ACTION_PAGE_DOWN;
      }
      else if (action.wID == Action.ActionType.ACTION_MOVE_UP)
      {
        // Frig the action to pretend its a page up
        action.wID = Action.ActionType.ACTION_PAGE_UP;
      }
      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnClose)
      {
        PageDestroy();
      }
    }

    public void SetHeading(string strLine)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      lblHeading.Label = strLine;
    }

    public void SetHeading(int iString)
    {
      SetHeading(GUILocalizeStrings.Get(iString));
    }

    public void SetText(string text)
    {
      txtArea.OnMessage(new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID, 0, txtArea.GetID, 0, 0, null));
      txtArea.Label = text;
    }

    public void SetImage(string filename)
    {
      imgLogo.FreeResources();
      imgLogo.SetFileName(filename);
      imgLogo.AllocResources();
    }

    public void SetImageDimensions(Size size, bool keepAspectRatio, bool centered)
    {
      imgLogo.Width = size.Width;
      imgLogo.Height = size.Height;
      imgLogo.KeepAspectRatio = keepAspectRatio;
      imgLogo.Centered = centered;
    }
  }
}