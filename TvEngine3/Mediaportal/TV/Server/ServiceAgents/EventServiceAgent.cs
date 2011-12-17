using System;
using System.Net;
using System.ServiceModel;
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
  }

  public class EventServiceAgent : IEventServiceAgent
  {
    #region events & delegates

    private delegate void HeartbeatRequestReceivedDelegate();
    private delegate void TvServerEventReceivedDelegate(TvServerEventArgs eventArgs);
    private delegate void CiMenuCallbackDelegate(CiMenu menu);

    private static event HeartbeatRequestReceivedDelegate OnHeartbeatRequestReceived;
    private static event TvServerEventReceivedDelegate OnTvServerEventReceived;
    private static event CiMenuCallbackDelegate OnCiMenuCallbackReceived;

    #endregion

    #region private members

    private readonly DuplexChannelFactory<IEventService> _channelFactory;
    private readonly IEventService _channel;
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
      var endpoint = new EndpointAddress(ServiceHelper.GetTcpEndpointURL(typeof(IEventService), _hostname));
      _channelFactory = new DuplexChannelFactory<IEventService>(new InstanceContext(this), binding, endpoint);
      _channel = _channelFactory.CreateChannel();
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


    private static bool AreAnyEventHandlersInUse ()
    {
      return (OnCiMenuCallbackReceived != null ||
              OnHeartbeatRequestReceived != null ||
              OnTvServerEventReceived != null);
    }    

    private static void RegisterEventServiceIfNeeded()
    {
      if (AreAnyEventHandlersInUse())
      {
        ServiceAgents.Instance.EventServiceAgent.Subscribe(_hostname);
      }
    }

    private static void UnRegisterEventServiceIfNeeded()
    {
      if (!AreAnyEventHandlersInUse())
      {
        ServiceAgents.Instance.EventServiceAgent.Unsubscribe(_hostname);
      }
    }
    

    public static void RegisterCiMenuCallbacks(ICiMenuEventCallback handler)
    {
      Log.Info("EventServiceAgent: RegisterCiMenuCallbacks");
      try
      {
        RegisterEventServiceIfNeeded();
        ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForCiMenu(_hostname);
        OnCiMenuCallbackReceived -= new CiMenuCallbackDelegate(handler.CiMenuCallback);
        OnCiMenuCallbackReceived += new CiMenuCallbackDelegate(handler.CiMenuCallback);
      }
      catch (Exception ex)
      {
        Log.Error("EventServiceAgent: RegisterCiMenuCallbacks exception = {0}", ex);
      }
    }


    public static void UnRegisterCiMenuCallbacks(ICiMenuEventCallback handler)
    {
      Log.Info("EventServiceAgent: UnRegisterCiMenuCallbacks");
      ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForCiMenu(_hostname);
      OnCiMenuCallbackReceived -= new CiMenuCallbackDelegate(handler.CiMenuCallback);
      UnRegisterEventServiceIfNeeded();
    }

    public static void RegisterHeartbeatCallbacks(IHeartbeatEventCallback handler)
    {
      Log.Info("EventServiceAgent: RegisterHeartbeatCallbacks");
      try
      {
        RegisterEventServiceIfNeeded();
        ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForHeartbeatMonitoring(_hostname);
        OnHeartbeatRequestReceived -= new HeartbeatRequestReceivedDelegate(handler.HeartbeatRequestReceived);
        OnHeartbeatRequestReceived += new HeartbeatRequestReceivedDelegate(handler.HeartbeatRequestReceived);
      }
      catch (Exception ex)
      {
        Log.Error("EventServiceAgent: RegisterHeartbeatCallbacks exception = {0}", ex);
      }
    }

    public static void UnRegisterHeartbeatCallbacks(IHeartbeatEventCallback handler)
    {
      Log.Info("EventServiceAgent: UnRegisterHeartbeatCallbacks");
      ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForHeartbeatMonitoring(_hostname);
      OnHeartbeatRequestReceived -= new HeartbeatRequestReceivedDelegate(handler.HeartbeatRequestReceived);
      UnRegisterEventServiceIfNeeded();
    }

    public static void RegisterTvServerEventCallbacks(ITvServerEventCallback handler)
    {
      Log.Info("EventServiceAgent: RegisterTvServerEventCallbacks");
      try
      {
        RegisterEventServiceIfNeeded();
        ServiceAgents.Instance.ControllerServiceAgent.RegisterUserForTvServerEvents(_hostname);
        OnTvServerEventReceived -= new TvServerEventReceivedDelegate(handler.CallbackTvServerEvent);
        OnTvServerEventReceived += new TvServerEventReceivedDelegate(handler.CallbackTvServerEvent);        
      }
      catch (Exception ex)
      {
        Log.Error("EventServiceAgent: RegisterTvServerEventCallbacks exception = {0}", ex);
      }
    }

    public static void UnRegisterTvServerEventCallbacks(ITvServerEventCallback handler)
    {
      Log.Info("EventServiceAgent: UnRegisterTvServerEventCallbacks");
      ServiceAgents.Instance.ControllerServiceAgent.UnRegisterUserForTvServerEvents(_hostname);
      OnTvServerEventReceived -= new TvServerEventReceivedDelegate(handler.CallbackTvServerEvent);
      UnRegisterEventServiceIfNeeded();
    }
  }

}
