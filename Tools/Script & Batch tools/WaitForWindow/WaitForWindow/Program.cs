using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Windows;

namespace WaitForWindow
{
  static class Program
  {
    /// <summary>
    /// Der Haupteinstiegspunkt für die Anwendung.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      if (args != null && args.Length != 0)
      {
        cmdArgs tmpArgs = new cmdArgs(args);
        string WaitForWindowName = string.Empty;
        bool WaitForForeGroundWindow = false;
        if (tmpArgs.ArgExists("WindowName")) WaitForWindowName = tmpArgs.Values[tmpArgs.FindArgPos("WindowName")];
        if (tmpArgs.ArgExists("ForeGroundWindowName"))
        {
          WaitForWindowName = tmpArgs.Values[tmpArgs.FindArgPos("ForeGroundWindowName")];
          WaitForForeGroundWindow = true;
        }

        if (WaitForWindowName != string.Empty)
        {
          IntPtr tmpFoundWindowHandle = IntPtr.Zero;
          WindowManagement.GetHandleFromPartialCaption(ref tmpFoundWindowHandle, WaitForWindowName);

          while (tmpFoundWindowHandle == IntPtr.Zero || (WaitForForeGroundWindow && WindowManagement.GetForegroundWindow() != tmpFoundWindowHandle))
          {
            System.Threading.Thread.Sleep(250);
            WindowManagement.GetHandleFromPartialCaption(ref tmpFoundWindowHandle, WaitForWindowName);
          }
        }
      }

      //Application.Run(new Form1());
    }
  }
}