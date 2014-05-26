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
using MediaPortal.GUI.Library;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Action = MediaPortal.GUI.Library.Action;

//using MediaPortal.Utils.Services;

namespace Mediaportal.TV.TvPlugin
{
  /// <summary>
  /// Summary description for GUISettingsRecordings.
  /// </summary>
  public class TvRecordingSettings : GUIInternalWindow
  {
    [SkinControl(4)] protected GUICheckMarkControl cbAutoDeleteRecordings = null;
    [SkinControl(27)] protected GUISpinControl spinPreRecord = null;
    [SkinControl(30)] protected GUISpinControl spinPostRecord = null;

    public TvRecordingSettings()
    {
      GetID = (int)Window.WINDOW_SETTINGS_RECORDINGS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_recording.xml"));
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      spinPreRecord.SetRange(0, 30);
      spinPostRecord.SetRange(0, 30);

      spinPreRecord.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("preRecordInterval", 5);
      spinPostRecord.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("postRecordInterval", 5);

      cbAutoDeleteRecordings.Selected = ServiceAgents.Instance.SettingServiceAgent.GetValue("autodeletewatchedrecordings", false);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == cbAutoDeleteRecordings)
      {
        OnAutoDeleteRecordings();
      }
      if (control == spinPreRecord)
      {
        OnPreRecord();
      }
      if (control == spinPostRecord)
      {
        OnPostRecord();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnAutoDeleteRecordings()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("autodeletewatchedrecordings", cbAutoDeleteRecordings.Selected);
    }

    private void OnPreRecord()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("preRecordInterval", spinPreRecord.Value);
    }

    private void OnPostRecord()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveValue("postRecordInterval", spinPostRecord.Value);
    }

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
    }
  }
}