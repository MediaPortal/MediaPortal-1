/* 
 *	Copyright (C) 2005 Team MediaPortal
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

#region Usings
using System;
using System.Management;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using mbm5.MBMInfo;
#endregion

namespace MediaPortal.GUI.GUIStatus {
	/// <summary>
	/// Summary description for GUIStatusPrefs.
	/// </summary>
	public class GUIStatusPrefs : GUIWindow
	{
		public static int WINDOW_STATUS_PREFS = 757;

		#region Private Enumerations
		enum Controls {
			CONTROL_BACK	= 2,
			CONTROL_TEST	= 3,
			CONTROL_SPIN	= 4,
			CONTROL_SHUT	= 5,
			CONTROL_LIST	= 10
		};
		#endregion

		#region Private Variables
		private string soundFolder = string.Empty;
		private string sound = string.Empty;
		private int spin;
		private GUIListItem selectedItem;
		#endregion
		
		#region Constructor
		public GUIStatusPrefs() { 
			//
			// TODO: Add constructor logic here
			//
			GetID=GUIStatusPrefs.WINDOW_STATUS_PREFS;
		}
		#endregion

		#region Overrides
		public override bool Init() {
			return Load (GUIGraphicsContext.Skin+@"\mystatusprefs.xml");
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
					LoadSettings();
					LoadListControl();
					GUISpinControl cntlYieldInterval=GetControl((int)Controls.CONTROL_SPIN) as GUISpinControl;
					if (cntlYieldInterval!=null) {
						for (int i=1; i <= 100; i++) cntlYieldInterval.AddLabel("",i);	
						cntlYieldInterval.Value=1;
					}
					cntlYieldInterval.Value=GUIStatus.GetInterval();
					((GUIToggleButtonControl)GetControl((int)Controls.CONTROL_SHUT)).Selected = GUIStatus.IsShutdown();
					return true;
				}
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
					//get sender control
					base.OnMessage(message);
					int iControl=message.SenderControlId;
					if (iControl==(int)Controls.CONTROL_SPIN) {	
						GUISpinControl cntlYieldInt=GetControl((int)Controls.CONTROL_SPIN) as GUISpinControl;
						int iInterval=(cntlYieldInt.Value)+1;
						GUIStatus.SetInterval(iInterval);
						spin=iInterval;
					} 
					if (iControl==(int)Controls.CONTROL_BACK) {
						GUIStatus.SetShutdown(((GUIToggleButtonControl)GetControl((int)Controls.CONTROL_SHUT)).Selected);
						SaveSettings();
						GUIWindowManager.ShowPreviousWindow();
						return true;
					}
					if (iControl==(int)Controls.CONTROL_TEST) {
						Play();
						return true;
					}
					if (iControl==(int)Controls.CONTROL_LIST) {
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);
						int iItem=(int)msg.Param1;
						int iAction=(int)message.Param1;
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM) {
							GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
							if (item!=null) {
								sound = item.Label;
								item.IconImage = "check-box.png";
								if(selectedItem != null) {
									selectedItem.IconImage = string.Empty;
									selectedItem=item;
								}
								GUIControl.RefreshControl(GetID,(int)Controls.CONTROL_LIST);
							}
						}
						GUIControl.FocusControl(GetID, iControl);
						return true;
					}
					return true;
			}
			return base.OnMessage(message);
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Loads the list control with the type of sound selected
		/// </summary>
		/// 
		private void LoadListControl() {	
			//clear the list
			GUIControl.ClearControl(GetID,(int)Controls.CONTROL_LIST);
			VirtualDirectory Directory;
			ArrayList itemlist;
			Directory = new VirtualDirectory();
			Directory.SetExtensions(Util.Utils.AudioExtensions);
			itemlist = Directory.GetDirectory(soundFolder);
				
			foreach (GUIListItem item in itemlist) {
				if(!item.IsFolder) {
					GUIListItem pItem = new GUIListItem(item.FileInfo.Name);
					if(pItem.Label == sound) {
						pItem.IconImage = "check-box.png";
						selectedItem = pItem;
					}
					GUIControl.AddListItemControl(GetID,(int)Controls.CONTROL_LIST,pItem);
				}	
			}
			string strObjects =String.Format("{0} {1}",GUIControl.GetItemCount(GetID,(int)Controls.CONTROL_LIST).ToString(), GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
		}

		private void Play() {
			try {
				g_Player.Play(soundFolder + "\\" + sound);
				g_Player.Volume=99;
			}
			catch {
			}
		}

		private void SaveSettings() {
			using(MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml")) {
				xmlwriter.SetValueAsBool("status","status_shutdown",((GUIToggleButtonControl)GetControl((int)Controls.CONTROL_SHUT)).Selected);
				xmlwriter.SetValue("status","status_sound",sound);
				xmlwriter.SetValue("status","status_sound_delay",spin);
			}
		}

		private void LoadSettings() {
			using(MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml")) {
				soundFolder=xmlreader.GetValueAsString("status","status_sound_folder","");
				sound=xmlreader.GetValueAsString("status","status_sound","");		
			}
		}
		#endregion

	}
}
