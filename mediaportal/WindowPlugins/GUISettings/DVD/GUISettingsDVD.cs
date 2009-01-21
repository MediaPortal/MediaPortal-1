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
using System.Collections;
using System.Globalization;
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
  /// Summary description for GUISettingsDVD.
  /// </summary>
  public class GUISettingsDVD : GUIWindow
  {
    [SkinControl(21)] protected GUIButtonControl btnDVDNavigator = null;
    [SkinControl(22)] protected GUIToggleButtonControl btnEnableSubtitles = null;
    [SkinControl(23)] protected GUIToggleButtonControl btnDXVA = null;
    [SkinControl(24)] protected GUIButtonControl btnVideoCodec = null;
    [SkinControl(25)] protected GUIButtonControl btnAudioCodec = null;
    [SkinControl(27)] protected GUIButtonControl btnAudioRenderer = null;
    [SkinControl(28)] protected GUIButtonControl btnAspectRatio = null;
    [SkinControl(29)] protected GUIButtonControl btnSubtitle = null;
    [SkinControl(30)] protected GUIButtonControl btnAudioLanguage = null;

    private bool dxvaSetting;
    private bool subtitleSettings;
    private bool settingsLoaded = false;

    private class CultureComparer : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        CultureInfo info1 = (CultureInfo) x;
        CultureInfo info2 = (CultureInfo) y;
        return String.Compare(info1.EnglishName, info2.EnglishName, true);
      }

      #endregion
    }

    public GUISettingsDVD()
    {
      GetID = (int) Window.WINDOW_SETTINGS_DVD;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\settings_dvd.xml");
      return bResult;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadSettings();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnDVDNavigator)
      {
        OnDVDNavigator();
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
      if (control == btnAudioRenderer)
      {
        OnAudioRenderer();
      }
      if (control == btnSubtitle)
      {
        OnSubtitle();
      }
      if (control == btnAudioLanguage)
      {
        OnAudioLanguage();
      }
      if (control == btnDXVA)
      {
        OnDXVA();
      }
      if (control == btnEnableSubtitles)
      {
        OnSubtitleOnOff();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnDVDNavigator()
    {
      string strDVDNavigator = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strDVDNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
      }
      ArrayList availableDVDNavigators = FilterHelper.GetDVDNavigators();
      availableDVDNavigators.Sort();
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        int selected = 0;
        int count = 0;
        foreach (string codec in availableDVDNavigators)
        {
          dlg.Add(codec);
          if (codec == strDVDNavigator)
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
        xmlwriter.SetValue("dvdplayer", "navigator", (string) availableDVDNavigators[dlg.SelectedLabel]);
      }
    }

    private void OnVideoCodec()
    {
      string strVideoCodec = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strVideoCodec = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
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
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
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
        xmlwriter.SetValue("dvdplayer", "videocodec", (string) availableVideoFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAudioCodec()
    {
      string strAudioCodec = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strAudioCodec = xmlreader.GetValueAsString("dvdplayer", "audiocodec", "");
      }
      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
      //Remove Muxer's from Audio decoder list to avoid confusion.
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
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
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
        xmlwriter.SetValue("dvdplayer", "audiocodec", (string) availableAudioFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAspectRatio()
    {
      Geometry.Type aspectRatio = Geometry.Type.Normal;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string aspectRatioText = xmlreader.GetValueAsString("dvdplayer", "defaultar", "normal");
        aspectRatio = Utils.GetAspectRatio(aspectRatioText);
      }
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
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
      dlg.SelectedLabel = (int) aspectRatio;
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
        xmlwriter.SetValue("dvdplayer", "defaultar", aspectRatioText);
      }
    }

    private void OnAudioRenderer()
    {
      string strAudioRenderer = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strAudioRenderer = xmlreader.GetValueAsString("dvdplayer", "audiorenderer", "Default DirectSound Device");
      }
      ArrayList availableAudioFilters = FilterHelper.GetAudioRenderers();
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
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
        xmlwriter.SetValue("dvdplayer", "audiorenderer", (string) availableAudioFilters[dlg.SelectedLabel]);
      }
    }

    private void OnSubtitle()
    {
      string defaultSubtitleLanguage = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        defaultSubtitleLanguage = xmlreader.GetValueAsString("dvdplayer", "subtitlelanguage", "English");
      }
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        dlg.ShowQuickNumbers = false;
        int selected = 0;
        ArrayList cultures = new ArrayList();
        CultureInfo[] culturesInfos = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
        for (int i = 0; i < culturesInfos.Length; ++i)
        {
          cultures.Add(culturesInfos[i]);
        }
        cultures.Sort(new CultureComparer());
        for (int i = 0; i < cultures.Count; ++i)
        {
          CultureInfo info = (CultureInfo) cultures[i];
          if (info.EnglishName.Equals(defaultSubtitleLanguage))
          {
            selected = i;
          }
          dlg.Add(info.EnglishName);
        }
        dlg.SelectedLabel = selected;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }
        using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          CultureInfo info = (CultureInfo) cultures[dlg.SelectedLabel];
          xmlwriter.SetValue("dvdplayer", "subtitlelanguage", info.EnglishName);
        }
      }
    }

    private void OnAudioLanguage()
    {
      string defaultAudioLanguage = "";
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        defaultAudioLanguage = xmlreader.GetValueAsString("dvdplayer", "audiolanguage", "English");
      }
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        dlg.ShowQuickNumbers = false;
        int selected = 0;
        ArrayList cultures = new ArrayList();
        CultureInfo[] culturesInfos = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
        for (int i = 0; i < culturesInfos.Length; ++i)
        {
          cultures.Add(culturesInfos[i]);
        }
        cultures.Sort(new CultureComparer());
        for (int i = 0; i < cultures.Count; ++i)
        {
          CultureInfo info = (CultureInfo) cultures[i];
          if (info.EnglishName.Equals(defaultAudioLanguage))
          {
            selected = i;
          }
          dlg.Add(info.EnglishName);
        }
        dlg.SelectedLabel = selected;
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0)
        {
          return;
        }
        using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          CultureInfo info = (CultureInfo) cultures[dlg.SelectedLabel];
          xmlwriter.SetValue("dvdplayer", "audiolanguage", info.EnglishName);
        }
      }
    }

    private void OnDXVA()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("dvdplayer", "turnoffdxva", btnDXVA.Selected);
      }
    }

    private void OnSubtitleOnOff()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("dvdplayer", "showsubtitles", btnEnableSubtitles.Selected);
      }
    }

    private void LoadSettings()
    {
      if (settingsLoaded)
      {
        return;
      }
      settingsLoaded = true;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        subtitleSettings = xmlreader.GetValueAsBool("dvdplayer", "showsubtitles", false);
        btnEnableSubtitles.Selected = subtitleSettings;
        dxvaSetting = xmlreader.GetValueAsBool("dvdplayer", "turnoffdxva", true);
        btnDXVA.Selected = dxvaSetting;
      }
    }
  }
}