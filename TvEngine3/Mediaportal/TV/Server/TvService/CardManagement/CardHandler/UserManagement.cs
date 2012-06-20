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
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.TVService.Services;

namespace Mediaportal.TV.Server.TVService.CardManagement.CardHandler
{
  public class UserManagement : IUserManagement
  {
    private readonly ITvCardHandler _cardHandler;
        
    private ITvCardContext Context
    {
      get { return _cardHandler.Card.Context as ITvCardContext; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserManagement"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public UserManagement(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;      
    }
   

    public IUser GetUser(string name)
    {
      IUser existingUser;
      Context.Users.TryGetValue(name, out existingUser);
      return existingUser;
    }

    /// <summary>
    /// Removes the user from this card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="idChannel"> </param>
    public void RemoveUser(IUser user, int idChannel)
    {      
      if (Context == null)
      {
        return;
      }
      if (!DoesUserExist(user.Name))
      {
        return;
      }
      RefreshUser(ref user);

      int subChannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel);

      RemoveChannelFromUser(user, subChannelId);
      foreach (ISubChannel subchannel in user.SubChannels.Values)
      {
        if (subchannel.IdChannel == idChannel)
        {
          Log.Debug("usermanagement.RemoveUser: {0}, subch: {1} of {2}, card: {3}", user.Name, subchannel.Id, _cardHandler.Card.SubChannels.Length, _cardHandler.DataBaseCard.idCard);
          if (!ContainsUsersForSubchannel(subchannel.Id))
          {
            //only remove subchannel if it exists.
            if (_cardHandler.Card.GetSubChannel(subchannel.Id) != null)
            {
              //_cardHandler.ParkedUserManagement.CancelAllParkedChannelsForUser(user.Name);//once
              int usedSubChannel = subchannel.Id;
              // Before we remove the subchannel we have to stop it
              ITvSubChannel subChannel = _cardHandler.Card.GetSubChannel(subchannel.Id);
              if (subChannel.IsTimeShifting)
              {
                subChannel.StopTimeShifting();
              }
              else if (subChannel.IsRecording)
              {
                subChannel.StopRecording();
              }
              _cardHandler.Card.FreeSubChannel(subchannel.Id);
              var cleanTimeshiftFilesThread =
                new CleanTimeshiftFilesThread(_cardHandler.DataBaseCard.timeshiftingFolder,
                                              String.Format("live{0}-{1}.ts", _cardHandler.DataBaseCard.idCard,
                                                            usedSubChannel));
              var cleanupThread = new Thread(cleanTimeshiftFilesThread.CleanTimeshiftFiles) { IsBackground = true, Name = "TS_File_Cleanup", Priority = ThreadPriority.Lowest };
              cleanupThread.Start();
            }
          }
          break;
        }
      }

      if (_cardHandler.IsIdle)
      {
        if (_cardHandler.Card.SupportsPauseGraph)
        {
          _cardHandler.Card.PauseGraph();
        }
        else
        {
          _cardHandler.Card.StopGraph();
        }
      }
    }


    /// <summary>
    /// Gets the users for this card.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string,IUser> Users
    {
      get
      {        
        if (Context == null)
        {
          return new Dictionary<string, IUser>();
        }
        return Context.Users; 
      }      
    }

    public bool HasEqualOrHigherPriority(IUser user)
    {
      bool hasEqualOrHigherPriority = false;      
      if (Context != null)
      {
        hasEqualOrHigherPriority = HasUserEqualOrHigherPriority(user);
      }
      return hasEqualOrHigherPriority;
    }

    public bool HasHighestPriority(IUser user)
    {
      bool hasHighestPriority = false;      
      if (Context != null)
      {
        hasHighestPriority = HasUserHighestPriority(user);
      }
      return hasHighestPriority;         
    }

