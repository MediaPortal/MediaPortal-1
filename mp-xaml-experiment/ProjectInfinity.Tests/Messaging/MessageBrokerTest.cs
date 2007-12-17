using System;
using NUnit.Framework;
using ProjectInfinity.Logging;
using ProjectInfinity.Messaging;
using ProjectInfinity.Tests.Messaging.Mocks;

namespace ProjectInfinity.Tests.Messaging
{
  /// <summary>
  /// <see cref="MessageBroker"/> tests.
  /// </summary>
  [TestFixture]
  public class MessageBrokerTest
  {
    [TestFixtureSetUp]
    public void Setup()
    {
      new ServiceScope();
      ServiceScope.Add<ILogger>(new NoLogger());
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      ServiceScope.Current.Dispose();
    }

    /// <summary>
    /// Checks whether a subscriber actually receives the message it subscribes to.
    /// </summary>
    [Test]
    public void TestMessageRegistration()
    {
      MessageBroker messageBroker = new MessageBroker();
      Publisher publisher = new Publisher();
      Subscriber subscriber = new Subscriber();

      messageBroker.Register(publisher);
      messageBroker.Register(subscriber);
      publisher.DoPublish();
      Assert.IsTrue(subscriber.Received);
    }

    /// <summary>
    /// Checks whether mutlitple subscribers all receive the message.
    /// </summary>
    [Test]
    public void TestMultipleReceivers()
    {
      MessageBroker messageBroker = new MessageBroker();
      Publisher publisher = new Publisher();
      Subscriber subscriber1 = new Subscriber();
      Subscriber subscriber2 = new Subscriber();

      messageBroker.Register(publisher);
      messageBroker.Register(subscriber1);
      messageBroker.Register(subscriber2);
      publisher.DoPublish();
      Assert.IsTrue(subscriber1.Received);
      Assert.IsTrue(subscriber2.Received);
    }

    /// <summary>
    /// Checks whether the subscriber stops receiving the message when he unregisters.
    /// </summary>
    [Test]
    public void TestUnRegisterSubscriber()
    {
      MessageBroker messageBroker = new MessageBroker();
      Publisher publisher = new Publisher();
      Subscriber subscriber = new Subscriber();

      messageBroker.Register(publisher);
      messageBroker.Register(subscriber);
      publisher.DoPublish();
      Assert.IsTrue(subscriber.Received);
      subscriber.Reset();
      Assert.IsFalse(subscriber.Received);
      messageBroker.Unregister(subscriber);
      publisher.DoPublish();
      Assert.IsFalse(subscriber.Received);
    }

    /// <summary>
    /// Checks whether the subscriber stops receiving the message when the publisher unregisters.
    /// </summary>
    [Test]
    public void TestUnRegisterPublisher()
    {
      MessageBroker messageBroker = new MessageBroker();
      Publisher publisher = new Publisher();
      Subscriber subscriber = new Subscriber();

      messageBroker.Register(publisher);
      messageBroker.Register(subscriber);
      publisher.DoPublish();
      Assert.IsTrue(subscriber.Received);
      subscriber.Reset();
      Assert.IsFalse(subscriber.Received);
      messageBroker.Unregister(publisher);
      publisher.DoPublish();
      Assert.IsFalse(subscriber.Received);
    }

    /// <summary>
    /// Checks whether a subscriber is still able to receive the message if 
    /// it registers before the publisher.
    /// </summary>
    [Test]
    public void TestSubsciberIsFirst()
    {
      MessageBroker messageBroker = new MessageBroker();
      Publisher publisher = new Publisher();
      Subscriber subscriber = new Subscriber();

      messageBroker.Register(subscriber);
      messageBroker.Register(publisher);
      publisher.DoPublish();
      Assert.IsTrue(subscriber.Received);
    }


    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void TestInvalidReceiverMethod()
    {
      MessageBroker messageBroker = new MessageBroker();
      InvalidReceiver invalidReceiver = new InvalidReceiver();
      messageBroker.Register(invalidReceiver);
    }

    [Test]
    [ExpectedException(typeof (ArgumentException))]
    public void TestInvalidPublisherEvent()
    {
      MessageBroker messageBroker = new MessageBroker();
      InvalidPublisher invalidPublisher = new InvalidPublisher();
      messageBroker.Register(invalidPublisher);
    }

    /// <summary>
    /// Tests manual triggering of messages.
    /// </summary>
    [Test]
    public void TestManualMessage()
    {
      MessageBroker messageBroker = new MessageBroker();
      Subscriber subscriber = new Subscriber();

      messageBroker.Register(subscriber);
      messageBroker.Send("ProjectInfinity.Tests.Messaging.Mocks.MockMessage","hello");
      Assert.IsTrue(subscriber.Received);
    }

  }
}