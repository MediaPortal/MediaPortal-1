using System;
using System.Drawing;
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
using MediaPortal.InputDevices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class CFontz : BaseDisplay, IDisplay, IDisposable
  {
    private bool _BackLightControl;
    private int _BackLightLevel = 0x7f;
    private bool _BlankDisplayOnExit;
    private bool _ContrastControl;
    private int _ContrastLevel = 0x7f;
    private Thread _EqThread;
    private readonly string _ErrorMessage = "";
    private bool _IsDisabled;
    private bool _IsDisplayOff;
    private bool _IsOpen;
    private bool _mpIsIdle;
    public static bool _stopUpdateEqThread;
    private int _Tcols;
    private int _Trows;
    private bool _UseKeypad;
    private readonly CFDisplay CFD = new CFDisplay();
    private DisplayControl DisplaySettings;
    private string DisplayType = string.Empty;
    private bool DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
    private object DWriteMutex = new object();
    private EQControl EQSettings;
    private object EqWriteMutex = new object();
    private string IdleMessage = string.Empty;
    private KeyPadControl KPSettings;
    private DateTime LastSettingsCheck = DateTime.Now;
    private SystemStatus MPStatus = new SystemStatus();
    private DateTime SettingsLastModTime;
    private object ThreadMutex = new object();

    private void AdvancedSettings_OnSettingsChanged()
    {
      Log.Info("CFontz.AdvancedSettings_OnSettingsChanged(): called", new object[0]);
      this.CleanUp();
      this.LoadAdvancedSettings();
      Thread.Sleep(100);
      this.Setup(Settings.Instance.Port, Settings.Instance.TextHeight, Settings.Instance.TextWidth, Settings.Instance.TextComDelay, Settings.Instance.GraphicHeight, Settings.Instance.GraphicWidth, Settings.Instance.GraphicComDelay, Settings.Instance.BackLightControl, Settings.Instance.Backlight, Settings.Instance.ContrastControl, Settings.Instance.Contrast, Settings.Instance.BlankOnExit);
      this.Initialize();
    }

    public void CleanUp()
    {
      AdvancedSettings.OnSettingsChanged -= new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
      if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
      {
        while (this._EqThread.IsAlive)
        {
          Log.Info("CFontz.Cleanup(): Stoping EQ_Update() Thread", new object[0]);
          lock (this.ThreadMutex)
          {
            _stopUpdateEqThread = true;
          }
          _stopUpdateEqThread = true;
          Thread.Sleep(500);
        }
      }
      this.CFD.ClearDisplay();
      if (!this._BlankDisplayOnExit && ((this.DisplaySettings._Shutdown1 != string.Empty) || (this.DisplaySettings._Shutdown2 != string.Empty)))
      {
        this.CFD.SetLine(0, this.DisplaySettings._Shutdown1);
        this.CFD.SetLine(1, this.DisplaySettings._Shutdown2);
      }
      this.CFD.CloseDisplay(this._BlankDisplayOnExit);
    }

    private void Clear()
    {
      this.CFD.ClearDisplay();
    }

    public void Configure()
    {
      Form form = new CFontz_AdvancedSetupForm();
      form.ShowDialog();
      form.Dispose();
    }

    private ulong ConvertPluginIconsToDriverIcons(ulong IconMask)
    {
      return 0L;
    }

    private void DisplayEQ()
    {
      if ((this.EQSettings.UseEqDisplay & this.EQSettings._EqDataAvailable) && !(this.EQSettings.RestrictEQ & ((DateTime.Now.Ticks - this.EQSettings._LastEQupdate.Ticks) < this.EQSettings._EqUpdateDelay)))
      {
        if (this.DoDebug)
        {
          Log.Info("\nCFDisplay.DisplayEQ(): Retrieved {0} samples of Equalizer data.", new object[] { this.EQSettings.EqFftData.Length / 2 });
        }
        if (this.EQSettings.UseVUmeter || this.EQSettings.UseVUmeter2)
        {
          this.EQSettings.Render_MaxValue = this.CFD.GetColumns() * 6;
          this.EQSettings.Render_BANDS = 1;
          if (this.EQSettings._useVUindicators)
          {
            this.EQSettings.Render_MaxValue = (this.CFD.GetColumns() - 1) * 6;
          }
        }
        else
        {
          this.EQSettings.Render_MaxValue = this.CFD.GetRows() * 8;
          if (this.EQSettings.UseStereoEq)
          {
            this.EQSettings.Render_BANDS = 8;
          }
          else
          {
            this.EQSettings.Render_BANDS = 0x10;
          }
        }
        MiniDisplayHelper.ProcessEqData(ref this.EQSettings);
        this.RenderEQ(this.EQSettings.EqArray);
        this.EQSettings._LastEQupdate = DateTime.Now;
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
              Log.Info("CFDisplay.DisplayOff(): DisplayControlAction Timer = {0}.", new object[] { DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction });
            }
            return;
          }
          if (this.DoDebug)
          {
            Log.Info("CFDisplay.DisplayOff(): DisplayControlAction Timeout expired.", new object[0]);
          }
          this.DisplaySettings._DisplayControlAction = false;
          this.DisplaySettings._DisplayControlLastAction = 0L;
        }
        Log.Info("CFDisplay.DisplayOff(): completed", new object[0]);
        lock (this.DWriteMutex)
        {
          Log.Info("CFDisplay.DisplayOff(): Turning display OFF", new object[0]);
          this.Clear();
          if (this._BackLightControl)
          {
            this.CFD.BacklightOff();
          }
          this._IsDisplayOff = true;
          this.CFD.DisplayOff();
        }
        Log.Info("CFDisplay.DisplayOff(): completed", new object[0]);
      }
    }

    private void DisplayOn()
    {
      if (this._IsDisplayOff)
      {
        Log.Info("CFDisplay.DisplayOn(): called", new object[0]);
        lock (this.DWriteMutex)
        {
          Log.Info("CFDisplay.DisplayOn(): Turning Display ON", new object[0]);
          this.CFD.DisplayOn();
          this.CFD.BacklightOn();
        }
        this._IsDisplayOff = false;
        Log.Info("CFDisplay.DisplayOn(): completed", new object[0]);
      }
    }

    public void Dispose()
    {
      this.CFD.CloseDisplay(this._BackLightControl);
    }

    public void DrawImage(Bitmap bitmap)
    {
    }

    private void EQ_Update()
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
            Log.Info("CFDisplay.EQ_Update(): Checking for Thread termination request", new object[0]);
          }
          if (_stopUpdateEqThread)
          {
            if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
            {
              GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
            }
            if (this.DoDebug)
            {
              Log.Info("CFDisplay.EQ_Update(): EQ_Update Thread terminating", new object[0]);
            }
            _stopUpdateEqThread = false;
            return;
          }
          MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
          if ((!this.MPStatus.MediaPlayer_Active & this.DisplaySettings.BlankDisplayWithVideo) & (this.DisplaySettings.BlankDisplayWhenIdle & !this._mpIsIdle))
          {
            this.DisplayOn();
          }
          if (this.MPStatus.MediaPlayer_Playing)
          {
            if (this.EQSettings.UseEqDisplay)
            {
              this.GetEQ();
              this.DisplayEQ();
            }
            if (this.DisplaySettings.BlankDisplayWithVideo & (((this.MPStatus.Media_IsDVD || this.MPStatus.Media_IsVideo) || this.MPStatus.Media_IsTV) || this.MPStatus.Media_IsTVRecording))
            {
              if (this.DoDebug)
              {
                Log.Info("CFDisplay.EQ_Update(): Turning off display while playing video", new object[0]);
              }
              this.DisplayOff();
            }
          }
          else
          {
            this.RestoreDisplayFromVideoOrIdle();
            lock (this.DWriteMutex)
            {
              this.EQSettings._EqDataAvailable = false;
              this._EqThread.Priority = ThreadPriority.BelowNormal;
            }
          }
        }
        if (!this.EQSettings._EqDataAvailable || this.MPStatus.MediaPlayer_Paused)
        {
          Thread.Sleep(250);
        }
      }
    }

    private void GetEQ()
    {
      lock (this.DWriteMutex)
      {
        this.EQSettings._EqDataAvailable = MiniDisplayHelper.GetEQ(ref this.EQSettings);
        if (this.EQSettings._EqDataAvailable)
        {
          this._EqThread.Priority = ThreadPriority.AboveNormal;
        }
        else
        {
          this._EqThread.Priority = ThreadPriority.BelowNormal;
        }
      }
    }

    public void Initialize()
    {
      this.Clear();
    }

    private void InitKeyPadSettings(ref KeyPadControl KPsettings)
    {
      KPsettings.EnableKeyPad = false;
      KPsettings.EnableCustom = false;
    }

    private void LoadAdvancedSettings()
    {
      Log.Info("CFontz.LoadAdvancedSettings(): Called", new object[0]);
      AdvancedSettings settings = AdvancedSettings.Load();
      this.IdleMessage = (Settings.Instance.IdleMessage != string.Empty) ? Settings.Instance.IdleMessage : "MediaPortal";
      this.KPSettings.EnableKeyPad = settings.EnableKeypad;
      this.KPSettings.EnableCustom = settings.UseCustomKeypadMap;
      this.DisplayType = settings.DeviceType;
      this.DisplaySettings.BlankDisplayWithVideo = settings.BlankDisplayWithVideo;
      this.DisplaySettings.EnableDisplayAction = settings.EnableDisplayAction;
      this.DisplaySettings.DisplayActionTime = settings.EnableDisplayActionTime;
      this.DisplaySettings.BlankDisplayWhenIdle = settings.BlankDisplayWhenIdle;
      this.DisplaySettings.BlankIdleDelay = settings.BlankIdleTime;
      this.DisplaySettings._BlankIdleTimeout = this.DisplaySettings.BlankIdleDelay * 0x989680;
      this.DisplaySettings._DisplayControlTimeout = this.DisplaySettings.DisplayActionTime * 0x989680;
      this.DisplaySettings._Shutdown1 = Settings.Instance.Shutdown1;
      this.DisplaySettings._Shutdown2 = Settings.Instance.Shutdown2;
      this.EQSettings.UseVUmeter = settings.VUmeter;
      this.EQSettings.UseVUmeter2 = settings.VUmeter2;
      this.EQSettings._useVUindicators = settings.VUindicators;
      this.EQSettings.UseEqDisplay = settings.EqDisplay;
      this.EQSettings.UseStereoEq = settings.StereoEQ;
      this.EQSettings.DelayEQ = settings.DelayEQ;
      this.EQSettings._DelayEQTime = settings.DelayEqTime;
      this.EQSettings.SmoothEQ = settings.SmoothEQ;
      this.EQSettings.RestrictEQ = settings.RestrictEQ;
      this.EQSettings._EQ_Restrict_FPS = settings.EqRate;
      this.EQSettings.EQTitleDisplay = settings.EQTitleDisplay;
      this.EQSettings._EQTitleDisplayTime = settings.EQTitleDisplayTime;
      this.EQSettings._EQTitleShowTime = settings.EQTitleShowTime;
      this.EQSettings._EqUpdateDelay = (this.EQSettings._EQ_Restrict_FPS == 0) ? 0 : ((0x989680 / this.EQSettings._EQ_Restrict_FPS) - (0xf4240 / this.EQSettings._EQ_Restrict_FPS));
      Log.Info("CFontz.LoadAdvancedSettings(): Extensive Logging: {0}", new object[] { Settings.Instance.ExtensiveLogging });
      Log.Info("CFontz.LoadAdvancedSettings(): Device Port: {0}", new object[] { Settings.Instance.Port });
      Log.Info("CFontz.LoadAdvancedSettings(): Display Type: {0}", new object[] { this.DisplayType });
      Log.Info("CFontz.LoadAdvancedSettings(): Enable Keypad: {0}", new object[] { this.KPSettings.EnableKeyPad });
      Log.Info("CFontz.LoadAdvancedSettings():   Use Custom KeypadMap: {0}", new object[] { this.KPSettings.EnableCustom });
      Log.Info("CFontz.LoadAdvancedSettings(): Shutdown Message - Line 1: {0}", new object[] { this.DisplaySettings._Shutdown1 });
      Log.Info("CFontz.LoadAdvancedSettings(): Shutdown Message - Line 2: {0}", new object[] { this.DisplaySettings._Shutdown2 });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options - Equalizer Display: {0}", new object[] { this.EQSettings.UseEqDisplay });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -   Stereo Equalizer Display: {0}", new object[] { this.EQSettings.UseStereoEq });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -   VU Meter Display: {0}", new object[] { this.EQSettings.UseVUmeter });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -   VU Meter Style 2 Display: {0}", new object[] { this.EQSettings.UseVUmeter2 });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -     Use VU Channel indicators: {0}", new object[] { this.EQSettings._useVUindicators });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -   Restrict EQ Update Rate: {0}", new object[] { this.EQSettings.RestrictEQ });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -     Restricted EQ Update Rate: {0} updates per second", new object[] { this.EQSettings._EQ_Restrict_FPS });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -   Delay EQ Startup: {0}", new object[] { this.EQSettings.DelayEQ });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -     Delay EQ Startup Time: {0} seconds", new object[] { this.EQSettings._DelayEQTime });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -   Smooth EQ Amplitude Decay: {0}", new object[] { this.EQSettings.SmoothEQ });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -   Show Track Info with EQ display: {0}", new object[] { this.EQSettings.EQTitleDisplay });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -     Show Track Info Interval: {0} seconds", new object[] { this.EQSettings._EQTitleDisplayTime });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -     Show Track Info duration: {0} seconds", new object[] { this.EQSettings._EQTitleShowTime });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options - Blank display with video: {0}", new object[] { this.DisplaySettings.BlankDisplayWithVideo });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -   Enable Display on Action: {0}", new object[] { this.DisplaySettings.EnableDisplayAction });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -     Enable display for: {0} seconds", new object[] { this.DisplaySettings._DisplayControlTimeout });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options - Blank display when idle: {0}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle });
      Log.Info("CFontz.LoadAdvancedSettings(): Advanced options -     blank display after: {0} seconds", new object[] { this.DisplaySettings._BlankIdleTimeout / 0xf4240L });
      Log.Info("CFontz.LoadAdvancedSettings(): Setting - Audio using ASIO: {0}", new object[] { this.EQSettings._AudioUseASIO });
      Log.Info("CFontz.LoadAdvancedSettings(): Completed", new object[0]);
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_CFontz.xml"));
      this.SettingsLastModTime = info.LastWriteTime;
      this.LastSettingsCheck = DateTime.Now;
    }

    private void OnExternalAction(Action action)
    {
      if (this.DisplaySettings.EnableDisplayAction)
      {
        if (this.DoDebug)
        {
          Log.Info("CFDisplay.OnExternalAction(): received action {0}", new object[] { action.wID.ToString() });
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
          Log.Info("CFDisplay.OnExternalAction(): received DisplayControlAction", new object[0]);
        }
        this.DisplayOn();
      }
    }

    private void RenderEQ(byte[] EqDataArray)
    {
      if (this.EQSettings.UseVUmeter || this.EQSettings.UseVUmeter2)
      {
        this.CFD.InitHorizontalBarGraph();
        Thread.Sleep(40);
        if (this.EQSettings._useVUindicators)
        {
          this.CFD.SetLine(0, "L");
          this.CFD.DrawHorizontalBarGraph(1, 0, 0, EqDataArray[1]);
          if (this.EQSettings.UseVUmeter)
          {
            this.CFD.SetLine(1, "R");
            this.CFD.DrawHorizontalBarGraph(1, 1, 0, EqDataArray[2]);
          }
          else
          {
            this.CFD.SetLine(1, "                   R");
            this.CFD.DrawHorizontalBarGraph(0, 1, 1, EqDataArray[2]);
          }
        }
        else
        {
          this.CFD.DrawHorizontalBarGraph(0, 0, 0, EqDataArray[1]);
          if (this.EQSettings.UseVUmeter)
          {
            this.CFD.DrawHorizontalBarGraph(0, 1, 0, EqDataArray[2]);
          }
          else
          {
            this.CFD.DrawHorizontalBarGraph(0x13, 1, 1, EqDataArray[2]);
          }
        }
      }
      else
      {
        this.CFD.InitVerticalBarGraph();
        this.CFD.DrawVerticalBarGraph(ref this.EQSettings, ref EqDataArray);
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
          Log.Info("CFontz.SetLine(): Unable to display text - driver disabled", new object[0]);
        }
      }
      else
      {
        this.UpdateAdvancedSettings();
        if (this.DoDebug)
        {
          Log.Info("CFontz.SetLine() Called", new object[0]);
        }
        if (this.EQSettings._EqDataAvailable || this._IsDisplayOff)
        {
          if (this.DoDebug)
          {
            Log.Info("CFontz.SetLine(): Suppressing display update!", new object[0]);
          }
        }
        else
        {
          if (this.DoDebug)
          {
            Log.Info("CFontz.SetLine(): Line {0} - Message = \"{1}\"", new object[] { line, message });
          }
          this.CFD.SetLine(line, message);
          if (this.DoDebug)
          {
            Log.Info("CFontz.SetLine(): message sent to display", new object[0]);
          }
        }
        MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
        if ((line == 0) && this.MPStatus.MP_Is_Idle)
        {
          if (this.DoDebug)
          {
            Log.Info("CFontz.SetLine(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle, this.DisplaySettings._BlankIdleTimeout });
          }
          if (this.DisplaySettings.BlankDisplayWhenIdle)
          {
            if (!this._mpIsIdle)
            {
              if (this.DoDebug)
              {
                Log.Info("CFontz.SetLine(): MP going IDLE", new object[0]);
              }
              this.DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
            }
            if (!this._IsDisplayOff && ((DateTime.Now.Ticks - this.DisplaySettings._BlankIdleTime) > this.DisplaySettings._BlankIdleTimeout))
            {
              if (this.DoDebug)
              {
                Log.Info("CFontz.SetLine(): Blanking display due to IDLE state", new object[0]);
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
              Log.Info("CFontz.SetLine(): MP no longer IDLE - restoring display", new object[0]);
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
      Log.Info("CFontz.Setup(): called", new object[0]);
      MiniDisplayHelper.InitEQ(ref this.EQSettings);
      MiniDisplayHelper.InitDisplayControl(ref this.DisplaySettings);
      this.InitKeyPadSettings(ref this.KPSettings);
      this._BlankDisplayOnExit = _blankOnExit;
      this._BackLightControl = _backLight;
      this._BackLightLevel = _backLightLevel;
      this._ContrastControl = _contrast;
      this._ContrastLevel = _contrastLevel;
      this.LoadAdvancedSettings();
      this._UseKeypad = false;
      if (this.KPSettings.EnableKeyPad)
      {
        this._UseKeypad = this.CFD.SetupKeypad(this.KPSettings.EnableCustom);
      }
      else
      {
        this._UseKeypad = false;
        Log.Info("CFontz.Setup(): Keypad support disabled!", new object[0]);
      }
      this._IsOpen = false;
      try
      {
        this._IsOpen = this.CFD.OpenDisplay(_port, this.DisplayType, this._BackLightControl, this._BackLightLevel, this._ContrastControl, this._ContrastLevel, this._UseKeypad);
      } catch (Exception exception)
      {
        Log.Info("CFontz.Setup() - CAUGHT EXCEPTION opening display port!: {0}", new object[] { exception });
      }
      if (this._IsOpen)
      {
        this._IsDisabled = false;
        this._Trows = _lines;
        if (this._Trows > this.CFD.GetRows())
        {
          Log.Info("CFontz.Setup() - Invalid Text Lines value", new object[0]);
          this._Trows = this.CFD.GetRows();
        }
        this._Tcols = _cols;
        if (this._Tcols > this.CFD.GetColumns())
        {
          Log.Info("CFontz.Setup() - Invalid Text Columns value", new object[0]);
          this._Tcols = this.CFD.GetColumns();
        }
      }
      else
      {
        this._IsDisabled = true;
      }
      if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
      {
        Log.Info("CFontz.Setup(): starting EQ_Update() thread", new object[0]);
        this._EqThread = new Thread(new ThreadStart(this.EQ_Update));
        this._EqThread.IsBackground = true;
        this._EqThread.Priority = ThreadPriority.BelowNormal;
        this._EqThread.Name = "EQ_Update";
        this._EqThread.Start();
        if (this._EqThread.IsAlive)
        {
          Log.Info("CFontz.Setup(): EQ_Update() Thread Started", new object[0]);
        }
        else
        {
          Log.Info("CFontz.Setup(): EQ_Update() FAILED TO START", new object[0]);
        }
      }
      AdvancedSettings.OnSettingsChanged += new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
      Log.Info("CFontz.Setup(): completed", new object[0]);
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= this.LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (this.DoDebug)
        {
          Log.Info("CFontz.UpdateAdvancedSettings(): called", new object[0]);
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_CFontz.xml")))
        {
          FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_CFontz.xml"));
          if (info.LastWriteTime.Ticks > this.SettingsLastModTime.Ticks)
          {
            if (this.DoDebug)
            {
              Log.Info("CFontz.UpdateAdvancedSettings(): updating advanced settings", new object[0]);
            }
            this.LoadAdvancedSettings();
          }
        }
        if (this.DoDebug)
        {
          Log.Info("CFontz.UpdateAdvancedSettings(): completed", new object[0]);
        }
      }
    }

    public string Description
    {
      get
      {
        return "CrystalFontz Character LCD driver v04_17_2008";
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
        return "CFontz";
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
      private bool m_DelayEQ;
      private int m_DelayEqTime = 10;
      private string m_DeviceType;
      private bool m_DisableRepeat;
      private bool m_EnableDisplayAction;
      private int m_EnableDisplayActionTime = 5;
      private bool m_EnableKeypad;
      private bool m_EqDisplay;
      private int m_EqRate = 10;
      private bool m_EQTitleDisplay;
      private int m_EQTitleDisplayTime = 10;
      private int m_EQTitleShowTime = 2;
      private static CFontz.AdvancedSettings m_Instance;
      private bool m_NormalEQ;
      private int m_RepeatDelay;
      private bool m_RestrictEQ;
      private bool m_SmoothEQ;
      private bool m_StereoEQ;
      private bool m_UseCustomKeypadMap;
      private bool m_VUindicators;
      private bool m_VUmeter;
      private bool m_VUmeter2;

      public static event OnSettingsChangedHandler OnSettingsChanged;

      public static bool CreateDefaultKeyPadMapping()
      {
        Log.Info("CFontz.AdvancedSettings.CreateDefaultKeyPadMapping(): called", new object[0]);
        bool flag = false;
        string str = "CFontz_Keypad";
        try
        {
          Log.Info("CFontz.AdvancedSettings.CreateDefaultKeyPadMapping(): remote mapping file does not exist - Creating default mapping file", new object[0]);
          XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.CustomInputDefault, str + ".xml"), Encoding.UTF8);
          writer.Formatting = Formatting.Indented;
          writer.Indentation = 1;
          writer.IndentChar = '\t';
          writer.WriteStartDocument(true);
          writer.WriteStartElement("mappings");
          writer.WriteAttributeString("version", "3");
          writer.WriteStartElement("remote");
          writer.WriteAttributeString("family", str);
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "UP");
          writer.WriteAttributeString("code", "75");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "3");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "DOWN");
          writer.WriteAttributeString("code", "76");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "4");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "RIGHT");
          writer.WriteAttributeString("code", "70");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "2");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "LEFT");
          writer.WriteAttributeString("code", "82");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "1");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "OK");
          writer.WriteAttributeString("code", "74");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "11");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "47");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "10");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "7");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "F2");
          writer.WriteAttributeString("code", "81");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "PLAYER");
          writer.WriteAttributeString("conproperty", "DVD");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "92");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "6");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "29");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2005");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "92");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "15");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "F1");
          writer.WriteAttributeString("code", "80");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "13");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndDocument();
          writer.Close();
          flag = true;
          Log.Info("CFontz.AdvancedSettings.CreateDefaultKeyPadMapping(): remote mapping file created", new object[0]);
        } catch
        {
          Log.Info("CFontz.AdvancedSettings.CreateDefaultKeyPadMapping(): Error saving remote mapping to XML file", new object[0]);
          flag = false;
        }
        Log.Info("CFontz.AdvancedSettings.CreateDefaultKeyPadMapping(): completed", new object[0]);
        return flag;
      }

      private static void Default(CFontz.AdvancedSettings _settings)
      {
        Log.Info("CFontz.AdvancedSettings.Default(): called", new object[0]);
        _settings.EnableKeypad = false;
        _settings.UseCustomKeypadMap = false;
        _settings.DisableRepeat = false;
        _settings.RepeatDelay = 0;
        _settings.EqDisplay = false;
        _settings.NormalEQ = true;
        _settings.StereoEQ = false;
        _settings.VUmeter = false;
        _settings.VUindicators = false;
        _settings.RestrictEQ = false;
        _settings.EqRate = 10;
        _settings.DelayEQ = false;
        _settings.DelayEqTime = 10;
        _settings.SmoothEQ = false;
        _settings.BlankDisplayWithVideo = false;
        _settings.EnableDisplayAction = false;
        _settings.EnableDisplayActionTime = 5;
        _settings.EQTitleDisplay = false;
        _settings.EQTitleDisplayTime = 10;
        _settings.EQTitleShowTime = 2;
        _settings.BlankDisplayWhenIdle = false;
        _settings.BlankIdleTime = 30;
        Log.Info("CFontz.AdvancedSettings.Default(): completed", new object[0]);
      }

      public static CFontz.AdvancedSettings Load()
      {
        CFontz.AdvancedSettings settings;
        Log.Info("CFontz.AdvancedSettings.Load(): started", new object[0]);
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_CFontz.xml")))
        {
          Log.Info("CFontz.AdvancedSettings.Load(): Loading settings from XML file", new object[0]);
          XmlSerializer serializer = new XmlSerializer(typeof(CFontz.AdvancedSettings));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_CFontz.xml"));
          settings = (CFontz.AdvancedSettings)serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Info("CFontz.AdvancedSettings.Load(): Loading settings from defaults", new object[0]);
          settings = new CFontz.AdvancedSettings();
          Default(settings);
        }
        Log.Info("CFontz.AdvancedSettings.Load(): completed", new object[0]);
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

      public static void Save(CFontz.AdvancedSettings ToSave)
      {
        Log.Info("CFontz.AdvancedSettings.Save(): Saving settings to XML file", new object[0]);
        XmlSerializer serializer = new XmlSerializer(typeof(CFontz.AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_CFontz.xml"), Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter)writer, ToSave);
        writer.Close();
        Log.Info("CFontz.AdvancedSettings.Save(): completed", new object[0]);
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
      public bool DelayEQ
      {
        get
        {
          return this.m_DelayEQ;
        }
        set
        {
          this.m_DelayEQ = value;
        }
      }

      [XmlAttribute]
      public int DelayEqTime
      {
        get
        {
          return this.m_DelayEqTime;
        }
        set
        {
          this.m_DelayEqTime = value;
        }
      }

      [XmlAttribute]
      public string DeviceType
      {
        get
        {
          return this.m_DeviceType;
        }
        set
        {
          this.m_DeviceType = value;
        }
      }

      [XmlAttribute]
      public bool DisableRepeat
      {
        get
        {
          return this.m_DisableRepeat;
        }
        set
        {
          this.m_DisableRepeat = value;
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

      [XmlAttribute]
      public bool EnableKeypad
      {
        get
        {
          return this.m_EnableKeypad;
        }
        set
        {
          this.m_EnableKeypad = value;
        }
      }

      [XmlAttribute]
      public bool EqDisplay
      {
        get
        {
          return this.m_EqDisplay;
        }
        set
        {
          this.m_EqDisplay = value;
        }
      }

      [XmlAttribute]
      public int EqRate
      {
        get
        {
          return this.m_EqRate;
        }
        set
        {
          this.m_EqRate = value;
        }
      }

      [XmlAttribute]
      public bool EQTitleDisplay
      {
        get
        {
          return this.m_EQTitleDisplay;
        }
        set
        {
          this.m_EQTitleDisplay = value;
        }
      }

      [XmlAttribute]
      public int EQTitleDisplayTime
      {
        get
        {
          return this.m_EQTitleDisplayTime;
        }
        set
        {
          this.m_EQTitleDisplayTime = value;
        }
      }

      [XmlAttribute]
      public int EQTitleShowTime
      {
        get
        {
          return this.m_EQTitleShowTime;
        }
        set
        {
          this.m_EQTitleShowTime = value;
        }
      }

      public static CFontz.AdvancedSettings Instance
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
      public bool NormalEQ
      {
        get
        {
          return this.m_NormalEQ;
        }
        set
        {
          this.m_NormalEQ = value;
        }
      }

      [XmlAttribute]
      public int RepeatDelay
      {
        get
        {
          return this.m_RepeatDelay;
        }
        set
        {
          this.m_RepeatDelay = value;
        }
      }

      [XmlAttribute]
      public bool RestrictEQ
      {
        get
        {
          return this.m_RestrictEQ;
        }
        set
        {
          this.m_RestrictEQ = value;
        }
      }

      [XmlAttribute]
      public bool SmoothEQ
      {
        get
        {
          return this.m_SmoothEQ;
        }
        set
        {
          this.m_SmoothEQ = value;
        }
      }

      [XmlAttribute]
      public bool StereoEQ
      {
        get
        {
          return this.m_StereoEQ;
        }
        set
        {
          this.m_StereoEQ = value;
        }
      }

      [XmlAttribute]
      public bool UseCustomKeypadMap
      {
        get
        {
          return this.m_UseCustomKeypadMap;
        }
        set
        {
          this.m_UseCustomKeypadMap = value;
        }
      }

      [XmlAttribute]
      public bool VUindicators
      {
        get
        {
          return this.m_VUindicators;
        }
        set
        {
          this.m_VUindicators = value;
        }
      }

      [XmlAttribute]
      public bool VUmeter
      {
        get
        {
          return this.m_VUmeter;
        }
        set
        {
          this.m_VUmeter = value;
        }
      }

      [XmlAttribute]
      public bool VUmeter2
      {
        get
        {
          return this.m_VUmeter2;
        }
        set
        {
          this.m_VUmeter2 = value;
        }
      }

      public delegate void OnSettingsChangedHandler();
    }

    private class CFDisplay
    {
      private CommandSet _CommandSet;
      private int _currentBrightness = 100;
      private int _currentContrast;
      private int _DisplayType = -1;
      private InputHandler _inputHandler;
      private bool _IsDisplayOff;
      private bool _isOpen;
      private int _LastCharset;
      private bool _UseBrightness;
      private bool _UseContrast;
      private bool _useCustomKeypadMapping;
      private SerialPort commPort;
      public static readonly uint[] crcLookupTable = new uint[] { 
                0, 0x1189, 0x2312, 0x329b, 0x4624, 0x57ad, 0x6536, 0x74bf, 0x8c48, 0x9dc1, 0xaf5a, 0xbed3, 0xca6c, 0xdbe5, 0xe97e, 0xf8f7, 
                0x1081, 0x108, 0x3393, 0x221a, 0x56a5, 0x472c, 0x75b7, 0x643e, 0x9cc9, 0x8d40, 0xbfdb, 0xae52, 0xdaed, 0xcb64, 0xf9ff, 0xe876, 
                0x2102, 0x308b, 0x210, 0x1399, 0x6726, 0x76af, 0x4434, 0x55bd, 0xad4a, 0xbcc3, 0x8e58, 0x9fd1, 0xeb6e, 0xfae7, 0xc87c, 0xd9f5, 
                0x3183, 0x200a, 0x1291, 0x318, 0x77a7, 0x662e, 0x54b5, 0x453c, 0xbdcb, 0xac42, 0x9ed9, 0x8f50, 0xfbef, 0xea66, 0xd8fd, 0xc974, 
                0x4204, 0x538d, 0x6116, 0x709f, 0x420, 0x15a9, 0x2732, 0x36bb, 0xce4c, 0xdfc5, 0xed5e, 0xfcd7, 0x8868, 0x99e1, 0xab7a, 0xbaf3, 
                0x5285, 0x430c, 0x7197, 0x601e, 0x14a1, 0x528, 0x37b3, 0x263a, 0xdecd, 0xcf44, 0xfddf, 0xec56, 0x98e9, 0x8960, 0xbbfb, 0xaa72, 
                0x6306, 0x728f, 0x4014, 0x519d, 0x2522, 0x34ab, 0x630, 0x17b9, 0xef4e, 0xfec7, 0xcc5c, 0xddd5, 0xa96a, 0xb8e3, 0x8a78, 0x9bf1, 
                0x7387, 0x620e, 0x5095, 0x411c, 0x35a3, 0x242a, 0x16b1, 0x738, 0xffcf, 0xee46, 0xdcdd, 0xcd54, 0xb9eb, 0xa862, 0x9af9, 0x8b70, 
                0x8408, 0x9581, 0xa71a, 0xb693, 0xc22c, 0xd3a5, 0xe13e, 0xf0b7, 0x840, 0x19c9, 0x2b52, 0x3adb, 0x4e64, 0x5fed, 0x6d76, 0x7cff, 
                0x9489, 0x8500, 0xb79b, 0xa612, 0xd2ad, 0xc324, 0xf1bf, 0xe036, 0x18c1, 0x948, 0x3bd3, 0x2a5a, 0x5ee5, 0x4f6c, 0x7df7, 0x6c7e, 
                0xa50a, 0xb483, 0x8618, 0x9791, 0xe32e, 0xf2a7, 0xc03c, 0xd1b5, 0x2942, 0x38cb, 0xa50, 0x1bd9, 0x6f66, 0x7eef, 0x4c74, 0x5dfd, 
                0xb58b, 0xa402, 0x9699, 0x8710, 0xf3af, 0xe226, 0xd0bd, 0xc134, 0x39c3, 0x284a, 0x1ad1, 0xb58, 0x7fe7, 0x6e6e, 0x5cf5, 0x4d7c, 
                0xc60c, 0xd785, 0xe51e, 0xf497, 0x8028, 0x91a1, 0xa33a, 0xb2b3, 0x4a44, 0x5bcd, 0x6956, 0x78df, 0xc60, 0x1de9, 0x2f72, 0x3efb, 
                0xd68d, 0xc704, 0xf59f, 0xe416, 0x90a9, 0x8120, 0xb3bb, 0xa232, 0x5ac5, 0x4b4c, 0x79d7, 0x685e, 0x1ce1, 0xd68, 0x3ff3, 0x2e7a, 
                0xe70e, 0xf687, 0xc41c, 0xd595, 0xa12a, 0xb0a3, 0x8238, 0x93b1, 0x6b46, 0x7acf, 0x4854, 0x59dd, 0x2d62, 0x3ceb, 0xe70, 0x1ff9, 
                0xf78f, 0xe606, 0xd49d, 0xc514, 0xb1ab, 0xa022, 0x92b9, 0x8330, 0x7bc7, 0x6a4e, 0x58d5, 0x495c, 0x3de3, 0x2c6a, 0x1ef1, 0xf78
             };
      public static readonly int[,] Display_Parameters = new int[,] { { 1, 0x1c200, 2, 20, 1 }, { 0, 0x2580, 4, 20, 0 }, { 1, 0x1c200, 2, 0x10, 1 }, { 0, 0x2580, 4, 20, 0 }, { 1, 0x1c200, 4, 20, 1 } };
      private Display_Settings dSettings = new Display_Settings();

      public void BacklightOff()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          byte[] backLight = this._CommandSet.BackLight;
          if (this.dSettings.PacketFormat)
          {
            backLight[2] = 0;
            this.Calc_CRC(ref backLight, backLight.Length);
          }
          else
          {
            backLight[1] = 0;
          }
          this.commPort.Write(backLight, 0, backLight.Length);
        }
      }

      public void BacklightOn()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          byte[] backLight = this._CommandSet.BackLight;
          if (this.dSettings.PacketFormat)
          {
            backLight[2] = 100;
            this.Calc_CRC(ref backLight, backLight.Length);
          }
          else
          {
            backLight[1] = 100;
          }
          this.commPort.Write(backLight, 0, backLight.Length);
        }
      }

      public void Calc_CRC(ref byte[] Packet, int len)
      {
        uint num = 0xffff;
        for (int i = 0; i < (len - 1); i++)
        {
          num = (num >> 8) ^ crcLookupTable[(int)((IntPtr)((num ^ Packet[i]) & 0xff))];
        }
        Packet[len - 1] = (byte)~num;
      }

      public void ClearDisplay()
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          byte[] clearDisplay = this._CommandSet.ClearDisplay;
          if (Display_Parameters[this._DisplayType, 0] == 1)
          {
            this.Calc_CRC(ref clearDisplay, clearDisplay.Length);
          }
          this.commPort.Write(clearDisplay, 0, clearDisplay.Length);
        }
      }

      public void CloseDisplay(bool _blankOnExit)
      {
        try
        {
          if (this._isOpen & this.commPort.IsOpen)
          {
            if (_blankOnExit)
            {
              for (int i = this._currentContrast; i > 0; i--)
              {
                this.SetBacklightBrightness(i);
                Thread.Sleep(5);
              }
              this.BacklightOff();
            }
            this.ClearDisplay();
            if ((this.commPort != null) && this.commPort.IsOpen)
            {
              this.commPort.Close();
              this.commPort.DataReceived -= new SerialDataReceivedEventHandler(this.WhenDataReceived);
            }
            this._isOpen = false;
          }
        } catch (Exception exception)
        {
          Log.Error(exception);
        }
      }

      private void CreateCommands(DisplayType dType)
      {
        this._CommandSet = new CommandSet();
        if (this.dSettings.PacketFormat)
        {
          this._CommandSet.HideDisplay = null;
          this._CommandSet.RestoreDisplay = null;
          this._CommandSet.HideCursor = new byte[] { 12, 1, 0, 0xff };
          this._CommandSet.BackLight = new byte[] { 14, 1, 0xff, 0xff };
          this._CommandSet.Contrast = new byte[] { 13, 1, 0xff, 0xff };
          this._CommandSet.SetPosition = new byte[] { 11, 2, 0xff, 0xff, 0xff };
          this._CommandSet.HorizontalBar = null;
          this._CommandSet.ScrollOff = null;
          this._CommandSet.WrapOff = null;
          this._CommandSet.SetCustomChar = new byte[] { 9, 9, 0xff, 0, 0, 0, 0, 0, 0, 0, 0, 0xff };
          this._CommandSet.SetKeyReport = new byte[] { 0x17, 2, 0, 60, 0xff };
          if (Display_Parameters[(int)dType, 0] == 0x10)
          {
            this._CommandSet.SetLine = new byte[] { 
                            0x1f, 0x12, 0, 0xff, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                            0x20, 0x20, 0x20, 0x20, 0xff
                         };
          }
          else
          {
            this._CommandSet.SetLine = new byte[] { 
                            0x1f, 0x16, 0, 0xff, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0xff
                         };
          }
          byte[] buffer = new byte[3];
          buffer[0] = 6;
          buffer[2] = 0xff;
          this._CommandSet.ClearDisplay = buffer;
        }
        else
        {
          this._CommandSet.HideDisplay = new byte[] { 2 };
          this._CommandSet.RestoreDisplay = new byte[] { 3 };
          this._CommandSet.HideCursor = new byte[] { 4 };
          this._CommandSet.BackLight = new byte[] { 14, 0xff };
          this._CommandSet.Contrast = new byte[] { 15, 0xff };
          this._CommandSet.SetPosition = new byte[] { 0x11, 0xff, 0xff };
          this._CommandSet.HorizontalBar = new byte[] { 0x12, 0, 60, 0, 0x13, 0xff, 0xff };
          this._CommandSet.ScrollOff = new byte[] { 20 };
          this._CommandSet.WrapOff = new byte[] { 0x18 };
          byte[] buffer9 = new byte[10];
          buffer9[0] = 0x19;
          this._CommandSet.SetCustomChar = buffer9;
          this._CommandSet.SetKeyReport = null;
          if (Display_Parameters[(int)dType, 3] == 0x10)
          {
            this._CommandSet.SetLine = new byte[] { 
                            0x11, 0, 0xff, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                            0x20, 0x20, 0x20
                         };
            if (Display_Parameters[(int)dType, 2] == 2)
            {
              this._CommandSet.ClearDisplay = new byte[] { 
                                0x11, 0, 0, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x11, 0, 1, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x20
                             };
            }
            else
            {
              this._CommandSet.ClearDisplay = new byte[] { 
                                0x11, 0, 0, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x11, 0, 1, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x11, 0, 2, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x11, 0, 3, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
                             };
            }
          }
          else
          {
            this._CommandSet.SetLine = new byte[] { 
                            0x11, 0, 0xff, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
                         };
            if (Display_Parameters[(int)dType, 2] == 2)
            {
              this._CommandSet.ClearDisplay = new byte[] { 
                                0x11, 0, 0, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x11, 0, 1, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
                             };
            }
            else
            {
              this._CommandSet.ClearDisplay = new byte[] { 
                                0x11, 0, 0, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x11, 0, 1, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x11, 0, 
                                2, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x11, 0, 3, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20
                             };
            }
          }
        }
      }

      public void DisableCustomKeypadMapping()
      {
        this._useCustomKeypadMapping = false;
      }

      public void DisplayOff()
      {
        this._IsDisplayOff = true;
      }

      public void DisplayOn()
      {
        this._IsDisplayOff = false;
      }

      public void DrawHorizontalBarGraph(int column, int row, int RightToLeft, int length)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          byte[] setLine;
          if (this.dSettings.PacketFormat)
          {
            setLine = this._CommandSet.SetLine;
            if (length > 0)
            {
              setLine[3] = (byte)row;
              if (RightToLeft == 0)
              {
                for (int i = 0; i < this.dSettings.Columns; i++)
                {
                  if (((i + 1) * 6) < length)
                  {
                    setLine[4] = 5;
                  }
                  else if ((length - (i * 6)) == 0)
                  {
                    setLine[4 + i] = 0x20;
                  }
                  else
                  {
                    setLine[4 + i] = (byte)(0xa5 - (length - (i * 6)));
                  }
                }
              }
              else
              {
                for (int j = 0; j < this.dSettings.Columns; j++)
                {
                  if (((j + 1) * 6) < length)
                  {
                    setLine[(4 + (this.dSettings.Columns - 1)) - j] = 5;
                  }
                  else if ((length - (j * 6)) == 0)
                  {
                    setLine[(4 + (this.dSettings.Columns - 1)) - j] = 0x20;
                  }
                  else
                  {
                    setLine[(4 + (this.dSettings.Columns - 1)) - j] = (byte)(0xa5 - (length - (j * 6)));
                  }
                }
              }
            }
            this.Calc_CRC(ref setLine, setLine.Length);
          }
          else
          {
            setLine = this._CommandSet.HorizontalBar;
            setLine[4] = (byte)(this.dSettings.Columns - 1);
            setLine[5] = (RightToLeft == 0) ? ((byte)length) : ((byte)(0x100 - length));
            setLine[6] = (byte)row;
          }
          this.commPort.Write(setLine, 0, setLine.Length);
        }
      }

      public void DrawVerticalBarGraph(ref EQControl EQSettings, ref byte[] EqDataArray)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          int num = (this.dSettings.Columns - EQSettings.Render_BANDS) / 2;
          if ((num > 0) & EQSettings.UseStereoEq)
          {
            num--;
          }
          if (EQSettings.UseStereoEq)
          {
            num = 1;
          }
          byte[] setLine = this._CommandSet.SetLine;
          byte[] packet = this._CommandSet.SetLine;
          byte[] buffer3 = this._CommandSet.SetLine;
          byte[] buffer4 = this._CommandSet.SetLine;
          int index = 2;
          if (this.dSettings.PacketFormat)
          {
            index = 3;
          }
          if (this.dSettings.Rows == 2)
          {
            setLine[index] = 1;
            packet[index] = 0;
          }
          else
          {
            setLine[index] = 3;
            packet[index] = 2;
            buffer3[index] = 1;
            buffer4[index] = 0;
          }
          index = 3;
          if (this.dSettings.PacketFormat)
          {
            index = 4;
          }
          for (int i = 0; i < 0x10; i++)
          {
            bool flag = true;
            if (EQSettings.UseStereoEq)
            {
              if ((i == 7) & (EQSettings.Render_BANDS == this.dSettings.Columns))
              {
                flag = false;
              }
              else if ((i == 15) & (EQSettings.Render_BANDS == this.dSettings.Columns))
              {
                flag = false;
              }
            }
            if (flag && (EqDataArray[1 + i] > 0))
            {
              if (this.dSettings.Rows == 2)
              {
                if (EqDataArray[1 + i] > 8)
                {
                  packet[index + i] = (byte)(EqDataArray[1 + i] - 9);
                }
                else
                {
                  packet[index + i] = 0x20;
                  setLine[index + i] = (byte)(EqDataArray[1 + i] - 1);
                }
              }
              else if (EqDataArray[1 + i] > 0x18)
              {
                buffer4[index + i] = (byte)(EqDataArray[1 + i] - 0x19);
              }
              else if (EqDataArray[1 + i] > 0x10)
              {
                buffer4[index + i] = 0x20;
                buffer3[index + i] = (byte)(EqDataArray[1 + i] - 0x11);
              }
              else if (EqDataArray[1 + i] > 8)
              {
                buffer4[index + i] = 0x20;
                buffer3[index + i] = 0x20;
                packet[index + i] = (byte)(EqDataArray[1 + i] - 9);
              }
              else
              {
                buffer4[index + i] = 0x20;
                buffer3[index + i] = 0x20;
                packet[index + i] = 0x20;
                setLine[index + i] = (byte)(EqDataArray[1 + i] - 1);
              }
            }
          }
          if (this.dSettings.Rows == 4)
          {
            if (this.dSettings.PacketFormat)
            {
              this.Calc_CRC(ref buffer4, buffer4.Length);
              this.Calc_CRC(ref buffer3, buffer3.Length);
            }
            this.commPort.Write(buffer4, 0, buffer4.Length);
            this.commPort.Write(buffer3, 0, buffer3.Length);
          }
          if (this.dSettings.PacketFormat)
          {
            this.Calc_CRC(ref packet, packet.Length);
            this.Calc_CRC(ref setLine, setLine.Length);
          }
          this.commPort.Write(packet, 0, packet.Length);
          this.commPort.Write(setLine, 0, setLine.Length);
        }
      }

      public void EnableCustomKeypadMapping()
      {
        this._useCustomKeypadMapping = true;
      }

      public void FireKeypadEvent(int EventCode)
      {
        bool extensiveLogging = Settings.Instance.ExtensiveLogging;
        if (extensiveLogging)
        {
          Log.Info("CFontz.FireKeypadEvent(): called", new object[0]);
        }
        if (!this._inputHandler.MapAction(EventCode))
        {
          if (extensiveLogging)
          {
            Log.Info("CFontz.FireKeypadEvent(): No button mapping for Keypad button = {0}", new object[] { EventCode.ToString("x00") });
          }
        }
        else if (extensiveLogging)
        {
          Log.Info("CFontz.FireKeypadEvent(): fired event for Keypad button = {0}", new object[] { EventCode.ToString("x00") });
        }
        if (extensiveLogging)
        {
          Log.Info("CFontz.FireKeypadEvent(): completed", new object[0]);
        }
      }

      public int GetColumns()
      {
        return this.dSettings.Columns;
      }

      private int GetDisplayType(string DisplayName)
      {
        switch (DisplayName.Substring(0, 6))
        {
          case "CFA631":
            return 0;

          case "CFA632":
            return 1;

          case "CFA633":
            return 2;

          case "CFA634":
            return 3;

          case "CFA635":
            return 4;
        }
        return -1;
      }

      public int GetRows()
      {
        return this.dSettings.Rows;
      }

      public void InitHorizontalBarGraph()
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff) && (this._LastCharset != 1))
        {
          byte[,] buffer = new byte[,] { { 0, 1, 1, 1, 1, 1, 1, 0 }, { 0, 3, 3, 3, 3, 3, 3, 0 }, { 0, 7, 7, 7, 7, 7, 7, 0 }, { 0, 15, 15, 15, 15, 15, 15, 0 }, { 0, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0 }, { 0, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0x3f, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0 } };
          for (int i = 0; i < 8; i++)
          {
            int num2;
            byte[] setCustomChar = this._CommandSet.SetCustomChar;
            if (this.dSettings.PacketFormat)
            {
              setCustomChar[2] = (byte)i;
              num2 = 3;
            }
            else
            {
              setCustomChar[1] = (byte)i;
              num2 = 2;
            }
            for (int j = 0; j < 8; j++)
            {
              setCustomChar[num2 + j] = buffer[i, j];
            }
            if (this.dSettings.PacketFormat)
            {
              this.Calc_CRC(ref setCustomChar, setCustomChar.Length);
            }
            this.commPort.Write(setCustomChar, 0, setCustomChar.Length);
          }
          this._LastCharset = 1;
        }
      }

      public void InitVerticalBarGraph()
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff) && (this._LastCharset != 2))
        {
          byte[,] buffer = new byte[,] { { 0, 0, 0, 0, 0, 0, 0, 30 }, { 0, 0, 0, 0, 0, 0, 30, 30 }, { 0, 0, 0, 0, 0, 30, 30, 30 }, { 0, 0, 0, 0, 30, 30, 30, 30 }, { 0, 0, 0, 30, 30, 30, 30, 30 }, { 0, 0, 30, 30, 30, 30, 30, 30 }, { 0, 30, 30, 30, 30, 30, 30, 30 }, { 30, 30, 30, 30, 30, 30, 30, 30 } };
          for (int i = 0; i < 8; i++)
          {
            int num2;
            byte[] setCustomChar = this._CommandSet.SetCustomChar;
            if (this.dSettings.PacketFormat)
            {
              setCustomChar[2] = (byte)i;
              num2 = 3;
            }
            else
            {
              setCustomChar[1] = (byte)i;
              num2 = 2;
            }
            for (int j = 0; j < 8; j++)
            {
              setCustomChar[num2 + j] = buffer[i, j];
            }
            if (this.dSettings.PacketFormat)
            {
              this.Calc_CRC(ref setCustomChar, setCustomChar.Length);
            }
            this.commPort.Write(setCustomChar, 0, setCustomChar.Length);
          }
          this._LastCharset = 2;
        }
      }

      public bool OpenDisplay(string _port, string displayName, bool _brightness, int _brightnessLevel, bool _contrast, int _contrastLevel, bool EnableKeypad)
      {
        this._DisplayType = this.GetDisplayType(displayName);
        if ((this._DisplayType > 4) || (this._DisplayType == -1))
        {
          Log.Info("CFontz.CFDisplay.OpenDisplay(): INVALID DISPLAY TYPE = {0}!", new object[] { displayName });
          this._isOpen = false;
        }
        else
        {
          try
          {
            this._UseBrightness = _brightness;
            this._currentBrightness = _brightnessLevel;
            this._UseContrast = _contrast;
            this._currentContrast = _contrastLevel;
            this.dSettings.PacketFormat = Display_Parameters[this._DisplayType, 0] == 1;
            this.dSettings.BaudRate = Display_Parameters[this._DisplayType, 1];
            this.dSettings.Rows = Display_Parameters[this._DisplayType, 2];
            this.dSettings.Columns = Display_Parameters[this._DisplayType, 3];
            this.dSettings.HasKeypad = Display_Parameters[this._DisplayType, 4] == 1;
            this.commPort = new SerialPort(_port, this.dSettings.BaudRate, Parity.None, 8, StopBits.One);
            if (EnableKeypad & this.dSettings.HasKeypad)
            {
              this.commPort.DataReceived += new SerialDataReceivedEventHandler(this.WhenDataReceived);
              this.commPort.ReceivedBytesThreshold = 1;
            }
            this.commPort.Open();
            this.BacklightOn();
            this.SetBacklightBrightness(this._currentBrightness);
            this.ClearDisplay();
            this.commPort.DiscardInBuffer();
            this._isOpen = true;
            this._IsDisplayOff = false;
          } catch (Exception exception)
          {
            Log.Info("CFontz.CFDisplay.OpenDisplay(): CAUGHT EXCEPTION while opening display! - {0}", new object[] { exception });
            this._isOpen = false;
          }
        }
        return this._isOpen;
      }

      public void SetBacklightBrightness(int brightness)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          byte[] backLight = this._CommandSet.BackLight;
          if (this.dSettings.PacketFormat)
          {
            backLight[2] = (byte)brightness;
            this.Calc_CRC(ref backLight, backLight.Length);
          }
          else
          {
            backLight[1] = (byte)brightness;
          }
          this._currentBrightness = brightness;
          this.commPort.Write(backLight, 0, backLight.Length);
        }
      }

      public void SetContrast(int contrast)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          byte[] packet = this._CommandSet.Contrast;
          if (Display_Parameters[this._DisplayType, 0] == 1)
          {
            packet[2] = (byte)contrast;
            this.Calc_CRC(ref packet, packet.Length);
          }
          else
          {
            packet[1] = (byte)contrast;
          }
          this.commPort.Write(packet, 0, packet.Length);
        }
      }

      public void SetLine(int _line, string _message)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.BacklightOn();
          this.SetBacklightBrightness(this._currentBrightness);
          this.SetContrast(this._currentContrast);
          if (_line <= (this.dSettings.Rows - 1))
          {
            int num;
            byte[] setLine = this._CommandSet.SetLine;
            _message = _message + "                      ";
            _message = _message.Substring(0, this.dSettings.Columns);
            if (this.dSettings.PacketFormat)
            {
              setLine[3] = (byte)_line;
              num = 4;
            }
            else
            {
              setLine[2] = (byte)_line;
              num = 3;
            }
            for (int i = 0; i < this.dSettings.Columns; i++)
            {
              setLine[num + i] = (byte)_message[i];
            }
            if (this.dSettings.PacketFormat)
            {
              this.Calc_CRC(ref setLine, setLine.Length);
            }
            this.commPort.Write(setLine, 0, setLine.Length);
          }
        }
      }

      public bool SetupKeypad(bool UseCustomKeypadMap)
      {
        bool flag = false;
        bool flag2 = false;
        if (!UseCustomKeypadMap)
        {
          return true;
        }
        try
        {
          if (this.TestXmlVersion(Config.GetFile(Config.Dir.CustomInputDefault, "CFontz_Keypad.xml")) < 3)
          {
            Log.Info("CFontz.CFDisplay.SetupKeypad(): Deleting CFontz_Keypad mapping file with the wrong version stamp.", new object[0]);
            File.Delete(Config.GetFile(Config.Dir.CustomInputDefault, "CFontz_Keypad.xml"));
          }
          if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "CFontz_Keypad.xml")))
          {
            Log.Info("CFontz.Setup(): Creating default CFontz_Keypad mapping file", new object[0]);
            if (!CFontz.AdvancedSettings.CreateDefaultKeyPadMapping())
            {
              Log.Info("CFontz.CFDisplay.SetupKeypad(): ERROR Creating default CFontz_Keypad mapping file", new object[0]);
              flag2 = false;
            }
            else
            {
              flag2 = true;
            }
          }
          else
          {
            flag2 = true;
          }
        } catch (Exception exception)
        {
          Log.Info("CFontz.CFDisplay.SetupKeypad(): CAUGHT EXCEPTION while loading InputHander - {0}", new object[] { exception });
          flag2 = false;
          flag = false;
        }
        if (flag2)
        {
          Log.Info("CFontz.CFDisplay.SetupKeypad(): Loading InputHandler", new object[0]);
          this._inputHandler = new InputHandler("CFontz_Keypad");
          Log.Info("CFontz.CFDisplay.SetupKeypad(): InputHandler loaded = {0}", new object[] { this._inputHandler.IsLoaded });
          if (this._inputHandler.IsLoaded)
          {
            flag = true;
          }
          else
          {
            flag = false;
            Log.Info("CFontz.CFDisplay.SetupKeypad(): error loading InputHandler - remote support disabled", new object[0]);
          }
        }
        else
        {
          Log.Info("CFontz.CFDisplay.SetupKeypad(): Keypad support disabled - no keypad mapping file", new object[0]);
          flag = false;
        }
        if (flag && this._inputHandler.IsLoaded)
        {
          return flag;
        }
        Log.Info("CFontz.CFDisplay.SetupKeypad(): Error loading Keypad mapping file - Keypad support disabled", new object[0]);
        return false;
      }

      public int TestXmlVersion(string xmlPath)
      {
        if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "CFontz_Keypad.xml")))
        {
          return 3;
        }
        XmlDocument document = new XmlDocument();
        document.Load(xmlPath);
        return Convert.ToInt32(document.DocumentElement.SelectSingleNode("/mappings").Attributes["version"].Value);
      }

      private void WhenDataReceived(object sender, SerialDataReceivedEventArgs e)
      {
        byte eventCode = (byte)this.commPort.ReadByte();
        if (!this._useCustomKeypadMapping)
        {
          Action action;
          Log.Info("CFDisplay: received KeyPad event", new object[0]);
          switch (eventCode)
          {
            case 0x4b:
              Log.Info("CFDisplay: received KeyPad event - Cursor Up {0}", new object[] { eventCode.ToString("x00") });
              action = new Action(Action.ActionType.ACTION_MOVE_UP, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 0x4c:
              Log.Info("CFDisplay: received KeyPad event - Cursor Down {0}", new object[] { eventCode.ToString("x00") });
              action = new Action(Action.ActionType.ACTION_MOVE_DOWN, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 0x52:
              Log.Info("CFDisplay: received KeyPad event - Cursor Left{0}", new object[] { eventCode.ToString("x00") });
              action = new Action(Action.ActionType.ACTION_MOVE_LEFT, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 70:
              Log.Info("CFDisplay: received KeyPad event - Cursor Right{0}", new object[] { eventCode.ToString("x00") });
              action = new Action(Action.ActionType.ACTION_MOVE_RIGHT, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 0x4a:
              Log.Info("CFDisplay: received KeyPad event - Enter {0}", new object[] { eventCode.ToString("x00") });
              action = new Action(Action.ActionType.ACTION_SELECT_ITEM, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 80:
              Log.Info("CFDisplay: received KeyPad event - F1 {0}", new object[] { eventCode.ToString("x00") });
              action = new Action(Action.ActionType.ACTION_STEP_BACK, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 0x51:
              Log.Info("CFDisplay: received KeyPad event - F2 {0}", new object[] { eventCode.ToString("x00") });
              action = new Action(Action.ActionType.ACTION_STOP, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;
          }
          Log.Info("CFDisplay: received KeyPad event - received byte {0} Unknown Key", new object[] { eventCode.ToString("x00") });
        }
        else
        {
          this.FireKeypadEvent(eventCode);
        }
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct CommandSet
      {
        public byte[] HideDisplay;
        public byte[] RestoreDisplay;
        public byte[] HideCursor;
        public byte[] BackLight;
        public byte[] Contrast;
        public byte[] SetPosition;
        public byte[] HorizontalBar;
        public byte[] ScrollOff;
        public byte[] WrapOff;
        public byte[] SetCustomChar;
        public byte[] SetKeyReport;
        public byte[] SetLine;
        public byte[] ClearDisplay;
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct Display_Settings
      {
        public bool PacketFormat;
        public int BaudRate;
        public int Rows;
        public int Columns;
        public bool HasKeypad;
      }

      public enum DisplayType
      {
        CFA631 = 0,
        CFA632 = 1,
        CFA633 = 2,
        CFA634 = 3,
        CFA635 = 4,
        MAX = 4,
        UNSUPPORTED = -1
      }

      private enum KEYPAD_Codes : byte
      {
        CursorDown = 0x4c,
        CursorLeft = 0x52,
        CursorRight = 70,
        CursorUp = 0x4b,
        Enter = 0x4a,
        F1 = 0x51,
        F2 = 80
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyPadControl
    {
      public bool EnableKeyPad;
      public bool EnableCustom;
    }
  }
}

