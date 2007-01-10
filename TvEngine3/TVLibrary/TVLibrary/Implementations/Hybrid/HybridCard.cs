/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Implementations;
using DirectShowLib.SBE;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.Hybrid
{
  public class HybridCard : ITVCard
  {
    List<ITVCard> _cards = new List<ITVCard>();
    int _currentCardIndex = 0;

    public HybridCard()
    {
    }
    
    protected bool _isHybrid = false;
    public void Add(ITVCard card)
    {
      _cards.Add(card);
    }

    public int Count
    {
      get
      {
        return _cards.Count;
      }
    }

    public ITVCard this[int index]
    {
      get
      {
        return _cards[index];
      }
      set
      {
        _cards[index] = value;
      }
    }

    #region ITVCard Members

    /// <summary>
    /// Returns if the tuner belongs to a hybrid card
    /// </summary>
    public bool IsHybrid 
    {
      get
      {
        return false;
      }
      set
      {
      }
    }

    public string Name
    {
      get
      {
        return _cards[_currentCardIndex].Name;
      }
      set
      {
        _cards[_currentCardIndex].Name = value;
      }
    }

    public string DevicePath
    {
      get
      {
        return _cards[_currentCardIndex].DevicePath;
      }
    }

    public string TimeShiftFileName
    {
      get
      {
        return _cards[_currentCardIndex].TimeShiftFileName;
      }
    }

    public DateTime StartOfTimeShift
    {
      get
      {
        return _cards[_currentCardIndex].StartOfTimeShift;
      }
    }

    public DateTime RecordingStarted
    {
      get
      {
        return _cards[_currentCardIndex].RecordingStarted;
      }
    }

    public bool CanTune(IChannel channel)
    {
      foreach (ITVCard card in _cards)
      {
        if (card.CanTune(channel)) return true;
      }
      return false;
    }

    public void StopGraph()
    {
      _cards[_currentCardIndex].StopGraph();
    }

    public bool IsReceivingAudioVideo
    {
      get
      {
        return _cards[_currentCardIndex].IsReceivingAudioVideo;
      }
    }

    public string FileName
    {
      get
      {
        return _cards[_currentCardIndex].FileName;
      }
    }

    public bool IsRecording
    {
      get
      {
        return _cards[_currentCardIndex].IsRecording;
      }
    }

    public bool IsTimeShifting
    {
      get
      {
        return _cards[_currentCardIndex].IsTimeShifting;
      }
    }

    public bool IsEpgGrabbing
    {
      get
      {
        return _cards[_currentCardIndex].IsEpgGrabbing;
      }
      set
      {
        _cards[_currentCardIndex].IsEpgGrabbing = value;
      }
    }

    public bool IsScanning
    {
      get
      {
        return _cards[_currentCardIndex].IsScanning;
      }
      set
      {
        _cards[_currentCardIndex].IsScanning = value;
      }
    }

    public IChannel Channel
    {
      get
      {
        return _cards[_currentCardIndex].Channel;
      }
    }

    public int MinChannel
    {
      get
      {
        return _cards[_currentCardIndex].MinChannel;
      }
    }

    public int MaxChannel
    {
      get
      {
        return _cards[_currentCardIndex].MaxChannel;
      }
    }

    public bool IsTimeshiftingTransportStream
    {
      get
      {
        return _cards[_currentCardIndex].IsTimeshiftingTransportStream;
      }
    }

    public bool IsRecordingTransportStream
    {
      get
      {
        return _cards[_currentCardIndex].IsRecordingTransportStream;
      }
    }

    public CamType CamType
    {
      get
      {
        return _cards[_currentCardIndex].CamType;
      }
      set
      {
        _cards[_currentCardIndex].CamType = value;
      }
    }

    public int cardType
    {
      get
      {
        return _cards[_currentCardIndex].cardType;
      }
    }

    public IDiSEqCMotor DiSEqCMotor
    {
      get
      {
        return _cards[_currentCardIndex].DiSEqCMotor;
      }
    }

    public void GrabEpg(BaseEpgGrabber callback)
    {
      _cards[_currentCardIndex].GrabEpg(callback);
    }

    public List<EpgChannel> Epg
    {
      get
      {
        return _cards[_currentCardIndex].Epg;
      }
    }

    public ITVScanning ScanningInterface
    {
      get
      {
        return _cards[_currentCardIndex].ScanningInterface;
      }
    }

    public bool GrabTeletext
    {
      get
      {
        return _cards[_currentCardIndex].GrabTeletext;
      }
      set
      {
        _cards[_currentCardIndex].GrabTeletext = value;
      }
    }

    public TvLibrary.Teletext.ITeletext TeletextDecoder
    {
      get
      {
        return _cards[_currentCardIndex].TeletextDecoder;
      }
    }

    public bool HasTeletext
    {
      get
      {
        return _cards[_currentCardIndex].HasTeletext;
      }
    }

    public TvLibrary.Teletext.IVbiCallback TeletextCallback
    {
      get
      {
        return _cards[_currentCardIndex].TeletextCallback;
      }
      set
      {
        _cards[_currentCardIndex].TeletextCallback = value;
      }
    }

    public bool TuneScan(IChannel channel)
    {
      return _cards[_currentCardIndex].TuneScan(channel);
    }

    public bool Tune(IChannel channel)
    {
      return _cards[_currentCardIndex].Tune(channel);
    }

    public bool StartTimeShifting(string fileName)
    {
      return _cards[_currentCardIndex].StartTimeShifting(fileName);
    }

    public bool StopTimeShifting()
    {
      return _cards[_currentCardIndex].StopTimeShifting();
    }

    public bool StartRecording(bool transportStream, string fileName)
    {
      return _cards[_currentCardIndex].StartRecording(transportStream, fileName);
    }

    public bool StopRecording()
    {
      return _cards[_currentCardIndex].StopRecording();
    }

    public List<IAudioStream> AvailableAudioStreams
    {
      get
      {
        return _cards[_currentCardIndex].AvailableAudioStreams;
      }
    }

    public IAudioStream CurrentAudioStream
    {
      get
      {
        return _cards[_currentCardIndex].CurrentAudioStream;
      }
      set
      {
        _cards[_currentCardIndex].CurrentAudioStream = value;
      }
    }

    public IQuality Quality
    {
      get
      {
        return _cards[_currentCardIndex].Quality;
      }
      set
      {
        _cards[_currentCardIndex].Quality = value;
      }
    }

    public bool SupportsQualityControl
    {
      get
      {
        return _cards[_currentCardIndex].SupportsQualityControl;
      }
    }

    public bool IsTunerLocked
    {
      get
      {
        return _cards[_currentCardIndex].IsTunerLocked;
      }
    }

    public int SignalQuality
    {
      get
      {
        return _cards[_currentCardIndex].SignalQuality;
      }
    }

    public int SignalLevel
    {
      get
      {
        return _cards[_currentCardIndex].SignalLevel;
      }
    }

    public void ResetSignalUpdate()
    {
      _cards[_currentCardIndex].ResetSignalUpdate();
    }

    public object Context
    {
      get
      {
        return _cards[_currentCardIndex].Context;
      }
      set
      {
        _cards[_currentCardIndex].Context = value;
      }
    }

    public void Dispose()
    {
      _cards[_currentCardIndex].Dispose();
    }

    #endregion
  }
}
