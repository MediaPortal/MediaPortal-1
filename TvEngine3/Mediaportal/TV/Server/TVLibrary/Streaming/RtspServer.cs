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

    private enum ServerJobType
    {
      AddStream,
      RemoveStream,
      RemoveFile,
      GetClientDetails,
      RemoveClient,
      ReconfigureServer
    }

    private class ServerJob
    {
      public ServerJobType JobType;
      public object[] Parameters;
      public bool WasSuccessful;
      public ManualResetEvent WaitEvent;
    }

    #region variables

    private string _serverInterfaceConfigured = null;
    private string _serverInterfaceActual = null;
    private ushort _serverPort = 0;
    private readonly Dictionary<string, RtspStream> _streams = new Dictionary<string, RtspStream>();

    private object _serverThreadLock = new object();
    private Thread _serverThread = null;
    private bool _isServerThreadStarted = false;
    private ManualResetEvent _serverThreadStopEvent = null;

    private object _jobQueueLock = new object();
    private Queue<ServerJob> _jobs = new Queue<ServerJob>();

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
        object[] parameters = new object[1];
        InvokeServerJob(ServerJobType.GetClientDetails, ref parameters);
        return parameters[0] as ICollection<RtspClient>;
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
      lock (_serverThreadLock)
      {
        if (string.Equals(serverInterface, _serverInterfaceConfigured) && serverPort == _serverPort)
        {
          return;
        }

        // Server interface and/or host name changed.
        string ourHostName = Dns.GetHostName();
        this.LogInfo("RTSP: host name = {0}, interface = {1}, port = {2}", ourHostName, serverInterface, serverPort);
        _serverInterfaceConfigured = serverInterface;
        ushort serverPortActual = _serverPort;
        _serverPort = serverPort;

        if (_serverThread == null || !_serverThread.IsAlive || (string.Equals(_serverInterfaceActual, _serverInterfaceConfigured) && serverPortActual == _serverPort))
        {
          // The server isn't running or happens to be already using the
          // correct configuration. There's no need to do anything more. If the
          // server isn't running, it'll simply pick up the new configuration
          // automatically next time it starts.
          return;
        }

        object[] parameters = null;
        InvokeServerJob(ServerJobType.ReconfigureServer, ref parameters);
      }
    }

    /// <summary>
    /// Start the server.
    /// </summary>
    /// <returns><c>true</c> if the server is started successfully, otherwise <c>false</c></returns>
    public bool Start()
    {
      this.LogInfo("RTSP: start");
      lock (_serverThreadLock)
      {
        if (_serverThread != null)
        {
          this.LogWarn("RTSP: server thread is already running");
          if (_serverThread.IsAlive)
          {
            return true;
          }

          // Kill the existing thread if it is in "zombie" state.
          Stop();
        }

        ManualResetEvent startEvent = new ManualResetEvent(false);
        try
        {
          _serverThreadStopEvent = new ManualResetEvent(false);
          _serverThread = new Thread(new ParameterizedThreadStart(ServerThread));
          _serverThread.Name = "RTSP server";
          _serverThread.SetApartmentState(ApartmentState.STA);
          _serverThread.IsBackground = true;
          _serverThread.Start(startEvent);

          startEvent.WaitOne();
          return _isServerThreadStarted;
        }
        finally
        {
          startEvent.Close();
          startEvent.Dispose();
        }
      }
    }

    /// <summary>
    /// Stop the server.
    /// </summary>
    public void Stop()
    {
      this.LogInfo("RTSP: stop");
      lock (_serverThreadLock)
      {
        if (_serverThread == null)
        {
          return;
        }

        if (!_serverThread.IsAlive)
        {
          this.LogWarn("RTSP: aborting server thread");
          _serverThread.Abort();
        }
        else
        {
          _serverThreadStopEvent.Set();
          if (!_serverThread.Join(5000))
          {
            this.LogWarn("RTSP: failed to join server thread, aborting thread");
            _serverThread.Abort();
          }
        }
        _serverThread = null;

        if (_serverThreadStopEvent != null)
        {
          _serverThreadStopEvent.Close();
          _serverThreadStopEvent = null;
        }
      }
    }

    /// <summary>
    /// Get the server's information.
    /// </summary>
    /// <param name="boundInterface">The interface (IP address) that the server is bound to.</param>
    /// <param name="port">The port that the server is listening on.</param>
    public void GetInformation(out string boundInterface, out ushort port)
    {
      lock (_serverThreadLock)
      {
        if (_serverThread == null || !_serverThread.IsAlive)
        {
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
      url = string.Empty;
      object[] parameters = new object[2] { new RtspStream(id, fileName, mediaType, name), url };
      if (InvokeServerJob(ServerJobType.AddStream, ref parameters))
      {
        url = (string)parameters[1];
        return true;
      }
      return false;
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
      object[] parameters = new object[2] { new RtspStream(id, fileName, mediaType, tuner, subChannelId), string.Empty };
      return InvokeServerJob(ServerJobType.AddStream, ref parameters);
    }

    /// <summary>
    /// Get the URL for an active stream.
    /// </summary>
    /// <param name="streamId">The stream's identifier.</param>
    /// <returns>the stream's URL if <paramref name="streamId">the stream identifier</paramref> is valid, otherwise <c>string.Empty</c></returns>
    public string GetStreamUrl(string streamId)
    {
      lock (_serverThreadLock)
      {
        if (_serverThread == null || !_serverThread.IsAlive || _serverInterfaceActual == null)
        {
          return string.Empty;
        }
        return ServerThreadGetStreamUrl(streamId);
      }
    }

    /// <summary>
    /// Remove an RTSP stream.
    /// </summary>
    /// <param name="streamId">The stream's identifier.</param>
    public void RemoveStream(string streamId)
    {
      object[] parameters = new object[1] { streamId };
      InvokeServerJob(ServerJobType.RemoveStream, ref parameters);
    }

    /// <summary>
    /// Remove all RTSP streams for a file.
    /// </summary>
    /// <param name="fileName">The file's name.</param>
    public void RemoveFile(string fileName)
    {
      object[] parameters = new object[1] { fileName };
      InvokeServerJob(ServerJobType.RemoveFile, ref parameters);
    }

    /// <summary>
    /// Disconnect an RTSP stream client.
    /// </summary>
    /// <param name="sessionId">The client's session identifier.</param>
    public void DisconnectStreamClient(uint sessionId)
    {
      object[] parameters = new object[1] { sessionId };
      InvokeServerJob(ServerJobType.RemoveClient, ref parameters);
    }

    #endregion

    #region private members

    private ISet<IPAddress> GetDefaultNetworkGatewayInterfaces()
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
    /// The server worker thread.
    /// </summary>
    /// <remarks>
    /// All interactions with the server must be performed from this thread.
    /// This is a limitation of the LIVE555 library used to implement the
    /// server.
    /// </remarks>
    private void ServerThread(object eventParam)
    {
      try
      {
        _isServerThreadStarted = ServerThreadStartServer();
        if (!_isServerThreadStarted)
        {
          return;
        }

        lock (_jobQueueLock)
        {
          _jobs.Clear();
        }
      }
      finally
      {
        (eventParam as ManualResetEvent).Set();
      }

      try
      {
        while (!_serverThreadStopEvent.WaitOne(1))
        {
          lock (_jobQueueLock)
          {
            while (_jobs.Count > 0)
            {
              ServerJob job = _jobs.Dequeue();
              try
              {
                job.WasSuccessful = true;
                switch (job.JobType)
                {
                  case ServerJobType.AddStream:
                    string url;
                    job.WasSuccessful = ServerThreadAddStream(job.Parameters[0] as RtspStream, out url);
                    job.Parameters[1] = url;
                    break;
                  case ServerJobType.RemoveStream:
                    ServerThreadRemoveStream(job.Parameters[0] as string);
                    break;
                  case ServerJobType.RemoveFile:
                    ServerThreadRemoveFile(job.Parameters[0] as string);
                    break;
                  case ServerJobType.GetClientDetails:
                    job.Parameters[0] = ServerThreadGetClientDetails();
                    break;
                  case ServerJobType.RemoveClient:
                    ServerThreadRemoveClient((uint)job.Parameters[0]);
                    break;
                  case ServerJobType.ReconfigureServer:
                    ServerThreadReconfigureServer();
                    break;
                }
              }
              catch (ThreadAbortException)
              {
                job.WasSuccessful = false;
                throw;
              }
              catch (Exception ex)
              {
                this.LogError(ex, "RTSP: unexpected server thread job exception, job type = {0}", job.JobType);
                job.WasSuccessful = false;
              }
              finally
              {
                job.WaitEvent.Set();
              }
            }
          }

          StreamRun();
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RTSP: streamer thread exception");
      }

      ServerThreadStopServer();
    }

    /// <summary>
    /// Invoke a job on the server's thread.
    /// </summary>
    /// <param name="jobType">The job type.</param>
    /// <param name="parameters">The job's parameters.</param>
    /// <returns><c>true</c> if the job was completed successfully, otherwise <c>false</c></returns>
    private bool InvokeServerJob(ServerJobType jobType, ref object[] parameters)
    {
      lock (_serverThreadLock)
      {
        if (_serverThread == null || !_serverThread.IsAlive)
        {
          // Odd! The server should be running but it's not. It must have
          // crashed. We'll try to start it again.
          this.LogWarn("RTSP: server not running, attempting to restart");
          if (!Start())
          {
            return false;
          }
        }

        ServerJob job = new ServerJob();
        job.JobType = jobType;
        job.Parameters = parameters;
        job.WaitEvent = new ManualResetEvent(false);

        lock (_jobQueueLock)
        {
          _jobs.Enqueue(job);
        }

        // Wait for the job to complete. The time limit is arbitrary. Normally
        // we'd expect a job to complete within a second. Mainly we just have
        // to be careful to avoid causing deadlock.
        if (!job.WaitEvent.WaitOne(60000))
        {
          this.LogError("RTSP: job failed to complete within a reasonable time, job type = {0}.", jobType);
          return false;
        }
        job.WaitEvent.Close();
        return job.WasSuccessful;
      }
    }

    #region private server thread job implementations
    // These functions should only be called from the server thread. We impose
    // this restriction in order to comply with the limitations of the LIVE555
    // library which is used to implement the server.

    private bool ServerThreadStartServer()
    {
      this.LogInfo("RTSP: start server");
      try
      {
        string serverInterface = _serverInterfaceConfigured;
        if (string.IsNullOrEmpty(_serverInterfaceConfigured))
        {
          ISet<IPAddress> preferredInterfaces = GetDefaultNetworkGatewayInterfaces();
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
            return false;
          }
        }
        this.LogDebug("RTSP: selected interface = {0}", serverInterface);

        int result = ServerSetup(serverInterface, _serverPort);
        if (result != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("RTSP: failed to setup server, result = {0}, interface = {1}, port = {2}", result, serverInterface, _serverPort);
          return false;
        }
        _serverInterfaceActual = serverInterface;
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "RTSP: failed to setup server, unexpected error");
        return false;
      }
    }

    private void ServerThreadStopServer()
    {
      this.LogInfo("RTSP: stop server");
      foreach (RtspStream stream in _streams.Values)
      {
        StreamRemove(stream.Id);
      }
      ServerShutdown();
      _streams.Clear();
      _serverInterfaceActual = null;
    }

    private string ServerThreadGetStreamUrl(string streamId)
    {
      return string.Format("rtsp://{0}:{1}/{2}", _serverInterfaceActual, _serverPort, streamId);
    }

    private bool ServerThreadAddStream(RtspStream stream, out string url)
    {
      this.LogDebug("RTSP: add stream, ID = {0}, media type = {1}, file name = {2}", stream.Id, stream.MediaType, stream.FileName);
      url = string.Empty;

      if (_streams.ContainsKey(stream.Id))
      {
        this.LogError("RTSP: failed to add duplicate stream, ID = {0}", stream.Id);
        return false;
      }
      if (!File.Exists(stream.FileName))
      {
        this.LogError("RTSP: failed to add stream for invalid file, file name = {0}", stream.FileName);
        return false;
      }

      StreamAdd(stream.Id, stream.FileName, (byte)(stream.MediaType == MediaType.Television ? 0 : 1), !stream.IsTimeShifting);
      _streams[stream.Id] = stream;
      url = ServerThreadGetStreamUrl(stream.Id);
      return true;
    }

    private void ServerThreadRemoveStream(string streamId)
    {
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

    private void ServerThreadRemoveFile(string fileName)
    {
      this.LogDebug("RTSP: remove file, file name = {0}", fileName);
      List<string> streamIds = new List<string>();
      foreach (RtspStream stream in _streams.Values)
      {
        if (string.Equals(stream.FileName, fileName))
        {
          StreamRemove(stream.Id);
          streamIds.Add(stream.Id);
        }
      }
      this.LogDebug("RTSP: IDs for file = {0}", string.Join(", ", streamIds));
      foreach (string id in streamIds)
      {
        _streams.Remove(id);
      }
    }

    private ICollection<RtspClient> ServerThreadGetClientDetails()
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

        clients.Add(new RtspClient(sessionId, Marshal.PtrToStringAnsi(ipAddressBuffer), new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(connectionTickCount), isActive, streamId, description, ServerThreadGetStreamUrl(streamId)));
      }
      return clients;
    }

    private void ServerThreadRemoveClient(uint sessionId)
    {
      this.LogDebug("RTSP: remove client, session ID = {0}", sessionId);
      ClientRemove(sessionId);
    }

    private void ServerThreadReconfigureServer()
    {
      // Reconfiguring the server requires restarting the server, which will
      // kill all streaming sessions. TV Server Configuration will warn if the
      // server has any clients. If the user chooses to ignore the warning then
      // that is their business. All we can do is restore the streams after
      // restarting.
      this.LogInfo("RTSP: reconfigure server, stream count = {0}, client count = {1}", _streams.Count, ClientGetCount());
      List<RtspStream> streams = new List<RtspStream>(_streams.Values);
      ServerThreadStopServer();
      if (!ServerThreadStartServer())
      {
        return;
      }

      this.LogDebug("RTSP: restoring streams");
      string url;
      foreach (RtspStream stream in streams)
      {
        ServerThreadAddStream(stream, out url);
      }
    }

    #endregion

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