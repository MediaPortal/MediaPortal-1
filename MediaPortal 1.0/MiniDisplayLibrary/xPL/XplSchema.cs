using System;
using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.xPL
{
  [StructLayout(LayoutKind.Sequential)]
  public struct XplSchema
  {
    public string msgClass;
    public string msgType;
  }
}

