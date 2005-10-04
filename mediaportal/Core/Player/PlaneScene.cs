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
using System.Drawing;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;
using DShowNET;

namespace MediaPortal.Player 
{
	/// <summary>
	/// This class will draw a video texture onscreen when using VMR9 renderless
	/// Its controlled by the allocator wrapper 
	/// Example on how to use:
	/// PlaneScene scene = new PlaneScene(GUIGraphicsContext.RenderGUI)
	/// scene.Init()
	/// ... allocate direct3d texture
	/// scene.SetSrcRect(1.0f,1.0f); //change this depending on the texture dimensions
	/// scene.SetSurface(texture); //change this depending on the texture dimensions
	/// while (playingMovie)
	/// {
	///   scene.Render(GUIGraphicsContext.DX9Device, videoTextre, videoSize) 
	/// }
	/// scene.ReleaseSurface(texture)
	/// scene.Stop();
	/// scene.DeInit();
	/// </summary>
	public class PlaneScene :IVMR9PresentCallback
	{
		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEngineRemoveTexture(int textureNo);

		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern int  FontEngineAddTexture(int hasCode,bool useAlphaBlend,void* fontTexture);
		
		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern int  FontEngineAddSurface(int hasCode,bool useAlphaBlend,void* fontTexture);
		
		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEngineDrawTexture(int textureNo,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color);

		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEnginePresentTextures();

		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEngineSetTexture(void* texture);

		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEngineDrawSurface(int fx, int fy, int nw, int nh, 
																														int dstX, int dstY, int dstWidth, int dstHeight,
																														void* surface);

		bool					  m_bStop = false;
		Surface					rTarget = null;
		IRender				  m_renderer;
		long					  m_lColorDiffuse = 0xFFFFFFFF;
		int						  m_iFrame = 0;
		bool					  m_bFadeIn = true;
		float           m_fU=1.0f;
		float           m_fV=1.0f;
		Rectangle       previousRect;
		bool            renderTexture=false;
		bool            lastOverlay=false;
		bool						isEnabled=true;
		MediaPortal.GUI.Library.Geometry.Type arType;
		System.Drawing.Rectangle							rSource, rDest;    
		MediaPortal.GUI.Library.Geometry			m_geometry = new MediaPortal.GUI.Library.Geometry();
		VMR9Util				  m_vmr9Util=null;		
		float							_fx,_fy,_nw,_nh,_uoff,_voff,_umax,_vmax;
		VertexBuffer			m_vertexBuffer;
		uint              m_surfAdr,m_texAdr;
		
		int               arVideoWidth=4;
		int								arVideoHeight=3;
		int               prevVideoWidth=0;
		int               prevVideoHeight=0;
		int               prevArVideoWidth=0;
		int               prevArVideoHeight=0;
		static            bool reentrant=false;
		bool              drawVideoAllowed=true;
		int               m_idebugstep=0;

