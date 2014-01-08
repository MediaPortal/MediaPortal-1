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

using System;
using System.Collections.Generic;
using System.Threading;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.EPG;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardReservation;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardReservation.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.CardReservation;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.Epg
{
  /// <summary>
  /// Class which will  grab the epg for for a specific card
  public class EpgCard : BaseEpgGrabber, IDisposable
  {


    #region const

    private int _epgTimeOut = (10*60); // 10 mins

    #endregion

    #region enums

    private enum EpgState
    {
      Idle,
      Grabbing,
      Updating
    }

    #endregion

    #region variables

    private readonly System.Timers.Timer _epgTimer = new System.Timers.Timer();
    private EpgState _state;

    private bool _reEntrant;
    private List<EpgChannel> _epg;

    private DateTime _grabStartTime;
    private bool _isRunning;

    private Transponder _currentTransponder;

    private readonly TvServerEventHandler _eventHandler;
    private bool _disposed;
    private readonly Card _card;
    private IUser _user;
    private readonly EpgDBUpdater _dbUpdater;

    #endregion

    #region ctor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="card">The card</param>
    public EpgCard(Card card)
    {      
      _card = card;
      _user = UserFactory.CreateEpgUser();

      _grabStartTime = DateTime.MinValue;
      _state = EpgState.Idle;

      _epgTimer.Interval = 30000;
      _epgTimer.Elapsed += _epgTimer_Elapsed;
      _eventHandler = controller_OnTvServerEvent;
      _dbUpdater = new EpgDBUpdater(ServiceManager.Instance.InternalControllerService, "IdleEpgGrabber", true);
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets the card.
    /// </summary>
    /// <value>The card.</value>
    public Card Card
    {
      get { return _card; }
    }

    /// <summary>
    /// Property which returns true if EPG grabber is currently grabbing the epg
    /// or false is epg grabber is idle
    /// </summary>
    public bool IsRunning
    {
      get { return _isRunning; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is grabbing.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is grabbing; otherwise, <c>false</c>.
    /// </value>
    public bool IsGrabbing
    {
      get { return (_state != EpgState.Idle); }
    }

    #endregion

    #region epg callback

    /// <summary>
    /// Gets called when epg has been cancelled
    /// Should be overriden by the class
    /// </summary>
    public override void OnEpgCancelled()
    {      
      this.LogInfo("epg grabber:epg cancelled");

      if (_state == EpgState.Idle)
      {
        return;
      }
      _state = EpgState.Idle;
      ServiceManager.Instance.InternalControllerService.StopGrabbingEpg(_user);
      _user.CardId = -1;
      _currentTransponder.InUse = false;
      return;
    }

    /// <summary>
    /// Callback fired by the tvcontroller when EPG data has been received
    /// The method checks if epg grabbing was in progress and ifso creates a new workerthread
    /// to update the database with the new epg data
    /// </summary>
    public override int OnEpgReceived()
    {
      try
      {
        //is epg grabbing in progress?
        /*if (_state == EpgState.Idle)
        {
          this.LogInfo("Epg: card:{0} OnEpgReceived while idle", _user.CardId);
          return 0;
        }*/
        //is epg grabber already updating the database?

        int cardId = _user.CardId;
        if (_state == EpgState.Updating)
        {
          this.LogInfo("Epg: card:{0} OnEpgReceived while updating", cardId);
          return 0;
        }

        //is the card still idle?
        if (IsCardIdle(_user) == false)
        {
          this.LogInfo("Epg: card:{0} OnEpgReceived but card is not idle", cardId);
          _state = EpgState.Idle;
          ServiceManager.Instance.InternalControllerService.StopGrabbingEpg(_user);
          _user.CardId = -1;
          _currentTransponder.InUse = false;
          return 0;
        }

        List<EpgChannel> epg = ServiceManager.Instance.InternalControllerService.Epg(cardId) ??
                               new List<EpgChannel>();
        //did we receive epg info?
        if (epg.Count == 0)
        {
          //no epg found for this transponder
          this.LogInfo("Epg: card:{0} no epg found", cardId);
          _currentTransponder.InUse = false;
          _currentTransponder.OnTimeOut();

          _state = EpgState.Idle;
          ServiceManager.Instance.InternalControllerService.StopGrabbingEpg(_user);
          ServiceManager.Instance.InternalControllerService.StopCard(cardId);
          _user.CardId = -1;
          _currentTransponder.InUse = false;
          return 0;
        }

        //create worker thread to update the database
        this.LogInfo("Epg: card:{0} received epg for {1} channels", cardId, epg.Count);
        _state = EpgState.Updating;
        _epg = epg;
        Thread workerThread = new Thread(UpdateDatabaseThread);
        workerThread.IsBackground = true;
        workerThread.Name = "EPG Update thread";
        workerThread.Start();
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      return 0;
    }

    #endregion

    #region public members

    /// <summary>
    /// Grabs the epg.
    /// </summary>
    public void GrabEpg()
    {
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent += _eventHandler;
      _epgTimeOut = SettingsManagement.GetValue("timeoutEPG", 10);
      _currentTransponder = TransponderList.Instance.CurrentTransponder;
      Channel channel = _currentTransponder.CurrentChannel;

      this.LogInfo("EpgCard: grab epg on card: #{0} transponder: #{1} ch:{2} ", _card.IdCard,
              TransponderList.Instance.CurrentIndex, channel.DisplayName);

      _state = EpgState.Idle;
      _isRunning = true;
      _user = UserFactory.CreateEpgUser();      
      if (GrabEpgForChannel(channel, _currentTransponder.Tuning, _card))
      {
        this.LogInfo("EpgCard: card: {0} starting to grab {1}", _user.CardId, _currentTransponder.Tuning.ToString());
        _currentTransponder.InUse = true;
        //succeeded, then wait for epg to be received
        _state = EpgState.Grabbing;
        _grabStartTime = DateTime.Now;
        _epgTimer.Enabled = true;
        return;
      }
      this.LogInfo("EpgCard: unable to grab epg transponder: {0} ch: {1} started on {2}",
              TransponderList.Instance.CurrentIndex, channel.DisplayName, _user.CardId);
      this.LogInfo("{0}", _currentTransponder.Tuning.ToString());
    }

    /// <summary>
    /// Stops this instance.
    /// </summary>
    public void Stop()
    {
      if (_user.CardId >= 0)
      {
        this.LogInfo("EpgCard: card: {0} stop grabbing", _user.CardId);
        ServiceManager.Instance.InternalControllerService.StopGrabbingEpg(_user);
      }
      if (_currentTransponder != null)
      {
        _currentTransponder.InUse = false;
      }
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent -= _eventHandler;
      _epgTimer.Enabled = false;
      _isRunning = false;
    }

    #endregion

    #region private members

    /// <summary>
    /// timer callback.
    /// This method is called by a timer every 30 seconds to wake up the epg grabber
    /// the epg grabber will check if its time to grab the epg for a channel
    /// and ifso it starts the grabbing process
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _epgTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      //security check, dont allow re-entrancy here
      if (_reEntrant)
        return;

      try
      {
        _reEntrant = true;
        //if epg grabber is idle, then grab epg for the next channel
        if (_state == EpgState.Idle)
        {
          return;
        }
        else if (_state == EpgState.Grabbing)
        {
          //if we are grabbing epg, then check if card is still idle
          if (_user.CardId >= 0)
          {
            if (!IsCardIdle(_user))
            {
              //not idle? then cancel epg grabbing
              if (_state != EpgState.Idle)
              {
                this.LogInfo("EpgCard: Canceled epg, card is not idle:{0}", _user.CardId);
              }
              ServiceManager.Instance.InternalControllerService.AbortEPGGrabbing(_user.CardId);
              _state = EpgState.Idle;
              _user.CardId = -1;
              _currentTransponder.InUse = false;
              return;
            }
          }

          //wait until grabbing has finished
          TimeSpan ts = DateTime.Now - _grabStartTime;
          if (ts.TotalMinutes > _epgTimeOut)
          {
            //epg grabber timed out. Update database and go back to idle mode
            this.LogInfo("EpgCard: card: {0} timeout after {1} mins", _user.CardId, ts.TotalMinutes);
            ServiceManager.Instance.InternalControllerService.AbortEPGGrabbing(_user.CardId);
            this.LogInfo("EpgCard: Aborted epg grab");
          }
          else
          {
            this.LogInfo("EpgCard: allow grabbing for {0} seconds on card {1}", ts.TotalSeconds, _user.CardId);
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "EpgCard: Error in EPG timer");
      }
      finally
      {
        _reEntrant = false;
      }
    }

    /// <summary>
    /// This method will try to start the epg grabber for the channel and tuning details specified
    /// Epg grabbing can only be started if there is a card idle which can receive the channel specified
    /// </summary>
    /// <param name="channel">channel to grab/param>
    /// <param name="tuning">tuning information</param>
    /// <param name="card">card to use for grabbing</param>
    /// <returns>true if grabbing has started else false</returns>
    private bool GrabEpgForChannel(Channel channel, IChannel tuning, Card card)
    {
      if (channel == null)
      {
        this.LogError("Epg: invalid channel");
        return false;
      }
      if (tuning == null)
      {
        this.LogError("Epg: invalid tuning");
        return false;
      }
      if (card == null)
      {
        this.LogError("Epg: invalid card");
        return false;
      }
      if (ServiceManager.Instance.InternalControllerService == null)
      {
        this.LogError("Epg: invalid tvcontroller");
        return false;
      }
      if (_user == null)
      {
        this.LogError("Epg: invalid user");
        return false;
      }
      //remove following check to enable multi-card epg grabbing (still beta)
      if (ServiceManager.Instance.InternalControllerService.AllCardsIdle == false)
      {
        this.LogInfo("Epg: card:{0} cards are not idle", card.IdCard);
        return false;
      }

      TvResult result = TvResult.UnknownError;
      //handle ATSC
      ATSCChannel atscChannel = tuning as ATSCChannel;
      if (atscChannel != null)
      {
        if (ServiceManager.Instance.InternalControllerService.Type(card.IdCard) == CardType.Atsc)
        {
          if (IsCardIdle(Card.IdCard) == false)
          {
            this.LogInfo("Epg: card:{0} atsc card is not idle", card.IdCard);
            return false; //card is busy
          }
          return TuneEPGgrabber(channel, tuning, card, result);
        }
        this.LogInfo("Epg: card:{0} could not tune to atsc channel:{1}", card.IdCard, tuning.ToString());
        return false;
      }

      //handle DVBC
      DVBCChannel dvbcChannel = tuning as DVBCChannel;
      if (dvbcChannel != null)
      {
        if (ServiceManager.Instance.InternalControllerService.Type(card.IdCard) == CardType.DvbC)
        {
          if (IsCardIdle(Card.IdCard) == false)
          {
            this.LogInfo("Epg: card:{0} dvbc card is not idle", card.IdCard);
            return false; //card is busy
          }
          return TuneEPGgrabber(channel, tuning, card, result);
        }
        this.LogInfo("Epg: card:{0} could not tune to dvbc channel:{1}", card.IdCard, tuning.ToString());
        return false;
      }

      //handle DVBS
      DVBSChannel dvbsChannel = tuning as DVBSChannel;
      if (dvbsChannel != null)
      {
        if (ServiceManager.Instance.InternalControllerService.Type(card.IdCard) == CardType.DvbS)
        {
          if (IsCardIdle(Card.IdCard) == false)
          {
            this.LogInfo("Epg: card:{0} dvbs card is not idle", card.IdCard);
            return false; //card is busy
          }
          return TuneEPGgrabber(channel, tuning, card, result);
        }
        this.LogInfo("Epg: card:{0} could not tune to dvbs channel:{1}", card.IdCard, tuning.ToString());
        return false;
      }

      //handle DVBT
      DVBTChannel dvbtChannel = tuning as DVBTChannel;
      if (dvbtChannel != null)
      {
        if (ServiceManager.Instance.InternalControllerService.Type(card.IdCard) == CardType.DvbT)
        {
          if (IsCardIdle(Card.IdCard) == false)
          {
            this.LogInfo("Epg: card:{0} dvbt card is not idle", card.IdCard);
            return false; //card is busy
          }

          return TuneEPGgrabber(channel, tuning, card, result);
        }
        this.LogInfo("Epg: card:{0} could not tune to dvbt channel:{1}", card.IdCard, tuning.ToString());
        return false;
      }

      //handle DVBIP
      DVBIPChannel dvbipChannel = tuning as DVBIPChannel;
      if (dvbipChannel != null)
      {
        if (ServiceManager.Instance.InternalControllerService.Type(card.IdCard) == CardType.DvbIP)
        {
          if (IsCardIdle(Card.IdCard) == false)
          {
            this.LogInfo("Epg: card:{0} dvbip card is not idle", card.IdCard);
            return false; //card is busy
          }
          return TuneEPGgrabber(channel, tuning, card, result);
        }
        else
        {
          this.LogInfo("Epg: card:{0} could not tune to dvbip channel:{1}", card.IdCard, tuning.ToString());
        }
        return false;
      }
      this.LogInfo("Epg: card:{0} could not tune to channel:{1}", card.IdCard, tuning.ToString());
      return false;
    }

    private bool TuneEPGgrabber(Channel channel, IChannel tuning, Card card, TvResult result)
    {
      try
      {
        _user.CardId = Card.IdCard;
        ITvCardHandler cardHandler;
        if (ServiceManager.Instance.InternalControllerService.CardCollection.TryGetValue(card.IdCard, out cardHandler))
        {
          ICardTuneReservationTicket ticket = null;
          try
          {
            ICardReservation cardReservationImpl = new CardReservationTimeshifting();
            ticket = cardReservationImpl.RequestCardTuneReservation(cardHandler, tuning, _user, cardHandler.UserManagement.GetRecentChannelId(_user.Name));

            if (ticket != null)
            {
              result = ServiceManager.Instance.InternalControllerService.Tune(ref _user, tuning, channel.IdChannel,
                                                                              ticket);
              if (result == TvResult.Succeeded)
              {
                if (!_isRunning || false == ServiceManager.Instance.InternalControllerService.GrabEpg(this, _user))
                {
                  if (!_isRunning)
                    this.LogInfo("Tuning finished but EpgGrabber no longer enabled");
                  ServiceManager.Instance.InternalControllerService.StopGrabbingEpg(_user);
                  _user.CardId = -1;
                  this.LogInfo("Epg: card:{0} could not start dvbt grabbing", card.IdCard);
                  return false;
                }
                _user.CardId = Card.IdCard;
                return true;
              }
            }
          }
          catch (Exception)
          {
            CardReservationHelper.CancelCardReservation((ITvCardHandler) cardHandler, ticket);
            throw;
          }
        }
        _user.CardId = -1;
        this.LogInfo("Epg: card:{0} could not tune to channel:{1}", card.IdCard, result.ToString());
        return false;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        throw;
      }
    }

    #region database update routines

    /// <summary>
    /// workerthread which will update the database with the new epg received
    /// </summary>
    private void UpdateDatabaseThread()
    {
      Thread.CurrentThread.Priority = ThreadPriority.Lowest;

      //if card is not idle anymore we return
      if (IsCardIdle(_user) == false)
      {
        _currentTransponder.InUse = false;
        return;
      }
      int cardId = _user.CardId;
      this.LogInfo("Epg: card:{0} Updating database with new programs", cardId);
      bool timeOut = false;
      _dbUpdater.ReloadConfig();
      try
      {
        foreach (EpgChannel epgChannel in _epg)
        {
          _dbUpdater.UpdateEpgForChannel(epgChannel);
          if (_state != EpgState.Updating)
          {
            this.LogInfo("Epg: card:{0} stopped updating state changed", cardId);
            timeOut = true;
            return;
          }
          if (IsCardIdle(_user) == false)
          {
            this.LogInfo("Epg: card:{0} stopped updating card not idle", cardId);
            timeOut = true;
            return;
          }
        }
        _epg.Clear();
        ProgramManagement.SynchProgramStatesForAllSchedules();
        this.LogInfo("Epg: card:{0} Finished updating the database.", cardId);
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      finally
      {
        if (timeOut == false)
        {
          try
          {
            _currentTransponder.OnTimeOut();
          }
          catch (Exception ex)
          {
            this.LogError(ex);
          }          
        }
        if (_state != EpgState.Idle && cardId >= 0)
        {
          ServiceManager.Instance.InternalControllerService.StopGrabbingEpg(_user);
          ServiceManager.Instance.InternalControllerService.StopCard(cardId);
        }
        _currentTransponder.InUse = false;
        _state = EpgState.Idle;
        _user.CardId = -1;
        ServiceManager.Instance.InternalControllerService.Fire(this,
                                                               new TvServerEventArgs(TvServerEventType.ProgramUpdated));
      }
    }

    #endregion

    #region helper methods

    /// <summary>
    /// Method which returns if the card is idle or not
    /// </summary>
    /// <param name="cardId">id of card</param>
    /// <returns>true if card is idle, otherwise false</returns>
    private bool IsCardIdle(int cardId)
    {
      if (_isRunning == false)
        return false;
      if (cardId < 0)
        return false;
      IUser user = new User();
      user.CardId = cardId;
      if (ServiceManager.Instance.InternalControllerService.IsRecording(ref user))
        return false;
      if (ServiceManager.Instance.InternalControllerService.IsTimeShifting(user.Name))
        return false;
      if (ServiceManager.Instance.InternalControllerService.IsScanning(user.CardId))
        return false;
      IUser cardUser;
      if (ServiceManager.Instance.InternalControllerService.IsCardInUse(cardId, out cardUser))
        return false;
      if (ServiceManager.Instance.InternalControllerService.IsCardInUse(user.CardId, out cardUser))
      {
        if (cardUser != null)
        {
          return cardUser.Name == _user.Name;
        }
        return false;
      }
      return true;
    }

    /// <summary>
    /// Method which returns if the card is idle or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true if card is idle, otherwise false</returns>
    private bool IsCardIdle(IUser user)
    {
      if (_isRunning == false)
        return false;
      if (user.CardId < 0)
        return false;
      if (ServiceManager.Instance.InternalControllerService.IsRecording(ref _user))
        return false;
      if (ServiceManager.Instance.InternalControllerService.IsTimeShifting(_user.Name))
        return false;
      if (ServiceManager.Instance.InternalControllerService.IsScanning(user.CardId))
        return false;
      IUser cardUser;
      if (ServiceManager.Instance.InternalControllerService.IsCardInUse(user.CardId, out cardUser))
      {
        if (cardUser != null)
        {
          return cardUser.Name == _user.Name;
        }
        return false;
      }
      return true;
    }

    #endregion

    #endregion

    #region IDisposable Members    

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // get rid of managed resources
        if (!_disposed)
        {
          _epgTimer.Dispose();
          _disposed = true;
        }
      }

      // get rid of unmanaged resources
    }


    /// <summary>
    /// Disposes the EPG card
    /// </summary>    
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~EpgCard()
    {
      Dispose(false);
    }

    private void controller_OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      if (_state == EpgState.Idle)
        return;
      TvServerEventArgs tvArgs = eventArgs as TvServerEventArgs;
      if (eventArgs == null)
        return;
      if (tvArgs != null)
        switch (tvArgs.EventType)
        {
          case TvServerEventType.StartTimeShifting:
            this.LogInfo("epg cancelled due to start timeshifting");
            OnEpgCancelled();
            break;

          case TvServerEventType.StartRecording:
            this.LogInfo("epg cancelled due to start recording");
            OnEpgCancelled();
            break;
        }
    }

    #endregion
  }
}