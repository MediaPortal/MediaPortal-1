using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;


using MediaPortal.GUI.Library;

namespace MediaPortal.Util
{
	/// <summary>
	/// 
	/// </summary>
  public class Picture
  {
    // singleton. Dont allow any instance of this class
    private Picture()
    {
    }

    static public Texture Load(string strPic, int iRotate,int iMaxWidth,int iMaxHeight, bool bRGB, bool bZoom, out int iWidth, out int iHeight)
    {
      GC.Collect();
      iWidth=0;
      iHeight=0;
      Log.Write("load picture {0}", strPic);
      Direct3D.Texture texture=null;
      Image theImage=null;
      try
      {
        theImage = Image.FromFile(strPic);
        if (iRotate>0)
        {
          RotateFlipType fliptype;
          switch (iRotate)
          {
            case 1:
              fliptype=RotateFlipType.Rotate90FlipNone;
              theImage.RotateFlip(fliptype);
              break;
            case 2:
              fliptype=RotateFlipType.Rotate180FlipNone;
              theImage.RotateFlip(fliptype);
              break;
            case 3:
              fliptype=RotateFlipType.Rotate270FlipNone;
              theImage.RotateFlip(fliptype);
              break;
            default:
              fliptype=RotateFlipType.RotateNoneFlipNone;
              break;
          }
        }
        iWidth  = theImage.Size.Width;
        iHeight = theImage.Size.Height;

        int iBitmapWidth=iWidth;
        int iBitmapHeight=iHeight;

        bool bResize=false;
        float fOutputFrameAR ;
        if (bZoom)
        {
          bResize=true;
          iBitmapWidth=iMaxWidth;
          iBitmapHeight=iMaxHeight;
          while (iWidth < iMaxWidth || iHeight < iMaxHeight) 
          {  
            iWidth*=2; 
            iHeight*=2;
          }
          int iOffsetX1 = GUIGraphicsContext.OverScanLeft;
          int iOffsetY1 = GUIGraphicsContext.OverScanTop;
          int iScreenWidth = GUIGraphicsContext.OverScanWidth;
          int iScreenHeight = GUIGraphicsContext.OverScanHeight;
          float fPixelRatio = GUIGraphicsContext.PixelRatio;
          float fSourceFrameAR = ((float)iWidth)/((float)iHeight);
          fOutputFrameAR = fSourceFrameAR / fPixelRatio;
        }
        else
        {
          fOutputFrameAR=((float)iWidth)/((float)iHeight);
        }
        
        if (iWidth > iMaxWidth)
        {
          bResize=true;
          iWidth  = iMaxWidth;
          iHeight = (int)( ( (float)iWidth) / fOutputFrameAR);
        }

        if (iHeight > (int)iMaxHeight)
        {
          bResize=true;
          iHeight = iMaxHeight;
          iWidth  = (int)(  fOutputFrameAR * ( (float)iHeight) );
        }
        
        if (!bZoom)
        {
          iBitmapWidth=iWidth;
          iBitmapHeight=iHeight;
        }

        if (bResize)
        {
          
          using (Bitmap result= new Bitmap(iBitmapWidth,iBitmapHeight))
          {
            using (Graphics g = Graphics.FromImage(result))
            {
              g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
              g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
              g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
              if (bZoom)
              {
                int xpos=(iMaxWidth-iWidth)/2;
                int ypos= (iMaxHeight-iHeight)/2;
                g.DrawImage(theImage, new Rectangle(xpos,ypos,iWidth,iHeight) );
              }
              else
              {
                g.DrawImage(theImage, new Rectangle(0,0,iWidth,iHeight) );
              }
            }
            texture=Picture.ConvertImageToTexture( result, out iWidth, out iHeight);
          }
        }
        else
        {
          texture=Picture.ConvertImageToTexture( (Bitmap)theImage, out iWidth, out iHeight);
        }
        
      }
      catch(Exception ex)
      {
        Log.Write("Picture.load exception {0} err:{1} stack:{2}", strPic, ex.Message,ex.StackTrace);
      }
      finally
      {
        if (theImage!=null)
        {
          theImage.Dispose();
        }
      }
      return texture;
    }

