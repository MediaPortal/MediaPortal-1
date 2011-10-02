using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TvService;
using TypeMock.ArrangeActAssert;

namespace TVServiceTests
{
  [TestFixture]
  [Isolated]
  public class CardDetailTest
  {

    [Isolated]
    [Test]
    public void SortTest ()
    {            
      IChannel fakeIChannel = Isolate.Fake.Instance<IChannel>();

      Card fakeCard1 =  Isolate.Fake.Instance<Card>();
      Isolate.WhenCalled(() => fakeCard1.Priority).WillReturn(3);

      Card fakeCard2 = Isolate.Fake.Instance<Card>();
      Isolate.WhenCalled(() => fakeCard2.Priority).WillReturn(2);

      Card fakeCard3 = Isolate.Fake.Instance<Card>();
      Isolate.WhenCalled(() => fakeCard3.Priority).WillReturn(1);

      var cardDetails = new List<CardDetail>();
      var card = new CardDetail(1, fakeCard1, fakeIChannel, false, 0);
      cardDetails.Add(card);
      card = new CardDetail(2, fakeCard2, fakeIChannel, false, 0);
      cardDetails.Add(card);
      card = new CardDetail(3, fakeCard3, fakeIChannel, true, 1);
      cardDetails.Add(card);

      cardDetails.Sort();

      List<CardDetail> freeCards =
          cardDetails.Where(t => t.NumberOfOtherUsers == 0 || (t.NumberOfOtherUsers > 0 && t.SameTransponder)).ToList();
      List<CardDetail> availCards = cardDetails.Where(t => t.NumberOfOtherUsers > 0 && !t.SameTransponder).ToList();
    }

  }
}
