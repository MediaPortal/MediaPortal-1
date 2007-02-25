using System;
using System.Reflection;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Messaging
{
  public class MessageBroker : IMessageBroker
  {
    private MessageTopicCollection topics = new MessageTopicCollection();

    /// <summary>
    /// Registers an object to the message broker.
    /// </summary>
    /// <param name="item"></param>
    /// <remarks>The message broker will inspect the object to find out what messages it publishes or wants
    /// to subscribe to.  To find published messages it searches for events that are marked with the 
    /// <see cref="MessagePublicationAttribute"/> attribute.  To find subscriptions it searches for methods that are 
    /// marked with the <see cref="MessageSubscriptionAttribute"/> attribute.</remarks>
    public void Register(object item)
    {
      Type type = item.GetType();
      ServiceScope.Get<ILogger>().Debug("MessageBroker: Registering object {0}", type);
      ProcessPublications(item, type, true);
      ProcessSubscriptions(item, type, true);
    }

    /// <summary>
    /// Unregisters an object from the message broker.
    /// </summary>
    /// <param name="item"></param>
    public void Unregister(object item)
    {
      Type type = item.GetType();
      ServiceScope.Get<ILogger>().Debug("MessageBroker: Removing registration for object {0}", type);
      ProcessPublications(item, type, false);
      ProcessSubscriptions(item, type, false);
    }

    /// <summary>
    /// Deletes all registered objects and resets the <see cref="MessageBroker"/> to its
    /// initial state.
    /// </summary>
    /// <remarks>
    /// This method is typically used in unit tests.
    /// </remarks>
    public void Reset()
    {
      ServiceScope.Get<ILogger>().Debug("MessageBroker: Deleting all registered subscribers and publishers");
      topics = new MessageTopicCollection();
    }

    private void ProcessSubscriptions(object item, Type type, bool register)
    {
      foreach (MethodInfo info in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
      {
        MessageSubscriptionAttribute[] attributes = (MessageSubscriptionAttribute[])info.GetCustomAttributes(typeof(MessageSubscriptionAttribute), true);
        foreach (MessageSubscriptionAttribute attr in attributes)
        {
          HandleSubscriber(item, register, info, attr);
        }
      }
    }

    private void ProcessPublications(object item, Type type, bool register)
    {
      //Process MessagePublication attributes on the type
      foreach (EventInfo info in type.GetEvents())
      {
        MessagePublicationAttribute[] attributes = (MessagePublicationAttribute[])info.GetCustomAttributes(typeof(MessagePublicationAttribute), true);

        foreach (MessagePublicationAttribute attr in attributes)
        {
          HandlePublisher(item, register, info, attr);
        }
      }
      //Process MessagePublication attributes on all interfaces implemented by the type
      foreach (Type interfaceType in type.GetInterfaces())
      {
        foreach (EventInfo info in interfaceType.GetEvents())
        {
          MessagePublicationAttribute[] attributes = (MessagePublicationAttribute[])info.GetCustomAttributes(typeof(MessagePublicationAttribute), true);

          foreach (MessagePublicationAttribute attr in attributes)
          {
            HandlePublisher(item, register, info, attr);
          }
        }
      }
    }

    private void HandlePublisher(object item, bool register, EventInfo info, MessagePublicationAttribute attr)
    {
      MessageTopic topic = topics[attr.Topic];
      if (register)
      {
        topic.AddPublisher(item, info);
      }
      else
      {
        topic.RemovePublisher(item, info);
      }
    }

    private void HandleSubscriber(object item, bool register, MethodInfo info, MessageSubscriptionAttribute attr)
    {
      MessageTopic topic = topics[attr.Topic];
      if (register)
      {
        topic.AddSubscriber(item, info);
      }
      else
      {
        topic.RemoveSubscriber(item, info);
      }
    }
  }
}