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

#region Usings

using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

#endregion

namespace MediaPortal.Plugins.Process
{
  [PluginIcons("MediaPortal.Plugins.Process.PowerScheduler.gif",
    "MediaPortal.Plugins.Process.PowerScheduler_disabled.gif")]
  public class PowerSchedulerClientPlugin : IPlugin, ISetupForm, IPluginReceiver
  {
    #region Variables

    private PowerScheduler _powerScheduler;

    #endregion

    #region Ctor

    public PowerSchedulerClientPlugin()
    {
      _powerScheduler = new PowerScheduler();
    }

    #endregion

    #region IPlugin implementation

    public void Start()
    {
      _powerScheduler.Start();
    }

    public void Stop()
    {
      _powerScheduler.Stop();
    }

    #endregion

    #region ISetupForm implementation

    public string Author()
    {
      return "micheloe";
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
      return "Enables standby/wakeup support for MP together with TVEngine3";
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public int GetWindowId()
    {
      return 0;
    }

    public bool HasSetup()
    {
      return true;
    }

    public string PluginName()
    {
      return "PowerScheduler client plugin";
    }

    public void ShowPlugin()
    {
      Form f = new PowerSchedulerClientSetup();
      f.Show();
    }

    #endregion

    #region IPluginReceiver implementation

    public bool WndProc(ref Message msg)
    {
      return _powerScheduler.WndProc(ref msg);
    }

    #endregion
  }
}