#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Management;

namespace TvLibrary.Streaming
{
  /// <summary>
  /// class which handles all RTSP related tasks
  /// </summary>
  public class RtspStreaming
  {
    #region imports

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    private static extern void StreamSetup(string ipAdress);

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    private static extern int StreamSetupEx(string ipAdress, int port);

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    private static extern void StreamRun();

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    private static extern void StreamAddTimeShiftFile(string streamName, string fileName, bool isProgramStream);

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    private static extern void StreamAddMpegFile(string streamName, string fileName);

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    private static extern void StreamRemove(string streamName);


    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    private static extern void StreamGetClientCount(ref short clients);

    [DllImport("StreamingServer.dll", CharSet = CharSet.Ansi)]
    private static extern void StreamGetClientDetail(short clientNr, out IntPtr ipAdres, out IntPtr streamName,
                                                     ref short isActive, out long ticks);

    #endregion

    #region constants

    public const int DefaultPort = 554;

    #endregion

    #region variables

    private int _port;
    private bool _running;
    private readonly bool _initialized;
    private readonly Dictionary<string, RtspStream> _streams;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RtspStreaming"/> class.
    /// </summary>
    /// <param name="hostName">ipadress to use for streaming.</param>
    public RtspStreaming(string hostName)
      : this(hostName, DefaultPort) {}

