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
using DirectShowLib.BDA;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
  class OnAirATSC
  {
    #region enums

    public enum DTVDemodeMode
    {
      QAM64_MODE = 0x00,
      QAM256_MODE = 0x01,
      VSBMODE = 0x03
    };

    public enum NimTunerProperties
    {
      INIM_DMODE_TUNER_ID = 0,		// Get Only
      INIM_DMODE_SWRESET = 1,		// Set Only
      INIM_DMODE_MODE = 2,		// Set Only
      INIM_DMODE_SSINVERSION = 3,		// Set Only
      INIM_DMODE_SNR_REGISTER = 4			// Get Only
    };
    #endregion

    #region constants

    readonly Guid guidOnAirNimTuner = new Guid(0x87f6acbc, 0xbe46, 0x4ea5, 0x90, 0x12, 0x1d, 0x21, 0x1c, 0x47, 0x3f, 0x71);
    #endregion

    #region variables

    readonly bool _isOnAirATSC;
    readonly IntPtr _tempValue = Marshal.AllocCoTaskMem(1024);
    readonly IntPtr _tempInstance = Marshal.AllocCoTaskMem(1024);
    readonly IKsPropertySet _propertySet;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericATSC"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public OnAirATSC(IBaseFilter tunerFilter)
    {
      //May need to add the WDM Filter to the graph for the OnAir tuners Get / Set properties
      //We'll test the BDA filter first in case SDK is dated...
      IPin pin = DsFindPin.ByName(tunerFilter, "MPEG2 Transport");
      if (pin != null)
      {
        _propertySet = tunerFilter as IKsPropertySet;
        if (_propertySet != null)
        {
          KSPropertySupport supported;
          //Use the Modulation mode below the SDK
          _propertySet.QuerySupported(guidOnAirNimTuner, (int)NimTunerProperties.INIM_DMODE_MODE, out supported);
          //Log.Log.Info("OnAirATSC: QuerySupported: {0}", supported);
          if ((supported & KSPropertySupport.Set) != 0)
          {
            Log.Log.Info("OnAirATSC: QAM capable card found!");
            _isOnAirATSC = true;
          }
        }
      }
    }

    /// <summary>
    /// sets the QAM modulation for ATSC cards under XP
    /// </summary>
    public void SetOnAirQam(ATSCChannel channel)
    {
      int hr;
      KSPropertySupport supported;
      _propertySet.QuerySupported(guidOnAirNimTuner, (int)NimTunerProperties.INIM_DMODE_MODE, out supported);
      Log.Log.Info("OnAirATSC: QuerySupported: {0}", supported);
      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        //From the SDK it looks as if for 64Qam & 256Qam you have to set an additional tuner property first
        //i.e. where hr = _propertySet.Set(guidOnAirNimTuner, (int)NimTunerProperties.INIM_DMODE_SSINVERSION, _tempInstance, 32, _tempValue, 4);
        //_tempValue for the above line would be 0 and if that fails then 1Log.Log.Info("OnAirATSC: Set ModulationType: {0}", channel.ModulationType);

        //Below is a BDA test as SDK was based on WDM
        int onAirMod = (int)DTVDemodeMode.VSBMODE;
        if (channel.ModulationType == ModulationType.Mod64Qam)
          onAirMod = (int)DTVDemodeMode.QAM64_MODE;
        if (channel.ModulationType == ModulationType.Mod256Qam)
          onAirMod = (int)DTVDemodeMode.QAM256_MODE;
        Marshal.WriteInt32(_tempValue, onAirMod);
        hr = _propertySet.Set(guidOnAirNimTuner, (int)NimTunerProperties.INIM_DMODE_MODE, _tempInstance, 32, _tempValue, 4);
        if (hr != 0)
        {
          Log.Log.Info("OnAirATSC: Set returned:{0:X}", hr);
        }
      }
      //below is for info only - uncomment if debugging
      if ((supported & KSPropertySupport.Get) == KSPropertySupport.Get)
      {
        int length;
        Log.Log.Info("OnAirATSC: Get ModulationType");
        Marshal.WriteInt32(_tempValue, 0);
        hr = _propertySet.Get(guidOnAirNimTuner, (int)NimTunerProperties.INIM_DMODE_TUNER_ID, _tempInstance, 32, _tempValue, 4, out length);
        Log.Log.Info("OnAirATSC: Get Modulation returned:{0:X} len:{1} value:{2}", hr, length, Marshal.ReadInt32(_tempValue));
        //Added this here to see the the Tuner Get properties report anything also.
        hr = _propertySet.Get(guidOnAirNimTuner, (int)NimTunerProperties.INIM_DMODE_SSINVERSION, _tempInstance, 32, _tempValue, 4, out length);
        Log.Log.Info("OnAirATSC: Get Inversion returned:{0:X} len:{1} value:{2}", hr, length, Marshal.ReadInt32(_tempValue));
      }
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

    public bool IsOnAirATSC
    {
      get
      {
        return _isOnAirATSC;
      }
    }
  }
}
