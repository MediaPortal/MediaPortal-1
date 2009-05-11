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
using TvLibrary.ChannelLinkage;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.Hybrid
{
  public class HybridCardGroup
  {
    #region variables
    List<ITVCard> _cards = new List<ITVCard>();
    List<int> _idCards = new List<int>();
    int _currentCardIndex = 0;
    private string _groupName;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="HybridCard"/> class.
    /// </summary>
    /// <param name="groupName">The name of the group</param>
    public HybridCardGroup(string groupName)
    {
      _groupName = groupName;
    }
    #endregion

    #region methods
    /// <summary>
    /// Adds the specified id card.
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="card">The card.</param>
    public HybridCard Add(int idCard, ITVCard card)
    {
      _idCards.Add(idCard);
      TvCardAnalog analogCard = card as TvCardAnalog;
      if (analogCard != null)
      {
        analogCard.CardId = idCard;
      }
      _cards.Add(card);
      return new HybridCard(this, card);
    }

    /// <summary>
    /// Checks if the active card is the one with given id
    /// </summary>
    /// <param name="idCard">ID of the card to check</param>
    /// <returns> 
    ///   <c>true</c> if card with the given id is active; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCardIdActive(int idCard)
    {
      return _idCards[_currentCardIndex] == idCard;
    }

    #endregion

    #region properties

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
    /// Starts scanning for linkage info
    /// </summary>
    public void StartLinkageScanner(BaseChannelLinkageScanner callback)
    {
      _cards[_currentCardIndex].StartLinkageScanner(callback);
    }

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public void ResetLinkageScanner()
    {
      _cards[_currentCardIndex].ResetLinkageScanner();
    }

    /// <summary>
    /// Returns the channel linkages grabbed
    /// </summary>
    public List<PortalChannel> ChannelLinkages
    {
      get
      {
        return _cards[_currentCardIndex].ChannelLinkages;
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
    /// Start grabbing the epg while timeshifting
    /// </summary>
    public void GrabEpg()
    {
      _cards[_currentCardIndex].GrabEpg();
    }

    /// <summary>
    /// Aborts grabbing the epg
    /// </summary>
    public void AbortGrabbing()
    {
      _cards[_currentCardIndex].AbortGrabbing();
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
