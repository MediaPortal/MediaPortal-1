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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Xml;
using TvControl;
using TvDatabase;
using TvEngine.Events;
using TvLibrary;
using TvLibrary.Epg;
using TvLibrary.Implementations;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.Hybrid;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvLibrary.Streaming;

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

    private readonly Dictionary<int, bool> _cardPresent = new Dictionary<int, bool>();
    private readonly TvBusinessLayer _layer = new TvBusinessLayer();
    private readonly ICardAllocation _cardAllocation;
    private readonly ChannelStates _channelStates;

    /// <summary>
    /// EPG grabber for DVB
    /// </summary>
    private EpgGrabber _epgGrabber;

    /// <summary>
    /// Recording scheduler
    /// </summary>
    private Scheduler _scheduler;

    /// <summary>
    /// RTSP Streaming Server
    /// </summary>
    private RtspStreaming _streamer;

    /// <summary>
    /// Indicates if we're the master server or not
    /// </summary>
    private bool _isMaster;

    private Thread heartBeatMonitorThread;

    /// <summary>
    /// Reference to our server
    /// </summary>
    private Server _ourServer;

    /// <summary>
    private TvCardCollection _localCardCollection;

    /// <summary>
    /// Indicates how many free cards to try for timeshifting
    /// </summary>
    private int _maxFreeCardsToTry;

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

    private Dictionary<int, ITvCardHandler> _cards;

    /// 
    // contains a cached copy of all the channels in the user defined groups (excl. the all channels group)
    // used to speedup "mini EPG" channel state creation.
    private List<Channel> _tvChannelListGroups;

    #region CI Menu Event handling

    /// <summary>
    /// Flag that is that to true when a users opens CI menu interactive.
    /// It is used to filter out unwanted, unrequested CI callbacks.
    /// </summary>
    private bool IsCiMenuInteractive = false;

    /// <summary>
    /// Remember the number of currently attached CI menu supporting card.
    /// </summary>
    private int ActiveCiMenuCard = -1;

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
      _channelStates = new ChannelStates(_layer, this);
      _cardAllocation = new AdvancedCardAllocation(_layer, this);
    }

    public Dictionary<int, ITvCardHandler> CardCollection
    {
      get { return _cards; }
    }

    /// <summary>
    /// Determines whether the specified card is the local pc or not.
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>
    /// 	<c>true</c> if the specified host name is local; otherwise, <c>false</c>.
    /// </returns>
    private bool IsLocal(int cardId)
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
      {
        IsCiMenuInteractive = true; // user action
        return _cards[cardId].CiMenuActions.EnterCIMenu();
      }
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
      if (_cards[cardId].CiMenuActions != null)
      {
        IsCiMenuInteractive = false; // user action ended by wanted close
        return _cards[cardId].CiMenuActions.CloseCIMenu();
      }
      return false;
    }

    /// <summary>
    /// Sends a menu answer back to CAM
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="Cancel">true to cancel request</param>
    /// <param name="Answer">answer string</param>
    /// <returns></returns>
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
        ActiveCiMenuCard = cardId;
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
    private static bool IsLocal(string hostName)
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
    public bool IsCardInUse(int cardId, out IUser user)
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
    public IUser GetUserForCard(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return null;

      IUser user;
      _cards[cardId].Users.IsLocked(out user);
      return user;
    }

    /// <summary>
    /// Locks the card for the specified user
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="user">The user.</param>
    public void LockCard(int cardId, IUser user)
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
    public void UnlockCard(IUser user)
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
          }
          catch (Exception)
          {
            Log.Error("Controller: Error while deinit TvServer in Init");
          }

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
    private bool InitController()
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
        Log.Info(@"{0}\gentle.config", PathManager.GetDataPath);
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
        }
        catch (Exception ex)
        {
          Log.Error("Controller: Failed to fetch tv servers from database - {0}",
                    Utils.BlurConnectionStringPassword(ex.Message));
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
            Log.Error(
              "Controller: sorry, master/slave server setups are not supported. Since there is already another server in the db, we exit here.");
            return false;
          }
        }
        _isMaster = _ourServer.IsMaster;

        //enumerate all tv cards in this pc...        
        _maxFreeCardsToTry = Int32.Parse(_layer.GetSetting("timeshiftMaxFreeCardsToTry", "0").Value);

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
            _layer.AddCard(_localCardCollection.Cards[i].Name, _localCardCollection.Cards[i].DevicePath, _ourServer);
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
                Card cardDB = _layer.GetCardByDevicePath(_localCardCollection.Cards[cardNumber].DevicePath);

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
                      try
                      {
                        Log.Info("Controller: preloading card :{0}", card.Name);
                        card.BuildGraph();
                        if (unknownCard is TvCardAnalog)
                        {
                          ((TvCardAnalog)unknownCard).ReloadCardConfiguration();
                        }
                      }
                      catch (Exception ex)
                      {
                        Log.Error("failed to preload card '{0}', ex = {1}", card.Name, ex);
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

              // Fix mantis 0002790: Bad behavior when card count for IPTV = 0 
              if (dbsCard.Name.StartsWith("MediaPortal IPTV Source Filter"))
              {
                CardRemove(dbsCard.IdCard);
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
              TimeShiftPath = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer",
                                            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
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
                    delFile = (fInfo.Extension.ToUpperInvariant().IndexOf(".TS") == 0) &&
                              (fInfo.Name.ToUpperInvariant().IndexOf("TSBUFFER") > 0);
                  }
                  if (delFile)
                    File.Delete(fInfo.FullName);
                }
                catch (IOException) {}
              }
            }
          }
          catch (Exception exd)
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

        SetupHeartbeatThread();
        ExecutePendingDeletions();

        // Re-evaluate program states
        Log.Info("Controller: recalculating program states");
        TvDatabase.Program.ResetAllStates();
        Schedule.SynchProgramStatesForAll();
      }
      catch (Exception ex)
      {
        Log.Write("TvControllerException: {0}\r\n{1}", ex.ToString(), ex.StackTrace);
        return false;
      }

      Log.Info("Controller: initalized");
      return true;
    }

    private void SetupHeartbeatThread()
    {
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
          if (!Service1.HasThreadCausedAnUnhandledException(heartBeatMonitorThread))
          {
            if (heartBeatMonitorThread.IsAlive)
            {
              Log.Info("Controller: HeartBeat monitor stopped...");
              try
              {
                heartBeatMonitorThread.Abort();
              }
              catch (Exception) {}
            }
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
        StopEPGgrabber();
        _epgGrabber = null;        

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
      get { return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion; }
    }

    /// <summary>
    /// Gets the server.
    /// </summary>
    /// <value>The server.</value>
    public int IdServer
    {
      get { return _ourServer.IdServer; }
    }

    /// <summary>
    /// Gets the total number of cards installed.
    /// </summary>
    /// <value>Number which indicates the cards installed</value>
    public int Cards
    {
      get { return _cards.Count; }
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
      bool cardPresent = false;
      if (cardId > 0)
      {
        bool cardPresentFound = _cardPresent.TryGetValue(cardId, out cardPresent);

        if (!cardPresentFound)
        {
          if (_cards.ContainsKey(cardId))
          {
            string devicePath = _cards[cardId].Card.DevicePath;
            if (devicePath.Length > 0)
            {
              // Remove it from the local card collection
              cardPresent =
                (from t in _localCardCollection.Cards where t.DevicePath == devicePath select t.CardPresent).
                  FirstOrDefault();
            }
          }
          _cardPresent.Add(cardId, cardPresent);
        }
      }
      return cardPresent;
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
    public IChannel CurrentChannel(ref IUser user)
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
    public int CurrentDbChannel(ref IUser user)
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
    public string CurrentChannelName(ref IUser user)
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
    public string RecordingFileName(ref IUser user)
    {
      if (ValidateTvControllerParams(user))
        return "";
      return _cards[user.CardId].Recorder.FileName(ref user);
    }

    public string TimeShiftFileName(ref IUser user)
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
    public bool TimeShiftGetCurrentFilePosition(ref IUser user, ref Int64 position, ref long bufferId)
    {
      if (ValidateTvControllerParams(user))
        return false;
      return _cards[user.CardId].TimeShifter.GetCurrentFilePosition(ref user, ref position, ref bufferId);
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(ref IUser user)
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
    public IVideoStream GetCurrentVideoStream(IUser user)
    {
      if (ValidateTvControllerParams(user))
        return null;
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
        IUser user = new User();
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
    public bool IsAnyCardRecordingOrTimeshifting(IUser userTS, out bool isUserTS, out bool isAnyUserTS, out bool isRec)
    {
      isUserTS = false;
      isAnyUserTS = false;
      isRec = false;

      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      while (en.MoveNext())
      {
        ITvCardHandler card = en.Current.Value;
        IUser user = new User();
        user.CardId = card.DataBaseCard.IdCard;

        if (!isRec)
        {
          isRec = card.Recorder.IsAnySubChannelRecording;
        }
        if (!isUserTS)
        {
          isUserTS = card.TimeShifter.IsTimeShifting(ref userTS);
        }

        IUser[] users = card.Users.GetUsers();
        if (users == null)
          continue;
        if (users.Length == 0)
          continue;
        for (int i = 0; i < users.Length; ++i)
        {
          IUser anyUser = users[i];

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
    public bool IsRecording(int idChannel, out VirtualCard card)
    {
      card = null;
      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      while (en.MoveNext())
      {
        ITvCardHandler tvcard = en.Current.Value;
        IUser[] users = tvcard.Users.GetUsers();
        if (users == null)
          continue;
        if (users.Length == 0)
          continue;
        for (int i = 0; i < users.Length; ++i)
        {
          IUser user = users[i];
          if (tvcard.CurrentChannelName(ref user) == null)
            continue;
          if (tvcard.CurrentDbChannel(ref user) == idChannel)
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
        IUser[] users = tvcard.Users.GetUsers();
        if (users == null)
          continue;
        if (users.Length == 0)
          continue;
        for (int i = 0; i < users.Length; ++i)
        {
          IUser user = users[i];
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
      IUser recUser = null;
      while (en.MoveNext())
      {
        tvcard = en.Current.Value;
        IUser[] users = tvcard.Users.GetUsers();
        if (users == null)
          continue;
        if (users.Length == 0)
          continue;
        for (int i = 0; i < users.Length; ++i)
        {
          IUser user = users[i];
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
    public bool IsRecording(ref IUser user)
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
    public bool IsGrabbingTeletext(IUser user)
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
    public bool HasTeletext(IUser user)
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
    public TimeSpan TeletextRotation(IUser user, int pageNumber)
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
    public DateTime TimeShiftStarted(IUser user)
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
    public DateTime RecordingStarted(IUser user)
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
    public void CopyTimeShiftFile(Int64 position1, string bufferFile1, Int64 position2, string bufferFile2,
                                  string recordingFile)
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
    public bool IsScrambled(ref IUser user)
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
    public TvResult Scan(ref IUser user, IChannel channel, int idChannel)
    {
      if (ValidateTvControllerParams(user) || ValidateTvControllerParams(channel))
      {
        return TvResult.UnknownError;
      }
      try
      {
        int cardId = user.CardId;
        ITvCardHandler cardHandler = _cards[cardId];
        if (cardHandler.DataBaseCard.Enabled == false)
        {
          return TvResult.CardIsDisabled;
        }
        FireZapChannelEvent(ref user, channel);

        TvResult res = cardHandler.Tuner.Scan(ref user, channel, idChannel);

        return res;
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(user), (User)user, channel));
      }
    }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    public TvResult Tune(ref IUser user, IChannel channel, int idChannel)
    {
      ITvCardHandler tvCardHandler;
      TvResult result = TvResult.UnknownError;

      if (CardCollection.TryGetValue(user.CardId, out tvCardHandler))
      {
        ICardTuneReservationTicket ticket = null;
        ICardReservation cardreservationImpl = new CardReservationTimeshifting(this);
        try
        {
          ticket = cardreservationImpl.RequestCardTuneReservation(tvCardHandler, channel, user);
          result = Tune(ref user, channel, idChannel, ticket); 
        }
        catch(Exception)
        {
          CardReservationHelper.CancelCardReservation(tvCardHandler, ticket);
          throw;
        }        
      }

      return result;
    }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="ticket">card reservation ticket</param>
    /// <returns>true if succeeded</returns>
    public TvResult Tune(ref IUser user, IChannel channel, int idChannel, object ticket)
     {
       ICardReservation cardResImpl = new CardReservationTimeshifting(this);
       return Tune(ref user, channel, idChannel, ticket, cardResImpl);
     }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="ticket">card reservation ticket</param>
    /// <param name="cardResImpl"></param>
    /// <returns>true if succeeded</returns>
    public TvResult Tune(ref IUser user, IChannel channel, int idChannel, object ticket, object cardResImpl)
    {
      if (ValidateTvControllerParams(user) || ValidateTvControllerParams(channel))
      {
        return TvResult.UnknownError;
      }
      try
      {
        int cardId = user.CardId;
        ITvCardHandler cardHandler = _cards[cardId];
        if (cardHandler.DataBaseCard.Enabled == false)
        {
          return TvResult.CardIsDisabled;
        }
        FireZapChannelEvent(ref user, channel);

        ICardTuneReservationTicket resTicket = ticket as ICardTuneReservationTicket;
        ICardReservation resCardResImpl = cardResImpl as ICardReservation;
        if (resTicket != null && resCardResImpl != null)
        {
          TvResult res = resCardResImpl.Tune(cardHandler, ref user, channel, idChannel, resTicket);
          return res;
        }
        return TvResult.UnknownError;
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(user), (User)user, channel));
      }
    }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    private void FireZapChannelEvent(ref IUser user, IChannel channel)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(user), (User)user, channel));
    }

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="onOff">turn on/off teletext grabbing</param>
    public void GrabTeletext(IUser user, bool onOff)
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
    public byte[] GetTeletextPage(IUser user, int pageNumber, int subPageNumber)
    {
      if (ValidateTvControllerParams(user))
        return new byte[] {1};
      return _cards[user.CardId].Teletext.GetTeletextPage(user, pageNumber, subPageNumber);
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    public int SubPageCount(IUser user, int pageNumber)
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
    public int GetTeletextRedPageNumber(IUser user)
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
    public int GetTeletextGreenPageNumber(IUser user)
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
    public int GetTeletextYellowPageNumber(IUser user)
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
    public int GetTeletextBluePageNumber(IUser user)
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
    public TvResult StartTimeShifting(ref IUser user, ref string fileName)
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
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      _cards[cardId].DataBaseCard.ReferencedServer().HostName);
            return TvResult.UnknownError;
          }
        }

        Fire(this, new TvServerEventArgs(TvServerEventType.StartTimeShifting, GetVirtualCard(user), (User)user));
        StopEPGgrabber();        

        bool isTimeShifting;
        try
        {
          isTimeShifting = _cards[cardId].TimeShifter.IsTimeShifting(ref user);
        }
        catch (Exception ex)
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

              //  Default to tv
              bool isTv = true;

              ITvSubChannel subChannel = _cards[cardId].Card.GetSubChannel(user.SubChannel);

              if (subChannel != null && subChannel.CurrentChannel != null)
                isTv = subChannel.CurrentChannel.IsTv;
              else
                Log.Error("SubChannel or CurrentChannel is null when starting streaming");

              RtspStream stream = new RtspStream(String.Format("stream{0}.{1}", cardId, user.SubChannel), fileName,
                                                 _cards[cardId].Card, isTv);
              _streamer.AddStream(stream);
            }
            else
            {
              Log.Write("Controller: streaming: file not found:{0}", fileName);
            }
          }
        }

        if (result == TvResult.Succeeded)
        {
          Log.Write("Controller: StartTimeShifting started on card:{0} to {1}", user.CardId, fileName);
        }

        return result;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return TvResult.UnknownError;
    }

    public void StopCard(IUser user)
    {
      if (ValidateTvControllerParams(user))
        return;
      _cards[user.CardId].StopCard(user);
    }

    public void PauseCard(IUser user)
    {
      if (ValidateTvControllerParams(user))
        return;
      _cards[user.CardId].PauseCard(user);
    }

    public bool StopTimeShifting(ref IUser user, TvStoppedReason reason)
    {
      if (ValidateTvControllerParams(user))
        return false;
      _cards[user.CardId].Users.SetTvStoppedReason(user, reason);
      return StopTimeShifting(ref user);
    }

    public TvStoppedReason GetTvStoppedReason(IUser user)
    {
      if (ValidateTvControllerParams(user))
        return TvStoppedReason.UnknownReason;

      try
      {
        if (_cards[user.CardId].DataBaseCard.Enabled == false)
          return TvStoppedReason.UnknownReason;
        //if (!CardPresent(user.CardId)) return TvStoppedReason.UnknownReason;

        return _cards[user.CardId].Users.GetTvStoppedReason(user);
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
    /// <param name="user">User</param>
    /// <returns></returns>
    public bool StopTimeShifting(ref IUser user)
    {
      if (ValidateTvControllerParams(user))
        return false;
      try
      {        
        int cardId = user.CardId;
        ITvCardHandler tvcard = _cards[cardId];
        if (tvcard.DataBaseCard.Enabled == false)
          return true;        

        if (false == tvcard.IsLocal)
        {
          try
          {
            if (IsGrabbingEpg(cardId))
            {              
              StopEPGgrabber();        
              // we need this, otherwise tvservice will hang in the event stoptimeshifting is called by heartbeat timeout function
            }
            RemoteControl.HostName = tvcard.DataBaseCard.ReferencedServer().HostName;
            return RemoteControl.Instance.StopTimeShifting(ref user);
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      tvcard.DataBaseCard.ReferencedServer().HostName);
            return false;
          }
        }

        HybridCard hybridCard = tvcard.Card as HybridCard;
        if (hybridCard != null)
        {
          if (!hybridCard.IsCardIdActive(cardId))
          {
            return true;
          }
        }
        
        if (false == tvcard.TimeShifter.IsTimeShifting(ref user))
          return true;
        Fire(this, new TvServerEventArgs(TvServerEventType.EndTimeShifting, GetVirtualCard(user), (User)user));

        if (tvcard.Recorder.IsRecording(ref user))
          return true;

        Log.Write("Controller: StopTimeShifting {0}", cardId);       
        return DoStopTimeShifting(ref user, cardId);                
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
                  
      return false;
    }

    private bool DoStopTimeShifting(ref IUser user, int cardId)
    {
      if (IsGrabbingEpg(cardId))
      {
        StopEPGgrabber();        
        // we need this, otherwise tvservice will hang in the event stoptimeshifting is called by heartbeat timeout function
      }
      ITvCardHandler tvcard = _cards[cardId];      
      ICardStopReservationTicket ticket = CardReservationHelper.RequestAndWaitForCardStopReservation(tvcard, user);
      bool stopped = false;
      if (ticket != null)
      {
        stopped = CardReservationHelper.Stop(tvcard, ref user, ticket);
        if (stopped)
        {
          //we must not stop streaming if subchannel is still in use.
          ITvCardContext context = (ITvCardContext)_cards[user.CardId].Card.Context;
          if (!context.ContainsUsersForSubchannel(user.SubChannel))
          {
            Log.Write("Controller:Timeshifting stopped on card:{0}", cardId);
            int subChannel = user.SubChannel;
            _streamer.Remove(String.Format("stream{0}.{1}", cardId, subChannel));
          }
          StartEPGgrabber();
          UpdateChannelStatesForUsers();
        }
      }
      return stopped;
    }

    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns></returns>
    public TvResult StartRecording(ref IUser user, ref string fileName)
    {
      if (ValidateTvControllerParams(user))
      {
        return TvResult.UnknownError;
      }
      StopEPGgrabber();        
      TvResult result = _cards[user.CardId].Recorder.Start(ref user, ref fileName);

      if (result == TvResult.Succeeded)
      {
        UpdateChannelStatesForUsers();
      }
      else
      {
        StartEPGgrabber();
      }

      return result;
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns></returns>
    public bool StopRecording(ref IUser user)
    {
      if (ValidateTvControllerParams(user))
      {
        return false;
      }
      bool result = _cards[user.CardId].Recorder.Stop(ref user);

      if (result)
      {
        UpdateChannelStatesForUsers();
      }
      StartEPGgrabber();
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
      _cards[cardId].SetParameters();
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
        bool result = RecordingFileHandler.DeleteRecordingOnDisk(rec.FileName);
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
      }
      catch (Exception)
      {
        return true;
      }
    }

    /// <summary>
    /// Deletes invalid recordings from database. A recording is invalid if the corresponding file no longer exists.
    /// </summary>
    public bool DeleteInvalidRecordings()
    {
      Log.Debug("Deleting invalid recordings");
      IList<Recording> itemlist = Recording.ListAll();
      bool foundInvalidRecording = false;
      foreach (Recording rec in itemlist)
      {
        if (!IsRecordingValid(rec.IdRecording))
        {
          try
          {
            rec.Delete();
          }
          catch (Exception e)
          {
            Log.Error("Controller: Can't delete invalid recording", e);
          }
          foundInvalidRecording = true;
        }
      }
      return foundInvalidRecording;
    }

    /// <summary>
    /// Deletes watched recordings from database.
    /// </summary>
    public bool DeleteWatchedRecordings(string currentTitle)
    {
      IList<Recording> itemlist = Recording.ListAll();
      bool foundWatchedRecordings = false;
      foreach (Recording rec in itemlist)
      {
        if (rec.TimesWatched > 0)
        {
          if (currentTitle == null || currentTitle == rec.Title)
          {
            DeleteRecording(rec.IdRecording);
            foundWatchedRecordings = true;
          }
        }
      }
      return foundWatchedRecordings;
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
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    #region audio streams

    public IAudioStream[] AvailableAudioStreams(IUser user)
    {
      if (ValidateTvControllerParams(user))
        return null;
      return _cards[user.CardId].Audio.Streams(user);
    }

    public IAudioStream GetCurrentAudioStream(IUser user)
    {
      if (ValidateTvControllerParams(user))
        return null;
      return _cards[user.CardId].Audio.GetCurrent(user);
    }

    public void SetCurrentAudioStream(IUser user, IAudioStream stream)
    {
      if (ValidateTvControllerParams(user))
        return;
      _cards[user.CardId].Audio.Set(user, stream);
    }

    public string GetStreamingUrl(IUser user)
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
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}",
                      _cards[user.CardId].DataBaseCard.ReferencedServer().HostName);
            return "";
          }
        }
        return String.Format("rtsp://{0}:{1}/stream{2}.{3}", _ourServer.HostName, _streamer.Port, user.CardId,
                             user.SubChannel);
      }
      catch (Exception)
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
          }
          catch (Exception)
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
        }
        catch (Exception)
        {
          Log.Error("Controller: Can't get recroding url - First catch");
        }
      }
      catch (Exception)
      {
        Log.Error("Controller: Can't get recroding url - Second catch");
      }
      return "";
    }

    /// <summary>
    /// Returns the contents of the chapters file (if any) for a recording 
    /// </summary>
    /// <param name="idRecording">id of recording</param>
    /// <returns>The contents of the chapters file of the recording</returns>
    public string GetRecordingChapters(int idRecording)
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
            return RemoteControl.Instance.GetRecordingChapters(idRecording);
          }
          catch (Exception)
          {
            Log.Error("Controller: unable to connect to slave controller at:{0}", recording.ReferencedServer().HostName);
            return "";
          }
        }
        try
        {
          string chapterFile = Path.ChangeExtension(recording.FileName, ".txt");
          if (File.Exists(chapterFile))
          {
            using (StreamReader chapters = new StreamReader(chapterFile))
            {
              return chapters.ReadToEnd();
            }
          }
        }
        catch (Exception)
        {
          Log.Error("Controller: Can't get recording chapters - First catch");
        }
      }
      catch (Exception)
      {
        Log.Error("Controller: Can't get recording chapters - Second catch");
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
    public int TimeShiftingWouldUseCard(ref IUser user, int idChannel)
    {
      if (user == null)
        return -1;

      Channel channel = Channel.Retrieve(idChannel);
      Log.Write("Controller: TimeShiftingWouldUseCard {0} {1}", channel.DisplayName, channel.IdChannel);

      try
      {
        user.Priority = UserFactory.GetDefaultPriority(user.Name);
        List<CardDetail> freeCards = _cardAllocation.GetFreeCardsForChannel(_cards, channel, ref user);
        if (freeCards.Count > 0)
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
    /// <param name="forceCardId">Indicated, if the card should be forced</param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref IUser user, int idChannel, out VirtualCard card, bool forceCardId)
    {
      bool cardChanged = false;
      return StartTimeShifting(ref user, idChannel, out card, forceCardId, out cardChanged);
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <param name="forceCardId">Indicated, if the card should be forced</param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    private TvResult StartTimeShifting(ref IUser user, int idChannel, out VirtualCard card, bool forceCardId,
                                      out bool cardChanged)
    {
      TvResult result = TvResult.UnknownError;
      card = null;
      cardChanged = false;
      if (user != null)
      {
        user.Priority = UserFactory.GetDefaultPriority(user.Name, user.Priority);
        Channel channel = Channel.Retrieve(idChannel);
        Log.Write("Controller: StartTimeShifting {0} {1}", channel.DisplayName, channel.IdChannel);
        StopEPGgrabber();

        ICollection<ICardTuneReservationTicket> tickets = null;
        try
        {
          var cardAllocationStatic = new AdvancedCardAllocationStatic(_layer, this);
          List<CardDetail> freeCardsForReservation = cardAllocationStatic.GetFreeCardsForChannel(_cards, channel, ref user);
          if (HasFreeCards(freeCardsForReservation))
          {
            tickets = IterateCardsUntilTimeshifting(
              ref user,
              channel,
              forceCardId,
              freeCardsForReservation,
              out cardChanged, ref result, ref card);
          }
          else
          {
            Log.Write("Controller: StartTimeShifting failed:{0} - no cards found during initial card allocation", result);
            result = AllCardsBusy(result);
          }
        }
        catch (Exception ex)
        {
          Log.Write(ex);
          result = TvResult.UnknownError;
        }
        finally
        {
          CardReservationHelper.CancelAllCardReservations(tickets, _cards);          
          if (!HasTvSucceeded(result))
          {
            StartEPGgrabber();
          }
        }
      }      
      return result;
    }

    private ICollection<ICardTuneReservationTicket> IterateCardsUntilTimeshifting(ref IUser user, Channel channel, bool forceCardId, ICollection<CardDetail> freeCardsForReservation, out bool cardChanged, ref TvResult result, ref VirtualCard card)
    {      
      cardChanged = false;
      VirtualCard initialCard = GetValidVirtualCard(user);
      string intialTimeshiftingFilename = GetIntialTimeshiftingFilename(initialCard);
      var cardResImpl = new CardReservationTimeshifting(this);
      ICollection<ICardTuneReservationTicket> tickets = null;
      ICollection<int> freeCardsIterated = UpdateCardsIteratedBasedOnForceCardId(user, forceCardId);
      int cardsIterated = 0;
      bool moreCardsAvailable = true;
      while (moreCardsAvailable && !HasTvSucceeded(result))
      {
        tickets = CardReservationHelper.RequestCardReservations(user, freeCardsForReservation, this, cardResImpl,
                                                                freeCardsIterated);
        if (HasTickets(tickets))
        {
          var cardAllocationTicket = new AdvancedCardAllocationTicket(_layer, this, tickets);
          ICollection<CardDetail> freeCards = cardAllocationTicket.UpdateFreeCardsForChannelBasedOnTicket(_cards,
                                                                                                          freeCardsForReservation,
                                                                                                          user, out result);
          CardReservationHelper.CancelCardReservationsExceedingMaxConcurrentTickets(tickets, freeCards, _cards);
          CardReservationHelper.CancelCardReservationsNotFoundInFreeCards(freeCardsForReservation, tickets, freeCards,
                                                                          _cards);
          int maxCards = GetMaxCards(freeCards);
          CardReservationHelper.CancelCardReservationsBasedOnMaxCardsLimit(tickets, freeCards, maxCards, _cards);
          UpdateFreeCardsIterated(freeCardsIterated, freeCards); //keep tracks of what cards have been iterated here.
          moreCardsAvailable = HasFreeCards(freeCards);
          if (moreCardsAvailable)
          {
            moreCardsAvailable = IterateTicketsUntilTimeshifting(
              ref user,
              channel,
              tickets,
              cardResImpl,
              intialTimeshiftingFilename,
              freeCards,
              maxCards, ref card, ref result, ref cardsIterated, out cardChanged);
          }
          else
          {
            result = AllCardsBusy(result);
            Log.Write("Controller: StartTimeShifting failed:{0}", result);
          }
        }
        else
        {
          result = AllCardsBusy(result);
          Log.Write("Controller: StartTimeShifting failed:{0} - no card reservation(s) could be made", result);          
          moreCardsAvailable = false;
        }
      } //end of while             
      return tickets;
    }

    private ICollection<int> UpdateCardsIteratedBasedOnForceCardId(IUser user, bool forceCardId)
    {
      ICollection<int> freeCardsIterated = new HashSet<int>();
      if (forceCardId)
      {
        foreach (KeyValuePair<int, ITvCardHandler> card in _cards.Where(t => t.Value.DataBaseCard.IdCard != user.CardId))
        {
          freeCardsIterated.Add(card.Value.DataBaseCard.IdCard);
        }
      }
      return freeCardsIterated;
    }

    private bool IterateTicketsUntilTimeshifting(ref IUser user, Channel channel, ICollection<ICardTuneReservationTicket> tickets, CardReservationTimeshifting cardResImpl, string intialTimeshiftingFilename, ICollection<CardDetail> freeCards, int maxCards, ref VirtualCard card, ref TvResult result, ref int cardsIterated, out bool cardChanged)
    {
      cardChanged = false;
      int failedCardId = -1;
      bool moreCardsAvailable = true;
      Log.Write("Controller: try max {0} of {1} cards for timeshifting", maxCards, freeCards.Count);
      //keep tuning each card until we are succesful                   
      int cardIteration = 0;
      foreach (CardDetail cardInfo in freeCards)
      {
        if (!moreCardsAvailable)
        {
          break;
        }
        IUser userCopy = UserFactory.CreateBasicUser(user.Name, cardInfo.Id, user.Priority, user.IsAdmin);
        SetupTimeShiftingFolders(cardInfo);
        ITvCardHandler tvcard = _cards[cardInfo.Id];
        try
        {
          ICardTuneReservationTicket ticket = GetTicketByCardId(cardInfo, tickets);
          if (ticket == null)
          {
            Log.Write("Controller: StartTimeShifting - could not find cardreservation on card:{0}",
                      userCopy.CardId);
            HandleAllCardsBusy(tickets, out result, out failedCardId, cardInfo, tvcard);
            continue;
          }
          cardsIterated++;
          bool isTimeshifting = ticket.IsAnySubChannelTimeshifting;
          if (isTimeshifting)
          {
            RemoveInactiveUsers(ticket);
            if (!IsTransponderAvailable(user, maxCards, cardInfo, cardIteration, tvcard, ticket))
            {
              HandleAllCardsBusy(tickets, out result, out failedCardId, cardInfo, tvcard);
              continue;
            }
          }

          //tune to the new channel                  
          IChannel tuneChannel = cardInfo.TuningDetail;
          result = CardTune(ref userCopy, tuneChannel, channel, ticket, cardResImpl);
          if (!HasTvSucceeded(result))
          {
            HandleTvException(tickets, out failedCardId, cardInfo, tvcard);
            StopTimeShifting(ref userCopy);
            continue; //try next card            
          }

          //reset failedCardId incase previous card iteration failed.
          failedCardId = -1;
          CardReservationHelper.CancelAllCardReservations(tickets, _cards);
          Log.Info("control2:{0} {1} {2}", userCopy.Name, userCopy.CardId, userCopy.SubChannel);
          card = GetVirtualCard(userCopy);
          card.NrOfOtherUsersTimeshiftingOnCard = ticket.NumberOfOtherUsersOnSameChannel;
          RemoveUserFromOtherCards(card.Id, userCopy);
          UpdateChannelStatesForUsers();
        }
        catch (Exception)
        {
          CardReservationHelper.CancelCardReservationAndRemoveTicket(tvcard, tickets);
          if ((cardIteration + 1) < maxCards)
          {
            //in case of exception, try next card if available.
            HandleTvException(tickets, out failedCardId, cardInfo, tvcard);
            continue;
          }
          throw;
        }
        finally
        {
          if (failedCardId > 0)
          {
            user.FailedCardId = failedCardId;
          }
          if (!HasTvSucceeded(result))
          {
            moreCardsAvailable = AreMoreCardsAvailable(cardsIterated, maxCards, cardIteration);
            Log.Write(moreCardsAvailable
                        ? "Controller: Timeshifting failed, lets try next available card."
                        : "Controller: Timeshifting failed, no more cards available.");
            cardChanged = (maxCards > 1);
          }
          else
          {
            cardChanged = GetCardChanged(card, intialTimeshiftingFilename);
          }
          cardIteration++;
        }
        break; //if we made it to the bottom, then we have a successful timeshifting.          
      } //end of foreach      
      return moreCardsAvailable;
    }

    private static void HandleTvException(ICollection<ICardTuneReservationTicket> tickets, out int failedCardId,
                                       CardDetail cardInfo, ITvCardHandler tvcard)
    {
      CardReservationHelper.CancelCardReservationAndRemoveTicket(tvcard, tickets);      
      failedCardId = cardInfo.Id;      
    }

    private static void HandleAllCardsBusy(ICollection<ICardTuneReservationTicket> tickets, out TvResult result, out int failedCardId,
                                           CardDetail cardInfo, ITvCardHandler tvcard)
    {
      HandleTvException (tickets,  out failedCardId, cardInfo, tvcard);
      result = TvResult.AllCardsBusy;
    }

    private bool IsTransponderAvailable(IUser user, int maxCards,
                                                CardDetail cardInfo, int cardIteration, ITvCardHandler tvcard,
                                                ICardTuneReservationTicket ticket)
    {      
      bool isTimeshiftingChannelAvailable = true;      
      if (ticket.IsSameTransponder)
      {
        SameTransponder(user, ticket);
      }
      else
      {
        bool isDifferentTransponderAvail = DifferentTransponder(maxCards, ticket, tvcard,
                                                                cardInfo, cardIteration);
        if (!isDifferentTransponderAvail)
        {          
          isTimeshiftingChannelAvailable = false;
        }
      }      
      return isTimeshiftingChannelAvailable;
    }

    private static bool HasTvSucceeded(TvResult result)
    {
      return result == TvResult.Succeeded;
    }

    private static TvResult AllCardsBusy(TvResult result)
    {
      // do not overwite existing tvresult from previous tune iteration.
      if (result == TvResult.UnknownError)
      {
        //no free cards available
        result = TvResult.AllCardsBusy;
      }
      return result;
    }

    private VirtualCard GetValidVirtualCard(IUser user)
    {
      VirtualCard initialCard = null;
      if (user.CardId != -1)
      {
        initialCard = GetVirtualCard(user);
      }
      return initialCard;
    }

    private bool AreMoreCardsAvailable(int cardsIterated, int maxCards, int i)
    {
      return (i < maxCards) && (_maxFreeCardsToTry == 0 || _maxFreeCardsToTry > cardsIterated);
    }

    private static ICardTuneReservationTicket GetTicketByCardId(CardDetail cardInfo, IEnumerable<ICardTuneReservationTicket> tickets)
    {
      return tickets.FirstOrDefault(t => t.CardId == cardInfo.Id);
    }

    private bool DifferentTransponder(int maxCards, ICardTuneReservationTicket ticket, ITvCardHandler tvcard, CardDetail cardInfo, int cardIteration)
    {
      bool isDifferentTransponderAvail = false; 
      bool foundAnyUsersOnCard = FoundAnyUsersOnCard(ticket);
      if (foundAnyUsersOnCard)
      {
        bool foundCandidateForKicking = FoundCandidateForKicking(ticket);
        if (foundCandidateForKicking)
        {
          bool kickLeechingUsersIfNoMoreCardsAvail = KickLeechingUsersIfNoMoreCardsAvail(tvcard, cardInfo, ticket,
                                                                                          cardIteration,
                                                                                          maxCards);
          bool cardsAvailable = ((cardIteration + 1) < maxCards);
          if (!kickLeechingUsersIfNoMoreCardsAvail && cardsAvailable)
          {
            Log.Write(
              "Controller: skipping card:{0} since other users are present on the same channel and there are still cards available.",
              cardInfo.Card.IdCard);
              //TODO: what if the following cards fail, should we then try and kick the leech user, in order to make room for a tune ?            
          }
          else
          {
            isDifferentTransponderAvail = true;
          }
        }
        else
        {
          Log.Write(
            "Controller: skipping card:{0} since is it busy (users present with higher priority).",
            cardInfo.Card.IdCard);
        }
      }
      else
      {
        isDifferentTransponderAvail = true;
      }
      return isDifferentTransponderAvail;
    }

    private static void SameTransponder(IUser user, ICardTuneReservationTicket ticket)
    {
      bool existingOwnerFoundOnSameChannel = ExistingOwnerFoundOnSameChannel(ticket);
      if (existingOwnerFoundOnSameChannel)
      {
        InheritSubChannelFromOwner(user, ticket);
      }
    }

    private static void UpdateFreeCardsIterated(ICollection<int> freeCardsIterated, IEnumerable<CardDetail> freeCards)
    {
      foreach (CardDetail card in freeCards)
      {
        int idCard = card.Card.IdCard;
        if (!freeCardsIterated.Contains(idCard))
        {
          freeCardsIterated.Add(idCard);
        }
      }
    }

    private static bool HasFreeCards <T>(ICollection<T> freeCards)
    {
      bool hasFreeCards = (freeCards.Count > 0);      
      return hasFreeCards;
    }

    private static bool HasTickets(ICollection<ICardTuneReservationTicket> tickets)
    {
      bool hasTickets = (tickets.Count > 0);      
      return hasTickets;
    }    

    private static void InheritSubChannelFromOwner(IUser user, ICardTuneReservationTicket ticket)
    {
      Log.Write("Controller: leech user={0} inherits subch={1}", user.Name, ticket.OwnerSubchannel);      
      user.SubChannel = ticket.OwnerSubchannel;      
    }

    private static bool FoundCandidateForKicking(ICardTuneReservationTicket ticket)
    {
      return (FoundAnyUsersOnCard(ticket) && (ticket.HasHighestPriority || (ticket.IsOwner && ticket.HasEqualOrHigherPriority)));
    }

    private static bool FoundAnyUsersOnCard(ICardTuneReservationTicket ticket)
    {
      return (ticket.NumberOfOtherUsersOnCurrentCard > 0);
    }
    
    private static bool ExistingOwnerFoundOnSameChannel(ICardTuneReservationTicket ticket)
    {      
      return (ticket.OwnerSubchannel > -1);
    }

    private bool KickLeechingUsersIfNoMoreCardsAvail(ITvCardHandler tvcard, CardDetail cardInfo, ICardTuneReservationTicket ticket, int cardIteration, int maxCards)
    {
      bool kickLeechingUsersIfNoMoreCardsAvail = false;               

      if ((cardIteration+1) == maxCards) // only kick users if we have no more cards to choose from
      {
        IUser user = ticket.User;
        GetUser(tvcard, ref user);        
        for (int j = ticket.ActiveUsers.Count - 1; j > -1; j--)
        {
          IUser activeUser = ticket.ActiveUsers[j];
          
          string channelInfo = Convert.ToString(activeUser.IdChannel);
          if (activeUser.IdChannel > 0)
          {
            Channel ch = Channel.Retrieve(activeUser.IdChannel);
            if (ch != null)
            {
              channelInfo = ch.DisplayName;
            }
          }

          Log.Write(
            "Controller: kicking leech user '{0}' with prio={1} off card={2} on channel={3} (subchannel #{4}) since owner '{5}' with prio={6} (subchannel #{7}) changed transponder and there are no more cards available",
            activeUser.Name,
            activeUser.Priority,
            cardInfo.Card.Name,
            channelInfo,
            activeUser.SubChannel,
            user.Name,
            user.Priority,
            user.SubChannel);

          StopTimeShifting(ref activeUser, TvStoppedReason.OwnerChangedTS);
          kickLeechingUsersIfNoMoreCardsAvail = true;                                
        }
      }
      return kickLeechingUsersIfNoMoreCardsAvail;
    }

    private static void GetUser(ITvCardHandler tvcard, ref IUser user) 
    {
      if (user != null)
      {
        var context = tvcard.Card.Context as ITvCardContext;
        if (context != null)
        {
          context.GetUser(ref user);
        }
      }      
    }

    private void RemoveInactiveUsers(ICardTuneReservationTicket ticket)
    {
      for (int i = 0; i < ticket.InactiveUsers.Count; i++ )
      {
        IUser inactiveUser = ticket.InactiveUsers[i];
        Log.Debug("controller: RemoveInactiveUsers {0}", inactiveUser.Name);        
        StopTimeShifting(ref inactiveUser);
        //removing inactive user which shouldnt happen, but atleast its better than having timeshfiting fail.
      }
    }   

    private static bool GetCardChanged(VirtualCard card, string intialTimeshiftingFilename)
    {
      bool cardChanged = false;
      if (card != null && card.TimeShiftFileName != null)
      {
        string newTimeshiftingFilename = card.TimeShiftFileName;
        cardChanged = (intialTimeshiftingFilename != newTimeshiftingFilename);
      }
      return cardChanged;
    }
    
    private string GetIntialTimeshiftingFilename(VirtualCard initialCard)
    {
      string intialTimeshiftingFilename = "";
      if (initialCard != null && initialCard.TimeShiftFileName != null)
      {
        intialTimeshiftingFilename = initialCard.TimeShiftFileName;
      }
      return intialTimeshiftingFilename;
    }

    private void StopEPGgrabber()
    {
      if (_epgGrabber != null)
      {
        Log.Write("Controller: epg stop");
        _epgGrabber.Stop();
      }
    }

    private void StartEPGgrabber()
    {
      if (_epgGrabber != null && AllCardsIdle)
      {
        Log.Write("Controller: epg start");
        _epgGrabber.Start();
      }
    }

    private int GetMaxCards(ICollection<CardDetail> freeCards)
    {
      int maxCards;
      if (_maxFreeCardsToTry == 0)
      {
        maxCards = freeCards.Count;
      }
      else
      {
        maxCards = Math.Min(_maxFreeCardsToTry, freeCards.Count);

        if (maxCards > freeCards.Count)
        {
          maxCards = freeCards.Count;
        }
      }
      return maxCards;
    }

    private void SetupTimeShiftingFolders(CardDetail cardInfo) 
    {
      //setup folders
      if (cardInfo.Card.RecordingFolder == String.Empty)
      {
        cardInfo.Card.RecordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings",
                                                      Environment.GetFolderPath(
                                                        Environment.SpecialFolder.CommonApplicationData));
        if (!Directory.Exists(cardInfo.Card.RecordingFolder))
        {
          Log.Write("Controller: creating recording folder {0} for card {0}", cardInfo.Card.RecordingFolder,
                    cardInfo.Card.Name);
          Directory.CreateDirectory(cardInfo.Card.RecordingFolder);
        }
      }
      if (cardInfo.Card.TimeShiftFolder == String.Empty)
      {
        cardInfo.Card.TimeShiftFolder = String.Format(
          @"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer",
          Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        if (!Directory.Exists(cardInfo.Card.TimeShiftFolder))
        {
          Log.Write("Controller: creating timeshifting folder {0} for card {0}", cardInfo.Card.TimeShiftFolder,
                    cardInfo.Card.Name);
          Directory.CreateDirectory(cardInfo.Card.TimeShiftFolder);
        }
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
    public TvResult StartTimeShifting(ref IUser user, int idChannel, out VirtualCard card)
    {
      bool cardChanged;
      return StartTimeShifting(ref user, idChannel, out card, false, out cardChanged);
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref IUser user, int idChannel, out VirtualCard card, out bool cardChanged)
    {
      return StartTimeShifting(ref user, idChannel, out card, false, out cardChanged);
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
        if (_isMaster == false)
          return;
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
        Gentle.Common.CacheManager.ClearQueryResultsByType(typeof (Schedule));
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
        Gentle.Common.CacheManager.ClearQueryResultsByType(typeof (Schedule));
        if (_scheduler != null)
        {
          _scheduler.ResetTimer();
        }
        TvServerEventArgs tvargs = (TvServerEventArgs)args;
        Fire(this,
             new TvServerEventArgs(TvServerEventType.ScheduledAdded, tvargs.Schedules, tvargs.Conflicts,
                                   tvargs.ArgsUpdatedState));
      }
      catch (Exception ex)
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
          if (_epgGrabber == null)
            return false;
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
              if (_layer.GetSetting("idleEPGGrabberEnabled", "yes").Value == "yes")
              {                
                StartEPGgrabber();
              }
            }
          }
          else
          {
            if (_epgGrabber != null)
            {
              if (_layer.GetSetting("idleEPGGrabberEnabled", "yes").Value == "yes")
              {
                StopEPGgrabber();                
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
        //Give it a few secounds.
        Thread.Sleep(5000);
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
    public void GetDatabaseConnectionString(out string connectionString, out string provider)
    {
      connectionString = "";
      provider = "";
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(String.Format(@"{0}\gentle.config", PathManager.GetDataPath));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode nodeConnection = nodeKey.Attributes.GetNamedItem("connectionString");
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
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
        doc.Load(String.Format(@"{0}\gentle.config", PathManager.GetDataPath));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode nodeConnection = nodeKey.Attributes.GetNamedItem("connectionString");
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
        nodeProvider.InnerText = connectionString;
        nodeConnection.InnerText = provider;
        doc.Save(String.Format(@"{0}\gentle.config", PathManager.GetDataPath));

        Gentle.Framework.ProviderFactory.ResetGentle(true);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);
        DeInit();
        //Give it a few seconds.
        Thread.Sleep(3000);
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
      _cards[cardId].DisEqC.GetPosition(out satellitePosition, out stepsAzimuth, out stepsElevation);
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
    public void StopGrabbingEpg(IUser user)
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

    public IUser[] GetUsersForCard(int cardId)
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
      Dictionary<int, ITvCardHandler>.ValueCollection cardHandlers = _cards.Values;


      foreach (ITvCardHandler tvcard in cardHandlers)
      {
        IUser[] users = tvcard.Users.GetUsers();

        if (users == null || users.Length == 0)
          continue;

        for (int i = 0; i < users.Length; ++i)
        {
          IUser user = users[i];
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
    public void GetAllRecordingChannels(out List<int> currentRecChannels, out List<int> currentTSChannels,
                                        out List<int> currentUnavailChannels, out List<int> currentAvailChannels)
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
        IUser[] users = tvcard.Users.GetUsers();

        if (users == null || users.Length == 0)
          continue;

        for (int i = 0; i < users.Length; ++i)
        {
          IUser user = users[i];
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
    public Dictionary<int, ChannelState> GetAllChannelStatesCached(IUser user)
    {
      if (user == null || user.CardId < 1)
      {
        return null;
      }

      IUser[] users = _cards[user.CardId].Users.GetUsers();

      if (users != null)
      {
        for (int i = 0; i < users.Length; i++)
        {
          IUser u = users[i];

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
    public Dictionary<int, ChannelState> GetAllChannelStatesForGroup(int idGroup, IUser user)
    {
      if (idGroup < 1)
      {
        return null;
      }

      if (user == null)
      {
        return null;
      }      

      IList<Channel> tvChannelList = _layer.GetTVGuideChannelsForGroup(idGroup);

      if (tvChannelList == null || tvChannelList.Count == 0)
        return null;

      Dictionary<int, ChannelState> channelStatesList = new Dictionary<int, ChannelState>();
      ChannelStates channelStates = new ChannelStates(_layer, this);

      if (channelStates != null)
      {
        user.Priority = UserFactory.GetDefaultPriority(user.Name);
        channelStatesList = channelStates.GetChannelStates(_cards, tvChannelList, ref user, this);
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
    public ChannelState GetChannelState(int idChannel, IUser user)
    {
      ChannelState chanState = ChannelState.tunable;

      if (user != null)
      {
        user.Priority = UserFactory.GetDefaultPriority(user.Name);
        Dictionary<int, ChannelState> channelStates = GetAllChannelStatesCached(user);

        if (channelStates != null)
        {
          if (!channelStates.TryGetValue(idChannel, out chanState))
          {
            chanState = ChannelState.tunable;
          }
        }
        else
        {
          Channel dbchannel = Channel.Retrieve(idChannel);
          TvResult viewResult;
          _cardAllocation.GetFreeCardsForChannel(_cards, dbchannel, ref user, out viewResult);
          chanState = viewResult == TvResult.Succeeded ? ChannelState.tunable : ChannelState.nottunable;
        }
      }
      else
      {
        chanState = ChannelState.nottunable;
      }

      return chanState;
    }

    /// <summary>
    /// Returns an ordered, distinct list of all program genres.  Maintained for backward compatibility.
    /// </summary>
    /// <returns></returns>
    public List<string> GetGenres()
    {
      return GetProgramGenres();
    }

    /// <summary>
    /// Returns an ordered, distinct list of all program genres.
    /// </summary>
    /// <returns></returns>
    public List<string> GetProgramGenres()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      return layer.GetProgramGenres();
    }

    /// <summary>
    /// Returns a list of MediaPortal genre objects.
    /// </summary>
    /// <returns></returns>
    public List<MpGenre> GetMpGenres()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      return (List<MpGenre>)layer.GetMpGenres();
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
      {
        return false;
      }
      bool result;
      try
      {
        result = _cards[cardId].Card.SupportsQualityControl;
      }
      catch
      {
        return false;
      }
      return result;
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

      if (_channelStates != null)
      {
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
              _tvChannelListGroups = _layer.GetTVGuideChannelsForGroup(group.IdGroup);
            }
            else
            {
              IList<Channel> tvChannelList = _layer.GetTVGuideChannelsForGroup(group.IdGroup);

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

        _channelStates.SetChannelStates(_cards, _tvChannelListGroups, this);
      }
    }

    private void HeartBeatMonitor()
    {
      Log.Info("Controller: Heartbeat Monitor initiated, max timeout allowed is {0} sec.",
               HEARTBEAT_MAX_SECS_EXCEED_ALLOWED);
      while (true)
      {
        Dictionary<int, ITvCardHandler>.Enumerator enumerator = _cards.GetEnumerator();

        //for each card
        while (enumerator.MoveNext())
        {
          KeyValuePair<int, ITvCardHandler> keyPair = enumerator.Current;
          //get a list of all users for this card
          IUser[] users = keyPair.Value.Users.GetUsers();
          if (users != null)
          {
            //for each user
            for (int i = 0; i < users.Length; ++i)
            {
              IUser tmpUser = users[i];
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
        //throw new Exception("heartbeat died on purpose, causing an unhandled exception");
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
    public bool IsOwner(int cardId, IUser user)
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
    public void RemoveUserFromOtherCards(int cardId, IUser user)
    {
      if (user.CardId > 0)
      {
        IUser newUser = (IUser)user.Clone();
        IEnumerator<KeyValuePair<int, ITvCardHandler>> enumerator = CardCollection.GetEnumerator();

        ITVCard card = CardCollection[user.CardId].Card;
        while (enumerator.MoveNext())
        {
          KeyValuePair<int, ITvCardHandler> key = enumerator.Current;
          if (key.Key == cardId)
            continue;

          if (key.Value.Card.Context == card.Context)
            continue;

          var context = key.Value.Card.Context as ITvCardContext;
          if (context != null)
          {
            if (context.DoesExists(newUser))
            {
              newUser.CardId = key.Key;
              Log.Debug("RemoveUserFromOtherCards : {0} - {1}", newUser.Name, newUser.CardId);
              StopTimeShifting(ref newUser);
            }
          }
          else
          {
            continue;
          }
        }
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

    public void HeartBeat(IUser user)
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
    private TvResult CardTune(ref IUser user, IChannel channel, Channel dbChannel, ICardTuneReservationTicket ticket, CardReservationTimeshifting cardResTS)
    {
      if (ValidateTvControllerParams(user))
      {
        return TvResult.CardIsDisabled;
      }

      try
      {
        if (_cards[user.CardId].DataBaseCard.Enabled == false)
        {
          return TvResult.CardIsDisabled;
        }

        if (ticket.ConflictingSubchannelFound)
        {
          var context = _cards[user.CardId].Card.Context as ITvCardContext;
          if (context != null)
          {
            context.UserNextAvailableSubchannel(user);
          }
        }

        Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(user), (User)user, channel));

        _cards[user.CardId].Tuner.OnAfterTuneEvent -= Tuner_OnAfterTuneEvent;
        _cards[user.CardId].Tuner.OnBeforeTuneEvent -= Tuner_OnBeforeTuneEvent;

        _cards[user.CardId].Tuner.OnAfterTuneEvent += Tuner_OnAfterTuneEvent;
        _cards[user.CardId].Tuner.OnBeforeTuneEvent += Tuner_OnBeforeTuneEvent;
        
        cardResTS.OnStartCardTune += CardResTsOnStartCardTune;
        TvResult result = cardResTS.CardTune(_cards[user.CardId], ref user, channel, dbChannel, ticket);
        cardResTS.OnStartCardTune -= CardResTsOnStartCardTune;

        Log.Info("Controller: {0} {1} {2}", user.Name, user.CardId, user.SubChannel);

        return result;
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(user), (User)user, channel));
      }
    }

    TvResult CardResTsOnStartCardTune(ref IUser user, ref string fileName)
    {
      return StartTimeShifting(ref user, ref fileName);
    }

    private void Tuner_OnBeforeTuneEvent(ITvCardHandler cardHandler)
    {
      cardHandler.TimeShifter.OnBeforeTune();
    }

    private void Tuner_OnAfterTuneEvent(ITvCardHandler cardHandler)
    {
      cardHandler.TimeShifter.OnAfterTune();
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
    private VirtualCard GetVirtualCard(IUser user)
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
          IUser[] users = _cards[cardId].Users.GetUsers();
          if (users != null)
          {
            for (int i = 0; i < users.Length; ++i)
            {
              if (_cards[cardId].Recorder.IsRecording(ref users[i]) ||
                  _cards[cardId].TimeShifter.IsTimeShifting(ref users[i]))
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
        return true;
      }
    }

    #region private methods

    private bool ValidateTvControllerParams(int cardId, bool checkCardPresent)
    {
      if (cardId < 0 || !_cards.ContainsKey(cardId) || (checkCardPresent && !CardPresent(cardId)))
      {
#if DEBUG
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);
        Log.Error(
          "TVController:" + sf.GetMethod().Name +
          " - incorrect parameters used! cardId {0} _cards.ContainsKey(cardId) == {1} CardPresent {2}", cardId,
          _cards.ContainsKey(cardId), CardPresent(cardId));
        Log.Error("{0}", st);
#endif
        return true;
      }
      return false;
    }

    private bool ValidateTvControllerParams(int cardId)
    {
      return ValidateTvControllerParams(cardId, true);
    }

    private bool ValidateTvControllerParams(IUser user)
    {
      if (user == null || user.CardId < 0 || !_cards.ContainsKey(user.CardId) || (!CardPresent(user.CardId)))
      {
#if DEBUG
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);

        if (user != null)
        {
          Log.Error(
            "TVController:" + sf.GetMethod().Name +
            " - incorrect parameters used! user {0} cardId {1} _cards.ContainsKey(cardId) == {2} CardPresent(cardId) {3}",
            user, user.CardId, _cards.ContainsKey(user.CardId), CardPresent(user.CardId));
        }
        else
        {
          Log.Error("TVController:" + sf.GetMethod().Name + " - incorrect parameters used! user NULL");
        }
        Log.Error("{0}", st);
#endif
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

    private CiMenu curMenu;

    /// <summary>
    /// Checks menu state; If it's ready, fire event to "push" it to client
    /// </summary>
    private void CheckForCallback()
    {
      if (curMenu != null)
      {
        if (curMenu.State == CiMenuState.Ready || curMenu.State == CiMenuState.NoChoices ||
            curMenu.State == CiMenuState.Request || curMenu.State == CiMenuState.Close)
        {
          // special workaround for AstonCrypt2 cam type (according to database CamType)
          // avoid unwanted CI menu callbacks if user has not opened CI menu interactively
          if (ActiveCiMenuCard != -1 && _cards[ActiveCiMenuCard].DataBaseCard.CamType == 1 && !IsCiMenuInteractive)
          {
            Log.Debug("AstonCrypt2: unrequested CI menu received, no action done. Menu Title: {0}", curMenu.Title);
            return;
          }

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
      curMenu.AddEntry(nChoice + 1, lpszText); // choices for display +1 
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
        curMenu = new CiMenu(String.Empty, String.Empty, String.Empty, CiMenuState.Close);
      }
      curMenu.State = CiMenuState.Close;
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

    public void ExecutePendingDeletions()
    {
      try
      {
        // System.Diagnostics.Debugger.Launch();
        List<int> pendingDelitionRemove = new List<int>();
        IList<PendingDeletion> pendingDeletions = PendingDeletion.ListAll();

        Log.Debug("ExecutePendingDeletions: number of pending deletions : " + Convert.ToString(pendingDeletions.Count));

        foreach (PendingDeletion pendingDelition in pendingDeletions)
        {
          Log.Debug("ExecutePendingDeletions: trying to remove file : " + pendingDelition.FileName);

          bool wasPendingDeletionAdded = false;
          bool wasDeleted = RecordingFileHandler.DeleteRecordingOnDisk(pendingDelition.FileName,
                                                                       out wasPendingDeletionAdded);
          if (wasDeleted && !wasPendingDeletionAdded)
          {
            pendingDelitionRemove.Add(pendingDelition.IdPendingDeletion);
          }
        }

        foreach (int id in pendingDelitionRemove)
        {
          PendingDeletion pendingDelition = PendingDeletion.Retrieve(id);

          if (pendingDelition != null)
          {
            pendingDelition.Remove();
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("ExecutePendingDeletions exception : " + ex.Message);
      }
    }

    private bool _onResumeDone = false;

    public void OnResume()
    {
      if (!_onResumeDone)
      {
        Log.Info("TvController.OnResume()");
        SetupHeartbeatThread();

        if (_scheduler != null)
        {
          _scheduler.Start();
        }
      }
      _onResumeDone = true;
    }

    public void OnSuspend()
    {
      _onResumeDone = false;
      Log.Info("TvController.OnSuspend()");
      if (heartBeatMonitorThread != null && heartBeatMonitorThread.IsAlive)
      {
        heartBeatMonitorThread.Abort();
        heartBeatMonitorThread = null;
      }
      if (_scheduler != null)
      {
        _scheduler.Stop();
      }

      IUser tmpUser = new User();
      foreach (ITvCardHandler cardhandler in this.CardCollection.Values)
      {
        cardhandler.StopCard(tmpUser);
      }
    }

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>   
    /// <param name="user">user</param>    
    /// <param name="totalTSpackets">Amount of packets processed</param>    
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>
    public void GetStreamQualityCounters(IUser user, out int totalTSpackets, out int discontinuityCounter)
    {
      int cardId = user.CardId;
      ITvCardHandler cardHandler = _cards[cardId];

      cardHandler.TimeShifter.GetStreamQualityCounters(user, out totalTSpackets, out discontinuityCounter);
    }

    /// <summary>
    /// Returns the subchannels count for the selected card
    /// stream for the selected card
    /// </summary>
    /// <param name="idCard">card id.</param>
    /// <returns>
    /// subchannels count
    /// </returns>
    public int GetSubChannels(int idCard)
    {
      int subchannels = 0;

      if (idCard > 0)
      {
        ITvCardHandler cardHandler = _cards[idCard];
        if (cardHandler.Card != null && cardHandler.Card.SubChannels != null)
        {
          subchannels = cardHandler.Card.SubChannels.Length;
        }
      }

      return subchannels;
    }
  }
}