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
  public class MainMenuWindow : GUIInternalWindow
  {
    [SkinControl(3)] protected GUILabelControl labelInfo1 = null;
    [SkinControl(4)] protected GUILabelControl labelInfo2 = null;
    [SkinControl(5)] protected GUIButtonControl btnKeyPad = null;
    [SkinControl(6)] protected GUIButtonControl btnDisplayOptions = null;
    [SkinControl(7)] protected GUIButtonControl btnDisplayControl = null;
    [SkinControl(8)] protected GUIButtonControl btnEqualizer = null;
    [SkinControl(9)] protected GUIButtonControl btnBacklight = null;
    [SkinControl(10)] protected GUIButtonControl btnRemote = null;
    [SkinControl(11)] protected GUIToggleButtonControl btnMonitorPower = null;
    [SkinControl(12)] protected GUISelectButtonControl btnContrast = null;
    [SkinControl(13)] protected GUISelectButtonControl btnBrightness = null;

    public MainMenuWindow()
    {
      this.GetID = 9000;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_MiniDisplay_Main.xml"));
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnMonitorPower)
      {
        XMLUTILS.SaveMonitorPowerState(this.btnMonitorPower.Selected);
      }
      if (control == this.btnContrast)
      {
        XMLUTILS.SaveContrast(this.btnContrast.SelectedItem);
      }
      if (control == this.btnBrightness)
      {
        XMLUTILS.SaveBrightness(this.btnBrightness.SelectedItem);
      }
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
          SetButtons();
          break;
      }
      return true;
    }

    private void LoadSettings()
    {
      this.btnMonitorPower.Selected = XMLUTILS.LoadMonitorPowerSate();
      this.SetContrast();
      this.SetBrightness();
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      this.LoadSettings();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(109000));

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
      if (MediaPortal.GUI.Settings.GUISettings.SettingsChanged && !MediaPortal.Util.Utils.IsGUISettingsWindow(newWindowId))
      {
        MediaPortal.GUI.Settings.GUISettings.OnRestartMP(GetID);
      }

      base.OnPageDestroy(newWindowId);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    private void SetBrightness()
    {
      if (this.btnBrightness != null)
      {
        this.btnBrightness.Clear();
        for (int i = 0; i < 256; i++)
        {
          GUIControl.AddItemLabelControl(this.GetID, this.btnBrightness.GetID, i.ToString());
        }
        GUIControl.SelectItemControl(this.GetID, this.btnBrightness.GetID, Settings.Instance.Backlight);
      }
    }

    private void SetContrast()
    {
      if (this.btnContrast != null)
      {
        this.btnContrast.Clear();
        for (int i = 0; i < 256; i++)
        {
          GUIControl.AddItemLabelControl(this.GetID, this.btnContrast.GetID, i.ToString());
        }
        GUIControl.SelectItemControl(this.GetID, this.btnBrightness.GetID, Settings.Instance.Contrast);
      }
    }

    private void SetButtons()
    {
      MainMenuLayout layout = XMLUTILS.GetMainMenuLayout();
      labelInfo1.Visible = layout.LabelInfo1;
      labelInfo2.Visible = layout.LabelInfo2;
      btnBacklight.Visible = layout.Backlight;
      btnDisplayControl.Visible = layout.DisplayControl;
      btnDisplayOptions.Visible = layout.DisplayOptions;
      btnEqualizer.Visible = layout.Equalizer;
      btnKeyPad.Visible = layout.KeyPad;
      btnRemote.Visible = layout.Remote;
      btnContrast.Visible = layout.Contrast;
      btnMonitorPower.Visible = layout.MonitorPower;
      btnBrightness.Visible = layout.Brightness;
    }
  }
}