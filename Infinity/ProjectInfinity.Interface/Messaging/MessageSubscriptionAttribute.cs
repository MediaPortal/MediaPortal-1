using System;

namespace ProjectInfinity.Messaging
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MessageSubscriptionAttribute : Attribute
    {
        private string topic;

        public MessageSubscriptionAttribute(Type topic)
        {
            this.topic = topic.FullName;
        }

        public string Topic
        {
            get { return topic; }
        }
    }
}