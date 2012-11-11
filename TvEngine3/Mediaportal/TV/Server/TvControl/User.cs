#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Net;
using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl
{
  /// <summary>
  /// 
  /// </summary>
  [DataContract]
  public class User : ICloneable, IUser
  {
    #region private members

    [DataMember] 
    private UserType _userType;

    [DataMember]
    private string _hostName;

    [DataMember]
    private int _cardId;

    [DataMember]
    private int _failedCardId;

    [DataMember]
    private TvStoppedReason _timeshiftStoppedReason;

    [NonSerialized]
    private object _history;

    [DataMember]
    private Dictionary<int, ChannelState> _channelStates; //used primarily for miniepg.

    [DataMember]
    private int? _priority;

    [DataMember]
    private IDictionary<int, ISubChannel> _subChannels; //key is subChannelId

    #endregion

    #region constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    public User()
    {
      _userType = UserType.Normal;
      _priority = null;
      _hostName = Dns.GetHostName();
      _cardId = -1;
      _failedCardId = -1;
      _timeshiftStoppedReason = TvStoppedReason.UnknownReason;
      _channelStates = new Dictionary<int, ChannelState>();
      _subChannels = new SortedDictionary<int, ISubChannel>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="userType"> </param>    
    public User(string name, UserType userType) : this()
    {      
      _hostName = name;
      _userType = userType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">The name.</param>    
    /// <param name="userType"> </param>
    /// <param name="cardId">The card id.</param>
    public User(string name, UserType userType, int cardId) : this(name, userType)
    {      
      _cardId = cardId;     
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="userType"> </param>
    /// <param name="cardId">The card id.</param>
    /// <param name="priority">card lock priority</param>
    public User(string name, UserType userType, int cardId, int priority)
      : this(name, userType, cardId)
    {     
      _priority = priority;           
    }

    #endregion    

    /// <summary>
    /// Gets an integer defining the user's card lock priority (higher number=higher priority)
    /// </summary>    
    /// <returns>user priority</returns>
    public int? Priority
    {
      get { return _priority; }
      set { _priority = value; }
    }

    /// <summary>
    /// Gets a list of all channel states    
    /// </summary>    
    /// <returns>dictionary containing all channel states of the channels supplied</returns>
    public Dictionary<int, ChannelState> ChannelStates
    {
      get { return _channelStates; }
      set { _channelStates = value; }
    }

    /// <summary>
    /// Gets or sets the failed card id.
    /// </summary>
    /// <value>The card id.</value>
    public int FailedCardId
    {
      get { return _failedCardId; }
      set { _failedCardId = value; }
    }

    /// <summary>
    /// Gets or sets the card id.
    /// </summary>
    /// <value>The card id.</value>
    public int CardId
    {
      get { return _cardId; }
      set { _cardId = value; }
    }
    
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
      get { return _hostName; }
      set { _hostName = value; }
    }    

    /// <summary>
    /// Gets or sets the history.
    /// </summary>
    /// <value>The history.</value>
    public object History
    {
      get { return _history; }
      set { _history = value; }
    }

    /// <summary>
    /// Gets/Sets the stop reason
    /// </summary>
    public TvStoppedReason TvStoppedReason
    {
      get { return _timeshiftStoppedReason; }
      set { _timeshiftStoppedReason = value; }
    }

    public IDictionary<int, ISubChannel> SubChannels
    {
      get { return _subChannels; }
      set { _subChannels = value; }
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
      var user = new User(_hostName, _userType, _cardId, _priority.GetValueOrDefault())
                    {                                              
                      SubChannels = new SortedDictionary<int, ISubChannel>(_subChannels),
                      History = _history,
                      ChannelStates = new Dictionary<int, ChannelState>(_channelStates),
                      FailedCardId = _failedCardId,
                      TvStoppedReason = _timeshiftStoppedReason                      
                    };

      return user;
    }

    public UserType UserType
    {
      get { return _userType; }
    }


    #endregion


   
  }
}
