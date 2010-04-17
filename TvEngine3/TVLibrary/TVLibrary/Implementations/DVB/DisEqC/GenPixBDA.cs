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
    private readonly Guid BdaTunerExtentionProperties = new Guid(0x0B5221EB, 0xF4C4, 0x4976, 0xB9, 0x59, 0xEF, 0x74,
                                                                 0x42, 0x74, 0x64, 0xD9);

    #endregion

    #region variables

    private readonly bool _isGenPix;
    private readonly IntPtr _ptrDiseqc = IntPtr.Zero;
    private readonly IntPtr _ptrTempInstance = IntPtr.Zero;
    private readonly IKsPropertySet _propertySet;

    #endregion

    #region enums

    private enum enSimpleToneBurst
    {
      SEC_MINI_A = 0x00,
      SEC_MINI_B = 0x01
    }

    #endregion

    #region structs

    /// <summary>
    /// Sets up the DiSEqC struct for GenPix DVB-S cards.
    /// </summary>
    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private unsafe class DISEQC_COMMAND
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] ucMessage;
      public byte ucMessageLength;

      public DISEQC_COMMAND()
      {
        ucMessage = new byte[6];
      }
    }

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
          _propertySet.QuerySupported(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC,
                                      out supported);
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
      get { return _isGenPix; }
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
      Log.Log.Debug("SendDiseqc: {0},{1}", parameters.ToString(), channel.ToString());

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
      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                           (channel.Polarisation == Polarisation.CircularL));
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);

      DISEQC_COMMAND DiseqcCommand = new DISEQC_COMMAND();
      DiseqcCommand.ucMessage[0] = 0xE0; //framing byte
      DiseqcCommand.ucMessage[1] = 0x10; //address byte
      DiseqcCommand.ucMessage[2] = 0x38; //command byte
      DiseqcCommand.ucMessage[3] = cmd; //data byte (port group 0)
      DiseqcCommand.ucMessage[4] = 0; //Need not fill this up
      DiseqcCommand.ucMessage[5] = 0; //Need not fill this up
      DiseqcCommand.ucMessageLength = 4; //Number of Valid bytes in the Command.

      Marshal.StructureToPtr(DiseqcCommand, _ptrDiseqc, false);
      //get the length of the structure command - usually 7 bytes.
      int len = Marshal.SizeOf(DiseqcCommand);

      string txt = "";
      for (int i = 0; i < len; ++i)
        txt += String.Format("0x{0:X} ", Marshal.ReadByte(_ptrDiseqc, i));
      Log.Log.Debug("GenPix: SendDiseqCommand: {0} with length {1}", txt, len);
      //set the DisEqC command to the tuner pin
      int hr = _propertySet.Set(BdaTunerExtentionProperties, (int)BdaTunerExtension.KSPROPERTY_BDA_DISEQC,
                                _ptrTempInstance, 32, _ptrDiseqc, len);
      if (hr != 0)
      {
        Log.Log.Info("GenPix: SendDiseqCommand returned: 0x{0:X} - {1}{2}", hr, HResult.GetDXErrorString(hr),
                     HResult.GetDXErrorDescription(hr));
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