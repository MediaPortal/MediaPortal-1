using System.Collections.Generic;

namespace ProjectInfinity.Messaging
{
  internal class MessageTopicCollection
  {
    private Dictionary<string, MessageTopic> topics = new Dictionary<string, MessageTopic>();

    internal MessageTopic this[string id]
    {
      get
      {
        MessageTopic topic;
        if (!topics.ContainsKey(id))
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
  }
}