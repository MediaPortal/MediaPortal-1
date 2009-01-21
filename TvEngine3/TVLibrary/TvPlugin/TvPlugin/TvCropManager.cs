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

#region usings

using System;
using System.Threading;
using System.Text;
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.Player;

#endregion

namespace TvPlugin
{
  class TvCropManager
  {
    #region Ctor/Dtor

    public TvCropManager()
    {
      g_Player.PlayBackStarted += new g_Player.StartedHandler(g_Player_PlayBackStarted);
      Log.Info("TvCropManager: Started");
    }

    ~TvCropManager()
    {
      Log.Info("TvCropManager: Stopped");
    }

    #endregion

    public static CropSettings CropSettings
    {
      get
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          CropSettings cropSettings = new CropSettings(
                                           xmlreader.GetValueAsInt("tv", "croptop", 0),
                                           xmlreader.GetValueAsInt("tv", "cropbottom", 0),
                                           xmlreader.GetValueAsInt("tv", "cropleft", 0),
                                           xmlreader.GetValueAsInt("tv", "cropright", 0)
                                          );
          return cropSettings;
        }
      }
      set
      {
        CropSettings cropSettings = value;
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValue("tv", "croptop", cropSettings.Top);
          xmlwriter.SetValue("tv", "cropbottom", cropSettings.Bottom);
          xmlwriter.SetValue("tv", "cropleft", cropSettings.Left);
          xmlwriter.SetValue("tv", "cropright", cropSettings.Right);
        }
        Log.Info("TvCropManager.SendCropMessage(): {0}, {1}, {2}, {3}", cropSettings.Top, cropSettings.Bottom, cropSettings.Left, cropSettings.Right);
        GUIWindowManager.SendThreadMessage(new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLANESCENE_CROP, 0, 0, 0, 0, 0, cropSettings));
      }
    }
    /// <summary>
    /// Gets called by g_Player when playback of media has started.
    /// This handles cropping timeshifted TV and recordings.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="filename"></param>
    void g_Player_PlayBackStarted(g_Player.MediaType type, string filename)
    {
      if (type == g_Player.MediaType.TV || type == g_Player.MediaType.Recording)
      {
        try
        {
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          {

            Log.Info("TvCropManager.SendCropMessage(): {0}, {1}, {2}, {3}", TvCropManager.CropSettings.Top, TvCropManager.CropSettings.Bottom, TvCropManager.CropSettings.Left, TvCropManager.CropSettings.Right);
            GUIWindowManager.SendThreadMessage(new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLANESCENE_CROP, 0, 0, 0, 0, 0, TvCropManager.CropSettings));
          }
        }
        catch (Exception)
        {
        }
      }
    }
  }
}
