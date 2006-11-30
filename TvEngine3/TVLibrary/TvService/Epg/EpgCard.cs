/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TvControl;

using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Epg;
using System.Threading;

namespace TvService
{
  /// <summary>
  /// Class which will  grab the epg for for a specific card
  public class EpgCard : BaseEpgGrabber
  {
    #region const
    const int EpgGrabInterval = 60;//secs
    const int EpgTimeOut = (10 * 60);// 10 mins
    const int EpgReGrabAfter = 4;//hours
    #endregion

    #region enums
    enum EpgState
    {
      Idle,
      Grabbing,
      Updating,
      Stopped
    }

    #endregion

    #region variables
    System.Timers.Timer _epgTimer = new System.Timers.Timer();
    EpgState _state;
    DateTime _lastEpgGrabTime;

    bool _reEntrant = false;
    List<EpgChannel> _epg;
    int _currentCardId = -1;

    DateTime _grabStartTime;
    bool _isRunning;
    TVController _tvController;
    List<Transponder> _transponders;
    int _currentTransponderIndex = -1;
    Card _card;
    #endregion

    #region ctor
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="controller">instance of a TVController</param>
    public EpgCard(TVController controller, Card card)
    {
      _card = card;

      _tvController = controller;
      _currentCardId = -1;
      _lastEpgGrabTime = DateTime.MinValue;
      _grabStartTime = DateTime.MinValue;
      _state = EpgState.Idle;
      _currentCardId = -1;
      _epgTimer.Interval = 30000;
      _epgTimer.Elapsed += new System.Timers.ElapsedEventHandler(_epgTimer_Elapsed);
    }
    #endregion

    #region epg callback

    /// <summary>
    /// Gets the card.
    /// </summary>
    /// <value>The card.</value>
    public Card Card
    {
      get
      {
        return _card;
      }
    }

    /// <summary>
    /// Gets called when epg has been cancelled
    /// Should be overriden by the class
    /// </summary>
    public override void OnEpgCancelled()
    {
      Log.Epg("epg grabber:epg cancelled");

      if (_state == EpgState.Idle) return;
      _state = EpgState.Idle;
      _tvController.StopGrabbingEpg(_currentCardId);
      _currentCardId = -1;
      return;
    }

