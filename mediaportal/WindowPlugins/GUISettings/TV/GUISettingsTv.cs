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

using System.Collections;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace WindowPlugins.GUISettings.TV
{
  /// <summary>
  /// Summary description for GUISettingsTv.
  /// </summary>
  public class GUISettingsTv : GUIInternalWindow
  {
    [SkinControl(24)] protected GUIButtonControl btnVideoCodec = null;
    [SkinControl(25)] protected GUIButtonControl btnAudioCodec = null;
    [SkinControl(27)] protected GUIButtonControl btnDeinterlace = null;
    [SkinControl(28)] protected GUIButtonControl btnAspectRatio = null;
    [SkinControl(30)] protected GUIButtonControl btnAutoTurnOnTv = null;
    [SkinControl(33)] protected GUIButtonControl btnAudioRenderer = null;
    [SkinControl(35)] protected GUIButtonControl btnH264VideoCodec = null;
    [SkinControl(36)] protected GUIButtonControl btnAACAudioCodec = null;

    public readonly string Windows7Codec = "Microsoft DTV-DVD Video Decoder";

    public GUISettingsTv()
    {
      GetID = (int)Window.WINDOW_SETTINGS_TV;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_tv.xml");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnAudioRenderer)
      {
        OnAudioRenderer();
      }
      if (control == btnVideoCodec)
      {
        OnVideoCodec();
      }
      if (control == btnAudioCodec)
      {
        OnAudioCodec();
      }
      if (control == btnAspectRatio)
      {
        OnAspectRatio();
      }
      if (control == btnDeinterlace)
      {
        OnDeinterlace();
      }
      if (control == btnAutoTurnOnTv)
      {
        OnAutoTurnOnTv();
      }
      if (control == btnH264VideoCodec)
      {
        OnH264VideoCodec();
      }
      if (control == btnAACAudioCodec)
      {
        OnAACAudioCodec();
      }
      base.OnClicked(controlId, control, actionType);
    }

    ArrayList MPEG2list()
    {
      ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
      //Remove Muxer's from the list to avoid confusion.
      while (availableVideoFilters.Contains("CyberLink MPEG Muxer"))
      {
        availableVideoFilters.Remove("CyberLink MPEG Muxer");
      }
      while (availableVideoFilters.Contains("Ulead MPEG Muxer"))
      {
        availableVideoFilters.Remove("Ulead MPEG Muxer");
      }
      while (availableVideoFilters.Contains("PDR MPEG Muxer"))
      {
        availableVideoFilters.Remove("PDR MPEG Muxer");
      }
      while (availableVideoFilters.Contains("Nero Mpeg2 Encoder"))
      {
        availableVideoFilters.Remove("Nero Mpeg2 Encoder");
      }
      availableVideoFilters.Sort();

      return availableVideoFilters;
    }

    ArrayList H264list()
    {
      return FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
    }

    private void OnVideoCodec()
    {
      string strVideoCodec = "";
      using (Settings xmlreader = new MPSettings())
      {
        strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
      }
      ArrayList availableVideoFilters = MPEG2list();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableVideoFilters)
        {
          dlg.Add(codec); //delete
          if (codec == strVideoCodec)
          {
            selected = count;
          }
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
      {
        return;
      }
      string selectedCodec = (string)availableVideoFilters[dlg.SelectedLabel];
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("mytv", "videocodec", selectedCodec);

        if (selectedCodec.Contains(Windows7Codec))
        {
          xmlwriter.SetValue("mytv", "h264videocodec", selectedCodec);
        }
        else
        {
          string h264Codec = xmlwriter.GetValue("mytv", "h264videocodec");
          if (h264Codec.Contains(Windows7Codec))
          {
            for (int i = 0; i < H264list().Count; i++)
            {
              string listedCodec = H264list()[i].ToString();
              if (listedCodec == Windows7Codec) continue;
              xmlwriter.SetValue("mytv", "h264videocodec", listedCodec);
              break;
            }
          }
        }
      }
    }

    private void OnH264VideoCodec()
    {
      string strH264VideoCodec = "";
      using (Settings xmlreader = new MPSettings())
      {
        strH264VideoCodec = xmlreader.GetValueAsString("mytv", "h264videocodec", "");
      }
      ArrayList availableH264VideoFilters = H264list();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableH264VideoFilters)
        {
          dlg.Add(codec); //delete
          if (codec == strH264VideoCodec)
          {
            selected = count;
          }
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
      {
        return;
      }
      string selectedCodec = (string)availableH264VideoFilters[dlg.SelectedLabel];
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("mytv", "h264videocodec", selectedCodec);

        if (selectedCodec.Contains(Windows7Codec))
        {
          xmlwriter.SetValue("mytv", "videocodec", selectedCodec);
        }
        else
        {
          string videoCodec = xmlwriter.GetValue("mytv", "videocodec");
          if (videoCodec.Contains(Windows7Codec))
          {
            for (int i = 0; i < H264list().Count; i++)
            {
              string listedCodec = MPEG2list()[i].ToString();
              if (listedCodec == Windows7Codec) continue;
              xmlwriter.SetValue("mytv", "videocodec", listedCodec);
              break;
            }
          }
        }
      }

    }

    private void OnAudioCodec()
    {
      string strAudioCodec = "";
      using (Settings xmlreader = new MPSettings())
      {
        strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
      }
      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
      //Remove Muxer's from the list to avoid confusion.
      while (availableAudioFilters.Contains("CyberLink MPEG Muxer"))
      {
        availableAudioFilters.Remove("CyberLink MPEG Muxer");
      }
      while (availableAudioFilters.Contains("Ulead MPEG Muxer"))
      {
        availableAudioFilters.Remove("Ulead MPEG Muxer");
      }
      while (availableAudioFilters.Contains("PDR MPEG Muxer"))
      {
        availableAudioFilters.Remove("PDR MPEG Muxer");
      }
      while (availableAudioFilters.Contains("Nero Mpeg2 Encoder"))
      {
        availableAudioFilters.Remove("Nero Mpeg2 Encoder");
      }
      availableAudioFilters.Sort();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableAudioFilters)
        {
          dlg.Add(codec); //delete
          if (codec == strAudioCodec)
          {
            selected = count;
          }
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("mytv", "audiocodec", (string)availableAudioFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAACAudioCodec()
    {
      string strAACAudioCodec = "";
      using (Settings xmlreader = new MPSettings())
      {
        strAACAudioCodec = xmlreader.GetValueAsString("mytv", "aacaudiocodec", "");
      }
      ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.LATMAAC);
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableAACAudioFilters)
        {
          dlg.Add(codec); //delete
          if (codec == strAACAudioCodec)
          {
            selected = count;
          }
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("mytv", "aacaudiocodec", (string)availableAACAudioFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAspectRatio()
    {
      Geometry.Type aspectRatio = Geometry.Type.Normal;
      using (Settings xmlreader = new MPSettings())
      {
        string aspectRatioText = xmlreader.GetValueAsString("mytv", "defaultar", "Normal");
        aspectRatio = Utils.GetAspectRatio(aspectRatioText);
      }

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(941); // Change aspect ratio

      dlg.AddLocalizedString(942); // Stretch
      dlg.AddLocalizedString(943); // Normal
      dlg.AddLocalizedString(944); // Original
      dlg.AddLocalizedString(945); // Letterbox
      dlg.AddLocalizedString(946); // Pan and scan
      dlg.AddLocalizedString(947); // Zoom
      dlg.AddLocalizedString(1190); // Zoom 14:9

      // set the focus to currently used mode
      dlg.SelectedLabel = (int)aspectRatio;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
      {
        return;
      }

      aspectRatio = Utils.GetAspectRatioByLangID(dlg.SelectedId);

      using (Settings xmlwriter = new MPSettings())
      {
        string aspectRatioText = Utils.GetAspectRatio(aspectRatio);
        xmlwriter.SetValue("mytv", "defaultar", aspectRatioText);
      }
    }

    private void OnDeinterlace()
    {
      string[] deinterlaceModes = { "None", "Bob", "Weave", "Best" };
      int deInterlaceMode = 1;
      using (Settings xmlreader = new MPSettings())
      {
        deInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 3);
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        for (int index = 0; index < deinterlaceModes.Length; index++)
        {
          dlg.Add(deinterlaceModes[index]);
        }
        dlg.SelectedLabel = deInterlaceMode;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }
        using (Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValue("mytv", "deinterlace", dlg.SelectedLabel);
        }
      }
    }

    private void OnAutoTurnOnTv()
    {
      bool autoTurnOn = false;
      using (Settings xmlreader = new MPSettings())
      {
        autoTurnOn = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        dlg.Add(GUILocalizeStrings.Get(775)); //Start TV in MyTV sections automatically
        dlg.Add(GUILocalizeStrings.Get(776)); //Do not start / switch to TV automatically
        dlg.SelectedLabel = autoTurnOn ? 0 : 1;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }
        using (Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValueAsBool("mytv", "autoturnontv", (dlg.SelectedLabel == 0));
        }
      }
    }

    private void OnAudioRenderer()
    {
      string strAudioRenderer = "";
      using (Settings xmlreader = new MPSettings())
      {
        strAudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "Default DirectSound Device");
      }
      ArrayList availableAudioFilters = FilterHelper.GetAudioRenderers();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableAudioFilters)
        {
          dlg.Add(codec); //delete
          if (codec == strAudioRenderer)
          {
            selected = count;
          }
          count++;
        }
        dlg.SelectedLabel = selected;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0)
      {
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("mytv", "audiorenderer", (string)availableAudioFilters[dlg.SelectedLabel]);
      }
    }
  }
}