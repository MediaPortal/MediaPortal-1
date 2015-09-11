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

using System;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Enum;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;

namespace Mediaportal.TV.TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvPriorities : GUIInternalWindow
  {
    public static void OnSetEpisodesToKeep(Schedule rec)
    {
      Schedule schedule = ServiceAgents.Instance.ScheduleServiceAgent.GetSchedule(rec.IdSchedule);
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(887); //quality settings
      dlg.ShowQuickNumbers = false;
      dlg.AddLocalizedString(889); //All episodes
      for (int i = 1; i < 40; ++i)
      {
        dlg.Add(i.ToString() + " " + GUILocalizeStrings.Get(874));
      }
      if (schedule.MaxAirings == Int32.MaxValue)
      {
        dlg.SelectedLabel = 0;
      }
      else
      {
        dlg.SelectedLabel = schedule.MaxAirings;
      }

      dlg.DoModal(GUIWindowManager.ActiveWindow);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }

      if (dlg.SelectedLabel == 0)
      {
        schedule.MaxAirings = Int32.MaxValue;
      }
      else
      {
        schedule.MaxAirings = dlg.SelectedLabel;
      }
      ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(schedule);
      ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
    }

    public static void OnSetQuality(Schedule rec)
    {
      ScheduleBLL recBLL = new ScheduleBLL(rec);

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(882);

        dlg.ShowQuickNumbers = true;
        dlg.AddLocalizedString(968);
        dlg.AddLocalizedString(965);
        dlg.AddLocalizedString(966);
        dlg.AddLocalizedString(967);

        switch (recBLL.BitRateMode)
        {
          case EncoderBitRateMode.NotSet:
            dlg.SelectedLabel = 0;
            break;
          case EncoderBitRateMode.ConstantBitRate:
            dlg.SelectedLabel = 1;
            break;
          case EncoderBitRateMode.VariableBitRateAverage:
            dlg.SelectedLabel = 2;
            break;
          case EncoderBitRateMode.VariableBitRatePeak:
            dlg.SelectedLabel = 3;
            break;
        }

        dlg.DoModal(GUIWindowManager.ActiveWindow);

        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedLabel)
        {
          case 0: // Not Set
            recBLL.BitRateMode = EncoderBitRateMode.NotSet;
            break;

          case 1: // CBR
            recBLL.BitRateMode = EncoderBitRateMode.ConstantBitRate;
            break;

          case 2: // VBR
            recBLL.BitRateMode = EncoderBitRateMode.VariableBitRateAverage;
            break;

          case 3: // VBR Peak
            recBLL.BitRateMode = EncoderBitRateMode.VariableBitRatePeak;
            break;
        }

        ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(recBLL.Entity);

        dlg.Reset();
        dlg.SetHeading(882);

        dlg.ShowQuickNumbers = true;
        dlg.AddLocalizedString(968);
        dlg.AddLocalizedString(886); //Default
        dlg.AddLocalizedString(993); // Custom
        dlg.AddLocalizedString(893); //Portable
        dlg.AddLocalizedString(883); //Low
        dlg.AddLocalizedString(884); //Medium
        dlg.AddLocalizedString(885); //High

        switch (recBLL.QualityType)
        {
          case QualityType.NotSet:
            dlg.SelectedLabel = 0;
            break;
          case QualityType.Default:
            dlg.SelectedLabel = 1;
            break;
          case QualityType.Custom:
            dlg.SelectedLabel = 2;
            break;
          case QualityType.Portable:
            dlg.SelectedLabel = 3;
            break;
          case QualityType.Low:
            dlg.SelectedLabel = 4;
            break;
          case QualityType.Medium:
            dlg.SelectedLabel = 5;
            break;
          case QualityType.High:
            dlg.SelectedLabel = 6;
            break;
        }

        dlg.DoModal(GUIWindowManager.ActiveWindow);

        if (dlg.SelectedLabel == -1)
        {
          return;
        }
        switch (dlg.SelectedLabel)
        {
          case 0: // Not Set
            recBLL.QualityType = QualityType.NotSet;
            break;

          case 1: // Default
            recBLL.QualityType = QualityType.Default;
            break;

          case 2: // Custom
            recBLL.QualityType = QualityType.Custom;
            break;

          case 3: // Protable
            recBLL.QualityType = QualityType.Portable;
            break;

          case 4: // Low
            recBLL.QualityType = QualityType.Low;
            break;

          case 5: // Medium
            recBLL.QualityType = QualityType.Medium;
            break;

          case 6: // High
            recBLL.QualityType = QualityType.High;
            break;
        }

        ServiceAgents.Instance.ScheduleServiceAgent.SaveSchedule(recBLL.Entity);
      }
      
      ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
    }
  }
}