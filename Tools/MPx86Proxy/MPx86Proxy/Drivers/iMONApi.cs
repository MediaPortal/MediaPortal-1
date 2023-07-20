using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MPx86Proxy.Drivers
{
    public class iMONApi
    {
        [DllImport("iMONRemoteControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RcResult IMON_RcApi_Init(IntPtr hwndNoti, UInt32 uMsgNotification);

        [DllImport("iMONRemoteControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RcResult IMON_RcApi_IsInited();

        [DllImport("iMONRemoteControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RcResult IMON_RcApi_IsPluginModeEnabled();

        [DllImport("iMONRemoteControl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern RcResult IMON_RcApi_Uninit();


        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_Init(IntPtr hwndNoti, UInt32 uMsgNotification);

        [DllImport("iMONDisplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern DSPResult IMON_Display_Uninit();

        [DllImport("iMONDisplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern DSPResult IMON_Display_IsInited();

        [DllImport("iMONDisplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern DSPResult IMON_Display_IsPluginModeEnabled();

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetVfdText([MarshalAs(UnmanagedType.LPWStr)] string lpsz1stLine, [MarshalAs(UnmanagedType.LPWStr)] string lpsz2ndLine);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetVfdEqData(ref DspEqData pEqData);

        [DllImport("iMONDisplay.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern DSPResult IMON_Display_SetLcdText([MarshalAs(UnmanagedType.LPWStr)] string lpszText);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdEqData(ref DspEqData pEqDataL, ref DspEqData pEqDataR);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdAllIcons(bool bOn);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdOrangeIcon(byte btIconData1, byte btIconData2);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdMediaTypeIcon(byte btIconData);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdSpeakerIcon(byte btIconData1, byte btIconData2);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdVideoCodecIcon(byte btIconData);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdAudioCodecIcon(byte btIconData);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdAspectRatioIcon(byte btIconData);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdEtcIcon(byte btIconData);

        [DllImport("iMONDisplay.dll")]
        public static extern DSPResult IMON_Display_SetLcdProgress(int nCurPos, int nTotal);



        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct DspEqData
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
            public int[] BandData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IDWINITRESULT
        {
            public int initResult;
            public int dspType;
        }


        public enum RcResult
        {
            RCR_SUCCEEDED = 0,				//// Function Call Succeeded Without Error
            RCR_E_FAIL,						//// Unspecified Failure
            RCR_E_OUTOFMEMORY,				//// Failed to Allocate Necessary Memory
            RCR_E_INVALIDARG,				//// One or More Arguments Are Not Valid
            RCR_E_NOT_INITED,				//// API is Not Initialized
            RCR_E_POINTER,					//// Pointer is Not Valid

            RCR_S_INITED = 0x1000,			//// API is Initialized
            RCR_S_NOT_INITED,				//// API is Not Initialized
            RCR_S_IN_PLUGIN_MODE,			//// API Can Control iMON Remote Control (Remote Control Plug-in Mode)
            RCR_S_NOT_IN_PLUGIN_MODE,		//// API Can't Control iMON Remote Control
        };

        public enum RCNInitResult
        {
            RCN_SUCCEEDED = 0,				//// Remote Control Plug-in Mode is Initialized Successfully
            RCN_ERR_IN_USED = 0x0100,		//// Remote Control Plug-in is Already Used by Other Application
            RCN_ERR_HW_DISCONNECTED,		//// iMON HW is Not Connected
            RCN_ERR_NOT_SUPPORTED_HW,		//// The Connected iMON HW doesn't Support Remote Control Plug-in
            RCN_ERR_PLUGIN_DISABLED,		//// Remote Control Plug-in Mode Option is Disabled
            RCN_ERR_IMON_NO_REPLY,			//// The Latest iMON is Not Installed or iMON Not Running
            RCN_ERR_UNKNOWN = 0x0200,		//// Unknown Failure
        };

        public enum RCRemote
        {
            REMOTE_NONE = 0,
            REMOTE_IMON_RC = 101,			//// iMON RC Remote
            REMOTE_IMON_RSC = 102,			//// iMON RSC Remote
            REMOTE_IMON_MM = 107,			//// iMON MM Remote
            REMOTE_IMON_EX = 112,			//// iMON EX Remote
            REMOTE_IMON_PAD = 115,			//// iMON PAD Remote
            REMOTE_IMON_24G = 116,			//// iMON 2.4G Remote
            REMOTE_MCE = 119,				//// MCE Remote
            REMOTE_IMON_MINI = 124,			//// iMON Mini Remote
        };

        public enum RCButton
        {
            RC_BTN_NONE = 0,
            RC_BTN_PLAY = 2,				//// Play
            RC_BTN_PAUSE,					//// Pause
            RC_BTN_STOP,					//// Stop
            RC_BTN_PREV,					//// Prev, Reply
            RC_BTN_NEXT,					//// Next, Skip
            RC_BTN_REW,						//// Rewind, Rew
            RC_BTN_FF,						//// Forward, F.F
            RC_BTN_EJECT,					//// Eject
            RC_BTN_VOL_UP,					//// Volume Up
            RC_BTN_VOL_DOWN,				//// Volume Down
            RC_BTN_MUTE,					//// Mute
            RC_BTN_FILE_OPEN,				//// Open
            RC_BTN_RECORD,					//// Record
            RC_BTN_QUICK_LAUNCH,			//// Quick Launch
            RC_BTN_CH_UP,					//// Channel Up
            RC_BTN_CH_DOWN,					//// Channel Down
            RC_BTN_A_UP,					//// Up
            RC_BTN_A_DOWN,					//// Down

            RC_BTN_A_LEFT = 20,				//// Left
            RC_BTN_A_RIGHT,					//// Right
            RC_BTN_ENTER,					//// Enter
            RC_BTN_BACKSPACE,				//// Backspace, Back
            RC_BTN_ESC,						//// Esc
            RC_BTN_SPACE,					//// Space
            RC_BTN_LAUNCHER,				//// App. Launcher
            RC_BTN_SWITCHER,				//// Task Switcher
            RC_BTN_TIMER,					//// Timer
            RC_BTN_SHIFT_TAB,				//// Shift Tab
            RC_BTN_TAB,						//// Tab
            RC_BTN_MYMUSIC,					//// Music, My Music
            RC_BTN_MYMOVIE,					//// Videos, My Movie
            RC_BTN_MYPHOTO,					//// Pictures, My Photo
            RC_BTN_MYTV,					//// TV, My TV
            RC_BTN_THUMBNAIL = 36,			//// Thumbnail
            RC_BTN_ASPECT,					//// Zoom, Aspect Ratio
            RC_BTN_FULLSCR,					//// Full Screen
            RC_BTN_MYDVD,					//// DVD, My DVD

            RC_BTN_DVD_MENU = 40,			//// DVD Menu
            RC_BTN_CAPTION,					//// Subtitle, DVD Caption
            RC_BTN_LANG,					//// Audio, DVD Language
            RC_BTN_APP_EXIT,				//// App. Exit
            RC_BTN_WINDOWS,					//// Windows
            RC_BTN_MENU,					//// Menu, More, Detail, Info
            RC_BTN_MOUSEKBD,				//// Mouse / Keyboard
            RC_BTN_NUM1,					//// Number 1
            RC_BTN_NUM2,					//// Number 2
            RC_BTN_NUM3,					//// Number 3
            RC_BTN_NUM4,					//// Number 4
            RC_BTN_NUM5,					//// Number 5
            RC_BTN_NUM6,					//// Number 6
            RC_BTN_NUM7,					//// Number 7
            RC_BTN_NUM8,					//// Number 8
            RC_BTN_NUM9,					//// Number 9
            RC_BTN_NUM0,					//// Number 0
            RC_BTN_POWER,					//// Power
            RC_BTN_BOOKMARK,				//// Bookmark

            RC_BTN_PLAY_PAUSE = 60,			//// Play/Pause
            RC_BTN_STAR = 81,				//// Star (*)
            RC_BTN_SHARP,					//// Sharp (#)
            RC_BTN_RADIO = 86,				//// Radio
            RC_BTN_LIVE_TV,					//// Live TV
            RC_BTN_RECORDED_TV,				//// Recorded TV
            RC_BTN_GUIDE,					//// TV Guide
            RC_BTN_NUM_CLEAR,				//// Clear
            RC_BTN_NUM_PRINT,				//// Print
            RC_BTN_NUM_ENTER,				//// Enter (Numpad)
            RC_BTN_MESSENGER,				//// Messenger
            RC_BTN_VIDEOTEXT,				//// Videotext
            RC_BTN_VT_RED,					//// Red (Videotext)
            RC_BTN_VT_GREEN,				//// Green (Videotext)
            RC_BTN_VT_YELLOW,				//// Yellow (Videotext)
            RC_BTN_VT_BLUE,					//// Blue (Videotext)

            RC_BTN_USER_01 = 100,			//// Custom Buttons for Wise Receiver feature 
            RC_BTN_USER_02,					//// iMON products doesn't support this feature
            RC_BTN_USER_03,					////
            RC_BTN_USER_04,					////
            RC_BTN_USER_05,					////
            RC_BTN_USER_06,					////
            RC_BTN_USER_07,					////
            RC_BTN_USER_08,					////
            RC_BTN_USER_09,					////
            RC_BTN_USER_10,					////

            RC_BTN_VISUALIZATION = 110,		//// Visualization
            RC_BTN_SLIDESHOW,				//// Slideshow
            RC_BTN_ANGLE,					//// Angle

            RC_VKNOB_PUSH = 0x1000,			//// Volume Knob Push
            RC_VKNOB_LONG,					//// Volume Knob Long Push
            RC_VKNOB_ROTATE_L,				//// Volume Knob Rotate Left
            RC_VKNOB_ROTATE_R,				//// Volume Knob Rotate Right
            RC_NKNOB_PUSH,					//// Navigation Knob Push
            RC_NKNOB_LONG,					//// Navigation Knob Long Push
            RC_NKNOB_ROTATE_L,				//// Navigation Knob Rotate Left
            RC_NKNOB_ROTATE_R,				//// Navigation Knob Rotate Right
        };

        public enum RCNotifyCode
        {
            /**RCNM_PLUGIN_SUCCEED
            @brief	When API succeeds to get the control for remote control, API will post caller-specified message with RCNM_PLUGIN_SUCCEED as WPARAM parameter.\n
            LPARAM is not used.*/
            RCNM_PLUGIN_SUCCEED = 0,

            /**RCNM_PLUGIN_FAILED
            @brief	When API fails to get the control for remote control, API will post caller-specified message with RCNM_PLUGIN_FAILED as WPARAM parameter.\n
            LPARAM represents error code with RCNInitResult.*/
            RCNM_PLUGIN_FAILED,

            /**RCNM_IMON_RESTARTED
            @brief	When iMON starts, API will post caller-specified message with RCNM_IMON_RESTARTED as WPARAM parameter.\n
            LPARAM is not used.*/
            RCNM_IMON_RESTARTED,

            /**RCNM_IMON_CLOSED
            @brief	When iMON closed, API will post caller-specified message with RCNM_IMON_CLOSED as WPARAM parameter.\n
            LPARAM is not used.*/
            RCNM_IMON_CLOSED,

            /**RCNM_HW_CONNECTED
            @brief	When iMON HW newly connected, API will post caller-specified message with RCNM_HW_CONNECTED as WPARAM parameter.\n
            LPARAM is not used.*/
            RCNM_HW_CONNECTED,

            /**RCNM_HW_DISCONNECTED
            @brief	When iMON HW disconnected, API will post caller-specified message with RCNM_HW_DISCONNECTED as WPARAM parameter.\n
            LPARAM is RCNInitResult value, RCN_ERR_HW_DISCONNECTED.*/
            RCNM_HW_DISCONNECTED,

            /**RCNM_RC_REMOTE
            @brief	Plug-in mode is initialized or user change the remote selection in iMON Manager,\n
            API will post caller-specified message with RCNM_RC_REMOTE as WPARAM parameter.\n
            LPARAM is one of RCRemote values or other number for OEM remote.*/
            RCNM_RC_REMOTE = 0x1000,

            /**RCNM_RC_BUTTON_DOWN
            @brief	When user press the remote button, API will post caller-specified message with RCNM_RC_BUTTON_DOWN as WPARAM parameter.\n
            LPARAM is one of RCButton values.*/
            RCNM_RC_BUTTON_DOWN,

            /**RCNM_RC_BUTTON_UP
            @brief	When user release the remote button, API will post caller-specified message with RCNM_RC_BUTTON_UP as WPARAM parameter.\n
            LPARAM is one of RCButton values.*/
            RCNM_RC_BUTTON_UP,

            /**RCNM_KNOB_ACTION
            @brief	When user press or rotate Knob, API will post caller-specified message with RCNM_KNOB_ACTION as WPARAM parameter.\n
            LPARAM is one of RCButton values.*/
            RCNM_KNOB_ACTION,
        };


        public enum DSPResult
        {
            DSP_SUCCEEDED = 0,				//// Function Call Succeeded Without Error
            DSP_E_FAIL,						//// Unspecified Failure
            DSP_E_OUTOFMEMORY,				//// Failed to Allocate Necessary Memory
            DSP_E_INVALIDARG,				//// One or More Arguments Are Not Valid
            DSP_E_NOT_INITED,				//// API is Not Initialized
            DSP_E_POINTER,					//// Pointer is Not Valid

            DSP_S_INITED = 0x1000,			//// API is Initialized
            DSP_S_NOT_INITED,				//// API is Not Initialized
            DSP_S_IN_PLUGIN_MODE,			//// API Can Control iMON Display (Display Plug-in Mode)
            DSP_S_NOT_IN_PLUGIN_MODE,		//// API Can't Control iMON Display
        };

        public enum DSPNInitResult
        {
            DSPN_SUCCEEDED = 0,				//// Display Plug-in Mode is Initialized Successfully
            DSPN_ERR_IN_USED = 0x0100,		//// Display Plug-in is Already Used by Other Application
            DSPN_ERR_HW_DISCONNECTED,		//// iMON HW is Not Connected
            DSPN_ERR_NOT_SUPPORTED_HW,		//// The Connected iMON HW doesn't Support Display Plug-in
            DSPN_ERR_PLUGIN_DISABLED,		//// Display Plug-in Mode Option is Disabled
            DSPN_ERR_IMON_NO_REPLY,			//// The Latest iMON is Not Installed or iMON Not Running
            DSPN_ERR_UNKNOWN = 0x0200,		//// Unknown Failure
        };

        public enum DSPType
        {
            DSPN_DSP_NONE = 0,
            DSPN_DSP_VFD = 0x01,			//// VFD products
            DSPN_DSP_LCD = 0x02,			//// LCD products
        };

        public enum DSPNotifyCode
        {
            /**DSPNM_PLUGIN_SUCCEED
            @brief	When API succeeds to get the control for the display, API will post caller-specified message with DSPNM_PLUGIN_SUCCEED as WPARAM parameter.\n
                        LPARAM represents DSPType. This value can be 0x01 (VFD), 0x02 (LCD) or 0x03 (VFD+LCD).*/
            DSPNM_PLUGIN_SUCCEED = 0,

            /**DSPNM_PLUGIN_FAILED
            @brief	When API fails to get the control for the display, API will post caller-specified message with DSPNM_PLUGIN_FAILED as WPARAM parameter.\n
                        LPARAM represents error code with DSPNResult.*/
            DSPNM_PLUGIN_FAILED,

            /**DSPNM_IMON_RESTARTED
            @brief	When iMON starts, API will post caller-specified message with DSPNM_IMON_RESTARTED as WPARAM parameter.\n
                        LPARAM represents DSPType. This value can be 0 (No Display), 0x01 (VFD), 0x02 (LCD) or 0x03 (VFD+LCD).*/
            DSPNM_IMON_RESTARTED,

            /**DSPNM_IMON_CLOSED
            @brief	When iMON closed, API will post caller-specified message with DSPNM_IMON_CLOSED as WPARAM parameter.\n
                        LPARAM is not used.*/
            DSPNM_IMON_CLOSED,

            /**DSPNM_HW_CONNECTED
            @brief	When iMON HW newly connected, API will post caller-specified message with DSPNM_HW_CONNECTED as WPARAM parameter.\n
                        LPARAM represents DSPType. This value can be 0 (No Display), 0x01 (VFD), 0x02 (LCD) or 0x03 (VFD+LCD).*/
            DSPNM_HW_CONNECTED,

            /**DSPNM_HW_DISCONNECTED
            @brief	When iMON HW disconnected, API will post caller-specified message with DSPNM_HW_DISCONNECTED as WPARAM parameter.\n
                        LPARAM is DSPNResult value, DSPN_ERR_HW_DISCONNECTED.*/
            DSPNM_HW_DISCONNECTED,


            /**DSPNM_LCD_TEXT_SCROLL_DONE
            @brief	When iMON LCD finishes scrolling Text, API will post caller-specified message with DSPNM_LCD_TEXT_SCROLL_DONE as WPARAM parameter.\n
                        The caller application may need to know when text scroll is finished, for sending next text.\n
                        LPARAM is not used.*/
            DSPNM_LCD_TEXT_SCROLL_DONE = 0x1000,
        };
    }
}
