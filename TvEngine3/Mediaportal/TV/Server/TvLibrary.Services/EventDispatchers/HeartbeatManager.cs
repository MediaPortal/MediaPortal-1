using System;
using System.Collections.Generic;
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.EventDispatchers
{
  public class HeartbeatManager : EventDispatcher
  {
    private const int HEARTBEAT_REQUEST_INTERVAL_SECS = 15;
    private const int HEARTBEAT_MAX_SECS_EXCEED_ALLOWED = 30;

    private Thread _requestHeartBeatThread;
    private Thread _heartBeatMonitorThread;
    private static readonly ManualResetEvent _evtHeartbeatCtrl = new ManualResetEvent(false);        
    private readonly object _disconnectedHeartbeatUsersLock = new object();        
    private readonly IDictionary<string, DateTime> _diconnectedHeartbeatUsers = new Dictionary<string, DateTime>();    

    ~HeartbeatManager()
    {
      StopHeartbeatThreads();      
    }

    private void UserDisconnectedFromService(string username)
    {
      //bool isAnyCardParkedByUser = ServiceManager.Instance.InternalControllerService.IsAnyCardParkedByUser(username);
      //if (!isAnyCardParkedByUser)
      {
        lock (_disconnectedHeartbeatUsersLock)
        {
          _diconnectedHeartbeatUsers[username] = DateTime.Now;
        }
      }
      //else
     // {
      //  Log.Info("HeartbeatManager: will not monitor parked user '{0}'", username);
     // }
    }

    #region public methods
    
    public override void Start()
    {
      Log.Info("HeartbeatManager: start");
      SetupHeartbeatThreads();      
      EventService.UserDisconnectedFromService += UserDisconnectedFromService;
    }

    public override void Stop()
    {
      Log.Info("HeartbeatManager: stop");
      StopHeartbeatThreads();
      EventService.UserDisconnectedFromService -= UserDisconnectedFromService;
    }

    #endregion

    private void SetupHeartbeatThreads()
    {            
      StopHeartbeatThreads();

      _evtHeartbeatCtrl.Reset();
      _requestHeartBeatThread = new Thread(RequestHeartBeatThread) { Name = "RequestHeartBeatThread", IsBackground = true };
      _requestHeartBeatThread.Start();

      _heartBeatMonitorThread = new Thread(HeartBeatMonitorThread) { Name = "HeartBeatMonitorThread", IsBackground = true };
      _heartBeatMonitorThread.Start();
    }

    private void StopHeartbeatThreads()
    {
      _evtHeartbeatCtrl.Set();
      if (_requestHeartBeatThread != null && _requestHeartBeatThread.IsAlive)
      {
        try
        {          
          _requestHeartBeatThread.Join();
        }
        catch (Exception) { }
      }
      
      if (_heartBeatMonitorThread != null && _heartBeatMonitorThread.IsAlive)
      {
        try
        {          
          _heartBeatMonitorThread.Join();
        }
        catch (Exception) { }
      }      
    }

    private void HeartBeatMonitorThread()
    {
      while (!_evtHeartbeatCtrl.WaitOne(HEARTBEAT_REQUEST_INTERVAL_SECS * 1000))
      {
        try
        {
          IDictionary<string, DateTime> diconnectedHeartbeatUsersCopy;
          lock (_disconnectedHeartbeatUsersLock)
          {
            diconnectedHeartbeatUsersCopy = new Dictionary<string, DateTime>(_diconnectedHeartbeatUsers);
          }

          foreach (KeyValuePair<string, DateTime> kvp in diconnectedHeartbeatUsersCopy)
          {
            string username = kvp.Key;
            DateTime disconnectionTime = kvp.Value;

            bool isUserRegistered;
            DateTime lastSeen;
            lock (_usersLock)
            {
              isUserRegistered = _users.TryGetValue(username, out lastSeen);
            }

            if (isUserRegistered)
            {
              DateTime now = DateTime.Now;
              TimeSpan ts = lastSeen - now;

              // more than 30 seconds have elapsed since last heartbeat was received. lets kick the client
              if (ts.TotalSeconds < (-1 * HEARTBEAT_MAX_SECS_EXCEED_ALLOWED))
              {
                Log.Write("HeartbeatManager: idle user found: {0}", username);
                IDictionary<int, ITvCardHandler> cards = ServiceManager.Instance.InternalControllerService.CardCollection;
                foreach (ITvCardHandler card in cards.Values)
                {                  
                  IDictionary<string, IUser> users = card.UserManagement.UsersCopy;
                  IUser tmpUser;
                  bool foundUser = users.TryGetValue(username, out tmpUser);
                  if (foundUser)
                  {                    
                    int timeshiftingChannelId = card.UserManagement.GetTimeshiftingChannelId(tmpUser.Name);
                    if (timeshiftingChannelId > 0)
                    {
                      Log.Write("Controller: Heartbeat Monitor - kicking idle user {0}", tmpUser.Name);
                      ServiceManager.Instance.InternalControllerService.StopTimeShifting(ref tmpUser,
                                                                                       TvStoppedReason.HeartBeatTimeOut, timeshiftingChannelId);
                    }
                    
                    lock (_disconnectedHeartbeatUsersLock)
                    {
                      if (_diconnectedHeartbeatUsers.ContainsKey(username))
                      {
                        _diconnectedHeartbeatUsers.Remove(username);
                      }
                    }                    
                    break;
                  }
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("HeartbeatManager: HeartBeatMonitorThread exception - {0}", ex);
        }
      }
      Log.Info("HeartbeatManager: HeartBeatMonitorThread stopped...");
    }

    private void RequestHeartBeatThread()
    {
      //#if !DEBUG      
      while (!_evtHeartbeatCtrl.WaitOne(HEARTBEAT_REQUEST_INTERVAL_SECS * 1000))
      {
        IList<string> updateUsers = new List<string>();
        try
        {
          IDictionary<string, DateTime> usersCopy = GetUsersCopy();
          foreach (KeyValuePair<string, DateTime> heartbeatuser in usersCopy)
          {
            if (EventService.CallbackRequestHeartbeat(heartbeatuser.Key))
            {
              updateUsers.Add(heartbeatuser.Key);
            }            
          }                    
        }
        catch(Exception ex)
        {
          Log.Error("HeartbeatManager: RequestHeartBeatThread exception - {0}", ex);
        }
        finally
        {
          foreach (var updateUser in updateUsers)
          {
            _users[updateUser] = DateTime.Now;
          }
        }
      }
      Log.Info("HeartbeatManager: RequestHeartBeatThread stopped...");
      //#endif
    }

  }
}
