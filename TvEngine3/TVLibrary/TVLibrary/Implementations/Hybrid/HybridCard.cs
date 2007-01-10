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
    List<int> _idCards = new List<int>();
    int _currentCardIndex = 0;
    protected bool _isHybrid = false;

    public HybridCard()
    {
    }
    

    public void Add(int idCard,ITVCard card)
    {
      _idCards.Add(idCard);
      _cards.Add(card);
    }

    public bool Contains(int idCard)
    {
      for (int i = 0; i < _idCards.Count; ++i)
      {
        if (_idCards[i] == idCard) return true;
      }
      return false;
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

    /// <summary>
    /// Gets/sets the card name
    /// </summary>
    /// <value></value>
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

    /// <summary>
    /// Gets/sets the card device
    /// </summary>
    /// <value></value>
    public string DevicePath
    {
      get
      {
        return _cards[_currentCardIndex].DevicePath;
      }
    }

    /// <summary>
    /// gets the current filename used for timeshifting
    /// </summary>
    /// <value></value>
    public string TimeShiftFileName
    {
      get
      {
        return _cards[_currentCardIndex].TimeShiftFileName;
      }
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <value></value>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime StartOfTimeShift
    {
      get
      {
        return _cards[_currentCardIndex].StartOfTimeShift;
      }
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <value></value>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted
    {
      get
      {
        return _cards[_currentCardIndex].RecordingStarted;
      }
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="channel"></param>
    /// <returns>
    /// true if card can tune to the channel otherwise false
    /// </returns>
    public bool CanTune(IChannel channel)
    {
      foreach (ITVCard card in _cards)
      {
        if (card.CanTune(channel)) return true;
      }
      return false;
    }

    /// <summary>
    /// Stops the current graph
    /// </summary>
    public void StopGraph()
    {
      _cards[_currentCardIndex].StopGraph();
    }

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <value></value>
    /// <returns>true of false</returns>
    public bool IsReceivingAudioVideo
    {
      get
      {
        return _cards[_currentCardIndex].IsReceivingAudioVideo;
      }
    }

    /// <summary>
    /// gets the current filename used for recording
    /// </summary>
    /// <value></value>
    public string FileName
    {
      get
      {
        return _cards[_currentCardIndex].FileName;
      }
    }

    /// <summary>
    /// returns true if card is currently recording
    /// </summary>
    /// <value></value>
    public bool IsRecording
    {
      get
      {
        return _cards[_currentCardIndex].IsRecording;
      }
    }

    /// <summary>
    /// returns true if card is currently timeshifting
    /// </summary>
    /// <value></value>
    public bool IsTimeShifting
    {
      get
      {
        return _cards[_currentCardIndex].IsTimeShifting;
      }
    }

    /// <summary>
    /// returns true if card is currently grabbing the epg
    /// </summary>
    /// <value></value>
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

    /// <summary>
    /// returns true if card is currently scanning
    /// </summary>
    /// <value></value>
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

    /// <summary>
    /// returns the IChannel to which the card is currently tuned
    /// </summary>
    /// <value></value>
    public IChannel Channel
    {
      get
      {
        return _cards[_currentCardIndex].Channel;
      }
    }

    /// <summary>
    /// returns the min. channel number for analog cards
    /// </summary>
    /// <value></value>
    public int MinChannel
    {
      get
      {
        return _cards[_currentCardIndex].MinChannel;
      }
    }

    /// <summary>
    /// returns the max. channel number for analog cards
    /// </summary>
    /// <value>The max channel.</value>
    public int MaxChannel
    {
      get
      {
        return _cards[_currentCardIndex].MaxChannel;
      }
    }

    /// <summary>
    /// returns true if we timeshift in transport stream mode
    /// false we timeshift in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsTimeshiftingTransportStream
    {
      get
      {
        return _cards[_currentCardIndex].IsTimeshiftingTransportStream;
      }
    }

    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsRecordingTransportStream
    {
      get
      {
        return _cards[_currentCardIndex].IsRecordingTransportStream;
      }
    }

    /// <summary>
    /// Gets or sets the type of the cam.
    /// </summary>
    /// <value>The type of the cam.</value>
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

    /// <summary>
    /// Gets/sets the card type
    /// </summary>
    /// <value></value>
    public int cardType
    {
      get
      {
        return _cards[_currentCardIndex].cardType;
      }
    }

    /// <summary>
    /// Gets the interface for controlling the diseqc motor
    /// </summary>
    /// <value>Theinterface for controlling the diseqc motor.</value>
    public IDiSEqCMotor DiSEqCMotor
    {
      get
      {
        return _cards[_currentCardIndex].DiSEqCMotor;
      }
    }

    /// <summary>
    /// Grabs the epg.
    /// </summary>
    /// <param name="callback">The callback which gets called when epg is received or canceled.</param>
    public void GrabEpg(BaseEpgGrabber callback)
    {
      _cards[_currentCardIndex].GrabEpg(callback);
    }

    /// <summary>
    /// returns a list of all epg data for each channel found.
    /// </summary>
    /// <value>The epg.</value>
    public List<EpgChannel> Epg
    {
      get
      {
        return _cards[_currentCardIndex].Epg;
      }
    }

    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    /// <value></value>
    public ITVScanning ScanningInterface
    {
      get
      {
        return _cards[_currentCardIndex].ScanningInterface;
      }
    }

    /// <summary>
    /// Turn on/off teletext grabbing
    /// </summary>
    /// <value></value>
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

    /// <summary>
    /// returns the ITeletext interface used for retrieving the teletext pages
    /// </summary>
    /// <value></value>
    public TvLibrary.Teletext.ITeletext TeletextDecoder
    {
      get
      {
        return _cards[_currentCardIndex].TeletextDecoder;
      }
    }

    /// <summary>
    /// Property which returns true when the current channel contains teletext
    /// </summary>
    /// <value></value>
    public bool HasTeletext
    {
      get
      {
        return _cards[_currentCardIndex].HasTeletext;
      }
    }

    /// <summary>
    /// Gets or sets the teletext callback.
    /// </summary>
    /// <value>The teletext callback.</value>
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

    /// <summary>
    /// tune the card to the channel specified by IChannel
    /// </summary>
    /// <param name="channel">channel to tune</param>
    /// <returns>true if succeeded else false</returns>
    public bool TuneScan(IChannel channel)
    {
      for (int i = 0; i < _cards.Count; ++i)
      {
        if (_cards[i].CanTune(channel))
        {
          _currentCardIndex = i;
          for (int x = 0; x < _cards.Count; x++)
          {
            if (x != i)
            {
              _cards[x].Dispose();
            }
          }
          return _cards[_currentCardIndex].TuneScan(channel);
        }
      }
      return false;
    }

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public bool Tune(IChannel channel)
    {
      for (int i = 0; i < _cards.Count; ++i)
      {
        if (_cards[i].CanTune(channel))
        {
          _currentCardIndex = i;
          for (int x = 0; x < _cards.Count; x++)
          {
            if (x != i)
            {
              _cards[x].Dispose();
            }
          }
          return _cards[_currentCardIndex].Tune(channel);
        }
      }
      return false;
    }

    /// <summary>
    /// Starts timeshifting. Note card has to be tuned first
    /// </summary>
    /// <param name="fileName">filename used for the timeshiftbuffer</param>
    /// <returns>true if succeeded else false</returns>
    public bool StartTimeShifting(string fileName)
    {
      return _cards[_currentCardIndex].StartTimeShifting(fileName);
    }

    /// <summary>
    /// Stops timeshifting
    /// </summary>
    /// <returns>true if succeeded else false</returns>
    public bool StopTimeShifting()
    {
      return _cards[_currentCardIndex].StopTimeShifting();
    }

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="transportStream">if true, then record transport stream</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <returns>true if succeeded else false</returns>
    public bool StartRecording(bool transportStream, string fileName)
    {
      return _cards[_currentCardIndex].StartRecording(transportStream, fileName);
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns>true if succeeded else false</returns>
    public bool StopRecording()
    {
      return _cards[_currentCardIndex].StopRecording();
    }

    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    /// <value></value>
    public List<IAudioStream> AvailableAudioStreams
    {
      get
      {
        return _cards[_currentCardIndex].AvailableAudioStreams;
      }
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    /// <value></value>
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

    /// <summary>
    /// Get/Set the quality
    /// </summary>
    /// <value></value>
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

    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    /// <value></value>
    public bool SupportsQualityControl
    {
      get
      {
        return _cards[_currentCardIndex].SupportsQualityControl;
      }
    }

    /// <summary>
    /// When the tuner is locked onto a signal this property will return true
    /// otherwise false
    /// </summary>
    /// <value></value>
    public bool IsTunerLocked
    {
      get
      {
        return _cards[_currentCardIndex].IsTunerLocked;
      }
    }

    /// <summary>
    /// returns the signal quality
    /// </summary>
    /// <value></value>
    public int SignalQuality
    {
      get
      {
        return _cards[_currentCardIndex].SignalQuality;
      }
    }

    /// <summary>
    /// returns the signal level
    /// </summary>
    /// <value></value>
    public int SignalLevel
    {
      get
      {
        return _cards[_currentCardIndex].SignalLevel;
      }
    }

    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    public void ResetSignalUpdate()
    {
      _cards[_currentCardIndex].ResetSignalUpdate();
    }

    /// <summary>
    /// Gets or sets the context.
    /// </summary>
    /// <value>The context.</value>
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

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      _cards[_currentCardIndex].Dispose();
    }


    #endregion
  }
}
