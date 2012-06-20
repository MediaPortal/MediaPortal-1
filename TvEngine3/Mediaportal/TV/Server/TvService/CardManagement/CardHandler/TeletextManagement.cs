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
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.CardManagement.CardHandler
{
  public class TeletextManagement : ITeletextManagement
  {
    private readonly ITvCardHandler _cardHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisEqcManagement"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public TeletextManagement(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }

    /// <summary>
    /// Returns if the card is grabbing teletext or not
    /// </summary>
    /// <param name="user">USer</param>
    /// <returns>true when card is grabbing teletext otherwise false</returns>
    public bool IsGrabbingTeletext(IUser user)
    {
      try
      {
        bool grabTeletext = false;
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return false;
        }        
        
        _cardHandler.UserManagement.RefreshUser(ref user);


        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(_cardHandler.UserManagement.GetTimeshiftingChannelId(user.Name));
        if (subchannel != null)
        {
          grabTeletext = subchannel.GrabTeletext;              
        }

        return grabTeletext;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    public bool HasTeletext(IUser user)
    {
      bool hasTeletext = false;
      if (_cardHandler.DataBaseCard.enabled == false)
      {
        return false;
      }
      
      _cardHandler.UserManagement.RefreshUser(ref user);


      ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(_cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
      if (subchannel != null)
      {
        hasTeletext = subchannel.HasTeletext;
      }          
      

      return hasTeletext;
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(IUser user, int pageNumber)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return new TimeSpan(0, 0, 0, 15);
        }       
        
        _cardHandler.UserManagement.RefreshUser(ref user);

        int subchannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subchannelId);
        if (subchannel == null)
          return new TimeSpan(0, 0, 0, 15);
        return subchannel.TeletextDecoder.RotationTime(pageNumber);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return new TimeSpan(0, 0, 0, 15);
      }
    }

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="onOff">turn on/off teletext grabbing</param>
    public void GrabTeletext(IUser user, bool onOff)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return;
        }       
        
        _cardHandler.UserManagement.RefreshUser(ref user);
        int subchannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subchannelId);
        if (subchannel == null)
          return;
        subchannel.GrabTeletext = onOff;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
      }
    }

    /// <summary>
    /// Gets the teletext page.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    public byte[] GetTeletextPage(IUser user, int pageNumber, int subPageNumber)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return new byte[] {1};
        }      
        
        _cardHandler.UserManagement.RefreshUser(ref user);

        int subchannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subchannelId);
        if (subchannel == null)
          return new byte[] {1};
        if (subchannel.TeletextDecoder == null)
          return new byte[] {1};
        return subchannel.TeletextDecoder.GetRawPage(pageNumber, subPageNumber);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return new byte[] {1};
      }
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    public int SubPageCount(IUser user, int pageNumber)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return -1;
        }
                
        _cardHandler.UserManagement.RefreshUser(ref user);
        int subchannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subchannelId);
        if (subchannel == null)
          return -1;
        if (subchannel.TeletextDecoder == null)
          return -1;
        return subchannel.TeletextDecoder.NumberOfSubpages(pageNumber) + 1;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the red button</returns>
    public int GetTeletextRedPageNumber(IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return -1;
        }
               
        _cardHandler.UserManagement.RefreshUser(ref user);
        int subchannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subchannelId);
        if (subchannel == null)
          return -1;
        if (subchannel.TeletextDecoder == null)
          return -1;
        return subchannel.TeletextDecoder.PageRed;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the green button</returns>
    public int GetTeletextGreenPageNumber(IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return -1;
        }
                
        _cardHandler.UserManagement.RefreshUser(ref user);
        int subchannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subchannelId);
        if (subchannel == null)
          return -1;
        if (subchannel.TeletextDecoder == null)
          return -1;
        return subchannel.TeletextDecoder.PageGreen;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    public int GetTeletextYellowPageNumber(IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return -1;
        }
        
        _cardHandler.UserManagement.RefreshUser(ref user);
        int subchannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subchannelId);
        if (subchannel == null)
          return -1;
        if (subchannel.TeletextDecoder == null)
          return -1;
        return subchannel.TeletextDecoder.PageYellow;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the blue button</returns>
    public int GetTeletextBluePageNumber(IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.enabled == false)
        {
          return -1;
        }
              
        _cardHandler.UserManagement.RefreshUser(ref user);
        int subchannelId = _cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, _cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(subchannelId);
        if (subchannel == null)
          return -1;
        if (subchannel.TeletextDecoder == null)
          return -1;
        return subchannel.TeletextDecoder.PageBlue;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }
  }
}