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

using System;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Recording;

namespace WindowPlugins.GUISettings.Wizard.DVBS
{
  /// <summary>
  /// Summary description for GUIWizardDVBSLNBDetails.
  /// </summary>
  public class GUIWizardDVBSLNBDetails : GUIWindow
  {
    [SkinControl(5)] protected GUIButtonControl btnNext = null;
    [SkinControl(25)] protected GUIButtonControl btnBack = null;
    [SkinControl(100)] protected GUILabelControl lblLNB = null;
    [SkinControl(40)] protected GUICheckMarkControl cmDisEqcNone = null;
    [SkinControl(41)] protected GUICheckMarkControl cmDisEqcSimpleA = null;
    [SkinControl(42)] protected GUICheckMarkControl cmDisEqcSimpleB = null;
    [SkinControl(43)] protected GUICheckMarkControl cmDisEqcLevel1AA = null;
    [SkinControl(44)] protected GUICheckMarkControl cmDisEqcLevel1BA = null;
    [SkinControl(45)] protected GUICheckMarkControl cmDisEqcLevel1AB = null;
    [SkinControl(46)] protected GUICheckMarkControl cmDisEqcLevel1BB = null;


    [SkinControl(60)] protected GUICheckMarkControl cmLnbBandKU = null;
    [SkinControl(61)] protected GUICheckMarkControl cmLnbBandC = null;
    [SkinControl(62)] protected GUICheckMarkControl cmLnbBandCircular = null;


    private int LNBNumber = 1;
    private int maxLNBs = 1;
    private int card = 0;

    public GUIWizardDVBSLNBDetails()
    {
      GetID = (int) Window.WINDOW_WIZARD_DVBS_SELECT_DETAILS;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_dvbs_LNB2.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      card = Int32.Parse(GUIPropertyManager.GetProperty("#WizardCard"));
      maxLNBs = Int32.Parse(GUIPropertyManager.GetProperty("#WizardsDVBSLNB"));

      TVCaptureDevice captureCard = Recorder.Get(card);
      if (captureCard != null)
      {
        string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", captureCard.FriendlyName));

        using (Settings xmlwriter = new Settings(filename))
        {
          xmlwriter.SetValueAsBool("dvbs", "useLNB1", maxLNBs >= 1);
          xmlwriter.SetValueAsBool("dvbs", "useLNB2", maxLNBs >= 2);
          xmlwriter.SetValueAsBool("dvbs", "useLNB3", maxLNBs >= 3);
          xmlwriter.SetValueAsBool("dvbs", "useLNB4", maxLNBs >= 4);
        }
      }
      Update();
    }

    private void Update()
    {
      LoadSettings();
      lblLNB.Label = String.Format("Please specify the details for LNB:{0}", LNBNumber);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnNext)
      {
        OnNextPage();
        return;
      }
      if (control == btnBack)
      {
        OnPreviousPage();
      }
      if (controlId >= cmDisEqcNone.GetID && controlId <= cmDisEqcLevel1BB.GetID)
      {
        OnDisEqC(control);
      }

      if (controlId >= cmLnbBandKU.GetID && controlId <= cmLnbBandCircular.GetID)
      {
        Onband(control);
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void Onband(GUIControl control)
    {
      cmLnbBandKU.Selected = false;
      cmLnbBandC.Selected = false;
      cmLnbBandCircular.Selected = false;
      if (control == cmLnbBandKU)
      {
        cmLnbBandKU.Selected = true;
      }
      if (control == cmLnbBandC)
      {
        cmLnbBandC.Selected = true;
      }
      if (control == cmLnbBandCircular)
      {
        cmLnbBandCircular.Selected = true;
      }
    }

    private void OnDisEqC(GUIControl control)
    {
      cmDisEqcNone.Selected = false;
      cmDisEqcSimpleA.Selected = false;
      cmDisEqcSimpleB.Selected = false;
      cmDisEqcLevel1AA.Selected = false;
      cmDisEqcLevel1BA.Selected = false;
      cmDisEqcLevel1AB.Selected = false;
      cmDisEqcLevel1BB.Selected = false;
      if (control == cmDisEqcNone)
      {
        cmDisEqcNone.Selected = true;
      }
      if (control == cmDisEqcSimpleA)
      {
        cmDisEqcSimpleA.Selected = true;
      }
      if (control == cmDisEqcSimpleB)
      {
        cmDisEqcSimpleB.Selected = true;
      }
      if (control == cmDisEqcLevel1AA)
      {
        cmDisEqcLevel1AA.Selected = true;
      }
      if (control == cmDisEqcLevel1BA)
      {
        cmDisEqcLevel1BA.Selected = true;
      }
      if (control == cmDisEqcLevel1AB)
      {
        cmDisEqcLevel1AB.Selected = true;
      }
      if (control == cmDisEqcLevel1BB)
      {
        cmDisEqcLevel1BB.Selected = true;
      }
    }

    private void OnNextPage()
    {
      SaveSettings();
      if (LNBNumber < maxLNBs)
      {
        LNBNumber++;
        Update();
        return;
      }
      GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_DVBS_SELECT_TRANSPONDER);
    }

