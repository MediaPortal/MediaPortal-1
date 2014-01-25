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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.CP.DeviceTree;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service
{
  public class ServiceBase : IDisposable
  {
    protected CpDevice _device = null;
    protected CpService _service = null;
    protected StateVariableChangedDlgt _stateVariableDelegate = null;
    protected EventSubscriptionFailedDlgt _eventSubscriptionDelegate = null;
    protected string _unqualifiedServiceName = string.Empty;

    public ServiceBase(CpDevice device, string serviceName, bool isOptional = false)
    {
      Initialise(device, serviceName, isOptional);
    }

    private void Initialise(CpDevice device, string serviceName, bool isOptional)
    {
      _device = device;
      _unqualifiedServiceName = serviceName.Substring(serviceName.LastIndexOf(":") + 1);
      if (!device.Services.TryGetValue(serviceName, out _service) && !isOptional)
      {
        throw new NotImplementedException(string.Format("Device does not implement a {0} service.", _unqualifiedServiceName));
      }
    }

    public void Dispose()
    {
      UnsubscribeStateVariables();
    }

    public void SubscribeStateVariables(StateVariableChangedDlgt svChangeDlg, EventSubscriptionFailedDlgt esFailDlg)
    {
      if (_service == null)
      {
        return;
      }
      UnsubscribeStateVariables();
      if (svChangeDlg != null)
      {
        _stateVariableDelegate = svChangeDlg;
        _service.StateVariableChanged += _stateVariableDelegate;
      }
      if (esFailDlg != null)
      {
        _eventSubscriptionDelegate = esFailDlg;
        _service.EventSubscriptionFailed += _eventSubscriptionDelegate;
      }
      _service.SubscribeStateVariables();
    }

    public void UnsubscribeStateVariables()
    {
      if (_service == null)
      {
        return;
      }
      if (_service.IsStateVariablesSubscribed)
      {
        _service.UnsubscribeStateVariables();
      }
      if (_stateVariableDelegate != null)
      {
        _service.StateVariableChanged -= _stateVariableDelegate;
      }
      if (_eventSubscriptionDelegate != null)
      {
        _service.EventSubscriptionFailed -= _eventSubscriptionDelegate;
      }
      _stateVariableDelegate = null;
      _eventSubscriptionDelegate = null;
    }

    private void SubscribeFailed(CpService service, UPnPError error)
    {
      this.LogError("UPnP: failed to subscribe to state variable events for service {0}, code = {1}, description = {2}", _unqualifiedServiceName, error.ErrorCode, error.ErrorDescription);
    }
  }
}