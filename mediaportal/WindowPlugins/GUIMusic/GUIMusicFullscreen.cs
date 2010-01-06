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

using System.IO;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// GUIWindow used for fullscreen music visualizations. Note that the visualization isn't actually rendered on this window.
  /// This window serves as a back drop for the System.Windows.Forms.UserControl based visualization window and helps
  /// ensure that we don't popup any context menus or other GUI controls over the visualization UserControl.
  /// </summary>
  public class GUIMusicFullscreen : GUIInternalWindow, IRenderLayer
  {
    private PlayListPlayer playlistPlayer;

    public override bool IsOverlayAllowed
    {
      get { return false; }
    }

    public GUIMusicFullscreen()
    {
      GetID = (int)Window.WINDOW_FULLSCREEN_MUSIC;
      playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    public override bool Init()
    {
      bool bResult = Load(Path.Combine(Application.StartupPath,
                                       GUIGraphicsContext.Skin + @"\musicFullScreen.xml"));

      GetID = (int)Window.WINDOW_FULLSCREEN_MUSIC;
      return bResult;
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
          ////case Action.ActionType.ACTION_PREV_ITEM:
          ////    {
          ////        //g_playlistPlayer.PlayPrevious();
          ////    }
          ////    break;

          ////case Action.ActionType.ACTION_NEXT_ITEM:
          ////    {
          ////        //g_playlistPlayer.PlayNext();
          ////    }
          ////    break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
        case Action.ActionType.ACTION_MOUSE_CLICK:
        case Action.ActionType.ACTION_SHOW_GUI:
          {
            GUIGraphicsContext.IsFullScreenVideo = false;
            GUIWindowManager.ShowPreviousWindow();
            return;
          }

        case Action.ActionType.ACTION_MOVE_DOWN:
        case Action.ActionType.ACTION_BIG_STEP_BACK:
          {
            double currentpos = g_Player.CurrentPosition;
            double duration = g_Player.Duration;
            double percent = (currentpos / duration) * 100d;
            percent -= 10d;

            if (percent < 0)
            {
              percent = 0;
            }

            g_Player.SeekAsolutePercentage((int)percent);
            return;
          }

        case Action.ActionType.ACTION_MOVE_UP:
        case Action.ActionType.ACTION_BIG_STEP_FORWARD:
          {
            double currentpos = g_Player.CurrentPosition;
            double duration = g_Player.Duration;
            double percent = (currentpos / duration) * 100d;
            percent += 10d;

            if (percent > 100d)
            {
              percent = 100d;
            }

            g_Player.SeekAsolutePercentage((int)percent);
            return;
          }

        case Action.ActionType.ACTION_STOP:
          {
            Log.Info("GUIMusicFullscreen:stop");
            g_Player.Stop();
            GUIWindowManager.ShowPreviousWindow();
          }
          break;

        case Action.ActionType.ACTION_PAUSE:
          g_Player.Pause();
          break;

        case Action.ActionType.ACTION_SMALL_STEP_BACK:
          {
            if (g_Player.CanSeek)
            {
              // seek back 5 sec
              double dPos = g_Player.CurrentPosition;

              if (dPos > 5)
              {
                g_Player.SeekAbsolute(dPos - 5.0d);
              }
            }
          }
          break;

        case Action.ActionType.ACTION_PLAY:
        case Action.ActionType.ACTION_MUSIC_PLAY:
          {
            g_Player.StepNow();
            g_Player.Speed = 1;

            if (g_Player.Paused)
            {
              g_Player.Pause();
            }
          }
          break;
      }

      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            base.OnMessage(message);
            GUILayerManager.UnRegisterLayer(this);
            GUIGraphicsContext.IsFullScreenVideo = false;
            return true;
          }
      }

      return base.OnMessage(message);
    }

    public override void Process()
    {
      if (!g_Player.Playing)
      {
        if (playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC ||
            playlistPlayer.CurrentPlaylistType == PlayListType.PLAYLIST_MUSIC_TEMP)
        {
          return;
        }

        GUIWindowManager.ShowPreviousWindow();
        return;
      }
    }

    public override void OnDeviceRestored()
    {
      base.OnDeviceRestored();

      if (GUIGraphicsContext.Fullscreen && g_Player.IsMusic && g_Player.Playing)
      {
        // Make sure we don't go into fullscreen exclusive mode!
        Log.Info("GUIMusicFullscreen Window: Disabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
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