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
using System.Threading;
using System.Runtime.CompilerServices;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;

namespace TvService
{
  public class AdvancedCardAllocation : CardAllocationBase, ICardAllocation
  {
    #region private members    

    private static bool IsCardFoundEnabled(List<CardDetail> cardsAvailable, KeyValuePair<int, ITvCardHandler> keyPair)
    {
      //get the card info
      bool isEnabled = false;
      bool isFound = true;
      foreach (CardDetail info in cardsAvailable)
      {
        if (info.Card.DevicePath == keyPair.Value.DataBaseCard.DevicePath)
        {
          isFound = false;
        }
      }

      if (isFound)
      {
        //check if card is enabled
        isEnabled = keyPair.Value.DataBaseCard.Enabled;
      }
      return isEnabled;
    }

    private static bool IsCardPresent(int cardId, string hostName)
    {
      bool isCardPresent = false;
      try
      {
        RemoteControl.HostName = hostName;
        if (RemoteControl.Instance.CardPresent(cardId))
        {
          isCardPresent = true;
        }
      }
      catch (Exception)
      {
        isCardPresent = true;
      }
      return isCardPresent;
    }

    private bool CheckTransponder(Channel dbChannel, User user, ITvCardHandler tvcard, int decryptLimit, int cardId,
                                  IChannel tuningDetail, bool checkTransponders, out bool isSameTransponder,
                                  out bool canDecrypt)
    {
      bool checkTransponder = true;
      isSameTransponder = (tvcard.Tuner.IsTunedToTransponder(tuningDetail) &&
                           (tvcard.SupportsSubChannels || (!checkTransponders)));
      canDecrypt = false;

      bool isOwnerOfCard = tvcard.Users.IsOwner(user);

      if (isSameTransponder)
      {
        if (!isOwnerOfCard)
        {
          //card is in use, but it is tuned to the same transponder.
          //meaning.. we can use it.
          if (tvcard.HasCA && decryptLimit > 0)
            //does the card have a CA module and a CA limit, if yes then proceed to check cam decrypt limit.
          {
            //but we must check if cam can decode the extra channel as well
            //first check if cam is already decrypting this channel          
            bool isCamAlreadyDecodingChannel = IsCamAlreadyDecodingChannel(tvcard, dbChannel);

            //if the user is already using this card
            //and is watching a scrambled signal
            //then we must the CAM will always be able to watch the requested channel
            //since the users zaps

            //check if cam is capable of descrambling an extra channel                
            bool isRec = false;
            bool isCamAbleToDecrypChannel = IsCamAbleToDecrypChannel(user, tvcard, dbChannel, decryptLimit, out isRec);

            if (isCamAbleToDecrypChannel || isCamAlreadyDecodingChannel)
            {
              Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels",
                       cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, decryptLimit);
              isSameTransponder = true;
            }
            else
            {
              //it is not, skip this card
              Log.Info(
                "Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels. cam limit reached",
                cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, decryptLimit);

              //allow admin users like the scheduler to use this card anyway       
              checkTransponder = (!user.IsAdmin || (user.IsAdmin && isRec));
            }
          }
        } //end of cam present block              
        else // no cam present
        {
          Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder no CA present", cardId, tvcard.Type);
          isSameTransponder = true;
        }
      }
      else
      {
        //different transponder, are we the owner of this card?
        //allow admin users like the scheduler to use this card anyway
        if (!isOwnerOfCard && !user.IsAdmin)
        {
          Log.Info("Controller:    card:{0} type:{1} is tuned to different transponder", cardId, tvcard.Type);
          checkTransponder = false;
        }
      }
      return checkTransponder;
    }

    private int NumberOfUsersOnCard(Dictionary<int, ITvCardHandler> cards, User user, CardDetail cardInfo)
    {
      //determine how many other users are using this card
      int nrOfOtherUsers = 0;
      User[] users = cards[cardInfo.Id].Users.GetUsers();
      if (users != null)
      {
        for (int i = 0; i < users.Length; ++i)
        {
          if (users[i].Name != user.Name)
            nrOfOtherUsers++;
        }
      }
      return nrOfOtherUsers;
    }

    private int CardPriority(int priority, int nrOfOtherUsers, bool sameTransponder, bool canDecrypt, int cardId,
                             int recommendedCardId)
    {
      //if there are other users on this card and we want to switch to another transponder
      //then set this cards priority as very low...
      if (nrOfOtherUsers > 0 && !sameTransponder)
      {
        priority -= 100;
      }

      // handle recommended cardid.
      // boost priority if a cardid matches, but only if these criterias are met.
      // A) if card is free while other cards are busy.
      // B) if card is busy (but decryption slot available) while other cards are busy or free.

      if (recommendedCardId == cardId)
      {
        if (nrOfOtherUsers == 0 || canDecrypt)
        {
          priority += 100;
        }
      }

      return priority;
    }

    #endregion

    #region ICardAllocation Members

