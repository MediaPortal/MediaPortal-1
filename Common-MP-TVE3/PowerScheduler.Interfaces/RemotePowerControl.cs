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

    private static string _hostName = "localhost";

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the name the hostname of the master tv-server.
    /// </summary>
    /// <value>The name of the host.</value>
    public static string HostName
    {
      get { return _hostName; }
      set
      {
        if (_hostName != value)
        {
          _powerController = null;
          _hostName = value;
        }
      }
    }

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
            (IPowerController)
            Activator.GetObject(typeof (IPowerController), String.Format("http://{0}:31457/PowerControl", _hostName));
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