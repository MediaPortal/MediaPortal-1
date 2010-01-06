/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using TvLibrary;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;
using TvDatabase;


namespace TvService
{
  public class CardTuner
  {
    private readonly ITvCardHandler _cardHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CardTuner"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public CardTuner(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }


    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The channel id</param>
    /// <returns></returns>
    public TvResult Tune(ref User user, IChannel channel, int idChannel)
    {
      ITvSubChannel result = null;
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return TvResult.CardIsDisabled;
        Log.Info("card: Tune {0} to {1}", _cardHandler.DataBaseCard.IdCard, channel.Name);
        lock (this)
        {
          if (_cardHandler.IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
              return RemoteControl.Instance.Tune(ref user, channel, idChannel);
            }
            catch (Exception)
            {
              Log.Error("card: unable to connect to slave controller at: {0}",
                        _cardHandler.DataBaseCard.ReferencedServer().HostName);
              return TvResult.ConnectionToSlaveFailed;
            }
          }

          //@FIX this fails for back-2-back recordings
          //if (CurrentDbChannel(ref user) == idChannel && idChannel >= 0)
          //{
          //  return true;
          //}
          Log.Debug("card: user: {0}:{1}:{2} tune {3}", user.Name, user.CardId, user.SubChannel, channel.ToString());
          _cardHandler.Card.CamType = (CamType)_cardHandler.DataBaseCard.CamType;
          _cardHandler.SetParameters();

          //check if transponder differs
          TvCardContext context = (TvCardContext)_cardHandler.Card.Context;
          if (_cardHandler.Card.SubChannels.Length > 0)
          {
            if (IsTunedToTransponder(channel) == false)
            {
              if (context.IsOwner(user) || user.IsAdmin)
              {
                Log.Debug("card: to different transponder");

                //remove all subchannels, except for this user...
                User[] users = context.Users;
                for (int i = 0; i < users.Length; ++i)
                {
                  if (users[i].Name != user.Name)
                  {
                    Log.Debug("  stop subchannel: {0} user: {1}", i, users[i].Name);

                    //fix for b2b mantis; http://mantis.team-mediaportal.com/view.php?id=1112
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
              else
              {
                Log.Debug("card: user: {0} is not the card owner. Cannot switch transponder", user.Name);
                return TvResult.NotTheOwner;
              }
            }
            else // same transponder, free previous subchannel before tuning..
            {
              _cardHandler.Card.FreeSubChannel(user.SubChannel);
            }
          }

          result = _cardHandler.Card.Tune(user.SubChannel, channel);

          bool isLocked = _cardHandler.Card.IsTunerLocked;
          Log.Debug("card: Tuner locked: {0}", isLocked);

          Log.Info("**************************************************");
          Log.Info("***** SIGNAL LEVEL: {0}, SIGNAL QUALITY: {1} *****", _cardHandler.Card.SignalLevel,
                   _cardHandler.Card.SignalQuality);
          Log.Info("**************************************************");

          if (result != null)
          {
            Log.Debug("card: tuned user: {0} subchannel: {1}", user.Name, result.SubChannelId);
            user.SubChannel = result.SubChannelId;
            user.IdChannel = idChannel;
            context.Add(user);
          }
          else
          {
            //_cardHandler.Card.FreeSubChannel(result.SubChannelId);
            return TvResult.AllCardsBusy;
          }

          //no need to recheck if signal is ok, this is done sooner now.
          /*if (!isLocked)
          {
            _cardHandler.Card.FreeSubChannel(result.SubChannelId);
            return TvResult.NoSignalDetected;
          } 
          */

          if (result.IsTimeShifting || result.IsRecording)
          {
            context.OnZap(user);
          }
          return TvResult.Succeeded;
        }
      }
      catch (TvExceptionNoSignal)
      {
        if (result != null)
        {
          _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        }
        return TvResult.NoSignalDetected;
      }
      catch (TvExceptionSWEncoderMissing ex)
      {
        Log.Write(ex);
        if (result != null)
        {
          _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        }
        return TvResult.SWEncoderMissing;
      }
      catch (TvExceptionGraphBuildingFailed ex2)
      {
        Log.Write(ex2);
        if (result != null)
        {
          _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        }
        return TvResult.GraphBuildingFailed;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        if (result != null)
        {
          _cardHandler.Card.FreeSubChannel(result.SubChannelId);
        }
        return TvResult.UnknownError;
      }
    }


    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="dbChannel">The db channel</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult CardTune(ref User user, IChannel channel, Channel dbChannel)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return TvResult.CardIsDisabled;

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return TvResult.CardIsDisabled;
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return TvResult.UnknownError;
        }

        TvResult result;
        Log.WriteFile("card: CardTune {0} {1} {2}:{3}:{4}", _cardHandler.DataBaseCard.IdCard, channel.Name, user.Name,
                      user.CardId, user.SubChannel);
        if (_cardHandler.IsScrambled(ref user))
        {
          result = Tune(ref user, channel, dbChannel.IdChannel);
          Log.Info("card2:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);
          return result;
        }
        bool cardActive = true;
        HybridCard hybridCard = _cardHandler.Card as HybridCard;
        if (hybridCard != null)
        {
          if (!hybridCard.IsCardIdActive(user.CardId))
          {
            cardActive = false;
          }
        }

        if (cardActive && _cardHandler.CurrentDbChannel(ref user) == dbChannel.IdChannel && dbChannel.IdChannel >= 0)
        {
          return TvResult.Succeeded;
        }
        result = Tune(ref user, channel, dbChannel.IdChannel);
        Log.Info("card2:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);
        return result;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
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
      if (_cardHandler.IsLocal == false)
      {
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          return RemoteControl.Instance.IsTunedToTransponder(_cardHandler.DataBaseCard.IdCard, transponder);
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }
      }
      ITvSubChannel[] subchannels = _cardHandler.Card.SubChannels;
      if (subchannels == null)
        return false;
      if (subchannels.Length == 0)
        return false;
      if (subchannels[0].CurrentChannel == null)
        return false;
      return (false == subchannels[0].CurrentChannel.IsDifferentTransponder(transponder));
    }


    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="channel">channel.</param>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(IChannel channel)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return false;

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return false;

          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.CanTune(_cardHandler.DataBaseCard.IdCard, channel);
          }
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }
        return _cardHandler.Card.CanTune(channel);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }
  }
}