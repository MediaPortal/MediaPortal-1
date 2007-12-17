using System;

namespace ProjectInfinity.Messaging
{
  [Serializable]
  public delegate void MessageHandler<TMessage>(TMessage e) where TMessage : Message;
}