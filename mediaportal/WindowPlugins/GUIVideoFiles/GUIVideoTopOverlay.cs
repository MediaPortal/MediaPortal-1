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

using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIVideoTopOverlay : GUIOverlayWindow, IRenderLayer
  {
    [SkinControl(4)] protected GUIImage imagePause = null;
    [SkinControl(5)] protected GUIImage imageFastForward = null;
    [SkinControl(6)] protected GUIImage imageRewind = null;

    public GUIVideoTopOverlay()
    {
      GetID = (int) Window.WINDOW_VIDEO_OVERLAY_TOP;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\videoOverlayTop.xml");
      GetID = (int) Window.WINDOW_VIDEO_OVERLAY_TOP;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.TopOverlay);
      return bResult;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void PreInit()
    {
      base.PreInit();
      AllocResources();
    }

    public override void Render(float timePassed)
    {
    }

    public override bool DoesPostRender()
    {
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        return false;
      }
      if (GUIGraphicsContext.Calibrating)
      {
        return false;
      }
      if (!GUIGraphicsContext.Overlay)
      {
        return false;
      }
      if (!g_Player.Playing)
      {
        return false;
      }
      if (g_Player.IsMusic)
      {
        return false;
      }

      return true;
    }

    public override void PostRender(float timePassed, int iLayer)
    {
      imagePause.Visible = g_Player.Paused;
      imageRewind.Visible = (g_Player.Speed < 0);
      imageFastForward.Visible = (g_Player.Speed > 1);
      base.Render(timePassed);
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return DoesPostRender();
    }

    public void RenderLayer(float timePassed)
    {
      PostRender(timePassed, 2);
    }

    #endregion
  }
}