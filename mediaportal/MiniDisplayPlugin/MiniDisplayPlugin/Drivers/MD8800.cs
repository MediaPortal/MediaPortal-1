namespace CybrDisplayPlugin.Drivers
{
    using CybrDisplayPlugin;
    using MediaPortal.Configuration;
    using MediaPortal.GUI.Library;
    using System;
    using System.Drawing;
    using System.IO;
    using System.IO.Ports;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Serialization;

    public class MD8800 : BaseDisplay, IDisplay, IDisposable
    {
        private bool _BlankDisplayOnExit;
        private Thread _DisplayThread;
        private readonly string _ErrorMessage = "";
        private bool _IsDisabled;
        private bool _IsDisplayOff;
        private bool _IsOpen;
        private bool _mpIsIdle;
        public static bool _stopUpdateEqThread;
        private int _Tcols;
        private int _Trows;
        private bool _UseBackLight;
        private int _UseBackLightLevel;
        private bool _UseContrast;
        private int _UseContrastLevel;
        private string _UsePort = string.Empty;
        private MiniDisplay.DisplayControl DisplaySettings;
        private bool DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
        private object DWriteMutex = new object();
        private string IdleMessage = string.Empty;
        private DateTime LastSettingsCheck = DateTime.Now;
        private int LastVolLevel;
        private readonly MD8800_Display MD = new MD8800_Display();
        private MiniDisplay.SystemStatus MPStatus = new MiniDisplay.SystemStatus();
        private DateTime SettingsLastModTime;
        private object ThreadMutex = new object();
        private int volLevel;

        private void AdvancedSettings_OnSettingsChanged()
        {
            Log.Info("MD8800.AdvancedSettings_OnSettingsChanged(): called", new object[0]);
            this.CleanUp();
            this.LoadAdvancedSettings();
            Thread.Sleep(100);
            this.Setup(Settings.Instance.Port, Settings.Instance.TextHeight, Settings.Instance.TextWidth, Settings.Instance.TextComDelay, Settings.Instance.GraphicHeight, Settings.Instance.GraphicWidth, Settings.Instance.GraphicComDelay, Settings.Instance.BackLightControl, Settings.Instance.Backlight, Settings.Instance.ContrastControl, Settings.Instance.Contrast, Settings.Instance.BlankOnExit);
            this.Initialize();
        }

        public void CleanUp()
        {
            AdvancedSettings.OnSettingsChanged -= new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
            if (this.DisplaySettings.BlankDisplayWithVideo)
            {
                while (this._DisplayThread.IsAlive)
                {
                    Log.Info("MD8800.Cleanup(): Stoping Display_Update() Thread", new object[0]);
                    lock (this.ThreadMutex)
                    {
                        _stopUpdateEqThread = true;
                    }
                    _stopUpdateEqThread = true;
                    Thread.Sleep(500);
                }
            }
            this.MD.ClearDisplay();
            if (!this._BlankDisplayOnExit && ((this.DisplaySettings._Shutdown1 != string.Empty) || (this.DisplaySettings._Shutdown2 != string.Empty)))
            {
                this.MD.SetLine(0, this.DisplaySettings._Shutdown1);
                this.MD.SetLine(1, this.DisplaySettings._Shutdown2);
            }
            this.MD.CloseDisplay(this._BlankDisplayOnExit);
        }

        private void Clear()
        {
            this.MD.ClearDisplay();
        }

        public void Configure()
        {
            Form form = new MD8800_AdvancedSetupForm();
            form.ShowDialog();
            form.Dispose();
        }

        private void Display_Update()
        {
            uint iconBitmap = 0;
            if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
            {
                GUIWindowManager.OnNewAction += new OnActionHandler(this.OnExternalAction);
            }
            while (true)
            {
                lock (this.ThreadMutex)
                {
                    if (this.DoDebug)
                    {
                        Log.Info("MD8800.Display_Update(): Checking for Thread termination request", new object[0]);
                    }
                    if (_stopUpdateEqThread)
                    {
                        if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
                        {
                            GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
                        }
                        if (this.DoDebug)
                        {
                            Log.Info("MD8800.Display_Update(): EQ_Update Thread terminating", new object[0]);
                        }
                        _stopUpdateEqThread = false;
                        return;
                    }
                    MiniDisplay.GetSystemStatus(ref this.MPStatus);
                    iconBitmap = MD8800_Display.ConvertPluginIconsToDisplayIcons(this.MPStatus.CurrentIconMask);
                    if ((!this.MPStatus.MediaPlayer_Active & this.DisplaySettings.BlankDisplayWithVideo) & (this.DisplaySettings.BlankDisplayWhenIdle & !this._mpIsIdle))
                    {
                        this.DisplayOn();
                    }
                    uint num2 = iconBitmap;
                    if (this.DoDebug)
                    {
                        Log.Info("iMONLCDg.UpdateIcons(): Checking TV Card status: IsAnyCardRecording = {0}, IsViewing = {1}", new object[] { MiniDisplay.IsCaptureCardRecording().ToString(), MiniDisplay.IsCaptureCardViewing().ToString() });
                    }
                    if (MiniDisplay.IsCaptureCardRecording())
                    {
                        iconBitmap |= 0x100;
                        if (this.DoDebug)
                        {
                            Log.Info("iMONLCDg.UpdateIcons(): Setting RECORDING icon", new object[0]);
                        }
                    }
                    else if (MiniDisplay.IsCaptureCardViewing())
                    {
                        iconBitmap |= 0x20;
                        if (this.DoDebug)
                        {
                            Log.Info("iMONLCDg.UpdateIcons(): Setting TV icon", new object[0]);
                        }
                    }
                    if (this.DoDebug)
                    {
                        Log.Info("iMONLCDg.UpdateIcons(): Checking g_player status: IsTV = {0}, IsTVRecording = {1}, Playing = {2}, Paused = {3}, IsTimeshifting = {4}", new object[] { this.MPStatus.Media_IsTV.ToString(), this.MPStatus.Media_IsTVRecording.ToString(), this.MPStatus.MediaPlayer_Playing.ToString(), this.MPStatus.MediaPlayer_Paused.ToString(), this.MPStatus.Media_IsTimeshifting.ToString() });
                    }
                    if (!this.MPStatus.MediaPlayer_Active)
                    {
                        this.RestoreDisplayFromVideoOrIdle();
                        lock (this.DWriteMutex)
                        {
                            this._DisplayThread.Priority = ThreadPriority.BelowNormal;
                        }
                        if (this._mpIsIdle)
                        {
                            iconBitmap |= 0x400;
                            if (this.DoDebug)
                            {
                                Log.Info("iMONLCDg.UpdateIcons(): Setting STOP icon", new object[0]);
                            }
                        }
                    }
                    else if (this.MPStatus.MediaPlayer_Playing)
                    {
                        if ((this.MPStatus.Media_IsVideo | this.MPStatus.Media_IsTV) && this.DisplaySettings.BlankDisplayWithVideo)
                        {
                            this.DisplayOff();
                        }
                    }
                    else
                    {
                        if (this.MPStatus.Media_IsVideo | this.MPStatus.Media_IsTV)
                        {
                            this.RestoreDisplayFromVideoOrIdle();
                        }
                        lock (this.DWriteMutex)
                        {
                            this._DisplayThread.Priority = ThreadPriority.BelowNormal;
                        }
                        if (this.DoDebug)
                        {
                            Log.Info("iMONLCDg.UpdateIcons(): Setting PAUSED icon", new object[0]);
                        }
                    }
                    if (!this.MPStatus.MediaPlayer_Playing & !MiniDisplay.IsCaptureCardViewing())
                    {
                        iconBitmap |= 0x400;
                    }
                    if (iconBitmap != num2)
                    {
                        lock (this.DWriteMutex)
                        {
                            this.MD.SetIcons(iconBitmap);
                        }
                    }
                    lock (this.DWriteMutex)
                    {
                        this.ShowVolumeLevel();
                    }
                }
                Thread.Sleep(250);
            }
        }

        private void DisplayOff()
        {
            if (!this._IsDisplayOff)
            {
                if (this.DisplaySettings.EnableDisplayAction & this.DisplaySettings._DisplayControlAction)
                {
                    if ((DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction) < this.DisplaySettings._DisplayControlTimeout)
                    {
                        if (this.DoDebug)
                        {
                            Log.Info("MD8800.DisplayOff(): DisplayControlAction Timer = {0}.", new object[] { DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction });
                        }
                        return;
                    }
                    if (this.DoDebug)
                    {
                        Log.Info("MD8800.DisplayOff(): DisplayControlAction Timeout expired.", new object[0]);
                    }
                    this.DisplaySettings._DisplayControlAction = false;
                    this.DisplaySettings._DisplayControlLastAction = 0L;
                }
                Log.Info("MD8800.DisplayOff(): completed", new object[0]);
                lock (this.DWriteMutex)
                {
                    Log.Info("MD8800.DisplayOff(): Turning display OFF", new object[0]);
                    this.Clear();
                    this._IsDisplayOff = true;
                    this.MD.DisplayOff();
                }
                Log.Info("MD8800.DisplayOff(): completed", new object[0]);
            }
        }

        private void DisplayOn()
        {
            if (this._IsDisplayOff)
            {
                Log.Info("MD8800.DisplayOn(): called", new object[0]);
                lock (this.DWriteMutex)
                {
                    Log.Info("MD8800.DisplayOn(): Turning Display ON", new object[0]);
                    this.MD.DisplayOn();
                }
                this._IsDisplayOff = false;
                Log.Info("MD8800.DisplayOn(): completed", new object[0]);
            }
        }

        public void Dispose()
        {
            this.MD.CloseDisplay(this._BlankDisplayOnExit);
        }

        public void DrawImage(Bitmap bitmap)
        {
        }

        public void Initialize()
        {
            if (this._IsDisabled)
            {
                Log.Info("MD8800.Initialize(): DRIVER DISABLED!", new object[0]);
            }
            else
            {
                this._IsOpen = this.MD.OpenDisplay(this._UsePort, this._UseBackLight, this._UseBackLightLevel, this._UseContrast, this._UseContrastLevel);
                if (this._IsOpen)
                {
                    if (this.DisplaySettings.BlankDisplayWithVideo)
                    {
                        Log.Info("MD8800.Initialize(): starting Display_Update() thread", new object[0]);
                        this._DisplayThread = new Thread(new ThreadStart(this.Display_Update));
                        this._DisplayThread.IsBackground = true;
                        this._DisplayThread.Priority = ThreadPriority.BelowNormal;
                        this._DisplayThread.Name = "Display_Update";
                        this._DisplayThread.Start();
                        if (this._DisplayThread.IsAlive)
                        {
                            Log.Info("MD8800.Initialize(): Display_Update() Thread Started", new object[0]);
                        }
                        else
                        {
                            Log.Info("MD8800.Initialize(): Display_Update() FAILED TO START", new object[0]);
                        }
                    }
                    AdvancedSettings.OnSettingsChanged += new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
                    this.Clear();
                }
            }
        }

        private void LoadAdvancedSettings()
        {
            Log.Info("MD8800.LoadAdvancedSettings(): Called", new object[0]);
            AdvancedSettings settings = AdvancedSettings.Load();
            this.IdleMessage = (Settings.Instance.IdleMessage != string.Empty) ? Settings.Instance.IdleMessage : "MediaPortal";
            this.DisplaySettings.BlankDisplayWithVideo = settings.BlankDisplayWithVideo;
            this.DisplaySettings.EnableDisplayAction = settings.EnableDisplayAction;
            this.DisplaySettings.DisplayActionTime = settings.EnableDisplayActionTime;
            this.DisplaySettings.BlankDisplayWhenIdle = settings.BlankDisplayWhenIdle;
            this.DisplaySettings.BlankIdleDelay = settings.BlankIdleTime;
            this.DisplaySettings._BlankIdleTimeout = this.DisplaySettings.BlankIdleDelay * 0x989680;
            this.DisplaySettings._DisplayControlTimeout = this.DisplaySettings.DisplayActionTime * 0x989680;
            this.DisplaySettings._Shutdown1 = Settings.Instance.Shutdown1;
            this.DisplaySettings._Shutdown2 = Settings.Instance.Shutdown2;
            Log.Info("MD8800.LoadAdvancedSettings(): Extensive Logging: {0}", new object[] { Settings.Instance.ExtensiveLogging });
            Log.Info("MD8800.LoadAdvancedSettings(): Device Port: {0}", new object[] { Settings.Instance.Port });
            Log.Info("MD8800.LoadAdvancedSettings(): Use BackLight Control: {0}", new object[] { this._UseBackLight });
            Log.Info("MD8800.LoadAdvancedSettings(): BackLight Level: {0}", new object[] { this._UseBackLightLevel });
            Log.Info("MD8800.LoadAdvancedSettings(): Blank Display on MediaPortal Exit: {0}", new object[] { this._BlankDisplayOnExit });
            Log.Info("MD8800.LoadAdvancedSettings(): Shutdown Message - Line 1: {0}", new object[] { this.DisplaySettings._Shutdown1 });
            Log.Info("MD8800.LoadAdvancedSettings(): Shutdown Message - Line 2: {0}", new object[] { this.DisplaySettings._Shutdown2 });
            Log.Info("MD8800.LoadAdvancedSettings(): Advanced options - Blank display with video: {0}", new object[] { this.DisplaySettings.BlankDisplayWithVideo });
            Log.Info("MD8800.LoadAdvancedSettings(): Advanced options -   Enable Display on Action: {0}", new object[] { this.DisplaySettings.EnableDisplayAction });
            Log.Info("MD8800.LoadAdvancedSettings(): Advanced options -     Enable display for: {0} seconds", new object[] { this.DisplaySettings._DisplayControlTimeout });
            Log.Info("MD8800.LoadAdvancedSettings(): Advanced options - Blank display when idle: {0}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle });
            Log.Info("MD8800.LoadAdvancedSettings(): Advanced options -     blank display after: {0} seconds", new object[] { this.DisplaySettings._BlankIdleTimeout / 0xf4240L });
            Log.Info("MD8800.LoadAdvancedSettings(): Completed", new object[0]);
            FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "CybrDisplay_MD8800.xml"));
            this.SettingsLastModTime = info.LastWriteTime;
            this.LastSettingsCheck = DateTime.Now;
        }

        private void OnExternalAction(Action action)
        {
            if (this.DisplaySettings.EnableDisplayAction)
            {
                if (this.DoDebug)
                {
                    Log.Info("MD8800.OnExternalAction(): received action {0}", new object[] { action.wID.ToString() });
                }
                Action.ActionType wID = action.wID;
                if (wID <= Action.ActionType.ACTION_SHOW_OSD)
                {
                    if ((wID != Action.ActionType.ACTION_SHOW_INFO) && (wID != Action.ActionType.ACTION_SHOW_OSD))
                    {
                        return;
                    }
                }
                else if (((wID != Action.ActionType.ACTION_SHOW_MPLAYER_OSD) && (wID != Action.ActionType.ACTION_KEY_PRESSED)) && (wID != Action.ActionType.ACTION_MOUSE_CLICK))
                {
                    return;
                }
                this.DisplaySettings._DisplayControlAction = true;
                this.DisplaySettings._DisplayControlLastAction = DateTime.Now.Ticks;
                if (this.DoDebug)
                {
                    Log.Info("MD8800.OnExternalAction(): received DisplayControlAction", new object[0]);
                }
                this.DisplayOn();
            }
        }

        private void RestoreDisplayFromVideoOrIdle()
        {
            if (this.DisplaySettings.BlankDisplayWithVideo)
            {
                if (this.DisplaySettings.BlankDisplayWhenIdle)
                {
                    if (!this._mpIsIdle)
                    {
                        this.DisplayOn();
                    }
                }
                else
                {
                    this.DisplayOn();
                }
            }
        }

        public void SetCustomCharacters(int[][] customCharacters)
        {
        }

        public void SetLine(int line, string message)
        {
            if (this._IsDisabled)
            {
                if (this.DoDebug)
                {
                    Log.Info("MD8800.SetLine(): Unable to display text - driver disabled", new object[0]);
                }
            }
            else
            {
                this.UpdateAdvancedSettings();
                if (this.DoDebug)
                {
                    Log.Info("MD8800.SetLine() Called", new object[0]);
                }
                if (this._IsDisplayOff)
                {
                    if (this.DoDebug)
                    {
                        Log.Info("MD8800.SetLine(): Suppressing display update!", new object[0]);
                    }
                }
                else
                {
                    if (this.DoDebug)
                    {
                        Log.Info("MD8800.SetLine(): Line {0} - Message = \"{1}\"", new object[] { line, message });
                    }
                    this.MD.SetLine(line, message);
                    if (this.DoDebug)
                    {
                        Log.Info("MD8800.SetLine(): message sent to display", new object[0]);
                    }
                }
                if (line == (this._Trows - 1))
                {
                    MiniDisplay.GetSystemStatus(ref this.MPStatus);
                }
                if ((line == (this._Trows - 1)) && this.MPStatus.MP_Is_Idle)
                {
                    if (this.DoDebug)
                    {
                        Log.Info("MD8800.SetLine(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle, this.DisplaySettings._BlankIdleTimeout });
                    }
                    if (this.DisplaySettings.BlankDisplayWhenIdle)
                    {
                        if (!this._mpIsIdle)
                        {
                            if (this.DoDebug)
                            {
                                Log.Info("MD8800.SetLine(): MP going IDLE", new object[0]);
                            }
                            this.DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
                        }
                        if (!this._IsDisplayOff && ((DateTime.Now.Ticks - this.DisplaySettings._BlankIdleTime) > this.DisplaySettings._BlankIdleTimeout))
                        {
                            if (this.DoDebug)
                            {
                                Log.Info("MD8800.SetLine(): Blanking display due to IDLE state", new object[0]);
                            }
                            this.DisplayOff();
                        }
                    }
                    this._mpIsIdle = true;
                }
                else
                {
                    if (this.DisplaySettings.BlankDisplayWhenIdle & this._mpIsIdle)
                    {
                        if (this.DoDebug)
                        {
                            Log.Info("MD8800.SetLine(): MP no longer IDLE - restoring display", new object[0]);
                        }
                        this.DisplayOn();
                    }
                    this._mpIsIdle = false;
                }
            }
        }

        public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG, bool _backLight, int _backLightLevel, bool _contrast, int _contrastLevel, bool _blankOnExit)
        {
            this.DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
            Log.Info("{0}", new object[] { this.Description });
            Log.Info("MD8800.Setup(): called", new object[0]);
            MiniDisplay.InitDisplayControl(ref this.DisplaySettings);
            this._BlankDisplayOnExit = _blankOnExit;
            this._UseBackLight = _backLight;
            this._UseBackLightLevel = _backLightLevel;
            this._UseContrast = _contrast;
            this._UseContrastLevel = _contrastLevel;
            this.LoadAdvancedSettings();
            this._Trows = _lines;
            if (this._Trows != 2)
            {
                Log.Info("MD8800.Setup() - Invalid Text Lines value", new object[0]);
                this._Trows = 2;
            }
            this._Tcols = _cols;
            if (this._Tcols != 0x10)
            {
                Log.Info("MD8800.Setup() - Invalid Text Columns value", new object[0]);
                this._Tcols = 0x10;
            }
            this._IsOpen = false;
            this._UsePort = _port;
            try
            {
                this._IsOpen = this.MD.OpenDisplay(this._UsePort, this._UseBackLight, this._UseBackLightLevel, this._UseContrast, this._UseContrastLevel);
            }
            catch (Exception exception)
            {
                Log.Info("MD8800.Setup() - CAUGHT EXCEPTION opening display port!: {0}", new object[] { exception });
            }
            if (this._IsOpen)
            {
                this._IsDisabled = false;
                this._IsOpen = false;
                this.MD.CloseDisplay(this._BlankDisplayOnExit);
            }
            else
            {
                this._IsDisabled = true;
            }
            Log.Info("MD8800.Setup(): completed", new object[0]);
        }

        private void ShowVolumeLevel()
        {
            this.volLevel = 0;
            if (this.MPStatus.MediaPlayer_Playing || MiniDisplay.IsCaptureCardViewing())
            {
                try
                {
                    if (!this.MPStatus.IsMuted)
                    {
                        this.volLevel = this.MPStatus.SystemVolumeLevel / 0x2000;
                    }
                }
                catch (Exception exception)
                {
                    if (this.DoDebug)
                    {
                        Log.Info("MD8800.ShowVolumeLevel(): Audio Mixer NOT available! exception: {0}", new object[] { exception });
                    }
                }
            }
            if (this.LastVolLevel != this.volLevel)
            {
                if (this.DoDebug)
                {
                    Log.Info("MD8800.ShowVolumeLevel(): Sending volume = {0} to VFD.", new object[] { this.volLevel.ToString() });
                }
                this.MD.SetVolume(this.volLevel);
            }
            this.LastVolLevel = this.volLevel;
        }

        private void UpdateAdvancedSettings()
        {
            if (DateTime.Now.Ticks >= this.LastSettingsCheck.AddMinutes(1.0).Ticks)
            {
                if (this.DoDebug)
                {
                    Log.Info("MD8800.UpdateAdvancedSettings(): called", new object[0]);
                }
                if (File.Exists(Config.GetFile(Config.Dir.Config, "CybrDisplay_MD8800.xml")))
                {
                    FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "CybrDisplay_MD8800.xml"));
                    if (info.LastWriteTime.Ticks > this.SettingsLastModTime.Ticks)
                    {
                        if (this.DoDebug)
                        {
                            Log.Info("MD8800.UpdateAdvancedSettings(): updating advanced settings", new object[0]);
                        }
                        this.LoadAdvancedSettings();
                    }
                }
                if (this.DoDebug)
                {
                    Log.Info("MD8800.UpdateAdvancedSettings(): completed", new object[0]);
                }
            }
        }

        public string Description
        {
            get
            {
                return "Medion MD8800 (Dritek) VFD driver v04_17_2008";
            }
        }

        public string ErrorMessage
        {
            get
            {
                return this._ErrorMessage;
            }
        }

        public bool IsDisabled
        {
            get
            {
                return this._IsDisabled;
            }
        }

        public string Name
        {
            get
            {
                return "MD8800";
            }
        }

        public bool SupportsGraphics
        {
            get
            {
                return false;
            }
        }

        public bool SupportsText
        {
            get
            {
                return true;
            }
        }

        [Serializable]
        public class AdvancedSettings
        {
            private bool m_BlankDisplayWhenIdle;
            private bool m_BlankDisplayWithVideo;
            private int m_BlankIdleTime = 30;
            private bool m_EnableDisplayAction;
            private int m_EnableDisplayActionTime = 5;
            private static MD8800.AdvancedSettings m_Instance;

            public static  event OnSettingsChangedHandler OnSettingsChanged;

            private static void Default(MD8800.AdvancedSettings _settings)
            {
                Log.Info("MD8800.AdvancedSettings.Default(): called", new object[0]);
                _settings.BlankDisplayWithVideo = false;
                _settings.EnableDisplayAction = false;
                _settings.EnableDisplayActionTime = 5;
                _settings.BlankDisplayWhenIdle = false;
                _settings.BlankIdleTime = 30;
                Log.Info("MD8800.AdvancedSettings.Default(): completed", new object[0]);
            }

            public static MD8800.AdvancedSettings Load()
            {
                MD8800.AdvancedSettings settings;
                Log.Info("MD8800.AdvancedSettings.Load(): started", new object[0]);
                if (File.Exists(Config.GetFile(Config.Dir.Config, "CybrDisplay_MD8800.xml")))
                {
                    Log.Info("MD8800.AdvancedSettings.Load(): Loading settings from XML file", new object[0]);
                    XmlSerializer serializer = new XmlSerializer(typeof(MD8800.AdvancedSettings));
                    XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "CybrDisplay_MD8800.xml"));
                    settings = (MD8800.AdvancedSettings) serializer.Deserialize(xmlReader);
                    xmlReader.Close();
                }
                else
                {
                    Log.Info("MD8800.AdvancedSettings.Load(): Loading settings from defaults", new object[0]);
                    settings = new MD8800.AdvancedSettings();
                    Default(settings);
                }
                Log.Info("MD8800.AdvancedSettings.Load(): completed", new object[0]);
                return settings;
            }

            public static void NotifyDriver()
            {
                if (OnSettingsChanged != null)
                {
                    OnSettingsChanged();
                }
            }

            public static void Save()
            {
                Save(Instance);
            }

            public static void Save(MD8800.AdvancedSettings ToSave)
            {
                Log.Info("MD8800.AdvancedSettings.Save(): Saving settings to XML file", new object[0]);
                XmlSerializer serializer = new XmlSerializer(typeof(MD8800.AdvancedSettings));
                XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "CybrDisplay_MD8800.xml"), Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                serializer.Serialize((XmlWriter) writer, ToSave);
                writer.Close();
                Log.Info("MD8800.AdvancedSettings.Save(): completed", new object[0]);
            }

            public static void SetDefaults()
            {
                Default(Instance);
            }

            [XmlAttribute]
            public bool BlankDisplayWhenIdle
            {
                get
                {
                    return this.m_BlankDisplayWhenIdle;
                }
                set
                {
                    this.m_BlankDisplayWhenIdle = value;
                }
            }

            [XmlAttribute]
            public bool BlankDisplayWithVideo
            {
                get
                {
                    return this.m_BlankDisplayWithVideo;
                }
                set
                {
                    this.m_BlankDisplayWithVideo = value;
                }
            }

            [XmlAttribute]
            public int BlankIdleTime
            {
                get
                {
                    return this.m_BlankIdleTime;
                }
                set
                {
                    this.m_BlankIdleTime = value;
                }
            }

            [XmlAttribute]
            public bool EnableDisplayAction
            {
                get
                {
                    return this.m_EnableDisplayAction;
                }
                set
                {
                    this.m_EnableDisplayAction = value;
                }
            }

            [XmlAttribute]
            public int EnableDisplayActionTime
            {
                get
                {
                    return this.m_EnableDisplayActionTime;
                }
                set
                {
                    this.m_EnableDisplayActionTime = value;
                }
            }

            public static MD8800.AdvancedSettings Instance
            {
                get
                {
                    if (m_Instance == null)
                    {
                        m_Instance = Load();
                    }
                    return m_Instance;
                }
                set
                {
                    m_Instance = value;
                }
            }

            public delegate void OnSettingsChangedHandler();
        }

        private class MD8800_Display
        {
            private int _currentBrightness = 0x80;
            private string _DisplayLine0 = "                ";
            private string _DisplayLine1 = "                ";
            private bool _IsDisplayOff;
            private bool _isOpen;
            private bool _UseBacklightControl;
            private SerialPort commPort;

            public void ClearDisplay()
            {
                if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff) && this._UseBacklightControl)
                {
                    this.commPort.Write(new byte[] { 0x1b, 0x51 }, 0, 2);
                    this.commPort.Write(new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, 0, 8);
                    this.commPort.Write(new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, 0, 8);
                    this.commPort.Write(new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, 0, 8);
                    this.commPort.Write(new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, 0, 8);
                }
            }

            public void CloseDisplay(bool _blankOnExit)
            {
                try
                {
                    if (this._isOpen & this.commPort.IsOpen)
                    {
                        this.ClearDisplay();
                        if (_blankOnExit)
                        {
                            this.SetBacklightBrightness(0);
                            this.commPort.Write(new byte[] { 0x1b, 0x53 }, 0, 2);
                        }
                        else
                        {
                            DateTime now = DateTime.Now;
                            this.commPort.Write(new byte[] { 0x1b, 2 }, 0, 2);
                            Thread.Sleep(50);
                            byte[] buffer = new byte[8];
                            buffer[0] = 0x1b;
                            buffer[2] = (byte) now.Minute;
                            buffer[3] = (byte) now.Hour;
                            buffer[4] = (byte) now.Month;
                            buffer[5] = (byte) now.Day;
                            buffer[6] = (byte) Math.Floor((double) (now.Year / 100));
                            buffer[7] = (byte) (now.Year - (Math.Floor((double) (now.Year / 100)) * 100.0));
                            this.commPort.Write(buffer, 0, 3);
                            Thread.Sleep(50);
                            this.commPort.Write(new byte[] { 0x1b, 5 }, 0, 2);
                        }
                        if ((this.commPort != null) && this.commPort.IsOpen)
                        {
                            this.commPort.Close();
                        }
                        this._isOpen = false;
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                }
            }

            public static uint ConvertPluginIconsToDisplayIcons(ulong PluginIconMask)
            {
                uint num = 0;
                if ((PluginIconMask & ((ulong) 0x10L)) > 0L)
                {
                    num += 4;
                }
                if ((PluginIconMask & ((ulong) 0x40L)) > 0L)
                {
                    num += 0x10;
                }
                if ((PluginIconMask & ((ulong) 8L)) > 0L)
                {
                    num += 0x20;
                }
                if ((PluginIconMask & ((ulong) 0x80L)) > 0L)
                {
                    num += 0x40;
                }
                if ((PluginIconMask & ((ulong) 0x20L)) > 0L)
                {
                    num += 0x80;
                }
                if ((PluginIconMask & ((ulong) 0x400000000L)) > 0L)
                {
                    num += 0x100;
                }
                if ((PluginIconMask & ((ulong) 0x100000000000L)) > 0L)
                {
                    num += 0x200;
                }
                if ((PluginIconMask & ((ulong) 0x40000000000L)) > 0L)
                {
                    num += 0x400;
                }
                if ((PluginIconMask & ((ulong) 0x80000000000L)) > 0L)
                {
                    num += 0x800;
                }
                if ((PluginIconMask & ((ulong) 0x20000000000L)) > 0L)
                {
                    num += 0x1000;
                }
                if ((PluginIconMask & ((ulong) 0x10000000000L)) > 0L)
                {
                    num += 0x2000;
                }
                return num;
            }

            public void DisplayOff()
            {
                this._IsDisplayOff = true;
                this.ClearDisplay();
                this.SetIcons(0);
                this.SetBacklightBrightness(0);
            }

            public void DisplayOn()
            {
                this.SetBacklightBrightness(this._currentBrightness);
                this._IsDisplayOff = false;
            }

            public bool OpenDisplay(string _port, bool _useBacklight, int _backlightLevel, bool _useContrast, int _contrastLevel)
            {
                try
                {
                    this._UseBacklightControl = _useBacklight;
                    this._currentBrightness = _backlightLevel;
                    this.commPort = new SerialPort(_port, 0x2580, Parity.None, 8, StopBits.One);
                    this.commPort.Open();
                    this.commPort.Write(new byte[] { 0x1f }, 0, 1);
                    Thread.Sleep(50);
                    this.commPort.Write(new byte[] { 0x1b, 0x20 }, 0, 2);
                    Thread.Sleep(50);
                    this.commPort.Write(new byte[] { 0x1b, 0x52 }, 0, 2);
                    this.SetBacklightBrightness(this._currentBrightness);
                    this.ClearDisplay();
                    this._isOpen = true;
                    this._IsDisplayOff = false;
                }
                catch (Exception exception)
                {
                    Log.Info("MD8800.MD8800_Display.OpenDisplay(): CAUGHT EXCEPTION while opening display! - {0}", new object[] { exception });
                    this._isOpen = false;
                }
                return this._isOpen;
            }

            public void SetBacklightBrightness(int brightness)
            {
                if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
                {
                    brightness = (int) Math.Floor((double) (brightness / 0x2a));
                    this.commPort.Write(new byte[] { 0x1b, 0x40, (byte) brightness }, 0, 3);
                }
            }

            public void SetContrast(int contrast)
            {
            }

            public void SetIcons(uint IconBitmap)
            {
                if (!this._IsDisplayOff)
                {
                    byte[] buffer = new byte[4];
                    buffer[0] = 0x1b;
                    buffer[1] = 0x30;
                    buffer[3] = ((IconBitmap & 1) > 0) ? ((byte) this._currentBrightness) : ((byte) 0);
                    this.commPort.Write(buffer, 0, 3);
                    byte[] buffer2 = new byte[] { 0x1b, 0x30, 1, 0 };
                    buffer2[3] = ((IconBitmap & 2) > 0) ? ((byte) this._currentBrightness) : ((byte) 0);
                    this.commPort.Write(buffer2, 0, 3);
                    byte[] buffer3 = new byte[] { 0x1b, 0x30, 2, 0 };
                    buffer3[3] = ((IconBitmap & 4) > 0) ? ((byte) this._currentBrightness) : ((byte) 0);
                    this.commPort.Write(buffer3, 0, 3);
                    byte[] buffer4 = new byte[] { 0x1b, 0x30, 3, 0 };
                    buffer4[3] = ((IconBitmap & 8) > 0) ? ((byte) this._currentBrightness) : ((byte) 0);
                    this.commPort.Write(buffer4, 0, 3);
                    byte[] buffer5 = new byte[] { 0x1b, 0x30, 4, 0 };
                    buffer5[3] = ((IconBitmap & 0x10) > 0) ? ((byte) this._currentBrightness) : ((byte) 0);
                    this.commPort.Write(buffer5, 0, 3);
                    byte[] buffer6 = new byte[] { 0x1b, 0x30, 5, 0 };
                    buffer6[3] = ((IconBitmap & 0x20) > 0) ? ((byte) this._currentBrightness) : ((byte) 0);
                    this.commPort.Write(buffer6, 0, 3);
                    byte[] buffer7 = new byte[] { 0x1b, 0x30, 6, 0 };
                    buffer7[3] = ((IconBitmap & 0x40) > 0) ? ((byte) this._currentBrightness) : ((byte) 0);
                    this.commPort.Write(buffer7, 0, 3);
                    byte[] buffer8 = new byte[] { 0x1b, 0x30, 7, 0 };
                    buffer8[3] = ((IconBitmap & 0x80) > 0) ? ((byte) this._currentBrightness) : ((byte) 0);
                    this.commPort.Write(buffer8, 0, 3);
                    byte[] buffer9 = new byte[] { 0x1b, 0x30, 8, 0 };
                    buffer9[3] = ((IconBitmap & 0x100) > 0) ? ((byte) this._currentBrightness) : ((byte) 0);
                    this.commPort.Write(buffer9, 0, 3);
                    if ((IconBitmap & 0x200) > 0)
                    {
                        this.commPort.Write(new byte[] { 0x1b, 0x31, 0, 0, 8, 0x1c, 0x3e, 0x7f, 0, 0, 0 }, 0, 11);
                    }
                    else if ((IconBitmap & 0x400) > 0)
                    {
                        this.commPort.Write(new byte[] { 0x1b, 0x31, 0, 0x3e, 0x3e, 0x3e, 0x3e, 0x3e, 0, 0, 0 }, 0, 11);
                    }
                    else if ((IconBitmap & 0x800) > 0)
                    {
                        this.commPort.Write(new byte[] { 0x1b, 0x31, 0, 0x3e, 0x3e, 0, 0x3e, 0x3e, 0, 0, 0 }, 0, 11);
                    }
                    else if ((IconBitmap & 0x1000) > 0)
                    {
                        this.commPort.Write(new byte[] { 0x1b, 0x31, 0, 8, 0x1c, 0x3e, 8, 0x1c, 0x3e, 0, 0 }, 0, 11);
                    }
                    else if ((IconBitmap & 0x2000) > 0)
                    {
                        this.commPort.Write(new byte[] { 0x1b, 0x31, 0, 0x3e, 0x1c, 8, 0x3e, 0x1c, 8, 0, 0 }, 0, 11);
                    }
                    else
                    {
                        byte[] buffer10 = new byte[11];
                        buffer10[0] = 0x1b;
                        buffer10[1] = 0x31;
                        this.commPort.Write(buffer10, 0, 11);
                    }
                }
            }

            public void SetLine(int _Line, string _Message)
            {
                _Message = _Message + "                ";
                _Message = _Message.Substring(0, 0x10);
                if (_Line == 0)
                {
                    this._DisplayLine0 = _Message;
                }
                else
                {
                    this._DisplayLine1 = _Message;
                    this.ShowDisplay(this._DisplayLine0, this._DisplayLine1);
                }
            }

            public void SetVolume(int Volume)
            {
                if (Volume > 8)
                {
                    Volume = 8;
                }
                this.commPort.Write(new byte[] { 0x1b, 0x30, 11, 0 }, 0, 4);
                this.commPort.Write(new byte[] { 0x1b, 0x30, 12, 0 }, 0, 4);
                this.commPort.Write(new byte[] { 0x1b, 0x30, 13, 0 }, 0, 4);
                this.commPort.Write(new byte[] { 0x1b, 0x30, 14, 0 }, 0, 4);
                this.commPort.Write(new byte[] { 0x1b, 0x30, 15, 0 }, 0, 4);
                this.commPort.Write(new byte[] { 0x1b, 0x30, 0x10, 0 }, 0, 4);
                this.commPort.Write(new byte[] { 0x1b, 0x30, 0x11, 0 }, 0, 4);
                this.commPort.Write(new byte[] { 0x1b, 0x30, 0x12, 0 }, 0, 4);
                if (Volume > 0)
                {
                    this.commPort.Write(new byte[] { 0x1b, 0x30, 11, 1 }, 0, 4);
                }
                if (Volume > 1)
                {
                    this.commPort.Write(new byte[] { 0x1b, 0x30, 12, 1 }, 0, 4);
                }
                if (Volume > 2)
                {
                    this.commPort.Write(new byte[] { 0x1b, 0x30, 13, 1 }, 0, 4);
                }
                if (Volume > 3)
                {
                    this.commPort.Write(new byte[] { 0x1b, 0x30, 14, 1 }, 0, 4);
                }
                if (Volume > 4)
                {
                    this.commPort.Write(new byte[] { 0x1b, 0x30, 15, 1 }, 0, 4);
                }
                if (Volume > 5)
                {
                    this.commPort.Write(new byte[] { 0x1b, 0x30, 0x10, 1 }, 0, 4);
                }
                if (Volume > 6)
                {
                    this.commPort.Write(new byte[] { 0x1b, 0x30, 0x11, 1 }, 0, 4);
                }
                if (Volume > 7)
                {
                    this.commPort.Write(new byte[] { 0x1b, 0x30, 0x12, 1 }, 0, 4);
                }
            }

            public void ShowDisplay(string _Line1, string _Line2)
            {
                if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
                {
                    this.SetBacklightBrightness(this._currentBrightness);
                    this.commPort.Write(new byte[] { 0x1b, 0x51 }, 0, 2);
                    this.commPort.Write(_Line1);
                    this.commPort.Write(_Line2);
                    for (int i = 0; i < 0x10; i++)
                    {
                        this.commPort.Write(new byte[] { (byte) _Line1[i] }, 0, 1);
                    }
                    for (int j = 0; j < 0x10; j++)
                    {
                        this.commPort.Write(new byte[] { (byte) _Line2[j] }, 0, 1);
                    }
                }
            }

            public enum Display_Icons : uint
            {
                CDROM = 4,
                FIREWIRE = 2,
                HDD = 1,
                MM_FFWD = 0x1000,
                MM_PAUSE = 0x800,
                MM_PLAY = 0x200,
                MM_RWND = 0x2000,
                MM_STOP = 0x400,
                MOVIE = 0x10,
                MUSIC = 0x40,
                PHOTO = 0x80,
                RECORDING = 0x100,
                TV = 0x20,
                USB = 8
            }
        }
    }
}

