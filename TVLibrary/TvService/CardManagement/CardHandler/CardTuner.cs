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
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;
using TvDatabase;

#endregion

namespace TvService
{
  public class CardTuner
  {
    #region private vars

    private readonly ITvCardHandler _cardHandler;

    #endregion

    #region delegates / events

    public event OnAfterTuneDelegate OnAfterTuneEvent;
    public delegate void OnAfterTuneDelegate(ITvCardHandler cardHandler);
    public event OnBeforeTuneDelegate OnBeforeTuneEvent;
    public delegate void OnBeforeTuneDelegate(ITvCardHandler cardHandler);

    #endregion        

    #region ctors

    /// <summary>
    /// Initializes a new instance of the <see cref="CardTuner"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public CardTuner(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }

    #endregion

    #region public methods

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The channel id</param>
    /// <returns></returns>
    public TvResult Tune(ref IUser user, IChannel channel, int idChannel)
    {
      ITvSubChannel result = null;
      TvResult tvResult;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          tvResult = TvResult.CardIsDisabled;
        }
        else
        {
          Log.Info("card: Tune {0} to {1}", _cardHandler.DataBaseCard.IdCard, channel.Name);
          if (_cardHandler.IsLocal == false)
          {
            tvResult = RemoteTune(channel, ref user, idChannel);
          }
          else
          {
            tvResult = BeforeTune(channel, ref user);
            bool canTune = (tvResult == TvResult.Succeeded);
            if (canTune)
            {
              user.FailedCardId = -1;
              result = _cardHandler.Card.Tune(user.SubChannel, channel);

              if (result != null)
              {
                tvResult = AfterTune(user, idChannel, result);
              }
              else
              {
                tvResult = TvResult.UnknownError;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        user.FailedCardId = _cardHandler.DataBaseCard.IdCard;
        Log.Write(ex);
        if (result != null)
        {
          _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        }
        tvResult = TranslateExceptionToTvResult(ex);
      }

      return tvResult;
    }

    /// <summary>
    /// Scans the the specified card to the channel.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The channel id</param>
    /// <returns></returns>
    public TvResult Scan(ref IUser user, IChannel channel, int idChannel)
    {
      ITvSubChannel result = null;
      TvResult tvResult;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          tvResult = TvResult.CardIsDisabled;
        }
        else
        {
          Log.Info("card: Scan {0} to {1}", _cardHandler.DataBaseCard.IdCard, channel.Name);
          if (_cardHandler.IsLocal == false)
          {
            tvResult = RemoteScan(channel, ref user, idChannel);
          }
          else
          {
            tvResult = BeforeTune(channel, ref user);
            bool canTune = (tvResult == TvResult.Succeeded);
            if (canTune)
            {
              result = _cardHandler.Card.Scan(user.SubChannel, channel);
              if (result != null)
              {
                tvResult = AfterTune(user, idChannel, result);
              }
              else
              {
                tvResult = TvResult.UnknownError;
              }  
            }              
          }                      
        }
      }     
      catch (Exception ex)
      {
        Log.Write(ex);
        if (result != null)
        {
          _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        }
        tvResult = TranslateExceptionToTvResult(ex);
      }
      return tvResult;
    }

    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="dbChannel">The db channel</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult CardTune(ref IUser user, IChannel channel, Channel dbChannel)
    {
      TvResult tvResult;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          tvResult = TvResult.CardIsDisabled;
        }
        else
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            {
              tvResult = TvResult.CardIsDisabled;
            }
            else
            {              
              Log.WriteFile("card: CardTune {0} {1} {2}:{3}:{4}", _cardHandler.DataBaseCard.IdCard, channel.Name, user.Name,
                            user.CardId, user.SubChannel);
              if (_cardHandler.IsScrambled(ref user))
              {
                tvResult = Tune(ref user, channel, dbChannel.IdChannel);
                Log.Info("card2:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);
              }
              else
              {                                
                if (IsActiveCardOnCurrentUserChannel(ref user, dbChannel))
                {
                  tvResult = TvResult.Succeeded;
                }
                else
                {
                  tvResult = Tune(ref user, channel, dbChannel.IdChannel);
                  Log.Info("card2:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);                  
                }                
              }
            }
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      _cardHandler.DataBaseCard.ReferencedServer().HostName);
            tvResult = TvResult.UnknownError;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        tvResult = TvResult.UnknownError;
      }
      return tvResult;
    }
    

