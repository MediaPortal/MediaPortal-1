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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.CardReservation;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.Epg
{
  /// <summary>
  /// Class which will  grab the epg for for a specific card
  /// </summary>
  public class EpgCard : IEpgGrabberCallBack, IDisposable
  {
    #region const

    private int _epgTimeOut = 10;   // minutes

    #endregion

    #region enums

    private enum EpgState
    {
      Idle,
      Tuning,
      Grabbing,
      Updating
    }

    #endregion

    #region variables

    private readonly System.Timers.Timer _epgTimer = new System.Timers.Timer();
    private EpgState _state;

    private bool _reEntrant;

    private DateTime _grabStartTime;

    private Transponder _currentTransponder;

    private readonly TvServerEventHandler _eventHandler;
    private bool _disposed;
    private readonly ITvCardHandler _tuner;
    private IUser _user;
    private readonly EpgDBUpdater _dbUpdater;

    #endregion

    public EpgCard(ITvCardHandler tuner)
    {      
      _tuner = tuner;
      _user = UserFactory.CreateEpgUser();

      _grabStartTime = DateTime.MinValue;
      _state = EpgState.Idle;

      _epgTimer.Interval = 30000;
      _epgTimer.Elapsed += _epgTimer_Elapsed;
      _eventHandler = controller_OnTvServerEvent;
      _dbUpdater = new EpgDBUpdater(ServiceManager.Instance.InternalControllerService.OnImportEpgPrograms, "IdleEpgGrabber");
    }

    #region properties

    public int IdTuner
    {
      get { return _tuner.Card.TunerId; }
    }

    public bool IsGrabbingEnabled
    {
      get { return _tuner.DataBaseCard.UseForEpgGrabbing; }
    }

    /// <summary>
    /// Get a value indicating whether this instance is grabbing.
    /// </summary>
    public bool IsGrabbing
    {
      get { return (_state != EpgState.Idle); }
    }

    #endregion

    #region epg callback

    /// <summary>
    /// Called when electronic programme guide data grabbing is cancelled.
    /// </summary>
    public void OnEpgCancelled()
    {      
      this.LogInfo("EPG grabber: grabbing cancelled");
      Stop();
    }

    /// <summary>
    /// Called when electronic programme guide data grabbing is complete.
    /// </summary>
    /// <param name="tuningDetail">The tuning details of the transmitter from which the EPG was grabbed.</param>
    /// <param name="epg">The grabbed data.</param>
    public void OnEpgReceived(IChannel tuningDetail, IDictionary<IChannel, IList<EpgProgram>> epg)
    {
      try
      {
        int cardId = _user.CardId;
        if (_state == EpgState.Updating)
        {
          this.LogInfo("Epg: card:{0} OnEpgReceived while updating", cardId);
          return;
        }

        // It is not safe to stop the tuner graph from here because we're in
        // a callback that has been triggered during sample processing. Start
        // a thread to stop the graph safely...
        //is the card still idle?
        if (IsCardIdle(_user) == false)
        {
          this.LogInfo("Epg: card:{0} OnEpgReceived but card is not idle", cardId);
          Thread stopThread = new Thread(Stop);
          stopThread.IsBackground = true;
          stopThread.Name = "EPG grabber stop thread";
          stopThread.Start();
          return;
        }

        //did we receive epg info?
        if (epg == null)
        {
          //no epg found for this transponder
          this.LogInfo("Epg: card:{0} no epg found", cardId);
          _currentTransponder.OnTimeOut();
          Thread stopThread = new Thread(Stop);
          stopThread.IsBackground = true;
          stopThread.Name = "EPG grabber stop thread";
          stopThread.Start();
          return;
        }

        //create worker thread to update the database
        this.LogInfo("Epg: card:{0} received epg for {1} channels", cardId, epg.Count);
        _state = EpgState.Updating;
        ThreadStart starter = delegate { UpdateDatabaseThread(tuningDetail, epg); };
        Thread workerThread = new Thread(starter);
        workerThread.IsBackground = true;
        workerThread.Priority = ThreadPriority.Lowest;
        workerThread.Name = "EPG Update thread";
        workerThread.Start();
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
    }

    #endregion

    #region public members

    public void GrabEpg(Transponder transponder)
    {
      this.LogInfo("EPG grabber: use tuner {0} to grab EPG from transmitter {1}{2}{3}", IdTuner, transponder.Index, Environment.NewLine, transponder.TuningChannel);
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent += _eventHandler;
      _epgTimeOut = SettingsManagement.GetValue("timeOutEpg", 10);
      _currentTransponder = transponder;
      _state = EpgState.Idle;

      if (TuneEpgGrabber(transponder.CurrentChannel, transponder.TuningChannel, IdTuner))
      {
        _state = EpgState.Grabbing;
        _grabStartTime = DateTime.Now;
        _epgTimer.Enabled = true;
      }
    }

    public void Stop()
    {
      if (_user.CardId >= 0)
      {
        this.LogInfo("EPG grabber: tuner {0} stop grabbing", _user.CardId);
        ServiceManager.Instance.InternalControllerService.StopGrabbingEpg(_user);
      }
      ServiceManager.Instance.InternalControllerService.OnTvServerEvent -= _eventHandler;
      _epgTimer.Enabled = false;
      _state = EpgState.Idle;
      _user.CardId = -1;
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
        //if we are grabbing epg, then check if card is still idle
        else if (_state == EpgState.Grabbing)
        {
          TimeSpan ts = DateTime.Now - _grabStartTime;
          if (_user.CardId >= 0 && !IsCardIdle(_user))
          {
            //not idle? then cancel epg grabbing
            if (_state != EpgState.Idle)
            {
              this.LogInfo("EpgCard: tuner {0} EPG grabbing cancelled after {1} minute(s), no longer idle", _user.CardId, ts.TotalMinutes);
            }
            ServiceManager.Instance.InternalControllerService.AbortEPGGrabbing(_user.CardId);
            _state = EpgState.Idle;
            _user.CardId = -1;
            return;
          }

          //has grabbing run too long?
          if (ts.TotalMinutes > _epgTimeOut)
          {
            this.LogInfo("EpgCard: tuner {0} EPG grabbing timed out after {1} minute(s), aborting", _user.CardId, ts.TotalMinutes);
            ServiceManager.Instance.InternalControllerService.AbortEPGGrabbing(_user.CardId);
          }
          else
          {
            this.LogDebug("EpgCard: tuner {0} still EPG grabbing after {1} seconds...", _user.CardId, ts.TotalSeconds);
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "EpgCard: error in EPG timer");
      }
      finally
      {
        _reEntrant = false;
      }
    }

    private bool TuneEpgGrabber(Channel channel, IChannel tuningChannel, int idTuner)
    {
      bool started = false;
      try
      {
        _user.CardId = idTuner;
        _state = EpgState.Tuning;
        ICardReservation cardReservationImpl = new CardReservationTimeshifting();
        ICardTuneReservationTicket ticket = cardReservationImpl.RequestCardTuneReservation(_tuner, tuningChannel, _user, _tuner.UserManagement.GetRecentChannelId(_user.Name));
        if (ticket == null)
        {
          this.LogWarn("EPG grabber: failed to acquire tune reservation ticket");
          return false;
        }
        TvResult result = TvResult.UnknownError;
        try
        {
          result = ServiceManager.Instance.InternalControllerService.Tune(ref _user, tuningChannel, channel.IdChannel, ticket);
          if (result == TvResult.Succeeded)
          {
            if (_state == EpgState.Tuning && ServiceManager.Instance.InternalControllerService.GrabEpg(this, _user))
            {
              this.LogInfo("EPG grabber: started grabbing...");
              started = true;
              return true;
            }
            if (_state != EpgState.Tuning)
            {
              this.LogInfo("EPG grabber: tuning finished but EPG grabbing cancelled");
            }
            else
            {
              this.LogError("EPG grabber: tuner {0} failed to start EPG grabbing", idTuner);
            }
            return false;
          }
        }
        catch
        {
          CardReservationHelper.CancelCardReservation(_tuner, ticket);
          throw;
        }

        this.LogError("EPG grabber: tuner {0} failed to tune channel, result = {1}", idTuner, result);
        return false;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "EPG grabber: tuner {0} failed to tune and start EPG grabbing{1}{2}", idTuner, Environment.NewLine, tuningChannel);
        throw;
      }
      finally
      {
        if (!started)
        {
          Stop();
        }
      }
    }

    #region database update routines

    /// <summary>
    /// workerthread which will update the database with the new epg received
    /// </summary>
    private void UpdateDatabaseThread(IChannel tuningDetail, IDictionary<IChannel, IList<EpgProgram>> epg)
    {
      int cardId = _user.CardId;
      this.LogInfo("Epg: card:{0} Updating database with new programs", cardId);
      bool timeOut = false;
      _dbUpdater.ReloadConfig();
      try
      {
        foreach (var epgChannel in epg)
        {
          _dbUpdater.UpdateEpgForChannel(tuningDetail, epgChannel);
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
        Stop();
        ServiceManager.Instance.InternalControllerService.Fire(this,
                                                               new TvServerEventArgs(TvServerEventType.ProgramUpdated));
      }
    }

    #endregion

    #region helper methods

    /// <summary>
    /// Method which returns if the card is idle or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true if card is idle, otherwise false</returns>
    private bool IsCardIdle(IUser user)
    {
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
      {
        return;
      }
      TvServerEventArgs args = eventArgs as TvServerEventArgs;
      if (args == null || args.Card.Id != IdTuner)
      {
        return;
      }
      switch (args.EventType)
      {
        case TvServerEventType.StartTimeShifting:
          this.LogInfo("EPG grabber: grabbing cancelled for timeshifting");
          Stop();
          break;

        case TvServerEventType.StartRecording:
          this.LogInfo("EPG grabber: grabbing cancelled for recording");
          Stop();
          break;
      }
    }

    #endregion
  }
}