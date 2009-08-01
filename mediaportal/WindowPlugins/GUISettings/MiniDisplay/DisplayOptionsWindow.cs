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

using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setup
{
  public class DisplayOptionsWindow : GUIInternalWindow
  {
    [SkinControl(30)] protected GUIToggleButtonControl btnVolume = null;
    [SkinControl(31)] protected GUIToggleButtonControl btnProgress = null;
    [SkinControl(32)] protected GUIToggleButtonControl btnDiskIcon = null;
    [SkinControl(33)] protected GUIToggleButtonControl btnMediaStatus = null;
    [SkinControl(34)] protected GUIToggleButtonControl btnDiskStatus = null;
    [SkinControl(35)] protected GUIToggleButtonControl btnCustomFont = null;
    [SkinControl(36)] protected GUIToggleButtonControl btnLargeIcons = null;
    [SkinControl(37)] protected GUIToggleButtonControl btnCustomIcons = null;
    [SkinControl(38)] protected GUIToggleButtonControl btnInvertIcons = null;
    [SkinControl(39)] protected GUIButtonControl btnFontEditor = null;
    [SkinControl(40)] protected GUIButtonControl btnIconEditor = null;
    private DisplayOptions DisplayOptions = new DisplayOptions();

    public DisplayOptionsWindow()
    {
      this.GetID = 9003;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_Display_DisplayOptions.xml");
    }

    private void LoadSettings()
    {
      this.DisplayOptions = XMLUTILS.LoadDisplayOptionsSettings();
      this.SetVolume();
      this.SetProgress();
      this.SetDiskIcon();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnVolume)
      {
        this.SaveSettings();
      }
      if (control == this.btnProgress)
      {
        this.SaveSettings();
      }
      if (control == this.btnDiskIcon)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnMediaStatus)
      {
        this.SaveSettings();
      }
      if (control == this.btnDiskStatus)
      {
        this.SaveSettings();
      }
      if (control == this.btnCustomFont)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnLargeIcons)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnCustomIcons)
      {
        this.SaveSettings();
      }
      if (control == this.btnInvertIcons)
      {
        this.SaveSettings();
      }
      if (control == this.btnFontEditor)
      {
        Form form = new iMONLCDg_FontEdit();
        form.ShowDialog();
        form.Dispose();
      }
      if (control == this.btnIconEditor)
      {
        Form form2 = new iMONLCDg_IconEdit();
        form2.ShowDialog();
        form2.Dispose();
      }
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      this.SaveSettings();
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
      GUIControl.FocusControl(this.GetID, this.btnVolume.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(109003));
    }

    private void SaveSettings()
    {
      this.DisplayOptions.VolumeDisplay = this.btnVolume.Selected;
      this.DisplayOptions.ProgressDisplay = this.btnProgress.Selected;
      this.DisplayOptions.DiskIcon = this.btnDiskIcon.Selected;
      if (this.btnMediaStatus != null)
      {
        this.DisplayOptions.DiskMediaStatus = this.btnMediaStatus.Selected;
      }
      if (this.btnDiskStatus != null)
      {
        this.DisplayOptions.DiskMonitor = this.btnDiskStatus.Selected;
      }
      this.DisplayOptions.UseCustomFont = this.btnCustomFont.Selected;
      this.DisplayOptions.UseLargeIcons = this.btnLargeIcons.Selected;
      if (this.btnCustomIcons != null)
      {
        this.DisplayOptions.UseCustomIcons = this.btnCustomIcons.Selected;
      }
      if (this.btnInvertIcons != null)
      {
        this.DisplayOptions.UseInvertedIcons = this.btnInvertIcons.Selected;
      }
      XMLUTILS.SaveDisplayOptionsSettings(this.DisplayOptions);
    }

    private void SetDiskIcon()
    {
      if (DisplayOptions.DiskIcon)
      {
        GUIControl.SelectControl(this.GetID, this.btnDiskIcon.GetID);
      }
    }

    private void SetProgress()
    {
      if (DisplayOptions.ProgressDisplay)
      {
        GUIControl.SelectControl(this.GetID, this.btnProgress.GetID);
      }
    }

    private void SetVolume()
    {
      if (DisplayOptions.VolumeDisplay)
      {
        GUIControl.SelectControl(this.GetID, this.btnVolume.GetID);
      }
    }

    private void SetButtons()
    {
      DisplayOptionsLayout layout = XMLUTILS.GetDisplayOptionsLayout(DisplayOptions);
      btnVolume.Visible = layout.Volume;
      btnProgress.Visible = layout.Progress;
      btnDiskIcon.Visible = layout.DiskIcon;
      btnMediaStatus.Visible = layout.MediaStatus;
      btnDiskStatus.Visible = layout.DiskStatus;
      btnCustomFont.Visible = layout.CustomFont;
      btnLargeIcons.Visible = layout.LargeIcons;
      btnCustomIcons.Visible = layout.CustomIcons;
      btnInvertIcons.Visible = layout.InvertIcons;
      btnFontEditor.Visible = layout.FontEditor;
      btnIconEditor.Visible = layout.IconEditor;
    }
  }
}