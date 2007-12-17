using System;
using System.Collections.Generic;
using System.Text;
using TvDatabase;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  public class ScheduleModel
  {
    #region variables
    string _channel;
    Schedule _schedule;
    string _logo;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleModel"/> class.
    /// </summary>
    /// <param name="schedule">The schedule.</param>
    public ScheduleModel(Schedule schedule)
    {
      _schedule = schedule;
      _channel = _schedule.ReferencedChannel().Name;
      _logo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.GetLogoFileName(_channel));
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
        return _schedule.ProgramName;
      }
    }
    /// <summary>
    /// Gets the schedule type
    /// </summary>
    /// <value>The schedule type.</value>
    public string ScheduleType
    {
      get
      {
        switch((ScheduleRecordingType)_schedule.ScheduleType)
        {
          case ScheduleRecordingType.Once:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 59)/*"Record once"*/;
          case ScheduleRecordingType.Daily:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 63)/*"Record every day at this time"*/ ;
          case ScheduleRecordingType.Weekly:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 62)/*"Record every week at this time"*/;
          case ScheduleRecordingType.Weekends:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 65)/*"Record Sat-Sun"*/;
          case ScheduleRecordingType.WorkingDays:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 64)/*"Record Mon-fri"*/;
          case ScheduleRecordingType.EveryTimeOnEveryChannel:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 61)/*"Record everytime on every channel"*/;
          case ScheduleRecordingType.EveryTimeOnThisChannel:
            return ServiceScope.Get<ILocalisation>().ToString("mytv", 60)/*"Record everytime on this channel"*/;
        }
        return "";
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
          return _schedule.StartTime;
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
          return _schedule.EndTime;
        }
      }
      /// <summary>
      /// Gets the schedule.
      /// </summary>
      /// <value>The schedule.</value>
      public Schedule Schedule
      {
        get
        {
          return _schedule;
        }
      }

      /// <summary>
      /// Gets the start-end label.
      /// </summary>
      /// <value>The start-end label.</value>
      public string StartEndLabel
      {
        get
        {
          string date = "";
          if (StartTime.Date == DateTime.Now.Date)
          {
            date = ServiceScope.Get<ILocalisation>().ToString("mytv", 133);//today
          }
          else if (StartTime.Date == DateTime.Now.Date.AddDays(1))
          {
            date = ServiceScope.Get<ILocalisation>().ToString("mytv", 134);//tomorrow
          }
          else
          {
            int dayofWeek=(int)StartTime.DayOfWeek;
            int month=StartTime.Month;
            date = String.Format("{0} {1} {2}", ServiceScope.Get<ILocalisation>().ToString("days", dayofWeek),
                                              StartTime.Day,
                                              ServiceScope.Get<ILocalisation>().ToString("months", month));
          }
          return String.Format("{0} {1}-{2}", date, StartTime.ToString("HH:mm"), EndTime.ToString("HH:mm"));
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
          TimeSpan ts = EndTime - StartTime;
          if (ts.Minutes < 10)
            return String.Format("{0}:0{1}", ts.Hours, ts.Minutes);
          else
            return String.Format("{0}:B{1}", ts.Hours, ts.Minutes);
        }
      }
    #endregion
    }
  }
