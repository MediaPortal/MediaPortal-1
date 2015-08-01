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

using Mediaportal.TV.Server.TVControl.ServiceAgents;
using MediaPortal.GUI.Library;
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

      spinPreRecord.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("preRecordInterval", 7);
      spinPostRecord.Value = ServiceAgents.Instance.SettingServiceAgent.GetValue("postRecordInterval", 10);

      cbAutoDeleteRecordings.Selected = false;  // obsolete setting, removed
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == spinPreRecord)
      {
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("preRecordInterval", spinPreRecord.Value);
      }
      else if (control == spinPostRecord)
      {
        ServiceAgents.Instance.SettingServiceAgent.SaveValue("postRecordInterval", spinPostRecord.Value);
      }
      base.OnClicked(controlId, control, actionType);
    }

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
    }
  }
}