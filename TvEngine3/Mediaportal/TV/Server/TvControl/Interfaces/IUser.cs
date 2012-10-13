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


namespace TvControl
{
  /// <summary>
  /// User
  /// </summary>
  public interface IUser
  {
    /// <summary>
    /// Gets an integer defining the user's card lock priority (higher number=higher priority)
    /// </summary>    
    /// <returns>user priority</returns>
    int? Priority { get; set; }

    /// <summary>
    /// Gets a list of all channel states    
    /// </summary>    
    /// <returns>dictionary containing all channel states of the channels supplied</returns>
    Dictionary<int, ChannelState> ChannelStates { get; set; }

    /// <summary>
    /// Gets or sets the failed card id.
    /// </summary>
    /// <value>The card id.</value>
    int FailedCardId { get; set; }

    /// <summary>
    /// Gets or sets the card id.
    /// </summary>
    /// <value>The card id.</value>
    int CardId { get; set; }

    /// <summary>
    /// Gets or sets the database id channel.
    /// </summary>
    /// <value>The id channel.</value>
    int IdChannel { get; set; }

    /// <summary>
    /// Gets or sets the subchannel id.
    /// </summary>
    /// <value>The subchannel id.</value>
    int SubChannel { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is admin.
    /// </summary>
    /// <value><c>true</c> if this instance is admin; otherwise, <c>false</c>.</value>
    bool IsAdmin { get; set; }

    /// <summary>
    /// Gets or sets the history.
    /// </summary>
    /// <value>The history.</value>
    object History { get; set; }

    /// <summary>
    /// Gets/Sets the time of the last heartbeat
    /// </summary>
    DateTime HeartBeat { get; set; }

    /// <summary>
    /// Gets/Sets the stop reason
    /// </summary>
    TvStoppedReason TvStoppedReason { get; set; }

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    /// A new object that is a copy of this instance.
    /// </returns>
    object Clone();
  }
}