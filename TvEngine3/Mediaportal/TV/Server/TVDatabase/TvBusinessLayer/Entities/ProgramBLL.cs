using System;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities
{
  public class ProgramBLL
  {        
    private readonly Program _entity;

    public ProgramBLL(Program entity)
    {      
      _entity = entity;      
    }

    public Program Entity
    {
      get { return _entity; }
    }

    private void SetFlag(ProgramState flag, bool isSet)
    {
      ProgramState newState = (ProgramState)_entity.State;
      if (!isSet && newState.HasFlag(flag))
      {
        newState ^= flag;
        _entity.State = (int)newState;
      }
      else if (isSet && !newState.HasFlag(flag))
      {
        newState |= flag;
        _entity.State = (int)newState;
      }
    }

    /// <summary>
    /// Property relating to database column notify
    /// </summary>
    public bool Notify
    {
      get { return ((ProgramState)_entity.State).HasFlag(ProgramState.Notify); }
      set { SetFlag(ProgramState.Notify, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingOnce
    /// </summary>
    public bool IsRecordingOnce
    {
      get { return ((ProgramState)_entity.State).HasFlag(ProgramState.RecordOnce); }
      set { SetFlag(ProgramState.RecordOnce, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingSeriesPending
    /// </summary>
    public bool IsRecordingSeriesPending
    {
      get { return ((ProgramState)_entity.State).HasFlag(ProgramState.RecordSeriesPending); }
      set { SetFlag(ProgramState.RecordSeriesPending, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingSeriesPending
    /// </summary>
    public bool IsPartialRecordingSeriesPending
    {
      get { return ((ProgramState)_entity.State).HasFlag(ProgramState.PartialRecordSeriesPending); }
      set { SetFlag(ProgramState.PartialRecordSeriesPending, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingOncePending
    /// </summary>
    public bool IsRecordingOncePending
    {
      get { return ((ProgramState)_entity.State).HasFlag(ProgramState.RecordOncePending); }
      set { SetFlag(ProgramState.RecordOncePending, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingManual
    /// </summary>
    public bool IsRecordingManual
    {
      get { return ((ProgramState)_entity.State).HasFlag(ProgramState.RecordManual); }
      set { SetFlag(ProgramState.RecordManual, value); }
    }

    /// <summary>
    /// Property relating to database column isRecording
    /// </summary>
    public bool IsRecordingSeries
    {
      get { return ((ProgramState)_entity.State).HasFlag(ProgramState.RecordSeries); }
      set { SetFlag(ProgramState.RecordSeries, value); }
    }

    /// <summary>
    /// Property relating to database column conflict
    /// </summary>
    public bool HasConflict
    {
      get { return ((ProgramState)_entity.State).HasFlag(ProgramState.Conflict); }
      set { SetFlag(ProgramState.Conflict, value); }
    }

    /// <summary>
    /// Property relating to database column IsRecording
    /// </summary>
    public bool IsRecording
    { // This should reflect whether program is actually recording right now
      get { return (IsRecordingSeries || IsRecordingManual || IsRecordingOnce); }
    }

    public void ClearRecordPendingState()
    {
      _entity.State &=
        ~(int)
         (ProgramState.RecordOncePending | ProgramState.RecordSeriesPending | ProgramState.PartialRecordSeriesPending);
    }

    /// <summary>
    /// Checks if the program ended prior to the specified date/time
    /// </summary>
    /// <param name="tCurTime">date and time</param>
    /// <returns>true if program ended prior to tCurTime</returns>
    public bool EndedBefore(DateTime tCurTime)
    {
      return _entity.EndTime <= tCurTime;
    }

    /// <summary>
    /// Checks if the program is running between the specified start and end time/dates, i.e. whether the intervals overlap
    /// </summary>
    /// <param name="tStartTime">Start date and time</param>
    /// <param name="tEndTime">End date and time</param>
    /// <returns>true if program is running between tStartTime-tEndTime</returns>
    public bool RunningAt(DateTime tStartTime, DateTime tEndTime)
    {
      // do NOT use <= >= since end-times are non-including
      return tStartTime < _entity.EndTime && tEndTime > _entity.StartTime;
    }

    /// <summary>
    /// Checks if the program is running at the specified date/time
    /// </summary>
    /// <param name="tCurTime">date and time</param>
    /// <returns>true if program is running at tCurTime</returns>
    public bool IsRunningAt(DateTime tCurTime)
    {
      return tCurTime >= _entity.StartTime && tCurTime <= _entity.EndTime;
    }
  }
}