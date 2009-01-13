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
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setup
{
  public class RemoteWindow : GUIWindow
  {
    [SkinControl(40)] protected GUILabelControl label1 = null;
    [SkinControl(50)] protected GUIToggleButtonControl btnDisableRemote = null;
    [SkinControl(51)] protected GUIToggleButtonControl btnDisableRepeat = null;
    [SkinControl(52)] protected GUISelectButtonControl btnRepeatDelay = null;
    [SkinControl(53)] protected GUIButtonControl btnRemoteMapping = null;

    private VLSYS_Mplay.RemoteControl RCSettings = new VLSYS_Mplay.RemoteControl();

    public RemoteWindow()
    {
      this.GetID = 9006;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_Display_Remote.xml");
    }

    private void LoadSettings()
    {
      this.RCSettings = XMLUTILS.LoadRemoteSettings();
      this.SetDisableRemote();
      this.SetDisableRepeat();
      this.SetRepeatDelay();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnDisableRemote)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnDisableRepeat)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnRepeatDelay)
      {
        this.SaveSettings();
      }
      if (control == this.btnRemoteMapping)
      {
        try
        {
          if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "VLSYS_Mplay.xml")))
          {
            VLSYS_Mplay.AdvancedSettings.CreateDefaultRemoteMapping();
          }
          new InputMappingForm("VLSYS_Mplay").ShowDialog();
        }
        catch (Exception exception)
        {
          Log.Info("VLSYS_AdvancedSetupForm.btnRemoteSetup_Click() CAUGHT EXCEPTION: {0}", new object[] {exception});
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
      GUIControl.FocusControl(this.GetID, this.btnDisableRemote.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(109006));
    }

    private void SaveSettings()
    {
      this.RCSettings.DisableRemote = this.btnDisableRemote.Selected;
      this.RCSettings.DisableRepeat = this.btnDisableRepeat.Selected;
      this.RCSettings.RepeatDelay = this.btnRepeatDelay.SelectedItem;
      XMLUTILS.SaveRemoteSettings(this.RCSettings);
    }

    private void SetDisableRemote()
    {
      this.btnDisableRemote.Selected = this.RCSettings.DisableRemote;
    }

    private void SetDisableRepeat()
    {
      if (this.RCSettings.DisableRemote)
      {
        this.btnDisableRepeat.Selected = this.RCSettings.DisableRepeat;
      }
    }

    private void SetRepeatDelay()
    {
      if (btnRepeatDelay != null)
      {
        GUIControl.ClearControl(this.GetID, this.btnRepeatDelay.GetID);
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "0");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "25");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "50");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "75");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "100");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "125");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "150");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "175");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "200");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "225");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "250");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "275");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "300");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "325");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "350");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "375");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "400");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "425");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "450");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "475");
        GUIControl.AddItemLabelControl(this.GetID, this.btnRepeatDelay.GetID, "500");
        GUIControl.SelectItemControl(this.GetID, this.btnRepeatDelay.GetID, this.RCSettings.RepeatDelay);
      }
    }

    private void SetButtons()
    {
      RemoteLayout layout = XMLUTILS.GetRemoteLayout(RCSettings);
      label1.Visible = layout.Label1;
      btnDisableRemote.Visible = layout.DisableRemote;
      btnDisableRepeat.Visible = layout.DisableRepeat;
      btnRepeatDelay.Visible = layout.RepeatDelay;
      btnRemoteMapping.Visible = layout.RemoteMapping;
    }
  }
}