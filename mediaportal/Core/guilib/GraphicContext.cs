using System;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using AMS.Profile;

namespace MediaPortal.GUI.Library
{
  public delegate void OnActionHandler(Action action);
  public delegate void SendMessageHandler(GUIMessage message);
  public delegate void VideoWindowChangedHandler();
  public delegate void VideoGammaContrastBrightnessHandler();
  public class GUIGraphicsContext
  {
    public enum State
    {
      STARTING,
      RUNNING,
      STOPPING
    }
    /// <summary>
    /// Event which will be triggered when a message has arrived
    /// </summary>
    static public event SendMessageHandler     Receivers;
    static public event OnActionHandler        ActionHandlers;
		
    /// <summary>
    /// Event which will be triggered when the video window location/size or AR changes
    /// </summary>
    static public event VideoWindowChangedHandler OnVideoWindowChanged;
    static public event VideoGammaContrastBrightnessHandler OnGammaContrastBrightnessChanged;

    static protected Direct3D.Device m_device=null;
    static private string           m_strSkin="";
    static private bool             m_bFullScreenVideo=false;
    static private System.IntPtr    m_ipActiveForm   ;
    static System.Drawing.Rectangle m_RectVideo;
    static Geometry.Type            m_ARType=Geometry.Type.Normal;
    static int                      m_iOSDOffset=0;
    static int                      m_iOverScanLeft=0;
    static int                      m_iOverScanTop=0;
    static int                      m_iOverScanWidth=0;
    static int                      m_iOverScanHeight=0;
    static float                    m_fPixelRatio=1.0f;
    static State                    m_eState;
    static bool                     m_bOverlay=true;
    static int                      m_iOffsetX=0;
    static int                      m_iOffsetY=0;
    static int                      m_iSubtitles=550;
    static bool                     m_bCalibrating=false;
    static bool                     m_bPlaying;
    static Graphics                 m_graphics=null;
    static Form                     m_form=null;
    static int                      m_iBrightness=0;
    static int                      m_iGamma=0;
    static int                      m_iContrast=0;
    static int                      m_iSaturation=0;
    static int                      m_Sharpness=0;
    static bool                     m_bMouseSupport=true;
    static Size                     m_skinSize = new Size(720,576);
    static bool											m_bShowBackGround=true;
    static bool											m_bPlayingVideo=false;
    static int                      m_iScrollSpeed=5;
    static int                      m_iCharsInCharacterSet=255;
    static bool                     m_bEditMode=false;
    static bool                     m_bAnimations=true;
    // singleton. Dont allow any instance of this class
    private GUIGraphicsContext()
    {
    }

    static public bool Animations
    {
      get { return m_bAnimations;}
      set { m_bAnimations=value;}
    }
    static public bool EditMode
    {
      get { return m_bEditMode;}
      set { m_bEditMode=value;}
    }
    /// <summary>
    /// Save calibration settings to calibrationWxH.xml
    /// where W=resolution width
    /// H=resolution height
    /// </summary>
    static public void Save()
    {
      string strFileName=String.Format("calibration{0}x{1}.xml", Width,Height);
      // Log.Write("save {0}" ,strFileName);
      using (Xml xmlWriter= new Xml(strFileName))
      {
        xmlWriter.SetValue("screen","offsetx",m_iOffsetX.ToString() );
        xmlWriter.SetValue("screen","offsety",m_iOffsetY.ToString() );
        xmlWriter.SetValue("screen","offsetosd",m_iOSDOffset.ToString());
        xmlWriter.SetValue("screen","overscanleft",m_iOverScanLeft.ToString());
        xmlWriter.SetValue("screen","overscantop",m_iOverScanTop.ToString());
        xmlWriter.SetValue("screen","overscanwidth",m_iOverScanWidth.ToString());
        xmlWriter.SetValue("screen","overscanheight",m_iOverScanHeight.ToString());
        xmlWriter.SetValue("screen","pixelratio",m_fPixelRatio.ToString());
        xmlWriter.SetValue("screen","subtitles",m_iSubtitles.ToString());
      }

      using (AMS.Profile.Xml xmlWriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        xmlWriter.SetValue("screen","brightness",m_iBrightness.ToString());
        xmlWriter.SetValue("screen","contrast",m_iContrast.ToString());
        xmlWriter.SetValue("screen","gamma",m_iGamma.ToString());
        xmlWriter.SetValue("screen","saturation",m_iSaturation.ToString());
      }
    }

