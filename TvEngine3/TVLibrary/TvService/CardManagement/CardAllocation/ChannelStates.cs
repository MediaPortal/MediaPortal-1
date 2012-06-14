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
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;
using TvDatabase;

namespace TvService
{
  public class ChannelStates : CardAllocationBase
  {
    #region private members   
    
    private readonly object _lock = new object();
    private readonly object _threadlock = new object();
    private Thread _setChannelStatesThread;

    public ChannelStates(TvBusinessLayer businessLayer, IController controller) : base(businessLayer, controller)
    {
      LogEnabled = false;
    }

    private void UpdateChannelStateUsers(IEnumerable<IUser> allUsers, ChannelState chState, int channelId)
    {
      foreach (IUser t in allUsers)
      {
        IUser u = null;
        try
        {
          u = t;
        }
        catch (NullReferenceException) {}

        if (u == null)
          continue;
        if (u.IsAdmin)
          continue; //scheduler users do not need to have their channelstates set.

        try
        {
          UpdateChannelStateUser(u, chState, channelId);
        }
        catch (NullReferenceException) {}
      }
    }

    private static void UpdateChannelStateUser(IUser user, ChannelState chState, int channelId)
    {
      ChannelState currentChState;

      bool stateExists = user.ChannelStates.TryGetValue(channelId, out currentChState);

      if (stateExists)
      {
        if (chState == ChannelState.nottunable)
        {
          return;
        }
        bool recording = (currentChState == ChannelState.recording);
        if (!recording)
        {
          user.ChannelStates[channelId] = chState;
          //add key if does not exist, or update existing one.                            
        }
      }
      else
      {
        user.ChannelStates[channelId] = chState;
        //add key if does not exist, or update existing one.                          
      }
    }

    private static IList<IUser> GetActiveUsers(IDictionary<int, ITvCardHandler> cards)
    {
      // find all users
      var allUsers = new List<IUser>();
      try
      {
        ICollection<ITvCardHandler> cardHandlers = cards.Values;
        foreach (ITvCardHandler cardHandler in cardHandlers)
        {
          //get a list of all users for this card
          IUser[] usersAvail = cardHandler.Users.GetUsers();
          if (usersAvail != null)
          {            
            foreach (IUser tmpUser in usersAvail.Where(tmpUser => !tmpUser.IsAdmin)) 
            {
              tmpUser.ChannelStates = new Dictionary<int, ChannelState>();
              allUsers.Add(tmpUser);
            }
          }
        }
      }
      catch (InvalidOperationException tex)
      {
        Log.Error("ChannelState: Possible race condition occured when getting users - {0}", tex);
      }

      return allUsers;
    }
    
    private void DoSetChannelStates(IDictionary<int, ITvCardHandler> cards, ICollection<Channel> channels, ICollection<IUser> allUsers, IController tvController)
    {
      lock (_lock)
      {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
          //construct list of all cards we can use to tune to the new channel
          Log.Debug("Controller: DoSetChannelStates for {0} channels", channels.Count);

          if (allUsers == null || allUsers.Count == 0)
          {
            return; // no users, no point in continuing.
          }

          IDictionary<int, ChannelState> timeshiftingAndRecordingStates = null;
          ICollection<ITvCardHandler> cardHandlers = cards.Values;
          foreach (Channel ch in channels)
          {
            if (!ch.VisibleInGuide)
            {
              UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
              continue;
            }

            ICollection<IChannel> tuningDetails = CardAllocationCache.GetTuningDetailsByChannelId(ch);
            bool isValidTuningDetails = IsValidTuningDetails(tuningDetails);
            if (!isValidTuningDetails)
            {
              UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
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
                  UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                  continue;
                }

                if (!cardHandler.Tuner.CanTune(tuningDetail))
                {
                  //card cannot tune to this channel, so skip it
                  UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                  continue;
                }

                //check if channel is mapped to this card and that the mapping is not for "Epg Only"
                bool isChannelMappedToCard = CardAllocationCache.IsChannelMappedToCard(ch, cardHandler.DataBaseCard);//, channelMapping);
                if (!isChannelMappedToCard)
                {
                  UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                  continue;
                }

                if (!tuningDetail.FreeToAir && !cardHandler.DataBaseCard.CAM)
                {
                  UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                  continue;
                }

                //ok card could be used to tune to this channel
                //now we check if its free...                                            
                CheckTransponderAllUsers(ch, allUsers, cardHandler, tuningDetail);
              } //while card end
            } //foreach tuningdetail end              

            //only query once
            if (timeshiftingAndRecordingStates == null)
            {
              Stopwatch stopwatchTimeshiftingAndRecording = Stopwatch.StartNew();
              timeshiftingAndRecordingStates = tvController.GetAllTimeshiftingAndRecordingChannels();
              stopwatchTimeshiftingAndRecording.Stop();
              Log.Info("ChannelStates.GetAllTimeshiftingAndRecordingChannels took {0} msec",
                       stopwatchTimeshiftingAndRecording.ElapsedMilliseconds);
            }
            UpdateRecOrTSChannelStateForUsers(ch, allUsers, timeshiftingAndRecordingStates);
          }

