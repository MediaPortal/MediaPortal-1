/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using TvLibrary.Interfaces;
using TvLibrary.Channels;

using DirectShowLib;
namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Summary description for Twinhan.
  /// </summary>
  public class Twinhan
  {

    #region guids
    readonly Guid THBDA_TUNER = new Guid("E5644CC4-17A1-4eed-BD90-74FDA1D65423");
    readonly Guid GUID_THBDA_CMD = new Guid("255E0082-2017-4b03-90F8-856A62CB3D67");
    readonly uint THBDA_IOCTL_CI_SEND_PMT = 0xaa000338;     //CTL_CODE(THBDA_IO_INDEX, 206, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_CHECK_INTERFACE = 0xaa0001e4; //CTL_CODE(THBDA_IO_INDEX, 121, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_CI_GET_STATE = 0xaa000320;    //CTL_CODE(THBDA_IO_INDEX, 200, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_CI_GET_PMT_REPLY = 0xaa000348;//CTL_CODE(THBDA_IO_INDEX, 210, METHOD_BUFFERED, FILE_ANY_ACCESS)
    readonly uint THBDA_IOCTL_SET_DiSEqC = 0xaa0001a0;//CTL_CODE(THBDA_IO_INDEX, 104, METHOD_BUFFERED, FILE_ANY_ACCESS) 
    readonly uint THBDA_IOCTL_SET_LNB_DATA = 0xaa000200;//CTL_CODE(THBDA_IO_INDEX, 128, METHOD_BUFFERED, FILE_ANY_ACCESS) 
    #endregion

    #region variables
    bool _initialized;
    bool _isTwinHanCard;
    bool _camPresent;
    IBaseFilter _captureFilter;
    IntPtr _ptrPmt;
    IntPtr _ptrDiseqc;
    IntPtr _ptrDwBytesReturned;
    IntPtr _thbdaBuf;
    IntPtr _ptrOutBuffer;
    IntPtr _ptrOutBuffer2;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Twinhan"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="captureFilter">The capture filter.</param>
    public Twinhan(IBaseFilter tunerFilter, IBaseFilter analyzerFilter)
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
          Log.Log.WriteFile("Cam detected:{0}", _camPresent);
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
        if (_initialized) return _isTwinHanCard;

        bool result = IsTwinhanCard();
        if (result)
        {
          if (IsCamPresent())
          {
            Log.Log.WriteFile("twinhan: CAM inserted");
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
      /*
       typedef struct {
        ULONG ulCIState;
        ULONG ulMMIState;
      } THCIState, *PTHCIState;
      */
      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        DirectShowLib.IKsPropertySet propertySet = pin as DirectShowLib.IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          try
          {
            int thbdaLen = 0x28;
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
              int bytesReturned = Marshal.ReadInt32(_ptrDwBytesReturned);
              CIState = (uint)Marshal.ReadInt32(_ptrOutBuffer, 0);
              MMIState = (uint)Marshal.ReadInt32(_ptrOutBuffer, 4);
              Log.Log.WriteFile("twinhan:CI State:{0:X} MMI State:{1:X}", CIState, MMIState);
            }
            else
            {
              Log.Log.WriteFile("twinhan: unable to get CI State hr:{0:X}", hr);
            }
          }
          finally
          {
          }
        }
        //Marshal.ReleaseComObject(pin);
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
      if (_initialized) return _camPresent;
      uint CIState;
      uint MMIState;
      GetCAMStatus(out CIState, out MMIState);
      if (CIState != 0) return true;
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
      if (_initialized) return _isTwinHanCard;
      Log.Log.WriteFile("Twinhan: check for twinhan driver");

      bool success = false;

      try
      {
        int thbdaLen = 0x28;
        try
        {
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
            DirectShowLib.IKsPropertySet propertySet = pin as DirectShowLib.IKsPropertySet;
            if (propertySet != null)
            {
              Guid propertyGuid = THBDA_TUNER;

              int hr = propertySet.Set(propertyGuid, 0, _thbdaBuf, thbdaLen, _thbdaBuf, thbdaLen);
              if (hr == 0)
              {
                Log.Log.WriteFile("twinhan card detected");
                success = true;
              }
              //Marshal.ReleaseComObject(propertySet);
            }
            //Marshal.ReleaseComObject(pin);
          }
        }
        finally
        {
        }
      }
      finally
      {

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
      int thbdaLen = 0x28;
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
            Log.Log.WriteFile("GetPmtReply() failed 0x{0:X}", hr);
          }

          Log.Log.WriteFile("GetPmtReply() returned {0} bytes", back);
          Marshal.ReleaseComObject(propertySet);
          try
          {
            System.IO.File.Delete("c:\\pmtreply.dat");
          }
          catch (Exception)
          {
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
    /// <param name="camType">Type of the cam.</param>
    /// <param name="videoPid">The video pid.</param>
    /// <param name="audioPid">The audio pid.</param>
    /// <param name="caPMT">The caPMT structure.</param>
    /// <param name="caPMTLen">The caPMT lenght</param>
    public void SendPMT(CamType camType, uint videoPid, uint audioPid, byte[] caPMT, int caPMTLen)
    {
      if (IsCamPresent() == false) return;
      int camNumber = (int)camType;


      Log.Log.WriteFile("Twinhan: send PMT cam:{0} len:{1} video:0x{2:X} audio:0x{3:X}", camType, caPMTLen, videoPid, audioPid);

      if (caPMT.Length == 0)
        return;
      string line = "";
      for (int i = 0; i < caPMTLen; ++i)
      {
        string tmp = String.Format("{0:X} ", caPMT[i]);
        line += tmp;
      }
      Log.Log.WriteFile("capmt:{0}", line);
      Marshal.Copy(caPMT, 0, _ptrPmt, caPMTLen);
      int thbdaLen = 0x28;
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
      Marshal.WriteInt32(_thbdaBuf, 20, (int)_ptrPmt.ToInt32());//lpInBuffer
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
          int hr = propertySet.Set(propertyGuid, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
          int back = Marshal.ReadInt32(_ptrDwBytesReturned);

          if (hr != 0)
          {
            Log.Log.WriteFile("Twinhan: cam failed 0x{0:X}", hr);
          }
          else
            Log.Log.WriteFile("Twinhan: cam returned ok 0x{0:X}", hr);
          Marshal.ReleaseComObject(propertySet);

        }
        Marshal.ReleaseComObject(pin);
      }

      //System.Threading.Thread.Sleep(1000);
      //GetPmtReply();

    }
    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public void SendDiseqCommand(DVBSChannel channel)
    {
      byte disEqcPort = 0;

      switch (channel.DisEqc)
      {
        case DisEqcType.None:
          disEqcPort = 0;
          break;
        case DisEqcType.SimpleA://simple A
          disEqcPort = 0;
          break;
        case DisEqcType.SimpleB://simple B
          disEqcPort = 1;
          break;
        case DisEqcType.Level1AA://Level 1 A/A
          disEqcPort = 1;
          break;
        case DisEqcType.Level1BA://Level 1 B/A
          disEqcPort = 2;
          break;
        case DisEqcType.Level1AB://Level 1 A/B
          disEqcPort = 3;
          break;
        case DisEqcType.Level1BB://Level 1 B/B
          disEqcPort = 4;
          break;
      }
      byte turnon22Khz = 0;
      Int32 LNBLOFLowBand = 9750;
      Int32 LNBLOFHighBand = 11700;
      Int32 LNBLOFHiLoSW = 10600;

      switch (channel.BandType)
      {
        case BandType.Universal:
          if (channel.Frequency >= 11700000)
          {
            turnon22Khz = 1;
            //hiBand = true;
          }
          else
          {
            turnon22Khz = 0;
            //hiBand = false;
          }
          break;
        case BandType.Circular:
          LNBLOFLowBand = 11250;
          LNBLOFHighBand = 11250;
          LNBLOFHiLoSW = 0;
          break;
        case BandType.Linear:
          LNBLOFLowBand = 10750;
          LNBLOFHighBand = 10750;
          LNBLOFHiLoSW = 0;
          break;
        case BandType.CBand:
          LNBLOFLowBand = 5150;
          LNBLOFHighBand = 5150;
          LNBLOFHiLoSW = 0;
          break;
      }

      int thbdaLen = 0x28;
      int disEqcLen = 20;
      Marshal.WriteByte(_ptrDiseqc, 0, 1);// LNB_POWER
      Marshal.WriteByte(_ptrDiseqc, 1, 0);// Tone_Data_Burst
      Marshal.WriteInt32(_thbdaBuf, 4, LNBLOFLowBand);// ulLNBLOFLowBand   LNBLOF LowBand MHz
      Marshal.WriteInt32(_thbdaBuf, 8, LNBLOFHighBand);// ulLNBLOFHighBand  LNBLOF HighBand MHz
      Marshal.WriteInt32(_thbdaBuf, 12, LNBLOFHiLoSW);// ulLNBLOFHiLoSW   LNBLOF HiLoSW MHz
      Marshal.WriteByte(_thbdaBuf, 16, turnon22Khz);// f22K_Output
      Marshal.WriteByte(_thbdaBuf, 17, disEqcPort);// DiSEqC_Port

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
      Marshal.WriteInt32(_thbdaBuf, 20, (int)_ptrDiseqc.ToInt32());//lpInBuffer
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
          int back = Marshal.ReadInt32(_ptrDwBytesReturned);

          if (hr != 0)
          {
            Log.Log.WriteFile("TwinHan diseqc failed 0x{0:X}", hr);
          }
          else
            Log.Log.WriteFile("TwinHan diseqc ok 0x{0:X}", hr);
          Marshal.ReleaseComObject(propertySet);

        }
        Marshal.ReleaseComObject(pin);
      }
    }
  }
}
