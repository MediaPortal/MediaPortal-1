using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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
  public class DebugForm : MediaPortal.UserInterface.Controls.MPForm, IDisplay, IDisposable
  {
    private Thread _displayThread;
    private bool _IsDisplayOff;
    private byte[] _lastHash;
    private byte[] _lastTextHash;
    private bool _mpIsIdle;
    private readonly SHA256Managed _sha256 = new SHA256Managed();
    private static bool _stopDisplayUpdateThread = false;
    private AdvancedSettings AdvSettings = AdvancedSettings.Load();
    private byte[] bitmapData;
    private Container components = null;
    private DisplayControl DisplaySettings;
    private bool DoDebug;
    private bool DrawingText;
    private object dWriteMutex = new object();
    private static object DWriteMutex = new object();
    private EQControl EQSettings;
    private string errorMessage = "";
    private int gCols;
    private int gLines;
    private PictureBox graphicDisplay;
    private bool isDisabled;
    private DateTime LastSettingsCheck = DateTime.Now;
    private string[] LastText;
    private SystemStatus MPStatus = new SystemStatus();
    private DateTime SettingsLastModTime;
    private Bitmap tBitmap;
    private int tCols;
    private Graphics tGraphics;
    private int tHeight;
    private static object ThreadMutex = new object();
    private int tLineHeight;
    private int tLines;
    private int tWidth;
    private Font UseFont;
    private bool UseTextMode;

    public DebugForm()
    {
      try
      {
        this.InitializeComponent();
        base.Size = new Size(Settings.Instance.GraphicWidth + 6, Settings.Instance.GraphicHeight + 0x18);
        this.DoRefresh();
      } catch (Exception exception)
      {
        this.isDisabled = true;
        this.errorMessage = exception.Message;
      }
    }

    private void Check_Idle_State()
    {
      if (this.MPStatus.MP_Is_Idle)
      {
        if (this.DoDebug)
        {
          Log.Info("DebugForm.DisplayLines(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle, this.DisplaySettings._BlankIdleTimeout });
        }
        if (this.DisplaySettings.BlankDisplayWhenIdle)
        {
          if (!this._mpIsIdle)
          {
            if (this.DoDebug)
            {
              Log.Info("DebugForm.DisplayLines(): MP going IDLE", new object[0]);
            }
            this.DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
          }
          if (!this._IsDisplayOff && ((DateTime.Now.Ticks - this.DisplaySettings._BlankIdleTime) > this.DisplaySettings._BlankIdleTimeout))
          {
            if (this.DoDebug)
            {
              Log.Info("DebugForm.DisplayLines(): Blanking display due to IDLE state", new object[0]);
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
            Log.Info("DebugForm.DisplayLines(): MP no longer IDLE - restoring display", new object[0]);
          }
          this.DisplayOn();
        }
        this._mpIsIdle = false;
      }
    }

    public void CleanUp()
    {
      base.Hide();
      this.tGraphics.Dispose();
      this.tBitmap.Dispose();
    }

    public void Configure()
    {
      new DebugForm_AdvancedSetupForm().ShowDialog();
    }

    private void DisplayEQ()
    {
      if (this.EQSettings._EqDataAvailable)
      {
        lock (DWriteMutex)
        {
          object obj3;
          RectangleF bounds = this.graphicDisplay.Bounds;
          if (this.DoDebug)
          {
            Log.Info("DebugForm.DisplayEQ(): called", new object[0]);
          }
          this.EQSettings.Render_MaxValue = (this.EQSettings.UseNormalEq | this.EQSettings.UseStereoEq) ? ((int)bounds.Height) : ((int)bounds.Width);
          this.EQSettings.Render_BANDS = this.EQSettings.UseNormalEq ? 0x10 : (this.EQSettings.UseStereoEq ? 8 : 1);
          MiniDisplayHelper.ProcessEqData(ref this.EQSettings);
          Monitor.Enter(obj3 = DWriteMutex);
          try
          {
            this.tGraphics.FillRectangle(Brushes.White, bounds);
            for (int i = 0; i < this.EQSettings.Render_BANDS; i++)
            {
              RectangleF ef2;
              if (this.DoDebug)
              {
                Log.Info("DebugForm.DisplayEQ(): Rendering {0} band {1} = {2}", new object[] { this.EQSettings.UseNormalEq ? "Normal EQ" : (this.EQSettings.UseStereoEq ? "Stereo EQ" : (this.EQSettings.UseVUmeter ? "VU Meter" : "VU Meter 2")), i, this.EQSettings.UseNormalEq ? this.EQSettings.EqArray[1 + i].ToString() : (this.EQSettings.UseStereoEq ? (this.EQSettings.EqArray[1 + i].ToString() + " : " + this.EQSettings.EqArray[9 + i].ToString()) : (this.EQSettings.EqArray[1 + i].ToString() + " : " + this.EQSettings.EqArray[2 + i].ToString())) });
              }
              if (this.EQSettings.UseNormalEq)
              {
                ef2 = new RectangleF((bounds.X + (i * (((int)bounds.Width) / this.EQSettings.Render_BANDS))) + 1f, bounds.Y + (((int)bounds.Height) - this.EQSettings.EqArray[1 + i]), (float)((((int)bounds.Width) / this.EQSettings.Render_BANDS) - 2), (float)this.EQSettings.EqArray[1 + i]);
                this.tGraphics.FillRectangle(Brushes.Black, ef2);
              }
              else
              {
                int num2;
                RectangleF ef3;
                if (this.EQSettings.UseStereoEq)
                {
                  int num4 = (((int)bounds.Width) / 2) / this.EQSettings.Render_BANDS;
                  num2 = i * num4;
                  int num3 = (i + this.EQSettings.Render_BANDS) * num4;
                  ef2 = new RectangleF((bounds.X + num2) + 1f, bounds.Y + (((int)bounds.Height) - this.EQSettings.EqArray[1 + i]), (float)(num4 - 2), (float)this.EQSettings.EqArray[1 + i]);
                  ef3 = new RectangleF((bounds.X + num3) + 1f, bounds.Y + (((int)bounds.Height) - this.EQSettings.EqArray[9 + i]), (float)(num4 - 2), (float)this.EQSettings.EqArray[9 + i]);
                  this.tGraphics.FillRectangle(Brushes.Black, ef2);
                  this.tGraphics.FillRectangle(Brushes.Black, ef3);
                }
                else if (this.EQSettings.UseVUmeter | this.EQSettings.UseVUmeter2)
                {
                  ef2 = new RectangleF(bounds.X + 1f, bounds.Y + 1f, (float)this.EQSettings.EqArray[1 + i], (float)(((int)(bounds.Height / 2f)) - 2));
                  num2 = this.EQSettings.UseVUmeter ? 0 : (((int)bounds.Width) - this.EQSettings.EqArray[2 + i]);
                  ef3 = new RectangleF((bounds.X + num2) + 1f, (bounds.Y + (bounds.Height / 2f)) + 1f, (float)this.EQSettings.EqArray[2 + i], (float)(((int)(bounds.Height / 2f)) - 2));
                  this.tGraphics.FillRectangle(Brushes.Black, ef2);
                  this.tGraphics.FillRectangle(Brushes.Black, ef3);
                }
              }
            }
            this.graphicDisplay.Image = this.tBitmap;
          } catch (Exception exception)
          {
            Log.Info("DebugForm.DisplayEQ(): CAUGHT EXCEPTION {0}", new object[] { exception });
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
              Log.Info("DebugForm.DisplayOff(): DisplayControlAction Timer = {0}.", new object[] { DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction });
            }
            return;
          }
          if (this.DoDebug)
          {
            Log.Info("DebugForm.DisplayOff(): DisplayControlAction Timeout expired.", new object[0]);
          }
          this.DisplaySettings._DisplayControlAction = false;
          this.DisplaySettings._DisplayControlLastAction = 0L;
        }
        Log.Info("DebugForm.DisplayOff(): called", new object[0]);
        lock (DWriteMutex)
        {
          Log.Info("DebugForm.DisplayOff(): Turning Display OFF", new object[0]);
          this.tGraphics.FillRectangle(Brushes.DarkGreen, new Rectangle(0, 0, this.tBitmap.Width, this.tBitmap.Height));
          this.graphicDisplay.Image = this.tBitmap;
          this._IsDisplayOff = true;
        }
        Log.Info("DebugForm.DisplayOff(): completed", new object[0]);
      }
    }

    private void DisplayOn()
    {
      if (this._IsDisplayOff)
      {
        Log.Info("DebugForm.DisplayOn(): called", new object[0]);
        lock (DWriteMutex)
        {
          Log.Info("DebugForm.DisplayOn(): Turning Display ON", new object[0]);
          this._IsDisplayOff = false;
        }
        Log.Info("DebugForm.DisplayOn(): called", new object[0]);
      }
    }

    private void DisplayUpdate()
    {
      if (this.DoDebug)
      {
        Log.Info("DebugForm.DisplayUpdate() Starting Display Update Thread", new object[0]);
      }
      if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
      {
        GUIWindowManager.OnNewAction += new OnActionHandler(this.OnExternalAction);
      }
      while (true)
      {
        lock (ThreadMutex)
        {
          if (_stopDisplayUpdateThread)
          {
            if (this.DoDebug)
            {
              Log.Info("DebugForm.DisplayUpdate() Display Update Thread terminating", new object[0]);
            }
            if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
            {
              GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
            }
            return;
          }
        }
        MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
        this.Check_Idle_State();
        if (((!this.MPStatus.MediaPlayer_Active | !this.MPStatus.MediaPlayer_Playing) & this.DisplaySettings.BlankDisplayWithVideo) & (this.DisplaySettings.BlankDisplayWhenIdle & !this._mpIsIdle))
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
              Log.Info("DebugForm.Display_Update(): Turning off display while playing video", new object[0]);
            }
            this.DisplayOff();
          }
        }
        else
        {
          this.RestoreDisplayFromVideoOrIdle();
          lock (DWriteMutex)
          {
            this.EQSettings._EqDataAvailable = false;
            this._displayThread.Priority = ThreadPriority.BelowNormal;
          }
        }
        if (!this.EQSettings._EqDataAvailable || this.MPStatus.MediaPlayer_Paused)
        {
          if (this.DoDebug)
          {
            Log.Info("DebugForm.DisplayUpdate() Sleeping...", new object[0]);
          }
          Thread.Sleep(250);
          if (this.DoDebug)
          {
            Log.Info("DebugForm.DisplayUpdate() Waking...", new object[0]);
          }
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && (this.components != null))
      {
        this.components.Dispose();
        base.Hide();
      }
      base.Dispose(disposing);
    }

    private void DoRefresh()
    {
      if (base.InvokeRequired)
      {
        base.Invoke(new MethodInvoker(this.DoRefresh));
      }
      else
      {
        base.Update();
      }
    }

    public void DrawImage(Bitmap bitmap)
    {
      if (this.DoDebug)
      {
        Log.Info("DebugForm.DrawImage(): EQ DATA = {0}, Display Off = {1}", new object[] { this.EQSettings._EqDataAvailable, this._IsDisplayOff });
      }
      if (this.EQSettings._EqDataAvailable || this._IsDisplayOff)
      {
        if (this.DoDebug)
        {
          Log.Info("DebugForm.DrawImage(): Suppressing display update!", new object[0]);
        }
      }
      else
      {
        base.BringToFront();
        lock (this.dWriteMutex)
        {
          if (!(this.UseTextMode & !this.DrawingText))
          {
            if (bitmap == null)
            {
              if (Settings.Instance.ExtensiveLogging)
              {
                Log.Debug("DebugForm.DrawImage():  bitmap null", new object[0]);
              }
            }
            else
            {
              if (this.DoDebug)
              {
                Log.Info("DebugForm.DrawImage():  called", new object[0]);
              }
              BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
              try
              {
                if (this.bitmapData == null)
                {
                  this.bitmapData = new byte[bitmapdata.Stride * this.gLines];
                }
                Marshal.Copy(bitmapdata.Scan0, this.bitmapData, 0, this.bitmapData.Length);
              }
              finally
              {
                bitmap.UnlockBits(bitmapdata);
              }
              byte[] buffer = this._sha256.ComputeHash(this.bitmapData);
              if (ByteArray.AreEqual(buffer, this._lastHash))
              {
                if (this.DoDebug)
                {
                  Log.Info("DebugForm.DrawImage():  bitmap not changed", new object[0]);
                }
              }
              else
              {
                this._lastHash = buffer;
                this.graphicDisplay.Image = bitmap;
                base.Visible = true;
                this.DoRefresh();
                Application.DoEvents();
                if (this.DoDebug)
                {
                  Log.Info("DebugForm.DrawImage():  completed", new object[0]);
                }
              }
            }
          }
        }
      }
    }

    private void DrawText(int _line, string _message)
    {
      if (this.EQSettings._EqDataAvailable || this._IsDisplayOff)
      {
        if (this.DoDebug)
        {
          Log.Info("DebugForm.DrawText(): Suppressing display update!", new object[0]);
        }
      }
      else
      {
        Log.Info("DebugForm.DrawText() - called", new object[0]);
        if (!_message.Equals(this.LastText[_line]))
        {
          this.LastText[_line] = _message;
          RectangleF rect = new RectangleF(0f, (float)(this.tLineHeight * _line), (float)this.tWidth, (float)this.tLineHeight);
          this.tGraphics.SmoothingMode = SmoothingMode.None;
          this.tGraphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
          this.tGraphics.FillRectangle(Brushes.White, rect);
          int length = _message.Length;
          while (this.tGraphics.MeasureString(_message.Substring(0, length), this.UseFont).Width > this.tWidth)
          {
            length--;
          }
          this.tGraphics.DrawString(_message.Substring(0, length), this.UseFont, Brushes.Black, rect);
          this.DrawingText = true;
          this.DrawImage(this.tBitmap);
          this.DrawingText = false;
        }
      }
    }

    public void DrawTextImage(Bitmap bitmap)
    {
      if (bitmap == null)
      {
        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Debug("DebugForm.DrawTextImage():  bitmap null", new object[0]);
        }
      }
      else
      {
        if (this.DoDebug)
        {
          Log.Info("DebugForm.DrawTextImage():  completed", new object[0]);
        }
        BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        try
        {
          if (this.bitmapData == null)
          {
            this.bitmapData = new byte[bitmapdata.Stride * this.gLines];
          }
          Marshal.Copy(bitmapdata.Scan0, this.bitmapData, 0, this.bitmapData.Length);
        }
        finally
        {
          bitmap.UnlockBits(bitmapdata);
        }
        byte[] buffer = this._sha256.ComputeHash(this.bitmapData);
        if (ByteArray.AreEqual(buffer, this._lastTextHash))
        {
          if (this.DoDebug)
          {
            Log.Info("DebugForm.DrawTextImage():  bitmap not changed", new object[0]);
          }
        }
        else
        {
          this._lastTextHash = buffer;
          this.graphicDisplay.Image = bitmap;
          base.Visible = true;
          this.DoRefresh();
          Application.DoEvents();
          if (this.DoDebug)
          {
            Log.Info("DebugForm.DrawTextImage():  completed", new object[0]);
          }
        }
      }
    }

    private void GetEQ()
    {
      lock (DWriteMutex)
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

    public void Initialize()
    {
      if (base.InvokeRequired)
      {
        base.Invoke(new MethodInvoker(this.Initialize));
      }
      else
      {
        base.Show();
        Application.DoEvents();
        if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
        {
          this._displayThread = new Thread(new ThreadStart(this.DisplayUpdate));
          this._displayThread.IsBackground = true;
          this._displayThread.Priority = ThreadPriority.BelowNormal;
          this._displayThread.Name = "DebugForm_Update";
          this._displayThread.Start();
          if (this._displayThread.IsAlive)
          {
            Log.Info("DebugForm.Initialize(): DebugForm.Display_Update() Thread Started", new object[0]);
          }
          else
          {
            Log.Info("DebugForm.Initialize(): DebugForm.Display_Update() FAILED TO START", new object[0]);
          }
        }
      }
    }

    private void InitializeComponent()
    {
      this.graphicDisplay = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.graphicDisplay)).BeginInit();
      this.SuspendLayout();
      // 
      // graphicDisplay
      // 
      this.graphicDisplay.BackColor = System.Drawing.Color.Lime;
      this.graphicDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.graphicDisplay.Location = new System.Drawing.Point(0, 0);
      this.graphicDisplay.Name = "graphicDisplay";
      this.graphicDisplay.Size = new System.Drawing.Size(360, 82);
      this.graphicDisplay.TabIndex = 0;
      this.graphicDisplay.TabStop = false;
      this.graphicDisplay.WaitOnLoad = true;
      // 
      // DebugForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 21F);
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
      this.ClientSize = new System.Drawing.Size(360, 82);
      this.Controls.Add(this.graphicDisplay);
      this.Font = new System.Drawing.Font("Lucida Console", 16F);
      this.Name = "DebugForm";
      this.Text = "MiniDisplay - Debug (Preview)";
      this.TopMost = true;
      ((System.ComponentModel.ISupportInitialize)(this.graphicDisplay)).EndInit();
      this.ResumeLayout(false);

    }

    private void InitializeDriver()
    {
      this.DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      Log.Info("DebugForm.InitializeDriver(): started.", new object[0]);
      Log.Info("DebugForm.InitializeDriver(): DebugForm Driver - {0}", new object[] { this.Description });
      Log.Info("DebugForm.InitializeDriver(): Called by \"{0}\".", new object[] { Assembly.GetEntryAssembly().FullName });
      FileInfo info = new FileInfo(Assembly.GetExecutingAssembly().Location);
      if (this.DoDebug)
      {
        Log.Info("DebugForm.InitializeDriver(): Assembly creation time: {0} ( {1} UTC )", new object[] { info.LastWriteTime, info.LastWriteTimeUtc.ToUniversalTime() });
      }
      if (this.DoDebug)
      {
        Log.Info("DebugForm.InitializeDriver(): Platform: {0}", new object[] { Environment.OSVersion.VersionString });
      }
      MiniDisplayHelper.InitEQ(ref this.EQSettings);
      MiniDisplayHelper.InitDisplayControl(ref this.DisplaySettings);
      this.LoadAdvancedSettings();
      Log.Info("DebugForm.InitializeDriver(): Advanced options - Force Graphic Text: {0}", new object[] { Settings.Instance.ForceGraphicText });
      Log.Info("DebugForm.InitializeDriver(): Advanced options - Equalizer Display: {0}", new object[] { this.EQSettings.UseEqDisplay });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -   Normal Equalizer Display: {0}", new object[] { this.EQSettings.UseNormalEq });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -   Stereo Equalizer Display: {0}", new object[] { this.EQSettings.UseStereoEq });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -   VU Meter Display: {0}", new object[] { this.EQSettings.UseVUmeter });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -   VU Meter Style 2 Display: {0}", new object[] { this.EQSettings.UseVUmeter2 });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -     Use VU Channel indicators: {0}", new object[] { this.EQSettings._useVUindicators });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -   Restrict EQ Update Rate: {0}", new object[] { this.EQSettings.RestrictEQ });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -     Restricted EQ Update Rate: {0} updates per second", new object[] { this.EQSettings._EQ_Restrict_FPS });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -   Delay EQ Startup: {0}", new object[] { this.EQSettings.DelayEQ });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -     Delay EQ Startup Time: {0} seconds", new object[] { this.EQSettings._DelayEQTime });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -   Smooth EQ Amplitude Decay: {0}", new object[] { this.EQSettings.SmoothEQ });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -   Show Track Info with EQ display: {0}", new object[] { this.EQSettings.EQTitleDisplay });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -     Show Track Info Interval: {0} seconds", new object[] { this.EQSettings._EQTitleDisplayTime });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -     Show Track Info duration: {0} seconds", new object[] { this.EQSettings._EQTitleShowTime });
      Log.Info("DebugForm.InitializeDriver(): Advanced options - Blank display with video: {0}", new object[] { this.DisplaySettings.BlankDisplayWithVideo });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -   Enable Display on Action: {0}", new object[] { this.DisplaySettings.EnableDisplayAction });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -     Enable display for: {0} seconds", new object[] { this.DisplaySettings.DisplayActionTime });
      Log.Info("DebugForm.InitializeDriver(): Advanced options - Blank display when idle: {0}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle });
      Log.Info("DebugForm.InitializeDriver(): Advanced options -     blank display after: {0} seconds", new object[] { this.DisplaySettings._BlankIdleTimeout / 0xf4240L });
      Log.Info("DebugForm.InitializeDriver(): Advanced options - Shutdown Message - Line 1: {0}", new object[] { this.DisplaySettings._Shutdown1 });
      Log.Info("DebugForm.InitializeDriver(): Advanced options - Shutdown Message - Line 2: {0}", new object[] { this.DisplaySettings._Shutdown2 });
      Log.Info("DebugForm.InitializeDriver(): Setting - Audio using ASIO: {0}", new object[] { this.EQSettings._AudioUseASIO });
      Log.Info("DebugForm.InitializeDriver(): Setting - Audio using Mixer: {0}", new object[] { this.EQSettings._AudioIsMixing });
      Log.Info("DebugForm.InitializeDriver(): Extensive logging: {0}", new object[] { this.DoDebug });
      Log.Info("DebugForm.InitializeDriver(): completed.", new object[0]);
    }

    private void LoadAdvancedSettings()
    {
      this.AdvSettings = AdvancedSettings.Load();
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
      this.EQSettings._EQTitleShowTime = this.AdvSettings.EQTitleShowTime;
      this.EQSettings._EQTitleDisplayTime = this.AdvSettings.EQTitleDisplayTime;
      this.EQSettings._EqUpdateDelay = (this.EQSettings._EQ_Restrict_FPS == 0) ? 0 : ((0x989680 / this.EQSettings._EQ_Restrict_FPS) - (0xf4240 / this.EQSettings._EQ_Restrict_FPS));
      this.DisplaySettings.BlankDisplayWithVideo = this.AdvSettings.BlankDisplayWithVideo;
      this.DisplaySettings.EnableDisplayAction = this.AdvSettings.EnableDisplayAction;
      this.DisplaySettings.DisplayActionTime = this.AdvSettings.EnableDisplayActionTime;
      this.DisplaySettings.BlankDisplayWhenIdle = this.AdvSettings.BlankDisplayWhenIdle;
      this.DisplaySettings.BlankIdleDelay = this.AdvSettings.BlankIdleTime;
      this.DisplaySettings._BlankIdleTimeout = this.DisplaySettings.BlankIdleDelay * 0x989680;
      this.DisplaySettings._Shutdown1 = Settings.Instance.Shutdown1;
      this.DisplaySettings._Shutdown2 = Settings.Instance.Shutdown2;
      this.DisplaySettings._DisplayControlTimeout = this.DisplaySettings.DisplayActionTime * 0x989680;
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_DebugForm.xml"));
      this.SettingsLastModTime = info.LastWriteTime;
      this.LastSettingsCheck = DateTime.Now;
    }

    private void OnExternalAction(Action action)
    {
      if (this.DisplaySettings.EnableDisplayAction)
      {
        if (this.DoDebug)
        {
          Log.Info("DebugForm.OnExternalAction(): received action {0}", new object[] { action.wID.ToString() });
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
          Log.Info("DebugForm.OnExternalAction(): received DisplayControlAction", new object[0]);
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
          if (!this.MPStatus.MP_Is_Idle)
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

    public void SetLine(int _line, string _message)
    {
      if (base.InvokeRequired)
      {
        Log.Info("DebugForm.SetLine() - Invoke required", new object[0]);
        base.Invoke(new SetLineDelegate(this.SetLine), new object[] { _line, _message });
      }
      else
      {
        this.DrawText(_line, _message);
        this.DoRefresh();
      }
    }

    public void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight, int backLightLevel, bool contrast, int contrastLevel, bool BlankOnExit)
    {
      this.InitializeDriver();
      this.tLines = lines;
      this.tCols = cols;
      if (this.UseFont != null)
      {
        this.UseFont.Dispose();
      }
      this.UseFont = new Font("Lucida Console", 16f);
      if (!Settings.Instance.ForceGraphicText)
      {
        this.UseTextMode = true;
        this.LastText = new string[lines];
        for (int i = 0; i < lines; i++)
        {
          this.LastText[i] = string.Empty;
        }
        this.tBitmap = new Bitmap(1, 1);
        this.tGraphics = Graphics.FromImage(this.tBitmap);
        this.tGraphics.SmoothingMode = SmoothingMode.None;
        this.tGraphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        this.tLineHeight = ((int)this.tGraphics.MeasureString("Zg", this.UseFont).Height) + 2;
        this.tWidth = ((int)this.tGraphics.MeasureString("M", this.UseFont).Width) * this.tCols;
        this.tHeight = (((int)this.tGraphics.MeasureString("Zg", this.UseFont).Height) * this.tLines) + (2 * (this.tLines - 1));
        base.ShowInTaskbar = false;
        base.Size = new Size(((int)(this.tWidth * 0.7f)) + 6, this.tHeight + 0x18);
        this.tGraphics.Dispose();
        this.tBitmap.Dispose();
        this.tBitmap = new Bitmap(this.tWidth, this.tHeight);
        this.tGraphics = Graphics.FromImage(this.tBitmap);
        this.tGraphics.FillRectangle(Brushes.White, this.tGraphics.ClipBounds);
        this.tGraphics.SmoothingMode = SmoothingMode.None;
        this.tGraphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        this.gLines = this.tBitmap.Height;
        this.gCols = this.tBitmap.Width;
        this.DoRefresh();
      }
      else
      {
        this.tBitmap = new Bitmap(Settings.Instance.GraphicWidth, Settings.Instance.GraphicHeight);
        this.tGraphics = Graphics.FromImage(this.tBitmap);
        this.tGraphics.FillRectangle(Brushes.White, this.tGraphics.ClipBounds);
        this.UseTextMode = false;
        this.gLines = linesG;
        this.gCols = colsG;
        base.Size = new Size(Settings.Instance.GraphicWidth + 6, Settings.Instance.GraphicHeight + 0x18);
        this.DoRefresh();
      }
    }

    public void Start()
    {
      if (base.InvokeRequired)
      {
        base.Invoke(new MethodInvoker(this.Start));
      }
      else
      {
        base.Show();
      }
    }

    public void Stop()
    {
      if (base.InvokeRequired)
      {
        base.Invoke(new MethodInvoker(this.Stop));
      }
      else
      {
        if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
        {
          while (this._displayThread.IsAlive)
          {
            Log.Info("DebugForm.Stop(): Stopping DebugForm.Display_Update() Thread", new object[0]);
            lock (ThreadMutex)
            {
              _stopDisplayUpdateThread = true;
            }
            _stopDisplayUpdateThread = true;
            Thread.Sleep(500);
          }
        }
        base.Close();
      }
    }

    string IDisplay.Name
    {
      get
      {
        return "DebugForm";
      }
    }

    public string Description
    {
      get
      {
        return "Debug (Preview) Form 04_24_2008";
      }
    }

    public string ErrorMessage
    {
      get
      {
        return this.errorMessage;
      }
    }

    public bool IsDisabled
    {
      get
      {
        return this.isDisabled;
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
      private bool m_EnableDisplayAction;
      private int m_EnableDisplayActionTime = 5;
      private bool m_EqDisplay;
      private int m_EqRate = 10;
      private bool m_EQTitleDisplay;
      private int m_EQTitleDisplayTime = 10;
      private int m_EQTitleShowTime = 2;
      private static DebugForm.AdvancedSettings m_Instance;
      private bool m_NormalEQ = true;
      private bool m_RestrictEQ;
      private bool m_SmoothEQ;
      private bool m_StereoEQ;
      private bool m_VUindicators;
      private bool m_VUmeter;
      private bool m_VUmeter2;

      public static event OnSettingsChangedHandler OnSettingsChanged;

      private static void Default(DebugForm.AdvancedSettings _settings)
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

      public static DebugForm.AdvancedSettings Load()
      {
        DebugForm.AdvancedSettings settings;
        Log.Debug("DebugForm.AdvancedSettings.Load() started", new object[0]);
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_DebugForm.xml")))
        {
          Log.Debug("DebugForm.AdvancedSettings.Load() Loading settings from XML file", new object[0]);
          XmlSerializer serializer = new XmlSerializer(typeof(DebugForm.AdvancedSettings));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_DebugForm.xml"));
          settings = (DebugForm.AdvancedSettings)serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Debug("DebugForm.AdvancedSettings.Load() Loading settings from defaults", new object[0]);
          settings = new DebugForm.AdvancedSettings();
          Default(settings);
        }
        Log.Debug("DebugForm.AdvancedSettings.Load() completed", new object[0]);
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

      public static void Save(DebugForm.AdvancedSettings ToSave)
      {
        Log.Debug("DebugForm.AdvancedSettings.Save() Saving settings to XML file", new object[0]);
        XmlSerializer serializer = new XmlSerializer(typeof(DebugForm.AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_DebugForm.xml"), Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter)writer, ToSave);
        writer.Close();
        Log.Debug("DebugForm.AdvancedSettings.Save() completed", new object[0]);
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

      public static DebugForm.AdvancedSettings Instance
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

    public delegate void SetLineDelegate(int _line, string message);
  }
}

