using System;
using ProjectInfinity.Messaging;

namespace ProjectInfinity.Tests.Messaging.Mocks
{
    public class Publisher
    {
        [MessagePublication("pimsg://Test/publish")]
        public event EventHandler<MessageEventArgs<string>> Publish;

        public void DoPublish()
        {
            if (Publish != null)
            {
                Publish(this, new MessageEventArgs<string>("Hello"));
            }
        }
    }
}