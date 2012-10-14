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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardAllocation
{
  public class ChannelStates : CardAllocationBase
  {
    public delegate void OnChannelStatesSetDelegate(IUser user);
    public event OnChannelStatesSetDelegate OnChannelStatesSet;

    #region private members   

    private readonly object _lock = new object();
    private readonly object _threadlock = new object();
    private Thread _setChannelStatesThread;

    public ChannelStates()      
    {
      LogEnabled = false;
    }

    private void UpdateChannelStateUsers(IEnumerable<IUser> allUsers, ChannelState chState, int channelId)
    {
      Parallel.ForEach(allUsers, user =>
          {
            if (user != null && user.UserType != UserType.Scheduler)
            {
              try
              {
                UpdateChannelStateUser(user, chState, channelId);
              }
              catch (NullReferenceException)
              {
              }
            }
          }
        );      
    }

    private static void UpdateChannelStateUser(IUser user, ChannelState channelState, int channelId)
    {
      ChannelState currentChState;

      bool stateExists = user.ChannelStates.TryGetValue(channelId, out currentChState);

      if (stateExists)
      {
        if (channelState == ChannelState.nottunable)
        {
          return;
        }
        bool recording = (currentChState == ChannelState.recording);
        bool timeshifting = (currentChState == ChannelState.timeshifting);
        if (!recording && !timeshifting)
        {          
          user.ChannelStates[channelId] = channelState; 
        }
      }
      else
      {
        user.ChannelStates[channelId] = channelState;        
      }
    }

    private static IList<IUser> GetActiveUsers()
    {
      // find all users
      var tvControllerService = GlobalServiceProvider.Get<IInternalControllerService>();
      IDictionary<int, ITvCardHandler> cards = tvControllerService.CardCollection;
      var allUsers = new List<IUser>();
      try
      {
        ICollection<ITvCardHandler> cardHandlers = cards.Values;
        foreach (ITvCardHandler cardHandler in cardHandlers)
        {
          //get a list of all users for this card 
          IList<IUser> activeUsers = cardHandler.UserManagement.GetActiveUsersCopy();
          foreach(IUser user in activeUsers)
          {
            user.ChannelStates = new Dictionary<int, ChannelState>();  
          }
          allUsers.AddRange(activeUsers);                    
        }
      }
      catch (InvalidOperationException tex)
      {
        Log.Error("ChannelState: Possible race condition occured when getting users - {0}", tex);
      }

      return allUsers;
    }    

    private void DoSetChannelStatesForAllUsers(ICollection<Channel> channels, ICollection<IUser> allUsers)
    {
      IInternalControllerService tvControllerService = GlobalServiceProvider.Get<IInternalControllerService>();
      IDictionary<int, ITvCardHandler> cards = tvControllerService.CardCollection;
      lock (_lock)
      {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
          //construct list of all cards we can use to tune to the new channel
          Log.Debug("Controller: DoSetChannelStatesForAllUsers for {0} channels", channels.Count);

          if (allUsers == null || allUsers.Count == 0)
          {
            return; // no users, no point in continuing.
          }
          
          UpdateRecOrTSChannelStateForUsers(allUsers);

          ICollection<ITvCardHandler> cardHandlers = cards.Values;
          foreach (Channel channel in channels)
          {
            if (!channel.VisibleInGuide)
            {
              UpdateChannelStateUsers(allUsers, ChannelState.nottunable, channel.IdChannel);
              continue;
            }

              ICollection<IChannel> tuningDetails = CardAllocationCache.GetTuningDetailsByChannelId(channel);
            bool isValidTuningDetails = IsValidTuningDetails(tuningDetails);
            if (!isValidTuningDetails)
            {
              UpdateChannelStateUsers(allUsers, ChannelState.nottunable, channel.IdChannel);
              continue;
            }

            foreach (IChannel tuningDetail in tuningDetails)
            {
              foreach (ITvCardHandler cardHandler in cardHandlers)
              {
                //check if card is enabled
                if (!cardHandler.DataBaseCard.Enabled)
                {
                  //not enabled, so skip the card
                  UpdateChannelStateUsers(allUsers, ChannelState.nottunable, channel.IdChannel);
                  continue;
                }

                if (!cardHandler.Tuner.CanTune(tuningDetail))
                {
                  //card cannot tune to this channel, so skip it
                  UpdateChannelStateUsers(allUsers, ChannelState.nottunable, channel.IdChannel);
                  continue;
                }

                //check if channel is mapped to this card and that the mapping is not for "Epg Only"
                bool isChannelMappedToCard = CardAllocationCache.IsChannelMappedToCard(channel.IdChannel, cardHandler.DataBaseCard.IdCard);
                if (!isChannelMappedToCard)
                {
                  UpdateChannelStateUsers(allUsers, ChannelState.nottunable, channel.IdChannel);
                  continue;
                }

                if (!tuningDetail.FreeToAir && !cardHandler.DataBaseCard.UseConditionalAccess)
                {
                  UpdateChannelStateUsers(allUsers, ChannelState.nottunable, channel.IdChannel);
                  continue;
                }

                //ok card could be used to tune to this channel
                //now we check if its free...                              
                CheckTransponderAllUsers(channel, allUsers, cardHandler, tuningDetail);
              } //while card end
            } //foreach tuningdetail end              

            //only query once
            /*if (timeshiftingAndRecordingStates == null)
            {
              Stopwatch stopwatchTimeshiftingAndRecording = Stopwatch.StartNew();
              timeshiftingAndRecordingStates = tvControllerService.GetAllTimeshiftingAndRecordingChannels();
              stopwatchTimeshiftingAndRecording.Stop();
              Log.Info("ChannelStates.GetAllTimeshiftingAndRecordingChannels took {0} msec",
                         stopwatchTimeshiftingAndRecording.ElapsedMilliseconds);
            }
            UpdateRecOrTSChannelStateForUsers(channel, allUsers, timeshiftingAndRecordingStates);*/
          }

          RemoveAllTunableChannelStates(allUsers);        
        }
        catch (ThreadAbortException)
        {
          Log.Info("ChannelState.DoSetChannelStatesForAllUsers: thread obsolete and aborted.");
        }
        catch (InvalidOperationException tex)
        {
            Log.Error("ChannelState.DoSetChannelStatesForAllUsers: Possible race condition occured setting channel states - {0}", tex);
        }
        catch (Exception ex)
        {
            Log.Error("ChannelState.DoSetChannelStatesForAllUsers: An unknown error occured while setting channel states - {0}\n{1}", ex.Message,
                      ex);
        }
        finally
        {
          stopwatch.Stop();
          Log.Info("ChannelStates.DoSetChannelStatesForAllUsers took {0} msec", stopwatch.ElapsedMilliseconds);

          if (OnChannelStatesSet != null)
          {
            if (allUsers != null)
            {
              foreach (var user in allUsers)
              {
                Log.Debug("DoSetChannelStatesForAllUsers OnChannelStatesSet user={0}", user.Name);
                OnChannelStatesSet(user);
                try
                {
                  if (user.CardId > 0)
                  {
                    ITvCardHandler card = cards[user.CardId];
                    card.UserManagement.SetChannelStates(user.Name, user.ChannelStates); 
                  }                  
                }
                catch (Exception e)
                {
                  Log.Error("ChannelState.DoSetChannelStatesForAllUsers: could not set channel state for user: {0}, exc: {1}", user.Name, e);
                }
              } 
            }              
          }
        }
      }
    }

    private static void RemoveAllTunableChannelStates(IEnumerable<IUser> allUsers)
    {
      Parallel.ForEach(allUsers, user =>
      {
        var keysToDelete = user.ChannelStates.Where(x => x.Value == ChannelState.tunable).Select(kvp => kvp.Key).ToList();
        foreach (int key in keysToDelete)
        {
          user.ChannelStates.Remove(key);
        }
      }
      );         
    }

    private void UpdateRecOrTSChannelStateForUsers(IEnumerable<IUser> allUsers)
    {
      var tvControllerService = GlobalServiceProvider.Get<IInternalControllerService>();
            
      Stopwatch stopwatchTimeshiftingAndRecording = Stopwatch.StartNew();
      IDictionary<int, ChannelState> timeshiftingAndRecordingStates = tvControllerService.GetAllTimeshiftingAndRecordingChannels();
      stopwatchTimeshiftingAndRecording.Stop();
      Log.Info("ChannelStates.GetAllTimeshiftingAndRecordingChannels took {0} msec",
                  stopwatchTimeshiftingAndRecording.ElapsedMilliseconds);

      foreach (KeyValuePair<int, ChannelState> kvp in timeshiftingAndRecordingStates)
      {
        int idChannel = kvp.Key;
        ChannelState state = kvp.Value;

        if (state == ChannelState.recording)
        {
          UpdateChannelStateUsers(allUsers, ChannelState.recording, idChannel);
        }
        else if (state == ChannelState.timeshifting)
        {
          UpdateChannelStateUsers(allUsers, ChannelState.timeshifting, idChannel);
        }
      }
    }

    private void CheckTransponderAllUsers(Channel ch, IEnumerable<IUser> allUsers, ITvCardHandler tvcard,
                                                 IChannel tuningDetail)
    {
      Parallel.ForEach(allUsers, user =>
                {
                  if (user.UserType != UserType.Scheduler)
                  {
                    bool checkTransponder = CheckTransponder(user, tvcard, tuningDetail);
                    if (checkTransponder)
                    {
                      UpdateChannelStateUser(user, ChannelState.tunable, ch.IdChannel);
                    }
                    else
                    {
                      UpdateChannelStateUser(user, ChannelState.nottunable, ch.IdChannel);
                    }
                  }
                }
        );
    }

    #endregion

    #region public members

    private void AbortChannelStates()
    {
      lock (_threadlock)
      {
        if (_setChannelStatesThread != null && _setChannelStatesThread.IsAlive)
        {
          _setChannelStatesThread.Abort();
        }
      }
    }

    public void SetChannelStatesForAllUsers(ICollection<Channel> channels)
    {
      if (channels == null)
      {
        return;
      }
      AbortChannelStates();
      //call the real work as a thread in order to avoid slower channel changes.
      // find all users      
      ICollection<IUser> allUsers = GetActiveUsers();
      ThreadStart starter = () => DoSetChannelStatesForAllUsers(channels, allUsers);
      lock (_threadlock)
      {
        _setChannelStatesThread = new Thread(starter)
                                    {
                                      Name = "Channel state thread",
                                      IsBackground = true,
                                      Priority = ThreadPriority.Lowest
                                    };
        _setChannelStatesThread.Start();
    }
    }    

    /// <summary>
    /// Gets a list of all channel states    
    /// </summary>    
    /// <returns>dictionary containing all channel states of the channels supplied</returns>
    public void SetChannelStatesForUser(ICollection<Channel> channels, ref IUser user)
    {            
      if (channels != null)
      {
        var allUsers = new List<IUser> { user };
        DoSetChannelStatesForAllUsers(channels, allUsers);
        if (OnChannelStatesSet != null)
        {
          Log.Debug("SetChannelStatesForUser OnChannelStatesSet user={0}", user.Name);
          OnChannelStatesSet(user);
        }
      }           
    }

    #endregion
  }
}