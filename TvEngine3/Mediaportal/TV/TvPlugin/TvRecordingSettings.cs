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
    [SkinControl(5)] protected GUICheckMarkControl cbCreateTagInfoXML = null;
    [SkinControl(27)] protected GUISpinControl spinPreRecord = null;
    [SkinControl(30)] protected GUISpinControl spinPostRecord = null;

    public TvRecordingSettings()
    {
      GetID = (int)Window.WINDOW_SETTINGS_RECORDINGS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_recording.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      spinPreRecord.SetRange(0, 30);
      spinPostRecord.SetRange(0, 30);

      spinPreRecord.Value = Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("preRecordInterval", "5").Value);
      spinPostRecord.Value = Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("postRecordInterval", "5").Value);

      cbAutoDeleteRecordings.Selected = (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("autodeletewatchedrecordings", "no").Value == "yes");
      cbCreateTagInfoXML.Selected = (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("createtaginfoxml", "yes").Value == "yes");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == cbAutoDeleteRecordings)
      {
        OnAutoDeleteRecordings();
      }
      if (control == cbCreateTagInfoXML)
      {
        OnCreateTagInfoXML();
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
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("autodeletewatchedrecordings", cbAutoDeleteRecordings.Selected ? "yes" : "no");
    }

    private void OnCreateTagInfoXML()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("createtaginfoxml", cbCreateTagInfoXML.Selected ? "yes" : "no");
    }

    private void OnPreRecord()
    {
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("preRecordInterval", spinPreRecord.Value.ToString());      
    }

    private void OnPostRecord()
    {
      ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("postRecordInterval", spinPostRecord.Value.ToString());
    }

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
    }
  }
}