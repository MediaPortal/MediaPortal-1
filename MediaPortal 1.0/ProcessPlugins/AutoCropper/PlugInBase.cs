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
using System.Collections.Generic;
using System.Text;

namespace ProcessPlugins.AutoCropper
{
  /// <summary>
  /// Handles the plug part of the autocropper, except Start and Stop
  /// which are left to the AutoCropper class which extends PlugInBase.
  /// </summary>
  public abstract class PlugInBase : MediaPortal.GUI.Library.IPlugin, MediaPortal.GUI.Library.ISetupForm
  {

    public abstract void Stop();
    public abstract void Start();

    public string PluginName()
    {
      return "AutoCropper";
    }

    public string Description()
    {
      return "Automatically removes black bars from letterboxed tv broadcasts";
    }

    public string Author()
    {
      return "Ziphnor";
    }

    public void ShowPlugin()
    {
      AutoCropperConfig form = new AutoCropperConfig("AutoCropper");
      form.ShowDialog();
    }

    public int GetWindowId()
    {
      return 432486;
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = null; strButtonImage = null; strButtonImageFocus = null; strPictureImage = null;

      return false;
    }
  }
}
