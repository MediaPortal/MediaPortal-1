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
using System.Runtime.Remoting;
using System.Threading;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
// Reverted mantis #1409: using System.Collections;

namespace TvControl
{
  /// <summary>
  /// Class which holds the connection with the master tv-server
  /// </summary>
  public class RemoteControl
  {
    #region consts

    private const int MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION = 5; //seconds
    private const int MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION_WOL = 20; //seconds

    #endregion

    #region delegates / events

    public delegate void RemotingDisconnectedDelegate();
    public delegate void RemotingConnectedDelegate(bool recovered);
    private delegate bool SynchIsConnectedDelegate();
    private delegate bool AsynchIsConnectedDelegate();

    public static event RemotingDisconnectedDelegate OnRemotingDisconnected;
    public static event RemotingConnectedDelegate OnRemotingConnected;

    #endregion

    #region private members
    
    private static bool _firstFailure = true;
    private static ManualResetEvent _isRemotingConnectedResetEvt;
    private static bool _isRemotingConnected = false;
    private static IController _tvControl;
    private static string _hostName = System.Net.Dns.GetHostName();
    private static TcpChannel _callbackChannel; // callback channel
    private static bool _WOL = false;
    

    // Reverted mantis #1409: private static uint _timeOut = 45000; // specified in ms (currently all remoting calls are aborted if processing takes more than 45 sec)
    
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
    /// returns an the <see cref="T:TvControl.IController"/> interface to the tv server
    /// </summary>
    /// <value>The instance.</value>
    public static IController Instance
    {
      get
      {
       // System.Diagnostics.Debugger.Launch();                
        bool conn = false;
        try
        {
          if (_tvControl != null)
          {            
            RefreshRemotingConnectionStatusASynch();
           
            if (!_isRemotingConnected)
              return null;

            return _tvControl;
          }

          // Reverted mantis #1409: 
          //#if !DEBUG //only use timeouts when (build=release)
          //          try
          //          {
          //            IDictionary t = new Hashtable();
          //            t.Add("timeout", _timeOut);
          //            TcpClientChannel clientChannel = new TcpClientChannel(t, null);
          //            ChannelServices.RegisterChannel(clientChannel);
          //          }
          //          catch (Exception e)
          //          {
          //            //Log.Debug("RemoteControl: could not set timeout on Remoting framework - {0}", e.Message);
          //            //ignore
          //          }
          //#endif    

          // register remoting channel          
          RegisterChannel();
                    
          _tvControl =
            (IController)
            Activator.GetObject(typeof (IController), String.Format("tcp://{0}:31456/TvControl", _hostName));                    

          RefreshRemotingConnectionStatusASynch();
          if (!_isRemotingConnected)
            return null;          
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
              Activator.GetObject(typeof (IController), String.Format("tcp://{0}:31456/TvControl", _hostName));

            RefreshRemotingConnectionStatusASynch();
            if (!_isRemotingConnected)
              return null;    
            // int card = _tvControl.Cards;
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

        return _tvControl;
      }
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
      AsynchIsConnectedDelegate rem = (AsynchIsConnectedDelegate)((AsyncResult)ar).AsyncDelegate;      
      return;
    }    

    private static void RefreshRemotingConnectionStatusASynch()
    {               
      try
      {
        if (_tvControl == null)
        {
          _isRemotingConnected = false;
          return;
        }

        bool oldState = _isRemotingConnected;

        _isRemotingConnectedResetEvt = new ManualResetEvent(false); //block

        SynchIsConnectedDelegate remoteSyncDel = new SynchIsConnectedDelegate(IsRemotingConnected);
        AsyncCallback remoteCallback = new AsyncCallback(IsConnectedAsyncCallBack);
        AsynchIsConnectedDelegate remoteDel = new AsynchIsConnectedDelegate(IsRemotingConnected);
        
        IAsyncResult remAr = remoteDel.BeginInvoke(remoteCallback, null);

        int timeout = 250;
        if (_firstFailure)
        {
          if (_WOL)
          {
            //have a slightly longer timeout period for WOL, if the server has just awoken it should be given a longer grace.
            timeout = MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION_WOL;
            _WOL = false;
          }
          else
          {
            timeout = MAX_WAIT_FOR_SERVER_REMOTING_CONNECTION * 1000; ;  
          }          
        }

        _isRemotingConnected = remAr.AsyncWaitHandle.WaitOne(timeout);

        // TODO: cancel the callbacks, since they are too late.

        if (!_isRemotingConnected)
        {
          _firstFailure = false;
          if (OnRemotingDisconnected != null) //raise event
          {
            Log.Info("RemoteControl - Disconnected");
            OnRemotingDisconnected();
          }                    
        }
        else
        {
          if (OnRemotingConnected != null)
          {
            _firstFailure = true;
            bool recovered = (!oldState);
            if (recovered)
            {
              Log.Info("RemoteControl - Reconnected");
            }
            OnRemotingConnected(recovered);
          }          
        }
      }
      catch
      {
        //ignore
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

    private static bool IsRemotingConnected()
    {      
      try
      {
        int id = _tvControl.Cards;
        return id >= 0;
      }
      catch (Exception)
      {
        //ignore
      }
      return false;
    }

    #endregion

    #region public properties

    public static bool IsConnected
    {
      get { return IsRemotingConnected(); }
    }

    public static bool WakeOnLAN
    {
      get { return _WOL; }
      set { _WOL = value; }
    }

    #endregion
  }
}