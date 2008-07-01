/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using DirectShowLib.SBE;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvEngine;
using TvDatabase;
using TvEngine.Events;

namespace TvService
{
  public class SimpleCardAllocation : ICardAllocation
  {
    #region ICardAllocation Members


    /// <summary>
    /// Gets a list of all channel states    
    /// </summary>    
    /// <returns>dictionary containing all channel states of the channels supplied</returns>
    public Dictionary<int, ChannelState> GetChannelStates(Dictionary<int, ITvCardHandler> cards, List<Channel> channels, ref User user, bool checkTransponders)
    {
      if (channels == null)
      {
        return null;
      }

      //construct list of all cards we can use to tune to the new channel
      List<CardDetail> cardsAvailable = new List<CardDetail>();
      Log.Info("Controller: GetChannelStates for channels{0}", channels.Count);

      TvBusinessLayer layer = new TvBusinessLayer();
      Dictionary<int, ChannelState> channelStates = new Dictionary<int, ChannelState>();

      foreach (Channel ch in channels)
      {
        bool chTunable = false;
        //get the tuning details for the channel
        List<IChannel> tuningDetails = layer.GetTuningChannelByName(ch);


        if (tuningDetails == null || tuningDetails.Count == 0)
        {
          //no tuning details??
          Log.Info("Controller:  No tuning details for channel:{0}", ch.Name);
          channelStates.Add(ch.IdChannel, ChannelState.nottunable);
        }
        else
        {      
          int number = 0;

          foreach (IChannel tuningDetail in tuningDetails)
          {
            number++;
            Log.Info("Controller:   channel #{0} {1} ", number, tuningDetail.ToString());
            Dictionary<int, ITvCardHandler>.Enumerator enumerator = cards.GetEnumerator();

            while (enumerator.MoveNext() && !chTunable)
            {
              KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
              bool check = true;
              int cardId = keyPair.Value.DataBaseCard.IdCard;
              ITvCardHandler tvcard = cards[cardId];

              //get the card info
              foreach (CardDetail info in cardsAvailable)
              {
                if (info.Card.DevicePath == keyPair.Value.DataBaseCard.DevicePath)
                {
                  check = false;
                }
              }

              if (!check) continue;

              //check if card is enabled
              if (!keyPair.Value.DataBaseCard.Enabled)
              {
                //not enabled, so skip the card
                Log.Info("Controller:    card:{0} type:{1} is disabled", cardId, tvcard.Type);
                continue;
              }

              if (!tvcard.Tuner.CanTune(tuningDetail))
              {
                //card cannot tune to this channel, so skip it
                Log.Info("Controller:    card:{0} type:{1} cannot tune to channel", cardId, tvcard.Type);
                continue;
              }

              //check if channel is mapped to this card and that the mapping is not for "Epg Only"
              ChannelMap channelMap = null;
              foreach (ChannelMap map in ch.ReferringChannelMap())
              {
                if (map.ReferencedCard().DevicePath == keyPair.Value.DataBaseCard.DevicePath && !map.EpgOnly)
                {
                  //yes
                  channelMap = map;
                  break;
                }
              }
              if (null == channelMap)
              {
                //channel is not mapped to this card, so skip it
                Log.Info("Controller:    card:{0} type:{1} channel not mapped", cardId, tvcard.Type);
                continue;
              }

              //ok card could be used to tune to this channel
              //now we check if its free...                            

              if (tvcard.Tuner.IsTunedToTransponder(tuningDetail) && (tvcard.SupportsSubChannels || (checkTransponders == false)))
              {
                //card is in use, but it is tuned to the same transponder.
                //meaning.. we can use it.
                //but we must check if cam can decode the extra channel as well

                //first check if cam is already decrypting this channel
                int camDecrypting = tvcard.NumberOfChannelsDecrypting;

                bool checkCam = true;
                User[] currentUsers = tvcard.Users.GetUsers();
                if (currentUsers != null)
                {
                  for (int i = 0; i < currentUsers.Length; ++i)
                  {
                    User tmpUser = currentUsers[i];
                    if (tvcard.CurrentDbChannel(ref tmpUser) == ch.IdChannel)
                    {
                      //yes, cam already is descrambling this channel
                      checkCam = false;
                      break;
                    }
                  }
                }

                //if the user is already using this card
                //and is watching a scrambled signal
                //then we must the CAM will always be able to watch the requested channel
                //since the users zaps
                if (tvcard.TimeShifter.IsTimeShifting(ref user))
                {
                  Channel current = Channel.Retrieve(tvcard.CurrentDbChannel(ref user));
                  if (current != null)
                  {
                    if (current.FreeToAir == false)
                    {
                      camDecrypting--;
                    }
                  }
                }

                //check if cam is capable of descrambling an extra channel
                bool canDecrypt = true;
                int dbDecryptLimit = keyPair.Value.DataBaseCard.DecryptLimit;
                if (dbDecryptLimit > 0)
                {
                  canDecrypt = (camDecrypting < dbDecryptLimit);
                }
                if (canDecrypt || ch.FreeToAir || (checkCam == false))
                {
                  //it is.. we can really use this card
                  Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels",
                      cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, keyPair.Value.DataBaseCard.DecryptLimit);                  
                }
                else
                {
                  //it is not, skip this card
                  Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels. cam limit reached",
                         cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, keyPair.Value.DataBaseCard.DecryptLimit);

                  //allow admin users like the scheduler to use this card anyway
                  if (user.IsAdmin)
                  {
                    //allow admin users like the scheduler to use this card anyway
                  }
                  else
                  {
                    continue;
                  }
                }
              }
              else
              {
                //different transponder, are we the owner of this card?
                if (false == tvcard.Users.IsOwner(user))
                {
                  //no
                  Log.Info("Controller:    card:{0} type:{1} is tuned to different transponder", cardId, tvcard.Type);
                  if (user.IsAdmin)
                  {
                    //allow admin users like the scheduler to use this card anyway
                  }
                  else
                  {
                    continue;
                  }
                }
              }

              bool isRec = false;
              bool isTS = false;

              User[] users = tvcard.Users.GetUsers();
              if (users == null || users.Length > 0)
              {
                for (int i = 0; i < users.Length; ++i)
                {
                  User userAny = users[i];
                  if (tvcard.CurrentChannelName(ref userAny) == null) continue;
                  if (tvcard.CurrentChannelName(ref userAny) != ch.Name) continue;

                  if (tvcard.Recorder.IsRecording(ref userAny) && !isRec)
                  {
                    isRec = true;
                    break;
                  }

                  if (tvcard.TimeShifter.IsTimeShifting(ref userAny) && !isTS)                  
                  {
                    isTS = true;                    
                  }
                }
              }

              if (isRec) //recording
              {
                channelStates.Add(ch.IdChannel, ChannelState.recording);
              }

              else
              {                
                if (isTS) //timeshifting
                {
                  channelStates.Add(ch.IdChannel, ChannelState.timeshifting);
                }
                else //just available
                {
                  channelStates.Add(ch.IdChannel, ChannelState.tunable);
                }
              }

              chTunable = true;
            } //while end

            if (chTunable) break;

          } //foreach end

          if (!chTunable)
          {
            channelStates.Add(ch.IdChannel, ChannelState.nottunable);
          }
        }
      }

