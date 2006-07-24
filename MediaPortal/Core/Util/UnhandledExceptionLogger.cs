using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using MediaPortal.Support;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;

namespace MediaPortal
{
  public class UnhandledExceptionLogger
  {
    public void LogCrash(object sender, UnhandledExceptionEventArgs eventArgs)
    {

      ServiceProvider services = GlobalServiceProvider.Instance;
      ILog log = services.Get<ILog>();

      log.Error("MediaPortal: Unhandled exception occured");
      string directory = "log";

      Exception ex;
      if (eventArgs.ExceptionObject is Exception)
      {
        ex = (Exception)eventArgs.ExceptionObject;
        log.Error(ex);
      }
      else
      {
        ex = new Exception(string.Format(
                @"A crash occured, but no Exception object was found. 
                Type of exception: {0}
                object.ToString {1}", eventArgs.ExceptionObject.GetType(), eventArgs.ExceptionObject.ToString())
                );
        log.Error(ex);
      }

      ExceptionLogger logger = new ExceptionLogger(ex);
      logger.CreateLogs(directory);

      log.Info("MediaPortal: stop...");
      //Process.Start("crash.exe");
      Application.Exit();
    }
  }
}