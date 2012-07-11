using System;
using System.Net;
using System.ServiceModel;
using System.Threading;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public interface IEventServiceAgent : IEventService, IServerEventCallback
  {
    IEventService Channel { get; }
    void RegisterCiMenuCallbacks(ICiMenuEventCallback handler);
    void UnRegisterCiMenuCallbacks(ICiMenuEventCallback handler);
    void RegisterHeartbeatCallbacks(IHeartbeatEventCallback handler);
    void UnRegisterHeartbeatCallbacks(IHeartbeatEventCallback handler);
    void RegisterTvServerEventCallbacks(ITvServerEventCallback handler);
    void UnRegisterTvServerEventCallbacks(ITvServerEventCallback handler);
  }

  public class EventServiceAgent : ServiceAgent<IEventService>, IEventServiceAgent
  {
    #region events & delegates

    private delegate void HeartbeatRequestReceivedDelegate();
    private delegate void TvServerEventReceivedDelegate(TvServerEventArgs eventArgs);
    private delegate void CiMenuCallbackDelegate(CiMenu menu);

    private event HeartbeatRequestReceivedDelegate OnHeartbeatRequestReceived;
    private event TvServerEventReceivedDelegate OnTvServerEventReceived;
    private event CiMenuCallbackDelegate OnCiMenuCallbackReceived;

    #endregion

    #region private members

    private readonly DuplexChannelFactory<IEventService> _channelFactory;
    //private readonly IEventService _channel;
    private static string _hostname;

    #endregion

    #region ctor's
    /// <summary>
    /// EventServiceAgent
    /// </summary>
    public EventServiceAgent(string hostname) 
    {
      _hostname = hostname;
      NetTcpBinding binding = ServiceHelper.GetTcpBinding();
      if (!String.IsNullOrEmpty(_hostname))
      {
        var endpoint = new EndpointAddress(ServiceHelper.GetTcpEndpointURL(typeof(IEventService), _hostname));
        _channelFactory = new DuplexChannelFactory<IEventService>(new InstanceContext(this), binding, endpoint);
        _channel = _channelFactory.CreateChannel();
      }
    }

    #endregion

    public IEventService Channel
    {
      get
      {        
        return _channel;
      }
    }

    #region Implementation of IEventService

    public void Subscribe(string username)
    {
      try
      {
        Channel.Subscribe(username);
      }
      catch (CommunicationObjectFaultedException)
      {
        _channelFactory.Abort();        
      }      
    }

    public void Unsubscribe(string username)
    {
      try
      {
        Channel.Unsubscribe(username);
      }
      catch (CommunicationObjectFaultedException)
      {
        _channelFactory.Abort();        
      }
    }

    #endregion

    #region Implementation of ITvServerEventCallbacks

    public void CallbackTvServerEvent(TvServerEventArgs eventArgs)
    {
      if (OnTvServerEventReceived != null)
      {
        OnTvServerEventReceived(eventArgs);
      }
    }

    public void HeartbeatRequestReceived()
    {
      if (OnHeartbeatRequestReceived != null)
      {
        OnHeartbeatRequestReceived();
      }
    }

    public void Ping()
    {
      
    }

    #endregion

    #region Implementation of ICiMenuCallback

    public void CiMenuCallback(CiMenu menu)
    {
      if (OnCiMenuCallbackReceived != null)
      {
        OnCiMenuCallbackReceived(menu);
      }
    }

    #endregion


    private bool AreAnyEventHandlersInUse ()
    {
      return (OnCiMenuCallbackReceived != null ||
              OnHeartbeatRequestReceived != null ||
              OnTvServerEventReceived != null);
    }    

    private void RegisterEventServiceIfNeeded()
    {
      if (AreAnyEventHandlersInUse())      
      {
        Subscribe(_hostname);
      }
    }

    private void UnRegisterEventServiceIfNeeded()
    {
      if (OnCiMenuCallbackReceived == null && OnHeartbeatRequestReceived == null && OnTvServerEventReceived == null)      
      {
        Unsubscribe(_hostname);
      }
    }
    
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
    public void UnRegisterCiMenuCallbacks(ICiMenuEventCallback handler)
    {
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
          ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForCiMenu(_hostname);
          UnRegisterEventServiceIfNeeded();          
        }

      }      
      finally
      {
        Interlocked.Exchange(ref _isRunningUnRegisterCiMenuCallbacks, 0);
      }

    }
    
    private int _isRunningRegisterHeartbeatCallbacks;
    public void RegisterHeartbeatCallbacks(IHeartbeatEventCallback handler)
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
    public void UnRegisterHeartbeatCallbacks(IHeartbeatEventCallback handler)
    {
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
          ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForHeartbeatMonitoring(_hostname);
          UnRegisterEventServiceIfNeeded();          
        }        
      }
      finally
      {
        Interlocked.Exchange(ref _isRunningUnRegisterHeartbeatCallbacks, 0);
      }

    }

    private int _isRunningRegisterTvServerEventCallbacks;    
    public void RegisterTvServerEventCallbacks(ITvServerEventCallback handler)
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
    public void UnRegisterTvServerEventCallbacks(ITvServerEventCallback handler)
    {
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
          ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForTvServerEvents(_hostname);
          UnRegisterEventServiceIfNeeded();
        }
      } 
      finally
      {
        Interlocked.Exchange(ref _isRunningUnRegisterTvServerEventCallbacks, 0);
      }      
    }
  }

}
