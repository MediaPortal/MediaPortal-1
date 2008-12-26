/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Xml.Serialization;
using TvLibrary.Interfaces;
using System.Net;

namespace TvControl
{
  /// <summary>
  /// Virtual Card class
  /// This class provides methods and properties which a client can use
  /// The class will handle the communication and control with the
  /// tv service backend
  /// </summary>
  [Serializable]
  public class VirtualCard
  {
    #region variables
    string _server;
    string _recordingFolder;
    string _timeShiftFolder;
    int _recordingFormat;
    User _user;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualCard"/> class.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="server">The server.</param>
    /// <param name="recordingFormat">The recording format.</param>
    public VirtualCard(User user, string server, int recordingFormat)
    {
      _user = user;
      _server = server;
      _recordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
      _recordingFormat = recordingFormat;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualCard"/> class.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="server">The server.</param>
    public VirtualCard(User user, string server)
    {
      _user = user;
      _server = server;
      _recordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualCard"/> class.
    /// </summary>
    /// <param name="user">The user.</param>
    public VirtualCard(User user)
    {
      _user = user;
      _server = Dns.GetHostName();
      _recordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    }
    #endregion

    static void HandleFailure()
    {
      RemoteControl.Clear();
    }

    #region properties
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
    /// returns the card id of this virtual card
    /// </summary>
    public int Id
    {
      get
      {
        return _user.CardId;
      }
    }
    /// <summary>
    /// returns if the card is enabled;
    /// </summary>
    [XmlIgnore]
    public bool Enabled
    {
      get
      {
        if (User.CardId < 0)
          return false;
        try
        {
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.Enabled(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
    }

    /// <summary>
    /// gets the ip adress of the tvservice
    /// </summary>
    public string RemoteServer
    {
      get
      {
        return _server;
      }
      set
      {
        _server = value;
      }
    }
    ///<summary>
    /// Gets/Set the recording format
    ///</summary>
    public int RecordingFormat
    {
      get
      {
        return _recordingFormat;
      }
      set
      {
        _recordingFormat = value;
      }
    }

    /// <summary>
    /// gets/sets the recording folder for the card
    /// </summary>
    [XmlIgnore]
    public string RecordingFolder
    {
      get
      {
        return _recordingFolder;
      }
      set
      {
        _recordingFolder = value;
        if (_recordingFolder == String.Empty)
          _recordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

      }
    }

    /// <summary>
    /// gets/sets the timeshifting folder for the card
    /// </summary>
    [XmlIgnore]
    public string TimeshiftFolder
    {
      get
      {
        return _timeShiftFolder;
      }
      set
      {
        _timeShiftFolder = value;
        if (_timeShiftFolder == String.Empty)
          _timeShiftFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

      }
    }
    /// <summary>
    /// Gets the type of card (analog,dvbc,dvbs,dvbt,atsc)
    /// </summary>
    /// <value>cardtype</value>
    [XmlIgnore]
    public CardType Type
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return CardType.Analog;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.Type(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return CardType.Analog;
      }
    }

    /// <summary>
    /// Gets the name 
    /// </summary>
    /// <returns>name of card</returns>
    [XmlIgnore]
    public string Name
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return "";
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.CardName(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return "";
      }
    }

    /// <summary>
    /// Gets the device path 
    /// </summary>
    /// <returns>devicePath of card</returns>
    [XmlIgnore]
    public string Device
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return "";
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.CardDevice(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return "";
      }
    }


    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <returns>filename of the recording or null when not recording</returns>
    [XmlIgnore]
    public string RecordingFileName
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return "";
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.RecordingFileName(ref _user);
        } catch (Exception)
        {
          HandleFailure();
        }
        return "";
      }
    }

