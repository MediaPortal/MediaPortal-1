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

using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure.CP.DeviceTree;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service
{
  internal class ServiceAvTransport : ServiceBase
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

    public ServiceAvTransport(CpDevice device)
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

    public void SetAvTransportUri(uint instanceId, string currentUri, string currentUriMetaData)
    {
      _setAvTransportUriAction.InvokeAction(new List<object> { instanceId, currentUri, currentUriMetaData });
    }

    public bool SetNextAvTransportUri(uint instanceId, string nextUri, string nextUriMetaData)
    {
      if (_setNextAvTransportUriAction == null)
      {
        this.LogWarn("UPnP: device {0} does not implement an AVTransport SetNextAVTransportURI action", _device.UDN);
        return false;
      }

      _setNextAvTransportUriAction.InvokeAction(new List<object> { instanceId, nextUri, nextUriMetaData });
      return true;
    }

    public void GetMediaInfo(uint instanceId, out uint nrTracks, out string mediaDuration, out string currentUri,
                              out string currentUriMetaData, out string nextUri, out string nextUriMetaData,
                              out AvTransportStorageMedium playMedium, out AvTransportStorageMedium recordMedium,
                              out AvTransportRecordMediumWriteStatus writeStatus)
    {
      IList<object> outParams = _getMediaInfoAction.InvokeAction(new List<object> { instanceId });
      nrTracks = (uint)outParams[0];
      mediaDuration = (string)outParams[1];
      currentUri = (string)outParams[2];
      currentUriMetaData = (string)outParams[3];
      nextUri = (string)outParams[4];
      nextUriMetaData = (string)outParams[5];
      playMedium = (AvTransportStorageMedium)(string)outParams[6];
      recordMedium = (AvTransportStorageMedium)(string)outParams[7];
      writeStatus = (AvTransportRecordMediumWriteStatus)(string)outParams[8];
    }

    public void GetTransportInfo(uint instanceId, out AvTransportState currentTransportState,
                                out AvTransportStatus currentTransportStatus, out string currentSpeed)
    {
      IList<object> outParams = _getTransportInfoAction.InvokeAction(new List<object> { instanceId });
      currentTransportState = (AvTransportState)(string)outParams[0];
      currentTransportStatus = (AvTransportStatus)(string)outParams[1];
      currentSpeed = (string)outParams[2];
    }

    public void GetPositionInfo(uint instanceId, out uint track, out string trackDuration, out string trackMetaData,
                                out string trackUri, out string relTime, out string absTime, out int relCount,
                                out int absCount)
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

    public void GetDeviceCapabilities(uint instanceId, out IList<AvTransportStorageMedium> playMedia,
                                      out IList<AvTransportStorageMedium> recMedia, out IList<AvTransportRecordQualityMode> recQualityModes)
    {
      IList<object> outParams = _getDeviceCapabilitiesAction.InvokeAction(new List<object> { instanceId });
      playMedia = outParams[0].ToString().Split(',').Select(x => (AvTransportStorageMedium)(string)x).ToList<AvTransportStorageMedium>();
      recMedia = outParams[1].ToString().Split(',').Select(x => (AvTransportStorageMedium)(string)x).ToList<AvTransportStorageMedium>();
      recQualityModes = outParams[2].ToString().Split(',').Select(x => (AvTransportRecordQualityMode)(string)x).ToList<AvTransportRecordQualityMode>();
    }

    public void GetTransportSettings(uint instanceId, out AvTransportCurrentPlayMode playMode, out AvTransportRecordQualityMode recQualityMode)
    {
      IList<object> outParams = _getTransportSettingsAction.InvokeAction(new List<object> { instanceId });
      playMode = (AvTransportCurrentPlayMode)(string)outParams[0];
      recQualityMode = (AvTransportRecordQualityMode)(string)outParams[1];
    }

    public void Stop(uint instanceId)
    {
      _stopAction.InvokeAction(new List<object> { instanceId });
    }

    public void Play(uint instanceId, string speed)
    {
      _playAction.InvokeAction(new List<object> { instanceId, speed });
    }

    public bool Pause(uint instanceId)
    {
      if (_pauseAction == null)
      {
        this.LogWarn("UPnP: device {0} does not implement an AVTransport Pause action", _device.UDN);
        return false;
      }

      _pauseAction.InvokeAction(new List<object> { instanceId });
      return true;
    }

    public bool Record(uint instanceId)
    {
      if (_recordAction == null)
      {
        this.LogWarn("UPnP: device {0} does not implement an AVTransport Record action", _device.UDN);
        return false;
      }

      _recordAction.InvokeAction(new List<object> { instanceId });
      return false;
    }

    public void Seek(uint instanceId, AvTransportSeekMode unit, string target)
    {
      // actual target type is unit-dependent
      // "...depending on the actual seek mode used, it must contains string
      // representations of values of UPnP types ‘ui4’ (ABS_COUNT, REL_COUNT,
      // TRACK_NR, TAPE-INDEX, FRAME), ‘time’ (ABS_TIME, REL_TIME) or ‘float‘
      // (CHANNEL_FREQ, in Hz). Supported ranges of these integer, time or
      // float values are device-dependent."
      _seekAction.InvokeAction(new List<object> { instanceId, unit.ToString(), target });
    }

    public void Next(uint instanceId)
    {
      _nextAction.InvokeAction(new List<object> { instanceId });
    }

    public void Previous(uint instanceId)
    {
      _previousAction.InvokeAction(new List<object> { instanceId });
    }

    public bool SetPlayMode(uint instanceId, AvTransportCurrentPlayMode newPlayMode)
    {
      if (_setPlayModeAction == null)
      {
        this.LogWarn("UPnP: device {0} does not implement an AVTransport SetPlayMode action", _device.UDN);
        return false;
      }

      _setPlayModeAction.InvokeAction(new List<object> { instanceId, newPlayMode.ToString() });
      return true;
    }

    public bool SetRecordQualityMode(uint instanceId, AvTransportRecordQualityMode newRecordQualityMode)
    {
      if (_setRecordQualityModeAction == null)
      {
        this.LogWarn("UPnP: device {0} does not implement an AVTransport SetRecordQualityMode action", _device.UDN);
        return false;
      }

      _setRecordQualityModeAction.InvokeAction(new List<object> { instanceId, newRecordQualityMode.ToString() });
      return true;
    }

    public bool GetCurrentTransportActions(uint instanceId, out IList<AvTransportAction> actions)
    {
      actions = new List<AvTransportAction>();
      if (_getCurrentTransportActionsAction == null)
      {
        this.LogWarn("UPnP: device {0} does not implement an AVTransport GetCurrentTransportActions action", _device.UDN);
        return false;
      }

      try
      {
        IList<object> outParams = _getCurrentTransportActionsAction.InvokeAction(new List<object> { instanceId });
        actions = outParams[0].ToString().Split(',').Select(x => (AvTransportAction)x).ToList<AvTransportAction>();
      }
      catch
      {
        this.LogWarn("UPnP: device {0} does not implement an AVTransport GetCurrentTransportActions action, threw exception", _device.UDN);
        return false;
      }
      return true;
    }
  }
}