    /// <summary>
    /// Load calibration values for current resolution
    /// </summary>
    static public void Load()
    {
      
      OverScanLeft=0;
      OverScanTop=0;
      PixelRatio=1.0f;
      OSDOffset=0;
      Subtitles=Height-50;
      OverScanWidth=Width;
      OverScanHeight=Height;

      string strFileName=String.Format("calibration{0}x{1}.xml", Width,Height);
      Log.Write("load {0}" ,strFileName);
      using (Xml xmlReader= new Xml(strFileName))
      {
        m_iOffsetX=xmlReader.GetValueAsInt("screen","offsetx",0);
        m_iOffsetY=xmlReader.GetValueAsInt("screen","offsety",0);
        m_iOSDOffset=xmlReader.GetValueAsInt("screen","offsetosd",0);
        m_iOverScanLeft=xmlReader.GetValueAsInt("screen","overscanleft",0);
        m_iOverScanTop=xmlReader.GetValueAsInt("screen","overscantop",0);
        m_iOverScanWidth=xmlReader.GetValueAsInt("screen","overscanwidth",Width);
        m_iOverScanHeight=xmlReader.GetValueAsInt("screen","overscanheight",Height);
        m_iSubtitles=xmlReader.GetValueAsInt("screen","subtitles",Height-50);
        m_fPixelRatio=xmlReader.GetValueAsFloat("screen","pixelratio",1.0f);
      }

      using (AMS.Profile.Xml xmlReader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        m_iBrightness=xmlReader.GetValueAsInt("screen","brightness",0);
        m_iContrast=xmlReader.GetValueAsInt("screen","contrast",0);
        m_iGamma=xmlReader.GetValueAsInt("screen","gamma",0);
        m_iSaturation=xmlReader.GetValueAsInt("screen","saturation",0);
        m_iScrollSpeed=xmlReader.GetValueAsInt("general","scrollspeed",5);
        m_bAnimations=xmlReader.GetValueAsBool("general","animations",true);
      }

    }

		/// <summary>
		/// Send a message to anyone interested
		/// </summary>
		/// <param name="msg">The message.</param>
    static public void SendMessage(GUIMessage msg)
    {
      if (Receivers!=null)
      {
        Receivers(msg);
      }
    }

		/// <summary>
		/// Get/set DX9 device.
		/// </summary>
    static public Direct3D.Device DX9Device
    {
      get { return m_device;}
      set { m_device=value;Load();}
    }

		/// <summary>
		/// Return screen/window Height
		/// </summary>
    static public int Height
    {
      get 
      {
        if (DX9Device !=null)
        {
          return DX9Device.PresentationParameters.BackBufferHeight;
        }
        return m_form.ClientSize.Height;
      }
    }

		/// <summary>
		/// Return screen/window Width.
		/// </summary>
    static public int Width
    {
      get 
      {
        if (DX9Device !=null)
        {
          return DX9Device.PresentationParameters.BackBufferWidth;
        }
        return m_form.ClientSize.Width;
      }
    }

		/// <summary>
		/// Apply screen calibration.
		/// </summary>
		/// <param name="fx">X correction.</param>
		/// <param name="fy">Y correction.</param>
    static public void Correct(ref float fx, ref float fy)
    {
      fx  += (float)OffsetX;
      fy  += (float)OffsetY;
    }

		/// <summary>
		/// Scale rectangle for current resolution.
		/// </summary>
		/// <param name="left">left side</param>
		/// <param name="top">top side</param>
		/// <param name="right">right side</param>
		/// <param name="bottom">bottom side</param>
    static public void ScaleRectToScreenResolution( ref int left, ref int top, ref int right, ref int bottom)
    {
      float fSkinWidth =(float)m_skinSize.Width;
      float fSkinHeight=(float)m_skinSize.Height;

      float fPercentX = ((float)Width ) / fSkinWidth;
      left   = (int)Math.Round( ((float)left)	* fPercentX); 
      right  = (int)Math.Round( ((float)right)	* fPercentX); 
      
      float fPercentY = ((float)Height) / fSkinHeight;
      top    = (int)Math.Round ( ((float)top)		 * fPercentY); 
      bottom = (int)Math.Round ( ((float)bottom) * fPercentY); 
    }

		/// <summary>
		/// Scale position for current resolution
		/// </summary>
		/// <param name="x">X coordinate to scale.</param>
		/// <param name="y">Y coordinate to scale.</param>
    static public void ScalePosToScreenResolution(ref int x, ref int y)
    {
      float fSkinWidth =(float)m_skinSize.Width;
      float fSkinHeight=(float)m_skinSize.Height;

      float fPercentX = ((float)Width ) / fSkinWidth;
      float fPercentY = ((float)Height) / fSkinHeight;
      x  = (int)Math.Round  ( ((float)x)		 * fPercentX); 
      y  = (int)Math.Round  ( ((float)y)		 * fPercentY); 
    }

