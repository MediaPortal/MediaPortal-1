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

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;

namespace TvPlugin
{
  /// <summary>
  /// Fullscreen teletext window of TVE3
  /// </summary>
  public class TVTeletextFullScreen : TvTeletextBase, IRenderLayer
  {
    #region variables

    private bool _isFullScreenVideo;

    #endregion

    #region ctor

    public TVTeletextFullScreen()
    {
      GetID = (int)Window.WINDOW_FULLSCREEN_TELETEXT;
    }

    #endregion

    #region GUIWindow initializing methods

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\myfsteletext.xml");
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();
      Join();
      TVHome.Card.GrabTeletext = false;
      GUILayerManager.UnRegisterLayer(this);
      GUIGraphicsContext.IsFullScreenVideo = _isFullScreenVideo;
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      _isFullScreenVideo = GUIGraphicsContext.IsFullScreenVideo;
      base.OnPageLoad();
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
          _renderer.TransparentMode = _transparentMode;
          // Rerender the image
          RequestUpdate(false);
          break;
        case Action.ActionType.ACTION_CONTEXT_MENU:
          ShowContextMenu();
          return;
      }
      base.OnAction(action);
    }

    #endregion

    #region Context Menu
    private void ShowContextMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null || _renderer == null)
      {
        GUIWindowManager.ShowPreviousWindow();
        return;
      }

      dlg.Reset();
      dlg.SetHeading(1441); // Teletext

      dlg.AddLocalizedString(1439); // Change default language
      dlg.AddLocalizedString(970); // Previous window

      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
        return;
      switch (dlg.SelectedId)
      {
        case 1439: // Change default language
          {
            dlg.Reset();
            dlg.SetHeading(1438); // Change default Teletext language

            dlg.AddLocalizedString(1400); // Latin
            dlg.AddLocalizedString(1401); // Latin / Polish
            dlg.AddLocalizedString(1402); // Latin / Turkish
            dlg.AddLocalizedString(1403); // Latin: sb/cr/sl/ro
            dlg.AddLocalizedString(1404); // Cyrilic
            dlg.AddLocalizedString(1405); // Greek / Turkish
            dlg.AddLocalizedString(1406); // Arabic
            dlg.AddLocalizedString(1407); // Hebrew / Arabic

            if (DefaultCharSetDesignation <= 4 && DefaultCharSetDesignation >= 0)
              dlg.SelectedLabel = DefaultCharSetDesignation;
            else if (DefaultCharSetDesignation == 6)
              dlg.SelectedLabel = 5;
            else if (DefaultCharSetDesignation == 8)
              dlg.SelectedLabel = 6;
            else if (DefaultCharSetDesignation == 10)
              dlg.SelectedLabel = 7;

            dlg.DoModal(GetID);

            if (dlg.SelectedId == -1)
              return;
            switch (dlg.SelectedId)
            {
              case 1400:
                DefaultCharSetDesignation = 0;
                break;
              case 1401:
                DefaultCharSetDesignation = 1;
                break;
              case 1402:
                DefaultCharSetDesignation = 2;
                break;
              case 1403:
                DefaultCharSetDesignation = 3;
                break;
              case 1404:
                DefaultCharSetDesignation = 4;
                break;
              case 1405:
                DefaultCharSetDesignation = 6;
                break;
              case 1406:
                DefaultCharSetDesignation = 8;
                break;
              case 1407:
                DefaultCharSetDesignation = 10;
                break;
            }
            //SaveSettings();
            return;
          }
        case 970: // Previous window
          GUIWindowManager.ShowPreviousWindow();
          return;
      }
    }
    #endregion

    #region Rendering method

    public override void Render(float timePassed)
    {
      // Force the fullscreen video
      GUIGraphicsContext.IsFullScreenVideo = true;

      // Only the render one of the images
      if (!_redrawForeground)
      {
        imgTeletextForeground.Render(timePassed);
      }
      else
      {
        imgTeletextBackground.Render(timePassed);
      }
    }

    #endregion

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      //TVHome.SendHeartBeat(); //not needed, now sent from tvoverlay.cs
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    #endregion
  }

  // class
}

// namespace