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
using System.Runtime.InteropServices;
using System.Text;


namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Offers methods to check the current connectivity state of the network interfaces
  /// </summary>
  public class CheckConnections
  {
    [Flags]
    enum ConnectionState : int
    {
      INTERNET_CONNECTION_MODEM = 0x1,
      INTERNET_CONNECTION_LAN = 0x2,
      INTERNET_CONNECTION_PROXY = 0x4,
      INTERNET_RAS_INSTALLED = 0x10,
      INTERNET_CONNECTION_OFFLINE = 0x20,
      INTERNET_CONNECTION_CONFIGURED = 0x40
    }

    private bool _connected = false;
    private ConnectionState _currentState;

    // check connectivity http://www.developerfusion.co.uk/show/5346/
    [DllImport("wininet.dll", CharSet = CharSet.Auto)]
    static extern bool InternetGetConnectedState(ref ConnectionState lpdwFlags, int dwReserved);

    /// <summary>
    /// This method gets the status of the computer's networking devices
    /// </summary>
    /// <returns>true if computer is connected to the internet</returns>
    bool CheckInternetConnection()
    {
      ConnectionState Description = 0;
      string connState = InternetGetConnectedState(ref Description, 0).ToString();
      _connected = Convert.ToBoolean(connState);
      //        Log.Info("AudioscrobblerEngine: check connection - {0}", Description.ToString());
      _currentState = Description;

      return _connected;
    }

    /// <summary>
    /// True if the computer has an active internet connection
    /// </summary>
    public bool Connected
    {
      get
      {
        return CheckInternetConnection();
      }
    }

    /// <summary>
    /// True if there is a proxy configured on this computer
    /// </summary>
    public bool HasProxy
    {
      get
      {
        CheckInternetConnection();
        string flaggedState = _currentState.ToString();
        if (flaggedState.Contains("INTERNET_CONNECTION_PROXY"))
          return true;
        else
          return false;
      }
    }

    /// <summary>
    /// True if the computer is hooked up via LAN interface
    /// </summary>
    public bool IsLAN
    {
      get
      {
        CheckInternetConnection();
        string flaggedState = _currentState.ToString();
        if (flaggedState.Contains("INTERNET_CONNECTION_LAN"))
          return true;
        else
          return false;
      }
    }

    /// <summary>
    /// True if the computer's internet connection is via dial-up
    /// </summary>
    public bool IsDialUp
    {
      get
      {
        CheckInternetConnection();
        string flaggedState = _currentState.ToString();
        if (flaggedState.Contains("INTERNET_CONNECTION_MODEM"))
          return true;
        else
          return false;
      }
    }
  }
}
