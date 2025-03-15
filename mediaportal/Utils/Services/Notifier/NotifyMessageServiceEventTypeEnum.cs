using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Services
{
  public enum NotifyMessageServiceEventTypeEnum
  {
    None,
    MessageRegistered,
    MessageUnregistered,
    MessageStatusChanged,
    MessageClicked,
    MessageDialogModeCleared
  }
}
