#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;
using System.Collections;
using System.Runtime.InteropServices;
using DirectShowLib;
using MediaPortal.GUI.Library;

namespace DShowNET
{
  public class DigitalEverywhere : IksPropertyUtils
  {
    #region structs

    [StructLayout(LayoutKind.Explicit, Size = 56), ComVisible(true)]
    private struct FIRESAT_SELECT_PIDS_DVBS //also for DVBC
    {
      [FieldOffset(0)] public bool bCurrentTransponder;
      [FieldOffset(4)] public bool bFullTransponder;
      [FieldOffset(8)] public bool uLnb;
      [FieldOffset(12)] public uint uFrequency;
      [FieldOffset(16)] public uint uSymbolRate;
      [FieldOffset(20)] public byte uFecInner;
      [FieldOffset(21)] public byte uPolarization;
      [FieldOffset(22)] public byte dummy1; // 1-16
      [FieldOffset(23)] public byte dummy2; // 
      [FieldOffset(24)] public byte uNumberOfValidPids; // 1-16
      [FieldOffset(25)] public byte dummy3; // 
      [FieldOffset(26)] public ushort uPid1;
      [FieldOffset(28)] public ushort uPid2;
      [FieldOffset(30)] public ushort uPid3;
      [FieldOffset(32)] public ushort uPid4;
      [FieldOffset(34)] public ushort uPid5;
      [FieldOffset(36)] public ushort uPid6;
      [FieldOffset(38)] public ushort uPid7;
      [FieldOffset(40)] public ushort uPid8;
      [FieldOffset(42)] public ushort uPid9;
      [FieldOffset(44)] public ushort uPid10;
      [FieldOffset(46)] public ushort uPid11;
      [FieldOffset(48)] public ushort uPid12;
      [FieldOffset(50)] public ushort uPid13;
      [FieldOffset(52)] public ushort uPid14;
      [FieldOffset(54)] public ushort uPid15;
      [FieldOffset(56)] public ushort uPid16;
      [FieldOffset(58)] public ushort dummy4;
    }

    [StructLayout(LayoutKind.Explicit, Size = 56), ComVisible(true)]
    private struct FIRESAT_SELECT_PIDS_DVBT
    {
      [FieldOffset(0)] public bool bCurrentTransponder; //Set TRUE
      [FieldOffset(4)] public bool bFullTransponder; //Set FALSE when selecting PIDs
      [FieldOffset(8)] public uint uFrequency; // kHz 47.000-860.000
      [FieldOffset(12)] public byte uBandwidth; // BANDWIDTH_8_MHZ, BANDWIDTH_7_MHZ, BANDWIDTH_6_MHZ

      [FieldOffset(13)] public byte uConstellation;
                                    // CONSTELLATION_DVB_T_QPSK,CONSTELLATION_QAM_16,CONSTELLATION_QAM_64,OFDM_AUTO

      [FieldOffset(14)] public byte uCodeRateHP; // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO
      [FieldOffset(15)] public byte uCodeRateLP; // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO

      [FieldOffset(16)] public byte uGuardInterval;
                                    // GUARD_INTERVAL_1_32,GUARD_INTERVAL_1_16,GUARD_INTERVAL_1_8,GUARD_INTERVAL_1_4,OFDM_AUTO

      [FieldOffset(17)] public byte uTransmissionMode; // TRANSMISSION_MODE_2K, TRANSMISSION_MODE_8K, OFDM_AUTO
      [FieldOffset(18)] public byte uHierarchyInfo; // HIERARCHY_NONE,HIERARCHY_1,HIERARCHY_2,HIERARCHY_4,OFDM_AUTO
      [FieldOffset(19)] public byte dummy; // 
      [FieldOffset(20)] public byte uNumberOfValidPids; // 1-16
      [FieldOffset(21)] public byte dummy2; // 
      [FieldOffset(22)] public ushort uPid1;
      [FieldOffset(24)] public ushort uPid2;
      [FieldOffset(26)] public ushort uPid3;
      [FieldOffset(28)] public ushort uPid4;
      [FieldOffset(30)] public ushort uPid5;
      [FieldOffset(32)] public ushort uPid6;
      [FieldOffset(34)] public ushort uPid7;
      [FieldOffset(36)] public ushort uPid8;
      [FieldOffset(38)] public ushort uPid9;
      [FieldOffset(40)] public ushort uPid10;
      [FieldOffset(42)] public ushort uPid11;
      [FieldOffset(44)] public ushort uPid12;
      [FieldOffset(46)] public ushort uPid13;
      [FieldOffset(48)] public ushort uPid14;
      [FieldOffset(50)] public ushort uPid15;
      [FieldOffset(52)] public ushort uPid16;
      [FieldOffset(54)] public ushort dummy3;
    }

