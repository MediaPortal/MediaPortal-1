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

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace WindowPlugins.GUISettings.TV
{
  /// <summary>
  /// Summary description for GUISettingsRecordings.
  /// </summary>
  public class GUISettingsRecordings : GUIWindow
  {
    [SkinControl(4)] protected GUICheckMarkControl cbAutoDeleteRecordings = null;
    [SkinControl(5)] protected GUICheckMarkControl cbAddRecordingsToDbs = null;
    [SkinControl(27)] protected GUISpinControl spinPreRecord = null;
    [SkinControl(30)] protected GUISpinControl spinPostRecord = null;

    public GUISettingsRecordings()
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
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        spinPreRecord.Value = (xmlreader.GetValueAsInt("capture", "prerecord", 5));
        spinPostRecord.Value = (xmlreader.GetValueAsInt("capture", "postrecord", 5));
        cbAutoDeleteRecordings.Selected = xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
        cbAddRecordingsToDbs.Selected = xmlreader.GetValueAsBool("capture", "addrecordingstomoviedatabase", true);
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == cbAutoDeleteRecordings)
      {
        OnAutoDeleteRecordings();
      }
      if (control == cbAddRecordingsToDbs)
      {
        OnAddRecordingsToMovieDatabase();
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
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("capture", "deletewatchedshows", cbAutoDeleteRecordings.Selected);
      }
    }

    private void OnAddRecordingsToMovieDatabase()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("capture", "addrecordingstomoviedatabase", cbAddRecordingsToDbs.Selected);
      }
    }

    private void OnPreRecord()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("capture", "prerecord", spinPreRecord.Value.ToString());
      }
    }

    private void OnPostRecord()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("capture", "postrecord", spinPostRecord.Value.ToString());
      }
    }
  }
}