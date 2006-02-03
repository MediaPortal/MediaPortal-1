using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using MediaPortal.Support;
using MediaPortal.GUI.Library;

namespace MediaPortal
{
  public class UnhandledExceptionLogger
  {
    public void LogCrash(object sender, UnhandledExceptionEventArgs eventArgs)
    {
      string directory = "log";

      Exception ex;
      if (eventArgs.ExceptionObject is Exception)
      {
        ex = (Exception)eventArgs.ExceptionObject;
      }
      else
      {
        ex = new Exception(string.Format(
@"A crash occured, but no Exception object was found. 
Type of exception: {0}
object.ToString {1}" , eventArgs.ExceptionObject.GetType(), eventArgs.ExceptionObject.ToString())
);
      }

      ExceptionLogger logger = new ExceptionLogger(ex);
      logger.CreateLogs(directory);

      //Process.Start("crash.exe");
      Application.Exit();
    }
  }
}