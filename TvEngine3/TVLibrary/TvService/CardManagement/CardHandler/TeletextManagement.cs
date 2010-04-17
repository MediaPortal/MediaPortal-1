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
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;

namespace TvService
{
  public class TeletextManagement
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
    public bool IsGrabbingTeletext(User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return false;
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return false;
          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.IsGrabbingTeletext(user);
          }
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return false;
        return subchannel.GrabTeletext;
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
    public bool HasTeletext(User user)
    {
      if (_cardHandler.DataBaseCard.Enabled == false)
        return false;

      try
      {
        RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
        if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
          return false;
        if (_cardHandler.IsLocal == false)
        {
          return RemoteControl.Instance.HasTeletext(user);
        }
      }
      catch (Exception)
      {
        Log.Error("card: unable to connect to slave controller at:{0}",
                  _cardHandler.DataBaseCard.ReferencedServer().HostName);
        return false;
      }

      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null)
        return false;
      context.GetUser(ref user);
      ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
      if (subchannel == null)
        return false;
      return subchannel.HasTeletext;
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(User user, int pageNumber)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return new TimeSpan(0, 0, 0, 15);
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return new TimeSpan(0, 0, 0, 15);
          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.TeletextRotation(user, pageNumber);
          }
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return new TimeSpan(0, 0, 0, 15);
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return new TimeSpan(0, 0, 0, 15);
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
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
    public void GrabTeletext(User user, bool onOff)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return;
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return;
          if (_cardHandler.IsLocal == false)
          {
            RemoteControl.Instance.GrabTeletext(user, onOff);
          }
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return;
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
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
    public byte[] GetTeletextPage(User user, int pageNumber, int subPageNumber)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return new byte[] {1};
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return new byte[] {1};
          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.GetTeletextPage(user, pageNumber, subPageNumber);
          }
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return new byte[] {1};
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return new byte[] {1};
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
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
    public int SubPageCount(User user, int pageNumber)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return -1;
        if (_cardHandler.IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.SubPageCount(user, pageNumber);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return -1;
          }
        }
        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return -1;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
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
    public int GetTeletextRedPageNumber(User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return -1;
        if (_cardHandler.IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.GetTeletextRedPageNumber(user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return -1;
          }
        }
        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return -1;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
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
    public int GetTeletextGreenPageNumber(User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return -1;
        if (_cardHandler.IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.GetTeletextGreenPageNumber(user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return -1;
          }
        }
        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return -1;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
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
    public int GetTeletextYellowPageNumber(User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return -1;
        if (_cardHandler.IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.GetTeletextYellowPageNumber(user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return -1;
          }
        }
        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return -1;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
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
    public int GetTeletextBluePageNumber(User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return -1;
        if (_cardHandler.IsLocal == false)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.GetTeletextBluePageNumber(user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return -1;
          }
        }
        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return -1;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
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