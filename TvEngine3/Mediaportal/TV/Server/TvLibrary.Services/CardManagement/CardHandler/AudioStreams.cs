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

using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public class AudioStreams : IAudioStreams
  {
    private readonly ITvCardHandler _cardHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisEqcManagement"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public AudioStreams(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }

    #region audio streams

    /// <summary>
    /// Gets the available audio streams.
    /// </summary>
    /// <value>The available audio streams.</value>
    public IAudioStream[] Streams(string userName, int idChannel)
    {
      var audioStreams = new List<IAudioStream>().ToArray();
      if (_cardHandler.DataBaseCard.enabled)
      {        
        int subChannelIdByChannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(userName, idChannel);
        if (subChannelIdByChannelId > -1)
        {
          ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subChannelIdByChannelId);
          if (subchannel != null)
          {
            audioStreams = subchannel.AvailableAudioStreams.ToArray();
          } 
        }
      }
                          
      return audioStreams;
    }

    /// <summary>
    /// Gets the current audio stream.
    /// </summary>
    /// <returns></returns>
    public IAudioStream GetCurrent(IUser user, int idChannel)
    {
      IAudioStream currentAudioStream = null;
      if (_cardHandler.DataBaseCard.enabled)
      {        
        _cardHandler.UserManagement.RefreshUser(ref user);
        int subChannelIdByChannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel);
        if (subChannelIdByChannelId > -1)
        {
          ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subChannelIdByChannelId);
          if (subchannel != null)
          {
            currentAudioStream = subchannel.CurrentAudioStream;
          }         
        }        
      }
      return currentAudioStream;
    }

    /// <summary>
    /// Sets the current audio stream.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="stream">The stream.</param>
    /// <param name="idChannel"> </param>
    public void Set(IUser user, IAudioStream stream, int idChannel)
    {
      if (_cardHandler.DataBaseCard.enabled)
      {
        _cardHandler.UserManagement.RefreshUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(_cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel));
        if (subchannel != null)
        {
          subchannel.CurrentAudioStream = stream;          
        }
        else
        {
          Log.WriteFile("card: SetCurrentAudioStream: ITvSubChannel == null");
        }        
      }        
    }

    #endregion
  }
}