    #endregion

    public static readonly Guid KSPROPSETID_Firesat = new Guid(0xab132414, 0xd060, 0x11d0, 0x85, 0x83, 0x00, 0xc0, 0x4f,
                                                               0xd9, 0xba, 0xf3);

    #region property ids

    private const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C = 8;
    private const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T = 6;
    private const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S = 2;
    private const int KSPROPERTY_FIRESAT_HOST2CA = 22;
    private const int KSPROPERTY_FIRESAT_DRIVER_VERSION = 4;
    private const int KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION = 11;
    private const int KSPROPERTY_FIRESAT_GET_CI_STATUS = 28;
    private const int KSPROPERTY_FIRESAT_LNB_CONTROL = 12;

    #endregion

    #region CI STATUS bits

    private const int CI_MMI_REQUEST = 0x0100;
    private const int CI_PMT_REPLY = 0x0080;
    private const int CI_DATE_TIME_REQEST = 0x0040;
    private const int CI_APP_INFO_AVAILABLE = 0x0020;
    private const int CI_MODULE_PRESENT = 0x0010;
    private const int CI_MODULE_IS_DVB = 0x0008;
    private const int CI_MODULE_ERROR = 0x0004;
    private const int CI_MODULE_INIT_READY = 0x0002;
    private const int CI_ERR_MSG_AVAILABLE = 0x0001;

    #endregion

    #region variables

    private bool _isDigitalEverywhere;
    private bool _hasCAM;
    private bool _isInitialized;

    private int _prevDisEqcType = -1;
    private int _prevFrequency = -1;
    private int _prevPolarisation = -1;

    #endregion

    public DigitalEverywhere(IBaseFilter filter)
      : base(filter)
    {
      _hasCAM = false;
      _isInitialized = false;
      _isDigitalEverywhere = false;

      if (captureFilter != null)
      {
        _isDigitalEverywhere = IsDigitalEverywhere;
        if (_isDigitalEverywhere)
        {
          _hasCAM = IsCamPresent();

          //Log.Info("FireDTV Driver version:{0} ", GetDriverVersionNumber());
          //Log.Info("FireDTV FW version:{0} ", GetFirmwareVersionNumber());
        }
      }
      _isInitialized = true;
    }

    public bool IsDigitalEverywhere
    {
      get
      {
        if (_isInitialized)
        {
          return _isDigitalEverywhere;
        }

        IKsPropertySet propertySet = captureFilter as IKsPropertySet;
        if (propertySet == null)
        {
          return false;
        }
        Guid propertyGuid = KSPROPSETID_Firesat;
        uint isTypeSupported = 0;
        int hr = propertySet.QuerySupported(ref propertyGuid, KSPROPERTY_FIRESAT_HOST2CA, out isTypeSupported);
        if (hr != 0 || (isTypeSupported & (uint) KsPropertySupport.Set) == 0)
        {
          return false;
        }
        return true;
      }
    }

