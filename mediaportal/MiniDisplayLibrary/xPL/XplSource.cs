using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.xPL
{
  [StructLayout(LayoutKind.Sequential)]
  public struct XplSource
  {
    public string Vendor;
    public string Device;
    public string Instance;
  }
}