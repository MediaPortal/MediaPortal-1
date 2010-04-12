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
  public class MatrixMX : BaseDisplay, IDisplay
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
    private DisplayControl DisplaySettings;
    private bool DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
    private object DWriteMutex = new object();
    private EQControl EQSettings;
    private object EqWriteMutex = new object();
    private string IdleMessage = string.Empty;
    private KeyPadControl KPSettings;
    private DateTime LastSettingsCheck = DateTime.Now;
    private readonly MODisplay MOD = new MODisplay();
    private SystemStatus MPStatus = new SystemStatus();
    private DateTime SettingsLastModTime;
    private object ThreadMutex = new object();

    private void AdvancedSettings_OnSettingsChanged()
    {
      Log.Info("MatrixMX.AdvancedSettings_OnSettingsChanged(): called");
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
      AdvancedSettings.OnSettingsChanged -=
        new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
      if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
      {
        while (this._EqThread.IsAlive)
        {
          Log.Info("MatrixMX.Cleanup(): Stoping EQ_Update() Thread");
          lock (this.ThreadMutex)
          {
            _stopUpdateEqThread = true;
          }
          _stopUpdateEqThread = true;
          Thread.Sleep(500);
        }
      }
      this.MOD.ClearDisplay();
      if (!this._BlankDisplayOnExit &&
          ((this.DisplaySettings._Shutdown1 != string.Empty) || (this.DisplaySettings._Shutdown2 != string.Empty)))
      {
        this.MOD.SetLine(0, this.DisplaySettings._Shutdown1);
        this.MOD.SetLine(1, this.DisplaySettings._Shutdown2);
      }
      this.MOD.CloseDisplay(this._BlankDisplayOnExit);
    }

    private void Clear()
    {
      this.MOD.ClearDisplay();
    }

    public void Configure()
    {
      Form form = new MatrixMX_AdvancedSetupForm();
      form.ShowDialog();
      form.Dispose();
    }

    private ulong ConvertPluginIconsToDriverIcons(ulong IconMask)
    {
      return 0L;
    }

    private void DisplayEQ()
    {
      if ((this.EQSettings.UseEqDisplay & this.EQSettings._EqDataAvailable) &&
          !(this.EQSettings.RestrictEQ &
            ((DateTime.Now.Ticks - this.EQSettings._LastEQupdate.Ticks) < this.EQSettings._EqUpdateDelay)))
      {
        if (this.DoDebug)
        {
          Log.Info("\nMODisplay.DisplayEQ(): Retrieved {0} samples of Equalizer data.",
                   new object[] {this.EQSettings.EqFftData.Length / 2});
        }
        if (this.EQSettings.UseVUmeter || this.EQSettings.UseVUmeter2)
        {
          this.EQSettings.Render_MaxValue = 100;
          this.EQSettings.Render_BANDS = 1;
          if (this.EQSettings._useVUindicators)
          {
            this.EQSettings.Render_MaxValue = 0x5f;
          }
        }
        else
        {
          this.EQSettings.Render_MaxValue = 0x10;
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
              Log.Info("MODisplay.DisplayOff(): DisplayControlAction Timer = {0}.",
                       new object[] {DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction});
            }
            return;
          }
          if (this.DoDebug)
          {
            Log.Info("MODisplay.DisplayOff(): DisplayControlAction Timeout expired.");
          }
          this.DisplaySettings._DisplayControlAction = false;
          this.DisplaySettings._DisplayControlLastAction = 0L;
        }
        Log.Info("MODisplay.DisplayOff(): completed");
        lock (this.DWriteMutex)
        {
          Log.Info("MODisplay.DisplayOff(): Turning display OFF");
          this.Clear();
          if (this._BackLightControl)
          {
            this.MOD.BacklightOff();
          }
          this._IsDisplayOff = true;
          this.MOD.DisplayOff();
        }
        Log.Info("MODisplay.DisplayOff(): completed");
      }
    }

    private void DisplayOn()
    {
      if (this._IsDisplayOff)
      {
        Log.Info("MODisplay.DisplayOn(): called");
        lock (this.DWriteMutex)
        {
          Log.Info("MODisplay.DisplayOn(): Turning Display ON");
          this.MOD.DisplayOn();
          this.MOD.BacklightOn();
        }
        this._IsDisplayOff = false;
        Log.Info("MODisplay.DisplayOn(): completed");
      }
    }

    public void Dispose()
    {
      this.MOD.CloseDisplay(this._BackLightControl);
    }

    public void DrawImage(Bitmap bitmap) {}

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
            Log.Info("MODisplay.EQ_Update(): Checking for Thread termination request");
          }
          if (_stopUpdateEqThread)
          {
            if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
            {
              GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
            }
            if (this.DoDebug)
            {
              Log.Info("MODisplay.EQ_Update(): EQ_Update Thread terminating");
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
              this.GetEQ();
              this.DisplayEQ();
            }
            if (this.DisplaySettings.BlankDisplayWithVideo &
                (((this.MPStatus.Media_IsDVD || this.MPStatus.Media_IsVideo) || this.MPStatus.Media_IsTV) ||
                 this.MPStatus.Media_IsTVRecording))
            {
              if (this.DoDebug)
              {
                Log.Info("MODisplay.EQ_Update(): Turning off display while playing video");
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
      Log.Info("MatrixMX.LoadAdvancedSettings(): Called");
      AdvancedSettings settings = AdvancedSettings.Load();
      this.IdleMessage = (Settings.Instance.IdleMessage != string.Empty) ? Settings.Instance.IdleMessage : "MediaPortal";
      this.KPSettings.EnableKeyPad = settings.EnableKeypad;
      this.KPSettings.EnableCustom = settings.UseCustomKeypadMap;
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
      this.EQSettings._EqUpdateDelay = (this.EQSettings._EQ_Restrict_FPS == 0)
                                         ? 0
                                         : ((0x989680 / this.EQSettings._EQ_Restrict_FPS) -
                                            (0xf4240 / this.EQSettings._EQ_Restrict_FPS));
      Log.Info("MatrixMX.LoadAdvancedSettings(): Extensive Logging: {0}",
               new object[] {Settings.Instance.ExtensiveLogging});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Device Port: {0}", new object[] {Settings.Instance.Port});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Enable Keypad: {0}", new object[] {this.KPSettings.EnableKeyPad});
      Log.Info("MatrixMX.LoadAdvancedSettings():   Use Custom KeypadMap: {0}",
               new object[] {this.KPSettings.EnableCustom});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Shutdown Message - Line 1: {0}",
               new object[] {this.DisplaySettings._Shutdown1});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Shutdown Message - Line 2: {0}",
               new object[] {this.DisplaySettings._Shutdown2});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options - Equalizer Display: {0}",
               new object[] {this.EQSettings.UseEqDisplay});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -   Stereo Equalizer Display: {0}",
               new object[] {this.EQSettings.UseStereoEq});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -   VU Meter Display: {0}",
               new object[] {this.EQSettings.UseVUmeter});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -   VU Meter Style 2 Display: {0}",
               new object[] {this.EQSettings.UseVUmeter2});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -     Use VU Channel indicators: {0}",
               new object[] {this.EQSettings._useVUindicators});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -   Restrict EQ Update Rate: {0}",
               new object[] {this.EQSettings.RestrictEQ});
      Log.Info(
        "MatrixMX.LoadAdvancedSettings(): Advanced options -     Restricted EQ Update Rate: {0} updates per second",
        new object[] {this.EQSettings._EQ_Restrict_FPS});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -   Delay EQ Startup: {0}",
               new object[] {this.EQSettings.DelayEQ});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -     Delay EQ Startup Time: {0} seconds",
               new object[] {this.EQSettings._DelayEQTime});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -   Smooth EQ Amplitude Decay: {0}",
               new object[] {this.EQSettings.SmoothEQ});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -   Show Track Info with EQ display: {0}",
               new object[] {this.EQSettings.EQTitleDisplay});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -     Show Track Info Interval: {0} seconds",
               new object[] {this.EQSettings._EQTitleDisplayTime});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -     Show Track Info duration: {0} seconds",
               new object[] {this.EQSettings._EQTitleShowTime});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options - Blank display with video: {0}",
               new object[] {this.DisplaySettings.BlankDisplayWithVideo});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -   Enable Display on Action: {0}",
               new object[] {this.DisplaySettings.EnableDisplayAction});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -     Enable display for: {0} seconds",
               new object[] {this.DisplaySettings._DisplayControlTimeout});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options - Blank display when idle: {0}",
               new object[] {this.DisplaySettings.BlankDisplayWhenIdle});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Advanced options -     blank display after: {0} seconds",
               new object[] {this.DisplaySettings._BlankIdleTimeout / 0xf4240L});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Setting - Audio using ASIO: {0}",
               new object[] {this.EQSettings._AudioUseASIO});
      Log.Info("MatrixMX.LoadAdvancedSettings(): Completed");
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixMX.xml"));
      this.SettingsLastModTime = info.LastWriteTime;
      this.LastSettingsCheck = DateTime.Now;
    }

    private void OnExternalAction(Action action)
    {
      if (this.DisplaySettings.EnableDisplayAction)
      {
        if (this.DoDebug)
        {
          Log.Info("MODisplay.OnExternalAction(): received action {0}", new object[] {action.wID.ToString()});
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
          Log.Info("MODisplay.OnExternalAction(): received DisplayControlAction");
        }
        this.DisplayOn();
      }
    }

    private void RenderEQ(byte[] EqDataArray)
    {
      if (this.EQSettings.UseVUmeter || this.EQSettings.UseVUmeter2)
      {
        this.MOD.InitHorizontalBarGraph();
        Thread.Sleep(40);
        if (this.EQSettings._useVUindicators)
        {
          this.MOD.SetLine(0, "L");
          this.MOD.DrawHorizontalBarGraph(1, 0, 0, EqDataArray[1]);
          if (this.EQSettings.UseVUmeter)
          {
            this.MOD.SetLine(1, "R");
            this.MOD.DrawHorizontalBarGraph(1, 1, 0, EqDataArray[2]);
          }
          else
          {
            this.MOD.SetLine(1, "                   R");
            this.MOD.DrawHorizontalBarGraph(0x13, 1, 1, EqDataArray[2]);
          }
        }
        else
        {
          this.MOD.DrawHorizontalBarGraph(0, 0, 0, EqDataArray[1]);
          if (this.EQSettings.UseVUmeter)
          {
            this.MOD.DrawHorizontalBarGraph(0, 1, 0, EqDataArray[2]);
          }
          else
          {
            this.MOD.DrawHorizontalBarGraph(0x13, 1, 1, EqDataArray[2]);
          }
        }
      }
      else
      {
        this.MOD.InitNarrowVerticalBarGraph();
        int num = 2;
        if (this.EQSettings.UseStereoEq)
        {
          num = 1;
        }
        for (int i = 0; i < 0x10; i++)
        {
          int num3 = 0;
          if (this.EQSettings.UseStereoEq && (i > 7))
          {
            num3 = 2;
          }
          this.MOD.DrawVerticalBarGraph((num + i) + num3, EqDataArray[1 + i]);
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

    public void SetCustomCharacters(int[][] customCharacters) {}

    public void SetLine(int line, string message)
    {
      if (this._IsDisabled)
      {
        if (this.DoDebug)
        {
          Log.Info("MatrixMX.SetLine(): Unable to display text - driver disabled");
        }
      }
      else
      {
        this.UpdateAdvancedSettings();
        MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
        if (this.DoDebug)
        {
          Log.Info("MatrixMX.SetLine() Called");
        }
        if (this.EQSettings._EqDataAvailable || this._IsDisplayOff)
        {
          if (this.DoDebug)
          {
            Log.Info("MatrixMX.SetLine(): Suppressing display update!");
          }
        }
        else
        {
          if (this.DoDebug)
          {
            Log.Info("MatrixMX.SetLine(): Line {0} - Message = \"{1}\"", new object[] {line, message});
          }
          this.MOD.SetLine(line, message);
          if (this.DoDebug)
          {
            Log.Info("MatrixMX.SetLine(): message sent to display");
          }
        }
        if ((line == 0) && this.MPStatus.MP_Is_Idle)
        {
          if (this.DoDebug)
          {
            Log.Info("MatrixMX.SetLine(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}",
                     new object[] {this.DisplaySettings.BlankDisplayWhenIdle, this.DisplaySettings._BlankIdleTimeout});
          }
          if (this.DisplaySettings.BlankDisplayWhenIdle)
          {
            if (!this._mpIsIdle)
            {
              if (this.DoDebug)
              {
                Log.Info("MatrixMX.SetLine(): MP going IDLE");
              }
              this.DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
            }
            if (!this._IsDisplayOff &&
                ((DateTime.Now.Ticks - this.DisplaySettings._BlankIdleTime) > this.DisplaySettings._BlankIdleTimeout))
            {
              if (this.DoDebug)
              {
                Log.Info("MatrixMX.SetLine(): Blanking display due to IDLE state");
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
              Log.Info("MatrixMX.SetLine(): MP no longer IDLE - restoring display");
            }
            this.DisplayOn();
          }
          this._mpIsIdle = false;
        }
      }
    }

    public void Setup(string _port, int _lines, int _cols, int _delay, int _linesG, int _colsG, int _delayG,
                      bool _useBackLight, int _useBackLightLevel, bool _useContrast, int _useContrastLevel,
                      bool _blankOnExit)
    {
      this.DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      Log.Info("{0}", new object[] {this.Description});
      Log.Info("MatrixMX.Setup(): called");
      MiniDisplayHelper.InitEQ(ref this.EQSettings);
      MiniDisplayHelper.InitDisplayControl(ref this.DisplaySettings);
      this.InitKeyPadSettings(ref this.KPSettings);
      this._BlankDisplayOnExit = _blankOnExit;
      this._BackLightControl = _useBackLight;
      this._BackLightLevel = _useBackLightLevel;
      this._ContrastControl = _useContrast;
      this._ContrastLevel = _useContrastLevel;
      this.LoadAdvancedSettings();
      this._UseKeypad = false;
      if (this.KPSettings.EnableKeyPad)
      {
        this._UseKeypad = this.MOD.SetupKeypad(this.KPSettings.EnableCustom);
      }
      else
      {
        this._UseKeypad = false;
        Log.Info("MatrixMX.Setup(): Keypad support disabled!");
      }
      this._Trows = _lines;
      if (this._Trows > 2)
      {
        Log.Info("MatrixMX.Setup() - Invalid Text Lines value");
        this._Trows = 2;
      }
      this._Tcols = _cols;
      if (this._Tcols > 20)
      {
        Log.Info("MatrixMX.Setup() - Invalid Text Columns value");
        this._Tcols = 20;
      }
      this._IsOpen = false;
      try
      {
        this._IsOpen = this.MOD.OpenDisplay(_port, _useBackLight, _useBackLightLevel, _useContrast, _useContrastLevel,
                                            this._UseKeypad);
      }
      catch (Exception exception)
      {
        Log.Info("MatrixMX.Setup() - CAUGHT EXCEPTION opening display port!: {0}", new object[] {exception});
      }
      if (this._IsOpen)
      {
        this._IsDisabled = false;
      }
      else
      {
        this._IsDisabled = true;
      }
      if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
      {
        Log.Info("MatrixMX.Setup(): starting EQ_Update() thread");
        this._EqThread = new Thread(new ThreadStart(this.EQ_Update));
        this._EqThread.IsBackground = true;
        this._EqThread.Priority = ThreadPriority.BelowNormal;
        this._EqThread.Name = "EQ_Update";
        this._EqThread.Start();
        if (this._EqThread.IsAlive)
        {
          Log.Info("MatrixMX.Setup(): EQ_Update() Thread Started");
        }
        else
        {
          Log.Info("MatrixMX.Setup(): EQ_Update() FAILED TO START");
        }
      }
      AdvancedSettings.OnSettingsChanged +=
        new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
      Log.Info("MatrixMX.Setup(): completed");
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= this.LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (this.DoDebug)
        {
          Log.Info("MatrixMX.UpdateAdvancedSettings(): called");
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixMX.xml")))
        {
          FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixMX.xml"));
          if (info.LastWriteTime.Ticks > this.SettingsLastModTime.Ticks)
          {
            if (this.DoDebug)
            {
              Log.Info("MatrixMX.UpdateAdvancedSettings(): updating advanced settings");
            }
            this.LoadAdvancedSettings();
          }
        }
        if (this.DoDebug)
        {
          Log.Info("MatrixMX.UpdateAdvancedSettings(): completed");
        }
      }
    }

    public string Description
    {
      get { return "Matrix Orbital Character LCD driver v03_09_2008b"; }
    }

    public string ErrorMessage
    {
      get { return this._ErrorMessage; }
    }

    public bool IsDisabled
    {
      get { return this._IsDisabled; }
    }

    public string Name
    {
      get { return "MatrixMX"; }
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
      private bool m_DisableRepeat;
      private bool m_EnableDisplayAction;
      private int m_EnableDisplayActionTime = 5;
      private bool m_EnableKeypad;
      private bool m_EqDisplay;
      private int m_EqRate = 10;
      private bool m_EQTitleDisplay;
      private int m_EQTitleDisplayTime = 10;
      private int m_EQTitleShowTime = 2;
      private static AdvancedSettings m_Instance;
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
        Log.Info("MatrixMX.AdvancedSettings.CreateDefaultKeyPadMapping(): called");
        bool flag = false;
        string str = "MatrixMX_Keypad";
        try
        {
          Log.Info(
            "MatrixMX.AdvancedSettings.CreateDefaultKeyPadMapping(): remote mapping file does not exist - Creating default mapping file",
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
          Log.Info("MatrixMX.AdvancedSettings.CreateDefaultKeyPadMapping(): remote mapping file created");
        }
        catch
        {
          Log.Info("MatrixMX.AdvancedSettings.CreateDefaultKeyPadMapping(): Error saving remote mapping to XML file",
                   new object[0]);
          flag = false;
        }
        Log.Info("MatrixMX.AdvancedSettings.CreateDefaultKeyPadMapping(): completed");
        return flag;
      }

      private static void Default(AdvancedSettings _settings)
      {
        Log.Info("MatrixMX.AdvancedSettings.Default(): called");
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
        Log.Info("MatrixMX.AdvancedSettings.Default(): completed");
      }

      public static AdvancedSettings Load()
      {
        AdvancedSettings settings;
        Log.Info("MatrixMX.AdvancedSettings.Load(): started");
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixMX.xml")))
        {
          Log.Info("MatrixMX.AdvancedSettings.Load(): Loading settings from XML file");
          XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixMX.xml"));
          settings = (AdvancedSettings)serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Info("MatrixMX.AdvancedSettings.Load(): Loading settings from defaults");
          settings = new AdvancedSettings();
          Default(settings);
        }
        Log.Info("MatrixMX.AdvancedSettings.Load(): completed");
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
        Log.Info("MatrixMX.AdvancedSettings.Save(): Saving settings to XML file");
        XmlSerializer serializer = new XmlSerializer(typeof (AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_MatrixMX.xml"),
                                                 Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter)writer, ToSave);
        writer.Close();
        Log.Info("MatrixMX.AdvancedSettings.Save(): completed");
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
      public bool DisableRepeat
      {
        get { return this.m_DisableRepeat; }
        set { this.m_DisableRepeat = value; }
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
      public bool EnableKeypad
      {
        get { return this.m_EnableKeypad; }
        set { this.m_EnableKeypad = value; }
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
      public int RepeatDelay
      {
        get { return this.m_RepeatDelay; }
        set { this.m_RepeatDelay = value; }
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
      public bool UseCustomKeypadMap
      {
        get { return this.m_UseCustomKeypadMap; }
        set { this.m_UseCustomKeypadMap = value; }
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
    public struct KeyPadControl
    {
      public bool EnableKeyPad;
      public bool EnableCustom;
    }

    private class MODisplay
    {
      private int _currentBrightness;
      private int _currentContrast;
      private InputHandler _inputHandler;
      private bool _IsDisplayOff;
      private bool _isOpen;
      private bool _UseBacklight;
      private bool _UseContrast;
      private bool _useCustomKeypadMapping;
      private SerialPort commPort;

      public void BacklightOff()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          this.commPort.Write(new byte[] {0xfe, 70}, 0, 2);
        }
      }

      public void BacklightOn()
      {
        this.BacklightOn(0);
      }

      public void BacklightOn(int MinutesTillOff)
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          this.commPort.Write(new byte[] {0xfe, 0x42, (byte)MinutesTillOff}, 0, 3);
        }
      }

      public void ClearDisplay()
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.commPort.Write(new byte[] {0xfe, 0x58}, 0, 2);
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
        }
        catch (Exception exception)
        {
          Log.Error(exception);
        }
      }

      public void CursorPosition(int column, int row)
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          this.commPort.Write(new byte[] {0xfe, 0x47, (byte)column, (byte)row}, 0, 4);
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

      public void DrawHorizontalBarGraph(int column, int row, int direction, int length)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.commPort.Write(new byte[] {0xfe, 0x7c, (byte)column, (byte)row, (byte)direction, (byte)length}, 0, 6);
        }
      }

      public void DrawVerticalBarGraph(int column, int height)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.commPort.Write(new byte[] {0xfe, 0x3d, (byte)column, (byte)height}, 0, 4);
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
          Log.Info("MatrixMX.FireKeypadEvent(): called");
        }
        if (!this._inputHandler.MapAction(EventCode))
        {
          if (extensiveLogging)
          {
            Log.Info("MatrixMX.FireKeypadEvent(): No button mapping for Keypad button = {0}",
                     new object[] {EventCode.ToString("x00")});
          }
        }
        else if (extensiveLogging)
        {
          Log.Info("MatrixMX.FireKeypadEvent(): fired event for Keypad button = {0}",
                   new object[] {EventCode.ToString("x00")});
        }
        if (extensiveLogging)
        {
          Log.Info("MatrixMX.FireKeypadEvent(): completed");
        }
      }

      public void InitHorizontalBarGraph()
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.commPort.Write(new byte[] {0xfe, 0x68}, 0, 2);
        }
      }

      public void InitNarrowVerticalBarGraph()
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.commPort.Write(new byte[] {0xfe, 0x73}, 0, 2);
        }
      }

      public void InitWideVerticalBarGraph()
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.commPort.Write(new byte[] {0xfe, 0x76}, 0, 2);
        }
      }

      public void Key_AutoRepeatModeOn(int _mode)
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          this.commPort.Write(new byte[] {0xfe, 0x7e, (byte)_mode}, 0, 3);
        }
      }

      public void Key_AutoTransmitKeypressOn()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          this.commPort.Write(new byte[] {0xfe, 0x41}, 0, 2);
        }
      }

      public void Key_ClearKeyBuffer()
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          this.commPort.Write(new byte[] {0xfe, 0x45}, 0, 2);
        }
      }

      public void Key_SetDebounceTime(int _time)
      {
        if (!(!this._isOpen | !this.commPort.IsOpen))
        {
          this.commPort.Write(new byte[] {0xfe, 0x55, (byte)_time}, 0, 3);
        }
      }

      public bool OpenDisplay(string _port, bool _useBacklight, int _backlightLevel, bool _useContrast,
                              int _contrastLevel, bool EnableKeypad)
      {
        try
        {
          this._UseBacklight = _useBacklight;
          this._currentBrightness = _backlightLevel;
          this._UseContrast = _useContrast;
          this._currentContrast = _contrastLevel;
          this.commPort = new SerialPort(_port, 0x4b00, Parity.None, 8, StopBits.One);
          if (EnableKeypad)
          {
            this.commPort.DataReceived += new SerialDataReceivedEventHandler(this.WhenDataReceived);
            this.commPort.ReceivedBytesThreshold = 1;
          }
          this.commPort.Open();
          this.BacklightOn();
          this.SetBacklightBrightness(this._currentBrightness);
          this.SetContrast(this._currentContrast);
          this.ClearDisplay();
          this.Key_AutoRepeatModeOn(1);
          this.Key_ClearKeyBuffer();
          this.Key_SetDebounceTime(0x80);
          this.Key_AutoTransmitKeypressOn();
          this.commPort.DiscardInBuffer();
          this._isOpen = true;
          this._IsDisplayOff = false;
        }
        catch (Exception)
        {
          Log.Info("MatrixMX.MODisplay.OpenDisplay(): CAUGHT EXCEPTION while opening display!");
          this._isOpen = false;
        }
        return this._isOpen;
      }

      public void SetBacklightBrightness(int brightness)
      {
        if (!(((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff) | !this._UseBacklight))
        {
          this.commPort.Write(new byte[] {0xfe, 0x99, (byte)brightness}, 0, 3);
        }
      }

      public void SetContrast(int contrast)
      {
        if (!(((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff) | !this._UseContrast))
        {
          this.commPort.Write(new byte[] {0xfe, 80, (byte)contrast}, 0, 3);
        }
      }

      public void SetLine(int _line, string _message)
      {
        if (!((!this._isOpen | !this.commPort.IsOpen) | this._IsDisplayOff))
        {
          this.BacklightOn();
          this.SetBacklightBrightness(this._currentBrightness);
          this.SetContrast(this._currentContrast);
          if (_line == 0)
          {
            this.CursorPosition(1, 1);
          }
          else
          {
            this.CursorPosition(1, 2);
          }
          for (int i = 0; i < 20; i++)
          {
            if (i < _message.Length)
            {
              this.commPort.Write(new byte[] {(byte)_message[i]}, 0, 1);
            }
            else
            {
              this.commPort.Write(new byte[] {0x20}, 0, 1);
            }
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
          if (this.TestXmlVersion(Config.GetFile(Config.Dir.CustomInputDefault, "MatrixMX_Keypad.xml")) < 3)
          {
            Log.Info(
              "MatrixMX.MODisplay.SetupKeypad(): Deleting MatrixMX_Keypad mapping file with the wrong version stamp.",
              new object[0]);
            File.Delete(Config.GetFile(Config.Dir.CustomInputDefault, "MatrixMX_Keypad.xml"));
          }
          if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "MatrixMX_Keypad.xml")))
          {
            Log.Info("MatrixMX.Setup(): Creating default MatrixMX_Keypad mapping file");
            if (!AdvancedSettings.CreateDefaultKeyPadMapping())
            {
              Log.Info("MatrixMX.MODisplay.SetupKeypad(): ERROR Creating default MatrixMX_Keypad mapping file",
                       new object[0]);
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
        }
        catch (Exception exception)
        {
          Log.Info("MatrixMX.MODisplay.SetupKeypad(): CAUGHT EXCEPTION while loading InputHander - {0}",
                   new object[] {exception});
          flag2 = false;
          flag = false;
        }
        if (flag2)
        {
          Log.Info("MatrixMX.MODisplay.SetupKeypad(): Loading InputHandler");
          this._inputHandler = new InputHandler("MatrixMX_Keypad");
          Log.Info("MatrixMX.MODisplay.SetupKeypad(): InputHandler loaded = {0}",
                   new object[] {this._inputHandler.IsLoaded});
          if (this._inputHandler.IsLoaded)
          {
            flag = true;
          }
          else
          {
            flag = false;
            Log.Info("MatrixMX.MODisplay.SetupKeypad(): error loading InputHandler - remote support disabled",
                     new object[0]);
          }
        }
        else
        {
          Log.Info("MatrixMX.MODisplay.SetupKeypad(): Keypad support disabled - no keypad mapping file");
          flag = false;
        }
        if (flag && this._inputHandler.IsLoaded)
        {
          return flag;
        }
        Log.Info("MatrixMX.MODisplay.SetupKeypad(): Error loading Keypad mapping file - Keypad support disabled",
                 new object[0]);
        return false;
      }

      public int TestXmlVersion(string xmlPath)
      {
        if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "MatrixMX_Keypad.xml")))
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
          Log.Info("MODisplay: received KeyPad event");
          switch (eventCode)
          {
            case 0x4b:
              Log.Info("MODisplay: received KeyPad event - Cursor Up {0}", new object[] {eventCode.ToString("x00")});
              action = new Action(Action.ActionType.ACTION_MOVE_UP, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 0x4c:
              Log.Info("MODisplay: received KeyPad event - Cursor Down {0}", new object[] {eventCode.ToString("x00")});
              action = new Action(Action.ActionType.ACTION_MOVE_DOWN, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 0x52:
              Log.Info("MODisplay: received KeyPad event - Cursor Left{0}", new object[] {eventCode.ToString("x00")});
              action = new Action(Action.ActionType.ACTION_MOVE_LEFT, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 70:
              Log.Info("MODisplay: received KeyPad event - Cursor Right{0}", new object[] {eventCode.ToString("x00")});
              action = new Action(Action.ActionType.ACTION_MOVE_RIGHT, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 0x4a:
              Log.Info("MODisplay: received KeyPad event - Enter {0}", new object[] {eventCode.ToString("x00")});
              action = new Action(Action.ActionType.ACTION_SELECT_ITEM, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 80:
              Log.Info("MODisplay: received KeyPad event - F1 {0}", new object[] {eventCode.ToString("x00")});
              action = new Action(Action.ActionType.ACTION_STEP_BACK, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;

            case 0x51:
              Log.Info("MODisplay: received KeyPad event - F2 {0}", new object[] {eventCode.ToString("x00")});
              action = new Action(Action.ActionType.ACTION_STOP, 0f, 0f);
              GUIGraphicsContext.OnAction(action);
              return;
          }
          Log.Info("MODisplay: received KeyPad event - received byte {0} Unknown Key",
                   new object[] {eventCode.ToString("x00")});
        }
        else
        {
          this.FireKeypadEvent(eventCode);
        }
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
  }
}