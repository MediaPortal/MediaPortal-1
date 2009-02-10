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
using TvLibrary.Log;
using TvControl;


namespace TvService
{
  public class AudioStreams
  {
    readonly ITvCardHandler _cardHandler;
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
    public IAudioStream[] Streams(User user)
    {
      if (_cardHandler.DataBaseCard.Enabled == false)
        return new List<IAudioStream>().ToArray();

      try
      {
        RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
        if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
          return new List<IAudioStream>().ToArray();
        if (_cardHandler.IsLocal == false)
        {
          return RemoteControl.Instance.AvailableAudioStreams(user);
        }
      } catch (Exception)
      {
        Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
        return null;
      }

      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null)
        return new List<IAudioStream>().ToArray();
      context.GetUser(ref user);
      ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
      if (subchannel == null)
        return new List<IAudioStream>().ToArray();
      return subchannel.AvailableAudioStreams.ToArray();
    }

    /// <summary>
    /// Gets the current audio stream.
    /// </summary>
    /// <returns></returns>
    public IAudioStream GetCurrent(User user)
    {
      if (_cardHandler.DataBaseCard.Enabled == false)
        return null;

      try
      {
        RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
        if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
          return null;
        if (_cardHandler.IsLocal == false)
        {
          return RemoteControl.Instance.GetCurrentAudioStream(user);
        }
      } catch (Exception)
      {
        Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
        return null;
      }

      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null)
        return null;
      context.GetUser(ref user);
      ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
      if (subchannel == null)
        return null;
      return subchannel.CurrentAudioStream;
    }

    /// <summary>
    /// Sets the current audio stream.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="stream">The stream.</param>
    public void Set(User user, IAudioStream stream)
    {
      if (_cardHandler.DataBaseCard.Enabled == false)
      {
        return;
      }

      try
      {
        RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
        if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
          return;
        Log.WriteFile("card: SetCurrentAudioStream: {0} - {1}", _cardHandler.DataBaseCard.IdCard, stream);
        if (_cardHandler.IsLocal == false)
        {
          Log.WriteFile("card: SetCurrentAudioStream: controlling remote instance {0}", RemoteControl.HostName);
          RemoteControl.Instance.SetCurrentAudioStream(user, stream);
        }
      } catch (Exception)
      {
        Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
        return;
      }

      TvCardContext context = _cardHandler.Card.Context as TvCardContext;
      if (context == null)
      {
        Log.WriteFile("card: SetCurrentAudioStream: TvCardContext == null");
        return;
      }

      context.GetUser(ref user);
      ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
      if (subchannel == null)
      {
        Log.WriteFile("card: SetCurrentAudioStream: ITvSubChannel == null");
        return;
      }

      subchannel.CurrentAudioStream = stream;
    }



    #endregion

  }
}
