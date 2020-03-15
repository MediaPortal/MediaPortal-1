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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvLibrary;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Implementations.DVB;
using TvDatabase;


namespace TvLibrary.Implementations.DVB
{


    /// <summary>
    /// Support for TBS Cards
    /// </summary>
    public class Turbosight : IDiSEqCController, ICiMenuActions, IDisposable
    {

        #region Invoke imports

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibraryEx(string dllFilePath, IntPtr hFile, LoadLibraryFlags dwFlags);
        [DllImport("kernel32.dll")]
        public extern static bool FreeLibrary(IntPtr dllPointer);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public extern static IntPtr GetProcAddress(IntPtr dllPointer, string functionName);
        //We need to define api.dll for every tuner.

        #endregion

        #region Invoke delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool DelCamavailable(IntPtr handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelOn_Exit_CI(IntPtr handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr DelOn_Start_CI(IBaseFilter Filter, [MarshalAs(UnmanagedType.LPWStr)] string tuner_name, uint iDeviceIndex);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelTBS_ci_MMI_Process(IntPtr handle, IntPtr command, IntPtr response);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DelTBS_ci_SendPmt(IntPtr handle, IntPtr pmt, ushort pmtLength);

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
        private struct UsbIrCommand
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            private byte[] Reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Codes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xf4)]
            private byte[] Reserved2;
        }

        private struct BDA_NBC_PARAMS
        {
            public int rolloff;
            public int pilot;
            public int dvbtype;         // 1 for dvbs 2 for dvbs2 0 for auto
            public int fecrate;
            public int modtype;
        }

        // MP internal message holder - purely for convenience.
        private struct MmiMessage
        {
            public TbsMmiMessageType Type;
            public Int32 Length;
            public byte[] Message;

            public MmiMessage(TbsMmiMessageType type, Int32 length)
            {
                Type = type;
                Length = length;
                Message = new byte[length];
            }

            public MmiMessage(TbsMmiMessageType type)
            {
                Type = type;
                Length = 0;
                Message = null;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
        private struct IrCommand
        {
            public uint Address;
            public uint Command;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
        private struct NbcTuningParams
        {
            public Turbosight.TbsRollOff RollOff;
            public Turbosight.TbsPilot Pilot;
            public Turbosight.TbsDvbsStandard DvbsStandard;
            public BinaryConvolutionCodeRate InnerFecRate;
            public ModulationType ModulationType;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
        private struct TbsAccessParams
        {
            public Turbosight.TbsAccessMode AccessMode;
            public Turbosight.TbsTone Tone;
            private uint Reserved1;
            public Turbosight.TbsLnbPower LnbPower;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x80)]
            public byte[] DiseqcTransmitMessage;
            public uint DiseqcTransmitMessageLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x80)]
            public byte[] DiseqcReceiveMessage;
            public uint DiseqcReceiveMessageLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
            public byte[] Reserved2;
        }

        #endregion

        #region Enums

        [System.Flags]
        enum LoadLibraryFlags : uint
        {
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
        }
        // Common properties that can be controlled on all TBS products.
        private enum TbsAccessMode : uint
        {
            LnbPower = 0,       // Control the LNB power supply.
            Diseqc = 1,             // Send and receive DiSEqC messages.
            Tone = 2                // Control the 22 kHz oscillator state.
        }

        private enum TbsDvbsStandard : uint
        {
            Auto = 0,
            Dvbs = 1,
            Dvbs2 = 2
        }

        private enum TbsIrCode : byte
        {
            Down1 = 0x88,
            Down2 = 150,
            Eight = 0x8e,
            Epg = 0x97,
            Exit = 0x9f,
            Five = 0x8a,
            Four = 0x8b,
            FullScreen = 0x9d,
            Info = 0x9c,
            Left1 = 140,
            Left2 = 0x90,
            Menu = 0x9e,
            Mute = 0x94,
            Nine = 0x8d,
            Ok = 0x99,
            One = 0x87,
            Pause = 0x98,
            Play = 0x9b,
            Power = 0x84,
            Recall = 0x80,
            Record = 0x83,
            Right1 = 130,
            Right2 = 0x93,
            Seven = 0x8f,
            Six = 0x89,
            Snapshot = 0x9a,
            Tab = 0x95,
            Three = 0x85,
            Two = 0x86,
            Up1 = 0x81,
            Up2 = 0x91,
            Zero = 0x92
        }

        private enum TbsIrProperty
        {
            Codes = 0,
            ReceiverCommand = 1
        }

        private enum TbsIrReceiverCommand : byte
        {
            Flush = 3,
            Start = 1,
            Stop = 2
        }

        private enum TbsLnbPower : uint
        {
            Off = 0,
            High = 1,               // 18 V - linear horizontal, circular left.
            Low = 2,                // 13 V - linear vertical, circular right.
            On = 3                  // Power on using the previous voltage.
        }

        private enum TbsMmiMessageType : byte
        {
            Null = 0,
            ApplicationInfo = 0x01,         // PC <-->
            CaInfo = 0x02,                  // PC <-->
            //CaPmt = 0x03,                 // PC -->
            //CaPmtReply = 0x04,            // PC <--
            DateTimeEnquiry = 0x05,         // PC <--
            //DateTime = 0x06,              // PC -->
            Enquiry = 0x07,                 // PC <--
            Answer = 0x08,                  // PC -->
            EnterMenu = 0x09,               // PC -->
            Menu = 0x0a,                    // PC <--
            MenuAnswer = 0x0b,              // PC -->
            List = 0x0c,                    // PC <--
            GetMmi = 0x0d,                  // PC <--
            CloseMmi = 0x0e,                // PC -->
            //DateTimeMode = 0x10,          // PC -->
            //SetDateTime = 0x12            // PC <--
        }

        private enum TbsPilot : uint
        {
            Off = 0,
            On = 1,
            Unknown = 2     // (not used...)
        }

        private enum TbsRollOff : uint
        {
            Undefined = 0xff,
            Twenty = 0,           // 0.2
            TwentyFive = 1,           // 0.25
            ThirtyFive = 2            // 0.35
        }

        private enum TbsTone : uint
        {
            Off = 0,
            On = 1,                     // Continuous tone on.
            BurstUnmodulated = 2,       // Simple DiSEqC port A (tone burst).
            BurstModulated = 3           // Simple DiSEqC port B (data burst).
        }

        // USB (QBOX) only.
        private enum UsbBdaExtensionProperty
        {
            Reserved = 0,
            Ir = 1,             // Property for retrieving IR codes from the device's IR receiver.
            CiAccess = 8,       // Property for interacting with the CI slot.
            BlindScan = 9,      // Property for accessing and controlling the hardware blind scan capabilities.
            TbsAccess = 18      // TBS property for enabling control of the common properties in the TbsAccessMode enum.
        }

        // PCIe/PCI only.
        private enum BdaExtensionProperty
        {
            Reserved = 0,
            NbcParams = 10,     // Property for setting DVB-S2 parameters that could not initially be set through BDA interfaces.
            BlindScan = 11,     // Property for accessing and controlling the hardware blind scan capabilities.
            CiAccess = 18,      // Property for interacting with the CI slot.
            TbsAccess = 21      // TBS property for enabling control of the common properties in the BdaExtensionCommand enum.
        }

        public enum MmiApplicationType : byte
        {
            /// <summary>
            /// Conditional access application.
            /// </summary>
            ConditionalAccess = 1,
            /// <summary>
            /// Electronic programme guide application.
            /// </summary>
            ElectronicProgrammeGuide
        }

        /// <summary>
        /// DVB MMI enquiry answer response types.
        /// </summary>
        public enum MmiResponseType : byte
        {
            /// <summary>
            /// The response is a cancel request.
            /// </summary>
            Cancel = 0,
            /// <summary>
            /// The response contains an answer from the user.
            /// </summary>
            Answer
        }

        /// <summary>
        /// DVB MMI close MMI command types.
        /// </summary>
        public enum MmiCloseType : byte
        {
            /// <summary>
            /// The MMI dialog should be closed immediately.
            /// </summary>
            Immediate = 0,
            /// <summary>
            /// The MMI dialog should be closed after a [short] delay.
            /// </summary>
            Delayed
        }

        /// <summary>
        /// DVB MMI message tags.
        /// </summary>
        public enum MmiTag
        {
            /// <summary>
            /// Unknown tag.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Profile enquiry.
            /// </summary>
            ProfileEnquiry = 0x9f8010,
            /// <summary>
            /// Profile.
            /// </summary>
            Profile,
            /// <summary>
            /// Profile change.
            /// </summary>
            ProfileChange,

            /// <summary>
            /// Application information enquiry.
            /// </summary>
            ApplicationInfoEnquiry = 0x9f8020,
            /// <summary>
            /// Application information.
            /// </summary>
            ApplicationInfo,
            /// <summary>
            /// Enter menu.
            /// </summary>
            EnterMenu,

            /// <summary>
            /// Conditional access information enquiry.
            /// </summary>
            ConditionalAccessInfoEnquiry = 0x9f8030,
            /// <summary>
            /// Conditional access information.
            /// </summary>
            ConditionalAccessInfo,
            /// <summary>
            /// Conditional access information programme map table.
            /// </summary>
            ConditionalAccessPmt,
            /// <summary>
            /// Conditional access information programme map table response.
            /// </summary>
            ConditionalAccessPmtResponse,

            /// <summary>
            /// Tune.
            /// </summary>
            Tune = 0x9f8400,
            /// <summary>
            /// Replace.
            /// </summary>
            Replace,
            /// <summary>
            /// Clear replace.
            /// </summary>
            ClearReplace,
            /// <summary>
            /// Ask release.
            /// </summary>
            AskRelease,

            /// <summary>
            /// Date/time enquiry.
            /// </summary>
            DateTimeEnquiry = 0x9f8440,
            /// <summary>
            /// Date/time.
            /// </summary>
            DateTime,

            /// <summary>
            /// Close man-machine interface.
            /// </summary>
            CloseMmi = 0x9f8800,
            /// <summary>
            /// Display control.
            /// </summary>
            DisplayControl,
            /// <summary>
            /// Display reply.
            /// </summary>
            DisplayReply,
            /// <summary>
            /// Text - last.
            /// </summary>
            TextLast,
            /// <summary>
            /// Text - more.
            /// </summary>
            TextMore,
            /// <summary>
            /// Keypad control.
            /// </summary>
            KeypadControl,
            /// <summary>
            /// Key press.
            /// </summary>
            KeyPress,
            /// <summary>
            /// Enquiry.
            /// </summary>
            Enquiry,
            /// <summary>
            /// Answer.
            /// </summary>
            Answer,
            /// <summary>
            /// Menu - last.
            /// </summary>
            MenuLast,
            /// <summary>
            /// Menu - more.
            /// </summary>
            MenuMore,
            /// <summary>
            /// Menu answer.
            /// </summary>
            MenuAnswer,
            /// <summary>
            /// List - last.
            /// </summary>
            ListLast,
            /// <summary>
            /// List - more.
            /// </summary>
            ListMore,
            /// <summary>
            /// Subtitle segment - last.
            /// </summary>
            SubtitleSegmentLast,
            /// <summary>
            /// Subtitle segment - more.
            /// </summary>
            SubtitleSegmentMore,
            /// <summary>
            /// Display message.
            /// </summary>
            DisplayMessage,
            /// <summary>
            /// Scene end mark.
            /// </summary>
            SceneEndMark,
            /// <summary>
            /// Scene done.
            /// </summary>
            SceneDone,
            /// <summary>
            /// Scene control.
            /// </summary>
            SceneControl,
            /// <summary>
            /// Subtitle download - last.
            /// </summary>
            SubtitleDownloadLast,
            /// <summary>
            /// Subtitle download - more.
            /// </summary>
            SubtitleDownloadMore,
            /// <summary>
            /// Flush download.
            /// </summary>
            FlushDownload,
            /// <summary>
            /// Download reply.
            /// </summary>
            DownloadReply,

            /// <summary>
            /// Communication command.
            /// </summary>
            CommsCommand = 0x9f8c00,
            /// <summary>
            /// Connection descriptor.
            /// </summary>
            ConnectionDescriptor,
            /// <summary>
            /// Communication reply.
            /// </summary>
            CommsReply,
            /// <summary>
            /// Communication send - last.
            /// </summary>
            CommsSendLast,
            /// <summary>
            /// Communication send - more.
            /// </summary>
            CommsSendMore,
            /// <summary>
            /// Communication receive - last.
            /// </summary>
            CommsReceiveLast,
            /// <summary>
            /// Communication receive - more.
            /// </summary>
            CommsReceiveMore
        }

        public enum ToneBurst
        {
            None,
            ToneBurst,
            DataBurst
        }

        public enum Tone22k
        {
            Off,
            On,
            Auto
        }

        #endregion

        #region Fields

        private IntPtr _ciHandle = IntPtr.Zero;
        private ICiMenuCallbacks _ciMenuCallbacks;
        private IntPtr _generalBuffer = IntPtr.Zero;
        private bool _isCamPresent;
        private bool _isCamReady;
        private bool _isCiSlotPresent;
        private bool _isTurbosight;
        private bool _isUsb;
        private Thread _mmiHandlerThread;
        private IntPtr _mmiMessageBuffer = IntPtr.Zero;
        private List<MmiMessage> _mmiMessageQueue;
        private IntPtr _mmiResponseBuffer = IntPtr.Zero;
        private IntPtr _pmtBuffer = IntPtr.Zero;
        private IKsPropertySet _propertySet;
        private Guid _propertySetGuid = Guid.Empty;
        private bool _stopMmiHandlerThread;
        private int _tbsAccessProperty;
        private IBaseFilter _tunerFilter;
        private string _tunerFilterName;
        private static readonly Guid BdaExtensionPropertySet = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 110, 0xc9);
        private static readonly Guid IrPropertySet = new Guid(0xb51c4994, 0x54, 0x4749, 130, 0x43, 2, 0x9a, 0x66, 0x86, 0x36, 0x36);
        private const int MaxDiseqcMessageLength = 0x80;
        private const int MaxPmtLength = 0x400;
        private const int MmiMessageBufferSize = 0x200;
        private const int MmiResponseBufferSize = 0x800;
        private const int NbcTuningParamsSize = 20;
        private const int TbsAccessParamsSize = 0x218;
        private static readonly string[] TunersWithCiSlots = new string[] { "TBS DVBC Tuner",
            "TBS 5880 DVB-T/T2 Tuner",
            "TBS 5880 DVBC Tuner",
            "TBS 5881 DVB-T/T2 Tuner",
            "TBS 5881 DVBC Tuner",
            "TBS 5680 DVBC Tuner",
            "TBS 5980 CI Tuner", 
            "TBS 5990 DVBS/S2 Tuner A",
            "TBS 5990 DVBS/S2 Tuner B",
            "TBS 6290 DVBT/T2 Tuner A",
            "TBS 6290 DVBT/T2 Tuner B",
            "TBS 6290 DVBC Tuner A",
            "TBS 6290 DVBC Tuner B",
            "TBS 6680 BDA DVBC Tuner A",
            "TBS 6680 BDA DVBC Tuner B",
            "TBS 6928 DVBS/S2 Tuner", 
            "TBS 6991 DVBS/S2 Tuner A",
            "TBS 6991 DVBS/S2 Tuner B",
            "TBS 6992 DVBS/S2 Tuner A",
            "TBS 6992 DVBS/S2 Tuner B",
            "TBS 6618 BDA DVBC Tuner" };

        private static readonly Guid UsbBdaExtensionPropertySet = new Guid(0xc6efe5eb, 0x855a, 0x4f1b, 0xb7, 170, 0x87, 0xb5, 0xe1, 220, 0x41, 0x13);

        private const int MmiHandlerThreadSleepTime = 2000;   // unit = ms
        private const int TbsNBCParamsSize = 20;
        private uint _deviceIndex;
        private IntPtr _apiLibraryHandle;
        private string _apiFileName;

        #endregion

        #region Delegates implementation

        private bool Camavailable(IntPtr handle)
        {
            if (_apiLibraryHandle == IntPtr.Zero)
            {
                LoadCIApi();
            }
            IntPtr pApi = GetProcAddress(_apiLibraryHandle, "Camavailable");
            if (pApi == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: unable to invoke Camavailable");
                return false;
            }
            DelCamavailable fdel = (DelCamavailable)Marshal.GetDelegateForFunctionPointer(
                pApi,
                typeof(DelCamavailable));
            return fdel(handle);
        }

        private void On_Exit_CI(IntPtr handle)
        {
            if (_apiLibraryHandle == IntPtr.Zero)
            {
                LoadCIApi();
            }
            IntPtr pApi = GetProcAddress(_apiLibraryHandle, "On_Exit_CI");
            if (pApi == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: unable to invoke On_Exit_CI");
                return;
            }
            DelOn_Exit_CI fdel = (DelOn_Exit_CI)Marshal.GetDelegateForFunctionPointer(
                pApi,
                typeof(DelOn_Exit_CI));
            fdel(handle);
        }

        private IntPtr On_Start_CI(IBaseFilter Filter, [MarshalAs(UnmanagedType.LPWStr)] string tuner_name, uint iDeviceIndex)
        {
            if (_apiLibraryHandle == IntPtr.Zero)
            {
                LoadCIApi();
            }
            IntPtr pApi = GetProcAddress(_apiLibraryHandle, "On_Start_CI");
            if (pApi == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: unable to invoke On_Start_CI");
                return IntPtr.Zero;
            }
            DelOn_Start_CI fdel = (DelOn_Start_CI)Marshal.GetDelegateForFunctionPointer(
                pApi,
                typeof(DelOn_Start_CI));
            return fdel(Filter, tuner_name, iDeviceIndex);
        }

        private void TBS_ci_MMI_Process(IntPtr handle, IntPtr command, IntPtr response)
        {
            if (_apiLibraryHandle == IntPtr.Zero)
            {
                LoadCIApi();
            }
            IntPtr pApi = GetProcAddress(_apiLibraryHandle, "TBS_ci_MMI_Process");
            if (pApi == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: unable to invoke TBS_ci_MMI_Process");
                return;
            }
            DelTBS_ci_MMI_Process fdel = (DelTBS_ci_MMI_Process)Marshal.GetDelegateForFunctionPointer(
                pApi,
                typeof(DelTBS_ci_MMI_Process));
            fdel(handle, command, response);
        }

        private void TBS_ci_SendPmt(IntPtr handle, IntPtr pmt, ushort pmtLength)
        {
            if (_apiLibraryHandle == IntPtr.Zero)
            {
                LoadCIApi();
            }
            IntPtr pApi = GetProcAddress(_apiLibraryHandle, "TBS_ci_SendPmt");
            if (pApi == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: unable to invoke TBS_ci_SendPmt");
                return;
            }
            DelTBS_ci_SendPmt fdel = (DelTBS_ci_SendPmt)Marshal.GetDelegateForFunctionPointer(
                pApi,
                typeof(DelTBS_ci_SendPmt));
            fdel(handle, pmt, pmtLength);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Turbosight constructor
        /// </summary>
        /// <param name="tunerFilter"></param>
        /// <param name="deviceIndex"></param>
        public Turbosight(IBaseFilter tunerFilter, uint deviceIndex)
        {
            if (tunerFilter == null)
            {
                TvLibrary.Log.Log.Debug("Turbosight: tuner filter is null", new object[0]);
            }
            else
            {
                this._tunerFilterName = FilterGraphTools.GetFilterName(tunerFilter);
                _deviceIndex = deviceIndex;

                if ((this._tunerFilterName == null) || (!this._tunerFilterName.StartsWith("TBS") && !this._tunerFilterName.StartsWith("QBOX")))
                {
                    TvLibrary.Log.Log.Debug("Turbosight: tuner filter name does not match", new object[0]);
                }
                else
                {
                    KSPropertySupport support;
                    this._propertySet = tunerFilter as IKsPropertySet;
                    if ((this._propertySet != null) && (this._propertySet.QuerySupported(UsbBdaExtensionPropertySet, 1, out support) == 0))
                    {
                        TvLibrary.Log.Log.Debug("Turbosight: supported tuner detected (USB interface)", new object[0]);
                        this._isTurbosight = true;
                        this._isUsb = true;
                        this._propertySetGuid = UsbBdaExtensionPropertySet;
                        this._tbsAccessProperty = 0x12;
                    }
                    if (!this._isTurbosight)
                    {
                        IPin o = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
                        this._propertySet = o as IKsPropertySet;
                        if ((this._propertySet != null) && (this._propertySet.QuerySupported(BdaExtensionPropertySet, 0x15, out support) == 0))
                        {
                            TvLibrary.Log.Log.Debug("Turbosight: supported tuner detected (PCIe/PCI interface)", new object[0]);
                            this._isTurbosight = true;
                            this._isUsb = false;
                            this._propertySetGuid = BdaExtensionPropertySet;
                            this._tbsAccessProperty = 0x15;
                        }
                        if ((o != null) && !this._isTurbosight)
                        {
                            Release.ComObject(o);
                        }
                    }
                    if (this._isTurbosight)
                    {
                        this._tunerFilter = tunerFilter;
                        this._generalBuffer = Marshal.AllocCoTaskMem(0x218);
                        _deviceIndex = deviceIndex;
                        this.OpenCi();
                        this.SetPowerState(true);
                    }
                }
            }
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Returns deviceindex for card.
        /// Just by sorting cards by devicepath
        /// </summary>
        /// <param name="TvCard"></param>
        /// <returns></returns>
        public static int GetDeviceIndex(TvCardBase TvCard)
        {
            return Server.ListAll()
                .SelectMany(x => x.ReferringCard())
                .ToList()
                .Where(x => x.Name.StartsWith("TBS"))
                .OrderBy(x => x.DevicePath)
                .Select((card, index) => new { card = card, index = index })
                .Where(x => x.card.DevicePath == TvCard.DevicePath)
                .Select(x => x.index).FirstOrDefault();
        }

        #endregion

        #region Private methods

        #region Dynamic DLL handling

        private void LoadCIApi()
        {
            _apiFileName = string.Format("TBSCiApi{0}.dll", _deviceIndex);
            if (!File.Exists(_apiFileName))
            {
                File.Copy("TBSCIApi.dll", _apiFileName);
            }
            _apiLibraryHandle = LoadLibraryEx(
                _apiFileName,
                IntPtr.Zero,
                (LoadLibraryFlags)0);
            if (_apiLibraryHandle == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug(string.Format("Turbosight: unable to load {0}", _apiFileName), new object[0]);
            }
        }

        #endregion

        private void HandleApplicationInformation(byte[] content, int length)
        {
            TvLibrary.Log.Log.Debug("Turbosight: application information", new object[0]);
            if (length < 5)
            {
                TvLibrary.Log.Log.Debug("Turbosight: error, response too short", new object[0]);
                DVB_MMI.DumpBinary(content, 0, length);
            }
            else
            {
                MmiApplicationType type = (MmiApplicationType)content[0];
                //DVB_MMI.ApplicationType type = (DVB_MMI.ApplicationType)content[0];
                String title = System.Text.Encoding.ASCII.GetString(content, 5, length - 5);
                TvLibrary.Log.Log.Debug("  type         = {0}", type);
                TvLibrary.Log.Log.Debug("  manufacturer = 0x{0:x}{1:x}", content[1], content[2]);
                TvLibrary.Log.Log.Debug("  code         = 0x{0:x}{1:x}", content[3], content[4]);
                TvLibrary.Log.Log.Debug("  menu title   = {0}", title);
            }
        }

        private void HandleCaInformation(byte[] content, int length)
        {
            TvLibrary.Log.Log.Debug("Turbosight: conditional access information", new object[0]);
            if (length == 0)
            {
                TvLibrary.Log.Log.Debug("Turbosight: error, response too short", new object[0]);
            }
            else
            {
                int numCasIds = content[0];
                TvLibrary.Log.Log.Debug("  # CAS IDs = {0}", new object[] { numCasIds });
                int i = 1;
                int l = 1;
                while ((l + 2) <= length)
                {
                    TvLibrary.Log.Log.Debug("  {0,-2}        = 0x{1:x2}{2:x2}", i, content[l + 1], content[l]);
                    l += 2;
                    i++;
                }
                if (length != ((numCasIds * 2) + 1))
                {
                    TvLibrary.Log.Log.Debug("Turbosight: error, unexpected numCasIds", new object[0]);
                    DVB_MMI.DumpBinary(_mmiResponseBuffer, 0, length);
                }
            }
        }

        private void HandleEnquiry(byte[] content, int length)
        {
            TvLibrary.Log.Log.Debug("Turbosight: enquiry", new object[0]);
            if (length < 3)
            {
                TvLibrary.Log.Log.Debug("Turbosight: error, response too short", new object[0]);
                DVB_MMI.DumpBinary(content, 0, length);
            }
            else
            {
                bool blind = (content[0] != 0);
                uint answerLength = content[1];
                String text = System.Text.Encoding.ASCII.GetString(content, 2, length - 2);
                TvLibrary.Log.Log.Debug("  text   = {0}", text);
                TvLibrary.Log.Log.Debug("  length = {0}", answerLength);
                TvLibrary.Log.Log.Debug("  blind  = {0}", blind);
                if (this._ciMenuCallbacks != null)
                {
                    try
                    {
                        this._ciMenuCallbacks.OnCiRequest(blind, answerLength, text);
                    }
                    catch (Exception exception)
                    {
                        TvLibrary.Log.Log.Debug("Turbosight: CAM request callback exception\r\n{0}", new object[] { exception.ToString() });
                    }
                }
                else
                {
                    TvLibrary.Log.Log.Debug("Turbosight: menu callbacks are not set");
                }
            }
        }

        private void HandleMenu(byte[] content, int length)
        {
            TvLibrary.Log.Log.Debug("Turbosight: menu", new object[0]);
            if (length == 0)
            {
                TvLibrary.Log.Log.Debug("Turbosight: error, response too short", new object[0]);
            }
            else
            {
                int numEntries = content[0];

                // Read all the entries into a list. Entries are NULL terminated.
                List<string> entries = new List<string>();
                byte[] source = null;
                int entryCount = 0;
                for (int i = 1; i < length; i++)
                {
                    if (content[i] == 0)
                    {
                        IntPtr ptr = Marshal.AllocCoTaskMem(source.Length + 1);
                        Marshal.Copy(source, 0, ptr, source.Length);
                        Marshal.WriteByte(ptr, source.Length, 0);
                        //DVB_MMI.DumpBinary(ptr, 0, source.Length);
                        entries.Add(DvbTextConverter.Convert(ptr, null));
                        entryCount++;
                        source = new byte[] { };
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    else
                    {
                        if (source != null)
                        {
                            byte[] array = new byte[source.Length + 1];
                            source.CopyTo(array, 0);
                            array[array.Length - 1] = content[i];
                            source = array;
                        }
                        else
                        {
                            source = new byte[] { content[i] };
                        }
                        //TvLibrary.Log.Log.Debug("OK2", new object[0]);
                    }
                }
                IntPtr destination = Marshal.AllocCoTaskMem(source.Length + 1);
                Marshal.Copy(source, 0, destination, source.Length);
                Marshal.WriteByte(destination, source.Length, 0);
                //DVB_MMI.DumpBinary(destination, 0, source.Length);
                entries.Add(DvbTextConverter.Convert(destination, null));
                source = null;
                Marshal.FreeCoTaskMem(destination);
                entryCount -= 2;
                if (entryCount < 0)
                {
                    TvLibrary.Log.Log.Debug("Turbosight: error, not enough menu entries", new object[0]);
                    DVB_MMI.DumpBinary(content, 0, length);
                }
                else
                {
                    TvLibrary.Log.Log.Debug("  title     = {0}", new object[] { entries[0] });
                    TvLibrary.Log.Log.Debug("  sub-title = {0}", new object[] { entries[1] });
                    TvLibrary.Log.Log.Debug("  footer    = {0}", new object[] { entries[2] });
                    TvLibrary.Log.Log.Debug("  # entries = {0}", new object[] { numEntries });
                    if (this._ciMenuCallbacks != null)
                    {
                        try
                        {
                            this._ciMenuCallbacks.OnCiMenu(entries[0], entries[1], entries[2], entryCount);
                        }
                        catch (Exception exception)
                        {
                            TvLibrary.Log.Log.Debug("Turbosight: menu header callback exception\r\n{0}", new object[] { exception.ToString() });
                        }
                    }
                    for (int j = 0; j < entryCount; j++)
                    {
                        TvLibrary.Log.Log.Debug("  entry {0,-2}  = {1}", new object[] { j + 1, entries[j + 3] });
                        if (this._ciMenuCallbacks != null)
                        {
                            try
                            {
                                this._ciMenuCallbacks.OnCiMenuChoice(j, entries[j + 3]);
                            }
                            catch (Exception exception2)
                            {
                                TvLibrary.Log.Log.Debug("Turbosight: menu entry callback exception\r\n{0}", new object[] { exception2.ToString() });
                            }
                        }
                    }
                    if (entryCount != numEntries)
                    {
                        TvLibrary.Log.Log.Debug("Turbosight: error, numEntries != entryCount", new object[0]);
                    }
                }
            }
        }

        private void MmiHandler()
        {
            TvLibrary.Log.Log.Debug("Turbosight: MMI handler thread start polling", new object[0]);
            TbsMmiMessageType message = TbsMmiMessageType.Null;
            ushort sendCount = 0;
            try
            {
                while (!this._stopMmiHandlerThread)
                {
                    // Check for CAM state changes.
                    bool newState;
                    lock (this)
                    {
                        newState = Camavailable(this._ciHandle);
                    }
                    if (newState != this._isCamPresent)
                    {
                        this._isCamPresent = newState;
                        TvLibrary.Log.Log.Debug("Turbosight: CAM state change, CAM present = {0}", new object[] { this._isCamPresent });
                        // If a CAM has just been inserted then clear the message queue - we consider
                        // any old messages as invalid now.                        
                        if (this._isCamPresent)
                        {
                            lock (this)
                            {
                                this._mmiMessageQueue = new List<MmiMessage>();
                            }
                            message = TbsMmiMessageType.Null;
                        }
                    }
                    // If there is no CAM then we can't send or receive messages.
                    if (!this._isCamPresent)
                    {
                        Thread.Sleep(MmiHandlerThreadSleepTime);
                        continue;
                    }

                    // Are we still trying to get a response?
                    if (message == TbsMmiMessageType.Null)
                    {
                        // No -> do we have a message to send?
                        lock (this)
                        {
                            // Yes -> load it into the message buffer.
                            if (this._mmiMessageQueue.Count > 0)
                            {
                                message = _mmiMessageQueue[0].Type;
                                TvLibrary.Log.Log.Debug("Turbosight: sending message {0}", message);
                                Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)message);
                                for (ushort i = 0; i < _mmiMessageQueue[0].Length; i++)
                                {
                                    Marshal.WriteByte(_mmiMessageBuffer, i + 1, _mmiMessageQueue[0].Message[i]);
                                }
                                sendCount = 0;
                            }
                            // No -> poll for unrequested messages from the CAM.
                            else
                            {
                                Marshal.WriteByte(this._mmiMessageBuffer, 0, (byte)TbsMmiMessageType.GetMmi);
                            }
                        }
                    }
                    // Send/resend the message.
                    lock (this)
                    {
                        TBS_ci_MMI_Process(this._ciHandle, this._mmiMessageBuffer, this._mmiResponseBuffer);
                    }
                    // Do we expect a response to this message?
                    if (message == TbsMmiMessageType.EnterMenu || message == TbsMmiMessageType.MenuAnswer || message == TbsMmiMessageType.Answer || message == TbsMmiMessageType.CloseMmi)
                    {
                        // No -> remove this message from the queue and move on.
                        lock (this)
                        {
                            this._mmiMessageQueue.RemoveAt(0);
                        }
                        message = TbsMmiMessageType.Null;
                        if (this._mmiMessageQueue.Count == 0)
                        {
                            TvLibrary.Log.Log.Debug("Turbosight: resuming polling...", new object[0]);
                        }
                        continue;
                    }

                    // Yes, we expect a response -> check for a response.
                    TbsMmiMessageType response = TbsMmiMessageType.Null;
                    response = (TbsMmiMessageType)Marshal.ReadByte(this._mmiResponseBuffer, 4);
                    if (response == TbsMmiMessageType.Null)
                    {
                        // Responses don't always arrive quickly so give the CAM time to respond if
                        // the response isn't ready yet.
                        Thread.Sleep(MmiHandlerThreadSleepTime);

                        // If we are waiting for a response to a message that we sent
                        // directly and we haven't received a response after 10 requests
                        // then give up and move on.
                        if (message != TbsMmiMessageType.Null)
                        {
                            sendCount = sendCount++;
                            if (sendCount >= 10)
                            {
                                lock (this)
                                {
                                    this._mmiMessageQueue.RemoveAt(0);
                                }
                                TvLibrary.Log.Log.Debug("Turbosight: giving up on message {0}", new object[] { message });
                                message = TbsMmiMessageType.Null;
                                if (this._mmiMessageQueue.Count == 0)
                                {
                                    TvLibrary.Log.Log.Debug("Turbosight: resuming polling...", new object[0]);
                                }
                            }
                        }
                        continue;
                    }
                    TvLibrary.Log.Log.Debug("Turbosight: received MMI response {0} to message {1}", new object[] { response, message });
                    #region response handling

                    // Get the response bytes.
                    byte lsb = Marshal.ReadByte(this._mmiResponseBuffer, 5);
                    byte msb = Marshal.ReadByte(this._mmiResponseBuffer, 6);
                    int length = (256 * msb) + lsb;
                    if (length > MmiResponseBufferSize - 7)
                    {
                        TvLibrary.Log.Log.Debug("Turbosight: response too long, length = {0}", new object[] { length });
                        // We know we haven't got the complete response (DLL internal buffer overflow),
                        // so wipe the message and response buffers and give up on this message.
                        for (int i = 0; i < MmiResponseBufferSize; i++)
                        {
                            Marshal.WriteByte(this._mmiResponseBuffer, i, 0);
                        }
                        // If we requested this response directly then remove the request
                        // message from the queue.
                        if (message != TbsMmiMessageType.Null)
                        {
                            lock (this)
                            {
                                _mmiMessageQueue.RemoveAt(0);
                            }
                            message = TbsMmiMessageType.Null;
                            if (this._mmiMessageQueue.Count == 0)
                            {
                                TvLibrary.Log.Log.Debug("Turbosight: resuming polling...", new object[0]);
                            }
                        }
                        continue;
                    }

                    TvLibrary.Log.Log.Debug("Turbosight: response length = {0}", new object[] { length });
                    byte[] responseBytes = new byte[length];
                    int j = 7;
                    for (int i = 0; i < length; i++)
                    {
                        responseBytes[i] = Marshal.ReadByte(this._mmiResponseBuffer, j++);
                    }

                    if (response == TbsMmiMessageType.ApplicationInfo)
                    {
                        HandleApplicationInformation(responseBytes, length);
                    }
                    else if (response == TbsMmiMessageType.CaInfo)
                    {
                        HandleCaInformation(responseBytes, length);
                    }
                    else if (response == TbsMmiMessageType.Menu || response == TbsMmiMessageType.List)
                    {
                        HandleMenu(responseBytes, length);
                    }
                    else if (response == TbsMmiMessageType.Enquiry)
                    {
                        HandleEnquiry(responseBytes, length);
                    }
                    else
                    {
                        TvLibrary.Log.Log.Debug("Turbosight: unhandled response message {0}", response);
                        DVB_MMI.DumpBinary(_mmiResponseBuffer, 0, length);
                    }


                    // A message has been handled and now we move on to handling the
                    // next message or revert to polling for messages from the CAM.
                    for (int i = 0; i < MmiResponseBufferSize; i++)
                    {
                        Marshal.WriteByte(_mmiResponseBuffer, i, 0);
                    }
                    // If we requested this response directly then remove the request
                    // message from the queue.
                    if (message != TbsMmiMessageType.Null)
                    {
                        lock (this)
                        {
                            _mmiMessageQueue.RemoveAt(0);
                        }
                        message = TbsMmiMessageType.Null;
                        if (_mmiMessageQueue.Count == 0)
                        {
                            TvLibrary.Log.Log.Debug("Turbosight: resuming polling...");
                        }
                    }
                    #endregion
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                TvLibrary.Log.Log.Debug("Turbosight: error in MMI handler thread\r\n{0}", new object[] { exception.ToString() });
            }
        }

        /// <summary>
        /// Start a thread that will handle interaction with the CAM.
        /// </summary>
        private void StartMmiHandlerThread()
        {
            // Don't start a thread if there is no purpose for it.
            if (!_isTurbosight || !_isCiSlotPresent || _ciHandle == IntPtr.Zero)
            {
                return;
            }

            // Check if an existing thread is still alive. It will be terminated in case of errors, i.e. when CI callback failed.
            if ((this._mmiHandlerThread != null) && !this._mmiHandlerThread.IsAlive)
            {
                TvLibrary.Log.Log.Debug("Turbosight: aborting old MMI handler thread");
                this._mmiHandlerThread.Abort();
                this._mmiHandlerThread = null;
            }
            if (this._mmiHandlerThread == null)
            {
                TvLibrary.Log.Log.Debug("Turbosight: starting new MMI handler thread", new object[0]);
                this._mmiMessageQueue = new List<MmiMessage>();
                for (int i = 0; i < MmiMessageBufferSize; i++)
                {
                    Marshal.WriteByte(this._mmiMessageBuffer, i, 0);
                }
                for (int j = 0; j < MmiResponseBufferSize; j++)
                {
                    Marshal.WriteByte(this._mmiResponseBuffer, j, 0);
                }
                this._stopMmiHandlerThread = false;
                this._mmiHandlerThread = new Thread(new ThreadStart(this.MmiHandler));
                this._mmiHandlerThread.Name = "Turbosight MMI handler";
                this._mmiHandlerThread.IsBackground = true;
                this._mmiHandlerThread.Priority = ThreadPriority.Lowest;
                this._mmiHandlerThread.Start();
            }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Close the conditional access interface.
        /// </summary>
        /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
        public bool CloseCi()
        {
            TvLibrary.Log.Log.Debug("Turbosight: close conditional access interface");

            if (_mmiHandlerThread != null && _mmiHandlerThread.IsAlive)
            {
                _stopMmiHandlerThread = true;
                // In the worst case scenario it should take approximately
                // twice the thread sleep time to cleanly stop the thread.
                _mmiHandlerThread.Join(MmiHandlerThreadSleepTime * 2);
                if (_mmiHandlerThread.IsAlive)
                {
                    TvLibrary.Log.Log.Debug("Turbosight: warning, failed to join MMI handler thread => aborting thread");
                    _mmiHandlerThread.Abort();
                }
                _mmiHandlerThread = null;
            }

            if (_ciHandle != IntPtr.Zero)
            {
                On_Exit_CI(_ciHandle);
                _ciHandle = IntPtr.Zero;
            }

            _isCiSlotPresent = false;
            _isCamPresent = false;
            _mmiMessageQueue = null;
            if (_mmiMessageBuffer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(_mmiMessageBuffer);
                _mmiMessageBuffer = IntPtr.Zero;
            }
            if (_mmiResponseBuffer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(_mmiResponseBuffer);
                _mmiResponseBuffer = IntPtr.Zero;
            }
            if (_pmtBuffer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(_pmtBuffer);
                _pmtBuffer = IntPtr.Zero;
            }
            //if (_libHandle != IntPtr.Zero)
            //{
            //    NativeMethods.FreeLibrary(_libHandle);
            //    _libHandle = IntPtr.Zero;
            //}
            //_dllLoaded = false;

            TvLibrary.Log.Log.Debug("Turbosight: result = true");
            return true;







            //if (this._ciHandle != IntPtr.Zero)
            //{
            //    TvLibrary.Log.Log.Debug("Turbosight: close conditional access interface", new object[0]);
            //    try
            //    {
            //        On_Exit_CI(this._ciHandle);
            //    }
            //    catch (Exception)
            //    {
            //    }
            //    this._ciHandle = IntPtr.Zero;
            //    Marshal.FreeCoTaskMem(this._mmiMessageBuffer);
            //    Marshal.FreeCoTaskMem(this._mmiResponseBuffer);
            //    Marshal.FreeCoTaskMem(this._pmtBuffer);
            //}
            //return true;
        }

        /// <summary>
        /// Send a request from the user to the CAM to close the menu.
        /// </summary>
        /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
        public bool CloseCIMenu()
        {
            TvLibrary.Log.Log.Debug("Turbosight: close menu");

            if (!_isTurbosight || _ciHandle == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: device not initialised or interface not supported");
                return false;
            }
            if (!this._isCamPresent)
            {
                TvLibrary.Log.Log.Debug("Turbosight: the CAM is not present");
                return false;
            }
            lock (this)
            {
                this._mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.CloseMmi));
                //this._mmiMessageQueue.Add(1);
                //this._mmiMessageQueue.Add(14);
            }
            return true;
        }

        /// <summary>
        /// Sets the DVB-Type for TBS PCIe Card.
        /// </summary>
        /// <param name="reply">The reply message.</param>
        /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
        public void SetDVBS2(DVBSChannel channel)
        {
            //Set the Pilot
            Log.Log.Info("Turbosight: Set DVB-S2");
            if (channel.ModulationType != ModulationType.ModNbc8Psk && channel.ModulationType != ModulationType.ModNbcQpsk)
                return;
            int hr;
            KSPropertySupport supported;
            _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.NbcParams,
                                        out supported);
            if ((supported & KSPropertySupport.Set) == KSPropertySupport.Set)
            {
                BDA_NBC_PARAMS DVBNBCParams = new BDA_NBC_PARAMS();
                DVBNBCParams.fecrate = (int)channel.InnerFecRate;
                DVBNBCParams.modtype = (int)channel.ModulationType;
                DVBNBCParams.pilot = (int)channel.Pilot;
                DVBNBCParams.rolloff = (int)channel.Rolloff;
                DVBNBCParams.dvbtype = 2; //DVB-S2
                Log.Log.Info("Turbosight: Set DVB-S2: {0}", DVBNBCParams.dvbtype);
                Marshal.StructureToPtr(DVBNBCParams, _generalBuffer, true);
                DVB_MMI.DumpBinary(_generalBuffer, 0, TbsNBCParamsSize);

                hr = _propertySet.Set(_propertySetGuid, (int)BdaExtensionProperty.NbcParams,
                  _generalBuffer, TbsNBCParamsSize,
                  _generalBuffer, TbsNBCParamsSize
                );

                if (hr != 0)
                {
                    Log.Log.Info("Turbosight: Set DVB-S2 returned {0}", hr, DsError.GetErrorText(hr));
                }
            }
            else
            {
                Log.Log.Info("Turbosight: Set DVB-S2 not supported");
            }
        }

        public void Dispose()
        {
            if (this._isTurbosight)
            {
                this.SetPowerState(false);
                if (this._mmiHandlerThread != null)
                {
                    this._stopMmiHandlerThread = true;
                    Thread.Sleep(0xbb8);
                }
                this.CloseCi();
                Marshal.FreeCoTaskMem(this._generalBuffer);
                if (this._isUsb)
                {
                    Release.ComObject(this._propertySet);
                }
            }
            // MBU
            _tunerFilter = null;            // MBU
            _isTurbosight = false;          // MBU

        }

        /// <summary>
        /// Send a request from the user to the CAM to open the menu.
        /// </summary>
        /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
        public bool EnterCIMenu()
        {
            TvLibrary.Log.Log.Debug("Turbosight: enter menu", new object[0]);
            if (!_isTurbosight || _ciHandle == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: device not initialised or interface not supported");
                return false;
            }
            if (!this._isCamPresent)
            {
                TvLibrary.Log.Log.Debug("Turbosight: the CAM is not present");
                return false;
            }
            lock (this)
            {
                // Close any existing sessions otherwise the CAM gets confused.
                _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.CloseMmi));
                // We send an "application info" message because attempting to enter the menu will fail
                // if you don't get the application information first.
                _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.ApplicationInfo));
                // The CA information is just for information purposes.
                _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.CaInfo));
                // The main message.
                _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.EnterMenu));
                // We have to request a response.
                _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.GetMmi));

                //this._mmiMessageQueue.Add(1);
                //this._mmiMessageQueue.Add(14);
                //this._mmiMessageQueue.Add(1);
                //this._mmiMessageQueue.Add(1);
                //this._mmiMessageQueue.Add(1);
                //this._mmiMessageQueue.Add(2);
                //this._mmiMessageQueue.Add(1);
                //this._mmiMessageQueue.Add(9);
                //this._mmiMessageQueue.Add(1);
                //this._mmiMessageQueue.Add(13);
            }
            return true;
        }

        /// <summary>
        /// IsCamPresent
        /// </summary>
        /// <returns></returns>
        public bool IsCamPresent()
        {
            TvLibrary.Log.Log.Debug("Turbosight: is CAM present", new object[0]);
            if (!this._isCiSlotPresent)
            {
                TvLibrary.Log.Log.Debug("Turbosight: CI slot not present", new object[0]);
                return false;
            }
            if (this._ciHandle == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: interface not opened", new object[0]);
                return false;
            }
            bool flag = false;
            lock (this)
            {
                flag = Camavailable(this._ciHandle);
            }
            TvLibrary.Log.Log.Debug("Turbosight: result = {0}", new object[] { flag });
            return flag;
        }

        public bool IsCamReady()
        {
            TvLibrary.Log.Log.Debug("Turbosight: is CAM ready", new object[0]);
            if (this._ciHandle == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: interface not opened", new object[0]);
                return false;
            }
            // We can only tell whether a CAM is present, not whether it is ready.
            bool camPresent = false;
            lock (this)
            {
                camPresent = Camavailable(this._ciHandle);
            }
            TvLibrary.Log.Log.Debug("Turbosight: result = {0}", new object[] { camPresent });
            return camPresent;
        }

        public bool IsCiSlotPresent()
        {
            // Check whether a CI slot is present.
            TvLibrary.Log.Log.Debug("Turbosight: is CI slot present", new object[0]);
            int ciAccessProperty = (int)BdaExtensionProperty.CiAccess;
            if (_isUsb)
            {
              ciAccessProperty = (int)UsbBdaExtensionProperty.CiAccess;
            }
            KSPropertySupport support;
            int hr = _propertySet.QuerySupported(_propertySetGuid, ciAccessProperty, out support);
            if (hr != (int)HResult.Serverity.Success || support == 0)
            {
              TvLibrary.Log.Log.Debug("Turbosight: device doesn't have a CI slot");
              return false;
            }
            TvLibrary.Log.Log.Debug("Turbosight: device does have a CI slot");
            return true;  
          
            /*TvLibrary.Log.Log.Debug("Turbosight: is CI slot present", new object[0]);
            string filterName = FilterGraphTools.GetFilterName(this._tunerFilter);
            bool flag = false;
            for (int i = 0; i < TunersWithCiSlots.Length; i++)
            {
                if (filterName.Equals(TunersWithCiSlots[i]))
                {
                    flag = true;
                    break;
                }
            }
            TvLibrary.Log.Log.Debug("Turbosight: result = {0}", new object[] { flag });
            return flag;*/
        }

        /// <summary>
        /// Open the conditional access interface. For the interface to be opened successfully it is expected
        /// that any necessary hardware (such as a CI slot) is connected.
        /// </summary>
        /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
        public bool OpenCi()
        {
            TvLibrary.Log.Log.Debug("Turbosight: open conditional access interface");

            if (!_isTurbosight)
            {
                TvLibrary.Log.Log.Debug("Turbosight: device not initialised or interface not supported");
                return false;
            }
            if (this._ciHandle != IntPtr.Zero)
            {
                return false;
            }

            // Check whether a CI slot is present.
            this._isCiSlotPresent = this.IsCiSlotPresent();
            if (!this._isCiSlotPresent)
            {
                return false;
            }
            TvLibrary.Log.Log.Debug("Turbosight: open conditional access interface", new object[0]);

            this._ciHandle = On_Start_CI(this._tunerFilter, FilterGraphTools.GetFilterName(this._tunerFilter), _deviceIndex);
            if (this._ciHandle == IntPtr.Zero || _ciHandle.ToInt64() == -1)
            {
                TvLibrary.Log.Log.Debug("Turbosight: interface handle is null", new object[0]);
                this._isCiSlotPresent = false;
                return false;
            }
            else
            {
                TvLibrary.Log.Log.Debug(string.Format("Turbosight: interface handle {0}", _ciHandle), new object[0]);
            }

            TvLibrary.Log.Log.Debug("Turbosight: interface opened successfully", new object[0]);
            this._mmiMessageBuffer = Marshal.AllocCoTaskMem(MmiMessageBufferSize);
            this._mmiResponseBuffer = Marshal.AllocCoTaskMem(MmiResponseBufferSize);
            this._pmtBuffer = Marshal.AllocCoTaskMem(MaxPmtLength + 2);  // + 2 for TBS PMT header
            this._isCamPresent = this.IsCamPresent();
            this._isCamReady = this.IsCamReady();
            return true;
        }

        /// <summary>
        /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
        /// intended for this tuner).
        /// </summary>
        /// <param name="response">The response (or command).</param>
        /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
        public bool ReadDiSEqCCommand(out byte[] response)
        {
            TvLibrary.Log.Log.Debug("Turbosight: read DiSEqC response");
            response = null;

            if (!_isTurbosight || _propertySet == null)
            {
                TvLibrary.Log.Log.Debug("Turbosight: device not initialised or interface not supported");
                return false;
            }

            for (int i = 0; i < TbsAccessParamsSize; i++)
            {
                Marshal.WriteByte(_generalBuffer, i, 0);
            }

            TbsAccessParams accessParams = new TbsAccessParams();
            accessParams.AccessMode = TbsAccessMode.Diseqc;
            int returnedByteCount;
            int hr = _propertySet.Get(_propertySetGuid, _tbsAccessProperty,
              _generalBuffer, TbsAccessParamsSize,
              _generalBuffer, TbsAccessParamsSize,
              out returnedByteCount
            );
            if (hr != 0)
            {
                TvLibrary.Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                return false;
            }

            DVB_MMI.DumpBinary(_generalBuffer, 0, returnedByteCount);

            if (returnedByteCount != TbsAccessParamsSize)
            {
                TvLibrary.Log.Log.Debug("Turbosight: result = failure, unexpected number of bytes ({0}) returned", returnedByteCount);
                return false;
            }

            accessParams = (TbsAccessParams)Marshal.PtrToStructure(_generalBuffer, typeof(TbsAccessParams));
            if (accessParams.DiseqcReceiveMessageLength > MaxDiseqcMessageLength)
            {
                TvLibrary.Log.Log.Debug("Turbosight: result = failure, unexpected number of message bytes ({0}) returned", accessParams.DiseqcReceiveMessageLength);
                return false;
            }
            response = new byte[accessParams.DiseqcReceiveMessageLength];
            Buffer.BlockCopy(accessParams.DiseqcReceiveMessage, 0, response, 0, (int)accessParams.DiseqcReceiveMessageLength);
            TvLibrary.Log.Log.Debug("Turbosight: result = success");
            return true;
        }

        /// <summary>
        /// Reset the conditional access interface.
        /// </summary>
        /// <param name="rebuildGraph">This parameter will be set to <c>true</c> if the BDA graph must be rebuilt
        ///   for the interface to be completely and successfully reset.</param>
        /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
        public bool ResetCi(out bool rebuildGraph)
        {
            // TBS have confirmed that it is not currently possible to call On_Start_CI() multiple times on a
            // filter instance ***even if On_Exit_CI() is called***. The graph must be rebuilt to reset the CI.
            rebuildGraph = true;
            return true;
        }

        /// <summary>
        /// Send a menu entry selection from the user to the CAM.
        /// </summary>
        /// <param name="choice">The index of the selection as an unsigned byte value.</param>
        /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
        public bool SelectMenu(byte choice)
        {
            TvLibrary.Log.Log.Debug("Turbosight: select menu entry, choice = {0}", choice);

            if (!_isTurbosight || _ciHandle == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: device not initialised or interface not supported");
                return false;
            }
            if (!_isCamPresent)
            {
                TvLibrary.Log.Log.Debug("Turbosight: the CAM is not present");
                return false;
            }

            lock (this)
            {
                MmiMessage selectMessage = new MmiMessage(TbsMmiMessageType.MenuAnswer, 3);
                selectMessage.Message[0] = 0;
                selectMessage.Message[1] = 0;
                selectMessage.Message[2] = choice;
                _mmiMessageQueue.Add(selectMessage);
                // Don't explicitly request a response for a "back" request as that
                // could choke the message queue with a message that the CAM
                // never answers.
                if (choice != 0)
                {
                    _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.GetMmi));
                }
            }
            return true;
        }


        public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
        {
            bool flag = BandTypeConverter.IsHiBand(channel, parameters);
            ToneBurst off = ToneBurst.None;
            bool successDiseqc = true;
            if (channel.DisEqc == DisEqcType.SimpleA)
            {
                off = ToneBurst.ToneBurst;
            }
            else if (channel.DisEqc == DisEqcType.SimpleB)
            {
                off = ToneBurst.DataBurst;
            }
            else if (channel.DisEqc != DisEqcType.None)
            {
                int antennaNr = BandTypeConverter.GetAntennaNr(channel);
                bool flag3 = (channel.Polarisation == Polarisation.LinearH) || (channel.Polarisation == Polarisation.CircularL);
                byte num2 = 240;
                num2 = (byte)(num2 | (flag ? ((byte)1) : ((byte)0)));
                num2 = (byte)(num2 | (flag3 ? ((byte)2) : ((byte)0)));
                num2 = (byte)(num2 | ((byte)((antennaNr - 1) << 2)));
                byte[] command = new byte[] { 0xe0, 0x10, 0x38, 0 };
                command[3] = num2;
                successDiseqc = this.SendDiSEqCCommand(command);
            }
            Tone22k on = Tone22k.Off;
            if (flag)
            {
                on = Tone22k.On;
            }
            bool successTone = this.SetToneState(off, on);

            SetDVBS2(channel);  // Don't need to know the result of this

            return (successDiseqc && successTone);
        }

        /// <summary>
        /// Send an arbitrary DiSEqC command.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
        public virtual bool SendDiSEqCCommand(byte[] command)
        {
            TvLibrary.Log.Log.Debug("Turbosight: send DiSEqC command");

            if (!_isTurbosight || _propertySet == null)
            {
                TvLibrary.Log.Log.Debug("Turbosight: device not initialised or interface not supported");
                return false;
            }
            if (command == null || command.Length == 0)
            {
                TvLibrary.Log.Log.Debug("Turbosight: command not supplied");
                return true;
            }
            if (command.Length > MaxDiseqcMessageLength)
            {
                TvLibrary.Log.Log.Debug("Turbosight: command too long, length = {0}", command.Length);
                return false;
            }

            TbsAccessParams accessParams = new TbsAccessParams();
            accessParams.AccessMode = TbsAccessMode.Diseqc;
            accessParams.DiseqcTransmitMessageLength = (uint)command.Length;
            accessParams.DiseqcTransmitMessage = new byte[MaxDiseqcMessageLength];
            Buffer.BlockCopy(command, 0, accessParams.DiseqcTransmitMessage, 0, command.Length);

            Marshal.StructureToPtr(accessParams, _generalBuffer, true);
            //DVB_MMI.DumpBinary(_generalBuffer, 0, TbsAccessParamsSize);

            int hr = _propertySet.Set(_propertySetGuid, _tbsAccessProperty,
              _generalBuffer, TbsAccessParamsSize,
              _generalBuffer, TbsAccessParamsSize
            );
            if (hr == 0)
            {
                TvLibrary.Log.Log.Debug("Turbosight: result = success");
                return true;
            }

            TvLibrary.Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
        }

        /// <summary>
        /// Send a response from the user to the CAM.
        /// </summary>
        /// <param name="cancel"><c>True</c> to cancel the request.</param>
        /// <param name="answer">The user's response.</param>
        /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
        public bool SendMenuAnswer(bool cancel, string answer)
        {
            if (answer == null)
            {
                answer = String.Empty;
            }
            TvLibrary.Log.Log.Debug("Turbosight: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

            if (!_isTurbosight || _ciHandle == IntPtr.Zero)
            {
                TvLibrary.Log.Log.Debug("Turbosight: device not initialised or interface not supported");
                return false;
            }
            if (!_isCamPresent)
            {
                TvLibrary.Log.Log.Debug("Turbosight: the CAM is not present");
                return false;
            }

            if (answer.Length > 254)
            {
                TvLibrary.Log.Log.Debug("Turbosight: answer too long, length = {0}", answer.Length);
                return false;
            }

            byte responseType = (byte)MmiResponseType.Answer;
            if (cancel)
            {
                responseType = (byte)MmiResponseType.Cancel;
            }
            lock (this)
            {
                MmiMessage answerMessage = new MmiMessage(TbsMmiMessageType.Answer, answer.Length + 3);
                answerMessage.Message[0] = (byte)(answer.Length + 1);
                answerMessage.Message[1] = 0;
                answerMessage.Message[2] = responseType;
                int offset = 3;
                for (int i = 0; i < answer.Length; i++)
                {
                    answerMessage.Message[offset++] = (byte)answer[i];
                }
                _mmiMessageQueue.Add(answerMessage);
                // We have to request a response.
                _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.GetMmi));
            }
            return true;
        }

        public bool SendPmt(ListManagementType listAction, CommandIdType command, byte[] pmt, int length)
        {
            TvLibrary.Log.Log.Debug("Turbosight: send PMT to CAM, list action = {0}, command = {1}", new object[] { listAction, command });
            if (!this._isCamPresent)
            {
                TvLibrary.Log.Log.Debug("Turbosight: CAM not available", new object[0]);
                return true;
            }
            if (length > 0x400)
            {
                TvLibrary.Log.Log.Debug("Turbosight: buffer capacity too small, length = {0}", new object[] { length });
                return false;
            }
            Marshal.WriteByte(this._pmtBuffer, 0, (byte)listAction);
            Marshal.WriteByte(this._pmtBuffer, 1, (byte)command);
            int ofs = 2;
            for (int i = 0; i < length; i++)
            {
                Marshal.WriteByte(this._pmtBuffer, ofs, pmt[i]);
                ofs++;
            }
            TBS_ci_SendPmt(this._ciHandle, this._pmtBuffer, (ushort)(length + 2));
            return true;
        }

        /// <summary>
        /// Set the CAM callback handler functions.
        /// </summary>
        /// <param name="ciMenuHandler">A set of callback handler functions.</param>
        /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
        public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
        {
            if (ciMenuHandler != null)
            {
                this._ciMenuCallbacks = ciMenuHandler;
                this.StartMmiHandlerThread();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Turn the device power supply on or off.
        /// </summary>
        /// <param name="powerOn"><c>True</c> to turn the power supply on; <c>false</c> to turn the power supply off.</param>
        /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
        public bool SetPowerState(bool powerOn)
        {
            TvLibrary.Log.Log.Debug("Turbosight: set power state, on = {0}", new object[] { powerOn });
            if (!_isTurbosight)
            {
                TvLibrary.Log.Log.Debug("Turbosight: device not initialised or interface not supported");
                return false;
            }
            TbsAccessParams accessParams = new TbsAccessParams();
            accessParams.AccessMode = TbsAccessMode.LnbPower;
            if (powerOn)
            {
                accessParams.LnbPower = TbsLnbPower.On;
            }
            else
            {
                accessParams.LnbPower = TbsLnbPower.Off;
            }

            Marshal.StructureToPtr(accessParams, this._generalBuffer, true);

            int hresult = this._propertySet.Set(this._propertySetGuid, this._tbsAccessProperty, this._generalBuffer, TbsAccessParamsSize, this._generalBuffer, TbsAccessParamsSize);
            if (hresult == 0)
            {
                TvLibrary.Log.Log.Debug("Turbosight: result = success", new object[0]);
                return true;
            }
            TvLibrary.Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", new object[] { hresult, HResult.GetDXErrorString(hresult) });
            return false;
        }

        /// <summary>
        /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
        /// </summary>
        /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
        /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
        /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
        public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
        {
            TvLibrary.Log.Log.Debug("Turbosight: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

            if (!_isTurbosight || _propertySet == null)
            {
                TvLibrary.Log.Log.Debug("Turbosight: device not initialised or interface not supported");
                return false;
            }

            TbsAccessParams accessParams = new TbsAccessParams();
            accessParams.AccessMode = TbsAccessMode.Tone;
            bool success = true;
            int hr;

            // Send the burst command first.
            if (toneBurstState != ToneBurst.None)
            {
                accessParams.Tone = TbsTone.BurstUnmodulated;
                if (toneBurstState == ToneBurst.DataBurst)
                {
                    accessParams.Tone = TbsTone.BurstModulated;
                }

                Marshal.StructureToPtr(accessParams, _generalBuffer, true);
                //DVB_MMI.DumpBinary(_generalBuffer, 0, TbsAccessParamsSize);

                hr = _propertySet.Set(_propertySetGuid, _tbsAccessProperty,
                  _generalBuffer, TbsAccessParamsSize,
                  _generalBuffer, TbsAccessParamsSize
                );
                if (hr == 0)
                {
                    TvLibrary.Log.Log.Debug("Turbosight: burst result = success");
                }
                else
                {
                    TvLibrary.Log.Log.Debug("Turbosight: burst result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                    success = false;
                }
            }

            // Now set the 22 kHz tone state.
            accessParams.Tone = TbsTone.Off;
            if (tone22kState == Tone22k.On)
            {
                accessParams.Tone = TbsTone.On;
            }

            Marshal.StructureToPtr(accessParams, _generalBuffer, true);
            //DVB_MMI.DumpBinary(_generalBuffer, 0, TbsAccessParamsSize);

            hr = _propertySet.Set(_propertySetGuid, _tbsAccessProperty,
              _generalBuffer, TbsAccessParamsSize,
              _generalBuffer, TbsAccessParamsSize
            );
            if (hr == 0)
            {
                TvLibrary.Log.Log.Debug("Turbosight: 22 kHz result = success");
            }
            else
            {
                TvLibrary.Log.Log.Debug("Turbosight: 22 kHz result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                success = false;
            }

            return success;
        }

        public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
        {
            KSPropertySupport support;
            TvLibrary.Log.Log.Debug("Turbosight: set tuning parameters", new object[0]);
            DVBSChannel channel2 = channel as DVBSChannel;
            if (channel2 == null)
            {
                return channel;
            }
            NbcTuningParams structure = new NbcTuningParams
            {
                DvbsStandard = TbsDvbsStandard.Auto,
                InnerFecRate = channel2.InnerFecRate
            };
            TvLibrary.Log.Log.Debug("  inner FEC rate = {0}", new object[] { structure.InnerFecRate });

            if (channel2.ModulationType == ModulationType.ModNotSet)
            {
                channel2.ModulationType = ModulationType.ModQpsk;
                structure.DvbsStandard = TbsDvbsStandard.Dvbs;
            }
            else if (channel2.ModulationType == ModulationType.ModQpsk)
            {
                channel2.ModulationType = ModulationType.ModNbcQpsk;
                structure.DvbsStandard = TbsDvbsStandard.Dvbs2;
            }
            else if (channel2.ModulationType == ModulationType.Mod8Psk)
            {
                channel2.ModulationType = ModulationType.ModNbc8Psk;
                structure.DvbsStandard = TbsDvbsStandard.Dvbs2;
            }
            structure.ModulationType = channel2.ModulationType;
            TvLibrary.Log.Log.Debug("  modulation     = {0}", new object[] { channel2.ModulationType });
            if (channel2.Pilot == Pilot.On)
            {
                structure.Pilot = TbsPilot.On;
            }
            else
            {
                structure.Pilot = TbsPilot.Off;
            }
            TvLibrary.Log.Log.Debug("  pilot          = {0}", new object[] { structure.Pilot });
            if (channel2.Rolloff == RollOff.Twenty)
            {
                structure.RollOff = TbsRollOff.Twenty;
            }
            else if (channel2.Rolloff == RollOff.TwentyFive)
            {
                structure.RollOff = TbsRollOff.TwentyFive;
            }
            else if (channel2.Rolloff == RollOff.ThirtyFive)
            {
                structure.RollOff = TbsRollOff.ThirtyFive;
            }
            else
            {
                structure.RollOff = TbsRollOff.Undefined;
            }
            TvLibrary.Log.Log.Debug("  roll-off       = {0}", new object[] { structure.RollOff });
            int hresult = this._propertySet.QuerySupported(BdaExtensionPropertySet, 10, out support);
            if (hresult != 0)
            {
                TvLibrary.Log.Log.Debug("Turbosight: failed to query property support, hr = 0x{0:x} ({1})", new object[] { hresult, HResult.GetDXErrorString(hresult) });
                return channel2;
            }
            if ((support & KSPropertySupport.Set) == ((KSPropertySupport)0))
            {
                TvLibrary.Log.Log.Debug("Turbosight: NBC tuning parameter property not supported", new object[0]);
                return channel2;
            }
            Marshal.StructureToPtr(structure, this._generalBuffer, true);
            hresult = this._propertySet.Set(BdaExtensionPropertySet, 10, this._generalBuffer, 20, this._generalBuffer, 20);
            if (hresult == 0)
            {
                TvLibrary.Log.Log.Debug("Turbosight: result = success", new object[0]);
                return channel2;
            }
            TvLibrary.Log.Log.Debug("Turbosight: result = failure, hr = 0x{0:x} ({1})", new object[] { hresult, HResult.GetDXErrorString(hresult) });
            return channel2;
        }

        #endregion

        #region Public properties

        public bool IsTurbosight
        {
            get
            {
                return this._isTurbosight;
            }
        }


        #endregion

    }
}
