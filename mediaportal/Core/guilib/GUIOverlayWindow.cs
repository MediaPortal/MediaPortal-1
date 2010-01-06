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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Summary description for GUIOverlayWindow.
  /// </summary>
  public class GUIOverlayWindow : GUIWindow
  {
    public GUIOverlayWindow()
    {
      GUIWindowManager.OnPostRenderAction += new GUIWindowManager.PostRenderActionHandler(OnPostRenderAction);
    }

    // Moved to Ctor to prevent multiple event registrations (fixes http://mantis.team-mediaportal.com/view.php?id=724 )
    // public override void PreInit()
    // {
    // 	base.PreInit ();
    // 	//GUIWindowManager.OnPostRender+=new MediaPortal.GUI.Library.GUIWindowManager.PostRendererHandler(OnPostRender);
    // 	GUIWindowManager.OnPostRenderAction+=new MediaPortal.GUI.Library.GUIWindowManager.PostRenderActionHandler(OnPostRenderAction);
    // }

    /// <summary>
    /// PostRender() gives the window the oppertunity to overlay itself ontop of
    /// the other window(s)
    /// It gets called at the end of every rendering cycle even 
    /// if the window is not activated
    /// <param name="iLayer">indicates which overlay layer is rendered (1-10)
    /// this gives the plugins the oppertunity to tell which overlay layer they are using
    /// For example the topbar is rendered on layer #1
    /// while the music overlay is rendered on layer #2 (and thus on top of the topbar)</param>
    /// </summary>
    public virtual void PostRender(float timePassed, int iLayer) {}

    /// <summary>
    /// Returns wither or not the window does postrendering.
    /// </summary>
    /// <returns>false</returns>
    public virtual bool DoesPostRender()
    {
      return false;
    }

    private void OnPostRender(int level, float timePassed)
    {
      if (DoesPostRender())
      {
        PostRender(timePassed, level);
      }
    }

    private int OnPostRenderAction(Action action, GUIMessage msg, bool focus)
    {
      if (msg != null)
      {
        if (msg.Message == GUIMessage.MessageType.GUI_MSG_LOSTFOCUS ||
            msg.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS)
        {
          if (Focused)
          {
            if (DoesPostRender())
            {
              OnMessage(msg);
              return (int)(Focused ? GUIWindowManager.FocusState.FOCUSED : GUIWindowManager.FocusState.NOT_FOCUSED);
            }
          }
        }
      }

      if (action != null)
      {
        if (action.wID == Action.ActionType.ACTION_MOVE_LEFT ||
            action.wID == Action.ActionType.ACTION_MOVE_RIGHT ||
            action.wID == Action.ActionType.ACTION_MOVE_UP ||
            action.wID == Action.ActionType.ACTION_MOVE_DOWN ||
            action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          if (Focused)
          {
            if (DoesPostRender())
            {
              bool foc = Focused;
              OnAction(action);
              return
                (int)
                (Focused
                   ? GUIWindowManager.FocusState.FOCUSED
                   : (foc == Focused
                        ? GUIWindowManager.FocusState.NOT_FOCUSED
                        : GUIWindowManager.FocusState.JUST_LOST_FOCUS));
            }
          }
        }
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
        {
          if (DoesPostRender())
          {
            OnAction(action);
          }
        }
      }
      if (focus && msg == null)
      {
        if (DoesPostRender())
        {
          if (ShouldFocus(action))
          {
            Focused = true;
            return (int)GUIWindowManager.FocusState.FOCUSED;
          }
        }
        Focused = false;
      }
      return (int)GUIWindowManager.FocusState.NOT_FOCUSED;
    }

    protected virtual bool ShouldFocus(Action action)
    {
      return false;
    }

    protected override void OnMouseMove(int cx, int cy, Action action)
    {
      for (int i = Children.Count - 1; i >= 0; i--)
      {
        GUIControl control = (GUIControl)Children[i];
        bool bFocus;
        int controlID;
        if (control.HitTest(cx, cy, out controlID, out bFocus))
        {
          if (!bFocus)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, controlID, 0, 0, null);
            OnMessage(msg);
            control.HitTest(cx, cy, out controlID, out bFocus);
          }
          control.OnAction(action);
          return;
        }
        else
        {
          // no control selected
          control.Focus = false;
        }
      }
      Focused = false;
    }
  }
}