		static public void ScaleVertical(ref int y)
		{
			float fSkinHeight=(float)m_skinSize.Height;
			float fPercentY = ((float)Height) / fSkinHeight;
			y  = (int)Math.Round  ( ((float)y)		 * fPercentY); 
		}
		static public void ScaleHorizontal(ref int x)
		{
			float fSkinWidth =(float)m_skinSize.Width;
			float fPercentX = ((float)Width ) / fSkinWidth;
			x  = (int)Math.Round  ( ((float)x)		 * fPercentX); 
		}

		/// <summary>
		/// Descale a position
		/// </summary>
		/// <param name="x">X coordinate to descale.</param>
		/// <param name="y">Y coordinate to descale.</param>
    static public void DescalePosToScreenResolution(ref int x, ref int y)
    {
      float fSkinWidth =(float)m_skinSize.Width;
      float fSkinHeight=(float)m_skinSize.Height;

      float fPercentX = fSkinWidth/((float)Width ) ;
      float fPercentY = fSkinHeight/((float)Height) ;
      x  = (int)Math.Round  ( ((float)x)		 * fPercentX); 
      y  = (int)Math.Round  ( ((float)y)		 * fPercentY); 
    }

		/// <summary>
		/// Get/set current Aspect Ratio Mode
		/// </summary>
    static public Geometry.Type ARType
    {
      get { return m_ARType;}
      set 
      { 
        if (value!= m_ARType)
        {
          m_ARType=value;
          if (OnVideoWindowChanged!=null) OnVideoWindowChanged();
        }
      }
    }

		/// <summary>
		/// Get/set current skin
		/// </summary>
    static public string Skin
    {
      set { m_strSkin=value;}
      get { return m_strSkin;}
    }

    /// <summary>
    /// Get/Set vertical offset for the OSD
    /// </summary>
    static public int OSDOffset
    {
      get {return m_iOSDOffset;}
      set {m_iOSDOffset=value;}
    }

    /// <summary>
    /// Get/Set left calibration value
    /// </summary>
    static public int OverScanLeft
    {
      get {return m_iOverScanLeft;}
      set {m_iOverScanLeft=value;}
    }

    /// <summary>
    /// Get/Set upper calibration value
    /// </summary>
    static public int OverScanTop
    {
      get {return m_iOverScanTop;}
      set {m_iOverScanTop=value;}
    }

    /// <summary>
    /// Get/Set calibration width
    /// </summary>
    static public int OverScanWidth
    {
      get {return m_iOverScanWidth;}
      set {m_iOverScanWidth=value;}
    }

    /// <summary>
    /// Get/Set calibration height
    /// </summary>
    static public int OverScanHeight
    {
      get {return m_iOverScanHeight;}
      set {m_iOverScanHeight=value;}
    }

    /// <summary>
    /// Get/Set current pixel Ratio
    /// </summary>
    static public float PixelRatio
    {
      get { return m_fPixelRatio;}
      set { m_fPixelRatio=value;}
    }

