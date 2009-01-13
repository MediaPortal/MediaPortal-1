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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace MediaPortal.InputDevices.FireDTV
{
  public class FireDTVConstants
  {
    #region Constants

    public const int WM_USER = 0x0400;
    public const int MAX_PMT_SIZE = 1024;
    public const int FIRMWARE_HEXDATASIZE = 368754;
    public const int OFDM_AUTO = 0xFF;
    public const int LNB_COMMAND_DONT_CARE = 0xFF;

    public const int LNB_COMMAND_CONT_TONE_OFF = 0;
    public const int LNB_COMMAND_CONT_TONE_ON = 1;

    public const int LNB_COMMAND_SAT_A = 0;
    public const int LNB_COMMAND_SAT_B = 1;

    #endregion

    #region FireDTV Enumeration

    public enum FireDTVWindowMessages
    {
      DeviceAttached = WM_USER + 1,
      DeviceDetached = WM_USER + 2,
      DeviceChanged = WM_USER + 3,
      CIModuleInserted = WM_USER + 4,
      CIModuleReady = WM_USER + 5,
      CIModuleRemoved = WM_USER + 6,
      CIMMI = WM_USER + 7,
      CIDateTime = WM_USER + 8,
      CIPMTReply = WM_USER + 9,
      RemoteControlEvent = WM_USER + 10
    }

    public enum FireDTVStatusCodes
    {
      Success = 0,
      Error = 1,
      InvalidDeviceHandle = 2,
      InvalidValue = 3,
      AlreadyInUse = 4,
      NotSuppotedByTuner = 5,
    } ;

    public enum FireDTVTunerType
    {
      WDM,
      BDA
    } ;

    public enum FireDTVRemoteControlKeyCodes
    {
      RemoteKey_Power = 768,
      RemoteKey_Sleep = 769,
      RemoteKey_Mute = 789,
      RemoteKey_Record = 791,
      RemoteKey_PreviousChapter = 795,
      RemoteKey_StopEject = 770,
      RemoteKey_NextChapter = 798,
      RemoteKey_SubTitle = 790,
      RemoteKey_Rewind = 796,
      RemoteKey_PausePlay = 797,
      RemoteKey_FastFoward = 847,
      RemoteKey_List = 848,
      RemoteKey_Favourites = 849,
      RemoteKey_UpArrow = 780,
      RemoteKey_Menu = 850,
      RemoteKey_LeftArrow = 776,
      RemoteKey_Select = 771,
      RemoteKey_RightArrow = 772,
      RemoteKey_EPG = 851,
      RemoteKey_DownArrow = 784,
      RemoteKey_Exit = 852,
      RemoteKey_VolumeUp = 799,
      RemoteKey_VolumeDown = 843,
      RemoteKey_ChannelUp = 832,
      RemoteKey_ChannelDown = 844,
      RemoteKey_ChannelList = 841,
      RemoteKey_Last = 845,
      RemoteKey_Full = 788,
      RemoteKey_Info = 846,
      RemoteKey_1 = 773,
      RemoteKey_2 = 774,
      RemoteKey_3 = 775,
      RemoteKey_4 = 777,
      RemoteKey_5 = 778,
      RemoteKey_6 = 779,
      RemoteKey_7 = 781,
      RemoteKey_8 = 782,
      RemoteKey_9 = 783,
      RemoteKey_0 = 786,
      RemoteKey_Text = 792,
      RemoteKey_Audio = 793,
      RemoteKey_CI = 842,
      RemoteKey_Display4_3 = 833,
      RemoteKey_Display16_9 = 787,
      RemoteKey_OnScreenDisplay = 785,
      RemoteKey_TV = 834,
      RemoteKey_DVD = 835,
      RemoteKey_VCR = 836,
      RemoteKey_AUX = 837,
      RemoteKey_Red = 794,
      RemoteKey_Green = 838,
      RemoteKey_Yellow = 839,
      RemoteKey_Blue = 840
    }

    public enum FireDTVCIStatusCodes
    {
      CIMMIRequest = 0x0100,
      CIPMTReply = 0x0080,
      CIDateTimeRequest = 0x0040,
      CIApplicationInformationAvailable = 0x0020,
      CIModulePresent = 0x0010,
      CIModuleIsDVB = 0x0008,
      CIModuleError = 0x0004,
      CIModuleInitReady = 0x0002,
      CIErrorMessageAvailable = 0x0001
    }

    public enum FireDTVMMIMessages
    {
      MMITagClose = 0x9F8800,
      MMITagEnquiry = 0x9F8807,
      MMITagMenuMore = 0x9F880A,
      MMITagMenuLast = 0x9F8809,
      MMITagListMore = 0x9F880D,
      MMITagListLast = 0x9F880C,
      MMITagTextMore = 0x9F8804,
      MMITagTextLast = 0x9F8803
    }

    public enum FireDTVFirmwareStatusCodes
    {
      ResponseInvCount = 0x4,
      ResponseInvCRC = 0x5,
      FirmwareResponseSuccess = 0x6,
      ResponseError = 0x7,
      ResponseStartBurning = 0x8,
      MaximiumNumberSections = 0x2,
      InvHardwareVersion = 0x9
    }

    public enum FireDTVFrontEndCodes
    {
      PowerSupply = 0x01,
      PowerStatus = 0x02,
      Autotune = 0x04,
      AntennaError = 0x08,
      FrontEndError = 0x10,
      VoltageValid = 0x40,
      FlagsValid = 0x80
    }

    public enum FireDTVSystemInformation
    {
      AnalogAudio = 0x10,
      AnalogVideo = 0x11,
      DVB = 0x20,
      DAB = 0x21,
      ATSC = 0x22,
      DVBSatellite = 0x1,
      DVBCable = 0x2,
      DVBTerrestrial = 0x3
    }

    public enum FireDTVProtocolCodes
    {
      Satellite = 0x1,
      Cable = 0x2,
      Terrestrial = 0x3
    }

    public enum FireDTVAntennaCodes
    {
      Fixed = 0x0,
      Movable = 0x1,
      Mobile = 0x2
    }

    public enum FireDTVFEC
    {
      FECAUTO = 0,
      FEC12 = 1,
      FEC23 = 2,
      FEC34 = 3,
      FEC56 = 4,
      FEC78 = 5
    }

    public enum FireDTVPolarization
    {
      Horizontal = 0,
      Vertical = 1,
      None = 0xFF
    }

    public enum FireDTVConstellation
    {
      DVB_T_QPSK = 0,
      QAM16 = 1,
      QAM64 = 2
    }

    public enum FireDTVHierarchy
    {
      None = 0,
      One = 1,
      Two = 2,
      Four = 4
    }

    public enum FireDTVCodeRate
    {
      Rate12 = 0,
      Rate23 = 1,
      Rate34 = 2,
      Rate56 = 3,
      Rate76 = 4
    }

    public enum FireDTVTransmission
    {
      Mode2K = 0,
      Mode8K = 1
    }

    public enum FireDTVBandWidth
    {
      _8Mhz = 0,
      _7Mhz = 1,
      _6Mhz = 2
    }

    public enum FireDTVPower
    {
      StatusOn = 0x70,
      StatusOff = 0x60
    }

    public enum FireDTVGuard
    {
      Interval_1_32 = 0,
      Interval_1_16 = 1,
      Interval_1_8 = 2,
      Interval_1_4 = 3
    }

    #endregion

    #region Structs

    public struct FireDTV_GUID
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] Bytes;
    }

    public struct FireDTV_DRIVER_VERSION
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] DriverVersion;
    }

    public struct FireDTV_SELECT_MULTIPLEX_DVBS
    {
      public ulong uFrequency; // 9.750.000 - 12.750.000 kHz
      public ulong uSymbolRate; // 1.000 - 40.000 kBaud
      public byte uFecInner; // FEC_AUTO, FEC_12,FEC_23, FEC_34, FEC_56, FEC_78
      public byte uPolarization; // POLARIZATION_HORIZONTAL,POLARIZATION_VERTICAL,POLARIZATION_NONE
      public byte uLnb;
    }

    public struct FireDTV_SELECT_MULTIPLEX_DVBT
    {
      public ulong uFrequency; // 47.000 - 860.000 kHz
      public byte uBandwidth; // BANDWIDTH_8_MHZ, BANDWIDTH_7_MHZ, BANDWIDTH_6_MHZ
      public byte uConstellation; // CONSTELLATION_DVB_T_QPSK,CONSTELLATION_QAM_16,CONSTELLATION_QAM_64,OFDM_AUTO
      public byte uCodeRateHP; // CODE_RATE_12,CODE_RATE_23,CODE_RATE_34,CODE_RATE_56,CODE_RATE_78,OFDM_AUTO
      public byte uCodeRateLP; // CODE_RATE_12,CODE_RATE_23,CODE_RATE_34,CODE_RATE_56,CODE_RATE_78,OFDM_AUTO

      public byte uGuardInterval;
                  // GUARD_INTERVAL_1_32,GUARD_INTERVAL_1_16,GUARD_INTERVAL_1_8,GUARD_INTERVAL_1_4,OFDM_AUTO

      public byte uTransmissionMode; // TRANSMISSION_MODE_2K, TRANSMISSION_MODE_8K, OFDM_AUTO
      public byte uHierarchyInfo; // HIERARCHY_NONE,HIERARCHY_1,HIERARCHY_2,HIERARCHY_4,OFDM_AUTO
    }

    public struct FireDTV_SELECT_SERVICE_DVBS
    {
      public bool bCurrentTransponder;
      public ulong uLnb;

      public struct QpskParameter
      {
        public ulong uFrequency; // kHz
        public ulong uSymbolRate; // kBaud
        public byte uFecInner; // FEC_12,FEC_23, FEC_34, FEC_56, FEC_78
        public byte uPolarization; // POLARIZATION_HORIZONTAL, POLARIZATION_VERTICAL
      } ;

      public short uOriginalNetworkId;
      public short uTransportStreamId;
      public short uServiceId;
      public short uVideoPid;
      public short uAudioPid;
      public short uPcrPid;
      public short uTeleTxtPid;
      public short uPMTPid;
    }

    public struct FireDTV_SELECT_PIDS_DVBS
    {
      public bool bCurrentTransponder;
      public bool bFullTransponder;
      public byte uLnb; // 0-3

      public struct QpskParameter
      {
        public ulong uFrequency; // 9.750.000 - 12.750.000 kHz
        public ulong uSymbolRate; // kBaud 1.000 - 40.000
        public byte uFecInner; // FEC_12,FEC_23, FEC_34, FEC_56, FEC_78
        public byte uPolarization; // POLARIZATION_HORIZONTAL, POLARIZATION_VERTICAL, POLARIZATION_NONE
      } ;

      public byte uNumberOfValidPids; // 1-16
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public short[] uPids;
    }

    public struct FireDTV_SELECT_PIDS_DVBT
    {
      public bool bCurrentTransponder;
      public bool bFullTransponder;

      public struct OFDMParameter
      {
        public ulong uFrequency; // kHz 47.000-860.000
        public byte uBandwidth; // BANDWIDTH_8_MHZ, BANDWIDTH_7_MHZ, BANDWIDTH_6_MHZ
        public byte uConstellation; // CONSTELLATION_DVB_T_QPSK,CONSTELLATION_QAM_16,CONSTELLATION_QAM_64,OFDM_AUTO
        public byte uCodeRateHP; // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO
        public byte uCodeRateLP; // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO

        public byte uGuardInterval;
                    // GUARD_INTERVAL_1_32,GUARD_INTERVAL_1_16,GUARD_INTERVAL_1_8,GUARD_INTERVAL_1_4,OFDM_AUTO

        public byte uTransmissionMode; // TRANSMISSION_MODE_2K, TRANSMISSION_MODE_8K, OFDM_AUTO
        public byte uHierarchyInfo; // HIERARCHY_NONE,HIERARCHY_1,HIERARCHY_2,HIERARCHY_4,OFDM_AUTO
      } ;

      public byte uNumberOfValidPids; // 1-16
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public short[] uPids;
    }

    public struct FireDTV_SIGNAL_STRENGTH
    {
      public ulong uSignalStrength;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FireDTV_FIRMWARE_VERSION
    {
      public byte uHWMajor;
      public byte uHWMiddle;
      public byte uHWMinor;
      public byte uSWMajor;
      public byte uSWMiddle;
      public byte uSWMinor;
      public byte uBuildNrMSB;
      public byte uBuildNrLSB;
    }

    public struct FireDTV_FRONTEND_STATUS
    {
      public ulong uFrequency; //kHz
      public ulong uBer; //10-6
      public byte uSignalStrength; //0-100%
      public bool bLock;

      public short uCNRatio;
      public byte uAGC;
      public byte uValue2;
      public byte uStatusFlags;
      public short uCIFlags;

      public byte uSupplyVoltage;
      public byte uAntennaVoltage;
      public byte uBusVoltage;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FireDTV_SYSTEM_INFO
    {
      public byte uNrAntennas; //0-3
      public byte uAntennaInfo; //ANTENNA_FIX, ANTENNA_MOVABLE, ANTENNA_MOBIL
      public byte uSystem; // 
      public byte uTransport; //TRANSPORT_SATELLITE, TRANSPORT_CABLE, TRANSPORT_TERRESTRIAL
      public bool bLists;
    }

    public struct FireDTV_LNB_CMD
    {
      public byte uVoltage;
      public byte uContTone;
      public byte uBurst;
      public byte uNrDiseqcCmds;

      public struct DiseqcCmd
      {
        public byte uLength;
        public byte uFraming;
        public byte uAddress;
        public byte uCommand;
        public byte[] Data;
      } ;
    }

    public struct FireDTV_FIRMWARE_UPDATE_STATUS
    {
      public byte uPercentageDone;
      public byte uLastAVCTransactionStatusCode;
      public byte uLastFirmwareStatusCode;
    }

    public struct FireDTV_LNB_PARAMETERS
    {
      public byte uNrAntennas;

      public struct LnbParameter
      {
        public byte uAntennaNr;
        public bool bEast;
        public short OrbitalPosition;
        public short LOF1; // MHz
        public short SwitchFreq; // MHz
        public short LOF2; // MHz
      } ;
    }

    public struct FireDTV_BOARD_TEMPERATURE
    {
      public byte[] uTemperature;
    }

    public struct FireDTV_TUNE_QPSK_PARAM
    {
      public ulong uFrequency; // 950.000 - 2.150.000 kHz
      public short uSymbolRate; // 1.000 - 40.000 kBaud
      public byte uFecInner; // FEC_AUTO, FEC_12,FEC_23, FEC_34, FEC_56, FEC_78
      public byte uPolarization; // POLARIZATION_HORIZONTAL, POLARIZATION_VERTICAL,POLARIZATION_NONE
      public bool bBand; // TRUE = HighBand FALSE = LowBand
    }

    public struct FireDTV_CI_DEBUG_ERROR_MSG
    {
      public byte MsgType;
      public byte MsgTextLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] MsgText;
    }

    public struct UTC_TIME
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] Date;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] Time;
    }

    #endregion
  }

  #region FireDTV Exception Class

  public class FireDTVException : Exception
  {
    public FireDTVException()
      : base()
    {
    }

    public FireDTVException(string message)
      : base(message)
    {
    }

    public FireDTVException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public FireDTVException(FireDTVConstants.FireDTVStatusCodes status, string message)
      : base()
    {
      friendlyMessage = message;
      statusCode = status;
    }


    private FireDTVConstants.FireDTVStatusCodes statusCode;
    private string friendlyMessage;

    public FireDTVConstants.FireDTVStatusCodes StatusCode
    {
      get { return statusCode; }
    }

    public override string Message
    {
      get
      {
        if (friendlyMessage != string.Empty)
        {
          return friendlyMessage;
        }
        else
        {
          switch (statusCode)
          {
            case FireDTVConstants.FireDTVStatusCodes.AlreadyInUse:
              return "DEVICE ALREADY IN USE!";


            case FireDTVConstants.FireDTVStatusCodes.Error:
              return "STATUS ERROR!";


            case FireDTVConstants.FireDTVStatusCodes.InvalidDeviceHandle:
              return "INVALID DEVICE HANDLE!";


            case FireDTVConstants.FireDTVStatusCodes.InvalidValue:
              return "INVALID VALUE!";


            case FireDTVConstants.FireDTVStatusCodes.NotSuppotedByTuner:
              return "NOT SUPPORTED BY TUNER!";


            default:
              return base.Message;
          }
        }
      }
    }
  }

  public class FireDTVInitializationException : FireDTVException
  {
    public FireDTVInitializationException()
      : base()
    {
    }

    public FireDTVInitializationException(string message)
      : base(message)
    {
    }

    public FireDTVInitializationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }

  public class FireDTVDeviceOpenException : FireDTVException
  {
    public FireDTVDeviceOpenException()
      : base()
    {
    }

    public FireDTVDeviceOpenException(string message)
      : base(message)
    {
    }

    public FireDTVDeviceOpenException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public FireDTVDeviceOpenException(FireDTVConstants.FireDTVStatusCodes status, string message)
      : base(status, message)
    {
    }
  }

  #endregion

  #region FireDTV Event Classes and Delegates.

  public class FireDTVEventArgs : EventArgs
  {
    #region Private Variables

    private FireDTVSourceFilterInfo _sourceFilter;

    #endregion

    public FireDTVEventArgs(FireDTVSourceFilterInfo sourceFilter)
      : base()
    {
      _sourceFilter = sourceFilter;
    }

    public FireDTVSourceFilterInfo SourceFilter
    {
      get { return _sourceFilter; }
    }
  }

  public class FireDTVRemoteControlEventArgs : FireDTVEventArgs
  {
    private FireDTVConstants.FireDTVRemoteControlKeyCodes keyValue;

    public FireDTVRemoteControlEventArgs(FireDTVSourceFilterInfo sourceFilter, IntPtr KeyValue)
      : base(sourceFilter)
    {
      keyValue = (FireDTVConstants.FireDTVRemoteControlKeyCodes) KeyValue.ToInt32();
    }

    public FireDTVConstants.FireDTVRemoteControlKeyCodes RemoteKey
    {
      get { return keyValue; }
    }
  }

  public delegate void FireDTVEventHandler(object sender, FireDTVEventArgs e);

  public delegate void FireDTVRemoteControlEventHandler(object sender, FireDTVRemoteControlEventArgs e);

  #endregion

  /// <summary>
  /// Summary description for FireDTVDefines.
  /// </summary>
  public class FireDTVAPI
  {
    #region FireDTV API Imports

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_Initialize@@YAKXZ",
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_Initialize();

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_RegisterGeneralNotifications@@YAKPAUHWND__@@@Z",
      SetLastError = true,
      CharSet = CharSet.Unicode,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_RegisterGeneralNotifications(int hWnd);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetNumberOfWDMDevices@@YAKPAK@Z",
      SetLastError = true,
      CharSet = CharSet.Unicode,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetNumberOfWDMDevices(out uint puNumberOfWDMDevices);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetNumberOfBDADevices@@YAKPAK@Z",
      SetLastError = true,
      CharSet = CharSet.Unicode,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetNumberOfBDADevices(out uint puNumberOfWDMDevices);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetNumberOfDevices@@YAKPAK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetNumberOfDevices(IntPtr puNumberOfDevices);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_OpenWDMDeviceHandle@@YAKKPAK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_OpenWDMDeviceHandle(uint uWDMDeviceNumber, out uint pDeviceHandle);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_OpenBDADeviceHandle@@YAKKPAK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_OpenBDADeviceHandle(uint uBDADeviceNumber, out uint DeviceHandle);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_OpenDeviceHandle@@YAKKPAK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_OpenDeviceHandle(uint uDeviceNumber, out uint DeviceHandle);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_CloseDeviceHandle@@YAKK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_CloseDeviceHandle(uint DeviceHandle);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetTunerType@@YAKKPAW4FireDTV_TUNER_TYPE@@@Z",
      SetLastError = true,
      CharSet = CharSet.Unicode,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetTunerType(uint DeviceHandle, ref FireDTVConstants.FireDTVTunerType TunerType);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetSubunitID@@YAKKPAI@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetSubunitID(uint DeviceHandle, IntPtr puSubunitID);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_RegisterNotifications@@YAKKPAUHWND__@@@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_RegisterNotifications(uint DeviceHandle, int hWnd);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_UnregisterNotifications@@YAKK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_UnregisterNotifications(uint DeviceHandle);

    /*
          [DllImport("FireSATApi.dll", 
            EntryPoint="?FS_GetDirectShowFilter@@YAKKPAPAUIBaseFilter@@@Z",  
            SetLastError=true,
            CharSet=CharSet.Unicode, 
            ExactSpelling=true,
            CallingConvention=CallingConvention.StdCall)]
        public unsafe static extern ulong FS_GetDirectShowFilter(ulong	DeviceHandle,DirectShowHelperLib.IBaseFilter** ppFilter);

          [DllImport("FireSATApi.dll", 
            EntryPoint="?FS_GetReceiverDirectShowFilter@@YAKKPAPAUIBaseFilter@@@Z",  
            SetLastError=true,
            CharSet=CharSet.Unicode, 
            ExactSpelling=true,
            CallingConvention=CallingConvention.StdCall)]
        public unsafe static extern ulong FS_GetReceiverDirectShowFilter(ulong DeviceHandle,DirectShowHelperLib.IBaseFilter** ppFilter);
        */

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetApiVersion@@YAPADXZ",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr FS_GetApiVersion();

    /*
        [DllImport("FireSATApi.dll", 
          EntryPoint="?FS_GetGUID@@YAKKPAU_FireDTV_GUID@@@Z",  
          SetLastError=true,
          CharSet=CharSet.Unicode, 
          ExactSpelling=true,
          CallingConvention=CallingConvention.StdCall)]
        public unsafe static extern ulong FS_GetGUID(ulong DeviceHandle,FireDTVConstants.FireDTV_GUID*	pGUID);
        */

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetFriendlyString@@YAKKPAPAD@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetFriendlyString(uint deviceHandle, out string friendlyName);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetDisplayString@@YAKKPAD@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetDisplayString(uint DeviceHandle, StringBuilder strDisplayName);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetDeviceString@@YAKKPAD@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetDeviceString(uint DeviceHandle, StringBuilder strDeviceName);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetGUIDString@@YAKKPAD@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetGUIDString(uint DeviceHandle, StringBuilder strGUIDName);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_IsDeviceAttached@@YAHK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern bool FS_IsDeviceAttached(uint DeviceHandle);


    [DllImport("FiresatApi.dll",
      EntryPoint = "?FS_GetDriverVersion@@YAKKPAU_FIRESAT_DRIVER_VERSION@@@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetDriverVersion(uint DeviceHandle,
                                                  ref FireDTVConstants.FireDTV_DRIVER_VERSION pDriverVersion);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_GetFirmwareVersion@@YAKKPAU_FIRESAT_FIRMWARE_VERSION@@@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetFirmwareVersion(uint DeviceHandle,
                                                    ref FireDTVConstants.FireDTV_FIRMWARE_VERSION Version);

    /*
		

            [DllImport("FireSATApi.dll", 
               EntryPoint="?FS_SelectMultiplex_DVBS@@YAKKPAU_FireDTV_SELECT_MULTIPLEX_DVBS@@@Z",  
               SetLastError=true,
               CharSet=CharSet.Unicode, 
               ExactSpelling=true,
               CallingConvention=CallingConvention.StdCall)]
            public  static extern ulong FS_SelectMultiplex_DVBS(ulong	DeviceHandle,FireDTVConstants.FireDTV_SELECT_MULTIPLEX_DVBS* pMultiplex);

            [DllImport("FireSATApi.dll", 
              EntryPoint="?FS_SelectMultiplex_DVBT@@YAKKPAU_FireDTV_SELECT_MULTIPLEX_DVBT@@@Z",  
              SetLastError=true,
              CharSet=CharSet.Unicode, ExactSpelling=true,
              CallingConvention=CallingConvention.StdCall)]
            public  static extern ulong  FS_SelectMultiplex_DVBT(ulong DeviceHandle,FireDTVConstants.FireDTV_SELECT_MULTIPLEX_DVBT* pMultiplex);

		
            [DllImport("FireSATApi.dll", 
              EntryPoint="?FS_SelectPids_DVBS@@YAKKPAU_FireDTV_SELECT_PIDS_S@@@Z",  
              SetLastError=true,
              CharSet=CharSet.Unicode, 
              ExactSpelling=true,
              CallingConvention=CallingConvention.StdCall)]
            public unsafe static extern ulong FS_SelectPids_DVBS(ulong DeviceHandle,FireDTVConstants.FireDTV_SELECT_PIDS_DVBS*	pPids);

		
            [DllImport("FireSATApi.dll", 
               EntryPoint="?FS_SelectPids_DVBT@@YAKKPAU_FireDTV_SELECT_PIDS_DVBT@@@Z",  
               SetLastError=true,
               CharSet=CharSet.Unicode, 
               ExactSpelling=true,
               CallingConvention=CallingConvention.StdCall)]
            public  static extern ulong  FS_SelectPids_DVBT(ulong	DeviceHandle,FireDTVConstants.FireDTV_SELECT_MULTIPLEX_DVBT* pPids);

				
            [DllImport("FireSATApi.dll", 
               EntryPoint="?FS_GetFrontendStatus@@YAKKPAU_FireDTV_FRONTEND_STATUS@@@Z",  
               SetLastError=true,
               CharSet=CharSet.Unicode, 
               ExactSpelling=true,
               CallingConvention=CallingConvention.StdCall)]
            public  static extern ulong FS_GetFrontendStatus(ulong DeviceHandle,FireDTVConstants.FireDTV_FRONTEND_STATUS*	pStatus);
		
		
            [DllImport("FireSATApi.dll", 
               EntryPoint="?FS_SendLnbCmd@@YAKKPAU_FireDTV_LNB_CMD@@@Z",  
               SetLastError=true,
               CharSet=CharSet.Unicode, 
               ExactSpelling=true,
               CallingConvention=CallingConvention.StdCall)]
            public  static extern ulong FS_SendLnbCmd(ulong DeviceHandle,FireDTVConstants.FireDTV_LNB_CMD* pLnbCmd);
            */

    [DllImport("FiresatApi.dll",
      EntryPoint = "?FS_GetSystemInfo@@YAKKPAU_FIRESAT_SYSTEM_INFO@@@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_GetSystemInfo(uint DeviceHandle, ref FireDTVConstants.FireDTV_SYSTEM_INFO pSystemInfo);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_SetPowerState@@YAKKE@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_SetPowerState(uint DeviceHandle, byte uPowerState);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_SetAutoTune@@YAKKH@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_SetAutoTune(uint DeviceHandle, bool bAutoTune);

    /*
        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_GetFirmwareUpdateStatus@@YAKKPAU_FireDTV_FIRMWARE_UPDATE_STATUS@@@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_GetFirmwareUpdateStatus(ulong DeviceHandle,FireDTVConstants.FireDTV_FIRMWARE_UPDATE_STATUS* pStatus);
		
        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_UpdateFirmware@@YAKKQAE@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_UpdateFirmware(ulong DeviceHandle,
            //[MarshalAs (UnmanagedType.ByValArray, SizeConst=FireDTVConstants.FIRMWARE_HEXDATASIZE)]	
            byte[]	HexData);
		
        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_SetLNBParameter@@YAKKPAU_FireDTV_LNB_PARAMETERS@@@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public static extern ulong FS_SetLNBParameter(ulong DeviceHandle,FireDTVConstants.FireDTV_LNB_PARAMETERS* pParam);

        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_GetLNBParameter@@YAKKPAU_FireDTV_LNB_PARAMETERS@@@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_GetLNBParameter(ulong DeviceHandle,FireDTVConstants.FireDTV_LNB_PARAMETERS*	pParam);

		
          [DllImport("FireSATApi.dll", 
            EntryPoint="?FS_GetBoardTemperature@@YAKKPAU_FireDTV_BOARD_TEMPERATURE@@@Z",  
            SetLastError=true,
            CharSet=CharSet.Unicode, ExactSpelling=true,
            CallingConvention=CallingConvention.StdCall)]
        public unsafe static extern ulong FS_GetBoardTemperature(ulong	DeviceHandle,
        FireDTVConstants.FireDTV_BOARD_TEMPERATURE*	pTemperature);
		
        [DllImport("FireSATApi.dll", 
              EntryPoint="?FS_TuneQPSK@@YAKKPAU_FireDTV_TUNE_QPSK_PARAM@@@Z",  
              SetLastError=true,
              CharSet=CharSet.Unicode, ExactSpelling=true,
              CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_TuneQPSK(ulong	DeviceHandle,FireDTVConstants.FireDTV_TUNE_QPSK_PARAM* pTuneQpskParam);
        */

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_CI_Open@@YAKK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_CI_Open(uint DeviceHandle);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_CI_Close@@YAKK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_CI_Close(uint DeviceHandle);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_CI_SetPollingIntervall@@YAKKK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_CI_SetPollingIntervall(uint DeviceHandle, uint uPollingIntervall);

    /*
        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_CI_SendPMT@@YAKKGPAE@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_CI_SendPMT(ulong DeviceHandle,ushort uLength,Byte* pData);

        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_CI_GetApplicationInfo@@YAKKPAG0PAE1@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_CI_GetApplicationInfo( ulong DeviceHandle,
                                      ushort* pAppManufacturer, 
                                      ushort* pManufacturerCode, 
                                      byte* pLength, 
                                      byte* pText);

        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_CI_EnterMenu@@YAKK@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_CI_EnterMenu(ulong	DeviceHandle);

        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_CI_CloseMMI@@YAKK@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_CI_CloseMMI(ulong DeviceHandle);

        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_CI_GetMMIObject@@YAKKPAGPAKPAE@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_CI_GetMMIObject(ulong DeviceHandle,
                                  ushort* pLength, 
                                  ulong* pMMI_Tag,
                                  byte* pData);

        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_CI_SendMenuAnswer@@YAKKE@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_CI_SendMenuAnswer(ulong DeviceHandle,byte Choice);

        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_CI_SendEnqAnswer@@YAKKEEPAE@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_CI_SendEnqAnswer(	ulong DeviceHandle,
                                    byte AnswerID, 
                                    byte Length, 
                                    byte* pText);

        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_CI_GetDateTimeInfo@@YAKKPAE@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_CI_GetDateTimeInfo( ulong DeviceHandle,byte* pResponseInterval);

        [DllImport("FireSATApi.dll", 
           EntryPoint="?FS_CI_SetDateTimeInfo@@YAKKU_UTC_TIME@@G@Z",  
           SetLastError=true,
           CharSet=CharSet.Unicode, 
           ExactSpelling=true,
           CallingConvention=CallingConvention.StdCall)]
        public  static extern ulong FS_CI_SetDateTimeInfo(ulong DeviceHandle,
                                    FireDTVConstants.UTC_TIME	Time,
                                    double LocalOffset);
        */

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_RemoteControl_Start@@YAKK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_RemoteControl_Start(uint DeviceHandle);

    [DllImport("FireSATApi.dll",
      EntryPoint = "?FS_RemoteControl_Stop@@YAKK@Z",
      SetLastError = true,
      CharSet = CharSet.Ansi,
      ExactSpelling = true,
      PreserveSig = false,
      CallingConvention = CallingConvention.StdCall)]
    public static extern uint FS_RemoteControl_Stop(uint DeviceHandle);

    #endregion

    #region Win32 API Imports

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    public static extern int GetActiveWindow();

    [DllImport("user32.dll")]
    public static extern int GetWindowText(int hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    public static extern int GetWindowThreadProcessId(int hwnd, ref int lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(int hWnd);

    #endregion
  } ;
}