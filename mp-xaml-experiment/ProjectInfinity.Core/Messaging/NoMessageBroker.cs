namespace ProjectInfinity.Messaging
{
  /// <summary>
  /// Dummu <see cref="IMessageBroker"/> implementation that does absolutely nothing.
  /// </summary>
  /// <remarks>
  /// This class is only used in Unit tests and in design mode (inside M$ Blend)
  /// </remarks>
  internal class NoMessageBroker : IMessageBroker
  {
    #region IMessageBroker Members

    /// <summary>
    /// Registers an object to the message broker.
    /// </summary>
    /// <param name="item"></param>
    /// <remarks>The message broker will inspect the object to find out what messages it publishes or wants
    /// to subscribe to.  To find published messages it searches for events that are marked with the 
    /// <see cref="MessagePublicationAttribute"/> attribute.  To find subscriptions it searches for methods that are 
    /// marked with the <see cref="MessageSubscriptionAttribute"/> attribute.</remarks>
    public void Register(object item)
    {}

    /// <summary>
    /// Unregisters an object from the message broker.
    /// </summary>
    /// <param name="item"></param>
    public void Unregister(object item)
    {}

    /// <summary>
    /// Triggers the given message in the system.
    /// </summary>
    /// <param name="message">The message to trigger.</param>
    /// <param name="args">The arguments to pass to the message constructor</param>
    public void Send(string message, params object[] args)
    {}

    #endregion
  }
}