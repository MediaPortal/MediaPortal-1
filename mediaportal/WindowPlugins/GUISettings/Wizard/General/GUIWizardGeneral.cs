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

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings.Wizard
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIWizardGeneral : GUIWindow
  {
    [SkinControl(4)] protected GUICheckMarkControl cmInternetYes = null;
    [SkinControl(5)] protected GUICheckMarkControl cmInternetNo = null;
    [SkinControl(6)] protected GUICheckMarkControl cmDeadicatedPC = null;
    [SkinControl(7)] protected GUICheckMarkControl cmSharedPC = null;
    //[SkinControlAttribute(8)]       protected GUICheckMarkControl cmAutoStartYes = null;
    //[SkinControlAttribute(9)]       protected GUICheckMarkControl cmAutoStartNo = null;
    //[SkinControlAttribute(6)]       protected GUICheckMarkControl cmDeadicatedYes = null;
    //[SkinControlAttribute(7)]       protected GUICheckMarkControl cmDeadicatedNo = null;
    [SkinControl(26)] protected GUIButtonControl btnNext = null;
    [SkinControl(25)] protected GUIButtonControl btnBack = null;
    //[SkinControlAttribute(10)]		protected GUIImage				imgHTPC=null;

    public GUIWizardGeneral()
    {
      GetID = (int) Window.WINDOW_WIZARD_GENERAL;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_general.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadSettings();
      GUIControl.FocusControl(GetID, btnNext.GetID);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (cmInternetYes == control)
      {
        OnInternetAccess(true);
      }
      if (cmInternetNo == control)
      {
        OnInternetAccess(false);
      }
      if (cmDeadicatedPC == control)
      {
        OnUsageType(true);
      }
      if (cmSharedPC == control)
      {
        OnUsageType(false);
      }
      if (btnNext == control)
      {
        OnNextPage();
      }
      if (btnBack == control)
      {
        GUIWindowManager.ShowPreviousWindow();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnUsageType(bool deadicated)
    {
      if (deadicated)
      {
        cmDeadicatedPC.Selected = true;
        cmSharedPC.Selected = false;
        GUIControl.FocusControl(GetID, cmDeadicatedPC.GetID);
      }
      else
      {
        cmDeadicatedPC.Selected = false;
        cmSharedPC.Selected = true;
        GUIControl.FocusControl(GetID, cmSharedPC.GetID);
      }
    }

    private void OnInternetAccess(bool yes)
    {
      if (yes)
      {
        cmInternetYes.Selected = true;
        cmInternetNo.Selected = false;
        GUIControl.FocusControl(GetID, cmInternetYes.GetID);
      }
      else
      {
        cmInternetYes.Selected = false;
        cmInternetNo.Selected = true;
        GUIControl.FocusControl(GetID, cmInternetNo.GetID);
      }
    }

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        OnInternetAccess(xmlreader.GetValueAsBool("general", "internetaccess", true));
        OnUsageType(true);
        //OnFullScreen(xmlreader.GetValueAsBool("general", "startfullscreen", true));
        //OnAutoStart(xmlreader.GetValueAsBool("general", "autostart", false));
      }
    }

    private void OnNextPage()
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("general", "internetaccess", cmInternetYes.Selected);

        // general defaults
        bool startfullscreen = false;
        bool mousesupport = true;
        bool autohidemouse = false;
        bool autostart = false;
        bool baloontips = false;
        bool hidetaskbar = false;
        bool dblclickasrightclick = false;
        bool alwaysontop = false;
        bool exclusivemode = true;
        bool useVMR9ZapOSD = false;
        bool enableguisounds = true;
        bool screensaver = false;

        if (cmDeadicatedPC.Selected)
        {
          startfullscreen = true;
          autostart = true;
          autohidemouse = true;
          baloontips = true;
          hidetaskbar = true;
          alwaysontop = true;
          enableguisounds = true;
        }

        xmlwriter.SetValueAsBool("general", "startfullscreen", startfullscreen);
        xmlwriter.SetValueAsBool("general", "mousesupport", mousesupport);
        xmlwriter.SetValueAsBool("general", "autohidemouse", autohidemouse);
        xmlwriter.SetValueAsBool("general", "autostart", autostart);
        xmlwriter.SetValueAsBool("general", "alwaysontop", alwaysontop);
        xmlwriter.SetValueAsBool("general", "baloontips", baloontips);
        xmlwriter.SetValueAsBool("general", "hidetaskbar", hidetaskbar);
        xmlwriter.SetValueAsBool("general", "dblclickasrightclick", dblclickasrightclick);
        xmlwriter.SetValueAsBool("general", "useVMR9ZapOSD", useVMR9ZapOSD);
        xmlwriter.SetValueAsBool("general", "enableguisounds", enableguisounds);
        xmlwriter.SetValueAsBool("general", "IdleTimer", screensaver);
        xmlwriter.SetValueAsBool("general", "exclusivemode", exclusivemode);
      }

      GUIPropertyManager.SetProperty("#Wizard.General.Done", "yes");
      GUIPropertyManager.SetProperty("#InternetAccess", cmInternetYes.Selected.ToString());

      GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_REMOTE);
    }
  }
}