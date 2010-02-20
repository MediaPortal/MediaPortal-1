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

using System;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Services;

namespace MediaPortal.MusicShareWatcher
{
  /// <summary>
  /// This is a Process Plugin for the Music Share Watcher
  /// </summary>
  [PluginIcons("ProcessPlugins.MusicShareWatcher.MusicShareWatcher.gif",
    "ProcessPlugins.MusicShareWatcher.MusicShareWatcher_deactivated.gif")]
  public class MusicShareWatcherPlugin : IPluginReceiver, ISetupForm
  {
    private const string _version = "0.3";
    private bool _monitor = false;
    private static MusicShareWatcherHelper watcher = null;

    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_APMSUSPEND = 0x0004;
    private const int PBT_APMRESUMECRITICAL = 0x0006;
    private const int PBT_APMRESUMESUSPEND = 0x0007;
    private const int PBT_APMRESUMESTANDBY = 0x0008;
    private const int PBT_APMRESUMEAUTOMATIC = 0x0012;

    private bool _suspended = false;

    public MusicShareWatcherPlugin() {}

    #region Interface IPluginReceiver

    public void Start()
    {
      using (Settings xmlreader = new MPSettings())
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
      {
        Log.Info("MusicShareWatcher Plugin {0} stopping.", _version);
      }
      return;
    }

    public bool WndProc(ref Message msg)
    {
      try
      {
        if (msg.Msg == WM_POWERBROADCAST)
        {
          switch (msg.WParam.ToInt32())
          {
            case PBT_APMSUSPEND:
              _suspended = true;
              break;

            case PBT_APMRESUMECRITICAL:
            case PBT_APMRESUMESUSPEND:
            case PBT_APMRESUMESTANDBY:
            case PBT_APMRESUMEAUTOMATIC:
              OnResume();
              break;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return false; // false = all other processes will handle the msg
    }

    #endregion

    #region Private Methods

    //called when windows wakes up again
    private static object syncResume = new object();

    private void OnResume()
    {
      lock (syncResume)
      {
        if (!_suspended)
        {
          return;
        }

        if (watcher != null)
        {
          Log.Info(LogType.MusicShareWatcher,
                   "Windows has resumed from standby/hibernate mode: Re-enabling file system watcher");
          watcher.StartMonitor();
        }

        _suspended = false;
      }
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "Monitors changes to the Music Shares and updates the Database accordingly.";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = string.Empty;
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = string.Empty;
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

    public void ShowPlugin() {}

    #endregion
  }
}