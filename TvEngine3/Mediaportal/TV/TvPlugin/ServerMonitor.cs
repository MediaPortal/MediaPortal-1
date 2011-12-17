using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.ServiceAgents;

namespace Mediaportal.TV.TvPlugin
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

    public void Start()
    {
      //System.Diagnostics.Debugger.Launch();
      StartServerMonitorThread();
    }

    public void Stop()
    {
      StopServerMonitorThread();
    }

    private void StartServerMonitorThread()
    {
      if (_serverMonitorThread == null || !_serverMonitorThread.IsAlive)
      {
        _evtHeartbeatCtrl.Reset();
        Log.Debug("ServerMonitor: ServerMonitor thread started.");
        _serverMonitorThread = new Thread(ServerMonitorThread) {IsBackground = true, Name = "ServerMonitor thread"};
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
      while (!_evtHeartbeatCtrl.WaitOne(SERVER_ALIVE_INTERVAL_SEC*1000))
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
        }
      }
    }
  }
}
