using System;
using System.Net;
using System.ServiceModel;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public class EventServiceAgent : ServiceAgent<IEventService>, IEventService, IServerEventCallback
  {
    #region events & delegates

    public delegate void HeartbeatRequestReceivedDelegate();
    public delegate void TvServerEventReceivedDelegate(TvServerEventArgs eventArgs);
    public delegate void CiMenuCallbackDelegate(CiMenu menu);

    public delegate void ConnectionLostDelegate();

    public event HeartbeatRequestReceivedDelegate OnHeartbeatRequestReceived;
    public event TvServerEventReceivedDelegate OnTvServerEventReceived;
    public event CiMenuCallbackDelegate OnCiMenuCallbackReceived;

    public event ConnectionLostDelegate OnConnectionLost;

    #endregion

    private readonly DuplexChannelFactory<IEventService> _channelFactory;    
    private static string _hostname;    

    #region ctor's
    /// <summary>
    /// EventServiceAgent
    /// </summary>
    public EventServiceAgent(string hostname)
    {
      _hostname = hostname;
      NetTcpBinding binding = ServiceHelper.GetTcpBinding();
      if (!String.IsNullOrWhiteSpace(_hostname))
      {
        var endpoint = new EndpointAddress(ServiceHelper.GetTcpEndpointURL(typeof(IEventService), _hostname));
        var callbackInstance = new InstanceContext(this);
        _channelFactory = new DuplexChannelFactory<IEventService>(callbackInstance, binding, endpoint);
        _channel = _channelFactory.CreateChannel();

        callbackInstance.Faulted += new EventHandler(EventServiceAgent_Faulted);
        callbackInstance.Closed += new EventHandler(EventServiceAgent_Closed);

        _channelFactory.Faulted += new EventHandler(EventServiceAgent_Faulted);
        _channelFactory.Closed += new EventHandler(EventServiceAgent_Closed);

        ((IClientChannel)_channel).Faulted += new EventHandler(EventServiceAgent_Faulted);
        ((IClientChannel)_channel).Closed += new EventHandler(EventServiceAgent_Closed);        
      }      
    }

    #endregion

    void EventServiceAgent_Closed(object sender, EventArgs e)
    {
      var comm = sender as ICommunicationObject;
      if (comm != null)
      {
       comm.Abort();
       comm.Close();
       Dispose();
      }      
    }

    void EventServiceAgent_Faulted(object sender, EventArgs e)
    {
      var comm = sender as ICommunicationObject;
      if (comm != null)
      {
        comm.Abort();
        comm.Close();
        Dispose();
      }    
    }

    private IEventService Channel
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
        AbortChannel();
      }
    }

    private void AbortChannel()
    {      
      _channelFactory.Abort();
      _channelFactory.Close();
      Dispose();
    }

    public void Unsubscribe(string username)
    {
      try
      {
        Channel.Unsubscribe(username);
      }
      catch (CommunicationObjectFaultedException)
      {
        AbortChannel();
      }
    }

    #endregion

    #region Implementation of IServerEventCallback

    public IAsyncResult BeginOnCallbackTvServerEvent(TvServerEventArgs eventArgs, AsyncCallback callback, object asyncState)
    {      
      Action<TvServerEventArgs> act = (tvServerEventArgs) =>
                                        {
                                          if (OnTvServerEventReceived != null)
                                          {
                                            OnTvServerEventReceived(eventArgs);
                                          }                                          
                                        };        
      return act.BeginInvoke(eventArgs, callback, asyncState); 
    }

    public void EndOnCallbackTvServerEvent(IAsyncResult result)
    {
      var act = (Action<TvServerEventArgs>)((System.Runtime.Remoting.Messaging.AsyncResult)result).AsyncDelegate;
      act.EndInvoke(result);
    }

    public IAsyncResult BeginOnCallbackCiMenuEvent(CiMenu menu, AsyncCallback callback, object asyncState)
    {
      Action<CiMenu> act = (ciMenu) =>
      {
        if (OnCiMenuCallbackReceived != null)
        {
          OnCiMenuCallbackReceived(ciMenu);
        }
      };
      return act.BeginInvoke(menu, callback, asyncState); 
    }

    public void EndOnCallbackCiMenuEvent(IAsyncResult result)
    {
      var act = (Action<CiMenu>)((System.Runtime.Remoting.Messaging.AsyncResult)result).AsyncDelegate;
      act.EndInvoke(result);
    }

    public IAsyncResult BeginOnCallbackHeartBeatEvent(AsyncCallback callback, object asyncState)
    {
      Action act = () =>
      {
        if (OnHeartbeatRequestReceived != null)
        {
          OnHeartbeatRequestReceived();
        }
      };
      return act.BeginInvoke(callback, asyncState); 
    }

    public void EndOnCallbackHeartBeatEvent(IAsyncResult result)
    {
      var act = (Action)((System.Runtime.Remoting.Messaging.AsyncResult)result).AsyncDelegate;
      act.EndInvoke(result);
    }

    #endregion    

    public override void Dispose ()
    {
      base.Dispose();
      if (OnConnectionLost != null)
      {
        OnConnectionLost();
      }
    }
  }

}
