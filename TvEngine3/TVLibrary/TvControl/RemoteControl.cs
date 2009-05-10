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

// Reverted mantis #1409: using System.Collections;

namespace TvControl
{
  /// <summary>
  /// Class which holds the connection with the master tv-server
  /// </summary>
  public class RemoteControl
  {
    private static bool _channelRegistered = false;
    private static IController _tvControl;
    private static string _hostName = "localhost";
    private static TcpChannel CallbackChannel; // callback channel
    // Reverted mantis #1409: private static uint _timeOut = 45000; // specified in ms (currently all remoting calls are aborted if processing takes more than 45 sec)

    /// <summary>
    /// Registers a remoting channel for allowing callback from server to client
    /// </summary>
    private static void RegisterChannel()
    {
      try {
        // start listening on local port only once!
        if (CallbackChannel == null)
        {
          //turn off customErrors to receive full exception messages
          RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

          // Creating a custom formatter for a TcpChannel sink chain.
          BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
          //SoapServerFormatterSinkProvider provider   = new SoapServerFormatterSinkProvider();
          provider.TypeFilterLevel                   = TypeFilterLevel.Full; // needed for passing objref!
          IDictionary channelProperties              = new Hashtable(); // Creating the IDictionary to set the port on the channel instance.
          channelProperties.Add("port", 31459); // "0" chooses one available port
          channelProperties.Add("exclusiveAddressUse", false);

          // Pass the properties for the port setting and the server provider in the server chain argument. (Client remains null here.)
          CallbackChannel = new TcpChannel(channelProperties, null, provider);
          ChannelServices.RegisterChannel(CallbackChannel, false);
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
    /// Registers Ci Menu Callbackhandler in TvPlugin, connects to a server side event
    /// </summary>
    public static void RegisterCiMenuCallbacks(CiMenuCallbackSink sink)
    {
      // Define sink for events
      RemotingConfiguration.RegisterWellKnownServiceType(
          typeof(CiMenuCallbackSink),
          "ServerEvents",
          WellKnownObjectMode.Singleton);

      // Assign the callback from the server to here
      _tvControl.OnCiMenu += new CiMenuCallback(sink.FireCiMenuCallback);
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
        try
        {
          if (_tvControl != null)
          {
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

          // int card = _tvControl.Cards;
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

            // int card = _tvControl.Cards;
            return _tvControl;
          }
            // didn't help - do nothing
          catch (Exception ex)
          {
            Log.Error("RemoteControl: Error getting server Instance - {0}", ex.Message);
          }
        }
          //   catch (RemotingException exr)
          //   {
          //     Log.Error("RemoteControl: Error getting server Instance - {0}", exr.Message);
          //   }
        catch (Exception exg)
        {
          Log.Error("RemoteControl: Error getting server Instance - {0}", exg.Message);
        }

        return _tvControl;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is connected with the tv server
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is connected; otherwise, <c>false</c>.
    /// </value>
    public static bool IsConnected
    {
      get
      {
        try
        {
          int id = Instance.Cards;

          return id >= 0;
        }
        catch (Exception)
        {
          Log.Error("RemoteControl - Error checking connection state");
        }
        return false;
      }
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public static void Clear()
    {
      _tvControl = null;
    }
  }
}