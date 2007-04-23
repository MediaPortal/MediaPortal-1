#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
    #region Properties (Skin)
    [SkinControlAttribute(4)]
    protected GUIImage imgRed = null;
    [SkinControlAttribute(5)]
    protected GUIImage imgYellow = null;
    [SkinControlAttribute(6)]
    protected GUILabelControl lblState = null;
    #endregion

    static TvOverlay _instance;

    /// <summary>
    /// returns an the <see cref="T:TvControl.IController"/> interface to the tv server
    /// </summary>
    /// <value>The instance.</value>
    static public TvOverlay Instance
    {
      get
      {
        return _instance;
      }
    }


    DateTime _updateTimer = DateTime.Now;
    bool _lastStatus = false;
    bool _forceRed = false;
    int _lastMode = 0;
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
      _instance = this;
      return bResult;
    }

    public virtual void DeInit()
    {
      _instance = null;
      base.DeInit();
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

    private void SetBulb()
    {
      if (imgRed != null) imgRed.Visible = _lastMode == 1 || _forceRed;
      if (imgYellow != null) imgYellow.Visible = _lastMode == 2 && !_forceRed;
    }

    /// <summary>
    /// mode= 0 : not visible
    /// mode= 1 : red logo
    /// mode= 2 : yellow logo
    /// mode= 3 : no logo, text only
    /// text: text for state
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="text"></param>
    public void UpdateState(int mode, String text)
    {
      _lastMode = mode;
      SetBulb();
      if (lblState != null) lblState.Label = text == null ? "" : text;
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

      // check TV-Server
      TvServer server = new TvServer();
      bool recording = false;
      if (TVHome.Connected)
        recording= server.IsAnyCardRecording();

      bool setBulb = _forceRed != recording;

      _forceRed = recording;
      _lastStatus= recording || _lastMode != 0;
      _updateTimer = DateTime.Now;

      if (setBulb) SetBulb();
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
