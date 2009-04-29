#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

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
      Log.Error("MediaPortal: Unhandled exception occured");
      string directory = "log";

      Exception ex;
      if (eventArgs.ExceptionObject is Exception)
      {
        ex = (Exception)eventArgs.ExceptionObject;
        Log.Error(ex);
      }
      else
      {
        ex = new Exception(string.Format(
                @"A crash occured, but no Exception object was found. 
                Type of exception: {0}
                object.ToString {1}", eventArgs.ExceptionObject.GetType(), eventArgs.ExceptionObject.ToString())
                );
        Log.Error(ex);
      }

      ExceptionLogger logger = new ExceptionLogger(ex);
      logger.CreateLogs(directory);
      Log.Info("MediaPortal: stop...");
      // GEMX 08.04.08: The WatchDog is now always started in the background and monitors MP itself
      /*
      Process mpWatchDog = new Process();
      mpWatchDog.StartInfo.ErrorDialog = true;
      mpWatchDog.StartInfo.UseShellExecute = true;
      mpWatchDog.StartInfo.WorkingDirectory = Application.StartupPath;
      mpWatchDog.StartInfo.FileName = "WatchDog.exe";
      mpWatchDog.StartInfo.Arguments = "-crashed";
      mpWatchDog.Start();
       */
      Application.Exit();
    }
  }
}