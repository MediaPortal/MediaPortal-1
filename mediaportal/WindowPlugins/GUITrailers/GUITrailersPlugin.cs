#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Drawing;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
  public class GUITrailersPlugin : ISetupForm, IShowPlugin
  {
    // Returns the name of the plugin which is shown in the plugin menu
    public string PluginName()
    {
      return "My Trailers";
    }

    // Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
      return "Browse Trailers & clips on Yahoo Movies";
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public string Author()
    {
      return "Zipperzip/sur_uix";
    }

    // show the setup dialog
    public void ShowPlugin()
    {
      GUITrailersSetupForm setup = new GUITrailersSetupForm();
      setup.ShowDialog();
    }

    // Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return true;
    }

    // get ID of windowplugin belonging to this setup
    public int GetWindowId()
    {
      return 5900;
    }

    // Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return true;
    }
    // indicates if a plugin has its own setup screen
    public bool HasSetup()
    {
      return true;
    }

    /// <summary>
    /// If the plugin should have its own button on the main menu of Mediaportal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true  : plugin needs its own button on home
    ///          false : plugin does not need its own button on home</returns>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(5900);
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = "hover_my trailers.png";
      return true;
    }

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

  }
}


