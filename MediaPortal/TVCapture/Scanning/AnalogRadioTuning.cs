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
using System.Threading;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Scanning
{
  /// <summary>
  /// Class which can search & find all tv channels for an analog capture card
  /// </summary>
  public class AnalogRadioTuning : ITuning
  {
    private AutoTuneCallback _callback = null;
    private TVCaptureDevice _captureCard;
    private int _currentStationFreq = 0;
    private int _maxStationFreq;
    private int _minStationFreq;
    //bool stopped = true;
    private int _updatedRadioStations = 0;
    private int _newRadioStations = 0;

    public AnalogRadioTuning()
    {
    }

    #region ITuning Members

    public void Start()
    {
      _newRadioStations = 0;
      _updatedRadioStations = 0;

      _currentStationFreq = _minStationFreq - 100000;
      _callback.OnSignal(0, 0);
      _callback.OnProgress(0);
    }

    public void Next()
    {
      _currentStationFreq += 100000;
      if (IsFinished())
      {
        return;
      }
      Tune();
    }

    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback statusCallback)
    {
      _captureCard = card;
      card.DeleteGraph();
      card.RadioChannelMinMax(out _minStationFreq, out _maxStationFreq);
      if (_minStationFreq == -1)
      {
        _minStationFreq = 87500000;
      }
      else
      {
        _minStationFreq = (int) (Math.Ceiling(((double) _minStationFreq/100000d)))*100000;
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

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback, string[] parameters)
    {
      _callback.OnEnded();
    }


    public bool IsFinished()
    {
      if (_currentStationFreq > _maxStationFreq)
      {
        return true;
      }
      return false;
    }

    private void Tune()
    {
      float percent = ((float) _currentStationFreq - (float) _minStationFreq)/
                      ((float) _maxStationFreq - (float) _minStationFreq);
      percent *= 100.0f;
      _callback.OnProgress((int) percent);
      float frequency = ((float) _currentStationFreq)/1000000f;
      string description = String.Format("Radio Station: frequency:{0:###.##} MHz.", frequency);
      _callback.OnStatus(description);
      TuneStation();
      int strength = SignalStrength(_captureCard.RadioSensitivity);
      _callback.OnSignal(strength, strength);
      if (strength == 100)
      {
        int tvTemp = 0;
        _callback.OnStatus2(String.Format("New Radio:{0} updated Radio:{1} ", _newRadioStations, _updatedRadioStations));
        _captureCard.StoreTunedChannels(true, false, ref tvTemp, ref tvTemp, ref _newRadioStations,
                                        ref _updatedRadioStations);
        _callback.OnStatus2(String.Format("New Radio:{0} updated Radio:{1} ", _newRadioStations, _updatedRadioStations));
        _callback.OnNewChannel();
        _callback.UpdateList();
        return;
      }
    }

    private int SignalStrength(int sensitivity)
    {
      int i = 0;
      for (i = 0; i < sensitivity*2; i++)
      {
        if (!_captureCard.SignalPresent())
        {
          break;
        }
        Thread.Sleep(50);
      }
      return ((i*50)/sensitivity);
    }

    private void TuneStation()
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