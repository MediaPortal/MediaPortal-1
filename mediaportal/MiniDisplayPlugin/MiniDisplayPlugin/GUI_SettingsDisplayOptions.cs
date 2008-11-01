using System;
using System.Windows.Forms;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class GUI_SettingsDisplayOptions : GUIWindow
  {
    [SkinControl(0x23)]
    protected GUIToggleButtonControl btnCustomFont = new GUIToggleButtonControl(0x4da8);
    [SkinControl(0x25)]
    protected GUIToggleButtonControl btnCustomIcons = new GUIToggleButtonControl(0x4da8);
    [SkinControl(0x20)]
    protected GUIToggleButtonControl btnDiskIcon = new GUIToggleButtonControl(0x4da8);
    [SkinControl(0x22)]
    protected GUIToggleButtonControl btnDiskStatus = new GUIToggleButtonControl(0x4da8);
    [SkinControl(0x27)]
    protected GUIButtonControl btnFontEditor;
    [SkinControl(40)]
    protected GUIButtonControl btnIconEditor;
    [SkinControl(0x26)]
    protected GUIToggleButtonControl btnInvertIcons = new GUIToggleButtonControl(0x4da8);
    [SkinControl(0x24)]
    protected GUIToggleButtonControl btnLargeIcons = new GUIToggleButtonControl(0x4da8);
    [SkinControl(0x21)]
    protected GUIToggleButtonControl btnMediaStatus = new GUIToggleButtonControl(0x4da8);
    [SkinControl(0x1f)]
    protected GUIToggleButtonControl btnProgress = new GUIToggleButtonControl(0x4da8);
    [SkinControl(30)]
    protected GUIToggleButtonControl btnVolume = new GUIToggleButtonControl(0x4da8);
    private MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplay.DisplayOptions DisplayOptions = new MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplay.DisplayOptions();
    private bool selectedCustomFont;
    private bool selectedCustomIcons;
    private bool selectedDiskIcon;
    private bool selectedDiskStatus;
    private bool selectedInvertIcons;
    private bool selectedLargeIcons;
    private bool selectedMediaStatus;
    private bool selectedProgress;
    private bool selectedVolume;

    public GUI_SettingsDisplayOptions()
    {
      this.GetID = 0x4da8;
    }

    private void BackupButtons()
    {
      this.selectedVolume = this.btnVolume.Selected;
      this.selectedProgress = this.btnProgress.Selected;
      this.selectedDiskIcon = this.btnDiskIcon.Selected;
      if (this.selectedDiskIcon)
      {
        this.selectedMediaStatus = this.btnMediaStatus.Selected;
        this.selectedDiskStatus = this.btnDiskStatus.Selected;
      }
      this.selectedCustomFont = this.btnCustomFont.Selected;
      this.selectedLargeIcons = this.btnLargeIcons.Selected;
      if (this.selectedLargeIcons)
      {
        this.selectedCustomIcons = this.btnCustomIcons.Selected;
        this.selectedInvertIcons = this.btnInvertIcons.Selected;
      }
    }

    public override bool Init()
    {
      this.Restore();
      return this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayOptions));
    }

    private void LoadSettings()
    {
      this.DisplayOptions = XMLUTILS.LoadDisplayOptionsSettings();
    }

    public override void OnAdded()
    {
      Log.Info("MiniDisplay.GUI_SettingsDisplayOptions.OnAdded(): Window {0} added to window manager", new object[] { this.GetID });
      base.OnAdded();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnVolume)
      {
        this.OnVolumeChanged();
      }
      if (control == this.btnProgress)
      {
        this.OnProgressChanged();
      }
      if (control == this.btnDiskIcon)
      {
        this.OnDiskIconChanged();
      }
      if (control == this.btnMediaStatus)
      {
        this.OnMediaStatusChanged();
      }
      if (control == this.btnDiskStatus)
      {
        this.OnDiskStatusChanged();
      }
      if (control == this.btnCustomFont)
      {
        this.BackupButtons();
        this.SaveSettings();
        XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayOptions);
        this.Restore();
        this.RestoreButtons();
        GUIControl.FocusControl(this.GetID, this.btnCustomFont.GetID);
      }
      if (control == this.btnLargeIcons)
      {
        this.BackupButtons();
        this.SaveSettings();
        XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayOptions);
        this.Restore();
        this.RestoreButtons();
        GUIControl.FocusControl(this.GetID, this.btnLargeIcons.GetID);
      }
      if (control == this.btnCustomIcons)
      {
        this.SaveSettings();
        GUIControl.FocusControl(this.GetID, this.btnCustomIcons.GetID);
      }
      if (control == this.btnInvertIcons)
      {
        this.SaveSettings();
        GUIControl.FocusControl(this.GetID, this.btnInvertIcons.GetID);
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

    private void OnDiskIconChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayOptions);
      this.Restore();
      this.RestoreButtons();
      GUIControl.FocusControl(this.GetID, this.btnDiskIcon.GetID);
    }

    private void OnDiskStatusChanged()
    {
      this.SaveSettings();
      GUIControl.FocusControl(this.GetID, this.btnDiskStatus.GetID);
    }

    private void OnMediaStatusChanged()
    {
      this.SaveSettings();
      GUIControl.FocusControl(this.GetID, this.btnMediaStatus.GetID);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      this.SaveSettings();
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.DisplayOptions);
    }

    protected override void OnPageLoad()
    {
      this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayOptions));
      this.Restore();
      base.LoadSkin();
      base.OnPageLoad();
      this.LoadSettings();
      this.SetVolume();
      this.SetProgress();
      this.SetDiskIcon();
      GUIControl.FocusControl(this.GetID, this.btnVolume.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", "MiniDisplay Display Options");
    }

    private void OnProgressChanged()
    {
      this.SaveSettings();
      GUIControl.FocusControl(this.GetID, this.btnProgress.GetID);
    }

    private void OnVolumeChanged()
    {
      this.SaveSettings();
      GUIControl.FocusControl(this.GetID, this.btnVolume.GetID);
    }

    private void RestoreButtons()
    {
      if (this.selectedVolume)
      {
        GUIControl.SelectControl(this.GetID, this.btnVolume.GetID);
      }
      if (this.selectedProgress)
      {
        GUIControl.SelectControl(this.GetID, this.btnProgress.GetID);
      }
      if (this.selectedDiskIcon)
      {
        GUIControl.SelectControl(this.GetID, this.btnDiskIcon.GetID);
        if (this.selectedMediaStatus)
        {
          GUIControl.SelectControl(this.GetID, this.btnMediaStatus.GetID);
        }
        if (this.selectedDiskStatus)
        {
          GUIControl.SelectControl(this.GetID, this.btnDiskStatus.GetID);
        }
      }
      if (this.selectedCustomFont)
      {
        GUIControl.SelectControl(this.GetID, this.btnCustomFont.GetID);
      }
      if (this.selectedLargeIcons)
      {
        GUIControl.SelectControl(this.GetID, this.btnLargeIcons.GetID);
        if (this.selectedCustomIcons)
        {
          GUIControl.SelectControl(this.GetID, this.btnCustomIcons.GetID);
        }
        if (this.selectedInvertIcons)
        {
          GUIControl.SelectControl(this.GetID, this.btnInvertIcons.GetID);
        }
      }
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
      if (this.DisplayOptions.DiskIcon)
      {
        GUIControl.SelectControl(this.GetID, this.btnDiskIcon.GetID);
      }
    }

    private void SetProgress()
    {
      if (this.DisplayOptions.ProgressDisplay)
      {
        GUIControl.SelectControl(this.GetID, this.btnProgress.GetID);
      }
    }

    private void SetVolume()
    {
      if (this.DisplayOptions.VolumeDisplay)
      {
        GUIControl.SelectControl(this.GetID, this.btnVolume.GetID);
      }
    }
  }
}

