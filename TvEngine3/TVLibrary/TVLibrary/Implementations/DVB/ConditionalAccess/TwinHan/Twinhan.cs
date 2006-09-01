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


using DirectShowLib;
namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Summary description for Twinhan.
  /// </summary>
  public class Twinhan
  {
    #region twinhan sample app code:
    //#define CTL_CODE( DeviceType, Function, Method, Access ) (                 \
    //		((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method) \
    //		)
    //#define THBDA_IO_INDEX													0xAA00
    //#define THBDA_IOCTL_CI_SEND_PMT                 CTL_CODE(THBDA_IO_INDEX, 206, METHOD_BUFFERED, FILE_ANY_ACCESS)
    //#define THBDA_IOCTL_CHECK_INTERFACE             CTL_CODE(THBDA_IO_INDEX, 121, METHOD_BUFFERED, FILE_ANY_ACCESS)
    //#define THBDA_IOCTL_CI_PARSER_PMT               CTL_CODE(THBDA_IO_INDEX, 207, METHOD_BUFFERED, FILE_ANY_ACCESS)
    //typedef struct 
    //				{
    //					GUID    CmdGUID;            // Private Command GUID
    //					DWORD   dwIoControlCode;    // operation
    //					LPVOID  lpInBuffer;         // input data buffer
    //					DWORD   nInBufferSize;      // size of input data buffer
    //					LPVOID  lpOutBuffer;        // output data buffer
    //					DWORD   nOutBufferSize;     // size of output data buffer
    //					LPDWORD lpBytesReturned;    // byte count
    //				} THBDACMD, *PTHBDACMD;

    //		BOOL CBDAFilterGraph::BDAIOControl( DWORD  dwIoControlCode,
    //			LPVOID lpInBuffer,
    //			DWORD  nInBufferSize,
    //			LPVOID lpOutBuffer,
    //			DWORD  nOutBufferSize,
    //			LPDWORD lpBytesReturned)
    //		{
    //			if (!m_KsTunerPropSet)
    //				return FALSE;
    //
    //			KSPROPERTY instance_data;
    //
    //			ULONG    ulOutBuf = 0;
    //			ULONG    ulReturnBuf = 0;
    //			THBDACMD THBDACmd;
    //
    //			THBDACmd.CmdGUID = GUID_THBDA_CMD;
    //			THBDACmd.dwIoControlCode = dwIoControlCode;
    //			THBDACmd.lpInBuffer = lpInBuffer;
    //			THBDACmd.nInBufferSize = nInBufferSize;
    //			THBDACmd.lpOutBuffer = lpOutBuffer;
    //			THBDACmd.nOutBufferSize = nOutBufferSize;
    //			THBDACmd.lpBytesReturned = lpBytesReturned;
    //
    //			HRESULT hr = m_KsTunerPropSet->Set(GUID_THBDA_TUNER, 
    //				NULL, 
    //				&instance_data, sizeof(instance_data),
    //				&THBDACmd, sizeof(THBDACmd));
    //
    //			if (FAILED(hr))
    //				return FALSE;
    //			else
    //				return TRUE;
    //		}
    //
    //		BOOL CBDAFilterGraph::CheckBDAInterface()
    //		{
    //			BOOL bStatus = FALSE;
    //			DWORD dwBytesReturned = 0;
    //
    //			bStatus = BDAIOControl( THBDA_IOCTL_CHECK_INTERFACE,
    //				NULL,
    //				0,
    //				NULL,
    //				0,
    //				(LPDWORD)&dwBytesReturned);
    //
    //			return bStatus;
    //		}

    //		BOOL CBDAFilterGraph::SendCAPMT(PBYTE pBuff, BYTE byBuffSize)
    //		{
    //				BOOL bStatus = FALSE;
    //				DWORD dwBytesReturned = 0;
    //
    //				bStatus = BDAIOControl( THBDA_IOCTL_CI_SEND_PMT,
    //									(LPVOID)pBuff,
    //									byBuffSize,
    //									NULL,
    //									0,
    //									(LPDWORD)&dwBytesReturned);
    //		    
    //				return bStatus;
    //		}
    #endregion

    #region guids
    readonly Guid THBDA_TUNER = new Guid("E5644CC4-17A1-4eed-BD90-74FDA1D65423");
    readonly Guid GUID_THBDA_CMD = new Guid("255E0082-2017-4b03-90F8-856A62CB3D67");
    //readonly uint THBDA_IOCTL_CI_SEND_PMT = 0xaa000338;
    readonly uint THBDA_IOCTL_CHECK_INTERFACE = 0xaa0001e4;
    readonly uint THBDA_IOCTL_CI_PARSER_PMT = 0xaa00033c;
    readonly uint THBDA_IOCTL_CI_GET_STATE = 0xaa000320;//CTL_CODE(THBDA_IO_INDEX, 200, METHOD_BUFFERED, FILE_ANY_ACCESS)
    #endregion

    #region variables
    bool _initialized;
    bool _isTwinHanCard;
    bool _camPresent;
    IBaseFilter _captureFilter;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Twinhan"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="captureFilter">The capture filter.</param>
    public Twinhan(IBaseFilter tunerFilter, IBaseFilter captureFilter)
    {
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
          IntPtr thbdaBuf = Marshal.AllocCoTaskMem(1024);
          IntPtr ptrDwBytesReturned = Marshal.AllocCoTaskMem(4);
          IntPtr ptrOutBuffer = Marshal.AllocCoTaskMem(4096);
          try
          {
            int thbdaLen = 0x28;
            Marshal.WriteInt32(thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
            Marshal.WriteInt16(thbdaBuf, 4, 0x2017);
            Marshal.WriteInt16(thbdaBuf, 6, 0x4b03);
            Marshal.WriteByte(thbdaBuf, 8, 0x90);
            Marshal.WriteByte(thbdaBuf, 9, 0xf8);
            Marshal.WriteByte(thbdaBuf, 10, 0x85);
            Marshal.WriteByte(thbdaBuf, 11, 0x6a);
            Marshal.WriteByte(thbdaBuf, 12, 0x62);
            Marshal.WriteByte(thbdaBuf, 13, 0xcb);
            Marshal.WriteByte(thbdaBuf, 14, 0x3d);
            Marshal.WriteByte(thbdaBuf, 15, 0x67);
            Marshal.WriteInt32(thbdaBuf, 16, (int)THBDA_IOCTL_CI_GET_STATE);//control code
            Marshal.WriteInt32(thbdaBuf, 20, (int)IntPtr.Zero); //LPVOID inbuffer
            Marshal.WriteInt32(thbdaBuf, 24, 0);                //DWORD inbuffersize
            Marshal.WriteInt32(thbdaBuf, 28, ptrOutBuffer.ToInt32()); //LPVOID outbuffer
            Marshal.WriteInt32(thbdaBuf, 32, 4096);                //DWORD outbuffersize
            Marshal.WriteInt32(thbdaBuf, 36, (int)ptrDwBytesReturned);//LPVOID bytesreturned

            int hr = propertySet.Set(propertyGuid, 0, thbdaBuf, thbdaLen, thbdaBuf, thbdaLen);
            if (hr == 0)
            {
              int bytesReturned = Marshal.ReadInt32(ptrDwBytesReturned);
              CIState = (uint)Marshal.ReadInt32(ptrOutBuffer, 0);
              MMIState = (uint)Marshal.ReadInt32(ptrOutBuffer, 4);
              Log.Log.WriteFile("twinhan:CI State:{0:X} MMI State:{1:X}", CIState, MMIState);
            }
            else
            {
              Log.Log.WriteFile("twinhan: unable to get CI State hr:{0:X}", hr);
            }
          }
          finally
          {
            Marshal.FreeCoTaskMem(thbdaBuf);
            Marshal.FreeCoTaskMem(ptrDwBytesReturned);
            Marshal.FreeCoTaskMem(ptrOutBuffer);
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
      return IsCamPresent();
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
      IntPtr ptrDwBytesReturned = Marshal.AllocCoTaskMem(4);
      try
      {
        int thbdaLen = 0x28;
        IntPtr thbdaBuf = Marshal.AllocCoTaskMem(thbdaLen);
        try
        {
          Marshal.WriteInt32(thbdaBuf, 0, 0x255e0082);//GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
          Marshal.WriteInt16(thbdaBuf, 4, 0x2017);
          Marshal.WriteInt16(thbdaBuf, 6, 0x4b03);
          Marshal.WriteByte(thbdaBuf, 8, 0x90);
          Marshal.WriteByte(thbdaBuf, 9, 0xf8);
          Marshal.WriteByte(thbdaBuf, 10, 0x85);
          Marshal.WriteByte(thbdaBuf, 11, 0x6a);
          Marshal.WriteByte(thbdaBuf, 12, 0x62);
          Marshal.WriteByte(thbdaBuf, 13, 0xcb);
          Marshal.WriteByte(thbdaBuf, 14, 0x3d);
          Marshal.WriteByte(thbdaBuf, 15, 0x67);
          Marshal.WriteInt32(thbdaBuf, 16, (int)THBDA_IOCTL_CHECK_INTERFACE);//control code
          Marshal.WriteInt32(thbdaBuf, 20, (int)IntPtr.Zero);
          Marshal.WriteInt32(thbdaBuf, 24, 0);
          Marshal.WriteInt32(thbdaBuf, 28, (int)IntPtr.Zero);
          Marshal.WriteInt32(thbdaBuf, 32, 0);
          Marshal.WriteInt32(thbdaBuf, 36, (int)ptrDwBytesReturned);

          IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
          if (pin != null)
          {
            DirectShowLib.IKsPropertySet propertySet = pin as DirectShowLib.IKsPropertySet;
            if (propertySet != null)
            {
              Guid propertyGuid = THBDA_TUNER;

              int hr = propertySet.Set(propertyGuid, 0, thbdaBuf, thbdaLen, thbdaBuf, thbdaLen);
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
          Marshal.FreeCoTaskMem(thbdaBuf);
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(ptrDwBytesReturned);
      }
      return success;
    }


    /// <summary>
    /// Sends the PMT to the CAM/CI module
    /// </summary>
    /// <param name="camType">Type of the cam.</param>
    /// <param name="videoPid">The video pid.</param>
    /// <param name="audioPid">The audio pid.</param>
    /// <param name="PMT">The PMT.</param>
    /// <param name="pmtLen">The PMT lenght</param>
    public void SendPMT(string camType, uint videoPid, uint audioPid, byte[] PMT, int pmtLen)
    {
      if (IsCamPresent() == false) return;
      int camNumber = 1;
      camType = camType.ToLower();
      if (camType.ToLower() == "default") camNumber = 0;
      if (camType.ToLower() == "viaccess") camNumber = 1;
      else if (camType.ToLower() == "aston") camNumber = 2;
      else if (camType.ToLower() == "conax") camNumber = 3;
      else if (camType.ToLower() == "cryptoworks") camNumber = 4;

      IntPtr ptrPMT = Marshal.AllocCoTaskMem(pmtLen + 1);

      Log.Log.WriteFile("Twinhan: send PMT cam:{0} {1} len:{2} video:0x{3:X} audio:0x{4:X}", camType, camNumber, pmtLen, videoPid, audioPid);

      if (ptrPMT == IntPtr.Zero)
        return;
      Marshal.Copy(PMT, 0, (IntPtr)(((int)ptrPMT) + 1), pmtLen);
      Marshal.WriteByte(ptrPMT, 0);
      pmtLen += 1;
      int lenParserPMTInfo = 20;
      IntPtr ptrPARSERPMTINFO = Marshal.AllocCoTaskMem(lenParserPMTInfo);
      if (ptrPMT == IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(ptrPMT);
        return;
      }
      Marshal.WriteInt32(ptrPARSERPMTINFO, 0, (int)ptrPMT);
      Marshal.WriteInt32(ptrPARSERPMTINFO, 4, pmtLen);
      Marshal.WriteInt32(ptrPARSERPMTINFO, 8, (int)videoPid);
      Marshal.WriteInt32(ptrPARSERPMTINFO, 12, (int)audioPid);
      Marshal.WriteInt32(ptrPARSERPMTINFO, 16, camNumber);//default cam

      IntPtr ptrDwBytesReturned = Marshal.AllocCoTaskMem(4);
      if (ptrDwBytesReturned == IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(ptrPMT);
        Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
        return;
      }
      IntPtr ksBla = Marshal.AllocCoTaskMem(0x18);
      if (ksBla == IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(ptrPMT);
        Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
        Marshal.FreeCoTaskMem(ptrDwBytesReturned);
        return;
      }

      int thbdaLen = 0x28;
      IntPtr thbdaBuf = Marshal.AllocCoTaskMem(thbdaLen);
      if (thbdaBuf == IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(ptrPMT);
        Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
        Marshal.FreeCoTaskMem(ptrDwBytesReturned);
        Marshal.FreeCoTaskMem(ksBla);
        return;
      }


      Marshal.WriteInt32(thbdaBuf, 0, 0x255e0082);
      Marshal.WriteInt16(thbdaBuf, 4, 0x2017);
      Marshal.WriteInt16(thbdaBuf, 6, 0x4b03);
      Marshal.WriteByte(thbdaBuf, 8, 0x90);
      Marshal.WriteByte(thbdaBuf, 9, 0xf8);
      Marshal.WriteByte(thbdaBuf, 10, 0x85);
      Marshal.WriteByte(thbdaBuf, 11, 0x6a);
      Marshal.WriteByte(thbdaBuf, 12, 0x62);
      Marshal.WriteByte(thbdaBuf, 13, 0xcb);
      Marshal.WriteByte(thbdaBuf, 14, 0x3d);
      Marshal.WriteByte(thbdaBuf, 15, 0x67);
      Marshal.WriteInt32(thbdaBuf, 16, (int)THBDA_IOCTL_CI_PARSER_PMT);
      Marshal.WriteInt32(thbdaBuf, 20, (int)ptrPARSERPMTINFO);
      Marshal.WriteInt32(thbdaBuf, 24, 20);
      Marshal.WriteInt32(thbdaBuf, 28, 0);
      Marshal.WriteInt32(thbdaBuf, 32, 0);
      Marshal.WriteInt32(thbdaBuf, 36, (int)ptrDwBytesReturned);

      IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
      if (pin != null)
      {
        IKsPropertySet propertySet = pin as IKsPropertySet;
        if (propertySet != null)
        {
          Guid propertyGuid = THBDA_TUNER;
          int hr = propertySet.Set(propertyGuid, 0, ksBla, 0x18, thbdaBuf, thbdaLen);
          int back = Marshal.ReadInt32(ptrDwBytesReturned);
          int ksBlaVal = Marshal.ReadInt32(ksBla);

          if (hr != 0)
          {
            Log.Log.WriteFile("SetStructure() failed 0x{0:X}", hr);
          }
          else
            Log.Log.WriteFile("SetStructure() returned ok 0x{0:X}", hr);
          Marshal.ReleaseComObject(propertySet);

        }
        Marshal.ReleaseComObject(pin);
      }


      Marshal.FreeCoTaskMem(thbdaBuf);
      Marshal.FreeCoTaskMem(ptrDwBytesReturned);
      Marshal.FreeCoTaskMem(ptrPARSERPMTINFO);
      Marshal.FreeCoTaskMem(ptrPMT);
      Marshal.FreeCoTaskMem(ksBla);
    }
  }
}
