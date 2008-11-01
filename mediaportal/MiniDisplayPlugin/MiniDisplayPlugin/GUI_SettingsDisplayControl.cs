using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class GUI_SettingsDisplayControl : GUIWindow
  {
    [SkinControl(0x29)]
    protected GUIToggleButtonControl btnDisplayAction = new GUIToggleButtonControl(0x4da9);
    [SkinControl(0x2a)]
    protected GUISelectButtonControl btnDisplayActionTime = new GUISelectButtonControl(0x4da9);
    [SkinControl(0x2b)]
    protected GUIToggleButtonControl btnDisplayIdle = new GUIToggleButtonControl(0x4da9);
    [SkinControl(40)]
    protected GUIToggleButtonControl btnDisplayVideo = new GUIToggleButtonControl(0x4da9);
    [SkinControl(0x2c)]
    protected GUISelectButtonControl btnIdleDelay = new GUISelectButtonControl(0x4da9);
    private MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplay.DisplayControl DisplayControl = new MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplay.DisplayControl();
    private int selectedActionDelayIndex = -1;
    private bool selectedDisplayAction;
    private bool selectedDisplayIdle;
    private bool selectedDisplayVideo;
    private int selectedIdleDelayIndex = -1;

    public GUI_SettingsDisplayControl()
    {
      this.GetID = 0x4da9;
    }

    private void BackupButtons()
    {
      this.selectedDisplayVideo = this.btnDisplayVideo.Selected;
      if (this.btnDisplayAction != null)
      {
        this.selectedDisplayAction = this.btnDisplayAction.Selected;
      }
      if (this.btnDisplayActionTime != null)
      {
        this.selectedActionDelayIndex = this.btnDisplayActionTime.SelectedItem;
      }
      this.selectedDisplayIdle = this.btnDisplayIdle.Selected;
      if (this.btnIdleDelay != null)
      {
        this.selectedIdleDelayIndex = this.btnIdleDelay.SelectedItem;
      }
    }

    public override bool Init()
    {
      this.Restore();
      return this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayControl));
    }

    private void LoadSettings()
    {
      this.DisplayControl = XMLUTILS.LoadDisplayControlSettings();
    }

    public override void OnAdded()
    {
      Log.Info("MiniDisplay.GUI_SettingsDisplayControl.OnAdded(): Window {0} added to window manager", new object[] { this.GetID });
      base.OnAdded();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnDisplayVideo)
      {
        this.OnDisplayVideoChanged();
      }
      if (control == this.btnDisplayAction)
      {
        this.OnDisplayActionChanged();
      }
      if (control == this.btnDisplayActionTime)
      {
        this.OnDisplayActionTimeChanged();
      }
      if (control == this.btnDisplayIdle)
      {
        this.OnDisplayIdleChanged();
      }
      if (control == this.btnIdleDelay)
      {
        this.OnIdleDelayChanged();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnDisplayActionChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayControl);
      this.Restore();
      GUIControl.FocusControl(this.GetID, this.btnDisplayAction.GetID);
      this.RestoreButtons();
    }

    private void OnDisplayActionTimeChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      GUIControl.SelectItemControl(this.GetID, this.btnDisplayActionTime.GetID, this.selectedActionDelayIndex);
    }

    private void OnDisplayIdleChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayControl);
      this.Restore();
      GUIControl.FocusControl(this.GetID, this.btnDisplayIdle.GetID);
      this.RestoreButtons();
    }

    private void OnDisplayVideoChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayControl);
      this.Restore();
      GUIControl.FocusControl(this.GetID, this.btnDisplayVideo.GetID);
      this.RestoreButtons();
    }

    private void OnIdleDelayChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      GUIControl.SelectItemControl(this.GetID, this.btnIdleDelay.GetID, this.selectedIdleDelayIndex);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      this.SaveSettings();
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.DisplayControl);
    }

    protected override void OnPageLoad()
    {
      this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.DisplayControl));
      this.Restore();
      base.LoadSkin();
      base.OnPageLoad();
      this.LoadSettings();
      this.SetDisplayVideo();
      this.SetDisplayAction();
      this.SetDisplayActionTime();
      this.SetDisplayIdle();
      this.SetIdleDelay();
      GUIControl.FocusControl(this.GetID, this.btnDisplayVideo.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", "MiniDisplay Display Control Options");
    }

    private void RestoreButtons()
    {
      if (this.selectedDisplayVideo)
      {
        GUIControl.SelectControl(this.GetID, this.btnDisplayVideo.GetID);
      }
      if ((this.btnDisplayAction != null) & this.selectedDisplayAction)
      {
        GUIControl.SelectControl(this.GetID, this.btnDisplayAction.GetID);
      }
      if ((this.btnDisplayAction != null) & (this.btnDisplayActionTime != null))
      {
        GUIControl.SelectItemControl(this.GetID, this.btnDisplayActionTime.GetID, this.selectedActionDelayIndex);
      }
      if (this.selectedDisplayIdle)
      {
        GUIControl.SelectControl(this.GetID, this.btnDisplayIdle.GetID);
      }
      if (this.btnIdleDelay != null)
      {
        GUIControl.SelectItemControl(this.GetID, this.btnIdleDelay.GetID, this.selectedIdleDelayIndex);
      }
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
        for (int i = 0; i < 0x1f; i++)
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
        for (int i = 0; i < 0x1f; i++)
        {
          GUIControl.AddItemLabelControl(this.GetID, this.btnIdleDelay.GetID, i.ToString());
        }
        GUIControl.SelectItemControl(this.GetID, this.btnIdleDelay.GetID, this.DisplayControl.BlankIdleDelay);
      }
    }
  }
}

