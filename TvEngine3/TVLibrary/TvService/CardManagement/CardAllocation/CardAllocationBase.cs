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

#region usings 

using System.Collections.Generic;
using TvControl;
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Interfaces;

#endregion

namespace TvService
{
  public class CardAllocationBase
  {
    #region protected members

    protected static bool IsOnlyActiveUserCurrentUser(Dictionary<int, ITvCardHandler> cards, User currentUser)
    {      
      bool isOnlyActiveUserCurrentUser = true;

      Dictionary<int, ITvCardHandler>.ValueCollection cardHandlers = cards.Values;                    
      foreach (ITvCardHandler cardHandler in cardHandlers)
      {
        User[] users = cardHandler.Users.GetUsers();

        if (users != null && users.Length > 0)
        {
          foreach (User u in users)
          {
            if (u.Name != currentUser.Name)
            {
              isOnlyActiveUserCurrentUser = false;
              break;
            }
          }
        }
        if (!isOnlyActiveUserCurrentUser)
        {
          break;
        }
      }
      return isOnlyActiveUserCurrentUser;
    }

    protected static bool IsCamAbleToDecrypChannel(User user, ITvCardHandler tvcard, Channel ch, int decryptLimit,
                                                   out bool isRec)
    {
      bool IsCamAbleToDecrypChannel = false;
      int camDecrypting = tvcard.NumberOfChannelsDecrypting;
      Channel currentUserCh = Channel.Retrieve(user.IdChannel);
      isRec = false;
      if (currentUserCh != null)
      {
        isRec = tvcard.Recorder.IsRecordingChannel(currentUserCh.Name);
      }
      if (!isRec && tvcard.TimeShifter.IsTimeShifting(ref user))
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
        DVBBaseChannel dvbChannel = unknownChannel as DVBBaseChannel;

        if (dvbChannel != null)
        {
          fta = dvbChannel.FreeToAir;
        }        
      }
      return fta;
    }

    protected static bool IsChannelMappedToCard(Channel dbChannel, string devicePath,
                                                out ChannelMap channelMap)
    {
      //check if channel is mapped to this card and that the mapping is not for "Epg Only"
      bool isChannelMappedToCard = false;
      channelMap = null;

      List<ChannelMap> channelMaps = dbChannel.ReferringChannelMap() as List<ChannelMap>;

      if (channelMaps != null)
      {
        channelMap = channelMaps.Find(m => !m.EpgOnly && m.ReferencedCard().DevicePath == devicePath);
        isChannelMappedToCard = (channelMap != null);
      }      
     
      return isChannelMappedToCard;
    }

    protected static bool IsValidTuningDetails(List<IChannel> tuningDetails)
    {
      bool isValid = (tuningDetails != null && tuningDetails.Count > 0);
      return isValid;
    }

    #endregion
  }
}