/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Collections;
using System.Drawing;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Picture.Database;
using MediaPortal.Dialogs;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Pictures
{
	/// <summary>
	/// todo : adding zoom OSD (stripped if for KenBurns)
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
		int MAX_PICTURE_WIDTH  = 2040;
		int MAX_PICTURE_HEIGHT = 2040;

		int LABEL_ROW1			=10;
		int LABEL_ROW2			=11;
		int LABEL_ROW2_EXTRA	=12;

		float KENBURNS_ZOOM_FACTOR = 1.30f; // Zoom factor for pictures that have a black border on the sides
		float KENBURNS_ZOOM_FACTOR_FS = 1.20f; // Zoom factor for pictures that are filling the whole screen 

		float KENBURNS_MAXZOOM = 1.30f;
		int   KENBURNS_XFADE_FRAMES = 15;

		int m_iSlideShowTransistionFrames=60;
		int m_iKenBurnTransistionSpeed=40;

		ArrayList m_slides = new ArrayList();
		int 										m_lSlideTime=0;
		int dwCounter=0;
		Texture  			          m_pTextureBackGround=null;
		float 							 			m_fWidthBackGround=0;
		float 							 			m_fHeightBackGround=0;
		float                     m_fZoomFactorBackGround=1.0f;
		float                     m_fZoomLeftBackGround=0;
		float                     m_fZoomTopBackGround=0;
		int                       m_iZoomTypeBackGround=0;

		Texture            			m_pTextureCurrent=null;
		float 							 			m_fWidthCurrent=0;
		float 							 			m_fHeightCurrent=0;
		float                     m_fZoomFactorCurrent=1.0f;
		float                     m_fZoomLeftCurrent=0;
		float                     m_fZoomTopCurrent=0;
		int                       m_iZoomTypeCurrent=0;

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
		float 			            m_fZoomWidth=0, m_fZoomHeight=0;
		int                     m_iRotate=0;
		int                     m_iSpeed=3;
		bool                    m_bPrevOverlay;
		bool                    m_bUpdate=false;    
		bool                    m_bUseRandomTransistions=true;
		float                   m_fDefaultZoomFactor=1.0f;
		bool                    m_bPictureZoomed=false;
		float                   m_fUserZoomLevel=1.0f;
		bool                    m_bTrueSizeTexture=false;
		bool                    m_bAutoShuffle=false;
		bool                    m_bAutoRepeat=false;
    
		Random                  randomizer = new Random(DateTime.Now.Second);     

		// Kenburns transition variables
		bool                    m_bKenBurns=true;
		bool                    m_bLandscape=false;
		bool                    m_bFullScreen=false;
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
		bool                    m_bLoadingRawPicture=false;
		bool					_isBackgroundMusicEnabled = false;
		bool					_isBackgroundMusicPlaying = false;
		string[]				_musicFileExtensions;
 
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


		Texture GetSlide(bool bTrueSize, out float dwWidth, out float dwHeight, out string strSlide)
		{
			dwWidth=0;
			dwHeight=0;
			strSlide="";
			if ( m_slides.Count==0) return null;

			m_iRotate=0;
			m_fZoomFactorCurrent=1.0f;
			m_iKenBurnsEffect=0;
			m_fZoomLeftCurrent=0;
			m_fZoomTopCurrent=0;
			m_bShowZoomInfo = false;

			strSlide=(string)m_slides[m_iCurrentSlide];
			Log.Write("Next Slide: {0}/{1} : {2}", m_iCurrentSlide+1,m_slides.Count, strSlide);
			using (PictureDatabase dbs = new PictureDatabase())
			{
				m_iRotate=dbs.GetRotation(strSlide);
			}
			int iMaxWidth=GUIGraphicsContext.OverScanWidth;
			int iMaxHeight=GUIGraphicsContext.OverScanHeight;      
     
			m_bTrueSizeTexture=bTrueSize;
			if (bTrueSize)
			{
				iMaxWidth=MAX_PICTURE_WIDTH;
				iMaxHeight=MAX_PICTURE_HEIGHT;        
			}

			int X,Y;
			Texture texture=MediaPortal.Util.Picture.Load(strSlide,m_iRotate,iMaxWidth,iMaxHeight, true, false, true, out X,out Y);
			dwWidth=X;
			dwHeight=Y;

			CalculateBestZoom(dwWidth, dwHeight);      
			m_fZoomFactorCurrent = m_fDefaultZoomFactor;
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
			m_fZoomFactorBackGround=m_fDefaultZoomFactor;
			m_fZoomFactorCurrent=m_fDefaultZoomFactor;
			m_iKenBurnsEffect=0;
			m_bPictureZoomed=false;
			m_fZoomLeftBackGround=0;
			m_fZoomTopBackGround=0;
			m_fZoomLeftCurrent=0;
			m_fZoomTopCurrent=0;
			m_dwFrameCounter=0;
			m_iCurrentSlide=-1;
			m_iLastShownSlide=-1;
			m_strBackgroundSlide="";
			m_strCurrentSlide="";
			m_lSlideTime=0;
			m_fUserZoomLevel=1.0f;
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

			if(_isBackgroundMusicPlaying)
				g_Player.Stop();
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
				if ( m_bAutoRepeat )
				{
					m_iCurrentSlide=0;
					if ( m_bAutoShuffle )
						Shuffle();
				}
				else
					ShowPreviousWindow();
			}
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

		public void Shuffle()
		{
			Random r = new System.Random( DateTime.Now.Millisecond );
			int nItemCount = m_slides.Count;

			// iterate through each catalogue item performing arbitrary swaps
			for ( int nItem=0; nItem < nItemCount; nItem++ )
			{
				int nArbitrary = r.Next( nItemCount );

				if ( nArbitrary != nItem )
				{
					string anItem = (string)m_slides[ nArbitrary ];
					m_slides[ nArbitrary ] = m_slides[ nItem ];
					m_slides[ nItem ] = anItem;
				}
			}
		}

		void StartSlideShow()
		{
			_isBackgroundMusicPlaying = false;

			if(m_bAutoShuffle)
				Shuffle();

			m_bSlideShow=true;
		}

		public void StartSlideShow(string path)
		{
			_isBackgroundMusicPlaying = false;

			StartBackgroundMusic(path);

			if(m_bAutoShuffle)
				Shuffle();

			m_bSlideShow=true;
		}

		public override void Render(float timePassed)
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
									if ( m_bAutoRepeat )
									{
										m_iCurrentSlide=0;
										if ( m_bAutoShuffle )
											Shuffle();
									}
									else
										// How to exit back to GUIPictures?
										ShowPreviousWindow();
								}
							}
						}
					}
				}

				if (m_iCurrentSlide != m_iLastShownSlide)
				{ 
					// Reset
					m_dwFrameCounter=0;
					m_iLastShownSlide=m_iCurrentSlide;

					// Get selected picture (zoomed to full screen)
					m_pTextureCurrent=GetSlide(false, out m_fWidthCurrent,out m_fHeightCurrent, out m_strCurrentSlide);              
					if (m_bKenBurns && m_bSlideShow)
					{
						//Select transition based upon picture width/height
						//m_fBestZoomFactorCurrent = CalculateBestZoom(m_fWidthCurrent, m_fHeightCurrent);
						m_fBestZoomFactorCurrent = m_fZoomFactorCurrent;
						m_iKenBurnsEffect = InitKenBurnsTransition();                
						KenBurns(m_iKenBurnsEffect, true);        
						ZoomCurrent(m_fZoomFactorCurrent);
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

        
				// swap our buffers over
				if (null==m_pTextureBackGround) 
				{
					if (null==m_pTextureCurrent) return;
					m_pTextureBackGround=m_pTextureCurrent;
					m_fWidthBackGround=m_fWidthCurrent;
					m_fHeightBackGround=m_fHeightCurrent;         
					m_fZoomFactorBackGround=m_fZoomFactorCurrent;
					m_fZoomLeftBackGround=m_fZoomLeftCurrent;
					m_fZoomTopBackGround=m_fZoomTopCurrent;
					m_iZoomTypeBackGround=m_iZoomTypeCurrent;
					m_strBackgroundSlide=m_strCurrentSlide;
					m_pTextureCurrent=null;          
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
				}
			}

			// render the background overlay     
			float x,y,width,height;

			// x-fade
			GUIGraphicsContext.DX9Device.Clear( ClearFlags.Target, Color.Black, 1.0f, 0);
			if (m_iTransistionMethod != 9 || m_pTextureCurrent==null)
			{
				GetOutputRect(m_fWidthBackGround, m_fHeightBackGround, m_fZoomFactorBackGround, out x, out y, out width, out height);
				if (m_fZoomTopBackGround+m_fZoomHeight > m_fHeightBackGround) m_fZoomHeight = m_fHeightBackGround-m_fZoomTopBackGround;
				if (m_fZoomLeftBackGround+m_fZoomWidth > m_fWidthBackGround) m_fZoomWidth = m_fWidthBackGround-m_fZoomLeftBackGround;
				MediaPortal.Util.Picture.RenderImage(ref m_pTextureBackGround, x, y, width, height, m_fZoomWidth, m_fZoomHeight, m_fZoomLeftBackGround, m_fZoomTopBackGround,true);

				//MediaPortal.Util.Picture.DrawLine(10,10,300,300,0xffffffff);
				//MediaPortal.Util.Picture.DrawRectangle( new Rectangle(100,100,30,30),0xaaff0000,true);
				//MediaPortal.Util.Picture.DrawRectangle( new Rectangle(200,100,30,30),0xffffffff,false);
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
					m_fWidthBackGround=m_fWidthCurrent;
					m_fHeightBackGround=m_fHeightCurrent;
					m_fZoomFactorBackGround=m_fZoomFactorCurrent;
					m_fZoomLeftBackGround=m_fZoomLeftCurrent;
					m_fZoomTopBackGround=m_fZoomTopCurrent;
					m_iZoomTypeBackGround=m_iZoomTypeCurrent;
					m_strBackgroundSlide=m_strCurrentSlide;
					m_pTextureCurrent=null; 
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
				}
			}
			else
			{
				// Start KenBurns effect
				if (m_bKenBurns && m_bSlideShow && !m_bPause) 
				{
					if (m_bPictureZoomed) 
						m_iKenBurnsEffect=0;
					else
						KenBurns(m_iKenBurnsEffect, false);
				}
			}

			if (m_bSlideShow)
			{
				if (RenderPause()) return;     
			}

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

      
			if (m_bShowZoomInfo || m_bShowInfo)
			{
				GUIControl.SetControlLabel(GetID, LABEL_ROW1,"");
				GUIControl.SetControlLabel(GetID, LABEL_ROW2,"");
      
				string strZoomInfo;
				//strZoomInfo=String.Format("{0}% ({1} , {2})", (int)(m_fUserZoomLevel*100.0f), (int)m_fZoomLeftBackGround, (int)m_fZoomTopBackGround);
				strZoomInfo=String.Format("{0}% ({1} , {2})", (int)(m_fZoomFactorBackGround*100.0f), (int)m_fZoomLeftBackGround, (int)m_fZoomTopBackGround);

				GUIControl.SetControlLabel(GetID, LABEL_ROW2_EXTRA,strZoomInfo);
			}

			if ( m_bShowInfo || m_bLoadingRawPicture)
			{
				string strFileInfo, strSlideInfo;
				string strFileName=System.IO.Path.GetFileName(m_strBackgroundSlide);
				if (m_bTrueSizeTexture)
					strFileInfo=String.Format("{0} ({1}x{2}) ", strFileName, m_fWidthBackGround-2, m_fHeightBackGround-2);
				else
					strFileInfo=String.Format("{0}", strFileName);
        
				if (m_bLoadingRawPicture)
				{
					strFileInfo=String.Format("{0}", GUILocalizeStrings.Get(13012));
					m_bShowZoomInfo=false;
				}


				GUIControl.SetControlLabel(GetID, LABEL_ROW1,strFileInfo);
				strSlideInfo=String.Format("{0}/{1}", 1+m_iCurrentSlide ,m_slides.Count);
				GUIControl.SetControlLabel(GetID, LABEL_ROW2,strSlideInfo);

				if (!m_bShowZoomInfo)
				{
					GUIControl.SetControlLabel(GetID, LABEL_ROW2_EXTRA,"");
				}
			}
			base.Render(timePassed);
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

			iRandom = randomizer.Next(2);
			switch (iRandom)
			{
				default:
				case 0: 
					iEffect = 1; // Zoom
					break;
				case 1: 
					iEffect = 2; // Pan
					break;
			}     
			return iEffect;
		}


		// open from left->right
		bool RenderMethod1()
		{
			bool bResult=false;

			float x,y,width,height;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			float iStep= width/m_iSlideShowTransistionFrames;
			if (0==iStep) iStep=1;
			float iExpandWidth=m_dwFrameCounter*iStep;
			if (iExpandWidth >= width)
			{
				iExpandWidth=width;
				bResult=true;
			}
			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent, x, y, iExpandWidth, height, m_fWidthCurrent, m_fHeightCurrent,0,0,false);
			return bResult;
		}

		// move into the screen from left->right
		bool RenderMethod2()
		{
			bool bResult=false;
			float x,y,width,height;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			float iStep= width/m_iSlideShowTransistionFrames;
			if (0==iStep) iStep=1;
			float iPosX = m_dwFrameCounter*iStep - (int)width;
			if (iPosX >=x)
			{
				iPosX=x;
				bResult=true;
			}
			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent, iPosX, y, width, height, m_fWidthCurrent, m_fHeightCurrent,0,0,false);
			return bResult;
		}

		// move into the screen from right->left
		bool RenderMethod3()
		{
			bool bResult=false;
			float x,y,width,height;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			float iStep= width/m_iSlideShowTransistionFrames;
			if (0==iStep) iStep=1;
			float posx = x + width - m_dwFrameCounter*iStep ;
			if (posx <=x )
			{
				posx = x;
				bResult=true;
			}
			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,posx,y,width,height,m_fWidthCurrent,m_fHeightCurrent,0,0,false);
			return bResult;
		}

		// move into the screen from up->bottom
		bool RenderMethod4()
		{
			bool bResult=false;
			float x,y,width,height;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			float iStep= height/m_iSlideShowTransistionFrames;
			if (0==iStep) iStep=1;
			float posy = m_dwFrameCounter*iStep - height;
			if (posy >= y)
			{
				posy = y;
				bResult=true;
			}
			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,x,posy,width,height,m_fWidthCurrent,m_fHeightCurrent,0,0,false);
			return bResult;
		}

		// move into the screen from bottom->top
		bool RenderMethod5()
		{
			bool bResult=false;
			float x,y,width,height;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			float iStep= height/m_iSlideShowTransistionFrames;
			if (0==iStep) iStep=1;
			float posy = y+height-m_dwFrameCounter*iStep ;
			if (posy <=y)
			{
				posy=y;
				bResult=true;
			}
			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,x,posy,width,height,m_fWidthCurrent,m_fHeightCurrent,0,0,false);
			return bResult;
		}


		// open from up->bottom
		bool RenderMethod6()
		{
			bool bResult=false;
			float x,y,width,height;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			float iStep= height/m_iSlideShowTransistionFrames;
			if (0==iStep) iStep=1;
			float newheight=m_dwFrameCounter*iStep;
			if (newheight >= height)
			{
				newheight=height;
				bResult=true;
			}
			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,x,y,width,newheight,m_fWidthCurrent,m_fHeightCurrent,0,0,false);
			return bResult;
		}

		// slide from left<-right
		bool RenderMethod7()
		{
			bool bResult=false;
			float x,y,width,height;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			float iStep= width/m_iSlideShowTransistionFrames;
			if (0==iStep) iStep=1;
			float newwidth=m_dwFrameCounter*iStep;
			if (newwidth >=width)
			{
				newwidth=width;
				bResult=true;
			}

			//right align the texture
			float posx=x + width-newwidth;
			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent, posx, y, newwidth, height, m_fWidthCurrent, m_fHeightCurrent, 0, 0, false);
			return bResult;
		}


		// slide from down->up
		bool RenderMethod8()
		{
			bool bResult=false;
			float x,y,width,height;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			float iStep= height/m_iSlideShowTransistionFrames;
			if (0==iStep) iStep=1;
			float newheight=m_dwFrameCounter*iStep;
			if (newheight >=height)
			{
				newheight=height;
				bResult=true;
			}

			//bottom align the texture
			float posy=y + height-newheight;
			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,x,posy,width,newheight,m_fWidthCurrent,m_fHeightCurrent,0,0,false);
			return bResult;
		}

		// grow from middle
		bool RenderMethod9()
		{
			bool bResult=false;
			float x,y,width,height;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			float iStepX= width/m_iSlideShowTransistionFrames;
			float iStepY= height/m_iSlideShowTransistionFrames;
			if (0==iStepX) iStepX=1;
			if (0==iStepY) iStepY=1;
			float newheight=m_dwFrameCounter*iStepY;
			float newwidth=m_dwFrameCounter*iStepX;
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
			float posx = x + (width - newwidth)/2;
			float posy = y + (height - newheight)/2;
      
			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent,posx,posy,newwidth,newheight,m_fWidthCurrent, m_fHeightCurrent,0,0,false);
			return bResult;
		}

		// fade in
		bool RenderMethod10()
		{
			bool bResult=false;

			float x,y,width,height;
			GetOutputRect(m_fWidthBackGround, m_fHeightBackGround, m_fZoomFactorBackGround, out x, out y, out width, out height);
			if (m_fZoomTopBackGround+m_fZoomHeight > m_fHeightBackGround) m_fZoomHeight = m_fHeightBackGround-m_fZoomTopBackGround;
			if (m_fZoomLeftBackGround+m_fZoomWidth > m_fWidthBackGround) m_fZoomWidth = m_fWidthBackGround-m_fZoomLeftBackGround;

			float iStep= 0xff/m_iSlideShowTransistionFrames;
			if (m_bKenBurns) iStep=0xff/KENBURNS_XFADE_FRAMES;

			if (0==iStep) iStep=1;
			int iAlpha=(int)(m_dwFrameCounter*iStep);
			if (iAlpha>= 0xff)
			{
				iAlpha=0xff;
				bResult=true;
			}
			//render background first
			int lColorDiffuse=(0xff-iAlpha);
			lColorDiffuse<<=24;
			lColorDiffuse |= 0xffffff;

			MediaPortal.Util.Picture.RenderImage(ref m_pTextureBackGround, x, y, width, height, m_fZoomWidth, m_fZoomHeight, m_fZoomLeftBackGround, m_fZoomTopBackGround, lColorDiffuse);

			//next render new image
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent, out x, out y, out width, out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			lColorDiffuse=(iAlpha);
			lColorDiffuse<<=24;
			lColorDiffuse |= 0xffffff;

			MediaPortal.Util.Picture.RenderImage(ref m_pTextureCurrent, x, y, width, height, m_fZoomWidth, m_fZoomHeight, m_fZoomLeftCurrent, m_fZoomTopCurrent,lColorDiffuse);
			return bResult;
		}

		float CalculateBestZoom(float fWidth, float fHeight)
		{
			float fZoom;
			// Default picutes is zoom best fit (max width or max height)
			float fPixelRatio = GUIGraphicsContext.PixelRatio;
			float ZoomFactorX = (float)(GUIGraphicsContext.OverScanWidth*fPixelRatio)/fWidth;
			float ZoomFactorY = (float)GUIGraphicsContext.OverScanHeight/fHeight;

			// Get minimal zoom level (1.0==100%)
			fZoom = ZoomFactorX;//-ZoomFactorY+1.0f;
			m_bLandscape = true;
			if (ZoomFactorY < ZoomFactorX)
			{
				fZoom = ZoomFactorY;//-ZoomFactorX+1.0f;
				m_bLandscape = false;
			}

			m_bFullScreen=false;
			if ((ZoomFactorY < KENBURNS_ZOOM_FACTOR_FS) && (ZoomFactorX < KENBURNS_ZOOM_FACTOR_FS))
				m_bFullScreen=true;

			// Fit to screen default zoom factor
			m_fDefaultZoomFactor = fZoom;

			// Zoom 100%..150%
			if (fZoom < 1.00f)
				fZoom = 1.00f;
			if (fZoom > KENBURNS_MAXZOOM)
				fZoom = KENBURNS_MAXZOOM;

			//return fZoom;
			return 1.0f;
		}

		/// <summary>
		/// Ken Burn effects
		/// </summary>
		/// <param name="iEffect"></param>
		/// <returns></returns>
		bool KenBurns(int iEffect, bool bReset)
		{
			bool bEnd=false;
			int iNrOfFramesPerEffect = m_iKenBurnTransistionSpeed*30;
    
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
					bEnd = KenBurnsRandomZoom(m_iKenBurnsState, m_iFrameNr, iNrOfFramesPerEffect, bReset);
					break;

				case 2: 
					bEnd = KenBurnsRandomPan(m_iKenBurnsState, m_iFrameNr, iNrOfFramesPerEffect, bReset);
					break;

			}

			// Check new rectangle
			if (m_fZoomTopBackGround > (m_fHeightBackGround - m_fZoomHeight)) m_fZoomTopBackGround = (m_fHeightBackGround - m_fZoomHeight);
			if (m_fZoomLeftBackGround > (m_fWidthBackGround - m_fZoomWidth)) m_fZoomLeftBackGround = (m_fWidthBackGround - m_fZoomWidth);
			if (m_fZoomTopBackGround < 0) m_fZoomTopBackGround = 0;
			if (m_fZoomLeftBackGround < 0) m_fZoomLeftBackGround = 0;

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

		bool KenBurnsRandomZoom(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
		{
			int iRandom;
			bool bEnd = false;
			if (bReset)
			{
				iRandom = randomizer.Next(3);
				switch(iRandom)
				{
					case 0:
						if (m_bLandscape) 
							m_iZoomTypeCurrent = 8; // from left
						else
							m_iZoomTypeCurrent = 2; // from top
						break;

					case 1:
						if (m_bLandscape) 
							m_iZoomTypeCurrent = 4; // from right
						else
							m_iZoomTypeCurrent = 6; // from bottom
						break;

					default:
					case 2:
						m_iZoomTypeCurrent = 0; // centered
						break;
				}

				// Init zoom        
				if (m_bFullScreen) 
					m_fEndZoomFactor = m_fBestZoomFactorCurrent * KENBURNS_ZOOM_FACTOR_FS;
				else
					m_fEndZoomFactor = m_fBestZoomFactorCurrent * KENBURNS_ZOOM_FACTOR;

				m_fStartZoomFactor = m_fBestZoomFactorCurrent;
				m_fZoomChange = (m_fEndZoomFactor - m_fStartZoomFactor)/iNrOfFramesPerEffect;            
				m_fZoomFactorCurrent = m_fStartZoomFactor;        
			}
			else
			{
				switch (iState)
				{
					case 0: // Zoom in
						float m_fZoomFactor = m_fStartZoomFactor + m_fZoomChange * iFrameNr;
						ZoomBackGround(m_fZoomFactor);
						break;

					default:
						bEnd = true;
						break;
				}
			}

			return bEnd;
		}



		bool KenBurnsRandomPan(int iState, int iFrameNr, int iNrOfFramesPerEffect, bool bReset)
		{
			// For Landscape picutres zoomstart BestWidth than Pan 
			int iRandom;
			bool bEnd = false;
			if (bReset)
			{
				// find start and end points (8 possible points around the rectangle)
				iRandom = randomizer.Next(14);
				if (m_bLandscape)
				{
					switch(iRandom)
					{
						default:
						case 0:
							iStartPoint = 1;
							iEndPoint = 4;
							break;
						case 1:
							iStartPoint = 1;
							iEndPoint = 5;
							break;
						case 2:
							iStartPoint = 8;
							iEndPoint = 3;
							break;
						case 3:
							iStartPoint = 8;
							iEndPoint = 4;
							break;
						case 4:
							iStartPoint = 8;
							iEndPoint = 5;
							break;
						case 5:
							iStartPoint = 7;
							iEndPoint = 4;
							break;
						case 6:
							iStartPoint = 7;
							iEndPoint = 3;
							break;
						case 7:
							iStartPoint = 5;
							iEndPoint = 8;
							break;
						case 8:
							iStartPoint = 5;
							iEndPoint = 1;
							break;
						case 9:
							iStartPoint = 4;
							iEndPoint = 7;
							break;
						case 10:
							iStartPoint = 4;
							iEndPoint = 8;
							break;
						case 11:
							iStartPoint = 4;
							iEndPoint = 1;
							break;
						case 12:
							iStartPoint = 3;
							iEndPoint = 7;
							break;
						case 13:
							iStartPoint = 3;
							iEndPoint = 8;
							break;
					}
				}
				else
				{
					// Portrait
					switch(iRandom)
					{
						default:
						case 0:
							iStartPoint = 1;
							iEndPoint = 6;
							break;
						case 1:
							iStartPoint = 1;
							iEndPoint = 5;
							break;
						case 2:
							iStartPoint = 2;
							iEndPoint = 7;
							break;
						case 3:
							iStartPoint = 2;
							iEndPoint = 6;
							break;
						case 4:
							iStartPoint = 2;
							iEndPoint = 5;
							break;
						case 5:
							iStartPoint = 3;
							iEndPoint = 7;
							break;
						case 6:
							iStartPoint = 3;
							iEndPoint = 6;
							break;
						case 7:
							iStartPoint = 5;
							iEndPoint = 2;
							break;
						case 8:
							iStartPoint = 5;
							iEndPoint = 1;
							break;
						case 9:
							iStartPoint = 6;
							iEndPoint = 3;
							break;
						case 10:
							iStartPoint = 6;
							iEndPoint = 2;
							break;
						case 11:
							iStartPoint = 6;
							iEndPoint = 1;
							break;
						case 12:
							iStartPoint = 7;
							iEndPoint = 3;
							break;
						case 13:
							iStartPoint = 7;
							iEndPoint = 2;
							break;
					}        
				}

				// Init 120% top center fixed
				if (m_bFullScreen) 
					m_fZoomFactorCurrent = m_fBestZoomFactorCurrent * KENBURNS_ZOOM_FACTOR_FS;
				else
					m_fZoomFactorCurrent = m_fBestZoomFactorCurrent * KENBURNS_ZOOM_FACTOR;

				m_iZoomTypeCurrent = iStartPoint;
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
									iDestY = (float)m_fHeightBackGround/2;
									iDestX = (float)m_fZoomWidth/2;
									break;
								case 4:
									iDestY = (float)m_fHeightBackGround/2;
									iDestX = (float)m_fWidthBackGround - (float)m_fZoomWidth/2;
									break;
								case 2:
									iDestY = (float)m_fZoomHeight/2;
									iDestX = (float)m_fWidthBackGround/2;
									break;
								case 6:
									iDestY = (float)m_fHeightBackGround - (float)m_fZoomHeight/2;
									iDestX = (float)m_fWidthBackGround/2;
									break;
								case 1:
									iDestY = (float)m_fZoomHeight/2;
									iDestX = (float)m_fZoomWidth/2;
									break;
								case 3:
									iDestY = (float)m_fZoomHeight/2;
									iDestX = (float)m_fWidthBackGround - (float)m_fZoomWidth/2;
									break;
								case 7:
									iDestY = (float)m_fHeightBackGround - (float)m_fZoomHeight/2;
									iDestX = (float)m_fZoomWidth/2;
									break;
								case 5:
									iDestY = (float)m_fHeightBackGround - (float)m_fZoomHeight/2;
									iDestX = (float)m_fWidthBackGround - (float)m_fZoomWidth/2;
									break;
							}   

							m_fPanYChange = (iDestY - (m_fZoomTopBackGround+(float)m_fZoomHeight/2))/iNrOfFramesPerEffect; // Travel Y;
							m_fPanXChange = (iDestX - (m_fZoomLeftBackGround+(float)m_fZoomWidth/2))/iNrOfFramesPerEffect; // Travel Y;
						}

						PanBackGround(m_fPanXChange, m_fPanYChange);
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

		bool PanBackGround(float fPanX, float fPanY)
		{
			if ((fPanX == 0.0f) && (fPanY == 0.0f)) return false;
      
			m_fZoomLeftBackGround += fPanX;
			m_fZoomTopBackGround += fPanY;
            
			if (m_fZoomTopBackGround < 0) return false;
			if (m_fZoomLeftBackGround < 0) return false;
			if (m_fZoomTopBackGround > (m_fHeightBackGround - m_fZoomHeight)) return false;
			if (m_fZoomLeftBackGround > (m_fWidthBackGround - m_fZoomWidth)) return false;

			return true;
		}

		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_MOUSE_CLICK:
					int x=(int)action.fAmount1;

					if (!m_bPictureZoomed)
					{
						// Divide screen into three sections (previous / pause / next)
						if (x < (GUIGraphicsContext.OverScanWidth/3))
							ShowPrevious();
						else if (x > (GUIGraphicsContext.OverScanWidth/3)*2)
							ShowNext();
						else					
							if (m_bSlideShow) m_bPause=!m_bPause;
					}
					else
					{
						m_fUserZoomLevel=1.0f;
						ZoomBackGround(m_fDefaultZoomFactor);
						m_bPause = false;
					}
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;

				case Action.ActionType.ACTION_PREVIOUS_MENU:
					ShowPreviousWindow();
					break;
				
				case Action.ActionType.ACTION_DELETE_ITEM:
					OnDelete();				
					break;

				case Action.ActionType.ACTION_PREV_PICTURE:
					if (!m_bPictureZoomed) 
					{
						ShowPrevious();
					}
					else
					{
						// Move picture
						m_fZoomLeftBackGround-=25;
						if (m_fZoomLeftBackGround < 0) m_fZoomLeftBackGround = 0;
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
						// Move picture
						m_fZoomLeftBackGround+=25;
						if (m_fZoomLeftBackGround > (int)m_fWidthBackGround - m_fZoomWidth) m_fZoomLeftBackGround = (m_fWidthBackGround - m_fZoomWidth);
						m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					}
					break;

				case Action.ActionType.ACTION_MOVE_LEFT:
					m_fZoomLeftBackGround-=25;
					if (m_fZoomLeftBackGround < 0) m_fZoomLeftBackGround = 0;
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;

				case Action.ActionType.ACTION_MOVE_RIGHT:
					m_fZoomLeftBackGround+=25;
					if (m_fZoomLeftBackGround > (int)m_fWidthBackGround - m_fZoomWidth) m_fZoomLeftBackGround = (m_fWidthBackGround - m_fZoomWidth);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;

				case Action.ActionType.ACTION_MOVE_DOWN:
					if (m_bPictureZoomed) m_fZoomTopBackGround+=25;
					if (m_fZoomTopBackGround > (int)m_fHeightBackGround - m_fZoomHeight) m_fZoomTopBackGround = (m_fHeightBackGround - m_fZoomHeight);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;

				case Action.ActionType.ACTION_MOVE_UP:
					if (m_bPictureZoomed) m_fZoomTopBackGround-=25;
					if (m_fZoomTopBackGround < 0) m_fZoomTopBackGround = 0;
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;

				case Action.ActionType.ACTION_SHOW_INFO:
					if (m_bShowInfo)
					{ 
						m_bShowInfo = false;
						m_bShowZoomInfo = false;
					}
					else
					{
						if (m_bPictureZoomed) m_bShowZoomInfo = true;
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
							m_fUserZoomLevel=1.0f;
							ZoomBackGround(m_fDefaultZoomFactor);
							m_bPause = false;
						}
						else
							m_bPause=!m_bPause;
					}
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;

				case Action.ActionType.ACTION_ZOOM_OUT:
					m_fUserZoomLevel -= 0.25f;
					if (m_fUserZoomLevel<1) m_fUserZoomLevel=1.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;

				case Action.ActionType.ACTION_ZOOM_IN:
					m_fUserZoomLevel += 0.25f;
					if (m_fUserZoomLevel>20.0f) m_fUserZoomLevel=20.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;

				case Action.ActionType.ACTION_ROTATE_PICTURE:
					DoRotate();          
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;

				case Action.ActionType.ACTION_ZOOM_LEVEL_NORMAL:
					m_fUserZoomLevel = 1.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ZOOM_LEVEL_1:
					m_fUserZoomLevel = 2.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ZOOM_LEVEL_2:
					m_fUserZoomLevel = 3.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ZOOM_LEVEL_3:
					m_fUserZoomLevel = 4.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ZOOM_LEVEL_4:
					m_fUserZoomLevel = 5.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ZOOM_LEVEL_5:
					m_fUserZoomLevel = 6.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ZOOM_LEVEL_6:
					m_fUserZoomLevel = 7.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ZOOM_LEVEL_7:
					m_fUserZoomLevel = 8.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ZOOM_LEVEL_8:
					m_fUserZoomLevel = 9.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ZOOM_LEVEL_9:
					m_fUserZoomLevel = 10.0f;
					ZoomBackGround(m_fDefaultZoomFactor*m_fUserZoomLevel);
					m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
					break;
				case Action.ActionType.ACTION_ANALOG_MOVE:
					float fX=2*action.fAmount1;
					float fY=2*action.fAmount2;
					if (fX!=0.0f||fY!=0.0f)
					{
						if (m_bPictureZoomed)
						{
							m_fZoomLeftBackGround += (int)fX;
							m_fZoomTopBackGround -= (int)fY;
							if (m_fZoomTopBackGround < 0) m_fZoomTopBackGround = 0;
							if (m_fZoomLeftBackGround < 0) m_fZoomLeftBackGround = 0;
							if (m_fZoomTopBackGround > m_fHeightBackGround - m_fZoomHeight) m_fZoomTopBackGround = (m_fHeightBackGround - m_fZoomHeight);
							if (m_fZoomLeftBackGround > m_fWidthBackGround - m_fZoomWidth) m_fZoomLeftBackGround = (m_fWidthBackGround - m_fZoomWidth);
            			
							m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
						}
					}
					break;

				case Action.ActionType.ACTION_CONTEXT_MENU:
					ShowContextMenu();
					break;
			} 
		}

		void ShowContextMenu()
		{
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
			dlg.Reset();
			dlg.SetHeading(924); // menu

			dlg.AddLocalizedString(117); //delete
			dlg.AddLocalizedString(735); //rotate
			if (!m_bSlideShow) dlg.AddLocalizedString(108); //start slideshow    
			dlg.AddLocalizedString(940); //properties
			dlg.AddLocalizedString(970); //Exit

			dlg.DoModal( GetID);
			if (dlg.SelectedId==-1) return;
			switch (dlg.SelectedId)
			{
				case 117: // Delete
					OnDelete();
					break;

				case 735: // Rotate					
					DoRotate();
					break;
				
				case 108: // Start slideshow
					StartSlideShow();
					break;

				case 940: // Properties
					OnShowInfo();
					break;

				case 970:
					ShowPreviousWindow();
					break;
			}
		}

		void OnDelete()
		{
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
			if (Utils.FileDelete(m_strBackgroundSlide) == true)
			{
				if (m_iCurrentSlide < m_slides.Count) m_slides.RemoveAt(m_iCurrentSlide);

				m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
				m_iLastShownSlide=-1;
				m_bUpdate=true;			
			}
		}

		void OnShowInfo()
		{
			GUIDialogExif exifDialog = (GUIDialogExif)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_EXIF);
			exifDialog.FileName=m_strBackgroundSlide;
			exifDialog.DoModal(GetID);
		}


		bool RenderPause()
		{
			dwCounter++;
			if (dwCounter > 25)
			{
				dwCounter=0;
			}
			if ((!m_bPause && !m_bShowInfo && !m_bShowZoomInfo && !m_bPictureZoomed) || m_bShowZoomInfo || m_bShowInfo) return false;

			if (dwCounter <13) return false;
			GUIFont pFont=GUIFontManager.GetFont("font13");
			if (pFont!=null)
			{
				string szText=GUILocalizeStrings.Get(112);
				pFont.DrawShadowText(500.0f,60.0f,0xffffffff,szText,GUIControl.Alignment.ALIGN_LEFT,2,2,0xff000000);
			}
			return true;
		}

		void DoRotate()
		{
			m_iRotate++;
			if (m_iRotate>=4)
			{
				m_iRotate=0;
			}

			using (PictureDatabase dbs = new PictureDatabase())
			{
				dbs.SetRotation(m_strBackgroundSlide,m_iRotate);
			}

			if (null!=m_pTextureCurrent)
			{
				m_pTextureCurrent.Dispose();
				m_pTextureCurrent = null;
			}
			if (m_strBackgroundSlide.Length==0) return;
	    

			int iMaxWidth=GUIGraphicsContext.OverScanWidth;
			int iMaxHeight=GUIGraphicsContext.OverScanHeight;
			int X, Y;
			m_pTextureCurrent=MediaPortal.Util.Picture.Load(m_strBackgroundSlide, m_iRotate,iMaxWidth, iMaxHeight,  true,true, out X,out Y);
			m_fWidthCurrent = X;
			m_fHeightCurrent = Y;
			m_strCurrentSlide=m_strBackgroundSlide;
			m_fZoomFactorBackGround=m_fDefaultZoomFactor;
			m_iKenBurnsEffect=0;
			m_fUserZoomLevel=1.0f;
			m_bPictureZoomed=false;
			m_fZoomLeftBackGround=0;
			m_fZoomTopBackGround=0;
			m_lSlideTime=(int)(DateTime.Now.Ticks/10000);
			m_dwFrameCounter=0;
			m_iTransistionMethod=9;

			DeleteThumb(m_strBackgroundSlide);
		}

		void GetOutputRect(float iSourceWidth, float iSourceHeight, float fZoomLevel, out float x,out float y,out float width,out float height)
		{
			// calculate aspect ratio correction factor
			float iOffsetX1 = GUIGraphicsContext.OverScanLeft;
			float iOffsetY1 = GUIGraphicsContext.OverScanTop;
			float iScreenWidth = GUIGraphicsContext.OverScanWidth;
			float iScreenHeight = GUIGraphicsContext.OverScanHeight;
			float fPixelRatio = GUIGraphicsContext.PixelRatio;

			float fSourceFrameAR = ((float)iSourceWidth)/((float)iSourceHeight);
			float fOutputFrameAR = fSourceFrameAR / fPixelRatio;
                 
			width=(iSourceWidth/fPixelRatio)*fZoomLevel;
			height=iSourceHeight*fZoomLevel;

			m_fZoomWidth = iSourceWidth;
			m_fZoomHeight = iSourceHeight;

			// check org rectangle 
			if (width > iScreenWidth)
			{
				width = iScreenWidth;
				m_fZoomWidth = (width*fPixelRatio) / fZoomLevel;
			}

			if (height > iScreenHeight)
			{
				height = iScreenHeight;
				m_fZoomHeight = height / fZoomLevel;
			}

			if (m_fZoomHeight > iSourceHeight)
			{
				m_fZoomHeight = iSourceHeight;
				m_fZoomWidth = m_fZoomHeight * fSourceFrameAR;
			}

			if (m_fZoomWidth > iSourceWidth)
			{
				m_fZoomWidth = iSourceWidth;
				m_fZoomHeight = m_fZoomWidth / fSourceFrameAR;
			}

			x = (iScreenWidth - width)/2 + iOffsetX1;
			y = (iScreenHeight - height)/2 + iOffsetY1;
		}
    
		void ZoomCurrent(float fZoom)
		{
			if (fZoom > MAX_ZOOM_FACTOR || fZoom < 0.0f)
				return;
      
			// Start and End point positions along the picture rectangle
			// Point zoom in/out only works if the selected Point is at the border
			// example:  "m_dwWidthBackGround == m_iZoomLeft + m_fZoomWidth"  and zooming to the left (iZoomType=4)
			float middlex = m_fWidthCurrent/2;
			float middley = m_fHeightCurrent/2;
			float xend = m_fWidthCurrent;
			float yend = m_fHeightCurrent;
      
			float x,y,width,height;
			m_fZoomFactorCurrent = fZoom;
			GetOutputRect(m_fWidthCurrent, m_fHeightCurrent, m_fZoomFactorCurrent,out x,out y,out width,out height);
			if (m_fZoomTopCurrent+m_fZoomHeight > m_fHeightCurrent) m_fZoomHeight = m_fHeightCurrent-m_fZoomTopCurrent;
			if (m_fZoomLeftCurrent+m_fZoomWidth > m_fWidthCurrent) m_fZoomWidth = m_fWidthCurrent-m_fZoomLeftCurrent;

			switch(m_iZoomTypeCurrent)
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
					m_fZoomLeftCurrent = middlex - m_fZoomWidth*0.5f;
					m_fZoomTopCurrent = middley - m_fZoomHeight*0.5f;
					break;
				case 2: // Width centered, Top unchanged
					m_fZoomLeftCurrent = middlex - m_fZoomWidth*0.5f;
					break;
				case 8: // Heigth centered, Left unchanged
					m_fZoomTopCurrent = middley - m_fZoomHeight*0.5f;
					break;
				case 6: // Widht centered, Bottom unchanged
					m_fZoomLeftCurrent = middlex - m_fZoomWidth*0.5f;
					m_fZoomTopCurrent = yend - m_fZoomHeight;
					break;
				case 4: // Height centered, Right unchanged
					m_fZoomTopCurrent = middley - m_fZoomHeight*0.5f;
					m_fZoomLeftCurrent = xend - m_fZoomWidth;
					break;
				case 1: // Top Left unchanged
					break;
				case 3: // Top Right unchanged
					m_fZoomLeftCurrent = xend - m_fZoomWidth;
					break;
				case 7: // Bottom Left unchanged          
					m_fZoomTopCurrent = yend - m_fZoomHeight;
					break;
				case 5: // Bottom Right unchanged
					m_fZoomTopCurrent = yend - m_fZoomHeight;
					m_fZoomLeftCurrent = xend - m_fZoomWidth;
					break;
			}
			if (m_fZoomLeftCurrent > m_fWidthCurrent-m_fZoomWidth) m_fZoomLeftCurrent = (m_fWidthCurrent-m_fZoomWidth);
			if (m_fZoomTopCurrent > m_fHeightCurrent-m_fZoomHeight) m_fZoomTopCurrent = (m_fHeightCurrent-m_fZoomHeight);
			if (m_fZoomLeftCurrent < 0) m_fZoomLeftCurrent = 0;
			if (m_fZoomTopCurrent < 0) m_fZoomTopCurrent = 0; 
		}

		void ZoomBackGround(float fZoom)
		{
			if (fZoom > MAX_ZOOM_FACTOR || fZoom < 0.0f)
				return;

			m_bPictureZoomed = (m_fUserZoomLevel!=1.0f);
			// Load raw picture when zooming
			if (!m_bTrueSizeTexture && m_bPictureZoomed)
			{        
				m_bShowZoomInfo=true;
				m_bLoadingRawPicture=true;

				// Update window
				GUIWindowManager.Process();

				// load picture
				m_pTextureBackGround=GetSlide(true, out m_fWidthBackGround,out m_fHeightBackGround, out m_strCurrentSlide);              
				m_bLoadingRawPicture=false;
				fZoom = m_fDefaultZoomFactor * m_fUserZoomLevel;
			}

			// Start and End point positions along the picture rectangle
			// Point zoom in/out only works if the selected Point is at the border
			// example:  "m_dwWidthBackGround == m_iZoomLeft + m_fZoomWidth"  and zooming to the left (iZoomType=4)
			float middlex = m_fZoomLeftBackGround + m_fZoomWidth/2;
			float middley = m_fZoomTopBackGround + m_fZoomHeight/2;
			float xend = m_fWidthBackGround;
			float yend = m_fHeightBackGround;

			m_fZoomFactorBackGround = fZoom;     

			float x,y,width,height;
			GetOutputRect(m_fWidthBackGround, m_fHeightBackGround, m_fZoomFactorBackGround,out x,out y,out width,out height);

			if (m_bPictureZoomed) m_iZoomTypeBackGround = 0;
			switch(m_iZoomTypeBackGround)
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
					m_fZoomLeftBackGround = middlex - m_fZoomWidth*0.5f;
					m_fZoomTopBackGround = middley - m_fZoomHeight*0.5f;
					break;
				case 2: // Width centered, Top unchanged
					m_fZoomLeftBackGround = middlex - m_fZoomWidth*0.5f;
					break;
				case 8: // Heigth centered, Left unchanged
					m_fZoomTopBackGround = middley - m_fZoomHeight*0.5f;
					break;
				case 6: // Widht centered, Bottom unchanged
					m_fZoomLeftBackGround = middlex - m_fZoomWidth*0.5f;
					m_fZoomTopBackGround = yend - m_fZoomHeight;
					break;
				case 4: // Height centered, Right unchanged
					m_fZoomTopBackGround = middley - m_fZoomHeight*0.5f;
					m_fZoomLeftBackGround = xend - m_fZoomWidth;
					break;
				case 1: // Top Left unchanged
					break;
				case 3: // Top Right unchanged
					m_fZoomLeftBackGround = xend - m_fZoomWidth;
					break;
				case 7: // Bottom Left unchanged          
					m_fZoomTopBackGround = yend - m_fZoomHeight;
					break;
				case 5: // Bottom Right unchanged
					m_fZoomTopBackGround = yend - m_fZoomHeight;
					m_fZoomLeftBackGround = xend - m_fZoomWidth;
					break;
			}
			if (m_fZoomLeftBackGround > m_fWidthBackGround-m_fZoomWidth) m_fZoomLeftBackGround = (m_fWidthBackGround-m_fZoomWidth);
			if (m_fZoomTopBackGround > m_fHeightBackGround-m_fZoomHeight) m_fZoomTopBackGround = (m_fHeightBackGround-m_fZoomHeight);
			if (m_fZoomLeftBackGround < 0) m_fZoomLeftBackGround = 0;
			if (m_fZoomTopBackGround < 0) m_fZoomTopBackGround = 0;      
		}

		void LoadSettings()
		{
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_iSpeed=xmlreader.GetValueAsInt("pictures","speed",3);
				m_iSlideShowTransistionFrames=xmlreader.GetValueAsInt("pictures","transition",20);
				m_iKenBurnTransistionSpeed=xmlreader.GetValueAsInt("pictures","kenburnsspeed",20);
				m_bKenBurns=xmlreader.GetValueAsBool("pictures","kenburns", false);
				m_bUseRandomTransistions=xmlreader.GetValueAsBool("pictures","random", true);
				m_bAutoShuffle=xmlreader.GetValueAsBool("pictures","autoShuffle", false);
				m_bAutoRepeat=xmlreader.GetValueAsBool("pictures","autoRepeat", false);
//				_isBackgroundMusicEnabled = xmlreader.GetValueAsBool("pictures", "backgroundmusic", false);
				_isBackgroundMusicEnabled = true;

				if(_isBackgroundMusicEnabled)
				{
					string extensions = xmlreader.GetValueAsString("music", "extensions", ".mp3,.pls,.wpl");
					_musicFileExtensions = extensions.Split(',');
				}
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

		void StartBackgroundMusic(string path)
		{
			if(g_Player.IsMusic || g_Player.IsRadio || g_Player.IsTV || g_Player.IsVideo)
				return;

			if(_musicFileExtensions == null)
			{
				using(MediaPortal.Profile.Xml reader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
					_musicFileExtensions = reader.GetValueAsString("music", "extensions", ".mp3,.pls,.wpl").Split(',');
			}

			foreach(string extension in _musicFileExtensions)
			{
				string filename = string.Format(@"{0}\Folder{1}", path, extension);

				if(File.Exists(filename) == false)
					continue;

				Log.Write("Filename: {0}", filename);

				try
				{
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Clear();
					PlayListPlayer.Reset();
					PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP;
					
					PlayList.PlayListItem playlistItem = new PlayList.PlayListItem();
				
					playlistItem.Type = Playlists.PlayList.PlayListItem.PlayListItemType.Audio;
					playlistItem.FileName = filename;
					PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP).Add(playlistItem);

					if(PlayListFactory.IsPlayList(filename))
					{
						PlayListPlayer.Play(filename);
					}
					else
					{
						PlayListPlayer.Play(0);
					}
					
					_isBackgroundMusicPlaying = true;
				}
				catch(Exception e)
				{
					Log.Write("GUISlideShow.StartBackgroundMusic", e.Message);
				}

				break;
			}
		}

		void ShowPreviousWindow()
		{
			if(_isBackgroundMusicPlaying)
				g_Player.Stop();

			GUIWindowManager.ShowPreviousWindow();
		}
	}
}
