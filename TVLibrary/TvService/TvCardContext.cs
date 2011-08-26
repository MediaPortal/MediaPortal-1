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
using System.Runtime.CompilerServices;
using System.Timers;
using TvControl;
using TvDatabase;
using TvLibrary.Log;

#endregion

[assembly: InternalsVisibleTo("TVServiceTests")]

namespace TvService
{
  /// <summary>
  ///   Class which holds the context for a specific card
  /// </summary>
  public class TvCardContext : ITvCardContext
  {
    #region variables

    private readonly Timer _timer = new Timer();
    private readonly List<IUser> _users;

    private readonly List<IUser> _usersOld;
    //holding a list of all the timeshifting users that have been stopped - mkaing it possible for the client to query the possible stop reason.

    private IUser _owner;

    #endregion

    #region ctor

    /// <summary>
    ///   Initializes a new instance of the <see cref = "TvCardContext" /> class.
    /// </summary>
    public TvCardContext()
    {
      _users = new List<IUser>();
      _usersOld = new List<IUser>();
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
      int i = _users.FindIndex(t => t.Name == user.Name);
      if (i > -1)
      {
        _users[i] = (User)user.Clone();
      }
      else
      {
        _users.Add(user);
      }
    }

    /// <summary>
    ///   Removes the specified user.
    /// </summary>
    /// <param name = "user">The user.</param>
    public void Remove(IUser user)
    {
      string username = user.Name;
      Log.Info("user:{0} remove", username);
      IUser existingUser = _users.Find(t => t.Name.Equals(username));
      if (existingUser != null)
      {
        OnStopUser(existingUser);
        _users.Remove(existingUser);
      }

      if (_owner != null && _owner.Name.Equals(username))
      {
        if (_users.Count > 0)
        {
          IUser existingScheduler = _users.Find(t => t.IsAdmin);

          if (existingScheduler != null)
          {
            _owner = existingScheduler;
          }
          else
          {
            _owner = _users[0];
          }
        }
        else
        {
          _owner = null;
        }
      }
    }

    public void HeartBeatUser(IUser user)
    {
      //Log.Debug("user:{0} heartbeat received", user.Name);

      IUser existingUser = _users.Find(t => t.Name == user.Name);
      if (existingUser != null)
      {
        existingUser.HeartBeat = DateTime.Now;
      }
    }

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    public void GetUser(ref IUser user)
    {
      User userCopy = (User)user.Clone();
      IUser existingUser = _users.Find(t => t.Name == userCopy.Name && t.CardId == userCopy.CardId);
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
    /// <param name = "cardId">The card id of the user to be found</param>
    public void GetUser(ref IUser user, int cardId)
    {
      User userCopy = (User)user.Clone();
      IUser existingUser = _users.Find(t => t.Name == userCopy.Name && t.CardId == cardId);
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
      IUser existingUser = _users.Find(t => t.Name == userCopy.Name && t.CardId == userCopy.CardId);
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
      return (_users.Exists(t => t.Name == user.Name));
    }

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "subChannelId">The sub channel id.</param>
    /// <param name = "user">The user.</param>
    public void GetUser(int subChannelId, out IUser user)
    {
      user = null;
      IUser existingUser = _users.Find(t => t.SubChannel == subChannelId);
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
      IUser existingUser = _users.Find(t => t.Name == user.Name);
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
      IUser existingUser = _usersOld.Find(t => t.Name == user.Name);
      if (existingUser != null)
      {
        User userFound = (User)existingUser.Clone();
        return userFound.TvStoppedReason;
      }
      return TvStoppedReason.UnknownReason;
    }

    public void UserNextAvailableSubchannel(IUser user)
    {
      IUser existingUser = _users.Find(t => t.Name == user.Name);
      if (existingUser != null)
      {
        int nextSubchannel = -1;
        foreach (IUser u in _users)
        {
          if (u.SubChannel > nextSubchannel)
          {
            nextSubchannel = u.SubChannel;
          }
        }

        if (nextSubchannel >= 0)
        {
          existingUser.SubChannel = nextSubchannel + 1;
        }
      }
    }

    /// <summary>
    ///   Gets the users.
    /// </summary>
    /// <value>The users.</value>
    public IUser[] Users
    {
      get { return _users.ToArray(); }
    }

    protected internal IList<IUser> UsersOld
    {
      get { return _usersOld; }
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
      return _users.Exists(t => t.SubChannel == subchannelId);
    }

    /// <summary>
    ///   Removes all users
    /// </summary>
    public void Clear()
    {
      foreach (IUser user in _users)
      {
        OnStopUser(user);
      }
      _users.Clear();
      _owner = null;
    }


    public void OnStopUser(IUser user)
    {
      if (!user.IsAdmin)
      {
        _usersOld.RemoveAll(t => t.Name == user.Name);
        _usersOld.Add(user);
      }

      History history = user.History as History;
      if (history != null)
      {
        history.Save();
      }
      user.History = null;
    }

    public void OnZap(IUser user)
    {
      IUser existingUser = _users.Find(t => t.Name == user.Name);
      if (existingUser != null)
      {
        Channel channel = Channel.Retrieve(user.IdChannel);
        if (channel != null)
        {
          History history = existingUser.History as History;
          if (history != null)
          {
            history.Save();
          }
          existingUser.History = null;
          Program p = channel.CurrentProgram;
          if (p != null)
          {
            existingUser.History = new History(channel.IdChannel, p.StartTime, p.EndTime, p.Title, p.Description,
                                               p.Genre, false, 0);
          }
        }
      }
    }

    #endregion

    private void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      try
      {
        foreach (IUser existingUser in _users)
        {
          History history = existingUser.History as History;
          if (history != null)
          {
            Channel channel = Channel.Retrieve(existingUser.IdChannel);
            if (channel != null)
            {
              Program p = channel.CurrentProgram;
              if (p != null && p.StartTime != history.StartTime)
              {
                history.Save();
                existingUser.History = new History(channel.IdChannel, p.StartTime, p.EndTime, p.Title, p.Description,
                                                   p.Genre, false, 0);
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