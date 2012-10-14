#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Remoting;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVControl
{
  /// <summary>
  /// Class which holds the connection with the master tv-server
  /// </summary>
  public class RemoteControl
  {
    #region consts

    public const string InitializedEventName = @"Global\MPTVServiceInitializedEvent";
    private const int MAX_TCP_TIMEOUT = 1000; //msecs
    private const int REMOTING_PORT = 8000;

    #endregion

    #region private members

    private static bool _isRemotingConnected;    
    private static string _hostName = System.Net.Dns.GetHostName();
    private static bool _useIncreasedTimeoutForInitialConnection = true;

    #endregion

    #region public constructors

    static RemoteControl()
    {
      _useIncreasedTimeoutForInitialConnection = true;
    }

    #endregion

    #region public static methods


    #endregion

    

    #region public properties

    public static bool IsConnected
    {
      get { return CheckTcpPort(); }
    }

    public static bool UseIncreasedTimeoutForInitialConnection
    {
      get { return _useIncreasedTimeoutForInitialConnection; }
      set
      {
        _useIncreasedTimeoutForInitialConnection = value;
        //if (value)
          //_firstFailure = true;
      }
    }

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
          _hostName = value;
        }
      }
    }

    

    private static void ConnectCallback(System.IAsyncResult ar)
    {
      TcpClient tcpClient = (TcpClient)ar.AsyncState;
      try
      {
        tcpClient.EndConnect(ar);
      }
      catch (Exception)
      {
        // EndConnect will throw an exception here if the connection has failed
        // so we have to catch it or MP will exit with unhandled exception
      }
      finally
      {
        tcpClient.Close();
      }
    }

    private static bool CheckTcpPort()
    {
      Stopwatch benchClock = Stopwatch.StartNew();

      TcpClient tcpClient = new TcpClient();

      try
      {
        IAsyncResult result = tcpClient.BeginConnect(
          _hostName,
          REMOTING_PORT, ConnectCallback,
          tcpClient);

        _isRemotingConnected = result.AsyncWaitHandle.WaitOne(MAX_TCP_TIMEOUT, true);
        return _isRemotingConnected;
      }
      catch (Exception)
      {
        return false;
      }
      finally
      {
        benchClock.Stop();
        Log.Debug("TCP connect took : {0}", benchClock.ElapsedMilliseconds);
      }
    }

   

    #endregion
  }
}