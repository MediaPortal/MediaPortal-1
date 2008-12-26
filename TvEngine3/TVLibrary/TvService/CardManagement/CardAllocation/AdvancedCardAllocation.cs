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
using System.Collections.Generic;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;
using TvDatabase;

namespace TvService
{
  public class AdvancedCardAllocation : CardAllocationBase, ICardAllocation
  {
    #region private members
    #endregion

    #region ICardAllocation Members
    /// <summary>
    /// Gets a list of all free cards which can receive the channel specified
    /// List is sorted by priority
    /// </summary>
    /// <returns>list containg all free cards which can receive the channel</returns>
    public List<CardDetail> GetAvailableCardsForChannel(Dictionary<int, ITvCardHandler> cards, Channel dbChannel, ref User user, bool checkTransponders, out TvResult result, int recommendedCardId)
    {
      try
      {
        //construct list of all cards we can use to tune to the new channel
        List<CardDetail> cardsAvailable = new List<CardDetail>();

        Log.Info("Controller: find free card for channel {0}", dbChannel.Name);
        TvBusinessLayer layer = new TvBusinessLayer();

        //get the tuning details for the channel
        List<IChannel> tuningDetails = layer.GetTuningChannelByName(dbChannel);


        if (tuningDetails == null)
        {
          //no tuning details??
          Log.Info("Controller:  No tuning details for channel:{0}", dbChannel.Name);
          result = TvResult.NoTuningDetails;
          return cardsAvailable;
        }

        if (tuningDetails.Count == 0)
        {
          //no tuning details??
          Log.Info("Controller:  No tuning details for channel:{0}", dbChannel.Name);
          result = TvResult.NoTuningDetails;
          return cardsAvailable;
        }
        Log.Info("Controller:   got {0} tuning details for {1}", tuningDetails.Count, dbChannel.Name);


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
            if (check == false)
              continue;


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
            } catch (Exception)
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
            bool sameTransponder = false;
            bool canDecrypt = true;

            if (tvcard.Tuner.IsTunedToTransponder(tuningDetail) && (tvcard.SupportsSubChannels || (checkTransponders == false)))
            {
              //card is in use, but it is tuned to the same transponder.
              //meaning.. we can use it.
              if (tvcard.HasCA && keyPair.Value.DataBaseCard.DecryptLimit > 0) //does the card have a CA module and a CA limit, if yes then proceed to check cam decrypt limit.
              {
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

                //bool isRec = tvcard.Recorder.IsAnySubChannelRecording;                                
                //bool isRec = tvcard.Recorder.IsRecording(ref user); // IsRecordingChannel(user.);                                
                bool isRec = false;

                Channel currentUserCh = Channel.Retrieve(user.IdChannel);
                if (currentUserCh != null)
                {
                  isRec = tvcard.Recorder.IsRecordingChannel(currentUserCh.Name);
                }

                if (tvcard.TimeShifter.IsTimeShifting(ref user) && !isRec)
                {
                  bool fta = isFTA(tvcard, user);
                  if (!fta)
                  {
                    camDecrypting--;
                  }
                }

                //check if cam is capable of descrambling an extra channel                
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
                  sameTransponder = true;
                }
                else
                {
                  //it is not, skip this card
                  Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder decrypting {2}/{3} channels. cam limit reached",
                         cardId, tvcard.Type, tvcard.NumberOfChannelsDecrypting, keyPair.Value.DataBaseCard.DecryptLimit);


                  //allow admin users like the scheduler to use this card anyway                  
                  if (user.IsAdmin)
                  {
                    // lets find out what is going on on this transponder
                    // if just one channel is recording, then we dont want to interrupt it.
                    //bool isRec = tvcard.Recorder.IsAnySubChannelRecording;

                    if (isRec)
                    {
                      // we are already doing stuff on this transponder, skip it.
                      continue;
                    }
                  }
                  else
                  {
                    continue;
                  }
                }
              } //end of cam present block              
              else // no cam present
              {
                Log.Info("Controller:    card:{0} type:{1} is tuned to same transponder no CA present", cardId, tvcard.Type);
                sameTransponder = true;
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


            //determine how many other users are using this card
            int nrOfOtherUsers = 0;
            User[] users = cards[cardInfo.Id].Users.GetUsers();
            if (users != null)
            {
              for (int i = 0; i < users.Length; ++i)
              {
                if (users[i].Name != user.Name)
                  nrOfOtherUsers++;
              }
            }

            //if there are other users on this card and we want to switch to another transponder
            //then set this cards priority as very low...
            if (nrOfOtherUsers > 0 && !sameTransponder)
            {
              cardInfo.Priority -= 100;
            }




            // handle recommended cardid.
            // boost priority if a cardid matches, but only if these criterias are met.
            // A) if card is free while other cards are busy.
            // B) if card is busy (but decryption slot available) while other cards are busy or free.

            if (recommendedCardId == cardId)
            {
              if (nrOfOtherUsers == 0 || canDecrypt)
              {
                cardInfo.Priority += 100;
              }
            }

            Log.Info("Controller:    card:{0} type:{1} is available priority:{2} #users:{3} same transponder:{4}",
                          cardInfo.Id, tvcard.Type, cardInfo.Priority, nrOfOtherUsers, sameTransponder);


            cardsAvailable.Add(cardInfo);
          }
        }


        //sort cards on priority
        cardsAvailable.Sort();


        if (cardsAvailable.Count > 0)
        {
          result = TvResult.Succeeded;
        }
        else
        {
          result = cardsFound == 0 ? TvResult.ChannelNotMappedToAnyCard : TvResult.AllCardsBusy;
        }
        Log.Info("Controller: found {0} available", cardsAvailable.Count);

        return cardsAvailable;
      } catch (Exception ex)
      {
        result = TvResult.UnknownError;
        Log.Write(ex);
        return null;
      }
    }

    #endregion
  }
}
