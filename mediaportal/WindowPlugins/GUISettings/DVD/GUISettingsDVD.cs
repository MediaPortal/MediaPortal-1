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
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings.TV
{
  /// <summary>
  /// Summary description for GUISettingsDVD.
  /// </summary>
  public class GUISettingsDVD : GUIInternalWindow
  {
    [SkinControl(22)] protected GUICheckButton btnEnableSubtitles = null;
    [SkinControl(23)] protected GUICheckButton btnDXVA = null;
    [SkinControl(24)] protected GUIButtonControl btnVideo = null;
    [SkinControl(25)] protected GUIButtonControl btnAudio = null;
    [SkinControl(29)] protected GUIButtonControl btnSubtitle = null;
    [SkinControl(31)] protected GUICheckButton btnEnableCC = null;
    

    private bool _dxvaSetting;
    private bool _subtitleSettings;
    private int _selectedOption;
    private int _selectedOptionLvl2;
    private bool _ccSetting;

    private enum AspectRatioCorrectionMode
    {
      Crop = 0,
      Letterbox = 1,
      Stretch = 2,
      Followstream = 3
    }

    private enum AspectRatioDisplayMode
    {
      DisplayContentDefault =0,
      Display16x9 = 1,
      Display4x3PanScanPreferred = 2,
      Display4x3LetterBoxPreferred = 3
    }

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

    public GUISettingsDVD()
    {
      GetID = (int)Window.WINDOW_SETTINGS_DVD; //704
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_dvd.xml"));
      return bResult;
    }

    #region Serialization

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _subtitleSettings = xmlreader.GetValueAsBool("dvdplayer", "showsubtitles", false);
        btnEnableSubtitles.Selected = _subtitleSettings;
        _dxvaSetting = xmlreader.GetValueAsBool("dvdplayer", "turnoffdxva", true);
        btnDXVA.Selected = _dxvaSetting;
        _ccSetting = xmlreader.GetValueAsBool("dvdplayer", "showclosedcaptions", false);
        btnEnableCC.Selected = _ccSetting;
      }
    }

    #endregion

    #region Overrides
    
    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100704)); //DVD
      LoadSettings();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnVideo)
      {
        _selectedOption = -1;
        _selectedOptionLvl2 = -1;
        OnVideo();
      }
      if (control == btnAudio)
      {
        _selectedOption = -1;
        OnAudio();
      }
      if (control == btnDXVA)
      {
        OnDXVA();
      }
      if (control == btnEnableSubtitles)
      {
        OnSubtitleOnOff();
      }
      if (control == btnEnableCC)
      {
        OnEnableCC();
      }
      base.OnClicked(controlId, control, actionType);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    #endregion

    private AspectRatioCorrectionMode GetAspectRatioCorrectionMode(string aspectRatioCorrectionModeText)
    {
      switch (aspectRatioCorrectionModeText)
      {
        case "Crop":
          return AspectRatioCorrectionMode.Crop;

        case "Letterbox":
          return AspectRatioCorrectionMode.Letterbox;

        case "Stretch":
          return AspectRatioCorrectionMode.Stretch;
        
        case "Follow Stream":
          return AspectRatioCorrectionMode.Followstream;

        default:
          return AspectRatioCorrectionMode.Followstream;
      }
    }

    private string GetAspectRatioCorrectionMode(AspectRatioCorrectionMode aspectRatioCorrectionMode)
    {
      switch (aspectRatioCorrectionMode)
      {
        case AspectRatioCorrectionMode.Crop:
          return "Crop";

        case AspectRatioCorrectionMode.Letterbox:
          return "Letterbox";

        case AspectRatioCorrectionMode.Stretch:
          return "Stretch";

        case AspectRatioCorrectionMode.Followstream:
          return "Follow stream";

        default:
          return "Follow stream";
      }
    }

    private AspectRatioDisplayMode GetAspectRatioDisplayMode(string aspectRatioDisplayModeText)
    {
      switch (aspectRatioDisplayModeText)
      {
        case "Default":
          return AspectRatioDisplayMode.DisplayContentDefault;

        case "16:9":
          return AspectRatioDisplayMode.Display16x9;

        case "4:3 Pan Scan":
          return AspectRatioDisplayMode.Display4x3PanScanPreferred;

        case "4:3 Letterbox":
          return AspectRatioDisplayMode.Display4x3LetterBoxPreferred;

        default:
          return AspectRatioDisplayMode.DisplayContentDefault;
      }
    }

    private string GetAspectRatioDisplayMode(AspectRatioDisplayMode aspectRatioDisplayMode)
    {
      switch (aspectRatioDisplayMode)
      {
        case AspectRatioDisplayMode.DisplayContentDefault:
          return "Default";

        case AspectRatioDisplayMode.Display16x9:
          return "16:9";

        case AspectRatioDisplayMode.Display4x3PanScanPreferred:
          return "4:3 Pan Scan";

        case AspectRatioDisplayMode.Display4x3LetterBoxPreferred:
          return "4:3 Letterbox";

        default:
          return "Default";
      }
    }

    private void OnVideo()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu

        dlg.AddLocalizedString(1198); // Navigator
        dlg.AddLocalizedString(6000); // MPEG2
        dlg.AddLocalizedString(6004); // Aspect Ratio
        dlg.AddLocalizedString(1029); // Subtitle

        _selectedOptionLvl2 = -1;

        if (_selectedOption != -1)
          dlg.SelectedLabel = _selectedOption;

        dlg.DoModal(GetID);

        if (dlg.SelectedId == -1)
        {
          return;
        }

        _selectedOption = dlg.SelectedLabel;

        switch (dlg.SelectedId)
        {
          case 1198:
            OnDVDNavigator();
            break;
          case 6000:
            OnVideoCodec();
            break;
          case 6004:
            OnAspectRatio();
            break;
          case 1029:
            OnSubtitle();
            break;
        }
      }
    }

    private void OnAudio()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu

        dlg.AddLocalizedString(6001); // Audio codec
        dlg.AddLocalizedString(6002); // Audio render
        dlg.AddLocalizedString(492);  // Audio language

        if (_selectedOption != -1)
          dlg.SelectedLabel = _selectedOption;

        dlg.DoModal(GetID);

        if (dlg.SelectedId == -1)
        {
          return;
        }

        _selectedOption = dlg.SelectedLabel;

        switch (dlg.SelectedId)
        {
          case 6001:
            OnAudioCodec();
            break;
          case 6002:
            OnAudioRenderer();
            break;
          case 492:
            OnAudioLanguage();
            break;
        }
      }
    }

    private void OnDVDNavigator()
    {
      string strDVDNavigator = "";
      
      using (Settings xmlreader = new MPSettings())
      {
        strDVDNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
      }
      
      ArrayList availableDVDNavigators = FilterHelper.GetDVDNavigators();
      availableDVDNavigators.Sort();
      
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        return;
      }

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
      
      dlg.DoModal(GetID);
      
      if (dlg.SelectedLabel < 0)
      {
        OnVideo();
        return;
      }
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("dvdplayer", "navigator", (string)availableDVDNavigators[dlg.SelectedLabel]);
      }
      OnVideo();
    }

    private void OnVideoCodec()
    {
      string strVideoCodec = "";
      using (Settings xmlreader = new MPSettings())
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
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }

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
      
      dlg.DoModal(GetID);
      
      if (dlg.SelectedLabel < 0)
      {
        OnVideo();
        return;
      }
      
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("dvdplayer", "videocodec", (string)availableVideoFilters[dlg.SelectedLabel]);
      }
      
      OnVideo();
    }

    private void OnAudioCodec()
    {
      string strAudioCodec = "";
      using (Settings xmlreader = new MPSettings())
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
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }

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
      
      dlg.DoModal(GetID);
      
      if (dlg.SelectedLabel < 0)
      {
        OnAudio();
        return;
      }
      
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("dvdplayer", "audiocodec", (string)availableAudioFilters[dlg.SelectedLabel]);
      }
      
      OnAudio();
    }
    
    private void OnAspectRatio()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(941); // Change aspect ratio

      dlg.AddLocalizedString(300000); // Pixel ratio correction
      dlg.AddLocalizedString(300001); // Correction mode
      dlg.AddLocalizedString(300002); // Display mode
      dlg.AddLocalizedString(300003); // Zoom mode

      if (_selectedOptionLvl2 != -1)
      {
        dlg.SelectedLabel = _selectedOptionLvl2;
      }

      // show dialog and wait for result
      dlg.DoModal(GetID);
      
      if (dlg.SelectedId == -1)
      {
        OnVideo();
        return;
      }

      _selectedOptionLvl2 = dlg.SelectedLabel;

      switch (dlg.SelectedId)
      {
        case 300000:
          OnPixelRatioCorrection();
          break;
        case 300001:
          OnAspectRatioCorrectionMode();
          break;
        case 300002:
          OnAspectRatioDisplayMode();
          break;
        case 300003:
          OnAspectRatioZoomMode();
          break;
      }
    }

    private void OnPixelRatioCorrection()
    {
      bool usePixelRatioCorrection;
      using (Settings xmlreader = new MPSettings())
      {
        usePixelRatioCorrection = xmlreader.GetValueAsBool("dvdplayer", "pixelratiocorrection", false);
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }
      
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      dlg.AddLocalizedString(107);
      dlg.AddLocalizedString(106);
      
      // set the focus to currently used mode
      if (usePixelRatioCorrection)
      {
        dlg.SelectedLabel = 0;
      }
      else
      {
        dlg.SelectedLabel = 1;
      }
      // show dialog and wait for result
      dlg.DoModal(GetID);
      
      if (dlg.SelectedLabel == -1)
      {
        OnAspectRatio();
        return;
      }
      
      if (dlg.SelectedLabel == 0)
      {
        usePixelRatioCorrection = true;
      }
      else
      {
        usePixelRatioCorrection = false;
      }
      
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("dvdplayer", "pixelratiocorrection", usePixelRatioCorrection); ;
      }
      
      OnAspectRatio();
    }

    private void OnAspectRatioCorrectionMode()
    {
      AspectRatioCorrectionMode aspectRatioCorrectionMode = AspectRatioCorrectionMode.Followstream;
      using (Settings xmlreader = new MPSettings())
      {
        string aspectRatioCorrectionModeText = xmlreader.GetValueAsString("dvdplayer", "armode", "Follow stream");
        aspectRatioCorrectionMode = GetAspectRatioCorrectionMode(aspectRatioCorrectionModeText);
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      dlg.Add("Crop");
      dlg.Add("Letterbox");
      dlg.Add("Stretch");
      dlg.Add("Follow Stream");
      // set the focus to currently used mode
      dlg.SelectedLabel = (int)aspectRatioCorrectionMode;
      // show dialog and wait for result
      dlg.DoModal(GetID);
      
      if (dlg.SelectedId == -1)
      {
        OnAspectRatio();
        return;
      }
      
      aspectRatioCorrectionMode = GetAspectRatioCorrectionMode(dlg.SelectedLabelText);
      
      using (Settings xmlwriter = new MPSettings())
      {
        string aspectRatioCorrectionModetext = GetAspectRatioCorrectionMode(aspectRatioCorrectionMode);
        xmlwriter.SetValue("dvdplayer", "armode", aspectRatioCorrectionModetext);
      }
      OnAspectRatio();
    }

    private void OnAspectRatioDisplayMode()
    {
      AspectRatioDisplayMode aspectRatioDisplayMode = AspectRatioDisplayMode.DisplayContentDefault;
      using (Settings xmlreader = new MPSettings())
      {
        string aspectRatioDisplayModeText = xmlreader.GetValueAsString("dvdplayer", "displaymode", "Default");
        aspectRatioDisplayMode = GetAspectRatioDisplayMode(aspectRatioDisplayModeText);
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }
      
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      dlg.Add("Default");
      dlg.Add("16:9");
      dlg.Add("4:3 Pan Scan");
      dlg.Add("4:3 Letterbox");
      // set the focus to currently used mode
      dlg.SelectedLabel = (int)aspectRatioDisplayMode;
      // show dialog and wait for result
      dlg.DoModal(GetID);
      
      if (dlg.SelectedId == -1)
      {
        OnAspectRatio();
        return;
      }
      
      aspectRatioDisplayMode = GetAspectRatioDisplayMode(dlg.SelectedLabelText);
      
      using (Settings xmlwriter = new MPSettings())
      {
        string aspectRatioDisplayModetext = GetAspectRatioDisplayMode(aspectRatioDisplayMode);
        xmlwriter.SetValue("dvdplayer", "displaymode", aspectRatioDisplayModetext);
      }
      
      OnAspectRatio();
    }

    private void OnAspectRatioZoomMode()
    {
      Geometry.Type aspectRatio = Geometry.Type.Normal;
      
      using (Settings xmlreader = new MPSettings())
      {
        string aspectRatioText = xmlreader.GetValueAsString("dvdplayer", "defaultar", "Normal");
        aspectRatio = Utils.GetAspectRatio(aspectRatioText);
      }
      
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }
      
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
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
        OnAspectRatio();
        return;
      }
      
      aspectRatio = Utils.GetAspectRatioByLangID(dlg.SelectedId);
      
      using (Settings xmlwriter = new MPSettings())
      {
        string aspectRatioText = Utils.GetAspectRatio(aspectRatio);
        xmlwriter.SetValue("dvdplayer", "defaultar", aspectRatioText);
      }
      
      OnAspectRatio();
    }

    private void OnAudioRenderer()
    {
      string strAudioRenderer = "";
      
      using (Settings xmlreader = new MPSettings())
      {
        strAudioRenderer = xmlreader.GetValueAsString("dvdplayer", "audiorenderer", "Default DirectSound Device");
      }
      
      ArrayList availableAudioFilters = FilterHelper.GetAudioRenderers();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }

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
      dlg.DoModal(GetID);
      
      if (dlg.SelectedLabel < 0)
      {
        OnAudio();
        return;
      }
      
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("dvdplayer", "audiorenderer", (string)availableAudioFilters[dlg.SelectedLabel]);
      }
      
      OnAudio();
    }

    private void OnSubtitle()
    {
      string defaultSubtitleLanguage = "";
      
      using (Settings xmlreader = new MPSettings())
      {
        defaultSubtitleLanguage = xmlreader.GetValueAsString("dvdplayer", "subtitlelanguage", "English");
      }
      
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }
      
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
        OnVideo();
        return;
      }
      
      using (Settings xmlwriter = new MPSettings())
      {
        CultureInfo info = (CultureInfo)cultures[dlg.SelectedLabel];
        xmlwriter.SetValue("dvdplayer", "subtitlelanguage", info.EnglishName);
      }
      
      OnVideo();
    }

    private void OnAudioLanguage()
    {
      string defaultAudioLanguage = "";
      
      using (Settings xmlreader = new MPSettings())
      {
        defaultAudioLanguage = xmlreader.GetValueAsString("dvdplayer", "audiolanguage", "English");
      }
      
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }

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
        OnAudio();
        return;
      }
      
      using (Settings xmlwriter = new MPSettings())
      {
        CultureInfo info = (CultureInfo)cultures[dlg.SelectedLabel];
        xmlwriter.SetValue("dvdplayer", "audiolanguage", info.EnglishName);
      }
      
      OnAudio();
    }

    private void OnDXVA()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("dvdplayer", "turnoffdxva", btnDXVA.Selected);
      }
    }

    private void OnSubtitleOnOff()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("dvdplayer", "showsubtitles", btnEnableSubtitles.Selected);
      }
    }

    private void OnEnableCC()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("dvdplayer", "showclosedcaptions", btnEnableCC.Selected);
      }
    }
    
  }
}