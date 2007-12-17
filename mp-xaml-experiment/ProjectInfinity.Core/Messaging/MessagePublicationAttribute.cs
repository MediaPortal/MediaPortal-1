using System;

namespace ProjectInfinity.Messaging
{
  [AttributeUsage(AttributeTargets.Event, AllowMultiple = true)]
  public sealed class MessagePublicationAttribute : Attribute
  {
    private Type topic;

    public MessagePublicationAttribute(Type topic)
    {
      if (!typeof (Message).IsAssignableFrom(topic))
      {
        throw new ArgumentException();
      }
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