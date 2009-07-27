/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Reflection;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Handles the DiSEqC interface for GenPix BDA driver devices
  /// </summary>
  public class GenPixBDA
  {
    #region constants
    //Guid used to extend the feature of the BDA driver - {0B5221EB-F4C4-4976-B959-EF74427464D9}
    readonly Guid BdaTunerExtentionProperties = new Guid(0x0B5221EB, 0xF4C4, 0x4976, 0xB9, 0x59, 0xEF, 0x74, 0x42, 0x74, 0x64, 0xD9);
    #endregion

    #region variables
    readonly bool _isGenPix;
    readonly IntPtr _ptrDiseqc = IntPtr.Zero;
    readonly IntPtr _ptrTempInstance = IntPtr.Zero;
    readonly IKsPropertySet _propertySet;
    #endregion

    #region enums
    enum enSimpleToneBurst
    {
      SEC_MINI_A = 0x00,
      SEC_MINI_B = 0x01
    }
    #endregion

    #region structs
    public unsafe struct DISEQC_COMMAND
    {
      public byte Framing;
      public byte Address;
      public byte Command;
      public byte Data0;
      public byte Data1;
      public byte Data2;
      public byte MessageLength;
      public static int GetSize()
      {
        return sizeof(DISEQC_COMMAND);
      }
    }

    /*
    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public unsafe struct DISEQC_COMMAND
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
      public byte[] ucMessage;
      public byte ucMessageLength;
    }*/

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="GenPixBDA"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public GenPixBDA(IBaseFilter tunerFilter)
    {
      //check the filter name
      FilterInfo tInfo;
      tunerFilter.QueryFilterInfo(out tInfo);
      Log.Log.Debug("GenPix tuner filter name: {0}", tInfo.achName);
      //check the pin name
      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
      PinInfo pInfo;
      pin.QueryPinInfo(out pInfo);
      Log.Log.Debug("GenPix tuner filter pin name: {0}", pInfo.name);
      if (pin != null)
      {
        _propertySet = pin as IKsPropertySet;
        if (_propertySet != null)
        {
          KSPropertySupport supported;
          _propertySet.QuerySupported(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, out supported);
          if ((supported & KSPropertySupport.Set) != 0)
          {
            Log.Log.Debug("GenPix BDA: DVB-S card found!");
            _isGenPix = true;
            _ptrDiseqc = Marshal.AllocCoTaskMem(1024);
            _ptrTempInstance = Marshal.AllocCoTaskMem(1024);
          }
          else
          {
            Log.Log.Debug("GenPix BDA: DVB-S card NOT found!");
            _isGenPix = false;
            Dispose();
          }
        }
      }
      else
        Log.Log.Info("GenPix BDA: tuner pin not found!");
    }

    /// <summary>
    /// Gets a value indicating whether this instance is a GenPix based card.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is GenPix; otherwise, <c>false</c>.
    /// </value>
    public bool IsGenPix
    {
      get
      {
        return _isGenPix;
      }
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The channels scanning parameters.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (_isGenPix == false)
        return;
      //get previous diseqc message - debug purposes only.
      Log.Log.Info("GenPix: get diseqc");
      int length;
      int hrget = _propertySet.Get(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, _ptrTempInstance, 188, _ptrDiseqc, 188, out length);
      if (hrget != 0)
      {
        Log.Log.Info("GenPix: IKSPropertySet.Get returned: 0x{0:X} - {1}", hrget, HResult.GetDXErrorDescription(hrget));
      }
      string str = "";
      for (int i = 0; i < length; ++i)
        str += String.Format("0x{0:X} ", Marshal.ReadByte(_ptrDiseqc, i));
      Log.Log.Debug("GenPix: get diseqc returned: {0} len: {1}", str, length);

      //get tunning parameters for DiSEqC message
      //bit 0	(1)	: 0=low band, 1 = hi band
      //bit 1 (2) : 0=vertical, 1 = horizontal
      //bit 3 (4) : 0=satellite position A, 1=satellite position B
      //bit 4 (8) : 0=switch option A, 1=switch option  B
      // LNB    option  position
      // 1        A         A
      // 2        A         B
      // 3        B         A
      // 4        B         B
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);
      bool hiBand = BandTypeConverter.IsHiBand(channel, parameters);
      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) || (channel.Polarisation == Polarisation.CircularL));
      //byte cmd = 0xf0;
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);
      
      //ulong diseqc = 0xE0103800;//currently committed switches only. i.e. ports 1-4
      //diseqc += cmd;

      DISEQC_COMMAND DiseqcCommand = new DISEQC_COMMAND();
      /*
        DiseqcCommand.ucMessage[0] = 0xE0;//framing byte
        DiseqcCommand.ucMessage[1] = 0x10;//address byte
        DiseqcCommand.ucMessage[2] = 0x38;//command byte
        DiseqcCommand.ucMessage[3] = cmd;//data byte (port group 0)
        DiseqcCommand.ucMessage[4] = 0;//Need not fill this up
        DiseqcCommand.ucMessage[5] = 0;//Need not fill this up
        DiseqcCommand.ucMessageLength = 4;//Number of Valid bytes in the Command.

      */

      DiseqcCommand.Framing = 0xE0;
      DiseqcCommand.Address = 0x10;
      DiseqcCommand.Command = 0x38;
      DiseqcCommand.Data0 = cmd;
      DiseqcCommand.Data1 = 0;
      DiseqcCommand.Data2 = 0;
      DiseqcCommand.MessageLength = 4;
      
      int len = Marshal.SizeOf(DiseqcCommand);
      //for (int i = 0; i < len; ++i)
      //  Marshal.WriteByte(_ptrDiseqc, i, 0);

      //write the diseqc command to memory
      Log.Log.Debug("GenPix: Write diseqc command");
      //Marshal.StructureToPtr(DiseqcCommand, _ptrDiseqc, true); 
      
      Marshal.WriteByte(_ptrDiseqc, 0, (byte)DiseqcCommand.Framing);//framing byte
      Marshal.WriteByte(_ptrDiseqc, 1, (byte)DiseqcCommand.Address);//address byte
      Marshal.WriteByte(_ptrDiseqc, 2, (byte)DiseqcCommand.Command);//command byte
      Marshal.WriteByte(_ptrDiseqc, 3, (byte)DiseqcCommand.Data0);//data byte (port group 0)
      Marshal.WriteByte(_ptrDiseqc, 4, (byte)DiseqcCommand.Data1);//Need not fill this up
      Marshal.WriteByte(_ptrDiseqc, 5, (byte)DiseqcCommand.Data2);//Need not fill this up
      Marshal.WriteInt16(_ptrDiseqc, 6, (byte)DiseqcCommand.MessageLength);//Number of Valid bytes in the Command.

      /*Marshal.WriteByte(_ptrDiseqc, 0, (byte)((diseqc >> 24) & 0xff));//framing byte
      Marshal.WriteByte(_ptrDiseqc, 1, (byte)((diseqc >> 16) & 0xff));//address byte
      Marshal.WriteByte(_ptrDiseqc, 2, (byte)((diseqc >> 8) & 0xff));//command byte
      Marshal.WriteByte(_ptrDiseqc, 3, (byte)(diseqc & 0xff));//data byte (port group 0)
      */

      //check the command
      string txt = "";
      //int len = DISEQC_COMMAND.GetSize();

      for (int i = 0; i < len; ++i)
        txt += String.Format("0x{0:X} ", Marshal.ReadByte(_ptrDiseqc, i));
      Log.Log.Debug("GenPix: SendDiseq: {0} legth: {1}", txt, len);
      //set it to the tuner pin
      int hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC, _ptrTempInstance, 32, _ptrDiseqc, len);
      if (hr != 0)
      {
        Log.Log.Info("GenPix: SendDiseqCommand returned: 0x{0:X} - {1}", hr, HResult.GetDXErrorDescription(hr));
      }
    }

    /// <summary>
    /// Disposes COM task memory resources
    /// </summary>
    public void Dispose()
    {
      Marshal.FreeCoTaskMem(_ptrDiseqc);
      Marshal.FreeCoTaskMem(_ptrTempInstance);
    }
  }
}