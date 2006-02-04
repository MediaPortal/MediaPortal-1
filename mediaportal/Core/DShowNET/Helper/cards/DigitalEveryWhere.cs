/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using MediaPortal.GUI.Library;
using DirectShowLib;

namespace DShowNET
{
  public class DigitalEverywhere : IksPropertyUtils
  {
    //"01,00,00,00,00,00,00,00,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,0a,cc,01,00,02,00,cc,cc,04,00,05,00,05,00,cc,cc,cc,cc,"	char [2048]
    [StructLayout(LayoutKind.Explicit, Size = 56), ComVisible(true)]
    struct FIRESAT_SELECT_PIDS_DVBS //also for DVBC
    {
      [FieldOffset(0)]
      public bool bCurrentTransponder;
      [FieldOffset(4)]
      public bool bFullTransponder;
      [FieldOffset(8)]
      public bool uLnb;
      [FieldOffset(12)]
      public uint uFrequency;
      [FieldOffset(16)]
      public uint uSymbolRate;
      [FieldOffset(20)]
      public byte uFecInner;
      [FieldOffset(21)]
      public byte uPolarization;
      [FieldOffset(22)]
      public byte dummy1; // 1-16
      [FieldOffset(23)]
      public byte dummy2; // 
      [FieldOffset(24)]
      public byte uNumberOfValidPids; // 1-16
      [FieldOffset(25)]
      public byte dummy3; // 
      [FieldOffset(26)]
      public ushort uPid1;
      [FieldOffset(28)]
      public ushort uPid2;
      [FieldOffset(30)]
      public ushort uPid3;
      [FieldOffset(32)]
      public ushort uPid4;
      [FieldOffset(34)]
      public ushort uPid5;
      [FieldOffset(36)]
      public ushort uPid6;
      [FieldOffset(38)]
      public ushort uPid7;
      [FieldOffset(40)]
      public ushort uPid8;
      [FieldOffset(42)]
      public ushort uPid9;
      [FieldOffset(44)]
      public ushort uPid10;
      [FieldOffset(46)]
      public ushort uPid11;
      [FieldOffset(48)]
      public ushort uPid12;
      [FieldOffset(50)]
      public ushort uPid13;
      [FieldOffset(52)]
      public ushort uPid14;
      [FieldOffset(54)]
      public ushort uPid15;
      [FieldOffset(56)]
      public ushort uPid16;
      [FieldOffset(58)]
      public ushort dummy4;
    }
    [StructLayout(LayoutKind.Explicit, Size = 56), ComVisible(true)]
    struct FIRESAT_SELECT_PIDS_DVBT
    {
      [FieldOffset(0)]
      public bool bCurrentTransponder;//Set TRUE
      [FieldOffset(4)]
      public bool bFullTransponder;   //Set FALSE when selecting PIDs
      [FieldOffset(8)]
      public uint uFrequency;    // kHz 47.000-860.000
      [FieldOffset(12)]
      public byte uBandwidth;    // BANDWIDTH_8_MHZ, BANDWIDTH_7_MHZ, BANDWIDTH_6_MHZ
      [FieldOffset(13)]
      public byte uConstellation;// CONSTELLATION_DVB_T_QPSK,CONSTELLATION_QAM_16,CONSTELLATION_QAM_64,OFDM_AUTO
      [FieldOffset(14)]
      public byte uCodeRateHP;   // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO
      [FieldOffset(15)]
      public byte uCodeRateLP;   // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO
      [FieldOffset(16)]
      public byte uGuardInterval;// GUARD_INTERVAL_1_32,GUARD_INTERVAL_1_16,GUARD_INTERVAL_1_8,GUARD_INTERVAL_1_4,OFDM_AUTO
      [FieldOffset(17)]
      public byte uTransmissionMode;// TRANSMISSION_MODE_2K, TRANSMISSION_MODE_8K, OFDM_AUTO
      [FieldOffset(18)]
      public byte uHierarchyInfo;// HIERARCHY_NONE,HIERARCHY_1,HIERARCHY_2,HIERARCHY_4,OFDM_AUTO
      [FieldOffset(19)]
      public byte dummy; // 
      [FieldOffset(20)]
      public byte uNumberOfValidPids; // 1-16
      [FieldOffset(21)]
      public byte dummy2; // 
      [FieldOffset(22)]
      public ushort uPid1;
      [FieldOffset(24)]
      public ushort uPid2;
      [FieldOffset(26)]
      public ushort uPid3;
      [FieldOffset(28)]
      public ushort uPid4;
      [FieldOffset(30)]
      public ushort uPid5;
      [FieldOffset(32)]
      public ushort uPid6;
      [FieldOffset(34)]
      public ushort uPid7;
      [FieldOffset(36)]
      public ushort uPid8;
      [FieldOffset(38)]
      public ushort uPid9;
      [FieldOffset(40)]
      public ushort uPid10;
      [FieldOffset(42)]
      public ushort uPid11;
      [FieldOffset(44)]
      public ushort uPid12;
      [FieldOffset(46)]
      public ushort uPid13;
      [FieldOffset(48)]
      public ushort uPid14;
      [FieldOffset(50)]
      public ushort uPid15;
      [FieldOffset(52)]
      public ushort uPid16;
      [FieldOffset(54)]
      public ushort dummy3;
    }
    static public readonly Guid KSPROPSETID_Firesat = new Guid(0xab132414, 0xd060, 0x11d0, 0x85, 0x83, 0x00, 0xc0, 0x4f, 0xd9, 0xba, 0xf3);
    const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C = 8;
    const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T = 6;
    const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S = 2;
    const int KSPROPERTY_FIRESAT_HOST2CA = 22;
    const int KSPROPERTY_FIRESAT_DRIVER_VERSION = 4;
    const int KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION = 11;

