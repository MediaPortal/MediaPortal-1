using System;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class GUI_SettingsMain : GUIWindow
  {
    private string _LoadedSkin = string.Empty;
    [SkinControl(8)]
    protected GUISelectButtonControl btnBrightness;
    [SkinControl(7)]
    protected GUISelectButtonControl btnContrast;
    [SkinControl(6)]
    protected GUIToggleButtonControl btnMonitorPower;
    private int LastControlID = -1;

    public GUI_SettingsMain()
    {
      this.GetID = 0x4da6;
    }

    public override bool Init()
    {
      this.Restore();
      return this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.MainMenu));
    }

    public override void OnAdded()
    {
      Log.Info("MiniDisplay.GUI_SettingsMain.OnAdded(): Window {0} added to window manager", new object[] { this.GetID });
      base.OnAdded();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnMonitorPower)
      {
        iMONLCDg.AdvancedSettings toSave = iMONLCDg.AdvancedSettings.Load();
        toSave.MonitorPowerState = this.btnMonitorPower.Selected;
        iMONLCDg.AdvancedSettings.Save(toSave);
        iMONLCDg.AdvancedSettings.NotifyDriver();
      }
      if (control == this.btnContrast)
      {
        Settings.Instance.Contrast = this.btnContrast.SelectedItem;
        Settings.Save();
        GUIControl.SelectItemControl(this.GetID, this.btnContrast.GetID, Settings.Instance.Contrast);
        string type = Settings.Instance.Type;
        if (type != null)
        {
          if (!(type == "iMONLCDg"))
          {
            if (type == "VLSYS_Mplay")
            {
              VLSYS_Mplay.AdvancedSettings.NotifyDriver();
            }
            else if (type == "MatrixMX")
            {
              MatrixMX.AdvancedSettings.NotifyDriver();
            }
            else if (type == "MatrixGX")
            {
              MatrixGX.AdvancedSettings.NotifyDriver();
            }
            else if (type == "CFontz")
            {
              CFontz.AdvancedSettings.NotifyDriver();
            }
            else if (type == "MD8800")
            {
              MD8800.AdvancedSettings.NotifyDriver();
            }
          }
          else
          {
            iMONLCDg.AdvancedSettings.NotifyDriver();
          }
        }
      }
      if (control == this.btnBrightness)
      {
        Settings.Instance.Backlight = this.btnBrightness.SelectedItem;
        Settings.Save();
        GUIControl.SelectItemControl(this.GetID, this.btnBrightness.GetID, Settings.Instance.Backlight);
        string str2 = Settings.Instance.Type;
        if (str2 != null)
        {
          if (!(str2 == "iMONLCDg"))
          {
            if (str2 == "VLSYS_Mplay")
            {
              VLSYS_Mplay.AdvancedSettings.NotifyDriver();
            }
            else if (str2 == "MatrixMX")
            {
              MatrixMX.AdvancedSettings.NotifyDriver();
            }
            else if (str2 == "MatrixGX")
            {
              MatrixGX.AdvancedSettings.NotifyDriver();
            }
            else if (str2 == "CFontz")
            {
              CFontz.AdvancedSettings.NotifyDriver();
            }
            else if (str2 == "MD8800")
            {
              MD8800.AdvancedSettings.NotifyDriver();
            }
          }
          else
          {
            iMONLCDg.AdvancedSettings.NotifyDriver();
          }
        }
      }
      this.LastControlID = controlId;
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      XMLUTILS.Delete_GUI_Menu(XMLUTILS.GUIMenu.MainMenu);
    }

    protected override void OnPageLoad()
    {
      Log.Info("MiniDisplay.GUI_SettingsMain.OnPageLoad(): called", new object[0]);
      this.Load(XMLUTILS.Create_GUI_Menu(XMLUTILS.GUIMenu.MainMenu));
      this.Restore();
      base.LoadSkin();
      base.OnPageLoad();
      this.SetContrast();
      this.SetBrightness();
      GUIPropertyManager.SetProperty("#currentmodule", "MiniDisplay Setup");
      if (this.LastControlID == -1)
      {
        GUIControl.FocusControl(this.GetID, 3);
      }
      else
      {
        GUIControl.FocusControl(this.GetID, this.LastControlID);
      }
    }

    private void SetBrightness()
    {
      if (this.btnBrightness != null)
      {
        this.btnBrightness.Clear();
        for (int i = 0; i < 0x100; i++)
        {
          GUIControl.AddItemLabelControl(this.GetID, this.btnBrightness.GetID, i.ToString());
        }
        GUIControl.SelectItemControl(this.GetID, this.btnBrightness.GetID, Settings.Instance.Backlight);
      }
    }

    private void SetContrast()
    {
      this.btnContrast.Clear();
      for (int i = 0; i < 0x100; i++)
      {
        GUIControl.AddItemLabelControl(this.GetID, this.btnContrast.GetID, i.ToString());
      }
      if (Settings.Instance.Type == "iMONLCDg")
      {
        iMONLCDg.AdvancedSettings.Load();
        this.btnMonitorPower.Selected = this.btnMonitorPower.Selected;
      }
      GUIControl.SelectItemControl(this.GetID, this.btnContrast.GetID, Settings.Instance.Contrast);
    }
  }
}

