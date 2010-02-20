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
  ///<summary>
  /// Generic ATSC devices BDA calls
  ///</summary>
  internal class GenericATSC
  {
    #region constants

    private readonly Guid guidBdaDigitalDemodulator = new Guid(0xef30f379, 0x985b, 0x4d10, 0xb6, 0x40, 0xa7, 0x9d, 0x5e,
                                                               0x4, 0xe1, 0xe0);

    #endregion

    #region variables

    private readonly bool _isGenericATSC;
    private readonly IntPtr _tempValue = IntPtr.Zero;
    private readonly IntPtr _tempInstance = IntPtr.Zero;
    private readonly IKsPropertySet _propertySet;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericATSC"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public GenericATSC(IBaseFilter tunerFilter)
    {
      IPin pin = DsFindPin.ByName(tunerFilter, "MPEG2 Transport");
      if (pin != null)
      {
        _propertySet = pin as IKsPropertySet;
        if (_propertySet != null)
        {
          KSPropertySupport supported;
          _propertySet.QuerySupported(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
          if ((supported & KSPropertySupport.Set) != 0)
          {
            Log.Log.Debug("GenericATSC: QAM capable card found!");
            _isGenericATSC = true;
            _tempValue = Marshal.AllocCoTaskMem(1024);
            _tempInstance = Marshal.AllocCoTaskMem(1024);
          }
          else
          {
            Log.Log.Debug("GenericATSC: QAM card NOT found!");
            _isGenericATSC = false;
            Dispose();
          }
        }
      }
      else
        Log.Log.Info("GenericATSC: tuner pin not found!");
    }

    /// <summary>
    /// Gets a value indicating whether this instance is generic qam.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is generic qam; otherwise, <c>false</c>.
    /// </value>
    public bool IsGenericATSC
    {
      get { return _isGenericATSC; }
    }

    /// <summary>
    /// Determines whether [is cam present].
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam present]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      return false;
    }

    /// <summary>
    /// sets the QAM modulation for ATSC cards under XP
    /// </summary>
    public void SetXPATSCQam(ATSCChannel channel)
    {
      if (_isGenericATSC == false)
        return;
      KSPropertySupport supported;
      _propertySet.QuerySupported(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        Log.Log.Info("GenericATSC: Set ModulationType: {0}", channel.ModulationType);
        Marshal.WriteInt32(_tempValue, (Int32)channel.ModulationType);
        int hr = _propertySet.Set(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, _tempInstance, 32,
                                  _tempValue, 4);
        if (hr != 0)
        {
          Log.Log.Info("GenericATSC: Set returned: 0x{0:X} - {1}", hr, HResult.GetDXErrorString(hr));
        }
      }
      //Below is for debug only...
      /*
      if ((supported & KSPropertySupport.Get) == KSPropertySupport.Get)
      {
        Log.Log.Info("GenericATSC: Get ModulationType");
        Marshal.WriteInt32(_tempValue, (Int32)0);
        hr = _propertySet.Get(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, _tempInstance, 32, _tempValue, 4, out length);
        Log.Log.Info("GenericATSC: Get   returned:{0:X} len:{1} value:{2}", hr, length, Marshal.ReadInt32(_tempValue));
      }
      */
    }

    /// <summary>
    /// Disposes COM task memory resources
    /// </summary>
    public void Dispose()
    {
      Marshal.FreeCoTaskMem(_tempValue);
      Marshal.FreeCoTaskMem(_tempInstance);
    }
  }
}