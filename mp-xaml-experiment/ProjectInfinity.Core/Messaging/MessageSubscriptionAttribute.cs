using System;

namespace ProjectInfinity.Messaging
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
  public sealed class MessageSubscriptionAttribute : Attribute
  {
    private Type topic;

    public MessageSubscriptionAttribute(Type topic)
    {
      this.topic = topic;
    }

    public string Topic
    {
      get { return topic.FullName; }
    }

    public Type GetTopic()
    {
      return topic;
    }
  }
}