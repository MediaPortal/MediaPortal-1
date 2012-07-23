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

using System.Collections.Generic;
using TvControl;

namespace TvService
{
  public interface ITvCardContext
  {
    /// <summary>
    ///   Sets the owner.
    /// </summary>
    /// <value>The owner.</value>
    IUser Owner { get; set; }

    /// <summary>
    ///   Gets the users.
    /// </summary>
    /// <value>The users.</value>
    IUser[] Users { get; }

    /// <summary>
    ///   Locks the card for the user specifies
    /// </summary>
    /// <param name = "newUser">The user.</param>
    void Lock(IUser newUser);

    /// <summary>
    ///   Unlocks this card.
    /// </summary>
    void Unlock();

    /// <summary>
    ///   Determines whether the the card is locked and ifso returns by which used.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns>
    ///   <c>true</c> if the specified user is locked; otherwise, <c>false</c>.
    /// </returns>
    bool IsLocked(out IUser user);

    /// <summary>
    ///   Determines whether the specified user is owner.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns>
    ///   <c>true</c> if the specified user is owner; otherwise, <c>false</c>.
    /// </returns>
    bool IsOwner(IUser user);

    /// <summary>
    ///   Adds the specified user.
    /// </summary>
    /// <param name = "user">The user.</param>
    void Add(IUser user);

    /// <summary>
    ///   Removes the specified user.
    /// </summary>
    /// <param name = "user">The user.</param>
    void Remove(IUser user);

    void HeartBeatUser(IUser user);

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    void GetUser(ref IUser user);

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name = "cardId">The card id of the user to be found</param>
    void GetUser(ref IUser user, int cardId);

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <param name = "exists">User exists</param>
    void GetUser(ref IUser user, out bool exists);

    /// <summary>
    ///   Returns if the user exists or not
    /// </summary>
    /// <param name = "user">The user.</param>
    /// <returns></returns>
    bool DoesExists(IUser user);

    /// <summary>
    ///   Gets the user.
    /// </summary>
    /// <param name = "subChannelId">The sub channel id.</param>
    /// <param name = "user">The user.</param>
    void GetUser(int subChannelId, out IUser user);

    /// <summary>
    ///   Sets the timeshifting stopped reason.
    /// </summary>
    /// <param name = "user">user.</param>
    /// <param name = "reason">TvStoppedReason.</param>
    void SetTimeshiftStoppedReason(IUser user, TvStoppedReason reason);

    /// <summary>
    ///   Gets the timeshifting stopped reason.
    /// </summary>
    /// <param name = "user">user.</param>
    TvStoppedReason GetTimeshiftStoppedReason(IUser user);

    /// <summary>
    ///   Determines whether one or more users exist for the given subchannel
    /// </summary>
    /// <param name = "subchannelId">The subchannel id.</param>
    /// <returns>
    ///   <c>true</c> if users exists; otherwise, <c>false</c>.
    /// </returns>
    bool ContainsUsersForSubchannel(int subchannelId);

    /// <summary>
    ///   Removes all users
    /// </summary>
    void Clear();

    void OnStopUser(IUser user);
    void OnZap(IUser user);
    void UserNextAvailableSubchannel(IUser user);
    bool HasUserEqualOrHigherPriority(IUser user);
    bool HasUserHighestPriority(IUser user);
  }
}