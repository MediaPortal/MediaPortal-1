/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Text;

namespace TvControl
{
  /// <summary>
  /// Class which connects to the remote tv-server
  /// </summary>
  public class TvServer
  {

    void HandleFailure(Exception ex)
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
        catch (Exception ex)
        {
          HandleFailure(ex);
        }
        return 0;
      }
    }

    /// <summary>
    /// returns a virtual card for the specified index
    /// which can be used to control the card
    /// </summary>
    /// <param name="index">index of card</param>
    /// <returns></returns>
    public VirtualCard CardByIndex(int index)
    {
      try
      {
        int id = RemoteControl.Instance.CardId(index);
        return new VirtualCard(id, RemoteControl.HostName);
      }
      catch (Exception ex)
      {
        HandleFailure(ex);
      }
      return null;
    }

    /// <summary>
    /// Returns if any card is recording
    /// </summary>
    /// <returns>true if any card is recording, otherwise false</returns>
    public bool IsAnyCardRecording()
    {
      try
      {
        for (int i = 0; i < RemoteControl.Instance.Cards; ++i)
        {
          int id = RemoteControl.Instance.CardId(i);
          if (RemoteControl.Instance.IsRecording(id)) return true;
        }
      }
      catch (Exception ex)
      {
        HandleFailure(ex);
      }

      return false;
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="idChannel">id of the channel</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult StartTimeShifting(int idChannel, out VirtualCard card)
    {
      card = null;
      try
      {
        TvResult result = RemoteControl.Instance.StartTimeShifting(idChannel, new User(), out card);
        return result;
      }
      catch (Exception ex)
      {
        HandleFailure(ex);
      }
      return TvResult.UnknownError;
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="channelName">Name of the channel</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult StartTimeShifting(string channelName, out VirtualCard card)
    {
      card = null;
      try
      {
        TvResult result = RemoteControl.Instance.StartTimeShifting(channelName, new User(), out card);
        return result;
      }
      catch (Exception ex)
      {
        HandleFailure(ex);
      }
      return TvResult.UnknownError;
    }

    /// <summary>
    /// Checks if the channel specified is being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="channelName">Name of the channel</param>
    /// <param name="card">returns card is recording the channel</param>
    /// <returns>true if a card is recording the channel, otherwise false</returns>
    public bool IsRecording(string channelName, out VirtualCard card)
    {
      card = null;
      try
      {
        return RemoteControl.Instance.IsRecording(channelName, out  card);

      }
      catch (Exception ex)
      {
        HandleFailure(ex);
      }
      return false;
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
        return RemoteControl.Instance.IsRecordingSchedule(idSchedule, out  card);

      }
      catch (Exception ex)
      {
        HandleFailure(ex);
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
      catch (Exception ex)
      {
        HandleFailure(ex);
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
      catch (Exception ex)
      {
        HandleFailure(ex);
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
        catch (Exception ex)
        {
          HandleFailure(ex);
        }
        return false;
      }
      set
      {
        try
        {
          RemoteControl.Instance.EpgGrabberEnabled = value;
        }
        catch (Exception ex)
        {
          HandleFailure(ex);
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
      catch (Exception ex)
      {
        HandleFailure(ex);
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
      catch (Exception ex)
      {
        HandleFailure(ex);
      }
      return "";
    }
    #endregion
  }
}
