using System;

namespace Mediaportal.TV.Server.TVControl.Events
{
  /// <summary>
  /// Enum for the different event types
  /// </summary>
  [Serializable]
  public enum TvServerEventType
  {
    /// <summary>
    /// Event indicating that the tvserver is going to zap to a different channel
    /// </summary>
    StartZapChannel,
    /// <summary>
    /// Event indicating that the tvserver is has zapped to a different channel
    /// </summary>
    EndZapChannel,
    /// <summary>
    /// Event indicating that the tvserver is going to start timeshifting
    /// </summary>
    StartTimeShifting,
    /// <summary>
    /// Event indicating that the tvserver is going to start timeshifting
    /// </summary>
    EndTimeShifting,
    /// <summary>
    /// Event indicating that the tvserver has forcefully stopped timeshifting
    /// </summary>
    ForcefullyStoppedTimeShifting,
    /// <summary>
    /// Event indicating that the tvserver is going to stop timeshifting
    /// </summary>
    StartRecording,
    /// <summary>
    /// Event indicating that the tvserver is recording has begun
    /// </summary>
    RecordingStarted,
    /// <summary>
    /// Event indicating that the tvserver is recording has stopped
    /// </summary>
    RecordingFailed,
    /// <summary>
    /// Event indicating that the tvserver recording failed
    /// </summary>
    RecordingEnded,
    /// <summary>
    /// Event indicating that a new schedule has been added
    /// </summary>
    ScheduledAdded,
    /// <summary>
    /// Event indicating that a  schedule has been deleted
    /// </summary>
    ScheduleDeleted,
    /// <summary>
    /// Event indicating that a new conflict has been added
    /// </summary>
    ConflictAdded,
    /// <summary>
    /// Event indicating that a  conflict has been deleted
    /// </summary>
    ConflictDeleted,
    /// <summary>
    /// Event indicating that the program db was updated
    /// </summary>
    ProgramUpdated,
    /// <summary>
    /// Event indicating that new EPG data is about to be imported
    /// </summary>
    ImportEpgPrograms,
    /// <summary>
    /// Event indicating that new channelstate data is available
    /// </summary>
    ChannelStatesChanged,
    /// <summary>
    /// Event indicating that timeshifting was parked by user
    /// </summary>
    TimeShiftingParked,
    /// <summary>
    /// Event indicating that timeshifting was unparked by user
    /// </summary>
    TimeShiftingUnParked,
    /// <summary>
    /// 
    /// </summary>
    EpgGrabbingStarted,
    /// <summary>
    /// 
    /// </summary>
    EpgGrabbingStopped,
    /// <summary>
    /// 
    /// </summary>
    ScanningStarted,
    /// <summary>
    /// 
    /// </summary>
    ScanningStopped
  } ;
}