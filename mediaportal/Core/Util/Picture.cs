#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using Direct3D = Microsoft.DirectX.Direct3D;

using MediaPortal.GUI.Library;

namespace MediaPortal.Util
{
  /// <summary>
  /// General helper class to load, rotate, scale and render pictures
  /// </summary>
  public class Picture
  {

    // singleton. Dont allow any instance of this class
    static Picture()
    {
    }

    /// <summary>
    /// This method will load a picture from file and return a DirectX Texture of it
    /// </summary>
    /// <param name="strPic">filename of picture</param>
    /// <param name="iRotate">
    /// 0 = no rotate
    /// 1 = rotate 90 degrees
    /// 2 = rotate 180 degrees
    /// 3 = rotate 270 degrees
    /// </param>
    /// <param name="iMaxWidth">Maximum width allowed. if picture is larger then it will be downscaled</param>
    /// <param name="iMaxHeight">Maximum height allowed. if picture is larger then it will be downscaled</param>
    /// <param name="bRGB">not used</param>
    /// <param name="bZoom">
    /// true : zoom in /scale picture so that it's iMaxWidth/iMaxHeight
    /// false: dont zoom in
    /// </param>
    /// <param name="iWidth">width of the returned texture</param>
    /// <param name="iHeight">height of the returned texture</param>
    /// <returns>Texture with image or null if image could not be loaded</returns>
    /// 
    public static Texture Load(string strPic, int iRotate, int iMaxWidth, int iMaxHeight, bool bRGB, bool bZoom, out int iWidth, out int iHeight)
    {
      return Load(strPic, iRotate, iMaxWidth, iMaxHeight, bRGB, bZoom, false, out iWidth, out iHeight);
    }

    public static Texture Load(string strPic, int iRotate, int iMaxWidth, int iMaxHeight, bool bRGB, bool bZoom, bool bOversized, out int iWidth, out int iHeight)
    {
      iWidth = 0;
      iHeight = 0;
      if (strPic == null)
        return null;
      if (strPic == string.Empty)
        return null;

      Log.Info("load picture {0}", strPic);
      Direct3D.Texture texture = null;
      Image theImage = null;
      try
      {
        theImage = Image.FromFile(strPic);
        if (theImage == null)
          return null;
        if (iRotate > 0)
        {
          RotateFlipType fliptype;
          switch (iRotate)
          {
            case 1:
              fliptype = RotateFlipType.Rotate90FlipNone;
              theImage.RotateFlip(fliptype);
              break;
            case 2:
              fliptype = RotateFlipType.Rotate180FlipNone;
              theImage.RotateFlip(fliptype);
              break;
            case 3:
              fliptype = RotateFlipType.Rotate270FlipNone;
              theImage.RotateFlip(fliptype);
              break;
            default:
              fliptype = RotateFlipType.RotateNoneFlipNone;
              break;
          }
        }
        iWidth = theImage.Size.Width;
        iHeight = theImage.Size.Height;

        int iBitmapWidth = iWidth;
        int iBitmapHeight = iHeight;

        bool bResize = false;
        float fOutputFrameAR;
        if (bZoom)
        {
          bResize = true;
          iBitmapWidth = iMaxWidth;
          iBitmapHeight = iMaxHeight;
          while (iWidth < iMaxWidth || iHeight < iMaxHeight)
          {
            iWidth *= 2;
            iHeight *= 2;
          }
          int iOffsetX1 = GUIGraphicsContext.OverScanLeft;
          int iOffsetY1 = GUIGraphicsContext.OverScanTop;
          int iScreenWidth = GUIGraphicsContext.OverScanWidth;
          int iScreenHeight = GUIGraphicsContext.OverScanHeight;
          float fPixelRatio = GUIGraphicsContext.PixelRatio;
          float fSourceFrameAR = ((float)iWidth) / ((float)iHeight);
          fOutputFrameAR = fSourceFrameAR / fPixelRatio;
        }
        else
        {
          fOutputFrameAR = ((float)iWidth) / ((float)iHeight);
        }

        if (iWidth > iMaxWidth)
        {
          bResize = true;
          iWidth = iMaxWidth;
          iHeight = (int)(((float)iWidth) / fOutputFrameAR);
        }

        if (iHeight > (int)iMaxHeight)
        {
          bResize = true;
          iHeight = iMaxHeight;
          iWidth = (int)(fOutputFrameAR * ((float)iHeight));
        }

        if (!bOversized)
        {
          iBitmapWidth = iWidth;
          iBitmapHeight = iHeight;
        }
        else
        {
          // Adjust width/height 2 pixcels for smoother zoom actions at the edges
          iBitmapWidth = iWidth + 2;
          iBitmapHeight = iHeight + 2;
          bResize = true;
        }

        if (bResize)
        {
          using (Bitmap result = new Bitmap(iBitmapWidth, iBitmapHeight))
          {
            using (Graphics g = Graphics.FromImage(result))
            {
              g.CompositingQuality = Thumbs.Compositing;
              g.InterpolationMode = Thumbs.Interpolation;
              g.SmoothingMode = Thumbs.Smoothing;
              if (bOversized)
              {
                // Set picture at center position
                int xpos = 1;// (iMaxWidth-iWidth)/2;
                int ypos = 1;// (iMaxHeight-iHeight)/2;
                g.DrawImage(theImage, new Rectangle(xpos, ypos, iWidth, iHeight));
              }
              else
              {
                g.DrawImage(theImage, new Rectangle(0, 0, iWidth, iHeight));
              }
            }
            texture = Picture.ConvertImageToTexture(result, out iWidth, out iHeight);
          }
        }
        else
        {
          texture = Picture.ConvertImageToTexture((Bitmap)theImage, out iWidth, out iHeight);
        }

      }

      catch (ThreadAbortException ext)
      {
        Log.Debug("Picture: exception loading {0} err:{1}", strPic, ext.Message);
      }

      catch (Exception ex)
      {
        Log.Warn("Picture: exception loading {0} err:{1}", strPic, ex.Message);
      }
      finally
      {
        if (theImage != null)
        {
          theImage.Dispose();
        }
      }
      return texture;
    }

