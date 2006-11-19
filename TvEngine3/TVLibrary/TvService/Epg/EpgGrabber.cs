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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TvControl;

using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Epg;
using System.Threading;

namespace TvService
{
  /// <summary>
  /// Class which will continously grab the epg for all channels
  /// Epg is grabbed when:
  ///  - channel is a DVB or ATSC channel
  ///  - if at least 2 hours have past since the previous time the epg for the channel was grabbed
  ///  - if no cards are timeshifting or recording
  /// </summary>
  public class EpgGrabber
  {

    #region variables
    const int EpgReGrabAfter = 4;//hours
    System.Timers.Timer _epgTimer = new System.Timers.Timer();

    bool _isRunning;
    bool _reEntrant = false;
    TVController _tvController;
    List<Transponder> _transponders;
    int _currentTransponderIndex = -1;
    List<EpgCard> _epgCards;
    #endregion

    #region ctor
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="controller">instance of a TVController</param>
    public EpgGrabber(TVController controller)
    {
      _tvController = controller;
      _epgTimer.Interval = 1000;
      _epgTimer.Elapsed += new System.Timers.ElapsedEventHandler(_epgTimer_Elapsed);
    }
    #endregion


    #region public members
    /// <summary>
    /// Property which returns true if EPG grabber is currently grabbing the epg
    /// or false is epg grabber is idle
    /// </summary>
    public bool IsRunning
    {
      get
      {
        return _isRunning;
      }
    }

    /// <summary>
    /// Starts the epg grabber
    /// </summary>
    public void Start()
    {
      if (_isRunning) return;
      GetTransponders();
      Log.Epg("EPG: grabber started {0} transponders..", _transponders.Count);
      _isRunning = true;
      _epgTimer.Enabled = true;
      IList cards = Card.ListAll();
      _epgCards = new List<EpgCard>();
      foreach (Card card in cards)
      {
        EpgCard epgCard = new EpgCard(_tvController, card);
        _epgCards.Add(epgCard);
      }
    }

    /// <summary>
    /// Stops the epg grabber
    /// </summary>
    public void Stop()
    {
      if (_isRunning == false) return;
      Log.Epg("EPG: grabber stopped..");
      _epgTimer.Enabled = false;
      _isRunning = false;
      foreach (EpgCard epgCard in _epgCards)
      {
        epgCard.Stop();
      }
    }
    #endregion

    #region private members
    /// <summary>
    /// timer callback.
    /// This method is called by a timer every 30 seconds to wake up the epg grabber
    /// the epg grabber will check if its time to grab the epg for a channel
    /// and ifso it starts the grabbing process
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void _epgTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      //security check, dont allow re-entrancy here
      if (_reEntrant) return;

      try
      {
        _reEntrant = true;
        if (_tvController.AllCardsIdle == false) return;
        foreach (EpgCard card in _epgCards)
        {
          //Log.Epg("card:{0} grabbing:{1}", card.Card.IdCard, card.IsGrabbing);
          if (card.IsGrabbing) continue;
          GrabEpgOnCard(card);
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        _reEntrant = false;
      }
    }

    void GrabEpgOnCard(EpgCard epgCard)
    {
      CardType type = _tvController.Type(epgCard.Card.IdCard);
      if (type == CardType.Analog) return;
      int counter = -1;
      foreach (Transponder transponder in _transponders)
      {
        counter++;
        if (transponder.InUse) continue;
        if (type == CardType.Atsc && transponder.TuningDetail.ChannelType != 1) continue;
        if (type == CardType.DvbC && transponder.TuningDetail.ChannelType != 2) continue;
        if (type == CardType.DvbS && transponder.TuningDetail.ChannelType != 3) continue;
        if (type == CardType.DvbT && transponder.TuningDetail.ChannelType != 4) continue;

        int allChecked = 0;
        while (true)
        {
          allChecked++;
          transponder.Index++;
          if (transponder.Index >= transponder.Channels.Count)
            transponder.Index = 0;
          if (transponder.Index >= transponder.Channels.Count) continue;

          TimeSpan ts = DateTime.Now - transponder.Channels[transponder.Index].LastGrabTime;
          if (ts.TotalHours < EpgReGrabAfter)
          {
            if (allChecked >= transponder.Channels.Count) break;
            continue; // less then 2 hrs ago
          }
          Channel ch = transponder.Channels[transponder.Index];
          Log.Epg("epg:Grab for transponder #{0} {1}", counter, transponder.ToString());
          epgCard.GrabEpg(_transponders, counter, transponder.Channels[transponder.Index]);

          return;
        }
      }
    }


    void GetTransponders()
    {
      _transponders = new List<Transponder>();
      IList channels = Channel.ListAll();
      foreach (Channel channel in channels)
      {
        if (channel.GrabEpg == false) continue;
        if (channel.IsRadio == false && channel.IsTv == false) continue;
        foreach (TuningDetail detail in channel.ReferringTuningDetail())
        {
          if (detail.ChannelType == 0) continue;//analog

          Transponder t = new Transponder(detail);
          bool found = false;
          foreach (Transponder transponder in _transponders)
          {
            if (transponder.Equals(t))
            {
              found = true;
              transponder.Channels.Add(channel);
              break;
            }
          }

          if (!found)
          {
            t.Channels.Add(channel);
            _transponders.Add(t);
          }
        }
      }
    }
    Channel GetNextChannel(out Transponder transponder)
    {
      transponder = null;
      if (_transponders.Count == 0) return null;
      while (true)
      {
        _currentTransponderIndex++;
        if (_currentTransponderIndex >= _transponders.Count)
          _currentTransponderIndex = 0;
        if (_currentTransponderIndex >= _transponders.Count) return null;

        transponder = _transponders[_currentTransponderIndex];
        int allChecked = 0;
        while (true)
        {
          allChecked++;
          transponder.Index++;
          if (transponder.Index >= transponder.Channels.Count)
            transponder.Index = 0;
          if (transponder.Index >= transponder.Channels.Count) return null;

          TimeSpan ts = DateTime.Now - transponder.Channels[transponder.Index].LastGrabTime;
          if (ts.TotalHours < EpgReGrabAfter)
          {
            if (allChecked >= transponder.Channels.Count) break;
            continue; // less then 2 hrs ago
          }
          return transponder.Channels[transponder.Index];
        }
      }
    }
    #endregion
  }
}