    public int GetTimeshiftingSubChannel(string userName)
    {
      int subchannelId = -1;
      IUser user = GetUser(userName);
      if (user != null)
      {
        foreach (ISubChannel subch in user.SubChannels.Values)
        {
          if (subch.TvUsage == TvUsage.Timeshifting)
          {
            subchannelId = subch.Id;
            break;
          }
        }
      }
      return subchannelId;
    }

    public int GetTimeshiftingChannelId(string userName)
    {
      int channelId = -1;
      IUser user = GetUser(userName);
      if (user != null)
      {
        foreach (ISubChannel subch in user.SubChannels.Values)
        {
          if (subch.TvUsage == TvUsage.Timeshifting)
          {
            channelId = subch.IdChannel;
            break;
          }
        }
      }
      return channelId;
    }


    public int GetRecentChannelId(string userName)
    {
      IUser user = GetUser(userName);
      if (user != null)
      {
        if (user.SubChannels.Count > 0)
        {
          KeyValuePair<int, ISubChannel> subChannel = user.SubChannels.LastOrDefault();
          return subChannel.Value.IdChannel;
        }
      }
      return -1;
    }

    public int GetRecentSubChannelId(string userName)
    {
      IUser user = GetUser(userName);
      if (user != null)
      {
        if (user.SubChannels.Count > 0)
        {
          KeyValuePair<int, ISubChannel> subChannel = user.SubChannels.LastOrDefault();
          return subChannel.Key;
        }
      }
      return -1;
    }

    public void AddSubChannel(IUser user, int id, int idChannel, TvUsage tvUsage)
    {
      if (user.SubChannels.ContainsKey(id))
      {
        throw new Exception("subchannel '" + id + "' already exists for user '" + user.Name + "'");
      }
      user.SubChannels.Add(id, new SubChannel(id, idChannel, tvUsage));
    }


    public ISubChannel GetSubChannelByChannelId(string name, int idChannel)
    {
      ISubChannel subChannel = null;
      IUser user = GetUser(name);
      if (user != null)
      {
        ICollection<ISubChannel> subchannels = user.SubChannels.Values;

        foreach (ISubChannel subch in subchannels)
        {
          if (subch.IdChannel == idChannel)
          {
            subChannel = subch;
            break;
          }
        }
      }
      return subChannel;
    }

    public void SetOwnerSubChannel(int subChannelId, string userName)
    {
      Context.OwnerSubChannel = new OwnerSubChannel(subChannelId, userName);
    }

    public int GetSubChannelIdByChannelId(string name, int idChannel)
    {
      int subChannelId = -1;
      ISubChannel subchannel = GetSubChannelByChannelId(name, idChannel);
      if (subchannel != null)
      {
        subChannelId = subchannel.Id;
      }
     
      return subChannelId;
    }

    public int GetSubChannelIdByChannelId(string name, int idChannel, TvUsage tvUsage)
    {
      int subChannelId = -1;
      IUser user = GetUser(name);
      if (user != null)
      {
        ICollection<ISubChannel> subchannels = user.SubChannels.Values;

        foreach (ISubChannel subchannel in subchannels)
        {
          if (subchannel.IdChannel == idChannel && subchannel.TvUsage == tvUsage)
          {
            subChannelId = subchannel.Id;
            break;
          }
        }  
      }
      
      return subChannelId;
    }

    #region moved from context

    /// <summary>
    ///   Determines whether the the card is locked and ifso returns by which used.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns>
    ///   <c>true</c> if the specified user is locked; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLocked(out IUser user)
    {
      user = null;
      bool isLocked = false;

      if (Context.OwnerSubChannel != null)
      {
        IUser existingUser = GetUser(Context.OwnerSubChannel.OwnerName);
        if (existingUser != null)
        {
          if (existingUser.SubChannels.ContainsKey(Context.OwnerSubChannel.OwnerSubChannelId))
          {
            isLocked = true;
            user = existingUser;            
          }
        }       
      }
      return isLocked;
    }

