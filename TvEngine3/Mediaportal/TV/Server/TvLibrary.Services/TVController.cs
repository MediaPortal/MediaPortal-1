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

#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MediaPortal.Common.Utils;
using MediaPortal.Common.Utils.ExtensionMethods;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Presentation;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardAllocation;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardHandler;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardReservation;
using Mediaportal.TV.Server.TVLibrary.CardManagement.CardReservation.Implementations;
using Mediaportal.TV.Server.TVLibrary.DiskManagement;
using Mediaportal.TV.Server.TVLibrary.Epg;
using Mediaportal.TV.Server.TVLibrary.EventDispatchers;
using Mediaportal.TV.Server.TVLibrary.Implementations;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog.Graphs.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.Hybrid;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Scheduler;
using Mediaportal.TV.Server.TVLibrary.Services;
using Mediaportal.TV.Server.TVLibrary.Streaming;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.CardReservation;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using RecordingManagement = Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.RecordingManagement;

#endregion

namespace Mediaportal.TV.Server.TVLibrary
{
  /// <summary>
  /// This class servers all requests from remote clients
  /// and if server is the master it will delegate the requests to the 
  /// correct slave servers
  /// </summary>
  public class 
      TvController : IInternalControllerService, IDisposable, ITvServerEvent, ICiMenuCallbacks
  {


    #region constants

    #endregion

    #region variables

    private bool _onResumeDone;
    private int _rtspStreamingPort;
    private string _hostName;

    private readonly TvServerEventDispatcher _tvServerEventDispatcher;
    private readonly HeartbeatManager _heartbeatManager;
    private readonly CiMenuManager _ciMenuManager;
    private readonly ICardAllocation _cardAllocation;
    private readonly ChannelStates _channelStates;

    /// <summary>
    /// EPG grabber for DVB
    /// </summary>
    private EpgGrabber _epgGrabber;

    /// <summary>
    /// Recording scheduler
    /// </summary>
    private Scheduler.Scheduler _scheduler;

    /// <summary>
    /// RTSP Streaming Server
    /// </summary>
    private RtspStreaming _streamer;


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
      bool initConditionalAccess = false;
      try
      {
        if (ValidateTvControllerParams(cardId, false))
        {
          // Note: this is *far* from ideal. We have no way to actually tell whether the tuner supports
          // conditional access unless the tuner graph is initialised. We also have no way to tell if the
          // tuner graph is initialised. If we try to initialise the graph and it is already initialised
          // then we'll get an exception. We do the best that we can, but we can't guarantee that this
          // will work for hybrid tuners - we don't want to interrupt timeshifting and recording.
          ITVCard card = _cards[cardId].Card;
          if (card.IsConditionalAccessSupported)
          {
            return true;
          }
          TvCardBase baseCard = card as TvCardBase;
          if (baseCard == null)
          {
            // Non-initialised HybridCard instances exit here.
            return false;
          }
          try
          {
            baseCard.BuildGraph();
          }
          catch (Exception)
          {
            // If we get here, we assume the graph is already built - not a problem.
          }
          initConditionalAccess = card.IsConditionalAccessSupported;
        }
        else
        {          
          this.LogDebug("InitConditionalAccess: ValidateTvControllerParams failed");         
        }                
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return initConditionalAccess;
    }

    private Dictionary<int, ITvCardHandler> _cards = new Dictionary<int, ITvCardHandler>();

    /// 
    // contains a cached copy of all the channels in the user defined groups (excl. the all channels group)
    // used to speedup "mini EPG" channel state creation.
    private List<Channel> _tvChannelListGroups;

   

    #endregion

    #region events

    public event TvServerEventHandler OnTvServerEvent;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvControllerService"/> class.
    /// </summary>
    public TvController()
    {
      _tvServerEventDispatcher = new TvServerEventDispatcher();
      _heartbeatManager = new HeartbeatManager();
      _channelStates = new ChannelStates();
      _ciMenuManager = new CiMenuManager();
      _cardAllocation = new AdvancedCardAllocation();

      _channelStates.OnChannelStatesSet -= new ChannelStates.OnChannelStatesSetDelegate(channelStates_OnChannelStatesSet);
      _channelStates.OnChannelStatesSet += new ChannelStates.OnChannelStatesSetDelegate(channelStates_OnChannelStatesSet);
    }

    public IDictionary<int, ITvCardHandler> CardCollection
    {
      get { return _cards; }
    }

    #region CI Menu action functions

    /// <summary>
    /// Returns if selected card has CI Menu capabilities
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if supported</returns>
    public bool CiMenuSupported(int cardId)
    {
      bool checkCardPresent = false;
      try
      {
        this.LogDebug("CiMenuSupported called cardid {0}", cardId);
        if (ValidateTvControllerParams(cardId, checkCardPresent))
        {
          this.LogDebug("CiMenuSupported card {0} supported: {1}", _cards[cardId].CardName, _cards[cardId].CiMenuSupported);
          checkCardPresent = _cards[cardId].CiMenuSupported;          
        }
        else
        {
          this.LogDebug("ValidateTvControllerParams failed");          
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return checkCardPresent;
      
    }

    /// <summary>
    /// Enters the card's CI menu
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    public bool EnterCiMenu(int cardId)
    {
      bool checkCardPresent = false;
      try
      {
        this.LogDebug("EnterCiMenu called");
        if (ValidateTvControllerParams(cardId, checkCardPresent))
        {
          if (_cards[cardId].CiMenuActions != null)
          {
            _ciMenuManager.IsCiMenuInteractive = true; // user action
            checkCardPresent = _cards[cardId].CiMenuActions.EnterCIMenu();
          }  
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      } 
      
      return checkCardPresent;
    }

    /// <summary>
    /// SelectMenu selects an ci menu entry; 
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="choice">choice,0 for "back" action</param>
    /// <returns>true if successful</returns>
    public bool SelectMenu(int cardId, byte choice)
    {
      bool checkCardPresent = false;
      try
      {
        this.LogDebug("SelectCiMenu called");
        if (ValidateTvControllerParams(cardId, false))
        {
          checkCardPresent = _cards[cardId].CiMenuActions != null && _cards[cardId].CiMenuActions.SelectMenu(choice);  
        }                  
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return checkCardPresent;
    }

    /// <summary>
    /// CloseMenu closes the menu
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true if successful</returns>
    public bool CloseMenu(int cardId)
    {
      bool closeMenu = false;
      try
      {
        this.LogDebug("CloseMenu called");
        if (ValidateTvControllerParams(cardId, false))
        {
          if (_cards[cardId].CiMenuActions != null)
          {
            _ciMenuManager.IsCiMenuInteractive = false; // user action ended by wanted close
            closeMenu = _cards[cardId].CiMenuActions.CloseCIMenu();
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return closeMenu;
    }

    /// <summary>
    /// Sends a menu answer back to CAM
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="cancel">true to cancel request</param>
    /// <param name="answer">answer string</param>
    /// <returns></returns>
    public bool SendMenuAnswer(int cardId, bool cancel, string answer)
    {
      bool sendMenuAnswer = false;
      try
      {
        this.LogDebug("SendMenuAnswer called");
        if (ValidateTvControllerParams(cardId, false))
        {
          sendMenuAnswer = _cards[cardId].CiMenuActions != null && _cards[cardId].CiMenuActions.SendMenuAnswer(cancel, answer);
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return sendMenuAnswer;
    }

    /// <summary>
    /// sets a CI menu callback handler. dummy in this case, because it's an interface member
    /// </summary>
    /// <param name="cardId">card</param>
    /// <param name="callbackHandler">null, not required</param>
    /// <returns>true is successful</returns>
    public bool SetCiMenuHandler(int cardId, ICiMenuCallbacks callbackHandler)
    {
      // register tvservice itself as handler
      try
      {
        return EnableCiMenuHandler(cardId);
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return false;
    }

    /// <summary>
    /// Registers the tvserver as primary CI menu handler on serverside
    /// </summary>
    /// <param name="cardId">card</param>
    /// <returns>true is successful</returns>
    public bool EnableCiMenuHandler(int cardId)
    {
      bool enableCiMenuHandler = false;
      this.LogDebug("TvController: EnableCiMenuHandler called");
      if (ValidateTvControllerParams(cardId, false))
      {
        if (_cards[cardId].CiMenuActions != null)
        {
          _ciMenuManager.ActiveCiMenuCard = cardId;
          enableCiMenuHandler = _cards[cardId].CiMenuActions.SetCiMenuHandler(this);
          this.LogDebug("TvController: SetCiMenuHandler: result {0}", enableCiMenuHandler);
        }
      }
      return enableCiMenuHandler;
    }

    #endregion


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
      user = null;
      bool isCardInUse = false;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          isCardInUse = _cards[cardId].UserManagement.IsLocked(out user);
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return isCardInUse;
    }

    /// <summary>
    /// Gets the user for card.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    public IUser GetUserForCard(int cardId)
    {
      IUser user = null;
      if (ValidateTvControllerParams(cardId))
      {
        _cards[cardId].UserManagement.IsLocked(out user);  
      }
      return user;
    }

    public void Init()
    {
      this.LogInfo("Controller: Initializing TVServer");      
      bool result = false;

      Exception ex = null;

      for (int i = 0; i < 5 && !result; i++)
      {
        if (i != 0)
        {
          //Fresh start
          try
          {
            DeInit();
          }
          catch (Exception e)
          {
            ex = e;
            this.LogError("Controller: Error while deinit TvServer in Init");
          }
          Thread.Sleep(3000);
        }
        this.LogInfo("Controller: {0} init attempt", (i + 1));
        try
        {
          InitController();
        }
        catch (Exception e)
        {
          ex = e;          
        }
        result = (ex == null);
      }

      if (result)
      {
        this.LogInfo("Controller: TVServer initialized okay");
      }
      else
      {
        this.LogInfo("Controller: Failed to initialize TVServer");
        if (ex != null)
        {
          throw ex;
        }
      }
    }


    /// <summary>
    /// Initalizes the controller.
    /// It will update the database with the cards found on this system
    /// start the epg grabber and scheduler
    /// and check if its supposed to be a master or slave controller
    /// </summary>
    private void InitController()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        GlobalServiceProvider.Instance.Remove<ITvServerEvent>();
      }
      GlobalServiceProvider.Instance.Add<ITvServerEvent>(this);
      try
      {
        _cards = new Dictionary<int, ITvCardHandler>();
        _localCardCollection = new TvCardCollection(this);

        //log all local ip adresses, usefull for debugging problems
        this.LogDebug("Controller: started at {0}", Dns.GetHostName());
        IPHostEntry local = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ipaddress in local.AddressList)
        {
          // Show only IPv4 family addresses
          if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
          {
            this.LogInfo("Controller: local ip address:{0}", ipaddress.ToString());
          }
        }

        _rtspStreamingPort = SettingsManagement.GetValue("rtspPort", RtspStreaming.DefaultPort);

        //enumerate all tv cards in this pc...
        _maxFreeCardsToTry = SettingsManagement.GetValue("timeshiftMaxFreeCardsToTry", 0);

        IList<Card> allCards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None);

        bool oneOrMoreCardsFound = false;

        foreach (ITVCard itvCard in _localCardCollection.Cards)
        {
          //for each card, check if its already mentioned in the database
          bool found = allCards.Any(card => card.DevicePath == itvCard.DevicePath);
          if (!found)
          {
            // card is not yet in the database, so add it
            this.LogInfo("Controller: add card:{0}", itvCard.Name);

            var newCard = new Card
            {
              TimeshiftingFolder = "",
              RecordingFolder = "",
              DevicePath = itvCard.DevicePath,
              Name = itvCard.Name,
              Priority = 1,
              GrabEPG = true,
              Enabled = true,
              CamType = 0,
              DecryptLimit = 0,              
              NetProvider = (int)DbNetworkProvider.Generic,              
              MultiChannelDecryptMode = (int)MultiChannelDecryptMode.Disabled,
              PidFilterMode = (int)PidFilterMode.Auto,
              IdleMode = (int)DeviceIdleMode.Stop
            };
            oneOrMoreCardsFound = true;
            TVDatabase.TVBusinessLayer.CardManagement.SaveCard(newCard);
          }
        }
        if (oneOrMoreCardsFound)
        {
          allCards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None);
        }
        //notify log about cards from the database which are removed from the pc
        int cardsInstalled = _localCardCollection.Cards.Count;
        foreach (Card dbsCard in allCards)
        {
          {
            bool found = false;
            for (int cardNumber = 0; cardNumber < cardsInstalled; ++cardNumber)
            {
              if (dbsCard.DevicePath == _localCardCollection.Cards[cardNumber].DevicePath)
              {
                Card cardDB = TVDatabase.TVBusinessLayer.CardManagement.GetCardByDevicePath(_localCardCollection.Cards[cardNumber].DevicePath, CardIncludeRelationEnum.None);

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
                        this.LogInfo("Controller: preloading card :{0}", card.Name);
                        card.BuildGraph();
                        if (unknownCard is TvCardAnalog)
                        {
                          ((TvCardAnalog)unknownCard).ReloadCardConfiguration();
                        }
                      }
                      catch (Exception ex)
                      {
                        this.LogError("failed to preload card '{0}', ex = {1}", card.Name, ex);
                      }
                    }
                    else
                    {
                      this.LogInfo("Controller: NOT preloading card :{0}", card.Name);
                    }
                  }
                  else
                  {
                    this.LogInfo("Controller: NOT preloading card :{0}", unknownCard.Name);
                  }
                }

                found = true;
                break;
              }
            }
            if (!found)
            {
              this.LogInfo("Controller: card not found :{0}", dbsCard.Name);

              foreach (ITVCard t in _localCardCollection.Cards.Where(t => t.DevicePath == dbsCard.DevicePath))
              {
                t.CardPresent = false;
                break;
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


        //allCards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards();
        foreach (Card card in allCards)
        {
          {
            foreach (ITVCard t in _localCardCollection.Cards)
            {
              if (t.DevicePath == card.DevicePath)
              {
                localcards[card.IdCard] = t;
                break;
              }
            }
          }
        }

        this.LogInfo("Controller: setup hybrid cards");
        IList<CardGroup> cardgroups = TVDatabase.TVBusinessLayer.CardManagement.ListAllCardGroups();
        foreach (CardGroup group in cardgroups)
        {
          IList<CardGroupMap> cards = group.CardGroupMaps;
          var hybridCardGroup = new HybridCardGroup();
          foreach (CardGroupMap card in cards)
          {
            if (localcards.ContainsKey(card.IdCard))
            {
              localcards[card.IdCard].IsHybrid = true;
              this.LogDebug("Hybrid card: " + localcards[card.IdCard].Name + " (" + group.Name + ")");
              HybridCard hybridCard = hybridCardGroup.Add(card.IdCard, localcards[card.IdCard]);
              localcards[card.IdCard] = hybridCard;
            }
          }
        }

        allCards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB
        foreach (Card dbsCard in allCards)
        {
          if (localcards.ContainsKey(dbsCard.IdCard))
          {
            ITVCard card = localcards[dbsCard.IdCard];
            var tvcard = new TvCardHandler(dbsCard, card);
            _cards[dbsCard.IdCard] = tvcard;
          }

          // remove any old timeshifting TS files	
          try
          {
            string timeShiftPath = dbsCard.TimeshiftingFolder;
            if (string.IsNullOrEmpty(dbsCard.TimeshiftingFolder))
            {
              timeShiftPath = Path.Combine(PathManager.GetDataPath, "timeshiftbuffer");
            }
            if (!Directory.Exists(timeShiftPath))
            {
              this.LogInfo("Controller: creating timeshifting folder {0} for card \"{1}\"", timeShiftPath, dbsCard.Name);
              Directory.CreateDirectory(timeShiftPath);
            }

            this.LogDebug("Controller: card {0}: current timeshiftpath = {1}", dbsCard.Name, timeShiftPath);
            if (timeShiftPath != null)
            {
              string[] files = Directory.GetFiles(timeShiftPath);

              foreach (string file in files)
              {
                try
                {
                  var fInfo = new FileInfo(file);
                  bool delFile = (fInfo.Extension.ToUpperInvariant().IndexOf(".TSBUFFER") == 0);

                  if (!delFile)
                  {
                    delFile = (fInfo.Extension.ToUpperInvariant().IndexOf(".TS") == 0) &&
                              (fInfo.Name.ToUpperInvariant().IndexOf("TSBUFFER") > 0);
                  }
                  if (delFile)
                  {
                    File.Delete(fInfo.FullName);
                  }
                }
                catch (IOException) { }
              }
            }
          }
          catch (Exception exd)
          {
            this.LogInfo("Controller: Error cleaning old ts buffer - {0}", exd.Message);
          }
        }

        this.LogInfo("Controller: setup streaming");
        _hostName = Dns.GetHostName();
        _hostName = SettingsManagement.GetValue("hostname", _hostName);
        _streamer = new RtspStreaming(_hostName, _rtspStreamingPort);


        _epgGrabber = new EpgGrabber();
        _epgGrabber.Start();
        _scheduler = new Scheduler.Scheduler();
        _scheduler.Start();

        StartHeartbeatManager();
        StartTvServerEventDispatcher();

        ExecutePendingDeletions();

        SynchProgramStatesForAllSchedules();
      }
      catch (Exception ex)
      {
        this.LogError("TvControllerException");
        throw;
      }

      this.LogInfo("Controller: initalized");      
    }

    private void SynchProgramStatesForAllSchedules()
    {
      // Re-evaluate program states
      this.LogInfo("Controller: recalculating program states");

      ThreadPool.QueueUserWorkItem(
        delegate
          {
            try
            {
              ProgramManagement.ResetAllStates();
            ProgramManagement.SynchProgramStatesForAllSchedules();
            }
            catch (Exception e)
            {
              this.LogError(e, "could not sync rogram states for all schedules");
            }
          })
        ;
    }

    private void StartTvServerEventDispatcher()
    {
      _tvServerEventDispatcher.Start();
    }

    private void StartHeartbeatManager()
    {      
      _heartbeatManager.Start();
    }


    #endregion


    #region IDisposable Members

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // get rid of managed resources
        DeInit();
      }

      // get rid of unmanaged resources      
    }


    /// <summary>
    /// Clean up the controller when service is stopped
    /// </summary> 
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~TvController()
    {
      Dispose(false);
    }

    /// <summary>
    /// Cleans up the controller
    /// </summary>
    public void DeInit()
    {
      this.LogInfo("Controller: DeInit.");
      try
      {
        StopHeartbeatManager();
        StopTvserverEventDispatcher();

        //stop the RTSP streamer server
        if (_streamer != null)
        {
          this.LogInfo("Controller: stop streamer...");
          _streamer.Stop();
          _streamer = null;
          this.LogInfo("Controller: streamer stopped...");
        }
        //stop the recording scheduler
        if (_scheduler != null)
        {
          this.LogInfo("Controller: stop scheduler...");
          _scheduler.Stop();
          _scheduler = null;
          this.LogInfo("Controller: scheduler stopped...");
        }
        //stop the epg grabber
        StopEPGgrabber();        
        _epgGrabber.SafeDispose();
        _epgGrabber = null;

        //clean up the tv cards
        FreeCards();

        ////Gentle.Common.CacheManager.Clear();
        if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
        {
          GlobalServiceProvider.Instance.Remove<ITvServerEvent>();
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TvController: Deinit failed");
      }
    }

    private void StopTvserverEventDispatcher()
    {
      _tvServerEventDispatcher.Stop();
    }

    private void StopHeartbeatManager()
    {      
      _heartbeatManager.Stop();
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
    /// Gets the total number of cards installed.
    /// </summary>
    /// <value>Number which indicates the cards installed</value>
    public int Cards
    {
      get
      {
        try
        {
            return _cards.Count;    
        }
        catch(Exception e)
        {
            HandleControllerException(e);
        }
        return 0;
      }
    }

    private void HandleControllerException(Exception ex, string errorMsg)
    {
      this.LogError(errorMsg);
      HandleControllerException(ex);      
    }

    private void HandleControllerException(Exception ex)
    {
      var st = new StackTrace(true);
      StackFrame sf = st.GetFrame(0);
      string methodName = sf.GetMethod().Name;
      this.LogError("exception occurred in TVController: {0} - {1}", methodName, ex);
      //throw exception;
    }

      /// <summary>
    /// Gets the card Id for a card
    /// </summary>
    /// <param name="cardIndex">Index of the card.</param>
    /// <value>id of card</value>
    public int CardId(int cardIndex)
    {
      IList<Card> cards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB
      return cards != null && cards.Count > cardIndex ? cards[cardIndex].IdCard : -1;
    }

    /// <summary>
    /// returns if the card is enabled or disabled
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <value>true if enabled, otherwise false</value>
    public bool IsCardEnabled(int cardId)
    {
      bool isCardEnabled = false;
      if (ValidateTvControllerParams(cardId))
      {
        isCardEnabled = _cards[cardId].DataBaseCard.Enabled;  
      }
      return isCardEnabled;
    }

    /// <summary>
    /// Gets the type of card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <value>cardtype (Analog,DvbS,DvbT,DvbC,Atsc)</value>
    public CardType Type(int cardId)
    {
      CardType cardType = CardType.Unknown;
      try
      {
        if (_cards.ContainsKey(cardId))
        {
          if (ValidateTvControllerParams(cardId))
          {
            cardType = _cards[cardId].Type;
          }  
        }
      }
      catch (Exception e)
      {
       HandleControllerException(e); 
      }
      return cardType;
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <returns>name of card</returns>
    public string CardName(int cardId)
    {
      string cardName = "";
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          cardName = _cards[cardId].CardName;
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return cardName;
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="cardId">id of card.</param>
    /// <param name="channel">channel.</param>
    /// <returns>true if card can tune to the channel otherwise false</returns>
    public bool CanTune(int cardId, IChannel channel)
    {
      bool canTune = false;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          canTune = _cards[cardId].Tuner.CanTune(channel);
        }

      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return canTune;
     }


    /// <summary>
    /// Method to check if card is currently present and detected
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>		
    public bool IsCardPresent(int cardId)
    {
      bool cardPresent = false;
      try
      {        
        if (cardId > 0)
        {
          if (_cards != null)
          {
            if (_cards.ContainsKey(cardId))
            {
              string devicePath = _cards[cardId].Card.DevicePath;
              if (devicePath.Length > 0)
              {
                // RemoveUser it from the local card collection
                cardPresent =
                  (from t in _localCardCollection.Cards where t.DevicePath == devicePath select t.CardPresent).
                    FirstOrDefault();
              }
            }
          }
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return cardPresent;
    }

    /// <summary>
    /// Method to remove a non-present card from the local card collection
    /// </summary>
    /// <returns>true if card is present otherwise false</returns>		
    public void CardRemove(int cardId)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          string devicePath = _cards[cardId].Card.DevicePath;
          if (devicePath.Length > 0)
          {
            // RemoveUser database instance
            TVDatabase.TVBusinessLayer.CardManagement.DeleteCard(cardId);
            // RemoveUser it from the card collection
            _cards.Remove(cardId);
            // RemoveUser it from the local card collection
            _localCardCollection.Cards.Remove(_cards[cardId].Card);
          }
        }
        else
        {
          TVDatabase.TVBusinessLayer.CardManagement.DeleteCard(cardId);          
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    /// <summary>
    /// Gets the name for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>device of card</returns>
    public string CardDevice(int cardId)
    {
      string cardDevice = "";
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          cardDevice = _cards[cardId].CardDevice(); 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return cardDevice;
    }

    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel"> </param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel(string userName, int idChannel)
    {
      IChannel channel = null;
      try
      {
        IUser userCopy = GetUserFromContext(userName, idChannel);
        if (userCopy != null)
        {
          channel = _cards[userCopy.CardId].CurrentChannel(userCopy.Name, idChannel);
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return channel;
    }

    private void RefreshTimeshiftingUserFromAnyContext(ref IUser user)
    {
      foreach (ITvCardHandler cardHandler in CardCollection.Values)
      {
        int subChId = cardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name);
        if (subChId > -1)
        {
          cardHandler.UserManagement.RefreshUser(ref user);          
          return;           
        }               
      }
      user.CardId = -1;
    }

    private void RefreshUserFromSpecificContext(ref IUser user)
    {
      if (user != null && user.CardId > 0)
      {
        ITvCardHandler tvCardHandler = _cards[user.CardId];
        tvCardHandler.UserManagement.RefreshUser(ref user);        
      }      
    }

    /// <summary>
    /// Gets the current channel.
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>id of database channel</returns>
    public int CurrentDbChannel(string userName)
    {
      int currentDbChannel = -1;
      try
      {        
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            currentDbChannel = _cards[userCopy.CardId].CurrentDbChannel(userName);
          } 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return currentDbChannel;
    }

    /// <summary>
    /// Gets the current channel name.
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>channel</returns>
    public string CurrentChannelName(string userName)
    {
      string currentChannelName = "";
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          int idChannel;
          IUser userCopy = GetUserFromContext(userName, out idChannel, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            ITvCardHandler tvCardHandler = _cards[userCopy.CardId];
            currentChannelName = tvCardHandler.CurrentChannelName(userCopy.Name, idChannel);
          }          
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return currentChannelName;
    }


    /// <summary>
    /// Returns if the tuner is locked onto a signal or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when tuner is locked to a signal otherwise false</returns>
    public bool TunerLocked(int cardId)
    {
      bool tunerLocked = true;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          tunerLocked = _cards[cardId].TunerLocked; 
        }                  
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return tunerLocked;
    }

    /// <summary>
    /// Returns the signal quality for a card
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal quality (0-100)</returns>
    public int SignalQuality(int cardId)
    {
      int signalQuality = -1;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          signalQuality = _cards[cardId].SignalQuality; 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return signalQuality;
    }

    /// <summary>
    /// Returns the signal level for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>signal level (0-100)</returns>
    public int SignalLevel(int cardId)
    {
      int signalLevel = -1;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          signalLevel = _cards[cardId].SignalLevel;
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return signalLevel;
    }

    /// <summary>
    /// Updates the signal state for a card.
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    public void UpdateSignalSate(int cardId)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].UpdateSignalSate();
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    /// <summary>
    /// Returns the current filename used for recording
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>filename or null when not recording</returns>
    public string RecordingFileName(string userName)
    {
      string recordingFileName = "";
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            recordingFileName = _cards[userCopy.CardId].Recorder.FileName(userCopy.Name); 
          }          
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return recordingFileName;
    }

    public string TimeShiftFileName(string userName, int cardId)
    {
      string timeShiftFileName = "";
      try
      {
        if (IsValidTvControllerParams(userName, cardId))
        {
          IUser user = GetUserFromSpecificContext(userName, cardId);
          if (user != null)
          {
            timeShiftFileName = _cards[user.CardId].TimeShifter.FileName(ref user);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return timeShiftFileName;
    }

    private IUser GetUserFromSpecificContext(string userName, int cardId)
    {
      IUser user = null;
      if (cardId > 0)
      {
        ITvCardHandler tvCardHandler = _cards[cardId];
        if (tvCardHandler  != null)
        {
          user = tvCardHandler.UserManagement.GetUserCopy(userName);
        }
      }
            
      return user;
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    public bool TimeShiftGetCurrentFilePosition(string userName, ref long position, ref long bufferId)
    {
      bool timeShiftGetCurrentFilePosition = false;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            timeShiftGetCurrentFilePosition = _cards[userCopy.CardId].TimeShifter.GetCurrentFilePosition(userName, ref position, ref bufferId);  
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return timeShiftGetCurrentFilePosition;
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(string userName)
    {
      bool isTimeShifting = false;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName);
          if (userCopy != null)
          {
            isTimeShifting = userCopy != null && _cards[userCopy.CardId].TimeShifter.IsTimeShifting(userCopy);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return isTimeShifting;
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

    /// <summary>
    /// This function checks if a spedific schedule should be recorded at the given time.
    /// </summary>
    /// <param name="time">the time to check for recordings.</param>
    /// <param name="scheduleId">the time id of the recording.</param>
    /// <returns>true if any recording due to time</returns>
    public bool IsTimeToRecord(DateTime time, int scheduleId)
    {
      Schedule schedule = ScheduleManagement.GetSchedule(scheduleId);
      return _scheduler.IsTimeToRecord(schedule, time);
    }


    /// <summary>
    /// Does the card support conditional access?
    /// </summary>
    /// <param name="cardId">The ID of the card to check.</param>
    /// <return><c>true</c> if the card supports conditional access, otherwise <c>false</c></return>
    public bool IsConditionalAccessSupported(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
      {
        return _cards[cardId].IsConditionalAccessSupported;
      }
      else
      {
        return false;
      }
    }
    

    /// <summary>
    /// Determines if any card is currently busy recording
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if a card is recording; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAnyCardRecording()
    {
      try
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
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return false;
    }

    /// <summary>
    /// Determines if any card is currently busy recording or timeshifting
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="isUserTS">true if the specified user is timeshifting</param>
    /// <param name="isAnyUserTS">true if any user (except for the userTS) is timeshifting</param>
    /// <param name="isRec">true if recording</param>
    /// <returns>
    /// 	<c>true</c> if a card is recording or timeshifting; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAnyCardRecordingOrTimeshifting(string userName, out bool isUserTS, out bool isAnyUserTS, out bool isRec)
    {
      isUserTS = false;
      isAnyUserTS = false;
      isRec = false;

      try
      {
        Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
        while (en.MoveNext())
        {
          ITvCardHandler card = en.Current.Value;
          IUser user = card.UserManagement.GetUserCopy(userName);

          if (!isRec)
          {
            isRec = card.Recorder.IsAnySubChannelRecording;
          }
          if (!isUserTS)
          {
            isUserTS = card.TimeShifter.IsTimeShifting(user);
          }

          isAnyUserTS = card.UserManagement.IsAnyUserTimeShifting();
        }

        if (isRec || isUserTS || isAnyUserTS)
        {
          return true;
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return false;
    }

    /// <summary>
    /// Determines whether the specified channel name is recording.
    /// </summary>
    /// <param name="channelName">Name of the channel.</param>
    /// <param name="idChannel"></param>
    /// <param name="card">The vcard.</param>
    /// <returns>
    /// 	<c>true</c> if the specified channel name is recording; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRecording(int idChannel, out IVirtualCard card)
    {
      bool isRecording = false;
      card = null;
      try
      {
        Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
        while (en.MoveNext())
        {
          ITvCardHandler tvcard = en.Current.Value;
          IUser user = tvcard.UserManagement.GetUserRecordingChannel(idChannel);
          if (user != null && user.UserType == UserType.Scheduler)
          {
            card = GetVirtualCard(user);
            isRecording = true;
            break;
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return isRecording;
    }

    public bool IsRecording(int idChannel, int idCard)
    {
      bool isRecording = false;
      try
      {
        ITvCardHandler tvcard = CardCollection[idCard];                        
        if (tvcard != null)
        {
          IUser user = tvcard.UserManagement.GetUserRecordingChannel(idChannel);
          if (user != null && user.UserType == UserType.Scheduler)
          {            
            isRecording = true;            
          }
        }                
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return isRecording;
    }

    public List<IVirtualCard> GetAllRecordingCards()
    {
      var recCards = new List<IVirtualCard>();
      foreach (ITvCardHandler card in _cards.Values)
      {
        IList<IUser> recUsers = card.UserManagement.GetAllRecordingUsersCopy();
        recCards.AddRange(recUsers.Select(recUser => GetVirtualCard(recUser)).Cast<IVirtualCard>());
      }
      return recCards;
    }

    

    /// <summary>
    /// Returns if the card is recording or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true when card is recording otherwise false</returns>
    public bool IsRecording(ref IUser user)
    {
      bool isRecording = false;
      if (ValidateTvControllerParams(user))
      {
        RefreshUserFromSpecificContext(ref user);
        if (user.CardId > 0)
        {
          ITvCardHandler tvCardhandler;
          bool hasCard = _cards.TryGetValue(user.CardId, out tvCardhandler);
          if (hasCard)
          {
           isRecording = tvCardhandler.Recorder.IsRecording(user.Name);   
          }                    
        }        
      }      
      return isRecording;
    }

    /// <summary>
    /// Returns if the card is scanning or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is scanning otherwise false</returns>
    public bool IsScanning(int cardId)
    {
      bool isScanning = false;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          isScanning = _cards[cardId].Scanner.IsScanning; 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return isScanning;
    }

    /// <summary>
    /// Returns if the card is grabbing the epg or not
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <returns>true when card is grabbing the epg  otherwise false</returns>
    public bool IsGrabbingEpg(int cardId)
    {
      bool isGrabbingEpg = false;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          isGrabbingEpg = _cards[cardId].Epg.IsGrabbing;  
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return isGrabbingEpg;
    }

    /// <summary>
    /// Returns if the card is grabbing teletext or not
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>true when card is grabbing teletext otherwise false</returns>
    public bool IsGrabbingTeletext(IUser user)
    {
      bool isGrabbingTeletext = false;
      if (ValidateTvControllerParams(user))
      {
        RefreshUserFromSpecificContext(ref user);
        isGrabbingTeletext = user != null && _cards[user.CardId].Teletext.IsGrabbingTeletext(user); 
      }                  
      return isGrabbingTeletext;
    }

    /// <summary>
    /// Returns if the channel to which the card is currently tuned
    /// has teletext or not
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>yes if channel has teletext otherwise false</returns>
    public bool HasTeletext(string userName)
    {
      bool hasTeletext = false;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            hasTeletext = userCopy != null && _cards[userCopy.CardId].Teletext.HasTeletext(userName);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return hasTeletext;
    }

    /// <summary>
    /// Returns the rotation time for a specific teletext page
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="pageNumber">The pagenumber (0x100-0x899)</param>
    /// <returns>timespan containing the rotation time</returns>
    public TimeSpan TeletextRotation(string userName, int pageNumber)
    {
      var teletextRotation = new TimeSpan(0, 0, 15);
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            teletextRotation = _cards[userCopy.CardId].Teletext.TeletextRotation(userName, pageNumber);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return teletextRotation;
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="idChannel"> </param>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime TimeShiftStarted(IUser user, int idChannel)
    {
      DateTime timeShiftStarted = DateTime.MinValue;
      if (ValidateTvControllerParams(user))
      {
        RefreshUserFromSpecificContext(ref user);
        timeShiftStarted = _cards[user.CardId].TimeShifter.TimeShiftStarted(user.Name, idChannel);
      }      
      return timeShiftStarted;
    }

    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted(IUser user)
    {
      DateTime recordingStarted = DateTime.MinValue;
      if (ValidateTvControllerParams(user))
      {
        RefreshUserFromSpecificContext(ref user);
        recordingStarted = _cards[user.CardId].Recorder.RecordingStarted(user.Name); 
      }      
      return recordingStarted;
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
      try
      {
        TsCopier copier = new TsCopier(position1, bufferFile1, position2, bufferFile2, recordingFile);
        Thread worker = new Thread(new ThreadStart(copier.DoCopy));
        worker.Start();
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    /// <summary>
    /// Returns whether the channel to which the card is tuned is
    /// scrambled or not.
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>yes if channel is scrambled and CI/CAM cannot decode it, otherwise false</returns>
    public bool IsScrambled(string userName)
    {
      bool isScrambled = false;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            isScrambled = _cards[userCopy.CardId].IsScrambled(userName);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return isScrambled;
    }

    public bool IsScrambled(int cardId, int subChannel)
    {
      return _cards[cardId].IsScrambled(subChannel);
    }

    /// <summary>
    /// returns the min/max channel numbers for analog cards
    /// </summary>
    public int MinChannel(int cardId)
    {
      int minChannel = 0;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          minChannel = _cards[cardId].MinChannel; 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return minChannel;
    }

    public int MaxChannel(int cardId)
    {
      int maxChannel = 0;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          maxChannel = _cards[cardId].MaxChannel; 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
      return maxChannel;        
    }


    /// <summary>
    /// Gets the number of channels decrypting.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting(int cardId)
    {
      int numberOfChannelsDecrypting = 0;
      if (ValidateTvControllerParams(cardId))
      {
        numberOfChannelsDecrypting = _cards[cardId].NumberOfChannelsDecrypting;
      }
      return numberOfChannelsDecrypting;      
    }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idCard"> </param>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    public TvResult Scan(string userName, int idCard, out IUser user, IChannel channel, int idChannel)
    {
      user = null;
      TvResult result = TvResult.UnknownError;
      try
      {
        if (ValidateTvControllerParams(userName) && ValidateTvControllerParams(channel))
        {
          StopEPGgrabber();
          user = new User(userName, UserType.Scanner, idCard);
          ITvCardHandler cardHandler = _cards[idCard];
          if (cardHandler.DataBaseCard.Enabled)
          {
            FireScanningStartedEvent(user, channel);
            result = cardHandler.Tuner.Scan(ref user, channel, idChannel);
          }
          else
          {
            result = TvResult.CardIsDisabled;
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      finally
      {
        FireScanningStoppedEvent(user, channel);
        StartEPGgrabber();
      }
      return result;
    }

    /// <summary>
    /// Tunes the the specified card to the channel.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idCard"> </param>
    /// <param name="user">The user.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>true if succeeded</returns>
    public TvResult Tune(string userName, int idCard, out IUser user, IChannel channel, int idChannel)
    {
      TvResult result = TvResult.UnknownError;
      user = null;
      try
      {        
        ITvCardHandler tvCardHandler;        

        if (CardCollection.TryGetValue(idCard, out tvCardHandler))
        {
          ICardTuneReservationTicket ticket = null;
          ICardReservation cardreservationImpl = new CardReservationTimeshifting();
          try
          {
            user = new User { CardId = idCard, Name = userName };
            ticket = cardreservationImpl.RequestCardTuneReservation(tvCardHandler, channel, user, idChannel);
            result = Tune(ref user, channel, idChannel, ticket);
          }
          catch (Exception)
          {
            CardReservationHelper.CancelCardReservation(tvCardHandler, ticket);
            throw;
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
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
      ICardReservation cardResImpl = new CardReservationTimeshifting();
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
      TvResult result = TvResult.UnknownError;
      if (ValidateTvControllerParams(user) && ValidateTvControllerParams(channel))
      {
        try
        {
          RefreshUserFromSpecificContext(ref user);
          int cardId = user.CardId;
          ITvCardHandler cardHandler = _cards[cardId];
          if (cardHandler.DataBaseCard.Enabled)
          {
            FireStartZapChannelEvent(user, channel);
            var resTicket = ticket as ICardTuneReservationTicket;
            var resCardResImpl = cardResImpl as ICardReservation;
            if (resTicket != null && resCardResImpl != null)
            {
              result = resCardResImpl.Tune(cardHandler, ref user, channel, idChannel, resTicket);
            }  
          }
          else
          {
            result = TvResult.CardIsDisabled;
          }
        }
        finally
        {
          FireEndZapChannelEvent(user, channel);          
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
    /// <returns>true if succeeded</returns>
    private void FireStartZapChannelEvent(IUser user, IChannel channel)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(user), (User)user, channel));
    }

    private void FireEndZapChannelEvent(IUser user, IChannel channel)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(user), (User)user, channel));
    }

    private void FireEpgGrabbingStartedEvent(IUser user)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.EpgGrabbingStarted, new VirtualCard(user), (User)user));
    }    

    private void FireEpgGrabbingStoppedEvent(IUser user)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.EpgGrabbingStopped, GetVirtualCard(user), (User)user));
    }

    private void FireScanningStartedEvent(IUser user, IChannel channel)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.ScanningStarted, GetVirtualCard(user), (User)user, channel));
    }

    private void FireScanningStoppedEvent(IUser user, IChannel channel)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.ScanningStopped, GetVirtualCard(user), (User)user, channel));
    }

    private void FireStartTimeShiftingEvent(IUser user)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.StartTimeShifting, GetVirtualCard(user), (User)user));
    }

    private void FireEndTimeShiftingEvent(IUser user)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.EndTimeShifting, GetVirtualCard(user), (User)user));
    }    

    private void FireTimeShiftingParkedEvent(IUser user)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.TimeShiftingParked, GetVirtualCard(user), (User)user));
    }

    private void FireTimeShiftingUnParkedEvent(IUser user, VirtualCard virtualCard)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.TimeShiftingUnParked, virtualCard, (User)user));      
    }

