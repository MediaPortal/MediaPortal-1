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

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.TVService.Services;

#endregion

[assembly: InternalsVisibleTo("TVServiceTests")]

namespace Mediaportal.TV.Server.TVService
{
  /// <summary>
  ///   Class which holds the context for a specific card
  /// </summary>
  public class TvCardContext : ITvCardContext
  {
    #region variables
    private readonly Timer _timer = new Timer();
    private readonly IDictionary<string,IUser> _users;

    private readonly IDictionary<string, IUser> _usersOld;
    //holding a list of all the timeshifting users that have been stopped - mkaing it possible for the client to query the possible stop reason.

    private IUser _owner;

    #endregion

    #region ctor

    /// <summary>
    ///   Initializes a new instance of the <see cref = "TvCardContext" /> class.
    /// </summary>
    public TvCardContext()
    {
      _users = new Dictionary<string, IUser>();
      _usersOld = new Dictionary<string, IUser>();
      _owner = null;
      _timer.Interval = 60000;
      _timer.Enabled = true;
      _timer.Elapsed += _timer_Elapsed;
    }

    #endregion

    #region public methods   

    /// <summary>
    ///   Locks the card for the user specifies
    /// </summary>
    /// <param name = "newUser">The user.</param>
    public void Lock(IUser newUser)
    {
      _owner = newUser;
    }

    /// <summary>
    ///   Unlocks this card.
    /// </summary>
    public void Unlock()
    {
      _owner = null;
    }

    /// <summary>
    ///   Determines whether the the card is locked and ifso returns by which used.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns>
    ///   <c>true</c> if the specified user is locked; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLocked(out IUser user)
    {
      user = _owner;
      return (user != null);
    }

    /// <summary>
    ///   Determines whether the specified user is owner.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns>
    ///   <c>true</c> if the specified user is owner; otherwise, <c>false</c>.
    /// </returns>
    public bool IsOwner(IUser user)
    {
      if (_owner == null)
        return true;
      if (_owner.Name == user.Name)
        return true;

      //exception, always allow everyone to stop the epg grabber
      if (_owner.Name == "epg")
        return true;
      return false;
    }

    /// <summary>
    ///   Sets the owner.
    /// </summary>
    /// <value>The owner.</value>
    public IUser Owner
    {
      get { return _owner; }
      set { _owner = value; }
    }


    /// <summary>
    ///   Adds the specified user.
    /// </summary>
    /// <param name = "user">The user.</param>
    public void Add(IUser user)
    {
      Log.Info("user:{0} add", user.Name);
      if (_owner == null)
      {
        _owner = user;
      }
      _users[user.Name] = user; //add or replace
    }

    /// <summary>
    ///   Removes the specified user.
    /// </summary>
    /// <param name = "user">The user.</param>
    public void Remove(IUser user)
    {
      string username = user.Name;
      Log.Info("user:{0} remove", username);
      IUser existingUser = GetUserByName(_users, username);
      if (existingUser != null)
      {
        OnStopUser(existingUser);
        _users.Remove(username);
      }

      if (_owner != null && _owner.Name.Equals(username))
      {
        if (_users.Count > 0)
        {
          IUser existingScheduler = _users.Values.FirstOrDefault(t => t.IsAdmin);
          if (existingScheduler != null)
          {
            _owner = existingScheduler;
          }
          else
          {
            _owner = _users.Values.FirstOrDefault();
          }
        }
        else
        {
          _owner = null;
        }
      }
    }   

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    public void GetUser(ref IUser user)
    {
      User userCopy = (User)user.Clone();
      IUser existingUser = GetUserByNameAndCardId(userCopy.Name, userCopy.CardId);
      if (existingUser != null)
      {
        TvStoppedReason reason = user.TvStoppedReason;
        user = (User) existingUser.Clone();
        user.TvStoppedReason = reason;
      }
    }

    private IUser GetUserByNameAndCardId (string name, int cardId)
    {
      IUser existingUser;
      bool userFound = _users.TryGetValue(name, out existingUser);
      if (userFound && existingUser != null && existingUser.CardId == cardId)
      {
        return existingUser;
      }
      return null;
    }

