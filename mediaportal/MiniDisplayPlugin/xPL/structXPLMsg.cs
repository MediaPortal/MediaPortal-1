using System;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.xPL
{
  [StructLayout(LayoutKind.Sequential)]
  public struct structXPLMsg
  {
    public string keyName;
    public string Value;
    public structXPLMsg(string newKeyName, string newKeyValue)
    {
      try
      {
        this.keyName = newKeyName;
        this.Value = newKeyValue;
      } catch
      {
        this.keyName = string.Empty;
        this.Value = string.Empty;
        Log.Info("xPL.structXplSection(structXPLMsg): caught exception", new object[0]);
      }
    }
  }
}

