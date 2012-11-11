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

#region usings 

using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

#endregion

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
      if (!tuningDetail.FreeToAir)
      {
        bool isCamAbleToDecryptChannel = true;
        if (decryptLimit > 0)
        {
          int camDecrypting = NumberOfChannelsDecrypting(tvcard);

          //Check if the user is currently occupying a decoding slot and subtract that from the number of channels it is decoding.
          if (user.CardId == tvcard.DataBaseCard.IdCard)
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
      return true;
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
      return (currentUserCh != null && currentUserCh.FreeToAir);
    }

    protected virtual int NumberOfChannelsDecrypting(ITvCardHandler tvcard)
    {
      return tvcard.NumberOfChannelsDecrypting;
    }

    protected virtual bool IsCamAlreadyDecodingChannel(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      bool isCamAlreadyDecodingChannel = tvcard.UserManagement.IsAnyUserOnTuningDetail(tuningDetail);      
      return isCamAlreadyDecodingChannel;
    }



    protected static bool IsValidTuningDetails(ICollection<IChannel> tuningDetails)
    {
      bool isValid = (tuningDetails != null && tuningDetails.Count > 0);
      return isValid;
    }

    public virtual bool CheckTransponder(IUser user, ITvCardHandler tvcard, IChannel tuningDetail)
    {
      int decryptLimit = tvcard.DataBaseCard.DecryptLimit;
      int cardId = tvcard.DataBaseCard.IdCard;      

      bool checkTransponder = true;
      bool isOwnerOfCard = IsOwnerOfCard(tvcard, user);

      //TODO: Being card owner you can do whatever you want, but in case of decryptlimit that could mean kicking users. This is not handled in the code.
      if (isOwnerOfCard)
      {
        if (LogEnabled)
        {
          this.LogInfo("Controller:    card:{0} type:{1} is available", cardId, tvcard.Type);
        }
      }
      else
      {
        bool isSameTransponder = IsSameTransponder(tvcard, tuningDetail);
        if (isSameTransponder)
        {
          //card is in use, but it is tuned to the same transponder.
          //meaning.. we can use it.          
          //if the channel is encrypted check cam decrypt limit.
          if (!tuningDetail.FreeToAir)
          {
            //but we must check if cam can decode the extra channel as well
            //first check if cam is already decrypting this channel          
            bool canDecrypt = false;
            bool isCamAlreadyDecodingChannel = IsCamAlreadyDecodingChannel(tvcard, tuningDetail);

            if (!isCamAlreadyDecodingChannel)
            {
              //check if cam is capable of descrambling an extra channel                            
              bool isCamAbleToDecrypChannel = IsCamAbleToDecryptChannel(user, tvcard, tuningDetail, decryptLimit);
              if (isCamAbleToDecrypChannel)
              {
                canDecrypt = true;
              }
            }
            else
            {
              canDecrypt = true;
            }

            if (canDecrypt)
            {
              if (decryptLimit > 0)
              {
                if (LogEnabled)
                {
                  this.LogInfo(
                    "Controller:    card:{0} type:{1} is available, tuned to same transponder decrypting {2}/{3} channels",
                    cardId, tvcard.Type, NumberOfChannelsDecrypting(tvcard), decryptLimit);
                }
              }
              else
              {
                if (LogEnabled)
                {
                  this.LogInfo(
                    "Controller:    card:{0} type:{1} is available, tuned to same transponder",
                    cardId, tvcard.Type);
                }
              }
            }
            else
            {
              //it is not, skip this card
              if (LogEnabled)
              {
                this.LogInfo(
                  "Controller:    card:{0} type:{1} is not available, tuned to same transponder decrypting {2}/{3} channels (cam limit reached)",
                  cardId, tvcard.Type, NumberOfChannelsDecrypting(tvcard), decryptLimit);
              }
              checkTransponder = false;
            }
          }
        }
        else
        {
          if (LogEnabled)
          {
            this.LogInfo("Controller:    card:{0} type:{1} is not available, tuned to different transponder",
                     cardId, tvcard.Type);
          }
          checkTransponder = false;
        }
      }
      return checkTransponder;
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
      return tvcard.Tuner.IsTunedToTransponder(tuningDetail) &&
             tvcard.SupportsSubChannels;
    }

    #endregion
  }
}