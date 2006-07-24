/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
				GUIWindowManager.ShowPreviousWindow();  
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
