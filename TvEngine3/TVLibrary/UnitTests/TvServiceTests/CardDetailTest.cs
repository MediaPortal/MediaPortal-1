using TvLibrary.Interfaces;
using TypeMock.ArrangeActAssert;
using TvDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TvService;

namespace TVServiceTests
{
  [TestFixture]
  [Isolated]
  public class CardDetailTest
  {
    [TestFixtureSetUp]
    public void FixtureSetUp()
    {

    }

    [TestFixtureTearDown]
    public void FixtureTearDown()
    {

    }

    [SetUp]
    public void SetUp()
    {
      
    }

    [TearDown]
    public void TearDown()
    {

    }
    [Test]
    public void TestPrioritySorting()
    {
      IChannel fakeIChannel = Isolate.Fake.Instance<IChannel>();
      Card fakeCard = Isolate.Fake.Instance<Card>();
      CardDetail c1 = new CardDetail(1, CreateCardMock(3), fakeIChannel, true, 0);
      CardDetail c2 = new CardDetail(2, CreateCardMock(2), fakeIChannel, true, 0);
      CardDetail c3 = new CardDetail(3, CreateCardMock(1), fakeIChannel, true, 0);
      Assert.AreEqual(0, c1.CompareTo(c1));
      Assert.AreEqual(-1, c1.CompareTo(c2));
      Assert.AreEqual(-1, c1.CompareTo(c3));

      Assert.AreEqual(1, c2.CompareTo(c1));
      Assert.AreEqual(0, c2.CompareTo(c2));
      Assert.AreEqual(-1, c2.CompareTo(c3));

      Assert.AreEqual(1, c3.CompareTo(c1));
      Assert.AreEqual(1, c3.CompareTo(c2));
      Assert.AreEqual(0, c3.CompareTo(c3));
    }

    [Test]
    public void TestTransponderSorting()
    {
      IChannel fakeIChannel = Isolate.Fake.Instance<IChannel>();
      Card fakeCard = Isolate.Fake.Instance<Card>();
      CardDetail c1 = new CardDetail(1, CreateCardMock(1), fakeIChannel, true, 0);
      CardDetail c2 = new CardDetail(2, CreateCardMock(1), fakeIChannel, false, 0);
      Assert.AreEqual(0, c1.CompareTo(c1));
      Assert.AreEqual(-1, c1.CompareTo(c2));
      Assert.AreEqual(1, c2.CompareTo(c1));
      Assert.AreEqual(0, c2.CompareTo(c2));
    }

    [Test]
    public void TestNumberOfUsersSortingWhenNotSameTransponderSorting()
    {
      IChannel fakeIChannel = Isolate.Fake.Instance<IChannel>();
      Card fakeCard = Isolate.Fake.Instance<Card>();
      CardDetail c1 = new CardDetail(1, CreateCardMock(1), fakeIChannel, false, 0);
      CardDetail c2 = new CardDetail(2, CreateCardMock(1), fakeIChannel, false, 1);
      CardDetail c3 = new CardDetail(2, CreateCardMock(1), fakeIChannel, false, 2);

      Assert.AreEqual(0, c1.CompareTo(c1));
      Assert.AreEqual(-1, c1.CompareTo(c2));
      Assert.AreEqual(-1, c1.CompareTo(c3));

      Assert.AreEqual(1, c2.CompareTo(c1));
      Assert.AreEqual(0, c2.CompareTo(c2));
      Assert.AreEqual(-1, c2.CompareTo(c3));

      Assert.AreEqual(1, c3.CompareTo(c1));
      Assert.AreEqual(1, c3.CompareTo(c2));
      Assert.AreEqual(0, c3.CompareTo(c3));
    }

    [Test]
    public void TestSortingOrder()
    {
      //TODO: Test the combinations between all tests.
    }

    private static Card CreateCardMock(int priority)
    {
      Card fakeCard = Isolate.Fake.Instance<Card>();
      Isolate.WhenCalled(() => fakeCard.Priority).WillReturn(priority);
      return fakeCard;
    }
   
  }
}
