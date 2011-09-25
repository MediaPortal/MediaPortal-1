#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.GUI.Library;
using TvControl;

namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvOverlay : GUIInternalOverlayWindow, IRenderLayer
  {
    private DateTime _updateTimer = DateTime.Now;
    private bool _lastStatus = false;
    private bool _didRenderLastTime = false;

    public TvOverlay()
    {
      GetID = (int)Window.WINDOW_TV_OVERLAY;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\tvOverlay.xml");
      GetID = (int)Window.WINDOW_TV_OVERLAY;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.TvOverlay);
      return bResult;
    }

    public override void PreInit()
    {
      base.PreInit();
      AllocResources();
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    #region IRenderLayer

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

    public bool ShouldRenderLayer()
    {
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        return false;
      }
      //{
      //  OnUpdateState(false);
      //  return base.IsAnimating(AnimationType.WindowClose);
      //}      

      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds < 1000)
      {
        return _lastStatus;
      }

      _lastStatus = TVHome.IsAnyCardRecording;

      _updateTimer = DateTime.Now;
      OnUpdateState(_lastStatus);
      if (!_lastStatus)
      {
        return base.IsAnimating(AnimationType.WindowClose);
      }
      else
      {
        return _lastStatus;
      }
    }

    public void RenderLayer(float timePassed)
    {
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        return;
      }
      Render(timePassed);
    }

    #endregion
  }
}