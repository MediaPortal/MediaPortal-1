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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;

#endregion

namespace TvService
{
  public abstract class CardAllocationBase
  {
    private bool _logEnabled = true;

    protected bool LogEnabled
    {
      get { return _logEnabled; }
      set { _logEnabled = value; }
    }
    
    protected readonly TvBusinessLayer _businessLayer;
    protected readonly TVController _controller;

    protected CardAllocationBase(TvBusinessLayer businessLayer, TVController controller)
    {      
      _businessLayer = businessLayer;
      _controller = controller;
      //_tuningChannelMapping = new Dictionary<int, IList<IChannel>>();
      //_channelMapping = new Dictionary<int, bool>();
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
            bool isFreeToAir = IsFreeToAir(tvcard, ref user);
            if (!isFreeToAir)
            {
              int numberOfUsersOnCurrentChannel = GetNumberOfUsersOnCurrentChannel(tvcard, user);

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

    protected virtual int GetNumberOfUsersOnCurrentChannel(ITvCardHandler tvcard, IUser user) 
    {
      int currentChannelId = tvcard.CurrentDbChannel(ref user);
      return tvcard.Users.GetUsers().Count(aUser => aUser.IdChannel == currentChannelId);
    }

    protected virtual bool IsFreeToAir(ITvCardHandler tvcard, ref IUser user)
    {      
      IChannel currentUserCh = tvcard.CurrentChannel(ref user);
      return (currentUserCh != null && currentUserCh.FreeToAir);
    }

    protected virtual int NumberOfChannelsDecrypting(ITvCardHandler tvcard)
    {
      return tvcard.NumberOfChannelsDecrypting;
    }

    protected virtual bool IsCamAlreadyDecodingChannel(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      bool isCamAlreadyDecodingChannel = false;
      IUser[] currentUsers = tvcard.Users.GetUsers();
      if (currentUsers != null)
      {
        for (int i = 0; i < currentUsers.Length; ++i)
        {
          IUser tmpUser = currentUsers[i];
          IChannel currentChannel = tvcard.CurrentChannel(ref tmpUser);
          if (currentChannel != null && currentChannel.Equals(tuningDetail))
          {
            //yes, cam already is descrambling this channel
            isCamAlreadyDecodingChannel = true;
            break;
          }
        }
      }
      return isCamAlreadyDecodingChannel;
    }

    protected static bool IsValidTuningDetails(ICollection<IChannel> tuningDetails)
    {
      bool isValid = (tuningDetails != null && tuningDetails.Count > 0);
      return isValid;
    }

    public virtual bool CheckTransponder(IUser user, ITvCardHandler tvcard, IChannel tuningDetail, int checkLevel)
    {
      int cardId = tvcard.DataBaseCard.IdCard;
      bool checkTransponder = true;
      bool isOwnerOfCard = IsOwnerOfCard(tvcard, user);
      
      //WIP code to be able to handle card owner you can do whatever you want, but in case of decryptlimit that could mean kicking users.
      
      if (checkLevel == 1)
      {      
        //Check if the only users of the card are the requesting user, epg grabber or none
        //If so, the requesting user can become the 'owner' of the card without kicking other users
        int otherUsersOnCard = NumberOfOtherUsersOnCard(tvcard, user);
        isOwnerOfCard = (otherUsersOnCard > 0) ? false : true;
      }
      
      if (checkLevel > 0)
      {      
        if (isOwnerOfCard)
        {
          //The owner can do whatever they want, including kicking off current user,
          //so return transponder as 'available'
          if (LogEnabled)
          {
            Log.Debug("CheckTransponder:  checkLevel:{0}, card:{1} type:{2} is available, requesting user:{3}",
                     checkLevel, cardId, tvcard.Type, user.Name);
          }
          return true;
        }
        else if (checkLevel < 3)
        {
          if (LogEnabled)
          {
            Log.Debug("CheckTransponder:  checkLevel:{0}, card:{1} type:{2} is not available, could not kick current owner, requesting user:{3}",
                     checkLevel, cardId, tvcard.Type, user.Name);
          }
          return false;
        }
      }

      //We are on checkLevel 0 or 3, so check if we can create a sub-channel on same transponder
      bool isSameTransponder = IsSameTransponder(tvcard, tuningDetail); //Check if already tuned to transponder and it supports sub-channels
      int decryptLimit = tvcard.DataBaseCard.DecryptLimit;
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
                Log.Debug(
                  "CheckTransponder:  checkLevel:{0}, card:{1} type:{2} is available, tuned to same transponder decrypting {3}/{4} channels, requesting user:{5}",
                  checkLevel, cardId, tvcard.Type, NumberOfChannelsDecrypting(tvcard), decryptLimit, user.Name);
              }
            }
            else
            {
              if (LogEnabled)
              {
                Log.Debug(
                  "CheckTransponder:  checkLevel:{0}, card:{1} type:{2} is available, tuned to same transponder, can decrypt, requesting user:{3}",
                  checkLevel, cardId, tvcard.Type, user.Name);
              }
            }
          }
          else
          {
            //it is not, skip this card
            if (LogEnabled)
            {
              Log.Debug(
                "CheckTransponder:  checkLevel:{0}, card:{1} type:{2} is not available, tuned to same transponder decrypting {3}/{4} channels (cam limit reached), requesting user:{5}",
                checkLevel, cardId, tvcard.Type, NumberOfChannelsDecrypting(tvcard), decryptLimit, user.Name);
            }
            checkTransponder = false;
          }
        }
        else
        {
          if (LogEnabled)
          {
            Log.Debug(
              "CheckTransponder:  checkLevel:{0}, card:{1} type:{2} is available, tuned to same transponder, requesting user:{3}",
              checkLevel, cardId, tvcard.Type, user.Name);
          }
        }
      }
      else
      {
        if (LogEnabled)
        {
          Log.Debug("CheckTransponder:  checkLevel:{0}, card:{1} type:{2} tuned to different transponder or sub-channels not supported, requesting user:{3}",
                   checkLevel, cardId, tvcard.Type, user.Name);
        }
        checkTransponder = false;
      }
      return checkTransponder;
    }

    private static bool HasEqualOrHigherPriority(ITvCardHandler tvcard, IUser user)
    {
      return tvcard.Users.HasEqualOrHigherPriority(user);
    }

    private static bool HasHighestPriority(ITvCardHandler tvcard, IUser user)
    {
      return tvcard.Users.HasHighestPriority(user);
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
          isOwnerOfCard = tvcard.Users.IsOwner(user);
        }
      }

      return isOwnerOfCard;
    }

    protected virtual bool IsSameTransponder(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      return tvcard.Tuner.IsTunedToTransponder(tuningDetail) &&
             tvcard.SupportsSubChannels;
    }

    protected virtual int NumberOfOtherUsersOnCard(ITvCardHandler card, IUser user)
    {
      //determine how many other users are using this card
      int nrOfOtherUsers = 0;
      IUser[] users = card.Users.GetUsers();
      if (users != null)
      {
        nrOfOtherUsers = users.Count(t => t.Name != user.Name && !t.Name.Equals("epg"));
      }
      return nrOfOtherUsers;
    }

    protected virtual bool IsStatic()
    {
      return false;
    }

    #endregion    
  }
}