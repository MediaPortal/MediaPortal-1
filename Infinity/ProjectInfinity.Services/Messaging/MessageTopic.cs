using System;
using System.Collections.Generic;
using System.Reflection;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Messaging
{
  internal sealed class MessageTopic
  {
    private string id;
    private List<KeyValuePair<object, MethodInfo>> handlers = new List<KeyValuePair<object, MethodInfo>>();

    private static MethodInfo raiseMethodInfo = typeof(MessageTopic).GetMethod("DoRaise", BindingFlags.Instance | BindingFlags.NonPublic);

    public string Id
    {
      get { return id; }
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
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <remarks>
    /// DO NOT DELETE THIS METHOD!!!
    /// Resharper marks this method as "never used" because it is private and it is only called through
    /// reflection.
    /// </remarks>
    private void DoRaise(object sender, EventArgs e)
    {
      ServiceScope.Get<ILogger>().Debug("MessageBroker: sending {0}({1}) message", id, e);
      foreach (KeyValuePair<object, MethodInfo> pair in handlers)
      {
        pair.Value.Invoke(pair.Key, new object[] { sender, e });
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