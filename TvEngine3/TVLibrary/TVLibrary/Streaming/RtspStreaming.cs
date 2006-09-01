/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using TvLibrary.Log;

namespace TVLibrary.Streaming
{
  /// <summary>
  /// class which handles all RTSP related tasks
  /// </summary>
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
    /// <summary>
    /// Starts RTSP Streaming.
    /// </summary>
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


    /// <summary>
    /// Stops RTSP streaming.
    /// </summary>
    public void Stop()
    {
      if (_initialized == false) return;
      Log.WriteFile("RTSP: stop streamer");
      _running = false;
    }

    /// <summary>
    /// Creates a new RTSP stream
    /// </summary>
    /// <param name="streamName">Name of the stream.</param>
    /// <param name="fileName">Name of the file.</param>
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

    /// <summary>
    /// Creates a new RTSP stream
    /// </summary>
    /// <param name="fileName">file to stream.</param>
    /// <returns>name of the stream</returns>
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

    /// <summary>
    /// Removes the specified stream .
    /// </summary>
    /// <param name="streamName">Name of the stream.</param>
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

    /// <summary>
    /// worker thread which handles all streaming activity
    /// </summary>
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
