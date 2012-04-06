using System;
using System.Collections.Generic;
using System.Threading;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Services;

namespace Mediaportal.TV.Server.TVService.EventDispatchers
{
  public class TvServerEventDispatcher : EventDispatcher
  {            
    private void OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      var tvEvent = eventArgs as TvServerEventArgs;
      if (tvEvent != null)
      {
        ThreadPool.QueueUserWorkItem(delegate
                                       {
                                         DoOnTvServerEventAsynch(tvEvent);
                                       });        
      }
    }

    private void DoOnTvServerEventAsynch(TvServerEventArgs tvEvent)
    {
      try
      {
        IDictionary<string, DateTime> usersCopy = GetUsersCopy();

        if (tvEvent.EventType == TvServerEventType.ChannelStatesChanged)
        {
          foreach (string username in usersCopy.Keys)
          {
            //todo channel states for idle users should ideally only be pushed out to idle users and not all users.
            if (tvEvent.User.Name.Equals(username) || tvEvent.User.Name.Equals("idle"))
            {
              EventService.CallbackTvServerEvent(username, tvEvent);
            }
          }
        }
        else
        {
          foreach (string username in usersCopy.Keys)
          {
            EventService.CallbackTvServerEvent(username, tvEvent);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Debug("DoOnTvServerEventAsynch failed : {0}", ex);        
      }
    }

    #region Overrides of EventDispatcher

    public override void Start()
    {
      Log.Info("TvServerEventDispatcher: start");
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent -= new TvServerEventHandler(OnTvServerEvent);
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent += new TvServerEventHandler(OnTvServerEvent);
    }

    public override void Stop()
    {
      Log.Info("TvServerEventDispatcher: stop");
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent -= new TvServerEventHandler(OnTvServerEvent);
    }

    #endregion
  }
}
