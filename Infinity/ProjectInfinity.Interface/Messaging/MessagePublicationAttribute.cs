using System;

namespace ProjectInfinity.Messaging
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = true)]
    public sealed class MessagePublicationAttribute : Attribute
    {
        private string topic;

        public MessagePublicationAttribute(Type topic)
        {
            this.topic = topic.FullName;
        }

        public string Topic
        {
            get { return topic; }
        }
    }
}