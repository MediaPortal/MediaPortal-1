#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using System.Text;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Handles the CI/CAM interface for FireDtv and FloppyDtv devices from 
  /// Digital Everywhere
  /// </summary>
  public class DigitalEverywhere : IDiSEqCController, ICiMenuActions, IDisposable
  {
    private const int MAX_PMT_SIZE = 1024;
    private const int CA_DATA_SIZE = 1036;
    private const int INFO_DATA_SIZE = 64;

    #region structs

    [StructLayout(LayoutKind.Explicit, Size = 60), ComVisible(true)]
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

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct FIRESAT_CA_DATA //  CA_DATA_SIZE
    {
      public byte uSlot; //     1
      public byte uTag; //     2
      public byte bMoreSpacer1; //     3
      public byte bMoreSpacer2; //     4
      public byte bMoreSpacer3; //     5
      public byte bMore; //     6
      public byte uLengthSpacer1; //     7
      public byte uLengthSpacer2; //     8
      public byte uLength1; //     9
      public byte uLength2; //    10
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PMT_SIZE)] public byte[] uData; //  1034
      public byte uDataSpacer1; //  1035
      public byte uDataSpacer2; //  1036
    } ;

    #endregion

    #region helper

    private static FIRESAT_CA_DATA GET_FIRESAT_CA_DATA(byte tag, ushort length)
    {
      FIRESAT_CA_DATA CA = new FIRESAT_CA_DATA
                             {
                               uSlot = 0,
                               //reserved for future implementations with multiple CI slots
                               uTag = tag,
                               bMoreSpacer1 = 0,
                               bMoreSpacer2 = 0,
                               bMoreSpacer3 = 0,
                               bMore = 0,
                               //don�t care; set by driver
                               uLengthSpacer1 = 0,
                               uLengthSpacer2 = 0,
                               uLength1 = ((byte)(length % 256)),
                               uLength2 = ((byte)(length / 256)),
                               uData = new byte[MAX_PMT_SIZE],
                               uDataSpacer1 = 0,
                               uDataSpacer2 = 0
                             };
      return CA;
    }

    #endregion

    /// <summary>
    /// FireDtv guid
    /// </summary>
    public static readonly Guid KSPROPSETID_Firesat = new Guid(0xab132414, 0xd060, 0x11d0, 0x85, 0x83, 0x00, 0xc0, 0x4f,
                                                               0xd9, 0xba, 0xf3);

    #region property ids

#pragma warning disable 169
    private const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C = 8;
    private const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T = 6;
    private const int KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S = 2;
    private const int KSPROPERTY_FIRESAT_HOST2CA = 22;
    private const int KSPROPERTY_FIRESAT_CA2HOST = 23;
    private const int KSPROPERTY_FIRESAT_DRIVER_VERSION = 4;
    private const int KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION = 11;
    private const int KSPROPERTY_FIRESAT_GET_CI_STATUS = 28;
    private const int KSPROPERTY_FIRESAT_LNB_CONTROL = 12;

    #endregion

    #region CI STATUS bits

    /// <summary>
    /// DE CI Status bits
    /// </summary>
    [Flags]
    public enum DE_CI_STATUS
    {
      /// CI_ERR_MSG_AVAILABLE
      CI_ERR_MSG_AVAILABLE = 0x01,
      /// CI_MODULE_INIT_READY
      CI_MODULE_INIT_READY = 0x02,
      /// CI_MODULE_ERROR
      CI_MODULE_ERROR = 0x04,
      /// CI_MODULE_IS_DVB
      CI_MODULE_IS_DVB = 0x08,
      /// CI_MODULE_PRESENT
      CI_MODULE_PRESENT = 0x10,
      /// CI_APP_INFO_AVAILABLE
      CI_APP_INFO_AVAILABLE = 0x20,
      /// CI_DATE_TIME_REQEST
      CI_DATE_TIME_REQEST = 0x40,
      /// CI_PMT_REPLY
      CI_PMT_REPLY = 0x80,
      /// CI_MMI_REQUEST
      CI_MMI_REQUEST = 0x100
    }
