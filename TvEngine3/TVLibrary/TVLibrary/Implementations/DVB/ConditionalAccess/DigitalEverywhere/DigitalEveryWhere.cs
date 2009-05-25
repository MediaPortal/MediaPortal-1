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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Handles the CI/CAM interface for FireDtv and FloppyDtv devices from 
  /// Digital Everywhere
  /// </summary>
  public class DigitalEverywhere : IDiSEqCController
  {
    #region structs
    [StructLayout(LayoutKind.Explicit, Size = 60), ComVisible(true)]
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
    #endregion

    /// <summary>
    /// FireDtv guid
    /// </summary>
    static public readonly Guid KSPROPSETID_Firesat = new Guid(0xab132414, 0xd060, 0x11d0, 0x85, 0x83, 0x00, 0xc0, 0x4f, 0xd9, 0xba, 0xf3);
    #region property ids
#pragma warning disable 169
    const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C = 8;
    const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T = 6;
    const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S = 2;
    const int KSPROPERTY_FIRESAT_HOST2CA = 22;
    const int KSPROPERTY_FIRESAT_DRIVER_VERSION = 4;
    const int KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION = 11;
    const int KSPROPERTY_FIRESAT_GET_CI_STATUS = 28;
    const int KSPROPERTY_FIRESAT_LNB_CONTROL = 12;
    #endregion

    #region CI STATUS bits
    const int CI_MMI_REQUEST = 0x0100;
    const int CI_PMT_REPLY = 0x0080;
    const int CI_DATE_TIME_REQEST = 0x0040;
    const int CI_APP_INFO_AVAILABLE = 0x0020;
    const int CI_MODULE_PRESENT = 0x0010;
    const int CI_MODULE_IS_DVB = 0x0008;
    const int CI_MODULE_ERROR = 0x0004;
    const int CI_MODULE_INIT_READY = 0x0002;
    const int CI_ERR_MSG_AVAILABLE = 0x0001;
#pragma warning restore 169
    #endregion

    #region variables

    readonly bool _isDigitalEverywhere;
    readonly bool _hasCAM;
    readonly bool _isInitialized;
    readonly IBaseFilter _filterTuner;

    readonly IntPtr _ptrDataInstance;
    readonly IntPtr _ptrDataReturned;

    DVBSChannel _previousChannel;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DigitalEverywhere"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public DigitalEverywhere(IBaseFilter tunerFilter)
    {
      _previousChannel = null;
      _filterTuner = tunerFilter;
      _hasCAM = false;
      _isInitialized = false;
      _isDigitalEverywhere = false;

      _ptrDataInstance = Marshal.AllocCoTaskMem(1036);
      _ptrDataReturned = Marshal.AllocCoTaskMem(1036);
      if (_filterTuner != null)
      {
        _isDigitalEverywhere = IsDigitalEverywhere;
        if (_isDigitalEverywhere)
        {
          _hasCAM = IsCamPresent();
          Log.Log.WriteFile("FireDTV detected CAM:{0} ", _hasCAM);
          Log.Log.WriteFile("FireDTV Driver version:{0} ", GetDriverVersionNumber());
          Log.Log.WriteFile("FireDTV {0} ", GetHardwareFirmwareVersionNumber());
        }
      }
      _isInitialized = true;

    }

    /// <summary>
    /// Returns if the card is a FireDtv/Floppy DTV device or not.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is a FireDtv/Floppy DtV; otherwise, <c>false</c>.
    /// </value>
    public bool IsDigitalEverywhere
    {
      get
      {
        if (_isInitialized)
          return _isDigitalEverywhere;

        IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
        if (propertySet == null)
          return false;
        Guid propertyGuid = KSPROPSETID_Firesat;
        KSPropertySupport isTypeSupported;
        int hr = propertySet.QuerySupported(propertyGuid, KSPROPERTY_FIRESAT_HOST2CA, out isTypeSupported);
        if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
        {
          return false;
        }
        return true;
      }
    }

    /// <summary>
    /// This function sends the PMT (Program Map Table) to the FireDTV DVB-T/DVB-C/DVB-S card
    /// This allows the integrated CI and CAM module inside the FireDTv device to decrypt the current TV channel
    /// (provided that offcourse a smartcard with the correct subscription and its inserted in the CAM)
    /// </summary>
    /// <param name="PMT">Program Map Table received from digital transport stream</param>
    /// <param name="pmtLength">length in bytes of PMT</param>
    /// <param name="current">The current channel index</param>
    /// <param name="max">The max. channel index</param>
    /// <returns></returns>
    /// <remarks>
    /// 1. first byte in PMT is 0x02=tableId for PMT
    /// 2. This function is vender specific. It will only work on the FireDTV devices
    /// </remarks>
    /// <preconditions>
    /// 1. FireDTV device should be tuned to a digital DVB-C/S/T TV channel
    /// 2. PMT should have been received
    /// </preconditions>
    private bool SendPMTToFireDTV(byte[] PMT, int pmtLength, int current, int max)
    {

      //typedef struct _FIRESAT_CA_DATA{ 
      //  UCHAR uSlot;                      //0
      //  UCHAR uTag;                       //1   (2..3 = padding)
      //  BOOL bMore;                       //4   (5..7 = padding)
      //  USHORT uLength;                   //8..9
      //  UCHAR uData[MAX_PMT_SIZE];        //10....
      //}FIRESAT_CA_DATA, *PFIRESAT_CA_DATA;

      if (!_hasCAM)
      {
        return true;
      }
      if (PMT == null)
      {
        return false;
      }
      if (pmtLength == 0)
      {
        return false;
      }

      Log.Log.WriteFile("SendPMTToFireDTV pmtLength:{0}", pmtLength);
      Guid propertyGuid = KSPROPSETID_Firesat;
      const int propId = KSPROPERTY_FIRESAT_HOST2CA;
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      KSPropertySupport isTypeSupported;
      if (propertySet == null)
      {
        Log.Log.WriteFile("FireDTV:SendPmt() properySet=null");
        return true;
      }

      int hr = propertySet.QuerySupported(propertyGuid, propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Log.WriteFile("FireDTV:SendPmt() not supported");
        return true;
      }

      byte[] byData = new byte[1036];
      uint uLength = (uint)(2 + pmtLength); //bytes 0-1 contain the length of pmt
      byData[0] = 0;//slot       0
      byData[1] = 2;//utag       1
      byData[2] = 0;//padding    2
      byData[3] = 0;//padding    3
      byData[4] = 0;//bmore      4
      byData[5] = 0;//padding    5
      byData[6] = 0;//padding    6
      byData[7] = 0;//padding    7
      byData[8] = (byte)(uLength % 256);		//ulength lo    8..9
      byData[9] = (byte)(uLength / 256);		//ulength hi
      if (current == 0 && max == 1)
        byData[10] = 3;     // 10     List Management = ONLY (only=3, first=1, more=0, last=2)
      else if (current == 0 && max > 1)
        byData[10] = 1;     // 10     List Management = ONLY (only=3, first=1, more=0, last=2)
      else if (current > 0 && current < max - 1)
        byData[10] = 0;     // 10     List Management = ONLY (only=3, first=1, more=0, last=2)
      else if (current == max - 1)
        byData[10] = 2;     // 10

      byData[11] = 1;     // 11     pmt_cmd = OK DESCRAMBLING		
      for (int i = 0; i < pmtLength; ++i)
      {
        byData[i + 12] = PMT[i];
      }

      string log = String.Format("FireDTV: #{0}/{1} pmt data:", current, max);
      for (int i = 0; i < 1036; ++i)
      {
        Marshal.WriteByte(_ptrDataInstance, i, byData[i]);
        Marshal.WriteByte(_ptrDataReturned, i, byData[i]);
        log += String.Format("0x{0:X} ", byData[i]);
      }

      Log.Log.WriteFile(log);
      hr = propertySet.Set(propertyGuid, propId, _ptrDataInstance, 1036, _ptrDataReturned, 1036);

      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV:  failed 0x{0:X}", hr);
        ResetCAM();
        return false;
      }
      return true;
    }

    /// <summary>
    /// Resets the CAM.
    /// </summary>
    public void ResetCAM()
    {
      Log.Log.WriteFile("FireDTV:ResetCAM()");
      Guid propertyGuid = KSPROPSETID_Firesat;
      const int propId = KSPROPERTY_FIRESAT_HOST2CA;
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      KSPropertySupport isTypeSupported;
      if (propertySet == null)
      {
        Log.Log.WriteFile("FireDTV:ResetCAM() properySet=null");
        return;
      }

      int hr = propertySet.QuerySupported(propertyGuid, (int)propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Log.WriteFile("FireDTV:ResetCAM() Reset CI is not supported");
        return;
      }
      try
      {
        byte[] byData = new byte[1036];
        byData[0] = 0; //slot
        byData[1] = 0; //utag (CA RESET)
        byData[2] = 0; //padding
        byData[3] = 0; //padding
        byData[4] = 0; //bmore (FALSE)
        byData[5] = 0; //padding
        byData[6] = 0; //padding
        byData[7] = 0; //padding
        byData[8] = 1; 		//ulength lo
        byData[9] = 0; 		//ulength hi
        byData[10] = 0; // HW Reset of CI part
        for (int i = 0; i < 1036; ++i)
        {
          Marshal.WriteByte(_ptrDataInstance, i, byData[i]);
          Marshal.WriteByte(_ptrDataReturned, i, byData[i]);
        }

        //Log.Log.WriteFile(log);
        hr = propertySet.Set(propertyGuid, propId, _ptrDataInstance, 1036, _ptrDataReturned, 1036);

        if (hr != 0)
        {
          Log.Log.WriteFile("FireDTV:ResetCAM() failed 0x{0:X}", hr);
          return;
        }
      } finally
      {
        Log.Log.WriteFile("FireDTV:ResetCAM() cam has been reset");
      }
      return;

    }

    /// <summary>
    /// Sends the PMT of all subchannels to fire DTV.
    /// </summary>
    /// <param name="subChannels">The sub channels.</param>
    /// <returns></returns>
    public bool SendPMTToFireDTV(Dictionary<int, ConditionalAccessContext> subChannels)
    {
      if(!_hasCAM)
      {
        return true;
      }
      List<ConditionalAccessContext> filteredChannels = new List<ConditionalAccessContext>();
      bool succeeded = true;
      Dictionary<int, ConditionalAccessContext>.Enumerator en = subChannels.GetEnumerator();
      while (en.MoveNext())
      {
        bool exists = false;
        ConditionalAccessContext context = en.Current.Value;
        foreach (ConditionalAccessContext c in filteredChannels)
        {
          if (c.Channel.Equals(context.Channel))
            exists = true;
        }
        if (!exists)
        {
          filteredChannels.Add(context);
        }
      }

      int count = 0;
      foreach (ConditionalAccessContext context in filteredChannels)
      {
        bool result = SendPMTToFireDTV(context.PMT, context.PMTLength, count, filteredChannels.Count);
        count++;
        if (!result)
          succeeded = false;
      }
      return succeeded;
    }

    /// <summary>
    /// Sets the pids for hardware pid filtering.
    /// </summary>
    /// <param name="isDvbc">if set to <c>true</c> [is DVB-C].</param>
    /// <param name="isDvbT">if set to <c>true</c> [is DVB-T].</param>
    /// <param name="isDvbS">if set to <c>true</c> [is DVB-S].</param>
    /// <param name="isAtsc">if set to <c>true</c> [is atsc].</param>
    /// <param name="pids">The pids to filter</param>
    /// <returns></returns>
    public bool SetHardwarePidFiltering(bool isDvbc, bool isDvbT, bool isDvbS, bool isAtsc, List<ushort> pids)
    {
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      uint propertySelect = KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T;
      //if (isDvbc)
      //{
      //  propertySelect = (uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C;
      //  logStart = "dvbc:";
      //}
      if (isDvbc || isDvbS)
      {
        propertySelect = KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S;
      }
      if (propertySet != null)
      {
        KSPropertySupport isTypeSupported;
        int hr = propertySet.QuerySupported(propertyGuid, (int)propertySelect, out isTypeSupported);
        if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
        {
          Log.Log.WriteFile("FireDTV: Set H/W pid filtering is not supported");
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
            pidCount = 16;
          //get only specific pids
          dvbtStruct.uNumberOfValidPids = (byte)pidCount;
          dvbsStruct.uNumberOfValidPids = (byte)pidCount;
          if (pids.Count >= 1) { dvbtStruct.uPid1 = pids[0]; dvbsStruct.uPid1 = pids[0]; }
          if (pids.Count >= 2) { dvbtStruct.uPid2 = pids[1]; dvbsStruct.uPid2 = pids[1]; }
          if (pids.Count >= 3) { dvbtStruct.uPid3 = pids[2]; dvbsStruct.uPid3 = pids[2]; }
          if (pids.Count >= 4) { dvbtStruct.uPid4 = pids[3]; dvbsStruct.uPid4 = pids[3]; }
          if (pids.Count >= 5) { dvbtStruct.uPid5 = pids[4]; dvbsStruct.uPid5 = pids[4]; }
          if (pids.Count >= 6) { dvbtStruct.uPid6 = pids[5]; dvbsStruct.uPid6 = pids[5]; }
          if (pids.Count >= 7) { dvbtStruct.uPid7 = pids[6]; dvbsStruct.uPid7 = pids[6]; }
          if (pids.Count >= 8) { dvbtStruct.uPid8 = pids[7]; dvbsStruct.uPid8 = pids[7]; }
          if (pids.Count >= 9) { dvbtStruct.uPid9 = pids[8]; dvbsStruct.uPid9 = pids[8]; }
          if (pids.Count >= 10) { dvbtStruct.uPid10 = pids[9]; dvbsStruct.uPid10 = pids[9]; }
          if (pids.Count >= 11) { dvbtStruct.uPid11 = pids[10]; dvbsStruct.uPid11 = pids[10]; }
          if (pids.Count >= 12) { dvbtStruct.uPid12 = pids[11]; dvbsStruct.uPid12 = pids[11]; }
          if (pids.Count >= 13) { dvbtStruct.uPid13 = pids[12]; dvbsStruct.uPid13 = pids[12]; }
          if (pids.Count >= 14) { dvbtStruct.uPid14 = pids[13]; dvbsStruct.uPid14 = pids[13]; }
          if (pids.Count >= 15) { dvbtStruct.uPid15 = pids[14]; dvbsStruct.uPid15 = pids[14]; }
          if (pids.Count >= 16) { dvbtStruct.uPid16 = pids[15]; dvbsStruct.uPid16 = pids[15]; }
        }
        else
        {
          //get entire stream
          dvbtStruct.bFullTransponder = true;
          dvbsStruct.bFullTransponder = true;
          dvbtStruct.uNumberOfValidPids = 0;
          dvbsStruct.uNumberOfValidPids = 0;
        }

        int len;
        if (isDvbT)
        {
          len = Marshal.SizeOf(dvbtStruct);
          Marshal.StructureToPtr(dvbtStruct, _ptrDataInstance, true);
          Marshal.StructureToPtr(dvbtStruct, _ptrDataReturned, true);
        }
        else
        {
          len = Marshal.SizeOf(dvbsStruct);
          Marshal.StructureToPtr(dvbsStruct, _ptrDataInstance, true);
          Marshal.StructureToPtr(dvbsStruct, _ptrDataReturned, true);
        }

        //      Log.Log.WriteFile("FireDTV: Set H/W pid filtering count:{0} len:{1}", pids.Count, len);

        //      string txt = "";
        //      for (int i = 0; i < len; ++i)
        //        txt += String.Format("0x{0:X} ", Marshal.ReadByte(_ptrDataInstance, i));

        //      Log.Log.WriteFile("FireDTV: Set H/W pid filtering pid {0} data:{1}", logStart, txt);
        hr = propertySet.Set(propertyGuid,
                             (int)propertySelect,
                             _ptrDataInstance, len,
                             _ptrDataReturned, len);

        if (hr != 0)
        {
          Log.Log.WriteFile("FireDTV: Set H/W pid filtering failed 0x{0:X}", hr);
          return false;
        }
      }

      return true;
    }

    /// <summary>
    ///  Get the hardware and firmware versions
    /// </summary>
    /// <returns></returns>
    public string GetHardwareFirmwareVersionNumber()
    {
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      string version = String.Empty;
      if (propertySet != null)
      {
        KSPropertySupport isTypeSupported;
        int hr = propertySet.QuerySupported(propertyGuid, KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION, out isTypeSupported);
        if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
        {
          Log.Log.WriteFile("FireDTV:GetFirmwareVersion() not supported");
          return String.Empty;
        }
        int byteCount;
        hr = propertySet.Get(propertyGuid,
                             KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION,
                             _ptrDataInstance, 22,
                             _ptrDataReturned, 22, out byteCount);


        if (hr != 0)
        {
          Log.Log.WriteFile("FireDTV:GetFirmwareVersion() failed 0x{0:X}", hr);
          return String.Empty;
        }

        byte[] k = { 0, 0, 0, 0, 0, 0 };
        Marshal.Copy(_ptrDataReturned, k, 0, 6);

        // HW in first 3 bytes of returned data ( 8 = 3bytes of 2 chars and 2 separators )
        string hwrev = BitConverter.ToString(k).Replace("-", ".").Substring(0, 8);

        // SW firmware 3 bytes of returned data ( 8 = 3bytes of 2 chars and 2 separators )
        string fwrev = BitConverter.ToString(k).Replace("-", ".").Substring(9, 8);

        // SW firmware build in next 2 bytes
        string fwbuild = (((int)Marshal.ReadByte(_ptrDataReturned, 6) * 256) + Marshal.ReadByte(_ptrDataReturned, 7)).ToString();

        version = String.Format("HW: {0}, FW: {1} build {2}", hwrev, fwrev, fwbuild);
      }
      return version;
    }

    /// <summary>
    /// Gets the driver version number.
    /// </summary>
    /// <returns></returns>
    public string GetDriverVersionNumber()
    {
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      string version = String.Empty;
      if (propertySet != null)
      {
        KSPropertySupport isTypeSupported;
        int hr = propertySet.QuerySupported(propertyGuid, KSPROPERTY_FIRESAT_DRIVER_VERSION, out isTypeSupported);
        if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
        {
          Log.Log.WriteFile("FireDTV:GetDriverVersion() not supported");
          return String.Empty;
        }
        int byteCount;
        hr = propertySet.Get(propertyGuid,
                             KSPROPERTY_FIRESAT_DRIVER_VERSION,
                             _ptrDataInstance, 22,
                             _ptrDataReturned, 22, out byteCount);


        if (hr != 0)
        {
          Log.Log.WriteFile("FireDTV:GetDriverVersion() failed 0x{0:X}", hr);
          return String.Empty;
        }
        //      Log.Log.WriteFile("count:{0}", byteCount);


        for (int i = 0; i < byteCount; ++i)
        {
          char ch;
          byte k = Marshal.ReadByte(_ptrDataReturned, i);

          // Log.Log.WriteFile("{0} = 0x{1:X} = {2} = {3}",
          //         i, k, k, (char)k);
          if (k < 0x20)
            ch = '.';
          else
            ch = (char)k;
          version += ch;
        }
      }
      return version;
    }

    /// <summary>
    /// Gets the CAM status.
    /// </summary>
    /// <returns></returns>
    int GetCAMStatus()
    {
      Guid propertyGuid = KSPROPSETID_Firesat;
      const int propId = KSPROPERTY_FIRESAT_GET_CI_STATUS;
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      KSPropertySupport isTypeSupported;
      if (propertySet == null)
      {
        Log.Log.WriteFile("FireDTV:GetCAMStatus() properySet=null");
        return 0;
      }

      int hr = propertySet.QuerySupported(propertyGuid, propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
      {
        Log.Log.WriteFile("FireDTV:GetCAMStatus() get is not supported");
        return 0;
      }
      try
      {
        int bytesReturned;
        hr = propertySet.Get(propertyGuid, propId, _ptrDataInstance, 1036, _ptrDataReturned, 1036, out bytesReturned);
        if (hr != 0)
        {
          Log.Log.WriteFile("FireDTV:GetCAMStatus() failed 0x{0:X}", hr);
          if (((uint)hr) == (0x8007001F))
          {
            ResetCAM();
            hr = propertySet.Get(propertyGuid, propId, _ptrDataInstance, 1036, _ptrDataReturned, 1036, out bytesReturned);
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
        ushort camStatus = (ushort)Marshal.ReadInt16(_ptrDataReturned, 0);

        Log.Log.Debug("FireDTV:GetCAMStatus() status is <{0}>", camStatus);
        return camStatus;
      }
      finally
      {
        Log.Log.WriteFile("FireDTV:GetCAMStatus() finished");
      }
    }

    /// <summary>
    /// Determines whether a cam is present
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if cam is present; otherwise, <c>false</c>.
    /// </returns>
    private bool IsCamPresent()
    {
      if (_isInitialized)
        return _hasCAM;
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

    /// <summary>
    /// Determines whether cam is ready
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if cam is ready; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCamReady()
    {
      int camStatus = GetCAMStatus();
      if ((camStatus & CI_MODULE_PRESENT) != 0)
      {
        //CAM is inserted
        Log.Log.WriteFile("  FireDTV:cam is inserted");
        if ((camStatus & CI_MODULE_IS_DVB) != 0)
        {
          //CAM is DVB CAM 
          Log.Log.WriteFile("  FireDTV:cam is valid");
          if ((camStatus & CI_MODULE_ERROR) != 0)
          {
            //CAM has an error
            Log.Log.WriteFile("  FireDTV:cam has error");
            return false;
          }
          if ((camStatus & CI_MODULE_INIT_READY) != 0)
          {
            //CAM is initialized
            Log.Log.WriteFile("  FireDTV:cam is ready");
          }
          else
          {
            Log.Log.WriteFile("  FireDTV:cam is NOT ready");
            return false;
          }
          if ((camStatus & CI_APP_INFO_AVAILABLE) != 0)
          {
            Log.Log.WriteFile("  FireDTV:cam is able to descramble");
          }
          else
          {
            Log.Log.WriteFile("  FireDTV:cam is UNABLE to descramble");
            return false;
          }
          return true;
        }
        Log.Log.WriteFile("  FireDTV:cam is NOT valid");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Sends the diseqc command.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The scanparameters.</param>
    public void SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      if (_previousChannel != null)
      {
        if (_previousChannel.Frequency == channel.Frequency &&
            _previousChannel.DisEqc == channel.DisEqc &&
            _previousChannel.Polarisation == channel.Polarisation &&
            _previousChannel.Pilot == channel.Pilot &&
            _previousChannel.Rolloff == channel.Rolloff &&
            _previousChannel.InnerFecRate == channel.InnerFecRate)
        {
      _previousChannel = channel;
          Log.Log.WriteFile("FireDTV: already tuned to diseqc:{0}, frequency:{1}, polarisation:{2}",
              channel.DisEqc, channel.Frequency, channel.Polarisation);
          return;
        }
        if (_previousChannel.DisEqc == DisEqcType.None && channel.DisEqc == DisEqcType.None)
        {
          _previousChannel = channel;
          Log.Log.WriteFile("FireDTV: already no diseqc used",
              channel.DisEqc, channel.Frequency, channel.Polarisation);
          return;
        }
      }
      if (_previousChannel == null && channel.DisEqc == DisEqcType.None)
      {
        _previousChannel = channel;
        Log.Log.WriteFile("FireDTV: diseqc isn't used - skip it",
            channel.DisEqc, channel.Frequency, channel.Polarisation);
        return;
      }
      _previousChannel = channel;
      int antennaNr = BandTypeConverter.GetAntennaNr(channel);

      //"01,02,03,04,05,06,07,08,09,0a,0b,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,"	

      Marshal.WriteByte(_ptrDataInstance, 0, 0xFF);//Voltage;
      Marshal.WriteByte(_ptrDataInstance, 1, 0xFF);//ContTone;
      Marshal.WriteByte(_ptrDataInstance, 2, 0xFF);//Burst;
      Marshal.WriteByte(_ptrDataInstance, 3, 0x01);//NrDiseqcCmds;

      Marshal.WriteByte(_ptrDataInstance, 4, 0x04);//diseqc command 1. length=4
      Marshal.WriteByte(_ptrDataInstance, 5, 0xE0);//diseqc command 1. uFraming=0xe0
      Marshal.WriteByte(_ptrDataInstance, 6, 0x10);//diseqc command 1. uAddress=0x10
      Marshal.WriteByte(_ptrDataInstance, 7, 0x38);//diseqc command 1. uCommand=0x38


      //bit 0	(1)	: 0=low band, 1 = hi band
      //bit 1 (2) : 0=vertical, 1 = horizontal
      //bit 3 (4) : 0=satellite position A, 1=satellite position B
      //bit 4 (8) : 0=switch option A, 1=switch option  B
      // LNB    option  position
      // 1        A         A
      // 2        A         B
      // 3        B         A
      // 4        B         B
      bool hiBand = BandTypeConverter.IsHiBand(channel, parameters);
      Log.Log.WriteFile("FireDTV SendDiseqcCommand() diseqc:{0}, antenna:{1} frequency:{2},  polarisation:{3} hiband:{4}",
              channel.DisEqc, antennaNr, channel.Frequency, channel.Polarisation, hiBand);

      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) || (channel.Polarisation == Polarisation.CircularL));
      byte cmd = 0xf0;
      cmd |= (byte)(hiBand ? 1 : 0);
      cmd |= (byte)((isHorizontal) ? 2 : 0);
      cmd |= (byte)((antennaNr - 1) << 2);
      Marshal.WriteByte(_ptrDataInstance, 8, cmd);

      Guid propertyGuid = KSPROPSETID_Firesat;
      const int propId = KSPROPERTY_FIRESAT_LNB_CONTROL;
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      KSPropertySupport isTypeSupported;
      if (propertySet == null)
      {
        Log.Log.WriteFile("FireDTV:SendDiseqcCommand() properySet=null");
        return;
      }

      int hr = propertySet.QuerySupported(propertyGuid, propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Log.WriteFile("FireDTV:SendDiseqcCommand() set is not supported {0:X} {1}", hr, (int)isTypeSupported);
        return;
      }

      string txt = "";
      for (int i = 0; i < 10; ++i)
        txt += String.Format("0x{0:X} ", Marshal.ReadByte(_ptrDataInstance, i));
      Log.Log.WriteFile("FireDTV:SendDiseq: {0}", txt);

      hr = propertySet.Set(propertyGuid, propId, _ptrDataInstance, 25, _ptrDataInstance, 25);
      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV:SendDiseqcCommand() failed:{0:X}", hr);
      }
    }

    #region IDiSEqCController Members

    /// <summary>
    /// Sends the DiSEqC command.
    /// </summary>
    /// <param name="diSEqC">The DiSEqC command.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool SendDiSEqCCommand(byte[] diSEqC)
    {
      Marshal.WriteByte(_ptrDataInstance, 0, 0xFF);//Voltage;
      Marshal.WriteByte(_ptrDataInstance, 1, 0xFF);//ContTone;
      Marshal.WriteByte(_ptrDataInstance, 2, 0xFF);//Burst;
      Marshal.WriteByte(_ptrDataInstance, 3, 0x01);//NrDiseqcCmds;
      Marshal.WriteByte(_ptrDataInstance, 4, (byte)diSEqC.Length);
      for (int i = 0; i < diSEqC.Length; ++i)
        Marshal.WriteByte(_ptrDataInstance, 5 + i, diSEqC[i]);


      Guid propertyGuid = KSPROPSETID_Firesat;
      const int propId = KSPROPERTY_FIRESAT_LNB_CONTROL;
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      KSPropertySupport isTypeSupported;
      if (propertySet == null)
      {
        Log.Log.WriteFile("FireDTV:SendDiseqcCommand() properySet=null");
        return false;
      }

      int hr = propertySet.QuerySupported(propertyGuid, propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Log.WriteFile("FireDTV:SendDiseqcCommand() set is not supported {0:X} {1}", hr, (int)isTypeSupported);
        return false;
      }

      hr = propertySet.Set(propertyGuid, propId, _ptrDataInstance, 25, _ptrDataInstance, 25);
      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV:SendDiseqcCommand() failed:{0:X}", hr);
        return false;
      }
      return true;
    }

    /// <summary>
    /// gets the diseqc reply
    /// </summary>
    /// <param name="reply">The reply.</param>
    /// <returns>true if succeeded, otherwise false</returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      reply = new byte[1];
      return false;
    }

    #endregion

    /// <summary>
    /// Set parameter to null when stopping the Graph.
    /// </summary>
    public void OnStopGraph()
    {
      _previousChannel = null;
    }
  }
}