    public DigitalEverywhere(IBaseFilter filter)
      : base(filter)
    {
    }

    public bool IsDigitalEverywhere
    {
      get
      {
        IKsPropertySet propertySet = captureFilter as IKsPropertySet;
        if (propertySet == null) return false;
        Guid propertyGuid = KSPROPSETID_Firesat;
        uint isTypeSupported = 0;
        int hr = propertySet.QuerySupported(ref propertyGuid, KSPROPERTY_FIRESAT_HOST2CA, out isTypeSupported);
        if (hr != 0 || (isTypeSupported & (uint)KsPropertySupport.Set) == 0)
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
      //typedef struct _FIRESAT_CA_DATA{ 
      //  UCHAR uSlot;                      //0
      //  UCHAR uTag;                       //1   (2..3 = padding)
      //  BOOL bMore;                       //4   (5..7 = padding)
      //  USHORT uLength;                   //8..9
      //  UCHAR uData[MAX_PMT_SIZE];        //10....
      //}FIRESAT_CA_DATA, *PFIRESAT_CA_DATA;

      //Log.Write("FireDTV Driver version:{0} ", GetDriverVersionNumber());
      //Log.Write("FireDTV FW version:{0} ", GetFirmwareVersionNumber());
      if (PMT == null) return false;
      if (pmtLength == 0) return false;

      //Log.Write("SendPMTToFireDTV pmt:{0}", pmtLength);
      Guid propertyGuid = KSPROPSETID_Firesat;
      int propId = KSPROPERTY_FIRESAT_HOST2CA;
      IKsPropertySet propertySet = captureFilter as IKsPropertySet;
      uint isTypeSupported = 0;
      if (propertySet == null)
      {
        Log.Write("SendPMTToFireDTV() properySet=null");
        return true;
      }

      int hr = propertySet.QuerySupported(ref propertyGuid, (uint)propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & (uint)KsPropertySupport.Set) == 0)
      {
        Log.Write("SendPMTToFireDTV() SendPMT is not supported");
        return true;
      }

      int iSize = 12 + 2 + pmtLength;
      IntPtr pDataInstance = Marshal.AllocCoTaskMem(1036);
      IntPtr pDataReturned = Marshal.AllocCoTaskMem(1036);
      int offs = 0;

      //example data:0x0 0x2 0x0 0x0 0x0 0x0 0x0 0x0 0x14 0x0 0x3 0x1 | 0x2 0xB0 0x12 0x1 0x3D 0xC1 0x0 0x0 0xFF 0xFF 0xF0 0x0 0x3 0xEC 0x8C 0xF0 0x0 0xD3 
      byte[] byData = new byte[1036];
      uint uLength = (uint)(2 + pmtLength);
      byData[offs] = 0; offs++;//slot
      byData[offs] = 2; offs++;//utag

      byData[offs] = 0; offs++;//padding
      byData[offs] = 0; offs++;//padding

      byData[offs] = 0; offs++;//bmore

      byData[offs] = 0; offs++;//padding
      byData[offs] = 0; offs++;//padding
      byData[offs] = 0; offs++;//padding

      byData[offs] = (byte)(uLength % 256); offs++;		//ulength lo
      byData[offs] = (byte)(uLength / 256); offs++;		//ulength hi

      //byData[offs]= 0; offs++;
      //byData[offs]= 0; offs++;

      byData[offs] = 3; offs++;// List Management = ONLY
      byData[offs] = 1; offs++;// pmt_cmd = OK DESCRAMBLING		
      for (int i = 0; i < pmtLength; ++i)
      {
        byData[offs] = PMT[i];
        offs++;
      }
      string log = String.Format("pmt len:{0} data:", pmtLength);
      for (int i = 0; i < offs; ++i)
      {
        Marshal.WriteByte(pDataInstance, i, byData[i]);
        Marshal.WriteByte(pDataReturned, i, byData[i]);
        log += String.Format("0x{0:X} ", byData[i]);
      }

      Log.Write(log);
      hr = propertySet.RemoteSet(ref propertyGuid, (uint)propId, pDataInstance, (uint)1036, pDataReturned, (uint)1036);
      Marshal.FreeCoTaskMem(pDataReturned);
      Marshal.FreeCoTaskMem(pDataInstance);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Log, true, "FireDTV:SetPMT() failed 0x{0:X} offs:{1}", hr, offs);
        return false;
      }
      return true;
    }//public bool SendPMTToFireDTV(byte[] PMT)

    public bool SetPIDS(bool isDvbc, bool isDvbT, bool isDvbS, bool isAtsc, ArrayList pids)
    {
      string logStart = "dvbt:";
      DirectShowLib.IKsPropertySet propertySet = captureFilter as DirectShowLib.IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      KSPropertySupport isTypeSupported = 0;
      uint propertySelect = (uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T;
      if (isDvbc)
      {
        propertySelect = (uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C;
        logStart = "dvbc:";
      }
      if (isDvbS)
      {
        propertySelect = (uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S;
        logStart = "dvbs:";
      }
      int hr = propertySet.QuerySupported(propertyGuid, (int)propertySelect, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Write("SendPMTToFireDTV() SetPIDS is not supported");
        return true;
      }

      FIRESAT_SELECT_PIDS_DVBT dvbtStruct = new FIRESAT_SELECT_PIDS_DVBT();
      FIRESAT_SELECT_PIDS_DVBS dvbsStruct = new FIRESAT_SELECT_PIDS_DVBS();
      dvbtStruct.bCurrentTransponder = true;
      dvbtStruct.bFullTransponder = true;
      dvbsStruct.bCurrentTransponder = true;
      dvbsStruct.bFullTransponder = true;
      if (pids.Count > 0)
      {
        dvbtStruct.bFullTransponder = false;
        dvbtStruct.uNumberOfValidPids = (byte)pids.Count;
        dvbsStruct.bFullTransponder = false;
        dvbsStruct.uNumberOfValidPids = (byte)pids.Count;
        if (pids.Count >= 1) { dvbtStruct.uPid1 = (ushort)pids[0]; dvbsStruct.uPid1 = (ushort)pids[0]; }
        if (pids.Count >= 2) { dvbtStruct.uPid2 = (ushort)pids[1]; dvbsStruct.uPid2 = (ushort)pids[1]; }
        if (pids.Count >= 3) { dvbtStruct.uPid3 = (ushort)pids[2]; dvbsStruct.uPid3 = (ushort)pids[2]; }
        if (pids.Count >= 4) { dvbtStruct.uPid4 = (ushort)pids[3]; dvbsStruct.uPid4 = (ushort)pids[3]; }
        if (pids.Count >= 5) { dvbtStruct.uPid5 = (ushort)pids[4]; dvbsStruct.uPid5 = (ushort)pids[4]; }
        if (pids.Count >= 6) { dvbtStruct.uPid6 = (ushort)pids[5]; dvbsStruct.uPid6 = (ushort)pids[5]; }
        if (pids.Count >= 7) { dvbtStruct.uPid7 = (ushort)pids[6]; dvbsStruct.uPid7 = (ushort)pids[6]; }
        if (pids.Count >= 8) { dvbtStruct.uPid8 = (ushort)pids[7]; dvbsStruct.uPid8 = (ushort)pids[7]; }
        if (pids.Count >= 9) { dvbtStruct.uPid9 = (ushort)pids[8]; dvbsStruct.uPid9 = (ushort)pids[8]; }
        if (pids.Count >= 10) { dvbtStruct.uPid10 = (ushort)pids[9]; dvbsStruct.uPid10 = (ushort)pids[9]; }
        if (pids.Count >= 11) { dvbtStruct.uPid11 = (ushort)pids[10]; dvbsStruct.uPid11 = (ushort)pids[10]; }
        if (pids.Count >= 12) { dvbtStruct.uPid12 = (ushort)pids[11]; dvbsStruct.uPid12 = (ushort)pids[11]; }
        if (pids.Count >= 13) { dvbtStruct.uPid13 = (ushort)pids[12]; dvbsStruct.uPid13 = (ushort)pids[12]; }
        if (pids.Count >= 14) { dvbtStruct.uPid14 = (ushort)pids[13]; dvbsStruct.uPid14 = (ushort)pids[13]; }
        if (pids.Count >= 15) { dvbtStruct.uPid15 = (ushort)pids[14]; dvbsStruct.uPid15 = (ushort)pids[14]; }
        if (pids.Count >= 16) { dvbtStruct.uPid16 = (ushort)pids[15]; dvbsStruct.uPid16 = (ushort)pids[15]; }
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

      Log.WriteFile(Log.LogType.Log, true, "FireDTV:SetPIDS() count:{0} len:{1}", pids.Count, len);

      string txt = "";
      for (int i = 0; i < len; ++i)
        txt += String.Format("0x{0:X} ", Marshal.ReadByte(pDataInstance, i));

      Log.Write("pid {0} data:{1}", logStart, txt);
      hr = propertySet.Set(propertyGuid,
                          (int)propertySelect,
                          pDataInstance, (int)len,
                          pDataReturned, (int)len);
      Marshal.FreeCoTaskMem(pDataReturned);
      Marshal.FreeCoTaskMem(pDataInstance);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Log, true, "FireDTV:SetPIDS() failed 0x{0:X}", hr);
        return false;
      }

      return true;
    }

    public string GetFirmwareVersionNumber()
    {
      DirectShowLib.IKsPropertySet propertySet = captureFilter as DirectShowLib.IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      KSPropertySupport isTypeSupported = 0;
      int hr = propertySet.QuerySupported(propertyGuid, (int)KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
      {
        Log.WriteFile(Log.LogType.Log, true, "FireDTV:GetDriverVersion() not supported");
        return String.Empty;
      }
      int byteCount = 0;
      IntPtr pDataInstance = Marshal.AllocHGlobal(100);
      IntPtr pDataReturned = Marshal.AllocHGlobal(100);
      hr = propertySet.Get(propertyGuid,
                                  (int)KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION,
                                  pDataInstance, (int)100,
                                  pDataReturned, (int)100, out byteCount);
      Marshal.FreeHGlobal(pDataReturned);
      Marshal.FreeHGlobal(pDataInstance);

      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Log, true, "FireDTV:GetFirmwareVersionNumber() failed 0x{0:X}", hr);
        return String.Empty;
      }
      Log.Write("count:{0}", byteCount);

      string version = String.Empty;
      for (int i = 0; i < byteCount; ++i)
      {
        char ch;
        byte k = Marshal.ReadByte(pDataReturned, i);

        Log.Write("{0} = 0x{1:X} = {2} = {3}",
                i, k, k, (char)k);
        if (k < 0x20)
          ch = '.';
        else
          ch = (char)k;
        version += ch;
      }
      return version;
    }

    public string GetDriverVersionNumber()
    {
      DirectShowLib.IKsPropertySet propertySet = captureFilter as DirectShowLib.IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      KSPropertySupport isTypeSupported = 0;
      int hr = propertySet.QuerySupported(propertyGuid, (int)KSPROPERTY_FIRESAT_DRIVER_VERSION, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
      {
        Log.WriteFile(Log.LogType.Log, true, "FireDTV:GetDriverVersion() not supported");
        return String.Empty;
      }
      int byteCount = 0;
      IntPtr pDataInstance = Marshal.AllocHGlobal(22);
      IntPtr pDataReturned = Marshal.AllocHGlobal(22);
      hr = propertySet.Get(propertyGuid,
                                  (int)KSPROPERTY_FIRESAT_DRIVER_VERSION,
                                  pDataInstance, (int)22,
                                  pDataReturned, (int)22, out byteCount);
      Marshal.FreeHGlobal(pDataReturned);
      Marshal.FreeHGlobal(pDataInstance);

      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Log, true, "FireDTV:GetDriverVersion() failed 0x{0:X}", hr);
        return String.Empty;
      }
      Log.Write("count:{0}", byteCount);

      string version = String.Empty;

      for (int i = 0; i < byteCount; ++i)
      {
        char ch;
        byte k = Marshal.ReadByte(pDataReturned, i);

        Log.Write("{0} = 0x{1:X} = {2} = {3}",
                i, k, k, (char)k);
        if (k < 0x20)
          ch = '.';
        else
          ch = (char)k;
        version += ch;
      }
      return version;
    }
  }
}
