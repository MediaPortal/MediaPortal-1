using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using IdeaBlade.Persistence;
using IdeaBlade.Util;

namespace TvDatabase
{
  public enum KeepMethodType
  {
    UntilSpaceNeeded,
    UntilWatched,
    TillDate,
    Always
  }
  public enum ScheduleRecordingType
  {
    Once,
    Daily,
    Weekly,
    EveryTimeOnThisChannel,
    EveryTimeOnEveryChannel,
    Weekends,
    WorkingDays
  }

  //quality of recording
  [Serializable]
  public sealed class Schedule : ScheduleDataRow
  {
    public enum QualityType
    {
      NotSet,
      Portable,
      Low,
      Medium,
      High
    }
    public class PriorityComparer : System.Collections.Generic.IComparer<Schedule>
    {
      bool _sortAscending = true;

      public PriorityComparer(bool sortAscending)
      {
        _sortAscending = sortAscending;
      }

      #region IComparer Members
      public int Compare(Schedule rec1, Schedule rec2)
      {
        if (_sortAscending)
        {
          if (rec1.Priority > rec2.Priority) return -1;
          if (rec1.Priority < rec2.Priority) return 1;
        }
        else
        {
          if (rec1.Priority > rec2.Priority) return 1;
          if (rec1.Priority < rec2.Priority) return -1;
        }
        return 0;
      }

      #endregion

    }


    static public readonly int HighestPriority = Int32.MaxValue;
    static public readonly int LowestPriority = 0;
    static public DateTime MinSchedule = new DateTime(2000, 1, 1);
    #region Constructors -- DO NOT MODIFY
    // Do not create constructors for this class
    // Developers cannot create instances of this class with the "new" keyword
    // You should write a static Create method instead; see the "Suggested Customization" region
    // Please review the documentation to learn about entity creation and inheritance

    // This private constructor prevents accidental instance creation via 'new'
    private Schedule() : this(null) { }

    // Typed DataSet constructor (needed by framework) - DO NOT REMOVE
    public Schedule(DataRowBuilder pRowBuilder)
      : base(pRowBuilder)
    {
    }

    #endregion

    #region Suggested Customizations

    //    // Use this factory method template to create new instances of this class
    //    public static Schedule Create(PersistenceManager pManager,
    //      ... your creation parameters here ...) { 
    //
    //      Schedule aSchedule = pManager.CreateEntity<Schedule>();
    //
    //      // if this object type requires a unique id and you have implemented
    //      // the IIdGenerator interface implement the following line
    //      pManager.GenerateId(aSchedule, // add id column here //);
    //
    //      // Add custom code here
    //
    //      aSchedule.AddToManager();
    //      return aSchedule;
    //    }

    //    // Implement this method if you want your object to sort in a specific order
    //    public override int CompareTo(Object pObject) {
    //    }

    //    // Implement this method to customize the null object version of this class
    //    protected override void UpdateNullEntity() {
    //    }

    #endregion

    // Add additional logic to your business object here...

    bool _isSeries = false;
    public bool Series
    {
      get
      {
        return _isSeries;
      }
      set
      {
        _isSeries = value;
      }
    }
    public void DeleteAll()
    {
      while (CanceledSchedules.Count > 0)
      {
        CanceledSchedules[0].Delete();
      }
      this.Delete();
    }

