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
  internal class ServiceAux : ServiceBase
  {
    private CpAction _getAuxCapabilitiesAction = null;
    private CpAction _setAuxParametersAction = null;

    public ServiceAux(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Aux", true)
    {
      if (_service != null)
      {
        _service.Actions.TryGetValue("GetAuxCapabilities", out _getAuxCapabilitiesAction);
        _service.Actions.TryGetValue("SetAuxParameters", out _setAuxParametersAction);
      }
    }

    /// <summary>
    /// Upon receipt of the GetAuxCapabilities action, the DRIT SHALL provide a detailed description of its baseband
    /// video inputs in less than 1s.
    /// </summary>
    /// <param name="supportedFormat">This argument provides the value of the FormatList state variable when the action response is created.</param>
    /// <param name="svideoNbr">This argument provides the value of the SVideoInputs state variable when the action response is created.</param>
    /// <param name="videoNbr">This argument provides the value of the VideoInputs state variable when the action response is created.</param>
    /// <returns><c>true</c> if the action is executed, otherwise <c>false</c></returns>
    public bool GetAuxCapabilities(out IList<AuxFormat> supportedFormat, out byte svideoNbr, out byte videoNbr)
    {
      supportedFormat = new List<AuxFormat>();
      svideoNbr = 0;
      videoNbr = 0;
      if (_service == null)
      {
        this.LogWarn("DRI: device {0} does not implement an Aux service", _device.UDN);
        return false;
      }

      IList<object> outParams = _getAuxCapabilitiesAction.InvokeAction(null);
      supportedFormat = outParams[0].ToString().Split(',').Select(x => (AuxFormat)(string)x).ToList<AuxFormat>();
      svideoNbr = (byte)outParams[1];
      videoNbr = (byte)outParams[2];
      return true;
    }

    /// <summary>
    /// Upon receipt of the SetAuxParameters action, the DRIT SHALL select the required inputs, detect signal presence, and format in less than 5 seconds.
    /// </summary>
    /// <param name="selectType">This argument sets the InputType state variable.</param>
    /// <param name="selectInput">This argument sets the InputNumber state variable.</param>
    /// <param name="selectFormat">This argument sets the Format state variable.</param>
    /// <param name="actualFormat">This argument reflects the value of the Format state variable.</param>
    /// <param name="currentGenLock">This argument provides the value of the GenLock state variable when the action response is created.</param>
    /// <returns><c>true</c> if the action is executed, otherwise <c>false</c></returns>
    public bool SetAuxParameters(AuxInputType selectType, byte selectInput, AuxFormat selectFormat,
                                  out AuxFormat actualFormat, out bool currentGenLock)
    {
      actualFormat = AuxFormat.Unknown;
      currentGenLock = false;
      if (_service == null)
      {
        this.LogWarn("DRI: device {0} does not implement an Aux service", _device.UDN);
        return false;
      }

      IList<object> outParams = _setAuxParametersAction.InvokeAction(new List<object> {
        selectType.ToString(), selectInput, selectFormat.ToString()
      });
      actualFormat = (AuxFormat)(string)outParams[0];
      currentGenLock = (bool)outParams[1];
      return true;
    }
  }
}