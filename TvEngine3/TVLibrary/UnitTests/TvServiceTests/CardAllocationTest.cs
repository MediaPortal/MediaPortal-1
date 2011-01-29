#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvService;
using TVServiceTests.Mocks;
using TVServiceTests.Mocks.Channel;
using TVServiceTests.Mocks.DVBCCard;
using TVServiceTests.Mocks.DVBSCard;
using TVServiceTests.Mocks.DVBTCard;
using TypeMock.ArrangeActAssert;
using TvLibrary.Channels;

#endregion

namespace CardAllocationTests
{
  [TestFixture]
  [Isolated]
  public class CardAllocationTest
  {
    #region setup/teardown

    [TestFixtureSetUp]
    public void TVHomeTestFixtureSetup()
    {
      
    }

    [TearDown]
    public void TVHomeTearDown()
    {
      Console.WriteLine("Test");
    }

    [TestFixtureTearDown]
    public void TVHomeTestFixtureTearDown()
    {      
    }

    [SetUp]
    public void TVHomeSetUp()
    {
      Isolate.Fake.StaticMethods<Log>(Members.MustSpecifyReturnValues);
      Isolate.WhenCalled(() => Log.Info("test", null)).DoInstead(callContext =>
      {
        string format = callContext.Parameters[0] as string;
        object[] arg = callContext.Parameters[1] as object[];
        string logLine = string.Format(format, arg);
        string threadName = Thread.CurrentThread.Name;
        int threadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff} [{1}({2})]: {3}", DateTime.Now, threadName, threadId, logLine);
      });
      Isolate.WhenCalled(() => Log.Debug("test", null)).DoInstead(callContext =>
      {
        string format = callContext.Parameters[0] as string;
        object[] arg = callContext.Parameters[1] as object[];
        string logLine = string.Format(format, arg);
        string threadName = Thread.CurrentThread.Name;
        int threadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff} [{1}({2})]: {3}", DateTime.Now, threadName, threadId, logLine);
      });
      Isolate.WhenCalled(() => Log.Error("test", null)).DoInstead(callContext =>
      {
        string format = callContext.Parameters[0] as string;
        object[] arg = callContext.Parameters[1] as object[];
        string logLine = string.Format(format, arg);
        string threadName = Thread.CurrentThread.Name;
        int threadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff} [{1}({2})]: {3}", DateTime.Now, threadName, threadId, logLine);
      });
    }    

    #endregion

    #region Card Allocation Tests

    #region simple 1 card tests

    #region dvb-t

    [Test]
    public void TestOneAvailIdleDVBTCardForFTAChannel()
    {
      TestOneAvailIdleCardForFTAChannel(CardType.DvbT);
    }

    [Test]
    public void TestOneAvailIdleDVBTCardForScrambledChannel()
    {
      TestOneAvailIdleFTACardForScrambledChannel(CardType.DvbT);
    }

    [Test]
    public void TestOneAvailIdleCAMDVBTCardForScrambledChannel()
    {
      TestOneAvailIdleCAMCardForScrambledChannel(CardType.DvbT);
    }    
   
    [Test]
    public void TestOneAvailLockedDVBTCardForFTAChannel()
    {
      TestOneAvailLockedCardForFTAChannel(CardType.DvbT);
    }

    [Test]
    public void TestOneDisabledDVBTCardForFTAChannel()
    {
      TestOneDisabledCardForFTAChannel(CardType.DvbT);
    }

    [Test]
    public void TestOneAbsentDVBTCardForFTAChannel()
    {
      TestOneAbsentCardForFTAChannel(CardType.DvbT);     
    }
   
    [Test]
    public void TestWrongTuningDetailForDVBTCardForFTAChannel()
    {
      TestWrongTuningDetailForCardForFTAChannel(CardType.DvbT);
    }

    

    #endregion

    #region dvb-s

    [Test]
    public void TestOneAvailIdleDVBSCardForFTAChannel()
    {
      TestOneAvailIdleCardForFTAChannel(CardType.DvbS);
    }

    [Test]
    public void TestOneAvailIdleDVBSCardForScrambledChannel()
    {
      TestOneAvailIdleFTACardForScrambledChannel(CardType.DvbS);
    }

    [Test]
    public void TestOneAvailIdleCAMDVBSCardForScrambledChannel()
    {      
      TestOneAvailIdleCAMCardForScrambledChannel(CardType.DvbS);
    }    

    [Test]
    public void TestOneAvailLockedDVBSCardForFTAChannel()
    {
      TestOneAvailLockedCardForFTAChannel(CardType.DvbS);
    }

    [Test]
    public void TestOneDisabledDVBSCardForFTAChannel()
    {
      TestOneDisabledCardForFTAChannel(CardType.DvbS);
    }

    [Test]
    public void TestOneAbsentDVBSCardForFTAChannel()
    {
      TestOneAbsentCardForFTAChannel(CardType.DvbS);
    }

    [Test]
    public void TestWrongTuningDetailForDVBSCardForFTAChannel()
    {
      TestWrongTuningDetailForCardForFTAChannel(CardType.DvbS);
    }
    
    #endregion

    #region dvb-c

    [Test]
    public void TestOneAvailIdleDVBCCardForFTAChannel()
    {
      TestOneAvailIdleCardForFTAChannel(CardType.DvbC);
    }

    [Test]
    public void TestOneAvailIdleDVBCCardForScrambledChannel()
    {
      TestOneAvailIdleFTACardForScrambledChannel(CardType.DvbC);
    }

    [Test]
    public void TestOneAvailIdleCAMDVBCCardForScrambledChannel()
    {
      TestOneAvailIdleCAMCardForScrambledChannel(CardType.DvbC);
    }    

    [Test]
    public void TestOneAvailLockedDVBCCardForFTAChannel()
    {
      TestOneAvailLockedCardForFTAChannel(CardType.DvbC);
    }

    [Test]
    public void TestOneDisabledDVBCCardForFTAChannel()
    {
      TestOneDisabledCardForFTAChannel(CardType.DvbC);
    }

    [Test]
    public void TestOneAbsentDVBCCardForFTAChannel()
    {
      TestOneAbsentCardForFTAChannel(CardType.DvbC);
    }

    [Test]
    public void TestWrongTuningDetailForDVBCCardForFTAChannel()
    {
      TestWrongTuningDetailForCardForFTAChannel(CardType.DvbC);
    }

    #endregion

    #region common

    private void TestOneAvailIdleCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      IUser user = Fakes.FakeUser();
      TvBusinessLayer businessLayer = Fakes.FakeTvBusinessLayer();;
      TVController controller = Fakes.FakeTVController();

      IChannel tuningDetail1 = GetFTATuningDetailBasedOnCardType(cardType);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Idle = true;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      cards.Add(dvbCardMocks.CardId, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelsByDbChannel(channel)).WillReturn(tuningDetails);
      TVControllerMocks.CardPresent(cardHandler1.DataBaseCard.IdCard, controller);

      SetupChannelMapping(cardHandler1, channelMap, businessLayer, channel);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer, controller);

      TvResult result;
      List<CardDetail> availCards = allocation.GetFreeCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(1, availCards.Count, "The number of cards returned is not as expected");
    }

    private void TestOneAvailIdleCAMCardForScrambledChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      IUser user = Fakes.FakeUser();
      TvBusinessLayer businessLayer = Fakes.FakeTvBusinessLayer();;
      TVController controller = Fakes.FakeTVController();
      IChannel tuningDetail1 = GetScrambledTuningDetailBasedOnCardType(cardType);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Idle = true;
      dvbCardMocks.HasCAM = true;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      cards.Add(dvbCardMocks.CardId, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelsByDbChannel(channel)).WillReturn(tuningDetails);
      TVControllerMocks.CardPresent(cardHandler1.DataBaseCard.IdCard, controller);

      SetupChannelMapping(cardHandler1, channelMap, businessLayer, channel);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer, controller);

      TvResult result;
      List<CardDetail> availCards = allocation.GetFreeCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(1, availCards.Count, "The number of cards returned is not as expected");
    }

    private void TestOneAvailIdleFTACardForScrambledChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      IUser user = Fakes.FakeUser();
      TvBusinessLayer businessLayer = Fakes.FakeTvBusinessLayer();;
      TVController controller = Fakes.FakeTVController();
      IChannel tuningDetail1 = GetScrambledTuningDetailBasedOnCardType(cardType);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Idle = true;
      dvbCardMocks.HasCAM = false;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      cards.Add(dvbCardMocks.CardId, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelsByDbChannel(channel)).WillReturn(tuningDetails);
      TVControllerMocks.CardPresent(cardHandler1.DataBaseCard.IdCard, controller);

      SetupChannelMapping(cardHandler1, channelMap, businessLayer, channel);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer, controller);

      TvResult result;
      List<CardDetail> availCards = allocation.GetFreeCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(0, availCards.Count, "The number of cards returned is not as expected");
    }
    

    private void TestOneAvailLockedCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      IUser user = Fakes.FakeUser();
      TvBusinessLayer businessLayer = Fakes.FakeTvBusinessLayer();;
      TVController controller = Fakes.FakeTVController();

      IChannel tuningDetail1 = GetFTATuningDetailBasedOnCardType(cardType);
      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Idle = false;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      cards.Add(dvbCardMocks.CardId, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelsByDbChannel(channel)).WillReturn(tuningDetails);
      TVControllerMocks.CardPresent(cardHandler1.DataBaseCard.IdCard, controller);

      SetupChannelMapping(cardHandler1, channelMap, businessLayer, channel);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer, controller);

      TvResult result;
      List<CardDetail> availCards = allocation.GetFreeCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(1, availCards.Count, "The number of cards returned is not as expected");
      Assert.AreEqual(availCards[0].SameTransponder, false, "same transponder not as expected");
    }


    private void TestOneDisabledCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      IUser user = Fakes.FakeUser();
      TvBusinessLayer businessLayer = Fakes.FakeTvBusinessLayer();;
      TVController controller = Fakes.FakeTVController();
      IChannel tuningDetail1 = GetFTATuningDetailBasedOnCardType(cardType);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Enabled = false;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      //ITvCardHandler cardHandler1 = DVBTCardMocks.AddDisabledFTADVBTCard(tuningDetail1, user);
      cards.Add(dvbCardMocks.CardId, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelsByDbChannel(channel)).WillReturn(tuningDetails);
      TVControllerMocks.CardPresent(cardHandler1.DataBaseCard.IdCard, controller);

      SetupChannelMapping(cardHandler1, channelMap, businessLayer, channel);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer, controller);

      TvResult result;
      List<CardDetail> availCards = allocation.GetFreeCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(0, availCards.Count, "The number of cards returned is not as expected");
    }

    private void TestOneAbsentCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      IUser user = Fakes.FakeUser();
      TvBusinessLayer businessLayer = Fakes.FakeTvBusinessLayer();;
      TVController controller = Fakes.FakeTVController();
      IChannel tuningDetail1 = GetFTATuningDetailBasedOnCardType(cardType);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Present = false;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      //ITvCardHandler cardHandler1 = DVBTCardMocks.AddAbsentFTADVBTCard(tuningDetail1, user);
      cards.Add(dvbCardMocks.CardId, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelsByDbChannel(channel)).WillReturn(tuningDetails);
      TVControllerMocks.CardNotPresent(cardHandler1.DataBaseCard.IdCard, controller);

      SetupChannelMapping(cardHandler1, channelMap, businessLayer, channel);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer, controller);

      TvResult result;
      List<CardDetail> availCards = allocation.GetFreeCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(0, availCards.Count, "The number of cards returned is not as expected");
    }

    [Test]
    public void TestNoTuningDetailsChannel()
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      IUser user = Fakes.FakeUser();
      TvBusinessLayer businessLayer = Fakes.FakeTvBusinessLayer();;
      TVController controller = Fakes.FakeTVController();

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(CardType.DvbC, tuningDetails, user);
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      cards.Add(dvbCardMocks.CardId, cardHandler1);


      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelsByDbChannel(channel)).WillReturn(tuningDetails);
      TVControllerMocks.CardPresent(cardHandler1.DataBaseCard.IdCard, controller);

      SetupChannelMapping(cardHandler1, channelMap, businessLayer, channel);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer, controller);

      TvResult result;
      List<CardDetail> availCards = allocation.GetFreeCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(0, availCards.Count, "The number of cards returned is not as expected");
    }

    private void TestWrongTuningDetailForCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      IUser user = Fakes.FakeUser();
      TvBusinessLayer businessLayer = Fakes.FakeTvBusinessLayer();;
      TVController controller = Fakes.FakeTVController();

      //we need to pick a different cardtype here-
      CardType cardTypeDifferent = cardType;
      switch (cardType)
      {
        case CardType.DvbT:
          cardTypeDifferent = CardType.DvbC;
          break;

        case CardType.DvbC:
          cardTypeDifferent = CardType.DvbS;
          break;

        case CardType.DvbS:
          cardTypeDifferent = CardType.DvbT;
          break;
      }
      IChannel tuningDetail1 = GetFTATuningDetailBasedOnCardType(cardTypeDifferent);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      //ITvCardHandler cardHandler1 = DVBTCardMocks.AddIdleFTADVBTCard(tuningDetail1, user);
      cards.Add(dvbCardMocks.CardId, cardHandler1);


      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelsByDbChannel(channel)).WillReturn(tuningDetails);
      TVControllerMocks.CardPresent(cardHandler1.DataBaseCard.IdCard, controller);

      SetupChannelMapping(cardHandler1, channelMap, businessLayer, channel);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer, controller);

      TvResult result;
      List<CardDetail> availCards = allocation.GetFreeCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(0, availCards.Count, "The number of cards returned is not as expected");
    }
    #endregion

    #endregion

    #region advanced multiple card tests

   

    [Test]
    public void Test2AvailIdleDVBTCardsForFTAChannel()
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      IUser user = Fakes.FakeUser();
      TvBusinessLayer businessLayer = Fakes.FakeTvBusinessLayer();
      TVController controller = Fakes.FakeTVController();
      IChannel tuningDetail1 = GetFTATuningDetailBasedOnCardType(CardType.DvbT);      

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks1 = GetCardMockByCardType(CardType.DvbT, tuningDetails, user);
      DVBCardMocks dvbCardMocks2 = GetCardMockByCardType(CardType.DvbT, tuningDetails, user);
      dvbCardMocks1.Idle = true;
      dvbCardMocks2.Idle = true;

      ITvCardHandler cardHandler1 = dvbCardMocks1.GetMockedCardHandler();
      cards.Add(dvbCardMocks1.CardId, cardHandler1);

      ITvCardHandler cardHandler2 = dvbCardMocks1.GetMockedCardHandler();
      cards.Add(dvbCardMocks2.CardId, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelsByDbChannel(channel)).WillReturn(tuningDetails);      

      TVControllerMocks.CardPresent(cardHandler1.DataBaseCard.IdCard, controller);
      TVControllerMocks.CardPresent(cardHandler2.DataBaseCard.IdCard, controller);

      SetupChannelMapping(cardHandler1, channelMap, businessLayer, channel);
      SetupChannelMapping(cardHandler2, channelMap, businessLayer, channel);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer, controller);

      TvResult result;
      List<CardDetail> availCards = allocation.GetFreeCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(2, availCards.Count, "The number of cards returned is not as expected");
    }

   

    #region common
    #endregion

    #endregion

    #region helpers


    //TODO: move these helpers to other static classes

    private void SetupChannelMapping(ITvCardHandler cardHandler1, ChannelMap channelMap, TvBusinessLayer businessLayer, Channel channel)
    {
      Isolate.WhenCalled(() => channelMap.EpgOnly).WillReturn(false);
      Isolate.WhenCalled(() => channelMap.ReferencedCard().DevicePath).WillReturn(cardHandler1.DataBaseCard.DevicePath);

      TvDatabase.Card card = cardHandler1.DataBaseCard;
      Isolate.WhenCalled(() => businessLayer.IsChannelMappedToCard(channel, card, false)).WillReturn(true);      

    }

    private DVBCardMocks GetCardMockByCardType(CardType cardType, List<IChannel> tuningDetails, IUser user)
    {
      DVBCardMocks cardMocks = null;
      switch (cardType)
      {
        case CardType.DvbT:
          cardMocks = new DVBTCardMocks(tuningDetails, user);
          break;

        case CardType.DvbC:
          cardMocks = new DVBCCardMocks(tuningDetails, user);
          break;

        case CardType.DvbS:
          cardMocks = new DVBSCardMocks(tuningDetails, user);
          break;
      }
      return cardMocks;
    }

    private IChannel GetFTATuningDetailBasedOnCardType(CardType cardType)
    {
      IChannel channel = GetTuningDetailBasedOnCardType(cardType);
      Isolate.WhenCalled(() => channel.FreeToAir).WillReturn(true);
      return channel;
    }

    private IChannel GetScrambledTuningDetailBasedOnCardType(CardType cardType)
    {
      IChannel channel = GetTuningDetailBasedOnCardType(cardType);
      Isolate.WhenCalled(() => channel.FreeToAir).WillReturn(false);
      return channel;
    }

    private IChannel GetTuningDetailBasedOnCardType(CardType cardType)
    {
      IChannel tuningDetail = null;
      switch (cardType)
      {
        case CardType.DvbT:
          tuningDetail = Isolate.Fake.Instance<DVBTChannel>();
          break;

        case CardType.DvbC:
          tuningDetail = Isolate.Fake.Instance<DVBCChannel>();
          break;

        case CardType.DvbS:
          tuningDetail = Isolate.Fake.Instance<DVBSChannel>();
          break;
      }
      return tuningDetail;
    }

   
       

    #endregion

    #endregion

  }
}