    /// <summary>
    /// Initializes a new instance of the <see cref="RtspStreaming"/> class.
    /// </summary>
    /// <param name="hostName">ipadress to use for streaming.</param>
    /// <param name="port">port no to use for streaming</param>
    public RtspStreaming(string hostName, int port)
    {
      int result;
      try
      {
        IList<IPAddress> preferedAddresses = GetDefGatewayNetAddresses();
        IPHostEntry local = Dns.GetHostByName(hostName);
        IPAddress selectedAddress = null;

        foreach (IPAddress ipaddress in local.AddressList)
        {
          if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
          {
            if (selectedAddress == null)
            {
              selectedAddress = ipaddress;
            }
            if (preferedAddresses.Contains(ipaddress))
            {
              selectedAddress = ipaddress;
              break;
            }
          }
        }

        if (selectedAddress != null)
        {
          result = StreamSetupEx(selectedAddress.ToString(), port);
          if (result == 1)
          {
            throw new Exception("Error initializing streaming server");
          }
          _port = port;
          _initialized = true;
          _streams = new Dictionary<string, RtspStream>();
        }
        else
        {
          throw new Exception("RtspStreaming: Could not find an ip address to listen on.");
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets the streaming clients.
    /// </summary>
    /// <value>The clients.</value>
    public List<RtspClient> Clients
    {
      get
      {
        List<RtspClient> clients = new List<RtspClient>();
        short count = 0;
        StreamGetClientCount(ref count);
        for (short i = 0; i < count; ++i)
        {
          short isActive = 0;
          IntPtr ptrIpAdress;
          IntPtr ptrStream;
          string ipadress = "";
          string streamName = "";
          long ticks;
          StreamGetClientDetail(i, out ptrIpAdress, out ptrStream, ref isActive, out ticks);
          DateTime started = new DateTime(1970, 1, 1, 0, 0, 0);
          started = started.AddSeconds(ticks);

          if (ptrIpAdress != IntPtr.Zero)
          {
            ipadress = Marshal.PtrToStringAnsi(ptrIpAdress);
          }
          if (ptrStream != null)
          {
            streamName = Marshal.PtrToStringAnsi(ptrStream);
          }
          string description = "";

          if (_streams.ContainsKey(streamName))
          {
            RtspStream stream = _streams[streamName];
            if (!string.IsNullOrEmpty(stream.Recording))
              description = stream.Recording;
            else if (stream.Card.SubChannels.Length > 0)
            {
              description = stream.Card.SubChannels[0].CurrentChannel.Name;
            }
          }

          if (description.Length > 0)
          {
            RtspClient client = new RtspClient(isActive != 0, ipadress, streamName, description, started);
            clients.Add(client);
          }
        }
        return clients;
      }
    }

    public int Port
    {
      get { return _initialized ? _port : 0; }
    }

    #endregion

    #region public members

    /// <summary>
    /// Starts RTSP Streaming.
    /// </summary>
    public void Start()
    {
      if (_initialized == false)
        return;
      if (_running)
        return;
      Log.Log.WriteFile("RTSP: start streamer");
      _running = true;
      Thread thread = new Thread(workerThread);
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
      Log.Log.WriteFile("RTSP: stop streamer");
      if (_initialized == false)
        return;
      StopAllStreams();
      _running = false;
    }

    private void StopAllStreams()
    {
      Log.Log.WriteFile("RTSP: stop all streams ({0})", _streams.Count);

      List<string> removals = new List<string>();
      foreach (string key in _streams.Keys)
      {
        removals.Add(key);
      }
      foreach (string key in removals)
      {
        Remove(key);
      }
    }

    /// <summary>
    /// Creates a new RTSP stream
    /// </summary>
    /// <param name="stream">The rtsp stream</param>
    public void AddStream(RtspStream stream)
    {
      if (_initialized == false)
        return;
      if (_streams.ContainsKey(stream.Name))
      {
        return;
      }
      if (System.IO.File.Exists(stream.FileName))
      {
        Log.Log.WriteFile("RTSP: add stream {0} file:{1}", stream.Name, stream.FileName);
        if (stream.Card != null)
        {
          StreamAddTimeShiftFile(stream.Name, stream.FileName, false);
        }
        else
        {
          StreamAddMpegFile(stream.Name, stream.FileName);
        }
        _streams[stream.Name] = stream;
      }
    }

    /// <summary>
    /// Removes the specified stream .
    /// </summary>
    /// <param name="streamName">Name of the stream.</param>
    public void Remove(string streamName)
    {
      if (_initialized == false)
        return;
      Log.Log.WriteFile("RTSP: remove stream {0}", streamName);
      if (_streams.ContainsKey(streamName))
      {
        StreamRemove(streamName);
        _streams.Remove(streamName);
      }
    }

    /// <summary>
    /// Stops streaming the file
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public void RemoveFile(string fileName)
    {
      Dictionary<string, RtspStream>.Enumerator enumer = _streams.GetEnumerator();
      while (enumer.MoveNext())
      {
        if (String.Compare(fileName, enumer.Current.Value.FileName, true) == 0)
        {
          Remove(enumer.Current.Key);
          return;
        }
      }
    }

    #endregion

    #region streaming thread

    /// <summary>
    /// worker thread which handles all streaming activity
    /// </summary>
    protected void workerThread()
    {
      Log.Log.WriteFile("RTSP: Streamer started");
      try
      {
        while (_running)
        {
          StreamRun();
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      Log.Log.WriteFile("RTSP: Streamer stopped");
      _running = false;
    }

    #endregion

    #region properties

    ///<summary>
    /// Number of active rtsp streams
    ///</summary>
    public int ActiveStreams
    {
      get { return _streams.Count; }
    }

    #endregion

    #region private members

    private IList<IPAddress> GetDefGatewayNetAddresses()
    {
      List<IPAddress> addresses = new List<IPAddress>();
      try
      {
        ManagementObjectSearcher searcher =
          new ManagementObjectSearcher("root\\CIMV2",
                                       "SELECT DefaultIPGateway, IPAddress FROM Win32_NetworkAdapterConfiguration");

        foreach (ManagementObject queryObj in searcher.Get())
        {
          if (queryObj["DefaultIPGateway"] != null && queryObj["IPAddress"] != null)
          {
            String[] arrDefaultIPGateway = (String[])(queryObj["DefaultIPGateway"]);
            String[] arrIPAddress = (String[])(queryObj["IPAddress"]);
            if (arrDefaultIPGateway.Length > 0 && arrIPAddress.Length > 0)
            {
              foreach (string address in arrIPAddress)
              {
                IPAddress ipAddress = IPAddress.Parse(address);
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                  addresses.Add(ipAddress);
                }
              }
            }
          }
        }
      }
      catch (ManagementException e)
      {
        Log.Log.Error("Failed to retrieve ip addresses with default gateway, WMI error: " + e.ToString());
      }
      return addresses;
    }

    #endregion
  }
}