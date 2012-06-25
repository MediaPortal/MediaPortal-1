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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using MediaPortal.Common.Utils;
using MediaPortal.Common.Utils.ExtensionMethods;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Presentation;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog.Graphs.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.Hybrid;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Streaming;
using Mediaportal.TV.Server.TVService.CardManagement.CardAllocation;
using Mediaportal.TV.Server.TVService.CardManagement.CardHandler;
using Mediaportal.TV.Server.TVService.CardManagement.CardReservation;
using Mediaportal.TV.Server.TVService.CardManagement.CardReservation.Implementations;
using Mediaportal.TV.Server.TVService.DiskManagement;
using Mediaportal.TV.Server.TVService.Epg;
using Mediaportal.TV.Server.TVService.EventDispatchers;
using Mediaportal.TV.Server.TVService.Interfaces;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.CardReservation;
using Mediaportal.TV.Server.TVService.Interfaces.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.TVService.Scheduler;
using Mediaportal.TV.Server.TVService.Services;
using RecordingManagement = Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.RecordingManagement;

#endregion

namespace Mediaportal.TV.Server.TVService
{
  /// <summary>
  /// This class servers all requests from remote clients
  /// and if server is the master it will delegate the requests to the 
  /// correct slave servers
  /// </summary>
  public class TvController : MarshalByRefObject, IInternalControllerService, IDisposable, ITvServerEvent, ICiMenuCallbacks
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
        _ciMenuManager.IsCiMenuInteractive = true; // user action
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
        _ciMenuManager.IsCiMenuInteractive = false; // user action ended by wanted close
        return _cards[cardId].CiMenuActions.CloseCIMenu();
      }
      return false;
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
      Log.Debug("SendMenuAnswer called");
      if (ValidateTvControllerParams(cardId, false))
        return false;
      return _cards[cardId].CiMenuActions != null && _cards[cardId].CiMenuActions.SendMenuAnswer(cancel, answer);
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
        _ciMenuManager.ActiveCiMenuCard = cardId;
        res = _cards[cardId].CiMenuActions.SetCiMenuHandler(this);
        Log.Debug("TvController: SetCiMenuHandler: result {0}", res);
        return res;
      }
      else
        return false;
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
      if (ValidateTvControllerParams(cardId))
      {
        user = null;
        return false;
      }
      return _cards[cardId].UserManagement.IsLocked(out user);
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
      _cards[cardId].UserManagement.IsLocked(out user);
      return user;
    }

    public void Init()
    {
      Log.Info("Controller: Initializing TVServer");
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
            Log.Error("Controller: Error while deinit TvServer in Init");
          }
          Thread.Sleep(3000);
        }
        Log.Info("Controller: {0} init attempt", (i + 1));
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
        Log.Info("Controller: TVServer initialized okay");
      }
      else
      {
        Log.Info("Controller: Failed to initialize TVServer");
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

        _rtspStreamingPort = Int32.Parse(SettingsManagement.GetSetting("rtspPort", RtspStreaming.DefaultPort.ToString()).value);

        //enumerate all tv cards in this pc...
        _maxFreeCardsToTry = Int32.Parse(SettingsManagement.GetSetting("timeshiftMaxFreeCardsToTry", "0").value);

        IList<Card> allCards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None);

        foreach (ITVCard itvCard in _localCardCollection.Cards)
        {
          //for each card, check if its already mentioned in the database
          bool found = allCards.Any(card => card.devicePath == itvCard.DevicePath);
          if (!found)
          {
            // card is not yet in the database, so add it
            Log.Info("Controller: add card:{0}", itvCard.Name);

            var newCard = new Card
            {
              timeshiftingFolder = "",
              recordingFolder = "",
              devicePath = itvCard.DevicePath,
              name = itvCard.Name,
              priority = 1,
              grabEPG = true,
              enabled = true,
              camType = 0,
              recordingFormat = 0,
              decryptLimit = 0,
              stopgraph = true,
              NetProvider = (int)DbNetworkProvider.Generic
            };

            TVDatabase.TVBusinessLayer.CardManagement.SaveCard(newCard);
          }
        }
        //notify log about cards from the database which are removed from the pc
        int cardsInstalled = _localCardCollection.Cards.Count;
        foreach (Card dbsCard in allCards)
        {
          {
            bool found = false;
            for (int cardNumber = 0; cardNumber < cardsInstalled; ++cardNumber)
            {
              if (dbsCard.devicePath == _localCardCollection.Cards[cardNumber].DevicePath)
              {
                Card cardDB = TVDatabase.TVBusinessLayer.CardManagement.GetCardByDevicePath(_localCardCollection.Cards[cardNumber].DevicePath, CardIncludeRelationEnum.None);

                bool cardEnabled = cardDB.enabled;
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
              Log.Info("Controller: card not found :{0}", dbsCard.name);

              foreach (ITVCard t in _localCardCollection.Cards.Where(t => t.DevicePath == dbsCard.devicePath))
              {
                t.CardPresent = false;
                break;
              }

              // Fix mantis 0002790: Bad behavior when card count for IPTV = 0 
              if (dbsCard.name.StartsWith("MediaPortal IPTV Source Filter"))
              {
                CardRemove(dbsCard.idCard);
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
              if (t.DevicePath == card.devicePath)
              {
                localcards[card.idCard] = t;
                break;
              }
            }
          }
        }

        Log.Info("Controller: setup hybrid cards");
        IList<CardGroup> cardgroups = TVDatabase.TVBusinessLayer.CardManagement.ListAllCardGroups();
        foreach (CardGroup group in cardgroups)
        {
          IList<CardGroupMap> cards = group.CardGroupMaps;
          var hybridCardGroup = new HybridCardGroup();
          foreach (CardGroupMap card in cards)
          {
            if (localcards.ContainsKey(card.idCard))
            {
              localcards[card.idCard].IsHybrid = true;
              Log.WriteFile("Hybrid card: " + localcards[card.idCard].Name + " (" + group.name + ")");
              HybridCard hybridCard = hybridCardGroup.Add(card.idCard, localcards[card.idCard]);
              localcards[card.idCard] = hybridCard;
            }
          }
        }

        allCards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB
        foreach (Card dbsCard in allCards)
        {
          if (localcards.ContainsKey(dbsCard.idCard))
          {
            ITVCard card = localcards[dbsCard.idCard];
            var tvcard = new TvCardHandler(dbsCard, card);
            _cards[dbsCard.idCard] = tvcard;
          }

          // remove any old timeshifting TS files	
          try
          {
            string timeShiftPath = dbsCard.timeshiftingFolder;
            if (string.IsNullOrEmpty(dbsCard.timeshiftingFolder))
            {
              timeShiftPath = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer",
                                            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            }
            if (!Directory.Exists(timeShiftPath))
            {
              Log.Info("Controller: creating timeshifting folder {0} for card \"{1}\"", timeShiftPath, dbsCard.name);
              Directory.CreateDirectory(timeShiftPath);
            }

            Log.Debug("Controller: card {0}: current timeshiftpath = {1}", dbsCard.name, timeShiftPath);
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
            Log.Info("Controller: Error cleaning old ts buffer - {0}", exd.Message);
          }
        }

        Log.Info("Controller: setup streaming");
        _hostName = System.Net.Dns.GetHostName();
        SettingsManagement.SaveSetting("hostname", _hostName);
        _streamer = new RtspStreaming(_hostName, _rtspStreamingPort);


        _epgGrabber = new EpgGrabber();
        _epgGrabber.Start();
        _scheduler = new Scheduler.Scheduler();
        _scheduler.Start();

        SetupHeartbeatManager();
        SetupTvServerEventDispatcher();

        ExecutePendingDeletions();

        // Re-evaluate program states
        Log.Info("Controller: recalculating program states");

        ProgramManagement.ResetAllStates();
        ProgramManagement.SynchProgramStatesForAllSchedules(ScheduleManagement.ListAllSchedules());
      }
      catch (Exception ex)
      {
        Log.Write("TvControllerException: {0}\r\n{1}", ex.ToString(), ex.StackTrace);
        throw;
      }

      Log.Info("Controller: initalized");      
    }

    private void SetupTvServerEventDispatcher()
    {
      _tvServerEventDispatcher.Start();
    }

    private void SetupHeartbeatManager()
    {      
      _heartbeatManager.Start();
    }


    #endregion

    #region MarshalByRefObject overrides

    public override object InitializeLifetimeService()
    {
      return null;
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
      Log.Info("Controller: DeInit.");
      try
      {
        StopHeartbeatManager();
        StopTvserverEventDispatcher();

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
        Log.Error("TvController: Deinit failed - {0}", ex.Message);
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
      get { return _cards.Count; }
    }

    /// <summary>
    /// Gets the card Id for a card
    /// </summary>
    /// <param name="cardIndex">Index of the card.</param>
    /// <value>id of card</value>
    public int CardId(int cardIndex)
    {
      IList<Card> cards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None); //SEB
      return cards != null && cards.Count > cardIndex ? cards[cardIndex].idCard : -1;
    }

    /// <summary>
    /// returns if the card is enabled or disabled
    /// </summary>
    /// <param name="cardId">id of the card.</param>
    /// <value>true if enabled, otherwise false</value>
    public bool IsCardEnabled(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
        return false;
      return _cards[cardId].DataBaseCard.enabled;
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
    public bool IsCardPresent(int cardId)
    {
      bool cardPresent = false;
      if (cardId > 0)
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
        TVDatabase.TVBusinessLayer.CardManagement.DeleteCard(cardId);
        return;
      }

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
    /// <param name="idChannel"> </param>
    /// <returns>channel</returns>
    public IChannel CurrentChannel(ref IUser user, int idChannel)
    {
      if (ValidateTvControllerParams(user))
        return null;
      RefreshUserFromSpecificContext(ref user);
      return _cards[user.CardId].CurrentChannel(ref user, idChannel);
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
        /*IUser userExisting;
        bool foundUser = cardHandler.UserManagement.Users.TryGetValue(user.Name, out userExisting);
        if (foundUser)
        {
          if (cardHandler.UserManagement.GetTimeshiftingChannelId (userExisting.Name) > 0)
          {
            user = userExisting;
            return;
          }
        }*/
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
    /// <param name="user">User</param>
    /// <returns>id of database channel</returns>
    public int CurrentDbChannel(ref IUser user)
    {
      if (ValidateTvControllerParams(user))
        return -1;
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
      ITvCardHandler tvCardHandler = _cards[user.CardId];
      return tvCardHandler.CurrentChannelName(ref user, tvCardHandler.UserManagement.GetTimeshiftingChannelId(user.Name));
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
      RefreshUserFromSpecificContext(ref user);
      return _cards[user.CardId].Recorder.FileName(ref user);
    }

    public string TimeShiftFileName(string userName, int cardId)
    {
      if (!IsValidTvControllerParams(userName, cardId))
      {
        return "";
      }

      IUser user = GetUserFromSpecificContext(userName, cardId);
      if (ValidateTvControllerParams(user))
      {
        return "";
      }
      return _cards[user.CardId].TimeShifter.FileName(ref user);
    }

    private IUser GetUserFromSpecificContext(string userName, int cardId)
    {
      IUser user = null;
      if (cardId > 0)
      {
        ITvCardHandler tvCardHandler = _cards[cardId];
        if (tvCardHandler  != null)
        {
          user = tvCardHandler.UserManagement.GetUser(userName);
        }
      }
            
      return user;
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
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
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
      Schedule schedule = ScheduleManagement.GetSchedule(scheduleId);
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
      RefreshUserFromSpecificContext(ref user);
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
        user.CardId = card.DataBaseCard.idCard;
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
        user.CardId = card.DataBaseCard.idCard;

        if (!isRec)
        {
          isRec = card.Recorder.IsAnySubChannelRecording;
        }
        if (!isUserTS)
        {
          isUserTS = card.TimeShifter.IsTimeShifting(ref userTS);
        }

        IDictionary<string, IUser> users = card.UserManagement.Users;        
        foreach (IUser u in users.Values)
        {
          if (u.Name != userTS.Name)
          {
            if (!isAnyUserTS)
            {
              IUser anyUser = (IUser) u.Clone();
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
    /// <param name="idChannel"></param>
    /// <param name="card">The vcard.</param>
    /// <returns>
    /// 	<c>true</c> if the specified channel name is recording; otherwise, <c>false</c>.
    /// </returns>
    public bool IsRecording(int idChannel, out IVirtualCard card)
    {
      card = null;
      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      while (en.MoveNext())
      {
        ITvCardHandler tvcard = en.Current.Value;
        IDictionary<string, IUser> users = tvcard.UserManagement.Users;

        foreach (IUser user in users.Values)
        {
          if (user.UserType == UserType.Scheduler)
          {
            IUser userCopy = (IUser) user.Clone();
            if (tvcard.CurrentChannelName(ref userCopy, tvcard.UserManagement.GetTimeshiftingChannelId(user.Name)) == null)
            {
              continue;
            }
            if (tvcard.CurrentDbChannel(ref userCopy) == idChannel)
            {
              if (tvcard.Recorder.IsRecording(ref userCopy))
              {
                card = GetVirtualCard(userCopy);
                return true;
              }
            }
          }
        }
      }
      return false;
    }

    public List<IVirtualCard> GetAllRecordingCards()
    {
      List<IVirtualCard> recCards = new List<IVirtualCard>();

      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      ITvCardHandler tvcard;
      while (en.MoveNext())
      {
        tvcard = en.Current.Value;
        IDictionary<string, IUser> users = tvcard.UserManagement.Users;

        foreach (IUser user in users.Values)
        {
          IUser userCopy = (IUser)user.Clone();
          bool isREC = tvcard.Recorder.IsRecording(ref userCopy);
          if (isREC)
          {
            VirtualCard card = GetVirtualCard(userCopy);
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
    public bool IsRecordingTimeshifting(string channelName, out IVirtualCard card, out bool isTS, out bool isREC)
    {
      isREC = false;
      isTS = false;
      card = null;
      Dictionary<int, ITvCardHandler>.Enumerator en = _cards.GetEnumerator();
      IUser recUser = null;
      while (en.MoveNext())
      {
        ITvCardHandler tvcard = en.Current.Value;
        IDictionary<string, IUser> users = tvcard.UserManagement.Users;
        foreach (IUser user in users.Values)
        {
          foreach (var subchannel in user.SubChannels.Values)
          {
            IUser userCopy = (IUser) user.Clone();
            if (tvcard.CurrentChannelName(ref userCopy, subchannel.IdChannel) == null)
            {
              continue;
            }
            if (tvcard.CurrentChannelName(ref userCopy, subchannel.IdChannel) == channelName)
            {
              if (userCopy.UserType == UserType.Scheduler)
              {
                if (!isREC)
                {
                  isREC = tvcard.Recorder.IsRecording(ref userCopy);
                  if (isREC)
                  {
                    recUser = user;
                  }
                } 
              } 
              else if (!isTS)
              {
                isTS = tvcard.TimeShifter.IsTimeShifting(ref userCopy);
              }
            }
            if (isTS && isREC)
            {
              break;
            }
          }
        }
        if (isTS && isREC)
        {
          break;
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
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
      return _cards[user.CardId].Teletext.TeletextRotation(user, pageNumber);
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <param name="user">User</param>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime TimeShiftStarted(IUser user, int idChannel)
    {
      if (ValidateTvControllerParams(user))
        return DateTime.MinValue;
      RefreshUserFromSpecificContext(ref user);
      return _cards[user.CardId].TimeShifter.TimeShiftStarted(user, idChannel);
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
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
      return _cards[user.CardId].IsScrambled(ref user);
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
        RefreshUserFromSpecificContext(ref user);
        int cardId = user.CardId;
        ITvCardHandler cardHandler = _cards[cardId];
        if (cardHandler.DataBaseCard.enabled == false)
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
        ICardReservation cardreservationImpl = new CardReservationTimeshifting();
        try
        {
          ticket = cardreservationImpl.RequestCardTuneReservation(tvCardHandler, channel, user, idChannel);
          result = Tune(ref user, channel, idChannel, ticket);
        }
        catch (Exception)
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
      if (ValidateTvControllerParams(user) || ValidateTvControllerParams(channel))
      {
        return TvResult.UnknownError;
      }
      try
      {
        RefreshUserFromSpecificContext(ref user);
        int cardId = user.CardId;
        ITvCardHandler cardHandler = _cards[cardId];
        if (cardHandler.DataBaseCard.enabled == false)
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
      RefreshUserFromSpecificContext(ref user);
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
        return new byte[] { 1 };
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
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
    public TvResult StartTimeShifting(ref IUser user, ref string fileName, int idChannel)
    {
      if (ValidateTvControllerParams(user))
        return TvResult.UnknownError;
      try
      {
        RefreshUserFromSpecificContext(ref user);
        int cardId = user.CardId;
        Fire(this, new TvServerEventArgs(TvServerEventType.StartTimeShifting, GetVirtualCard(user), (User)user));
        StopEPGgrabber();

        bool isTimeShifting;
        ITvCardHandler tvCardHandler = _cards[cardId];
        try
        {
          isTimeShifting = tvCardHandler.TimeShifter.IsTimeShifting(ref user);
        }
        catch (Exception ex)
        {
          isTimeShifting = false;
          Log.Error("Exception in checking  " + ex.Message);
        }
        int subChannelId = tvCardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name);
        TvResult result = tvCardHandler.TimeShifter.Start(ref user, ref fileName, subChannelId, idChannel);
        if (result == TvResult.Succeeded)
        {
          if (!isTimeShifting)
          {            
            Log.Info("user:{0} card:{1} sub:{2} add stream:{3}", user.Name, user.CardId, subChannelId, fileName);
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
                  Log.Error("ParkedSubChannel or CurrentChannel is null when starting streaming");
                }

                var stream = new RtspStream(String.Format("stream{0}.{1}", cardId, subChannelId), fileName,
                                                  tvCardHandler.Card, mediaType);
                _streamer.AddStream(stream); 
              }
              else
              {
                Log.Error("could not start streaming server.");
              }
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
      RefreshUserFromSpecificContext(ref user);
      _cards[user.CardId].StopCard();
    }

    public bool ParkTimeShifting(ref IUser user, double duration, int idChannel)
    {      
      bool result = false;      
      //if (!ValidateTvControllerParams(user))
      {
        try
        {          
          ITvCardHandler cardHandler = GetCardHandlerByChannel(idChannel, TvUsage.Timeshifting);
          if (cardHandler != null)
          {
            if (cardHandler.DataBaseCard.enabled)
            {
              RefreshTimeshiftingUserFromAnyContext(ref user);
              if (cardHandler.TimeShifter.IsTimeShifting(ref user))
              {
                Fire(this, new TvServerEventArgs(TvServerEventType.TimeShiftingParked, GetVirtualCard(user), (User)user));
                Log.Write("Controller: ParkTimeShifting {0}", cardHandler.DataBaseCard.idCard);
                cardHandler.ParkedUserManagement.ParkUser(ref user, duration, idChannel);
                UpdateChannelStatesForUsers();
                result = true;
              }
            } 
          }
          else
          {
            Log.Error("StopTimeShifting - could not find channel to park. {0} - {1} - {2}", user.Name, idChannel, duration);
          }
        }
        catch (Exception ex)
        {
          Log.Write(ex);
        }
      }
      return result;
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

    public bool UnParkTimeShifting(ref IUser user, double duration, int idChannel, out IVirtualCard card)
    {
      bool result = false;
      card = null;
      try
      {                
        ITvCardHandler cardHandler = GetCardHandlerByParkedChannelAndDuration(idChannel, duration);
        if (cardHandler != null)
        {
          if (cardHandler.DataBaseCard.enabled)
          {            
            RefreshTimeshiftingUserFromAnyContext(ref user);
            int timeshiftingChannelId = GetTimeShiftingChannelIdFromContext(user);
            
            Log.Write("Controller: UnParkTimeShifting {0}", cardHandler.DataBaseCard.idCard);
            cardHandler.ParkedUserManagement.UnParkUser(ref user, duration, idChannel);
            VirtualCard virtualCard = GetVirtualCard(user);
            card = virtualCard;

            Fire(this, new TvServerEventArgs(TvServerEventType.TimeShiftingUnParked, virtualCard, (User)user));
            if (timeshiftingChannelId > 0)
            {
              StopTimeShifting(ref user, timeshiftingChannelId); // 'StopTimeShifting' already calls UpdateChannelStatesForUsers
            }
            else
            {
              UpdateChannelStatesForUsers();
            }
            StopTimeShiftingAllChannelsExcept(user, idChannel);
            result = true;            
          } 
        }
        else
        {
          Log.Error("StopTimeShifting - could not find channel to unpark. {0} - {1} - {2}", user.Name, idChannel, duration);          
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      
      return result;
    }

    

    public void PauseCard(IUser user)
    {
      if (ValidateTvControllerParams(user))
        return;
      RefreshUserFromSpecificContext(ref user);
      _cards[user.CardId].PauseCard();
    }

    public bool StopTimeShifting(ref IUser user, TvStoppedReason reason)
    {
      ITvCardHandler tvCardHandler = _cards[user.CardId];
      return StopTimeShifting(ref user, reason, tvCardHandler.UserManagement.GetTimeshiftingChannelId(user.Name));
    }

    public TvStoppedReason GetTvStoppedReason(IUser user)
    {
      if (ValidateTvControllerParams(user))
        return TvStoppedReason.UnknownReason;

      try
      {
        RefreshUserFromSpecificContext(ref user);
        if (_cards[user.CardId].DataBaseCard.enabled == false)
          return TvStoppedReason.UnknownReason;
        //if (!CardPresent(user.CardId)) return TvStoppedReason.UnknownReason;

        return _cards[user.CardId].UserManagement.GetTimeshiftStoppedReason(user);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return TvStoppedReason.UnknownReason;
    }

    public bool StopTimeShifting(ref IUser user, TvStoppedReason reason, int channelId)
    {
      if (ValidateTvControllerParams(user))
        return false;
      RefreshUserFromSpecificContext(ref user);
      _cards[user.CardId].UserManagement.SetTimeshiftStoppedReason(user, reason);      

      user.TvStoppedReason = reason;
      Fire(this, new TvServerEventArgs(TvServerEventType.ForcefullyStoppedTimeShifting, GetVirtualCard(user), (User)user));

      return StopTimeShifting(ref user, channelId);
    }

    public bool StopTimeShifting(ref IUser user, int channelId)
    {
      if (ValidateTvControllerParams(user))
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
          int cardId = tvcard.DataBaseCard.idCard;          
          if (tvcard.DataBaseCard.enabled == false)
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
          if (!tvcard.TimeShifter.IsTimeShifting(ref user))
            return true;
          Fire(this, new TvServerEventArgs(TvServerEventType.EndTimeShifting, GetVirtualCard(user), (User)user));

          if (tvcard.Recorder.IsRecording(ref user))
            return true;

          Log.Write("Controller: StopTimeShifting {0}", cardId);
          return DoStopTimeShifting(ref user, cardId, channelId);
        }
        else
        {
          Log.Error("StopTimeShifting - could not find channel to stop. {0} - {1}", user.Name, channelId);
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }

      return false;
    }

    private ITvCardHandler GetCardHandlerByParkedChannelAndDuration(int channelId, double duration)
    {            
      foreach (ITvCardHandler cardHandler in _cards.Values)
      {        
        foreach (IUser user in cardHandler.UserManagement.Users.Values)
        {
          int subchannelId = cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, channelId, TvUsage.Parked);
          if (subchannelId > -1)
          {
            bool hasParkedUserWithDuration = cardHandler.ParkedUserManagement.HasParkedUserWithDuration(subchannelId, duration);
            if (hasParkedUserWithDuration)
            {              
              return cardHandler;              
            }            
          }          
        }        
      }
      return null;
    }

    private ITvCardHandler GetCardHandlerByChannel (int channelId, TvUsage tvUsage)
    {
      foreach (ITvCardHandler cardHandler in _cards.Values)
      {        
        foreach (IUser user in cardHandler.UserManagement.Users.Values)
        {
          if (cardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, channelId, tvUsage) > -1)
          {
            return cardHandler;
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
        IUser user = card.UserManagement.GetUser(userName);
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
    /// <param name="user">User</param>
    /// <param name="channelId"> </param>
    /// <returns></returns>
    public bool StopTimeShifting(ref IUser user)
    {      
      RefreshTimeshiftingUserFromAnyContext(ref user);
      int timeshiftingChannelId = GetTimeShiftingChannelIdFromContext(user);
      if (timeshiftingChannelId > 0)
      {        
        return StopTimeShifting(ref user, timeshiftingChannelId); 
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
            Log.Write("Controller:Timeshifting stopped on card:{0}", cardId);
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
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the recording file.</param>
    /// <returns></returns>
    public TvResult StartRecording(ref IUser user, ref string fileName)
    {
      if (ValidateTvControllerParams(user))
      {
        return TvResult.UnknownError;
      }
      RefreshUserFromSpecificContext(ref user);
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
      RefreshUserFromSpecificContext(ref user);
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

      settings.TimeOutTune = Int32.Parse(SettingsManagement.GetSetting("timeoutTune", "2").value);
      settings.TimeOutPAT = Int32.Parse(SettingsManagement.GetSetting("timeoutPAT", "5").value);
      settings.TimeOutCAT = Int32.Parse(SettingsManagement.GetSetting("timeoutCAT", "5").value);
      settings.TimeOutPMT = Int32.Parse(SettingsManagement.GetSetting("timeoutPMT", "10").value);
      settings.TimeOutSDT = Int32.Parse(SettingsManagement.GetSetting("timeoutSDT", "20").value);
      settings.TimeOutAnalog = Int32.Parse(SettingsManagement.GetSetting("timeoutAnalog", "20").value);
      return _cards[cardId].Scanner.Scan(channel, settings);
    }

    public IChannel[] ScanNIT(int cardId, IChannel channel)
    {
      if (ValidateTvControllerParams(cardId))
        return null;

      ScanParameters settings = new ScanParameters();

      settings.TimeOutTune = Int32.Parse(SettingsManagement.GetSetting("timeoutTune", "2").value);
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
        Recording rec = TVDatabase.TVBusinessLayer.RecordingManagement.GetRecording(idRecording);
        if (rec == null)
        {
          return false;
        }

        if (_streamer != null)
        {
          _streamer.RemoveFile(rec.fileName);
        }
        bool result = RecordingFileHandler.DeleteRecordingOnDisk(rec.fileName);
        if (result)
        {
          RecordingManagement.DeleteRecording(rec.idRecording);
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
    /// <param name="rec">recording</param>
    private bool IsRecordingValid(Recording rec)
    {
      try
      {
        if (rec == null)
        {
          return false;
        }
        return (File.Exists(rec.fileName));
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
      IList<Recording> itemlist = RecordingManagement.ListAllRecordingsByMediaType(MediaTypeEnum.TV);
      bool foundInvalidRecording = false;
      foreach (Recording rec in itemlist.Where(rec => !IsRecordingValid(rec)))
      {
        try
        {
          RecordingManagement.DeleteRecording(rec.idRecording);
        }
        catch (Exception e)
        {
          Log.Error("Controller: Can't delete invalid recording", e);
        }
        foundInvalidRecording = true;
      }
      return foundInvalidRecording;
    }

    /// <summary>
    /// Deletes watched recordings from database.
    /// </summary>
    public bool DeleteWatchedRecordings(string currentTitle)
    {
      IList<Recording> itemlist = TVDatabase.TVBusinessLayer.RecordingManagement.ListAllRecordingsByMediaType(MediaTypeEnum.TV);
      bool foundWatchedRecordings = false;
      foreach (Recording rec in itemlist)
      {
        if (rec.timesWatched > 0)
        {
          if (currentTitle == null || currentTitle == rec.title)
          {
            DeleteRecording(rec.idRecording);
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
    /// <param name="userName"> </param>
    /// <returns>
    /// id of Schedule or -1 if  card not recording
    /// </returns>
    public int GetRecordingSchedule(int cardId, string userName)
    {
      try
      {
        
        if (ValidateTvControllerParams(cardId))
        {
          return -1;
        }
        ITvCardHandler tvCardHandler = _cards[cardId];
        if (!tvCardHandler.DataBaseCard.enabled)
        {
          return -1;
        }
                
        IUser user = tvCardHandler.UserManagement.GetUser(userName);
        int channelId = tvCardHandler.UserManagement.GetTimeshiftingChannelId(user.Name);
        return _scheduler.GetRecordingScheduleForCard(cardId, channelId);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return -1;
      }
    }

    #region audio streams

    public IList<IAudioStream> AvailableAudioStreams(IUser user)
    {
      if (ValidateTvControllerParams(user))
        return null;
      RefreshUserFromSpecificContext(ref user);

      ITvCardHandler tvCardHandler = _cards[user.CardId];
      int idChannel = tvCardHandler.UserManagement.GetTimeshiftingChannelId(user.Name);
      return tvCardHandler.Audio.Streams(user, idChannel);
    }

    public IAudioStream GetCurrentAudioStream(IUser user, int idChannel)
    {
      if (ValidateTvControllerParams(user))
        return null;
      RefreshUserFromSpecificContext(ref user);
      return _cards[user.CardId].Audio.GetCurrent(user, idChannel);
    }

    public void SetCurrentAudioStream(IUser user, IAudioStream stream, int idChannel)
    {
      if (ValidateTvControllerParams(user))
        return;
      RefreshUserFromSpecificContext(ref user);
      _cards[user.CardId].Audio.Set(user, stream, idChannel);
    }

    public string GetStreamingUrl(IUser user)
    {
      if (ValidateTvControllerParams(user))
      {
        return "";
      }
      try
      {
        RefreshUserFromSpecificContext(ref user);
        ITvCardHandler tvCardHandler = _cards[user.CardId];
        if (tvCardHandler.DataBaseCard.enabled == false)
        {
          return "";
        }

        return String.Format("rtsp://{0}:{1}/stream{2}.{3}", _hostName, _streamer.Port, user.CardId,
                             tvCardHandler.UserManagement.GetTimeshiftingSubChannel(user.Name));
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
        Recording recording = TVDatabase.TVBusinessLayer.RecordingManagement.GetRecording(idRecording);
        if (recording == null)
          return "";
        if (recording.fileName == null)
          return "";
        if (recording.fileName.Length == 0)
          return "";

        try
        {
          if (File.Exists(recording.fileName))
          {            
            _streamer.Start();
            string streamName = String.Format("{0:X}", recording.fileName.GetHashCode());
            RtspStream stream = new RtspStream(streamName, recording.fileName, recording.title);
            _streamer.AddStream(stream);
            string url = String.Format("rtsp://{0}:{1}/{2}", _hostName, _streamer.Port, streamName);
            Log.Info("Controller: streaming url:{0} file:{1}", url, recording.fileName);
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
        Recording recording = TVDatabase.TVBusinessLayer.RecordingManagement.GetRecording(idRecording);
        if (recording == null)
          return "";
        if (recording.fileName == null)
          return "";
        if (recording.fileName.Length == 0)
          return "";

        try
        {
          string chapterFile = Path.ChangeExtension(recording.fileName, ".txt");
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
        if (_streamer != null)
        {
          _streamer.Start();
          string streamName = String.Format("{0:X}", fileName.GetHashCode());
          RtspStream stream = new RtspStream(streamName, fileName, streamName);
          _streamer.AddStream(stream);
          string url = String.Format("rtsp://{0}:{1}/{2}", _hostName, _streamer.Port, streamName);
          Log.Info("Controller: streaming url:{0} file:{1}", url, fileName);
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
        Log.Info("Controller: dispose card:{0}", tvCardHandler.CardName);
        try
        {
          tvCardHandler.ParkedUserManagement.CancelAllParkedUsers();
          tvCardHandler.StopCard();
          tvCardHandler.Dispose();
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

      Channel channel = ChannelManagement.GetChannel(idChannel);
      Log.Write("Controller: TimeShiftingWouldUseCard {0} {1}", channel.displayName, channel.idChannel);

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
    /// <param name="kickCardId"> </param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <param name="kickableCards"> </param>
    /// <param name="forceCardId">Indicated, if the card should be forced</param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>    
    public TvResult StartTimeShifting(ref IUser user, int idChannel, int? kickCardId, out IVirtualCard card, out Dictionary<int, List<IUser>> kickableCards, bool forceCardId)
    {      
      bool cardChanged = false;
      double? parkedDuration;
      return StartTimeShifting(ref user, idChannel, kickCardId, out card, out kickableCards, forceCardId, out cardChanged, out parkedDuration);
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
      int oldCardId = -1;
      RefreshTimeshiftingUserFromAnyContext(ref user);
      if (user != null)
      {
        oldCardId = user.CardId;
        string initialTimeshiftingFile = TimeShiftFileName(user.Name, user.CardId);
        user.Priority = UserFactory.GetDefaultPriority(user.Name, user.Priority);
        Channel channel = ChannelManagement.GetChannel(idChannel);
        Log.Write("Controller: StartTimeShifting {0} {1}", channel.displayName, channel.idChannel);
        StopEPGgrabber();

        ICollection<ICardTuneReservationTicket> tickets = null;
        try
        {
          var cardAllocationStatic = new AdvancedCardAllocationStatic();
          List<CardDetail> freeCardsForReservation = cardAllocationStatic.GetFreeCardsForChannel(_cards, channel, ref user);
          if (HasFreeCards(freeCardsForReservation))
          {
            tickets = IterateCardsUntilTimeshifting(
              ref user,
              channel,
              forceCardId,
              freeCardsForReservation,
              kickCardId,
              out kickableCards,
              out cardChanged, ref result, ref card, ref parkedDuration);
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
          else if (!cardChanged)
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

   

    private ICollection<ICardTuneReservationTicket> IterateCardsUntilTimeshifting(ref IUser user, Channel channel, bool forceCardId, ICollection<CardDetail> freeCardsForReservation, int? kickCardId, out Dictionary<int, List<IUser>> kickableCards, out bool cardChanged, ref TvResult result, ref IVirtualCard card, ref double? parkedDuration)
    {
      kickableCards = null;
      cardChanged = false;      
      var cardResImpl = new CardReservationTimeshifting();
      ICollection<ICardTuneReservationTicket> tickets = null;
      ICollection<int> freeCardsIterated = UpdateCardsIteratedBasedOnForceCardId(user, forceCardId);
      int cardsIterated = 0;
      bool moreCardsAvailable = true;
      while (moreCardsAvailable && !HasTvSucceeded(result))
      {
        tickets = CardReservationHelper.RequestCardReservations(user, freeCardsForReservation, cardResImpl,
                                                                freeCardsIterated, channel.idChannel);
        if (HasTickets(tickets))
        {
          var cardAllocationTicket = new AdvancedCardAllocationTicket(tickets);
          ICollection<CardDetail> freeCards = cardAllocationTicket.UpdateFreeCardsForChannelBasedOnTicket(freeCardsForReservation,
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
              freeCards,
              maxCards,
              kickCardId,
              out kickableCards,
              ref card, ref result, ref cardsIterated, out cardChanged, ref parkedDuration);
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
        foreach (KeyValuePair<int, ITvCardHandler> card in _cards.Where(t => t.Value.DataBaseCard.idCard != user.CardId))
        {
          freeCardsIterated.Add(card.Value.DataBaseCard.idCard);
        }
      }
      return freeCardsIterated;
    }

    private bool IterateTicketsUntilTimeshifting(ref IUser userBefore, Channel channel, ICollection<ICardTuneReservationTicket> tickets, CardReservationTimeshifting cardResImpl, ICollection<CardDetail> freeCards, int maxCards, int? kickCardId, out Dictionary<int, List<IUser>> kickableCards, ref IVirtualCard card, ref TvResult result, ref int cardsIterated, out bool cardChanged, ref double? parkedDuration)
    {      
      kickableCards = null;
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
        var newCardId = cardInfo.Id;
        IUser userNow = UserFactory.CreateBasicUser(userBefore.Name, newCardId, userBefore.Priority, userBefore.UserType);
        SetupTimeShiftingFolders(cardInfo);
        ITvCardHandler tvcard = _cards[newCardId];
        try
        {
          ICardTuneReservationTicket ticket = GetTicketByCardId(cardInfo, tickets);
          if (ticket == null)
          {
            Log.Write("Controller: StartTimeShifting - could not find cardreservation on card:{0}",
                      userNow.CardId);
            HandleAllCardsBusy(tickets, out result, out failedCardId, cardInfo, tvcard);
            continue;
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
                Log.Write("Controller: leech user={0} inherits subch={1}", userBefore.Name, ticket.OwnerSubchannel.Id);                
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
                  }

                  result = TvResult.AlreadyParked;
                  break;
                  /*
                  tvCardHandler.ParkedUserManagement.CancelParkedUserBySubChannelId(userBefore.Name, ticket.OwnerSubchannel.Id);
                  ISubChannel subch = tvCardHandler.UserManagement.GetSubChannel(userBefore.Name, ticket.OwnerSubchannel.Id);
                  if (subch != null)
                  {
                    subch.TvUsage = TvUsage.Timeshifting;                    
                  }*/
                }
                else
                {
                  tvCardHandler.UserManagement.AddSubChannelOrUser(userNow, ticket.OwnerSubchannel.IdChannel, ticket.OwnerSubchannel.Id); 
                }                
              }
            }
            else
            {              
              if (!TransponderAcquired(maxCards, ticket, tvcard, cardIteration, channel.idChannel, kickCardId, ref kickableCards))
              {
                if (kickableCards != null && kickableCards.Count > 0)
                {
                  result = TvResult.UsersBlocking;
                  HandleTvException(tickets, out failedCardId, cardInfo, tvcard);
                }
                else
                {
                  HandleAllCardsBusy(tickets, out result, out failedCardId, cardInfo, tvcard); 
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
              HandleTvException(tickets, out failedCardId, cardInfo, tvcard);
              StopTimeShifting(ref userNow, channel.idChannel);
              continue; //try next card            
            }
          }

          //reset failedCardId incase previous card iteration failed.
          failedCardId = -1;
          CardReservationHelper.CancelAllCardReservations(tickets, _cards);
          Log.Info("control2:{0} {1} {2}", userNow.Name, userNow.CardId, tvcard.UserManagement.GetSubChannelIdByChannelId(userNow.Name, channel.idChannel));
          card = GetVirtualCard(userNow);
          card.NrOfOtherUsersTimeshiftingOnCard = ticket.NumberOfOtherUsersOnSameChannel;

          StopTimeShiftingAllChannelsExcept(userNow, channel.idChannel);
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
          cardIteration++;

          if (failedCardId > 0)
          {
            userBefore.FailedCardId = failedCardId;
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
            kickableCards = null;            
          }
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
      HandleTvException(tickets, out failedCardId, cardInfo, tvcard);
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

    private bool TransponderAcquired(int maxCards, ICardTuneReservationTicket ticket, ITvCardHandler tvcard, int cardIteration, int idChannel, int? kickCardId, ref Dictionary<int, List<IUser>> kickableCards)
    {      
      bool isTransponderAvailable = false;
      bool foundAnyUsersOnCard = FoundAnyUsersOnCard(ticket);
      if (foundAnyUsersOnCard)
      {
        bool foundCandidateForKicking = FoundCandidateForKicking(ticket);
        if (foundCandidateForKicking)
        {
          if (!kickCardId.HasValue || (kickCardId.Value == tvcard.DataBaseCard.idCard))
          {
            bool usersKicked = KickLeechingUsersIfNoMoreCardsAvail(tvcard, ticket, cardIteration, maxCards, idChannel);
            bool cardsAvailable = ((cardIteration + 1) < maxCards);       
            if (!usersKicked && cardsAvailable)
            {
              Log.Write(
                "Controller: skipping card:{0} since other users are present on the same channel and there are still cards available.",
                tvcard.DataBaseCard.idCard);
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
            kickableCards[tvcard.DataBaseCard.idCard] = ticket.ActiveUsers.ToList();
            Log.Write("Controller: not allowed to kick users on card:{0}, politely asking client...",
            tvcard.DataBaseCard.idCard);
          }
          else
          {
            isTransponderAvailable = true;
          }
        }
        else
        {
          Log.Write(
            "Controller: skipping card:{0} since it is busy (user(s) present with higher priority).",
            tvcard.DataBaseCard.idCard);
        }
      }
      else
      {
        isTransponderAvailable = true;
      }      
      return isTransponderAvailable;
    }
    

    private static void UpdateFreeCardsIterated(ICollection<int> freeCardsIterated, IEnumerable<CardDetail> freeCards)
    {
      foreach (CardDetail card in freeCards)
      {
        int idCard = card.Card.idCard;
        if (!freeCardsIterated.Contains(idCard))
        {
          freeCardsIterated.Add(idCard);
        }
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
              Channel ch = ChannelManagement.GetChannel(subchannel.IdChannel);
              if (ch != null)
              {
                channelInfo = ch.displayName;
              }

              Log.Write(
                "Controller: kicking leech user '{0}' with prio={1} off card={2} on channel={3} (subchannel #{4}) since owner '{5}' with prio={6} (subchannel #{7}) changed transponder and there are no more cards available",
                activeUser.Name,
                activeUser.Priority,
                tvcard.DataBaseCard.name,
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
            IUser activeUser = tvcard.UserManagement.GetUser(userName);
            StopTimeShifting(ref activeUser, TvStoppedReason.OwnerChangedTS, channelId); 
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
          Log.Debug("controller: RemoveInactiveUsers {0}", inactiveUser.Name);
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
      if (cardInfo.Card.recordingFolder == String.Empty)
      {
        cardInfo.Card.recordingFolder = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\recordings",
                                                      Environment.GetFolderPath(
                                                        Environment.SpecialFolder.CommonApplicationData));
        if (!Directory.Exists(cardInfo.Card.recordingFolder))
        {
          Log.Write("Controller: creating recording folder {0} for card {0}", cardInfo.Card.recordingFolder,
                    cardInfo.Card.name);
          Directory.CreateDirectory(cardInfo.Card.recordingFolder);
        }
      }
      if (cardInfo.Card.timeshiftingFolder == String.Empty)
      {
        cardInfo.Card.timeshiftingFolder = String.Format(
          @"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer",
          Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        if (!Directory.Exists(cardInfo.Card.timeshiftingFolder))
        {
          Log.Write("Controller: creating timeshifting folder {0} for card {0}", cardInfo.Card.timeshiftingFolder,
                    cardInfo.Card.name);
          Directory.CreateDirectory(cardInfo.Card.timeshiftingFolder);
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
    public TvResult StartTimeShifting(ref IUser user, int idChannel, out IVirtualCard card)
    {
      bool cardChanged;
      Dictionary<int, List<IUser>> kickableCards;
      double? parkedDuration;
      return StartTimeShifting(ref user, idChannel, null, out card, out kickableCards, false, out cardChanged, out parkedDuration);
    }

    /// <summary>
    /// Start timeshifting on a specific channel
    /// </summary>
    /// <param name="user">user credentials.</param>
    /// <param name="idChannel">The id channel.</param>
    /// <param name="kickCardId"> </param>
    /// <param name="card">returns card for which timeshifting is started</param>
    /// <param name="kickableCards"> </param>
    /// <param name="cardChanged">indicates if card was changed</param>
    /// <param name="parkedDuration"> </param>
    /// <returns>
    /// TvResult indicating whether method succeeded
    /// </returns>
    public TvResult StartTimeShifting(ref IUser user, int idChannel, int? kickCardId, out IVirtualCard card, out Dictionary<int, List<IUser>> kickableCards, out bool cardChanged, out double? parkedDuration)
    {
      return StartTimeShifting(ref user, idChannel, kickCardId, out card, out kickableCards, false, out cardChanged, out parkedDuration);
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
        //Gentle.Common.CacheManager.ClearQueryResultsByType(typeof (Schedule));
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
        //Gentle.Common.CacheManager.ClearQueryResultsByType(typeof (Schedule));
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
              if (SettingsManagement.GetSetting("idleEPGGrabberEnabled", "yes").value == "yes")
              {
                StartEPGgrabber();
              }
            }
          }
          else
          {
            if (_epgGrabber != null)
            {
              if (SettingsManagement.GetSetting("idleEPGGrabberEnabled", "yes").value == "yes")
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
      RefreshUserFromSpecificContext(ref user);
      _cards[user.CardId].Epg.Stop(user);
    }

    public IEnumerable<string> ServerIpAdresses
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



    public IDictionary<string, IUser> GetUsersForCard(int cardId)
    {
      if (ValidateTvControllerParams(cardId))
      {
        return null;
      }
      return _cards[cardId].UserManagement.Users;
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
        IDictionary<string, IUser> users = tvcard.UserManagement.Users;
        foreach (IUser user in users.Values)        
        {
          IUser userCopy = (IUser)user.Clone();         
          foreach(var subchannel in userCopy.SubChannels.Values)
          {
            string tmpChannel = tvcard.CurrentChannelName(ref userCopy, subchannel.IdChannel);
            if (string.IsNullOrEmpty(tmpChannel))
            {
              continue;
            }

            int idChannel = subchannel.IdChannel;
            if (tvcard.Recorder.IsRecording(ref userCopy))
            {
              if (result.ContainsKey(idChannel))
              {
                result.Remove(idChannel);
              }
              result.Add(idChannel, ChannelState.recording);
            }
            else if (tvcard.TimeShifter.IsTimeShifting(ref userCopy))
            {
              if (!result.ContainsKey(idChannel))
              {
                result.Add(idChannel, ChannelState.timeshifting);
              }
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
        IDictionary<string, IUser> users = tvcard.UserManagement.Users;

        foreach (IUser user in users.Values)
        {
          IUser userCopy = (IUser)user.Clone();

          foreach (ISubChannel subchannel in userCopy.SubChannels.Values)
          {
            int idChannel = subchannel.IdChannel;

            string tmpChannel = tvcard.CurrentChannelName(ref userCopy, idChannel);
            if (string.IsNullOrEmpty(tmpChannel))
            {
              continue;
            }
            if (user.UserType == UserType.Scheduler && tvcard.Recorder.IsRecording(ref userCopy))
            {
              currentRecChannels.Add(tvcard.CurrentDbChannel(ref userCopy));
            }
            else if (tvcard.TimeShifter.IsTimeShifting(ref userCopy))
            {
              currentTSChannels.Add(tvcard.CurrentDbChannel(ref userCopy));
            }
            else
            {
              ChannelState cState = GetChannelState(tvcard.CurrentDbChannel(ref userCopy), userCopy);
              if (cState == ChannelState.tunable)
              {
                currentAvailChannels.Add(tvcard.CurrentDbChannel(ref userCopy));
              }
              else
              {
                currentUnavailChannels.Add(tvcard.CurrentDbChannel(ref userCopy));
              }
            }
          }
        }
      }
    }

    public IDictionary<int, ChannelState> GetAllChannelStatesForIdleUserCached()
    {
      return _channelStatesCachedForIdleUser;
    }

    /// <summary>
    /// Fetches all channel states for a specific user (cached - faster)
    /// </summary>    
    /// <param name="user"></param>      
    public IDictionary<int, ChannelState> GetAllChannelStatesCached(IUser user)
    {
      IDictionary<int, ChannelState> allChannelStatesCached = null;

      if (user != null && user.CardId > 0 && _cards.Any())
      {
        IDictionary<string, IUser> users = _cards[user.CardId].UserManagement.Users;
        IUser u;
        bool userFound = users.TryGetValue(user.Name, out u);
        if (userFound)
        {
          allChannelStatesCached = u.ChannelStates;
        }
      }

      if (allChannelStatesCached == null)
      {
        allChannelStatesCached = _channelStatesCachedForIdleUser;
      }

      return allChannelStatesCached;
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
        IDictionary<int, ChannelState> channelStates = GetAllChannelStatesCached(user);

        if (channelStates != null)
        {
          if (!channelStates.TryGetValue(idChannel, out chanState))
          {
            chanState = ChannelState.tunable;
          }
        }
        else
        {
          Channel dbchannel = ChannelManagement.GetChannel(idChannel);
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
          if (group.groupName.Equals(TvConstants.TvGroupNames.AllChannels))
            continue;

          if (_tvChannelListGroups == null)
          {
            _tvChannelListGroups =
              ChannelManagement.GetAllChannelsByGroupIdAndMediaType(group.idGroup, MediaTypeEnum.TV).ToList();
          }
          else
          {
            IList<Channel> tvChannelList = ChannelManagement.GetAllChannelsByGroupIdAndMediaType(group.idGroup,
                                                                                                 MediaTypeEnum.TV);
            foreach (Channel ch in tvChannelList)
            {
              bool exists = _tvChannelListGroups.Exists(c => c.idChannel == ch.idChannel);
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
      Fire(this, new TvServerEventArgs(TvServerEventType.ChannelStatesChanged, new VirtualCard(user), (User) user));
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
      RefreshUserFromSpecificContext(ref user);
      return _cards[cardId].UserManagement.IsOwner(user.Name);
    }


    //string userName, int cardId, int channelId
    private void StopTimeShiftingAllChannelsExcept(IUser user, int idChannel)
    {
      var channelIdsRemoveList = new List<int>();
      foreach (ITvCardHandler cardHandler in CardCollection.Values)
      {
        IUser existingUser = cardHandler.UserManagement.GetUser(user.Name);
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
        Log.Debug("StopTimeShiftingAllChannelsExcept : {0} - {1} - {2}", user.Name, user.CardId, idChannel);
        StopTimeShifting(ref user, channelId2Remove); 
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
        RefreshUserFromSpecificContext(ref user);
        ITvCardHandler tvCardHandler = _cards[user.CardId];
        if (!tvCardHandler.DataBaseCard.enabled)
        {
          return TvResult.CardIsDisabled;
        }

        if (ticket.ConflictingSubchannelFound)
        {
          tvCardHandler.UserManagement.GetNextAvailableSubchannel(user.Name);
        }

        Fire(this, new TvServerEventArgs(TvServerEventType.StartZapChannel, GetVirtualCard(user), (User)user, channel));

        tvCardHandler.Tuner.OnAfterTuneEvent -= Tuner_OnAfterTuneEvent;
        tvCardHandler.Tuner.OnBeforeTuneEvent -= Tuner_OnBeforeTuneEvent;

        tvCardHandler.Tuner.OnAfterTuneEvent += Tuner_OnAfterTuneEvent;
        tvCardHandler.Tuner.OnBeforeTuneEvent += Tuner_OnBeforeTuneEvent;

        cardResTS.OnStartCardTune += CardResTsOnStartCardTune;
        TvResult result = cardResTS.CardTune(tvCardHandler, ref user, channel, dbChannel, ticket);
        cardResTS.OnStartCardTune -= CardResTsOnStartCardTune;

        Log.Info("Controller: {0} {1} {2}", user.Name, user.CardId, tvCardHandler.UserManagement.GetSubChannelIdByChannelId(user.Name, dbChannel.idChannel));

        return result;
      }
      finally
      {
        Fire(this, new TvServerEventArgs(TvServerEventType.EndZapChannel, GetVirtualCard(user), (User)user, channel));
      }
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
      RefreshUserFromSpecificContext(ref user);
      var card = new VirtualCard(user)
      {
        RecordingFormat = _cards[user.CardId].DataBaseCard.recordingFormat,
        RecordingFolder = _cards[user.CardId].DataBaseCard.recordingFolder,
        TimeshiftFolder = _cards[user.CardId].DataBaseCard.timeshiftingFolder,
        RemoteServer = Dns.GetHostName()
      };
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
          {
            continue;
          }
          IDictionary<string, IUser> users = _cards[cardId].UserManagement.Users;
          foreach (IUser user in users.Values)
          {
            IUser userCopy = (IUser)user.Clone();
            if (_cards[cardId].Recorder.IsRecording(ref userCopy) ||
                _cards[cardId].TimeShifter.IsTimeShifting(ref userCopy))
            {
              //Log.Debug("TVController.CanSuspend: checking cards finished -> cannot suspend");
              return false;
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
      if (cardId < 0 || !_cards.ContainsKey(cardId) || (checkCardPresent && !IsCardPresent(cardId)))
      {
#if DEBUG
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(0);
        Log.Error(
          "TVController:" + sf.GetMethod().Name +
          " - incorrect parameters used! cardId {0} _cards.ContainsKey(cardId) == {1} CardPresent {2}", cardId,
          _cards.ContainsKey(cardId), IsCardPresent(cardId));
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
          Log.Error(
            "TVController:" + sf.GetMethod().Name +
            " - incorrect parameters used! user {0} cardId {1} _cards.ContainsKey(cardId) == {2} CardPresent(cardId) {3}",
            userName, cardId, _cards.ContainsKey(cardId), IsCardPresent(cardId));
        }
        else
        {
          Log.Error("TVController:" + sf.GetMethod().Name + " - incorrect parameters used! user NULL");
        }
        Log.Error("{0}", st);
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
          Log.Error(
            "TVController:" + sf.GetMethod().Name +
            " - incorrect parameters used! user {0} cardId {1} _cards.ContainsKey(cardId) == {2} CardPresent(cardId) {3}",
            user, user.CardId, _cards.ContainsKey(user.CardId), IsCardPresent(user.CardId));
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
      try
      {
        // System.Diagnostics.Debugger.Launch();
        List<int> pendingDelitionRemove = new List<int>();
        IList<PendingDeletion> pendingDeletions = RecordingManagement.ListAllPendingRecordingDeletions();

        Log.Debug("ExecutePendingDeletions: number of pending deletions : " + Convert.ToString(pendingDeletions.Count));

        foreach (PendingDeletion pendingDelition in pendingDeletions)
        {
          Log.Debug("ExecutePendingDeletions: trying to remove file : " + pendingDelition.fileName);

          bool wasPendingDeletionAdded = false;
          bool wasDeleted = RecordingFileHandler.DeleteRecordingOnDisk(pendingDelition.fileName,
                                                                       out wasPendingDeletionAdded);
          if (wasDeleted && !wasPendingDeletionAdded)
          {
            pendingDelitionRemove.Add(pendingDelition.idPendingDeletion);
          }
        }

        foreach (int id in pendingDelitionRemove)
        {
          PendingDeletion pendingDelition = RecordingManagement.GetPendingRecordingDeletion(id);

          if (pendingDelition != null)
          {
            RecordingManagement.DeletePendingRecordingDeletion(pendingDelition.idPendingDeletion);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("ExecutePendingDeletions exception : " + ex.Message);
      }
    }



    public void OnResume()
    {
      if (!_onResumeDone)
      {
        Log.Info("TvController.OnResume()");
        SetupHeartbeatManager();

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

      StopHeartbeatManager();
      StopTvserverEventDispatcher();
      if (_scheduler != null)
      {
        _scheduler.Stop();
      }

      IUser tmpUser = new User();
      foreach (ITvCardHandler cardhandler in this.CardCollection.Values)
      {
        cardhandler.ParkedUserManagement.CancelAllParkedUsers();
        cardhandler.StopCard();
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

    public void RegisterUserForHeartbeatMonitoring (string username)
    {
      _heartbeatManager.Register(username);
    }

    public void UnRegisterUserForHeartbeatMonitoring(string username)
    {
      _heartbeatManager.UnRegister(username);
    }

    public void RegisterUserForCiMenu(string username)
    {
      _ciMenuManager.Register(username);      
    }
    public void UnRegisterUserForCiMenu(string username)
    {
      _ciMenuManager.UnRegister(username);
    }

    public void RegisterUserForTvServerEvents(string username)
    {
      _tvServerEventDispatcher.Register(username);
    }
    public void UnRegisterUserForTvServerEvents(string username)
    {
      _tvServerEventDispatcher.UnRegister(username);
    }

    

    public IDictionary<string, byte[]> GetPluginBinaries()
    {
      var fileStreams = new Dictionary<string, byte[]>();
      var dirInfo = new DirectoryInfo("plugins");

      FileInfo[] files = dirInfo.GetFiles("*.dll");

      foreach (FileInfo fileInfo in files)
      {
        using (var filestream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
        {
          long length = filestream.Length;
          var data = new byte[length];
          filestream.Read(data, 0, (int) length);
          fileStreams.Add(fileInfo.Name, data);
        }
      }
      
      return fileStreams;
    }

    public IList<StreamPresentation> ListAllStreamingChannels()
    {
      IList<StreamPresentation> streams = new List<StreamPresentation>();
            
      foreach (ITvCardHandler cardHandler in _cards.Values)
      {
        if (!cardHandler.DataBaseCard.enabled)
        {
          continue;
        }
        int idCard = cardHandler.DataBaseCard.idCard;
        if (!IsCardPresent(idCard))
        {
          continue;
        }        

        IDictionary<string, IUser> users = cardHandler.UserManagement.Users;
        if (users != null)
        {
          foreach (IUser user in users.Values)
          {
            IVirtualCard vcard;
            var userCopy = (IUser)user.Clone();
            bool isRecording = IsRecording(CurrentDbChannel(ref userCopy), out vcard);
            bool isTimeShifting = IsTimeShifting(ref userCopy);
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
          if (!card.enabled)
          {
            continue;
          }
          if (!IsCardPresent(card.idCard))
          {
            continue;
          }
          IDictionary<string, IUser> users = GetUsersForCard(card.idCard);
          if (users != null)
          {
            foreach (IUser user in users.Values.Where(user => card.idCard == user.CardId))
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
      catch (Exception ex)
      {
        Log.Write(ex);
      }

      return isCardParkedByUser;
    }

    public IList<CardPresentation> ListAllCards()
    {
      IList<CardPresentation> cardPresentations = new List<CardPresentation>();
      IEnumerable<Card> cards = TVDatabase.TVBusinessLayer.CardManagement.ListAllCards(CardIncludeRelationEnum.None);

      foreach (Card card in cards)
      {
        ITvCardHandler cardHandler;
        bool isAvailable = _cards.TryGetValue(card.idCard, out cardHandler);
        bool isEnabled = false;

        if (isAvailable)
        {
          isEnabled = card.enabled;
          isAvailable = IsCardPresent(card.idCard);  
        }
                                
        if (!isAvailable)
        {
          var cardPresentation = new CardPresentation("n/a", card.idCard, card.name) { SubChannelsCountOk = true, SubChannels = 0, State = "n/a" };          
          cardPresentations.Add(cardPresentation);
        }
        else if (!isEnabled)
        {
          string cardType = Type(card.idCard).ToString();
          var cardPresentation = new CardPresentation(cardType, card.idCard, card.name) { SubChannelsCountOk = true, SubChannels = 0, State = "disabled" };
          cardPresentations.Add(cardPresentation);
        }

        else
        {
          string cardType = Type(card.idCard).ToString();          
          IDictionary<string, IUser> users = GetUsersForCard(card.idCard);

          int subChannels = GetSubChannels(card.idCard);
          if (users.Count == 0)
          {
            var cardPresentation = new CardPresentation(cardType, card.idCard, card.name) { SubChannels = subChannels, SubChannelsCountOk = (subChannels == 0), Idle = true, State = "Idle" };

            bool isScanning = IsScanning(card.idCard);
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
              if (user.CardId == card.idCard)
              {
                bool isOwner = IsOwner(user.CardId, user);
                foreach (ISubChannel subchannel in user.SubChannels.Values)
                {

                  if (!subchannelsInUse.Contains(subchannel.Id))
                  {
                    subchannelsInUse.Add(subchannel.Id);
                  }

                  bool isScrambled = IsScrambled(user.CardId, subchannel.Id);
                  var cardPresentation = new CardPresentation(cardType, card.idCard, card.name)
                                           {
                                             SubChannels = subChannels,
                                             SubChannelsCountOk = true,
                                             Idle = false,                                             
                                             UserName = user.Name,
                                             ChannelName =
                                               ChannelManagement.GetChannel(subchannel.IdChannel).displayName,
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
                    bool isGrabbingEpg = IsGrabbingEpg(card.idCard);
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
                if (cardPresentation.CardId == card.idCard)
                cardPresentation.SubChannelsCountOk = false;
              }
            }
          }
        }
      }
      return cardPresentations;
    }

   


    public bool CanUserParkTimeshifting(IUser user)
    {
      bool canUserParkTimeshifting = false;
      if (!ValidateTvControllerParams(user))
      {
        try
        {
          RefreshUserFromSpecificContext(ref user);
          int cardId = user.CardId;
          ITvCardHandler tvcard = _cards[cardId];
          if (tvcard.DataBaseCard.enabled)
          {
            if (tvcard.TimeShifter.IsTimeShifting(ref user))
            {
              canUserParkTimeshifting = !tvcard.ParkedUserManagement.IsUserParkedOnAnyChannel(user.Name);
            }
          }
        }
        catch (Exception ex)
        {
          Log.Write(ex);
        }
      }
      
      return canUserParkTimeshifting;
    }
  }
}