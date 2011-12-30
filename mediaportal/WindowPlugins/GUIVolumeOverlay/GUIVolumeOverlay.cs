﻿using System;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace WindowPlugins.GUIVolumeOverlay
{
  public class GUIMixerOverlay : GUIInternalOverlayWindow, IRenderLayer
  {
    [SkinControl(501)]
    protected GUIImage _imgBG = null;
    [SkinControl(502)]
    protected GUIImage _imgMute = null;
    [SkinControl(503)]
    protected GUIVolumeBar _volumeBar = null;

    public GUIMixerOverlay()
    {
      GetID = 3004;
    }

    public override bool Init()
    {
        bool result = Load(GUIGraphicsContext.Skin + @"\VolumeOverlay.xml");
        GetID = 3004;
        GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Volume);
        return result;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override bool DoesPostRender()
    {
      //if (!m_bEnabled || // SE - TODO: setting
      if(
         !GUIGraphicsContext.VolumeOverlay ||
         GUIGraphicsContext.DisableVolumeOverlay ||
         GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_MOVIE_CALIBRATION ||
         GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_UI_CALIBRATION)
      {
        GUIGraphicsContext.VolumeOverlay = false;
        return false;
      }
      return true;
    }

    public override void PostRender(float timePassed, int iLayer)
    {
      if (iLayer != 3)
      {
        GUIGraphicsContext.VolumeOverlay = false;
        return;
      }
      if (_volumeBar == null || _imgMute == null || _imgBG == null)
      {
        GUIGraphicsContext.VolumeOverlay = false;
        return;
      }
      TimeSpan ts = DateTime.Now - GUIGraphicsContext.VolumeOverlayTimeOut;
      //if ((ts.TotalSeconds > m_iAutoHideTimeOut)) // SE - TODO: setting
      if ((ts.TotalSeconds > 10))
      {
        GUIGraphicsContext.VolumeOverlay = false;
        return;
      }
      //if (m_FullScreenOnly && !GUIGraphicsContext.IsFullScreenVideo) // SE - TODO: setting
      if (false && !GUIGraphicsContext.IsFullScreenVideo) // SE - TODO: setting
      {
        GUIGraphicsContext.VolumeOverlay = false;
        return;
      }
      if (base.IsAnimating(AnimationType.WindowClose))
      {
          Render(timePassed);
          return;
      }

      foreach (CPosition pos in _listPositions)
      {
        pos.control.SetPosition(pos.XPos + +GUIGraphicsContext.VolumeOverlayOffsetX, pos.YPos + GUIGraphicsContext.VolumeOverlayOffsetY);
      }

      if (VolumeHandler.Instance.IsMuted)
      {
        _volumeBar.Maximum = VolumeHandler.Instance.StepMax;
        _volumeBar.Current = 0;
        if (!_imgBG.IsEffectAnimating(AnimationType.Hidden))
          _imgBG.Visible = false;
        _imgMute.Visible = true;
        _volumeBar.Image1 = 1;
      }
      else
      {
        _volumeBar.Maximum = VolumeHandler.Instance.StepMax;
        _volumeBar.Current = VolumeHandler.Instance.Step;
        if (!_imgMute.IsEffectAnimating(AnimationType.Hidden))
          _imgMute.Visible = false;
        _imgBG.Visible = true;
        _volumeBar.Image1 = 2;
        _volumeBar.Image2 = 1;
      }

      Render(timePassed);
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return DoesPostRender();
    }

    public void RenderLayer(float timePassed)
    {
      PostRender(timePassed, 3);
    }

    #endregion
  }
}