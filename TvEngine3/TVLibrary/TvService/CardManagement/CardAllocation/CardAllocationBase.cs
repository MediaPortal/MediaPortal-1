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

using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvDatabase;
using TvControl;

using System;
using System.Collections.Generic;

namespace TvService
{
  public class CardAllocationBase
  {
    #region protected members

    protected static bool IsCamAbleToDecrypChannel(User user, ITvCardHandler tvcard, Channel ch, int decryptLimit, out bool isRec)
    {
      bool IsCamAbleToDecrypChannel = false;
      int camDecrypting = tvcard.NumberOfChannelsDecrypting;
      Channel currentUserCh = Channel.Retrieve(user.IdChannel);
      isRec = false;
      if (currentUserCh != null)
      {
        isRec = tvcard.Recorder.IsRecordingChannel(currentUserCh.Name);
      }
      if (tvcard.TimeShifter.IsTimeShifting(ref user) && !isRec)
      {
        bool fta = isFTA(tvcard, user);
        if (fta)
        {
          camDecrypting--;
        }
      }
      //check if cam is capable of descrambling an extra channel
      if (decryptLimit > 0)
      {
        IsCamAbleToDecrypChannel = (camDecrypting < decryptLimit);
      }

      return (IsCamAbleToDecrypChannel || ch.FreeToAir);                    
    }

    protected static bool IsCamAlreadyDecodingChannel(ITvCardHandler tvcard, Channel dbChannel)
    {
      bool isCamAlreadyDecodingChannel = false;
      User[] currentUsers = tvcard.Users.GetUsers();
      if (currentUsers != null)
      {
        for (int i = 0; i < currentUsers.Length; ++i)
        {
          User tmpUser = currentUsers[i];
          if (tvcard.CurrentDbChannel(ref tmpUser) == dbChannel.IdChannel)
          {
            //yes, cam already is descrambling this channel
            isCamAlreadyDecodingChannel = true;
            break;
          }
        }
      }
      return isCamAlreadyDecodingChannel;
    }

    protected static bool isFTA(ITvCardHandler tvcard, User user)
    {
      IChannel unknownChannel = tvcard.CurrentChannel(ref user);

      bool fta = true;

      if (unknownChannel != null)
      {
        if (unknownChannel is DVBCChannel)
        {
          fta = ((DVBCChannel)unknownChannel).FreeToAir;
        }
        else if (unknownChannel is DVBSChannel)
        {
          fta = ((DVBSChannel)unknownChannel).FreeToAir;
        }
        else if (unknownChannel is DVBTChannel)
        {
          fta = ((DVBTChannel)unknownChannel).FreeToAir;
        }
        else if (unknownChannel is ATSCChannel)
        {
          fta = ((ATSCChannel)unknownChannel).FreeToAir;
        }
        else if (unknownChannel is DVBIPChannel)
        {
          fta = ((DVBIPChannel)unknownChannel).FreeToAir;
        }
      }
      return fta;
    }     

    protected static bool IsChannelMappedToCard(Channel dbChannel, KeyValuePair<int, ITvCardHandler> keyPair, out ChannelMap channelMap)
    {
      //check if channel is mapped to this card and that the mapping is not for "Epg Only"
      bool isChannelMappedToCard = false;
      channelMap = null;
      foreach (ChannelMap map in dbChannel.ReferringChannelMap())
      {
        if (map.ReferencedCard().DevicePath == keyPair.Value.DataBaseCard.DevicePath && !map.EpgOnly)
        {
          //yes
          channelMap = map;

          if (null != channelMap)
          {
            //channel is not mapped to this card, so skip it            
            isChannelMappedToCard = true;
          }

          break;
        }
      }
      return isChannelMappedToCard;
    }

    protected static bool IsValidTuningDetails(List<IChannel> tuningDetails)
    {
      bool isValid = true;
      if (tuningDetails == null)
      {
        isValid = false;
      }
      else if (tuningDetails.Count == 0)
      {
        isValid = false;
      }

      return isValid;
    }


    #endregion

  }
}
