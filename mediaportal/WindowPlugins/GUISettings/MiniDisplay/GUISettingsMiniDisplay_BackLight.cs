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
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setup
{
  public class BacklightWindow : GUIInternalWindow
  {
    private MatrixGX.MOGX_Control BackLightOptions = new MatrixGX.MOGX_Control();
    [SkinControl(65)] protected GUISelectButtonControl btnRed = null;
    [SkinControl(66)] protected GUISelectButtonControl btnGreen = null;
    [SkinControl(67)] protected GUISelectButtonControl btnBlue = null;
    [SkinControl(68)] protected GUIToggleButtonControl btnInvert = null;

    public BacklightWindow()
    {
      this.GetID = 9001;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_MiniDisplay_BackLight.xml"));
    }

    #region Serialization

    private void LoadSettings()
    {
      this.BackLightOptions = XMLUTILS.LoadBackLightSettings();
      this.SetRed();
      this.SetGreen();
      this.SetBlue();
      this.SetInverted();
    }

    #endregion

    #region overrides

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnRed)
      {
        this.SaveSettings();
      }
      if (control == this.btnGreen)
      {
        this.SaveSettings();
      }
      if (control == this.btnBlue)
      {
        this.SaveSettings();
      }
      if (control == this.btnInvert)
      {
        this.SaveSettings();
      }
      base.OnClicked(controlId, control, actionType);
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

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      this.LoadSettings();
      GUIControl.FocusControl(this.GetID, this.btnRed.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(109001));

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

    private void SaveSettings()
    {
      this.BackLightOptions.BackLightRed = this.btnRed.SelectedItem;
      this.BackLightOptions.BackLightGreen = this.btnGreen.SelectedItem;
      this.BackLightOptions.BackLightBlue = this.btnBlue.SelectedItem;
      this.BackLightOptions.InvertDisplay = this.btnInvert.Selected;
      XMLUTILS.SaveBackLightSettings(this.BackLightOptions);
    }

    private void SetBlue()
    {
      Log.Info("MiniDisplay.GUI_SettingsBacklight.SetBlue(): setting Blue = {0}\n",
               new object[] {this.BackLightOptions.BackLightBlue});
      GUIControl.ClearControl(this.GetID, this.btnBlue.GetID);
      for (int i = 0; i < 256; i++)
      {
        GUIControl.AddItemLabelControl(this.GetID, this.btnBlue.GetID, i.ToString());
      }
      GUIControl.SelectItemControl(this.GetID, this.btnBlue.GetID, this.BackLightOptions.BackLightBlue);
    }

    private void SetGreen()
    {
      GUIControl.ClearControl(this.GetID, this.btnGreen.GetID);
      for (int i = 0; i < 256; i++)
      {
        GUIControl.AddItemLabelControl(this.GetID, this.btnGreen.GetID, i.ToString());
      }
      GUIControl.SelectItemControl(this.GetID, this.btnGreen.GetID, this.BackLightOptions.BackLightGreen);
    }

    private void SetInverted()
    {
      if (this.BackLightOptions.InvertDisplay)
      {
        GUIControl.SelectControl(this.GetID, this.btnInvert.GetID);
      }
    }

    private void SetRed()
    {
      GUIControl.ClearControl(this.GetID, this.btnRed.GetID);
      for (int i = 0; i < 256; i++)
      {
        GUIControl.AddItemLabelControl(this.GetID, this.btnRed.GetID, i.ToString());
      }
      GUIControl.SelectItemControl(this.GetID, this.btnRed.GetID, this.BackLightOptions.BackLightRed);
    }
  }
}