		public PlaneScene(IRender renderer, VMR9Util util)
		{
			Log.Write("PlaneScene: ctor()");
			
			m_surfAdr=0;
			m_texAdr=0;
			m_vmr9Util=util;
			m_renderer = renderer;
			m_vertexBuffer =new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured),
				4, GUIGraphicsContext.DX9Device, 
				0, CustomVertex.TransformedColoredTextured.Format, 
				Pool.Managed);
		}

		/// <summary>
		/// Stop VMR9 rendering
		/// this method will restore the DirectX render target since
		/// this might have been changed by the Video Mixing Renderer9
		/// </summary>
		public void Stop()
		{
			Log.Write("PlaneScene: Stop()");
			DrawVideo=false;
			m_bStop = true;
		}
		public bool DrawVideo
		{
			get { return drawVideoAllowed;}
			set { 
				drawVideoAllowed=value;
				Log.Write("PlaneScene: video draw allowed:{0}", drawVideoAllowed);
			}

		}

		/// <summary>
		/// Set the texture dimensions. Sometimes the video texture is larger then the
		/// video resolution. In this case we should copy only a part from the video texture
		/// Using this function one can set how much of the video texture should be used
		/// </summary>
		/// <param name="fU">(0-1) Specifies the width to used of the video texture</param>
		/// <param name="fV">(0-1) Specifies the height to used of the video texture</param>
		public void SetSrcRect(float fU, float fV)
		{
			previousRect = new Rectangle(0,0,0,0);
			m_fU = fU;
			m_fV = fV;
		}

		/// <summary>
		/// Returns a rectangle specifing the part of the video texture which is 
		/// shown
		/// </summary>
		public System.Drawing.Rectangle SourceRect
		{
			get { return  rSource;}
		}
		/// <summary>
		/// Returns a rectangle specifing the video window onscreen
		/// </summary>
		public System.Drawing.Rectangle DestRect
		{
			get { return  rDest;}
		}

		/// <summary>
		/// Deinitialize. Release the vertex buffer and the render target resources
		/// This function should be called at last when playing has been stopped
		/// </summary>
		public void Deinit()
		{
			//lock(this) 
			{
				Log.Write("PlaneScene: deinit()");

				if (rTarget!=null)
				{
					//VMR9 changes the directx 9 render target. Thats why we set it back to what it was
					GUIGraphicsContext.DX9Device.SetRenderTarget(0, rTarget);
					rTarget.Dispose();
					rTarget=null;
				}
				if (m_vertexBuffer!=null)
				{
					m_vertexBuffer.Dispose();
					m_vertexBuffer=null;
				}
			}
		}

		/// <summary>
		/// Initialize.
		/// This should be called before any other methods.
		/// It allocates resources needed
		/// </summary>
		/// <param name="device">Direct3d devices</param>
		public void Init() 
		{
			Log.Write("PlaneScene: init()");
			rTarget = GUIGraphicsContext.DX9Device.GetRenderTarget(0);
		}

		public bool InTv
		{
			get
			{
				int windowId=GUIWindowManager.ActiveWindow;
				if (windowId==(int)GUIWindow.Window.WINDOW_TV||
					windowId==(int)GUIWindow.Window.WINDOW_TVGUIDE||  
					windowId==(int)GUIWindow.Window.WINDOW_SEARCHTV||  
					windowId==(int)GUIWindow.Window.WINDOW_TELETEXT||  
					windowId==(int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT||  
					windowId==(int)GUIWindow.Window.WINDOW_SCHEDULER||  
					windowId==(int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES||
					windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTV||
					windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL||
					windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE ||
					windowId==(int)GUIWindow.Window.WINDOW_TV_CONFLICTS||
					windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_MAIN ||
					windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_AUTO ||
					windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS ||
					windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS_STATUS ||
					windowId== (int)GUIWindow.Window.WINDOW_TV_COMPRESS_SETTINGS ||
					windowId== (int)GUIWindow.Window.WINDOW_TV_NO_SIGNAL  ||
					windowId== (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO)
					return true;
				return false;
			}
		}
    
		/// <summary>
		/// This method calculates the rectangle on screen where the video should be presented
		/// this depends on if we are in fullscreen mode or preview mode
		/// and on the current aspect ration settings
		/// </summary>
		/// <param name="videoSize">Size of video stream</param>
		/// <returns>
		/// true : video window is visible
		/// false: video window is not visible
		/// </returns>
		public bool SetVideoWindow(Size videoSize)
		{
			try
			{
				GUIGraphicsContext.VideoSize=videoSize;
				// get the window where the video/tv should be shown
				float x  = GUIGraphicsContext.VideoWindow.X;
				float y  = GUIGraphicsContext.VideoWindow.Y;
				float nw = GUIGraphicsContext.VideoWindow.Width;
				float nh = GUIGraphicsContext.VideoWindow.Height; 

				//sanity checks
				if (nw > GUIGraphicsContext.OverScanWidth)
					nw = GUIGraphicsContext.OverScanWidth;
				if (nh > GUIGraphicsContext.OverScanHeight)
					nh = GUIGraphicsContext.OverScanHeight;

				//are we supposed to show video in fullscreen or in a preview window?
				if (GUIGraphicsContext.IsFullScreenVideo || !GUIGraphicsContext.ShowBackground)
				{
					//yes fullscreen, then use the entire screen
					x  = GUIGraphicsContext.OverScanLeft;
					y  = GUIGraphicsContext.OverScanTop;
					nw = GUIGraphicsContext.OverScanWidth;
					nh = GUIGraphicsContext.OverScanHeight;
				}
				else 
				{
					// we're in preview mode. Check if we are in the tv module

					if (!InTv)
					{
						//we are not in the my tv module
						//then check if video/tv preview window is enable
						if (!GUIGraphicsContext.Overlay) return false; //not enabled, dont show tv
					}
				}

				//sanity check
				if (nw <= 10  || nh <= 10) return false;
				if (x  < 0    || y  <   0) return false;

				//did the video window,aspect ratio change? if not
				//then we dont need to recalculate and just return the previous settings
				if (x ==previousRect.X     && y ==previousRect.Y      && 
					nw==previousRect.Width && nh==previousRect.Height &&
					GUIGraphicsContext.ARType  == arType              &&
					GUIGraphicsContext.Overlay == lastOverlay && renderTexture &&
					prevVideoWidth==videoSize.Width && prevVideoHeight==videoSize.Height &&
					prevArVideoWidth==arVideoWidth && prevArVideoHeight==arVideoHeight)
				{
					//not changed, return previous settings
					return renderTexture;
				}

				//settings (position,size,aspect ratio) changed.
				//Store these settings and start calucating the new video window
				previousRect = new Rectangle((int)x, (int)y, (int)nw, (int)nh);
				arType       = GUIGraphicsContext.ARType;
				lastOverlay  = GUIGraphicsContext.Overlay;
				prevVideoWidth=videoSize.Width;
				prevVideoHeight=videoSize.Height;
				prevArVideoWidth=arVideoWidth;
				prevArVideoHeight=arVideoHeight;
        
				//calculate the video window according to the current aspect ratio settings
				float fVideoWidth  = (float)videoSize.Width;
				float fVideoHeight = (float)videoSize.Height;
				m_geometry.ImageWidth   = (int)fVideoWidth;
				m_geometry.ImageHeight  = (int)fVideoHeight;
				m_geometry.ScreenWidth  = (int)nw;
				m_geometry.ScreenHeight = (int)nh;
				m_geometry.ARType       = GUIGraphicsContext.ARType;
				m_geometry.PixelRatio   = GUIGraphicsContext.PixelRatio;
				m_geometry.GetWindow(arVideoWidth, arVideoHeight,out rSource, out rDest);
				rDest.X += (int)x;
				rDest.Y += (int)y;

				//sanity check
				if (rDest.Width    < 10) return false;
				if (rDest.Height   < 10) return false;
				if (rSource.Width  < 10) return false;
				if (rSource.Height < 10) return false;

				Log.Write("PlaneScene: video WxH  : {0}x{1}",videoSize.Width,videoSize.Height);
				Log.Write("PlaneScene: video AR   : {0}:{1}",arVideoWidth, arVideoHeight);
				Log.Write("PlaneScene: screen WxH : {0}x{1}",nw,nh);
				Log.Write("PlaneScene: AR type    : {0}",GUIGraphicsContext.ARType);
				Log.Write("PlaneScene: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
				Log.Write("PlaneScene: src        : ({0},{1})-({2},{3})",
					rSource.X,rSource.Y, rSource.X+rSource.Width,rSource.Y+rSource.Height);
			  Log.Write("PlaneScene: dst        : ({0},{1})-({2},{3})",
					rDest.X,rDest.Y,rDest.X+rDest.Width,rDest.Y+rDest.Height);

				if (rSource.Y==0)
				{
					rSource.Y+=5;
					rSource.Height-=10;
				}

				//next calculate which part of the video texture should be copied
				//into the video window
				float uoffs = ((float)rSource.X)      / (fVideoWidth);
				float voffs = ((float)rSource.Y)      / (fVideoHeight);
				float u     = ((float)rSource.Width)  / (fVideoWidth);
				float v     = ((float)rSource.Height) / (fVideoHeight);
				
				//take in account that the texture might be larger
				//then the video size
				uoffs *= m_fU;
				u		  *= m_fU;
				voffs *= m_fV;
				v		  *= m_fV;

				//set the video window positions
				x  = (float)rDest.X;
				y  = (float)rDest.Y;
				nw = (float)rDest.Width;
				nh = (float)rDest.Height;

				_fx=x;
				_fy=y;
				_nw=nw;
				_nh=nh;
				_uoff=uoffs;
				_voff=voffs;
				_umax=u;
				_vmax=v;
											
				return true;
			}
			catch (Exception ex)
			{
				Log.Write("planescene.SetVideoWindow excpetion:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
				return false;
			}
		}

		public bool Enabled
		{
			get { return isEnabled;}
			set { 
				isEnabled=value;
				Log.Write("planescene: enabled:{0}", isEnabled);
			}
		}

		#region IVMR9Callback Members

		
		public int PresentImage(Int16 width,Int16 height,Int16 arWidth, Int16 arHeight, uint pTex)
		{
			try
			{
				m_texAdr=pTex;
				if (pTex==0)
				{
					Log.Write("PlaneScene: dispose surfaces");
					m_surfAdr=0;
					m_vmr9Util.VideoWidth=0;
					m_vmr9Util.VideoHeight=0;
					m_vmr9Util.VideoAspectRatioX=0;
					m_vmr9Util.VideoAspectRatioY=0;
					arVideoWidth=0;
					arVideoHeight=0;
					return 0;
				}
				if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING) return 0;
				if (!drawVideoAllowed  || !isEnabled)
				{
					Log.Write("planescene:frame:{0} enabled:{1} allowed:{2}", m_vmr9Util.FrameCounter,drawVideoAllowed,isEnabled);
					m_vmr9Util.FrameCounter++;
					return 0;
				}
				m_vmr9Util.FrameCounter++;
				//			Log.Write("vmr9:present image()");
				InternalPresentImage(width,height,arWidth, arHeight, false);
				//			Log.Write("vmr9:present image() done");
			}
			catch(Exception)
			{
			}
			return 0;
		}
		public int PresentSurface(Int16 width,Int16 height,Int16 arWidth, Int16 arHeight,uint pSurface)
		{
			try
			{
				m_surfAdr=pSurface;
				if (pSurface==0)
				{
					Log.Write("PlaneScene: dispose surfaces");
					m_texAdr=0;
					m_vmr9Util.VideoWidth=0;
					m_vmr9Util.VideoHeight=0;
					m_vmr9Util.VideoAspectRatioX=0;
					m_vmr9Util.VideoAspectRatioY=0;
					arVideoWidth=0;
					arVideoHeight=0;
					return 0;
				}
				if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING) return 0;
				if (!drawVideoAllowed  || !isEnabled)
				{
					Log.Write("planescene:frame:{0} enabled:{1} allowed:{2}", m_vmr9Util.FrameCounter,drawVideoAllowed,isEnabled);
					m_vmr9Util.FrameCounter++;
					return 0;
				}
				m_vmr9Util.FrameCounter++;
				InternalPresentSurface(width,height,arWidth, arHeight, false);
			}
			catch(Exception)
			{
			}
			return 0;
		}

		private void InternalPresentImage(int width,int height,int arWidth, int arHeight, bool isRepaint)
		{
			if (reentrant)
			{
				Log.WriteFile(Log.LogType.Log,true,"PlaneScene: re-entrancy in presentimage");
				return;
			}
			try
			{
				m_idebugstep=0;
				reentrant=true;
				GUIGraphicsContext.InVmr9Render=true;
				if (width>0 && height>0)
				{
					m_vmr9Util.VideoWidth=width;
					m_vmr9Util.VideoHeight=height;
					m_vmr9Util.VideoAspectRatioX=arWidth;
					m_vmr9Util.VideoAspectRatioY=arHeight;
					arVideoWidth=arWidth;
					arVideoHeight=arHeight;
				}
				
				//if we're stopping then just return
				float timePassed=GUIGraphicsContext.TimePassed;
				if (m_bStop) return;
				//sanity checks
				if (GUIGraphicsContext.DX9Device==null)  return;
				if (GUIGraphicsContext.DX9Device.Disposed) return;
				if (GUIWindowManager.IsSwitchingToNewWindow) return; //dont present video during window transitions
				
				m_idebugstep=1;
				if (rTarget!=null) 
				{
				  if (!rTarget.Disposed)
				    GUIGraphicsContext.DX9Device.SetRenderTarget(0, rTarget);
				}

				m_idebugstep=2;
				//					backBuffer=GUIGraphicsContext.DX9Device.GetBackBuffer(0,0,BackBufferType.Mono);
				//first time, fade in the video in 12 steps
				int iMaxSteps = 12;
				if (m_iFrame < iMaxSteps)
				{
					// fade in
					int iStep = 0xff / iMaxSteps;
					if (m_bFadeIn)
					{
						m_lColorDiffuse = iStep * m_iFrame;
						m_lColorDiffuse <<= 24;
						m_lColorDiffuse |= 0xffffff;
					}
					else
					{
						m_lColorDiffuse = (iMaxSteps - iStep) * m_iFrame;
						m_lColorDiffuse <<= 24;
						m_lColorDiffuse |= 0xffffff;
					}
					m_iFrame++;
				}
				else 
				{
					//after 12 steps, just present the video texture
					m_lColorDiffuse = 0xFFffffff;
				}

				m_idebugstep=3;
				//get desired video window
				if (width>0 && height>0 && m_texAdr!=0)
				{
					Size nativeSize = new Size(width, height);
					renderTexture= SetVideoWindow(nativeSize);
				}
				else renderTexture=false;

				m_idebugstep=4;
				//clear screen
				GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
				
				m_idebugstep=5;
				GUIGraphicsContext.DX9Device.BeginScene();
				if (!GUIGraphicsContext.BlankScreen)
				{
					
					m_idebugstep=6;
					//check if we should render the GUI first and then the video or
					//first the video and then the GUI
					bool bRenderGUIFirst = false;
					if (!GUIGraphicsContext.IsFullScreenVideo)
					{
						if (GUIGraphicsContext.ShowBackground)
						{
							//when we're looking at the GUI and the video is presented in a video preview window
							//then first render the GUI
							//In the other case the video is presented fullscreen. Then we first draw the video
							//and after that we draw the GUI (like the OSD)
							bRenderGUIFirst = true;
						}
					}

					m_idebugstep=7;
					//render GUI if needed
					if (bRenderGUIFirst)
					{
						if (m_renderer != null) 
						{
							m_idebugstep=8;
							m_renderer.RenderFrame(timePassed);
							m_idebugstep=9;
						}
					}
		
					m_idebugstep=10;
					//Render video texture
					if (renderTexture)
					{
						m_idebugstep=11;
						GUIFontManager.Present();
						
						m_idebugstep=12;
						unsafe
						{
							if (m_texAdr!=0)
							{
								DrawTexture(m_texAdr,_fx,_fy,_nw,_nh, _uoff, _voff, _umax, _vmax, m_lColorDiffuse);
							}
						}
						
						m_idebugstep=13;
					}
					
					m_idebugstep=14;
					//render GUI if needed
					if (!bRenderGUIFirst)
					{
						if (m_renderer != null) 
						{
							m_idebugstep=15;
							m_renderer.RenderFrame(timePassed);
							m_idebugstep=16;
						}
					}
					m_idebugstep=17;

					//using (GraphicsStream strm=backBuffer.LockRectangle(LockFlags.None))
					//{
					//}
					//backBuffer.UnlockRectangle();
					GUIFontManager.Present();
					m_idebugstep=18;
					//and present it onscreen
				}
				GUIGraphicsContext.DX9Device.EndScene();
				m_idebugstep=19;
				GUIGraphicsContext.DX9Device.Present();
				m_idebugstep=20;
			}
			catch (Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"Planescene({0},{1},{2},{3},{4},{5},{6}):Unhandled exception in {7} {8} {9}",
					width,height,arWidth, arHeight, m_texAdr, isRepaint,m_idebugstep,
					ex.Message,ex.Source,ex.StackTrace);
			}
			finally
			{
				reentrant=false;
				GUIGraphicsContext.InVmr9Render=false;
			}
		}

		
		private void InternalPresentSurface(int width,int height,int arWidth, int arHeight, bool InRepaint)
		{
			if (reentrant)
			{
				Log.WriteFile(Log.LogType.Log,true,"PlaneScene: re-entrancy in PresentSurface");
				return;
			}
			try
			{
				reentrant=true;
				m_idebugstep=1;
				if (width >0 && height >0) 
				{
					m_vmr9Util.VideoWidth=width;
					m_vmr9Util.VideoHeight=height;
					m_vmr9Util.VideoAspectRatioX=arWidth;
					m_vmr9Util.VideoAspectRatioY=arHeight;
				}
				arVideoWidth=arWidth;
				arVideoHeight=arHeight;
				GUIGraphicsContext.InVmr9Render=true;
				//if we're stopping then just return
				float timePassed=GUIGraphicsContext.TimePassed;
				if (m_bStop) return;
				//Direct3D.Surface backBuffer=null;
				//sanity checks
				if (GUIGraphicsContext.DX9Device==null) return;
				if (GUIGraphicsContext.DX9Device.Disposed) return;
				if (GUIWindowManager.IsSwitchingToNewWindow) return; //dont present video during window transitions

				m_idebugstep=2;
				if (rTarget!=null) 
				{
				  if (!rTarget.Disposed)
				    GUIGraphicsContext.DX9Device.SetRenderTarget(0, rTarget);
				}


				m_idebugstep=3;
				//					backBuffer=GUIGraphicsContext.DX9Device.GetBackBuffer(0,0,BackBufferType.Mono);
				//first time, fade in the video in 12 steps
				int iMaxSteps = 12;
				if (m_iFrame < iMaxSteps)
				{
					// fade in
					int iStep = 0xff / iMaxSteps;
					if (m_bFadeIn)
					{
						m_lColorDiffuse = iStep * m_iFrame;
						m_lColorDiffuse <<= 24;
						m_lColorDiffuse |= 0xffffff;
					}
					else
					{
						m_lColorDiffuse = (iMaxSteps - iStep) * m_iFrame;
						m_lColorDiffuse <<= 24;
						m_lColorDiffuse |= 0xffffff;
					}
					m_iFrame++;
				}
				else 
				{
					//after 12 steps, just present the video texture
					m_lColorDiffuse = 0xFFffffff;
				}

				
				m_idebugstep=4;
				//get desired video window
				Size nativeSize = new Size(width, height);
				if (width>0 && height>0 && m_surfAdr!=0)
				{
					renderTexture= SetVideoWindow(nativeSize);
				}
				else renderTexture=false;

				m_idebugstep=5;
				//clear screen
				GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
				m_idebugstep=6;
				GUIGraphicsContext.DX9Device.BeginScene();
				m_idebugstep=7;

				if (!GUIGraphicsContext.BlankScreen) 
				{
					//check if we should render the GUI first and then the video or
					//first the video and then the GUI
					bool bRenderGUIFirst = false;
					if (!GUIGraphicsContext.IsFullScreenVideo)
					{
						if (GUIGraphicsContext.ShowBackground)
						{
							//when we're looking at the GUI and the video is presented in a video preview window
							//then first render the GUI
							//In the other case the video is presented fullscreen. Then we first draw the video
							//and after that we draw the GUI (like the OSD)
							bRenderGUIFirst = true;
						}
					}

					//render GUI if needed
					if (bRenderGUIFirst)
					{
						if (m_renderer != null) 
						{
							m_idebugstep=8;
							m_renderer.RenderFrame(timePassed);
						}
					}
					m_idebugstep=9;

					//Render video texture
					if (renderTexture )
					{
						m_idebugstep=10;
						GUIFontManager.Present();
						m_idebugstep=11;
						unsafe
						{
							if (m_surfAdr!=0)
							{
								DrawSurface(m_surfAdr,_fx,_fy,_nw,_nh, _uoff, _voff, _umax, _vmax, m_lColorDiffuse);
							}
						}
					}

					m_idebugstep=12;
					//render GUI if needed
					if (!bRenderGUIFirst)
					{
						if (m_renderer != null) 
						{
							
							m_idebugstep=13;
							m_renderer.RenderFrame(timePassed);
						}
					}
					m_idebugstep=14;

					//using (GraphicsStream strm=backBuffer.LockRectangle(LockFlags.None))
					//{
					//}
					//backBuffer.UnlockRectangle();
					GUIFontManager.Present();
					//and present it onscreen
				}
				m_idebugstep=15;
				GUIGraphicsContext.DX9Device.EndScene();
				m_idebugstep=16;
				GUIGraphicsContext.DX9Device.Present();
				m_idebugstep=17;
			}
			catch (Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"Planescene({0},{1},{2},{3},{4},{5},{6}):Unhandled exception in {7} {8} {9}",
					width,height,arWidth, arHeight, m_surfAdr, InRepaint,m_idebugstep,
					ex.Message,ex.Source,ex.StackTrace);
			}
			finally
			{
				reentrant=false;
				GUIGraphicsContext.InVmr9Render=false;
			}
		}

		private void DrawTexture(uint texAddr, float fx,float fy,float nw,float nh, float uoff, float voff, float umax, float vmax, long lColorDiffuse)
		{
			if (texAddr==0) return;
			CustomVertex.TransformedColoredTextured[] verts = (CustomVertex.TransformedColoredTextured[])m_vertexBuffer.Lock(0,0); // Lock the buffer (which will return our structs)
			verts[0].X=fx- 0.5f; verts[0].Y=fy+nh- 0.5f;verts[0].Z= 0.0f;verts[0].Rhw=1.0f ;
			verts[0].Color = (int)lColorDiffuse;
			verts[0].Tu = uoff;
			verts[0].Tv = voff+vmax;

			verts[1].X= fx- 0.5f; verts[1].Y=fy- 0.5f;verts[1].Z= 0.0f; verts[1].Rhw=1.0f ;
			verts[1].Color = (int)lColorDiffuse;
			verts[1].Tu = uoff;
			verts[1].Tv = voff;

			verts[2].X= fx+nw- 0.5f; verts[2].Y=fy+nh- 0.5f;verts[2].Z= 0.0f;verts[2].Rhw=1.0f;
			verts[2].Color = (int)lColorDiffuse;
			verts[2].Tu = uoff+umax;
			verts[2].Tv = voff+vmax;

			verts[3].X=  fx+nw- 0.5f;verts[3].Y=  fy- 0.5f;verts[3].Z=   0.0f;verts[3].Rhw=  1.0f ;
			verts[3].Color = (int)lColorDiffuse;
			verts[3].Tu = uoff+umax;
			verts[3].Tv = voff;
			m_vertexBuffer.Unlock();
			unsafe
			{

				IntPtr ptr = new IntPtr( texAddr);
				FontEngineSetTexture(ptr.ToPointer());
				GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaBlendEnable,false);
				GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaTestEnable,false);

				GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter=TextureFilter.Linear;
				GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter=TextureFilter.Linear;
				GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter=TextureFilter.Linear;
				GUIGraphicsContext.DX9Device.SamplerState[0].AddressU=TextureAddress.Clamp;
				GUIGraphicsContext.DX9Device.SamplerState[0].AddressV=TextureAddress.Clamp;

				GUIGraphicsContext.DX9Device.SetStreamSource( 0, m_vertexBuffer, 0);
				GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
				GUIGraphicsContext.DX9Device.DrawPrimitives( PrimitiveType.TriangleStrip, 0, 2 );


				// unset the texture and palette or the texture caching crashes because the runtime still has a reference
				GUIGraphicsContext.DX9Device.SetTexture( 0, null);
			}
		}

		private void DrawSurface(uint texAddr, float fx,float fy,float nw,float nh, float uoff, float voff, float umax, float vmax, long lColorDiffuse)
		{
			if (texAddr==0) return;
			unsafe
			{
				GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaBlendEnable,false);
				IntPtr ptr = new IntPtr( texAddr);
				FontEngineDrawSurface(rSource.Left,rSource.Top,rSource.Width,rSource.Height,
															rDest.Left,rDest.Top,rDest.Width,rDest.Height,
																ptr.ToPointer());
			}
		}

		public void Repaint()
		{	
			if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING) return;
			if (!isEnabled) return;
			if (m_bStop) return;
//			Log.Write("scene.repaint");
			try
			{
				
				if (m_texAdr!=0)
				{
					if (!GUIGraphicsContext.InVmr9Render)
						InternalPresentImage(m_vmr9Util.VideoWidth,m_vmr9Util.VideoHeight,arVideoWidth,arVideoHeight,true);
				}
				else if (m_surfAdr!=0)
				{
					if (!GUIGraphicsContext.InVmr9Render)
						InternalPresentSurface(m_vmr9Util.VideoWidth,m_vmr9Util.VideoHeight,arVideoWidth,arVideoHeight,true);
				}
				else
				{
					if (!GUIGraphicsContext.InVmr9Render)
						InternalPresentImage(m_vmr9Util.VideoWidth,m_vmr9Util.VideoHeight,arVideoWidth,arVideoHeight,true);
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"planescene:Unhandled exception in {0} {1} {2}",
						ex.Message,ex.Source,ex.StackTrace);
			}
//			Log.Write("scene.repaint done");
		}
		#endregion
	}//public class PlaneScene 
}//namespace MediaPortal.Player 
