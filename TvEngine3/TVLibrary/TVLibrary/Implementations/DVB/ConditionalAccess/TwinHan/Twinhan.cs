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
  /// Summary description for Twinhan.
  /// </summary>
  public class Twinhan : IDiSEqCController
  {
    #region guids
    readonly Guid THBDA_TUNER = new Guid("E5644CC4-17A1-4eed-BD90-74FDA1D65423");
#pragma warning disable 169
    readonly Guid GUID_THBDA_CMD = new Guid("255E0082-2017-4b03-90F8-856A62CB3D67");
#pragma warning restore 169
    readonly uint THBDA_IOCTL_CI_SEND_PMT = 0xaa000338;     //CTL_CODE(THBDA_IO_INDEX, 206, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_CHECK_INTERFACE = 0xaa0001e4; //CTL_CODE(THBDA_IO_INDEX, 121, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_CI_GET_STATE = 0xaa000320;    //CTL_CODE(THBDA_IO_INDEX, 200, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_CI_GET_PMT_REPLY = 0xaa000348;//CTL_CODE(THBDA_IO_INDEX, 210, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_SET_DiSEqC = 0xaa0001a0;//CTL_CODE(THBDA_IO_INDEX, 104, METHOD_BUFFERED, FILE_ANY_ACCESS) 
    readonly uint THBDA_IOCTL_GET_DiSEqC = 0xaa0001a4;//CTL_CODE(THBDA_IO_INDEX, 105, METHOD_BUFFERED, FILE_ANY_ACCESS) 
    readonly uint THBDA_IOCTL_SET_LNB_DATA = 0xaa000200;//CTL_CODE(THBDA_IO_INDEX, 128, METHOD_BUFFERED, FILE_ANY_ACCESS) 
    #endregion

    #region variables

    readonly bool _initialized;
    readonly bool _isTwinHanCard;
    readonly bool _camPresent;
    readonly IBaseFilter _captureFilter;
    readonly IntPtr _ptrPmt;
    readonly IntPtr _ptrDiseqc;
    readonly IntPtr _ptrDwBytesReturned;
    readonly IntPtr _thbdaBuf;
    readonly IntPtr _ptrOutBuffer;
    readonly IntPtr _ptrOutBuffer2;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Twinhan"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Twinhan(IBaseFilter tunerFilter)
    {
      _ptrPmt = Marshal.AllocCoTaskMem(8192);
      _ptrDwBytesReturned = Marshal.AllocCoTaskMem(20);
      _thbdaBuf = Marshal.AllocCoTaskMem(8192);
      _ptrOutBuffer = Marshal.AllocCoTaskMem(8192);
      _ptrOutBuffer2 = Marshal.AllocCoTaskMem(8192);
      _ptrDiseqc = Marshal.AllocCoTaskMem(8192);
      _captureFilter = tunerFilter;
      _initialized = false;
      _camPresent = false;
      _isTwinHanCard = false;
      if (_captureFilter != null)
      {
        _isTwinHanCard = IsTwinhan;
        if (_isTwinHanCard)
        {
          _camPresent = IsCamPresent();
          Log.Log.WriteFile("Twinhan:  CAM detected:{0}", _camPresent);
        }
      }
      _initialized = true;
    }
    /// <summary>
    /// Reutns if the tuner specified in the constructor supports twinhan CI/CAM handling
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is twinhan compatible; otherwise, <c>false</c>.
    /// </value>
    public bool IsTwinhan
    {
      get
      {
        if (_initialized)
          return _isTwinHanCard;
        bool result = IsTwinhanCard();
        if (result)
        {
          if (IsCamPresent())
          {
            Log.Log.WriteFile("Twinhan:  CAM inserted");
          }
        }
        return result;
      }
    }

    /// <summary>
    /// Gets the status of the CAM and CI.
    /// </summary>
    /// <param name="CIState">State of the CI.</param>
    /// <param name="MMIState">State of the MMI.</param>
    public void GetCAMStatus(out uint CIState, out uint MMIState)
    {
      CIState = 0;
      MMIState = 0;
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          try
          {
            const int thbdaLen = 0x28;
            Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
            Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
            Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
            Marshal.WriteByte(_thbdaBuf, 8, 0x90);
            Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
            Marshal.WriteByte(_thbdaBuf, 10, 0x85);
            Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
            Marshal.WriteByte(_thbdaBuf, 12, 0x62);
            Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
            Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
            Marshal.WriteByte(_thbdaBuf, 15, 0x67);
            Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_CI_GET_STATE);//control code
            Marshal.WriteInt32(_thbdaBuf, 20, (int)IntPtr.Zero); //LPVOID inbuffer
            Marshal.WriteInt32(_thbdaBuf, 24, 0);                //DWORD inbuffersize
            Marshal.WriteInt32(_thbdaBuf, 28, _ptrOutBuffer.ToInt32()); //LPVOID outbuffer
            Marshal.WriteInt32(_thbdaBuf, 32, 4096);                //DWORD outbuffersize
            Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//LPVOID bytesreturned
            int hr = propertySet.Set(propertyGuid, 0, _thbdaBuf, thbdaLen, _thbdaBuf, thbdaLen);
            if (hr == 0)
            {
              CIState = (uint)Marshal.ReadInt32(_ptrOutBuffer, 0);
              MMIState = (uint)Marshal.ReadInt32(_ptrOutBuffer, 4);
              Log.Log.WriteFile("Twinhan:  CI State:{0:X} MMI State:{1:X}", CIState, MMIState);
            }
            else
            {
              Log.Log.WriteFile("Twinhan:  unable to get CI State hr:{0:X}", hr);
            }
          }
          finally
          {
            Log.Log.WriteFile("Twinhan: CI status read");
          }
        }
      }
    }

    /// <summary>
    /// Determines whether a cam is present or not
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if cam is present; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      if (_initialized)
        return _camPresent;
      uint CIState;
      uint MMIState;
      GetCAMStatus(out CIState, out MMIState);
      if (CIState != 0)
        return true;
      return false;
    }

    /// <summary>
    /// Determines whether the cam is ready
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if cam is ready; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamReady()
    {
      return true;
    }

    /// <summary>
    /// Determines whether this card is twinhan compatible
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if card is twinhan compatible; otherwise, <c>false</c>.
    /// </returns>
    public bool IsTwinhanCard()
    {
      if (_initialized)
        return _isTwinHanCard;
      //Log.Log.WriteFile("Twinhan:  Check for twinhan driver");
      bool success = false;
      try
      {
        const int thbdaLen = 0x28;
        Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
        Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
        Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
        Marshal.WriteByte(_thbdaBuf, 8, 0x90);
        Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
        Marshal.WriteByte(_thbdaBuf, 10, 0x85);
        Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
        Marshal.WriteByte(_thbdaBuf, 12, 0x62);
        Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
        Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
        Marshal.WriteByte(_thbdaBuf, 15, 0x67);
        Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_CHECK_INTERFACE);//control code
        Marshal.WriteInt32(_thbdaBuf, 20, (int)IntPtr.Zero);
        Marshal.WriteInt32(_thbdaBuf, 24, 0);
        Marshal.WriteInt32(_thbdaBuf, 28, (int)IntPtr.Zero);
        Marshal.WriteInt32(_thbdaBuf, 32, 0);
        Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);
        IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
        if (pin != null)
        {
          IKsPropertySet propertySet = pin as IKsPropertySet;
          if (propertySet != null)
          {
            Guid propertyGuid = THBDA_TUNER;
            int hr = propertySet.Set(propertyGuid, 0, _thbdaBuf, thbdaLen, _thbdaBuf, thbdaLen);
            if (hr == 0)
            {
              success = true;
            }
          }
        }
      }
      finally
      {
        Log.Log.WriteFile("Twinhan: CI detection finished");
      }
      return success;
    }

    /// <summary>
    /// Gets the answer from the CAM after sending the PMT .
    /// </summary>
    /// <returns>string containing the CAM answer</returns>
    public string GetPmtReply()
    {
      if (IsCamPresent() == false)
      {
        return "";
      }
      for (int i = 0; i < 1024; ++i)
      {
        Marshal.WriteByte(_ptrPmt, i, 0);
      }
      const int thbdaLen = 0x28;
      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_CI_GET_PMT_REPLY);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, (int)IntPtr.Zero);//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, 0);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, _ptrPmt.ToInt32());//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, 1024);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          int back = Marshal.ReadInt32(_ptrDwBytesReturned);
          if (hr != 0)
          {
            Log.Log.WriteFile("Twinhan:  GetPmtReply() failed 0x{0:X}", hr);
          }
          Log.Log.WriteFile("Twinhan:  GetPmtReply() returned {0} bytes", back);
          Marshal.ReleaseComObject(propertySet);
          try
          {
            System.IO.File.Delete("c:\\pmtreply.dat");
          } catch (Exception ex)
          {
            Log.Log.WriteFile("Error while deleting file: ", ex);
          }
          using (System.IO.FileStream stream = new System.IO.FileStream("c:\\pmtreply.dat", System.IO.FileMode.OpenOrCreate))
          {
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream))
            {
              for (int i = 0; i < 1024; ++i)
              {
                byte k = Marshal.ReadByte(_ptrPmt, i);
                writer.Write(k);
              }
              writer.Flush();
            }
          }
        }
        Marshal.ReleaseComObject(pin);
      }
      return "";
    }

    /// <summary>
    /// Sends the PMT to the CAM/CI module
    /// </summary>
    /// <param name="caPMT">The caPMT structure.</param>
    /// <param name="caPMTLen">The caPMT lenght</param>
    /// <returns>false on failure to send PMT</returns>
    public bool SendPMT(byte[] caPMT, int caPMTLen)
    {
      if (IsCamPresent() == false)
        return true; // Nothing to do
      Log.Log.WriteFile("Twinhan: Send PMT, len: {0}", caPMTLen);
      if (caPMT.Length == 0)
        return false;

      string line = "";
      for (int i = 0; i < caPMTLen; ++i)
        line += String.Format("{0:X} ", caPMT[i]);
      Log.Log.WriteFile(" capmt:{0}", line);

      bool suceeded = false;
      Marshal.Copy(caPMT, 0, _ptrPmt, caPMTLen);
      const int thbdaLen = 0x28;
      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_CI_SEND_PMT);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, _ptrPmt.ToInt32());//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, caPMTLen);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, (int)IntPtr.Zero);//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, 0);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int failedAttempts = 0;
          while (true)
          {
            int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
            if (hr != 0)
            {
              failedAttempts++;
              Log.Log.WriteFile("Twinhan:  CAM failed 0x{0:X}", hr);
              if (((uint)hr) == (0x8007001F) && failedAttempts < 10)
              {
                Log.Log.Debug(" sleep and then retry again, failedAttempts: {0}", failedAttempts);
                System.Threading.Thread.Sleep(100);
                continue;
              }
            }
            else
            {
              Log.Log.WriteFile("Twinhan:  CAM returned ok 0x{0:X}", hr);
              suceeded = true;
            }
            break;
          }
          Marshal.ReleaseComObject(propertySet);
        }
        Marshal.ReleaseComObject(pin);
      }
      return suceeded;
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="parameters">Scan parameters</param>
    /// <param name="channel">The channel.</param>
    public void SendDiseqCommand(ScanParameters parameters, DVBSChannel channel)
    {
      byte disEqcPort = (byte)BandTypeConverter.GetAntennaNr(channel);
      Int32 LNBLOFLowBand;
      Int32 LNBLOFHighBand;
      Int32 LNBLOFHiLoSW;
      BandTypeConverter.GetDefaultLnbSetup(parameters, channel.BandType, out LNBLOFLowBand, out LNBLOFHighBand, out LNBLOFHiLoSW);
      byte turnon22Khz = BandTypeConverter.IsHiBand(channel, parameters) ? (byte)2 : (byte)1;
      SetLnbData(true, LNBLOFLowBand, LNBLOFHighBand, LNBLOFHiLoSW, turnon22Khz, disEqcPort);
      SendDiseqcCommandTest(parameters, channel);
    }

    void SetLnbData(bool lnbPower, int LNBLOFLowBand, int LNBLOFHighBand, int LNBLOFHiLoSW, int turnon22Khz, int disEqcPort)
    {
      Log.Log.WriteFile("Twinhan:  SetLnb diseqc port:{0} 22khz:{1} low:{2} hi:{3} switch:{4} power:{5}", disEqcPort, turnon22Khz, LNBLOFLowBand, LNBLOFHighBand, LNBLOFHiLoSW, lnbPower);
      const int thbdaLen = 0x28;
      const int disEqcLen = 20;
      Marshal.WriteByte(_ptrDiseqc, 0, (byte)(lnbPower ? 1 : 0));              // 0: LNB_POWER
      Marshal.WriteByte(_ptrDiseqc, 1, 0);              // 1: Tone_Data_Burst (Tone_Data_OFF:0 | Tone_Burst_ON:1 | Data_Burst_ON:2)
      Marshal.WriteByte(_ptrDiseqc, 2, 0);
      Marshal.WriteByte(_ptrDiseqc, 3, 0);
      Marshal.WriteInt32(_ptrDiseqc, 4, LNBLOFLowBand); // 4: ulLNBLOFLowBand   LNBLOF LowBand MHz
      Marshal.WriteInt32(_ptrDiseqc, 8, LNBLOFHighBand);// 8: ulLNBLOFHighBand  LNBLOF HighBand MHz
      Marshal.WriteInt32(_ptrDiseqc, 12, LNBLOFHiLoSW); //12: ulLNBLOFHiLoSW   LNBLOF HiLoSW MHz
      Marshal.WriteByte(_ptrDiseqc, 16, (byte)turnon22Khz);   //16: f22K_Output (F22K_Output_HiLo:0 | F22K_Output_Off:1 | F22K_Output_On:2
      Marshal.WriteByte(_ptrDiseqc, 17, (byte)disEqcPort);    //17: DiSEqC_Port
      Marshal.WriteByte(_ptrDiseqc, 18, 0);
      Marshal.WriteByte(_ptrDiseqc, 19, 0);
      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_SET_LNB_DATA);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, _ptrDiseqc.ToInt32());//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, disEqcLen);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, (int)IntPtr.Zero);//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, 0);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          if (hr != 0)
          {
            Log.Log.WriteFile("Twinhan:  SetLNB failed 0x{0:X}", hr);
          }
          else
            Log.Log.WriteFile("Twinhan:  SetLNB ok 0x{0:X}", hr);
          Marshal.ReleaseComObject(propertySet);
        }
        Marshal.ReleaseComObject(pin);
      }
    }

    #region IDiSEqCController Members
    ///<summary>
    /// Send DiseqC Command test
    ///</summary>
    ///<param name="parameters">Scan parameters</param>
    ///<param name="channel">Channel</param>
    public void SendDiseqcCommandTest(ScanParameters parameters, DVBSChannel channel)
    {
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);
      bool hiBand = BandTypeConverter.IsHiBand(channel, parameters);
      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) || (channel.Polarisation == Polarisation.CircularL));
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);
      byte[] diseqc = new byte[4];
      diseqc[0] = 0xe0;
      diseqc[1] = 0x10;
      diseqc[2] = 0x38;
      diseqc[3] = cmd;
      SendDiSEqCCommand(diseqc);
    }

    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="diSEqC">The DiSEqC command.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      const int thbdaLen = 0x28;
      const int disEqcLen = 16;
      for (int i = 0; i < 12; ++i)
        Marshal.WriteByte(_ptrDiseqc, 4 + i, 0);
      Marshal.WriteInt32(_ptrDiseqc, 0, diSEqC.Length);//command len
      for (int i = 0; i < diSEqC.Length; ++i)
      {
        Marshal.WriteByte(_ptrDiseqc, 4 + i, diSEqC[i]);
      }
      string line = "";
      for (int i = 0; i < disEqcLen; ++i)
      {
        byte k = Marshal.ReadByte(_ptrDiseqc, i);
        line += String.Format("{0:X} ", k);
      }
      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_SET_DiSEqC);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, _ptrDiseqc.ToInt32());//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, disEqcLen);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, (int)IntPtr.Zero);//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, 0);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned
      bool success = false;
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          if (hr != 0)
          {
            Log.Log.WriteFile("TwinHan DiSEqC cmd:{0} failed 0x{1:X}", line, hr);
          }
          else
          {
            Log.Log.WriteFile("TwinHan DiSEqC cmd:{0} succeeded", line);
            success = true;
          }
          Marshal.ReleaseComObject(propertySet);
        }
        Marshal.ReleaseComObject(pin);
      }
      return success;
    }

    /// <summary>
    /// Sends a diseqc command and reads a reply
    /// </summary>
    /// <param name="reply">The reply.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      reply = new byte[1];
      reply[0] = 0;
      const int thbdaLen = 0x28;
      const int disEqcLen = 16;
      for (int i = 0; i < 16; ++i)
        Marshal.WriteByte(_ptrDiseqc, i, 0);
      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
      Marshal.WriteInt16(_thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(_thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(_thbdaBuf, 8, 0x90);
      Marshal.WriteByte(_thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(_thbdaBuf, 10, 0x85);
      Marshal.WriteByte(_thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(_thbdaBuf, 12, 0x62);
      Marshal.WriteByte(_thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(_thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(_thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(_thbdaBuf, 16, (int)THBDA_IOCTL_GET_DiSEqC);//dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, IntPtr.Zero.ToInt32());//lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, 0);//nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, _ptrDiseqc.ToInt32());//lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, disEqcLen);//nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned);//lpBytesReturned
      bool success = false;
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          if (hr != 0)
          {
            Log.Log.WriteFile("TwinHan get DiSEqC failed 0x{0:X}", hr);
          }
          else
          {
            Log.Log.WriteFile("TwinHan get DiSEqC ok 0x{0:X}", hr);
          }
          string line = "";
          for (int i = 0; i < 16; ++i)
          {
            byte k = Marshal.ReadByte(_ptrDiseqc, i);
            line += String.Format("{0:X} ", k);
          }
          Log.Log.Write("reply:{0}", line);
          success = true;
          int bytesReturned = Marshal.ReadInt32(_ptrDiseqc);
          if (bytesReturned > 0)
          {
            reply = new byte[bytesReturned];
            for (int i = 0; i < bytesReturned; ++i)
            {
              reply[i] = Marshal.ReadByte(_ptrDiseqc, 4 + i);
            }
          }
          Marshal.ReleaseComObject(propertySet);
        }
        Marshal.ReleaseComObject(pin);
      }
      return success;
    }
    #endregion
  }
}
