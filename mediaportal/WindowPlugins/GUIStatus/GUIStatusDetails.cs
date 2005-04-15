#region Usings
using System;
using System.Management;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using mbm5.MBMInfo;
#endregion

namespace MediaPortal.GUI.GUIStatus {
	public class GUIStatusDetails : GUIWindow {
		public static int WINDOW_STATUS_DETAILS = 756;
		
		private mbmSharedData MBMInfo=new mbmSharedData();

		#region Private Enumerations
		enum Controls {
			CONTROL_BACK	= 2,
			CONTROL_ALARM	= 3
		};
		#endregion

		#region Constructor
		public GUIStatusDetails() { 
			//
			// TODO: Add constructor logic here
			//
			GetID=GUIStatusDetails.WINDOW_STATUS_DETAILS;
		}
		#endregion

		#region Overrides
		public override bool Init() {
			return Load (GUIGraphicsContext.Skin+@"\mystatusdetails.xml");
		}

		public override void OnAction(Action action) {
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU) {
				GUIWindowManager.ShowPreviousWindow();
				return;
			}
			base.OnAction(action);
		}

		public override bool OnMessage(GUIMessage message) {
			switch ( message.Message ) {

				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT: {
					base.OnMessage(message);
					((GUIToggleButtonControl)GetControl((int)Controls.CONTROL_ALARM)).Selected = GUIStatus.IsAlarm();
					return true;
				}
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					//get sender control
					base.OnMessage(message);
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_BACK) {
						SaveSettings();
						GUIWindowManager.ShowPreviousWindow();
						return true;
					}
					return true;
			}
			return base.OnMessage(message);
		}

		private void SaveSettings() {
			using(MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml")) {
				xmlwriter.SetValueAsBool("status","status_"+GUIStatus.GetName()+"al",((GUIToggleButtonControl)GetControl((int)Controls.CONTROL_ALARM)).Selected);
				GUIStatus.SetAlarm(((GUIToggleButtonControl)GetControl((int)Controls.CONTROL_ALARM)).Selected);
			}
		}
	}
	#endregion
}
