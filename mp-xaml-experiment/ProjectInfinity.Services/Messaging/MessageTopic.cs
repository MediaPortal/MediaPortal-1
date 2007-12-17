using System;
using System.Collections.Generic;
using System.Reflection;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Messaging
{
  internal sealed class MessageTopic
  {
    private readonly string id;
    private Type _messageType;
    private readonly List<KeyValuePair<object, MethodInfo>> handlers = new List<KeyValuePair<object, MethodInfo>>();

    private static readonly MethodInfo raiseMethodInfo =
      typeof(MessageTopic).GetMethod("DoRaise", BindingFlags.Instance | BindingFlags.NonPublic);


    public string Id
    {
      get { return id; }
    }

    public Type MessageType
    {
      get { return _messageType; }
      set { _messageType = value; }
    }

    public MessageTopic(string id)
    {
      this.id = id;
    }

    public void AddPublisher(object item, EventInfo info)
    {
      Delegate handler = Delegate.CreateDelegate(info.EventHandlerType, this, raiseMethodInfo);
      info.AddEventHandler(item, handler);
    }

    /// <summary>
    /// This method passes the event that is raised by the publisher through to the subscribers
    /// </summary>
    /// <param name="e"></param>
    internal void DoRaise(Message e)
    {
      ServiceScope.Get<ILogger>().Debug("MessageBroker: sending {0}({1}) message", id, e);
      for (int i = 0; i < handlers.Count; ++i)
      {
        if (i >= handlers.Count) break;
        KeyValuePair<object, MethodInfo> pair = handlers[i];
        pair.Value.Invoke(pair.Key, new object[] { e });
      }
    }

    public void AddSubscriber(object item, MethodInfo info)
    {
      handlers.Add(new KeyValuePair<object, MethodInfo>(item, info));
    }


    public void RemovePublisher(object item, EventInfo info)
    {
      Delegate handler = Delegate.CreateDelegate(info.EventHandlerType, this, raiseMethodInfo);
      info.RemoveEventHandler(item, handler);
    }

    public void RemoveSubscriber(object item, MethodInfo info)
    {
      handlers.Remove(new KeyValuePair<object, MethodInfo>(item, info));
    }
  }
}