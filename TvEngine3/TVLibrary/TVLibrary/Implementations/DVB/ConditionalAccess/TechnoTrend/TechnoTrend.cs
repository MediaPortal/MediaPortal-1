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
using System.Collections;
using System.Runtime.InteropServices;

using DirectShowLib;
using DirectShowLib.BDA;
using System.Windows.Forms;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
  public class TechnoTrend : IDisposable
  {

    #region enums
    enum TechnoTrendDeviceType
    {
      /// not set
      Unknown = 0,
      /// Budget 2
      Budget2,
      /// Budget 3 aka TT-budget T-3000
      Budget3,
      /// USB 2.0
      Usb2,
      /// USB 2.0 Pinnacle
      Usb2Pinnacle
    } ;
    public enum CiSlotStatusType : byte
    {
      Empty = 0,
      Inserted = 1,
      ModuleOk = 2,
      CaOk = 3,
      DebugMessage = 4,
      UnknownState = 0xff
    }

    #endregion
    #region structs

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    protected struct KSMULTIPLE_ITEM
    {
      public int Size;
      public int Count;
    };

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    protected struct REGPINMEDIUM
    {
      Guid clsMedium;
      public uint dw1;
      public uint dw2;
    };
    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct SlotInfo
    {
      /// CI status
      public Byte CiStatus;
      /// menu title string
      public IntPtr pMenuTitleString;
      /// cam system ID's
      unsafe public UInt16* pCaSystemIDs;
      /// number of cam system ID's
      public UInt16 wNoOfCaSystemIDs;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CallbackFunctionsSlim
    {
      /// PCBFCN_CI_OnSlotStatus
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public PCBFCN_CI_OnSlotStatus onSlotStatus;
      /// Context pointer for PCBFCN_CI_OnSlotStatus
      public UInt32 onSlotStatusContext;
      /// PCBFCN_CI_OnCAStatus
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public PCBFCN_CI_OnCAStatus onCAStatus;
      /// Context pointer for PCBFCN_CI_OnCAStatus
      public UInt32 onCAStatusContext;
    }
    public unsafe delegate void PCBFCN_CI_OnSlotStatus(UInt32 Context,
                                          Byte nSlot,
                                          Byte nStatus,
                                          SlotInfo* csInfo);

    public unsafe delegate void PCBFCN_CI_OnCAStatus(UInt32 Context,
                                                  Byte nSlot,
                                                  Byte nReplyTag,
                                                  UInt16 wStatus);
    #endregion

    #region imports
    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiOpenHWIdx", CallingConvention = CallingConvention.StdCall)]
    public static extern uint bdaapiOpenHWIdx(uint DevType, uint uiDevID);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiOpenCISlim", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiOpenCISlim(uint hOpen, CallbackFunctionsSlim CbFuncPointer);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiOpenCIWithoutPointer", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiOpenCIWithoutPointer(uint hOpen);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiCIGetSlotStatus", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiCIGetSlotStatus(uint hOpen, byte nSlot);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiCloseCI", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiCloseCI(uint hOpen);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiClose", CallingConvention = CallingConvention.StdCall)]
    public static extern void bdaapiClose(uint hOpen);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiCIReadPSIFastDrvDemux", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiCIReadPSIFastDrvDemux(uint hOpen, int PNR);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiSetDiSEqCMsg", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiSetDiSEqCMsg(uint hOpen, IntPtr data, byte length, byte repeat, byte toneburst, int polarity);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiSetDVBTAntPwr", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiSetDVBTAntPwr(uint hOpen, bool bAntPwrOnOff);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiGetDVBTAntPwr", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiGetDVBTAntPwr(uint hOpen, ref int uiAntPwrOnOff);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiSetVideoport", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiSetVideoport(uint hOpen, byte CIEnabled, ref byte ciReturnStatus);

    [DllImport("ttBdaDrvApi_Dll.dll", EntryPoint = "bdaapiGetDrvVersion", CallingConvention = CallingConvention.StdCall)]
    public static extern int bdaapiGetDrvVersion(uint hOpen, ref byte v1, ref byte v2, ref byte v3, ref byte v4);
    #endregion

    #region variables
    TechnoTrendDeviceType _deviceType = TechnoTrendDeviceType.Unknown;
    CallbackFunctionsSlim _technoTrendStructure = new CallbackFunctionsSlim();
    uint _handle = 0xffffffff;
    bool _hasCam = false;
    static Hashtable _isCamInitializedTable = new Hashtable();
    IBaseFilter _captureFilter;
    IntPtr _ptrDiseqc;
    #endregion

    #region constants
    public const string Budget2Capture = "TechnoTrend BDA/DVB Capture";
    public const string Budget2DvbcTuner = "TechnoTrend BDA/DVB-C Tuner";
    public const string Budget2DvbsTuner = "TechnoTrend BDA/DVB-S Tuner";
    public const string Budget2DvbtTuner = "TechnoTrend BDA/DVB-T Tuner";
    public const string Budget3Capture = "TTHybridTV BDA Digital Capture";
    public const string Budget3DvbtTuner = "TTHybridTV BDA DVBT Tuner";
    public const string Budget3AnalogTuner = "TTHybridTV BDA Analog TV Tuner";
    public const string Budget3AnalogCapture = "TTHybridTV BDA Analog Capture";
    public const string Usb2Capture = "USB 2.0 BDA DVB Capture";
    public const string Usb2DvbcTuner = "USB 2.0 BDA DVB-C Tuner";
    public const string Usb2DvbsTuner = "USB 2.0 BDA DVB-S Tuner";
    public const string UsbDvbtTuner = "USB 2.0 BDA DVB-T Tuner";
    public const string Usb2PinnacleCapture = "Pinnacle PCTV 400e Capture";
    public const string Usb2PinnacleTuner = "Pinnacle PCTV 400e Tuner";
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="TechnoTrend"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="captureFilter">The capture filter.</param>
    public TechnoTrend(IBaseFilter tunerFilter, IBaseFilter captureFilter)
    {
      _ptrDiseqc = Marshal.AllocCoTaskMem(128);
      _captureFilter = tunerFilter;
      if (_captureFilter == null) return;
      FilterInfo info;
      _captureFilter.QueryFilterInfo(out info);
      if ((info.achName == TechnoTrend.Usb2DvbcTuner) ||
          (info.achName == TechnoTrend.UsbDvbtTuner) ||
          (info.achName == TechnoTrend.Usb2DvbsTuner))
      {
        Log.Log.WriteFile("TechnoTrend card type:usb 2");
        _deviceType = TechnoTrendDeviceType.Usb2;
      }
      else if (info.achName == TechnoTrend.Budget3DvbtTuner)
      {
        Log.Log.WriteFile("TechnoTrend card type:budget 3");
        _deviceType = TechnoTrendDeviceType.Budget3;
      }
      else if ((info.achName == TechnoTrend.Budget2DvbcTuner) ||
                (info.achName == TechnoTrend.Budget2DvbsTuner) ||
                (info.achName == TechnoTrend.Budget2DvbtTuner))
      {
        Log.Log.WriteFile("TechnoTrend card type:budget 2");
        _deviceType = TechnoTrendDeviceType.Budget2;
      }
      else if (info.achName == TechnoTrend.Usb2PinnacleTuner)
      {
        Log.Log.WriteFile("TechnoTrend card type:usb2 pinnacle");
        _deviceType = TechnoTrendDeviceType.Usb2Pinnacle;
      }
      else
      {
        // Log.Log.WriteFile( "Technotrend Unknown card type");
        _deviceType = TechnoTrendDeviceType.Unknown;
      }

      if (!IsTechnoTrend) return;
      try
      {
        int deviceId = GetDeviceID(_captureFilter);
        if (deviceId < 0)
        {
          Log.Log.WriteFile("TechnoTrend: unable to determine device id");
          return;
        }
        _handle = bdaapiOpenHWIdx((UInt32)_deviceType, (uint)deviceId);
        if (_handle != 0xffffffff)
        {
          int hr;
          Log.Log.WriteFile("Technotrend: card detected");
          byte v1, v2, v3, v4;
          v1 = v2 = v3 = v4 = 0;
          hr = bdaapiGetDrvVersion(_handle, ref v1, ref v2, ref v3, ref v4);
          Log.Log.WriteFile("Technotrend: driver version:{0}.{1}.{2}.{3} {4:X}", v1, v2, v3, v4, hr);
          _isCamInitializedTable.Add(_handle, false);
          unsafe
          {
            //_technoTrendStructure.onCAStatus = new PCBFCN_CI_OnCAStatus(OnCAStatus);
            //_technoTrendStructure.onCAStatusContext = _handle;
            //_technoTrendStructure.onSlotStatus = new PCBFCN_CI_OnSlotStatus(OnSlotStatus);
            //_technoTrendStructure.onSlotStatusContext = _handle;
            //hr = bdaapiOpenCISlim(_handle, _technoTrendStructure);
            hr = bdaapiOpenCIWithoutPointer(_handle);
            if (hr == 0)
            {
              Log.Log.WriteFile("Technotrend: CI opened.");
              _hasCam = true;
              //byte enabled = 1;
              //hr = bdaapiSetVideoport(_handle, 1, ref enabled);
              //Log.Log.WriteFile("Technotrend: CI enabled:{0} {1:X}", enabled, hr);
            }
            if (IsTechnoTrendUSBDVBT)
            {
              EnableAntenna(true);
            }
            return;
          }
        }
      }
      catch (Exception)
      {
        Log.Log.WriteFile("Technotrend: unable to initialize (does ttBdaDrvApi_Dll.dll exists?)");
        //int x = 1;
      }
      _deviceType = TechnoTrendDeviceType.Unknown;
    }

    /// <summary>
    /// Gets the device ID.
    /// </summary>
    /// <param name="tunerfilter">The tunerfilter.</param>
    /// <returns></returns>
    int GetDeviceID(IBaseFilter tunerfilter)
    {
      Log.Log.WriteFile("TechnoTrend: GetDeviceID");
      IPin outputPin = DirectShowLib.DsFindPin.ByDirection(tunerfilter, PinDirection.Output, 0);
      if (outputPin == null)
      {
        Log.Log.WriteFile("TechnoTrend: failed to get output pin");
        return -1;
      }
      IKsPin iKsPin = outputPin as IKsPin;
      KSMULTIPLE_ITEM pmi;
      IntPtr pDataReturned;
      int hr = iKsPin.KsQueryMediums(out pDataReturned);
      Release.ComObject("technotrend pin", outputPin);
      if (hr != 0)
      {
        Log.Log.WriteFile("TechnoTrend: Pin does not support Mediums");
        return -1;  // Pin does not support mediums.
      }
      pmi = (KSMULTIPLE_ITEM)Marshal.PtrToStructure(pDataReturned, typeof(KSMULTIPLE_ITEM));
      if (pmi.Count != 0)
      {
        // Use pointer arithmetic to reference the first medium structure.
        int sizeProperty = Marshal.SizeOf(pmi);
        int address = pDataReturned.ToInt32() + sizeProperty;
        IntPtr ptrData = new IntPtr(address);

        REGPINMEDIUM medium = (REGPINMEDIUM)Marshal.PtrToStructure(ptrData, typeof(REGPINMEDIUM));
        int id = (int)medium.dw1;
        Marshal.FreeCoTaskMem(pDataReturned);
        Log.Log.WriteFile("TechnoTrend: Device ID:{0} {1}", medium.dw1, medium.dw2);
        return id;
      }
      else
      {
        Log.Log.WriteFile("TechnoTrend: no mediums detected");
        Marshal.FreeCoTaskMem(pDataReturned);
        return -1;
      }
    }


    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (_handle != 0xffffffff)
      {
        _isCamInitializedTable.Remove(_handle);
        Log.Log.WriteFile("Technotrend: close");
        if (_hasCam)
        {
          bdaapiCloseCI(_handle);
        }
        bdaapiClose(_handle);
      }
      _handle = 0xffffffff;
      _hasCam = false;
    }

    /// <summary>
    /// Determines whether cam is ready.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam ready]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamReady()
    {
      return (bool)_isCamInitializedTable[_handle];
    }

    /// <summary>
    /// Gets a value indicating whether this instance is techno trend.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is techno trend; otherwise, <c>false</c>.
    /// </value>
    public bool IsTechnoTrend
    {
      get
      {
        return (_deviceType != TechnoTrendDeviceType.Unknown);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is techno trend USBDVBT.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is techno trend USBDVBT; otherwise, <c>false</c>.
    /// </value>
    public bool IsTechnoTrendUSBDVBT
    {
      get
      {
        return (_deviceType == TechnoTrendDeviceType.Usb2);
      }
    }

    /// <summary>
    /// Sends the PMT.
    /// </summary>
    /// <param name="serviceId">The service id.</param>
    /// <returns></returns>
    public bool SendPMT(int serviceId)
    {
      Log.Log.WriteFile("Technotrend: SendPMT serviceId:{0}", serviceId);
     /* if ((bool)_isCamInitializedTable[_handle] == false)
      {
        Log.Log.WriteFile("Technotrend: CAM is not ready yet");
        return false;
      }*/
      byte enabled = 1;
      int hr = bdaapiSetVideoport(_handle, 1, ref enabled);
      Log.Log.WriteFile("Technotrend: CI enabled:{0} {1:X}", enabled, hr);

      hr = bdaapiCIReadPSIFastDrvDemux(_handle, serviceId);
      if (hr == 0)
      {
        Log.Log.WriteFile("Technotrend: service decoded");
        return true;
      }
      else
      {
        Log.Log.WriteFile("Technotrend: unable to decode service:{0:X}", hr);
        return false;
      }
    }

    /// <summary>
    /// Sends the diseq command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    public void SendDiseqCommand(DVBSChannel channel)
    {
      // send DISEQC:
      //Data             : 4 bytes in form of 
      //                      0: high word high byte
      //                      1: high word low byte
      //                      2: low word high byte
      //                      3: low word low byte
      //                      data : 0xE01038F0   
      //                          band     : bit 1 (1) (high =1, low  =0)
      //                          polarity : bit 2 (2) (horz =1, vert =0)
      //                          position : bit 3 (4) (Sat BAB=0)
      //                          Option   : bit 4 (8) (option B=1, option A=0)
      //bytes        0   : only toneburst
      //             4   : also lo/hi band, polarization, and diseqc A/A, A/B, B/A, B/B
      //repeatCount  0-2 : number of repeats
      //Toneburst    0   : No Toneburst
      //             1   : Toneburst A (unmodulated)
      //             2   : Toneburst B (modulated)
      //Polarization 0   : vertical
      //             1   : horizontal
      byte toneburst = 0;
      byte repeat = 0;
      byte length = 4;
      byte position = 0;
      byte option = 0;
      switch (channel.DisEqc)
      {
        case DisEqcType.None://simple A
          position = 0;
          option = 0;
          break;
        case DisEqcType.SimpleA://simple A
          position = 0;
          option = 0;
          break;
        case DisEqcType.SimpleB://simple B
          position = 0;
          option = 0;
          break;
        case DisEqcType.Level1AA://Level 1 A/A
          position = 0;
          option = 0;
          break;
        case DisEqcType.Level1BA://Level 1 B/A
          position = 1;
          option = 0;
          break;
        case DisEqcType.Level1AB://Level 1 A/B
          position = 0;
          option = 1;
          break;
        case DisEqcType.Level1BB://Level 1 B/B
          position = 1;
          option = 1;
          break;
      }
      //int lnbFrequency = 10600000;
      bool hiBand = true;
      if (channel.Frequency >= 11700000)
      {
        //lnbFrequency = 10600000;
        hiBand = true;
      }
      else
      {
        //lnbFrequency = 9750000;
        hiBand = false;
      }

      uint diseqc = 0xE01038F0;

      if (hiBand)                 // high band
        diseqc |= 0x00000001;
      else                        // low band
        diseqc &= 0xFFFFFFFE;

      if (channel.Polarisation == Polarisation.LinearV)             // vertikal
        diseqc &= 0xFFFFFFFD;
      else                        // horizontal
        diseqc |= 0x00000002;

      if (position != 0)             // Sat B
        diseqc |= 0x00000004;
      else                        // Sat A
        diseqc &= 0xFFFFFFFB;

      if (option != 0)               // option B
        diseqc |= 0x00000008;
      else                        // option A
        diseqc &= 0xFFFFFFF7;

      Marshal.WriteByte(_ptrDiseqc, 0, (byte)((diseqc >> 24) & 0xff));
      Marshal.WriteByte(_ptrDiseqc, 1, (byte)((diseqc >> 16) & 0xff));
      Marshal.WriteByte(_ptrDiseqc, 2, (byte)((diseqc >> 8) & 0xff));
      Marshal.WriteByte(_ptrDiseqc, 3, (byte)((diseqc) & 0xff));

      int hr = bdaapiSetDiSEqCMsg(_handle, _ptrDiseqc, length, repeat, toneburst, (int)channel.Polarisation);
      Log.Log.WriteFile("Technotrend: Diseqc Command Send:{0:X} {1:X}", diseqc, hr);
    }

    /// <summary>
    /// Determines whether [is cam present].
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if [is cam present]; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamPresent()
    {
      return (_hasCam);
    }

    /// <summary>
    /// Enables the power for the antenna.
    /// only usefull for USB-2 DVBT
    /// </summary>
    /// <param name="onOff">if set to <c>true</c> [on off].</param>
    public void EnableAntenna(bool onOff)
    {
      int uiAntPwrOnOff = 0;
      string Get5vAntennae = "Disabled";
      Log.Log.WriteFile("Setting TechnoTrend DVB-T 5v Antennae Power enabled:{0}", onOff);
      bdaapiSetDVBTAntPwr(_handle, onOff);
      bdaapiGetDVBTAntPwr(_handle, ref uiAntPwrOnOff);
      if (uiAntPwrOnOff == 0) Get5vAntennae = "Disabled";
      if (uiAntPwrOnOff == 1) Get5vAntennae = "Enabled";
      if (uiAntPwrOnOff == 2) Get5vAntennae = "Not Connected";
      Log.Log.WriteFile("TechnoTrend DVB-T 5v Antennae status:{0}", Get5vAntennae);
    }

    /// <summary>
    /// Called by the technotrend driver when slot status changes.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="slot">The slot.</param>
    /// <param name="status">The status.</param>
    /// <param name="slotInfo">The slot info.</param>
    unsafe public static void OnSlotStatus(UInt32 context, Byte slot, byte status, SlotInfo* slotInfo)
    {
      CiSlotStatusType slotStatus = (CiSlotStatusType)status;
      try
      {
        if ((slotStatus == CiSlotStatusType.ModuleOk) || (slotStatus == CiSlotStatusType.CaOk) || (slotStatus == CiSlotStatusType.DebugMessage))
        {
          Log.Log.WriteFile("Technotrend: CAM initialized , status:{0} context:{1:X} slot:{2}", slotStatus, context, slot);
          _isCamInitializedTable[context] = true;
        }
        else
        {
          Log.Log.WriteFile("Technotrend: CAM not initialized, status:{0} context:{1:X} slot:{2}", slotStatus, context, slot);
          _isCamInitializedTable[context] = false;
        }
      }
      catch (Exception ex)
      {
        Log.Log.Error("exception in technotrend:OnSlotStatus()");
        Log.Log.Write(ex);
      }
    }

    /// <summary>
    /// Called by the technotrend driver when the CA status changes.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="slot">The slot.</param>
    /// <param name="replyTag">The reply tag.</param>
    /// <param name="status">The status.</param>
    unsafe public static void OnCAStatus(UInt32 context, Byte slot, Byte replyTag, UInt16 status)
    {
      try
      {
        Log.Log.WriteFile("Technotrend: OnCAStatus: context:{0:X} slot:{1:X} replytag:{2:X} statud:{3:X}", context, slot, replyTag, status);
      }
      catch (Exception ex)
      {
        Log.Log.Error("exception in technotrend:OnCAStatus()");
        Log.Log.Write(ex);
      }
    }
  }
}
