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
  public sealed class BDOSDRenderer
  {
    private enum BD_OVERLAY_PLANE
    {
      BD_OVERLAY_PG = 0,  /* Presentation Graphics plane */
      BD_OVERLAY_IG = 1,  /* Interactive Graphics plane (on top of PG plane) */
    }

    private static BDOSDRenderer _instance;
    private static bool _render = false;
    private readonly static object _instanceLock = new Object();

    /// <summary>
    /// The coordinates of current vertex buffer
    /// </summary>
    private int _wx, _wy, _wwidth, _wheight;

    /// <summary>
    /// Vertex buffer for rendering OSD (shared between planes)
    /// </summary>
    private VertexBuffer _vertexBuffer;

    /// <summary>
    /// Array containing interactive and presentation graphics overlay textures
    /// </summary>
    private Texture[] _planes;
    
    /// <summary>
    /// Lock for syncronising the texture update and rendering
    /// </summary>
    private object _OSDLock;

    private BDOSDRenderer()
    {
      _OSDLock = new Object();
      _planes = new Texture[2];
    }

    public static BDOSDRenderer GetInstance()
    {
      if (_instance == null)
      {
        lock (_instanceLock)
        {
          if (_instance == null)
          {
            _instance = new BDOSDRenderer();
          }
        }
      }
      return _instance;
    }

    public static void StartRendering()
    {
      lock (_instanceLock)
      {
        _render = true;
      }
    }

    public static void StopRendering()
    {
      if (_instance != null)
      {
        lock (_instanceLock)
        {
          if (_instance != null)
          {
            _instance.ReleaseTextures();
          }
        }
      }
    }

    public void DrawItem(OSDTexture item)
    {
      try
      {
        lock (_instanceLock)
        {
          if (!_render)
          {
            return;
          }

          if (item.texture != null && item.width > 0 && item.height > 0)
          {
            if (_planes[item.plane] == null)
            {
              _planes[item.plane] = new Texture(item.texture);
            }
          }
          else
          {
            _planes[item.plane] = null;
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
      lock (_instanceLock)
      {
        if (!_render)
        {
          return;
        }

        // Store current settings so they can be restored when we are done
        VertexFormats vertexFormat = GUIGraphicsContext.DX9Device.VertexFormat;
        
        try
        {
          if ((_planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_PG] == null || _planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_PG].Disposed) &&
              (_planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_IG] == null || _planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_IG].Disposed))
          {
            return;
          }

          int wx = 0, wy = 0, wwidth = 0, wheight = 0;
          if (GUIGraphicsContext.IsFullScreenVideo)
          {
            wheight = GUIGraphicsContext.Height;
            wwidth = GUIGraphicsContext.Width;

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

          DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_ALPHABLENDENABLE, 1);
          CreateVertexBuffer(wx, wy, wwidth, wheight);

          // Make sure D3D objects haven't been disposed for some reason. This would cause
          // an access violation on native side, causing Skin Engine to halt rendering
          
          if (_planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_PG] != null && !_planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_PG].Disposed && !_vertexBuffer.Disposed)
          {
            GUIGraphicsContext.DX9Device.SetStreamSource(0, _vertexBuffer, 0);
            GUIGraphicsContext.DX9Device.SetTexture(0, _planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_PG]);
            GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedTextured.Format;
            GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
          }

          if (_planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_IG] != null && !_planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_IG].Disposed && !_vertexBuffer.Disposed)
          {
            GUIGraphicsContext.DX9Device.SetStreamSource(0, _vertexBuffer, 0);
            GUIGraphicsContext.DX9Device.SetTexture(0, _planes[(int)BD_OVERLAY_PLANE.BD_OVERLAY_IG]);
            GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedTextured.Format;
            GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
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

    private void ReleaseTextures()
    {
      _render = false;
      _planes[0] = null;
      _planes[1] = null;
    }
  }
}