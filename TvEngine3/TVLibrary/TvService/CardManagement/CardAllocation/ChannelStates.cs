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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;

namespace TvService
{
  public class ChannelStates : CardAllocationBase
  {
    #region private members   

    private static void UpdateChannelStateUserBasedOnCardOwnership(ITvCardHandler tvcard, IList<User> allUsers,
                                                                   Channel ch)
    {
      for (int i = 0; i < allUsers.Count; i++)
      {
        User user = allUsers[i];
        if (user.IsAdmin)
        {
          continue;
        }
        if (!tvcard.Users.IsOwner(user))
        {
          //no
          //Log.Info("Controller:    card:{0} type:{1} is tuned to different transponder", cardId, tvcard.Type);
          //allow admin users like the scheduler to use this card anyway          
          UpdateChannelStateUser(user, ChannelState.nottunable, ch.IdChannel);          
        }
        else
        {
          UpdateChannelStateUser(user, ChannelState.tunable, ch.IdChannel);          
        }
        allUsers[i] = user;
      }
    }

    private static void RemoveChannelStates(User user, int channelId)
    {
      ChannelState currentChState = ChannelState.tunable;
      user.ChannelStates.TryGetValue(channelId, out currentChState);
             
      if (currentChState == ChannelState.nottunable)
      {
        user.ChannelStates.Remove(channelId);  
      }                      
    }

    private static void UpdateChannelStateUsers(IList<User> allUsers, ChannelState chState, int channelId)
    {
      for (int i = 0; i < allUsers.Count; i++)
      {
        User u = null;
        try
        {
          u = allUsers[i];
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

    private static void UpdateChannelStateUser(User user, ChannelState chState, int channelId)
    {
      ChannelState currentChState;
      bool stateExists = user.ChannelStates.TryGetValue(channelId, out currentChState);

      if (stateExists && currentChState == chState)
        return;

      if (currentChState != ChannelState.nottunable && chState == ChannelState.nottunable)
        return;

      if (currentChState == ChannelState.recording && chState != ChannelState.nottunable)
        return;

      user.ChannelStates[channelId] = chState; //add key if does not exist, or update existing one.          
    }

    private static IList<User> GetActiveUsers(Dictionary<int, ITvCardHandler> cards)
    {
      // find all users
      IList<User> allUsers = new List<User>();

      try
      {
        Dictionary<int, ITvCardHandler>.ValueCollection cardHandlers = cards.Values;
        foreach (ITvCardHandler cardHandler in cardHandlers)
        {          
          //get a list of all users for this card
          User[] usersAvail = cardHandler.Users.GetUsers();
          if (usersAvail != null)
          {
            //for each user
            for (int i = 0; i < usersAvail.Length; ++i)
            {
              User tmpUser = usersAvail[i];
              if (!tmpUser.IsAdmin)
              {
                tmpUser.ChannelStates = new Dictionary<int, ChannelState>();
                allUsers.Add(tmpUser);
              }
            }
          }
        }
      }
      catch (InvalidOperationException tex)
      {
        Log.Error("ChannelState: Possible race condition occured when getting users - {0}", tex.StackTrace);
      }

      return allUsers;
    }   

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void DoSetChannelStates(Dictionary<int, ITvCardHandler> cards, ICollection<Channel> channels,
                                           bool checkTransponders, IList<User> allUsers, TVController tvController)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      try
      {
        //construct list of all cards we can use to tune to the new channel
        Log.Debug("Controller: DoSetChannelStates for {0} channels", channels.Count);

        Dictionary<int, ITvCardHandler>.Enumerator enumerator;

        if (allUsers == null || allUsers.Count == 0)
        {
          return; // no users, no point in continuing.
        }

        TvBusinessLayer layer = new TvBusinessLayer();

        Dictionary<int, ChannelState> TSandRecStates = null;
        List<int> clearChannelStates = new List<int>();

        Dictionary<int, ITvCardHandler>.ValueCollection cardHandlers = cards.Values;

        bool isOnlyActiveUserCurrentUser = IsOnlyActiveUserCurrentUser(cards, allUsers[0]);

        foreach (Channel ch in channels)
        {
          if (!ch.VisibleInGuide)
          {
            UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
            continue;
          }

          //get the tuning details for the channel
          List<IChannel> tuningDetails = layer.GetTuningChannelByName(ch);

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
              int cardId = cardHandler.DataBaseCard.IdCard;              

              //check if card is enabled
              if (!cardHandler.DataBaseCard.Enabled)
              {
                //not enabled, so skip the card
                //Log.Info("Controller:    card:{0} type:{1} is disabled", cardId, tvcard.Type);
                UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                continue;
              }

              if (!cardHandler.Tuner.CanTune(tuningDetail))
              {
                //card cannot tune to this channel, so skip it
                //Log.Info("Controller:    card:{0} type:{1} cannot tune to channel", cardId, tvcard.Type);
                UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                continue;
              }

              //check if channel is mapped to this card and that the mapping is not for "Epg Only"
              ChannelMap channelMap = null;
              bool isChannelMappedToCard = IsChannelMappedToCard(ch, cardHandler.DataBaseCard.DevicePath, out channelMap);
              if (!isChannelMappedToCard)
              {
                UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                continue;
              }

              if (!isOnlyActiveUserCurrentUser)
              {
                //ok card could be used to tune to this channel
                //now we check if its free...                              
                int decryptLimit = cardHandler.DataBaseCard.DecryptLimit;
                CheckTransponderAllUsers(ch, allUsers, cardHandler, decryptLimit, cardId, tuningDetail, checkTransponders);
              }
              else
              {
                //no need to do any further checks if user is the only one currently active.
                if (!clearChannelStates.Contains(ch.IdChannel))
                {
                  clearChannelStates.Add(ch.IdChannel);
                }                
              }                        
            } //while card end
          } //foreach tuningdetail end              

          //only query once
          if (TSandRecStates == null)
          {
            TSandRecStates = tvController.GetAllTimeshiftingAndRecordingChannels();
          }
          UpdateRecOrTSChannelStateForUsers(ch, allUsers, TSandRecStates);
        }
        foreach (int id in clearChannelStates)
        {
          RemoveChannelStates(allUsers[0], id);
        }
      }
      catch (InvalidOperationException tex)
      {
        Log.Error("ChannelState: Possible race condition occured setting channel states - {0}", tex.StackTrace);
      }
      catch (Exception ex)
      {
        Log.Error("ChannelState: An unknown error occured while setting channel states - {0}\n{1}", ex.Message,
                  ex.StackTrace);
      }
      finally
      {
        stopwatch.Stop();
        Log.Info("ChannelStates.DoSetChannelStates took {0} msec", stopwatch.ElapsedMilliseconds);
      }
    }    

