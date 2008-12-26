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
  /// <summary>
  /// Handles the DiSEqC interface for Conexant BDA driver devices
  /// </summary>
  public class ConexantBDA : IDiSEqCController
  {
    #region constants

    readonly Guid BdaTunerExtentionProperties = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);
    #endregion

    #region variables

    readonly bool _isConexant;
    readonly IntPtr _ptrDiseqc = IntPtr.Zero;
    readonly IKsPropertySet _propertySet;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ConexantBDA"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public ConexantBDA(IBaseFilter tunerFilter)
    {
      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        _propertySet = pin as IKsPropertySet;
        if (_propertySet != null)
        {
          KSPropertySupport supported;
          _propertySet.QuerySupported(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, out supported);
          if ((supported & KSPropertySupport.Set) != 0)
          {
            Log.Log.Info("Conexant BDA: DVB-S card found!");
            _isConexant = true;
            _ptrDiseqc = Marshal.AllocCoTaskMem(1024);
          }
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is a conexant based card.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is conexant; otherwise, <c>false</c>.
    /// </value>
    public bool IsConexant
    {
      get
      {
        return _isConexant;
      }
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The channels scanning parameters.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (_isConexant == false)
        return;
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);
      //hack - bypass diseqc settings for single LNB implementations
      if (antennaNr == 0)
        return;
      //end of hack

      //get previous diseqc message - debug purposes only.
      /*Log.Log.Info("Conexant: Get diseqc");
      int length;
      int hrget = _propertySet.Get(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, _tempInstance, 188, _tempValue, 188, out length);
      string str = "";
      for (int i = 0; i < 4; ++i)
        str += String.Format("0x{0:X} ", Marshal.ReadByte(_tempValue, i));
      for (int i = 160; i < 188; i = (i + 4))
        str += String.Format("0x{0:X} ", Marshal.ReadByte(_ptrDiseqc, i));
      Log.Log.WriteFile("Conexant: getdiseqc: {0}", str);*/

      //clear the message params before writing in order to avoid corruption of the diseqc message.
      for (int i = 0; i < 188; ++i)
      {
        Marshal.WriteByte(_ptrDiseqc, i, 0x00);
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

      const int len = 188;
      ulong diseqc = 0xE0103800;//currently committed switches only. i.e. ports 1-4
      diseqc += cmd;

      Marshal.WriteByte(_ptrDiseqc, 0, (byte)((diseqc >> 24) & 0xff));//framing byte
      Marshal.WriteByte(_ptrDiseqc, 1, (byte)((diseqc >> 16) & 0xff));//address byte
      Marshal.WriteByte(_ptrDiseqc, 2, (byte)((diseqc >> 8) & 0xff));//command byte
      Marshal.WriteByte(_ptrDiseqc, 3, (byte)(diseqc & 0xff));//data byte (port group 0)
      Marshal.WriteInt32(_ptrDiseqc, 160, 4);//send_message_length
      Marshal.WriteInt32(_ptrDiseqc, 164, 0);//receive_message_length
      Marshal.WriteInt32(_ptrDiseqc, 168, 3);//amplitude_attenuation
      if (antennaNr == 1)//for simple diseqc switches (i.e. 22KHz tone burst)
      {
        Marshal.WriteByte(_ptrDiseqc, 172, (int)BurstModulationType.TONE_BURST_UNMODULATED);//
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
      Log.Log.WriteFile("Conexant: SendDiseq: {0}", txt);

      int hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, _ptrDiseqc, len, _ptrDiseqc, len);
      Log.Log.Info("Conexant: setdiseqc returned:{0:X}", hr);
    }

    #region IDiSEqCController Members

    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="diSEqC">The DiSEqC command.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      const int len = 188;
      for (int i = 0; i < diSEqC.Length; ++i)
        Marshal.WriteByte(_ptrDiseqc, i, diSEqC[i]);
      Marshal.WriteInt32(_ptrDiseqc, 160, diSEqC.Length);//send_message_length
      Marshal.WriteInt32(_ptrDiseqc, 164, 0);//receive_message_length
      Marshal.WriteInt32(_ptrDiseqc, 168, 3);//amplitude_attenuation
      Marshal.WriteByte(_ptrDiseqc, 172, (int)BurstModulationType.TONE_BURST_MODULATED);//tone_burst_modulated
      Marshal.WriteByte(_ptrDiseqc, 176, (int)DisEqcVersion.DISEQC_VER_1X);
      Marshal.WriteByte(_ptrDiseqc, 180, (int)RxMode.RXMODE_NOREPLY);
      Marshal.WriteByte(_ptrDiseqc, 184, 1);//last_message TRUE

      int hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, _ptrDiseqc, len, _ptrDiseqc, len);
      Log.Log.Info("conexant: setdiseqc returned:{0:X}", hr);
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
  }
}
