using System;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

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

    /// <summary>
    /// Property relating to database column notify
    /// </summary>
    public bool Notify
    {
      get { return ((_entity.state & (int)ProgramState.Notify) == (int)ProgramState.Notify); }
      set
      {
        ProgramState newState = (ProgramState)_entity.state;
        if (!value && (_entity.state & (int)ProgramState.Notify) == (int)ProgramState.Notify)
        // remove the notify bit flag if present
        {
          newState ^= ProgramState.Notify;
        }
        if (value) //add the notify bit flag
        {
          newState |= ProgramState.Notify;
        }
        
        _entity.state = (int)newState;
      }
    }

    /// <summary>
    /// Property relating to database column IsRecording
    /// </summary>
    public bool IsRecording
    { // This should reflect whether program is actually recording right now
      get { return (IsRecordingSeries || IsRecordingManual || IsRecordingOnce); }
    }

    /// <summary>
    /// Property relating to database column IsRecordingOnce
    /// </summary>
    public bool IsRecordingOnce
    {
      get { return ((_entity.state & (int)ProgramState.RecordOnce) == (int)ProgramState.RecordOnce); }
      set
      {
        ProgramState newState = (ProgramState)_entity.state;
        if (!value && (_entity.state & (int)ProgramState.RecordOnce) == (int)ProgramState.RecordOnce)
        // remove the Record bit flag if present                        
        {
          newState ^= ProgramState.RecordOnce;
        }
        if (value) //add the Record bit flag
        {
          newState |= ProgramState.RecordOnce;
        }

        _entity.state = (int)newState;
      }
    }


    /// <summary>
    /// Property relating to database column IsRecordingSeriesPending
    /// </summary>
    public bool IsRecordingSeriesPending
    {
      get { return ((_entity.state & (int)ProgramState.RecordSeriesPending) == (int)ProgramState.RecordSeriesPending); }
      set
      {
        ProgramState newState = (ProgramState)_entity.state;
        if (!value && (_entity.state & (int)ProgramState.RecordSeriesPending) == (int)ProgramState.RecordSeriesPending)
        // remove the Record bit flag if present                
        {
          newState ^= ProgramState.RecordSeriesPending;
        }
        if (value) //add the Record bit flag
        {
          newState |= ProgramState.RecordSeriesPending;
        }

        _entity.state = (int)newState;
      }
    }

    /// <summary>
    /// Property relating to database column IsRecordingSeriesPending
    /// </summary>
    public bool IsPartialRecordingSeriesPending
    {
      get { return (_entity != null && (_entity.state & (int)ProgramState.PartialRecordSeriesPending) == (int)ProgramState.PartialRecordSeriesPending); }
      set
      {
        ProgramState newState = (ProgramState)_entity.state;
        if (value) //add the Record bit flag
        {
          newState |= ProgramState.PartialRecordSeriesPending;
        }
        else
        {
          newState &= ~ProgramState.PartialRecordSeriesPending;
        }
        _entity.state = (int)newState;
      }
    }

    /// <summary>
    /// Property relating to database column IsRecordingOncePending
    /// </summary>
    public bool IsRecordingOncePending
    {
      get { return ((_entity.state & (int)ProgramState.RecordOncePending) == (int)ProgramState.RecordOncePending); }
      set
      {
        ProgramState newState = (ProgramState)_entity.state;
        if (!value && (_entity.state & (int)ProgramState.RecordOncePending) == (int)ProgramState.RecordOncePending)
        // remove the Record bit flag if present        
        {
          newState ^= ProgramState.RecordOncePending;
        }
        if (value) //add the Record bit flag
        {
          newState |= ProgramState.RecordOncePending;
        }

        _entity.state = (int)newState;
      }
    }

    /// <summary>
    /// Property relating to database column IsRecordingManual
    /// </summary>
    public bool IsRecordingManual
    {
      get { return ((_entity.state & (int)ProgramState.RecordManual) == (int)ProgramState.RecordManual); }
      set
      {
        ProgramState newState = (ProgramState)_entity.state;
        if (!value && (_entity.state & (int)ProgramState.RecordManual) == (int)ProgramState.RecordManual)
        // remove the Record bit flag if present
        {
          newState ^= ProgramState.RecordManual;
        }
        if (value) //add the Record bit flag
        {
          newState |= ProgramState.RecordManual;
        }

        _entity.state = (int)newState;
      }
    }

    public void ClearRecordPendingState()
    {
      _entity.state &=
        ~(int)
         (ProgramState.RecordOncePending | ProgramState.RecordSeriesPending | ProgramState.PartialRecordSeriesPending);
    }


    /// <summary>
    /// Property relating to database column isRecording
    /// </summary>
    public bool IsRecordingSeries
    {
      get { return ((_entity.state & (int)ProgramState.RecordSeries) == (int)ProgramState.RecordSeries); }
      set
      {
        ProgramState newState = (ProgramState)_entity.state;
        if (!value && (_entity.state & (int)ProgramState.RecordSeries) == (int)ProgramState.RecordSeries)
        // remove the RecordSeries bit flag if present
        {
          newState ^= ProgramState.RecordSeries;
        }
        if (value) //add the RecordSeries bit flag
        {
          newState |= ProgramState.RecordSeries;
        }
        
        _entity.state = (int)newState;
      }
    }

    /// <summary>
    /// Property relating to database column conflict
    /// </summary>
    public bool HasConflict
    {
      get { return ((_entity.state & (int)ProgramState.Conflict) == (int)ProgramState.Conflict); }
      set
      {
        ProgramState newState = (ProgramState)_entity.state;
        if (!value && (_entity.state & (int)ProgramState.Conflict) == (int)ProgramState.Conflict)
        // remove the Conflict bit flag if present
        {
          newState ^= ProgramState.Conflict;
        }
        if (value) //add the Conflict bit flag
        {
          newState |= ProgramState.Conflict;
        }

        _entity.state = (int)newState;
      }
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
      return tStartTime < _entity.endTime && tEndTime > _entity.startTime;
    }

    /// <summary>
    /// Checks if the program is running at the specified date/time
    /// </summary>
    /// <param name="tCurTime">date and time</param>
    /// <returns>true if program is running at tCurTime</returns>
    public bool IsRunningAt(DateTime tCurTime)
    {
      bool bRunningAt = tCurTime >= _entity.startTime && tCurTime <= _entity.endTime;
      return bRunningAt;
    }
  }
}