    /// <summary>
    /// This method converts a GDI image to a DirectX Textures
    /// </summary>
    /// <param name="theImage">GDI Image</param>
    /// <param name="iWidth">width of returned texture</param>
    /// <param name="iHeight">height of returned texture</param>
    /// <returns>Texture with image or null if image could not be loaded</returns>
    public static Texture ConvertImageToTexture(Bitmap theImage, out int iWidth, out int iHeight)
    {
      iWidth = 0;
      iHeight = 0;
      if (theImage == null)
        return null;
      // Texture texture=null;
      try
      {
        Texture texture = null;
        using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
        {
          ImageInformation info2 = new ImageInformation();
          theImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
          stream.Flush();
          stream.Seek(0, System.IO.SeekOrigin.Begin);
          texture = TextureLoader.FromStream(
            GUIGraphicsContext.DX9Device,
            stream,
            0, 0,//width/height
            1,//mipslevels
            0,//Usage.Dynamic,
            Format.X8R8G8B8,
            GUIGraphicsContext.GetTexturePoolType(),
            Filter.None,
            Filter.None,
            (int)0,
            ref info2);
          stream.Close();
          iWidth = info2.Width;
          iHeight = info2.Height;
        }
        return texture;

      }
      catch (Exception ex)
      {
        Log.Info("Picture.ConvertImageToTexture( {0}x{1} ) exception err:{2} stack:{3}",
          iWidth, iHeight,
          ex.Message, ex.StackTrace);
      }
      return null;
    }


    /// <summary>
    /// render the image contained in texture onscreen
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
    //public static void RenderImage(ref Texture texture, float x, float y, float nw, float nh, float iTextureWidth, float iTextureHeight, float iTextureLeft, float iTextureTop, bool bHiQuality)
    public static void RenderImage(Texture texture, float x, float y, float nw, float nh, float iTextureWidth, float iTextureHeight, float iTextureLeft, float iTextureTop, bool bHiQuality)
    {
      if (texture == null)
        return;
      if (texture.Disposed)
        return;
      if (GUIGraphicsContext.DX9Device == null)
        return;
      if (GUIGraphicsContext.DX9Device.Disposed)
        return;

      if (x < 0 || y < 0)
        return;
      if (nw < 0 || nh < 0)
        return;
      if (iTextureWidth < 0 || iTextureHeight < 0)
        return;
      if (iTextureLeft < 0 || iTextureTop < 0)
        return;

      VertexBuffer m_vbBuffer = null;
      try
      {
        m_vbBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured),
          4, GUIGraphicsContext.DX9Device,
          0, CustomVertex.TransformedColoredTextured.Format,
          GUIGraphicsContext.GetTexturePoolType());

        Direct3D.SurfaceDescription desc;
        desc = texture.GetLevelDescription(0);

        float uoffs = ((float)iTextureLeft) / ((float)desc.Width);
        float voffs = ((float)iTextureTop) / ((float)desc.Height);
        float umax = ((float)iTextureWidth) / ((float)desc.Width);
        float vmax = ((float)iTextureHeight) / ((float)desc.Height);
        long _diffuseColor = 0xffffffff;