    static public Texture ConvertImageToTexture( Bitmap theImage, out int iWidth, out int iHeight)
    {
      iWidth=0;
      iHeight=0;
     // Texture texture=null;
      try
      {
        iWidth=theImage.Width;
        iHeight=theImage.Height;
        return Texture.FromBitmap(GUIGraphicsContext.DX9Device,theImage,Direct3D.Usage.None,Pool.Managed);
      }
      catch(Exception)
      {
        try
        {
          Texture texture=null;
          using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
          {
            ImageInformation info2 = new ImageInformation();
            theImage.Save(stream,System.Drawing.Imaging.ImageFormat.Bmp);
            stream.Flush();
            stream.Seek(0,System.IO.SeekOrigin.Begin);
            texture=TextureLoader.FromStream(
              GUIGraphicsContext.DX9Device,
              stream,
              0,0,//width/height
              1,//mipslevels
              0,//Usage.Dynamic,
              Format.X8R8G8B8,
              Pool.Managed,
              Filter.None,
              Filter.None,
              (int)0,
              ref info2);
            stream.Close();
            iWidth  = info2.Width;
            iHeight = info2.Height;
          }
          return texture;
        
        }
        catch (Exception ex)
        {
          Log.Write("Picture.ConvertImageToTexture( {0}x{1} ) exception err:{2} stack:{3}", 
            iWidth,iHeight,
            ex.Message,ex.StackTrace);
        }
      }
      return null;
    }

    /// <summary>
    /// render the image contained in texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="x">x (left) coordinate</param>
    /// <param name="y">y (top) coordinate</param>
    /// <param name="nw">width </param>
    /// <param name="nh">height</param>
    /// <param name="iTextureWidth">width in texture</param>
    /// <param name="iTextureHeight">height in texture</param>
    /// <param name="iTextureLeft">x (left) offset in texture</param>
    /// <param name="iTextureTop">y (top) offset in texture</param>
    /// <param name="bHiQuality">true :render in hi quality but slow, 
    ///                          false:render in lo quality but fast,  </param>
    static public void RenderImage(ref Texture texture, int x, int y, int nw, int nh, int iTextureWidth, int iTextureHeight, int iTextureLeft, int iTextureTop, bool bHiQuality)
    {

      VertexBuffer	m_vbBuffer=null;
      m_vbBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured),
                                              4, GUIGraphicsContext.DX9Device, 
                                              0, CustomVertex.TransformedColoredTextured.Format, 
                                              Pool.Managed);

      Direct3D.SurfaceDescription desc;
      desc=texture.GetLevelDescription(0);

      float uoffs = ((float)iTextureLeft)   / ((float)desc.Width);
      float voffs = ((float)iTextureTop)    / ((float)desc.Height);
      float umax  = ((float)iTextureWidth)  / ((float)desc.Width);
      float vmax  = ((float)iTextureHeight) / ((float)desc.Height);
      long m_colDiffuse=0xffffffff;
      
      CustomVertex.TransformedColoredTextured[] verts = (CustomVertex.TransformedColoredTextured[])m_vbBuffer.Lock(0,0); // Lock the buffer (which will return our structs)
      verts[0].X=x- 0.5f; verts[0].Y=y+nh- 0.5f;verts[0].Z= 0.0f;verts[0].Rhw=1.0f ;
      verts[0].Color = (int)m_colDiffuse;
      verts[0].Tu = uoffs;
      verts[0].Tv = voffs+vmax;

      verts[1].X= x- 0.5f; verts[1].Y=y- 0.5f;verts[1].Z= 0.0f; verts[1].Rhw=1.0f ;
      verts[1].Color = (int)m_colDiffuse;
      verts[1].Tu = uoffs;
      verts[1].Tv = voffs;

      verts[2].X= x+nw- 0.5f; verts[2].Y=y+nh- 0.5f;verts[2].Z= 0.0f;verts[2].Rhw=1.0f;
      verts[2].Color = (int)m_colDiffuse;
      verts[2].Tu = uoffs+umax;
      verts[2].Tv = voffs+vmax;

      verts[3].X=  x+nw- 0.5f;verts[3].Y=  y- 0.5f;verts[3].Z=   0.0f;verts[3].Rhw=  1.0f ;
      verts[3].Color = (int)m_colDiffuse;
      verts[3].Tu = uoffs+umax;
      verts[3].Tv = voffs;


      m_vbBuffer.Unlock();

