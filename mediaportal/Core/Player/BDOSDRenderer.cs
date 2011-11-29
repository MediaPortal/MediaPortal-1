#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Player
{
  public class BDOSDRenderer
  {
    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineSetAlphaBlend(UInt32 alphaBlend);

    private static BDOSDRenderer _instance;     

    /// <summary>
    /// The coordinates of current vertex buffer
    /// </summary>
    private int _wx, _wy, _wwidth, _wheight;

    /// <summary>
    /// Vertex buffer for rendering OSD
    /// </summary>
    private VertexBuffer _vertexBuffer;

    /// <summary>
    /// Texture containing the whole OSD area (1920x1080)
    /// </summary>
    private Texture _OSDTexture;

    /// <summary>
    /// Lock for syncronising the texture update and rendering
    /// </summary>
    private object _OSDLock;

    private BDOSDRenderer()
    {
      _OSDLock = new Object();
    }

    public static BDOSDRenderer GetInstance()
    {
      if (_instance == null)
      {
        _instance = new BDOSDRenderer();
      }
      return _instance;
    }

    public static void Release()
    {
      _instance = null;
    }
    
    public void DrawItem(OSDTexture item)
    {
      try
      {
        lock (_OSDLock)
        {
          if (item.texture != null && item.width > 0 && item.height > 0)
          {
			      // todo: support 2 planes
            if (_OSDTexture == null)
              _OSDTexture = new Texture(item.texture);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void Render()
    {
      lock (_OSDLock)
      {
        // Store current settings so they can be restored when we are done
        VertexFormats vertexFormat = GUIGraphicsContext.DX9Device.VertexFormat;        
        
        try
        {
          if (_OSDTexture == null || _OSDTexture.Disposed)
          {
            return;
          }

          int wx = 0, wy = 0, wwidth = 0, wheight = 0;          
          if (GUIGraphicsContext.IsFullScreenVideo)
          {
            wheight = PlaneScene.DestRect.Height;
            wwidth = PlaneScene.DestRect.Width;

            wx = GUIGraphicsContext.OverScanLeft;
            wy = GUIGraphicsContext.OverScanTop;
            
            if (PlaneScene.DestRect.X == 0 || PlaneScene.DestRect.Y == 0)
            {
              wx += PlaneScene.DestRect.X;
              wy += PlaneScene.DestRect.Y;
            }
          }
          else // Video overlay
          {
            wheight = GUIGraphicsContext.VideoWindow.Height;
            wwidth = GUIGraphicsContext.VideoWindow.Width;

            wx = GUIGraphicsContext.VideoWindow.Right - (GUIGraphicsContext.VideoWindow.Width);
            wy = GUIGraphicsContext.VideoWindow.Top;
          }
          
          FontEngineSetAlphaBlend(1); //TRUE
          CreateVertexBuffer(wx, wy, wwidth, wheight);

          // Make sure D3D objects haven't been disposed for some reason. This would cause
          // an access violation on native side, causing Skin Engine to halt rendering
          if (!_OSDTexture.Disposed && !_vertexBuffer.Disposed)
          {
            GUIGraphicsContext.DX9Device.SetStreamSource(0, _vertexBuffer, 0);
            GUIGraphicsContext.DX9Device.SetTexture(0, _OSDTexture);
            GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedTextured.Format;
            GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
          }
          else
          {
            Log.Debug("OSD renderer: D3D resource was disposed! Not trying to render the texture");
          }
        }
        catch (Exception e)
        {
          Log.Error(e);
        }

        try
        {
          // Restore device settings
          GUIGraphicsContext.DX9Device.SetTexture(0, null);
          GUIGraphicsContext.DX9Device.VertexFormat = vertexFormat;
        }
        catch (Exception e)
        {
          Log.Error(e);
        }
      }
    }

    private void CreateVertexBuffer(int wx, int wy, int wwidth, int wheight)
    {
      if (_vertexBuffer == null)
      {
        _vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedTextured),
                                        4, GUIGraphicsContext.DX9Device,
                                        Usage.Dynamic | Usage.WriteOnly, 
                                        CustomVertex.TransformedTextured.Format,
                                        GUIGraphicsContext.GetTexturePoolType());
        _wx = _wy = _wwidth = _wheight = 0;
      }

      if (_wx != wx || _wy != wy || _wwidth != wwidth || _wheight != wheight)
      {
        CustomVertex.TransformedTextured[] verts = (CustomVertex.TransformedTextured[])_vertexBuffer.Lock(0, 0);

        // upper left
        verts[0] = new CustomVertex.TransformedTextured(wx, wy, 0, 1, 0, 0);

        // upper right
        verts[1] = new CustomVertex.TransformedTextured(wx + wwidth, wy, 0, 1, 1, 0);

        // lower left
        verts[2] = new CustomVertex.TransformedTextured(wx, wy + wheight, 0, 1, 0, 1);

        // lower right
        verts[3] = new CustomVertex.TransformedTextured(wx + wwidth, wy + wheight, 0, 1, 1, 1);

        _vertexBuffer.SetData(verts, 0, LockFlags.None);
        _vertexBuffer.Unlock();
        
        // remember what the vertexBuffer is set to
        _wy = wy;
        _wx = wx;
        _wheight = wheight;
        _wwidth = wwidth;
      }
    }
  }
}