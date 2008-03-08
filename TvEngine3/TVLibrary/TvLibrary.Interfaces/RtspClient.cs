using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Streaming
{
  /// <summary>
  ///class holding the details about a single rtsp streaming client
  /// </summary>
  [Serializable]
  public class RtspClient
  {
    #region variables
    bool _isActive;
    string _ipAdress;
    string _streamName;
    DateTime _dateTimeStarted;
    string _description;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="RtspClient"/> class.
    /// </summary>
    public RtspClient()
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="RtspClient"/> class.
    /// </summary>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <param name="ipadress">The ipadress.</param>
    /// <param name="streamName">Name of the stream.</param>
    /// <param name="started">The time the client connected.</param>
    /// <param name="description">The description.</param>
    public RtspClient(bool isActive, string ipadress, string streamName,string description,DateTime started)
    {
      _isActive = isActive;
      _ipAdress = ipadress;
      _streamName = streamName;
      _dateTimeStarted = started;
      _description = description;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description
    {
      get
      {
        return _description;
      }
    }

    /// <summary>
    /// Gets the date and time this client has connected.
    /// </summary>
    /// <value>The date time started.</value>
    public DateTime DateTimeStarted
    {
      get
      {
        return _dateTimeStarted;
      }
    }

    /// <summary>
    /// Gets a value indicating whether user is is active.
    /// </summary>
    /// <value><c>true</c> if this user is active; otherwise, <c>false</c>.</value>
    public bool IsActive
    {
      get
      {
        return _isActive;
      }
    }

    /// <summary>
    /// Gets the ip adress.
    /// </summary>
    /// <value>The ip adress.</value>
    public string IpAdress
    {
      get
      {
        return _ipAdress;
      }
    }

    /// <summary>
    /// Gets the name of the stream.
    /// </summary>
    /// <value>The name of the stream.</value>
    public string StreamName
    {
      get
      {
        return _streamName;
      }
    }
    #endregion
  }
}
