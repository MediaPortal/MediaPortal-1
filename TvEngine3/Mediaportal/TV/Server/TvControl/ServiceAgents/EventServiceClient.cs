using System;
using System.ServiceModel;
using System.Threading;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class EventServiceClient : IEventServiceAgent
  {
    #region events & delegates

    private delegate void HeartbeatRequestReceivedDelegate();
    private delegate void TvServerEventReceivedDelegate(TvServerEventArgs eventArgs);
    private delegate void CiMenuCallbackDelegate(CiMenu menu);

    private event HeartbeatRequestReceivedDelegate OnHeartbeatRequestReceived;
    private event TvServerEventReceivedDelegate OnTvServerEventReceived;
    private event CiMenuCallbackDelegate OnCiMenuCallbackReceived;

    #endregion

    private static bool _serverDown = false;
    private readonly string _hostname;
    private EventServiceAgent _eventServiceAgent;

    public EventServiceClient(string hostname)
    {      
      _hostname = hostname;      
    }
    
    private EventServiceAgent EventServiceAgent
    {
      get
      {
        if (_eventServiceAgent == null)
        {
          CreateEventService();
          RegisterEventServiceIfNeeded();
          RegisterUserForServerEvents();
        }
        return _eventServiceAgent;        
      }
    }

    private void CreateEventService()
    {
      _eventServiceAgent = new EventServiceAgent(_hostname);
      AddEventHandlers();
    }

    private void AddEventHandlers()
    {
      _eventServiceAgent.OnCiMenuCallbackReceived += _eventServiceAgent_OnCiMenuCallbackReceived;
      _eventServiceAgent.OnHeartbeatRequestReceived += _eventServiceAgent_OnHeartbeatRequestReceived;
      _eventServiceAgent.OnTvServerEventReceived += _eventServiceAgent_OnTvServerEventReceived;
      _eventServiceAgent.OnConnectionLost += _eventServiceAgent_OnConnectionLost;
    }

    #region event handlers

    private void _eventServiceAgent_OnConnectionLost()
    {
      RemoveEventHandlers();
      _eventServiceAgent = null;
      if (!_serverDown)
      {
        ReConnect();
      }
    }

    private void RemoveEventHandlers()
    {
      _eventServiceAgent.OnCiMenuCallbackReceived -= _eventServiceAgent_OnCiMenuCallbackReceived;
      _eventServiceAgent.OnHeartbeatRequestReceived -= _eventServiceAgent_OnHeartbeatRequestReceived;
      _eventServiceAgent.OnTvServerEventReceived -= _eventServiceAgent_OnTvServerEventReceived;
      _eventServiceAgent.OnConnectionLost -= _eventServiceAgent_OnConnectionLost;
    }

    private void _eventServiceAgent_OnTvServerEventReceived(TvServerEventArgs eventArgs)
    {
      if (OnTvServerEventReceived != null)
      {
        OnTvServerEventReceived(eventArgs);
      }
    }

    private void _eventServiceAgent_OnHeartbeatRequestReceived()
    {
      if (OnHeartbeatRequestReceived != null)
      {
        OnHeartbeatRequestReceived();
      }
    }

    private void _eventServiceAgent_OnCiMenuCallbackReceived(CiMenu menu)
    {
      if (OnCiMenuCallbackReceived != null)
      {
        OnCiMenuCallbackReceived(menu);
      }
    }

    #endregion

    public void ReConnect()
    {
      if (_eventServiceAgent == null)
      {
        CreateEventService();
        RegisterEventServiceIfNeeded();
        RegisterUserForServerEvents();
      }
    }

    private void RegisterUserForServerEvents()
    {
      if (OnCiMenuCallbackReceived != null)
      {
        ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForCiMenu(_hostname);
      }

      if (OnHeartbeatRequestReceived != null)
      {
        ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForHeartbeatMonitoring(_hostname);
      }

      if (OnTvServerEventReceived != null)
      {
        ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForTvServerEvents(_hostname);
      }
    }

    public void Disconnect()
    {
      if (_eventServiceAgent != null)
      {
        _eventServiceAgent.Dispose();
      }      
    }

    #region register/unregister callback events

    private int _isRunningRegisterCiMenuCallbacks;    
    public void RegisterCiMenuCallbacks(ICiMenuEventCallback handler)
    {
      if (Interlocked.Exchange(ref _isRunningRegisterCiMenuCallbacks, 1) == 1)
      {
        return;
      }
      try
      {
        if (OnCiMenuCallbackReceived == null)
        {
          Log.Info("EventServiceAgent: RegisterCiMenuCallbacks");
          OnCiMenuCallbackReceived += new CiMenuCallbackDelegate(handler.CiMenuCallback);
          RegisterEventServiceIfNeeded();
          ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForCiMenu(_hostname);
        }
      }
      catch (Exception ex)
      {
        OnCiMenuCallbackReceived = null;
        Log.Error("EventServiceAgent: RegisterCiMenuCallbacks exception = {0}", ex);
      }
      finally
      {
        Interlocked.Exchange(ref _isRunningRegisterCiMenuCallbacks, 0);
      }
    }

    private int _isRunningUnRegisterCiMenuCallbacks;
    public void UnRegisterCiMenuCallbacks(ICiMenuEventCallback handler, bool serverDown)
    {
      _serverDown = serverDown;
      if (Interlocked.Exchange(ref _isRunningUnRegisterCiMenuCallbacks, 1) == 1)
      {
        return;
      }
      try
      {
        if (OnCiMenuCallbackReceived != null)
        {
          Log.Info("EventServiceAgent: UnRegisterCiMenuCallbacks");
          OnCiMenuCallbackReceived -= new CiMenuCallbackDelegate(handler.CiMenuCallback);
          if (!serverDown)
          {
            UnRegisterEventServiceIfNeeded();
            ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForCiMenu(_hostname);
          }
        }
      }
      finally
      {
        Interlocked.Exchange(ref _isRunningUnRegisterCiMenuCallbacks, 0);
      }
    }

    private int _isRunningRegisterHeartbeatCallbacks;
    public void RegisterHeartbeatCallbacks(IHeartbeatEventCallbackClient handler)
    {
      if (Interlocked.Exchange(ref _isRunningRegisterHeartbeatCallbacks, 1) == 1)
      {
        return;
      }
      try
      {
        if (OnHeartbeatRequestReceived == null)
        {
          OnHeartbeatRequestReceived += new HeartbeatRequestReceivedDelegate(handler.HeartbeatRequestReceived);
          Log.Info("EventServiceAgent: RegisterHeartbeatCallbacks");
          RegisterEventServiceIfNeeded();
          ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForHeartbeatMonitoring(_hostname);
        }
      }
      catch (Exception ex)
      {
        OnHeartbeatRequestReceived = null;
        Log.Error("EventServiceAgent: RegisterHeartbeatCallbacks exception = {0}", ex);
      }
      finally
      {
        Interlocked.Exchange(ref _isRunningRegisterHeartbeatCallbacks, 0);
      }
    }

    private int _isRunningUnRegisterHeartbeatCallbacks;
    public void UnRegisterHeartbeatCallbacks(IHeartbeatEventCallbackClient handler, bool serverDown)
    {
      _serverDown = serverDown;
      if (Interlocked.Exchange(ref _isRunningUnRegisterHeartbeatCallbacks, 1) == 1)
      {
        return;
      }
      try
      {
        if (OnHeartbeatRequestReceived != null)
        {
          Log.Info("EventServiceAgent: UnRegisterHeartbeatCallbacks");
          OnHeartbeatRequestReceived -= new HeartbeatRequestReceivedDelegate(handler.HeartbeatRequestReceived);
          if (!serverDown)
          {
            UnRegisterEventServiceIfNeeded();
            ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForHeartbeatMonitoring(_hostname);
          }
        }
      }
      finally
      {
        Interlocked.Exchange(ref _isRunningUnRegisterHeartbeatCallbacks, 0);
      }
    }

    private int _isRunningRegisterTvServerEventCallbacks;
    public void RegisterTvServerEventCallbacks(ITvServerEventCallbackClient handler)
    {
      if (Interlocked.Exchange(ref _isRunningRegisterTvServerEventCallbacks, 1) == 1)
      {
        return;
      }
      try
      {
        if (OnTvServerEventReceived == null)
        {
          //System.Diagnostics.Debugger.Launch();
          Log.Info("EventServiceAgent: RegisterTvServerEventCallbacks");
          OnTvServerEventReceived += new TvServerEventReceivedDelegate(handler.CallbackTvServerEvent);
          RegisterEventServiceIfNeeded();
          ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForTvServerEvents(_hostname);
        }
      }
      catch (Exception ex)
      {
        OnTvServerEventReceived -= new TvServerEventReceivedDelegate(handler.CallbackTvServerEvent);
        Log.Error("EventServiceAgent: RegisterTvServerEventCallbacks exception = {0}", ex);
      }
      finally
      {
        Interlocked.Exchange(ref _isRunningRegisterTvServerEventCallbacks, 0);
      }
    }

    private int _isRunningUnRegisterTvServerEventCallbacks;
    public void UnRegisterTvServerEventCallbacks(ITvServerEventCallbackClient handler, bool serverDown)
    {
      _serverDown = serverDown;
      if (Interlocked.Exchange(ref _isRunningUnRegisterTvServerEventCallbacks, 1) == 1)
      {
        return;
      }
      try
      {
        if (OnTvServerEventReceived != null)
        {
          Log.Info("EventServiceAgent: UnRegisterTvServerEventCallbacks");
          OnTvServerEventReceived -= new TvServerEventReceivedDelegate(handler.CallbackTvServerEvent);
          if (!serverDown)
          {
            UnRegisterEventServiceIfNeeded();
            ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForTvServerEvents(_hostname);
          }
        }
      }
      finally
      {
        Interlocked.Exchange(ref _isRunningUnRegisterTvServerEventCallbacks, 0);
      }
    }
    #endregion


    private bool AreAnyEventHandlersInUse()
    {
      return (OnCiMenuCallbackReceived != null ||
              OnHeartbeatRequestReceived != null ||
              OnTvServerEventReceived != null);
    }

    private void RegisterEventServiceIfNeeded()
    {
      if (AreAnyEventHandlersInUse())
      {
        EventServiceAgent.Subscribe(_hostname);
      }
    }

    private void UnRegisterEventServiceIfNeeded()
    {
      if (OnCiMenuCallbackReceived == null && OnHeartbeatRequestReceived == null && OnTvServerEventReceived == null)
      {
        //if (IsConnectionReady ())
        if (_eventServiceAgent != null)
        {
          EventServiceAgent.Unsubscribe(_hostname); 
        }        
      }
    }

    private static bool IsConnectionReady(ICommunicationObject callback)
    {
      bool connectionReady = callback != null && (callback.State == CommunicationState.Opened);
      return connectionReady;
    }

  }
}