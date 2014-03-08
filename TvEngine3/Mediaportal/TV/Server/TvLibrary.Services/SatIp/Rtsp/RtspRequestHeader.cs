using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediaportal.TV.Server.TVLibrary.SatIp.Rtsp
{
  class RtspRequestHeader
  {
    private int _CSeq;
    private string _sessionId;
    private RtspMethod _method;
    private int _rtpPort;
    private int _rtcpPort;

    /// <summary>
    /// Get the CSeq.
    /// </summary>
    public int CSeq
    {
      get
      {
        return _CSeq;
      }
    }

    /// <summary>
    /// Get the session id
    /// </summary>
    public string sessionId
    {
      get
      {
        return _sessionId;
      }
    }

    /// <summary>
    /// Get the request method (SETUP, PLAY...)
    /// </summary>
    public RtspMethod method
    {
      get
      {
        return _method;
      }
    }

    /// <summary>
    /// Get the rtp Port
    /// </summary>
    public int rtpPort
    {
      get
      {
        return _rtpPort;
      }
    }

    /// <summary>
    /// Get the rtspPort
    /// </summary>
    public int rtcpPort
    {
      get
      {
        return _rtcpPort;
      }
    }
    
    public RtspRequestHeader(string message)
    {
      // Details: CSeq, sessionId
      string[] lines = message.Split('\n');
      foreach (string line in lines)
      {
        if (line.IndexOf("CSeq", StringComparison.OrdinalIgnoreCase) >= 0)
        {
          _CSeq = int.Parse(line.Split(':')[1].Trim());
        }
        if (line.IndexOf("Session", StringComparison.OrdinalIgnoreCase) >= 0)
        {
          _sessionId = line.Split(':')[1].Trim();
        }
        if (line.IndexOf("Transport", StringComparison.OrdinalIgnoreCase) >= 0)
        {
          _rtpPort = int.Parse(line.Split('=')[1].Trim().Split('-')[0]);
          _rtcpPort = int.Parse(line.Split('=')[1].Trim().Split('-')[1]);
        }
      }
    }
  }
}
