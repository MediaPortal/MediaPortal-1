namespace ProjectInfinity.Messaging
{
  /// <summary>
  /// The interface for all message broker implementations.
  /// </summary>
  public interface IMessageBroker
  {
    /// <summary>
    /// Registers an object to the message broker.
    /// </summary>
    /// <param name="item"></param>
    /// <remarks>The message broker will inspect the object to find out what messages it publishes or wants
    /// to subscribe to.  To find published messages it searches for events that are marked with the 
    /// <see cref="MessagePublicationAttribute"/> attribute.  To find subscriptions it searches for methods that are 
    /// marked with the <see cref="MessageSubscriptionAttribute"/> attribute.</remarks>
    void Register(object item);

    /// <summary>
    /// Unregisters an object from the message broker.
    /// </summary>
    /// <param name="item"></param>
    void Unregister(object item);

    /// <summary>
    /// Triggers the given message in the system.
    /// </summary>
    /// <param name="message">The message to trigger.</param>
    /// <param name="args">The arguments to pass to the message constructor</param>
    void Send(string message, params object[] args);
  }
}