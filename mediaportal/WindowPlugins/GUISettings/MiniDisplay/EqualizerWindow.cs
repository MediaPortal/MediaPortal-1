#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setup
{
  public class EqualizerWindow : GUIInternalWindow
  {
    [SkinControl(20)] protected GUIToggleButtonControl btnUseEqualizer = null;
    [SkinControl(21)] protected GUISelectButtonControl btnUseStyle = null;
    [SkinControl(22)] protected GUIToggleButtonControl btnSmothEQ = null;
    [SkinControl(23)] protected GUIToggleButtonControl btnDelayStart = null;
    [SkinControl(24)] protected GUISelectButtonControl btnDelayStartTime = null;
    [SkinControl(25)] protected GUIToggleButtonControl btnShowTitle = null;
    [SkinControl(26)] protected GUISelectButtonControl btnShowTitleTime = null;
    [SkinControl(27)] protected GUISelectButtonControl btnShowTitleFreq = null;
    private EQControl EQSettings = new EQControl();

    public EqualizerWindow()
    {
      this.GetID = 9004;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_Display_Equalizer.xml");
    }

    private void LoadSettings()
    {
      this.EQSettings = XMLUTILS.LoadEqualizerSettings();
      this.SetUseEqualizer();
      this.SetUseStyle();
      this.SetUseSmoothEQ();
      this.SetDelayStart();
      this.SetShowTitle();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == this.btnUseEqualizer)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnSmothEQ)
      {
        this.SaveSettings();
      }
      if (control == this.btnUseStyle)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnDelayStart)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnDelayStartTime)
      {
        this.SaveSettings();
      }
      if (control == this.btnShowTitle)
      {
        this.SaveSettings();
        this.SetButtons();
      }
      if (control == this.btnShowTitleTime)
      {
        this.SaveSettings();
      }
      if (control == this.btnShowTitleFreq)
      {
        this.SaveSettings();
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
      GUIControl.FocusControl(this.GetID, this.btnUseEqualizer.GetID);
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(109004));
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
      if (btnDelayStartTime != null)
      {
        GUIControl.ClearControl(this.GetID, this.btnDelayStart.GetID);
        for (int i = 0; i < 31; i++)
        {
          GUIControl.AddItemLabelControl(this.GetID, this.btnDelayStartTime.GetID, i.ToString());
        }
        GUIControl.SelectItemControl(this.GetID, this.btnDelayStartTime.GetID, this.EQSettings._DelayEQTime);
      }
    }

    private void SetShowTitle()
    {
      if (btnShowTitleTime != null && btnShowTitleFreq != null)
      {
        GUIControl.ClearControl(this.GetID, this.btnShowTitleTime.GetID);
        GUIControl.ClearControl(this.GetID, this.btnShowTitleFreq.GetID);
        for (int i = 0; i < 31; i++)
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
      this.btnSmothEQ.Selected = this.EQSettings.SmoothEQ;
    }

    private void SetUseStyle()
    {
      if (this.btnUseStyle != null)
      {
        int iItem = this.EQSettings.UseNormalEq
                      ? 0
                      : (this.EQSettings.UseStereoEq ? 1 : (this.EQSettings.UseVUmeter ? 2 : 3));
        GUIControl.ClearControl(this.GetID, this.btnUseStyle.GetID);
        GUIControl.AddItemLabelControl(this.GetID, this.btnUseStyle.GetID, GUILocalizeStrings.Get(9149));
        GUIControl.AddItemLabelControl(this.GetID, this.btnUseStyle.GetID, GUILocalizeStrings.Get(9150));
        GUIControl.AddItemLabelControl(this.GetID, this.btnUseStyle.GetID, GUILocalizeStrings.Get(9151));
        GUIControl.AddItemLabelControl(this.GetID, this.btnUseStyle.GetID, GUILocalizeStrings.Get(9152));
        GUIControl.SelectItemControl(this.GetID, this.btnUseStyle.GetID, iItem);
      }
    }

    private void SetButtons()
    {
      btnUseEqualizer.Visible = true;
      btnUseStyle.Visible = EQSettings.UseEqDisplay;
      btnSmothEQ.Visible = EQSettings.UseEqDisplay;
      btnDelayStart.Visible = EQSettings.UseEqDisplay;
      btnDelayStartTime.Visible = EQSettings.UseEqDisplay && EQSettings.DelayEQ;
      btnShowTitle.Visible = EQSettings.UseEqDisplay;
      btnShowTitleTime.Visible = EQSettings.UseEqDisplay && EQSettings.EQTitleDisplay;
      btnShowTitleFreq.Visible = EQSettings.UseEqDisplay && EQSettings.EQTitleDisplay;
    }
  }
}