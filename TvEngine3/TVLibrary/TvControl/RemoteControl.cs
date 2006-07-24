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
  public class RemoteControl
  {
    static IController _tvControl;
    static string _hostName = "localhost";

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
    static public void Clear()
    {
      _tvControl = null;
    }
  }
}
