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
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;
using TvDatabase;


namespace TvService
{
  public class Recorder
  {
    readonly ITvCardHandler _cardHandler;
    readonly bool _timeshiftingEpgGrabberEnabled;
    /// <summary>
    /// Initializes a new instance of the <see cref="Recording"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public Recorder(ITvCardHandler cardHandler)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      _cardHandler = cardHandler;
      _timeshiftingEpgGrabberEnabled = (layer.GetSetting("timeshiftingEpgGrabberEnabled", "no").Value == "yes");
    }
    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <param name="contentRecording">if true then create a content recording else a reference recording</param>
    /// <param name="startTime">not used</param>
    /// <returns></returns>
    public bool Start(ref User user, ref string fileName, bool contentRecording, long startTime)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return false;

        lock (this)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
              return false;

            if (_cardHandler.IsLocal == false)
            {
              return RemoteControl.Instance.StartRecording(ref user, ref fileName, contentRecording, startTime);
            }
          } catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return false;
          }

          TvCardContext context = _cardHandler.Card.Context as TvCardContext;
          if (context == null)
            return false;

          context.GetUser(ref user);
          ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
          if (subchannel == null)
            return false;
          //gibman 
          // RecordingFormat 0 = ts
          // RecordingFormat 1 = mpeg
          if (subchannel.IsRecordingTransportStream || (_cardHandler.DataBaseCard.RecordingFormat == 0))
          {
            fileName = System.IO.Path.ChangeExtension(fileName, ".ts");
          }
          else
          {
            fileName = System.IO.Path.ChangeExtension(fileName, ".mpg");
          }

          Log.Write("card: StartRecording {0} {1}", _cardHandler.DataBaseCard.IdCard, fileName);
          bool result = subchannel.StartRecording((_cardHandler.DataBaseCard.RecordingFormat == 0), fileName);
          if (result)
          {
            fileName = subchannel.RecordingFileName;
            context.Owner = user;
          }

          if (_timeshiftingEpgGrabberEnabled)
          {
            Channel channel = Channel.Retrieve(user.IdChannel);
            if (channel.GrabEpg)
              _cardHandler.Card.GrabEpg();
            else
              Log.Info("TimeshiftingEPG: channel {0} is not configured for grabbing epg", channel.DisplayName);
          }

          return result;

        }
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns></returns>
    public bool Stop(ref User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return false;
        Log.Write("card: StopRecording {0}", _cardHandler.DataBaseCard.IdCard);
        lock (this)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
              return false;

            if (_cardHandler.IsLocal == false)
            {
              return RemoteControl.Instance.StopRecording(ref user);
            }
          } catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return false;
          }
          Log.Write("card: StopRecording for card:{0}", _cardHandler.DataBaseCard.IdCard);
          TvCardContext context = _cardHandler.Card.Context as TvCardContext;
          if (context == null)
            return false;
          if (IsRecording(ref user))
          {
            context.GetUser(ref user);
            ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
            if (subchannel == null)
              return false;
            subchannel.StopRecording();
            _cardHandler.Card.FreeSubChannel(user.SubChannel);
            if (subchannel.IsTimeShifting == false || context.Users.Length <= 1)
            {
              _cardHandler.Users.RemoveUser(user);
            }
          }

          User[] users = context.Users;
          for (int i = 0; i < users.Length; ++i)
          {
            ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(users[i].SubChannel);
            if (subchannel != null)
            {
              if (subchannel.IsRecording)
              {
                context.Owner = users[i];
                break;
              }
            }
          }
          return true;
        }
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }



    public bool IsRecordingChannel(string channelName)
    {
      User[] users = _cardHandler.Users.GetUsers();
      if (users == null)
        return false;
      if (users.Length == 0)
        return false;

      for (int i = 0; i < users.Length; ++i)
      {
        User user = users[i];
        if (!user.IsAdmin)
          continue;
        if (_cardHandler.CurrentChannelName(ref user) == null)
          continue;
        if (_cardHandler.CurrentChannelName(ref user) == channelName)
        {
          if (_cardHandler.Recorder.IsRecording(ref user))
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Gets a value indicating whether this card is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this card is recording; otherwise, <c>false</c>.
    /// </value>
    public bool IsAnySubChannelRecording
    {
      get
      {
        User[] users = _cardHandler.Users.GetUsers();
        if (users == null)
          return false;
        if (users.Length == 0)
          return false;
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (IsRecording(ref user))
            return true;
        }
        return false;
      }
    }

    public bool IsRecordingAnyUser()
    {
      User[] users = _cardHandler.Users.GetUsers();
      if (users == null)
        return false;
      if (users.Length == 0)
        return false;

      for (int i = 0; i < users.Length; ++i)
      {
        User user = users[i];
        if (!user.IsAdmin)
          continue;
        if (_cardHandler.CurrentChannelName(ref user) == null)
          continue;

        if (_cardHandler.Recorder.IsRecording(ref user))
        {
          return true;
        }

      }
      return false;
    }

    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true when card is recording otherwise false</returns>
    public bool IsRecording(ref User user)
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
            return RemoteControl.Instance.IsRecording(ref user);
          }
        } catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return false;
        return subchannel.IsRecording;
      } catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>filename or null when not recording</returns>
    public string FileName(ref User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return "";

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return "";

          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.RecordingFileName(ref user);
          }
        } catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return "";
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return null;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return null;
        return subchannel.RecordingFileName;
      } catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted(User user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return DateTime.MinValue;
        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return DateTime.MinValue;
          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.RecordingStarted(user);
          }
        } catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}", _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return DateTime.MinValue;
        }

        TvCardContext context = _cardHandler.Card.Context as TvCardContext;
        if (context == null)
          return DateTime.MinValue;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return DateTime.MinValue;
        return subchannel.RecordingStarted;
      } catch (Exception ex)
      {
        Log.Write(ex);
        return DateTime.MinValue;
      }
    }

  }
}
