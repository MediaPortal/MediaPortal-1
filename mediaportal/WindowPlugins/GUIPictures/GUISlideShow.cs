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
	/// todo : add the 'ken burns' effect
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
    int                			m_iTransistionMethod=0;
    bool               			m_bSlideShow=false ;
    bool							 			m_bShowInfo=false;
    string						 			m_strBackgroundSlide="";
    string						 			m_strCurrentSlide="";
    bool                    m_bPause=false;
    int                     m_iZoomFactor=0;
    int                     m_iZoomLeft=0,m_iZoomTop=0;
    int					            m_iZoomWidth=0, m_iZoomHeight=0;
    int                     m_iRotate=0;
    int                     m_iSpeed=3;
    bool                    m_bPrevOverlay;
    bool                    m_bUpdate=false;    
    bool                    m_bUseRandomTransistions=true;
    Random                  randomizer = new Random(DateTime.Now.Second);     

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


    Texture GetNextSlide(out int dwWidth, out int dwHeight, out string strSlide)
    {
      dwWidth=0;
      dwHeight=0;
      strSlide="";
	    if ( m_slides.Count==0) return null;

	    m_iCurrentSlide++;
	    if ( m_iCurrentSlide >= m_slides.Count ) 
	    {
		    m_iCurrentSlide=0;
	    }
	    m_iRotate=0;
	    m_iZoomFactor=1;
	    m_iZoomLeft=0;
	    m_iZoomTop=0;

	    strSlide=(string)m_slides[m_iCurrentSlide];
      Log.Write("Next Slide: {0}/{1} : {2}", m_iCurrentSlide+1,m_slides.Count, strSlide);

			m_iRotate=PictureDatabase.GetRotation(strSlide);
      CreateThumb(strSlide);
      int iMaxWidth=MAX_PICTURE_WIDTH;
      int iMaxHeight=MAX_PICTURE_HEIGHT;
      //if (m_bSlideShow)
      {
        iMaxWidth=GUIGraphicsContext.OverScanWidth;
        iMaxHeight=GUIGraphicsContext.OverScanHeight;      
      }
      Texture texture=MediaPortal.Util.Picture.Load(strSlide,m_iRotate,iMaxWidth,iMaxHeight, true,true, out dwWidth,out dwHeight);
      GC.Collect();
      GC.Collect();
	    return texture;
    }

    Texture GetPreviousSlide(out int dwWidth,out int dwHeight, out string strSlide)
    {
      dwWidth=0;
      dwHeight=0;
      strSlide="";
      if ( m_slides.Count==0) return null;

	    m_iCurrentSlide--;
	    if ( m_iCurrentSlide < 0 ) 
	    {
		    m_iCurrentSlide=m_slides.Count-1;
	    }
	    m_iRotate=0;
	    m_iZoomFactor=1;
	    m_iZoomLeft=0;
	    m_iZoomTop=0;

	    strSlide=(string)m_slides[m_iCurrentSlide];

      Log.Write("Prev Slide: {0}/{1} : {2}", m_iCurrentSlide+1,m_slides.Count, strSlide);
			m_iRotate=PictureDatabase.GetRotation(strSlide);
      CreateThumb(strSlide);
      int iMaxWidth=MAX_PICTURE_WIDTH;
      int iMaxHeight=MAX_PICTURE_HEIGHT;
      //if (m_bSlideShow)
      {
        iMaxWidth=GUIGraphicsContext.OverScanWidth;
        iMaxHeight=GUIGraphicsContext.OverScanHeight;
      }

      Texture texture=MediaPortal.Util.Picture.Load(strSlide,m_iRotate,iMaxWidth,iMaxHeight, true,true, out dwWidth,out dwHeight);
      GC.Collect();
      GC.Collect();

	    return texture;
    }


    public void Reset()
    {
      m_slides.Clear();
      m_bShowInfo=false;
      m_bSlideShow=false;
      m_bPause=false;
 
      m_iRotate=0;
      m_iZoomFactor=1;
      m_iZoomLeft=0;
      m_iZoomTop=0;
      m_dwFrameCounter=0;
      m_iCurrentSlide=-1;
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
      m_lSlideTime=(int)(DateTime.Now.Ticks/10000) -(m_iSpeed*1000);
      
		}

    void  ShowPrevious()
    {
      if (!m_bSlideShow) 
        m_bUpdate=true;
      m_lSlideTime=(int)(DateTime.Now.Ticks/10000) -(m_iSpeed*1000);
			m_iCurrentSlide--;
			if ( m_iCurrentSlide < 0 ) 
			{
				m_iCurrentSlide=m_slides.Count-1;
			}
			m_iCurrentSlide--;
			if ( m_iCurrentSlide < 0 ) 
			{
				m_iCurrentSlide=m_slides.Count-1;
			}
			return;
			
    }

    public void Add(string strPicture)
    {
      m_slides.Add(strPicture);
    }

    public void Select(string strFile)
    {
      for (int i=0; i < m_slides.Count; ++i)
      {
        string strSlide=(string)m_slides[i];
        if (strSlide==strFile)
        {
					m_iCurrentSlide=i-1;
					m_iRotate=PictureDatabase.GetRotation(strSlide);
          return;
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

        if (iSlides > 1 || m_pTextureBackGround==null)
        {
          if (m_pTextureCurrent==null)
          {
            int dwTimeElapsed = ((int)(DateTime.Now.Ticks/10000)) - m_lSlideTime;
            if (dwTimeElapsed >= (m_iSpeed*1000) || m_pTextureBackGround==null)
            {
              if (!m_bPause|| m_pTextureBackGround==null)
              {
                // get next picture
                m_pTextureCurrent=GetNextSlide(out m_dwWidthCurrent,out m_dwHeightCurrent, out m_strCurrentSlide);
                m_dwFrameCounter=0;
                int iNewMethod;
                if (m_bUseRandomTransistions)
                {
                  do
                  {
                    iNewMethod=randomizer.Next(MAX_RENDER_METHODS);
                  } while ( iNewMethod==m_iTransistionMethod);
                  m_iTransistionMethod=iNewMethod;
                }
                else m_iTransistionMethod=9;//fade

                //g_application.ResetScreenSaver();
              }
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
        }
      }

      // render the background overlay
      
      int x,y,width,height;

      // x-fade
      if (m_iTransistionMethod!=9 || m_pTextureCurrent==null)
      {
        GUIGraphicsContext.DX9Device.Clear( ClearFlags.Target, Color.Black, 1.0f, 0);
        GetOutputRect(m_dwWidthBackGround, m_dwHeightBackGround, out x, out y, out width, out height);
        MediaPortal.Util.Picture.RenderImage(ref m_pTextureBackGround, x, y, width, height, m_iZoomWidth, m_iZoomHeight, m_iZoomLeft, m_iZoomTop,true);
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
            bResult=RenderMethod10();//x-fade 
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

      RenderPause();

      
      if (!m_bShowInfo && m_iZoomFactor == 1)
        return;

      if (m_iZoomFactor > 1)
      {
        GUIControl.SetControlLabel(GetID, LABEL_ROW1,"");
        GUIControl.SetControlLabel(GetID, LABEL_ROW2,"");
      
        string strZoomInfo;
        strZoomInfo=String.Format("{0}x ({1},{2})", m_iZoomFactor, m_iZoomLeft, m_iZoomTop);
        
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

        if (m_iZoomFactor == 1)
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

      MediaPortal.Util.Picture.RenderImage(ref m_pTextureBackGround, x, y, width, height, m_iZoomWidth, m_iZoomHeight, m_iZoomLeft, m_iZoomTop,lColorDiffuse);

      //next render new image
      GetOutputRect(m_dwWidthCurrent, m_dwHeightCurrent, out x, out y, out width, out height);
      lColorDiffuse=(iAlpha);
      lColorDiffuse<<=24;
      lColorDiffuse |= 0xffffff;
      MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent, x, y, width, height, m_dwWidthCurrent, m_dwHeightCurrent,0,0,lColorDiffuse);
      return bResult;
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
          if (m_iZoomFactor==1) 
          {
            ShowPrevious();
          }
          else
          {
            m_iZoomLeft-=25;
            if (m_iZoomLeft < 0) m_iZoomLeft = 0;
          }
          break;
        case Action.ActionType.ACTION_NEXT_PICTURE:
          if (m_iZoomFactor==1)
          {
            ShowNext();
          }
          else
          {
            m_iZoomLeft+=25;
            if (m_iZoomLeft > (int)m_dwWidthBackGround - m_iZoomWidth) m_iZoomLeft = m_dwWidthBackGround - m_iZoomWidth;
          }
          break;

          case Action.ActionType.ACTION_MOVE_DOWN:
            if (m_iZoomFactor > 1 ) m_iZoomTop+=25;
            if (m_iZoomTop > (int)m_dwHeightBackGround - m_iZoomHeight) m_iZoomTop = m_dwHeightBackGround - m_iZoomHeight;
            m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;

          case Action.ActionType.ACTION_MOVE_UP:
            if (m_iZoomFactor > 1 ) m_iZoomTop-=25;
            if (m_iZoomTop < 0) m_iZoomTop = 0;
            m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;

          case Action.ActionType.ACTION_SHOW_CODEC:
            m_bShowInfo=!m_bShowInfo;
            m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;

        case Action.ActionType.ACTION_PAUSE:
          if (m_bSlideShow) 
          {
            m_bPause=!m_bPause;
          }
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        break;

        case Action.ActionType.ACTION_ZOOM_OUT:
          Zoom(m_iZoomFactor-1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        break;

        case Action.ActionType.ACTION_ZOOM_IN:
          Zoom(m_iZoomFactor+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        break;

        case Action.ActionType.ACTION_ROTATE_PICTURE:
          m_iRotate++;
          if (m_iRotate>=4)
          {
            m_iRotate=0;
          }
          PictureDatabase.SetRotation(m_strBackgroundSlide,m_iRotate);
          DoRotate();
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        break;

        case Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_1:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_2:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_3:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_4:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_5:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_6:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_7:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_8:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
          break;
        case Action.ActionType.ACTION_ZOOM_LEVEL_9:
          Zoom((action.wID - Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL)+1);
          m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
        break;
        case Action.ActionType.ACTION_ANALOG_MOVE:
          float fX=2*action.fAmount1;
          float fY=2*action.fAmount2;
          if (fX!=0.0f||fY!=0.0f)
          {
            if (m_iZoomFactor>1)
            {
              m_iZoomLeft += (int)fX;
              m_iZoomTop -= (int)fY;
              if (m_iZoomTop < 0) m_iZoomTop = 0;
              if (m_iZoomLeft < 0) m_iZoomLeft = 0;
              if (m_iZoomTop > (int)m_dwHeightBackGround - m_iZoomHeight) m_iZoomTop = m_dwHeightBackGround - m_iZoomHeight;
              if (m_iZoomLeft > (int)m_dwWidthBackGround - m_iZoomWidth) m_iZoomLeft = m_dwWidthBackGround - m_iZoomWidth;
            			
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
	    if (!m_bPause) return;
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
      m_iZoomFactor=1;
	    m_iZoomLeft=m_iZoomTop=0;
			CreateThumb(m_strBackgroundSlide);
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
	    if (m_iZoomFactor != 1)
	    {
		    float fScaleWidthFactor = (float)width/iSourceWidth;
		    float fScaleHeightFactor = (float)height/iSourceHeight;
		    if (width*m_iZoomFactor<iScreenWidth)
			    width*=m_iZoomFactor;
		    else
			    width=iScreenWidth;
		    if (height*m_iZoomFactor<iScreenHeight)
			    height*=m_iZoomFactor;
		    else
			    height=iScreenHeight;
		    // OK, height and width are as required - now alter our source rectangle
		    m_iZoomWidth = (int)(width/(m_iZoomFactor*fScaleWidthFactor));
		    m_iZoomHeight = (int)(height/(m_iZoomFactor*fScaleHeightFactor));
	    }
	    x = (iScreenWidth - width)/2 + iOffsetX1;
	    y = (iScreenHeight - height)/2 + iOffsetY1;
    }

    void Zoom(int iZoom)
    {
	    if (iZoom > MAX_ZOOM_FACTOR || iZoom < 1)
		    return;
	    int x,y,width,height;
	    GetOutputRect(m_dwWidthBackGround,m_dwHeightBackGround,out x,out y,out width,out height);
	    float middlex = m_iZoomLeft + m_iZoomWidth*0.5f;
	    float middley = m_iZoomTop + m_iZoomHeight*0.5f;
	    m_iZoomFactor = iZoom;
	    if (m_iZoomFactor == 1)
	    {
		    m_iZoomLeft=0;
		    m_iZoomTop=0;
		    m_iZoomWidth=m_dwWidthBackGround;
		    m_iZoomHeight=m_dwHeightBackGround;
        m_bPause=false;
	    }
	    else
	    {
		    GetOutputRect(m_dwWidthBackGround,m_dwHeightBackGround,out x,out y,out width,out height);
		    m_iZoomLeft = (int)(middlex - m_iZoomWidth*0.5f);
		    m_iZoomTop = (int)(middley - m_iZoomHeight*0.5f);
		    if (m_iZoomLeft < 0) m_iZoomLeft = 0;
		    if (m_iZoomTop < 0) m_iZoomTop = 0;
		    if (m_iZoomLeft > (int)m_dwWidthBackGround-m_iZoomWidth) m_iZoomLeft = m_dwWidthBackGround-m_iZoomWidth;
		    if (m_iZoomTop > (int)m_dwHeightBackGround-m_iZoomHeight) m_iZoomTop = m_dwHeightBackGround-m_iZoomHeight;
        m_bPause=true;
	    }
    }

    void LoadSettings()
    {
      using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        m_iSpeed=xmlreader.GetValueAsInt("pictures","speed",3);
        m_iSlideShowTransistionFrames=xmlreader.GetValueAsInt("pictures","transisition",20);
        m_bUseRandomTransistions=xmlreader.GetValueAsBool("pictures","random",true);
      }
    }
    
    public override void OnDeviceRestored()
    {
      if (GUIWindowManager.ActiveWindow==GetID)
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_PICTURES);
    }

		void CreateThumb(string strSlide)
		{
      return;
      //if (m_bSlideShow) return;

			//string strThumb=Utils.GetThumb(strSlide);
			//MediaPortal.Util.Picture.CreateThumbnail(strSlide,strThumb,512,512, m_iRotate);
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
