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
using System.Collections;
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
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvEngine;
using TvDatabase;
using TvEngine.Events;
namespace TvService
{
  /// <summary>
  /// This class servers all requests from remote clients
  /// and if server is the master it will delegate the requests to the 
  /// correct slave servers
  /// </summary>
  public class TVController : MarshalByRefObject, IController, IDisposable, ITvServerEvent
  {
    #region variables
    /// <summary>
    /// EPG grabber for DVB
    /// </summary>
    EpgGrabber _epgGrabber;
    /// <summary>
    /// Recording scheduler
    /// </summary>
    Scheduler _scheduler;
    /// <summary>
    /// RTSP Streaming Server
    /// </summary>
    RtspStreaming _streamer;
    /// <summary>
    /// Indicates if we're the master server or not
    /// </summary>
    bool _isMaster = false;


    /// <summary>
    /// Reference to our server
    /// </summary>
    Server _ourServer = null;/// <summary>


    Dictionary<int, TvCard> _cards;
    /// 
    /// Plugins
    /// </summary>
    PluginLoader _plugins = null;
    #endregion

    #region events
    public event TvServerEventHandler OnTvServerEvent;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TVController"/> class.
    /// </summary>
    public TVController()
    {
      GlobalServiceProvider.Instance.Add<ITvServerEvent>(this);
      if (Init() == false)
      {
        System.Threading.Thread.Sleep(5000);
        if (Init() == false)
        {
          System.Threading.Thread.Sleep(5000);
          Init();
        }
      }
    }

    /// <summary>
    /// Determines whether the specified card is the local pc or not.
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>
    /// 	<c>true</c> if the specified host name is local; otherwise, <c>false</c>.
    /// </returns>
    bool IsLocal(int cardId)
    {
      return _cards[cardId].IsLocal;
    }

    /// <summary>
    /// Determines whether the specified card is the local pc or not.
    /// </summary>
    /// <param name="card">Card</param>
    /// <returns>
    /// 	<c>true</c> if the specified host name is local; otherwise, <c>false</c>.
    /// </returns>
    bool IsLocal(Card card)
    {
      return _cards[card.IdCard].IsLocal;
    }

    /// <summary>
    /// Determines whether the specified host name is the local pc or not.
    /// </summary>
    /// <param name="hostName">Name of the host or ip adress</param>
    /// <returns>
    /// 	<c>true</c> if the specified host name is local; otherwise, <c>false</c>.
    /// </returns>
    bool IsLocal(string hostName)
    {
      if (hostName == "127.0.0.1") return true;
      string localHostName = Dns.GetHostName();
      if (String.Compare(hostName, localHostName, true) == 0) return true;
      IPHostEntry local = Dns.GetHostByName(localHostName);
      foreach (IPAddress ipaddress in local.AddressList)
      {
        if (String.Compare(hostName, ipaddress.ToString(), true) == 0) return true;
      }
      return false;
    }

    /// <summary>
    /// Determines whether the card is in use
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user which uses the card</param>
    /// <returns>
    /// 	<c>true</c> if card is in use; otherwise, <c>false</c>.
    /// </returns>
    public bool IsCardInUse(int cardId, out User user)
    {
      return _cards[cardId].IsLocked(out user);
    }
    /// <summary>
    /// Gets the user for card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    public User GetUserForCard(int cardId)
    {
      User user;
      _cards[cardId].IsLocked(out user);
      return user;
    }

    /// <summary>
    /// Locks the card for the specified user
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user.</param>
    public void LockCard(int cardId, User user)
    {
      _cards[cardId].Lock(user);
    }

    /// <summary>
    /// Unlocks the card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    public void UnlockCard(int cardId)
    {
      _cards[cardId].Unlock();
    }


