using System;

namespace ProjectInfinity.Messaging
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = true)]
    public sealed class MessagePublicationAttribute : Attribute
    {
        private string topic;

        public MessagePublicationAttribute(string topic)
        {
            this.topic = topic;
        }

        public string Topic
        {
            get { return topic; }
        }
    }
}