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
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Scheduler;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using IServiceSubChannel = Mediaportal.TV.Server.TVService.Interfaces.Services.ISubChannel;
using ITvLibrarySubChannel = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ISubChannel;

#endregion

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardAllocation
{
  public class AdvancedCardAllocation : CardAllocationBase, ICardAllocation
  {
    #region private members

    public AdvancedCardAllocation()
    {
    }

    private static bool IsCardEnabled(ITvCardHandler cardHandler)
    {
      return cardHandler.Card.IsEnabled;
    }

    private bool IsCardPresent(int cardId)
    {
      bool isCardPresent = false;
      try
      {
        if (ServiceManager.Instance.InternalControllerService.IsCardPresent(cardId))
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

    protected virtual int NumberOfOtherUsersOnCurrentCard(ITvCardHandler card, IUser user)
    {
      //determine how many other users are using this card            
      int nrOfOtherUsers = card.UserManagement.NumberOfOtherUsers(user.Name);

      if (LogEnabled)
      {
        this.LogInfo("Controller:    card:{0} type:{1} users: {2}", card.Card.TunerId, card.Card.SupportedBroadcastStandards, nrOfOtherUsers);
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
    public List<CardDetail> GetFreeCardsForChannel(IDictionary<int, ITvCardHandler> cards, Channel dbChannel, IUser user)
    {
      TvResult result;
      return GetFreeCardsForChannel(cards, dbChannel, user, out result);
    }

    /// <summary>
    /// Gets a list of all free cards which can receive the channel specified
    /// List is sorted.
    /// </summary>
    /// <returns>list containg all free cards which can receive the channel</returns>
    public List<CardDetail> GetFreeCardsForChannel(IDictionary<int, ITvCardHandler> cards, Channel dbChannel, IUser user, out TvResult result)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        //this.LogInfo("GetFreeCardsForChannel st {0}", Environment.StackTrace);
        //construct list of all cards we can use to tune to the new channel
        if (LogEnabled)
        {
          this.LogInfo("Controller: find free card for channel {0}", dbChannel.Name);
        }
        var cardsAvailable = new List<CardDetail>();

        IDictionary<int, TvResult> cardsUnAvailable;
        List<CardDetail> cardDetails = GetAvailableCardsForChannel(cards, dbChannel, user, out cardsUnAvailable);
        foreach (CardDetail cardDetail in cardDetails)
        {
          ITvCardHandler tvCardHandler = cards[cardDetail.Id];
          bool checkTransponder = CheckTransponder(user, tvCardHandler, cardDetail.TuningDetail);
          if (checkTransponder)
          {
            cardsAvailable.Add(cardDetail);
          }
        }

        //sort cards
        cardsAvailable.SortStable();

        if (cardsAvailable.Count > 0)
        {
          result = TvResult.Succeeded;
        }
        else
        {
          TvResult resultNoCards = GetResultNoCards(cardsUnAvailable);
          // TODO gibman I think this is wrong; what about the cases where the channel is not mapped to any cards, or when the channel is encrypted but no cards can decrypt it?
          result = cardDetails.Count == 0 ? resultNoCards : TvResult.AllCardsBusy;
        }
        if (LogEnabled)
        {
          this.LogInfo("Controller: found {0} free card(s)", cardsAvailable.Count);
        }

        return cardsAvailable;
      }
      catch (Exception ex)
      {
        result = TvResult.UnknownError;
        this.LogError(ex);
        return null;
      }
      finally
      {
        stopwatch.Stop();
        this.LogInfo("AdvancedCardAllocation.GetFreeCardsForChannel took {0} msec", stopwatch.ElapsedMilliseconds);
      }
    }

    private static TvResult GetResultNoCards(IDictionary<int, TvResult> cardsUnAvailable)
    {
      TvResult resultNoCards = TvResult.ChannelNotMappedToAnyCard;
      //Dictionary<int, TvResult>.ValueCollection values = cardsUnAvailable.Values;
      ICollection<TvResult> values = cardsUnAvailable.Values;

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
    public List<CardDetail> GetAvailableCardsForChannel(IDictionary<int, ITvCardHandler> cards, Channel dbChannel,
                                                        ref IUser user)
    {
      IDictionary<int, TvResult> cardsUnAvailable;
      return GetAvailableCardsForChannel(cards, dbChannel, user, out cardsUnAvailable);
    }

    /// <summary>
    /// Gets a list of all available cards which can receive the channel specified
    /// List is sorted.
    /// </summary>
    /// <returns>list containg all cards which can receive the channel</returns>
    public List<CardDetail> GetAvailableCardsForChannel(IDictionary<int, ITvCardHandler> cards, Channel dbChannel, IUser user, out IDictionary<int, TvResult> cardsUnAvailable)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      cardsUnAvailable = new Dictionary<int, TvResult>();
      try
      {
        //this.LogInfo("GetFreeCardsForChannel st {0}", Environment.StackTrace);
        //construct list of all cards we can use to tune to the new channel
        var cardsAvailable = new List<CardDetail>();
        if (LogEnabled)
        {
          this.LogInfo("Controller: find card for channel {0}", dbChannel.Name);
        }
        //get the tuning details for the channel
        ICollection<IChannel> tuningDetails = CardAllocationCache.GetTuningDetailsByChannelId(dbChannel.IdChannel);
        bool isValidTuningDetails = IsValidTuningDetails(tuningDetails);
        if (!isValidTuningDetails)
        {
          //no tuning details??
          if (LogEnabled)
          {
            this.LogInfo("Controller:  No tuning details for channel:{0}", dbChannel.Name);
          }
          return cardsAvailable;
        }

        if (LogEnabled)
        {
          this.LogInfo("Controller:   got {0} tuning details for {1}", tuningDetails.Count, dbChannel.Name);
        }
        int number = 0;
        ICollection<ITvCardHandler> cardHandlers = cards.Values;

        int tuningDetailPriority = 1;
        foreach (IChannel tuningDetail in tuningDetails)
        {
          cardsUnAvailable.Clear();
          number++;
          if (LogEnabled)
          {
            this.LogInfo("Controller:   channel #{0} {1} ", number, tuningDetail.ToString());
          }
          foreach (ITvCardHandler cardHandler in cardHandlers)
          {
            int cardId = cardHandler.Card.TunerId;

            if (cardsUnAvailable.ContainsKey(cardId))
            {
              if (LogEnabled)
              {
                this.LogInfo("Controller:    card:{0} has already been queried, skipping.", cardId);
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
              this.LogInfo("Controller:    card:{0} type:{1} can tune to channel", cardId, cardHandler.Card.SupportedBroadcastStandards);
            }
            int nrOfOtherUsers = NumberOfOtherUsersOnCurrentCard(cardHandler, user);
            var cardInfo = new CardDetail(cardId, cardHandler.DataBaseCard.Priority, tuningDetail, tuningDetailPriority, isSameTransponder,
                                                 nrOfOtherUsers);
            cardsAvailable.Add(cardInfo);
          }
          tuningDetailPriority++;
        }


        //sort cards
        cardsAvailable.SortStable();
        if (LogEnabled)
        {
          this.LogInfo("Controller: found {0} card(s) for channel", cardsAvailable.Count);
        }

        return cardsAvailable;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        return null;
      }
      finally
      {
        stopwatch.Stop();
        this.LogInfo("AdvancedCardAllocation.GetAvailableCardsForChannel took {0} msec", stopwatch.ElapsedMilliseconds);
      }
    }

    private static bool CanCardDecodeChannel(ITvCardHandler cardHandler, IChannel tuningDetail)
    {
      bool canCardDecodeChannel = true;
      int cardId = cardHandler.Card.TunerId;
      if (tuningDetail.IsEncrypted && !cardHandler.Card.IsConditionalAccessSupported)
      {
        Log.Info("Controller:    card:{0} type:{1} channel is encrypted but conditional access disabled or not supported", cardId, cardHandler.Card.SupportedBroadcastStandards);
        canCardDecodeChannel = false;
      }
      return canCardDecodeChannel;
    }

    protected virtual bool CanCardTuneChannel(ITvCardHandler cardHandler, Channel dbChannel, IChannel tuningDetail)
    {
      int cardId = cardHandler.Card.TunerId;
      bool isCardEnabled = IsCardEnabled(cardHandler);
      if (!isCardEnabled)
      {
        //not enabled, so skip the card
        if (LogEnabled)
        {
          this.LogInfo("Controller:    card:{0} type:{1} is disabled", cardId, cardHandler.Card.SupportedBroadcastStandards);
        }
        return false;
      }

      bool isCardPresent = IsCardPresent(cardId);
      if (!isCardPresent)
      {
        return false;
      }

      if (!cardHandler.Tuner.CanTune(tuningDetail))
      {
        //card cannot tune to this channel, so skip it
        if (LogEnabled)
        {
          this.LogInfo("Controller:    card:{0} type:{1} cannot tune to channel", cardId, cardHandler.Card.SupportedBroadcastStandards);
        }
        return false;
      }

      //check if channel is mapped to this card and that the mapping is not for "Epg Only"
      bool isChannelMappedToCard = CardAllocationCache.IsChannelMappedToCard(dbChannel.IdChannel, cardHandler.Card.TunerId);
      if (!isChannelMappedToCard)
      {
        if (LogEnabled)
        {
          this.LogInfo("Controller:    card:{0} type:{1} channel not mapped", cardId, cardHandler.Card.SupportedBroadcastStandards);
        }
        return false;
      }
      return true;
    }

    private static void AddCardUnAvailable(ref IDictionary<int, TvResult> cardsUnAvailable, int cardId, TvResult tvResult)
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