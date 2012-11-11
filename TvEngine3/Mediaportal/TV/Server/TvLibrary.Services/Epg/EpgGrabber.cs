#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using MediaPortal.Common.Utils.ExtensionMethods;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;

namespace Mediaportal.TV.Server.TVLibrary.Epg
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
  public class EpgGrabber : IDisposable
  {


    #region variables    
    
    private int _epgReGrabAfter = 4 * 60; //hours
    private readonly System.Timers.Timer _epgTimer = new System.Timers.Timer();

    private bool _disposed;
    private bool _isRunning;
    private bool _reEntrant;    
    private List<EpgCard> _epgCards;

    #endregion

    #region ctor

    /// <summary>
    /// Constructor
    /// </summary>
    public EpgGrabber()
    {      
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
      get { return _isRunning; }
    }

    #endregion

    #region public members

    /// <summary>
    /// Starts the epg grabber
    /// </summary>
    public void Start()
    {
      if (SettingsManagement.GetSetting("idleEPGGrabberEnabled", "yes").Value != "yes")
      {
        this.LogInfo("EPG: grabber disabled");
        return;
      }
      if (_isRunning)
      {
        return;
      }            

      Setting s = SettingsManagement.GetSetting("timeoutEPGRefresh", "240");      
      if (Int32.TryParse(s.Value, out _epgReGrabAfter) == false)
      {
        _epgReGrabAfter = 240;
      }
      TransponderList.Instance.RefreshTransponders();
      if (TransponderList.Instance.Count == 0)
      {
        return;
      }
      this.LogInfo("EPG: grabber initialized for {0} transponders..", TransponderList.Instance.Count);
      _isRunning = true;
      IList<Card> cards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.ChannelMaps);

      if (_epgCards != null)
      {
        foreach (EpgCard epgCard in _epgCards)
        {
          epgCard.Dispose();
        }
      }

      _epgCards = new List<EpgCard>();

      foreach (Card card in cards)
      {
        if (!card.Enabled || !card.GrabEPG)
        {
          continue;
        }
        try
        {
          RemoteControl.HostName = SettingsManagement.GetSetting("hostname").Value;
          if (!ServiceManager.Instance.InternalControllerService.IsCardPresent(card.IdCard))
          {
            continue;
          }
        }
        catch (Exception e)
        {
          this.LogError("card: unable to start job for card {0} at:{0}", e.Message, card.Name, RemoteControl.HostName);
        }

        var epgCard = new EpgCard(card);
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
      {
        return;
      }
      this.LogInfo("EPG: grabber stopped..");
      _epgTimer.Enabled = false;
      _isRunning = false;
      foreach (EpgCard epgCard in _epgCards)
      {
        epgCard.Stop();
      }
    }

    #endregion

    #region IDisposable Members    

    protected virtual void Dispose(bool disposing)
		{
		  if (disposing)
		  {
		    // get rid of managed resources
        if (!_disposed)
        {
          _epgTimer.SafeDispose();
          _epgCards.SafeDispose();
          _disposed = true;
        }
		  }
		  // get rid of unmanaged resources
		}
		
		
		/// <summary>
		/// Disposes the EPG card grabber
		/// </summary>    
		public void Dispose()
		{
		  Dispose(true);
		  GC.SuppressFinalize(this);
		}
		
		~EpgGrabber()
		{
		  Dispose(false);
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
    private void _epgTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
        }
        catch (InvalidOperationException) {}

        if (ServiceManager.Instance.InternalControllerService.AllCardsIdle == false)
          return;
        foreach (EpgCard card in _epgCards)
        {
          //this.LogInfo("card:{0} grabbing:{1}", card.Card.idCard, card.IsGrabbing);
          if (!_isRunning)
            return;
          if (card.IsGrabbing)
            continue;
          if (ServiceManager.Instance.InternalControllerService.AllCardsIdle == false)
            return;
          GrabEpgOnCard(card);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
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
    private void GrabEpgOnCard(EpgCard epgCard)
    {
      CardType type = ServiceManager.Instance.InternalControllerService.Type(epgCard.Card.IdCard);
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
        if (type == CardType.DvbIP && TransponderList.Instance.CurrentTransponder.TuningDetail.ChannelType != 7)
          continue;

        //find next channel to grab
        while (TransponderList.Instance.CurrentTransponder.GetNextChannel() != null)
        {
          //get the channel
          Channel ch = TransponderList.Instance.CurrentTransponder.CurrentChannel;

          //check if its time to grab the epg for this channel
          TimeSpan ts = DateTime.Now - TransponderList.Instance.CurrentTransponder.CurrentChannel.LastGrabTime.GetValueOrDefault(DateTime.MinValue);
          if (ts.TotalMinutes < _epgReGrabAfter)
          {
            //this.LogInfo("Skip card:#{0} transponder #{1}/{2} channel: {3} - Less than regrab time",
            //         epgCard.Card.idCard, TransponderList.Instance.CurrentIndex + 1, TransponderList.Instance.Count, ch.displayName);
            continue; // less then 2 hrs ago
          }

          if (TVDatabase.TVBusinessLayer.CardManagement.CanTuneTvChannel(epgCard.Card, ch.IdChannel))
          {
            this.LogInfo("Grab for card:#{0} transponder #{1}/{2} channel: {3}",
                    epgCard.Card.IdCard, TransponderList.Instance.CurrentIndex + 1, TransponderList.Instance.Count,
                    ch.DisplayName);
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