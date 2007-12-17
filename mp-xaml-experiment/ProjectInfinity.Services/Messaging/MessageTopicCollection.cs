using System.Collections.Generic;

namespace ProjectInfinity.Messaging
{
  internal class MessageTopicCollection
  {
    private readonly Dictionary<string, MessageTopic> topics = new Dictionary<string, MessageTopic>();

    internal MessageTopic this[string id]
    {
      get
      {
        MessageTopic topic;
        if (!Contains(id))
        {
          topic = new MessageTopic(id);
          topics.Add(id, topic);
        }
        else
        {
          topic = topics[id];
        }
        return topic;
      }
    }

    internal bool Contains(string id)
    {
      return topics.ContainsKey(id);
    }
  }
}