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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    public AdvancedCardAllocation(TvBusinessLayer businessLayer, TVController controller)
      : base(businessLayer, controller) {}

    private static bool IsCardEnabled(ITvCardHandler cardHandler)
    {
      return cardHandler.DataBaseCard.Enabled;
    }

    private bool IsCardPresent(int cardId)
    {
      bool isCardPresent = false;
      try
      {
        if (Controller.CardPresent(cardId))
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


    private static int NumberOfOtherUsersOnCard(ITvCardHandler card, IUser user)
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

    #endregion

    #region ICardAllocation Members

    /// <summary>
    /// Gets a list of all free cards which can receive the channel specified
    /// List is sorted.
    /// </summary>
    /// <returns>list containg all free cards which can receive the channel</returns>
    public List<CardDetail> GetFreeCardsForChannel(Dictionary<int, ITvCardHandler> cards, Channel dbChannel,
                                                   ref IUser user, out TvResult result)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        //Log.Info("GetFreeCardsForChannel st {0}", Environment.StackTrace);
        //construct list of all cards we can use to tune to the new channel
        if (LogEnabled)
        {
          Log.Info("Controller: find free card for channel {0}", dbChannel.DisplayName);
        }
        var cardsAvailable = new List<CardDetail>();


        Dictionary<int, TvResult> cardsUnAvailable;
        List<CardDetail> cardDetails = GetAvailableCardsForChannel(cards, dbChannel, ref user, out cardsUnAvailable);
        foreach (CardDetail cardDetail in cardDetails)
        {
          bool checkTransponder = CheckTransponder(user, cards[cardDetail.Card.IdCard], cardDetail.Card.DecryptLimit,
                                                   cardDetail.Card.IdCard, cardDetail.TuningDetail);
          if (checkTransponder)
          {
            cardsAvailable.Add(cardDetail);
          }
        }


        //sort cards
        cardsAvailable.Sort();

        if (cardsAvailable.Count > 0)
        {
          result = TvResult.Succeeded;
        }
        else
        {
          TvResult resultNoCards = GetResultNoCards(cardsUnAvailable);
          result = cardDetails.Count == 0 ? resultNoCards : TvResult.AllCardsBusy;
        }
        if (LogEnabled)
        {
          Log.Info("Controller: found {0} free card(s)", cardsAvailable.Count);
        }

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
        Log.Info("AdvancedCardAllocation.GetFreeCardsForChannel took {0} msec", stopwatch.ElapsedMilliseconds);
      }
    }

    private static TvResult GetResultNoCards(Dictionary<int, TvResult> cardsUnAvailable) 
    {
      TvResult resultNoCards = TvResult.ChannelNotMappedToAnyCard;
      Dictionary<int, TvResult>.ValueCollection values = cardsUnAvailable.Values;

      if (values.Any(tvResult => tvResult == TvResult.ChannelIsScrambled)) 
      {
        resultNoCards = TvResult.ChannelIsScrambled;
      }
      return resultNoCards;
    }

    /// <summary>
    /// Gets a list of all available cards which can receive the channel specified
    /// List is sorted.
    /// </summary>
    /// <returns>list containg all cards which can receive the channel</returns>
    public List<CardDetail> GetAvailableCardsForChannel(Dictionary<int, ITvCardHandler> cards, Channel dbChannel,
                                                        ref IUser user)
    {
      Dictionary<int, TvResult> cardsUnAvailable;
      return GetAvailableCardsForChannel(cards, dbChannel, ref user, out cardsUnAvailable);
    }

    /// <summary>
    /// Gets a list of all available cards which can receive the channel specified
    /// List is sorted.
    /// </summary>
    /// <returns>list containg all cards which can receive the channel</returns>
    public List<CardDetail> GetAvailableCardsForChannel(Dictionary<int, ITvCardHandler> cards, Channel dbChannel, ref IUser user, out Dictionary<int, TvResult> cardsUnAvailable)
    {      
      Stopwatch stopwatch = Stopwatch.StartNew();
      cardsUnAvailable = new Dictionary<int, TvResult>();
      try
      {
        //Log.Info("GetFreeCardsForChannel st {0}", Environment.StackTrace);
        //construct list of all cards we can use to tune to the new channel
        var cardsAvailable = new List<CardDetail>();        
        if (LogEnabled)
        {
          Log.Info("Controller: find card for channel {0}", dbChannel.DisplayName);
        }
        //get the tuning details for the channel
        List<IChannel> tuningDetails = _businessLayer.GetTuningChannelsByDbChannel(dbChannel);

        bool isValidTuningDetails = IsValidTuningDetails(tuningDetails);
        if (!isValidTuningDetails)
        {
          //no tuning details??
          if (LogEnabled)
          {
            Log.Info("Controller:  No tuning details for channel:{0}", dbChannel.DisplayName);
          }
          return cardsAvailable;
        }

        if (LogEnabled)
        {
          Log.Info("Controller:   got {0} tuning details for {1}", tuningDetails.Count, dbChannel.DisplayName);
        }
        int number = 0;
        Dictionary<int, ITvCardHandler>.ValueCollection cardHandlers = cards.Values;

        foreach (IChannel tuningDetail in tuningDetails)
        {
          cardsUnAvailable.Clear();
          number++;
          if (LogEnabled)
          {
            Log.Info("Controller:   channel #{0} {1} ", number, tuningDetail.ToString());
          }
          foreach (ITvCardHandler cardHandler in cardHandlers)
          {
            int cardId = cardHandler.DataBaseCard.IdCard;

            if (cardsUnAvailable.ContainsKey(cardId))
            {
              if (LogEnabled)
              {
                Log.Info("Controller:    card:{0} has already been queried, skipping.", cardId);
              }
              continue;
            }
            if (!CanCardTuneChannel(cardHandler, dbChannel, tuningDetail))
            {
              AddCardUnAvailable(ref cardsUnAvailable, cardId, TvResult.ChannelNotMappedToAnyCard);
              continue;
            }

            if (!CanCardDecodeChannel(cardHandler, tuningDetail))
            {
              AddCardUnAvailable(ref cardsUnAvailable, cardId, TvResult.ChannelIsScrambled);
              continue;
            }

            //ok card could be used to tune to this channel
            bool isSameTransponder = IsSameTransponder(cardHandler, tuningDetail);
            if (LogEnabled)
            {
              Log.Info("Controller:    card:{0} type:{1} can tune to channel", cardId, cardHandler.Type);
            }
            int nrOfOtherUsers = NumberOfOtherUsersOnCard(cardHandler, user);
            if (LogEnabled)
            {
              Log.Info("Controller:    card:{0} type:{1} users: {2}", cardId, cardHandler.Type, nrOfOtherUsers);
            }
            CardDetail cardInfo = new CardDetail(cardId, cardHandler.DataBaseCard, tuningDetail, isSameTransponder,
                                                 nrOfOtherUsers);
            cardsAvailable.Add(cardInfo);
          }
        }


        //sort cards
        cardsAvailable.Sort();
        if (LogEnabled)
        {
          Log.Info("Controller: found {0} card(s) for channel", cardsAvailable.Count);
        }

        return cardsAvailable;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return null;
      }
      finally
      {
        stopwatch.Stop();
        Log.Info("AdvancedCardAllocation.GetAvailableCardsForChannel took {0} msec", stopwatch.ElapsedMilliseconds);
      }
    }

    private static bool CanCardDecodeChannel(ITvCardHandler cardHandler, IChannel tuningDetail)
    {
      bool canCardDecodeChannel = true;
      int cardId = cardHandler.DataBaseCard.IdCard;
      if (!tuningDetail.FreeToAir && !cardHandler.DataBaseCard.CAM)
      {
        Log.Info("Controller:    card:{0} type:{1} channel is encrypted but card has no CAM", cardId, cardHandler.Type);
        canCardDecodeChannel = false;
      }
      return canCardDecodeChannel;
    }

    private bool CanCardTuneChannel(ITvCardHandler cardHandler, Channel dbChannel, IChannel tuningDetail)
    {
      int cardId = cardHandler.DataBaseCard.IdCard;
      bool isCardEnabled = IsCardEnabled(cardHandler);
      if (!isCardEnabled)
      {
        //not enabled, so skip the card
        if (LogEnabled)
        {
          Log.Info("Controller:    card:{0} type:{1} is disabled", cardId, cardHandler.Type);
        }
        return false;
      }

      bool isCardPresent = IsCardPresent(cardId);
      if (!isCardPresent)
      {
        Log.Error("card: unable to connect to slave controller at:{0}",
                  cardHandler.DataBaseCard.ReferencedServer().HostName);
        return false;
      }

      if (!cardHandler.Tuner.CanTune(tuningDetail))
      {
        //card cannot tune to this channel, so skip it
        if (LogEnabled)
        {
          Log.Info("Controller:    card:{0} type:{1} cannot tune to channel", cardId, cardHandler.Type);
        }
        return false;
      }

      //check if channel is mapped to this card and that the mapping is not for "Epg Only"
      bool isChannelMappedToCard = IsChannelMappedToCard(dbChannel, cardHandler.DataBaseCard);
      if (!isChannelMappedToCard)
      {
        if (LogEnabled)
        {
          Log.Info("Controller:    card:{0} type:{1} channel not mapped", cardId, cardHandler.Type);
        }
        return false;
      }
      return true;
    }

    private static void AddCardUnAvailable(ref Dictionary<int, TvResult> cardsUnAvailable, int cardId, TvResult tvResult)
    {
      if (!cardsUnAvailable.ContainsKey(cardId))
      {
        cardsUnAvailable.Add(cardId, tvResult);
      }
    }

    #endregion

    #region public members       

    #endregion
  }
}