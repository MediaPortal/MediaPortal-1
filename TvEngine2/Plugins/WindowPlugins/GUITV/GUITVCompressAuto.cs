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
using System.Collections.Generic;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// Summary description for GUITVCompressAuto.
  /// </summary>
  public class GUITVCompressAuto : GUIWindow
  {
    [SkinControl(5)] protected GUISpinControl spinHour = null;
    [SkinControl(2)] protected GUICheckMarkControl checkAutoCompress = null;
    [SkinControl(7)] protected GUICheckMarkControl checkDeleteOriginal = null;

    public GUITVCompressAuto()
    {
      GetID = (int) Window.WINDOW_TV_COMPRESS_AUTO;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\mytvcompressauto_TVE2.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadSettings();
    }

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        spinHour.Value = xmlreader.GetValueAsInt("autocompression", "hour", 4);
        checkDeleteOriginal.Selected = xmlreader.GetValueAsBool("autocompression", "deleteoriginal", true);
        checkAutoCompress.Selected = xmlreader.GetValueAsBool("autocompression", "enabled", true);
      }
      UpdateButtons();
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      UpdateButtons();
    }

    private void UpdateButtons()
    {
      spinHour.Disabled = !checkAutoCompress.Selected;
      checkDeleteOriginal.Disabled = !checkAutoCompress.Selected;
    }

    private void AutoCompress()
    {
      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      foreach (TVRecorded rec in recordings)
      {
        if (Transcoder.IsTranscoding(rec))
        {
          continue; //already transcoding...
        }
        try
        {
          if (!File.Exists(rec.FileName))
          {
            continue;
          }
          string ext = Path.GetExtension(rec.FileName).ToLower();
          if (ext != ".dvr-ms" && ext != ".sbe")
          {
            continue;
          }
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