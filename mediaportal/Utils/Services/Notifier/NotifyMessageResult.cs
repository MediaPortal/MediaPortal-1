using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Services
{
  public enum NotifyMessageResult
  {
    Unknown,
    OK,
    Failed,
    Pending,
    InvalidMessage,
    NoAction,
    AlreadyExists
  }
}