        if (uoffs < 0 || uoffs > 1)
          return;
        if (voffs < 0 || voffs > 1)
          return;
        if (umax < 0 || umax > 1)
          return;
        if (vmax < 0 || vmax > 1)
          return;
        if (iTextureWidth + iTextureLeft < 0 || iTextureWidth + iTextureLeft > (float)desc.Width)
          return;
        if (iTextureHeight + iTextureTop < 0 || iTextureHeight + iTextureTop > (float)desc.Height)
          return;
        if (x < 0)
          x = 0;
        if (x > GUIGraphicsContext.Width)
          x = GUIGraphicsContext.Width;
        if (y < 0)
          y = 0;
        if (y > GUIGraphicsContext.Height)
          y = GUIGraphicsContext.Height;
        if (nw < 0)
          nw = 0;
        if (nh < 0)
          nh = 0;
        if (x + nw > GUIGraphicsContext.Width)
        {
          nw = GUIGraphicsContext.Width - x;
        }
        if (y + nh > GUIGraphicsContext.Height)
        {
          nh = GUIGraphicsContext.Height - y;
        }

        CustomVertex.TransformedColoredTextured[] verts = (CustomVertex.TransformedColoredTextured[])m_vbBuffer.Lock(0, 0); // Lock the buffer (which will return our structs)
        verts[0].X = x - 0.5f;
        verts[0].Y = y + nh - 0.5f;
        verts[0].Z = 0.0f;
        verts[0].Rhw = 1.0f;
        verts[0].Color = (int)_diffuseColor;
        verts[0].Tu = uoffs;
        verts[0].Tv = voffs + vmax;

        verts[1].X = x - 0.5f;
        verts[1].Y = y - 0.5f;
        verts[1].Z = 0.0f;
        verts[1].Rhw = 1.0f;
        verts[1].Color = (int)_diffuseColor;
        verts[1].Tu = uoffs;
        verts[1].Tv = voffs;

        verts[2].X = x + nw - 0.5f;
        verts[2].Y = y + nh - 0.5f;
        verts[2].Z = 0.0f;
        verts[2].Rhw = 1.0f;
        verts[2].Color = (int)_diffuseColor;
        verts[2].Tu = uoffs + umax;
        verts[2].Tv = voffs + vmax;

        verts[3].X = x + nw - 0.5f;
        verts[3].Y = y - 0.5f;
        verts[3].Z = 0.0f;
        verts[3].Rhw = 1.0f;
        verts[3].Color = (int)_diffuseColor;
        verts[3].Tu = uoffs + umax;
        verts[3].Tv = voffs;


        m_vbBuffer.Unlock();

        GUIGraphicsContext.DX9Device.SetTexture(0, texture);
        int g_nAnisotropy = GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
        float g_fMipMapLodBias = 0.0f;
        GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy = g_nAnisotropy;
        GUIGraphicsContext.DX9Device.SamplerState[0].MipMapLevelOfDetailBias = g_fMipMapLodBias;

        GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy = g_nAnisotropy;
        GUIGraphicsContext.DX9Device.SamplerState[1].MipMapLevelOfDetailBias = g_fMipMapLodBias;


        // Render the image
        GUIGraphicsContext.DX9Device.SetStreamSource(0, m_vbBuffer, 0);
        GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
        GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