    /// <summary>
    /// This function sends the PMT (Program Map Table) to the FireDTV DVB-T/DVB-C/DVB-S card
    /// This allows the integrated CI & CAM module inside the FireDTv device to decrypt the current TV channel
    /// (provided that offcourse a smartcard with the correct subscription and its inserted in the CAM)
    /// </summary>
    /// <param name="PMT">Program Map Table received from digital transport stream</param>
    /// <remarks>
    /// 1. first byte in PMT is 0x02=tableId for PMT
    /// 2. This function is vender specific. It will only work on the FireDTV devices
    /// </remarks>
    /// <preconditions>
    /// 1. FireDTV device should be tuned to a digital DVB-C/S/T TV channel 
    /// 2. PMT should have been received 
    /// </preconditions>
    public bool SendPMTToFireDTV(byte[] PMT, int pmtLength)
    {
      if (_hasCAM == false)
      {
        return true;
      }
      if (IsCamReady() == false)
      {
        ResetCAM();
      }

      //typedef struct _FIRESAT_CA_DATA{ 
      //  UCHAR uSlot;                      //0
      //  UCHAR uTag;                       //1   (2..3 = padding)
      //  BOOL bMore;                       //4   (5..7 = padding)
      //  USHORT uLength;                   //8..9
      //  UCHAR uData[MAX_PMT_SIZE];        //10....
      //}FIRESAT_CA_DATA, *PFIRESAT_CA_DATA;

      if (PMT == null)
      {
        return false;
      }
      if (pmtLength == 0)
      {
        return false;
      }

      //Log.Info("SendPMTToFireDTV pmt:{0}", pmtLength);
      Guid propertyGuid = KSPROPSETID_Firesat;
      int propId = KSPROPERTY_FIRESAT_HOST2CA;
      DirectShowLib.IKsPropertySet propertySet = captureFilter as DirectShowLib.IKsPropertySet;
      KSPropertySupport isTypeSupported = 0;
      if (propertySet == null)
      {
        Log.Info("FireDTV:SendPmt() properySet=null");
        return true;
      }

      int hr = propertySet.QuerySupported(propertyGuid, propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Info("FireDTV:SendPmt() not supported");
        return true;
      }

      int iSize = 12 + 2 + pmtLength;
      IntPtr pDataInstance = Marshal.AllocCoTaskMem(1036);
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(1036);
      int offs = 0;

      //example data:0x0 0x2 0x0 0x0 0x0 0x0 0x0 0x0 0x14 0x0 0x3 0x1 | 0x2 0xB0 0x12 0x1 0x3D 0xC1 0x0 0x0 0xFF 0xFF 0xF0 0x0 0x3 0xEC 0x8C 0xF0 0x0 0xD3 
      byte[] byData = new byte[1036];
      uint uLength = (uint) (2 + pmtLength);
      byData[offs] = 0;
      offs++; //slot
      byData[offs] = 2;
      offs++; //utag

      byData[offs] = 0;
      offs++; //padding
      byData[offs] = 0;
      offs++; //padding

      byData[offs] = 0;
      offs++; //bmore

      byData[offs] = 0;
      offs++; //padding
      byData[offs] = 0;
      offs++; //padding
      byData[offs] = 0;
      offs++; //padding

      byData[offs] = (byte) (uLength%256);
      offs++; //ulength lo
      byData[offs] = (byte) (uLength/256);
      offs++; //ulength hi

      //byData[offs]= 0; offs++;
      //byData[offs]= 0; offs++;

      byData[offs] = 3;
      offs++; // List Management = ONLY
      byData[offs] = 1;
      offs++; // pmt_cmd = OK DESCRAMBLING		
      for (int i = 0; i < pmtLength; ++i)
      {
        byData[offs] = PMT[i];
        offs++;
      }

      string log = String.Format("FireDTV: pmt len:{0} data:", pmtLength);
      for (int i = 0; i < offs; ++i)
      {
        Marshal.WriteByte(pDataInstance, i, byData[i]);
        Marshal.WriteByte(pDataReturned, i, byData[i]);
        log += String.Format("0x{0:X} ", byData[i]);
      }

      Log.Info(log);
      hr = propertySet.Set(propertyGuid, propId, pDataInstance, 1036, pDataReturned, 1036);
      Marshal.FreeCoTaskMem(pDataReturned);
      Marshal.FreeCoTaskMem(pDataInstance);
      if (hr != 0)
      {
        Log.Error("FireDTV:  failed 0x{0:X} offs:{1}", hr, offs);
        ResetCAM();
        return false;
      }
      return true;
    } //public bool SendPMTToFireDTV(byte[] PMT)

