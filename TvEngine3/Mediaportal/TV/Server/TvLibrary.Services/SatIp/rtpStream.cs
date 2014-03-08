using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Mediaportal.TV.Server.TVLibrary.SatIp
{
  class rtpStream
  {
    private bool _stopStream;
    private Thread _thread;

    /// <summary>
    /// Get/Set the stream stop flag.
    /// </summary>
    public bool stopStream
    {
      get
      {
        return _stopStream;
      }
      set
      {
        _stopStream = value;
      }
    }

    /// <summary>
    /// Get/Set the thread.
    /// </summary>
    public Thread thread
    {
      get
      {
        return _thread;
      }
      set
      {
        _thread = value;
      }
    }

    public rtpStream(Thread thread, bool stopStream = false)
    {
      _stopStream = stopStream;
      _thread = thread;
    }
  }
}
