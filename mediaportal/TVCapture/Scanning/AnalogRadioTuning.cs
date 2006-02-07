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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using DirectShowLib;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Scanning
{
  /// <summary>
  /// Class which can search & find all tv channels for an analog capture card
  /// </summary>
  public class AnalogRadioTuning : ITuning
  {
    AutoTuneCallback _callback = null;
    TVCaptureDevice _captureCard;
    int _currentStationFreq = 0;
    int _maxStationFreq;
    int _minStationFreq;
    bool stopped = true;

    public AnalogRadioTuning()
    {
    }
    #region ITuning Members

    public void Start()
    {
      _currentStationFreq = _minStationFreq;
      _callback.OnSignal(0, 0);
      _callback.OnProgress(0);
    }
    public void Next()
    {
      if (IsFinished()) return;
      Tune();
      _currentStationFreq += 100000;
    }

    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback statusCallback)
    {
      _captureCard = card;
      card.RadioChannelMinMax(out _minStationFreq, out _maxStationFreq);
      if (_minStationFreq == -1)
      {
        _minStationFreq = 87500000;
      }
      else
      {
        _minStationFreq = (int)(Math.Floor(((double)_minStationFreq / 100000d))) * 100000;
      }
      if (_maxStationFreq == -1)
      {
        _maxStationFreq = 108000000;
      }
      _currentStationFreq = _minStationFreq;
      _callback = statusCallback;
      _callback.OnSignal(0, 0);
      _callback.OnProgress(0);
    }

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback, string countryName)
    {
    }
    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback, string[] countryName)
    {
    }
    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback)
    {
      _callback.OnEnded();
    }

    


    public bool IsFinished()
    {
      if (_currentStationFreq > _maxStationFreq)
        return true;
      return false;
    }

    void Tune()
    {
      float percent = ((float)_currentStationFreq - (float)_minStationFreq) / ((float)_maxStationFreq - (float)_minStationFreq);
      percent *= 100.0f;
      _callback.OnProgress((int)percent);
      float frequency = ((float)_currentStationFreq) / 1000000f;
      string description = String.Format("Radio Station: frequency:{0:###.##} MHz.", frequency);
      _callback.OnStatus(description);
      TuneStation();
      int strength = SignalStrength(_captureCard.RadioSensitivity);
      _callback.OnSignal(strength, strength);
      if (strength == 100)
      {
        _callback.OnNewChannel();
        return;
      }
    }

    int SignalStrength(int sensitivity)
    {
      int i = 0;
      for (i = 0; i < sensitivity * 2; i++)
      {
        if (!_captureCard.SignalPresent())
        {
          break;
        }
        System.Threading.Thread.Sleep(50);
      }
      return ((i * 50) / sensitivity);
    }
    void TuneStation()
    {
      _captureCard.TuneRadioFrequency(_currentStationFreq);
    }

    public int MapToChannel(string channelName)
    {
      RadioStation station;
      RadioDatabase.GetStation(channelName, out station);
      station.Frequency = _currentStationFreq;
      station.Scrambled = false;
      RadioDatabase.UpdateStation(station);
      RadioDatabase.MapChannelToCard(station.ID, _captureCard.ID);
      return station.Channel;
    }

    #endregion
  }
}