    /// <summary>
    /// get /set whether we're playing a movie , visz or TV in
    /// fullscreen mode or in windowed (preview) mode
    /// </summary>
    static public bool IsFullScreenVideo
    {
      get { return m_bFullScreenVideo;}
      set 
      { 
        if (value != m_bFullScreenVideo)
        {
          m_bFullScreenVideo=value;
          if (OnVideoWindowChanged!=null) OnVideoWindowChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set video window rectangle
    /// </summary>
    static public System.Drawing.Rectangle VideoWindow
    {
      get { return m_RectVideo;}
      set 
      {
        if (!m_RectVideo.Equals(value))
        {
          if (m_RectVideo.Width==0) m_RectVideo.Width=1;
          if (m_RectVideo.Height==0) m_RectVideo.Height=1;
          m_RectVideo=value;
          if (OnVideoWindowChanged!=null) OnVideoWindowChanged();
        }
      }
    }

    /// <summary>
    /// Get/Set application state (starting,running,stopping)
    /// </summary>
    static public State CurrentState
    {
      get { return m_eState;}
      set 
      {
        m_eState=value;
      }
    }
    
    /// <summary>
    /// Get pointer to the applications form (needed by overlay windows)
    /// </summary>
    static public IntPtr ActiveForm
    {
      get { return m_ipActiveForm;}
      set { m_ipActiveForm=value;}
    }

    /// <summary>
    /// return whether we're currently calibrating or not
    /// </summary>
    static public bool Calibrating
    {
      get { return m_bCalibrating;}
      set { m_bCalibrating=value;}
    }

    /// <summary>
    /// Get/Set wheter overlay window is enabled or disabled
    /// </summary>
    static public bool Overlay
    {
      get { return m_bOverlay;}
      set 
      { 
        if ( m_bOverlay!=value)
        {
          if (OnVideoWindowChanged!=null) OnVideoWindowChanged();
          m_bOverlay=value;
        }
      }
    }

    /// <summary>
    /// Get/Set left screen calibration
    /// </summary>
    static public int OffsetX
    {
      get { return m_iOffsetX;}
      set { m_iOffsetX=value;}
    }

    /// <summary>
    /// Get/Set upper screen calibration
    /// </summary>
    static public int OffsetY
    {
      get { return m_iOffsetY;}
      set { m_iOffsetY=value;}
    }
    
    /// <summary>
    /// Get/Set Y-position for subtitles
    /// </summary>
    static public int Subtitles
    {
      get { return m_iSubtitles;}
      set { m_iSubtitles=value;}
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
    static public void GetOutputRect(int iSourceWidth, int iSourceHeight, int iMaxWidth, int iMaxHeight, out int width,out int height)
    {
      // calculate aspect ratio correction factor
      float fPixelRatio = GUIGraphicsContext.PixelRatio;

      float fSourceFrameAR = (float)iSourceWidth/iSourceHeight;
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
      get { return m_bPlaying;}
      set 
      {
        m_bPlaying=value;
				if (!m_bPlaying) IsPlayingVideo=false;
      }
    }
		

		/// <summary>
		/// Get/Set whether a a movie (or livetv) is currently playing
		/// </summary>
		static public bool IsPlayingVideo
		{
			get { return m_bPlayingVideo;}
			set 
			{
				m_bPlayingVideo=value;
			}
		}

    /// <summary>
    /// get/set, placeholder for form.graphics
    /// </summary>
    static public Graphics graphics
    {
      get 
      { 
        return m_graphics;
      }
      set { m_graphics=value;}
    }

    /// <summary>
    /// Get/Set placeholder to the app's form
    /// </summary>
    static public Form form
    {
      get  { return m_form;}
      set { m_form=value;}
    }

		/// <summary>
		/// Get/Set the Brightness.
		/// </summary>
    static public int Brightness
    {
      get 
      {  return m_iBrightness;}
      set 
      {
        if (m_iBrightness!=value)
        {
          m_iBrightness=value;
          if (OnGammaContrastBrightnessChanged!=null) OnGammaContrastBrightnessChanged();
        }
      }
    }
    
		/// <summary>
		/// Get/Set the Contrast.
		/// </summary>
		static public int Contrast
    {
      get 
      {  return m_iContrast;}
      set 
      {
        if (m_iContrast!=value)
        {
          m_iContrast=value;
          if (OnGammaContrastBrightnessChanged!=null) OnGammaContrastBrightnessChanged();
        }
      }
    }

		/// <summary>
		/// Get/Set the Gamma.
		/// </summary>
		static public int Gamma
    {
      get 
      {  return m_iGamma;}
      set 
      {
        if (m_iGamma!=value)
        {
          m_iGamma=value;
          if (OnGammaContrastBrightnessChanged!=null) OnGammaContrastBrightnessChanged();
        }
      }
    }

		/// <summary>
		/// Get/Set the Saturation.
		/// </summary>
    static public int Saturation
    {
      get 
      {  return m_iSaturation;}
      set 
      {
        if (m_iSaturation!=value)
        {
          m_iSaturation=value;
          if (OnGammaContrastBrightnessChanged!=null) OnGammaContrastBrightnessChanged();
        }
      }
    }

		/// <summary>
		/// Get/Set the Sharpness.
		/// </summary>
    static public int Sharpness
    {
      get 
      {  return m_Sharpness;}
      set 
      {
        if (m_Sharpness!=value)
        {
          m_Sharpness=value;
          if (OnGammaContrastBrightnessChanged!=null) OnGammaContrastBrightnessChanged();
        }
      }
    }

		/// <summary>
		/// Get/Set the if there is MouseSupport.
		/// </summary>
    static public bool MouseSupport
    {
      get { return m_bMouseSupport;}
      set { m_bMouseSupport=value;}
    }
    
		/// <summary>
		/// Get/Set the size of the skin.
		/// </summary>	
		static public Size SkinSize
    {
      get { return m_skinSize;}
      set { m_skinSize=value;}
    }
 
		/// <summary>
		/// Delegates an action if there are actionhandlers.
		/// </summary>
		/// <param name="action">The action that needs to be delegated.</param>
		static public void OnAction(Action action)
    {
      if (ActionHandlers!=null) ActionHandlers(action);
    }
		static public bool ShowBackground
		{
			get { return m_bShowBackGround;}
			set { m_bShowBackGround=value;}
    }
    static public int ScrollSpeed
    {
      get { return m_iScrollSpeed;}
      set { m_iScrollSpeed=value;}
    }
    static public int CharsInCharacterSet
    {
      get { return m_iCharsInCharacterSet;}
      set {m_iCharsInCharacterSet=value;}
    }
  }
}