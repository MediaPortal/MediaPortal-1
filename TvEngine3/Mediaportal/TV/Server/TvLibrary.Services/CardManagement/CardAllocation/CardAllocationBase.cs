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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardAllocation
{
  public abstract class CardAllocationBase
  {
    private bool _logEnabled = true;

    protected bool LogEnabled
    {
      get { return _logEnabled; }
      set { _logEnabled = value; }
    }

    #region protected members

    private bool IsCamAbleToDecryptChannel(IUser user, ITvCardHandler tvcard, IChannel tuningDetail, int decryptLimit)
    {
      if (!tuningDetail.IsEncrypted)
      {
        return true;
      }

      bool isCamAbleToDecryptChannel = true;
      if (decryptLimit > 0)
      {
        int camDecrypting = NumberOfChannelsDecrypting(tvcard);

        //Check if the user is currently occupying a decoding slot and subtract that from the number of channels it is decoding.
        if (user.CardId == tvcard.Card.TunerId)
        {
          //todo gibman - could be buggy, needs looking at.
          //foreach(var subchannel in user.SubChannels.Values)
          {
            bool isFreeToAir = IsFreeToAir(tvcard, user.Name, tvcard.UserManagement.GetTimeshiftingChannelId(user.Name));
            if (!isFreeToAir)
            {
              int numberOfUsersOnCurrentChannel = GetNumberOfUsersOnCurrentChannel(tvcard, user.Name);

              //Only subtract the slot the user is currently occupying if he is the only user occupying the slot.
              if (numberOfUsersOnCurrentChannel == 1)
              {
                --camDecrypting;
              }
            }
          }
            
        }
        //check if cam is capable of descrambling an extra channel
        isCamAbleToDecryptChannel = (camDecrypting < decryptLimit);
      }
      return isCamAbleToDecryptChannel;
    }

    protected virtual int GetNumberOfUsersOnCurrentChannel(ITvCardHandler tvcard, string userName) 
    {
      int currentChannelId = tvcard.CurrentDbChannel(userName);
      int count = tvcard.UserManagement.GetNumberOfUsersOnChannel(currentChannelId);      
      return count;
    }

    protected virtual bool IsFreeToAir(ITvCardHandler tvcard, string userName, int idChannel)
    {      
      IChannel currentUserCh = tvcard.CurrentChannel(userName, idChannel);
      return (currentUserCh != null && !currentUserCh.IsEncrypted);
    }

    protected virtual int NumberOfChannelsDecrypting(ITvCardHandler tvcard)
    {
      return tvcard.Card.NumberOfChannelsDecrypting;
    }

    protected virtual bool IsCamAlreadyDecodingChannel(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      return tvcard.UserManagement.IsAnyUserOnTuningDetail(tuningDetail);      
    }

    protected static bool IsValidTuningDetails(ICollection<IChannel> tuningDetails)
    {
      return (tuningDetails != null && tuningDetails.Count > 0);
    }

    public virtual bool CheckTransponder(IUser user, ITvCardHandler tvcard, IChannel tuningDetail)
    {
      int cardId = tvcard.Card.TunerId;

      // TODO: The tuner owner can do whatever they want, but taking control
      // could require kicking users to stay within the decrypt limit. This is
      // not handled in the code.
      if (IsOwnerOfCard(tvcard, user))
      {
        if (LogEnabled)
        {
          this.LogInfo("Controller:    card:{0} type:{1} is available", cardId, tvcard.Card.SupportedBroadcastStandards);
        }
        return true;
      }

      if (!IsSameTransponder(tvcard, tuningDetail))
      {
        if (LogEnabled)
        {
          this.LogInfo("Controller:    card:{0} type:{1} is not available, tuned to different transmitter",
                    cardId, tvcard.Card.SupportedBroadcastStandards);
        }
        return false;
      }

      // The tuner is in use, but it is tuned to the transmitter we need to
      // tune so we can use it.
      if (!tuningDetail.IsEncrypted)
      {
        if (LogEnabled)
        {
          this.LogInfo(
            "Controller:    card:{0} type:{1} is available, tuned to same transmitter",
            cardId, tvcard.Card.SupportedBroadcastStandards);
        }
        return true;
      }

      // If the channel is encrypted check the tuner's decrypt limit and
      // decryptable provider list.
      if (
        tvcard.Card.ConditionalAccessProviders.Count > 0 &&
        !string.IsNullOrEmpty(tuningDetail.Provider) &&
        !tvcard.Card.ConditionalAccessProviders.Contains(tuningDetail.Provider)
      )
      {
        if (LogEnabled)
        {
          this.LogInfo(
            "Controller:    card:{0} type:{1} is not available, tuned to same transmitter but provider not decryptable",
            cardId, tvcard.Card.SupportedBroadcastStandards);
        }
        return false;
      }

      // Does the decrypt limit prevent the tuner from decrypting the channel?
      if (!IsCamAlreadyDecodingChannel(tvcard, tuningDetail) && !IsCamAbleToDecryptChannel(user, tvcard, tuningDetail, tvcard.Card.DecryptLimit))
      {
        if (LogEnabled)
        {
          this.LogInfo(
            "Controller:    card:{0} type:{1} is not available, tuned to same transmitter but decrypt limit {2} reached",
            cardId, tvcard.Card.SupportedBroadcastStandards, tvcard.Card.DecryptLimit);
        }
        return false;
      }
      if (LogEnabled)
      {
        this.LogInfo(
          "Controller:    card:{0} type:{1} is available, tuned to same transmitter decrypting {2}/{3} channels",
          cardId, tvcard.Card.SupportedBroadcastStandards, NumberOfChannelsDecrypting(tvcard), tvcard.Card.DecryptLimit);
      }
      return true;
    }

    private static bool HasEqualOrHigherPriority(ITvCardHandler tvcard, IUser user)
    {
      return tvcard.UserManagement.HasEqualOrHigherPriority(user);
    }

    private static bool HasHighestPriority(ITvCardHandler tvcard, IUser user)
    {
      return tvcard.UserManagement.HasHighestPriority(user);
    }

    protected virtual bool IsOwnerOfCard(ITvCardHandler tvcard, IUser user)
    {      
      bool hasHighestPriority = HasHighestPriority(tvcard, user);
      bool isOwnerOfCard = false;

      if (hasHighestPriority)
      {
        isOwnerOfCard = true;
      }
      else
      {
        bool hasEqualOrHigherPriority = HasEqualOrHigherPriority(tvcard, user);
        if (hasEqualOrHigherPriority)
        {
          isOwnerOfCard = tvcard.UserManagement.IsOwner(user.Name);
        }
      }

      return isOwnerOfCard;
    }

    protected virtual bool IsSameTransponder(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      return tvcard.Tuner.IsTunedToTransponder(tuningDetail);
    }

    #endregion
  }
}