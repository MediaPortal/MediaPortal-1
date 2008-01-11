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
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Player;
namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITVOverlay:GUIOverlayWindow, IRenderLayer
  {
    bool _didRenderLastTime = false;
    public GUITVOverlay()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_OVERLAY;
		}

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\tvOverlay.xml");
      GetID = (int)GUIWindow.Window.WINDOW_TV_OVERLAY;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.TvOverlay);
      return bResult;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false;}
    }    
    
    public override void PreInit()
		{
			base.PreInit();
      AllocResources();
    
    }

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
