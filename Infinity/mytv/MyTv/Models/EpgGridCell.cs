using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;
using TvDatabase;

namespace MyTv
{

  public class EpgGridCell : DataGridCell
  {
    #region variables
    ProgramModel _model;
    Channel _channel;
    public Style _buttonStyle;
    string _timeStamp;
    string _recodingLogo;
    string _channelName;
    #endregion

    #region ctors
    /// <summary>
    /// Initializes a new instance of the <see cref="EpgGridCell"/> class.
    /// </summary>
    public EpgGridCell()
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="EpgGridCell"/> class.
    /// </summary>
    /// <param name="column">The column.</param>
    public EpgGridCell(int column)
      : base(column)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="EpgGridCell"/> class.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="columnSpan">The column span.</param>
    public EpgGridCell(int column, int columnSpan)
      : base(column, columnSpan)
    {
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the time.
    /// </summary>
    /// <value>The time.</value>
    public string Time
    {
      get
      {
        return _timeStamp;
      }
      set
      {
        _timeStamp = value;
        ChangeProperty("Time");
      }
    }
    /// <summary>
    /// Gets or sets the button style.
    /// </summary>
    /// <value>The button style.</value>
    public Style ButtonStyle
    {
      get
      {
        return _buttonStyle;
      }
      set
      {
        _buttonStyle = value;
        ChangeProperty("ButtonStyle");
      }
    }
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    /// <value>The channel.</value>
    public Channel Channel
    {
      get
      {
        return _channel;
      }
      set
      {
        _channel = value;
        ChangeProperty("Channel");
        ChangeProperty("ChannelName");
        ChangeProperty("Logo");
      }
    }
    /// <summary>
    /// Gets or sets the program model.
    /// </summary>
    /// <value>The program model.</value>
    public ProgramModel ProgramModel
    {
      get
      {
        return _model;
      }
      set
      {
        _model = value;
        ChangeProperty("Channel");
        ChangeProperty("ChannelName");
        ChangeProperty("Title");
        ChangeProperty("Description");
        ChangeProperty("Genre");
        ChangeProperty("Logo");
      }
    }
    /// <summary>
    /// Gets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title
    {
      get
      {
        if (_model == null) return "";
        return _model.Title;
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
        if (_model == null) return "";
        return _model.Description;
      }
    }
    /// <summary>
    /// Gets or sets the name of the channel.
    /// </summary>
    /// <value>The name of the channel.</value>
    public string ChannelName
    {
      get
      {
        if (_channelName != null)
        {
          return _channelName;
        }
        if (_model == null)
        {
          if (_channel != null)
          {
            return _channel.Name;
          }
          return "";
        }
        return _model.Channel;
      }
      set
      {
        _channelName = value;
      }
    }
    /// <summary>
    /// Gets the date.
    /// </summary>
    /// <value>The date.</value>
    public string Date
    {
      get
      {
        if (_model == null) return "";
        return _model.Date;
      }
    }
    /// <summary>
    /// Gets the duration.
    /// </summary>
    /// <value>The duration.</value>
    public string Duration
    {
      get
      {
        if (_model == null) return "";
        return _model.Duration;
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
        if (_model == null) return DateTime.MinValue;
        return _model.StartTime;
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
        if (_model == null) return DateTime.MinValue;
        return _model.EndTime;
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
        if (_model == null) return "";
        return _model.Genre;
      }
    }
    /// <summary>
    /// Gets a value indicating whether this instance is recorded.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is recorded; otherwise, <c>false</c>.
    /// </value>
    public bool IsRecorded
    {
      get
      {
        if (_model == null) return false;
        return _model.IsRecorded;
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
        if (_model == null)
        {
          if (Channel != null)
          {
            return String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.GetLogoFileName(Channel.Name));
          }
          return "";
        }
        return _model.Logo;
      }
    }
    /// <summary>
    /// Gets the program.
    /// </summary>
    /// <value>The program.</value>
    public Program Program
    {
      get
      {
        if (_model == null) return null;
        return _model.Program;
      }
    }
    /// <summary>
    /// Gets the notify logo.
    /// </summary>
    /// <value>The notify logo.</value>
    public string NotifyLogo
    {
      get
      {
        if (_model == null) return null;
        return _model.NotifyLogo;
      }
    }
    /// <summary>
    /// Gets or sets the recording logo.
    /// </summary>
    /// <value>The recording logo.</value>
    public string RecordingLogo
    {
      get
      {
        return _recodingLogo;
      }
      set
      {
        _recodingLogo = value;
        ChangeProperty("RecodingLogo");
      }
    }
    /// <summary>
    /// Gets the start end label.
    /// </summary>
    /// <value>The start end label.</value>
    public string StartEndLabel
    {
      get
      {
        if (_model == null) return "";
        return _model.StartEndLabel;
      }
    }
    #endregion
  }
}