    private void OnPreviousPage()
    {
      SaveSettings();
      if (LNBNumber > 1)
      {
        LNBNumber--;
      }
      Update();
    }

    private void LoadSettings()
    {
      cmDisEqcNone.Selected = false;
      cmDisEqcSimpleA.Selected = false;
      cmDisEqcSimpleB.Selected = false;
      cmDisEqcLevel1AA.Selected = false;
      cmDisEqcLevel1BA.Selected = false;
      cmDisEqcLevel1AB.Selected = false;
      cmDisEqcLevel1BB.Selected = false;
      cmLnbBandKU.Selected = false;
      cmLnbBandC.Selected = false;
      cmLnbBandCircular.Selected = false;

      TVCaptureDevice captureCard = Recorder.Get(card);
      if (captureCard == null)
      {
        return;
      }
      string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", captureCard.FriendlyName));

      using (Settings xmlreader = new Settings(filename))
      {
        string lnbKey = String.Format("lnb{0}", LNBNumber);
        if (LNBNumber == 1)
        {
          lnbKey = "lnb";
        }


        lnbKey = String.Format("lnbKind{0}", LNBNumber);
        if (LNBNumber == 1)
        {
          lnbKey = "lnbKind";
        }
        int lnbKind = xmlreader.GetValueAsInt("dvbs", lnbKey, 0);
        switch (lnbKind)
        {
          case 0:
            cmLnbBandKU.Selected = true;
            break;
          case 1:
            cmLnbBandC.Selected = true;
            break;
          case 2:
            cmLnbBandCircular.Selected = true;
            break;
        }

        lnbKey = String.Format("diseqc{0}", LNBNumber);
        if (LNBNumber == 1)
        {
          lnbKey = "diseqc";
        }
        int diseqc = xmlreader.GetValueAsInt("dvbs", lnbKey, 0);
        switch (diseqc)
        {
          case 1:
            cmDisEqcSimpleA.Selected = true;
            break;
          case 2:
            cmDisEqcSimpleB.Selected = true;
            break;
          case 3:
            cmDisEqcLevel1AA.Selected = true;
            break;
          case 4:
            cmDisEqcLevel1BA.Selected = true;
            break;
          case 5:
            cmDisEqcLevel1AB.Selected = true;
            break;
          case 6:
            cmDisEqcLevel1BB.Selected = true;
            break;
          default:
            cmDisEqcNone.Selected = true;
            break;
        }
      }
    }

    private void SaveSettings()
    {
      TVCaptureDevice captureCard = Recorder.Get(card);
      if (captureCard == null)
      {
        return;
      }
      string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", captureCard.FriendlyName));
      using (Settings xmlwriter = new Settings(filename))
      {
        string lnbKey = String.Format("useLNB{0}", LNBNumber);
        xmlwriter.SetValueAsBool("dvbs", lnbKey, true);

        lnbKey = String.Format("diseqc{0}", LNBNumber);
        if (LNBNumber == 1)
        {
          lnbKey = "diseqc";
        }
        int ivalue = 0;
        if (cmDisEqcSimpleA.Selected)
        {
          ivalue = 1;
        }
        if (cmDisEqcSimpleB.Selected)
        {
          ivalue = 2;
        }
        if (cmDisEqcLevel1AA.Selected)
        {
          ivalue = 3;
        }
        if (cmDisEqcLevel1BA.Selected)
        {
          ivalue = 4;
        }
        if (cmDisEqcLevel1AB.Selected)
        {
          ivalue = 5;
        }
        if (cmDisEqcLevel1BB.Selected)
        {
          ivalue = 6;
        }
        xmlwriter.SetValue("dvbs", lnbKey, ivalue);

        lnbKey = String.Format("lnb{0}", LNBNumber);
        if (LNBNumber == 1)
        {
          lnbKey = "lnb";
        }
        ivalue = 22;
        xmlwriter.SetValue("dvbs", lnbKey, ivalue);

        lnbKey = String.Format("lnbKind{0}", LNBNumber);
        if (LNBNumber == 1)
        {
          lnbKey = "lnbKind";
        }
        ivalue = 0;
        if (cmLnbBandKU.Selected == true)
        {
          ivalue = 0;
        }
        if (cmLnbBandC.Selected == true)
        {
          ivalue = 1;
        }
        if (cmLnbBandCircular.Selected == true)
        {
          ivalue = 2;
        }
        xmlwriter.SetValue("dvbs", lnbKey, ivalue);
      }
    }
  }
}