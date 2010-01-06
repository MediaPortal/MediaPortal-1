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
using TvLibrary.Interfaces;
using TvLibrary.Epg;
using TvLibrary.Log;
using TvControl;

namespace TvService
{
  public class EpgGrabbing
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
      Log.Epg("EpgGrabbing: Start");
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return false;
        }

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
          {
            return false;
          }
        }
        catch (Exception)
        {
          Log.Error("EpgGrabbing: unable to connect to controller at: {0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }

        if (_cardHandler.IsLocal == false)
        {
          //RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          //RemoteControl.Instance.GrabEpg();
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
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Aborts grabbing the epg. This also triggers the OnEpgReceived callback.
    /// </summary>
    public void Abort()
    {
      Log.Epg("EpgGrabbing: Abort");
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return;
        }
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
          {
            return;
          }
        }
        catch (Exception)
        {
          Log.Error("EpgGrabbing: unable to connect to controller at: {0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return;
        }
        if (_cardHandler.IsLocal == false)
        {
          //RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          //RemoteControl.Instance.GrabEpg();
          return;
        }
        _cardHandler.Card.AbortGrabbing();
        return;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
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
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
          {
            return new List<EpgChannel>();
          }
        }
        catch (Exception)
        {
          Log.Error("EpgGrabbing: unable to connect to controller at: {0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return null;
        }
        if (_cardHandler.IsLocal == false)
        {
          //RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          //RemoteControl.Instance.GrabEpg();
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
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            {
              return false;
            }
          }
          catch (Exception)
          {
            Log.Error("EpgGrabbing: unable to connect to controller at: {0}",
                      _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return false;
          }
          if (_cardHandler.IsLocal == false)
          {
            try
            {
              RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
              return RemoteControl.Instance.IsGrabbingEpg(_cardHandler.DataBaseCard.IdCard);
            }
            catch (Exception)
            {
              Log.Error("EpgGrabbing: unable to connect to controller at: {0}",
                        _cardHandler.DataBaseCard.ReferencedServer().HostName);
              return false;
            }
          }
          return _cardHandler.Card.IsEpgGrabbing;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
      }
    }

    /// <summary>
    /// Stops the grabbing epg.
    /// </summary>
    /// <param name="user">User</param>
    public void Stop(User user)
    {
      Log.Epg("EpgGrabbing: Stop - user {0}", user.Name);
      if (_cardHandler.IsLocal == false)
      {
        // RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
        // RemoteControl.Instance.StopGrabbingEpg();
        return;
      }
      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context != null)
      {
        context.Remove(user);
        if (context.ContainsUsersForSubchannel(user.SubChannel) == false)
        {
          if (user.SubChannel > -1)
          {
            _cardHandler.Card.FreeSubChannel(user.SubChannel);
          }
        }
      }
      else
      {
        Log.Epg("EpgGrabbing: Stop - context == null");
      }

      _cardHandler.Card.IsEpgGrabbing = false;
    }
  }
}