    /// <summary>
    /// Determines whether card is tuned to the transponder specified by transponder
    /// </summary>
    /// <param name="transponder">The transponder.</param>
    /// <returns>
    /// 	<c>true</c> if card is tuned to the transponder; otherwise, <c>false</c>.
    /// </returns>
    public bool IsTunedToTransponder(IChannel transponder)
    {
      bool isTunedToTransponder;
      if (_cardHandler.IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          isTunedToTransponder = RemoteControl.Instance.IsTunedToTransponder(_cardHandler.DataBaseCard.IdCard, transponder);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          isTunedToTransponder = false;
        }
      }
      else
      {
        ITvSubChannel[] subchannels = _cardHandler.Card.SubChannels;
        if (subchannels == null)
        {
          isTunedToTransponder = false;
        }
        else if (subchannels.Length == 0)
        {
          isTunedToTransponder = false;
        }
        else if (subchannels[0].CurrentChannel == null)
        {
          isTunedToTransponder = false;
        }
        else
        {
          isTunedToTransponder = (false == subchannels[0].CurrentChannel.IsDifferentTransponder(transponder));
        } 
      }      

      return isTunedToTransponder;
    }


    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="channel">channel.</param>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {
      bool canTune = false;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled)        
        {
          if (_cardHandler.IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
              if (RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
              {
                if (_cardHandler.IsLocal == false)
                {
                  canTune = RemoteControl.Instance.CanTune(_cardHandler.DataBaseCard.IdCard, channel);
                }
              }
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at:{0}",
                        _cardHandler.DataBaseCard.ReferencedServer().HostName);
              canTune = false;
            }
          }
          else
          {
            canTune = _cardHandler.Card.CanTune(channel); 
          }          
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        canTune = false;
      }
      return canTune;
    }

    #endregion

    #region private methods

    private bool IsActiveCardOnCurrentUserChannel(ref IUser user, Channel dbChannel)
    {
      bool cardActive = IsHybridCardActive(user);
      return cardActive && _cardHandler.CurrentDbChannel(ref user) == dbChannel.IdChannel &&
             dbChannel.IdChannel >= 0;
    }

    private bool IsHybridCardActive(IUser user)
    {
      bool cardActive = true;
      var hybridCard = _cardHandler.Card as HybridCard;
      if (hybridCard != null)
      {
        if (!hybridCard.IsCardIdActive(user.CardId))
        {
          cardActive = false;
        }
      }
      return cardActive;
    }

    private TvResult RemoteScan(IChannel channel, ref IUser user, int idChannel)
    {
      TvResult tvResult;
      try
      {
        RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
        tvResult = RemoteControl.Instance.Scan(ref user, channel, idChannel);
      }
      catch (Exception)
      {
        Log.Error("card: unable to connect to slave controller at: {0}",
                  _cardHandler.DataBaseCard.ReferencedServer().HostName);
        tvResult = TvResult.ConnectionToSlaveFailed;
      }
      return tvResult;
    }

    private static TvResult TranslateExceptionToTvResult(Exception ex)
    {
      TvResult tvResult;

      if (ex is TvExceptionNoSignal)
      {
        tvResult = TvResult.NoSignalDetected;
      }
      else if (ex is TvExceptionSWEncoderMissing)
      {
        tvResult = TvResult.SWEncoderMissing;
      }
      else if (ex is TvExceptionGraphBuildingFailed)
      {
        tvResult = TvResult.GraphBuildingFailed;
      }
      else if (ex is TvExceptionNoPMT)
      {
        tvResult = TvResult.NoPmtFound;
      }
      else
      {
        tvResult = TvResult.UnknownError;
      }
      return tvResult;
    }

    private TvResult RemoteTune(IChannel channel, ref IUser user, int idChannel)
    {
      TvResult tvResult;
      try
      {
        RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
        tvResult = RemoteControl.Instance.Tune(ref user, channel, idChannel);
      }
      catch (Exception)
      {
        Log.Error("card: unable to connect to slave controller at: {0}",
                  _cardHandler.DataBaseCard.ReferencedServer().HostName);
        tvResult = TvResult.ConnectionToSlaveFailed;
      }
      return tvResult;
    }

    private TvResult AfterTune(IUser user, int idChannel, ITvSubChannel result)
    {
      TvResult tvResult;
      bool isLocked = _cardHandler.Card.IsTunerLocked;
      Log.Debug("card: Tuner locked: {0}", isLocked);

      Log.Info("**************************************************");
      Log.Info("***** SIGNAL LEVEL: {0}, SIGNAL QUALITY: {1} *****", _cardHandler.Card.SignalLevel,
               _cardHandler.Card.SignalQuality);
      Log.Info("**************************************************");

      var context = _cardHandler.Card.Context as ITvCardContext;
      if (result != null)
      {
        Log.Debug("card: tuned user: {0} subchannel: {1}", user.Name, result.SubChannelId);
        user.SubChannel = result.SubChannelId;
        user.IdChannel = idChannel;
        if (context != null)
        {
          context.Add(user);
        }
        if (result.IsTimeShifting || result.IsRecording)
        {
          if (context != null)
          {
            context.OnZap(user);
          }
        }
        tvResult = TvResult.Succeeded;
      }
      else
      {
        tvResult = TvResult.AllCardsBusy;
      }      
      
      return tvResult;
    }

    private TvResult BeforeTune(IChannel channel, ref IUser user)
    {
      var result = TvResult.Succeeded; 
      Log.Debug("card: user: {0}:{1}:{2} tune {3}", user.Name, user.CardId, user.SubChannel, channel.ToString());
      _cardHandler.Card.CamType = (CamType)_cardHandler.DataBaseCard.CamType;
      _cardHandler.SetParameters();

      //check if transponder differs
      var context = _cardHandler.Card.Context as ITvCardContext;
      if (_cardHandler.Card.SubChannels.Length > 0)
      {
        if (IsTunedToTransponder(channel) == false)
        {
          if (IsUserOwnerOrAdmin(user, context))
          {
            Log.Debug("card: to different transponder");            
            RemoveAllSubChannelsExceptCurrentUser(user, context);
          }
          else
          {
            Log.Debug("card: user: {0} is not the card owner. Cannot switch transponder", user.Name);
            result = TvResult.NotTheOwner;            
          }
        }
        else // same transponder, free previous subchannel before tuning..
        {
          _cardHandler.Card.FreeSubChannel(user.SubChannel);
        }
      }

      if (result == TvResult.Succeeded)
      {
        RaiseOnBeforeTuneEvent();
        AttachAfterTuneEventHandlers();
      }
      return result;
    }

    private void AttachAfterTuneEventHandlers() 
    {
      var card = _cardHandler.Card as TvCardBase;
      if (card != null)
      {
        card.AfterTuneEvent -= Card_OnAfterTuneEvent;
        card.AfterTuneEvent += Card_OnAfterTuneEvent;
      }
      else
      {
        var hybridCard = _cardHandler.Card as HybridCard;
        if (hybridCard != null)
        {
          hybridCard.AfterTuneEvent = new TvCardBase.OnAfterTuneDelegate(Card_OnAfterTuneEvent);
        }
      }
    }

    private void RaiseOnBeforeTuneEvent()
    {
      if (OnBeforeTuneEvent != null)
      {
        OnBeforeTuneEvent(_cardHandler);
      }
    }

    private static bool IsUserOwnerOrAdmin(IUser user, ITvCardContext context)
    {
      return context != null && (context.IsOwner(user) || user.IsAdmin);
    }

    private void RemoveAllSubChannelsExceptCurrentUser(IUser user, ITvCardContext context) 
    {
      IUser[] users = context.Users;
      for (int i = 0; i < users.Length; ++i)
      {
        if (users[i].Name != user.Name)
        {
          Log.Debug("  stop subchannel: {0} user: {1}", i, users[i].Name);                
          if (users[i].IsAdmin)
            // if we are stopping an on-going recording/schedule (=admin), we have to make sure that we remove the schedule also.
          {
            Log.Debug("user is scheduler: {0}", users[i].Name);
            int recScheduleId = RemoteControl.Instance.GetRecordingSchedule(users[i].CardId,
                                                                            users[i].IdChannel);

            if (recScheduleId > 0)
            {
              Schedule schedule = Schedule.Retrieve(recScheduleId);
              Log.Info("removing schedule with id: {0}", schedule.IdSchedule);
              RemoteControl.Instance.StopRecordingSchedule(schedule.IdSchedule);
              schedule.Delete();
            }
          }
          else
          {
            _cardHandler.Card.FreeSubChannel(users[i].SubChannel);
            context.Remove(users[i]);
          }
        }
      }
    }

    private void Card_OnAfterTuneEvent()
    {
      if (OnAfterTuneEvent != null)
      {
        OnAfterTuneEvent(_cardHandler);
      }
    }

    #endregion    
  }
} 