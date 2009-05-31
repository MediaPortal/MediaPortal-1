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
  public class GUISettingsTv : GUIWindow
  {
    [SkinControl(24)]
    protected GUIButtonControl btnVideoCodec = null;
    [SkinControl(25)]
    protected GUIButtonControl btnAudioCodec = null;
    [SkinControl(27)]
    protected GUIButtonControl btnDeinterlace = null;
    [SkinControl(28)]
    protected GUIButtonControl btnAspectRatio = null;
    [SkinControl(30)]
    protected GUIButtonControl btnAutoTurnOnTv = null;
    [SkinControl(33)]
    protected GUIButtonControl btnAudioRenderer = null;
    [SkinControl(35)]
    protected GUIButtonControl btnH264VideoCodec = null;
    [SkinControl(36)]
    protected GUIButtonControl btnAACAudioCodec = null;

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

    private void OnVideoCodec()
    {
      string strVideoCodec = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strVideoCodec = xmlreader.GetValueAsString("mytve2", "videocodec", "");
      }
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
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytve2", "videocodec", (string)availableVideoFilters[dlg.SelectedLabel]);
      }
    }

    private void OnH264VideoCodec()
    {
      string strH264VideoCodec = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strH264VideoCodec = xmlreader.GetValueAsString("mytve2", "h264videocodec", "");
      }
      ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
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
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytve2", "h264videocodec", (string)availableH264VideoFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAudioCodec()
    {
      string strAudioCodec = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strAudioCodec = xmlreader.GetValueAsString("mytve2", "audiocodec", "");
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
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytve2", "audiocodec", (string)availableAudioFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAACAudioCodec()
    {
      string strAACAudioCodec = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strAACAudioCodec = xmlreader.GetValueAsString("mytve2", "aacaudiocodec", "");
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
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytve2", "aacaudiocodec", (string)availableAACAudioFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAspectRatio()
    {
      Geometry.Type aspectRatio = Geometry.Type.Normal;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string aspectRatioText = xmlreader.GetValueAsString("mytve2", "defaultar", "Normal");
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

      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string aspectRatioText = Utils.GetAspectRatio(aspectRatio);
        xmlwriter.SetValue("mytve2", "defaultar", aspectRatioText);
      }
    }

    private void OnDeinterlace()
    {
      string[] deinterlaceModes = { "None", "Bob", "Weave", "Best" };
      int deInterlaceMode = 1;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        deInterlaceMode = xmlreader.GetValueAsInt("mytve2", "deinterlace", 3);
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
        using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValue("mytve2", "deinterlace", dlg.SelectedLabel);
        }
      }
    }

    private void OnAutoTurnOnTv()
    {
      bool autoTurnOn = false;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        autoTurnOn = xmlreader.GetValueAsBool("mytve2", "autoturnontv", false);
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
        using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValueAsBool("mytve2", "autoturnontv", (dlg.SelectedLabel == 0));
        }
      }
    }

    private void OnAudioRenderer()
    {
      string strAudioRenderer = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strAudioRenderer = xmlreader.GetValueAsString("mytve2", "audiorenderer", "Default DirectSound Device");
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
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("mytve2", "audiorenderer", (string)availableAudioFilters[dlg.SelectedLabel]);
      }
    }
  }
}