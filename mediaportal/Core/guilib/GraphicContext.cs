/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using AMS.Profile;
using System.Runtime.InteropServices;
using MediaPortal.Utils.Services;

namespace MediaPortal.GUI.Library
{
  public delegate void OnActionHandler(Action action);
  public delegate void SendMessageHandler(GUIMessage message);
  public delegate void VideoWindowChangedHandler();
  public delegate void VideoGammaContrastBrightnessHandler();

  /// <summary>
  /// Singleton class which holds all GFX related settings
  /// </summary>
  public class GUIGraphicsContext
  {
    //enum containing current state of mediaportal
    public enum State
    {
      STARTING,		// starting up
      RUNNING,		// running
      STOPPING		// stopping
    }
    /// <summary>
    /// Event which will be triggered when a message has arrived
    /// </summary>
    static public event SendMessageHandler Receivers;

    /// <summary>
    /// Event which will be triggered when a action has arrived
    /// </summary>
    static public event OnActionHandler OnNewAction;


    /// <summary>
    /// Event which will be triggered when the video window location/size or AR have been changed
    /// </summary>
    static public event VideoWindowChangedHandler OnVideoWindowChanged;


    /// <summary>
    /// Event which will be triggered when contrast,brightness,gamma settings have been changed
    /// </summary>
    static public event VideoGammaContrastBrightnessHandler OnGammaContrastBrightnessChanged;

    static public Direct3D.Device DX9Device = null;								// pointer to current DX9 device
    static private string m_strSkin = "";									// name of the current skin
    static private bool m_bFullScreenVideo = false;			// boolean indicating if we're in GUI or fullscreen video/tv mode
    static private System.IntPtr m_ipActiveForm;						// pointer to the current GDI window
    static System.Drawing.Rectangle m_RectVideo;									// rectangle of the video preview window
    static Geometry.Type m_ARType = Geometry.Type.Normal;// current video transformation type (see geometry.cs)
    static int m_iOSDOffset = 0;								// y-offset of the video/tv OSD
    static int m_iOverScanLeft = 0;						// x offset screen calibration
    static int m_iOverScanTop = 0;							// y offset screen calibration
    static int m_iOverScanWidth = 0;						// width screen calibratoin
    static int m_iOverScanHeight = 0;					// height screen calibratoin
    static float m_fPixelRatio = 1.0f;						// current pixel ratio correction
    static State m_eState;											// state of application
    static bool m_bOverlay = true;							// boolean indicating if the overlay window is allowed to be shown
    static int m_iOffsetX = 0;						// x offset of GUI calibration
    static int m_iOffsetY = 0;						// y offset of GUI calibration
    static float m_fZoomHorizontal = 1.0f;           // x zoom of GUI calibration
    static float m_fZoomVertical = 1.0f;             // y zoom of GUI calibration
    static bool m_bTopBarHidden = false; 				// Topbar hidden status for autohide
    static bool m_bAutoHideTopBar = false;      // Topbar autohide status
    static bool m_bDefaultTopBarHide = false;      // Topbar.xml default autohide status
    static DateTime m_dtTopBarTimeOut = DateTime.Now; // Topbar timeout timer
    static int m_iSubtitles = 550; 						// Y position for subtitles
    static bool m_bCalibrating = false;					// boolean indicating if we are in calibration mode or in normal mode
    static bool m_bPlaying;										// boolean indicating if we are playing any media or not
    static public Graphics graphics = null;							// GDI+ Graphics object
    static public Form form = null;									// Current GDI form
    static int m_iBrightness = -1;							// brightness value
    static int m_iGamma = -1;										// gamma value
    static int m_iContrast = -1;								// contrast value
    static int m_iSaturation = -1;							// saturation value
    static int m_Sharpness = -1;								// sharpness value
    static bool m_bMouseSupport = true;					// boolean indicating if we should present mouse controls like scrollbars
    static bool m_bDBLClickAsRightclick = false;	// boolean indicating that we want to use double click to open a context menu
    static Size m_skinSize = new Size(720, 576);// original width/height for which the skin was designed
    static bool m_bShowBackGround = true;				//boolean indicating if we should show the GUI background or if we should show live tv in the background
    static bool m_bPlayingVideo = false;				//boolean indicating if we are playing a movie
    static int m_iScrollSpeedVertical = 5;							//scroll speed for controls which scroll
    static int m_iScrollSpeedHorizontal = 5;							//scroll speed for controls which scroll
    static int m_iCharsInCharacterSet = 255;		//number of characters for current fonts
    static bool m_bEditMode = false;						//boolean indicating if we are in skin edit mode
    static bool m_bAnimations = true;						//boolean indicating animiations are turned on or off
    static IRender m_renderFrame = null;
    static bool vmr9Active = false;
    static int m_iMaxFPS = 20;
    static long m_iDesiredFrameTime = 100;
    static float m_fCurrentFPS = 0;
    static float m_fVMR9FPS = 0;
    static float lasttime = 0f;
    static bool vmr9RenderBusy = false;
    static bool blankScreen = false;
    static bool turnOffMonitor = false;
    static PresentParameters presentParameters;
    static bool vmr9Allowed = true;
    static Size videoSize;
    static bool hasFocus = false;
    static ILog _log;

