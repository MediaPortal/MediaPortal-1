using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TvDatabase;
namespace MyTv
{
  public class ProgramModel
  {
    #region variables
    string _channel;
    Program _program;
    string _logo;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="ProgramModel"/> class.
    /// </summary>
    /// <param name="program">The program.</param>
    public ProgramModel(Program program)
    {
      _program = program;
      _channel = _program.ReferencedChannel().Name;
      _logo = String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.GetLogoFileName(_channel));
      if (!System.IO.File.Exists(_logo))
      {
        _logo = "";
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the title of the program
    /// </summary>
    /// <value>The title.</value>
    public string Title
    {
      get
      {
        return _program.Title;
      }
    }
    /// <summary>
    /// Gets the genre of the program
    /// </summary>
    /// <value>The genre.</value>
    public string Genre
    {
      get
      {
        return _program.Genre;
      }
    }
    /// <summary>
    /// Gets the programs' description.
    /// </summary>
    /// <value>The description.</value>
    public string Description
    {
      get
      {
        return _program.Description;
      }
    }
    /// <summary>
    /// Gets the channel name
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
    /// Gets the programs start time.
    /// </summary>
    /// <value>The start time.</value>
    public DateTime StartTime
    {
      get
      {
        return _program.StartTime;
      }
    }
    /// <summary>
    /// Gets the programs  end time.
    /// </summary>
    /// <value>The end time.</value>
    public DateTime EndTime
    {
      get
      {
        return _program.EndTime;
      }
    }
    /// <summary>
    /// Gets the program itself
    /// </summary>
    /// <value>The program.</value>
    public Program Program
    {
      get
      {
        return _program;
      }
    }

    /// <summary>
    /// Gets the start-end in hh:mm-hh:mm format
    /// </summary>
    /// <value>The start-end label.</value>
    public string StartEndLabel
    {
      get
      {
        return String.Format("{0}-{1}", StartTime.ToString("HH:mm"), EndTime.ToString("HH:mm"));
      }
    }
    /// <summary>
    /// Gets the duration in hh:mm format
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
          return String.Format("{0}:{1}", ts.Hours, ts.Minutes);
      }
    }
    /// <summary>
    /// Gets the date the program is started
    /// </summary>
    /// <value>The date.</value>
    public string Date
    {
      get
      {
        return StartTime.ToString("dd-MM HH:mm");
      }
    }
    /// <summary>
    /// Gets a value indicating whether this program is being recorded.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this program is being recorded; otherwise, <c>false</c>.
    /// </value>
    public bool IsRecorded
    {
      get
      {
        IList schedules = Schedule.ListAll();
        foreach (Schedule schedule in schedules)
        {
          if (schedule.Canceled != Schedule.MinSchedule) continue;
          if (schedule.IsRecordingProgram(this.Program, true))
          {
            return true;
          }
        }

        return false;
      }
    }
    public bool IsNotRecorded
    {
      get
      {
        return !IsRecorded;
      }
    }
    /// <summary>
    /// Gets the is notified.
    /// </summary>
    /// <value>The is notified.</value>
    public bool? IsNotified
    {
      get
      {
        return _program.Notify;
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
        if (_program.Notify)
        {
          return String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.TvNotifyIcon);
        }
        return "";
      }
    }
    /// <summary>
    /// Gets the recording logo.
    /// </summary>
    /// <value>The recording logo.</value>
    public string RecordingLogo
    {
      get
      {
        if (IsRecorded)
        {
          return String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Thumbs.TvRecordingIcon);
        }
        return "";
      }
    }
    #endregion
    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(object obj)
    {
      ProgramModel model = obj as ProgramModel;
      if (model == null)
      {
        ListBoxItem box = obj as ListBoxItem;
        if (box != null)
        {
          model = box.Content as ProgramModel;
        }
      }
      if (model == null) return false;
      return (model.Program.IdProgram == Program.IdProgram);
    }
    /// <summary>
    /// Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override int GetHashCode()
    {
      return Program.IdProgram;
    }

    /// <summary>
    /// Determines whether this program is recorded and ifso returns the schedule
    /// </summary>
    /// <param name="recordingSchedule">The schedule recording the program.</param>
    /// <param name="filterCanceledRecordings">if set to <c>true</c> [filter canceled recordings].</param>
    /// <returns>
    /// 	<c>true</c> if [is recording program] [the specified recording schedule]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRecordingProgram( out Schedule recordingSchedule, bool filterCanceledRecordings)
    {
      recordingSchedule = null;
      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (schedule.IsRecordingProgram(Program, filterCanceledRecordings))
        {
          recordingSchedule = schedule;
          return true;
        }
      }
      return false;
    }
  }
}
