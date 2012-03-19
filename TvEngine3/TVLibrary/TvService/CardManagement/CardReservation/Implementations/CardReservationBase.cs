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
using System.IO;
using System.Linq;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Log;
#endregion

namespace TvService
{
  #region public enums
  public enum CardTuneState
  {
    Idle,
    TunePending,
    Tuning,
    Tuned,
    TuneCancelled,
    TuneFailed    
  }

  public enum CardStopState
  {
    Idle,
    StopPending,
    Stopping,
    Stopped,
    StopFailed
  }

  #endregion

  public abstract class CardReservationBase : ICardReservation
  {    
    #region vars
    protected readonly TVController _tvController;
    #endregion

    #region ctors
    protected CardReservationBase(TVController tvController)
    {
      _tvController = tvController;
    }
    #endregion

    #region events & delegates

    public delegate TvResult StartCardTuneDelegate(ref IUser user, ref string fileName);
    public event StartCardTuneDelegate OnStartCardTune;    

    protected abstract bool OnStartTune(IUser user);

    #endregion    

    #region public members
    
    /// <summary>
    /// deletes time shifting files left in the specified folder.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="fileName">Name of the file.</param>
    private static void CleanTimeShiftFiles(string folder, string fileName)
    {
      try
      {
        Log.Write(@"Controller: delete timeshift files {0}\{1}", folder, fileName);
        string[] files = Directory.GetFiles(folder);
        foreach (string t in files.Where(t => t.IndexOf(fileName) >= 0)) 
        {
          try
          {
            Log.Write("Controller:   delete {0}", t);
            File.Delete(t);
          }
          catch (Exception e)
          {
            Log.Debug("Controller: Error \"{0}\" on delete in CleanTimeshiftFiles", e.Message);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    public TvResult Tune(ITvCardHandler tvcard, ref IUser user, IChannel channel, int idChannel, ICardTuneReservationTicket ticket)
    {
      TvResult tvResult = TvResult.AllCardsBusy;
      bool ticketFound;
      bool isTuningPending = CardReservationHelper.GetIsTuningPending(tvcard, ticket, out ticketFound);

      try
      {
        if (isTuningPending && ticketFound)
        {
          tvResult = tvcard.Tuner.Tune(ref user, channel, idChannel);

          bool succes = (tvResult == TvResult.Succeeded);
          if (succes)
          {
            if (!OnStartTune(user))
            {
              tvResult = TvResult.AllCardsBusy;
            }
          }
          CardReservationHelper.SetCardStateBasedOnTVresult(tvcard, tvResult);
        }
        else // state is not tuning, some other card tune session is busy.
        {
        }
      }
      finally
      {
        CardReservationHelper.RemoveTuneTicket(tvcard, ticket, ticketFound);
        tvcard.Tuner.CleanUpPendingTune(ticket.PendingSubchannel);
      }
      return tvResult;
    }

    public TvResult CardTune(ITvCardHandler tvcard, ref IUser user, IChannel channel, Channel dbChannel, ICardTuneReservationTicket ticket)
    {
      TvResult tvResult = TvResult.AllCardsBusy;
      bool ticketFound;
      bool isTuningPending = CardReservationHelper.GetIsTuningPending(tvcard, ticket, out ticketFound);

      try
      {
        if (isTuningPending && ticketFound)
        {
          Log.Debug("CardReservationBase: tvcard={0}, user={1}, dbChannel={2}, ticket={3}, tunestate={4}, stopstate={5}", tvcard.DataBaseCard.IdCard, user.Name, dbChannel.IdChannel, ticket.Id, tvcard.Tuner.CardTuneState, tvcard.Tuner.CardStopState);
          tvResult = tvcard.Tuner.CardTune(ref user, channel, dbChannel);

          if (tvResult == TvResult.Succeeded)
          {
            if (OnStartCardTune != null)
            {
              if (!_tvController.IsTimeShifting(ref user))
              {
                CleanTimeShiftFiles(tvcard.DataBaseCard.TimeShiftFolder,
                                    String.Format("live{0}-{1}.ts", user.CardId, user.SubChannel));
              }

              string timeshiftFileName = String.Format(@"{0}\live{1}-{2}.ts", tvcard.DataBaseCard.TimeShiftFolder,
                                                       user.CardId,
                                                       user.SubChannel);
              tvResult = OnStartCardTune(ref user, ref timeshiftFileName);
            }
          }

          CardReservationHelper.SetCardStateBasedOnTVresult(tvcard, tvResult);
        }
        else // state is not tuning, some other card tune session is busy.
        {
        }
      }
      finally
      {        
        CardReservationHelper.RemoveTuneTicket(tvcard, ticket, ticketFound);
        tvcard.Tuner.CleanUpPendingTune(ticket.PendingSubchannel);
      }
      return tvResult;
    }

    public ICardTuneReservationTicket RequestCardTuneReservation(ITvCardHandler tvcard, IChannel tuningDetail, IUser user)
    {
      ICardTuneReservationTicket cardTuneReservationTicket = null;
      var layer = new TvBusinessLayer();

      CardTuneState cardTuneState;
      int ticketId = 0;
      bool isCardAvail;
      bool hasUserHigherPriorityThanBlockingUser = false;
      lock (tvcard.Tuner.CardReservationsLock)
      {
        isCardAvail = IsCardAvail(tvcard);
        if (!isCardAvail)
        {
          if (tvcard.Tuner.CardTuneState != CardTuneState.TuneCancelled)
          {
            IUser blockingUser = GetBlockingUser(tvcard);
            hasUserHigherPriorityThanBlockingUser = (HasUserHigherPriorityThanBlockingUser(user, blockingUser));
            if (hasUserHigherPriorityThanBlockingUser)
            {
              tvcard.Tuner.CardTuneState = CardTuneState.TuneCancelled;
            }
          }          
        }
      }
      if (!isCardAvail)
      {
        if (hasUserHigherPriorityThanBlockingUser)
        {
          tvcard.Tuner.CancelTune(tvcard.Tuner.ActiveCardTuneReservationTicket.PendingSubchannel);          
          lock (tvcard.Tuner.CardReservationsLock)
          {
            isCardAvail = IsCardAvail(tvcard);
          }
        }
      }

      lock (tvcard.Tuner.CardReservationsLock)
      {
        if (isCardAvail)
        {          
          tvcard.Tuner.CardTuneState = CardTuneState.TunePending;            
          bool isTunedToTransponder = IsTunedToTransponder(tvcard, tuningDetail);

          if (isTunedToTransponder)
          {
            CheckTransponder(tvcard, tuningDetail, layer, user);
          }

          int ownerSubchannel = -1;
          int numberOfUsersOnSameCurrentChannel = 0;
          int numberOfOtherUsersOnSameChannel = 0;
          int numberOfOtherUsersOnCurrentCard = 0;

          bool hasUserHighestPriority = false;
          bool hasUserEqualOrHigherPriority = false;
          bool isCamAlreadyDecodingChannel = false;
          bool conflictingSubchannelFound = false;
          bool isRecordingAnyUser = false;
          bool isAnySubChannelTimeshifting = tvcard.TimeShifter.IsAnySubChannelTimeshifting;
          bool isOwner = tvcard.Users.IsOwner(user);
          var users = new List<IUser>(tvcard.Users.GetUsers());
          var inactiveUsers = new List<IUser>();
          var activeUsers = new List<IUser>();
          var recUsers = new List<IUser>();
          var tsUsers = new List<IUser>();

          var context = tvcard.Card.Context as ITvCardContext;
          if (context != null)
          {
            context.GetUser(ref user);
            hasUserHighestPriority = context.HasUserHighestPriority(user);
            hasUserEqualOrHigherPriority = context.HasUserEqualOrHigherPriority(user);
          }          

          int currentChannelId = tvcard.CurrentDbChannel(ref user);

          for (int i = users.Count - 1; i > -1; i--)
          {
            IUser actualUser = users[i];                           
            CardReservationHelper.AddUserIfRecording(tvcard, ref actualUser, recUsers);
            CardReservationHelper.AddUserIfTimeshifting(tvcard, ref actualUser, tsUsers);

            bool isCurrentUser = user.Name.Equals(actualUser.Name);

            IChannel userChannel = tvcard.CurrentChannel(ref actualUser);
            var userDVBchannel = userChannel as DVBBaseChannel;

            if (!isCurrentUser)
            {
              if (!isRecordingAnyUser)
              {
                isRecordingAnyUser = CardReservationHelper.IsRecordingUser(tvcard, user, ref actualUser);
              }

              if (actualUser.SubChannel == user.SubChannel && user.IdChannel > 0)
              {
                conflictingSubchannelFound = true;
              }
              numberOfOtherUsersOnCurrentCard = CardReservationHelper.GetNumberOfOtherUsersOnCurrentCard(user,
                                                                                                         numberOfOtherUsersOnCurrentCard);

              
              if (userChannel == null)
              {
                inactiveUsers.Add(actualUser);
              }
              else
              {                
                if (userDVBchannel != null)
                {
                  actualUser.IdChannel = layer.GetTuningDetail(userDVBchannel).IdChannel;
                }

                bool isDiffTS = tuningDetail.IsDifferentTransponder(userChannel);

                if (isDiffTS)
                {
                  activeUsers.Add(actualUser);
                }
                else
                {
                  if (!isOwner)
                  {
                    bool isUserOnSameChannel = CardReservationHelper.IsUserOnSameChannel(tuningDetail, layer,
                                                                                         userDVBchannel);
                    if (isUserOnSameChannel)
                    {
                      numberOfOtherUsersOnSameChannel++;
                      //we do not want to hook up on schedulers existing subchannel
                      if (!actualUser.IsAdmin)
                      {
                        ownerSubchannel = actualUser.SubChannel;
                      }
                    }                    
                  }                  
                }
              }
            }

            bool isUserOnSameCurrentChannel = CardReservationHelper.IsUserOnSameCurrentChannel(currentChannelId, actualUser);
            if (isUserOnSameCurrentChannel)
            {
              numberOfUsersOnSameCurrentChannel++;
            }

            if (!isCamAlreadyDecodingChannel)
            {
              isCamAlreadyDecodingChannel = IsCamAlreadyDecodingChannel(tuningDetail, userChannel);
            }
          }

          bool isFreeToAir = CardReservationHelper.IsFreeToAir(tvcard, user);

          cardTuneReservationTicket = new CardTuneReservationTicket
              (                
              user,
              tuningDetail, 
              isTunedToTransponder, 
              numberOfOtherUsersOnSameChannel, 
              isAnySubChannelTimeshifting, 
              inactiveUsers, 
              activeUsers, 
              users, 
              ownerSubchannel, 
              isOwner, 
              tvcard.DataBaseCard.IdCard, 
              tvcard.NumberOfChannelsDecrypting, 
              isFreeToAir, 
              numberOfOtherUsersOnCurrentCard, 
              recUsers, 
              tsUsers,               
              conflictingSubchannelFound,
              numberOfUsersOnSameCurrentChannel,
              isCamAlreadyDecodingChannel,
              hasUserHighestPriority,
              hasUserEqualOrHigherPriority);
          tvcard.Tuner.ActiveCardTuneReservationTicket = cardTuneReservationTicket;
          tvcard.Tuner.ReservationsForTune.Add(cardTuneReservationTicket);          
        }

        cardTuneState = tvcard.Tuner.CardTuneState;        
        if (tvcard.Tuner.ActiveCardTuneReservationTicket != null)
        {
          ticketId = tvcard.Tuner.ActiveCardTuneReservationTicket.Id;  
        }        
      }


      if (cardTuneReservationTicket != null)
      {
        Log.Debug("CardReservationBase.RequestCardTuneReservation: placed reservation with id={0}, tuningdetails={1}", cardTuneReservationTicket.Id, cardTuneReservationTicket.TuningDetail);
      }
      else
      {
        if (ticketId > 0)
        {
          Log.Debug("CardReservationBase.RequestCardTuneReservation: failed reservation tuningdetails={0}, res id blocking={1}, state={2}", tuningDetail, ticketId, cardTuneState);
        }
        else
        {
          Log.Debug("CardReservationBase.RequestCardTuneReservation: failed reservation tuningdetails={0}, res id blocking={1}, state={2}", tuningDetail, "n/a", cardTuneState);          
        }
      }              
      return cardTuneReservationTicket;
    }    

    private static bool IsCardAvail(ITvCardHandler tvcard)
    {
      bool isCardStopStateIdle = (tvcard.Tuner.CardStopState == CardStopState.Idle);
      bool isCardStopStateStopped = (tvcard.Tuner.CardStopState == CardStopState.Stopped);

      bool isCardTuneStateIdle = (tvcard.Tuner.CardTuneState == CardTuneState.Idle);
      bool isCardTuneStateTuned = (tvcard.Tuner.CardTuneState == CardTuneState.Tuned);

      bool isCardAvail = (isCardStopStateIdle || isCardStopStateStopped) && (isCardTuneStateIdle || isCardTuneStateTuned);
      return isCardAvail;
    }

    private IUser GetBlockingUser (ITvCardHandler tvcard)
    {
      IUser blockingUser = null;
      if (tvcard.Tuner.ActiveCardTuneReservationTicket != null)
      {
        blockingUser = tvcard.Tuner.ActiveCardTuneReservationTicket.User;
      }
      return blockingUser;
    }

    private static bool HasUserHigherPriorityThanBlockingUser(IUser user, IUser blockingUser)
    {
      bool hasUserHigherPriority = false;
            
      if (blockingUser != null)
      {
        hasUserHigherPriority = (user.Priority > blockingUser.Priority);
        Log.Debug("CardReservationBase.HasUserHigherPriorityThanBlockingUser: {0} - user '{1}' with prio={2} vs blocking user '{3}' with prio={4}", hasUserHigherPriority, 
          user.Name, user.Priority, blockingUser.Name, blockingUser.Priority);
      }

      return hasUserHigherPriority;
    }

    #endregion

    #region private members

    private static bool IsCamAlreadyDecodingChannel(IChannel tuningDetail, IChannel currentChannel)
    {
      bool isCamAlreadyDecodingChannel = false;      
      if (currentChannel != null)
      {
        isCamAlreadyDecodingChannel = currentChannel.Equals(tuningDetail);
      }
      
      return isCamAlreadyDecodingChannel;
    }

    private void CheckTransponder(ITvCardHandler tvcard, IChannel tuningDetail, TvBusinessLayer layer, IUser user)
    {
      var cardAlloc = new AdvancedCardAllocation(layer, _tvController);
      cardAlloc.CheckTransponder(user, tvcard, tuningDetail);
    }      

    #endregion

    #region abstract members

    protected abstract bool IsTunedToTransponder(ITvCardHandler tvcard, IChannel tuningDetail);    

    #endregion    
  }
}
