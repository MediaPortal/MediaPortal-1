using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class LCDHypeWrapper : BaseDisplay, IDisplay, IDisposable
  {
    private bool _BlankDisplayOnExit;
    private Thread _EqThread;
    private bool _IsDisplayOff;
    private bool _mpIsIdle;
    private bool _ReverseLightContrast;
    public static bool _stopUpdateEqThread;
    private const BindingFlags BINDING_FLAGS = (BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);
    private byte[] bytes = new byte[0x12c00];
    private DisplayControl DisplaySettings;
    private string dllFile;
    private bool DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
    private object DWriteMutex = new object();
    private EQControl EQSettings;
    private object EqWriteMutex = new object();
    private string errorMessage = "";
    private string IdleMessage = string.Empty;
    private DLLInfo info;
    private bool isDisabled;
    private Bitmap lastBitmap;
    private DateTime LastSettingsCheck = DateTime.Now;
    private LCDHype_CONFIG LCD_CONFIG = new LCDHype_CONFIG();
    private string m_Description;
    private Type m_tDllReg;
    private const int MAX_RESPIXELS = 0x12c00;

    private const MethodAttributes METHOD_ATTRIBUTES =
      (MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public);

    private SystemStatus MPStatus = new SystemStatus();
    private string name;
    private static ModuleBuilder s_mb;
    private DateTime SettingsLastModTime;
    private object ThreadMutex = new object();

    public LCDHypeWrapper(string dllFile)
    {
      try
      {
        this.dllFile = dllFile;
        string[] strArray = dllFile.Split(new char[] {'/', '.', '\\'});
        this.name = strArray[strArray.Length - 2];
        this.CreateLCDHypeWrapper();
        this.GetDllInfo();
      }
      catch (TargetInvocationException exception)
      {
        this.isDisabled = true;
        Exception innerException = exception.InnerException;
        if (innerException != null)
        {
          this.errorMessage = innerException.Message;
        }
        else
        {
          this.errorMessage = exception.Message;
        }
        Log.Error("MiniDisplay:Error while loading driver {0}: {1}", new object[] {dllFile, this.errorMessage});
      }
    }

    private void AdvancedSettings_OnSettingsChanged()
    {
      Log.Info("LCDHypeWrapper.AdvancedSettings_OnSettingsChanged(): called", new object[0]);
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
      Log.Info("LCDHypeWrapper.Cleanup(): called", new object[0]);
      AdvancedSettings.OnSettingsChanged -=
        new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
      if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
      {
        while (this._EqThread.IsAlive)
        {
          Log.Info("LCDHypeWrapper.Cleanup(): Stoping Display_Update() Thread", new object[0]);
          lock (this.ThreadMutex)
          {
            _stopUpdateEqThread = true;
          }
          _stopUpdateEqThread = true;
          Thread.Sleep(500);
        }
      }
      if (!this._BlankDisplayOnExit)
      {
        if (!(this.DisplaySettings._Shutdown1 != string.Empty) && !(this.DisplaySettings._Shutdown2 != string.Empty))
        {
          goto Label_024D;
        }
        lock (this.DWriteMutex)
        {
          this.LCDHypeWrapper_SetLine(0, this.DisplaySettings._Shutdown1);
          this.LCDHypeWrapper_SetLine(1, this.DisplaySettings._Shutdown2);
          this.LCD_SetIOPropertys(this.LCD_CONFIG.Port, this.LCD_CONFIG.DelayText, this.LCD_CONFIG.DelayGraphics,
                                  this.LCD_CONFIG.ColumnsText, this.LCD_CONFIG.RowsText, this.LCD_CONFIG.ColumnsGraphics,
                                  this.LCD_CONFIG.RowsGraphics, true, this.LCD_CONFIG.BacklightLevel,
                                  this.info.SupportContrastSlider, this.LCD_CONFIG.ContrastLevel,
                                  this.LCD_CONFIG.OutPortsMask, this.LCD_CONFIG.UnderLineMode,
                                  this.LCD_CONFIG.UnderlineOutput);
          this.LCD_CleanUp();
          goto Label_024D;
        }
      }
      lock (this.DWriteMutex)
      {
        this.LCD_SetIOPropertys(this.LCD_CONFIG.Port, this.LCD_CONFIG.DelayText, this.LCD_CONFIG.DelayGraphics,
                                this.LCD_CONFIG.ColumnsText, this.LCD_CONFIG.RowsText, this.LCD_CONFIG.ColumnsGraphics,
                                this.LCD_CONFIG.RowsGraphics, false, this.LCD_CONFIG.BacklightLevel,
                                this.info.SupportContrastSlider, this.LCD_CONFIG.ContrastLevel,
                                this.LCD_CONFIG.OutPortsMask, this.LCD_CONFIG.UnderLineMode,
                                this.LCD_CONFIG.UnderlineOutput);
        this.LCD_CleanUp();
      }
      Label_024D:
      Log.Info("LCDHypeWrapper.Cleanup(): completed", new object[0]);
    }

    public void Clear()
    {
      Log.Debug("LCDHypeWrapper.Clear(): called", new object[0]);
      if (Settings.Instance.ForceGraphicText)
      {
        Bitmap bitmap = new Bitmap(this.LCD_CONFIG.ColumnsGraphics, this.LCD_CONFIG.RowsGraphics);
        this.DrawImage(bitmap);
      }
      else
      {
        string str = string.Empty.PadRight(this.LCD_CONFIG.ColumnsText, ' ');
        for (int i = 0; i < this.LCD_CONFIG.RowsText; i++)
        {
          this.LCDHypeWrapper_SetLine(i, str);
        }
      }
      Log.Debug("LCDHypeWrapper.Clear(): completed", new object[0]);
    }

    public void Configure()
    {
      string tag = string.Empty;
      Form form = new LCDHypeWrapper_SetupPickerForm();
      form.ShowDialog();
      tag = (string) form.Tag;
      form.Dispose();
      if (tag != string.Empty)
      {
        if (tag.Equals("LCDHype"))
        {
          this.Configure_DLL();
        }
        if (tag.Equals("MiniDisplay"))
        {
          this.Configure_Advanced();
        }
      }
    }

    public void Configure_Advanced()
    {
      Form form = new LCDHypeWrapper_AdvancedSetupForm();
      form.ShowDialog();
      form.Dispose();
    }

    public void Configure_DLL()
    {
      try
      {
        this.m_tDllReg.InvokeMember("LCD_ConfigDialog",
                                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                    null);
      }
      catch (TargetInvocationException exception)
      {
        if (!(exception.InnerException is EntryPointNotFoundException))
        {
          throw;
        }
      }
    }

    private void CreateLCDHypeWrapper()
    {
      if (s_mb == null)
      {
        AssemblyName name = new AssemblyName();
        name.Name = "LCDHypeWrapper" + Guid.NewGuid().ToString("N");
        s_mb =
          AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule(
            "LCDDriverModule");
      }
      TypeBuilder builder2 = s_mb.DefineType(this.name + Guid.NewGuid().ToString("N"));
      MethodBuilder builder3 = builder2.DefinePInvokeMethod("DLL_GetInfo", this.dllFile,
                                                            MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                                            MethodAttributes.Static | MethodAttributes.Public,
                                                            CallingConventions.Standard, typeof (void),
                                                            new Type[]
                                                              {
                                                                Type.GetType(
                                                                  "MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers.LCDHypeWrapper+DLLInfo&")
                                                              }, CallingConvention.StdCall, CharSet.Auto);
      builder3.DefineParameter(1, ParameterAttributes.Out, "_info");
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_IsReadyToReceive", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (bool), null,
                                              CallingConvention.StdCall, CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_Init", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (void), null,
                                              CallingConvention.StdCall, CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_ConfigDialog", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (void), null,
                                              CallingConvention.StdCall, CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_CleanUp", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (void), null,
                                              CallingConvention.StdCall, CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_GetCGRAMChar", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (byte), new Type[] {typeof (byte)},
                                              CallingConvention.StdCall, CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_SetCGRAMChar", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (void),
                                              new Type[] {typeof (CharacterData)}, CallingConvention.StdCall,
                                              CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_SendToController", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (void), new Type[] {typeof (byte)},
                                              CallingConvention.StdCall, CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_SendToMemory", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (void), new Type[] {typeof (byte)},
                                              CallingConvention.StdCall, CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_SendToGfxMemory", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (void),
                                              new Type[]
                                                {
                                                  typeof (byte[]), typeof (int), typeof (int), typeof (int), typeof (int)
                                                  , typeof (bool)
                                                }, CallingConvention.StdCall, CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_SetOutputAddress", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (void),
                                              new Type[] {typeof (int), typeof (int)}, CallingConvention.StdCall,
                                              CharSet.Auto);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      builder3 = builder2.DefinePInvokeMethod("LCD_SetIOPropertys", this.dllFile,
                                              MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig |
                                              MethodAttributes.Static | MethodAttributes.Public,
                                              CallingConventions.Standard, typeof (void),
                                              new Type[]
                                                {
                                                  typeof (string), typeof (int), typeof (int), typeof (int), typeof (int)
                                                  , typeof (int), typeof (int), typeof (bool), typeof (byte),
                                                  typeof (bool), typeof (byte), typeof (int), typeof (bool),
                                                  typeof (bool)
                                                }, CallingConvention.StdCall, CharSet.Ansi);
      builder3.SetImplementationFlags(MethodImplAttributes.PreserveSig | builder3.GetMethodImplementationFlags());
      this.m_tDllReg = builder2.CreateType();
    }

    private void Display_Update()
    {
      if (this.DisplaySettings.BlankDisplayWithVideo | this.DisplaySettings.EnableDisplayAction)
      {
        GUIWindowManager.OnNewAction += new OnActionHandler(this.OnExternalAction);
      }
      while (true)
      {
        lock (this.ThreadMutex)
        {
          if (this.DoDebug)
          {
            Log.Info("LCDHypeWrapper.Display_Update(): Checking for Thread termination request", new object[0]);
          }
          if (_stopUpdateEqThread)
          {
            if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
            {
              GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
            }
            if (this.DoDebug)
            {
              Log.Info("LCDHypeWrapper.Display_Update(): Display_Update Thread terminating", new object[0]);
            }
            _stopUpdateEqThread = false;
            return;
          }
          MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
          if ((!this.MPStatus.MediaPlayer_Active & this.DisplaySettings.BlankDisplayWithVideo) &
              (this.DisplaySettings.BlankDisplayWhenIdle & !this._mpIsIdle))
          {
            this.DisplayOn();
          }
          if (this.MPStatus.MediaPlayer_Playing)
          {
            if (this.EQSettings.UseEqDisplay)
            {
              if (
                !(this.EQSettings.RestrictEQ &
                  ((DateTime.Now.Ticks - this.EQSettings._LastEQupdate.Ticks) < this.EQSettings._EqUpdateDelay)))
              {
                this.GetEQ();
                this.DisplayEQ();
              }
              else
              {
                Thread.Sleep(50);
              }
            }
            if (this.DisplaySettings.BlankDisplayWithVideo &
                (((this.MPStatus.Media_IsDVD || this.MPStatus.Media_IsVideo) || this.MPStatus.Media_IsTV) ||
                 this.MPStatus.Media_IsTVRecording))
            {
              if (this.DoDebug)
              {
                Log.Info("LCDHypeWrapper.Display_Update(): Turning off display while playing video", new object[0]);
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

    private void DisplayEQ()
    {
      if (this.EQSettings.UseEqDisplay & this.EQSettings._EqDataAvailable)
      {
        if (this.DoDebug)
        {
          Log.Info("\nLCDHypeWrapper.DisplayEQ(): Retrieved {0} samples of Equalizer data.",
                   new object[] {this.EQSettings.EqFftData.Length/2});
        }
        if (this.info.SupportGfxLCD)
        {
          if (this.EQSettings.UseVUmeter || this.EQSettings.UseVUmeter2)
          {
            this.EQSettings.Render_MaxValue = Settings.Instance.GraphicWidth;
            this.EQSettings.Render_BANDS = 1;
            if (this.EQSettings._useVUindicators)
            {
              this.EQSettings.Render_MaxValue = Settings.Instance.GraphicWidth - 8;
            }
          }
          else
          {
            this.EQSettings.Render_MaxValue = Settings.Instance.GraphicHeight;
            if (this.EQSettings.UseStereoEq)
            {
              this.EQSettings.Render_BANDS = 8;
            }
            else
            {
              this.EQSettings.Render_BANDS = 0x10;
            }
          }
        }
        else if (this.EQSettings.UseVUmeter || this.EQSettings.UseVUmeter2)
        {
          this.EQSettings.Render_MaxValue = Settings.Instance.TextWidth;
          this.EQSettings.Render_BANDS = 1;
          if (this.EQSettings._useVUindicators)
          {
            this.EQSettings.Render_MaxValue = Settings.Instance.TextWidth - 1;
          }
        }
        else
        {
          this.EQSettings.Render_MaxValue = Settings.Instance.TextHeight*8;
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
          if ((DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction) <
              this.DisplaySettings._DisplayControlTimeout)
          {
            if (this.DoDebug)
            {
              Log.Info("LCDHypeWrapper.DisplayOff(): DisplayControlAction Timer = {0}.",
                       new object[] {DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction});
            }
            return;
          }
          if (this.DoDebug)
          {
            Log.Info("LCDHypeWrapper.DisplayOff(): DisplayControlAction Timeout expired.", new object[0]);
          }
          this.DisplaySettings._DisplayControlAction = false;
          this.DisplaySettings._DisplayControlLastAction = 0L;
        }
        Log.Info("LCDHypeWrapper.DisplayOff(): completed", new object[0]);
        lock (this.DWriteMutex)
        {
          Log.Info("MODisplay.DisplayOff(): Turning display OFF", new object[0]);
          this.Clear();
          this._IsDisplayOff = true;
          if (this.info.SupportLightSlider)
          {
            this.LCD_SetIOPropertys(this.LCD_CONFIG.Port, this.LCD_CONFIG.DelayText, this.LCD_CONFIG.DelayGraphics,
                                    this.LCD_CONFIG.ColumnsText, this.LCD_CONFIG.RowsText,
                                    this.LCD_CONFIG.ColumnsGraphics, this.LCD_CONFIG.RowsText, false,
                                    this.LCD_CONFIG.BacklightLevel, this.LCD_CONFIG.ContrastControl,
                                    this.LCD_CONFIG.ContrastLevel, this.LCD_CONFIG.OutPortsMask,
                                    this.LCD_CONFIG.UnderLineMode, this.LCD_CONFIG.UnderlineOutput);
          }
        }
        Log.Info("LCDHypeWrapper.DisplayOff(): completed", new object[0]);
      }
    }

    private void DisplayOn()
    {
      if (this._IsDisplayOff)
      {
        Log.Info("LCDHypeWrapper.DisplayOn(): called", new object[0]);
        lock (this.DWriteMutex)
        {
          Log.Info("LCDHypeWrapper.DisplayOn(): Turning Display ON", new object[0]);
          this._IsDisplayOff = false;
          if (this.info.SupportLightSlider)
          {
            this.LCD_SetIOPropertys(this.LCD_CONFIG.Port, this.LCD_CONFIG.DelayText, this.LCD_CONFIG.DelayGraphics,
                                    this.LCD_CONFIG.ColumnsText, this.LCD_CONFIG.RowsText,
                                    this.LCD_CONFIG.ColumnsGraphics, this.LCD_CONFIG.RowsText, true,
                                    this.LCD_CONFIG.BacklightLevel, this.LCD_CONFIG.ContrastControl,
                                    this.LCD_CONFIG.ContrastLevel, this.LCD_CONFIG.OutPortsMask,
                                    this.LCD_CONFIG.UnderLineMode, this.LCD_CONFIG.UnderlineOutput);
          }
        }
        this._IsDisplayOff = false;
        Log.Info("LCDHypeWrapper.DisplayOn(): completed", new object[0]);
      }
    }

    public void Dispose()
    {
    }

    public void DrawImage(Bitmap bitmap)
    {
      if ((this.SupportsGraphics && !this.EQSettings._EqDataAvailable) && (bitmap != this.lastBitmap))
      {
        Array.Clear(this.bytes, 0, this.bytes.Length);
        Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        int num = bitmapdata.Stride/bitmapdata.Width;
        byte[] destination = new byte[bitmapdata.Stride*rect.Height];
        Marshal.Copy(bitmapdata.Scan0, destination, 0, destination.Length);
        bitmap.UnlockBits(bitmapdata);
        for (int i = 0; i < rect.Height; i++)
        {
          for (int j = 0; j < rect.Width; j++)
          {
            int index = (j*num) + (i*bitmapdata.Stride);
            if (Color.FromArgb(destination[index + 2], destination[index + 1], destination[index]).GetBrightness() <
                0.5f)
            {
              this.bytes[j + (i*rect.Width)] = 1;
            }
          }
        }
        lock (this.DWriteMutex)
        {
          this.m_tDllReg.InvokeMember("LCD_SendToGfxMemory",
                                      BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                      new object[] {this.bytes, 0, 0, rect.Width - 1, rect.Height - 1, false});
        }
        this.lastBitmap = bitmap;
      }
    }

    public void GetDllInfo()
    {
      object[] args = new object[1];
      this.m_tDllReg.InvokeMember("DLL_GetInfo", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                                  null, null, args);
      this.info = (DLLInfo) args[0];
    }

    private void GetEQ()
    {
      lock (this.DWriteMutex)
      {
        long Now = DateTime.Now.Ticks;
        if (this.DisplaySettings.EnableDisplayAction &&
            ((Now - this.DisplaySettings._DisplayControlLastAction) <=
             (this.DisplaySettings.DisplayActionTime*1000*1000*10)))
        {
          this.EQSettings._EqDataAvailable = false;
        }
        else
        {
          this.EQSettings._EqDataAvailable = MiniDisplayHelper.GetEQ(ref this.EQSettings);
        }

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
      Log.Info("LCDHypeWrapper.Initialize(): called", new object[0]);
      this.LCD_Init();
      this.LCD_SetIOPropertys(this.LCD_CONFIG.Port, this.LCD_CONFIG.DelayText, this.LCD_CONFIG.DelayGraphics,
                              this.LCD_CONFIG.ColumnsText, this.LCD_CONFIG.RowsText, this.LCD_CONFIG.ColumnsGraphics,
                              this.LCD_CONFIG.RowsGraphics, true, this.LCD_CONFIG.BacklightLevel, true,
                              this.LCD_CONFIG.ContrastLevel, this.LCD_CONFIG.OutPortsMask, this.LCD_CONFIG.UnderLineMode,
                              this.LCD_CONFIG.UnderlineOutput);
      this.lastBitmap = null;
      AdvancedSettings.OnSettingsChanged +=
        new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
      this.LoadAdvancedSettings();
      if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
      {
        Log.Info("LCDHypeWrapper.Setup(): starting Display_Update() thread", new object[0]);
        this._EqThread = new Thread(new ThreadStart(this.Display_Update));
        this._EqThread.IsBackground = true;
        this._EqThread.Priority = ThreadPriority.BelowNormal;
        this._EqThread.Name = "Display_Update";
        this._EqThread.Start();
        if (this._EqThread.IsAlive)
        {
          Log.Info("LCDHypeWrapper.Setup(): Display_Update() Thread Started", new object[0]);
        }
        else
        {
          Log.Info("LCDHypeWrapper.Setup(): Display_Update() FAILED TO START", new object[0]);
        }
      }
      Log.Info("LCDHypeWrapper.Initialize(): completed", new object[0]);
    }

    public bool IsReadyToReceive()
    {
      return
        (bool)
        this.m_tDllReg.InvokeMember("LCD_IsReadyToReceive",
                                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                    null);
    }

    public void LCD_CleanUp()
    {
      if (this.DoDebug)
      {
        Log.Info("LCDHypeWrapper.LCD_CleanUp()", new object[0]);
      }
      this.m_tDllReg.InvokeMember("LCD_CleanUp", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                                  null, null, null);
    }

    public void LCD_Init()
    {
      Log.Info("LCDHypeWrapper.LCD_Init()", new object[0]);
      this.m_tDllReg.InvokeMember("LCD_Init", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                                  null, null, null);
    }

    public void LCD_SetIOPropertys(string Port, int DelayText, int DelayGraphics, int ColumnsText, int RowsText,
                                   int ColumnsGraphics, int RowsGraphics, bool BacklightControl, byte BacklightLevel,
                                   bool ContrastControl, byte ContrastLevel, uint OutPortsMask, bool UnderLineMode,
                                   bool UnderlineOutput)
    {
      try
      {
        if (this.DoDebug)
        {
          Log.Info(
            "LCDHypeWrapper.LCD_SetIOPropertys(Port={0},Exectime={1},ExectimeGfx={2},X={3},Y={4},gX={5},gY={6},LightOn={7},LightSliderValue={8},ContrastOn={9},ContrastSliderValue={10},Outports={11},UnderlineMode={12},UnderlineOutput={13})",
            new object[]
              {
                Port, DelayText, DelayGraphics, ColumnsText, RowsText, ColumnsGraphics, RowsGraphics, BacklightControl,
                BacklightLevel, ContrastControl, ContrastLevel, OutPortsMask, UnderLineMode, UnderlineOutput
              });
        }
        this.m_tDllReg.InvokeMember("LCD_SetIOPropertys",
                                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                    new object[]
                                      {
                                        Port, DelayText, DelayGraphics, ColumnsText, RowsText, ColumnsGraphics,
                                        RowsGraphics, BacklightControl, BacklightLevel, ContrastControl, ContrastLevel,
                                        (int) OutPortsMask, UnderLineMode, UnderlineOutput
                                      });
      }
      catch
      {
      }
    }

    public void LCDHypeWrapper_SetLine(int _line, string _message)
    {
      lock (this.DWriteMutex)
      {
        Debug.WriteLine(String.Format("{0}", _message));
        this.SetPosition(0, _line);
        this.SendText(_message);
      }
    }

    private void LoadAdvancedSettings()
    {
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Called", new object[0]);
      AdvancedSettings settings = AdvancedSettings.Load();
      this.IdleMessage = (Settings.Instance.IdleMessage != string.Empty) ? Settings.Instance.IdleMessage : "MediaPortal";
      this._ReverseLightContrast = AdvancedSettings.Instance.ReverseLightContrast;
      this.DisplaySettings.BlankDisplayWithVideo = settings.BlankDisplayWithVideo;
      this.DisplaySettings.EnableDisplayAction = settings.EnableDisplayAction;
      this.DisplaySettings.DisplayActionTime = settings.EnableDisplayActionTime;
      this.DisplaySettings.BlankDisplayWhenIdle = settings.BlankDisplayWhenIdle;
      this.DisplaySettings.BlankIdleDelay = settings.BlankIdleTime;
      this.DisplaySettings._BlankIdleTimeout = this.DisplaySettings.BlankIdleDelay*0x989680;
      this.DisplaySettings._DisplayControlTimeout = this.DisplaySettings.DisplayActionTime*0x989680;
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
      this.EQSettings._EqUpdateDelay = (this.EQSettings._EQ_Restrict_FPS == 0)
                                         ? 0
                                         : ((0x989680/this.EQSettings._EQ_Restrict_FPS) -
                                            (0xf4240/this.EQSettings._EQ_Restrict_FPS));
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Extensive Logging: {0}",
               new object[] {Settings.Instance.ExtensiveLogging});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Device Port: {0}", new object[] {Settings.Instance.Port});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Shutdown Message - Line 1: {0}",
               new object[] {this.DisplaySettings._Shutdown1});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Shutdown Message - Line 2: {0}",
               new object[] {this.DisplaySettings._Shutdown2});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Reverse Brightness and Contrast Controls: {0}",
               new object[] {this._ReverseLightContrast});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options - Equalizer Display: {0}",
               new object[] {this.EQSettings.UseEqDisplay});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -   Stereo Equalizer Display: {0}",
               new object[] {this.EQSettings.UseStereoEq});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -   VU Meter Display: {0}",
               new object[] {this.EQSettings.UseVUmeter});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -   VU Meter Style 2 Display: {0}",
               new object[] {this.EQSettings.UseVUmeter2});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -     Use VU Channel indicators: {0}",
               new object[] {this.EQSettings._useVUindicators});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -   Restrict EQ Update Rate: {0}",
               new object[] {this.EQSettings.RestrictEQ});
      Log.Info(
        "LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -     Restricted EQ Update Rate: {0} updates per second",
        new object[] {this.EQSettings._EQ_Restrict_FPS});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -   Delay EQ Startup: {0}",
               new object[] {this.EQSettings.DelayEQ});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -     Delay EQ Startup Time: {0} seconds",
               new object[] {this.EQSettings._DelayEQTime});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -   Smooth EQ Amplitude Decay: {0}",
               new object[] {this.EQSettings.SmoothEQ});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -   Show Track Info with EQ display: {0}",
               new object[] {this.EQSettings.EQTitleDisplay});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -     Show Track Info Interval: {0} seconds",
               new object[] {this.EQSettings._EQTitleDisplayTime});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -     Show Track Info duration: {0} seconds",
               new object[] {this.EQSettings._EQTitleShowTime});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options - Blank display with video: {0}",
               new object[] {this.DisplaySettings.BlankDisplayWithVideo});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -   Enable Display on Action: {0}",
               new object[] {this.DisplaySettings.EnableDisplayAction});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -     Enable display for: {0} seconds",
               new object[] {this.DisplaySettings._DisplayControlTimeout});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options - Blank display when idle: {0}",
               new object[] {this.DisplaySettings.BlankDisplayWhenIdle});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Advanced options -     blank display after: {0} seconds",
               new object[] {this.DisplaySettings._BlankIdleTimeout/0xf4240L});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Setting - Audio using ASIO: {0}",
               new object[] {this.EQSettings._AudioUseASIO});
      Log.Info("LCDHypeWrapper.LoadAdvancedSettings(): Completed", new object[0]);
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_LCDHypeWrapper.xml"));
      this.SettingsLastModTime = info.LastWriteTime;
      this.LastSettingsCheck = DateTime.Now;
    }

    private void OnExternalAction(Action action)
    {
      if (this.DisplaySettings.EnableDisplayAction)
      {
        if (this.DoDebug)
        {
          Log.Info("LCDHypeWrapper.OnExternalAction(): received action {0}", new object[] {action.wID.ToString()});
        }
        Action.ActionType wID = action.wID;
        if (wID <= Action.ActionType.ACTION_SHOW_OSD)
        {
          if ((wID != Action.ActionType.ACTION_SHOW_INFO) && (wID != Action.ActionType.ACTION_SHOW_OSD))
          {
            this.DisplaySettings._DisplayControlLastAction = DateTime.Now.Ticks;
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
          Log.Info("LCDHypeWrapper.OnExternalAction(): received DisplayControlAction", new object[0]);
        }
        this.DisplayOn();
      }
    }

    private void RenderEQ(byte[] EqDataArray)
    {
      if (this.info.SupportGfxLCD)
      {
        object obj2;
        Monitor.Enter(obj2 = this.DWriteMutex);
        try
        {
          Bitmap image = new Bitmap(this.LCD_CONFIG.ColumnsGraphics, this.LCD_CONFIG.RowsGraphics);
          GraphicsUnit pixel = GraphicsUnit.Pixel;
          RectangleF bounds = image.GetBounds(ref pixel);
          Graphics graphics = Graphics.FromImage(image);
          graphics.FillRectangle(Brushes.White, bounds);
          for (int i = 0; i < this.EQSettings.Render_BANDS; i++)
          {
            RectangleF ef2;
            if (this.DoDebug)
            {
              Log.Info("LCDHypeWrapper.RenderEQ(): Rendering {0} band {1} = {2}",
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
              ef2 = new RectangleF((bounds.X + (i*(((int) bounds.Width)/this.EQSettings.Render_BANDS))) + 1f,
                                   bounds.Y + (((int) bounds.Height) - this.EQSettings.EqArray[1 + i]),
                                   (float) ((((int) bounds.Width)/this.EQSettings.Render_BANDS) - 2),
                                   (float) this.EQSettings.EqArray[1 + i]);
              graphics.FillRectangle(Brushes.Black, ef2);
            }
            else
            {
              int num2;
              RectangleF ef3;
              if (this.EQSettings.UseStereoEq)
              {
                int num4 = (((int) bounds.Width)/2)/this.EQSettings.Render_BANDS;
                num2 = i*num4;
                int num3 = (i + this.EQSettings.Render_BANDS)*num4;
                ef2 = new RectangleF((bounds.X + num2) + 1f,
                                     bounds.Y + (((int) bounds.Height) - this.EQSettings.EqArray[1 + i]),
                                     (float) (num4 - 2), (float) this.EQSettings.EqArray[1 + i]);
                ef3 = new RectangleF((bounds.X + num3) + 1f,
                                     bounds.Y + (((int) bounds.Height) - this.EQSettings.EqArray[9 + i]),
                                     (float) (num4 - 2), (float) this.EQSettings.EqArray[9 + i]);
                graphics.FillRectangle(Brushes.Black, ef2);
                graphics.FillRectangle(Brushes.Black, ef3);
              }
              else if (this.EQSettings.UseVUmeter | this.EQSettings.UseVUmeter2)
              {
                ef2 = new RectangleF(bounds.X + 1f, bounds.Y + 1f, (float) this.EQSettings.EqArray[1 + i],
                                     (float) (((int) (bounds.Height/2f)) - 2));
                num2 = this.EQSettings.UseVUmeter ? 0 : (((int) bounds.Width) - this.EQSettings.EqArray[2 + i]);
                ef3 = new RectangleF((bounds.X + num2) + 1f, (bounds.Y + (bounds.Height/2f)) + 1f,
                                     (float) this.EQSettings.EqArray[2 + i], (float) (((int) (bounds.Height/2f)) - 2));
                graphics.FillRectangle(Brushes.Black, ef2);
                graphics.FillRectangle(Brushes.Black, ef3);
              }
            }
          }
          this.DrawImage(image);
          return;
        }
        catch (Exception exception)
        {
          Log.Info("LCDHypeWrapper.DisplayEQ(): CAUGHT EXCEPTION {0}", new object[] {exception});
          if (exception.Message.Contains("ThreadAbortException"))
          {
          }
          return;
        }
        finally
        {
          Monitor.Exit(obj2);
        }
      }
      lock (this.DWriteMutex)
      {
        if (this.EQSettings.UseVUmeter || this.EQSettings.UseVUmeter2)
        {
          if (this.DoDebug)
          {
            Log.Info("LCDHypeWrapper.RenderEQ(): Drawing VU meter", new object[0]);
          }
          string strLeft = "";
          string strRight = "";
          int segmentCount = this.LCD_CONFIG.ColumnsText;
          if (this.EQSettings._useVUindicators)
          {
            strLeft = "L";
            strRight = "R";
            segmentCount--;
          }
          if (this.DoDebug)
          {
            Log.Info("LCDHypeWrapper.RenderEQ(): segment count: {0}", segmentCount);
          }
          for (int j = 0; j < segmentCount; j++)
          {
            if (EqDataArray[1] > j)
            {
              strLeft = strLeft + '=';
            }
            else
            {
              strLeft = strLeft + ' ';
            }
            if (EqDataArray[2] > j)
            {
              strRight = strRight + '=';
            }
            else
            {
              strRight = strRight + ' ';
            }
          }
          if (this.EQSettings.UseVUmeter2)
          {
            char[] strArray = strRight.ToCharArray();
            Array.Reverse(strArray);
            strRight = new string(strArray);
            //strRight = strRight.Replace(">", "<");
          }
          if (this.DoDebug)
          {
            Log.Info("LCDHypeWrapper.RenderEQ(): Sending VU meter data to display: L = \"{0}\" - R = \"{1}\"",
                     new object[] {strLeft, strRight});
          }
          this.LCDHypeWrapper_SetLine(0, strLeft);
          this.LCDHypeWrapper_SetLine(1, strRight);
        }
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

    private void SendText(string _text)
    {
      for (int i = 0; i < _text.Length; i++)
      {
        byte num2 = (byte) _text[i];
        if (num2 < 0x20)
        {
          num2 =
            (byte)
            this.m_tDllReg.InvokeMember("LCD_GetCGRAMChar",
                                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null,
                                        null, new object[] {num2});
        }
        this.m_tDllReg.InvokeMember("LCD_SendToMemory",
                                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                    new object[] {num2});
      }
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
      for (int i = 0; i < customCharacters.GetLength(0); i++)
      {
        CharacterData data = new CharacterData();
        data.Position = (byte) i;
        data.SetData(customCharacters[i]);
        this.m_tDllReg.InvokeMember("LCD_SetCGRAMChar",
                                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                    new object[] {data});
      }
    }

    public void SetLine(int line, string message)
    {
      this.UpdateAdvancedSettings();
      if (this.DoDebug)
      {
        Log.Info("LCDHypeWrapper.SetLine() Called", new object[0]);
      }
      if (this.EQSettings._EqDataAvailable || this._IsDisplayOff)
      {
        if (this.DoDebug)
        {
          Log.Info("LCDHypeWrapper.SetLine(): Suppressing display update!", new object[0]);
        }
      }
      else
      {
        if (this.DoDebug)
        {
          Log.Info("LCDHypeWrapper.SetLine(): Line {0} - Message = \"{1}\"", new object[] {line, message});
        }
        this.LCDHypeWrapper_SetLine(line, message);
        if (this.DoDebug)
        {
          Log.Info("LCDHypeWrapper.SetLine(): message sent to display", new object[0]);
        }
      }
      MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
      if ((line == 0) && this.MPStatus.MP_Is_Idle)
      {
        if (this.DoDebug)
        {
          Log.Info("LCDHypeWrapper.SetLine(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}",
                   new object[] {this.DisplaySettings.BlankDisplayWhenIdle, this.DisplaySettings._BlankIdleTimeout});
        }
        if (this.DisplaySettings.BlankDisplayWhenIdle)
        {
          if (!this._mpIsIdle)
          {
            if (this.DoDebug)
            {
              Log.Info("LCDHypeWrapper.SetLine(): MP going IDLE", new object[0]);
            }
            this.DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
          }
          if (!this._IsDisplayOff &&
              ((DateTime.Now.Ticks - this.DisplaySettings._BlankIdleTime) > this.DisplaySettings._BlankIdleTimeout))
          {
            if (this.DoDebug)
            {
              Log.Info("LCDHypeWrapper.SetLine(): Blanking display due to IDLE state", new object[0]);
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
            Log.Info("LCDHypeWrapper.SetLine(): MP no longer IDLE - restoring display", new object[0]);
          }
          this.DisplayOn();
        }
        this._mpIsIdle = false;
      }
    }

    protected void SetPosition(int x, int y)
    {
      if (this.DoDebug)
      {
        Log.Info("LCDHypeWrapper.SetPosition(): LCD_SetOutputAddress(X={0},Y={1})", new object[] {x, y});
      }
      this.m_tDllReg.InvokeMember("LCD_SetOutputAddress",
                                  BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null,
                                  new object[] {x, y});
    }

    public void Setup(string _port, int _lines, int _cols, int _time, int _linesG, int _colsG, int _timeG,
                      bool _backLight, int _backlightSetting, bool _contrast, int _contrastSetting, bool _blankOnExit)
    {
      this.DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      Log.Info("{0}", new object[] {this.Description});
      Log.Info("LCDHypeWrapper.Setup(): called", new object[0]);
      this.LoadAdvancedSettings();
      this.LCD_CONFIG.Port = _port;
      this.LCD_CONFIG.DelayText = _time;
      this.LCD_CONFIG.DelayGraphics = _timeG;
      this.LCD_CONFIG.ColumnsText = _cols;
      this.LCD_CONFIG.RowsText = _lines;
      this.LCD_CONFIG.ColumnsGraphics = _colsG;
      this.LCD_CONFIG.RowsGraphics = _linesG;
      this.LCD_CONFIG.BacklightLevel = (byte) _backlightSetting;
      this.LCD_CONFIG.ContrastLevel = (byte) _contrastSetting;
      this.LCD_CONFIG.ContrastControl = this.info.SupportContrastSlider;
      this.LCD_CONFIG.OutPortsMask = 0;
      this.LCD_CONFIG.UnderLineMode = false;
      this.LCD_CONFIG.UnderlineOutput = false;
      MiniDisplayHelper.InitEQ(ref this.EQSettings);
      MiniDisplayHelper.InitDisplayControl(ref this.DisplaySettings);
      this._BlankDisplayOnExit = _blankOnExit;
      Log.Info("LCDHypeWrapper.Setup(): LCDHype driver supports backlight = {0}",
               new object[] {this.info.SupportLightSlider});
      Log.Info("LCDHypeWrapper.Setup(): BackLight Setting: {0}", new object[] {this.LCD_CONFIG.BacklightLevel});
      Log.Info("LCDHypeWrapper.Setup(): LCDHype driver supports contrast = {0}",
               new object[] {this.info.SupportContrastSlider});
      Log.Info("LCDHypeWrapper.Setup(): Contrast Setting: {0}", new object[] {this.LCD_CONFIG.ContrastLevel});
      this.LCD_SetIOPropertys(this.LCD_CONFIG.Port, this.LCD_CONFIG.DelayText, this.LCD_CONFIG.DelayGraphics,
                              this.LCD_CONFIG.ColumnsText, this.LCD_CONFIG.RowsText, this.LCD_CONFIG.ColumnsGraphics,
                              this.LCD_CONFIG.RowsGraphics, true, this.LCD_CONFIG.BacklightLevel, true,
                              this.LCD_CONFIG.ContrastLevel, this.LCD_CONFIG.OutPortsMask, this.LCD_CONFIG.UnderLineMode,
                              this.LCD_CONFIG.UnderlineOutput);
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= this.LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (this.DoDebug)
        {
          Log.Info("LCDHypeWrapper.UpdateAdvancedSettings(): called", new object[0]);
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_LCDHypeWrapper.xml")))
        {
          FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_LCDHypeWrapper.xml"));
          if (info.LastWriteTime.Ticks > this.SettingsLastModTime.Ticks)
          {
            if (this.DoDebug)
            {
              Log.Info("LCDHypeWrapper.UpdateAdvancedSettings(): updating advanced settings", new object[0]);
            }
            this.LoadAdvancedSettings();
          }
        }
        if (this.DoDebug)
        {
          Log.Info("LCDHypeWrapper.UpdateAdvancedSettings(): completed", new object[0]);
        }
      }
    }

    public string Description
    {
      get
      {
        string str = "(LCDHype 04_22_2008) ";
        if (this.m_Description == null)
        {
          if ((this.info.IDArray == null) || this.isDisabled)
          {
            return (str + this.Name + " (disabled)");
          }
          int index = 0;
          while ((index < 0x100) && (this.info.IDArray[index] != '\0'))
          {
            index++;
          }
          this.m_Description = new string(this.info.IDArray, 0, index);
        }
        return (str + this.m_Description);
      }
    }

    public string ErrorMessage
    {
      get { return this.errorMessage; }
    }

    public bool IsDisabled
    {
      get { return this.isDisabled; }
    }

    public string Name
    {
      get { return this.name; }
    }

    public bool SupportsGraphics
    {
      get { return this.info.SupportGfxLCD; }
    }

    public bool SupportsText
    {
      get { return this.info.SupportTxtLCD; }
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
      private bool m_NormalEQ;
      private bool m_RestrictEQ;
      private bool m_ReverseLightContrast;
      private bool m_SmoothEQ;
      private bool m_StereoEQ;
      private bool m_VUindicators;
      private bool m_VUmeter;
      private bool m_VUmeter2;

      public static event OnSettingsChangedHandler OnSettingsChanged;

      private static void Default(AdvancedSettings _settings)
      {
        Log.Info("LCDHypeWrapper.AdvancedSettings.Default(): called", new object[0]);
        _settings.EqDisplay = false;
        _settings.NormalEQ = false;
        _settings.StereoEQ = false;
        _settings.VUmeter = true;
        _settings.VUindicators = false;
        _settings.RestrictEQ = false;
        _settings.EqRate = 10;
        _settings.DelayEQ = true;
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
        Log.Info("LCDHypeWrapper.AdvancedSettings.Default(): completed", new object[0]);
      }

      public static AdvancedSettings Load()
      {
        AdvancedSettings settings;
        Log.Info("LCDHypeWrapper.AdvancedSettings.Load(): started", new object[0]);
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_LCDHypeWrapper.xml")))
        {
          Log.Info("LCDHypeWrapper.AdvancedSettings.Load(): Loading settings from XML file", new object[0]);
          XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
          XmlTextReader xmlReader =
            new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_LCDHypeWrapper.xml"));
          settings = (AdvancedSettings) serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Info("LCDHypeWrapper.AdvancedSettings.Load(): Loading settings from defaults", new object[0]);
          settings = new AdvancedSettings();
          Default(settings);
        }
        Log.Info("LCDHypeWrapper.AdvancedSettings.Load(): completed", new object[0]);
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
        Log.Info("LCDHypeWrapper.AdvancedSettings.Save(): Saving settings to XML file", new object[0]);
        XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_LCDHypeWrapper.xml"),
                                                 Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter) writer, ToSave);
        writer.Close();
        Log.Info("LCDHypeWrapper.AdvancedSettings.Save(): completed", new object[0]);
      }

      public static void SetDefaults()
      {
        Default(Instance);
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
      public bool RestrictEQ
      {
        get { return this.m_RestrictEQ; }
        set { this.m_RestrictEQ = value; }
      }

      [XmlAttribute]
      public bool ReverseLightContrast
      {
        get { return this.m_ReverseLightContrast; }
        set { this.m_ReverseLightContrast = value; }
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
    private struct CharacterData
    {
      [MarshalAs(UnmanagedType.U1)] public byte Position;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40)] public byte[,] Data;

      public void SetData(int[] data)
      {
        this.Data = new byte[8,8];
        for (int i = 0; i < 8; i++)
        {
          for (int j = 0; j < 8; j++)
          {
            this.Data[7 - i, j] = ((data[j] & ((int) Math.Pow(2.0, (double) i))) > 0) ? ((byte) 1) : ((byte) 0);
          }
        }
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DLLInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)] public char[] IDArray;
      [MarshalAs(UnmanagedType.I1)] public bool SupportGfxLCD;
      [MarshalAs(UnmanagedType.I1)] public bool SupportTxtLCD;
      [MarshalAs(UnmanagedType.I1)] public bool SupportLightSlider;
      [MarshalAs(UnmanagedType.I1)] public bool SupportContrastSlider;
      [MarshalAs(UnmanagedType.I1)] public bool SupportOutports;
      [MarshalAs(UnmanagedType.U1)] public byte CCharWidth;
      [MarshalAs(UnmanagedType.U1)] public byte CCharHeight;
      [MarshalAs(UnmanagedType.U1)] public byte FontPitch;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LCDHype_CONFIG
    {
      public string Port;
      public int DelayText;
      public int DelayGraphics;
      public int ColumnsText;
      public int RowsText;
      public int ColumnsGraphics;
      public int RowsGraphics;
      public byte BacklightLevel;
      public bool ContrastControl;
      public byte ContrastLevel;
      public uint OutPortsMask;
      public bool UnderLineMode;
      public bool UnderlineOutput;
    }
  }
}