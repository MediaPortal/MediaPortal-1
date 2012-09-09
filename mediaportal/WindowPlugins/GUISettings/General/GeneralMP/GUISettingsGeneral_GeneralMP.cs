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
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsMovies.
  /// </summary>
  public class GUISettingsGeneralMP: GUIInternalWindow
  {
    [SkinControl(2)] protected GUIButtonControl btnLog = null;
    [SkinControl(3)] protected GUIButtonControl btnProcess = null;
    [SkinControl(4)] protected GUICheckButton cmWatchdog = null;
    [SkinControl(5)] protected GUICheckButton cmAutoRestart = null;

    private enum Controls
    {
      CONTROL_DELAYINSEC = 6
    } ;

    private enum Priority
    {
      High = 0,
      AboveNormal = 1,
      Normal = 2,
      BelowNormal = 3
    }

    private int _iDelay = 10;
    private string _loglevel = "2"; // 0= Error, 1= warning, 2 = info, 3 = debug
    private string _priority = "Normal";

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

    public GUISettingsGeneralMP()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GENERALMP;//1018
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_General_MP.xml"));
    }

    #region Serialization

    private void LoadSettings()
    {
      using (MPSettings xmlreader = new MPSettings())
      {
        _loglevel = xmlreader.GetValueAsString("general", "loglevel", "2"); // set loglevel to 2:info 3:debug
        _priority = xmlreader.GetValueAsString("general", "ThreadPriority", "Normal");
        cmWatchdog.Selected = xmlreader.GetValueAsBool("general", "watchdogEnabled", false);
        cmAutoRestart.Selected = xmlreader.GetValueAsBool("general", "restartOnError", true);
        
        if (!cmAutoRestart.Selected)
        {
          GUIControl.DisableControl(GetID, (int)Controls.CONTROL_DELAYINSEC);
        }
        else
        {
          GUIControl.EnableControl(GetID, (int)Controls.CONTROL_DELAYINSEC);
        }

        _iDelay = xmlreader.GetValueAsInt("general", "restart delay", 10);
      }
    }

    private void SaveSettings()
    {
      using (MPSettings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("general", "restart delay", _iDelay.ToString());
        xmlwriter.SetValueAsBool("general", "watchdogEnabled", cmWatchdog.Selected);//
        xmlwriter.SetValueAsBool("general", "restartOnError", cmAutoRestart.Selected);//
      }
    }

    #endregion

    #region Overrides

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            LoadSettings();

            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_DELAYINSEC);
            
            for (int i = 1; i <= 100; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_DELAYINSEC, i.ToString());
            }

            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_DELAYINSEC, _iDelay - 1);

            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.CONTROL_DELAYINSEC)
            {
              string strLabel = message.Label;
              _iDelay = Int32.Parse(strLabel);
              SettingsChanged(true);
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101018)); //General - Media Portal

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnLog)
      {
        OnLog();
      }
      if (control == btnProcess)
      {
        OnProcess();
      }
      if (control == cmWatchdog)
      {
        SettingsChanged(true);
      }
      if (control == cmAutoRestart)
      {
        if (!cmAutoRestart.Selected)
        {
          GUIControl.DisableControl(GetID, (int)Controls.CONTROL_DELAYINSEC);
        }
        else
        {
          GUIControl.EnableControl(GetID, (int)Controls.CONTROL_DELAYINSEC);
        }
        SettingsChanged(true);
      }
      
      base.OnClicked(controlId, control, actionType);
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

    private int GetPriority(string priority)
    {
      switch (priority)
      {
        case "High":
          return (int)Priority.High;

        case "AboveNormal":
          return (int)Priority.AboveNormal; ;

        case "Normal":
          return (int)Priority.Normal; ;

        case "BelowNormal":
          return (int)Priority.BelowNormal; ;

        default:
          return (int)Priority.Normal; ;
      }
    }

    private void OnLog()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // Options

      dlg.Add("Error");
      dlg.Add("Warning");
      dlg.Add("Information");
      dlg.Add("Debug");

      dlg.SelectedLabel = Convert.ToInt16(_loglevel);

      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      using (MPSettings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("general", "loglevel", dlg.SelectedLabel);
        SettingsChanged(true);
      }
      _loglevel = dlg.SelectedLabel.ToString();
    }

    private void OnProcess()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // Options

      dlg.Add("High");
      dlg.Add("AboveNormal");
      dlg.Add("Normal");
      dlg.Add("BelowNormal");

      dlg.SelectedLabel = GetPriority(_priority);

      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      using (MPSettings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("general", "ThreadPriority", dlg.SelectedLabelText);
        SettingsChanged(true);
      }

      _priority = dlg.SelectedLabelText;
    }

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
    
  }
}