        // unset the texture and palette or the texture caching crashes because the runtime still has a reference
        GUIGraphicsContext.DX9Device.SetTexture(0, null);
      }
      finally
      {
        if (m_vbBuffer != null)
          m_vbBuffer.Dispose();
      }
    }

    /// <summary>
    /// render the image contained in texture onscreen
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
    //public static void RenderImage(ref Texture texture, int x, int y, int nw, int nh, int iTextureWidth, int iTextureHeight, int iTextureLeft, int iTextureTop, bool bHiQuality)
    public static void RenderImage(Texture texture, int x, int y, int nw, int nh, int iTextureWidth, int iTextureHeight, int iTextureLeft, int iTextureTop, bool bHiQuality)
    {
      if (texture == null)
        return;
      if (texture.Disposed)
        return;
      if (GUIGraphicsContext.DX9Device == null)
        return;
      if (GUIGraphicsContext.DX9Device.Disposed)
        return;

      if (x < 0 || y < 0)
        return;
      if (nw < 0 || nh < 0)
        return;
      if (iTextureWidth < 0 || iTextureHeight < 0)
        return;
      if (iTextureLeft < 0 || iTextureTop < 0)
        return;

      VertexBuffer m_vbBuffer = null;
      try
      {
        m_vbBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured),
          4, GUIGraphicsContext.DX9Device,
          0, CustomVertex.TransformedColoredTextured.Format,
          GUIGraphicsContext.GetTexturePoolType());

        Direct3D.SurfaceDescription desc;
        desc = texture.GetLevelDescription(0);

        float uoffs = ((float)iTextureLeft) / ((float)desc.Width);
        float voffs = ((float)iTextureTop) / ((float)desc.Height);
        float umax = ((float)iTextureWidth) / ((float)desc.Width);
        float vmax = ((float)iTextureHeight) / ((float)desc.Height);
        long _diffuseColor = 0xffffffff;

        if (uoffs < 0 || uoffs > 1)
          return;
        if (voffs < 0 || voffs > 1)
          return;
        if (umax < 0 || umax > 1)
          return;
        if (vmax < 0 || vmax > 1)
          return;
        if (umax + uoffs < 0 || umax + uoffs > 1)
          return;
        if (vmax + voffs < 0 || vmax + voffs > 1)
          return;
        if (x < 0)
          x = 0;
        if (x > GUIGraphicsContext.Width)
          x = GUIGraphicsContext.Width;
        if (y < 0)
          y = 0;
        if (y > GUIGraphicsContext.Height)
          y = GUIGraphicsContext.Height;
        if (nw < 0)
          nw = 0;
        if (nh < 0)
          nh = 0;
        if (x + nw > GUIGraphicsContext.Width)
        {
          nw = GUIGraphicsContext.Width - x;
        }
        if (y + nh > GUIGraphicsContext.Height)
        {
          nh = GUIGraphicsContext.Height - y;
        }

        CustomVertex.TransformedColoredTextured[] verts = (CustomVertex.TransformedColoredTextured[])m_vbBuffer.Lock(0, 0); // Lock the buffer (which will return our structs)
        verts[0].X = x - 0.5f;
        verts[0].Y = y + nh - 0.5f;
        verts[0].Z = 0.0f;
        verts[0].Rhw = 1.0f;
        verts[0].Color = (int)_diffuseColor;
        verts[0].Tu = uoffs;
        verts[0].Tv = voffs + vmax;

        verts[1].X = x - 0.5f;
        verts[1].Y = y - 0.5f;
        verts[1].Z = 0.0f;
        verts[1].Rhw = 1.0f;
        verts[1].Color = (int)_diffuseColor;
        verts[1].Tu = uoffs;
        verts[1].Tv = voffs;

        verts[2].X = x + nw - 0.5f;
        verts[2].Y = y + nh - 0.5f;
        verts[2].Z = 0.0f;
        verts[2].Rhw = 1.0f;
        verts[2].Color = (int)_diffuseColor;
        verts[2].Tu = uoffs + umax;
        verts[2].Tv = voffs + vmax;

        verts[3].X = x + nw - 0.5f;
        verts[3].Y = y - 0.5f;
        verts[3].Z = 0.0f;
        verts[3].Rhw = 1.0f;
        verts[3].Color = (int)_diffuseColor;
        verts[3].Tu = uoffs + umax;
        verts[3].Tv = voffs;


        m_vbBuffer.Unlock();

        GUIGraphicsContext.DX9Device.SetTexture(0, texture);
        int g_nAnisotropy = GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
        float g_fMipMapLodBias = 0.0f;
        GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy = g_nAnisotropy;
        GUIGraphicsContext.DX9Device.SamplerState[0].MipMapLevelOfDetailBias = g_fMipMapLodBias;

        GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy = g_nAnisotropy;
        GUIGraphicsContext.DX9Device.SamplerState[1].MipMapLevelOfDetailBias = g_fMipMapLodBias;


        // Render the image
        GUIGraphicsContext.DX9Device.SetStreamSource(0, m_vbBuffer, 0);
        GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
        GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

        // unset the texture and palette or the texture caching crashes because the runtime still has a reference
        GUIGraphicsContext.DX9Device.SetTexture(0, null);
      }
      finally
      {
        if (m_vbBuffer != null)
          m_vbBuffer.Dispose();
      }
    }


    /// <summary>
    /// render the image contained in texture onscreen
    /// </summary>
    /// <param name="texture">Directx texture containing the image</param>
    /// <param name="x">x (left) coordinate</param>
    /// <param name="y">y (top) coordinate</param>
    /// <param name="nw">width </param>
    /// <param name="nh">height</param>
    /// <param name="iTextureWidth">width in texture</param>
    /// <param name="iTextureHeight">height in texture</param>
    /// <param name="iTextureLeft">x (left) offset in texture</param>
    /// <param name="iTextureTop">y (top) offset in texture</param>
    /// <param name="lColorDiffuse">diffuse color</param>
    //public static void RenderImage(ref Texture texture, float x, float y, float nw, float nh, float iTextureWidth, float iTextureHeight, float iTextureLeft, float iTextureTop, long lColorDiffuse)
    public static void RenderImage(Texture texture, float x, float y, float nw, float nh, float iTextureWidth, float iTextureHeight, float iTextureLeft, float iTextureTop, long lColorDiffuse)
    {
      if (texture == null)
        return;
      if (texture.Disposed)
        return;
      if (GUIGraphicsContext.DX9Device == null)
        return;
      if (GUIGraphicsContext.DX9Device.Disposed)
        return;

      if (x < 0 || y < 0)
        return;
      if (nw < 0 || nh < 0)
        return;
      if (iTextureWidth < 0 || iTextureHeight < 0)
        return;
      if (iTextureLeft < 0 || iTextureTop < 0)
        return;

      VertexBuffer m_vbBuffer = null;
      try
      {
        m_vbBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured),
          4, GUIGraphicsContext.DX9Device,
          0, CustomVertex.TransformedColoredTextured.Format,
          GUIGraphicsContext.GetTexturePoolType());

        Direct3D.SurfaceDescription desc;
        desc = texture.GetLevelDescription(0);

        float uoffs = ((float)iTextureLeft) / ((float)desc.Width);
        float voffs = ((float)iTextureTop) / ((float)desc.Height);
        float umax = ((float)iTextureWidth) / ((float)desc.Width);
        float vmax = ((float)iTextureHeight) / ((float)desc.Height);

        if (uoffs < 0 || uoffs > 1)
          return;
        if (voffs < 0 || voffs > 1)
          return;
        if (umax < 0 || umax > 1)
          return;
        if (vmax < 0 || vmax > 1)
          return;
        if (umax + uoffs < 0 || umax + uoffs > 1)
          return;
        if (vmax + voffs < 0 || vmax + voffs > 1)
          return;
        if (x < 0)
          x = 0;
        if (x > GUIGraphicsContext.Width)
          x = GUIGraphicsContext.Width;
        if (y < 0)
          y = 0;
        if (y > GUIGraphicsContext.Height)
          y = GUIGraphicsContext.Height;
        if (nw < 0)
          nw = 0;
        if (nh < 0)
          nh = 0;
        if (x + nw > GUIGraphicsContext.Width)
        {
          nw = GUIGraphicsContext.Width - x;
        }
        if (y + nh > GUIGraphicsContext.Height)
        {
          nh = GUIGraphicsContext.Height - y;
        }

        CustomVertex.TransformedColoredTextured[] verts = (CustomVertex.TransformedColoredTextured[])m_vbBuffer.Lock(0, 0); // Lock the buffer (which will return our structs)
        verts[0].X = x - 0.5f;
        verts[0].Y = y + nh - 0.5f;
        verts[0].Z = 0.0f;
        verts[0].Rhw = 1.0f;
        verts[0].Color = (int)lColorDiffuse;
        verts[0].Tu = uoffs;
        verts[0].Tv = voffs + vmax;

        verts[1].X = x - 0.5f;
        verts[1].Y = y - 0.5f;
        verts[1].Z = 0.0f;
        verts[1].Rhw = 1.0f;
        verts[1].Color = (int)lColorDiffuse;
        verts[1].Tu = uoffs;
        verts[1].Tv = voffs;

        verts[2].X = x + nw - 0.5f;
        verts[2].Y = y + nh - 0.5f;
        verts[2].Z = 0.0f;
        verts[2].Rhw = 1.0f;
        verts[2].Color = (int)lColorDiffuse;
        verts[2].Tu = uoffs + umax;
        verts[2].Tv = voffs + vmax;

        verts[3].X = x + nw - 0.5f;
        verts[3].Y = y - 0.5f;
        verts[3].Z = 0.0f;
        verts[3].Rhw = 1.0f;
        verts[3].Color = (int)lColorDiffuse;
        verts[3].Tu = uoffs + umax;
        verts[3].Tv = voffs;


        m_vbBuffer.Unlock();

        GUIGraphicsContext.DX9Device.SetTexture(0, texture);

        GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation = Direct3D.TextureOperation.Modulate;
        GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 = Direct3D.TextureArgument.TextureColor;
        GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 = Direct3D.TextureArgument.Diffuse;

        GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation = Direct3D.TextureOperation.Modulate;

        GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 = Direct3D.TextureArgument.TextureColor;
        GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 = Direct3D.TextureArgument.Diffuse;
        GUIGraphicsContext.DX9Device.TextureState[1].ColorOperation = Direct3D.TextureOperation.Disable;
        GUIGraphicsContext.DX9Device.TextureState[1].AlphaOperation = Direct3D.TextureOperation.Disable;

        int g_nAnisotropy = GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
        float g_fMipMapLodBias = 0.0f;
        GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy = g_nAnisotropy;
        GUIGraphicsContext.DX9Device.SamplerState[0].MipMapLevelOfDetailBias = g_fMipMapLodBias;

        GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy = g_nAnisotropy;
        GUIGraphicsContext.DX9Device.SamplerState[1].MipMapLevelOfDetailBias = g_fMipMapLodBias;

        // Render the image
        GUIGraphicsContext.DX9Device.SetStreamSource(0, m_vbBuffer, 0);
        GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
        GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

        // unset the texture and palette or the texture caching crashes because the runtime still has a reference
        GUIGraphicsContext.DX9Device.SetTexture(0, null);
      }
      finally
      {
        if (m_vbBuffer != null)
        {
          m_vbBuffer.Dispose();
        }
      }
    }

    /// <summary>
    /// render the image contained in texture onscreen
    /// </summary>
    /// <param name="texture">Directx texture containing the image</param>
    /// <param name="x">x (left) coordinate</param>
    /// <param name="y">y (top) coordinate</param>
    /// <param name="nw">width </param>
    /// <param name="nh">height</param>
    /// <param name="iTextureWidth">width in texture</param>
    /// <param name="iTextureHeight">height in texture</param>
    /// <param name="iTextureLeft">x (left) offset in texture</param>
    /// <param name="iTextureTop">y (top) offset in texture</param>
    /// <param name="lColorDiffuse">diffuse color</param>
    //public static void RenderImage(ref Texture texture, int x, int y, int nw, int nh, int iTextureWidth, int iTextureHeight, int iTextureLeft, int iTextureTop, long lColorDiffuse)
    public static void RenderImage(Texture texture, int x, int y, int nw, int nh, int iTextureWidth, int iTextureHeight, int iTextureLeft, int iTextureTop, long lColorDiffuse)
    {
      if (texture == null)
        return;
      if (texture.Disposed)
        return;
      if (GUIGraphicsContext.DX9Device == null)
        return;
      if (GUIGraphicsContext.DX9Device.Disposed)
        return;

      if (x < 0 || y < 0)
        return;
      if (nw < 0 || nh < 0)
        return;
      if (iTextureWidth < 0 || iTextureHeight < 0)
        return;
      if (iTextureLeft < 0 || iTextureTop < 0)
        return;

      VertexBuffer m_vbBuffer = null;
      try
      {
        m_vbBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured),
          4, GUIGraphicsContext.DX9Device,
          0, CustomVertex.TransformedColoredTextured.Format,
          GUIGraphicsContext.GetTexturePoolType());

        Direct3D.SurfaceDescription desc;
        desc = texture.GetLevelDescription(0);

        float uoffs = ((float)iTextureLeft) / ((float)desc.Width);
        float voffs = ((float)iTextureTop) / ((float)desc.Height);
        float umax = ((float)iTextureWidth) / ((float)desc.Width);
        float vmax = ((float)iTextureHeight) / ((float)desc.Height);

        if (uoffs < 0 || uoffs > 1)
          return;
        if (voffs < 0 || voffs > 1)
          return;
        if (umax < 0 || umax > 1)
          return;
        if (vmax < 0 || vmax > 1)
          return;
        if (umax + uoffs < 0 || umax + uoffs > 1)
          return;
        if (vmax + voffs < 0 || vmax + voffs > 1)
          return;
        if (x < 0)
          x = 0;
        if (x > GUIGraphicsContext.Width)
          x = GUIGraphicsContext.Width;
        if (y < 0)
          y = 0;
        if (y > GUIGraphicsContext.Height)
          y = GUIGraphicsContext.Height;
        if (nw < 0)
          nw = 0;
        if (nh < 0)
          nh = 0;
        if (x + nw > GUIGraphicsContext.Width)
        {
          nw = GUIGraphicsContext.Width - x;
        }
        if (y + nh > GUIGraphicsContext.Height)
        {
          nh = GUIGraphicsContext.Height - y;
        }

        CustomVertex.TransformedColoredTextured[] verts = (CustomVertex.TransformedColoredTextured[])m_vbBuffer.Lock(0, 0); // Lock the buffer (which will return our structs)
        verts[0].X = x - 0.5f;
        verts[0].Y = y + nh - 0.5f;
        verts[0].Z = 0.0f;
        verts[0].Rhw = 1.0f;
        verts[0].Color = (int)lColorDiffuse;
        verts[0].Tu = uoffs;
        verts[0].Tv = voffs + vmax;

        verts[1].X = x - 0.5f;
        verts[1].Y = y - 0.5f;
        verts[1].Z = 0.0f;
        verts[1].Rhw = 1.0f;
        verts[1].Color = (int)lColorDiffuse;
        verts[1].Tu = uoffs;
        verts[1].Tv = voffs;

        verts[2].X = x + nw - 0.5f;
        verts[2].Y = y + nh - 0.5f;
        verts[2].Z = 0.0f;
        verts[2].Rhw = 1.0f;
        verts[2].Color = (int)lColorDiffuse;
        verts[2].Tu = uoffs + umax;
        verts[2].Tv = voffs + vmax;

        verts[3].X = x + nw - 0.5f;
        verts[3].Y = y - 0.5f;
        verts[3].Z = 0.0f;
        verts[3].Rhw = 1.0f;
        verts[3].Color = (int)lColorDiffuse;
        verts[3].Tu = uoffs + umax;
        verts[3].Tv = voffs;


        m_vbBuffer.Unlock();

        GUIGraphicsContext.DX9Device.SetTexture(0, texture);

        GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation = Direct3D.TextureOperation.Modulate;
        GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 = Direct3D.TextureArgument.TextureColor;
        GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 = Direct3D.TextureArgument.Diffuse;

        GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation = Direct3D.TextureOperation.Modulate;

        GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 = Direct3D.TextureArgument.TextureColor;
        GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 = Direct3D.TextureArgument.Diffuse;
        GUIGraphicsContext.DX9Device.TextureState[1].ColorOperation = Direct3D.TextureOperation.Disable;
        GUIGraphicsContext.DX9Device.TextureState[1].AlphaOperation = Direct3D.TextureOperation.Disable;

        int g_nAnisotropy = GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
        float g_fMipMapLodBias = 0.0f;
        GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy = g_nAnisotropy;
        GUIGraphicsContext.DX9Device.SamplerState[0].MipMapLevelOfDetailBias = g_fMipMapLodBias;

        GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy = g_nAnisotropy;
        GUIGraphicsContext.DX9Device.SamplerState[1].MipMapLevelOfDetailBias = g_fMipMapLodBias;

        // Render the image
        GUIGraphicsContext.DX9Device.SetStreamSource(0, m_vbBuffer, 0);
        GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
        GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

        // unset the texture and palette or the texture caching crashes because the runtime still has a reference
        GUIGraphicsContext.DX9Device.SetTexture(0, null);
      }
      finally
      {
        if (m_vbBuffer != null)
        {
          m_vbBuffer.Dispose();
        }
      }
    }

    /// <summary>
    /// Creates a thumbnail of the specified image
    /// </summary>
    /// <param name="strFile">filename of the image</param>
    /// <param name="strThumb">filename of the thumbnail to create</param>
    /// <param name="iMaxWidth">maximum width of the thumbnail</param>
    /// <param name="iMaxHeight">maximum height of the thumbnail</param>
    /// <param name="iRotate">
    /// 0 = no rotate
    /// 1 = rotate 90 degrees
    /// 2 = rotate 180 degrees
    /// 3 = rotate 270 degrees
    /// </param>
    public static bool CreateThumbnail(string aInputFilename, string aThumbTargetPath, int iMaxWidth, int iMaxHeight, int iRotate, bool aFastMode)
    {
      if (string.IsNullOrEmpty(aInputFilename) || string.IsNullOrEmpty(aThumbTargetPath) || iMaxHeight <= 0 || iMaxHeight <= 0) return false;      
      if (!System.IO.File.Exists(aInputFilename)) return false;

      Image myImage = null;

      try
      {
        myImage = Image.FromFile(aInputFilename, true);

        return CreateThumbnail(myImage, aThumbTargetPath, iMaxWidth, iMaxHeight, iRotate, aFastMode);
      }
      catch (OutOfMemoryException)
      {
        Log.Warn("Picture: Creating thumbnail failed - image format is not supported of {0}", aInputFilename);
        return false;
      }
      catch (Exception ex)
      {
        Log.Error("Picture: CreateThumbnail exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        return false;
      }
      finally
      {
        if (myImage != null)
          myImage.Dispose();
      }
    }

    public static bool ThumbnailCallback()
    {
      return false;
    }

    /// <summary>
    /// Creates a thumbnail of the specified image
    /// </summary>
    /// <param name="strFile">filename of the image</param>
    /// <param name="strThumb">filename of the thumbnail to create</param>
    /// <param name="iMaxWidth">maximum width of the thumbnail</param>
    /// <param name="iMaxHeight">maximum height of the thumbnail</param>
    /// <param name="iRotate">
    /// 0 = no rotate
    /// 1 = rotate 90 degrees
    /// 2 = rotate 180 degrees
    /// 3 = rotate 270 degrees
    /// </param>
    public static bool CreateThumbnail(Image aDrawingImage, string aThumbTargetPath, int aThumbWidth, int aThumbHeight, int aRotation, bool aFastMode)
    {
      if (string.IsNullOrEmpty(aThumbTargetPath) || aThumbHeight <= 0 || aThumbHeight <= 0) return false;
      
      Bitmap myBitmap = null;
      Image myThumbnail = null;

      try
      {
        switch (aRotation)
        {
          case 1: aDrawingImage.RotateFlip(RotateFlipType.Rotate90FlipNone);  break;
          case 2: aDrawingImage.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
          case 3: aDrawingImage.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
          default: break;
        }

        int iWidth = aThumbWidth;
        int iHeight = aThumbHeight;
        float fAR = (aDrawingImage.Width) / ((float)aDrawingImage.Height);

        if (aDrawingImage.Width > aDrawingImage.Height)
          iHeight = (int)Math.Floor((((float)iWidth) / fAR));
        else
          iWidth = (int)Math.Floor((fAR * ((float)iHeight)));

        try
        {
          Utils.FileDelete(aThumbTargetPath);
        }
        catch (Exception ex)
        {
          Log.Error("Picture: Error deleting old thumbnail - {0}", ex.Message);
        }

        if (aFastMode)
        {
          Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
          myBitmap = new Bitmap(aDrawingImage, iWidth, iHeight);
          myThumbnail = myBitmap.GetThumbnailImage(iWidth, iHeight, myCallback, IntPtr.Zero);
        }
        else
        {
          myBitmap = new Bitmap(iWidth, iHeight);
          using (Graphics g = Graphics.FromImage(myBitmap))
          {
            g.CompositingQuality = Thumbs.Compositing;
            g.InterpolationMode = Thumbs.Interpolation;
            g.SmoothingMode = Thumbs.Smoothing;
            g.DrawImage(aDrawingImage, new Rectangle(0, 0, iWidth, iHeight));
          }
        }

        try
        {
          myBitmap.Save(aThumbTargetPath, System.Drawing.Imaging.ImageFormat.Jpeg);
          File.SetAttributes(aThumbTargetPath, File.GetAttributes(aThumbTargetPath) | FileAttributes.Hidden);          
          // even if run in background thread wait a little so the main process does not starve on IO
          System.Threading.Thread.Sleep(10);
          return true;
        }
        catch (Exception ex)
        {
          Log.Error("Picture: Error saving new thumbnail {0} - {1}", aThumbTargetPath, ex.Message);
          return false;
        }

      }
      catch (Exception)
      {
        return false;
      }
      finally
      {
        if (myThumbnail != null)
          myThumbnail.Dispose();
        if (myBitmap != null)
          myBitmap.Dispose();
      }
    }

    public static void DrawLine(int x1, int y1, int x2, int y2, long color)
    {
      Vector2[] vec = new Vector2[2];
      vec[0].X = x1;
      vec[0].Y = y1;
      vec[1].X = x2;
      vec[1].Y = y2;
      using (Line line = new Line(GUIGraphicsContext.DX9Device))
      {
        line.Begin();
        line.Draw(vec, (int)color);
        line.End();
      }
    }

    public static void DrawRectangle(Rectangle rect, long color, bool fill)
    {
      if (fill)
      {
        Rectangle[] rects = new Rectangle[1];
        rects[0] = rect;
        GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, (int)color, 1.0f, 0, rects);
      }
      else
      {
        Vector2[] vec = new Vector2[2];
        vec[0].X = rect.Left;
        vec[0].Y = rect.Top;
        vec[1].X = rect.Left + rect.Width;
        vec[1].Y = rect.Top;
        using (Line line = new Line(GUIGraphicsContext.DX9Device))
        {
          line.Begin();
          line.Draw(vec, (int)color);

          vec[0].X = rect.Left + rect.Width;
          vec[0].Y = rect.Top;
          vec[1].X = rect.Left + rect.Width;
          vec[1].Y = rect.Top + rect.Height;
          line.Draw(vec, (int)color);

          vec[0].X = rect.Left + rect.Width;
          vec[0].Y = rect.Top + rect.Width;
          vec[1].X = rect.Left;
          vec[1].Y = rect.Top + rect.Height;
          line.Draw(vec, (int)color);


          vec[0].X = rect.Left;
          vec[0].Y = rect.Top + rect.Height;
          vec[1].X = rect.Left;
          vec[1].Y = rect.Top;
          line.Draw(vec, (int)color);
          line.End();
        }
      }
    }
  }//public class Picture
}
