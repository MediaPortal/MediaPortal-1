using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class GenericSerial : BaseDisplay, IDisplay, IDisposable
  {
    private bool _BackLightControl = false;
    //private int _BackLightLevel = 0x7f;
    private bool _BlankDisplayOnExit;
    //private bool _ContrastControl;
    //private int _ContrastLevel = 0x7f;
    private Thread _DisplayThread;
    private readonly string _ErrorMessage = "";
    private bool _IsDisabled;
    private bool _IsDisplayOff;
    private bool _IsOpen;
    private bool _mpIsIdle;
    public static bool _stopDisplayThread;
    private int _Tcols;
    private int _Trows;
    private CommandStrings CommandSet;
    private MiniDisplay.DisplayControl DisplaySettings;
    private bool DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
    private object DWriteMutex = new object();
    private object EqWriteMutex = new object();
    private readonly GSDisplay GSD = new GSDisplay();
    private string IdleMessage = string.Empty;
    private DateTime LastSettingsCheck = DateTime.Now;
    private MiniDisplay.SystemStatus MPStatus = new MiniDisplay.SystemStatus();
    private DateTime SettingsLastModTime;
    private object ThreadMutex = new object();

    private void AdvancedSettings_OnSettingsChanged()
    {
      Log.Info("GenericSerial.AdvancedSettings_OnSettingsChanged(): called", new object[0]);
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
          Log.Info("GenericSerial.Cleanup(): Stoping Display_Update() Thread", new object[0]);
          lock (this.ThreadMutex)
          {
            _stopDisplayThread = true;
          }
          _stopDisplayThread = true;
          Thread.Sleep(500);
        }
      }
      this.GSD.ClearDisplay();
      if (!this._BlankDisplayOnExit && ((this.DisplaySettings._Shutdown1 != string.Empty) || (this.DisplaySettings._Shutdown2 != string.Empty)))
      {
        this.GSD.SetLine(0, this.DisplaySettings._Shutdown1);
        this.GSD.SetLine(1, this.DisplaySettings._Shutdown2);
      }
      this.GSD.CloseDisplay(this._BlankDisplayOnExit);
    }

    private void Clear()
    {
      this.GSD.ClearDisplay();
    }

    public void Configure()
    {
      Form form = new GenericSerial_AdvancedSetupForm();
      form.ShowDialog();
      form.Dispose();
    }

    private ulong ConvertPluginIconsToDriverIcons(ulong IconMask)
    {
      return 0L;
    }

    private void Display_Update()
    {
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
            Log.Info("GenericSerial.Display_Update(): Checking for Thread termination request", new object[0]);
          }
          if (_stopDisplayThread)
          {
            if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
            {
              GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
            }
            if (this.DoDebug)
            {
              Log.Info("GenericSerial.Display_Update(): EQ_Update Thread terminating", new object[0]);
            }
            _stopDisplayThread = false;
            return;
          }
          MiniDisplay.GetSystemStatus(ref this.MPStatus);
          if ((!this.MPStatus.MediaPlayer_Active & this.DisplaySettings.BlankDisplayWithVideo) & (this.DisplaySettings.BlankDisplayWhenIdle & !this._mpIsIdle))
          {
            this.DisplayOn();
          }
          if (this.MPStatus.MediaPlayer_Playing)
          {
            if (this.DisplaySettings.BlankDisplayWithVideo & (((this.MPStatus.Media_IsDVD || this.MPStatus.Media_IsVideo) || this.MPStatus.Media_IsTV) || this.MPStatus.Media_IsTVRecording))
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.Display_Update(): Turning off display while playing video", new object[0]);
              }
              this.DisplayOff();
            }
          }
          else
          {
            this.RestoreDisplayFromVideoOrIdle();
          }
        }
        if (this.MPStatus.MediaPlayer_Paused)
        {
          Thread.Sleep(250);
        }
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
              Log.Info("GenericSerial.DisplayOff(): DisplayControlAction Timer = {0}.", new object[] { DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction });
            }
            return;
          }
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.DisplayOff(): DisplayControlAction Timeout expired.", new object[0]);
          }
          this.DisplaySettings._DisplayControlAction = false;
          this.DisplaySettings._DisplayControlLastAction = 0L;
        }
        lock (this.DWriteMutex)
        {
          Log.Info("GenericSerial.DisplayOff(): Turning display OFF", new object[0]);
          this.Clear();
          this._IsDisplayOff = true;
          this.GSD.DisplayOff();
        }
        Log.Info("GenericSerial.DisplayOff(): completed", new object[0]);
      }
    }

    private void DisplayOn()
    {
      if (this._IsDisplayOff)
      {
        Log.Info("GenericSerial.DisplayOn(): called", new object[0]);
        lock (this.DWriteMutex)
        {
          Log.Info("GenericSerial.DisplayOn(): Turning Display ON", new object[0]);
          this.GSD.DisplayOn();
        }
        this._IsDisplayOff = false;
        Log.Info("GenericSerial.DisplayOn(): completed", new object[0]);
      }
    }

    public void Dispose()
    {
      this.GSD.CloseDisplay(this._BackLightControl);
    }

    public void DrawImage(Bitmap bitmap)
    {
    }

    private void InitCommandSet(ref CommandStrings CommandSet)
    {
      CommandSet.BaudRate = string.Empty;
      CommandSet.Parity = "None";
      CommandSet.DataBits = "8";
      CommandSet.StopBits = "1";
      CommandSet.CommDelay = Settings.Instance.TextComDelay;
      CommandSet.CMD_DisplayInit = string.Empty;
      CommandSet.CMD_ClearDisplay = string.Empty;
      CommandSet.CMD_CursorLeft = string.Empty;
      CommandSet.CMD_CursorRight = string.Empty;
      CommandSet.CMD_CursorUp = string.Empty;
      CommandSet.CMD_CursorDown = string.Empty;
      CommandSet.CMD_CursorHome = string.Empty;
      CommandSet.CMD_ToggleRTS = false;
      CommandSet.CMD_ToggleDTR = false;
    }

    public void Initialize()
    {
      this.Clear();
    }

    private void LoadAdvancedSettings()
    {
      Log.Info("GenericSerial.LoadAdvancedSettings(): Called", new object[0]);
      AdvancedSettings settings = AdvancedSettings.Load();
      this.IdleMessage = (Settings.Instance.IdleMessage != string.Empty) ? Settings.Instance.IdleMessage : "MediaPortal";
      this.CommandSet.BaudRate = settings.BaudRate;
      this.CommandSet.Parity = settings.Parity;
      this.CommandSet.StopBits = settings.StopBits;
      this.CommandSet.DataBits = settings.DataBits;
      this.CommandSet.CommDelay = Settings.Instance.TextComDelay;
      this.CommandSet.AssertRTS = settings.AssertRTS;
      this.CommandSet.AssertDTR = settings.AssertDTR;
      this.CommandSet.PositionBase = settings.PositionBase1 ? 1 : 0;
      this.CommandSet.CMD_DisplayInit = settings.CMD_DisplayInit;
      this.CommandSet.CMD_ClearDisplay = settings.CMD_ClearDisplay;
      this.CommandSet.CMD_CursorLeft = settings.CMD_CursorLeft;
      this.CommandSet.CMD_CursorRight = settings.CMD_CursorRight;
      this.CommandSet.CMD_CursorUp = settings.CMD_CursorUp;
      this.CommandSet.CMD_CursorDown = settings.CMD_CursorDown;
      this.CommandSet.CMD_CursorHome = settings.CMD_CursorHome;
      this.CommandSet.CMD_CursorSet = settings.CMD_CursorSet;
      this.CommandSet.CMD_DisplayClose = settings.CMD_DisplayClose;
      this.CommandSet.CMD_ToggleRTS = settings.CMD_ToggleRTS;
      this.CommandSet.CMD_ToggleDTR = settings.CMD_ToggleDTR;
      this.DisplaySettings.BlankDisplayWithVideo = settings.BlankDisplayWithVideo;
      this.DisplaySettings.EnableDisplayAction = settings.EnableDisplayAction;
      this.DisplaySettings.DisplayActionTime = settings.EnableDisplayActionTime;
      this.DisplaySettings.BlankDisplayWhenIdle = settings.BlankDisplayWhenIdle;
      this.DisplaySettings.BlankIdleDelay = settings.BlankIdleTime;
      this.DisplaySettings._BlankIdleTimeout = this.DisplaySettings.BlankIdleDelay * 0x989680;
      this.DisplaySettings._DisplayControlTimeout = this.DisplaySettings.DisplayActionTime * 0x989680;
      this.DisplaySettings._Shutdown1 = Settings.Instance.Shutdown1;
      this.DisplaySettings._Shutdown2 = Settings.Instance.Shutdown2;
      Log.Info("GenericSerial.LoadAdvancedSettings(): Extensive Logging: {0}", new object[] { Settings.Instance.ExtensiveLogging });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Device Port: {0}", new object[] { Settings.Instance.Port });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Device Baud Rate: {0}", new object[] { this.CommandSet.BaudRate });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Device Parity: {0}", new object[] { this.CommandSet.Parity });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Device Stop Bits: {0}", new object[] { this.CommandSet.StopBits });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Device Data Bits: {0}", new object[] { this.CommandSet.DataBits });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Device RTS control: Assert RTS = {0}, Toggle RTS = {1}", new object[] { this.CommandSet.AssertRTS.ToString(), this.CommandSet.CMD_ToggleRTS.ToString() });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Device DTR control: Assert DTR = {0}, Toggle DTR = {1}", new object[] { this.CommandSet.AssertDTR.ToString(), this.CommandSet.CMD_ToggleDTR.ToString() });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Device Home Position: ({0},{1})", new object[] { this.CommandSet.PositionBase, this.CommandSet.PositionBase });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Shutdown Message - Line 1: {0}", new object[] { this.DisplaySettings._Shutdown1 });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Shutdown Message - Line 2: {0}", new object[] { this.DisplaySettings._Shutdown2 });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Advanced options - Blank display with video: {0}", new object[] { this.DisplaySettings.BlankDisplayWithVideo });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Advanced options -   Enable Display on Action: {0}", new object[] { this.DisplaySettings.EnableDisplayAction });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Advanced options -     Enable display for: {0} seconds", new object[] { this.DisplaySettings._DisplayControlTimeout });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Advanced options - Blank display when idle: {0}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Advanced options -     blank display after: {0} seconds", new object[] { this.DisplaySettings._BlankIdleTimeout / 0xf4240L });
      Log.Info("GenericSerial.LoadAdvancedSettings(): Completed", new object[0]);
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_GenericSerial.xml"));
      this.SettingsLastModTime = info.LastWriteTime;
      this.LastSettingsCheck = DateTime.Now;
    }

    private void OnExternalAction(Action action)
    {
      if (this.DisplaySettings.EnableDisplayAction)
      {
        if (this.DoDebug)
        {
          Log.Info("GenericSerial.OnExternalAction(): received action {0}", new object[] { action.wID.ToString() });
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
          Log.Info("GenericSerial.OnExternalAction(): received DisplayControlAction", new object[0]);
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
          Log.Info("GenericSerial.SetLine(): Unable to display text - driver disabled", new object[0]);
        }
      }
      else
      {
        this.UpdateAdvancedSettings();
        MiniDisplay.GetSystemStatus(ref this.MPStatus);
        if (this.DoDebug)
        {
          Log.Info("GenericSerial.SetLine() Called", new object[0]);
        }
        if (this._IsDisplayOff)
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.SetLine(): Suppressing display update!", new object[0]);
          }
        }
        else
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.SetLine(): Line {0} - Message = \"{1}\"", new object[] { line, message });
          }
          this.GSD.SetLine(line, message);
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.SetLine(): message sent to display", new object[0]);
          }
        }
        if ((line == 0) && this.MPStatus.MP_Is_Idle)
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.SetLine(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle, this.DisplaySettings._BlankIdleTimeout });
          }
          if (this.DisplaySettings.BlankDisplayWhenIdle)
          {
            if (!this._mpIsIdle)
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.SetLine(): MP going IDLE", new object[0]);
              }
              this.DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
            }
            if (!this._IsDisplayOff && ((DateTime.Now.Ticks - this.DisplaySettings._BlankIdleTime) > this.DisplaySettings._BlankIdleTimeout))
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.SetLine(): Blanking display due to IDLE state", new object[0]);
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
              Log.Info("GenericSerial.SetLine(): MP no longer IDLE - restoring display", new object[0]);
            }
            this.DisplayOn();
          }
          this._mpIsIdle = false;
        }
      }
    }

    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG, bool _useBackLight, int _useBackLightLevel, bool _useContrast, int _useContrastLevel, bool _blankOnExit)
    {
      this.DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      Log.Info("{0}", new object[] { this.Description });
      Log.Info("GenericSerial.Setup(): called", new object[0]);
      MiniDisplay.InitDisplayControl(ref this.DisplaySettings);
      this.InitCommandSet(ref this.CommandSet);
      this._BlankDisplayOnExit = _blankOnExit;
      this.LoadAdvancedSettings();
      this.CommandSet.CommDelay = _delay;
      this._Trows = _lines;
      this._Tcols = _cols;
      if (this._Trows > 4)
      {
        Log.Info("GenericSerial.Setup() - Invalid Text Lines value", new object[0]);
        this._Trows = 4;
      }
      this._Tcols = _cols;
      if (this._Tcols > 40)
      {
        Log.Info("GenericSerial.Setup() - Invalid Text Columns value", new object[0]);
        this._Tcols = 40;
      }
      this._IsOpen = false;
      try
      {
        this._IsOpen = this.GSD.OpenDisplay(_port, this._Trows, this._Tcols, this.CommandSet);
      } catch (Exception exception)
      {
        Log.Info("GenericSerial.Setup() - CAUGHT EXCEPTION opening serial port!: {0}", new object[] { exception });
      }
      if (this._IsOpen)
      {
        this._IsDisabled = false;
      }
      else
      {
        this._IsDisabled = true;
      }
      if (this.DisplaySettings.BlankDisplayWithVideo & !this._IsDisabled)
      {
        Log.Info("GenericSerial.Setup(): starting Display_Update() thread", new object[0]);
        this._DisplayThread = new Thread(new ThreadStart(this.Display_Update));
        this._DisplayThread.IsBackground = true;
        this._DisplayThread.Priority = ThreadPriority.BelowNormal;
        this._DisplayThread.Name = "Display_Update";
        this._DisplayThread.Start();
        if (this._DisplayThread.IsAlive)
        {
          Log.Info("GenericSerial.Setup(): Display_Update() Thread Started", new object[0]);
        }
        else
        {
          Log.Info("GenericSerial.Setup(): Display_Update() FAILED TO START", new object[0]);
        }
      }
      AdvancedSettings.OnSettingsChanged += new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
      Log.Info("GenericSerial.Setup(): completed", new object[0]);
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= this.LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (this.DoDebug)
        {
          Log.Info("GenericSerial.UpdateAdvancedSettings(): called", new object[0]);
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_GenericSerial.xml")))
        {
          FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_GenericSerial.xml"));
          if (info.LastWriteTime.Ticks > this.SettingsLastModTime.Ticks)
          {
            if (this.DoDebug)
            {
              Log.Info("GenericSerial.UpdateAdvancedSettings(): updating advanced settings", new object[0]);
            }
            this.LoadAdvancedSettings();
          }
        }
        if (this.DoDebug)
        {
          Log.Info("GenericSerial.UpdateAdvancedSettings(): completed", new object[0]);
        }
      }
    }

    public string Description
    {
      get
      {
        return "Generic Serial Character VFD/LCD driver v04_10_2008";
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
        return "GenericSerial";
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
      private bool m_AssertDTR;
      private bool m_AssertRTS;
      private string m_BaudRate = string.Empty;
      private bool m_BlankDisplayWhenIdle;
      private bool m_BlankDisplayWithVideo;
      private int m_BlankIdleTime = 30;
      private string m_CMD_ClearDisplay = string.Empty;
      private string m_CMD_CursorDown = string.Empty;
      private string m_CMD_CursorHome = string.Empty;
      private string m_CMD_CursorLeft = string.Empty;
      private string m_CMD_CursorRight = string.Empty;
      private string m_CMD_CursorSet = string.Empty;
      private string m_CMD_CursorUp = string.Empty;
      private string m_CMD_DisplayClose = string.Empty;
      private string m_CMD_DisplayInit = string.Empty;
      private bool m_CMD_ToggleDTR;
      private bool m_CMD_ToggleRTS;
      private string m_DataBits = string.Empty;
      private bool m_EnableDisplayAction;
      private int m_EnableDisplayActionTime = 5;
      private static GenericSerial.AdvancedSettings m_Instance;
      private string m_Parity = string.Empty;
      private bool m_PositionBase1;
      private string m_StopBits = string.Empty;

      public static event OnSettingsChangedHandler OnSettingsChanged;

      private static void Default(GenericSerial.AdvancedSettings _settings)
      {
        Log.Info("GenericSerial.AdvancedSettings.Default(): called", new object[0]);
        _settings.CMD_DisplayInit = string.Empty;
        _settings.CMD_ClearDisplay = string.Empty;
        _settings.CMD_CursorLeft = string.Empty;
        _settings.CMD_CursorRight = string.Empty;
        _settings.CMD_CursorUp = string.Empty;
        _settings.CMD_CursorDown = string.Empty;
        _settings.CMD_CursorHome = string.Empty;
        _settings.BlankDisplayWithVideo = false;
        _settings.EnableDisplayAction = false;
        _settings.EnableDisplayActionTime = 5;
        _settings.BlankDisplayWhenIdle = false;
        _settings.BlankIdleTime = 30;
        Log.Info("GenericSerial.AdvancedSettings.Default(): completed", new object[0]);
      }

      public static GenericSerial.AdvancedSettings Load()
      {
        GenericSerial.AdvancedSettings settings;
        Log.Info("GenericSerial.AdvancedSettings.Load(): started", new object[0]);
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_GenericSerial.xml")))
        {
          Log.Info("GenericSerial.AdvancedSettings.Load(): Loading settings from XML file", new object[0]);
          XmlSerializer serializer = new XmlSerializer(typeof(GenericSerial.AdvancedSettings));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_GenericSerial.xml"));
          settings = (GenericSerial.AdvancedSettings)serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Info("GenericSerial.AdvancedSettings.Load(): Loading settings from defaults", new object[0]);
          settings = new GenericSerial.AdvancedSettings();
          Default(settings);
        }
        Log.Info("GenericSerial.AdvancedSettings.Load(): completed", new object[0]);
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

      public static void Save(GenericSerial.AdvancedSettings ToSave)
      {
        Log.Info("GenericSerial.AdvancedSettings.Save(): Saving settings to XML file", new object[0]);
        XmlSerializer serializer = new XmlSerializer(typeof(GenericSerial.AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_GenericSerial.xml"), Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter)writer, ToSave);
        writer.Close();
        Log.Info("GenericSerial.AdvancedSettings.Save(): completed", new object[0]);
      }

      public static void SetDefaults()
      {
        Default(Instance);
      }

      [XmlAttribute]
      public bool AssertDTR
      {
        get
        {
          return this.m_AssertDTR;
        }
        set
        {
          this.m_AssertDTR = value;
        }
      }

      [XmlAttribute]
      public bool AssertRTS
      {
        get
        {
          return this.m_AssertRTS;
        }
        set
        {
          this.m_AssertRTS = value;
        }
      }

      [XmlAttribute]
      public string BaudRate
      {
        get
        {
          return this.m_BaudRate;
        }
        set
        {
          this.m_BaudRate = value;
        }
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
      public string CMD_ClearDisplay
      {
        get
        {
          return this.m_CMD_ClearDisplay;
        }
        set
        {
          this.m_CMD_ClearDisplay = value;
        }
      }

      [XmlAttribute]
      public string CMD_CursorDown
      {
        get
        {
          return this.m_CMD_CursorDown;
        }
        set
        {
          this.m_CMD_CursorDown = value;
        }
      }

      [XmlAttribute]
      public string CMD_CursorHome
      {
        get
        {
          return this.m_CMD_CursorHome;
        }
        set
        {
          this.m_CMD_CursorHome = value;
        }
      }

      [XmlAttribute]
      public string CMD_CursorLeft
      {
        get
        {
          return this.m_CMD_CursorLeft;
        }
        set
        {
          this.m_CMD_CursorLeft = value;
        }
      }

      [XmlAttribute]
      public string CMD_CursorRight
      {
        get
        {
          return this.m_CMD_CursorRight;
        }
        set
        {
          this.m_CMD_CursorRight = value;
        }
      }

      [XmlAttribute]
      public string CMD_CursorSet
      {
        get
        {
          return this.m_CMD_CursorSet;
        }
        set
        {
          this.m_CMD_CursorSet = value;
        }
      }

      [XmlAttribute]
      public string CMD_CursorUp
      {
        get
        {
          return this.m_CMD_CursorUp;
        }
        set
        {
          this.m_CMD_CursorUp = value;
        }
      }

      [XmlAttribute]
      public string CMD_DisplayClose
      {
        get
        {
          return this.m_CMD_DisplayClose;
        }
        set
        {
          this.m_CMD_DisplayClose = value;
        }
      }

      [XmlAttribute]
      public string CMD_DisplayInit
      {
        get
        {
          return this.m_CMD_DisplayInit;
        }
        set
        {
          this.m_CMD_DisplayInit = value;
        }
      }

      [XmlAttribute]
      public bool CMD_ToggleDTR
      {
        get
        {
          return this.m_CMD_ToggleDTR;
        }
        set
        {
          this.m_CMD_ToggleDTR = value;
        }
      }

      [XmlAttribute]
      public bool CMD_ToggleRTS
      {
        get
        {
          return this.m_CMD_ToggleRTS;
        }
        set
        {
          this.m_CMD_ToggleRTS = value;
        }
      }

      [XmlAttribute]
      public string DataBits
      {
        get
        {
          return this.m_DataBits;
        }
        set
        {
          this.m_DataBits = value;
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

      public static GenericSerial.AdvancedSettings Instance
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

      [XmlAttribute]
      public string Parity
      {
        get
        {
          return this.m_Parity;
        }
        set
        {
          this.m_Parity = value;
        }
      }

      [XmlAttribute]
      public bool PositionBase1
      {
        get
        {
          return this.m_PositionBase1;
        }
        set
        {
          this.m_PositionBase1 = value;
        }
      }

      [XmlAttribute]
      public string StopBits
      {
        get
        {
          return this.m_StopBits;
        }
        set
        {
          this.m_StopBits = value;
        }
      }

      public delegate void OnSettingsChangedHandler();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CommandStrings
    {
      public string BaudRate;
      public string Parity;
      public string StopBits;
      public string DataBits;
      public bool AssertRTS;
      public bool AssertDTR;
      public int CommDelay;
      public int PositionBase;
      public string CMD_DisplayInit;
      public string CMD_ClearDisplay;
      public string CMD_CursorLeft;
      public string CMD_CursorRight;
      public string CMD_CursorUp;
      public string CMD_CursorDown;
      public string CMD_CursorHome;
      public string CMD_CursorSet;
      public string CMD_DisplayClose;
      public bool CMD_ToggleRTS;
      public bool CMD_ToggleDTR;
    }

    private class GSDisplay
    {
      private int _CurrentColumn;
      private int _CurrentLine;
      private int _DisplayColumns;
      private int _DisplayLines;
      private bool _IsDisplayOff;
      private bool _isOpen;
      private CommandBytes Commands;
      private int CommDelay;
      private SerialPort commPort;
      private bool DoDebug;

      public void ClearDisplay()
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.GSDisplay.ClearDisplay(): Called", new object[0]);
          }
          if (this.Commands.CMD_ClearDisplay == null)
          {
            if ((this._CurrentColumn == 0) & (this._CurrentLine == 0))
            {
              this.SendCharToDisplay(' ');
            }
            while ((this._CurrentLine != 0) & (this._CurrentColumn != 0))
            {
              this.SendCharToDisplay(' ');
            }
            this.SendCharToDisplay(' ');
            while ((this._CurrentLine != 0) & (this._CurrentColumn != 0))
            {
              this.SendCharToDisplay(' ');
            }
          }
          else
          {
            if (this.DoDebug)
            {
              Log.Info("GenericSerial.GSDisplay.ClearDisplay(): using CMD_ClearDisplay to clear the display", new object[0]);
            }
            for (int i = 0; i < this.Commands.CMD_ClearDisplay.Length; i++)
            {
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
              this.commPort.Write(new byte[] { this.Commands.CMD_ClearDisplay[i] }, 0, 1);
              Thread.Sleep(this.CommDelay);
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
            }
          }
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.GSDisplay.ClearDisplay(): Completed", new object[0]);
          }
        }
      }

      public void CloseDisplay(bool _blankOnExit)
      {
        Log.Info("GenericSerial.GSDisplay.CloseDisplay(): called", new object[0]);
        try
        {
          if (this._isOpen & this.commPort.IsOpen)
          {
            if (_blankOnExit)
            {
              Log.Info("GenericSerial.GSDisplay.CloseDisplay(): Clearing display due to _blankOnExit option", new object[0]);
              this.ClearDisplay();
            }
            if (this.Commands.CMD_DisplayClose != null)
            {
              this.SendDataToDisplay(this.Commands.CMD_DisplayClose);
            }
            if ((this.commPort != null) && this.commPort.IsOpen)
            {
              if (this.Commands.AssertDTR)
              {
                this.commPort.DtrEnable = false;
              }
              if (this.Commands.AssertRTS)
              {
                this.commPort.RtsEnable = false;
              }
              Log.Info("GenericSerial.GSDisplay.CloseDisplay(): Closing Comm Port {0}", new object[] { this.commPort.PortName });
              this.commPort.Close();
            }
            this._isOpen = false;
          }
        } catch (Exception exception)
        {
          Log.Error("GenericSerial.GSDisplay.CloseDisplay(): CAUGHT EXCEPTION Closing Comm Port {0}\n{1}", new object[] { this.commPort.PortName, exception });
        }
      }

      private byte[] CommandStringToBytes(string CommandString)
      {
        if (CommandString.Equals(string.Empty))
        {
          return null;
        }
        string[] strArray = CommandString.Trim().Split(new char[] { ' ' });
        if (strArray.Length == 0)
        {
          return null;
        }
        byte[] buffer = new byte[strArray.Length];
        for (int i = 0; i < strArray.Length; i++)
        {
          if (strArray[i].Substring(0, 1).ToLower() == "TL")
          {
            buffer[i] = 0xfb;
          }
          if (strArray[i].Substring(0, 1).ToLower() == "TC")
          {
            buffer[i] = 0xfc;
          }
          if (strArray[i].Substring(0, 1).ToLower() == "PL")
          {
            buffer[i] = 0xfd;
          }
          if (strArray[i].Substring(0, 1).ToLower() == "PC")
          {
            buffer[i] = 0xfe;
          }
          if (strArray[i].Substring(0, 1).ToLower() == "PZ")
          {
            buffer[i] = 0xff;
          }
          if (strArray[i].Substring(0, 1).ToLower() == "x")
          {
            buffer[i] = byte.Parse(strArray[i].Substring(1), NumberStyles.HexNumber);
          }
          else if (strArray[i].Substring(0, 2).ToLower() == "0x")
          {
            buffer[i] = byte.Parse(strArray[i].Substring(2), NumberStyles.HexNumber);
          }
          else
          {
            buffer[i] = byte.Parse(strArray[i], NumberStyles.HexNumber);
          }
        }
        return buffer;
      }

      public void CursorDown()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.GSDisplay.CursorDown(): Called", new object[0]);
          }
          if (this.Commands.CMD_CursorDown == null)
          {
            if (this.Commands.CMD_CursorSet == null)
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorDown(): can't move cursor down", new object[0]);
              }
            }
            else
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorDown(): using CMD_CursorSet to move cursor down", new object[0]);
              }
              this._CurrentLine++;
              if (this._CurrentLine > (this._DisplayLines - 1))
              {
                this._CurrentLine = 0;
              }
              this.SendDataToDisplay(this.Commands.CMD_CursorSet);
            }
          }
          else
          {
            if (this.DoDebug)
            {
              Log.Info("GenericSerial.GSDisplay.CursorDown(): Using CMD_CursorDown to move cursor down", new object[0]);
            }
            for (int i = 0; i < this.Commands.CMD_CursorDown.Length; i++)
            {
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
              this.commPort.Write(new byte[] { this.Commands.CMD_CursorDown[i] }, 0, 1);
              Thread.Sleep(this.CommDelay);
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
            }
            this._CurrentLine++;
            if (this._CurrentLine > (this._DisplayLines - 1))
            {
              this._CurrentLine = 0;
            }
          }
        }
      }

      public void CursorHome()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.GSDisplay.CursorHome(): Called", new object[0]);
          }
          if (this.Commands.CMD_CursorHome == null)
          {
            if (this.DoDebug)
            {
              Log.Info("GenericSerial.GSDisplay.CursorHome(): Unable to move cursor to home position", new object[0]);
            }
          }
          else
          {
            if (this.DoDebug)
            {
              Log.Info("GenericSerial.GSDisplay.CursorHome(): Using CMD_CursorHome to move cursor to home position", new object[0]);
            }
            for (int i = 0; i < this.Commands.CMD_CursorHome.Length; i++)
            {
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
              this.commPort.Write(new byte[] { this.Commands.CMD_CursorHome[i] }, 0, 1);
              Thread.Sleep(this.CommDelay);
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
            }
            this._CurrentLine = 0;
            this._CurrentColumn = 0;
          }
        }
      }

      public void CursorLeft()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.GSDisplay.CursorLeft(): Called", new object[0]);
          }
          if (this.Commands.CMD_CursorLeft == null)
          {
            if (this.Commands.CMD_CursorSet == null)
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorLeft(): can't move cursor left", new object[0]);
              }
            }
            else
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorLeft(): Using CMD_CursorSet to move cursor left", new object[0]);
              }
              this._CurrentColumn--;
              if (this._CurrentColumn < 0)
              {
                this._CurrentColumn = this._DisplayColumns - 1;
                this._CurrentLine--;
                if (this._CurrentLine < 0)
                {
                  this._CurrentLine = this._DisplayLines - 1;
                }
              }
              this.SendDataToDisplay(this.Commands.CMD_CursorSet);
            }
          }
          else
          {
            if (this.DoDebug)
            {
              Log.Info("GenericSerial.GSDisplay.CursorLeft(): Using CMD_CursorLeft to move cursor left", new object[0]);
            }
            for (int i = 0; i < this.Commands.CMD_CursorLeft.Length; i++)
            {
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
              this.commPort.Write(new byte[] { this.Commands.CMD_CursorLeft[i] }, 0, 1);
              Thread.Sleep(this.CommDelay);
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
            }
            this._CurrentColumn--;
            if (this._CurrentColumn < 0)
            {
              this._CurrentColumn = this._DisplayColumns - 1;
              this._CurrentLine--;
              if (this._CurrentLine < 0)
              {
                this._CurrentLine = this._DisplayLines - 1;
              }
            }
          }
        }
      }

      public void CursorPosition(int column, int row)
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.GSDisplay.CursorPositon(column = {0}, row = (1}): Called", new object[] { column, row });
          }
          if ((row != this._CurrentLine) || (column != this._CurrentColumn))
          {
            if ((column == 0) & (row == 0))
            {
              if (this.Commands.CMD_CursorHome != null)
              {
                this.CursorHome();
              }
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorPosition(): Completed", new object[0]);
              }
            }
            else if (this.Commands.CMD_CursorSet != null)
            {
              this._CurrentLine = row;
              this._CurrentColumn = column;
              this.SendDataToDisplay(this.Commands.CMD_CursorSet);
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorPosition(): Completed", new object[0]);
              }
            }
            else
            {
              if (this.Commands.CMD_CursorHome != null)
              {
                this.CursorHome();
              }
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorPosition(): Moving cursor from {0},{1} to {2},{3}", new object[] { this._CurrentColumn, this._CurrentLine, column, row });
              }
              int num = column - this._CurrentColumn;
              if (num != 0)
              {
                if ((num < 0) & (this.Commands.CMD_CursorLeft != null))
                {
                  while (this._CurrentColumn != column)
                  {
                    this.CursorLeft();
                  }
                }
                else if (!((num < 0) & (this.Commands.CMD_CursorRight != null)))
                {
                  while (this._CurrentColumn != column)
                  {
                    this.SendCharToDisplay(' ');
                  }
                }
                else
                {
                  while (this._CurrentColumn != column)
                  {
                    this.CursorRight();
                  }
                }
              }
              int num2 = row - this._CurrentLine;
              if (num2 != 0)
              {
                if ((num2 < 0) & (this.Commands.CMD_CursorUp != null))
                {
                  while (this._CurrentLine != row)
                  {
                    this.CursorUp();
                  }
                }
                else if (!((num2 > 0) & (this.Commands.CMD_CursorDown != null)))
                {
                  while ((this._CurrentLine != row) & (this._CurrentColumn != column))
                  {
                    this.SendCharToDisplay(' ');
                  }
                }
                else
                {
                  while (this._CurrentLine != row)
                  {
                    this.CursorDown();
                  }
                }
              }
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorPosition(): Completed", new object[0]);
              }
            }
          }
        }
      }

      public void CursorRight()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.GSDisplay.CursorRight(): Called", new object[0]);
          }
          if (this.Commands.CMD_CursorRight == null)
          {
            if (this.Commands.CMD_CursorSet == null)
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorRight(): unable to move cursor right", new object[0]);
              }
            }
            else
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorRight(): Using CMD_CursorSet to move cursor right", new object[0]);
              }
              this._CurrentColumn++;
              if (this._CurrentColumn > (this._DisplayColumns - 1))
              {
                this._CurrentColumn = 0;
                this._CurrentLine++;
                if (this._CurrentLine > (this._DisplayLines - 1))
                {
                  this._CurrentLine = 0;
                }
              }
              this.SendDataToDisplay(this.Commands.CMD_CursorSet);
            }
          }
          else
          {
            if (this.DoDebug)
            {
              Log.Info("GenericSerial.GSDisplay.CursorRight(): Using CMD_CursorRight to move cursor right", new object[0]);
            }
            for (int i = 0; i < this.Commands.CMD_CursorRight.Length; i++)
            {
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
              this.commPort.Write(new byte[] { this.Commands.CMD_CursorRight[i] }, 0, 1);
              Thread.Sleep(this.CommDelay);
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
            }
            this._CurrentColumn++;
            if (this._CurrentColumn > (this._DisplayColumns - 1))
            {
              this._CurrentColumn = 0;
              this._CurrentLine++;
              if (this._CurrentLine > (this._CurrentLine - 1))
              {
                this._CurrentLine = 0;
              }
            }
          }
        }
      }

      public void CursorUp()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          if (this.DoDebug)
          {
            Log.Info("GenericSerial.GSDisplay.CursorUp(): Called", new object[0]);
          }
          if (this.Commands.CMD_CursorUp == null)
          {
            if (this.Commands.CMD_CursorSet == null)
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorUp(): can't move cursor up", new object[0]);
              }
            }
            else
            {
              if (this.DoDebug)
              {
                Log.Info("GenericSerial.GSDisplay.CursorUp(): Using CMD_CursorSet to move cursor up", new object[0]);
              }
              this._CurrentLine--;
              if (this._CurrentLine < 0)
              {
                this._CurrentLine = this._DisplayLines - 1;
              }
              this.SendDataToDisplay(this.Commands.CMD_CursorSet);
            }
          }
          else
          {
            if (this.DoDebug)
            {
              Log.Info("GenericSerial.GSDisplay.CursorUp(): Using CMD_CursorUp to move cursor up", new object[0]);
            }
            for (int i = 0; i < this.Commands.CMD_CursorUp.Length; i++)
            {
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
              this.commPort.Write(new byte[] { this.Commands.CMD_CursorUp[i] }, 0, 1);
              Thread.Sleep(this.CommDelay);
              if (this.Commands.CMD_ToggleRTS)
              {
                this.commPort.RtsEnable = !this.commPort.RtsEnable;
              }
              if (this.Commands.CMD_ToggleDTR)
              {
                this.commPort.DtrEnable = !this.commPort.DtrEnable;
              }
            }
            this._CurrentLine--;
            if (this._CurrentLine < 0)
            {
              this._CurrentLine = this._DisplayLines - 1;
            }
          }
        }
      }

      private void DebugShowCommand(string cmdName, byte[] cmdValue)
      {
        if (this.DoDebug)
        {
          string str = string.Empty;
          int length = 0;
          if (cmdValue != null)
          {
            length = cmdValue.Length;
            if (length > 0)
            {
              for (int i = 0; i < length; i++)
              {
                str = str + " " + cmdValue[i].ToString("x00");
              }
              str = str.Trim();
            }
          }
          Log.Info("GenericSerial.GSDisplay.ShowCommand(): Command = {0}, Size = {1}, data = {2}", new object[] { cmdName, length.ToString(), str });
        }
      }

      public void DisplayOff()
      {
        this._IsDisplayOff = true;
      }

      public void DisplayOn()
      {
        this._IsDisplayOff = false;
      }

      private bool InitCommandSet(GenericSerial.CommandStrings CommandStrings)
      {
        this.Commands.CMD_DisplayInit = null;
        this.Commands.CMD_ClearDisplay = null;
        this.Commands.CMD_CursorLeft = null;
        this.Commands.CMD_CursorRight = null;
        this.Commands.CMD_CursorUp = null;
        this.Commands.CMD_CursorDown = null;
        this.Commands.CMD_CursorHome = null;
        this.Commands.CMD_CursorSet = null;
        this.Commands.CMD_DisplayClose = null;
        this.Commands.PositionBase = CommandStrings.PositionBase;
        this.Commands.CMD_ToggleRTS = CommandStrings.CMD_ToggleRTS;
        this.Commands.CMD_ToggleDTR = CommandStrings.CMD_ToggleDTR;
        this.Commands.AssertRTS = CommandStrings.AssertRTS;
        this.Commands.AssertDTR = CommandStrings.AssertDTR;
        this.Commands.CMD_DisplayInit = this.CommandStringToBytes(CommandStrings.CMD_DisplayInit);
        this.DebugShowCommand("Commands.CMD_DisplayInit", this.Commands.CMD_DisplayInit);
        this.Commands.CMD_ClearDisplay = this.CommandStringToBytes(CommandStrings.CMD_ClearDisplay);
        this.DebugShowCommand("Commands.CMD_ClearDisplay", this.Commands.CMD_ClearDisplay);
        this.Commands.CMD_CursorLeft = this.CommandStringToBytes(CommandStrings.CMD_CursorLeft);
        this.DebugShowCommand("Commands.CMD_CursorLeft", this.Commands.CMD_CursorLeft);
        this.Commands.CMD_CursorRight = this.CommandStringToBytes(CommandStrings.CMD_CursorRight);
        this.DebugShowCommand("Commands.CMD_CursorRight", this.Commands.CMD_CursorRight);
        this.Commands.CMD_CursorUp = this.CommandStringToBytes(CommandStrings.CMD_CursorUp);
        this.DebugShowCommand("Commands.CMD_CursorUp", this.Commands.CMD_CursorUp);
        this.Commands.CMD_CursorDown = this.CommandStringToBytes(CommandStrings.CMD_CursorDown);
        this.DebugShowCommand("Commands.CMD_CursorDown", this.Commands.CMD_CursorDown);
        this.Commands.CMD_CursorHome = this.CommandStringToBytes(CommandStrings.CMD_CursorHome);
        this.DebugShowCommand("Commands.CMD_CursorHome", this.Commands.CMD_CursorHome);
        this.Commands.CMD_CursorHome = this.CommandStringToBytes(CommandStrings.CMD_CursorSet);
        this.DebugShowCommand("Commands.CMD_CursorSet", this.Commands.CMD_CursorSet);
        this.Commands.CMD_DisplayClose = this.CommandStringToBytes(CommandStrings.CMD_DisplayClose);
        this.DebugShowCommand("Commands.CMD_DisplayClose", this.Commands.CMD_DisplayClose);
        return (this.Commands.CMD_DisplayInit != null);
      }

      public void InitDisplay()
      {
        if (this.commPort.IsOpen && (this.Commands.CMD_DisplayInit != null))
        {
          if (this.Commands.CMD_ToggleRTS)
          {
            this.commPort.RtsEnable = !this.commPort.RtsEnable;
          }
          if (this.Commands.CMD_ToggleDTR)
          {
            this.commPort.DtrEnable = !this.commPort.DtrEnable;
          }
          for (int i = 0; i < this.Commands.CMD_DisplayInit.Length; i++)
          {
            this.commPort.Write(new byte[] { this.Commands.CMD_DisplayInit[i] }, 0, 1);
            Thread.Sleep(this.CommDelay);
          }
          if (this.Commands.CMD_ToggleRTS)
          {
            this.commPort.RtsEnable = !this.commPort.RtsEnable;
          }
          if (this.Commands.CMD_ToggleDTR)
          {
            this.commPort.DtrEnable = !this.commPort.DtrEnable;
          }
        }
      }

      public bool OpenDisplay(string _port, int dRows, int dCols, GenericSerial.CommandStrings DefinedCommandStrings)
      {
        this.DoDebug = Settings.Instance.ExtensiveLogging;
        Log.Info("GenericSerial.GSDisplay.OpenDisplay(): Called", new object[0]);
        if (!this.InitCommandSet(DefinedCommandStrings))
        {
          Log.Info("GenericSerial.GSDisplay.OpenDisplay(): Command Set not properly defined!!", new object[0]);
          this._isOpen = false;
          return false;
        }
        Log.Info("GenericSerial.GSDisplay.OpenDisplay(): Command Set loaded", new object[0]);
        try
        {
          int num;
          int num2;
          string str2;
          this._DisplayLines = dRows;
          this._DisplayColumns = dCols;
          this._CurrentLine = 0;
          this._CurrentColumn = 0;
          this.CommDelay = DefinedCommandStrings.CommDelay;
          if (DefinedCommandStrings.BaudRate == string.Empty)
          {
            num = 0x2580;
          }
          else
          {
            num = int.Parse(DefinedCommandStrings.BaudRate);
          }
          if (DefinedCommandStrings.DataBits == string.Empty)
          {
            num2 = 8;
          }
          else
          {
            num2 = int.Parse(DefinedCommandStrings.DataBits);
          }
          string str = DefinedCommandStrings.Parity;
          if (str == null)
          {
            goto Label_0130;
          }
          if (!(str == "Even"))
          {
            if (str == "Mark")
            {
              goto Label_0120;
            }
            if (str == "None")
            {
              goto Label_0124;
            }
            if (str == "Odd")
            {
              goto Label_0128;
            }
            if (str == "Space")
            {
              goto Label_012C;
            }
            goto Label_0130;
          }
          Parity even = Parity.Even;
          goto Label_0132;
        Label_0120:
          even = Parity.Mark;
          goto Label_0132;
        Label_0124:
          even = Parity.None;
          goto Label_0132;
        Label_0128:
          even = Parity.Odd;
          goto Label_0132;
        Label_012C:
          even = Parity.Space;
          goto Label_0132;
        Label_0130:
          even = Parity.None;
        Label_0132:
          if ((str2 = DefinedCommandStrings.StopBits) == null)
          {
            goto Label_0188;
          }
          if (!(str2 == "None"))
          {
            if (str2 == "One")
            {
              goto Label_017C;
            }
            if (str2 == "OnePointFive")
            {
              goto Label_0180;
            }
            if (str2 == "Two")
            {
              goto Label_0184;
            }
            goto Label_0188;
          }
          StopBits none = StopBits.None;
          goto Label_018A;
        Label_017C:
          none = StopBits.One;
          goto Label_018A;
        Label_0180:
          none = StopBits.OnePointFive;
          goto Label_018A;
        Label_0184:
          none = StopBits.Two;
          goto Label_018A;
        Label_0188:
          none = StopBits.None;
        Label_018A:
          ;
          Log.Info("GenericSerial.GSDisplay.OpenDisplay(): Opening display - Port = {0}, Baud = {1}, DataBits = {2}, Parity = {3}, StopBits = {4}, CommDelay = {5}", new object[] { _port, num.ToString(), num2.ToString(), even.ToString(), none.ToString(), this.CommDelay.ToString() });
          this.commPort = new SerialPort(_port, num, even, num2, none);
          this.commPort.Open();
          Log.Info("GenericSerial.GSDisplay.OpenDisplay(): Comm Port {0} opened", new object[] { _port });
          if (this.Commands.AssertRTS)
          {
            this.commPort.RtsEnable = true;
          }
          if (this.Commands.AssertDTR)
          {
            this.commPort.DtrEnable = true;
          }
          this.InitDisplay();
          this.ClearDisplay();
          this._IsDisplayOff = false;
          this._isOpen = true;
        } catch (Exception exception)
        {
          Log.Info("GenericSerial.GSDisplay.OpenDisplay(): CAUGHT EXCEPTION while opening display! \n{0}", new object[] { exception });
          if (this.commPort.IsOpen)
          {
            this.commPort.Close();
          }
          this._isOpen = false;
        }
        Log.Info("GenericSerial.GSDisplay.OpenDisplay(): Completed - Returning {0}", new object[] { this._isOpen.ToString() });
        return this._isOpen;
      }

      private void SendCharToDisplay(char charToSend)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.commPort.Write(new byte[] { (byte)charToSend }, 0, 1);
          this._CurrentColumn++;
          if (this._CurrentColumn > (this._DisplayColumns - 1))
          {
            this._CurrentLine++;
            this._CurrentColumn = 0;
            if (this._CurrentLine > (this._DisplayLines - 1))
            {
              this._CurrentLine = 0;
            }
          }
          Thread.Sleep(this.CommDelay);
        }
      }

      private void SendDataToDisplay(byte[] SendThisData)
      {
        if (this.Commands.CMD_ToggleRTS)
        {
          this.commPort.RtsEnable = !this.commPort.RtsEnable;
        }
        if (this.Commands.CMD_ToggleDTR)
        {
          this.commPort.DtrEnable = !this.commPort.DtrEnable;
        }
        for (int i = 0; i < SendThisData.Length; i++)
        {
          byte[] buffer;
          int num4;
          switch (SendThisData[i])
          {
            case 0xfb:
              buffer = new byte[] { (byte)((this._CurrentLine + this.Commands.PositionBase) + 0x30) };
              goto Label_01DB;

            case 0xfc:
              this._CurrentColumn += this.Commands.PositionBase;
              if (this._CurrentColumn >= 10)
              {
                break;
              }
              buffer = new byte[] { (byte)(this._CurrentColumn + 0x30) };
              goto Label_0132;

            case 0xfd:
              buffer = new byte[] { (byte)(this._CurrentLine + this.Commands.PositionBase) };
              goto Label_01DB;

            case 0xfe:
              buffer = new byte[] { (byte)(this._CurrentColumn + this.Commands.PositionBase) };
              goto Label_01DB;

            case 0xff:
              buffer = new byte[] { (byte)(((this._CurrentLine * this._DisplayColumns) + this._CurrentColumn) + this.Commands.PositionBase) };
              goto Label_01DB;

            default:
              buffer = new byte[] { SendThisData[i] };
              goto Label_01DB;
          }
          double num2 = Math.Floor((double)(((double)this._CurrentColumn) / 10.0));
          double num3 = this._CurrentColumn - (num2 * 10.0);
          buffer = new byte[] { (byte)(num2 + 48.0), (byte)num3 };
        Label_0132:
          this._CurrentColumn -= this.Commands.PositionBase;
        Label_01DB:
          num4 = 0;
          while (num4 < buffer.Length)
          {
            this.commPort.Write(new byte[] { buffer[num4] }, 0, 1);
            Thread.Sleep(this.CommDelay);
            num4++;
          }
        }
        if (this.Commands.CMD_ToggleDTR)
        {
          this.commPort.DtrEnable = !this.commPort.DtrEnable;
        }
        if (this.Commands.CMD_ToggleRTS)
        {
          this.commPort.RtsEnable = !this.commPort.RtsEnable;
        }
      }

      public void SetLine(int _line, string _message)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.CursorPosition(0, _line);
          for (int i = 0; i < this._DisplayColumns; i++)
          {
            if (i < _message.Length)
            {
              this.SendCharToDisplay(_message[i]);
            }
            else
            {
              this.SendCharToDisplay(' ');
            }
          }
        }
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct CommandBytes
      {
        public int PositionBase;
        public bool AssertRTS;
        public bool AssertDTR;
        public byte[] CMD_DisplayInit;
        public byte[] CMD_ClearDisplay;
        public byte[] CMD_CursorLeft;
        public byte[] CMD_CursorRight;
        public byte[] CMD_CursorUp;
        public byte[] CMD_CursorDown;
        public byte[] CMD_CursorHome;
        public byte[] CMD_CursorSet;
        public byte[] CMD_DisplayClose;
        public bool CMD_ToggleRTS;
        public bool CMD_ToggleDTR;
      }
    }
  }
}

