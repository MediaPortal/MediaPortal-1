/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Generic;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Epg;
using TvLibrary.Log;
using TvLibrary.Streaming;
using TvControl;
using TvDatabase;
using TvEngine.Events;

namespace TvService
{
  /// <summary>
  /// This class servers all requests from remote clients
  /// and if server is the master it will delegate the requests to the 
  /// correct slave servers
  /// </summary>
  public class TVController : MarshalByRefObject, IController, IDisposable, ITvServerEvent, IEpgEvents, ICiMenuCallbacks
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
    bool _isMaster;

    Thread heartBeatMonitorThread;

    /// <summary>
    /// Reference to our server
    /// </summary>
    Server _ourServer;/// <summary>

    TvCardCollection _localCardCollection;

    /// <summary>
    /// Initialized Conditional Access handler
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true if successful</returns>
    public bool InitConditionalAccess(int cardId)
    {
      if (ValidateTvControllerParams(cardId, false))
      {
        Log.Debug("InitConditionalAccess: ValidateTvControllerParams failed");
        return false;
      }
      ITVCard unknownCard = _cards[cardId].Card;

      if (unknownCard is TvCardBase)
      {
        TvCardBase card = (TvCardBase)unknownCard;
        if (card.ConditionalAccess == null)
        {
          card.BuildGraph();
        }
        return true;
      }
      return false;
    }

    Dictionary<int, ITvCardHandler> _cards;
    /// 

    // contains a cached copy of all the channels in the user defined groups (excl. the all channels group)
    // used to speedup "mini EPG" channel state creation.
    List<Channel> _tvChannelListGroups;

    #region CI Menu Event handling
    /// <summary>
    /// Local copy of event holding a collection
    /// </summary>
    private static event CiMenuCallback s_ciMenu;

