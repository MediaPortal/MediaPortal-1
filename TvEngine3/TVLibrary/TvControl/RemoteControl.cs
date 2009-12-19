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
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvControl
{
  /// <summary>
  /// Class which holds the connection with the master tv-server
  /// </summary>
  public class RemoteControl
  {
    #region consts

    private const int MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION = 3; //seconds   
    private const int MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION_INITIAL = 20; //seconds

    #endregion

    #region delegates / events

    public delegate void RemotingDisconnectedDelegate();
    public delegate void RemotingConnectedDelegate();

    private delegate bool SynchIsConnectedDelegate();
    private delegate bool AsynchIsConnectedDelegate();

    public static event RemotingDisconnectedDelegate OnRemotingDisconnected;
    public static event RemotingConnectedDelegate OnRemotingConnected;

    #endregion

    #region private members

    private static bool _doingRefreshRemotingConnectionStatusASynch = false;
    private static bool _firstFailure = true;
    private static bool _isRemotingConnected = false;
    private static IController _tvControl;
    private static string _hostName = System.Net.Dns.GetHostName();
    private static TcpChannel _callbackChannel = null; // callback channel
    private static bool _useIncreasedTimeoutForInitialConnection = true;

    // Reverted mantis #1409: private static uint _timeOut = 45000; // specified in ms (currently all remoting calls are aborted if processing takes more than 45 sec)

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
      RefreshRemotingConnectionStatusASynch();
      // Define sink for events
      RemotingConfiguration.RegisterWellKnownServiceType(
          typeof(CiMenuCallbackSink),
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

    [OneWayAttribute]
    private static void IsConnectedAsyncCallBack(IAsyncResult ar)
    {
      IsRemotingConnectedDelegate rem = (IsRemotingConnectedDelegate)((AsyncResult)ar).AsyncDelegate;
    }

    private static bool IsRemotingConnected()
    {
      try
      {        
        return _tvControl.Cards >= 0;
      }
      catch (Exception)
      {        
      }
      return false;
    }

    // The delegate must have the same signature as the method
    // you want to call asynchronously.    
    public delegate bool IsRemotingConnectedDelegate();

    private static void CallRemotingAsynch(int timeout)
    {
      // Create the delegate.
      IsRemotingConnectedDelegate dlgt = new IsRemotingConnectedDelegate(IsRemotingConnected);

      // Initiate the asychronous call.
      AsyncCallback remoteCallback = new AsyncCallback(IsConnectedAsyncCallBack);
      IAsyncResult ar = dlgt.BeginInvoke(remoteCallback, null);

      Thread.Sleep(0);

      // Wait for the WaitHandle to become signaled.
      if (!ar.AsyncWaitHandle.WaitOne(timeout))
      {
        _isRemotingConnected = false;
      }
      else
      {
        _isRemotingConnected = dlgt.EndInvoke(ar);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void RefreshRemotingConnectionStatusASynch()
    {
      try
      {
        if (_doingRefreshRemotingConnectionStatusASynch)
          return;

        _doingRefreshRemotingConnectionStatusASynch = true;

        if (_tvControl == null)
        {
          _isRemotingConnected = false;
          return;
        }

        int timeout = 250;
        if (!_isRemotingConnected && _firstFailure)
        {
          timeout = MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION * 1000;
          if (_useIncreasedTimeoutForInitialConnection)
          {
            timeout = MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION_INITIAL * 1000;
            _useIncreasedTimeoutForInitialConnection = false;
          }          
        }

        CallRemotingAsynch(timeout);
        // TODO: cancel the callbacks, since they are too late.

        if (!_isRemotingConnected)
        {          
          if (OnRemotingDisconnected != null) //raise event
          {
            if (!_firstFailure)
            {
              Log.Info("RemoteControl - Disconnected");
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
      catch
      {
        //ignore
      }
      finally
      {
        _doingRefreshRemotingConnectionStatusASynch = false;
      }
    }
    #endregion

    #region public properties

    public static bool IsConnected
    {
      get { return IsRemotingConnected(); }
    }

    public static bool UseIncreasedTimeoutForInitialConnection
    {
      get { return _useIncreasedTimeoutForInitialConnection; }
      set
      {
        _useIncreasedTimeoutForInitialConnection = value;
        if(value)
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
          Log.Debug("RemoteControl: RegisterChannel first called in Domain {0} for thread {1} with id {2}", AppDomain.CurrentDomain.FriendlyName, Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);

          //turn off customErrors to receive full exception messages
          RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

          // Creating a custom formatter for a TcpChannel sink chain.
          BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
          provider.TypeFilterLevel = TypeFilterLevel.Full; // needed for passing objref!
          IDictionary channelProperties = new Hashtable(); // Creating the IDictionary to set the port on the channel instance.
          channelProperties.Add("port", 0);         // "0" chooses one available port

          // Pass the properties for the port setting and the server provider in the server chain argument. (Client remains null here.)
          _callbackChannel = new TcpChannel(channelProperties, null, provider);
          ChannelServices.RegisterChannel(_callbackChannel, false);
        }
      }
      catch (RemotingException) { }
      catch (System.Net.Sockets.SocketException) { }
      catch (Exception e)
      {
        Log.Error(e.ToString());
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
        // System.Diagnostics.Debugger.Launch();                
        try
        {
          if (_tvControl != null)
          {
            //only query state if the caller has subcribed to the disconnect/connect events
            if (OnRemotingDisconnected != null || OnRemotingConnected != null)
            {
              RefreshRemotingConnectionStatusASynch();
              if (!_isRemotingConnected)
                return null;
            }

            return _tvControl;
          }

          // register remoting channel          
          RegisterChannel();

          _tvControl = (IController) Activator.GetObject(typeof(IController), String.Format("tcp://{0}:31456/TvControl", _hostName));

          //only query state if the caller has subcribed to the disconnect/connect events
          if (OnRemotingDisconnected != null || OnRemotingConnected != null)
          {
            RefreshRemotingConnectionStatusASynch();
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

            _tvControl = (IController) Activator.GetObject(typeof(IController), String.Format("tcp://{0}:31456/TvControl", _hostName));

            //only query state if the caller has subcribed to the disconnect/connect events
            if (OnRemotingDisconnected != null || OnRemotingConnected != null)
            {
              RefreshRemotingConnectionStatusASynch();
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