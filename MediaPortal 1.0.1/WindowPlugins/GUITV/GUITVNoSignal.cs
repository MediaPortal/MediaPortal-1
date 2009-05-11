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
using MediaPortal.Player;
using MediaPortal.TV.Recording;

namespace WindowPlugins.GUITV
{
  /// <summary>
  /// Summary description for GUITVNoSignal.
  /// </summary>
  public class GUITVNoSignal : GUIWindow, IRenderLayer
  {
    [SkinControl(102)] protected GUILabelControl lblNotify = null;
    [SkinControl(1)] protected GUIProgressControl progressControl = null;
    private string notify = string.Empty;

    public GUITVNoSignal()
    {
      GetID = (int) Window.WINDOW_TV_NO_SIGNAL;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvNoSignal.xml");
      return bResult;
    }

    public override void Process()
    {
      if (VideoRendererStatistics.IsVideoFound)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_TVFULLSCREEN, true);
        return;
      }
      progressControl.Percentage = Recorder.SignalStrength;
      progressControl.IsVisible = true;
      switch (VideoRendererStatistics.VideoState)
      {
        case VideoRendererStatistics.State.NoSignal:
          notify = GUILocalizeStrings.Get(1034);
          break;
        case VideoRendererStatistics.State.Scrambled:
          notify = GUILocalizeStrings.Get(1035);
          break;
        case VideoRendererStatistics.State.Signal:
          notify = GUILocalizeStrings.Get(1036);
          break;
      }
      lblNotify.Label = notify;
    }

    public string Notify
    {
      set { notify = value; }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      GUILayerManager.UnRegisterLayer(this);
      base.OnPageDestroy(new_windowId);
    }

    protected override void OnPageLoad()
    {
      GUIGraphicsContext.IsFullScreenVideo = true;

      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);
      base.OnPageLoad();
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_SHOW_GUI:
          GUIWindowManager.ShowPreviousWindow();
          return;
        case Action.ActionType.ACTION_PLAY:
          if (g_Player.Playing)
          {
            g_Player.Play(g_Player.CurrentFile);
          }
          return;
      }
      base.OnAction(action);
    }

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
  }
}