    const uint SC_MONITORPOWER = 0xF170;
    const uint WM_SYSCOMMAND = 0x0112;
    const uint MONITOR_ON = 0x0001;
    const uint MONITOR_OFF = 0x0002;

    [DllImport("user32.dll")]
    static extern bool SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

    // singleton. Dont allow any instance of this class
    private GUIGraphicsContext()
    {
    }

    static GUIGraphicsContext()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    /// <summary>
    /// Enable/disable screen output
    /// </summary>
    static public bool BlankScreen
    {
      get { return blankScreen; }
      set
      {
        if (value != blankScreen)
        {
          if (turnOffMonitor)
          {
            if (value)
              SendMessage(Form.ActiveForm.Handle, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_OFF);
            else
              SendMessage(Form.ActiveForm.Handle, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_ON);
          }

          blankScreen = value;
          if (OnVideoWindowChanged != null)
            OnVideoWindowChanged();
        }
      }
    }

    /// <summary>
    /// Property to enable/disable animations
    /// </summary>
    static public bool Animations
    {
      get { return m_bAnimations; }
      set { m_bAnimations = value; }
    }

    /// <summary>
    /// property to enable/disable skin-editting mode
    /// </summary>
    static public bool EditMode
    {
      get { return m_bEditMode; }
      set { m_bEditMode = value; }
    }

    /// <summary>
    /// Property to get windowed/fullscreen state of application
    /// </summary>
    static bool Fullscreen
    {
      get
      {
        return ((Width == Screen.PrimaryScreen.Bounds.Width) && (Height == Screen.PrimaryScreen.Bounds.Height));
      }
    }

    /// <summary>
    /// Save calibration settings to calibrationWxH.xml
    /// where W=resolution width
    /// H=resolution height
    /// </summary>
    static public void Save()
    {
      string strFileName = String.Format("ScreenCalibration{0}x{1}", Width, Height);
      if (Fullscreen)
        strFileName += ".fs.xml";
      else
        strFileName += ".xml";

      // _log.Info("save {0}" ,strFileName);
      using (MediaPortal.Profile.Settings xmlWriter = new MediaPortal.Profile.Settings(strFileName))
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
      }
    }

    /// <summary>
    /// Load calibration values for current resolution
    /// </summary>
    static public void Load()
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

      string strFileName = String.Format("ScreenCalibration{0}x{1}", Width, Height);
      if (Fullscreen)
        strFileName += ".fs.xml";
      else
        strFileName += ".xml";

