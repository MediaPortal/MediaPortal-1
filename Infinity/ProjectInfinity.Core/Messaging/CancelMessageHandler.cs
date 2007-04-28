using System;

namespace ProjectInfinity.Messaging
{
  [Serializable]
  public delegate void CancelMessageHandler<TMessage>(TMessage e) where TMessage : CancelMessage;
}