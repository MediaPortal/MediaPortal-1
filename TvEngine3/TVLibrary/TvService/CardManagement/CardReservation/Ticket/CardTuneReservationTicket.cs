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
using System.Linq;
using TvControl;
using TvLibrary.Interfaces;

namespace TvService
{
  public class CardTuneReservationTicket : CardReservationTicketBase, ICardTuneReservationTicket
  {
    #region vars

    private readonly IUser _user;
    private readonly IChannel _tuningDetail;
    private readonly bool _isSameTransponder;
    private readonly int _numberOfOtherUsersOnSameChannel;
    private readonly int _numberOfOtherUsersOnCurrentCard;
    private readonly int _numberOfUsersOnSameCurrentChannel;
    private readonly bool _isAnySubChannelTimeshifting;    
    private readonly List<IUser> _inactiveUsers = new List<IUser>();
    private readonly List<IUser> _activeUsers = new List<IUser>();
    private readonly List<IUser> _users = new List<IUser>();
    private readonly int _ownerSubchannel = -1;
    private readonly bool _isOwner;    
    private readonly int _idCard;
    private readonly int _numberOfChannelsDecrypting;
    private readonly bool _isFreeToAir = true;            
    private readonly bool _conflictingSubchannelFound;
    private readonly bool _isCamAlreadyDecodingChannel;

    private readonly List<IUser> _recordingUsers = new List<IUser>();
    private readonly IList<IUser> _timeshiftingUsers = new List<IUser>();
    private int _pendingSubchannel;
    private readonly bool _hasHighestPriority;
    private readonly bool _hasEqualOrHigherPriority;

    #endregion

    public CardTuneReservationTicket(
      IUser user, 
      IChannel tuningDetail, 
      bool isSameTransponder, 
      int numberOfOtherUsersOnSameChannel, 
      bool isAnySubChannelTimeshifting, 
      List<IUser> inactiveUsers, 
      List<IUser> activeUsers, 
      List<IUser> users, 
      int ownerSubchannel, 
      bool isOwner, 
      int idCard, 
      int numberOfChannelsDecrypting, 
      bool isFreeToAir, 
      int numberOfOtherUsersOnCurrentCard, 
      List<IUser> recUsers, 
      List<IUser> tsUsers, 
      bool conflictingSubchannelFound, 
      int numberOfUsersOnSameCurrentChannel, 
      bool isCamAlreadyDecodingChannel,
      bool hasHighestPriority,
      bool hasEqualOrHigherPriority)
    {
      _pendingSubchannel = -1;
      _user = user;
      _isCamAlreadyDecodingChannel = isCamAlreadyDecodingChannel;
      _numberOfUsersOnSameCurrentChannel = numberOfUsersOnSameCurrentChannel;
      _conflictingSubchannelFound = conflictingSubchannelFound;      
      _recordingUsers = recUsers;
      _timeshiftingUsers = tsUsers;
      _numberOfOtherUsersOnCurrentCard = numberOfOtherUsersOnCurrentCard;
      _isFreeToAir = isFreeToAir;
      _numberOfChannelsDecrypting = numberOfChannelsDecrypting;
      _idCard = idCard;
      _isOwner = isOwner;
      _ownerSubchannel = ownerSubchannel;
      _tuningDetail = tuningDetail;
      _isSameTransponder = isSameTransponder;
      _numberOfOtherUsersOnSameChannel = numberOfOtherUsersOnSameChannel;
      _isAnySubChannelTimeshifting = isAnySubChannelTimeshifting;
      _inactiveUsers = inactiveUsers;
      _activeUsers = activeUsers;
      _users = users;
      _hasHighestPriority = hasHighestPriority;
      _hasEqualOrHigherPriority = hasEqualOrHigherPriority;
    }    

    public IChannel TuningDetail
    {
      get { return _tuningDetail; }
    }

    public bool IsSameTransponder
    {
      get { return _isSameTransponder; }
    }

    public int NumberOfOtherUsersOnSameChannel
    {
      get { return _numberOfOtherUsersOnSameChannel; }
    }

    public bool IsAnySubChannelTimeshifting
    {
      get { return _isAnySubChannelTimeshifting; }
    }

    public IList<IUser> InactiveUsers
    {
      get { return _inactiveUsers; }
    }

    public IList<IUser> ActiveUsers
    {
      get { return _activeUsers; }
    }

    public IList<IUser> Users
    {
      get { return _users; }
    }

    public int OwnerSubchannel
    {
      get { return _ownerSubchannel; }
    }

    public bool HasHighestPriority
    {     
      get
      {
        return _hasHighestPriority;        
      }
    }

    public bool HasEqualOrHigherPriority
    {      
      get
      {
        return _hasEqualOrHigherPriority;        
      }
    }

    public bool IsOwner
    {
      get { return _isOwner; }
    }    

    public int CardId
    {
      get { return _idCard; }
    }

    public int NumberOfChannelsDecrypting
    {
      get { return _numberOfChannelsDecrypting; }
    }

    public bool IsCamAlreadyDecodingChannel
    {
      get { return _isCamAlreadyDecodingChannel; }
    }

    public IUser User
    {
      get { return _user; }
    }

    public int PendingSubchannel
    {
      get { return _pendingSubchannel; }
      set { _pendingSubchannel = value; }
    }

    public bool IsFreeToAir
    {
      get { return _isFreeToAir; }
    }

    public int NumberOfOtherUsersOnCurrentCard
    {
      get { return _numberOfOtherUsersOnCurrentCard; }
    }

    public List<IUser> RecordingUsers
    {
      get { return _recordingUsers; }
    }

    public IList<IUser> TimeshiftingUsers
    {
      get { return _timeshiftingUsers; }
    }   

    public bool ConflictingSubchannelFound
    {
      get { return _conflictingSubchannelFound; }
    }

    public int NumberOfUsersOnSameCurrentChannel
    {
      get { return _numberOfUsersOnSameCurrentChannel; }
    }
  }
}
