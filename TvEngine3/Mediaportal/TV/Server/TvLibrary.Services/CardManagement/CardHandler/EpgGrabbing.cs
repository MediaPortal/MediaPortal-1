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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler
{
  public class EpgGrabbing : IEpgGrabbing
  {


    private readonly ITvCardHandler _cardHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisEqcManagement"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public EpgGrabbing(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }

    /// <summary>
    /// grabs the epg.
    /// </summary>
    /// <returns></returns>
    public bool Start(BaseEpgGrabber grabber)
    {
      this.LogInfo("EpgGrabbing: Start");
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return false;
        }

        if (grabber == null)
        {
          return false;
        }
        _cardHandler.Card.GrabEpg(grabber);
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
        return false;
      }
    }

    /// <summary>
    /// Aborts grabbing the epg. This also triggers the OnEpgReceived callback.
    /// </summary>
    public void Abort()
    {
      this.LogInfo("EpgGrabbing: Abort");
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return;
        }
        _cardHandler.Card.AbortGrabbing();
        _cardHandler.Card.IsEpgGrabbing = false;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
    }

    /// <summary>
    /// Gets the epg.
    /// </summary>
    /// <value>The epg.</value>
    public List<EpgChannel> Epg
    {
      get
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return new List<EpgChannel>();
        }
        return _cardHandler.Card.Epg;
      }
    }


    /// <summary>
    /// Returns if the card is grabbing the epg or not
    /// </summary>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    public bool IsGrabbing
    {
      get
      {
        try
        {
          if (_cardHandler.DataBaseCard.Enabled == false)
          {
            return false;
          }
         
          return _cardHandler.Card.IsEpgGrabbing;
        }
        catch (Exception ex)
        {
          this.LogError(ex);
          return false;
        }
      }
    }

    /// <summary>
    /// Stops the grabbing epg.
    /// </summary>
    /// <param name="user">User</param>
    public void Stop(IUser user)
    {
      this.LogInfo("EpgGrabbing: Stop - user {0}", user.Name);
      _cardHandler.UserManagement.RemoveUser(user);
      int recentSubChannelId = _cardHandler.UserManagement.GetRecentSubChannelId(user.Name);
      if (recentSubChannelId > -1 && !_cardHandler.UserManagement.ContainsUsersForSubchannel(recentSubChannelId))
      {
        _cardHandler.Card.FreeSubChannel(recentSubChannelId);        
      }      
      _cardHandler.Card.IsEpgGrabbing = false;
    }
  }
}