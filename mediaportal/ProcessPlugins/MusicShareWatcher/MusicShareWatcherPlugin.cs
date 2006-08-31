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
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.MusicShareWatcher
{
  /// <summary>
  /// This is a Process Plugin for the Music Share Watcher
  /// </summary>
  public class MusicShareWatcherPlugin : IPlugin, ISetupForm
  {
    private const string _version = "0.2";
    private bool _monitor = false;
    private static MusicShareWatcherHelper watcher = null;

    public MusicShareWatcherPlugin()
    {
    }

    public void Start()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
      {
        _monitor = xmlreader.GetValueAsBool("musicfiles", "monitorShares", false);
      }

      if (_monitor)
      {
        Log.Info("MusicShareWatcher Plugin {0} starting.", _version);
        watcher = new MusicShareWatcherHelper();
        watcher.SetMonitoring(true);
        watcher.StartMonitor();
        Log.Info("MusicShareWatcher Plugin now monitoring the shares.");
      }
    }

    public void Stop()
    {
      if (_monitor)
        Log.Info("MusicShareWatcher Plugin {0} stopping.", _version);
      return;
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "Starts the Music Share Watcher inside Mediaportal";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = String.Empty;
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
      return false;
    }

    public string Author()
    {
      return "hwahrmann";
    }

    public string PluginName()
    {
      return "Music Share Watcher";
    }

    public bool HasSetup()
    {
      return false;
    }

    public void ShowPlugin()
    {
    }

    #endregion
  }
}
