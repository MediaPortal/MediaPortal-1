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

using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setup
{
  public class DisplayControlWindow : GUIInternalWindow
  {
    [SkinControl(40)] protected GUIToggleButtonControl btnDisplayVideo = null;
    [SkinControl(41)] protected GUIToggleButtonControl btnDisplayAction = null;
    [SkinControl(42)] protected GUISelectButtonControl btnDisplayActionTime = null;
    [SkinControl(43)] protected GUIToggleButtonControl btnDisplayIdle = null;
    [SkinControl(44)] protected GUISelectButtonControl btnIdleDelay = null;
    private DisplayControl DisplayControl = new DisplayControl();

    public DisplayControlWindow()
    {
      this.GetID = 9002;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_MiniDisplay_Control.xml"));
    }

    #region Serialization

    private void LoadSettings()
    {
      this.DisplayControl = XMLUTILS.LoadDisplayControlSettings();
      this.SetDisplayVideo();
      this.SetDisplayAction();
      this.SetDisplayActionTime();
      this.SetDisplayIdle();
      this.SetIdleDelay();
    }

    private void SaveSettings()
    {
      this.DisplayControl.BlankDisplayWithVideo = this.btnDisplayVideo.Selected;
      this.DisplayControl.EnableDisplayAction = this.btnDisplayAction.Selected;
      if (this.btnDisplayAction.Selected)
      {
        this.DisplayControl.DisplayActionTime = this.btnDisplayActionTime.SelectedItem;
      }
      this.DisplayControl.BlankDisplayWhenIdle = this.btnDisplayIdle.Selected;
      if (this.btnDisplayIdle.Selected)
      {
        this.DisplayControl.BlankIdleDelay = this.btnIdleDelay.SelectedItem;
      }
      XMLUTILS.SaveDisplayControlSettings(this.DisplayControl);
    }

    #endregion

    #region Overrides

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnDisplayVideo)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnDisplayAction)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnDisplayActionTime)
      {
        this.SaveSettings();
      }
      if (control == this.btnDisplayIdle)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnIdleDelay)
      {
        this.SaveSettings();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void SetButtons()
    {
      btnDisplayVideo.Visible = true;
      btnDisplayAction.Visible = DisplayControl.BlankDisplayWithVideo;
      btnDisplayActionTime.Visible = DisplayControl.BlankDisplayWithVideo && DisplayControl.EnableDisplayAction;
      btnDisplayIdle.Visible = true;
      btnIdleDelay.Visible = DisplayControl.BlankDisplayWhenIdle;
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      if (MediaPortal.GUI.Settings.GUISettings.SettingsChanged && !MediaPortal.Util.Utils.IsGUISettingsWindow(newWindowId))
      {
        MediaPortal.GUI.Settings.GUISettings.OnRestartMP(GetID);
      }

      base.OnPageDestroy(newWindowId);
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (!base.OnMessage(message))
      {
        return false;
      }
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          LoadSettings();
          SetButtons();
          break;
      }
      return true;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      this.LoadSettings();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(109002));

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    #endregion
    
    private void SetDisplayAction()
    {
      if (this.DisplayControl.BlankDisplayWithVideo)
      {
        this.btnDisplayAction.Selected = this.DisplayControl.EnableDisplayAction;
      }
    }

    private void SetDisplayActionTime()
    {
      if (this.btnDisplayActionTime != null)
      {
        GUIControl.ClearControl(this.GetID, this.btnDisplayActionTime.GetID);
        for (int i = 0; i < 31; i++)
        {
          GUIControl.AddItemLabelControl(this.GetID, this.btnDisplayActionTime.GetID, i.ToString());
        }
        GUIControl.SelectItemControl(this.GetID, this.btnDisplayActionTime.GetID, this.DisplayControl.DisplayActionTime);
      }
    }

    private void SetDisplayIdle()
    {
      this.btnDisplayIdle.Selected = this.DisplayControl.BlankDisplayWhenIdle;
    }

    private void SetDisplayVideo()
    {
      this.btnDisplayVideo.Selected = this.DisplayControl.BlankDisplayWithVideo;
    }

    private void SetIdleDelay()
    {
      if (this.btnIdleDelay != null)
      {
        GUIControl.ClearControl(this.GetID, this.btnIdleDelay.GetID);
        for (int i = 0; i < 31; i++)
        {
          GUIControl.AddItemLabelControl(this.GetID, this.btnIdleDelay.GetID, i.ToString());
        }
        GUIControl.SelectItemControl(this.GetID, this.btnIdleDelay.GetID, this.DisplayControl.BlankIdleDelay);
      }
    }
  }
}