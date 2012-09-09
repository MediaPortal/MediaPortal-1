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
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings.TV
{
  /// <summary>
  /// Summary description for GUISettingsMovies.
  /// </summary>
  public class GUISettingsBluRay : GUIInternalWindow
  {
    [SkinControl(2)] protected GUIButtonControl btnRegion = null;
    [SkinControl(3)] protected GUIButtonControl btnAudioType = null;
    [SkinControl(4)] protected GUIButtonControl btnVideo = null;
    [SkinControl(5)] protected GUIButtonControl btnAudio = null;
    [SkinControl(6)] protected GUICheckButton btnEnableSubtitles = null;

    private enum Controls
    {
      CONTROL_PARENTALAGELIMIT = 7
    } ;
    
    private bool _subtitleSettings;
    private int _selectedOption;
    
    private string _strVideoCodec;
    private string _strH264VideoCodec;
    private string _strVC1VideoCodec;
    private string _strAudioCodec;
    private string _strAudioRenderer;
    
    private string _defaultSubtitleLanguage;
    private CultureInfo _info;
    private CultureInfo _infoAudio;
    private string _defaultAudioLanguage;
    private string _defaultAudioType;
    private string _defaultRegion;
    private int i_ageLimit = 99;
    
    private string _strDefaultRegionLanguage = "English";
    string[] _regions = { "A", "B", "C" };
    string[] _audioTypes = { "AC3", "AC3+", "DTS", "DTS-HD", "DTS-HD Master", "LPCM", "TrueHD" };


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

    public GUISettingsBluRay()
    {
      GetID = (int)Window.WINDOW_SETTINGS_BLURAY; //1024
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_BluRay.xml"));
    }

    #region Serialization

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        //Video Codecs
        _strVideoCodec = xmlreader.GetValueAsString("bdplayer", "mpeg2videocodec", "");
        _strH264VideoCodec = xmlreader.GetValueAsString("bdplayer", "h264videocodec", "");
        _strVC1VideoCodec = xmlreader.GetValueAsString("bdplayer", "vc1videocodec", "");
        // Audio codecs
        _strAudioCodec = xmlreader.GetValueAsString("bdplayer", "mpeg2audiocodec", "");
        _strAudioRenderer = xmlreader.GetValueAsString("bdplayer", "audiorenderer", "Default DirectSound Device");
        // Subs/Language
        _defaultSubtitleLanguage = xmlreader.GetValueAsString("bdplayer", "subtitlelanguage", _strDefaultRegionLanguage);
        _defaultAudioLanguage = xmlreader.GetValueAsString("bdplayer", "audiolanguage", _strDefaultRegionLanguage);
        _subtitleSettings = xmlreader.GetValueAsBool("bdplayer", "subtitlesenabled", true);
        _defaultAudioType = xmlreader.GetValueAsString("bdplayer", "audiotype", "AC3");
        _defaultRegion = xmlreader.GetValueAsString("bdplayer", "regioncode", "B");
        i_ageLimit = xmlreader.GetValueAsInt("bdplayer", "parentalcontrol", 99);
        btnEnableSubtitles.Selected = _subtitleSettings;
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        // Video
        xmlwriter.SetValue("bdplayer", "mpeg2videocodec", _strVideoCodec);
        xmlwriter.SetValue("bdplayer", "h264videocodec", _strH264VideoCodec);
        xmlwriter.SetValue("bdplayer", "vc1videocodec", _strVC1VideoCodec);
        // Audio
        xmlwriter.SetValue("bdplayer", "mpeg2audiocodec", _strAudioCodec);
        xmlwriter.SetValue("bdplayer", "audiorenderer", _strAudioRenderer);
        // Subtitle and language
        if (_info != null)
        {
          xmlwriter.SetValue("bdplayer", "subtitlelanguage", _info.EnglishName);
        }
        if (_infoAudio != null)
        {
          xmlwriter.SetValue("bdplayer", "audiolanguage", _infoAudio.EnglishName);
        }
        xmlwriter.SetValueAsBool("bdplayer", "subtitlesenabled", btnEnableSubtitles.Selected);
        xmlwriter.SetValue("bdplayer", "audiotype", _defaultAudioType);
        xmlwriter.SetValue("bdplayer", "regioncode", _defaultRegion);
        xmlwriter.SetValue("bdplayer", "parentalcontrol", i_ageLimit);
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            LoadSettings();
            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_PARENTALAGELIMIT);
            for (int i = 1; i <= 99; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_PARENTALAGELIMIT, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_PARENTALAGELIMIT, i_ageLimit - 1);
            
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.CONTROL_PARENTALAGELIMIT)
            {
              string strLabel = message.Label;
              i_ageLimit = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedHorizontal = i_ageLimit;
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101024));
      LoadSettings();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnRegion)
      {
        OnRegion();
      }
      if (control == btnAudioType)
      {
        OnAudioType();
      }
      if (control == btnVideo)
      {
        _selectedOption = -1;
        OnVideo();
      }
      if (control == btnAudio)
      {
        _selectedOption = -1;
        OnAudio();
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

    private void OnVideo()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu

        dlg.AddLocalizedString(6000); // MPEG2
        dlg.AddLocalizedString(6036); // H264
        dlg.AddLocalizedString(300212); // VC-1
        dlg.AddLocalizedString(1029); // Subtitle

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
          case 6000:
            OnVideoCodec();
            break;
          case 6036:
            OnH264VideoCodec();
            break;
          case 1029:
            OnSubtitle();
            break;
          case 300212:
            OnVC1();
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
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        OnVideo();
        return;
      }

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      int selected = 0;
      int count = 0;

      foreach (string codec in availableVideoFilters)
      {
        dlg.Add(codec); //delete

        if (codec == _strVideoCodec)
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

      _strVideoCodec = (string)availableVideoFilters[dlg.SelectedLabel];
      OnVideo();
    }

    private void OnH264VideoCodec()
    {
      ArrayList availableH264VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.H264);
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        OnVideo();
        return;
      }

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      int selected = 0;
      int count = 0;

      foreach (string codec in availableH264VideoFilters)
      {
        dlg.Add(codec); //delete
        if (codec == _strH264VideoCodec)
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

      _strH264VideoCodec = (string)availableH264VideoFilters[dlg.SelectedLabel];
      OnVideo();
    }

    private void OnVC1()
    {
      ArrayList availableVC1VideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.VC1);
      ArrayList availableVC1CyberlinkVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.CyberlinkVC1);
      availableVC1VideoFilters.AddRange(availableVC1CyberlinkVideoFilters.ToArray());
      availableVC1VideoFilters.Sort();

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        OnVideo();
        return;
      }

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      int selected = 0;
      int count = 0;

      foreach (string codec in availableVC1VideoFilters)
      {
        dlg.Add(codec); //delete
        if (codec == _strVC1VideoCodec)
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

      _strVC1VideoCodec = (string)availableVC1VideoFilters[dlg.SelectedLabel];
      OnVideo();
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
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        OnAudio();
        return;
      }

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      int selected = 0;
      int count = 0;

      foreach (string codec in availableAudioFilters)
      {
        dlg.Add(codec); //delete
        if (codec == _strAudioCodec)
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

      _strAudioCodec = (string)availableAudioFilters[dlg.SelectedLabel];
      OnAudio();
    }

    private void OnAudioRenderer()
    {
      ArrayList availableAudioFilters = FilterHelper.GetAudioRenderers();
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        OnAudio();
        return;
      }

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      int selected = 0;
      int count = 0;

      foreach (string codec in availableAudioFilters)
      {
        dlg.Add(codec); //delete
        if (codec == _strAudioRenderer)
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

      _strAudioRenderer = (string)availableAudioFilters[dlg.SelectedLabel];
      OnAudio();
    }
    
    private void OnSubtitle()
    {
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
          if (info.EnglishName.Equals(_defaultSubtitleLanguage))
          {
            selected = i;
          }
          dlg.Add(info.EnglishName);
        }

        _info = (CultureInfo)cultures[selected];
        dlg.SelectedLabel = selected;
        dlg.DoModal(GetID);

        if (dlg.SelectedLabel < 0)
        {
          OnVideo();
          return;
        }

        _defaultSubtitleLanguage = dlg.SelectedLabelText;
        _info = (CultureInfo)cultures[dlg.SelectedLabel];
        OnVideo();
      }
    }

    private void OnAudioLanguage()
    {
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

          if (info.EnglishName.Equals(_defaultAudioLanguage))
          {
            selected = i;
          }
          dlg.Add(info.EnglishName);
        }

        _infoAudio = (CultureInfo)cultures[selected];
        dlg.SelectedLabel = selected;
        dlg.DoModal(GetID);

        if (dlg.SelectedLabel < 0)
        {
          OnAudio();
          return;
        }

        _defaultAudioLanguage = dlg.SelectedLabelText;
        _infoAudio = (CultureInfo)cultures[dlg.SelectedLabel];
        OnAudio();
      }
    }

    private void OnRegion()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      int selected = 0;
      int count = 0;

      foreach (string region in _regions)
      {
        dlg.Add(region); //delete
        if (region == _defaultRegion)
        {
          selected = count;
        }
        count++;
      }

      dlg.SelectedLabel = selected;

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel < 0)
      {
        return;
      }

      _defaultRegion = dlg.SelectedLabelText;
    }

    private void OnAudioType()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);

      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Menu
      int selected = 0;
      int count = 0;

      foreach (string audioType in _audioTypes)
      {
        dlg.Add(audioType); //delete
        if (audioType == _defaultAudioType)
        {
          selected = count;
        }
        count++;
      }

      dlg.SelectedLabel = selected;

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel < 0)
      {
        return;
      }

      _defaultAudioType = dlg.SelectedLabelText;
    }
    
  }
}