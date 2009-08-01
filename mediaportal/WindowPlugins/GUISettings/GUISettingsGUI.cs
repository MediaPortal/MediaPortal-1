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

using System;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  public class GUISettingsGUI : GUIInternalWindow
  {
    private enum Controls
    {
      CONTROL_SPEED_HORIZONTAL = 2,
      CONTROL_SPEED_VERTICAL = 4,
      CONTROL_FPS = 3,
      CONTROL_EXAMPLE = 25,
      CONTROL_EXAMPLE2 = 26,
    } ;

    private int m_iSpeedHorizontal = 1;
    private int m_iSpeedVertical = 4;

    public GUISettingsGUI()
    {
      GetID = (int) Window.WINDOW_SETTINGS_GUI;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\SettingsGUI.xml");
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
      }
      base.OnAction(action);
    }

    public override void Render(float timePassed)
    {
      string fps = String.Format("{0} fps", GUIGraphicsContext.CurrentFPS.ToString("f2"));
      GUIPropertyManager.SetProperty("#fps", fps);
      base.Render(timePassed);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            LoadSettings();
            GUISpinControl cntl = (GUISpinControl) GetControl((int) Controls.CONTROL_FPS);
            cntl.ShowRange = false;
            GUIControl.ClearControl(GetID, (int) Controls.CONTROL_SPEED_HORIZONTAL);
            for (int i = 1; i <= 5; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int) Controls.CONTROL_SPEED_HORIZONTAL, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int) Controls.CONTROL_SPEED_HORIZONTAL, m_iSpeedHorizontal - 1);

            GUIControl.ClearControl(GetID, (int) Controls.CONTROL_SPEED_VERTICAL);
            for (int i = 1; i <= 5; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int) Controls.CONTROL_SPEED_VERTICAL, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int) Controls.CONTROL_SPEED_VERTICAL, m_iSpeedVertical - 1);

            GUIControl.ClearControl(GetID, (int) Controls.CONTROL_FPS);
            for (int i = 10; i <= 100; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int) Controls.CONTROL_FPS, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int) Controls.CONTROL_FPS, GUIGraphicsContext.MaxFPS - 10);

            ResetExampleLabels();

            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            SaveSettings();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int) Controls.CONTROL_SPEED_HORIZONTAL)
            {
              string strLabel = message.Label;
              m_iSpeedHorizontal = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedHorizontal = m_iSpeedHorizontal;
              ResetExampleLabels();
            }
            if (iControl == (int) Controls.CONTROL_SPEED_VERTICAL)
            {
              string strLabel = message.Label;
              m_iSpeedVertical = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedVertical = m_iSpeedVertical;
              ResetExampleLabels();
            }
            if (iControl == (int) Controls.CONTROL_FPS)
            {
              string strLabel = message.Label;
              int fps = Int32.Parse(strLabel);
              GUIGraphicsContext.MaxFPS = fps;
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    private void ResetExampleLabels()
    {
      GUIControl.ClearControl(GetID, (int) Controls.CONTROL_EXAMPLE);
      GUIControl.ClearControl(GetID, (int) Controls.CONTROL_EXAMPLE2);
      string strTmp =
        "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
      GUIControl.AddItemLabelControl(GetID, (int) Controls.CONTROL_EXAMPLE, strTmp);
      GUIControl.SetControlLabel(GetID, (int) Controls.CONTROL_EXAMPLE2, strTmp);
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        m_iSpeedHorizontal = xmlreader.GetValueAsInt("general", "ScrollSpeedRight", 1);
        m_iSpeedVertical = xmlreader.GetValueAsInt("general", "ScrollSpeedDown", 4);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("general", "ScrollSpeedRight", m_iSpeedHorizontal.ToString());
        xmlwriter.SetValue("general", "ScrollSpeedDown", m_iSpeedVertical.ToString());
        xmlwriter.SetValue("screen", "GuiRenderFps", GUIGraphicsContext.MaxFPS);
      }
    }

    #endregion
  }
}