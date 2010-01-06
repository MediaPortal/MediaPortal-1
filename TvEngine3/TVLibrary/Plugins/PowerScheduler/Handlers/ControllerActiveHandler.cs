#region Copyright (C) 2007-2009 Team MediaPortal

/* 
 *	Copyright (C) 2007-2009 Team MediaPortal
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