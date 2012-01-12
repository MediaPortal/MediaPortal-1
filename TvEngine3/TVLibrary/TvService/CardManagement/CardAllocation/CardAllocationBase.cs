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
using System.Diagnostics;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Log;

#endregion

namespace TvService
{
  public abstract class CardAllocationBase
  {
    private bool _logEnabled = true;

    public bool LogEnabled
    {
      get { return _logEnabled; }
      set { _logEnabled = value; }
    }

    protected readonly TvBusinessLayer _businessLayer;

    protected readonly TVController Controller;

    protected CardAllocationBase(TvBusinessLayer businessLayer, TVController controller)
    {
      _businessLayer = businessLayer;
      Controller = controller;
    }

    #region protected members            

    protected bool IsCamAbleToDecryptChannel(IUser user, ITvCardHandler tvcard, IChannel tuningDetail, int decryptLimit)
    {
      if (!tuningDetail.FreeToAir)
      {
        bool isCamAbleToDecryptChannel = true;
        if (decryptLimit > 0)
        {
          int camDecrypting = tvcard.NumberOfChannelsDecrypting;

          //Check if the user is currently occupying a decoding slot and subtract that from the number of channels it is decoding.
          if (user.CardId == tvcard.DataBaseCard.IdCard)
          {
            IChannel currentUserCh = tvcard.CurrentChannel(ref user);
            if (currentUserCh != null && !currentUserCh.FreeToAir)
            {
              int currentChannelId = tvcard.CurrentDbChannel(ref user);
              int numberOfUsersOnCurrentChannel = 0;
              foreach (IUser aUser in tvcard.Users.GetUsers())
              {
                if (aUser.IdChannel == currentChannelId)
                {
                  ++numberOfUsersOnCurrentChannel;
                }
              }

              //Only subtract the slot the user is currently occupying if he is the only user occupying the slot.
              if (numberOfUsersOnCurrentChannel == 1)
              {
                --camDecrypting;
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

    protected bool IsCamAlreadyDecodingChannel(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      bool isCamAlreadyDecodingChannel = false;
      IUser[] currentUsers = tvcard.Users.GetUsers();
      if (currentUsers != null)
      {
        for (int i = 0; i < currentUsers.Length; ++i)
        {
          IUser tmpUser = currentUsers[i];
          if (tvcard.CurrentChannel(ref tmpUser).Equals(tuningDetail))
          {
            //yes, cam already is descrambling this channel
            isCamAlreadyDecodingChannel = true;
            break;
          }
        }
      }
      return isCamAlreadyDecodingChannel;
    }

    protected bool IsChannelMappedToCard(Channel dbChannel, Card card)
    {
      //check if channel is mapped to this card and that the mapping is not for "Epg Only"
      bool isChannelMappedToCard = _businessLayer.IsChannelMappedToCard(dbChannel, card, false);
      return isChannelMappedToCard;
    }

    protected bool IsValidTuningDetails(IList<IChannel> tuningDetails)
    {
      bool isValid = (tuningDetails != null && tuningDetails.Count > 0);
      return isValid;
    }

    protected bool CheckTransponder(IUser user, ITvCardHandler tvcard, int decryptLimit, int cardId,
                                    IChannel tuningDetail)
    {
      bool checkTransponder = true;
      bool isSameTransponder = IsSameTransponder(tvcard, tuningDetail);
      bool isOwnerOfCard = tvcard.Users.IsOwner(user);

      //FIXME: Being card owner you can do whatever you want, but in case of decryptlimit that could mean kicking users. This is not handled in the code.
      if (isOwnerOfCard)
      {
        if (LogEnabled)
        {
          Log.Info("Controller:    card:{0} type:{1} is available", cardId, tvcard.Type);
        }
      }
      else
      {
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
                  Log.Info(
                    "Controller:    card:{0} type:{1} is available, tuned to same transponder decrypting {2}/{3} channels",
                    cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, decryptLimit);
                }
              }
              else
              {
                if (LogEnabled)
                {
                  Log.Info(
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
                Log.Info(
                  "Controller:    card:{0} type:{1} is not available, tuned to same transponder decrypting {2}/{3} channels (cam limit reached)",
                  cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, decryptLimit);
              }
              checkTransponder = false;
            }
          }
        }
        else
        {
          if (LogEnabled)
          {
            Log.Info("Controller:    card:{0} type:{1} is not available, tuned to different transponder",
                     cardId, tvcard.Type);
          }
          checkTransponder = false;
        }
      }
      return checkTransponder;
    }

    protected bool IsSameTransponder(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      return tvcard.Tuner.IsTunedToTransponder(tuningDetail) &&
             tvcard.SupportsSubChannels;
    }

    #endregion
  }
}