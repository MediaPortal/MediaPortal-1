using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Streaming
{
  /// <summary>
  /// Class describing a single rtsp stream
  /// </summary>
  public class RtspStream
  {
    #region variables
    string _fileName;
    string _streamName;
    ITVCard _card;
    string _recording;
    #endregion

    #region ctors
    /// <summary>
    /// Initializes a new instance of the <see cref="RtspStream"/> class.
    /// </summary>
    /// <param name="streamName">Name of the stream.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="card">The card.</param>
    public RtspStream(string streamName, string fileName, ITVCard card)
    {
      _streamName = streamName;
      _fileName = fileName;
      _recording = "";
      _card = card;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RtspStream"/> class.
    /// </summary>
    /// <param name="streamName">Name of the stream.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="recording">The recording.</param>
    public RtspStream(string streamName, string fileName, string recording)
    {
      _streamName = streamName;
      _fileName = fileName;
      _recording = recording;
      _card = null;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the stream name.
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
      get
      {
        return _streamName;
      }
    }
    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    public string FileName
    {
      get
      {
        return _fileName;
      }
    }
    /// <summary>
    /// Gets the recording.
    /// </summary>
    /// <value>The recording.</value>
    public string Recording
    {
      get
      {
        return _recording;
      }
    }
    /// <summary>
    /// Gets the card.
    /// </summary>
    /// <value>The card.</value>
    public ITVCard Card
    {
      get
      {
        return _card;
      }
    }
    #endregion
  }
}
