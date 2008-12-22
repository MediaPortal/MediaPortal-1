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
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.GUI.Pictures;
using MediaPortal.Configuration;

using TvDatabase;

using Gentle.Common;
using Gentle.Framework;

namespace TvPlugin {
  /// <summary>
  /// Fullscreen teletext window of TVE3
  /// </summary>
  public class TVTeletextFullScreen : TvTeletextBase, IRenderLayer {
    #region variables
    bool _isFullScreenVideo = false;
    #endregion

    #region ctor
    public TVTeletextFullScreen() {
      GetID = (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT;
    }
    #endregion

    #region GUIWindow initializing methods
    public override bool Init() {
      return Load(GUIGraphicsContext.Skin + @"\myfsteletext.xml");
    }

    public override void OnAdded() {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT, this);
      Restore();
      PreInit();
      ResetAllControls();
    }

    protected override void OnPageDestroy(int newWindowId) {
      SaveSettings();
      _updateThreadStop = true;
      TVHome.Card.GrabTeletext = false;
      GUILayerManager.UnRegisterLayer(this);
      GUIGraphicsContext.IsFullScreenVideo = _isFullScreenVideo;
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad() {
      _isFullScreenVideo = GUIGraphicsContext.IsFullScreenVideo;
      base.OnPageLoad();
      int left = GUIGraphicsContext.Width / 20;
      int top = GUIGraphicsContext.Height / 20;
      if (imgTeletextForeground != null) {
        imgTeletextForeground.Width = GUIGraphicsContext.Width - (2 * left);
        imgTeletextForeground.Height = GUIGraphicsContext.Height - (2 * top);
        imgTeletextForeground.SetPosition(left, top);
      }
      if (imgTeletextBackground != null) {
        imgTeletextBackground.Width = GUIGraphicsContext.Width - (2 * left);
        imgTeletextBackground.Height = GUIGraphicsContext.Height - (2 * top);
        imgTeletextBackground.SetPosition(left, top);
      }
      InitializeWindow(true);
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);
    }
    #endregion

    #region OnAction
    public override void OnAction(Action action) {
      switch (action.wID) {
        case Action.ActionType.ACTION_SWITCH_TELETEXT_TRANSPARENT:
          // Switch tranparent mode
          _transparentMode = !_transparentMode;
          _renderer.TransparentMode = _transparentMode;
          // Rerender the image
          _numberOfRequestedUpdates++;
          break;
      }
      base.OnAction(action);
    }
    #endregion

    #region Rendering method
    public override void Render(float timePassed) {
      // Force the fullscreen video
      GUIGraphicsContext.IsFullScreenVideo = true;
      // Only the render one of the images, if no update is running
      if (!_updatingForegroundImage) {
        imgTeletextForeground.Render(timePassed);
      }
      if (!_updatingBackgroundImage) {
        imgTeletextBackground.Render(timePassed);
      }
    }
    #endregion

    #region IRenderLayer
    public bool ShouldRenderLayer() {
      //TVHome.SendHeartBeat(); //not needed, now sent from tvoverlay.cs
      return true;
    }

    public void RenderLayer(float timePassed) {
      Render(timePassed);
    }
    #endregion

  }// class
}// namespace
