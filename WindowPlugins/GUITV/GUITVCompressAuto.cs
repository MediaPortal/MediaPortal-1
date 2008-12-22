#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// Summary description for GUITVCompressAuto.
  /// </summary>
  public class GUITVCompressAuto : GUIWindow
  {
    [SkinControlAttribute(5)]
    protected GUISpinControl spinHour = null;
    [SkinControlAttribute(2)]
    protected GUICheckMarkControl checkAutoCompress = null;
    [SkinControlAttribute(7)]
    protected GUICheckMarkControl checkDeleteOriginal = null;

    public GUITVCompressAuto()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TV_COMPRESS_AUTO;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\mytvcompressauto.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadSettings();
    }
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        spinHour.Value = xmlreader.GetValueAsInt("autocompression", "hour", 4);
        checkDeleteOriginal.Selected = xmlreader.GetValueAsBool("autocompression", "deleteoriginal", true);
        checkAutoCompress.Selected = xmlreader.GetValueAsBool("autocompression", "enabled", true);
      }
      UpdateButtons();
    }

    void SaveSettings()
    {

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("autocompression", "hour", spinHour.Value);
        xmlreader.SetValueAsBool("autocompression", "deleteoriginal", checkDeleteOriginal.Selected);
        xmlreader.SetValueAsBool("autocompression", "enabled", checkAutoCompress.Selected);
      }
    }

    public override void OnAction(Action action)
    {
      //switch (action.wID)
      //{
      //}
      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      UpdateButtons();
    }
    void UpdateButtons()
    {
      spinHour.Disabled = !checkAutoCompress.Selected;
      checkDeleteOriginal.Disabled = !checkAutoCompress.Selected;
    }

    void AutoCompress()
    {
      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      foreach (TVRecorded rec in recordings)
      {
        if (Transcoder.IsTranscoding(rec)) continue; //already transcoding...
        try
        {
          if (!System.IO.File.Exists(rec.FileName)) continue;
          string ext = System.IO.Path.GetExtension(rec.FileName).ToLower();
          if (ext != ".dvr-ms" && ext != ".sbe") continue;
        }
        catch (Exception)
        {
          continue;
        }
        Transcoder.Transcode(rec, false);
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            Recorder.StopViewing();
          }
        }
      }
      base.OnPageDestroy(newWindowId);
      SaveSettings();
      if (checkAutoCompress.Selected)
      {
        AutoCompress();
      }
    }
  }
}
