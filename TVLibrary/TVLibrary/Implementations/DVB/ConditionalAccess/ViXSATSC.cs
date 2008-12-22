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
  class ViXSATSC
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
    Guid guidViXSTunerExtention = new Guid(0x02779308, 0x77d8, 0x4914, 0x9f, 0x15, 0x7f, 0xa6, 0xe1, 0x55, 0x84, 0xc7);
    #endregion

    #region variables
    bool _isViXSATSC = false;
    IntPtr _tempValue = IntPtr.Zero; //Marshal.AllocCoTaskMem(1024);
    //IntPtr _tempInstance = Marshal.AllocCoTaskMem(1024);
    DirectShowLib.IKsPropertySet _propertySet = null;
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
      get
      {
        return _isViXSATSC;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViXSATSC"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The analyzer filter.</param>
    public ViXSATSC(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {
      IPin pin = DsFindPin.ByName(tunerFilter, "MPEG2 Transport");
      if (pin != null)
      {
        _propertySet = tunerFilter as DirectShowLib.IKsPropertySet;
        if (_propertySet != null)
        {
          KSPropertySupport supported;
          _propertySet.QuerySupported(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
          if ((supported & KSPropertySupport.Set) != 0)
          {
            _tempValue = Marshal.AllocCoTaskMem(1024);
            _isViXSATSC = true;
          }
          else
            Log.Log.Info("ViXS ATSC: property not supported");
        }
        else
          Log.Log.Info("ViXS ATSC: could not find tuner filter!");
      }
      else
        Log.Log.Info("ViXS ATSC: could not find MPEG2 Transport pin!");
    }

    /// <summary>
    /// sets the QAM modulation for ViXS ATSC cards
    /// </summary>
    public void SetViXSQam(ATSCChannel channel)
    {
      int hr;
      KSPropertySupport supported;
      _propertySet.QuerySupported(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        Marshal.WriteInt32(_tempValue, (Int32)channel.ModulationType);
        hr = _propertySet.Set(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, _tempValue, 4, _tempValue, 4);
        if (hr != 0)
        {
          Log.Log.Info("ViXS ATSC: Set returned:{0:X}", hr);
        }
        Log.Log.Info("ViXS ATSC: Set ModulationType value: {0}", (Int32)channel.ModulationType);
      }
      //below is for info only - uncomment if debugging
      /*
      if ((supported & KSPropertySupport.Get) == KSPropertySupport.Get)
      {
        int length;
        //Log.Log.Info("ViXS ATSC: Get ModulationType");
        Marshal.WriteInt32(_tempValue, (Int32)0);
        hr = _propertySet.Get(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, _tempInstance, 32, _tempValue, 4, out length);
        Log.Log.Info("ViXS ATSC: Get returned:{0:X} len:{1} value:{2}", hr, length, Marshal.ReadInt32(_tempValue));
      }
      */
    }

    /// <summary>
    /// gets the QAM modulation for ViXS ATSC cards
    /// </summary>
    public void GetViXSQam(ATSCChannel channel)
    {
      int hr;
      KSPropertySupport supported;
      _propertySet.QuerySupported(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
      int length;
      if ((supported & KSPropertySupport.Get) == KSPropertySupport.Get)
      {
        Marshal.WriteInt32(_tempValue, (Int32)0);
        hr = _propertySet.Get(guidViXSTunerExtention, (int)BdaDigitalModulator.MODULATION_TYPE, _tempValue, 4, _tempValue, 4, out length);
        if (hr != 0)
        {
          Log.Log.Info("ViXS ATSC: Get returned:{0:X}", hr);
        }
        Log.Log.Info("ViXS ATSC: Get ModulationType returned value: {0}", Marshal.ReadInt32(_tempValue));
      }
    }
  }
}