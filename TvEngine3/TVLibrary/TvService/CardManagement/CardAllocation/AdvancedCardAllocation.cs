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

#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;

#endregion

namespace TvService
{
  public class AdvancedCardAllocation : CardAllocationBase, ICardAllocation
  {
    #region private members    

    private static bool IsCardFoundAndEnabled(List<CardDetail> cardsAvailable, ITvCardHandler cardHandler)
    {      
      //get the card info
      bool isEnabled = false;      

      if (cardsAvailable != null)
      {
        bool isFound = !(cardsAvailable.Exists(c => c.Card.DevicePath == cardHandler.DataBaseCard.DevicePath));

        if (isFound)
        {
          //check if card is enabled
          isEnabled = cardHandler.DataBaseCard.Enabled;
        }
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
        if (isOwnerOfCard)
        {
          Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder", cardId, tvcard.Type);          
        } 
        else
        {
          //card is in use, but it is tuned to the same transponder.
          //meaning.. we can use it.          
          if (tvcard.HasCA)
          //does the card have a CA module and a CA limit, if yes then proceed to check cam decrypt limit.
          {
            if (decryptLimit > 0)
            {
              //but we must check if cam can decode the extra channel as well
              //first check if cam is already decrypting this channel          
              bool isCamAlreadyDecodingChannel = IsCamAlreadyDecodingChannel(tvcard, dbChannel);

              //if the user is already using this card
              //and is watching a scrambled signal
              //then we must the CAM will always be able to watch the requested channel
              //since the users zaps

              //check if cam is capable of descrambling an extra channel                            
              bool isCamAbleToDecrypChannel = IsCamAbleToDecrypChannel(user, tvcard, dbChannel, decryptLimit);

              canDecrypt = isCamAbleToDecrypChannel || isCamAlreadyDecodingChannel;

              if (canDecrypt)
              {
                Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels",
                         cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, decryptLimit);
              }
              else
              {
                //it is not, skip this card
                Log.Info(
                  "Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels. cam limit reached",
                  cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, decryptLimit);
              }

              checkTransponder = (user.IsAdmin || canDecrypt); 
            }            
          }
          else //fta
          {
            canDecrypt = true;
          }
        }
      }
      else
      {
        bool isRecordingAnyUser = tvcard.Recorder.IsRecordingAnyUser();
        Log.Info("Controller:    card:{0} type:{1} is tuned to different transponder", cardId, tvcard.Type);
        canDecrypt = true;
        checkTransponder = ((user.IsAdmin && !isRecordingAnyUser) || isOwnerOfCard);
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
      Stopwatch stopwatch = Stopwatch.StartNew();
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

        Dictionary<int, bool> cardsUsedByUser = GetCardsUsedByUser(cards, user);

        Dictionary<int, ITvCardHandler>.ValueCollection cardHandlers = null;
        if (tuningDetails != null && tuningDetails.Count > 0)
        {
          cardHandlers = cards.Values;  
        }

        
        
        foreach (IChannel tuningDetail in tuningDetails)
        {
          cardsUnAvailable.Clear();
          number++;
          Log.Info("Controller:   channel #{0} {1} ", number, tuningDetail.ToString());                    
          foreach (ITvCardHandler cardHandler in cardHandlers)
          {                        
            int cardId = cardHandler.DataBaseCard.IdCard;

            if (cardsUnAvailable.Contains(cardId))
            {
              Log.Info("Controller:    card:{0} has already been queried, skipping.", cardId);
              continue;              
            }            

            bool isCardFoundAndEnabled = IsCardFoundAndEnabled(cardsAvailable, cardHandler);
            if (!isCardFoundAndEnabled)
            {
              //not enabled, so skip the card
              Log.Info("Controller:    card:{0} type:{1} is disabled", cardId, cardHandler.Type);
              AddCardUnAvailable(ref cardsUnAvailable, cardId);
              continue;
            }

            bool isCardPresent = IsCardPresent(cardId, cardHandler.DataBaseCard.ReferencedServer().HostName);
            if (!isCardPresent)
            {
              Log.Error("card: unable to connect to slave controller at:{0}",
                        cardHandler.DataBaseCard.ReferencedServer().HostName);
              AddCardUnAvailable(ref cardsUnAvailable, cardId);
              continue;
            }

            if (!cardHandler.Tuner.CanTune(tuningDetail))
            {
              //card cannot tune to this channel, so skip it
              Log.Info("Controller:    card:{0} type:{1} cannot tune to channel", cardId, cardHandler.Type);
              AddCardUnAvailable(ref cardsUnAvailable, cardId);
              continue;
            }

            //check if channel is mapped to this card and that the mapping is not for "Epg Only"
            ChannelMap channelMap = null;
            bool isChannelMappedToCard = IsChannelMappedToCard(dbChannel, cardHandler.DataBaseCard.DevicePath, out channelMap);
            if (!isChannelMappedToCard)
            {
              Log.Info("Controller:    card:{0} type:{1} channel not mapped", cardId, cardHandler.Type);
              AddCardUnAvailable(ref cardsUnAvailable, cardId);
              continue;
            }

            CardDetail cardInfo = null;
            int nrOfOtherUsers = 0;
            bool isSameTransponder = false;

            bool isOnlyActiveUserCurrentUser = true;
            cardsUsedByUser.TryGetValue(cardHandler.DataBaseCard.IdCard, out isOnlyActiveUserCurrentUser);

            if (isOnlyActiveUserCurrentUser)
            {
              isSameTransponder = true;
              nrOfOtherUsers = 0;
              cardInfo = new CardDetail(cardId, channelMap.ReferencedCard(), tuningDetail);
              cardInfo.Priority = CardPriority(cardInfo.Priority, nrOfOtherUsers, isSameTransponder, true, cardId,
                                            recommendedCardId);              
            }
            else
            {
              //ok card could be used to tune to this channel
              //now we check if its free...
              cardsFound++;
              isSameTransponder = false;
              bool canDecrypt = true;
              int decryptLimit = cardHandler.DataBaseCard.DecryptLimit;

              bool checkTransponder = CheckTransponder(dbChannel, user, cardHandler, decryptLimit, cardId, tuningDetail,
                                                       checkTransponders, out isSameTransponder, out canDecrypt);
              
              if (!checkTransponder)
              {
                AddCardUnAvailable(ref cardsUnAvailable, cardId);
                continue;
              }

              cardInfo = new CardDetail(cardId, channelMap.ReferencedCard(), tuningDetail);
              cardInfo.CanDecrypt = canDecrypt;

              //determine how many other users are using this card                        
              nrOfOtherUsers = NumberOfUsersOnCard(cards, user, cardInfo);
              cardInfo.Priority = CardPriority(cardInfo.Priority, nrOfOtherUsers, isSameTransponder, canDecrypt, cardId,
                                            recommendedCardId);
            }
                        
            Log.Info("Controller:    card:{0} type:{1} is available priority:{2} #users:{3} same transponder:{4}",
                     cardInfo.Id, cardHandler.Type, cardInfo.Priority, nrOfOtherUsers, isSameTransponder);

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
      finally
      {
        stopwatch.Stop();        
        Log.Info("AdvancedCardAllocation.GetAvailableCardsForChannel took {0} msec", stopwatch.ElapsedMilliseconds);
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