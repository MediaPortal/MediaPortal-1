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

        protected int DisplayType { get; set; }

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
                    int ret = IDW_Init(initResult);
                    if (ret != DSP_SUCCEEDED)
                    {
                        ImonErrorMessage = DSPResult2String(ret);
                        Disabled = true;
                        LogError(string.Format("{0}.IsDisabled: {1}", ClassErrorName, ImonErrorMessage));
                    }
                    else if (initResult.initResult != DSPN_SUCCEEDED)
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

        protected string DSPResult2String(int result)
        {
            switch (result)
            {
                case DSP_SUCCEEDED:
                    return "Success";
                case DSP_E_FAIL:
                    return "An unknown error occurred";
                case DSP_E_OUTOFMEMORY:
                    return "Out of memory";
                case DSP_E_INVALIDARG:
                    return "One or more arguments are invalid";
                case DSP_E_NOT_INITED:
                    return "API is not initialized";
                case DSP_E_POINTER:
                    return "Pointer is invalid";
                default:
                    return "An unknown error occurred";
            }
        }

        protected string DSPNResult2String(int result)
        {
            switch (result)
            {
                case DSPN_SUCCEEDED:
                    return "Success";
                case DSPN_ERR_IN_USING:
                    return "Display plug-in is already used by another application";
                case DSPN_ERR_HW_DISCONNECTED:
                    return "iMON hardware is not connected";
                case DSPN_ERR_NOT_SUPPORTED_HW:
                    return "The connected hardware does not support the plug-in mode";
                case DSPN_ERR_PLUGIN_DISABLED:
                    return "The plug-in mode option is disabled";
                case DSPN_ERR_IMON_NO_REPLY:
                    return "The latest iMON is not installed or iMON is not running";
                default:
                    return "An unknown error occurred";
            }
        }

        protected const int DSP_SUCCEEDED = 0;
        protected const int DSP_E_FAIL = 1;
        protected const int DSP_E_OUTOFMEMORY = 2;
        protected const int DSP_E_INVALIDARG = 3;
        protected const int DSP_E_NOT_INITED = 4;
        protected const int DSP_E_POINTER = 5;
        protected const int DSP_S_INITED = 0x1000;
        protected const int DSP_S_NOT_INITED = 0x1001;
        protected const int DSP_S_IN_PLUGIN_MODE = 0x1002;
        protected const int DSP_S_NOT_IN_PLUGIN_MODE = 0x1003;

        protected const int DSPN_SUCCEEDED = 0;
        protected const int DSPN_ERR_IN_USING = 0x0100;
        protected const int DSPN_ERR_HW_DISCONNECTED = 0x0101;
        protected const int DSPN_ERR_NOT_SUPPORTED_HW = 0x0102;
        protected const int DSPN_ERR_PLUGIN_DISABLED = 0x0103;
        protected const int DSPN_ERR_IMON_NO_REPLY = 0x0104;
        protected const int DSPN_ERR_UNKNOWN = 0x0200;

        protected const int DSPN_DSP_NONE = 0;
        protected const int DSPN_DSP_VFD = 0x01;
        protected const int DSPN_DSP_LCD = 0x02;

        [StructLayout(LayoutKind.Sequential)]
        protected class IDWINITRESULT
        {
            public int initResult;
            public int dspType;
        }

        [DllImport("iMONDisplayWrapper.dll")]
        protected static extern int IDW_Init(IDWINITRESULT initResult);

        [DllImport("iMONDisplayWrapper.dll")]
        protected static extern int IDW_Uninit();
    }
}
