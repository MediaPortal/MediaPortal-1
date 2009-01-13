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

#region usings

using System;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Scanning;

#endregion

namespace MediaPortal.TV.Recording
{
  internal class AutoTuneCommand : CardCommand
  {
    private NetworkType _networkType = NetworkType.Unknown;
    private int _cardNo = -1;
    private AutoTuneCallback _autoTuneCallback = null;

    public AutoTuneCommand(NetworkType networkType, int cardNo, AutoTuneCallback autoTuneCallback)
    {
      _networkType = networkType;
      _cardNo = cardNo;
      _autoTuneCallback = autoTuneCallback;
    }

    public override void Execute(CommandProcessor handler)
    {
      if (_cardNo < 0 || _cardNo >= handler.TVCards.Count)
      {
        Succeeded = false;
        ErrorMessage = GUILocalizeStrings.Get(1502); // "No free card available";
        return;
      }
      TVCaptureDevice dev = handler.TVCards[_cardNo];
      dev.CreateGraph();
      ITuning tuning = GetTunningInterface(dev);
      if (tuning == null)
      {
        Succeeded = false;
        ErrorMessage = GUILocalizeStrings.Get(1502); // "No free card available";
        return;
      }

      tuning.Start();
      while (!tuning.IsFinished())
      {
        tuning.Next();
      }

      dev.DeleteGraph();
      dev = null;
      tuning = null;
      Succeeded = true;
      if (_autoTuneCallback != null)
      {
        _autoTuneCallback.OnEnded();
      }
    }


    protected ITuning GetTunningInterface(TVCaptureDevice dev)
    {
      ITuning tuning = null;
      switch (_networkType)
      {
        case NetworkType.ATSC:
          tuning = new ATSCTuning();
          tuning.AutoTuneTV(dev, _autoTuneCallback, null);
          break;
        case NetworkType.DVBC:
          string country = GUIPropertyManager.GetProperty("#WizardCountry");
          String[] parameters = new String[1];
          parameters[0] = country;
          tuning = new DVBCTuning();
          tuning.AutoTuneTV(dev, _autoTuneCallback, parameters);
          break;
        case NetworkType.DVBS:
          int m_diseqcLoops = 1;
          string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", dev.FriendlyName));
          using (Settings xmlreader = new Settings(filename))
          {
            if (xmlreader.GetValueAsBool("dvbs", "useLNB2", false))
            {
              m_diseqcLoops++;
            }
            if (xmlreader.GetValueAsBool("dvbs", "useLNB3", false))
            {
              m_diseqcLoops++;
            }
            if (xmlreader.GetValueAsBool("dvbs", "useLNB4", false))
            {
              m_diseqcLoops++;
            }
          }
          string[] tplFiles = new string[m_diseqcLoops];
          for (int i = 0; i < m_diseqcLoops; ++i)
          {
            using (Settings xmlreader = new Settings(filename))
            {
              string key = String.Format("sat{0}", i + 1);
              tplFiles[i] = xmlreader.GetValue("dvbs", key);
            }
          }
          tuning = new DVBSTuning();
          tuning.AutoTuneTV(dev, _autoTuneCallback, tplFiles);
          break;
        case NetworkType.DVBT:
          string countryT = GUIPropertyManager.GetProperty("#WizardCountry");
          String[] parametersT = new String[1];
          parametersT[0] = countryT;
          tuning = new DVBTTuning();
          tuning.AutoTuneTV(dev, _autoTuneCallback, parametersT);
          break;
        case NetworkType.Analog:
          tuning = new AnalogTVTuning();
          tuning.AutoTuneTV(dev, _autoTuneCallback, null);
          break;
      }
      return tuning;
    }
  }
}