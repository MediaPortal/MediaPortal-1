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
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp.Service;
using UPnP.Infrastructure.CP.DeviceTree;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service
{
  internal class ServiceDiag : ServiceBase
  {
    private CpAction _getParameterAction = null;

    public ServiceDiag(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Diag")
    {
      _service.Actions.TryGetValue("GetParameter", out _getParameterAction);
    }

    /// <summary>
    /// Upon receipt of the GetParameter action, the DRIT SHALL return the value and the type of the parameter in less
    /// than 1s.
    /// </summary>
    /// <param name="parameter">This argument sets the A_ARG_TYPE_Parameter state variable.</param>
    /// <param name="value">This argument provides the value of the A_ARG_TYPE_Value state variable when the action response is created.</param>
    /// <param name="isVolatile">This argument provides the value of the A_ARG_TYPE_Volatile state variable when the action response is created.</param>
    public void GetParameter(string parameter, out string value, out bool isVolatile)
    {
      IList<object> outParams = _getParameterAction.InvokeAction(new List<object> { parameter });
      value = (string)outParams[0];
      isVolatile = (bool)outParams[1];
    }
  }
}