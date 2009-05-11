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
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using TvDatabase;
using TvControl;

using Gentle.Common;
using Gentle.Framework;
namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvOverlay : GUIOverlayWindow, IRenderLayer
  {
    DateTime _updateTimer = DateTime.Now;		
    bool _lastStatus = false;
    bool _didRenderLastTime = false;
    public TvOverlay()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TV_OVERLAY;
    }
    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TV_OVERLAY, this);
      Restore();
      PreInit();
      ResetAllControls();
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.TvOverlay);
    }
    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\tvOverlay.xml");
      GetID = (int)GUIWindow.Window.WINDOW_TV_OVERLAY;
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

    void OnUpdateState(bool render)
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
      if (GUIGraphicsContext.IsFullScreenVideo) return false;
      //{
      //  OnUpdateState(false);
      //  return base.IsAnimating(AnimationType.WindowClose);
      //}      
      
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds < 1000) return _lastStatus;

      if (!TVHome.Connected)
      {        
        return false;
      }

      TvServer server = new TvServer();
      if (!TVHome.HandleServerNotConnected())
      {
        _lastStatus = server.IsAnyCardRecording();
      }
      else
      {
        _lastStatus = false;
      }
      _updateTimer = DateTime.Now;
      OnUpdateState(_lastStatus);
      if (!_lastStatus) 
        return base.IsAnimating(AnimationType.WindowClose);
      else 
        return _lastStatus;
    }

    public void RenderLayer(float timePassed)
    {
      if (GUIGraphicsContext.IsFullScreenVideo) return ;
      Render(timePassed);			
    }
    #endregion
  }
}
