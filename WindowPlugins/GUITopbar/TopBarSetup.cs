#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace MediaPortal.Topbar
{
  /// <summary>
  /// Displays a bar which holds basic playback control elements
  /// </summary>
  [PluginIcons("WindowPlugins.GUITopbar.Topbar.gif", "WindowPlugins.GUITopbar.Topbar_disabled.gif")]
  public class TopBarSetup : ISetupForm, IShowPlugin
  {
    public TopBarSetup()
    {
    }

    #region ISetupForm Members

    public bool HasSetup()
    {
      return true;
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public string Description()
    {
      return "Navigation bar for music, video, etc. for all MediaPortal screens";
    }

    public int GetWindowId()
    {
      return (int)GUIWindow.Window.WINDOW_TOPBAR;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = "";
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return false;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string PluginName()
    {
      return "Topbar";
    }

    public void ShowPlugin()
    {
      System.Windows.Forms.Form setup = new TopBarSetupForm();
      setup.ShowDialog();
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;  // not relevant, see CanShowInMenu()
    }

    public bool CanShowInMenu()
    {
      return false;
    }

    #endregion
  }
}
