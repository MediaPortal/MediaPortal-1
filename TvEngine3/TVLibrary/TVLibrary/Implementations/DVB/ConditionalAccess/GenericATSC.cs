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
using System.Runtime.InteropServices;
using DirectShowLib;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
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

  enum BdaTunerExtension
  {
    KSPROPERTY_BDA_DISEQC = 0,
    KSPROPERTY_BDA_SCAN_FREQ,
    KSPROPERTY_BDA_CHANNEL_CHANGE,
    KSPROPERTY_BDA_EFFECTIVE_FREQ,
    KSPROPERTY_BDA_PILOT = 0x20,
    KSPROPERTY_BDA_ROLL_OFF = 0x21
  };

  enum DisEqcVersion
  {
    DISEQC_VER_1X = 1,
    DISEQC_VER_2X,
  };

  enum RxMode
  {
    RXMODE_INTERROGATION = 1, // Expecting multiple devices attached
    RXMODE_QUICKREPLY,      // Expecting 1 rx (rx is suspended after 1st rx received)
    RXMODE_NOREPLY,         // Expecting to receive no Rx message(s)
    RXMODE_DEFAULT = 0        // use current register setting
  };

  enum BurstModulationType
  {
    TONE_BURST_UNMODULATED = 0,
    TONE_BURST_MODULATED
  };

  class GenericATSC
  {
    #region enums

    #endregion

    #region constants

    readonly Guid guidBdaDigitalDemodulator = new Guid(0xef30f379, 0x985b, 0x4d10, 0xb6, 0x40, 0xa7, 0x9d, 0x5e, 0x4, 0xe1, 0xe0);
    #endregion

    #region variables

    readonly bool _isGenericATSC;
    readonly IntPtr _tempValue = Marshal.AllocCoTaskMem(1024);
    readonly IntPtr _tempInstance = Marshal.AllocCoTaskMem(1024);
    readonly IKsPropertySet _propertySet;
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
      KSPropertySupport supported;
      _propertySet.QuerySupported(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, out supported);
      //Log.Log.Info("GenericATSC: BdaDigitalDemodulator supported: {0}", supported);
      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        Log.Log.Info("GenericATSC: Set ModulationType: {0}", channel.ModulationType);
        Marshal.WriteInt32(_tempValue, (Int32)channel.ModulationType);
        int hr = _propertySet.Set(guidBdaDigitalDemodulator, (int)BdaDigitalModulator.MODULATION_TYPE, _tempInstance, 32, _tempValue, 4);
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
