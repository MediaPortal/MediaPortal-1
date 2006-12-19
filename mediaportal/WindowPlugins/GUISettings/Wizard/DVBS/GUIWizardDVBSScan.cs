#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Xml;
using System.Threading;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Scanning;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;

namespace WindowPlugins.GUISettings.Wizard.DVBS
{
  /// <summary>
  /// Summary description for GUIWizardDVBSCountry.
  /// </summary>
  public class GUIWizardDVBSScan : GUIWizardScanBase
  {
    int m_diseqcLoops;

    public GUIWizardDVBSScan()
    {
      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_DVBS_SCAN;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_DVBS_scan.xml");
    }

    protected override ITuning GetTuningInterface(TVCaptureDevice captureCard)
    {
      m_diseqcLoops = 1;
      string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", captureCard.FriendlyName));
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
      {
        if (xmlreader.GetValueAsBool("dvbs", "useLNB2", false) == true)
          m_diseqcLoops++;
        if (xmlreader.GetValueAsBool("dvbs", "useLNB3", false) == true)
          m_diseqcLoops++;
        if (xmlreader.GetValueAsBool("dvbs", "useLNB4", false) == true)
          m_diseqcLoops++;
      }
      string[] tplFiles = new string[m_diseqcLoops];
      for (int i = 0; i < m_diseqcLoops; ++i)
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
        {
          string key = String.Format("sat{0}", i+1);
          tplFiles[i] = xmlreader.GetValue("dvbs", key);
        }
      }
      ITuning tuning = new DVBSTuning();
      tuning.AutoTuneTV(captureCard, this, tplFiles);
      return tuning;
    }

    protected override void OnScanDone()
    {
      GUIPropertyManager.SetProperty("#Wizard.DVBS.Done", "yes");
    }
    protected override NetworkType Network()
    {
      return NetworkType.DVBS;
    }
  }
}
