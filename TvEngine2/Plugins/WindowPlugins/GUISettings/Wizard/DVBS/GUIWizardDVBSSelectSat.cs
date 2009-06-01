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
using System.Collections;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Recording;

namespace WindowPlugins.GUISettings.Wizard.DVBS
{
  /// <summary>
  /// Summary description for GUIWizardDVBSSelectSat.
  /// </summary>
  public class GUIWizardDVBSSelectSat : GUIWindow
  {
    private class Transponder
    {
      public string SatName;
      public string FileName;

      public override string ToString()
      {
        return SatName;
      }
    }

    [SkinControl(10)] protected GUILabelControl lblLNB1 = null;
    [SkinControl(11)] protected GUILabelControl lblLNB2 = null;
    [SkinControl(12)] protected GUILabelControl lblLNB3 = null;
    [SkinControl(13)] protected GUILabelControl lblLNB4 = null;


    [SkinControl(50)] protected GUIButtonControl btnLNB1 = null;
    [SkinControl(51)] protected GUIButtonControl btnLNB2 = null;
    [SkinControl(52)] protected GUIButtonControl btnLNB3 = null;
    [SkinControl(53)] protected GUIButtonControl btnLNB4 = null;


    [SkinControl(25)] protected GUIButtonControl btnBack = null;
    [SkinControl(26)] protected GUIButtonControl btnNext = null;

    private int maxLNBs = 1;
    private int card = 0;
    private Transponder[] lnbConfig = new Transponder[5];

    public GUIWizardDVBSSelectSat()
    {
      GetID = (int) Window.WINDOW_WIZARD_DVBS_SELECT_TRANSPONDER;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_dvbs_LNB3.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      card = Int32.Parse(GUIPropertyManager.GetProperty("#WizardCard"));
      maxLNBs = Int32.Parse(GUIPropertyManager.GetProperty("#WizardsDVBSLNB"));
      Update();
    }

    private void Update()
    {
      lblLNB1.Label = "-";
      lblLNB2.Label = "-";
      lblLNB3.Label = "-";
      lblLNB4.Label = "-";
      btnLNB1.Disabled = !(maxLNBs >= 1);
      btnLNB2.Disabled = !(maxLNBs >= 2);
      btnLNB3.Disabled = !(maxLNBs >= 3);
      btnLNB4.Disabled = !(maxLNBs >= 4);
      if (lnbConfig[1] != null)
      {
        lblLNB1.Label = lnbConfig[1].SatName;
      }
      if (lnbConfig[2] != null)
      {
        lblLNB2.Label = lnbConfig[2].SatName;
      }
      if (lnbConfig[3] != null)
      {
        lblLNB3.Label = lnbConfig[3].SatName;
      }
      if (lnbConfig[4] != null)
      {
        lblLNB4.Label = lnbConfig[4].SatName;
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnLNB1)
      {
        OnLNB(1);
      }
      if (control == btnLNB2)
      {
        OnLNB(2);
      }
      if (control == btnLNB3)
      {
        OnLNB(3);
      }
      if (control == btnLNB4)
      {
        OnLNB(4);
      }
      if (control == btnNext)
      {
        OnNextPage();
      }
      if (control == btnBack)
      {
        OnPreviousPage();
      }
      base.OnClicked(controlId, control, actionType);
    }

    private Transponder LoadTransponder(string file)
    {
      TextReader tin = File.OpenText(file);
      Transponder ts = new Transponder();
      ts.FileName = file;
      string line = null;
      do
      {
        line = null;
        line = tin.ReadLine();
        if (line != null)
        {
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
            {
              continue;
            }
            int pos = line.IndexOf("satname=");
            if (pos >= 0)
            {
              ts.SatName = line.Substring(pos + "satname=".Length);
              tin.Close();
              return ts;
            }
          }
        }
      } while (!(line == null));
      tin.Close();
      return null;
    }

    private void OnLNB(int lnb)
    {
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      dlg.Reset();
      dlg.SetHeading("Select transponder"); //Menu
      string[] files = Directory.GetFiles(Config.GetSubFolder(Config.Dir.Config, "Tuningparameters"));
      ArrayList items = new ArrayList();
      foreach (string file in files)
      {
        if (file.ToLower().IndexOf(".tpl") >= 0)
        {
          Transponder ts = LoadTransponder(file);
          if (ts != null)
          {
            GUIListItem item = new GUIListItem(ts.SatName);
            item.MusicTag = (object) ts;
            dlg.Add(item);
            items.Add(ts);
          }
        }
      }
      dlg.DoModal(GetID);
      int itemNo = dlg.SelectedLabel;
      if (itemNo < 0)
      {
        return;
      }
      lnbConfig[lnb] = (Transponder) items[itemNo];
      Update();
    }

    private void OnNextPage()
    {
      TVCaptureDevice captureCard = Recorder.Get(card);
      if (captureCard != null)
      {
        string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", captureCard.FriendlyName));

        using (Settings xmlwriter = new Settings(filename))
        {
          if (lnbConfig[1] != null)
          {
            xmlwriter.SetValue("dvbs", "sat1", lnbConfig[1].FileName);
          }
          if (lnbConfig[2] != null)
          {
            xmlwriter.SetValue("dvbs", "sat2", lnbConfig[2].FileName);
          }
          if (lnbConfig[3] != null)
          {
            xmlwriter.SetValue("dvbs", "sat3", lnbConfig[3].FileName);
          }
          if (lnbConfig[4] != null)
          {
            xmlwriter.SetValue("dvbs", "sat4", lnbConfig[4].FileName);
          }
        }
      }
      GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_DVBS_SCAN);
    }

    private void OnPreviousPage()
    {
      GUIWindowManager.ShowPreviousWindow();
    }
  }
}