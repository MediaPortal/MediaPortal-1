using System;
using System.Collections.Generic;
using System.Text;
using TvDatabase;
namespace MyTv
{
  public class RecordingModel
  {
    #region variables
    string _channel;
    Recording _recording;
    string _logo;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="RecordingModel"/> class.
    /// </summary>
    /// <param name="recording">The recording.</param>
    public RecordingModel(Recording recording)
    {
      _recording = recording;
      _channel = _recording.ReferencedChannel().Name;
      _logo = System.IO.Path.ChangeExtension(recording.FileName, ".png");
      if (!System.IO.File.Exists(_logo))
      {
        _logo = "";
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title
    {
      get
      {
        return _recording.Title;
      }
    }
    /// <summary>
    /// Gets the genre.
    /// </summary>
    /// <value>The genre.</value>
    public string Genre
    {
      get
      {
        return _recording.Genre;
      }
    }
    /// <summary>
    /// Gets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description
    {
      get
      {
        return _recording.Description;
      }
    }
    /// <summary>
    /// Gets the times watched.
    /// </summary>
    /// <value>The times watched.</value>
    public int TimesWatched
    {
      get
      {
        return _recording.TimesWatched;
      }
    }
    /// <summary>
    /// Gets the channel.
    /// </summary>
    /// <value>The channel.</value>
    public string Channel
    {
      get
      {
        return _channel;
      }
    }
    /// <summary>
    /// Gets the channel logo.
    /// </summary>
    /// <value>The logo.</value>
    public string Logo
    {
      get
      {
        return _logo;
      }
    }
    /// <summary>
    /// Gets the start time.
    /// </summary>
    /// <value>The start time.</value>
    public DateTime StartTime
    {
      get
      {
        return _recording.StartTime;
      }
    }
    /// <summary>
    /// Gets the end time.
    /// </summary>
    /// <value>The end time.</value>
    public DateTime EndTime
    {
      get
      {
        return _recording.EndTime;
      }
    }
    /// <summary>
    /// Gets the recording.
    /// </summary>
    /// <value>The recording.</value>
    public Recording Recording
    {
      get
      {
        return _recording;
      }
    }

    public string StartEndLabel
    {
      get
      {
        return  String.Format("{0}-{1}", StartTime.ToString("HH:mm"), EndTime.ToString("HH:mm"));
      }
    }
    #endregion
    }
  }
