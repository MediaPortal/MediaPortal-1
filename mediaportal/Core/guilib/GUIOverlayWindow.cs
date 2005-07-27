using System;


namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for GUIOverlayWindow.
	/// </summary>
	public class GUIOverlayWindow : GUIWindow
	{
		public override void PreInit()
		{
			base.PreInit ();
			GUIWindowManager.OnPostRender+=new MediaPortal.GUI.Library.GUIWindowManager.PostRendererHandler(OnPostRender);
			GUIWindowManager.OnPostRenderAction+=new MediaPortal.GUI.Library.GUIWindowManager.PostRenderActionHandler(OnPostRenderAction);
		}

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
		public virtual void PostRender(float timePassed,int iLayer)
		{
		}

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
				PostRender(timePassed,level);
			}
		}

		private bool OnPostRenderAction(Action action, GUIMessage msg, bool focus)
		{
			if (msg!=null)
			{
				if (msg.Message==GUIMessage.MessageType.GUI_MSG_LOSTFOCUS||msg.Message==GUIMessage.MessageType.GUI_MSG_SETFOCUS)
				{
					if (Focused)
					{
						if (DoesPostRender())
						{
							OnMessage(msg);
							return Focused;
						}
					}
				}
			}
			
			if (action!=null)
			{
				if( action.wID == Action.ActionType.ACTION_MOVE_LEFT ||
					action.wID == Action.ActionType.ACTION_MOVE_RIGHT ||
					action.wID == Action.ActionType.ACTION_MOVE_UP ||
					action.wID == Action.ActionType.ACTION_MOVE_DOWN ||
					action.wID == Action.ActionType.ACTION_SELECT_ITEM)
				{
					if (Focused)
					{
						if (DoesPostRender())
						{
							OnAction(action);
							return Focused;
						}
					}
				}
				if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
				{
					if (DoesPostRender())
					{
						OnAction(action);
					}
				}
			}
			if (focus&&msg==null)
			{
				if (DoesPostRender())
				{
					if (ShouldFocus(action))
					{
						Focused=true;
						return true;
					}
				}
				Focused=false;
			}
			return false;
		}
		protected virtual bool ShouldFocus(Action action)
		{
			return false;
		}
	}
}
