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
using System.Linq;
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri.Service
{
  public enum UpnpAvTransportState
  {
    STOPPED,
    PLAYING,
    TRANSITIONING,
    PAUSED_PLAYBACK,
    PAUSED_RECORDING,
    RECORDING,
    NO_MEDIA_PRESENT
  }

  public enum UpnpAvTransportStatus
  {
    OK,
    ERROR_OCCURRED
  }

  public enum UpnpAvTransportAction
  {
    Play,
    Stop,
    Pause,
    Seek,
    Next,
    Previous,
    Record
  }

  public sealed class UpnpAvStorageMedium
  {
    private readonly string _name;
    private static readonly IDictionary<string, UpnpAvStorageMedium> _values = new Dictionary<string, UpnpAvStorageMedium>();

    public static readonly UpnpAvStorageMedium Unknown = new UpnpAvStorageMedium("UNKNOWN");
    public static readonly UpnpAvStorageMedium Dv = new UpnpAvStorageMedium("DV");
    public static readonly UpnpAvStorageMedium MiniDv = new UpnpAvStorageMedium("MINI-DV");
    public static readonly UpnpAvStorageMedium Vhs = new UpnpAvStorageMedium("VHS");
    public static readonly UpnpAvStorageMedium Wvhs = new UpnpAvStorageMedium("W-VHS");
    public static readonly UpnpAvStorageMedium Svhs = new UpnpAvStorageMedium("S-VHS");
    public static readonly UpnpAvStorageMedium Dvhs = new UpnpAvStorageMedium("D-VHS");
    public static readonly UpnpAvStorageMedium Vhsc = new UpnpAvStorageMedium("VHSC");
    public static readonly UpnpAvStorageMedium Video8 = new UpnpAvStorageMedium("VIDEO8");
    public static readonly UpnpAvStorageMedium Hi8 = new UpnpAvStorageMedium("HI8");
    public static readonly UpnpAvStorageMedium Cdrom = new UpnpAvStorageMedium("CD-ROM");
    public static readonly UpnpAvStorageMedium Cdda = new UpnpAvStorageMedium("CD-DA");
    public static readonly UpnpAvStorageMedium Cdr = new UpnpAvStorageMedium("CD-R");
    public static readonly UpnpAvStorageMedium Cdrw = new UpnpAvStorageMedium("CD-RW");
    public static readonly UpnpAvStorageMedium VideoCd = new UpnpAvStorageMedium("VIDEO-CD");
    public static readonly UpnpAvStorageMedium Sacd = new UpnpAvStorageMedium("SACD");
    public static readonly UpnpAvStorageMedium MdAudio = new UpnpAvStorageMedium("MD-AUDIO");
    public static readonly UpnpAvStorageMedium MdPicture = new UpnpAvStorageMedium("MD-PICTURE");
    public static readonly UpnpAvStorageMedium Dvdrom = new UpnpAvStorageMedium("DVD-ROM");
    public static readonly UpnpAvStorageMedium DvdVideo = new UpnpAvStorageMedium("DVD-VIDEO");
    public static readonly UpnpAvStorageMedium Dvdr = new UpnpAvStorageMedium("DVD-R");
    public static readonly UpnpAvStorageMedium DvdPlusRw = new UpnpAvStorageMedium("DVD+RW");
    public static readonly UpnpAvStorageMedium DvdMinusRw = new UpnpAvStorageMedium("DVD-RW");
    public static readonly UpnpAvStorageMedium Dvdram = new UpnpAvStorageMedium("DVD-RAM");
    public static readonly UpnpAvStorageMedium DvdAudio = new UpnpAvStorageMedium("DVD-AUDIO");
    public static readonly UpnpAvStorageMedium Dat = new UpnpAvStorageMedium("DAT");
    public static readonly UpnpAvStorageMedium Ld = new UpnpAvStorageMedium("LD");
    public static readonly UpnpAvStorageMedium Hdd = new UpnpAvStorageMedium("HDD");
    public static readonly UpnpAvStorageMedium MicroMv = new UpnpAvStorageMedium("MICRO-MV");
    public static readonly UpnpAvStorageMedium Network = new UpnpAvStorageMedium("NETWORK");
    public static readonly UpnpAvStorageMedium None = new UpnpAvStorageMedium("NONE");
    public static readonly UpnpAvStorageMedium NotImplemented = new UpnpAvStorageMedium("NOT_IMPLEMENTED");

    private UpnpAvStorageMedium(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      UpnpAvStorageMedium medium = obj as UpnpAvStorageMedium;
      if (medium != null && this == medium)
      {
        return true;
      }
      return false;
    }

    public static explicit operator UpnpAvStorageMedium(string name)
    {
      UpnpAvStorageMedium value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(UpnpAvStorageMedium medium)
    {
      return medium._name;
    }
  }

  public enum UpnpAvCurrentPlayMode
  {
    NORMAL,
    SHUFFLE,
    REPEAT_ONE,
    REPEAT_ALL,
    RANDOM,
    DIRECT_I,
    INTRO
  }

  public enum UpnpAvRecordMediumWriteStatus
  {
    WRITABLE,
    PROTECTED,
    NOT_WRITABLE,
    UNKNOWN,
    NOT_IMPLEMENTED
  }

  public sealed class UpnpAvRecordQualityMode
  {
    private readonly string _name;
    private static readonly IDictionary<string, UpnpAvRecordQualityMode> _values = new Dictionary<string, UpnpAvRecordQualityMode>();

    public static readonly UpnpAvRecordQualityMode Ep0 = new UpnpAvRecordQualityMode("0:EP");
    public static readonly UpnpAvRecordQualityMode Lp1 = new UpnpAvRecordQualityMode("1:LP");
    public static readonly UpnpAvRecordQualityMode Sp2 = new UpnpAvRecordQualityMode("2:SP");
    public static readonly UpnpAvRecordQualityMode Basic0 = new UpnpAvRecordQualityMode("0:BASIC");
    public static readonly UpnpAvRecordQualityMode Medium1 = new UpnpAvRecordQualityMode("1:MEDIUM");
    public static readonly UpnpAvRecordQualityMode High2 = new UpnpAvRecordQualityMode("2:HIGH");
    public static readonly UpnpAvRecordQualityMode NotImplemented  = new UpnpAvRecordQualityMode("NOT_IMPLEMENTED");

    private UpnpAvRecordQualityMode(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      UpnpAvRecordQualityMode mode = obj as UpnpAvRecordQualityMode;
      if (mode != null && this == mode)
      {
        return true;
      }
      return false;
    }

    public static ICollection<UpnpAvRecordQualityMode> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator UpnpAvRecordQualityMode(string name)
    {
      UpnpAvRecordQualityMode value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(UpnpAvRecordQualityMode mode)
    {
      return mode._name;
    }
  }

  public sealed class UpnpAvSeekMode
  {
    private readonly string _name;
    private static readonly IDictionary<string, UpnpAvSeekMode> _values = new Dictionary<string, UpnpAvSeekMode>();

    public static readonly UpnpAvSeekMode TrackNr = new UpnpAvSeekMode("TRACK_NR");
    public static readonly UpnpAvSeekMode AbsTime = new UpnpAvSeekMode("ABS_TIME");
    public static readonly UpnpAvSeekMode RelTime = new UpnpAvSeekMode("REL_TIME");
    public static readonly UpnpAvSeekMode AbsCount = new UpnpAvSeekMode("ABS_COUNT");
    public static readonly UpnpAvSeekMode RelCount = new UpnpAvSeekMode("REL_COUNT");
    public static readonly UpnpAvSeekMode ChannelFreq = new UpnpAvSeekMode("CHANNEL_FREQ");
    public static readonly UpnpAvSeekMode TapeIndex = new UpnpAvSeekMode("TAPE-INDEX");
    public static readonly UpnpAvSeekMode Frame = new UpnpAvSeekMode("FRAME");

    private UpnpAvSeekMode(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      UpnpAvSeekMode mode = obj as UpnpAvSeekMode;
      if (mode != null && this == mode)
      {
        return true;
      }
      return false;
    }

    public static ICollection<UpnpAvSeekMode> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator UpnpAvSeekMode(string name)
    {
      UpnpAvSeekMode value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(UpnpAvSeekMode mode)
    {
      return mode._name;
    }
  }

  public class AvTransportService : BaseService
  {
    private CpAction _setAvTransportUriAction = null;
    private CpAction _setNextAvTransportUriAction = null;
    private CpAction _getMediaInfoAction = null;
    private CpAction _getTransportInfoAction = null;
    private CpAction _getPositionInfoAction = null;
    private CpAction _getDeviceCapabilitiesAction = null;
    private CpAction _getTransportSettingsAction = null;
    private CpAction _stopAction = null;
    private CpAction _playAction = null;
    private CpAction _pauseAction = null;
    private CpAction _recordAction = null;
    private CpAction _seekAction = null;
    private CpAction _nextAction = null;
    private CpAction _previousAction = null;
    private CpAction _setPlayModeAction = null;
    private CpAction _setRecordQualityModeAction = null;
    private CpAction _getCurrentTransportActionsAction = null;

    public AvTransportService(CpDevice device)
      : base(device, "urn:upnp-org:serviceId:urn:schemas-upnp-org:service:AVTransport")
    {
      _service.Actions.TryGetValue("SetAVTransportURI", out _setAvTransportUriAction);
      _service.Actions.TryGetValue("SetNextAVTransportURI", out _setNextAvTransportUriAction);
      _service.Actions.TryGetValue("GetMediaInfo", out _getMediaInfoAction);
      _service.Actions.TryGetValue("GetTransportInfo", out _getTransportInfoAction);
      _service.Actions.TryGetValue("GetPositionInfo", out _getPositionInfoAction);
      _service.Actions.TryGetValue("GetDeviceCapabilities", out _getDeviceCapabilitiesAction);
      _service.Actions.TryGetValue("GetTransportSettings", out _getTransportSettingsAction);
      _service.Actions.TryGetValue("Stop", out _stopAction);
      _service.Actions.TryGetValue("Play", out _playAction);
      _service.Actions.TryGetValue("Pause", out _pauseAction);
      _service.Actions.TryGetValue("Record", out _recordAction);
      _service.Actions.TryGetValue("Seek", out _seekAction);
      _service.Actions.TryGetValue("Next", out _nextAction);
      _service.Actions.TryGetValue("Previous", out _previousAction);
      _service.Actions.TryGetValue("SetPlayMode", out _setPlayModeAction);
      _service.Actions.TryGetValue("SetRecordQualityMode", out _setRecordQualityModeAction);
      _service.Actions.TryGetValue("GetCurrentTransportActions", out _getCurrentTransportActionsAction);
    }

    public void SetAvTransportUri(UInt32 instanceId, string currentUri, string currentUriMetaData)
    {
      _setAvTransportUriAction.InvokeAction(new List<object> { instanceId, currentUri, currentUriMetaData });
    }

    public bool SetNextAvTransportUri(UInt32 instanceId, string nextUri, string nextUriMetaData)
    {
      if (_setNextAvTransportUriAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement an AVTransport SetNextAVTransportURI action", _device.UDN);
        return false;
      }

      _setNextAvTransportUriAction.InvokeAction(new List<object> { instanceId, nextUri, nextUriMetaData });
      return true;
    }

    public void GetMediaInfo(UInt32 instanceId, out UInt32 nrTracks, out string mediaDuration, out string currentUri,
                              out string currentUriMetaData, out string nextUri, out string nextUriMetaData,
                              out UpnpAvStorageMedium playMedium, out UpnpAvStorageMedium recordMedium,
                              out UpnpAvRecordMediumWriteStatus writeStatus)
    {
      IList<object> outParams = _getMediaInfoAction.InvokeAction(new List<object> { instanceId });
      nrTracks = (uint)outParams[0];
      mediaDuration = (string)outParams[1];
      currentUri = (string)outParams[2];
      currentUriMetaData = (string)outParams[3];
      nextUri = (string)outParams[4];
      nextUriMetaData = (string)outParams[5];
      playMedium = (UpnpAvStorageMedium)(string)outParams[6];
      recordMedium = (UpnpAvStorageMedium)(string)outParams[7];
      writeStatus = (UpnpAvRecordMediumWriteStatus)Enum.Parse(typeof(UpnpAvRecordMediumWriteStatus), (string)outParams[8]);
    }

    public void GetTransportInfo(UInt32 instanceId, out UpnpAvTransportState currentTransportState,
                                out UpnpAvTransportStatus currentTransportStatus, out string currentSpeed)
    {
      IList<object> outParams = _getTransportInfoAction.InvokeAction(new List<object> { instanceId });
      currentTransportState = (UpnpAvTransportState)Enum.Parse(typeof(UpnpAvTransportState), (string)outParams[0]);
      currentTransportStatus = (UpnpAvTransportStatus)Enum.Parse(typeof(UpnpAvTransportStatus), (string)outParams[1]);
      currentSpeed = (string)outParams[2];
    }

    public void GetPositionInfo(UInt32 instanceId, out UInt32 track, out string trackDuration, out string trackMetaData,
                                out string trackUri, out string relTime, out string absTime, out Int32 relCount,
                                out Int32 absCount)
    {
      IList<object> outParams = _getPositionInfoAction.InvokeAction(new List<object> { instanceId });
      track = (uint)outParams[0];
      trackDuration = (string)outParams[1];
      trackMetaData = (string)outParams[2];
      trackUri = (string)outParams[3];
      relTime = (string)outParams[4];
      absTime = (string)outParams[5];
      relCount = (int)outParams[6];
      absCount = (int)outParams[7];
    }

    public void GetDeviceCapabilities(UInt32 instanceId, out IList<UpnpAvStorageMedium> playMedia,
                                      out IList<UpnpAvStorageMedium> recMedia, out IList<UpnpAvRecordQualityMode> recQualityModes)
    {
      IList<object> outParams = _getDeviceCapabilitiesAction.InvokeAction(new List<object> { instanceId });
      playMedia = outParams[0].ToString().Split(',').Select(x => (UpnpAvStorageMedium)(string)x).ToList<UpnpAvStorageMedium>();
      recMedia = outParams[1].ToString().Split(',').Select(x => (UpnpAvStorageMedium)(string)x).ToList<UpnpAvStorageMedium>();
      recQualityModes = outParams[2].ToString().Split(',').Select(x => (UpnpAvRecordQualityMode)(string)x).ToList<UpnpAvRecordQualityMode>();
    }

    public void GetTransportSettings(UInt32 instanceId, out UpnpAvCurrentPlayMode playMode, out UpnpAvRecordQualityMode recQualityMode)
    {
      IList<object> outParams = _getTransportSettingsAction.InvokeAction(new List<object> { instanceId });
      playMode = (UpnpAvCurrentPlayMode)Enum.Parse(typeof(UpnpAvCurrentPlayMode), (string)outParams[0]);
      recQualityMode = (UpnpAvRecordQualityMode)(string)outParams[1];
    }

    public void Stop(UInt32 instanceId)
    {
      _stopAction.InvokeAction(new List<object> { instanceId });
    }

    public void Play(UInt32 instanceId, string speed)
    {
      _playAction.InvokeAction(new List<object> { instanceId, speed });
    }

    public bool Pause(UInt32 instanceId)
    {
      if (_pauseAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a AVTransport Pause action", _device.UDN);
        return false;
      }

      _pauseAction.InvokeAction(new List<object> { instanceId });
      return true;
    }

    public bool Record(UInt32 instanceId)
    {
      if (_recordAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a AVTransport Record action", _device.UDN);
        return false;
      }

      _recordAction.InvokeAction(new List<object> { instanceId });
      return false;
    }

    public void Seek(UInt32 instanceId, UpnpAvSeekMode unit, string target)
    {
      // actual target type is unit-dependent
      // "...depending on the actual seek mode used, it must contains string
      // representations of values of UPnP types ‘ui4’ (ABS_COUNT, REL_COUNT,
      // TRACK_NR, TAPE-INDEX, FRAME), ‘time’ (ABS_TIME, REL_TIME) or ‘float‘
      // (CHANNEL_FREQ, in Hz). Supported ranges of these integer, time or
      // float values are device-dependent."
      _seekAction.InvokeAction(new List<object> { instanceId, unit.ToString(), target });
    }

    public void Next(UInt32 instanceId)
    {
      _nextAction.InvokeAction(new List<object> { instanceId });
    }

    public void Previous(UInt32 instanceId)
    {
      _previousAction.InvokeAction(new List<object> { instanceId });
    }

    public bool SetPlayMode(UInt32 instanceId, UpnpAvCurrentPlayMode newPlayMode)
    {
      if (_setPlayModeAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a AVTransport SetPlayMode action", _device.UDN);
        return false;
      }

      _setPlayModeAction.InvokeAction(new List<object> { instanceId, newPlayMode.ToString() });
      return true;
    }

    public bool SetRecordQualityMode(UInt32 instanceId, UpnpAvRecordQualityMode newRecordQualityMode)
    {
      if (_setRecordQualityModeAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a AVTransport SetRecordQualityMode action", _device.UDN);
        return false;
      }

      _setRecordQualityModeAction.InvokeAction(new List<object> { instanceId, newRecordQualityMode.ToString() });
      return true;
    }

    public bool GetCurrentTransportActions(UInt32 instanceId, out IList<UpnpAvTransportAction> actions)
    {
      actions = new List<UpnpAvTransportAction>();
      if (_getCurrentTransportActionsAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a AVTransport GetCurrentTransportActions action", _device.UDN);
        return false;
      }

      try
      {
        IList<object> outParams = _getCurrentTransportActionsAction.InvokeAction(new List<object> { instanceId });
        actions = outParams[0].ToString().Split(',').Select(x => (UpnpAvTransportAction)Enum.Parse(typeof(UpnpAvTransportAction), x)).ToList<UpnpAvTransportAction>();
      }
      catch
      {
        Log.Log.Debug("DRI: device {0} does not implement a AVTransport GetCurrentTransportActions action, threw exception", _device.UDN);
        return false;
      }
      return true;
    }
  }
}
