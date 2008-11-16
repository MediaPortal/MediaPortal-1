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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using System.Windows.Forms;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  class GenericATSC
  {
    #region enums
    enum BdaDigitalModulator
    {
      MODULATION_TYPE = 0,
      INNER_FEC_TYPE,
      INNER_FEC_RATE,
      OUTER_FEC_TYPE,
      OUTER_FEC_RATE,
      SYMBOL_RATE,
      SPECTRAL_INVERSION,
      GUARD_INTERVAL,
      TRANSMISSION_MODE
    };
    #endregion

    #region constants
    Guid guidBdaDigitalDemodulator = new Guid(0xef30f379, 0x985b, 0x4d10, 0xb6, 0x40, 0xa7, 0x9d, 0x5e, 0x4, 0xe1, 0xe0);
    #endregion

    #region variables
    bool _isGenericATSC = false;
    IntPtr _tempValue = Marshal.AllocCoTaskMem(1024);
    IntPtr _tempInstance = Marshal.AllocCoTaskMem(1024);
    DirectShowLib.IKsPropertySet _propertySet = null;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericATSC"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The analyzer filter.</param>
    public GenericATSC(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {
      IPin pin = DsFindPin.ByName(tunerFilter, "MPEG2 Transport");
      if (pin != null)
      {
        _propertySet = pin as DirectShowLib.IKsPropertySet;
        if (_propertySet != null)
        {
          KSPropertySupport supported;
          _propertySet.QuerySupported(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
          //Log.Log.Info("GenericATSC: QuerySupported: {0}", supported);
          if ((supported & KSPropertySupport.Set) != 0)
          {
            //Log.Log.Info("GenericATSC: QAM capable card found!");
            _isGenericATSC = true;
          }
        }
      }
    }

    /// <summary>
    /// sets the QAM modulation for ATSC cards under XP
    /// </summary>
    public void SetXPATSCQam(ATSCChannel channel)
    {
      int hr;
      KSPropertySupport supported;
      _propertySet.QuerySupported(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
      //Log.Log.Info("GenericATSC: BdaDigitalDemodulator supported: {0}", supported);
      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        Log.Log.Info("GenericATSC: Set ModulationType: {0}", channel.ModulationType);
        Marshal.WriteInt32(_tempValue, (Int32)channel.ModulationType);
        hr = _propertySet.Set(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, _tempInstance, 32, _tempValue, 4);
        if (hr != 0)
        {
          Log.Log.Info("GenericATSC: Set returned:{0:X}", hr);
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
    /// Gets a value indicating whether this instance is generic qam.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is generic qam; otherwise, <c>false</c>.
    /// </value>
    public bool IsCamPresent()
    {
      return false;
    }

    public bool IsGenericATSC
    {
      get
      {
        return _isGenericATSC;
      }
    }
  }
}