    public void ResetCAM()
    {
      //typedef struct _FIRESAT_CA_DATA{ 
      //  UCHAR uSlot;                      //0
      //  UCHAR uTag;                       //1   (2..3 = padding)
      //  BOOL bMore;                       //4   (5..7 = padding)
      //  USHORT uLength;                   //8..9
      //  UCHAR uData[MAX_PMT_SIZE];        //10....
      //}FIRESAT_CA_DATA, *PFIRESAT_CA_DATA;

      Log.Info("FireDTV:ResetCAM()");
      Guid propertyGuid = KSPROPSETID_Firesat;
      int propId = KSPROPERTY_FIRESAT_HOST2CA;
      IKsPropertySet propertySet = captureFilter as IKsPropertySet;
      uint isTypeSupported = 0;
      if (propertySet == null)
      {
        Log.Info("FireDTV:ResetCAM() properySet=null");
        return;
      }

      int hr = propertySet.QuerySupported(ref propertyGuid, (uint) propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & (uint) KsPropertySupport.Set) == 0)
      {
        Log.Info("FireDTV:ResetCAM() Reset CI is not supported");
        return;
      }
      int dataLength = 1;
      int iSize = 12 + dataLength;
      IntPtr pDataInstance = Marshal.AllocCoTaskMem(1036);
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(1036);
      try
      {
        int offs = 0;

        //example data:0x0 0x2 0x0 0x0 0x0 0x0 0x0 0x0 0x14 0x0 0x3 0x1 | 0x2 0xB0 0x12 0x1 0x3D 0xC1 0x0 0x0 0xFF 0xFF 0xF0 0x0 0x3 0xEC 0x8C 0xF0 0x0 0xD3 
        byte[] byData = new byte[1036];
        uint uLength = (uint) dataLength;
        byData[offs] = 0;
        offs++; //slot
        byData[offs] = 0;
        offs++; //utag (CA RESET)

        byData[offs] = 0;
        offs++; //padding
        byData[offs] = 0;
        offs++; //padding

        byData[offs] = 0;
        offs++; //bmore (FALSE)

        byData[offs] = 0;
        offs++; //padding
        byData[offs] = 0;
        offs++; //padding
        byData[offs] = 0;
        offs++; //padding

        byData[offs] = (byte) (uLength%256);
        offs++; //ulength lo
        byData[offs] = (byte) (uLength/256);
        offs++; //ulength hi

        byData[offs] = 0;
        offs++; // HW Reset of CI part
        string log = "hw resetdata:";
        for (int i = 0; i < offs; ++i)
        {
          Marshal.WriteByte(pDataInstance, i, byData[i]);
          Marshal.WriteByte(pDataReturned, i, byData[i]);
          log += String.Format("0x{0:X} ", byData[i]);
        }

        Log.Info(log);
        hr = propertySet.RemoteSet(ref propertyGuid, (uint) propId, pDataInstance, (uint) 1036, pDataReturned,
                                   (uint) 1036);

        if (hr != 0)
        {
          Log.Error("FireDTV:ResetCAM() failed 0x{0:X} offs:{1}", hr, offs);
          return;
        }
        Log.Info("FireDTV:ResetCAM() cam has been reset");
      }
      finally
      {
        Marshal.FreeCoTaskMem(pDataReturned);
        Marshal.FreeCoTaskMem(pDataInstance);
      }
      return;
    }

