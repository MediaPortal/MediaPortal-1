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
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;

namespace WindowPlugins.VideoEditor
{
  [PluginIcons("WindowPlugins.VideoEditor.VideoEditor.gif", "WindowPlugins.VideoEditor.VideoEditor_disabled.gif")]
  public class VideoEditorSetup : ISetupForm
  {
    //    int windowID = 170601;

    public VideoEditorSetup()
    {
    }

    #region ISetupForm Member

    public string Author()
    {
      return "kaybe and brutus, skin by Ralph";
    }

    public bool CanEnable()
    {
      return true;
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public string Description()
    {
      return "This plugin is able to cut .mpg, .dvr-ms and .ts files";
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(2090);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = "hover_videoeditor.png";
      return true;
    }

    public int GetWindowId()
    {
      return (int)GUIWindow.Window.WINDOW_VIDEO_EDITOR;
    }

    public bool HasSetup()
    {
      return true;
    }

    public string PluginName()
    {
      return "VideoEditor";
    }

    public void ShowPlugin()
    {
      //System.Windows.Forms.MessageBox.Show("Nothing to configure - just enable and start MP ;)");
      VideoEditorConfiguration config = new VideoEditorConfiguration();
      config.Show();
    }

    #endregion
  }
}
