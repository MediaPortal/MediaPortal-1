#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

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
      Log.Info("IOWarrior.AdvancedSettings_OnSettingsChanged(): called");
      CleanUp();
      LoadAdvancedSettings();
      Thread.Sleep(100);
      Setup(Settings.Instance.Port, Settings.Instance.TextHeight, Settings.Instance.TextWidth,
            Settings.Instance.TextComDelay, Settings.Instance.GraphicHeight, Settings.Instance.GraphicWidth,
            Settings.Instance.GraphicComDelay, Settings.Instance.BackLightControl, Settings.Instance.Backlight,
            Settings.Instance.ContrastControl, Settings.Instance.Contrast, Settings.Instance.BlankOnExit);
      Initialize();
    }

    public void CleanUp()
    {
      if (iowcommDisplay.IsOpen)
      {
        Log.Debug("IOWarrior.CleanUp() - called");
        iowcommDisplay.ClearDisplay();
        iowcommDisplay.CloseDisplay();
        AdvancedSettings.OnSettingsChanged -=
          new AdvancedSettings.OnSettingsChangedHandler(AdvancedSettings_OnSettingsChanged);
      }
    }

    private void Clear()
    {
      if (iowcommDisplay.IsOpen)
      {
        Log.Debug("IOWarrior.Clear() - called");
        iowcommDisplay.ClearDisplay();
      }
    }

    public void Configure()
    {
      new IOWarrior_AdvancedSetupForm().ShowDialog();
    }

    public void Dispose()
    {
      Log.Debug("IOWarrior.Dispose() - called");
    }

    public void DrawImage(Bitmap bitmap)
    {
      if (!_isDisabled && iowcommDisplay.IsOpen)
      {
        Log.Debug("IOWarrior.DrawImage() - called");
        if (bitmap == null)
        {
          Log.Debug("IOWarrior.DrawImage():  bitmap null");
        }
        else
        {
          BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly,
                                                  bitmap.PixelFormat);
          try
          {
            if (ReceivedBitmapData == null)
            {
              ReceivedBitmapData = new byte[bitmapdata.Stride * _Grows];
            }
            Marshal.Copy(bitmapdata.Scan0, ReceivedBitmapData, 0, ReceivedBitmapData.Length);
          }
          catch (Exception exception)
          {
            Log.Debug("IOWarrior.DrawImage(): caught exception - {0}", new object[] {exception.ToString()});
          }
          finally
          {
            bitmap.UnlockBits(bitmapdata);
          }
          byte[] buffer = sha256.ComputeHash(ReceivedBitmapData);
          if (ByteArray.AreEqual(buffer, lastHash))
          {
            Log.Debug("IOWarrior.DrawImage() - completed - bitmap not changed");
          }
          else
          {
            lastHash = buffer;
            iowcommDisplay.SendImage(bitmap);
            UpdateAdvancedSettings();
            Log.Debug("IOWarrior.DrawImage() - completed");
          }
        }
      }
    }

    public void Initialize()
    {
      if (!_isDisabled)
      {
        if (iowcommDisplay.IsOpen)
        {
          iowcommDisplay.CloseDisplay();
        }
        _IsOpen = iowcommDisplay.OpenDisplay(AdvSettings);
        if (_IsOpen)
        {
          Log.Debug("IOWarrior.Initialize() - Display opened.");
          _isDisabled = false;
          AdvancedSettings.OnSettingsChanged +=
            new AdvancedSettings.OnSettingsChangedHandler(AdvancedSettings_OnSettingsChanged);
        }
        else
        {
          Log.Debug("IOWarrior.Initialize() - Unable to open device - display disabled");
          _isDisabled = true;
          _errorMessage = "IOWarrior.Initialize() failed... No IOWarrior display found";
        }
        Log.Debug("IOWarrior.Initialize() - completed");
        Clear();
      }
    }

    private void LoadAdvancedSettings()
    {
      AdvSettings = AdvancedSettings.Load();
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml"));
      SettingsLastModTime = info.LastWriteTime;
      LastSettingsCheck = DateTime.Now;
    }

    public void SetCustomCharacters(int[][] customCharacters) {}

    public void SetLine(int line, string message) {}

    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLight, int _backLightLevel, bool _contrast, int _contrastLevel, bool _blankOnExit)
    {
      _IsConfiguring = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
      DoDebug = _IsConfiguring | Settings.Instance.ExtensiveLogging;
      Log.Info("IOWarrior.Setup() - called");
      Log.Info("IOWarrior.Setup(): IOWarrior Graphical LCD Driver - {0}", new object[] {Description});
      Log.Info("IOWarrior.Setup(): Called by \"{0}\".", new object[] {Assembly.GetEntryAssembly().FullName});
      FileInfo info = new FileInfo(Assembly.GetExecutingAssembly().Location);
      if (DoDebug)
      {
        Log.Info("IOWarrior: Assembly creation time: {0} ( {1} UTC )",
                 new object[] {info.LastWriteTime, info.LastWriteTimeUtc.ToUniversalTime()});
      }
      if (DoDebug)
      {
        Log.Info("IOWarrior: Platform: {0}", new object[] {Environment.OSVersion.VersionString});
      }
      try
      {
        LoadAdvancedSettings();
        AdvancedSettings.OnSettingsChanged +=
          new AdvancedSettings.OnSettingsChangedHandler(AdvancedSettings_OnSettingsChanged);
        _Grows = _linesG;
        _Gcols = _colsG;
        if (_Gcols > 0x80)
        {
          Log.Info("IOWarrior.Setup() - Invalid Graphics Columns value");
          _Grows = 0x80;
        }
        if (_Grows > 0x40)
        {
          Log.Info("IOWarrior.Setup() - Invalid Graphics Lines value");
          _Grows = 0x40;
        }
      }
      catch (Exception exception)
      {
        Log.Debug("IOWarrior.Setup() - threw an exception: {0}", new object[] {exception.ToString()});
        _isDisabled = true;
        _errorMessage = "IOWarrior.setup() failed... Did you copy the required files to the MediaPortal directory?";
      }
      Log.Info("IOWarrior.Setup() - completed");
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (DoDebug)
        {
          Log.Info("IOWarrior.UpdateAdvancedSettings(): called");
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml")))
        {
          FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml"));
          if (info.LastWriteTime.Ticks > SettingsLastModTime.Ticks)
          {
            if (DoDebug)
            {
              Log.Info("IOWarrior.UpdateAdvancedSettings(): updating advanced settings");
            }
            LoadAdvancedSettings();
          }
        }
        if (DoDebug)
        {
          Log.Info("IOWarrior.UpdateAdvancedSettings(): completed");
        }
      }
    }

    public string Description
    {
      get { return "IOWarrior Series Graphic LCD driver v04_17_2008"; }
    }

    public string ErrorMessage
    {
      get { return _errorMessage; }
    }

    public bool IsDisabled
    {
      get
      {
        _isDisabled = false;
        int num = 0;
        if (!File.Exists("iowkit.dll"))
        {
          num++;
        }
        if ((num > 0) && ((num & 1) > 0))
        {
          _errorMessage = "Required file \"iowkit.dll\" is not installed!\n";
          _isDisabled = true;
          Log.Info("MatrixGX.IsDisabled() - Required file \"iowkit.dll\" is not installed!");
        }
        return _isDisabled;
      }
    }

    public string Name
    {
      get { return "IOWarrior"; }
    }

    public bool SupportsGraphics
    {
      get { return true; }
    }

    public bool SupportsText
    {
      get { return false; }
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
      private static AdvancedSettings m_Instance;
      private bool m_NormalEQ = true;
      private bool m_RestrictEQ;
      private bool m_SmoothEQ;
      private bool m_StereoEQ;
      private bool m_VUindicators;
      private bool m_VUmeter;
      private bool m_VUmeter2;

      public static event OnSettingsChangedHandler OnSettingsChanged;

      private static void Default(AdvancedSettings _settings)
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

      public static AdvancedSettings Load()
      {
        AdvancedSettings settings;
        Log.Debug("IOWarrior.AdvancedSettings.Load() started");
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml")))
        {
          Log.Debug("IOWarrior.AdvancedSettings.Load() Loading settings from XML file");
          XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml"));
          settings = (AdvancedSettings)serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Debug("IOWarrior.AdvancedSettings.Load() Loading settings from defaults");
          settings = new AdvancedSettings();
          Default(settings);
        }
        Log.Debug("IOWarrior.AdvancedSettings.Load() completed");
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
        Log.Debug("IOWarrior.AdvancedSettings.Save() Saving settings to XML file");
        XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_IOWarrior.xml"),
                                                 Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter)writer, ToSave);
        writer.Close();
        Log.Debug("IOWarrior.AdvancedSettings.Save() completed");
      }

      public static void SetDefaults()
      {
        Default(Instance);
      }

      [XmlAttribute]
      public bool BlankDisplayWhenIdle
      {
        get { return m_BlankDisplayWhenIdle; }
        set { m_BlankDisplayWhenIdle = value; }
      }

      [XmlAttribute]
      public bool BlankDisplayWithVideo
      {
        get { return m_BlankDisplayWithVideo; }
        set { m_BlankDisplayWithVideo = value; }
      }

      [XmlAttribute]
      public int BlankIdleTime
      {
        get { return m_BlankIdleTime; }
        set { m_BlankIdleTime = value; }
      }

      [XmlAttribute]
      public bool DelayEQ
      {
        get { return m_DelayEQ; }
        set { m_DelayEQ = value; }
      }

      [XmlAttribute]
      public int DelayEqTime
      {
        get { return m_DelayEqTime; }
        set { m_DelayEqTime = value; }
      }

      [XmlAttribute]
      public bool EnableDisplayAction
      {
        get { return m_EnableDisplayAction; }
        set { m_EnableDisplayAction = value; }
      }

      [XmlAttribute]
      public int EnableDisplayActionTime
      {
        get { return m_EnableDisplayActionTime; }
        set { m_EnableDisplayActionTime = value; }
      }

      [XmlAttribute]
      public bool EqDisplay
      {
        get { return m_EqDisplay; }
        set { m_EqDisplay = value; }
      }

      [XmlAttribute]
      public int EqRate
      {
        get { return m_EqRate; }
        set { m_EqRate = value; }
      }

      [XmlAttribute]
      public bool EQTitleDisplay
      {
        get { return m_EQTitleDisplay; }
        set { m_EQTitleDisplay = value; }
      }

      [XmlAttribute]
      public int EQTitleDisplayTime
      {
        get { return m_EQTitleDisplayTime; }
        set { m_EQTitleDisplayTime = value; }
      }

      [XmlAttribute]
      public int EQTitleShowTime
      {
        get { return m_EQTitleShowTime; }
        set { m_EQTitleShowTime = value; }
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
        get { return m_NormalEQ; }
        set { m_NormalEQ = value; }
      }

      [XmlAttribute]
      public bool RestrictEQ
      {
        get { return m_RestrictEQ; }
        set { m_RestrictEQ = value; }
      }

      [XmlAttribute]
      public bool SmoothEQ
      {
        get { return m_SmoothEQ; }
        set { m_SmoothEQ = value; }
      }

      [XmlAttribute]
      public bool StereoEQ
      {
        get { return m_StereoEQ; }
        set { m_StereoEQ = value; }
      }

      [XmlAttribute]
      public bool VUindicators
      {
        get { return m_VUindicators; }
        set { m_VUindicators = value; }
      }

      [XmlAttribute]
      public bool VUmeter
      {
        get { return m_VUmeter; }
        set { m_VUmeter = value; }
      }

      [XmlAttribute]
      public bool VUmeter2
      {
        get { return m_VUmeter2; }
        set { m_VUmeter2 = value; }
      }

      public delegate void OnSettingsChangedHandler();
    }

    private class IOWDisplay
    {
      private bool _AudioUseASIO;
      private Thread _displayThread;
      private Type _IOWDLL;
      private bool _isClosing;
      private bool _IsDisplayOff;
      private bool _isOpen;
      public static bool _mpIsIdle;
      private AdvancedSettings AdvSettings;
      private const BindingFlags BINDING_FLAGS = (BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);
      private BitmapConverter bitmaputils = new BitmapConverter();
      private byte[] byLocal = new byte[0x400];
      private readonly byte[] byOldImage = new byte[0x400];
      private DisplayControl DisplaySettings;
      private bool DoDebug;
      private readonly object DWriteMutex = new object();
      private EQControl EQSettings;
      private int iDispHeight = 0x40;
      private int iDispWidth = 0x80;

      private string IdleMessage = ((Settings.Instance.IdleMessage != string.Empty)
                                      ? Settings.Instance.IdleMessage
                                      : "MediaPortal");

      private string IOW_DLLFile;
      private Graphics IOW_Graphics;
      private int IOW_Handle;
      private Bitmap IOW_Surface;
      private byte[] m_Buffer;

      private const MethodAttributes METHOD_ATTRIBUTES =
        (MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public);

      private SystemStatus MPStatus = new SystemStatus();
      private static ModuleBuilder s_mb;
      private bool stopDisplayUpdateThread;
      private readonly object StopMutex = new object();

      internal IOWDisplay()
      {
        if (DoDebug)
        {
          Log.Info("IOWarrior.iMONDisplay constructor: called");
        }
        if (_IOWDLL == null)
        {
          CreateDLLWrapper();
        }
        AdvancedSettings.Load();
        if (DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay constructor: completed");
        }
      }

      private void CheckIdleState()
      {
        if (MPStatus.MP_Is_Idle)
        {
          if (DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.DisplayLines(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}",
                     new object[] {DisplaySettings.BlankDisplayWhenIdle, DisplaySettings._BlankIdleTimeout});
          }
          if (DisplaySettings.BlankDisplayWhenIdle)
          {
            if (!_mpIsIdle)
            {
              if (DoDebug)
              {
                Log.Info("IOWarrior.IOWDisplay.DisplayLines(): MP going IDLE");
              }
              DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
            }
            if (!_IsDisplayOff &&
                ((DateTime.Now.Ticks - DisplaySettings._BlankIdleTime) > DisplaySettings._BlankIdleTimeout))
            {
              if (DoDebug)
              {
                Log.Info("IOWarrior.IOWDisplay.DisplayLines(): Blanking display due to IDLE state");
              }
              DisplayOff();
            }
          }
          _mpIsIdle = true;
        }
        else
        {
          if (DisplaySettings.BlankDisplayWhenIdle & _mpIsIdle)
          {
            if (DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayLines(): MP no longer IDLE - restoring display");
            }
            DisplayOn();
          }
          _mpIsIdle = false;
        }
      }

      public void ClearDisplay()
      {
        if (_isOpen && !_isClosing)
        {
          lock (DWriteMutex)
          {
            Log.Debug("IOWarrior.IOWDisplay.ClearDisplay() - called");
            IOW_Graphics = Graphics.FromImage(IOW_Surface);
            IOW_Graphics.Clear(Color.White);
            Log.Debug("IOWarrior.IOWDisplay.ClearDisplay() - Sending blank image to device");
            IOW_ClearDisplay();
          }
          Log.Debug("IOWarrior.IOWDisplay.ClearDisplay() - completed");
        }
      }

      public void CloseDisplay()
      {
        Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() - called");
        try
        {
          lock (StopMutex)
          {
            Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() Stopping DisplayUpdate() Thread");
            Thread.Sleep(250);
            stopDisplayUpdateThread = true;
            goto Label_004E;
          }
          Label_0047:
          Thread.Sleep(100);
          Label_004E:
          if (_displayThread.IsAlive)
          {
            goto Label_0047;
          }
          Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() DisplayUpdate() Thread stopped.");
          _isClosing = true;
          lock (DWriteMutex)
          {
            IOW_ClearDisplay();
            IOW_StopDisplay();
            IOWLib_IowKitCloseDevice(IOW_Handle);
            IOW_Graphics.Dispose();
            IOW_Handle = 0;
            _isOpen = false;
          }
          Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() - Display closed.");
        }
        catch (Exception exception)
        {
          Log.Debug("IOWarrior.IOWDisplay.CloseDisplay() - caught exception on display close: {0}",
                    new object[] {exception.ToString()});
          Log.Error(exception);
          _isOpen = false;
        }
      }

      private bool CreateDLLWrapper()
      {
        if (DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay.CreateDLLWrapper(): called");
        }
        new FileInfo(Assembly.GetEntryAssembly().Location);
        IOW_DLLFile = Config.GetFile(Config.Dir.Base, "iowkit.dll");
        if (DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay.CreateDLLWrapper(): using IOW DLL {1}", new object[] {IOW_DLLFile});
        }
        if (IOW_DLLFile == string.Empty)
        {
          if (DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.CreateDLLWrapper(): FAILED - iowkit.dll not found");
          }
          return false;
        }
        try
        {
          if (s_mb == null)
          {
            AssemblyName name = new AssemblyName();
            name.Name = "IOWDLLWrapper" + Guid.NewGuid().ToString("N");
            s_mb =
              AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule(
                "IOW_DLL_wrapper");
          }
          TypeBuilder builder2 = s_mb.DefineType("IOWDLLWrapper" + Guid.NewGuid().ToString("N"));
          MethodBuilder builder3 = builder2.DefinePInvokeMethod("IowKitOpenDevice", IOW_DLLFile,
                                                                MethodAttributes.PinvokeImpl |
                                                                MethodAttributes.HideBySig | MethodAttributes.Static |
                                                                MethodAttributes.Public, CallingConventions.Standard,
                                                                typeof (int), null, CallingConvention.StdCall,
                                                                CharSet.Auto);
          builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
          builder3 = builder2.DefinePInvokeMethod("IowKitCloseDevice", IOW_DLLFile,
                                                  MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                  MethodAttributes.Static | MethodAttributes.Public,
                                                  CallingConventions.Standard, typeof (void), new Type[] {typeof (int)},
                                                  CallingConvention.StdCall, CharSet.Auto);
          builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
          builder3 = builder2.DefinePInvokeMethod("IowKitWrite", IOW_DLLFile,
                                                  MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                  MethodAttributes.Static | MethodAttributes.Public,
                                                  CallingConventions.Standard, typeof (int),
                                                  new Type[] {typeof (int), typeof (int), typeof (byte[]), typeof (int)},
                                                  CallingConvention.StdCall, CharSet.Auto);
          builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
          _IOWDLL = builder2.CreateType();
        }
        catch (Exception exception)
        {
          Log.Error("IOWarrior.IOWDisplay.CreateImonDLLwrapper(): caught exception: {0}", new object[] {exception});
          return false;
        }
        if (DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay.CreateImonDLLWrapper(): Completed - RC DLL wrapper created.");
        }
        return true;
      }

      private void DisplayEQ()
      {
        if (!_mpIsIdle && EQSettings._EqDataAvailable)
        {
          lock (DWriteMutex)
          {
            object obj3;
            RectangleF textBounds = GetTextBounds();
            if (DoDebug)
            {
              Log.Info("MatrixGX.MOGXDisplay.DisplayEQ(): called");
            }
            EQSettings.Render_MaxValue = (EQSettings.UseNormalEq | EQSettings.UseStereoEq)
                                           ? ((int)textBounds.Height)
                                           : ((int)textBounds.Width);
            EQSettings.Render_BANDS = EQSettings.UseNormalEq ? 0x10 : (EQSettings.UseStereoEq ? 8 : 1);
            MiniDisplayHelper.ProcessEqData(ref EQSettings);
            Monitor.Enter(obj3 = DWriteMutex);
            try
            {
              IOW_Graphics.FillRectangle(Brushes.White, textBounds);
              for (int i = 0; i < EQSettings.Render_BANDS; i++)
              {
                RectangleF ef2;
                if (DoDebug)
                {
                  Log.Info("MatrixGX.MOGXDisplay.DisplayEQ(): Rendering {0} band {1} = {2}",
                           new object[]
                             {
                               EQSettings.UseNormalEq
                                 ? "Normal EQ"
                                 : (EQSettings.UseStereoEq
                                      ? "Stereo EQ"
                                      : (EQSettings.UseVUmeter ? "VU Meter" : "VU Meter 2")), i,
                               EQSettings.UseNormalEq
                                 ? EQSettings.EqArray[1 + i].ToString()
                                 : (EQSettings.UseStereoEq
                                      ? (EQSettings.EqArray[1 + i].ToString() + " : " +
                                         EQSettings.EqArray[9 + i].ToString())
                                      : (EQSettings.EqArray[1 + i].ToString() + " : " +
                                         EQSettings.EqArray[2 + i].ToString()))
                             });
                }
                if (EQSettings.UseNormalEq)
                {
                  ef2 = new RectangleF(
                    (textBounds.X + (i * (((int)textBounds.Width) / EQSettings.Render_BANDS))) + 1f,
                    textBounds.Y + (((int)textBounds.Height) - EQSettings.EqArray[1 + i]),
                    (float)((((int)textBounds.Width) / EQSettings.Render_BANDS) - 2),
                    (float)EQSettings.EqArray[1 + i]);
                  IOW_Graphics.FillRectangle(Brushes.Black, ef2);
                }
                else
                {
                  int num2;
                  RectangleF ef3;
                  if (EQSettings.UseStereoEq)
                  {
                    int num4 = (((int)textBounds.Width) / 2) / EQSettings.Render_BANDS;
                    num2 = i * num4;
                    int num3 = (i + EQSettings.Render_BANDS) * num4;
                    ef2 = new RectangleF((textBounds.X + num2) + 1f,
                                         textBounds.Y + (((int)textBounds.Height) - EQSettings.EqArray[1 + i]),
                                         (float)(num4 - 2), (float)EQSettings.EqArray[1 + i]);
                    ef3 = new RectangleF((textBounds.X + num3) + 1f,
                                         textBounds.Y + (((int)textBounds.Height) - EQSettings.EqArray[9 + i]),
                                         (float)(num4 - 2), (float)EQSettings.EqArray[9 + i]);
                    IOW_Graphics.FillRectangle(Brushes.Black, ef2);
                    IOW_Graphics.FillRectangle(Brushes.Black, ef3);
                  }
                  else if (EQSettings.UseVUmeter | EQSettings.UseVUmeter2)
                  {
                    ef2 = new RectangleF(textBounds.X + 1f, textBounds.Y + 1f, (float)EQSettings.EqArray[1 + i],
                                         (float)(((int)(textBounds.Height / 2f)) - 2));
                    num2 = EQSettings.UseVUmeter ? 0 : (((int)textBounds.Width) - EQSettings.EqArray[2 + i]);
                    ef3 = new RectangleF((textBounds.X + num2) + 1f, (textBounds.Y + (textBounds.Height / 2f)) + 1f,
                                         (float)EQSettings.EqArray[2 + i],
                                         (float)(((int)(textBounds.Height / 2f)) - 2));
                    IOW_Graphics.FillRectangle(Brushes.Black, ef2);
                    IOW_Graphics.FillRectangle(Brushes.Black, ef3);
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
        if (!_IsDisplayOff)
        {
          if (DisplaySettings.EnableDisplayAction & DisplaySettings._DisplayControlAction)
          {
            if ((DateTime.Now.Ticks - DisplaySettings._DisplayControlLastAction) <
                DisplaySettings._DisplayControlTimeout)
            {
              if (DoDebug)
              {
                Log.Info("IOWarrior.IOWDisplay.DisplayOff(): DisplayControlAction Timer = {0}.",
                         new object[] {DateTime.Now.Ticks - DisplaySettings._DisplayControlLastAction});
              }
              return;
            }
            if (DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayOff(): DisplayControlAction Timeout expired.");
            }
            DisplaySettings._DisplayControlAction = false;
            DisplaySettings._DisplayControlLastAction = 0L;
          }
          Log.Info("IOWarrior.IOWDisplay.DisplayOff(): called");
          lock (DWriteMutex)
          {
            Log.Info("IOWarrior.IOWDisplay.DisplayOff(): Sending Display OFF command to LCD");
            IOW_BacklightOff();
            ClearDisplay();
            _IsDisplayOff = true;
          }
          Log.Info("IOWarrior.IOWDisplay.DisplayOff(): completed");
        }
      }

      private void DisplayOn()
      {
        if (_IsDisplayOff)
        {
          Log.Info("MatrixGX.MOGXDisplay.DisplayOn(): called");
          lock (DWriteMutex)
          {
            Log.Info("MatrixGX.MOGXDisplay.DisplayOn(): Sending Display ON command to LCD");
            _IsDisplayOff = false;
            IOW_BacklightOn();
          }
          Log.Info("MatrixGX.MOGXDisplay.DisplayOn(): called");
        }
      }

      private void DisplayUpdate()
      {
        if (DoDebug)
        {
          Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Starting Display Update Thread");
        }
        if (DisplaySettings.BlankDisplayWithVideo & DisplaySettings.EnableDisplayAction)
        {
          GUIWindowManager.OnNewAction += new OnActionHandler(OnExternalAction);
        }
        while (true)
        {
          lock (StopMutex)
          {
            if (stopDisplayUpdateThread)
            {
              if (DoDebug)
              {
                Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Display Update Thread terminating");
              }
              if (DisplaySettings.BlankDisplayWithVideo & DisplaySettings.EnableDisplayAction)
              {
                GUIWindowManager.OnNewAction -= new OnActionHandler(OnExternalAction);
              }
              return;
            }
          }
          MiniDisplayHelper.GetSystemStatus(ref MPStatus);
          if (DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Collecting status...");
          }
          if (MPStatus.MediaPlayer_Playing)
          {
            if ((MPStatus.Media_IsTV || MPStatus.Media_IsTVRecording) &
                (!MPStatus.Media_IsDVD && !MPStatus.Media_IsCD))
            {
              if (MPStatus.MediaPlayer_Playing)
              {
                if (DisplaySettings.BlankDisplayWithVideo)
                {
                  DisplayOff();
                }
              }
              else
              {
                RestoreDisplayFromVideoOrIdle();
              }
            }
            if (MPStatus.Media_IsDVD || MPStatus.Media_IsCD)
            {
              if (MPStatus.MediaPlayer_Playing)
              {
                if ((MPStatus.Media_IsDVD & MPStatus.Media_IsVideo) &&
                    DisplaySettings.BlankDisplayWithVideo)
                {
                  DisplayOff();
                }
              }
              else
              {
                RestoreDisplayFromVideoOrIdle();
              }
            }
            if (MPStatus.Media_IsVideo & !MPStatus.Media_IsDVD)
            {
              if (MPStatus.MediaPlayer_Playing)
              {
                if (DisplaySettings.BlankDisplayWithVideo)
                {
                  DisplayOff();
                }
              }
              else
              {
                RestoreDisplayFromVideoOrIdle();
              }
            }
          }
          if (!MPStatus.MediaPlayer_Playing && !MPStatus.MediaPlayer_Playing)
          {
            RestoreDisplayFromVideoOrIdle();
          }
          if (!MPStatus.MediaPlayer_Active)
          {
            RestoreDisplayFromVideoOrIdle();
          }
          if (DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Status collected...");
          }
          GetEQ();
          DisplayEQ();
          lock (DWriteMutex)
          {
            if (DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() image built - sending to display");
            }
            IOW_SendImage(IOW_Surface);
          }
          if (!EQSettings._EqDataAvailable)
          {
            if (DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Sleeping...");
            }
            Thread.Sleep(250);
            if (DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Waking...");
            }
          }
        }
      }

      private void GetEQ()
      {
        lock (DWriteMutex)
        {
          EQSettings._EqDataAvailable = MiniDisplayHelper.GetEQ(ref EQSettings);
          if (EQSettings._EqDataAvailable)
          {
            _displayThread.Priority = ThreadPriority.AboveNormal;
          }
          else
          {
            _displayThread.Priority = ThreadPriority.BelowNormal;
          }
        }
      }

      private RectangleF GetTextBounds()
      {
        lock (DWriteMutex)
        {
          GraphicsUnit pixel = GraphicsUnit.Pixel;
          return IOW_Surface.GetBounds(ref pixel);
        }
      }

      public void IOW_BacklightOff()
      {
        IOW_SendBits(4, 0, 0, 0, 0, 0, 0, 0);
      }

      public void IOW_BacklightOn()
      {
        IOW_SendBits(4, 1, 0, 0, 0, 0, 0, 0);
      }

      public Bitmap IOW_Bitmap_ConvertBitmap(Bitmap img)
      {
        BitmapData bitmapdata = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly,
                                             img.PixelFormat);
        Bitmap bitmap = new Bitmap(0x80, 0x40, PixelFormat.Format1bppIndexed);
        BitmapData bmd = bitmap.LockBits(new Rectangle(0, 0, 0x80, 0x40), ImageLockMode.ReadWrite,
                                         PixelFormat.Format1bppIndexed);
        for (int i = 0; i < 0x40; i++)
        {
          for (int j = 0; j < 0x80; j++)
          {
            int ofs = (i * bitmapdata.Stride) + (j * 4);
            if (Marshal.ReadByte(bitmapdata.Scan0, ofs) != 0xff)
            {
              IOW_Bitmap_SetIndexedPixel(j, i, bmd, true);
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
        for (int i = 0; i < byLocal.Length; i++)
        {
          byLocal[i] = 0;
        }
        BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly,
                                                bitmap.PixelFormat);
        int length = bitmapdata.Stride * bitmap.Height;
        m_Buffer = new byte[length];
        Marshal.Copy(bitmapdata.Scan0, m_Buffer, 0, length);
        for (int i = 0; i < 8; i++)
        {
          for (int j = 0; j < 0x10; j++)
          {
            for (int k = 0; k < 8; k++)
            {
              for (int m = 0; m < 8; m++)
              {
                if (m_Buffer[((i * 0x80) + j) + (k * 0x10)] == 0)
                {
                  break;
                }
                if (m_Buffer[((k * 0x10) + j) + (i * 0x80)] >= Math.Pow(2.0, (double)(7 - m)))
                {
                  byLocal[(m + (j * 8)) + (i * 0x80)] =
                    (byte)(byLocal[(m + (j * 8)) + (i * 0x80)] + Math.Pow(2.0, (double)k));
                  m_Buffer[((i * 0x80) + j) + (k * 0x10)] =
                    (byte)(m_Buffer[((i * 0x80) + j) + (k * 0x10)] - Math.Pow(2.0, (double)(7 - m)));
                }
              }
            }
          }
        }
        bitmap.UnlockBits(bitmapdata);
        return byLocal;
      }

      public void IOW_ClearDisplay()
      {
        Bitmap bitmapRaw = new Bitmap(iDispWidth, iDispHeight, PixelFormat.Format32bppPArgb);
        IOW_SendImage(bitmapRaw);
      }

      private void IOW_SendBits(byte reportId, byte d1, byte d2, byte d3, byte d4, byte d5, byte d6, byte d7)
      {
        byte[] buffer = new byte[] {reportId, d1, d2, d3, d4, d5, d6, d7};
        IOWLib_IowKitWrite(IOW_Handle, 1, ref buffer, buffer.Length);
      }

      private void IOW_SendIIC(byte byte1)
      {
        IOW_SendBits(2, 0xc2, 0x70, byte1, 0, 0, 0, 0);
      }

      public void IOW_SendImage(Bitmap bitmapRaw)
      {
        Bitmap bitmap = new Bitmap(iDispWidth, iDispHeight, PixelFormat.Format1bppIndexed);
        bitmap = IOW_Bitmap_ConvertBitmap(bitmapRaw);
        byte[] buffer = IOW_BitMap_ToByteArray(bitmap);
        IOW_SwitchPanel(0);
        int num = 0;
        for (int i = 0; i < 8; i++)
        {
          for (int j = 0; j < 2; j++)
          {
            for (int k = 0; k < 0x40; k++)
            {
              int num5 = 0;
              while (buffer[(((j * 0x40) + k) + num5) + (i * 0x80)] !=
                     byOldImage[(((j * 0x40) + k) + num5) + (i * 0x80)])
              {
                byOldImage[(((j * 0x40) + k) + num5) + (i * 0x80)] = buffer[(((j * 0x40) + k) + num5) + (i * 0x80)];
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
                  IOW_SwitchPanel(j);
                  num = j;
                }
                IOW_SendBits(5, 3, 0xc0, (byte)(0x40 + k), (byte)(0xb8 + i), 0, 0, 0);
                int num6 = 0;
                while (num5 >= 6)
                {
                  IOW_SendBits(5, 0x86, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)],
                               buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)],
                               buffer[((((j * 0x40) + k) + num6) + 2) + (i * 0x80)],
                               buffer[((((j * 0x40) + k) + num6) + 3) + (i * 0x80)],
                               buffer[((((j * 0x40) + k) + num6) + 4) + (i * 0x80)],
                               buffer[((((j * 0x40) + k) + num6) + 5) + (i * 0x80)]);
                  num5 -= 6;
                  num6 += 6;
                }
                switch (num5)
                {
                  case 1:
                    IOW_SendBits(5, 0x81, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)], 0, 0, 0, 0, 0);
                    break;

                  case 2:
                    IOW_SendBits(5, 130, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)], 0, 0, 0, 0);
                    break;

                  case 3:
                    IOW_SendBits(5, 0x83, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 2) + (i * 0x80)], 0, 0, 0);
                    break;

                  case 4:
                    IOW_SendBits(5, 0x84, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 2) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 3) + (i * 0x80)], 0, 0);
                    break;

                  case 5:
                    IOW_SendBits(5, 0x85, buffer[(((j * 0x40) + k) + num6) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 1) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 2) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 3) + (i * 0x80)],
                                 buffer[((((j * 0x40) + k) + num6) + 4) + (i * 0x80)], 0);
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
        IOW_SendBits(4, 1, 0, 0, 0, 0, 0, 0);
        IOW_SendBits(5, 1, 0x3f, 0, 0, 0, 0, 0);
        IOW_SendBits(1, 1, 0, 0, 0, 0, 0, 0);
      }

      public void IOW_StopDisplay()
      {
        IOW_SendBits(1, 0, 0, 0, 0, 0, 0, 0);
        IOW_SendBits(4, 0, 0, 0, 0, 0, 0, 0);
      }

      public void IOW_SwitchPanel(int panel)
      {
        switch (panel)
        {
          case 0:
            IOW_SendIIC(0xdf);
            return;

          case 1:
            IOW_SendIIC(0xef);
            return;
        }
      }

      public void IOWLib_IowKitCloseDevice(int iowHandle)
      {
        if (_IOWDLL != null)
        {
          try
          {
            _IOWDLL.InvokeMember("IowKitCloseDevice",
                                 BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                 new object[] {iowHandle});
            if (DoDebug)
            {
              Log.Info("IOWarrior.IOWDisplay.IOWLib_IowKitCloseDevice(): called");
            }
          }
          catch (Exception exception)
          {
            Log.Error("IOWarrior.IOWDisplay.IOWLib_IowKitOpenDevice(): Caught exception: {0}", new object[] {exception});
          }
        }
      }

      public int IOWLib_IowKitOpenDevice()
      {
        if (_IOWDLL == null)
        {
          return 0;
        }
        try
        {
          int num =
            (int)
            _IOWDLL.InvokeMember("IowKitOpenDevice",
                                 BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                 null);
          if (DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.IOWLib_IowKitOpenDevice(): Returning: {0}", new object[] {num});
          }
          return num;
        }
        catch (Exception exception)
        {
          Log.Error("IOWarrior.IOWDisplay.IOWLib_IowKitOpenDevice(): Caught exception: {0}", new object[] {exception});
          return 0;
        }
      }

      public int IOWLib_IowKitWrite(int iowHandle, int numPipe, ref byte[] Buffer, int BufferSize)
      {
        if (_IOWDLL == null)
        {
          return 0;
        }
        try
        {
          int num =
            (int)
            _IOWDLL.InvokeMember("IowKitWrite",
                                 BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                 new object[] {iowHandle, numPipe, Buffer, BufferSize});
          if (DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.IOWLib_IowKitWrite(): Returning: {0}", new object[] {num});
          }
          return num;
        }
        catch (Exception exception)
        {
          Log.Error("IOWarrior.IOWDisplay.IOWLib_IowKitWrite(): Caught exception: {0}", new object[] {exception});
          return 0;
        }
      }

      private void OnExternalAction(Action action)
      {
        if (DisplaySettings.EnableDisplayAction)
        {
          if (DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.OnExternalAction(): received action {0}",
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
          DisplaySettings._DisplayControlAction = true;
          DisplaySettings._DisplayControlLastAction = DateTime.Now.Ticks;
          if (DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.OnExternalAction(): received DisplayControlAction");
          }
          DisplayOn();
        }
      }

      public bool OpenDisplay(AdvancedSettings UseSettings)
      {
        Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - called");
        AdvSettings = UseSettings;
        MiniDisplayHelper.InitEQ(ref EQSettings);
        MiniDisplayHelper.InitDisplayControl(ref DisplaySettings);
        ParseAdvancedSettings();
        try
        {
          using (Profile.Settings settings = new Profile.MPSettings())
          {
            _AudioUseASIO = settings.GetValueAsBool("audioplayer", "asio", false);
          }
          IOW_Handle = IOWLib_IowKitOpenDevice();
          if ((IOW_Handle == -1) | (IOW_Handle == 0))
          {
            Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - IOWarrior Device not found");
            _isOpen = false;
          }
          else
          {
            Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - IOWarrior Device found");
            IOW_StartDisplay();
            IOW_ClearDisplay();
            IOW_Surface = new Bitmap(Settings.Instance.GraphicWidth, Settings.Instance.GraphicHeight);
            IOW_Graphics = Graphics.FromImage(IOW_Surface);
            _isOpen = true;
            _isClosing = false;
            Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - Display Opened");
            stopDisplayUpdateThread = false;
            _displayThread = new Thread(new ThreadStart(DisplayUpdate));
            _displayThread.IsBackground = true;
            _displayThread.Priority = ThreadPriority.BelowNormal;
            _displayThread.Name = "DisplayUpdateThread";
            _displayThread.Start();
            if (_displayThread.IsAlive)
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() Thread Started");
            }
            else
            {
              Log.Info("IOWarrior.IOWDisplay.DisplayUpdate() FAILED TO START");
              CloseDisplay();
            }
          }
        }
        catch (Exception exception)
        {
          Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - Display not opened - caught exception {0}",
                   new object[] {exception.ToString()});
          Log.Error(exception);
          _isOpen = false;
        }
        Log.Info("IOWarrior.IOWDisplay.OpenDisplay() - Completed");
        return _isOpen;
      }

      private void ParseAdvancedSettings()
      {
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Called");
        DoDebug = Settings.Instance.ExtensiveLogging;
        EQSettings.UseEqDisplay = AdvSettings.EqDisplay;
        EQSettings.UseNormalEq = AdvSettings.NormalEQ;
        EQSettings.UseStereoEq = AdvSettings.StereoEQ;
        EQSettings.UseVUmeter = AdvSettings.VUmeter;
        EQSettings.UseVUmeter2 = AdvSettings.VUmeter2;
        EQSettings._useVUindicators = AdvSettings.VUindicators;
        EQSettings.RestrictEQ = AdvSettings.RestrictEQ;
        EQSettings._EQ_Restrict_FPS = AdvSettings.EqRate;
        EQSettings.DelayEQ = AdvSettings.DelayEQ;
        EQSettings._DelayEQTime = AdvSettings.DelayEqTime;
        EQSettings.SmoothEQ = AdvSettings.SmoothEQ;
        EQSettings.EQTitleDisplay = AdvSettings.EQTitleDisplay;
        EQSettings._EQTitleDisplayTime = AdvSettings.EQTitleDisplayTime;
        EQSettings._EQTitleShowTime = AdvSettings.EQTitleShowTime;
        EQSettings._EqUpdateDelay = (EQSettings._EQ_Restrict_FPS == 0)
                                      ? 0
                                      : ((0x989680 / EQSettings._EQ_Restrict_FPS) -
                                         (0xf4240 / EQSettings._EQ_Restrict_FPS));
        DisplaySettings.BlankDisplayWithVideo = AdvSettings.BlankDisplayWithVideo;
        DisplaySettings.EnableDisplayAction = AdvSettings.EnableDisplayAction;
        DisplaySettings.DisplayActionTime = AdvSettings.EnableDisplayActionTime;
        DisplaySettings._DisplayControlTimeout = DisplaySettings.DisplayActionTime * 0x989680;
        DisplaySettings.BlankDisplayWhenIdle = AdvSettings.BlankDisplayWhenIdle;
        DisplaySettings.BlankIdleDelay = AdvSettings.BlankIdleTime;
        DisplaySettings._BlankIdleTimeout = DisplaySettings.BlankIdleDelay * 0x989680;
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Logging Options - Extensive Logging = {0}",
                 new object[] {DoDebug});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options - Equalizer Display: {0}",
                 new object[] {EQSettings.UseEqDisplay});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Normal Equalizer Display: {0}",
                 new object[] {EQSettings.UseNormalEq});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Stereo Equalizer Display: {0}",
                 new object[] {EQSettings.UseStereoEq});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   VU Meter Display: {0}",
                 new object[] {EQSettings.UseVUmeter});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   VU Meter Style 2 Display: {0}",
                 new object[] {EQSettings.UseVUmeter2});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Use VU Channel indicators: {0}",
                 new object[] {EQSettings._useVUindicators});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Restrict EQ Update Rate: {0}",
                 new object[] {EQSettings.RestrictEQ});
        Log.Info(
          "IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Restricted EQ Update Rate: {0} updates per second",
          new object[] {EQSettings._EQ_Restrict_FPS});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Delay EQ Startup: {0}",
                 new object[] {EQSettings.DelayEQ});
        Log.Info(
          "IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Delay EQ Startup Time: {0} seconds",
          new object[] {EQSettings._DelayEQTime});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Smooth EQ Amplitude Decay: {0}",
                 new object[] {EQSettings.SmoothEQ});
        Log.Info(
          "IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Show Track Info with EQ display: {0}",
          new object[] {EQSettings.EQTitleDisplay});
        Log.Info(
          "IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Show Track Info Interval: {0} seconds",
          new object[] {EQSettings._EQTitleDisplayTime});
        Log.Info(
          "IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Show Track Info duration: {0} seconds",
          new object[] {EQSettings._EQTitleShowTime});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options - Blank display with video: {0}",
                 new object[] {DisplaySettings.BlankDisplayWithVideo});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -   Enable Display on Action: {0}",
                 new object[] {DisplaySettings.EnableDisplayAction});
        Log.Info(
          "IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     Enable display for: {0} seconds",
          new object[] {DisplaySettings._DisplayControlTimeout});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options - Blank display when idle: {0}",
                 new object[] {DisplaySettings.BlankDisplayWhenIdle});
        Log.Info(
          "IOWarrior.IOWDisplay.ParseAdvancedSettings(): Advanced options -     blank display after: {0} seconds",
          new object[] {DisplaySettings._BlankIdleTimeout / 0xf4240L});
        Log.Info("IOWarrior.IOWDisplay.ParseAdvancedSettings(): Completed");
      }

      private void RestoreDisplayFromVideoOrIdle()
      {
        if (DisplaySettings.BlankDisplayWithVideo)
        {
          if (DisplaySettings.BlankDisplayWhenIdle)
          {
            if (!_mpIsIdle)
            {
              DisplayOn();
            }
          }
          else
          {
            DisplayOn();
          }
        }
      }

      public void SendImage(Bitmap _Bitmap)
      {
        if (!_isClosing)
        {
          CheckIdleState();
          if (!(EQSettings._EqDataAvailable | _IsDisplayOff))
          {
            lock (DWriteMutex)
            {
              Log.Debug("IOWarrior.IOWDisplay.SendImage() - called");
              if ((_Bitmap.Height == IOW_Surface.Height) & (_Bitmap.Width == IOW_Surface.Width))
              {
                IOW_Surface = (Bitmap)_Bitmap.Clone();
              }
              else
              {
                int height = Math.Min(_Bitmap.Height, IOW_Surface.Height);
                int width = Math.Min(_Bitmap.Width, IOW_Surface.Width);
                IOW_Surface = _Bitmap.Clone(new Rectangle(0, 0, width, height), PixelFormat.Format1bppIndexed);
              }
            }
            Log.Debug("IOWarrior.IOWDisplay.SendImage() - completed");
          }
        }
      }

      public void SendText(string _line1, string _line2)
      {
        CheckIdleState();
        if (EQSettings._EqDataAvailable || _IsDisplayOff)
        {
          if (DoDebug)
          {
            Log.Info(
              "IOWarrior.IOWDisplay.SendText(): Suppressing display update! (EqDataAvailable = {0}, _IsDisplayOff = {1})",
              new object[] {EQSettings._EqDataAvailable, _IsDisplayOff});
          }
        }
        else if (_isClosing)
        {
          if (DoDebug)
          {
            Log.Info("IOWarrior.IOWDisplay.SendText(): Suppressing display update! Driver is closing");
          }
        }
        else
        {
          RectangleF textBounds = GetTextBounds();
          lock (DWriteMutex)
          {
            Log.Info("IOWarrior.IOWDisplay.SendText() - called");
            IOW_Graphics = Graphics.FromImage(IOW_Surface);
            IOW_Graphics.SmoothingMode = SmoothingMode.None;
            IOW_Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            IOW_Graphics.Clear(Color.White);
            Font font = new Font(Settings.Instance.Font, (float)Settings.Instance.FontSize);
            int length = _line1.Length;
            while (IOW_Graphics.MeasureString(_line1.Substring(0, length), font).Width > textBounds.Width)
            {
              length--;
            }
            IOW_Graphics.DrawString(_line1.Substring(0, length), font, Brushes.Black, textBounds);
            textBounds.Offset(0f, font.GetHeight() + 1f);
            textBounds.Height -= font.GetHeight() + 1f;
            length = _line2.Length;
            while (IOW_Graphics.MeasureString(_line2.Substring(0, length), font).Width > textBounds.Width)
            {
              length--;
            }
            IOW_Graphics.DrawString(_line2.Substring(0, length), font, Brushes.Black, textBounds);
          }
          Log.Info("IOWarrior.IOWDisplay.SendText() - completed");
        }
      }

      public bool IsOpen
      {
        get { return _isOpen; }
      }
    }
  }
}