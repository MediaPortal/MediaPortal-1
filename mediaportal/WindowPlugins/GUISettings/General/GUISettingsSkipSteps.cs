#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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

using System;
using System.IO;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;

namespace WindowPlugins.GUISettings
{
	public class GUISettingsSkipSteps : GUIWindow
	{
		[SkinControlAttribute(2)]			protected GUIToggleButtonControl toggleButtonStep1 =null;

    public GUISettingsSkipSteps()
		{
			GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_SKIPSTEPS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\settingsSkipSteps.xml");      
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      //Load settings
      Log.Write("DEBUG - GUISkipSteps: {0}", "LoadSettings");
      GUIControl.FocusControl(GetID, toggleButtonStep1.GetID);
    }

    protected override void OnPageDestroy( int newWindowId )
    {
      base.OnPageDestroy(newWindowId);
      SaveSettings();
    }

    void SaveSettings()
    {
      using ( MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml") )
      {
        //xmlwriter.SetValueAsBool("general", "startfullscreen", btnFullscreen.Selected);
        //xmlwriter.SetValue("skin", "language", btnLanguage.SelectedLabel);
        Log.Write("DEBUG - GUISkipSteps: {0}", "SaveSettings");
      }
    }

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
      if (control == toggleButtonStep1)
      {
          Log.Write("DEBUG - GUISkipSteps: {0}", "toggleButtonStep1.onclick");
      }
			base.OnClicked (controlId, control, actionType);
		}
    
	}
}