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
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using TunerLib;

namespace MediaPortal.TV.Scanning
{
  /// <summary>
  /// Summary description for DVBSTuning.
  /// </summary>
  public class DVBSTuning : ITuning
  {
    private struct TPList
    {
      public int TPfreq; // frequency
      public int TPpol; // polarisation 0=hori, 1=vert
      public int TPsymb; // symbol rate
    }

    private TVCaptureDevice _captureCard;
    private AutoTuneCallback _callback = null;
    private int _currentIndex = 0;
    private TPList[] _transponderList = new TPList[800];
    private int _count = 0;
    private string[] _tplFiles;
    private int _newChannels, _updatedChannels;
    private int _newRadioChannels, _updatedRadioChannels;
    private int _diseqcLoops = 1;
    private int _currentDiseqc = 1;
    //bool _reentrant = false;

    public DVBSTuning()
    {
    }

    #region ITuning Members

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback, string[] tplFileNames)
    {
      if ((tplFileNames == null) || (tplFileNames.Length == 0))
      {
        return;
      }

      GetNumberOfDiseqcs(card);

      Log.Info("dvbs-scan: diseqc loops:{0}", _diseqcLoops);
      _tplFiles = (string[]) tplFileNames.Clone();
      for (int i = 0; i < _tplFiles.Length; ++i)
      {
        Log.Info("dvbs-scan: diseqc:{0} file:{1}", i + 1, _tplFiles[i]);
      }
      _newRadioChannels = 0;
      _updatedRadioChannels = 0;
      _newChannels = 0;
      _updatedChannels = 0;

      _captureCard = card;
      _callback = statusCallback;

      _currentDiseqc = 1;
      _currentIndex = 0;
    }

    private void GetNumberOfDiseqcs(TVCaptureDevice card)
    {
      string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", card.FriendlyName));
      //
      // load card settings to check diseqc
      _diseqcLoops = 1;
      using (Settings xmlreader = new Settings(filename))
      {
        if (xmlreader.GetValueAsBool("dvbs", "useLNB2", false) == true)
        {
          _diseqcLoops++;
        }
        if (xmlreader.GetValueAsBool("dvbs", "useLNB3", false) == true)
        {
          _diseqcLoops++;
        }
        if (xmlreader.GetValueAsBool("dvbs", "useLNB4", false) == true)
        {
          _diseqcLoops++;
        }
      }
    }

