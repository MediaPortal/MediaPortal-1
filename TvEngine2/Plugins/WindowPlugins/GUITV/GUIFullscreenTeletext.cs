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
using MediaPortal.TV.Teletext;

namespace MediaPortal.GUI.TV
{
  /// <summary>
  /// Fullscreen teletext window
  /// </summary>
  public class GUITVFullscreenTeletext : GUITVTeletextBase, IRenderLayer
  {
    #region variables

    private bool _isFullScreenVideo = false;

    #endregion

    #region ctor

    public GUITVFullscreenTeletext()
    {
      GetID = (int) Window.WINDOW_FULLSCREEN_TELETEXT;
    }

    #endregion

    #region GUIWindow initializing methods

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\myfsteletext_TVE2.xml");
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      // Save the settings and then stop the update thread. Also the teletext grabbing
      SaveSettings();
      _updateThreadStop = true;

      TeletextGrabber.Grab = false;
      TeletextGrabber.TeletextCache.PageUpdatedEvent -= new DVBTeletext.PageUpdated(dvbTeletextParser_PageUpdatedEvent);

      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            Recorder.StopViewing();
          }
        }
      }
      GUILayerManager.UnRegisterLayer(this);
      GUIGraphicsContext.IsFullScreenVideo = _isFullScreenVideo;
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      _isFullScreenVideo = GUIGraphicsContext.IsFullScreenVideo;
      base.OnPageLoad();
      int left = GUIGraphicsContext.Width/20;
      int top = GUIGraphicsContext.Height/20;
      if (imgTeletextFirst != null)
      {
        imgTeletextFirst.Width = GUIGraphicsContext.Width - (2*left);
        imgTeletextFirst.Height = GUIGraphicsContext.Height - (2*top);
        imgTeletextFirst.SetPosition(left, top);
      }
      if (imgTeletextSecond != null)
      {
        imgTeletextSecond.Width = GUIGraphicsContext.Width - (2*left);
        imgTeletextSecond.Height = GUIGraphicsContext.Height - (2*top);
        imgTeletextSecond.SetPosition(left, top);
      }
      InitializeWindow(true);
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);
    }

    #endregion

    #region OnAction

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_SWITCH_TELETEXT_TRANSPARENT:
          // Switch tranparent mode
          _transparentMode = !_transparentMode;
          TeletextGrabber.TeletextCache.TransparentMode = _transparentMode;
          // Rerender the image
          _numberOfRequestedUpdates++;
          break;
      }
      base.OnAction(action);
    }

    #endregion

    #region Rendering method

    public override void Render(float timePassed)
    {
      // Force the fullscreen video
      GUIGraphicsContext.IsFullScreenVideo = true;
      // Only render one of the images
      if (_updateFirst)
      {
        imgTeletextFirst.Render(timePassed);
      }
      else
      {
        imgTeletextSecond.Render(timePassed);
      }
    }

    #endregion

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    #endregion
  } // class
} // namespace