    private static void UpdateRecOrTSChannelStateForUsers(Channel ch, IList<User> allUsers,
                                                          Dictionary<int, ChannelState> TSandRecStates)
    {
      ChannelState cs = ChannelState.tunable;
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

    private static void CheckTransponderAllUsers(Channel ch, IList<User> allUsers, ITvCardHandler tvcard,
                                                 int decryptLimit, int cardId, IChannel tuningDetail,
                                                 bool checkTransponders)
    {
      bool isSameTransponder = tvcard.Tuner.IsTunedToTransponder(tuningDetail) &&
                               (tvcard.SupportsSubChannels || (checkTransponders == false));
      if (isSameTransponder)
      {
        bool hasCA = tvcard.HasCA;
        int camDecrypting = tvcard.NumberOfChannelsDecrypting;
        for (int i = 0; i < allUsers.Count; i++)
        {
          User user = allUsers[i];

          //ignore admin users, like scheduler
          if (user.IsAdmin)
          {
            continue;
          }

          bool isOwnerOfCard = tvcard.Users.IsOwner(user);         
          if (!isOwnerOfCard)
          {
            if (hasCA && decryptLimit > 0)
              //does the card have a CA module and a CA limit, if yes then proceed to check cam decrypt limit.                
            {
              //but we must check if cam can decode the extra channel as well
              //first check if cam is already decrypting this channel          
              bool isCamAlreadyDecodingChannel = IsCamAlreadyDecodingChannel(tvcard, ch);

              //check if cam is capable of descrambling an extra channel              
              bool isCamAbleToDecrypChannel = IsCamAbleToDecrypChannel(user, tvcard, ch, decryptLimit);


              if (isCamAbleToDecrypChannel || isCamAlreadyDecodingChannel)
              {
                //it is.. we can really use this card
                //Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels",
                //    cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, keyPair.Value.DataBaseCard.DecryptLimit);
                user = allUsers[i];
                UpdateChannelStateUser(user, ChannelState.tunable, ch.IdChannel);
                allUsers[i] = user;
              }
              else
              {
                //it is not, skip this card
                //Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels. cam limit reached",
                //     cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, keyPair.Value.DataBaseCard.DecryptLimit);
                user = allUsers[i];
                if (tvcard.Users.IsOwner(user))
                {
                  UpdateChannelStateUser(user, ChannelState.tunable, ch.IdChannel);
                }
                else
                {
                  UpdateChannelStateUser(user, ChannelState.nottunable, ch.IdChannel);
                }
                allUsers[i] = user;
              }
            }
            else // no cam present
            {
              UpdateChannelStateUser(user, ChannelState.tunable, ch.IdChannel);
            }
          }
          else //in case of cardowner 
          {
            UpdateChannelStateUser(user, ChannelState.tunable, ch.IdChannel);
          }
        } //foreach allusers end                         
      }
      else
      {
        //different transponder, are we the owner of this card?
        UpdateChannelStateUserBasedOnCardOwnership(tvcard, allUsers, ch);
      }
    }

    #endregion

    #region public members

    public void SetChannelStates(Dictionary<int, ITvCardHandler> cards, IList<Channel> channels, bool checkTransponders,
                                 TVController tvController)
    {
      if (channels == null)
      {
        return;
      }

      //call the real work as a thread in order to avoid slower channel changes.
      // find all users      
      IList<User> allUsers = GetActiveUsers(cards);
      ThreadStart starter = delegate { DoSetChannelStates(cards, channels, checkTransponders, allUsers, tvController); };
      Thread setChannelStatesThread = new Thread(starter);
      setChannelStatesThread.Name = "Channel state thread";
      setChannelStatesThread.IsBackground = true;
      setChannelStatesThread.Priority = ThreadPriority.Lowest;
      setChannelStatesThread.Start();
    }


    /// <summary>
    /// Gets a list of all channel states    
    /// </summary>    
    /// <returns>dictionary containing all channel states of the channels supplied</returns>
    public Dictionary<int, ChannelState> GetChannelStates(Dictionary<int, ITvCardHandler> cards, IList<Channel> channels,
                                                          ref User user, bool checkTransponders,
                                                          TVController tvController)
    {
      if (channels == null)
      {
        return null;
      }

      List<User> allUsers = new List<User>();
      allUsers.Add(user);

      DoSetChannelStates(cards, channels, checkTransponders, allUsers, tvController);

      if (allUsers.Count > 0)
      {
        return allUsers[0].ChannelStates;
      }
      return new Dictionary<int, ChannelState>();
    }

    #endregion
  }
}