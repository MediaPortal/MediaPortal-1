#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using MediaPortal.TV.Recording;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// 
  /// </summary>
  public class GUITVOverlay : GUIOverlayWindow, IRenderLayer
  {
    private bool _didRenderLastTime = false;

    public GUITVOverlay()
    {
      GetID = (int) Window.WINDOW_TV_OVERLAY;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\tvOverlay_TVE2.xml");
      GetID = (int) Window.WINDOW_TV_OVERLAY;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.TvOverlay);
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

    private void OnUpdateState(bool render)
    {
      if (_didRenderLastTime != render)
      {
        _didRenderLastTime = render;
        if (render)
        {
          QueueAnimation(AnimationType.WindowOpen);
        }
        else
        {
          QueueAnimation(AnimationType.WindowClose);
        }
      }
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        OnUpdateState(false);
        return base.IsAnimating(AnimationType.WindowClose);
      }
      if (Recorder.IsAnyCardRecording())
      {
        OnUpdateState(true);
        return true;
      }

      OnUpdateState(false);
      return base.IsAnimating(AnimationType.WindowClose);
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    #endregion
  }
}