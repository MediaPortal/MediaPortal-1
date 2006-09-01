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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using DirectShowLib.SBE;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Log;
using TVLibrary.Streaming;
using TvControl;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
using TvDatabase;

namespace TvService
{
  /// <summary>
  /// This class servers all requests from remote clients
  /// and if server is the master it will delegate the requests to the 
  /// correct slave servers
  /// </summary>
  public class TVController : MarshalByRefObject, IController, IDisposable
  {
    #region variables
    EpgGrabber _epgGrabber;
    Scheduler _scheduler;
    RtspStreaming _streamer;
    EpgReceivedHandler _handler;
    bool _isMaster = false;
    Dictionary<int, bool> _cardLocks;
    Dictionary<int, Card> _allDbscards;
    Dictionary<int, ITVCard> _localCards;
    Dictionary<int, int> _clientReferenceCount;
    #endregion

    #region events
    public event EpgReceivedHandler OnEpgReceived;
    #endregion

    #region ctor
    public TVController()
    {
      Init();
    }

    bool IsLocal(string hostName)
    {
      if (hostName == Dns.GetHostName()) return true;
      if (hostName == "127.0.0.1") return true;
      IPHostEntry local = Dns.GetHostByName(Dns.GetHostName());
      foreach (IPAddress ipaddress in local.AddressList)
      {
        if (hostName == ipaddress.ToString()) return true;
      }
      return false;
    }

