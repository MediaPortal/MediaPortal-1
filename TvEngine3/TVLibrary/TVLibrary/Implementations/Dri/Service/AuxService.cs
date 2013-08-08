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
  public sealed class DriAuxFormat
  {
    private readonly string _name;
    private static readonly IDictionary<string, DriAuxFormat> _values = new Dictionary<string, DriAuxFormat>();

    public static readonly DriAuxFormat Unknown = new DriAuxFormat("UNKNOWN");
    public static readonly DriAuxFormat NtscM = new DriAuxFormat("NTSC-M");

    private DriAuxFormat(string name)
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
      DriAuxFormat auxFormat = obj as DriAuxFormat;
      if (auxFormat != null && this == auxFormat)
      {
        return true;
      }
      return false;
    }

    public static ICollection<DriAuxFormat> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator DriAuxFormat(string name)
    {
      DriAuxFormat value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DriAuxFormat auxFormat)
    {
      return auxFormat._name;
    }
  }

  public sealed class DriInputType
  {
    private readonly string _name;
    private static readonly IDictionary<string, DriInputType> _values = new Dictionary<string, DriInputType>();

    public static readonly DriInputType Svideo = new DriInputType("S-VIDEO");
    public static readonly DriInputType Video = new DriInputType("VIDEO");

    private DriInputType(string name)
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
      DriInputType inputType = obj as DriInputType;
      if (inputType != null && this == inputType)
      {
        return true;
      }
      return false;
    }

    public static ICollection<DriInputType> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator DriInputType(string name)
    {
      DriInputType value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DriInputType inputType)
    {
      return inputType._name;
    }
  }

  public class AuxService : BaseService
  {
    private CpAction _getAuxCapabilitiesAction = null;
    private CpAction _setAuxParametersAction = null;

    public AuxService(CpDevice device)
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
    public bool GetAuxCapabilities(out IList<DriAuxFormat> supportedFormat, out byte svideoNbr, out byte videoNbr)
    {
      supportedFormat = new List<DriAuxFormat>();
      svideoNbr = 0;
      videoNbr = 0;
      if (_service == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement an Aux service", _device.UDN);
        return false;
      }

      IList<object> outParams = _getAuxCapabilitiesAction.InvokeAction(null);
      supportedFormat = outParams[0].ToString().Split(',').Select(x => (DriAuxFormat)(string)x).ToList<DriAuxFormat>();
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
    public bool SetAuxParameters(DriInputType selectType, byte selectInput, DriAuxFormat selectFormat,
                                out DriAuxFormat actualFormat, out bool currentGenLock)
    {
      actualFormat = DriAuxFormat.Unknown;
      currentGenLock = false;
      if (_service == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement an Aux service", _device.UDN);
        return false;
      }

      IList<object> outParams = _setAuxParametersAction.InvokeAction(new List<object> {
        selectType.ToString(), selectInput, selectFormat.ToString()
      });
      actualFormat = (DriAuxFormat)(string)outParams[0];
      currentGenLock = (bool)outParams[1];
      return true;
    }
  }
}
