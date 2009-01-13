#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
  public class GUIWizardRemote : GUIWindow
  {
    [SkinControl(4)] protected GUICheckMarkControl cmMicrosoftMCE = null;
    [SkinControl(6)] protected GUICheckMarkControl cmHauppauge = null;
    [SkinControl(7)] protected GUICheckMarkControl cmX10Medion = null;
    [SkinControl(11)] protected GUICheckMarkControl cmX10Ati = null;
    [SkinControl(8)] protected GUICheckMarkControl cmFireDTV = null;
    [SkinControl(9)] protected GUICheckMarkControl cmOther = null;
    [SkinControl(26)] protected GUIButtonControl btnNext = null;
    [SkinControl(25)] protected GUIButtonControl btnBack = null;
    [SkinControl(10)] protected GUIImage imgRemote = null;


    public GUIWizardRemote()
    {
      GetID = (int) Window.WINDOW_WIZARD_REMOTE;
    }


    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_remote_control.xml");
    }


    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadSettings();
      GUIControl.FocusControl(GetID, btnNext.GetID);
    }


    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (cmMicrosoftMCE == control)
      {
        OnMicrosoftMCE();
      }
      if (cmHauppauge == control)
      {
        OnHauppauge();
      }
      if (cmX10Medion == control)
      {
        OnX10Medion();
      }
      if (cmX10Ati == control)
      {
        OnX10Ati();
      }
      if (cmFireDTV == control)
      {
        OnFireDTV();
      }
      if (cmOther == control)
      {
        OnOther();
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


    private void OnMicrosoftMCE()
    {
      cmMicrosoftMCE.Selected = true;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(Config.GetFile(Config.Dir.Base, "Wizards", "remote_mce_eu.png"));
      GUIControl.FocusControl(GetID, cmMicrosoftMCE.GetID);
    }


    private void OnHauppauge()
    {
      cmMicrosoftMCE.Selected = false;
      cmHauppauge.Selected = true;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(Config.GetFile(Config.Dir.Base, "Wizards", "remote_hcw.png"));
      GUIControl.FocusControl(GetID, cmHauppauge.GetID);
    }


    private void OnX10Medion()
    {
      cmMicrosoftMCE.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = true;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(Config.GetFile(Config.Dir.Base, "Wizards", "remote_x10.png"));
      GUIControl.FocusControl(GetID, cmX10Medion.GetID);
    }


    private void OnX10Ati()
    {
      cmMicrosoftMCE.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = true;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(Config.GetFile(Config.Dir.Base, "Wizards", "remote_x10.png"));
      GUIControl.FocusControl(GetID, cmX10Ati.GetID);
    }


    private void OnFireDTV()
    {
      cmMicrosoftMCE.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = true;
      cmOther.Selected = false;
      imgRemote.SetFileName(Config.GetFile(Config.Dir.Base, "Wizards", "remote_firedtv.png"));
      GUIControl.FocusControl(GetID, cmFireDTV.GetID);
    }


    private void OnOther()
    {
      cmMicrosoftMCE.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = true;
      imgRemote.SetFileName(Config.GetFile(Config.Dir.Base, "Wizards", "remote_other.png"));
      GUIControl.FocusControl(GetID, cmOther.GetID);
    }


    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        bool useMicrosoft = xmlreader.GetValueAsBool("remote", "MCE", true);
        bool useHCW = xmlreader.GetValueAsBool("remote", "HCW", false);
        bool useX10 = xmlreader.GetValueAsBool("remote", "X10", false);
        bool useX10Medion = xmlreader.GetValueAsBool("remote", "X10Medion", false);
        bool useX10Ati = xmlreader.GetValueAsBool("remote", "X10ATI", false);
        bool useFireDTV = xmlreader.GetValueAsBool("remote", "FireDTV", false);

        if (useMicrosoft)
        {
          OnMicrosoftMCE();
        }
        else if (useHCW)
        {
          OnHauppauge();
        }
        else if (useX10 && useX10Medion)
        {
          OnX10Medion();
        }
        else if (useX10 && useX10Ati)
        {
          OnX10Ati();
        }
        else if (useFireDTV)
        {
          OnFireDTV();
        }
        else
        {
          OnOther();
        }
      }
    }


    private void OnNextPage()
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("remote", "MCE", cmMicrosoftMCE.Selected);
        xmlwriter.SetValueAsBool("remote", "HCW", cmHauppauge.Selected);
        xmlwriter.SetValueAsBool("remote", "X10", cmX10Medion.Selected || cmX10Ati.Selected);
        xmlwriter.SetValueAsBool("remote", "X10Medion", cmX10Medion.Selected);
        xmlwriter.SetValueAsBool("remote", "X10ATI", cmX10Ati.Selected);
        xmlwriter.SetValueAsBool("remote", "FireDTV", cmFireDTV.Selected);
      }
      GUIPropertyManager.SetProperty("#Wizard.Remote.Done", "yes");

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESTART_REMOTE_CONTROLS, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msg);

      bool tvPluginInstalled;
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        tvPluginInstalled = xmlreader.GetValueAsBool("pluginsdlls", "TvPlugin.dll", false);
      }

      if (tvPluginInstalled)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_FINISHED);
      }
      else
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_CARDS_DETECTED);
      }
    }
  }
}