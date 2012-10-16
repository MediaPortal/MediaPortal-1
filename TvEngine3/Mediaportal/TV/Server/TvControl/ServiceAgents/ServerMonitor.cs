using System;
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class ServerMonitor
  {
    #region events & delegates

    public delegate void ServerDisconnectedDelegate();
    public delegate void ServerConnectedDelegate();

    public event ServerDisconnectedDelegate OnServerDisconnected;
    public event ServerConnectedDelegate OnServerConnected;

    #endregion

    private Thread _serverMonitorThread;
    private readonly static ManualResetEvent _evtHeartbeatCtrl = new ManualResetEvent(false);
    private const int SERVER_ALIVE_INTERVAL_SEC = 5;
    private bool _isConnected;

    private readonly ManualResetEvent _evtServer = new ManualResetEvent(false);

    public void Start()
    {
      _evtHeartbeatCtrl.Reset();
      StartServerMonitorThread();
    }

    public void Stop()
    {
      StopServerMonitorThread();
      _evtHeartbeatCtrl.Reset();
    }

    public bool WaitForConnection(int timeOut)
    {
      return _evtServer.WaitOne(timeOut);
    }

    private void StartServerMonitorThread()
    {
      if (_serverMonitorThread == null || !_serverMonitorThread.IsAlive)
      {
        _evtHeartbeatCtrl.Reset();
        Log.Debug("ServerMonitor: ServerMonitor thread started.");
        _serverMonitorThread = new Thread(ServerMonitorThread) { IsBackground = true, Name = "ServerMonitor thread" };
        _serverMonitorThread.Start();
      }
    }

    private void StopServerMonitorThread()
    {
      if (_serverMonitorThread != null && _serverMonitorThread.IsAlive)
      {
        try
        {
          _evtHeartbeatCtrl.Set();
          _serverMonitorThread.Join();
          Log.Debug("ServerMonitor: ServerMonitor thread stopped.");
        }
        catch (Exception) { }
      }
    }



    private void ServerMonitorThread()
    {
      while (!_evtHeartbeatCtrl.WaitOne(SERVER_ALIVE_INTERVAL_SEC * 1000))
      {

        bool isconnected = false;
        try
        {
          ServiceAgents.Instance.DiscoverServiceAgent.Ping();
          isconnected = true;
        }
        catch
        {
        }
        finally
        {
          if (!_isConnected && isconnected)
          {            
            if (OnServerConnected != null)
            {
              OnServerConnected();
            }
          }
          else if (_isConnected && !isconnected)
          {            
            if (OnServerDisconnected != null)
            {
              OnServerDisconnected();
            }
          }
          _isConnected = isconnected;

          if (isconnected)
          {
            _evtServer.Set();
          }
          else
          {
            _evtServer.Reset();
          }
        }
      }
    }
  }
}
