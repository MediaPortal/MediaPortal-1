using System;
using System.Drawing;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.Player 
{
	/// <summary>
	/// This class will draw a video texture onscreen when using VMR9 renderless
	/// Its controlled by the allocator wrapper 
	/// Example on how to use:
	/// PlaneScene scene = new PlaneScene(GUIGraphicsContext.RenderGUI)
	/// scene.Init(GUIGraphicsContext.DX9Device)
	/// ... allocate direct3d texture
	/// scene.SetSrcRect(1.0f,1.0f); //change this depending on the texture dimensions
	/// while (playingMovie)
	/// {
	///   scene.Render(GUIGraphicsContext.DX9Device, videoTextre, videoSize) 
	/// }
	/// scene.Stop();
	/// scene.DeInit();
	/// </summary>
  public class PlaneScene 
  {
    bool					  m_bStop = false;
    static Surface	rTarget = null;
    VertexBuffer		vertexBuffer = null;
    IRender				  m_renderer;
    long					  m_lColorDiffuse = 0xFFFFFFFF;
    int						  m_iFrame = 0;
    bool					  m_bFadeIn = true;
    float           m_fU=1.0f;
    float           m_fV=1.0f;
    Rectangle       previousRect;
    bool            renderTexture=false;
    bool            lastOverlay=false;
    MediaPortal.GUI.Library.Geometry.Type arType;
    System.Drawing.Rectangle							rSource, rDest;    
    MediaPortal.GUI.Library.Geometry			m_geometry = new MediaPortal.GUI.Library.Geometry();
		int							m_iFrameCounter;
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="renderer">interface to render the GUI</param>
    public PlaneScene(IRender renderer)
    {
      m_renderer = renderer;
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
        if (rTarget != null)
          GUIGraphicsContext.DX9Device.SetRenderTarget(0, rTarget);
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
        if (vertexBuffer != null) vertexBuffer.Dispose();
        vertexBuffer = null;

        if (rTarget!=null)
        {
          rTarget.Dispose();
          rTarget=null;
        }
      }
    }

		/// <summary>
		/// Initialize.
		/// This should be called before any other methods.
		/// It allocates resources needed
		/// </summary>
		/// <param name="device">Direct3d devices</param>
		public void Init(Device device) 
		{
      vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedTextured),
                                      4, device, 
                                      0, CustomVertex.TransformedTextured.Format, 
                                      Pool.Managed);
			//device.RenderState.CullMode = Cull.None;
			//device.RenderState.Lighting = false;
			//device.RenderState.ZBufferEnable = true;

      if (rTarget == null)
			  rTarget = device.GetRenderTarget(0);
			m_iFrameCounter=0;
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
          bool inTV=false;
          int windowId=GUIWindowManager.ActiveWindow;
          if (windowId==(int)GUIWindow.Window.WINDOW_TV||
              windowId==(int)GUIWindow.Window.WINDOW_TVGUIDE||  
              windowId==(int)GUIWindow.Window.WINDOW_SEARCHTV||  
              windowId==(int)GUIWindow.Window.WINDOW_SCHEDULER||  
              windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTV||
              windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL||
              windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE)
            inTV=true;

          if (!inTV)
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
            GUIGraphicsContext.Overlay == lastOverlay && renderTexture)
        {
					//not changed, return previous settings
          return renderTexture;
        }

				//settings (position,size,aspect ratio) changed.
				//Store these settings and start calucating the new video window
        previousRect = new Rectangle((int)x, (int)y, (int)nw, (int)nh);
        arType       = GUIGraphicsContext.ARType;
        lastOverlay  = GUIGraphicsContext.Overlay;
        
				//calculate the video window according to the current aspect ratio settings
        float fVideoWidth  = (float)videoSize.Width;
				float fVideoHeight = (float)videoSize.Height;
        m_geometry.ImageWidth   = (int)fVideoWidth;
        m_geometry.ImageHeight  = (int)fVideoHeight;
        m_geometry.ScreenWidth  = (int)nw;
        m_geometry.ScreenHeight = (int)nh;
        m_geometry.ARType       = GUIGraphicsContext.ARType;
        m_geometry.PixelRatio   = GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(out rSource, out rDest);
        rDest.X += (int)x;
        rDest.Y += (int)y;

				//sanity check
        if (rDest.Width    < 10) return false;
        if (rDest.Height   < 10) return false;
        if (rSource.Width  < 10) return false;
        if (rSource.Height < 10) return false;

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

				//and update the vertex buffer with the new coordinates
        CustomVertex.TransformedTextured[] verts = (CustomVertex.TransformedTextured[])vertexBuffer.Lock(0, 0);
        
        verts[0].X  = x - 0.5f;      verts[0].Y = y + nh - 0.5f; verts[0].Z = 0.0f; verts[0].Rhw = 1.0f;
        verts[0].Tu = uoffs;
        verts[0].Tv = voffs + v;

        verts[1].X  = x - 0.5f;      verts[1].Y = y - 0.5f;      verts[1].Z = 0.0f; verts[1].Rhw = 1.0f;
        verts[1].Tu = uoffs;
        verts[1].Tv = voffs;

        verts[2].X  = x + nw - 0.5f; verts[2].Y = y + nh - 0.5f; verts[2].Z = 0.0f; verts[2].Rhw = 1.0f;
        verts[2].Tu = uoffs + u;
        verts[2].Tv = voffs + v;

        verts[3].X  = x + nw - 0.5f; verts[3].Y = y - 0.5f;      verts[3].Z = 0.0f; verts[3].Rhw = 1.0f;
        verts[3].Tu = uoffs + u;
        verts[3].Tv = voffs;
  			
        vertexBuffer.Unlock();
        return true;
      }
      catch (Exception ex)
      {
        Log.Write("planescene.SetVideoWindow excpetion:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        return false;
      }
    }


		/// <summary>
		/// This method is been called by the allocatorwrapper when we need to draw a new video frame onscreen
		/// </summary>
		/// <param name="device">Direct3d device</param>
		/// <param name="tex">Direct3d texture containing the Video frame</param>
		/// <param name="nativeSize">Size of video</param>
		public void Render(Device device, Texture tex, Size nativeSize) 
		{
			//if we're stopping then just return
      if (m_bStop) return;
			int speed=g_Player.Speed;
			if (speed < 0) speed=-speed;
			if (speed >= 4)
			{
				m_iFrameCounter++;
				if (speed ==4 && m_iFrameCounter<1) return;
				if (speed ==8 && m_iFrameCounter<2) return;
				if (speed ==16 && m_iFrameCounter<6) return;
				if (speed ==32 && m_iFrameCounter<10) return;
				m_iFrameCounter=0;
			}

      lock(this) 
      {
        try
        {
					//sanity checks
          if (device==null) return;
          if (device.Disposed) return;
          if (rTarget!=null) device.SetRenderTarget(0, rTarget);
          if (GUIWindowManager.IsSwitchingToNewWindow) return; //dont present video during window transitions

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
          renderTexture= SetVideoWindow(nativeSize);

					//clear screen
          device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
          device.BeginScene();

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
							m_renderer.RenderFrame();
							GUIFontManager.Present();
						}
          }
    
					//Render video texture
          if (renderTexture)
          {
            device.SetTexture(0, tex);
            int g_nAnisotropy=GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
            device.SamplerState[0].MinFilter=TextureFilter.Linear;
            device.SamplerState[0].MagFilter=TextureFilter.Linear;
            device.SamplerState[0].MipFilter=TextureFilter.Linear;
            device.SamplerState[0].MaxAnisotropy=g_nAnisotropy;
            device.SamplerState[1].MinFilter=TextureFilter.Linear;
            device.SamplerState[1].MagFilter=TextureFilter.Linear;
            device.SamplerState[1].MipFilter=TextureFilter.Linear;
            device.SamplerState[1].MaxAnisotropy=g_nAnisotropy;
            device.SetStreamSource(0, vertexBuffer, 0);
            device.VertexFormat = CustomVertex.TransformedTextured.Format;
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            device.SetTexture(0, null);
          }

					//render GUI if needed
          if (!bRenderGUIFirst)
          {
						if (m_renderer != null) 
						{
							m_renderer.RenderFrame();
							GUIFontManager.Present();
						}
          }

					//and present it onscreen
          device.EndScene();
          device.Present();
        }
        catch (Exception)
        {
        }
      }//lock(this) 
		}//public void Render(Device device, Texture tex, Size nativeSize) 
	}//public class PlaneScene 
}//namespace MediaPortal.Player 
