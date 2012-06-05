#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings.TV
{
  /// <summary>
  /// Summary description for GUISettingsMovies.
  /// </summary>
  public class GUISettingsMovies : GUIInternalWindow
  {
    [SkinControl(24)]
    protected GUIButtonControl btnVideoCodec = null;
    [SkinControl(25)]
    protected GUIButtonControl btnAudioCodec = null;
    //[SkinControlAttribute(26)]			protected GUIButtonControl btnVideoRenderer=null;
    [SkinControl(27)]
    protected GUIButtonControl btnAudioRenderer = null;
    [SkinControl(28)]
    protected GUIButtonControl btnAspectRatio = null;
    [SkinControl(29)]
    protected GUIButtonControl btnH264VideoCodec = null;
    [SkinControl(30)]
    protected GUIButtonControl btnAACAudioCodec = null;
    [SkinControl(31)]
    protected GUIToggleButtonControl btnEnableSubtitles = null;
    [SkinControl(32)]
    protected GUIButtonControl btnSubtitle = null;
    [SkinControl(33)]
    protected GUIButtonControl btnAudioLanguage = null;
    [SkinControl(34)]
    protected GUIButtonControl btnVC1VideoCodec = null;
    [SkinControl(35)]
    protected GUIButtonControl btnVC1IVideoCodec = null;
    [SkinControl(36)]
    protected GUIButtonControl btnXVIDVideoCodec = null;
    [SkinControl(37)]
    protected GUIButtonControl btnFileSyncSplitter = null;
    [SkinControl(38)]
    protected GUIButtonControl btnSourceSplitter = null;
    [SkinControl(39)]
    protected GUIToggleButtonControl btnForceSourceSplitter = null;

    private class CultureComparer : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        CultureInfo info1 = (CultureInfo)x;
        CultureInfo info2 = (CultureInfo)y;
        return String.Compare(info1.EnglishName, info2.EnglishName, true);
      }

      #endregion
    }

    public GUISettingsMovies()
    {
      GetID = (int)Window.WINDOW_SETTINGS_MOVIES;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_movies.xml"));
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      SetButtonsState();
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
      if (control == btnVC1VideoCodec)
      {
        OnVC1VideoCodec();
      }
      if (control == btnVC1IVideoCodec)
      {
        OnVC1IVideoCodec();
      }
      if (control == btnXVIDVideoCodec)
      {
        OnXVIDVideoCodec();
      }
      if (control == btnForceSourceSplitter)
      {
        OnForceSourceSplitter();
      }
      if (control == btnFileSyncSplitter)
      {
        OnFileSyncSplitter();
      }
      if (control == btnSourceSplitter)
      {
        OnSourceSplitter();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnVideoCodec()
    {
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

      ShowSettingsDialog("movieplayer", "mpeg2videocodec", availableVideoFilters);
    }

    private void OnH264VideoCodec()
    {
      ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
      availableH264VideoFilters.Sort();

      ShowSettingsDialog("movieplayer", "h264videocodec", availableH264VideoFilters);
    }

    private void OnAudioCodec()
    {
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

      ShowSettingsDialog("movieplayer", "mpeg2audiocodec", availableAudioFilters);
    }

    private void OnAspectRatio()
    {
      Geometry.Type aspectRatio = Geometry.Type.Normal;
      using (Settings xmlreader = new MPSettings())
      {
        string aspectRatioText = xmlreader.GetValueAsString("movieplayer", "defaultar", "Normal");
        aspectRatio = Utils.GetAspectRatio(aspectRatioText);
      }

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(941); // Change aspect ratio

      dlg.AddLocalizedString(943); // Normal
      dlg.AddLocalizedString(944); // Original
      dlg.AddLocalizedString(947); // Zoom
      dlg.AddLocalizedString(1190); // Zoom 14:9
      dlg.AddLocalizedString(942); // Stretch
      dlg.AddLocalizedString(945); // Letterbox
      dlg.AddLocalizedString(946); // Non linear stretch

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
        xmlwriter.SetValue("movieplayer", "defaultar", aspectRatioText);
      }
    }

    private void OnAudioRenderer()
    {
      ArrayList availableAudioFilters = FilterHelper.GetAudioRenderers();
      availableAudioFilters.Sort();

      ShowSettingsDialog("movieplayer", "audiorenderer", "Default DirectSound Device", availableAudioFilters);
    }

    private void OnAACAudioCodec()
    {
      ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.AAC);
      availableAACAudioFilters.Sort();

      ShowSettingsDialog("movieplayer", "aacaudiocodec", availableAACAudioFilters);
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
        defaultSubtitleLanguage = xmlreader.GetValueAsString("subtitles", "language", "EN");
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
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
          CultureInfo info = (CultureInfo)cultures[i];
          if (info.Name.Equals(defaultSubtitleLanguage))
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
          CultureInfo info = (CultureInfo)cultures[dlg.SelectedLabel];
          xmlwriter.SetValue("subtitles", "language", info.Name);
        }
      }
    }

    private void OnAudioLanguage()
    {
      string defaultAudioLanguage = "";
      using (Settings xmlreader = new MPSettings())
      {
        defaultAudioLanguage = xmlreader.GetValueAsString("movieplayer", "audiolanguage", "EN");
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
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
          CultureInfo info = (CultureInfo)cultures[i];
          if (info.Name.Equals(defaultAudioLanguage))
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
          CultureInfo info = (CultureInfo)cultures[dlg.SelectedLabel];
          xmlwriter.SetValue("movieplayer", "audiolanguage", info.Name);
        }
      }
    }

    private void OnVC1VideoCodec()
    {
      ArrayList availableVC1VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.VC1);
      ArrayList availableVC1CyberlinkVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.CyberlinkVC1);

      availableVC1VideoFilters.AddRange(availableVC1CyberlinkVideoFilters.ToArray());
      availableVC1VideoFilters.Sort();

      ShowSettingsDialog("movieplayer", "vc1videocodec", availableVC1VideoFilters);
    }

    private void OnVC1IVideoCodec()
    {
      ArrayList availableVC1IVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.VC1);
      ArrayList availableVC1ICyberlinkVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.CyberlinkVC1);

      while (availableVC1IVideoFilters.Contains("MPC - Video decoder"))
      {
        availableVC1IVideoFilters.Remove("MPC - Video decoder");
        break;
      }
      while (availableVC1IVideoFilters.Contains("WMVideo Decoder DMO"))
      {
        availableVC1IVideoFilters.Remove("WMVideo Decoder DMO");
        break;
      }
      availableVC1IVideoFilters.AddRange(availableVC1ICyberlinkVideoFilters.ToArray());
      availableVC1IVideoFilters.Sort();

      ShowSettingsDialog("movieplayer", "vc1ivideocodec", availableVC1IVideoFilters);
    }

    private void OnXVIDVideoCodec()
    {
      ArrayList availableXVIDVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.XVID);

      availableXVIDVideoFilters.Sort();

      ShowSettingsDialog("movieplayer", "xvidvideocodec", availableXVIDVideoFilters);
    }

    private void OnForceSourceSplitter()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        btnFileSyncSplitter.Disabled = !btnForceSourceSplitter.Selected;
        btnSourceSplitter.Disabled = !btnForceSourceSplitter.Selected;

        xmlwriter.SetValueAsBool("movieplayer", "forcesourcesplitter", btnForceSourceSplitter.Selected);
      }
    }

    private void OnFileSyncSplitter()
    {
      ArrayList availableFileSyncFilters = FilterHelper.GetFilters(MediaType.Stream, MediaSubType.Null);
      availableFileSyncFilters.Sort();

      ShowSettingsDialog("movieplayer", "splitterfilefilter", availableFileSyncFilters);
    }

    private void OnSourceSplitter()
    {
      ArrayList availableSourcesFilters = FilterHelper.GetFilterSource();
      ArrayList availableFileSyncFilters = FilterHelper.GetFilters(MediaType.Stream, MediaSubType.Null);

      while (availableFileSyncFilters.Contains("Haali Media Splitter (AR)"))
      {
        availableSourcesFilters.Add("Haali Media Splitter");
        break;
      }

      availableSourcesFilters.Sort();

      ShowSettingsDialog("movieplayer", "splitterfilter", availableSourcesFilters);
    }

    private void ShowSettingsDialog(string section, string entry, ArrayList availableValues)
    {
      ShowSettingsDialog(section, entry, "", availableValues);
    }

    private void ShowSettingsDialog(string section, string entry, string defaultValue, ArrayList availableValues)
    {
      string strCurrentValue = "";
      using (Settings xmlreader = new MPSettings())
      {
        strCurrentValue = xmlreader.GetValueAsString(section, entry, defaultValue);
      }

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
        int selected = 0;
        int count = 0;
        foreach (string value in availableValues)
        {
          dlg.Add(value); //delete
          if (value == strCurrentValue)
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
        xmlwriter.SetValue(section, entry, (string)availableValues[dlg.SelectedLabel]);
      }
    }

    private void SetButtonsState()
    {
      using (Settings xmlreader = new MPSettings())
      {
        btnEnableSubtitles.Selected = xmlreader.GetValueAsBool("subtitles", "enabled", false);

        bool forceSourceSplitter = xmlreader.GetValueAsBool("movieplayer", "forcesourcesplitter", false);
        btnForceSourceSplitter.Selected = forceSourceSplitter;

        btnFileSyncSplitter.Disabled = !forceSourceSplitter;
        btnSourceSplitter.Disabled = !forceSourceSplitter;
      }
    }
  }
}