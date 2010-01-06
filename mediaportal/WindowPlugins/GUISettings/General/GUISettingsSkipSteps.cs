#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace WindowPlugins.GUISettings
{
  public class GUISettingsSkipSteps : GUIInternalWindow
  {
    #region Skin elements

    [SkinControl(2)] protected GUICheckMarkControl checkMarkButtonStep1 = null;
    [SkinControl(3)] protected GUICheckMarkControl checkMarkButtonStep2 = null;
    [SkinControl(4)] protected GUICheckMarkControl checkMarkButtonStep3 = null;
    [SkinControl(5)] protected GUICheckMarkControl checkMarkButtonStep4 = null;
    [SkinControl(6)] protected GUICheckMarkControl checkMarkButtonStep5 = null;
    [SkinControl(7)] protected GUICheckMarkControl checkMarkButtonStep6 = null;
    [SkinControl(8)] protected GUICheckMarkControl checkMarkButtonStep7 = null;
    [SkinControl(9)] protected GUICheckMarkControl checkMarkButtonStep8 = null;
    [SkinControl(10)] protected GUICheckMarkControl checkMarkButtonStep9 = null;
    [SkinControl(11)] protected GUICheckMarkControl checkMarkButtonStep10 = null;
    [SkinControl(12)] protected GUICheckMarkControl checkMarkButtonStep11 = null;
    [SkinControl(13)] protected GUICheckMarkControl checkMarkButtonStep12 = null;
    [SkinControl(14)] protected GUICheckMarkControl checkMarkButtonStep13 = null;
    [SkinControl(15)] protected GUICheckMarkControl checkMarkButtonStep14 = null;
    [SkinControl(16)] protected GUICheckMarkControl checkMarkButtonStep15 = null;
    [SkinControl(17)] protected GUICheckMarkControl checkMarkButtonStep16 = null;
    [SkinControl(18)] protected GUIButtonControl buttonReset = null;
    [SkinControl(19)] protected GUIButtonControl buttonAdd = null;
    [SkinControl(20)] protected GUIButtonControl buttonRemove = null;
    [SkinControl(21)] protected GUILabelControl labelCurrent = null;

    #endregion

    private const string DEFAULT_SETTING = "15,30,60,180,300,600,900,1800,3600,7200";

    public GUISettingsSkipSteps()
    {
      GetID = (int)Window.WINDOW_SETTINGS_SKIPSTEPS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\settingsSkipSteps.xml");
    }

    #region loading

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      //Load settings
      Log.Info("GUISkipSteps: {0}", "Load settings");
      string regValue = string.Empty;

      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          regValue = xmlreader.GetValueAsString("movieplayer", "skipsteps", DEFAULT_SETTING);
          if (regValue == string.Empty) // config after wizard run 1st
          {
            regValue = DEFAULT_SETTING;
            Log.Info("GeneralSkipSteps - creating new Skip-Settings {0}", "");
          }
          else if (OldStyle(regValue))
          {
            regValue = ConvertToNewStyle(regValue);
          }
          labelCurrent.Label = regValue;
        }
        catch (Exception ex)
        {
          Log.Info("GeneralSkipSteps - Exception while loading Skip-Settings: {0}", ex.ToString());
        }
      }
      SetCheckMarksBasedOnString(regValue);

      GUIControl.FocusControl(GetID, checkMarkButtonStep1.GetID);
    }

    #endregion

    #region saving

    protected override void OnPageDestroy(int newWindowId)
    {
      base.OnPageDestroy(newWindowId);
      SaveSettings();
    }

    private void SaveSettings()
    {
      Log.Info("GUISkipSteps: {0}", "Save settings");

      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("movieplayer", "skipsteps", labelCurrent.Label);
      }
      g_Player.configLoaded = false;
      Log.Info("GUISkipSteps: {0}", "reset g_player settings");
    }

    #endregion

    private void SetCheckMarksBasedOnString(string s)
    {
      bool check1 = false, check2 = false, check3 = false, check4 = false, check5 = false, check6 = false;
      bool check7 = false, check8 = false, check9 = false, check10 = false, check11 = false, check12 = false;
      bool check13 = false, check14 = false, check15 = false, check16 = false;
      foreach (string token in s.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        try
        {
          int step = Convert.ToInt16(token);
          switch (step)
          {
            case 5:
              check1 = true;
              break;
            case 15:
              check2 = true;
              break;
            case 30:
              check3 = true;
              break;
            case 45:
              check4 = true;
              break;
            case 60:
              check5 = true;
              break;
            case 180:
              check6 = true;
              break;
            case 300:
              check7 = true;
              break;
            case 420:
              check8 = true;
              break;
            case 600:
              check9 = true;
              break;
            case 900:
              check10 = true;
              break;
            case 1800:
              check11 = true;
              break;
            case 2700:
              check12 = true;
              break;
            case 3600:
              check13 = true;
              break;
            case 5400:
              check14 = true;
              break;
            case 7200:
              check15 = true;
              break;
            case 10800:
              check16 = true;
              break;
            default:
              break; // Do nothing
          }
        }
        catch (Exception)
        {
          Log.Error("Invalid skip step configuration in MediaPortal.xml");
        }
      }
      checkMarkButtonStep1.Selected = check1;
      checkMarkButtonStep2.Selected = check2;
      checkMarkButtonStep3.Selected = check3;
      checkMarkButtonStep4.Selected = check4;
      checkMarkButtonStep5.Selected = check5;
      checkMarkButtonStep6.Selected = check6;
      checkMarkButtonStep7.Selected = check7;
      checkMarkButtonStep8.Selected = check8;
      checkMarkButtonStep9.Selected = check9;
      checkMarkButtonStep10.Selected = check10;
      checkMarkButtonStep11.Selected = check11;
      checkMarkButtonStep12.Selected = check12;
      checkMarkButtonStep13.Selected = check13;
      checkMarkButtonStep14.Selected = check14;
      checkMarkButtonStep15.Selected = check15;
      checkMarkButtonStep16.Selected = check16;
    }

    #region event handling

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == buttonReset)
      {
        labelCurrent.Label = DEFAULT_SETTING;
        SetCheckMarksBasedOnString(DEFAULT_SETTING);
      }
      else if (control == buttonAdd)
      {
        VirtualKeyboard vk = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
        vk.Reset();
        vk.DoModal(GetID);
        string newStep = vk.Text;
        if (newStep == string.Empty || newStep == null)
        {
          return;
        }
        string error = verifySkipStep(newStep);
        if (error != null)
        {
          GUIDialogOK errDialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          errDialog.SetHeading(257);
          errDialog.SetLine(1, error);
          errDialog.DoModal(GetID);
        }
        else
        {
          AddStep(Convert.ToInt16(newStep)); // Already verifed, so no numberformatexception can occur
        }
      }
      else if (control == buttonRemove)
      {
        GUIDialogSelect2 dlgSel = (GUIDialogSelect2)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT2);
        dlgSel.Reset();

        foreach (string token in labelCurrent.Label.Split(new char[] {',', ';', ' '}))
        {
          if (token == string.Empty)
          {
            continue;
          }
          dlgSel.Add(token);
        }

        dlgSel.SetHeading(200040); // Remove skip step
        dlgSel.DoModal(GetID);
        if (dlgSel.SelectedLabel != -1)
        {
          try
          {
            RemoveStep(Convert.ToInt16(dlgSel.SelectedLabelText));
          }
          catch (Exception)
          {
            // Should never happen
          }
        }
      }
      else if (control is GUICheckMarkControl)
      {
        int stepSize = 5;
        if (control == checkMarkButtonStep1)
        {
          stepSize = 5;
        }
        else if (control == checkMarkButtonStep2)
        {
          stepSize = 15;
        }
        else if (control == checkMarkButtonStep3)
        {
          stepSize = 30;
        }
        else if (control == checkMarkButtonStep4)
        {
          stepSize = 45;
        }
        else if (control == checkMarkButtonStep5)
        {
          stepSize = 60;
        }
        else if (control == checkMarkButtonStep6)
        {
          stepSize = 180;
        }
        else if (control == checkMarkButtonStep7)
        {
          stepSize = 300;
        }
        else if (control == checkMarkButtonStep8)
        {
          stepSize = 420;
        }
        else if (control == checkMarkButtonStep9)
        {
          stepSize = 600;
        }
        else if (control == checkMarkButtonStep10)
        {
          stepSize = 900;
        }
        else if (control == checkMarkButtonStep11)
        {
          stepSize = 1800;
        }
        else if (control == checkMarkButtonStep12)
        {
          stepSize = 2700;
        }
        else if (control == checkMarkButtonStep13)
        {
          stepSize = 3600;
        }
        else if (control == checkMarkButtonStep14)
        {
          stepSize = 5400;
        }
        else if (control == checkMarkButtonStep15)
        {
          stepSize = 7200;
        }
        else if (control == checkMarkButtonStep16)
        {
          stepSize = 10800;
        }

        if (!((GUICheckMarkControl)control).Selected)
        {
          RemoveStep(stepSize);
        }
        else
        {
          AddStep(stepSize);
        }
      }
      base.OnClicked(controlId, control, actionType);
    }

    #endregion

    #region Verification

    /// <summary>
    /// Verify that the entered skip step is valid to add
    /// </summary>
    /// <param name="newStep">Entered text</param>
    /// <returns>null if the entered text is a valid, new skip step, otherwise a string describing what's wrong with it</returns>
    private string verifySkipStep(string newStep)
    {
      int step;
      //int multiplyer = 1;
      //if (newStep.IndexOf('s') == (newStep.Length - 1) || newStep.IndexOf('S') == (newStep.Length - 1))
      //{
      //  newStep = newStep.Substring(0, newStep.Length - 1);
      //}
      //else if (newStep.IndexOf('m') == (newStep.Length - 1) || newStep.IndexOf('M') == (newStep.Length - 1))
      //{
      //  newStep = newStep.Substring(0, newStep.Length - 1);
      //  multiplyer = 60;
      //}
      //else if (newStep.IndexOf('m') == (newStep.Length - 1) || newStep.IndexOf('M') == (newStep.Length - 1))
      //{
      //  newStep = newStep.Substring(0, newStep.Length - 1);
      //  multiplyer = 3600;
      //} -- This doesn't really help the user a lot, so it's not worth the trouble
      try
      {
        step = Convert.ToInt16(newStep);
      }
      catch (Exception)
      {
        return "Not a valid integer";
      }
      //step *= multiplyer;
      if (step < 0)
      {
        return "Postive values only!";
      }
      else if (step == 0)
      {
        return "Zero skip is not allowed!";
      }
      else if (step > 10800)
      {
        return "3 hour skip is maximum!";
      }
      else
      {
        // Check that whole minutes are entered
        if (step > 60 && (step % 60) != 0)
        {
          return "Enter whole minutes only!";
        }
      }
      if (CheckExists(step))
      {
        return "Skip step already defined";
      }
      return null;
    }

    /// <summary>
    /// Check that the given step isn't already defined
    /// </summary>
    /// <param name="step">Step to check</param>
    /// <returns>True if it already exists, else false</returns>
    private bool CheckExists(int step)
    {
      foreach (string token in labelCurrent.Label.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        int value;
        try
        {
          value = Convert.ToInt16(token);
          if (value == step)
          {
            return true;
          }
        }
        catch (Exception)
        {
          // Should never happen
          return false;
        }
      }

      return false;
    }

    #endregion

    #region add/remove steps

    private void AddStep(int stepsize)
    {
      switch (stepsize)
      {
        case 5:
          checkMarkButtonStep1.Selected = true;
          break;
        case 15:
          checkMarkButtonStep2.Selected = true;
          break;
        case 30:
          checkMarkButtonStep3.Selected = true;
          break;
        case 45:
          checkMarkButtonStep4.Selected = true;
          break;
        case 60:
          checkMarkButtonStep5.Selected = true;
          break;
        case 180:
          checkMarkButtonStep6.Selected = true;
          break;
        case 300:
          checkMarkButtonStep7.Selected = true;
          break;
        case 420:
          checkMarkButtonStep8.Selected = true;
          break;
        case 600:
          checkMarkButtonStep9.Selected = true;
          break;
        case 900:
          checkMarkButtonStep10.Selected = true;
          break;
        case 1800:
          checkMarkButtonStep11.Selected = true;
          break;
        case 2700:
          checkMarkButtonStep12.Selected = true;
          break;
        case 3600:
          checkMarkButtonStep13.Selected = true;
          break;
        case 5400:
          checkMarkButtonStep14.Selected = true;
          break;
        case 7200:
          checkMarkButtonStep15.Selected = true;
          break;
        case 10800:
          checkMarkButtonStep16.Selected = true;
          break;
        default:
          break; // Do nothing
      }

      string newText = string.Empty;
      bool stepAdded = false;
      foreach (string token in labelCurrent.Label.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        try
        {
          int curInt = Convert.ToInt16(token);
          if (stepsize < curInt && !stepAdded)
          {
            newText += Convert.ToString(stepsize);
            newText += ",";
            stepAdded = true;
          }
          else if (stepsize == curInt)
          {
            stepAdded = true; // Should never get here, but just in case...
          }
          newText += token;
          newText += ",";
        }
        catch (Exception)
        {
          return;
        }
      }
      if (!stepAdded)
      {
        newText += Convert.ToString(stepsize);
        newText += ",";
      }
      labelCurrent.Label = (newText.Length > 0 ? newText.Substring(0, newText.Length - 1) : string.Empty);
    }

    private void RemoveStep(int stepsize)
    {
      switch (stepsize)
      {
        case 5:
          checkMarkButtonStep1.Selected = false;
          break;
        case 15:
          checkMarkButtonStep2.Selected = false;
          break;
        case 30:
          checkMarkButtonStep3.Selected = false;
          break;
        case 45:
          checkMarkButtonStep4.Selected = false;
          break;
        case 60:
          checkMarkButtonStep5.Selected = false;
          break;
        case 180:
          checkMarkButtonStep6.Selected = false;
          break;
        case 300:
          checkMarkButtonStep7.Selected = false;
          break;
        case 420:
          checkMarkButtonStep8.Selected = false;
          break;
        case 600:
          checkMarkButtonStep9.Selected = false;
          break;
        case 900:
          checkMarkButtonStep10.Selected = false;
          break;
        case 1800:
          checkMarkButtonStep11.Selected = false;
          break;
        case 2700:
          checkMarkButtonStep12.Selected = false;
          break;
        case 3600:
          checkMarkButtonStep13.Selected = false;
          break;
        case 5400:
          checkMarkButtonStep14.Selected = false;
          break;
        case 7200:
          checkMarkButtonStep15.Selected = false;
          break;
        case 10800:
          checkMarkButtonStep16.Selected = false;
          break;
        default:
          break; // Do nothing
      }

      string newText = string.Empty;
      foreach (string token in labelCurrent.Label.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        try
        {
          if (Convert.ToInt16(token) != stepsize)
          {
            newText += token;
            newText += ",";
          }
        }
        catch (Exception)
        {
          return;
        }
      }
      labelCurrent.Label = (newText.Length > 0 ? newText.Substring(0, newText.Length - 1) : string.Empty);
    }

    #endregion

    #region backward compatibility

    private bool OldStyle(string strSteps)
    {
      int count = 0;
      bool foundOtherThanZeroOrOne = false;
      foreach (string token in strSteps.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          continue;
        }
        try
        {
          int curInt = Convert.ToInt16(token);
          if (curInt != 0 && curInt != 1)
          {
            foundOtherThanZeroOrOne = true;
          }
          count++;
        }
        catch (Exception)
        {
          return true;
        }
      }
      return (count == 16 && !foundOtherThanZeroOrOne);
    }

    private string ConvertToNewStyle(string strSteps)
    {
      int count = 0;
      string newStyle = string.Empty;
      foreach (string token in strSteps.Split(new char[] {',', ';', ' '}))
      {
        if (token == string.Empty)
        {
          count++;
          continue;
        }
        try
        {
          int curInt = Convert.ToInt16(token);
          count++;
          if (curInt == 1)
          {
            switch (count)
            {
              case 1:
                newStyle += "5,";
                break;
              case 2:
                newStyle += "15,";
                break;
              case 3:
                newStyle += "30,";
                break;
              case 4:
                newStyle += "45,";
                break;
              case 5:
                newStyle += "60,";
                break;
              case 6:
                newStyle += "180,";
                break;
              case 7:
                newStyle += "300,";
                break;
              case 8:
                newStyle += "420,";
                break;
              case 9:
                newStyle += "600,";
                break;
              case 10:
                newStyle += "900,";
                break;
              case 11:
                newStyle += "1800,";
                break;
              case 12:
                newStyle += "2700,";
                break;
              case 13:
                newStyle += "3600,";
                break;
              case 14:
                newStyle += "5400,";
                break;
              case 15:
                newStyle += "7200,";
                break;
              case 16:
                newStyle += "10800,";
                break;
              default:
                break; // Do nothing
            }
          }
        }
        catch (Exception)
        {
          return DEFAULT_SETTING;
        }
      }
      return (newStyle == string.Empty ? string.Empty : newStyle.Substring(0, newStyle.Length - 1));
    }

    #endregion
  }
}