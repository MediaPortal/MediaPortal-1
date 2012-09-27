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
using System.Linq;
using System.Threading;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public class ParkedUserManagement : IParkedUserManagement, IDisposable
  {
    private readonly TvCardHandler _cardHandler;
    private readonly object _parkedUsersLock = new object();
    private readonly int _parkedStreamTimeout;

    public ParkedUserManagement(TvCardHandler tvCardHandler)
    {
      _cardHandler = tvCardHandler;
      _parkedStreamTimeout = Int32.Parse(SettingsManagement.GetSetting("parkedStreamTimeout", "5").Value)*60*1000;
    }

    private ITvCardContext Context
    {
      get { return _cardHandler.Card.Context as ITvCardContext; }
    }


    public ParkedUser GetUser(string name)
    {
      ParkedUser existingUser;
      lock (_parkedUsersLock)
      {
        Context.ParkedUsers.TryGetValue(name, out existingUser);
      }
      return existingUser;
    }

    public void ParkUser(ref IUser user, double duration, int idChannel)
    {
      if (_cardHandler.DataBaseCard.Enabled)
      {
        SetSubChannelStatusParked(user, idChannel);

        ParkedUser parkedUser = GetUser(user.Name);
        if (parkedUser != null)
        {
          lock (_parkedUsersLock)
          {
            foreach (ISubChannel subchannel in GetSubChannels(user))
            {
              if (!parkedUser.SubChannels.ContainsKey(subchannel.Id))
              {                
                if (subchannel.TvUsage == TvUsage.Parked)
                {
                  if (!parkedUser.Events.ContainsKey(subchannel.Id))
                  {
                    var evt = new ManualResetEvent(false);
                    parkedUser.Events.Add(subchannel.Id, evt);
                  }
                  if (!parkedUser.ParkedAtList.ContainsKey(subchannel.Id))
                  {
                    parkedUser.ParkedAtList.Add(subchannel.Id, DateTime.Now);
                  }
                  if (!parkedUser.ParkedDurations.ContainsKey(subchannel.Id))
                  {
                    parkedUser.ParkedDurations.Add(subchannel.Id, duration);
                  }
                }
              }
            }
            parkedUser.SubChannels = user.SubChannels;
          }
        }
        else
        {
          parkedUser = new ParkedUser(user);
          int subChannelByChannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(parkedUser.Name, idChannel);
          lock (_parkedUsersLock)
          {
            parkedUser.ParkedDurations[subChannelByChannelId] = duration;
            parkedUser.ParkedAtList[subChannelByChannelId] = DateTime.Now;
          }
          AddParkedUser(parkedUser);          
        }

        ThreadPool.QueueUserWorkItem(delegate
                                        {
                                          try
                                          {
                                            HandleParkedUserTimeOutThread(parkedUser, idChannel);
                                          }
                                          catch (Exception ex)
                                          {
                                            Log.Error("HandleParkedUserTimeOutThread exception : {0}", ex);
                                          }
                                        });
        
      }
    }

    private void SetSubChannelStatusParked(IUser user, int idChannel)
    {
      ISubChannel subch = _cardHandler.UserManagement.GetSubChannelByChannelId(user.Name, idChannel);
      if (subch != null)
      {
        subch.TvUsage = TvUsage.Parked;
      }
    }


    private void HandleParkedUserTimeOutThread(ParkedUser parkedUser, int channelId)
    {
      ManualResetEvent evt;
      bool hasEvent;
      lock (_parkedUsersLock)
      {
        hasEvent =
          parkedUser.Events.TryGetValue(
            _cardHandler.UserManagement.GetSubChannelIdByChannelId(parkedUser.Name, channelId), out evt);
      }
      if (hasEvent)
      {
        if (!evt.WaitOne(_parkedStreamTimeout))
        {
          Log.Debug("HandleParkedUserTimeOutThread: removing idle parked channel '{0}' for user '{1}'", channelId,
                    parkedUser.Name);
          var user = parkedUser as IUser;
          ServiceManager.Instance.InternalControllerService.StopTimeShifting(ref user, channelId);
        }
        else
        {
          Log.Debug("HandleParkedUserTimeOutThread: stopping timeout event for channel '{0}' for user '{1}'", channelId,
                    parkedUser.Name);
        }
        Log.Debug("HandleParkedUserTimeOutThread: dispose event");        
        RemoveChannelOrParkedUser(parkedUser, channelId);
        evt.Dispose();
      }
    }

    public void UnParkUser(ref IUser user, double duration, int idChannel)
    {
      if (_cardHandler.DataBaseCard.Enabled)
      {
        bool hasParkedUser = false;
        lock (_parkedUsersLock)
        {
          if (Context != null)
          {
            foreach (ParkedUser parkedUser in Context.ParkedUsers.Values)
            {
              foreach (ISubChannel subchannel in GetSubChannels(parkedUser))
              {
                double durationFound;
                bool parkedDurationFound = parkedUser.ParkedDurations.TryGetValue(subchannel.Id, out durationFound);
                if (parkedDurationFound && durationFound.Equals(duration))
                {
                  if (subchannel.IdChannel == idChannel && subchannel.TvUsage == TvUsage.Parked)
                  {
                    hasParkedUser = true;
                    IUser userUpdated = _cardHandler.UserManagement.GetUserCopy(user.Name);
                    if (userUpdated != null && user.Name == parkedUser.Name)
                    {
                      SetTimeShiftingStatusToIdenticalUser(idChannel, userUpdated);
                    }
                    else
                    {
                      if (userUpdated == null)
                      {
                        userUpdated = user.Clone() as IUser;
                      }                      
                      //remove any existing parks done by the parking user, if they are done on the same channel.
                      RemoveExistingParksOnUser(user.Name, idChannel, subchannel, userUpdated);
                      _cardHandler.UserManagement.AddSubChannelOrUser(userUpdated, idChannel, subchannel.Id);
                    }
                    user = userUpdated;
                    HandleUserOwnerShip(user, subchannel, parkedUser);

                    Log.Info("UnParkUser: {0} - {1} - {2}", user.Name, duration, idChannel);
                    CancelParkedTimeoutEvent(subchannel.Id, parkedUser);
                    RemoveChannelFromParkedUser(idChannel, parkedUser);
                    break;
                  }
                }
              }
              if (hasParkedUser)
              {
                break;
              }
            }
          }
        }
      }
    }

    private void SetTimeShiftingStatusToIdenticalUser(int idChannel, IUser user)
    {
      ISubChannel subch = GetParkedSubChannelByChannelId(GetSubChannels(user), idChannel);
      if (subch != null)
      {
        subch.TvUsage = TvUsage.Timeshifting;
      }
    }

    private static void CancelParkedTimeoutEvent(int subchannelId, ParkedUser parkedUser)
    {
      ManualResetEvent evt;
      bool hasEvent = parkedUser.Events.TryGetValue(subchannelId, out evt);
      if (hasEvent)
      {
        Log.Debug("CancelParkedTimeoutEvent on subch={0} - parkeduser={}", subchannelId, parkedUser.Name);
        evt.Set();
      }
      else
      {
        throw new Exception("could not find associated event for parked user's subchannel");
      }
    }

    private void HandleUserOwnerShip(IUser user, ISubChannel subchannel, ParkedUser parkedUser)
    {
      bool wasCurrentUserOwner = WasCurrentUserOwner(parkedUser, subchannel);
      if (wasCurrentUserOwner) //inherit ownership
      {
        Context.OwnerSubChannel.OwnerName = user.Name;
      }
    }

    private void RemoveExistingParksOnUser(string userName, int idChannel, ISubChannel subchannel, IUser userUpdated)
    {
      ISubChannel subch;
      bool foundsubch = userUpdated.SubChannels.TryGetValue(subchannel.Id, out subch);
      if (foundsubch && subch.IdChannel == idChannel && subch.TvUsage == TvUsage.Parked)
      {
        userUpdated.SubChannels.Remove(subchannel.Id);
        CancelParkedUserByChannelId(userName, idChannel);
      }
    }

    private bool WasCurrentUserOwner(ParkedUser parkedUser, ISubChannel subchannel)
    {
      return (Context.OwnerSubChannel.OwnerSubChannelId == subchannel.Id &&
              Context.OwnerSubChannel.OwnerName == parkedUser.Name);
    }


    private void RemoveChannelFromParkedUser(int idChannel, ParkedUser parkedUser)
    {
      var origParkedUserUpdate = parkedUser as IUser;
      _cardHandler.UserManagement.RefreshUser(ref origParkedUserUpdate);
      if (origParkedUserUpdate != null)
      {
        ISubChannel subch = GetParkedSubChannelByChannelId(origParkedUserUpdate, idChannel);
        if (subch != null)
        {
          int subChannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(origParkedUserUpdate.Name, idChannel);
          _cardHandler.UserManagement.RemoveChannelFromUser(origParkedUserUpdate, subChannelId);
          if (subChannelId == -1)
          {
            throw new Exception("UnParkUser - could not find subChannelId from idChannel.");
          }
        }     
      }
    }

    public bool IsUserParkedOnAnyChannel(string userName)
    {
      lock (_parkedUsersLock)
      {
        if (Context.ParkedUsers.Values.Any(user => user.Name == userName))
        {
          return true;
        }
      }
      return false;
    }

    public bool IsUserParkedOnChannel(string userName, int idChannel)
    {
      double parkedDuration;
      DateTime parkedAt;
      return IsUserParkedOnChannel(userName, idChannel, out parkedDuration, out parkedAt);
    }

    public bool IsUserParkedOnChannel(string userName, int idChannel, out double parkedDuration, out DateTime parkedAt)
    {
      parkedDuration = 0;
      parkedAt = DateTime.MinValue;
      lock (_parkedUsersLock)
      {
        ParkedUser existingParkedUser = GetUser(userName);
        if (existingParkedUser != null)
        {
          int subChannelId = GetParkedSubChannelIdByChannelId(existingParkedUser, idChannel);
          if (subChannelId > -1)
          {
            existingParkedUser.ParkedDurations.TryGetValue(subChannelId, out parkedDuration);
            existingParkedUser.ParkedAtList.TryGetValue(subChannelId, out parkedAt);
            return true;
          }           
        }
      }
      return false;
    }

    public void CancelParkedUserBySubChannelId(string name, int subchannelId)
    {
      ParkedUser user = GetUser(name);
      if (user != null)
      {
        ISubChannel subch;
        bool hasSubChannel = user.SubChannels.TryGetValue(subchannelId, out subch);
        if (hasSubChannel)
        {
          CancelParkedTimeoutEvent(subchannelId, user);
          //var userCopy = user.Clone() as IUser;
          //_cardHandler.TimeShifter.Stop(ref userCopy, subch.IdChannel);
        }        
      }      
    }

    public void CancelAllParkedUsers()
    {
      ICollection<ParkedUser> parkedUsersCopy;
      lock (_parkedUsersLock)
      {
        parkedUsersCopy = new List<ParkedUser>(Context.ParkedUsers.Values);
      }

      foreach (ParkedUser parkedUser in parkedUsersCopy)
      {
        CancelAllParkedChannelsForUser(parkedUser.Name);
        var userCopy = parkedUser.Clone() as IUser;
        if (userCopy != null)
        {
          StopTimeShiftingAllParkedSubChannels(userCopy);
        }
      }
    }

    private void StopTimeShiftingAllParkedSubChannels(IUser user)
    {
      foreach (ISubChannel subch in GetSubChannels(user))
      {
        if (subch.TvUsage == TvUsage.Parked)
        {
          _cardHandler.TimeShifter.Stop(ref user, subch.IdChannel);
        }
      }
    }

    private static IEnumerable<ISubChannel> GetSubChannels(IUser user)
    {
      return user.SubChannels.Values;
    }

    public bool HasAnyParkedUsers()
    {
      lock (_parkedUsersLock)
      {
        return (Context.ParkedUsers.Count > 0);
      }
    }

    private void CancelAllParkedChannelsForUser(string userName)
    {
      lock (_parkedUsersLock)
      {
        ParkedUser parkedUser = GetUser(userName);
        if (parkedUser != null)
        {
          CancelAllParkedTimeoutEvents(userName, parkedUser);
        }
      }
    }

    private static void CancelAllParkedTimeoutEvents(string userName, ParkedUser parkedUser)
    {
      foreach (ManualResetEvent evt in parkedUser.Events.Values)
      {
        Log.Info("CancelParkedUserByChannelId: {0}", userName);
        evt.Set();
      }
    }

    private ISubChannel GetParkedSubChannelByChannelId(IEnumerable<ISubChannel> subChannels, int idChannel)
    {
      ISubChannel subChannel = null;
      foreach (ISubChannel subch in subChannels)
      {
        if (subch.IdChannel == idChannel && subch.TvUsage == TvUsage.Parked)
        {
          subChannel = subch;
          break;
        }
      }
      return subChannel;
    }

    private ISubChannel GetParkedSubChannelByChannelId(IUser parkedUser, int idChannel)
    {            
      return GetParkedSubChannelByChannelId(GetSubChannels(parkedUser), idChannel); 
    }

    private int GetParkedSubChannelIdByChannelId (IUser parkedUser, int idChannel)
    {
      int subChannelId = -1;
      ISubChannel subchannel = GetParkedSubChannelByChannelId(parkedUser, idChannel);
      if (subchannel != null)
      {
        subChannelId = subchannel.Id;
      }
      return subChannelId;            
    }

    private void CancelParkedUserByChannelId(string name, int idChannel)
    {
      ParkedUser parkedUser = GetUser(name);
      if (parkedUser != null)
      {
        int subChannelId = GetParkedSubChannelIdByChannelId(parkedUser, idChannel);
        if (subChannelId > -1)
        {
          CancelParkedTimeoutEvent(subChannelId, parkedUser);          
        }        
      }
    }

    public bool IsAnyUserParkedOnChannel(int idChannel)
    {
      lock (_parkedUsersLock)
      {
        foreach (ParkedUser parkedUser in Context.ParkedUsers.Values)
        {
          int subChannelId = GetParkedSubChannelIdByChannelId(parkedUser, idChannel);
          if (subChannelId > -1)
          {
            return true;
          }                      
        }
      }
      return false;
    }

    public void Dispose()
    {
      CancelAllParkedUsers();
    }

    public bool HasParkedUserWithDuration(int channelId, double duration)
    {
      bool hasParkedUserWithDuration = false;
      IEnumerable<int>  subchannelIds =_cardHandler.UserManagement.GetAllSubChannelForChannel(channelId, TvUsage.Parked);
      foreach (int subchannelId in subchannelIds)
      {
        lock (_parkedUsersLock)
        {
          foreach (ParkedUser parkedUser in Context.ParkedUsers.Values)
          {
            double durationFound;
            bool hasDuration = parkedUser.ParkedDurations.TryGetValue(subchannelId, out durationFound);
            if (hasDuration && durationFound.Equals(duration))
            {
              ISubChannel parkedSubchannel;
              if (parkedUser.SubChannels.TryGetValue(subchannelId, out parkedSubchannel))
              {
                if (parkedSubchannel.TvUsage == TvUsage.Parked)
                {
                  hasParkedUserWithDuration = true;
                  break;
                }
              }
            }
          }
        }
      }
      
      return hasParkedUserWithDuration;
    }

    private void AddParkedUser(ParkedUser parkedUser)
    {
      lock (_parkedUsersLock)
      {
        Context.ParkedUsers[parkedUser.Name] = parkedUser;
      }
    }

    private void RemoveChannelOrParkedUser(ParkedUser parkedUser, int idChannel)
    {
      lock (_parkedUsersLock)
      {
        if (Context.ParkedUsers.ContainsKey(parkedUser.Name))
        {
          int removeSubChId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(parkedUser.Name, idChannel);          
          if (removeSubChId > -1)
          {
            Log.Debug("RemoveChannelOrParkedUser: removing event from list.");
            //parkedUser.SubChannels.Remove(removeSubChId);
            parkedUser.Events.Remove(removeSubChId);
            parkedUser.ParkedAtList.Remove(removeSubChId);
            parkedUser.ParkedDurations.Remove(removeSubChId);
          }
          else
          {
            Log.Error("RemoveChannelOrParkedUser - could not find subchannel id for user: {0} - channel: {1}", parkedUser.Name, idChannel);
          }
          if (parkedUser.Events.Count == 0)
          {
            Context.ParkedUsers.Remove(parkedUser.Name);
          }
        }
      }
    }

  }
}