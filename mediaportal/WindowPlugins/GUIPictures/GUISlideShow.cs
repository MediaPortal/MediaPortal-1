using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Picture.Database;
using MediaPortal.Dialogs;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Pictures
{
	/// <summary>
	/// todo : adding 'ken burns' effects
	/// todo : adding OSD (stripped if for KenBurns)
	/// </summary>
	public class GUISlideShow: GUIWindow
	{
    enum DirectionType
    {
      Left,
      Right,
      Up,
      Down
    }
    int MAX_RENDER_METHODS =10;
    int MAX_ZOOM_FACTOR    =10;
    int MAX_PICTURE_WIDTH  =2048; //1024
    int MAX_PICTURE_HEIGHT =2048; //1024

    int LABEL_ROW1			=10;
    int LABEL_ROW2			=11;
    int LABEL_ROW2_EXTRA	=12;

    float KENBURNS_ZOOM_FACTOR = 1.15f;
    float KENBURNS_MAXZOOM = 1.30f;

    int m_iSlideShowTransistionFrames=60;
    ArrayList m_slides = new ArrayList();
    int 										m_lSlideTime=0;
    int dwCounter=0;
    Texture  			          m_pTextureBackGround=null;
    int 							 			m_dwWidthBackGround=0;
    int 							 			m_dwHeightBackGround=0;

    Texture            			m_pTextureCurrent=null;
    int 							 			m_dwWidthCurrent=0;
    int 							 			m_dwHeightCurrent=0;

    int               			m_dwFrameCounter=0;
    int								 			m_iCurrentSlide=0;
    int                     m_iLastShownSlide=-1;
    int                			m_iTransistionMethod=0;
    bool               			m_bSlideShow=false ;
    bool							 			m_bShowInfo=false;
    bool                    m_bShowZoomInfo=false;
    bool                    m_bOSDAutoHide=true;
    string						 			m_strBackgroundSlide="";
    string						 			m_strCurrentSlide="";
    bool                    m_bPause=false;
    float                   m_iZoomLeft=0,m_iZoomTop=0;
    int					            m_iZoomWidth=0, m_iZoomHeight=0;
    int                     m_iRotate=0;
    int                     m_iSpeed=3;
    bool                    m_bPrevOverlay;
    bool                    m_bUpdate=false;    
    bool                    m_bUseRandomTransistions=true;
    bool                    m_bPictureZoomed=false;
    int                     m_iZoomType=0;
    float                   m_fZoomFactor=1.0f;
    Random                  randomizer = new Random(DateTime.Now.Second);     

    // Kenburns transition variables
    bool                    m_bKenBurns=true;
    bool                    m_bLandscape=false;
    float                   m_fBestZoomFactorCurrent=1.0f;   
    float                   m_fEndZoomFactor=1.0f;      
    float                   m_fStartZoomFactor=1.0f;
    
    int                     m_iKenBurnsEffect=0;
    int                     m_iKenBurnsState=0;
    int                     m_iFrameNr;
    int                     iStartPoint;
    int                     iEndPoint;

    float                   m_fPanYChange;
    float                   m_fPanXChange;
    float                   m_fZoomChange;
    

    public GUISlideShow()
    {
      GetID=(int)GUIWindow.Window.WINDOW_SLIDESHOW;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\slideshow.xml");
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          m_bPrevOverlay=GUIGraphicsContext.Overlay;
          base.OnMessage(message);
          GUIGraphicsContext.Overlay=false;
          m_bUpdate=false;
          m_iLastShownSlide=-1;
          m_iCurrentSlide=-1;
          LoadSettings();
					return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
					Reset();
					GUIGraphicsContext.Overlay=m_bPrevOverlay;
          break;
      }
      return base.OnMessage(message);
    }

    public bool Playing
    {
      get { return m_pTextureBackGround!=null;}
    }


    Texture GetSlide(bool bZoom, out int dwWidth, out int dwHeight, out string strSlide)
    {
      dwWidth=0;
      dwHeight=0;
      strSlide="";
	    if ( m_slides.Count==0) return null;

	    m_iRotate=0;
	    m_fZoomFactor=1.0f;
      m_iKenBurnsEffect=0;
	    m_iZoomLeft=0;
	    m_iZoomTop=0;

	    strSlide=(string)m_slides[m_iCurrentSlide];
      Log.Write("Next Slide: {0}/{1} : {2}", m_iCurrentSlide+1,m_slides.Count, strSlide);
      using (PictureDatabase dbs = new PictureDatabase())
      {
        m_iRotate=dbs.GetRotation(strSlide);
      }
      int iMaxWidth=MAX_PICTURE_WIDTH;
      int iMaxHeight=MAX_PICTURE_HEIGHT;
      //if (m_bSlideShow)
      {
        iMaxWidth=GUIGraphicsContext.OverScanWidth;
        iMaxHeight=GUIGraphicsContext.OverScanHeight;      
      }
      Texture texture=MediaPortal.Util.Picture.Load(strSlide,m_iRotate,iMaxWidth,iMaxHeight, true, bZoom, out dwWidth,out dwHeight);
      GC.Collect();
      GC.Collect();
	    return texture;
    }
    
    public void Reset()
    {
      m_slides.Clear();
      m_bShowInfo=false;
      m_bShowZoomInfo=false;
      m_bSlideShow=false;
      m_bPause=false;
 
      m_iRotate=0;
      m_fZoomFactor=1.0f;
      m_iKenBurnsEffect=0;
      m_bPictureZoomed=false;
      m_iZoomLeft=0;
      m_iZoomTop=0;
      m_dwFrameCounter=0;
      m_iCurrentSlide=-1;
      m_iLastShownSlide=-1;
      m_strBackgroundSlide="";
      m_strCurrentSlide="";
      m_lSlideTime=0;
      if (null!=m_pTextureBackGround)
      {
        m_pTextureBackGround.Dispose();
        m_pTextureBackGround=null;
      }
      if (null!=m_pTextureCurrent)
      {
        m_pTextureCurrent.Dispose();
        m_pTextureCurrent=null;
      }
    }

    void  ShowNext()
    {
      if (!m_bSlideShow)
        m_bUpdate=true;

      // Check image transition
      if (m_pTextureCurrent != null)
        return;

      // Reset slide time
      m_lSlideTime=(int)(DateTime.Now.Ticks/10000);

      // Get next picture
      m_iCurrentSlide++;
      if ( m_iCurrentSlide >= m_slides.Count ) 
      {
        m_iCurrentSlide=0;
      }

      // Change image
      if (null!=m_pTextureBackGround)
      {
        m_pTextureBackGround.Dispose();
        m_pTextureBackGround=null;
      }
      m_iZoomLeft=0;
      m_iZoomTop=0;
      m_fZoomFactor=1.0f;
      m_iKenBurnsEffect=0;
      m_pTextureBackGround=GetSlide(true, out m_dwWidthBackGround,out m_dwHeightBackGround, out m_strCurrentSlide);
      m_iLastShownSlide=m_iCurrentSlide;
      m_strBackgroundSlide=m_strCurrentSlide;
      m_dwFrameCounter=0;
		}

    void  ShowPrevious()
    {
      if (!m_bSlideShow)
        m_bUpdate=true;

      // Check image transition
      if (m_pTextureCurrent != null)
        return;
      
      // Reset slide time
      m_lSlideTime=(int)(DateTime.Now.Ticks/10000);

      // Get previous picture
      m_iCurrentSlide--;
      if ( m_iCurrentSlide < 0 ) 
      {
        m_iCurrentSlide=m_slides.Count-1;
      }

      // Change image
      if (null!=m_pTextureBackGround)
      {
        m_pTextureBackGround.Dispose();
        m_pTextureBackGround=null;
      }
      m_iZoomLeft=0;
      m_iZoomTop=0;
      m_fZoomFactor=1.0f;
      m_iKenBurnsEffect=0;
      m_pTextureBackGround=GetSlide(true, out m_dwWidthCurrent,out m_dwHeightCurrent, out m_strCurrentSlide);
      m_iLastShownSlide=m_iCurrentSlide;
      m_strBackgroundSlide=m_strCurrentSlide;
      m_dwFrameCounter=0;
    }

    public void Add(string strPicture)
    {
      if (Utils.IsPicture(strPicture))
      {
        m_slides.Add(strPicture);
      }
    }

    public void Select(string strFile)
    {
      using (PictureDatabase dbs = new PictureDatabase())
      {
        for (int i=0; i < m_slides.Count; ++i)
        {
          string strSlide=(string)m_slides[i];
          if (strSlide==strFile)
          {
            m_iCurrentSlide=i-1;
            m_iRotate=dbs.GetRotation(strSlide);
            return;
          }
        }
      }
    }
    
    public bool InSlideShow
    {
      get { return m_bSlideShow;}
    }

    public void StartSlideShow()
    {
      m_bSlideShow=true;
    }


    public override void Render()
    {
      m_dwFrameCounter++;
      int iSlides= m_slides.Count;
      if (0==iSlides) return;

      if (m_bUpdate||m_bSlideShow || null==m_pTextureBackGround) 
      {
        m_bUpdate=false;
        if (iSlides > 1 || m_pTextureBackGround==null)
        {
          if (m_pTextureCurrent==null)
          {
            int dwTimeElapsed = ((int)(DateTime.Now.Ticks/10000)) - m_lSlideTime;
            if (dwTimeElapsed >= (m_iSpeed*1000) || m_pTextureBackGround==null)
            {
              if ((!m_bPause && !m_bPictureZoomed) || m_pTextureBackGround==null)
              {
                m_iCurrentSlide++;
                if (m_iCurrentSlide >= m_slides.Count) 
                {
                  m_iCurrentSlide=0;
                }
              }
            }

            if (m_iCurrentSlide != m_iLastShownSlide)
            { 
              // Reset
              m_dwFrameCounter=0;
              m_iLastShownSlide=m_iCurrentSlide;

              // Get selected picture (zoomed to full screen)
              if (!m_bKenBurns)
                m_pTextureCurrent=GetSlide(true, out m_dwWidthCurrent,out m_dwHeightCurrent, out m_strCurrentSlide);
              else
              {
                m_pTextureCurrent=GetSlide(false, out m_dwWidthCurrent,out m_dwHeightCurrent, out m_strCurrentSlide);

                // Select transition based upon picture width/height
                m_fBestZoomFactorCurrent = BestZoomCurrent();
                m_iKenBurnsEffect = InitKenBurnsTransition();                
                KenBurns(m_iKenBurnsEffect, true);

                // When using KenBurns skip picture 2 picture transition
                if (null!=m_pTextureBackGround)
                {
                  // Clear background
                  m_pTextureBackGround.Dispose();
                  m_pTextureBackGround=null;
                }
              }
              
              int iNewMethod;
              if (m_bUseRandomTransistions)
              {
                do
                {
                  iNewMethod=randomizer.Next(MAX_RENDER_METHODS);
                } while ( iNewMethod==m_iTransistionMethod);
                m_iTransistionMethod=iNewMethod;
              }
              else 
              {
                m_iTransistionMethod=9;
              }

              //g_application.ResetScreenSaver();
            }
          }         
        }

        
        // swap our buffers over
        if (null==m_pTextureBackGround) 
        {
          if (null==m_pTextureCurrent) return;
          m_pTextureBackGround=m_pTextureCurrent;
          m_dwWidthBackGround=m_dwWidthCurrent;
          m_dwHeightBackGround=m_dwHeightCurrent;         
          m_strBackgroundSlide=m_strCurrentSlide;
          m_pTextureCurrent=null;          
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          if (m_bKenBurns) Zoom(m_fZoomFactor);
        }
      }

      // render the background overlay     
      int x,y,width,height;

      // x-fade
      GUIGraphicsContext.DX9Device.Clear( ClearFlags.Target, Color.Black, 1.0f, 0);
      if (m_iTransistionMethod != 9 || m_pTextureCurrent==null)
      {
        GetOutputRect(m_dwWidthBackGround, m_dwHeightBackGround, out x, out y, out width, out height);
        MediaPortal.Util.Picture.RenderImage(ref m_pTextureBackGround, x, y, width, height, m_iZoomWidth, m_iZoomHeight, (int)m_iZoomLeft, (int)m_iZoomTop,true);
      }

      //	g_graphicsContext.Get3DDevice()->UpdateOverlay(m_pSurfaceBackGround, &source, &dest, true, 0x00010001);

      if (m_pTextureCurrent!=null) 
      {
        // render the new picture
        bool bResult=false;
        
        switch (m_iTransistionMethod)
        {
          case 0:
            bResult=RenderMethod1();// open from left->right
            break;
          case 1:
            bResult=RenderMethod2();// move into the screen from left->right
            break;
          case 2:
            bResult=RenderMethod3();// move into the screen from right->left
            break;
          case 3:
            bResult=RenderMethod4();// move into the screen from up->bottom
            break;
          case 4:
            bResult=RenderMethod5();// move into the screen from bottom->top
            break;
          case 5:
            bResult=RenderMethod6();// open from up->bottom
            break;
          case 6:
            bResult=RenderMethod7();// slide from left<-right
            break;
          case 7:
            bResult=RenderMethod8();// slide from down->up
            break;
          case 8:
            bResult=RenderMethod9();// grow from middle
            break;
          case 9:
            bResult=RenderMethod10();// x-fade 
            break;
        }

        if (bResult)
        {
          if (null==m_pTextureCurrent) return;
          if (null!=m_pTextureBackGround)
          {
            m_pTextureBackGround.Dispose();
            m_pTextureBackGround=null;
          }
          m_pTextureBackGround=m_pTextureCurrent;
          m_dwWidthBackGround=m_dwWidthCurrent;
          m_dwHeightBackGround=m_dwHeightCurrent;
          m_strBackgroundSlide=m_strCurrentSlide;
          m_pTextureCurrent=null; 
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        }
      }
      else
      {
        // Start KenBurns effect
        if (m_bKenBurns)          
        {
          if (m_bPictureZoomed || m_bPause) 
            m_iKenBurnsEffect=0;
          else
            KenBurns(m_iKenBurnsEffect, false);
        }
      }
      
      RenderPause();

      if (!m_bShowInfo && !m_bShowZoomInfo)
      {
        m_bOSDAutoHide = true;
        return;
      }
     
      // Auto hide OSD
      if (m_bOSDAutoHide && (m_bShowInfo||m_bShowZoomInfo))
      {
        int dwOSDTimeElapsed = ((int)(DateTime.Now.Ticks/10000)) - m_lSlideTime;
        if (dwOSDTimeElapsed >= 3000)
        {
          m_bShowInfo = false;
          m_bShowZoomInfo = false;
        }                  
      }

      if ((m_bPictureZoomed && m_bShowZoomInfo) || m_bShowInfo)
      {
        GUIControl.SetControlLabel(GetID, LABEL_ROW1,"");
        GUIControl.SetControlLabel(GetID, LABEL_ROW2,"");
      
        string strZoomInfo;
        strZoomInfo=String.Format("{0}x ({1},{2})", m_fZoomFactor, m_iZoomLeft, m_iZoomTop);

        GUIControl.SetControlLabel(GetID, LABEL_ROW2_EXTRA,strZoomInfo);
      }

      if ( m_bShowInfo )
      {
        string strFileInfo, strSlideInfo;
        string strFileName=System.IO.Path.GetFileName(m_strBackgroundSlide);
        strFileInfo=String.Format("{0}x{1} {2}", m_dwWidthBackGround, m_dwHeightBackGround,strFileName);

				GUIControl.SetControlLabel(GetID, LABEL_ROW1,strFileInfo);
				strSlideInfo=String.Format("{0}/{1}", 1+m_iCurrentSlide ,m_slides.Count);
				GUIControl.SetControlLabel(GetID, LABEL_ROW2,strSlideInfo);

        if (!m_bPictureZoomed)
        {
          GUIControl.SetControlLabel(GetID, LABEL_ROW2_EXTRA,"");
        }
      }
      base.Render();
    }

    //pan from left->right
    bool RenderKenBurns(float zoom, float pan, DirectionType direction)
    {
      //zoom (75%-100%)
      if (zoom <  75) zoom=75;
      if (zoom > 100) zoom=100;

      //pan 75%-125%
      if (pan  <  75) pan=75;
      if (pan  > 125) pan=125;
      
      //direction (left,right,up,down)

      return true;
    }

    // Select transition based upon picture width/height
    int InitKenBurnsTransition()
    {
      int iEffect=0;
      int iRandom=0;

      if (m_bLandscape)
      {
        // Landscape picture
        iRandom = randomizer.Next(2);

        switch (iRandom)
        {
          default:
          case 0: 
            iEffect = 6; // start with BestZoom / Pan / Zoom out
            break;
          case 1: 
            iEffect = 7; // start with 100% Zoom / Zoom in / Pan 
            break;
        }
      }
      else
      {
        // Portrait picture
        iRandom = randomizer.Next(2);

        switch (iRandom)
        {
          default:
          case 0: 
            iEffect = 4; // For Portait pictures zoomstart 100 / Zoom in to BestZoom / pan down
            break;

          case 1: 
            iEffect = 5; // For Portait pictures zoomstart BestZoom / Pan down / Zoom to 100%
            break;
        }
      }

      Log.Write("KernBurns effect nr: {0}", iEffect);
      
      return iEffect;
    }


    // open from left->right
    bool RenderMethod1()
    {
      bool bResult=false;

      int x,y,width,height;
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);

      int iStep= width/m_iSlideShowTransistionFrames;
      if (0==iStep) iStep=1;
      int iExpandWidth=m_dwFrameCounter*iStep;
      if (iExpandWidth >= width)
      {
        iExpandWidth=width;
        bResult=true;
      }
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent, x, y, iExpandWidth, height, m_dwWidthCurrent, m_dwHeightCurrent,0,0,false);
      return bResult;
    }

    // move into the screen from left->right
    bool RenderMethod2()
    {
      bool bResult=false;
      int x,y,width,height;
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);

      int iStep= width/m_iSlideShowTransistionFrames;
      if (0==iStep) iStep=1;
      int iPosX = m_dwFrameCounter*iStep - (int)width;
      if (iPosX >=x)
      {
        iPosX=x;
        bResult=true;
      }
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent, iPosX, y, width, height, m_dwWidthCurrent, m_dwHeightCurrent,0,0,false);
      return bResult;
    }

    // move into the screen from right->left
    bool RenderMethod3()
    {
      bool bResult=false;
      int x,y,width,height;
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);

      int iStep= width/m_iSlideShowTransistionFrames;
      if (0==iStep) iStep=1;
      int posx = x + width - m_dwFrameCounter*iStep ;
      if (posx <=x )
      {
        posx = x;
        bResult=true;
      }
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,posx,y,width,height,m_dwWidthCurrent,m_dwHeightCurrent,0,0,false);
      return bResult;
    }

    // move into the screen from up->bottom
    bool RenderMethod4()
    {
      bool bResult=false;
      int x,y,width,height;
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);

      int iStep= height/m_iSlideShowTransistionFrames;
      if (0==iStep) iStep=1;
      int posy = m_dwFrameCounter*iStep - height;
      if (posy >= y)
      {
        posy = y;
        bResult=true;
      }
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,x,posy,width,height,m_dwWidthCurrent,m_dwHeightCurrent,0,0,false);
      return bResult;
    }

    // move into the screen from bottom->top
    bool RenderMethod5()
    {
      bool bResult=false;
      int x,y,width,height;
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);

      int iStep= height/m_iSlideShowTransistionFrames;
      if (0==iStep) iStep=1;
      int posy = y+height-m_dwFrameCounter*iStep ;
      if (posy <=y)
      {
        posy=y;
        bResult=true;
      }
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,x,posy,width,height,m_dwWidthCurrent,m_dwHeightCurrent,0,0,false);
      return bResult;
    }


    // open from up->bottom
    bool RenderMethod6()
    {
      bool bResult=false;
      int x,y,width,height;
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);

      int iStep= height/m_iSlideShowTransistionFrames;
      if (0==iStep) iStep=1;
      int newheight=m_dwFrameCounter*iStep;
      if (newheight >= height)
      {
        newheight=height;
        bResult=true;
      }
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,x,y,width,newheight,m_dwWidthCurrent,m_dwHeightCurrent,0,0,false);
      return bResult;
    }

    // slide from left<-right
    bool RenderMethod7()
    {
      bool bResult=false;
      int x,y,width,height;
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);

      int iStep= width/m_iSlideShowTransistionFrames;
      if (0==iStep) iStep=1;
      int newwidth=m_dwFrameCounter*iStep;
      if (newwidth >=width)
      {
        newwidth=width;
        bResult=true;
      }

      //right align the texture
      int posx=x + width-newwidth;
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,posx,y,newwidth,height,m_dwWidthCurrent,m_dwHeightCurrent,0,0,false);
      return bResult;
    }


    // slide from down->up
    bool RenderMethod8()
    {
      bool bResult=false;
      int x,y,width,height;
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);

      int iStep= height/m_iSlideShowTransistionFrames;
      if (0==iStep) iStep=1;
      int newheight=m_dwFrameCounter*iStep;
      if (newheight >=height)
      {
        newheight=height;
        bResult=true;
      }

      //bottom align the texture
      int posy=y + height-newheight;
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,x,posy,width,newheight,m_dwWidthCurrent,m_dwHeightCurrent,0,0,false);
      return bResult;
    }

    // grow from middle
    bool RenderMethod9()
    {
      bool bResult=false;
      int x,y,width,height;
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);
      int iStepX= width/m_iSlideShowTransistionFrames;
      int iStepY= height/m_iSlideShowTransistionFrames;
      if (0==iStepX) iStepX=1;
      if (0==iStepY) iStepY=1;
      int newheight=m_dwFrameCounter*iStepY;
      int newwidth=m_dwFrameCounter*iStepX;
      if (newheight >=height)
      {
        newheight=height;
        bResult=true;
      }
      if (newwidth >=width)
      {
        newwidth=width;
        bResult=true;
      }

      //center align the texture
      int posx = x + (width - newwidth)/2;
      int posy = y + (height - newheight)/2;
      
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,posx,posy,newwidth,newheight,m_dwWidthCurrent, m_dwHeightCurrent,0,0,false);
      return bResult;
    }

    // fade in
    bool RenderMethod10()
    {
      bool bResult=false;

      int x,y,width,height;

      GetOutputRect(m_dwWidthBackGround, m_dwHeightBackGround, out x, out y, out width, out height);

      int iStep= 0xff/m_iSlideShowTransistionFrames;
      if (0==iStep) iStep=1;
      int iAlpha=m_dwFrameCounter*iStep;
      if (iAlpha>= 0xff)
      {
        iAlpha=0xff;
        bResult=true;
      }
      //render background first
      long lColorDiffuse=(0xff-iAlpha);
      lColorDiffuse<<=24;
      lColorDiffuse |= 0xffffff;

      MediaPortal.Util.Picture.RenderImage(ref m_pTextureBackGround, x, y, width, height, m_iZoomWidth, m_iZoomHeight, (int)m_iZoomLeft, (int)m_iZoomTop, lColorDiffuse);

      //next render new image
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);
      lColorDiffuse=(iAlpha);
      lColorDiffuse<<=24;
      lColorDiffuse |= 0xffffff;
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent, x, y, width, height, m_dwWidthCurrent, m_dwHeightCurrent,0,0,lColorDiffuse);
      return bResult;
    }

    float BestZoom()
    {     
      float fZoom;
      // Default picutes is zoom best fit (max width or max height)
      float ZoomFactorX = (float)GUIGraphicsContext.OverScanWidth/(float)m_dwWidthBackGround;
      float ZoomFactorY = (float)GUIGraphicsContext.OverScanHeight/(float)m_dwHeightBackGround;

      // Get minimal zoom level (1.0==100%)
      m_bLandscape = false;
      fZoom = ZoomFactorX-ZoomFactorY+1.0f;
      if (ZoomFactorY > ZoomFactorX)
      {
        fZoom = ZoomFactorY-ZoomFactorX+1.0f;
        m_bLandscape=true;
      }

      // Zoom 100%..150%
      if (fZoom < 1.00f)
        fZoom = 1.00f;
      if (fZoom > KENBURNS_MAXZOOM)
        fZoom = KENBURNS_MAXZOOM;

      return fZoom;
    }

    float BestZoomCurrent()
    {
      float fZoom;
      // Default picutes is zoom best fit (max width or max height)
      float ZoomFactorX = (float)GUIGraphicsContext.OverScanWidth/(float)m_dwWidthCurrent;
      float ZoomFactorY = (float)GUIGraphicsContext.OverScanHeight/(float)m_dwHeightCurrent;


      // Get minimal zoom level (1.0==100%)
      m_bLandscape = false;
      fZoom = ZoomFactorX-ZoomFactorY+1.0f;
      if (ZoomFactorY > ZoomFactorX)
      {
        fZoom = ZoomFactorY-ZoomFactorX+1.0f;
        m_bLandscape=true;
      }

      // Zoom 100%..150%
      if (fZoom < 1.00f)
        fZoom = 1.00f;
      if (fZoom > KENBURNS_MAXZOOM)
        fZoom = KENBURNS_MAXZOOM;

      return fZoom;
    }

    /// <summary>
    /// Ken Burn effects
    /// </summary>
    /// <param name="iEffect"></param>
    /// <returns></returns>
    bool KenBurns(int iEffect, bool bReset)
    {
      bool bEnd=false;
      int iNrOfFramesPerEffect = m_iSlideShowTransistionFrames*20;
    
      // Init methode
      if (bReset)
      {
        // Set first state parameters: start and end zoom factor
        m_iFrameNr = 0;
        m_iKenBurnsState = 0;        
      }

      // Check single effect end
      if (m_iFrameNr == iNrOfFramesPerEffect)
      {
        m_iFrameNr = 0;
        m_iKenBurnsState++;
      }

      // Select effect
      switch (iEffect)
      {
        default:
        case 0:
          // No effects, just wait for next picture
          break;

        case 1:
          bEnd = KenBurnsEffect1(m_iKenBurnsState, m_iFrameNr, iNrOfFramesPerEffect, bReset);
          break;
        
        case 2:
          bEnd = KenBurnsEffect2(m_iKenBurnsState, m_iFrameNr, iNrOfFramesPerEffect, bReset);
          break;

        case 3: // For Landscape picutres;
          bEnd = KenBurnsEffect3(m_iKenBurnsState, m_iFrameNr, iNrOfFramesPerEffect, bReset);
          break;

        case 4: // For Portait picutres zoomstart 100 / Zoom in to 120 / pan down
          bEnd = KenBurnsPortrait1(m_iKenBurnsState, m_iFrameNr, iNrOfFramesPerEffect, bReset);
          break;

        case 5: // For Portait picutres zoomstart BestWidth / Pan down / Zoom to 100%
          bEnd = KenBurnsPortrait2(m_iKenBurnsState, m_iFrameNr, iNrOfFramesPerEffect, bReset);
          break;

        case 6: // For Landscape picutres zoomstart 100 / Zoom in to BestWidth / pan 
          bEnd = KenBurnsLandscape1(m_iKenBurnsState, m_iFrameNr, iNrOfFramesPerEffect, bReset);
          break;

        case 7: // For Landscape picutres zoomstart BestWidth / Pan / Zoom to 100%
          bEnd = KenBurnsLandscape2(m_iKenBurnsState, m_iFrameNr, iNrOfFramesPerEffect, bReset);
          break;

      }

      // Check new rectangle
      if (m_iZoomTop < 0) m_iZoomTop = 0;
      if (m_iZoomLeft < 0) m_iZoomLeft = 0;
      if (m_iZoomTop > (m_dwHeightBackGround - m_iZoomHeight)) m_iZoomTop = (m_dwHeightBackGround - m_iZoomHeight);
      if (m_iZoomLeft > (m_dwWidthBackGround - m_iZoomWidth)) m_iZoomLeft = (m_dwWidthBackGround - m_iZoomWidth);


      if (iEffect != 0)
      {
        if (!bEnd)
        {
          if (!bReset)
          {
            m_iFrameNr++;
          }
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        }
        else
        {
          // end
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000) - (m_iSpeed*1000);
        }
      }
      return bEnd;
    }

    /* Zoom types:
       * 0: // centered, centered
       * 1: // Width centered, Top unchanged
       * 2: // Heigth centered, Left unchanged
       * 3: // Widht centered, Bottom unchanged
       * 4: // Height centered, Right unchanged
       * 5: // Top Left unchanged
       * 6: // Top Right unchanged
       * 7: // Bottom Left unchanged
       * 8: // Bottom Right unchanged
       * */

    /* Zoom points arround the rectangle
     * Selected zoom type will hold the selected point at the same place while zooming the rectangle
     * 
     *     1---------2---------3
     *     |                   |
     *     8         0         4
     *     |                   | 
     *     7---------6---------5
     * 
     */

    bool KenBurnsLandscape1(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // For Landscape picutres zoomstart 100 / Zoom in to BestWidth / pan 
      int iRandom;
      bool bEnd = false;
      if (bReset)
      {
        // find start and end points (8 possible points around the rectangle)
        iRandom = randomizer.Next(4);
        switch(iRandom)
        {
          default:
          case 0:
            iStartPoint = 1;
            iEndPoint = 4;
            break;
          case 1:
            iStartPoint = 8;
            iEndPoint = 3;
            break;
          case 2:
            iStartPoint = 8;
            iEndPoint = 5;
            break;
          case 3:
            iStartPoint = 7;
            iEndPoint = 4;
            break;
        }

        Log.Write("Start point: {0}", iStartPoint);
        Log.Write("End Point : {0}", iEndPoint);

        // Init 100% top center fixed
        m_fZoomFactor = m_fBestZoomFactorCurrent;
        m_iZoomType = iStartPoint;
      }
      else
      {
        switch (iState)
        {
          case 0: // - Zoom in from start point to 120%        
            if (iFrameNr == 0)
            {
              // Calc changes per slide
              m_fEndZoomFactor = m_fBestZoomFactorCurrent * KENBURNS_ZOOM_FACTOR; // Calc best zoom (whole screen filled + 20%)
              m_fStartZoomFactor = m_fZoomFactor;
              m_fZoomChange = (m_fEndZoomFactor - m_fZoomFactor)/iNrOfFramesPerEffect;
            }

            m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
            if (m_fZoomFactor > m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
            Zoom(m_fZoomFactor);
            break;

          case 1: // - Pan start point to end point
            if (iFrameNr == 0)
            {
              // Init single effect
              float iDestY=0;
              float iDestX=0;
              switch (iEndPoint)
              {
                case 8:
                  iDestY = (float)m_dwHeightBackGround/2;
                  iDestX = (float)m_iZoomWidth/2;
                  break;
                case 4:
                  iDestY = (float)m_dwHeightBackGround/2;
                  iDestX = (float)m_dwWidthBackGround - (float)m_iZoomWidth/2;
                  break;
                case 2:
                  iDestY = (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround/2;
                  break;
                case 6:
                  iDestY = (float)m_dwHeightBackGround - (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround/2;
                  break;
                case 1:
                  iDestY = (float)m_iZoomHeight/2;
                  iDestX = (float)m_iZoomWidth/2;
                  break;
                case 3:
                  iDestY = (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround - (float)m_iZoomWidth/2;
                  break;
                case 7:
                  iDestY = (float)m_dwHeightBackGround - (float)m_iZoomHeight/2;
                  iDestX = (float)m_iZoomWidth/2;
                  break;
                case 5://8:
                  iDestY = (float)m_dwHeightBackGround - (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround - (float)m_iZoomWidth/2;
                  break;
              }   

              m_fPanYChange = (iDestY - (m_iZoomTop+(float)m_iZoomHeight/2))/iNrOfFramesPerEffect; // Travel Y;
              m_fPanXChange = (iDestX - (m_iZoomLeft+(float)m_iZoomWidth/2))/iNrOfFramesPerEffect; // Travel Y;
            }

            Pan(m_fPanXChange, m_fPanYChange);
            break;    

          default:
            bEnd = true;
            break;
        }
      }

      return bEnd;
    }

    bool KenBurnsLandscape2(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // For Landscape picutres zoomstart BestWidth / Pan / Zoom to 100%
      int iRandom;
      bool bEnd = false;
      if (bReset)
      {
        // find start and end points (8 possible points around the rectangle)
        iRandom = randomizer.Next(4);
        switch(iRandom)
        {
          default:
          case 0:
            iStartPoint = 1;
            iEndPoint = 4;
            break;
          case 1:
            iStartPoint = 8;
            iEndPoint = 3;
            break;
          case 2:
            iStartPoint = 8;
            iEndPoint = 5;
            break;
          case 3:
            iStartPoint = 7;
            iEndPoint = 4;
            break;
        }

        Log.Write("Start point: {0}", iStartPoint);
        Log.Write("End Point : {0}", iEndPoint);

        // Init 120% top center fixed
        m_fZoomFactor = m_fBestZoomFactorCurrent * KENBURNS_ZOOM_FACTOR; // Calc best zoom (whole screen filled + 20%)
        m_iZoomType = iStartPoint;
      }
      else
      {
        switch (iState)
        {
          case 0: // - Pan start point to end point
            if (iFrameNr == 0)
            {
              // Init single effect
              float iDestY=0;
              float iDestX=0;
              switch (iEndPoint)
              {
                case 8:
                  iDestY = (float)m_dwHeightBackGround/2;
                  iDestX = (float)m_iZoomWidth/2;
                  break;
                case 4:
                  iDestY = (float)m_dwHeightBackGround/2;
                  iDestX = (float)m_dwWidthBackGround - (float)m_iZoomWidth/2;
                  break;
                case 2:
                  iDestY = (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround/2;
                  break;
                case 6:
                  iDestY = (float)m_dwHeightBackGround - (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround/2;
                  break;
                case 1:
                  iDestY = (float)m_iZoomHeight/2;
                  iDestX = (float)m_iZoomWidth/2;
                  break;
                case 3:
                  iDestY = (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround - (float)m_iZoomWidth/2;
                  break;
                case 7:
                  iDestY = (float)m_dwHeightBackGround - (float)m_iZoomHeight/2;
                  iDestX = (float)m_iZoomWidth/2;
                  break;
                case 5://8:
                  iDestY = (float)m_dwHeightBackGround - (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround - (float)m_iZoomWidth/2;
                  break;
              }   

              m_fPanYChange = (iDestY - (m_iZoomTop+(float)m_iZoomHeight/2))/iNrOfFramesPerEffect; // Travel Y;
              m_fPanXChange = (iDestX - (m_iZoomLeft+(float)m_iZoomWidth/2))/iNrOfFramesPerEffect; // Travel Y;
            }

            Pan(m_fPanXChange, m_fPanYChange);
            break;    

          case 1: // - zoom out from end point to 100%
            if (iFrameNr == 0)
            {
              // Init single effect
              m_iZoomType = iEndPoint; // Random

              // Calc changes per slide
              m_fEndZoomFactor = m_fBestZoomFactorCurrent;
              m_fStartZoomFactor = m_fZoomFactor;
              m_fZoomChange = (m_fZoomFactor - m_fEndZoomFactor)/iNrOfFramesPerEffect;
            }

            m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
            if (m_fZoomFactor < m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
            Zoom(m_fZoomFactor);
            break;

          default:
            bEnd = true;
            break;
        }
      }
    
      return bEnd;
    }

    bool KenBurnsEffectRandom(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // Start from 100%
      // - Zoom in from Random start point to 120%
      // - Pan to the right and down
      // - Zoom out from Random end point to 100%
      
      bool bEnd = false;
      if (bReset)
      {
        // find start and end points (8 possible points around the rectangle)
        iStartPoint = randomizer.Next(8); // 8 point to choise from
        iEndPoint = randomizer.Next(8);
        bool bFoundEnd=false;
        while (!bFoundEnd)
        {
          iEndPoint = randomizer.Next(8);
          if ((iEndPoint % 8) > ((iStartPoint+2)%8)) bFoundEnd = true;
          if ((iEndPoint % 8) < (iStartPoint-2)) bFoundEnd = true;
        }
        iStartPoint++; // 1..8
        iEndPoint++; // 1..8
        Log.Write("Picture start point: {0}", iStartPoint);
        Log.Write("Picture end point: {0}", iEndPoint);
      }
      else
      {
        switch (iState)
        {
          case 0: // - Zoom in from start point to 120%        
            if (iFrameNr == 0)
            {
              // Init single effect
              m_iZoomType = iStartPoint; // Random

              // Calc changes per slide
              m_fEndZoomFactor = BestZoomCurrent() * KENBURNS_ZOOM_FACTOR; // Calc best zoom (whole screen filled + 20%)
              m_fStartZoomFactor = m_fZoomFactor;
              m_fZoomChange = (m_fEndZoomFactor - m_fZoomFactor)/iNrOfFramesPerEffect;
            }

            m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
            if (m_fZoomFactor > m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
            Zoom(m_fZoomFactor);
            break;

          case 1: // - Pan start point to end point
            if (iFrameNr == 0)
            {
              // Init single effect
              float iDestY=0;
              float iDestX=0;
              switch (iEndPoint)
              {
                case 8:
                  iDestY = (float)m_dwHeightBackGround/2;
                  iDestX = (float)m_iZoomWidth/2;
                  break;
                case 4:
                  iDestY = (float)m_dwHeightBackGround/2;
                  iDestX = (float)m_dwWidthBackGround - (float)m_iZoomWidth/2;
                  break;
                case 2:
                  iDestY = (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround/2;
                  break;
                case 6:
                  iDestY = (float)m_dwHeightBackGround - (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround/2;
                  break;
                case 1:
                  iDestY = (float)m_iZoomHeight/2;
                  iDestX = (float)m_iZoomWidth/2;
                  break;
                case 3:
                  iDestY = (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround - (float)m_iZoomWidth/2;
                  break;
                case 7:
                  iDestY = (float)m_dwHeightBackGround - (float)m_iZoomHeight/2;
                  iDestX = (float)m_iZoomWidth/2;
                  break;
                case 5://8:
                  iDestY = (float)m_dwHeightBackGround - (float)m_iZoomHeight/2;
                  iDestX = (float)m_dwWidthBackGround - (float)m_iZoomWidth/2;
                  break;
              }   

              m_fPanYChange = (iDestY - (m_iZoomTop+(float)m_iZoomHeight/2))/iNrOfFramesPerEffect; // Travel Y;
              m_fPanXChange = (iDestX - (m_iZoomLeft+(float)m_iZoomWidth/2))/iNrOfFramesPerEffect; // Travel Y;
            }

            Pan(m_fPanXChange, m_fPanYChange);
            break;    

          case 2: // - zoom out from end point to 100%
            if (iFrameNr == 0)
            {
              // Init single effect
              m_iZoomType = iEndPoint; // Random

              // Calc changes per slide
              m_fEndZoomFactor = 1.0f;
              m_fStartZoomFactor = m_fZoomFactor;
              m_fZoomChange = (m_fZoomFactor - m_fEndZoomFactor)/iNrOfFramesPerEffect;
            }

            m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
            if (m_fZoomFactor < m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
            Zoom(m_fZoomFactor);
            break;

          default:
            bEnd = true;
            break;
        }
      }

      return bEnd;
    }

    bool KenBurnsEffect5(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // Start from 100%
      // - Zoom in from left center to 120%
      // - Pan to the right and down
      // - Zoom out from right center to 100%
            
      bool bEnd = false;
      switch (iState)
      {
        case 0: // - Zoom in from top left to 120%        
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 8; // Heigth centered, Left unchanged

            // Calc changes per slide
            m_fEndZoomFactor = BestZoomCurrent() * KENBURNS_ZOOM_FACTOR; // Calc best zoom (whole screen filled + 20%)
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fEndZoomFactor - m_fZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor > m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        case 1: // - Pan right and down
          if (iFrameNr == 0)
          {
            // Init single effect
            m_fPanXChange = ((float)m_dwWidthBackGround-(float)m_iZoomWidth)/iNrOfFramesPerEffect; // Travel X
            m_fPanYChange = ((float)m_dwHeightBackGround-(float)m_iZoomHeight)/iNrOfFramesPerEffect; // Travel Y
          }

          Pan(m_fPanXChange, m_fPanYChange);
          break;    

        case 2: // - zoom out from bottom right to 100%
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 5; // Bottom Right unchanged

            // Calc changes per slide
            m_fEndZoomFactor = 1.0f;
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fZoomFactor - m_fEndZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor < m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        default:
          bEnd = true;
          break;
      }

      return bEnd;
    }

    bool KenBurnsEffect4(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // For Portait picutres;
      //
      // Start from 100%
      // - Zoom in from top center to 120%
      // - Pan to the bottom
      // - Zoom out from bottom center to 100%
            
      bool bEnd = false;
      switch (iState)
      {
        case 0: // - Zoom in from top width centered to 120%        
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 2; // Widht centered, Top unchanged

            // Calc changes per slide
            m_fEndZoomFactor = BestZoomCurrent() * KENBURNS_ZOOM_FACTOR; // Calc best zoom (whole screen filled + 20%)
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fEndZoomFactor - m_fZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor > m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        case 1: // - Pan down
          if (iFrameNr == 0)
          {
            // Init single effect
            m_fPanXChange = 0;
            m_fPanYChange = ((float)m_dwHeightBackGround-(float)m_iZoomHeight-m_iZoomTop)/iNrOfFramesPerEffect; // Travel Y;
          }

          Pan(m_fPanXChange, m_fPanYChange);
          break;    

        case 2: // - zoom out from bottom width centered to 100%
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 6; // Widtht centered, Bottom unchanged

            // Calc changes per slide
            m_fEndZoomFactor = 1.0f;
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fZoomFactor - m_fEndZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor < m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        default:
          bEnd = true;
          break;
      }

      return bEnd;
    }

    bool KenBurnsEffect3(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // For Landscape picutres;
      //
      // Start from 100%
      // - Zoom in from left center to 120%
      // - Pan to the right
      // - Zoom out from right center to 100%
            
      bool bEnd = false;
      switch (iState)
      {
        case 0: // - Zoom in from left height centered to 120%        
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 8; // Heigth centered, Left unchanged

            // Calc changes per slide
            m_fEndZoomFactor = BestZoomCurrent() * KENBURNS_ZOOM_FACTOR; // Calc best zoom (whole screen filled + 20%)
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fEndZoomFactor - m_fZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor > m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        case 1: // - Pan right
          if (iFrameNr == 0)
          {
            // Init single effect
            m_fPanXChange = ((float)m_dwWidthBackGround-(float)m_iZoomWidth)/iNrOfFramesPerEffect; // Travel X
            m_fPanYChange = 0;
          }

          Pan(m_fPanXChange, m_fPanYChange);
          break;    

        case 2: // - zoom out from right height centered to 100%
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 4; // Height centered, Right unchanged

            // Calc changes per slide
            m_fEndZoomFactor = 1.0f;
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fZoomFactor - m_fEndZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor < m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        default:
          bEnd = true;
          break;
      }

      return bEnd;
    }

    bool KenBurnsEffect2(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // Start from 100%
      // - Zoom in center to 120%
      // - Pan right and down
      // - zoom out from bottom right to 100%      
      
      bool bEnd = false;
      switch (iState)
      {
        case 0: // - Zoom in from top left to 120%        
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 0; // centered, centered

            // Calc changes per slide
            m_fEndZoomFactor = BestZoomCurrent()*1.25f; // Calc best zoom (whole screen filled + 25%)
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fEndZoomFactor - m_fZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor > m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        case 1: // - Pan right and down
          if (iFrameNr == 0)
          {
            // Init single effect
            m_fPanXChange = ((float)m_dwWidthBackGround-(float)m_iZoomWidth-m_iZoomLeft)/iNrOfFramesPerEffect; // Travel X
            m_fPanYChange = ((float)m_dwHeightBackGround-(float)m_iZoomHeight-m_iZoomTop)/iNrOfFramesPerEffect; // Travel Y
          }

          Pan(m_fPanXChange, m_fPanYChange);
          break;    

        case 2: // - zoom out from bottom right to 100%
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 5; // Bottom Right unchanged

            // Calc changes per slide
            m_fEndZoomFactor = 1.0f;
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fZoomFactor - m_fEndZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor < m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        default:
          bEnd = true;
          break;
      }

      return bEnd;
    }

    bool KenBurnsEffect1(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // Start from 100%
      // - Zoom in from top left to 120%
      // - Pan right and down
      // - zoom out from bottom right to 100%      
      
      bool bEnd = false;
      switch (iState)
      {
        case 0: // - Zoom in from top left to 120%        
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 1; // Top Left unchanged

            // Calc changes per slide
            m_fEndZoomFactor = BestZoomCurrent()*1.25f; // Calc best zoom (whole screen filled + 25%)
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fEndZoomFactor - m_fZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor > m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        case 1: // - Pan right and down
          if (iFrameNr == 0)
          {
            // Init single effect
            m_fPanXChange = ((float)m_dwWidthBackGround-(float)m_iZoomWidth)/iNrOfFramesPerEffect; // Travel X
            m_fPanYChange = ((float)m_dwHeightBackGround-(float)m_iZoomHeight)/iNrOfFramesPerEffect; // Travel Y
          }

          Pan(m_fPanXChange, m_fPanYChange);
          break;    

        case 2: // - zoom out from bottom right to 100%
          if (iFrameNr == 0)
          {
            // Init single effect
            m_iZoomType = 5; // Bottom Right unchanged

            // Calc changes per slidee
            m_fEndZoomFactor = 1.0f;
            m_fStartZoomFactor = m_fZoomFactor;
            m_fZoomChange = (m_fZoomFactor - m_fEndZoomFactor)/iNrOfFramesPerEffect;
          }

          m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
          if (m_fZoomFactor < m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
          Zoom(m_fZoomFactor);
          break;

        default:
          bEnd = true;
          break;
      }

      return bEnd;
    }

    bool KenBurnsPortrait1(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // - Start from 100% and zoom in from top center to BestWidth
      // - Pan down
      
      bool bEnd = false;
      if (bReset)
      {
        // Init 100% top center fixed
        m_fZoomFactor = m_fBestZoomFactorCurrent;
        m_iZoomType = 2;
      }
      else
      {
        switch (iState)
        {
          case 0: // - Zoom in from top center
            if (iFrameNr == 0)
            {
              // Init single effect
              m_iZoomType = 2;

              // Calc changes per slide
              m_fEndZoomFactor = m_fBestZoomFactorCurrent*1.25f; // Calc best zoom (whole screen filled + 25%)
              m_fStartZoomFactor = m_fZoomFactor;
              m_fZoomChange = (m_fEndZoomFactor - m_fZoomFactor)/iNrOfFramesPerEffect;
            }

            m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
            if (m_fZoomFactor > m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
            Zoom(m_fZoomFactor);
            break;

          case 1: // - Pan down
            if (iFrameNr == 0)
            {
              // Init single effect
              m_fPanXChange = 0;
              m_fPanYChange = ((float)m_dwHeightBackGround-(float)m_iZoomHeight)/iNrOfFramesPerEffect; // Travel Y
            }

            Pan(m_fPanXChange, m_fPanYChange);
            break;    

          default:
            bEnd = true;
            break;
        }
      }
    
      return bEnd;
    }

    bool KenBurnsPortrait2(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
    {
      // - Start from BestWidth zoomed in top center 
      // - Pan down
      // - Zoomout to 100% bottom centered
            
      bool bEnd = false;
      if (bReset)
      {
        // Init 100% top center fixed
        m_fZoomFactor = m_fBestZoomFactorCurrent*1.25f;
        m_iZoomType = 2;
      }
      else
      {
        switch (iState)
        {
          case 0: // - Pan down
            if (iFrameNr == 0)
            {
              // Init single effect
              m_fPanXChange = 0;
              m_fPanYChange = ((float)m_dwHeightBackGround-(float)m_iZoomHeight)/iNrOfFramesPerEffect; // Travel Y
            }

            Pan(m_fPanXChange, m_fPanYChange);
            break;    

          case 1: // - zoom out from bottom center to 100%
            if (iFrameNr == 0)
            {
              // Init single effect
              m_iZoomType = 6; // Bottom center unchanged

              // Calc changes per slidee
              m_fEndZoomFactor = m_fBestZoomFactorCurrent;
              m_fStartZoomFactor = m_fZoomFactor;
              m_fZoomChange = (m_fZoomFactor - m_fEndZoomFactor)/iNrOfFramesPerEffect;
            }

            m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
            if (m_fZoomFactor < m_fEndZoomFactor) m_fZoomFactor = m_fEndZoomFactor;
            Zoom(m_fZoomFactor);
            break;

          default:
            bEnd = true;
            break;
        }
      }
    
      return bEnd;
    }
  
    /// <summary>
    /// Pan picture rectangle
    /// </summary>
    /// <param name="fPanX"></param>
    /// <param name="fPanY"></param>
    /// <returns>False if panned outside the picture</returns>

    bool Pan(float fPanX, float fPanY)
    {
      if ((fPanX == 0.0f) && (fPanY == 0.0f)) return false;
      
      m_iZoomLeft += fPanX;
      m_iZoomTop += fPanY;
            
      if (m_iZoomTop < 0) return false;
      if (m_iZoomLeft < 0) return false;
      if (m_iZoomTop > (m_dwHeightBackGround - m_iZoomHeight)) return false;
      if (m_iZoomLeft > (m_dwWidthBackGround - m_iZoomWidth)) return false;

      return true;
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          GUIWindowManager.PreviousWindow();
          break;
				
        case Action.ActionType.ACTION_DELETE_ITEM:
          // delete current picture
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          if (null==dlgYesNo) return;
          if (m_strBackgroundSlide.Length==0) return;
          bool bPause=m_bPause;
          m_bPause=true;
          string strFileName=System.IO.Path.GetFileName(m_strBackgroundSlide);
          dlgYesNo.SetHeading(GUILocalizeStrings.Get(664));
          dlgYesNo.SetLine(1,String.Format("{0}/{1}", 1+m_iCurrentSlide ,m_slides.Count) );
          dlgYesNo.SetLine(2, strFileName);
          dlgYesNo.SetLine(3, "");
          dlgYesNo.DoModal(GetID);

          m_bPause=bPause;
          if (!dlgYesNo.IsConfirmed) return;
          Utils.FileDelete(m_strBackgroundSlide);
          ShowNext();
					
          break;

        case Action.ActionType.ACTION_PREV_PICTURE:
          if (!m_bPictureZoomed) 
          {
            ShowPrevious();
          }
          else
          {
            m_iZoomLeft-=25;
            if (m_iZoomLeft < 0) m_iZoomLeft = 0;
            m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          }
          break;
        case Action.ActionType.ACTION_NEXT_PICTURE:
          if (!m_bPictureZoomed)
          {
            ShowNext();
          }
          else
          {
            m_iZoomLeft+=25;
            if (m_iZoomLeft > (int)m_dwWidthBackGround - m_iZoomWidth) m_iZoomLeft = (m_dwWidthBackGround - m_iZoomWidth);
            m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
          if (m_bPictureZoomed) m_iZoomTop+=25;
          if (m_iZoomTop > (int)m_dwHeightBackGround - m_iZoomHeight) m_iZoomTop = (m_dwHeightBackGround - m_iZoomHeight);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;

        case Action.ActionType.ACTION_MOVE_UP:
          if (m_bPictureZoomed) m_iZoomTop-=25;
          if (m_iZoomTop < 0) m_iZoomTop = 0;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;

        case Action.ActionType.ACTION_SHOW_INFO:
          if (m_bShowInfo)
          { 
            m_bOSDAutoHide = !m_bOSDAutoHide;
            if (m_bOSDAutoHide)
              m_bShowInfo = false;
          }
          else
          {
            m_bShowInfo=true;
            m_bOSDAutoHide=true;
            m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          }
          break;

        case Action.ActionType.ACTION_PAUSE_PICTURE:
          if (m_bSlideShow) 
          {
            if (m_bPictureZoomed)
            {
              Zoom(1.0f);
              m_bPictureZoomed=false;
              m_bPause = false;
            }
            else
              m_bPause=!m_bPause;
          }
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;

        case Action.ActionType.ACTION_ZOOM_OUT:
          Zoom(m_fZoomFactor-1.0f);
          m_bPictureZoomed = (m_fZoomFactor > 1.0f);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        break;

        case Action.ActionType.ACTION_ZOOM_IN:
          Zoom(m_fZoomFactor+1.0f);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        break;

        case Action.ActionType.ACTION_ROTATE_PICTURE:
          m_iRotate++;
          if (m_iRotate>=4)
          {
            m_iRotate=0;
          }

          using (PictureDatabase dbs = new PictureDatabase())
          {
            dbs.SetRotation(m_strBackgroundSlide,m_iRotate);
          }
          DoRotate();
          DeleteThumb(m_strBackgroundSlide);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        break;

        case Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = false;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_1:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_2:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_3:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_4:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_5:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_6:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_7:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_8:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_9:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_bPictureZoomed = true;
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        break;
        case Action.ActionType.ACTION_ANALOG_MOVE:
          float fX=2*action.fAmount1;
          float fY=2*action.fAmount2;
          if (fX!=0.0f||fY!=0.0f)
          {
            if (m_bPictureZoomed)
            {
              m_iZoomLeft += (int)fX;
              m_iZoomTop -= (int)fY;
              if (m_iZoomTop < 0) m_iZoomTop = 0;
              if (m_iZoomLeft < 0) m_iZoomLeft = 0;
              if (m_iZoomTop > (int)m_dwHeightBackGround - m_iZoomHeight) m_iZoomTop = (m_dwHeightBackGround - m_iZoomHeight);
              if (m_iZoomLeft > (int)m_dwWidthBackGround - m_iZoomWidth) m_iZoomLeft = (m_dwWidthBackGround - m_iZoomWidth);
            			
              m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
            }
          }
          break;
      } 
    }

    void RenderPause()
    {
      
	    dwCounter++;
	    if (dwCounter > 25)
	    {
		    dwCounter=0;
	    }
      if ((!m_bPause && !m_bShowInfo && !m_bShowZoomInfo && !m_bPictureZoomed) || m_bShowZoomInfo || m_bShowInfo) return;

	    if (dwCounter <13) return;
	    GUIFont pFont=GUIFontManager.GetFont("font13");
      if (pFont!=null)
      {
        string szText=GUILocalizeStrings.Get(112);
        pFont.DrawShadowText(500.0f,60.0f,0xffffffff,szText,GUIControl.Alignment.ALIGN_LEFT,2,2,0xff000000);
      }
    }

    void DoRotate()
    {
	    if (null!=m_pTextureCurrent)
	    {
		    m_pTextureCurrent.Dispose();
		    m_pTextureCurrent = null;
	    }
      if (m_strBackgroundSlide.Length==0) return;
	    

      int iMaxWidth=GUIGraphicsContext.OverScanWidth;
      int iMaxHeight=GUIGraphicsContext.OverScanHeight;      
      m_pTextureCurrent=MediaPortal.Util.Picture.Load(m_strBackgroundSlide, m_iRotate,iMaxWidth,iMaxHeight,  true,true, out m_dwWidthCurrent,out m_dwHeightCurrent);
      m_strCurrentSlide=m_strBackgroundSlide;
      m_fZoomFactor=1.0f;
      m_iKenBurnsEffect=0;
      m_bPictureZoomed=false;
	    m_iZoomLeft=m_iZoomTop=0;
	    m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
      m_dwFrameCounter=0;
      m_iTransistionMethod=9;
    }

    void GetOutputRect(int iSourceWidth, int iSourceHeight, out int x,out int y,out int width,out int height)
    {
	    // calculate aspect ratio correction factor
	    //RESOLUTION iRes = g_graphicsContext.GetVideoResolution();
	    int iOffsetX1 = GUIGraphicsContext.OverScanLeft;
	    int iOffsetY1 = GUIGraphicsContext.OverScanTop;
	    int iScreenWidth = GUIGraphicsContext.OverScanWidth;
	    int iScreenHeight = GUIGraphicsContext.OverScanHeight;
	    float fPixelRatio = GUIGraphicsContext.PixelRatio;

	    float fSourceFrameAR = ((float)iSourceWidth)/((float)iSourceHeight);
	    float fOutputFrameAR = fSourceFrameAR / fPixelRatio;
            fOutputFrameAR = ((float)iSourceWidth)/((float)iSourceHeight);
      
	    width = iScreenWidth;
	    height = (int)(width / fOutputFrameAR);
	    if (height > iScreenHeight)
	    {
		    height = iScreenHeight;
		    width = (int)(height * fOutputFrameAR);
	    }
	    m_iZoomWidth=iSourceWidth;
	    m_iZoomHeight=iSourceHeight;
	    // recalculate in case we're zooming
	    if (m_fZoomFactor != 1.0f)
	    {
		    float fScaleWidthFactor = (float)width/iSourceWidth;
		    float fScaleHeightFactor = (float)height/iSourceHeight;
		    if (width*m_fZoomFactor<iScreenWidth)
			    width = (int)(width*m_fZoomFactor);
		    else
			    width=iScreenWidth;
		    if (height*m_fZoomFactor<iScreenHeight)
			    height = (int)(height*m_fZoomFactor);
		    else
			    height=iScreenHeight;
		    // OK, height and width are as required - now alter our source rectangle
		    m_iZoomWidth = (int)(width/(m_fZoomFactor*fScaleWidthFactor));
		    m_iZoomHeight = (int)(height/(m_fZoomFactor*fScaleHeightFactor));
	    }
	    x = (iScreenWidth - width)/2 + iOffsetX1;
	    y = (iScreenHeight - height)/2 + iOffsetY1;
    }
    
    void Zoom(float fZoom)
    {
	    if (fZoom > MAX_ZOOM_FACTOR || fZoom < 1.0f)
		    return;
	    int x,y,width,height;
	    GetOutputRect(m_dwWidthBackGround,m_dwHeightBackGround,out x,out y,out width,out height);

      // Start and End point positions along the picture rectangle
      // Point zoom in/out only works if the selected Point is at the border
      // example:  "m_dwWidthBackGround == m_iZoomLeft + m_iZoomWidth"  and zooming to the left (iZoomType=4)
      float middlex = m_dwWidthBackGround/2;
      float middley = m_dwHeightBackGround/2;
      float xstart = 0;
      float xend = m_dwWidthBackGround;
      float ystart = 0;
      float yend = m_dwHeightBackGround;
      
      //float middlex = m_iZoomLeft + m_iZoomWidth*0.5f;
      //float middley = m_iZoomTop + m_iZoomHeight*0.5f;
      //float xstart = m_iZoomLeft;
      //float xend = m_iZoomLeft + m_iZoomWidth;
      //float ystart = m_iZoomTop;
      //float yend = m_iZoomTop + m_iZoomHeight;
 
	    m_fZoomFactor = fZoom;
	    if (m_fZoomFactor == 1.0f)
	    {
		    m_iZoomLeft=0;
		    m_iZoomTop=0;
		    m_iZoomWidth=m_dwWidthBackGround;
		    m_iZoomHeight=m_dwHeightBackGround;
        m_bShowZoomInfo = false;
	    }
	    else
	    {
		    GetOutputRect(m_dwWidthBackGround,m_dwHeightBackGround,out x,out y,out width,out height);
        switch(m_iZoomType)
        {
           /* 0: // centered, centered
            * 1: // Top Left unchanged
            * 2: // Width centered, Top unchanged
            * 3: // Top Right unchanged
            * 4: // Height centered, Right unchanged
            * 5: // Bottom Right unchanged
            * 6: // Widht centered, Bottom unchanged
            * 7: // Bottom Left unchanged
            * 8: // Heigth centered, Left unchanged            
            * */
          case 0: // centered, centered
            m_iZoomLeft = (int)(middlex - m_iZoomWidth*0.5f);
            m_iZoomTop = (int)(middley - m_iZoomHeight*0.5f);
            break;
          case 2: // Width centered, Top unchanged
            m_iZoomLeft = (int)(middlex - m_iZoomWidth*0.5f);
            break;
          case 8: // Heigth centered, Left unchanged
            m_iZoomTop = (int)(middley - m_iZoomHeight*0.5f);
            break;
          case 6: // Widht centered, Bottom unchanged
            m_iZoomLeft = (int)(middlex - m_iZoomWidth*0.5f);
            m_iZoomTop = yend - m_iZoomHeight;
            break;
          case 4: // Height centered, Right unchanged
            m_iZoomTop = (int)(middley - m_iZoomHeight*0.5f);
            m_iZoomLeft = xend - m_iZoomWidth;
            break;
          case 1: // Top Left unchanged
            break;
          case 3: // Top Right unchanged
            m_iZoomLeft = xend - m_iZoomWidth;
            break;
          case 7: // Bottom Left unchanged          
            m_iZoomTop = yend - m_iZoomHeight;
            break;
          case 5: // Bottom Right unchanged
            m_iZoomTop = yend - m_iZoomHeight;
            m_iZoomLeft = xend - m_iZoomWidth;
            break;

        }
        if (m_iZoomLeft < 0) m_iZoomLeft = 0;
		    if (m_iZoomTop < 0) m_iZoomTop = 0;
		    if (m_iZoomLeft > (int)m_dwWidthBackGround-m_iZoomWidth) m_iZoomLeft = (m_dwWidthBackGround-m_iZoomWidth);
		    if (m_iZoomTop > (int)m_dwHeightBackGround-m_iZoomHeight) m_iZoomTop = (m_dwHeightBackGround-m_iZoomHeight);
  
        if (m_bOSDAutoHide)
        {
        //  m_bShowZoomInfo = true;
        }
	    }
    }

    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        m_iSpeed=xmlreader.GetValueAsInt("pictures","speed",3);
        m_iSlideShowTransistionFrames=xmlreader.GetValueAsInt("pictures","transition",20);
        m_bKenBurns=xmlreader.GetValueAsBool("pictures","kenburns", false);
        m_bUseRandomTransistions=xmlreader.GetValueAsBool("pictures","random", true);
      }
    }
    
    public override void OnDeviceRestored()
    {
      if (GUIWindowManager.ActiveWindow==GetID)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_PICTURES);
    }

		void DeleteThumb(string strSlide)
		{
			string strThumb=GUIPictures.GetThumbnail(strSlide);
			Utils.FileDelete(strThumb);
      strThumb=GUIPictures.GetLargeThumbnail(strSlide) ;
      Utils.FileDelete(strThumb);
		}
    public override bool NeedRefresh()
    {
      return m_bSlideShow;
    }

    public override bool FullScreenVideoAllowed
    {
      get { return false;}
    }
	}
}
