#region usings
using System;
using System.IO;
using System.Collections;
using System.Management;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
#endregion

namespace MediaPortal.GUI.GUIScript
{
  /// <summary>
  /// Summary description for GUIExplorer
  /// </summary>
  public class GUIScript : GUIWindow 
  {
		public static int WINDOW_STATUS = 740;

		#region Constructor
		public GUIScript()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#endregion
	
		#region Overrides		
		public override int GetID 
		{
			get { return WINDOW_STATUS; }
			set { base.GetID = value; }
		}

		public override bool Init() 
		{
			//return Load (GUIGraphicsContext.Skin+@"\myexplorer.xml");
			return true;
		}

		public override void OnAction(Action action) 
		{
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) 
			{
				GUIWindowManager.PreviousWindow();  
				return;
			}
			base.OnAction(action);
		}

		public override bool OnMessage(GUIMessage message) 
		{
			switch ( message.Message ) 
			{  
				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:	// MyScript starts
					base.OnMessage(message);
					LoadSettings();													// loads all settings from XML
					return true;
			}
			return base.OnMessage (message);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Loads all Settings from MediaPortal.xml
		/// </summary>
		private void LoadSettings() 
		{

		}

		#endregion
  }
}
