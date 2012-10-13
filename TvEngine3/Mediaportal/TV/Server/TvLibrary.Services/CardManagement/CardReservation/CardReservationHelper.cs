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
using System.Threading;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvService
{
  public static class CardReservationHelper
  {
    private static int _idCounter;

    public static int GetNextId
    {
      get
      {
        _idCounter++;
        return _idCounter;
      }
    }

    private static void ResetCardTuneStateToIdle(ITvCardHandler tvcard)
    {
      bool hasRes4Tune = (tvcard.Tuner.ReservationsForTune.Count > 0);
      if (!hasRes4Tune /*&& tvcard.Card.SubChannels.Length == 0*/)
      {
        tvcard.Tuner.CardTuneState = CardTuneState.Idle;
        tvcard.Tuner.ActiveCardTuneReservationTicket = null;        
      }
    }

    private static void ResetCardStopStateToIdle(ITvCardHandler tvcard)
    {
      bool hasRes4Stop = (tvcard.Tuner.ReservationsForStop.Count > 0);
      
      if (!hasRes4Stop /*&& tvcard.Card.SubChannels.Length == 0*/)
      {
        tvcard.Tuner.CardStopState = CardStopState.Idle;        
      }
    }

    private static void SetFailedCardState(ITvCardHandler tvcard)
    {
      lock (tvcard.Tuner.CardReservationsLock)
      {
        tvcard.Tuner.CardTuneState = CardTuneState.TuneFailed;                
      }
    }

    private static void SetTunedCardState(ITvCardHandler tvcard)
    {
      lock (tvcard.Tuner.CardReservationsLock)
      {
        tvcard.Tuner.CardTuneState = CardTuneState.Tuned;        
      }
    }

    private static void SetCurrentCardState(ITvCardHandler tvcard)
    {
      lock (tvcard.Tuner.CardReservationsLock)
      {
        if (tvcard.Card.SubChannels.Length > 0)
        {
          tvcard.Tuner.CardTuneState = CardTuneState.Tuned;          
        }
        else if (tvcard.Tuner.ReservationsForTune.Count > 1)
        {
          tvcard.Tuner.CardTuneState = CardTuneState.TunePending;                   
        }
        else
        {          
          tvcard.Tuner.CardTuneState = CardTuneState.TuneFailed;         
        }
      }
    }



    public static void SetCardStateBasedOnTVresult(ITvCardHandler tvcard, TvResult tvResult)
    {
      if (tvResult == TvResult.Succeeded)
      {
        SetTunedCardState(tvcard);
      }
      else if (tvResult == TvResult.NoVideoAudioDetected ||
               tvResult == TvResult.NoPmtFound ||
               tvResult == TvResult.ChannelIsScrambled)
      {
        SetCurrentCardState(tvcard);
      }
      else
      {
        SetFailedCardState(tvcard);
      }
    }

    

    public static int GetNumberOfOtherUsersOnCurrentCard(IUser user, int numberOfOtherUsersOnCurrentCard)
    {
      //if (!user.Name.Equals("epg"))
      //{
        numberOfOtherUsersOnCurrentCard++;
      //}
      return numberOfOtherUsersOnCurrentCard;
    }

    public static bool IsRecordingUser(ITvCardHandler tvcard, IUser user, ref IUser u)
    {
      bool isRecordingAnyUser = false;
      if (user.IsAdmin)
      {
        if (tvcard.CurrentChannelName(ref u) != null)
        {
          isRecordingAnyUser = tvcard.Recorder.IsRecording(ref u);
        }
      }
      return isRecordingAnyUser;
    }

    public static void AddUserIfTimeshifting(ITvCardHandler tvcard, ref IUser u, List<IUser> tsUsers)
    {
      bool isUserTS = tvcard.TimeShifter.IsTimeShifting(ref u);

      if (isUserTS)
      {
        tsUsers.Add(u);
      }
    }
            

    public static void RemoveTuneTicket(ITvCardHandler tvcard, ICardTuneReservationTicket ticket, bool ticketFound)
    {
      if (ticketFound)
      {
        lock (tvcard.Tuner.CardReservationsLock)
        {
          Log.Debug("CardReservation.RemoveTuneTicket: removed reservation with id={0}, tuningdetails={1}", ticket.Id, ticket.TuningDetail);
          tvcard.Tuner.ReservationsForTune.Remove(ticket);
          ResetCardTuneStateToIdle(tvcard);          
        }
      }
    }

    public static void CancelCardReservation(ITvCardHandler tvCardHandler, ICardTuneReservationTicket ticket)
    {
      lock (tvCardHandler.Tuner.CardReservationsLock)
      {
        if (ticket != null && tvCardHandler.Tuner.ReservationsForTune.Contains(ticket))
        {
          Log.Debug("CardReservation.CancelCardReservation id={0}", ticket.Id);
          if (tvCardHandler.Tuner.ActiveCardTuneReservationTicket != null && tvCardHandler.Tuner.ActiveCardTuneReservationTicket.Id == ticket.Id)
          {
            tvCardHandler.Tuner.ActiveCardTuneReservationTicket = null;
          }

          tvCardHandler.Tuner.ReservationsForTune.Remove(ticket);

          ResetCardTuneStateToIdle(tvCardHandler);
        }
        else
        {
          //Log.Debug("CardReservation.CancelCardReservation FAILED id={0}", ticket.Id);
        }

      }
    }

    public static ICardStopReservationTicket RequestAndWaitForCardStopReservation(ITvCardHandler tvcard, IUser user)
    {
      ICardStopReservationTicket ticket = null;
      
      while (ticket == null)
      {
        ticket = RequestCardStopReservation(tvcard, user);
        Thread.Sleep(100);
      }

      return ticket;
    }

    private static void RemoveStopTicket(ITvCardHandler tvcard, ICardStopReservationTicket ticket, bool ticketFound)
    {
      if (ticketFound)
      {
        lock (tvcard.Tuner.CardReservationsLock)
        {
          Log.Debug("CardReservation.RemoveStopTicket: removed STOP reservation with id={0}", ticket.Id);
          tvcard.Tuner.ReservationsForStop.Remove(ticket);
          ResetCardStopStateToIdle(tvcard);          
        }
      }
    }

    public static void AddUserIfRecording(ITvCardHandler tvcard, ref IUser u, List<IUser> recUsers)
    {
      bool isUserRec = tvcard.Recorder.IsRecording(ref u);

      if (isUserRec)
      {
        recUsers.Add(u);
      }
    }

    public static bool IsUserOnSameCurrentChannel(int currentChannelId, IUser user)
    {
      /*if (currentChannelId > 0)
      {                  
          TuningDetail userDVBtuningDetail = layer.GetTuningDetail(userDVBchannel);
          isUserOnSameCurrentChannel = (userDVBtuningDetail.IdChannel == currentChannelId);        
      }
      */
      bool isUserOnSameCurrentChannel = (user.IdChannel == currentChannelId);
      return isUserOnSameCurrentChannel;
    }

    public static bool IsUserOnSameChannel(IChannel tuningDetail, TvBusinessLayer layer, DVBBaseChannel userDVBchannel)
    {
      bool isUserOnSameChannel = false;      
      if (userDVBchannel != null)
      {        
        var currentDVBchannel = tuningDetail as DVBBaseChannel;
        if (currentDVBchannel != null)
        {
          TuningDetail currentDVBtuningDetail = layer.GetTuningDetail(currentDVBchannel);
          TuningDetail userDVBtuningDetail = layer.GetTuningDetail(userDVBchannel);
          isUserOnSameChannel = (currentDVBtuningDetail != null && currentDVBtuningDetail.IdChannel == userDVBtuningDetail.IdChannel);

        }        
      }
      return isUserOnSameChannel;
    }

    public static bool IsFreeToAir(ITvCardHandler tvcard, IUser user)
    {
      IChannel currentUserCh = tvcard.CurrentChannel(ref user);
      return (currentUserCh != null && currentUserCh.FreeToAir);
    }

    public static bool GetIsTuningPending(ITvCardHandler tvcard, ICardTuneReservationTicket ticket, out bool ticketFound)
    {      
      bool isTuningPending;      
      bool cardStopStateIdle;

      lock (tvcard.Tuner.CardReservationsLock)
      {
        cardStopStateIdle = (tvcard.Tuner.CardStopState == CardStopState.Idle);
        isTuningPending = (tvcard.Tuner.CardTuneState == CardTuneState.TunePending);

        ticketFound = (tvcard.Tuner.ReservationsForTune.Contains(ticket));

        if (isTuningPending && cardStopStateIdle)
        {
          if (ticketFound)
          {              
            tvcard.Tuner.CardTuneState = CardTuneState.Tuning;     
          }
          else
          {
            //_cardTuneState = CardTuneState.TunePending;            
          }
        }
        
      }

      if (ticket == null)
      {
        Log.Debug("GetIsTuningPending: ticket is null!");
      }

      return (isTuningPending && cardStopStateIdle);
    }

    public static void CancelCardReservationAndRemoveTicket(ITvCardHandler tvCardHandler, ICollection<ICardTuneReservationTicket> tickets)
    {
      if (tvCardHandler != null && tickets != null)
      {
        int cardId = tvCardHandler.DataBaseCard.IdCard;
        ICardTuneReservationTicket ticket = tickets.FirstOrDefault(t => t.CardId == cardId);
        if (ticket != null)
        {
          tickets.Remove(ticket);
          CancelCardReservation(tvCardHandler, ticket);
        }
      }
    }

    public static void CancelAllCardReservations(IEnumerable<ICardTuneReservationTicket> tickets, IDictionary<int, ITvCardHandler> cards)
    {
      //always release tickets, important for those cards not tuned but still a part of the freecards list.
      if (tickets != null)
      {
        foreach (ICardTuneReservationTicket ticket in tickets)
        {
          int idcard = ticket.CardId;
          CancelCardReservation(cards[idcard], ticket);
        }          
      }
    }

    public static void CancelCardReservationsBasedOnMaxCardsLimit(ICollection<ICardTuneReservationTicket> tickets, ICollection<CardDetail> freeCards, int maxCards, IDictionary<int, ITvCardHandler> cards)
    {
      int exceedingCardsCount = freeCards.Count() - maxCards;
      if (exceedingCardsCount > 0)
      {
        if (tickets != null && tickets.Count > 0)
        {
          while (freeCards.Count() > exceedingCardsCount)
          {
            CardDetail cardDetailForReservation = freeCards.LastOrDefault();
            if (cardDetailForReservation != null)
            {
              int idcard = cardDetailForReservation.Card.IdCard;
              CancelCardReservationAndRemoveTicket(cards[idcard], tickets);
              freeCards.Remove(cardDetailForReservation);
            }
          }
        }
      }
    }

    private static void CancelMissingCardReservation(IEnumerable<CardDetail> freeCards, CardDetail cardDetailForReservation, ICollection<ICardTuneReservationTicket> tickets, IDictionary<int, ITvCardHandler> cards)
    {
      int idcard = cardDetailForReservation.Card.IdCard;
      if (freeCards != null && !freeCards.Any(t => t.Card.IdCard == idcard))
      {
        CancelCardReservationAndRemoveTicket(cards[idcard], tickets);
      }
    }

    public static void CancelCardReservationsNotFoundInFreeCards(IEnumerable<CardDetail> freeCardsForReservation, ICollection<ICardTuneReservationTicket> tickets, ICollection<CardDetail> freeCards, IDictionary<int, ITvCardHandler> cards)
    {
      //cancel tickets that are no longer needed, simply because the cardalloc. has discarded the card(s)
      if (tickets != null && tickets.Count > 0)
      {
        foreach (CardDetail cardDetailForReservation in freeCardsForReservation)
        {
          CancelMissingCardReservation(freeCards, cardDetailForReservation, tickets, cards);
        }
      }
    }

    public static void CancelCardReservationsExceedingMaxConcurrentTickets(ICollection<ICardTuneReservationTicket> tickets, ICollection<CardDetail> freeCards, IDictionary<int, ITvCardHandler> cards)
    {
      if (freeCards != null && freeCards.Count > 2)
      {                        
        Log.Debug("CancelCardReservationsExceedingMaxConcurrentTickets: removing exceeding nr of tickets, only 2 allowed at a time but found {0}", tickets.Count);       
        while (freeCards.Count > 2)
        {
          CardDetail cardDetailForReservation = freeCards.LastOrDefault(); 
          if (cardDetailForReservation != null)
          {
            int idcard = cardDetailForReservation.Card.IdCard;
            CancelCardReservationAndRemoveTicket(cards[idcard], tickets);
            freeCards.Remove(cardDetailForReservation);
          }
        }
      }      
    }

    public static ICollection<ICardTuneReservationTicket> RequestCardReservations(IUser user, IEnumerable<CardDetail> availCardsForReservation, TVController tvController, ICardReservation cardResImpl, IEnumerable<int> ignoreCards)
    {
      ICollection<ICardTuneReservationTicket> tickets = new List<ICardTuneReservationTicket>();
      IDictionary<int, ITvCardHandler> cards = tvController.CardCollection;

      foreach (CardDetail cardDetail in availCardsForReservation)
      {
        int idCard = cardDetail.Card.IdCard;
        bool foundIgnoredCard = ignoreCards.Contains(idCard);
        if (!foundIgnoredCard)
        {
          IUser userCopy = user.Clone() as User;
          if (userCopy != null)
          {
            userCopy.CardId = idCard;
            ITvCardHandler tvcard = cards[idCard];
            ICardTuneReservationTicket ticket = cardResImpl.RequestCardTuneReservation(tvcard, cardDetail.TuningDetail,
                                                                                   userCopy);
            if (ticket != null)
            {
              tickets.Add(ticket);
            }
          }          
        }
      }
      return tickets;
    }

    private static ICardStopReservationTicket RequestCardStopReservation(ITvCardHandler tvcard, IUser user)
    {
      ICardStopReservationTicket cardStopReservation = null;

      CardTuneState cardTuneState;
      bool hasMoreStopReservations;

      lock (tvcard.Tuner.CardReservationsLock)
      {
        bool isCardIdle = (tvcard.Tuner.CardTuneState == CardTuneState.Idle);
        bool isCardTuned = (tvcard.Tuner.CardTuneState == CardTuneState.Tuned);
        bool isCardTunePending = (tvcard.Tuner.CardTuneState == CardTuneState.TunePending);
        bool isCardTuneCancelled = (tvcard.Tuner.CardTuneState == CardTuneState.TuneCancelled);

        bool isCardStopIdle = (tvcard.Tuner.CardStopState == CardStopState.Idle);
        bool isCardStopped = (tvcard.Tuner.CardStopState == CardStopState.Stopped);

        bool isCardAvail = (isCardIdle || isCardTuned || isCardTunePending || isCardTuneCancelled) && (isCardStopped || isCardStopIdle);

        if (isCardAvail)
        {          
          tvcard.Tuner.CardStopState = CardStopState.StopPending;
          cardStopReservation = new CardStopReservationTicket();          
          tvcard.Tuner.ReservationsForStop.Add(cardStopReservation);
        }

        cardTuneState = tvcard.Tuner.CardTuneState;
        hasMoreStopReservations = (tvcard.Tuner.ReservationsForStop.Count > 0);

      }

      if (cardStopReservation != null)
      {
        Log.Debug("CardTuner.RequestCardStopReservation: placed reservation with id={0}, user={1}", cardStopReservation.Id, user.Name);
      }
      else
      {
        if (hasMoreStopReservations)
        {
          Log.Debug(
            "CardTuner.RequestCardStopReservation: failed reservation user={0}, cardstate={1}, res id blocking={2}",
            user.Name, cardTuneState, "n/a");
        }
        else
        {
          Log.Debug(
            "CardTuner.RequestCardStopReservation: failed reservation user={0}, cardstate={1}",
            user.Name, cardTuneState);
        }
      }

      return cardStopReservation;
    }

    public static bool Stop(ITvCardHandler tvcard, ref IUser user, ICardStopReservationTicket ticket)
    {
      bool isStopPending;
      bool ticketFound;
      bool result = false;

      lock (tvcard.Tuner.CardReservationsLock)
      {        
        isStopPending = (tvcard.Tuner.CardStopState == CardStopState.StopPending);
        ticketFound = (tvcard.Tuner.ReservationsForStop.Contains(ticket));
        if (isStopPending)
        {
          Log.Debug("CardTuner.Stop: ticket id={0}, found={1}", ticket.Id, ticketFound);
          if (ticketFound)
          {
            tvcard.Tuner.CardStopState = CardStopState.Stopping;            
          }
          else
          {
            Log.Debug("ticket not found!");
          }
        }
      }

      try
      {
        if (isStopPending && ticketFound)
        {
          Log.Info("Stop cardid={0}, ticket={1}, tunestate={2}, stopstate={3}", tvcard.DataBaseCard.IdCard, ticket.Id, tvcard.Tuner.CardTuneState, tvcard.Tuner.CardStopState);

          result = tvcard.TimeShifter.Stop(ref user);

          lock (tvcard.Tuner.CardReservationsLock)
          {
            if (result)
            {              
              tvcard.Tuner.CardStopState = CardStopState.Stopped;              
              ResetCardTuneStateToIdle(tvcard);
            }
            else
            {              
              if (tvcard.Tuner.ReservationsForStop.Count > 1)
              {
                tvcard.Tuner.CardStopState = CardStopState.StopPending;                
              }
              else
              {
                tvcard.Tuner.CardStopState = CardStopState.StopFailed;                
              }
              
            }            
          }
        }
        else // state is not tuning, some other card tune session is busy.
        {
        }
      }
      finally
      {
        RemoveStopTicket(tvcard, ticket, ticketFound);
      }

      return result;
    }

    
  }
}
