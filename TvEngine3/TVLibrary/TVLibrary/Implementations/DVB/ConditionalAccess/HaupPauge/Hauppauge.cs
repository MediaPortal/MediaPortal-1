/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  public class Hauppauge : IDiSEqCController
  {
    #region enums
    enum BdaNodes
    {
      BDA_TUNER_NODE = 0,
      BDA_DEMODULATOR_NODE
    };
    enum BdaTunerExtension
    {
      KSPROPERTY_BDA_DISEQC = 0,
      KSPROPERTY_BDA_PILOT = 0x20,
      KSPROPERTY_BDA_ROLL_OFF = 0x21
    };
    enum DisEqcVersion
    {
      DISEQC_VER_1X = 1,
      DISEQC_VER_2X,
      ECHOSTAR_LEGACY,	// (not supported)
      DISEQC_VER_UNDEF = 0	// undefined (results in an error)
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
    enum RollOff
    {
      HCW_ROLL_OFF_NOT_SET = -1,
      HCW_ROLL_OFF_NOT_DEFINED = 0,
      HCW_ROLL_OFF_20 = 1,         // .20 Roll Off (DVB-S2 Only)
      HCW_ROLL_OFF_25,             // .25 Roll Off (DVB-S2 Only)
      HCW_ROLL_OFF_35,             // .35 Roll Off (DVB-S2 Only) (Default for DVB-S2)
      HCW_ROLL_OFF_MAX
    };
    enum Pilot
    {
      HCW_PILOT_NOT_SET = -1,
      HCW_PILOT_NOT_DEFINED = 0,
      HCW_PILOT_OFF = 1,           // Pilot Off (DVB-S2 Only) (Default for DVB-S2)
      HCW_PILOT_ON,                // Pilot On  (DVB-S2 Only)
      HCW_PILOT_MAX
    }
    #endregion

    #region constants
    const byte DISEQC_TX_BUFFER_SIZE = 150;	// 3 bytes per message * 50 messages
    const byte DISEQC_RX_BUFFER_SIZE = 8;		// reply fifo size, do not increase
    #endregion

    #region variables
    Guid BdaTunerExtentionProperties = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0x00, 0xa0, 0xc9, 0xf2, 0x1f, 0xc7);
    bool _isHauppauge = false;
    IntPtr _ptrDiseqc = IntPtr.Zero;
    IntPtr _tempPtr = Marshal.AllocCoTaskMem(1024);
    IntPtr _tempValue = Marshal.AllocCoTaskMem(1024);
    DirectShowLib.IKsPropertySet _propertySet = null;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Hauppauge"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="analyzerFilter">The analyzer filter.</param>
    public Hauppauge(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
    {
      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        _propertySet = pin as DirectShowLib.IKsPropertySet;
        if (_propertySet != null)
        {
          KSPropertySupport supported;
          _propertySet.QuerySupported(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, out supported);
          if ((supported & KSPropertySupport.Set) != 0)
          {
            _isHauppauge = true;
            _ptrDiseqc = Marshal.AllocCoTaskMem(1024);
          }
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is hauppauge.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is hauppauge; otherwise, <c>false</c>.
    /// </value>
    public bool IsHauppauge
    {
      get
      {
        return _isHauppauge;
      }
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (_isHauppauge == false) return;
      bool hiBand = BandTypeConverter.IsHiBand(channel, parameters);
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);

      //bit 0	(1)	: 0=low band, 1 = hi band
      //bit 1 (2) : 0=vertical, 1 = horizontal
      //bit 3 (4) : 0=satellite position A, 1=satellite position B
      //bit 4 (8) : 0=switch option A, 1=switch option  B
      // LNB    option  position
      // 1        A         A
      // 2        A         B
      // 3        B         A
      // 4        B         B

      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) || (channel.Polarisation == Polarisation.CircularL));
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);

      int len = 188;//sizeof(DISEQC_MESSAGE_PARAMS);
      ulong diseqc = 0xE0103800;
      diseqc += cmd;

      Marshal.WriteByte(_ptrDiseqc, 0, (byte)((diseqc >> 24) & 0xff));
      Marshal.WriteByte(_ptrDiseqc, 1, (byte)((diseqc >> 16) & 0xff));
      Marshal.WriteByte(_ptrDiseqc, 2, (byte)((diseqc >> 8) & 0xff));
      Marshal.WriteByte(_ptrDiseqc, 3, (byte)(diseqc & 0xff));
      Marshal.WriteInt32(_ptrDiseqc, 160, (Int32)4);//send_message_length
      Marshal.WriteInt32(_ptrDiseqc, 164, (Int32)0);//receive_message_length
      Marshal.WriteInt32(_ptrDiseqc, 168, (Int32)3);//amplitude_attenuation
      Marshal.WriteByte(_ptrDiseqc, 172, 1);//tone_burst_modulated
      Marshal.WriteByte(_ptrDiseqc, 176, (int)DisEqcVersion.DISEQC_VER_1X);
      Marshal.WriteByte(_ptrDiseqc, 180, (int)RxMode.RXMODE_NOREPLY);
      Marshal.WriteByte(_ptrDiseqc, 184, 1);//last_message

      int hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, _ptrDiseqc, len, _ptrDiseqc, len);
      Log.Log.Info("Hauppauge: setdiseqc returned:{0:X}", hr);
    }

    #region IDiSEqCController Members

    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="diSEqC">The DiSEqC command.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      int len = 188;//sizeof(DISEQC_MESSAGE_PARAMS);
      for (int i = 0; i < diSEqC.Length; ++i)
        Marshal.WriteByte(_ptrDiseqc, i, diSEqC[i]);
      Marshal.WriteInt32(_ptrDiseqc, 160, (Int32)diSEqC.Length);//send_message_length
      Marshal.WriteInt32(_ptrDiseqc, 164, (Int32)0);//receive_message_length
      Marshal.WriteInt32(_ptrDiseqc, 168, (Int32)3);//amplitude_attenuation
      Marshal.WriteByte(_ptrDiseqc, 172, 1);//tone_burst_modulated
      Marshal.WriteByte(_ptrDiseqc, 176, (int)DisEqcVersion.DISEQC_VER_1X);
      Marshal.WriteByte(_ptrDiseqc, 180, (int)RxMode.RXMODE_NOREPLY);
      Marshal.WriteByte(_ptrDiseqc, 184, 1);//last_message

      int hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, _ptrDiseqc, len, _ptrDiseqc, len);
      Log.Log.Info("hauppauge: setdiseqc returned:{0:X}", hr);
      return (hr == 0);
    }

    /// <summary>
    /// gets the diseqc reply
    /// </summary>
    /// <param name="reply">The reply.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      reply = new byte[1];
      return false;
    }

    #endregion

    /// <summary>
    /// sets the dvb-s2 pilot / roll-off
    /// </summary>
    public void SetDVBS2PilotRolloff()
    {
      //Set the Pilot
      int hr;
      KSPropertySupport supported;
      _propertySet.QuerySupported(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_PILOT, out supported);
      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        Log.Log.Info("Hauppauge: Set Pilot");
        Marshal.WriteInt32(_tempValue, (Int32)Pilot.HCW_PILOT_OFF);
        hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_PILOT, _tempPtr, 1024, _tempValue, 4);
        Log.Log.Info("Hauppauge: Set Pilot returned:{0:X}", hr);
      }

      //get Pilot
      int length;
      if ((supported & KSPropertySupport.Get) == KSPropertySupport.Get)
      {
        Log.Log.Info("Hauppauge: Get Pilot");
        Marshal.WriteInt32(_tempValue, (Int32)0);
        hr = _propertySet.Get(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_PILOT, _tempPtr, 1024, _tempValue, 4, out length);
        Log.Log.Info("Hauppauge: Get Pilot returned:{0:X} len:{1} value:{2}", hr, length, Marshal.ReadInt32(_tempValue));
      }
      
      //Set the Roll-off
      _propertySet.QuerySupported(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_ROLL_OFF, out supported);
      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        Log.Log.Info("Hauppauge: Set BDA Roll-Off");
        Marshal.WriteInt32(_tempValue, (Int32)RollOff.HCW_ROLL_OFF_35);
        hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_ROLL_OFF, _tempPtr, 1024, _tempValue, 4);
        Log.Log.Info("Hauppauge: Set BDA Roll-Off returned:{0:X}", hr);
      }

      //get roll-off
      if ((supported & KSPropertySupport.Get) == KSPropertySupport.Get)
      {
        Log.Log.Info("Hauppauge: Get BDA Roll-Off");
        Marshal.WriteInt32(_tempValue, (Int32)0);
        hr = _propertySet.Get(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_ROLL_OFF, _tempPtr, 1024, _tempValue, 4, out length);
        Log.Log.Info("Hauppauge: Get BDA Roll-Off returned:{0:X} len:{1} value:{2}", hr, length, Marshal.ReadInt32(_tempValue));
      }
    }
  }
}