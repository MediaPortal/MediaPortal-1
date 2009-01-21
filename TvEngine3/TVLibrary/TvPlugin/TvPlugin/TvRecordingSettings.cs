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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using AMS.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
//using MediaPortal.Utils.Services;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;


using Gentle.Common;
using Gentle.Framework;
namespace TvPlugin
{
  /// <summary>
  /// Summary description for GUISettingsRecordings.
  /// </summary>
  public class TvRecordingSettings : GUIWindow
  {
    [SkinControlAttribute(4)]    protected GUICheckMarkControl cbAutoDeleteRecordings = null;
    [SkinControlAttribute(5)]    protected GUICheckMarkControl cbCreateTagInfoXML = null;
    [SkinControlAttribute(27)]   protected GUISpinControl spinPreRecord = null;
    [SkinControlAttribute(30)]   protected GUISpinControl spinPostRecord = null;

    public TvRecordingSettings()
    {
      GetID = (int)GUIWindow.Window.WINDOW_SETTINGS_RECORDINGS;
    }

    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_SETTINGS_RECORDINGS, this);
      Restore();
      PreInit();
      ResetAllControls();
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

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == cbAutoDeleteRecordings) OnAutoDeleteRecordings();
      if (control == cbCreateTagInfoXML) OnCreateTagInfoXML();
      if (control == spinPreRecord) OnPreRecord();
      if (control == spinPostRecord) OnPostRecord();
      base.OnClicked(controlId, control, actionType);
    }

    void OnAutoDeleteRecordings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("autodeletewatchedrecordings", "no");
      setting.Value = cbAutoDeleteRecordings.Selected ? "yes" : "no";
      setting.Persist();
    }

    void OnCreateTagInfoXML()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("createtaginfoxml", "yes");
      setting.Value = cbCreateTagInfoXML.Selected ? "yes" : "no";
      setting.Persist();

    }

    void OnPreRecord()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("preRecordInterval", "5");
      setting.Value = spinPreRecord.Value.ToString();
      setting.Persist();
    }

    void OnPostRecord()
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
