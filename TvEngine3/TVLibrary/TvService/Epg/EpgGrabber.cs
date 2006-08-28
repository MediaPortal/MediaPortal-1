using System;
using System.Collections.Generic;
using System.Text;
using TvControl;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
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
  /// Class which will continously grab the epg for all channels
  /// Epg is grabbed when:
  ///  - channel is a DVB or ATSC channel
  ///  - if at least 2 hours have past since the previous time the epg for the channel was grabbed
  ///  - if no cards are timeshifting or recording
  /// </summary>
  public class EpgGrabber
  {
    #region enums
    enum EpgState
    {
      Idle,
      Grabbing,
      Updating,
      Stopped
    }

    #endregion

    #region const
    const int EpgGrabInterval = 60;//secs
    const int EpgTimeOut = (5 * 60);// 5 mins
    const int EpgReGrabAfter = 4;//hours
    #endregion

    #region variables
    System.Timers.Timer _epgTimer = new System.Timers.Timer();
    EpgState _state;
    DateTime _lastEpgGrabTime;

    bool _reEntrant = false;
    List<EpgChannel> _epg;
    int _currentCardId;
    Channel _currentChannel;
    DateTime _grabStartTime;
    bool _isRunning;
    TVController _tvController;
    #endregion

    #region ctor
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="controller">instance of a TVController</param>
    public EpgGrabber(TVController controller)
    {
      _tvController = controller;
      try
      {
        DatabaseManager.Instance.DefaultQueryStrategy = QueryStrategy.Normal;
        EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>();
        Log.Write("dbs:{0} channels", channels.Count);
      }
      catch (Exception)
      {
        Log.Write("Unable to open database!!!!");
      }
      _currentCardId = -1;
      _currentChannel = null;
      _lastEpgGrabTime = DateTime.MinValue;
      _grabStartTime = DateTime.MinValue;
      _state = EpgState.Idle;


      controller.OnEpgReceived += new EpgReceivedHandler(OnEpgReceived);
      _epgTimer.Interval = 1000;
      _epgTimer.Elapsed += new System.Timers.ElapsedEventHandler(_epgTimer_Elapsed);
    }
    #endregion

    #region epg callback

    /// <summary>
    /// Callback fired by the tvcontroller when EPG data has been received
    /// The method checks if epg grabbing was in progress and ifso creates a new workerthread
    /// to update the database with the new epg data
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="epg">new epg data</param>
    void OnEpgReceived(object sender, List<EpgChannel> epg)
    {
      try
      {
        //is epg grabbing in progress?
        if (_state == EpgState.Idle)
        {
          Log.Write("epg grabber:OnEpgReceived while idle");
          return;
        }
        //is epg grabber already updating the database?

        if (_state == EpgState.Updating)
        {
          Log.Write("epg grabber:OnEpgReceived while updating");
          return;
        }
        // which channel is the epg grabber currently grabbing
        if (_currentChannel == null) return;

        //is the card still idle?
        if (IsCardIdle(_currentCardId) == false)
        {
          _state = EpgState.Idle;
          return;
        }

        //was epg grabbing canceled?
        if (epg == null)
        {
          //epg grabbing was canceled
          Log.Write("EPG canceled..");
          _currentChannel = null;
          _state = EpgState.Idle;
          return;
        }

        //did we receive epg info?
        if (epg.Count == 0)
        {
          //no epg found for this channel
          Log.Write("EPG no data.");
          if (_currentChannel != null)
          {
            //update database
            _currentChannel.LastGrabTime = DateTime.Now;
            DatabaseManager.Instance.SaveChanges();
            DatabaseManager.Instance.ClearQueryCache();
            _currentChannel = null;
          }
          _state = EpgState.Idle;
          return;
        }

        if (_currentChannel != null)
        {
          //create worker thread to update the database
          Log.Write("EPG received for {0} channels.", epg.Count);
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
    /// Starts the epg grabber
    /// </summary>
    public void Start()
    {
      if (_isRunning) return;
      Log.Write("EPG: grabber started..");
      DatabaseManager.Instance.ClearQueryCache();
      _isRunning = true;
      _epgTimer.Enabled = true;
      _state = EpgState.Idle;
      _grabStartTime = DateTime.Now;
    }

    /// <summary>
    /// Stops the epg grabber
    /// </summary>
    public void Stop()
    {
      if (_isRunning == false) return;
      Log.Write("EPG: grabber stopped..");
      _epgTimer.Enabled = false;
      _state = EpgState.Stopped;
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
          GrabEpg();
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
              return;
            }
          }

          //wait until grabbing has finished
          TimeSpan ts = DateTime.Now - _grabStartTime;
          if (ts.TotalSeconds > EpgTimeOut)
          {
            //epg grabber timed out. Update database
            //and go back to idle mode
            Log.Write("EPG timeout:{0}.", ts.TotalSeconds);
            _currentChannel.LastGrabTime = DateTime.Now;
            DatabaseManager.Instance.SaveChanges();
            DatabaseManager.Instance.ClearQueryCache();

            _currentChannel = null;
            _grabStartTime = DateTime.MinValue;
            _state = EpgState.Idle;
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
    /// Method which returns if a channel is analog tv or digital tv
    /// </summary>
    /// <param name="channel">Channel</param>
    /// <returns>true if channel is digital otherwise false</returns>
    bool IsDigitalChannel(Channel channel)
    {
      foreach (TuningDetail detail in channel.TuningDetails)
      {
        if (detail.ChannelType > 0)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// This method checks if there is a channel who's epg needs to be updated
    /// Channel is updated when:
    ///   - it has been at least 2 hours since the last time the epg was grabbed
    ///   - there is a card available which can receive the channel at this time
    /// </summary>
    void GrabEpg()
    {
      TimeSpan ts = DateTime.Now - _lastEpgGrabTime;
      if (ts.TotalSeconds < EpgGrabInterval) return;
      _lastEpgGrabTime = DateTime.Now;
      TvBusinessLayer layer = new TvBusinessLayer();
      EntityList<Channel> channels = DatabaseManager.Instance.GetEntities<Channel>();
      try
      {
        foreach (Channel channel in channels)
        {
          // channel.GrabEpg = true;
          // channel.LastGrabTime = new DateTime(2000, 1, 1, 0, 0, 0);
          if (channel.GrabEpg == false) continue;
          ts = DateTime.Now - channel.LastGrabTime;
          if (ts.TotalHours < EpgReGrabAfter) continue; // less then 2 hrs ago
          if (!IsDigitalChannel(channel))
          {
            //dont grab epg for analog channels
            channel.GrabEpg = false;
            channel.LastGrabTime = DateTime.Now;
            DatabaseManager.Instance.SaveChanges();
            DatabaseManager.Instance.ClearQueryCache();
            continue;
          }
          // this channel needs its epg to be updated.
          // check which cards can receive it and if one is free
          List<IChannel> tuningList = layer.GetTuningChannelByName(channel.Name);
          foreach (IChannel tuning in tuningList)
          {
            //try grabbing the epg for this channel
            if (GrabEpgForChannel(channel, tuning))
            {
              //succeeded, then wait for epg to be received
              _currentChannel = channel;
              _state = EpgState.Grabbing;
              _grabStartTime = DateTime.Now;
              return;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
    /// <summary>
    /// This method will try to start the epg grabber for the channel and tuning details specified
    /// Epg grabbing can only be started if there is a card idle which can receive the channel specified
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="tuning"></param>
    /// <returns></returns>
    bool GrabEpgForChannel(Channel channel, IChannel tuning)
    {
      EntityList<Card> dbsCards = DatabaseManager.Instance.GetEntities<Card>();

      //handle ATSC
      ATSCChannel atscChannel = tuning as ATSCChannel;
      if (atscChannel != null)
      {
        foreach (Card card in dbsCards)
        {
          if (_tvController.Type(card.IdCard) == CardType.Atsc)
          {
            if (IsCardIdle(card.IdCard) == false)
            {
              continue;//card is busy
            }
            Log.Write("EPG: grab atsc epg for {0}", tuning.ToString());
            bool cardLocked = false;
            try
            {
              cardLocked = _tvController.Lock(card.IdCard);
              if (cardLocked == false) continue;
              RemoteControl.Instance.TuneScan(card.IdCard, tuning);
              _currentCardId = card.IdCard;
              RemoteControl.Instance.GrabEpg(card.IdCard);
            }
            catch (Exception ex)
            {
              throw ex;
            }
            finally
            {
              if (cardLocked) _tvController.Unlock(card.IdCard);
            }
            return true;
          }
        }

        return false;
      }

      //handle DVBC
      DVBCChannel dvbcChannel = tuning as DVBCChannel;
      if (dvbcChannel != null)
      {
        foreach (Card card in dbsCards)
        {
          if (_tvController.Type(card.IdCard) == CardType.DvbC)
          {
            if (IsCardIdle(card.IdCard) == false)
            {
              continue;//card is busy
            }
            Log.Write("EPG: grab dvbc epg for {0}", tuning.ToString());
            bool cardLocked = false;
            try
            {
              cardLocked = _tvController.Lock(card.IdCard);
              if (cardLocked == false) continue;
              RemoteControl.Instance.TuneScan(card.IdCard, tuning);
              _currentCardId = card.IdCard;
              RemoteControl.Instance.GrabEpg(card.IdCard);
            }
            catch (Exception ex)
            {
              throw ex;
            }
            finally
            {
              if (cardLocked) _tvController.Unlock(card.IdCard);
            }
            return true;
          }
        }
        return false;
      }

      //handle DVBS
      DVBSChannel dvbsChannel = tuning as DVBSChannel;
      if (dvbsChannel != null)
      {
        foreach (Card card in dbsCards)
        {
          if (_tvController.Type(card.IdCard) == CardType.DvbS)
          {
            if (IsCardIdle(card.IdCard) == false)
            {
              continue;//card is busy
            }
            Log.Write("EPG: grab dvbs epg for {0}", tuning.ToString());
            bool cardLocked = false;
            try
            {
              cardLocked = _tvController.Lock(card.IdCard);
              if (cardLocked == false) continue;
              RemoteControl.Instance.TuneScan(card.IdCard, tuning);
              _currentCardId = card.IdCard;
              RemoteControl.Instance.GrabEpg(card.IdCard);
            }
            catch (Exception ex)
            {
              throw ex;
            }
            finally
            {
              if (cardLocked) _tvController.Unlock(card.IdCard);
            }
            return true;
          }
        }
        return false;
      }

      //handle DVBT
      DVBTChannel dvbtChannel = tuning as DVBTChannel;
      if (dvbtChannel != null)
      {
        foreach (Card card in dbsCards)
        {
          if (_tvController.Type(card.IdCard) == CardType.DvbT)
          {
            if (IsCardIdle(card.IdCard) == false)
            {
              continue;//card is busy
            }
            Log.Write("EPG: grab dvbt epg for {0}", tuning.ToString());
            bool cardLocked = false;
            try
            {
              cardLocked = _tvController.Lock(card.IdCard);
              if (cardLocked == false) continue;
              RemoteControl.Instance.TuneScan(card.IdCard, tuning);
              _currentCardId = card.IdCard;
              RemoteControl.Instance.GrabEpg(card.IdCard);
            }
            catch (Exception ex)
            {
              throw ex;
            }
            finally
            {
              if (cardLocked) _tvController.Unlock(card.IdCard);
            }
            return true;
          }
        }
        return false;
      }
      return false;
    }

    /// <summary>
    /// workerthread which will update the database with the new epg received
    /// </summary>
    void UpdateDatabaseThread()
    {
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;

      //if card is not idle anymore we return
      if (IsCardIdle(_currentCardId) == false) return;

      //remove old programs from the epg
      Log.Write("EPG: Remove old programs from database...");
      TvBusinessLayer layer = new TvBusinessLayer();
      layer.RemoveOldPrograms();
      DatabaseManager.Instance.SaveChanges();
      DatabaseManager.Instance.ClearQueryCache();

      Log.Write("EPG: Updating database with new programs...");
      try
      {
        EntityList<TuningDetail> channels = DatabaseManager.Instance.GetEntities<TuningDetail>();
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
              bool success = UpdateDatabaseChannel(channelNr, epgChannel, detail.Channel);
              if (_state != EpgState.Updating)
              {
                _currentChannel = null;
                Log.Write("EPG: stopped updating state changed");
                return;
              }
              if (IsCardIdle(_currentCardId) == false)
              {
                _currentChannel = null;
                Log.Write("EPG: stopped updating card not idle");
                return;
              }
              if (success)
              {
                DatabaseManager.Instance.SaveChanges();
              }
              break;
            }
          }
          if (!found)
          {
            DVBBaseChannel dvbChannel = epgChannel.Channel as DVBBaseChannel;
            Log.Write("EPG: no channel found for networkid:{0} transportid:{1} serviceid:{2}",
                    dvbChannel.NetworkId, dvbChannel.TransportId, dvbChannel.ServiceId);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        if (_currentChannel != null)
        {
          _currentChannel.LastGrabTime = DateTime.Now;
          DatabaseManager.Instance.SaveChanges();
          Log.Write("EPG: database updated for {0} {1} {2}", _currentChannel.Name, _state, IsCardIdle(_currentCardId));
        }
        _state = EpgState.Idle;
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
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("epgLanguages");
      string epgLanguages = setting.Value;
      TimeSpan ts = DateTime.Now - channel.LastGrabTime;
      if (ts.TotalHours < EpgReGrabAfter)
      {
        Log.Write("EPG: channel:{0} {1} not needed lastUpdate:{2}", channelNr, channel.Name, channel.LastGrabTime);
        return false;
      }
      Log.Write("EPG: channel:{0} {1} titles:{2} lastUpdate:{3}", channelNr, channel.Name, epgChannel.Programs.Count, channel.LastGrabTime);

      ReadOnlyEntityList<TvDatabase.Program> progs = channel.Programs;
      //List<TvDatabase.Program> programsToDelete = new List<TvDatabase.Program>();
      EntityQuery query = new EntityQuery(typeof(TvDatabase.Program));
      query.AddClause(TvDatabase.Program.IdChannelEntityColumn, EntityQueryOp.EQ, channel.IdChannel);
      query.AddOrderBy(TvDatabase.Program.StartTimeEntityColumn, System.ComponentModel.ListSortDirection.Descending);
      query.Top = 5;
      EntityList<TvDatabase.Program> programsInDbs = DatabaseManager.Instance.GetEntities<TvDatabase.Program>(query);
      DateTime lastProgram = DateTime.MinValue;
      if (programsInDbs.Count > 0)
      {
        lastProgram = programsInDbs[0].EndTime;
      }

      foreach (EpgProgram program in epgChannel.Programs)
      {
        if (_state != EpgState.Updating)
        {
          Log.Write("EPG: stopped updating state changed");
          return false;
        }
        if (IsCardIdle(_currentCardId) == false)
        {
          Log.Write("EPG: stopped updating card not idle");
          return false;
        }
        if (program.Text.Count == 0) continue;
        if (program.EndTime <= lastProgram) continue;

        int offset = -1;
        for (int i = 0; i < program.Text.Count; ++i)
        {
          if (program.Text[0].Language == "ALL")
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
        if (offset == -1)
        {
          channel.LastGrabTime = DateTime.Now;
          return true;
        }
        string title = program.Text[offset].Title;
        string description = program.Text[offset].Description;
        string genre = program.Text[offset].Genre;

        if (title == null) title = "";
        if (description == null) description = "";
        if (genre == null) genre = "";

        TvDatabase.Program newProgram = TvDatabase.Program.Create();
        newProgram.Channel = channel;
        newProgram.IdChannel = channel.IdChannel;
        newProgram.StartTime = program.StartTime;
        newProgram.EndTime = program.EndTime;
        newProgram.Description = description;
        newProgram.Title = title;
        newProgram.Genre = genre;
        lastProgram = program.EndTime;
        /*
        bool changed = false;
        foreach (TvDatabase.Program dbsProgram in programsInDbs)
        {
          if (_state != EpgState.Updating) return false;
          if (IsCardIdle(_currentCard) == false) return false;
          bool isCorrect = false;
          if (dbsProgram.EndTime > program.StartTime && dbsProgram.EndTime < program.EndTime) isCorrect = true;
          else if (dbsProgram.StartTime >= program.StartTime && dbsProgram.StartTime <= program.EndTime) isCorrect = true;
          else if (dbsProgram.StartTime <= program.StartTime && dbsProgram.EndTime >= program.EndTime) isCorrect = true;
          if (isCorrect)
          {
            if (changed)
            {
              programsToDelete.Add(dbsProgram);
            }
            else
            {
              changed = true;
              dbsProgram.StartTime = program.StartTime;
              dbsProgram.EndTime = program.EndTime;
              dbsProgram.Description = description;
              dbsProgram.Title = title;
              dbsProgram.Genre = genre;
            }
          }
        }

        if (!changed)
        {
          TvDatabase.Program newProgram = TvDatabase.Program.Create();
          newProgram.Channel = channel;
          newProgram.IdChannel = channel.IdChannel;
          newProgram.StartTime = program.StartTime;
          newProgram.EndTime = program.EndTime;
          newProgram.Description = description;
          newProgram.Title = title;
          newProgram.Genre = genre;

        }
        foreach (TvDatabase.Program dbsProgram in programsToDelete)
        {
          programsInDbs.Remove(dbsProgram);
        }*/
      }//foreach (EpgProgram program in epgChannel.Programs)

      /*
    foreach (TvDatabase.Program dbsProgram in programsToDelete)
    {
      if (_state != EpgState.Updating)
      {
        Log.Write("EPG: stopped updating state changed");
        return false;
      }
      //System.Threading.Thread.Sleep(100);
      if (IsCardIdle(_currentCard) == false) return false;
      if (dbsProgram.RowState != System.Data.DataRowState.Deleted &&
          dbsProgram.RowState != System.Data.DataRowState.Detached)
      {
        dbsProgram.Delete();
      }
    }*/
      channel.LastGrabTime = DateTime.Now;
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
      if (_tvController.IsLocked(cardId)) return false;
      return true;
    }
    #endregion
  }
}