    public bool SetHardwarePidFiltering(bool isDvbc, bool isDvbT, bool isDvbS, bool isAtsc, ArrayList pids)
    {
      string logStart = "dvbt:";
      DirectShowLib.IKsPropertySet propertySet = captureFilter as DirectShowLib.IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      KSPropertySupport isTypeSupported = 0;
      uint propertySelect = (uint) KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T;
      //if (isDvbc)
      //{
      //  propertySelect = (uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C;
      //  logStart = "dvbc:";
      //}
      if (isDvbc || isDvbS)
      {
        propertySelect = (uint) KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S;
        logStart = "dvbs:";
      }
      int hr = propertySet.QuerySupported(propertyGuid, (int) propertySelect, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Info("FireDTV: Set H/W pid filtering is not supported");
        return true;
      }

      FIRESAT_SELECT_PIDS_DVBT dvbtStruct = new FIRESAT_SELECT_PIDS_DVBT();
      FIRESAT_SELECT_PIDS_DVBS dvbsStruct = new FIRESAT_SELECT_PIDS_DVBS();
      dvbtStruct.bCurrentTransponder = true;
      dvbtStruct.bFullTransponder = false;
      dvbsStruct.bCurrentTransponder = true;
      dvbsStruct.bFullTransponder = false;
      if (pids.Count > 0)
      {
        int pidCount = pids.Count;
        if (pidCount > 16)
        {
          pidCount = 16;
        }
        //get only specific pids
        dvbtStruct.bFullTransponder = false;
        dvbtStruct.uNumberOfValidPids = (byte) pidCount;
        dvbsStruct.bFullTransponder = false;
        dvbsStruct.uNumberOfValidPids = (byte) pidCount;
        if (pids.Count >= 1)
        {
          dvbtStruct.uPid1 = (ushort) pids[0];
          dvbsStruct.uPid1 = (ushort) pids[0];
        }
        if (pids.Count >= 2)
        {
          dvbtStruct.uPid2 = (ushort) pids[1];
          dvbsStruct.uPid2 = (ushort) pids[1];
        }
        if (pids.Count >= 3)
        {
          dvbtStruct.uPid3 = (ushort) pids[2];
          dvbsStruct.uPid3 = (ushort) pids[2];
        }
        if (pids.Count >= 4)
        {
          dvbtStruct.uPid4 = (ushort) pids[3];
          dvbsStruct.uPid4 = (ushort) pids[3];
        }
        if (pids.Count >= 5)
        {
          dvbtStruct.uPid5 = (ushort) pids[4];
          dvbsStruct.uPid5 = (ushort) pids[4];
        }
        if (pids.Count >= 6)
        {
          dvbtStruct.uPid6 = (ushort) pids[5];
          dvbsStruct.uPid6 = (ushort) pids[5];
        }
        if (pids.Count >= 7)
        {
          dvbtStruct.uPid7 = (ushort) pids[6];
          dvbsStruct.uPid7 = (ushort) pids[6];
        }
        if (pids.Count >= 8)
        {
          dvbtStruct.uPid8 = (ushort) pids[7];
          dvbsStruct.uPid8 = (ushort) pids[7];
        }
        if (pids.Count >= 9)
        {
          dvbtStruct.uPid9 = (ushort) pids[8];
          dvbsStruct.uPid9 = (ushort) pids[8];
        }
        if (pids.Count >= 10)
        {
          dvbtStruct.uPid10 = (ushort) pids[9];
          dvbsStruct.uPid10 = (ushort) pids[9];
        }
        if (pids.Count >= 11)
        {
          dvbtStruct.uPid11 = (ushort) pids[10];
          dvbsStruct.uPid11 = (ushort) pids[10];
        }
        if (pids.Count >= 12)
        {
          dvbtStruct.uPid12 = (ushort) pids[11];
          dvbsStruct.uPid12 = (ushort) pids[11];
        }
        if (pids.Count >= 13)
        {
          dvbtStruct.uPid13 = (ushort) pids[12];
          dvbsStruct.uPid13 = (ushort) pids[12];
        }
        if (pids.Count >= 14)
        {
          dvbtStruct.uPid14 = (ushort) pids[13];
          dvbsStruct.uPid14 = (ushort) pids[13];
        }
        if (pids.Count >= 15)
        {
          dvbtStruct.uPid15 = (ushort) pids[14];
          dvbsStruct.uPid15 = (ushort) pids[14];
        }
        if (pids.Count >= 16)
        {
          dvbtStruct.uPid16 = (ushort) pids[15];
          dvbsStruct.uPid16 = (ushort) pids[15];
        }
      }
      else
      {
        //get entire stream
        dvbtStruct.bFullTransponder = true;
        dvbsStruct.bFullTransponder = true;
        dvbtStruct.uNumberOfValidPids = 0;
        dvbsStruct.uNumberOfValidPids = 0;
      }

      IntPtr pDataInstance;
      IntPtr pDataReturned;
      int len;
      if (isDvbT)
      {
        len = Marshal.SizeOf(dvbtStruct);
        pDataInstance = Marshal.AllocCoTaskMem(len);
        pDataReturned = Marshal.AllocCoTaskMem(len);
        Marshal.StructureToPtr(dvbtStruct, pDataInstance, true);
        Marshal.StructureToPtr(dvbtStruct, pDataReturned, true);
      }
      else
      {
        len = Marshal.SizeOf(dvbsStruct);
        pDataInstance = Marshal.AllocCoTaskMem(len);
        pDataReturned = Marshal.AllocCoTaskMem(len);
        Marshal.StructureToPtr(dvbsStruct, pDataInstance, true);
        Marshal.StructureToPtr(dvbsStruct, pDataReturned, true);
      }

      Log.Info("FireDTV: Set H/W pid filtering count:{0} len:{1}", pids.Count, len);

      string txt = "";
      for (int i = 0; i < len; ++i)
      {
        txt += String.Format("0x{0:X} ", Marshal.ReadByte(pDataInstance, i));
      }

      Log.Info("FireDTV: Set H/W pid filtering pid {0} data:{1}", logStart, txt);
      hr = propertySet.Set(propertyGuid,
                           (int) propertySelect,
                           pDataInstance, (int) len,
                           pDataReturned, (int) len);
      Marshal.FreeCoTaskMem(pDataReturned);
      Marshal.FreeCoTaskMem(pDataInstance);
      if (hr != 0)
      {
        Log.Error("FireDTV: Set H/W pid filtering failed 0x{0:X}", hr);
        return false;
      }

      return true;
    }

