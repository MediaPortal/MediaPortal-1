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
      Log.WriteFile(Log.LogType.Error,true,"Mediaportal:unhandled exception occured");
      string directory = "log";

      Exception ex;
      if (eventArgs.ExceptionObject is Exception)
      {
        ex = (Exception)eventArgs.ExceptionObject;
        Log.Write(ex);
      }
      else
      {
        ex = new Exception(string.Format(
                @"A crash occured, but no Exception object was found. 
                Type of exception: {0}
                object.ToString {1}", eventArgs.ExceptionObject.GetType(), eventArgs.ExceptionObject.ToString())
                );
        Log.Write(ex);
      }

      ExceptionLogger logger = new ExceptionLogger(ex);
      logger.CreateLogs(directory);

      Log.Write("Mediaportal: stop...");
      //Process.Start("crash.exe");
      Application.Exit();
    }
  }
}