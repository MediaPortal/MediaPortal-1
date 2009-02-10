/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using TvControl;
using TvLibrary.Interfaces;
using TvDatabase;
using TvLibrary.Epg;

namespace TvEngine.Events
{
  /// <summary>
  /// Enum for the different event types
  /// </summary>
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
  };

  public class TvServerEventArgs : EventArgs
  {
    #region variables

    readonly User _user;
    readonly VirtualCard _card;
    readonly IChannel _channel;
    IController _controller = null;
    //Channel _databaseChannel = null;
    //TuningDetail _tuningDetail = null;
    readonly Schedule _schedule;
    readonly Recording _recording;
    Conflict _conflict;
    // Added by Broce for exchanges between TVPlugin & ConflictsManager
    IList<Schedule> _schedules;
    IList<Conflict> _conflicts;
    object _argsUpdatedState;
    private IList<EpgProgram> _epgPrograms;
    //
    readonly TvServerEventType _eventType;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvServerEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">Type of the _event.</param>
    public TvServerEventArgs(TvServerEventType eventType)
    {
      _eventType = eventType;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TvServerEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">Type of the _event.</param>
    /// <param name="card">The card.</param>
    /// <param name="user">The user.</param>
    public TvServerEventArgs(TvServerEventType eventType, VirtualCard card, User user)
    {
      _eventType = eventType;
      _card = card;
      _user = user;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TvServerEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">Type of the _event.</param>
    /// <param name="card">The card.</param>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    public TvServerEventArgs(TvServerEventType eventType, VirtualCard card, User user, IChannel channel)
    {
      _eventType = eventType;
      _card = card;
      _user = user;
      _channel = channel;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TvServerEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="card">The card.</param>
    /// <param name="user">The user.</param>
    /// <param name="schedule">The schedule.</param>
    /// <param name="recording">The recording.</param>
    public TvServerEventArgs(TvServerEventType eventType, VirtualCard card, User user, Schedule schedule, Recording recording)
    {
      _eventType = eventType;
      _card = card;
      _user = user;
      _channel = channel;
      _schedule = schedule;
      _recording = recording;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TvServerEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="card">The card.</param>
    /// <param name="user">The user.</param>
    /// <param name="conflict">The conflict.</param>
    public TvServerEventArgs(TvServerEventType eventType, VirtualCard card, User user, Conflict conflict)
    {
      _eventType = eventType;
      _card = card;
      _user = user;
      _channel = channel;
      _conflict = conflict;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TvServerEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="schedulesList">a IList of schedules</param>
    /// <param name="conflictsList">a IList of conflicts</param>
    /// <param name="argsUpdated">bool flag</param>
    public TvServerEventArgs(TvServerEventType eventType, IList<Schedule> schedulesList, IList<Conflict> conflictsList, object argsUpdated)
    {
      _eventType = eventType;
      _schedules = schedulesList;
      _conflicts = conflictsList;
      _argsUpdatedState = argsUpdated;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TvServerEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="epgPrograms">An IList of the new programs to import.</param>
    public TvServerEventArgs(TvServerEventType eventType, IList<EpgProgram> epgPrograms)
    {
      _eventType = eventType;
      _epgPrograms = epgPrograms;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the controller.
    /// </summary>
    /// <value>The controller.</value>
    public IController Controller
    {
      get
      {
        return _controller;
      }
    }
    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <value>The user.</value>
    public User User
    {
      get
      {
        return _user;
      }
    }

    /// <summary>
    /// Gets the card.
    /// </summary>
    /// <value>The card.</value>
    public VirtualCard Card
    {
      get
      {
        return _card;
      }
    }

    /// <summary>
    /// Gets the channel.
    /// </summary>
    /// <value>The channel.</value>
    public IChannel channel
    {
      get
      {
        return _channel;
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

    /// <summary>
    /// Gets the conflict.
    /// </summary>
    /// <value>The conflict.</value>
    public Conflict Conflict
    {
      get
      {
        return _conflict;
      }
      set
      {
        _conflict = value;
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
    // Added by Broce for exchanges between TVPlugin & ConflictsManager
    public IList<Schedule> Schedules
    {
      get { return _schedules; }
      set { _schedules = value; }
    }
    public IList<Conflict> Conflicts
    {
      get { return _conflicts; }
      set { _conflicts = value; }
    }
    public object ArgsUpdatedState
    {
      get { return _argsUpdatedState; }
      set { _argsUpdatedState = value; }
    }
    /// <summary>
    /// List of recieved EPG programs.
    /// </summary>
    public IList<EpgProgram> EpgPrograms
    {
      get { return _epgPrograms; }
      set { _epgPrograms = value; }
    }
    //
    /// <summary>
    /// Gets the type of the event.
    /// </summary>
    /// <value>The type of the event.</value>
    public TvServerEventType EventType
    {
      get
      {
        return _eventType;
      }
    }
    #endregion
  }
}
