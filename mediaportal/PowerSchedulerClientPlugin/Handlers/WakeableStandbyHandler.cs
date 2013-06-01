#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

using System.Collections;
using MediaPortal.GUI.Library;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace MediaPortal.Plugins.Process.Handlers
{
  /// <summary>
  /// Standby handler for Wakeable plugins
  /// </summary>
  class WakeableStandbyHandler : IStandbyHandler
  {
    #region Variables

    private string _handlerName;

    #endregion

    #region IStandbyHandler implementation

    /// <summary>
    /// DisAllowShutdown
    /// </summary>
    public bool DisAllowShutdown
    {
      get
      {
        _handlerName = "WakeableStandbyPlugins";

        // Check all found IWakeable plugins
        ArrayList wakeables = PluginManager.WakeablePlugins;
        foreach (IWakeable wakeable in wakeables)
        {
          if (wakeable.DisallowShutdown())
          {
            _handlerName = wakeable.PluginName();
            return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// UserShutdownNow()
    /// </summary>
    public void UserShutdownNow() {}

    /// <summary>
    /// HandlerName
    /// </summary>
    public string HandlerName
    {
      get { return _handlerName; }
    }

    #endregion
  }
}
