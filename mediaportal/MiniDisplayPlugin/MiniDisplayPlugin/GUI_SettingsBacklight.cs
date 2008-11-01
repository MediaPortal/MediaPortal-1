using System;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class GUI_SettingsBacklight : GUIWindow
  {
    private MatrixGX.MOGX_Control BackLightOptions = new MatrixGX.MOGX_Control();
    [SkinControl(0x43)]
    protected GUISelectButtonControl btnBlue = new GUISelectButtonControl(0x4dac);
    [SkinControl(0x42)]
    protected GUISelectButtonControl btnGreen = new GUISelectButtonControl(0x4dac);
    [SkinControl(0x44)]
    protected GUIToggleButtonControl btnInvert = new GUIToggleButtonControl(0x4dac);
    [SkinControl(0x41)]
    protected GUISelectButtonControl btnRed = new GUISelectButtonControl(0x4dac);
    private int selectedBlue;
    private int selectedGreen;
    private bool selectedInvert;
    private int selectedRed;

    public GUI_SettingsBacklight()
    {
      this.GetID = 0x4dac;
    }

    private void BackupButtons()
    {
      this.selectedRed = this.btnRed.SelectedItem;
      this.selectedGreen = this.btnGreen.SelectedItem;
      this.selectedBlue = this.btnBlue.SelectedItem;
      this.selectedInvert = this.btnInvert.Selected;
    }

    public override bool Init()
    {
      this.Restore();
      return this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.BackLight));
    }

    private void LoadSettings()
    {
      this.BackLightOptions = XMLUTILS.LoadBackLightSettings();
    }

    public override void OnAdded()
    {
      Log.Info("MiniDisplay.GUI_SettingsBacklight.OnAdded(): Window {0} added to window manager", new object[] { this.GetID });
      base.OnAdded();
    }

    private void OnBlueChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      this.RestoreButtons();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnRed)
      {
        this.OnRedChanged();
      }
      if (control == this.btnGreen)
      {
        this.OnGreenChanged();
      }
      if (control == this.btnBlue)
      {
        this.OnBlueChanged();
      }
      if (control == this.btnInvert)
      {
        this.OnInvertedChanged();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnGreenChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      this.RestoreButtons();
    }

    private void OnInvertedChanged()
    {
      this.SaveSettings();
      GUIControl.FocusControl(this.GetID, this.btnInvert.GetID);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      this.SaveSettings();
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.BackLight);
    }

    protected override void OnPageLoad()
    {
      this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.BackLight));
      this.Restore();
      base.LoadSkin();
      base.OnPageLoad();
      this.LoadSettings();
      this.SetRed();
      this.SetGreen();
      this.SetBlue();
      this.SetInverted();
      GUIControl.FocusControl(this.GetID, this.btnRed.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", "MiniDisplay BackLight Options");
    }

    private void OnRedChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      this.RestoreButtons();
    }

    private void RestoreButtons()
    {
      GUIControl.SelectItemControl(this.GetID, this.btnRed.GetID, this.selectedRed);
      GUIControl.SelectItemControl(this.GetID, this.btnGreen.GetID, this.selectedGreen);
      GUIControl.SelectItemControl(this.GetID, this.btnBlue.GetID, this.selectedBlue);
      if (this.selectedInvert)
      {
        GUIControl.SelectControl(this.GetID, this.btnInvert.GetID);
      }
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
      Log.Info("MiniDisplay.GUI_SettingsBacklight.SetBlue(): setting Blue = {0}\n", new object[] { this.BackLightOptions.BackLightBlue });
      GUIControl.ClearControl(this.GetID, this.btnBlue.GetID);
      for (int i = 0; i < 0x100; i++)
      {
        GUIControl.AddItemLabelControl(this.GetID, this.btnBlue.GetID, i.ToString());
      }
      GUIControl.SelectItemControl(this.GetID, this.btnBlue.GetID, this.BackLightOptions.BackLightBlue);
    }

    private void SetGreen()
    {
      GUIControl.ClearControl(this.GetID, this.btnGreen.GetID);
      for (int i = 0; i < 0x100; i++)
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
      for (int i = 0; i < 0x100; i++)
      {
        GUIControl.AddItemLabelControl(this.GetID, this.btnRed.GetID, i.ToString());
      }
      GUIControl.SelectItemControl(this.GetID, this.btnRed.GetID, this.BackLightOptions.BackLightRed);
    }
  }
}

