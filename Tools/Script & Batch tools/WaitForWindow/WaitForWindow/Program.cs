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
        int WaitTimeOut = -1;
        DateTime startTime = DateTime.Now;

        if (tmpArgs.ArgExists("WindowName")) WaitForWindowName = tmpArgs.Values[tmpArgs.FindArgPos("WindowName")];
        if (tmpArgs.ArgExists("ForeGroundWindowName"))
        {
          WaitForWindowName = tmpArgs.Values[tmpArgs.FindArgPos("ForeGroundWindowName")];
          WaitForForeGroundWindow = true;
        }

        if (tmpArgs.ArgExists("WaitTimeOut"))
        {
          int.TryParse(tmpArgs.Values[tmpArgs.FindArgPos("WaitTimeOut")], out WaitTimeOut);
        }
        
        if (WaitForWindowName != string.Empty)
        {
          IntPtr tmpFoundWindowHandle = IntPtr.Zero;
          WindowManagement.GetHandleFromPartialCaption(ref tmpFoundWindowHandle, WaitForWindowName);

          while (tmpFoundWindowHandle == IntPtr.Zero || (WaitForForeGroundWindow && WindowManagement.GetForegroundWindow() != tmpFoundWindowHandle))
          {
            System.Threading.Thread.Sleep(250);
            WindowManagement.GetHandleFromPartialCaption(ref tmpFoundWindowHandle, WaitForWindowName);

            if (WaitTimeOut != -1)
            {
              TimeSpan tmpSpan = DateTime.Now - startTime;
              if (WaitTimeOut <= tmpSpan.TotalMilliseconds)
              {
                System.Environment.Exit(1);
              }
            }
          }
        }
      }

      System.Environment.Exit(0);
    }
  }
}