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
  /// Summary description for GUISettingsMovies.
  /// </summary>
  public class GUISettingsMovies : GUIWindow
  {
    [SkinControl(24)] protected GUIButtonControl btnVideoCodec = null;
    [SkinControl(25)] protected GUIButtonControl btnAudioCodec = null;
    //[SkinControlAttribute(26)]			protected GUIButtonControl btnVideoRenderer=null;
    [SkinControl(27)] protected GUIButtonControl btnAudioRenderer = null;
    [SkinControl(28)] protected GUIButtonControl btnAspectRatio = null;
    [SkinControl(29)] protected GUIButtonControl btnH264VideoCodec = null;
    [SkinControl(30)] protected GUIButtonControl btnAACAudioCodec = null;
    [SkinControl(31)] protected GUIToggleButtonControl btnEnableSubtitles = null;
    [SkinControl(32)] protected GUIButtonControl btnSubtitle = null;
    [SkinControl(33)] protected GUIButtonControl btnAudioLanguage = null;

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

    public GUISettingsMovies()
    {
      GetID = (int) Window.WINDOW_SETTINGS_MOVIES;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settings_movies.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadSettings();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
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
      if (control == btnH264VideoCodec)
      {
        OnH264VideoCodec();
      }
      if (control == btnAACAudioCodec)
      {
        OnAACAudioCodec();
      }
      if (control == btnEnableSubtitles)
      {
        OnSubtitleOnOff();
      }
      if (control == btnSubtitle)
      {
        OnSubtitle();
      }
      if (control == btnAudioLanguage)
      {
        OnAudioLanguage();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnVideoCodec()
    {
      string strVideoCodec = "";
      using (Settings xmlreader = new MPSettings())
      {
        strVideoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
      }
      ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
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
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("movieplayer", "mpeg2videocodec", (string) availableVideoFilters[dlg.SelectedLabel]);
      }
    }

    private void OnH264VideoCodec()
    {
      string strH264VideoCodec = "";
      using (Settings xmlreader = new MPSettings())
      {
        strH264VideoCodec = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
      }
      ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
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
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("movieplayer", "h264videocodec", (string) availableH264VideoFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAudioCodec()
    {
      string strAudioCodec = "";
      using (Settings xmlreader = new MPSettings())
      {
        strAudioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
      }
      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
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
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("movieplayer", "mpeg2audiocodec", (string) availableAudioFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAspectRatio()
    {
      Geometry.Type aspectRatio = Geometry.Type.Normal;
      using (Settings xmlreader = new MPSettings())
      {
        string aspectRatioText = xmlreader.GetValueAsString("movieplayer", "defaultar", "Normal");
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

      using (Settings xmlwriter = new MPSettings())
      {
        string aspectRatioText = Utils.GetAspectRatio(aspectRatio);
        xmlwriter.SetValue("movieplayer", "defaultar", aspectRatioText);
      }
    }

    private void OnAudioRenderer()
    {
      string strAudioRenderer = "";
      using (Settings xmlreader = new MPSettings())
      {
        strAudioRenderer = xmlreader.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
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
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("movieplayer", "audiorenderer", (string) availableAudioFilters[dlg.SelectedLabel]);
      }
    }

    private void OnAACAudioCodec()
    {
      string strAACAudioCodec = "";
      using (Settings xmlreader = new MPSettings())
      {
        strAACAudioCodec = xmlreader.GetValueAsString("movieplayer", "aacaudiocodec", "");
      }
      ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.AAC);
      availableAACAudioFilters.Sort();
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
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
        xmlwriter.SetValue("movieplayer", "aacaudiocodec", (string) availableAACAudioFilters[dlg.SelectedLabel]);
      }
    }

    private void OnSubtitleOnOff()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("subtitles", "enabled", btnEnableSubtitles.Selected);
      }
    }

    private void OnSubtitle()
    {
      string defaultSubtitleLanguage = "";
      using (Settings xmlreader = new MPSettings())
      {
        defaultSubtitleLanguage = xmlreader.GetValueAsString("subtitles", "language", "English");
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
        using (Settings xmlwriter = new MPSettings())
        {
          CultureInfo info = (CultureInfo) cultures[dlg.SelectedLabel];
          xmlwriter.SetValue("subtitles", "language", info.EnglishName);
        }
      }
    }

    private void OnAudioLanguage()
    {
      string defaultAudioLanguage = "";
      using (Settings xmlreader = new MPSettings())
      {
        defaultAudioLanguage = xmlreader.GetValueAsString("movieplayer", "audiolanguage", "English");
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
        using (Settings xmlwriter = new MPSettings())
        {
          CultureInfo info = (CultureInfo) cultures[dlg.SelectedLabel];
          xmlwriter.SetValue("movieplayer", "audiolanguage", info.EnglishName);
        }
      }
    }

    private void LoadSettings()
    {
      if (settingsLoaded)
      {
        return;
      }
      settingsLoaded = true;
      using (Settings xmlreader = new MPSettings())
      {
        subtitleSettings = xmlreader.GetValueAsBool("subtitles", "enabled", false);
        btnEnableSubtitles.Selected = subtitleSettings;
      }
    }
  }
}