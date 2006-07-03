using System;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Player;
namespace WindowPlugins.GUITV
{
	/// <summary>
	/// Summary description for GUITVNoSignal.
	/// </summary>
	public class GUITVNoSignal : GUIWindow, IRenderLayer
	{
		[SkinControlAttribute(102)]			  protected GUILabelControl lblNotify=null;
		[SkinControlAttribute(1)]			  protected GUIProgressControl progressControl=null;
		string notify=String.Empty;
		public GUITVNoSignal()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_NO_SIGNAL;
		}
		public override bool Init()
		{
			bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvNoSignal.xml");
			return bResult;
		}
		public override void Process()
		{
			if (VideoRendererStatistics.IsVideoFound)
			{
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN,true);
				return;
			}
			progressControl.Percentage=Recorder.SignalStrength;
			progressControl.IsVisible=true;
			switch (VideoRendererStatistics.VideoState)
			{
				case VideoRendererStatistics.State.NoSignal:
					notify=GUILocalizeStrings.Get(1034);
					break;
				case VideoRendererStatistics.State.Scrambled:
					notify=GUILocalizeStrings.Get(1035);
					break;
				case VideoRendererStatistics.State.Signal:
					notify=GUILocalizeStrings.Get(1036);
					break;
			}
			lblNotify.Label=notify;
		}
		public string Notify
		{
			set
			{
				notify=value;
			}
		}
    protected override void OnPageDestroy(int new_windowId)
    {
      GUILayerManager.UnRegisterLayer(this);
      base.OnPageDestroy(new_windowId);
    }
		protected override void OnPageLoad()
		{
			GUIGraphicsContext.IsFullScreenVideo=true;

      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);
			base.OnPageLoad ();
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
						g_Player.Play(g_Player.CurrentFile);
					return;
			}
			base.OnAction (action);
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
