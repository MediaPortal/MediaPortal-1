using System;
using System.Drawing;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.Player 
{
	/// <summary>
	/// Summary description for PlaneScene.
	/// </summary>
  public class PlaneScene 
  {
    bool					  m_bStop = false;
    static Surface	rTarget = null;
    VertexBuffer vertexBuffer = null;
    IRender				  m_renderer;
    long					  m_lColorDiffuse = 0xFFFFFFFF;
    int						  m_iFrame = 0;
    bool					  m_bFadeIn = true;
    float           m_fU=1.0f;
    float           m_fV=1.0f;
    Rectangle       previousRect;
    bool            renderTexture=false;
    MediaPortal.GUI.Library.Geometry.Type arType;

    System.Drawing.Rectangle rSource, rDest;    
    MediaPortal.GUI.Library.Geometry m_geometry = new MediaPortal.GUI.Library.Geometry();

    public PlaneScene(IRender renderer)
    {
      m_renderer = renderer;
    }

    public void Stop()
    {
      m_bStop = true;
      try
      {
        if (rTarget != null)
          GUIGraphicsContext.DX9Device.SetRenderTarget(0, rTarget);
      }
      catch(Exception)
      {
        Log.Write("exception in planescene.cs Stop()");
      }
    }

    public void SetSrcRect(float fU, float fV)
    {
      m_fU = fU;
      m_fV = fV;
    }

    public System.Drawing.Rectangle SourceRect
    {
      get { return  rSource;}
    }
    public System.Drawing.Rectangle DestRect
    {
      get { return  rDest;}
    }

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
		}

    
    public bool SetVideoWindow(Size videoSize)
    {
      try
      {
        float x = GUIGraphicsContext.VideoWindow.X;
        float y = GUIGraphicsContext.VideoWindow.Y;
        float nw = GUIGraphicsContext.VideoWindow.Width;
        float nh = GUIGraphicsContext.VideoWindow.Height; 

        if (nw > GUIGraphicsContext.OverScanWidth)
          nw = GUIGraphicsContext.OverScanWidth;
        if (nh > GUIGraphicsContext.OverScanHeight)
          nh = GUIGraphicsContext.OverScanHeight;
        if (GUIGraphicsContext.IsFullScreenVideo || !GUIGraphicsContext.ShowBackground)
        {
          x = GUIGraphicsContext.OverScanLeft;
          y = GUIGraphicsContext.OverScanTop;
          nw = GUIGraphicsContext.OverScanWidth;
          nh = GUIGraphicsContext.OverScanHeight;
        }
        else if (!g_Player.IsTV)
        {
          if (!GUIGraphicsContext.Overlay) return false;
        }
        if (nw <= 10 || nh <= 10) return false;
        if (x < 0 || y < 0) return false;

        
        if (x ==previousRect.X && y==previousRect.Y && 
            nw==previousRect.Width && nh==previousRect.Height &&
            GUIGraphicsContext.ARType==arType)
        {
          return renderTexture;
        }
        previousRect=new Rectangle((int)x,(int)y,(int)nw,(int)nh);
        arType=GUIGraphicsContext.ARType;
        
        float fVideoWidth=(float)videoSize.Width;
        fVideoWidth *= m_fU;
        float fVideoHeight=(float)videoSize.Height;
        fVideoWidth *= m_fV;
        m_geometry.ImageWidth = (int) fVideoWidth;//videoSize.Width;
        m_geometry.ImageHeight = (int)fVideoHeight;//videoSize.Height;
        m_geometry.ScreenWidth = (int)nw;
        m_geometry.ScreenHeight = (int)nh;
        m_geometry.ARType = GUIGraphicsContext.ARType;
        m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(out rSource, out rDest);
        rDest.X += (int)x;
        rDest.Y += (int)y;

        if (rDest.Width < 10) return false;
        if (rDest.Height < 10) return false;
        if (rSource.Width < 10) return false;
        if (rSource.Height < 10) return false;


        float uoffs = ((float)rSource.X) / (fVideoWidth);
        float voffs = ((float)rSource.Y) / (fVideoHeight);
        float u = ((float)rSource.Width) / (fVideoWidth);
        float v = ((float)rSource.Height) / (fVideoHeight);

        x = (float)rDest.X;
        y = (float)rDest.Y;
        nw = (float)rDest.Width;
        nh = (float)rDest.Height;
        CustomVertex.TransformedTextured[] verts = (CustomVertex.TransformedTextured[])vertexBuffer.Lock(0, 0);
        
        verts[0].X = x - 0.5f; verts[0].Y = y + nh - 0.5f; verts[0].Z = 0.0f; verts[0].Rhw = 1.0f;
        verts[0].Tu = uoffs;
        verts[0].Tv = voffs + v;
//        verts[0].Color = (int)m_lColorDiffuse;

        verts[1].X = x - 0.5f; verts[1].Y = y - 0.5f; verts[1].Z = 0.0f; verts[1].Rhw = 1.0f;
        verts[1].Tu = uoffs;
        verts[1].Tv = voffs;
//        verts[1].Color = (int)m_lColorDiffuse;

        verts[2].X = x + nw - 0.5f; verts[2].Y = y + nh - 0.5f; verts[2].Z = 0.0f; verts[2].Rhw = 1.0f;
        verts[2].Tu = uoffs + u;
        verts[2].Tv = voffs + v;
//        verts[2].Color = (int)m_lColorDiffuse;

        verts[3].X = x + nw - 0.5f; verts[3].Y = y - 0.5f; verts[3].Z = 0.0f; verts[3].Rhw = 1.0f;
        verts[3].Tu = uoffs + u;
        verts[3].Tv = voffs;
//        verts[3].Color = (int)m_lColorDiffuse;
  			
        vertexBuffer.Unlock();
        return true;
      }
      catch (Exception ex)
      {
        Log.Write("planescene.SetVideoWindow excpetion:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        return false;
      }
    }


		public void Render(Device device, Texture tex, Size nativeSize) 
		{
      if (m_bStop) return;

      lock(this) 
      {
        try
        {
          if (device==null) return;
          if (device.Disposed) return;
          if (rTarget!=null) device.SetRenderTarget(0, rTarget);
          if (GUIWindowManager.IsSwitchingToNewWindow) return;

  				
          int iMaxSteps = 12;
          // fade in
          if (m_iFrame < iMaxSteps)
          {
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
            m_lColorDiffuse = 0xFFffffff;
          }

          renderTexture= SetVideoWindow(nativeSize);

          device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);


          device.BeginScene();

  				
          bool bRenderGUIFirst = false;
          if (!GUIGraphicsContext.IsFullScreenVideo)
          {
            if (GUIGraphicsContext.ShowBackground)
            {
              bRenderGUIFirst = true;
            }
          }
          if (bRenderGUIFirst)
          {
            if (m_renderer != null) m_renderer.RenderFrame();
          }
    
          if (renderTexture)
          {
            device.SetTexture(0, tex);
            /*
                      device.TextureState[0].ColorOperation =Direct3D.TextureOperation.Modulate;
                      device.TextureState[0].ColorArgument1 =Direct3D.TextureArgument.TextureColor;
                      device.TextureState[0].ColorArgument2 =Direct3D.TextureArgument.Diffuse;
  				
                      device.TextureState[0].AlphaOperation =Direct3D.TextureOperation.Modulate;
  				
                      device.TextureState[0].AlphaArgument1 =Direct3D.TextureArgument.TextureColor;
                      device.TextureState[0].AlphaArgument2 =Direct3D.TextureArgument.Diffuse;
                      device.TextureState[1].ColorOperation =Direct3D.TextureOperation.Disable;
                      device.TextureState[1].AlphaOperation =Direct3D.TextureOperation.Disable ;
  				
                    */
            int g_nAnisotropy=GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
            //      float g_fMipMapLodBias=0.0f;
            device.SamplerState[0].MinFilter=TextureFilter.Linear;
            device.SamplerState[0].MagFilter=TextureFilter.Linear;
            device.SamplerState[0].MipFilter=TextureFilter.Linear;
            device.SamplerState[0].MaxAnisotropy=g_nAnisotropy;

            //      device.SamplerState[0].MipMapLevelOfDetailBias=g_fMipMapLodBias;
  	      
            device.SamplerState[1].MinFilter=TextureFilter.Linear;
            device.SamplerState[1].MagFilter=TextureFilter.Linear;
            device.SamplerState[1].MipFilter=TextureFilter.Linear;
            device.SamplerState[1].MaxAnisotropy=g_nAnisotropy;
            /*
                      device.RenderState.ZBufferEnable=false;
                      device.RenderState.FogEnable=false;
                      device.RenderState.FogTableMode=Direct3D.FogMode.None;
                      device.RenderState.FillMode=Direct3D.FillMode.Solid;
                      device.RenderState.CullMode=Direct3D.Cull.CounterClockwise;
                      device.RenderState.AlphaBlendEnable=true;
                      device.RenderState.SourceBlend=Direct3D.Blend.SourceAlpha;
                      device.RenderState.DestinationBlend=Direct3D.Blend.InvSourceAlpha;
            */
            device.SetStreamSource(0, vertexBuffer, 0);
            device.VertexFormat = CustomVertex.TransformedTextured.Format;
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            device.SetTexture(0, null);
          }

          if (!bRenderGUIFirst)
          {
            if (m_renderer != null) m_renderer.RenderFrame();
          }

          device.EndScene();
          device.Present();
        }
        catch (Exception)
        {
        }
      }

		}

	}
}