      GUIGraphicsContext.DX9Device.SetTexture( 0, texture);
      /*
      GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation =Direct3D.TextureOperation.Modulate;
      GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 =Direct3D.TextureArgument.TextureColor;
      GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 =Direct3D.TextureArgument.Diffuse;
			
      GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation =Direct3D.TextureOperation.Modulate;
			
      GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 =Direct3D.TextureArgument.TextureColor;
      GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 =Direct3D.TextureArgument.Diffuse;
      GUIGraphicsContext.DX9Device.TextureState[1].ColorOperation =Direct3D.TextureOperation.Disable;
      GUIGraphicsContext.DX9Device.TextureState[1].AlphaOperation =Direct3D.TextureOperation.Disable ;
			

      GUIGraphicsContext.DX9Device.RenderState.ZBufferEnable=false;
      GUIGraphicsContext.DX9Device.RenderState.FogEnable=false;
      GUIGraphicsContext.DX9Device.RenderState.FogTableMode=Direct3D.FogMode.None;
      GUIGraphicsContext.DX9Device.RenderState.FillMode=Direct3D.FillMode.Solid;
      GUIGraphicsContext.DX9Device.RenderState.CullMode=Direct3D.Cull.CounterClockwise;
      GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable=true;
      GUIGraphicsContext.DX9Device.RenderState.SourceBlend=Direct3D.Blend.SourceAlpha;
      GUIGraphicsContext.DX9Device.RenderState.DestinationBlend=Direct3D.Blend.InvSourceAlpha;
			*/
      int g_nAnisotropy=GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
      float g_fMipMapLodBias=0.0f;
      GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy=g_nAnisotropy;
      GUIGraphicsContext.DX9Device.SamplerState[0].MipMapLevelOfDetailBias=g_fMipMapLodBias;
      
      GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy=g_nAnisotropy;
      GUIGraphicsContext.DX9Device.SamplerState[1].MipMapLevelOfDetailBias=g_fMipMapLodBias;
			

      // Render the image
      GUIGraphicsContext.DX9Device.SetStreamSource( 0, m_vbBuffer, 0);
      GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
      GUIGraphicsContext.DX9Device.DrawPrimitives( PrimitiveType.TriangleStrip, 0, 2 );

      // unset the texture and palette or the texture caching crashes because the runtime still has a reference
      GUIGraphicsContext.DX9Device.SetTexture( 0, null);
 
