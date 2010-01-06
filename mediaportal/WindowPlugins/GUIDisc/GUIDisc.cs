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

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Services;
using System;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Adds "Play Disc" button to homescreen
  /// </summary>
  [PluginIcons("WindowPlugins.GUIDisc.DVD.gif", "WindowPlugins.GUIDisc.DVDDisabled.gif")]
  public class GUIDisc : GUIInternalWindow, ISetupForm, IShowPlugin
  {
    public GUIDisc()
      : base()
    {
      GetID = (int)Window.WINDOW_DVD;
    }

    public override bool Init()
    {
      return true;
    }

    protected override void OnPageLoad()
    {
      GUIWindowManager.ShowPreviousWindow();

      ISelectDVDHandler selectDVDHandler;
      if (GlobalServiceProvider.IsRegistered<ISelectDVDHandler>())
      {
        selectDVDHandler = GlobalServiceProvider.Get<ISelectDVDHandler>();
      }
      else
      {
        selectDVDHandler = new SelectDVDHandler();
        GlobalServiceProvider.Add<ISelectDVDHandler>(selectDVDHandler);
      }

      string dvdToPlay = selectDVDHandler.ShowSelectDriveDialog(GetID, false);
      if (!String.IsNullOrEmpty(dvdToPlay) && !g_Player.CurrentFile.StartsWith(dvdToPlay) && !g_Player.Playing)
      {
        MediaPortal.Ripper.AutoPlay.ExamineCD(dvdToPlay, true);
      }
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public bool HasSetup()
    {
      return false;
    }

    public string PluginName()
    {
      return "Play Disc Shortcut";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(51);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = @"hover_play disc.png";
      return true;
    }

    public string Author()
    {
      return "-Manfred-";
    }

    public string Description()
    {
      return @"Menu shortcut for direct optical media playback";
    }

    public void ShowPlugin()
    {
      // TODO:  Add GUIVideoFiles.ShowPlugin implementation
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion
  }
}