    public static Schedule Create()
    {
      Schedule schedule = (Schedule)DatabaseManager.Instance.CreateEntity(typeof(Schedule));
      DatabaseManager.Instance.GenerateId(schedule, Schedule.IdScheduleEntityColumn);

      schedule.ProgramName = "";
      schedule.Canceled = MinSchedule;
      //schedule.CanceledSchedules = rec.CanceledSchedules;
      schedule.Directory = "";
      schedule.EndTime = MinSchedule;
      schedule.KeepDate = MinSchedule;
      schedule.KeepMethod = (int)KeepMethodType.UntilSpaceNeeded;
      schedule.MaxAirings = 5;
      schedule.PostRecordInterval = 5;
      schedule.PreRecordInterval = 5;
      schedule.Priority = 0;
      schedule.Quality = 0;
      schedule.ScheduleType = (int)ScheduleRecordingType.Once;
      schedule.Series = false;
      schedule.StartTime = MinSchedule; 
      DatabaseManager.Instance.AddEntity(schedule);
      return schedule;
    }
    public static Schedule New()
    {
      Schedule schedule = (Schedule)DatabaseManager.Instance.CreateEntity(typeof(Schedule));
      schedule.ProgramName = "";
      schedule.Canceled = MinSchedule;
      //schedule.CanceledSchedules = rec.CanceledSchedules;
      schedule.Directory = "";
      schedule.EndTime = MinSchedule;
      schedule.KeepDate = MinSchedule;
      schedule.KeepMethod = (int)KeepMethodType.UntilSpaceNeeded;
      schedule.MaxAirings = 5;
      schedule.PostRecordInterval = 5;
      schedule.PreRecordInterval = 5;
      schedule.Priority = 0;
      schedule.Quality = 0;
      schedule.ScheduleType = (int)ScheduleRecordingType.Once;
      schedule.Series = false;
      schedule.StartTime = MinSchedule; 

      return schedule;
    }
    public static Schedule New(Schedule rec)
    {
      Schedule schedule = (Schedule)DatabaseManager.Instance.CreateEntity(typeof(Schedule));
      schedule.ProgramName = rec.ProgramName;
      schedule.Canceled = rec.Canceled;
      //schedule.CanceledSchedules = rec.CanceledSchedules;
      schedule.Channel = rec.Channel;
      schedule.Directory = rec.Directory;
      schedule.EndTime = rec.EndTime;
      schedule.KeepDate = rec.KeepDate;
      schedule.KeepMethod = rec.KeepMethod;
      schedule.MaxAirings = rec.MaxAirings;
      schedule.PostRecordInterval = rec.PostRecordInterval;
      schedule.PreRecordInterval = rec.PreRecordInterval;
      schedule.Priority = rec.Priority;
      schedule.Quality = rec.Quality;
      schedule.ScheduleType = rec.ScheduleType;
      schedule.Series = rec.Series;
      schedule.StartTime = rec.StartTime;

      return schedule;
    }
    /// <summary>
    /// Checks if the recording should record the specified tvprogram
    /// </summary>
    /// <param name="program">TVProgram to check</param>
    /// <returns>true if the specified tvprogram should be recorded</returns>
    /// <returns>filterCanceledRecordings (true/false)
    /// if true then  we'll return false if recording has been canceled for this program</returns>
    /// if false then we'll return true if recording has been not for this program</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public bool IsRecordingProgram(Program program, bool filterCanceledRecordings)
    {
      ScheduleRecordingType scheduleType = (ScheduleRecordingType)this.ScheduleType;
      switch (scheduleType)
      {
        case ScheduleRecordingType.Once:
          {
            if (program.StartTime == StartTime && program.EndTime == EndTime && program.Channel.IdChannel == Channel.IdChannel)
            {
              if (filterCanceledRecordings)
              {
                if (this.CanceledSchedules.Count > 0) return false;
              }
              return true;
            }
          }
          break;
        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          if (program.Title == ProgramName)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
            return true;
          }
          break;
        case ScheduleRecordingType.EveryTimeOnThisChannel:
          if (program.Title == ProgramName && program.Channel == Channel)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
            return true;
          }
          break;
        case ScheduleRecordingType.Daily:
          if (program.Channel == Channel)
          {
            int iHourProg = program.StartTime.Hour;
            int iMinProg = program.StartTime.Minute;
            if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
            {
              iHourProg = program.EndTime.Hour;
              iMinProg = program.EndTime.Minute;
              if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
              {
                if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
                return true;
              }
            }
          }
          break;
        case ScheduleRecordingType.WorkingDays:
          if (program.StartTime.DayOfWeek >= DayOfWeek.Monday && program.StartTime.DayOfWeek <= DayOfWeek.Friday)
          {
            if (program.Channel == Channel)
            {
              int iHourProg = program.StartTime.Hour;
              int iMinProg = program.StartTime.Minute;
              if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
              {
                iHourProg = program.EndTime.Hour;
                iMinProg = program.EndTime.Minute;
                if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
                {
                  if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
                  return true;
                }
              }
            }
          }
          break;

        case ScheduleRecordingType.Weekends:
          if (program.StartTime.DayOfWeek == DayOfWeek.Saturday || program.StartTime.DayOfWeek == DayOfWeek.Sunday)
          {
            if (program.Channel == Channel)
            {
              int iHourProg = program.StartTime.Hour;
              int iMinProg = program.StartTime.Minute;
              if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
              {
                iHourProg = program.EndTime.Hour;
                iMinProg = program.EndTime.Minute;
                if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
                {
                  if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
                  return true;
                }
              }
            }
          }
          break;

        case ScheduleRecordingType.Weekly:
          if (program.Channel == Channel)
          {
            int iHourProg = program.StartTime.Hour;
            int iMinProg = program.StartTime.Minute;
            if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
            {
              iHourProg = program.EndTime.Hour;
              iMinProg = program.EndTime.Minute;
              if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
              {
                if (StartTime.DayOfWeek == program.StartTime.DayOfWeek)
                {
                  if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
                  return true;
                }
              }
            }
          }
          break;
      }
      return false;
    }//IsRecordingProgram(TVProgram program, bool filterCanceledRecordings)

    public bool IsSerieIsCanceled(DateTime startTime)
    {
      foreach (CanceledSchedule schedule in CanceledSchedules)
      {
        if (schedule.CancelDateTime == startTime) return true;
      }
      return false;
    }
    public void UnCancelSerie(DateTime startTime)
    {
      foreach (CanceledSchedule schedule in CanceledSchedules)
      {
        if (schedule.CancelDateTime == startTime)
        {
          schedule.Delete();
          return;
        }
      }
      return;
    }

    public bool DoesUseEpisodeManagement
    {
      get
      {
        if (ScheduleType == (int)ScheduleRecordingType.Once) return false;
        if (MaxAirings == Int32.MaxValue) return false;
        if (MaxAirings < 1) return false;
        return true;
      }
    }
    /// <summary>
    /// Checks whether this recording is finished and can be deleted
    /// 
    /// </summary>
    /// <returns>true:Recording is finished can be deleted
    ///          false:Recording is not done yet, or needs to be done multiple times
    /// </returns>
    public bool IsDone()
    {
      if (ScheduleType != (int)ScheduleRecordingType.Once) return false;
      if (DateTime.Now > EndTime) return true;
      return false;
    }
  }

}