      m_vbBuffer.Dispose();

    }
    static public void RenderImage(ref Texture texture, int x, int y, int nw, int nh, int iTextureWidth, int iTextureHeight, int iTextureLeft, int iTextureTop, long lColorDiffuse)
    {

      VertexBuffer	m_vbBuffer=null;
      m_vbBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured),
        4, GUIGraphicsContext.DX9Device, 
        0, CustomVertex.TransformedColoredTextured.Format, 
        Pool.Managed);

      Direct3D.SurfaceDescription desc;
      desc=texture.GetLevelDescription(0);

      float uoffs = ((float)iTextureLeft)   / ((float)desc.Width);
      float voffs = ((float)iTextureTop)    / ((float)desc.Height);
      float umax  = ((float)iTextureWidth)  / ((float)desc.Width);
      float vmax  = ((float)iTextureHeight) / ((float)desc.Height);
      
      CustomVertex.TransformedColoredTextured[] verts = (CustomVertex.TransformedColoredTextured[])m_vbBuffer.Lock(0,0); // Lock the buffer (which will return our structs)
      verts[0].X=x- 0.5f; verts[0].Y=y+nh- 0.5f;verts[0].Z= 0.0f;verts[0].Rhw=1.0f ;
      verts[0].Color = (int)lColorDiffuse;
      verts[0].Tu = uoffs;
      verts[0].Tv = voffs+vmax;

      verts[1].X= x- 0.5f; verts[1].Y=y- 0.5f;verts[1].Z= 0.0f; verts[1].Rhw=1.0f ;
      verts[1].Color = (int)lColorDiffuse;
      verts[1].Tu = uoffs;
      verts[1].Tv = voffs;

      verts[2].X= x+nw- 0.5f; verts[2].Y=y+nh- 0.5f;verts[2].Z= 0.0f;verts[2].Rhw=1.0f;
      verts[2].Color = (int)lColorDiffuse;
      verts[2].Tu = uoffs+umax;
      verts[2].Tv = voffs+vmax;

      verts[3].X=  x+nw- 0.5f;verts[3].Y=  y- 0.5f;verts[3].Z=   0.0f;verts[3].Rhw=  1.0f ;
      verts[3].Color = (int)lColorDiffuse;
      verts[3].Tu = uoffs+umax;
      verts[3].Tv = voffs;


      m_vbBuffer.Unlock();

      GUIGraphicsContext.DX9Device.SetTexture( 0, texture);
      
      GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation =Direct3D.TextureOperation.Modulate;
      GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 =Direct3D.TextureArgument.TextureColor;
      GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 =Direct3D.TextureArgument.Diffuse;
			
      GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation =Direct3D.TextureOperation.Modulate;
			
      GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 =Direct3D.TextureArgument.TextureColor;
      GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 =Direct3D.TextureArgument.Diffuse;
      GUIGraphicsContext.DX9Device.TextureState[1].ColorOperation =Direct3D.TextureOperation.Disable;
      GUIGraphicsContext.DX9Device.TextureState[1].AlphaOperation =Direct3D.TextureOperation.Disable ;
			
      int g_nAnisotropy=GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
      float g_fMipMapLodBias=0.0f;
      GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy=g_nAnisotropy;
      GUIGraphicsContext.DX9Device.SamplerState[0].MipMapLevelOfDetailBias=g_fMipMapLodBias;
      
      GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter=TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy=g_nAnisotropy;
      GUIGraphicsContext.DX9Device.SamplerState[1].MipMapLevelOfDetailBias=g_fMipMapLodBias;
			
			/*
      GUIGraphicsContext.DX9Device.RenderState.ZBufferEnable=false;
      GUIGraphicsContext.DX9Device.RenderState.FogEnable=false;
      GUIGraphicsContext.DX9Device.RenderState.FogTableMode=Direct3D.FogMode.None;
      GUIGraphicsContext.DX9Device.RenderState.FillMode=Direct3D.FillMode.Solid;
      GUIGraphicsContext.DX9Device.RenderState.CullMode=Direct3D.Cull.CounterClockwise;
      GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable=true;
      GUIGraphicsContext.DX9Device.RenderState.SourceBlend=Direct3D.Blend.SourceAlpha;
      GUIGraphicsContext.DX9Device.RenderState.DestinationBlend=Direct3D.Blend.InvSourceAlpha;
*/
      // Render the image
      GUIGraphicsContext.DX9Device.SetStreamSource( 0, m_vbBuffer, 0);
      GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
      GUIGraphicsContext.DX9Device.DrawPrimitives( PrimitiveType.TriangleStrip, 0, 2 );

      // unset the texture and palette or the texture caching crashes because the runtime still has a reference
      GUIGraphicsContext.DX9Device.SetTexture( 0, null);
 
      m_vbBuffer.Dispose();

    }
    
    static bool ThumbnailCallback()
    {
      return false;
    }

    static public void CreateThumbnail(string strFile, string strThumb, int iMaxWidth, int iMaxHeight, int iRotate)
    {
      GC.Collect();
      if (strFile==null) return;
      if (strFile.Length==0) return;
      Log.Write("create thumbnail for {0}", strFile);
      Image theImage=null;
      try
      {
        theImage=Image.FromFile(strFile);
        
        switch (iRotate)
        {
          case 1:
            theImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            break;
          case 2:
            theImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
            break;
          case 3:
            theImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
            break;
          default:
            break;
        }
      
        int iWidth=iMaxWidth;
        int iHeight=iMaxHeight;
        float fAR = (theImage.Width) / ((float)theImage.Height);
        if ( theImage.Width> theImage.Height)
        {
          iHeight=(int) Math.Floor( (  ((float)iWidth) / fAR) );
        }
        else
        {
          iWidth=(int)Math.Floor( (fAR*((float)iHeight)) );
        }
        Utils.FileDelete(strThumb);
        Image imageThumb=null;
        try
        {
          //Image.GetThumbnailImageAbort myCallback =new Image.GetThumbnailImageAbort(ThumbnailCallback);
          //imageThumb=theImage.GetThumbnailImage(iWidth,iHeight,myCallback, IntPtr.Zero);
          //imageThumb.Save(strThumb,System.Drawing.Imaging.ImageFormat.Jpeg);

          using (Bitmap result= new Bitmap(iWidth,iHeight))
          {
            using (Graphics g = Graphics.FromImage(result))
            {
              g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
              g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
              g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
              g.DrawImage(theImage, new Rectangle(0,0,iWidth,iHeight) );
            }
            result.Save(strThumb,System.Drawing.Imaging.ImageFormat.Jpeg);
          }

        }
        finally
        {
          if (imageThumb!=null) 
            imageThumb.Dispose();
        }
      }
      catch (Exception ex)
      {
        Log.Write("Picture.CreateThumbnail exception {0} err:{1} stack:{2}", strFile, ex.Message,ex.StackTrace);
      }
      finally
      {
        if (theImage!=null)
        {
          theImage.Dispose();
        }
      }
    }
	}
}
