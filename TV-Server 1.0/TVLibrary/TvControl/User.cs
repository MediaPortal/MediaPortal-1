/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace TvControl
{
  /// <summary>
  /// Class holding user credentials
  /// </summary>
  [Serializable]
  public class User : ICloneable
  {
    string _hostName;
    bool _isAdmin;
    int _cardId;
    int _subChannel;
    int _idChannel;
    TvStoppedReason _timeshiftStoppedReason;
    DateTime _lastHeartBeat;
    [NonSerialized]
    object _history;
    private Dictionary<int, ChannelState> _channelStates = null; //used primarily for miniepg.

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    public User()
    {
      _hostName = Dns.GetHostName();
      _isAdmin = false;
      _cardId = -1;
      _idChannel = -1;
      _subChannel = -1;
      _lastHeartBeat = DateTime.MinValue;
      _timeshiftStoppedReason = TvStoppedReason.UnknownReason;
      _channelStates = new Dictionary<int, ChannelState>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
    public User(string name, bool isAdmin)
    {
      _hostName = name;
      _isAdmin = isAdmin;
      _cardId = -1;
      _subChannel = -1;
      _timeshiftStoppedReason = TvStoppedReason.UnknownReason;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
    /// <param name="cardId">The card id.</param>
    public User(string name, bool isAdmin, int cardId)
    {
      _hostName = name;
      _isAdmin = isAdmin;
      _cardId = cardId;
      _subChannel = -1;
      _timeshiftStoppedReason = TvStoppedReason.UnknownReason;
    }

    /// <summary>
    /// Gets a list of all channel states    
    /// </summary>    
    /// <returns>dictionary containing all channel states of the channels supplied</returns>
    public Dictionary<int, ChannelState> ChannelStates
    {
      get
      {
        return _channelStates;
      }
      set
      {
        _channelStates = value;
      }
    }

    /// <summary>
    /// Gets or sets the card id.
    /// </summary>
    /// <value>The card id.</value>
    public int CardId
    {
      get
      {
        return _cardId;
      }
      set
      {
        _cardId = value;
      }
    }
    /// <summary>
    /// Gets or sets the database id channel.
    /// </summary>
    /// <value>The id channel.</value>
    public int IdChannel
    {
      get
      {
        return _idChannel;
      }
      set
      {
        _idChannel = value;
      }
    }

    /// <summary>
    /// Gets or sets the subchannel id.
    /// </summary>
    /// <value>The subchannel id.</value>
    public int SubChannel
    {
      get
      {
        return _subChannel;
      }
      set
      {
        _subChannel = value;
      }
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
      get
      {
        return _hostName;
      }
      set
      {
        _hostName = value;
      }
    }
    /// <summary>
    /// Gets or sets a value indicating whether this instance is admin.
    /// </summary>
    /// <value><c>true</c> if this instance is admin; otherwise, <c>false</c>.</value>
    public bool IsAdmin
    {
      get
      {
        return _isAdmin;
      }
      set
      {
        _isAdmin = value;
      }
    }

    /// <summary>
    /// Gets or sets the history.
    /// </summary>
    /// <value>The history.</value>
    public object History
    {
      get
      {
        return _history;
      }
      set
      {
        _history = value;
      }
    }

    public DateTime HeartBeat
    {
      get
      {
        return _lastHeartBeat;
      }
      set
      {
        _lastHeartBeat = value;
      }
    }

    public TvStoppedReason TvStoppedReason
    {
      get
      {
        return _timeshiftStoppedReason;
      }
      set
      {
        _timeshiftStoppedReason = value;
      }
    }



    #region ICloneable Members

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    /// A new object that is a copy of this instance.
    /// </returns>
    public object Clone()
    {
      User user = new User();
      user._hostName = _hostName;
      user._isAdmin = _isAdmin;
      user._cardId = _cardId;
      user._subChannel = _subChannel;
      user._idChannel = _idChannel;
      user._timeshiftStoppedReason = _timeshiftStoppedReason;
      return user;
    }

    #endregion


  }
}
