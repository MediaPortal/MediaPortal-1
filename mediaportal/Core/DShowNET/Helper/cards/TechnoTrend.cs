#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using DirectShowLib;
using System.Windows.Forms;
using DShowNET.Helper;

namespace DShowNET
{
  public class TechnoTrend : IksPropertyUtils, IDisposable
  {

    #region enums
    enum TechnoTrendDeviceType
    {
      /// not set
      eTypeUnknown = 0,
      /// Budget 2
      eDevTypeB2,
      /// Budget 3 aka TT-budget T-3000
      eDevTypeB3,
      /// USB 2.0
      eDevTypeUsb2,
      /// USB 2.0 Pinnacle
      eDevTypeUsb2Pinnacle
    } ;

    #endregion
    #region structs
    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct SlotInfo
    {
      /// CI status
      public Byte nStatus;
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
    //public static extern int bdaapiGetDVBTAntPwr(uint hOpen, UIntPtr uiAntPwrOnOff);

    #endregion

    #region variables
    TechnoTrendDeviceType _deviceType = TechnoTrendDeviceType.eTypeUnknown;
    CallbackFunctionsSlim _technoTrendStructure = new CallbackFunctionsSlim();
    uint _handle = 0xffffffff;
    bool _hasCam = false;
    static Hashtable _isCamInitializedTable = new Hashtable();
    #endregion

    #region constants
    public const string BUDGET2_CAPTURE = "TechnoTrend BDA/DVB Capture";
    public const string BUDGET2_C_TUNER = "TechnoTrend BDA/DVB-C Tuner";
    public const string BUDGET2_S_TUNER = "TechnoTrend BDA/DVB-S Tuner";
    public const string BUDGET2_T_TUNER = "TechnoTrend BDA/DVB-T Tuner";
    public const string BUDGET3_CAPTURE = "TTHybridTV BDA Digital Capture";
    public const string BUDGET3_TUNER = "TTHybridTV BDA DVBT Tuner";
    public const string BUDGET3_ANLG_TUNER = "TTHybridTV BDA Analog TV Tuner";
    public const string BUDGET3_ANLG_CAPTURE = "TTHybridTV BDA Analog Capture";
    public const string USB2_CAPTURE = "USB 2.0 BDA DVB Capture";
    public const string USB2_C_TUNER = "USB 2.0 BDA DVB-C Tuner";
    public const string USB2_S_TUNER = "USB 2.0 BDA DVB-S Tuner";
    public const string USB2_T_TUNER = "USB 2.0 BDA DVB-T Tuner";
    public const string USB2_PINNACLE_CAPTURE = "Pinnacle PCTV 400e Capture";
    public const string USB2_PINNACLE_TUNER = "Pinnacle PCTV 400e Tuner";
    #endregion

    public TechnoTrend(IBaseFilter filter)
      : base(filter)
    {
      if (captureFilter == null) return;
      FilterInfo info;
      filter.QueryFilterInfo(out info);
      if ((info.achName == TechnoTrend.USB2_C_TUNER) ||
          (info.achName == TechnoTrend.USB2_T_TUNER) ||
          (info.achName == TechnoTrend.USB2_S_TUNER))
      {
        Log.Info("TechnoTrend card type:{0}", TechnoTrendDeviceType.eDevTypeUsb2);
        _deviceType = TechnoTrendDeviceType.eDevTypeUsb2;
      }
      else if (info.achName == TechnoTrend.BUDGET3_TUNER)
      {
        Log.Info("TechnoTrend card type:{0}", TechnoTrendDeviceType.eDevTypeB3);
        _deviceType = TechnoTrendDeviceType.eDevTypeB3;
      }
      else if ((info.achName == TechnoTrend.BUDGET2_C_TUNER) ||
                (info.achName == TechnoTrend.BUDGET2_S_TUNER) ||
                (info.achName == TechnoTrend.BUDGET2_T_TUNER))
      {
        Log.Info("TechnoTrend card type:{0}", TechnoTrendDeviceType.eDevTypeB2);
        _deviceType = TechnoTrendDeviceType.eDevTypeB2;
      }
      else if (info.achName == TechnoTrend.USB2_PINNACLE_TUNER)
      {
        Log.Info("TechnoTrend card type:{0}", TechnoTrendDeviceType.eDevTypeUsb2Pinnacle);
        _deviceType = TechnoTrendDeviceType.eDevTypeUsb2Pinnacle;
      }
      else
      {
        // Log.Info("Technotrend Unknown card type");
        _deviceType = TechnoTrendDeviceType.eTypeUnknown;
      }

      if (!IsTechnoTrend) return;
      try
      {
        uint deviceId = (uint)GetDeviceID(filter);
        _handle = bdaapiOpenHWIdx((UInt32)_deviceType, deviceId);
        if (_handle != 0xffffffff)
        {
          Log.Info("Technotrend: card detected");
          _isCamInitializedTable.Add(_handle, false);
          unsafe
          {
            _technoTrendStructure.onCAStatus = new PCBFCN_CI_OnCAStatus(OnCAStatus);
            _technoTrendStructure.onCAStatusContext = _handle;
            _technoTrendStructure.onSlotStatus = new PCBFCN_CI_OnSlotStatus(OnSlotStatus);
            _technoTrendStructure.onSlotStatusContext = _handle;
            int hr = bdaapiOpenCISlim(_handle, _technoTrendStructure);
            if (hr == 0)
            {
              Log.Info("Technotrend: CI opened");
              _hasCam = true;
            }
            return;
          }
        }
      }
      catch (Exception)
      {
        Log.Error("Technotrend: unable to initialize (does ttBdaDrvApi_Dll.dll exists?)");
        //int x = 1;
      }
      _deviceType = TechnoTrendDeviceType.eTypeUnknown;
    }

    int GetDeviceID(IBaseFilter tunerfilter)
    {
      Log.Info("TechnoTrend: Looking Device ID");
      IPin outputPin = DirectShowLib.DsFindPin.ByDirection(tunerfilter, PinDirection.Output, 0);
      if (outputPin == null)
        return -1;
      Log.Info("TechnoTrend: Got Pin");
      IKsPin iKsPin = outputPin as IKsPin;
      KSMULTIPLE_ITEM pmi;
      IntPtr pDataReturned;
      int hr = iKsPin.KsQueryMediums(out pDataReturned);
      DirectShowUtil.ReleaseComObject(outputPin);
      if (hr != 0)
      {
        Log.Info("TechnoTrend: Pin does not support Mediums");
        return -1;  // Pin does not support mediums.
      }
      pmi = (KSMULTIPLE_ITEM)Marshal.PtrToStructure(pDataReturned, typeof(KSMULTIPLE_ITEM));
      Log.Info("TechnoTrend: Got Mediums:{0}", pmi.Count);
      if (pmi.Count != 0)
      {
        // Use pointer arithmetic to reference the first medium structure.
        int sizeProperty = Marshal.SizeOf(pmi);
        int address = pDataReturned.ToInt32() + sizeProperty;
        IntPtr ptrData = new IntPtr(address);

        REGPINMEDIUM medium = (REGPINMEDIUM)Marshal.PtrToStructure(ptrData, typeof(REGPINMEDIUM));
        int id = (int)medium.dw1;
        Marshal.FreeCoTaskMem(pDataReturned);
        Log.Info("TechnoTrend: Device ID:{0}", id);
        return id;
      }
      else
      {
        Marshal.FreeCoTaskMem(pDataReturned);
        return -1;
      }
    }


    public void Dispose()
    {
      if (_handle != 0xffffffff)
      {
        _isCamInitializedTable.Remove(_handle);
        Log.Info("Technotrend: closing");
        if (_hasCam)
        {
          bdaapiCloseCI(_handle);
          Log.Info("Technotrend: close CI");
        }
        bdaapiClose(_handle);
        Log.Info("Technotrend: closed");
      }
      _handle = 0xffffffff;
      _hasCam = false;
    }

    public bool IsTechnoTrend
    {
      get
      {
        return (_deviceType != TechnoTrendDeviceType.eTypeUnknown);
      }
    }

    public bool IsTechnoTrendUSBDVBT
    {
      get
      {
        return (_deviceType == TechnoTrendDeviceType.eDevTypeUsb2);
      }
    }

    public bool SendPMT(int serviceId)
    {
      if ((bool)_isCamInitializedTable[_handle] == false)
      {
        Log.Info("Technotrend: service cannot be decoded because the CAM is not ready yet");
        return false;
      }
      int hr = bdaapiCIReadPSIFastDrvDemux(_handle, serviceId);
      if (hr == 0)
      {
        Log.Info("Technotrend: service decoded");
        return true;
      }
      else
      {
        Log.Info("Technotrend: unable to decode service");
        return false;
      }
    }

    public void SendDiseqCommand(int antennaNr, int frequency, int switchingFrequency, int polarisation, int diseqcType)
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
      switch (diseqcType)
      {
        case 0:
          goto case 1;
        case 1://simple A
          position = 0;
          option = 0;
          break;
        case 2://simple B
          position = 0;
          option = 0;
          break;
        case 3://Level 1 A/A
          position = 0;
          option = 0;
          break;
        case 4://Level 1 B/A
          position = 1;
          option = 0;
          break;
        case 5://Level 1 A/B
          position = 0;
          option = 1;
          break;
        case 6://Level 1 B/B
          position = 1;
          option = 1;
          break;
      }
      IntPtr ptrData = Marshal.AllocCoTaskMem(4);
      try
      {
        int pol;
        uint diseqc = 0xE01038F0;

        if (frequency > switchingFrequency)                 // high band
          diseqc |= 0x00000001;
        else                        // low band
          diseqc &= 0xFFFFFFFE;

        if (polarisation == 1)             // vertikal
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

        if (polarisation == 0)//horizontal
          pol = (int)TunerLib.Polarisation.BDA_POLARISATION_LINEAR_H;
        else
          pol = (int)TunerLib.Polarisation.BDA_POLARISATION_LINEAR_V;

        Marshal.WriteByte(ptrData, 0, (byte)((diseqc >> 24) & 0xff));
        Marshal.WriteByte(ptrData, 1, (byte)((diseqc >> 16) & 0xff));
        Marshal.WriteByte(ptrData, 2, (byte)((diseqc >> 8) & 0xff));
        Marshal.WriteByte(ptrData, 3, (byte)((diseqc) & 0xff));

        bdaapiSetDiSEqCMsg(_handle, ptrData, length, repeat, toneburst, pol);
        Log.Info("Technotrend: Diseqc Command Send");
      }
      finally
      {
        Marshal.FreeCoTaskMem(ptrData);
      }
    }

