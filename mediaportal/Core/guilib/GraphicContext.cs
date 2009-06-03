#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Player;
using MediaPortal.Profile;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
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

    private static bool _renderBlackImage = false;
    private static List<Point> _cameras = new List<Point>();
    private static List<TransformMatrix> _groupTransforms = new List<TransformMatrix>();
    private static TransformMatrix _guiTransform = new TransformMatrix();
    private static TransformMatrix _finalTransform = new TransformMatrix();
    //enum containing current state of mediaportal
    public enum State
    {
      STARTING, // starting up
      RUNNING, // running
      STOPPING, // stopping
      SUSPENDING, // system in suspend mode
      LOST
    }

    /// <summary>
    /// Event which will be triggered when a message has arrived
    /// </summary>
    public static event SendMessageHandler Receivers;

    /// <summary>
    /// Event which will be triggered when a action has arrived
    /// </summary>
    public static event OnActionHandler OnNewAction;


    /// <summary>
    /// Event which will be triggered when the video window location/size or AR have been changed
    /// </summary>
    public static event VideoWindowChangedHandler OnVideoWindowChanged;

    /// <summary>
    /// Event which will be triggered when contrast,brightness,gamma settings have been changed
    /// </summary>
    public static event VideoGammaContrastBrightnessHandler OnGammaContrastBrightnessChanged;

    public static Device DX9Device = null; // pointer to current DX9 device
    private static string m_strSkin = ""; // name of the current skin
    private static bool m_bFullScreenVideo = false; // boolean indicating if we're in GUI or fullscreen video/tv mode
    private static IntPtr m_ipActiveForm; // pointer to the current GDI window
    private static Rectangle m_RectVideo; // rectangle of the video preview window
    private static Geometry.Type m_ARType = Geometry.Type.Normal; // current video transformation type (see geometry.cs)
    private static int m_iOSDOffset = 0; // y-offset of the video/tv OSD
    private static int m_iOverScanLeft = 0; // x offset screen calibration
    private static int m_iOverScanTop = 0; // y offset screen calibration
    private static int m_iOverScanWidth = 0; // width screen calibratoin
    private static int m_iOverScanHeight = 0; // height screen calibratoin
    private static float m_fPixelRatio = 1.0f; // current pixel ratio correction
    private static State m_eState; // state of application
    private static bool m_bOverlay = true; // boolean indicating if the overlay window is allowed to be shown
    private static int m_iOffsetX = 0; // x offset of GUI calibration
    private static int m_iOffsetY = 0; // y offset of GUI calibration
    private static float m_fZoomHorizontal = 1.0f; // x zoom of GUI calibration
    private static float m_fZoomVertical = 1.0f; // y zoom of GUI calibration
    private static bool m_bTopBarHidden = false; // Topbar hidden status for autohide
    private static bool m_bAutoHideTopBar = false; // Topbar autohide status
    private static bool m_bDefaultTopBarHide = false; // Topbar.xml default autohide status
    private static DateTime m_dtTopBarTimeOut = DateTime.Now; // Topbar timeout timer
    private static bool _disableTopBar = false; // Topbar diaanled view status   
    private static int m_iSubtitles = 550; // Y position for subtitles
    private static bool m_bCalibrating = false; // boolean indicating if we are in calibration mode or in normal mode
    private static bool m_bPlaying; // boolean indicating if we are playing any media or not
    public static Graphics graphics = null; // GDI+ Graphics object
    public static Form form = null; // Current GDI form
    private static int m_iBrightness = -1; // brightness value
    private static int m_iGamma = -1; // gamma value
    private static int m_iContrast = -1; // contrast value
    private static int m_iSaturation = -1; // saturation value
    private static int m_Sharpness = -1; // sharpness value

    private static bool m_bMouseSupport = true;
    // boolean indicating if we should present mouse controls like scrollbars

    private static bool m_bDBLClickAsRightclick = false;
    // boolean indicating that we want to use double click to open a context menu

    private static Size m_skinSize = new Size(720, 576); // original width/height for which the skin was designed

    private static bool m_bShowBackGround = true;
    //boolean indicating if we should show the GUI background or if we should show live tv in the background

    private static bool m_bPlayingVideo = false; //boolean indicating if we are playing a movie
    private static int m_iScrollSpeedVertical = 4; //scroll speed for controls which scroll
    private static int m_iScrollSpeedHorizontal = 3; //scroll speed for controls which scroll
    private static int m_iCharsInCharacterSet = 255; //number of characters for current fonts
    private static bool m_bEditMode = false; //boolean indicating if we are in skin edit mode
    private static bool m_bAnimations = true; //boolean indicating animiations are turned on or off
    private static IRender m_renderFrame = null;
    private static bool vmr9Active = false;
    private static bool m_bisvmr9Exclusive = false;
    private static bool m_bisevr = false;
    private static int m_iMaxFPS = 50;
    private static long m_iDesiredFrameTime = 100;
    private static float m_fCurrentFPS = 0;
    private static float m_fVMR9FPS = 0;
    private static float lasttime = 0f;
    private static bool vmr9RenderBusy = false;
    private static bool blankScreen = false;
    private static bool idleTimePowerSaving = false;
    private static bool turnOffMonitor = false;
    private static PresentParameters presentParameters;
    private static bool vmr9Allowed = true;
    private static Size videoSize;
    private static bool hasFocus = false;
    private static DateTime _lastActivity = DateTime.Now;
    public static IAutoCrop autoCropper = null;
    private const uint SC_MONITORPOWER = 0xF170;
    private const uint WM_SYSCOMMAND = 0x0112;
    private const int MONITOR_ON = -1;
    private const int MONITOR_OFF = 2;
    private static bool _useSeparateRenderThread = false;
    public static bool _useScreenSelector = false;
    private static AdapterInformation _currentFullscreenAdapterInfo = null;
    private static int _currentScreenNumber = -1;
    private static Screen _currentScreen = null;
    private static bool _isDX9EXused = Environment.OSVersion.Version.Major >= 6;

    private static Point _screenCenterPos = new Point();
    private static bool m_bAllowRememberLastFocusedItem = true;

    [DllImport("user32.dll")]
    private static extern bool SendMessage(IntPtr hWnd, uint Msg, uint wParam, IntPtr lParam);

    // singleton. Dont allow any instance of this class
    private GUIGraphicsContext()
    {
    }

    static GUIGraphicsContext()
    {
    }

    /// <summary>
    /// Set/get last User Activity
    /// </summary>
    public static DateTime LastActivity
    {
      get { return _lastActivity; }
    }

    public static bool SaveRenderCycles
    {
      get { return idleTimePowerSaving; }
      set 
      {
        // Since every action (like keypresses) resets the Blankscreen field we certainly want to check
        // whether we really need to reload the "old" FPS.
        if (idleTimePowerSaving != value)
        {
          idleTimePowerSaving = value;

          if (idleTimePowerSaving)
          {
            MaxFPS = 5;
          }
          else
          {
            using (Settings xmlReader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {       
              MaxFPS = xmlReader.GetValueAsInt("screen", "GuiRenderFps", 50);
            }
          }
        }
      }
    }

    public static bool UseSeparateRenderThread
    {
      get { return _useSeparateRenderThread; }
      set
      {
        _useSeparateRenderThread = value;
        if (_useSeparateRenderThread)
        {
          Log.Warn("GraphicContext: Using separate thread for GUI rendering");
        }
        //else
        //  Log.Info("GraphicContext: not using separate thread for GUI rendering");
      }
    }

    /// <summary>
    /// Enable/disable screen output
    /// </summary>
    public static bool BlankScreen
    {
      get { return blankScreen; }
      set
      {
        if (value == false)
        {
          SaveRenderCycles = false;
        }
        if (value != blankScreen)
        {
          try
          {
            if (turnOffMonitor)
            {
              if (Form.ActiveForm.Handle != IntPtr.Zero)
              {
                SendMessage(Form.ActiveForm.Handle, WM_SYSCOMMAND, SC_MONITORPOWER,
                            value ? (IntPtr)MONITOR_OFF : (IntPtr)MONITOR_ON);
              }
              else
              {
                Log.Warn("GraphicContext: Could not power monitor {0}", value ? "off" : "on");
              }
            }
            blankScreen = value;

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

    /// <summary>
    /// Property to enable/disable animations
    /// </summary>
    public static bool Animations
    {
      get { return m_bAnimations; }
      set { m_bAnimations = value; }
    }

    /// <summary>
    /// property to enable/disable skin-editting mode
    /// </summary>
    public static bool EditMode
    {
      get { return m_bEditMode; }
      set { m_bEditMode = value; }
    }

    /// <summary>
    /// Property to get and set current adapter for creating directx surface
    /// </summary>
    public static AdapterInformation currentFullscreenAdapterInfo
    {
      get
      {
        if (_currentFullscreenAdapterInfo != null)
        {
          return _currentFullscreenAdapterInfo;
        }
        else
        {
          return Manager.Adapters.Default;
        }
      }
      set { _currentFullscreenAdapterInfo = value; }
    }

    public static int currentScreenNumber
    {
      get
      {
        if (_currentScreen != null)
        {
          return _currentScreenNumber;
        }
        else
        {
          return 0;
        }
      }
      set { _currentScreenNumber = value; }
    }

    /// <summary>
    /// Property to get and set current screen on witch MP is displayed
    /// </summary>
    public static Screen currentScreen
    {
      get
      {
        if (_currentScreen != null)
        {
          return _currentScreen;
        }
        else
        {
          return Screen.PrimaryScreen;
        }
      }
      set { _currentScreen = value; }
    }

    /// <summary>
    /// Property to get windowed/fullscreen state of application
    /// </summary>
    //SV
    //static bool Fullscreen
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
      if (Fullscreen)
      {
        strFileName += ".fs.xml";
      }
      else
      {
        strFileName += ".xml";
      }

      // Log.Info("save {0}" ,strFileName);
      using (Settings xmlWriter = new Settings(strFileName))
      {
        xmlWriter.SetValue("screen", "offsetx", m_iOffsetX.ToString());
        xmlWriter.SetValue("screen", "offsety", m_iOffsetY.ToString());

        float zoomHorizontal = m_fZoomHorizontal;
        zoomHorizontal *= 10000f;
        int intZoomHorizontal = (int)zoomHorizontal;
        xmlWriter.SetValue("screen", "zoomhorizontal", intZoomHorizontal.ToString());

        float zoomVertical = m_fZoomVertical;
        zoomVertical *= 10000f;
        int intZoomVertical = (int)zoomVertical;
        xmlWriter.SetValue("screen", "zoomvertical", intZoomVertical.ToString());

        xmlWriter.SetValue("screen", "offsetosd", m_iOSDOffset.ToString());
        xmlWriter.SetValue("screen", "overscanleft", m_iOverScanLeft.ToString());
        xmlWriter.SetValue("screen", "overscantop", m_iOverScanTop.ToString());
        xmlWriter.SetValue("screen", "overscanwidth", m_iOverScanWidth.ToString());
        xmlWriter.SetValue("screen", "overscanheight", m_iOverScanHeight.ToString());

        float pixelRatio = m_fPixelRatio;
        pixelRatio *= 10000f;
        int intPixelRatio = (int)pixelRatio;
        xmlWriter.SetValue("screen", "pixelratio", intPixelRatio.ToString());
        xmlWriter.SetValue("screen", "subtitles", m_iSubtitles.ToString());

        Log.Debug("GraphicContext: Settings saved to {0}", strFileName);
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

      string strFileName = Config.GetFile(Config.Dir.Config, String.Format("ScreenCalibration{0}x{1}", Width, Height));
      if (Fullscreen)
      {
        strFileName += ".fs.xml";
      }
      else
      {
        strFileName += ".xml";
      }

      if (!File.Exists(strFileName))
      {
        Log.Warn("GraphicContext: NO screen calibration file found for resolution {0}x{1}!", Width, Height);
      }
      else
      {
        Log.Debug("GraphicContext: Loading settings from {0}", strFileName);
      }

      using (Settings xmlReader = new Settings(strFileName))
      {
        try
        {
          m_iOffsetX = xmlReader.GetValueAsInt("screen", "offsetx", 0);
          m_iOffsetY = xmlReader.GetValueAsInt("screen", "offsety", 0);

          m_iOSDOffset = xmlReader.GetValueAsInt("screen", "offsetosd", 0);
          m_iOverScanLeft = xmlReader.GetValueAsInt("screen", "overscanleft", 0);
          m_iOverScanTop = xmlReader.GetValueAsInt("screen", "overscantop", 0);
          m_iOverScanWidth = xmlReader.GetValueAsInt("screen", "overscanwidth", Width);
          m_iOverScanHeight = xmlReader.GetValueAsInt("screen", "overscanheight", Height);
          m_iSubtitles = xmlReader.GetValueAsInt("screen", "subtitles", Height - 50);
          int intPixelRation = xmlReader.GetValueAsInt("screen", "pixelratio", 10000);
          m_fPixelRatio = (float)intPixelRation;
          m_fPixelRatio /= 10000f;

          int intZoomHorizontal = xmlReader.GetValueAsInt("screen", "zoomhorizontal", 10000);
          m_fZoomHorizontal = (float)intZoomHorizontal;
          m_fZoomHorizontal /= 10000f;

          int intZoomVertical = xmlReader.GetValueAsInt("screen", "zoomvertical", 10000);
          m_fZoomVertical = (float)intZoomVertical;
          m_fZoomVertical /= 10000f;
        }
        catch (Exception ex)
        {
          Log.Error("GraphicContext: Error loading settings from {0} - {1}", strFileName, ex.Message);
        }
      }

      using (Settings xmlReader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        m_iMaxFPS = xmlReader.GetValueAsInt("screen", "GuiRenderFps", 50);
        SyncFrameTime();
        m_iScrollSpeedVertical = xmlReader.GetValueAsInt("general", "ScrollSpeedDown", 4);
        m_iScrollSpeedHorizontal = xmlReader.GetValueAsInt("general", "ScrollSpeedRight", 3);
        m_bAnimations = xmlReader.GetValueAsBool("general", "animations", true);
        turnOffMonitor = xmlReader.GetValueAsBool("general", "turnoffmonitor", false);

        Log.Info("GraphicContext: MP will render at {0} FPS, use animations = {1}", m_iMaxFPS,
                 Convert.ToString(m_bAnimations));
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
    /// <param name="msg">The message.</param>
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
        if (DX9Device != null)
        {
          return DX9Device.PresentationParameters.BackBufferHeight;
        }
        return form.ClientSize.Height;
      }
    }

    /// <summary>
    /// Return screen/window Width.
    /// </summary>
    public static int Width
    {
      get
      {
        if (DX9Device != null)
        {
          return DX9Device.PresentationParameters.BackBufferWidth;
        }
        return form.ClientSize.Width;
      }
    }

    /// <summary>
    /// Gets the center of MP's current Window Area
    /// </summary>
    public static Point OutputScreenCenter
    {
      get
      {
        int borderWidth = (form.Size.Width - form.ClientRectangle.Width) / 2;
        // we can only assume that the title bar occupies this space
        int borderHeight = (form.Size.Height - form.ClientRectangle.Height) + (2 * borderWidth);
        _screenCenterPos = new Point((form.ClientRectangle.Width / 2) + borderWidth + form.Location.X,
                                     (form.ClientRectangle.Height / 2) + borderHeight + form.Location.Y);

        return _screenCenterPos;
      }
    }

    public static void ResetCursor(bool interpolate)
    {
      int newX, newY;
      if (interpolate)
      {
        Point oldPos = Cursor.Position;
        if (form.ClientRectangle.Width - oldPos.X < OutputScreenCenter.X)
        {
          newX = OutputScreenCenter.X + (int)((oldPos.X - OutputScreenCenter.X) / 2);
        }
        else
        {
          newX = OutputScreenCenter.X - (int)((OutputScreenCenter.X - oldPos.X) / 2);
        }

        if (form.ClientRectangle.Height - oldPos.Y < OutputScreenCenter.Y)
        {
          newY = OutputScreenCenter.Y + (int)((oldPos.Y - OutputScreenCenter.Y) / 2);
        }
        else
        {
          newY = OutputScreenCenter.Y - (int)((OutputScreenCenter.Y - oldPos.Y) / 2);
        }
      }
      else
      {
        newX = OutputScreenCenter.X;
        newY = OutputScreenCenter.Y;
      }
      Cursor.Position = new Point(newX, newY);

      //if (DX9Device != null)
      //  DX9Device.SetCursorPosition(posx, posy, true);
      Cursor.Hide();
    }

    /// <summary>
    /// Apply screen offset correct 
    /// </summary>
    /// <param name="fx">X correction.</param>
    /// <param name="fy">Y correction.</param>
    public static void Correct(ref float fx, ref float fy)
    {
      fx += (float)OffsetX;
      fy += (float)OffsetY;
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
      // Adjust for global zoom.
      float fZoomedScreenWidth = (float)Width * ZoomHorizontal;
      float fZoomedScreenHeight = (float)Height * ZoomVertical;

      // Get skin size
      float fSkinWidth = (float)m_skinSize.Width;
      float fSkinHeight = (float)m_skinSize.Height;

      // X
      float fPercentX = fZoomedScreenWidth / fSkinWidth;
      left = (int)Math.Round(((float)left) * fPercentX);
      right = (int)Math.Round(((float)right) * fPercentX);
      // Y
      float fPercentY = fZoomedScreenHeight / fSkinHeight;
      top = (int)Math.Round(((float)top) * fPercentY);
      bottom = (int)Math.Round(((float)bottom) * fPercentY);
    }

    /// <summary>
    /// Scale position for current resolution
    /// </summary>
    /// <param name="x">X coordinate to scale.</param>
    /// <param name="y">Y coordinate to scale.</param>
    public static void ScalePosToScreenResolution(ref int x, ref int y)
    {
      // Adjust for global zoom.
      float fZoomedScreenWidth = (float)Width * ZoomHorizontal;
      float fZoomedScreenHeight = (float)Height * ZoomVertical;

      float fSkinWidth = (float)m_skinSize.Width;
      float fSkinHeight = (float)m_skinSize.Height;

      // X,Y
      float fPercentX = fZoomedScreenWidth / fSkinWidth;
      float fPercentY = fZoomedScreenHeight / fSkinHeight;
      x = (int)Math.Round(((float)x) * fPercentX);
      y = (int)Math.Round(((float)y) * fPercentY);
    }

    /// <summary>
    /// Scale y position for current resolution
    /// </summary>
    /// <param name="y">Y coordinate to scale.</param>
    public static void ScaleVertical(ref int y)
    {
      // Adjust for global zoom.
      float fZoomedScreenHeight = (float)Height * ZoomVertical;

      float fSkinHeight = (float)m_skinSize.Height;

      float fPercentY = fZoomedScreenHeight / fSkinHeight;
      y = (int)Math.Round(((float)y) * fPercentY);
    }

    public static void ScaleVertical(ref float y)
    {
      // Adjust for global zoom.
      float fZoomedScreenHeight = (float)Height * ZoomVertical;

      float fSkinHeight = (float)m_skinSize.Height;

      float fPercentY = fZoomedScreenHeight / fSkinHeight;
      y = (float)Math.Round(((float)y) * fPercentY);
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
    /// <param name="y">X coordinate to scale.</param>
    public static void ScaleHorizontal(ref int x)
    {
      // Adjust for global zoom.
      float fZoomedScreenWidth = (float)Width * ZoomHorizontal;

      float fSkinWidth = (float)m_skinSize.Width;

      // X
      float fPercentX = (fZoomedScreenWidth) / fSkinWidth;
      x = (int)Math.Round(((float)x) * fPercentX);
    }

    public static void ScaleHorizontal(ref float x)
    {
      // Adjust for global zoom.
      float fZoomedScreenWidth = (float)Width * ZoomHorizontal;

      float fSkinWidth = (float)m_skinSize.Width;

      // X
      float fPercentX = (fZoomedScreenWidth) / fSkinWidth;
      x = (float)Math.Round(((float)x) * fPercentX);
    }


    /// <summary>
    /// Scale X position for current resolution
    /// </summary>
    /// <param name="y">X coordinate to scale.</param>
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
      // Adjust for global zoom.
      float fZoomedScreenWidth = (float)Width * ZoomHorizontal;
      float fZoomedScreenHeight = (float)Height * ZoomVertical;

      float fSkinWidth = (float)m_skinSize.Width;
      float fSkinHeight = (float)m_skinSize.Height;

      float fPercentX = fSkinWidth / fZoomedScreenWidth;
      float fPercentY = fSkinHeight / fZoomedScreenHeight;
      x = (int)Math.Round(((float)x) * fPercentX);
      y = (int)Math.Round(((float)y) * fPercentY);
    }
    
    public static void BlackImageRendered()
    {
      if (OnBlackImageRendered != null)
      {
        OnBlackImageRendered();
      }
    }

    public static void VideoReceived()
    {
      if (OnVideoReceived != null)
      {
        OnVideoReceived();
      }
    }

    public static bool RenderBlackImage
    {
      get { return _renderBlackImage; }
      set { _renderBlackImage = value; }
    }
        
    /// <summary>
    /// Get/set current Aspect Ratio Mode
    /// </summary>
    public static Geometry.Type ARType
    {
      get { return m_ARType; }
      set
      {
        m_ARType = value;
        if (OnVideoWindowChanged != null)
        {
          OnVideoWindowChanged();
        }
      }
    }

    /// <summary>
    /// Get/set current skin name
    /// </summary>
    public static string Skin
    {
      set { m_strSkin = Config.GetSubFolder(Config.Dir.Skin, value); }
      get { return m_strSkin; }
    }

    /// <summary>
    /// Gets the current skin cache folder
    /// </summary>
    public static string SkinCacheFolder
    {
      get
      {
        string skinName = m_strSkin.Substring(m_strSkin.LastIndexOf(@"\"));
        return string.Format("{0}{1}", Config.GetFolder(Config.Dir.Cache), skinName);
      }
    }

    /// <summary>
    /// Get/Set vertical offset for the OSD
    /// </summary>
    public static int OSDOffset
    {
      get { return m_iOSDOffset; }
      set { m_iOSDOffset = value; }
    }

    /// <summary>
    /// Get/Set left calibration value
    /// </summary>
    public static int OverScanLeft
    {
      get { return m_iOverScanLeft; }
      set { m_iOverScanLeft = value; }
    }

    /// <summary>
    /// Get/Set upper calibration value
    /// </summary>
    public static int OverScanTop
    {
      get { return m_iOverScanTop; }
      set { m_iOverScanTop = value; }
    }

    /// <summary>
    /// Get/Set calibration width
    /// </summary>
    public static int OverScanWidth
    {
      get { return m_iOverScanWidth; }
      set { m_iOverScanWidth = value; }
    }

    /// <summary>
    /// Get/Set calibration height
    /// </summary>
    public static int OverScanHeight
    {
      get { return m_iOverScanHeight; }
      set { m_iOverScanHeight = value; }
    }

    /// <summary>
    /// Get/Set current pixel Ratio
    /// </summary>
    public static float PixelRatio
    {
      get { return m_fPixelRatio; }
      set { m_fPixelRatio = value; }
    }

    /// <summary>
    /// get /set whether we're playing a movie , visz or TV in
    /// fullscreen mode or in windowed (preview) mode
    /// </summary>
    public static bool IsFullScreenVideo
    {
      get { return m_bFullScreenVideo; }
      set
      {
        if (value != m_bFullScreenVideo)
        {
          m_bFullScreenVideo = value;
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
      get { return m_RectVideo; }
      set
      {
        if (!m_RectVideo.Equals(value))
        {
          if (m_RectVideo.Width == 0)
          {
            m_RectVideo.Width = 1;
          }
          if (m_RectVideo.Height == 0)
          {
            m_RectVideo.Height = 1;
          }
          m_RectVideo = value;
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
    public static State CurrentState
    {
      get { return m_eState; }
      set { m_eState = value; }
    }

    /// <summary>
    /// Get pointer to the applications form (needed by overlay windows)
    /// </summary>
    public static IntPtr ActiveForm
    {
      get { return m_ipActiveForm; }
      set { m_ipActiveForm = value; }
    }

    /// <summary>
    /// return whether we're currently calibrating or not
    /// </summary>
    public static bool Calibrating
    {
      get { return m_bCalibrating; }
      set { m_bCalibrating = value; }
    }

    /// <summary>
    /// Get/Set wheter overlay window is enabled or disabled
    /// </summary>
    public static bool Overlay
    {
      get { return m_bOverlay; }
      set
      {
        m_bOverlay = value;
        if (!m_bOverlay)
        {
          m_RectVideo.Width = 1;
          m_RectVideo.Height = 1;
        }
        if (!ShowBackground)
        {
          m_bOverlay = false;
        }
        if (OnVideoWindowChanged != null)
        {
          OnVideoWindowChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set left screen calibration
    /// </summary>
    public static int OffsetX
    {
      get { return m_iOffsetX; }
      set { m_iOffsetX = value; }
    }

    /// <summary>
    /// Get/Set upper screen calibration
    /// </summary>
    public static int OffsetY
    {
      get { return m_iOffsetY; }
      set { m_iOffsetY = value; }
    }

    /// <summary>
    /// Get/Set vertical zoom screen calibration
    /// </summary>
    public static float ZoomVertical
    {
      get { return m_fZoomVertical; }
      set { m_fZoomVertical = value; }
    }

    /// <summary>
    /// Get/Set vertical zoom screen calibration
    /// </summary>
    public static float ZoomHorizontal
    {
      get { return m_fZoomHorizontal; }
      set { m_fZoomHorizontal = value; }
    }

    /// <summary>
    /// Get/Set topbar hidden status
    /// </summary>
    public static bool TopBarHidden
    {
      get { return m_bTopBarHidden; }
      set { m_bTopBarHidden = value; }
    }

    /// <summary>
    /// Get/Set topbar autohide status
    /// </summary>
    public static bool AutoHideTopBar
    {
      get { return m_bAutoHideTopBar; }
      set { m_bAutoHideTopBar = value; }
    }

    /// <summary>
    /// Get/Set default topbar autohide status
    /// </summary>
    public static bool DefaultTopBarHide
    {
      get { return m_bDefaultTopBarHide; }
      set { m_bDefaultTopBarHide = value; }
    }

    /// <summary>
    /// Get/Set topbar timeout
    /// </summary>
    public static DateTime TopBarTimeOut
    {
      get { return m_dtTopBarTimeOut; }
      set { m_dtTopBarTimeOut = value; }
    }

    /// <summary>
    /// Get/Set disable topbar view status
    /// </summary>
    public static bool DisableTopBar
    {
      get { return _disableTopBar; }
      set { _disableTopBar = value; }
    }

    /// <summary>
    /// Get/Set Y-position for subtitles
    /// </summary>
    public static int Subtitles
    {
      get { return m_iSubtitles; }
      set { m_iSubtitles = value; }
    }

    /// <summary>
    /// Calculates a rectangle based on current calibration values/pixel ratio
    /// </summary>
    /// <param name="iSourceWidth">width of source rectangle</param>
    /// <param name="iSourceHeight">height of source rectangle</param>
    /// <param name="iMaxWidth">max. width allowed</param>
    /// <param name="iMaxHeight">max. height allowed</param>
    /// <param name="width">returned width of calculated rectangle</param>
    /// <param name="height">returned height of calculated rectangle</param>
    public static void GetOutputRect(int iSourceWidth, int iSourceHeight, int iMaxWidth, int iMaxHeight, out int width,
                                     out int height)
    {
      // calculate aspect ratio correction factor
      float fPixelRatio = PixelRatio;

      float fSourceFrameAR = (float)iSourceWidth / iSourceHeight;
      float fOutputFrameAR = fSourceFrameAR / fPixelRatio;
      width = iMaxWidth;
      height = (int)(width / fOutputFrameAR);
      if (height > iMaxHeight)
      {
        height = iMaxHeight;
        width = (int)(height * fOutputFrameAR);
      }
    }

    /// <summary>
    /// Get/Set whether a file (music or video) is currently playing
    /// </summary>
    public static bool IsPlaying
    {
      get { return m_bPlaying; }
      set
      {
        m_bPlaying = value;
        if (!m_bPlaying)
        {
          IsPlayingVideo = false;
        }
      }
    }


    /// <summary>
    /// Get/Set whether a a movie (or livetv) is currently playing
    /// </summary>
    public static bool IsPlayingVideo
    {
      get { return m_bPlayingVideo; }
      set { m_bPlayingVideo = value; }
    }

    /// <summary>
    /// Get/Set the Brightness.
    /// </summary>
    public static int Brightness
    {
      get { return m_iBrightness; }
      set
      {
        if (m_iBrightness != value)
        {
          m_iBrightness = value;
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
      get { return m_iContrast; }
      set
      {
        if (m_iContrast != value)
        {
          m_iContrast = value;
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
      get { return m_iGamma; }
      set
      {
        if (m_iGamma != value)
        {
          m_iGamma = value;
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
      get { return m_iSaturation; }
      set
      {
        if (m_iSaturation != value)
        {
          m_iSaturation = value;
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
      get { return m_Sharpness; }
      set
      {
        if (m_Sharpness != value)
        {
          m_Sharpness = value;
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
      get { return m_bMouseSupport; }
      set { m_bMouseSupport = value; }
    }

    /// <summary>
    /// Get/Set  if it is allowed to remember last focused item on supported window/skin 
    /// </summary>
    public static bool AllowRememberLastFocusedItem
    {
      get { return m_bAllowRememberLastFocusedItem; }
      set { m_bAllowRememberLastFocusedItem = value; }
    }

    /// <summary>
    /// Get/Set  if we want to use double click to be used as right click
    /// </summary>
    public static bool DBLClickAsRightClick
    {
      get { return m_bDBLClickAsRightclick; }
      set { m_bDBLClickAsRightclick = value; }
    }

    /// <summary>
    /// Get/Set the size of the skin.
    /// </summary>	
    public static Size SkinSize
    {
      get { return m_skinSize; }
      set { m_skinSize = value; }
    }

    /// <summary>
    /// Get/Set whether we should show the GUI as background or 
    /// live tv as background
    /// </summary>
    public static bool ShowBackground
    {
      get { return m_bShowBackGround; }
      set
      {
        m_bShowBackGround = value;
        if (!m_bShowBackGround)
        {
          Overlay = false;
        }
      }
    }

    /// <summary>
    /// Get/Set the current scroll speed 
    /// </summary>
    public static int ScrollSpeedVertical
    {
      get { return m_iScrollSpeedVertical; }
      set
      {
        if (m_iScrollSpeedVertical < 0)
        {
          return;
        }
        m_iScrollSpeedVertical = value;
      }
    }

    /// <summary>
    /// Get/Set the current scroll speed 
    /// </summary>
    public static int ScrollSpeedHorizontal
    {
      get { return m_iScrollSpeedHorizontal; }
      set
      {
        if (m_iScrollSpeedHorizontal < 0)
        {
          return;
        }
        m_iScrollSpeedHorizontal = value;
      }
    }

    /// <summary>
    /// Get/Set the current maximum number of FPS
    /// </summary>
    public static int MaxFPS
    {
      get { return m_iMaxFPS; }
      set
      {
        if (m_iMaxFPS < 0)
        {
          return;
        }
        m_iMaxFPS = value;
        SyncFrameTime();
      }
    }

    /// <summary>
    /// Get the number of ticks for each frame to get MaxFPS
    /// </summary>
    public static long DesiredFrameTime
    {
      get { return m_iDesiredFrameTime; }
    }

    /// <summary>
    /// Get/Set the current maximum number of FPS
    /// </summary>
    public static float CurrentFPS
    {
      get { return m_fCurrentFPS; }
      set
      {
        if (m_fCurrentFPS < 0)
        {
          return;
        }
        m_fCurrentFPS = value;
      }
    }

    /// <summary>
    /// Get/Set the number of characters used for the fonts
    /// </summary>
    public static int CharsInCharacterSet
    {
      get { return m_iCharsInCharacterSet; }
      set
      {
        if (m_iCharsInCharacterSet < 128)
        {
          return;
        }
        m_iCharsInCharacterSet = value;
        Log.Info("GraphicContext: Using {0} chars of the character set. ", m_iCharsInCharacterSet);
      }
    }

    public static IRender RenderGUI
    {
      get { return m_renderFrame; }
      set { m_renderFrame = value; }
    }

    public static float Vmr9FPS
    {
      get { return m_fVMR9FPS; }
      set { m_fVMR9FPS = value; }
    }

    public static bool Vmr9Active
    {
      get { return vmr9Active; }
      set
      {
        if (value != vmr9Active)
        {
          vmr9Active = value;
          if (vmr9Active)
          {
            Log.Debug("VMR9: Now active");
          }
          else
          {
            Log.Debug("VMR9: Inactive");
          }
        }
      }
    }

    public static bool IsVMR9Exclusive
    {
      get { return m_bisvmr9Exclusive; }
      set { m_bisvmr9Exclusive = value; }
    }

    public static bool IsEvr
    {
      get { return m_bisevr; }
      set { m_bisevr = value; }
    }

    public static float TimePassed
    {
      get
      {
        float time = DXUtil.Timer(DirectXTimer.GetAbsoluteTime);
        float difftime = time - lasttime;
        lasttime = time;
        return (difftime);
      }
    }

    public static bool InVmr9Render
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get { return vmr9RenderBusy; }
      set { vmr9RenderBusy = value; }
    }

    private static void SyncFrameTime()
    {
      m_iDesiredFrameTime = DXUtil.TicksPerSecond / m_iMaxFPS;
    }

    public static PresentParameters DirectXPresentParameters
    {
      get { return presentParameters; }
      set { presentParameters = value; }
    }

    public static bool VMR9Allowed
    {
      get { return vmr9Allowed; }
      set { vmr9Allowed = value; }
    }

    public static Size VideoSize
    {
      get { return videoSize; }
      set { videoSize = value; }
    }

    public static bool HasFocus
    {
      get { return hasFocus; }
      set { hasFocus = value; }
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
      if (windowId == (int)GUIWindow.Window.WINDOW_TV)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TVGUIDE)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTV)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_SCHEDULER)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_SEARCHTV)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TELETEXT)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_CONFLICTS)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_MAIN)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_AUTO)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS_STATUS)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_SETTINGS)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_NO_SIGNAL)
      {
        return true;
      }
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO)
      {
        return true;
      }

      return false;
    }

    public static bool IsDirectX9ExUsed()
    {
      return _isDX9EXused;
    }

    public static Pool GetTexturePoolType()
    {
      // DirectX9 Ex device works only with Pool.Default
      if (IsDirectX9ExUsed())
      {
        return Pool.Default;
      }
      return Pool.Managed;
    }

    public static TransformMatrix ControlTransform
    {
      get { return _finalTransform; }
      set { _finalTransform = value; }
    }

    public static void SetScalingResolution( /*RESOLUTION res,*/ int posX, int posY, bool needsScaling)
    {
      //m_windowResolution = res;
      if (needsScaling)
      {
        /*
                // calculate necessary scalings
                float fFromWidth = (float)g_settings.m_ResInfo[res].iWidth;
                float fFromHeight = (float)g_settings.m_ResInfo[res].iHeight;
                float fToPosX = (float)g_settings.m_ResInfo[m_Resolution].Overscan.left;
                float fToPosY = (float)g_settings.m_ResInfo[m_Resolution].Overscan.top;
                float fToWidth = (float)g_settings.m_ResInfo[m_Resolution].Overscan.right - fToPosX;
                float fToHeight = (float)g_settings.m_ResInfo[m_Resolution].Overscan.bottom - fToPosY;
                // add additional zoom to compensate for any overskan built in skin
                float fZoom = g_SkinInfo.GetSkinZoom();
                if (!g_guiSkinzoom) // lookup gui setting if we didn't have it already
                  g_guiSkinzoom = (CSettingInt*)g_guiSettings.GetSetting("lookandfeel.skinzoom");
                if (g_guiSkinzoom)
                  fZoom *= (100 + g_guiSkinzoom->GetData()) * 0.01f;
                fZoom -= 1.0f;
                fToPosX -= fToWidth * fZoom * 0.5f;
                fToWidth *= fZoom + 1.0f;
                // adjust for aspect ratio as zoom is given in the vertical direction and we don't 
                // do aspect ratio corrections in the gui code 
                fZoom = fZoom / g_settings.m_ResInfo[m_Resolution].fPixelRatio;
                fToPosY -= fToHeight * fZoom * 0.5f;
                fToHeight *= fZoom + 1.0f;
                _windowScaleX = fToWidth / fFromWidth;
                _windowScaleY = fToHeight / fFromHeight;
                TransformMatrix windowOffset = TransformMatrix.CreateTranslation((float)posX, (float)posY);
                TransformMatrix guiScaler = TransformMatrix.CreateScaler(fToWidth / fFromWidth, fToHeight / fFromHeight);
                TransformMatrix guiOffset = TransformMatrix.CreateTranslation(fToPosX, fToPosY);
                _guiTransform = guiOffset * guiScaler * windowOffset;
        */
      }
      else
      {
        _guiTransform = TransformMatrix.CreateTranslation((float)posX, (float)posY, 0);
        //_windowScaleX = 1.0f;
        //_windowScaleY = 1.0f;
      }

      _cameras.Clear();
      _cameras.Add(new Point(Width / 2, Height / 2));
      UpdateCameraPosition(_cameras[0]);
      // reset the final transform and window transforms
      UpdateFinalTransform(_guiTransform);
    }

    public static void UpdateFinalTransform(TransformMatrix matrix)
    {
      _finalTransform = matrix;
    }

    public static float[,] GetFinalMatrix()
    {
      return _finalTransform.Matrix;
    }

    public static float ScaleFinalXCoord(float x, float y)
    {
      return _finalTransform.TransformXCoord(x, y, 0);
    }

    public static float ScaleFinalYCoord(float x, float y)
    {
      return _finalTransform.TransformYCoord(x, y, 0);
    }

    public static float ScaleFinalZCoord(float x, float y)
    {
      return _finalTransform.TransformZCoord(x, y, 0);
    }

    public static void ScaleFinalCoords(ref float x, ref float y, ref float z)
    {
      _finalTransform.TransformPosition(ref x, ref y, ref z);
    }

    public static uint MergeAlpha(uint color)
    {
      uint alpha = _finalTransform.TransformAlpha((color >> 24) & 0xff);
      return ((alpha << 24) & 0xff000000) | (color & 0xffffff);
    }

    public static void SetWindowTransform(TransformMatrix matrix)
    {
      // reset the group transform stack

      _groupTransforms.Clear();
      _groupTransforms.Add(_guiTransform.multiply(matrix));
      UpdateFinalTransform(_groupTransforms[0]);
    }

    public static void AddTransform(TransformMatrix matrix)
    {
      if (_groupTransforms.Count > 0)
      {
        _groupTransforms.Add(_groupTransforms[_groupTransforms.Count - 1].multiply(matrix));
      }
      else
      {
        _groupTransforms.Add(matrix);
      }
      UpdateFinalTransform(_groupTransforms[_groupTransforms.Count - 1]);
    }

    public static void RemoveTransform()
    {
      if (_groupTransforms.Count > 0)
      {
        _groupTransforms.RemoveAt(_groupTransforms.Count - 1);
      }
      if (_groupTransforms.Count > 0)
      {
        UpdateFinalTransform(_groupTransforms[_groupTransforms.Count - 1]);
      }
      else
      {
        UpdateFinalTransform(new TransformMatrix());
      }
    }

    public static void SetCameraPosition(Point camera)
    {
      // offset the camera from our current location (this is in XML coordinates) and scale it up to
      // the screen resolution
      Point cam = new Point(camera.X, camera.Y);


      _cameras.Add(cam);
      UpdateCameraPosition(_cameras[0]);
    }

    public static void RestoreCameraPosition()
    {
      _cameras.RemoveAt(0);
      UpdateCameraPosition(_cameras[0]);
    }

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
      Point offset = new Point(camera.X - (Width / 2), camera.Y - (Height / 2));

      // grab the viewport dimensions and location
      Viewport viewport = DX9Device.Viewport;
      float w = viewport.Width * 0.5f;
      float h = viewport.Height * 0.5f;

      // world view.  Until this is moved onto the GPU (via a vertex shader for instance), we set it to the identity
      // here.
      Matrix mtxWorld;
      mtxWorld = Matrix.Identity;
      DX9Device.Transform.World = mtxWorld;
      // camera view.  Multiply the Y coord by -1 then translate so that everything is relative to the camera
      // position.
      Matrix flipY, translate, mtxView;
      flipY = Matrix.Scaling(1.0f, -1.0f, 1.0f);
      translate = Matrix.Translation(-(viewport.X + w + offset.X), -(viewport.Y + h + offset.Y), 2 * h);
      mtxView = Matrix.Multiply(translate, flipY);
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
  }
}