    public string GetFirmwareVersionNumber()
    {
      DirectShowLib.IKsPropertySet propertySet = captureFilter as DirectShowLib.IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      KSPropertySupport isTypeSupported = 0;
      int hr = propertySet.QuerySupported(propertyGuid, (int) KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION,
                                          out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
      {
        Log.Error("FireDTV:GetDriverVersion() not supported");
        return string.Empty;
      }
      int byteCount = 0;
      IntPtr pDataInstance = Marshal.AllocHGlobal(100);
      IntPtr pDataReturned = Marshal.AllocHGlobal(100);
      hr = propertySet.Get(propertyGuid,
                           (int) KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION,
                           pDataInstance, (int) 100,
                           pDataReturned, (int) 100, out byteCount);
      Marshal.FreeHGlobal(pDataReturned);
      Marshal.FreeHGlobal(pDataInstance);

      if (hr != 0)
      {
        Log.Error("FireDTV:GetFirmwareVersionNumber() failed 0x{0:X}", hr);
        return string.Empty;
      }
      Log.Info("count:{0}", byteCount);

      string version = string.Empty;
      for (int i = 0; i < byteCount; ++i)
      {
        char ch;
        byte k = Marshal.ReadByte(pDataReturned, i);

        Log.Info("{0} = 0x{1:X} = {2} = {3}",
                 i, k, k, (char) k);
        if (k < 0x20)
        {
          ch = '.';
        }
        else
        {
          ch = (char) k;
        }
        version += ch;
      }
      return version;
    }

    public string GetDriverVersionNumber()
    {
      DirectShowLib.IKsPropertySet propertySet = captureFilter as DirectShowLib.IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      KSPropertySupport isTypeSupported = 0;
      int hr = propertySet.QuerySupported(propertyGuid, (int) KSPROPERTY_FIRESAT_DRIVER_VERSION, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
      {
        Log.Error("FireDTV:GetDriverVersion() not supported");
        return string.Empty;
      }
      int byteCount = 0;
      IntPtr pDataInstance = Marshal.AllocHGlobal(22);
      IntPtr pDataReturned = Marshal.AllocHGlobal(22);
      hr = propertySet.Get(propertyGuid,
                           (int) KSPROPERTY_FIRESAT_DRIVER_VERSION,
                           pDataInstance, (int) 22,
                           pDataReturned, (int) 22, out byteCount);
      Marshal.FreeHGlobal(pDataReturned);
      Marshal.FreeHGlobal(pDataInstance);

      if (hr != 0)
      {
        Log.Error("FireDTV:GetDriverVersion() failed 0x{0:X}", hr);
        return string.Empty;
      }
      Log.Info("count:{0}", byteCount);

      string version = string.Empty;

      for (int i = 0; i < byteCount; ++i)
      {
        char ch;
        byte k = Marshal.ReadByte(pDataReturned, i);

        Log.Info("{0} = 0x{1:X} = {2} = {3}",
                 i, k, k, (char) k);
        if (k < 0x20)
        {
          ch = '.';
        }
        else
        {
          ch = (char) k;
        }
        version += ch;
      }
      return version;
    }

    private int GetCAMStatus()
    {
      Guid propertyGuid = KSPROPSETID_Firesat;
      int propId = KSPROPERTY_FIRESAT_GET_CI_STATUS;
      DirectShowLib.IKsPropertySet propertySet = captureFilter as DirectShowLib.IKsPropertySet;
      KSPropertySupport isTypeSupported;
      if (propertySet == null)
      {
        Log.Info("FireDTV:GetCAMStatus() properySet=null");
        return 0;
      }

      int hr = propertySet.QuerySupported(propertyGuid, propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
      {
        Log.Info("FireDTV:GetCAMStatus() get is not supported");
        return 0;
      }
      int bytesReturned;
      IntPtr pDataInstance = Marshal.AllocCoTaskMem(1036);
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(1036);
      try
      {
        hr = propertySet.Get(propertyGuid, propId, pDataInstance, 1036, pDataReturned, 1036, out bytesReturned);
        if (hr != 0)
        {
          Log.Error("FireDTV:GetCAMStatus() failed 0x{0:X}", hr);
          if (((uint) hr) == ((uint) 0x8007001F))
          {
            ResetCAM();
            hr = propertySet.Get(propertyGuid, propId, pDataInstance, 1036, pDataReturned, 1036, out bytesReturned);
            if (hr != 0)
            {
              return 0;
            }
          }
          else
          {
            return 0;
          }
        }
        ushort camStatus = (ushort) Marshal.ReadInt16(pDataReturned, 0);
        return camStatus;
      }
      finally
      {
        Marshal.FreeCoTaskMem(pDataReturned);
        Marshal.FreeCoTaskMem(pDataInstance);
      }
    }

    public bool IsCamPresent()
    {
      if (_isInitialized)
      {
        return _hasCAM;
      }
      int camStatus = GetCAMStatus();
      if ((camStatus & CI_MODULE_PRESENT) != 0)
      {
        //CAM is inserted
        if ((camStatus & CI_MODULE_IS_DVB) != 0)
        {
          //CAM is DVB
          return true;
        }
      }
      return false;
    }

    public bool IsCamReady()
    {
      int camStatus = GetCAMStatus();
      if ((camStatus & CI_MODULE_PRESENT) != 0)
      {
        //CAM is inserted
        if ((camStatus & CI_MODULE_IS_DVB) != 0)
        {
          //CAM is DVB CAM 
          if ((camStatus & CI_MODULE_ERROR) != 0)
          {
            //CAM has an error
            return false;
          }
          if ((camStatus & CI_MODULE_INIT_READY) != 0)
          {
            //CAM is initialized
            return true;
          }
        }
      }
      return false;
    }

    public void SendDiseqCommand(int disEqcType, int frequency, int switchingFrequency, int polarisation)
    {
      if (_prevDisEqcType == disEqcType && _prevFrequency == frequency && _prevPolarisation == polarisation)
      {
        Log.Info("FireDTV: Skipping DiSEqC command for type={0}, freq={1}, pol={2}", disEqcType, frequency, polarisation);
        return;
      }

      int antennaNr = 1;
      switch (disEqcType)
      {
        case 0: // none
          antennaNr = 1;
          break;
        case 1: // Simple A
          antennaNr = 1;
          break;
        case 2: // Simple B
          antennaNr = 2;
          break;
        case 3: // Level 1 A/A
          antennaNr = 1;
          break;
        case 4: // Level 1 B/A
          antennaNr = 2;
          break;
        case 5: // Level 1 A/B
          antennaNr = 3;
          break;
        case 6: // Level 1 B/B
          antennaNr = 4;
          break;
      }
      //"01,02,03,04,05,06,07,08,09,0a,0b,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,"	
      Log.Info(
        "FireDTV SendDiseqcCommand() diseqc:{0}, antenna:{1} frequency:{2}, switching frequency:{3}, polarisation:{4}",
        disEqcType, antennaNr, frequency, switchingFrequency, polarisation);
      IntPtr ptrCmd = Marshal.AllocCoTaskMem(25);
      try
      {
        Marshal.WriteByte(ptrCmd, 0, 0xFF); //Voltage;
        Marshal.WriteByte(ptrCmd, 1, 0xFF); //ContTone;
        Marshal.WriteByte(ptrCmd, 2, 0xFF); //Burst;
        Marshal.WriteByte(ptrCmd, 3, 0x01); //NrDiseqcCmds;

        Marshal.WriteByte(ptrCmd, 4, 0x04); //diseqc command 1. length=4
        Marshal.WriteByte(ptrCmd, 5, 0xE0); //diseqc command 1. uFraming=0xe0
        Marshal.WriteByte(ptrCmd, 6, 0x10); //diseqc command 1. uAddress=0x10
        Marshal.WriteByte(ptrCmd, 7, 0x38); //diseqc command 1. uCommand=0x38

        // Antenna nr = 1-4 in this example, but this is based on application
        // Diseqc standard is 0 based, so for Diseqc 1.0 antenna index is from 0 -3
        // if your application numbers antennas from 0-3 then dont sub (-1)  

        // for the write to port group 0 command:
        // data 0 : high nibble specifices which bits are valid , 0XF means all bits are valid and should be set)
        // data 0 : low nibble specifies the values of each bit
        //				0    :  0= low band,   1 = high band
        //              1    :  0= horizontal, 1 = vertical
        //              2..3 :  antenna number (0-3)
        byte uContTone;
        if (frequency < switchingFrequency)
        {
          // We are in Low Band
          uContTone = 0;
        }
        else
        {
          // We are in High Band
          uContTone = 1;
        }
        byte cmd = 0xf0;
        cmd |= (byte) (((antennaNr - 1)*4) & 0x0F);
        cmd |= (byte) (uContTone == 1 ? 1 : 0);
        cmd |= (byte) (polarisation == 0 ? 2 : 0); //uPolarization = 0 = HOR,1 = VER
        Marshal.WriteByte(ptrCmd, 8, cmd);

        DirectShowLib.IKsPropertySet propertySet = captureFilter as DirectShowLib.IKsPropertySet;
        Guid propertyGuid = KSPROPSETID_Firesat;
        KSPropertySupport isTypeSupported = 0;
        int hr = propertySet.QuerySupported(propertyGuid, (int) KSPROPERTY_FIRESAT_LNB_CONTROL, out isTypeSupported);
        if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
        {
          Log.Error("FireDTV:SendDiseqCommand() not supported");
          return;
        }

        string txt = "";
        for (int i = 0; i < 25; ++i)
        {
          txt += String.Format("0x{0:X} ", Marshal.ReadByte(ptrCmd, i));
        }
        Log.Info("FireDTV:SendDiseq: {0}", txt);

        hr = propertySet.Set(propertyGuid, KSPROPERTY_FIRESAT_LNB_CONTROL, ptrCmd, 25, ptrCmd, 25);
        if (hr != 0)
        {
          Log.Error("FireDTV:SendDiseqCommand() not supported");
        }
        else
        {
          _prevDisEqcType = disEqcType;
          _prevFrequency = frequency;
          _prevPolarisation = polarisation;
          //System.Threading.Thread.Sleep(250);
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(ptrCmd);
      }
    }
  }
}