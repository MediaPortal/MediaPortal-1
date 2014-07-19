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
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure.CP.DeviceTree;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service
{
  internal class ServiceUserActivity : ServiceBase
  {
    private CpAction _setCurrentTunerUseReason = null;
    private CpAction _getUserActivityInterval = null;
    private CpAction _userActivityDetected = null;

    public ServiceUserActivity(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:UserActivity", true)
    {
      if (_service != null)
      {
        _service.Actions.TryGetValue("SetCurrentTunerUseReason", out _setCurrentTunerUseReason);
        _service.Actions.TryGetValue("GetUserActivityInterval", out _getUserActivityInterval);
        _service.Actions.TryGetValue("UserActivityDetected", out _userActivityDetected);
      }
    }

    public bool SetCurrentTunerUseReason(UserActivityUseReason useReason, out int result)
    {
      result = 0;
      if (_service == null)
      {
        this.LogWarn("DRI: device {0} does not implement a UserActivity service", _device.UDN);
        return false;
      }

      IList<object> outParams = _setCurrentTunerUseReason.InvokeAction(new List<object> { (int)useReason });
      result = (int)outParams[0];
      return true;
    }

    public bool GetUserActivityInterval(out uint activityInterval, out int result)
    {
      activityInterval = 0;
      result = 0;
      if (_service == null)
      {
        this.LogWarn("DRI: device {0} does not implement a UserActivity service", _device.UDN);
        return false;
      }

      IList<object> outParams = _getUserActivityInterval.InvokeAction(null);
      activityInterval = (uint)outParams[0];
      result = (int)outParams[1];
      return true;
    }

    public bool UserActivityDetected(out int result)
    {
      result = 0;
      if (_service == null)
      {
        this.LogWarn("DRI: device {0} does not implement a UserActivity service", _device.UDN);
        return false;
      }

      IList<object> outParams = _userActivityDetected.InvokeAction(null);
      result = (int)outParams[0];
      return true;
    }
  }
}