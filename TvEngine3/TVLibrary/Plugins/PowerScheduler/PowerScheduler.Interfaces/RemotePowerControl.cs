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

#region Usings

using System;

#endregion

namespace TvEngine.PowerScheduler.Interfaces
{
  /// <summary>
  /// Class which holds the connection with powerscheduler in the tvengine
  /// </summary>
  public class RemotePowerControl
  {
    #region Variables

    /// <summary>
    /// IPowerController singleton
    /// </summary>
    private static IPowerController _powerController;

    #endregion

    #region Public Properties

    /// <summary>
    /// Returns the one and only instance of the IPowerController (PowerScheduler)
    /// </summary>
    public static IPowerController Instance
    {
      get
      {
        try
        {
          if (_powerController != null)
          {
            return _powerController;
          }
          _powerController =
            (IPowerController) Activator.GetObject(typeof (IPowerController), "http://localhost:31457/PowerControl");
          bool connected = _powerController.IsConnected;
          return _powerController;
        }
        catch (Exception)
        {
          return _powerController;
        }
      }
    }

    /// <summary>
    /// Is the RemotePowerControl client connected to the server?
    /// </summary>
    public static bool Isconnected
    {
      get
      {
        try
        {
          if (_powerController == null)
          {
            return false;
          }
          return _powerController.IsConnected;
        }
        catch (Exception)
        {
          return false;
        }
      }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Reinitializes the IPowercontroller singleton
    /// </summary>
    public static void Clear()
    {
      _powerController = null;
    }

    #endregion
  }
}