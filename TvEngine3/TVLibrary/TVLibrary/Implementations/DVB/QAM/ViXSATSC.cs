#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Runtime.InteropServices;
using DirectShowLib;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
  internal class ViXSATSC
  {
    #region constants

    private readonly Guid guidViXSTunerExtention = new Guid(0x02779308, 0x77d8, 0x4914, 0x9f, 0x15, 0x7f, 0xa6, 0xe1,
                                                            0x55, 0x84, 0xc7);

    #endregion

    #region variables

    private readonly bool _isViXSATSC;
    private readonly IntPtr _tempValue = IntPtr.Zero; //Marshal.AllocCoTaskMem(1024);
    private readonly IKsPropertySet _propertySet;

    #endregion

    /// <summary>
    /// Gets a value indicating whether this instance is generic qam.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is generic qam; otherwise, <c>false</c>.
    /// </value>
    public bool IsCamPresent()
    {
      return false;
    }

    public bool IsViXSATSC
    {
      get { return _isViXSATSC; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViXSATSC"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public ViXSATSC(IBaseFilter tunerFilter)
    {
      IPin pin = DsFindPin.ByName(tunerFilter, "MPEG2 Transport");
      if (pin != null)
      {
        _propertySet = tunerFilter as IKsPropertySet;
        if (_propertySet != null)
        {
          KSPropertySupport supported;
          _propertySet.QuerySupported(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
          if ((supported & KSPropertySupport.Set) != 0)
          {
            Log.Log.Debug("ViXS ATSC: DVB-S card found!");
            _tempValue = Marshal.AllocCoTaskMem(1024);
            _isViXSATSC = true;
          }
          else
          {
            Log.Log.Debug("ViXS ATSC: card NOT found!");
            _isViXSATSC = false;
            Dispose();
          }
        }
      }
      else
        Log.Log.Info("ViXS ATSC: could not find MPEG2 Transport pin!");
    }

    /// <summary>
    /// sets the QAM modulation for ViXS ATSC cards
    /// </summary>
    public void SetViXSQam(ATSCChannel channel)
    {
      KSPropertySupport supported;
      _propertySet.QuerySupported(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        Log.Log.Debug("ViXS ATSC: Set ModulationType value: {0}", (Int32)channel.ModulationType);
        Marshal.WriteInt32(_tempValue, (Int32)channel.ModulationType);
        int hr = _propertySet.Set(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, _tempValue, 4,
                                  _tempValue, 4);
        if (hr != 0)
        {
          Log.Log.Info("ViXS ATSC: Set returned: 0x{0:X} - {1}", hr, HResult.GetDXErrorDescription(hr));
        }
      }
    }

    /// <summary>
    /// gets the QAM modulation for ViXS ATSC cards
    /// </summary>
    public void GetViXSQam(ATSCChannel channel)
    {
      KSPropertySupport supported;
      _propertySet.QuerySupported(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
      if ((supported & KSPropertySupport.Get) == KSPropertySupport.Get)
      {
        int length;
        Marshal.WriteInt32(_tempValue, 0);
        int hr = _propertySet.Get(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, _tempValue, 4,
                                  _tempValue, 4, out length);
        if (hr != 0)
        {
          Log.Log.Info("ViXS ATSC: Get returned:{0:X}", hr);
        }
        Log.Log.Info("ViXS ATSC: Get ModulationType returned value: {0}", Marshal.ReadInt32(_tempValue));
      }
    }

    /// <summary>
    /// Disposes COM task memory resources
    /// </summary>
    public void Dispose()
    {
      Marshal.FreeCoTaskMem(_tempValue);
    }
  }
}