    /// <summary>
    /// Callback fired by the tvcontroller when EPG data has been received
    /// The method checks if epg grabbing was in progress and ifso creates a new workerthread
    /// to update the database with the new epg data
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="epg">new epg data</param>
    public override int OnEpgReceived()
    {
      try
      {
        //is epg grabbing in progress?
        if (_state == EpgState.Idle)
        {
          Log.Epg("Epg: card:{0} OnEpgReceived while idle", _currentCardId);
          return 0;
        }
        //is epg grabber already updating the database?

        if (_state == EpgState.Updating)
        {
          Log.Epg("Epg: card:{0} OnEpgReceived while updating", _currentCardId);
          return 0;
        }

        //is the card still idle?
        if (IsCardIdle(_currentCardId) == false)
        {
          Log.Epg("Epg: card:{0} OnEpgReceived but card is not idle", _currentCardId);
          _state = EpgState.Idle;
          _tvController.StopGrabbingEpg(_currentCardId);
          _currentCardId = -1;
          if (_currentTransponderIndex >= 0 && _currentTransponderIndex < _transponders.Count)
          {
            _transponders[_currentTransponderIndex].InUse = false;
            _currentTransponderIndex = -1;
          }
          return 0;
        }

        List<EpgChannel> epg = _tvController.Epg(_currentCardId);
        if (epg == null)
        {
          epg = new List<EpgChannel>();
        }
        //did we receive epg info?
        if (epg.Count == 0)
        {
          //no epg found for this channel
          Log.Epg("Epg: card:{0} no epg found", _currentCardId);
          if (_currentTransponderIndex >= 0 && _currentTransponderIndex < _transponders.Count)
          {
            Transponder transponder = _transponders[_currentTransponderIndex];
            transponder.OnTimeOut();
            transponder.InUse = false;
            _currentTransponderIndex = -1;
          }
          _state = EpgState.Idle;
          _tvController.StopGrabbingEpg(_currentCardId);
          _currentCardId = -1;
          return 0;
        }

        if (_currentTransponderIndex >= 0 && _currentTransponderIndex < _transponders.Count)
        {
          //create worker thread to update the database
          Log.Epg("Epg: card:{0} received epg for {1} channels", _currentCardId, epg.Count);
          _state = EpgState.Updating;
          _epg = epg;
          Thread workerThread = new Thread(new ThreadStart(UpdateDatabaseThread));
          workerThread.IsBackground = true;
          workerThread.Name = "EPG Update thread";
          workerThread.Start();
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return 0;
    }
    #endregion

    #region public members
    /// <summary>
    /// Property which returns true if EPG grabber is currently grabbing the epg
    /// or false is epg grabber is idle
    /// </summary>
    public bool IsRunning
    {
      get
      {
        return _isRunning;
      }
    }

    /// <summary>
    /// Grabs the epg.
    /// </summary>
    /// <param name="transponders">The transponders.</param>
    /// <param name="index">The index.</param>
    /// <param name="channel">The channel.</param>
    public void GrabEpg(List<Transponder> transponders, int index, Channel channel)
    {

      //Log.Epg("grab epg transponder: ch:{0} {1} started on {2}", index, channel.Name, _card.Name);
      _transponders = transponders;
      _currentTransponderIndex = index;
      _state = EpgState.Idle;
      _isRunning = true;
      if (GrabEpgForChannel(channel, transponders[index].Tuning, _card))
      {
        Log.Epg("Epg: card:{0} start grab {1}", _currentCardId, transponders[index].Tuning.ToString());
        transponders[index].InUse = true;
        //succeeded, then wait for epg to be received
        _state = EpgState.Grabbing;
        _grabStartTime = DateTime.Now;
        _epgTimer.Enabled = true;
        return;
      }
      else
      {
        //Log.Epg("unable to grab epg transponder: ch:{0} {1} started on {2}", index, channel.Name, _currentCardId);
        //Log.Epg("{0}", transponders[index].Tuning.ToString());
      }
    }

    /// <summary>
    /// Stops this instance.
    /// </summary>
    public void Stop()
    {
      if (_state != EpgState.Idle && _currentCardId >= 0)
      {
        Log.Epg("Epg: card:{0} stop grab", _currentCardId);
        _tvController.StopGrabbingEpg(_currentCardId);
      }
      if (_currentTransponderIndex >= 0 && _currentTransponderIndex < _transponders.Count)
      {
        _transponders[_currentTransponderIndex].InUse = false;
        _currentTransponderIndex = -1;
      }
      _epgTimer.Enabled = false;
      _isRunning = false;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is grabbing.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is grabbing; otherwise, <c>false</c>.
    /// </value>
    public bool IsGrabbing
    {
      get
      {
        return (_state != EpgState.Idle);
      }
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
    void _epgTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      //security check, dont allow re-entrancy here
      if (_reEntrant) return;

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
          if (_currentCardId >= 0)
          {
            if (!IsCardIdle(_currentCardId))
            {
              //not idle? then cancel epg grabbing
              _state = EpgState.Idle;
              _currentCardId = -1;
              if (_currentTransponderIndex >= 0 && _currentTransponderIndex < _transponders.Count)
              {
                _transponders[_currentTransponderIndex].InUse = false;
                _currentTransponderIndex = -1;
              }
              return;
            }
          }

          //wait until grabbing has finished
          TimeSpan ts = DateTime.Now - _grabStartTime;
          if (ts.TotalSeconds > EpgTimeOut)
          {
            //epg grabber timed out. Update database
            //and go back to idle mode
            Log.Epg("Epg: card:{0} timeout", _currentCardId, ts.TotalSeconds);
            if (_currentTransponderIndex >= 0 && _currentTransponderIndex < _transponders.Count)
            {
              Transponder transponder = _transponders[_currentTransponderIndex];
              transponder.OnTimeOut();
              transponder.InUse = false;
              _currentTransponderIndex = -1;
            }
            _tvController.StopGrabbingEpg(_currentCardId);
            //DatabaseManager.Instance.SaveChanges();
            //DatabaseManager.Instance.ClearQueryCache();


            _grabStartTime = DateTime.MinValue;
            _state = EpgState.Idle;
            _currentCardId = -1;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
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
    bool GrabEpgForChannel(Channel channel, IChannel tuning, Card card)
    {
      if (_tvController.AllCardsIdle == false)
      {
        Log.Epg("Epg: card:{0} cards are not idle", _currentCardId);
        return false;
      }
      IList dbsCards = Card.ListAll();

      //handle ATSC
      ATSCChannel atscChannel = tuning as ATSCChannel;
      if (atscChannel != null)
      {
        if (_tvController.Type(card.IdCard) == CardType.Atsc)
        {
          if (IsCardIdle(card.IdCard) == false)
          {
            Log.Epg("Epg: card:{0} atsc card is not idle", _currentCardId);
            return false;//card is busy
          }
          try
          {
            User cardUser;
            if (_tvController.IsCardInUse(card.IdCard, out cardUser) == false)
            {
              RemoteControl.Instance.TuneScan(card.IdCard, tuning);
              _currentCardId = card.IdCard;
              if (false == _tvController.GrabEpg(this, card.IdCard))
              {
                _currentCardId = -1;
                Log.Epg("Epg: card:{0} could not start atsc grabbing", _currentCardId);
                return false;
              }
            }
          }
          catch (Exception ex)
          {
            throw ex;
          }
          return true;
        }

        return false;
      }

      //handle DVBC
      DVBCChannel dvbcChannel = tuning as DVBCChannel;
      if (dvbcChannel != null)
      {
        if (_tvController.Type(card.IdCard) == CardType.DvbC)
        {
          if (IsCardIdle(card.IdCard) == false)
          {
            Log.Epg("Epg: card:{0} dvbc card is not idle", _currentCardId);
            return false;//card is busy
          }
          try
          {
            RemoteControl.Instance.TuneScan(card.IdCard, tuning);
            _currentCardId = card.IdCard;
            if (false == _tvController.GrabEpg(this, card.IdCard))
            {
              _currentCardId = -1;
              Log.Epg("Epg: card:{0} could not start dvbc grabbing", _currentCardId);
              return false;
            }
          }
          catch (Exception ex)
          {
            throw ex;
          }
          return true;
        }
        return false;
      }

      //handle DVBS
      DVBSChannel dvbsChannel = tuning as DVBSChannel;
      if (dvbsChannel != null)
      {
        if (_tvController.Type(card.IdCard) == CardType.DvbS)
        {
          if (IsCardIdle(card.IdCard) == false)
          {
            Log.Epg("Epg: card:{0} dvbs card is not idle", _currentCardId);
            return false;//card is busy
          }
          try
          {
            RemoteControl.Instance.TuneScan(card.IdCard, tuning);
            _currentCardId = card.IdCard;
            if (false == _tvController.GrabEpg(this, card.IdCard))
            {
              _currentCardId = -1;
              Log.Epg("Epg: card:{0} could not start dvbc grabbing", _currentCardId);
              return false;
            }
          }
          catch (Exception ex)
          {
            throw ex;
          }
          return true;
        }
        return false;
      }

      //handle DVBT
      DVBTChannel dvbtChannel = tuning as DVBTChannel;
      if (dvbtChannel != null)
      {
        if (_tvController.Type(card.IdCard) == CardType.DvbT)
        {
          if (IsCardIdle(card.IdCard) == false)
          {
            Log.Epg("Epg: card:{0} dvbt card is not idle", _currentCardId);
            return false;//card is busy
          }
          try
          {
            RemoteControl.Instance.TuneScan(card.IdCard, tuning);
            _currentCardId = card.IdCard;
            if (false == _tvController.GrabEpg(this, card.IdCard))
            {
              _currentCardId = -1;
              Log.Epg("Epg: card:{0} could not start dvbt grabbing", _currentCardId);
              return false;
            }
          }
          catch (Exception ex)
          {
            throw ex;
          }
          return true;
        }
        return false;
      }
      return false;
    }

    /// <summary>
    /// Gets the channel.
    /// </summary>
    /// <param name="idChannel">The id channel.</param>
    /// <returns></returns>
    Channel GetChannel(int idChannel)
    {
      foreach (Transponder transponder in _transponders)
      {
        foreach (Channel ch in transponder.Channels)
        {
          if (ch.IdChannel == idChannel) return ch;
        }
      }
      return null;
    }
    /// <summary>
    /// workerthread which will update the database with the new epg received
    /// </summary>
    void UpdateDatabaseThread()
    {
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;

      //if card is not idle anymore we return
      if (IsCardIdle(_currentCardId) == false)
      {
        if (_currentTransponderIndex >= 0 && _currentTransponderIndex < _transponders.Count)
        {
          _transponders[_currentTransponderIndex].InUse = false;
          _currentTransponderIndex = -1;
        }
        return;
      }
      //remove old programs from the epg
      Log.Epg("Epg: card:{0} Remove old programs from database", _currentCardId);
      TvBusinessLayer layer = new TvBusinessLayer();
      layer.RemoveOldPrograms();
      IList channels = TuningDetail.ListAll();
      bool timeOut = false;
      Log.Epg("Epg: card:{0} Updating database with new programs", _currentCardId);
      try
      {
        int channelNr = 0;
        foreach (EpgChannel epgChannel in _epg)
        {
          channelNr++;
          bool found = false;

          foreach (TuningDetail detail in channels)
          {
            if (_isRunning == false) return;
            DVBBaseChannel dvbChannel = epgChannel.Channel as DVBBaseChannel;
            if (detail.NetworkId == dvbChannel.NetworkId &&
                detail.TransportId == dvbChannel.TransportId &&
                detail.ServiceId == dvbChannel.ServiceId)
            {
              found = true;
              bool success = UpdateDatabaseChannel(channelNr, epgChannel, GetChannel(detail.IdChannel));
              if (_state != EpgState.Updating)
              {
                Log.Epg("Epg: card:{0} stopped updating state changed", _currentCardId);
                timeOut = true;
                return;
              }
              if (IsCardIdle(_currentCardId) == false)
              {
                Log.Epg("Epg: card:{0} stopped updating card not idle", _currentCardId);
                timeOut = true;
                return;
              }
              if (success)
              {
                //SaveOptions options = new SaveOptions();
                //options.IsTransactional = false;
                //options.UpdateBatchSize = 5000;
                //DatabaseManager.Instance.SaveChanges(options);
                //DatabaseManager.Instance.SaveChanges();
                Thread.Sleep(500);
              }
              break;
            }
          }
          if (!found)
          {
            DVBBaseChannel dvbChannel = epgChannel.Channel as DVBBaseChannel;
            // Log.Epg("EPG: no channel found for networkid:{0} transportid:{1} serviceid:{2}",
            //         dvbChannel.NetworkId, dvbChannel.TransportId, dvbChannel.ServiceId);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        if (timeOut == false)
        {
          if (_currentTransponderIndex >= 0 && _currentTransponderIndex < _transponders.Count)
          {
            Transponder transponder = _transponders[_currentTransponderIndex];
            transponder.OnTimeOut();
            transponder.InUse = false;
            _currentTransponderIndex = -1;
          }
        }
        if (_state != EpgState.Idle && _currentCardId >= 0)
        {
          _tvController.StopGrabbingEpg(_currentCardId);
        }
        _currentCardId = -1;
        _state = EpgState.Idle;
        _currentCardId = -1;
        Gentle.Common.CacheManager.Clear();
      }
    }

    /// <summary>
    /// method which updates the epg in the database for a single channel
    /// </summary>
    /// <param name="channelNr">channel number (for logging)</param>
    /// <param name="epgChannel">EpgChannel object containing all epg data for this channel</param>
    /// <param name="channel">channel database object</param>
    /// <returns>true if succeeded otherwise false</returns>
    bool UpdateDatabaseChannel(int channelNr, EpgChannel epgChannel, Channel channel)
    {
      if (channel == null) return false;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("epgLanguages");
      string epgLanguages = setting.Value;

      TimeSpan ts = DateTime.Now - channel.LastGrabTime;
      if (ts.TotalHours < EpgReGrabAfter)
      {
        Log.Epg("Epg: card:{0} :{1} {2} not needed lastUpdate:{3}", _currentCardId, channelNr, channel.Name, channel.LastGrabTime);
        return false;
      }
      Log.Epg("Epg: card:{0} :{1} {2}  lastUpdate:{3}", _currentCardId, channelNr, channel.Name, channel.LastGrabTime);

      //IList progs = channel.ReferringProgram();
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(TvDatabase.Program));
      sb.AddConstraint(Operator.Equals, "idChannel", channel.IdChannel);
      sb.AddOrderByField(false, "starttime");
      sb.SetRowLimit(5);
      SqlStatement stmt = sb.GetStatement(true);
      IList programsInDbs = ObjectFactory.GetCollection(typeof(TvDatabase.Program), stmt.Execute());

      DateTime lastProgram = DateTime.MinValue;
      if (programsInDbs.Count > 0)
      {
        TvDatabase.Program p = (TvDatabase.Program)programsInDbs[0];
        lastProgram = p.EndTime;
      }

      foreach (EpgProgram program in epgChannel.Programs)
      {
        if (_state != EpgState.Updating)
        {
          Log.Epg("Epg: card:{0} stopped updating state changed", _currentCardId);
          return false;
        }
        if (IsCardIdle(_currentCardId) == false)
        {
          Log.Epg("Epg: card:{0} updating card not idle", _currentCardId);
          return false;
        }
        if (program.EndTime <= lastProgram) continue;
        string title = "";
        string description = "";
        string genre = "";

        if (program.Text.Count != 0)
        {
          int offset = -1;
          for (int i = 0; i < program.Text.Count; ++i)
          {
            if (program.Text[0].Language.ToLower() == "all")
            {
              offset = i;
              break;
            }
            if (epgLanguages.Length == 0 || epgLanguages.ToLower().IndexOf(program.Text[i].Language.ToLower()) >= 0)
            {
              offset = i;
              break;
            }
          }
          if (offset != -1)
          {
            title = program.Text[offset].Title;
            description = program.Text[offset].Description;
            genre = program.Text[offset].Genre;
          }
          else
          {
            title = program.Text[0].Title;
            description = program.Text[0].Description;
            genre = program.Text[0].Genre;
          }
        }

        if (title == null) title = "";
        if (description == null) description = "";
        if (genre == null) genre = "";

        TvDatabase.Program newProgram = new TvDatabase.Program(channel.IdChannel, program.StartTime, program.EndTime, title, description, genre, false);
        newProgram.Persist();
        lastProgram = program.EndTime;

      }//foreach (EpgProgram program in epgChannel.Programs)

      channel.LastGrabTime = DateTime.Now;
      channel.Persist();
      return true;
    }

    /// <summary>
    /// Method which returns if the card is idle or not
    /// </summary>
    /// <param name="cardId">id of card</param>
    /// <returns>true if card is idle, otherwise false</returns>
    bool IsCardIdle(int cardId)
    {
      if (_isRunning == false) return false;
      if (cardId < 0) return false;
      if (RemoteControl.Instance.IsRecording(cardId)) return false;
      if (RemoteControl.Instance.IsTimeShifting(cardId)) return false;
      if (RemoteControl.Instance.IsScanning(cardId)) return false;
      User cardUser;
      if (_tvController.IsCardInUse(cardId, out cardUser)) return false;
      return true;
    }
    #endregion
  }
}
