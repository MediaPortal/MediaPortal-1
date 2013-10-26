#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsGeneral.
  /// </summary>
  public class GUISettingsGeneralVolume : GUIInternalWindow
  {
    [SkinControl(2)] protected GUICheckButton btnMasterVolume = null;
    [SkinControl(3)] protected GUICheckButton btnWave= null;
    [SkinControl(4)] protected GUICheckButton btnWinXP = null;
    [SkinControl(5)] protected GUICheckButton btnClassic= null;
    [SkinControl(6)] protected GUICheckButton btnLogarithmic = null;
    [SkinControl(7)] protected GUICheckButton btnVistaWin7 = null;
    [SkinControl(8)] protected GUICheckButton btnCustom = null;
    [SkinControl(9)] protected GUICheckButton btnEnableOSDVolume = null;
    
    private bool _settingsSaved;
    private string _customVolume = string.Empty;
    private bool _useMixing = false;

    private class CultureComparer : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        CultureInfo info1 = (CultureInfo)x;
        CultureInfo info2 = (CultureInfo)y;
        return String.Compare(info1.EnglishName, info2.EnglishName, true);
      }

      #endregion
    }

    public GUISettingsGeneralVolume()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GENERALVOLUME; //1007
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_General_Volume.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        int volumeStyle = xmlreader.GetValueAsInt("volume", "handler", 1);
        bool isDigital = xmlreader.GetValueAsBool("volume", "digital", true);
        btnClassic.Selected = volumeStyle == 0;
        btnWinXP.Selected = volumeStyle == 1;
        btnLogarithmic.Selected= volumeStyle == 2;
        btnCustom.Selected = volumeStyle == 3;
        btnVistaWin7.Selected = volumeStyle == 4;
        _customVolume = xmlreader.GetValueAsString("volume", "table",
                                              "0, 4095, 8191, 12287, 16383, 20479, 24575, 28671, 32767, 36863, 40959, 45055, 49151, 53247, 57343, 61439, 65535");

        // When Upmixing has selected, we need to use Wave Volume
        _useMixing = xmlreader.GetValueAsBool("audioplayer", "mixing", false);
        
        if (_useMixing)
        {
          isDigital = true;
        }

        btnMasterVolume.Selected = !isDigital;
        btnWave.Selected = isDigital;

        btnEnableOSDVolume.Selected = xmlreader.GetValueAsBool("volume", "defaultVolumeOSD", true);
      }
    }

    private void SaveSettings()
    {
      if (!_settingsSaved)
      {
        _settingsSaved = true;
        using (Settings xmlwriter = new MPSettings())
        {
          if (btnClassic.Selected)
          {
            xmlwriter.SetValue("volume", "handler", 0);
          }
          else if (btnWinXP.Selected)
          {
            xmlwriter.SetValue("volume", "handler", 1);
          }
          else if (btnLogarithmic.Selected)
          {
            xmlwriter.SetValue("volume", "handler", 2);
          }
          else if (btnCustom.Selected)
          {
            xmlwriter.SetValue("volume", "handler", 3);
          }
          else if (btnVistaWin7.Selected)
          {
            xmlwriter.SetValue("volume", "handler", 4);
          }

          bool useDigital = btnWave.Selected;
          if (_useMixing)
          {
            useDigital = true;
          }
          xmlwriter.SetValueAsBool("volume", "digital", useDigital);
          xmlwriter.SetValue("volume", "table", _customVolume);
          xmlwriter.SetValueAsBool("volume", "defaultVolumeOSD", btnEnableOSDVolume.Selected);
        }
      }
    }

    #endregion

    #region Overrides
    
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      // Control
      if (control == btnMasterVolume)
      {
        if (btnMasterVolume.Selected)
        {
          btnWave.Selected = false;
        }
        else
        {
          btnWave.Selected = true;
        }
        SettingsChanged(true);
      }
      if (control == btnWave)
      {
        if (btnWave.Selected)
        {
          btnMasterVolume.Selected = false;
        }
        else
        {
          btnMasterVolume.Selected = true;
        }
        SettingsChanged(true);
      }
      //Scale
      if (control == btnWinXP)
      {
        if (btnWinXP.Selected)
        {
          btnClassic.Selected = false;
          btnLogarithmic.Selected = false;
          btnVistaWin7.Selected = false;
          btnCustom.Selected = false;
          _customVolume = string.Empty;
          SetProperties();
          SettingsChanged(true);
        }
      }
      if (control == btnClassic)
      {
        if (btnClassic.Selected)
        {
          btnWinXP.Selected = false;
          btnLogarithmic.Selected = false;
          btnVistaWin7.Selected = false;
          btnCustom.Selected = false;
          _customVolume = string.Empty;
          SetProperties();
          SettingsChanged(true);
        }
      }
      if (control == btnLogarithmic)
      {
        if (btnLogarithmic.Selected)
        {
          btnWinXP.Selected = false;
          btnClassic.Selected = false;
          btnVistaWin7.Selected = false;
          btnCustom.Selected = false;
          _customVolume = string.Empty;
          SetProperties();
          SettingsChanged(true);
        }
      }
      if (control == btnVistaWin7)
      {
        if (btnVistaWin7.Selected)
        {
          btnWinXP.Selected = false;
          btnClassic.Selected = false;
          btnLogarithmic.Selected = false;
          btnCustom.Selected = false;
          _customVolume = string.Empty;
          SetProperties();
          SettingsChanged(true);
        }
      }
      if (control == btnCustom)
      {
        if (btnCustom.Selected)
        {
          btnWinXP.Selected = false;
          btnClassic.Selected = false;
          btnLogarithmic.Selected = false;
          btnVistaWin7.Selected = false;
          
          string volumeTable = _customVolume;
          GetStringFromKeyboard(ref volumeTable);
          ValidateCustomTable(ref volumeTable);

          if (!string.IsNullOrEmpty(volumeTable))
          {
            _customVolume = volumeTable;
          }

          SetProperties();
          SettingsChanged(true);
        }
      }
      
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101007)); //General - Volume
      LoadSettings();
      SetProperties();
      _settingsSaved = false;

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      if (MediaPortal.GUI.Settings.GUISettings.SettingsChanged && !MediaPortal.Util.Utils.IsGUISettingsWindow(newWindowId))
      {
        MediaPortal.GUI.Settings.GUISettings.OnRestartMP(GetID);
      }

      base.OnPageDestroy(newWindowId);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    #endregion

    private void SetProperties()
    {
      GUIPropertyManager.SetProperty("#customScalevalues", _customVolume);
    }

    private void EnableMixerButtons(bool enable)
    {
      if (enable)
      {
        btnMasterVolume.IsEnabled = true;
        btnWave.IsEnabled = true;
      }
      else
      {
        btnMasterVolume.IsEnabled = false;
        btnWave.IsEnabled = false;
      }
    }

    private void EnableScaleButtons(bool enable)
    {
      if (enable)
      {
        btnWinXP.IsEnabled = true;
        btnClassic.IsEnabled = true;
        btnLogarithmic.IsEnabled = true;
        btnVistaWin7.IsEnabled = true;
        btnCustom.IsEnabled = true;
      }
      else
      {
        btnWinXP.IsEnabled = false;
        btnClassic.IsEnabled = false;
        btnLogarithmic.IsEnabled = false;
        btnVistaWin7.IsEnabled = false;
        btnCustom.IsEnabled = false;
      }
    }

    private void GetStringFromKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return;
      }
      keyboard.Reset();
      keyboard.Text = strLine;

      keyboard.DoModal(GUIWindowManager.ActiveWindow);
      
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }

    private void ValidateCustomTable(ref string customTable)
    {
      try
      {
        StringBuilder builder = new StringBuilder();
        ArrayList valueArray = new ArrayList();

        foreach (string token in (customTable).Split(new char[] { ',', ';', ' ' }))
        {
          if (token == string.Empty)
          {
            continue;
          }

          // for now we're happy so long as the token can be converted to integer
          valueArray.Add(Math.Max(0, Math.Min(65535, Convert.ToInt32(token))));
        }

        valueArray.Sort();

        // rebuild a fully formatted string to represent the volume table
        foreach (int volume in valueArray)
        {
          if (builder.Length != 0)
          {
            builder.Append(", ");
          }

          builder.Append(volume.ToString());
        }

        if (valueArray.Count < 2)
        {
          customTable = string.Empty;
          return ;
        }

        customTable = builder.ToString();
      }
      catch (Exception)
      {
        customTable = string.Empty;
      }
    }
    
    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
   }
}