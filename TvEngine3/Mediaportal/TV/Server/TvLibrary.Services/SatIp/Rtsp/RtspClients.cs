using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Mediaportal.TV.Server.TVLibrary.SatIp.Rtsp
{
  class RtspClients
  {
    private static Int32 counter = 0;
    private Int32 id;
    private string _ip;
    private int _rtpClientPort;
    private int _rtcpClientPort;
    private int _rtpServerPort;
    private int _rtcpServerPort;
    private int _streamId;

    //?src=1&fe=1&freq=12402&pol=v&msys=dvbs&sr=27500&fec=34&pids=0,16
    private int _src;
    private double _freq;
    private string _pol;
    private string _msys;
    private int _sr;
    private int _fec;
    private int _sessionId;
    private ArrayList _pids = new ArrayList();

    /// <summary>
    /// Get the client id.
    /// </summary>
    public int clientId
    {
      get
      {
        return id;
      }
    }

    /// <summary>
    /// Get the session id.
    /// </summary>
    public int sessionId
    {
      get
      {
        return _sessionId;
      }
    }

    /// <summary>
    /// Get/Set the stream id.
    /// </summary>
    public int streamId
    {
      get
      {
        return _streamId;
      }
      set
      {
        _streamId = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public int src
    {
      get
      {
        return _src;
      }
      set
      {
        _src = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public double freq
    {
      get
      {
        return _freq;
      }
      set
      {
        _freq = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public string pol
    {
      get
      {
        return _pol;
      }
      set
      {
        _pol = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public string msys
    {
      get
      {
        return _msys;
      }
      set
      {
        _msys = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public Int32 sr
    {
      get
      {
        return _sr;
      }
      set
      {
        _sr = value;
      }
    }

    /// <summary>
    /// Get the client id.
    /// </summary>
    public Int32 fec
    {
      get
      {
        return _fec;
      }
      set
      {
        _fec = value;
      }
    }

    /// <summary>
    /// Get the pids.
    /// </summary>
    public ArrayList pids
    {
      get
      {
        return _pids;
      }
    }

    /// <summary>
    /// Get the client rtp port.
    /// </summary>
    public int rtpClientPort
    {
      get
      {
        return _rtpClientPort;
      }
      set
      {
        _rtpClientPort = value;
      }
    }

    /// <summary>
    /// Get the client rtcp port.
    /// </summary>
    public int rtcpClientPort
    {
      get
      {
        return _rtcpClientPort;
      }
      set
      {
        _rtcpClientPort = value;
      }
    }

    /// <summary>
    /// Get the server rtp port.
    /// </summary>
    public int rtpServerPort
    {
      get
      {
        return _rtpServerPort;
      }
      set
      {
        _rtpServerPort = value;
      }
    }

    /// <summary>
    /// Get the server rtcp port.
    /// </summary>
    public int rtcpServerPort
    {
      get
      {
        return _rtcpServerPort;
      }
      set
      {
        _rtcpServerPort = value;
      }
    }

    /// <summary>
    /// Get the client ip.
    /// </summary>
    public string ip
    {
      get
      {
        return _ip;
      }
      set
      {
        _ip = value;
      }
    }
    
    public RtspClients()
    {
      lock (this)
      {
        counter++;
        id = counter;

        string sessionId = string.Empty;
        Random rand = new Random();
        for (int ctr = 0; ctr <= 7; ctr++)
          sessionId += rand.Next(0,10).ToString();
        _sessionId = int.Parse(sessionId);
      }
    }

    public void addPid(int value)
    {
      _pids.Add(value); 
    }

    public void delPid(int value)
    {
      _pids.Remove(value);
    }
  }
}
