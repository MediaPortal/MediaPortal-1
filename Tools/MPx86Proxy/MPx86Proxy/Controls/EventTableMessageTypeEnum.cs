using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPx86Proxy.Controls
{
    [Flags]
    public enum EventTableMessageTypeEnum
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 4,
        System = 256,
        All = Info | Warning | Error | System
    }
}
