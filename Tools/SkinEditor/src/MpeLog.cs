#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

namespace Mpe
{
  public enum MpeLogLevel
  {
    Debug = 0,
    Info = 1,
    Warn = 2,
    Error = 3
  } ;


  public class MpeLog
  {
    private static MpeLogLevel level;

    public static MpeLogLevel Threshold
    {
      get { return level; }
      set { level = value; }
    }

    private static void Message(MpeLogLevel level, string message, int progress)
    {
      if (MediaPortalEditor.Global != null && MediaPortalEditor.Global.StatusBar != null)
      {
        MediaPortalEditor.Global.StatusBar.Message(level, message, progress);
      }
      Console.WriteLine(level.ToString() + ": " + message);
    }

    private static void Message(MpeLogLevel level, string message)
    {
      if (MediaPortalEditor.Global != null && MediaPortalEditor.Global.StatusBar != null)
      {
        MediaPortalEditor.Global.StatusBar.Message(level, message);
      }
      Console.WriteLine(level.ToString() + ": " + message);
    }

    private static void Message(MpeLogLevel level, Exception e)
    {
      string m = "";
      if (e != null)
      {
        m += "[" + e.GetType().ToString() + "] ";
        if (e.Message != null && e.Message.Length > 0)
        {
          m += e.Message;
          m += Environment.NewLine;
          m += e.StackTrace;
        }
        if (MediaPortalEditor.Global != null && MediaPortalEditor.Global.StatusBar != null)
        {
          MediaPortalEditor.Global.StatusBar.Message(level, e);
        }
        Console.WriteLine(level.ToString() + ": " + m);
      }
    }

    public static void Debug(string message, int progress)
    {
      if (Threshold <= MpeLogLevel.Debug)
      {
        Message(MpeLogLevel.Debug, message, progress);
      }
    }

    public static void Debug(string message)
    {
      if (Threshold <= MpeLogLevel.Debug)
      {
        Message(MpeLogLevel.Debug, message);
      }
    }

    public static void Debug(Exception exception)
    {
      if (Threshold <= MpeLogLevel.Debug)
      {
        Message(MpeLogLevel.Debug, exception);
      }
    }

    public static void Info(string message, int progress)
    {
      if (Threshold <= MpeLogLevel.Info)
      {
        Message(MpeLogLevel.Info, message, progress);
      }
    }

    public static void Info(string message)
    {
      if (Threshold <= MpeLogLevel.Info)
      {
        Message(MpeLogLevel.Info, message);
      }
    }

    public static void Info(Exception exception)
    {
      if (Threshold <= MpeLogLevel.Info)
      {
        Message(MpeLogLevel.Info, exception);
      }
    }

    public static void Warn(string message, int progress)
    {
      if (Threshold <= MpeLogLevel.Warn)
      {
        Message(MpeLogLevel.Warn, message, progress);
      }
    }

    public static void Warn(string message)
    {
      if (Threshold <= MpeLogLevel.Warn)
      {
        Message(MpeLogLevel.Warn, message);
      }
    }

    public static void Warn(Exception exception)
    {
      if (Threshold <= MpeLogLevel.Warn)
      {
        Message(MpeLogLevel.Warn, exception);
      }
    }

    public static void Error(string message, int progress)
    {
      if (Threshold <= MpeLogLevel.Error)
      {
        Message(MpeLogLevel.Error, message, progress);
      }
    }

    public static void Error(string message)
    {
      if (Threshold <= MpeLogLevel.Error)
      {
        Message(MpeLogLevel.Error, message);
      }
    }

    public static void Error(Exception exception)
    {
      if (Threshold <= MpeLogLevel.Error)
      {
        Message(MpeLogLevel.Error, exception);
      }
    }

    public static void Progress(int min, int max, int progress)
    {
      Debug("Progress(min=" + min + ", max=" + max + ", val=" + progress + ")");
      if (MediaPortalEditor.Global != null && MediaPortalEditor.Global.StatusBar != null)
      {
        MediaPortalEditor.Global.StatusBar.Progress(min, max, progress);
      }
    }

    public static void Progress(int min, int max)
    {
      Debug("Progress(min=" + min + ", max=" + max + ")");
      if (MediaPortalEditor.Global != null && MediaPortalEditor.Global.StatusBar != null)
      {
        MediaPortalEditor.Global.StatusBar.Progress(min, max);
      }
    }

    public static void Progress(int progress)
    {
      Debug("Progress(" + progress + ")");
      if (MediaPortalEditor.Global != null && MediaPortalEditor.Global.StatusBar != null)
      {
        MediaPortalEditor.Global.StatusBar.Progress(progress);
      }
    }
  }
}