          RemoveAllTunableChannelStates(allUsers);
        }
        catch (ThreadAbortException)
        {
          Log.Info("ChannelState.DoSetChannelStates: thread obsolete and aborted.");
        }
        catch (InvalidOperationException tex)
        {
          Log.Error("ChannelState.DoSetChannelStates: Possible race condition occured setting channel states - {0}", tex);
        }
        catch (Exception ex)
        {
          Log.Error("ChannelState.DoSetChannelStates: An unknown error occured while setting channel states - {0}\n{1}", ex.Message,
                    ex);
        }
        finally
        {
          stopwatch.Stop();
          Log.Info("ChannelStates.DoSetChannelStates took {0} msec", stopwatch.ElapsedMilliseconds);
        }
      }
    }

    private static void RemoveAllTunableChannelStates(IEnumerable<IUser> allUsers)
    {
      foreach (IUser user in allUsers)
      {
        var keysToDelete = user.ChannelStates.Where(x => x.Value == ChannelState.tunable).Select(kvp => kvp.Key).ToList();
        foreach (int key in keysToDelete)
        {
          user.ChannelStates.Remove(key);
        }
      }     
    }

    private void UpdateRecOrTSChannelStateForUsers(Channel ch, IEnumerable<IUser> allUsers,
                                                          IDictionary<int, ChannelState> TSandRecStates)
    {
      ChannelState cs;
      TSandRecStates.TryGetValue(ch.IdChannel, out cs);

      if (cs == ChannelState.recording)
      {
        UpdateChannelStateUsers(allUsers, ChannelState.recording, ch.IdChannel);
      }
      else if (cs == ChannelState.timeshifting)
      {
        UpdateChannelStateUsers(allUsers, ChannelState.timeshifting, ch.IdChannel);
      }
    }

    private void CheckTransponderAllUsers(Channel ch, IEnumerable<IUser> allUsers, ITvCardHandler tvcard,
                                                 IChannel tuningDetail)
    {      
      foreach (IUser user in allUsers) 
      {
        //ignore admin users, like scheduler
        if (!user.IsAdmin)
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

    public void SetChannelStates(IDictionary<int, ITvCardHandler> cards, ICollection<Channel> channels,
                                 IController tvController)
    {
      if (channels == null)
      {
        return;
      }
      AbortChannelStates();
      //call the real work as a thread in order to avoid slower channel changes.
      // find all users      
      ICollection<IUser> allUsers = GetActiveUsers(cards);
      ThreadStart starter = () => DoSetChannelStates(cards, channels, allUsers, tvController);
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
    public Dictionary<int, ChannelState> GetChannelStates(IDictionary<int, ITvCardHandler> cards, ICollection<Channel> channels,
                                                          ref IUser user,
                                                          IController tvController)
    {
      if (channels == null)
      {
        return null;
      }

      var allUsers = new List<IUser>();
      allUsers.Add(user);

      DoSetChannelStates(cards, channels, allUsers, tvController);

      if (allUsers.Count > 0)
      {
        return allUsers[0].ChannelStates;
      }
      return new Dictionary<int, ChannelState>();
    }

    #endregion
  }
}