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

using System.Collections.Generic;
using System.Linq;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvService
{

  public class AdvancedCardAllocationTicket : AdvancedCardAllocation
  {
    private readonly ICollection<ICardTuneReservationTicket> _tickets;

    public AdvancedCardAllocationTicket(TvBusinessLayer businessLayer, IController controller, ICollection<ICardTuneReservationTicket> tickets)
      : base(businessLayer, controller)
    {
      _tickets = tickets;
    }   

    private void LogNumberOfOtherUsersFound(CardDetail cardDetail)
    {
      if (LogEnabled && cardDetail.Card.IdCard > 0)
      {
        var tvController = _controller as TVController;
        if (tvController != null)
        {
          ITvCardHandler card = tvController.CardCollection[cardDetail.Card.IdCard];
          Log.Info("Controller:    card:{0} type:{1} users: {2}", cardDetail.Card.IdCard, card.Type, cardDetail.NumberOfOtherUsers);              
        }
      }
    }

    public ICollection<CardDetail> UpdateFreeCardsForChannelBasedOnTicket(IDictionary<int, ITvCardHandler> cards, ICollection<CardDetail> cardsAvailable, IUser user, out TvResult result)
    {
      var cardetails = new List<CardDetail>();

      foreach (CardDetail cardDetail in cardsAvailable)
      {
        ICardTuneReservationTicket ticket = _tickets.FirstOrDefault(t => t.CardId == cardDetail.Card.IdCard);

        if (ticket != null)
        {          
          cardDetail.SameTransponder = ticket.IsSameTransponder;
          cardDetail.NumberOfOtherUsers = ticket.NumberOfOtherUsersOnCurrentCard;
          LogNumberOfOtherUsersFound(cardDetail);
          bool checkTransponder = CheckTransponder(user, 
                                                   cards[cardDetail.Card.IdCard],                                                    
                                                   cardDetail.TuningDetail);
          if (checkTransponder)
          {
            cardetails.Add(cardDetail);
          }          
        }
      }

      cardetails.Sort();

      if (cardetails.Count > 0)
      {
        result = TvResult.Succeeded;
      }
      else
      {
        result = cardsAvailable.Count == 0 ? TvResult.ChannelNotMappedToAnyCard : TvResult.AllCardsBusy;
      }

      return cardetails;
    }

    #region overrides   

    protected override bool CanCardTuneChannel(ITvCardHandler cardHandler, Channel dbChannel, IChannel tuningDetail)
    {
      return true;
    }

    protected override int GetNumberOfUsersOnCurrentChannel(ITvCardHandler tvcard, IUser user)
    {
      int numberOfUsersOnCurrentChannel = 0;
      ICardTuneReservationTicket ticket = _tickets.FirstOrDefault(t => t.CardId == tvcard.DataBaseCard.IdCard);      

      if (ticket != null)
      {
        //TODO: check code
        numberOfUsersOnCurrentChannel = ticket.NumberOfUsersOnSameCurrentChannel;
      }
      return numberOfUsersOnCurrentChannel;
    }

    protected override bool IsFreeToAir(ITvCardHandler tvcard, ref IUser user)
    {
      bool isFreeToAir = true;
      ICardTuneReservationTicket ticket = _tickets.FirstOrDefault(t => t.CardId == tvcard.DataBaseCard.IdCard);

      if (ticket != null)
      {
        isFreeToAir = (ticket.IsFreeToAir);
      }
      return isFreeToAir;
    }

    protected override int NumberOfChannelsDecrypting(ITvCardHandler tvcard)
    {
      int numberOfChannelsDecrypting = 0;
      ICardTuneReservationTicket ticket = _tickets.FirstOrDefault(t => t.CardId == tvcard.DataBaseCard.IdCard);

      if (ticket != null)
      {
        numberOfChannelsDecrypting = (ticket.NumberOfChannelsDecrypting);
      }
      return numberOfChannelsDecrypting;
    }

    protected override bool IsCamAlreadyDecodingChannel(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      bool isCamAlreadyDecodingChannel = false;
      ICardTuneReservationTicket ticket = _tickets.FirstOrDefault(t => t.CardId == tvcard.DataBaseCard.IdCard);

      if (ticket != null)
      {
        //TODO: check code
        isCamAlreadyDecodingChannel = (ticket.IsCamAlreadyDecodingChannel);
      }
      return isCamAlreadyDecodingChannel;
    }


    protected override bool IsOwnerOfCard(ITvCardHandler tvcard, IUser user)
    {
      bool isOwnerOfCard = false;
      ICardTuneReservationTicket ticket = _tickets.FirstOrDefault(t => t.CardId == tvcard.DataBaseCard.IdCard);

      if (ticket != null)
      {
        bool hasHighestPriority = ticket.HasHighestPriority;        
        if (hasHighestPriority)
        {
          isOwnerOfCard = true;
        }
        else
        {
          bool hasEqualOrHigherPriority = ticket.HasEqualOrHigherPriority;
          if (hasEqualOrHigherPriority)
          {
            isOwnerOfCard = ticket.IsOwner;
          }
        }
      }
      return isOwnerOfCard;
    }

    protected override bool IsSameTransponder(ITvCardHandler tvcard, IChannel tuningDetail)
    {
      bool isSameTransponder = false;
      ICardTuneReservationTicket ticket = _tickets.FirstOrDefault(t => t.CardId == tvcard.DataBaseCard.IdCard);

      if (ticket != null)
      {
        isSameTransponder = ticket.IsSameTransponder;
      }
      return isSameTransponder;
    }

    protected override int NumberOfOtherUsersOnCurrentCard(ITvCardHandler tvcard, IUser user)
    {
      int numberOfOtherUsersOnCurrentCard = 0;
      ICardTuneReservationTicket ticket = _tickets.FirstOrDefault(t => t.CardId == tvcard.DataBaseCard.IdCard);

      if (ticket != null)
      {
        numberOfOtherUsersOnCurrentCard = ticket.NumberOfOtherUsersOnCurrentCard;
      }
      return numberOfOtherUsersOnCurrentCard;
    }

    #endregion

  }
}
