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
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using ThreadState = System.Threading.ThreadState;

namespace TvControl
{
  /// <summary>
  /// Class which holds the connection with the master tv-server
  /// </summary>
  public class RemoteControl
  {
    #region consts

    public const string InitializedEventName = @"Global\MPTVServiceInitializedEvent";

    private const int MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION = 3000; //msecs
    private const int MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION_INITIAL = 20000; //msecs
    private const int MAX_TCP_TIMEOUT = 1000; //msecs
    private const int REMOTING_PORT = 31456;

    #endregion

    #region delegates / events

    public delegate void RemotingDisconnectedDelegate();

    public delegate void RemotingConnectedDelegate();

    public static event RemotingDisconnectedDelegate OnRemotingDisconnected;
    public static event RemotingConnectedDelegate OnRemotingConnected;

    #endregion

    #region private members

    private static bool _firstFailure = true;
    private static bool _isRemotingConnected = false;
    private static IController _tvControl;
    private static string _hostName = System.Net.Dns.GetHostName();
    private static TcpChannel _callbackChannel = null; // callback channel
    private static bool _useIncreasedTimeoutForInitialConnection = true;

    #endregion

    #region public constructors

    static RemoteControl()
    {
      _useIncreasedTimeoutForInitialConnection = true;
    }

    #endregion

    #region public static methods

    /// <summary>
    /// Registers Ci Menu Callbackhandler in TvPlugin, connects to a server side event
    /// </summary>
    public static void RegisterCiMenuCallbacks(CiMenuCallbackSink sink)
    {
      RefreshRemotingConnectionStatus();
      // Define sink for events
      RemotingConfiguration.RegisterWellKnownServiceType(
        typeof (CiMenuCallbackSink),
        "ServerEvents",
        WellKnownObjectMode.Singleton);

      // Assign the callback from the server to here
      _tvControl.OnCiMenu += new CiMenuCallback(sink.FireCiMenuCallback);
    }