    /// <summary>
    /// Initalizes the controller.
    /// It will update the database with the cards found on this system
    /// start the epg grabber and scheduler
    /// and check if its supposed to be a master or slave controller
    /// </summary>
    bool Init()
    {
      try
      {
        //load the database connection string from the config file
        Log.WriteFile(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(DatabaseConnectionString);

        _cards = new Dictionary<int, TvCard>();
        TvCardCollection localCardCollection = new TvCardCollection();
        Dictionary<int, ITVCard> localcards = new Dictionary<int, ITVCard>();

        _plugins = new PluginLoader();
        _plugins.Load();


        //log all local ip adresses, usefull for debugging problems
        Log.Write("Controller: Started at {0}", Dns.GetHostName());
        IPHostEntry local = Dns.GetHostByName(Dns.GetHostName());
        foreach (IPAddress ipaddress in local.AddressList)
        {
          Log.Write("Controller: local ip adress:{0}", ipaddress.ToString());
        }

        //get all registered servers from the database
        IList servers;
        try
        {
          servers = Server.ListAll();
        }
        catch (Exception ex)
        {
          Log.Error("!!!Controller:Unable to connect to database!!!");
          Log.Error("Controller: database connection string:{0}", Gentle.Framework.ProviderFactory.GetDefaultProvider().ConnectionString);
          Log.Error("Sql error:{0}", ex.Message);
          return false;
        }

        // find ourself
        foreach (Server server in servers)
        {
          if (IsLocal(server.HostName))
          {
            Log.WriteFile("Controller: server running on {0}", server.HostName);
            _ourServer = server;
            break;
          }
        }

        //we dont exists yet?
        if (_ourServer == null)
        {
          //then add ourselfs to the server
          Log.WriteFile("Controller: create new server in database");
          _ourServer = new Server(false, Dns.GetHostName());
          if (servers.Count == 0)
          {
            //there are no other servers
            //so we are the master one.
            _ourServer.IsMaster = true;
            _isMaster = true;
          }
          _ourServer.Persist();
          Log.WriteFile("Controller: new server created for {0} master:{1} ", Dns.GetHostName(), _isMaster);
        }
        _isMaster = _ourServer.IsMaster;

        //enumerate all tv cards in this pc...
        TvBusinessLayer layer = new TvBusinessLayer();
        for (int i = 0; i < localCardCollection.Cards.Count; ++i)
        {
          //for each card, check if its already mentioned in the database
          bool found = false;
          IList cards = _ourServer.ReferringCard();
          foreach (Card card in cards)
          {
            if (card.DevicePath == localCardCollection.Cards[i].DevicePath)
            {
              found = true;
              break;
            }
          }
          if (!found)
          {
            // card is not yet in the database, so add it
            Log.WriteFile("Controller: add card:{0}", localCardCollection.Cards[i].Name);
            layer.AddCard(localCardCollection.Cards[i].Name, localCardCollection.Cards[i].DevicePath, _ourServer);
          }
        }

        //delete cards from the database which are removed from the pc
        IList cardsInDbs = Card.ListAll();
        int cardsInstalled = localCardCollection.Cards.Count;
        foreach (Card dbsCard in cardsInDbs)
        {
          if (dbsCard.ReferencedServer().IdServer == _ourServer.IdServer)
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
              dbsCard.Delete();
            }
          }
        }

        localcards = new Dictionary<int, ITVCard>();


        cardsInDbs = Card.ListAll();
        foreach (Card card in cardsInDbs)
        {
          if (IsLocal(card.ReferencedServer().HostName))
          {
            for (int x = 0; x < localCardCollection.Cards.Count; ++x)
            {
              if (localCardCollection.Cards[x].DevicePath == card.DevicePath)
              {
                localcards[card.IdCard] = localCardCollection.Cards[x];
                break;
              }
            }
          }
        }

        Log.WriteFile("Controller: setup hybrid cards");
        IList cardgroups = CardGroup.ListAll();
        foreach (CardGroup group in cardgroups)
        {
          IList cards = group.CardGroupMaps();
          HybridCard hybridCard = new HybridCard();
          foreach (CardGroupMap card in cards)
          {
            if (localcards.ContainsKey(card.IdCard))
            {
              localcards[card.IdCard].IsHybrid = true;
              hybridCard.Add(card.IdCard, localcards[card.IdCard]);
              localcards[card.IdCard] = hybridCard;
            }
          }
        }
        cardsInDbs = Card.ListAll();
        foreach (Card card in cardsInDbs)
        {
          TvCard tvcard = new TvCard();
          tvcard.DataBaseCard = card;
          if (localcards.ContainsKey(card.IdCard))
          {
            tvcard.Card = localcards[card.IdCard];
            tvcard.IsLocal = true;
          }
          else
          {
            tvcard.Card = null;
            tvcard.IsLocal = false;
          }
          _cards[card.IdCard] = tvcard;
        }

        Log.WriteFile("Controller: setup streaming");
        _streamer = new RtspStreaming(_ourServer.HostName);

        if (_isMaster)
        {
          _epgGrabber = new EpgGrabber(this);
          _epgGrabber.Start();
          _scheduler = new Scheduler(this);
          _scheduler.Start();
        }

        foreach (ITvServerPlugin plugin in _plugins.Plugins)
        {
          if (plugin.MasterOnly == false || _isMaster)
          {
            plugin.Start(this);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
      Log.WriteFile("Controller: initalized");
      return true;
    }
    #endregion

    #region MarshalByRefObject overrides
    public override object InitializeLifetimeService()
    {
      return null;
    }
    #endregion

    #region IDisposable Members

    /// <summary>
    /// Clean up the controller when service is stopped
    /// </summary>
    public void Dispose()
    {
      DeInit();
    }

    /// <summary>
    /// Cleans up the controller
    /// </summary>
    public void DeInit()
    {
      if (_plugins != null)
      {
        foreach (ITvServerPlugin plugin in _plugins.Plugins)
        {
          if (plugin.MasterOnly == false || _isMaster)
          {
            plugin.Stop();
          }
        }
        _plugins = null;
      }
      //stop the RTSP streamer server
      if (_streamer != null)
      {
        Log.WriteFile("Controller: stop streamer...");
        _streamer.Stop();
        _streamer = null;
        Log.WriteFile("Controller: streamer stopped...");
      }
      //stop the recording scheduler
      if (_scheduler != null)
      {
        Log.WriteFile("Controller: stop scheduler...");
        _scheduler.Stop();
        _scheduler = null;
        Log.WriteFile("Controller: scheduler stopped...");
      }
      //stop the epg grabber
      if (_epgGrabber != null)
      {
        Log.WriteFile("Controller: stop epg grabber...");
        _epgGrabber.Stop();
        _epgGrabber = null;
        Log.WriteFile("Controller: epg stopped...");
      }

      //clean up the tv cards
      Dictionary<int, TvCard>.Enumerator enumerator = _cards.GetEnumerator();
      while (enumerator.MoveNext())
      {
        KeyValuePair<int, TvCard> key = enumerator.Current;
        Log.WriteFile("Controller:  dispose card:{0}", key.Value.CardName);
        try
        {
          key.Value.Dispose();
        }
        catch (Exception ex)
        {
          Log.Write(ex);
        }
      }
      Gentle.Common.CacheManager.Clear();
    }

    #endregion

    #region IController Members

    #region internal interface
    /// <summary>
    /// Gets the server.
    /// </summary>
    /// <value>The server.</value>
    public int IdServer
    {
      get
      {
        return _ourServer.IdServer;
      }
    }
    /// <summary>
    /// Gets the total number of cards installed.
    /// </summary>
    /// <value>Number which indicates the cards installed</value>
    public int Cards
    {
      get
      {
        return _cards.Count;
      }
    }

    /// <summary>
    /// Gets the card Id for a card
    /// </summary>
    /// <param name="cardIndex">Index of the card.</param>
    /// <value>id of card</value>
    public int CardId(int cardIndex)
    {
      IList cards = Card.ListAll();
      return ((Card)cards[cardIndex]).IdCard;
    }

    /// <summary>
    /// returns if the card is enabled or disabled
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <value>true if enabled, otherwise false</value>
    public bool Enabled(int cardId)
    {
      return _cards[cardId].DataBaseCard.Enabled;
    }

    /// <summary>
    /// Gets the type of card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <value>cardtype (Analog,DvbS,DvbT,DvbC,Atsc)</value>
    public CardType Type(int cardId)
    {
      return _cards[cardId].Type;
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <returns>name of card</returns>
    public string CardName(int cardId)
    {
      return _cards[cardId].CardName;
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <param name="channel">channel.</param>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(int cardId, IChannel channel)
    {
      return _cards[cardId].CanTune(channel);
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>device of card</returns>
    public string CardDevice(int cardId)
    {
      return _cards[cardId].CardDevice();
    }

    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel(int cardId)
    {
      return _cards[cardId].CurrentChannel;
    }
    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>id of database channel</returns>
    public int CurrentDbChannel(int cardId)
    {
      return _cards[cardId].CurrentDbChannel;
    }

    /// <summary>
    /// Gets the current channel name.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>channel</returns>
    public string CurrentChannelName(int cardId)
    {
      return _cards[cardId].CurrentChannelName;
    }


    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    public bool TunerLocked(int cardId)
    {
      return _cards[cardId].TunerLocked;
    }

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    public int SignalQuality(int cardId)
    {
      return _cards[cardId].SignalQuality;
    }

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal level (0-100)</returns>
    public int SignalLevel(int cardId)
    {
      return _cards[cardId].SignalLevel;
    }
    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    public void UpdateSignalSate(int cardId)
    {
      _cards[cardId].UpdateSignalSate();
    }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>filename or null when not recording</returns>
    public string FileName(int cardId)
    {
      return _cards[cardId].FileName;
    }

    public string TimeShiftFileName(int cardId)
    {
      return _cards[cardId].TimeShiftFileName;
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(int cardId)
    {
      return _cards[cardId].IsTimeShifting;
    }

    /// <summary>
    /// Gets a value indicating whether any card is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this a card is recording; otherwise, <c>false</c>.
    /// </value>
    public bool IsAnyCardRecording
    {
      get
      {
        Dictionary<int, TvCard>.Enumerator enumerator = _cards.GetEnumerator();

        while (enumerator.MoveNext())
        {
          KeyValuePair<int, TvCard> keyPair = enumerator.Current;

          if (keyPair.Value.IsRecording) return true;
        }
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
      return _cards[cardId].IsRecording;
    }

    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    public bool IsScanning(int cardId)
    {
      return _cards[cardId].IsScanning;
    }

    /// <summary>
    /// Returns if the card is grabbing the epg or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    public bool IsGrabbingEpg(int cardId)
    {
      return _cards[cardId].IsGrabbingEpg;
    }

    /// <summary>
    /// Returns if the card is grabbing teletext or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing teletext otherwise false</returns>
    public bool IsGrabbingTeletext(int cardId)
    {
      return _cards[cardId].IsGrabbingTeletext;
    }

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    public bool HasTeletext(int cardId)
    {
      return _cards[cardId].HasTeletext;
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(int cardId, int pageNumber)
    {
      return _cards[cardId].TeletextRotation(pageNumber);
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime TimeShiftStarted(int cardId)
    {
      return _cards[cardId].TimeShiftStarted;
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted(int cardId)
    {
      return _cards[cardId].RecordingStarted;
    }


    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    public bool IsScrambled(int cardId)
    {
      return _cards[cardId].IsScrambled;
    }

    /// <summary>
    /// returns the min/max channel numbers for analog cards
    /// </summary>
    public int MinChannel(int cardId)
    {
      return _cards[cardId].MinChannel;
    }

    public int MaxChannel(int cardId)
    {
      return _cards[cardId].MaxChannel;
    }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public bool Tune(int cardId, IChannel channel, int idChannel)
    {
      try
      {
        if (_cards[cardId].DataBaseCard.Enabled == false) return false;
        Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(cardId), GetUserForCard(cardId), channel));
        return _cards[cardId].Tune(channel, idChannel);
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(cardId), GetUserForCard(cardId), channel));
      }
    }

    public bool TuneScan(int cardId, IChannel channel, int idChannel)
    {
      try
      {
        if (_cards[cardId].DataBaseCard.Enabled == false) return false;
        Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(cardId), GetUserForCard(cardId), channel));
        Log.Write("Controller:TuneScan {0} to {1}", cardId, channel.Name);
        return _cards[cardId].TuneScan(channel, idChannel);
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(cardId), GetUserForCard(cardId), channel));
      }
    }



    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    public void GrabTeletext(int cardId, bool onOff)
    {
      _cards[cardId].GrabTeletext(onOff);
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
      return _cards[cardId].GetTeletextPage(pageNumber, subPageNumber);
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    public int SubPageCount(int cardId, int pageNumber)
    {
      return _cards[cardId].SubPageCount(pageNumber);
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
        if (_epgGrabber != null)
        {
          _epgGrabber.Stop();
        }
        TvResult result = _cards[cardId].StartTimeShifting(fileName);
        if (result == TvResult.Succeeded)
        {
          if (System.IO.File.Exists(fileName + ".tsbuffer"))
          {
            _streamer.Start();
            RtspStream stream = new RtspStream(String.Format("stream{0}", cardId), fileName + ".tsbuffer", _cards[cardId].Card);
            _streamer.AddStream(stream);
          }
          else
          {
            Log.Write("Controller: streaming: file not found:{0}", fileName);
          }
        }
        return result;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return TvResult.UnknownError;
    }

    public void StopCard(int cardId)
    {
      _cards[cardId].StopCard();
    }
    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool StopTimeShifting(int cardId, User user)
    {
      try
      {
        if (_cards[cardId].DataBaseCard.Enabled == false) return true;
        if (false == _cards[cardId].IsTimeShifting) return true;
        if (_cards[cardId].IsRecording) return true;

        Log.Write("Controller: StopTimeShifting {0}", cardId);
        lock (this)
        {
          bool result = false;
          User cardUser;
          if (IsCardInUse(cardId, out cardUser))
          {
            if (user.IsAdmin == false && cardUser.Name != user.Name) return false;
          }
          if (_cards[cardId].StopTimeShifting(user))
          {
            result = true;
            Log.Write("Controller:Timeshifting stopped on card:{0}", cardId);
            _streamer.Remove(String.Format("stream{0}", cardId));
          }

          bool allStopped = true;
          Dictionary<int, TvCard>.Enumerator enumerator = _cards.GetEnumerator();
          while (enumerator.MoveNext())
          {
            KeyValuePair<int, TvCard> keyPair = enumerator.Current;
            if (keyPair.Value.IsLocal)
            {
              if (keyPair.Value.IsTimeShifting || keyPair.Value.IsRecording)
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
    public bool StartRecording(int cardId, ref string fileName, bool contentRecording, long startTime)
    {
      return _cards[cardId].StartRecording(ref  fileName, contentRecording, startTime);
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool StopRecording(int cardId)
    {
      return _cards[cardId].StopRecording();
    }

    /// <summary>
    /// scans current transponder for more channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="cardId">IChannel containing the transponder tuning details.</param>
    /// <returns>list of channels found</returns>
    public IChannel[] Scan(int cardId, IChannel channel)
    {
      return _cards[cardId].Scan(channel);
    }

    /// <summary>
    /// grabs the epg.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool GrabEpg(BaseEpgGrabber grabber, int cardId)
    {
      return _cards[cardId].GrabEpg(grabber);
    }
    public List<EpgChannel> Epg(int cardId)
    {
      return _cards[cardId].Epg;
    }

    public int GetRecordingSchedule(int cardId)
    {
      try
      {
        if (_isMaster == false) return -1;
        if (_cards[cardId].DataBaseCard.Enabled == false) return -1;
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
      return _cards[cardId].AvailableAudioStreams;
    }

    public IAudioStream GetCurrentAudioStream(int cardId)
    {
      return _cards[cardId].GetCurrentAudioStream();
    }

    public void SetCurrentAudioStream(int cardId, IAudioStream stream)
    {
      _cards[cardId].SetCurrentAudioStream(stream);
    }

    public string GetStreamingUrl(int cardId)
    {
      try
      {
        if (_cards[cardId].DataBaseCard.Enabled == false) return "";
        if (IsLocal(cardId) == false)
        {
          try
          {
            RemoteControl.HostName = _cards[cardId].DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.GetStreamingUrl(cardId);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _cards[cardId].DataBaseCard.ReferencedServer().HostName);
            return "";
          }
        }
        return String.Format("rtsp://{0}/stream{1}", _ourServer.HostName, cardId);
      }
      catch (Exception)
      {
      }
      return "";
    }

    public string GetRecordingUrl(int idRecording)
    {
      try
      {
        Recording recording = Recording.Retrieve(idRecording);
        if (recording == null) return "";
        if (recording.FileName == null) return "";
        if (recording.FileName.Length == 0) return "";
        if (!IsLocal(recording.ReferencedServer().HostName))
        {
          try
          {
            RemoteControl.HostName = recording.ReferencedServer().HostName;
            return RemoteControl.Instance.GetRecordingUrl(idRecording);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", recording.ReferencedServer().HostName);
            return "";
          }
        }
        try
        {
          if (System.IO.File.Exists(recording.FileName))
          {
            _streamer.Start();
            string streamName = recording.FileName.GetHashCode().ToString();
            RtspStream stream = new RtspStream(streamName, recording.FileName, recording.Title);
            _streamer.AddStream(stream);
            string url = String.Format("rtsp://{0}/{1}", _ourServer.HostName, streamName);
            Log.WriteFile("Controller: streaming url:{0} file:{1}", url, recording.FileName);
            return url;
          }
        }
        catch (Exception)
        {
        }
      }
      catch (Exception)
      {
      }
      return "";
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
    public TvResult StartTimeShifting(string channelName, User user, out VirtualCard card)
    {
      card = null;
      TvBusinessLayer layer = new TvBusinessLayer();
      Channel ch = layer.GetChannelByName(channelName);
      if (ch == null) return TvResult.UnknownChannel;
      return StartTimeShifting(ch.IdChannel, user, out card);

    }
    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="channelName">Name of the channel</param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult StartTimeShifting(int idChannel, User user, out VirtualCard card)
    {
      Channel channel = Channel.Retrieve(idChannel);
      Log.Write("Controller: StartTimeShifting {0} {1}", channel.Name, channel.IdChannel);
      card = null;
      try
      {
        Dictionary<int, TvCard>.Enumerator enumerator = _cards.GetEnumerator();

        while (enumerator.MoveNext())
        {
          KeyValuePair<int, TvCard> keyPair = enumerator.Current;
          if (keyPair.Value.IsTimeShifting)
          {
            if (keyPair.Value.CurrentDbChannel == channel.IdChannel)
            {
              card = GetVirtualCard(keyPair.Value.DataBaseCard.IdCard);
              return TvResult.Succeeded;
            }
          }
        }

        TvResult result;
        List<CardDetail> freeCards = GetFreeCardsForChannel(channel, user, out result);
        if (freeCards.Count == 0)
        {
          Log.Write("Controller: StartTimeShifting failed:{0}", result);
          return result;
        }
        CardDetail cardInfo = freeCards[0];
        int cardId = cardInfo.Id;
        IChannel tuneChannel = cardInfo.TuningDetail;
        if (cardInfo.Card.RecordingFolder == String.Empty)
          cardInfo.Card.RecordingFolder = System.IO.Directory.GetCurrentDirectory();
        if (cardInfo.Card.TimeShiftFolder == String.Empty)
          cardInfo.Card.TimeShiftFolder = System.IO.Directory.GetCurrentDirectory();
        if (!IsTimeShifting(cardId))
        {
          CleanTimeShiftFiles(cardInfo.Card.TimeShiftFolder, String.Format("live{0}.ts", cardId));
        }
        string timeshiftFileName = String.Format(@"{0}\live{1}.ts", cardInfo.Card.TimeShiftFolder, cardId);

        result = CardTune(cardId, tuneChannel, channel);
        if (result != TvResult.Succeeded)
        {
          return result;
        }

        result = StartTimeShifting(cardId, timeshiftFileName);
        if (result != TvResult.Succeeded)
        {
          return result;
        }
        LockCard(cardId, user);
        Log.Write("Controller: StartTimeShifting started on card:{0} to {1}", cardId, timeshiftFileName);
        card = GetVirtualCard(cardId);
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
        Dictionary<int, TvCard>.Enumerator enumerator = _cards.GetEnumerator();

        while (enumerator.MoveNext())
        {
          KeyValuePair<int, TvCard> keyPair = enumerator.Current;
          if (keyPair.Value.IsRecording)
          {
            if (keyPair.Value.CurrentChannelName == channel)
            {
              card = GetVirtualCard(keyPair.Value.DataBaseCard.IdCard);
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

        card = GetVirtualCard(cardId);
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
        //DatabaseManager.Instance.ClearQueryCache();
        //Dispose();
        //Init();
        if (_scheduler != null)
        {
          _scheduler.ResetTimer();
        }
        Fire(this, new TvServerEventArgs(TvServerEventType.ScheduledAdded));

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

    public void Restart()
    {
      try
      {
        DeInit();
        Init();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
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
          doc.Load(String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
          XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
          XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString"); ;
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
          doc.Load(String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));

          XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
          XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString"); ;
          node.InnerText = value;
          doc.Save(String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
          Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(value);
          DeInit();
          Init();
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          return;
        }
      }
    }
    /// <summary>
    /// Gets a value indicating whether all cards are idle.
    /// </summary>
    /// <value><c>true</c> if [all cards idle]; otherwise, <c>false</c>.</value>
    public bool AllCardsIdle
    {
      get
      {
        Dictionary<int, TvCard>.Enumerator enumer = _cards.GetEnumerator();
        while (enumer.MoveNext())
        {
          int cardId = enumer.Current.Key;
          if (IsRecording(cardId)) return false;
          if (IsTimeShifting(cardId)) return false;
          if (IsScanning(cardId)) return false;
          if (IsGrabbingEpg(cardId)) return false;
        }
        return true;
      }
    }
    #region DiSEqC

    public void DiSEqCGetPosition(int cardId, out int satellitePosition, out int stepsAzimuth, out int stepsElevation)
    {
      _cards[cardId].DiSEqCGetPosition(out  satellitePosition, out  stepsAzimuth, out  stepsElevation);
    }

    public void DiSEqCReset(int cardId)
    {
      _cards[cardId].DiSEqCReset();
    }
    public void DiSEqCStopMotor(int cardId)
    {
      _cards[cardId].DiSEqCStopMotor();
    }
    public void DiSEqCSetEastLimit(int cardId)
    {
      _cards[cardId].DiSEqCSetEastLimit();
    }
    public void DiSEqCSetWestLimit(int cardId)
    {
      _cards[cardId].DiSEqCSetWestLimit();
    }
    public void DiSEqCForceLimit(int cardId, bool onOff)
    {
      _cards[cardId].DiSEqCForceLimit(onOff);
    }
    public void DiSEqCDriveMotor(int cardId, DiSEqCDirection direction, byte numberOfSteps)
    {
      _cards[cardId].DiSEqCDriveMotor(direction, numberOfSteps);
    }
    public void DiSEqCStorePosition(int cardId, byte position)
    {
      _cards[cardId].DiSEqCStorePosition(position);
    }
    public void DiSEqCGotoReferencePosition(int cardId)
    {
      _cards[cardId].DiSEqCGotoReferencePosition();
    }
    public void DiSEqCGotoPosition(int cardId, byte position)
    {
      _cards[cardId].DiSEqCGotoPosition(position);
    }
    #endregion
    /// <summary>
    /// Stops the grabbing epg.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    public void StopGrabbingEpg(int cardId)
    {
      _cards[cardId].StopGrabbingEpg();
    }

    public List<string> ServerIpAdresses
    {
      get
      {
        List<string> ipadresses = new List<string>();
        string localHostName = Dns.GetHostName();
        IPHostEntry local = Dns.GetHostByName(localHostName);
        foreach (IPAddress ipaddress in local.AddressList)
        {
          ipadresses.Add(ipaddress.ToString());
        }
        return ipadresses;
      }
    }
    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void ClearCache()
    {
      Gentle.Common.CacheManager.Clear();
    }


    #endregion

    #region streaming
    public List<RtspClient> StreamingClients
    {
      get
      {
        if (_streamer == null) return new List<RtspClient>();
        return _streamer.Clients;
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
    public List<CardDetail> GetFreeCardsForChannel(Channel dbChannel, User user, out TvResult result)
    {
      try
      {
        List<CardDetail> cardsAvailable = new List<CardDetail>();

        Log.Write("Controller: find free card for channel {0}", dbChannel.Name);
        TvBusinessLayer layer = new TvBusinessLayer();

        List<IChannel> tuningDetails = layer.GetTuningChannelByName(dbChannel);
        if (tuningDetails == null)
        {
          Log.Write("Controller:  No tuning details for channel:{0}", dbChannel.Name);
          result = TvResult.NoTuningDetails;
          return cardsAvailable;
        }

        if (tuningDetails.Count == 0)
        {
          Log.Write("Controller:  No tuning details for channel:{0}", dbChannel.Name);
          result = TvResult.NoTuningDetails;
          return cardsAvailable;
        }

        int cardsFound = 0;
        int number = 0;
        Log.Write("Controller:   got {0} tuning details for {1}", tuningDetails.Count, dbChannel.Name);
        foreach (IChannel tuningDetail in tuningDetails)
        {
          number++;
          Log.Write("Controller:   tuning detail #{0} {1} ", number, tuningDetail.ToString());
          Dictionary<int, TvCard>.Enumerator enumerator = _cards.GetEnumerator();

          //for each card...
          while (enumerator.MoveNext())
          {
            KeyValuePair<int, TvCard> keyPair = enumerator.Current;
            bool check = true;

            //get the card info
            foreach (CardDetail info in cardsAvailable)
            {
              if (info.Card.DevicePath == keyPair.Value.DataBaseCard.DevicePath)
              {
                check = false;
              }
            }
            if (check == false) continue;

            //check if card is enabled
            if (keyPair.Value.DataBaseCard.Enabled == false)
            {
              Log.Write("Controller:    card:{0} type:{1} is disabled", keyPair.Value.DataBaseCard.IdCard, Type(keyPair.Value.DataBaseCard.IdCard));
              continue;
            }

            //check if card is able to tune to the channel
            if (CanTune(keyPair.Value.DataBaseCard.IdCard, tuningDetail) == false)
            {
              Log.Write("Controller:    card:{0} type:{1} cannot tune to channel", keyPair.Value.DataBaseCard.IdCard, Type(keyPair.Value.DataBaseCard.IdCard));
              continue;
            }

            //check if channel is mapped to this card
            ChannelMap channelMap = null;
            foreach (ChannelMap map in dbChannel.ReferringChannelMap())
            {
              if (map.ReferencedCard().DevicePath == keyPair.Value.DataBaseCard.DevicePath)
              {
                channelMap = map;
                break;
              }
            }
            if (null == channelMap)
            {
              Log.Write("Controller:    card:{0} type:{1} channel not mapped", keyPair.Value.DataBaseCard.IdCard, Type(keyPair.Value.DataBaseCard.IdCard));
              continue;
            }

            cardsFound++;
            //check if card is in use
            if (user.IsAdmin == false)
            {
              User cardUser;
              if (IsCardInUse(keyPair.Value.DataBaseCard.IdCard, out cardUser))
              {
                if (cardUser.Name != user.Name)
                {
                  Log.Write("Controller:    card:{0} type:{1} is used by {2}", keyPair.Value.DataBaseCard.IdCard, Type(keyPair.Value.DataBaseCard.IdCard), cardUser.Name);
                  continue;
                }
              }
            }
            //check if card is recording
            if (IsRecording(keyPair.Value.DataBaseCard.IdCard))
            {
              if (CurrentDbChannel(keyPair.Value.DataBaseCard.IdCard) != dbChannel.IdChannel)
              {
                Log.Write("Controller:    card:{0} type:{1} is recording:{2}", keyPair.Value.DataBaseCard.IdCard, Type(keyPair.Value.DataBaseCard.IdCard), CurrentChannelName(keyPair.Value.DataBaseCard.IdCard));
                continue;
              }
            }

            Log.Write("Controller:    card:{0} type:{1} is free priority:{2}", keyPair.Value.DataBaseCard.IdCard, Type(keyPair.Value.DataBaseCard.IdCard), channelMap.ReferencedCard().Priority);

            cardsAvailable.Add(new CardDetail(keyPair.Value.DataBaseCard.IdCard, channelMap.ReferencedCard(), tuningDetail));
          }
        }
        cardsAvailable.Sort();
        if (cardsAvailable.Count > 0)
        {
          result = TvResult.Succeeded;
        }
        else
        {
          if (cardsFound == 0)
            result = TvResult.ChannelNotMappedToAnyCard;
          else
            result = TvResult.AllCardsBusy;
        }
        return cardsAvailable;
      }
      catch (Exception ex)
      {
        result = TvResult.UnknownError;
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
    TvResult CardTune(int idCard, IChannel channel, Channel dbChannel)
    {
      try
      {
        if (_cards[idCard].DataBaseCard.Enabled == false) return TvResult.CardIsDisabled;
        Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(idCard), GetUserForCard(idCard), channel));
        return _cards[idCard].CardTune(channel, dbChannel);
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(idCard), GetUserForCard(idCard), channel));
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
      return _cards[idCard].CardTimeShift(fileName);
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


    public void Fire(object sender, EventArgs args)
    {
      try
      {
        if (OnTvServerEvent != null)
          OnTvServerEvent(sender, args);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }
    #endregion

    VirtualCard GetVirtualCard(int cardId)
    {
      VirtualCard card = new VirtualCard(cardId);
      card.RecordingFormat = _cards[cardId].DataBaseCard.RecordingFormat;
      card.RecordingFolder = _cards[cardId].DataBaseCard.RecordingFolder;
      card.TimeshiftFolder = _cards[cardId].DataBaseCard.TimeShiftFolder;
      card.RemoteServer = Dns.GetHostName();
      return card;
    }
  }
}
