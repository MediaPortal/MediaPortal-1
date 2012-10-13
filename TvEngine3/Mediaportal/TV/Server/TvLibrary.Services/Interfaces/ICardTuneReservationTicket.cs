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
using TvControl;
using TvLibrary.Interfaces;

namespace TvService
{
  public interface ICardTuneReservationTicket
  {
    IChannel TuningDetail  { get; }

    IList<IUser> InactiveUsers { get; }
    IList<IUser> ActiveUsers { get; }
    IList<IUser> Users { get; }
    List<IUser> RecordingUsers { get; }
    IList<IUser> TimeshiftingUsers { get; }        

    bool IsSameTransponder  { get; }
    bool IsOwner { get; }
    bool IsAnySubChannelTimeshifting { get; }
    bool IsFreeToAir { get; }
    bool ConflictingSubchannelFound { get; }    

    int NumberOfOtherUsersOnSameChannel { get; }
    int NumberOfOtherUsersOnCurrentCard { get; }
    int NumberOfUsersOnSameCurrentChannel { get; }

    int Id { get; }
    int OwnerSubchannel { get; }    
    int CardId { get; }
    int NumberOfChannelsDecrypting { get; }
    bool IsCamAlreadyDecodingChannel { get; }
    IUser User { get; }
    int PendingSubchannel { get; set; }
    bool HasHighestPriority { get; }
    bool HasEqualOrHigherPriority { get; }
  }
}