      _log.Info("  load {0}", strFileName);
      using (MediaPortal.Profile.Settings xmlReader = new MediaPortal.Profile.Settings(strFileName))
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

      using (MediaPortal.Profile.Settings xmlReader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        m_iMaxFPS = xmlReader.GetValueAsInt("screen", "maxguifps", 25);
        SyncFrameTime();
        m_iScrollSpeedVertical = xmlReader.GetValueAsInt("general", "scrollspeedvertical", 8);
        m_iScrollSpeedHorizontal = xmlReader.GetValueAsInt("general", "scrollspeedhorizontal", 2);
        m_bAnimations = xmlReader.GetValueAsBool("general", "animations", true);
        turnOffMonitor = xmlReader.GetValueAsBool("general", "turnoffmonitor", false);
      }
    }

    /// <summary>
    /// Send a message to anyone interested
    /// </summary>
    /// <param name="msg">The message.</param>
    static public void SendMessage(GUIMessage msg)
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
    static public void OnAction(Action action)
    {
      if (OnNewAction != null)
      {
        OnNewAction(action);
      }
    }

    /// <summary>
    /// Return screen/window Height
    /// </summary>
    static public int Height
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
    static public int Width
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
    /// Apply screen offset correct 
    /// </summary>
    /// <param name="fx">X correction.</param>
    /// <param name="fy">Y correction.</param>
    static public void Correct(ref float fx, ref float fy)
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
    static public void ScaleRectToScreenResolution(ref int left, ref int top, ref int right, ref int bottom)
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
    static public void ScalePosToScreenResolution(ref int x, ref int y)
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
    static public void ScaleVertical(ref int y)
    {
      // Adjust for global zoom.
      float fZoomedScreenHeight = (float)Height * ZoomVertical;

      float fSkinHeight = (float)m_skinSize.Height;

      float fPercentY = fZoomedScreenHeight / fSkinHeight;
      y = (int)Math.Round(((float)y) * fPercentY);
    }

    /// <summary>
    /// Scale y position for current resolution
    /// </summary>
    /// <param name="y">Y coordinate to scale.</param>
    static public int ScaleVertical(int y)
    {
      int sy = y;
      ScaleVertical(ref sy);
      return sy;
    }

    /// <summary>
    /// Scale X position for current resolution
    /// </summary>
    /// <param name="y">X coordinate to scale.</param>
    static public void ScaleHorizontal(ref int x)
    {
      // Adjust for global zoom.
      float fZoomedScreenWidth = (float)Width * ZoomHorizontal;

      float fSkinWidth = (float)m_skinSize.Width;

      // X
      float fPercentX = (fZoomedScreenWidth) / fSkinWidth;
      x = (int)Math.Round(((float)x) * fPercentX);
    }


    /// <summary>
    /// Scale X position for current resolution
    /// </summary>
    /// <param name="y">X coordinate to scale.</param>
    static public int ScaleHorizontal(int x)
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
    static public void DescalePosToScreenResolution(ref int x, ref int y)
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

    /// <summary>
    /// Get/set current Aspect Ratio Mode
    /// </summary>
    static public Geometry.Type ARType
    {
      get { return m_ARType; }
      set
      {
        if (value != m_ARType)
        {
          m_ARType = value;
          if (OnVideoWindowChanged != null) OnVideoWindowChanged();
        }
      }
    }

    /// <summary>
    /// Get/set current skin name
    /// </summary>
    static public string Skin
    {
      set { m_strSkin = value; }
      get { return m_strSkin; }
    }

    /// <summary>
    /// Get/Set vertical offset for the OSD
    /// </summary>
    static public int OSDOffset
    {
      get { return m_iOSDOffset; }
      set { m_iOSDOffset = value; }
    }

    /// <summary>
    /// Get/Set left calibration value
    /// </summary>
    static public int OverScanLeft
    {
      get { return m_iOverScanLeft; }
      set { m_iOverScanLeft = value; }
    }

    /// <summary>
    /// Get/Set upper calibration value
    /// </summary>
    static public int OverScanTop
    {
      get { return m_iOverScanTop; }
      set { m_iOverScanTop = value; }
    }

    /// <summary>
    /// Get/Set calibration width
    /// </summary>
    static public int OverScanWidth
    {
      get { return m_iOverScanWidth; }
      set { m_iOverScanWidth = value; }
    }

    /// <summary>
    /// Get/Set calibration height
    /// </summary>
    static public int OverScanHeight
    {
      get { return m_iOverScanHeight; }
      set { m_iOverScanHeight = value; }
    }

    /// <summary>
    /// Get/Set current pixel Ratio
    /// </summary>
    static public float PixelRatio
    {
      get { return m_fPixelRatio; }
      set { m_fPixelRatio = value; }
    }

    /// <summary>
    /// get /set whether we're playing a movie , visz or TV in
    /// fullscreen mode or in windowed (preview) mode
    /// </summary>
    static public bool IsFullScreenVideo
    {
      get { return m_bFullScreenVideo; }
      set
      {
        if (value != m_bFullScreenVideo)
        {
          m_bFullScreenVideo = value;
          if (OnVideoWindowChanged != null) OnVideoWindowChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set video window rectangle
    /// </summary>
    static public System.Drawing.Rectangle VideoWindow
    {
      get { return m_RectVideo; }
      set
      {
        if (!m_RectVideo.Equals(value))
        {
          if (m_RectVideo.Width == 0) m_RectVideo.Width = 1;
          if (m_RectVideo.Height == 0) m_RectVideo.Height = 1;
          m_RectVideo = value;
          if (OnVideoWindowChanged != null) OnVideoWindowChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set application state (starting,running,stopping)
    /// </summary>
    static public State CurrentState
    {
      get { return m_eState; }
      set
      {
        m_eState = value;
      }
    }

    /// <summary>
    /// Get pointer to the applications form (needed by overlay windows)
    /// </summary>
    static public IntPtr ActiveForm
    {
      get { return m_ipActiveForm; }
      set { m_ipActiveForm = value; }
    }

    /// <summary>
    /// return whether we're currently calibrating or not
    /// </summary>
    static public bool Calibrating
    {
      get { return m_bCalibrating; }
      set { m_bCalibrating = value; }
    }

    /// <summary>
    /// Get/Set wheter overlay window is enabled or disabled
    /// </summary>
    static public bool Overlay
    {
      get { return m_bOverlay; }
      set
      {
        m_bOverlay = value;
        if (!m_bOverlay) { m_RectVideo.Width = 1; m_RectVideo.Height = 1; }
        if (!ShowBackground) m_bOverlay = false;
        if (OnVideoWindowChanged != null) OnVideoWindowChanged();
      }
    }

    /// <summary>
    /// Get/Set left screen calibration
    /// </summary>
    static public int OffsetX
    {
      get { return m_iOffsetX; }
      set { m_iOffsetX = value; }
    }

    /// <summary>
    /// Get/Set upper screen calibration
    /// </summary>
    static public int OffsetY
    {
      get { return m_iOffsetY; }
      set { m_iOffsetY = value; }
    }

    /// <summary>
    /// Get/Set vertical zoom screen calibration
    /// </summary>
    static public float ZoomVertical
    {
      get { return m_fZoomVertical; }
      set { m_fZoomVertical = value; }
    }

    /// <summary>
    /// Get/Set vertical zoom screen calibration
    /// </summary>
    static public float ZoomHorizontal
    {
      get { return m_fZoomHorizontal; }
      set { m_fZoomHorizontal = value; }
    }

    /// <summary>
    /// Get/Set topbar hidden status
    /// </summary>
    static public bool TopBarHidden
    {
      get { return m_bTopBarHidden; }
      set { m_bTopBarHidden = value; }
    }

    /// <summary>
    /// Get/Set topbar autohide status
    /// </summary>
    static public bool AutoHideTopBar
    {
      get { return m_bAutoHideTopBar; }
      set { m_bAutoHideTopBar = value; }
    }

    /// <summary>
    /// Get/Set default topbar autohide status
    /// </summary>
    static public bool DefaultTopBarHide
    {
      get { return m_bDefaultTopBarHide; }
      set { m_bDefaultTopBarHide = value; }
    }

    /// <summary>
    /// Get/Set topbar timeout
    /// </summary>
    static public DateTime TopBarTimeOut
    {
      get { return m_dtTopBarTimeOut; }
      set { m_dtTopBarTimeOut = value; }
    }

    /// <summary>
    /// Get/Set Y-position for subtitles
    /// </summary>
    static public int Subtitles
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
    static public void GetOutputRect(int iSourceWidth, int iSourceHeight, int iMaxWidth, int iMaxHeight, out int width, out int height)
    {
      // calculate aspect ratio correction factor
      float fPixelRatio = GUIGraphicsContext.PixelRatio;

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
    static public bool IsPlaying
    {
      get { return m_bPlaying; }
      set
      {
        m_bPlaying = value;
        if (!m_bPlaying) IsPlayingVideo = false;
      }
    }


    /// <summary>
    /// Get/Set whether a a movie (or livetv) is currently playing
    /// </summary>
    static public bool IsPlayingVideo
    {
      get { return m_bPlayingVideo; }
      set
      {
        m_bPlayingVideo = value;
      }
    }

    /// <summary>
    /// Get/Set the Brightness.
    /// </summary>
    static public int Brightness
    {
      get
      { return m_iBrightness; }
      set
      {
        if (m_iBrightness != value)
        {
          m_iBrightness = value;
          if (OnGammaContrastBrightnessChanged != null) OnGammaContrastBrightnessChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set the Contrast.
    /// </summary>
    static public int Contrast
    {
      get
      { return m_iContrast; }
      set
      {
        if (m_iContrast != value)
        {
          m_iContrast = value;
          if (OnGammaContrastBrightnessChanged != null) OnGammaContrastBrightnessChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set the Gamma.
    /// </summary>
    static public int Gamma
    {
      get
      { return m_iGamma; }
      set
      {
        if (m_iGamma != value)
        {
          m_iGamma = value;
          if (OnGammaContrastBrightnessChanged != null) OnGammaContrastBrightnessChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set the Saturation.
    /// </summary>
    static public int Saturation
    {
      get
      { return m_iSaturation; }
      set
      {
        if (m_iSaturation != value)
        {
          m_iSaturation = value;
          if (OnGammaContrastBrightnessChanged != null) OnGammaContrastBrightnessChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set the Sharpness.
    /// </summary>
    static public int Sharpness
    {
      get
      { return m_Sharpness; }
      set
      {
        if (m_Sharpness != value)
        {
          m_Sharpness = value;
          if (OnGammaContrastBrightnessChanged != null) OnGammaContrastBrightnessChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set  if there is MouseSupport.
    /// </summary>
    static public bool MouseSupport
    {
      get { return m_bMouseSupport; }
      set { m_bMouseSupport = value; }
    }

    /// <summary>
    /// Get/Set  if we want to use double click to be used as right click
    /// </summary>
    static public bool DBLClickAsRightClick
    {
      get { return m_bDBLClickAsRightclick; }
      set { m_bDBLClickAsRightclick = value; }
    }

    /// <summary>
    /// Get/Set the size of the skin.
    /// </summary>	
    static public Size SkinSize
    {
      get { return m_skinSize; }
      set { m_skinSize = value; }
    }


    /// <summary>
    /// Get/Set whether we should show the GUI as background or 
    /// live tv as background
    /// </summary>
    static public bool ShowBackground
    {
      get { return m_bShowBackGround; }
      set
      {
        m_bShowBackGround = value;
        if (!m_bShowBackGround) Overlay = false;
      }
    }

    /// <summary>
    /// Get/Set the current scroll speed 
    /// </summary>
    static public int ScrollSpeedVertical
    {
      get { return m_iScrollSpeedVertical; }
      set
      {
        if (m_iScrollSpeedVertical < 0) return;
        m_iScrollSpeedVertical = value;
      }
    }

    /// <summary>
    /// Get/Set the current scroll speed 
    /// </summary>
    static public int ScrollSpeedHorizontal
    {
      get { return m_iScrollSpeedHorizontal; }
      set
      {
        if (m_iScrollSpeedHorizontal < 0) return;
        m_iScrollSpeedHorizontal = value;
      }
    }


    /// <summary>
    /// Get/Set the current maximum number of FPS
    /// </summary>
    static public int MaxFPS
    {
      get { return m_iMaxFPS; }
      set
      {
        if (m_iMaxFPS < 0) return;
        m_iMaxFPS = value;
        SyncFrameTime();
      }
    }

    /// <summary>
    /// Get the number of ticks for each frame to get MaxFPS
    /// </summary>
    static public long DesiredFrameTime
    {
      get { return m_iDesiredFrameTime; }
    }

    /// <summary>
    /// Get/Set the current maximum number of FPS
    /// </summary>
    static public float CurrentFPS
    {
      get { return m_fCurrentFPS; }
      set
      {
        if (m_fCurrentFPS < 0) return;
        m_fCurrentFPS = value;
      }
    }

    /// <summary>
    /// Get/Set the number of characters used for the fonts
    /// </summary>
    static public int CharsInCharacterSet
    {
      get { return m_iCharsInCharacterSet; }
      set
      {
        if (m_iCharsInCharacterSet < 128) return;
        m_iCharsInCharacterSet = value;
      }
    }
    static public IRender RenderGUI
    {
      get { return m_renderFrame; }
      set { m_renderFrame = value; }
    }

    static public float Vmr9FPS
    {
      get { return m_fVMR9FPS; }
      set { m_fVMR9FPS = value; }
    }
    static public bool Vmr9Active
    {
      get
      {
        return vmr9Active;
      }
      set
      {
        if (value != vmr9Active)
        {
          vmr9Active = value;
          if (vmr9Active) _log.Info("VMR9: now active");
          else
          {
            _log.Info("VMR9: not active");
          }
        }
      }
    }
    static public float TimePassed
    {
      get
      {
        float time = DXUtil.Timer(DirectXTimer.GetAbsoluteTime);
        float difftime = time - lasttime;
        lasttime = time;
        return (difftime);
      }
    }
    static public bool InVmr9Render
    {
      get
      {
        return vmr9RenderBusy;
      }
      set
      {
        vmr9RenderBusy = value;
      }
    }

    static void SyncFrameTime()
    {
      m_iDesiredFrameTime = DXUtil.TicksPerSecond / m_iMaxFPS;
    }

    static public PresentParameters DirectXPresentParameters
    {
      get { return presentParameters; }
      set { presentParameters = value; }
    }
    static public bool VMR9Allowed
    {
      get { return vmr9Allowed; }
      set { vmr9Allowed = value; }
    }
    static public Size VideoSize
    {
      get { return videoSize; }
      set { videoSize = value; }
    }
    static public bool HasFocus
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
    static public bool IsTvWindow(int windowId)
    {
      if (windowId == (int)GUIWindow.Window.WINDOW_TV) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TVGUIDE) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTV) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_SCHEDULER) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_SEARCHTV) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TELETEXT) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_CONFLICTS) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_MAIN) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_AUTO) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS_STATUS) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_SETTINGS) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_NO_SIGNAL) return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO) return true;

      return false;
    }


  }
}
