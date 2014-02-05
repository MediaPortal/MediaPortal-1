using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using MediaPortal.GUI.Library;


namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    /// <summary>
    /// Base Class for iMON Display API in iMON Manager >= 8.01.0419
    /// </summary>
    public abstract class ImonBase : IDisplay
    {
        public ImonBase()
        {
            Disabled = null;
            ImonErrorMessage = string.Empty;
            Initialized = false;
            Line1 = string.Empty;
            Line2 = string.Empty;
        }

        protected static void LogDebug(string msg) {Log.Debug(msg);}
        protected static void LogInfo(string msg) {Log.Info(msg);}
        protected static void LogError(string msg) {Log.Error(msg);}

        protected bool? Disabled { get; set; }

        protected string ImonErrorMessage { get; set;}

        protected bool Initialized { get; set; }

        protected string Line1 { get; set; }

        protected string Line2 { get; set; }

        public string Description { get; protected set; }

        protected string ClassErrorName { get; set; }

        protected string UnsupportedDeviceErrorMessage { get; set; }

        protected DSPType DisplayType { get; set; }

        public string ErrorMessage
        {
            get
            {
                if (IsDisabled)
                {
                    return ImonErrorMessage;
                }
                return string.Empty;
            }
        }

        public bool IsDisabled
        {
            get
            {
                if (!Disabled.HasValue)
                {
                    IDWINITRESULT initResult = new IDWINITRESULT();
                    DSPResult ret = IDW_Init(initResult);
                    if (ret != DSPResult.DSP_SUCCEEDED)
                    {
                        ImonErrorMessage = DSPResult2String(ret);
                        Disabled = true;
                        LogError(string.Format("{0}.IsDisabled: {1}", ClassErrorName, ImonErrorMessage));
                    }
                    else if (initResult.initResult != DSPNInitResult.DSPN_SUCCEEDED)
                    {
                        ImonErrorMessage = DSPNResult2String(initResult.initResult);
                        Disabled = true;
                        LogError(string.Format("{0}.IsDisabled: {1}", ClassErrorName, ImonErrorMessage));
                    }
                    else if (initResult.dspType != DisplayType)
                    {
                        ImonErrorMessage = UnsupportedDeviceErrorMessage;
                        Disabled = true;
                        IDW_Uninit();
                        LogError(string.Format("{0}.IsDisabled: {1}", ClassErrorName, ImonErrorMessage));
                    }
                    else
                    {
                        Disabled = false;
                        IDW_Uninit();
                    }

                }
                return Disabled.Value;
            }
        }

        public string Name { get; protected set; }

        public bool SupportsGraphics { get { return false; } }

        public bool SupportsText { get { return true; } }

        public virtual void Dispose()
        {
            CleanUp();
        }

        public virtual void Initialize()
        {
            LogDebug("(IDisplay) ImonBase.Initialize(): called");
            if (Initialized)
            {
                LogDebug("(IDisplay) ImonBase.Initialize(): already initialized, skipping");
                return;
            }
            if (IsDisabled)
            {
                LogDebug("(IDisplay) ImonBase.Initialize(): driver disabled, cannot initialize");
                return;
            }
            IDWINITRESULT initResult = new IDWINITRESULT();
            IDW_Init(initResult);
            Initialized = true;
            LogDebug("(IDisplay) ImonBase.Initialize(): completed");
        }

        public virtual void CleanUp()
        {
            LogDebug("(IDisplay) ImonBase.CleanUp(): called");
            if (!Initialized)
            {
                return;
            }
            IDW_Uninit();
            Initialized = false;
            Log.Debug("(IDisplay) ImonBase.CleanUp(): completed");
        }

        public abstract void SetLine(int line, string message);

        public virtual void Configure()
        {
            // No configuration possible/necessary
        }

        public void DrawImage(Bitmap bitmap)
        {
            // Not supported
        }

        public void SetCustomCharacters(int[][] customCharacters)
        {
            // Not supported
        }

        public void Setup(string port,
          int lines, int cols, int delay,
          int linesG, int colsG, int timeG,
          bool backLight, int backLightLevel,
          bool contrast, int contrastLevel,
          bool BlankOnExit)
        {
            // iMON VFD/LCD cannot be setup
        }

        protected string DSPResult2String(DSPResult result)
        {
            switch (result)
            {
                case DSPResult.DSP_SUCCEEDED:
                    return "Success";
                case DSPResult.DSP_E_FAIL:
                    return "An unknown error occurred";
                case DSPResult.DSP_E_OUTOFMEMORY:
                    return "Out of memory";
                case DSPResult.DSP_E_INVALIDARG:
                    return "One or more arguments are invalid";
                case DSPResult.DSP_E_NOT_INITED:
                    return "API is not initialized";
                case DSPResult.DSP_E_POINTER:
                    return "Pointer is invalid";
                default:
                    return "An unknown error occurred";
            }
        }

        protected string DSPNResult2String(DSPNInitResult result)
        {
            switch (result)
            {
                case DSPNInitResult.DSPN_SUCCEEDED:
                    return "Success";
                case DSPNInitResult.DSPN_ERR_IN_USE:
                    return "Display plug-in is already used by another application";
                case DSPNInitResult.DSPN_ERR_HW_DISCONNECTED:
                    return "iMON hardware is not connected";
                case DSPNInitResult.DSPN_ERR_NOT_SUPPORTED_HW:
                    return "The connected hardware does not support the plug-in mode";
                case DSPNInitResult.DSPN_ERR_PLUGIN_DISABLED:
                    return "The plug-in mode option is disabled";
                case DSPNInitResult.DSPN_ERR_IMON_NO_REPLY:
                    return "The latest iMON is not installed or iMON is not running";
                default:
                    return "An unknown error occurred";
            }
        }

        //Possible return values from iMON Display APIs
        protected enum DSPResult : int
        {
            DSP_SUCCEEDED = 0,
            DSP_E_FAIL = 1,
            DSP_E_OUTOFMEMORY = 2,
            DSP_E_INVALIDARG = 3,
            DSP_E_NOT_INITED = 4,
            DSP_E_POINTER = 5,
            DSP_S_INITED = 0x1000,
            DSP_S_NOT_INITED = 0x1001,
            DSP_S_IN_PLUGIN_MODE = 0x1002,
            DSP_S_NOT_IN_PLUGIN_MODE = 0x1003
        }

        //Possible results from display initialization 
        protected enum DSPNInitResult : int
        {
            DSPN_SUCCEEDED = 0,
            DSPN_ERR_IN_USE = 0x0100,
            DSPN_ERR_HW_DISCONNECTED = 0x0101,
            DSPN_ERR_NOT_SUPPORTED_HW = 0x0102,
            DSPN_ERR_PLUGIN_DISABLED = 0x0103,
            DSPN_ERR_IMON_NO_REPLY = 0x0104,
            DSPN_ERR_UNKNOWN = 0x0200
        }

        //Type of display
        protected enum DSPType : int
        {
            DSPN_DSP_NONE = 0,
            DSPN_DSP_VFD = 0x01,
            DSPN_DSP_LCD = 0x02
        };

        //Not sure why we had to revert the order of our data members from the native implementation.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
        protected class IDWINITRESULT
        {
            [MarshalAs(UnmanagedType.U4)]
            public DSPNInitResult initResult;
            [MarshalAs(UnmanagedType.U4)]
            public DSPType dspType;
        }

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        protected static extern DSPResult IDW_Init(IDWINITRESULT initResult);

        [DllImport("iMONDisplayWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        protected static extern DSPResult IDW_Uninit();
    }
}
