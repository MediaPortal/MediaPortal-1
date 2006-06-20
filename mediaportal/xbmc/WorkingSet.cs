using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Text;

namespace MediaPortal
{
  class WorkingSet
  {
    [DllImport("kernel32")]
    static extern bool SetProcessWorkingSetSize(IntPtr handle, int minSize, int maxSize);

    static public void Minimize()
    {
      SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }
  }
}
