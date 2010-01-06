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
using System.Text;
using TvLibrary.Interfaces;
using System.Threading;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Summary description for Twinhan.
  /// </summary>
  public class Twinhan : IDiSEqCController, ICiMenuActions, IDisposable
  {
    #region guids and constants

    private readonly Guid THBDA_TUNER = new Guid("E5644CC4-17A1-4eed-BD90-74FDA1D65423");
    private readonly Guid GUID_THBDA_CMD = new Guid("255E0082-2017-4b03-90F8-856A62CB3D67");

    /// <summary>
    /// CI State enum
    /// </summary>
    public enum CIState
    {
      /// NON_CI_INFO      0
      CI_STATUS_EMPTY_OLD = 0,
      /// ME0				1
      CI_STATUS_CAM_OK1_OLD,
      /// ME1				2
      CI_STATUS_CAM_OK2_OLD,

      /// MMI0				3
      MMI_STATUS_GET_MENU_OK1_OLD,
      /// MMI1				4
      MMI_STATUS_GET_MENU_OK2_OLD,
      /// MMI0_ClOSE		5
      MMI_STATUS_GET_MENU_CLOSE1_OLD,
      /// MMI1_ClOSE		6
      MMI_STATUS_GET_MENU_CLOSE2_OLD,

      /// New CI messages
      /// No CAM inserted
      CI_STATUS_EMPTY = 10,
      /// CAM is inserted
      CI_STATUS_INSERTED,
      /// Initila CAM OK
      CI_STATUS_CAM_OK,
      /// Unkonw CAM type
      CI_STATUS_CAM_UNKNOW,

      /// Communicating with CAM 
      MMI_STATUS_ANSWER_SEND,
      /// Get information from CAM
      MMI_STATUS_GET_MENU_OK,
      /// Fail to get information from CAM
      MMI_STATUS_GET_MENU_FAIL,
      /// Init MMI
      MMI_STATUS_GET_MENU_INIT,
      /// Close MMI
      MMI_STATUS_GET_MENU_CLOSE,
      /// MMI Closed
      MMI_STATUS_GET_MENU_CLOSED,
    }

    /// <summary>
    /// Length of BDA command
    /// </summary>
    private const int thbdaLen = 0x28;

    private static uint THBDA_IO_INDEX = 0xAA00;
    private static uint METHOD_BUFFERED = 0x0000;
    private static uint FILE_ANY_ACCESS = 0x0000;

    /// <summary>
    /// creates control command
    /// </summary>
    private static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
    {
      return ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method);
    }

    private readonly uint THBDA_IOCTL_CI_SEND_PMT = CTL_CODE(THBDA_IO_INDEX, 206, METHOD_BUFFERED, FILE_ANY_ACCESS);
    private readonly uint THBDA_IOCTL_CHECK_INTERFACE = CTL_CODE(THBDA_IO_INDEX, 121, METHOD_BUFFERED, FILE_ANY_ACCESS);
    private readonly uint THBDA_IOCTL_CI_GET_STATE = CTL_CODE(THBDA_IO_INDEX, 200, METHOD_BUFFERED, FILE_ANY_ACCESS);
    private readonly uint THBDA_IOCTL_CI_GET_PMT_REPLY = CTL_CODE(THBDA_IO_INDEX, 210, METHOD_BUFFERED, FILE_ANY_ACCESS);
    private readonly uint THBDA_IOCTL_SET_DiSEqC = CTL_CODE(THBDA_IO_INDEX, 104, METHOD_BUFFERED, FILE_ANY_ACCESS);
    private readonly uint THBDA_IOCTL_GET_DiSEqC = CTL_CODE(THBDA_IO_INDEX, 105, METHOD_BUFFERED, FILE_ANY_ACCESS);
    private readonly uint THBDA_IOCTL_SET_LNB_DATA = CTL_CODE(THBDA_IO_INDEX, 128, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Init MMI
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_INIT_MMI = CTL_CODE(THBDA_IO_INDEX, 202, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Get MMI
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct THMMIInfo
    //OutBufferSize : sizeof(THMMIInfo) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_GET_MMI = CTL_CODE(THBDA_IO_INDEX, 203, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Answer
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : struct THMMIInfo
    //OutBufferSize : sizeof(THMMIInfo) bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_ANSWER = CTL_CODE(THBDA_IO_INDEX, 204, METHOD_BUFFERED, FILE_ANY_ACCESS);

    //*******************************************************************************************************
    //Functionality : Close MMI
    //InBuffer      : NULL
    //InBufferSize  : 0 bytes
    //OutBuffer     : NULL
    //OutBufferSize : 0 bytes
    //*******************************************************************************************************
    private readonly uint THBDA_IOCTL_CI_CLOSE_MMI = CTL_CODE(THBDA_IO_INDEX, 205, METHOD_BUFFERED, FILE_ANY_ACCESS);

    #endregion

    #region structs

    private struct MMIItem
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)] public String MenuItem;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private class MMIInfoStruct
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public String Header;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public String SubHeader;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public String BottomLine;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)] public MMIItem[] MenuItems;
      //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      //public String MenuItem1;
      //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      //public String MenuItem2;
      //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      //public String MenuItem3;
      //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      //public String MenuItem4;
      //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      //public String MenuItem5;
      //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      //public String MenuItem6;
      //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      //public String MenuItem7;
      //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      //public String MenuItem8;
      //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
      //public String MenuItem9;

      public Int32 ItemCount;

      public Int32 EnqFlag;

      public Int32 Blind_Answer;
      public Int32 Answer_Text_Length;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public String Prompt;

      public Int32 Answer;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public String AnswerStr;

      public Int32 Type;
    } ;

    #endregion

    #region helper functions

    /// <summary>
    /// Initialized structure for passing to Filter
    /// </summary>
    /// <param name="ControlCode"></param>
    /// <param name="InBuffer"></param>
    /// <param name="InBufferSize"></param>
    /// <param name="OutBuffer"></param>
    /// <param name="OutBufferSize"></param>
    private void InitStructure(Int32 ControlCode, IntPtr InBuffer, Int32 InBufferSize, IntPtr OutBuffer,
                               Int32 OutBufferSize)
    {
      Marshal.WriteInt32(_thbdaBuf, 0, 0x255e0082);
      //GUID_THBDA_CMD  = new Guid( "255E0082-2017-4b03-90F8-856A62CB3D67" );
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
      Marshal.WriteInt32(_thbdaBuf, 16, ControlCode); //dwIoControlCode
      Marshal.WriteInt32(_thbdaBuf, 20, InBuffer.ToInt32()); //lpInBuffer
      Marshal.WriteInt32(_thbdaBuf, 24, InBufferSize); //nInBufferSize
      Marshal.WriteInt32(_thbdaBuf, 28, OutBuffer.ToInt32()); //lpOutBuffer
      Marshal.WriteInt32(_thbdaBuf, 32, OutBufferSize); //nOutBufferSize
      Marshal.WriteInt32(_thbdaBuf, 36, (int)_ptrDwBytesReturned); //lpBytesReturned
    }

    #endregion

    #region variables

    private readonly bool _initialized;
    private readonly bool _isTwinHanCard;
    private readonly bool _camPresent;
    private readonly IBaseFilter _captureFilter;
    // TODO: reduce number of buffers
    private readonly IntPtr _ptrPmt;
    private readonly IntPtr _ptrDiseqc;
    private readonly IntPtr _ptrDwBytesReturned;
    private readonly IntPtr _thbdaBuf;
    private readonly IntPtr _ptrOutBuffer;
    private readonly IntPtr _ptrOutBuffer2;
    private readonly IntPtr _ptrMMIBuffer;

    private IKsPropertySet propertySet;

    private bool StopThread;
    private ICiMenuCallbacks m_ciMenuCallback;
    private Thread CiMenuThread;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Twinhan"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Twinhan(IBaseFilter tunerFilter)
    {
      _ptrPmt = Marshal.AllocCoTaskMem(8192);
      _ptrDwBytesReturned = Marshal.AllocCoTaskMem(4); // int32
      _thbdaBuf = Marshal.AllocCoTaskMem(8192);
      _ptrOutBuffer = Marshal.AllocCoTaskMem(8192);
      _ptrOutBuffer2 = Marshal.AllocCoTaskMem(8192);
      _ptrDiseqc = Marshal.AllocCoTaskMem(8192);
      _ptrMMIBuffer = Marshal.AllocCoTaskMem(8192);
      _captureFilter = tunerFilter;
      _initialized = false;
      _camPresent = false;
      _isTwinHanCard = false;
      if (_captureFilter != null)
      {
        IPin pin = DsFindPin.ByDirection(_captureFilter, PinDirection.Input, 0);
        propertySet = pin as IKsPropertySet;

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
      GetCAMStatus(out CIState, out MMIState, false);
    }

    /// <summary>
    /// Gets the status of the CAM and CI.
    /// </summary>
    /// <param name="CIState">State of the CI.</param>
    /// <param name="MMIState">State of the MMI.</param>
    /// <param name="SilentMode">No outputs (polling mode)</param>
    private void GetCAMStatus(out uint CIState, out uint MMIState, bool SilentMode)
    {
      CIState = 0;
      MMIState = 0;
      if (propertySet != null)
      {
        try
        {
          InitStructure((int)THBDA_IOCTL_CI_GET_STATE, IntPtr.Zero, 0, _ptrOutBuffer, 4096);
          int hr = propertySet.Set(THBDA_TUNER, 0, _thbdaBuf, thbdaLen, _thbdaBuf, thbdaLen);
          if (hr == 0)
          {
            CIState = (uint)Marshal.ReadInt32(_ptrOutBuffer, 0);
            MMIState = (uint)Marshal.ReadInt32(_ptrOutBuffer, 4);
            if (!SilentMode) Log.Log.WriteFile("Twinhan:  CI State:{0:X} MMI State:{1:X}", CIState, MMIState);
          }
          else
          {
            Log.Log.WriteFile("Twinhan:  unable to get CI State hr:{0:X}", hr);
          }
        }
        finally
        {
          if (!SilentMode) Log.Log.WriteFile("Twinhan: CI status read");
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
        InitStructure((int)THBDA_IOCTL_CHECK_INTERFACE, IntPtr.Zero, 0, IntPtr.Zero, 0);
        if (propertySet != null)
        {
          int hr = propertySet.Set(THBDA_TUNER, 0, _thbdaBuf, thbdaLen, _thbdaBuf, thbdaLen);
          if (hr == 0)
          {
            success = true;
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
      InitStructure((int)THBDA_IOCTL_CI_GET_PMT_REPLY, IntPtr.Zero, 0, _ptrPmt, 1024);
      if (propertySet != null)
      {
        int hr = propertySet.Set(THBDA_TUNER, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
        int back = Marshal.ReadInt32(_ptrDwBytesReturned);
        if (hr != 0)
        {
          Log.Log.WriteFile("Twinhan:  GetPmtReply() failed 0x{0:X}", hr);
        }
        Log.Log.WriteFile("Twinhan:  GetPmtReply() returned {0} bytes", back);
        DVB_MMI.DumpBinary(_ptrPmt, 0, back);
        //Marshal.ReleaseComObject(propertySet);
        /*
        try
        {
          System.IO.File.Delete("c:\\pmtreply.dat");
        }
        catch (Exception ex)
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
        */
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

      Log.Log.WriteFile(" capmt:");
      DVB_MMI.DumpBinary(caPMT, 0, caPMTLen);


      bool suceeded = false;
      Marshal.Copy(caPMT, 0, _ptrPmt, caPMTLen);

      InitStructure((int)THBDA_IOCTL_CI_SEND_PMT, _ptrPmt, caPMTLen, IntPtr.Zero, 0);
      if (propertySet != null)
      {
        int failedAttempts = 0;
        while (true)
        {
          int hr = propertySet.Set(THBDA_TUNER, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
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
        //Marshal.ReleaseComObject(propertySet);
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
      BandTypeConverter.GetDefaultLnbSetup(parameters, channel.BandType, out LNBLOFLowBand, out LNBLOFHighBand,
                                           out LNBLOFHiLoSW);
      byte turnon22Khz = BandTypeConverter.IsHiBand(channel, parameters) ? (byte)2 : (byte)1;
      SetLnbData(true, LNBLOFLowBand, LNBLOFHighBand, LNBLOFHiLoSW, turnon22Khz, disEqcPort);
      SendDiseqcCommandTest(parameters, channel);
    }

    private void SetLnbData(bool lnbPower, int LNBLOFLowBand, int LNBLOFHighBand, int LNBLOFHiLoSW, int turnon22Khz,
                            int disEqcPort)
    {
      Log.Log.WriteFile("Twinhan:  SetLnb diseqc port:{0} 22khz:{1} low:{2} hi:{3} switch:{4} power:{5}", disEqcPort,
                        turnon22Khz, LNBLOFLowBand, LNBLOFHighBand, LNBLOFHiLoSW, lnbPower);

      const int disEqcLen = 20;
      Marshal.WriteByte(_ptrDiseqc, 0, (byte)(lnbPower ? 1 : 0)); // 0: LNB_POWER
      Marshal.WriteByte(_ptrDiseqc, 1, 0); // 1: Tone_Data_Burst (Tone_Data_OFF:0 | Tone_Burst_ON:1 | Data_Burst_ON:2)
      Marshal.WriteByte(_ptrDiseqc, 2, 0);
      Marshal.WriteByte(_ptrDiseqc, 3, 0);
      Marshal.WriteInt32(_ptrDiseqc, 4, LNBLOFLowBand); // 4: ulLNBLOFLowBand   LNBLOF LowBand MHz
      Marshal.WriteInt32(_ptrDiseqc, 8, LNBLOFHighBand); // 8: ulLNBLOFHighBand  LNBLOF HighBand MHz
      Marshal.WriteInt32(_ptrDiseqc, 12, LNBLOFHiLoSW); //12: ulLNBLOFHiLoSW   LNBLOF HiLoSW MHz
      Marshal.WriteByte(_ptrDiseqc, 16, (byte)turnon22Khz);
      //16: f22K_Output (F22K_Output_HiLo:0 | F22K_Output_Off:1 | F22K_Output_On:2
      Marshal.WriteByte(_ptrDiseqc, 17, (byte)disEqcPort); //17: DiSEqC_Port
      Marshal.WriteByte(_ptrDiseqc, 18, 0);
      Marshal.WriteByte(_ptrDiseqc, 19, 0);

      InitStructure((int)THBDA_IOCTL_SET_LNB_DATA, _ptrDiseqc, disEqcLen, IntPtr.Zero, 0);
      if (propertySet != null)
      {
        int hr = propertySet.Set(THBDA_TUNER, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
        if (hr != 0)
        {
          Log.Log.WriteFile("Twinhan:  SetLNB failed 0x{0:X}", hr);
        }
        else
          Log.Log.WriteFile("Twinhan:  SetLNB ok 0x{0:X}", hr);
        //Marshal.ReleaseComObject(propertySet);
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
      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                           (channel.Polarisation == Polarisation.CircularL));
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
      const int disEqcLen = 16;
      for (int i = 0; i < 12; ++i)
      {
        Marshal.WriteByte(_ptrDiseqc, 4 + i, 0);
      }
      Marshal.WriteInt32(_ptrDiseqc, 0, diSEqC.Length); //command len
      for (int i = 0; i < diSEqC.Length; ++i)
      {
        Marshal.WriteByte(_ptrDiseqc, 4 + i, diSEqC[i]);
      }

      DVB_MMI.DumpBinary(_ptrDiseqc, 0, disEqcLen);
      InitStructure((int)THBDA_IOCTL_SET_DiSEqC, _ptrDiseqc, disEqcLen, IntPtr.Zero, 0);
      bool success = false;
      if (propertySet != null)
      {
        int hr = propertySet.Set(THBDA_TUNER, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
        if (hr != 0)
        {
          Log.Log.WriteFile("TwinHan DiSEqC failed 0x{1:X8}", hr);
        }
        else
        {
          Log.Log.WriteFile("TwinHan DiSEqC succeeded");
          success = true;
        }
        //Marshal.ReleaseComObject(propertySet);
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
      const int disEqcLen = 16;
      for (int i = 0; i < 16; ++i)
      {
        Marshal.WriteByte(_ptrDiseqc, i, 0);
      }

      bool success = false;

      InitStructure((int)THBDA_IOCTL_GET_DiSEqC, IntPtr.Zero, 0, _ptrDiseqc, disEqcLen);
      if (propertySet != null)
      {
        int hr = propertySet.Set(THBDA_TUNER, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
        if (hr != 0)
        {
          Log.Log.WriteFile("TwinHan get DiSEqC failed 0x{0:X}", hr);
        }
        else
        {
          Log.Log.WriteFile("TwinHan get DiSEqC ok 0x{0:X}", hr);
        }
        Log.Log.Write("ReadDiSEqCCommand");
        DVB_MMI.DumpBinary(_ptrDiseqc, 0, 16);

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
        //Marshal.ReleaseComObject(propertySet);
      }
      return success;
    }

    #endregion

    #region ICiMenuActions Member

    /// <summary>
    /// Starts CiHandler thread
    /// </summary>
    private void StartCiHandlerThread()
    {
      if (CiMenuThread == null)
      {
        Log.Log.Debug("TwinHan: Starting new CI handler thread");
        StopThread = false;
        CiMenuThread = new Thread(new ThreadStart(CiMenuHandler));
        CiMenuThread.Name = "TwinHan CiMenuHandler";
        CiMenuThread.IsBackground = true;
        CiMenuThread.Priority = ThreadPriority.Lowest;
        CiMenuThread.Start();
      }
    }

    /// <summary>
    /// Sets the callback handler
    /// </summary>
    /// <param name="ciMenuHandler"></param>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        Log.Log.Debug("Twinhan: registering ci callbacks");
        m_ciMenuCallback = ciMenuHandler;
        StartCiHandlerThread();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Enters the CI menu 
    /// </summary>
    /// <returns></returns>
    public bool EnterCIMenu()
    {
      InitStructure((int)THBDA_IOCTL_CI_INIT_MMI, IntPtr.Zero, 0, IntPtr.Zero, 0);
      if (propertySet != null)
      {
        int hr = propertySet.Set(THBDA_TUNER, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
        if (hr == 0)
        {
          Log.Log.Debug("TwinHan: enter CI menu successful");
          return true;
        }

        Log.Log.Debug("TwinHan: enter CI menu failed 0x{0:X}", hr);
      }
      return false;
    }

    /// <summary>
    /// Closes the CI menu 
    /// </summary>
    /// <returns></returns>
    public bool CloseCIMenu()
    {
      InitStructure((int)THBDA_IOCTL_CI_CLOSE_MMI, IntPtr.Zero, 0, IntPtr.Zero, 0);
      if (propertySet != null)
      {
        int hr = propertySet.Set(THBDA_TUNER, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
        if (hr == 0)
        {
          Log.Log.Debug("TwinHan: close CI menu successful");
          return true;
        }

        Log.Log.Debug("TwinHan: close CI menu failed 0x{0:X}", hr);
      }
      return false;
    }

    /// <summary>
    /// Selects a CI menu entry
    /// </summary>
    /// <param name="choice"></param>
    /// <returns></returns>
    public bool SelectMenu(byte choice)
    {
      MMIInfoStruct MMI = new MMIInfoStruct();
      MMI.Answer = choice;
      return SendMMI(MMI);
    }

    /// <summary>
    /// Sends an answer after CI request
    /// </summary>
    /// <param name="Cancel"></param>
    /// <param name="Answer"></param>
    /// <returns></returns>
    public bool SendMenuAnswer(bool Cancel, string Answer)
    {
      MMIInfoStruct MMI = new MMIInfoStruct();
      if (Cancel == true)
      {
        MMI.Answer = 0; // 0 means back
      }
      else
      {
        MMI.AnswerStr = Answer;
      }
      return SendMMI(MMI);
    }

    /// <summary>
    /// Sends a MMI object with answer back
    /// </summary>
    /// <param name="MMI"></param>
    /// <returns></returns>
    private bool SendMMI(MMIInfoStruct MMI)
    {
      Marshal.StructureToPtr(MMI, _ptrMMIBuffer, true);
      int sizeMMI = Marshal.SizeOf(MMI);
      Log.Log.Debug("SendMMI: size {0}", sizeMMI);
      DVB_MMI.DumpBinary(_ptrMMIBuffer, 0, sizeMMI);
      InitStructure((int)THBDA_IOCTL_CI_ANSWER, IntPtr.Zero, 0, _ptrMMIBuffer, sizeMMI);
      if (propertySet != null)
      {
        int hr = propertySet.Set(THBDA_TUNER, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
        if (hr == 0)
        {
          Log.Log.Debug("TwinHan: SendMMI successful");
          return true;
        }

        Log.Log.Debug("TwinHan: SendMMI failed 0x{0:X}", hr);
      }
      return false;
    }

    /// <summary>
    /// Reads a MMI object
    /// </summary>
    /// <returns></returns>
    private MMIInfoStruct ReadMMI()
    {
      Int32 bytesReturned;
      MMIInfoStruct MMI;
      // clear buffer first
      for (int i = 0; i < 8192; i += 4)
      {
        Marshal.WriteInt32(_ptrMMIBuffer, i, 0);
      }
      InitStructure((int)THBDA_IOCTL_CI_GET_MMI, IntPtr.Zero, 0, _ptrMMIBuffer, 8192);
      if (propertySet != null)
      {
        int hr = propertySet.Set(THBDA_TUNER, 0, _ptrOutBuffer2, 0x18, _thbdaBuf, thbdaLen);
        if (hr == 0)
        {
          bytesReturned = Marshal.ReadInt32(_ptrDwBytesReturned);
          Log.Log.Debug("TwinHan: ReadMMI successful, bytes {0}", bytesReturned);
          DVB_MMI.DumpBinary(_ptrMMIBuffer, 0, bytesReturned);
          try
          {
            MMI = (MMIInfoStruct)Marshal.PtrToStructure(_ptrMMIBuffer, typeof (MMIInfoStruct));
          }
          catch (Exception e)
          {
            Log.Log.Write(e);
            return null;
          }

          return MMI;
        }

        Log.Log.Debug("TwinHan: ReadMMI failed 0x{0:X}", hr);
      }
      return null;
    }

    #endregion

    #region CiMenuHandlerThread for polling status and handling MMI

    /// <summary>
    /// Thread that checks for CI menu 
    /// </summary>
    private void CiMenuHandler()
    {
      Log.Log.Debug("TwinHan: CI handler thread start polling status");
      try
      {
        while (!StopThread)
        {
          uint CIState;
          uint MMIState;
          GetCAMStatus(out CIState, out MMIState, true);
          switch (MMIState)
          {
            case 3: // TODO: find proper MMIState codings
              MMIInfoStruct MMI = ReadMMI();
              if (MMI != null)
              {
                Log.Log.Debug("TwinHan MMI:");
                Log.Log.Debug("Type        :{0}", MMI.Type);
                Log.Log.Debug("Header:      {0}", MMI.Header);
                Log.Log.Debug("SubHeader:   {0}", MMI.SubHeader);
                Log.Log.Debug("ButtomLine:  {0}", MMI.BottomLine);
                Log.Log.Debug("ItemCount:   {0}", MMI.ItemCount);
                Log.Log.Debug("EnqFlag:     {0}", MMI.EnqFlag);
                Log.Log.Debug("Prompt:      {0}", MMI.Prompt);
                Log.Log.Debug("AnswerLength:{0}", MMI.Answer_Text_Length);
                Log.Log.Debug("Blind_Answer:{0}", MMI.Blind_Answer);
                if (MMI.EnqFlag != 0)
                {
                  if (m_ciMenuCallback != null)
                  {
                    m_ciMenuCallback.OnCiRequest((MMI.Blind_Answer == 1), (uint)MMI.Answer_Text_Length, MMI.Prompt);
                  }
                }
                // which types do we get???
                if (MMI.Type == 1)
                {
                  if (m_ciMenuCallback != null)
                  {
                    m_ciMenuCallback.OnCiMenu(MMI.Header, MMI.SubHeader, MMI.BottomLine, MMI.ItemCount);
                    for (int m = 0; m < MMI.ItemCount; m++)
                    {
                      // choice number start with 0
                      m_ciMenuCallback.OnCiMenuChoice(m, MMI.MenuItems[m].MenuItem);
                    }
                    //if (MMI.ItemCount > 0) m_ciMenuCallback.OnCiMenuChoice(0, MMI.MenuItem1);
                    //if (MMI.ItemCount > 1) m_ciMenuCallback.OnCiMenuChoice(1, MMI.MenuItem2);
                    //if (MMI.ItemCount > 2) m_ciMenuCallback.OnCiMenuChoice(2, MMI.MenuItem3);
                    //if (MMI.ItemCount > 3) m_ciMenuCallback.OnCiMenuChoice(3, MMI.MenuItem4);
                    //if (MMI.ItemCount > 4) m_ciMenuCallback.OnCiMenuChoice(4, MMI.MenuItem5);
                    //if (MMI.ItemCount > 5) m_ciMenuCallback.OnCiMenuChoice(5, MMI.MenuItem6);
                    //if (MMI.ItemCount > 6) m_ciMenuCallback.OnCiMenuChoice(6, MMI.MenuItem7);
                    //if (MMI.ItemCount > 7) m_ciMenuCallback.OnCiMenuChoice(7, MMI.MenuItem8);
                    //if (MMI.ItemCount > 8) m_ciMenuCallback.OnCiMenuChoice(8, MMI.MenuItem9);
                  }
                }
              }
              break;
            default:
              Log.Log.Write("MMI State {0}", (CIState)MMIState);
              break;
          }
          Thread.Sleep(500);
        }
      }
      catch (ThreadAbortException) {}
      catch (Exception ex)
      {
        Log.Log.Debug("TwinHan: error in CiMenuHandler thread\r\n{0}", ex.ToString());
        return;
      }
    }

    #endregion

    #region IDisposable Member

    /// <summary>
    /// Disposes unmanaged resources
    /// </summary>
    public void Dispose()
    {
      if (CiMenuThread != null)
      {
        try
        {
          CiMenuThread.Abort();
        }
        catch {}
      }
      Marshal.FreeCoTaskMem(_ptrPmt);
      Marshal.FreeCoTaskMem(_ptrDwBytesReturned); // int32
      Marshal.FreeCoTaskMem(_thbdaBuf);
      Marshal.FreeCoTaskMem(_ptrOutBuffer);
      Marshal.FreeCoTaskMem(_ptrOutBuffer2);
      Marshal.FreeCoTaskMem(_ptrDiseqc);
      Marshal.FreeCoTaskMem(_ptrMMIBuffer);
    }

    #endregion
  }
}