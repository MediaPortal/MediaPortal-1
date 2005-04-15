using System;
using MediaPortal.GUI.Library;
namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class  GUISettings : GUIWindow, ISetupForm
  {

    public  GUISettings()
    {
			
      GetID=(int)GUIWindow.Window.WINDOW_SETTINGS;
    }
    
    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\settings.xml");
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

		#region ISetupForm Members

		public bool CanEnable()
		{
			return true;
		}

    public bool HasSetup()
    {
      return false;
    }
		public string PluginName()
		{
			return "Settings";
		}

		public bool DefaultEnabled()
		{
			return true;
		}

		public int GetWindowId()
		{
			return GetID;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = GUILocalizeStrings.Get(5);
			strButtonImage = "";
			strButtonImageFocus = "";
			strPictureImage = "";
			return true;
		}

		public string Author()
		{
			return "Frodo";
		}

		public string Description()
		{
			return "Settings";
		}

		public void ShowPlugin()
		{
		}

		#endregion
  }
}
