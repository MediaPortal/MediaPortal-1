/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using DShowNET;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.GUI.Library;
using System.Xml;

namespace MediaPortal.TV.Scanning
{

  /// <summary>
  /// Summary description for DVBTTuning.
  /// </summary>
  public class DVBTTuning : ITuning
  {
    TVCaptureDevice _captureCard;
    AutoTuneCallback _callback = null;
    ArrayList _listFrequencies = new ArrayList();
    int _currentFrequencyIndex = 0;
    int _scanOffset = 0;
    int _newChannels, _updatedChannels;
    int _newRadioChannels, _updatedRadioChannels;

    bool reentrant = false;
    public DVBTTuning()
    {
    }

    #region ITuning Members

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
    {
      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:Opening dvbt.xml");
      XmlDocument doc = new XmlDocument();
      doc.Load("Tuningparameters/dvbt.xml");

      FormCountry formCountry = new FormCountry();
      XmlNodeList countryList = doc.DocumentElement.SelectNodes("/dvbt/country");
      foreach (XmlNode nodeCountry in countryList)
      {
        string name = nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
        formCountry.AddCountry(name);
      }
      formCountry.ShowDialog();
      string countryName = formCountry.countryName;
      AutoTuneTV(card, statusCallback, countryName);
    }

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback, string[] countryName)
    {
    }

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback, string countryName)
    {
      _newRadioChannels = 0;
      _updatedRadioChannels = 0;
      _newChannels = 0;
      _updatedChannels = 0;

      _captureCard = card;
      _callback = statusCallback;

      _listFrequencies.Clear();
      _currentFrequencyIndex = -1;
      String countryCode = String.Empty;

      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:Opening dvbt.xml");
      XmlDocument doc = new XmlDocument();
      doc.Load("Tuningparameters/dvbt.xml");

      if (countryName == String.Empty) return;
      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:auto tune for {0}", countryName);
      _listFrequencies.Clear();

      XmlNodeList countryList = doc.DocumentElement.SelectNodes("/dvbt/country");
      foreach (XmlNode nodeCountry in countryList)
      {
        string name = nodeCountry.Attributes.GetNamedItem(@"name").InnerText;
        if (name != countryName) continue;
        Log.WriteFile(Log.LogType.Capture, "dvbt-scan:found country {0} in dvbt.xml", countryName);
        try
        {
          _scanOffset = XmlConvert.ToInt32(nodeCountry.Attributes.GetNamedItem(@"offset").InnerText);
          Log.WriteFile(Log.LogType.Capture, "dvbt-scan:scanoffset: {0} ", _scanOffset);
        }
        catch (Exception) { }

        XmlNodeList frequencyList = nodeCountry.SelectNodes("carrier");
        Log.WriteFile(Log.LogType.Capture, "dvbt-scan:number of carriers:{0}", frequencyList.Count);
        int[] carrier;
        foreach (XmlNode node in frequencyList)
        {
          carrier = new int[2];
          carrier[0] = XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"frequency").InnerText);
          try
          {
            carrier[1] = XmlConvert.ToInt32(node.Attributes.GetNamedItem(@"bandwidth").InnerText);
          }
          catch (Exception) { }

          if (carrier[1] == 0) carrier[1] = 8;
          _listFrequencies.Add(carrier);
          Log.WriteFile(Log.LogType.Capture, "dvbt-scan:added:{0}", carrier[0]);
        }
      }
      if (_listFrequencies.Count == 0) return;

      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:loaded:{0} _listFrequencies", _listFrequencies.Count);
      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:{0} has a scan offset of {1}KHz", countryCode, _scanOffset);

      return;
    }

    public void Start()
    {
      _currentFrequencyIndex = 0;
      _callback.OnProgress(0);
    }

    public bool IsFinished()
    {
      if (_currentFrequencyIndex  >= _listFrequencies.Count) return true;
      return false;
    }

    public void Next()
    {
      if ( IsFinished() ) return;
      UpdateStatus();
      Tune(0);
      Scan();

      if (_scanOffset != 0)
      {
        Tune(-_scanOffset);
        Scan();
        Tune(_scanOffset);
        Scan();
      }
      _currentFrequencyIndex++;
    } 
    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback _callback)
    {
      // TODO:  Add DVBTTuning.AutoTuneRadio implementation
    }


    public int MapToChannel(string channel)
    {
      // TODO:  Add DVBTTuning.MapToChannel implementation
      return 0;
    }

    void UpdateStatus()
    {
      int currentFreq = _currentFrequencyIndex;
      if (_currentFrequencyIndex < 0) currentFreq = 0;
      float percent = ((float)currentFreq) / ((float)_listFrequencies.Count);
      percent *= 100.0f;
      _callback.OnProgress((int)percent);
      int[] tmp = _listFrequencies[currentFreq] as int[];
      //Log.WriteFile(Log.LogType.Capture,"dvbt-scan:FREQ: {0} BWDTH: {1}", tmp[0], tmp[1]);
      float frequency = tmp[0];
      frequency /= 1000;
      string description = String.Format("Transponder:{0:###.##} MHz. Bandwidth:{1} MHz", frequency, tmp[1]);
      _callback.OnStatus(description);
    }

    void DetectAvailableStreams()
    {
      Log.Write("dvbt-scan:ScanChannels() {0}/{1}", _currentFrequencyIndex, _listFrequencies.Count);
      if (_currentFrequencyIndex < 0 || _currentFrequencyIndex >= _listFrequencies.Count) return;
      int[] tmp;
      tmp = (int[])_listFrequencies[_currentFrequencyIndex];
      _captureCard.Process();
      string description = String.Format("Transponder:{0:###.##} MHz. Getting info...", tmp[0] / 1000);
      _callback.OnStatus(description);

      _captureCard.Process();
      _callback.OnSignal(_captureCard.SignalQuality, _captureCard.SignalStrength);
      Log.Write("ScanChannels() {0} {1}", _captureCard.SignalStrength, _captureCard.SignalQuality);
      _callback.OnStatus2(String.Format("new tv:{0} new radio:{1} updated tv:{2} updated radio:{3}", _newChannels, _newRadioChannels, _updatedChannels, _updatedRadioChannels));
      _captureCard.StoreTunedChannels(false, true, ref _newChannels, ref _updatedChannels, ref _newRadioChannels, ref _updatedRadioChannels);
      _callback.OnStatus2(String.Format("new tv:{0} new radio:{1} updated tv:{2} updated radio:{3}", _newChannels, _newRadioChannels, _updatedChannels, _updatedRadioChannels));
      _callback.UpdateList();
      Log.Write("dvbt-scan:ScanChannels() done");
    }

    void Scan()
    {
      _captureCard.Process();
      if (_captureCard.SignalPresent())
      {
        DetectAvailableStreams();
      }
    }

    void Tune(int offset)
    {
      Log.Write("dvbt-scan:ScanNextFrequency() {0}/{1}", _currentFrequencyIndex, _listFrequencies.Count);
      if (_currentFrequencyIndex < 0) _currentFrequencyIndex = 0;
      if (_currentFrequencyIndex >= _listFrequencies.Count) return;

      DVBChannel chan = new DVBChannel();
      int[] tmp;
      tmp = (int[])_listFrequencies[_currentFrequencyIndex];
      chan.Frequency = tmp[0];
      chan.Bandwidth = tmp[1];
      chan.Frequency += offset;
      chan.NetworkID = -1;
      chan.TransportStreamID = -1;
      chan.ProgramNumber = -1;

      float frequency = ((float)chan.Frequency) / 1000f;
      string description = String.Format("Transponder:{0:###.##} MHz. Bandwidth:{1} MHz locking...", frequency, tmp[1]);
      _callback.OnStatus(description);

      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:tune to freq:{0} bandwidth:{1} offset:{2}", chan.Frequency, chan.Bandwidth, offset);
      _captureCard.Tune(chan, 0);

      //tune locking : 2 seconds
      for (int i = 0; i < 5; ++i)
      {
        _captureCard.Process();
        if (_captureCard.SignalPresent()) break;
        System.Threading.Thread.Sleep(400);
      }
      _captureCard.Process();
      _callback.OnSignal(_captureCard.SignalQuality, _captureCard.SignalStrength);
      Log.WriteFile(Log.LogType.Capture, "dvbt-scan:locked:{0} tuned quality:{1} strength:{2}",
              _captureCard.SignalPresent(), _captureCard.SignalQuality, _captureCard.SignalStrength);
      return;
    }

    #endregion
  }
}
