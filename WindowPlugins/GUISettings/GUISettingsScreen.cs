using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings
{
	/// <summary>
	/// 
	/// </summary>
	public class GUISettingsScreen : GUIWindow
	{
		public GUISettingsScreen()
		{
      GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_SCREEN;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\settingsScreen.xml");
    }
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
        {
          GUIWindowManager.ShowPreviousWindow();
          return;
        }
      }
      base.OnAction(action);
    }
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          base.OnMessage(message);
          return true;
        }
        
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
        }
          break;

      }
      return base.OnMessage(message);
    }


	}
}
