using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;

namespace Mediaportal.TV.Server.TVLibrary.EventDispatchers
{
  public class TvServerEventDispatcher : EventDispatcher
  {


    private void OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      var tvEvent = eventArgs as TvServerEventArgs;
      if (tvEvent != null)
      {              
        try
        {
          IDictionary<string, DateTime> usersCopy = GetUsersCopy();

          if (usersCopy.Count > 0)
          {            
            if (tvEvent.EventType == TvServerEventType.ChannelStatesChanged)
            {
              foreach (string username in usersCopy.Keys)
              {
                //todo channel states for idle users should ideally only be pushed out to idle users and not all users.
                if (tvEvent.User.Name.Equals(username))
                {
                  EventService.CallbackTvServerEvent(username, tvEvent);
                }
                else if (tvEvent.User.Name.Equals("idle"))
                {
                  bool isTimeShifting = ServiceManager.Instance.InternalControllerService.IsTimeShifting(username);
                  if (!isTimeShifting)
                  {
                    EventService.CallbackTvServerEvent(username, tvEvent);
                  }
                }
              }
            }
            else
            {
              //todo : filter out any events raised by the same user.. eg. user1 doesnt care about events caused by user1. like timeshiftingstarted etc.
              foreach (string username in usersCopy.Keys)
              {
                EventService.CallbackTvServerEvent(username, tvEvent);
              }
            }
          }
          else
          {
            this.LogDebug("TvServerEventDispatcher.DoOnTvServerEventAsynch : tvserver event received but no users found...");
          }
        }
        catch (Exception ex)
        {
          this.LogDebug("DoOnTvServerEventAsynch failed : {0}", ex);        
        }            
      }
    }

    #region Overrides of EventDispatcher

    public override void Start()
    {
      this.LogInfo("TvServerEventDispatcher: start");
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent -= new TvServerEventHandler(OnTvServerEvent);
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent += new TvServerEventHandler(OnTvServerEvent);
    }

    public override void Stop()
    {
      this.LogInfo("TvServerEventDispatcher: stop");
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent -= new TvServerEventHandler(OnTvServerEvent);
    }

    #endregion
  }
}
