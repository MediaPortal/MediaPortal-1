using System;
using System.Collections;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
  public class DigitalEverywhere //: IksPropertyUtils
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

    static public readonly Guid KSPROPSETID_Firesat = new Guid(0xab132414, 0xd060, 0x11d0, 0x85, 0x83, 0x00, 0xc0, 0x4f, 0xd9, 0xba, 0xf3);
    #region property ids
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
    #endregion

    #region variables
    bool _isDigitalEverywhere;
    bool _hasCAM;
    bool _isInitialized;
    IBaseFilter _filterTuner;

    IntPtr _ptrDataInstance;
    IntPtr _ptrDataReturned;
    #endregion

    public DigitalEverywhere(IBaseFilter tunerFilter, IBaseFilter captureFilter)
    //: base(filter)
    {
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
          //Log.Log.WriteFile("FireDTV FW version:{0} ", GetFirmwareVersionNumber());
        }
      }
      _isInitialized = true;

    }

    public bool IsDigitalEverywhere
    {
      get
      {
        if (_isInitialized) return _isDigitalEverywhere;

        IKsPropertySet propertySet = _filterTuner as IKsPropertySet;
        if (propertySet == null) return false;
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
      if (_hasCAM == false) return true;
      if (IsCamReady() == false)
      {
        ResetCAM();
        return false;
      }

      //typedef struct _FIRESAT_CA_DATA{ 
      //  UCHAR uSlot;                      //0
      //  UCHAR uTag;                       //1   (2..3 = padding)
      //  BOOL bMore;                       //4   (5..7 = padding)
      //  USHORT uLength;                   //8..9
      //  UCHAR uData[MAX_PMT_SIZE];        //10....
      //}FIRESAT_CA_DATA, *PFIRESAT_CA_DATA;

      if (PMT == null) return false;
      if (pmtLength == 0) return false;

      //Log.Log.WriteFile("SendPMTToFireDTV pmt:{0}", pmtLength);
      Guid propertyGuid = KSPROPSETID_Firesat;
      int propId = KSPROPERTY_FIRESAT_HOST2CA;
      DirectShowLib.IKsPropertySet propertySet = _filterTuner as DirectShowLib.IKsPropertySet;
      KSPropertySupport isTypeSupported = 0;
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
      byData[10] = 3;     // 10     List Management = ONLY (only=3, first=1, more=0, last=2)
      byData[11] = 1;     // 11     pmt_cmd = OK DESCRAMBLING		
      for (int i = 0; i < pmtLength; ++i)
      {
        byData[i + 12] = PMT[i];
      }

      string log = String.Format("FireDTV: pmt data:");
      for (int i = 0; i < 1036; ++i)
      {
        Marshal.WriteByte(_ptrDataInstance, i, byData[i]);
        Marshal.WriteByte(_ptrDataReturned, i, byData[i]);
        log += String.Format("0x{0:X} ", byData[i]);
      }

      //      Log.Log.WriteFile(log);
      hr = propertySet.Set(propertyGuid, propId, _ptrDataInstance, 1036, _ptrDataReturned, 1036);

      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV:  failed 0x{0:X}", hr);
        ResetCAM();
        return false;
      }
      return true;
    }//public bool SendPMTToFireDTV(byte[] PMT)

    public void ResetCAM()
    {
      Log.Log.WriteFile("FireDTV:ResetCAM()");
      Guid propertyGuid = KSPROPSETID_Firesat;
      int propId = KSPROPERTY_FIRESAT_HOST2CA;
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
        string log = "hw resetdata:";
        for (int i = 0; i < 1036; ++i)
        {
          Marshal.WriteByte(_ptrDataInstance, i, byData[i]);
          Marshal.WriteByte(_ptrDataReturned, i, byData[i]);
          log += String.Format("0x{0:X} ", byData[i]);
        }

        //Log.Log.WriteFile(log);
        hr = propertySet.Set(propertyGuid, (int)propId, _ptrDataInstance, (int)1036, _ptrDataReturned, (int)1036);

        if (hr != 0)
        {
          Log.Log.WriteFile("FireDTV:ResetCAM() failed 0x{0:X}", hr);
          return;
        }
        Log.Log.WriteFile("FireDTV:ResetCAM() cam has been reset");
      }
      finally
      {
      }
      return;
    }

    public bool SetHardwarePidFiltering(bool isDvbc, bool isDvbT, bool isDvbS, bool isAtsc, ArrayList pids)
    {
      string logStart = "dvbt:";
      DirectShowLib.IKsPropertySet propertySet = _filterTuner as DirectShowLib.IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      KSPropertySupport isTypeSupported = 0;
      uint propertySelect = (uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T;
      //if (isDvbc)
      //{
      //  propertySelect = (uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C;
      //  logStart = "dvbc:";
      //}
      if (isDvbc || isDvbS)
      {
        propertySelect = (uint)KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S;
        logStart = "dvbs:";
      }
      int hr = propertySet.QuerySupported(propertyGuid, (int)propertySelect, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Set) == 0)
      {
        Log.Log.WriteFile("FireDTV: Set H/W pid filtering is not supported");
        return true;
      }

      FIRESAT_SELECT_PIDS_DVBT dvbtStruct = new FIRESAT_SELECT_PIDS_DVBT();
      FIRESAT_SELECT_PIDS_DVBS dvbsStruct = new FIRESAT_SELECT_PIDS_DVBS();
      dvbtStruct.bCurrentTransponder = true; dvbtStruct.bFullTransponder = false;
      dvbsStruct.bCurrentTransponder = true; dvbsStruct.bFullTransponder = false;
      if (pids.Count > 0)
      {
        int pidCount = pids.Count;
        if (pidCount > 16) pidCount = 16;
        //get only specific pids
        dvbtStruct.bFullTransponder = false; dvbtStruct.uNumberOfValidPids = (byte)pidCount;
        dvbsStruct.bFullTransponder = false; dvbsStruct.uNumberOfValidPids = (byte)pidCount;
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

      Log.Log.WriteFile("FireDTV: Set H/W pid filtering count:{0} len:{1}", pids.Count, len);

      string txt = "";
      for (int i = 0; i < len; ++i)
        txt += String.Format("0x{0:X} ", Marshal.ReadByte(_ptrDataInstance, i));

      Log.Log.WriteFile("FireDTV: Set H/W pid filtering pid {0} data:{1}", logStart, txt);
      hr = propertySet.Set(propertyGuid,
                          (int)propertySelect,
                          _ptrDataInstance, (int)len,
                          _ptrDataReturned, (int)len);

      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV: Set H/W pid filtering failed 0x{0:X}", hr);
        return false;
      }

      return true;
    }
    /*
    public string GetFirmwareVersionNumber()
    {
      DirectShowLib.IKsPropertySet propertySet = _filterTuner as DirectShowLib.IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      KSPropertySupport isTypeSupported = 0;
      int hr = propertySet.QuerySupported(propertyGuid, (int)KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
      {
        Log.Log.WriteFile("FireDTV:GetDriverVersion() not supported");
        return String.Empty;
      }
      int byteCount = 0;
      hr = propertySet.Get(propertyGuid,
                                  (int)KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION,
                                  _ptrDataInstance, (int)100,
                                  _ptrDataReturned, (int)100, out byteCount);


      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV:GetFirmwareVersionNumber() failed 0x{0:X}", hr);
        return String.Empty;
      }
      Log.Log.WriteFile("count:{0}", byteCount);

      string version = String.Empty;
      for (int i = 0; i < byteCount; ++i)
      {
        char ch;
        byte k = Marshal.ReadByte(_ptrDataReturned, i);

        Log.Log.WriteFile("{0} = 0x{1:X} = {2} = {3}",
                i, k, k, (char)k);
        if (k < 0x20)
          ch = '.';
        else
          ch = (char)k;
        version += ch;
      }
      return version;
    }
    */
    public string GetDriverVersionNumber()
    {
      DirectShowLib.IKsPropertySet propertySet = _filterTuner as DirectShowLib.IKsPropertySet;
      Guid propertyGuid = KSPROPSETID_Firesat;
      KSPropertySupport isTypeSupported = 0;
      int hr = propertySet.QuerySupported(propertyGuid, (int)KSPROPERTY_FIRESAT_DRIVER_VERSION, out isTypeSupported);
      if (hr != 0 || (isTypeSupported & KSPropertySupport.Get) == 0)
      {
        Log.Log.WriteFile("FireDTV:GetDriverVersion() not supported");
        return String.Empty;
      }
      int byteCount = 0;
      hr = propertySet.Get(propertyGuid,
                                  (int)KSPROPERTY_FIRESAT_DRIVER_VERSION,
                                  _ptrDataInstance, (int)22,
                                  _ptrDataReturned, (int)22, out byteCount);


      if (hr != 0)
      {
        Log.Log.WriteFile("FireDTV:GetDriverVersion() failed 0x{0:X}", hr);
        return String.Empty;
      }
      //      Log.Log.WriteFile("count:{0}", byteCount);

      string version = String.Empty;

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
      return version;
    }

    int GetCAMStatus()
    {
      Guid propertyGuid = KSPROPSETID_Firesat;
      int propId = KSPROPERTY_FIRESAT_GET_CI_STATUS;
      DirectShowLib.IKsPropertySet propertySet = _filterTuner as DirectShowLib.IKsPropertySet;
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
      int bytesReturned;
      try
      {
        hr = propertySet.Get(propertyGuid, propId, _ptrDataInstance, 1036, _ptrDataReturned, 1036, out bytesReturned);
        if (hr != 0)
        {
          Log.Log.WriteFile("FireDTV:GetCAMStatus() failed 0x{0:X}", hr);
          if (((uint)hr) == ((uint)0x8007001F))
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

        return camStatus;
      }
      finally
      {
      }
    }

    public bool IsCamPresent()
    {
      if (_isInitialized) return _hasCAM;
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
            return true;
          }
          else
          {
            Log.Log.WriteFile("  FireDTV:cam is not ready");
            return false;
          }
        }
        else
        {
          return true;
        }
      }
      return true;
    }
    public void SendDiseqcCommand(DVBSChannel channel)
    {
      int antennaNr = 1;
      switch (channel.DisEqc)
      {
        case DisEqcType.None: // none
          return;
        case DisEqcType.SimpleA: // Simple A
          antennaNr = 1;
          break;
        case DisEqcType.SimpleB: // Simple B
          antennaNr = 2;
          break;
        case DisEqcType.Level1AA: // Level 1 A/A
          antennaNr = 1;
          break;
        case DisEqcType.Level1BA: // Level 1 B/A
          antennaNr = 2;
          break;
        case DisEqcType.Level1AB: // Level 1 A/B
          antennaNr = 3;
          break;
        case DisEqcType.Level1BB: // Level 1 B/B
          antennaNr = 4;
          break;
      }
      //"01,02,03,04,05,06,07,08,09,0a,0b,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,cc,"	
      Log.Log.WriteFile("FireDTV SendDiseqcCommand() diseqc:{0}, antenna:{1} frequency:{2}, switching frequency:{3}, polarisation:{4}",
              channel.DisEqc, antennaNr, channel.Frequency, channel.SwitchingFrequency, channel.Polarisation);
      
      Marshal.WriteByte(_ptrDataInstance, 0, 0xFF);//Voltage;
      Marshal.WriteByte(_ptrDataInstance, 1, 0xFF);//ContTone;
      Marshal.WriteByte(_ptrDataInstance, 2, 0xFF);//Burst;
      Marshal.WriteByte(_ptrDataInstance, 3, 0x01);//NrDiseqcCmds;

      Marshal.WriteByte(_ptrDataInstance, 4, 0x04);//diseqc command 1. length=4
      Marshal.WriteByte(_ptrDataInstance, 5, 0xE0);//diseqc command 1. uFraming=0xe0
      Marshal.WriteByte(_ptrDataInstance, 6, 0x10);//diseqc command 1. uAddress=0x10
      Marshal.WriteByte(_ptrDataInstance, 7, 0x38);//diseqc command 1. uCommand=0x38

      // for the write to port group 0 command:
      // data 0 : low nibble specifies the values of each bit
      //		bit		0    :  0= low band,   1 = high band
      //    bit   1    :  0= horizontal, 1 = vertical
      //    bits  2..3 :  antenna number (0-3)
      //    bit   4-7  : specifices which bits are valid , 0XF means all bits are valid and should be set)
      byte uContTone;
      if (channel.Frequency < channel.SwitchingFrequency)
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
      cmd |= (byte)(((antennaNr - 1) * 4) & 0x0F);
      cmd |= (byte)(uContTone == 1 ? 1 : 0);
      cmd |= (byte)(channel.Polarisation == Polarisation.LinearV ? 2 : 0);
      Marshal.WriteByte(_ptrDataInstance, 8, cmd);

      Guid propertyGuid = KSPROPSETID_Firesat;
      int propId = KSPROPERTY_FIRESAT_LNB_CONTROL;
      DirectShowLib.IKsPropertySet propertySet = _filterTuner as DirectShowLib.IKsPropertySet;
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
        Log.Log.WriteFile("FireDTV:SendDiseqcCommand() failed:{0:X}",hr);
      }
    }
  }
}
