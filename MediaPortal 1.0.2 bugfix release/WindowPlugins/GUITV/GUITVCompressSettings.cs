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
using MediaPortal.TV.Recording;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// Summary description for GUITVCompressSettings.
  /// </summary>
  public class GUITVCompressSettings : GUIWindow
  {
    [SkinControl(3)] protected GUISpinControl spinType = null;
    [SkinControl(5)] protected GUISpinControl spinQuality = null;
    [SkinControl(7)] protected GUISpinControl spinScreenSize = null;
    [SkinControl(9)] protected GUISpinControl spinFPS = null;
    [SkinControl(11)] protected GUISpinControl spinBitrate = null;
    [SkinControl(13)] protected GUISpinControl spinPriority = null;
    [SkinControl(15)] protected GUICheckMarkControl checkDeleteOriginal = null;
    [SkinControl(17)] protected GUISpinControl spinStandard = null;

    public GUITVCompressSettings()
    {
      GetID = (int) Window.WINDOW_TV_COMPRESS_SETTINGS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\mytvcompresssettings.xml");
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
        spinType.Value = xmlreader.GetValueAsInt("compression", "type", 0);
        spinQuality.Value = xmlreader.GetValueAsInt("compression", "quality", 3);
        spinStandard.Value = xmlreader.GetValueAsInt("compression", "standard", 2);
        spinBitrate.Value = xmlreader.GetValueAsInt("compression", "bitrate", 4);
        spinFPS.Value = xmlreader.GetValueAsInt("compression", "fps", 1);
        spinScreenSize.Value = xmlreader.GetValueAsInt("compression", "screensize", 1);
        spinPriority.Value = xmlreader.GetValueAsInt("compression", "priority", 0);
        checkDeleteOriginal.Selected = xmlreader.GetValueAsBool("compression", "deleteoriginal", false);
      }
      UpdateButtons();
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlreader.SetValue("compression", "type", spinType.Value);
        xmlreader.SetValue("compression", "quality", spinQuality.Value);
        xmlreader.SetValue("compression", "standard", spinStandard.Value);
        xmlreader.SetValue("compression", "bitrate", spinBitrate.Value);
        xmlreader.SetValue("compression", "fps", spinFPS.Value);
        xmlreader.SetValue("compression", "screensize", spinScreenSize.Value);
        xmlreader.SetValue("compression", "priority", spinPriority.Value);
        xmlreader.SetValueAsBool("compression", "deleteoriginal", checkDeleteOriginal.Selected);
      }
    }

    public override void OnAction(Action action)
    {
      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == spinType)
      {
        UpdateButtons();
      }
      if (control == spinQuality)
      {
        UpdateProfiles();
      }
      if (control == spinStandard)
      {
        UpdateProfiles();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void UpdateButtons()
    {
      bool isMpeg2 = (spinType.Value == 0);
      bool isWMV = (spinType.Value == 1);
      //re-enable this when MP4 is finished.
      //bool isMP4 = (spinType.Value == 2);
      spinBitrate.Disabled = (isMpeg2);
      spinFPS.Disabled = (isMpeg2);
      spinQuality.Disabled = (isMpeg2);
      spinScreenSize.Disabled = (isMpeg2);
      spinStandard.Disabled = (isMpeg2);
      if (isWMV)
      {
        bool isCustom = (spinQuality.Value == 6);
        spinBitrate.Disabled = !isCustom;
        spinFPS.Disabled = !isCustom;
        spinScreenSize.Disabled = !isCustom;
      }
      //re-enable this when MP4 & custom settings are done
      /*
      if (isMP4)
      {
        bool isCustom = (spinQuality.Value == 6);
        spinBitrate.Disabled = !isCustom;
        spinFPS.Disabled = !isCustom;
        spinScreenSize.Disabled = !isCustom;
      }*/
    }

    private void UpdateProfiles()
    {
      switch (spinQuality.Value)
      {
        case 0: //Portable
          spinBitrate.Value = 0; //100kbs
          if (spinStandard.Value == 0)
          {
            spinFPS.Value = 0; //12.5fps (Forced to PAL)
            spinScreenSize.Value = 0; //240x180 (NTSC)
          }
          if (spinStandard.Value == 1)
          {
            spinFPS.Value = 1; //15fps (NTSC)
            spinScreenSize.Value = 0; //240x180 (NTSC)
          }
          if (spinStandard.Value == 2)
          {
            spinFPS.Value = 0; //12.5fps (PAL)
            spinScreenSize.Value = 1; //288x216 (PAL)
          }
          break;
        case 1: //Low
          if (spinStandard.Value == 0)
          {
            spinFPS.Value = 2; //23.97fps (Film)
            spinScreenSize.Value = 2; //352x240 (NTSC)
          }
          if (spinStandard.Value == 1)
          {
            spinFPS.Value = 4; //29.97fps (NTSC)
            spinScreenSize.Value = 2; //352x240 (NTSC)
          }
          if (spinStandard.Value == 2)
          {
            spinFPS.Value = 3; //25fps (PAL)
            spinScreenSize.Value = 3; //352x288 (PAL)
          }
          spinBitrate.Value = 1; //256kbs
          break;
        case 2: //Medium
          spinBitrate.Value = 2; //384kbs
          spinScreenSize.Value = 4; //640x480
          if (spinStandard.Value == 0)
          {
            spinFPS.Value = 2; //23.97fps (Film)
          }
          if (spinStandard.Value == 1)
          {
            spinFPS.Value = 4; //29.97fps (NTSC)
          }
          if (spinStandard.Value == 2)
          {
            spinFPS.Value = 3; //25fps (PAL)
          }
          break;
        case 3: //High
          spinBitrate.Value = 3; //768kbs
          if (spinStandard.Value == 0)
          {
            spinFPS.Value = 2; //23.97fps (Film)
            spinScreenSize.Value = 5; //720x480 (NTSC)
          }
          if (spinStandard.Value == 1)
          {
            spinFPS.Value = 4; //29.97fps (NTSC)
            spinScreenSize.Value = 5; //720x480 (NTSC)
          }
          if (spinStandard.Value == 2)
          {
            spinFPS.Value = 3; //25fps (PAL)
            spinScreenSize.Value = 6; //720x576 (PAL)
          }
          break;
        case 4: //Very High
          spinBitrate.Value = 4; //1536kbs
          if (spinStandard.Value == 0)
          {
            spinFPS.Value = 2; //23.97fps (Film)
            spinScreenSize.Value = 5; //720x480 (NTSC)
          }
          if (spinStandard.Value == 1)
          {
            spinFPS.Value = 4; //29.97fps (NTSC)
            spinScreenSize.Value = 5; //720x480 (NTSC)
          }
          if (spinStandard.Value == 2)
          {
            spinFPS.Value = 3; //25fps (PAL)
            spinScreenSize.Value = 6; //720x576 (PAL)
          }
          break;
        case 5: //HiDef
          spinBitrate.Value = 5; //3072kbs
          if (spinStandard.Value == 0)
          {
            spinFPS.Value = 2; //23.97fps (Film)
          }
          if (spinStandard.Value == 1)
          {
            spinFPS.Value = 4; //29.97fps (NTSC)
          }
          if (spinStandard.Value == 2)
          {
            spinFPS.Value = 3; //25fps (PAL)
          }
          spinScreenSize.Value = 7; //1280x720
          break;
      }
      UpdateButtons();
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
    }
  }
}