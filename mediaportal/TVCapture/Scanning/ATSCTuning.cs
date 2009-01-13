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
using DirectShowLib.BDA;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Scanning
{
  /// <summary>
  /// Summary description for ATSCTuning.
  /// </summary>
  public class ATSCTuning : ITuning
  {
    //#DM - changed maximum number of channels to tune to 69#
    //const int MaxATSCChannel = 255;
    private const int MaxATSCChannel = 69;

    private TVCaptureDevice _captureCard;
    private AutoTuneCallback _callback = null;
    private int _currentIndex = -1;

    private int _newChannels, _updatedChannels;
    private int _newRadioChannels, _updatedRadioChannels;

    public ATSCTuning()
    {
    }

    #region ITuning Members

    public void Start()
    {
      _newRadioChannels = 0;
      _updatedRadioChannels = 0;
      _newChannels = 0;
      _updatedChannels = 0;

      //#DM - changed starting channel number so tuning starts at 2#
      //_currentIndex = 0;
      _currentIndex = 2;
      _callback.OnProgress(0);
    }

    public void Next()
    {
      if (_currentIndex > MaxATSCChannel)
      {
        return;
      }
      UpdateStatus();
      Tune();
      Scan();
      _currentIndex++;
    }

    public void AutoTuneTV(TVCaptureDevice card, AutoTuneCallback statusCallback, string[] tuningFile)
    {
      _newRadioChannels = 0;
      _updatedRadioChannels = 0;
      _newChannels = 0;
      _updatedChannels = 0;
      _captureCard = card;
      _callback = statusCallback;

      _currentIndex = 0;
      return;
    }

    public void AutoTuneRadio(TVCaptureDevice card, AutoTuneCallback _callback)
    {
      // TODO:  Add ATSCTuning.AutoTuneRadio implementation
    }


    public int MapToChannel(string channel)
    {
      // TODO:  Add ATSCTuning.MapToChannel implementation
      return 0;
    }

    private void UpdateStatus()
    {
      int index = _currentIndex;
      if (index < 0)
      {
        index = 0;
      }
      float percent = ((float) index)/((float) MaxATSCChannel);
      percent *= 100.0f;
      _callback.OnProgress((int) percent);
    }

    public bool IsFinished()
    {
      if (_currentIndex >= MaxATSCChannel)
      {
        return true;
      }
      return false;
    }

    private void DetectAvailableStreams()
    {
      Log.Info("atsc-scan:Found signal,scanning for channels. Quality:{0} level:{1}", _captureCard.SignalQuality,
               _captureCard.SignalStrength);
      string chanDesc = String.Format("Channel:{0}", _currentIndex);
      string description = String.Format("Found signal for channel:{0} {1}, Scanning channels", _currentIndex, chanDesc);
      _callback.OnStatus(description);

      _captureCard.Process();
      _callback.OnSignal(_captureCard.SignalQuality, _captureCard.SignalStrength);
      _callback.OnStatus2(String.Format("new tv:{0} new radio:{1}", _newChannels, _newRadioChannels));
      _captureCard.StoreTunedChannels(false, true, ref _newChannels, ref _updatedChannels, ref _newRadioChannels,
                                      ref _updatedRadioChannels);
      _callback.OnStatus2(String.Format("new tv:{0} new radio:{1}", _newChannels, _newRadioChannels));

      _callback.UpdateList();
      return;
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
      if (_currentIndex < 0 || _currentIndex >= MaxATSCChannel)
      {
        return;
      }

      string chanDesc = String.Format("Channel:{0}", _currentIndex);
      string description = String.Format("Channel:{0}/{1} {2}", _currentIndex, MaxATSCChannel, chanDesc);
      _callback.OnStatus(description);

      Log.Info("tune channel:{0}/{1} {2}", _currentIndex, MaxATSCChannel, chanDesc);

      DVBChannel newchan = new DVBChannel();
      newchan.NetworkID = -1;
      newchan.TransportStreamID = -1;
      newchan.ProgramNumber = -1;
      newchan.MinorChannel = -1;
      newchan.MajorChannel = -1;
      newchan.Frequency = -1;
      newchan.PhysicalChannel = _currentIndex;
      newchan.Frequency = -1;
      newchan.Symbolrate = -1;
      newchan.Modulation = (int) ModulationType.ModNotSet;
      newchan.FEC = (int) FECMethod.MethodNotSet;
      _captureCard.Tune(newchan, 0);

      //tune locking : 2 seconds
      _captureCard.Process();
      _callback.OnSignal(_captureCard.SignalQuality, _captureCard.SignalStrength);
      Log.Info("atsc-scan:signal quality:{0} signal strength:{1} signal present:{2}",
               _captureCard.SignalQuality, _captureCard.SignalStrength, _captureCard.SignalPresent());
    }

    #endregion
  }
}