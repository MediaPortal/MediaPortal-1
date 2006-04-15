#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Settings.Wizard
{
  public class GUIWizardRemote : GUIWindow
  {
    [SkinControlAttribute(4)]
    protected GUICheckMarkControl cmMicrosoftWithoutTeletext = null;
    [SkinControlAttribute(5)]
    protected GUICheckMarkControl cmMicrosoftWithTeletext = null;
    [SkinControlAttribute(12)]
    protected GUICheckMarkControl cmMicrosoftOther = null;
    [SkinControlAttribute(6)]
    protected GUICheckMarkControl cmHauppauge = null;
    [SkinControlAttribute(7)]
    protected GUICheckMarkControl cmX10Medion = null;
    [SkinControlAttribute(11)]
    protected GUICheckMarkControl cmX10Ati = null;
    [SkinControlAttribute(8)]
    protected GUICheckMarkControl cmFireDTV = null;
    [SkinControlAttribute(9)]
    protected GUICheckMarkControl cmOther = null;
    [SkinControlAttribute(26)]
    protected GUIButtonControl btnNext = null;
    [SkinControlAttribute(25)]
    protected GUIButtonControl btnBack = null;
    [SkinControlAttribute(10)]
    protected GUIImage imgRemote = null;


    public GUIWizardRemote()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_REMOTE;
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


    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (cmMicrosoftWithoutTeletext == control) OnMicrosoftWithoutTeletext();
      if (cmMicrosoftWithTeletext == control) OnMicrosoftWithTeletext();
      if (cmMicrosoftOther == control) OnMicrosoftOther();
      if (cmHauppauge == control) OnHauppauge();
      if (cmX10Medion == control) OnX10Medion();
      if (cmX10Ati == control) OnX10Ati();
      if (cmFireDTV == control) OnFireDTV();
      if (cmOther == control) OnOther();
      if (btnNext == control) OnNextPage();
      if (btnBack == control) GUIWindowManager.ShowPreviousWindow();
      base.OnClicked(controlId, control, actionType);
    }


    void OnMicrosoftWithoutTeletext()
    {
      cmMicrosoftWithoutTeletext.Selected = true;
      cmMicrosoftWithTeletext.Selected = false;
      cmMicrosoftOther.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(@"Wizards\remote_mce_us.png");
      GUIControl.FocusControl(GetID, cmMicrosoftWithoutTeletext.GetID);
    }


    void OnMicrosoftWithTeletext()
    {
      cmMicrosoftWithoutTeletext.Selected = false;
      cmMicrosoftWithTeletext.Selected = true;
      cmMicrosoftOther.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(@"Wizards\remote_mce_eu.png");
      GUIControl.FocusControl(GetID, cmMicrosoftWithTeletext.GetID);
    }

    void OnMicrosoftOther()
    {
      cmMicrosoftWithoutTeletext.Selected = false;
      cmMicrosoftWithTeletext.Selected = false;
      cmMicrosoftOther.Selected = true;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(@"Wizards\remote_other.png");
      GUIControl.FocusControl(GetID, cmMicrosoftOther.GetID);
    }

    void OnHauppauge()
    {
      cmMicrosoftWithoutTeletext.Selected = false;
      cmMicrosoftWithTeletext.Selected = false;
      cmMicrosoftOther.Selected = false;
      cmHauppauge.Selected = true;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(@"Wizards\remote_hcw.png");
      GUIControl.FocusControl(GetID, cmHauppauge.GetID);
    }


    void OnX10Medion()
    {
      cmMicrosoftWithoutTeletext.Selected = false;
      cmMicrosoftWithTeletext.Selected = false;
      cmMicrosoftOther.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = true;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(@"Wizards\remote_x10.png");
      GUIControl.FocusControl(GetID, cmX10Medion.GetID);
    }


    void OnX10Ati()
    {
      cmMicrosoftWithoutTeletext.Selected = false;
      cmMicrosoftWithTeletext.Selected = false;
      cmMicrosoftOther.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = true;
      cmFireDTV.Selected = false;
      cmOther.Selected = false;
      imgRemote.SetFileName(@"Wizards\remote_x10.png");
      GUIControl.FocusControl(GetID, cmX10Ati.GetID);
    }


    void OnFireDTV()
    {
      cmMicrosoftWithoutTeletext.Selected = false;
      cmMicrosoftWithTeletext.Selected = false;
      cmMicrosoftOther.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = true;
      cmOther.Selected = false;
      imgRemote.SetFileName(@"Wizards\remote_firedtv.png");
      GUIControl.FocusControl(GetID, cmFireDTV.GetID);
    }


    void OnOther()
    {
      cmMicrosoftWithoutTeletext.Selected = false;
      cmMicrosoftWithTeletext.Selected = false;
      cmMicrosoftOther.Selected = false;
      cmHauppauge.Selected = false;
      cmX10Medion.Selected = false;
      cmX10Ati.Selected = false;
      cmFireDTV.Selected = false;
      cmOther.Selected = true;
      imgRemote.SetFileName(@"Wizards\remote_other.png");
      GUIControl.FocusControl(GetID, cmOther.GetID);
    }


    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        bool useMicrosoft = xmlreader.GetValueAsBool("remote", "MCE", true);
        string mceType = xmlreader.GetValueAsString("remote", "MCEType", "Teletext");
        bool useHCW = xmlreader.GetValueAsBool("remote", "HCW", false);
        bool useX10 = xmlreader.GetValueAsBool("remote", "X10", false);
        bool useX10Medion = xmlreader.GetValueAsBool("remote", "X10Medion", false);
        bool useX10Ati = xmlreader.GetValueAsBool("remote", "X10ATI", false);
        bool useFireDTV = xmlreader.GetValueAsBool("remote", "FireDTV", false);
        if (useMicrosoft)
          switch (mceType)
          {
            case "NoTeletext":
              OnMicrosoftWithoutTeletext();
              break;
            case "General":
              OnMicrosoftOther();
              break;
            default:
              OnMicrosoftWithTeletext();
              break;
          }
        else if (useHCW) OnHauppauge();
        else if (useX10 && useX10Medion) OnX10Medion();
        else if (useX10 && useX10Ati) OnX10Ati();
        else if (useFireDTV) OnFireDTV();
        else OnOther();
      }
    }


    void OnNextPage()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        if (cmMicrosoftWithTeletext.Selected)
        {
          xmlwriter.SetValueAsBool("remote", "MCE", true);
          xmlwriter.SetValue("remote", "MCEType", "Teletext");
        }
        else if (cmMicrosoftWithoutTeletext.Selected)
        {
          xmlwriter.SetValueAsBool("remote", "MCE", true);
          xmlwriter.SetValue("remote", "MCEType", "NoTeletext");
        }
        else if (cmMicrosoftOther.Selected)
        {
          xmlwriter.SetValueAsBool("remote", "MCE", true);
          xmlwriter.SetValue("remote", "MCEType", "General");
        }
        else
          xmlwriter.SetValueAsBool("remote", "MCE", false);

        xmlwriter.SetValueAsBool("remote", "HCW", cmHauppauge.Selected);
        xmlwriter.SetValueAsBool("remote", "X10", cmX10Medion.Selected || cmX10Ati.Selected);
        xmlwriter.SetValueAsBool("remote", "X10Medion", cmX10Medion.Selected);
        xmlwriter.SetValueAsBool("remote", "X10ATI", cmX10Ati.Selected);
        xmlwriter.SetValueAsBool("remote", "FireDTV", cmFireDTV.Selected);
      }
      GUIPropertyManager.SetProperty("#Wizard.Remote.Done", "yes");

      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESTART_REMOTE_CONTROLS, 0, 0, 0, 0, 0, null);
      GUIGraphicsContext.SendMessage(msg);

      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_CARDS_DETECTED);
    }
  }
}
