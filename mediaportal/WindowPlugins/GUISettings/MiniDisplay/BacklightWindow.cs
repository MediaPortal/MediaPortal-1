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

using MediaPortal.GUI.Library;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setup
{
  public class BacklightWindow : GUIWindow
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
      return Load(GUIGraphicsContext.Skin + @"\settings_Display_BackLight.xml");
    }

    private void LoadSettings()
    {
      this.BackLightOptions = XMLUTILS.LoadBackLightSettings();
      this.SetRed();
      this.SetGreen();
      this.SetBlue();
      this.SetInverted();
    }

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
      base.OnPageDestroy(newWindowId);
      this.SaveSettings();
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      this.LoadSettings();
      GUIControl.FocusControl(this.GetID, this.btnRed.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(109001));
    }

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