    private IUser GetUserByName(IDictionary<string, IUser> users, string name)
    {
      IUser existingUser;
      users.TryGetValue(name, out existingUser);      
      return existingUser;      
    }

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name = "cardId">The card id of the user to be found</param>
    public void GetUser(ref IUser user, int cardId)
    {
      User userCopy = (User)user.Clone();
      IUser existingUser = GetUserByNameAndCardId(userCopy.Name, userCopy.CardId);
      if (existingUser != null)
      {
        TvStoppedReason reason = user.TvStoppedReason;
        user = (User)existingUser.Clone();
        user.TvStoppedReason = reason;
      }
    }

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name = "exists">IUser exists</param>
    public void GetUser(ref IUser user, out bool exists)
    {
      User userCopy = (User)user.Clone();
      IUser existingUser = GetUserByNameAndCardId(userCopy.Name, userCopy.CardId);
      if (existingUser != null)
      {
        TvStoppedReason reason = user.TvStoppedReason;
        user = (User)existingUser.Clone();
        user.TvStoppedReason = reason;
        exists = true;
        return;
      }
      exists = false;
    }

    /// <summary>
    ///   Returns if the user exists or not
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns></returns>
    public bool DoesExists(IUser user)
    {
      return (_users.ContainsKey(user.Name));
    }

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "subChannelId">The sub channel id.</param>
    /// <param name = "user">The user.</param>
    public void GetUser(int subChannelId, out IUser user)
    {
      user = null;
      IUser existingUser = _users.Values.FirstOrDefault(t => t.SubChannel == subChannelId);
      if (existingUser != null)
      {
        user = (User)existingUser.Clone();
      }
    }

    /// <summary>
    ///   Sets the timeshifting stopped reason.
    /// </summary>
    /// <param name = "user">user.</param>
    /// <param name = "reason">TvStoppedReason.</param>
    public void SetTimeshiftStoppedReason(IUser user, TvStoppedReason reason)
    {
      IUser existingUser = GetUserByName(_users, user.Name);      
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
      IUser existingUser = GetUserByName(_usersOld, user.Name);      
      if (existingUser != null)
      {
        User userFound = (User)existingUser.Clone();
        return userFound.TvStoppedReason;
      }
      return TvStoppedReason.UnknownReason;
    }

    public void UserNextAvailableSubchannel(IUser user)
    {
      IUser existingUser = GetUserByName(_users, user.Name);            
      if (existingUser != null)
      {
        int nextSubchannel = (from IUser u in _users select u.SubChannel).Concat(new[] {-1}).Max();

        if (nextSubchannel >= 0)
        {
          existingUser.SubChannel = nextSubchannel + 1;
        }
      }
    }

    public bool HasUserEqualOrHigherPriority(IUser user)
    {      
      bool hasEqualOrHigherPriority;
      ICollection<KeyValuePair<string, IUser>> otherUsers = _users.Where(t => !t.Value.Name.Equals(user.Name)).ToList();
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
      ICollection<KeyValuePair<string, IUser>> otherUsers = _users.Where(t => !t.Value.Name.Equals(user.Name)).ToList();
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
    ///   Gets the users.
    /// </summary>
    /// <value>The users.</value>
    public IDictionary<string, IUser> Users
    {
      get { return _users; }
    }

    /// <summary>
    ///   Determines whether one or more users exist for the given subchannel
    /// </summary>
    /// <param name = "subchannelId">The subchannel id.</param>
    /// <returns>
    ///   <c>true</c> if users exists; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsUsersForSubchannel(int subchannelId)
    {
      return _users.Values.Any(t => t.SubChannel == subchannelId);
    }

    /// <summary>
    ///   Removes all users
    /// </summary>
    public void Clear()
    {
      foreach (KeyValuePair<string, IUser> user in _users)
      {
        OnStopUser(user.Value);
      }
      _users.Clear();
      _owner = null;
    }


    public void OnStopUser(IUser user)
    {
      if (!user.IsAdmin)
      {        
        _usersOld[user.Name] = user;        
      }

      var history = user.History as History;
      if (history != null)
      {
         ChannelManagement.SaveChannelHistory(history);
      }
      user.History = null;
    }

    public void OnZap(IUser user)
    {
      IUser existingUser = GetUserByName(_users, user.Name);
      if (existingUser != null)
      {                
        Channel channel = ChannelManagement.GetChannel(user.IdChannel);
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

    private void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      try
      {
        foreach (KeyValuePair<string, IUser> existingUser in _users)
        {
          History history = existingUser.Value.History as History;
          if (history != null)
          {
            Channel channel = ChannelManagement.GetChannel(existingUser.Value.IdChannel);
            if (channel != null)
            {
              Program p = new ChannelBLL(channel).CurrentProgram;
              if (p != null && p.startTime != history.startTime)
              {                
                ChannelManagement.SaveChannelHistory(history);
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

                existingUser.Value.History = history1;                
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
  }
}