    /// <summary>
    /// Gets a list of all free cards which can receive the channel specified
    /// List is sorted by priority
    /// </summary>
    /// <returns>list containg all free cards which can receive the channel</returns>
    public List<CardDetail> GetAvailableCardsForChannel(Dictionary<int, ITvCardHandler> cards, Channel dbChannel,
                                                        ref User user, bool checkTransponders, out TvResult result,
                                                        int recommendedCardId)
    {
      try
      {

        //Log.Info("GetAvailableCardsForChannel st {0}", Environment.StackTrace);
        //construct list of all cards we can use to tune to the new channel
        List<CardDetail> cardsAvailable = new List<CardDetail>();
        List<int> cardsUnAvailable = new List<int>();

        

        Log.Info("Controller: find free card for channel {0}", dbChannel.Name);
        TvBusinessLayer layer = new TvBusinessLayer();

        //get the tuning details for the channel
        List<IChannel> tuningDetails = layer.GetTuningChannelByName(dbChannel);

        bool isValidTuningDetails = IsValidTuningDetails(tuningDetails);
        if (!isValidTuningDetails)
        {
          //no tuning details??
          Log.Info("Controller:  No tuning details for channel:{0}", dbChannel.Name);
          result = TvResult.NoTuningDetails;
          return cardsAvailable;
        }

        Log.Info("Controller:   got {0} tuning details for {1}", tuningDetails.Count, dbChannel.Name);

        int cardsFound = 0;
        int number = 0;
        //foreach tuning detail
        foreach (IChannel tuningDetail in tuningDetails)
        {
          cardsUnAvailable.Clear();
          number++;
          Log.Info("Controller:   channel #{0} {1} ", number, tuningDetail.ToString());
          Dictionary<int, ITvCardHandler>.Enumerator enumerator = cards.GetEnumerator();

          //for each card...
          while (enumerator.MoveNext())
          {
            KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
            int cardId = keyPair.Value.DataBaseCard.IdCard;

            if (cardsUnAvailable.Contains(cardId))
            {
              Log.Info("Controller:    card:{0} has already been queried, skipping.", cardId);
              continue;              
            }

            ITvCardHandler tvcard = cards[cardId];
            
            bool isCardFoundAndEnabled = IsCardFoundEnabled(cardsAvailable, keyPair);
            if (!isCardFoundAndEnabled)
            {
              //not enabled, so skip the card
              Log.Info("Controller:    card:{0} type:{1} is disabled", cardId, tvcard.Type);
              AddCardUnAvailable(ref cardsUnAvailable, cardId);
              continue;
            }

            bool isCardPresent = IsCardPresent(cardId, keyPair.Value.DataBaseCard.ReferencedServer().HostName);
            if (!isCardPresent)
            {
              Log.Error("card: unable to connect to slave controller at:{0}",
                        keyPair.Value.DataBaseCard.ReferencedServer().HostName);
              AddCardUnAvailable(ref cardsUnAvailable, cardId);
              continue;
            }

            if (!tvcard.Tuner.CanTune(tuningDetail))
            {
              //card cannot tune to this channel, so skip it
              Log.Info("Controller:    card:{0} type:{1} cannot tune to channel", cardId, tvcard.Type);
              AddCardUnAvailable(ref cardsUnAvailable, cardId);
              continue;
            }

            //check if channel is mapped to this card and that the mapping is not for "Epg Only"
            ChannelMap channelMap = null;
            bool isChannelMappedToCard = IsChannelMappedToCard(dbChannel, keyPair, out channelMap);
            if (!isChannelMappedToCard)
            {
              Log.Info("Controller:    card:{0} type:{1} channel not mapped", cardId, tvcard.Type);
              AddCardUnAvailable(ref cardsUnAvailable, cardId);
              continue;
            }

            //ok card could be used to tune to this channel
            //now we check if its free...
            cardsFound++;
            bool isSameTransponder = false;
            bool canDecrypt = true;
            int decryptLimit = keyPair.Value.DataBaseCard.DecryptLimit;

            bool checkTransponder = CheckTransponder(dbChannel, user, tvcard, decryptLimit, cardId, tuningDetail,
                                                     checkTransponders, out isSameTransponder, out canDecrypt);
            if (!checkTransponder)
            {
              AddCardUnAvailable(ref cardsUnAvailable, cardId);
              continue;
            }

            CardDetail cardInfo = new CardDetail(cardId, channelMap.ReferencedCard(), tuningDetail);

            //determine how many other users are using this card            
            int nrOfOtherUsers = NumberOfUsersOnCard(cards, user, cardInfo);
            cardInfo.Priority = CardPriority(cardInfo.Priority, nrOfOtherUsers, isSameTransponder, canDecrypt, cardId,
                                             recommendedCardId);

            Log.Info("Controller:    card:{0} type:{1} is available priority:{2} #users:{3} same transponder:{4}",
                     cardInfo.Id, tvcard.Type, cardInfo.Priority, nrOfOtherUsers, isSameTransponder);

            cardsAvailable.Add(cardInfo);
          }
        }


        //sort cards on priority
        cardsAvailable.Sort();

        if (cardsAvailable.Count > 0)
        {
          result = TvResult.Succeeded;
        }
        else
        {
          result = cardsFound == 0 ? TvResult.ChannelNotMappedToAnyCard : TvResult.AllCardsBusy;
        }
        Log.Info("Controller: found {0} available", cardsAvailable.Count);

        return cardsAvailable;
      }
      catch (Exception ex)
      {
        result = TvResult.UnknownError;
        Log.Write(ex);
        return null;
      }
    }

    private void AddCardUnAvailable(ref List<int> cardsUnAvailable, int cardId)
    {
      if (!cardsUnAvailable.Contains(cardId))
      {
        cardsUnAvailable.Add(cardId);
      }
    }

    #endregion

    #region public members       

    #endregion
  }
}