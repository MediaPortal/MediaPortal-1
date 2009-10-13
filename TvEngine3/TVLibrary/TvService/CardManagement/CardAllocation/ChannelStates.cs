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
using System.Collections.Generic;
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

    private static void UpdateChannelStateUserBasedOnCardOwnership(ITvCardHandler tvcard, IList<User> allUsers, Channel ch)
    {
      for (int i = 0; i < allUsers.Count; i++)
      {
        User user = allUsers[i];
        if (!tvcard.Users.IsOwner(user))
        {
          //no
          //Log.Info("Controller:    card:{0} type:{1} is tuned to different transponder", cardId, tvcard.Type);
          if (user.IsAdmin)
          {
            //allow admin users like the scheduler to use this card anyway
          }
          else
          {
            UpdateChannelStateUser(user, ChannelState.nottunable, ch.IdChannel);
            allUsers[i] = user;
          }
        }
        else
        {
          UpdateChannelStateUser(user, ChannelState.tunable, ch.IdChannel);
          allUsers[i] = user;
        }
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
        catch (NullReferenceException) { }

        if (u == null)
          continue;
        if (u.IsAdmin)
          continue; //scheduler users do not need to have their channelstates set.
        ChannelState currentChState;
        bool stateExists = u.ChannelStates.TryGetValue(channelId, out currentChState);

        if (stateExists && currentChState == chState)
          continue;
        if (currentChState != ChannelState.nottunable && chState == ChannelState.nottunable)
          continue;

        try
        {
          if (stateExists && currentChState != chState) //&& replace
          {
            u.ChannelStates.Remove(channelId);
            stateExists = false;
          }
          if (!stateExists)
          {
            u.ChannelStates.Add(channelId, chState);
          }
        }
        catch (NullReferenceException) { }

      }
    }

    private static void UpdateChannelStateUser(User user, ChannelState chState, int channelId)
    {
      ChannelState currentChState;
      bool stateExists = user.ChannelStates.TryGetValue(channelId, out currentChState);

      if (stateExists && currentChState == chState)
        return;
      if (currentChState != ChannelState.nottunable)
        return;

      if (stateExists && currentChState == ChannelState.nottunable)
      {
        user.ChannelStates.Remove(channelId);
        stateExists = false;
      }
      if (!stateExists)
      {
        user.ChannelStates.Add(channelId, chState);
      }
    }

    private static IList<User> GetActiveUsers(Dictionary<int, ITvCardHandler> cards)
    {
      // find all users
      IList<User> allUsers = new List<User>();

      try
      {
        Dictionary<int, ITvCardHandler>.Enumerator enumerator = cards.GetEnumerator();

        while (enumerator.MoveNext())
        {
          KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
          //get a list of all users for this card
          User[] usersAvail = keyPair.Value.Users.GetUsers();
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
        Log.Error("SimpleCardAllocation: Possible race condition occured when getting users - {0}", tex.StackTrace);
      }

      return allUsers;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void DoSetChannelStates(Dictionary<int, ITvCardHandler> cards, ICollection<Channel> channels, bool checkTransponders, IList<User> allUsers, TVController tvController)
    {
      try
      {
        //construct list of all cards we can use to tune to the new channel
        Log.Debug("Controller: DoSetChannelStates for {0} channels", channels.Count);

        Dictionary<int, ITvCardHandler>.Enumerator enumerator;

        // find all users      
        //IList<User> allUsers = GetActiveUsers(cards);
        if (allUsers == null || allUsers.Count == 0)
        {
          return; // no users, no point in continuing.
        }        

        TvBusinessLayer layer = new TvBusinessLayer();

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
            //Log.Info("Controller:   channel #{0} {1} ", number, tuningDetail.ToString());
            enumerator = cards.GetEnumerator();

            while (enumerator.MoveNext())
            {
              KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;

              int cardId = keyPair.Value.DataBaseCard.IdCard;
              ITvCardHandler tvcard = cards[cardId];

              //check if card is enabled
              if (!keyPair.Value.DataBaseCard.Enabled)
              {
                //not enabled, so skip the card
                //Log.Info("Controller:    card:{0} type:{1} is disabled", cardId, tvcard.Type);
                UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                continue;
              }

              if (!tvcard.Tuner.CanTune(tuningDetail))
              {
                //card cannot tune to this channel, so skip it
                //Log.Info("Controller:    card:{0} type:{1} cannot tune to channel", cardId, tvcard.Type);
                UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                continue;
              }

              //check if channel is mapped to this card and that the mapping is not for "Epg Only"
              ChannelMap channelMap = null;
              bool isChannelMappedToCard = IsChannelMappedToCard(ch, keyPair, out channelMap);
              if (!isChannelMappedToCard)
              {
                UpdateChannelStateUsers(allUsers, ChannelState.nottunable, ch.IdChannel);
                continue;
              }

              //ok card could be used to tune to this channel
              //now we check if its free...                              
              int decryptLimit = keyPair.Value.DataBaseCard.DecryptLimit;
              CheckTransponderAllUsers(ch, allUsers, tvcard, decryptLimit, cardId, tuningDetail, checkTransponders);

              UpdateRecOrTSChannelStateForUsers(ch, allUsers, tvController);            
            } //while card end
          } //foreach tuningdetail end      
        }
      }
      catch (InvalidOperationException tex)
      {
        Log.Error("SimpleCardAllocation: Possible race condition occured setting channel states - {0}", tex.StackTrace);
      }
      catch (Exception ex)
      {
        Log.Error("SimpleCardAllocation: An unknown error occured while setting channel states - {0}\n{1}", ex.Message, ex.StackTrace);
      }
    }

    private static void UpdateRecOrTSChannelStateForUsers(Channel ch, IList<User>  allUsers, TVController tvController)
    {
        Dictionary<int, ChannelState> TSandRecStates = tvController.GetAllTimeshiftingAndRecordingChannels();
        if (TSandRecStates.ContainsKey(ch.IdChannel))
        {
          ChannelState cs = TSandRecStates[ch.IdChannel];

          if (cs == ChannelState.recording)
          {
            UpdateChannelStateUsers(allUsers, ChannelState.recording, ch.IdChannel);
          }
          else if (cs == ChannelState.timeshifting)
          {
            UpdateChannelStateUsers(allUsers, ChannelState.timeshifting, ch.IdChannel);
          }
        }
    }

    private static void CheckTransponderAllUsers(Channel ch, IList<User> allUsers, ITvCardHandler tvcard, int decryptLimit, int cardId, IChannel tuningDetail, bool checkTransponders)
    {           
      if (tvcard.Tuner.IsTunedToTransponder(tuningDetail) && (tvcard.SupportsSubChannels || (checkTransponders == false)))
      {
        //card is in use, but it is tuned to the same transponder.
        //meaning.. we can use it.
        if (tvcard.HasCA && decryptLimit > 0) //does the card have a CA module and a CA limit, if yes then proceed to check cam decrypt limit.                
        {
          //but we must check if cam can decode the extra channel as well
          //first check if cam is already decrypting this channel          
          bool isCamAlreadyDecodingChannel = (!IsCamAlreadyDecodingChannel(tvcard, ch));          

          //if the user is already using this card
          //and is watching a scrambled signal
          //then we must the CAM will always be able to watch the requested channel
          //since the users zaps

          int camDecrypting = tvcard.NumberOfChannelsDecrypting;
          for (int i = 0; i < allUsers.Count; i++)
          {
            User user = allUsers[i];
            
            //check if cam is capable of descrambling an extra channel
            bool isRec = false;
            bool isCamAbleToDecrypChannel = IsCamAbleToDecrypChannel(user, tvcard, ch, decryptLimit, out isRec);


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

              //allow admin users like the scheduler to use this card anyway
              if (user.IsAdmin)
              {
                //allow admin users like the scheduler to use this card anyway
              }
              else
              {
                user = allUsers[i];
                UpdateChannelStateUser(user, ChannelState.nottunable, ch.IdChannel);
                allUsers[i] = user;
                //continue;                      
              }
            }
          } //foreach allusers end
          //continue;                      
        } //end of cam present block              
        else // no cam present
        {
          UpdateChannelStateUsers(allUsers, ChannelState.tunable, ch.IdChannel);
        }
      }
      else
      {
        //different transponder, are we the owner of this card?
        UpdateChannelStateUserBasedOnCardOwnership(tvcard, allUsers, ch);        
      }

    }           



    #endregion    
    
    #region public members

    public void SetChannelStates(Dictionary<int, ITvCardHandler> cards, IList<Channel> channels, bool checkTransponders, TVController tvController)
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
    public Dictionary<int, ChannelState> GetChannelStates(Dictionary<int, ITvCardHandler> cards, IList<Channel> channels, ref User user, bool checkTransponders, TVController tvController)
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
