#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Streaming
{
  /// <summary>
  /// A class which manages an RTSP server.
  /// </summary>
  public class RtspServer
  {
    #region imports

    [DllImport("StreamingServer.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ServerSetup([MarshalAs(UnmanagedType.LPStr)] string ipAddress, ushort port);

    [DllImport("StreamingServer.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ServerShutdown();

    [DllImport("StreamingServer.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void StreamRun();

    [DllImport("StreamingServer.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void StreamAdd([MarshalAs(UnmanagedType.LPStr)] string id, 
                                          [MarshalAs(UnmanagedType.LPWStr)] string fileName, 
                                          byte channelType,
                                          [MarshalAs(UnmanagedType.I1)] bool isStaticFile);

    [DllImport("StreamingServer.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void StreamRemove([MarshalAs(UnmanagedType.LPStr)] string id);


    [DllImport("StreamingServer.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern ushort ClientGetCount();

    [DllImport("StreamingServer.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ClientGetDetail(ushort index,
                                                      out uint sessionId,
                                                      out IntPtr ipAddress,
                                                      out IntPtr streamId,
                                                      out ulong connectionTickCount,
                                                      [MarshalAs(UnmanagedType.I1)] out bool isActive);

    [DllImport("StreamingServer.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ClientRemove(uint sessionId);

    #endregion

    #region variables

    private object _lock = new object();
    private string _serverInterfaceConfigured = null;
    private string _serverInterfaceActual = null;
    private ushort _serverPort = 0;
    private Thread _streamingThread = null;
    private AutoResetEvent _streamingThreadStopEvent = null;
    private readonly Dictionary<string, RtspStream> _streams = new Dictionary<string, RtspStream>();

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="RtspServer"/> class.
    /// </summary>
    public RtspServer()
    {
      ReloadConfiguration();
    }

    ~RtspServer()
    {
      Dispose(false);
    }

    #endregion

    #region properties

    /// <summary>
    /// Get a list of client details for the current streams.
    /// </summary>
    public ICollection<RtspClient> Clients
    {
      get
      {
        List<RtspClient> clients = new List<RtspClient>();
        ushort count = ClientGetCount();
        for (ushort i = 0; i < count; ++i)
        {
          uint sessionId;
          IntPtr ipAddressBuffer;
          IntPtr streamIdBuffer;
          ulong connectionTickCount;
          bool isActive;
          ClientGetDetail(i, out sessionId, out ipAddressBuffer, out streamIdBuffer, out connectionTickCount, out isActive);
          if (ipAddressBuffer == IntPtr.Zero || streamIdBuffer == IntPtr.Zero)
          {
            continue;
          }

          string streamId = Marshal.PtrToStringAnsi(streamIdBuffer);
          string description = string.Empty;
          if (_streams.ContainsKey(streamId))
          {
            RtspStream stream = _streams[streamId];
            if (!string.IsNullOrEmpty(stream.Name))
            {
              description = stream.Name;
            }
            else
            {
              description = "unknown";
            }
          }

          clients.Add(new RtspClient(sessionId, Marshal.PtrToStringAnsi(ipAddressBuffer), new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(connectionTickCount), isActive, streamId, description, GetStreamUrl(streamId)));
        }
        return clients;
      }
    }

    #endregion

    #region public members

    /// <summary>
    /// Reload the server's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("RTSP: reload configuration");
      string serverInterface = SettingsManagement.GetValue("rtspServerInterface", string.Empty);
      ushort serverPort = (ushort)SettingsManagement.GetValue("rtspServerPort", 554);
      lock (_lock)
      {
        if (string.Equals(serverInterface, _serverInterfaceConfigured) && serverPort == _serverPort)
        {
          return;
        }

        // Server interface and/or host name changed.
        string ourHostName = Dns.GetHostName();
        this.LogInfo("RTSP: host name = {0}, interface = {1}, port = {2}", ourHostName, serverInterface, serverPort);
        _serverInterfaceConfigured = serverInterface;
        _serverPort = serverPort;

        if (_streamingThread == null)
        {
          return;
        }

        if (!_streamingThread.IsAlive)
        {
          // Odd! The server should be running but it's not. It must have
          // crashed. We'll try to start it again.
          this.LogWarn("RTSP: server not running, attempting to restart");
          try
          {
            Start();
          }
          catch (Exception ex)
          {
            this.LogError(ex, "RTSP: failed to restart server");
          }
          return;
        }

        if (string.Equals(_serverInterfaceActual, _serverInterfaceConfigured))
        {
          // The server happens to be already using the selected interface. No
          // restart is required.
          return;
        }

        // Restarting the server will kill all streaming sessions. TV Server
        // Configuration will warn if the streamer has any clients. If the user
        // chooses to ignore the warning then that is their business. All we
        // can do is restore the streams after restarting.
        this.LogInfo("RTSP: reconfiguring server, client count = {0}", ClientGetCount());
        try
        {
          List<RtspStream> streams = new List<RtspStream>(_streams.Values);
          Stop();
          Start();
          string url;
          foreach (RtspStream stream in streams)
          {
            AddStream(stream, out url);
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "RTSP: failed to reconfigure server");
        }
      }
    }

    /// <summary>
    /// Start the server.
    /// </summary>
    public void Start()
    {
      lock (_lock)
      {
        if (_streamingThread != null)
        {
          if (_streamingThread.IsAlive)
          {
            return;
          }

          // Kill the existing thread if it is in "zombie" state.
          Stop();
        }

        this.LogInfo("RTSP: start server");
        try
        {
          string serverInterface = _serverInterfaceConfigured;
          if (string.IsNullOrEmpty(_serverInterfaceConfigured))
          {
            ISet<IPAddress> preferredInterfaces = GetDefaultGatewayInterfaces();
            this.LogDebug("RTSP: auto-select interface, preferred candidates = [{0}]", string.Join(", ", preferredInterfaces.Select(x => x.ToString())));
            foreach (IPAddress address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
              if (address.AddressFamily == AddressFamily.InterNetwork)
              {
                if (preferredInterfaces.Contains(address))
                {
                  serverInterface = address.ToString();
                  break;
                }
                if (string.IsNullOrEmpty(serverInterface))
                {
                  serverInterface = address.ToString();
                }
              }
            }

            if (string.IsNullOrEmpty(serverInterface))
            {
              this.LogError("RTSP: failed to find an appropriate interface, host name = {0}, interfaces = [{1}]", Dns.GetHostName(), string.Join(", ", Dns.GetHostAddresses(_serverInterfaceConfigured).Select(x => x.ToString())));
              return;
            }
          }
          this.LogDebug("RTSP: selected interface = {0}", serverInterface);

          int result = ServerSetup(serverInterface, _serverPort);
          if (result != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("RTSP: failed to setup server, result = {0}, interface = {1}, port = {2}", result, serverInterface, _serverPort);
            return;
          }
          _serverInterfaceActual = serverInterface;
        }
        catch (Exception ex)
        {
          this.LogError(ex, "RTSP: failed to setup server, unexpected error");
          return;
        }

        _streamingThreadStopEvent = new AutoResetEvent(false);
        _streamingThread = new Thread(new ThreadStart(StreamingThread));
        _streamingThread.Name = "RTSP streamer";
        _streamingThread.SetApartmentState(ApartmentState.STA);
        _streamingThread.IsBackground = true;
        _streamingThread.Start();
      }
    }

    /// <summary>
    /// Stop the server.
    /// </summary>
    public void Stop()
    {
      lock (_lock)
      {
        if (_streamingThread == null)
        {
          _streams.Clear();
          _serverInterfaceActual = null;
          return;
        }

        if (!_streamingThread.IsAlive)
        {
          this.LogWarn("RTSP: aborting streaming thread");
          _streamingThread.Abort();
        }
        else
        {
          this.LogInfo("RTSP: stop server");
          _streamingThreadStopEvent.Set();
          if (!_streamingThread.Join(5000))
          {
            this.LogWarn("RTSP: failed to join streaming thread, aborting thread");
            _streamingThread.Abort();
          }
        }
        _streamingThread = null;
        if (_streamingThreadStopEvent != null)
        {
          _streamingThreadStopEvent.Close();
          _streamingThreadStopEvent = null;
        }

        foreach (RtspStream stream in _streams.Values)
        {
          StreamRemove(stream.Id);
        }
        ServerShutdown();
        _streams.Clear();
        _serverInterfaceActual = null;
      }
    }

    /// <summary>
    /// Get the server's information.
    /// </summary>
    /// <param name="boundInterface">The interface (IP address) that the server is bound to.</param>
    /// <param name="port">The port that the server is listening on.</param>
    public void GetInformation(out string boundInterface, out ushort port)
    {
      lock (_lock)
      {
        if (_streamingThread == null || !_streamingThread.IsAlive)
        {
          this.LogDebug("debug: server not running");
          boundInterface = string.Empty;
          port = 0;
          return;
        }
        boundInterface = _serverInterfaceActual;
        port = _serverPort;
      }
    }

    /// <summary>
    /// Create a new RTSP stream for a static file.
    /// </summary>
    /// <param name="id">The identifier for the stream.</param>
    /// <param name="fileName">The name of the file to stream.</param>
    /// <param name="mediaType">The type of the stream.</param>
    /// <param name="name">A human-readable name for the stream.</param>
    /// <param name="url">The stream's URL.</param>
    /// <returns><c>true</c> if the stream is added successfully, otherwise <c>false</c></returns>
    public bool AddFileStream(string id, string fileName, MediaType mediaType, string name, out string url)
    {
      return AddStream(new RtspStream(id, fileName, mediaType, name), out url);
    }

    /// <summary>
    /// Create a new RTSP stream for a time-shifting session.
    /// </summary>
    /// <param name="id">The identifier for the stream.</param>
    /// <param name="fileName">The time-shifting register file name.</param>
    /// <param name="mediaType">The type of the stream.</param>
    /// <param name="tuner">The tuner that the session is associated with.</param>
    /// <param name="subChannelId">The identifier of the tuner sub-channel that the session is associated with.</param>
    /// <returns><c>true</c> if the stream is added successfully, otherwise <c>false</c></returns>
    public bool AddTimeShiftingStream(string id, string fileName, MediaType mediaType, ITuner tuner, int subChannelId)
    {
      string url;
      return AddStream(new RtspStream(id, fileName, mediaType, tuner, subChannelId), out url);
    }

    /// <summary>
    /// Get the URL for an active stream.
    /// </summary>
    /// <param name="streamId">The stream's identifier.</param>
    /// <returns>the stream's URL if <paramref name="streamId">the stream identifier</paramref> is valid, otherwise <c>string.Empty</c></returns>
    public string GetStreamUrl(string streamId)
    {
      lock (_lock)
      {
        if (_streamingThread == null || !_streamingThread.IsAlive || _serverInterfaceActual == null)
        {
          return string.Empty;
        }
        return string.Format("rtsp://{0}:{1}/{2}", _serverInterfaceActual, _serverPort, streamId);
      }
    }

    /// <summary>
    /// Remove an RTSP stream.
    /// </summary>
    /// <param name="streamId">The stream's identifier.</param>
    public void RemoveStream(string streamId)
    {
      lock (_lock)
      {
        if (_streamingThread == null || !_streamingThread.IsAlive)
        {
          this.LogError("RTSP: failed to remove stream, server not running");
          return;
        }
        this.LogDebug("RTSP: remove stream, ID = {0}", streamId);
        if (_streams.ContainsKey(streamId))
        {
          StreamRemove(streamId);
          _streams.Remove(streamId);
        }
        else
        {
          this.LogWarn("RTSP: asked to remove invalid stream, ID = {0}", streamId);
        }
      }
    }

    /// <summary>
    /// Remove all RTSP streams for a file.
    /// </summary>
    /// <param name="fileName">The file's name.</param>
    public void RemoveFile(string fileName)
    {
      lock (_lock)
      {
        if (_streamingThread == null || !_streamingThread.IsAlive)
        {
          this.LogError("RTSP: failed to remove file, server not running");
          return;
        }
        this.LogDebug("RTSP: remove file, file name = {0}", fileName);
        List<string> ids = new List<string>();
        foreach (RtspStream stream in _streams.Values)
        {
          if (string.Equals(stream.FileName, fileName))
          {
            StreamRemove(stream.Id);
            ids.Add(stream.Id);
          }
        }
        this.LogDebug("RTSP: IDs for file = {0}", string.Join(", ", ids));
        foreach (string id in ids)
        {
          _streams.Remove(id);
        }
      }
    }

    /// <summary>
    /// Disconnect an RTSP stream client.
    /// </summary>
    /// <param name="sessionId">The client's session identifier.</param>
    public void DisconnectStreamClient(uint sessionId)
    {
      lock (_lock)
      {
        if (_streamingThread == null || !_streamingThread.IsAlive)
        {
          this.LogError("RTSP: failed to disconnect stream client, server not running");
          return;
        }
        this.LogDebug("RTSP: disconnect stream client, session ID = {0}", sessionId);
        ClientRemove(sessionId);
      }
    }

    #endregion

    #region private members

    private ISet<IPAddress> GetDefaultGatewayInterfaces()
    {
      HashSet<IPAddress> addresses = new HashSet<IPAddress>();
      try
      {
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT DefaultIPGateway, IPAddress FROM Win32_NetworkAdapterConfiguration"))
        {
          foreach (ManagementObject queryObj in searcher.Get())
          {
            if (queryObj["DefaultIPGateway"] != null && queryObj["IPAddress"] != null)
            {
              string[] defaultIpGateways = (string[])queryObj["DefaultIPGateway"];
              string[] ipAddresses = (string[])queryObj["IPAddress"];
              if (defaultIpGateways.Length > 0 && ipAddresses.Length > 0)
              {
                foreach (string address in ipAddresses)
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
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RTSP: failed to assemble default gateway interface list");
      }
      return addresses;
    }

    /// <summary>
    /// The streaming worker thread.
    /// </summary>
    private void StreamingThread()
    {
      try
      {
        while (!_streamingThreadStopEvent.WaitOne(1))
        {
          StreamRun();
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RTSP: streamer thread exception");
        lock (_streamingThread)
        {
          foreach (RtspStream stream in _streams.Values)
          {
            StreamRemove(stream.Id);
          }
          ServerShutdown();
          _streams.Clear();
          _serverInterfaceActual = null;
        }
      }
    }

    /// <summary>
    /// Create a new RTSP stream.
    /// </summary>
    /// <param name="stream">The stream details.</param>
    /// <param name="url">The stream's URL.</param>
    /// <returns><c>true</c> if the stream is added successfully, otherwise <c>false</c></returns>
    private bool AddStream(RtspStream stream, out string url)
    {
      url = string.Empty;
      lock (_lock)
      {
        if (_streamingThread == null || !_streamingThread.IsAlive)
        {
          Start();
        }
        else if (_streams.ContainsKey(stream.Id))
        {
          this.LogError("RTSP: failed to add duplicate stream, ID = {0}", stream.Id);
          return false;
        }
        if (!File.Exists(stream.FileName))
        {
          this.LogError("RTSP: failed to add stream for invalid file, file name = {0}", stream.FileName);
          return false;
        }
        this.LogDebug("RTSP: add stream, ID = {0}, media type = {1}, file name = {2}", stream.Id, stream.MediaType, stream.FileName);
        StreamAdd(stream.Id, stream.FileName, (byte)(stream.MediaType == MediaType.Television ? 0 : 1), !stream.IsTimeShifting);
        _streams[stream.Id] = stream;
      }
      url = GetStreamUrl(stream.Id);
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the detector is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        Stop();
      }
    }

    #endregion
  }
}