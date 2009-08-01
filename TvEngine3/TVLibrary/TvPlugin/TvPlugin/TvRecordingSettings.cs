#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using MediaPortal.GUI.Library;
using TvDatabase;
//using MediaPortal.Utils.Services;

namespace TvPlugin
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
      GetID = (int) Window.WINDOW_SETTINGS_RECORDINGS;
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
      TvBusinessLayer layer = new TvBusinessLayer();

      spinPreRecord.Value = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
      spinPostRecord.Value = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);

      cbAutoDeleteRecordings.Selected = (layer.GetSetting("autodeletewatchedrecordings", "no").Value == "yes");
      cbCreateTagInfoXML.Selected = (layer.GetSetting("createtaginfoxml", "yes").Value == "yes");
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
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("autodeletewatchedrecordings", "no");
      setting.Value = cbAutoDeleteRecordings.Selected ? "yes" : "no";
      setting.Persist();
    }

    private void OnCreateTagInfoXML()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("createtaginfoxml", "yes");
      setting.Value = cbCreateTagInfoXML.Selected ? "yes" : "no";
      setting.Persist();
    }

    private void OnPreRecord()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("preRecordInterval", "5");
      setting.Value = spinPreRecord.Value.ToString();
      setting.Persist();
    }

    private void OnPostRecord()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("postRecordInterval", "5");
      setting.Value = spinPostRecord.Value.ToString();
      setting.Persist();
    }

    public override void Process()
    {
      TVHome.UpdateProgressPercentageBar();
    }
  }
}