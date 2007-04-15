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
    ProgramModel _model;
    Channel _channel;
    public Style _buttonStyle;
    string _timeStamp;
    string _recodingLogo;
    string _channelName;

    public EpgGridCell()
    {
    }
    public EpgGridCell(int column)
      : base(column)
    {
    }
    public EpgGridCell(int column, int columnSpan)
      : base(column, columnSpan)
    {
    }
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
    public string Title
    {
      get
      {
        if (_model == null) return "";
        return _model.Title;
      }
    }
    public string Description
    {
      get
      {
        if (_model == null) return "";
        return _model.Description;
      }
    }
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
    public string Date
    {
      get
      {
        if (_model == null) return "";
        return _model.Date;
      }
    }
    public string Duration
    {
      get
      {
        if (_model == null) return "";
        return _model.Duration;
      }
    }
    public DateTime StartTime
    {
      get
      {
        if (_model == null) return DateTime.MinValue;
        return _model.StartTime;
      }
    }
    public DateTime EndTime
    {
      get
      {
        if (_model == null) return DateTime.MinValue;
        return _model.EndTime;
      }
    }
    public string Genre
    {
      get
      {
        if (_model == null) return "";
        return _model.Genre;
      }
    }
    public bool IsRecorded
    {
      get
      {
        if (_model == null) return false;
        return _model.IsRecorded;
      }
    }
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
    public Program Program
    {
      get
      {
        if (_model == null) return null;
        return _model.Program;
      }
    }
    public string NotifyLogo
    {
      get
      {
        if (_model == null) return null;
        return _model.NotifyLogo;
      }
    }
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
    public string StartEndLabel
    {
      get
      {
        if (_model == null) return "";
        return _model.StartEndLabel;
      }
    }
  }
}