      return channelStates;
    }


    /// <summary>
    /// Gets a list of all free cards which can receive the channel specified
    /// List is sorted by priority
    /// </summary>
    /// <param name="channelName">Name of the channel.</param>
    /// <returns>list containg all free cards which can receive the channel</returns>
    public List<CardDetail> GetAvailableCardsForChannel(Dictionary<int, ITvCardHandler> cards, Channel dbChannel, ref User user, bool checkTransponders, out TvResult result)
    {
      try
      {
        //construct list of all cards we can use to tune to the new channel
        List<CardDetail> cardsAvailable = new List<CardDetail>();

        TvBusinessLayer layer = new TvBusinessLayer();

        //get the tuning details for the channel
        List<IChannel> tuningDetails = layer.GetTuningChannelByName(dbChannel);


        int cardsFound = 0;
        int number = 0;
        //foreach tuning detail
        foreach (IChannel tuningDetail in tuningDetails)
        {
          number++;
          Log.Info("Controller:   channel #{0} {1} ", number, tuningDetail.ToString());
          Dictionary<int, ITvCardHandler>.Enumerator enumerator = cards.GetEnumerator();

          //for each card...
          while (enumerator.MoveNext())
          {
            KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;

            int cardId = keyPair.Value.DataBaseCard.IdCard;
            ITvCardHandler tvcard = cards[cardId];

            //check if card is enabled
            if (keyPair.Value.DataBaseCard.Enabled == false)
            {
              //not enabled, so skip the card
              Log.Info("Controller:    card:{0} type:{1} is disabled", cardId, tvcard.Type);
              continue;
            }

						try
						{
							RemoteControl.HostName = keyPair.Value.DataBaseCard.ReferencedServer().HostName;
							if (!RemoteControl.Instance.CardPresent(cardId))
							{
								//not found, so skip the card
								Log.Info("Controller:    card:{0} type:{1} is not present", cardId, tvcard.Type);
								continue;
							}
						}
						catch (Exception)
						{
							Log.Error("card: unable to connect to slave controller at:{0}", keyPair.Value.DataBaseCard.ReferencedServer().HostName);
							continue;
						}

            if (tvcard.Tuner.CanTune(tuningDetail) == false)
            {
              //card cannot tune to this channel, so skip it
              Log.Info("Controller:    card:{0} type:{1} cannot tune to channel", cardId, tvcard.Type);
              continue;
            }

            //check if channel is mapped to this card and that the mapping is not for "Epg Only"
            ChannelMap channelMap = null;
            foreach (ChannelMap map in dbChannel.ReferringChannelMap())
            {
                if (map.ReferencedCard().DevicePath == keyPair.Value.DataBaseCard.DevicePath && !map.EpgOnly)
              {
                //yes
                channelMap = map;
                break;
              }
            }
            if (null == channelMap)
            {
              //channel is not mapped to this card, so skip it
              Log.Info("Controller:    card:{0} type:{1} channel not mapped", cardId, tvcard.Type);
              continue;
            }

            //ok card could be used to tune to this channel
            //now we check if its free...
            cardsFound++;
						
						if (tvcard.Tuner.IsTunedToTransponder(tuningDetail) && (tvcard.SupportsSubChannels || (checkTransponders == false)))						
            {
              //card is in use, but it is tuned to the same transponder.
              //meaning.. we can use it.
              //but we must check if cam can decode the extra channel as well

              //first check if cam is already decrypting this channel
              int camDecrypting = tvcard.NumberOfChannelsDecrypting;
              bool checkCam = true;
              User[] currentUsers = tvcard.Users.GetUsers();
              if (currentUsers != null)
              {
                for (int i = 0; i < currentUsers.Length; ++i)
                {
                  User tmpUser = currentUsers[i];
                  if (tvcard.CurrentDbChannel(ref tmpUser) == dbChannel.IdChannel)
                  {
                    //yes, cam already is descrambling this channel
                    checkCam = false;
                    break;
                  }
                }
              }

              //if the user is already using this card
              //and is watching a scrambled signal
              //then we must the CAM will always be able to watch the requested channel
              //since the users zaps
              if (tvcard.TimeShifter.IsTimeShifting(ref user))
              {
                Channel current = Channel.Retrieve(tvcard.CurrentDbChannel(ref user));
                if (current != null)
                {
                  if (current.FreeToAir == false)
                  {
                    camDecrypting--;
                  }
                }
              }

              //check if cam is capable of descrambling an extra channel
              bool canDecrypt = true;
              int dbDecryptLimit = keyPair.Value.DataBaseCard.DecryptLimit;
              if (dbDecryptLimit > 0)
              {
                canDecrypt = (camDecrypting < dbDecryptLimit);
              }
              if (canDecrypt || dbChannel.FreeToAir || (checkCam == false))
              {
                //it is.. we can really use this card
                Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels",
                    cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, keyPair.Value.DataBaseCard.DecryptLimit);
              }
              else
              {
                //it is not, skip this card
                Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels. cam limit reached",
                       cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, keyPair.Value.DataBaseCard.DecryptLimit);

                //allow admin users like the scheduler to use this card anyway
                if (user.IsAdmin)
                {
                  //allow admin users like the scheduler to use this card anyway
                }
                else
                {
                  continue;
                }
              }
            }
            else
            {
              //different transponder, are we the owner of this card?
              if (false == tvcard.Users.IsOwner(user))
              {
                //no
                Log.Info("Controller:    card:{0} type:{1} is tuned to different transponder", cardId, tvcard.Type);
                if (user.IsAdmin)
                {
                  //allow admin users like the scheduler to use this card anyway
                }
                else
                {
                  continue;
                }
              }
            }
            CardDetail cardInfo = new CardDetail(cardId, channelMap.ReferencedCard(), tuningDetail);

           

            cardsAvailable.Add(cardInfo);
          }
        }


        if (cardsAvailable.Count > 0)
        {
          result = TvResult.Succeeded;
        }
        else
        {
          if (cardsFound == 0)
            result = TvResult.ChannelNotMappedToAnyCard;
          else
            result = TvResult.AllCardsBusy;
        }
        Log.Info("Controller: found {0} available", cardsAvailable.Count);

        return cardsAvailable;
      }
      catch (Exception ex)
      {
        result = TvResult.UnknownError;
        Log.Write(ex);
        return null;
      }
    }

    #endregion
  }
}
