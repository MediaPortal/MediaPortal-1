#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

// ReSharper disable CheckNamespace
namespace MediaPortal.GUI.Library
// ReSharper restore CheckNamespace
{
  public delegate void OnActionHandler(Action action);

  public delegate void SendMessageHandler(GUIMessage message);

  public delegate void VideoWindowChangedHandler();

  public delegate void VideoGammaContrastBrightnessHandler();

  public delegate void BlackImageRenderedHandler();

  public delegate void VideoReceivedHandler();


  /// <summary>
  /// Singleton class which holds all GFX related settings
  /// </summary>
  public class GUIGraphicsContext
  {
    public static event BlackImageRenderedHandler OnBlackImageRendered;
    public static event VideoReceivedHandler OnVideoReceived;
    
    private static readonly object RenderLoopLock = new object();  // Rendering loop lock - use this when removing any D3D resources
    private static readonly List<Point> Cameras = new List<Point>();
    private static readonly List<TransformMatrix> GroupTransforms = new List<TransformMatrix>();
    private static TransformMatrix _guiTransform = new TransformMatrix();
    private static TransformMatrix _finalTransform = new TransformMatrix();
    private static TransformMatrix _finalTransformCalibrated = new TransformMatrix();
    private static int _bypassUICalibration;

    // enum containing current state of MediaPortal
    public enum State
    {
      // ReSharper disable InconsistentNaming
      STARTING,
      RUNNING,
      STOPPING,
      SUSPENDING,
      LOST
      // ReSharper restore InconsistentNaming
    }
    
    public static event SendMessageHandler Receivers; // triggered when a message has arrived
    public static event OnActionHandler OnNewAction; // triggered when a action has arrived
    public static event VideoWindowChangedHandler OnVideoWindowChanged; // triggered when the video window location/size or AR have been changed
    public static event VideoGammaContrastBrightnessHandler OnGammaContrastBrightnessChanged; // triggered when contrast, brightness, gamma settings have been changed

    public static Device DX9Device = null; // pointer to current DX9 device
    public static Texture Auto3DTexture = null;
    public static Surface Auto3DSurface = null;

    // ReSharper disable InconsistentNaming
    public static Graphics graphics = null; // GDI+ Graphics object
    public static Form form = null; // Current GDI form
    public static IAutoCrop autoCropper = null;
    // ReSharper restore InconsistentNaming

    private const float DegreeToRadian = 0.01745329f;
    // ReSharper disable InconsistentNaming
    private const int SC_MONITORPOWER = 0xF170;
    private const int WM_SYSCOMMAND = 0x0112;
    private const int MONITOR_ON = -1;
    private const int MONITOR_OFF = 2;
    // ReSharper restore InconsistentNaming

    private static string _skin = "";
    private static string _theme = "";
    private static bool _isFullScreenVideo; // are we in GUI or fullscreen video/tv mode
    private static Rectangle _rectVideo; // video preview window
    private static Geometry.Type _geometryType = Geometry.Type.Normal; // video transformation type (see geometry.cs)
    private static bool _overlay = true; // indicating if the overlay window is allowed to be shown
    private static float _zoomHorizontal = 1.0f; // x zoom of GUI calibration
    private static float _zoomVertical = 1.0f; // y zoom of GUI calibration
    private static DateTime _topBarTimeOut = DateTime.Now;
    private static int _subtitles = 550; // Y position for subtitles
    private static bool _playing; // are playing any media or not
    private static int _brightness = -1;
    private static int _gamma = -1;
    private static int _contrast = -1;
    private static int _saturation = -1;
    private static int _sharpness = -1;
    private static bool _mouseSupport = true;
    private static Size _skinSize = new Size(720, 576);
    private static bool _showBackGround = true; // show the GUI or live tv in the background
    private static int _scrollSpeedVertical = 4;
    private static int _scrollSpeedHorizontal = 3;
    private static int _charsInCharacterSet = 255;
    private static volatile bool _vmr9Active;
    private static int _maxFPS = 60;
    private static long _desiredFrameTime = 100;
    private static float _currentFPS;
    private static long _lasttime;
    private static bool _blankScreen;
    private static bool _idleTimePowerSaving;
    private static bool _turnOffMonitor;
    private static bool _vmr9Allowed = true;
    private static DateTime _lastActivity = DateTime.Now;
    private static Screen _currentScreen;
    private static Screen _currentStartScreen;
    private static int _currentMonitorIdx = -1;
    private static readonly bool IsDX9EXused = OSInfo.OSInfo.VistaOrLater();
    private static bool _allowRememberLastFocusedItem = true;

    // Stacks for matrix transformations.
    private static readonly Stack<Matrix> ProjectionMatrixStack = new Stack<Matrix>();
    private static readonly Stack<FinalTransformBucket> FinalTransformStack = new Stack<FinalTransformBucket>();

    // Stack for managing clip rectangles.
    private static readonly Stack<Rectangle> ClipRectangleStack = new Stack<Rectangle>();

    /// <summary>
    /// This internal class contains the information needed to place the final transform on a stack.
    /// PushMatrix() and PopMatrix() manage the stack.
    /// </summary>
    private class FinalTransformBucket
    {
      private readonly TransformMatrix _finalTransformMatrix = new TransformMatrix();
      private readonly TransformMatrix _finalTransformMatrixCalibrated = new TransformMatrix();

      public FinalTransformBucket(TransformMatrix finalTransform, TransformMatrix finalTransformCalibrated)
      {
        // The matrices on the stack must be copies otherwise they may be illegally manipulated while on the stack.
        _finalTransformMatrix = (TransformMatrix)finalTransform.Clone();
        _finalTransformMatrixCalibrated = (TransformMatrix)finalTransformCalibrated.Clone();
      }

      /// <summary>
      /// 
      /// </summary>
      public TransformMatrix FinalTransform
      {
        get { return _finalTransformMatrix; }
      }

      /// <summary>
      /// 
      /// </summary>
      public TransformMatrix FinalTransformCalibrated
      {
        get { return _finalTransformMatrixCalibrated; }
      }
    }

    // singleton. Don't allow any instance of this class
    private GUIGraphicsContext() {}

    static GUIGraphicsContext()
    {
      Render3DMode = eRender3DMode.None;
      Switch3DSides = false;
    }

    /// <summary>
    /// Set/get last User Activity
    /// </summary>
    public static DateTime LastActivity
    {
      get { return _lastActivity; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool SaveRenderCycles
    {
      get { return _idleTimePowerSaving; }
      set
      {
        // Since every action (like  key press) resets th Blank screenfield we certainly want to check
        // whether we really need to reload the "old" FPS.
        if (_idleTimePowerSaving != value)
        {
          _idleTimePowerSaving = value;

          if (_idleTimePowerSaving)
          {
            MaxFPS = 5;
          }
          else
          {
            using (Settings xmlReader = new MPSettings())
            {
              MaxFPS = xmlReader.GetValueAsInt("screen", "GuiRenderFps", 60);
            }
          }
        }
      }
    }

    /// <summary>
    /// Enable/disable screen output
    /// </summary>
    public static bool BlankScreen
    {
      get { return _blankScreen; }
      set
      {
        if (value == false)
        {
          SaveRenderCycles = false;
        }

        if (value != _blankScreen)
        {
          try
          {
            if (_turnOffMonitor && Form.ActiveForm != null)
            {
              if (Form.ActiveForm.Handle != IntPtr.Zero)
              {
                Win32API.SendMessageA(Form.ActiveForm.Handle, WM_SYSCOMMAND, SC_MONITORPOWER, value ? MONITOR_OFF : MONITOR_ON);
              }
              else
              {
                Log.Warn("GraphicContext: Could not power monitor {0}", value ? "off" : "on");
              }
            }
            _blankScreen = value;

            if (OnVideoWindowChanged != null)
            {
              OnVideoWindowChanged();
            }
          }
          catch (Exception ex)
          {
            Log.Error("GraphicContext: Error setting Blankscreen to {0} - {1}", value, ex.ToString());
          }
        }
      }
    }

    public static object RenderModeSwitch = new Object();

    public enum eRender3DMode { None, SideBySide, TopAndBottom, SideBySideTo2D, TopAndBottomTo2D };

    static eRender3DMode _render3DMode;

    public static eRender3DMode Render3DMode
    {
      get { return _render3DMode; }
      set
      {
        lock (RenderModeSwitch)
        {
          _render3DMode = value;
        }
      }
    }

    public enum eRender3DModeHalf { None, SBSLeft, SBSRight, TABTop, TABBottom };

    public static eRender3DModeHalf Render3DModeHalf { get; set; }

    public static bool Switch3DSides { get; set; }

    public static bool Render3DSubtitle { get; set; }

    public static int Render3DSubtitleDistance { get; set; }

    /// <summary>
    /// Property to enable/disable animations
    /// </summary>
    public static bool Animations { get; set; }

    /// <summary>
    /// property to enable/disable skin-editing mode
    /// </summary>
    public static bool EditMode { get; set; }

    /// <summary>
    /// Property to get and set current screen on witch MP is displayed
    /// </summary>
    // ReSharper disable InconsistentNaming
    public static Screen currentScreen
    // ReSharper restore InconsistentNaming
    {
      get
      {
        return _currentScreen ?? Screen.PrimaryScreen;
      }
      set { _currentScreen = value; }
    }

    /// <summary>
    /// Property to get and set current start screen on witch MP is displayed
    /// </summary>
    // ReSharper disable InconsistentNaming
    public static Screen currentStartScreen
    // ReSharper restore InconsistentNaming
    {
      get
      {
        return _currentStartScreen ?? Screen.PrimaryScreen;
      }
      set { _currentStartScreen = value; }
    }

    /// <summary>
    /// Property to get and set current monitor index for refreshrate setting
    /// </summary>
    // ReSharper disable InconsistentNaming
    public static int currentMonitorIdx
    // ReSharper restore InconsistentNaming
    {
      get
      {
        return _currentMonitorIdx;
      }
      set { _currentMonitorIdx = value; }
    }

    /// <summary>
    /// Property to get windowed/fullscreen state of application
    /// </summary>
    public static bool Fullscreen
    {
      get { return ((Width == currentScreen.Bounds.Width) && (Height == currentScreen.Bounds.Height)); }
    }

    /// <summary>
    /// Resets last user activity & unblanks screen
    /// </summary>
    public static void ResetLastActivity()
    {
      _lastActivity = DateTime.Now;
      BlankScreen = false;
    }

    /// <summary>
    /// Save calibration settings to calibrationWxH.xml
    /// where W=resolution width
    /// H=resolution height
    /// </summary>
    public static void Save()
    {
      string strFileName = Config.GetFile(Config.Dir.Config, String.Format("ScreenCalibration{0}x{1}", Width, Height));
      strFileName += Fullscreen ? ".fs.xml" : ".xml";

      // Log.Info("save {0}" ,strFileName);
      using (var xmlWriter = new Settings(strFileName))
      {
        xmlWriter.SetValue("screen", "offsetx", OffsetX.ToString(CultureInfo.InvariantCulture));
        xmlWriter.SetValue("screen", "offsety", OffsetY.ToString(CultureInfo.InvariantCulture));

        float zoomHorizontal = _zoomHorizontal * 10000f;
        var intZoomHorizontal = (int)zoomHorizontal;
        xmlWriter.SetValue("screen", "zoomhorizontal", intZoomHorizontal.ToString(CultureInfo.InvariantCulture));

        float zoomVertical = _zoomVertical * 10000f;
        var intZoomVertical = (int)zoomVertical;
        xmlWriter.SetValue("screen", "zoomvertical", intZoomVertical.ToString(CultureInfo.InvariantCulture));

        xmlWriter.SetValue("screen", "offsetosd", OSDOffset.ToString(CultureInfo.InvariantCulture));
        xmlWriter.SetValue("screen", "overscanleft", OverScanLeft.ToString(CultureInfo.InvariantCulture));
        xmlWriter.SetValue("screen", "overscantop", OverScanTop.ToString(CultureInfo.InvariantCulture));
        xmlWriter.SetValue("screen", "overscanwidth", OverScanWidth.ToString(CultureInfo.InvariantCulture));
        xmlWriter.SetValue("screen", "overscanheight", OverScanHeight.ToString(CultureInfo.InvariantCulture));

        float pixelRatio = PixelRatio * 10000f;
        var intPixelRatio = (int)pixelRatio;
        xmlWriter.SetValue("screen", "pixelratio", intPixelRatio.ToString(CultureInfo.InvariantCulture));
        xmlWriter.SetValue("screen", "subtitles", _subtitles.ToString(CultureInfo.InvariantCulture));

        Log.Debug("GraphicContext: Settings saved to {0}", strFileName);
      }
    }

    public static void ResetAuto3D()
    {
      if (Auto3DSurface != null)
      {
        Auto3DSurface.ReleaseGraphics();
        Auto3DSurface = null;
      }

      if (Auto3DTexture != null)
      {
        Auto3DTexture.Dispose();
        Auto3DTexture = null;
      }
    }

    /// <summary>
    /// Load calibration values for current resolution
    /// </summary>
    public static void Load()
    {
      OverScanLeft = 0;
      OverScanTop = 0;
      PixelRatio = 1.0f;
      OSDOffset = 0;
      Subtitles = Height - 50;
      OverScanWidth = Width;
      OverScanHeight = Height;
      ZoomHorizontal = 1.0f;
      ZoomVertical = 1.0f;

      GUIGraphicsContext.ResetAuto3D();

      string strFileName = Config.GetFile(Config.Dir.Config, String.Format("ScreenCalibration{0}x{1}", Width, Height));
      strFileName += Fullscreen ? ".fs.xml" : ".xml";

      if (!File.Exists(strFileName))
      {
        Log.Warn("GraphicContext: NO screen calibration file found for resolution {0}x{1}!", Width, Height);
      }
      else
      {
        Log.Debug("GraphicContext: Loading settings from {0}", strFileName);
      }

      using (var xmlReader = new Settings(strFileName))
      {
        try
        {
          OffsetX = xmlReader.GetValueAsInt("screen", "offsetx", 0);
          OffsetY = xmlReader.GetValueAsInt("screen", "offsety", 0);

          OSDOffset = xmlReader.GetValueAsInt("screen", "offsetosd", 0);
          OverScanLeft = xmlReader.GetValueAsInt("screen", "overscanleft", 0);
          OverScanTop = xmlReader.GetValueAsInt("screen", "overscantop", 0);
          OverScanWidth = xmlReader.GetValueAsInt("screen", "overscanwidth", Width);
          OverScanHeight = xmlReader.GetValueAsInt("screen", "overscanheight", Height);
          _subtitles = xmlReader.GetValueAsInt("screen", "subtitles", Height - 50);
          int intPixelRation = xmlReader.GetValueAsInt("screen", "pixelratio", 10000);
          PixelRatio = intPixelRation;
          PixelRatio /= 10000f;

          int intZoomHorizontal = xmlReader.GetValueAsInt("screen", "zoomhorizontal", 10000);
          _zoomHorizontal = intZoomHorizontal / 10000f;

          int intZoomVertical = xmlReader.GetValueAsInt("screen", "zoomvertical", 10000);
          _zoomVertical = intZoomVertical / 10000f;
        }
        catch (Exception ex)
        {
          Log.Error("GraphicContext: Error loading settings from {0} - {1}", strFileName, ex.Message);
        }
      }

      using (Settings xmlReader = new MPSettings())
      {
        _maxFPS = xmlReader.GetValueAsInt("screen", "GuiRenderFps", 60);
        SyncFrameTime();
        _scrollSpeedVertical = xmlReader.GetValueAsInt("gui", "ScrollSpeedDown", 4);
        _scrollSpeedHorizontal = xmlReader.GetValueAsInt("gui", "ScrollSpeedRight", 3);
        Animations = xmlReader.GetValueAsBool("general", "animations", true);
        _turnOffMonitor = xmlReader.GetValueAsBool("general", "turnoffmonitor", false);
      }
    }

    /// <summary>
    /// Send a message to anyone interested
    /// </summary>
    /// <param name="msg">The message.</param>
    public static void SendMessage(GUIMessage msg)
    {
      if (Receivers != null)
      {
        Receivers(msg);
      }
    }

    /// <summary>
    /// Send a action to anyone interested
    /// </summary>
    /// <param name="action">The message.</param>
    public static void OnAction(Action action)
    {
      if (OnNewAction != null)
      {
        OnNewAction(action);
      }
    }

    /// <summary>
    /// Return screen/window Height
    /// </summary>
    public static int Height
    {
      get
      {
        return DX9Device != null ? DX9Device.PresentationParameters.BackBufferHeight : form.ClientSize.Height;
      }
    }

    /// <summary>
    /// Return screen/window Width.
    /// </summary>
    public static int Width
    {
      get
      {
        return DX9Device != null ? DX9Device.PresentationParameters.BackBufferWidth : form.ClientSize.Width;
      }
    }

    /// <summary>
    /// Gets the center of MP's current Window Area
    /// </summary>
    public static Point OutputScreenCenter
    {
      get
      {
        var clientCenter = new Point(form.ClientSize.Width / 2, form.ClientSize.Height / 2);
        try
        {
          return form.PointToScreen(clientCenter);
        }
        catch (Exception)
        {
          return new Point(0, 0);
        }

      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interpolate"></param>
    public static void ResetCursor(bool interpolate)
    {
      int newX, newY;
      if (interpolate)
      {
        Point oldPos = Cursor.Position;
        if (form.ClientRectangle.Width - oldPos.X < OutputScreenCenter.X)
        {
          newX = OutputScreenCenter.X + (oldPos.X - OutputScreenCenter.X) / 2;
        }
        else
        {
          newX = OutputScreenCenter.X - (OutputScreenCenter.X - oldPos.X) / 2;
        }

        if (form.ClientRectangle.Height - oldPos.Y < OutputScreenCenter.Y)
        {
          newY = OutputScreenCenter.Y + (oldPos.Y - OutputScreenCenter.Y) / 2;
        }
        else
        {
          newY = OutputScreenCenter.Y - (OutputScreenCenter.Y - oldPos.Y) / 2;
        }
      }
      else
      {
        newX = OutputScreenCenter.X;
        newY = OutputScreenCenter.Y;
      }
      Cursor.Position = new Point(newX, newY);
      Cursor.Hide();
    }

    /// <summary>
    /// Get the transformation matrix that corrects for UI Calibration translation
    /// </summary>
    /// <returns>The correction transform</returns>
    public static TransformMatrix GetOffsetCorrectionTransform()
    {
      return TransformMatrix.CreateTranslation(OffsetX, OffsetY, 0);
    }

    /// <summary>
    /// Apply screen offset correct 
    /// </summary>
    /// <param name="x">X correction.</param>
    /// <param name="y">Y correction.</param>
    public static void Correct(ref float x, ref float y)
    {
      x += OffsetX;
      y += OffsetY;
    }

    /// <summary>
    /// Scale rectangle for current resolution.
    /// </summary>
    /// <param name="left">left side</param>
    /// <param name="top">top side</param>
    /// <param name="right">right side</param>
    /// <param name="bottom">bottom side</param>
    public static void ScaleRectToScreenResolution(ref int left, ref int top, ref int right, ref int bottom)
    {
      // X
      float percentX = Width * ZoomHorizontal / _skinSize.Width;
      left = (int)Math.Round(left * percentX);
      right = (int)Math.Round(right * percentX);

      // Y
      float percentY = Height * ZoomVertical / _skinSize.Height;
      top = (int)Math.Round(top * percentY);
      bottom = (int)Math.Round(bottom * percentY);
    }

    /// <summary>
    /// Scale position for current resolution
    /// </summary>
    /// <param name="x">X coordinate to scale.</param>
    /// <param name="y">Y coordinate to scale.</param>
    public static void ScalePosToScreenResolution(ref int x, ref int y)
    {
      float percentX = Width * ZoomHorizontal / _skinSize.Width;
      float percentY = Height * ZoomVertical / _skinSize.Height;
      x = (int)Math.Round(x * percentX);
      y = (int)Math.Round(y * percentY);
    }

    /// <summary>
    /// Scale y position for current resolution
    /// </summary>
    /// <param name="y">Y coordinate to scale.</param>
    public static void ScaleVertical(ref int y)
    {
      // Adjust for global zoom.
      float zoomedScreenHeight = Height * ZoomVertical;
      float percentY = zoomedScreenHeight / _skinSize.Height;
      y = (int)Math.Round(y * percentY);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="y"></param>
    public static void ScaleVertical(ref float y)
    {
      float percentY = Height * ZoomVertical / _skinSize.Height;
      y = (float)Math.Round(y * percentY);
    }

    /// <summary>
    /// Scale y position for current resolution
    /// </summary>
    /// <param name="y">Y coordinate to scale.</param>
    public static int ScaleVertical(int y)
    {
      int sy = y;
      ScaleVertical(ref sy);
      return sy;
    }

    /// <summary>
    /// Scale X position for current resolution
    /// </summary>
    /// <param name="x">X coordinate to scale.</param>
    public static void ScaleHorizontal(ref int x)
    {
      float percentX = Width * ZoomHorizontal / _skinSize.Width;
      x = (int)Math.Round(x * percentX);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    public static void ScaleHorizontal(ref float x)
    {
      float percentX = Width * ZoomHorizontal / _skinSize.Width;
      x = (float)Math.Round(x * percentX);
    }


    /// <summary>
    /// Scale X position for current resolution
    /// </summary>
    /// <param name="x">X coordinate to scale.</param>
    public static int ScaleHorizontal(int x)
    {
      int sx = x;
      ScaleHorizontal(ref sx);
      return sx;
    }

    /// <summary>
    /// Descale a position from screen->skin resolutions
    /// </summary>
    /// <param name="x">X coordinate to descale.</param>
    /// <param name="y">Y coordinate to descale.</param>
    public static void DescalePosToScreenResolution(ref int x, ref int y)
    {
      float percentX = _skinSize.Width / (Width * ZoomHorizontal);
      float percentY = _skinSize.Height / (Height * ZoomVertical);
      x = (int)Math.Round(x * percentX);
      y = (int)Math.Round(y * percentY);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void BlackImageRendered()
    {
      if (OnBlackImageRendered != null)
      {
        OnBlackImageRendered();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void VideoReceived()
    {
      if (OnVideoReceived != null)
      {
        OnVideoReceived();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool RenderBlackImage { get; set; }

    /// <summary>
    /// Get/set current Aspect Ratio Mode
    /// </summary>
    public static Geometry.Type ARType
    {
      get { return _geometryType; }
      set
      {
        _geometryType = value;
        if (OnVideoWindowChanged != null)
        {
          OnVideoWindowChanged();
        }
      }
    }

    /// <summary>
    /// Get/set current skin folder path
    /// </summary>
    public static string Skin
    {
      set { 
        _skin = Config.GetSubFolder(Config.Dir.Skin, value);
        _theme = _skin; // The default theme is the skin itself.
      }
      get { return _skin; }
    }

    /// <summary>
    /// Convenience property to get just the name of the skin.
    /// </summary>
    public static string SkinName
    {
      get { return _skin.Substring(_skin.LastIndexOf(@"\", StringComparison.Ordinal) + 1); }
    }

    /// <summary>
    /// Get/set current skin theme folder path.  Returns the default skin path if no theme folder found.
    /// </summary>
    public static string Theme
    {
      set { _theme = Skin + (value != GUIThemeManager.THEME_SKIN_DEFAULT ? @"\Themes\" + value : ""); }
      get { return _theme; }
    }

    /// <summary>
    /// Convenience property to get just the name of the skin theme.
    /// </summary>
    public static string ThemeName
    {
      get
      {
        return _theme.Contains(@"\Themes\") ? _theme.Substring(_theme.LastIndexOf(@"\", StringComparison.Ordinal) + 1) : GUIThemeManager.THEME_SKIN_DEFAULT;
      }
    }

    /// <summary>
    /// Returns true if the current theme has the specified file.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static bool HasThemeSpecificSkinFile(string filename)
    {
      // Do not check for files in the default theme (base skin).
      return ThemeName != GUIThemeManager.THEME_SKIN_DEFAULT && File.Exists(Theme + filename);
    }

    /// <summary>
    /// Return a themed version of the requested skin filename, otherwise return the default skin filename.  Use a path to media to get images.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string GetThemedSkinFile(string filename)
    {
      return File.Exists(Theme + filename) ? Theme + filename : Skin + filename;
    }

    /// <summary>
    /// Return a themed version of the requested directory, otherwise return the default skin directory.
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string GetThemedSkinDirectory(string dir)
    {
      return Directory.Exists(Theme + dir) ? Theme + dir : Skin + dir;
    }

    /// <summary>
    /// Gets the current skin cache folder
    /// </summary>
    public static string SkinCacheFolder
    {
      get
      {
        string skinName = _skin.Substring(_skin.LastIndexOf(@"\", StringComparison.Ordinal));
        return string.Format("{0}{1}", Config.GetFolder(Config.Dir.Cache), skinName);
      }
    }

    /// <summary>
    /// Get/Set vertical offset for the OSD
    /// </summary>
    public static int OSDOffset { get; set; }

    /// <summary>
    /// Get/Set left calibration value
    /// </summary>
    public static int OverScanLeft { get; set; }

    /// <summary>
    /// Get/Set upper calibration value
    /// </summary>
    public static int OverScanTop { get; set; }

    /// <summary>
    /// Get/Set calibration width
    /// </summary>
    public static int OverScanWidth { get; set; }

    /// <summary>
    /// Get/Set calibration height
    /// </summary>
    public static int OverScanHeight { get; set; }

    /// <summary>
    /// Get/Set current pixel Ratio
    /// </summary>
    public static float PixelRatio  { get; set; }

    /// <summary>
    /// get /set whether we're playing a movie , visz or TV in
    /// fullscreen mode or in windowed (preview) mode
    /// </summary>
    public static bool IsFullScreenVideo
    {
      get { return _isFullScreenVideo; }
      set
      {
        if (value != _isFullScreenVideo)
        {
          _isFullScreenVideo = value;
          if (OnVideoWindowChanged != null)
          {
            OnVideoWindowChanged();
          }
        }
      }
    }

    /// <summary>
    /// Get/Set video window rectangle
    /// </summary>
    public static Rectangle VideoWindow
    {
      get { return _rectVideo; }
      set
      {
        if (!_rectVideo.Equals(value))
        {
          _rectVideo = value;
          if (OnVideoWindowChanged != null)
          {
            OnVideoWindowChanged();
          }
        }
      }
    }

    /// <summary>
    /// Get/Set application state (starting,running,stopping)
    /// </summary>
    public static State CurrentState { get; set; }

    /// <summary>
    /// Get pointer to the applications form (needed by overlay windows)
    /// </summary>
    public static IntPtr ActiveForm { get; set; }

    /// <summary>
    /// return whether we're currently calibrating or not
    /// </summary>
    public static bool Calibrating { get; set; }

    /// <summary>
    /// Get/Set whether overlay window is enabled or disabled
    /// </summary>
    public static bool Overlay
    {
      get { return _overlay; }
      set
      {
        // SE: Mantis 3474
        // some windows have overlay = false, but still have videocontrol.
        // switching to another window with overlay = false but without videocontrol will
        // leave old videocontrol "hanging" on screen (since dimensions aren't updated)
        // if (m_bOverlay != value)
        {
          bool bOldOverlay = _overlay;
          _overlay = value;
          if (!ShowBackground)
          {
            _overlay = false;
          }

          if (!_overlay)
          {
            VideoWindow = new Rectangle(0, 0, 1, 1);
          }

          if (bOldOverlay != _overlay && OnVideoWindowChanged != null)
          {
            OnVideoWindowChanged();
          }
        }
      }
    }

    /// <summary>
    /// Get/Set left screen calibration
    /// </summary>
    public static int OffsetX { get; set; }
 
    /// <summary>
    /// Get/Set upper screen calibration
    /// </summary>
    public static int OffsetY { get; set; }

    /// <summary>
    /// Get/Set vertical zoom screen calibration
    /// </summary>
    public static float ZoomVertical
    {
      get { return _zoomVertical; }
      set { _zoomVertical = value; }
    }

    /// <summary>
    /// Get/Set vertical zoom screen calibration
    /// </summary>
    public static float ZoomHorizontal
    {
      get { return _zoomHorizontal; }
      set { _zoomHorizontal = value; }
    }

    /// <summary>
    /// Get/Set topbar hidden status
    /// </summary>
    public static bool TopBarHidden { get; set; }

    /// <summary>
    /// Get/Set topbar autohide status
    /// </summary>
    public static bool AutoHideTopBar { get; set; }

    /// <summary>
    /// Get/Set default topbar autohide status
    /// </summary>
    public static bool DefaultTopBarHide { get; set; }

    /// <summary>
    /// Get/Set topbar timeout
    /// </summary>
    public static DateTime TopBarTimeOut
    {
      get { return _topBarTimeOut; }
      set { _topBarTimeOut = value; }
    }

    /// <summary>
    /// Get/Set disable topbar view status
    /// </summary>
    public static bool DisableTopBar { get; set; }

    /// <summary>
    /// Get/Set Y-position for subtitles
    /// </summary>
    public static int Subtitles
    {
      get { return _subtitles; }
      set { _subtitles = value; }
    }

    /// <summary>
    /// Calculates a rectangle based on current calibration values/pixel ratio
    /// </summary>
    /// <param name="sourceWidth">width of source rectangle</param>
    /// <param name="sourceHeight">height of source rectangle</param>
    /// <param name="maxWidth">max. width allowed</param>
    /// <param name="maxHeight">max. height allowed</param>
    /// <param name="width">returned width of calculated rectangle</param>
    /// <param name="height">returned height of calculated rectangle</param>
    public static void GetOutputRect(int sourceWidth, int sourceHeight, int maxWidth, int maxHeight, out int width, out int height)
    {
      // calculate aspect ratio correction factor
      float outputFrameAR = (float)sourceWidth / sourceHeight / PixelRatio;

      width = maxWidth;
      height = (int)(width / outputFrameAR);
      
      if (height > maxHeight)
      {
        height = maxHeight;
        width = (int)(height * outputFrameAR);
      }
    }

    /// <summary>
    /// Get/Set whether a file (music or video) is currently playing
    /// </summary>
    public static bool IsPlaying
    {
      get { return _playing; }
      set
      {
        _playing = value;
        if (!_playing)
        {
          IsPlayingVideo = false;
        }
      }
    }


    /// <summary>
    /// Get/Set whether a a movie (or livetv) is currently playing
    /// </summary>
    public static bool IsPlayingVideo { get; set; }

    /// <summary>
    /// Get/Set the Brightness.
    /// </summary>
    public static int Brightness
    {
      get { return _brightness; }
      set
      {
        if (_brightness != value)
        {
          _brightness = value;
          if (OnGammaContrastBrightnessChanged != null)
          {
            OnGammaContrastBrightnessChanged();
          }
        }
      }
    }

    /// <summary>
    /// Get/Set the Contrast.
    /// </summary>
    public static int Contrast
    {
      get { return _contrast; }
      set
      {
        if (_contrast != value)
        {
          _contrast = value;
          if (OnGammaContrastBrightnessChanged != null)
          {
            OnGammaContrastBrightnessChanged();
          }
        }
      }
    }

    /// <summary>
    /// Get/Set the Gamma.
    /// </summary>
    public static int Gamma
    {
      get { return _gamma; }
      set
      {
        if (_gamma != value)
        {
          _gamma = value;
          if (OnGammaContrastBrightnessChanged != null)
          {
            OnGammaContrastBrightnessChanged();
          }
        }
      }
    }

    /// <summary>
    /// Get/Set the Saturation.
    /// </summary>
    public static int Saturation
    {
      get { return _saturation; }
      set
      {
        if (_saturation != value)
        {
          _saturation = value;
          if (OnGammaContrastBrightnessChanged != null)
          {
            OnGammaContrastBrightnessChanged();
          }
        }
      }
    }

    /// <summary>
    /// Get/Set the Sharpness.
    /// </summary>
    public static int Sharpness
    {
      get { return _sharpness; }
      set
      {
        if (_sharpness != value)
        {
          _sharpness = value;
          if (OnGammaContrastBrightnessChanged != null)
          {
            OnGammaContrastBrightnessChanged();
          }
        }
      }
    }

    /// <summary>
    /// Get/Set  if there is MouseSupport.
    /// </summary>
    public static bool MouseSupport
    {
      get { return _mouseSupport; }
      set { _mouseSupport = value; }
    }

    /// <summary>
    /// Get/Set  if it is allowed to remember last focused item on supported window/skin 
    /// </summary>
    public static bool AllowRememberLastFocusedItem
    {
      get { return _allowRememberLastFocusedItem; }
      set { _allowRememberLastFocusedItem = value; }
    }

    /// <summary>
    /// Get/Set  if we want to use double click to be used as right click
    /// </summary>
    public static bool DBLClickAsRightClick { get; set; }

    /// <summary>
    /// Get/Set the size of the skin.
    /// </summary>	
    public static Size SkinSize
    {
      get { return _skinSize; }
      set { _skinSize = value; }
    }

    /// <summary>
    /// Get/Set whether we should show the GUI as background or 
    /// live tv as background
    /// </summary>
    public static bool ShowBackground
    {
      get { return _showBackGround; }
      set
      {
        _showBackGround = value;
        if (!_showBackGround)
        {
          Overlay = false;
        }
        GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
        if (window != null)
        {
          window.UpdateOverlay();
        }
      }
    }

    /// <summary>
    /// Get/Set the current scroll speed 
    /// </summary>
    public static int ScrollSpeedVertical
    {
      get { return _scrollSpeedVertical; }
      set
      {
        if (_scrollSpeedVertical < 0)
        {
          return;
        }
        _scrollSpeedVertical = value;
      }
    }

    /// <summary>
    /// Get/Set the current scroll speed 
    /// </summary>
    public static int ScrollSpeedHorizontal
    {
      get { return _scrollSpeedHorizontal; }
      set
      {
        if (_scrollSpeedHorizontal < 0)
        {
          return;
        }
        _scrollSpeedHorizontal = value;
      }
    }

    /// <summary>
    /// Get/Set the current maximum number of FPS
    /// </summary>
    public static int MaxFPS
    {
      get { return _maxFPS; }
      set
      {
        if (_maxFPS < 0)
        {
          return;
        }
        _maxFPS = value;
        SyncFrameTime();
        Log.Info("GraphicContext: MP will render at {0} FPS, use animations = {1}", _maxFPS, Animations.ToString());
      }
    }

    /// <summary>
    /// Get the number of ticks for each frame to get MaxFPS
    /// </summary>
    public static long DesiredFrameTime
    {
      get { return _desiredFrameTime; }
    }

    /// <summary>
    /// Get/Set the current maximum number of FPS
    /// </summary>
    public static float CurrentFPS
    {
      get { return _currentFPS; }
      set
      {
        if (_currentFPS < 0)
        {
          return;
        }
        _currentFPS = value;
      }
    }

    /// <summary>
    /// Get/Set the number of characters used for the fonts
    /// </summary>
    public static int CharsInCharacterSet
    {
      get { return _charsInCharacterSet; }
      set
      {
        if (_charsInCharacterSet < 128)
        {
          return;
        }
        _charsInCharacterSet = value;
        Log.Info("GraphicContext: Using {0} chars of the character set. ", _charsInCharacterSet);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static IRender RenderGUI { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static float Vmr9FPS { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static bool Vmr9Active
    {
      get { return _vmr9Active; }
      set
      {
        if (value != _vmr9Active)
        {
          _vmr9Active = value;
          Log.Debug(_vmr9Active ? "VMR9: Now active" : "VMR9: Inactive");
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool IsVMR9Exclusive { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static bool IsEvr { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static float TimePassed
    {
      get
      {
        long time = Stopwatch.GetTimestamp();
        float difftime = (float)(time - _lasttime) / Stopwatch.Frequency;
        _lasttime = time;
        return difftime;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool InVmr9Render  { get; set; }

    /// <summary>
    /// 
    /// </summary>
    private static void SyncFrameTime()
    {
      _desiredFrameTime = DXUtil.TicksPerSecond / _maxFPS;
    }

    /// <summary>
    /// 
    /// </summary>
    public static PresentParameters DirectXPresentParameters { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static bool VMR9Allowed
    {
      get { return _vmr9Allowed; }
      set { _vmr9Allowed = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static Size VideoSize { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static bool HasFocus { get; set; }

    /// <summary>
    /// Returns true if the active window belongs to the my tv plugin
    /// </summary>
    /// <returns>
    /// true: belongs to the my tv plugin
    /// false: does not belong to the my tv plugin</returns>
    public static bool IsTvWindow()
    {
      int windowId = GUIWindowManager.ActiveWindow;
      return IsTvWindow(windowId);
    }

    /// <summary>
    /// Returns true if the specified window belongs to the my tv plugin
    /// </summary>
    /// <param name="windowId">id of window</param>
    /// <returns>
    /// true: belongs to the my tv plugin
    /// false: does not belong to the my tv plugin</returns>
    public static bool IsTvWindow(int windowId)
    {
      GUIWindow window = GUIWindowManager.GetWindow(windowId);

      if (window != null && window.IsTv)
      {
        return true;
      }

      switch (windowId)
      {
        case (int)GUIWindow.Window.WINDOW_TV:
        case (int)GUIWindow.Window.WINDOW_TVFULLSCREEN:
        case (int)GUIWindow.Window.WINDOW_TVGUIDE:
        case (int)GUIWindow.Window.WINDOW_RECORDEDTV:
        case (int)GUIWindow.Window.WINDOW_SCHEDULER:
        case (int)GUIWindow.Window.WINDOW_SEARCHTV:
        case (int)GUIWindow.Window.WINDOW_TELETEXT:
        case (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT:
        case (int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES:
        case (int)GUIWindow.Window.WINDOW_TV_NO_SIGNAL:
        case (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO:
          return true;
      }
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsDirectX9ExUsed()
    {
      return IsDX9EXused;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Pool GetTexturePoolType()
    {
      // DirectX9 Ex device works only with Pool.Default
      if (IsDirectX9ExUsed())
      {
        return Pool.Default;
      }
      return Pool.Managed;
    }

    /// <summary>
    /// 
    /// </summary>
    public static TransformMatrix ControlTransform
    {
      get { return _finalTransform; }
      set
      {
        _finalTransform = value;
        _finalTransformCalibrated = GetOffsetCorrectionTransform().multiply(_finalTransform);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static object RenderLock
    {
      get { return RenderLoopLock; }
    }

    /// <summary>
    /// Enable/Disable bypassing of UI Calibration transforms
    /// </summary>
    /// <remarks>Calls have to be paired and can be nested.</remarks>
    /// <param name="bypass">true to enable bypassing</param>
    public static void BypassUICalibration(bool bypass)
    {
      if (bypass)
      {
        _bypassUICalibration++;
      }
      else
      {
        _bypassUICalibration--;
        if (_bypassUICalibration < 0)
        {
          _bypassUICalibration = 0;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <param name="needsScaling"></param>
    public static void SetScalingResolution(int posX, int posY, bool needsScaling)
    {
      if (!needsScaling)
      {
        _guiTransform = TransformMatrix.CreateTranslation(posX, posY, 0);
      }

      Cameras.Clear();
      Cameras.Add(new Point(Width / 2, Height / 2));
      UpdateCameraPosition(Cameras[Cameras.Count-1]);

      // reset the final transform and window transforms
      UpdateFinalTransform(_guiTransform);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    public static void UpdateFinalTransform(TransformMatrix matrix)
    {
      _finalTransform = matrix;
      _finalTransformCalibrated = GetOffsetCorrectionTransform().multiply(matrix);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static TransformMatrix GetFinalTransform()
    {
      return _bypassUICalibration > 0 ? _finalTransform : _finalTransformCalibrated;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static float[,] GetFinalMatrix()
    {
      return GetFinalTransform().Matrix;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static float ScaleFinalXCoord(float x, float y)
    {
      return GetFinalTransform().TransformXCoord(x, y, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static float ScaleFinalYCoord(float x, float y)
    {
      return GetFinalTransform().TransformYCoord(x, y, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static float ScaleFinalZCoord(float x, float y)
    {
      return GetFinalTransform().TransformZCoord(x, y, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public static void ScaleFinalCoords(ref float x, ref float y, ref float z)
    {
      GetFinalTransform().TransformPosition(ref x, ref y, ref z);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static uint MergeAlpha(uint color)
    {
      uint alpha = GetFinalTransform().TransformAlpha((color >> 24) & 0xff);
      return ((alpha << 24) & 0xff000000) | (color & 0xffffff);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    public static void SetWindowTransform(TransformMatrix matrix)
    {
      // reset the group transform stack
      GroupTransforms.Clear();
      GroupTransforms.Add(_guiTransform.multiply(matrix));
      _bypassUICalibration = 0;
      UpdateFinalTransform(GroupTransforms[0]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix"></param>
    public static void AddTransform(TransformMatrix matrix)
    {
      GroupTransforms.Add(GroupTransforms.Count > 0
                             ? GroupTransforms[GroupTransforms.Count - 1].multiply(matrix)
                             : matrix);
      UpdateFinalTransform(GroupTransforms[GroupTransforms.Count - 1]);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void RemoveTransform()
    {
      if (GroupTransforms.Count > 0)
      {
        GroupTransforms.RemoveAt(GroupTransforms.Count - 1);
      }
      UpdateFinalTransform(GroupTransforms.Count > 0
                             ? GroupTransforms[GroupTransforms.Count - 1]
                             : new TransformMatrix());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="camera"></param>
    public static void SetCameraPosition(Point camera)
    {
      // Position the camera relative to the default camera location (the geometric center of the screen).
      // The default camera is always camera [0].
      var cam = new Point(Cameras[0].X + camera.X, Cameras[0].Y + camera.Y);
      Cameras.Add(cam);
      UpdateCameraPosition(Cameras[Cameras.Count-1]);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void RestoreCameraPosition()
    {
      // It's an error to remove the default camera.
      if (Cameras.Count > 1)
      {
        Cameras.RemoveAt(Cameras.Count - 1);
      }
      else
      {
        Log.Error("GUIGraphicsContext: RestoreCameraPosition() - attempt to remove the default camera; calls to set a camera position should not be made from Render().");
      }
      UpdateCameraPosition(Cameras[Cameras.Count-1]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="camera"></param>
    public static void UpdateCameraPosition(Point camera)
    {
      // NOTE: This routine is currently called (twice) every time there is a <camera>
      //       tag in the skin.  It actually only has to be called before we render
      //       something, so another option is to just save the camera coordinates
      //       and then have a routine called before every draw that checks whether
      //       the camera has changed, and if so, changes it.  Similarly, it could set
      //       the world transform at that point as well (or even combine world + view
      //       to cut down on one setting)

      // and calculate the offset from the screen center
      var offset = new Point(camera.X - (Width / 2), camera.Y - (Height / 2));

      // grab the viewport dimensions and location
      Viewport viewport = DX9Device.Viewport;
      float w = viewport.Width * 0.5f;
      float h = viewport.Height * 0.5f;

      // world view.  Until this is moved onto the GPU (via a vertex shader for instance), we set it to the identity
      // here.
      Matrix mtxWorld = Matrix.Identity;
      DX9Device.Transform.World = mtxWorld;
      // camera view.  Multiply the Y coord by -1 then translate so that everything is relative to the camera
      // position.
      Matrix flipY = Matrix.Scaling(1.0f, -1.0f, 1.0f);
      Matrix translate = Matrix.Translation(-(viewport.X + w + offset.X), -(viewport.Y + h + offset.Y), 2 * h);
      Matrix mtxView = Matrix.Multiply(translate, flipY);
      DX9Device.Transform.View = mtxView;

      // projection onto screen space
      Matrix mtxProjection = Matrix.PerspectiveOffCenterLH((-w - offset.X) * 0.5f, //Minimum x-value of the view volume.
                                                           (w - offset.X) * 0.5f, //Maximum x-value of the view volume.
                                                           (-h + offset.Y) * 0.5f, //Minimum y-value of the view volume.
                                                           (h + offset.Y) * 0.5f, //Maximum y-value of the view volume.
                                                           h, //Minimum z-value of the view volume.
                                                           100 * h); //Maximum z-value of the view volume.
      DX9Device.Transform.Projection = mtxProjection;
    }

    #region Matrix Management and Transformations

    /// <summary>
    /// This is a convenience method for users who wish to manage the control transform matrix with an OpenGL-like matrix stack.
    /// The matrix stack managed is *not* used to set the final transform for MP callers who do not use Push/Pop.
    /// </summary>
    public static void PushMatrix()
    {
      // Push the current transform matrix onto the stack.
      FinalTransformStack.Push(new FinalTransformBucket(_finalTransform, _finalTransformCalibrated));
    }

    /// <summary>
    /// This is a convenience method for users who wish to manage the control transform matrix with an OpenGL-like matrix stack.
    /// The matrix stack managed is *not* used to set the final transform for MP callers who do not use Push/Pop.
    /// </summary>
    public static void PopMatrix()
    {
      // Pop the transform matrix off of the stack.
      if (FinalTransformStack.Count > 0)
      {
        FinalTransformBucket bucket = FinalTransformStack.Pop();
        _finalTransform = bucket.FinalTransform;
        _finalTransformCalibrated = bucket.FinalTransformCalibrated;
      }
      else
      {
        throw new InvalidOperationException("Attempt to pop final transform matrix off of an empty stack");
      }
    }

    /// <summary>
    /// This is a convenience method for users who wish to manage the direct3d projection matrix with an OpenGL-like matrix stack.
    /// The matrix stack managed is *not* used to set the direct3d projection matrix for MP callers who do not use Push/Pop.
    /// </summary>
    public static void PushProjectionMatrix()
    {
      // Push the current direct3d projection matrix onto the stack.
      ProjectionMatrixStack.Push(DX9Device.Transform.Projection);
    }

    /// <summary>
    /// This is a convenience method for users who wish to manage the direct3d projection matrix with an OpenGL-like matrix stack.
    /// The matrix stack managed is *not* used to set the direct3d projection matrix for MP callers who do not use Push/Pop.
    /// </summary>
    public static void PopProjectionMatrix()
    {
      // Pop the direct3d projection matrix off of the stack.
      if (ProjectionMatrixStack.Count > 0)
      {
        DX9Device.Transform.Projection = ProjectionMatrixStack.Pop();
      }
      else
      {
        throw new InvalidOperationException("Attempt to pop direct3d projection matrix off of an empty stack");
      }
    }

    /// <summary>
    /// Sets the direct3d project matrix to the specfied perspective view.
    /// </summary>
    public static void SetPerspectiveProjectionMatrix(float fovy, float aspectratio, float nearPlane, float farPlane)
    {
      DX9Device.Transform.Projection = Matrix.PerspectiveFovLH(fovy, aspectratio, nearPlane, farPlane);
    }

    /// <summary>
    /// Replaces the current control transform with the identity matrix.
    /// </summary>
    public static void LoadIdentity()
    {
      UpdateFinalTransform(ControlTransform.multiplyAssign(new TransformMatrix()));
    }

    /// <summary>
    /// Rotates the control transform matrix by the specified angle (in degrees) around the x-axis.
    /// </summary>
    public static void RotateX(float angle, float y, float z)
    {
      var m = new TransformMatrix();
      angle *= DegreeToRadian;
      m.SetXRotation(angle, y, z, 1.0f);
      UpdateFinalTransform(ControlTransform.multiplyAssign(m));
    }

    /// <summary>
    /// Rotates the control transform matrix by the specified angle (in degrees) around the y-axis.
    /// </summary>
    public static void RotateY(float angle, float x, float z)
    {
      var m = new TransformMatrix();
      angle *= DegreeToRadian;
      m.SetYRotation(angle, x, z, 1.0f);
      UpdateFinalTransform(ControlTransform.multiplyAssign(m));
    }

    /// <summary>
    /// Rotates the control transform matrix by the specified angle (in degrees) around the z-axis.
    /// </summary>
    public static void RotateZ(float angle, float x, float y)
    {
      var m = new TransformMatrix();
      angle *= DegreeToRadian;
      m.SetZRotation(angle, x, y, 1.0f);
      UpdateFinalTransform(ControlTransform.multiplyAssign(m));
    }

    /// <summary>
    /// Scales the control transform matrix by the specified vector.
    /// </summary>
    public static void Scale(float x, float y, float z)
    {
      TransformMatrix m = TransformMatrix.CreateScaler(x, y, z);
      UpdateFinalTransform(ControlTransform.multiplyAssign(m));
    }

    /// <summary>
    /// Translates the control transform matrix by the specified vector.
    /// </summary>
    public static void Translate(float x, float y, float z)
    {
      TransformMatrix m = TransformMatrix.CreateTranslation(x, y, z);
      UpdateFinalTransform(ControlTransform.multiplyAssign(m));
    }

    #endregion

    /// <summary>
    /// Returns the current clip rectangle.
    /// </summary>
    public static Rectangle GetClipRect()
    {
      return DX9Device.ScissorRectangle;
    }

    /// <summary>
    /// Sets a clip region. Set the clip rectangle as specified and enables the FontEngine to use clipping.
    /// </summary>
    /// <param name="rect"></param>
    public static void BeginClip(Rectangle rect)
    {
      BeginClip(rect, true);
    }

    /// <summary>
    /// Sets a clip region. Set the clip rectangle as specified and enables the FontEngine to use clipping.  If constrain is true then
    /// nested calls will clip the specified clip rectangle at the parents clip rectangle.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="constrain"></param>
    public static void BeginClip(Rectangle rect, bool constrain)
    {
      Rectangle r3 = rect;

      if (constrain && ClipRectangleStack.Count > 0)
      {
        // Default behavior for nested clipping is handled by not disturbing the outer clip rectangle.
        // Nested clip rectangles are themselves clipped at the boundary of the outer clip rectangle.
        Rectangle r1 = ClipRectangleStack.Peek();
        Rectangle r2 = rect;
        r3 = r1; // Default result is the clip rectangle on the top of the stack.

        bool intersect = !(r2.Left > r1.Right || r2.Right < r1.Left || r2.Top > r1.Bottom || r2.Bottom < r1.Top);

        if (intersect)
        {
          int x = Math.Max(r1.Left, r2.Left);
          int y = Math.Max(r1.Top, r2.Top);
          int width = Math.Min(r1.Right, r2.Right) - x;
          int height = Math.Min(r1.Bottom, r2.Bottom) - y;
          r3 = new Rectangle(x, y, width, height);
        }
      }

      // Place the clip rectangle on the top of the stack and set it as the current clip rectangle.
      ClipRectangleStack.Push(r3);
      DX9Device.ScissorRectangle = ClipRectangleStack.Peek();
      DXNative.FontEngineSetClipEnable();
    }

    /// <summary>
    /// Removes a clip region. Disables the FontEngine from using current clipping context.
    /// </summary>
    public static void EndClip()
    {
      // Remove the current clip rectangle.
      ClipRectangleStack.Pop();

      // If the clip stack is empty then tell the font engine to stop clipping otherwise restore the current clip rectangle.
      if (ClipRectangleStack.Count == 0)
      {
        DXNative.FontEngineSetClipDisable();
      }
      else
      {
        DX9Device.ScissorRectangle = ClipRectangleStack.Peek();
      }
    }
  }
}