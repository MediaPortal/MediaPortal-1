#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

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
    TimeShiftingUnParked
  } ;

  [Serializable]
  public class TvServerEventArgs : EventArgs
  {
    #region variables

    private readonly User _user;

    [NonSerialized]
    private readonly VirtualCard _card;

    [NonSerialized]
    private readonly IChannel _channel;

    [NonSerialized]
    private readonly IControllerService _controller = null;
    //Channel _databaseChannel = null;
    //TuningDetail _tuningDetail = null;

    private readonly int _schedule;
    private readonly int _recording;
    private int _conflict;
    // Added by Broce for exchanges between TVPlugin & ConflictsManager
    private IList<int> _schedules;
    private IList<int> _conflicts;
    private object _argsUpdatedState;

    [NonSerialized]
    private EpgChannel _epgChannel;
    //
    private readonly TvServerEventType _eventType;

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
    public TvServerEventArgs(TvServerEventType eventType, VirtualCard card, User user, int scheduleId,
                             int recordingId)
    {
      _eventType = eventType;
      _card = card;
      _user = user;
      _channel = channel;
      _schedule = scheduleId;
      _recording = recordingId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvServerEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="card">The card.</param>
    /// <param name="user">The user.</param>
    /// <param name="conflict">The conflict.</param>
    public TvServerEventArgs(TvServerEventType eventType, VirtualCard card, User user, int conflictId)
    {
      _eventType = eventType;
      _card = card;
      _user = user;
      _channel = channel;
      _conflict = conflictId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvServerEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="schedulesList">a IList of schedules</param>
    /// <param name="conflictsList">a IList of conflicts</param>
    /// <param name="argsUpdated">bool flag</param>
    public TvServerEventArgs(TvServerEventType eventType, IList<int> schedulesList, IList<int> conflictsList,
                             object argsUpdated)
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
    /// <param name="epgChannel">The epg channel</param>
    public TvServerEventArgs(TvServerEventType eventType, EpgChannel epgChannel)
    {
      _eventType = eventType;
      _epgChannel = epgChannel;
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets the controller.
    /// </summary>
    /// <value>The controller.</value>
    [System.Xml.Serialization.XmlIgnore]
    public IControllerService Controller
    {
      get { return _controller; }
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    /// <value>The user.</value>
    public User User
    {
      get { return _user; }
    }

    /// <summary>
    /// Gets the card.
    /// </summary>
    /// <value>The card.</value>
    [System.Xml.Serialization.XmlIgnore]
    public VirtualCard Card
    {
      get { return _card; }
    }

    /// <summary>
    /// Gets the channel.
    /// </summary>
    /// <value>The channel.</value>
    [System.Xml.Serialization.XmlIgnore]
    public IChannel channel
    {
      get { return _channel; }
    }

    /// <summary>
    /// Gets the recording.
    /// </summary>
    /// <value>The recording.</value>
    public int Recording
    {
      get { return _recording; }
    }

    /// <summary>
    /// Gets the conflict.
    /// </summary>
    /// <value>The conflict.</value>
    public int Conflict
    {
      get { return _conflict; }
      set { _conflict = value; }
    }

    /// <summary>
    /// Gets the schedule.
    /// </summary>
    /// <value>The schedule.</value>
    public int Schedule
    {
      get { return _schedule; }
    }

    // Added by Broce for exchanges between TVPlugin & ConflictsManager
    public IList<int> Schedules
    {
      get { return _schedules; }
      set { _schedules = value; }
    }

    public IList<int> Conflicts
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
    /// The received epgChannel
    /// </summary>
    [System.Xml.Serialization.XmlIgnore]
    public EpgChannel EpgChannel
    {
      get { return _epgChannel; }
      set { _epgChannel = value; }
    }

    //
    /// <summary>
    /// Gets the type of the event.
    /// </summary>
    /// <value>The type of the event.</value>
    public TvServerEventType EventType
    {
      get { return _eventType; }
    }

    #endregion
  }
}