    /// <summary>
    /// Initalizes the controller.
    /// It will update the database with the cards found on this system
    /// start the epg grabber and scheduler
    /// and check if its supposed to be a master or slave controller
    /// </summary>
    void Init()
    {
      DatabaseManager.New();
      try
      {
        Log.Write("Controller: Started");
        EntityList<Server> servers = DatabaseManager.Instance.GetEntities<Server>();
        Server ourServer = null;
        foreach (Server server in servers)
        {
          if (IsLocal(server.HostName))
          {
            ourServer = server;
            break;
          }
        }
        if (ourServer == null)
        {
          Log.WriteFile("Controller: create new server in database");
          ourServer = Server.Create();
          if (servers.Count == 0)
          {
            ourServer.IsMaster = true;
            _isMaster = true;
          }
          ourServer.HostName = Dns.GetHostName();
          DatabaseManager.Instance.SaveChanges();
          Log.WriteFile("Controller: new server created");
        }
        _isMaster = ourServer.IsMaster;


        TvCardCollection localCardCollection = new TvCardCollection();
        TvBusinessLayer layer = new TvBusinessLayer();
        for (int i = 0; i < localCardCollection.Cards.Count; ++i)
        {
          bool found = false;
          foreach (Card card in ourServer.Cards)
          {
            if (card.DevicePath == localCardCollection.Cards[i].DevicePath)
            {
              found = true;
              break;
            }
          }
          if (!found)
          {
            Log.WriteFile("Controller: add card:{0}", localCardCollection.Cards[i].Name);
            layer.AddCard(localCardCollection.Cards[i].Name, localCardCollection.Cards[i].DevicePath, ourServer);
          }
        }

        //remove old cards...
        EntityList<Card> cardsInDbs = DatabaseManager.Instance.GetEntities<Card>();
        cardsInDbs.ShouldRemoveDeletedEntities = false;
        int cardsInstalled = localCardCollection.Cards.Count;
        foreach (Card dbsCard in cardsInDbs)
        {
          if (dbsCard.Server.IdServer == ourServer.IdServer)
          {
            bool found = false;
            for (int cardNumber = 0; cardNumber < cardsInstalled; ++cardNumber)
            {
              if (dbsCard.DevicePath == localCardCollection.Cards[cardNumber].DevicePath)
              {
                found = true;
                break;
              }
            }
            if (!found)
            {
              Log.WriteFile("Controller: del card:{0}", dbsCard.Name);
              dbsCard.DeleteAll();
            }
          }
        }
        DatabaseManager.Instance.SaveChanges();
        _localCards = new Dictionary<int, ITVCard>();
        _allDbscards = new Dictionary<int, Card>();
        _cardLocks = new Dictionary<int, bool>();
        _clientReferenceCount = new Dictionary<int, int>();
        EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>();
        foreach (Card card in cards)
        {
          _allDbscards[card.IdCard] = card;
          _cardLocks[card.IdCard] = false;
          _clientReferenceCount[card.IdCard] = 0;
          if (IsLocal(card.Server.HostName))
          {
            for (int x = 0; x < localCardCollection.Cards.Count; ++x)
            {
              if (localCardCollection.Cards[x].DevicePath == card.DevicePath)
              {
                _localCards[card.IdCard] = localCardCollection.Cards[x];
                break;
              }
            }
          }
        }

        _streamer = new RtspStreaming();

        _handler = new EpgReceivedHandler(epgGrabber_OnEpgReceived);
        if (_isMaster)
        {
          _epgGrabber = new EpgGrabber(this);
          _epgGrabber.Start();
          _scheduler = new Scheduler(this);
          _scheduler.Start();
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
    #endregion

    #region MarshalByRefObject overrides
    public override object InitializeLifetimeService()
    {
      return null;
    }
    #endregion

    #region epg event callback
    /// <summary>
    /// Callback fired by one of the tvcards when EPG data has been received
    /// The method instructs the epg grabber to handle the new epg and update the database
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="epg">new epg data</param>
    void epgGrabber_OnEpgReceived(object sender, List<EpgChannel> epg)
    {
      try
      {
        ITVEPG grabber = (ITVEPG)sender;
        grabber.OnEpgReceived -= _handler;

        if (OnEpgReceived != null)
        {
          OnEpgReceived(sender, epg);
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
    #endregion

    #region IDisposable Members

    /// <summary>
    /// Clean up the controller when service is stopped
    /// </summary>
    public void Dispose()
    {
      Log.Write("Controller: stopped");
      if (_streamer != null)
      {
        Log.WriteFile("Controller: stop streamer...");
        _streamer.Stop();
        _streamer = null;
        Log.WriteFile("Controller: streamer stopped...");
      }
      if (_scheduler != null)
      {
        _scheduler.Stop();
        _scheduler = null;
      }
      if (_epgGrabber != null)
      {
        _epgGrabber.Stop();
        _epgGrabber = null;
      }
      Log.WriteFile("Controller: dispose cards");
      Dictionary<int, ITVCard>.Enumerator enumerator = _localCards.GetEnumerator();
      while (enumerator.MoveNext())
      {
        KeyValuePair<int, ITVCard> key = enumerator.Current;
        Log.WriteFile("Controller:  dispose:{0}", key.Value.Name);
        try
        {
          key.Value.Dispose();
        }
        catch (Exception ex)
        {
          Log.Write(ex);
        }
      }
      _localCards = null;
      Log.WriteFile("Controller: cards disposed");
    }

    #endregion

    #region IController Members

    #region internal interface
    /// <summary>
    /// Gets the total number of cards installed.
    /// </summary>
    /// <value>Number which indicates the cards installed</value>
    public int Cards
    {
      get
      {
        return _allDbscards.Count;
      }
    }

    /// <summary>
    /// Gets the card Id for a card
    /// </summary>
    /// <param name="cardIndex">Index of the card.</param>
    /// <value>id of card</value>
    public int CardId(int cardIndex)
    {
      EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>();
      return cards[cardIndex].IdCard;
    }

    /// <summary>
    /// Gets the type of card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <value>cardtype</value>
    public CardType Type(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.Type(cardId);
        }
        if ((_localCards[cardId] as TvCardAnalog) != null) return CardType.Analog;
        if ((_localCards[cardId] as TvCardATSC) != null) return CardType.Atsc;
        if ((_localCards[cardId] as TvCardDVBC) != null) return CardType.DvbC;
        if ((_localCards[cardId] as TvCardDVBS) != null) return CardType.DvbS;
        if ((_localCards[cardId] as TvCardDvbSS2) != null) return CardType.DvbS;
        if ((_localCards[cardId] as TvCardDVBT) != null) return CardType.DvbT;
        return CardType.Analog;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return CardType.Analog;
      }
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <returns>name of card</returns>
    public string CardName(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.CardName(cardId);
        }
        return _localCards[cardId].Name;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <param name="channel">channel.</param>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(int cardId, IChannel channel)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.CanTune(cardId, channel);
        }
        return _localCards[cardId].CanTune(channel);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>device of card</returns>
    public string CardDevice(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.CardDevice(cardId);
        }
        return _localCards[cardId].DevicePath;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.CurrentChannel(cardId);
        }
        return _localCards[cardId].Channel;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return null;
      }
    }

    /// <summary>
    /// Gets the current channel name.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>channel</returns>
    public string CurrentChannelName(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.CurrentChannelName(cardId);
        }
        if (_localCards[cardId].Channel == null) return "";
        return _localCards[cardId].Channel.Name;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }


    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    public bool TunerLocked(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.TunerLocked(cardId);
        }
        return _localCards[cardId].IsTunerLocked;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    public int SignalQuality(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.SignalQuality(cardId);
        }
        return _localCards[cardId].SignalQuality;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return 0;
      }
    }

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal level (0-100)</returns>
    public int SignalLevel(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.SignalLevel(cardId);
        }
        return _localCards[cardId].SignalLevel;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return 0;
      }
    }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>filename or null when not recording</returns>
    public string FileName(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.FileName(cardId);
        }
        return _localCards[cardId].FileName;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    public string TimeShiftFileName(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.TimeShiftFileName(cardId);
        }
        return _localCards[cardId].TimeShiftFileName + ".tsbuffer";
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.IsTimeShifting(cardId);
        }
        return _localCards[cardId].IsTimeShifting || _localCards[cardId].IsRecording;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is recording otherwise false</returns>
    public bool IsRecording(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.IsRecording(cardId);
        }
        return _localCards[cardId].IsRecording;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    public bool IsScanning(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.IsScanning(cardId);
        }
        return _localCards[cardId].IsScanning;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns if the card is grabbing the epg or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    public bool IsGrabbingEpg(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.IsGrabbingEpg(cardId);
        }
        return _localCards[cardId].IsEpgGrabbing;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns if the card is grabbing teletext or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing teletext otherwise false</returns>
    public bool IsGrabbingTeletext(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.IsGrabbingTeletext(cardId);
        }
        return _localCards[cardId].GrabTeletext;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    public bool HasTeletext(int cardId)
    {
      if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
      {
        RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
        return RemoteControl.Instance.HasTeletext(cardId);
      }
      return _localCards[cardId].HasTeletext;
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(int cardId, int pageNumber)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.TeletextRotation(cardId, pageNumber);
        }
        return _localCards[cardId].TeletextDecoder.RotationTime(pageNumber);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return new TimeSpan(0, 0, 0, 15);
      }
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime TimeShiftStarted(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.TimeShiftStarted(cardId);
        }
        return _localCards[cardId].StartOfTimeShift;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return DateTime.MinValue;
      }
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.RecordingStarted(cardId);
        }
        return _localCards[cardId].RecordingStarted;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return DateTime.MinValue;
      }
    }


    /// <summary>
    /// returns true if card is locked otherwise false
    /// </summary>
    /// <param name="cardId">id of the card</param>
    /// <returns></returns>
    public bool IsLocked(int cardId)
    {
      try
      {
        return _cardLocks[cardId];
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    public bool IsScrambled(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.IsScrambled(cardId);
        }
        return (false == _localCards[cardId].IsReceivingAudioVideo);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// returns the min/max channel numbers for analog cards
    /// </summary>
    public int MinChannel(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.MinChannel(cardId);
        }
        return _localCards[cardId].MinChannel;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return 0;
      }
    }

    public int MaxChannel(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.MaxChannel(cardId);
        }
        return _localCards[cardId].MaxChannel;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return 0;
      }
    }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public bool Tune(int cardId, IChannel channel)
    {
      try
      {
        Log.Write("Controller:Tune {0} to {1}", cardId, channel.Name);
        lock (this)
        {
          if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
          {
            RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
            return RemoteControl.Instance.Tune(cardId, channel);
          }
          if (CurrentChannel(cardId) != null)
          {
            if (CurrentChannel(cardId).Equals(channel)) return true;
          }
          bool result = _localCards[cardId].Tune(channel);
          Log.Write("Controller: Tuner locked:{0} signal strength:{1} signal quality:{2}",
             _localCards[cardId].IsTunerLocked, _localCards[cardId].SignalLevel, _localCards[cardId].SignalQuality);

          return result;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    public bool TuneScan(int cardId, IChannel channel)
    {
      try
      {
        Log.Write("Controller:TuneScan {0} to {1}", cardId, channel.Name);
        lock (this)
        {
          if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
          {
            RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
            return RemoteControl.Instance.TuneScan(cardId, channel);
          }
          bool result = _localCards[cardId].TuneScan(channel);
          Log.Write("Controller: Tuner locked:{0} signal strength:{1} signal quality:{2}",
             _localCards[cardId].IsTunerLocked, _localCards[cardId].SignalLevel, _localCards[cardId].SignalQuality);
          return result;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Locks the specified card
    /// </summary>
    /// <param name="cardId">id of card</param>
    /// <returns>true if card has been locked otherwise false</returns>
    public bool Lock(int cardId)
    {
      try
      {
        lock (this)
        {
          if (_cardLocks[cardId] == true) return false;
          Log.Write("Controller: Lock card:{0}", cardId);
          _cardLocks[cardId] = true;
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Unlocks the card specified
    /// </summary>
    /// <param name="cardId">id of card</param>
    public void Unlock(int cardId)
    {
      try
      {
        Log.Write("Controller: Unlock card:{0}", cardId);
        _cardLocks[cardId] = false;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
      }
    }

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    public void GrabTeletext(int cardId, bool onOff)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          RemoteControl.Instance.GrabTeletext(cardId, onOff);
          return;
        }
        _localCards[cardId].GrabTeletext = onOff;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
      }
    }

    /// <summary>
    /// Gets the teletext page.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    public byte[] GetTeletextPage(int cardId, int pageNumber, int subPageNumber)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.GetTeletextPage(cardId, pageNumber, subPageNumber);
        }
        if (_localCards[cardId].TeletextDecoder == null) return new byte[1] { 1 };
        return _localCards[cardId].TeletextDecoder.GetRawPage(pageNumber, subPageNumber);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return new byte[] { 1 };
      }
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    public int SubPageCount(int cardId, int pageNumber)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.SubPageCount(cardId, pageNumber);
        }
        if (_localCards[cardId].TeletextDecoder == null) return 0;
        return _localCards[cardId].TeletextDecoder.NumberOfSubpages(pageNumber) + 1;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult StartTimeShifting(int cardId, string fileName)
    {
      try
      {
        Log.Write("Controller: StartTimeShifting {0} {1} ", cardId, fileName);
        lock (this)
        {
          if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
          {
            RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
            return RemoteControl.Instance.StartTimeShifting(cardId, fileName);
          }

          if (_epgGrabber != null)
          {
            _epgGrabber.Stop();
          }
          if (_localCards[cardId].IsTimeShifting)
          {
            _clientReferenceCount[cardId]++;
            Log.Write("Controller:  refcount: card:{0} count:{1}", cardId, _clientReferenceCount[cardId]);
            return TvResult.Succeeded;
          }

          Log.Write("Controller: Tuner locked:{0} signal strength:{1} signal quality:{2}",
              _localCards[cardId].IsTunerLocked, _localCards[cardId].SignalLevel, _localCards[cardId].SignalQuality);
          if (WaitForUnScrambledSignal(cardId) == false)
          {
            Log.Write("Controller: channel is scrambled");
            return TvResult.ChannelIsScrambled;
          }

          bool result = _localCards[cardId].StartTimeShifting(fileName);
          if (result == false)
          {
            return TvResult.UnableToStartGraph;
          }
          _clientReferenceCount[cardId]++;
          Log.Write("Controller:  refcount: card:{0} count:{1}", cardId, _clientReferenceCount[cardId]);
          fileName += ".tsbuffer";
          if (!WaitForTimeShiftFile(cardId, fileName))
          {
            return TvResult.NoVideoAudioDetected;
          }
          if (System.IO.File.Exists(fileName))
          {
            _streamer.Start();
            _streamer.Add(String.Format("stream{0}", cardId), fileName);
          }
          else
          {
            Log.Write("Controller: streaming: file not found:{0}", fileName);
          }
          return TvResult.Succeeded;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return TvResult.UnknownError;
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool StopTimeShifting(int cardId)
    {
      try
      {
        Log.Write("Controller: StopTimeShifting {0}", cardId);
        lock (this)
        {
          if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
          {
            RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
            return RemoteControl.Instance.StopTimeShifting(cardId);
          }
          _clientReferenceCount[cardId]--;
          Log.Write("Controller:  refcount: card:{0} count:{1}", cardId, _clientReferenceCount[cardId]);
          if (_clientReferenceCount[cardId] <= 0)
          {
            _clientReferenceCount[cardId] = 0;
            bool result = _localCards[cardId].StopTimeShifting();
            if (result == true)
            {
              Log.Write("Controller:Timeshifting stopped on card:{0}", cardId);
              _streamer.Remove(String.Format("stream{0}", cardId));
            }
            bool allStopped = true;
            Dictionary<int, Card>.Enumerator enumerator = _allDbscards.GetEnumerator();
            while (enumerator.MoveNext())
            {
              KeyValuePair<int, Card> keyPair = enumerator.Current;
              if (IsLocal(keyPair.Value.Server.HostName))
              {
                if (IsTimeShifting(keyPair.Value.IdCard) || IsRecording(keyPair.Value.IdCard))
                {
                  allStopped = false;
                }
              }
            }
            if (allStopped)
            {
              if (_epgGrabber != null)
              {
                _epgGrabber.Start();
              }
            }
            return result;
          }
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <param name="contentRecording">if true then create a content recording else a reference recording</param>
    /// <param name="startTime">not used</param>
    /// <returns></returns>
    public bool StartRecording(int cardId, string fileName, bool contentRecording, long startTime)
    {
      try
      {
        Log.Write("Controller: StartRecording {0} {1}", cardId, fileName);
        lock (this)
        {
          RecordingType recType = RecordingType.Content;
          if (!contentRecording) recType = RecordingType.Reference;
          if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
          {
            RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
            return RemoteControl.Instance.StartRecording(cardId, fileName, contentRecording, startTime);
          }

          return _localCards[cardId].StartRecording(recType, fileName, startTime);
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool StopRecording(int cardId)
    {
      try
      {
        Log.Write("Controller: StopRecording {0}", cardId);
        lock (this)
        {
          if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
          {
            RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
            return RemoteControl.Instance.StopRecording(cardId);
          }
          Log.Write("Controller: StopRecording for card:{0}", cardId);
          if (IsRecording(cardId))
          {
            _localCards[cardId].StopRecording();
          }
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    /// <summary>
    /// scans current transponder for more channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="cardId">IChannel containing the transponder tuning details.</param>
    /// <returns>list of channels found</returns>
    public IChannel[] Scan(int cardId, IChannel channel)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          return RemoteControl.Instance.Scan(cardId, channel);
        }
        ITVScanning scanner = _localCards[cardId].ScanningInterface;
        if (scanner == null) return null;
        scanner.Reset();
        List<IChannel> channelsFound = scanner.Scan(channel);
        if (channelsFound == null) return null;
        return channelsFound.ToArray();

      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return null;
      }
    }

    /// <summary>
    /// grabs the epg.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public void GrabEpg(int cardId)
    {
      try
      {
        if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
        {
          RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
          RemoteControl.Instance.GrabEpg(cardId);
          return;
        }

        ITVEPG epgGrabber = _localCards[cardId].EpgInterface;
        epgGrabber.OnEpgReceived += _handler;
        epgGrabber.GrabEpg();

      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
      }
    }

    public int GetRecordingSchedule(int cardId)
    {
      try
      {
        if (_isMaster == false) return -1;
        return _scheduler.GetRecordingScheduleForCard(cardId);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    #region audio streams
    public IAudioStream[] AvailableAudioStreams(int cardId)
    {
      if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
      {
        RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
        return RemoteControl.Instance.AvailableAudioStreams(cardId);
      }
      List<IAudioStream> streams = _localCards[cardId].AvailableAudioStreams;
      return streams.ToArray();
    }

    public IAudioStream GetCurrentAudioStream(int cardId)
    {
      if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
      {
        RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
        return RemoteControl.Instance.GetCurrentAudioStream(cardId);
      }
      return _localCards[cardId].CurrentAudioStream;
    }

    public void SetCurrentAudioStream(int cardId, IAudioStream stream)
    {
      Log.WriteFile("Controller: setaudiostream:{0} {1}", cardId, stream);
      if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
      {
        RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
        RemoteControl.Instance.SetCurrentAudioStream(cardId, stream);
        return;
      }
      _localCards[cardId].CurrentAudioStream = stream;
    }

    public string GetStreamingUrl(int cardId)
    {
      if (IsLocal(_allDbscards[cardId].Server.HostName) == false)
      {
        RemoteControl.HostName = _allDbscards[cardId].Server.HostName;
        return RemoteControl.Instance.GetStreamingUrl(cardId);
      }
      string hostName = Dns.GetHostName();
      return String.Format("rtsp://{0}/stream{1}", hostName, cardId);
    }

    public string GetRecordingUrl(int idRecording)
    {
      EntityQuery query = new EntityQuery(typeof(Recording));
      query.AddClause(Recording.IdRecordingEntityColumn, EntityQueryOp.EQ, idRecording);
      EntityList<Recording> recordings = DatabaseManager.Instance.GetEntities<Recording>(query);
      if (recordings.Count == 0) return "";
      if (!IsLocal(recordings[0].Server.HostName))
      {
        RemoteControl.HostName = recordings[0].Server.HostName;
        return RemoteControl.Instance.GetRecordingUrl(idRecording);
      }
      _streamer.Start();
      string streamName = _streamer.Add(recordings[0].FileName);
      string hostName = Dns.GetHostName();
      string url = String.Format("rtsp://{0}/{1}", hostName, streamName);
      Log.WriteFile("Controller: streaming url:{0} file:{1}", url, recordings[0].FileName);
      return url;
    }
    #endregion

    #endregion

    #region public interface
    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="channelName">Name of the channel</param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult StartTimeShifting(string channelName, out VirtualCard card)
    {
      Log.Write("Controller: StartTimeShifting {0}", channelName);
      card = null;
      try
      {
        Dictionary<int, Card>.Enumerator enumerator = _allDbscards.GetEnumerator();

        while (enumerator.MoveNext())
        {
          KeyValuePair<int, Card> keyPair = enumerator.Current;
          if (IsTimeShifting(keyPair.Value.IdCard))
          {
            if (CurrentChannelName(keyPair.Value.IdCard) == channelName)
            {
              _clientReferenceCount[keyPair.Value.IdCard]++;
              Log.Write("Controller:  refcount: card:{0} count:{1}", keyPair.Value.IdCard, _clientReferenceCount[keyPair.Value.IdCard]);
              card = new VirtualCard(keyPair.Value.IdCard, Dns.GetHostName());
              card.RecordingFolder = keyPair.Value.RecordingFolder;
              return TvResult.Succeeded;
            }
          }
        }

        List<CardDetail> freeCards = GetFreeCardsForChannelName(channelName);
        if (freeCards.Count == 0)
        {
          Log.Write("Controller: StartTimeShifting failed, no card available");
          return TvResult.AllCardsBusy;
        }
        CardDetail cardInfo = freeCards[0];
        int cardId = cardInfo.Id;
        IChannel channel = cardInfo.TuningDetail;
        if (cardInfo.Card.RecordingFolder == String.Empty)
          cardInfo.Card.RecordingFolder = System.IO.Directory.GetCurrentDirectory();
        if (!IsTimeShifting(cardId))
        {
          CleanTimeShiftFiles(cardInfo.Card.RecordingFolder, String.Format("live{0}.ts", cardId));
        }
        string timeshiftFileName = String.Format(@"{0}\live{1}.ts", cardInfo.Card.RecordingFolder, cardId);

        TvResult result = CardTune(cardId, channel);
        if (result != TvResult.Succeeded)
        {
          return result;
        }

        result = CardTimeShift(cardId, timeshiftFileName);
        if (result != TvResult.Succeeded)
        {
          return result;
        }

        Log.Write("Controller: StartTimeShifting started on card:{0} to {1}", cardId, timeshiftFileName);
        card = new VirtualCard(cardId, Dns.GetHostName());
        card.RecordingFolder = _allDbscards[cardId].RecordingFolder;
        return TvResult.Succeeded;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
    }

    /// <summary>
    /// Checks if the channel specified is being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="channel">Name of the channel</param>
    /// <param name="card">returns the card which is recording the channel</param>
    /// <returns>true if a card is recording the channel, otherwise false</returns>
    public bool IsRecording(string channel, out VirtualCard card)
    {
      card = null;
      try
      {
        Dictionary<int, Card>.Enumerator enumerator = _allDbscards.GetEnumerator();

        while (enumerator.MoveNext())
        {
          KeyValuePair<int, Card> keyPair = enumerator.Current;
          if (IsRecording(keyPair.Value.IdCard))
          {
            if (CurrentChannelName(keyPair.Value.IdCard) == channel)
            {
              card = new VirtualCard(keyPair.Value.IdCard, Dns.GetHostName());
              card.RecordingFolder = keyPair.Value.RecordingFolder;
              return true;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
      return false;
    }

    /// <summary>
    /// Checks if the schedule specified is currently being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <param name="card">returns the card which is recording the channel</param>
    /// <returns>true if a card is recording the schedule, otherwise false</returns>
    public bool IsRecordingSchedule(int idSchedule, out VirtualCard card)
    {
      card = null;
      try
      {
        if (_isMaster == false) return false;
        int cardId;
        if (!_scheduler.IsRecordingSchedule(idSchedule, out cardId)) return false;

        card = new VirtualCard(cardId, Dns.GetHostName());
        card.RecordingFolder = _allDbscards[cardId].RecordingFolder;
        return true;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Stops recording the Schedule specified
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <returns></returns>
    public void StopRecordingSchedule(int idSchedule)
    {
      try
      {
        if (_isMaster == false) return;
        _scheduler.StopRecordingSchedule(idSchedule);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
      }
    }

    /// <summary>
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// </summary>
    public void OnNewSchedule()
    {
      try
      {
        DatabaseManager.Instance.ClearQueryCache();
        //Dispose();
        //Init();
        if (_scheduler != null)
        {
          _scheduler.ResetTimer();
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
      }
    }

    /// <summary>
    /// Enable or disable the epg-grabber
    /// </summary>
    public bool EpgGrabberEnabled
    {
      get
      {
        try
        {
          if (_epgGrabber == null) return false;
          return _epgGrabber.IsRunning;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return false;
        }
      }
      set
      {
        try
        {
          if (value)
          {
            Log.Write("Controller: epg start");
            if (_epgGrabber != null)
            {
              _epgGrabber.Start();
            }
          }
          else
          {
            Log.Write("Controller: epg stop");
            if (_epgGrabber != null)
            {
              _epgGrabber.Stop();
            }
          }
        }
        catch (Exception ex)
        {
          Log.Write(ex);
        }
      }
    }

    /// <summary>
    /// Returns the SQl connection string to the database
    /// </summary>
    public string DatabaseConnectionString
    {
      get
      {
        try
        {
          XmlDocument doc = new XmlDocument();
          doc.Load("IdeaBlade.ibconfig");
          XmlNode node = doc.SelectSingleNode("/ideaBlade/rdbKey/connection");
          return node.InnerText;
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return "";
        }
      }
      set
      {
        try
        {
          XmlDocument doc = new XmlDocument();
          doc.Load("IdeaBlade.ibconfig");
          XmlNode node = doc.SelectSingleNode("/ideaBlade/rdbKey/connection");
          node.InnerText = value;
          doc.Save("IdeaBlade.ibconfig");
          Init();
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return;
        }
      }
    }
    #endregion

    #endregion

    #region private members

    /// <summary>
    /// Gets a list of all free cards which can receive the channel specified
    /// List is sorted by priority
    /// </summary>
    /// <param name="channelName">Name of the channel.</param>
    /// <returns>list containg all free cards which can receive the channel</returns>
    public List<CardDetail> GetFreeCardsForChannelName(string channelName)
    {
      try
      {
        List<CardDetail> cardsAvailable = new List<CardDetail>();

        Log.Write("Controller: find free card for channel {0}", channelName);
        TvBusinessLayer layer = new TvBusinessLayer();
        Channel dbChannel = layer.GetChannelByName(channelName);
        if (dbChannel == null)
        {
          Log.Write("Controller:  channel {0} is not found", channelName);
          return cardsAvailable;
        }

        List<IChannel> tuningDetails = layer.GetTuningChannelByName(channelName);
        if (tuningDetails == null)
        {
          Log.Write("Controller:  No tuning details for channel:{0}", channelName);
          return cardsAvailable;
        }
        if (tuningDetails.Count == 0)
        {
          Log.Write("Controller:  No tuning details for channel:{0}", channelName);
          return cardsAvailable;
        }

        foreach (IChannel tuningDetail in tuningDetails)
        {
          Log.Write("Controller  Tuning detail:{0}", tuningDetail.ToString());
          Dictionary<int, Card>.Enumerator enumerator = _allDbscards.GetEnumerator();
          while (enumerator.MoveNext())
          {
            KeyValuePair<int, Card> keyPair = enumerator.Current;
            bool check = true;
            foreach (CardDetail info in cardsAvailable)
            {
              if (info.Card.DevicePath == keyPair.Value.DevicePath)
              {
                check = false;
              }
            }
            if (check == false) continue;

            //check if card can tune to this channel
            if (IsLocked(keyPair.Value.IdCard))
            {
              Log.Write("Controller:    card:{0} type:{1} is locked", keyPair.Value.IdCard, Type(keyPair.Value.IdCard));
              continue;
            }
            if (CanTune(keyPair.Value.IdCard, tuningDetail) == false)
            {
              Log.Write("Controller:    card:{0} type:{1} cannot tune to channel", keyPair.Value.IdCard, Type(keyPair.Value.IdCard));
              continue;
            }

            if (IsRecording(keyPair.Value.IdCard))
            {
              if (CurrentChannelName(keyPair.Value.IdCard) != channelName)
              {
                Log.Write("Controller:    card:{0} type:{1} is recording:{2}", keyPair.Value.IdCard, Type(keyPair.Value.IdCard), CurrentChannelName(keyPair.Value.IdCard));
                continue;
              }
            }

            //check if channel is mapped to this card...
            foreach (ChannelMap map in dbChannel.ChannelMaps)
            {
              if (map.Card.DevicePath == keyPair.Value.DevicePath)
              {
                Log.Write("Controller:    card:{0} type:{1} is free priority:{2}", keyPair.Value.IdCard, Type(keyPair.Value.IdCard), map.Card.Priority);
                cardsAvailable.Add(new CardDetail(keyPair.Value.IdCard, map.Card, tuningDetail));
                break;
              }
            }
          }
        }
        cardsAvailable.Sort();
        return cardsAvailable;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return null;
      }
    }

    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    TvResult CardTune(int idCard, IChannel channel)
    {
      try
      {
        bool result;
        Log.WriteFile("Controller: CardTune {0} {1}", idCard, channel.Name);
        if (IsScrambled(idCard))
        {
          result = TuneScan(idCard, channel);
          if (result == false) return TvResult.UnableToStartGraph;
          return TvResult.Succeeded;
        }
        if (CurrentChannel(idCard) != null)
        {
          if (CurrentChannel(idCard).Equals(channel)) return TvResult.Succeeded;
        }
        result = TuneScan(idCard, channel);
        if (result == false) return TvResult.UnableToStartGraph;
        return TvResult.Succeeded;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
    }

    /// <summary>
    /// Start timeshifting on the card
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    TvResult CardTimeShift(int idCard, string fileName)
    {
      try
      {
        Log.WriteFile("Controller: CardTimeShift {0} {1}", idCard, fileName);
        if (IsTimeShifting(idCard)) return TvResult.Succeeded;
        return StartTimeShifting(idCard, fileName);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
    }

    /// <summary>
    /// deletes time shifting files left in the specified folder.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="fileName">Name of the file.</param>
    void CleanTimeShiftFiles(string folder, string fileName)
    {
      try
      {
        Log.Write(@"Controller: delete timeshift files {0}\{1}", folder, fileName);
        string[] files = System.IO.Directory.GetFiles(folder);
        for (int i = 0; i < files.Length; ++i)
        {
          if (files[i].IndexOf(fileName) >= 0)
          {
            try
            {
              Log.Write("Controller:   delete {0}", files[i]);
              System.IO.File.Delete(files[i]);
            }
            catch (Exception)
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
    /// <summary>
    /// Waits for un scrambled signal.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns>true if channel is unscrambled else false</returns>
    bool WaitForUnScrambledSignal(int cardId)
    {
      Log.Write("Controller: WaitForUnScrambledSignal");
      DateTime timeStart = DateTime.Now;
      while (true)
      {
        if (IsScrambled(cardId))
        {
          Log.Write("Controller:   scrambled, sleep 100");
          System.Threading.Thread.Sleep(100);
          TimeSpan timeOut = DateTime.Now - timeStart;
          if (timeOut.TotalMilliseconds >= 5000)
          {
            Log.Write("Controller:   return scrambled");
            return false;
          }
        }
        else
        {
          Log.Write("Controller:   return not scrambled");
          return true;
        }
      }
    }

    /// <summary>
    /// Waits for time shift file to be at leat 300kb.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>true when timeshift files is at least of 300kb, else timeshift file is less then 300kb</returns>
    bool WaitForTimeShiftFile(int cardId, string fileName)
    {
      Log.Write("Controller: WaitForTimeShiftFile");
      if (!WaitForUnScrambledSignal(cardId)) return false;
      DateTime timeStart = DateTime.Now;
      ulong fileSize = 0;
      if (IsScrambled(cardId)) return false;

      timeStart = DateTime.Now;
      try
      {
        while (true)
        {
          if (System.IO.File.Exists(fileName))
          {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
              if (stream.Length > 0)
              {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                  stream.Seek(0, SeekOrigin.Begin);

                  ulong newfileSize = reader.ReadUInt64();
                  if (newfileSize != fileSize)
                  {
                    Log.Write("Controller: timeshifting fileSize:{0}", fileSize);
                  }
                  fileSize = newfileSize;
                  if (fileSize >= 300 * 1024) // 300kb ..
                  {
                    TimeSpan ts = DateTime.Now - timeStart;
                    Log.Write("Controller: timeshifting fileSize:{0} {1}", fileSize, ts.TotalMilliseconds);
                    return true;
                  }
                }
              }
            }
          }
          System.Threading.Thread.Sleep(100);
          TimeSpan timeOut = DateTime.Now - timeStart;
          if (timeOut.TotalMilliseconds >= 15000)
          {
            Log.Write("Controller: timeshifting fileSize:{0} TIMEOUT", fileSize);
            return false;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }
    #endregion

  }
}
