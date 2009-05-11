using System;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.xPL
{
  [Flags]
  public enum XplMessageTypes : byte
  {
    Any = 0xff,
    Command = 1,
    None = 0,
    Status = 2,
    Trigger = 4
  }
}