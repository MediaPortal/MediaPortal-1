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
using MediaPortal.Player;

namespace WindowPlugins.GUISettings
{
  public class GUISettingsSkipSteps : GUIWindow
  {
    #region Skin elements
    [SkinControlAttribute(2)]
    protected GUICheckMarkControl checkMarkButtonStep1 = null;
    [SkinControlAttribute(3)]
    protected GUICheckMarkControl checkMarkButtonStep2 = null;
    [SkinControlAttribute(4)]
    protected GUICheckMarkControl checkMarkButtonStep3 = null;
    [SkinControlAttribute(5)]
    protected GUICheckMarkControl checkMarkButtonStep4 = null;
    [SkinControlAttribute(6)]
    protected GUICheckMarkControl checkMarkButtonStep5 = null;
    [SkinControlAttribute(7)]
    protected GUICheckMarkControl checkMarkButtonStep6 = null;
    [SkinControlAttribute(8)]
    protected GUICheckMarkControl checkMarkButtonStep7 = null;
    [SkinControlAttribute(9)]
    protected GUICheckMarkControl checkMarkButtonStep8 = null;
    [SkinControlAttribute(10)]
    protected GUICheckMarkControl checkMarkButtonStep9 = null;
    [SkinControlAttribute(11)]
    protected GUICheckMarkControl checkMarkButtonStep10 = null;
    [SkinControlAttribute(12)]
    protected GUICheckMarkControl checkMarkButtonStep11 = null;
    [SkinControlAttribute(13)]
    protected GUICheckMarkControl checkMarkButtonStep12 = null;
    [SkinControlAttribute(14)]
    protected GUICheckMarkControl checkMarkButtonStep13 = null;
    [SkinControlAttribute(15)]
    protected GUICheckMarkControl checkMarkButtonStep14 = null;
    [SkinControlAttribute(16)]
    protected GUICheckMarkControl checkMarkButtonStep15 = null;
    [SkinControlAttribute(17)]
    protected GUICheckMarkControl checkMarkButtonStep16 = null;
    [SkinControlAttribute(18)]
    protected GUIButtonControl buttonReset = null;
    #endregion

    public GUISettingsSkipSteps()
    {
      GetID = (int)GUIWindow.Window.WINDOW_SETTINGS_SKIPSTEPS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settingsSkipSteps.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      //Load settings
      Log.Write("GUISkipSteps: {0}", "Load settings");
      ArrayList StepArray = new ArrayList();

      using ( MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml") )
        foreach ( string token in ( xmlreader.GetValueAsString("movieplayer", "skipsteps", "0;1;1;0;1;1;1;0;1;1;1;0;1;0;1;0").Split(new char[] { ',', ';', ' ' }) ) )
        {
          if ( token == string.Empty )
            StepArray.Add(0);
          else
            StepArray.Add(Convert.ToInt32(token));
        }
      //Log.Write("DEBUG - GUISkipSteps: Step 1 = {0}", Convert.ToString(StepArray[0]));
      checkMarkButtonStep1.Selected = ( (int)StepArray[0] == 1 ) ? true : false;
      checkMarkButtonStep2.Selected = ( (int)StepArray[1] == 1 ) ? true : false;
      checkMarkButtonStep3.Selected = ( (int)StepArray[2] == 1 ) ? true : false;
      checkMarkButtonStep4.Selected = ( (int)StepArray[3] == 1 ) ? true : false;
      checkMarkButtonStep5.Selected = ( (int)StepArray[4] == 1 ) ? true : false;
      checkMarkButtonStep6.Selected = ( (int)StepArray[5] == 1 ) ? true : false;
      checkMarkButtonStep7.Selected = ( (int)StepArray[6] == 1 ) ? true : false;
      checkMarkButtonStep8.Selected = ( (int)StepArray[7] == 1 ) ? true : false;
      checkMarkButtonStep9.Selected = ( (int)StepArray[8] == 1 ) ? true : false;
      checkMarkButtonStep10.Selected = ( (int)StepArray[9] == 1 ) ? true : false;
      checkMarkButtonStep11.Selected = ( (int)StepArray[10] == 1 ) ? true : false;
      checkMarkButtonStep12.Selected = ( (int)StepArray[11] == 1 ) ? true : false;
      checkMarkButtonStep13.Selected = ( (int)StepArray[12] == 1 ) ? true : false;
      checkMarkButtonStep14.Selected = ( (int)StepArray[13] == 1 ) ? true : false;
      checkMarkButtonStep15.Selected = ( (int)StepArray[14] == 1 ) ? true : false;
      checkMarkButtonStep16.Selected = ( (int)StepArray[15] == 1 ) ? true : false;

      GUIControl.FocusControl(GetID, checkMarkButtonStep1.GetID);
    }

    protected override void OnPageDestroy( int newWindowId )
    {
      base.OnPageDestroy(newWindowId);
      SaveSettings();
    }

    void SaveSettings()
    {
      Log.Write("GUISkipSteps: {0}", "Save settings");

      string skipSteps = ( Convert.ToInt16(checkMarkButtonStep1.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep2.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep3.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep4.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep5.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep6.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep7.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep8.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep9.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep10.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep11.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep12.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep13.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep14.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep15.Selected) ).ToString() + ";" +
                         ( Convert.ToInt16(checkMarkButtonStep16.Selected) ).ToString();
      using ( MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml") )
      {
        xmlwriter.SetValue("movieplayer", "skipsteps", skipSteps);
      }
      g_Player.configLoaded = false;
      Log.Write("GUISkipSteps: {0}", "reset g_player settings");
    }

    protected override void OnClicked( int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType )
    {
      if ( control == buttonReset )
      {
        checkMarkButtonStep1.Selected = false;
        checkMarkButtonStep2.Selected = true;
        checkMarkButtonStep3.Selected = true;
        checkMarkButtonStep4.Selected = false;
        checkMarkButtonStep5.Selected = true;
        checkMarkButtonStep6.Selected = true;
        checkMarkButtonStep7.Selected = true;
        checkMarkButtonStep8.Selected = false;
        checkMarkButtonStep9.Selected = true;
        checkMarkButtonStep10.Selected = true;
        checkMarkButtonStep11.Selected = true;
        checkMarkButtonStep12.Selected = false;
        checkMarkButtonStep13.Selected = true;
        checkMarkButtonStep14.Selected = false;
        checkMarkButtonStep15.Selected = true;
        checkMarkButtonStep16.Selected = false;
      }
      base.OnClicked(controlId, control, actionType);
    }

  }
}