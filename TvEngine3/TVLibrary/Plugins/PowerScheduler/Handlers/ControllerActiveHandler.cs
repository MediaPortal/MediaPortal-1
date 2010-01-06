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

using System;
using System.Collections.Generic;
using System.Text;
using TvControl;
using TvService;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Handles standby when the TVController is active
  /// </summary>
  public class ControllerActiveHandler : IStandbyHandler
  {
    #region Variables

    private TVController _controller;

    #endregion

    #region Constructor

    public ControllerActiveHandler(IController controller)
    {
      _controller = controller as TVController;
    }

    #endregion

    #region IStandbyHandler Implementation

    public bool DisAllowShutdown
    {
      get
      {
        if (_controller != null && _controller.CanSuspend)
          return false;
        return true;
      }
    }

    public void UserShutdownNow() {}

    public string HandlerName
    {
      get { return "ControllerActiveHandler"; }
    }

    #endregion
  }
}