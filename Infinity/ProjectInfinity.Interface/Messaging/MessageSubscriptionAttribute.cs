using System;

namespace ProjectInfinity.Messaging
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MessageSubscriptionAttribute : Attribute
    {
        private string topic;

        public MessageSubscriptionAttribute(string topic)
        {
            this.topic = topic;
        }

        public string Topic
        {
            get { return topic; }
        }
    }
}