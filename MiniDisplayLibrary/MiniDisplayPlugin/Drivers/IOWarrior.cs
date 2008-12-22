using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class IOWarrior : BaseDisplay, IDisplay, IDisposable
  {
    private string _errorMessage = "";
    private int _Gcols;
    private int _Grows;
    public bool _IsConfiguring;
    private bool _isDisabled;
    private bool _IsOpen;
    private readonly string[] _Lines = new string[2];
    private AdvancedSettings AdvSettings;
    private bool DoDebug;
    private readonly IOWDisplay iowcommDisplay = new IOWDisplay();
    private byte[] lastHash;
    private DateTime LastSettingsCheck = DateTime.Now;
    private byte[] ReceivedBitmapData;
    private DateTime SettingsLastModTime;
    private readonly SHA256Managed sha256 = new SHA256Managed();

    private void AdvancedSettings_OnSettingsChanged()
    {
      Log.Info("IOWarrior.AdvancedSettings_OnSettingsChanged(): called", new object[0]);
      this.CleanUp();
      this.LoadAdvancedSettings();
      Thread.Sleep(100);
      this.Setup(MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.Port, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.TextHeight, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.TextWidth, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.TextComDelay, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.GraphicHeight, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.GraphicWidth, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.GraphicComDelay, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.BackLightControl, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.Backlight, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.ContrastControl, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.Contrast, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.BlankOnExit);
      this.Initialize();
    }

    public void CleanUp()
    {
      if (this.iowcommDisplay.IsOpen)
      {
        Log.Debug("IOWarrior.CleanUp() - called", new object[0]);
        this.iowcommDisplay.ClearDisplay();
        this.iowcommDisplay.CloseDisplay();
        AdvancedSettings.OnSettingsChanged -= new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
      }
    }

    private void Clear()
    {
      if (this.iowcommDisplay.IsOpen)
      {
        Log.Debug("IOWarrior.Clear() - called", new object[0]);
        this.iowcommDisplay.ClearDisplay();
      }
    }

    public void Configure()
    {
      new IOWarrior_AdvancedSetupForm().ShowDialog();
    }

    public void Dispose()
    {
      Log.Debug("IOWarrior.Dispose() - called", new object[0]);
    }

    public void DrawImage(Bitmap bitmap)
    {
      if (!this._isDisabled && this.iowcommDisplay.IsOpen)
      {
        Log.Debug("IOWarrior.DrawImage() - called", new object[0]);
        if (bitmap == null)
        {
          Log.Debug("IOWarrior.DrawImage():  bitmap null", new object[0]);
        }
        else
        {
          BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
          try
          {
            if (this.ReceivedBitmapData == null)
            {
              this.ReceivedBitmapData = new byte[bitmapdata.Stride * this._Grows];
            }
            Marshal.Copy(bitmapdata.Scan0, this.ReceivedBitmapData, 0, this.ReceivedBitmapData.Length);
          } catch (Exception exception)
          {
            Log.Debug("IOWarrior.DrawImage(): caught exception - {0}", new object[] { exception.ToString() });
          }
          finally
          {
            bitmap.UnlockBits(bitmapdata);
          }
          byte[] buffer = this.sha256.ComputeHash(this.ReceivedBitmapData);
          if (ByteArray.AreEqual(buffer, this.lastHash))
          {
            Log.Debug("IOWarrior.DrawImage() - completed - bitmap not changed", new object[0]);
          }
          else
          {
            this.lastHash = buffer;
            this.iowcommDisplay.SendImage(bitmap);
            this.UpdateAdvancedSettings();
            Log.Debug("IOWarrior.DrawImage() - completed", new object[0]);
          }
        }
      }
    }

    public void Initialize()
    {
      if (!this._isDisabled)
      {
        if (this.iowcommDisplay.IsOpen)
        {
          this.iowcommDisplay.CloseDisplay();
        }
        this._IsOpen = this.iowcommDisplay.OpenDisplay(this.AdvSettings);
        if (this._IsOpen)
        {
          Log.Debug("IOWarrior.Initialize() - Display opened.", new object[0]);
          this._isDisabled = false;
          AdvancedSettings.OnSettingsChanged += new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
        }
        else
        {
          Log.Debug("IOWarrior.Initialize() - Unable to open device - display disabled", new object[0]);
          this._isDisabled = true;
          this._errorMessage = "IOWarrior.Initialize() failed... No IOWarrior display found";
        }
        Log.Debug("IOWarrior.Initialize() - completed", new object[0]);
        this.Clear();
      }
    }

    private void LoadAdvancedSettings()
    {
      this.AdvSettings = AdvancedSettings.Load();
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml"));
      this.SettingsLastModTime = info.LastWriteTime;
      this.LastSettingsCheck = DateTime.Now;
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
    }

    public void SetLine(int line, string message)
    {
    }

    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG, bool _backLight, int _backLightLevel, bool _contrast, int _contrastLevel, bool _blankOnExit)
    {
      this._IsConfiguring = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
      this.DoDebug = this._IsConfiguring | MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.ExtensiveLogging;
      Log.Info("IOWarrior.Setup() - called", new object[0]);
      Log.Info("IOWarrior.Setup(): IOWarrior Graphical LCD Driver - {0}", new object[] { this.Description });
      Log.Info("IOWarrior.Setup(): Called by \"{0}\".", new object[] { Assembly.GetEntryAssembly().FullName });
      FileInfo info = new FileInfo(Assembly.GetExecutingAssembly().Location);
      if (this.DoDebug)
      {
        Log.Info("IOWarrior: Assembly creation time: {0} ( {1} UTC )", new object[] { info.LastWriteTime, info.LastWriteTimeUtc.ToUniversalTime() });
      }
      if (this.DoDebug)
      {
        Log.Info("IOWarrior: Platform: {0}", new object[] { Environment.OSVersion.VersionString });
      }
      try
      {
        this.LoadAdvancedSettings();
        AdvancedSettings.OnSettingsChanged += new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
        this._Grows = _linesG;
        this._Gcols = _colsG;
        if (this._Gcols > 0x80)
        {
          Log.Info("IOWarrior.Setup() - Invalid Graphics Columns value", new object[0]);
          this._Grows = 0x80;
        }
        if (this._Grows > 0x40)
        {
          Log.Info("IOWarrior.Setup() - Invalid Graphics Lines value", new object[0]);
          this._Grows = 0x40;
        }
      } catch (Exception exception)
      {
        Log.Debug("IOWarrior.Setup() - threw an exception: {0}", new object[] { exception.ToString() });
        this._isDisabled = true;
        this._errorMessage = "IOWarrior.setup() failed... Did you copy the required files to the MediaPortal directory?";
      }
      Log.Info("IOWarrior.Setup() - completed", new object[0]);
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= this.LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (this.DoDebug)
        {
          Log.Info("IOWarrior.UpdateAdvancedSettings(): called", new object[0]);
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml")))
        {
          FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml"));
          if (info.LastWriteTime.Ticks > this.SettingsLastModTime.Ticks)
          {
            if (this.DoDebug)
            {
              Log.Info("IOWarrior.UpdateAdvancedSettings(): updating advanced settings", new object[0]);
            }
            this.LoadAdvancedSettings();
          }
        }
        if (this.DoDebug)
        {
          Log.Info("IOWarrior.UpdateAdvancedSettings(): completed", new object[0]);
        }
      }
    }

    public string Description
    {
      get
      {
        return "IOWarrior Series Graphic LCD driver v04_17_2008";
      }
    }

    public string ErrorMessage
    {
      get
      {
        return this._errorMessage;
      }
    }

    public bool IsDisabled
    {
      get
      {
        this._isDisabled = false;
        int num = 0;
        if (!File.Exists("iowkit.dll"))
        {
          num++;
        }
        if ((num > 0) && ((num & 1) > 0))
        {
          this._errorMessage = "Required file \"iowkit.dll\" is not installed!\n";
          this._isDisabled = true;
          Log.Info("MatrixGX.IsDisabled() - Required file \"iowkit.dll\" is not installed!", new object[0]);
        }
        return this._isDisabled;
      }
    }

    public string Name
    {
      get
      {
        return "IOWarrior";
      }
    }

    public bool SupportsGraphics
    {
      get
      {
        return true;
      }
    }

    public bool SupportsText
    {
      get
      {
        return false;
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
      private bool m_EnableDisplayAction;
      private int m_EnableDisplayActionTime = 5;
      private bool m_EqDisplay;
      private int m_EqRate = 10;
      private bool m_EQTitleDisplay;
      private int m_EQTitleDisplayTime = 10;
      private int m_EQTitleShowTime = 2;
      private static IOWarrior.AdvancedSettings m_Instance;
      private bool m_NormalEQ = true;
      private bool m_RestrictEQ;
      private bool m_SmoothEQ;
      private bool m_StereoEQ;
      private bool m_VUindicators;
      private bool m_VUmeter;
      private bool m_VUmeter2;

      public static event OnSettingsChangedHandler OnSettingsChanged;

      private static void Default(IOWarrior.AdvancedSettings _settings)
      {
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

      public static IOWarrior.AdvancedSettings Load()
      {
        IOWarrior.AdvancedSettings settings;
        Log.Debug("IOWarrior.AdvancedSettings.Load() started", new object[0]);
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml")))
        {
          Log.Debug("IOWarrior.AdvancedSettings.Load() Loading settings from XML file", new object[0]);
          XmlSerializer serializer = new XmlSerializer(typeof(IOWarrior.AdvancedSettings));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml"));
          settings = (IOWarrior.AdvancedSettings)serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Debug("IOWarrior.AdvancedSettings.Load() Loading settings from defaults", new object[0]);
          settings = new IOWarrior.AdvancedSettings();
          Default(settings);
        }
        Log.Debug("IOWarrior.AdvancedSettings.Load() completed", new object[0]);
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

      public static void Save(IOWarrior.AdvancedSettings ToSave)
      {
        Log.Debug("IOWarrior.AdvancedSettings.Save() Saving settings to XML file", new object[0]);
        XmlSerializer serializer = new XmlSerializer(typeof(IOWarrior.AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml"), Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter)writer, ToSave);
        writer.Close();
        Log.Debug("IOWarrior.AdvancedSettings.Save() completed", new object[0]);
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

      public static IOWarrior.AdvancedSettings Instance
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

    private class IOWDisplay
    {
      private bool _AudioUseASIO;
      private Thread _displayThread;
      private System.Type _IOWDLL;
      private bool _isClosing;
      private bool _IsDisplayOff;
      private bool _isOpen;
      public static bool _mpIsIdle;
      private IOWarrior.AdvancedSettings AdvSettings;
      private const BindingFlags BINDING_FLAGS = (BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);
      private BitmapConverter bitmaputils = new BitmapConverter();
      private byte[] byLocal = new byte[0x400];
      private byte[] byOldImage = new byte[0x400];
      private DisplayControl DisplaySettings;
      private bool DoDebug;
      private object DWriteMutex = new object();
      private EQControl EQSettings;
      private int iDispHeight = 0x40;
      private int iDispWidth = 0x80;
      private string IdleMessage = ((MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.IdleMessage != string.Empty) ? MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.IdleMessage : "MediaPortal");
      private string IOW_DLLFile;
      private Graphics IOW_Graphics;
      private int IOW_Handle;
      private Bitmap IOW_Surface;
      private byte[] m_Buffer;
      private const MethodAttributes METHOD_ATTRIBUTES = (MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public);
      private SystemStatus MPStatus = new SystemStatus();
      private static ModuleBuilder s_mb;
      private bool stopDisplayUpdateThread;
      private object StopMutex = new object();

      internal IOWDisplay()
      {
        if (this.DoDebug)
        {
          Log.Info("IOWarrior.iMONDisplay constructor: called", new object[0]);
        }
        if (this._IOWDLL == null)
        {
          this.CreateDLLWrapper();
        }
        IOWarrior.AdvancedSettings.Load();
        if (this.DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay constructor: completed", new object[0]);
        }
      }

      private void CheckIdleState()
      {
        if (this.MPStatus.MP_Is_Idle)
        {
          if (this.DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.DisplayLines(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle, this.DisplaySettings._BlankIdleTimeout });
          }
          if (this.DisplaySettings.BlankDisplayWhenIdle)
          {
            if (!_mpIsIdle)
            {
              if (this.DoDebug)
              {
                Log.Info("IOWarrior.IOWDisplay.DisplayLines(): MP going IDLE", new object[0]);
              }
              this.DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
            }
            if (!this._IsDisplayOff && ((DateTime.Now.Ticks - this.DisplaySettings._BlankIdleTime) > this.DisplaySettings._BlankIdleTimeout))
            {
              if (this.DoDebug)
              {
                Log.Info("IOWarrior.IOWDisplay.DisplayLines(): Blanking display due to IDLE state", new object[0]);
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
              Log.Info("IOWarrior.IOWDisplay.DisplayLines(): MP no longer IDLE - restoring display", new object[0]);
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
            Log.Debug("IOWarrior.IOWDisplay.ClearDisplay() - called", new object[0]);
            this.IOW_Graphics = Graphics.FromImage(this.IOW_Surface);
            this.IOW_Graphics.Clear(Color.White);
            Log.Debug("IOWarrior.IOWDisplay.ClearDisplay() - Sending blank image to device", new object[0]);
            this.IOW_ClearDisplay();
          }
          Log.Debug("IOWarrior.IOWDisplay.ClearDisplay() - completed", new object[0]);
        }
      }

      public void CloseDisplay()
      {
        Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() - called", new object[0]);
        try
        {
          lock (this.StopMutex)
          {
            Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() Stopping DisplayUpdate() Thread", new object[0]);
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
          Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() DisplayUpdate() Thread stopped.", new object[0]);
          this._isClosing = true;
          lock (this.DWriteMutex)
          {
            this.IOW_ClearDisplay();
            this.IOW_StopDisplay();
            this.IOWLib_IowKitCloseDevice(this.IOW_Handle);
            this.IOW_Graphics.Dispose();
            this.IOW_Handle = 0;
            this._isOpen = false;
          }
          Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() - Display closed.", new object[0]);
        } catch (Exception exception)
        {
          Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() - caught exception on display close: {0}", new object[] { exception.ToString() });
          Log.Error(exception);
          this._isOpen = false;
        }
      }

      private bool CreateDLLWrapper()
      {
        if (this.DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay.CreateDLLWrapper(): called", new object[0]);
        }
        new FileInfo(Assembly.GetEntryAssembly().Location);
        this.IOW_DLLFile = Config.GetFile(Config.Dir.Base, "iowkit.dll");
        if (this.DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay.CreateDLLWrapper(): using IOW DLL {1}", new object[] { this.IOW_DLLFile });
        }
        if (this.IOW_DLLFile == string.Empty)
        {
          if (this.DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.CreateDLLWrapper(): FAILED - iowkit.dll not found", new object[0]);
          }
          return false;
        }
        try
        {
          if (s_mb == null)
          {
            AssemblyName name = new AssemblyName();
            name.Name = "IOWDLLWrapper" + Guid.NewGuid().ToString("N");
            s_mb = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule("IOW_DLL_wrapper");
          }
          TypeBuilder builder2 = s_mb.DefineType("IOWDLLWrapper" + Guid.NewGuid().ToString("N"));
          MethodBuilder builder3 = builder2.DefinePInvokeMethod("IowKitOpenDevice", this.IOW_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(int), null, CallingConvention.StdCall, CharSet.Auto);
          builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
          builder3 = builder2.DefinePInvokeMethod("IowKitCloseDevice", this.IOW_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(void), new System.Type[] { typeof(int) }, CallingConvention.StdCall, CharSet.Auto);
          builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
          builder3 = builder2.DefinePInvokeMethod("IowKitWrite", this.IOW_DLLFile, MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(int), new System.Type[] { typeof(int), typeof(int), typeof(byte[]), typeof(int) }, CallingConvention.StdCall, CharSet.Auto);
          builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
          this._IOWDLL = builder2.CreateType();
        } catch (Exception exception)
        {
          Log.Error("IOWarrior.IOWDisplay.CreateImonDLLwrapper(): caught exception: {0}", new object[] { exception });
          return false;
        }
        if (this.DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay.CreateImonDLLWrapper(): Completed - RC DLL wrapper created.", new object[0]);
        }
        return true;
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
              Log.Info("MatrixGX.MOGXDisplay.DisplayEQ(): called", new object[0]);
            }
            this.EQSettings.Render_MaxValue = (this.EQSettings.UseNormalEq | this.EQSettings.UseStereoEq) ? ((int)textBounds.Height) : ((int)textBounds.Width);
            this.EQSettings.Render_BANDS = this.EQSettings.UseNormalEq ? 0x10 : (this.EQSettings.UseStereoEq ? 8 : 1);
            MiniDisplayHelper.ProcessEqData(ref this.EQSettings);
            Monitor.Enter(obj3 = this.DWriteMutex);
            try
            {
              this.IOW_Graphics.FillRectangle(Brushes.White, textBounds);
              for (int i = 0; i < this.EQSettings.Render_BANDS; i++)
              {
                RectangleF ef2;
                if (this.DoDebug)
                {
                  Log.Info("MatrixGX.MOGXDisplay.DisplayEQ(): Rendering {0} band {1} = {2}", new object[] { this.EQSettings.UseNormalEq ? "Normal EQ" : (this.EQSettings.UseStereoEq ? "Stereo EQ" : (this.EQSettings.UseVUmeter ? "VU Meter" : "VU Meter 2")), i, this.EQSettings.UseNormalEq ? this.EQSettings.EqArray[1 + i].ToString() : (this.EQSettings.UseStereoEq ? (this.EQSettings.EqArray[1 + i].ToString() + " : " + this.EQSettings.EqArray[9 + i].ToString()) : (this.EQSettings.EqArray[1 + i].ToString() + " : " + this.EQSettings.EqArray[2 + i].ToString())) });
                }
                if (this.EQSettings.UseNormalEq)
                {
                  ef2 = new RectangleF((textBounds.X + (i * (((int)textBounds.Width) / this.EQSettings.Render_BANDS))) + 1f, textBounds.Y + (((int)textBounds.Height) - this.EQSettings.EqArray[1 + i]), (float)((((int)textBounds.Width) / this.EQSettings.Render_BANDS) - 2), (float)this.EQSettings.EqArray[1 + i]);
                  this.IOW_Graphics.FillRectangle(Brushes.Black, ef2);
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
                    ef2 = new RectangleF((textBounds.X + num2) + 1f, textBounds.Y + (((int)textBounds.Height) - this.EQSettings.EqArray[1 + i]), (float)(num4 - 2), (float)this.EQSettings.EqArray[1 + i]);
                    ef3 = new RectangleF((textBounds.X + num3) + 1f, textBounds.Y + (((int)textBounds.Height) - this.EQSettings.EqArray[9 + i]), (float)(num4 - 2), (float)this.EQSettings.EqArray[9 + i]);
                    this.IOW_Graphics.FillRectangle(Brushes.Black, ef2);
                    this.IOW_Graphics.FillRectangle(Brushes.Black, ef3);
                  }
                  else if (this.EQSettings.UseVUmeter | this.EQSettings.UseVUmeter2)
                  {
                    ef2 = new RectangleF(textBounds.X + 1f, textBounds.Y + 1f, (float)this.EQSettings.EqArray[1 + i], (float)(((int)(textBounds.Height / 2f)) - 2));
                    num2 = this.EQSettings.UseVUmeter ? 0 : (((int)textBounds.Width) - this.EQSettings.EqArray[2 + i]);
                    ef3 = new RectangleF((textBounds.X + num2) + 1f, (textBounds.Y + (textBounds.Height / 2f)) + 1f, (float)this.EQSettings.EqArray[2 + i], (float)(((int)(textBounds.Height / 2f)) - 2));
                    this.IOW_Graphics.FillRectangle(Brushes.Black, ef2);
                    this.IOW_Graphics.FillRectangle(Brushes.Black, ef3);
                  }
                }
              }
            } catch (Exception exception)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayEQ(): CAUGHT EXCEPTION {0}", new object[] { exception });
              if (exception.Message.Contains("ThreadAbortException"))
              {
              }
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
            if ((DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction) < this.DisplaySettings._DisplayControlTimeout)
            {
              if (this.DoDebug)
              {
                Log.Info("IOWarrior.IOWDisplay.DisplayOff(): DisplayControlAction Timer = {0}.", new object[] { DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction });
              }
              return;
            }
            if (this.DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayOff(): DisplayControlAction Timeout expired.", new object[0]);
            }
            this.DisplaySettings._DisplayControlAction = false;
            this.DisplaySettings._DisplayControlLastAction = 0L;
          }
          Log.Info("IOWarrior.IOWDisplay.DisplayOff(): called", new object[0]);
          lock (this.DWriteMutex)
          {
            Log.Info("IOWarrior.IOWDisplay.DisplayOff(): Sending Display OFF command to LCD", new object[0]);
            this.IOW_BacklightOff();
            this.ClearDisplay();
            this._IsDisplayOff = true;
          }
          Log.Info("IOWarrior.IOWDisplay.DisplayOff(): completed", new object[0]);
        }
      }

      private void DisplayOn()
      {
        if (this._IsDisplayOff)
        {
          Log.Info("MatrixGX.MOGXDisplay.DisplayOn(): called", new object[0]);
          lock (this.DWriteMutex)
          {
            Log.Info("MatrixGX.MOGXDisplay.DisplayOn(): Sending Display ON command to LCD", new object[0]);
            this._IsDisplayOff = false;
            this.IOW_BacklightOn();
          }
          Log.Info("MatrixGX.MOGXDisplay.DisplayOn(): called", new object[0]);
        }
      }

      private void DisplayUpdate()
      {
        if (this.DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Starting Display Update Thread", new object[0]);
        }
        if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
        {
          GUIWindowManager.OnNewAction += new OnActionHandler(this.OnExternalAction);
        }
        while (true)
        {
          lock (this.StopMutex)
          {
            if (this.stopDisplayUpdateThread)
            {
              if (this.DoDebug)
              {
                Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Display Update Thread terminating", new object[0]);
              }
              if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
              {
                GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
              }
              return;
            }
          }
          MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
          if (this.DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Collecting status...", new object[0]);
          }
          if (this.MPStatus.MediaPlayer_Playing)
          {
            if ((this.MPStatus.Media_IsTV || this.MPStatus.Media_IsTVRecording) & (!this.MPStatus.Media_IsDVD && !this.MPStatus.Media_IsCD))
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
            }
            if (this.MPStatus.Media_IsDVD || this.MPStatus.Media_IsCD)
            {
              if (this.MPStatus.MediaPlayer_Playing)
              {
                if ((this.MPStatus.Media_IsDVD & this.MPStatus.Media_IsVideo) && this.DisplaySettings.BlankDisplayWithVideo)
                {
                  this.DisplayOff();
                }
              }
              else
              {
                this.RestoreDisplayFromVideoOrIdle();
              }
            }
            if (this.MPStatus.Media_IsVideo & !this.MPStatus.Media_IsDVD)
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
            }
          }
          if (!this.MPStatus.MediaPlayer_Playing && !this.MPStatus.MediaPlayer_Playing)
          {
            this.RestoreDisplayFromVideoOrIdle();
          }
          if (!this.MPStatus.MediaPlayer_Active)
          {
            this.RestoreDisplayFromVideoOrIdle();
          }
          if (this.DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Status collected...", new object[0]);
          }
          this.GetEQ();
          this.DisplayEQ();
          lock (this.DWriteMutex)
          {
            if (this.DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() image built - sending to display", new object[0]);
            }
            this.IOW_SendImage(this.IOW_Surface);
          }
          if (!this.EQSettings._EqDataAvailable)
          {
            if (this.DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Sleeping...", new object[0]);
            }
            Thread.Sleep(250);
            if (this.DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Waking...", new object[0]);
            }
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
        lock (this.DWriteMutex)
        {
          GraphicsUnit pixel = GraphicsUnit.Pixel;
          return this.IOW_Surface.GetBounds(ref pixel);
        }
      }

      public void IOW_BacklightOff()
      {
        this.IOW_SendBits(4, 0, 0, 0, 0, 0, 0, 0);
      }

      public void IOW_BacklightOn()
      {
        this.IOW_SendBits(4, 1, 0, 0, 0, 0, 0, 0);
      }

      public Bitmap IOW_Bitmap_ConvertBitmap(Bitmap img)
      {
        BitmapData bitmapdata = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
        Bitmap bitmap = new Bitmap(0x80, 0x40, PixelFormat.Format1bppIndexed);
        BitmapData bmd = bitmap.LockBits(new Rectangle(0, 0, 0x80, 0x40), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
        for (int i = 0; i < 0x40; i++)
        {
          for (int j = 0; j < 0x80; j++)
          {
            int ofs = (i * bitmapdata.Stride) + (j * 4);
            if (Marshal.ReadByte(bitmapdata.Scan0, ofs) != 0xff)
            {
              this.IOW_Bitmap_SetIndexedPixel(j, i, bmd, true);
            }
          }
        }
        bitmap.UnlockBits(bmd);
        img.UnlockBits(bitmapdata);
        return bitmap;
      }

      private void IOW_Bitmap_SetIndexedPixel(int x, int y, BitmapData bmd, bool pixel)
      {
        int ofs = (y * bmd.Stride) + (x >> 3);
        byte val = Marshal.ReadByte(bmd.Scan0, ofs);
        byte num3 = (byte)(((int)0x80) >> (x & 7));
        if (pixel)
        {
          val = (byte)(val | num3);
        }
        else
        {
          val = (byte)(val & ((byte)(num3 ^ 0xff)));
        }
        Marshal.WriteByte(bmd.Scan0, ofs, val);
      }

      public byte[] IOW_BitMap_ToByteArray(Bitmap bitmap)
      {
        BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        int length = bitmapdata.Stride * bitmap.Height;
        this.m_Buffer = new byte[length];
        Marshal.Copy(bitmapdata.Scan0, this.m_Buffer, 0, length);
        for (int i = 0; i < 8; i++)
        {
          for (int j = 0; j < 0x10; j++)
          {
            for (int k = 0; k < 8; k++)
            {
              for (int m = 0; m < 8; m++)
              {
                if (this.m_Buffer[((i * 0x80) + j) + (k * 0x10)] == 0)
                {
                  break;
                }
                if (this.m_Buffer[((k * 0x10) + j) + (i * 0x80)] >= Math.Pow(2.0, (double)(7 - m)))
                {
                  this.byLocal[(m + (j * 8)) + (i * 0x80)] = (byte)(this.byLocal[(m + (j * 8)) + (i * 0x80)] + Math.Pow(2.0, (double)k));
                  this.m_Buffer[((i * 0x80) + j) + (k * 0x10)] = (byte)(this.m_Buffer[((i * 0x80) + j) + (k * 0x10)] - Math.Pow(2.0, (double)(7 - m)));
                }
              }
            }
          }
        }
        bitmap.UnlockBits(bitmapdata);
        return this.byLocal;
      }

      public void IOW_ClearDisplay()
      {
        Bitmap bitmapRaw = new Bitmap(this.iDispWidth, this.iDispHeight, PixelFormat.Format32bppPArgb);
        this.IOW_SendImage(bitmapRaw);
      }

      private void IOW_SendBits(byte reportId, byte d1, byte d2, byte d3, byte d4, byte d5, byte d6, byte d7)
      {
        byte[] buffer = new byte[] { reportId, d1, d2, d3, d4, d5, d6, d7 };
        this.IOWLib_IowKitWrite(this.IOW_Handle, 1, ref buffer, buffer.Length);
      }

      private void IOW_SendIIC(byte byte1)
      {
        this.IOW_SendBits(2, 0xc2, 0x70, byte1, 0, 0, 0, 0);
      }

      public void IOW_SendImage(Bitmap bitmapRaw)
      {
        Bitmap bitmap = new Bitmap(this.iDispWidth, this.iDispHeight, PixelFormat.Format1bppIndexed);
        bitmap = this.IOW_Bitmap_ConvertBitmap(bitmapRaw);
        byte[] buffer = this.IOW_BitMap_ToByteArray(bitmap);
        this.IOW_SwitchPanel(0);
        int num = 0;
        for (int i = 0; i < 8; i++)
        {
          for (int j = 0; j < 2; j++)
          {
            for (int k = 0; k < 0x40; k++)
            {
              int num5 = 0;
              while (buffer[(((j * 0x40) + k) + num5) + (i * 0x80)] != this.byOldImage[(((j * 0x40) + k) + num5) + (i * 0x80)])
              {
                this.byOldImage[(((j * 0x40) + k) + num5) + (i * 0x80)] = buffer[(((j * 0x40) + k) + num5) + (i * 0x80)];
                num5++;
                if ((k + num5) >= 0x40)
                {
                  break;
                }
              }
              if (num5 > 0)
              {
                if (j != num)
                {
                  this.IOW_SwitchPanel(j);
                  num = j;
                }
                this.IOW_SendBits(5, 3, 0xc0, (byte)(0x40 + k), (byte)(0xb8 + i), 0, 0, 0);
                int num6 = 0;
                while (num5 >= 6)
                {
                  this.IOW_SendBits(5, 0x86, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 2) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 3) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 4) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 5) + (i * 0x80)]);
                  num5 -= 6;
                  num6 += 6;
                }
                switch (num5)
                {
                  case 1:
                    this.IOW_SendBits(5, 0x81, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)], 0, 0, 0, 0, 0);
                    break;

                  case 2:
                    this.IOW_SendBits(5, 130, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)], 0, 0, 0, 0);
                    break;

                  case 3:
                    this.IOW_SendBits(5, 0x83, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 2) + (i * 0x80)], 0, 0, 0);
                    break;

                  case 4:
                    this.IOW_SendBits(5, 0x84, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 2) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 3) + (i * 0x80)], 0, 0);
                    break;

                  case 5:
                    this.IOW_SendBits(5, 0x85, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 2) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 3) + (i * 0x80)], buffer[((((j * 0x40) + k) + num6) + 4) + (i * 0x80)], 0);
                    break;
                }
                k += (num5 + num6) - 1;
              }
            }
          }
        }
      }

      public void IOW_StartDisplay()
      {
        this.IOW_SendBits(4, 1, 0, 0, 0, 0, 0, 0);
        this.IOW_SendBits(5, 1, 0x3f, 0, 0, 0, 0, 0);
        this.IOW_SendBits(1, 1, 0, 0, 0, 0, 0, 0);
      }

      public void IOW_StopDisplay()
      {
        this.IOW_SendBits(1, 0, 0, 0, 0, 0, 0, 0);
        this.IOW_SendBits(4, 0, 0, 0, 0, 0, 0, 0);
      }

      public void IOW_SwitchPanel(int panel)
      {
        switch (panel)
        {
          case 0:
            this.IOW_SendIIC(0xdf);
            return;

          case 1:
            this.IOW_SendIIC(0xef);
            return;
        }
      }

      public void IOWLib_IowKitCloseDevice(int iowHandle)
      {
        if (this._IOWDLL != null)
        {
          try
          {
            this._IOWDLL.InvokeMember("IowKitCloseDevice", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { iowHandle });
            if (this.DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.IOWLib_IowKitCloseDevice(): called", new object[0]);
            }
          } catch (Exception exception)
          {
            Log.Error("IOWarrior.IOWDisplay.IOWLib_IowKitOpenDevice(): Caught exception: {0}", new object[] { exception });
          }
        }
      }

      public int IOWLib_IowKitOpenDevice()
      {
        if (this._IOWDLL == null)
        {
          return 0;
        }
        try
        {
          int num = (int)this._IOWDLL.InvokeMember("IowKitOpenDevice", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
          if (this.DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.IOWLib_IowKitOpenDevice(): Returning: {0}", new object[] { num });
          }
          return num;
        } catch (Exception exception)
        {
          Log.Error("IOWarrior.IOWDisplay.IOWLib_IowKitOpenDevice(): Caught exception: {0}", new object[] { exception });
          return 0;
        }
      }

      public int IOWLib_IowKitWrite(int iowHandle, int numPipe, ref byte[] Buffer, int BufferSize)
      {
        if (this._IOWDLL == null)
        {
          return 0;
        }
        try
        {
          int num = (int)this._IOWDLL.InvokeMember("IowKitWrite", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { iowHandle, numPipe, Buffer, BufferSize });
          if (this.DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.IOWLib_IowKitWrite(): Returning: {0}", new object[] { num });
          }
          return num;
        } catch (Exception exception)
        {
          Log.Error("IOWarrior.IOWDisplay.IOWLib_IowKitWrite(): Caught exception: {0}", new object[] { exception });
          return 0;
        }
      }

      private void OnExternalAction(Action action)
      {
        if (this.DisplaySettings.EnableDisplayAction)
        {
          if (this.DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.OnExternalAction(): received action {0}", new object[] { action.wID.ToString() });
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
            Log.Info("IOWarrior.IOWDisplay.OnExternalAction(): received DisplayControlAction", new object[0]);
          }
          this.DisplayOn();
        }
      }

      public bool OpenDisplay(IOWarrior.AdvancedSettings UseSettings)
      {
        Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - called", new object[0]);
        this.AdvSettings = UseSettings;
        MiniDisplayHelper.InitEQ(ref this.EQSettings);
        MiniDisplayHelper.InitDisplayControl(ref this.DisplaySettings);
        this.ParseAdvancedSettings();
        try
        {
          using (MediaPortal.Profile.Settings settings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            this._AudioUseASIO = settings.GetValueAsBool("audioplayer", "asio", false);
          }
          this.IOW_Handle = this.IOWLib_IowKitOpenDevice();
          if ((this.IOW_Handle == -1) | (this.IOW_Handle == 0))
          {
            Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - IOWarrior Device not found", new object[0]);
            this._isOpen = false;
          }
          else
          {
            Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - IOWarrior Device found", new object[0]);
            this.IOW_StartDisplay();
            this.IOW_ClearDisplay();
            this.IOW_Surface = new Bitmap(MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.GraphicWidth, MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.GraphicHeight);
            this.IOW_Graphics = Graphics.FromImage(this.IOW_Surface);
            this._isOpen = true;
            this._isClosing = false;
            Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - Display Opened", new object[0]);
            this.stopDisplayUpdateThread = false;
            this._displayThread = new Thread(new ThreadStart(this.DisplayUpdate));
            this._displayThread.IsBackground = true;
            this._displayThread.Priority = ThreadPriority.BelowNormal;
            this._displayThread.Name = "DisplayUpdateThread";
            this._displayThread.Start();
            if (this._displayThread.IsAlive)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Thread Started", new object[0]);
            }
            else
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() FAILED TO START", new object[0]);
              this.CloseDisplay();
            }
          }
        } catch (Exception exception)
        {
          Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - Display not opened - caught exception {0}", new object[] { exception.ToString() });
          Log.Error(exception);
          this._isOpen = false;
        }
        Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - Completed", new object[0]);
        return this._isOpen;
      }

      private void ParseAdvancedSettings()
      {
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Called", new object[0]);
        this.DoDebug = MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.ExtensiveLogging;
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
        this.EQSettings._EqUpdateDelay = (this.EQSettings._EQ_Restrict_FPS == 0) ? 0 : ((0x989680 / this.EQSettings._EQ_Restrict_FPS) - (0xf4240 / this.EQSettings._EQ_Restrict_FPS));
        this.DisplaySettings.BlankDisplayWithVideo = this.AdvSettings.BlankDisplayWithVideo;
        this.DisplaySettings.EnableDisplayAction = this.AdvSettings.EnableDisplayAction;
        this.DisplaySettings.DisplayActionTime = this.AdvSettings.EnableDisplayActionTime;
        this.DisplaySettings._DisplayControlTimeout = this.DisplaySettings.DisplayActionTime * 0x989680;
        this.DisplaySettings.BlankDisplayWhenIdle = this.AdvSettings.BlankDisplayWhenIdle;
        this.DisplaySettings.BlankIdleDelay = this.AdvSettings.BlankIdleTime;
        this.DisplaySettings._BlankIdleTimeout = this.DisplaySettings.BlankIdleDelay * 0x989680;
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Logging Options - Extensive Logging = {0}", new object[] { this.DoDebug });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options - Equalizer Display: {0}", new object[] { this.EQSettings.UseEqDisplay });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Normal Equalizer Display: {0}", new object[] { this.EQSettings.UseNormalEq });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Stereo Equalizer Display: {0}", new object[] { this.EQSettings.UseStereoEq });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   VU Meter Display: {0}", new object[] { this.EQSettings.UseVUmeter });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   VU Meter Style 2 Display: {0}", new object[] { this.EQSettings.UseVUmeter2 });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Use VU Channel indicators: {0}", new object[] { this.EQSettings._useVUindicators });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Restrict EQ Update Rate: {0}", new object[] { this.EQSettings.RestrictEQ });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Restricted EQ Update Rate: {0} updates per second", new object[] { this.EQSettings._EQ_Restrict_FPS });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Delay EQ Startup: {0}", new object[] { this.EQSettings.DelayEQ });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Delay EQ Startup Time: {0} seconds", new object[] { this.EQSettings._DelayEQTime });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Smooth EQ Amplitude Decay: {0}", new object[] { this.EQSettings.SmoothEQ });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Show Track Info with EQ display: {0}", new object[] { this.EQSettings.EQTitleDisplay });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Show Track Info Interval: {0} seconds", new object[] { this.EQSettings._EQTitleDisplayTime });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Show Track Info duration: {0} seconds", new object[] { this.EQSettings._EQTitleShowTime });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options - Blank display with video: {0}", new object[] { this.DisplaySettings.BlankDisplayWithVideo });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Enable Display on Action: {0}", new object[] { this.DisplaySettings.EnableDisplayAction });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Enable display for: {0} seconds", new object[] { this.DisplaySettings._DisplayControlTimeout });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options - Blank display when idle: {0}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     blank display after: {0} seconds", new object[] { this.DisplaySettings._BlankIdleTimeout / 0xf4240L });
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Completed", new object[0]);
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
              Log.Debug("IOWarrior.IOWDisplay.SendImage() - called", new object[0]);
              if ((_Bitmap.Height == this.IOW_Surface.Height) & (_Bitmap.Width == this.IOW_Surface.Width))
              {
                this.IOW_Surface = (Bitmap)_Bitmap.Clone();
              }
              else
              {
                int height = Math.Min(_Bitmap.Height, this.IOW_Surface.Height);
                int width = Math.Min(_Bitmap.Width, this.IOW_Surface.Width);
                this.IOW_Surface = _Bitmap.Clone(new Rectangle(0, 0, width, height), PixelFormat.Format1bppIndexed);
              }
            }
            Log.Debug("IOWarrior.IOWDisplay.SendImage() - completed", new object[0]);
          }
        }
      }

      public void SendText(string _line1, string _line2)
      {
        this.CheckIdleState();
        if (this.EQSettings._EqDataAvailable || this._IsDisplayOff)
        {
          if (this.DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.SendText(): Suppressing display update! (EqDataAvailable = {0}, _IsDisplayOff = {1})", new object[] { this.EQSettings._EqDataAvailable, this._IsDisplayOff });
          }
        }
        else if (this._isClosing)
        {
          if (this.DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.SendText(): Suppressing display update! Driver is closing", new object[0]);
          }
        }
        else
        {
          RectangleF textBounds = this.GetTextBounds();
          lock (this.DWriteMutex)
          {
            Log.Info("IOWarrior.IOWDisplay.SendText() - called", new object[0]);
            this.IOW_Graphics = Graphics.FromImage(this.IOW_Surface);
            this.IOW_Graphics.SmoothingMode = SmoothingMode.None;
            this.IOW_Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            this.IOW_Graphics.Clear(Color.White);
            Font font = new Font(MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.Font, (float)MediaPortal.ProcessPlugins.MiniDisplayPlugin.Settings.Instance.FontSize);
            int length = _line1.Length;
            while (this.IOW_Graphics.MeasureString(_line1.Substring(0, length), font).Width > textBounds.Width)
            {
              length--;
            }
            this.IOW_Graphics.DrawString(_line1.Substring(0, length), font, Brushes.Black, textBounds);
            textBounds.Offset(0f, font.GetHeight() + 1f);
            textBounds.Height -= font.GetHeight() + 1f;
            length = _line2.Length;
            while (this.IOW_Graphics.MeasureString(_line2.Substring(0, length), font).Width > textBounds.Width)
            {
              length--;
            }
            this.IOW_Graphics.DrawString(_line2.Substring(0, length), font, Brushes.Black, textBounds);
          }
          Log.Info("IOWarrior.IOWDisplay.SendText() - completed", new object[0]);
        }
      }

      public bool IsOpen
      {
        get
        {
          return this._isOpen;
        }
      }
    }
  }
}

