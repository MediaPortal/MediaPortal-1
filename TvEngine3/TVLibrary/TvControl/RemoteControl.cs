using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using TvControl;
using TvLibrary.Log;
namespace TvControl
{
  /// <summary>
  /// Class which holds the connection with the master tv-server
  /// </summary>
  public class RemoteControl
  {
    static IController _tvControl;
    static string _hostName = "localhost";

    /// <summary>
    /// Gets or sets the name the hostname of the master tv-server.
    /// </summary>
    /// <value>The name of the host.</value>
    static public string HostName
    {
      get
      {
        return _hostName;
      }
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
    static public IController Instance
    {
      get
      {
        try
        {
          if (_tvControl != null) return _tvControl;
          _tvControl = (IController)Activator.GetObject(typeof(IController), String.Format("tcp://{0}:31456/TvControl", _hostName));
          int card=_tvControl.Cards;
          return _tvControl;
        }
        catch (Exception)
        {
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
    static public bool IsConnected
    {
      get
      {
        try
        {
          int cards = RemoteControl.Instance.Cards;
          return true;
        }
        catch (Exception)
        {
        }
        return false;
      }
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    static public void Clear()
    {
      _tvControl = null;
    }
  }
}
