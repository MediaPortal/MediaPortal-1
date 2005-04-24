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
	public class PlaneScene :DirectShowHelperLib.VMR9Callback
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
		//static Surface	rTarget = null;
		//VertexBuffer		vertexBuffer = null;
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
		bool							m_repaint;
		int               arVideoWidth=4;
		int								arVideoHeight=3;
		int               prevVideoWidth=0;
		int               prevVideoHeight=0;
		int               prevArVideoWidth=0;
		int               prevArVideoHeight=0;
		public PlaneScene(IRender renderer, VMR9Util util)
		{
			m_repaint=false;
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
			m_bStop = true;
			try
			{
				//if (rTarget != null)
				//	GUIGraphicsContext.DX9Device.SetRenderTarget(0, rTarget);
			}
			catch(Exception ex)
			{
				Log.Write("exception in planescene.cs Stop()"+ex.ToString());
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
			lock(this) 
			{
				//if (vertexBuffer != null) vertexBuffer.Dispose();
				//vertexBuffer = null;

				//if (rTarget!=null)
				//{
				//	rTarget.Dispose();
				//	rTarget=null;
				//}
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
			//if (rTarget == null)
			//	rTarget = GUIGraphicsContext.DX9Device.GetRenderTarget(0);
			//m_iFrameCounter=0;
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
					windowId==(int)GUIWindow.Window.WINDOW_SCHEDULER||  
					windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTV||
					windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL||
					windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE)
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
/*
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
*/
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

				Log.Write("vmr9: video WxH  : {0}x{1}",videoSize.Width,videoSize.Height);
				Log.Write("vmr9: video AR   : {0}:{1}",arVideoWidth, arVideoHeight);
				Log.Write("vmr9: screen WxH : {0}x{1}",nw,nh);
				Log.Write("vmr9: AR type    : {0}",GUIGraphicsContext.ARType);
				Log.Write("vmr9: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
				Log.Write("vmr9: src        : ({0},{1})-({2},{3})",
					rSource.X,rSource.Y, rSource.X+rSource.Width,rSource.Y+rSource.Height);
			  Log.Write("vmr9: dst        : ({0},{1})-({2},{3})",
					rDest.X,rDest.Y,rDest.X+rDest.Width,rDest.Y+rDest.Height);

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
				Log.Write("vmr9 enabled:{0}", isEnabled);
			}
		}

		#region IVMR9Callback Members

		public void PresentImage(int width,int height,int arWidth, int arHeight, uint pTex)
		{
			m_vmr9Util.VideoWidth=width;
			m_vmr9Util.VideoHeight=height;
			arVideoWidth=arWidth;
			arVideoHeight=arHeight;
			if (!isEnabled) 
			{
				m_vmr9Util.FrameCounter++;
				return;
			}

			if (!GUIGraphicsContext.IsFullScreenVideo)
			{
				if (GUIGraphicsContext.Vmr9FPS>40)
				{
					if ( (m_vmr9Util.FrameCounter%2)==1)
					{
						m_vmr9Util.FrameCounter++;
						return;
					}
				}
			}
			try
			{
				GUIGraphicsContext.InVmr9Render=true;
				//if we're stopping then just return
				float timePassed=GUIGraphicsContext.TimePassed;
				if (m_bStop)
					return;
				lock(this) 
				{
					//Direct3D.Surface backBuffer=null;
					try
					{
						//sanity checks
						if (GUIGraphicsContext.DX9Device==null) 
							return;
						if (GUIGraphicsContext.DX9Device.Disposed) 
							return;
						//if (rTarget!=null) GUIGraphicsContext.DX9Device.SetRenderTarget(0, rTarget);
						if (GUIWindowManager.IsSwitchingToNewWindow) 
							return; //dont present video during window transitions

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

						//get desired video window
						Size nativeSize = new Size(width, height);
						renderTexture= SetVideoWindow(nativeSize);

						//clear screen
						GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
						GUIGraphicsContext.DX9Device.BeginScene();

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
								m_renderer.RenderFrame(timePassed);
							}
						}
		  
						//Render video texture
						if (renderTexture)
						{
							GUIFontManager.Present();
							unsafe
							{
								m_texAdr=pTex;
								DrawTexture(pTex,_fx,_fy,_nw,_nh, _uoff, _voff, _umax, _vmax, m_lColorDiffuse);
							}
						}
						//render GUI if needed
						if (!bRenderGUIFirst)
						{
							if (m_renderer != null) 
							{
								m_renderer.RenderFrame(timePassed);
							}
						}

						//using (GraphicsStream strm=backBuffer.LockRectangle(LockFlags.None))
						//{
						//}
						//backBuffer.UnlockRectangle();
						GUIFontManager.Present();
						//and present it onscreen

						GUIGraphicsContext.DX9Device.EndScene();
						GUIGraphicsContext.DX9Device.Present();
					}
					catch (Exception ex)
					{
						Log.WriteFile(Log.LogType.Log,true,"Unhandled exception in {0} {1} {2}",
							ex.Message,ex.Source,ex.StackTrace);
					}
					finally
					{
						//if (backBuffer!=null)
						//	backBuffer.Dispose();
					}
				}//lock(this) 
			}
			finally
			{
				GUIGraphicsContext.InVmr9Render=false;
			}
		}

		public void PresentSurface(int width,int height,int arWidth, int arHeight,uint pSurface)
		{
			m_vmr9Util.VideoWidth=width;
			m_vmr9Util.VideoHeight=height;
			arVideoWidth=arWidth;
			arVideoHeight=arHeight;
			if (!isEnabled) 
			{
				m_vmr9Util.FrameCounter++;
				return;
			}			

			if (!GUIGraphicsContext.IsFullScreenVideo)
			{
				if (GUIGraphicsContext.Vmr9FPS>40)
				{
					if ( (m_vmr9Util.FrameCounter%2)==1)
					{
						m_vmr9Util.FrameCounter++;
						return;
					}
				}
			}
			try
			{
				GUIGraphicsContext.InVmr9Render=true;
				//if we're stopping then just return
				float timePassed=GUIGraphicsContext.TimePassed;
				if (m_bStop) return;
				lock(this) 
				{
					//Direct3D.Surface backBuffer=null;
					try
					{
						//sanity checks
						if (GUIGraphicsContext.DX9Device==null) return;
						if (GUIGraphicsContext.DX9Device.Disposed) return;
						//if (rTarget!=null) GUIGraphicsContext.DX9Device.SetRenderTarget(0, rTarget);
						if (GUIWindowManager.IsSwitchingToNewWindow) return; //dont present video during window transitions

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

						//get desired video window
						Size nativeSize = new Size(width, height);
						renderTexture= SetVideoWindow(nativeSize);

						//clear screen
						GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
						GUIGraphicsContext.DX9Device.BeginScene();

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
								m_renderer.RenderFrame(timePassed);
							}
						}
	    
						//Render video texture
						if (renderTexture )
						{
							GUIFontManager.Present();
							unsafe
							{
								m_surfAdr=pSurface;
								DrawSurface(pSurface,_fx,_fy,_nw,_nh, _uoff, _voff, _umax, _vmax, m_lColorDiffuse);
							}
						}

						//render GUI if needed
						if (!bRenderGUIFirst)
						{
							if (m_renderer != null) 
							{
								m_renderer.RenderFrame(timePassed);
							}
						}

						//using (GraphicsStream strm=backBuffer.LockRectangle(LockFlags.None))
						//{
						//}
						//backBuffer.UnlockRectangle();
						GUIFontManager.Present();
						//and present it onscreen

						GUIGraphicsContext.DX9Device.EndScene();
						GUIGraphicsContext.DX9Device.Present();
					}
					catch (Exception ex)
					{
						Log.WriteFile(Log.LogType.Log,true,"Unhandled exception in {0} {1} {2}",
							ex.Message,ex.Source,ex.StackTrace);
					}
					finally
					{
						//if (backBuffer!=null)
						//	backBuffer.Dispose();
					}
				}//lock(this)
			}
			finally
			{
				GUIGraphicsContext.InVmr9Render=false;
			}
		}

		void DrawTexture(uint texAddr, float fx,float fy,float nw,float nh, float uoff, float voff, float umax, float vmax, long lColorDiffuse)
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
				if (!m_repaint) m_vmr9Util.FrameCounter++;
			}
		}

		void DrawSurface(uint texAddr, float fx,float fy,float nw,float nh, float uoff, float voff, float umax, float vmax, long lColorDiffuse)
		{
			
			if (texAddr==0) return;
			unsafe
			{
				GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.AlphaBlendEnable,false);
				IntPtr ptr = new IntPtr( texAddr);
				FontEngineDrawSurface(rSource.Left,rSource.Top,rSource.Width,rSource.Height,
															rDest.Left,rDest.Top,rDest.Width,rDest.Height,
																ptr.ToPointer());
				if (!m_repaint) m_vmr9Util.FrameCounter++;
			}
		}

		public void Repaint()
		{	
			if (!isEnabled) return;
			try
			{
				m_repaint=true;
				if (m_texAdr!=0)
				{
					PresentImage(m_vmr9Util.VideoWidth,m_vmr9Util.VideoHeight,arVideoWidth,arVideoHeight,m_texAdr);
				}
				else if (m_surfAdr!=0)
				{
					PresentSurface(m_vmr9Util.VideoWidth,m_vmr9Util.VideoHeight,arVideoWidth,arVideoHeight,m_surfAdr);
				}
				else
				{
					PresentImage(m_vmr9Util.VideoWidth,m_vmr9Util.VideoHeight,arVideoWidth,arVideoHeight,0);
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"Unhandled exception in {0} {1} {2}",
						ex.Message,ex.Source,ex.StackTrace);
			}
			finally
			{
				m_repaint=false;
			}
		}
		#endregion
	}//public class PlaneScene 
}//namespace MediaPortal.Player 
