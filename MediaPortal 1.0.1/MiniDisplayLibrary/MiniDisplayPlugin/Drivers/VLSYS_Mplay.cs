using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class VLSYS_Mplay : BaseDisplay, IDisplay, IDisposable
  {
    #region variables
    private int _Brightness = 0x3a;
    private bool _EnableDisplay;
    private Thread _EqThread;
    private bool _Fan1_AutoMS;
    private bool _Fan1Auto;
    private int _Fan1Set20;
    private int _Fan1Set30;
    private int _Fan1Set40;
    private int _Fan1Set50;
    private int _Fan1Set60;
    private int _Fan1Set70;
    private int _Fan1Set80;
    private int _Fan1Set90;
    private int _Fan1SetOff = 9;
    private int _Fan1SetOn = 0x27;
    private int _Fan1Speed;
    private bool _Fan2_AutoMS;
    private bool _Fan2Auto;
    private int _Fan2Set20;
    private int _Fan2Set30;
    private int _Fan2Set40;
    private int _Fan2Set50;
    private int _Fan2Set60;
    private int _Fan2Set70;
    private int _Fan2Set80;
    private int _Fan2Set90;
    private int _Fan2SetOff = 9;
    private int _Fan2SetOn = 0x27;
    private int _Fan2Speed;
    private bool _FlushDataBuffers;
    private InputHandler _inputHandler;
    private bool _IsDisplayOff;
    private int _LastCustomCharacterData;
    private int _LastRemoteButton = 0xff;
    private DateTime _LastRemoteButtonTimestamp;
    private bool _ManageMHC;
    private string _MHCFileName = string.Empty;
    private string _MHCWorkingDirectory = string.Empty;
    private bool _mpIsIdle;
    private bool _ProcessReceivedData;
    private int _RemoteButtonPending = 0xff;
    private bool _RestartMHC;
    private bool _ShutdownOnExit;
    public static bool _stopUpdateEqThread;
    private int _Temp1;
    private int _Temp2;
    private bool _TempCmdSent;
    private DateTime _TempCmdSentTime;
    private bool _TempDataValid;
    private bool _TempDataValid2;
    private int _TempIndex;
    private bool _UseBrightness;
    private bool _UseClockOnShutdown;
    private bool _UseFans;
    private bool _UseRemote;
    private int cols = 20;
    private SerialPort commPort;
    private object CommReadLock = new object();
    private static byte[][] DefaultCustomCharacters;
    private DisplayControl DisplaySettings;
    private bool DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
    private object DWriteMutex = new object();
    private EQControl EQSettings;
    private object EqWriteMutex = new object();
    private string errorMessage = "";
    private string IdleMessage = string.Empty;
    private bool isDisabled;
    private DateTime LastSettingsCheck = DateTime.Now;
    private int lines = 2;
    private string MPlay_Model = string.Empty;
    private SystemStatus MPStatus = new SystemStatus();
    private string Port = string.Empty;
    private RemoteControl RemoteSettings;
    private const int SC_CLOSE = 0xf060;
    private DateTime SettingsLastModTime;
    private int TempCount = 2;
    private object ThreadMutex = new object();
    private const int WM_CLOSE = 0x10;
    private const int WM_SYSCOMMAND = 0x112;
    #endregion

    private void AdjustSettingForDetectedDisplay()
    {
      if ((MPlay_Model.Equals("ME7") || MPlay_Model.Equals("MP7")) || (MPlay_Model == "MR2"))
      {
        TempCount = 1;
      }
      
      switch (MPlay_Model)
      {
        case "MZ5":  //remote only
          {
            _UseFans = false;
            DisplaySettings.BlankDisplayWithVideo = false;
            DisplaySettings.EnableDisplayAction = false;
            DisplaySettings.BlankDisplayWhenIdle = false;
            EQSettings.UseEqDisplay = false;
            _UseClockOnShutdown = false;
            _EnableDisplay = false;
            break;
          }
        case "LIS2":  //display only
          {
            Clear();
            Brightness(_Brightness);
            _UseFans = false;
            _UseRemote = false;
            RemoteSettings.DisableRemote = true;
            break;
          }
        default:
          {
            Brightness(_Brightness);
            Clear();
            if (_UseFans)
            {
              Log.Info("VLSYS_Mplay.Initialize(): configuring fan support", new object[0]);
              commPort.Write(new byte[] { 0xa4, 0x7d }, 0, 2);
              Log.Info("VLSYS_Mplay.Initialize(): setting initial fan speed", new object[0]);
              SetFanSpeed();
            }
            break;
          }
      }
      if (!MPlay_Model.Equals("MZ5"))
      {
        DisplaySplashScreen();
      }
    }

    private void AdvancedSettings_OnSettingsChanged()
    {
      Log.Info("VLSYS_Mplay.AdvancedSettings_OnSettingsChanged(): called", new object[0]);
      LoadAdvancedSettings();
      CleanUp();
      Thread.Sleep(100);
      Setup(Settings.Instance.Port, Settings.Instance.TextHeight, Settings.Instance.TextWidth,
                 Settings.Instance.TextComDelay, Settings.Instance.GraphicHeight, Settings.Instance.GraphicWidth,
                 Settings.Instance.GraphicComDelay, Settings.Instance.BackLightControl, Settings.Instance.Backlight,
                 Settings.Instance.ContrastControl, Settings.Instance.Contrast, Settings.Instance.BlankOnExit);
      Initialize();
    }

    private void Brightness(int level)
    {
      if (DoDebug)
      {
        Log.Info("VLSYS_Mplay.Brightness(): called", new object[0]);
      }
      if (isDisabled)
      {
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.Brightness(): completed - display disabled", new object[0]);
        }
      }
      else if (_UseBrightness)
      {
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.Brightness(): Level = {0}", new object[] {level});
        }
        if ((level <= 0x3f) && (level >= 0))
        {
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.Brightness(): Setting display drightness to 25%", new object[0]);
          }
          level = 0x3b;
        }
        if ((level <= 0x7e) && (level > 0x3f))
        {
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.Brightness(): Setting display drightness to 50%", new object[0]);
          }
          level = 0x3a;
        }
        if ((level <= 0xbd) && (level > 0x7e))
        {
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.Brightness(): Setting display drightness to 75%", new object[0]);
          }
          level = 0x39;
        }
        if (level > 0xbd)
        {
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.Brightness(): Setting display drightness to 100%", new object[0]);
          }
          level = 0x38;
        }
        commPort.Write(new byte[] {0xa5, (byte) level}, 0, 2);
        SerialWriteDelay(50);
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.Brightness(): completed", new object[0]);
        }
      }
    }

    public void CleanUp()
    {
      Log.Info("VLSYS_Mplay.CleanUp() called", new object[0]);
      if (isDisabled)
      {
        Log.Info("VLSYS_Mplay.CleanUp() completed - driver disabled", new object[0]);
      }
      else
      {
        AdvancedSettings.OnSettingsChanged -=
          new AdvancedSettings.OnSettingsChangedHandler(AdvancedSettings_OnSettingsChanged);
        _ProcessReceivedData = false;
        DateTime now = DateTime.Now;
        if (EQSettings.UseEqDisplay || DisplaySettings.BlankDisplayWithVideo)
        {
          while (_EqThread.IsAlive)
          {
            Log.Info("iMONLCDg.Cleanup(): Stoping EQ_Update() Thread", new object[0]);
            lock (ThreadMutex)
            {
              _stopUpdateEqThread = true;
            }
            _stopUpdateEqThread = true;
            Thread.Sleep(500);
          }
        }
        if (MPlay_Model == "MZ4" || MPlay_Model == "MP7")
        {
          Log.Info("VLSYS_Mplay.CleanUp() Doing shutdown for {0}", MPlay_Model);
          byte[] buffer = new byte[3];
          commPort.Write(buffer, 0, 3);
          SerialWriteDelay(500);
          Log.Info("VLSYS_Mplay.CleanUp() setting display to ACPI mode", new object[0]);
          commPort.Write(new byte[] {0xae, 0xae}, 0, 2);
          ShutdownFans();
          commPort.Write(new byte[] {0xa4, 0x7e}, 0, 2);
          SendShutdownScreen();
          if (_UseClockOnShutdown)
          {
            commPort.Write(new byte[] {0xa4, 0x76}, 0, 2);
            commPort.Write(
              new byte[]
                {
                  Convert.ToByte(now.Minute), Convert.ToByte(now.Hour), Convert.ToByte(now.Day),
                  Convert.ToByte(DateTime.DaysInMonth(now.Year, now.Month)), Convert.ToByte(now.DayOfWeek + 1),
                  Convert.ToByte(now.Month), Convert.ToByte(now.Second), 11
                }, 0, 8);
          }
          commPort.Write(new byte[] {0xae, 0xae, 0xae}, 0, 3);
          commPort.Write(new byte[] {0xae}, 0, 1);
          commPort.Write(new byte[] {0xa5, 0x3b}, 0, 2);
        }
        else if (!MPlay_Model.Equals("MZ5"))
        {
          Log.Info("VLSYS_Mplay.CleanUp() Doing shutdown for M.Play Blast", new object[0]);
          byte[] buffer8 = new byte[1];
          commPort.Write(buffer8, 0, 1);
          SerialWriteDelay(50);
          commPort.Write(new byte[] {0xae, 0xae, 0xae}, 0, 3);
          SerialWriteDelay(50);
          SendShutdownScreen();
          commPort.Write(new byte[] {0xae, 0xae, 0xae, 0xae}, 0, 4);
          SerialWriteDelay(500);
          commPort.Write(new byte[] {0xa5, 0x3b}, 0, 2);
          SerialWriteDelay(500);
        }
        _LastCustomCharacterData = 0;
        commPort.Close();
        Log.Info("VLSYS_Mplay.CleanUp() commPort closed", new object[0]);
        commPort.Dispose();
        commPort = null;
        if (_ManageMHC)
        {
          RestartMHC();
        }
        Log.Info("VLSYS_Mplay.CleanUp() completed", new object[0]);
      }
    }

    private void Clear()
    {
      if (!isDisabled && _EnableDisplay)
      {
        Log.Info("VLSYS_Mplay.Clear() called", new object[0]);
        if (MPlay_Model.Equals("LIS2"))
        {
          string message = new string(' ', cols);
          for (int i = 0; i < lines; i++)
          {
            SetLine(i, message);
          }
        }
        else
        {
          commPort.Write(new byte[] {160}, 0, 1);
        }
        Log.Info("VLSYS_Mplay.Clear() completed", new object[0]);
      }
    }

    public void Configure()
    {
      Form form = new VLSYS_AdvancedSetupForm();
      form.ShowDialog();
      form.Dispose();
    }

    private void DisplayEQ()
    {
      if ((EQSettings.UseEqDisplay & EQSettings._EqDataAvailable) &&
          !(EQSettings.RestrictEQ &
            ((DateTime.Now.Ticks - EQSettings._LastEQupdate.Ticks) < EQSettings._EqUpdateDelay)))
      {
        if (DoDebug)
        {
          Log.Info("\nVLSYS_Mplay.DisplayEQ(): Retrieved {0} samples of Equalizer data.",
                   new object[] {EQSettings.EqFftData.Length/2});
        }
        if (EQSettings.UseVUmeter || EQSettings.UseVUmeter2)
        {
          EQSettings.Render_MaxValue = 100;
          if (EQSettings._useVUindicators)
          {
            EQSettings.Render_MaxValue = 0x5f;
          }
          EQSettings.Render_BANDS = 1;
        }
        else
        {
          EQSettings.Render_MaxValue = 0x10;
          if (EQSettings.UseStereoEq)
          {
            EQSettings.Render_BANDS = 8;
          }
          else
          {
            EQSettings.Render_BANDS = 0x10;
          }
        }
        MiniDisplayHelper.ProcessEqData(ref EQSettings);
        RenderEQ(EQSettings.EqArray);
        EQSettings._LastEQupdate = DateTime.Now;
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
              Log.Info("VLSYS_Mplay.DisplayOff(): DisplayControlAction Timer = {0}.",
                       new object[] {DateTime.Now.Ticks - DisplaySettings._DisplayControlLastAction});
            }
            return;
          }
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.DisplayOff(): DisplayControlAction Timeout expired.", new object[0]);
          }
          DisplaySettings._DisplayControlAction = false;
          DisplaySettings._DisplayControlLastAction = 0L;
        }
        Log.Info("VLSYS_Mplay.DisplayOff(): completed", new object[0]);
        lock (DWriteMutex)
        {
          Log.Info("VLSYS_Mplay.DisplayOff(): Turning display OFF", new object[0]);
          Clear();
          Brightness(0);
          _IsDisplayOff = true;
        }
        Log.Info("VLSYS_Mplay.DisplayOff(): completed", new object[0]);
      }
    }

    private void DisplayOn()
    {
      if (_IsDisplayOff)
      {
        Log.Info("VLSYS_Mplay.DisplayOn(): called", new object[0]);
        lock (DWriteMutex)
        {
          Log.Info("VLSYS_Mplay.DisplayOn(): Turning Display ON", new object[0]);
          Brightness(_Brightness);
        }
        _IsDisplayOff = false;
        Log.Info("VLSYS_Mplay.DisplayOn(): completed", new object[0]);
      }
    }

    private void DisplaySplashScreen()
    {
      if (_LastCustomCharacterData != 1)
      {
        commPort.Write(new byte[] {0xad}, 0, 1);
        commPort.Write(new byte[] {14, 0x1b, 14, 0, 0, 0, 0, 0}, 0, 8);
        commPort.Write(new byte[] {3, 15, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f}, 0, 8);
        commPort.Write(new byte[] {0x18, 30, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f}, 0, 8);
        commPort.Write(new byte[] {0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 15, 3}, 0, 8);
        commPort.Write(new byte[] {0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 30, 0x18}, 0, 8);
        commPort.Write(new byte[] {3, 15, 0x1c, 0x1c, 0x1c, 15, 3, 0}, 0, 8);
        commPort.Write(new byte[] {0x18, 30, 7, 7, 7, 30, 0x18, 0}, 0, 8);
        commPort.Write(new byte[] {14, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 14}, 0, 8);
        _LastCustomCharacterData = 1;
      }
      byte[] buffer = new byte[3];
      buffer[0] = 0xa1;
      buffer[2] = 0xa7;
      commPort.Write(buffer, 0, 3);
      commPort.Write(new byte[] {1, 2, 1, 2, 5, 6, 0x20, 0x20}, 0, 8);
      commPort.Write(new byte[] {0x20, 0x20, 0x20, 0x20, 0x20, 7, 7, 8}, 0, 8);
      commPort.Write(new byte[] {0x20, 0x20, 0x20, 0x20, 0}, 0, 5);
      byte[] buffer3 = new byte[3];
      buffer3[0] = 0xa2;
      buffer3[2] = 0xa7;
      commPort.Write(buffer3, 0, 3);
      commPort.Write(new byte[] {3, 4, 3, 4, 0x20, 0x20, 0x20, 0x20}, 0, 8);
      commPort.Write(new byte[] {0x20, 0x4d, 0x65, 100, 0x69, 0x61, 80, 0x6f}, 0, 8);
      commPort.Write(new byte[] {0x72, 0x74, 0x61, 0x6c, 0}, 0, 5);
      SerialWriteDelay(0x7d0);
      _EnableDisplay = true;
    }

    public void Dispose()
    {
      Log.Info("VLSYS_Mplay.Dispose() called", new object[0]);
      try
      {
        if ((commPort != null) && commPort.IsOpen)
        {
          commPort.Close();
          commPort.DataReceived -= new SerialDataReceivedEventHandler(WhenDataReceived);
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
      Log.Info("VLSYS_Mplay.Dispose() completed", new object[0]);
    }

    public void DrawImage(Bitmap bitmap)
    {
    }

    private void EQ_Update()
    {
      if (DisplaySettings.BlankDisplayWithVideo & DisplaySettings.EnableDisplayAction)
      {
        GUIWindowManager.OnNewAction += new OnActionHandler(OnExternalAction);
      }
      while (true)
      {
        lock (ThreadMutex)
        {
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.VFD_EQ_Update(): Checking for Thread termination request", new object[0]);
          }
          if (_stopUpdateEqThread)
          {
            if (DisplaySettings.BlankDisplayWithVideo & DisplaySettings.EnableDisplayAction)
            {
              GUIWindowManager.OnNewAction -= new OnActionHandler(OnExternalAction);
            }
            if (DoDebug)
            {
              Log.Info("VLSYS_Mplay.EQ_Update(): EQ_Update Thread terminating", new object[0]);
            }
            _stopUpdateEqThread = false;
            return;
          }
          MiniDisplayHelper.GetSystemStatus(ref MPStatus);
          if ((!MPStatus.MediaPlayer_Active & DisplaySettings.BlankDisplayWithVideo) &
              (DisplaySettings.BlankDisplayWhenIdle & !_mpIsIdle))
          {
            DisplayOn();
          }
          if (MPStatus.MediaPlayer_Playing)
          {
            if (EQSettings.UseEqDisplay)
            {
              GetEQ();
              DisplayEQ();
            }
            if (DisplaySettings.BlankDisplayWithVideo &
                (((MPStatus.Media_IsDVD || MPStatus.Media_IsVideo) || MPStatus.Media_IsTV) ||
                 MPStatus.Media_IsTVRecording))
            {
              if (DoDebug)
              {
                Log.Info("VLSYS_Mplay.EQ_Update(): Turning off display while playing video", new object[0]);
              }
              DisplayOff();
            }
          }
          else
          {
            RestoreDisplayFromVideoOrIdle();
            lock (DWriteMutex)
            {
              EQSettings._EqDataAvailable = false;
              _EqThread.Priority = ThreadPriority.BelowNormal;
            }
          }
        }
        if (!EQSettings._EqDataAvailable || MPStatus.MediaPlayer_Paused)
        {
          Thread.Sleep(250);
        }
      }
    }

    private byte FanSpeedByte(int speed)
    {
      switch (speed)
      {
        case 0:
          return 0;

        case 1:
          return 20;

        case 2:
          return 0x25;

        case 3:
          return 0x36;

        case 4:
          return 0x47;

        case 5:
          return 0x58;

        case 6:
          return 0x6a;

        case 7:
          return 0x7c;

        case 8:
          return 0x8e;

        case 9:
          return 0x9f;

        case 10:
          return 0xb1;
      }
      return 0;
    }

    private byte FanSpeedFromTemp(int fan, int TempAbs)
    {
      if (fan == 1)
      {
        if (TempAbs < _Fan1SetOff)
        {
          return FanSpeedByte(0);
        }
        if (TempAbs < _Fan1Set20)
        {
          return FanSpeedByte(1);
        }
        if (TempAbs < _Fan1Set30)
        {
          return FanSpeedByte(2);
        }
        if (TempAbs < _Fan1Set40)
        {
          return FanSpeedByte(3);
        }
        if (TempAbs < _Fan1Set50)
        {
          return FanSpeedByte(4);
        }
        if (TempAbs < _Fan1Set60)
        {
          return FanSpeedByte(5);
        }
        if (TempAbs < _Fan1Set70)
        {
          return FanSpeedByte(6);
        }
        if (TempAbs < _Fan1Set80)
        {
          return FanSpeedByte(7);
        }
        if (TempAbs < _Fan1Set90)
        {
          return FanSpeedByte(8);
        }
        if (TempAbs < _Fan1SetOn)
        {
          return FanSpeedByte(9);
        }
        return FanSpeedByte(10);
      }
      if (TempAbs < _Fan2SetOff)
      {
        return FanSpeedByte(0);
      }
      if (TempAbs < _Fan2Set20)
      {
        return FanSpeedByte(1);
      }
      if (TempAbs < _Fan2Set30)
      {
        return FanSpeedByte(2);
      }
      if (TempAbs < _Fan2Set40)
      {
        return FanSpeedByte(3);
      }
      if (TempAbs < _Fan2Set50)
      {
        return FanSpeedByte(4);
      }
      if (TempAbs < _Fan2Set60)
      {
        return FanSpeedByte(5);
      }
      if (TempAbs < _Fan2Set70)
      {
        return FanSpeedByte(6);
      }
      if (TempAbs < _Fan2Set80)
      {
        return FanSpeedByte(7);
      }
      if (TempAbs < _Fan2Set90)
      {
        return FanSpeedByte(8);
      }
      if (TempAbs < _Fan2SetOn)
      {
        return FanSpeedByte(9);
      }
      return FanSpeedByte(10);
    }

    private void FireRemoteEvent(int EventCode)
    {
      if (DoDebug)
      {
        Log.Info("VLSYS_Mplay.FireRemoteEvent(): called", new object[0]);
      }
      if (!_inputHandler.MapAction(EventCode))
      {
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.FireRemoteEvent(): No button mapping for remote button = {0}",
                   new object[] {EventCode.ToString("x00")});
        }
      }
      else if (DoDebug)
      {
        Log.Info("VLSYS_Mplay.FireRemoteEvent(): fired event for remote button = {0}",
                 new object[] {EventCode.ToString("x00")});
      }
      _LastRemoteButton = EventCode;
      _RemoteButtonPending = 0xff;
      _LastRemoteButtonTimestamp = DateTime.Now;
      if (DoDebug)
      {
        Log.Info("VLSYS_Mplay.FireRemoteEvent(): completed", new object[0]);
      }
    }

    private void GetEQ()
    {
      lock (DWriteMutex)
      {
        EQSettings._EqDataAvailable = MiniDisplayHelper.GetEQ(ref EQSettings);
        if (EQSettings._EqDataAvailable)
        {
          _EqThread.Priority = ThreadPriority.AboveNormal;
        }
        else
        {
          _EqThread.Priority = ThreadPriority.BelowNormal;
        }
      }
    }

    private void GetTempReading()
    {
      if (_UseFans && (_Fan1Auto || _Fan2Auto))
      {
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.GetTempReading(): called");
        }
        if (!_TempCmdSent)
        {
          if (_ProcessReceivedData)
          {
            commPort.Write(new byte[] { 0xaf }, 0, 1);
            _TempCmdSent = true;
            _TempCmdSentTime = DateTime.Now;
            SerialWriteDelay(60);
            SetFanSpeed();
            if (DoDebug)
            {
              Log.Info("VLSYS_Mplay.GetTempReading(): Requesting Temperature data");
            }
          }
          else if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.GetTempReading(): delaying temperature request");
          }
        }
        else
        {
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.GetTempReading(): Temperature request already pending");
          }
          if (DateTime.Now.Ticks > _TempCmdSentTime.AddSeconds(15.0).Ticks || !_TempDataValid)
          {
            if (DoDebug)
            {
              Log.Info("VLSYS_Mplay.GetTempReading(): Temperature request timed out or not valid!");
            }
            _TempCmdSent = false;
          }
        }
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.GetTempReading(): completed");
        }
      }
    }

    public void Initialize()
    {
      Log.Info("VLSYS_Mplay.Initialize(): called", new object[0]);
      if (_ManageMHC)
      {
        TerminateMHC();
      }
      if (isDisabled)
      {
        Log.Info("VLSYS_Mplay.Initialize(): completed - driver disabled", new object[0]);
      }
      else
      {
        try
        {
          Log.Info("VLSYS_Mplay.Initialize(): opening port {0}", new object[] {Port});
          if (commPort != null)
          {
            commPort.Close();
            commPort.Dispose();
            commPort = null;
          }
          if (MPlay_Model.Equals("LIS2"))
          {
            commPort = new SerialPort(Port, 0x4b00, Parity.None, 8, StopBits.One);
          }
          else
          {
            commPort = new SerialPort(Port, 0x9600, Parity.None, 8, StopBits.One);
          }
          commPort.DiscardNull = false;
          commPort.DtrEnable = true;
          commPort.RtsEnable = true;
          commPort.ReceivedBytesThreshold = 1;
          commPort.Open();
          SerialWriteDelay(40);
                    
          if (MPlay_Model.Equals("AUTOMATIC"))
          {
            int num = 0;
            MPlay_Model = string.Empty;
            Log.Info("VLSYS_Mplay.Initialize(): attempting device detection", new object[0]);
            commPort.DataReceived += new SerialDataReceivedEventHandler(WhenDataReceived);
            while (MPlay_Model == string.Empty && num < 2500)
            {
              commPort.Write(new byte[] { 170, 170 }, 0, 2);
              Log.Info("VLSYS_Mplay.Initialize(): Waiting for device identification ({0}ms)", new object[] { num });
              SerialWriteDelay(250);
              num += 250;
            }
            commPort.DataReceived -= new SerialDataReceivedEventHandler(WhenDataReceived);
            if (MPlay_Model == string.Empty)
            {
              Log.Info(
                "VLSYS_Mplay.Initialize(): Device detection failed - using default device type (MZ4 - M.Play MR300)",
                new object[0]);
              MPlay_Model = "MZ4";
            }
            else
            {
              Log.Info("VLSYS_Mplay.Initialize(): detected device = \"{0}\"", new object[] {MPlay_Model});
              if (!IsValidModel(MPlay_Model))
              {
                Log.Info(
                  "VLSYS_Mplay.Initialize(): Device detected is not an explicitly supported device - using default device type (MZ4 - M.Play MR300)",
                  new object[0]);
                MPlay_Model = "MZ4";
              } 
            }
          }
          AdjustSettingForDetectedDisplay();          
          
          commPort.DiscardInBuffer();
          commPort.DiscardOutBuffer();
          Log.Info("VLSYS_Mplay.Initialize(): Enabling serial receive data processing", new object[0]);
          _FlushDataBuffers = true;              
          
          if (_UseRemote || (_UseFans && (_Fan1Auto || _Fan2Auto)))
          {
            Log.Info("VLSYS_Mplay.Initialize(): configuring serial data receive support", new object[0]);
            commPort.DataReceived += new SerialDataReceivedEventHandler(WhenDataReceived);
            _ProcessReceivedData = true;
          }          
        }
        catch (Exception exception)
        {
          Log.Error("VLSYS_Mplay.Initialize(): CAUGHT EXCEPTION while opening port {0} - {1}",
                    new object[] {Port, exception});
          isDisabled = true;
          errorMessage = "Unable to open port - " + Port;
          if (commPort != null)
          {
            commPort.Close();
            commPort.Dispose();
            commPort = null;
          }
          if (_ManageMHC)
          {
            RestartMHC();
          }
        }
        AdvancedSettings.OnSettingsChanged +=
          new AdvancedSettings.OnSettingsChangedHandler(AdvancedSettings_OnSettingsChanged);
        Clear();
        Log.Info("VLSYS_Mplay.Initialize() completed", new object[0]);
      }
    }

    private void InitRemoteSettings(ref RemoteControl RCsettings)
    {
      RCsettings.DisableRemote = false;
      RCsettings.DisableRepeat = false;
      RCsettings.RepeatDelay = 0;
    }

    private bool IsRemoteKeyCode(int Code)
    {
      switch (Code)
      {
        case 1:
        case 3:
        case 5:
        case 7:
        case 9:
        case 10:
        case 11:
        case 13:
        case 14:
        case 15:
        case 0x11:
        case 0x12:
        case 0x13:
        case 20:
        case 0x15:
        case 0x16:
        case 0x17:
        case 0x19:
        case 0x1a:
        case 0x1b:
        case 0x1d:
        case 30:
        case 0x1f:
        case 0x40:
        case 0x41:
        case 0x42:
        case 0x43:
        case 0x45:
        case 70:
        case 0x47:
        case 0x4a:
        case 0x4b:
        case 0x4c:
        case 0x4d:
        case 0x4e:
        case 0x4f:
        case 80:
        case 0x51:
        case 0x52:
        case 0x53:
        case 0x54:
        case 0x55:
        case 0x56:
          return true;

        case 0x7e:
          if (RemoteSettings.DisableRepeat)
          {
            return false;
          }
          return true;
      }
      return false;
    }

    private bool IsTemperatureCode(int Code)
    {
      if ((MPlay_Model.Equals("ME7") || MPlay_Model.Equals("MP7")) || MPlay_Model.Equals("MR2"))
      {
        if (Code > 0x7f)
        {
          return true;
        }
      }
      else if (Code > 0x95)
      {
        return true;
      }
      return false;
    }

    private bool IsValidModel(string cModel)
    {
      switch (cModel)
      {
        case "MZ5":
        case "MZ4":
        case "ME7":
        case "LE2":
        case "MP4":
        case "ME4":
        case "MP5":
        case "ME5":
        case "MP7":
        case "MR2":
          return true;
      }
      return false;
    }

    private void LoadAdvancedSettings()
    {
      AdvancedSettings settings = AdvancedSettings.Load();
      IdleMessage = (Settings.Instance.IdleMessage != string.Empty) ? Settings.Instance.IdleMessage : "MediaPortal";
      MPlay_Model = settings.DeviceType;
      if (MPlay_Model.Equals(string.Empty))
      {
        MPlay_Model = "AUTOMATIC";
      }
      _ManageMHC = settings.ManageMHC;
      RemoteSettings.DisableRemote = settings.DisableRemote;
      RemoteSettings.DisableRepeat = settings.DisableRepeat;
      RemoteSettings.RepeatDelay = settings.RepeatDelay*0x19;
      _UseFans = settings.UseFans;
      _Fan1Speed = settings.Fan1;
      _Fan2Speed = settings.Fan2;
      _Fan1Auto = settings.Fan1Auto;
      _Fan1SetOff = settings.Fan1_SetOff + 0x1a;
      _Fan1SetOn = settings.Fan1_SetOn + 0x1a;
      _Fan1_AutoMS = settings.Fan1_AutoMS;
      if (_Fan1Auto)
      {
        int num = (_Fan1SetOn - _Fan1SetOff)/10;
        _Fan1Set20 = _Fan1SetOff + num;
        _Fan1Set30 = _Fan1Set20 + num;
        _Fan1Set40 = _Fan1Set30 + num;
        _Fan1Set50 = _Fan1Set40 + num;
        _Fan1Set60 = _Fan1Set50 + num;
        _Fan1Set70 = _Fan1Set60 + num;
        _Fan1Set80 = _Fan1Set70 + num;
        _Fan1Set90 = _Fan1Set80 + num;
      }
      _Fan2Auto = settings.Fan2Auto;
      _Fan2SetOff = settings.Fan2_SetOff + 0x1a;
      _Fan2SetOn = settings.Fan2_SetOn + 0x1a;
      _Fan2_AutoMS = settings.Fan2_AutoMS;
      if (_Fan2Auto)
      {
        int num2 = (_Fan2SetOn - _Fan2SetOff)/10;
        _Fan2Set20 = _Fan2SetOff + num2;
        _Fan2Set30 = _Fan2Set20 + num2;
        _Fan2Set40 = _Fan2Set30 + num2;
        _Fan2Set50 = _Fan2Set40 + num2;
        _Fan2Set60 = _Fan2Set50 + num2;
        _Fan2Set70 = _Fan2Set60 + num2;
        _Fan2Set80 = _Fan2Set70 + num2;
        _Fan2Set90 = _Fan2Set80 + num2;
      }
      DisplaySettings.BlankDisplayWithVideo = settings.BlankDisplayWithVideo;
      DisplaySettings.EnableDisplayAction = settings.EnableDisplayAction;
      DisplaySettings.DisplayActionTime = settings.EnableDisplayActionTime;
      DisplaySettings.BlankDisplayWhenIdle = settings.BlankDisplayWhenIdle;
      DisplaySettings.BlankIdleDelay = settings.BlankIdleTime;
      DisplaySettings._BlankIdleTimeout = DisplaySettings.BlankIdleDelay*0x989680;
      DisplaySettings._DisplayControlTimeout = DisplaySettings.DisplayActionTime*0x989680;
      DisplaySettings._Shutdown1 = Settings.Instance.Shutdown1;
      DisplaySettings._Shutdown2 = Settings.Instance.Shutdown2;
      EQSettings.UseVUmeter = settings.VUmeter;
      EQSettings.UseVUmeter2 = settings.VUmeter2;
      EQSettings._useVUindicators = settings.VUindicators;
      EQSettings.UseEqDisplay = settings.EqDisplay;
      EQSettings.UseStereoEq = settings.StereoEQ;
      EQSettings.DelayEQ = settings.DelayEQ;
      EQSettings._DelayEQTime = settings.DelayEqTime;
      EQSettings.SmoothEQ = settings.SmoothEQ;
      EQSettings.RestrictEQ = settings.RestrictEQ;
      EQSettings._EQ_Restrict_FPS = settings.EqRate;
      EQSettings.EQTitleDisplay = settings.EQTitleDisplay;
      EQSettings._EQTitleDisplayTime = settings.EQTitleDisplayTime;
      EQSettings._EQTitleShowTime = settings.EQTitleShowTime;
      EQSettings._EqUpdateDelay = (EQSettings._EQ_Restrict_FPS == 0)
                                         ? 0
                                         : ((0x989680/EQSettings._EQ_Restrict_FPS) -
                                            (0xf4240/EQSettings._EQ_Restrict_FPS));
      _UseClockOnShutdown = settings.UseClockOnShutdown;
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Device Type: {0}", new object[] {MPlay_Model});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Device Port: {0}", new object[] {Settings.Instance.Port});
      if (!MPlay_Model.Equals("AUTOMATIC"))
      {
        MPlay_Model = MPlay_Model.Substring(0, MPlay_Model.IndexOf(" - ", 0)).Trim();
      }
      //AdjustSettingForDetectedDisplay();
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Extensive Logging: {0}",
               new object[] {Settings.Instance.ExtensiveLogging});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Manage MHC: {0}", new object[] {_ManageMHC});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Disable Remote: {0}",
               new object[] {RemoteSettings.DisableRemote});
      if (!RemoteSettings.DisableRemote)
      {
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Disable Repeat: {0}",
                 new object[] {RemoteSettings.DisableRepeat});
        if (!RemoteSettings.DisableRepeat)
        {
          Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Repeat Delay: {0}",
                   new object[] {RemoteSettings.RepeatDelay});
        }
      }
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Use Fan Control = {0}", new object[] {_UseFans});
      if (!MPlay_Model.Equals("AUTOMATIC"))
      {
        MPlay_Model = MPlay_Model.Substring(0, 3);
      }
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 manual speed = {0}", new object[] {_Fan1Speed});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto mode = {0}", new object[] {_Fan1Auto});
      if (_Fan1Auto)
      {
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto  Off Temp. = {0}", new object[] {_Fan1SetOff});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto  20% Temp. = {0}", new object[] {_Fan1Set20});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto  30% Temp. = {0}", new object[] {_Fan1Set30});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto  40% Temp. = {0}", new object[] {_Fan1Set40});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto  50% Temp. = {0}", new object[] {_Fan1Set50});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto  60% Temp. = {0}", new object[] {_Fan1Set60});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto  70% Temp. = {0}", new object[] {_Fan1Set70});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto  80% Temp. = {0}", new object[] {_Fan1Set80});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto  90% Temp. = {0}", new object[] {_Fan1Set90});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Auto 100% Temp. = {0}", new object[] {_Fan1SetOn});
        if (_Fan1_AutoMS)
        {
          Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #1 Use manual speed on shutdown = {0}",
                   new object[] {_Fan1_AutoMS});
        }
      }
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 manual speed = {0}", new object[] {_Fan2Speed});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto mode = {0}", new object[] {_Fan2Auto});
      if (_Fan2Auto)
      {
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto  Off Temp. = {0}", new object[] {_Fan2SetOff});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto  20% Temp. = {0}", new object[] {_Fan2Set20});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto  30% Temp. = {0}", new object[] {_Fan2Set30});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto  40% Temp. = {0}", new object[] {_Fan2Set40});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto  50% Temp. = {0}", new object[] {_Fan2Set50});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto  60% Temp. = {0}", new object[] {_Fan2Set60});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto  70% Temp. = {0}", new object[] {_Fan2Set70});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto  80% Temp. = {0}", new object[] {_Fan2Set80});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto  90% Temp. = {0}", new object[] {_Fan2Set90});
        Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Auto 100% Temp. = {0}", new object[] {_Fan2SetOn});
        if (_Fan2_AutoMS)
        {
          Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Fan #2 Use manual speed on shutdown = {0}",
                   new object[] {_Fan2_AutoMS});
        }
      }
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options - Equalizer Display: {0}",
               new object[] {EQSettings.UseEqDisplay});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -   Stereo Equalizer Display: {0}",
               new object[] {EQSettings.UseStereoEq});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -   VU Meter Display: {0}",
               new object[] {EQSettings.UseVUmeter});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -     Use VU Channel indicators: {0}",
               new object[] {EQSettings._useVUindicators});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -   Restrict EQ Update Rate: {0}",
               new object[] {EQSettings.RestrictEQ});
      Log.Info(
        "VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -     Restricted EQ Update Rate: {0} updates per second",
        new object[] {EQSettings._EQ_Restrict_FPS});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -   Delay EQ Startup: {0}",
               new object[] {EQSettings.DelayEQ});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -     Delay EQ Startup Time: {0} seconds",
               new object[] {EQSettings._DelayEQTime});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -   Smooth EQ Amplitude Decay: {0}",
               new object[] {EQSettings.SmoothEQ});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -   Show Track Info with EQ display: {0}",
               new object[] {EQSettings.EQTitleDisplay});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -     Show Track Info Interval: {0} seconds",
               new object[] {EQSettings._EQTitleDisplayTime});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -     Show Track Info duration: {0} seconds",
               new object[] {EQSettings._EQTitleShowTime});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options - Blank display with video: {0}",
               new object[] {DisplaySettings.BlankDisplayWithVideo});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -   Enable Display on Action: {0}",
               new object[] {DisplaySettings.EnableDisplayAction});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -     Enable display for: {0} seconds",
               new object[] {DisplaySettings._DisplayControlTimeout});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options - Blank display when idle: {0}",
               new object[] {DisplaySettings.BlankDisplayWhenIdle});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options -     blank display after: {0} seconds",
               new object[] {DisplaySettings._BlankIdleTimeout/0xf4240L});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Advanced options - Use Clock on shutdown: {0}",
               new object[] {_UseClockOnShutdown});
      Log.Info("VLSYS_Mplay.LoadAdvancedSettings(): Setting - Audio using ASIO: {0}",
               new object[] {EQSettings._AudioUseASIO});
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_vlsys_mplay.xml"));
      SettingsLastModTime = info.LastWriteTime;
      LastSettingsCheck = DateTime.Now;
    }

    private void OnExternalAction(Action action)
    {
      if (DisplaySettings.EnableDisplayAction)
      {
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.OnExternalAction(): received action {0}", new object[] {action.wID.ToString()});
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
          Log.Info("VLSYS_Mplay.OnExternalAction(): received DisplayControlAction", new object[0]);
        }
        DisplayOn();
      }
    }

    private void RenderEQ(byte[] EqDataArray)
    {
      byte[] buffer = new byte[]
                        {
                          0xa1, 0, 0xa7, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                          0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0
                        };
      byte[] buffer2 = new byte[]
                         {
                           0xa2, 0, 0xa7, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
                           0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0
                         };
      if (EQSettings.UseVUmeter)
      {
        Log.Info("VLSYS_Mplay.RenderEQ(): Rendering VU Meter", new object[0]);
        if (_LastCustomCharacterData != 2)
        {
          commPort.Write(new byte[] {0xad}, 0, 1);
          byte[] buffer4 = new byte[8];
          commPort.Write(buffer4, 0, 8);
          commPort.Write(new byte[] {0, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0}, 0, 8);
          commPort.Write(new byte[] {0, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0}, 0, 8);
          commPort.Write(new byte[] {0, 0x1c, 0x1c, 0x1c, 0x1c, 0x1c, 0x1c, 0}, 0, 8);
          commPort.Write(new byte[] {0, 30, 30, 30, 30, 30, 30, 0}, 0, 8);
          commPort.Write(new byte[] {0, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0}, 0, 8);
          byte[] buffer5 = new byte[8];
          commPort.Write(buffer5, 0, 8);
          byte[] buffer6 = new byte[8];
          commPort.Write(buffer6, 0, 8);
          _LastCustomCharacterData = 2;
          SerialWriteDelay(40);
        }
        Thread.Sleep(40);
        int num = EqDataArray[1];
        int num2 = EqDataArray[2];
        int num3 = 3;
        int num4 = 20;
        if (EQSettings._useVUindicators)
        {
          num3 = 4;
          num4 = 0x13;
          buffer[3] = 0x4c;
          buffer2[3] = 0x52;
        }
        for (int i = 0; i < num4; i++)
        {
          if (num > ((i + 1)*5))
          {
            buffer[num3 + i] = 5;
          }
          else if (num > (i*5))
          {
            buffer[num3 + i] = (byte) (num - (i*5));
          }
          if (num2 > ((i + 1)*5))
          {
            buffer2[num3 + i] = 5;
          }
          else if (num2 > (i*5))
          {
            buffer2[num3 + i] = (byte) (num2 - (i*5));
          }
        }
      }
      else if (!EQSettings.UseVUmeter2)
      {
        if (_LastCustomCharacterData != 3)
        {
          commPort.Write(new byte[] {0xad}, 0, 1);
          commPort.Write(new byte[] {0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}, 0, 8);
          byte[] buffer16 = new byte[8];
          buffer16[7] = 0xff;
          commPort.Write(buffer16, 0, 8);
          byte[] buffer17 = new byte[8];
          buffer17[6] = 0xff;
          buffer17[7] = 0xff;
          commPort.Write(buffer17, 0, 8);
          commPort.Write(new byte[] {0, 0, 0, 0, 0, 0xff, 0xff, 0xff}, 0, 8);
          commPort.Write(new byte[] {0, 0, 0, 0, 0xff, 0xff, 0xff, 0xff}, 0, 8);
          commPort.Write(new byte[] {0, 0, 0, 0xff, 0xff, 0xff, 0xff, 0xff}, 0, 8);
          commPort.Write(new byte[] {0, 0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}, 0, 8);
          commPort.Write(new byte[] {0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}, 0, 8);
          _LastCustomCharacterData = 3;
          SerialWriteDelay(40);
        }
        Thread.Sleep(40);
        int num14 = 5;
        if (EQSettings.UseStereoEq)
        {
          num14 = 4;
        }
        for (int j = 0; j < 0x10; j++)
        {
          int num16 = 0;
          if (EQSettings.UseStereoEq && (j > 7))
          {
            num16 = 2;
          }
          byte num17 = EqDataArray[1 + j];
          if (num17 > 0)
          {
            byte num18 = (num17 > 8) ? ((byte) (num17 - 8)) : ((byte) 0x20);
            byte num19 = (num17 < 9) ? num17 : ((byte) 8);
            buffer[(num14 + j) + num16] = num18;
            buffer2[(num14 + j) + num16] = num19;
          }
          else
          {
            buffer[(num14 + j) + num16] = 0x20;
            buffer2[(num14 + j) + num16] = 0x20;
          }
        }
      }
      else
      {
        Log.Info("VLSYS_Mplay.RenderEQ(): Rendering VU Meter 2", new object[0]);
        int num6 = 0;
        int num7 = 0;
        int num8 = EqDataArray[1];
        int num9 = EqDataArray[2];
        int num10 = 3;
        int num11 = 20;
        if (EQSettings._useVUindicators)
        {
          num10 = 4;
          num11 = 0x13;
          buffer[3] = 0x4c;
          buffer2[0x16] = 0x52;
        }
        for (int k = 0; k < num11; k++)
        {
          if (num8 > ((k + 1)*5))
          {
            buffer[num10 + k] = 3;
          }
          else if (num8 > (k*5))
          {
            num6 = num8 - (k*5);
            buffer[num10 + k] = 1;
          }
        }
        for (int m = num11 - 1; m > -1; m--)
        {
          if (num9 > ((num11 - m)*5))
          {
            buffer2[3 + m] = 3;
          }
          else if (num9 > ((num11 - (m + 1))*5))
          {
            num7 = num9 - (m*5);
            buffer2[3 + m] = 2;
          }
        }
        commPort.Write(new byte[] {0xad}, 0, 1);
        byte[] buffer8 = new byte[8];
        commPort.Write(buffer8, 0, 8);
        switch (num6)
        {
          case 0:
            {
              byte[] buffer9 = new byte[8];
              commPort.Write(buffer9, 0, 8);
              break;
            }
          case 1:
            commPort.Write(new byte[] {0, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0}, 0, 8);
            break;

          case 2:
            commPort.Write(new byte[] {0, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0}, 0, 8);
            break;

          case 3:
            commPort.Write(new byte[] {0, 0x1c, 0x1c, 0x1c, 0x1c, 0x1c, 0x1c, 0}, 0, 8);
            break;

          case 4:
            commPort.Write(new byte[] {0, 30, 30, 30, 30, 30, 30, 0}, 0, 8);
            break;

          case 5:
            commPort.Write(new byte[] {0, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0}, 0, 8);
            break;
        }
        switch (num7)
        {
          case 0:
            {
              byte[] buffer10 = new byte[8];
              commPort.Write(buffer10, 0, 8);
              break;
            }
          case 1:
            commPort.Write(new byte[] {0, 1, 1, 1, 1, 1, 1, 0}, 0, 8);
            break;

          case 2:
            commPort.Write(new byte[] {0, 3, 3, 3, 3, 3, 3, 0}, 0, 8);
            break;

          case 3:
            commPort.Write(new byte[] {0, 7, 7, 7, 7, 7, 7, 0}, 0, 8);
            break;

          case 4:
            commPort.Write(new byte[] {0, 15, 15, 15, 15, 15, 15, 0}, 0, 8);
            break;

          case 5:
            commPort.Write(new byte[] {0, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0}, 0, 8);
            break;
        }
        commPort.Write(new byte[] {0, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0}, 0, 8);
        byte[] buffer11 = new byte[8];
        commPort.Write(buffer11, 0, 8);
        byte[] buffer12 = new byte[8];
        commPort.Write(buffer12, 0, 8);
        byte[] buffer13 = new byte[8];
        commPort.Write(buffer13, 0, 8);
        byte[] buffer14 = new byte[8];
        commPort.Write(buffer14, 0, 8);
        SerialWriteDelay(40);
        _LastCustomCharacterData = 4;
      }
      if (DoDebug)
      {
        string str = "";
        string str2 = "";
        for (int n = 0; n < 20; n++)
        {
          str = str + buffer[3 + n].ToString("x00") + " ";
          str2 = str2 + buffer2[3 + n].ToString("x00") + " ";
        }
        Log.Info("VLSYS_Mplay.RenderEQ(): Line 1 data = {0}", new object[] {str});
        Log.Info("VLSYS_Mplay.RenderEQ(): Line 2 data = {0}", new object[] {str2});
      }
      commPort.Write(buffer, 0, 0x18);
      Thread.Sleep(40);
      commPort.Write(buffer2, 0, 0x18);
      Thread.Sleep(40);
    }

    private void RestartMHC()
    {
      if (_ManageMHC && _RestartMHC)
      {
        Log.Info("VLSYS_Mplay.RestartMHC(): called", new object[0]);
        Process process = new Process();
        if ((_MHCWorkingDirectory != string.Empty) && (_MHCFileName != string.Empty))
        {
          process.StartInfo.WorkingDirectory = _MHCWorkingDirectory;
          process.StartInfo.FileName = _MHCFileName;
          Log.Info("VLSYS_Mplay.RestartMHC(): Restarting MHC", new object[0]);
          Process.Start(process.StartInfo);
        }
        else
        {
          Log.Info("VLSYS_Mplay.RestartMHC(): Cannot restart MHC - Process data not available", new object[0]);
        }
        Log.Info("VLSYS_Mplay.RestartMHC(): Completed", new object[0]);
      }
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

    private void SendCustomCharactersToDisplay()
    {
      if (_LastCustomCharacterData != 9)
      {
        Log.Info("VLSYS_Mplay.SendCustomCharactersToDisplay: called", new object[0]);
        commPort.Write(new byte[] {0xad}, 0, 1);
        for (int i = 0; i < 8; i++)
        {
          byte[] buffer = new byte[8];
          for (int j = 0; j < 8; j++)
          {
            buffer[j] = DefaultCustomCharacters[i][j];
          }
          commPort.Write(
            new byte[] {buffer[0], buffer[1], buffer[2], buffer[3], buffer[4], buffer[5], buffer[6], buffer[7]}, 0, 8);
        }
        _LastCustomCharacterData = 9;
        SerialWriteDelay(40);
        Log.Info("VLSYS_Mplay.SendCustomCharactersToDisplay: completed", new object[0]);
      }
    }

    [DllImport("user32.dll")]
    private static extern bool SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

    public void SendShutdownScreen()
    {
      Clear();
      if (_ShutdownOnExit)
      {
        Brightness(0);
      }
      else if ((DisplaySettings._Shutdown1 != string.Empty) || (DisplaySettings._Shutdown2 != string.Empty))
      {
        commPort.Write(new byte[] { 0xa1, 0, 0xa7 }, 0, 3);
        commPort.Write(DisplaySettings._Shutdown1);
        commPort.Write(new byte[] {0, 0xa2, 0, 0xa7 }, 0, 4);
        commPort.Write(DisplaySettings._Shutdown2);
        commPort.Write(new byte[] { 0 }, 0, 1);
      }
      else
      {
        commPort.Write(new byte[] { 0xa1, 0, 0xa7 }, 0, 3);
        commPort.Write("     MediaPortal    ");
        commPort.Write(new byte[] { 0, 0xa2, 0, 0xa7 }, 0, 4);
        commPort.Write("     Not Active     ");
        commPort.Write(new byte[] { 0 }, 0, 1);
      }
    }

    private void SerialWriteDelay(int DelayTimeMS)
    {
      Thread.Sleep(DelayTimeMS);
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
      Log.Info("VLSYS_Mplay.SetCustomCharacters: called", new object[0]);
      if (DefaultCustomCharacters == null)
      {
        byte[][] bufferArray = new byte[8][];
        bufferArray[0] = new byte[8];
        bufferArray[1] = new byte[8];
        bufferArray[2] = new byte[8];
        bufferArray[3] = new byte[8];
        bufferArray[4] = new byte[8];
        bufferArray[5] = new byte[8];
        bufferArray[6] = new byte[8];
        bufferArray[7] = new byte[8];
        DefaultCustomCharacters = bufferArray;
      }
      for (int i = 0; i < customCharacters.Length; i++)
      {
        for (int j = 0; j < customCharacters[i].Length; j++)
        {
          DefaultCustomCharacters[i][j] = (byte) customCharacters[i][j];
        }
      }
      _LastCustomCharacterData = -1;
      SendCustomCharactersToDisplay();
      Log.Info("VLSYS_Mplay.SetCustomCharacters: completed", new object[0]);
    }

    private void SetFanSpeed()
    {
      if (!_UseFans || isDisabled)
      {
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.SetFanSpeed() completed - driver disabled", new object[0]);
        }
        return;
      }

      byte num = FanSpeedByte(_Fan1Speed);
      byte num2 = FanSpeedByte(_Fan2Speed);
      if (DoDebug)
      {
        Log.Info("VLSYS_Mplay.SetFanSpeed() called", new object[0]);
      }

      if (_Fan1Auto)
      {
        if (_TempDataValid)
        {
          num = FanSpeedFromTemp(1, _Temp1);
          Log.Debug("VLSYS_Mplay.SetFanSpeed() Using automatic setting for Fan1: temp = {0}, speed = {1}", _Temp1, num);
        }
        else if (DoDebug)
        {
          Log.Info(
            "VLSYS_Mplay.SetFanSpeed() Using automatic setting for Fan1: temp = {0}, speed = {1} (Temp data not yet valid)",
            new object[] { _Temp1, num });
        }
      }

      if (_Fan2Auto)
      {
        if (_TempDataValid2)
        {
          num2 = FanSpeedFromTemp(2, _Temp2);
          Log.Debug("VLSYS_Mplay.SetFanSpeed() Using automatic setting for Fan2: temp = {0}, speed = {1}", _Temp2, num2);
        }
        else if (DoDebug)
        {
          Log.Info(
            "VLSYS_Mplay.SetFanSpeed() Using automatic setting for Fan2: temp = {0}, speed = {1} (Temp data not yet valid)",
            new object[] { _Temp2, num2 });
        }
      }

      commPort.Write(new byte[] { 0xac, num, num2 }, 0, 3);

      if (DoDebug)
      {
        Log.Info("VLSYS_Mplay.SetFanSpeed() completed", new object[0]);
      }
    }

    public void SetLine(int line, string message)
    {
      if (!isDisabled && _EnableDisplay)
      {
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.SetLine() Called", new object[0]);
        }
        UpdateAdvancedSettings();
        if (EQSettings._EqDataAvailable || _IsDisplayOff)
        {
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.SetLine(): Suppressing display update!", new object[0]);
          }
        }
        else
        {
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.SetLine(): Line {0} - Message = \"{1}\"", new object[] { line, message });
          }
          //SendCustomCharactersToDisplay();
          commPort.Write(new byte[] { 0 }, 0, 1);
          if (line == 0)
          {
            commPort.Write(new byte[] { 0xa1, 0, 0xa7 }, 0, 3);
          }
          else if (line == 1)
          {
            commPort.Write(new byte[] { 0xa2, 0, 0xa7 }, 0, 3);
          }
          else
          {
            Log.Error("VLSYS_Mplay.SetLine: error bad line number" + line, new object[0]);
            return;
          }

          if (MPlay_Model.Equals("MZ4"))
          {
            // Another output method
            int len = message.Length;
            byte[] buffer_str = new byte[len + 1];
            for (int i = 0; i < len; i++)
            {
              buffer_str[i] = TranslateChar(message[i]);
            }
            buffer_str[len] = 0; // Finishing zero
            commPort.Write(buffer_str, 0, len + 1); // Output
          }
          else
          {
            commPort.Write(message);
            commPort.Write(new byte[] { 0 }, 0, 1);
          }
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.SetLine(): message sent to display", new object[0]);
          }
        }
        if ((line == 0) && MPStatus.MP_Is_Idle)
        {
          if (DoDebug)
          {
            Log.Info("VLSYS_Mplay.SetLine(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}",
                     new object[] {DisplaySettings.BlankDisplayWhenIdle, DisplaySettings._BlankIdleTimeout});
          }
          if (DisplaySettings.BlankDisplayWhenIdle)
          {
            if (!_mpIsIdle)
            {
              if (DoDebug)
              {
                Log.Info("VLSYS_Mplay.SetLine(): MP going IDLE", new object[0]);
              }
              DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
            }
            if (!_IsDisplayOff &&
                ((DateTime.Now.Ticks - DisplaySettings._BlankIdleTime) > DisplaySettings._BlankIdleTimeout))
            {
              if (DoDebug)
              {
                Log.Info("VLSYS_Mplay.SetLine(): Blanking display due to IDLE state", new object[0]);
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
              Log.Info("VLSYS_Mplay.SetLine(): MP no longer IDLE - restoring display", new object[0]);
            }
            DisplayOn();
          }
          _mpIsIdle = false;
        }
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.SetLine() completed", new object[0]);
        }
        GetTempReading();
      }
    }

    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _backLightControl, int _backlightLevel, bool _contrastControl, int _contrastLevel,
                      bool _blankOnExit)
    {
      DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      Log.Info("{0}", new object[] {Description});
      Log.Info("VLSYS_Mplay.Setup(): called", new object[0]);
      MiniDisplayHelper.InitEQ(ref EQSettings);
      MiniDisplayHelper.InitDisplayControl(ref DisplaySettings);
      InitRemoteSettings(ref RemoteSettings);
      _ShutdownOnExit = _blankOnExit;
      _UseBrightness = _backLightControl;
      _Brightness = _backlightLevel;
      _EnableDisplay = false;
      LoadAdvancedSettings();
      bool flag = false;
      _UseRemote = false;
      Port = _port;
      if (!RemoteSettings.DisableRemote)
      {
        try
        {
          if (TestXmlVersion(Config.GetFile(Config.Dir.CustomInputDefault, "VLSYS_Mplay.xml")) < 3)
          {
            Log.Info("VLSYS_Mplay.Setup(): Deleting VLSYS_Mplay mapping file with the wrong version stamp.",
                     new object[0]);
            File.Delete(Config.GetFile(Config.Dir.CustomInputDefault, "VLSYS_Mplay.xml"));
          }
          if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "VLSYS_Mplay.xml")))
          {
            Log.Info("VLSYS_Mplay.Setup(): Creating default VLSYS_Mplay mapping file", new object[0]);
            if (!AdvancedSettings.CreateDefaultRemoteMapping())
            {
              Log.Info("VLSYS_Mplay.Setup(): ERROR Creating default VLSYS_Mplay mapping file", new object[0]);
              flag = false;
            }
            else
            {
              flag = true;
            }
          }
          else
          {
            flag = true;
          }
        }
        catch (Exception exception)
        {
          Log.Info("VLSYS_Mplay.Setup(): CAUGHT EXCEPTION while loading InputHander - {0}", new object[] {exception});
          flag = false;
          _UseRemote = false;
        }
        if (flag)
        {
          Log.Info("VLSYS_Mplay.Setup(): Loading InputHandler", new object[0]);
          _inputHandler = new InputHandler("VLSYS_Mplay");
          Log.Info("VLSYS_Mplay.Setup(): InputHandler loaded = {0}", new object[] {_inputHandler.IsLoaded});
          if (_inputHandler.IsLoaded)
          {
            _UseRemote = true;
          }
          else
          {
            _UseRemote = false;
            Log.Info("VLSYS_Mplay.Setup(): error loading InputHandler - remote support disabled", new object[0]);
          }
        }
        else
        {
          Log.Info("VLSYS_Mplay.Setup(): Remote support disabled - no remote mapping file", new object[0]);
          _UseRemote = false;
        }
        if (!_UseRemote || !_inputHandler.IsLoaded)
        {
          Log.Info("VLSYS_Mplay.Setup(): Error loading remote mapping file - Remote support disabled", new object[0]);
          _UseRemote = false;
        }
      }
      else
      {
        _UseRemote = false;
      }
      if (EQSettings.UseEqDisplay || DisplaySettings.BlankDisplayWithVideo)
      {
        _EqThread = new Thread(new ThreadStart(EQ_Update));
        _EqThread.IsBackground = true;
        _EqThread.Priority = ThreadPriority.BelowNormal;
        _EqThread.Name = "EQ_Update";
        _EqThread.Start();
        if (_EqThread.IsAlive)
        {
          Log.Info("VLSYS_Mplay.Setup(): EQ_Update() Thread Started", new object[0]);
        }
        else
        {
          Log.Info("VLSYS_Mplay.Setup(): EQ_Update() FAILED TO START", new object[0]);
        }
      }
      Log.Info("VLSYS_Mplay.Setup() completed", new object[0]);
    }

    private void ShutdownFans()
    {
      if (_UseFans)
      {
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.ShutdownFans() called", new object[0]);
        }
        int num = 0;
        int num2 = 0;
        if (_Fan1Auto)
        {
          if (_Fan1_AutoMS)
          {
            num = FanSpeedByte(_Fan1Speed);
          }
          else
          {
            num = FanSpeedByte(0);
          }
          if (DoDebug)
          {
            Log.Info(
              "VLSYS_Mplay.ShutdownFans() Fan #1 Auto mode = TRUE - use manual speed on exit = {0} - speed = {1}",
              new object[] {_Fan1_AutoMS, num.ToString("x0")});
          }
        }
        if (_Fan2Auto)
        {
          if (_Fan2_AutoMS)
          {
            num2 = FanSpeedByte(_Fan2Speed);
          }
          else
          {
            num2 = FanSpeedByte(0);
          }
          if (DoDebug)
          {
            Log.Info(
              "VLSYS_Mplay.ShutdownFans() Fan #2 Auto mode = TRUE - use manual speed on exit = {0} - speed = {1}",
              new object[] {_Fan2_AutoMS, num2.ToString("x00")});
          }
        }
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.ShutdownFans() Setting fan speed - Fan #1 = {0}, Fan #2 = {1}",
                   new object[] {num.ToString("x00"), num2.ToString("x00")});
        }
        commPort.Write(new byte[] {0xac, (byte) num, (byte) num2}, 0, 3);
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.ShutdownFans() completed", new object[0]);
        }
      }
    }

    private void TerminateMHC()
    {
      if (_ManageMHC)
      {
        bool flag = false;
        _RestartMHC = false;
        Process[] processesByName = Process.GetProcessesByName("MHC");
        int num = 0x5dc;
        Log.Info("VLSYS_Mplay.TerminateMHC(): Found {0} instances of M.Play Home Center",
                 new object[] {processesByName.Length});
        if (processesByName.Length > 0)
        {
          _MHCFileName = processesByName[0].MainModule.FileName;
          _MHCWorkingDirectory = Path.GetDirectoryName(_MHCFileName);
          Log.Info("VLSYS_Mplay.TerminateMHC(): MHC.exe path = \"{0}\"", new object[] {_MHCFileName});
          _RestartMHC = true;
          flag = false;
          Log.Info("VLSYS_Mplay.TerminateMHC(): Closing M.Play Home Center", new object[0]);
          num = 0xdac;
          SendMessage(processesByName[0].Handle, 0x112, 0xf060, 0);
          processesByName[0].Refresh();
          Log.Info("VLSYS_Mplay.TerminateMHC(): M.Play Home Center - responding = {0}",
                   new object[] {processesByName[0].Responding});
          while (!flag)
          {
            Log.Info("VLSYS_Mplay.TerminateMHC(): Waiting for MHC to close ({0}ms remaining)", new object[] {num});
            Thread.Sleep(100);
            num -= 100;
            processesByName[0].Dispose();
            processesByName = Process.GetProcessesByName("MHC");
            if (processesByName.Length == 0)
            {
              flag = true;
              break;
            }
            if (num < 90)
            {
              Log.Info("VLSYS_Mplay.TerminateMHC(): Close timeout expired", new object[0]);
              break;
            }
            SendMessage(processesByName[0].Handle, 0x112, 0xf060, 0);
          }
          if (flag)
          {
            Log.Info("VLSYS_Mplay.TerminateMHC(): M.Play Home Center has closed!", new object[0]);
          }
          else
          {
            Log.Info("VLSYS_Mplay.TerminateMHC(): Killing M.Play Home Center", new object[0]);
            num = 0x5dc;
            processesByName[0].Dispose();
            processesByName = Process.GetProcessesByName("MHC");
            if (processesByName.Length > 0)
            {
              processesByName[0].Kill();
              flag = false;
              while (!flag)
              {
                Log.Info("VLSYS_Mplay.TerminateMHC(): Waiting for MHC to die ({0}ms remaining)", new object[] {num});
                Thread.Sleep(100);
                processesByName[0].Dispose();
                processesByName = Process.GetProcessesByName("MHC");
                if (processesByName.Length == 0)
                {
                  flag = true;
                  break;
                }
                num -= 100;
                if (num < 90)
                {
                  Log.Info("VLSYS_Mplay.TerminateMHC(): kill timeout expired", new object[0]);
                  break;
                }
              }
              if (flag)
              {
                Log.Info("VLSYS_Mplay.TerminateMHC(): M.Play Home Center has died!", new object[0]);
              }
              else
              {
                Log.Info("VLSYS_Mplay.TerminateMHC(): ERROR: Unable to close or kill M.Play Home Center!!",
                         new object[0]);
                _RestartMHC = false;
              }
            }
          }
          Log.Info("VLSYS_Mplay.TerminateMHC(): completed", new object[0]);
        }
        if (flag)
        {
          Thread.Sleep(0x3e8);
        }
      }
    }

    public int TestXmlVersion(string xmlPath)
    {
      if (!File.Exists(xmlPath))
      {
        return 3;
      }
      XmlDocument document = new XmlDocument();
      document.Load(xmlPath);
      return Convert.ToInt32(document.DocumentElement.SelectSingleNode("/mappings").Attributes["version"].Value);
    }

    private byte TranslateChar(char ch)
    {
      int ch32 = ch;

      if (ch32 < 256) return (byte)ch32;

      switch (ch32)
      {
        case 0x410: // Ð_Ð°
        case 0x430: return (byte)('A');
        case 0x411: // Ð'Ð+;
        case 0x431: return 128;
        case 0x412: // Ð'Ð_;
        case 0x432: return (byte)('B');
        case 0x413: // Ð"Ð_;
        case 0x433: return 146;
        case 0x414: // Ð"Ð_;
        case 0x434: return 129;
        case 0x415: // ÐÐ÷;
        case 0x435: return (byte)('E');
        case 0x416: // Ð-Ð¶;
        case 0x436: return 130;
        case 0x417: // Ð-Ð·;
        case 0x437: return 131;
        case 0x418: // Ð_Ð¸;
        case 0x438: return 132;
        case 0x419: // ÐTÐ¹;
        case 0x439: return 133;
        case 0x41a: // Ð_Ðº;
        case 0x43a: return (byte)('K');
        case 0x41b: // Ð>Ð>;
        case 0x43b: return 134;
        case 0x41c: // Ð_Ð_;
        case 0x43c: return (byte)('M');
        case 0x41d: // Ð_Ð_;
        case 0x43d: return (byte)('H');
        case 0x41e: // Ð_Ð_;
        case 0x43e: return (byte)('O');
        case 0x41f: // Ð_Ð¿;
        case 0x43f: return 135;
        case 0x420: // Ð Ñ_;
        case 0x440: return (byte)('P');
        case 0x421: // Ð¡Ñ_;
        case 0x441: return (byte)('C');
        case 0x422: // Ð¢Ñ';
        case 0x442: return (byte)('T');
        case 0x423: // Ð_Ñ_;
        case 0x443: return 136;
        case 0x424: // Ð¤Ñ";
        case 0x444: return 216;
        case 0x425: // Ð_Ñ:;
        case 0x445: return (byte)('X');
        case 0x426: // Ð¦Ñ+;
        case 0x446: return 137;
        case 0x427: // Ð§Ñ+;
        case 0x447: return 138;
        case 0x428: // Ð¨Ñ_;
        case 0x448: return 139;
        case 0x429: // ÐcÑ%;
        case 0x449: return 140;
        case 0x42a: // ÐªÑ_;
        case 0x44a: return 141;
        case 0x42b: // Ð<Ñ<;
        case 0x44b: return 142;
        case 0x42c: // Ð¬Ñ_;
        case 0x44c: return (byte)('b');
        case 0x42d: // Ð-Ñ_;
        case 0x44d: return 143;
        case 0x42e: // ÐRÑ_;
        case 0x44e: return 172;
        case 0x42f: // Ð¯Ñ_;
        case 0x44f: return 173;

      }
      return 32; // For unknown characters
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.UpdateAdvancedSettings(): called", new object[0]);
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_vlsys_mplay.xml")))
        {
          FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_vlsys_mplay.xml"));
          if (info.LastWriteTime.Ticks > SettingsLastModTime.Ticks)
          {
            if (DoDebug)
            {
              Log.Info("VLSYS_Mplay.UpdateAdvancedSettings(): updating advanced settings", new object[0]);
            }
            LoadAdvancedSettings();
          }
        }
        if (DoDebug)
        {
          Log.Info("VLSYS_Mplay.UpdateAdvancedSettings(): completed", new object[0]);
        }
      }
    }

    private void GetDisplayType()
    {
      int bytesToRead = commPort.BytesToRead;
      if (bytesToRead > 2)
      {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < 3; i++)
        {
          builder.Append((char)commPort.ReadChar());
        }
        string cModel = builder.ToString();
        if (IsValidModel(cModel))
        {
          MPlay_Model = cModel;
          Log.Info("VLSYS_Mplay.WhenDataReceived(): Received Model ID = \"{0}\"", new object[] { MPlay_Model });
        }
        else
        {
          Log.Info("VLSYS_Mplay.WhenDataReceived(): received invalid Model ID \"{0}\"- ignored",
                   new object[] { cModel.ToString() });
        }
      }
      else
      {
        Log.Info("VLSYS_Mplay.WhenDataReceived(): Not processing data - ignored {0} bytes",
                 new object[] { bytesToRead });
      }
    }

    private void WhenDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      int byteCount;
      int[] receivedBytes;
      if (!isDisabled && commPort.IsOpen)
      {
        if (_ProcessReceivedData)
        {
          lock (CommReadLock)
          {
            if (_FlushDataBuffers)
            {
              Log.Info("VLSYS_Mplay.WhenDataReceived(): flushing data buffers - starting", new object[0]);
              while (commPort.BytesToRead > 0)
              {
                commPort.DiscardInBuffer();
              }
              while (commPort.BytesToWrite > 0)
              {
                commPort.DiscardOutBuffer();
              }
              _FlushDataBuffers = false;
              Log.Info("VLSYS_Mplay.WhenDataReceived(): flushing data buffers - finished", new object[0]);
              return;
            }            
            if (DoDebug)
            {
              Log.Info("VLSYS_Mplay.WhenDataReceived(): Got DataReceived event", new object[0]);
            }
            try
            {
              byteCount = commPort.BytesToRead;
              if (byteCount == 0)
              {
                if (DoDebug)
                {
                  Log.Info("VLSYS_Mplay: Got DataReceived event with no data", new object[0]);
                }
                return;
              }
              receivedBytes = new int[byteCount];
              for (int count = 0; count < byteCount; count++)
              {
                receivedBytes[count] = commPort.ReadByte();
              }
            }
            catch
            {
              if (DoDebug)
              {
                Log.Info("VLSYS_Mplay.WhenDataReceived(): error reading from port - driver disabled", new object[0]);
              }
              isDisabled = true;
              return;
            }            

            if (DoDebug)
            {
              Log.Info("VLSYS_Mplay: Got DataReceived event - {0} bytes of data", new object[] { byteCount });
            }
            try
            {
              if (_UseRemote && !_inputHandler.IsLoaded)
              {
                if (DoDebug)
                {
                  Log.Info("VLSYS_Mplay.WhenDataReceived(): Unable to process incoming data - NO REMOTE MAPPING", new object[] { byteCount });
                }
                commPort.DiscardInBuffer();
                isDisabled = true;
                return;
              }
            }
            catch
            {
              Log.Info("VLSYS_Mplay.WhenDataReceived(): CAUGHT EXCEPTION - NO REMOTE MAPPING", new object[] { byteCount });
              isDisabled = true;
              return;
            }            
            
            if ((commPort.BytesToRead > 0) && DoDebug)
            {
              Log.Info("VLSYS_Mplay.WhenDataReceived(): new data arrived after notification... leaving for next read cycle", new object[0]);
            }
            
            if (DoDebug)
            {
              string str2 = string.Empty;
              for (int count = 0; count < byteCount; count++)
              {
               str2 = str2 + "0x" + ((byte)receivedBytes[count]).ToString("x00") + " ";
              }
              Log.Info("VLSYS_Mplay.WhenDataReceived(): Received RAW DATA: {0}", new object[] { str2 });
            }

            for (int count = 0; count < byteCount; count++)
            {
              int num12 = receivedBytes[count];
              int temp = 0;
              if (DoDebug)
              {
                Log.Info("VLSYS_Mplay.WhenDataReceived(): Received RAW REMOTE DATA: 0x{0} 0x{1}",
                         new object[] { num12.ToString("x00") });
              }
              if (IsRemoteKeyCode(num12) && (num12 != 0x7e))
              {
                if (!_UseRemote)
                {
                  if (DoDebug)
                  {
                    Log.Info("VLSYS_Mplay.WhenDataReceived(): Remote disabled - ignoring", new object[0]);
                  }
                }
                else
                {
                  FireRemoteEvent(num12);
                  if (DoDebug)
                  {
                    Log.Info("VLSYS_Mplay.WhenDataReceived(): Received Remote Button : code = {0}",
                             new object[] { num12.ToString("x00") });
                  }
                  _RemoteButtonPending = num12;
                }
              }
              else if (num12 == 0x7e)
              {
                if (IsRemoteKeyCode(_RemoteButtonPending))
                {
                  _RemoteButtonPending = 0xff;
                  if (DoDebug)
                  {
                    Log.Info(
                      "VLSYS_Mplay.WhenDataReceived(): Received spurious REPEAT attached to key code - discarding",
                      new object[0]);
                  }
                }
                else if (IsRemoteKeyCode(_LastRemoteButton))
                {
                  if (DateTime.Now.Ticks >
                      _LastRemoteButtonTimestamp.AddMilliseconds((double)RemoteSettings.RepeatDelay).
                        Ticks)
                  {
                    if ((DateTime.Now.Ticks >
                         _LastRemoteButtonTimestamp.AddMilliseconds(RemoteSettings.RepeatDelay * 2.5).Ticks) &&
                        (RemoteSettings.RepeatDelay > 0))
                    {
                      if (DoDebug)
                      {
                        Log.Info(
                          "VLSYS_Mplay.WhenDataReceived(): Received Remote Button : spurious REPEAT (excessive delay) - discarding",
                          new object[0]);
                      }
                    }
                    else
                    {
                      if (DoDebug)
                      {
                        Log.Info(
                          "VLSYS_Mplay.WhenDataReceived(): Received Remote Button : REPEAT - using code = 0x{0}",
                          new object[] { _LastRemoteButton.ToString("x00") });
                      }
                      try
                      {
                        if (_UseRemote)
                        {
                          FireRemoteEvent(_LastRemoteButton);
                        }
                      }
                      catch
                      {
                        Log.Info(
                          "VLSYS_Mplay.WhenDataReceived(): CAUGHT EXCEPTION: Unable to process Remote Button 0x{0}",
                          new object[] { num12.ToString("x00") });
                      }
                    }
                  }
                  else if (DoDebug)
                  {
                    Log.Info(
                      "VLSYS_Mplay.WhenDataReceived(): discarding REPEAT event - event received before repeat timeout ({0}ms) elapsed",
                      new object[] { RemoteSettings.RepeatDelay });
                  }
                }
              }
              else if (IsTemperatureCode(num12))
              {
                if ((MPlay_Model.Equals("ME7") || MPlay_Model.Equals("MP7")) ||
                    MPlay_Model.Equals("MR2"))
                {
                  if ((num12 != 0xff) && (num12 != 0xfe))
                  {
                    temp = num12 - 0x6c;
                  }
                }
                else
                {
                  temp = num12 - 150;
                }
                if (DoDebug)
                {
                  Log.Info("VLSYS_Mplay.WhenDataReceived(): Received temperature data: temp{0} = {1}", _TempIndex, temp);
                }
                if (_TempIndex == 0)
                {
                  _Temp1 = temp;
                  _TempDataValid = true;
                  if (TempCount > 1)
                  {
                    _TempIndex++;
                  }
                  else
                  {
                    _Temp2 = temp;
                    _TempDataValid2 = true;
                  }
                }
                else
                {
                  _Temp2 = temp;
                  _TempDataValid2 = true;
                  _TempIndex = 0;
                }
              }
            }
            if (DoDebug)
            {
              Log.Info("VLSYS_Mplay.WhenDataReceived(): completed", new object[0]);
            }
          }
        }
        else
        {
          GetDisplayType();
        }
      }
    }

    public string Description
    {
      get { return "VL System Mplay/LIS2 driver"; }
    }

    public string ErrorMessage
    {
      get
      {
        Log.Info("VLSYS_Mplay.ErrorMessage: called", new object[0]);
        return errorMessage;
      }
    }

    public bool IsDisabled
    {
      get
      {
        Log.Info("VLSYS_Mplay.IsDisabled: called", new object[0]);
        return isDisabled;
      }
    }

    public string Name
    {
      get { return "VLSYS_Mplay"; }
    }

    public bool SupportsGraphics
    {
      get { return false; }
    }

    public bool SupportsText
    {
      get { return true; }
    }

    [Serializable]
    public class AdvancedSettings
    {
      private bool m_BlankDisplayWhenIdle;
      private bool m_BlankDisplayWithVideo;
      private int m_BlankIdleTime = 30;
      private bool m_DelayEQ;
      private int m_DelayEqTime = 10;
      private string m_DeviceType = "AUTOMATIC";
      private bool m_DisableRemote;
      private bool m_DisableRepeat;
      private bool m_EnableDisplayAction;
      private int m_EnableDisplayActionTime = 5;
      private bool m_EqDisplay;
      private int m_EqRate = 10;
      private bool m_EQTitleDisplay;
      private int m_EQTitleDisplayTime = 10;
      private int m_EQTitleShowTime = 2;
      private int m_Fan1;
      private bool m_Fan1_AutoMS;
      private int m_Fan1_SetOff = 9;
      private int m_Fan1_SetOn = 0x27;
      private bool m_Fan1Auto;
      private int m_Fan2;
      private bool m_Fan2_AutoMS;
      private int m_Fan2_SetOff = 9;
      private int m_Fan2_SetOn = 0x27;
      private bool m_Fan2Auto;
      private static AdvancedSettings m_Instance;
      private bool m_ManageMHC;
      private bool m_NormalEQ;
      private int m_RepeatDelay;
      private bool m_RestrictEQ;
      private bool m_SmoothEQ;
      private bool m_StereoEQ;
      private bool m_UseClockOnShutdown;
      private bool m_UseFans;
      private bool m_VUindicators;
      private bool m_VUmeter;
      private bool m_VUmeter2;

      public static event OnSettingsChangedHandler OnSettingsChanged;

      public static bool CreateDefaultRemoteMapping()
      {
        Log.Info("VLSYS_Mplay.AdvancedSettings.CreateDefaultRemoteMapping(): called", new object[0]);
        bool flag = false;
        string str = "VLSYS_Mplay";
        try
        {
          Log.Info(
            "VLSYS_Mplay.AdvancedSettings.CreateDefaultRemoteMapping(): remote mapping file does not exist - Creating default mapping file",
            new object[0]);
          XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.CustomInputDefault, str + ".xml"),
                                                   Encoding.UTF8);
          writer.Formatting = Formatting.Indented;
          writer.Indentation = 1;
          writer.IndentChar = '\t';
          writer.WriteStartDocument(true);
          writer.WriteStartElement("mappings");
          writer.WriteAttributeString("version", "3");
          writer.WriteStartElement("remote");
          writer.WriteAttributeString("family", str);
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "PwrOff");
          writer.WriteAttributeString("code", "65");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "POWER");
          writer.WriteAttributeString("cmdproperty", "EXIT");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Movies");
          writer.WriteAttributeString("code", "64");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "6");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2005");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "6");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "eHome");
          writer.WriteAttributeString("code", "85");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "0");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteAttributeString("focus", "true");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My TV");
          writer.WriteAttributeString("code", "70");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "603");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Photos");
          writer.WriteAttributeString("code", "69");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "2");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Music");
          writer.WriteAttributeString("code", "86");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "501");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "1");
          writer.WriteAttributeString("code", "77");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "37");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "49");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "2");
          writer.WriteAttributeString("code", "78");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "38");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "50");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "3");
          writer.WriteAttributeString("code", "79");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "39");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "51");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "4");
          writer.WriteAttributeString("code", "80");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "40");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "52");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "5");
          writer.WriteAttributeString("code", "81");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "41");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "53");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "6");
          writer.WriteAttributeString("code", "82");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "42");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "54");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "7");
          writer.WriteAttributeString("code", "83");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "43");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "55");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "8");
          writer.WriteAttributeString("code", "3");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "44");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "56");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "9");
          writer.WriteAttributeString("code", "7");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "45");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "57");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "0");
          writer.WriteAttributeString("code", "76");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "25");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "603");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "605");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "606");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "501");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "601");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "759");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "6");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "10");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "48");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "11");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "48");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "88");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "48");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "VOL+");
          writer.WriteAttributeString("code", "10");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "103");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "VOL-");
          writer.WriteAttributeString("code", "14");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "102");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "CH+/Lang");
          writer.WriteAttributeString("code", "18");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9979");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9979");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "31");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "95");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "95");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "5");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "CH-/Page");
          writer.WriteAttributeString("code", "22");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9980");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9980");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "30");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "94");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "94");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "6");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Back");
          writer.WriteAttributeString("code", "11");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Guide");
          writer.WriteAttributeString("code", "15");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "600");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Live TV");
          writer.WriteAttributeString("code", "19");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "602");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "1");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "UP");
          writer.WriteAttributeString("code", "25");
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
          writer.WriteAttributeString("code", "29");
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
          writer.WriteAttributeString("code", "67");
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
          writer.WriteAttributeString("code", "84");
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
          writer.WriteAttributeString("code", "66");
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
          writer.WriteAttributeString("name", "Exit/Click");
          writer.WriteAttributeString("code", "31");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Run/D.Click");
          writer.WriteAttributeString("code", "27");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Task/Quick");
          writer.WriteAttributeString("code", "23");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "F.Rew");
          writer.WriteAttributeString("code", "13");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "87");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "29");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "17");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Play");
          writer.WriteAttributeString("code", "9");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "105");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "68");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "F.Fwd");
          writer.WriteAttributeString("code", "21");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "86");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "28");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "16");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Prev");
          writer.WriteAttributeString("code", "26");
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
          writer.WriteAttributeString("name", "Stop");
          writer.WriteAttributeString("code", "1");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "13");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Next");
          writer.WriteAttributeString("code", "30");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "PLAYER");
          writer.WriteAttributeString("conproperty", "DVD");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "91");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "5");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "28");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2005");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "91");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "14");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Pause");
          writer.WriteAttributeString("code", "5");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "105");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "12");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Mute");
          writer.WriteAttributeString("code", "74");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9982");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Warp/Mouse");
          writer.WriteAttributeString("code", "71");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Rec");
          writer.WriteAttributeString("code", "17");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "501");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "113");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "89");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "DVD/Menu");
          writer.WriteAttributeString("code", "20");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "PLAYER");
          writer.WriteAttributeString("conproperty", "DVD");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "90");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "3001");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Detail");
          writer.WriteAttributeString("code", "75");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "FULLSCREEN");
          writer.WriteAttributeString("conproperty", "true");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "24");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "106");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndDocument();
          writer.Close();
          flag = true;
          Log.Info("VLSYS_Mplay.AdvancedSettings.CreateDefaultRemoteMapping: remote mapping file created", new object[0]);
        }
        catch
        {
          Log.Info("VLSYS_Mplay.AdvancedSettings.CreateDefaultRemoteMapping: Error saving remote mapping to XML file",
                   new object[0]);
          flag = false;
        }
        Log.Info("VLSYS_Mplay.AdvancedSettings.CreateDefaultRemoteMapping: completed", new object[0]);
        return flag;
      }

      private static void Default(AdvancedSettings _settings)
      {
        _settings.DeviceType = "AUTOMATIC";
        _settings.ManageMHC = false;
        _settings.DisableRemote = true;
        _settings.DisableRepeat = false;
        _settings.RepeatDelay = 0;
        _settings.UseFans = false;
        _settings.Fan1 = 0;
        _settings.Fan2 = 0;
        _settings.Fan1Auto = false;
        _settings.Fan1_SetOff = 9;
        _settings.Fan1_SetOn = 0x27;
        _settings.Fan1_AutoMS = false;
        _settings.Fan2Auto = false;
        _settings.Fan2_SetOff = 9;
        _settings.Fan2_SetOn = 0x27;
        _settings.Fan2_AutoMS = false;
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
        Log.Info("VLSYS_Mplay.AdvancedSettings.Load(): started", new object[0]);
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_vlsys_mplay.xml")))
        {
          Log.Info("VLSYS_Mplay.AdvancedSettings.Load(): Loading settings from XML file", new object[0]);
          XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_vlsys_mplay.xml"));
          settings = (AdvancedSettings) serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Info("VLSYS_Mplay.AdvancedSettings.Load(): Loading settings from defaults", new object[0]);
          settings = new AdvancedSettings();
          Default(settings);
        }
        Log.Info("VLSYS_Mplay.AdvancedSettings.Load(): completed", new object[0]);
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
        Log.Info("VLSYS_Mplay.AdvancedSettings.Save(): Saving settings to XML file", new object[0]);
        XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_vlsys_mplay.xml"),
                                                 Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter) writer, ToSave);
        writer.Close();
        Log.Info("VLSYS_Mplay.AdvancedSettings.Save(): completed", new object[0]);
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
      public string DeviceType
      {
        get { return m_DeviceType; }
        set { m_DeviceType = value; }
      }

      [XmlAttribute]
      public bool DisableRemote
      {
        get { return m_DisableRemote; }
        set { m_DisableRemote = value; }
      }

      [XmlAttribute]
      public bool DisableRepeat
      {
        get { return m_DisableRepeat; }
        set { m_DisableRepeat = value; }
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

      [XmlAttribute]
      public int Fan1
      {
        get { return m_Fan1; }
        set { m_Fan1 = value; }
      }

      [XmlAttribute]
      public bool Fan1_AutoMS
      {
        get { return m_Fan1_AutoMS; }
        set { m_Fan1_AutoMS = value; }
      }

      [XmlAttribute]
      public int Fan1_SetOff
      {
        get { return m_Fan1_SetOff; }
        set { m_Fan1_SetOff = value; }
      }

      [XmlAttribute]
      public int Fan1_SetOn
      {
        get { return m_Fan1_SetOn; }
        set { m_Fan1_SetOn = value; }
      }

      [XmlAttribute]
      public bool Fan1Auto
      {
        get { return m_Fan1Auto; }
        set { m_Fan1Auto = value; }
      }

      [XmlAttribute]
      public int Fan2
      {
        get { return m_Fan2; }
        set { m_Fan2 = value; }
      }

      [XmlAttribute]
      public bool Fan2_AutoMS
      {
        get { return m_Fan2_AutoMS; }
        set { m_Fan2_AutoMS = value; }
      }

      [XmlAttribute]
      public int Fan2_SetOff
      {
        get { return m_Fan2_SetOff; }
        set { m_Fan2_SetOff = value; }
      }

      [XmlAttribute]
      public int Fan2_SetOn
      {
        get { return m_Fan2_SetOn; }
        set { m_Fan2_SetOn = value; }
      }

      [XmlAttribute]
      public bool Fan2Auto
      {
        get { return m_Fan2Auto; }
        set { m_Fan2Auto = value; }
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
      public bool ManageMHC
      {
        get { return m_ManageMHC; }
        set { m_ManageMHC = value; }
      }

      [XmlAttribute]
      public bool NormalEQ
      {
        get { return m_NormalEQ; }
        set { m_NormalEQ = value; }
      }

      [XmlAttribute]
      public int RepeatDelay
      {
        get { return m_RepeatDelay; }
        set { m_RepeatDelay = value; }
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
      public bool UseClockOnShutdown
      {
        get { return m_UseClockOnShutdown; }
        set { m_UseClockOnShutdown = value; }
      }

      [XmlAttribute]
      public bool UseFans
      {
        get { return m_UseFans; }
        set { m_UseFans = value; }
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

    [StructLayout(LayoutKind.Sequential)]
    public struct RemoteControl
    {
      public bool DisableRemote;
      public bool DisableRepeat;
      public int RepeatDelay;
    }
  }
}