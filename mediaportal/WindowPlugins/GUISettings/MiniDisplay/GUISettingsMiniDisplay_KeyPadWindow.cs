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
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setup
{
  public class KeyPadWindow : GUIInternalWindow
  {
    [SkinControl(50)] protected GUILabelControl label1 = null;
    [SkinControl(60)] protected GUIToggleButtonControl btnEnableKeyPad = null;
    [SkinControl(61)] protected GUIToggleButtonControl btnEnableCustom = null;
    [SkinControl(62)] protected GUIButtonControl btnKeyPadMapping = null;
    private MatrixMX.KeyPadControl KPSettings = new MatrixMX.KeyPadControl();

    public KeyPadWindow()
    {
      this.GetID = 9005;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_MiniDisplay_Keypad.xml"));
    }

    private void LoadSettings()
    {
      this.KPSettings = XMLUTILS.LoadKeyPadSettings();
      this.SetEnableKeyPad();
      this.SetEnableCustom();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnEnableKeyPad)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnEnableCustom)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnKeyPadMapping)
      {
        try
        {
          if (!File.Exists(MatrixMX.DefaultMappingPath))
          {
            MatrixMX.AdvancedSettings.CreateDefaultKeyPadMapping();
          }
          new InputMappingForm("MatrixMX_Keypad").ShowDialog();
        }
        catch (Exception exception)
        {
          Log.Info("VLSYS_AdvancedSetupForm.btnRemoteSetup_Click() CAUGHT EXCEPTION: {0}", new object[] {exception});
        }
      }
      base.OnClicked(controlId, control, actionType);
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
      GUIControl.FocusControl(this.GetID, this.btnEnableKeyPad.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(109005));

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
      SaveSettings();

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

    private void SaveSettings()
    {
      this.KPSettings.EnableKeyPad = this.btnEnableKeyPad.Selected;
      this.KPSettings.EnableCustom = this.btnEnableCustom.Selected;
      XMLUTILS.SaveKeyPadSettings(this.KPSettings);
    }

    private void SetEnableCustom()
    {
      if (this.KPSettings.EnableKeyPad)
      {
        this.btnEnableCustom.Selected = this.KPSettings.EnableCustom;
      }
    }

    private void SetEnableKeyPad()
    {
      this.btnEnableKeyPad.Selected = this.KPSettings.EnableKeyPad;
    }

    private void SetButtons()
    {
      KeyPadLayout layout = XMLUTILS.GetKeyPadLayout(KPSettings);
      btnEnableKeyPad.Visible = layout.EnableKeyPad;
      btnEnableCustom.Visible = layout.EnableCustom;
      btnKeyPadMapping.Visible = layout.KeyPadMapping;
      label1.Visible = layout.Label1;
    }
  }
}