#pragma warning restore 169

    #endregion

    #region variables

    private readonly bool _isDigitalEverywhere;
    private readonly bool _hasCAM;
    private readonly bool _isInitialized;
    private readonly IBaseFilter _filterTuner;

    private readonly IntPtr _ptrDataInstance;
    private readonly IntPtr _ptrDataReturned;
    private readonly IntPtr _ptrDataCiHandler;

    private DVBSChannel _previousChannel;

    private bool _readCamName;
    // CI menu related handlers
    private bool StopThread;
    private ICiMenuCallbacks m_ciMenuCallback;
    private Thread CiMenuThread;

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

      _ptrDataInstance = Marshal.AllocCoTaskMem(CA_DATA_SIZE);
      _ptrDataReturned = Marshal.AllocCoTaskMem(CA_DATA_SIZE);
      _ptrDataCiHandler = Marshal.AllocCoTaskMem(CA_DATA_SIZE);
      if (_filterTuner != null)
      {
        _isDigitalEverywhere = IsDigitalEverywhere;
        if (_isDigitalEverywhere)
        {
          _hasCAM = IsCamPresent();
          Log.Log.WriteFile("FireDTV cam detected  : {0}", _hasCAM);
          if (_hasCAM)
          {
            Log.Log.WriteFile("FireDTV cam name      : \"{0}\"", GetCAMName());
          }
          Log.Log.WriteFile("FireDTV driver version: {0}", GetDriverVersionNumber());
          Log.Log.WriteFile("FireDTV board version : {0}", GetHardwareFirmwareVersionNumber());
        }
      }
      _readCamName = true;
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

      // read CAM name, when it works, this usually means that CAM is ready to descramble (needed i.e. after resume)
      if (_readCamName)
      {
        Log.Log.WriteFile("FireDTV cam name    : \"{0}\"", GetCAMName());
      }

      //Log.Log.WriteFile("SendPMTToFireDTV pmtLength:{0}", pmtLength);
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

      FIRESAT_CA_DATA caData = GET_FIRESAT_CA_DATA(2, (ushort)(2 + pmtLength));

      string log = String.Format("FireDTV: #{0}/{1} pmt data:", current, max);
      log += String.Format("0x0 0x{0:X} 0x0 0x0 0x0 0x0 0x0 0x0 0x{1:X} 0x{2:X} ",
                           caData.uTag, caData.uLength2, caData.uLength1);

      if (current == 0 && max == 1)
        caData.uData[0] = 3; //      List Management = ONLY  (only=3, first=1, more=0, last=2)
      else if (current == 0 && max > 1)
        caData.uData[0] = 1; //      List Management = FIRST (only=3, first=1, more=0, last=2)
      else if (current > 0 && current < max - 1)
        caData.uData[0] = 0; //      List Management = MORE  (only=3, first=1, more=0, last=2)
      else if (current == max - 1)
        caData.uData[0] = 2; //      List Management = LAST  (only=3, first=1, more=0, last=2)
      log += String.Format("0x{0:X} ", caData.uData[0]);

      caData.uData[1] = 1; //      pmt_cmd = OK DESCRAMBLING
      log += String.Format("0x{0:X} ", caData.uData[1]);

      for (int i = 0; i < pmtLength; i++)
      {
        caData.uData[i + 2] = PMT[i];
        log += String.Format("0x{0:X} ", PMT[i]);
      }
      Log.Log.WriteFile(log);

      Marshal.StructureToPtr(caData, _ptrDataInstance, true);
      Marshal.StructureToPtr(caData, _ptrDataReturned, true);
      hr = propertySet.Set(propertyGuid, propId, _ptrDataInstance, CA_DATA_SIZE, _ptrDataReturned, CA_DATA_SIZE);

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

      int hr = propertySet.QuerySupported(propertyGuid, propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Log.WriteFile("FireDTV:ResetCAM() Reset CI is not supported");
        return;
      }

      FIRESAT_CA_DATA caData = GET_FIRESAT_CA_DATA(0, 1);
      caData.uData[0] = 0; // HW Reset of CI part

      Marshal.StructureToPtr(caData, _ptrDataInstance, true);
      Marshal.StructureToPtr(caData, _ptrDataReturned, true);
      hr = propertySet.Set(propertyGuid, propId, _ptrDataInstance, CA_DATA_SIZE, _ptrDataReturned, CA_DATA_SIZE);
      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV:ResetCAM() failed 0x{0:X}", hr);
        return;
      }
      Log.Log.WriteFile("FireDTV:ResetCAM() cam has been reset");
      return;
    }

    /// <summary>
    /// Sends the PMT of all subchannels to fire DTV.
    /// </summary>
    /// <param name="subChannels">The sub channels.</param>
    /// <returns></returns>
    public bool SendPMTToFireDTV(Dictionary<int, ConditionalAccessContext> subChannels)
    {
      if (!_hasCAM)
      {
        return true;
      }
      if (!IsCamReady())
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
        if (!exists && context.Channel.FreeToAir == false)
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
          if (pids.Count >= 1)
          {
            dvbtStruct.uPid1 = pids[0];
            dvbsStruct.uPid1 = pids[0];
          }
          if (pids.Count >= 2)
          {
            dvbtStruct.uPid2 = pids[1];
            dvbsStruct.uPid2 = pids[1];
          }
          if (pids.Count >= 3)
          {
            dvbtStruct.uPid3 = pids[2];
            dvbsStruct.uPid3 = pids[2];
          }
          if (pids.Count >= 4)
          {
            dvbtStruct.uPid4 = pids[3];
            dvbsStruct.uPid4 = pids[3];
          }
          if (pids.Count >= 5)
          {
            dvbtStruct.uPid5 = pids[4];
            dvbsStruct.uPid5 = pids[4];
          }
          if (pids.Count >= 6)
          {
            dvbtStruct.uPid6 = pids[5];
            dvbsStruct.uPid6 = pids[5];
          }
          if (pids.Count >= 7)
          {
            dvbtStruct.uPid7 = pids[6];
            dvbsStruct.uPid7 = pids[6];
          }
          if (pids.Count >= 8)
          {
            dvbtStruct.uPid8 = pids[7];
            dvbsStruct.uPid8 = pids[7];
          }
          if (pids.Count >= 9)
          {
            dvbtStruct.uPid9 = pids[8];
            dvbsStruct.uPid9 = pids[8];
          }
          if (pids.Count >= 10)
          {
            dvbtStruct.uPid10 = pids[9];
            dvbsStruct.uPid10 = pids[9];
          }
          if (pids.Count >= 11)
          {
            dvbtStruct.uPid11 = pids[10];
            dvbsStruct.uPid11 = pids[10];
          }
          if (pids.Count >= 12)
          {
            dvbtStruct.uPid12 = pids[11];
            dvbsStruct.uPid12 = pids[11];
          }
          if (pids.Count >= 13)
          {
            dvbtStruct.uPid13 = pids[12];
            dvbsStruct.uPid13 = pids[12];
          }
          if (pids.Count >= 14)
          {
            dvbtStruct.uPid14 = pids[13];
            dvbsStruct.uPid14 = pids[13];
          }
          if (pids.Count >= 15)
          {
            dvbtStruct.uPid15 = pids[14];
            dvbsStruct.uPid15 = pids[14];
          }
          if (pids.Count >= 16)
          {
            dvbtStruct.uPid16 = pids[15];
            dvbsStruct.uPid16 = pids[15];
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
    private string GetHardwareFirmwareVersionNumber()
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
                             _ptrDataInstance, INFO_DATA_SIZE,
                             _ptrDataReturned, INFO_DATA_SIZE, out byteCount);


        if (hr != 0)
        {
          Log.Log.WriteFile("FireDTV:GetFirmwareVersion() failed 0x{0:X}", hr);
          return String.Empty;
        }

        byte[] k = {0, 0, 0, 0, 0, 0};
        Marshal.Copy(_ptrDataReturned, k, 0, 6);

        // HW in first 3 bytes of returned data ( 8 = 3bytes of 2 chars and 2 separators )
        string hwrev = BitConverter.ToString(k).Replace("-", ".").Substring(0, 8);

        // SW firmware 3 bytes of returned data ( 8 = 3bytes of 2 chars and 2 separators )
        string fwrev = BitConverter.ToString(k).Replace("-", ".").Substring(9, 8);

        // SW firmware build in next 2 bytes
        string fwbuild =
          ((Marshal.ReadByte(_ptrDataReturned, 6) * 256) + Marshal.ReadByte(_ptrDataReturned, 7)).ToString();

        version = String.Format("HW {0}, FW {1} build {2}", hwrev, fwrev, fwbuild);
      }
      return version;
    }

    /// <summary>
    /// Gets the driver version number.
    /// </summary>
    /// <returns></returns>
    private string GetDriverVersionNumber()
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
                             _ptrDataInstance, INFO_DATA_SIZE,
                             _ptrDataReturned, INFO_DATA_SIZE, out byteCount);


        if (hr != 0)
        {
          Log.Log.WriteFile("FireDTV:GetDriverVersion() failed 0x{0:X}", hr);
          return String.Empty;
        }

        for (int i = 0; i < byteCount; ++i)
        {
          char ch;
          byte k = Marshal.ReadByte(_ptrDataReturned, i);

          if (k < 0x20)
            break;
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
    private int GetCAMStatus()
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

        //Log.Log.WriteFile("FireDTV:GetCAMStatus() status is <0x{0:X}>", camStatus);
        return camStatus;
      }
      finally
      {
        Log.Log.WriteFile("FireDTV:GetCAMStatus() finished");
      }
    }

    /// <summary>
    /// Read out CAM name
    /// When it works, this usually means that CAM is ready to descramble (i.e. after resume)
    /// </summary>
    /// <returns>CAM name</returns>
    private string GetCAMName()
    {
      Guid propertyGuid = KSPROPSETID_Firesat;
      const int propId = KSPROPERTY_FIRESAT_CA2HOST;
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      KSPropertySupport isTypeSupported;
      if (propertySet == null)
      {
        Log.Log.WriteFile("FireDTV:GetCAMName() properySet=null");
        return string.Empty;
      }

      int hr = propertySet.QuerySupported(propertyGuid, propId, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Log.WriteFile("FireDTV:GetCAMName() not supported");
        return string.Empty;
      }

      FIRESAT_CA_DATA caData = GET_FIRESAT_CA_DATA(1, 0);

      Marshal.StructureToPtr(caData, _ptrDataInstance, true);
      Marshal.StructureToPtr(caData, _ptrDataReturned, true);
      hr = propertySet.Set(propertyGuid, propId, _ptrDataInstance, CA_DATA_SIZE, _ptrDataReturned, CA_DATA_SIZE);
      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV: unable to set \"CA_APPLICATION_INFO\"");
      }

      const int timeout = 250; // at least 7 seconds for SamsungCAM - Italia (chemelli)
      const int loops = 40; // timeout * loops =  250 * 40 = 10.000 milliseconds

      for (int j = 0; j < loops; j++)
      {
        int bytesread;
        hr = propertySet.Get(propertyGuid, propId, _ptrDataInstance, CA_DATA_SIZE, _ptrDataReturned, CA_DATA_SIZE,
                             out bytesread);
        if (bytesread != 0)
        {
          break;
        }
        if (j == 0)
        {
          Log.Log.WriteFile("FireDTV: GetCAMName() looping for {0}s and retrying", (timeout * loops / 1000));
        }
        Thread.Sleep(timeout);
      }
      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV: GetCAMName() failed 0x{0:X}", hr);
        return string.Empty;
      }

      // cast ptr back to struct and handle it in c#
      FIRESAT_CA_DATA caDataReturned =
        (FIRESAT_CA_DATA)Marshal.PtrToStructure(_ptrDataReturned, typeof (FIRESAT_CA_DATA));

      short manufacturer_code = BitConverter.ToInt16(caDataReturned.uData, 0);
      short application_manufacturer = BitConverter.ToInt16(caDataReturned.uData, 2);
      Log.Log.WriteFile("FireDTV cam specs     : manufacturer_code={0}, application_manufacturer={1}",
                        manufacturer_code, application_manufacturer);

      int Length = Convert.ToInt16(caDataReturned.uData[4]);
      if (Length > 0)
      {
        string cam_name = string.Empty;
        for (int i = 0; i < Length; i++)
        {
          if (caDataReturned.uData[i + 5] == 0)
          {
            break;
          }
          cam_name += (char)caDataReturned.uData[i + 5];
        }
        _readCamName = false;
        return cam_name;
      }
      return "erroneous name";
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
      DE_CI_STATUS camStatus = (DE_CI_STATUS)GetCAMStatus();
      if ((camStatus & DE_CI_STATUS.CI_MODULE_PRESENT) != 0)
      {
        //CAM is inserted
        if ((camStatus & DE_CI_STATUS.CI_MODULE_IS_DVB) != 0)
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
      DE_CI_STATUS camStatus = (DE_CI_STATUS)GetCAMStatus();
      if ((camStatus & DE_CI_STATUS.CI_MODULE_PRESENT) != 0)
      {
        //CAM is inserted
        Log.Log.WriteFile("  FireDTV:cam is inserted");
        if ((camStatus & DE_CI_STATUS.CI_MODULE_IS_DVB) != 0)
        {
          //CAM is DVB CAM 
          Log.Log.WriteFile("  FireDTV:cam is valid");
          if ((camStatus & DE_CI_STATUS.CI_MODULE_ERROR) != 0)
          {
            //CAM has an error
            Log.Log.WriteFile("  FireDTV:cam has error");
            return false;
          }
          if ((camStatus & DE_CI_STATUS.CI_MODULE_INIT_READY) != 0)
          {
            //CAM is initialized
            Log.Log.WriteFile("  FireDTV:cam is ready");
          }
          else
          {
            Log.Log.WriteFile("  FireDTV:cam is NOT ready");
            return false;
          }
          if ((camStatus & DE_CI_STATUS.CI_APP_INFO_AVAILABLE) != 0)
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

      Marshal.WriteByte(_ptrDataInstance, 0, 0xFF); //Voltage;
      Marshal.WriteByte(_ptrDataInstance, 1, 0xFF); //ContTone;
      Marshal.WriteByte(_ptrDataInstance, 2, 0xFF); //Burst;
      Marshal.WriteByte(_ptrDataInstance, 3, 0x01); //NrDiseqcCmds;

      Marshal.WriteByte(_ptrDataInstance, 4, 0x04); //diseqc command 1. length=4
      Marshal.WriteByte(_ptrDataInstance, 5, 0xE0); //diseqc command 1. uFraming=0xe0
      Marshal.WriteByte(_ptrDataInstance, 6, 0x10); //diseqc command 1. uAddress=0x10
      Marshal.WriteByte(_ptrDataInstance, 7, 0x38); //diseqc command 1. uCommand=0x38


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
      Log.Log.WriteFile(
        "FireDTV SendDiseqcCommand() diseqc:{0}, antenna:{1} frequency:{2},  polarisation:{3} hiband:{4}",
        channel.DisEqc, antennaNr, channel.Frequency, channel.Polarisation, hiBand);

      bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                           (channel.Polarisation == Polarisation.CircularL));
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
      Marshal.WriteByte(_ptrDataInstance, 0, 0xFF); //Voltage;
      Marshal.WriteByte(_ptrDataInstance, 1, 0xFF); //ContTone;
      Marshal.WriteByte(_ptrDataInstance, 2, 0xFF); //Burst;
      Marshal.WriteByte(_ptrDataInstance, 3, 0x01); //NrDiseqcCmds;
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
    /// Start the ci handler with the graph
    /// </summary>
    internal void OnStartGraph()
    {
      StartCiHandlerThread();
    }

    /// <summary>
    /// Set parameter to null when stopping the Graph.
    /// </summary>
    public void OnStopGraph()
    {
      _readCamName = true;
      _previousChannel = null;
      StopCiHandlerThread();
    }

    #region CiMenuHandlerThread start and stop

    /// <summary>
    /// Stops CiHandler thread
    /// </summary>
    private void StopCiHandlerThread()
    {
      if (CiMenuThread != null)
      {
        CiMenuThread.Abort();
        CiMenuThread = null;
      }
    }

    /// <summary>
    /// Starts CiHandler thread
    /// </summary>
    private void StartCiHandlerThread()
    {
      // Check if the polling thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
      if (CiMenuThread != null && !CiMenuThread.IsAlive)
      {
        CiMenuThread.Abort();
        CiMenuThread = null;
      }
      if (CiMenuThread == null)
      {
        Log.Log.Debug("FireDTV: Starting new CI handler thread");
        StopThread = false;
        CiMenuThread = new Thread(new ThreadStart(CiMenuHandler));
        CiMenuThread.Name = "FireDTV CiMenuHandler";
        CiMenuThread.IsBackground = true;
        CiMenuThread.Priority = ThreadPriority.Lowest;
        CiMenuThread.Start();
      }
    }

    #endregion

    #region ICiMenuActions Member

    /// <summary>
    /// Sets the callback handler
    /// </summary>
    /// <param name="ciMenuHandler"></param>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        Log.Log.Debug("FireDTV: registering ci callbacks");
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
      Log.Log.Debug("FireDTV: Enter CI Menu");
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      if (propertySet == null)
      {
        Log.Log.Debug("FireDTV:EnterCIMenu() properySet=null");
        return false;
      }
      /* QuerySupported has been done in GetCAMName already */
      FIRESAT_CA_DATA caData = GET_FIRESAT_CA_DATA(7 /*CA_ENTER_MENU*/, 0);

      Marshal.StructureToPtr(caData, _ptrDataInstance, true);
      Marshal.StructureToPtr(caData, _ptrDataReturned, true);
      int hr = propertySet.Set(KSPROPSETID_Firesat, KSPROPERTY_FIRESAT_HOST2CA, _ptrDataInstance, CA_DATA_SIZE,
                               _ptrDataReturned, CA_DATA_SIZE);
      if (hr != 0)
      {
        Log.Log.Debug("FireDTV: unable to send CA_ENTER_MENU");
      }
      Log.Log.Debug("FireDTV: Enter CI Menu successful");
      return true;
    }

    /// <summary>
    /// Closes the CI menu 
    /// </summary>
    /// <returns></returns>
    public bool CloseCIMenu()
    {
      Log.Log.Debug("FireDTV: Close CI Menu");
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      if (propertySet == null)
      {
        Log.Log.Debug("FireDTV:EnterCIMenu() properySet=null");
        return false;
      }
      /* QuerySupported has been done in GetCAMName already */
      FIRESAT_CA_DATA caData = GET_FIRESAT_CA_DATA(5 /*CA_MMI*/, 5);
      DVB_MMI.CreateMMIClose(ref caData.uData);
      Marshal.StructureToPtr(caData, _ptrDataInstance, true);
      Marshal.StructureToPtr(caData, _ptrDataReturned, true);
      int hr = propertySet.Set(KSPROPSETID_Firesat, KSPROPERTY_FIRESAT_HOST2CA, _ptrDataInstance, CA_DATA_SIZE,
                               _ptrDataReturned, CA_DATA_SIZE);
      if (hr != 0)
      {
        Log.Log.Debug("FireDTV: unable to send CA_MMI close");
      }
      Log.Log.Debug("FireDTV: Close CI Menu successful");
      return true;
    }

    /// <summary>
    /// Selects a CI menu entry
    /// </summary>
    /// <param name="choice"></param>
    /// <returns></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Log.Debug("FireDTV: Select CI Menu entry {0}", choice);
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      if (propertySet == null)
      {
        Log.Log.Debug("FireDTV:SelectMenu() properySet=null");
        return false;
      }
      /* QuerySupported has been done in GetCAMName already */
      FIRESAT_CA_DATA caData = GET_FIRESAT_CA_DATA(5 /*CA_MMI*/, 5);
      DVB_MMI.CreateMMISelect(choice, ref caData.uData);

      Marshal.StructureToPtr(caData, _ptrDataInstance, true);
      Marshal.StructureToPtr(caData, _ptrDataReturned, true);
      int hr = propertySet.Set(KSPROPSETID_Firesat, KSPROPERTY_FIRESAT_HOST2CA, _ptrDataInstance, CA_DATA_SIZE,
                               _ptrDataReturned, CA_DATA_SIZE);
      if (hr != 0)
      {
        Log.Log.Debug("FireDTV: unable to select CI Menu entry");
      }
      Log.Log.Debug("FireDTV: Close CI Menu successful");
      return true;
    }

    /// <summary>
    /// Sends an answer after CI request
    /// </summary>
    /// <param name="Cancel"></param>
    /// <param name="Answer"></param>
    /// <returns></returns>
    public bool SendMenuAnswer(bool Cancel, String Answer)
    {
      if (Answer == null) Answer = "";
      Log.Log.Debug("FireDTV: Send CI Menu answer {0}", Answer);
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
      if (propertySet == null)
      {
        Log.Log.Debug("FireDTV:SendMenuAnswer() properySet=null");
        return false;
      }
      /* QuerySupported has been done in GetCAMName already */
      FIRESAT_CA_DATA caData = GET_FIRESAT_CA_DATA(5 /*CA_MMI*/, 0);
      DVB_MMI.CreateMMIAnswer(Cancel, Answer, ref caData.uData, ref caData.uLength1, ref caData.uLength2);
      //Log.Log.Debug("FireDTV:Created answer: length:{0},{1}", caData.uLength1, caData.uLength2);
      //DVB_MMI.DumpBinary(caData.uData,0,caData.uLength1);
      Marshal.StructureToPtr(caData, _ptrDataInstance, true);
      Marshal.StructureToPtr(caData, _ptrDataReturned, true);
      int hr = propertySet.Set(KSPROPSETID_Firesat, KSPROPERTY_FIRESAT_HOST2CA, _ptrDataInstance, CA_DATA_SIZE,
                               _ptrDataReturned, CA_DATA_SIZE);
      if (hr != 0)
      {
        Log.Log.Debug("FireDTV: unable to send CI Menu answer");
      }
      Log.Log.Debug("FireDTV: send CI Menu successful");
      return true;
    }

    #endregion

    #region CiMenuHandlerThread for polling status and handling MMI

    /// <summary>
    /// Thread that checks for CI menu 
    /// </summary>
    private void CiMenuHandler()
    {
      Log.Log.Debug("FireDTV: CI handler thread start polling status");
      int bytesReturned;
      int hr;
      DVB_MMI_Handler MMI = new DVB_MMI_Handler("FireDTV", ref m_ciMenuCallback);
      DE_CI_STATUS CiStatus;

      // Init CiStatus word to 0
      Marshal.WriteInt16(_ptrDataCiHandler, 0);
      IKsPropertySet propertySet = _filterTuner as IKsPropertySet;

      if (propertySet == null)
      {
        Log.Log.Debug("FireDTV:CiMenuHandler() properySet=null");
        return;
      }

      while (!StopThread)
      {
        try
        {
          // this code is equal to GetCAMStatus, but implemented separately to avoid memory / threading conflicts with used pointers!
          hr = propertySet.Get(KSPROPSETID_Firesat, KSPROPERTY_FIRESAT_GET_CI_STATUS, _ptrDataCiHandler, CA_DATA_SIZE,
                               _ptrDataCiHandler, CA_DATA_SIZE, out bytesReturned);
          if (hr != 0)
          {
            Log.Log.Debug("FireDTV: error reading CI state.");
          }
          else
          {
            CiStatus = (DE_CI_STATUS)Marshal.ReadInt16(_ptrDataCiHandler);
#if DEBUG
            Log.Log.Debug("FireDTV: CI iStatus:{0}", CiStatus);
#endif
            if ((CiStatus & DE_CI_STATUS.CI_MMI_REQUEST) != 0)
            {
              Log.Log.Debug("FireDTV: CI menu object available!");

              // Get the MMI object
              FIRESAT_CA_DATA caData = GET_FIRESAT_CA_DATA(5 /*CA_MMI*/, 0);

              Marshal.StructureToPtr(caData, _ptrDataInstance, true);
              Marshal.StructureToPtr(caData, _ptrDataCiHandler, true);
              hr = propertySet.Set(KSPROPSETID_Firesat, KSPROPERTY_FIRESAT_CA2HOST, _ptrDataInstance, CA_DATA_SIZE,
                                   _ptrDataCiHandler, CA_DATA_SIZE);
              if (hr != 0)
              {
                Log.Log.Debug("FireDTV: unable to set \"CA_MMI\"");
              }

              hr = propertySet.Get(KSPROPSETID_Firesat, KSPROPERTY_FIRESAT_CA2HOST, _ptrDataInstance, CA_DATA_SIZE,
                                   _ptrDataCiHandler, CA_DATA_SIZE, out bytesReturned);
              if (hr != 0)
              {
                Log.Log.Debug("FireDTV: unable to get \"CA_MMI\": hr {0:X}", hr);
              }
              else
              {
                // cast ptr back to struct and handle it in c#
                FIRESAT_CA_DATA caDataReturned =
                  (FIRESAT_CA_DATA)Marshal.PtrToStructure(_ptrDataCiHandler, typeof (FIRESAT_CA_DATA));

                Int32 caDataLength = caDataReturned.uLength2 << 8 | caDataReturned.uLength1;
                MMI.HandleMMI(caDataReturned.uData, caDataLength);
              }
            }
          }
          Thread.Sleep(500);
        }
        catch (ThreadAbortException) {}
        catch (Exception ex)
        {
          Log.Log.Debug("FireDTV: error in CiMenuHandler thread\r\n{0}", ex.ToString());
          return;
        }
      }
    }

    #endregion

    #region IDisposable Member

    /// <summary>
    /// Disposes DE class and free up memory
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
      Marshal.FreeCoTaskMem(_ptrDataInstance);
      Marshal.FreeCoTaskMem(_ptrDataReturned);
      Marshal.FreeCoTaskMem(_ptrDataCiHandler);
    }

    #endregion
  }
}