    private void FireForcefullyStoppedTimeShiftingEvent(IUser user)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.ForcefullyStoppedTimeShifting, GetVirtualCard(user), (User) user));
    }

    private void FireScheduleAddedEvent()
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.ScheduledAdded));
    }

    private void FireScheduleAddedEvent(TvServerEventArgs args)
    {
      Fire(this,
           new TvServerEventArgs(TvServerEventType.ScheduledAdded, args.Schedules, args.Conflicts,
                                 args.ArgsUpdatedState));
    }

    private void FireChannelStatesEvent(IUser user)
    {
      Fire(this, new TvServerEventArgs(TvServerEventType.ChannelStatesChanged, new VirtualCard(user), (User)user));
    }

    /// <summary>
    /// turn on/off teletext grabbing
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="onOff">turn on/off teletext grabbing</param>
    public void GrabTeletext(string userName, bool onOff)
    {
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            _cards[userCopy.CardId].Teletext.GrabTeletext(userName, onOff);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      
    }

    /// <summary>
    /// Gets the teletext page.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="subPageNumber">The sub page number.</param>
    /// <returns></returns>
    public byte[] GetTeletextPage(string userName, int pageNumber, int subPageNumber)
    {
      var teletextPage = new byte[] {1};
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            teletextPage = _cards[userCopy.CardId].Teletext.GetTeletextPage(userName, pageNumber, subPageNumber);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return teletextPage;
    }

    /// <summary>
    /// Gets the number of subpages for a teletext page.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns></returns>
    public int SubPageCount(string userName, int pageNumber)
    {
      int subPageCount = -1;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            subPageCount = _cards[userCopy.CardId].Teletext.SubPageCount(userName, pageNumber);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return subPageCount;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the red button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the red button</returns>
    public int GetTeletextRedPageNumber(string userName)
    {
      int teletextRedPageNumber = -1;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            teletextRedPageNumber = _cards[userCopy.CardId].Teletext.GetTeletextRedPageNumber(userName);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return teletextRedPageNumber;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the green button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the green button</returns>
    public int GetTeletextGreenPageNumber(string userName)
    {
      int teletextGreenPageNumber = -1;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            teletextGreenPageNumber = _cards[userCopy.CardId].Teletext.GetTeletextGreenPageNumber(userName);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return teletextGreenPageNumber;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the yellow button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the yellow button</returns>
    public int GetTeletextYellowPageNumber(string userName)
    {
      int teletextYellowPageNumber = -1;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            teletextYellowPageNumber = _cards[userCopy.CardId].Teletext.GetTeletextYellowPageNumber(userName);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return teletextYellowPageNumber;
    }

    /// <summary>
    /// Gets the teletext pagenumber for the blue button
    /// </summary>
    /// <param name="userName"> </param>
    /// <returns>Teletext pagenumber for the blue button</returns>
    public int GetTeletextBluePageNumber(string userName)
    {
      int teletextBluePageNumber = -1;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            teletextBluePageNumber = _cards[userCopy.CardId].Teletext.GetTeletextBluePageNumber(userName);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return teletextBluePageNumber;
    }

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref IUser user, ref string fileName, int idChannel)
    {
      TvResult result = TvResult.UnknownError;
      if (ValidateTvControllerParams(user))
      {
        try
        {
          RefreshUserFromSpecificContext(ref user);
          int cardId = user.CardId;          
          FireStartTimeShiftingEvent(user);
          StopEPGgrabber();

          bool isTimeShifting;
          ITvCardHandler tvCardHandler = _cards[cardId];
          try
          {
            isTimeShifting = tvCardHandler.TimeShifter.IsTimeShifting(user);
          }
          catch (Exception ex)
          {
            isTimeShifting = false;
            this.LogError(ex, "Exception in checking");
          }
          int subChannelId = tvCardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name);
          result = tvCardHandler.TimeShifter.Start(ref user, ref fileName, subChannelId, idChannel);
          if (result == TvResult.Succeeded)
          {
            if (!isTimeShifting)
            {
              this.LogInfo("user:{0} card:{1} sub:{2} add stream:{3}", user.Name, user.CardId, subChannelId, fileName);
              if (File.Exists(fileName))
              {
                if (_streamer != null)
                {
                  _streamer.Start();
                  //  Default to tv
                  MediaTypeEnum mediaType = MediaTypeEnum.TV;

                  ITvSubChannel subChannel = tvCardHandler.Card.GetSubChannel(subChannelId);

                  if (subChannel != null && subChannel.CurrentChannel != null)
                  {
                    mediaType = subChannel.CurrentChannel.MediaType;
                  }
                  else
                  {
                    this.LogError("ParkedSubChannel or CurrentChannel is null when starting streaming");
                  }

                  var stream = new RtspStream(String.Format("stream{0}.{1}", cardId, subChannelId), fileName,
                                                    tvCardHandler.Card, mediaType);
                  _streamer.AddStream(stream);
                }
                else
                {
                  this.LogError("could not start streaming server.");
                }
              }
              else
              {
                this.LogDebug("Controller: streaming: file not found:{0}", fileName);
              }
            }
          }

          if (result == TvResult.Succeeded)
          {
            this.LogDebug("Controller: StartTimeShifting started on card:{0} to {1}", user.CardId, fileName);
          }

          return result;
        }
        catch (Exception ex)
        {
          this.LogError(ex);
        }  
      }

      return result;
    }

    public void StopCard(int idCard)
    {
      try
      {
        if (ValidateTvControllerParams(idCard))
        {
          _cards[idCard].StopCard();
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public bool ParkTimeShifting(string userName, double duration, int idChannel, out IUser user)
    {
      user = null;
      bool result = false;
      try
      {        
        if (ValidateTvControllerParams(userName))
        {
          ITvCardHandler cardHandler = GetCardHandlerByChannel(idChannel, TvUsage.Timeshifting);
          if (cardHandler != null)
          {
            if (cardHandler.DataBaseCard.Enabled)
            {
              user = GetUserFromContext(userName, TvUsage.Timeshifting);
              if (cardHandler.TimeShifter.IsTimeShifting(user))
              {
                FireTimeShiftingParkedEvent(user);                
                this.LogDebug("Controller: ParkTimeShifting {0}", cardHandler.DataBaseCard.IdCard);
                cardHandler.ParkedUserManagement.ParkUser(ref user, duration, idChannel);
                UpdateChannelStatesForUsers();
                result = true;
              }
            }
          }
          else
          {
            this.LogError("StopTimeShifting - could not find channel to park. {0} - {1} - {2}", user.Name, idChannel, duration);
          }

        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return result;
    }

    private int GetTimeShiftingChannelIdFromContext(string userName)
    {      
      int timeshiftingChannelId = -1;
      foreach (ITvCardHandler cardHandler in _cards.Values)
      {
        timeshiftingChannelId = cardHandler.UserManagement.GetTimeshiftingChannelId(userName);
        if (timeshiftingChannelId > 0)
        {
          break;
        }                
      }      
      return timeshiftingChannelId;
    }

    private int GetTimeShiftingChannelIdFromContext (IUser user)
    {      
      int currentCardId = user.CardId;
      int timeshiftingChannelId = -1;
      if (currentCardId > 0)
      {
        ITvCardHandler timeShiftingCardHandler = CardCollection[currentCardId];        
        if (timeShiftingCardHandler != null)
        {
          timeshiftingChannelId = timeShiftingCardHandler.UserManagement.GetTimeshiftingChannelId(user.Name);
        }  
      }
      
      return timeshiftingChannelId;
    }

    public bool UnParkTimeShifting(string userName, double duration, int idChannel, out IUser user, out IVirtualCard card)
    {
      user = null;
      bool result = false;
      card = null;
      try
      {                
        ITvCardHandler cardHandler = GetCardHandlerByParkedChannelAndDuration(idChannel, duration);
        if (cardHandler != null)
        {
          if (cardHandler.DataBaseCard.Enabled)
          {
            int timeshiftingChannelId;
            user = GetUserFromContext(userName, out timeshiftingChannelId, TvUsage.Timeshifting);
            if (user == null)
            {
              user = CreateTimeshiftingUserWithPriority(userName);
            }
            
            this.LogDebug("Controller: UnParkTimeShifting {0}", cardHandler.DataBaseCard.IdCard);
            cardHandler.ParkedUserManagement.UnParkUser(ref user, duration, idChannel);
            StopTimeShiftingAllChannelsExcept(user, idChannel);

            VirtualCard virtualCard = GetVirtualCard(user);
            card = virtualCard;

            FireTimeShiftingUnParkedEvent(user, virtualCard);            
            if (timeshiftingChannelId < 1)
            {              
              UpdateChannelStatesForUsers();
            }
            result = true;            
          } 
        }
        else
        {
          this.LogError("StopTimeShifting - could not find channel to unpark. {0} - {1} - {2}", userName, idChannel, duration);          
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      
      return result;
    }

    public bool StopTimeShifting(string userName, out IUser user, TvStoppedReason reason)
    {
      user = null;
      try
      {
        user = GetUserFromContext(userName, TvUsage.Timeshifting);
        ITvCardHandler tvCardHandler = _cards[user.CardId];
        return StopTimeShifting(ref user, reason, tvCardHandler.UserManagement.GetTimeshiftingChannelId(user.Name));
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return false;
    }

    public TvStoppedReason GetTvStoppedReason(IUser user)
    {
      TvStoppedReason result = TvStoppedReason.UnknownReason;
      if (ValidateTvControllerParams(user))
      {
        try
        {
          RefreshUserFromSpecificContext(ref user);
          if (_cards[user.CardId].DataBaseCard.Enabled)
          {
            result = _cards[user.CardId].UserManagement.GetTimeshiftStoppedReason(user.Name);  
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex);
        }
      }
      return result;
    }

    public bool StopTimeShifting(ref IUser user, TvStoppedReason reason, int channelId)
    {
      bool stopTimeShifting = false;
      if (ValidateTvControllerParams(user))
      {
        RefreshUserFromSpecificContext(ref user);
        _cards[user.CardId].UserManagement.SetTimeshiftStoppedReason(user.Name, reason);

        user.TvStoppedReason = reason;
        FireForcefullyStoppedTimeShiftingEvent(user);

        stopTimeShifting = StopTimeShifting(ref user, channelId);
      }
      return stopTimeShifting;
    }

    public bool StopTimeShifting(ref IUser user, int channelId)
    {
      if (!ValidateTvControllerParams(user))
      {
        return false;
      }
      try
      {
        //RefreshUserFromSpecificContext(ref user);        
        //RefreshTimeshiftingUserFromAnyContext(ref user);
        ITvCardHandler tvcard = GetCardHandlerByUserAndChannel(user.Name, channelId);        
        if (tvcard != null)
        {
          int cardId = tvcard.DataBaseCard.IdCard;          
          if (tvcard.DataBaseCard.Enabled == false)
          {
            return true;
          }


          var hybridCard = tvcard.Card as HybridCard;
          if (hybridCard != null)
          {
            if (!hybridCard.IsCardIdActive(cardId))
            {
              return true;
            }
          }

          tvcard.UserManagement.RefreshUser(ref user);
          if (!tvcard.TimeShifter.IsTimeShifting(user))
            return true;

          FireEndTimeShiftingEvent(user);
          

          if (tvcard.Recorder.IsRecording(user.Name))
            return true;

          this.LogDebug("Controller: StopTimeShifting {0}", cardId);
          return DoStopTimeShifting(ref user, cardId, channelId);
        }
        else
        {
          this.LogError("StopTimeShifting - could not find channel to stop. {0} - {1}", user.Name, channelId);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }

      return false;
    }

    

    private ITvCardHandler GetCardHandlerByParkedChannelAndDuration(int channelId, double duration)
    {            
      foreach (ITvCardHandler cardHandler in _cards.Values)
      {
        bool hasParkedUserWithDuration = cardHandler.ParkedUserManagement.HasParkedUserWithDuration(channelId, duration);
        if (hasParkedUserWithDuration)
        {
          return cardHandler;
        }            
      }
      return null;
    }

    private ITvCardHandler GetCardHandlerByChannel (int channelId, TvUsage tvUsage)
    {
      foreach (ITvCardHandler cardHandler in _cards.Values)
      {
        if (cardHandler.UserManagement.IsAnyUserLockedOnChannel(channelId, tvUsage))
        {
          return cardHandler;
        }         
      }
      return null;
    }

    private IUser GetUserFromContext(string userName, TvUsage? tvUsage = null)
    {
      int channelId;
      return GetUserFromContext(userName, out channelId, tvUsage);
    }

    private IUser GetUserFromContext(string userName, out int channelId, TvUsage? tvUsage = null)
    {
      channelId = -1;
      foreach (ITvCardHandler cardHandler in _cards.Values)
      {
        if (tvUsage.HasValue)
        {
          channelId = cardHandler.UserManagement.GetChannelId(userName, tvUsage.GetValueOrDefault());
          if (channelId < 1)
          {
            continue;
          }          
        }

        IUser user = cardHandler.UserManagement.GetUserCopy(userName);  
        if (user != null)
        {
          return user;
        }
      }
      return null;
    }

    private IUser GetUserFromContext(string userName, int idChannel)
    {
      foreach (ITvCardHandler cardHandler in _cards.Values)
      {
        if (idChannel == cardHandler.UserManagement.GetTimeshiftingChannelId(userName))
        {
          IUser user = cardHandler.UserManagement.GetUserCopy(userName);
          if (user != null)
          {
            return user;
          }
        }                
      }
      return null;
    }

   
    private ITvCardHandler GetCardHandlerByUserAndChannel(string userName, int channelId)
    {
      ITvCardHandler tvCardHandler = null;
      foreach (ITvCardHandler card in _cards.Values)
      {
        IUser user = card.UserManagement.GetUserCopy(userName);
        if (user != null)
        {
          if (card.UserManagement.GetSubChannelIdByChannelId(user.Name, channelId) > -1)
          {
            tvCardHandler = card;
            break;
          }          
        }        
      }

      return tvCardHandler;
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="user">User</param>
    /// <param name="channelId"> </param>
    /// <returns></returns>
    public bool StopTimeShifting(string userName, out IUser user)
    {
      user = null;
      try
      {
        int timeshiftingChannelId;
        user = GetUserFromContext(userName, out timeshiftingChannelId, TvUsage.Timeshifting);
        if (timeshiftingChannelId > 0)
        {
          return StopTimeShifting(ref user, timeshiftingChannelId);
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
      return false;
    }

    private bool DoStopTimeShifting(ref IUser user, int cardId, int idChannel)
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
        stopped = CardReservationHelper.Stop(tvcard, ref user, ticket, idChannel);
        if (stopped)
        {
          //we must not stop streaming if subchannel is still in use.
          ITvCardHandler tvCardHandler = _cards[user.CardId];          
          int subChannelByChannelId = tvCardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel);
          if (!tvCardHandler.UserManagement.ContainsUsersForSubchannel(subChannelByChannelId))
          {
            this.LogDebug("Controller:Timeshifting stopped on card:{0}", cardId);
            if (_streamer != null)
            {
              _streamer.Remove(String.Format("stream{0}.{1}", cardId, subChannelByChannelId));
            }
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
    /// <param name="userName"> </param>
    /// <param name="cardId"> </param>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns></returns>
    public TvResult StartRecording(string userName, int cardId, out IUser user, ref string fileName)
    {
      TvResult result = TvResult.UnknownError;
      user = null;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          user = new User {Name = userName, CardId = cardId};
          StopEPGgrabber();
          result = _cards[user.CardId].Recorder.Start(ref user, ref fileName);

          if (result == TvResult.Succeeded)
          {
            UpdateChannelStatesForUsers();
          }
          else
          {
            StartEPGgrabber();
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return result;
    }

    /// <summary>
    /// Stops recording.
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idCard"> </param>
    /// <param name="user">User</param>
    /// <returns></returns>
    public bool StopRecording(string userName, int idCard, out IUser user)
    {
      user = null;
      bool result = false;
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          user = UserFactory.CreateSchedulerUser();
          user.Name = userName;
          user.CardId = idCard;
          
          result = _cards[user.CardId].Recorder.Stop(ref user);

          if (result)
          {
            UpdateChannelStatesForUsers();
          }
          StartEPGgrabber();
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
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
      IChannel[] channels = null;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          StopEPGgrabber();
          ScanParameters settings = new ScanParameters();

          settings.TimeOutTune = SettingsManagement.GetValue("timeoutTune", 2);
          settings.TimeOutPAT = SettingsManagement.GetValue("timeoutPAT", 5);
          settings.TimeOutCAT = SettingsManagement.GetValue("timeoutCAT", 5);
          settings.TimeOutPMT = SettingsManagement.GetValue("timeoutPMT", 10);
          settings.TimeOutSDT = SettingsManagement.GetValue("timeoutSDT", 20);
          settings.TimeOutAnalog = SettingsManagement.GetValue("timeoutAnalog", 20);
          channels = _cards[cardId].Scanner.Scan(channel, settings); 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      finally
      {
        StartEPGgrabber();
      }
      return channels;
    }

    public IChannel[] ScanNIT(int cardId, IChannel channel)
    {
      IChannel[] scanNit = null;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          StopEPGgrabber();
          ScanParameters settings = new ScanParameters();
          settings.TimeOutTune = SettingsManagement.GetValue("timeoutTune", 2);
          scanNit = _cards[cardId].Scanner.ScanNIT(channel, settings);
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      finally
      {
        StartEPGgrabber();
      }
      return scanNit;
    }

    /// <summary>
    /// grabs the epg.
    /// </summary>
    /// <param name="grabber">EPG grabber</param>    
    /// <param name="user"> </param>
    /// <returns></returns>
    public bool GrabEpg(BaseEpgGrabber grabber, IUser user)
    {
      int cardId = user.CardId;
      bool grabEpg = false;
      this.LogInfo("Controller: GrabEpg on card ID == {0}", cardId);
      if (ValidateTvControllerParams(cardId))
      {
        grabEpg = _cards[cardId].Epg.Start(grabber); 
      }
      if (grabEpg)
      {
        FireEpgGrabbingStartedEvent(user);        
      }
      return grabEpg;
    }

    /// <summary>
    /// Aborts grabbing the epg. This also triggers the OnEpgReceived callback.
    /// </summary>
    public void AbortEPGGrabbing(int cardId)
    {
      this.LogInfo("Controller: AbortEPGGrabbing on card ID == {0}", cardId);
      if (ValidateTvControllerParams(cardId))
      {
        _cards[cardId].Epg.Abort();        
      }
      else
      {
        this.LogError("Controller: AbortEPGGrabbing - invalid cardId");
      }      
    }

    /// <summary>
    /// Epgs the specified card id.
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <returns></returns>
    public List<EpgChannel> Epg(int cardId)
    {
      var epgChannels = new List<EpgChannel>();
      if (ValidateTvControllerParams(cardId))
      {
        epgChannels = _cards[cardId].Epg.Epg;
      }
      return epgChannels;      
    }

    /// <summary>
    /// Deletes the recording from database and disk
    /// </summary>
    /// <param name="idRecording">The id recording.</param>
    public bool DeleteRecording(int idRecording)
    {
      try
      {
        Recording rec = TVDatabase.TVBusinessLayer.RecordingManagement.GetRecording(idRecording);
        if (rec == null)
        {
          return false;
        }

        if (_streamer != null)
        {
          _streamer.RemoveFile(rec.FileName);
        }
        bool result = RecordingFileHandler.DeleteRecordingOnDisk(rec.FileName);
        if (result)
        {
          RecordingManagement.DeleteRecording(rec.IdRecording);
          return true;
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e, "Controller: Can't delete recording");        
      }
      return false;
    }

    /// <summary>
    /// Checks if the files of a recording still exist
    /// </summary>
    /// <param name="rec">recording</param>
    private bool IsRecordingValid(Recording rec)
    {
      try
      {
        if (rec == null)
        {
          return false;
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
      bool foundInvalidRecording = false;
      try
      {
        this.LogDebug("Deleting invalid recordings");
        IList<Recording> itemlist = RecordingManagement.ListAllRecordingsByMediaType(MediaTypeEnum.TV);        
        foreach (Recording rec in itemlist.Where(rec => !IsRecordingValid(rec)))
        {
          try
          {
            RecordingManagement.DeleteRecording(rec.IdRecording);
          }
          catch (Exception e)
          {
            this.LogError("Controller: Can't delete invalid recording", e);
          }
          foundInvalidRecording = true;
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return foundInvalidRecording;
    }

    /// <summary>
    /// Deletes watched recordings from database.
    /// </summary>
    public bool DeleteWatchedRecordings(string currentTitle)
    {
      bool foundWatchedRecordings = false;
      try
      {
        IList<Recording> itemlist = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllRecordingsByMediaType(MediaTypeEnum.TV);        
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
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return foundWatchedRecordings;
    }

    /// <summary>
    /// returns which schedule the card specified is currently recording
    /// </summary>
    /// <param name="cardId">card id</param>
    /// <param name="userName"> </param>
    /// <returns>
    /// id of Schedule or -1 if  card not recording
    /// </returns>
    public int GetRecordingSchedule(int cardId, string userName)
    {
      int recordingSchedule = -1;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          ITvCardHandler tvCardHandler = _cards[cardId];
          if (tvCardHandler.DataBaseCard.Enabled)
          {
            IUser user = tvCardHandler.UserManagement.GetUserCopy(userName);
            int channelId = tvCardHandler.UserManagement.GetTimeshiftingChannelId(user.Name);
            recordingSchedule = _scheduler.GetRecordingScheduleForCard(cardId, channelId);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);        
      }
      return recordingSchedule;
    }

    #region audio streams

   

 

    public string GetStreamingUrl(string userName)
    {
      string streamingUrl = "";
      try
      {
        if (ValidateTvControllerParams(userName))
        {
          IUser userCopy = GetUserFromContext(userName, TvUsage.Timeshifting);
          if (userCopy != null)
          {
            ITvCardHandler tvCardHandler = _cards[userCopy.CardId];
            if (tvCardHandler.DataBaseCard.Enabled)
            {
              if (_streamer != null)
              {
                streamingUrl = String.Format("rtsp://{0}:{1}/stream{2}.{3}", _hostName, _streamer.Port, userCopy.CardId,
                                             tvCardHandler.UserManagement.GetTimeshiftingSubChannel(userCopy.Name));
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e, "Controller: Can't get streaming url");        
      }
      return streamingUrl;
    }

    public string GetRecordingUrl(int idRecording)
    {
      try
      {
        Recording recording = TVDatabase.TVBusinessLayer.RecordingManagement.GetRecording(idRecording);
        if (recording == null)
          return "";
        if (recording.FileName == null)
          return "";
        if (recording.FileName.Length == 0)
          return "";

        try
        {
          if (File.Exists(recording.FileName))
          {            
            _streamer.Start();
            string streamName = String.Format("{0:X}", recording.FileName.GetHashCode());
            RtspStream stream = new RtspStream(streamName, recording.FileName, recording.Title);
            _streamer.AddStream(stream);
            string url = String.Format("rtsp://{0}:{1}/{2}", _hostName, _streamer.Port, streamName);
            this.LogInfo("Controller: streaming url:{0} file:{1}", url, recording.FileName);
            return url;
          }
        }
        catch (Exception)
        {
          this.LogError("Controller: Can't get recroding url - First catch");
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e, "Controller: Can't get recroding url - Second catch");        
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
        Recording recording = TVDatabase.TVBusinessLayer.RecordingManagement.GetRecording(idRecording);
        if (recording == null)
          return "";
        if (recording.FileName == null)
          return "";
        if (recording.FileName.Length == 0)
          return "";

        try
        {
          string chapterFile = Path.ChangeExtension(recording.FileName, ".txt");
          if (File.Exists(chapterFile))
          {
            using (var chapters = new StreamReader(chapterFile))
            {
              return chapters.ReadToEnd();
            }
          }
        }
        catch (Exception)
        {
          this.LogError("Controller: Can't get recording chapters - First catch");
        }
      }
      catch (Exception e)
      {        
        HandleControllerException(e, "Controller: Can't get recording chapters - Second catch");
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
        if (_streamer != null)
        {
          _streamer.Start();
          string streamName = String.Format("{0:X}", fileName.GetHashCode());
          RtspStream stream = new RtspStream(streamName, fileName, streamName);
          _streamer.AddStream(stream);
          string url = String.Format("rtsp://{0}:{1}/{2}", _hostName, _streamer.Port, streamName);
          this.LogInfo("Controller: streaming url:{0} file:{1}", url, fileName);
          return url;
        }
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

      foreach (ITvCardHandler tvCardHandler in _cards.Values)
      {
        this.LogInfo("Controller: dispose card:{0}", tvCardHandler.CardName);
        try
        {
          tvCardHandler.ParkedUserManagement.CancelAllParkedUsers();
          tvCardHandler.StopCard();
          tvCardHandler.Dispose();
        }
        catch (Exception ex)
        {
          this.LogError(ex);
        }
      }      
    }

    /// <summary>
    /// Query what card would be used for timeshifting on any given channel
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel">The id channel.</param>
    /// <returns>
    /// CardDetail which would be used when doing the actual timeshifting.
    /// </returns>
    public int TimeShiftingWouldUseCard(string userName, int idChannel)
    {
      try
      {
        if (userName == null)
          return -1;

        Channel channel = ChannelManagement.GetChannel(idChannel, ChannelIncludeRelationEnum.TuningDetails);
        this.LogDebug("Controller: TimeShiftingWouldUseCard {0} {1}", channel.DisplayName, channel.IdChannel);


        IUser userCopy = GetUserFromContext(userName, idChannel);
        if (userCopy == null)
        {
          userCopy = UserFactory.CreateBasicUser(userName, idChannel);
        }
        userCopy.Priority = UserFactory.GetDefaultPriority(userName);

        List<CardDetail> freeCards = _cardAllocation.GetFreeCardsForChannel(_cards, channel, userCopy);
        if (freeCards.Count > 0)
        {
          //get first free card
          return freeCards[0].Id;
        }      
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return -1;
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="kickCardId"> </param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <param name="kickableCards"> </param>
    /// <param name="forceCardId">Indicated, if the card should be forced</param>
    /// <param name="user">user credentials.</param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>    
    public TvResult StartTimeShifting(string userName, int idChannel, int? kickCardId, out IVirtualCard card, out Dictionary<int, List<IUser>> kickableCards, bool forceCardId, out IUser user)
    {
      card = null;
      kickableCards = null;
      user = null;
      TvResult result = TvResult.UnknownError;
      try
      {
        user = GetUserFromContext(userName, TvUsage.Timeshifting);
        if (user == null)
        {
          user = CreateTimeshiftingUserWithPriority(userName);
        }
        double? parkedDuration;
        bool cardChanged;
        result = StartTimeShifting(ref user, idChannel, kickCardId, out card, out kickableCards, forceCardId, out cardChanged, out parkedDuration);        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
      return result;      
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="kickCardId"> </param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <param name="kickableCards"> </param>
    /// <param name="forceCardId">Indicated, if the card should be forced</param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <param name="parkedDuration"> </param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    private TvResult StartTimeShifting(ref IUser user, int idChannel, int? kickCardId, out IVirtualCard card, out Dictionary<int, List<IUser>> kickableCards, bool forceCardId, out bool cardChanged, out double? parkedDuration)
    {
      parkedDuration = null;
      TvResult result = TvResult.UnknownError;
      kickableCards = null;
      card = null;
      cardChanged = false;      
      if (user != null)
      {
        int oldCardId = user.CardId;
        string initialTimeshiftingFile = "";
        if (oldCardId > 0)
        {
          initialTimeshiftingFile = TimeShiftFileName(user.Name, user.CardId);  
        }
        
        user.Priority = UserFactory.GetDefaultPriority(user.Name, user.Priority);
        Channel channel = ChannelManagement.GetChannel(idChannel, ChannelIncludeRelationEnum.TuningDetails);
        this.LogDebug("Controller: StartTimeShifting {0} {1}", channel.DisplayName, channel.IdChannel);
        StopEPGgrabber();

        IDictionary<CardDetail, ICardTuneReservationTicket> tickets = null;
        try
        {
          var cardAllocationStatic = new AdvancedCardAllocationStatic();
          List<CardDetail> freeCardsForReservation = cardAllocationStatic.GetFreeCardsForChannel(_cards, channel, user);

          if (HasFreeCards(freeCardsForReservation))
          {
            tickets = IterateCardsUntilTimeshifting(
              ref user,
              channel,
              forceCardId,
              freeCardsForReservation,
              kickCardId,
              out kickableCards,
              ref result, ref card, ref parkedDuration);
          }
          else
          {
            this.LogDebug("Controller: StartTimeShifting failed:{0} - no cards found during initial card allocation", result);
            result = AllCardsBusy(result);
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex);
          result = TvResult.UnknownError;
        }
        finally
        {
          CardReservationHelper.CancelAllCardReservations(tickets);
          if (!HasTvSucceeded(result))
          {
            StartEPGgrabber();
          }
          if (card != null)
          {
            cardChanged = card.Id != oldCardId;
            if (!cardChanged)
            {
              cardChanged = initialTimeshiftingFile != card.TimeShiftFileName;
            }
          }
        }
      }
      
      return result;
    }



    private IDictionary<CardDetail, ICardTuneReservationTicket> IterateCardsUntilTimeshifting(ref IUser user, Channel channel, 
        bool forceCardId, ICollection<CardDetail> freeCardsForReservation, int? kickCardId, out Dictionary<int, List<IUser>> kickableCards, 
        ref TvResult result, ref IVirtualCard card, ref double? parkedDuration)
    {
      kickableCards = null;
      var cardResImpl = new CardReservationTimeshifting();

      IDictionary<CardDetail, ICardTuneReservationTicket> tickets = null;
      int cardsIterated = 0;
      bool moreCardsAvailable = true;
      ICollection<CardDetail> freeCardsIterated = UpdateCardsIteratedBasedOnForceCardId(user, forceCardId, freeCardsForReservation);
      while (moreCardsAvailable && !HasTvSucceeded(result))
      {
        tickets = CardReservationHelper.RequestCardReservations(user, freeCardsForReservation, cardResImpl, freeCardsIterated, channel.IdChannel);
        AdjustCardReservations(user, tickets, channel.IdChannel, cardResImpl);

        List<ICardTuneReservationTicket> ticketsList = tickets.Values.ToList();
        if (HasTickets(ticketsList))
        {
          var cardAllocationTicket = new AdvancedCardAllocationTicket(ticketsList);
          IList<CardDetail> freeCards = cardAllocationTicket.UpdateFreeCardsForChannelBasedOnTicket(freeCardsForReservation, user, out result);
          CardReservationHelper.CancelCardReservationsExceedingMaxConcurrentTickets(tickets, freeCards);
          CardReservationHelper.CancelCardReservationsNotFoundInFreeCards(freeCardsForReservation, tickets, freeCards);
          int maxCards = GetMaxCards(freeCards);
          CardReservationHelper.CancelCardReservationsBasedOnMaxCardsLimit(tickets, freeCards, maxCards);
          UpdateFreeCardsIterated(freeCardsIterated, freeCards); //keep tracks of what card details have been iterated here.
          moreCardsAvailable = HasFreeCards(freeCards);
          if (moreCardsAvailable)
          {
            moreCardsAvailable = IterateTicketsUntilTimeshifting(
              ref user,
              channel,
              tickets,
              cardResImpl,              
              freeCards,
              maxCards,
              kickCardId,
              out kickableCards,
              ref card, ref result, ref cardsIterated, ref parkedDuration);
          }
          else
          {
            result = AllCardsBusy(result);
            this.LogDebug("Controller: StartTimeShifting failed:{0}", result);
          }
        }
        else
        {
          result = AllCardsBusy(result);
          this.LogDebug("Controller: StartTimeShifting failed:{0} - no card reservation(s) could be made", result);
          moreCardsAvailable = false;
        }
      } //end of while             

      return tickets;
    }

    private void AdjustCardReservations(IUser user, IDictionary<CardDetail, ICardTuneReservationTicket> tickets, int idChannel, ICardReservation cardResImpl)
    {
      long otherMux = -1;
      IList<CardDetail> removeList = new List<CardDetail>();

      foreach (KeyValuePair<CardDetail, ICardTuneReservationTicket> cardResKVP in tickets)
      {
        ICardTuneReservationTicket ticket = cardResKVP.Value;
        CardDetail cardDetail = cardResKVP.Key;
        if (ticket != null && ticket.ChannelTimeshiftingOnOtherMux.HasValue)
        {
          otherMux = ticket.ChannelTimeshiftingOnOtherMux.GetValueOrDefault();
          ITvCardHandler cardHandler = _cards[ticket.CardId];
          CardReservationHelper.CancelCardReservation(cardHandler, ticket);
          removeList.Add(cardDetail);          
        }
      }

      foreach (CardDetail cardDetail in removeList)
      {
        tickets.Remove(cardDetail);
      }

      if (removeList.Count > 0)
      {
        foreach (KeyValuePair<CardDetail, ICardTuneReservationTicket> cardResKVP in tickets)
        {
          ICardTuneReservationTicket ticket = cardResKVP.Value;
          CardDetail cardDetail = cardResKVP.Key;
          if (ticket == null)
          {
            if (cardDetail.Frequency.Equals(otherMux))
            {
              ticket = CardReservationHelper.RequestCardReservation(user, cardDetail, cardResImpl, idChannel);
              tickets[cardDetail] = ticket;
              break;
            } 
          }          
        }        
      }
    }

    private ICollection<CardDetail> UpdateCardsIteratedBasedOnForceCardId(IUser user, bool forceCardId, IEnumerable<CardDetail> freeCardsForReservation)
    {
      ICollection<CardDetail> freeCardsIterated = new HashSet<CardDetail>();
      if (forceCardId)
      {
        foreach (CardDetail cardDetail in freeCardsForReservation)
        {
          if (cardDetail.Id != user.CardId)
          {
            freeCardsIterated.Add(cardDetail);
          }
        }
      }
      return freeCardsIterated;
    }

    private bool IterateTicketsUntilTimeshifting(ref IUser userBefore, Channel channel, IDictionary<CardDetail,
        ICardTuneReservationTicket> tickets, CardReservationTimeshifting cardResImpl, IList<CardDetail> freeCards, 
        int maxCards, int? kickCardId, out Dictionary<int, List<IUser>> kickableCards, ref IVirtualCard card, 
        ref TvResult result, ref int cardsIterated, ref double? parkedDuration)
    {      
      kickableCards = null;
      int failedCardId = -1;
      bool moreCardsAvailable = true;
      this.LogDebug("Controller: try max {0} of {1} cards for timeshifting", maxCards, freeCards.Count);
      //keep tuning each card until we are succesful                
      int cardIteration = 0;
      foreach (CardDetail cardInfo in freeCards)
      {
        if (!moreCardsAvailable)
        {
          break;
        }
        var newCardId = cardInfo.Id;
        IUser userNow = UserFactory.CreateBasicUser(userBefore.Name, newCardId, userBefore.Priority, userBefore.UserType);
        SetupTimeShiftingFolders(cardInfo);
        ITvCardHandler tvcard = _cards[newCardId];
        try
        {
          ICardTuneReservationTicket ticket = GetTicketByCardDetail(cardInfo, tickets);
          if (ticket == null)
          {            
            ticket = CardReservationHelper.RequestCardReservation(userBefore, cardInfo, cardResImpl, channel.IdChannel);
            if (ticket == null)
            {
             this.LogDebug("Controller: StartTimeShifting - could not find cardreservation on card:{0}",
                      userNow.CardId);
             HandleAllCardsBusy(tickets, out result, cardInfo);
              failedCardId = cardInfo.Id;
              continue; 
            }        
            else
            {
              tickets[cardInfo] = ticket;
            }
          }
          cardsIterated++;
          bool isTimeshifting = ticket.IsAnySubChannelTimeshifting;
          bool existingOwnerFoundOnSameChannel = false;
          if (isTimeshifting)
          {
            RemoveInactiveUsers(ticket);

            if (ticket.IsSameTransponder)
            {
              existingOwnerFoundOnSameChannel = ExistingOwnerFoundOnSameChannel(ticket);
              if (existingOwnerFoundOnSameChannel)
              {
                this.LogDebug("Controller: leech user={0} inherits subch={1}", userBefore.Name, ticket.OwnerSubchannel.Id);                
                userNow.CardId = ticket.CardId;
                ITvCardHandler tvCardHandler = _cards[ticket.CardId];

                //we can't have a user both park the channel as well as watch it, since the subchannel needs sharing, which cant be done.
                if (ticket.OwnerSubchannel.TvUsage == TvUsage.Parked)
                {
                  //todo: ideally, ask the client if its ok to either : 1) unpark channel or .. 2) cancel park and watch live
                  DateTime parkedAt;
                  double parkedDurationFound;
                  bool hasParkedUser = tvcard.ParkedUserManagement.IsUserParkedOnChannel(userNow.Name,
                                                                    ticket.OwnerSubchannel.
                                                                      IdChannel, out parkedDurationFound,
                                                                    out parkedAt);
                  if (hasParkedUser)
                  {
                    parkedDuration = parkedDurationFound;
                    result = TvResult.AlreadyParked;
                    break;
                  }                
                  /*
                  tvCardHandler.ParkedUserManagement.CancelParkedUserBySubChannelId(userBefore.Name, ticket.OwnerSubchannel.Id);
                  ISubChannel subch = tvCardHandler.UserManagement.GetSubChannel(userBefore.Name, ticket.OwnerSubchannel.Id);
                  if (subch != null)
                  {
                    subch.TvUsage = TvUsage.Timeshifting;                    
                  }*/
                }
                
                tvCardHandler.UserManagement.AddSubChannelOrUser(userNow, ticket.OwnerSubchannel.IdChannel, ticket.OwnerSubchannel.Id); 
              }
            }
            else
            {              
              if (!TransponderAcquired(maxCards, ticket, tvcard, cardIteration, channel.IdChannel, kickCardId, ref kickableCards))
              {
                if (kickableCards != null && kickableCards.Count > 0)
                {
                  result = TvResult.UsersBlocking;                 
                  HandleTvException(tickets, cardInfo);                                     
                  failedCardId = cardInfo.Id;
                }
                else
                {
                  HandleAllCardsBusy(tickets, out result, cardInfo); 
                }                
                continue;
              }              
            }            
          }

          if (!existingOwnerFoundOnSameChannel)
          {
            //tune to the new channel                  
            IChannel tuneChannel = cardInfo.TuningDetail;
            result = CardTune(ref userNow, tuneChannel, channel, ticket, cardResImpl);
            if (!HasTvSucceeded(result))
            {              
              HandleTvException(tickets, cardInfo);                                             
              failedCardId = cardInfo.Id;
              StopTimeShifting(ref userNow, channel.IdChannel);
              continue; //try next card            
            }
          }

          //reset failedCardId incase previous card iteration failed.
          failedCardId = -1;
          CardReservationHelper.CancelAllCardReservations(tickets);
          this.LogInfo("control2:{0} {1} {2}", userNow.Name, userNow.CardId, tvcard.UserManagement.GetSubChannelIdByChannelId(userNow.Name, channel.IdChannel));
          card = GetVirtualCard(userNow);
          card.NrOfOtherUsersTimeshiftingOnCard = ticket.NumberOfOtherUsersOnSameChannel;

          StopTimeShiftingAllChannelsExcept(userNow, channel.IdChannel);
          UpdateChannelStatesForUsers();
        }
        catch (Exception)
        {
          CardReservationHelper.CancelCardReservationAndRemoveTicket(cardInfo, tickets);
          if ((cardIteration + 1) < maxCards)
          {            
            HandleTvException(tickets, cardInfo);                                          
            failedCardId = cardInfo.Id;
            continue;
          }
          throw;
        }
        finally
        {
          cardIteration++;

          if (failedCardId > 0)
          {
            userBefore.FailedCardId = failedCardId;
          }
          if (!HasTvSucceeded(result))
          {
            moreCardsAvailable = AreMoreCardsAvailable(cardsIterated, maxCards, cardIteration);
            this.LogDebug(moreCardsAvailable
                        ? "Controller: Timeshifting failed, lets try next available card."
                        : "Controller: Timeshifting failed, no more cards available.");            
          }
          else
          {
            kickableCards = null;            
          }
        }
        break; //if we made it to the bottom, then we have a successful timeshifting.          
      } //end of foreach            
      return moreCardsAvailable;
    }

    private static void HandleTvException(IDictionary<CardDetail, ICardTuneReservationTicket> tickets, CardDetail cardInfo)
    {
      CardReservationHelper.CancelCardReservationAndRemoveTicket(cardInfo, tickets);      
    }
   
    private static void HandleAllCardsBusy(IDictionary<CardDetail, ICardTuneReservationTicket> tickets, out TvResult result, CardDetail cardInfo)
    {
      HandleTvException(tickets, cardInfo);      
      result = TvResult.AllCardsBusy;
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

    private bool AreMoreCardsAvailable(int cardsIterated, int maxCards, int i)
    {
      return (i < maxCards) && (_maxFreeCardsToTry == 0 || _maxFreeCardsToTry > cardsIterated);
    }

    private static ICardTuneReservationTicket GetTicketByCardDetail(CardDetail cardInfo, IDictionary<CardDetail, ICardTuneReservationTicket> tickets)
    {      
      ICardTuneReservationTicket ticket;
      tickets.TryGetValue(cardInfo, out ticket);
      return ticket;
    }

    private bool TransponderAcquired(int maxCards, ICardTuneReservationTicket ticket, ITvCardHandler tvcard, int cardIteration, int idChannel, int? kickCardId, ref Dictionary<int, List<IUser>> kickableCards)
    {      
      bool isTransponderAvailable = false;
      bool foundAnyUsersOnCard = FoundAnyUsersOnCard(ticket);
      if (foundAnyUsersOnCard)
      {
        bool foundCandidateForKicking = FoundCandidateForKicking(ticket);
        if (foundCandidateForKicking)
        {
          if (!kickCardId.HasValue || (kickCardId.Value == tvcard.DataBaseCard.IdCard))
          {
            bool usersKicked = KickLeechingUsersIfNoMoreCardsAvail(tvcard, ticket, cardIteration, maxCards, idChannel);
            bool cardsAvailable = ((cardIteration + 1) < maxCards);       
            if (!usersKicked && cardsAvailable)
            {
              this.LogDebug(
                "Controller: skipping card:{0} since other users are present on the same channel and there are still cards available.",
                tvcard.DataBaseCard.IdCard);
              //TODO: what if the following cards fail, should we then try and kick the leech user, in order to make room for a tune ?            
            }
            else
            {
              isTransponderAvailable = true;
            }
          }
          else if ((cardIteration + 1) == maxCards)
          {
            // kicking not allowed on card, lets report back to client - but only if this is the last card to choose from            
            if (kickableCards == null)
            {
              kickableCards = new Dictionary<int, List<IUser>>();
            }
            kickableCards[tvcard.DataBaseCard.IdCard] = ticket.ActiveUsers.ToList();
            this.LogDebug("Controller: not allowed to kick users on card:{0}, politely asking client...",
            tvcard.DataBaseCard.IdCard);
          }
          else
          {
            isTransponderAvailable = true;
          }
        }
        else
        {
          this.LogDebug(
            "Controller: skipping card:{0} since it is busy (user(s) present with higher priority).",
            tvcard.DataBaseCard.IdCard);
        }
      }
      else
      {
        isTransponderAvailable = true;
      }      
      return isTransponderAvailable;
    }


    private static void UpdateFreeCardsIterated(ICollection<CardDetail> freeCardsIterated, IEnumerable<CardDetail> freeCards)
    {
      foreach (CardDetail card in freeCards)
      {
        UpdateFreeCardsIterated(freeCardsIterated, card);        
      }
    }

    private static void UpdateFreeCardsIterated(ICollection<CardDetail> freeCardsIterated, CardDetail card)
    {
      if (!freeCardsIterated.Contains(card))
      {
        freeCardsIterated.Add(card);
      }      
    }

    private static bool HasFreeCards<T>(ICollection<T> freeCards)
    {
      bool hasFreeCards = (freeCards.Count > 0);
      return hasFreeCards;
    }

    private static bool HasTickets(ICollection<ICardTuneReservationTicket> tickets)
    {
      bool hasTickets = (tickets.Count > 0);
      return hasTickets;
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
      return (ticket.OwnerSubchannel != null);
    }

    private bool KickLeechingUsersIfNoMoreCardsAvail(ITvCardHandler tvcard, ICardTuneReservationTicket ticket, int cardIteration, int maxCards, int idChannel)
    {
      bool kickLeechingUsersIfNoMoreCardsAvail = false;

      if ((cardIteration + 1) == maxCards) // only kick users if we have no more cards to choose from
      {        
        IDictionary<string, List<int>> kickChannelsList = new Dictionary<string, List<int>>();
        IUser user = ticket.User;
        GetUser(tvcard, ref user);
        for (int j = ticket.ActiveUsers.Count - 1; j > -1; j--)
        {
          IUser activeUser = ticket.ActiveUsers[j];

          foreach (ISubChannel subchannel in activeUser.SubChannels.Values)
          {
            string channelInfo = Convert.ToString(subchannel.IdChannel);
            if (subchannel.IdChannel > 0)
            {
              Channel ch = ChannelManagement.GetChannel(idChannel, ChannelIncludeRelationEnum.TuningDetails);
              if (ch != null)
              {
                channelInfo = ch.DisplayName;
              }

              this.LogDebug(
                "Controller: kicking leech user '{0}' with prio={1} off card={2} on channel={3} (subchannel #{4}) since owner '{5}' with prio={6} (subchannel #{7}) changed transponder and there are no more cards available",
                activeUser.Name,
                activeUser.Priority,
                tvcard.DataBaseCard.Name,
                channelInfo,
                subchannel.Id,
                user.Name,
                user.Priority,
                tvcard.UserManagement.GetSubChannelIdByChannelId(user.Name, idChannel));


              List<int> idChannelList;
              bool hasUser = kickChannelsList.TryGetValue(activeUser.Name, out idChannelList);
              if (!hasUser)
              {
                idChannelList = new List<int>();  
              }
              idChannelList.Add(subchannel.IdChannel);
              kickChannelsList[activeUser.Name] = idChannelList;                                          
              kickLeechingUsersIfNoMoreCardsAvail = true;
            }
          }          
        }

        //done in order to avoid threading issues.
        foreach (KeyValuePair<string, List<int>> userKvp in kickChannelsList)
        {
          string userName = userKvp.Key;
          List<int> channelIdList = userKvp.Value;
          foreach (int channelId in channelIdList)
          {
            IUser activeUser = tvcard.UserManagement.GetUserCopy(userName);
            if (userName.Equals(ticket.User.Name))
            {
              //a user is now able to kick its own parkedchannel sessions in order to make room for a new tuning, but we dont want any kick notifications on the client
              StopTimeShifting(ref activeUser, channelId);  
            } 
            else
            {
              StopTimeShifting(ref activeUser, TvStoppedReason.OwnerChangedTS, channelId);  
            }
          }
        }        
      }

      

      return kickLeechingUsersIfNoMoreCardsAvail;
    }

    private static void GetUser(ITvCardHandler tvcard, ref IUser user)
    {
      if (user != null)
      {        
        tvcard.UserManagement.RefreshUser(ref user);        
      }
    }

    private void RemoveInactiveUsers(ICardTuneReservationTicket ticket)
    {
      for (int i = 0; i < ticket.InactiveUsers.Count; i++)
      {
        IUser inactiveUser = ticket.InactiveUsers[i];
        foreach (var subchannel in inactiveUser.SubChannels.Values)
        {
          this.LogDebug("controller: RemoveInactiveUsers {0}", inactiveUser.Name);
          StopTimeShifting(ref inactiveUser, subchannel.IdChannel);
          //removing inactive user which shouldnt happen, but atleast its better than having timeshfiting fail. 
        }        
      }
    }

    private static bool HasCardChanged(IVirtualCard card, string intialTimeshiftingFilename)
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
        this.LogDebug("Controller: epg stop");
        _epgGrabber.Stop();
      }
    }

    private void StartEPGgrabber()
    {
      if (_epgGrabber != null && AllCardsIdle)
      {
        this.LogDebug("Controller: epg start");
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
        cardInfo.Card.RecordingFolder = Path.Combine(PathManager.GetDataPath, "recordings");
        if (!Directory.Exists(cardInfo.Card.RecordingFolder))
        {
          this.LogDebug("Controller: creating recording folder {0} for card {0}", cardInfo.Card.RecordingFolder,
                    cardInfo.Card.Name);
          Directory.CreateDirectory(cardInfo.Card.RecordingFolder);
        }
      }
      if (cardInfo.Card.TimeshiftingFolder == String.Empty)
      {
        cardInfo.Card.TimeshiftingFolder = Path.Combine(PathManager.GetDataPath, "timeshiftbuffer");
        if (!Directory.Exists(cardInfo.Card.TimeshiftingFolder))
        {
          this.LogDebug("Controller: creating timeshifting folder {0} for card {0}", cardInfo.Card.TimeshiftingFolder,
                    cardInfo.Card.Name);
          Directory.CreateDirectory(cardInfo.Card.TimeshiftingFolder);
        }
      }
    }

    // userPriority mainly used for setupTV stress test, as it has the ability to customize user priorities during testing
    public TvResult StartTimeShifting(string userName, int userPriority, int idChannel, out IVirtualCard card, out IUser user)
    {
      card = null;
      user = null;
      TvResult result = TvResult.UnknownError;
      try
      {
        bool cardChanged;
        Dictionary<int, List<IUser>> kickableCards;
        double? parkedDuration;

        user = GetUserFromContext(userName, TvUsage.Timeshifting);
        if (user == null)
        {
          user = UserFactory.CreateBasicUser(userName, userPriority);
        }
        result = StartTimeShifting(ref user, idChannel, null, out card, out kickableCards, false, out cardChanged, out parkedDuration);
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return result;
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <param name="user">user credentials.</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(string userName, int idChannel, out IVirtualCard card, out IUser user)
    {
      card = null;
      user = null;
      TvResult result = TvResult.UnknownError;
      try
      {
        bool cardChanged;
        Dictionary<int, List<IUser>> kickableCards;
        double? parkedDuration;

        user = GetUserFromContext(userName, TvUsage.Timeshifting);
        if (user == null)
        {
          user = CreateTimeshiftingUserWithPriority(userName);
        }
        result = StartTimeShifting(ref user, idChannel, null, out card, out kickableCards, false, out cardChanged, out parkedDuration);        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
      return result;
    }

    private static IUser CreateTimeshiftingUserWithPriority(string userName)
    {
      IUser user = UserFactory.CreateBasicUser(userName, UserFactory.GetDefaultPriority(userName));
      return user;
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="kickCardId"> </param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <param name="kickableCards"> </param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <param name="parkedDuration"> </param>
    /// <param name="user">user credentials.</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(string userName, int idChannel, int? kickCardId, out IVirtualCard card, out Dictionary<int, List<IUser>> kickableCards, out bool cardChanged, out double? parkedDuration, out IUser user)
    {
      TvResult result = TvResult.UnknownError;
      card = null;
      kickableCards = null;
      cardChanged = false;
      parkedDuration = null;
      user = null;
      try
      {
        user = GetUserFromContext(userName, TvUsage.Timeshifting);
        if (user == null)
        {
          user = CreateTimeshiftingUserWithPriority(userName);
        }
        result = StartTimeShifting(ref user, idChannel, kickCardId, out card, out kickableCards, false, out cardChanged, out parkedDuration);        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
      return result;
    }

    /// <summary>
    /// Checks if the schedule specified is currently being recorded and ifso
    /// returns on which card
    /// </summary>
    /// <param name="idSchedule">id of the Schedule</param>
    /// <param name="card">returns the card which is recording the channel</param>
    /// <returns>true if a card is recording the schedule, otherwise false</returns>
    public bool IsRecordingSchedule(int idSchedule, out IVirtualCard card)
    {
      card = null;
      try
      {
        if (!_scheduler.IsRecordingSchedule(idSchedule, out card))
        {
          this.LogInfo("IsRecordingSchedule: scheduler is not recording schedule");
          return false;
        }
        this.LogInfo("IsRecordingSchedule: scheduler is recording schedule on cardid:{0}", card.Id);

        return true;
      }
      catch (Exception e)
      {
        HandleControllerException(e);
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
        _scheduler.StopRecordingSchedule(idSchedule);
      }
      catch (Exception e)
      {
        HandleControllerException(e);
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
        //Gentle.Common.CacheManager.ClearQueryResultsByType(typeof (Schedule));
        if (_scheduler != null)
        {
          _scheduler.ResetTimer();
        }
        FireScheduleAddedEvent();

      }
      catch (Exception e)
      {
        HandleControllerException(e);        
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
        //Gentle.Common.CacheManager.ClearQueryResultsByType(typeof (Schedule));
        if (_scheduler != null)
        {
          _scheduler.ResetTimer();
        }
        FireScheduleAddedEvent((TvServerEventArgs)args);        
      }
      catch (Exception ex)
      {
        this.LogError(ex);
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
        this.LogError(ex);
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
        catch (Exception e)
        {
          HandleControllerException(e);
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
              if (SettingsManagement.GetValue("idleEPGGrabberEnabled", true))
              {
                StartEPGgrabber();
              }
            }
          }
          else
          {
            if (_epgGrabber != null)
            {
              if (SettingsManagement.GetValue("idleEPGGrabberEnabled", true))
              {
                StopEPGgrabber();
              }
            }
          }
        }
        catch (Exception e)
        {
          HandleControllerException(e);
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
      catch (Exception e)
      {
        HandleControllerException(e);
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
        this.LogError(ex);
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
      satellitePosition = -1;
      stepsAzimuth = -1;
      stepsElevation = -1;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.GetPosition(out satellitePosition, out stepsAzimuth, out stepsElevation);
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
    }

    public void DiSEqCReset(int cardId)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.Reset();
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }

    }

    public void DiSEqCStopMotor(int cardId)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.StopMotor(); 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      
    }

    public void DiSEqCSetEastLimit(int cardId)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.SetEastLimit();
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }

    }

    public void DiSEqCSetWestLimit(int cardId)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.SetWestLimit();
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public void DiSEqCForceLimit(int cardId, bool onOff)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.EnableEastWestLimits(onOff);
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public void DiSEqCDriveMotor(int cardId, DiseqcDirection direction, byte numberOfSteps)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.DriveMotor(direction, numberOfSteps); 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public void DiSEqCStorePosition(int cardId, byte position)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.StoreCurrentPosition(position); 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public void DiSEqCGotoReferencePosition(int cardId)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.GotoReferencePosition();
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public void DiSEqCGotoPosition(int cardId, byte position)
    {
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          _cards[cardId].DisEqC.GotoStoredPosition(position);
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      
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
        RefreshUserFromSpecificContext(ref user);
        _cards[user.CardId].Epg.Stop(user);
        FireEpgGrabbingStoppedEvent(user);        
      }
    }

    public IEnumerable<string> ServerIpAdresses
    {
      get
      {
        List<string> ipadresses = new List<string>();
        try
        {          
          string localHostName = Dns.GetHostName();
          IPHostEntry local = Dns.GetHostEntry(localHostName);
          foreach (IPAddress ipaddress in local.AddressList)
          {
            if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
            {
              ipadresses.Add(ipaddress.ToString());
            }
          }          
        }
        catch (Exception e)
        {
          HandleControllerException(e);
        }
        return ipadresses;
      }
    }



    public IDictionary<string, IUser> GetUsersForCard(int cardId)
    {
      IDictionary<string, IUser> usersForCard = null;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          usersForCard = _cards[cardId].UserManagement.UsersCopy;
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return usersForCard;
    }


    /// <summary>
    /// Returns a dictionary of channels that are timeshfiting and recording.
    /// </summary>
    public IDictionary<int, ChannelState> GetAllTimeshiftingAndRecordingChannels()
    {
      IDictionary<int, ChannelState> result = new Dictionary<int, ChannelState>();
      Dictionary<int, ITvCardHandler>.ValueCollection cardHandlers = _cards.Values;


      foreach (ITvCardHandler tvcard in cardHandlers)
      {
        IDictionary<int, ChannelState> channelIds = tvcard.UserManagement.GetAllTimeShiftingAndRecordingChannelIds();

        foreach (KeyValuePair<int, ChannelState> idKvp in channelIds)
        {
          int idChannel = idKvp.Key;
          ChannelState state = idKvp.Value;

          if (state == ChannelState.recording)
          {            
            result[idChannel] = ChannelState.recording;
          }
          else if (state == ChannelState.timeshifting)
          {
            if (!result.ContainsKey(idChannel))
            {
              result.Add(idChannel, ChannelState.timeshifting);
            }
          }          
        }        
      }
      return result;
    }

   

    public IDictionary<int, ChannelState> GetAllChannelStatesForIdleUserCached()
    {
      return _channelStatesCachedForIdleUser;
    }

    /// <summary>
    /// Fetches all channel states for a specific user (cached - faster)
    /// </summary>
    /// <param name="userName"> </param>
    public IDictionary<int, ChannelState> GetAllChannelStatesCached(string userName)
    {
      IDictionary<int, ChannelState> allChannelStatesCached = null;
      try
      {        
        if (userName != null && _cards.Any())
        {
          IUser userFound = GetUserFromContext(userName);
          if (userFound != null)
          {
            allChannelStatesCached = userFound.ChannelStates;
          }
        }

        if (allChannelStatesCached == null)
        {
          allChannelStatesCached = _channelStatesCachedForIdleUser;
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return allChannelStatesCached;
    }


    /// <summary>
    /// Checks if a channel is tunable/tuned or not...
    /// </summary>
    /// <param name="idChannel">Channel id</param>
    /// <param name="userName"> </param>
    /// <returns>
    ///       <c>channel state tunable|nottunable</c>.
    /// </returns>
    public ChannelState GetChannelState(int idChannel, string userName)
    {
      ChannelState chanState = ChannelState.nottunable;
      try
      {        
        if (!string.IsNullOrWhiteSpace(userName))
        {
          IDictionary<int, ChannelState> channelStates = GetAllChannelStatesCached(userName);

          if (channelStates != null)
          {
            if (!channelStates.TryGetValue(idChannel, out chanState))
            {
              chanState = ChannelState.tunable;
            }
          }
          else
          {
            Channel dbchannel = ChannelManagement.GetChannel(idChannel, ChannelIncludeRelationEnum.TuningDetails); 
            TvResult viewResult;

            IUser user = GetUserFromContext(userName);
            if (user != null)
            {
              _cardAllocation.GetFreeCardsForChannel(_cards, dbchannel, user, out viewResult);
              chanState = viewResult == TvResult.Succeeded ? ChannelState.tunable : ChannelState.nottunable;
            }
          }
        }
        else
        {
          chanState = ChannelState.nottunable;
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return chanState;
    }

    #endregion

    #region streaming

    public int StreamingPort
    {
      get
      {
        try
        {
          if (_streamer != null)
          {
            return _streamer.Port;
          }          
        }
        catch (Exception e)
        {
          HandleControllerException(e);
        }
        return 0;
      }
    }

    public List<RtspClient> StreamingClients
    {
      get
      {
        try
        {
          if (_streamer == null)
            return new List<RtspClient>();
          return _streamer.Clients;
        }
        catch (Exception e)
        {
          HandleControllerException(e);
        }
        return new List<RtspClient>();
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
      bool result = false;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          result = _cards[cardId].Card.SupportsQualityControl;
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);        
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
      bool supportsBitRateModes = false;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          IQuality qualityControl = _cards[cardId].Card.Quality;
          if (qualityControl != null)
          {
            supportsBitRateModes = qualityControl.SupportsBitRateModes();
          } 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
      return supportsBitRateModes;
    }

    /// <summary>
    /// Indicates if peak bit rate mode is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsPeakBitRateMode(int cardId)
    {
      bool supportsPeakBitRateMode = false;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          IQuality qualityControl = _cards[cardId].Card.Quality;
          if (qualityControl != null)
          {
            supportsPeakBitRateMode = qualityControl.SupportsPeakBitRateMode();
          } 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      
      return supportsPeakBitRateMode;
    }


    /// <summary>
    /// Indicates if bit rate control is supported
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>true/false</returns>
    public bool SupportsBitRate(int cardId)
    {
      bool supportsBitRate = false;
      try
      {
        if (ValidateTvControllerParams(cardId))
        {
          IQuality qualityControl = _cards[cardId].Card.Quality;
          if (qualityControl != null)
          {
            supportsBitRate = qualityControl.SupportsBitRate();
          } 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }

      
      return supportsBitRate;
    }

    /// <summary>
    /// Reloads the configuration of quality control for the given card
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    public void ReloadCardConfiguration(int cardId)
    {
      try
      {
        if (ValidateTvControllerParams(cardId) && SupportsQualityControl(cardId))
        {
          _cards[cardId].Card.ReloadCardConfiguration(); 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      
    }

    /// <summary>
    /// Gets the current quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    public QualityType GetQualityType(int cardId)
    {
      QualityType qualityType = QualityType.Default;
      try
      {
        if (ValidateTvControllerParams(cardId) && SupportsQualityControl(cardId))
        {
          IQuality qualityControl = _cards[cardId].Card.Quality;
          if (qualityControl != null)
          {
            qualityType = qualityControl.QualityType;
          }
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      
      return qualityType;
    }

    /// <summary>
    /// Sets the quality type
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="qualityType">The new quality type</param>
    public void SetQualityType(int cardId, QualityType qualityType)
    {
      try
      {
        if (ValidateTvControllerParams(cardId) && SupportsQualityControl(cardId))
        {
          IQuality qualityControl = _cards[cardId].Card.Quality;
          if (qualityControl != null)
          {
            qualityControl.QualityType = qualityType;
          }          
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    /// <summary>
    /// Gets the current bitrate mdoe
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <returns>QualityType</returns>
    public VIDEOENCODER_BITRATE_MODE GetBitRateMode(int cardId)
    {
      VIDEOENCODER_BITRATE_MODE videoencoderBitrateMode = VIDEOENCODER_BITRATE_MODE.Undefined;
      try
      {
        if (ValidateTvControllerParams(cardId) && SupportsQualityControl(cardId))
        {
          IQuality qualityControl = _cards[cardId].Card.Quality;
          if (qualityControl != null)
          {
            videoencoderBitrateMode = qualityControl.BitRateMode;
          } 
        }                  
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      
      return videoencoderBitrateMode;
    }

    /// <summary>
    /// Sets the bitrate mode
    /// </summary>
    /// <param name="cardId">Unique id of the card</param>
    /// <param name="bitRateMode">The new bitrate mdoe</param>
    public void SetBitRateMode(int cardId, VIDEOENCODER_BITRATE_MODE bitRateMode)
    {
      try
      {
        if (ValidateTvControllerParams(cardId) && SupportsQualityControl(cardId))
        {
          IQuality qualityControl = _cards[cardId].Card.Quality;
          if (qualityControl != null)
          {
            qualityControl.BitRateMode = bitRateMode;
          } 
        }        
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    #endregion

    #endregion

    #region private members

    private IDictionary<int, ChannelState> _channelStatesCachedForIdleUser = new Dictionary<int, ChannelState>();

    private void UpdateChannelStatesForUsers()
    {
      //System.Diagnostics.Debugger.Launch();
      // this section makes sure that all users are updated in regards to channel states.            
      
      IList<ChannelGroup> groups = ChannelGroupManagement.ListAllChannelGroups(ChannelGroupIncludeRelationEnum.None);

      // populating _tvChannelListGroups is only done once as is therefor cached.
      if (_tvChannelListGroups == null)
      {
        foreach (ChannelGroup group in groups)
        {
          // we will only update user created groups, since it will often have fewer channels than "all channels"
          // going into "all channels" group in mini EPG will always be slower.
          if (group.GroupName.Equals(TvConstants.TvGroupNames.AllChannels))
          {
            continue;
          }

          if (_tvChannelListGroups == null)
          {
            _tvChannelListGroups =
              ChannelManagement.GetAllChannelsByGroupIdAndMediaType(group.IdGroup, MediaTypeEnum.TV).ToList();
          }
          else
          {
            IList<Channel> tvChannelList = ChannelManagement.GetAllChannelsByGroupIdAndMediaType(group.IdGroup,
                                                                                                 MediaTypeEnum.TV);
            foreach (Channel ch in tvChannelList)
            {
              bool exists = _tvChannelListGroups.Exists(c => c.IdChannel == ch.IdChannel);
              if (!exists)
              {
                _tvChannelListGroups.Add(ch);
              }
            }
          }
        }
      }
            
      _channelStates.SetChannelStatesForAllUsers(_tvChannelListGroups);

      IUser idleUser = new User("idle", UserType.Normal, 0);
      idleUser.ChannelStates = new Dictionary<int, ChannelState>();

      ThreadPool.QueueUserWorkItem(delegate { _channelStates.SetChannelStatesForUser(_tvChannelListGroups, ref idleUser); });

    }

    private void channelStates_OnChannelStatesSet(IUser user)
    {
      _channelStatesCachedForIdleUser = user.ChannelStates;
      FireChannelStatesEvent(user);      
    }

    


    /// <summary>
    /// Determines whether the the user is the owner of the card
    /// </summary>
    /// <param name="cardId">The card id.</param>
    /// <param name="userName"> </param>
    /// <returns>
    /// 	<c>true</c> if the specified user is the card owner; otherwise, <c>false</c>.
    /// </returns>
    public bool IsOwner(int cardId, string userName)
    {
      bool isOwner = false;
      try
      {
        if (ValidateTvControllerParams(userName) && ValidateTvControllerParams(cardId))
        {
          isOwner = _cards[cardId].UserManagement.IsOwner(userName);
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return isOwner;
    }


    //string userName, int cardId, int channelId
    private void StopTimeShiftingAllChannelsExcept(IUser user, int idChannel)
    {
      var channelIdsRemoveList = new List<int>();
      foreach (ITvCardHandler cardHandler in CardCollection.Values)
      {
        IUser existingUser = cardHandler.UserManagement.GetUserCopy(user.Name);
        if (existingUser != null)
        {          
          foreach (ISubChannel subchannel in existingUser.SubChannels.Values)
          {
            if (subchannel.TvUsage == TvUsage.Timeshifting && subchannel.IdChannel != idChannel)
            {
              channelIdsRemoveList.Add(subchannel.IdChannel);
            }
          }
        }
      }

      foreach (int channelId2Remove in channelIdsRemoveList)
      {
        this.LogDebug("StopTimeShiftingAllChannelsExcept : {0} - {1} - {2}", user.Name, user.CardId, idChannel);
        StopTimeShifting(ref user, channelId2Remove); 
      }     
    }

    public bool SupportsSubChannels(int cardId)
    {
      bool supportsSubChannels = false;
      if (ValidateTvControllerParams(cardId))
      {
        supportsSubChannels = _cards[cardId].SupportsSubChannels; 
      }
      return supportsSubChannels;
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
      TvResult result = TvResult.CardIsDisabled;
      if (ValidateTvControllerParams(user))
      {
        try
        {         
          ITvCardHandler tvCardHandler = _cards[user.CardId];
          if (tvCardHandler.DataBaseCard.Enabled)
          {
            if (ticket.ConflictingSubchannelFound)
            {
              tvCardHandler.UserManagement.GetNextAvailableSubchannel(user.Name);
            }

            FireStartZapChannelEvent(user, channel);            

            tvCardHandler.Tuner.OnAfterTuneEvent -= Tuner_OnAfterTuneEvent;
            tvCardHandler.Tuner.OnBeforeTuneEvent -= Tuner_OnBeforeTuneEvent;

            tvCardHandler.Tuner.OnAfterTuneEvent += Tuner_OnAfterTuneEvent;
            tvCardHandler.Tuner.OnBeforeTuneEvent += Tuner_OnBeforeTuneEvent;

            cardResTS.OnStartCardTune += CardResTsOnStartCardTune;
            result = cardResTS.CardTune(tvCardHandler, ref user, channel, dbChannel, ticket);
            cardResTS.OnStartCardTune -= CardResTsOnStartCardTune;

            this.LogInfo("Controller: {0} {1} {2}", user.Name, user.CardId,
                     tvCardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, dbChannel.IdChannel));
          }
        }
        finally
        {
          FireEndZapChannelEvent(user, channel);          
        }
      }
      return result;
    }

    TvResult CardResTsOnStartCardTune(ref IUser user, ref string fileName, int idChannel)
    {
      return StartTimeShifting(ref user, ref fileName, idChannel);
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
      bool isTunedToTransponder = false;
      if (ValidateTvControllerParams(cardId))
      {
        isTunedToTransponder = _cards[cardId].Tuner.IsTunedToTransponder(transponder);
      }
      
      return isTunedToTransponder;
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
        this.LogError(ex);
      }
    }

    /// <summary>
    /// returns a virtual card for the card specified.
    /// </summary>
    /// <param name="user">User</param>
    /// <returns></returns>
    private VirtualCard GetVirtualCard(IUser user)
    {
      VirtualCard card = null;
      if (ValidateTvControllerParams(user))
      {
        RefreshUserFromSpecificContext(ref user);
        card = new VirtualCard(user)
        {
          RecordingFolder = _cards[user.CardId].DataBaseCard.RecordingFolder,
          TimeshiftFolder = _cards[user.CardId].DataBaseCard.TimeshiftingFolder,
          RemoteServer = Dns.GetHostName()
        }; 
      }      
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
        //this.LogDebug("TVController.CanSuspend: checking cards");
        if (_cards.Values.Where(card => card != null).Any(card => card.UserManagement.IsAnyUserTimeShiftingOrRecording()))
        {
          return false;
        }        

        // check whether the scheduler would like to record something now, but there is no card recording
        // this can happen if a recording is due, but the scheduler has not yet picked up recording (latency)
        if (_scheduler != null && _scheduler.IsTimeToRecord(DateTime.Now))
        {          
          return false;
        }
        return true;
      }
    }

    #region private methods

    private bool ValidateTvControllerParams(string userName)
    {
      if (string.IsNullOrWhiteSpace(userName))
      {
#if DEBUG
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);
        this.LogError(
          "TVController:" + sf.GetMethod().Name +
          " - incorrect parameters used! username {0}", userName);
        this.LogError("{0}", st);
#endif
        return false;
      }
      return true;
    }

    private bool ValidateTvControllerParams(int cardId, bool checkCardPresent = true)
    {
      if (cardId < 0 || !_cards.ContainsKey(cardId) || (checkCardPresent && !IsCardPresent(cardId)))
      {
#if DEBUG
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);
        this.LogError(
          "TVController:" + sf.GetMethod().Name +
          " - incorrect parameters used! cardId {0} _cards.ContainsKey(cardId) == {1} CardPresent {2}", cardId,
          _cards.ContainsKey(cardId), IsCardPresent(cardId));
        this.LogError("{0}", st);
#endif
        return false;
      }
      return true;
    }

    private bool IsValidTvControllerParams(string userName, int cardId)
    {
      bool isValidTvControllerParams = (userName != null && cardId > 0 && _cards.ContainsKey(cardId) && IsCardPresent(cardId));
                             
      if (!isValidTvControllerParams)
      {
#if DEBUG
        var st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);

        if (cardId > 0)
        {
          this.LogError(
            "TVController:" + sf.GetMethod().Name +
            " - incorrect parameters used! user {0} cardId {1} _cards.ContainsKey(cardId) == {2} CardPresent(cardId) {3}",
            userName, cardId, _cards.ContainsKey(cardId), IsCardPresent(cardId));
        }
        else
        {
          this.LogError("TVController:" + sf.GetMethod().Name + " - incorrect parameters used! user NULL");
        }
        this.LogError("{0}", st);
#endif
      }
      return isValidTvControllerParams;
    }

    private bool ValidateTvControllerParams(IUser user)
    {      
      if (user == null || user.CardId < 0 || !_cards.ContainsKey(user.CardId) || (!IsCardPresent(user.CardId)))
      {
#if DEBUG
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);

        if (user != null)
        {
          this.LogError(
            "TVController:" + sf.GetMethod().Name +
            " - incorrect parameters used! user {0} cardId {1} _cards.ContainsKey(cardId) == {2} CardPresent(cardId) {3}",
            user, user.CardId, _cards.ContainsKey(user.CardId), IsCardPresent(user.CardId));
        }
        else
        {
          this.LogError("TVController:" + sf.GetMethod().Name + " - incorrect parameters used! user NULL");
        }
        this.LogError("{0}", st);
#endif
        return false;
      }
      return true;
    }

    private static bool ValidateTvControllerParams(IChannel channel)
    {
      if (channel == null)
      {
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);

        Log.Error("TVController:" + sf.GetMethod().Name + " - incorrect parameters used! channel NULL");
        Log.Error("{0}", st);
        return false;
      }
      return true;
    }

    #endregion

    #region ICiMenuCallbacks Member    

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
      return _ciMenuManager.OnCiMenu(lpszTitle, lpszSubTitle, lpszBottom, nNumChoices);      
    }

    /// <summary>
    /// [TsWriter Interface Callback] Sets the choices to opening dialog
    /// </summary>
    /// <param name="nChoice">number of choice (0 based)</param>
    /// <param name="lpszText">title of choice</param>
    /// <returns>0</returns>
    public int OnCiMenuChoice(int nChoice, string lpszText)
    {
      return _ciMenuManager.OnCiMenuChoice(nChoice, lpszText);      
    }

    /// <summary>
    /// [TsWriter Interface Callback] call to close display
    /// </summary>
    /// <param name="nDelay">delay in (ms?)</param>
    /// <returns>0</returns>
    public int OnCiCloseDisplay(int nDelay)
    {
      return _ciMenuManager.OnCiCloseDisplay(nDelay);      
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
      return _ciMenuManager.OnCiRequest(bBlind, nAnswerLength, lpszText);            
    }

    #endregion

    public void ExecutePendingDeletions()
    {
      ThreadPool.QueueUserWorkItem(delegate
                                     {
                                       try
                                       {
                                         // System.Diagnostics.Debugger.Launch();
                                         List<int> pendingDelitionRemove = new List<int>();
                                         IList<PendingDeletion> pendingDeletions = RecordingManagement.ListAllPendingRecordingDeletions();

                                         this.LogDebug("ExecutePendingDeletions: number of pending deletions : " +
                                                       Convert.ToString(pendingDeletions.Count));

                                         Parallel.ForEach(pendingDeletions, pendingDelition =>
                                                                              {
                                                                                this.LogDebug("ExecutePendingDeletions: trying to remove file : {0}", pendingDelition.FileName);
                                                                                bool wasPendingDeletionAdded;
                                                                                bool wasDeleted =
                                                                                  RecordingFileHandler.DeleteRecordingOnDisk(pendingDelition.FileName,
                                                                                                                             out wasPendingDeletionAdded);
                                                                                if (wasDeleted && !wasPendingDeletionAdded)
                                                                                {
                                                                                  pendingDelitionRemove.Add(pendingDelition.IdPendingDeletion);
                                                                                }
                                                                              });

                                         Parallel.ForEach(pendingDelitionRemove, id =>
                                                                                   {
                                                                                     PendingDeletion pendingDelition = RecordingManagement.GetPendingRecordingDeletion(id);
                                                                                     if (pendingDelition != null)
                                                                                     {
                                                                                       RecordingManagement.DeletePendingRecordingDeletion(pendingDelition.IdPendingDeletion);
                                                                                     }
                                                                                   });                                         
                                       }
                                       catch (Exception ex)
                                       {
                                         this.LogError(ex, "ExecutePendingDeletions exception");
                                       }
                                     });
    }



    public void OnResume()
    {
      if (!_onResumeDone)
      {
        this.LogInfo("TvController.OnResume()");
        StartHeartbeatManager();
        StartTvServerEventDispatcher();

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
      this.LogInfo("TvController.OnSuspend()");

      StopHeartbeatManager();
      StopTvserverEventDispatcher();
      if (_scheduler != null)
      {
        _scheduler.Stop();
      }

      IUser tmpUser = new User();
      foreach (ITvCardHandler cardhandler in CardCollection.Values)
      {
        cardhandler.ParkedUserManagement.CancelAllParkedUsers();
        cardhandler.StopCard();
      }
    }

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>
    /// <param name="userName"> </param>
    /// <param name="totalTSpackets">Amount of packets processed</param>
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>
    public void GetStreamQualityCounters(string userName, out int totalTSpackets, out int discontinuityCounter)
    {
      totalTSpackets = 0;
      discontinuityCounter = 0;
      try
      {
        IUser user = GetUserFromContext(userName, TvUsage.Timeshifting);
        if (user != null)
        {
          int cardId = user.CardId;
          ITvCardHandler cardHandler = _cards[cardId];
          cardHandler.TimeShifter.GetStreamQualityCounters(userName, out totalTSpackets, out discontinuityCounter);
        }                
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
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

      try
      {
        if (idCard > 0)
        {
          ITvCardHandler cardHandler = _cards[idCard];
          if (cardHandler.Card != null && cardHandler.Card.SubChannels != null)
          {
            subchannels = cardHandler.Card.SubChannels.Length;
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }

      return subchannels;
    }

    public void RegisterUserForHeartbeatMonitoring (string username)
    {
      try
      {
        _heartbeatManager.Register(username);
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public void UnRegisterUserForHeartbeatMonitoring(string username)
    {
      try
      {
        _heartbeatManager.UnRegister(username);
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public void RegisterUserForCiMenu(string username)
    {
      try
      {
        _ciMenuManager.Register(username);      
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }
    public void UnRegisterUserForCiMenu(string username)
    {
      try
      {
        _ciMenuManager.UnRegister(username);
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public void RegisterUserForTvServerEvents(string username)
    {
      try
      {
        _tvServerEventDispatcher.Register(username);
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }
    public void UnRegisterUserForTvServerEvents(string username)
    {
      try
      {
        _tvServerEventDispatcher.UnRegister(username);
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }      
    }

    public IDictionary<string, byte[]> GetPluginBinaries()
    {
      var fileStreams = new Dictionary<string, byte[]>();
      try
      {
        string pluginsFolder = PathManager.BuildAssemblyRelativePath("plugins");
        var dirInfo = new DirectoryInfo(pluginsFolder);

        FileInfo[] files = dirInfo.GetFiles("*.dll");

        foreach (FileInfo fileInfo in files)
        {
          using (var filestream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
          {
            long length = filestream.Length;
            var data = new byte[length];
            filestream.Read(data, 0, (int)length);
            fileStreams.Add(fileInfo.Name, data);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return fileStreams;
    }

    public IDictionary<string, byte[]> GetPluginBinariesCustomDevices()
    {
      var fileStreams = new Dictionary<string, byte[]>();
      try
      {
        string customDevicesFolder = PathManager.BuildAssemblyRelativePath("plugins\\CustomDevices");
        var dirInfoCustomDevices = new DirectoryInfo(customDevicesFolder);

        FileInfo[] filesCustomDevices = dirInfoCustomDevices.GetFiles("*.dll");

        foreach (FileInfo fileInfo in filesCustomDevices)
        {
          using (var filestream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
          {
            long length = filestream.Length;
            var data = new byte[length];
            filestream.Read(data, 0, (int)length);
            fileStreams.Add(fileInfo.Name, data);
          }
        }      
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return fileStreams;
    }

    public IDictionary<string, byte[]> GetPluginBinariesResources()
    {
      var fileStreams = new Dictionary<string, byte[]>();
      try
      {
        string resourcesFolder = PathManager.BuildAssemblyRelativePath("plugins\\CustomDevices\\Resources");
        var dirInfoResources = new DirectoryInfo(resourcesFolder);

        FileInfo[] filesResources = dirInfoResources.GetFiles("*.dll");

        foreach (FileInfo fileInfo in filesResources)
        {
          using (var filestream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
          {
            long length = filestream.Length;
            var data = new byte[length];
            filestream.Read(data, 0, (int)length);
            fileStreams.Add(fileInfo.Name, data);
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return fileStreams;
    }

    public IList<StreamPresentation> ListAllStreamingChannels()
    {
      IList<StreamPresentation> streams = new List<StreamPresentation>();
      try
      {        
        foreach (ITvCardHandler cardHandler in _cards.Values)
        {
          if (!cardHandler.DataBaseCard.Enabled)
          {
            continue;
          }
          int idCard = cardHandler.DataBaseCard.IdCard;
          if (!IsCardPresent(idCard))
          {
            continue;
          }

          IDictionary<string, IUser> usersCopy = cardHandler.UserManagement.UsersCopy;
          if (usersCopy != null)
          {
            IList<IUser> usersCopyList = usersCopy.Values.ToList();
            for (int i = 0; i < usersCopyList.Count; i++)
            {
              IUser user = usersCopyList[i];
              IVirtualCard vcard;
              bool isRecording = IsRecording(CurrentDbChannel(user.Name), out vcard);
              bool isTimeShifting = IsTimeShifting(user.Name);
              if (isTimeShifting || isRecording)
              {
                foreach (ISubChannel subchannel in user.SubChannels.Values)
                {
                  bool isParked = subchannel.TvUsage == TvUsage.Parked;
                  double parkedDuration = 0;
                  DateTime parkedAt = DateTime.MinValue;

                  if (isParked)
                  {
                    //lets get duration etc.
                    cardHandler.ParkedUserManagement.IsUserParkedOnChannel(user.Name, subchannel.IdChannel, out parkedDuration,
                                                                      out parkedAt);
                  }

                  var streamPresentation = new StreamPresentation(ChannelManagement.GetChannel(subchannel.IdChannel), user,
                                                                  isParked, isRecording, isTimeShifting, parkedDuration,
                                                                  parkedAt, IsScrambled(idCard, subchannel.Id));
                  streams.Add(streamPresentation);
                }
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }

      
      return streams;
    }

    public bool IsAnyCardParkedByUser(string userName)
    {
      bool isCardParkedByUser = false;
      try
      {          
        IEnumerable<Card> cards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None);
        foreach (Card card in cards)
        {
          if (!card.Enabled)
          {
            continue;
          }
          if (!IsCardPresent(card.IdCard))
          {
            continue;
          }
          IDictionary<string, IUser> users = GetUsersForCard(card.IdCard);
          if (users != null)
          {
            foreach (IUser user in users.Values.Where(user => card.IdCard == user.CardId))
            {
              int cardId = user.CardId;
              ITvCardHandler tvcard = _cards[cardId];
              isCardParkedByUser = tvcard.ParkedUserManagement.IsUserParkedOnAnyChannel(user.Name);
              if (isCardParkedByUser)
              {
                break;
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }

      return isCardParkedByUser;
    }

    public IList<CardPresentation> ListAllCards()
    {      
      IList<CardPresentation> cardPresentations = new List<CardPresentation>();

      try
      {
        IEnumerable<Card> cards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None);

        foreach (Card card in cards)
        {
          ITvCardHandler cardHandler;
          bool isAvailable = _cards.TryGetValue(card.IdCard, out cardHandler);
          bool isEnabled = false;

          if (isAvailable)
          {
            isEnabled = card.Enabled;
            isAvailable = IsCardPresent(card.IdCard);
          }

          if (!isAvailable)
          {
            var cardPresentation = new CardPresentation("n/a", card.IdCard, card.Name) { SubChannelsCountOk = true, SubChannels = 0, State = "n/a" };
            cardPresentations.Add(cardPresentation);
          }
          else if (!isEnabled)
          {
            string cardType = Type(card.IdCard).ToString();
            var cardPresentation = new CardPresentation(cardType, card.IdCard, card.Name) { SubChannelsCountOk = true, SubChannels = 0, State = "disabled" };
            cardPresentations.Add(cardPresentation);
          }

          else
          {
            string cardType = Type(card.IdCard).ToString();
            IDictionary<string, IUser> users = GetUsersForCard(card.IdCard);

            int subChannels = GetSubChannels(card.IdCard);
            if (users.Count == 0)
            {
              var cardPresentation = new CardPresentation(cardType, card.IdCard, card.Name) { SubChannels = subChannels, SubChannelsCountOk = (subChannels == 0), Idle = true, State = "Idle" };

              bool isScanning = IsScanning(card.IdCard);
              if (isScanning)
              {
                cardPresentation.State = "Scanning";
                cardPresentation.UserName = "n/a";
                cardPresentation.ChannelName = "n/a";
                cardPresentation.IsScrambled = "n/a";
                cardPresentation.IsOwner = "no";
              }

              cardPresentations.Add(cardPresentation);
            }
            else
            {
              HashSet<int> subchannelsInUse = new HashSet<int>();
              ICollection<IUser> usersCopy = new List<IUser>(users.Values); //avoid threading issues, make a copy.
              foreach (IUser user in usersCopy)
              {
                if (user.CardId == card.IdCard)
                {
                  bool isOwner = IsOwner(user.CardId, user.Name);
                  foreach (ISubChannel subchannel in new List<ISubChannel>(user.SubChannels.Values)) //avoid threading issues, make a copy.
                  {

                    if (!subchannelsInUse.Contains(subchannel.Id))
                    {
                      subchannelsInUse.Add(subchannel.Id);
                    }

                    bool isScrambled = IsScrambled(user.CardId, subchannel.Id);
                    var cardPresentation = new CardPresentation(cardType, card.IdCard, card.Name)
                    {
                      SubChannels = subChannels,
                      SubChannelsCountOk = true,
                      Idle = false,
                      UserName = user.Name,
                      ChannelName =
                        ChannelManagement.GetChannel(subchannel.IdChannel, ChannelIncludeRelationEnum.None).DisplayName,
                      IsScrambled = isScrambled ? "yes" : "no",
                      IsOwner = isOwner ? "yes" : "no",
                    };

                    double parkedDuration;
                    DateTime parkedAt;
                    bool isParked = cardHandler.ParkedUserManagement.IsUserParkedOnChannel(user.Name, subchannel.IdChannel,
                                                                                           out parkedDuration,
                                                                                           out parkedAt);
                    string state = "n/a";

                    if (user.UserType == UserType.Scheduler)
                    {
                      state = "Recording";
                    }
                    else if (user.UserType == UserType.EPG)
                    {
                      state = "Grabbing EPG";
                    }
                    else if (user.UserType == UserType.Scanner)
                    {
                      state = "Scanning";
                    }
                    else if (user.UserType == UserType.Normal)
                    {
                      state = "Timeshifting";
                      if (isParked)
                      {
                        state += " (PARK@" + parkedAt.ToShortTimeString() + ")";
                      }
                    }

                    if (user.UserType != UserType.EPG)
                    {
                      bool isGrabbingEpg = IsGrabbingEpg(card.IdCard);
                      if (isGrabbingEpg)
                      {
                        state += " (Grabbing EPG)";
                      }
                    }
                    cardPresentation.State = state;
                    cardPresentations.Add(cardPresentation);
                  }
                }
              }

              if (subChannels != subchannelsInUse.Count)
              {
                foreach (CardPresentation cardPresentation in cardPresentations)
                {
                  if (cardPresentation.CardId == card.IdCard)
                    cardPresentation.SubChannelsCountOk = false;
                }
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        HandleControllerException(e);
      }
      return cardPresentations;
    }  
  }
}