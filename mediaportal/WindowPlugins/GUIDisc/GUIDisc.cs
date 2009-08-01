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
      GetID = (int) Window.WINDOW_DVD;
    }

    public override bool Init()
    {
      return true;
    }

    protected override void OnPageLoad()
    {
      string driveLetter = null;
      GUIWindowManager.ShowPreviousWindow();
      
      System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
      foreach (System.IO.DriveInfo drive in drives)
      {
        driveLetter = String.Format("{0}", drive.RootDirectory);
        if (drive.DriveType == System.IO.DriveType.CDRom && System.IO.Directory.Exists(driveLetter))
        {
          if (g_Player.CurrentFile.StartsWith(driveLetter) && g_Player.Playing)
            return;          
          break;          
        }
      }      
      MediaPortal.Ripper.AutoPlay.ExamineCD(driveLetter, true);
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
      strButtonText = GUILocalizeStrings.Get(208);
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