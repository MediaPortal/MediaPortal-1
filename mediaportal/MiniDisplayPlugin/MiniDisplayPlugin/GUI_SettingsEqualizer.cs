using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class GUI_SettingsEqualizer : GUIWindow
  {
    [SkinControl(0x17)]
    protected GUIToggleButtonControl btnDelayStart = new GUIToggleButtonControl(0x4da7);
    [SkinControl(0x18)]
    protected GUISelectButtonControl btnDelayStartTime = new GUISelectButtonControl(0x4da7);
    [SkinControl(0x19)]
    protected GUIToggleButtonControl btnShowTitle = new GUIToggleButtonControl(0x4da7);
    [SkinControl(0x1b)]
    protected GUISelectButtonControl btnShowTitleFreq = new GUISelectButtonControl(0x4da7);
    [SkinControl(0x1a)]
    protected GUISelectButtonControl btnShowTitleTime = new GUISelectButtonControl(0x4da7);
    [SkinControl(0x16)]
    protected GUIToggleButtonControl btnSmothEQ = new GUIToggleButtonControl(0x4da7);
    [SkinControl(20)]
    protected GUIToggleButtonControl btnUseEqualizer = new GUIToggleButtonControl(0x4da7);
    [SkinControl(0x15)]
    protected GUISelectButtonControl btnUseStyle = new GUISelectButtonControl(0x4da7);
    private MiniDisplay.EQControl EQSettings = new MiniDisplay.EQControl();
    private bool selectedDelayStart;
    private int selectedDelayStartIndex;
    private bool selectedShowTitle;
    private int selectedShowTitleIndex1;
    private int selectedShowTitleIndex2;
    private bool selectedSmoothEQ;
    private bool selectedUseEqualizer;
    private int selectedUseStyleIndex;

    public GUI_SettingsEqualizer()
    {
      this.GetID = 0x4da7;
    }

    private void BackupButtons()
    {
      this.selectedUseEqualizer = this.btnUseEqualizer.Selected;
      this.selectedUseStyleIndex = this.btnUseStyle.SelectedItem;
      this.selectedSmoothEQ = this.btnSmothEQ.Selected;
      this.selectedDelayStart = this.btnDelayStart.Selected;
      if (this.selectedDelayStart)
      {
        this.selectedDelayStartIndex = this.btnDelayStartTime.SelectedItem;
      }
      this.selectedShowTitle = this.btnShowTitle.Selected;
      if (this.selectedShowTitle)
      {
        this.selectedShowTitleIndex1 = this.btnShowTitleTime.SelectedItem;
      }
      if (this.selectedShowTitle)
      {
        this.selectedShowTitleIndex2 = this.btnShowTitleFreq.SelectedItem;
      }
    }

    public override bool Init()
    {
      this.Restore();
      return this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.Equalizer));
    }

    private void LoadSettings()
    {
      this.EQSettings = XMLUTILS.LoadEqualizerSettings();
    }

    public override void OnAdded()
    {
      Log.Info("MiniDisplay.GUI_Settings_Equalizer.OnAdded(): Window {0} added to window manager", new object[] { this.GetID });
      base.OnAdded();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnUseEqualizer)
      {
        this.OnUseEqChanged();
      }
      if (control == this.btnSmothEQ)
      {
        this.OnSmoothChanged();
      }
      if (control == this.btnUseStyle)
      {
        this.OnStyleChanged();
        GUIControl.FocusControl(this.GetID, controlId);
      }
      if (control == this.btnDelayStart)
      {
        this.OnDelayStartChanged();
      }
      if (control == this.btnDelayStartTime)
      {
        this.BackupButtons();
        this.SaveSettings();
        GUIControl.SelectItemControl(this.GetID, this.btnDelayStartTime.GetID, this.selectedDelayStartIndex);
      }
      if (control == this.btnShowTitle)
      {
        this.OnShowTitleChanged();
      }
      if (control == this.btnShowTitleTime)
      {
        this.BackupButtons();
        this.SaveSettings();
        GUIControl.SelectItemControl(this.GetID, this.btnShowTitleTime.GetID, this.selectedShowTitleIndex1);
      }
      if (control == this.btnShowTitleFreq)
      {
        this.BackupButtons();
        this.SaveSettings();
        GUIControl.SelectItemControl(this.GetID, this.btnShowTitleFreq.GetID, this.selectedShowTitleIndex2);
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnDelayStartChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.Equalizer);
      this.Restore();
      GUIControl.FocusControl(this.GetID, this.btnDelayStart.GetID);
      this.RestoreButtons();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      this.SaveSettings();
      base.OnPageDestroy(newWindowId);
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.Equalizer);
    }

    protected override void OnPageLoad()
    {
      this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.Equalizer));
      this.Restore();
      base.LoadSkin();
      base.OnPageLoad();
      this.LoadSettings();
      this.SetUseEqualizer();
      this.SetUseStyle();
      this.SetUseSmoothEQ();
      this.SetDelayStart();
      this.SetShowTitle();
      GUIControl.FocusControl(this.GetID, this.btnUseEqualizer.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", "MiniDisplay Equalizer Setup");
    }

    private void OnShowTitleChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.Equalizer);
      this.Restore();
      GUIControl.FocusControl(this.GetID, this.btnShowTitle.GetID);
      this.RestoreButtons();
    }

    private void OnSmoothChanged()
    {
      this.SaveSettings();
      GUIControl.FocusControl(this.GetID, this.btnUseStyle.GetID);
    }

    private void OnStyleChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      this.RestoreButtons();
    }

    private void OnUseEqChanged()
    {
      this.BackupButtons();
      this.SaveSettings();
      XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.Equalizer);
      this.Restore();
      GUIControl.FocusControl(this.GetID, this.btnUseEqualizer.GetID);
      this.RestoreButtons();
    }

    private void RestoreButtons()
    {
      if (this.selectedUseEqualizer)
      {
        GUIControl.SelectControl(this.GetID, this.btnUseEqualizer.GetID);
      }
      if (this.btnUseStyle != null)
      {
        this.SetUseStyle();
      }
      if (this.selectedSmoothEQ)
      {
        GUIControl.SelectControl(this.GetID, this.btnSmothEQ.GetID);
      }
      if (this.selectedDelayStart)
      {
        GUIControl.SelectControl(this.GetID, this.btnDelayStart.GetID);
      }
      if (this.selectedDelayStart)
      {
        this.SetDelayStart();
      }
      if (this.selectedShowTitle)
      {
        GUIControl.SelectControl(this.GetID, this.btnShowTitle.GetID);
      }
      if (this.selectedShowTitle)
      {
        this.SetShowTitle();
      }
    }

    private void SaveSettings()
    {
      this.EQSettings.UseEqDisplay = this.btnUseEqualizer.Selected;
      this.EQSettings.SmoothEQ = this.btnSmothEQ.Selected;
      if (this.btnUseStyle != null)
      {
        switch (this.btnUseStyle.SelectedItem)
        {
          case 0:
            this.EQSettings.UseNormalEq = true;
            this.EQSettings.UseStereoEq = false;
            this.EQSettings.UseVUmeter = false;
            this.EQSettings.UseVUmeter2 = false;
            break;

          case 1:
            this.EQSettings.UseNormalEq = false;
            this.EQSettings.UseStereoEq = true;
            this.EQSettings.UseVUmeter = false;
            this.EQSettings.UseVUmeter2 = false;
            break;

          case 2:
            this.EQSettings.UseNormalEq = false;
            this.EQSettings.UseStereoEq = false;
            this.EQSettings.UseVUmeter = true;
            this.EQSettings.UseVUmeter2 = false;
            break;

          case 3:
            this.EQSettings.UseNormalEq = false;
            this.EQSettings.UseStereoEq = false;
            this.EQSettings.UseVUmeter = false;
            this.EQSettings.UseVUmeter2 = true;
            break;
        }
      }
      this.EQSettings.DelayEQ = this.btnDelayStart.Selected;
      if (this.EQSettings.DelayEQ)
      {
        this.EQSettings._DelayEQTime = this.btnDelayStartTime.SelectedItem;
      }
      this.EQSettings.EQTitleDisplay = this.btnShowTitle.Selected;
      if (this.EQSettings.EQTitleDisplay)
      {
        this.EQSettings._EQTitleShowTime = this.btnShowTitleTime.SelectedItem;
        this.EQSettings._EQTitleDisplayTime = this.btnShowTitleFreq.SelectedItem;
      }
      XMLUTILS.SaveEqualizerSettings(this.EQSettings);
    }

    private void SetDelayStart()
    {
      if (this.EQSettings.DelayEQ)
      {
        GUIControl.ClearControl(this.GetID, this.btnDelayStart.GetID);
        for (int i = 0; i < 0x1f; i++)
        {
          GUIControl.AddItemLabelControl(this.GetID, this.btnDelayStartTime.GetID, i.ToString());
        }
        GUIControl.SelectItemControl(this.GetID, this.btnDelayStartTime.GetID, this.EQSettings._DelayEQTime);
      }
    }

    private void SetShowTitle()
    {
      if (this.EQSettings.EQTitleDisplay)
      {
        GUIControl.ClearControl(this.GetID, this.btnShowTitleTime.GetID);
        GUIControl.ClearControl(this.GetID, this.btnShowTitleFreq.GetID);
        for (int i = 0; i < 0x1f; i++)
        {
          GUIControl.AddItemLabelControl(this.GetID, this.btnShowTitleTime.GetID, i.ToString());
          GUIControl.AddItemLabelControl(this.GetID, this.btnShowTitleFreq.GetID, i.ToString());
        }
        GUIControl.SelectItemControl(this.GetID, this.btnShowTitleTime.GetID, this.EQSettings._EQTitleShowTime);
        GUIControl.SelectItemControl(this.GetID, this.btnShowTitleFreq.GetID, this.EQSettings._EQTitleDisplayTime);
      }
    }

    private void SetUseEqualizer()
    {
      this.btnUseEqualizer.Selected = this.EQSettings.UseEqDisplay;
    }

    private void SetUseSmoothEQ()
    {
      if (this.EQSettings.UseEqDisplay)
      {
        this.btnSmothEQ.Selected = this.EQSettings.SmoothEQ;
      }
    }

    private void SetUseStyle()
    {
      if (this.EQSettings.UseEqDisplay)
      {
        int iItem = this.EQSettings.UseNormalEq ? 0 : (this.EQSettings.UseStereoEq ? 1 : (this.EQSettings.UseVUmeter ? 2 : 3));
        GUIControl.ClearControl(this.GetID, this.btnUseStyle.GetID);
        GUIControl.AddItemLabelControl(this.GetID, this.btnUseStyle.GetID, "Normal");
        GUIControl.AddItemLabelControl(this.GetID, this.btnUseStyle.GetID, "Stereo");
        GUIControl.AddItemLabelControl(this.GetID, this.btnUseStyle.GetID, "VU Meter");
        GUIControl.AddItemLabelControl(this.GetID, this.btnUseStyle.GetID, "VU Meter 2");
        GUIControl.SelectItemControl(this.GetID, this.btnUseStyle.GetID, iItem);
      }
    }
  }
}