    /// <summary>
    /// Add or remove callback destinations on the client
    /// </summary>
    event CiMenuCallback IController.OnCiMenu
    {
      add 
      {
        s_ciMenu = null;
        s_ciMenu += value;
        Log.Debug("CiMenu: registered client event for callback"); 
      }
      remove 
      {
        s_ciMenu = null;
        Log.Debug("CiMenu: unregistered client callback."); 
      }
    }
    #endregion

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
      if (ValidateTvControllerParams(cardId, false))
        return false;
      return _cards[cardId].IsLocal;
    }

    #region CI Menu action functions
    /// <summary>
    /// Returns if selected card has CI Menu capabilities
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if supported</returns>
    public bool CiMenuSupported(int cardId)
    {
      Log.Debug("CiMenuSupported called cardid {0}", cardId);
      if (ValidateTvControllerParams(cardId, false))
      {
        Log.Debug("ValidateTvControllerParams failed");
        return false;
      }
      Log.Debug("CiMenuSupported card {0} supported: {1}", _cards[cardId].CardName, _cards[cardId].CiMenuSupported);
      return _cards[cardId].CiMenuSupported;
    }

    /// <summary>
    /// Enters the card's CI menu
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    public bool EnterCiMenu(int cardId)
    {
      Log.Debug("EnterCiMenu called");
      if (ValidateTvControllerParams(cardId, false))
        return false;
      if (_cards[cardId].CiMenuActions != null)
        return _cards[cardId].CiMenuActions.EnterCIMenu();
      return false;
    }

    /// <summary>
    /// SelectMenu selects an ci menu entry; 
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="choice">choice,0 for "back" action</param>
    /// <returns>true if successful</returns>
    public bool SelectMenu(int cardId, byte choice)
    {
      Log.Debug("SelectCiMenu called");
      if (ValidateTvControllerParams(cardId, false))
        return false;
      return _cards[cardId].CiMenuActions != null && _cards[cardId].CiMenuActions.SelectMenu(choice);
    }

    /// <summary>
    /// CloseMenu closes the menu
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    public bool CloseMenu(int cardId)
    {
      Log.Debug("CloseMenu called");
      if (ValidateTvControllerParams(cardId, false))
        return false;
      return _cards[cardId].CiMenuActions != null && _cards[cardId].CiMenuActions.CloseCIMenu();
    }

    public bool SendMenuAnswer(int cardId, bool Cancel, string Answer)
    {
      Log.Debug("SendMenuAnswer called");
      if (ValidateTvControllerParams(cardId, false))
        return false;
      return _cards[cardId].CiMenuActions != null && _cards[cardId].CiMenuActions.SendMenuAnswer(Cancel, Answer);
    }

    /// <summary>
    /// sets a CI menu callback handler. dummy in this case, because it's an interface member
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="CallbackHandler">null, not required</param>
    /// <returns>true is successful</returns>
    public bool SetCiMenuHandler(int cardId, ICiMenuCallbacks CallbackHandler)
    {
      // register tvservice itself as handler
      return EnableCiMenuHandler(cardId);
    }

    /// <summary>
    /// Registers the tvserver as primary CI menu handler on serverside
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true is successful</returns>
    public bool EnableCiMenuHandler(int cardId)
    {
      bool res;
      Log.Debug("TvController: EnableCiMenuHandler called");
      if (ValidateTvControllerParams(cardId, false))
        return false;
      if (_cards[cardId].CiMenuActions != null)
      {
        res = _cards[cardId].CiMenuActions.SetCiMenuHandler(this);
        Log.Debug("TvController: SetCiMenuHandler: result {0}", res);
        return res;
      }
      else
        return false;
    }
    #endregion

    /*
            /// <summary>
            /// Determines whether the specified card is the local pc or not.
            /// </summary>
            /// <param name="card">Card</param>
            /// <returns>
            /// 	<c>true</c> if the specified host name is local; otherwise, <c>false</c>.
            /// </returns>
            bool IsLocal(Card card)
            {
              if (ValidateTvControllerParams(card)) return false;
              return _cards[card.IdCard].IsLocal;
            }
        */

    /// <summary>
    /// Determines whether the specified host name is the local pc or not.
    /// </summary>
    /// <param name="hostName">Name of the host or ip adress</param>
    /// <returns>
    /// 	<c>true</c> if the specified host name is local; otherwise, <c>false</c>.
    /// </returns>
    static bool IsLocal(string hostName)
    {
      if (hostName == "127.0.0.1")
        return true;
      string localHostName = Dns.GetHostName();
      if (String.Compare(hostName, localHostName, true) == 0)
        return true;
      IPHostEntry local = Dns.GetHostEntry(localHostName);
      if (String.Compare(hostName, local.HostName, true) == 0)
        return true;
      foreach (IPAddress ipaddress in local.AddressList)
      {
        if (String.Compare(hostName, ipaddress.ToString(), true) == 0)
          return true;
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
      if (ValidateTvControllerParams(cardId))
      {
        user = null;
        return false;
      }
      return _cards[cardId].Users.IsLocked(out user);
    }
    /// <summary>
    /// Gets the user for card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    public User GetUserForCard(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return null;

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
      if (ValidateTvControllerParams(user) || (ValidateTvControllerParams(cardId)))
      {
        return;
      }
      _cards[cardId].Users.Lock(user);
    }

    /// <summary>
    /// Unlocks the card.
    /// </summary>
    /// <param name="user">The user.</param>
    public void UnlockCard(User user)
    {
      if (ValidateTvControllerParams(user) || (ValidateTvControllerParams(user.CardId)))
      {
        return;
      }
      _cards[user.CardId].Users.Unlock(user);
    }

    public void Init()
    {
      Log.Info("Controller: Initializing TVServer");
      bool result = false;

      for (int i = 0; i < 5 && !result; i++)
      {
        if (i != 0)
        {
          //Fresh start
          try
          {
            DeInit();
          } catch (Exception) { Log.Error("Controller: Error while deinit TvServer in Init"); }

          Thread.Sleep(3000);
        }
        Log.Info("Controller: {0} init attempt", (i + 1));
        result = InitController();
      }

      if (result)
        Log.Info("Controller: TVServer initialized okay");
      else
        Log.Info("Controller: Failed to initialize TVServer");

      return;
    }


    /// <summary>
    /// Initalizes the controller.
    /// It will update the database with the cards found on this system
    /// start the epg grabber and scheduler
    /// and check if its supposed to be a master or slave controller
    /// </summary>
    bool InitController()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        GlobalServiceProvider.Instance.Remove<ITvServerEvent>();
      }
      GlobalServiceProvider.Instance.Add<ITvServerEvent>(this);
      try
      {
        //string threadname = Thread.CurrentThread.Name;
        //if (string.IsNullOrEmpty(threadname))
        //  Thread.CurrentThread.Name = "TVController";

        //load the database connection string from the config file
        Log.Info(@"{0}\gentle.config", Log.GetPathName());
        string connectionString, provider;
        GetDatabaseConnectionString(out connectionString, out provider);
        string ConnectionLog = connectionString.Remove(connectionString.IndexOf(@"Password=") + 8);
        Log.Info("Controller: using {0} database connection: {1}", provider, ConnectionLog);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);

        _cards = new Dictionary<int, ITvCardHandler>();
        _localCardCollection = new TvCardCollection(this);

        //log all local ip adresses, usefull for debugging problems
        Log.Write("Controller: started at {0}", Dns.GetHostName());
        IPHostEntry local = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ipaddress in local.AddressList)
        {
          // Show only IPv4 family addresses
          if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
          {
            Log.Info("Controller: local ip address:{0}", ipaddress.ToString());
          }
        }

        //get all registered servers from the database
        IList<Server> servers;
        try
        {
          servers = Server.ListAll();
        } catch (Exception ex)
        {
          Log.Error("Controller: Failed to fetch tv servers from database - {0}", Utils.BlurConnectionStringPassword(ex.Message));
          return false;
        }

        // find ourself
        foreach (Server server in servers)
        {
          if (IsLocal(server.HostName))
          {
            Log.Info("Controller: server running on {0}", server.HostName);
            _ourServer = server;
            break;
          }
        }

        //we do not exist yet?
        if (_ourServer == null)
        {
          //then add ourself to the server
          if (servers.Count == 0)
          {
            //there are no other servers so we are the master one.
            Log.Info("Controller: create new server in database");
            _ourServer = new Server(false, Dns.GetHostName(), RtspStreaming.DefaultPort);
            _ourServer.IsMaster = true;
            _isMaster = true;
            _ourServer.Persist();
            Log.Info("Controller: new server created for {0} master:{1} ", Dns.GetHostName(), _isMaster);
          }
          else
          {
            Log.Error("Controller: sorry, master/slave server setups are not supported. Since there is already another server in the db, we exit here.");
            return false;
          }

        }
        _isMaster = _ourServer.IsMaster;

        //enumerate all tv cards in this pc...
        TvBusinessLayer layer = new TvBusinessLayer();
        for (int i = 0; i < _localCardCollection.Cards.Count; ++i)
        {
          //for each card, check if its already mentioned in the database
          bool found = false;
          IList<Card> cards = _ourServer.ReferringCard();
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
            Log.Info("Controller: add card:{0}", _localCardCollection.Cards[i].Name);
            layer.AddCard(_localCardCollection.Cards[i].Name, _localCardCollection.Cards[i].DevicePath, _ourServer);
          }
        }
        //notify log about cards from the database which are removed from the pc
        IList<Card> cardsInDbs = Card.ListAll();
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
                Card cardDB = layer.GetCardByDevicePath(_localCardCollection.Cards[cardNumber].DevicePath);

                bool cardEnabled = cardDB.Enabled;
                bool cardPresent = _localCardCollection.Cards[cardNumber].CardPresent;

                if (cardEnabled && cardPresent)
                {
                  ITVCard unknownCard = _localCardCollection.Cards[cardNumber];

                  if (unknownCard is TvCardBase)
                  {
                    TvCardBase card = (TvCardBase)unknownCard;
                    if (card.PreloadCard)
                    {
                      Log.Info("Controller: preloading card :{0}", card.Name);
                      card.BuildGraph();
                      if (unknownCard is TvCardAnalog)
                      {
                        ((TvCardAnalog)unknownCard).ReloadCardConfiguration();
                      }
                    }
                    else
                    {
                      Log.Info("Controller: NOT preloading card :{0}", card.Name);
                    }
                  }
                  else
                  {
                    Log.Info("Controller: NOT preloading card :{0}", unknownCard.Name);
                  }
                }

                found = true;
                break;
              }
            }
            if (!found)
            {
              Log.Info("Controller: card not found :{0}", dbsCard.Name);

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

        Dictionary<int, ITVCard> localcards = new Dictionary<int, ITVCard>();

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

        Log.Info("Controller: setup hybrid cards");
        IList<CardGroup> cardgroups = CardGroup.ListAll();
        foreach (CardGroup group in cardgroups)
        {
          IList<CardGroupMap> cards = group.CardGroupMaps();
          HybridCardGroup hybridCardGroup = new HybridCardGroup();
          foreach (CardGroupMap card in cards)
          {
            if (localcards.ContainsKey(card.IdCard))
            {
              localcards[card.IdCard].IsHybrid = true;
              Log.WriteFile("Hybrid card: " + localcards[card.IdCard].Name + " (" + group.Name + ")");
              HybridCard hybridCard = hybridCardGroup.Add(card.IdCard, localcards[card.IdCard]);
              localcards[card.IdCard] = hybridCard;
            }
          }
        }

        cardsInDbs = Card.ListAll();
        foreach (Card dbsCard in cardsInDbs)
        {
          if (localcards.ContainsKey(dbsCard.IdCard))
          {
            ITVCard card = localcards[dbsCard.IdCard];
            TvCardHandler tvcard = new TvCardHandler(dbsCard, card);
            _cards[dbsCard.IdCard] = tvcard;
          }

          // remove any old timeshifting TS files	
          try
          {
            string TimeShiftPath = dbsCard.TimeShiftFolder;
            if (string.IsNullOrEmpty(dbsCard.TimeShiftFolder))
            {
              TimeShiftPath = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            }
            if (!Directory.Exists(TimeShiftPath))
            {
              Log.Info("Controller: creating timeshifting folder {0} for card \"{1}\"", TimeShiftPath, dbsCard.Name);
              Directory.CreateDirectory(TimeShiftPath);
            }

            Log.Debug("Controller: card {0}: current timeshiftpath = {1}", dbsCard.Name, TimeShiftPath);
            if (TimeShiftPath != null)
            {
              string[] files = Directory.GetFiles(TimeShiftPath);

              foreach (string file in files)
              {
                try
                {
                  FileInfo fInfo = new FileInfo(file);
                  bool delFile = (fInfo.Extension.ToUpperInvariant().IndexOf(".TSBUFFER") == 0);

                  if (!delFile)
                  {
                    delFile = (fInfo.Extension.ToUpperInvariant().IndexOf(".TS") == 0) && (fInfo.Name.ToUpperInvariant().IndexOf("TSBUFFER") > 0);
                  }
                  if (delFile)
                    File.Delete(fInfo.FullName);
                } catch (IOException) { }
              }
            }
          } catch (Exception exd)
          {
            Log.Info("Controller: Error cleaning old ts buffer - {0}", exd.Message);
          }
        }

        Log.Info("Controller: setup streaming");
        _streamer = new RtspStreaming(_ourServer.HostName, _ourServer.RtspPort);

        if (_isMaster)
        {
          _epgGrabber = new EpgGrabber(this);
          _epgGrabber.Start();
          _scheduler = new Scheduler(this);
          _scheduler.Start();
        }

        // setup heartbeat monitoring thread.
        // useful for kicking idle/dead clients.
        Log.Info("Controller: setup HeartBeat Monitor");

        //stop thread, just incase it is running.
        if (heartBeatMonitorThread != null)
        {
          if (heartBeatMonitorThread.IsAlive)
          {
            heartBeatMonitorThread.Abort();
          }
        }
        heartBeatMonitorThread = new Thread(HeartBeatMonitor);
        heartBeatMonitorThread.Name = "HeartBeatMonitor";
        heartBeatMonitorThread.IsBackground = true;
        heartBeatMonitorThread.Start();
      } catch (Exception ex)
      {
        Log.Write("TvControllerException: {0}\r\n{1}", ex.ToString(), ex.StackTrace);
        return false;
      }
      Log.Info("Controller: initalized");
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
            try
            {
              heartBeatMonitorThread.Abort();
            } catch (Exception) { }
          }
        }

        //stop the RTSP streamer server
        if (_streamer != null)
        {
          Log.Info("Controller: stop streamer...");
          _streamer.Stop();
          _streamer = null;
          Log.Info("Controller: streamer stopped...");
        }
        //stop the recording scheduler
        if (_scheduler != null)
        {
          Log.Info("Controller: stop scheduler...");
          _scheduler.Stop();
          _scheduler = null;
          Log.Info("Controller: scheduler stopped...");
        }
        //stop the epg grabber
        if (_epgGrabber != null)
        {
          Log.Info("Controller: stop epg grabber...");
          _epgGrabber.Stop();
          _epgGrabber = null;
          Log.Info("Controller: epg stopped...");
        }

        //clean up the tv cards
        FreeCards();

        Gentle.Common.CacheManager.Clear();
        if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
        {
          GlobalServiceProvider.Instance.Remove<ITvServerEvent>();
        }
      } catch (Exception ex)
      {
        Log.Error("TvController: Deinit failed - {0}", ex.Message);
      }
    }

    #endregion

    #region IController Members

    #region internal interface
    /// <summary>
    /// Gets the assembly of tvservice.exe
    /// </summary>
    /// <value>Returns the AssemblyVersion of tvservice.exe</value>
    public string GetAssemblyVersion
    {
      get
      {
        return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
      }
    }

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
      IList<Card> cards = Card.ListAll();
      return cards != null && cards.Count > cardIndex ? cards[cardIndex].IdCard : -1;
    }

    /// <summary>
    /// returns if the card is enabled or disabled
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <value>true if enabled, otherwise false</value>
    public bool Enabled(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return false;
      return _cards[cardId].DataBaseCard.Enabled;
    }

    /// <summary>
    /// Gets the type of card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <value>cardtype (Analog,DvbS,DvbT,DvbC,Atsc)</value>
    public CardType Type(int cardId)
    {
      if (!_cards.ContainsKey(cardId))
        return CardType.Unknown;
      if (ValidateTvControllerParams(cardId))
        return CardType.Unknown;
      return _cards[cardId].Type;
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <returns>name of card</returns>
    public string CardName(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return "";
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
      if (ValidateTvControllerParams(cardId))
        return false;
      return _cards[cardId].Tuner.CanTune(channel);
    }

    /// <summary>
    /// Method to check if card is currently present and detected
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>		
    public bool CardPresent(int cardId)
    {
      //gemx 01.04.08: This is needed otherwise we get a recursive endless loop
      if (!_cards.ContainsKey(cardId))
        return false;
      if (!IsLocal(cardId))
      {
        RemoteControl.HostName = _cards[cardId].DataBaseCard.ReferencedServer().HostName;
        return RemoteControl.Instance.CardPresent(cardId);
      }
      if (!_cards.ContainsKey(cardId))
        return false;
      if (cardId < 0)
        return false;
      string devicePath = _cards[cardId].Card.DevicePath;
      if (devicePath.Length > 0)
      {
        // Remove it from the local card collection
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
    /// Method to remove a non-present card from the local card collection
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>		
    public void CardRemove(int cardId)
    {

      if (ValidateTvControllerParams(cardId))
      {
        Card card = Card.Retrieve(cardId);
        if (card != null)
        {
          card.Remove();
        }
        return;
      }

      /*if (!IsLocal(cardId))
      {
        RemoteControl.HostName = _cards[cardId].DataBaseCard.ReferencedServer().HostName;
        RemoteControl.Instance.CardRemove(cardId);
        return;
      }
      */

      string devicePath = _cards[cardId].Card.DevicePath;
      if (devicePath.Length > 0)
      {
        // Remove database instance
        _cards[cardId].DataBaseCard.Remove();
        // Remove it from the card collection
        _cards.Remove(cardId);
        // Remove it from the local card collection
        _localCardCollection.Cards.Remove(_cards[cardId].Card);
      }
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>device of card</returns>
    public string CardDevice(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return "";
      return _cards[cardId].CardDevice();
    }

    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return null;
      return _cards[user.CardId].CurrentChannel(ref user);
    }
    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>id of database channel</returns>
    public int CurrentDbChannel(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return -1;
      return _cards[user.CardId].CurrentDbChannel(ref user);
    }

    /// <summary>
    /// Gets the current channel name.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>channel</returns>
    public string CurrentChannelName(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return "";
      return _cards[user.CardId].CurrentChannelName(ref user);
    }


    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    public bool TunerLocked(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return true;
      return _cards[cardId].TunerLocked;
    }

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    public int SignalQuality(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return -1;
      return _cards[cardId].SignalQuality;
    }

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal level (0-100)</returns>
    public int SignalLevel(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return -1;
      return _cards[cardId].SignalLevel;
    }
    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    public void UpdateSignalSate(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return;
      _cards[cardId].UpdateSignalSate();
    }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>filename or null when not recording</returns>
    public string RecordingFileName(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return "";
      return _cards[user.CardId].Recorder.FileName(ref user);
    }

    public string TimeShiftFileName(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return "";
      return _cards[user.CardId].TimeShifter.FileName(ref user);
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    public bool TimeShiftGetCurrentFilePosition(ref User user,ref Int64 position,ref long bufferId)
    {
      if (ValidateTvControllerParams(user))
        return false;
      return _cards[user.CardId].TimeShifter.GetCurrentFilePosition(ref user,ref position,ref bufferId);
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return false;
      return _cards[user.CardId].TimeShifter.IsTimeShifting(ref user);
    }

    /// <summary>
    /// This function checks whether something should be recorded at the given time.
    /// </summary>
    /// <param name="time">the time to check for recordings.</param>
    /// <returns>true if any recording due to time</returns>
    public bool IsTimeToRecord(DateTime time)
    {
      return _scheduler.IsTimeToRecord(time);
    }

    /// This function checks if a spedific schedule should be recorded at the given time.
    /// </summary>
    /// <param name="time">the time to check for recordings.</param>
    /// <param name="scheduleId">the time id of the recording.</param>
    /// <returns>true if any recording due to time</returns>
    public bool IsTimeToRecord(DateTime time, int scheduleId)
    {
      Schedule schedule = Schedule.Retrieve(scheduleId);
      return _scheduler.IsTimeToRecord(schedule, time);
    }

    /// <summary>
    /// Returns the video stream currently associated with the card. 
    /// </summary>
    /// <returns>stream_type</returns>
    public int GetCurrentVideoStream(User user)
    {
      if (ValidateTvControllerParams(user))
        return -1;
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
    /// Determines if any card is currently busy recording or timeshifting
    /// </summary>
    /// <param name="userTS">timeshifting user</param>
    /// <param name="isUserTS">true if the specified user is timeshifting</param>
    /// <param name="isAnyUserTS">true if any user (except for the userTS) is timeshifting</param>
    /// <param name="isRec">true if recording</param>
    /// <returns>
    /// 	<c>true</c> if a card is recording or timeshifting; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAnyCardRecordingOrTimeshifting(User userTS, out bool isUserTS, out bool isAnyUserTS, out bool isRec)
    {
      isUserTS = false;
      isAnyUserTS = false;
      isRec = false;

      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      while (en.MoveNext())
      {
        ITvCardHandler card = en.Current.Value;
        User user = new User();
        user.CardId = card.DataBaseCard.IdCard;

        if (!isRec)
        {
          isRec = card.Recorder.IsAnySubChannelRecording;
        }
        if (!isUserTS)
        {
          isUserTS = card.TimeShifter.IsTimeShifting(ref userTS);
        }

        User[] users = card.Users.GetUsers();
        if (users == null)
          continue;
        if (users.Length == 0)
          continue;
        for (int i = 0; i < users.Length; ++i)
        {
          User anyUser = users[i];

          if (anyUser.Name != userTS.Name)
          {
            if (!isAnyUserTS)
            {
              isAnyUserTS = card.TimeShifter.IsTimeShifting(ref anyUser);
              break;
            }
          }
        }
      }

      if (isRec || isUserTS || isAnyUserTS)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Determines whether the specified channel name is recording.
    /// </summary>
    /// <param name="channelName">Name of the channel.</param>
    /// <param name="card">The vcard.</param>
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
        if (users == null)
          continue;
        if (users.Length == 0)
          continue;
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (tvcard.CurrentChannelName(ref user) == null)
            continue;
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

    public List<VirtualCard> GetAllRecordingCards()
    {
      List<VirtualCard> recCards = new List<VirtualCard>();

      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      ITvCardHandler tvcard;
      while (en.MoveNext())
      {
        tvcard = en.Current.Value;
        User[] users = tvcard.Users.GetUsers();
        if (users == null)
          continue;
        if (users.Length == 0)
          continue;
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          bool isREC = tvcard.Recorder.IsRecording(ref user);
          if (isREC)
          {
            VirtualCard card = GetVirtualCard(user);
            recCards.Add(card);
          }
        }
      }
      return recCards;
    }

    /// <summary>
    /// Determines whether the specified channel name is recording.
    /// </summary>
    /// <param name="channelName">Name of the channel.</param>
    /// <param name="card">The vcard.</param>    
    /// <param name="isTS">timeshifting.</param>    
    /// <param name="isREC">recording</param>    
    /// <returns>
    /// 	<c>true</c> if the specified channel name is recording or timeshifting; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRecordingTimeshifting(string channelName, out VirtualCard card, out bool isTS, out bool isREC)
    {
      isREC = false;
      isTS = false;
      card = null;
      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      ITvCardHandler tvcard;
      User recUser = null;
      while (en.MoveNext())
      {
        tvcard = en.Current.Value;
        User[] users = tvcard.Users.GetUsers();
        if (users == null)
          continue;
        if (users.Length == 0)
          continue;
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (tvcard.CurrentChannelName(ref user) == null)
            continue;
          if (tvcard.CurrentChannelName(ref user) == channelName)
          {
            if (!isREC)
            {
              isREC = tvcard.Recorder.IsRecording(ref user);
              if (isREC)
              {
                recUser = user;
              }
            }
            if (!isTS)
            {
              isTS = tvcard.TimeShifter.IsTimeShifting(ref user);
            }
          }
        }
      }

      if (isREC || isTS)
      {
        if (recUser != null)
        {
          card = GetVirtualCard(recUser);
        }
        return true;
      }
      return false;
    }

    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true when card is recording otherwise false</returns>
    public bool IsRecording(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return false;
      return _cards[user.CardId].Recorder.IsRecording(ref user);
    }

    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    public bool IsScanning(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return false;
      return _cards[cardId].Scanner.IsScanning;
    }

    /// <summary>
    /// Returns if the card is grabbing the epg or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    public bool IsGrabbingEpg(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return false;
      return _cards[cardId].Epg.IsGrabbing;
    }

    /// <summary>
    /// Returns if the card is grabbing teletext or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true when card is grabbing teletext otherwise false</returns>
    public bool IsGrabbingTeletext(User user)
    {
      if (ValidateTvControllerParams(user))
        return false;
      return _cards[user.CardId].Teletext.IsGrabbingTeletext(user);
    }

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    public bool HasTeletext(User user)
    {
      if (ValidateTvControllerParams(user))
        return false;
      return _cards[user.CardId].Teletext.HasTeletext(user);
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(User user, int pageNumber)
    {
      if (ValidateTvControllerParams(user))
        return new TimeSpan(0, 0, 15);
      return _cards[user.CardId].Teletext.TeletextRotation(user, pageNumber);
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime TimeShiftStarted(User user)
    {
      if (ValidateTvControllerParams(user))
        return DateTime.MinValue;
      return _cards[user.CardId].TimeShifter.TimeShiftStarted(user);
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted(User user)
    {
      if (ValidateTvControllerParams(user))
        return DateTime.MinValue;
      return _cards[user.CardId].Recorder.RecordingStarted(user);
    }

    /// <summary>
    /// Copies the time shift buffer files to the currently started recording 
    /// </summary>
    /// <param name="position1">start offset in first ts buffer file </param>
    /// <param name="bufferFile1">ts buffer file to start with</param>
    /// <param name="position2">end offset in last ts buffer file</param>
    /// <param name="bufferFile2">ts buffer file to stop at</param>
    /// <param name="recordingFile">filename of the recording</param>
    public void CopyTimeShiftFile(Int64 position1, string bufferFile1, Int64 position2, string bufferFile2,string recordingFile)
    {
      TsCopier copier = new TsCopier(position1, bufferFile1, position2, bufferFile2, recordingFile);
      Thread worker = new Thread(new ThreadStart(copier.DoCopy));
      worker.Start();
    }

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    public bool IsScrambled(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return false;
      return _cards[user.CardId].IsScrambled(ref user);
    }

    /// <summary>
    /// returns the min/max channel numbers for analog cards
    /// </summary>
    public int MinChannel(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return 0;
      return _cards[cardId].MinChannel;
    }

    public int MaxChannel(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return 0;
      return _cards[cardId].MaxChannel;
    }

    /// <summary>
    /// Does the card have a CA module.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public bool HasCA(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return false;
      return _cards[cardId].HasCA;
    }

    /// <summary>
    /// Gets the number of channels decrypting.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return 0;
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
      if (ValidateTvControllerParams(user) || ValidateTvControllerParams(channel))
        return TvResult.UnknownError;
      try
      {
        //if (user == null) return TvResult.UnknownError;
        //if (channel == null) return TvResult.UnknownError;
        //if (user.CardId < 0) return TvResult.CardIsDisabled;

        int cardId = user.CardId;
        if (_cards[cardId].DataBaseCard.Enabled == false)
          return TvResult.CardIsDisabled;
        //if (!CardPresent(cardId)) return TvResult.CardIsDisabled;

        if (_cards[cardId].Card.SubChannels.Length > 0)
        {
          bool isRec = _cards[cardId].Recorder.IsRecordingAnyUser();
          if (!isRec)
          {
            TvCardContext context = (TvCardContext)_cards[cardId].Card.Context;
            User[] users = context.Users;

            foreach (User userObj in users)
            {
              if (userObj.Name.Equals(user.Name))
              {
                if (userObj.SubChannel > -1)
                {
                  _cards[cardId].Card.FreeSubChannelContinueGraph(userObj.SubChannel);
                }
                break;
              }
            }
          }
        }



        //on tune we need to remember to remove the previous subchannel before tuning the next one.
        // but only if the subchannel is free, meaning not recording and no other users attached.          
        if (user.SubChannel > 0 && _cards[cardId].Card.SubChannels.Length > -1)
        {
          bool isRec = _cards[cardId].Recorder.IsRecordingAnyUser();
          if (!isRec)
          {
            _cards[cardId].Card.FreeSubChannelContinueGraph(user.SubChannel);
          }
        }

        Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(user), user, channel));
        TvResult res = _cards[cardId].Tuner.Tune(ref user, channel, idChannel);



        /*if (res == TvResult.Succeeded)
        {
          RemoveUserFromOtherCards(cardId, user);
        }
        */
        return res;

      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(user), user, channel));
      }
    }

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="onOff">turn on/off teletext grabbing</param>
    public void GrabTeletext(User user, bool onOff)
    {
      if (ValidateTvControllerParams(user))
        return;
      _cards[user.CardId].Teletext.GrabTeletext(user, onOff);
    }

    /// <summary>
    /// Gets the teletext page.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    public byte[] GetTeletextPage(User user, int pageNumber, int subPageNumber)
    {
      if (ValidateTvControllerParams(user))
        return new byte[] { 1 };
      return _cards[user.CardId].Teletext.GetTeletextPage(user, pageNumber, subPageNumber);
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    public int SubPageCount(User user, int pageNumber)
    {
      if (ValidateTvControllerParams(user))
        return -1;
      return _cards[user.CardId].Teletext.SubPageCount(user, pageNumber);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the red button</returns>
    public int GetTeletextRedPageNumber(User user)
    {
      if (ValidateTvControllerParams(user))
        return -1;
      return _cards[user.CardId].Teletext.GetTeletextRedPageNumber(user);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the green button</returns>
    public int GetTeletextGreenPageNumber(User user)
    {
      if (ValidateTvControllerParams(user))
        return -1;
      return _cards[user.CardId].Teletext.GetTeletextGreenPageNumber(user);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    public int GetTeletextYellowPageNumber(User user)
    {
      if (ValidateTvControllerParams(user))
        return -1;
      return _cards[user.CardId].Teletext.GetTeletextYellowPageNumber(user);
    }

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Teletext pagenumber for the blue button</returns>
    public int GetTeletextBluePageNumber(User user)
    {
      if (ValidateTvControllerParams(user))
        return -1;
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
      if (ValidateTvControllerParams(user))
        return TvResult.UnknownError;
      try
      {
        int cardId = user.CardId;
        if (false == _cards[cardId].IsLocal)
        {
          try
          {
            RemoteControl.HostName = _cards[cardId].DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.StartTimeShifting(ref user, ref fileName);
          } catch (Exception)
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

        bool isTimeShifting;
        try
        {
          isTimeShifting = _cards[cardId].TimeShifter.IsTimeShifting(ref user);
        } catch (Exception ex)
        {
          isTimeShifting = false;
          Log.Error("Exception in checking  " + ex.Message);
        }
        TvResult result = _cards[cardId].TimeShifter.Start(ref user, ref fileName);
        if (result == TvResult.Succeeded)
        {
          if (!isTimeShifting)
          {
            Log.Info("user:{0} card:{1} sub:{2} add stream:{3}", user.Name, user.CardId, user.SubChannel, fileName);
            if (File.Exists(fileName))
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
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
      return TvResult.UnknownError;
    }

    public void StopCard(User user)
    {
      if (ValidateTvControllerParams(user))
        return;
      _cards[user.CardId].StopCard(user);
    }

    public bool StopTimeShifting(ref User user, TvStoppedReason reason)
    {
      if (ValidateTvControllerParams(user))
        return false;
      _cards[user.CardId].Users.SetTvStoppedReason(user, reason);
      return StopTimeShifting(ref user);
    }

    public TvStoppedReason GetTvStoppedReason(User user)
    {
      if (ValidateTvControllerParams(user))
        return TvStoppedReason.UnknownReason;

      try
      {
        if (_cards[user.CardId].DataBaseCard.Enabled == false)
          return TvStoppedReason.UnknownReason;
        //if (!CardPresent(user.CardId)) return TvStoppedReason.UnknownReason;

        return _cards[user.CardId].Users.GetTvStoppedReason(user);
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
      return TvStoppedReason.UnknownReason;
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns></returns>
    public bool StopTimeShifting(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return false;
      try
      {
        int cardId = user.CardId;
        if (_cards[cardId].DataBaseCard.Enabled == false)
          return true;
        //if (!CardPresent(cardId)) return true;

        if (false == _cards[cardId].IsLocal)
        {
          try
          {
            if (IsGrabbingEpg(cardId))
            {
              _epgGrabber.Stop(); // we need this, otherwise tvservice will hang in the event stoptimeshifting is called by heartbeat timeout function
            }
            RemoteControl.HostName = _cards[cardId].DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.StopTimeShifting(ref user);
          } catch (Exception)
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


        if (false == _cards[cardId].TimeShifter.IsTimeShifting(ref user))
          return true;
        Fire(this, new TvServerEventArgs(TvServerEventType.EndTimeShifting, GetVirtualCard(user), user));

        if (_cards[cardId].Recorder.IsRecording(ref user))
          return true;

        Log.Write("Controller: StopTimeShifting {0}", cardId);
        lock (this)
        {
          if (IsGrabbingEpg(cardId))
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

          if (result)
          {
            UpdateChannelStatesForUsers();
          }

          return result;
        }
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <param name="contentRecording">if true then create a content recording else a reference recording</param>
    /// <param name="startTime">not used</param>
    /// <returns></returns>
    public TvResult StartRecording(ref User user, ref string fileName, bool contentRecording, long startTime)
    {
      if (ValidateTvControllerParams(user))
        return TvResult.UnknownError;
      TvResult result = _cards[user.CardId].Recorder.Start(ref user, ref  fileName, contentRecording, startTime);

      if (result == TvResult.Succeeded)
      {
        UpdateChannelStatesForUsers();
      }

      return result;
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns></returns>
    public bool StopRecording(ref User user)
    {
      if (ValidateTvControllerParams(user))
        return false;
      bool result = _cards[user.CardId].Recorder.Stop(ref user);

      if (result)
      {
        UpdateChannelStatesForUsers();
      }

      return result;
    }

    /// <summary>
    /// scans current transponder for more channels.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <param name="cardId">IChannel containing the transponder tuning details.</param>
    /// <param name="channel">The channel</param>
    /// <returns>list of channels found</returns>
    public IChannel[] Scan(int cardId, IChannel channel)
    {
      if (ValidateTvControllerParams(cardId))
        return null;

      ScanParameters settings = new ScanParameters();
      TvBusinessLayer layer = new TvBusinessLayer();
      settings.TimeOutTune = Int32.Parse(layer.GetSetting("timeoutTune", "2").Value);
      settings.TimeOutPAT = Int32.Parse(layer.GetSetting("timeoutPAT", "5").Value);
      settings.TimeOutCAT = Int32.Parse(layer.GetSetting("timeoutCAT", "5").Value);
      settings.TimeOutPMT = Int32.Parse(layer.GetSetting("timeoutPMT", "10").Value);
      settings.TimeOutSDT = Int32.Parse(layer.GetSetting("timeoutSDT", "20").Value);
      settings.TimeOutAnalog = Int32.Parse(layer.GetSetting("timeoutAnalog", "20").Value);
      return _cards[cardId].Scanner.Scan(channel, settings);
    }

    public IChannel[] ScanNIT(int cardId, IChannel channel)
    {
      if (ValidateTvControllerParams(cardId))
        return null;

      ScanParameters settings = new ScanParameters();
      TvBusinessLayer layer = new TvBusinessLayer();
      settings.TimeOutTune = Int32.Parse(layer.GetSetting("timeoutTune", "2").Value);
      settings.TimeOutPAT = Int32.Parse(layer.GetSetting("timeoutPAT", "5").Value);
      settings.TimeOutCAT = Int32.Parse(layer.GetSetting("timeoutCAT", "5").Value);
      settings.TimeOutPMT = Int32.Parse(layer.GetSetting("timeoutPMT", "10").Value);
      settings.TimeOutSDT = Int32.Parse(layer.GetSetting("timeoutSDT", "20").Value);
      settings.TimeOutAnalog = Int32.Parse(layer.GetSetting("timeoutAnalog", "20").Value);
      return _cards[cardId].Scanner.ScanNIT(channel, settings);
    }

    /// <summary>
    /// grabs the epg.
    /// </summary>
    /// <param name="grabber">EPG grabber</param>
    /// <param name="cardId">id of the card.</param>
    /// <returns></returns>
    public bool GrabEpg(BaseEpgGrabber grabber, int cardId)
    {
      Log.Info("Controller: GrabEpg on card ID == {0}", cardId);
      if (ValidateTvControllerParams(cardId))
      {
        Log.Error("Controller: GrabEpg - invalid cardId");
        return false;
      }
      return _cards[cardId].Epg.Start(grabber);
    }

    /// <summary>
    /// Aborts grabbing the epg. This also triggers the OnEpgReceived callback.
    /// </summary>
    public void AbortEPGGrabbing(int cardId)
    {
      Log.Info("Controller: AbortEPGGrabbing on card ID == {0}", cardId);
      if (ValidateTvControllerParams(cardId))
      {
        Log.Error("Controller: AbortEPGGrabbing - invalid cardId");
        return;
      }
      _cards[cardId].Epg.Abort();
    }

    /// <summary>
    /// Epgs the specified card id.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    public List<EpgChannel> Epg(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
      {
        return new List<EpgChannel>();
      }
      return _cards[cardId].Epg.Epg;
    }

    /// <summary>
    /// Deletes the recording from database and disk
    /// </summary>
    /// <param name="idRecording">The id recording.</param>
    public bool DeleteRecording(int idRecording)
    {
      try
      {
        Recording rec = Recording.Retrieve(idRecording);
        if (rec == null)
        {
          return false;
        }

        if (!IsLocal(rec.ReferencedServer().HostName))
        {
          try
          {
            RemoteControl.HostName = rec.ReferencedServer().HostName;
            return RemoteControl.Instance.DeleteRecording(rec.IdRecording);
          } 
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", rec.ReferencedServer().HostName);
          }
          return false;
        }

        _streamer.RemoveFile(rec.FileName);
        RecordingFileHandler handler = new RecordingFileHandler();
        bool result = handler.DeleteRecordingOnDisk(rec);
        if (result)
        {
          rec.Delete();
          return true;
        }
      } 
      catch (Exception)
      {
        Log.Error("Controller: Can't delete recording");
      }
      return false;
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
        if (rec == null)
        {
          return false;
        }
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
        return (File.Exists(rec.FileName));
      } catch (Exception)
      {
        return true;
      }
    }

    /// <summary>
    /// returns which schedule the card specified is currently recording
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="ChannelId">channel id</param>
    /// <returns>
    /// id of Schedule or -1 if  card not recording
    /// </returns>
    public int GetRecordingSchedule(int cardId, int ChannelId)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
          return -1;
        if (_isMaster == false)
          return -1;
        if (_cards[cardId].DataBaseCard.Enabled == false)
          return -1;
        //if (!CardPresent(cardId)) return -1;
        return _scheduler.GetRecordingScheduleForCard(cardId, ChannelId);
      } catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    #region audio streams
    public IAudioStream[] AvailableAudioStreams(User user)
    {
      if (ValidateTvControllerParams(user))
        return null;
      return _cards[user.CardId].Audio.Streams(user);
    }

    public IAudioStream GetCurrentAudioStream(User user)
    {
      if (ValidateTvControllerParams(user))
        return null;
      return _cards[user.CardId].Audio.GetCurrent(user);
    }

    public void SetCurrentAudioStream(User user, IAudioStream stream)
    {
      if (ValidateTvControllerParams(user))
        return;
      _cards[user.CardId].Audio.Set(user, stream);
    }

    public string GetStreamingUrl(User user)
    {
      if (ValidateTvControllerParams(user))
        return "";
      try
      {
        if (_cards[user.CardId].DataBaseCard.Enabled == false)
          return "";
        //if (!CardPresent(user.CardId)) return "";
        if (IsLocal(user.CardId) == false)
        {
          try
          {
            RemoteControl.HostName = _cards[user.CardId].DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.GetStreamingUrl(user);
          } catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", _cards[user.CardId].DataBaseCard.ReferencedServer().HostName);
            return "";
          }
        }
        return String.Format("rtsp://{0}:{1}/stream{2}.{3}", _ourServer.HostName, _streamer.Port, user.CardId, user.SubChannel);
      } catch (Exception)
      {
        Log.Error("Controller: Can't get streaming url");
      }
      return "";
    }

    public string GetRecordingUrl(int idRecording)
    {
      try
      {
        Recording recording = Recording.Retrieve(idRecording);
        if (recording == null)
          return "";
        if (recording.FileName == null)
          return "";
        if (recording.FileName.Length == 0)
          return "";
        if (!IsLocal(recording.ReferencedServer().HostName))
        {
          try
          {
            RemoteControl.HostName = recording.ReferencedServer().HostName;
            return RemoteControl.Instance.GetRecordingUrl(idRecording);
          } catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", recording.ReferencedServer().HostName);
            return "";
          }
        }
        try
        {
          if (File.Exists(recording.FileName))
          {
            _streamer.Start();
            string streamName = String.Format("{0:X}", recording.FileName.GetHashCode());
            RtspStream stream = new RtspStream(streamName, recording.FileName, recording.Title);
            _streamer.AddStream(stream);
            string url = String.Format("rtsp://{0}:{1}/{2}", _ourServer.HostName, _streamer.Port, streamName);
            Log.Info("Controller: streaming url:{0} file:{1}", url, recording.FileName);
            return url;
          }
        } catch (Exception)
        {
          Log.Error("Controller: Can't get recroding url - First catch");
        }
      } catch (Exception)
      {
        Log.Error("Controller: Can't get recroding url - Second catch");
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
      if (File.Exists(fileName))
      {
        _streamer.Start();
        string streamName = String.Format("{0:X}", fileName.GetHashCode());
        RtspStream stream = new RtspStream(streamName, fileName, streamName);
        _streamer.AddStream(stream);
        string url = String.Format("rtsp://{0}:{1}/{2}", _ourServer.HostName, _streamer.Port, streamName);
        Log.Info("Controller: streaming url:{0} file:{1}", url, fileName);
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
        Log.Info("Controller: dispose card:{0}", key.Value.CardName);
        try
        {
          key.Value.Dispose();
        } catch (Exception ex)
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
      if (user == null)
        return -1;

      Channel channel = Channel.Retrieve(idChannel);
      Log.Write("Controller: TimeShiftingWouldUseCard {0} {1}", channel.DisplayName, channel.IdChannel);

      try
      {
        AdvancedCardAllocation allocation = new AdvancedCardAllocation();
        TvResult result;
        List<CardDetail> freeCards = allocation.GetAvailableCardsForChannel(_cards, channel, ref user, true, out result, 0);
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
      } catch (Exception ex)
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
    /// <param name="forceCardId">Indicated, if the card should be forced</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref User user, int idChannel, out VirtualCard card,bool forceCardId)
    {
      if (user == null)
      {
        card = null;
        return TvResult.UnknownError;
      }

      Channel channel = Channel.Retrieve(idChannel);
      Log.Write("Controller: StartTimeShifting {0} {1}", channel.DisplayName, channel.IdChannel);
      card = null;
      if (_epgGrabber != null)
      {
        _epgGrabber.Stop();
      }
      try
      {
        AdvancedCardAllocation allocation = new AdvancedCardAllocation();
        TvResult result;
        List<CardDetail> freeCards = allocation.GetAvailableCardsForChannel(_cards, channel, ref user, true, out result, 0);
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

        //keep tuning each card until we are succesful                
        for (int i = 0; i < freeCards.Count; i++)
        {
          if (i > 0)
          {
            Log.Write("Controller: Timeshifting failed, lets try next available card.");
          }
          User userCopy = new User(user.Name, user.IsAdmin);

          CardDetail cardInfo = freeCards[i];
          userCopy.CardId = cardInfo.Id;
          if(forceCardId && user.CardId != cardInfo.Id)
          {
            continue;
          }
          IChannel tuneChannel = cardInfo.TuningDetail;

          //setup folders
          if (cardInfo.Card.RecordingFolder == String.Empty)
          {
            cardInfo.Card.RecordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            if (!Directory.Exists(cardInfo.Card.RecordingFolder))
            {
              Log.Write("Controller: creating recording folder {0} for card {0}", cardInfo.Card.RecordingFolder, cardInfo.Card.Name);
              Directory.CreateDirectory(cardInfo.Card.RecordingFolder);
            }
          }
          if (cardInfo.Card.TimeShiftFolder == String.Empty)
          {
            cardInfo.Card.TimeShiftFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            if (!Directory.Exists(cardInfo.Card.TimeShiftFolder))
            {
              Log.Write("Controller: creating timeshifting folder {0} for card {0}", cardInfo.Card.TimeShiftFolder, cardInfo.Card.Name);
              Directory.CreateDirectory(cardInfo.Card.TimeShiftFolder);
            }
          }

          //todo : if the owner is changing channel to a new transponder, then kick any leeching users.
          ITvCardHandler tvcard = _cards[cardInfo.Id];
          bool isTS = tvcard.TimeShifter.IsAnySubChannelTimeshifting;

          if (isTS)
          {
            User[] users = tvcard.Users.GetUsers();
            for (int j = users.Length - 1; j > -1; j--)
            {
              User u = users[j];
              if (user.Name.Equals(u.Name))
                continue;
              IChannel tmpChannel = tvcard.CurrentChannel(ref u);

              if (tmpChannel == null)
              {
                tvcard.Users.RemoveUser(u); //removing inactive user which shouldnt happen, but atleast its better than having timeshfiting fail.
                continue;
              }

              bool isDiffTS = tuneChannel.IsDifferentTransponder(tmpChannel);

              if (isDiffTS)
              {
                Log.Write("Controller: kicking leech user {0} off card {1} since owner {2} changed transponder", u.Name, cardInfo.Card.Name, user.Name);
                StopTimeShifting(ref u, TvStoppedReason.OwnerChangedTS);
              }
            }
          }

          //tune to the new channel                  
          result = CardTune(ref userCopy, tuneChannel, channel);
          if (result != TvResult.Succeeded)
          {
            continue; //try next card            
          }
          Log.Info("control2:{0} {1} {2}", userCopy.Name, userCopy.CardId, userCopy.SubChannel);
          if (!IsTimeShifting(ref userCopy))
          {
            CleanTimeShiftFiles(cardInfo.Card.TimeShiftFolder, String.Format("live{0}-{1}.ts", userCopy.CardId, userCopy.SubChannel));
          }
          string timeshiftFileName = String.Format(@"{0}\live{1}-{2}.ts", cardInfo.Card.TimeShiftFolder, userCopy.CardId, userCopy.SubChannel);

          //start timeshifting
          result = StartTimeShifting(ref userCopy, ref timeshiftFileName);
          if (result != TvResult.Succeeded)
          {
            continue; //try next card
          }
          Log.Write("Controller: StartTimeShifting started on card:{0} to {1}", userCopy.CardId, timeshiftFileName);
          card = GetVirtualCard(userCopy);
          RemoveUserFromOtherCards(card.Id, userCopy); //only remove user from other cards if new tuning was a success
          UpdateChannelStatesForUsers();
          break; //if we made it to the bottom, then we have a successful timeshifting.
        }

        if (result != TvResult.Succeeded)
        {
          if (_epgGrabber != null)
          {
            _epgGrabber.Start();
          }
        }

        return result;
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
      return StartTimeShifting(ref user, idChannel, out card, false);
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
        if (_isMaster == false)
          return false;
        if (!_scheduler.IsRecordingSchedule(idSchedule, out card))
        {
          Log.Info("IsRecordingSchedule: scheduler is not recording schedule");
          return false;
        }
        Log.Info("IsRecordingSchedule: scheduler is recording schedule on cardid:{0}", card.Id);

        return true;
      } catch (Exception ex)
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
        if (_isMaster == false)
          return;
        _scheduler.StopRecordingSchedule(idSchedule);
      } catch (Exception ex)
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
        Gentle.Common.CacheManager.ClearQueryResultsByType(typeof(Schedule));
        if (_scheduler != null)
        {
          _scheduler.ResetTimer();
        }
        Fire(this, new TvServerEventArgs(TvServerEventType.ScheduledAdded));

      } catch (Exception ex)
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

      } catch (Exception ex)
      {
        Log.Write(ex);
        return;
      }
    }

    /// <summary>
    /// This method will be called by the EPG grabber.
    /// </summary>
    public void OnImportEpgPrograms(EpgChannel epgChannel)
    {
      try
      {
        TvServerEventArgs eventArgs = new TvServerEventArgs(TvServerEventType.ImportEpgPrograms, epgChannel);
        Fire(this, eventArgs);
      } catch (Exception ex)
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
          if (_epgGrabber == null)
            return false;
          return _epgGrabber.IsRunning;
        } catch (Exception ex)
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
        } catch (Exception ex)
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
        //Give it a few secounds.
        Thread.Sleep(5000);
        Init();
      } catch (Exception ex)
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
        XmlDocument doc = new XmlDocument();
        doc.Load(String.Format(@"{0}\gentle.config", Log.GetPathName()));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode nodeConnection = nodeKey.Attributes.GetNamedItem("connectionString");
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
        connectionString = nodeConnection.InnerText;
        provider = nodeProvider.InnerText;
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    public void SetDatabaseConnectionString(string connectionString, string provider)
    {
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(String.Format(@"{0}\gentle.config", Log.GetPathName()));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode nodeConnection = nodeKey.Attributes.GetNamedItem("connectionString");
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
        nodeProvider.InnerText = connectionString;
        nodeConnection.InnerText = provider;
        doc.Save(String.Format(@"{0}\gentle.config", Log.GetPathName()));

        Gentle.Framework.ProviderFactory.ResetGentle(true);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);
        DeInit();
        //Give it a few seconds.
        Thread.Sleep(3000);
        Init();
      } catch (Exception ex)
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
          if (_cards[cardId].IsIdle == false)
            return false;
        }
        return true;
      }
    }

    #region DiSEqC

    public void DiSEqCGetPosition(int cardId, out int satellitePosition, out int stepsAzimuth, out int stepsElevation)
    {
      if (ValidateTvControllerParams(cardId))
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
      if (ValidateTvControllerParams(cardId))
        return;
      _cards[cardId].DisEqC.Reset();
    }

    public void DiSEqCStopMotor(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return;
      _cards[cardId].DisEqC.StopMotor();
    }

    public void DiSEqCSetEastLimit(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return;
      _cards[cardId].DisEqC.SetEastLimit();
    }

    public void DiSEqCSetWestLimit(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return;
      _cards[cardId].DisEqC.SetWestLimit();
    }

    public void DiSEqCForceLimit(int cardId, bool onOff)
    {
      if (ValidateTvControllerParams(cardId))
        return;
      _cards[cardId].DisEqC.EnableEastWestLimits(onOff);
    }

    public void DiSEqCDriveMotor(int cardId, DiSEqCDirection direction, byte numberOfSteps)
    {
      if (ValidateTvControllerParams(cardId))
        return;
      _cards[cardId].DisEqC.DriveMotor(direction, numberOfSteps);
    }

    public void DiSEqCStorePosition(int cardId, byte position)
    {
      if (ValidateTvControllerParams(cardId))
        return;
      _cards[cardId].DisEqC.StoreCurrentPosition(position);
    }

    public void DiSEqCGotoReferencePosition(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return;
      _cards[cardId].DisEqC.GotoReferencePosition();
    }

    public void DiSEqCGotoPosition(int cardId, byte position)
    {
      if (ValidateTvControllerParams(cardId))
      {
        return;
      }

      _cards[cardId].DisEqC.GotoStoredPosition(position);
    }
    #endregion

    /// <summary>
    /// Stops the grabbing epg.
    /// </summary>
    /// <param name="user">User</param>
    public void StopGrabbingEpg(User user)
    {
      if (ValidateTvControllerParams(user))
      {
        return;
      }

      _cards[user.CardId].Epg.Stop(user);
    }

    public List<string> ServerIpAdresses
    {
      get
      {
        List<string> ipadresses = new List<string>();
        string localHostName = Dns.GetHostName();
        IPHostEntry local = Dns.GetHostEntry(localHostName);
        foreach (IPAddress ipaddress in local.AddressList)
        {
          if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
          {
            ipadresses.Add(ipaddress.ToString());
          }
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
      if (ValidateTvControllerParams(cardId))
      {
        return null;
      }
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
    /// Returns a dictionary of channels that are timeshfiting and recording.
    /// </summary>
    public Dictionary<int, ChannelState> GetAllTimeshiftingAndRecordingChannels()
    {
      Dictionary<int, ChannelState> result = new Dictionary<int, ChannelState>();
      Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();

      while (enumerator.MoveNext())
      {
        KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
        ITvCardHandler tvcard = keyPair.Value;
        User[] users = tvcard.Users.GetUsers();

        if (users == null || users.Length == 0)
          continue;

        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          string tmpChannel = tvcard.CurrentChannelName(ref user);
          if (string.IsNullOrEmpty(tmpChannel))
            continue;
          int IdChannel = tvcard.CurrentDbChannel(ref user);
          if (tvcard.Recorder.IsRecording(ref user))
          {
            if (result.ContainsKey(IdChannel))
            {
              result.Remove(IdChannel);
            }
            result.Add(IdChannel, ChannelState.recording);
          }
          else if (tvcard.TimeShifter.IsTimeShifting(ref user))
          {
            if (!result.ContainsKey(IdChannel))
            {
              result.Add(IdChannel, ChannelState.timeshifting);
            }
          }
        }
      }
      return result;
    }

    /// <summary>
    /// Fetches all channels with backbuffer
    /// </summary>
    /// <param name="currentRecChannels"></param>
    /// <param name="currentTSChannels"></param>
    /// <param name="currentUnavailChannels"></param>
    /// <param name="currentAvailChannels"></param>
    public void GetAllRecordingChannels(out List<int> currentRecChannels, out List<int> currentTSChannels, out List<int> currentUnavailChannels, out List<int> currentAvailChannels)
    {
      currentRecChannels = new List<int>();
      currentTSChannels = new List<int>();
      currentUnavailChannels = new List<int>();
      currentAvailChannels = new List<int>();

      Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();

      while (enumerator.MoveNext())
      {
        KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
        ITvCardHandler tvcard = keyPair.Value;
        User[] users = tvcard.Users.GetUsers();

        if (users == null || users.Length == 0)
          continue;

        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          string tmpChannel = tvcard.CurrentChannelName(ref user);
          if (string.IsNullOrEmpty(tmpChannel))
            continue;
          if (tvcard.Recorder.IsRecording(ref user))
          {
            currentRecChannels.Add(tvcard.CurrentDbChannel(ref user));
          }
          else if (tvcard.TimeShifter.IsTimeShifting(ref user))
          {
            currentTSChannels.Add(tvcard.CurrentDbChannel(ref user));
          }
          else
          {
            ChannelState cState = GetChannelState(tvcard.CurrentDbChannel(ref user), user);
            if (cState == ChannelState.tunable)
            {
              currentAvailChannels.Add(tvcard.CurrentDbChannel(ref user));
            }
            else
            {
              currentUnavailChannels.Add(tvcard.CurrentDbChannel(ref user));
            }
          }
        }
      }
    }

    /// <summary>
    /// Fetches all channel states for a specific user (cached - faster)
    /// </summary>    
    /// <param name="user"></param>      
    public Dictionary<int, ChannelState> GetAllChannelStatesCached(User user)
    {
      if (user == null)
      {
        return null;
      }

      User[] users = _cards[user.CardId].Users.GetUsers();

      if (users != null)
      {
        for (int i = 0; i < users.Length; i++)
        {
          User u = users[i];

          if (u.Name.Equals(user.Name))
          {
            return u.ChannelStates;
          }
        }
      }

      return null;
    }


    /// <summary>
    /// Fetches all channel states for a specific group
    /// </summary>
    /// <param name="idGroup"></param>    
    /// <param name="user"></param>        
    public Dictionary<int, ChannelState> GetAllChannelStatesForGroup(int idGroup, User user)
    {
      if (idGroup < 1)
      {
        return null;
      }

      if (user == null)
      {
        return null;
      }

      TvBusinessLayer layer = new TvBusinessLayer();
      IList<Channel> tvChannelList = layer.GetTVGuideChannelsForGroup(idGroup);

      if (tvChannelList == null || tvChannelList.Count == 0)
        return null;

      Dictionary<int, ChannelState> channelStatesList = new Dictionary<int, ChannelState>();
      ChannelStates channelStates = new ChannelStates();

      if (channelStates != null)
      {
        channelStatesList = channelStates.GetChannelStates(_cards, tvChannelList, ref user, true, this);
      }

      return channelStatesList;
    }



    /// <summary>
    /// Checks if a channel is tunable/tuned or not...
    /// </summary>
    /// <param name="idChannel">Channel id</param>
    /// <param name="user">User</param>
    /// <returns>
    ///       <c>channel state tunable|nottunable</c>.
    /// </returns>
    public ChannelState GetChannelState(int idChannel, User user)
    {
      if (user == null)
        return ChannelState.nottunable;

      Channel dbchannel = Channel.Retrieve(idChannel);

      //User anyUser = new User();
      TvResult viewResult;
      AdvancedCardAllocation allocation = new AdvancedCardAllocation();
      allocation.GetAvailableCardsForChannel(_cards, dbchannel, ref user, true, out viewResult, 0);
      ChannelState chanState = viewResult == TvResult.Succeeded ? ChannelState.tunable : ChannelState.nottunable;

      return chanState;
    }
    #endregion

    #region streaming

    public int StreamingPort
    {
      get
      {
        if (_streamer != null)
        {
          return _streamer.Port;
        }
        else
        {
          return 0;
        }
      }
    }

    public List<RtspClient> StreamingClients
    {
      get
      {
        if (_streamer == null)
          return new List<RtspClient>();
        return _streamer.Clients;
      }
    }

    public int ActiveStreams
    {
      get
      {
        int activeCount = 0;
        if (_streamer == null)
          return activeCount;
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

    #region quality control
    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsQualityControl(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return false;
      return _cards[cardId].Card.SupportsQualityControl;
    }
    /// <summary>
    /// Indicates if bit rate modes are supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsBitRateModes(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return false;
      IQuality qualityControl = _cards[cardId].Card.Quality;
      if (qualityControl != null)
      {
        return qualityControl.SupportsBitRateModes();
      }
      return false;
    }

    /// <summary>
    /// Indicates if peak bit rate mode is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsPeakBitRateMode(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return false;
      IQuality qualityControl = _cards[cardId].Card.Quality;
      if (qualityControl != null)
      {
        return qualityControl.SupportsPeakBitRateMode();
      }
      return false;
    }


    /// <summary>
    /// Indicates if bit rate control is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsBitRate(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return false;
      IQuality qualityControl = _cards[cardId].Card.Quality;
      if (qualityControl != null)
      {
        return qualityControl.SupportsBitRate();
      }
      return false;
    }

    /// <summary>
    /// Reloads the configuration of quality control for the given card
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    public void ReloadCardConfiguration(int cardId)
    {
      if (ValidateTvControllerParams(cardId) || !SupportsQualityControl(cardId))
        return;
      _cards[cardId].Card.ReloadCardConfiguration();
    }

    /// <summary>
    /// Gets the current quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    public QualityType GetQualityType(int cardId)
    {
      if (ValidateTvControllerParams(cardId) || !SupportsQualityControl(cardId))
        return QualityType.Default;
      IQuality qualityControl = _cards[cardId].Card.Quality;
      if (qualityControl != null)
      {
        return qualityControl.QualityType;
      }
      return QualityType.Default;
    }
    /// <summary>
    /// Sets the quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="qualityType">The new quality type</param>
    public void SetQualityType(int cardId, QualityType qualityType)
    {
      if (ValidateTvControllerParams(cardId) || !SupportsQualityControl(cardId))
        return;
      IQuality qualityControl = _cards[cardId].Card.Quality;
      if (qualityControl != null)
      {
        qualityControl.QualityType = qualityType;
      }
    }

    /// <summary>
    /// Gets the current bitrate mdoe
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    public VIDEOENCODER_BITRATE_MODE GetBitRateMode(int cardId)
    {
      if (ValidateTvControllerParams(cardId) || !SupportsQualityControl(cardId))
        return VIDEOENCODER_BITRATE_MODE.Undefined;
      IQuality qualityControl = _cards[cardId].Card.Quality;
      if (qualityControl != null)
      {
        return qualityControl.BitRateMode;
      }
      return VIDEOENCODER_BITRATE_MODE.Undefined;
    }

    /// <summary>
    /// Sets the bitrate mode
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="bitRateMode">The new bitrate mdoe</param>
    public void SetBitRateMode(int cardId, VIDEOENCODER_BITRATE_MODE bitRateMode)
    {
      if (ValidateTvControllerParams(cardId) || !SupportsQualityControl(cardId))
        return;
      IQuality qualityControl = _cards[cardId].Card.Quality;
      if (qualityControl != null)
      {
        qualityControl.BitRateMode = bitRateMode;
      }
    }

    #endregion

    #endregion

    #region private members

    private void UpdateChannelStatesForUsers()
    {
      //System.Diagnostics.Debugger.Launch();
      // this section makes sure that all users are updated in regards to channel states.      
      ChannelStates channelStates = new ChannelStates();

      if (channelStates != null)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        IList<ChannelGroup> groups = ChannelGroup.ListAll();

        // populating _tvChannelListGroups is only done once as is therefor cached.
        if (_tvChannelListGroups == null)
        {
          foreach (ChannelGroup group in groups)
          {
            // we will only update user created groups, since it will often have fewer channels than "all channels"
            // going into "all channels" group in mini EPG will always be slower.
            if (group.GroupName.Equals(TvConstants.TvGroupNames.AllChannels))
              continue;

            if (_tvChannelListGroups == null)
            {
              _tvChannelListGroups = layer.GetTVGuideChannelsForGroup(group.IdGroup);
            }
            else
            {
              IList<Channel> tvChannelList = layer.GetTVGuideChannelsForGroup(group.IdGroup);

              foreach (Channel ch in tvChannelList)
              {
                bool exists = _tvChannelListGroups.Exists(delegate(Channel c) { return c.IdChannel == ch.IdChannel; });                
                if (!exists)
                {
                  _tvChannelListGroups.Add(ch);
                }
              }
            }
          }
        }

        channelStates.SetChannelStates(_cards, _tvChannelListGroups, true, this);
      }
    }

    private void HeartBeatMonitor()
    {
      Log.Info("Controller: Heartbeat Monitor initiated, max timeout allowed is {0} sec.", HEARTBEAT_MAX_SECS_EXCEED_ALLOWED);
      while (true)
      {
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

                // more than 30 seconds have elapsed since last heartbeat was received. lets kick the client
                if (ts.TotalSeconds < (-1 * HEARTBEAT_MAX_SECS_EXCEED_ALLOWED))
                {
                  Log.Write("Controller: Heartbeat Monitor - kicking idle user {0}", tmpUser.Name);
                  StopTimeShifting(ref tmpUser, TvStoppedReason.HeartBeatTimeOut);
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
      if (ValidateTvControllerParams(user) || ValidateTvControllerParams(cardId))
      {
        return false;
      }
      return _cards[cardId].Users.IsOwner(user);
    }

    /// <summary>
    /// Removes the user from other cards then the one specified
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user.</param>
    public void RemoveUserFromOtherCards(int cardId, User user)
    {
      if (ValidateTvControllerParams(user) || ValidateTvControllerParams(cardId))
      {
        return;
      }

      Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();
      ITVCard card = _cards[cardId].Card;
      while (enumerator.MoveNext())
      {
        KeyValuePair<int, ITvCardHandler> key = enumerator.Current;
        if (key.Key == cardId)
          continue;
        if (key.Value.Card.Context == card.Context)
          continue;
        key.Value.Users.RemoveUser(user);
      }
    }

    public bool SupportsSubChannels(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
      {
        return false;
      }

      return _cards[cardId].SupportsSubChannels;
    }

    public void HeartBeat(User user)
    {
      if (ValidateTvControllerParams(user))
      {
        return;
      }

      _cards[user.CardId].Users.HeartBeartUser(user);
    }

    /// <summary>
    /// Tune the card to the specified channel
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="channel">The channel.</param>
    /// <param name="dbChannel">The db channel</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    TvResult CardTune(ref User user, IChannel channel, Channel dbChannel)
    {
      if (ValidateTvControllerParams(user))
      {
        return TvResult.CardIsDisabled;
      }

      try
      {
        if (_cards[user.CardId].DataBaseCard.Enabled == false)
          return TvResult.CardIsDisabled;
        //if (!CardPresent(user.CardId)) return TvResult.CardIsDisabled;

        //on tune we need to remember to remove the previous subchannel before tuning the next one.
        // but only if the subchannel is free, meaning not recording and no other users attached.                  
        if (_cards[user.CardId].Card.SubChannels.Length > 0)
        {
          bool isRec = _cards[user.CardId].Recorder.IsRecordingAnyUser();
          if (!isRec)
          {
            TvCardContext context = (TvCardContext)_cards[user.CardId].Card.Context;
            User[] users = context.Users;

            int userSubCh = -1;
            int userChannelId = -1;
            bool otherUserFoundOnSameCh = false;

            //lets find the current user, if he has a subchannel and what channel it is.
            foreach (User userObj in users)
            {
              if (userObj.Name.Equals(user.Name))
              {
                if (userSubCh == -1)
                {
                  userChannelId = userObj.IdChannel;
                  userSubCh = userObj.SubChannel;
                  break;
                }
              }
            }

            //lets find if any other users are sharing that same subchannel/channel
            if (userChannelId > -1)
            {
              foreach (User userObj in users)
              {
                if (!userObj.Name.Equals(user.Name))
                {
                  if (userObj.IdChannel == userChannelId)
                  {
                    otherUserFoundOnSameCh = true;
                    break;
                  }
                }
              }
            }

            if (userSubCh > -1)
            {
              if (otherUserFoundOnSameCh)
              {
                _cards[user.CardId].Card.FreeSubChannelContinueGraph(userSubCh, true);
              }
              else
              {
                _cards[user.CardId].Card.FreeSubChannelContinueGraph(userSubCh);
              }
            }

          }
        }

        Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(user), user, channel));
        TvResult result = _cards[user.CardId].Tuner.CardTune(ref user, channel, dbChannel);
        Log.Info("Controller: {0} {1} {2}", user.Name, user.CardId, user.SubChannel);

        /*
        if (result == TvResult.Succeeded)
        {
          RemoveUserFromOtherCards(user.CardId, user);
        }
        */
        return result;
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(user), user, channel));
      }
    }

    /// <summary>
    /// deletes time shifting files left in the specified folder.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="fileName">Name of the file.</param>
    static void CleanTimeShiftFiles(string folder, string fileName)
    {
      try
      {
        Log.Write(@"Controller: delete timeshift files {0}\{1}", folder, fileName);
        string[] files = Directory.GetFiles(folder);
        for (int i = 0; i < files.Length; ++i)
        {
          if (files[i].IndexOf(fileName) >= 0)
          {
            try
            {
              Log.Write("Controller:   delete {0}", files[i]);
              File.Delete(files[i]);
            } catch (Exception e)
            {
              Log.Error("Controller: Error on delete in CleanTimeshiftFiles");
              Log.Error("{0}", e);
            }
          }
        }
      } catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    public bool IsTunedToTransponder(int cardId, IChannel transponder)
    {
      if (ValidateTvControllerParams(cardId))
      {
        return false;
      }

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
        {
          OnTvServerEvent(sender, args);
        }
      } 
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    /// <summary>
    /// returns a virtual card for the card specified.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns></returns>
    VirtualCard GetVirtualCard(User user)
    {
      if (ValidateTvControllerParams(user))
      {
        return null;
      }

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
          // Do we have a card or is it disposed?
          if (_cards[cardId] == null)
            continue;
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
        if (_scheduler != null && _scheduler.IsTimeToRecord(DateTime.Now))
        {
          //Log.Debug("TVController.CanSuspend: IsTimeToRecord finished -> cannot suspend" );
          return false;
        }
        //Log.Debug("TVController.CanSuspend: finished, can suspend");

        return true;
      }
    }

    #region private methods

    private bool ValidateTvControllerParams(int cardId, bool checkCardPresent)
    {
      if (cardId < 0 || !_cards.ContainsKey(cardId) || (checkCardPresent && !CardPresent(cardId)))
      {
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);
        Log.Error("TVController:" + sf.GetMethod().Name + " - incorrect parameters used! cardId {0} _cards.ContainsKey(cardId) == {1} CardPresent {2}", cardId, _cards.ContainsKey(cardId), CardPresent(cardId));
        Log.Error("{0}", st);
        return true;
      }
      return false;
    }

    private bool ValidateTvControllerParams(int cardId)
    {
      return ValidateTvControllerParams(cardId, true);
    }

    private bool ValidateTvControllerParams(User user)
    {
      if (user == null || user.CardId < 0 || !_cards.ContainsKey(user.CardId) || (!CardPresent(user.CardId)))
      {
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);

        if (user != null)
        {
          Log.Error("TVController:" + sf.GetMethod().Name + " - incorrect parameters used! user {0} cardId {1} _cards.ContainsKey(cardId) == {2} CardPresent(cardId) {3}", user, user.CardId, _cards.ContainsKey(user.CardId), CardPresent(user.CardId));
        }
        else
        {
          Log.Error("TVController:" + sf.GetMethod().Name + " - incorrect parameters used! user NULL");
        }
        Log.Error("{0}", st);
        return true;
      }
      return false;
    }

    private static bool ValidateTvControllerParams(IChannel channel)
    {
      if (channel == null)
      {
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);

        Log.Error("TVController:" + sf.GetMethod().Name + " - incorrect parameters used! channel NULL");
        Log.Error("{0}", st);
        return true;
      }
      return false;
    }

    #endregion


    #region ICiMenuCallbacks Member

    CiMenu curMenu;

    /// <summary>
    /// Checks menu state; If it's ready, fire event to "push" it to client
    /// </summary>
    private void CheckForCallback()
    {
      if (curMenu != null)
      {
        if (curMenu.State == CiMenuState.Ready || curMenu.State == CiMenuState.NoChoices || curMenu.State == CiMenuState.Request)
        {
          if (s_ciMenu != null)
          {
            s_ciMenu(curMenu); // pass to eventhandlers
          }
          else
          {
            Log.Debug("CI menu received but no listeners available");
          }
        }
      }
    }
    /// <summary>
    /// [TsWriter Interface Callback] Called on initialization of an menu. Options follow in OnCiMenuChoice
    /// </summary>
    /// <param name="lpszTitle">Title</param>
    /// <param name="lpszSubTitle">Subtitle</param>
    /// <param name="lpszBottom">Bottomtext</param>
    /// <param name="nNumChoices">number of choices</param>
    /// <returns>0</returns>
    public int OnCiMenu(string lpszTitle, string lpszSubTitle, string lpszBottom, int nNumChoices)
    {
      curMenu = new CiMenu(lpszTitle, lpszSubTitle, lpszBottom, CiMenuState.Opened);
      curMenu.NumChoices = nNumChoices;
      if (nNumChoices == 0)
        curMenu.State = CiMenuState.NoChoices;

      CheckForCallback();
      return 0;
    }

    /// <summary>
    /// [TsWriter Interface Callback] Sets the choices to opening dialog
    /// </summary>
    /// <param name="nChoice">number of choice (0 based)</param>
    /// <param name="lpszText">title of choice</param>
    /// <returns>0</returns>
    public int OnCiMenuChoice(int nChoice, string lpszText)
    {
      if (curMenu == null)
      {
        Log.Debug("Error in OnCiMenuChoice: menu choice sent before menu started");
        return 0;
      }
      curMenu.AddEntry(nChoice+1, lpszText); // choices for display +1 
      if (nChoice + 1 == curMenu.NumChoices)
      {
        curMenu.State = CiMenuState.Ready;
        CheckForCallback();
      }
      return 0;
    }

    /// <summary>
    /// [TsWriter Interface Callback] call to close display
    /// </summary>
    /// <param name="nDelay">delay in (ms?)</param>
    /// <returns>0</returns>
    public int OnCiCloseDisplay(int nDelay)
    {
      // sometimes first a "Close" is sent, even no others callbacks were done before 
      if (curMenu == null)
      {
        curMenu = new CiMenu(String.Empty, String.Empty, String.Empty, CiMenuState.Closed);
      }
      curMenu.State = CiMenuState.Closed;
      CheckForCallback();
      return 0;
    }

    /// <summary>
    /// [TsWriter Interface Callback] Opens a input request
    /// </summary>
    /// <param name="bBlind">?</param>
    /// <param name="nAnswerLength">expected maximum length of answer</param>
    /// <param name="lpszText">Title of input</param>
    /// <returns>0</returns>
    public int OnCiRequest(bool bBlind, uint nAnswerLength, string lpszText)
    {
      if (curMenu == null)
      {
        curMenu = new CiMenu(String.Empty, String.Empty, String.Empty, CiMenuState.Request);
      }
      curMenu.State = CiMenuState.Request;
      curMenu.Request(lpszText, (int)nAnswerLength, bBlind);
      CheckForCallback();
      return 0;
    }

    #endregion
  }
}
