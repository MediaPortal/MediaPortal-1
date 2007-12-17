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
    /// Triggers the given message in the system.
    /// </summary>
    /// <param name="messageId">The message to trigger.</param>
    /// <param name="args">The arguments to pass to the message constructor</param>
    public void Send(string messageId, params object[] args)
    {
      ILogger logger = ServiceScope.Get<ILogger>();
      logger.Debug("MessageBroker: Manual triggering of {0} message", messageId);
      //Check if we know the message type
      if (!topics.Contains(messageId))
      {
        logger.Warn("MessageBroker: message {0} is not (yet) registered");
        return;
      }
      MessageTopic messageTopic = topics[messageId];
      //Try to get the Type of the message
      if (messageTopic.MessageType==null)
      {
        logger.Warn("MessageBroker: could not get type for message {0}",messageId);
        return;
      }
      Message message = null;
      //Try to create an instance of the message (passing the arguments)
      //try
      //{
      message = Activator.CreateInstance(messageTopic.MessageType, args) as Message;
      //}
      //catch(Exception ex)
      //{
      //  logger.Error(ex);
      //}
      //Actually trigger the message
      //try
      //{
        messageTopic.DoRaise(message);
      //}
      //catch(Exception ex)
      //{
      //  logger.Error(ex);
      //}
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
        MessageSubscriptionAttribute[] attributes =
          (MessageSubscriptionAttribute[]) info.GetCustomAttributes(typeof (MessageSubscriptionAttribute), true);
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
        MessagePublicationAttribute[] attributes =
          (MessagePublicationAttribute[]) info.GetCustomAttributes(typeof (MessagePublicationAttribute), true);

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
          MessagePublicationAttribute[] attributes =
            (MessagePublicationAttribute[]) info.GetCustomAttributes(typeof (MessagePublicationAttribute), true);

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
        if (topic.MessageType == null)
          topic.MessageType = attr.GetTopic();
        Type eventHandlerType = info.EventHandlerType;
        //TODO: check event type
        //if (!messageType.IsAssignableFrom(eventHandlerType))
        //  throw new ArgumentException(
        //    string.Format("The arguments with type {0} of event {1} are incompatible with message type {2}",
        //                  eventHandlerType, info.DeclaringType.FullName, attr.GetTopic().FullName));

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
        if (topic.MessageType == null)
          topic.MessageType = attr.GetTopic();
        ParameterInfo[] parameters = info.GetParameters();
        if (parameters.Length != 1)
        {
          throw new ArgumentException(
            string.Format("Incorrect number of parameters for message handler method {0}", info.Name));
        }
        ParameterInfo parameter = parameters[0];
        if (!parameter.ParameterType.IsAssignableFrom(attr.GetTopic()))
        {
          throw new ArgumentException(
            string.Format("Parameter {0} with type {1} of method {2} is not compatible with type {3} from message",
                          parameter.Name, parameter.ParameterType, info.DeclaringType.FullName + "." + info.Name,
                          attr.GetTopic().FullName
              ));
        }
        topic.AddSubscriber(item, info);
      }
      else
      {
        topic.RemoveSubscriber(item, info);
      }
    }
  }
}