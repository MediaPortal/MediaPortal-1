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

    #region public interface
    /// <summary>
    /// Returns the number of cards found
    /// </summary>
    public int Count
    {
      get
      {
        return RemoteControl.Instance.Cards;
      }
    }

    /// <summary>
    /// returns a virtual card for the specified index
    /// which can be used to control the card
    /// </summary>
    /// <param name="index">index of card</param>
    /// <returns></returns>
    public VirtualCard Card(int index)
    {
      return new VirtualCard(index, RemoteControl.HostName);
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="channelName">Name of the channel</param>
    /// <param name="card">returns on which card timeshifting is started</param>
    /// <returns>true if timeshifting has started, otherwise false</returns>
    public bool StartTimeShifting(string channelName, out VirtualCard card)
    {
      bool result= RemoteControl.Instance.StartTimeShifting(channelName, out card);
      return result;
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
      return RemoteControl.Instance.IsRecording(channelName, out  card);
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
      return RemoteControl.Instance.IsRecordingSchedule(idSchedule, out  card);
    }

    /// <summary>
    /// Stops recording the Schedule specified
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <returns></returns>
    public void StopRecordingSchedule(int idSchedule)
    {
      RemoteControl.Instance.StopRecordingSchedule(idSchedule);
    }

    /// <summary>
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// </summary>
    public void OnNewSchedule()
    {
      RemoteControl.Instance.OnNewSchedule();
    }

    /// <summary>
    /// Enable or disable the epg-grabber
    /// </summary>
    public bool EpgGrabberEnabled
    {
      get
      {
        return RemoteControl.Instance.EpgGrabberEnabled;
      }
      set
      {
        RemoteControl.Instance.EpgGrabberEnabled = value;
      }
    }

    /// <summary>
    /// Returns the SQl connection string to the database
    /// </summary>
    public string DatabaseConnectionString
    {
      get
      {
        return RemoteControl.Instance.DatabaseConnectionString;
      }
    }


    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream 
    /// </summary>
    /// <returns>URL containing the RTSP adress on which the card transmits its stream</returns>
    public string GetStreamUrlForFileName(int idRecording)
    {
      return RemoteControl.Instance.GetRecordingUrl(idRecording);
    }
    #endregion
  }
}
