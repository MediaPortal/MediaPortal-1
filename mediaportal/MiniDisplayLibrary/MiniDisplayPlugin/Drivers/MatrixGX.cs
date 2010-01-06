using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using LibDriverCoreClient;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Ripper;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class MatrixGX : BaseDisplay, IDisplay, IDisposable
  {
    private string _errorMessage = "";
    private int _Gcols;
    private int _Grows;
    public bool _IsConfiguring;
    private bool _isDisabled;
    private bool _IsOpen;
    private readonly string[] _Lines = new string[2];
    private static bool _useDiskIconForAllMedia;
    private static bool _useIcons;
    //private static bool _useInvertedDisplay;
    private static bool _useProgressDisplay;
    private static bool _useVolumeDisplay;
    private AdvancedSettings AdvSettings;
    private bool DoDebug;
    private byte[] lastHash;
    private DateTime LastSettingsCheck = DateTime.Now;
    private readonly MOGXDisplay MOD = new MOGXDisplay();
    private byte[] ReceivedBitmapData;
    private DateTime SettingsLastModTime;
    private readonly SHA256Managed sha256 = new SHA256Managed();

    private void AdvancedSettings_OnSettingsChanged()
    {
      Log.Info("MatrixGX.AdvancedSettings_OnSettingsChanged(): called");
      this.CleanUp();
      this.LoadAdvancedSettings();
      Thread.Sleep(100);
      this.Setup(Settings.Instance.Port, Settings.Instance.TextHeight, Settings.Instance.TextWidth,
                 Settings.Instance.TextComDelay, Settings.Instance.GraphicHeight, Settings.Instance.GraphicWidth,
                 Settings.Instance.GraphicComDelay, Settings.Instance.BackLightControl, Settings.Instance.Backlight,
                 Settings.Instance.ContrastControl, Settings.Instance.Contrast, Settings.Instance.BlankOnExit);
      this.Initialize();
    }

    public void CleanUp()
    {
      if (this.MOD.IsOpen)
      {
        Log.Debug("(IDisplay) MatrixGX.CleanUp() - called");
        this.MOD.ClearDisplay();
        this.MOD.CloseDisplay();
        AdvancedSettings.OnSettingsChanged -=
          new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
      }
    }

    private void Clear()
    {
      if (this.MOD.IsOpen)
      {
        Log.Debug("(IDisplay) MatrixGX.Clear() - called");
        this.MOD.ClearDisplay();
      }
    }

    public void Configure()
    {
      new MatrixGX_AdvancedSetupForm().ShowDialog();
    }

    public void Dispose()
    {
      Log.Debug("MatrixGX.Dispose() - called");
    }

    public void DrawImage(Bitmap bitmap)
    {
      if (!this._isDisabled && this.MOD.IsOpen)
      {
        Log.Debug("(IDisplay) MatrixGX.DrawImage() - called");
        if (bitmap == null)
        {
          Log.Debug("(IDisplay) MatrixGX.DrawImage():  bitmap null");
        }
        else
        {
          BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly,
                                                  bitmap.PixelFormat);
          try
          {
            if (this.ReceivedBitmapData == null)
            {
              this.ReceivedBitmapData = new byte[bitmapdata.Stride * this._Grows];
            }
            Marshal.Copy(bitmapdata.Scan0, this.ReceivedBitmapData, 0, this.ReceivedBitmapData.Length);
          }
          catch (Exception exception)
          {
            Log.Debug("(IDisplay) MatrixGX.DrawImage(): caught exception - {0}", new object[] {exception.ToString()});
          }
          finally
          {
            bitmap.UnlockBits(bitmapdata);
          }
          byte[] buffer = this.sha256.ComputeHash(this.ReceivedBitmapData);
          if (ByteArray.AreEqual(buffer, this.lastHash))
          {
            Log.Debug("(IDisplay) MatrixGX.DrawImage() - completed - bitmap not changed");
          }
          else
          {
            this.lastHash = buffer;
            this.MOD.SendImage(bitmap);
            this.UpdateAdvancedSettings();
            Log.Debug("(IDisplay) MatrixGX.DrawImage() - completed");
          }
        }
      }
    }

    public void Initialize()
    {
      if (!this._isDisabled)
      {
        if (this.MOD.IsOpen)
        {
          this.MOD.CloseDisplay();
        }
        this._IsOpen = this.MOD.OpenDisplay(this.AdvSettings);
        if (this._IsOpen)
        {
          Log.Debug("(IDisplay) MatrixGX.Initialize() - Display opened.");
          this._isDisabled = false;
          AdvancedSettings.OnSettingsChanged +=
            new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
        }
        else
        {
          Log.Debug("(IDisplay) MatrixGX.Initialize() - Unable to open device - display disabled");
          this._isDisabled = true;
          this._errorMessage = "MatrixGX.Initialize() failed... No Matrix GX display found";
        }
        Log.Debug("(IDisplay) MatrixGX.Initialize() - called");
        this.Clear();
      }
    }

    private void LoadAdvancedSettings()
    {
      this.AdvSettings = AdvancedSettings.Load();
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixGX.xml"));
      this.SettingsLastModTime = info.LastWriteTime;
      this.LastSettingsCheck = DateTime.Now;
    }

    public void SetCustomCharacters(int[][] customCharacters) {}

    public void SetLine(int line, string message)
    {
      if (!this._isDisabled && this.MOD.IsOpen)
      {
        this.UpdateAdvancedSettings();
        Log.Info("(IDisplay) MatrixGX.SetLine() called for Line {0} msg: '{1}'", new object[] {line.ToString(), message});
        this._Lines[line] = message;
        if (line == 1)
        {
          this.MOD.SendText(this._Lines[0], this._Lines[1]);
        }
        Log.Info("(IDisplay) MatrixGX.SetLine() completed");
      }
    }

    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLight, int _backLightLevel, bool _contrast, int _contrastLevel, bool _blankOnExit)
    {
      this._IsConfiguring = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
      this.DoDebug = this._IsConfiguring | Settings.Instance.ExtensiveLogging;
      Log.Info("(IDisplay) MatrixGX.Setup() - called");
      Log.Info("(IDisplay) MatrixGX.Setup(): MatrixGX Driver - {0}", new object[] {this.Description});
      Log.Info("(IDisplay) MatrixGX.Setup(): Called by \"{0}\".", new object[] {Assembly.GetEntryAssembly().FullName});
      FileInfo info = new FileInfo(Assembly.GetExecutingAssembly().Location);
      if (this.DoDebug)
      {
        Log.Info("MatrixGX: Assembly creation time: {0} ( {1} UTC )",
                 new object[] {info.LastWriteTime, info.LastWriteTimeUtc.ToUniversalTime()});
      }
      if (this.DoDebug)
      {
        Log.Info("MatrixGX: Platform: {0}", new object[] {Environment.OSVersion.VersionString});
      }
      try
      {
        this.LoadAdvancedSettings();
        AdvancedSettings.OnSettingsChanged +=
          new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
        this._Grows = _linesG;
        this._Gcols = _colsG;
        if (this._Gcols > 240)
        {
          Log.Info("(IDisplay) MatrixGX.Setup() - Invalid Graphics Columns value");
          this._Grows = 240;
        }
        if (this._Grows > 0x40)
        {
          Log.Info("(IDisplay) MatrixGX.Setup() - Invalid Graphics Lines value");
          this._Grows = 0x40;
        }
      }
      catch (Exception exception)
      {
        Log.Debug("(IDisplay) MatrixGX.Setup() - threw an exception: {0}", new object[] {exception.ToString()});
        this._isDisabled = true;
        this._errorMessage = "MatrixGX.setup() failed... Did you copy the required files to the MediaPortal directory?";
      }
      Log.Info("(IDisplay) MatrixGX.Setup() - completed");
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= this.LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (this.DoDebug)
        {
          Log.Info("MatrixGX.UpdateAdvancedSettings(): called");
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixGX.xml")))
        {
          FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixGX.xml"));
          if (info.LastWriteTime.Ticks > this.SettingsLastModTime.Ticks)
          {
            if (this.DoDebug)
            {
              Log.Info("MatrixGX.UpdateAdvancedSettings(): updating advanced settings");
            }
            this.LoadAdvancedSettings();
          }
        }
        if (this.DoDebug)
        {
          Log.Info("MatrixGX.UpdateAdvancedSettings(): completed");
        }
      }
    }

    public string Description
    {
      get { return "Matrix Orbital GX Series LCD driver v03_09_2008b"; }
    }

    public string ErrorMessage
    {
      get { return this._errorMessage; }
    }

    public bool IsDisabled
    {
      get
      {
        this._isDisabled = false;
        int num = 0;
        if (!File.Exists("LibDriverCoreClient.dll"))
        {
          num++;
        }
        if (!File.Exists("log4net.dll"))
        {
          num += 2;
        }
        if (!File.Exists("fastbitmap.dll"))
        {
          num += 4;
        }
        if (num > 0)
        {
          if ((num & 1) > 0)
          {
            this._errorMessage = "Required file \"LibDriverCoreClient.dll\" is not installed!\n";
            this._isDisabled = true;
            Log.Info("(IDisplay) MatrixGX.IsDisabled() - Required file \"LibDriverCoreClient.dll\" is not installed!",
                     new object[0]);
          }
          if ((num & 2) > 0)
          {
            this._errorMessage = this._errorMessage + "Required file \"log4net.dll\" is not installed!\n";
            this._isDisabled = true;
            Log.Info("(IDisplay) MatrixGX.IsDisabled() - Required file \"log4net.dll\" is not installed!");
          }
          if ((num & 4) > 0)
          {
            this._errorMessage = this._errorMessage + "Required file \"fastbitmap.dll\" is not installed!\n";
            this._isDisabled = true;
            Log.Info("(IDisplay) MatrixGX.IsDisabled() - Required file \"fastbitmap.dll\" is not installed!",
                     new object[0]);
          }
        }
        return this._isDisabled;
      }
    }

    public string Name
    {
      get { return "MatrixGX"; }
    }

    public bool SupportsGraphics
    {
      get { return true; }
    }

    public bool SupportsText
    {
      get { return true; }
    }

    [Serializable]
    public class AdvancedSettings
    {
      private int m_backlightB;
      private int m_backlightG;
      private int m_backlightR;
      private bool m_BlankDisplayWhenIdle;
      private bool m_BlankDisplayWithVideo;
      private int m_BlankIdleTime = 30;
      private bool m_DelayEQ;
      private int m_DelayEqTime = 10;
      private bool m_EnableDisplayAction;
      private int m_EnableDisplayActionTime = 5;
      private bool m_EqDisplay;
      private int m_EqRate = 10;
      private bool m_EQTitleDisplay;
      private int m_EQTitleDisplayTime = 10;
      private int m_EQTitleShowTime = 2;
      private static AdvancedSettings m_Instance;
      private bool m_NormalEQ = true;
      private bool m_ProgressDisplay;
      private bool m_RestrictEQ;
      private bool m_SmoothEQ;
      private bool m_StereoEQ;
      private bool m_UseDiskIconForAllMedia;
      private bool m_UseIcons;
      private bool m_UseInvertedDisplay;
      private bool m_VolumeDisplay;
      private bool m_VUindicators;
      private bool m_VUmeter;
      private bool m_VUmeter2;

      public static event OnSettingsChangedHandler OnSettingsChanged;

      private static void Default(AdvancedSettings _settings)
      {
        _settings.ProgressDisplay = false;
        _settings.VolumeDisplay = false;
        _settings.UseInvertedDisplay = false;
        _settings.UseIcons = false;
        _settings.UseDiskIconForAllMedia = false;
        _settings.BacklightRED = 0xff;
        _settings.BacklightGREEN = 0xff;
        _settings.BacklightBLUE = 0xff;
        _settings.EqDisplay = false;
        _settings.NormalEQ = true;
        _settings.StereoEQ = false;
        _settings.VUmeter = false;
        _settings.VUmeter2 = false;
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
      }

      public static AdvancedSettings Load()
      {
        AdvancedSettings settings;
        Log.Debug("MatrixGX.AdvancedSettings.Load() started");
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixGX.xml")))
        {
          Log.Debug("MatrixGX.AdvancedSettings.Load() Loading settings from XML file");
          XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixGX.xml"));
          settings = (AdvancedSettings)serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Debug("MatrixGX.AdvancedSettings.Load() Loading settings from defaults");
          settings = new AdvancedSettings();
          Default(settings);
        }
        Log.Debug("MatrixGX.AdvancedSettings.Load() completed");
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

      public static void Save(AdvancedSettings ToSave)
      {
        Log.Debug("MatrixGX.AdvancedSettings.Save() Saving settings to XML file");
        XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixGX.xml"),
                                                 Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter)writer, ToSave);
        writer.Close();
        Log.Debug("MatrixGX.AdvancedSettings.Save() completed");
      }

      public static void SetDefaults()
      {
        Default(Instance);
      }

      [XmlAttribute]
      public int BacklightBLUE
      {
        get { return this.m_backlightB; }
        set { this.m_backlightB = value; }
      }

      [XmlAttribute]
      public int BacklightGREEN
      {
        get { return this.m_backlightG; }
        set { this.m_backlightG = value; }
      }

      [XmlAttribute]
      public int BacklightRED
      {
        get { return this.m_backlightR; }
        set { this.m_backlightR = value; }
      }

      [XmlAttribute]
      public bool BlankDisplayWhenIdle
      {
        get { return this.m_BlankDisplayWhenIdle; }
        set { this.m_BlankDisplayWhenIdle = value; }
      }

      [XmlAttribute]
      public bool BlankDisplayWithVideo
      {
        get { return this.m_BlankDisplayWithVideo; }
        set { this.m_BlankDisplayWithVideo = value; }
      }

      [XmlAttribute]
      public int BlankIdleTime
      {
        get { return this.m_BlankIdleTime; }
        set { this.m_BlankIdleTime = value; }
      }

      [XmlAttribute]
      public bool DelayEQ
      {
        get { return this.m_DelayEQ; }
        set { this.m_DelayEQ = value; }
      }

      [XmlAttribute]
      public int DelayEqTime
      {
        get { return this.m_DelayEqTime; }
        set { this.m_DelayEqTime = value; }
      }

      [XmlAttribute]
      public bool EnableDisplayAction
      {
        get { return this.m_EnableDisplayAction; }
        set { this.m_EnableDisplayAction = value; }
      }

      [XmlAttribute]
      public int EnableDisplayActionTime
      {
        get { return this.m_EnableDisplayActionTime; }
        set { this.m_EnableDisplayActionTime = value; }
      }

      [XmlAttribute]
      public bool EqDisplay
      {
        get { return this.m_EqDisplay; }
        set { this.m_EqDisplay = value; }
      }

      [XmlAttribute]
      public int EqRate
      {
        get { return this.m_EqRate; }
        set { this.m_EqRate = value; }
      }

      [XmlAttribute]
      public bool EQTitleDisplay
      {
        get { return this.m_EQTitleDisplay; }
        set { this.m_EQTitleDisplay = value; }
      }

      [XmlAttribute]
      public int EQTitleDisplayTime
      {
        get { return this.m_EQTitleDisplayTime; }
        set { this.m_EQTitleDisplayTime = value; }
      }

      [XmlAttribute]
      public int EQTitleShowTime
      {
        get { return this.m_EQTitleShowTime; }
        set { this.m_EQTitleShowTime = value; }
      }

      public static AdvancedSettings Instance
      {
        get
        {
          if (m_Instance == null)
          {
            m_Instance = Load();
          }
          return m_Instance;
        }
        set { m_Instance = value; }
      }

      [XmlAttribute]
      public bool NormalEQ
      {
        get { return this.m_NormalEQ; }
        set { this.m_NormalEQ = value; }
      }

      [XmlAttribute]
      public bool ProgressDisplay
      {
        get { return this.m_ProgressDisplay; }
        set { this.m_ProgressDisplay = value; }
      }

      [XmlAttribute]
      public bool RestrictEQ
      {
        get { return this.m_RestrictEQ; }
        set { this.m_RestrictEQ = value; }
      }

      [XmlAttribute]
      public bool SmoothEQ
      {
        get { return this.m_SmoothEQ; }
        set { this.m_SmoothEQ = value; }
      }

      [XmlAttribute]
      public bool StereoEQ
      {
        get { return this.m_StereoEQ; }
        set { this.m_StereoEQ = value; }
      }

      [XmlAttribute]
      public bool UseDiskIconForAllMedia
      {
        get { return this.m_UseDiskIconForAllMedia; }
        set { this.m_UseDiskIconForAllMedia = value; }
      }

      [XmlAttribute]
      public bool UseIcons
      {
        get { return this.m_UseIcons; }
        set { this.m_UseIcons = value; }
      }

      [XmlAttribute]
      public bool UseInvertedDisplay
      {
        get { return this.m_UseInvertedDisplay; }
        set { this.m_UseInvertedDisplay = value; }
      }

      [XmlAttribute]
      public bool VolumeDisplay
      {
        get { return this.m_VolumeDisplay; }
        set { this.m_VolumeDisplay = value; }
      }

      [XmlAttribute]
      public bool VUindicators
      {
        get { return this.m_VUindicators; }
        set { this.m_VUindicators = value; }
      }

      [XmlAttribute]
      public bool VUmeter
      {
        get { return this.m_VUmeter; }
        set { this.m_VUmeter = value; }
      }

      [XmlAttribute]
      public bool VUmeter2
      {
        get { return this.m_VUmeter2; }
        set { this.m_VUmeter2 = value; }
      }

      public delegate void OnSettingsChangedHandler();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOGX_Control
    {
      public int BackLightRed;
      public int BackLightGreen;
      public int BackLightBlue;
      public bool InvertDisplay;
    }

    private class MOGXDisplay
    {
      private bool _AudioUseASIO;
      private Thread _displayThread;
      private bool _isClosing;
      private bool _IsDisplayOff;
      private bool _isOpen;
      public static bool _mpIsIdle;
      private AdvancedSettings AdvSettings;
      private DisplayControl DisplaySettings;
      private bool DoDebug;
      private object DWriteMutex = new object();
      private EQControl EQSettings;
      private DCClient GX_Client;
      private DCCClientDeviceList GX_Devices;
      private Graphics GX_Graphics;
      private DCCSession GX_Session;
      private Bitmap GX_Surface;
      private readonly uint ICON_CDIn = 0x6b;
      private readonly uint ICON_DVDIn = 0x55;

      private string IdleMessage = ((Settings.Instance.IdleMessage != string.Empty)
                                      ? Settings.Instance.IdleMessage
                                      : "MediaPortal");

      private MOGX_Control MOGX_Control = new MOGX_Control();
      private SystemStatus MPStatus = new SystemStatus();
      private bool stopDisplayUpdateThread;
      private object StopMutex = new object();

      public void BacklightOff()
      {
        if (this._isOpen)
        {
          lock (this.DWriteMutex)
          {
            this.GX_Session.SetRGBBacklight(0, 0, 0);
          }
        }
      }

      public void BacklightOn()
      {
        if (this._isOpen)
        {
          lock (this.DWriteMutex)
          {
            this.GX_Session.SetRGBBacklight((byte)this.MOGX_Control.BackLightRed,
                                            (byte)this.MOGX_Control.BackLightGreen,
                                            (byte)this.MOGX_Control.BackLightBlue);
          }
        }
      }

      private void CheckIdleState()
      {
        if (this.MPStatus.MP_Is_Idle)
        {
          if (this.DoDebug)
          {
            Log.Info("MatrixGX.MOGXDisplay.DisplayLines(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}",
                     new object[] {this.DisplaySettings.BlankDisplayWhenIdle, this.DisplaySettings._BlankIdleTimeout});
          }
          if (this.DisplaySettings.BlankDisplayWhenIdle)
          {
            if (!_mpIsIdle)
            {
              if (this.DoDebug)
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayLines(): MP going IDLE");
              }
              this.DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
            }
            if (!this._IsDisplayOff &&
                ((DateTime.Now.Ticks - this.DisplaySettings._BlankIdleTime) > this.DisplaySettings._BlankIdleTimeout))
            {
              if (this.DoDebug)
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayLines(): Blanking display due to IDLE state");
              }
              this.DisplayOff();
            }
          }
          _mpIsIdle = true;
        }
        else
        {
          if (this.DisplaySettings.BlankDisplayWhenIdle & _mpIsIdle)
          {
            if (this.DoDebug)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayLines(): MP no longer IDLE - restoring display");
            }
            this.DisplayOn();
          }
          _mpIsIdle = false;
        }
      }

      public void ClearDisplay()
      {
        if (this._isOpen && !this._isClosing)
        {
          lock (this.DWriteMutex)
          {
            Log.Debug("MatrixGX.MOGXDisplay.ClearDisplay() - called");
            this.GX_Graphics = Graphics.FromImage(this.GX_Surface);
            this.GX_Graphics.Clear(Color.White);
            Log.Debug("MatrixGX.MOGXDisplay.ClearDisplay() - Sending blank image to device");
            this.GX_Session.SendAsyncFrame(this.GX_Surface);
          }
          Log.Debug("MatrixGX.MOGXDisplay.ClearDisplay() - completed");
        }
      }

      private void ClearIconArea()
      {
        if (_useIcons && !this._isClosing)
        {
          Log.Debug("MatrixGX.MOGXDisplay.ClearIconArea() - called");
          RectangleF textBounds = this.GetTextBounds();
          this.GX_Graphics.FillRectangle(Brushes.White, textBounds.Width, 0f, this.GX_Surface.Width - textBounds.Width,
                                         (float)this.GX_Surface.Height);
          Log.Debug("MatrixGX.MOGXDisplay.ClearIconArea() - completed");
        }
      }

      public void CloseDisplay()
      {
        Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() - called");
        try
        {
          lock (this.StopMutex)
          {
            Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() Stopping DisplayUpdate() Thread");
            Thread.Sleep(250);
            this.stopDisplayUpdateThread = true;
            goto Label_004E;
          }
          Label_0047:
          Thread.Sleep(100);
          Label_004E:
          if (this._displayThread.IsAlive)
          {
            goto Label_0047;
          }
          Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() DisplayUpdate() Thread stopped.");
          this._isClosing = true;
          lock (this.DWriteMutex)
          {
            this.GX_Graphics.Dispose();
            this.GX_Session.End();
            this.GX_Session = null;
            this.GX_Client.Disconnect();
            this.GX_Client = null;
            this._isOpen = false;
          }
          Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() - Display closed.");
        }
        catch (Exception exception)
        {
          Log.Debug("MatrixGX.MOGXDisplay.CloseDisplay() - caught exception on display close: {0}",
                    new object[] {exception.ToString()});
          Log.Error(exception);
          this._isOpen = false;
        }
      }

      private void DisplayEQ()
      {
        if (!_mpIsIdle && this.EQSettings._EqDataAvailable)
        {
          lock (this.DWriteMutex)
          {
            object obj3;
            RectangleF textBounds = this.GetTextBounds();
            if (this.DoDebug)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayEQ(): called");
            }
            this.EQSettings.Render_MaxValue = (this.EQSettings.UseNormalEq | this.EQSettings.UseStereoEq)
                                                ? ((int)textBounds.Height)
                                                : ((int)textBounds.Width);
            this.EQSettings.Render_BANDS = this.EQSettings.UseNormalEq ? 0x10 : (this.EQSettings.UseStereoEq ? 8 : 1);
            MiniDisplayHelper.ProcessEqData(ref this.EQSettings);
            Monitor.Enter(obj3 = this.DWriteMutex);
            try
            {
              this.GX_Graphics.FillRectangle(Brushes.White, textBounds);
              for (int i = 0; i < this.EQSettings.Render_BANDS; i++)
              {
                RectangleF ef2;
                if (this.DoDebug)
                {
                  Log.Info("MatrixGX.MOGXDisplay.DisplayEQ(): Rendering {0} band {1} = {2}",
                           new object[]
                             {
                               this.EQSettings.UseNormalEq
                                 ? "Normal EQ"
                                 : (this.EQSettings.UseStereoEq
                                      ? "Stereo EQ"
                                      : (this.EQSettings.UseVUmeter ? "VU Meter" : "VU Meter 2")), i,
                               this.EQSettings.UseNormalEq
                                 ? this.EQSettings.EqArray[1 + i].ToString()
                                 : (this.EQSettings.UseStereoEq
                                      ? (this.EQSettings.EqArray[1 + i].ToString() + " : " +
                                         this.EQSettings.EqArray[9 + i].ToString())
                                      : (this.EQSettings.EqArray[1 + i].ToString() + " : " +
                                         this.EQSettings.EqArray[2 + i].ToString()))
                             });
                }
                if (this.EQSettings.UseNormalEq)
                {
                  ef2 = new RectangleF(
                    (textBounds.X + (i * (((int)textBounds.Width) / this.EQSettings.Render_BANDS))) + 1f,
                    textBounds.Y + (((int)textBounds.Height) - this.EQSettings.EqArray[1 + i]),
                    (float)((((int)textBounds.Width) / this.EQSettings.Render_BANDS) - 2),
                    (float)this.EQSettings.EqArray[1 + i]);
                  this.GX_Graphics.FillRectangle(Brushes.Black, ef2);
                }
                else
                {
                  int num2;
                  RectangleF ef3;
                  if (this.EQSettings.UseStereoEq)
                  {
                    int num4 = (((int)textBounds.Width) / 2) / this.EQSettings.Render_BANDS;
                    num2 = i * num4;
                    int num3 = (i + this.EQSettings.Render_BANDS) * num4;
                    ef2 = new RectangleF((textBounds.X + num2) + 1f,
                                         textBounds.Y + (((int)textBounds.Height) - this.EQSettings.EqArray[1 + i]),
                                         (float)(num4 - 2), (float)this.EQSettings.EqArray[1 + i]);
                    ef3 = new RectangleF((textBounds.X + num3) + 1f,
                                         textBounds.Y + (((int)textBounds.Height) - this.EQSettings.EqArray[9 + i]),
                                         (float)(num4 - 2), (float)this.EQSettings.EqArray[9 + i]);
                    this.GX_Graphics.FillRectangle(Brushes.Black, ef2);
                    this.GX_Graphics.FillRectangle(Brushes.Black, ef3);
                  }
                  else if (this.EQSettings.UseVUmeter | this.EQSettings.UseVUmeter2)
                  {
                    ef2 = new RectangleF(textBounds.X + 1f, textBounds.Y + 1f, (float)this.EQSettings.EqArray[1 + i],
                                         (float)(((int)(textBounds.Height / 2f)) - 2));
                    num2 = this.EQSettings.UseVUmeter ? 0 : (((int)textBounds.Width) - this.EQSettings.EqArray[2 + i]);
                    ef3 = new RectangleF((textBounds.X + num2) + 1f, (textBounds.Y + (textBounds.Height / 2f)) + 1f,
                                         (float)this.EQSettings.EqArray[2 + i],
                                         (float)(((int)(textBounds.Height / 2f)) - 2));
                    this.GX_Graphics.FillRectangle(Brushes.Black, ef2);
                    this.GX_Graphics.FillRectangle(Brushes.Black, ef3);
                  }
                }
              }
            }
            catch (Exception exception)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayEQ(): CAUGHT EXCEPTION {0}", new object[] {exception});
              if (exception.Message.Contains("ThreadAbortException")) {}
            }
            finally
            {
              Monitor.Exit(obj3);
            }
          }
        }
      }

      private void DisplayOff()
      {
        if (!this._IsDisplayOff)
        {
          if (this.DisplaySettings.EnableDisplayAction & this.DisplaySettings._DisplayControlAction)
          {
            if ((DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction) <
                this.DisplaySettings._DisplayControlTimeout)
            {
              if (this.DoDebug)
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayOff(): DisplayControlAction Timer = {0}.",
                         new object[] {DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction});
              }
              return;
            }
            if (this.DoDebug)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayOff(): DisplayControlAction Timeout expired.");
            }
            this.DisplaySettings._DisplayControlAction = false;
            this.DisplaySettings._DisplayControlLastAction = 0L;
          }
          Log.Info("MatrixGX.MOGXDisplay.DisplayOff(): called");
          lock (this.DWriteMutex)
          {
            Log.Info("MatrixGX.MOGXDisplay.DisplayOff(): Sending Display OFF command to LCD");
            this.BacklightOff();
            this.ClearDisplay();
            this._IsDisplayOff = true;
          }
          Log.Info("MatrixGX.MOGXDisplay.DisplayOff(): completed");
        }
      }

      private void DisplayOn()
      {
        if (this._IsDisplayOff)
        {
          Log.Info("MatrixGX.MOGXDisplay.DisplayOn(): called");
          lock (this.DWriteMutex)
          {
            Log.Info("MatrixGX.MOGXDisplay.DisplayOn(): Sending Display ON command to LCD");
            this._IsDisplayOff = false;
            this.BacklightOn();
          }
          Log.Info("MatrixGX.MOGXDisplay.DisplayOn(): called");
        }
      }

      private void DisplayUpdate()
      {
        uint segments = 0;
        bool flag = false;
        DiskIcon icon = new DiskIcon();
        if (this.DoDebug)
        {
          Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Starting Display Update Thread");
        }
        CDDrive drive = new CDDrive();
        bool flag2 = false;
        int num2 = 0;
        if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
        {
          GUIWindowManager.OnNewAction += new OnActionHandler(this.OnExternalAction);
        }
        char[] cDDriveLetters = CDDrive.GetCDDriveLetters();
        if (this.DoDebug)
        {
          object[] arg = new object[] {cDDriveLetters.Length.ToString()};
          Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Found {0} CD/DVD Drives.", arg);
        }
        if (cDDriveLetters.Length > 0)
        {
          drive.Open(cDDriveLetters[0]);
        }
        icon.Reset();
        while (true)
        {
          lock (this.StopMutex)
          {
            if (this.stopDisplayUpdateThread)
            {
              if (this.DoDebug)
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Display Update Thread terminating");
              }
              if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
              {
                GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
              }
              return;
            }
          }
          MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
          flag = !flag;
          int num3 = num2;
          int num4 = 0;
          uint num5 = 0;
          icon.Off();
          icon.Animate();
          if (this.DoDebug)
          {
            Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Collecting status...");
          }
          if (MiniDisplayHelper.IsCaptureCardRecording())
          {
            num5 |= 0x2000;
            num4 = 5;
            if (this.DoDebug)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting REC icon");
            }
          }
          if (this.MPStatus.MediaPlayer_Playing)
          {
            string property;
            string[] strArray;
            if ((this.MPStatus.Media_IsTV || this.MPStatus.Media_IsTVRecording) &
                (!this.MPStatus.Media_IsDVD && !this.MPStatus.Media_IsCD))
            {
              if (this.MPStatus.MediaPlayer_Playing)
              {
                if (this.DisplaySettings.BlankDisplayWithVideo)
                {
                  this.DisplayOff();
                }
              }
              else
              {
                this.RestoreDisplayFromVideoOrIdle();
              }
              num5 |= 1;
              num4 = 1;
              if (this.DoDebug)
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_TV");
              }
            }
            if (this.MPStatus.Media_IsDVD || this.MPStatus.Media_IsCD)
            {
              if (this.MPStatus.Media_IsDVD & this.MPStatus.Media_IsVideo)
              {
                if (this.DisplaySettings.BlankDisplayWithVideo)
                {
                  this.DisplayOff();
                }
                num5 |= 8;
                num5 |= 2;
                num4 = 2;
                if (this.DoDebug)
                {
                  Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_DVD & ICON_MOVIE");
                }
              }
              else if (this.MPStatus.Media_IsCD & !this.MPStatus.Media_IsVideo)
              {
                num5 |= 0x10;
                num5 |= 4;
                num4 = 3;
                if (this.DoDebug)
                {
                  Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_MUSIC & ICON_CD");
                }
              }
              icon.On();
              icon.InvertOff();
              if (this.MPStatus.MediaPlayer_Playing)
              {
                if (this.MPStatus.Media_Speed > 0)
                {
                  icon.RotateCW();
                }
                else if (this.MPStatus.Media_Speed < 0)
                {
                  icon.RotateCCW();
                }
                icon.FlashOff();
              }
              else
              {
                this.RestoreDisplayFromVideoOrIdle();
                icon.FlashOn();
                num4 = 6;
                if (this.DoDebug)
                {
                  Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting PAUSED icon");
                }
              }
            }
            if (this.MPStatus.Media_IsMusic)
            {
              if (this.DoDebug)
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_MUSIC");
              }
              num5 |= 0x10;
              num4 = 3;
              icon.On();
              icon.InvertOn();
              if (this.MPStatus.MediaPlayer_Playing)
              {
                if (this.MPStatus.Media_Speed > 0)
                {
                  icon.RotateCW();
                }
                else if (this.MPStatus.Media_Speed < 0)
                {
                  icon.RotateCCW();
                }
                icon.FlashOff();
              }
              else
              {
                icon.FlashOn();
                num4 = 6;
                if (this.DoDebug)
                {
                  Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting PAUSED icon");
                }
              }
              if (!_useDiskIconForAllMedia)
              {
                icon.Off();
              }
              property = GUIPropertyManager.GetProperty("#Play.Current.File");
              if (property.Length > 0)
              {
                string str2;
                strArray = property.Split(new char[] {'.'});
                if ((strArray.Length > 1) && ((str2 = strArray[1]) != null))
                {
                  if (!(str2 == "mp3"))
                  {
                    if (str2 == "ogg")
                    {
                      num5 |= 0x80;
                      if (this.DoDebug)
                      {
                        Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_OGG");
                      }
                    }
                    else if (str2 == "wma")
                    {
                      num5 |= 0x200;
                      if (this.DoDebug)
                      {
                        Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_WMA");
                      }
                    }
                    else if (str2 == "wav")
                    {
                      num5 |= 0x4000;
                      if (this.DoDebug)
                      {
                        Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_WAV");
                      }
                    }
                  }
                  else
                  {
                    num5 |= 0x100;
                    if (this.DoDebug)
                    {
                      Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_MP3");
                    }
                  }
                }
              }
            }
            if (this.MPStatus.Media_IsVideo & !this.MPStatus.Media_IsDVD)
            {
              num5 |= 8;
              num5 |= 0x10;
              num4 = 4;
              if (this.DoDebug)
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_MOVIE");
              }
              icon.On();
              icon.InvertOn();
              if (this.MPStatus.MediaPlayer_Playing)
              {
                if (this.DisplaySettings.BlankDisplayWithVideo)
                {
                  this.DisplayOff();
                }
                if (this.MPStatus.Media_Speed > 0)
                {
                  icon.RotateCW();
                }
                else if (this.MPStatus.Media_Speed < 0)
                {
                  icon.RotateCCW();
                }
                icon.FlashOff();
              }
              else
              {
                this.RestoreDisplayFromVideoOrIdle();
                icon.FlashOn();
                num4 = 6;
                if (this.DoDebug)
                {
                  Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting PAUSED icon");
                }
              }
              if (!_useDiskIconForAllMedia)
              {
                icon.Off();
              }
              property = GUIPropertyManager.GetProperty("#Play.Current.File");
              Log.Debug("current file: {0}", new object[] {property});
              if (property.Length > 0)
              {
                string str3;
                num5 |= 0x10;
                strArray = property.Split(new char[] {'.'});
                if ((strArray.Length > 1) && ((str3 = strArray[1].ToLower()) != null))
                {
                  if ((!(str3 == "ifo") && !(str3 == "vob")) && !(str3 == "mpg"))
                  {
                    if (str3 == "wmv")
                    {
                      num5 |= 0x400;
                      if (this.DoDebug)
                      {
                        Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_WMV");
                      }
                    }
                    else if (str3 == "divx")
                    {
                      num5 |= 0x20;
                      if (this.DoDebug)
                      {
                        Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_DIVX");
                      }
                    }
                    else if (str3 == "xvid")
                    {
                      num5 |= 0x800;
                      if (this.DoDebug)
                      {
                        Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_XVID");
                      }
                    }
                  }
                  else
                  {
                    num5 |= 0x40;
                    if (this.DoDebug)
                    {
                      Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_MPG");
                    }
                  }
                }
              }
            }
          }
          if (!this.MPStatus.MediaPlayer_Playing || !_useDiskIconForAllMedia)
          {
            if (!this.MPStatus.MediaPlayer_Playing)
            {
              this.RestoreDisplayFromVideoOrIdle();
            }
            segments = 0;
            if (drive.IsOpened)
            {
              if (drive.IsCDReady())
              {
                if (!flag2)
                {
                  flag2 = drive.Refresh();
                }
                if (drive.GetNumAudioTracks() > 0)
                {
                  segments = this.ICON_CDIn;
                }
                else
                {
                  segments = this.ICON_DVDIn;
                }
              }
              else
              {
                flag2 = false;
              }
            }
          }
          if (!this.MPStatus.MediaPlayer_Active)
          {
            this.RestoreDisplayFromVideoOrIdle();
            if (_mpIsIdle)
            {
              num5 |= 0x1000;
              num4 = 0;
              if (this.DoDebug)
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Setting ICON_IDLE");
              }
            }
          }
          if (this.DoDebug)
          {
            Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Status collected...");
          }
          if (num4 != num3)
          {
            num2 = num4;
          }
          this.GetEQ();
          this.DisplayEQ();
          lock (this.DWriteMutex)
          {
            if (this.DoDebug)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Building display image");
            }
            if ((_useProgressDisplay || _useVolumeDisplay) || _useIcons)
            {
              this.ClearIconArea();
            }
            if (_useProgressDisplay || _useVolumeDisplay)
            {
              this.DrawProgressBars();
            }
            if (_useIcons)
            {
              if (!this.MPStatus.MediaPlayer_Playing || !_useDiskIconForAllMedia)
              {
                this.DrawDiskIcon(segments);
              }
              else
              {
                this.DrawDiskIcon(icon.Mask);
              }
              this.DrawIcons(num5);
              this.DrawLargeIcons();
            }
            if (this.DoDebug)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() image built - sending to display");
            }
            this.GX_Session.SendAsyncFrame(this.GX_Surface);
          }
          if (!this.EQSettings._EqDataAvailable)
          {
            if (this.DoDebug)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Sleeping...");
            }
            Thread.Sleep(250);
            if (this.DoDebug)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Waking...");
            }
          }
        }
      }

      private void DrawDiskIcon(uint segments)
      {
        if (_useIcons && !this._isClosing)
        {
          Log.Debug("MatrixGX.MOGXDisplay.DrawDiskIcon() - called");
          try
          {
            for (int i = 0; i < 8; i++)
            {
              if ((segments & (((int)1) << i)) > 0L)
              {
                this.GX_Graphics.FillPie(Brushes.Black, 0xdf, 0, 0x10, 0x10, i * 0x2d, 0x2d);
              }
            }
          }
          catch (Exception exception)
          {
            Log.Debug("MatrixGX.MOGXDisplay.DrawDiskIcon() - caught exception {0}", new object[] {exception.ToString()});
          }
          Log.Debug("MatrixGX.MOGXDisplay.DrawDiskIcon() - completed");
        }
      }

      private void DrawIcons(uint ICON_STATUS)
      {
        if (_useIcons && !this._isClosing)
        {
          Log.Debug("MatrixGX.MOGXDisplay.DrawIcons() - not yet implimented");
        }
      }

      private void DrawLargeIcons()
      {
        if (_useIcons && !this._isClosing)
        {
          Log.Debug("MatrixGX.MOGXDisplay.DrawLargeIcons() - called");
          Log.Debug("MatrixGX.MOGXDisplay.DrawLargeIcons() - completed");
        }
      }

      private void DrawProgressBars()
      {
        if ((!(!_useProgressDisplay & !_useVolumeDisplay) && !this._isClosing) &&
            (!this.DisplaySettings.BlankDisplayWithVideo || !this._IsDisplayOff))
        {
          Log.Debug("MatrixGX.MOGXDisplay.DrawProgressBars() - called");
          MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
          int num = ((int)this.GetTextBounds().Width) - 1;
          if ((this.MPStatus.MediaPlayer_Playing & _useVolumeDisplay) && !this.MPStatus.IsMuted)
          {
            float num2 = (0xffff / num) - 6;
            int width = ((int)((((float)this.MPStatus.SystemVolumeLevel) / num2) - 0.01)) + 1;
            this.GX_Graphics.FillRectangle(Brushes.Black, 0, 0, 3, 5);
            this.GX_Graphics.FillRectangle(Brushes.Black, num - 2, 0, 3, 5);
            this.GX_Graphics.DrawLine(Pens.Black, 0, 2, num, 2);
            this.GX_Graphics.FillRectangle(Brushes.Black, 3, 0, width, 5);
          }
          if (this.MPStatus.MediaPlayer_Playing & _useProgressDisplay)
          {
            int num4 =
              ((int)
               (((((float)this.MPStatus.Media_CurrentPosition) / ((float)this.MPStatus.Media_Duration)) - 0.01) *
                (num - 6))) + 1;
            this.GX_Graphics.FillRectangle(Brushes.Black, 0, 0x3b, 3, 5);
            this.GX_Graphics.FillRectangle(Brushes.Black, num - 2, 0x3b, 3, 5);
            this.GX_Graphics.DrawLine(Pens.Black, 0, 0x3d, num, 0x3d);
            this.GX_Graphics.FillRectangle(Brushes.Black, 3, 0x3b, num4, 5);
          }
          Log.Debug("MatrixGX.MOGXDisplay.DrawProgressBars() - completed");
        }
      }

      private void GetEQ()
      {
        lock (this.DWriteMutex)
        {
          this.EQSettings._EqDataAvailable = MiniDisplayHelper.GetEQ(ref this.EQSettings);
          if (this.EQSettings._EqDataAvailable)
          {
            this._displayThread.Priority = ThreadPriority.AboveNormal;
          }
          else
          {
            this._displayThread.Priority = ThreadPriority.BelowNormal;
          }
        }
      }

      private RectangleF GetTextBounds()
      {
        RectangleF bounds;
        lock (this.DWriteMutex)
        {
          GraphicsUnit pixel = GraphicsUnit.Pixel;
          bounds = this.GX_Surface.GetBounds(ref pixel);
          if (_useVolumeDisplay)
          {
            bounds.Offset(0f, 8f);
            bounds.Height -= 8f;
          }
          if (_useProgressDisplay)
          {
            bounds.Height -= 8f;
          }
          if (_useIcons)
          {
            bounds.Width -= 32f;
          }
        }
        return bounds;
      }

      private void OnExternalAction(Action action)
      {
        if (this.DisplaySettings.EnableDisplayAction)
        {
          if (this.DoDebug)
          {
            Log.Info("MatrixGX.MOGXDisplay.OnExternalAction(): received action {0}",
                     new object[] {action.wID.ToString()});
          }
          Action.ActionType wID = action.wID;
          if (wID <= Action.ActionType.ACTION_SHOW_OSD)
          {
            if ((wID != Action.ActionType.ACTION_SHOW_INFO) && (wID != Action.ActionType.ACTION_SHOW_OSD))
            {
              return;
            }
          }
          else if (((wID != Action.ActionType.ACTION_SHOW_MPLAYER_OSD) && (wID != Action.ActionType.ACTION_KEY_PRESSED)) &&
                   (wID != Action.ActionType.ACTION_MOUSE_CLICK))
          {
            return;
          }
          this.DisplaySettings._DisplayControlAction = true;
          this.DisplaySettings._DisplayControlLastAction = DateTime.Now.Ticks;
          if (this.DoDebug)
          {
            Log.Info("MatrixGX.MOGXDisplay.OnExternalAction(): received DisplayControlAction");
          }
          this.DisplayOn();
        }
      }

      public bool OpenDisplay(AdvancedSettings UseSettings)
      {
        Log.Info("MatrixGX.MOGXDisplay.OpenDisplay() - called");
        this.AdvSettings = UseSettings;
        MiniDisplayHelper.InitEQ(ref this.EQSettings);
        MiniDisplayHelper.InitDisplayControl(ref this.DisplaySettings);
        this.ParseAdvancedSettings();
        try
        {
          using (Profile.Settings settings = new Profile.MPSettings())
          {
            this._AudioUseASIO = settings.GetValueAsBool("audioplayer", "asio", false);
          }
          this.GX_Client = new DCClient();
          if (!this.GX_Client.Connect())
          {
            Log.Info("MatrixGX.MOGXDisplay.OpenDisplay() - Could not connect to DriverCore service");
            this._isOpen = false;
          }
          else
          {
            Log.Info("MatrixGX.MOGXDisplay.OpenDisplay() - Connect to the DriverCore service");
            this.GX_Devices = this.GX_Client.Devices;
            if (this.GX_Devices.Count() > 0)
            {
              Log.Info("MatrixGX.MOGXDisplay.OpenDisplay() - Found a GX series device");
              this.GX_Session = this.GX_Devices[0].CreateSession("MediaPortal");
              this.GX_Session.CreateGraphics(out this.GX_Surface);
              this.GX_Graphics = Graphics.FromImage(this.GX_Surface);
              this._isOpen = true;
              this._isClosing = false;
              Log.Info("MatrixGX.MOGXDisplay.OpenDisplay() - Display Opened");
              this.BacklightOn();
              this.GX_Session.SetOptions(true, false);
              if (this.MOGX_Control.InvertDisplay)
              {
                this.GX_Session.SetOptions(true, true);
              }
              this.stopDisplayUpdateThread = false;
              this._displayThread = new Thread(new ThreadStart(this.DisplayUpdate));
              this._displayThread.IsBackground = true;
              this._displayThread.Priority = ThreadPriority.BelowNormal;
              this._displayThread.Name = "DisplayUpdateThread";
              this._displayThread.Start();
              if (this._displayThread.IsAlive)
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() Thread Started");
              }
              else
              {
                Log.Info("MatrixGX.MOGXDisplay.DisplayUpdate() FAILED TO START");
                this.CloseDisplay();
              }
            }
            else
            {
              Log.Info("MatrixGX.MOGXDisplay.OpenDisplay() - No GX Series Display found");
              this._isOpen = false;
            }
          }
        }
        catch (Exception exception)
        {
          Log.Info("MatrixGX.MOGXDisplay.OpenDisplay() - Display not opened - caught exception {0}",
                   new object[] {exception.ToString()});
          Log.Error(exception);
          this._isOpen = false;
        }
        Log.Info("MatrixGX.MOGXDisplay.OpenDisplay() - Completed");
        return this._isOpen;
      }

      private void ParseAdvancedSettings()
      {
        Log.Info("MatrixGX.ParseAdvancedSettings(): Called");
        this.DoDebug = Settings.Instance.ExtensiveLogging;
        _useVolumeDisplay = this.AdvSettings.VolumeDisplay;
        _useProgressDisplay = this.AdvSettings.ProgressDisplay;
        _useDiskIconForAllMedia = this.AdvSettings.UseDiskIconForAllMedia;
        _useIcons = this.AdvSettings.UseIcons;
        this.MOGX_Control.InvertDisplay = this.AdvSettings.UseInvertedDisplay;
        this.MOGX_Control.BackLightRed = this.AdvSettings.BacklightRED;
        this.MOGX_Control.BackLightGreen = this.AdvSettings.BacklightGREEN;
        this.MOGX_Control.BackLightBlue = this.AdvSettings.BacklightBLUE;
        this.EQSettings.UseEqDisplay = this.AdvSettings.EqDisplay;
        this.EQSettings.UseNormalEq = this.AdvSettings.NormalEQ;
        this.EQSettings.UseStereoEq = this.AdvSettings.StereoEQ;
        this.EQSettings.UseVUmeter = this.AdvSettings.VUmeter;
        this.EQSettings.UseVUmeter2 = this.AdvSettings.VUmeter2;
        this.EQSettings._useVUindicators = this.AdvSettings.VUindicators;
        this.EQSettings.RestrictEQ = this.AdvSettings.RestrictEQ;
        this.EQSettings._EQ_Restrict_FPS = this.AdvSettings.EqRate;
        this.EQSettings.DelayEQ = this.AdvSettings.DelayEQ;
        this.EQSettings._DelayEQTime = this.AdvSettings.DelayEqTime;
        this.EQSettings.SmoothEQ = this.AdvSettings.SmoothEQ;
        this.EQSettings.EQTitleDisplay = this.AdvSettings.EQTitleDisplay;
        this.EQSettings._EQTitleDisplayTime = this.AdvSettings.EQTitleDisplayTime;
        this.EQSettings._EQTitleShowTime = this.AdvSettings.EQTitleShowTime;
        this.EQSettings._EqUpdateDelay = (this.EQSettings._EQ_Restrict_FPS == 0)
                                           ? 0
                                           : ((0x989680 / this.EQSettings._EQ_Restrict_FPS) -
                                              (0xf4240 / this.EQSettings._EQ_Restrict_FPS));
        this.DisplaySettings.BlankDisplayWithVideo = this.AdvSettings.BlankDisplayWithVideo;
        this.DisplaySettings.EnableDisplayAction = this.AdvSettings.EnableDisplayAction;
        this.DisplaySettings.DisplayActionTime = this.AdvSettings.EnableDisplayActionTime;
        this.DisplaySettings._DisplayControlTimeout = this.DisplaySettings.DisplayActionTime * 0x989680;
        this.DisplaySettings.BlankDisplayWhenIdle = this.AdvSettings.BlankDisplayWhenIdle;
        this.DisplaySettings.BlankIdleDelay = this.AdvSettings.BlankIdleTime;
        this.DisplaySettings._BlankIdleTimeout = this.DisplaySettings.BlankIdleDelay * 0x989680;
        Log.Info("MatrixGX.ParseAdvancedSettings(): Logging Options - Extensive Logging = {0}",
                 new object[] {this.DoDebug});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced Options - Volume Display = {0}",
                 new object[] {_useVolumeDisplay});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced Options - Progress Display = {0}",
                 new object[] {_useProgressDisplay});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced Options - Invert Display = {0}",
                 new object[] {this.MOGX_Control.InvertDisplay});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced Options - Backlight Color:");
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced Options -   Red = {0}",
                 new object[] {this.MOGX_Control.BackLightRed});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced Options -   Green = {0}",
                 new object[] {this.MOGX_Control.BackLightGreen});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced Options -   Blue = {0}",
                 new object[] {this.MOGX_Control.BackLightBlue});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced Options - Use Icons: {0}", new object[] {_useIcons});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced Options - Use Disk Icon for all media: {0}",
                 new object[] {_useDiskIconForAllMedia});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options - Equalizer Display: {0}",
                 new object[] {this.EQSettings.UseEqDisplay});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -   Normal Equalizer Display: {0}",
                 new object[] {this.EQSettings.UseNormalEq});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -   Stereo Equalizer Display: {0}",
                 new object[] {this.EQSettings.UseStereoEq});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -   VU Meter Display: {0}",
                 new object[] {this.EQSettings.UseVUmeter});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -   VU Meter Style 2 Display: {0}",
                 new object[] {this.EQSettings.UseVUmeter2});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -     Use VU Channel indicators: {0}",
                 new object[] {this.EQSettings._useVUindicators});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -   Restrict EQ Update Rate: {0}",
                 new object[] {this.EQSettings.RestrictEQ});
        Log.Info(
          "MatrixGX.ParseAdvancedSettings(): Advanced options -     Restricted EQ Update Rate: {0} updates per second",
          new object[] {this.EQSettings._EQ_Restrict_FPS});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -   Delay EQ Startup: {0}",
                 new object[] {this.EQSettings.DelayEQ});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -     Delay EQ Startup Time: {0} seconds",
                 new object[] {this.EQSettings._DelayEQTime});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -   Smooth EQ Amplitude Decay: {0}",
                 new object[] {this.EQSettings.SmoothEQ});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -   Show Track Info with EQ display: {0}",
                 new object[] {this.EQSettings.EQTitleDisplay});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -     Show Track Info Interval: {0} seconds",
                 new object[] {this.EQSettings._EQTitleDisplayTime});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -     Show Track Info duration: {0} seconds",
                 new object[] {this.EQSettings._EQTitleShowTime});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options - Blank display with video: {0}",
                 new object[] {this.DisplaySettings.BlankDisplayWithVideo});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -   Enable Display on Action: {0}",
                 new object[] {this.DisplaySettings.EnableDisplayAction});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -     Enable display for: {0} seconds",
                 new object[] {this.DisplaySettings._DisplayControlTimeout});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options - Blank display when idle: {0}",
                 new object[] {this.DisplaySettings.BlankDisplayWhenIdle});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Advanced options -     blank display after: {0} seconds",
                 new object[] {this.DisplaySettings._BlankIdleTimeout / 0xf4240L});
        Log.Info("MatrixGX.ParseAdvancedSettings(): Completed");
      }

      private void RestoreDisplayFromVideoOrIdle()
      {
        if (this.DisplaySettings.BlankDisplayWithVideo)
        {
          if (this.DisplaySettings.BlankDisplayWhenIdle)
          {
            if (!_mpIsIdle)
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

      public void SendImage(Bitmap _Bitmap)
      {
        if (!this._isClosing)
        {
          this.CheckIdleState();
          if (!(this.EQSettings._EqDataAvailable | this._IsDisplayOff))
          {
            lock (this.DWriteMutex)
            {
              Log.Debug("MatrixGX.MOGXDisplay.SendImage() - called");
              if ((_Bitmap.Height == this.GX_Surface.Height) & (_Bitmap.Width == this.GX_Surface.Width))
              {
                this.GX_Surface = (Bitmap)_Bitmap.Clone();
              }
              else
              {
                int height = Math.Min(_Bitmap.Height, this.GX_Surface.Height);
                int width = Math.Min(_Bitmap.Width, this.GX_Surface.Width);
                this.GX_Surface = _Bitmap.Clone(new Rectangle(0, 0, width, height), PixelFormat.Format1bppIndexed);
              }
            }
            Log.Debug("MatrixGX.MOGXDisplay.SendImage() - completed");
          }
        }
      }

      public void SendText(string _line1, string _line2)
      {
        this.CheckIdleState();
        if (this.EQSettings._EqDataAvailable || this._IsDisplayOff)
        {
          Log.Info(
            "MatrixGX.MOGXDisplay.SendText(): Suppressing display update! (EqDataAvailable = {0}, _IsDisplayOff = {1})",
            new object[] {this.EQSettings._EqDataAvailable, this._IsDisplayOff});
        }
        else if (this._isClosing)
        {
          Log.Info("MatrixGX.MOGXDisplay.SendText(): Suppressing display update! Driver is closing");
        }
        else
        {
          RectangleF textBounds = this.GetTextBounds();
          lock (this.DWriteMutex)
          {
            Log.Info("MatrixGX.MOGXDisplay.SendText() - called");
            this.GX_Graphics = Graphics.FromImage(this.GX_Surface);
            this.GX_Graphics.SmoothingMode = SmoothingMode.None;
            this.GX_Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            this.GX_Graphics.Clear(Color.White);
            Font font = new Font(Settings.Instance.Font, (float)Settings.Instance.FontSize);
            int length = _line1.Length;
            while (this.GX_Graphics.MeasureString(_line1.Substring(0, length), font).Width > textBounds.Width)
            {
              length--;
            }
            this.GX_Graphics.DrawString(_line1.Substring(0, length), font, Brushes.Black, textBounds);
            textBounds.Offset(0f, font.GetHeight() + 1f);
            textBounds.Height -= font.GetHeight() + 1f;
            length = _line2.Length;
            while (this.GX_Graphics.MeasureString(_line2.Substring(0, length), font).Width > textBounds.Width)
            {
              length--;
            }
            this.GX_Graphics.DrawString(_line2.Substring(0, length), font, Brushes.Black, textBounds);
          }
          Log.Info("MatrixGX.MOGXDisplay.SendText() - completed");
        }
      }

      public void SetContrast()
      {
        if (!this._isOpen) {}
      }

      public bool IsOpen
      {
        get { return this._isOpen; }
      }

      public class DiskIcon
      {
        private bool _diskFlash;
        private bool _diskInverted;
        private readonly uint[] _DiskMask = new uint[] {0xfe, 0xfd, 0xfb, 0xf7, 0xef, 0xdf, 0xbf, 0x7f};
        private readonly uint[] _DiskMaskInv = new uint[] {1, 2, 4, 8, 0x10, 0x20, 0x40, 0x80};
        private bool _diskOn;
        private bool _diskRotate;
        private bool _diskRotateClockwise = true;
        private int _diskSegment;
        private readonly uint _diskSolidOffMask = 0;
        private readonly uint _diskSolidOnMask = 0xff;
        private bool _diskSRWFlash = true;
        private int _flashState = 1;
        private DateTime _LastAnimate;

        public void Animate()
        {
          if ((DateTime.Now.Ticks - this._LastAnimate.Ticks) >= 0x4c4b40L)
          {
            if ((this._diskRotate & !this._diskFlash) || (this._diskRotate & (this._diskFlash & !this._diskSRWFlash)))
            {
              if (this._diskRotateClockwise)
              {
                this._diskSegment++;
                if (this._diskSegment > 7)
                {
                  this._diskSegment = 0;
                }
              }
              else
              {
                this._diskSegment--;
                if (this._diskSegment < 0)
                {
                  this._diskSegment = 7;
                }
              }
            }
            if (this._diskFlash)
            {
              if (this._flashState == 1)
              {
                this._flashState = 0;
              }
              else
              {
                this._flashState = 1;
              }
            }
            this._LastAnimate = DateTime.Now;
          }
        }

        public void FlashOff()
        {
          this._diskFlash = false;
          this._flashState = 1;
        }

        public void FlashOn()
        {
          this._diskFlash = true;
        }

        public void InvertOff()
        {
          this._diskInverted = false;
        }

        public void InvertOn()
        {
          this._diskInverted = true;
        }

        public void Off()
        {
          this._diskOn = false;
        }

        public void On()
        {
          this._diskOn = true;
        }

        public void Reset()
        {
          this._diskFlash = false;
          this._diskRotate = false;
          this._diskSegment = 0;
          this._diskRotateClockwise = true;
          this._diskOn = false;
          this._flashState = 1;
          this._diskInverted = false;
          this._diskSRWFlash = true;
        }

        public void RotateCCW()
        {
          this._diskRotateClockwise = false;
          this._diskRotate = true;
        }

        public void RotateCW()
        {
          this._diskRotateClockwise = true;
          this._diskRotate = true;
        }

        public uint Mask
        {
          get
          {
            if (!this._diskOn)
            {
              return this._diskSolidOffMask;
            }
            if (!this._diskRotate)
            {
              if (!this._diskFlash)
              {
                return this._diskSolidOnMask;
              }
              if (this._flashState == 1)
              {
                return this._diskSolidOnMask;
              }
              return this._diskSolidOffMask;
            }
            if (!this._diskFlash)
            {
              if (!this._diskInverted)
              {
                return this._DiskMask[this._diskSegment];
              }
              return this._DiskMaskInv[this._diskSegment];
            }
            if (this._flashState <= 0)
            {
              return this._diskSolidOffMask;
            }
            if (!this._diskInverted)
            {
              return this._DiskMask[this._diskSegment];
            }
            return this._DiskMaskInv[this._diskSegment];
          }
        }
      }

      private enum IconType : uint
      {
        ICON_CD = 4,
        ICON_DivX = 0x20,
        ICON_DVD = 2,
        ICON_Movie = 8,
        ICON_MP3 = 0x100,
        ICON_MPG = 0x40,
        ICON_Music = 0x10,
        ICON_OGG = 0x80,
        ICON_Rec = 0x2000,
        ICON_Time = 0x1000,
        ICON_TV = 1,
        ICON_WAV = 0x4000,
        ICON_WMA = 0x200,
        ICON_WMV = 0x400,
        ICON_xVid = 0x800
      }

      private enum LargeIconType
      {
        IDLE,
        TV,
        MOVIE,
        MUSIC,
        VIDEO,
        RECORDING,
        PAUSED
      }
    }
  }
}