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
using System.Collections.Generic;
using System.Threading;
using DirectShowLib;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Scanning
{
  /// <summary>
  /// Class which can search & find all tv channels for an analog capture card
  /// </summary>
  public class AnalogTVTuning : ITuning
  {
    private const int MaxChannelNo = 400;
    private int _currentChannel = 0, _maxChannel = 0, _minChannel = 0;
    private AutoTuneCallback _callback = null;
    private TVCaptureDevice _captureCard;
    private float _lastFrequency = -1f;
    private int _newChannels, _updatedChannels;

    public AnalogTVTuning()
    {
    }

    #region ITuning Members

    public void Start()
    {
      _newChannels = 0;
      _updatedChannels = 0;
      _currentChannel = _minChannel - 1;
      _callback.OnSignal(0, 0);
      _callback.OnProgress(0);
    }

    public void Next()
    {
      _currentChannel++;
      if (IsFinished())
      {
        _callback.OnEnded();
        return;
      }
      Tune();
    }

    private void Tune()
    {
      float percent = (((float) _currentChannel) - (float) _minChannel)/((float) _maxChannel - (float) _minChannel);
      percent *= 100.0f;
      _callback.OnProgress((int) percent);
      TuneChannel();
      //Wait for tuner to lock signal
      Thread.Sleep(500);
      float frequency = (float) _captureCard.VideoFrequency();
      if (frequency != _lastFrequency)
      {
        _lastFrequency = frequency;
        frequency /= 1000000f;
        string description = String.Format("channel:{0} frequency:{1:###.##} MHz.", _currentChannel, frequency);
        _callback.OnStatus(description);
        _callback.OnSignal(_captureCard.SignalQuality, _captureCard.SignalStrength);
        if (_captureCard.SignalPresent())
        {
          int radioTemp = 0;
          _callback.OnStatus2(String.Format("New tv:{0} updated tv:{1} ", _newChannels, _updatedChannels));
          _captureCard.StoreTunedChannels(false, true, ref _newChannels, ref _updatedChannels, ref radioTemp,
                                          ref radioTemp);
          _callback.OnStatus2(String.Format("New tv:{0} updated tv:{1} ", _newChannels, _updatedChannels));
          _callback.OnNewChannel();
          _callback.UpdateList();
          return;
        }
      }
      else
      {
        _callback.OnSignal(0, 0);
      }
    }


    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback statusCallback)
    {
    }


    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback, string[] parameters)
    {
      _newChannels = 0;
      _updatedChannels = 0;
      _lastFrequency = -1f;
      _captureCard = card;
      card.TVChannelMinMax(out _minChannel, out _maxChannel);
      if (_minChannel == -1)
      {
        _minChannel = 1;
      }
      if (_maxChannel == -1)
      {
        _maxChannel = MaxChannelNo;
      }
      _callback = statusCallback;
      _callback.OnSignal(0, 0);
      _callback.OnProgress(0);
    }

    public bool IsFinished()
    {
      if (_currentChannel > _maxChannel)
      {
        return true;
      }
      return false;
    }

    private void TuneChannel()
    {
      TVChannel chan = new TVChannel();
      chan.Number = _currentChannel;
      chan.Country = _captureCard.DefaultCountryCode;
      chan.TVStandard = AnalogVideoStandard.None;
      if (!_captureCard.ViewChannel(chan))
      {
        _callback.OnStatus("Search has finished.");
        _callback.OnSignal(0, 0);
        _callback.OnProgress(100);
        _callback.OnEnded();
        _captureCard.DeleteGraph();
        return;
      }
    }

    public int MapToChannel(string channelName)
    {
      List<TVChannel> channels = new List<TVChannel>();
      TVDatabase.GetChannels(ref channels);
      for (int i = 0; i < channels.Count; ++i)
      {
        TVChannel chan = channels[i];
        if (chan.Name == channelName)
        {
          chan.Number = _currentChannel;
          chan.Frequency = _captureCard.VideoFrequency();
          chan.Country = _captureCard.DefaultCountryCode;
          TVDatabase.UpdateChannel(chan, chan.Sort);

          TVDatabase.MapChannelToCard(chan.ID, _captureCard.ID);

          TVGroup group = new TVGroup();
          group.GroupName = "Analog";
          int groupid = TVDatabase.AddGroup(group);
          group.ID = groupid;
          TVDatabase.MapChannelToGroup(group, chan);
        }
      }
      return _currentChannel;
    }

    #endregion
  }
}