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
    #region variables
    List<ITVCard> _cards = new List<ITVCard>();
    List<int> _idCards = new List<int>();
    int _currentCardIndex = 0;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="HybridCard"/> class.
    /// </summary>
    public HybridCard()
    {
    }
    #endregion


    #region methods
    /// <summary>
    /// Adds the specified id card.
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="card">The card.</param>
    public void Add(int idCard, ITVCard card)
    {
      _idCards.Add(idCard);
      _cards.Add(card);
    }

    /// <summary>
    /// Determines whether [contains] [the specified id card].
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <returns>
    /// 	<c>true</c> if [contains] [the specified id card]; otherwise, <c>false</c>.
    /// </returns>
    public bool Contains(int idCard)
    {
      for (int i = 0; i < _idCards.Count; ++i)
      {
        if (_idCards[i] == idCard) return true;
      }
      return false;
    }

    /// <summary>
    /// Gets the by id.
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <returns></returns>
    public ITVCard GetById(int idCard)
    {
      for (int i = 0; i < _idCards.Count; ++i)
      {
        if (_idCards[i] == idCard) return _cards[i];
      }
      return null;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
      get
      {
        return _cards.Count;
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="TvLibrary.Interfaces.ITVCard"/> at the specified index.
    /// </summary>
    /// <value></value>
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
    /// <summary>
    /// Gets the number of channels the card is currently decrypting.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {
        return _cards[_currentCardIndex].NumberOfChannelsDecrypting;
      }
    }

    /// <summary>
    /// Gets or sets the timeout parameters.
    /// </summary>
    /// <value>The parameters.</value>
    public ScanParameters Parameters
    {
      get
      {
        return _cards[_currentCardIndex].Parameters;
      }
      set
      {
        _cards[_currentCardIndex].Parameters = value;
      }
    }
    #endregion
    #region ITVCard Members

    /// <summary>
    /// Gets a value indicating whether card supports subchannels
    /// </summary>
    /// <value><c>true</c> if card supports sub channels; otherwise, <c>false</c>.</value>
    public bool SupportsSubChannels
    {
      get
      {
        return _cards[_currentCardIndex].SupportsSubChannels;
      }
    }

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
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
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
          return _cards[_currentCardIndex].Tune(subChannelId,channel);
        }
      }
      return null;
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
        for (int i = 0; i < _cards.Count; ++i)
        {
          _cards[i].Context = value;
        }
      }
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
      _cards[_currentCardIndex].Dispose();
    }
    /// <summary>
    /// Gets the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    public ITvSubChannel GetSubChannel(int id)
    {
      return _cards[_currentCardIndex].GetSubChannel(id);
    }
    /// <summary>
    /// Gets the sub channels.
    /// </summary>
    /// <value>The sub channels.</value>
    public ITvSubChannel[] SubChannels
    {
      get
      {
        return _cards[_currentCardIndex].SubChannels;
      }
    }
    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    public void FreeSubChannel(int id)
    {
      _cards[_currentCardIndex].FreeSubChannel(id);
    }

    #endregion
  }
}