    private void LoadFrequencies()
    {
      Log.Info("dvbs-scan: load transponders for LNB: {0}", _currentDiseqc);
      _currentIndex = 0;

      string fileName = _tplFiles[_currentDiseqc - 1];
      if (fileName == string.Empty)
      {
        Log.Info("dvbs-scan: no transponders found");
        _currentIndex = _count + 1;
        return;
      }
      _count = 0;
      string line;
      string[] tpdata;
      Log.Info("dvbs-scan:Opening {0}", fileName);
      // load transponder list and start scan
      TextReader tin = File.OpenText(fileName);

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
            tpdata = line.Split(new char[] {','});
            if (tpdata.Length != 3)
            {
              tpdata = line.Split(new char[] {';'});
            }
            if (tpdata.Length == 3)
            {
              try
              {
                _transponderList[_count].TPfreq = Int32.Parse(tpdata[0])*1000;
                switch (tpdata[1].ToLower())
                {
                  case "v":

                    _transponderList[_count].TPpol = 1;
                    break;
                  case "h":

                    _transponderList[_count].TPpol = 0;
                    break;
                  default:

                    _transponderList[_count].TPpol = 0;
                    break;
                }
                _transponderList[_count].TPsymb = Int32.Parse(tpdata[2]);
                _count += 1;
              }
              catch
              {
              }
            }
          }
        }
      } while (!(line == null));
      tin.Close();


      Log.Info("dvbs-scan:loaded:{0} transponders", _count);
      return;
    }

    public void Start()
    {
      _newRadioChannels = 0;
      _updatedRadioChannels = 0;
      _newChannels = 0;
      _updatedChannels = 0;

      Log.Info("dvbs-scan: Start()");
      _currentDiseqc = 1;
      _currentIndex = 0;
      LoadFrequencies();
      if (_callback != null)
      {
        _callback.OnProgress(0);
      }
    }

    public void Next()
    {
      UpdateStatus();
      Tune();
      Scan();
      GotoNextTransponder();
    }

    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback _callback)
    {
      // TODO:  Add DVBSTuning.AutoTuneRadio implementation
    }


    public int MapToChannel(string channel)
    {
      // TODO:  Add DVBSTuning.MapToChannel implementation
      return 0;
    }

    private void UpdateStatus()
    {
      if (IsFinished())
      {
        return;
      }
      float percent = ((float) _currentIndex)/((float) _count);
      percent *= 100.0f;
      if (_callback != null)
      {
        _callback.OnProgress((int) percent);
      }
      TPList transponder = _transponderList[_currentIndex];
      string chanDesc = String.Format("LNB:{0} Freq.:{1} MHz, Pol.:{2} SymbolRate:{3}",
                                      _currentDiseqc, ((float) transponder.TPfreq)/1000.0,
                                      GetPolarisation(transponder.TPpol), transponder.TPsymb);
      string description = String.Format("{0} locking...", chanDesc);
      if (_callback != null)
      {
        _callback.OnStatus(description);
      }
    }

    private string GetPolarisation(int pol)
    {
      if (pol == 0)
      {
        return "Horz.";
      }
      return "Vert.";
    }

    public bool IsFinished()
    {
      if (_currentIndex >= _count)
      {
        if (_currentDiseqc >= _diseqcLoops)
        {
          return true;
        }
      }
      return false;
    }

    private void DetectAvailableStreams()
    {
      if (IsFinished())
      {
        return;
      }
      TPList transponder = _transponderList[_currentIndex];
      string chanDesc = String.Format("LNB:{0} Freq.:{1} MHz, Pol.:{2} SymbolRate:{3}",
                                      _currentDiseqc, ((float) transponder.TPfreq)/1000.0,
                                      GetPolarisation(transponder.TPpol), transponder.TPsymb);
      string description = String.Format("{0}, getting info...", chanDesc);

      if (_callback != null)
      {
        _callback.OnStatus(description);
      }
      _captureCard.Process();
      if (_callback != null)
      {
        _callback.OnSignal(_captureCard.SignalQuality, _captureCard.SignalStrength);
      }
      _captureCard.StoreTunedChannels(false, true, ref _newChannels, ref _updatedChannels, ref _newRadioChannels,
                                      ref _updatedRadioChannels);
      if (_callback != null)
      {
        _callback.OnStatus2(String.Format("new tv:{0} new radio:{1} updated tv:{2} updated radio:{3}", _newChannels,
                                          _newRadioChannels, _updatedChannels, _updatedRadioChannels));
      }

      _captureCard.Process();
      if (_callback != null)
      {
        _callback.UpdateList();
      }
      Log.Info("dvbs-scan:onto next transponder");
    }

    private void GotoNextTransponder()
    {
      if (IsFinished())
      {
        return;
      }

      _currentIndex++;
      if (_currentIndex >= _count)
      {
        if (_currentDiseqc >= _diseqcLoops)
        {
          return;
        }
        else
        {
          _currentDiseqc++;
          LoadFrequencies();
          _currentIndex = 0;
        }
      }
    }

    private void Scan()
    {
      _captureCard.Process();
      if (_captureCard.SignalPresent())
      {
        DetectAvailableStreams();
      }
    }

    private void Tune()
    {
      if (IsFinished())
      {
        return;
      }
      DVBChannel newchan = new DVBChannel();
      newchan.NetworkID = -1;
      newchan.TransportStreamID = -1;
      newchan.ProgramNumber = -1;

      newchan.Polarity = _transponderList[_currentIndex].TPpol;
      newchan.Symbolrate = _transponderList[_currentIndex].TPsymb;
      newchan.FEC = (int) FECMethod.BDA_FEC_METHOD_NOT_DEFINED;
      newchan.Frequency = _transponderList[_currentIndex].TPfreq;

      Log.Info("dvbs-scan:tune transponder:{0} freq:{1} kHz symbolrate:{2} polarisation:{3}", _currentIndex,
               newchan.Frequency, newchan.Symbolrate, newchan.Polarity);
      _captureCard.Tune(newchan, _currentDiseqc);

      _captureCard.Process();
      if (_callback != null)
      {
        _callback.OnSignal(_captureCard.SignalQuality, _captureCard.SignalStrength);
      }
      Log.Info("dvbs-scan:signal quality:{0} signal strength:{1} signal present:{2}",
               _captureCard.SignalQuality, _captureCard.SignalStrength, _captureCard.SignalPresent());
    }

    #endregion
  }
}