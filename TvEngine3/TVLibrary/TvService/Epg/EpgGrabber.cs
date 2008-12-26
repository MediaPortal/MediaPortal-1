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
using System.Collections;
using System.Collections.Generic;
using TvControl;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using System.Threading;

namespace TvService
{
  public class EpgCardPriorityComparer : IComparer<EpgCard>
  {
    // Highest priority first
    public int Compare(EpgCard x, EpgCard y)
    {
      if (x.Card.Priority < y.Card.Priority)
        return 1;
      if (x.Card.Priority == y.Card.Priority)
        return 0;
      return -1;
    }
  }

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
    int _epgReGrabAfter = 4 * 60;//hours
    readonly System.Timers.Timer _epgTimer = new System.Timers.Timer();

    bool _isRunning;
    bool _reEntrant;
    readonly TVController _tvController;
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
      _epgTimer.Interval = 30000;
      _epgTimer.Elapsed += _epgTimer_Elapsed;
    }
    #endregion

    #region properties
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
    #endregion

    #region public members
    /// <summary>
    /// Starts the epg grabber
    /// </summary>
    public void Start()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      if (layer.GetSetting("idleEPGGrabberEnabled", "yes").Value != "yes")
      {
        Log.Epg("EPG: grabber disabled");
        return;
      }
      if (_isRunning)
        return;

      Setting s = layer.GetSetting("timeoutEPGRefresh", "240");
      if (Int32.TryParse(s.Value, out _epgReGrabAfter) == false)
      {
        _epgReGrabAfter = 240;
      }
      TransponderList.Instance.RefreshTransponders();
      if (TransponderList.Instance.Count == 0)
        return;
      Log.Epg("EPG: grabber initialized for {0} transponders..", TransponderList.Instance.Count);
      _isRunning = true;
      IList cards = Card.ListAll();
      _epgCards = new List<EpgCard>();


      foreach (Card card in cards)
      {
        if (!card.Enabled || !card.GrabEPG)
          continue;
        try
        {
          RemoteControl.HostName = card.ReferencedServer().HostName;
          if (!_tvController.CardPresent(card.IdCard))
            continue;
        } catch (Exception e)
        {
          Log.Error("card: unable to start job for card {0} at:{0}", e.Message, card.Name, card.ReferencedServer().HostName);
        }

        EpgCard epgCard = new EpgCard(_tvController, card);
        _epgCards.Add(epgCard);
      }
      _epgCards.Sort(new EpgCardPriorityComparer());
      _epgTimer.Interval = 1000;
      _epgTimer.Enabled = true;
    }

    /// <summary>
    /// Stops the epg grabber
    /// </summary>
    public void Stop()
    {
      if (_isRunning == false)
        return;
      Log.Epg("EPG: grabber stopped..");
      _epgTimer.Enabled = false;
      _isRunning = false;
      foreach (EpgCard epgCard in _epgCards)
      {
        epgCard.Stop();
        epgCard.Dispose();
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
      if (_reEntrant)
        return;
      if (_epgTimer.Interval == 1000)
        _epgTimer.Interval = 30000;
      try
      {
        _reEntrant = true;

        try
        {
          string threadname = Thread.CurrentThread.Name;
          if (string.IsNullOrEmpty(threadname))
            Thread.CurrentThread.Name = "DVB EPG timer";
        } catch (InvalidOperationException) { }

        if (_tvController.AllCardsIdle == false)
          return;
        foreach (EpgCard card in _epgCards)
        {
          //Log.Epg("card:{0} grabbing:{1}", card.Card.IdCard, card.IsGrabbing);
          if (card.IsGrabbing)
            continue;
          if (_tvController.AllCardsIdle == false)
            return;
          GrabEpgOnCard(card);
        }
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        _reEntrant = false;
      }
    }

    /// <summary>
    /// Grabs the epg for a card.
    /// </summary>
    /// <param name="epgCard">The epg card.</param>
    void GrabEpgOnCard(EpgCard epgCard)
    {
      CardType type = _tvController.Type(epgCard.Card.IdCard);
      //skip analog and webstream cards 
      if (type == CardType.Analog || type == CardType.RadioWebStream)
        return;

      while (TransponderList.Instance.GetNextTransponder() != null)
      {
        //skip transponders which are in use
        if (TransponderList.Instance.CurrentTransponder.InUse)
          continue;

        //check if card type is the same as the channel type of the transponder
        if (type == CardType.Atsc && TransponderList.Instance.CurrentTransponder.TuningDetail.ChannelType != 1)
          continue;
        if (type == CardType.DvbC && TransponderList.Instance.CurrentTransponder.TuningDetail.ChannelType != 2)
          continue;
        if (type == CardType.DvbS && TransponderList.Instance.CurrentTransponder.TuningDetail.ChannelType != 3)
          continue;
        if (type == CardType.DvbT && TransponderList.Instance.CurrentTransponder.TuningDetail.ChannelType != 4)
          continue;

        //find next channel to grab
        while (TransponderList.Instance.CurrentTransponder.GetNextChannel() != null)
        {
          //check if its time to grab the epg for this channel
          TimeSpan ts = DateTime.Now - TransponderList.Instance.CurrentTransponder.CurrentChannel.LastGrabTime;
          if (ts.TotalMinutes < _epgReGrabAfter)
            continue; // less then 2 hrs ago

          //get the channel
          Channel ch = TransponderList.Instance.CurrentTransponder.CurrentChannel;
          if (epgCard.Card.canTuneTvChannel(ch.IdChannel))
          {
            Log.Epg("epg:Grab for card:#{0} transponder #{1}/{2} channel: {3}",
                      epgCard.Card.IdCard, TransponderList.Instance.CurrentIndex + 1, TransponderList.Instance.Count, ch.DisplayName);
            //start grabbing
            epgCard.GrabEpg();
            return;
          }
        }
      }
    }


    #endregion
  }
}