    /// <summary>
    /// Unregisters Ci Menu Callbackhandler in TvPlugin when it's no longer required
    /// </summary>
    public static void UnRegisterCiMenuCallbacks(CiMenuCallbackSink sink)
    {
      // Assign the callback from the server to here
      _tvControl.OnCiMenu -= new CiMenuCallback(sink.FireCiMenuCallback);
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public static void Clear()
    {
      _tvControl = null;
      _isRemotingConnected = false;
    }

    #endregion

    #region private static methods

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void RefreshRemotingConnectionStatus()
    {
      Stopwatch benchClock = Stopwatch.StartNew();
      try
      {
        if (_tvControl == null)
        {
          _isRemotingConnected = false;
          return;
        }

        int timeout = 250;
        if (!_isRemotingConnected && _firstFailure)
        {
          timeout = MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION;
          if (_useIncreasedTimeoutForInitialConnection)
          {
            timeout = MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION_INITIAL;
            _useIncreasedTimeoutForInitialConnection = false;
          }
        }

        int iterations = (timeout / MAX_TCP_TIMEOUT);

        if (iterations < 1)
        {
          iterations = 1;
        }

        int count = 0;

        while (!_isRemotingConnected && count < iterations)
        {
          CheckTcpPort();

          count++;
          if (!_isRemotingConnected)
          {
            Thread.Sleep(10);
          }
        }
      }
      catch (System.Threading.ThreadAbortException)
      {
        benchClock.Stop();
        Log.Info("RemoteControl - timed out after {0} msec", benchClock.ElapsedMilliseconds);
        Thread.ResetAbort();
      }
      catch (Exception e)
      {
        Log.Info("RemoteControl - RefreshRemotingConnectionStatus exception : {0} {1} {2}", e.Message, e.Source,
                 e.StackTrace);
        //ignore
      }
      finally
      {
        InvokeEvents();
      }
    }

    private static void InvokeEvents()
    {
      if (!_isRemotingConnected)
      {
        if (OnRemotingDisconnected != null) //raise event
        {
          if (!_firstFailure)
          {
            Log.Info("RemoteControl - Disconnected ");
            OnRemotingDisconnected();
          }
          _firstFailure = false;
        }
      }
      else
      {
        _firstFailure = true;
        if (OnRemotingConnected != null)
        {
          Log.Info("RemoteControl - Connected");
          OnRemotingConnected();
        }
      }
    }

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
        if (value)
          _firstFailure = true;
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
          _tvControl = null;
          _hostName = value;
        }
      }
    }

    /// <summary>
    /// Registers a remoting channel for allowing callback from server to client
    /// </summary>    
    private static void RegisterChannel()
    {
      try
      {
        if (_callbackChannel == null)
        {
          Log.Debug("RemoteControl: RegisterChannel first called in Domain {0} for thread {1} with id {2}",
                    AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.Name,
                    Thread.CurrentThread.ManagedThreadId);

          //turn off customErrors to receive full exception messages
          RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

          // Creating a custom formatter for a TcpChannel sink chain.
          BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
          provider.TypeFilterLevel = TypeFilterLevel.Full; // needed for passing objref!
          IDictionary channelProperties = new Hashtable();
          // Creating the IDictionary to set the port on the channel instance.
          channelProperties.Add("port", 0); // "0" chooses one available port

          // Pass the properties for the port setting and the server provider in the server chain argument. (Client remains null here.)
          _callbackChannel = new TcpChannel(channelProperties, null, provider);
          ChannelServices.RegisterChannel(_callbackChannel, false);
        }
      }
      catch (RemotingException) {}
      catch (System.Net.Sockets.SocketException) {}
      catch (Exception e)
      {
        Log.Error(e.ToString());
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

    private static bool CheckTcpPortMethod2() // Not used for now
    {
      IPAddress ipa = (IPAddress)Dns.GetHostAddresses(_hostName)[0];

      try
      {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(ipa, REMOTING_PORT);
        if (sock.Connected) // Port is in use and connection is successful
        {
          Log.Debug("RemoteControl: Port is Closed");
          _isRemotingConnected = true;
        }
        sock.Close();

      }
      catch (SocketException ex)
      {
        Log.Debug("RemoteControl: {0}", (ex.Message));
        _isRemotingConnected = false;
      }
      return _isRemotingConnected;
    }

    public static IPAddress GetIP4Addresses()
    {
      IPAddress ipv4Address = null;

      foreach (IPAddress currentIPAddress in Dns.GetHostAddresses(_hostName))
      {
        if (currentIPAddress.AddressFamily == AddressFamily.InterNetwork)
        {
          ipv4Address = currentIPAddress;
          break;
        }
      }

      return ipv4Address;
    }

    private static bool CheckTcpPort()
    {
      Stopwatch benchClock = Stopwatch.StartNew();
      if (string.IsNullOrEmpty(_hostName))
        return false;

      _isRemotingConnected = false;
      Socket sock = null;

      try
      {
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ip = (IPAddress) GetIP4Addresses();
        if (sock != null)
        {
          IPEndPoint ipEndPoint = new IPEndPoint(ip, REMOTING_PORT);

          _isRemotingConnected = false;
          //time out MAX_TCP_TIMEOUT
          CallWithTimeout(ConnectToProxyServers, MAX_TCP_TIMEOUT, sock, ipEndPoint);

          if (sock != null && sock.Connected)
          {
            _isRemotingConnected = true;
          }
        }
      }
      catch (Exception ex)
      {
        _isRemotingConnected = false;
      }
      finally
      {
        benchClock.Stop();
        if (sock != null && sock.Connected)
        {
          Log.Debug("RemoteControl: TCP connect took : {0}", benchClock.ElapsedMilliseconds);
        }
        try
        {
          if (sock != null)
          {
            sock.Shutdown(SocketShutdown.Both);
          }
        }
        catch (Exception ex)
        {
        }
        finally
        {
          if (sock != null)
            sock.Close();
        }
      }
      return _isRemotingConnected;
    }

    private static void CallWithTimeout(Action<Socket, IPEndPoint> action, int timeoutMilliseconds, Socket socket,
                                 IPEndPoint ipendPoint)
    {
      try
      {
        Action wrappedAction = () =>
                                 {
                                   action(socket, ipendPoint);
                                 };

        IAsyncResult result = wrappedAction.BeginInvoke(null, null);

        if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds))
        {
          wrappedAction.EndInvoke(result);
        }

      }
      catch (Exception ex)
      {

      }
    }

    private static void ConnectToProxyServers(Socket testSocket, IPEndPoint ipEndPoint)
    {
      try
      {
        if (testSocket == null || ipEndPoint == null)
          return;

        testSocket.Connect(ipEndPoint);

      }
      catch (Exception ex)
      {

      }
    }

    /// <summary>
    /// returns an the <see cref="T:TvControl.IController"/> interface to the tv server
    /// </summary>
    /// <value>The instance.</value>    
    public static IController Instance
    {
      get
      {
        try
        {
          if (_tvControl != null)
          {
            //only query state if the caller has subcribed to the disconnect/connect events
            if (OnRemotingDisconnected != null || OnRemotingConnected != null)
            {
              RefreshRemotingConnectionStatus();
              if (!_isRemotingConnected)
                return null;
            }

            return _tvControl;
          }

          // register remoting channel
          RegisterChannel();

          _tvControl =
            (IController)
            Activator.GetObject(typeof (IController),
                                String.Format("tcp://{0}:{1}/TvControl", _hostName, REMOTING_PORT));

          //only query state if the caller has subcribed to the disconnect/connect events
          if (OnRemotingDisconnected != null || OnRemotingConnected != null)
          {
            //StartConnectionMonitorThread();

            RefreshRemotingConnectionStatus();
            if (!_isRemotingConnected)
              return null;
          }

          return _tvControl;
        }
        catch (RemotingTimeoutException exrt)
        {
          try
          {
            Log.Error("RemoteControl: Timeout getting server Instance; retrying in 5 seconds - {0}", exrt.Message);
            // maybe the DB wasn't up yet - 2nd try...
            Thread.Sleep(5000);

            // register remoting channel
            RegisterChannel();

            _tvControl =
              (IController)
              Activator.GetObject(typeof (IController),
                                  String.Format("tcp://{0}:{1}/TvControl", _hostName, REMOTING_PORT));

            //only query state if the caller has subcribed to the disconnect/connect events
            if (OnRemotingDisconnected != null || OnRemotingConnected != null)
            {
              RefreshRemotingConnectionStatus();
              if (!_isRemotingConnected)
                return null;
            }

            return _tvControl;
          }
            // didn't help - do nothing
          catch (Exception ex)
          {
            Log.Error("RemoteControl: Error getting server Instance - {0}", ex.Message);
          }
        }
        catch (Exception exg)
        {
          Log.Error("RemoteControl: Error getting server Instance - {0}", exg.Message);
        }

        return null;
      }
    }

    #endregion
  }
}