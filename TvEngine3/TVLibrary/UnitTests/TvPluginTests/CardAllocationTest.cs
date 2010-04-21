#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using NUnit.Mocks;
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
  public class CardAllocationTest
  {
    #region setup/teardown

    [TestFixtureSetUp]
    public void TVHomeTestFixtureSetup()
    {
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
      Isolate.CleanUp();
    }    

    #endregion

    #region Card Allocation Tests

    public enum CardType
    {
      DVBt,
      DVBs,
      DVBc,
      Analogue,
      IP,
      ATSC
    }

    #region simple 1 card tests

    #region dvb-t

    [Test]
    public void TestOneAvailIdleDVBTCardForFTAChannel()
    {
      TestOneAvailIdleCardForFTAChannel(CardType.DVBt);
    }
   
    [Test]
    public void TestOneAvailLockedDVBTCardForFTAChannel()
    {      
      TestOneAvailLockedCardForFTAChannel(CardType.DVBt);
    }

    [Test]
    public void TestOneDisabledDVBTCardForFTAChannel()
    {
      TestOneDisabledCardForFTAChannel(CardType.DVBt);
    }

    [Test]
    public void TestOneAbsentDVBTCardForFTAChannel()
    {
      TestOneAbsentCardForFTAChannel(CardType.DVBt);     
    }
   
    [Test]
    public void TestNoTuningDetailForDVBTCardForFTAChannel()
    {
      TestNoTuningDetailForCardForFTAChannel(CardType.DVBt);     
    }    

    [Test]
    public void TestWrongTuningDetailForDVBTCardForFTAChannel()
    {
      TestWrongTuningDetailForCardForFTAChannel(CardType.DVBt);
    }

    

    #endregion

    #region dvb-s

    [Test]
    public void TestOneAvailIdleDVBSCardForFTAChannel()
    {
      TestOneAvailIdleCardForFTAChannel(CardType.DVBs);
    }

    [Test]
    public void TestOneAvailLockedDVBSCardForFTAChannel()
    {
      TestOneAvailLockedCardForFTAChannel(CardType.DVBs);
    }

    [Test]
    public void TestOneDisabledDVBSCardForFTAChannel()
    {
      TestOneDisabledCardForFTAChannel(CardType.DVBs);
    }

    [Test]
    public void TestOneAbsentDVBSCardForFTAChannel()
    {
      TestOneAbsentCardForFTAChannel(CardType.DVBs);
    }

    [Test]
    public void TestNoTuningDetailForDVBSCardForFTAChannel()
    {
      TestNoTuningDetailForCardForFTAChannel(CardType.DVBs);
    }

    [Test]
    public void TestWrongTuningDetailForDVBSCardForFTAChannel()
    {
      TestWrongTuningDetailForCardForFTAChannel(CardType.DVBs);
    }
    
    #endregion

    #region dvb-c

    [Test]
    public void TestOneAvailIdleDVBCCardForFTAChannel()
    {
      TestOneAvailIdleCardForFTAChannel(CardType.DVBc);
    }

    [Test]
    public void TestOneAvailLockedDVBCCardForFTAChannel()
    {
      TestOneAvailLockedCardForFTAChannel(CardType.DVBc);
    }

    [Test]
    public void TestOneDisabledDVBCCardForFTAChannel()
    {
      TestOneDisabledCardForFTAChannel(CardType.DVBc);
    }

    [Test]
    public void TestOneAbsentDVBCCardForFTAChannel()
    {
      TestOneAbsentCardForFTAChannel(CardType.DVBc);
    }

    [Test]
    public void TestNoTuningDetailForDVBCCardForFTAChannel()
    {
      TestNoTuningDetailForCardForFTAChannel(CardType.DVBc);
    }

    [Test]
    public void TestWrongTuningDetailForDVBCCardForFTAChannel()
    {
      TestWrongTuningDetailForCardForFTAChannel(CardType.DVBc);
    }

    #endregion

    #region common

    private void TestOneAvailIdleCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      User user = Isolate.Fake.Instance<User>();
      TvBusinessLayer businessLayer = Isolate.Fake.Instance<TvBusinessLayer>();
      IChannel tuningDetail1 = GetTuningDetailBasedOnCardType(cardType);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Idle = true;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      cards.Add(cardHandler1.DataBaseCard.IdCard, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetFTAChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelByName(channel)).WillReturn(tuningDetails);

      SetupChannelMapping(cardHandler1, channelMap);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer);

      TvResult result;
      List<CardDetail> availCards = allocation.GetAvailableCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(1, availCards.Count, "The number of cards returned is not as expected");
    }


    private void TestOneAvailLockedCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      User user = Isolate.Fake.Instance<User>();
      TvBusinessLayer businessLayer = Isolate.Fake.Instance<TvBusinessLayer>();

      IChannel tuningDetail1 = GetTuningDetailBasedOnCardType(cardType);
      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Idle = false;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      cards.Add(cardHandler1.DataBaseCard.IdCard, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetFTAChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelByName(channel)).WillReturn(tuningDetails);

      SetupChannelMapping(cardHandler1, channelMap);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer);

      TvResult result;
      List<CardDetail> availCards = allocation.GetAvailableCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(1, availCards.Count, "The number of cards returned is not as expected");
      Assert.AreEqual(availCards[0].SameTransponder, false, "same transponder not as expected");
    }


    private void TestOneDisabledCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      User user = Isolate.Fake.Instance<User>();
      TvBusinessLayer businessLayer = Isolate.Fake.Instance<TvBusinessLayer>();
      IChannel tuningDetail1 = GetTuningDetailBasedOnCardType(cardType);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Enabled = false;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      //ITvCardHandler cardHandler1 = DVBTCardMocks.AddDisabledFTADVBTCard(tuningDetail1, user);
      cards.Add(cardHandler1.DataBaseCard.IdCard, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetFTAChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelByName(channel)).WillReturn(tuningDetails);

      SetupChannelMapping(cardHandler1, channelMap);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer);

      TvResult result;
      List<CardDetail> availCards = allocation.GetAvailableCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(0, availCards.Count, "The number of cards returned is not as expected");
    }

    private void TestOneAbsentCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();
      
      User user = Isolate.Fake.Instance<User>();
      TvBusinessLayer businessLayer = Isolate.Fake.Instance<TvBusinessLayer>();
      IChannel tuningDetail1 = GetTuningDetailBasedOnCardType(cardType);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      dvbCardMocks.Present = false;
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      //ITvCardHandler cardHandler1 = DVBTCardMocks.AddAbsentFTADVBTCard(tuningDetail1, user);
      cards.Add(cardHandler1.DataBaseCard.IdCard, cardHandler1);

      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetFTAChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelByName(channel)).WillReturn(tuningDetails);

      SetupChannelMapping(cardHandler1, channelMap);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer);

      TvResult result;
      List<CardDetail> availCards = allocation.GetAvailableCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(0, availCards.Count, "The number of cards returned is not as expected");
    }


    private void TestNoTuningDetailForCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      User user = Isolate.Fake.Instance<User>();
      TvBusinessLayer businessLayer = Isolate.Fake.Instance<TvBusinessLayer>();
      IChannel tuningDetail1 = GetTuningDetailBasedOnCardType(cardType);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      //ITvCardHandler cardHandler1 = DVBTCardMocks.AddIdleFTADVBTCard(tuningDetail1, user);
      cards.Add(cardHandler1.DataBaseCard.IdCard, cardHandler1);


      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetFTAChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelByName(channel)).WillReturn(tuningDetails);

      SetupChannelMapping(cardHandler1, channelMap);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer);

      TvResult result;
      List<CardDetail> availCards = allocation.GetAvailableCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(0, availCards.Count, "The number of cards returned is not as expected");
    }

    private void TestWrongTuningDetailForCardForFTAChannel(CardType cardType)
    {
      Dictionary<int, ITvCardHandler> cards = new Dictionary<int, ITvCardHandler>();
      List<IChannel> tuningDetails = new List<IChannel>();

      User user = Isolate.Fake.Instance<User>();
      TvBusinessLayer businessLayer = Isolate.Fake.Instance<TvBusinessLayer>();

      //we need to pick a different cardtype here-
      CardType cardTypeDifferent = cardType;
      switch (cardType)
      {
        case CardType.DVBt:
          cardTypeDifferent = CardType.DVBc;
          break;

        case CardType.DVBc:
          cardTypeDifferent = CardType.DVBs;
          break;

        case CardType.DVBs:
          cardTypeDifferent = CardType.DVBt;
          break;
      }
      IChannel tuningDetail1 = GetTuningDetailBasedOnCardType(cardTypeDifferent);

      tuningDetails.Add(tuningDetail1);

      DVBCardMocks dvbCardMocks = GetCardMockByCardType(cardType, tuningDetails, user);
      ITvCardHandler cardHandler1 = dvbCardMocks.GetMockedCardHandler();
      //ITvCardHandler cardHandler1 = DVBTCardMocks.AddIdleFTADVBTCard(tuningDetail1, user);
      cards.Add(cardHandler1.DataBaseCard.IdCard, cardHandler1);


      ChannelMap channelMap;
      Channel channel = ChannelMocks.GetFTAChannel(out channelMap);
      Isolate.WhenCalled(() => businessLayer.GetTuningChannelByName(channel)).WillReturn(tuningDetails);

      SetupChannelMapping(cardHandler1, channelMap);

      AdvancedCardAllocation allocation = new AdvancedCardAllocation(businessLayer);

      TvResult result;
      List<CardDetail> availCards = allocation.GetAvailableCardsForChannel(cards, channel, ref user, out result);

      Assert.AreEqual(0, availCards.Count, "The number of cards returned is not as expected");
    }
    #endregion

    #endregion

    #region helpers


    //TODO: move these helpers to other static classes

    private void SetupChannelMapping(ITvCardHandler cardHandler1, ChannelMap channelMap)
    {
      Isolate.WhenCalled(() => channelMap.EpgOnly).WillReturn(false);
      Isolate.WhenCalled(() => channelMap.ReferencedCard().DevicePath).WillReturn(cardHandler1.DataBaseCard.DevicePath);      
    }

    private DVBCardMocks GetCardMockByCardType(CardType cardType, List<IChannel> tuningDetails, User user)
    {
      DVBCardMocks cardMocks = null;
      switch (cardType)
      {
        case CardType.DVBt:
          cardMocks = new DVBTCardMocks(tuningDetails, user);
          break;

        case CardType.DVBc:
          cardMocks = new DVBCCardMocks(tuningDetails, user);
          break;

        case CardType.DVBs:
          cardMocks = new DVBSCardMocks(tuningDetails, user);
          break;
      }
      return cardMocks;
    }

    private IChannel GetTuningDetailBasedOnCardType(CardType cardType)
    {
      IChannel tuningDetail = null;
      switch (cardType)
      {
        case CardType.DVBt:
          tuningDetail = Isolate.Fake.Instance<DVBTChannel>();
          break;

        case CardType.DVBc:
          tuningDetail = Isolate.Fake.Instance<DVBCChannel>();
          break;

        case CardType.DVBs:
          tuningDetail = Isolate.Fake.Instance<DVBSChannel>();
          break;
      }
      return tuningDetail;
    }

   
       

    #endregion

    #endregion

  }
}
