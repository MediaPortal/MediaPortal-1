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
using TvLibrary.Channels;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.DVB
{
  public class Hauppauge : IDiSEqCController
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
    #endregion

    #region constants
    const byte DISEQC_TX_BUFFER_SIZE = 150;	// 3 bytes per message * 50 messages
    const byte DISEQC_RX_BUFFER_SIZE = 8;		// reply fifo size, do not increase
    Guid BdaTunerExtentionProperties = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0x00, 0xa0, 0xc9, 0xf2, 0x1f, 0xc7);
    #endregion

    #region variables
    bool _isHauppauge = false;
    IntPtr _ptrDiseqc = IntPtr.Zero;
    IntPtr _tempValue = Marshal.AllocCoTaskMem(1024);
    IntPtr _tempInstance = Marshal.AllocCoTaskMem(1024);
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
            Log.Log.Info("Hauppauge: DVB-S card found!");
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
    /// <param name="parameters">The scanparameters.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (_isHauppauge == false) return;
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);
      //hack - bypass diseqc settings for single LNB implementations
      if (antennaNr == 0) return;
      //end of hack

      //get previous diseqc message - debug purposes only.
      /*Log.Log.Info("Hauppauge: Get diseqc");
      int length;
      int hrget = _propertySet.Get(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, _tempInstance, 188, _tempValue, 188, out length);
      string str = "";
      for (int i = 0; i < 4; ++i)
        str += String.Format("0x{0:X} ", Marshal.ReadByte(_tempValue, i));
      for (int i = 160; i < 188; i = (i + 4))
        str += String.Format("0x{0:X} ", Marshal.ReadByte(_ptrDiseqc, i));
      Log.Log.WriteFile("Hauppauge: getdiseqc: {0}", str);*/

      //clear the message params before writing in order to avoid corruption of the diseqc message.
      for (int i = 0; i < 188; ++i)
      {
        Marshal.WriteByte(_ptrDiseqc, i, (byte)0x00);
      }

      bool hiBand = BandTypeConverter.IsHiBand(channel, parameters);

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
      ulong diseqc = 0xE0103800;//currently committed switches only. i.e. ports 1-4
      diseqc += cmd;

      Marshal.WriteByte(_ptrDiseqc, 0, (byte)((diseqc >> 24) & 0xff));//framing byte
      Marshal.WriteByte(_ptrDiseqc, 1, (byte)((diseqc >> 16) & 0xff));//address byte
      Marshal.WriteByte(_ptrDiseqc, 2, (byte)((diseqc >> 8) & 0xff));//command byte
      Marshal.WriteByte(_ptrDiseqc, 3, (byte)(diseqc & 0xff));//data byte (port group 0)
      Marshal.WriteInt32(_ptrDiseqc, 160, (Int32)4);//send_message_length
      Marshal.WriteInt32(_ptrDiseqc, 164, (Int32)0);//receive_message_length
      Marshal.WriteInt32(_ptrDiseqc, 168, (Int32)3);//amplitude_attenuation
      if (antennaNr == 1)//for simple diseqc switches (i.e. 22KHz tone burst)
      {
        Marshal.WriteByte(_ptrDiseqc, 172, (int)BurstModulationType.TONE_BURST_UNMODULATED);
      }
      else
      {
        Marshal.WriteByte(_ptrDiseqc, 172, (int)BurstModulationType.TONE_BURST_MODULATED);//default to tone_burst_modulated
      }
      Marshal.WriteByte(_ptrDiseqc, 176, (int)DisEqcVersion.DISEQC_VER_1X);//default
      Marshal.WriteByte(_ptrDiseqc, 180, (int)RxMode.RXMODE_NOREPLY);//default
      Marshal.WriteByte(_ptrDiseqc, 184, 1);//last_message TRUE */

      string txt = "";
      for (int i = 0; i < 4; ++i)
        txt += String.Format("0x{0:X} ", Marshal.ReadByte(_ptrDiseqc, i));
      for (int i = 160; i < 188; i = (i + 4))
        txt += String.Format("0x{0:X} ", Marshal.ReadInt32(_ptrDiseqc, i));
      Log.Log.WriteFile("Hauppauge: SendDiseq: {0}", txt);

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
    public void SetDVBS2PilotRolloff(DVBSChannel channel)
    {
      //Set the Pilot
      int hr;
      int length;
      KSPropertySupport supported;
      _propertySet.QuerySupported(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_PILOT, out supported);

      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        Log.Log.Info("Hauppauge: Set Pilot: {0}", channel.Pilot);
        Marshal.WriteInt32(_tempValue, (Int32)channel.Pilot);
        hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_PILOT, _tempInstance, 32, _tempValue, 4);
        Log.Log.Info("Hauppauge:  returned:{0:X}", hr);
      }
      //get Pilot
      /*if ((supported & KSPropertySupport.Get) == KSPropertySupport.Get)
      {
        Log.Log.Info("Hauppauge: Get Pilot");
        hr = _propertySet.Get(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_PILOT, _tempInstance, 32, _tempValue, 4, out length);
        Log.Log.Info("Hauppauge:   returned:{0:X} len:{1} value:{2}", hr, length, Marshal.ReadInt32(_tempValue));
      }*/
      //Set the Roll-off
      _propertySet.QuerySupported(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_ROLL_OFF, out supported);
      if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
      {
        Log.Log.Info("Hauppauge: Set Roll-Off: {0}", channel.Rolloff);
        Marshal.WriteInt32(_tempValue, (Int32)channel.Rolloff);
        hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_ROLL_OFF, _tempInstance, 32, _tempValue, 4);
        Log.Log.Info("Hauppauge:   returned:{0:X}", hr);
      }
      //get roll-off
      /*if ((supported & KSPropertySupport.Get) == KSPropertySupport.Get)
      {
        Log.Log.Info("Hauppauge: Get BDA Roll-Off");
        hr = _propertySet.Get(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_ROLL_OFF, _tempInstance, 32, _tempValue, 4, out length);
        Log.Log.Info("Hauppauge:   returned:{0:X} len:{1} value:{2}", hr, length, Marshal.ReadInt32(_tempValue));
      }*/
    }
  }
}
