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
      CONTROL_LISTLOOP_DELAY= 5,
      CONTROL_EXAMPLE = 25,
      CONTROL_EXAMPLE2 = 26,
    } ;

    private int m_iSpeedHorizontal = 1;
    private int m_iSpeedVertical = 4;
    private int m_iListLoopDelay = 100;

    public GUISettingsGUI()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GUICONTROL; //31
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_GUI_ScrollSpeed.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        m_iSpeedHorizontal = xmlreader.GetValueAsInt("gui", "ScrollSpeedRight", 1);
        m_iSpeedVertical = xmlreader.GetValueAsInt("gui", "ScrollSpeedDown", 4);
        m_iListLoopDelay = xmlreader.GetValueAsInt("gui", "listLoopDelay", 100);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("gui", "ScrollSpeedRight", m_iSpeedHorizontal.ToString());
        xmlwriter.SetValue("gui", "ScrollSpeedDown", m_iSpeedVertical.ToString());
        xmlwriter.SetValue("screen", "GuiRenderFps", GUIGraphicsContext.MaxFPS);
        xmlwriter.SetValue("gui", "listLoopDelay", m_iListLoopDelay.ToString());
      }
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100031));

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if (GUISettings.SettingsChanged && !Util.Utils.IsGUISettingsWindow(newWindowId))
      {
        GUISettings.OnRestartMP(GetID);
      }

      base.OnPageDestroy(newWindowId);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

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
            GUISpinControl cntl = (GUISpinControl)GetControl((int)Controls.CONTROL_FPS);
            cntl.ShowRange = false;
            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_SPEED_HORIZONTAL);
            for (int i = 1; i <= 5; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_SPEED_HORIZONTAL, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_SPEED_HORIZONTAL, m_iSpeedHorizontal - 1);

            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_SPEED_VERTICAL);
            for (int i = 1; i <= 5; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_SPEED_VERTICAL, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_SPEED_VERTICAL, m_iSpeedVertical - 1);

            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_FPS);
            for (int i = 10; i <= 100; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_FPS, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_FPS, GUIGraphicsContext.MaxFPS - 10);


            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LISTLOOP_DELAY);
            for (int i = 1; i <= 10000; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_LISTLOOP_DELAY, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_LISTLOOP_DELAY, m_iListLoopDelay - 1);


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
            if (iControl == (int)Controls.CONTROL_SPEED_HORIZONTAL)
            {
              string strLabel = message.Label;
              m_iSpeedHorizontal = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedHorizontal = m_iSpeedHorizontal;
              ResetExampleLabels();
            }
            if (iControl == (int)Controls.CONTROL_SPEED_VERTICAL)
            {
              string strLabel = message.Label;
              m_iSpeedVertical = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedVertical = m_iSpeedVertical;
              ResetExampleLabels();
            }
            if (iControl == (int)Controls.CONTROL_FPS)
            {
              string strLabel = message.Label;
              int fps = Int32.Parse(strLabel);
              GUIGraphicsContext.MaxFPS = fps;
            }
            if (iControl == (int)Controls.CONTROL_LISTLOOP_DELAY)
            {
              string strLabel = message.Label;
              m_iListLoopDelay = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedVertical = m_iListLoopDelay;
              ResetExampleLabels();
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    #endregion

    private void ResetExampleLabels()
    {
      GUIControl.ClearControl(GetID, (int)Controls.CONTROL_EXAMPLE);
      GUIControl.ClearControl(GetID, (int)Controls.CONTROL_EXAMPLE2);
      const string exampleText =
        "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
      const string exampleText2 =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Curabitur sodales ligula in libero. Sed dignissim lacinia nunc.\n\n" +
        "Curabitur tortor. Pellentesque nibh. Aenean quam. In scelerisque sem at dolor. Maecenas mattis. Sed convallis tristique sem. Proin ut ligula vel nunc egestas porttitor. Morbi lectus risus, iaculis vel, suscipit quis, luctus non, massa. Fusce ac turpis quis ligula lacinia aliquet. Mauris ipsum. Nulla metus metus, ullamcorper vel, tincidunt sed, euismod in, nibh. Quisque volutpat condimentum velit. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nam nec ante.\n\n" +
        "Sed lacinia, urna non tincidunt mattis, tortor neque adipiscing diam, a cursus ipsum ante quis turpis. Nulla facilisi. Ut fringilla. Suspendisse potenti. Nunc feugiat mi a tellus consequat imperdiet. Vestibulum sapien. Proin quam. Etiam ultrices. Suspendisse in justo eu magna luctus suscipit. Sed lectus. Integer euismod lacus luctus magna.\n\n" +
        "Quisque cursus, metus vitae pharetra auctor, sem massa mattis sem, at interdum magna augue eget diam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Morbi lacinia molestie dui. Praesent blandit dolor. Sed non quam. In vel mi sit amet augue congue elementum. Morbi in ipsum sit amet pede facilisis laoreet. Donec lacus nunc, viverra nec, blandit vel, egestas et, augue. Vestibulum tincidunt malesuada tellus. Ut ultrices ultrices enim. Curabitur sit amet mauris. Morbi in dui quis est pulvinar ullamcorper. Nulla facilisi. Integer lacinia sollicitudin massa.\n\n" +
        "Cras metus. Sed aliquet risus a tortor. Integer id quam. Morbi mi. Quisque nisl felis, venenatis tristique, dignissim in, ultrices sit amet, augue. Proin sodales libero eget ante. Nulla quam. Aenean laoreet. Vestibulum nisi lectus, commodo ac, facilisis ac, ultricies eu, pede. Ut orci risus, accumsan porttitor, cursus quis, aliquet eget, justo. Sed pretium blandit orci.\n";
      GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_EXAMPLE, exampleText);
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_EXAMPLE2, exampleText2);
    }
    
  }
}