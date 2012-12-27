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
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsGeneral.
  /// </summary>
  public class GUISettingsGUIThumbnails : GUIInternalWindow
  {
    [SkinControl(2)] protected GUICheckButton btnEnableMusicThumbs = null;
    [SkinControl(3)] protected GUIButtonControl btnDeleteMusicThumbs= null;
    [SkinControl(4)] protected GUICheckButton btnEnablePicturesThumbs = null;
    [SkinControl(5)] protected GUIButtonControl btnDeletePicturesThumbs= null;
    [SkinControl(6)] protected GUICheckButton btnEnableVideosThumbs= null;
    [SkinControl(7)] protected GUICheckButton btnEnableLeaveThumbInFolder = null;
    [SkinControl(8)] protected GUIButtonControl btnDeleteVideosThumbs = null;
    [SkinControl(20)] protected GUIButtonControl btnClearBlacklistedThumbs = null;
    
    private enum Controls
    {
      CONTROL_THUMBNAILS_QUALITY = 10,
      CONTROL_THUMBNAILS_COLUMNS= 11,
      CONTROL_THUMBNAILS_ROWS = 12,
    } ;

    private int _iQuality = 3;
    private int _iColumns = 1;
    private int _iRows = 1;
    private bool _settingsSaved;

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
    
    public GUISettingsGUIThumbnails()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GUITHUMBNAILS; //1005
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_GUI_Thumbnails.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {

        _iQuality = xmlreader.GetValueAsInt("thumbnails", "quality", 3);
        _iQuality++;
        btnEnableMusicThumbs.Selected = xmlreader.GetValueAsBool("thumbnails", "musicfolderondemand", true);
        btnEnablePicturesThumbs.Selected = xmlreader.GetValueAsBool("thumbnails", "picturenolargethumbondemand", false);
        btnEnableVideosThumbs.Selected = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
        btnEnableLeaveThumbInFolder.Selected = xmlreader.GetValueAsBool("thumbnails", "tvrecordedsharepreview", false);
        _iColumns = xmlreader.GetValueAsInt("thumbnails", "tvthumbcols", 1);
        _iRows = xmlreader.GetValueAsInt("thumbnails", "tvthumbrows", 1);
      }
    }

    private void SaveSettings()
    {
      if (!_settingsSaved)
      {
        _settingsSaved = true;
        using (Settings xmlwriter = new MPSettings())
        {
          _iQuality--;
          xmlwriter.SetValue("thumbnails", "quality", _iQuality);
          xmlwriter.SetValueAsBool("thumbnails", "musicfolderondemand", btnEnableMusicThumbs.Selected);
          xmlwriter.SetValueAsBool("thumbnails", "picturenolargethumbondemand", btnEnablePicturesThumbs.Selected);
          xmlwriter.SetValueAsBool("thumbnails", "tvrecordedondemand", btnEnableVideosThumbs.Selected);
          xmlwriter.SetValueAsBool("thumbnails", "tvrecordedsharepreview", btnEnableLeaveThumbInFolder.Selected);
          xmlwriter.SetValue("thumbnails", "tvthumbcols", _iColumns);
          xmlwriter.SetValue("thumbnails", "tvthumbrows", _iRows);
        }
      }
    }

    #endregion

    #region Overrides

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);

            for (int i = 1; i <= 5; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_THUMBNAILS_QUALITY, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBNAILS_QUALITY, _iQuality - 1);

            for (int i = 1; i <= 3; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_THUMBNAILS_COLUMNS, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBNAILS_COLUMNS, _iColumns - 1);

            for (int i = 1; i <= 3; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_THUMBNAILS_ROWS, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_THUMBNAILS_ROWS, _iRows - 1);
          }
          return true;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            
            if (iControl == (int)Controls.CONTROL_THUMBNAILS_QUALITY)
            {
              string strLabel = message.Label;
              _iQuality = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedHorizontal = _iQuality;
              //settingsChanged = true;
              ThumbQualityValueChanged();
              SetProperties();
            }

            if (iControl == (int)Controls.CONTROL_THUMBNAILS_COLUMNS)
            {
              string strLabel = message.Label;
              _iColumns = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedHorizontal = _iColumns;
              //settingsChanged = true;
            }

            if (iControl == (int)Controls.CONTROL_THUMBNAILS_ROWS)
            {
              string strLabel = message.Label;
              _iRows = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedHorizontal = _iRows;
              //settingsChanged = true;
            }

            break;
          }
      }
      return base.OnMessage(message);
    }
    
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {

      if (control == btnEnableMusicThumbs)
      {
        SettingsChanged(true);
      }
      if (control == btnDeleteMusicThumbs)
      {
        Utils.DeleteFiles(Thumbs.MusicFolder, String.Format(@"*{0}", Utils.GetThumbExtension()));
        Utils.DeleteFiles(Thumbs.MusicAlbum, String.Format(@"*{0}", Utils.GetThumbExtension()));
        Utils.DeleteFiles(Thumbs.MusicArtists, String.Format(@"*{0}", Utils.GetThumbExtension()));
        Utils.DeleteFiles(Thumbs.MusicGenre, String.Format(@"*{0}", Utils.GetThumbExtension()));
      }
      if (control == btnEnablePicturesThumbs)
      {
        SettingsChanged(true);
      }
      if (control == btnDeletePicturesThumbs)
      {
        Utils.DeleteFiles(Thumbs.Pictures, String.Format(@"*{0}", Utils.GetThumbExtension()));
      }
      if (control == btnEnableVideosThumbs)
      {
        SettingsChanged(true);
      }
      if (control == btnEnableLeaveThumbInFolder)
      {
        SettingsChanged(true);
      }
      if (control == btnDeleteVideosThumbs)
      {
        Utils.DeleteFiles(Thumbs.TVRecorded, String.Format(@"*{0}", Utils.GetThumbExtension()));
        Utils.DeleteFiles(Thumbs.Videos, String.Format(@"*{0}", Utils.GetThumbExtension()));
      }
      if (control == btnClearBlacklistedThumbs)
      {
        ClearBlacklistedThumbs();
      }

      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101005));
      LoadSettings();
      SetProperties();
      _settingsSaved = false;

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();
      base.OnPageDestroy(newWindowId);
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

    private void ThumbQualityValueChanged()
    {
      switch (_iQuality)
      {
        case 1:
          Thumbs.Quality = Thumbs.ThumbQuality.fastest;
          break;
        case 2:
          Thumbs.Quality = Thumbs.ThumbQuality.fast;
          break;
        case 3:
          Thumbs.Quality = Thumbs.ThumbQuality.average;
          break;
        case 4:
          Thumbs.Quality = Thumbs.ThumbQuality.higher;
          break;
        case 5:
          Thumbs.Quality = Thumbs.ThumbQuality.highest;
          break;
      }
    }

    private void SetProperties()
    {
      switch (_iQuality)
      {
        case 1:
          GUIPropertyManager.SetProperty("#thumbResolution", Convert.ToString((int)Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int)Thumbs.ThumbLargeResolution));
          GUIPropertyManager.SetProperty("#thumbCompositing", GUILocalizeStrings.Get(300014)); //High speed
          GUIPropertyManager.SetProperty("#thumbInterpolation", GUILocalizeStrings.Get(300015)); // Nearest neighbor
          GUIPropertyManager.SetProperty("#thumbSmoothing", GUILocalizeStrings.Get(231)); //None
          GUIPropertyManager.SetProperty("#thumbScreen", GUILocalizeStrings.Get(300016)); //Small CRTs
          break;
        case 2:
          GUIPropertyManager.SetProperty("#thumbResolution", Convert.ToString((int)Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int)Thumbs.ThumbLargeResolution));
          GUIPropertyManager.SetProperty("#thumbCompositing", GUILocalizeStrings.Get(300014)); //High speed
          GUIPropertyManager.SetProperty("#thumbInterpolation", GUILocalizeStrings.Get(883)); // Low
          GUIPropertyManager.SetProperty("#thumbSmoothing", GUILocalizeStrings.Get(300014)); //High speed
          GUIPropertyManager.SetProperty("#thumbScreen", GUILocalizeStrings.Get(300017)); //Small wide CRTs, medium CRTs
          break;
        case 3:
          GUIPropertyManager.SetProperty("#thumbResolution", Convert.ToString((int)Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int)Thumbs.ThumbLargeResolution));
          GUIPropertyManager.SetProperty("#thumbCompositing", GUILocalizeStrings.Get(886)); // Default
          GUIPropertyManager.SetProperty("#thumbInterpolation", GUILocalizeStrings.Get(886)); // Default
          GUIPropertyManager.SetProperty("#thumbSmoothing", GUILocalizeStrings.Get(886)); // Default
          GUIPropertyManager.SetProperty("#thumbScreen", GUILocalizeStrings.Get(300018)); //Large wide CRTs, small LCDs
          break;
        case 4:
          GUIPropertyManager.SetProperty("#thumbResolution", Convert.ToString((int)Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int)Thumbs.ThumbLargeResolution));
          GUIPropertyManager.SetProperty("#thumbCompositing", GUILocalizeStrings.Get(300019)); //Assume Linear
          GUIPropertyManager.SetProperty("#thumbInterpolation", GUILocalizeStrings.Get(300020)); //High Quality
          GUIPropertyManager.SetProperty("#thumbSmoothing", GUILocalizeStrings.Get(300020)); //High Quality
          GUIPropertyManager.SetProperty("#thumbScreen", GUILocalizeStrings.Get(300021)); //LCDs, Plasmas
          break;
        case 5:
          GUIPropertyManager.SetProperty("#thumbResolution", Convert.ToString((int)Thumbs.ThumbResolution) + " + " +
                                        Convert.ToString((int)Thumbs.ThumbLargeResolution));
          GUIPropertyManager.SetProperty("#thumbCompositing", GUILocalizeStrings.Get(300020)); //High Quality
          GUIPropertyManager.SetProperty("#thumbInterpolation", GUILocalizeStrings.Get(300022)); //High Quality Bicubic
          GUIPropertyManager.SetProperty("#thumbSmoothing", GUILocalizeStrings.Get(300020)); //High Quality
          GUIPropertyManager.SetProperty("#thumbScreen", GUILocalizeStrings.Get(300023)); //Very large LCDs, Projectors
          break;
      }
    }

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }

    private void ClearBlacklistedThumbs()
    {
      IVideoThumbBlacklist blacklist;
      if (GlobalServiceProvider.IsRegistered<IVideoThumbBlacklist>())
      {
        blacklist = GlobalServiceProvider.Get<IVideoThumbBlacklist>();
      }
      else
      {
        blacklist = new VideoThumbBlacklistDBImpl();
        GlobalServiceProvider.Add<IVideoThumbBlacklist>(blacklist);
      }

      if (blacklist != null)
      {
        blacklist.Clear();
      }
    }

  }
}