    /// <summary>
    /// gets/sets the current audio stream 
    /// </summary>
    /// <returns>current audio stream</returns>
    [XmlIgnore]
    public IAudioStream AudioStream
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return null;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.GetCurrentAudioStream(User);
        } catch (Exception)
        {
          HandleFailure();
        }
        return null;
      }
      set
      {
        try
        {
          if (User.CardId < 0)
            return;
          RemoteControl.HostName = _server;
          RemoteControl.Instance.SetCurrentAudioStream(User, value);
        } catch (Exception)
        {
          HandleFailure();
        }
      }
    }
    /// <summary>
    /// Gets the available audio streams.
    /// </summary>
    /// <value>The available audio streams.</value>
    [XmlIgnore]
    public IAudioStream[] AvailableAudioStreams
    {
      get
      {
        if (User.CardId < 0)
          return null;
        try
        {
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.AvailableAudioStreams(User);
        } catch (Exception)
        {
          HandleFailure();
        }
        return null;
      }
    }

    /// <summary>
    /// Gets the current video stream format.
    /// </summary>
    /// <value>The available audio streams.</value>
    public int GetCurrentVideoStream(User user)
    {
      if (User.CardId < 0)
        return -1;
      try
      {
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetCurrentVideoStream(user);
      } catch (Exception)
      {
        HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// returns which schedule is currently being recorded
    /// </summary>
    /// <returns>id of Schedule or -1 if  card not recording</returns>
    [XmlIgnore]
    public int RecordingScheduleId
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return -1;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.GetRecordingSchedule(User.CardId, User.IdChannel);
        } catch (Exception)
        {
          HandleFailure();
        }
        return -1;
      }
    }

    /// <summary>
    /// Returns the URL for the RTSP stream on which the client can find the
    /// stream 
    /// </summary>
    /// <returns>URL containing the RTSP adress on which the card transmits its stream</returns>
    [XmlIgnore]
    public string RTSPUrl
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return "";
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.GetStreamingUrl(User);
        } catch (Exception)
        {
          HandleFailure();
        }
        return "";
      }
    }


    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    [XmlIgnore]
    public bool GrabTeletext
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return false;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.IsGrabbingTeletext(User);
        } catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
      set
      {
        try
        {
          if (User.CardId < 0)
            return;
          RemoteControl.HostName = _server;
          RemoteControl.Instance.GrabTeletext(User, value);
        } catch (Exception)
        {
          HandleFailure();
        }
      }
    }

    /// <summary>
    /// Returns if the current channel has teletext or not
    /// </summary>
    /// <returns>yes if channel has teletext otherwise false</returns>
    [XmlIgnore]
    public bool HasTeletext
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return false;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.HasTeletext(User);
        } catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
    }

    /// <summary>
    /// Returns if we arecurrently grabbing the epg or not
    /// </summary>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    [XmlIgnore]
    public bool IsGrabbingEpg
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return false;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.IsGrabbingEpg(User.CardId);

        } catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
    }

    /// <summary>
    /// Returns if card is currently recording or not
    /// </summary>
    /// <returns>true when card is recording otherwise false</returns>
    [XmlIgnore]
    public bool IsRecording
    {
      get
      {
        try
        {
          //if (User.CardId < 0) return false;
          RemoteControl.HostName = _server;
          //return RemoteControl.Instance.IsRecording(ref _user); //we will never get anything useful out of this, since the rec user is called schedulerxyz and not ex. user.name = htpc
          VirtualCard vc;
          bool isRec = RemoteControl.Instance.IsRecording(ChannelName, out vc);
          return (isRec && vc.Id == Id && vc.User.IsAdmin);
        } catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
    }

    /// <summary>
    /// Returns the reason as to why TV timeshifting stopped
    /// </summary>		
    [XmlIgnore]
    public TvStoppedReason GetTimeshiftStoppedReason
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return TvStoppedReason.UnknownReason;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.GetTvStoppedReason(_user);
        } catch (Exception)
        {
          HandleFailure();
        }
        return TvStoppedReason.UnknownReason;
      }
    }

    /// <summary>
    /// Returns if card is currently scanning or not
    /// </summary>
    /// <returns>true when card is scanning otherwise false</returns>
    [XmlIgnore]
    public bool IsScanning
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return false;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.IsScanning(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
    }

    /// <summary>
    /// Returns whether the current channel is scrambled or not.
    /// </summary>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    [XmlIgnore]
    public bool IsScrambled
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return false;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.IsScrambled(ref _user);
        } catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
    }

    /// <summary>
    /// Returns if card is currently timeshifting or not
    /// </summary>
    /// <returns>true when card is timeshifting otherwise false</returns>
    [XmlIgnore]
    public bool IsTimeShifting
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return false;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.IsTimeShifting(ref _user);
        } catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
    }

    /// <summary>
    /// returns the minium channel number
    /// </summary>
    /// <returns>minium channel number</returns>
    [XmlIgnore]
    public int MinChannel
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return 0;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.MinChannel(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return 0;
      }
    }

    /// <summary>
    /// returns the maximum channel number
    /// </summary>
    /// <returns>maximum channel number</returns>
    [XmlIgnore]
    public int MaxChannel
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return 0;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.MaxChannel(User.CardId);

        } catch (Exception)
        {
          HandleFailure();
        }
        return 0;
      }
    }
    /// <summary>
    /// Returns the current filename used for timeshifting
    /// </summary>
    /// <returns>timeshifting filename null when not timeshifting</returns>
    [XmlIgnore]
    public string TimeShiftFileName
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return "";
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.TimeShiftFileName(ref _user);
        } catch (Exception)
        {
          HandleFailure();
        }
        return "";
      }
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started 
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    [XmlIgnore]
    public DateTime TimeShiftStarted
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return DateTime.MinValue;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.TimeShiftStarted(User);
        } catch (Exception)
        {
          HandleFailure();
        }
        return DateTime.MinValue;
      }
    }

    /// <summary>
    /// returns the date/time when recording has been started 
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    [XmlIgnore]
    public DateTime RecordingStarted
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return DateTime.MinValue;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.RecordingStarted(User);
        } catch (Exception)
        {
          HandleFailure();
        }
        return DateTime.MinValue;
      }
    }

    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <returns>true if tuner is locked otherwise false</returns>
    [XmlIgnore]
    public bool IsTunerLocked
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return false;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.TunerLocked(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return false;
      }
    }

    /// <summary>
    /// Gets the name of the tv/radio channel to which we are tuned
    /// </summary>
    /// <returns>channel name</returns>
    [XmlIgnore]
    public string ChannelName
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return "";
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.CurrentChannelName(ref _user);
        } catch (Exception)
        {
          HandleFailure();
        }
        return "";
      }
    }


    /// <summary>
    /// Gets the of the tv/radio channel to which we are tuned
    /// </summary>
    /// <returns>channel name</returns>
    [XmlIgnore]
    public IChannel Channel
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return null;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.CurrentChannel(ref _user);
        } catch (Exception)
        {
          HandleFailure();
        }
        return null;
      }
    }

    /// <summary>
    /// returns the database channel
    /// </summary>
    /// <returns>int</returns>
    [XmlIgnore]
    public int IdChannel
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return -1;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.CurrentDbChannel(ref _user);
        } catch (Exception)
        {
          HandleFailure();
        }
        return -1;
      }
    }

    /// <summary>
    /// Returns the signal level 
    /// </summary>
    /// <returns>signal level (0-100)</returns>
    [XmlIgnore]
    public int SignalLevel
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return 0;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.SignalLevel(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return 0;
      }
    }

    /// <summary>
    /// Returns the signal quality 
    /// </summary>
    /// <returns>signal quality (0-100)</returns>
    [XmlIgnore]
    public int SignalQuality
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return 0;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.SignalQuality(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return 0;
      }
    }
    #endregion

    #region methods
    /// <summary>
    /// Gets a raw teletext page.
    /// </summary>
    /// <param name="pageNumber">The page number. (0x100-0x899)</param>
    /// <param name="subPageNumber">The sub page number.(0x0-0x79)</param>
    /// <returns>byte[] array containing the raw teletext page or null if page is not found</returns>
    public byte[] GetTeletextPage(int pageNumber, int subPageNumber)
    {
      try
      {
        if (User.CardId < 0)
          return new byte[] { 1 };
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetTeletextPage(User, pageNumber, subPageNumber);
      } catch (Exception)
      {
        HandleFailure();
      }
      return new byte[] { 1 };
    }

    /// <summary>
    /// scans current transponder for channels.
    /// </summary>
    /// <returns>list of all channels found</returns>
    public IChannel[] Scan(IChannel channel)
    {
      try
      {
        if (User.CardId < 0)
          return null;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.Scan(User.CardId, channel);
      } catch (Exception)
      {
        HandleFailure();
      }
      return null;
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    public void StopTimeShifting()
    {
      try
      {
        if (User.CardId < 0)
          return;
        //if (IsRecording) return;
        if (IsTimeShifting == false)
          return;
        RemoteControl.HostName = _server;
        RemoteControl.Instance.StopTimeShifting(ref _user);
      } catch (Exception)
      {
        HandleFailure();
      }
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <returns>true if success otherwise false</returns>
    public void StopRecording()
    {
      try
      {
        if (User.CardId < 0)
          return;
        RemoteControl.HostName = _server;
        RemoteControl.Instance.StopRecording(ref _user);
      } catch (Exception)
      {
        HandleFailure();
      }
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="fileName">Name of the recording file.</param>
    /// <param name="contentRecording">not used</param>
    /// <param name="startTime">not used</param>
    /// <returns>true if success otherwise false</returns>
    public bool StartRecording(ref string fileName, bool contentRecording, long startTime)
    {
      try
      {
        if (User.CardId < 0)
          return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.StartRecording(ref _user, ref fileName, contentRecording, startTime);
      } catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Determines whether the card is locked.
    /// </summary>
    /// <param name="user">The user which has locked the card</param>
    /// <returns>
    /// 	<c>true</c> if the card is locked; otherwise, <c>false</c>.
    /// </returns>
    public bool IsLocked(out User user)
    {
      user = null;
      try
      {
        if (User.CardId < 0)
          return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.IsCardInUse(User.CardId, out user);
      } catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="pageNumber">The page number (0x100-0x899)</param>
    /// <returns>number of teletext subpages for the pagenumber</returns>
    public int SubPageCount(int pageNumber)
    {
      try
      {
        if (User.CardId < 0)
          return -1;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.SubPageCount(User, pageNumber);
      } catch (Exception)
      {
        HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <returns>Teletext pagenumber for the red button</returns>
    public int GetTeletextRedPageNumber()
    {
      try
      {
        if (User.CardId < 0)
          return -1;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetTeletextRedPageNumber(User);
      } catch (Exception)
      {
        HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <returns>Teletext pagenumber for the green button</returns>
    public int GetTeletextGreenPageNumber()
    {
      try
      {
        if (User.CardId < 0)
          return -1;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetTeletextGreenPageNumber(User);
      } catch (Exception)
      {
        HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    public int GetTeletextYellowPageNumber()
    {
      try
      {
        if (User.CardId < 0)
          return -1;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetTeletextYellowPageNumber(User);
      } catch (Exception)
      {
        HandleFailure();
      }
      return -1;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <returns>Teletext pagenumber for the blue button</returns>
    public int GetTeletextBluePageNumber()
    {
      try
      {
        if (User.CardId < 0)
          return -1;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetTeletextBluePageNumber(User);
      } catch (Exception)
      {
        HandleFailure();
      }
      return -1;
    }

    /// <summary>f
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(int pageNumber)
    {
      try
      {
        if (User.CardId < 0)
          return new TimeSpan(0, 0, 0, 15);
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.TeletextRotation(User, pageNumber);

      } catch (Exception)
      {
        HandleFailure();
      }
      return new TimeSpan(0, 0, 0, 15);
    }

    /// <summary>
    /// Finds out whether a channel is currently tuneable or not
    /// </summary>
    /// <param name="idChannel">the channel id</param>
    /// <param name="user">User</param>
    /// <returns>an enum indicating tunable/timeshifting/recording</returns>
    public ChannelState GetChannelState(int idChannel, User user)
    {
      try
      {
        if (User.CardId < 0)
          return ChannelState.nottunable;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.GetChannelState(idChannel, user);
      } catch (Exception)
      {
        HandleFailure();
      }
      return ChannelState.nottunable;
    }
    #endregion

    #region quality control
    /// <summary>
    /// Indicates, if the user is the owner of the card
    /// </summary>
    /// <returns>true/false</returns>
    public bool IsOwner()
    {
      try
      {
        if (User.CardId < 0)
          return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.IsOwner(User.CardId, User);
      } catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }


    /// <summary>
    /// Indicates, if the card supports quality control
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsQualityControl()
    {
      try
      {
        if (User.CardId < 0)
          return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.SupportsQualityControl(User.CardId);
      } catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports bit rates
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsBitRate()
    {
      try
      {
        if (User.CardId < 0)
          return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.SupportsBitRate(User.CardId);
      } catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports bit rate modes 
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsBitRateModes()
    {
      try
      {
        if (User.CardId < 0)
          return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.SupportsBitRateModes(User.CardId);
      } catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Indicates, if the card supports bit rate peak mode
    /// </summary>
    /// <returns>true/false</returns>
    public bool SupportsPeakBitRateMode()
    {
      try
      {
        if (User.CardId < 0)
          return false;
        RemoteControl.HostName = _server;
        return RemoteControl.Instance.SupportsPeakBitRateMode(User.CardId);
      } catch (Exception)
      {
        HandleFailure();
      }
      return false;
    }

    /// <summary>
    /// Gets/Sts the quality type
    /// </summary>
    public QualityType QualityType
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return QualityType.Default;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.GetQualityType(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return QualityType.Default;
      }
      set
      {
        try
        {
          if (User.CardId < 0)
            return;
          RemoteControl.HostName = _server;
          RemoteControl.Instance.SetQualityType(User.CardId, value);
        } catch (Exception)
        {
          HandleFailure();
        }
      }
    }

    /// <summary>
    /// Gets/Sts the bitrate mode
    /// </summary>
    public VIDEOENCODER_BITRATE_MODE BitRateMode
    {
      get
      {
        try
        {
          if (User.CardId < 0)
            return VIDEOENCODER_BITRATE_MODE.Undefined;
          RemoteControl.HostName = _server;
          return RemoteControl.Instance.GetBitRateMode(User.CardId);
        } catch (Exception)
        {
          HandleFailure();
        }
        return VIDEOENCODER_BITRATE_MODE.Undefined;
      }
      set
      {
        try
        {
          if (User.CardId < 0)
            return;
          RemoteControl.HostName = _server;
          RemoteControl.Instance.SetBitRateMode(User.CardId, value);
        } catch (Exception)
        {
          HandleFailure();
        }
      }
    }

    #endregion
  }
}
