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

using System;
using System.IO;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setup
{
  public class KeyPadWindow : GUIWindow
  {
    [SkinControlAttribute(50)]
    protected GUILabelControl label1 = null;
    [SkinControlAttribute(60)]
    protected GUIToggleButtonControl btnEnableKeyPad = null;
    [SkinControlAttribute(61)]
    protected GUIToggleButtonControl btnEnableCustom = null;
    [SkinControlAttribute(62)]
    protected GUIButtonControl btnKeyPadMapping = null;
    private MatrixMX.KeyPadControl KPSettings = new MatrixMX.KeyPadControl();

    public KeyPadWindow()
    {
      this.GetID = 9005;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_Display_Keypad.xml");
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
          if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "MatrixMX_Keypad.xml")))
          {
            MatrixMX.AdvancedSettings.CreateDefaultKeyPadMapping();
          }
          new InputMappingForm("MatrixMX_Keypad").ShowDialog();
        } catch (Exception exception)
        {
          Log.Info("VLSYS_AdvancedSetupForm.btnRemoteSetup_Click() CAUGHT EXCEPTION: {0}", new object[] { exception });
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      this.SaveSettings();
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
      GUIControl.FocusControl(this.GetID, this.btnEnableKeyPad.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(109005));
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

