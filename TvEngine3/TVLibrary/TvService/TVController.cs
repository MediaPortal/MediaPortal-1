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
using System.Threading;
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
    #region constants
    private const int HEARTBEAT_MAX_SECS_EXCEED_ALLOWED = 30;
    #endregion

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

    Thread heartBeatMonitorThread = null;

    /// <summary>
    /// Reference to our server
    /// </summary>
    Server _ourServer = null;/// <summary>

    TvCardCollection _localCardCollection = null;

    Dictionary<int, ITvCardHandler> _cards;
    /// 
    /// Plugins
    /// </summary>
    PluginLoader _plugins = null;
    List<ITvServerPlugin> _pluginsStarted = new List<ITvServerPlugin>();
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

    public Dictionary<int, ITvCardHandler> CardCollection
    {
      get
      {
        return _cards;
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
    /// Checks if there's a card which is not in use
    /// </summary>
    /// <returns>true if there is a card no user has locked</returns>
    public bool IsAnyCardIdle()
    {
      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      while (en.MoveNext())
      {
        ITvCardHandler card = en.Current.Value;
        if (card.IsIdle)
          return true;
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
      return _cards[cardId].Users.IsLocked(out user);
    }
    /// <summary>
    /// Gets the user for card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    public User GetUserForCard(int cardId)
    {
      User user;
      _cards[cardId].Users.IsLocked(out user);
      return user;
    }

    /// <summary>
    /// Locks the card for the specified user
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user.</param>
    public void LockCard(int cardId, User user)
    {
      _cards[cardId].Users.Lock(user);
    }

    /// <summary>
    /// Unlocks the card.
    /// </summary>
    /// <param name="user">The user.</param>
    public void UnlockCard(User user)
    {
      if (user.CardId < 0) return;
      _cards[user.CardId].Users.Unlock(user);
    }


    /// <summary>
    /// Initalizes the controller.
    /// It will update the database with the cards found on this system
    /// start the epg grabber and scheduler
    /// and check if its supposed to be a master or slave controller
    /// </summary>
    bool Init()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        GlobalServiceProvider.Instance.Remove<ITvServerEvent>();
      }
      GlobalServiceProvider.Instance.Add<ITvServerEvent>(this);
      try
      {
        //load the database connection string from the config file
        Log.WriteFile(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        string connectionString, provider;
        GetDatabaseConnectionString(out connectionString, out provider);
        string ConnectionLog = connectionString.Remove(connectionString.IndexOf(@"Password=") + 8);
        Log.Info("Controller: using {0} database connection: {1}", provider, ConnectionLog);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);

        _cards = new Dictionary<int, ITvCardHandler>();
        _localCardCollection = new TvCardCollection();
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
          Log.Error("Controller: database connection string:{0}", Utils.BlurConnectionStringPassword(Gentle.Framework.ProviderFactory.GetDefaultProvider().ConnectionString));
          Log.Error("Sql error:{0}", Utils.BlurConnectionStringPassword(ex.Message));
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
        for (int i = 0; i < _localCardCollection.Cards.Count; ++i)
        {
          //for each card, check if its already mentioned in the database
          bool found = false;
          IList cards = _ourServer.ReferringCard();
          foreach (Card card in cards)
          {
            if (card.DevicePath == _localCardCollection.Cards[i].DevicePath)
            {
              found = true;
              break;
            }
          }
          if (!found)
          {
            // card is not yet in the database, so add it
            Log.WriteFile("Controller: add card:{0}", _localCardCollection.Cards[i].Name);
            layer.AddCard(_localCardCollection.Cards[i].Name, _localCardCollection.Cards[i].DevicePath, _ourServer);
          }
        }

        //notify log about cards from the database which are removed from the pc
        IList cardsInDbs = Card.ListAll();
        int cardsInstalled = _localCardCollection.Cards.Count;
        foreach (Card dbsCard in cardsInDbs)
        {
          if (dbsCard.ReferencedServer().IdServer == _ourServer.IdServer)
          {
            bool found = false;
            for (int cardNumber = 0; cardNumber < cardsInstalled; ++cardNumber)
            {
              if (dbsCard.DevicePath == _localCardCollection.Cards[cardNumber].DevicePath)
              {
                found = true;
                break;
              }
            }
            if (!found)
            {
              Log.WriteFile("Controller: card not found :{0}", dbsCard.Name);

              for (int i = 0; i < _localCardCollection.Cards.Count; ++i)
              {
                if (_localCardCollection.Cards[i].DevicePath == dbsCard.DevicePath)
                {
                  _localCardCollection.Cards[i].CardPresent = false;
                  break;
                }
              }

            }
          }
        }

        localcards = new Dictionary<int, ITVCard>();


        cardsInDbs = Card.ListAll();
        foreach (Card card in cardsInDbs)
        {
          if (IsLocal(card.ReferencedServer().HostName))
          {
            for (int x = 0; x < _localCardCollection.Cards.Count; ++x)
            {
              if (_localCardCollection.Cards[x].DevicePath == card.DevicePath)
              {
                localcards[card.IdCard] = _localCardCollection.Cards[x];
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
        foreach (Card dbsCard in cardsInDbs)
        {
          ITVCard card = null;
          if (localcards.ContainsKey(dbsCard.IdCard))
          {
            card = localcards[dbsCard.IdCard];
            TvCardHandler tvcard = new TvCardHandler(dbsCard, card);
            _cards[dbsCard.IdCard] = tvcard;
          }

          // remove any old timeshifting TS files	
          try
          {
            string[] files = Directory.GetFiles(dbsCard.TimeShiftFolder);

            foreach (string file in files)
            {
              FileInfo fInfo = new FileInfo(file);
              bool delFile = (fInfo.Extension.ToLower().IndexOf(".tsbuffer") == 0);

              if (!delFile)
              {
                delFile = (fInfo.Extension.ToLower().IndexOf(".ts") == 0) && (fInfo.Name.ToLower().IndexOf("tsbuffer") > 0);
              }
              if (delFile) File.Delete(fInfo.FullName);
            }
          }
          catch (Exception)
          {
            //ignore any errors encountered
          }


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

        // start plugins
        foreach (ITvServerPlugin plugin in _plugins.Plugins)
        {
          if (plugin.MasterOnly == false || _isMaster)
          {
            Setting setting = layer.GetSetting(String.Format("plugin{0}", plugin.Name), "false");
            if (setting.Value == "true")
            {
              Log.Info("Plugin:{0} started", plugin.Name);
              try
              {
                plugin.Start(this);
                _pluginsStarted.Add(plugin);
              }
              catch (Exception ex)
              {
                Log.Info("Plugin:{0} failed to start", plugin.Name);
                Log.Write(ex);
              }
            }
            else
            {
              Log.Info("Plugin:{0} disabled", plugin.Name);
            }
          }
        }

        // fire off startedAll on plugins
        foreach (ITvServerPlugin plugin in _pluginsStarted)
        {
          if (plugin is ITvServerPluginStartedAll)
          {
            Log.Info("Plugin:{0} started all", plugin.Name);
            try
            {
              (plugin as ITvServerPluginStartedAll).StartedAll();
            }
            catch (Exception ex)
            {
              Log.Info("Plugin:{0} failed to startedAll", plugin.Name);
              Log.Write(ex);
            }
          }
        }

        // setup heartbeat monitoring thread.
        // useful for kicking idle/dead clients.
        Log.WriteFile("Controller: setup HeartBeat Monitor");

        //stop thread, just incase it is running.
        if (heartBeatMonitorThread != null)
        {
          if (heartBeatMonitorThread.IsAlive)
          {
            heartBeatMonitorThread.Abort();
          }
        }

        heartBeatMonitorThread = new Thread(HeartBeatMonitor);
        heartBeatMonitorThread.Start();

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
      Log.Info("Controller: DeInit.");
      try
      {
        if (heartBeatMonitorThread != null)
        {
          if (heartBeatMonitorThread.IsAlive)
          {
            Log.Info("Controller: HeartBeat monitor stopped...");
            heartBeatMonitorThread.Abort();
          }
        }

        if (_pluginsStarted != null)
        {
          foreach (ITvServerPlugin plugin in _pluginsStarted)
          {
            try
            {
              plugin.Stop();
            }
            catch (Exception ex)
            {
              Log.Error("Controller: plugin:{0} failed to stop...", plugin.Name);
              Log.Write(ex);
            }
          }
          _pluginsStarted = new List<ITvServerPlugin>();
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
        FreeCards();

        Gentle.Common.CacheManager.Clear();
        if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
        {
          GlobalServiceProvider.Instance.Remove<ITvServerEvent>();
        }
      }
      catch (Exception ex)
      {
        Log.Error("TvController:Deinit() failed");
        Log.Write(ex);
      }
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
      if (cards != null && cards.Count > cardIndex)
      {
        return ((Card)cards[cardIndex]).IdCard;
      }
      else
      {
        return -1;
      }
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
      if (!_cards.ContainsKey(cardId)) return CardType.Unknown;
      return _cards[cardId].Type;
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <returns>name of card</returns>
    public string CardName(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return "";
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
      if (!_cards.ContainsKey(cardId)) return false;
      return _cards[cardId].Tuner.CanTune(channel);
    }

    /// <summary>
    /// Method to check if card is currently present and detected
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>		
    public bool CardPresent(int cardId)
    {
      string devicePath = "";
      IList cards = _ourServer.ReferringCard();
      if (!_cards.ContainsKey(cardId)) return false;
      foreach (Card card in cards)
      {
        if (card.IdCard == cardId)
        {
          devicePath = card.DevicePath;
          break;
        }
      }
      if (devicePath.Length > 0)
      {
        for (int i = 0; i < _localCardCollection.Cards.Count; i++)
        {
          if (_localCardCollection.Cards[i].DevicePath == devicePath)
          {
            return _localCardCollection.Cards[i].CardPresent;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>device of card</returns>
    public string CardDevice(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return "";
      return _cards[cardId].CardDevice();
    }

    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel(ref User user)
    {
      if (user.CardId < 0) return null;
      if (!_cards.ContainsKey(user.CardId)) return null;
      return _cards[user.CardId].CurrentChannel(ref user);
    }
    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>id of database channel</returns>
    public int CurrentDbChannel(ref User user)
    {
      if (user.CardId < 0) return -1;
      if (!_cards.ContainsKey(user.CardId)) return -1;
      return _cards[user.CardId].CurrentDbChannel(ref user);
    }

    /// <summary>
    /// Gets the current channel name.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>channel</returns>
    public string CurrentChannelName(ref User user)
    {
      if (user.CardId < 0) return "";
      if (!_cards.ContainsKey(user.CardId)) return "";
      return _cards[user.CardId].CurrentChannelName(ref user);
    }


    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    public bool TunerLocked(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return true;
      return _cards[cardId].TunerLocked;
    }

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    public int SignalQuality(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return -1;
      return _cards[cardId].SignalQuality;
    }

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal level (0-100)</returns>
    public int SignalLevel(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return -1;
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
    public string RecordingFileName(ref User user)
    {
      if (user.CardId < 0) return "";
      if (!_cards.ContainsKey(user.CardId)) return "";
      return _cards[user.CardId].Recorder.FileName(ref user);
    }

    public string TimeShiftFileName(ref User user)
    {
      if (user.CardId < 0) return "";
      if (!_cards.ContainsKey(user.CardId)) return "";
      return _cards[user.CardId].TimeShifter.FileName(ref user);
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(ref User user)
    {
      if (user.CardId < 0) return false;
      if (!_cards.ContainsKey(user.CardId)) return false;
      return _cards[user.CardId].TimeShifter.IsTimeShifting(ref user);
    }

    /// <summary>
    /// Returns the video stream currently associated with the card. 
    /// </summary>
    /// <returns>stream_type</returns>
    public int GetCurrentVideoStream(User user)
    {
      if (user.CardId < 0) return -1;
      if (!_cards.ContainsKey(user.CardId)) return -1;
      return _cards[user.CardId].GetCurrentVideoStream(user);
    }

    /// <summary>
    /// Determines if any card is currently busy recording
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if a card is recording; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAnyCardRecording()
    {
      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      while (en.MoveNext())
      {
        ITvCardHandler card = en.Current.Value;
        User user = new User();
        user.CardId = card.DataBaseCard.IdCard;
        if (card.Recorder.IsAnySubChannelRecording)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Determines whether the specified channel name is recording.
    /// </summary>
    /// <param name="channelName">Name of the channel.</param>
    /// <param name="vcard">The vcard.</param>
    /// <returns>
    /// 	<c>true</c> if the specified channel name is recording; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRecording(string channelName, out VirtualCard card)
    {
      card = null;
      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      while (en.MoveNext())
      {
        ITvCardHandler tvcard = en.Current.Value;
        User[] users = tvcard.Users.GetUsers();
        if (users == null) continue;
        if (users.Length == 0) continue;
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (tvcard.CurrentChannelName(ref user) == null) continue;
          if (tvcard.CurrentChannelName(ref user) == channelName)
          {
            if (tvcard.Recorder.IsRecording(ref user))
            {
              card = GetVirtualCard(user);
              return true;
            }
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is recording otherwise false</returns>
    public bool IsRecording(ref User user)
    {
      if (user.CardId < 0) return false;
      if (!_cards.ContainsKey(user.CardId)) return false;
      return _cards[user.CardId].Recorder.IsRecording(ref user);
    }

    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    public bool IsScanning(int cardId)
    {
      if (cardId < 0) return false;
      if (!_cards.ContainsKey(cardId)) return false;
      return _cards[cardId].Scanner.IsScanning;
    }

    /// <summary>
    /// Returns if the card is grabbing the epg or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    public bool IsGrabbingEpg(int cardId)
    {
      if (cardId < 0) return false;
      if (!_cards.ContainsKey(cardId)) return false;
      return _cards[cardId].Epg.IsGrabbing;
    }

    /// <summary>
    /// Returns if the card is grabbing teletext or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing teletext otherwise false</returns>
    public bool IsGrabbingTeletext(User user)
    {
      if (user.CardId < 0) return false;
      if (!_cards.ContainsKey(user.CardId)) return false;
      return _cards[user.CardId].Teletext.IsGrabbingTeletext(user);
    }

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    public bool HasTeletext(User user)
    {
      if (user.CardId < 0) return false;
      if (!_cards.ContainsKey(user.CardId)) return false;
      return _cards[user.CardId].Teletext.HasTeletext(user);
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(User user, int pageNumber)
    {
      if (user.CardId < 0) return new TimeSpan(0, 0, 15);
      if (!_cards.ContainsKey(user.CardId)) return new TimeSpan(0, 0, 15);
      return _cards[user.CardId].Teletext.TeletextRotation(user, pageNumber);
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime TimeShiftStarted(User user)
    {
      if (user.CardId < 0) return DateTime.MinValue;
      if (!_cards.ContainsKey(user.CardId)) return DateTime.MinValue;
      return _cards[user.CardId].TimeShifter.TimeShiftStarted(user);
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted(User user)
    {
      if (user.CardId < 0) return DateTime.MinValue;
      if (!_cards.ContainsKey(user.CardId)) return DateTime.MinValue;
      return _cards[user.CardId].Recorder.RecordingStarted(user);
    }

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    public bool IsScrambled(ref User user)
    {
      if (user.CardId < 0) return false;
      if (!_cards.ContainsKey(user.CardId)) return false;
      return _cards[user.CardId].IsScrambled(ref user);
    }

    /// <summary>
    /// returns the min/max channel numbers for analog cards
    /// </summary>
    public int MinChannel(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return 0;
      return _cards[cardId].MinChannel;
    }

    public int MaxChannel(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return 0;
      return _cards[cardId].MaxChannel;
    }

    /// <summary>
    /// Gets the number of channels decrypting.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return 0;
      return _cards[cardId].NumberOfChannelsDecrypting;
    }
    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    public TvResult Tune(ref User user, IChannel channel, int idChannel)
    {
      try
      {
        if (user == null) return TvResult.UnknownError;
        if (channel == null) return TvResult.UnknownError;
        if (user.CardId < 0) return TvResult.CardIsDisabled;
        int cardId = user.CardId;
        if (_cards[cardId].DataBaseCard.Enabled == false) return TvResult.CardIsDisabled;
        if (!CardPresent(cardId)) return TvResult.CardIsDisabled;
        RemoveUserFromOtherCards(cardId, user);
        Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(user), user, channel));
        return _cards[cardId].Tuner.Tune(ref user, channel, idChannel);
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(user), user, channel));
      }
    }

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    public void GrabTeletext(User user, bool onOff)
    {
      if (user.CardId < 0) return;
      if (!_cards.ContainsKey(user.CardId)) return;
      _cards[user.CardId].Teletext.GrabTeletext(user, onOff);
    }

    /// <summary>
    /// Gets the teletext page.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    public byte[] GetTeletextPage(User user, int pageNumber, int subPageNumber)
    {
      if (user.CardId < 0) return new byte[] { 1 };
      if (!_cards.ContainsKey(user.CardId)) return new byte[] { 1 };
      return _cards[user.CardId].Teletext.GetTeletextPage(user, pageNumber, subPageNumber);
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    public int SubPageCount(User user, int pageNumber)
    {
      if (user.CardId < 0) return -1;
      if (!_cards.ContainsKey(user.CardId)) return -1;
      return _cards[user.CardId].Teletext.SubPageCount(user, pageNumber);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the red button</returns>
    public int GetTeletextRedPageNumber(User user)
    {
      if (user.CardId < 0) return -1;
      if (!_cards.ContainsKey(user.CardId)) return -1;
      return _cards[user.CardId].Teletext.GetTeletextRedPageNumber(user);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the green button</returns>
    public int GetTeletextGreenPageNumber(User user)
    {
      if (user.CardId < 0) return -1;
      if (!_cards.ContainsKey(user.CardId)) return -1;
      return _cards[user.CardId].Teletext.GetTeletextGreenPageNumber(user);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    public int GetTeletextYellowPageNumber(User user)
    {
      if (user.CardId < 0) return -1;
      if (!_cards.ContainsKey(user.CardId)) return -1;
      return _cards[user.CardId].Teletext.GetTeletextYellowPageNumber(user);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the blue button</returns>
    public int GetTeletextBluePageNumber(User user)
    {
      if (user.CardId < 0) return -1;
      if (!_cards.ContainsKey(user.CardId)) return -1;
      return _cards[user.CardId].Teletext.GetTeletextBluePageNumber(user);
    }

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref User user, ref string fileName)
    {
      try
      {
        int cardId = user.CardId;
        if (false == _cards[cardId].IsLocal)
        {
          try
          {
            RemoteControl.HostName = _cards[cardId].DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.StartTimeShifting(ref user, ref fileName);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _cards[cardId].DataBaseCard.ReferencedServer().HostName);
            return TvResult.UnknownError;
          }
        }

        Fire(this, new TvServerEventArgs(TvServerEventType.StartTimeShifting, GetVirtualCard(user), user));
        if (_epgGrabber != null)
        {
          _epgGrabber.Stop();
        }

        bool isTimeShifting = _cards[cardId].TimeShifter.IsTimeShifting(ref user);
        TvResult result = _cards[cardId].TimeShifter.Start(ref user, ref fileName);
        if (result == TvResult.Succeeded)
        {
          if (!isTimeShifting)
          {
            Log.Info("user:{0} card:{1} sub:{2} add stream:{3}", user.Name, user.CardId, user.SubChannel, fileName);
            if (System.IO.File.Exists(fileName))
            {
              _streamer.Start();
              RtspStream stream = new RtspStream(String.Format("stream{0}.{1}", cardId, user.SubChannel), fileName, _cards[cardId].Card);
              _streamer.AddStream(stream);
            }
            else
            {
              Log.Write("Controller: streaming: file not found:{0}", fileName);
            }
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

    public void StopCard(User user)
    {
      if (user.CardId < 0) return;
      if (!_cards.ContainsKey(user.CardId)) return;
      _cards[user.CardId].StopCard(user);
    }

    public bool StopTimeShifting(ref User user, TvStoppedReason reason)
    {
      int cardId = user.CardId;
      if (cardId > 0)
      {
        _cards[cardId].Users.SetTvStoppedReason(user, reason);
      }
      return this.StopTimeShifting(ref user);
    }

    public TvStoppedReason GetTvStoppedReason(User user)
    {
      int cardId = user.CardId;
      if (cardId < 0) return TvStoppedReason.UnknownReason;

      try
      {
        if (_cards[cardId].DataBaseCard.Enabled == false) return TvStoppedReason.UnknownReason;
        if (!CardPresent(cardId)) return TvStoppedReason.UnknownReason;

        return _cards[cardId].Users.GetTvStoppedReason(user);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return TvStoppedReason.UnknownReason;
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool StopTimeShifting(ref User user)
    {
      int cardId = user.CardId;
      if (cardId < 0) return false;
      if (!_cards.ContainsKey(cardId)) return false;
      try
      {
        if (_cards[cardId].DataBaseCard.Enabled == false) return true;
        if (!CardPresent(cardId)) return true;

        if (false == _cards[cardId].IsLocal)
        {
          try
          {
            if (this.IsGrabbingEpg(cardId))
            {
              _epgGrabber.Stop(); // we need this, otherwise tvservice will hang in the event stoptimeshifting is called by heartbeat timeout function
            }
            RemoteControl.HostName = _cards[cardId].DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.StopTimeShifting(ref user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}", _cards[cardId].DataBaseCard.ReferencedServer().HostName);
            return false;
          }
        }
        HybridCard hybridCard = _cards[cardId].Card as HybridCard;
        if (hybridCard != null)
        {
          if (!hybridCard.IsCardIdActive(cardId))
          {
            return true;
          }
        }

        if (false == _cards[cardId].TimeShifter.IsTimeShifting(ref user)) return true;
        Fire(this, new TvServerEventArgs(TvServerEventType.EndTimeShifting, GetVirtualCard(user), user));

        if (_cards[cardId].Recorder.IsRecording(ref user)) return true;

        Log.Write("Controller: StopTimeShifting {0}", cardId);
        lock (this)
        {
          if (this.IsGrabbingEpg(cardId))
          {
            _epgGrabber.Stop(); // we need this, otherwise tvservice will hang in the event stoptimeshifting is called by heartbeat timeout function
          }
          bool result = false;
          int subChannel = user.SubChannel;
          if (_cards[cardId].TimeShifter.Stop(ref user))
          {
            result = true;
            Log.Write("Controller:Timeshifting stopped on card:{0}", cardId);
            _streamer.Remove(String.Format("stream{0}.{1}", cardId, subChannel));
          }

          bool allStopped = true;
          Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();
          while (enumerator.MoveNext())
          {
            KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
            if (keyPair.Value.IsLocal)
            {
              if (keyPair.Value.IsIdle == false)
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
    public bool StartRecording(ref User user, ref string fileName, bool contentRecording, long startTime)
    {
      if (user.CardId < 0) return false;
      if (!_cards.ContainsKey(user.CardId)) return false;
      return _cards[user.CardId].Recorder.Start(ref user, ref  fileName, contentRecording, startTime);
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool StopRecording(ref User user)
    {
      if (user.CardId < 0) return false;
      if (!_cards.ContainsKey(user.CardId)) return false;
      return _cards[user.CardId].Recorder.Stop(ref user);
    }

    /// <summary>
    /// scans current transponder for more channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="cardId">IChannel containing the transponder tuning details.</param>
    /// <returns>list of channels found</returns>
    public IChannel[] Scan(int cardId, IChannel channel)
    {
      ScanParameters settings = new ScanParameters();
      TvBusinessLayer layer = new TvBusinessLayer();
      settings.TimeOutTune = Int32.Parse(layer.GetSetting("timeoutTune", "2").Value);
      settings.TimeOutPAT = Int32.Parse(layer.GetSetting("timeoutPAT", "5").Value);
      settings.TimeOutCAT = Int32.Parse(layer.GetSetting("timeoutCAT", "5").Value);
      settings.TimeOutPMT = Int32.Parse(layer.GetSetting("timeoutPMT", "10").Value);
      settings.TimeOutSDT = Int32.Parse(layer.GetSetting("timeoutSDT", "20").Value);
      return _cards[cardId].Scanner.Scan(channel, settings);
    }

    public IChannel[] ScanNIT(int cardId, IChannel channel)
    {
      ScanParameters settings = new ScanParameters();
      TvBusinessLayer layer = new TvBusinessLayer();
      settings.TimeOutTune = Int32.Parse(layer.GetSetting("timeoutTune", "2").Value);
      settings.TimeOutPAT = Int32.Parse(layer.GetSetting("timeoutPAT", "5").Value);
      settings.TimeOutCAT = Int32.Parse(layer.GetSetting("timeoutCAT", "5").Value);
      settings.TimeOutPMT = Int32.Parse(layer.GetSetting("timeoutPMT", "10").Value);
      settings.TimeOutSDT = Int32.Parse(layer.GetSetting("timeoutSDT", "20").Value);
      return _cards[cardId].Scanner.ScanNIT(channel, settings);
    }

    /// <summary>
    /// grabs the epg.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool GrabEpg(BaseEpgGrabber grabber, int cardId)
    {
      if (cardId < 0) return false;
      if (!_cards.ContainsKey(cardId)) return false;
      return _cards[cardId].Epg.Start(grabber);
    }

    /// <summary>
    /// Aborts grabbing the epg. This also triggers the OnEpgReceived callback.
    /// </summary>
    public void AbortEPGGrabbing(int cardId)
    {
      if (cardId < 0) return;
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].Epg.Abort();
    }

    /// <summary>
    /// Epgs the specified card id.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    public List<EpgChannel> Epg(int cardId)
    {
      if (cardId < 0) return new List<EpgChannel>();
      if (!_cards.ContainsKey(cardId)) return new List<EpgChannel>();
      return _cards[cardId].Epg.Epg;
    }

    /// <summary>
    /// Deletes the recording from database and disk
    /// </summary>
    /// <param name="idRecording">The id recording.</param>
    public void DeleteRecording(int idRecording)
    {
      try
      {
        Recording rec = Recording.Retrieve(idRecording);
        if (rec == null) return;

        if (!IsLocal(rec.ReferencedServer().HostName))
        {
          try
          {
            RemoteControl.HostName = rec.ReferencedServer().HostName;
            RemoteControl.Instance.DeleteRecording(rec.IdRecording);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", rec.ReferencedServer().HostName);
          }
          return;
        }

        if (System.IO.File.Exists(rec.FileName))
        {
          try
          {
            _streamer.RemoveFile(rec.FileName);
            //Delete the matroska tag info xml file 
            if (File.Exists(Path.ChangeExtension(rec.FileName, ".xml")))
              File.Delete(Path.ChangeExtension(rec.FileName, ".xml"));
            // if a recording got interrupted there may be files like <recording name>_1.mpg, etc
            string SearchFile = System.IO.Path.GetFileNameWithoutExtension(rec.FileName) + @"*";
            // check only the ending for underscores as a user might have a naming pattern including them between e.g. station and program title
            string SubSearch = SearchFile.Substring((SearchFile.Length - 3));
            int UnderScorePosition = SubSearch.LastIndexOf(@"_");
            if (UnderScorePosition != -1)
              // Length - 3 should be enough since there won't be thousands of files with the same name..
              SearchFile = SearchFile.Substring(0, SearchFile.Length - 3) + @"*";
            string[] allRecordingFiles = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(rec.FileName), SearchFile);
            Log.Debug("Controller: found {0} file(s) to delete for recording {1}", Convert.ToString(allRecordingFiles.Length), SearchFile);
            foreach (string recPartPath in allRecordingFiles)
            {
              System.IO.File.Delete(recPartPath);
            }
            CleanRecordingFolders(rec.FileName);
            rec.Delete();
          }
          catch (Exception)
          {
          }
        }
        else
        {
          rec.Delete();
        }
      }
      catch (Exception)
      {
      }
    }
    /// <summary>
    /// Checks if the files of a recording still exist
    /// </summary>
    /// <param name="idRecording">The id of the recording</param>
    public bool IsRecordingValid(int idRecording)
    {
      try
      {
        Recording rec = Recording.Retrieve(idRecording);
        if (rec == null) return false;
        if (!IsLocal(rec.ReferencedServer().HostName))
        {
          try
          {
            RemoteControl.HostName = rec.ReferencedServer().HostName;
            return RemoteControl.Instance.IsRecordingValid(rec.IdRecording);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", rec.ReferencedServer().HostName);
          }
          return true;
        }
        return (System.IO.File.Exists(rec.FileName));
      }
      catch (Exception)
      {
        return true;
      }
    }

    /// <summary>
    /// returns which schedule the card specified is currently recording
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <returns>
    /// id of Schedule or -1 if  card not recording
    /// </returns>
    public int GetRecordingSchedule(int cardId, int ChannelId)
    {
      try
      {
        if (_isMaster == false) return -1;
        if (_cards[cardId].DataBaseCard.Enabled == false) return -1;
        if (!CardPresent(cardId)) return -1;
        return _scheduler.GetRecordingScheduleForCard(cardId, ChannelId);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    #region audio streams
    public IAudioStream[] AvailableAudioStreams(User user)
    {
      if (user.CardId < 0) return null;
      if (!_cards.ContainsKey(user.CardId)) return null;
      return _cards[user.CardId].Audio.Streams(user);
    }

    public IAudioStream GetCurrentAudioStream(User user)
    {
      if (user.CardId < 0) return null;
      if (!_cards.ContainsKey(user.CardId)) return null;
      return _cards[user.CardId].Audio.GetCurrent(user);
    }

    public void SetCurrentAudioStream(User user, IAudioStream stream)
    {
      if (user.CardId < 0) return;
      if (!_cards.ContainsKey(user.CardId)) return;
      _cards[user.CardId].Audio.Set(user, stream);
    }

    public string GetStreamingUrl(User user)
    {
      try
      {
        int cardId = user.CardId;
        if (cardId < 0) return "";
        if (!_cards.ContainsKey(cardId)) return "";

        if (_cards[cardId].DataBaseCard.Enabled == false) return "";
        if (!CardPresent(cardId)) return "";
        if (IsLocal(cardId) == false)
        {
          try
          {
            RemoteControl.HostName = _cards[cardId].DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.GetStreamingUrl(user);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _cards[cardId].DataBaseCard.ReferencedServer().HostName);
            return "";
          }
        }
        return String.Format("rtsp://{0}/stream{1}.{2}", _ourServer.HostName, cardId, user.SubChannel);
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
            string streamName = String.Format("{0:X}", recording.FileName.GetHashCode());
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

    /// <summary>
    /// Gets the rtsp URL for file located on the tvserver.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>rtsp url</returns>
    public string GetUrlForFile(string fileName)
    {
      if (System.IO.File.Exists(fileName))
      {
        _streamer.Start();
        string streamName = String.Format("{0:X}", fileName.GetHashCode());
        RtspStream stream = new RtspStream(streamName, fileName, streamName);
        _streamer.AddStream(stream);
        string url = String.Format("rtsp://{0}/{1}", _ourServer.HostName, streamName);
        Log.WriteFile("Controller: streaming url:{0} file:{1}", url, fileName);
        return url;
      }
      return "";
    }
    #endregion

    #endregion

    #region public interface
    /// <summary>
    /// Frees all resources occupied by the TV cards
    /// </summary>
    public void FreeCards()
    {
      Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();
      while (enumerator.MoveNext())
      {
        KeyValuePair<int, ITvCardHandler> key = enumerator.Current;
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
    }

    /// <summary>
    /// Query what card would be used for timeshifting on any given channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>    
    /// <returns>
    /// CardDetail which would be used when doing the actual timeshifting.
    /// </returns>
    public int TimeShiftingWouldUseCard(ref User user, int idChannel)
    {
      CardDetail cardDetail;
      Channel channel = Channel.Retrieve(idChannel);
      Log.Write("Controller: TimeShiftingWouldUseCard {0} {1}", channel.DisplayName, channel.IdChannel);
      TvResult result;

      try
      {
        ICardAllocation allocation = CardAllocationFactory.Create(false);
        List<CardDetail> freeCards = allocation.GetAvailableCardsForChannel(_cards, channel, ref user, true, out result);
        if (freeCards.Count == 0)
        {
          // enumerate all cards and check if some card is already timeshifting the channel requested
          Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();

          //for each card
          while (enumerator.MoveNext())
          {
            KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
            //get a list of all users for this card
            User[] users = keyPair.Value.Users.GetUsers();
            if (users != null)
            {
              //for each user
              for (int i = 0; i < users.Length; ++i)
              {
                User tmpUser = users[i];
                //is user timeshifting?
                if (keyPair.Value.TimeShifter.IsTimeShifting(ref tmpUser))
                {
                  //yes, is user timeshifting the correct channel
                  if (keyPair.Value.CurrentDbChannel(ref tmpUser) == channel.IdChannel)
                  {
                    //yes, if card does not support subchannels (analog cards)
                    //then assign user to this card
                    VirtualCard card = GetVirtualCard(tmpUser);
                    return card.Id;
                  }
                }
              }
            }
          }
        }
        else
        {
          //get first free card
          return freeCards[0].Id;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
      return -1;
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref User user, int idChannel, out VirtualCard card)
    {
      Channel channel = Channel.Retrieve(idChannel);
      Log.Write("Controller: StartTimeShifting {0} {1}", channel.DisplayName, channel.IdChannel);
      card = null;
      TvResult result;
      if (_epgGrabber != null)
      {
        _epgGrabber.Stop();
      }
      try
      {
        ICardAllocation allocation = CardAllocationFactory.Create(false);
        List<CardDetail> freeCards = allocation.GetAvailableCardsForChannel(_cards, channel, ref user, true, out result);
        if (freeCards.Count == 0)
        {
          // enumerate all cards and check if some card is already timeshifting the channel requested
          Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();

          //for each card
          while (enumerator.MoveNext())
          {
            KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
            //get a list of all users for this card
            User[] users = keyPair.Value.Users.GetUsers();
            if (users != null)
            {
              //for each user
              for (int i = 0; i < users.Length; ++i)
              {
                User tmpUser = users[i];
                //is user timeshifting?
                if (keyPair.Value.TimeShifter.IsTimeShifting(ref tmpUser))
                {
                  //yes, is user timeshifting the correct channel
                  if (keyPair.Value.CurrentDbChannel(ref tmpUser) == channel.IdChannel)
                  {
                    //yes, if card does not support subchannels (analog cards)
                    //then assign user to this card
                    card = GetVirtualCard(tmpUser);
                    return TvResult.Succeeded;
                  }
                }
              }
            }
          }
        }

        if (freeCards.Count == 0)
        {
          //no free cards available
          Log.Write("Controller: StartTimeShifting failed:{0}", result);

          if (_epgGrabber != null)
          {
            _epgGrabber.Start();
          }
          return result;
        }

        //get first free card
        CardDetail cardInfo = freeCards[0];
        user.CardId = cardInfo.Id;
        IChannel tuneChannel = cardInfo.TuningDetail;

        //setup folders
        if (cardInfo.Card.RecordingFolder == String.Empty)
          cardInfo.Card.RecordingFolder = System.IO.Directory.GetCurrentDirectory();
        if (cardInfo.Card.TimeShiftFolder == String.Empty)
          cardInfo.Card.TimeShiftFolder = System.IO.Directory.GetCurrentDirectory();

        //tune to the new channel
        result = CardTune(ref user, tuneChannel, channel);
        if (result != TvResult.Succeeded)
        {
          if (_epgGrabber != null)
          {
            _epgGrabber.Start();
          }
          return result;
        }
        Log.Info("control2:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);
        if (!IsTimeShifting(ref user))
        {
          CleanTimeShiftFiles(cardInfo.Card.TimeShiftFolder, String.Format("live{0}-{1}.ts", user.CardId, user.SubChannel));
        }
        string timeshiftFileName = String.Format(@"{0}\live{1}-{2}.ts", cardInfo.Card.TimeShiftFolder, user.CardId, user.SubChannel);

        //start timeshifting
        result = StartTimeShifting(ref user, ref timeshiftFileName);
        if (result != TvResult.Succeeded)
        {
          if (_epgGrabber != null)
          {
            _epgGrabber.Start();
          }
          return result;
        }
        Log.Write("Controller: StartTimeShifting started on card:{0} to {1}", user.CardId, timeshiftFileName);
        card = GetVirtualCard(user);
        return TvResult.Succeeded;
      }
      catch (Exception ex)
      {
        if (_epgGrabber != null)
        {
          _epgGrabber.Start();
        }
        Log.Write(ex);
        return TvResult.UnknownError;
      }
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
        Log.Info("IsRecordingSchedule:{0} {1}", idSchedule, _isMaster);
        if (_isMaster == false) return false;
        if (!_scheduler.IsRecordingSchedule(idSchedule, out card))
        {
          Log.Info("IsRecordingSchedule: scheduler is not recording schedule");
          return false;
        }
        Log.Info("IsRecordingSchedule: scheduler is recording schedule on cardid:{0}", card.Id);

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
    /// This method should be called by a client to indicate that
    /// there is a new or modified Schedule in the database
    /// this override allows to pass a custom TvServerEventArgs instance
    /// </summary>
    public void OnNewSchedule(EventArgs args)
    {
      try
      {
        if (_scheduler != null)
        {
          _scheduler.ResetTimer();
        }
        TvServerEventArgs tvargs = (TvServerEventArgs)args;
        Fire(this, new TvServerEventArgs(TvServerEventType.ScheduledAdded, tvargs.Schedules, tvargs.Conflicts, tvargs.ArgsUpdatedState));

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
            if (_epgGrabber != null)
            {
              TvBusinessLayer layer = new TvBusinessLayer();
              if (layer.GetSetting("idleEPGGrabberEnabled", "yes").Value == "yes")
              {
                Log.Write("Controller: epg start");
                _epgGrabber.Start();
              }
            }
          }
          else
          {
            if (_epgGrabber != null)
            {
              TvBusinessLayer layer = new TvBusinessLayer();
              if (layer.GetSetting("idleEPGGrabberEnabled", "yes").Value == "yes")
              {
                Log.Write("Controller: epg stop");
                _epgGrabber.Stop();
              }
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
      catch (Exception ex)
      {
        Log.Write(ex);
        return;
      }
    }

    /// <summary>
    /// Returns the SQl connection string to the database
    /// </summary>
    public void GetDatabaseConnectionString(out string connectionString, out string provider)
    {
      connectionString = "";
      provider = "";
      try
      {
        string fname = String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        try
        {
          System.IO.File.Copy(fname, "gentle.config", true);
        }
        catch (Exception) { }
        XmlDocument doc = new XmlDocument();
        doc.Load(String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode nodeConnection = nodeKey.Attributes.GetNamedItem("connectionString"); ;
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name"); ;
        connectionString = nodeConnection.InnerText;
        provider = nodeProvider.InnerText;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    public void SetDatabaseConnectionString(string connectionString, string provider)
    {
      try
      {

        XmlDocument doc = new XmlDocument();
        doc.Load(String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode nodeConnection = nodeKey.Attributes.GetNamedItem("connectionString"); ;
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
        nodeProvider.InnerText = connectionString;
        nodeConnection.InnerText = provider;
        doc.Save(String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));

        string fname = String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        try
        {
          System.IO.File.Copy(fname, "gentle.config", true);
        }
        catch (Exception) { }
        Gentle.Framework.ProviderFactory.ResetGentle(true);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);
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
    /// Gets a value indicating whether all cards are idle.
    /// </summary>
    /// <value><c>true</c> if [all cards idle]; otherwise, <c>false</c>.</value>
    public bool AllCardsIdle
    {
      get
      {
        Dictionary<int, ITvCardHandler>.Enumerator enumer = _cards.GetEnumerator();
        while (enumer.MoveNext())
        {
          int cardId = enumer.Current.Key;
          if (_cards[cardId].IsIdle == false) return false;
        }
        return true;
      }
    }

    #region DiSEqC

    public void DiSEqCGetPosition(int cardId, out int satellitePosition, out int stepsAzimuth, out int stepsElevation)
    {
      if (!_cards.ContainsKey(cardId))
      {
        satellitePosition = -1;
        stepsAzimuth = -1;
        stepsElevation = -1;
        return;
      }
      _cards[cardId].DisEqC.GetPosition(out  satellitePosition, out  stepsAzimuth, out  stepsElevation);
    }

    public void DiSEqCReset(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].DisEqC.Reset();
    }

    public void DiSEqCStopMotor(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].DisEqC.StopMotor();
    }

    public void DiSEqCSetEastLimit(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].DisEqC.SetEastLimit();
    }

    public void DiSEqCSetWestLimit(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].DisEqC.SetWestLimit();
    }

    public void DiSEqCForceLimit(int cardId, bool onOff)
    {
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].DisEqC.EnableEastWestLimits(onOff);
    }

    public void DiSEqCDriveMotor(int cardId, DiSEqCDirection direction, byte numberOfSteps)
    {
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].DisEqC.DriveMotor(direction, numberOfSteps);
    }

    public void DiSEqCStorePosition(int cardId, byte position)
    {
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].DisEqC.StoreCurrentPosition(position);
    }

    public void DiSEqCGotoReferencePosition(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].DisEqC.GotoReferencePosition();
    }

    public void DiSEqCGotoPosition(int cardId, byte position)
    {
      if (!_cards.ContainsKey(cardId)) return;
      _cards[cardId].DisEqC.GotoStoredPosition(position);
    }
    #endregion

    /// <summary>
    /// Stops the grabbing epg.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    public void StopGrabbingEpg(User user)
    {
      if (user.CardId < 0) return;
      if (!_cards.ContainsKey(user.CardId)) return;
      _cards[user.CardId].Epg.Stop(user);
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

    public User[] GetUsersForCard(int cardId)
    {
      if (!_cards.ContainsKey(cardId)) return null;
      return _cards[cardId].Users.GetUsers();
    }

    /// <summary>
    /// Indicates if we're the master server or not
    /// </summary>
    public bool IsMaster
    {
      get { return _isMaster; }
    }

    /// <summary>
    /// Fetches all channels with backbuffer
    /// </summary>
    /// <param name="currentRecChannels"></param>
    /// <param name="currentTSChannels"></param>
    public void GetAllRecordingChannels(out List<int> currentRecChannels, out List<int> currentTSChannels)
    {
      currentRecChannels = new List<int>();
      currentTSChannels = new List<int>();
      Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();

      while (enumerator.MoveNext())
      {
        KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
        ITvCardHandler tvcard = keyPair.Value;
        User[] users = tvcard.Users.GetUsers();
        string tmpChannel = string.Empty;

        if (users == null || users.Length == 0)
          continue;

        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          tmpChannel = tvcard.CurrentChannelName(ref user);
          if (tmpChannel == null || tmpChannel == string.Empty)
            continue;
          else
          {
            if (tvcard.Recorder.IsRecording(ref user))
              currentRecChannels.Add(tvcard.CurrentDbChannel(ref user));
            else
              if (tvcard.TimeShifter.IsTimeShifting(ref user))
                currentTSChannels.Add(tvcard.CurrentDbChannel(ref user));
          }
        }
      }
    }

    /// <summary>
    /// Checks if a channel is tunable/tuned or not...
    /// </summary>
    /// <param name="channelName">Name of the channel.</param>
    /// <param name="user">User</param>
    /// <returns>
    ///         <c>channel state tunable|nottunable</c>.
    /// </returns>
    public ChannelState GetChannelState(int idChannel, User user)
    {
      ChannelState chanState;
      Channel dbchannel = Channel.Retrieve(idChannel);

      //User anyUser = new User();
      TvResult viewResult;
      ICardAllocation allocation = CardAllocationFactory.Create(true);
      List<CardDetail> freeCards = allocation.GetAvailableCardsForChannel(_cards, dbchannel, ref user, true, out viewResult);
      if (viewResult == TvResult.Succeeded)
        chanState = ChannelState.tunable;
      else
        chanState = ChannelState.nottunable;

      return chanState;
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

    public int ActiveStreams
    {
      get
      {
        int activeCount = 0;
        if (_streamer == null) return activeCount;
        List<RtspClient> clients = _streamer.Clients;
        foreach (RtspClient client in clients)
        {
          if (client.IsActive)
            activeCount++;
        }
        return activeCount;
      }
    }
    #endregion

    #endregion

    #region private members

    private void HeartBeatMonitor()
    {
      Log.Write("Controller:   Heartbeat Monitor initiated; max timeout allowed is {0} sec.", HEARTBEAT_MAX_SECS_EXCEED_ALLOWED);
      while (true)
      {
        //Log.Write("Controller:   Heartbeat Monitor ping");

        Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();

        //for each card
        while (enumerator.MoveNext())
        {
          KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
          //get a list of all users for this card
          User[] users = keyPair.Value.Users.GetUsers();
          if (users != null)
          {
            //for each user
            for (int i = 0; i < users.Length; ++i)
            {
              User tmpUser = users[i];
              if (tmpUser.HeartBeat > DateTime.MinValue)
              {
                DateTime now = DateTime.Now;
                TimeSpan ts = tmpUser.HeartBeat - now;

                // more than 30 seconds have elapsed since last heartbeat was received.
                // lets kick the client
                if (ts.TotalSeconds < (-1 * HEARTBEAT_MAX_SECS_EXCEED_ALLOWED))
                {
                  Log.Write("Controller:   Heartbeat Monitor (30+ sec. max idletime allowed)- kicking idle user {0}", tmpUser.Name);
                  bool res = StopTimeShifting(ref tmpUser, TvStoppedReason.HeartBeatTimeOut);
                }
              }
            }
          }
        }
        // note; client signals heartbeats each 15 sec.
        Thread.Sleep(HEARTBEAT_MAX_SECS_EXCEED_ALLOWED * 1000); //sleep for 30 secs. before checking heartbeat again
      }
    }

    /// <summary>
    /// Determines whether the the user is the owner of the card
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user.</param>
    /// <returns>
    /// 	<c>true</c> if the specified user is the card owner; otherwise, <c>false</c>.
    /// </returns>
    public bool IsOwner(int cardId, User user)
    {
      if (cardId < 0) return false;
      if (!_cards.ContainsKey(cardId)) return false;
      return _cards[cardId].Users.IsOwner(user);
    }

    /// <summary>
    /// Removes the user from other cards then the one specified
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user.</param>
    public void RemoveUserFromOtherCards(int cardId, User user)
    {
      if (!_cards.ContainsKey(cardId)) return;
      Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();
      ITVCard card = _cards[cardId].Card;
      while (enumerator.MoveNext())
      {
        KeyValuePair<int, ITvCardHandler> key = enumerator.Current;
        if (key.Key == cardId) continue;
        if (key.Value.Card == card) continue;
        key.Value.Users.RemoveUser(user);
      }
    }

    public bool SupportsSubChannels(int cardId)
    {
      if (cardId < 0) return false;
      if (!_cards.ContainsKey(cardId)) return false;
      return _cards[cardId].SupportsSubChannels;
    }

    public void HeartBeat(User user)
    {
      if (user == null) return;
      if (user.CardId < 0) return;
      if (!_cards.ContainsKey(user.CardId)) return;
      _cards[user.CardId].Users.HeartBeartUser(user);
    }

    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    TvResult CardTune(ref User user, IChannel channel, Channel dbChannel)
    {
      try
      {
        int idCard = user.CardId;
        if (!_cards.ContainsKey(idCard)) return TvResult.CardIsDisabled;
        if (_cards[idCard].DataBaseCard.Enabled == false) return TvResult.CardIsDisabled;
        if (!CardPresent(idCard)) return TvResult.CardIsDisabled;
        Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(user), user, channel));
        TvResult result = _cards[idCard].Tuner.CardTune(ref user, channel, dbChannel);

        RemoveUserFromOtherCards(idCard, user);
        Log.Info("control1:{0} {1} {2}", user.Name, user.CardId, user.SubChannel);
        return result;
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(user), user, channel));
      }
    }

    /// <summary>
    /// Start timeshifting on the card
    /// </summary>
    /// <param name="idCard">The id card.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    TvResult CardTimeShift(ref User user, ref string fileName)
    {
      int idCard = user.CardId;
      if (!_cards.ContainsKey(idCard)) return TvResult.CardIsDisabled;
      if (_cards[idCard].IsLocal)
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.StartTimeShifting, GetVirtualCard(user), user));
      }
      return _cards[idCard].TimeShifter.CardTimeShift(ref user, ref fileName);
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

    void CleanRecordingFolders(string fileName)
    {
      try
      {
        Log.Debug("TVController: Clean orphan recording dirs for {0}", fileName);
        string recfolder = System.IO.Path.GetDirectoryName(fileName);
        List<string> recordingPaths = new List<string>();
        Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();

        while (enumerator.MoveNext())
        {
          KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
          string currentCardPath = _cards[keyPair.Value.DataBaseCard.IdCard].DataBaseCard.RecordingFolder;
          if (!recordingPaths.Contains(currentCardPath))
            recordingPaths.Add(currentCardPath);
        }
        Log.Debug("TVController: Checking {0} path(s) for cleanup", Convert.ToString(recordingPaths.Count));

        foreach (string checkPath in recordingPaths)
        {
          if (checkPath != string.Empty && checkPath != System.IO.Path.GetPathRoot(checkPath))
          {
            // make sure we're only deleting directories which are "recording dirs" from a tv card
            if (fileName.Contains(checkPath))
            {
              Log.Debug("TVController: Origin for recording {0} found: {1}", System.IO.Path.GetFileName(fileName), checkPath);
              string deleteDir = recfolder;
              // do not attempt to step higher than the recording base path
              while (deleteDir != System.IO.Path.GetDirectoryName(checkPath) && deleteDir.Length > checkPath.Length)
              {
                try
                {
                  string[] files = System.IO.Directory.GetFiles(deleteDir);
                  string[] subdirs = System.IO.Directory.GetDirectories(deleteDir);
                  if (files.Length == 0)
                  {
                    if (subdirs.Length == 0)
                    {
                      System.IO.Directory.Delete(deleteDir);
                      Log.Info("TVController: Deleted empty recording dir - {0}", deleteDir);
                      DirectoryInfo di = System.IO.Directory.GetParent(deleteDir);
                      deleteDir = di.FullName;
                    }
                    else
                    {
                      Log.Debug("TVController: Found {0} sub-directory(s) in recording path - not cleaning {1}", Convert.ToString(subdirs.Length), deleteDir);
                      return;
                    }
                  }
                  else
                  {
                    Log.Debug("TVController: Found {0} file(s) in recording path - not cleaning {1}", Convert.ToString(files.Length), deleteDir);
                    return;
                  }
                }
                catch (Exception ex1)
                {
                  Log.Info("TVController: Could not delete directory {0} - {1}", deleteDir, ex1.Message);
                  // bail out to avoid i-loop
                  return;
                }
              }
            }
          }
          else
            Log.Debug("TVController: Path not valid for removal - {1}", checkPath);
        }
      }
      catch (Exception ex)
      {
        Log.Error("TVController: Error cleaning the recording folders - {0},{1}", ex.Message, ex.StackTrace);
      }
    }

    public bool IsTunedToTransponder(int cardId, IChannel transponder)
    {
      if (cardId < 0) return false;
      if (!_cards.ContainsKey(cardId)) return false;
      return _cards[cardId].Tuner.IsTunedToTransponder(transponder);
    }

    /// <summary>
    /// Fires an ITvServerEvent to plugins.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// returns a virtual card for the card specified.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    VirtualCard GetVirtualCard(User user)
    {
      if (!_cards.ContainsKey(user.CardId)) return null;
      VirtualCard card = new VirtualCard(user);
      card.RecordingFormat = _cards[user.CardId].DataBaseCard.RecordingFormat;
      card.RecordingFolder = _cards[user.CardId].DataBaseCard.RecordingFolder;
      card.TimeshiftFolder = _cards[user.CardId].DataBaseCard.TimeShiftFolder;
      card.RemoteServer = Dns.GetHostName();
      return card;
    }
    #endregion

    /// <summary>
    /// Gets a value indicating whether the PC can suspend.
    /// When users are still timeshifting or recording we dont want windows to suspend the pc
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the pc can suspend; otherwise, <c>false</c>.
    /// </value>
    public bool CanSuspend
    {
      get
      {
        //Log.Debug("TVController.CanSuspend: checking cards");

        Dictionary<int, ITvCardHandler>.Enumerator enumer = _cards.GetEnumerator();
        while (enumer.MoveNext())
        {
          int cardId = enumer.Current.Key;
          User[] users = _cards[cardId].Users.GetUsers();
          if (users != null)
          {
            for (int i = 0; i < users.Length; ++i)
            {
              if (_cards[cardId].Recorder.IsRecording(ref users[i]) || _cards[cardId].TimeShifter.IsTimeShifting(ref users[i]))
              {
                //Log.Debug("TVController.CanSuspend: checking cards finished -> cannot suspend");
                return false;
              }
            }
          }
        }
        //Log.Debug("TVController.CanSuspend: IsTimeToRecord");

        // check whether the scheduler would like to record something now, but there is no card recording
        // this can happen if a recording is due, but the scheduler has not yet picked up recording (latency)
        if (_scheduler.IsTimeToRecord(DateTime.Now))
        {
          //Log.Debug("TVController.CanSuspend: IsTimeToRecord finished -> cannot suspend" );
          return false;
        }
        //Log.Debug("TVController.CanSuspend: finished, can suspend");

        return true;
      }
    }
  }
}