    /// <summary>
    ///   Determines whether the specified user is owner.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns>
    ///   <c>true</c> if the specified user is owner; otherwise, <c>false</c>.
    /// </returns>
    public bool IsOwner(string name)
    {
      if (Context.OwnerSubChannel == null)
      {
        return true;
      }

      IUser owner;
      bool isLocked = IsLocked(out owner);

      if (isLocked)
      {
        if (owner.Name == name)
        {
          return true;
        }
        //exception, always allow everyone to stop the epg grabber
        if (owner.Name == "epg")
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    ///   Adds the specified user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name="idChannel"> </param>
    /// <param name="subChannelId"> </param>
    public void AddSubChannelOrUser(IUser user, int idChannel, int subChannelId)
    {
      if (user == null)
      {
        throw new InvalidOperationException("user is null");
      }

      if (idChannel < 1)
      {
        throw new InvalidOperationException("idChannel is invalid");
      }

      if (subChannelId < 0)
      {
        throw new InvalidOperationException("subchannelid is invalid");
      }
      user.CardId = _cardHandler.DataBaseCard.idCard;
      Log.Info("user:{0} AddSubChannelOrUser", user.Name);
      IUser existingUser = GetUser(user.Name);      
      var subChannel = new SubChannel(subChannelId, idChannel, TvUsage.Timeshifting);
      if (existingUser != null)
      {
        bool hasSubchannel = existingUser.SubChannels.ContainsKey(subChannelId);
        if (hasSubchannel)
        {
          throw new Exception("tried to add already existning subchannel to user");
        }
        existingUser.SubChannels.Add(subChannelId, subChannel);
      }
      else
      {
        user.SubChannels.Clear();
        user.SubChannels.Add(subChannelId, subChannel);
        Context.Users.Add(user.Name, user);
      }

      if (Context.OwnerSubChannel == null)
      {
        Context.OwnerSubChannel = new OwnerSubChannel(subChannelId, user.Name);
      }
    }

    public void RemoveChannelFromUser(IUser user, int subChannelId)
    {
      string username = user.Name;
      Log.Info("user:{0} RemoveChannelFromUser", username);
      IUser existingUser = GetUser(username);

      if (existingUser != null)
      {
        existingUser.SubChannels.Remove(subChannelId);

        if (existingUser.SubChannels.Count == 0)
        {
          Context.Users.Remove(username);
        }

        if (Context.Users.Count > 0)
        {
          bool wasCurrentUserOwner = (Context.OwnerSubChannel.OwnerSubChannelId == subChannelId && Context.OwnerSubChannel.OwnerName == user.Name);
          if (wasCurrentUserOwner)
          {
            Context.OwnerSubChannel = GetNextAvailableSubchannelOwner();
          }
        }
        else
        {
          Context.OwnerSubChannel = null;
        }

        OnStopUser(existingUser);
      }
      else
      {
        throw new Exception("RemoveChannelFromUser existingUser not found =" + username);
      }
    }

    /// <summary>
    ///   Removes the specified user.
    /// </summary>
    /// <param name = "user">The user.</param>
    public void RemoveUser(IUser user)
    {
      string username = user.Name;
      Log.Info("user:{0} remove", username);
      IUser existingUser = GetUser(username);

      if (existingUser != null)
      {
        bool wasCurrentUserOwner = false;
        if (Context.OwnerSubChannel.OwnerName == existingUser.Name)
        {
          IList<int> removeSubChIds = (from subch in existingUser.SubChannels.Values where subch.TvUsage == TvUsage.Timeshifting select subch.Id).ToList();

          foreach (int removeSubChId in removeSubChIds)
          {
            if (!wasCurrentUserOwner)
            {
              wasCurrentUserOwner = (Context.OwnerSubChannel.OwnerSubChannelId == removeSubChId);
            }
            existingUser.SubChannels.Remove(removeSubChId);
          }
        }

        if (wasCurrentUserOwner)
        {
          Context.OwnerSubChannel = GetNextAvailableSubchannelOwner();
        }

        if (existingUser.SubChannels.Count == 0)
        {
          Context.Users.Remove(username);
        }

        OnStopUser(existingUser);
      }
    }

    private OwnerSubChannel GetNextAvailableSubchannelOwner()
    {
      //find new owner id 
      OwnerSubChannel nextAvailableSubchannelOwner = null;

      IUser existingScheduler = Context.Users.Values.FirstOrDefault(t => t.UserType == UserType.Scheduler);
      if (existingScheduler != null)
      {
        if (existingScheduler.SubChannels.Count > 0)
        {
          int subChannelId = existingScheduler.SubChannels.FirstOrDefault().Value.Id;
          string name = existingScheduler.Name;
          nextAvailableSubchannelOwner = new OwnerSubChannel(subChannelId, name);
        }
      }
      else
      {
        ICollection<IUser> users = Users.Values;
        int nextSubchannel = -1;
        IUser nextUser = null;
        foreach (IUser u in users)
        {
          if (u.SubChannels.Count > 0)
          {
            int maxSubchannelForUser = u.SubChannels.Values.Max(s => s.Id);
            if (maxSubchannelForUser > nextSubchannel)
            {
              nextSubchannel = maxSubchannelForUser;
              nextUser = u;
            }
            else if (maxSubchannelForUser == nextSubchannel) //a match
            {
              if (nextUser != null)
              {
                if (u.Priority > nextUser.Priority)
                {
                  nextSubchannel = maxSubchannelForUser;
                  nextUser = u;
                }
              }
            }
          }
        }
        if (nextUser != null)
        {
          nextAvailableSubchannelOwner = new OwnerSubChannel(nextSubchannel, nextUser.Name);
        }
      }
      return nextAvailableSubchannelOwner;
    }
    

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name = "exists">IUser exists</param>
    public void RefreshUser(ref IUser user)
    {
      bool userExists;
      RefreshUser(ref user, out userExists);
    }

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name = "userExists">IUser exists</param>
    public void RefreshUser(ref IUser user, out bool userExists)
    {
      User userCopy = (User)user.Clone();
      IUser existingUser = GetUser(userCopy.Name);

      if (existingUser != null)
      {
        TvStoppedReason reason = user.TvStoppedReason;
        user = (User)existingUser.Clone();
        user.TvStoppedReason = reason;
        userExists = true;
      }
      else
      {
        userExists = false;
      }
    }



    /// <summary>
    ///   Returns if the user exists or not
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns></returns>
    public bool DoesUserExist(string userName)
    {
      return (Context.Users.ContainsKey(userName));
    }

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "subChannelId">The sub channel idChannel.</param>
    /// <param name = "user">The user.</param>
    public IUser GetUser(int subChannelId)
    {
      return Context.Users.Values.FirstOrDefault(t => t.SubChannels.ContainsKey(subChannelId));
    }

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "subChannelId">The sub channel idChannel.</param>
    /// <param name = "user">The user.</param>
    public ISubChannel GetSubChannel(string name, int subChannelId)
    {
      ISubChannel subchannel = null;
      IUser user = GetUser(name);
      if (user != null)
      {        
        user.SubChannels.TryGetValue(subChannelId, out subchannel);
      }
      return subchannel;
    }

    /// <summary>
    ///   Sets the timeshifting stopped reason.
    /// </summary>
    /// <param name = "user">user.</param>
    /// <param name = "reason">TvStoppedReason.</param>
    public void SetTimeshiftStoppedReason(IUser user, TvStoppedReason reason)
    {
      IUser existingUser = GetUser(user.Name);
      if (existingUser != null)
      {
        existingUser.TvStoppedReason = reason;
      }
    }

    /// <summary>
    ///   Gets the timeshifting stopped reason.
    /// </summary>
    /// <param name = "user">user.</param>
    public TvStoppedReason GetTimeshiftStoppedReason(IUser user)
    {
      IUser existingUser = GetUser(user.Name);
      if (existingUser != null)
      {
        User userFound = (User)existingUser.Clone();
        return userFound.TvStoppedReason;
      }
      return TvStoppedReason.UnknownReason;
    }

    public int GetNextAvailableSubchannel(string userName)
    {
      int nextSubchannel = 0;
      IUser existingUser = GetUser(userName);
      if (existingUser != null)
      {
        ICollection<IUser> users = Users.Values;
        nextSubchannel = users.Select(u => u.SubChannels.Values.Max(s => s.Id)).Concat(new[] { nextSubchannel }).Max();
        if (nextSubchannel >= 0)
        {
          nextSubchannel++;
        }
      }
      return nextSubchannel;
    }

    public bool HasUserEqualOrHigherPriority(IUser user)
    {
      bool hasEqualOrHigherPriority;
      ICollection<KeyValuePair<string, IUser>> otherUsers = Context.Users.Where(t => !t.Value.Name.Equals(user.Name)).ToList();
      if (otherUsers.Count == 0)
      {
        hasEqualOrHigherPriority = true;
      }
      else
      {
        int? maxPriority = otherUsers.Max(t => t.Value.Priority);
        hasEqualOrHigherPriority = (user.Priority >= maxPriority);
      }

      return hasEqualOrHigherPriority;
    }

    public bool HasUserHighestPriority(IUser user)
    {
      bool hasHighestPriority;
      ICollection<KeyValuePair<string, IUser>> otherUsers = Context.Users.Where(t => !t.Value.Name.Equals(user.Name)).ToList();
      if (otherUsers.Count == 0)
      {
        hasHighestPriority = true;
      }
      else
      {
        int? maxPriority = otherUsers.Max(t => t.Value.Priority);
        hasHighestPriority = (user.Priority > maxPriority);
      }

      return hasHighestPriority;
    }

    

    /// <summary>
    ///   Determines whether one or more users exist for the given subchannel
    /// </summary>
    /// <param name = "subchannelId">The subchannel idChannel.</param>
    /// <returns>
    ///   <c>true</c> if users exists; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsUsersForSubchannel(int subchannelId)
    {
      IUser user = GetUser(subchannelId);
      return (user != null);
    }

    /// <summary>
    ///   Removes all users
    /// </summary>
    public void Clear()
    {
      foreach (IUser user in Context.Users.Values)
      {
        OnStopUser(user);
      }

      Context.Users.Clear();
      Context.OwnerSubChannel = null;
    }


    public void OnStopUser(IUser user)
    {
      if (user.UserType != UserType.Scheduler)
      {
        Context.UsersHistory[user.Name] = user;
      }

      var history = user.History as History;
      if (history != null)
      {
        ChannelManagement.SaveChannelHistory(history);
      }
      user.History = null;
    }

    public void OnZap(IUser user, int idChannel)
    {
      IUser existingUser = GetUser(user.Name);
      if (existingUser != null)
      {
        Channel channel = ChannelManagement.GetChannel(idChannel);
        if (channel != null)
        {
          History history = existingUser.History as History;
          if (history != null)
          {
            ChannelManagement.SaveChannelHistory(history);
          }
          existingUser.History = null;
          var channelBll = new ChannelBLL(channel);
          Program p = channelBll.CurrentProgram;
          if (p != null)
          {
            var history1 = new History
            {
              idChannel = channel.idChannel,
              startTime = p.startTime,
              endTime = p.endTime,
              title = p.title,
              description = p.description,
              ProgramCategory = p.ProgramCategory,
              recorded = false,
              watched = 0
            };
            existingUser.History = history1;
          }
        }
      }
    }

    #endregion


  }
}