    public bool IsCamPresent()
    {
      return (_hasCam);
    }

    // Here we turn on the USB DVB-T antennae
    public void EnableAntenna(bool onOff)
    {
      int uiAntPwrOnOff = 0;
      string Get5vAntennae = "Disabled";
      Log.Info("Setting TechnoTrend DVB-T 5v Antennae Power enabled:{0}", onOff);
      bdaapiSetDVBTAntPwr(_handle, onOff);
      bdaapiGetDVBTAntPwr(_handle, ref uiAntPwrOnOff);
      if (uiAntPwrOnOff == 0) Get5vAntennae = "Disabled";
      if (uiAntPwrOnOff == 1) Get5vAntennae = "Enabled";
      if (uiAntPwrOnOff == 2) Get5vAntennae = "Not Connected";
      Log.Info("TechnoTrend DVB-T 5v Antennae status:{0}", Get5vAntennae);
    }

    unsafe public static void OnSlotStatus(UInt32 Context, Byte nSlot, Byte nStatus, SlotInfo* csInfo)
    {
      if ((nStatus == 2) || (nStatus == 3) || (nStatus == 4))
      {
        Log.Info("Technotrend: CAM initialized {0}", Context);
        _isCamInitializedTable[Context] = true;
      }
      else
      {
        Log.Info("Technotrend: CAM not initialized, Card;{0} status:{1}", Context, nStatus);
        _isCamInitializedTable[Context] = false;
      }

    }

    unsafe public static void OnCAStatus(UInt32 Context, Byte nSlot, Byte nReplyTag, UInt16 wStatus)
    {
    }
  }
}
