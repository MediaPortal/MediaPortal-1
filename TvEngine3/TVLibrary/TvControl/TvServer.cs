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
using System.Collections.Generic;
using System.IO;

namespace TvControl
{
  /// <summary>
  /// Class which connects to the remote tv-server
  /// </summary>
  public class TvServer
  {
    private static void HandleFailure()
    {
      RemoteControl.Clear();
    }

    #region public interface

    /// <summary>
    /// Returns the number of cards found
    /// </summary>
    public int Count
    {
      get
      {
        try
        {
          return RemoteControl.Instance.Cards;
        }
        catch (Exception)
        {
          HandleFailure();
        }
        return 0;
      }
    }

    /// <summary>
    /// Gets the RTSP URL for a file located at the tvserver.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public string GetRtspUrlForFile(string fileName)
    {
      try
      {
        try
        {
          if (File.Exists(fileName))
          {
            return fileName;
          }
        }
        catch (Exception)
        {
          HandleFailure();
        }
        return RemoteControl.Instance.GetUrlForFile(fileName);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return "";
    }

    /// <summary>
    /// Deletes the recording at the tvserver
    /// </summary>
    /// <param name="idRecording">The id of the recording.</param>
    public bool DeleteRecording(int idRecording)
    {
      try
      {
        return RemoteControl.Instance.DeleteRecording(idRecording);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Checks if the files of a recording still exist
    /// </summary>
    /// <param name="idRecording">The id of the recording</param>
    public bool IsRecordingValid(int idRecording)
    {
      try
      {
        return RemoteControl.Instance.IsRecordingValid(idRecording);
      }
      catch (Exception)
      {
        return true;
      }
    }

    /// <summary>
    /// Gets the user for card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    public User GetUserForCard(int cardId)
    {
      try
      {
        return RemoteControl.Instance.GetUserForCard(cardId);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return null;
    }

    /// <summary>
    /// returns a virtual card for the specified index
    /// which can be used to control the card
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="index">index of card</param>
    /// <returns></returns>
    public VirtualCard CardByIndex(User user, int index)
    {
      try
      {
        RemoteControl.Instance.CardId(index);
        return new VirtualCard(user, RemoteControl.HostName);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return null;
    }

    /// <summary>
    /// Determines whether the specified channel name is recording.
    /// </summary>
    /// <param name="channelName">Name of the channel.</param>
    /// <param name="card">The vcard.</param>
    /// <returns>
    /// 	<c>true</c> if the specified channel name is recording; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRecording(string channelName, out VirtualCard card)
    {
      VirtualCard vc = card = null;
      try
      {
        bool result = WaitFor<bool>.Run(VirtualCard.CommandTimeOut,
                                        () => RemoteControl.Instance.IsRecording(channelName, out vc));
        card = vc;
        return result;
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Returns if any card is recording
    /// </summary>
    /// <returns>true if any card is recording, otherwise false</returns>
    public bool IsAnyCardRecording()
    {
      try
      {
        return WaitFor<bool>.Run(VirtualCard.CommandTimeOut, () => RemoteControl.Instance.IsAnyCardRecording());
      }
      catch (Exception)
      {
        HandleFailure();
      }

      return false;
    }

    /// <summary>
    /// Determines if any card is currently busy recording or timeshifting
    /// </summary>
    /// <param name="userTS">timeshifting user</param>
    /// <param name="isUserTS">true if the specified user is timeshifting</param>
    /// <param name="isAnyUserTS">true if any user (except for the userTS) is timeshifting</param>
    /// <param name="isRec">true if recording</param>
    /// <returns>
    /// 	<c>true</c> if a card is recording or timeshifting; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAnyCardRecordingOrTimeshifting(User userTS, out bool isUserTS, out bool isAnyUserTS, out bool isRec)
    {
      isUserTS = false;
      isAnyUserTS = false;
      isRec = false;

      try
      {
        if (RemoteControl.Instance.IsAnyCardRecordingOrTimeshifting(userTS, out isUserTS, out isAnyUserTS, out isRec))
        {
          return true;
        }
      }
      catch (Exception)
      {
        HandleFailure();
      }

      return false;
    }

    /// <summary>
    /// Determines if any card is not locked by a user
    /// </summary>
    /// <returns>true if any card is idle, otherwise false</returns>
    public bool IsAnyCardIdle()
    {
      try
      {
        if (RemoteControl.Instance.IsAnyCardIdle())
        {
          return true;
        }
      }
      catch (Exception)
      {
        HandleFailure();
      }

      return false;
    }

    /// <summary>
    /// Query what card would be used for timeshifting on any given channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>    
    /// <returns>
    /// returns card id which would be used when doing the actual timeshifting.
    /// </returns>
    public int TimeShiftingWouldUseCard(ref User user, int idChannel)
    {
      try
      {
        return RemoteControl.Instance.TimeShiftingWouldUseCard(ref user, idChannel);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="idChannel">id of the channel</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref User user, int idChannel, out VirtualCard card)
    {
      card = null;
      try
      {
        TvResult result = RemoteControl.Instance.StartTimeShifting(ref user, idChannel, out card);
        return result;
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return TvResult.UnknownError;
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="idChannel">id of the channel</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <param name="forceCardId">Indicated, if the card should be forced</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref User user, int idChannel, out VirtualCard card, bool forceCardId)
    {
      card = null;
      try
      {
        TvResult result = RemoteControl.Instance.StartTimeShifting(ref user, idChannel, out card, forceCardId);
        return result;
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return TvResult.UnknownError;
    }


    /// <summary>
    /// Checks if the schedule specified is currently being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <param name="card">returns card is recording the channel</param>
    /// <returns>true if a card is recording the schedule, otherwise false</returns>
    public bool IsRecordingSchedule(int idSchedule, out VirtualCard card)
    {
      card = null;
      try
      {
        return RemoteControl.Instance.IsRecordingSchedule(idSchedule, out card);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Stops recording the Schedule specified
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <returns></returns>
    public void StopRecordingSchedule(int idSchedule)
    {
      try
      {
        RemoteControl.Instance.StopRecordingSchedule(idSchedule);
      }
      catch (Exception)
      {
        HandleFailure();
      }
    }

    /// <summary>
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// </summary>
    public void OnNewSchedule()
    {
      try
      {
        RemoteControl.Instance.OnNewSchedule();
      }
      catch (Exception)
      {
        HandleFailure();
      }
    }


    /// <summary>
    /// This method should be called by a client to check
    /// if there is any upcoming recording
    /// </summary>
    public bool IsTimeToRecord(DateTime time)
    {
      try
      {
        return RemoteControl.Instance.IsTimeToRecord(time);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// This method should be called by a client to check 
    /// if a specific recording is due. 
    /// </summary>
    public bool IsTimeToRecord(DateTime time, int recordingId)
    {
      try
      {
        return RemoteControl.Instance.IsTimeToRecord(time, recordingId);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }


    /// <summary>
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// </summary>
    public void OnNewSchedule(EventArgs args)
    {
      try
      {
        RemoteControl.Instance.OnNewSchedule(args);
      }
      catch (Exception)
      {
        HandleFailure();
      }
    }

    /// <summary>
    /// Enable or disable the epg-grabber
    /// </summary>
    public bool EpgGrabberEnabled
    {
      get
      {
        try
        {
          return RemoteControl.Instance.EpgGrabberEnabled;
        }
        catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
      set
      {
        try
        {
          RemoteControl.Instance.EpgGrabberEnabled = value;
        }
        catch (Exception)
        {
          HandleFailure();
        }
      }
    }

    /// <summary>
    /// Returns the SQl connection string to the database
    /// </summary>
    public void GetDatabaseConnectionString(out string connectionString, out string provider)
    {
      connectionString = "";
      provider = "";
      try
      {
        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);
      }
      catch (Exception)
      {
        HandleFailure();
      }
    }


    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream 
    /// </summary>
    /// <returns>URL containing the RTSP adress on which the card transmits its stream</returns>
    public string GetStreamUrlForFileName(int idRecording)
    {
      try
      {
        return RemoteControl.Instance.GetRecordingUrl(idRecording);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return "";
    }

    /// <summary>
    /// Fetches all channel states for a specific user (cached - faster)
    /// </summary>    
    /// <param name="user"></param>      
    public Dictionary<int, ChannelState> GetAllChannelStatesCached(User user)
    {
      try
      {
        return RemoteControl.Instance.GetAllChannelStatesCached(user);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return null;
    }


    /// <summary>
    /// Fetches all channel states for a specific group
    /// </summary>
    /// <param name="idGroup"></param>    
    /// <param name="user"></param>        
    public Dictionary<int, ChannelState> GetAllChannelStatesForGroup(int idGroup, User user)
    {
      try
      {
        return RemoteControl.Instance.GetAllChannelStatesForGroup(idGroup, user);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return null;
    }

    /// <summary>
    /// Finds out whether a channel is currently tuneable or not
    /// </summary>
    /// <param name="idChannel">the channel id</param>
    /// <param name="user">User</param>
    /// <returns>an enum indicating tunable/timeshifting/recording</returns>
    public ChannelState GetChannelState(int idChannel, User user)
    {
      try
      {
        return RemoteControl.Instance.GetChannelState(idChannel, user);
      }
      catch (Exception)
      {
        HandleFailure();
      }
      return ChannelState.nottunable;
    }

    /// <summary>
    /// Fetches all channels with backbuffer
    /// </summary>
    /// <param name="currentRecChannels"></param>
    /// <param name="currentTSChannels"></param>
    /// <param name="currentUnavailChannels"></param>
    /// <param name="currentAvailChannels"></param>
    public void GetAllRecordingChannels(out List<int> currentRecChannels, out List<int> currentTSChannels,
                                        out List<int> currentUnavailChannels, out List<int> currentAvailChannels)
    {
      currentRecChannels = null;
      currentTSChannels = null;
      currentUnavailChannels = null;
      currentAvailChannels = null;
      try
      {
        RemoteControl.Instance.GetAllRecordingChannels(out currentRecChannels, out currentTSChannels,
                                                       out currentUnavailChannels, out currentAvailChannels);
      }
      catch (Exception)
      {
        HandleFailure();
      }
    }

    #endregion
  }
}