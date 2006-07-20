using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using TvLibrary.Log;

namespace TVLibrary.Streaming
{
  public class RtspStreaming
  {
    #region imports
    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    protected static extern void StreamSetup();

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    protected static extern void StreamRun();

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    protected static extern void StreamAddTs(string streamName, string fileName);

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    protected static extern void StreamAddMpg(string streamName, string fileName);

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    protected static extern void StreamRemove(string streamName);
    #endregion

    #region variables
    bool _running = false;
    bool _initialized = false;
    long _streamIndex = 0;
    Dictionary<string, string> _streams;
    #endregion

    #region ctor
    public RtspStreaming()
    {
      try
      {
        StreamSetup();
        _initialized = true;
        _streams = new Dictionary<string, string>();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
    #endregion

    #region public members
    public void Start()
    {
      if (_initialized == false) return;
      if (_running) return;
      _streamIndex = 100;
      Log.WriteFile("RTSP: start streamer");
      _running = true;
      Thread thread = new Thread(new ThreadStart(workerThread));
      thread.SetApartmentState(ApartmentState.STA);
      thread.IsBackground = true;
      thread.Name = "RTSP Streaming thread";
      thread.Start();
    }


    public void Stop()
    {
      if (_initialized == false) return;
      Log.WriteFile("RTSP: stop streamer");
      _running = false;
    }

    public void Add(string streamName, string fileName)
    {
      if (_initialized == false) return;
      if (_streams.ContainsKey(streamName))
      {
        Log.WriteFile("RTSP: add stream {0} already added");
        return;
      }
      if (System.IO.File.Exists(fileName))
      {
        Log.WriteFile("RTSP: add stream {0} file:{1}", streamName, fileName);
        StreamAddTs(streamName, fileName);
        _streams[streamName] = fileName;
      }
    }

    public string Add(string fileName)
    {
      if (_initialized == false) return "";
      string streamName = String.Format("file{0}", _streamIndex++);
      if (_streams.ContainsKey(streamName))
      {
        Log.WriteFile("RTSP: add stream {0} already added");
        return streamName;
      }
      if (System.IO.File.Exists(fileName))
      {
        if (fileName.ToLower().IndexOf(".mpg") >= 0)
        {
          Log.WriteFile("RTSP: add stream {0} file:{1}", streamName, fileName);
          StreamAddMpg(streamName, fileName);
          _streams[streamName] = fileName;
        }
      }
      return streamName;
    }

    public void Remove(string streamName)
    {
      if (_initialized == false) return;
      Log.WriteFile("RTSP: remove stream {0}", streamName);
      if (_streams.ContainsKey(streamName))
      {
        StreamRemove(streamName);
        _streams.Remove(streamName);
      }
    }
    #endregion

    #region streaming thread

    protected void workerThread()
    {
      Log.WriteFile("RTSP: Streamer started");
      try
      {
        while (_running)
        {
          StreamRun();
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      Log.WriteFile("RTSP: Streamer stopped");
      _running = false;
    }
    #endregion
  }
}
